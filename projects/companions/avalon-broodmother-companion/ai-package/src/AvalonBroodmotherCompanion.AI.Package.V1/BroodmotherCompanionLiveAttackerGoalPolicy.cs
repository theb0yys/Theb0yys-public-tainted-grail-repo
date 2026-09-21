using System;
using System.Collections.Generic;
using AvalonAI.Contracts.V2;

namespace AvalonBroodmotherCompanion.AI.Package.V1;

public sealed class BroodmotherCompanionLiveAttackerGoalPolicy : IAvalonGoalPolicy
{
    private static readonly IReadOnlyList<GoalRequest> NoGoals = Array.AsReadOnly(Array.Empty<GoalRequest>());
    private static readonly IReadOnlyList<GoalRequest> PredatorPressureGoal = Array.AsReadOnly(
        new[]
        {
            new GoalRequest(
                BroodmotherCompanionAiV1Contract.GoalPredatorPressure,
                0,
                0,
                TimeSpan.Zero,
                "broodmother-live-hero-attacker-candidate"),
        });

    public IReadOnlyList<GoalRequest> Evaluate(
        ActorSnapshot actor,
        IAvalonBlackboardReader blackboard)
    {
        _ = actor ?? throw new ArgumentNullException(nameof(actor));
        _ = blackboard ?? throw new ArgumentNullException(nameof(blackboard));

        if (!IsSupportedRole(actor.Role) ||
            !blackboard.TryRead(
                BroodmotherCompanionAiV1Contract.LiveHeroAttackerCandidate,
                out bool candidate) ||
            !candidate)
        {
            return NoGoals;
        }

        return PredatorPressureGoal;
    }

    private static bool IsSupportedRole(ActorRoleId role)
    {
        return role == BroodmotherCompanionAiV1Contract.BroodmotherActorRoleId ||
               role == BroodmotherCompanionAiV1Contract.SpiderActorRoleId;
    }
}
