using System;
using System.Collections.Generic;
using AvalonAI.Contracts.V2;

namespace AvalonCompanions.AI.Package.V2;

public sealed class AvalonCompanionCatchUpGoalPolicy : IAvalonGoalPolicy
{
    private static readonly IReadOnlyList<GoalRequest> NoGoals = Array.AsReadOnly(Array.Empty<GoalRequest>());
    private static readonly IReadOnlyList<GoalRequest> CatchUpGoal = Array.AsReadOnly(
        new[]
        {
            new GoalRequest(
                AvalonCompanionCatchUpV2Contract.GoalId,
                0,
                0,
                TimeSpan.Zero,
                AvalonCompanionCatchUpV2Contract.GoalReasonCode),
        });

    public IReadOnlyList<GoalRequest> Evaluate(
        ActorSnapshot actor,
        IAvalonBlackboardReader blackboard)
    {
        _ = actor ?? throw new ArgumentNullException(nameof(actor));
        _ = blackboard ?? throw new ArgumentNullException(nameof(blackboard));

        if (actor.Role != AvalonCompanionCatchUpV2Contract.ActorRoleId
            || !blackboard.TryRead(AvalonCompanionCatchUpV2Contract.FollowCatchUpCandidate, out bool shouldCatchUp)
            || !shouldCatchUp)
        {
            return NoGoals;
        }

        return CatchUpGoal;
    }
}
