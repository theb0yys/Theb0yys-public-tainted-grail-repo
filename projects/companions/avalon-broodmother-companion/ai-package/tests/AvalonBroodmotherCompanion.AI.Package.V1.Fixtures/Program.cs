using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AvalonAI.Contracts.V2;
using AvalonAI.Runtime.V2;
using AvalonBroodmotherCompanion.AI.Package.V1;

internal static class Program
{
    private static int Main()
    {
        var fixtures = new (string Name, Action Run)[]
        {
            ("manifest declares the spider-family companion boundary", ManifestIsExact),
            ("safety constants keep package free of FoA/native/save ownership", SafetyBoundaryIsExplicit),
            ("goal policy requires exact live combat candidate evidence", GoalPolicyIsBounded),
            ("role and positioning rules match the professional spider design", RoleAndPositioningRulesAreExact),
            ("attack selection follows short bite, leap, feint, and fallback envelopes", AttackRulesAreExact),
            ("commit window requires telegraph and preserves ShortRange 16 policy", CommitRulesAreExact),
            ("production package registers with Runtime V2", ProductionPackageRegisters),
            ("package assembly remains Contracts-only", AssemblyBoundaryIsGuarded),
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
            Console.WriteLine(BroodmotherCompanionAiV1Contract.PackageMarkerLine);
            Console.WriteLine("All " + fixtures.Length + " Broodmother companion AI package fixtures passed.");
            return 0;
        }

        foreach (string failure in failures)
        {
            Console.WriteLine(" - " + failure);
        }

        return 1;
    }

    private static void ManifestIsExact()
    {
        var package = new BroodmotherCompanionAiPackageV1();
        AvalonAiPackageManifest manifest = package.Manifest;

        Check(manifest.Id == BroodmotherCompanionAiV1Contract.PackageId, "package id changed");
        Check(manifest.RequiredRuntimeApi == AvalonAiContracts.ApiVersion, "package must require Contracts V2");
        Check(manifest.PackageVersion == BroodmotherCompanionAiV1Contract.PackageVersion, "package version changed");
        Check(manifest.GoalIds.SequenceEqual(BroodmotherCompanionAiV1Contract.GoalIds), "goal ids changed");
        Check(manifest.ActionIds.SequenceEqual(BroodmotherCompanionAiV1Contract.ActionIds), "action ids changed");
        Check(manifest.RequiredCapabilities.SequenceEqual(BroodmotherCompanionAiV1Contract.RequiredCapabilities), "capabilities changed");
        Check(
            manifest.SupportedActorRoles.SequenceEqual(new[]
            {
                BroodmotherCompanionAiV1Contract.BroodmotherActorRoleId,
                BroodmotherCompanionAiV1Contract.SpiderActorRoleId,
            }),
            "actor roles changed");
        Check(manifest.BlackboardKeys.Count == 0, "package must not declare writable package-local keys yet");
        Check(manifest.PersistentKeys.Count == 0, "package must not declare persistent keys");
        Check(manifest.MaximumPolicyCadence == TimeSpan.FromMilliseconds(150), "policy cadence changed");
        Check(package.GoalDefinitions.Count == 1, "one package goal expected");
        Check(package.ActionDefinitions.Count == 8, "eight bounded actions expected");
        Check(package.GoalPolicies.Count == 1, "one goal policy expected");
    }

