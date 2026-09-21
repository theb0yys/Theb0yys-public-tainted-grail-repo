using System;

namespace AvalonHumans.AI.Package;

public static class HumanIntentClassifier
{
    public static HumanAiIntent Classify(HumanBehaviorObservation? observation)
    {
        if (observation == null)
        {
            return HumanAiIntent.EvidenceInsufficient;
        }

        if (!observation.HasManagedHuman)
        {
            return HumanAiIntent.NoManagedHuman;
        }

        if (observation.Alive != true ||
            observation.Working != true ||
            !IsModeDefined(observation.Mode) ||
            observation.HeroLiveAttackers < 0)
        {
            return HumanAiIntent.EvidenceInsufficient;
        }

        if (observation.Mode == HumanCompanionMode.Hold)
        {
            return observation.HoldMovementLockAttached
                ? HumanAiIntent.HoldPosition
                : HumanAiIntent.ApplyHoldMovementLockCandidate;
        }

        if (observation.HoldMovementLockAttached)
        {
            return HumanAiIntent.ReleaseHoldMovementLockCandidate;
        }

        if (!HasValidDistances(observation))
        {
            return HumanAiIntent.EvidenceInsufficient;
        }

        float distance = observation.DistanceToHero!.Value;
        if (observation.EmergencyRecallEnabled && distance > observation.EmergencyRecallDistance)
        {
            return HumanAiIntent.EmergencyRecallCandidate;
        }

        bool shouldProtect = observation.HeroLiveAttackers > 0 && observation.NativeDefendAllowed;
        if (shouldProtect && distance > observation.CombatRecallDistance)
        {
            return HumanAiIntent.CombatRecallCandidate;
        }

        if (observation.FollowCatchUpEnabled && distance > observation.FollowRecallDistance)
        {
            return HumanAiIntent.FollowRecallCandidate;
        }

        if (shouldProtect && observation.DefendPromptReady)
        {
            return HumanAiIntent.NativeDefendCandidate;
        }

        if (observation.HeroLiveAttackers > 0)
        {
            return HumanAiIntent.ThreatObserved;
        }

        return observation.Mode == HumanCompanionMode.Defend
            ? HumanAiIntent.DefendWaitingForThreat
            : HumanAiIntent.KeepingClose;
    }

    private static bool IsModeDefined(HumanCompanionMode mode) =>
        mode == HumanCompanionMode.Follow ||
        mode == HumanCompanionMode.Hold ||
        mode == HumanCompanionMode.Defend;

    private static bool HasValidDistances(HumanBehaviorObservation observation) =>
        observation.DistanceToHero.HasValue &&
        IsFiniteNonNegative(observation.DistanceToHero.Value) &&
        IsFinitePositive(observation.FollowRecallDistance) &&
        IsFinitePositive(observation.CombatRecallDistance) &&
        IsFinitePositive(observation.EmergencyRecallDistance);

    private static bool IsFiniteNonNegative(float value) =>
        !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;

    private static bool IsFinitePositive(float value) =>
        !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
}
