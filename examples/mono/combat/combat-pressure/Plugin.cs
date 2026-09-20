using System;
using System.Collections.Generic;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Stats;
using Awaken.TG.Main.Heroes.Stats.Tweaks;
using Awaken.TG.Main.Settings.Gameplay;
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

    private ConfigEntry<float> _attackSlots = null!;
    private ConfigEntry<float> _attackRecovery = null!;
    private ConfigEntry<float> _actionStamina = null!;
    private Harmony? _harmony;

    private void Awake()
    {
        _attackSlots = Config.Bind("Pressure", "EnemyAttackSlotsMultiplier", 1.25f, "Scale Difficulty.MaxEnemiesAttacking.");
        _attackRecovery = Config.Bind("Pressure", "EnemyAttackRecoveryMultiplier", 0.85f, "Scale Difficulty.AttackActionUnBookProlong.");
        _actionStamina = Config.Bind("Pressure", "ActionStaminaCostMultiplier", 1.15f, "Scale hero StaminaUsageMultiplier.");

        _harmony = new Harmony(PluginGuid);
        PatchGetter(nameof(Difficulty.MaxEnemiesAttacking), nameof(MaxEnemiesPostfix));
        PatchGetter(nameof(Difficulty.AttackActionUnBookProlong), nameof(RecoveryPostfix));

        var init = AccessTools.Method(typeof(CharacterStats.CharacterStatsWrapper), nameof(CharacterStats.CharacterStatsWrapper.Initialize));
        var postfix = AccessTools.Method(typeof(Plugin), nameof(CharacterStatsPostfix));
        if (init != null && postfix != null)
        {
            _harmony.Patch(init, postfix: new HarmonyMethod(postfix));
        }

        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        RemoveCurrentTweak();
        _harmony?.UnpatchSelf();
    }

    private void PatchGetter(string propertyName, string postfixName)
    {
        var target = AccessTools.PropertyGetter(typeof(Difficulty), propertyName);
        var postfix = AccessTools.Method(typeof(Plugin), postfixName);
        if (target == null || postfix == null)
        {
            Logger.LogWarning($"Skipped Difficulty.{propertyName}; getter or postfix was not found.");
            return;
        }

        _harmony!.Patch(target, postfix: new HarmonyMethod(postfix));
    }

    private static void MaxEnemiesPostfix(ref int __result)
    {
        Plugin? plugin = Find();
        if (plugin == null)
        {
            return;
        }

        float multiplier = Clamp(plugin._attackSlots.Value, 0.5f, 3f);
        float scaled = __result * multiplier;
        int rounded = multiplier > 1f ? Mathf.CeilToInt(scaled) : Mathf.FloorToInt(scaled);
        __result = Mathf.Clamp(rounded, 1, 12);
    }

    private static void RecoveryPostfix(ref float __result)
    {
        Plugin? plugin = Find();
        if (plugin == null)
        {
            return;
        }

        __result = Mathf.Max(0f, __result * Clamp(plugin._attackRecovery.Value, 0.1f, 3f));
    }

    private static void CharacterStatsPostfix(CharacterStats stats)
    {
        Plugin? plugin = Find();
        plugin?.ApplyStaminaTweak(stats);
    }

    private void ApplyStaminaTweak(CharacterStats? stats)
    {
        if (stats == null || stats.ParentModel is not Hero hero || hero.HasBeenDiscarded)
        {
            return;
        }

        RemoveTweaks(hero);

        float multiplier = Clamp(_actionStamina.Value, 0.5f, 3f);
        if (Math.Abs(multiplier - 1f) <= 0.0001f)
        {
            return;
        }

        hero.AddElement(new ActionStaminaTweak(stats.StaminaUsageMultiplier, multiplier));
        Logger.LogInfo($"Applied runtime action-stamina multiplier {multiplier:0.###}.");
    }

    private void RemoveCurrentTweak()
    {
        Hero? hero = Hero.Current;
        if (hero != null && !hero.HasBeenDiscarded)
        {
            RemoveTweaks(hero);
        }
    }

    private static void RemoveTweaks(Hero hero)
    {
        var owned = new List<ActionStaminaTweak>();
        foreach (ActionStaminaTweak tweak in hero.Elements<ActionStaminaTweak>())
        {
            owned.Add(tweak);
        }

        foreach (ActionStaminaTweak tweak in owned)
        {
            tweak.Discard();
        }
    }

    private static Plugin? Find()
    {
        return UnityEngine.Object.FindObjectOfType<Plugin>();
    }

    private static float Clamp(float value, float min, float max)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            return 1f;
        }

        return Mathf.Clamp(value, min, max);
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
