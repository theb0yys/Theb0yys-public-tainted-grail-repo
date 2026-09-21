using System;

namespace AvalonCompanions.Framework;

internal sealed class CompanionTrustProfile
{
    internal const int MinScore = 0;
    internal const int MaxScore = 100;
    internal const int StartingTrust = 50;
    internal const int StartingLoyalty = 50;

    internal CompanionTrustProfile(int createdFrame)
    {
        TrustScore = StartingTrust;
        LoyaltyScore = StartingLoyalty;
        LastEvent = CompanionTrustEvent.Created;
        LastReason = "runtime trust profile created";
        LastEventFrame = createdFrame;
    }

    internal int TrustScore { get; private set; }

    internal int LoyaltyScore { get; private set; }

    internal CompanionBondLevel TrustLevel => Classify(TrustScore);

    internal CompanionBondLevel LoyaltyLevel => Classify(LoyaltyScore);

    internal CompanionTrustEvent LastEvent { get; private set; }

    internal string LastReason { get; private set; }

    internal int LastEventFrame { get; private set; }

    internal CompanionTrustChange Apply(
        CompanionTrustEvent trustEvent,
        string reason,
        int frame,
        float gainMultiplier = 1f,
        float penaltyMultiplier = 1f)
    {
        int previousTrust = TrustScore;
        int previousLoyalty = LoyaltyScore;
        int trustDelta = ScaleDelta(GetTrustDelta(trustEvent), gainMultiplier, penaltyMultiplier);
        int loyaltyDelta = ScaleDelta(GetLoyaltyDelta(trustEvent), gainMultiplier, penaltyMultiplier);

        TrustScore = Clamp(previousTrust + trustDelta);
        LoyaltyScore = Clamp(previousLoyalty + loyaltyDelta);
        LastEvent = trustEvent;
        LastReason = reason;
        LastEventFrame = frame;

        return new CompanionTrustChange(
            trustEvent,
            previousTrust,
            TrustScore,
            trustDelta,
            previousLoyalty,
            LoyaltyScore,
            loyaltyDelta,
            TrustLevel,
            LoyaltyLevel);
    }

    internal void RestoreProfileOnly(int trustScore, int loyaltyScore, CompanionTrustEvent lastEvent, string reason, int frame)
    {
        TrustScore = Clamp(trustScore);
        LoyaltyScore = Clamp(loyaltyScore);
        LastEvent = lastEvent;
        LastReason = reason;
        LastEventFrame = frame;
    }

    private static int GetTrustDelta(CompanionTrustEvent trustEvent)
    {
        return trustEvent switch
        {
            CompanionTrustEvent.DialogueOpened => 1,
            CompanionTrustEvent.Summoned => 2,
            CompanionTrustEvent.ExistingRecalled => 1,
            CompanionTrustEvent.Follow => 1,
            CompanionTrustEvent.Stay => 1,
            CompanionTrustEvent.Defend => 1,
            CompanionTrustEvent.RangeChanged => 1,
            CompanionTrustEvent.Recalled => 1,
            CompanionTrustEvent.ComeClose => 2,
            CompanionTrustEvent.Recovered => 2,
            CompanionTrustEvent.NativeAssistCatchUp => 1,
            CompanionTrustEvent.NativeAssistDefend => 1,
            CompanionTrustEvent.Dismissed => -1,
            CompanionTrustEvent.Removed => -3,
            _ => 0,
        };
    }

    private static int GetLoyaltyDelta(CompanionTrustEvent trustEvent)
    {
        return trustEvent switch
        {
            CompanionTrustEvent.Summoned => 2,
            CompanionTrustEvent.ExistingRecalled => 1,
            CompanionTrustEvent.Follow => 1,
            CompanionTrustEvent.Stay => 1,
            CompanionTrustEvent.Defend => 2,
            CompanionTrustEvent.Recalled => 1,
            CompanionTrustEvent.ComeClose => 1,
            CompanionTrustEvent.Recovered => 2,
            CompanionTrustEvent.NativeAssistCatchUp => 1,
            CompanionTrustEvent.NativeAssistDefend => 3,
            CompanionTrustEvent.Dismissed => -1,
            CompanionTrustEvent.Removed => -4,
            _ => 0,
        };
    }

    private static CompanionBondLevel Classify(int score)
    {
        if (score < 35)
        {
            return CompanionBondLevel.Wary;
        }

        if (score < 60)
        {
            return CompanionBondLevel.Familiar;
        }

        if (score < 80)
        {
            return CompanionBondLevel.Trusted;
        }

        return CompanionBondLevel.Loyal;
    }

    private static int Clamp(int value)
    {
        if (value < MinScore)
        {
            return MinScore;
        }

        return value > MaxScore ? MaxScore : value;
    }

    private static int ScaleDelta(int delta, float gainMultiplier, float penaltyMultiplier)
    {
        if (delta == 0)
        {
            return 0;
        }

        float multiplier = delta > 0 ? gainMultiplier : penaltyMultiplier;
        int scaled = (int)Math.Round(delta * multiplier, MidpointRounding.AwayFromZero);
        if (delta > 0)
        {
            return Math.Max(1, scaled);
        }

        return Math.Min(-1, scaled);
    }
}

internal enum CompanionBondLevel
{
    Wary,
    Familiar,
    Trusted,
    Loyal,
}

internal enum CompanionTrustEvent
{
    Created,
    Selected,
    DialogueOpened,
    Summoned,
    ExistingRecalled,
    Follow,
    Stay,
    Defend,
    RangeChanged,
    Recalled,
    ComeClose,
    Recovered,
    NativeAssistCatchUp,
    NativeAssistDefend,
    LifecycleChecked,
    Dismissed,
    Removed,
    BlockedCommand,
}

internal readonly struct CompanionTrustChange
{
    internal CompanionTrustChange(
        CompanionTrustEvent trustEvent,
        int previousTrust,
        int trustScore,
        int trustDelta,
        int previousLoyalty,
        int loyaltyScore,
        int loyaltyDelta,
        CompanionBondLevel trustLevel,
        CompanionBondLevel loyaltyLevel)
    {
        Event = trustEvent;
        PreviousTrust = previousTrust;
        TrustScore = trustScore;
        TrustDelta = trustDelta;
        PreviousLoyalty = previousLoyalty;
        LoyaltyScore = loyaltyScore;
        LoyaltyDelta = loyaltyDelta;
        TrustLevel = trustLevel;
        LoyaltyLevel = loyaltyLevel;
    }

    internal CompanionTrustEvent Event { get; }

    internal int PreviousTrust { get; }

    internal int TrustScore { get; }

    internal int TrustDelta { get; }

    internal int PreviousLoyalty { get; }

    internal int LoyaltyScore { get; }

    internal int LoyaltyDelta { get; }

    internal CompanionBondLevel TrustLevel { get; }

    internal CompanionBondLevel LoyaltyLevel { get; }

    internal bool Changed => TrustDelta != 0 || LoyaltyDelta != 0;
}
