namespace AvalonCompanions.AI.Owner.Contracts;

public enum AvalonCompanionOwnerCommand
{
    CatchUpRecall = 0,
}

public readonly struct AvalonCompanionOwnerObservation
{
    public AvalonCompanionOwnerObservation(
        long sequence,
        string actorRuntimeId,
        bool hasActiveCompanion,
        bool followCatchUpCandidate)
    {
        Sequence = sequence;
        ActorRuntimeId = actorRuntimeId ?? string.Empty;
        HasActiveCompanion = hasActiveCompanion;
        FollowCatchUpCandidate = followCatchUpCandidate;
    }

    public long Sequence { get; }

    public string ActorRuntimeId { get; }

    public bool HasActiveCompanion { get; }

    public bool FollowCatchUpCandidate { get; }
}

public interface IAvalonCompanionAiOwnerBoundary
{
    bool TryAcquire(string ownerId, out string reason);

    bool TryRelease(string ownerId);

    bool TryInspectActorId(string ownerId, out string actorRuntimeId);

    bool TryCollect(string ownerId, out AvalonCompanionOwnerObservation observation);

    bool TryDispatch(
        string ownerId,
        long observationSequence,
        AvalonCompanionOwnerCommand command);
}