    private static void SafetyBoundaryIsExplicit()
    {
        Check(BroodmotherCompanionAiV1Contract.RequiredOwnerId == "kane.tgfoa.avalon-broodmother-companion", "owner id changed");
        Check(BroodmotherCompanionAiV1Contract.TargetSource == "native-combat-candidates.prioritized-live-only", "target source changed");
        Check(BroodmotherCompanionAiV1Contract.BroodmotherActorRoleIdValue == "avalon-broodmother-companion.broodmother", "broodmother actor role changed");
        Check(BroodmotherCompanionAiV1Contract.SpiderActorRoleIdValue == "avalon-broodmother-companion.spider", "spider actor role changed");
        Check(BroodmotherCompanionAiV1Contract.TargetTemplateName == "Spec_Broodmother_CI4", "target template changed");
        Check(BroodmotherCompanionAiV1Contract.TargetTemplateGuid == "527d142b35e81a648b6e84ea921ac543", "location template guid changed");
        Check(BroodmotherCompanionAiV1Contract.TargetNpcTemplateGuid == "7623d4729979e814ea64992d9a399e17", "npc template guid changed");
        Check(BroodmotherCompanionAiV1Contract.SpiderTargetTemplateName == "Spec_Spider_CI4", "spider target template changed");
        Check(BroodmotherCompanionAiV1Contract.SpiderTargetTemplateGuid == "bfcd6db0f66afc14c94fcb6a9dbde4e7", "spider location template guid changed");
        Check(BroodmotherCompanionAiV1Contract.SpiderTargetNpcTemplateGuid == "5f36e2717dc68e04091fe7b0aa8bafe0", "spider npc template guid changed");
        Check(BroodmotherCompanionAiV1Contract.ValidateAuthority(new BroodmotherSpiderCompanionAiInput()).Kind == BroodmotherSpiderAiDecisionKind.None, "broodmother identity must authorize");
        Check(
            BroodmotherCompanionAiV1Contract.ValidateAuthority(new BroodmotherSpiderCompanionAiInput
            {
                ActorRoleId = BroodmotherCompanionAiV1Contract.SpiderActorRoleIdValue,
                LocationTemplateName = BroodmotherCompanionAiV1Contract.SpiderTargetTemplateName,
                LocationTemplateGuid = BroodmotherCompanionAiV1Contract.SpiderTargetTemplateGuid,
                NpcTemplateGuid = BroodmotherCompanionAiV1Contract.SpiderTargetNpcTemplateGuid,
            }).Kind == BroodmotherSpiderAiDecisionKind.None,
            "spider identity must authorize");
        Check(
            BroodmotherCompanionAiV1Contract.ValidateAuthority(new BroodmotherSpiderCompanionAiInput
            {
                ActorRoleId = BroodmotherCompanionAiV1Contract.SpiderActorRoleIdValue,
            }).Kind == BroodmotherSpiderAiDecisionKind.FailClosed,
            "mixed spider role with broodmother template must fail closed");
        Check(BroodmotherCompanionAiV1Contract.AdvancedAiConfigKey == "CompanionAI.EnableAdvancedSpiderAI=true", "advanced AI config default changed");
        Check(BroodmotherCompanionAiV1Contract.LeapRequestsConfigKey == "CompanionAI.EnableLeapAttackRequests=true", "leap config default changed");
        Check(BroodmotherCompanionAiV1Contract.CombatPromptSecondsConfigKey == "CompanionAI.CombatPromptSeconds=1.25", "combat prompt config changed");
        Check(BroodmotherCompanionAiV1Contract.RuntimeTickSecondsConfigKey == "Runtime.TickSeconds=0.75", "runtime tick config changed");
        Check(BroodmotherCompanionAiV1Contract.MaxAttackSlotsDefault == 1, "max attack slot policy changed");
        Check(!BroodmotherCompanionAiV1Contract.DirectNativeCalls, "package must not make direct native calls");
        Check(!BroodmotherCompanionAiV1Contract.NativeControl, "package must not own native control");
        Check(!BroodmotherCompanionAiV1Contract.NativeJumpStateProven, "native jump state must remain unproven");
        Check(!BroodmotherCompanionAiV1Contract.SaveWrites, "package must not write saves");
        Check(!BroodmotherCompanionAiV1Contract.RandomSpawns, "package must not random spawn");
        Check(!BroodmotherCompanionAiV1Contract.Persistence, "package must not persist actors");
        Check(BroodmotherCompanionAiV1Contract.ShortRangeStateId == 16, "short range state changed");
        Check(BroodmotherCompanionAiV1Contract.ResolveShortRangeClip(BroodmotherSpiderAttackKind.LeapJump, false) == BroodmotherCompanionAiV1Contract.ShortRangeSecondaryClipName, "leap must resolve to secondary ShortRange clip");
        Check(BroodmotherCompanionAiV1Contract.JumpMotionPolicy == "motion-only-until-native-jump-state-proven", "jump policy changed");
    }

