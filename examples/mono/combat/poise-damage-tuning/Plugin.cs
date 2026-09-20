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
    private Harmony? _harmony;

    private void Awake()
    {
        _multiplier = Config.Bind("Poise", "PlayerPoiseDamageMultiplier", 1.25f, "Scale player-originated poise damage.");

        var target = AccessTools.Method(typeof(NpcGeneralFSM), "OnDamageTaken", new[] { typeof(DamageOutcome) });
        if (target == null)
        {
            Logger.LogError("NpcGeneralFSM.OnDamageTaken(DamageOutcome) not found.");
            return;
        }

        _harmony = new Harmony(PluginGuid);
        _harmony.Patch(target,
            prefix: new HarmonyMethod(typeof(Plugin), nameof(Prefix)),
            postfix: new HarmonyMethod(typeof(Plugin), nameof(Postfix)));

        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy() => _harmony?.UnpatchSelf();

    private static void Prefix(DamageOutcome damageOutcome, ref State? __state)
    {
        __state = null;
        Damage damage = damageOutcome.Damage;
        if (damage == null || damage.PoiseDamage <= 0f || damageOutcome.AttackerPure != Hero.Current)
            return;

        float multiplier = Mathf.Clamp(_multiplier?.Value ?? 1f, 0f, 5f);
        if (Mathf.Abs(multiplier - 1f) <= 0.0001f)
            return;

        DamageParameters parameters = damage.Parameters;
        float original = parameters.PoiseDamage;
        parameters.PoiseDamage = Mathf.Max(0f, original * multiplier);
        damage.Parameters = parameters;
        __state = new State(damage, original);
    }

    private static void Postfix(State? __state)
    {
        if (__state == null) return;

        DamageParameters parameters = __state.Damage.Parameters;
        parameters.PoiseDamage = __state.OriginalPoiseDamage;
        __state.Damage.Parameters = parameters;
    }

    private sealed class State
    {
        internal State(Damage damage, float originalPoiseDamage)
        {
            Damage = damage;
            OriginalPoiseDamage = originalPoiseDamage;
        }

        internal Damage Damage { get; }
        internal float OriginalPoiseDamage { get; }
    }
}
