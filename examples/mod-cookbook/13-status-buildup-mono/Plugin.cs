using System;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Statuses;
using Awaken.TG.Main.Skills;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGExample.StatusBuildup;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.status-buildup";
    public const string PluginName = "TG Example - Status Buildup";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<float> _multiplier = null!;
    private Harmony? _harmony;

    internal static float Multiplier => Clamp(s_instance?._multiplier.Value ?? 1f, 0f, 5f);

    private void Awake()
    {
        s_instance = this;
        _multiplier = Config.Bind("Status", "BuildupMultiplier", 1f,
            new ConfigDescription("Player-caused buildup multiplier. 1 is vanilla.",
                new AcceptableValueRange<float>(0f, 5f)));

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded. Multiplier={Multiplier:0.##}");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        s_instance = null;
    }

    private static float Clamp(float value, float min, float max)
    {
        if (float.IsNaN(value) || float.IsInfinity(value)) return 1f;
        return Math.Max(min, Math.Min(max, value));
    }
}

[HarmonyPatch(typeof(CharacterStatuses), nameof(CharacterStatuses.BuildupStatus))]
internal static class StatusBuildupPatch
{
    private static void Prefix(ref float buildupStrength, StatusTemplate statusTemplate, StatusSourceInfo sourceInfo)
    {
        if (buildupStrength <= 0f || statusTemplate == null || !statusTemplate.IsBuildupAble)
        {
            return;
        }

        if (sourceInfo?.SourceCharacter.TryGet(out ICharacter sourceCharacter) != true ||
            !ReferenceEquals(sourceCharacter, Hero.Current))
        {
            return;
        }

        buildupStrength = Math.Max(0f, Math.Min(100000f, buildupStrength * Plugin.Multiplier));
    }
}
