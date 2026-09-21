namespace DragonKnight;

internal static class DragonKnightDk4Marker
{
    internal const string BlockedMarkerLine =
        "DRAGON_KNIGHT_DK4_SOURCE_GATE_BLOCKED fixtures=36 owner=dragon-knight.dk4.diagnostic-host actor-role=dragon-knight.boss actor-source=blocked actor-location-id=blocked actor-id=blocked target-source=blocked target-location-id=blocked activation=F7 release=F6 default-off=1 kill-switch-default=1 native-spawn=0 goals=0 actions=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save=0";

    internal const string PassMarker = "DRAGON_KNIGHT_DK4_LIVE_ACTOR_OBSERVATION_PASS";

    internal static string BuildPassMarker(
        DragonKnightDk4ActorObservation actor,
        DragonKnightDk4TargetObservation target,
        DragonKnightDk4OwnershipLease lease)
    {
        return PassMarker
            + " fixtures=36"
            + " owner=" + DragonKnightDk4DiagnosticOptions.OwnerId
            + " actor-role=" + DragonKnightDk4DiagnosticOptions.ActorRole
            + " actor-source=" + DragonKnightDk4DiagnosticOptions.ActorSource
            + " actor-location-id=" + MarkerValue(actor.LocationId)
            + " actor-id=" + MarkerValue(actor.ActorId)
            + " target-source=" + MarkerValue(target.Source)
            + " target-location-id=" + MarkerValue(target.LocationId)
            + " target-id=" + MarkerValue(target.TargetId)
            + " lease=" + MarkerValue(lease.LeaseId)
            + " activation=F7 release=F6 default-off=1 kill-switch=0 cleanup=pass native-spawn=1"
            + " goals=0 actions=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save=0";
    }

    internal static string BuildReleaseMarker(
        string reason,
        DragonKnightDk4OwnershipLease? lease,
        bool markedNotSaved,
        bool isNotSaved,
        bool locationDiscarded,
        bool npcDiscarded)
    {
        return "DRAGON_KNIGHT_DK4_RELEASE_PASS"
            + " reason=" + MarkerValue(reason)
            + " owner=" + DragonKnightDk4DiagnosticOptions.OwnerId
            + " lease=" + MarkerValue(lease?.LeaseId ?? "none")
            + " markedNotSaved=" + BoolValue(markedNotSaved)
            + " isNotSaved=" + BoolValue(isNotSaved)
            + " locationDiscarded=" + BoolValue(locationDiscarded)
            + " npcDiscarded=" + BoolValue(npcDiscarded)
            + " cleanup=pass save=0";
    }

    internal static string MarkerValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "blocked";
        }

        return value
            .Replace(' ', '_')
            .Replace(',', '_')
            .Replace(';', '_')
            .Replace('|', '_');
    }

    private static string BoolValue(bool value) => value ? "1" : "0";
}
