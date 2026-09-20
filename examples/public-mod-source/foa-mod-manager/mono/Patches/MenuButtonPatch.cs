using System;
using System.Reflection;
using Awaken.TG.Main.UI.Menu;
using Awaken.TG.Main.UI.TitleScreen;
using Awaken.TG.Main.Utility.UI;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace FoAModManager.Patches;

internal static class MenuButtonPatch
{
    private const string OptionsFieldName = "options";
    private const string OnInitializeMethodName = "OnInitialize";
    private const string ManagerButtonName = "FoAModManagerButton";
    private const string ManagerButtonText = Plugin.PluginName;

    private static ManualLogSource? _logger;

    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        _logger = logger;

        PatchInitialize(harmony, logger, typeof(VMenuUI), nameof(VMenuUIOnInitializePostfix));
        PatchInitialize(harmony, logger, typeof(VTitleScreenUI), nameof(VTitleScreenUIOnInitializePostfix));
    }

    private static void PatchInitialize(Harmony harmony, ManualLogSource logger, Type viewType, string postfixName)
    {
        MethodInfo? target = AccessTools.Method(viewType, OnInitializeMethodName);
        MethodInfo? postfix = AccessTools.Method(typeof(MenuButtonPatch), postfixName);

        if (target == null || postfix == null)
        {
            logger.LogWarning($"Could not find {viewType.FullName}.{OnInitializeMethodName}. FoA Mod Manager button will not be added there.");
            return;
        }

        harmony.Patch(target, postfix: new HarmonyMethod(postfix));
        logger.LogInfo($"Patched {viewType.Name}.{OnInitializeMethodName} for FoA Mod Manager button.");
    }

    private static void VMenuUIOnInitializePostfix(VMenuUI __instance)
    {
        AddManagerButton(__instance, typeof(VMenuUI));
    }

    private static void VTitleScreenUIOnInitializePostfix(VTitleScreenUI __instance)
    {
        AddManagerButton(__instance, typeof(VTitleScreenUI));
    }

    private static void AddManagerButton(object view, Type viewType)
    {
        try
        {
            ButtonConfig? optionsButton = GetOptionsButton(view, viewType);
            if (optionsButton == null || optionsButton.button == null)
            {
                _logger?.LogWarning($"{viewType.Name} options button was not available. FoA Mod Manager button was not added.");
                return;
            }

            Transform parent = optionsButton.transform.parent;
            if (parent == null)
            {
                _logger?.LogWarning($"{viewType.Name} options button has no parent. FoA Mod Manager button was not added.");
                return;
            }

            if (parent.Find(ManagerButtonName) != null)
            {
                return;
            }

            GameObject managerButtonObject = UnityEngine.Object.Instantiate(optionsButton.gameObject, parent);
            managerButtonObject.name = ManagerButtonName;
            managerButtonObject.transform.SetSiblingIndex(optionsButton.transform.GetSiblingIndex() + 1);

            ButtonConfig? managerButton = managerButtonObject.GetComponent<ButtonConfig>();
            if (managerButton == null || managerButton.button == null)
            {
                UnityEngine.Object.Destroy(managerButtonObject);
                _logger?.LogWarning($"{viewType.Name} cloned menu button is missing ButtonConfig. FoA Mod Manager button was not added.");
                return;
            }

            managerButton.button.ClearAllOnClickEvents();
            managerButton.InitializeButton(Plugin.ShowManager, ManagerButtonText);
        }
        catch (Exception ex)
        {
            _logger?.LogError($"Failed to add FoA Mod Manager button to {viewType.Name}: {ex}");
        }
    }

    private static ButtonConfig? GetOptionsButton(object view, Type viewType)
    {
        FieldInfo? optionsField = AccessTools.Field(viewType, OptionsFieldName);
        return optionsField?.GetValue(view) as ButtonConfig;
    }
}
