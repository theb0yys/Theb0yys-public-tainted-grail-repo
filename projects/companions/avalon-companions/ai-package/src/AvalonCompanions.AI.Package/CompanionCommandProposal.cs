using AvalonAI.Contracts;

namespace AvalonCompanions.AI.Package;

public enum CompanionCommandProposalKind
{
    CatchUpRecall,
    NativeDefendPrompt
}

public sealed class CompanionCommandProposal : IAvalonAiCommandProposal
{
    internal CompanionCommandProposal(
        CompanionCommandProposalKind kind,
        CompanionAiIntent sourceIntent)
    {
        Kind = kind;
        SourceIntent = sourceIntent;
    }

    public CompanionCommandProposalKind Kind { get; }

    public CompanionAiIntent SourceIntent { get; }
}
