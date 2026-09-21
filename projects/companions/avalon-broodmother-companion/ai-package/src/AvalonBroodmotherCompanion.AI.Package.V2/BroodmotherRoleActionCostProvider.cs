using System;
using AvalonAI.Contracts.V2;

namespace AvalonBroodmotherCompanion.AI.Package.V2;

public sealed class BroodmotherRoleActionCostProvider : IAvalonActionCostProvider
{
    private readonly float baseCost;
    private readonly int preferredDistanceBand;
    private readonly int requiredSlotKind;
    private readonly bool leap;
    private readonly bool spit;
    private readonly bool finisher;

    public BroodmotherRoleActionCostProvider(
        float baseCost,
        int preferredDistanceBand,
        int requiredSlotKind,
        bool leap,
        bool spit,
        bool finisher)
    {
        if (float.IsNaN(baseCost) || float.IsInfinity(baseCost) || baseCost < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(baseCost));
        }

        if (preferredDistanceBand < BroodmotherCompanionAiV2Contract.DistanceBandBite ||
            preferredDistanceBand > BroodmotherCompanionAiV2Contract.DistanceBandSpit)
        {
            throw new ArgumentOutOfRangeException(nameof(preferredDistanceBand));
        }

        if (requiredSlotKind < BroodmotherCompanionAiV2Contract.SlotKindNone ||
            requiredSlotKind > BroodmotherCompanionAiV2Contract.SlotKindSpit)
        {
            throw new ArgumentOutOfRangeException(nameof(requiredSlotKind));
        }

        this.baseCost = baseCost;
        this.preferredDistanceBand = preferredDistanceBand;
        this.requiredSlotKind = requiredSlotKind;
        this.leap = leap;
        this.spit = spit;
        this.finisher = finisher;
    }

    public float Evaluate(ActionCostContext context)
    {
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var cost = baseCost;
        var distanceBand = ReadRequiredFact(context, BroodmotherCompanionAiV2Contract.FactDistanceBand);
        var distanceDelta = Math.Abs(distanceBand - preferredDistanceBand);
        if (distanceDelta == 1)
        {
            cost += 3f;
        }
        else if (distanceDelta >= 2)
        {
            cost += 6f;
        }

        if (requiredSlotKind != BroodmotherCompanionAiV2Contract.SlotKindNone)
        {
            var slotGranted = ReadRequiredFact(context, BroodmotherCompanionAiV2Contract.FactSlotGranted);
            var slotKind = ReadRequiredFact(context, BroodmotherCompanionAiV2Contract.FactSlotKind);
            if (slotGranted != 1 || slotKind != requiredSlotKind)
            {
                cost += 8f;
            }
        }

        if (leap && ReadRequiredFact(context, BroodmotherCompanionAiV2Contract.FactAngleBand) ==
            BroodmotherCompanionAiV2Contract.AngleBandFront)
        {
            cost += 4f;
        }

        if (spit && ReadRequiredFact(context, BroodmotherCompanionAiV2Contract.FactRangedLaneClear) != 1)
        {
            cost += 5f;
        }

        if (finisher && ReadRequiredFact(context, BroodmotherCompanionAiV2Contract.FactTargetWeak) != 1)
        {
            cost += 4f;
        }

        if (float.IsNaN(cost) || float.IsInfinity(cost) || cost < 0f)
        {
            throw new InvalidOperationException("broodmother-action-cost-invalid");
        }

        return cost;
    }

    private static int ReadRequiredFact(ActionCostContext context, PlanningFactId id)
    {
        var found = false;
        var value = 0;
        for (var index = 0; index < context.Facts.Count; index++)
        {
            if (context.Facts[index].Id != id)
            {
                continue;
            }

            if (found)
            {
                throw new InvalidOperationException("broodmother-planning-fact-duplicate");
            }

            found = true;
            value = context.Facts[index].Value;
        }

        if (!found)
        {
            throw new InvalidOperationException("broodmother-planning-fact-missing");
        }

        return value;
    }
}

public sealed class BroodmotherRecoveryActionCostProvider : IAvalonActionCostProvider
{
    public float Evaluate(ActionCostContext context)
    {
        _ = context ?? throw new ArgumentNullException(nameof(context));

        if (ReadRequiredFact(context, BroodmotherCompanionAiV2Contract.FactRecoveryRequired) == 1)
        {
            return 0.5f;
        }

        return ReadRequiredFact(context, BroodmotherCompanionAiV2Contract.FactRoleCode) switch
        {
            BroodmotherCompanionAiV2Contract.CrimsonVanguardRoleCode => 1f,
            BroodmotherCompanionAiV2Contract.PalePouncerRoleCode => 1.5f,
            BroodmotherCompanionAiV2Contract.GildedSpitterRoleCode => 1f,
            BroodmotherCompanionAiV2Contract.CrimsonHarrierRoleCode => 0.8f,
            BroodmotherCompanionAiV2Contract.PaleAmbusherRoleCode => 1.2f,
            BroodmotherCompanionAiV2Contract.GildedFinisherRoleCode => 0.8f,
            _ => throw new InvalidOperationException("broodmother-recovery-role-invalid"),
        };
    }

    private static int ReadRequiredFact(ActionCostContext context, PlanningFactId id)
    {
        var found = false;
        var value = 0;
        for (var index = 0; index < context.Facts.Count; index++)
        {
            if (context.Facts[index].Id != id)
            {
                continue;
            }

            if (found)
            {
                throw new InvalidOperationException("broodmother-planning-fact-duplicate");
            }

            found = true;
            value = context.Facts[index].Value;
        }

        if (!found)
        {
            throw new InvalidOperationException("broodmother-planning-fact-missing");
        }

        return value;
    }
}
