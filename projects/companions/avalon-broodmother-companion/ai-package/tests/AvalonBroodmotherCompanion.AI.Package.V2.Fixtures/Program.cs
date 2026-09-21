using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AvalonAI.Contracts.V2;
using AvalonBroodmotherCompanion.AI.Package.V2;

internal static class Program
{
    private static readonly ActorLease Lease = new ActorLease(
        new ActorId("fixture.broodmother-spider"),
        new OwnershipLeaseId("fixture.broodmother-spider.lease"),
        ActorExecutionMode.BlazeOwned,
        7);

    private static int Main()
    {
        var fixtures = new (string Name, Action Run)[]
        {
            ("manifest exposes six stable roles and the exact Rabbit schema", ManifestAndRabbitSchemaAreExact),
            ("goal policy selects exact specialized goals and emergency precedence", GoalPolicyIsSpecialized),
            ("Rabbit planning projector fails closed and emits exact facts", RabbitProjectorIsExact),
            ("GOAP resolves only each variant's intended tactic chain", SixRoleGoapPlansAreExact),
            ("leap attack effects force pouncer and ambusher to re-flank", LeapAttacksForceReflank),
            ("dynamic action costs apply the researched modifiers", ActionCostsAreExact),
            ("replanning rules are bounded to the researched triggers", ReplanningRulesAreExact),
            ("authored Spider animation VFX and command asset handles are complete", AuthoredSpiderAssetHandlesAreComplete),
            ("V2 has no primitive evaluator or non-contract production dependency", AssemblyBoundaryIsExact),
            ("Broodmother plugin routes V2 PlayMaker host bridge source path", BroodmotherPluginRoutesV2PlayMakerHostBridge),
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
            Console.WriteLine("BROODMOTHER_AI_PACKAGE_V2_PASS fixtures=" + fixtures.Length + " roles=6 rabbit=1 goap=1 static-fallback=0");
            return 0;
        }

        Console.WriteLine(failures.Count + " fixture(s) failed:");
        foreach (var failure in failures) Console.WriteLine(" - " + failure);
        return 1;
    }

