using System;
using Awaken.TG.Main.AI.Idle.Interactions.Patrols;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Setup;
using Awaken.TG.Main.Locations.Spawners;
using Awaken.TG.Main.Templates;
using BepInEx;
using UnityEngine;

namespace TGCommunity.Example.OwnedRoutePatrol;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.owned-route-patrol";
    public const string PluginName = "TG Example - Owned Route Patrol";
    public const string PluginVersion = "0.1.0";

    private const string TemplateGuid = "843643575fa01ba4292e60afb9291fea";
    private const string TemplateName = "Spec_EnemyMonster_T1_Wyrdspirit";

    private Location? _ownedLocation;
    private GameObject? _patrolHost;

    private void Awake()
    {
        Logger.LogInfo($"{PluginName} loaded. F8 creates one session-only native patrol; F9 removes it.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
            CreatePatrol();

        if (Input.GetKeyDown(KeyCode.F9))
            Cleanup("player-command");
    }

    private void OnDestroy() => Cleanup("plugin-unload");

    private void CreatePatrol()
    {
        if (_ownedLocation != null && !_ownedLocation.HasBeenDiscarded)
        {
            Logger.LogInfo("Patrol already exists.");
            return;
        }

        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            Logger.LogWarning("Hero.Current unavailable.");
            return;
        }

        LocationTemplate? template;
        try
        {
            template = new TemplateReference(TemplateGuid).Get<LocationTemplate>();
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Template lookup failed: {ex.GetType().Name}: {ex.Message}");
            return;
        }

        if (template == null ||
            !string.Equals(template.GUID, TemplateGuid, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(template.name, TemplateName, StringComparison.Ordinal))
        {
            Logger.LogWarning("Exact Wyrdspirit template did not resolve.");
            return;
        }

        Vector3 requested = hero.Coords + hero.Rotation * (Vector3.forward * 8f);
        Vector3 spawn = BaseLocationSpawner.VerifyPosition(requested, template);

        Location location = template.SpawnLocation(spawn, hero.Rotation);
        location.MarkedNotSaved = true;
        _ownedLocation = location;

        location.AfterFullyInitialized(BindNativePatrol);
        Logger.LogInfo($"Owned actor spawned. location={location.ID}; MarkedNotSaved=true.");
    }

    private void BindNativePatrol()
    {
        Location? location = _ownedLocation;
        if (location == null || location.HasBeenDiscarded)
            return;

        if (!location.TryGetElement(out NpcElement npc) || npc == null || npc.HasBeenDiscarded)
        {
            Cleanup("missing-npc");
            return;
        }

        Vector3 origin = location.Coords;
        Vector3[] points =
        {
            origin,
            origin + location.Rotation * (Vector3.forward * 7f),
            origin + location.Rotation * (Vector3.forward * 7f + Vector3.right * 5f)
        };

        GameObject host = new("TGCommunity_OwnedRoutePatrol");
        PatrolInteraction patrol = host.AddComponent<PatrolInteraction>();
        patrol.PatrolPath = new PatrolPath
        {
            type = PatrolPath.Type.TwoWay,
            waypoints = Array.ConvertAll(points, p => new PatrolWaypoint(p))
        };

        _patrolHost = host;
        npc.Behaviours.PushToStack(patrol);

        Logger.LogInfo($"Native PatrolInteraction bound. waypoints={points.Length}; type=TwoWay.");
    }

    private void Cleanup(string reason)
    {
        if (_ownedLocation != null && !_ownedLocation.HasBeenDiscarded)
        {
            _ownedLocation.MarkedNotSaved = true;
            _ownedLocation.Discard();
        }

        _ownedLocation = null;

        if (_patrolHost != null)
            Destroy(_patrolHost);

        _patrolHost = null;
        Logger.LogInfo($"Owned patrol cleaned up. reason={reason}");
    }
}
