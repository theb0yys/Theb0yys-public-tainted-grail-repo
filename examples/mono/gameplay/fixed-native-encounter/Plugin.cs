using System;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Setup;
using Awaken.TG.Main.Locations.Spawners;
using Awaken.TG.Main.Templates;
using BepInEx;
using UnityEngine;

namespace TGCommunity.Example.FixedNativeEncounter;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.fixed-native-encounter";
    public const string PluginName = "TG Example - Fixed Native Encounter";
    public const string PluginVersion = "0.1.0";

    private const string TemplateGuid = "843643575fa01ba4292e60afb9291fea";
    private const string TemplateName = "Spec_EnemyMonster_T1_Wyrdspirit";

    private Location? _activeLocation;
    private NpcElement? _activeNpc;
    private float _cooldownUntil;
    private bool _deathLogged;

    private void Awake()
    {
        Logger.LogInfo($"{PluginName} loaded. F8 starts one session-only exact Wyrdspirit encounter; F9 cancels the exact owned actor.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            TryStart();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            CancelOwned("player-cancel");
        }

        if (_activeLocation == null)
        {
            return;
        }

        if (_activeLocation.HasBeenDiscarded)
        {
            Logger.LogInfo("Encounter actor Location was discarded; encounter bookkeeping cleared.");
            _activeLocation = null;
            _activeNpc = null;
            _cooldownUntil = Time.unscaledTime + 10f;
            return;
        }

        if (_activeNpc != null && !_activeNpc.IsAlive && !_deathLogged)
        {
            _deathLogged = true;
            _cooldownUntil = Time.unscaledTime + 10f;
            Logger.LogInfo("Native death observed for the exact Wyrdspirit. Encounter resolved; cooldown started.");
        }
    }

    private void OnDestroy()
    {
        CancelOwned("plugin-unload");
    }

    private void TryStart()
    {
        if (_activeLocation != null && !_activeLocation.HasBeenDiscarded)
        {
            Logger.LogInfo("Encounter blocked: an owned encounter is already active.");
            return;
        }

        if (Time.unscaledTime < _cooldownUntil)
        {
            Logger.LogInfo("Encounter blocked: cooldown is still active.");
            return;
        }

        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded || !hero.IsAlive || !hero.MainViewInitialized)
        {
            Logger.LogWarning("Encounter blocked: playable hero is not ready.");
            return;
        }

        LocationTemplate? template;
        try
        {
            template = new TemplateReference(TemplateGuid).Get<LocationTemplate>();
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Encounter blocked: template lookup failed ({ex.GetType().Name}).");
            return;
        }

        if (template == null ||
            !string.Equals(template.GUID, TemplateGuid, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(template.name, TemplateName, StringComparison.Ordinal))
        {
            Logger.LogWarning("Encounter blocked: exact Wyrdspirit template did not resolve.");
            return;
        }

        try
        {
            Vector3 requested = hero.Coords + hero.Rotation * (Vector3.forward * 8f);
            Vector3 verified = BaseLocationSpawner.VerifyPosition(requested, template);
            Location location = template.SpawnLocation(verified, hero.Rotation);

            // This public teaching example is deliberately session-only.
            location.MarkedNotSaved = true;
            _activeLocation = location;
            _activeNpc = null;
            _deathLogged = false;

            location.AfterFullyInitialized(OnLocationInitialized);
            Logger.LogInfo($"Encounter started. template={TemplateName}[{TemplateGuid}]; location={location.ID}; requested={requested}; verified={verified}; MarkedNotSaved=true.");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Encounter start failed: {ex.GetType().Name}: {ex.Message}");
            CancelOwned("start-failure");
        }
    }

    private void OnLocationInitialized()
    {
        Location? location = _activeLocation;
        Hero? hero = Hero.Current;

        if (location == null || location.HasBeenDiscarded || hero == null || hero.HasBeenDiscarded)
        {
            CancelOwned("initialization-owner-unavailable");
            return;
        }

        if (!location.TryGetElement(out NpcElement npc) || npc == null)
        {
            Logger.LogWarning("Encounter initialization failed: spawned Location has no NpcElement.");
            CancelOwned("missing-npc");
            return;
        }

        if (!npc.IsAlive || npc.IsSummonOrAlly || !npc.IsHostileToHero() || npc.NpcAI == null || !npc.CanEnterCombat(false))
        {
            Logger.LogWarning("Encounter initialization failed: exact actor did not satisfy the hostile/native-combat gate.");
            CancelOwned("combat-gate-failed");
            return;
        }

        _activeNpc = npc;
        location.MarkedNotSaved = true;
        npc.NpcAI.EnterCombatWith(hero);
        Logger.LogInfo("Exact Wyrdspirit initialized and native NpcAI.EnterCombatWith(Hero.Current) was requested.");
    }

    private void CancelOwned(string reason)
    {
        Location? owned = _activeLocation;
        _activeLocation = null;
        _activeNpc = null;

        if (owned != null && !owned.HasBeenDiscarded)
        {
            owned.MarkedNotSaved = true;
            owned.Discard();
            Logger.LogInfo($"Exact owned encounter actor discarded. reason={reason}");
        }
    }
}