    private static void ManifestAndRabbitSchemaAreExact()
    {
        var package = new BroodmotherCompanionAiPackageV2();
        var manifest = package.Manifest;

        Check(manifest.Id == BroodmotherCompanionAiV2Contract.PackageId, "package ID changed");
        Check(manifest.RequiredRuntimeApi == AvalonAiContracts.ApiVersion, "Contracts API changed");
        Check(manifest.BlackboardNamespace == BroodmotherCompanionAiV2Contract.BlackboardNamespace, "package Rabbit namespace changed");
        Check(manifest.BlackboardSchemaVersion == 2, "Rabbit schema version changed");
        Check(manifest.MaximumPolicyCadence == TimeSpan.FromMilliseconds(150), "policy cadence changed");
        Check(manifest.SupportedActorRoles.SequenceEqual(BroodmotherCompanionAiV2Contract.SupportedActorRoles), "role order changed");
        Check(manifest.SupportedActorRoles.Count == 6, "exactly six roles are required");
        Check(BroodmotherCompanionAiV2Contract.RoleDefinitions.Select(role => role.RoleId).SequenceEqual(manifest.SupportedActorRoles), "role definitions do not match the manifest");
        Check(BroodmotherCompanionAiV2Contract.RoleDefinitions.Select(role => role.RoleCode).SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 }), "role codes changed");
        Check(BroodmotherCompanionAiV2Contract.RoleDefinitions.Select(role => role.DisplayName).SequenceEqual(new[]
        {
            "Broodmother — Crimson Vanguard",
            "Broodmother — Pale Pouncer",
            "Broodmother — Gilded Spitter",
            "Crimson Harrier Spider",
            "Pale Ambusher Spider",
            "Gilded Finisher Spider",
        }), "distinctive role names changed");
        Check(!manifest.SupportedActorRoles.Any(role => role.Value == "avalon-broodmother-companion.broodmother" || role.Value == "avalon-broodmother-companion.spider"), "legacy generic role accepted");
        Check(manifest.GoalIds.Count == 10, "goal count changed");
        Check(manifest.ActionIds.Count == 15, "action count changed");
        Check(manifest.ProcedureRequirements.Count == 8, "procedure requirement count changed");
        Check(manifest.PersistentKeys.Count == 0, "V2 must remain session-only");

        CheckSchema(
            BroodmotherCompanionAiV2Contract.HostAuthoritativeKeyDeclarations,
            23,
            BroodmotherCompanionAiV2Contract.HostBlackboardNamespace,
            BlackboardAccess.Authoritative);
        CheckSchema(
            BroodmotherCompanionAiV2Contract.DerivedKeyDeclarations,
            9,
            BroodmotherCompanionAiV2Contract.DerivedBlackboardNamespace,
            BlackboardAccess.Derived);
        CheckSchema(
            BroodmotherCompanionAiV2Contract.PackageLocalKeyDeclarations,
            16,
            BroodmotherCompanionAiV2Contract.BlackboardNamespace,
            BlackboardAccess.PackageLocal);
        Check(manifest.BlackboardKeys.SequenceEqual(BroodmotherCompanionAiV2Contract.PackageLocalKeyDeclarations), "manifest must own exactly the local Rabbit keys");

        var hostNames = BroodmotherCompanionAiV2Contract.HostAuthoritativeKeyDeclarations.Select(key => key.Name).ToHashSet(StringComparer.Ordinal);
        Check(hostNames.SetEquals(new[]
        {
            "lifecycle.active", "lifecycle.cleanup_requested", "lease.valid", "executor.blaze_owned",
            "self.health_band", "self.damage_revision", "target.valid", "target.id",
            "target.distance_band", "target.angle_band", "target.health_band", "target.observation_revision",
            "pack.id", "pack.member_count", "pack.attack_slot_granted", "pack.attack_slot_generation",
            "pack.attack_slot_kind", "pack.flank_side", "pack.ranged_lane_clear", "pack.finisher_claim_granted",
            "asset.bite_ready", "asset.leap_ready", "asset.spit_ready",
        }), "host Rabbit key set changed");

        var localNames = manifest.BlackboardKeys.Select(key => key.Name).ToHashSet(StringComparer.Ordinal);
        Check(localNames.SetEquals(new[]
        {
            "role.bound_id", "intent.current_goal", "intent.plan_generation", "target.last_valid_id",
            "target.last_valid_revision", "action.last_id", "action.last_outcome", "action.failure_streak",
            "action.cooldown_bite_until", "action.cooldown_leap_until", "action.cooldown_spit_until",
            "procedure.active_id", "procedure.stage", "procedure.commit_open", "recovery.required",
            "pack.last_slot_generation",
        }), "package-local Rabbit key set changed");

        var derivedNames = BroodmotherCompanionAiV2Contract.DerivedKeyDeclarations.Select(key => key.Name).ToHashSet(StringComparer.Ordinal);
        Check(derivedNames.SetEquals(new[]
        {
            "must_stop", "must_retreat", "may_recover", "may_bite", "may_leap", "may_spit",
            "target_weak", "needs_flank", "needs_reposition",
        }), "derived Rabbit key set changed");

        var procedures = manifest.ProcedureRequirements.ToDictionary(requirement => requirement.Id.Value, requirement => requirement, StringComparer.Ordinal);
        Check(procedures.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(new[]
        {
            "avalon.broodmother.spider.procedure.bite.v1",
            "avalon.broodmother.spider.procedure.leap.v1",
            "avalon.broodmother.spider.procedure.spit.v1",
            "avalon.broodmother.spider.procedure.flank.v1",
            "avalon.broodmother.spider.procedure.reposition.v1",
            "avalon.broodmother.spider.procedure.retreat.v1",
            "avalon.broodmother.spider.procedure.recovery.v1",
            "avalon.broodmother.spider.procedure.interruption.v1",
        }), "procedure ID set changed");
        Check(BroodmotherCompanionAiV2Contract.BiteProcedure.Timeout == TimeSpan.FromMilliseconds(1800), "bite timeout changed");
        Check(BroodmotherCompanionAiV2Contract.LeapProcedure.Timeout == TimeSpan.FromMilliseconds(3500), "leap timeout changed");
        Check(BroodmotherCompanionAiV2Contract.SpitProcedure.Timeout == TimeSpan.FromMilliseconds(3000), "spit timeout changed");
        Check(BroodmotherCompanionAiV2Contract.FlankProcedure.Timeout == TimeSpan.FromMilliseconds(2500), "flank timeout changed");
        Check(BroodmotherCompanionAiV2Contract.RepositionProcedure.Timeout == TimeSpan.FromMilliseconds(2200), "reposition timeout changed");
        Check(BroodmotherCompanionAiV2Contract.RetreatProcedure.Timeout == TimeSpan.FromMilliseconds(2500), "retreat timeout changed");
        Check(BroodmotherCompanionAiV2Contract.RecoveryProcedure.Timeout == TimeSpan.FromMilliseconds(1400), "recovery timeout changed");
        Check(BroodmotherCompanionAiV2Contract.InterruptionProcedure.Timeout == TimeSpan.FromMilliseconds(500), "interruption timeout changed");
    }

    private static void AuthoredSpiderAssetHandlesAreComplete()
    {
        var bindings = BroodmotherCompanionAiV2Contract.AuthoredSpiderAssetBindings;
        Check(bindings.Count == 43, "authored Spider asset binding count changed");
        Check(bindings.Select(binding => binding.ProcedureId.Value + "|" + binding.StageId).Distinct(StringComparer.Ordinal).Count() == bindings.Count,
            "authored Spider asset bindings must be unique per procedure stage");
        Check(bindings.All(binding => !binding.AssetHandle.Contains("placeholder", StringComparison.OrdinalIgnoreCase)),
            "authored Spider asset handles must not use placeholders");
        Check(bindings.Any(binding => binding.AssetHandle == BroodmotherCompanionAiV2Contract.BiteAttackClipHandle),
            "bite animation handle missing");
        Check(bindings.Any(binding => binding.AssetHandle == BroodmotherCompanionAiV2Contract.LeapAttackClipHandle),
            "leap animation handle missing");
        Check(bindings.Any(binding => binding.AssetHandle == BroodmotherCompanionAiV2Contract.LeapArcMotionHandle),
            "leap motion handle missing");
        Check(bindings.Any(binding => binding.StageId == "move.flank-animation"
                && binding.AssetHandle == BroodmotherCompanionAiV2Contract.MovementRunClipHandle)
            && bindings.Any(binding => binding.StageId == "move.role-animation"
                && binding.AssetHandle == BroodmotherCompanionAiV2Contract.MovementRunClipHandle)
            && bindings.Any(binding => binding.StageId == "move.safe-animation"
                && binding.AssetHandle == BroodmotherCompanionAiV2Contract.MovementRunClipHandle),
            "movement animation stages must use the authored Spider run handle");
        Check(bindings.Any(binding => binding.AssetHandle == BroodmotherCompanionAiV2Contract.GildedSpitVfxHandle),
            "gilded spit VFX handle missing");
        Check(bindings.Any(binding => binding.AssetHandle == BroodmotherCompanionAiV2Contract.GildedSpitProjectileHandle),
            "gilded spit projectile handle missing");
        Check(bindings.Any(binding => binding.AssetHandle == BroodmotherCompanionAiV2Contract.BiteDamageWindowHandle)
            && bindings.Any(binding => binding.AssetHandle == BroodmotherCompanionAiV2Contract.LeapContactDamageWindowHandle)
            && bindings.Any(binding => binding.AssetHandle == BroodmotherCompanionAiV2Contract.SpitDamageWindowHandle),
            "damage-window handles missing");
    }

    private static void BroodmotherPluginRoutesV2PlayMakerHostBridge()
    {
        string repoRoot = FindRepositoryRoot();
        string pluginPath = Path.Combine(repoRoot, "mods", "avalon-broodmother-companion", "src", "Plugin.cs");
        string definitionsPath = Path.Combine(repoRoot, "mods", "avalon-broodmother-companion", "src", "SpiderFamilyCompanionDefinitions.cs");
        string plugin = File.ReadAllText(pluginPath);
        string definitions = File.ReadAllText(definitionsPath);

        Check(plugin.Contains("BroodmotherCompanionAiV2Contract.PackageIdValue", StringComparison.Ordinal),
            "plugin must log/use the V2 package ID");
        Check(plugin.Contains("AvalonFoABroodmotherSpiderRuntimeHostComposition", StringComparison.Ordinal),
            "plugin must create the FoAHost Broodmother Spider runtime composition");
        Check(plugin.Contains("AvalonPlayMakerSpiderRunner", StringComparison.Ordinal),
            "plugin must drive the PlayMaker Spider procedure runner");
        Check(plugin.Contains("bridge=ForSpawnedUnityActor", StringComparison.Ordinal),
            "plugin diagnostics must identify the spawned-actor bridge route");
        Check(plugin.Contains("CommitBroodmotherSpiderDamageWindow", StringComparison.Ordinal) &&
              plugin.Contains("LaunchBroodmotherSpiderSpitProjectile", StringComparison.Ordinal),
            "plugin must bind damage and spit callbacks behind the bridge");
        Check(plugin.Contains("_spiderLeapRepositionRequired", StringComparison.Ordinal) &&
              plugin.Contains("ShouldForceSpiderV2LeapReposition", StringComparison.Ordinal) &&
              plugin.Contains("leapEnabledForSelection", StringComparison.Ordinal) &&
              plugin.Contains("ActionPouncerFlank.Value", StringComparison.Ordinal) &&
              plugin.Contains("ActionAmbusherWideFlank.Value", StringComparison.Ordinal),
            "plugin must force pouncer/ambusher to re-establish flank after a successful leap");
        Check(plugin.Contains("BroodmotherCompanionAiV2Contract.MovementRunClipHandle", StringComparison.Ordinal) &&
              plugin.Contains("Spider_Run_CI4", StringComparison.Ordinal) &&
              !plugin.Contains("[BroodmotherCompanionAiV2Contract.LeapArcMotionHandle]", StringComparison.Ordinal),
            "plugin must map authored movement animation without treating leap arc motion as an animation handle");
        Check(!plugin.Contains("BroodmotherCompanionAiV1Contract.Evaluate", StringComparison.Ordinal),
            "plugin advanced Spider AI must not call the V1 static evaluator");

        Check(definitions.Contains("CrimsonVanguardRole.Value", StringComparison.Ordinal) &&
              definitions.Contains("PalePouncerRole.Value", StringComparison.Ordinal) &&
              definitions.Contains("GildedSpitterRole.Value", StringComparison.Ordinal) &&
              definitions.Contains("CrimsonHarrierRole.Value", StringComparison.Ordinal) &&
              definitions.Contains("PaleAmbusherRole.Value", StringComparison.Ordinal) &&
              definitions.Contains("GildedFinisherRole.Value", StringComparison.Ordinal),
            "plugin definitions must use all six stable V2 role IDs");
        Check(definitions.Contains("Broodmother Asha - Crimson Vanguard", StringComparison.Ordinal) &&
              definitions.Contains("Broodmother Velra - Pale Pouncer", StringComparison.Ordinal) &&
              definitions.Contains("Broodmother Aurex - Gilded Spitter", StringComparison.Ordinal) &&
              definitions.Contains("Rook - Crimson Harrier Spider", StringComparison.Ordinal) &&
              definitions.Contains("Vesper - Pale Ambusher Spider", StringComparison.Ordinal) &&
              definitions.Contains("Nox - Gilded Finisher Spider", StringComparison.Ordinal),
            "plugin definitions must expose distinctive runtime display names");
        Check(definitions.Contains("Broodmother's Call - Asha, Crimson Vanguard", StringComparison.Ordinal) &&
              definitions.Contains("Broodmother's Call - Velra, Pale Pouncer", StringComparison.Ordinal) &&
              definitions.Contains("Broodmother's Call - Aurex, Gilded Spitter", StringComparison.Ordinal) &&
              definitions.Contains("Spider's Call - Rook, Crimson Harrier", StringComparison.Ordinal) &&
              definitions.Contains("Spider's Call - Vesper, Pale Ambusher", StringComparison.Ordinal) &&
              definitions.Contains("Spider's Call - Nox, Gilded Finisher", StringComparison.Ordinal),
            "plugin definitions must expose distinctive spell item names");
        Check(definitions.Contains("close bite pressure", StringComparison.Ordinal) &&
              definitions.Contains("flanking leaps", StringComparison.Ordinal) &&
              definitions.Contains("gilded venom spit", StringComparison.Ordinal) &&
              definitions.Contains("rapid bite cycles", StringComparison.Ordinal) &&
              definitions.Contains("wide flanks and leap pressure", StringComparison.Ordinal) &&
              definitions.Contains("weak-target execution", StringComparison.Ordinal),
            "plugin definitions must expose role-specific tooltip descriptions");
        Check(!definitions.Contains("Broodmother's Call - Skin", StringComparison.Ordinal) &&
              !definitions.Contains("Spider's Call - Skin", StringComparison.Ordinal) &&
              !definitions.Contains("companion ({VariantId})", StringComparison.Ordinal) &&
              !definitions.Contains("through the guarded one-session companion route", StringComparison.Ordinal),
            "plugin definitions must not keep generic skin/route tooltip copy");
        Check(CountOccurrences(definitions, "5.5f") == 6,
            "all six Spider-family variants must use the approved 5.5 runtime scale");
    }

    private static void GoalPolicyIsSpecialized()
    {
        var policy = new BroodmotherCompanionGoalPolicyV2();
        var cases = new[]
        {
            GoalCase(BroodmotherCompanionAiV2Contract.CrimsonVanguardRole, BroodmotherCompanionAiV2Contract.GoalVanguardBitePressure, 62f, 20f, 300),
            GoalCase(BroodmotherCompanionAiV2Contract.PalePouncerRole, BroodmotherCompanionAiV2Contract.GoalPouncerLeapPressure, 64f, 24f, 400),
            GoalCase(BroodmotherCompanionAiV2Contract.GildedSpitterRole, BroodmotherCompanionAiV2Contract.GoalSpitterRangedPressure, 63f, 22f, 350),
            GoalCase(BroodmotherCompanionAiV2Contract.CrimsonHarrierRole, BroodmotherCompanionAiV2Contract.GoalHarrierBiteCycle, 61f, 26f, 250),
            GoalCase(BroodmotherCompanionAiV2Contract.PaleAmbusherRole, BroodmotherCompanionAiV2Contract.GoalAmbusherWideLeap, 65f, 25f, 450),
            GoalCase(BroodmotherCompanionAiV2Contract.GildedFinisherRole, BroodmotherCompanionAiV2Contract.GoalFinisherWeakTarget, 75f, 40f, 300, weak: true, claim: true),
        };

        foreach (var test in cases)
        {
            var goals = policy.Evaluate(Snapshot(test.Role), Blackboard(test.Role, weak: test.Weak, claim: test.Claim));
            Check(goals.Count == 1, test.Role.Value + " did not select one goal");
            Check(goals[0].GoalId == test.Goal, test.Role.Value + " selected the wrong goal");
            Check(goals[0].BasePriority == test.Priority && goals[0].Urgency == test.Urgency, test.Role.Value + " priority changed");
            Check(goals[0].MinimumCommitment == TimeSpan.FromMilliseconds(test.CommitmentMilliseconds), test.Role.Value + " commitment changed");
        }

        var healthyFinisher = policy.Evaluate(
            Snapshot(BroodmotherCompanionAiV2Contract.GildedFinisherRole),
            Blackboard(BroodmotherCompanionAiV2Contract.GildedFinisherRole, weak: false, claim: false));
        Check(healthyFinisher.Single().GoalId == BroodmotherCompanionAiV2Contract.GoalFinisherPressure, "healthy finisher pressure goal changed");

        var unclaimedWeak = policy.Evaluate(
            Snapshot(BroodmotherCompanionAiV2Contract.GildedFinisherRole),
            Blackboard(BroodmotherCompanionAiV2Contract.GildedFinisherRole, weak: true, claim: false));
        Check(unclaimedWeak.Count == 0, "finisher claimed an unreserved weak target");

        var emergency = Blackboard(BroodmotherCompanionAiV2Contract.PalePouncerRole);
        emergency.Set(BroodmotherCompanionAiV2Contract.MustStop, true);
        Check(policy.Evaluate(Snapshot(BroodmotherCompanionAiV2Contract.PalePouncerRole), emergency).Single().GoalId == BroodmotherCompanionAiV2Contract.GoalStopSafely, "stop did not outrank role goal");
        emergency.Set(BroodmotherCompanionAiV2Contract.MustStop, false);
        emergency.Set(BroodmotherCompanionAiV2Contract.MustRetreat, true);
        Check(policy.Evaluate(Snapshot(BroodmotherCompanionAiV2Contract.PalePouncerRole), emergency).Single().GoalId == BroodmotherCompanionAiV2Contract.GoalRetreat, "retreat did not outrank role goal");
        emergency.Set(BroodmotherCompanionAiV2Contract.MustRetreat, false);
        emergency.Set(BroodmotherCompanionAiV2Contract.RecoveryRequired, true);
        Check(policy.Evaluate(Snapshot(BroodmotherCompanionAiV2Contract.PalePouncerRole), emergency).Single().GoalId == BroodmotherCompanionAiV2Contract.GoalRecover, "recovery did not outrank role goal");

        var legacyRole = new ActorRoleId("avalon-broodmother-companion.spider");
        Check(policy.Evaluate(Snapshot(legacyRole), Blackboard(legacyRole)).Count == 0, "legacy generic role must fail closed");
        var wrongMode = new ActorSnapshot(
            new ActorLease(Lease.ActorId, Lease.LeaseId, ActorExecutionMode.NativeAssisted, Lease.Generation),
            BroodmotherCompanionAiV2Contract.CrimsonVanguardRole,
            1);
        Check(policy.Evaluate(wrongMode, Blackboard(BroodmotherCompanionAiV2Contract.CrimsonVanguardRole)).Count == 0, "non-BlazeOwned actor must fail closed");
    }

    private static void RabbitProjectorIsExact()
    {
        var projector = new BroodmotherPlanningProjectorV2();
        for (var index = 0; index < BroodmotherCompanionAiV2Contract.SupportedActorRoles.Count; index++)
        {
            var role = BroodmotherCompanionAiV2Contract.SupportedActorRoles[index];
            var projection = projector.Project(
                Snapshot(role),
                Blackboard(
                    role,
                    distance: BroodmotherCompanionAiV2Contract.DistanceBandLeap,
                    angle: BroodmotherCompanionAiV2Contract.AngleBandSide,
                    slotKind: BroodmotherCompanionAiV2Contract.SlotKindLeap,
                    needsReposition: false));
            Check(projection.Accepted, role.Value + " projection rejected: " + projection.Reason);
            Check(projection.Facts.Count == 14, role.Value + " planning fact count changed");
            Check(FactValue(projection.Facts, BroodmotherCompanionAiV2Contract.FactRoleCode) == index + 1, role.Value + " role code changed");
            Check(FactValue(projection.Facts, BroodmotherCompanionAiV2Contract.FactAtFlank) == 1, role.Value + " flank projection changed");
            Check(FactValue(projection.Facts, BroodmotherCompanionAiV2Contract.FactAtRoleBand) == 1, role.Value + " role-band projection changed");
            Check(FactValue(projection.Facts, BroodmotherCompanionAiV2Contract.FactSlotGranted) == 1, role.Value + " exact slot was not projected");
            Check(FactValue(projection.Facts, BroodmotherCompanionAiV2Contract.FactActionCompleted) == 0, role.Value + " fresh plan must begin incomplete");
        }

        var staleSlot = Blackboard(
            BroodmotherCompanionAiV2Contract.CrimsonVanguardRole,
            slotKind: BroodmotherCompanionAiV2Contract.SlotKindBite);
        staleSlot.Set(BroodmotherCompanionAiV2Contract.PackLastSlotGeneration, 7);
        Check(!projector.Project(Snapshot(BroodmotherCompanionAiV2Contract.CrimsonVanguardRole), staleSlot).Accepted, "stale slot generation was accepted");

        var wrongBinding = Blackboard(BroodmotherCompanionAiV2Contract.CrimsonVanguardRole);
        wrongBinding.Set(BroodmotherCompanionAiV2Contract.RoleBoundId, BroodmotherCompanionAiV2Contract.PalePouncerRole.Value);
        Check(!projector.Project(Snapshot(BroodmotherCompanionAiV2Contract.CrimsonVanguardRole), wrongBinding).Accepted, "role binding mismatch was accepted");

        Check(!projector.Project(Snapshot(BroodmotherCompanionAiV2Contract.CrimsonVanguardRole), new FixtureBlackboard()).Accepted, "incomplete Rabbit snapshot was accepted");
    }

    private static void SixRoleGoapPlansAreExact()
    {
        var cases = new[]
        {
            PlanCase(
                BroodmotherCompanionAiV2Contract.CrimsonVanguardRole,
                BroodmotherCompanionAiV2Contract.DistanceBandFar,
                BroodmotherCompanionAiV2Contract.AngleBandFront,
                BroodmotherCompanionAiV2Contract.SlotKindBite,
                true,
                BroodmotherCompanionAiV2Contract.ActionVanguardClose,
                BroodmotherCompanionAiV2Contract.ActionVanguardBite,
                BroodmotherCompanionAiV2Contract.ActionRecoverControl),
            PlanCase(
                BroodmotherCompanionAiV2Contract.PalePouncerRole,
                BroodmotherCompanionAiV2Contract.DistanceBandLeap,
                BroodmotherCompanionAiV2Contract.AngleBandFront,
                BroodmotherCompanionAiV2Contract.SlotKindLeap,
                true,
                BroodmotherCompanionAiV2Contract.ActionPouncerFlank,
                BroodmotherCompanionAiV2Contract.ActionPouncerLeap,
                BroodmotherCompanionAiV2Contract.ActionRecoverControl),
            PlanCase(
                BroodmotherCompanionAiV2Contract.GildedSpitterRole,
                BroodmotherCompanionAiV2Contract.DistanceBandBite,
                BroodmotherCompanionAiV2Contract.AngleBandFront,
                BroodmotherCompanionAiV2Contract.SlotKindSpit,
                true,
                BroodmotherCompanionAiV2Contract.ActionSpitterReposition,
                BroodmotherCompanionAiV2Contract.ActionSpitterSpit,
                BroodmotherCompanionAiV2Contract.ActionRecoverControl),
            PlanCase(
                BroodmotherCompanionAiV2Contract.CrimsonHarrierRole,
                BroodmotherCompanionAiV2Contract.DistanceBandBite,
                BroodmotherCompanionAiV2Contract.AngleBandSide,
                BroodmotherCompanionAiV2Contract.SlotKindBite,
                false,
                BroodmotherCompanionAiV2Contract.ActionHarrierBite,
                BroodmotherCompanionAiV2Contract.ActionHarrierReposition,
                BroodmotherCompanionAiV2Contract.ActionRecoverControl),
            PlanCase(
                BroodmotherCompanionAiV2Contract.PaleAmbusherRole,
                BroodmotherCompanionAiV2Contract.DistanceBandLeap,
                BroodmotherCompanionAiV2Contract.AngleBandFront,
                BroodmotherCompanionAiV2Contract.SlotKindLeap,
                true,
                BroodmotherCompanionAiV2Contract.ActionAmbusherWideFlank,
                BroodmotherCompanionAiV2Contract.ActionAmbusherLeap,
                BroodmotherCompanionAiV2Contract.ActionRecoverControl),
            PlanCase(
                BroodmotherCompanionAiV2Contract.GildedFinisherRole,
                BroodmotherCompanionAiV2Contract.DistanceBandFar,
                BroodmotherCompanionAiV2Contract.AngleBandSide,
                BroodmotherCompanionAiV2Contract.SlotKindBite,
                true,
                BroodmotherCompanionAiV2Contract.ActionFinisherClaimPosition,
                BroodmotherCompanionAiV2Contract.ActionFinisherBite,
                BroodmotherCompanionAiV2Contract.ActionRecoverControl,
                weak: true,
                claim: true),
        };

        foreach (var test in cases)
        {
            var blackboard = Blackboard(
                test.Role,
                test.DistanceBand,
                test.AngleBand,
                test.SlotKind,
                test.Weak,
                test.Claim,
                test.NeedsReposition);
            var actor = Snapshot(test.Role);
            var goals = new BroodmotherCompanionGoalPolicyV2().Evaluate(actor, blackboard);
            var projection = new BroodmotherPlanningProjectorV2().Project(actor, blackboard);
            Check(goals.Count == 1 && projection.Accepted, test.Role.Value + " planning inputs were rejected");

            var target = new PlanningTarget(
                BroodmotherCompanionAiV2Contract.CurrentTarget,
                TargetDescriptor.ForActor(new TargetId("fixture.target")),
                new AvalonPosition(3f, 0f, 1f));
            var actual = ResolveOfflineGoap(actor, goals[0], projection.Facts, target);
            Check(actual.SequenceEqual(test.ExpectedActions), test.Role.Value + " resolved " + string.Join(",", actual.Select(id => id.Value)));
        }

        var roleByAction = new Dictionary<ActionId, int>
        {
            [BroodmotherCompanionAiV2Contract.ActionVanguardClose] = 1,
            [BroodmotherCompanionAiV2Contract.ActionVanguardBite] = 1,
            [BroodmotherCompanionAiV2Contract.ActionPouncerFlank] = 2,
            [BroodmotherCompanionAiV2Contract.ActionPouncerLeap] = 2,
            [BroodmotherCompanionAiV2Contract.ActionSpitterReposition] = 3,
            [BroodmotherCompanionAiV2Contract.ActionSpitterSpit] = 3,
            [BroodmotherCompanionAiV2Contract.ActionHarrierBite] = 4,
            [BroodmotherCompanionAiV2Contract.ActionHarrierReposition] = 4,
            [BroodmotherCompanionAiV2Contract.ActionAmbusherWideFlank] = 5,
            [BroodmotherCompanionAiV2Contract.ActionAmbusherLeap] = 5,
            [BroodmotherCompanionAiV2Contract.ActionFinisherClaimPosition] = 6,
            [BroodmotherCompanionAiV2Contract.ActionFinisherBite] = 6,
        };
        foreach (var pair in roleByAction)
        {
            var action = ActionDefinition(pair.Key);
            Check(action.Conditions.Any(condition => condition.FactId == BroodmotherCompanionAiV2Contract.FactRoleCode && condition.Value == pair.Value), pair.Key.Value + " lacks exact role precondition");
        }
    }

    private static void LeapAttacksForceReflank()
    {
        CheckLeapAttackReset(BroodmotherCompanionAiV2Contract.ActionPouncerLeap, "pouncer");
        CheckLeapAttackReset(BroodmotherCompanionAiV2Contract.ActionAmbusherLeap, "ambusher");
        Check(!HasEffect(BroodmotherCompanionAiV2Contract.ActionVanguardBite, BroodmotherCompanionAiV2Contract.FactAtFlank),
            "non-leap attack must not change flank ownership");
    }

    private static void CheckLeapAttackReset(ActionId actionId, string label)
    {
        Check(EffectValue(actionId, BroodmotherCompanionAiV2Contract.FactActionCompleted) == 1,
            label + " leap must complete the current action");
        Check(EffectValue(actionId, BroodmotherCompanionAiV2Contract.FactRecoveryRequired) == 1,
            label + " leap must require recovery");
        Check(EffectValue(actionId, BroodmotherCompanionAiV2Contract.FactAtFlank) == 0,
            label + " leap must clear flank before the next leap");
        Check(EffectValue(actionId, BroodmotherCompanionAiV2Contract.FactAtRoleBand) == 0,
            label + " leap must clear role band before the next leap");
    }

    private static void ActionCostsAreExact()
    {
        Check(Near(ConstantCost(BroodmotherCompanionAiV2Contract.ActionStopSafely), 0f), "stop cost changed");
        Check(Near(ConstantCost(BroodmotherCompanionAiV2Contract.ActionRetreat), 0.5f), "retreat cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionVanguardClose, 0, 0, 0, 1, 0, 1), 2f), "vanguard close cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionVanguardBite, 0, 1, 1, 1, 0, 1), 1f), "vanguard base cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionPouncerFlank, 1, 0, 0, 1, 0, 1), 1.5f), "pouncer flank cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionPouncerLeap, 1, 1, 2, 1, 0, 1), 1f), "pouncer leap cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionSpitterReposition, 2, 0, 0, 1, 0, 1), 1f), "spitter reposition cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionSpitterSpit, 2, 1, 3, 1, 0, 1), 1f), "spitter spit cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionHarrierBite, 0, 1, 1, 1, 0, 1), 0.8f), "harrier bite cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionHarrierReposition, 0, 0, 0, 1, 0, 1), 0.8f), "harrier reposition cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionAmbusherWideFlank, 1, 0, 0, 1, 0, 1), 1f), "ambusher flank cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionAmbusherLeap, 1, 1, 2, 1, 0, 1), 0.9f), "ambusher leap cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionFinisherClaimPosition, 0, 0, 0, 1, 1, 1), 0.8f), "finisher positioning cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionFinisherBite, 0, 1, 1, 1, 1, 1), 0.6f), "finisher bite cost changed");
        Check(Near(RecoveryCost(1, false), 1f), "vanguard recovery cost changed");
        Check(Near(RecoveryCost(2, false), 1.5f), "pouncer recovery cost changed");
        Check(Near(RecoveryCost(3, false), 1f), "spitter recovery cost changed");
        Check(Near(RecoveryCost(4, false), 0.8f), "harrier recovery cost changed");
        Check(Near(RecoveryCost(5, false), 1.2f), "ambusher recovery cost changed");
        Check(Near(RecoveryCost(6, false), 0.8f), "finisher recovery cost changed");
        Check(Near(RecoveryCost(1, true), 0.5f), "emergency recovery cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionVanguardBite, 1, 1, 1, 1, 0, 1), 4f), "one-band cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionVanguardBite, 2, 0, 0, 1, 0, 1), 15f), "distance and missing-slot defense cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionPouncerLeap, 1, 1, 2, 1, 0, 0), 5f), "front-lane leap cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionSpitterSpit, 2, 1, 3, 0, 0, 1), 6f), "blocked spit lane cost changed");
        Check(Near(Cost(BroodmotherCompanionAiV2Contract.ActionFinisherBite, 0, 1, 1, 1, 0, 1), 4.6f), "healthy finisher penalty changed");
    }

    private static void ReplanningRulesAreExact()
    {
        Check(BroodmotherReplanningPolicyV2.Evaluate(BroodmotherReplanTriggerV2.CleanupRequested, BroodmotherCompanionAiV2Contract.ActionVanguardBite, false).InterruptAction, "cleanup must interrupt immediately");
        Check(BroodmotherReplanningPolicyV2.Evaluate(BroodmotherReplanTriggerV2.TargetChanged, BroodmotherCompanionAiV2Contract.ActionPouncerLeap, false).InterruptAction, "target change must interrupt a targeted action");
        Check(BroodmotherReplanningPolicyV2.Evaluate(BroodmotherReplanTriggerV2.DamageRevisionIncreased, BroodmotherCompanionAiV2Contract.ActionPouncerLeap, false).RequireRecovery, "damage during leap must require recovery");
        Check(BroodmotherReplanningPolicyV2.Evaluate(BroodmotherReplanTriggerV2.ThreatEnteredUnsafeBand, BroodmotherCompanionAiV2Contract.ActionSpitterSpit, false).InterruptAction, "unsafe threat must interrupt spit");
        Check(!BroodmotherReplanningPolicyV2.Evaluate(BroodmotherReplanTriggerV2.EmergencyGoalOutranks, BroodmotherCompanionAiV2Contract.ActionHarrierBite, false).InterruptAction, "goal change interrupted before commitment");
        Check(BroodmotherReplanningPolicyV2.Evaluate(BroodmotherReplanTriggerV2.EmergencyGoalOutranks, BroodmotherCompanionAiV2Contract.ActionHarrierBite, true).InterruptAction, "goal change did not interrupt after commitment");
        Check(!BroodmotherReplanningPolicyV2.Evaluate(BroodmotherReplanTriggerV2.OrdinaryObservation, BroodmotherCompanionAiV2Contract.ActionHarrierBite, true).InterruptAction, "ordinary observation thrashed the plan");
        Check(BroodmotherReplanningPolicyV2.Evaluate(BroodmotherReplanTriggerV2.CommandFault, BroodmotherCompanionAiV2Contract.ActionFinisherBite, true).RequireRecovery, "command fault did not require recovery");
    }

    private static void AssemblyBoundaryIsExact()
    {
        var assembly = typeof(BroodmotherCompanionAiPackageV2).Assembly;
        var references = assembly.GetReferencedAssemblies().Select(reference => reference.Name ?? string.Empty).ToArray();
        Check(references.Contains("AvalonAI.Contracts.V2"), "V2 must reference Contracts V2");
        Check(!references.Any(name => name.StartsWith("AvalonAI.Runtime", StringComparison.Ordinal)), "V2 references Runtime");
        Check(!references.Any(name => name.StartsWith("AvalonAI.Blackboard", StringComparison.Ordinal)), "V2 references Rabbit implementation");
        Check(!references.Any(name => name.StartsWith("AvalonAI.Planning", StringComparison.Ordinal)), "V2 references GOAP implementation");
        Check(!references.Any(name => name.StartsWith("AvalonAI.Execution", StringComparison.Ordinal)), "V2 references an executor");
        Check(!references.Any(name => name.StartsWith("Unity", StringComparison.Ordinal) || name == "BepInEx" || name.StartsWith("0Harmony", StringComparison.Ordinal)), "V2 references game or loader assemblies");
        Check(!references.Contains("AvalonBroodmotherCompanion.AI.Package.V1"), "V2 references the primitive V1 evaluator");
        Check(typeof(BroodmotherCompanionAiV2Contract).GetMethod("Evaluate", BindingFlags.Public | BindingFlags.Static) is null, "V2 exposes a primitive static tactic evaluator");
    }

    private static FixtureBlackboard Blackboard(
        ActorRoleId role,
        int distance = BroodmotherCompanionAiV2Contract.DistanceBandBite,
        int angle = BroodmotherCompanionAiV2Contract.AngleBandSide,
        int slotKind = BroodmotherCompanionAiV2Contract.SlotKindBite,
        bool weak = false,
        bool claim = false,
        bool needsReposition = false)
    {
        return new FixtureBlackboard()
            .Set(BroodmotherCompanionAiV2Contract.RoleBoundId, role.Value)
            .Set(BroodmotherCompanionAiV2Contract.MustStop, false)
            .Set(BroodmotherCompanionAiV2Contract.MustRetreat, false)
            .Set(BroodmotherCompanionAiV2Contract.TargetValid, true)
            .Set(BroodmotherCompanionAiV2Contract.TargetWeak, weak)
            .Set(BroodmotherCompanionAiV2Contract.TargetDistanceBand, distance)
            .Set(BroodmotherCompanionAiV2Contract.TargetAngleBand, angle)
            .Set(BroodmotherCompanionAiV2Contract.PackAttackSlotGranted, true)
            .Set(BroodmotherCompanionAiV2Contract.PackAttackSlotGeneration, 7)
            .Set(BroodmotherCompanionAiV2Contract.PackAttackSlotKind, slotKind)
            .Set(BroodmotherCompanionAiV2Contract.PackRangedLaneClear, true)
            .Set(BroodmotherCompanionAiV2Contract.PackFinisherClaimGranted, claim)
            .Set(BroodmotherCompanionAiV2Contract.RecoveryRequired, false)
            .Set(BroodmotherCompanionAiV2Contract.PackLastSlotGeneration, 6)
            .Set(BroodmotherCompanionAiV2Contract.NeedsFlank, angle == BroodmotherCompanionAiV2Contract.AngleBandFront)
            .Set(BroodmotherCompanionAiV2Contract.NeedsReposition, needsReposition);
    }

    private static ActorSnapshot Snapshot(ActorRoleId role) => new ActorSnapshot(Lease, role, 11);

    private static float Cost(
        ActionId id,
        int distance,
        int slotGranted,
        int slotKind,
        int laneClear,
        int targetWeak,
        int angle)
    {
        var facts = new[]
        {
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactDistanceBand, distance),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactSlotGranted, slotGranted),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactSlotKind, slotKind),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactRangedLaneClear, laneClear),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactTargetWeak, targetWeak),
            new PlanningFact(BroodmotherCompanionAiV2Contract.FactAngleBand, angle),
        };
        return ActionDefinition(id).CostProvider.Evaluate(new ActionCostContext(
            Lease,
            11,
            id,
            new AvalonPosition(0f, 0f, 0f),
            facts,
            null));
    }

    private static AvalonActionDefinition ActionDefinition(ActionId id) =>
        BroodmotherCompanionAiV2Contract.ActionDefinitions.Single(action => action.Id == id);

    private static bool HasEffect(ActionId actionId, PlanningFactId factId) =>
        ActionDefinition(actionId).Effects.Any(effect => effect.FactId == factId);

    private static int EffectValue(ActionId actionId, PlanningFactId factId) =>
        ActionDefinition(actionId).Effects.Single(effect => effect.FactId == factId).AssignedValue;

    private static float ConstantCost(ActionId id) =>
        ActionDefinition(id).CostProvider.Evaluate(new ActionCostContext(
            Lease,
            11,
            id,
            new AvalonPosition(0f, 0f, 0f),
            Array.Empty<PlanningFact>(),
            null));

    private static float RecoveryCost(int roleCode, bool recoveryRequired)
    {
        var id = BroodmotherCompanionAiV2Contract.ActionRecoverControl;
        return ActionDefinition(id).CostProvider.Evaluate(new ActionCostContext(
            Lease,
            11,
            id,
            new AvalonPosition(0f, 0f, 0f),
            new[]
            {
                new PlanningFact(BroodmotherCompanionAiV2Contract.FactRoleCode, roleCode),
                new PlanningFact(BroodmotherCompanionAiV2Contract.FactRecoveryRequired, recoveryRequired ? 1 : 0),
            },
            null));
    }

    private static ActionId[] ResolveOfflineGoap(
        ActorSnapshot actor,
        GoalRequest requestedGoal,
        IReadOnlyList<PlanningFact> initialFacts,
        PlanningTarget target)
    {
        var goal = BroodmotherCompanionAiV2Contract.GoalDefinitions.Single(definition => definition.Id == requestedGoal.GoalId);
        var initial = initialFacts.ToDictionary(fact => fact.Id, fact => fact.Value);
        var queue = new PriorityQueue<PlannerNode, float>();
        var initialNode = new PlannerNode(initial, Array.Empty<ActionId>(), 0f);
        queue.Enqueue(initialNode, 0f);
        var bestCosts = new Dictionary<string, float>(StringComparer.Ordinal)
        {
            [StateKey(initial)] = 0f,
        };

        var expansions = 0;
        while (queue.Count > 0 && expansions++ < 2048)
        {
            var node = queue.Dequeue();
            if (ConditionsHold(goal.DesiredState, node.Facts))
            {
                return node.Actions;
            }

            foreach (var action in BroodmotherCompanionAiV2Contract.ActionDefinitions)
            {
                if (!ConditionsHold(action.Conditions, node.Facts))
                {
                    continue;
                }

                var nextFacts = new Dictionary<PlanningFactId, int>(node.Facts);
                foreach (var effect in action.Effects)
                {
                    nextFacts[effect.FactId] = effect.AssignedValue;
                }

                var stateKey = StateKey(nextFacts);
                if (StringComparer.Ordinal.Equals(stateKey, StateKey(node.Facts)))
                {
                    continue;
                }

                var cost = action.CostProvider.Evaluate(new ActionCostContext(
                    actor.Lease,
                    actor.ObservationRevision,
                    action.Id,
                    new AvalonPosition(0f, 0f, 0f),
                    initialFacts,
                    action.TargetKeyId.IsEmpty ? null : target));
                if (float.IsNaN(cost) || float.IsInfinity(cost) || cost < 0f)
                {
                    throw new InvalidOperationException(action.Id.Value + " returned an invalid GOAP cost");
                }

                var nextCost = node.Cost + cost;
                if (bestCosts.TryGetValue(stateKey, out var priorCost) && priorCost <= nextCost)
                {
                    continue;
                }

                bestCosts[stateKey] = nextCost;
                var nextActions = node.Actions.Append(action.Id).ToArray();
                queue.Enqueue(new PlannerNode(nextFacts, nextActions, nextCost), nextCost);
            }
        }

        throw new InvalidOperationException(actor.Role.Value + " produced no GOAP plan");
    }

    private static bool ConditionsHold(
        IReadOnlyList<PlanningCondition> conditions,
        IReadOnlyDictionary<PlanningFactId, int> facts)
    {
        foreach (var condition in conditions)
        {
            if (!facts.TryGetValue(condition.FactId, out var actual) ||
                !Compare(actual, condition.Comparison, condition.Value))
            {
                return false;
            }
        }

        return true;
    }

    private static bool Compare(int actual, PlanningComparison comparison, int expected)
    {
        return comparison switch
        {
            PlanningComparison.Equal => actual == expected,
            PlanningComparison.NotEqual => actual != expected,
            PlanningComparison.GreaterThan => actual > expected,
            PlanningComparison.GreaterThanOrEqual => actual >= expected,
            PlanningComparison.LessThan => actual < expected,
            PlanningComparison.LessThanOrEqual => actual <= expected,
            _ => false,
        };
    }

    private static string StateKey(IReadOnlyDictionary<PlanningFactId, int> facts) =>
        string.Join("|", facts.OrderBy(pair => pair.Key.Value, StringComparer.Ordinal).Select(pair => pair.Key.Value + "=" + pair.Value));

    private static int FactValue(IReadOnlyList<PlanningFact> facts, PlanningFactId id) =>
        facts.Single(fact => fact.Id == id).Value;

    private static void CheckSchema(
        IReadOnlyList<BlackboardKeyDeclaration> keys,
        int expectedCount,
        string expectedNamespace,
        BlackboardAccess expectedAccess)
    {
        Check(keys.Count == expectedCount, expectedNamespace + " key count changed");
        Check(keys.All(key => key.Namespace == expectedNamespace && key.SchemaVersion == 2 && key.Access == expectedAccess && key.Scope == BlackboardScope.Actor), expectedNamespace + " key ownership changed");
        Check(keys.Select(key => key.Name).Distinct(StringComparer.Ordinal).Count() == keys.Count, expectedNamespace + " contains duplicate keys");
    }

    private static GoalFixtureCase GoalCase(
        ActorRoleId role,
        GoalId goal,
        float priority,
        float urgency,
        int commitmentMilliseconds,
        bool weak = false,
        bool claim = false) =>
        new GoalFixtureCase(role, goal, priority, urgency, commitmentMilliseconds, weak, claim);

    private static PlanFixtureCase PlanCase(
        ActorRoleId role,
        int distanceBand,
        int angleBand,
        int slotKind,
        bool needsReposition,
        ActionId first,
        ActionId second,
        ActionId third,
        bool weak = false,
        bool claim = false) =>
        new PlanFixtureCase(role, distanceBand, angleBand, slotKind, needsReposition, weak, claim, new[] { first, second, third });

    private static bool Near(float actual, float expected) => Math.Abs(actual - expected) < 0.001f;

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            string candidate = Path.Combine(directory.FullName, "mods", "avalon-broodmother-companion", "src", "Plugin.cs");
            if (File.Exists(candidate))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("repository root not found from " + AppContext.BaseDirectory);
    }

    private static int CountOccurrences(string source, string candidate)
    {
        var count = 0;
        var index = 0;
        while ((index = source.IndexOf(candidate, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += candidate.Length;
        }

        return count;
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class GoalFixtureCase
    {
        public GoalFixtureCase(ActorRoleId role, GoalId goal, float priority, float urgency, int commitmentMilliseconds, bool weak, bool claim)
        {
            Role = role;
            Goal = goal;
            Priority = priority;
            Urgency = urgency;
            CommitmentMilliseconds = commitmentMilliseconds;
            Weak = weak;
            Claim = claim;
        }

        public ActorRoleId Role { get; }
        public GoalId Goal { get; }
        public float Priority { get; }
        public float Urgency { get; }
        public int CommitmentMilliseconds { get; }
        public bool Weak { get; }
        public bool Claim { get; }
    }

    private sealed class PlannerNode
    {
        public PlannerNode(Dictionary<PlanningFactId, int> facts, ActionId[] actions, float cost)
        {
            Facts = facts;
            Actions = actions;
            Cost = cost;
        }

        public Dictionary<PlanningFactId, int> Facts { get; }
        public ActionId[] Actions { get; }
        public float Cost { get; }
    }

    private sealed class PlanFixtureCase
    {
        public PlanFixtureCase(ActorRoleId role, int distanceBand, int angleBand, int slotKind, bool needsReposition, bool weak, bool claim, ActionId[] expectedActions)
        {
            Role = role;
            DistanceBand = distanceBand;
            AngleBand = angleBand;
            SlotKind = slotKind;
            NeedsReposition = needsReposition;
            Weak = weak;
            Claim = claim;
            ExpectedActions = expectedActions;
        }

        public ActorRoleId Role { get; }
        public int DistanceBand { get; }
        public int AngleBand { get; }
        public int SlotKind { get; }
        public bool NeedsReposition { get; }
        public bool Weak { get; }
        public bool Claim { get; }
        public ActionId[] ExpectedActions { get; }
    }

    private sealed class FixtureBlackboard : IAvalonBlackboardReader
    {
        private readonly Dictionary<BlackboardKeyDeclaration, object> values = new Dictionary<BlackboardKeyDeclaration, object>();

        public FixtureBlackboard Set<T>(BlackboardKey<T> key, T value)
        {
            values[key.Declaration] = value!;
            return this;
        }

        public bool TryRead<T>(BlackboardKey<T> key, out T value)
        {
            if (values.TryGetValue(key.Declaration, out var stored) && stored is T typed)
            {
                value = typed;
                return true;
            }

            value = default!;
            return false;
        }

        public IDisposable Observe<T>(BlackboardKey<T> key, Action<BlackboardChange<T>> callback)
        {
            _ = key.Declaration;
            _ = callback ?? throw new ArgumentNullException(nameof(callback));
            return NoOpDisposable.Instance;
        }
    }

    private sealed class NoOpDisposable : IDisposable
    {
        public static NoOpDisposable Instance { get; } = new NoOpDisposable();
        public void Dispose() { }
    }
}
