using System;
using System.Linq;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Stats;
using Awaken.TG.Main.Heroes.Stats.Tweaks;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGExample.StaminaDrain;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.stamina-drain";
    public const string PluginName = "TG Example - Stamina Drain";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<float> _sprintMultiplier = null!;
    private ConfigEntry<float> _actionMultiplier = null!;
    private Harmony? _harmony;

    internal static float SprintMultiplier => Read(s_instance?._enabled.Value == true ? s_instance._sprintMultiplier.Value : 1f);
    internal static float ActionMultiplier => Read(s_instance?._enabled.Value == true ? s_instance._actionMultiplier.Value : 1f);

    private void Awake()
    {
        s_instance = this;

        _enabled = Config.Bind("General", "Enabled", true, "Enable the stamina example.");
        _sprintMultiplier = Config.Bind(
            "Sprinting",
            "Multiplier",
            1f,
            new ConfigDescription("Sprint stamina usage. 1 is vanilla; 0 is no supported sprint drain.",
                new AcceptableValueRange<float>(0f, 1f)));
        _actionMultiplier = Config.Bind(
            "Actions",
            "Multiplier",
            1f,
            new ConfigDescription("Supported action stamina usage. 1 is vanilla; 0 is no supported action drain.",
                new AcceptableValueRange<float>(0f, 1f)));

        _enabled.SettingChanged += OnSettingChanged;
        _sprintMultiplier.SettingChanged += OnSettingChanged;
        _actionMultiplier.SettingChanged += OnSettingChanged;

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        StaminaStatsPatch.ReapplyCurrentHero();

        Logger.LogInfo($"{PluginName} loaded. Sprint={SprintMultiplier:0.##}; Actions={ActionMultiplier:0.##}");
    }

    private void OnSettingChanged(object sender, EventArgs args)
    {
        StaminaStatsPatch.ReapplyCurrentHero();
    }

    private void OnDestroy()
    {
        StaminaStatsPatch.RemoveCurrentHeroTweaks();
        _harmony?.UnpatchSelf();
        s_instance = null;
    }

    private static float Read(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            return 1f;
        }

        return Mathf.Clamp01(value);
    }
}

[HarmonyPatch(typeof(CharacterStats.CharacterStatsWrapper), nameof(CharacterStats.CharacterStatsWrapper.Initialize))]
internal static class StaminaStatsPatch
{
    private static void Postfix(CharacterStats stats)
    {
        Apply(stats);
    }

    internal static void ReapplyCurrentHero()
    {
        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            return;
        }

        Apply(hero.CharacterStats);
    }

    internal static void RemoveCurrentHeroTweaks()
    {
        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            return;
        }

        foreach (ExampleStaminaTweak tweak in hero.Elements<ExampleStaminaTweak>().ToArray())
        {
            tweak.Discard();
        }
    }

    private static void Apply(CharacterStats stats)
    {
        if (stats?.ParentModel is not Hero hero || hero.HasBeenDiscarded)
        {
            return;
        }

        ApplyOne(hero, stats.SprintCostMultiplier, StaminaKind.Sprint, Plugin.SprintMultiplier);
        ApplyOne(hero, stats.StaminaUsageMultiplier, StaminaKind.Actions, Plugin.ActionMultiplier);
    }

    private static void ApplyOne(Hero hero, Stat stat, StaminaKind kind, float multiplier)
    {
        ExampleStaminaTweak? existing = hero.Elements<ExampleStaminaTweak>().FirstOrDefault(t => t.Kind == kind);

        if (Math.Abs(multiplier - 1f) <= 0.0001f)
        {
            existing?.Discard();
            return;
        }

        if (existing == null)
        {
            hero.AddElement(new ExampleStaminaTweak(kind, stat, multiplier));
        }
        else
        {
            existing.SetModifier(multiplier);
        }
    }

    private enum StaminaKind
    {
        Sprint,
        Actions
    }

    private sealed class ExampleStaminaTweak : StatTweak
    {
        public override bool IsNotSaved => true;
        internal StaminaKind Kind { get; }

        internal ExampleStaminaTweak(StaminaKind kind, Stat stat, float multiplier)
            : base(stat, multiplier, TweakPriority.Multiply, OperationType.Multi)
        {
            Kind = kind;
            MarkedNotSaved = true;
        }
    }
}
