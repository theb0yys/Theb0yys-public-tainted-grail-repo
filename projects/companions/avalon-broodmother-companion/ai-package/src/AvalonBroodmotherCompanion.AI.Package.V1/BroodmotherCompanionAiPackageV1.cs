using System;
using System.Collections.Generic;
using AvalonAI.Contracts.V2;

namespace AvalonBroodmotherCompanion.AI.Package.V1;

public sealed class BroodmotherCompanionAiPackageV1 : IAvalonAiPackage
{
    private readonly IReadOnlyList<IAvalonGoalPolicy> goalPolicies;

    public BroodmotherCompanionAiPackageV1()
    {
        Manifest = new AvalonAiPackageManifest(
            BroodmotherCompanionAiV1Contract.PackageId,
            BroodmotherCompanionAiV1Contract.DisplayName,
            BroodmotherCompanionAiV1Contract.PackageVersion,
            AvalonAiContracts.ApiVersion,
            BroodmotherCompanionAiV1Contract.GoalIds,
            BroodmotherCompanionAiV1Contract.ActionIds,
            BroodmotherCompanionAiV1Contract.RequiredCapabilities,
            BroodmotherCompanionAiV1Contract.BlackboardNamespace,
            BroodmotherCompanionAiV1Contract.BlackboardSchemaVersion,
            Array.Empty<BlackboardKeyDeclaration>(),
            new[]
            {
                BroodmotherCompanionAiV1Contract.BroodmotherActorRoleId,
                BroodmotherCompanionAiV1Contract.SpiderActorRoleId,
            },
            TimeSpan.FromMilliseconds(150),
            Array.Empty<BlackboardKeyDeclaration>(),
            Array.Empty<AvalonProcedureRequirement>());

        goalPolicies = Array.AsReadOnly<IAvalonGoalPolicy>(
            new IAvalonGoalPolicy[]
            {
                new BroodmotherCompanionLiveAttackerGoalPolicy(),
            });
    }

    public AvalonAiPackageManifest Manifest { get; }

    public IReadOnlyList<IAvalonGoalPolicy> GoalPolicies => goalPolicies;

    public IReadOnlyList<AvalonGoalDefinition> GoalDefinitions => BroodmotherCompanionAiV1Contract.GoalDefinitions;

    public IReadOnlyList<AvalonActionDefinition> ActionDefinitions => BroodmotherCompanionAiV1Contract.ActionDefinitions;
}
