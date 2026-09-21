using System;
using System.Linq;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Locations;
using Awaken.TG.MVC;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DragonKnight;

internal sealed class DragonKnightOwnedPlacementObserver
{
    private readonly ManualLogSource _logger;
    private readonly ConfigEntry<bool> _observeOwnedPlacements;
    private float _nextScanTime;
    private string _lastObservedActorId = string.Empty;
    private bool _waitingLogged;

    internal DragonKnightOwnedPlacementObserver(ManualLogSource logger, ConfigFile config)
    {
        _logger = logger;
        _observeOwnedPlacements = config.Bind(
            DragonKnightOwnedPlacementContract.ConfigSection,
            "ObserveOwnedPlacements",
            DragonKnightOwnedPlacementContract.DefaultObserveOwnedPlacements,
            "Read-only observer for Dragon Knight-owned CampaignMap_HOS placements produced by the native LocationSpawner candidate route.");

        _logger.LogInfo(
            "DRAGON_KNIGHT_OWNED_PLACEMENT_CONFIG"
            + " observe-owned-placements=" + (_observeOwnedPlacements.Value ? "1" : "0")
            + " owner=" + DragonKnightOwnedPlacementContract.OwnerId
            + " actor-source=" + DragonKnightOwnedPlacementContract.ActorSource
            + " scene=" + DragonKnightOwnedPlacementContract.SceneName
            + " placements=" + DragonKnightOwnedPlacementContract.Placements.Count
            + " native-spawner=1 native-candidate=1 direct-spawn=0 save-write=0");
    }

    internal void Tick()
    {
        if (!_observeOwnedPlacements.Value || Time.time < _nextScanTime)
        {
            return;
        }

        _nextScanTime = Time.time + 1f;

        Scene scene = SceneManager.GetSceneByName(DragonKnightOwnedPlacementContract.SceneName);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return;
        }

        if (!TryScan(out DragonKnightOwnedPlacementObservation observation, out string reason))
        {
            LogWaitingOnce(reason);
            return;
        }

        if (string.Equals(_lastObservedActorId, observation.ActorId, StringComparison.Ordinal))
        {
            return;
        }

        _lastObservedActorId = observation.ActorId;
        _logger.LogWarning(DragonKnightOwnedPlacementMarker.BuildObservedMarker(observation));
    }

    private bool TryScan(out DragonKnightOwnedPlacementObservation observation, out string reason)
    {
        observation = default;

        Location[] locations;
        try
        {
            locations = World.All<Location>().ToArraySlow();
        }
        catch (Exception ex)
        {
            reason = "World.All<Location>() failed: " + ex.GetType().Name + ":" + ex.Message;
            return false;
        }

        foreach (DragonKnightOwnedNpcPlacement placement in DragonKnightOwnedPlacementContract.Placements)
        {
            if (!TryFindPlacement(locations, placement, out observation, out reason))
            {
                continue;
            }

            reason = string.Empty;
            return true;
        }

        reason = "no Dragon Knight-owned placement actor observed in " + DragonKnightOwnedPlacementContract.SceneName;
        return false;
    }

    private static bool TryFindPlacement(
        Location[] locations,
        DragonKnightOwnedNpcPlacement placement,
        out DragonKnightOwnedPlacementObservation observation,
        out string reason)
    {
        observation = default;
        Location? match = null;
        int matches = 0;

        foreach (Location location in locations)
        {
            if (location == null || location.HasBeenDiscarded)
            {
                continue;
            }

            if (!string.Equals(location.Template?.GUID ?? string.Empty, placement.LocationTemplateGuid, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.Equals(location.Template?.name ?? string.Empty, placement.LocationTemplateName, StringComparison.Ordinal))
            {
                continue;
            }

            if (Vector3.Distance(location.Coords, placement.Position) > placement.MaxPlacementDistanceMeters)
            {
                continue;
            }

            match = location;
            matches++;
        }

        if (matches <= 0 || match == null)
        {
            reason = "missing " + placement.PlacementId;
            return false;
        }

        if (matches > 1)
        {
            reason = "duplicate " + placement.PlacementId;
            return false;
        }

        if (string.IsNullOrWhiteSpace(match.ID))
        {
            reason = "missing Location.ID for " + placement.PlacementId;
            return false;
        }

        if (match.MarkedNotSaved || match.IsNotSaved)
        {
            reason = "placement is temporary/no-save for " + placement.PlacementId;
            return false;
        }

        if (!match.TryGetElement(out NpcElement npc) || npc == null || npc.HasBeenDiscarded)
        {
            reason = "missing live NpcElement for " + placement.PlacementId;
            return false;
        }

        string npcTemplateGuid = npc.Template?.GUID ?? string.Empty;
        if (!string.Equals(npcTemplateGuid, placement.NpcTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = "NPC template mismatch for " + placement.PlacementId;
            return false;
        }

        observation = new DragonKnightOwnedPlacementObservation(
            placement,
            match.ID,
            "foa.location:" + match.ID,
            match.DisplayName ?? string.Empty,
            match.Coords,
            match.MarkedNotSaved,
            match.IsNotSaved);
        reason = string.Empty;
        return true;
    }

    private void LogWaitingOnce(string reason)
    {
        if (_waitingLogged)
        {
            return;
        }

        _waitingLogged = true;
        _logger.LogInfo(
            "DRAGON_KNIGHT_OWNED_PLACEMENT_WAITING"
            + " owner=" + DragonKnightOwnedPlacementContract.OwnerId
            + " actor-source=" + DragonKnightOwnedPlacementContract.ActorSource
            + " scene=" + DragonKnightOwnedPlacementContract.SceneName
            + " placements=" + DragonKnightOwnedPlacementContract.Placements.Count
            + " reason=" + DragonKnightDk4Marker.MarkerValue(reason)
            + " native-spawner=1 native-candidate=1 direct-spawn=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save-write=0");
    }
}

internal readonly struct DragonKnightOwnedPlacementObservation
{
    internal DragonKnightOwnedPlacementObservation(
        DragonKnightOwnedNpcPlacement placement,
        string locationId,
        string actorId,
        string displayName,
        Vector3 position,
        bool markedNotSaved,
        bool isNotSaved)
    {
        Placement = placement;
        LocationId = locationId;
        ActorId = actorId;
        DisplayName = displayName;
        Position = position;
        MarkedNotSaved = markedNotSaved;
        IsNotSaved = isNotSaved;
    }

    internal DragonKnightOwnedNpcPlacement Placement { get; }

    internal string LocationId { get; }

    internal string ActorId { get; }

    internal string DisplayName { get; }

    internal Vector3 Position { get; }

    internal bool MarkedNotSaved { get; }

    internal bool IsNotSaved { get; }
}
