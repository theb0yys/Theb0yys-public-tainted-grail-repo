using System;
using System.Collections.Generic;
using AvalonAI.Contracts.V2;

namespace AvalonBroodmotherCompanion.AI.Package.V2;

public sealed class BroodmotherCompanionGoalPolicyV2 : IAvalonGoalPolicy
{
    private static readonly IReadOnlyList<GoalRequest> NoGoals =
        Array.AsReadOnly(Array.Empty<GoalRequest>());

    public IReadOnlyList<GoalRequest> Evaluate(
        ActorSnapshot actor,
        IAvalonBlackboardReader blackboard)
    {
        _ = actor ?? throw new ArgumentNullException(nameof(actor));
        _ = blackboard ?? throw new ArgumentNullException(nameof(blackboard));

        if (actor.Lease.Mode != ActorExecutionMode.BlazeOwned ||
            !BroodmotherCompanionAiV2Contract.TryGetRoleCode(actor.Role, out var roleCode) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.RoleBoundId, out string boundRole) ||
            !StringComparer.Ordinal.Equals(boundRole, actor.Role.Value) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.MustStop, out bool mustStop))
        {
            return NoGoals;
        }

        if (mustStop)
        {
            return One(BroodmotherCompanionAiV2Contract.GoalStopSafely, 100f, 100f, 0, "broodmother-must-stop");
        }

        if (!blackboard.TryRead(BroodmotherCompanionAiV2Contract.MustRetreat, out bool mustRetreat) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.RecoveryRequired, out bool recoveryRequired))
        {
            return NoGoals;
        }

        if (mustRetreat)
        {
            return One(BroodmotherCompanionAiV2Contract.GoalRetreat, 90f, 80f, 250, "broodmother-must-retreat");
        }

        if (recoveryRequired)
        {
            return One(BroodmotherCompanionAiV2Contract.GoalRecover, 80f, 60f, 200, "broodmother-recovery-required");
        }

        if (!blackboard.TryRead(BroodmotherCompanionAiV2Contract.TargetValid, out bool targetValid) || !targetValid)
        {
            return NoGoals;
        }

        switch (roleCode)
        {
            case BroodmotherCompanionAiV2Contract.CrimsonVanguardRoleCode:
                return One(BroodmotherCompanionAiV2Contract.GoalVanguardBitePressure, 62f, 20f, 300, "crimson-vanguard-pressure");
            case BroodmotherCompanionAiV2Contract.PalePouncerRoleCode:
                return One(BroodmotherCompanionAiV2Contract.GoalPouncerLeapPressure, 64f, 24f, 400, "pale-pouncer-pressure");
            case BroodmotherCompanionAiV2Contract.GildedSpitterRoleCode:
                return One(BroodmotherCompanionAiV2Contract.GoalSpitterRangedPressure, 63f, 22f, 350, "gilded-spitter-pressure");
            case BroodmotherCompanionAiV2Contract.CrimsonHarrierRoleCode:
                return One(BroodmotherCompanionAiV2Contract.GoalHarrierBiteCycle, 61f, 26f, 250, "crimson-harrier-cycle");
            case BroodmotherCompanionAiV2Contract.PaleAmbusherRoleCode:
                return One(BroodmotherCompanionAiV2Contract.GoalAmbusherWideLeap, 65f, 25f, 450, "pale-ambusher-pressure");
            case BroodmotherCompanionAiV2Contract.GildedFinisherRoleCode:
                return FinisherGoal(blackboard);
            default:
                return NoGoals;
        }
    }

    private static IReadOnlyList<GoalRequest> FinisherGoal(IAvalonBlackboardReader blackboard)
    {
        if (!blackboard.TryRead(BroodmotherCompanionAiV2Contract.TargetWeak, out bool targetWeak) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.PackFinisherClaimGranted, out bool claimGranted))
        {
            return NoGoals;
        }

        if (targetWeak)
        {
            return claimGranted
                ? One(BroodmotherCompanionAiV2Contract.GoalFinisherWeakTarget, 75f, 40f, 300, "gilded-finisher-weak-claim")
                : NoGoals;
        }

        return One(BroodmotherCompanionAiV2Contract.GoalFinisherPressure, 55f, 15f, 250, "gilded-finisher-pressure");
    }

    private static IReadOnlyList<GoalRequest> One(
        GoalId goal,
        float priority,
        float urgency,
        int commitmentMilliseconds,
        string reason)
    {
        return Array.AsReadOnly(new[]
        {
            new GoalRequest(
                goal,
                priority,
                urgency,
                TimeSpan.FromMilliseconds(commitmentMilliseconds),
                reason),
        });
    }
}
