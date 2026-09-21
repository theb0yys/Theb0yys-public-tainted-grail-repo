using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AvalonAI.Contracts.V2;
using AvalonAI.Runtime.V2;
using DragonKnight.AI.Package.V2;

internal static class Program
{
    private static int Main()
    {
        Console.WriteLine(DragonKnightAiV2Contract.PackageMarkerLine);

        var fixtures = new (string Name, Action Run)[]
        {
            ("boss identity is exact", BossIdentityIsExact),
            ("manifest declares Dragon Knight Boss AI contract", ManifestDeclaresBossContract),
            ("actor lane rejects invalid ids", ActorLaneRejectsInvalidIds),
            ("ownership gate rejects unsafe decisions", OwnershipGateRejectsUnsafeDecisions),
            ("target policy rejects forbidden triggers", TargetPolicyRejectsForbiddenTriggers),
            ("Rabbit contract is session-only with no save writes", RabbitContractIsSessionOnly),
            ("Rabbit writes require explicit approval", RabbitWritesRequireApproval),
            ("planning contract declares goals and actions", PlanningContractDeclaresGoalsAndActions),
            ("planning actions carry costs cooldowns and no native calls", PlanningActionsCarryBoundedMetadata),
            ("required capabilities are exact", RequiredCapabilitiesAreExact),
            ("FoAHost bridge policy is default-off and bounded", FoAHostBridgePolicyIsBounded),
            ("boss phase policy is gated", BossPhasePolicyIsGated),
            ("combat request requires approved combat gate", CombatRequestRequiresGate),
            ("package registers with Runtime V2 offline when capabilities are supplied", PackageRegistersOffline),
            ("package assembly remains Contracts V2 only", AssemblyBoundaryIsGuarded),
        };

        var failures = new List<string>();
        foreach (var fixture in fixtures)
        {
            try
            {
                fixture.Run();
                Console.WriteLine("PASS " + fixture.Name);
            }
            catch (Exception exception)
            {
                failures.Add(fixture.Name + ": " + exception.Message);
                Console.WriteLine("FAIL " + fixture.Name + ": " + exception);
            }
        }

        if (failures.Count == 0)
        {
            Console.WriteLine("All " + fixtures.Length + " Dragon Knight Boss AI package fixtures passed.");
            return 0;
        }

        Console.WriteLine(failures.Count + " fixture(s) failed:");
        foreach (var failure in failures)
        {
            Console.WriteLine(" - " + failure);
        }

        return 1;
    }

    private static void BossIdentityIsExact()
    {
        Check(DragonKnightAiV2Contract.PackageIdValue == "dragon-knight.boss.ai.v2", "Package ID must be exact.");
        Check(DragonKnightAiV2Contract.AssemblyNameValue == "DragonKnight.AI.Package.V2", "Assembly name must be exact.");
        Check(DragonKnightAiV2Contract.BossActorRoleIdValue == "dragon-knight.boss", "Role ID must be exact.");
        Check(DragonKnightAiV2Contract.SupportedContractVersion == "2.0", "Supported contract version must be exact.");
        Check(DragonKnightAiV2Contract.PackageMarkerString == "DRAGON_KNIGHT_AIR49_AUTHORITY_PACKET_PASS", "Marker string must be exact.");
        Check(DragonKnightAiV2Contract.BossName == "Sir Vaelor, the Ashen Dragon Knight", "Boss name must be exact.");
        Check(DragonKnightAiV2Contract.NpcTemplateGuid == "00f608ee051b57748a6a9ed8dae28678", "NpcTemplate GUID must be exact.");
        Check(DragonKnightAiV2Contract.LocationTemplateGuid == "d7b09116519f7564593be62781bee3db", "LocationTemplate GUID must be exact.");
    }

