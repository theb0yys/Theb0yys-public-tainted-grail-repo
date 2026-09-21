namespace AvalonHumanCompanions;

internal readonly struct HumanCompanionRosterEntry
{
    internal HumanCompanionRosterEntry(
        string displayName,
        string templateName,
        string guid,
        string reviewId,
        string riskFlags)
    {
        DisplayName = displayName;
        TemplateName = templateName;
        Guid = guid;
        ReviewId = reviewId;
        RiskFlags = riskFlags;
    }

    internal string DisplayName { get; }

    internal string TemplateName { get; }

    internal string Guid { get; }

    internal string ReviewId { get; }

    internal string RiskFlags { get; }
}