    private static void GoalPolicyIsBounded()
    {
        var policy = new BroodmotherCompanionLiveAttackerGoalPolicy();
        ActorSnapshot actor = Snapshot(BroodmotherCompanionAiV1Contract.BroodmotherActorRoleId);
        ActorSnapshot spiderActor = Snapshot(BroodmotherCompanionAiV1Contract.SpiderActorRoleId);

        Check(policy.Evaluate(actor, new FixtureBlackboard()).Count == 0, "missing evidence must fail closed");
        Check(policy.Evaluate(actor, new FixtureBlackboard(false)).Count == 0, "false candidate must not request a goal");
        Check(
            policy.Evaluate(Snapshot(new ActorRoleId("fixture.other-role")), new FixtureBlackboard(true)).Count == 0,
            "different actor role must not request a goal");

        IReadOnlyList<GoalRequest> goals = policy.Evaluate(actor, new FixtureBlackboard(true));
        Check(goals.Count == 1, "true live attacker evidence must request one goal");
        Check(goals[0].GoalId == BroodmotherCompanionAiV1Contract.GoalPredatorPressure, "wrong goal requested");
        Check(goals[0].BasePriority == 0 && goals[0].Urgency == 0, "package invented cross-package priority");
        Check(goals[0].MinimumCommitment == TimeSpan.Zero, "package invented a commitment lock");
        Check(policy.Evaluate(spiderActor, new FixtureBlackboard(true)).Count == 1, "spider actor role must use same bounded goal");
    }

    private static void RoleAndPositioningRulesAreExact()
    {
        Check(BroodmotherCompanionAiV1Contract.ChooseRole(new BroodmotherSpiderCompanionAiInput { HealthFraction = 0.19f }) == BroodmotherSpiderRole.Retreating, "low health must retreat");
        Check(BroodmotherCompanionAiV1Contract.ChooseRole(new BroodmotherSpiderCompanionAiInput { RecentHits = 3 }) == BroodmotherSpiderRole.Retreating, "repeated hits must retreat");
        Check(BroodmotherCompanionAiV1Contract.ChooseRole(new BroodmotherSpiderCompanionAiInput { TargetHealthFraction = 0.2f, TargetVisible = true }) == BroodmotherSpiderRole.Finisher, "weak visible target must use finisher");
        Check(BroodmotherCompanionAiV1Contract.ChooseRole(new BroodmotherSpiderCompanionAiInput { PackIndex = 0 }) == BroodmotherSpiderRole.Harasser, "pack index 0 role");
        Check(BroodmotherCompanionAiV1Contract.ChooseRole(new BroodmotherSpiderCompanionAiInput { PackIndex = 1 }) == BroodmotherSpiderRole.Flanker, "pack index 1 role");
        Check(BroodmotherCompanionAiV1Contract.ChooseRole(new BroodmotherSpiderCompanionAiInput { PackIndex = 2 }) == BroodmotherSpiderRole.Ambusher, "pack index 2 role");
        Check(BroodmotherCompanionAiV1Contract.ChooseRole(new BroodmotherSpiderCompanionAiInput { PackIndex = 3 }) == BroodmotherSpiderRole.Harasser, "pack index 3 role");

        BroodmotherSpiderPositionSelection flanker = BroodmotherCompanionAiV1Contract.SelectPosition(
            new BroodmotherSpiderCompanionAiInput { PackIndex = 1 },
            BroodmotherSpiderRole.Flanker);
        Check(Near(flanker.DesiredDistanceMeters, 3.25f), "flanker distance changed");
        Check(flanker.AvoidsStacking, "position must avoid stacking");

        BroodmotherSpiderPositionSelection ambusher = BroodmotherCompanionAiV1Contract.SelectPosition(
            new BroodmotherSpiderCompanionAiInput { PackIndex = 2 },
            BroodmotherSpiderRole.Ambusher);
        Check(Near(ambusher.DesiredDistanceMeters, 4.5f), "ambusher distance changed");

        BroodmotherSpiderPositionSelection camera = BroodmotherCompanionAiV1Contract.SelectPosition(
            new BroodmotherSpiderCompanionAiInput { CameraDataAvailable = true, CameraCenterWeight = 0.65f },
            BroodmotherSpiderRole.Harasser);
        Check(camera.AvoidsCameraCenter, "camera center avoidance must be recorded");
    }

