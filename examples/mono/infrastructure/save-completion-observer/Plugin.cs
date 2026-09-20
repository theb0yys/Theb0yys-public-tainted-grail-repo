using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace TGCommunity.Example.SaveCompletionObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.save-completion-observer";
    public const string PluginName = "TG Example - Save Completion Observer";
    public const string PluginVersion = "0.1.0";

    private static readonly ConcurrentQueue<string> CompletedSlots = new();
    private Harmony? _harmony;

    private void Awake()
    {
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void Update()
    {
        while (CompletedSlots.TryDequeue(out string slotId))
        {
            Logger.LogInfo($"FoA save completion observed. slotId={slotId}");
        }
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }

    [HarmonyPatch]
    private static class CloudEndSavePatch
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
                MethodInfo? method = type == null
                    ? null
                    : AccessTools.Method(type, "EndSave", new[] { typeof(string) });

                if (method != null)
                {
                    yield return method;
                }
            }
        }

        private static void Postfix(string slotId)
        {
            if (!string.IsNullOrWhiteSpace(slotId))
            {
                CompletedSlots.Enqueue(slotId);
            }
        }
    }
}