    private static void ManifestDeclaresBossContract()
    {
        var package = new DragonKnightAiPackageV2();
        var manifest = package.Manifest;

        Check(manifest.Id == DragonKnightAiV2Contract.PackageId, "The package ID must remain stable.");
        Check(manifest.DisplayName == DragonKnightAiV2Contract.DisplayName, "The display name must identify the Boss AI lane.");
        Check(manifest.PackageVersion == DragonKnightAiV2Contract.PackageVersion, "The package version must be explicit.");
        Check(manifest.RequiredRuntimeApi == AvalonAiContracts.ApiVersion, "The package must target Contracts V2.");
        Check(manifest.BlackboardNamespace == DragonKnightAiV2Contract.BlackboardNamespace, "The package must own the Dragon Knight boss namespace.");
        Check(manifest.BlackboardSchemaVersion == DragonKnightAiV2Contract.BlackboardSchemaVersion, "The schema version must be exact.");
        Check(manifest.SupportedActorRoles.SequenceEqual(new[] { DragonKnightAiV2Contract.BossActorRoleId }), "Only the Dragon Knight boss role may be supported.");
        Check(manifest.MaximumPolicyCadence == TimeSpan.FromMilliseconds(250), "The package cadence must stay bounded.");
        Check(manifest.PersistentKeys.Count == 0, "The package must not request persistence.");
        Check(package.GoalDefinitions.Count == DragonKnightAiV2Contract.GoalIds.Count, "Every manifest goal must have a definition.");
        Check(package.ActionDefinitions.Count == DragonKnightAiV2Contract.ActionIds.Count, "Every manifest action must have a definition.");
    }

    private static void ActorLaneRejectsInvalidIds()
    {
        var valid = ValidInput();
        CheckAccepted(DragonKnightAiV2Contract.ValidateActor(valid), "Valid live-location actor ID must pass actor lane.");
        Check(DragonKnightAiV2Contract.ToActorId("DK-LOCATION-1") == "foa.location:DK-LOCATION-1", "ActorId must derive from live Location.ID.");

        CheckReason(DragonKnightAiV2Contract.ValidateActor(With(valid, actorId: "")), "dragon-knight.boss.actor-id-missing");
        CheckReason(DragonKnightAiV2Contract.ValidateActor(With(valid, actorId: "00f608ee051b57748a6a9ed8dae28678")), "dragon-knight.boss.actor-id-malformed");
        CheckReason(DragonKnightAiV2Contract.ValidateActor(With(valid, stale: true)), "dragon-knight.boss.actor-id-stale");
        CheckReason(DragonKnightAiV2Contract.ValidateActor(With(valid, duplicate: true)), "dragon-knight.boss.actor-id-duplicate");
        CheckReason(DragonKnightAiV2Contract.ValidateActor(With(valid, roleId: "dragon-knight.companion")), "dragon-knight.boss.actor-role-mismatch");
    }

    private static void OwnershipGateRejectsUnsafeDecisions()
    {
        var valid = ValidInput();
        CheckAccepted(DragonKnightAiV2Contract.ValidateDecision(valid), "Valid authority context must pass.");

        CheckReason(DragonKnightAiV2Contract.ValidateDecision(With(valid, packageEnabled: false)), "dragon-knight.boss.default-off-disabled");
        CheckReason(DragonKnightAiV2Contract.ValidateDecision(With(valid, killSwitch: true)), "dragon-knight.boss.kill-switch-active");
        CheckReason(DragonKnightAiV2Contract.ValidateDecision(With(valid, leaseId: "")), "dragon-knight.boss.lease-missing");
        CheckReason(DragonKnightAiV2Contract.ValidateDecision(With(valid, leaseExpired: true)), "dragon-knight.boss.lease-expired");
        CheckReason(DragonKnightAiV2Contract.ValidateDecision(With(valid, ownerId: "dragon-knight.dk4.diagnostic-host")), "dragon-knight.boss.owner-mismatch");
    }

