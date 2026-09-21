using AvalonCompanions.AI.Owner.Contracts;
using AvalonCompanions.Framework;

namespace AvalonCompanions.AI.Owner.FoA;

internal interface IFoACompanionAiGameSystems
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

internal sealed class DirectFoACompanionAiGameSystems : IFoACompanionAiGameSystems
{
    public bool TryAcquire(string ownerId, out string reason)
    {
        return AvalonCompanionAiGameSystems.TryAcquireOwnership(ownerId, out reason);
    }

    public bool TryRelease(string ownerId)
    {
        return AvalonCompanionAiGameSystems.TryReleaseOwnership(ownerId);
    }

    public bool TryInspectActorId(string ownerId, out string actorRuntimeId)
    {
        return AvalonCompanionAiGameSystems.TryInspectActiveActorId(ownerId, out actorRuntimeId);
    }

    public bool TryCollect(string ownerId, out AvalonCompanionOwnerObservation observation)
    {
        if (!AvalonCompanionAiGameSystems.TryCollectObservation(
                ownerId,
                out AvalonCompanionAiRuntimeObservation? source)
            || source is null)
        {
            observation = default;
            return false;
        }

        observation = new AvalonCompanionOwnerObservation(
            source.Sequence,
            source.ActorRuntimeId,
            source.HasActiveCompanion,
            source.FollowCatchUpCandidate);
        return true;
    }

    public bool TryDispatch(
        string ownerId,
        long observationSequence,
        AvalonCompanionOwnerCommand command)
    {
        if (command != AvalonCompanionOwnerCommand.CatchUpRecall)
        {
            return false;
        }

        return AvalonCompanionAiGameSystems.TryDispatch(
            ownerId,
            observationSequence,
            AvalonCompanionAiRuntimeCommand.CatchUpRecall);
    }
}

public sealed class DirectFoACompanionAiOwnerBoundary : IAvalonCompanionAiOwnerBoundary
{
    private readonly IFoACompanionAiGameSystems gameSystems;

    public DirectFoACompanionAiOwnerBoundary()
        : this(new DirectFoACompanionAiGameSystems())
    {
    }

    internal DirectFoACompanionAiOwnerBoundary(IFoACompanionAiGameSystems gameSystems)
    {
        this.gameSystems = gameSystems
            ?? throw new System.ArgumentNullException(nameof(gameSystems));
    }

    public bool TryAcquire(string ownerId, out string reason)
    {
        return gameSystems.TryAcquire(ownerId, out reason);
    }

    public bool TryRelease(string ownerId)
    {
        return gameSystems.TryRelease(ownerId);
    }

    public bool TryInspectActorId(string ownerId, out string actorRuntimeId)
    {
        return gameSystems.TryInspectActorId(ownerId, out actorRuntimeId);
    }

    public bool TryCollect(string ownerId, out AvalonCompanionOwnerObservation observation)
    {
        return gameSystems.TryCollect(ownerId, out observation);
    }

    public bool TryDispatch(
        string ownerId,
        long observationSequence,
        AvalonCompanionOwnerCommand command)
    {
        if (command != AvalonCompanionOwnerCommand.CatchUpRecall)
        {
            return false;
        }

        return gameSystems.TryDispatch(ownerId, observationSequence, command);
    }
}
