using System;
using System.Collections.Generic;
using System.Reflection;
using Awaken.TG.Main.Crafting.Fireplace;
using Awaken.TG.Main.Utility.UI;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace TGCommunity.Example.BonfireNativeServices;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.bonfire-native-services";
    public const string PluginName = "TG Example - Bonfire Native Services";
    public const string PluginVersion = "0.2.0";

    private const string NativeServicesButtonName = "TGCommunityBonfireServicesButton";
    private const string SubmenuPrefix = "TGCommunityBonfireServices_";

    private static readonly List<GameObject> OwnedSubmenuButtons = new();

    private static FireplaceUI? _activeFireplace;
    private static VFireplaceUI? _activeView;
    private static ButtonConfig? _buttonTemplate;
    private static Transform? _buttonParent;
    private static BepInEx.Logging.ManualLogSource? _log;
    private static GameObject? _ownedButton;
    private Harmony? _harmony;

    private void Awake()
    {
        _log = Logger;
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        CloseSubmenu();

        if (_ownedButton != null)
        {
            UnityEngine.Object.Destroy(_ownedButton);
            _ownedButton = null;
        }

        _activeFireplace = null;
        _activeView = null;
        _buttonTemplate = null;
        _buttonParent = null;
        _log = null;
    }

    private static void Invoke(string service, Action action)
    {
        try
        {
            action();
            _log?.LogInfo($"Native bonfire service invoked: {service}.");
        }
        catch (Exception ex)
        {
            _log?.LogWarning($"Native bonfire service failed: service={service}; {ex.GetType().Name}: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(VFireplaceUI), "OnInitialize")]
    private static class FireplaceInitializePatch
    {
        private static void Postfix(VFireplaceUI __instance)
        {
            if (__instance.GenericTarget is not FireplaceUI fireplace)
            {
                return;
            }

            CloseSubmenu();

            if (_ownedButton != null)
            {
                UnityEngine.Object.Destroy(_ownedButton);
                _ownedButton = null;
            }

            _activeView = __instance;
            _activeFireplace = fireplace;
            TryAttachNativeServicesEntry(__instance, fireplace);
            _log?.LogInfo($"Bonfire owner observed: {fireplace.GetType().FullName}.");
        }
    }

    private static void TryAttachNativeServicesEntry(VFireplaceUI view, FireplaceUI fireplace)
    {
        try
        {
            FieldInfo? levelUpField = AccessTools.Field(typeof(VFireplaceUI), "levelUp");
            object? levelUpEntry = levelUpField?.GetValue(view);
            if (levelUpEntry == null)
            {
                _log?.LogWarning("Services entry skipped: VFireplaceUI.levelUp was not found.");
                return;
            }

            Type entryType = levelUpEntry.GetType();
            FieldInfo? buttonConfigField = AccessTools.Field(entryType, "buttonConfig");
            if (buttonConfigField?.GetValue(levelUpEntry) is not ButtonConfig levelUpConfig ||
                levelUpConfig.button == null ||
                levelUpConfig.transform.parent == null)
            {
                _log?.LogWarning("Services entry skipped: Level Up ButtonConfig was unavailable.");
                return;
            }

            _buttonTemplate = levelUpConfig;
            _buttonParent = levelUpConfig.transform.parent;

            Transform? existing = _buttonParent.Find(NativeServicesButtonName);
            if (existing != null)
            {
                _ownedButton = existing.gameObject;
                return;
            }

            GameObject clone = UnityEngine.Object.Instantiate(levelUpConfig.gameObject, _buttonParent);
            clone.name = NativeServicesButtonName;
            clone.transform.SetSiblingIndex(
                Mathf.Min(_buttonParent.childCount - 1, levelUpConfig.transform.GetSiblingIndex() + 2));

            ButtonConfig? servicesConfig = clone.GetComponent<ButtonConfig>();
            if (servicesConfig == null || servicesConfig.button == null)
            {
                UnityEngine.Object.Destroy(clone);
                _log?.LogWarning("Services entry skipped: cloned ButtonConfig was unavailable.");
                return;
            }

            servicesConfig.button.ClearAllOnClickEvents();
            Action openServices = () => OpenSubmenu(view, fireplace);

            object? servicesEntry = Activator.CreateInstance(entryType, nonPublic: true);
            MethodInfo? registerButton = AccessTools.Method(entryType, "RegisterButton");
            MethodInfo? showDescription = AccessTools.Method(typeof(VFireplaceUI), "ShowDescription");

            if (servicesEntry != null && registerButton != null && showDescription != null)
            {
                buttonConfigField.SetValue(servicesEntry, servicesConfig);

                Action<bool, string> description = (state, text) =>
                    showDescription.Invoke(view, new object[] { state, text });

                registerButton.Invoke(
                    servicesEntry,
                    new object[]
                    {
                        openServices,
                        "Services",
                        "Open additional native bonfire services.",
                        description
                    });
            }
            else
            {
                servicesConfig.InitializeButton(openServices, "Services");
            }

            _ownedButton = clone;
            _log?.LogInfo("Native-styled Services entry attached by cloning the Level Up ButtonConfig.");
        }
        catch (Exception ex)
        {
            _log?.LogWarning($"Services entry failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void OpenSubmenu(VFireplaceUI view, FireplaceUI fireplace)
    {
        if (_buttonTemplate == null || _buttonParent == null ||
            fireplace.HasBeenDiscarded || !ReferenceEquals(view, _activeView))
        {
            return;
        }

        CloseSubmenu();

        AddSubmenuButton(
            "Stash",
            () =>
            {
                Invoke("stash", fireplace.OpenHeroStorage);
                CloseSubmenu();
            });

        AddSubmenuButton(
            "Cooking",
            () =>
            {
                Invoke("cooking", fireplace.CookAction);
                CloseSubmenu();
            });

        AddSubmenuButton(
            "Alchemy",
            () =>
            {
                Invoke("alchemy", fireplace.AlchemyAction);
                CloseSubmenu();
            });

        AddSubmenuButton(
            "Handcrafting",
            () =>
            {
                Invoke("handcrafting", fireplace.HandcraftingAction);
                CloseSubmenu();
            });

        AddSubmenuButton(
            "Back",
            CloseSubmenu);

        _log?.LogInfo($"Native-style Services submenu opened. rows={OwnedSubmenuButtons.Count}.");
    }

    private static void AddSubmenuButton(string label, Action action)
    {
        if (_buttonTemplate == null || _buttonParent == null)
        {
            return;
        }

        GameObject clone = UnityEngine.Object.Instantiate(
            _buttonTemplate.gameObject,
            _buttonParent,
            worldPositionStays: false);

        clone.name = SubmenuPrefix + label.Replace(" ", string.Empty);
        clone.transform.SetAsLastSibling();

        ButtonConfig? config = clone.GetComponent<ButtonConfig>();
        if (config == null || config.button == null)
        {
            UnityEngine.Object.Destroy(clone);
            return;
        }

        config.button.ClearAllOnClickEvents();
        config.InitializeButton(action, label);
        OwnedSubmenuButtons.Add(clone);
    }

    private static void CloseSubmenu()
    {
        for (int i = OwnedSubmenuButtons.Count - 1; i >= 0; i--)
        {
            GameObject button = OwnedSubmenuButtons[i];
            if (button != null)
            {
                UnityEngine.Object.Destroy(button);
            }
        }

        OwnedSubmenuButtons.Clear();
    }
}
