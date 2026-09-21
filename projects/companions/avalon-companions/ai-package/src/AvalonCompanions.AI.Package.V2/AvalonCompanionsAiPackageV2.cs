using System;
using System.Collections.Generic;
using AvalonAI.Contracts.V2;

namespace AvalonCompanions.AI.Package.V2;

public sealed class AvalonCompanionsAiPackageV2 : IAvalonAiPackage
{
    public const string PackageVersion = "0.3.0";

    private readonly IReadOnlyList<IAvalonGoalPolicy> goalPolicies;
    private readonly IReadOnlyList<AvalonGoalDefinition> goalDefinitions;
    private readonly IReadOnlyList<AvalonActionDefinition> actionDefinitions;

    public AvalonCompanionsAiPackageV2()
    {
        var goal = new AvalonGoalDefinition(
            AvalonCompanionCatchUpV2Contract.GoalId,
            new[]
            {
                new PlanningCondition(
                    AvalonCompanionCatchUpV2Contract.CatchUpRecallDispatchedFactId,
                    PlanningComparison.Equal,
                    1),
            });
        var action = new AvalonActionDefinition(
            AvalonCompanionCatchUpV2Contract.ActionId,
            new[]
            {
                new PlanningCondition(
                    AvalonCompanionCatchUpV2Contract.CatchUpRequiredFactId,
                    PlanningComparison.Equal,
                    1),
            },
            new[]
            {
                new PlanningEffect(
                    AvalonCompanionCatchUpV2Contract.CatchUpRecallDispatchedFactId,
                    1),
            },
            default,
            new ConstantActionCostProvider(1),
            AvalonCompanionCatchUpV2Contract.Capability,
            InterruptPolicy.OnGoalChange,
            TimeSpan.FromSeconds(1));

        Manifest = new AvalonAiPackageManifest(
            AvalonCompanionCatchUpV2Contract.PackageId,
            "Avalon Companions catch-up",
            PackageVersion,
            AvalonAiContracts.ApiVersion,
            new[] { AvalonCompanionCatchUpV2Contract.GoalId },
            new[] { AvalonCompanionCatchUpV2Contract.ActionId },
            new[] { AvalonCompanionCatchUpV2Contract.Capability },
            AvalonCompanionCatchUpV2Contract.PackageIdValue,
            1,
            Array.Empty<BlackboardKeyDeclaration>(),
            new[] { AvalonCompanionCatchUpV2Contract.ActorRoleId },
            TimeSpan.FromSeconds(1),
            Array.Empty<BlackboardKeyDeclaration>());

        goalPolicies = Array.AsReadOnly<IAvalonGoalPolicy>(new IAvalonGoalPolicy[]
        {
            new AvalonCompanionCatchUpGoalPolicy(),
        });
        goalDefinitions = Array.AsReadOnly(new[] { goal });
        actionDefinitions = Array.AsReadOnly(new[] { action });
    }

    public AvalonAiPackageManifest Manifest { get; }

    public IReadOnlyList<IAvalonGoalPolicy> GoalPolicies => goalPolicies;

    public IReadOnlyList<AvalonGoalDefinition> GoalDefinitions => goalDefinitions;

    public IReadOnlyList<AvalonActionDefinition> ActionDefinitions => actionDefinitions;
}