    private static void AttackRulesAreExact()
    {
        BroodmotherSpiderAttackSlotDecision granted = BroodmotherCompanionAiV1Contract.CoordinateAttackSlot(new BroodmotherSpiderCompanionAiInput());
        Check(granted.CanAttack && granted.MaxSimultaneousAttackers == 1, "one-slot attack permission changed");

        BroodmotherSpiderAttackSelection fallback = BroodmotherCompanionAiV1Contract.ChooseAttack(
            new BroodmotherSpiderCompanionAiInput { TargetSpatialAvailable = false },
            BroodmotherSpiderRole.Harasser,
            granted);
        Check(fallback.Kind == BroodmotherSpiderAttackKind.TargetedNativeCombat, "spatial fallback must be targeted native combat");

        BroodmotherSpiderAttackSelection shortBite = BroodmotherCompanionAiV1Contract.ChooseAttack(
            new BroodmotherSpiderCompanionAiInput { TargetDistanceMeters = 2.25f, TargetAngleDegrees = 55f },
            BroodmotherSpiderRole.Harasser,
            granted);
        Check(shortBite.Kind == BroodmotherSpiderAttackKind.ShortBite, "short bite envelope changed");

        BroodmotherSpiderAttackSelection flankerLeap = BroodmotherCompanionAiV1Contract.ChooseAttack(
            new BroodmotherSpiderCompanionAiInput { TargetDistanceMeters = 5.5f, TargetAngleDegrees = 100f, PackIndex = 1 },
            BroodmotherSpiderRole.Flanker,
            granted);
        Check(flankerLeap.Kind == BroodmotherSpiderAttackKind.LeapJump, "flanker leap envelope changed");

        BroodmotherSpiderAttackSelection leap = BroodmotherCompanionAiV1Contract.ChooseAttack(
            new BroodmotherSpiderCompanionAiInput { TargetDistanceMeters = 6.0f, TargetAngleDegrees = 75f },
            BroodmotherSpiderRole.Harasser,
            granted);
        Check(leap.Kind == BroodmotherSpiderAttackKind.LeapJump, "mid-range leap envelope changed");

        BroodmotherSpiderAttackSelection feint = BroodmotherCompanionAiV1Contract.ChooseAttack(
            new BroodmotherSpiderCompanionAiInput { TargetDistanceMeters = 3.5f, TargetAngleDegrees = 120f },
            BroodmotherSpiderRole.Harasser,
            granted);
        Check(feint.Kind == BroodmotherSpiderAttackKind.Feint, "feint envelope changed");

        BroodmotherSpiderAttackSelection held = BroodmotherCompanionAiV1Contract.ChooseAttack(
            new BroodmotherSpiderCompanionAiInput { ActiveAttackers = 1 },
            BroodmotherSpiderRole.Harasser,
            BroodmotherCompanionAiV1Contract.CoordinateAttackSlot(new BroodmotherSpiderCompanionAiInput { ActiveAttackers = 1 }));
        Check(held.Kind == BroodmotherSpiderAttackKind.Hold, "denied slot must hold without density pressure");

        BroodmotherSpiderAttackSelection deniedSpatialFallback = BroodmotherCompanionAiV1Contract.ChooseAttack(
            new BroodmotherSpiderCompanionAiInput { ActiveAttackers = 1, TargetSpatialAvailable = false },
            BroodmotherSpiderRole.Harasser,
            BroodmotherCompanionAiV1Contract.CoordinateAttackSlot(new BroodmotherSpiderCompanionAiInput { ActiveAttackers = 1 }));
        Check(deniedSpatialFallback.Kind == BroodmotherSpiderAttackKind.Hold, "denied slot must not bypass into spatial fallback");
    }

