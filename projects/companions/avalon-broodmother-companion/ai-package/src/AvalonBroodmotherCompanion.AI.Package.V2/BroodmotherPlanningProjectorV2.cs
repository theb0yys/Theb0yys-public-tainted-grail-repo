using System;
using System.Collections.Generic;
using AvalonAI.Contracts.V2;

namespace AvalonBroodmotherCompanion.AI.Package.V2;

public sealed class BroodmotherPlanningProjectionV2
{
    public BroodmotherPlanningProjectionV2(
        bool accepted,
        string reason,
        IEnumerable<PlanningFact> facts)
    {
        Accepted = accepted;
        Reason = reason ?? string.Empty;
        Facts = Array.AsReadOnly(new List<PlanningFact>(facts ?? throw new ArgumentNullException(nameof(facts))).ToArray());
    }

    public bool Accepted { get; }

    public string Reason { get; }

    public IReadOnlyList<PlanningFact> Facts { get; }
}

public sealed class BroodmotherPlanningProjectorV2
{
    private static readonly IReadOnlyList<PlanningFact> NoFacts =
        Array.AsReadOnly(Array.Empty<PlanningFact>());

    public BroodmotherPlanningProjectionV2 Project(
        ActorSnapshot actor,
        IAvalonBlackboardReader blackboard)
    {
        _ = actor ?? throw new ArgumentNullException(nameof(actor));
        _ = blackboard ?? throw new ArgumentNullException(nameof(blackboard));

        if (actor.Lease.Mode != ActorExecutionMode.BlazeOwned)
        {
            return Reject("broodmother-projector-not-blaze-owned");
        }

        if (!BroodmotherCompanionAiV2Contract.TryGetRoleCode(actor.Role, out var roleCode))
        {
            return Reject("broodmother-projector-unsupported-role");
        }

        if (!blackboard.TryRead(BroodmotherCompanionAiV2Contract.RoleBoundId, out string boundRole) ||
            !StringComparer.Ordinal.Equals(boundRole, actor.Role.Value))
        {
            return Reject("broodmother-projector-role-binding-mismatch");
        }

        if (!blackboard.TryRead(BroodmotherCompanionAiV2Contract.MustStop, out bool mustStop) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.MustRetreat, out bool mustRetreat) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.TargetValid, out bool targetValid) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.TargetWeak, out bool targetWeak) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.TargetDistanceBand, out int distanceBand) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.TargetAngleBand, out int angleBand) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.PackAttackSlotGranted, out bool slotGranted) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.PackAttackSlotGeneration, out int slotGeneration) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.PackAttackSlotKind, out int slotKind) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.PackRangedLaneClear, out bool rangedLaneClear) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.RecoveryRequired, out bool recoveryRequired) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.PackLastSlotGeneration, out int lastSlotGeneration) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.NeedsFlank, out bool needsFlank) ||
            !blackboard.TryRead(BroodmotherCompanionAiV2Contract.NeedsReposition, out bool needsReposition))
        {
            return Reject("broodmother-projector-rabbit-snapshot-incomplete");
        }

        if (distanceBand < BroodmotherCompanionAiV2Contract.DistanceBandBite ||
            distanceBand > BroodmotherCompanionAiV2Contract.DistanceBandUnknown ||
            angleBand < BroodmotherCompanionAiV2Contract.AngleBandFront ||
            angleBand > BroodmotherCompanionAiV2Contract.AngleBandUnknown ||
            slotGeneration < 0 ||
            lastSlotGeneration < 0 ||
            slotKind < BroodmotherCompanionAiV2Contract.SlotKindNone ||
            slotKind > BroodmotherCompanionAiV2Contract.SlotKindSpit)
        {
            return Reject("broodmother-projector-rabbit-value-invalid");
        }

        var exactSlotGranted = slotGranted &&
            slotKind != BroodmotherCompanionAiV2Contract.SlotKindNone &&
            slotGeneration > lastSlotGeneration;
        if ((!slotGranted && slotKind != BroodmotherCompanionAiV2Contract.SlotKindNone) ||
            (slotGranted && !exactSlotGranted))
        {
            return Reject("broodmother-projector-slot-generation-invalid");
        }

        var atFlank = targetValid &&
            !needsFlank &&
            (angleBand == BroodmotherCompanionAiV2Contract.AngleBandSide ||
             angleBand == BroodmotherCompanionAiV2Contract.AngleBandRear);

        var facts = new[]
        {
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactRoleCode, roleCode),
            Fact(BroodmotherCompanionAiV2Contract.FactMustStop, mustStop),
            Fact(BroodmotherCompanionAiV2Contract.FactMustRetreat, mustRetreat),
            Fact(BroodmotherCompanionAiV2Contract.FactTargetValid, targetValid),
            Fact(BroodmotherCompanionAiV2Contract.FactTargetWeak, targetWeak),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactDistanceBand, distanceBand),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactAngleBand, angleBand),
            Fact(BroodmotherCompanionAiV2Contract.FactSlotGranted, exactSlotGranted),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactSlotKind, exactSlotGranted ? slotKind : BroodmotherCompanionAiV2Contract.SlotKindNone),
            Fact(BroodmotherCompanionAiV2Contract.FactRangedLaneClear, rangedLaneClear),
            Fact(BroodmotherCompanionAiV2Contract.FactRecoveryRequired, recoveryRequired),
            Fact(BroodmotherCompanionAiV2Contract.FactAtFlank, atFlank),
            Fact(BroodmotherCompanionAiV2Contract.FactAtRoleBand, targetValid && !needsReposition),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactActionCompleted, 0),
        };

        return new BroodmotherPlanningProjectionV2(true, "broodmother-projector-accepted", facts);
    }

    private static PlanningFact Fact(PlanningFactId id, bool value) =>
        new PlanningFact(id, value ? 1 : 0);

    private static BroodmotherPlanningProjectionV2 Reject(string reason) =>
        new BroodmotherPlanningProjectionV2(false, reason, NoFacts);
}
