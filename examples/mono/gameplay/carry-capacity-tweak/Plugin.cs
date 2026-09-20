using System;
using System.Linq;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Stats;
using Awaken.TG.Main.Heroes.Stats.Tweaks;
using Awaken.Utility;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGCommunity.Example.CarryCapacityTweak;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.carry-capacity-tweak";
    public const string PluginName = "TG Example - Carry Capacity Tweak";
    public const string PluginVersion = "0.1.0";

    private static ConfigEntry<float>? _targetCapacity;
    private Harmony? _harmony;

    private void Awake()
    {
        _targetCapacity = Config.Bind("Carry", "TargetCapacity", 150f, "Desired total EncumbranceLimit.");
        _harmony = new Harmony(PluginGuid);
        _harmony.Patch(
            AccessTools.Method(typeof(HeroStats.HeroStatsWrapper), nameof(HeroStats.HeroStatsWrapper.Initialize)),
            postfix: new HarmonyMethod(typeof(Plugin), nameof(Postfix)));
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        RemoveOwned();
        _harmony?.UnpatchSelf();
    }

    private static void Postfix(HeroStats heroStats)
    {
        if (heroStats == null || heroStats.EncumbranceLimit == null) return;
        Hero hero = heroStats.ParentModel;
        if (hero == null || hero.HasBeenDiscarded) return;

        foreach (CarryTweak tweak in hero.Elements<CarryTweak>().ToArray())
            tweak.Discard();

        float desired = Math.Max(1f, _targetCapacity?.Value ?? heroStats.EncumbranceLimit.BaseValue);
        float modifier = desired - heroStats.EncumbranceLimit.BaseValue;
        if (Math.Abs(modifier) <= 0.001f) return;

        hero.AddElement(new CarryTweak(heroStats.EncumbranceLimit, modifier));
    }

    private static void RemoveOwned()
    {
        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded) return;

        foreach (CarryTweak tweak in hero.Elements<CarryTweak>().ToArray())
            tweak.Discard();
    }

    private sealed class CarryTweak : StatTweak
    {
        public override bool IsNotSaved => true;

        internal CarryTweak(Stat stat, float modifier)
            : base(stat, modifier, null, OperationType.Add)
        {
            MarkedNotSaved = true;
        }
    }
}
