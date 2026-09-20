using System;
using System.Collections.Generic;
using Awaken.TG.Main.AI;
using Awaken.TG.Main.Fights.Factions;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Heroes.Stats;
using Awaken.TG.MVC;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace TGCommunity.Example.ExactNpcTuning;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.exact-npc-tuning";
    public const string PluginName = "TG Example - Exact NPC Tuning";
    public const string PluginVersion = "0.1.0";

    private const string DrownerTemplateGuid = "bb613531c5d3bf5499ea3b8103a4024e";
    private const string DrownerTemplateName = "Spec_EnemyZombie_T1_Drowner";

    private readonly Dictionary<string, TuningState> _states = new Dictionary<string, TuningState>(StringComparer.Ordinal);
    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<bool> _immediateCombat = null!;
    private ConfigEntry<float> _sightMultiplier = null!;
    private ConfigEntry<float> _meleeMultiplier = null!;
    private float _nextScanAt;

    private void Awake()
    {
        _enabled = Config.Bind("Drowner", "Enabled", true, "Enable exact plain Drowner tuning.");
        _immediateCombat = Config.Bind("Drowner", "ImmediateCombat", true, "Attach native HyperAggressiveToHero.");
        _sightMultiplier = Config.Bind("Drowner", "SightMultiplier", 1.5f, "Temporary native SightLengthMultiplier tweak.");
        _meleeMultiplier = Config.Bind("Drowner", "MeleeDamageMultiplier", 1.25f, "Temporary native MeleeDamage tweak.");
        Logger.LogInfo($"{PluginName} loaded. Exact target={DrownerTemplateName} [{DrownerTemplateGuid}].");
    }

    private void Update()
    {
        if (Time.unscaledTime < _nextScanAt)
        {
            return;
        }

        _nextScanAt = Time.unscaledTime + 1f;

        if (!_enabled.Value)
        {
            ClearAll();
            return;
        }

        try
        {
            foreach (NpcElement npc in World.All<NpcElement>())
            {
                TryAttach(npc);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"NPC tuning scan failed: {ex.GetType().Name}: {ex.Message}");
        }

        PruneDiscarded();
    }

    private void OnDestroy()
    {
        ClearAll();
    }

    private void TryAttach(NpcElement npc)
    {
        if (npc == null || npc.HasBeenDiscarded || !npc.IsAlive || npc.NpcAI == null || npc.IsSummonOrAlly)
        {
            return;
        }

        var location = npc.ParentModel;
        var template = location?.Template;

        if (template == null ||
            !string.Equals(template.GUID, DrownerTemplateGuid, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(template.name, DrownerTemplateName, StringComparison.Ordinal) ||
            !npc.IsHostileToHero())
        {
            return;
        }

        string key = location?.ID ?? string.Empty;
        if (key.Length == 0 || _states.ContainsKey(key))
        {
            return;
        }

        HyperAggressiveToHero? aggression = null;
        if (_immediateCombat.Value && !npc.HasElement<HyperAggressiveToHero>())
        {
            aggression = HyperAggressiveToHero.Add(npc, 0);
        }

        float sight = Mathf.Clamp(_sightMultiplier.Value, 1f, 2f);
        float melee = Mathf.Clamp(_meleeMultiplier.Value, 1f, 2f);

        StatTweak? sightTweak = sight > 1f
            ? StatTweak.Multi(npc.NpcStats.SightLengthMultiplier, sight, null, npc.NpcAI)
            : null;

        StatTweak? meleeTweak = melee > 1f
            ? StatTweak.Multi(npc.NpcStats.MeleeDamage, melee, null, npc.NpcAI)
            : null;

        _states[key] = new TuningState(npc, aggression, sightTweak, meleeTweak);
        Logger.LogInfo($"Exact Drowner tuning attached. location={key}; sight={sight:0.###}x; melee={melee:0.###}x; immediateCombat={_immediateCombat.Value}.");
    }

    private void PruneDiscarded()
    {
        var stale = new List<string>();

        foreach (KeyValuePair<string, TuningState> pair in _states)
        {
            if (pair.Value.Actor.HasBeenDiscarded)
            {
                stale.Add(pair.Key);
            }
        }

        foreach (string key in stale)
        {
            if (_states.TryGetValue(key, out TuningState? state))
            {
                state.Dispose();
                _states.Remove(key);
            }
        }
    }

    private void ClearAll()
    {
        foreach (TuningState state in _states.Values)
        {
            state.Dispose();
        }

        _states.Clear();
    }

    private sealed class TuningState
    {
        private readonly HyperAggressiveToHero? _aggression;
        private readonly StatTweak? _sight;
        private readonly StatTweak? _melee;

        internal TuningState(NpcElement actor, HyperAggressiveToHero? aggression, StatTweak? sight, StatTweak? melee)
        {
            Actor = actor;
            _aggression = aggression;
            _sight = sight;
            _melee = melee;
        }

        internal NpcElement Actor { get; }

        internal void Dispose()
        {
            DiscardIfAlive(_aggression);
            DiscardIfAlive(_sight);
            DiscardIfAlive(_melee);
        }

        private static void DiscardIfAlive(IModel? model)
        {
            if (model != null && !model.HasBeenDiscarded)
            {
                model.Discard();
            }
        }
    }
}