    private static void TargetPolicyRejectsForbiddenTriggers()
    {
        var valid = ValidInput();
        CheckAccepted(DragonKnightAiV2Contract.ValidateTarget(valid), "Valid location target must pass.");
        CheckAccepted(DragonKnightAiV2Contract.ValidateTarget(With(valid, targetKind: DragonKnightAiV2Contract.PlayerTargetKind, targetId: "player:hero")), "Player target must be allowed as a target descriptor.");

        CheckReason(DragonKnightAiV2Contract.ValidateTarget(With(valid, targetKind: DragonKnightAiV2Contract.LocationTargetKind, targetId: "template:d7b09116519f7564593be62781bee3db")), "dragon-knight.boss.target-id-malformed");
        CheckReason(DragonKnightAiV2Contract.ValidateTarget(With(valid, targetKind: DragonKnightAiV2Contract.CompanionTargetKind, targetId: "foa.location:COMPANION")), "dragon-knight.boss.target-companion-blocked");
        CheckReason(DragonKnightAiV2Contract.ValidateTarget(With(valid, targetKind: DragonKnightAiV2Contract.ShrineTargetKind, targetId: "foa.location:SHRINE")), "dragon-knight.boss.target-forbidden-trigger");
        CheckReason(DragonKnightAiV2Contract.ValidateTarget(With(valid, targetKind: DragonKnightAiV2Contract.DiscoveryTargetKind, targetId: DragonKnightAiV2Contract.ForbiddenDiscoveryTrigger)), "dragon-knight.boss.target-forbidden-trigger");
        CheckReason(DragonKnightAiV2Contract.ValidateTarget(With(valid, targetKind: "position", targetId: "x:0,y:0,z:0")), "dragon-knight.boss.target-kind-invalid");
    }

    private static void RabbitContractIsSessionOnly()
    {
        var manifest = new DragonKnightAiPackageV2().Manifest;
        string[] expected =
        {
            "actor.id",
            "actor.location_id",
            "actor.owner_id",
            "actor.lease_id",
            "target.id",
            "target.location_id",
            "arena.center",
            "arena.outer_wake_radius_m",
            "arena.inner_fight_radius_m",
            "arena.soft_leash_radius_m",
            "arena.hard_leash_radius_m",
            "encounter.state",
            "encounter.phase",
            "combat.hp_fraction",
            "capability.state",
            "kill_switch.active",
        };

        Check(manifest.BlackboardNamespace == "dragon_knight.boss.vaelor.v1", "Rabbit namespace must be exact.");
        Check(DragonKnightAiV2Contract.RabbitPersistence == "session-only", "Rabbit persistence must be session-only.");
        Check(!DragonKnightAiV2Contract.SaveWrites, "Save writes must be disabled.");
        Check(!DragonKnightAiV2Contract.RabbitBypass, "Rabbit bypass must be disabled.");
        Check(manifest.PersistentKeys.Count == 0, "No save-backed keys are allowed.");
        CheckSet(expected, manifest.BlackboardKeys.Select(key => key.Name), "Rabbit keys must be exact.");
        Check(manifest.BlackboardKeys.All(key => key.Access == BlackboardAccess.PackageLocal), "Current Contracts V2 manifests may declare package-local keys only.");
    }

    private static void RabbitWritesRequireApproval()
    {
        CheckReason(DragonKnightAiV2Contract.ValidateRabbitSessionWrite(approved: false), "dragon-knight.boss.rabbit-write-unapproved");
        CheckAccepted(DragonKnightAiV2Contract.ValidateRabbitSessionWrite(approved: true), "Approved session write gate must pass.");
    }

    private static void PlanningContractDeclaresGoalsAndActions()
    {
        string[] goals =
        {
            "dragon-knight.boss.validate-actor",
            "dragon-knight.boss.validate-target",
            "dragon-knight.boss.maintain-lease",
            "dragon-knight.boss.awaken-presentation",
            "dragon-knight.boss.engage-target",
            "dragon-knight.boss.maintain-leash",
            "dragon-knight.boss.phase-two-transition",
            "dragon-knight.boss.fail-closed-stop",
        };
        string[] actions =
        {
            "dragon-knight.boss.observe-actor",
            "dragon-knight.boss.observe-target",
            "dragon-knight.boss.face-target",
            "dragon-knight.boss.approach-target",
            "dragon-knight.boss.melee-attack-request",
            "dragon-knight.boss.phase-transition-request",
            "dragon-knight.boss.leash-return-request",
            "dragon-knight.boss.stop-all-request",
        };

        CheckSet(goals, DragonKnightAiV2Contract.GoalIds.Select(id => id.Value), "GOAP goals must be exact.");
        CheckSet(actions, DragonKnightAiV2Contract.ActionIds.Select(id => id.Value), "GOAP actions must be exact.");
        Check(DragonKnightAiV2Contract.GoalDefinitions.All(goal => goal.DesiredState.Count == 1), "Each goal must have explicit desired state.");
        Check(DragonKnightAiV2Contract.ActionDefinitions.All(action => action.Conditions is not null && action.Effects is not null), "Each action must carry preconditions/effects collections.");
    }

