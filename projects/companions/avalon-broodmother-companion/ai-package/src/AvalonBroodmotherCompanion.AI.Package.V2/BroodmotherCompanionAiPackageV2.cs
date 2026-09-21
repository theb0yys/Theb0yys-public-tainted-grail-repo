using System;
using System.Collections.Generic;
using AvalonAI.Contracts.V2;

namespace AvalonBroodmotherCompanion.AI.Package.V2;

public sealed class BroodmotherCompanionAiPackageV2 : IAvalonAiPackage
{
    private readonly IReadOnlyList<IAvalonGoalPolicy> goalPolicies;

    public BroodmotherCompanionAiPackageV2()
    {
        Manifest = new AvalonAiPackageManifest(
            BroodmotherCompanionAiV2Contract.PackageId,
            BroodmotherCompanionAiV2Contract.DisplayName,
            BroodmotherCompanionAiV2Contract.PackageVersion,
            AvalonAiContracts.ApiVersion,
            BroodmotherCompanionAiV2Contract.GoalIds,
            BroodmotherCompanionAiV2Contract.ActionIds,
            BroodmotherCompanionAiV2Contract.RequiredCapabilities,
            BroodmotherCompanionAiV2Contract.BlackboardNamespace,
            BroodmotherCompanionAiV2Contract.BlackboardSchemaVersion,
            BroodmotherCompanionAiV2Contract.PackageLocalKeyDeclarations,
            BroodmotherCompanionAiV2Contract.SupportedActorRoles,
            TimeSpan.FromMilliseconds(150),
            Array.Empty<BlackboardKeyDeclaration>(),
            BroodmotherCompanionAiV2Contract.ProcedureRequirements);

        goalPolicies = Array.AsReadOnly<IAvalonGoalPolicy>(new IAvalonGoalPolicy[]
        {
            new BroodmotherCompanionGoalPolicyV2(),
        });
    }

    public AvalonAiPackageManifest Manifest { get; }

    public IReadOnlyList<IAvalonGoalPolicy> GoalPolicies => goalPolicies;

    public IReadOnlyList<AvalonGoalDefinition> GoalDefinitions =>
        BroodmotherCompanionAiV2Contract.GoalDefinitions;

    public IReadOnlyList<AvalonActionDefinition> ActionDefinitions =>
        BroodmotherCompanionAiV2Contract.ActionDefinitions;
}
