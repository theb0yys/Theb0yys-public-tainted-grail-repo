using System;
using System.Collections.Generic;
using AvalonAI.Contracts.V2;

namespace TGCommunity.AiPackageContracts;

public sealed class ExampleAiPackage : IAvalonAiPackage
{
    public ExampleAiPackage()
    {
        Manifest = new AvalonAiPackageManifest(
            new PackageId("community.example.infrastructure-ai"),
            "Community Infrastructure AI Example",
            "0.1.0",
            new AvalonAiApiVersion(2, 0),
            Array.Empty<GoalId>(),
            Array.Empty<ActionId>(),
            Array.Empty<ActionCapability>(),
            "community.example.infrastructure-ai",
            1,
            Array.Empty<BlackboardKeyDeclaration>(),
            Array.Empty<ActorRoleId>(),
            TimeSpan.FromSeconds(1),
            Array.Empty<BlackboardKeyDeclaration>());

        GoalPolicies = Array.Empty<IAvalonGoalPolicy>();
        GoalDefinitions = Array.Empty<AvalonGoalDefinition>();
        ActionDefinitions = Array.Empty<AvalonActionDefinition>();
    }

    public AvalonAiPackageManifest Manifest { get; }

    public IReadOnlyList<IAvalonGoalPolicy> GoalPolicies { get; }

    public IReadOnlyList<AvalonGoalDefinition> GoalDefinitions { get; }

    public IReadOnlyList<AvalonActionDefinition> ActionDefinitions { get; }
}
