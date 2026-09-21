namespace DragonKnight;

internal static class DragonKnightOwnedPlacementMarker
{
    internal const string SourceGateMarkerLine =
        "DRAGON_KNIGHT_OWNED_PLACEMENT_SOURCE_GATE_PASS fixtures=32 owner=dragon-knight.boss.host actor-source=native-location-spawner-candidate:hos-cromlech-grindylow01-native-slot scene=CampaignMap_HOS placements=1 multi-npc-ready=1 altar-ref=1 boss-root=1 altar-trigger=0 native-spawner=1 native-candidate=1 direct-spawn=0 relocated-to-boss-root=1 goals=0 actions=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save-write=0";

    internal const string ObservedMarker = "DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED";

    internal static string BuildObservedMarker(DragonKnightOwnedPlacementObservation observation)
    {
        DragonKnightOwnedNpcPlacement placement = observation.Placement;
        return ObservedMarker
            + " owner=" + DragonKnightOwnedPlacementContract.OwnerId
            + " encounter=" + MarkerValue(placement.EncounterId)
            + " placement=" + MarkerValue(placement.PlacementId)
            + " actor-role=" + MarkerValue(placement.ActorRole)
            + " actor-source=" + DragonKnightOwnedPlacementContract.ActorSource
            + " actor-location-id=" + MarkerValue(observation.LocationId)
            + " actor-id=" + MarkerValue(observation.ActorId)
            + " scene=" + DragonKnightOwnedPlacementContract.SceneName
            + " location-template=" + MarkerValue(placement.LocationTemplateGuid)
            + " npc-template=" + MarkerValue(placement.NpcTemplateGuid)
            + " display-name=" + MarkerValue(string.IsNullOrWhiteSpace(observation.DisplayName) ? placement.DisplayName : observation.DisplayName)
            + " position=" + FormatVector(observation.Position)
            + " markedNotSaved=" + BoolValue(observation.MarkedNotSaved)
            + " isNotSaved=" + BoolValue(observation.IsNotSaved)
            + " activation-trigger=" + DragonKnightOwnedPlacementContract.ActivationTrigger
            + " outer-wake-m=" + DragonKnightOwnedPlacementContract.OuterWakeRadiusMeters
            + " inner-fight-m=" + DragonKnightOwnedPlacementContract.InnerFightStartRadiusMeters
            + " soft-leash-m=" + DragonKnightOwnedPlacementContract.SoftLeashRadiusMeters
            + " hard-leash-m=" + DragonKnightOwnedPlacementContract.HardLeashRadiusMeters
            + " custom-owned=1 native-spawner=1 native-candidate=1 direct-spawn=0 relocated-to-boss-root=1 save-owned=1"
            + " goals=0 actions=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save-write=0";
    }

    private static string MarkerValue(string value)
    {
        return DragonKnightDk4Marker.MarkerValue(value);
    }

    private static string FormatVector(UnityEngine.Vector3 value)
    {
        return $"{value.x:0.###}|{value.y:0.###}|{value.z:0.###}";
    }

    private static string BoolValue(bool value) => value ? "1" : "0";
}
