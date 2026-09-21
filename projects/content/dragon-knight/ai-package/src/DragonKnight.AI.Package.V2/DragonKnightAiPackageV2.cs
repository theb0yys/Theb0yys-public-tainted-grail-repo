using System;
using System.Collections.Generic;
using AvalonAI.Contracts.V2;

namespace DragonKnight.AI.Package.V2;

public sealed class DragonKnightAiPackageV2 : IAvalonAiPackage
{
    private readonly IReadOnlyList<IAvalonGoalPolicy> goalPolicies;

    public DragonKnightAiPackageV2()
    {
        Manifest = new AvalonAiPackageManifest(
            DragonKnightAiV2Contract.PackageId,
            DragonKnightAiV2Contract.DisplayName,
            DragonKnightAiV2Contract.PackageVersion,
            AvalonAiContracts.ApiVersion,
            DragonKnightAiV2Contract.GoalIds,
            DragonKnightAiV2Contract.ActionIds,
            DragonKnightAiV2Contract.RequiredCapabilities,
            DragonKnightAiV2Contract.BlackboardNamespace,
            DragonKnightAiV2Contract.BlackboardSchemaVersion,
            DragonKnightAiV2Contract.BlackboardKeys,
            new[]
            {
                DragonKnightAiV2Contract.BossActorRoleId,
            },
            TimeSpan.FromMilliseconds(250),
            DragonKnightAiV2Contract.PersistentKeys,
            DragonKnightAiV2Contract.ProcedureRequirements);

        goalPolicies = Array.AsReadOnly(Array.Empty<IAvalonGoalPolicy>());
    }

    public AvalonAiPackageManifest Manifest { get; }

    public IReadOnlyList<IAvalonGoalPolicy> GoalPolicies => goalPolicies;

    public IReadOnlyList<AvalonGoalDefinition> GoalDefinitions => DragonKnightAiV2Contract.GoalDefinitions;

    public IReadOnlyList<AvalonActionDefinition> ActionDefinitions => DragonKnightAiV2Contract.ActionDefinitions;
}
