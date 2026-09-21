using System;
using System.Linq;
using System.Reflection;
using Awaken.TG.Main.Heroes;
using Awaken.TG.MVC.UI;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace AvalonBroodmotherCompanion;

internal static class BroodmotherCompanionInputLockPatch
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
    }

    private static void PatchGameMousePosition(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? target = AccessTools.Method(typeof(GameUI), "UpdateMousePosition");
        MethodInfo? prefix = AccessTools.Method(typeof(BroodmotherCompanionInputLockPatch), nameof(BlockWhenCompanionUiVisiblePrefix));

        if (target == null || prefix == null)
        {
            logger.LogWarning("Could not patch GameUI.UpdateMousePosition. Broodmother companion UI may not fully freeze game hover input while open.");
            return;
        }

        harmony.Patch(target, prefix: new HarmonyMethod(prefix));
        logger.LogInfo("Patched GameUI.UpdateMousePosition for Broodmother companion UI input lock.");
    }

    private static void PatchPlayerInput(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? target = AccessTools.Method(typeof(PlayerInput), "ProcessLateUpdate");
        MethodInfo? prefix = AccessTools.Method(typeof(BroodmotherCompanionInputLockPatch), nameof(PlayerInputProcessLateUpdatePrefix));

        if (target == null || prefix == null)
        {
            logger.LogWarning("Could not patch PlayerInput.ProcessLateUpdate. Broodmother companion UI may not fully freeze player movement/camera input while open.");
            return;
        }

        harmony.Patch(target, prefix: new HarmonyMethod(prefix));
        logger.LogInfo("Patched PlayerInput.ProcessLateUpdate for Broodmother companion UI input lock.");
    }

    private static void PatchRewiredAxes(Harmony harmony, ManualLogSource logger)
    {
        Type? rewiredPlayerType = AccessTools.TypeByName("Rewired.Player");
        MethodInfo? prefix = AccessTools.Method(typeof(BroodmotherCompanionInputLockPatch), nameof(AxisPrefix));

        if (rewiredPlayerType == null || prefix == null)
        {
            logger.LogWarning("Could not find Rewired.Player axis methods. Broodmother companion UI may not fully freeze camera axes while open.");
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
            logger.LogWarning("No Rewired.Player axis overloads were patched. Broodmother companion UI may not fully freeze camera axes while open.");
            return;
        }

        logger.LogInfo($"Patched {patched} Rewired.Player axis overloads for Broodmother companion UI input lock.");
    }

    private static void PatchRewiredButtons(Harmony harmony, ManualLogSource logger)
    {
        Type? rewiredPlayerType = AccessTools.TypeByName("Rewired.Player");
        MethodInfo? prefix = AccessTools.Method(typeof(BroodmotherCompanionInputLockPatch), nameof(ButtonPrefix));

        if (rewiredPlayerType == null || prefix == null)
        {
            logger.LogWarning("Could not find Rewired.Player button methods. Broodmother companion UI may not fully lock character actions while open.");
            return;
        }

        int patched = 0;
        foreach (MethodInfo method in rewiredPlayerType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!ButtonMethodNames.Contains(method.Name, StringComparer.Ordinal) || method.ReturnType != typeof(bool))
            {
                continue;
            }

            harmony.Patch(method, prefix: new HarmonyMethod(prefix));
            patched++;
        }

        if (patched == 0)
        {
            logger.LogWarning("No Rewired.Player button overloads were patched. Broodmother companion UI may not fully lock character actions while open.");
            return;
        }

        logger.LogInfo($"Patched {patched} Rewired.Player button overloads for Broodmother companion UI action lock.");
    }

    private static bool BlockWhenCompanionUiVisiblePrefix()
    {
        return !Plugin.IsCompanionUiVisibleForInputLock;
    }

    private static bool PlayerInputProcessLateUpdatePrefix(PlayerInput __instance)
    {
        if (!Plugin.IsCompanionUiVisibleForInputLock)
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
        if (!Plugin.IsCompanionUiVisibleForInputLock ||
            Plugin.IsCompanionDialogueVisibleForInputLock ||
            FoAModManagerBridge.IsControllerCursorInputReadActive())
        {
            return true;
        }

        __result = 0f;
        return false;
    }

    private static bool ButtonPrefix(ref bool __result)
    {
        if (!Plugin.IsCompanionUiVisibleForInputLock ||
            Plugin.IsCompanionDialogueVisibleForInputLock ||
            FoAModManagerBridge.IsControllerCursorInputReadActive())
        {
            return true;
        }

        __result = false;
        return false;
    }
}
