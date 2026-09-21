using System;

namespace AvalonCompanions.AI.Package;

public static class CompanionIntentClassifier
{
    public static CompanionAiIntent Classify(CompanionIntentObservation? observation)
    {
        if (observation == null)
        {
            return CompanionAiIntent.EvidenceInsufficient;
        }

        if (!observation.HasActiveCompanion)
        {
            return CompanionAiIntent.NoActiveCompanion;
        }

        if (observation.Mode != CompanionAiMode.Follow &&
            observation.Mode != CompanionAiMode.Stay &&
            observation.Mode != CompanionAiMode.Defend)
        {
            return CompanionAiIntent.EvidenceInsufficient;
        }

        if (!observation.HasNpcElement ||
            observation.NpcAlive != true ||
            observation.NpcWorking != true)
        {
            return CompanionAiIntent.EvidenceInsufficient;
        }

        if (observation.NpcInCombat == true ||
            observation.HeroLiveAttackers > 0 ||
            observation.PossibleTargetCount.GetValueOrDefault() > 0 ||
            observation.PossibleAttackerCount.GetValueOrDefault() > 0)
        {
            return CompanionAiIntent.NativeCombatObserved;
        }

        if (observation.CanMove == false)
        {
            return CompanionAiIntent.MovementBlocked;
        }

        if (observation.Mode == CompanionAiMode.Defend)
        {
            return CompanionAiIntent.DefendWaitingForThreat;
        }

        if (observation.FollowCatchUpCandidate)
        {
            return CompanionAiIntent.FollowCatchUpCandidate;
        }

        if (observation.Mode == CompanionAiMode.Stay)
        {
            return CompanionAiIntent.StayPosition;
        }

        if (string.Equals(observation.MovementState, "Patrol", StringComparison.Ordinal))
        {
            return CompanionAiIntent.IdleNativePatrol;
        }

        if (observation.PossibleTargetCount == 0 &&
            observation.PossibleAttackerCount == 0)
        {
            return CompanionAiIntent.NoTargets;
        }

        return CompanionAiIntent.EvidenceInsufficient;
    }
}
