using AvalonAI.Contracts;

namespace AvalonHumans.AI.Package;

public sealed class AvalonHumansAiPackage : IAvalonAiEvaluatingPackage
{
    public const string PackageId = "kane.tgfoa.avalon-human-companions.ai-companion-brain";
    public const string PackageVersion = "0.1.0";

    public AvalonHumansAiPackage()
    {
        Manifest = new AvalonAiPackageManifest(
            PackageId,
            "Avalon Human Companion Brain",
            PackageVersion,
            new AvalonAiApiVersion(1, 1));
    }

    public AvalonAiPackageManifest Manifest { get; }

    public IAvalonAiCommandProposal? Evaluate(IAvalonAiObservation observation)
    {
        if (!(observation is HumanBehaviorObservation humanObservation))
        {
            return null;
        }

        HumanAiIntent intent = HumanIntentClassifier.Classify(humanObservation);
        switch (intent)
        {
            case HumanAiIntent.ApplyHoldMovementLockCandidate:
                return new HumanCommandProposal(HumanCommandProposalKind.ApplyHoldMovementLock, intent);
            case HumanAiIntent.ReleaseHoldMovementLockCandidate:
                return new HumanCommandProposal(HumanCommandProposalKind.ReleaseHoldMovementLock, intent);
            case HumanAiIntent.EmergencyRecallCandidate:
            case HumanAiIntent.FollowRecallCandidate:
                return new HumanCommandProposal(
                    HumanCommandProposalKind.RecallToApprovedPlacement,
                    intent,
                    HumanRecallPlacementKind.FollowRole);
            case HumanAiIntent.CombatRecallCandidate:
                return new HumanCommandProposal(
                    HumanCommandProposalKind.RecallToApprovedPlacement,
                    intent,
                    HumanRecallPlacementKind.CombatRole);
            case HumanAiIntent.NativeDefendCandidate:
                return new HumanCommandProposal(HumanCommandProposalKind.PromptNativeDefend, intent);
            default:
                return null;
        }
    }
}
