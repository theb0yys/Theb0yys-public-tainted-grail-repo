using AvalonAI.Contracts;

namespace AvalonHumans.AI.Package;

public enum HumanCommandProposalKind
{
    ApplyHoldMovementLock,
    ReleaseHoldMovementLock,
    RecallToApprovedPlacement,
    PromptNativeDefend
}

public enum HumanRecallPlacementKind
{
    None,
    FollowRole,
    CombatRole
}

public sealed class HumanCommandProposal : IAvalonAiCommandProposal
{
    internal HumanCommandProposal(
        HumanCommandProposalKind kind,
        HumanAiIntent sourceIntent,
        HumanRecallPlacementKind recallPlacement = HumanRecallPlacementKind.None)
    {
        Kind = kind;
        SourceIntent = sourceIntent;
        RecallPlacement = recallPlacement;
    }

    public HumanCommandProposalKind Kind { get; }
    public HumanAiIntent SourceIntent { get; }
    public HumanRecallPlacementKind RecallPlacement { get; }
}
