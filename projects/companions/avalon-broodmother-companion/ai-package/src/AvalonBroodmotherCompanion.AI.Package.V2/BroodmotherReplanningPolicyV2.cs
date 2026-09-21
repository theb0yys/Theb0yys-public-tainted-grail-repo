using System;
using AvalonAI.Contracts.V2;

namespace AvalonBroodmotherCompanion.AI.Package.V2;

public enum BroodmotherReplanTriggerV2
{
    LeaseOrGenerationMismatch = 0,
    CleanupRequested = 1,
    RuntimeStopping = 2,
    ActorReleased = 3,
    TargetInvalid = 4,
    TargetChanged = 5,
    DamageRevisionIncreased = 6,
    ThreatEnteredUnsafeBand = 7,
    EmergencyGoalOutranks = 8,
    ProcedureTimeout = 9,
    ProcedureFault = 10,
    CommandFault = 11,
    OrdinaryObservation = 12,
}

public sealed class BroodmotherReplanDecisionV2
{
    public BroodmotherReplanDecisionV2(
        bool cancelPlan,
        bool interruptAction,
        bool requireRecovery,
        string reason)
    {
        CancelPlan = cancelPlan;
        InterruptAction = interruptAction;
        RequireRecovery = requireRecovery;
        Reason = reason ?? string.Empty;
    }

    public bool CancelPlan { get; }

    public bool InterruptAction { get; }

    public bool RequireRecovery { get; }

    public string Reason { get; }
}

public static class BroodmotherReplanningPolicyV2
{
    public static BroodmotherReplanDecisionV2 Evaluate(
        BroodmotherReplanTriggerV2 trigger,
        ActionId activeAction,
        bool minimumCommitmentElapsed)
    {
        if (IsImmediateLifecycleTrigger(trigger))
        {
            return Interrupt("broodmother-replan-lifecycle", requireRecovery: false);
        }

        if (trigger == BroodmotherReplanTriggerV2.ProcedureTimeout ||
            trigger == BroodmotherReplanTriggerV2.ProcedureFault ||
            trigger == BroodmotherReplanTriggerV2.CommandFault)
        {
            return Interrupt("broodmother-replan-procedure-fault", requireRecovery: true);
        }

        if ((trigger == BroodmotherReplanTriggerV2.TargetInvalid ||
             trigger == BroodmotherReplanTriggerV2.TargetChanged) &&
            IsTargeted(activeAction))
        {
            return Interrupt("broodmother-replan-invalid-target", requireRecovery: false);
        }

        if (trigger == BroodmotherReplanTriggerV2.DamageRevisionIncreased &&
            IsDamageInterruptible(activeAction))
        {
            return Interrupt("broodmother-replan-damage", requireRecovery: true);
        }

        if (trigger == BroodmotherReplanTriggerV2.ThreatEnteredUnsafeBand &&
            IsThreatInterruptible(activeAction))
        {
            return Interrupt("broodmother-replan-threat", requireRecovery: false);
        }

        if (trigger == BroodmotherReplanTriggerV2.EmergencyGoalOutranks && minimumCommitmentElapsed)
        {
            return Interrupt("broodmother-replan-emergency-goal", requireRecovery: false);
        }

        return new BroodmotherReplanDecisionV2(false, false, false, "broodmother-replan-keep-active");
    }

    private static bool IsImmediateLifecycleTrigger(BroodmotherReplanTriggerV2 trigger)
    {
        return trigger == BroodmotherReplanTriggerV2.LeaseOrGenerationMismatch ||
            trigger == BroodmotherReplanTriggerV2.CleanupRequested ||
            trigger == BroodmotherReplanTriggerV2.RuntimeStopping ||
            trigger == BroodmotherReplanTriggerV2.ActorReleased;
    }

    private static bool IsTargeted(ActionId action)
    {
        return action != BroodmotherCompanionAiV2Contract.ActionStopSafely &&
            action != BroodmotherCompanionAiV2Contract.ActionRetreat &&
            action != BroodmotherCompanionAiV2Contract.ActionRecoverControl &&
            ContainsAction(action);
    }

    private static bool IsDamageInterruptible(ActionId action)
    {
        return action == BroodmotherCompanionAiV2Contract.ActionRecoverControl ||
            action == BroodmotherCompanionAiV2Contract.ActionVanguardClose ||
            action == BroodmotherCompanionAiV2Contract.ActionPouncerFlank ||
            action == BroodmotherCompanionAiV2Contract.ActionPouncerLeap ||
            action == BroodmotherCompanionAiV2Contract.ActionSpitterReposition ||
            action == BroodmotherCompanionAiV2Contract.ActionSpitterSpit ||
            action == BroodmotherCompanionAiV2Contract.ActionHarrierReposition ||
            action == BroodmotherCompanionAiV2Contract.ActionAmbusherWideFlank ||
            action == BroodmotherCompanionAiV2Contract.ActionAmbusherLeap ||
            action == BroodmotherCompanionAiV2Contract.ActionFinisherClaimPosition;
    }

    private static bool IsThreatInterruptible(ActionId action)
    {
        return action == BroodmotherCompanionAiV2Contract.ActionVanguardClose ||
            action == BroodmotherCompanionAiV2Contract.ActionPouncerFlank ||
            action == BroodmotherCompanionAiV2Contract.ActionSpitterReposition ||
            action == BroodmotherCompanionAiV2Contract.ActionSpitterSpit ||
            action == BroodmotherCompanionAiV2Contract.ActionHarrierReposition ||
            action == BroodmotherCompanionAiV2Contract.ActionAmbusherWideFlank ||
            action == BroodmotherCompanionAiV2Contract.ActionFinisherClaimPosition;
    }

    private static bool ContainsAction(ActionId action)
    {
        for (var index = 0; index < BroodmotherCompanionAiV2Contract.ActionIds.Count; index++)
        {
            if (BroodmotherCompanionAiV2Contract.ActionIds[index] == action)
            {
                return true;
            }
        }

        return false;
    }

    private static BroodmotherReplanDecisionV2 Interrupt(string reason, bool requireRecovery) =>
        new BroodmotherReplanDecisionV2(true, true, requireRecovery, reason);
}
