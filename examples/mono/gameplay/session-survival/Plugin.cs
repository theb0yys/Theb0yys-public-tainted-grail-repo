using System;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.General.StatTypes;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Resting;
using Awaken.TG.Main.Heroes.Stats;
using Awaken.TG.Main.Heroes.Stats.Controls;
using Awaken.TG.Main.Heroes.Stats.Observers;
using Awaken.TG.Main.Heroes.Stats.Tweaks;
using Awaken.TG.Main.Timing;
using Awaken.TG.MVC;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace TGCommunity.Example.SessionSurvival;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.session-survival";
    public const string PluginName = "TG Example - Session Survival";
    public const string PluginVersion = "0.1.0";

    private static float _fatigue;
    private static FatigueStage _stage;
    private Harmony? _harmony;

    private void Awake()
    {
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded. Fatigue is session-only.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        _fatigue = 0f;
        _stage = FatigueStage.Rested;
        ReapplyEffects(forceNeutral: true);
    }

    private void OnGUI()
    {
        if (Hero.Current == null)
        {
            return;
        }

        GUI.Box(new Rect(24f, 80f, 260f, 58f), $"Fatigue: {_fatigue:0}/100\nStage: {_stage}");
    }

    private static void AddFatigue(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        SetFatigue(_fatigue + amount);
    }

    private static void Recover(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        SetFatigue(_fatigue - amount);
    }

    private static void SetFatigue(float value)
    {
        FatigueStage before = _stage;
        _fatigue = Mathf.Clamp(value, 0f, 100f);
        _stage = StageFor(_fatigue);

        if (_stage != before)
        {
            ReapplyEffects(forceNeutral: false);
        }
    }

    private static FatigueStage StageFor(float fatigue)
    {
        if (fatigue < 25f) return FatigueStage.Rested;
        if (fatigue < 50f) return FatigueStage.Tired;
        if (fatigue < 75f) return FatigueStage.Exhausted;
        return FatigueStage.Depleted;
    }

    private static float StaminaMultiplier()
    {
        return _stage switch
        {
            FatigueStage.Tired => 1.10f,
            FatigueStage.Exhausted => 1.25f,
            FatigueStage.Depleted => 1.45f,
            _ => 1f
        };
    }

    private static void ReapplyEffects(bool forceNeutral)
    {
        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            return;
        }

        ApplyToCharacterStats(hero.CharacterStats, forceNeutral);
    }

    private static void ApplyToCharacterStats(CharacterStats? stats, bool forceNeutral)
    {
        if (stats == null || stats.ParentModel is not Hero hero || hero.HasBeenDiscarded)
        {
            return;
        }

        hero.TryGetElement<FatigueStaminaTweak>()?.Discard();
        hero.TryGetElement<FatigueSprintTweak>()?.Discard();

        float multiplier = forceNeutral ? 1f : StaminaMultiplier();
        if (Math.Abs(multiplier - 1f) <= 0.0001f)
        {
            return;
        }

        hero.AddElement(new FatigueStaminaTweak(stats.StaminaUsageMultiplier, multiplier));
        hero.AddElement(new FatigueSprintTweak(stats.SprintCostMultiplier, multiplier));
    }

    [HarmonyPatch(typeof(ProficiencyEventListener), "XPGainEvent")]
    private static class MovementPatch
    {
        private static void Prefix(
            ProfStatType proficiencyToLevel,
            BaseXPType targetXPType,
            float xpGainParameter)
        {
            string source = targetXPType?.EnumName ?? string.Empty;
            if (!IsMovementSource(source))
            {
                return;
            }

            try
            {
                if (World.Any<ProficiencyGainBlockerModel>() != null)
                {
                    return;
                }
            }
            catch
            {
            }

            float pressure = Mathf.Clamp(Mathf.Abs(xpGainParameter), 0f, 10f);
            AddFatigue(0.05f + pressure * 0.02f);
        }
    }

    [HarmonyPatch(typeof(RestPopupUI), "SkipWeatherTime")]
    private static class RestPatch
    {
        private static void Postfix(
            Hero hero,
            GameRealTime gameRealTime,
            float hourValue,
            bool isSafelyResting)
        {
            if (hero == null || !isSafelyResting)
            {
                return;
            }

            float minutes = Mathf.Max(0f, hourValue * 60f);
            Recover(minutes * 0.20f);
        }
    }

    [HarmonyPatch(
        typeof(CharacterStats.CharacterStatsWrapper),
        nameof(CharacterStats.CharacterStatsWrapper.Initialize))]
    private static class CharacterStatsPatch
    {
        private static void Postfix(CharacterStats stats)
        {
            ApplyToCharacterStats(stats, forceNeutral: false);
        }
    }

    private static bool IsMovementSource(string source)
    {
        return source.Equals("Walk", StringComparison.OrdinalIgnoreCase) ||
               source.Equals("Sprint", StringComparison.OrdinalIgnoreCase) ||
               source.Equals("Dash", StringComparison.OrdinalIgnoreCase) ||
               source.Equals("Slide", StringComparison.OrdinalIgnoreCase) ||
               source.Equals("Jump", StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FatigueStaminaTweak : StatTweak
    {
        public override bool IsNotSaved => true;

        internal FatigueStaminaTweak(Stat stat, float multiplier)
            : base(stat, multiplier, TweakPriority.Multiply, OperationType.Multi)
        {
            MarkedNotSaved = true;
        }
    }

    private sealed class FatigueSprintTweak : StatTweak
    {
        public override bool IsNotSaved => true;

        internal FatigueSprintTweak(Stat stat, float multiplier)
            : base(stat, multiplier, TweakPriority.Multiply, OperationType.Multi)
        {
            MarkedNotSaved = true;
        }
    }

    private enum FatigueStage
    {
        Rested,
        Tired,
        Exhausted,
        Depleted
    }
}
