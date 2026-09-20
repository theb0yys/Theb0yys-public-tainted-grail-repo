using System;
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
    public const string PluginVersion = "0.1.0";

    private const string NativeServicesButtonName = "TGCommunityBonfireServicesButton";

    private static FireplaceUI? _activeFireplace;
    private static BepInEx.Logging.ManualLogSource? _log;
    private static GameObject? _ownedButton;
    private Harmony? _harmony;

    private void Awake()
    {
        _log = Logger;
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded. At an initialized bonfire: F6 stash, F7 cooking, F8 alchemy.");
    }

    private void Update()
    {
        FireplaceUI? fireplace = _activeFireplace;
        if (fireplace == null || fireplace.HasBeenDiscarded)
        {
            _activeFireplace = null;
            return;
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            Invoke("stash", fireplace.OpenHeroStorage);
        }
        else if (Input.GetKeyDown(KeyCode.F7))
        {
            Invoke("cooking", fireplace.CookAction);
        }
        else if (Input.GetKeyDown(KeyCode.F8))
        {
            Invoke("alchemy", fireplace.AlchemyAction);
        }
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();

        if (_ownedButton != null)
        {
            UnityEngine.Object.Destroy(_ownedButton);
            _ownedButton = null;
        }

        _activeFireplace = null;
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
            if (__instance.GenericTarget is FireplaceUI fireplace)
            {
                _activeFireplace = fireplace;
                TryAttachNativeServicesEntry(__instance, fireplace);
                _log?.LogInfo($"Bonfire owner observed: {fireplace.GetType().FullName}. Native services are now available to the example.");
            }
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
                _log?.LogWarning("Native Services entry skipped: VFireplaceUI.levelUp was not found.");
                return;
            }

            Type entryType = levelUpEntry.GetType();
            FieldInfo? buttonConfigField = AccessTools.Field(entryType, "buttonConfig");
            if (buttonConfigField?.GetValue(levelUpEntry) is not ButtonConfig levelUpConfig ||
                levelUpConfig.button == null ||
                levelUpConfig.transform.parent == null)
            {
                _log?.LogWarning("Native Services entry skipped: Level Up ButtonConfig was unavailable.");
                return;
            }

            Transform parent = levelUpConfig.transform.parent;
            Transform? existing = parent.Find(NativeServicesButtonName);
            if (existing != null)
            {
                _ownedButton = existing.gameObject;
                return;
            }

            GameObject clone = UnityEngine.Object.Instantiate(levelUpConfig.gameObject, parent);
            clone.name = NativeServicesButtonName;
            clone.transform.SetSiblingIndex(
                Mathf.Min(parent.childCount - 1, levelUpConfig.transform.GetSiblingIndex() + 2));

            ButtonConfig? servicesConfig = clone.GetComponent<ButtonConfig>();
            if (servicesConfig == null || servicesConfig.button == null)
            {
                UnityEngine.Object.Destroy(clone);
                _log?.LogWarning("Native Services entry skipped: cloned ButtonConfig was unavailable.");
                return;
            }

            servicesConfig.button.ClearAllOnClickEvents();
            Action openServices = () => Invoke("stash", fireplace.OpenHeroStorage);

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
                        "Example native-style entry. Opens the existing hero storage service.",
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
            _log?.LogWarning($"Native Services entry failed: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