    private static void CommitRulesAreExact()
    {
        BroodmotherSpiderAttackSelection shortBite = BroodmotherSpiderAttackSelection.Chosen(
            BroodmotherSpiderAttackKind.ShortBite,
            "fixture");
        Check(
            BroodmotherCompanionAiV1Contract.CommitAttackWindow(new BroodmotherSpiderCompanionAiInput(), shortBite).Kind == BroodmotherSpiderAiDecisionKind.TelegraphIntent,
            "committable attacks must telegraph before commit");

        Check(
            BroodmotherCompanionAiV1Contract.CommitAttackWindow(
                new BroodmotherSpiderCompanionAiInput { TelegraphAlreadyEmitted = true, ReleaseWindowOpen = false },
                shortBite).Kind == BroodmotherSpiderAiDecisionKind.Hold,
            "closed release window must hold");

        BroodmotherSpiderAiDecision valid = BroodmotherCompanionAiV1Contract.CommitAttackWindow(
            new BroodmotherSpiderCompanionAiInput
            {
                TelegraphAlreadyEmitted = true,
                ReleaseWindowOpen = true,
                TargetDistanceMeters = 2f,
                TargetAngleDegrees = 20f,
            },
            shortBite);
        Check(valid.Kind == BroodmotherSpiderAiDecisionKind.ShortRange16Request, "valid release must request ShortRange 16");

        BroodmotherSpiderCompanionAiEvaluation leap = BroodmotherCompanionAiV1Contract.Evaluate(
            new BroodmotherSpiderCompanionAiInput
            {
                PackIndex = 1,
                TargetDistanceMeters = 5f,
                TargetAngleDegrees = 80f,
                TelegraphAlreadyEmitted = true,
                ReleaseWindowOpen = true,
            });
        Check(leap.Attack.Kind == BroodmotherSpiderAttackKind.LeapJump, "evaluation must select leap");
        Check(leap.Decision.Kind == BroodmotherSpiderAiDecisionKind.ShortRange16Request, "leap hit must resolve through ShortRange 16");
        Check(leap.ShortRangeClip == BroodmotherCompanionAiV1Contract.ShortRangeSecondaryClipName, "leap must use secondary ShortRange clip");
        Check(leap.JumpMotionPolicy == BroodmotherCompanionAiV1Contract.JumpMotionPolicy, "jump motion policy changed");

        BroodmotherSpiderCompanionAiEvaluation reposition = BroodmotherCompanionAiV1Contract.Evaluate(
            new BroodmotherSpiderCompanionAiInput
            {
                TargetDistanceMeters = 7f,
                TargetAngleDegrees = 6f,
            });
        Check(reposition.Attack.Kind == BroodmotherSpiderAttackKind.Reposition, "mid-range target must request reposition pressure");
        Check(reposition.Decision.Kind == BroodmotherSpiderAiDecisionKind.NativeCombatHandoff, "reposition pressure must drive native combat handoff");

        BroodmotherSpiderCompanionAiEvaluation invalid = BroodmotherCompanionAiV1Contract.Evaluate(
            new BroodmotherSpiderCompanionAiInput { OwnerId = "fixture.wrong-owner" });
        Check(invalid.Decision.Kind == BroodmotherSpiderAiDecisionKind.FailClosed, "owner mismatch must fail closed");

        BroodmotherSpiderCompanionAiEvaluation noAttacker = BroodmotherCompanionAiV1Contract.Evaluate(
            new BroodmotherSpiderCompanionAiInput { LiveHeroAttackerCount = 0 });
        Check(noAttacker.Decision.Kind == BroodmotherSpiderAiDecisionKind.Hold, "missing live hero attacker must hold without choosing an attack");
        Check(noAttacker.Attack.Kind == BroodmotherSpiderAttackKind.Hold, "missing live attacker must not emit a combat attack");
    }