    private static void PlanningActionsCarryBoundedMetadata()
    {
        var actions = DragonKnightAiV2Contract.PlanningActions;

        Check(actions.Count == 8, "The planning contract must carry all eight actions.");
        Check(actions.All(action => action.Cost >= 1), "Every action must carry a non-zero cost.");
        Check(actions.All(action => action.Cooldown >= TimeSpan.Zero), "Every action must carry cooldown metadata.");
        Check(actions.All(action => !string.IsNullOrWhiteSpace(action.Preconditions)), "Every action must carry preconditions.");
        Check(actions.All(action => !string.IsNullOrWhiteSpace(action.Effects)), "Every action must carry effects.");
        Check(actions.All(action => !string.IsNullOrWhiteSpace(action.FailureBehavior)), "Every action must carry failure behavior.");
        Check(actions.All(action => !action.DirectNativeCalls), "No package action may call native APIs directly.");
    }

    private static void RequiredCapabilitiesAreExact()
    {
        string[] capabilities =
        {
            "observe_actor",
            "observe_target",
            "face_target",
            "approach_target",
            "melee_attack_request",
            "phase_transition_request",
            "leash_return_request",
            "stop_all_request",
        };

        CheckSet(capabilities, DragonKnightAiV2Contract.RequiredCapabilities.Select(capability => capability.Value), "Required capabilities must be exact.");
        CheckSet(capabilities, new DragonKnightAiPackageV2().Manifest.RequiredCapabilities.Select(capability => capability.Value), "Manifest capabilities must be exact.");
    }

    private static void FoAHostBridgePolicyIsBounded()
    {
        Check(DragonKnightAiV2Contract.DefaultConfigKey == "DragonKnightBossAI.Enabled=false", "Default-off key must be exact.");
        Check(DragonKnightAiV2Contract.KillSwitchConfigKey == "DragonKnightBossAI.KillSwitch=true", "Kill-switch key must be exact.");
        Check(DragonKnightAiV2Contract.ActivationTrigger == "dragon-knight.encounter.inner-ring-entered", "Activation trigger must be exact.");
        Check(DragonKnightAiV2Contract.ApproachBridgeProcedure == "approach target", "Approach bridge procedure must be exact.");
        Check(DragonKnightAiV2Contract.FaceBridgeProcedure == "face target", "Face bridge procedure must be exact.");
        Check(DragonKnightAiV2Contract.InteractOrAttackBridgeProcedure == "interact/attack request only after approved combat gate", "Interact/attack bridge must stay combat-gated.");
        Check(DragonKnightAiV2Contract.ProcedureRequirements.Count == 1, "Only one runtime procedure requirement is allowed.");
        Check(DragonKnightAiV2Contract.ProcedureRequirements[0].Id.Value == "avalon.core.use-interactable.v1", "Only the use-interactable procedure is allowed.");
    }

    private static void BossPhasePolicyIsGated()
    {
        Check(DragonKnightAiV2Contract.PhaseOneMinimumHpFraction == 0.25f, "Phase 1 must run from 100% down to 25%.");
        Check(DragonKnightAiV2Contract.PhaseTwoTriggerHpFraction == 0.25f, "Phase 2 must trigger at 25%.");
        Check(DragonKnightAiV2Contract.RefillPolicy == "none", "Refill policy must be none.");
        Check(DragonKnightAiV2Contract.PhaseTransitionLockout == "one-shot-session-lock", "Phase transition lockout must be one-shot.");
        Check(!DragonKnightAiV2Contract.NativeControl, "The package must not take native control.");
        Check(!DragonKnightAiV2Contract.DirectNativeCalls, "The package must not call native APIs.");
    }

