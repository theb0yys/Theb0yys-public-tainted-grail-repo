using System;
using System.Collections.Generic;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Stats;
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
    private static BepInEx.Logging.ManualLogSource? _log;
    private Harmony? _harmony;

    private void Awake()
    {
        _targetCapacity = Config.Bind("Carry", "TotalCarryCapacity", 150f, "Target total EncumbranceLimit.");
        _log = Logger;
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        Hero? hero = Hero.Current;
        if (hero != null && !hero.HasBeenDiscarded)
        {
            DiscardOwnedTweaks(hero);
        }

        _harmony?.UnpatchSelf();
        _log = null;
    }

    [HarmonyPatch(typeof(HeroStats.HeroStatsWrapper), nameof(HeroStats.HeroStatsWrapper.Initialize))]
    private static class HeroStatsInitializePatch
    {
        private static void Postfix(HeroStats heroStats)
        {
            Apply(heroStats);
        }
    }

    private static void Apply(HeroStats? heroStats)
    {
        if (heroStats == null || heroStats.EncumbranceLimit == null)
        {
            return;
        }

        Hero hero = heroStats.ParentModel;
        if (hero == null || hero.HasBeenDiscarded)
        {
            return;
        }

        DiscardOwnedTweaks(hero);

        float baseValue = heroStats.EncumbranceLimit.BaseValue;
        float target = Math.Max(0f, _targetCapacity?.Value ?? baseValue);
        float modifier = target - baseValue;

        if (Math.Abs(modifier) <= 0.001f)
        {
            return;
        }

        hero.AddElement(new CarryTweak(heroStats.EncumbranceLimit, modifier));
        _log?.LogInfo($"Carry capacity base={baseValue:0.##}; target={target:0.##}; modifier={modifier:0.##}.");
    }

    private static void DiscardOwnedTweaks(Hero hero)
    {
        var owned = new List<CarryTweak>();
        foreach (CarryTweak tweak in hero.Elements<CarryTweak>())
        {
            owned.Add(tweak);
        }

        foreach (CarryTweak tweak in owned)
        {
            tweak.Discard();
        }

        hero.TryGetElement<HeroEncumbered>()?.TryGetElement<CarryTweak>()?.Discard();
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