    private static void ProductionPackageRegisters()
    {
        var runtime = new AvalonAiRuntimeV2(
            new NoObservationSource(),
            new NoBlackboardBackend(),
            new NoPlanningBackend(),
            Array.Empty<AvalonExecutorBinding>(),
            BroodmotherCompanionAiV1Contract.RequiredCapabilities,
            new AvalonAiRuntimeOptions(
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(1),
                new PlanningBudget(TimeSpan.FromMilliseconds(10), 16)),
            new FixtureClock());

        PackageRegistrationResult result = runtime.RegisterPackage(new BroodmotherCompanionAiPackageV1());
        Check(result.Accepted, "runtime rejected the production package");
        Check(result.PackageId == BroodmotherCompanionAiV1Contract.PackageId, "registration package id changed");
    }

    private static void AssemblyBoundaryIsGuarded()
    {
        Assembly assembly = typeof(BroodmotherCompanionAiPackageV1).Assembly;
        string[] references = assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();
        Check(references.Contains("AvalonAI.Contracts.V2"), "package must reference Contracts V2");
        Check(!references.Any(name => name.StartsWith("AvalonAI.Runtime", StringComparison.Ordinal)), "package references Runtime");
        Check(!references.Any(name => name.StartsWith("AvalonAI.Blackboard", StringComparison.Ordinal)), "package references Rabbit");
        Check(!references.Any(name => name.StartsWith("AvalonAI.Planning", StringComparison.Ordinal)), "package references GOAP");
        Check(!references.Any(name => name.StartsWith("AvalonAI.Execution", StringComparison.Ordinal)), "package references execution modules");
        Check(!references.Any(name => name.StartsWith("Unity", StringComparison.Ordinal) || name == "BepInEx" || name.StartsWith("0Harmony", StringComparison.Ordinal)), "package references game or loader assemblies");
        Check(!references.Contains("AvalonBroodmotherCompanion"), "package references the live gameplay plugin");
    }

    private static ActorSnapshot Snapshot(ActorRoleId role)
    {
        return new ActorSnapshot(
            new ActorLease(
                new ActorId("fixture.broodmother"),
                new OwnershipLeaseId("fixture.lease"),
                ActorExecutionMode.NativeAssisted,
                1),
            role,
            1);
    }

    private static bool Near(float actual, float expected)
    {
        return Math.Abs(actual - expected) < 0.001f;
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class FixtureBlackboard : IAvalonBlackboardReader
    {
        private readonly bool hasValue;
        private readonly bool value;

        public FixtureBlackboard()
        {
        }

        public FixtureBlackboard(bool value)
        {
            hasValue = true;
            this.value = value;
        }

        public bool TryRead<T>(BlackboardKey<T> key, out T result)
        {
            if (hasValue &&
                key.Equals(BroodmotherCompanionAiV1Contract.LiveHeroAttackerCandidate) &&
                typeof(T) == typeof(bool))
            {
                result = (T)(object)value;
                return true;
            }

            result = default!;
            return false;
        }

        public IDisposable Observe<T>(BlackboardKey<T> key, Action<BlackboardChange<T>> callback)
        {
            _ = key;
            _ = callback ?? throw new ArgumentNullException(nameof(callback));
            return NoOpDisposable.Instance;
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
        public DateTimeOffset UtcNow => new DateTimeOffset(2026, 8, 6, 12, 0, 0, TimeSpan.Zero);
    }

    private sealed class NoOpDisposable : IDisposable
    {
        public static NoOpDisposable Instance { get; } = new NoOpDisposable();
        public void Dispose() { }
    }
}
