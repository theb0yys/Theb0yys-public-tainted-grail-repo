using System;
using System.Linq;
using System.Reflection;
using Awaken.TG.MVC.UI;
using Awaken.TG.Main.Heroes;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace FoAModManager.Patches;

internal static class GameInputPatch
{
    private static readonly string[] AxisMethodNames =
    {
        "GetAxis",
        "GetAxisRaw",
        "GetAxisPrev",
        "GetAxisRawPrev"
    };

    private static readonly string[] ButtonMethodNames =
    {
        "GetButton",
        "GetButtonDown",
        "GetButtonUp",
        "GetButtonPrev",
        "GetButtonDoublePressDown",
        "GetButtonDoublePressHold",
        "GetButtonSinglePressDown",
        "GetButtonSinglePressHold",
        "GetButtonTimedPress",
        "GetButtonTimedPressDown",
        "GetButtonTimedPressUp",
        "GetAnyButton",
        "GetAnyButtonDown",
        "GetAnyButtonUp",
        "GetAnyButtonPrev"
    };

    private static readonly FieldInfo? MoveInputField = AccessTools.Field(typeof(PlayerInput), "<MoveInput>k__BackingField");
    private static readonly FieldInfo? MountMoveInputField = AccessTools.Field(typeof(PlayerInput), "<MountMoveInput>k__BackingField");
    private static readonly FieldInfo? LookInputField = AccessTools.Field(typeof(PlayerInput), "<LookInput>k__BackingField");

    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        PatchGameMousePosition(harmony, logger);
        PatchPlayerInput(harmony, logger);
        PatchRewiredAxes(harmony, logger);
        PatchRewiredButtons(harmony, logger);
        PatchUnityKeyCodes(harmony, logger);
    }

    private static void PatchGameMousePosition(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? target = AccessTools.Method(typeof(GameUI), "UpdateMousePosition");
        MethodInfo? prefix = AccessTools.Method(typeof(GameInputPatch), nameof(BlockWhenManagerVisiblePrefix));

        if (target == null || prefix == null)
        {
            logger.LogWarning("Could not patch GameUI.UpdateMousePosition. FoA Mod Manager may not fully freeze menu hover input while open.");
            return;
        }

        harmony.Patch(target, prefix: new HarmonyMethod(prefix));
        logger.LogInfo("Patched GameUI.UpdateMousePosition for FoA Mod Manager input freeze.");
    }

    private static void PatchPlayerInput(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? target = AccessTools.Method(typeof(PlayerInput), "ProcessLateUpdate");
        MethodInfo? prefix = AccessTools.Method(typeof(GameInputPatch), nameof(PlayerInputProcessLateUpdatePrefix));

        if (target == null || prefix == null)
        {
            logger.LogWarning("Could not patch PlayerInput.ProcessLateUpdate. FoA Mod Manager may not fully freeze player camera input while open.");
            return;
        }

        harmony.Patch(target, prefix: new HarmonyMethod(prefix));
        logger.LogInfo("Patched PlayerInput.ProcessLateUpdate for FoA Mod Manager input freeze.");
    }

    private static void PatchRewiredAxes(Harmony harmony, ManualLogSource logger)
    {
        Type? rewiredPlayerType = AccessTools.TypeByName("Rewired.Player");
        MethodInfo? prefix = AccessTools.Method(typeof(GameInputPatch), nameof(AxisPrefix));

        if (rewiredPlayerType == null || prefix == null)
        {
            logger.LogWarning("Could not find Rewired.Player axis methods. FoA Mod Manager may not fully freeze camera axes while open.");
            return;
        }

        int patched = 0;
        foreach (MethodInfo method in rewiredPlayerType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!AxisMethodNames.Contains(method.Name, StringComparer.Ordinal))
            {
                continue;
            }

            ParameterInfo[] parameters = method.GetParameters();
            if (method.ReturnType != typeof(float) ||
                parameters.Length != 1 ||
                (parameters[0].ParameterType != typeof(string) && parameters[0].ParameterType != typeof(int)))
            {
                continue;
            }

            harmony.Patch(method, prefix: new HarmonyMethod(prefix));
            patched++;
        }

        if (patched == 0)
        {
            logger.LogWarning("No Rewired.Player axis overloads were patched. FoA Mod Manager may not fully freeze camera axes while open.");
            return;
        }

        logger.LogInfo($"Patched {patched} Rewired.Player axis overloads for FoA Mod Manager input freeze.");
    }

    private static void PatchRewiredButtons(Harmony harmony, ManualLogSource logger)
    {
        Type? rewiredPlayerType = AccessTools.TypeByName("Rewired.Player");
        MethodInfo? fallbackPrefix = AccessTools.Method(typeof(GameInputPatch), nameof(ButtonPrefix));
        MethodInfo? stringPrefix = AccessTools.Method(typeof(GameInputPatch), nameof(ButtonStringPrefix));
        MethodInfo? intPrefix = AccessTools.Method(typeof(GameInputPatch), nameof(ButtonIntPrefix));
        MethodInfo? stringPostfix = AccessTools.Method(typeof(GameInputPatch), nameof(ButtonStringPostfix));
        MethodInfo? intPostfix = AccessTools.Method(typeof(GameInputPatch), nameof(ButtonIntPostfix));
        MethodInfo? noArgumentPostfix = AccessTools.Method(typeof(GameInputPatch), nameof(ButtonNoArgumentPostfix));

        if (rewiredPlayerType == null || fallbackPrefix == null)
        {
            logger.LogWarning("Could not find Rewired.Player button methods. FoA Mod Manager may not fully lock character actions while open.");
            return;
        }

        int patched = 0;
        int probePatched = 0;
        foreach (MethodInfo method in rewiredPlayerType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!ButtonMethodNames.Contains(method.Name, StringComparer.Ordinal) || method.ReturnType != typeof(bool))
            {
                continue;
            }

            HarmonyMethod? buttonPrefix = GetControllerButtonPrefix(method, stringPrefix, intPrefix, fallbackPrefix);
            HarmonyMethod? probePostfix = GetControllerProbePostfix(method, stringPostfix, intPostfix, noArgumentPostfix);
            harmony.Patch(method, prefix: buttonPrefix, postfix: probePostfix);
            patched++;
            if (probePostfix != null)
            {
                probePatched++;
            }
        }

        if (patched == 0)
        {
            logger.LogWarning("No Rewired.Player button overloads were patched. FoA Mod Manager may not fully lock character actions while open.");
            return;
        }

        logger.LogInfo($"Patched {patched} Rewired.Player button overloads for FoA Mod Manager action lock; controller probe postfixes={probePatched}.");
    }

    private static void PatchUnityKeyCodes(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? prefix = AccessTools.Method(typeof(GameInputPatch), nameof(UnityKeyCodePrefix));
        if (prefix == null)
        {
            logger.LogWarning("Could not find Unity KeyCode prefix. Controller hotkeys may fire without L3.");
            return;
        }

        int patched = 0;
        foreach (string methodName in new[] { "GetKey", "GetKeyDown", "GetKeyUp" })
        {
            MethodInfo? target = typeof(Input)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(method =>
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    return string.Equals(method.Name, methodName, StringComparison.Ordinal) &&
                        method.ReturnType == typeof(bool) &&
                        parameters.Length == 1 &&
                        parameters[0].ParameterType == typeof(KeyCode);
                });

            if (target == null)
            {
                logger.LogWarning($"Could not patch Unity Input.{methodName}(KeyCode). Controller hotkeys may fire without L3.");
                continue;
            }

            try
            {
                harmony.Patch(target, prefix: new HarmonyMethod(prefix));
                patched++;
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Could not patch Unity Input.{methodName}(KeyCode): {ex.GetType().Name}: {ex.Message}. Controller hotkeys may fire without L3.");
            }
        }

        logger.LogInfo($"Patched {patched} Unity Input KeyCode methods for FoA Mod Manager controller hotkey gating.");
    }

    private static HarmonyMethod GetControllerButtonPrefix(
        MethodInfo method,
        MethodInfo? stringPrefix,
        MethodInfo? intPrefix,
        MethodInfo fallbackPrefix)
    {
        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length == 0)
        {
            return new HarmonyMethod(fallbackPrefix);
        }

        Type firstParameterType = parameters[0].ParameterType;
        if (firstParameterType == typeof(string) && stringPrefix != null)
        {
            return new HarmonyMethod(stringPrefix);
        }

        if (firstParameterType == typeof(int) && intPrefix != null)
        {
            return new HarmonyMethod(intPrefix);
        }

        return new HarmonyMethod(fallbackPrefix);
    }

    private static HarmonyMethod? GetControllerProbePostfix(
        MethodInfo method,
        MethodInfo? stringPostfix,
        MethodInfo? intPostfix,
        MethodInfo? noArgumentPostfix)
    {
        ParameterInfo[] parameters = method.GetParameters();
        if (parameters.Length == 0)
        {
            return noArgumentPostfix == null ? null : new HarmonyMethod(noArgumentPostfix);
        }

        Type firstParameterType = parameters[0].ParameterType;
        if (firstParameterType == typeof(string))
        {
            return stringPostfix == null ? null : new HarmonyMethod(stringPostfix);
        }

        if (firstParameterType == typeof(int))
        {
            return intPostfix == null ? null : new HarmonyMethod(intPostfix);
        }

        return null;
    }

    private static bool BlockWhenManagerVisiblePrefix()
    {
        return !Plugin.IsModUiInputOwned;
    }

    private static bool PlayerInputProcessLateUpdatePrefix(PlayerInput __instance)
    {
        if (!Plugin.IsModUiInputOwned)
        {
            return true;
        }

        MoveInputField?.SetValue(__instance, Vector2.zero);
        MountMoveInputField?.SetValue(__instance, Vector2.zero);
        LookInputField?.SetValue(__instance, Vector2.zero);
        return false;
    }

    private static bool AxisPrefix(ref float __result)
    {
        if (!Plugin.IsModUiInputOwned || Plugin.IsInternalControllerInputReadActive)
        {
            return true;
        }

        __result = 0f;
        return false;
    }

    private static bool ButtonPrefix(ref bool __result)
    {
        if (TryBlockModUiButton(ref __result))
        {
            return false;
        }

        // No-argument Rewired reads cannot distinguish L3 from other buttons.
        // Let them pass so FoA's native L3 sprint binding remains functional.
        return true;
    }

    private static bool ButtonStringPrefix(string __0, ref bool __result)
    {
        if (TryBlockModUiButton(ref __result))
        {
            return false;
        }

        if (Plugin.ShouldBlockControllerActionLayerButton(__0))
        {
            __result = false;
            return false;
        }

        if (Plugin.ShouldSuppressControllerActionLayerButton(__0))
        {
            __result = false;
            return false;
        }

        return true;
    }

    private static bool ButtonIntPrefix(int __0, ref bool __result)
    {
        if (TryBlockModUiButton(ref __result))
        {
            return false;
        }

        if (Plugin.ShouldBlockControllerActionLayerButton(__0))
        {
            __result = false;
            return false;
        }

        if (Plugin.ShouldSuppressControllerActionLayerButton(__0))
        {
            __result = false;
            return false;
        }

        return true;
    }

    private static bool UnityKeyCodePrefix(KeyCode __0, ref bool __result)
    {
        if (Plugin.ShouldBlockControllerHotkeyKey(__0))
        {
            __result = false;
            return false;
        }

        return true;
    }

    private static bool TryBlockModUiButton(ref bool __result)
    {
        if (!Plugin.IsModUiInputOwned || Plugin.IsInternalControllerInputReadActive)
        {
            return false;
        }

        __result = false;
        return true;
    }

    private static void ButtonStringPostfix(object __instance, string __0, bool __result, MethodBase __originalMethod)
    {
        if (__result && !Plugin.IsInternalControllerInputReadActive)
        {
            Plugin.RecordControllerProbeButtonResult("name", __0, __originalMethod.Name, __instance);
        }
    }

    private static void ButtonIntPostfix(object __instance, int __0, bool __result, MethodBase __originalMethod)
    {
        if (__result && !Plugin.IsInternalControllerInputReadActive)
        {
            Plugin.RecordControllerProbeButtonResult("id", __0.ToString(), __originalMethod.Name, __instance);
        }
    }

    private static void ButtonNoArgumentPostfix(object __instance, bool __result, MethodBase __originalMethod)
    {
        if (__result && !Plugin.IsInternalControllerInputReadActive)
        {
            Plugin.RecordControllerProbeButtonResult("none", "<none>", __originalMethod.Name, __instance);
        }
    }
}
