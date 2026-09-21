namespace AvalonCompanions.Framework;

internal readonly struct CompanionTamingProfile
{
    internal CompanionTamingProfile(
        CompanionTamingEligibility eligibility,
        string reason,
        string templateGuid,
        string templateName,
        string displayName,
        string reviewId,
        bool requiresPetComponent,
        bool runtimeApproved,
        CompanionRuntimeCombatRole combatRole,
        CompanionRuntimeTemperament temperament)
    {
        Eligibility = eligibility;
        Reason = reason;
        TemplateGuid = templateGuid;
        TemplateName = templateName;
        DisplayName = displayName;
        ReviewId = reviewId;
        RequiresPetComponent = requiresPetComponent;
        RuntimeApproved = runtimeApproved;
        CombatRole = combatRole;
        Temperament = temperament;
    }

    internal CompanionTamingEligibility Eligibility { get; }

    internal string Reason { get; }

    internal string TemplateGuid { get; }

    internal string TemplateName { get; }

    internal string DisplayName { get; }

    internal string ReviewId { get; }

    internal bool RequiresPetComponent { get; }

    internal bool RuntimeApproved { get; }

    internal CompanionRuntimeCombatRole CombatRole { get; }

    internal CompanionRuntimeTemperament Temperament { get; }
}

internal enum CompanionTamingEligibility
{
    TameableCandidate,
    AlreadyCompanion,
    UnsafeHostile,
    UndeadBlocked,
    PassiveOnly,
    EvidenceInsufficient,
}
