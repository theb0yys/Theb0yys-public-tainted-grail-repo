using System;

namespace AvalonCompanions.Framework;

internal static class CompanionBondPolicy
{
    internal static CompanionBondPolicyDecision Evaluate(CompanionCoreRuntimeProfile profile, bool enabled)
    {
        if (!enabled)
        {
            return CompanionBondPolicyDecision.Neutral("bond policy disabled");
        }

        CompanionTrustProfile trust = profile.TrustProfile;
        CompanionBondLevel bondLevel = trust.TrustLevel < trust.LoyaltyLevel
            ? trust.TrustLevel
            : trust.LoyaltyLevel;

        return bondLevel switch
        {
            CompanionBondLevel.Wary => new CompanionBondPolicyDecision(
                enabled: true,
                bondLevel,
                catchUpIntervalMultiplier: 1.35f,
                catchUpDistanceMultiplier: 1.2f,
                defendCooldownMultiplier: 1.5f,
                allowsAutomaticDefendAssist: false,
                trustGainMultiplier: 1.25f,
                trustPenaltyMultiplier: 1.35f,
                reason: "wary bond: slower response and automatic defend assist blocked unless explicit Defend mode is active"),
            CompanionBondLevel.Trusted => new CompanionBondPolicyDecision(
                enabled: true,
                bondLevel,
                catchUpIntervalMultiplier: 0.8f,
                catchUpDistanceMultiplier: 0.9f,
                defendCooldownMultiplier: 0.75f,
                allowsAutomaticDefendAssist: true,
                trustGainMultiplier: 0.9f,
                trustPenaltyMultiplier: 0.85f,
                reason: "trusted bond: faster response and softer penalties"),
            CompanionBondLevel.Loyal => new CompanionBondPolicyDecision(
                enabled: true,
                bondLevel,
                catchUpIntervalMultiplier: 0.65f,
                catchUpDistanceMultiplier: 0.8f,
                defendCooldownMultiplier: 0.55f,
                allowsAutomaticDefendAssist: true,
                trustGainMultiplier: 0.75f,
                trustPenaltyMultiplier: 0.5f,
                reason: "loyal bond: fastest response and much softer penalties"),
            _ => new CompanionBondPolicyDecision(
                enabled: true,
                CompanionBondLevel.Familiar,
                catchUpIntervalMultiplier: 1f,
                catchUpDistanceMultiplier: 1f,
                defendCooldownMultiplier: 1f,
                allowsAutomaticDefendAssist: true,
                trustGainMultiplier: 1f,
                trustPenaltyMultiplier: 1f,
                reason: "familiar bond: baseline response"),
        };
    }
}

internal readonly struct CompanionBondPolicyDecision
{
    private const float MinimumCatchUpIntervalSeconds = 0.35f;
    private const float MinimumCatchUpDistanceMeters = 8f;
    private const float MinimumDefendCooldownSeconds = 0.75f;

    internal CompanionBondPolicyDecision(
        bool enabled,
        CompanionBondLevel bondLevel,
        float catchUpIntervalMultiplier,
        float catchUpDistanceMultiplier,
        float defendCooldownMultiplier,
        bool allowsAutomaticDefendAssist,
        float trustGainMultiplier,
        float trustPenaltyMultiplier,
        string reason)
    {
        Enabled = enabled;
        BondLevel = bondLevel;
        CatchUpIntervalMultiplier = catchUpIntervalMultiplier;
        CatchUpDistanceMultiplier = catchUpDistanceMultiplier;
        DefendCooldownMultiplier = defendCooldownMultiplier;
        AllowsAutomaticDefendAssist = allowsAutomaticDefendAssist;
        TrustGainMultiplier = trustGainMultiplier;
        TrustPenaltyMultiplier = trustPenaltyMultiplier;
        Reason = reason;
    }

    internal bool Enabled { get; }

    internal CompanionBondLevel BondLevel { get; }

    internal float CatchUpIntervalMultiplier { get; }

    internal float CatchUpDistanceMultiplier { get; }

    internal float DefendCooldownMultiplier { get; }

    internal bool AllowsAutomaticDefendAssist { get; }

    internal float TrustGainMultiplier { get; }

    internal float TrustPenaltyMultiplier { get; }

    internal string Reason { get; }

    internal static CompanionBondPolicyDecision Neutral(string reason)
    {
        return new CompanionBondPolicyDecision(
            enabled: false,
            CompanionBondLevel.Familiar,
            catchUpIntervalMultiplier: 1f,
            catchUpDistanceMultiplier: 1f,
            defendCooldownMultiplier: 1f,
            allowsAutomaticDefendAssist: true,
            trustGainMultiplier: 1f,
            trustPenaltyMultiplier: 1f,
            reason);
    }

    internal float ApplyCatchUpInterval(float baseSeconds)
    {
        return Math.Max(MinimumCatchUpIntervalSeconds, baseSeconds * CatchUpIntervalMultiplier);
    }

    internal float ApplyCatchUpDistance(float baseMeters)
    {
        return Math.Max(MinimumCatchUpDistanceMeters, baseMeters * CatchUpDistanceMultiplier);
    }

    internal float ApplyDefendCooldown(float baseSeconds)
    {
        return Math.Max(MinimumDefendCooldownSeconds, baseSeconds * DefendCooldownMultiplier);
    }

    internal string FormatAuditSegment()
    {
        return $"bond={BondLevel}; catchUpIntervalX={CatchUpIntervalMultiplier:0.##}; "
            + $"catchUpDistanceX={CatchUpDistanceMultiplier:0.##}; defendCooldownX={DefendCooldownMultiplier:0.##}; "
            + $"autoDefendAllowed={AllowsAutomaticDefendAssist.ToString().ToLowerInvariant()}; "
            + $"trustGainX={TrustGainMultiplier:0.##}; trustPenaltyX={TrustPenaltyMultiplier:0.##}";
    }
}
