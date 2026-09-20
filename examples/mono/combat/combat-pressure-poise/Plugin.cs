using System;
using Awaken.TG.Main.Animations.FSM.Npc.Machines;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights.DamageInfo;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Stats;
using Awaken.TG.Main.Heroes.Stats.Tweaks;
using Awaken.TG.Main.Settings.Gameplay;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGCommunity.Example.CombatPressurePoise;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.combat-pressure-poise";
    public const string PluginName = "TG Example - Combat Pressure and Poise";
    public const string PluginVersion = "0.1.0";

    private static ConfigEntry<float>? _slotMultiplier;
    private static ConfigEntry<float>? _recoveryMultiplier;
    private static ConfigEntry<float>? _staminaMultiplier;
    private static ConfigEntry<float>? _poiseMultiplier;
    private Harmony? _harmony;

    private void Awake()
    {
        _slotMultiplier = Config.Bind("Pressure", "EnemyAttackSlotsMultiplier", 1f, "Scale Difficulty.MaxEnemiesAttacking.");
        _recoveryMultiplier = Config.Bind("Pressure", "AttackRecoveryMultiplier", 1f, "Scale Difficulty.AttackActionUnBookProlong. Lower values turn slots over faster.");
        _staminaMultiplier = Config.Bind("Pressure", "ActionStaminaMultiplier", 1f, "Scale the hero's negative stamina-use multiplier through a runtime StatTweak.");
        _poiseMultiplier = Config.Bind("Poise", "PlayerPoiseDamageMultiplier", 1f, "Scale hero-originated Damage.Parameters.PoiseDamage only while NpcGeneralFSM.OnDamageTaken runs.");

        _harmony = new Harmony(PluginGuid);
        PatchDifficulty(_harmony);
        PatchCharacterStats(_harmony);
        PatchPoise(_harmony);

        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        RemoveStaminaTweak();
    }

    private static float Slots => Mathf.Clamp(_slotMultiplier?.Value ?? 1f, 0.25f, 4f);
    private static float Recovery => Mathf.Clamp(_recoveryMultiplier?.Value ?? 1f, 0.1f, 4f);
    private static float Stamina => Mathf.Clamp(_staminaMultiplier?.Value ?? 1f, 0.25f, 4f);
    private static float Poise => Mathf.Clamp(_poiseMultiplier?.Value ?? 1f, 0f, 5f);

    private void PatchDifficulty(Harmony harmony)
    {
        PatchGetter(harmony, nameof(Difficulty.MaxEnemiesAttacking), nameof(MaxEnemiesPostfix));
        PatchGetter(harmony, nameof(Difficulty.AttackActionUnBookProlong), nameof(RecoveryPostfix));
    }

    private void PatchGetter(Harmony harmony, string propertyName, string postfixName)
    {
        var target = AccessTools.PropertyGetter(typeof(Difficulty), propertyName);
        var postfix = AccessTools.Method(typeof(Plugin), postfixName);
        if (target == null || postfix == null)
        {
            Logger.LogWarning($"Difficulty.{propertyName} was not found.");
            return;
        }

        harmony.Patch(target, postfix: new HarmonyMethod(postfix));
    }

    private static void MaxEnemiesPostfix(ref int __result)
    {
        float scaled = __result * Slots;
        __result = Mathf.Clamp(Slots > 1f ? Mathf.CeilToInt(scaled) : Mathf.FloorToInt(scaled), 1, 8);
    }

    private static void RecoveryPostfix(ref float __result)
    {
        __result = Mathf.Max(0f, __result * Recovery);
    }

    private void PatchCharacterStats(Harmony harmony)
    {
        var target = AccessTools.Method(
            typeof(CharacterStats.CharacterStatsWrapper),
            nameof(CharacterStats.CharacterStatsWrapper.Initialize));
        var postfix = AccessTools.Method(typeof(Plugin), nameof(CharacterStatsPostfix));

        if (target == null || postfix == null)
        {
            Logger.LogWarning("CharacterStatsWrapper.Initialize was not found.");
            return;
        }

        harmony.Patch(target, postfix: new HarmonyMethod(postfix));
    }

    private static void CharacterStatsPostfix(CharacterStats stats)
    {
        if (stats == null || stats.ParentModel is not Hero hero || hero.HasBeenDiscarded)
        {
            return;
        }

        hero.TryGetElement<ActionStaminaTweak>()?.Discard();

        if (Math.Abs(Stamina - 1f) <= 0.0001f)
        {
            return;
        }

        hero.AddElement(new ActionStaminaTweak(stats.StaminaUsageMultiplier, Stamina));
    }

    private static void RemoveStaminaTweak()
    {
        Hero? hero = Hero.Current;
        if (hero != null && !hero.HasBeenDiscarded)
        {
            hero.TryGetElement<ActionStaminaTweak>()?.Discard();
        }
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

    private void PatchPoise(Harmony harmony)
    {
        var target = AccessTools.Method(
            typeof(NpcGeneralFSM),
            "OnDamageTaken",
            new[] { typeof(DamageOutcome) });
        var prefix = AccessTools.Method(typeof(Plugin), nameof(PoisePrefix));
        var postfix = AccessTools.Method(typeof(Plugin), nameof(PoisePostfix));

        if (target == null || prefix == null || postfix == null)
        {
            Logger.LogWarning("NpcGeneralFSM.OnDamageTaken(DamageOutcome) was not found.");
            return;
        }

        harmony.Patch(target, prefix: new HarmonyMethod(prefix), postfix: new HarmonyMethod(postfix));
    }

    private static void PoisePrefix(DamageOutcome damageOutcome, ref PoiseState? __state)
    {
        __state = null;
        Damage? damage = damageOutcome.Damage;

        if (damage == null ||
            damage.PoiseDamage <= 0f ||
            damageOutcome.AttackerPure != Hero.Current ||
            Math.Abs(Poise - 1f) <= 0.0001f)
        {
            return;
        }

        DamageParameters parameters = damage.Parameters;
        float original = parameters.PoiseDamage;
        parameters.PoiseDamage = Mathf.Max(0f, original * Poise);
        damage.Parameters = parameters;
        __state = new PoiseState(damage, original);
    }

    private static void PoisePostfix(PoiseState? __state)
    {
        if (__state == null)
        {
            return;
        }

        DamageParameters parameters = __state.Damage.Parameters;
        parameters.PoiseDamage = __state.Original;
        __state.Damage.Parameters = parameters;
    }

    private sealed class PoiseState
    {
        internal PoiseState(Damage damage, float original)
        {
            Damage = damage;
            Original = original;
        }

        internal Damage Damage { get; }
        internal float Original { get; }
    }
}
