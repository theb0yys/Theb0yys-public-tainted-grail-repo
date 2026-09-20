using System;
using System.Collections.Generic;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Stats;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGCommunity.Example.CarryCapacity;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.carry-capacity";
    public const string PluginName = "TG Example - Carry Capacity";
    public const string PluginVersion = "0.1.0";

    private static ConfigEntry<float>? _totalCapacity;
    private Harmony? _harmony;

    private void Awake()
    {
        _totalCapacity = Config.Bind(
            "Carry",
            "TotalCapacity",
            100f,
            "Target total carry capacity. Set to the native base value to remove the tweak.");

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        Hero? hero = Hero.Current;
        if (hero != null && !hero.HasBeenDiscarded)
        {
            DiscardExisting(hero);
        }
    }

    [HarmonyPatch(typeof(HeroStats.HeroStatsWrapper), nameof(HeroStats.HeroStatsWrapper.Initialize))]
    private static class HeroStatsInitializePatch
    {
        private static void Postfix(HeroStats heroStats)
        {
            Apply(heroStats);
        }
    }

    private static void Apply(HeroStats heroStats)
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

        DiscardExisting(hero);

        float modifier = (_totalCapacity?.Value ?? heroStats.EncumbranceLimit.BaseValue)
            - heroStats.EncumbranceLimit.BaseValue;

        if (Math.Abs(modifier) <= 0.001f)
        {
            return;
        }

        hero.AddElement(new CarryTweak(heroStats.EncumbranceLimit, modifier));
    }

    private static void DiscardExisting(Hero hero)
    {
        var found = new List<CarryTweak>();
        foreach (CarryTweak tweak in hero.Elements<CarryTweak>())
        {
            found.Add(tweak);
        }

        foreach (CarryTweak tweak in found)
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
