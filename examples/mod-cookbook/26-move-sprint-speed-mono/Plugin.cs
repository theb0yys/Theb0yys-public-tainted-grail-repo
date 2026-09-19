using System;
using System.Linq;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Stats;
using Awaken.TG.Main.Heroes.Stats.Tweaks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGExample.MoveSprintSpeed;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.move-sprint-speed";
    public const string PluginName = "TG Example - Move Sprint Speed";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<float> _move = null!;
    private ConfigEntry<float> _sprint = null!;
    private Harmony? _harmony;

    internal static float Move => Clamp(s_instance?._move.Value ?? 1f);
    internal static float Sprint => Clamp(s_instance?._sprint.Value ?? 1f);

    private void Awake()
    {
        s_instance = this;
        _move = Config.Bind("Movement", "MoveSpeedMultiplier", 1f,
            new ConfigDescription("Normal movement speed multiplier.", new AcceptableValueRange<float>(0.25f, 3f)));
        _sprint = Config.Bind("Movement", "SprintSpeedMultiplier", 1f,
            new ConfigDescription("Sprint speed multiplier.", new AcceptableValueRange<float>(0.25f, 3f)));

        _move.SettingChanged += OnChanged;
        _sprint.SettingChanged += OnChanged;

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        SpeedPatch.Reapply();
        Logger.LogInfo($"{PluginName} loaded. Move={Move:0.##}; Sprint={Sprint:0.##}");
    }

    private void OnChanged(object sender, EventArgs args) => SpeedPatch.Reapply();

    private void OnDestroy()
    {
        SpeedPatch.Remove();
        _harmony?.UnpatchSelf();
        s_instance = null;
    }

    private static float Clamp(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value)) return 1f;
        return Math.Max(0.25f, Math.Min(3f, value));
    }
}

[HarmonyPatch(typeof(HeroStats.HeroStatsWrapper), nameof(HeroStats.HeroStatsWrapper.Initialize))]
internal static class SpeedPatch
{
    private static void Postfix(HeroStats heroStats) => Apply(heroStats);

    internal static void Reapply()
    {
        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded) return;
        Apply(hero.HeroStats);
    }

    internal static void Remove()
    {
        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded) return;
        foreach (SpeedTweak tweak in hero.Elements<SpeedTweak>().ToArray()) tweak.Discard();
    }

    private static void Apply(HeroStats? stats)
    {
        if (stats == null || stats.ParentModel is not Hero hero || hero.HasBeenDiscarded) return;
        Upsert(hero, "Move", stats.MoveSpeed, Plugin.Move);
        Upsert(hero, "Sprint", stats.SprintSpeed, Plugin.Sprint);
    }

    private static void Upsert(Hero hero, string key, Stat? stat, float multiplier)
    {
        SpeedTweak? existing = hero.Elements<SpeedTweak>().FirstOrDefault(t => t.Key == key);
        if (stat == null || Math.Abs(multiplier - 1f) <= 0.001f)
        {
            existing?.Discard();
            return;
        }

        if (existing == null)
        {
            hero.AddElement(new SpeedTweak(key, stat, multiplier));
        }
        else
        {
            existing.SetModifier(multiplier);
        }
    }

    private sealed class SpeedTweak : StatTweak
    {
        public override bool IsNotSaved => true;
        internal string Key { get; }

        internal SpeedTweak(string key, Stat stat, float multiplier)
            : base(stat, multiplier, TweakPriority.Multiply, OperationType.Multi)
        {
            Key = key;
            MarkedNotSaved = true;
        }
    }
}
