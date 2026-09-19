using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace TGExample.SaveObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.save-observer";
    public const string PluginName = "TG Example - Save Observer";
    public const string PluginVersion = "0.1.0";

    internal static Plugin? Instance { get; private set; }
    private Harmony? _harmony;

    private void Awake()
    {
        Instance = this;
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        Instance = null;
    }
}

[HarmonyPatch]
internal static class CloudServiceEndSavePatch
{
    private static readonly string[] TypeNames =
    {
        "Awaken.TG.Main.Saving.Cloud.Services.SteamCloudService",
        "Awaken.TG.Main.Saving.Cloud.Services.SteamNoCloudService",
        "Awaken.TG.Main.Saving.Cloud.Services.DebugCloudService",
        "Awaken.TG.Main.Saving.Cloud.Services.GogCloudService"
    };

    private static IEnumerable<MethodBase> TargetMethods()
    {
        foreach (string typeName in TypeNames)
        {
            System.Type? type = AccessTools.TypeByName(typeName);
            MethodInfo? method = type == null ? null : AccessTools.Method(type, "EndSave", new[] { typeof(string) });
            if (method != null)
            {
                yield return method;
            }
        }
    }

    private static void Postfix(string slotId)
    {
        Plugin.Instance?.Logger.LogInfo($"Observed native EndSave slotId='{slotId}'.");
    }
}
