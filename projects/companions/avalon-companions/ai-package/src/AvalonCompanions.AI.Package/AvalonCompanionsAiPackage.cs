using AvalonAI.Contracts;

namespace AvalonCompanions.AI.Package;

public sealed class AvalonCompanionsAiPackage : IAvalonAiEvaluatingPackage
{
    public const string PackageId = "kane.tgfoa.avalon-companions.ai-intent-profile";
    public const string PackageVersion = "0.2.1";

    public AvalonCompanionsAiPackage()
    {
        Manifest = new AvalonAiPackageManifest(
            PackageId,
            "Avalon Companions AI Intent Profile",
            PackageVersion,
            new AvalonAiApiVersion(1, 1));
    }

    public AvalonAiPackageManifest Manifest { get; }

    public IAvalonAiCommandProposal? Evaluate(IAvalonAiObservation observation)
    {
        if (!(observation is CompanionIntentObservation companionObservation))
        {
            return null;
        }

        CompanionAiIntent intent = CompanionIntentClassifier.Classify(companionObservation);

        if (intent == CompanionAiIntent.FollowCatchUpCandidate)
        {
            return new CompanionCommandProposal(
                CompanionCommandProposalKind.CatchUpRecall,
                intent);
        }

        if (intent == CompanionAiIntent.NativeCombatObserved &&
            companionObservation.HeroLiveAttackers > 0 &&
            companionObservation.NativeDefendPromptReady &&
            (companionObservation.Mode == CompanionAiMode.Defend ||
             companionObservation.NativeDefendAssistAllowed))
        {
            return new CompanionCommandProposal(
                CompanionCommandProposalKind.NativeDefendPrompt,
                intent);
        }

        return null;
    }
}
