using Awaken.TG.Main.Stories;
using Awaken.TG.Main.Stories.Steps.Helpers;
using BepInEx;
using HarmonyLib;

namespace TGExample.DialogueObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.dialogue-observer";
    public const string PluginName = "TG Example - Dialogue Observer";
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

[HarmonyPatch(typeof(Story), nameof(Story.OfferChoice))]
internal static class StoryOfferChoicePatch
{
    private static void Prefix(Story __instance, ChoiceConfig choiceConfig)
    {
        if (__instance == null || choiceConfig == null)
        {
            return;
        }

        Plugin.Instance?.Logger.LogInfo("Story.OfferChoice observed.");
    }
}
