using System;
using System.Linq;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Stats;
using Awaken.TG.Main.Heroes.Stats.Tweaks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGExample.CarryCapacity;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.carry-capacity";
    public const string PluginName = "TG Example - Carry Capacity";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<float> _targetCapacity = null!;
    private Harmony? _harmony;

    internal static float TargetCapacity => s_instance == null ? 0f : Clamp(s_instance._targetCapacity.Value);

    private void Awake()
    {
        s_instance = this;
        _targetCapacity = Config.Bind(
            "Balance",
            "TotalCarryCapacity",
            0f,
            new ConfigDescription("Final carry capacity. 0 keeps vanilla.",
                new AcceptableValueRange<float>(0f, 100000f)));

        _targetCapacity.SettingChanged += (_, _) => CarryCapacityPatch.ReapplyCurrentHero();

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        CarryCapacityPatch.ReapplyCurrentHero();

        Logger.LogInfo($"{PluginName} loaded. Target={TargetCapacity:0.##}");
    }

    private void OnDestroy()
    {
        CarryCapacityPatch.RemoveCurrentHeroTweak();
        _harmony?.UnpatchSelf();
        s_instance = null;
    }

    private static float Clamp(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            return 0f;
        }

        return Math.Max(0f, Math.Min(100000f, value));
    }
}

[HarmonyPatch(typeof(HeroStats.HeroStatsWrapper), nameof(HeroStats.HeroStatsWrapper.Initialize))]
internal static class CarryCapacityPatch
{
    private static void Postfix(HeroStats heroStats)
    {
        Apply(heroStats);
    }

    internal static void ReapplyCurrentHero()
    {
        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            return;
        }

        Apply(hero.HeroStats);
    }

    internal static void RemoveCurrentHeroTweak()
    {
        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            return;
        }

        foreach (ExampleCarryTweak tweak in hero.Elements<ExampleCarryTweak>().ToArray())
        {
            tweak.Discard();
        }
    }

    private static void Apply(HeroStats stats)
    {
        if (stats?.EncumbranceLimit == null || stats.ParentModel is not Hero hero || hero.HasBeenDiscarded)
        {
            return;
        }

        foreach (ExampleCarryTweak tweak in hero.Elements<ExampleCarryTweak>().ToArray())
        {
            tweak.Discard();
        }

        float target = Plugin.TargetCapacity;
        if (target <= 0f)
        {
            return;
        }

        float modifier = target - stats.EncumbranceLimit.BaseValue;
        if (Math.Abs(modifier) <= 0.001f)
        {
            return;
        }

        hero.AddElement(new ExampleCarryTweak(stats.EncumbranceLimit, modifier));
    }

    private sealed class ExampleCarryTweak : StatTweak
    {
        public override bool IsNotSaved => true;

        internal ExampleCarryTweak(Stat stat, float modifier)
            : base(stat, modifier, null, OperationType.Add)
        {
            MarkedNotSaved = true;
        }
    }
}
