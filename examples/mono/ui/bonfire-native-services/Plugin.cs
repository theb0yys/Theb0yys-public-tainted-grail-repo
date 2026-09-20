using System;
using Awaken.TG.Main.Crafting.Fireplace;
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

    private static FireplaceUI? _activeFireplace;
    private static BepInEx.Logging.ManualLogSource? _log;
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
                _log?.LogInfo($"Bonfire owner observed: {fireplace.GetType().FullName}. Native services are now available to the example.");
            }
        }
    }
}