    private static void CombatRequestRequiresGate()
    {
        var valid = ValidInput();
        CheckReason(DragonKnightAiV2Contract.ValidateCombatRequest(valid), "dragon-knight.boss.combat-gate-missing");
        CheckAccepted(DragonKnightAiV2Contract.ValidateCombatRequest(With(valid, combatGate: true)), "Combat-gated request must pass when approved.");
    }

    private static void PackageRegistersOffline()
    {
        var runtime = new AvalonAiRuntimeV2(
            new NoObservationSource(),
            new NoBlackboardBackend(),
            new NoPlanningBackend(),
            Array.Empty<AvalonExecutorBinding>(),
            DragonKnightAiV2Contract.RequiredCapabilities,
            new AvalonAiRuntimeOptions(
                maximumActorObservationsPerTick: 1,
                maximumDerivedUpdatesPerTick: 1,
                maximumGoalPolicyEvaluationsPerTick: 1,
                maximumPlanResolutionsPerTick: 1,
                maximumGoalsPerPlan: 1,
                maximumPendingActionsPerActor: 1,
                maximumPollOperationsPerTick: 1,
                packageExceptionThreshold: 1,
                minimumReplanInterval: TimeSpan.Zero,
                contextIdleEviction: TimeSpan.FromMinutes(1),
                globalTimeBudget: TimeSpan.FromSeconds(1),
                perActorPlanningBudget: new PlanningBudget(TimeSpan.FromMilliseconds(10), 16)),
            new FixtureClock());

        PackageRegistrationResult result = runtime.RegisterPackage(new DragonKnightAiPackageV2());
        Check(result.Accepted, "The package must register with Runtime V2 offline when required capabilities are supplied.");
        Check(result.PackageId == DragonKnightAiV2Contract.PackageId, "Registration must preserve the package ID.");
    }

    private static void AssemblyBoundaryIsGuarded()
    {
        Assembly assembly = typeof(DragonKnightAiPackageV2).Assembly;
        string[] references = assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Check(references.Contains("AvalonAI.Contracts.V2"), "The package must reference Contracts V2.");
        Check(!references.Any(name => name.StartsWith("AvalonAI.Runtime", StringComparison.Ordinal)), "The package must not reference Runtime.");
        Check(!references.Any(name => name.StartsWith("AvalonAI.Blackboard", StringComparison.Ordinal)), "The package must not reference Rabbit.");
        Check(!references.Any(name => name.StartsWith("AvalonAI.Planning", StringComparison.Ordinal)), "The package must not reference GOAP implementation.");
        Check(!references.Any(name => name.StartsWith("AvalonAI.Execution", StringComparison.Ordinal)), "The package must not reference executors.");
        Check(!references.Any(name => name.StartsWith("Unity", StringComparison.Ordinal) || name == "BepInEx" || name.StartsWith("0Harmony", StringComparison.Ordinal)), "The package must not reference game or loader assemblies.");

        foreach (Type type in assembly.GetExportedTypes())
        {
            foreach (MemberInfo member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                Check(!MentionsForbiddenType(member), "The public package surface must expose only package, Contracts V2, and BCL types.");
            }
        }
    }

    private static DragonKnightBossDecisionInput ValidInput()
    {
        return new DragonKnightBossDecisionInput
        {
            PackageEnabled = true,
            KillSwitchActive = false,
            ActorId = "foa.location:DRAGON-KNIGHT-BOSS",
            RoleId = DragonKnightAiV2Contract.BossActorRoleIdValue,
            OwnerId = DragonKnightAiV2Contract.RequiredOwnerId,
            LeaseId = "lease:dragon-knight-boss",
            TargetKind = DragonKnightAiV2Contract.LocationTargetKind,
            TargetId = "foa.location:PLAYER-TARGET",
        };
    }

