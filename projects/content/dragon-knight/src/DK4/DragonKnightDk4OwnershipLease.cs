using System;

namespace DragonKnight;

internal sealed class DragonKnightDk4OwnershipLease
{
    private DragonKnightDk4OwnershipLease(string actorLocationId, string targetLocationId, string leaseId)
    {
        ActorLocationId = actorLocationId;
        TargetLocationId = targetLocationId;
        LeaseId = leaseId;
        OwnerId = DragonKnightDk4DiagnosticOptions.OwnerId;
        AcquiredUtc = DateTimeOffset.UtcNow;
    }

    internal string ActorLocationId { get; }

    internal string TargetLocationId { get; }

    internal string LeaseId { get; }

    internal string OwnerId { get; }

    internal DateTimeOffset AcquiredUtc { get; }

    internal bool Released { get; private set; }

    internal static bool TryAcquire(
        string actorLocationId,
        string targetLocationId,
        out DragonKnightDk4OwnershipLease? lease,
        out string reason)
    {
        lease = null;

        if (string.IsNullOrWhiteSpace(actorLocationId))
        {
            reason = "missing actor Location.ID";
            return false;
        }

        if (string.IsNullOrWhiteSpace(targetLocationId))
        {
            reason = "missing target Location.ID";
            return false;
        }

        string leaseId =
            "dragon-knight.dk4.lease."
            + DragonKnightDk4Marker.MarkerValue(actorLocationId)
            + "."
            + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        lease = new DragonKnightDk4OwnershipLease(actorLocationId, targetLocationId, leaseId);
        reason = string.Empty;
        return true;
    }

    internal bool IsCurrentFor(string actorLocationId)
    {
        return !Released && string.Equals(ActorLocationId, actorLocationId, StringComparison.Ordinal);
    }

    internal void Release()
    {
        Released = true;
    }
}
