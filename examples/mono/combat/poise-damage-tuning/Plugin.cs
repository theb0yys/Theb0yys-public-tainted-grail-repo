using System;
using Awaken.TG.Main.Animations.FSM.Npc.Machines;
using Awaken.TG.Main.Fights.DamageInfo;
using Awaken.TG.Main.Heroes;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGCommunity.Example.PoiseDamageTuning;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.poise-damage-tuning";
    public const string PluginName = "TG Example - Poise Damage Tuning";
    public const string PluginVersion = "0.1.0";

    private static ConfigEntry<float>? _multiplier;
    private static BepInEx.Logging.ManualLogSource? _log;
    private Harmony? _harmony;

    private void Awake()
    {
        _multiplier = Config.Bind("Poise", "PlayerPoiseDamageMultiplier", 1.25f, "Scale hero-originated poise damage only.");
        _log = Logger;

        var target = AccessTools.Method(typeof(NpcGeneralFSM), "OnDamageTaken", new[] { typeof(DamageOutcome) });
        var prefix = AccessTools.Method(typeof(Plugin), nameof(Prefix));
        var postfix = AccessTools.Method(typeof(Plugin), nameof(Postfix));

        if (target == null || prefix == null || postfix == null)
        {
            Logger.LogError("NpcGeneralFSM.OnDamageTaken could not be patched.");
            return;
        }

        _harmony = new Harmony(PluginGuid);
        _harmony.Patch(target, prefix: new HarmonyMethod(prefix), postfix: new HarmonyMethod(postfix));
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        _log = null;
    }

    private static void Prefix(DamageOutcome damageOutcome, ref PatchState? __state)
    {
        __state = null;

        Damage damage = damageOutcome.Damage;
        if (damage == null || damage.PoiseDamage <= 0f || damageOutcome.AttackerPure != Hero.Current)
        {
            return;
        }

        float multiplier = Mathf.Clamp(_multiplier?.Value ?? 1f, 0f, 5f);
        if (Math.Abs(multiplier - 1f) <= 0.0001f)
        {
            return;
        }

        DamageParameters parameters = damage.Parameters;
        float original = parameters.PoiseDamage;
        parameters.PoiseDamage = Mathf.Max(0f, original * multiplier);
        damage.Parameters = parameters;
        __state = new PatchState(damage, original);

        _log?.LogDebug($"PoiseDamage {original:0.###} -> {parameters.PoiseDamage:0.###}.");
    }

    private static void Postfix(PatchState? __state)
    {
        if (__state == null)
        {
            return;
        }

        DamageParameters parameters = __state.Damage.Parameters;
        parameters.PoiseDamage = __state.Original;
        __state.Damage.Parameters = parameters;
    }

    private sealed class PatchState
    {
        internal PatchState(Damage damage, float original)
        {
            Damage = damage;
            Original = original;
        }

        internal Damage Damage { get; }
        internal float Original { get; }
    }
}