    private static DragonKnightBossDecisionInput With(
        DragonKnightBossDecisionInput input,
        bool? packageEnabled = null,
        bool? killSwitch = null,
        string? actorId = null,
        string? roleId = null,
        bool? stale = null,
        bool? duplicate = null,
        string? ownerId = null,
        string? leaseId = null,
        bool? leaseExpired = null,
        string? targetKind = null,
        string? targetId = null,
        bool? combatGate = null)
    {
        return new DragonKnightBossDecisionInput
        {
            PackageEnabled = packageEnabled ?? input.PackageEnabled,
            KillSwitchActive = killSwitch ?? input.KillSwitchActive,
            ActorId = actorId ?? input.ActorId,
            RoleId = roleId ?? input.RoleId,
            ActorIdStale = stale ?? input.ActorIdStale,
            ActorIdDuplicate = duplicate ?? input.ActorIdDuplicate,
            OwnerId = ownerId ?? input.OwnerId,
            LeaseId = leaseId ?? input.LeaseId,
            LeaseExpired = leaseExpired ?? input.LeaseExpired,
            TargetKind = targetKind ?? input.TargetKind,
            TargetId = targetId ?? input.TargetId,
            CompanionTargetsAllowed = input.CompanionTargetsAllowed,
            CombatGateApproved = combatGate ?? input.CombatGateApproved,
        };
    }

    private static bool MentionsForbiddenType(MemberInfo member)
    {
        IEnumerable<Type> types = member switch
        {
            MethodInfo method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType),
            ConstructorInfo constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType),
            PropertyInfo property => new[] { property.PropertyType },
            FieldInfo field => new[] { field.FieldType },
            EventInfo eventInfo when eventInfo.EventHandlerType is not null => new[] { eventInfo.EventHandlerType },
            _ => Array.Empty<Type>(),
        };

        return types.Any(type =>
            type.Namespace is not null
            && (type.Namespace.StartsWith("Unity", StringComparison.Ordinal)
                || type.Namespace.StartsWith("BepInEx", StringComparison.Ordinal)
                || type.Namespace.StartsWith("Harmony", StringComparison.Ordinal)
                || type.Namespace.StartsWith("Awaken", StringComparison.Ordinal)
                || type.Namespace.StartsWith("HutongGames", StringComparison.Ordinal)));
    }

    private static void CheckSet(
        IEnumerable<string> expected,
        IEnumerable<string> actual,
        string message)
    {
        string[] expectedArray = expected.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        string[] actualArray = actual.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        Check(expectedArray.SequenceEqual(actualArray, StringComparer.Ordinal), message);
    }

    private static void CheckAccepted(DragonKnightAuthorityDecision decision, string message)
    {
        Check(decision.Accepted, message + " Reason: " + decision.Reason);
    }

    private static void CheckReason(DragonKnightAuthorityDecision decision, string reason)
    {
        Check(!decision.Accepted, "Decision must reject with " + reason + ".");
        Check(decision.Reason == reason, "Expected reason " + reason + " but got " + decision.Reason + ".");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class NoObservationSource : IAvalonObservationSource
    {
        public ObservationCollectionResult Collect(ActorLease lease)
        {
            _ = lease;
            return new ObservationCollectionResult(ObservationCollectionStatus.Unavailable, null, "fixture-no-observation");
        }
    }

    private sealed class NoBlackboardBackend : IAvalonBlackboardBackend
    {
        public BlackboardActorOpenResult OpenActor(ActorLease lease, ActorRoleId role) => throw new NotSupportedException();
        public ObservationCommitResult CommitObservation(ActorLease lease, ObservationBatch batch, int maximumDerivedUpdates) => throw new NotSupportedException();
        public IAvalonPackageBlackboard GetPackageView(ActorLease lease, AvalonAiPackageManifest package) => throw new NotSupportedException();
        public PlanningStateSnapshot CapturePlanningState(ActorLease lease) => throw new NotSupportedException();
        public void RevokePackage(PackageId packageId) { }
        public void ReleaseActor(ActorLease lease) { }
    }

    private sealed class NoPlanningBackend : IAvalonPlanningBackend
    {
        public PlanResolution Resolve(PlanningRequest request) => throw new NotSupportedException();
        public void Cancel(PlanHandle plan, PlanCancelReason reason) { }
    }

    private sealed class FixtureClock : IAvalonRuntimeClock
    {
        public DateTimeOffset UtcNow => new DateTimeOffset(2026, 8, 2, 0, 0, 0, TimeSpan.Zero);
    }
}
