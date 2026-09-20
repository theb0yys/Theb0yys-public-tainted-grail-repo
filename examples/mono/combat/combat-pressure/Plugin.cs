using System;
using System.Linq;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Stats;
using Awaken.TG.Main.Heroes.Stats.Tweaks;
using Awaken.TG.Main.Settings.Gameplay;
using Awaken.Utility;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGCommunity.Example.CombatPressure;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.combat-pressure";
    public const string PluginName = "TG Example - Combat Pressure";
    public const string PluginVersion = "0.1.0";

    private static Plugin? Instance;
    private ConfigEntry<float> _attackSlots = null!;
    private ConfigEntry<float> _attackRecovery = null!;
    private ConfigEntry<float> _actionStamina = null!;
    private Harmony? _harmony;

    private void Awake()
    {
        Instance = this;
        _attackSlots = Config.Bind("Pressure", "AttackSlotsMultiplier", 1.25f, "Scale Difficulty.MaxEnemiesAttacking.");
        _attackRecovery = Config.Bind("Pressure", "AttackRecoveryMultiplier", 0.85f, "Scale Difficulty.AttackActionUnBookProlong.");
        _actionStamina = Config.Bind("Pressure", "ActionStaminaMultiplier", 1.10f, "Scale Hero CharacterStats.StaminaUsageMultiplier.");

        _harmony = new Harmony(PluginGuid);
        _harmony.Patch(
            AccessTools.PropertyGetter(typeof(Difficulty), nameof(Difficulty.MaxEnemiesAttacking)),
            postfix: new HarmonyMethod(typeof(Plugin), nameof(MaxEnemiesPostfix)));
        _harmony.Patch(
            AccessTools.PropertyGetter(typeof(Difficulty), nameof(Difficulty.AttackActionUnBookProlong)),
            postfix: new HarmonyMethod(typeof(Plugin), nameof(RecoveryPostfix)));
        _harmony.Patch(
            AccessTools.Method(typeof(CharacterStats.CharacterStatsWrapper), nameof(CharacterStats.CharacterStatsWrapper.Initialize)),
            postfix: new HarmonyMethod(typeof(Plugin), nameof(CharacterStatsPostfix)));

        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        RemoveOwnedTweak();
        _harmony?.UnpatchSelf();
        Instance = null;
    }

    private static void MaxEnemiesPostfix(ref int __result)
    {
        Plugin? p = Instance;
        if (p == null) return;

        float multiplier = Mathf.Clamp(p._attackSlots.Value, 0.5f, 3f);
        if (Mathf.Abs(multiplier - 1f) <= 0.0001f) return;

        float scaled = __result * multiplier;
        int rounded = multiplier > 1f ? Mathf.CeilToInt(scaled) : Mathf.FloorToInt(scaled);
        __result = Mathf.Clamp(rounded, 1, 8);
    }

    private static void RecoveryPostfix(ref float __result)
    {
        Plugin? p = Instance;
        if (p == null) return;

        float multiplier = Mathf.Clamp(p._attackRecovery.Value, 0.25f, 3f);
        if (Mathf.Abs(multiplier - 1f) <= 0.0001f) return;

        __result = Mathf.Max(0f, __result * multiplier);
    }

    private static void CharacterStatsPostfix(CharacterStats stats)
    {
        Plugin? p = Instance;
        if (p == null || stats?.ParentModel is not Hero hero || hero.HasBeenDiscarded) return;

        foreach (ActionStaminaTweak tweak in hero.Elements<ActionStaminaTweak>().ToArray())
            tweak.Discard();

        float multiplier = Mathf.Clamp(p._actionStamina.Value, 0.5f, 3f);
        if (Mathf.Abs(multiplier - 1f) <= 0.0001f) return;

        hero.AddElement(new ActionStaminaTweak(stats.StaminaUsageMultiplier, multiplier));
    }

    private static void RemoveOwnedTweak()
    {
        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded) return;

        foreach (ActionStaminaTweak tweak in hero.Elements<ActionStaminaTweak>().ToArray())
            tweak.Discard();
    }

    private sealed class ActionStaminaTweak : StatTweak
    {
        public override bool IsNotSaved => true;

        internal ActionStaminaTweak(Stat stat, float multiplier)
            : base(stat, multiplier, TweakPriority.Multiply, OperationType.Multi)
        {
            MarkedNotSaved = true;
        }
    }
}
