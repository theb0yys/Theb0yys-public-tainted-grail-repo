using System;
using System.Collections.Generic;
using AvalonAI.Contracts.V2;

namespace AvalonBroodmotherCompanion.AI.Package.V2;

public sealed class BroodmotherRoleDefinitionV2
{
    public BroodmotherRoleDefinitionV2(string displayName, ActorRoleId roleId, int roleCode)
    {
        DisplayName = string.IsNullOrWhiteSpace(displayName)
            ? throw new ArgumentException("A display name is required.", nameof(displayName))
            : displayName;
        RoleId = roleId.IsEmpty
            ? throw new ArgumentException("A stable role ID is required.", nameof(roleId))
            : roleId;
        RoleCode = roleCode is >= 1 and <= 6
            ? roleCode
            : throw new ArgumentOutOfRangeException(nameof(roleCode));
    }

    public string DisplayName { get; }

    public ActorRoleId RoleId { get; }

    public int RoleCode { get; }
}

public readonly struct BroodmotherSpiderAuthoredAssetBindingV2
{
    public BroodmotherSpiderAuthoredAssetBindingV2(
        AvalonProcedureId procedureId,
        string stageId,
        string assetKind,
        string assetHandle)
    {
        ProcedureId = procedureId.IsEmpty
            ? throw new ArgumentException("A procedure ID is required.", nameof(procedureId))
            : procedureId;
        StageId = RequireText(stageId, nameof(stageId));
        AssetKind = RequireText(assetKind, nameof(assetKind));
        AssetHandle = RequireText(assetHandle, nameof(assetHandle));
        if (AssetHandle.IndexOf("placeholder", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            throw new ArgumentException("Authored Spider assets must not use placeholder handles.", nameof(assetHandle));
        }
    }

    public AvalonProcedureId ProcedureId { get; }

    public string StageId { get; }

    public string AssetKind { get; }

    public string AssetHandle { get; }

    private static string RequireText(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A non-empty value is required.", parameterName)
            : value.Trim();
}

public static class BroodmotherCompanionAiV2Contract
{
    public const string PackageIdValue = "kane.tgfoa.avalon-broodmother-companion.ai-spider-companion.v2";
    public const string DisplayName = "Avalon Broodmother Companion Commercial-Stack Spider AI";
    public const string PackageVersion = "0.2.0-broodmother-spider-ai";
    public const string BlackboardNamespace = "avalon_broodmother_companion.spider_ai.v2";
    public const string HostBlackboardNamespace = "avalon.broodmother_companion.host.v2";
    public const string DerivedBlackboardNamespace = "avalon.broodmother_companion.derived.v2";
    public const int BlackboardSchemaVersion = 2;

    public const int CrimsonVanguardRoleCode = 1;
    public const int PalePouncerRoleCode = 2;
    public const int GildedSpitterRoleCode = 3;
    public const int CrimsonHarrierRoleCode = 4;
    public const int PaleAmbusherRoleCode = 5;
    public const int GildedFinisherRoleCode = 6;

    public const int SlotKindNone = 0;
    public const int SlotKindBite = 1;
    public const int SlotKindLeap = 2;
    public const int SlotKindSpit = 3;

    public const int DistanceBandBite = 0;
    public const int DistanceBandLeap = 1;
    public const int DistanceBandSpit = 2;
    public const int DistanceBandFar = 3;
    public const int DistanceBandUnknown = 4;

    public const int AngleBandFront = 0;
    public const int AngleBandSide = 1;
    public const int AngleBandRear = 2;
    public const int AngleBandUnknown = 3;

    public const string FaceMotionAssetHandle = "avalon.broodmother.spider.motion.face-target.authored.v1";
    public const string FlankMotionAssetHandle = "avalon.broodmother.spider.motion.flank-position.authored.v1";
    public const string RepositionMotionAssetHandle = "avalon.broodmother.spider.motion.role-position.authored.v1";
    public const string RetreatMotionAssetHandle = "avalon.broodmother.spider.motion.safe-retreat.authored.v1";
    public const string MovementRunClipHandle = "avalon.broodmother.spider.clip.movement-run.authored.v1";
    public const string BiteTelegraphClipHandle = "avalon.broodmother.spider.clip.bite-telegraph.authored.v1";
    public const string BiteAttackClipHandle = "avalon.broodmother.spider.clip.bite-commit.authored.v1";
    public const string LeapTelegraphClipHandle = "avalon.broodmother.spider.clip.leap-telegraph.authored.v1";
    public const string LeapAttackClipHandle = "avalon.broodmother.spider.clip.leap-commit.authored.v1";
    public const string LeapArcMotionHandle = "avalon.broodmother.spider.motion.leap-arc.authored.v1";
    public const string LeapLandingMotionHandle = "avalon.broodmother.spider.motion.leap-landing.authored.v1";
    public const string LeapArcVfxHandle = "avalon.broodmother.spider.vfx.leap-arc.authored.v1";
    public const string GildedSpitTelegraphClipHandle = "avalon.broodmother.spider.clip.gilded-spit-telegraph.authored.v1";
    public const string GildedSpitProjectileHandle = "avalon.broodmother.spider.projectile.gilded-spit.authored.v1";
    public const string GildedSpitVfxHandle = "avalon.broodmother.spider.vfx.gilded-spit.authored.v1";
    public const string BiteDamageWindowHandle = "avalon.broodmother.spider.damage-window.bite.authored.v1";
    public const string LeapContactDamageWindowHandle = "avalon.broodmother.spider.damage-window.leap-contact.authored.v1";
    public const string SpitDamageWindowHandle = "avalon.broodmother.spider.damage-window.spit-projectile.authored.v1";
    public const string ReservationReleaseHandle = "avalon.broodmother.spider.reservation.release.authored.v1";
    public const string SafeStopHandle = "avalon.broodmother.spider.motion.safe-stop.authored.v1";
    public const string RecoveryClipHandle = "avalon.broodmother.spider.clip.recovery.authored.v1";
    public const string GroundedControlReadyHandle = "avalon.broodmother.spider.state.grounded-control-ready.authored.v1";
    public const string InterruptionCleanupHandle = "avalon.broodmother.spider.cleanup.interruption.authored.v1";

    public static PackageId PackageId { get; } = new PackageId(PackageIdValue);

    public static ActorRoleId CrimsonVanguardRole { get; } =
        new ActorRoleId("avalon-broodmother-companion.role.broodmother.crimson-vanguard");

    public static ActorRoleId PalePouncerRole { get; } =
        new ActorRoleId("avalon-broodmother-companion.role.broodmother.pale-pouncer");

    public static ActorRoleId GildedSpitterRole { get; } =
        new ActorRoleId("avalon-broodmother-companion.role.broodmother.gilded-spitter");

    public static ActorRoleId CrimsonHarrierRole { get; } =
        new ActorRoleId("avalon-broodmother-companion.role.spider.crimson-harrier");

    public static ActorRoleId PaleAmbusherRole { get; } =
        new ActorRoleId("avalon-broodmother-companion.role.spider.pale-ambusher");

    public static ActorRoleId GildedFinisherRole { get; } =
        new ActorRoleId("avalon-broodmother-companion.role.spider.gilded-finisher");

    public static IReadOnlyList<ActorRoleId> SupportedActorRoles { get; } = Array.AsReadOnly(new[]
    {
        CrimsonVanguardRole,
        PalePouncerRole,
        GildedSpitterRole,
        CrimsonHarrierRole,
        PaleAmbusherRole,
        GildedFinisherRole,
    });

    public static IReadOnlyList<BroodmotherRoleDefinitionV2> RoleDefinitions { get; } =
        Array.AsReadOnly(new[]
        {
            new BroodmotherRoleDefinitionV2("Broodmother — Crimson Vanguard", CrimsonVanguardRole, CrimsonVanguardRoleCode),
            new BroodmotherRoleDefinitionV2("Broodmother — Pale Pouncer", PalePouncerRole, PalePouncerRoleCode),
            new BroodmotherRoleDefinitionV2("Broodmother — Gilded Spitter", GildedSpitterRole, GildedSpitterRoleCode),
            new BroodmotherRoleDefinitionV2("Crimson Harrier Spider", CrimsonHarrierRole, CrimsonHarrierRoleCode),
            new BroodmotherRoleDefinitionV2("Pale Ambusher Spider", PaleAmbusherRole, PaleAmbusherRoleCode),
            new BroodmotherRoleDefinitionV2("Gilded Finisher Spider", GildedFinisherRole, GildedFinisherRoleCode),
        });

    public static GoalId GoalStopSafely { get; } = Goal("stop-safely");
    public static GoalId GoalRetreat { get; } = Goal("retreat");
    public static GoalId GoalRecover { get; } = Goal("recover");
    public static GoalId GoalVanguardBitePressure { get; } = Goal("vanguard-bite-pressure");
    public static GoalId GoalPouncerLeapPressure { get; } = Goal("pouncer-leap-pressure");
    public static GoalId GoalSpitterRangedPressure { get; } = Goal("spitter-ranged-pressure");
    public static GoalId GoalHarrierBiteCycle { get; } = Goal("harrier-bite-cycle");
    public static GoalId GoalAmbusherWideLeap { get; } = Goal("ambusher-wide-leap");
    public static GoalId GoalFinisherWeakTarget { get; } = Goal("finisher-weak-target");
    public static GoalId GoalFinisherPressure { get; } = Goal("finisher-pressure");

    public static ActionId ActionStopSafely { get; } = Action("stop-safely");
    public static ActionId ActionRetreat { get; } = Action("retreat");
    public static ActionId ActionRecoverControl { get; } = Action("recover-control");
    public static ActionId ActionVanguardClose { get; } = Action("vanguard-close");
    public static ActionId ActionVanguardBite { get; } = Action("vanguard-bite");
    public static ActionId ActionPouncerFlank { get; } = Action("pouncer-flank");
    public static ActionId ActionPouncerLeap { get; } = Action("pouncer-leap");
    public static ActionId ActionSpitterReposition { get; } = Action("spitter-reposition");
    public static ActionId ActionSpitterSpit { get; } = Action("spitter-spit");
    public static ActionId ActionHarrierBite { get; } = Action("harrier-bite");
    public static ActionId ActionHarrierReposition { get; } = Action("harrier-reposition");
    public static ActionId ActionAmbusherWideFlank { get; } = Action("ambusher-wide-flank");
    public static ActionId ActionAmbusherLeap { get; } = Action("ambusher-leap");
    public static ActionId ActionFinisherClaimPosition { get; } = Action("finisher-claim-position");
    public static ActionId ActionFinisherBite { get; } = Action("finisher-bite");

    public static PlanningFactId FactRoleCode { get; } = Fact("role_code");
    public static PlanningFactId FactMustStop { get; } = Fact("must_stop");
    public static PlanningFactId FactMustRetreat { get; } = Fact("must_retreat");
    public static PlanningFactId FactTargetValid { get; } = Fact("target_valid");
    public static PlanningFactId FactTargetWeak { get; } = Fact("target_weak");
    public static PlanningFactId FactDistanceBand { get; } = Fact("distance_band");
    public static PlanningFactId FactAngleBand { get; } = Fact("angle_band");
    public static PlanningFactId FactSlotGranted { get; } = Fact("slot_granted");
    public static PlanningFactId FactSlotKind { get; } = Fact("slot_kind");
    public static PlanningFactId FactRangedLaneClear { get; } = Fact("ranged_lane_clear");
    public static PlanningFactId FactRecoveryRequired { get; } = Fact("recovery_required");
    public static PlanningFactId FactAtFlank { get; } = Fact("at_flank");
    public static PlanningFactId FactAtRoleBand { get; } = Fact("at_role_band");
    public static PlanningFactId FactActionCompleted { get; } = Fact("action_completed");

    public static PlanningTargetKeyId CurrentTarget { get; } =
        new PlanningTargetKeyId("avalon-broodmother-companion.target.current");

    public static ActionCapability CapabilityFace { get; } = Capability("face");
    public static ActionCapability CapabilityMove { get; } = Capability("move");
    public static ActionCapability CapabilityAnimate { get; } = Capability("animate");
    public static ActionCapability CapabilityBite { get; } = Capability("bite");
    public static ActionCapability CapabilityLeap { get; } = Capability("leap");
    public static ActionCapability CapabilitySpit { get; } = Capability("spit");
    public static ActionCapability CapabilityRecover { get; } = Capability("recover");
    public static ActionCapability CapabilityStop { get; } = Capability("stop");

    public static IReadOnlyList<ActionCapability> RequiredCapabilities { get; } = Array.AsReadOnly(new[]
    {
        CapabilityFace,
        CapabilityMove,
        CapabilityAnimate,
        CapabilityBite,
        CapabilityLeap,
        CapabilitySpit,
        CapabilityRecover,
        CapabilityStop,
    });

    public static AvalonProcedureReference BiteProcedure { get; } = Procedure(
        "bite", 1800, CapabilityFace, CapabilityAnimate, CapabilityBite);

    public static AvalonProcedureReference LeapProcedure { get; } = Procedure(
        "leap", 3500, CapabilityFace, CapabilityAnimate, CapabilityLeap);

    public static AvalonProcedureReference SpitProcedure { get; } = Procedure(
        "spit", 3000, CapabilityFace, CapabilityAnimate, CapabilitySpit);

    public static AvalonProcedureReference FlankProcedure { get; } = Procedure(
        "flank", 2500, CapabilityFace, CapabilityAnimate, CapabilityMove);

    public static AvalonProcedureReference RepositionProcedure { get; } = Procedure(
        "reposition", 2200, CapabilityFace, CapabilityAnimate, CapabilityMove);

    public static AvalonProcedureReference RetreatProcedure { get; } = Procedure(
        "retreat", 2500, CapabilityMove, CapabilityAnimate, CapabilityStop);

    public static AvalonProcedureReference RecoveryProcedure { get; } = Procedure(
        "recovery", 1400, CapabilityAnimate, CapabilityRecover);

    public static AvalonProcedureReference InterruptionProcedure { get; } = Procedure(
        "interruption", 500, CapabilityStop);

    public static IReadOnlyList<BroodmotherSpiderAuthoredAssetBindingV2> AuthoredSpiderAssetBindings { get; } =
        Array.AsReadOnly(new[]
        {
            Bind(BiteProcedure.Id, "face.target", "motion.face", FaceMotionAssetHandle),
            Bind(BiteProcedure.Id, "bite.telegraph", "animation.clip", BiteTelegraphClipHandle),
            Bind(BiteProcedure.Id, "bite.animation", "animation.clip", BiteAttackClipHandle),
            Bind(BiteProcedure.Id, "bite.window.open", "damage.window", BiteDamageWindowHandle),
            Bind(BiteProcedure.Id, "bite.damage.commit", "damage.window", BiteDamageWindowHandle),
            Bind(BiteProcedure.Id, "bite.window.close", "damage.window", BiteDamageWindowHandle),
            Bind(BiteProcedure.Id, "reservation.release", "reservation.release", ReservationReleaseHandle),

            Bind(LeapProcedure.Id, "face.launch-vector", "motion.face", FaceMotionAssetHandle),
            Bind(LeapProcedure.Id, "leap.telegraph", "animation.clip", LeapTelegraphClipHandle),
            Bind(LeapProcedure.Id, "leap.animation", "animation.clip", LeapAttackClipHandle),
            Bind(LeapProcedure.Id, "leap.arc", "motion.leap", LeapArcMotionHandle),
            Bind(LeapProcedure.Id, "leap.contact.commit", "damage.window", LeapContactDamageWindowHandle),
            Bind(LeapProcedure.Id, "leap.window.close", "damage.window", LeapContactDamageWindowHandle),
            Bind(LeapProcedure.Id, "leap.safe-landing", "motion.leap", LeapLandingMotionHandle),
            Bind(LeapProcedure.Id, "reservation.release", "reservation.release", ReservationReleaseHandle),

            Bind(SpitProcedure.Id, "face.ranged-lane", "motion.face", FaceMotionAssetHandle),
            Bind(SpitProcedure.Id, "spit.gilded-telegraph", "animation.clip", GildedSpitTelegraphClipHandle),
            Bind(SpitProcedure.Id, "spit.vfx.start", "vfx.spit", GildedSpitVfxHandle),
            Bind(SpitProcedure.Id, "spit.projectile.commit", "projectile.spit", GildedSpitProjectileHandle),
            Bind(SpitProcedure.Id, "spit.impact.poll", "damage.window", SpitDamageWindowHandle),
            Bind(SpitProcedure.Id, "spit.vfx.stop", "vfx.stop", GildedSpitVfxHandle),
            Bind(SpitProcedure.Id, "reservation.release", "reservation.release", ReservationReleaseHandle),

            Bind(FlankProcedure.Id, "face.flank-side", "motion.face", FaceMotionAssetHandle),
            Bind(FlankProcedure.Id, "move.flank-animation", "animation.clip", MovementRunClipHandle),
            Bind(FlankProcedure.Id, "move.flank-position", "motion.move", FlankMotionAssetHandle),

            Bind(RepositionProcedure.Id, "face.role-band", "motion.face", FaceMotionAssetHandle),
            Bind(RepositionProcedure.Id, "move.role-animation", "animation.clip", MovementRunClipHandle),
            Bind(RepositionProcedure.Id, "move.role-position", "motion.move", RepositionMotionAssetHandle),

            Bind(RetreatProcedure.Id, "retreat.window.close", "damage.window", BiteDamageWindowHandle),
            Bind(RetreatProcedure.Id, "reservation.release", "reservation.release", ReservationReleaseHandle),
            Bind(RetreatProcedure.Id, "move.safe-animation", "animation.clip", MovementRunClipHandle),
            Bind(RetreatProcedure.Id, "move.safe-vector", "motion.move", RetreatMotionAssetHandle),
            Bind(RetreatProcedure.Id, "retreat.safe-stop", "motion.stop", SafeStopHandle),

            Bind(RecoveryProcedure.Id, "recovery.stop-attack-motion", "motion.stop", SafeStopHandle),
            Bind(RecoveryProcedure.Id, "recovery.animation", "animation.clip", RecoveryClipHandle),
            Bind(RecoveryProcedure.Id, "recovery.grounded-control-ready", "state.control", GroundedControlReadyHandle),
            Bind(RecoveryProcedure.Id, "recovery.clear-required", "state.control", GroundedControlReadyHandle),

            Bind(InterruptionProcedure.Id, "interruption.mark", "cleanup.interrupt", InterruptionCleanupHandle),
            Bind(InterruptionProcedure.Id, "interruption.damage.close", "damage.window", BiteDamageWindowHandle),
            Bind(InterruptionProcedure.Id, "interruption.commands.cancel", "cleanup.interrupt", InterruptionCleanupHandle),
            Bind(InterruptionProcedure.Id, "interruption.vfx.stop", "vfx.stop", GildedSpitVfxHandle),
            Bind(InterruptionProcedure.Id, "interruption.reservations.release", "reservation.release", ReservationReleaseHandle),
            Bind(InterruptionProcedure.Id, "interruption.local-state.clear", "cleanup.interrupt", InterruptionCleanupHandle),
        });

    public static IReadOnlyList<AvalonProcedureRequirement> ProcedureRequirements { get; } =
        Array.AsReadOnly(new[]
        {
            Requirement(BiteProcedure),
            Requirement(LeapProcedure),
            Requirement(SpitProcedure),
            Requirement(FlankProcedure),
            Requirement(RepositionProcedure),
            Requirement(RetreatProcedure),
            Requirement(RecoveryProcedure),
            Requirement(InterruptionProcedure),
        });

    public static IReadOnlyList<GoalId> GoalIds { get; } = Array.AsReadOnly(new[]
    {
        GoalStopSafely,
        GoalRetreat,
        GoalRecover,
        GoalVanguardBitePressure,
        GoalPouncerLeapPressure,
        GoalSpitterRangedPressure,
        GoalHarrierBiteCycle,
        GoalAmbusherWideLeap,
        GoalFinisherWeakTarget,
        GoalFinisherPressure,
    });

    public static IReadOnlyList<ActionId> ActionIds { get; } = Array.AsReadOnly(new[]
    {
        ActionStopSafely,
        ActionRetreat,
        ActionRecoverControl,
        ActionVanguardClose,
        ActionVanguardBite,
        ActionPouncerFlank,
        ActionPouncerLeap,
        ActionSpitterReposition,
        ActionSpitterSpit,
        ActionHarrierBite,
        ActionHarrierReposition,
        ActionAmbusherWideFlank,
        ActionAmbusherLeap,
        ActionFinisherClaimPosition,
        ActionFinisherBite,
    });

    public static IReadOnlyList<AvalonGoalDefinition> GoalDefinitions { get; } =
        Array.AsReadOnly(new[]
        {
            GoalDefinition(GoalStopSafely, Eq(FactActionCompleted, 1)),
            GoalDefinition(GoalRetreat, Eq(FactActionCompleted, 1)),
            GoalDefinition(GoalRecover, Eq(FactRecoveryRequired, 0)),
            RoleGoal(GoalVanguardBitePressure, CrimsonVanguardRoleCode),
            RoleGoal(GoalPouncerLeapPressure, PalePouncerRoleCode),
            RoleGoal(GoalSpitterRangedPressure, GildedSpitterRoleCode),
            GoalDefinition(
                GoalHarrierBiteCycle,
                Eq(FactRoleCode, CrimsonHarrierRoleCode),
                Eq(FactActionCompleted, 1),
                Eq(FactAtRoleBand, 1),
                Eq(FactRecoveryRequired, 0)),
            RoleGoal(GoalAmbusherWideLeap, PaleAmbusherRoleCode),
            RoleGoal(GoalFinisherWeakTarget, GildedFinisherRoleCode),
            RoleGoal(GoalFinisherPressure, GildedFinisherRoleCode),
        });

    public static IReadOnlyList<AvalonActionDefinition> ActionDefinitions { get; } =
        Array.AsReadOnly(new[]
        {
            Define(
                ActionStopSafely,
                new[] { Eq(FactMustStop, 1) },
                new[] { Set(FactActionCompleted, 1) },
                default,
                new ConstantActionCostProvider(0f),
                CapabilityStop,
                InterruptPolicy.Always,
                InterruptionProcedure),
            Define(
                ActionRetreat,
                new[] { Eq(FactMustStop, 0), Eq(FactMustRetreat, 1) },
                new[] { Set(FactActionCompleted, 1) },
                default,
                new ConstantActionCostProvider(0.5f),
                CapabilityMove,
                InterruptPolicy.Always,
                RetreatProcedure),
            Define(
                ActionRecoverControl,
                new[] { Eq(FactMustStop, 0), Eq(FactRecoveryRequired, 1) },
                new[] { Set(FactRecoveryRequired, 0) },
                default,
                new BroodmotherRecoveryActionCostProvider(),
                CapabilityRecover,
                InterruptPolicy.OnDamage,
                RecoveryProcedure),
            DefineMovement(
                ActionVanguardClose,
                CrimsonVanguardRoleCode,
                new[] { Eq(FactAtRoleBand, 0) },
                new[] { Set(FactAtRoleBand, 1) },
                2f,
                DistanceBandBite,
                RepositionProcedure),
            DefineAttack(
                ActionVanguardBite,
                CrimsonVanguardRoleCode,
                new[] { Eq(FactAtRoleBand, 1) },
                AttackEffects(),
                1f,
                DistanceBandBite,
                SlotKindBite,
                CapabilityBite,
                InterruptPolicy.OnInvalidTarget,
                BiteProcedure),
            DefineMovement(
                ActionPouncerFlank,
                PalePouncerRoleCode,
                new[] { Eq(FactAtFlank, 0) },
                new[] { Set(FactAtFlank, 1), Set(FactAtRoleBand, 1) },
                1.5f,
                DistanceBandLeap,
                FlankProcedure),
            DefineAttack(
                ActionPouncerLeap,
                PalePouncerRoleCode,
                new[] { Eq(FactAtFlank, 1), Eq(FactAtRoleBand, 1) },
                LeapAttackEffects(),
                1f,
                DistanceBandLeap,
                SlotKindLeap,
                CapabilityLeap,
                InterruptPolicy.OnDamage,
                LeapProcedure),
            DefineMovement(
                ActionSpitterReposition,
                GildedSpitterRoleCode,
                new[] { Eq(FactAtRoleBand, 0) },
                new[] { Set(FactAtRoleBand, 1) },
                1f,
                DistanceBandSpit,
                RepositionProcedure),
            DefineAttack(
                ActionSpitterSpit,
                GildedSpitterRoleCode,
                new[] { Eq(FactAtRoleBand, 1), Eq(FactRangedLaneClear, 1) },
                AttackEffects(),
                1f,
                DistanceBandSpit,
                SlotKindSpit,
                CapabilitySpit,
                InterruptPolicy.OnThreatIncrease,
                SpitProcedure),
            DefineAttack(
                ActionHarrierBite,
                CrimsonHarrierRoleCode,
                new[] { Eq(FactAtRoleBand, 1) },
                new[] { Set(FactActionCompleted, 1), Set(FactAtRoleBand, 0) },
                0.8f,
                DistanceBandBite,
                SlotKindBite,
                CapabilityBite,
                InterruptPolicy.OnInvalidTarget,
                BiteProcedure),
            DefineMovement(
                ActionHarrierReposition,
                CrimsonHarrierRoleCode,
                new[] { Eq(FactActionCompleted, 1), Eq(FactAtRoleBand, 0) },
                new[] { Set(FactAtRoleBand, 1), Set(FactRecoveryRequired, 1) },
                0.8f,
                DistanceBandBite,
                RepositionProcedure),
            DefineMovement(
                ActionAmbusherWideFlank,
                PaleAmbusherRoleCode,
                new[] { Eq(FactAtFlank, 0) },
                new[] { Set(FactAtFlank, 1), Set(FactAtRoleBand, 1) },
                1f,
                DistanceBandLeap,
                FlankProcedure),
            DefineAttack(
                ActionAmbusherLeap,
                PaleAmbusherRoleCode,
                new[] { Eq(FactAtFlank, 1), Eq(FactAtRoleBand, 1) },
                LeapAttackEffects(),
                0.9f,
                DistanceBandLeap,
                SlotKindLeap,
                CapabilityLeap,
                InterruptPolicy.OnDamage,
                LeapProcedure),
            DefineMovement(
                ActionFinisherClaimPosition,
                GildedFinisherRoleCode,
                new[] { Eq(FactAtRoleBand, 0) },
                new[] { Set(FactAtRoleBand, 1) },
                0.8f,
                DistanceBandBite,
                RepositionProcedure,
                finisher: true),
            DefineAttack(
                ActionFinisherBite,
                GildedFinisherRoleCode,
                new[] { Eq(FactAtRoleBand, 1) },
                AttackEffects(),
                0.6f,
                DistanceBandBite,
                SlotKindBite,
                CapabilityBite,
                InterruptPolicy.OnInvalidTarget,
                BiteProcedure,
                finisher: true),
        });

    public static BlackboardKey<bool> LifecycleActive { get; } = Host<bool>("lifecycle.active");
    public static BlackboardKey<bool> LifecycleCleanupRequested { get; } = Host<bool>("lifecycle.cleanup_requested");
    public static BlackboardKey<bool> LeaseValid { get; } = Host<bool>("lease.valid");
    public static BlackboardKey<bool> ExecutorBlazeOwned { get; } = Host<bool>("executor.blaze_owned");
    public static BlackboardKey<int> SelfHealthBand { get; } = Host<int>("self.health_band");
    public static BlackboardKey<int> SelfDamageRevision { get; } = Host<int>("self.damage_revision");
    public static BlackboardKey<bool> TargetValid { get; } = Host<bool>("target.valid");
    public static BlackboardKey<string> TargetId { get; } = Host<string>("target.id");
    public static BlackboardKey<int> TargetDistanceBand { get; } = Host<int>("target.distance_band");
    public static BlackboardKey<int> TargetAngleBand { get; } = Host<int>("target.angle_band");
    public static BlackboardKey<int> TargetHealthBand { get; } = Host<int>("target.health_band");
    public static BlackboardKey<int> TargetObservationRevision { get; } = Host<int>("target.observation_revision");
    public static BlackboardKey<string> PackId { get; } = Host<string>("pack.id");
    public static BlackboardKey<int> PackMemberCount { get; } = Host<int>("pack.member_count");
    public static BlackboardKey<bool> PackAttackSlotGranted { get; } = Host<bool>("pack.attack_slot_granted");
    public static BlackboardKey<int> PackAttackSlotGeneration { get; } = Host<int>("pack.attack_slot_generation");
    public static BlackboardKey<int> PackAttackSlotKind { get; } = Host<int>("pack.attack_slot_kind");
    public static BlackboardKey<int> PackFlankSide { get; } = Host<int>("pack.flank_side");
    public static BlackboardKey<bool> PackRangedLaneClear { get; } = Host<bool>("pack.ranged_lane_clear");
    public static BlackboardKey<bool> PackFinisherClaimGranted { get; } = Host<bool>("pack.finisher_claim_granted");
    public static BlackboardKey<bool> AssetBiteReady { get; } = Host<bool>("asset.bite_ready");
    public static BlackboardKey<bool> AssetLeapReady { get; } = Host<bool>("asset.leap_ready");
    public static BlackboardKey<bool> AssetSpitReady { get; } = Host<bool>("asset.spit_ready");

    public static BlackboardKey<bool> MustStop { get; } = Derived<bool>("must_stop");
    public static BlackboardKey<bool> MustRetreat { get; } = Derived<bool>("must_retreat");
    public static BlackboardKey<bool> MayRecover { get; } = Derived<bool>("may_recover");
    public static BlackboardKey<bool> MayBite { get; } = Derived<bool>("may_bite");
    public static BlackboardKey<bool> MayLeap { get; } = Derived<bool>("may_leap");
    public static BlackboardKey<bool> MaySpit { get; } = Derived<bool>("may_spit");
    public static BlackboardKey<bool> TargetWeak { get; } = Derived<bool>("target_weak");
    public static BlackboardKey<bool> NeedsFlank { get; } = Derived<bool>("needs_flank");
    public static BlackboardKey<bool> NeedsReposition { get; } = Derived<bool>("needs_reposition");

    public static BlackboardKey<string> RoleBoundId { get; } = Local<string>("role.bound_id");
    public static BlackboardKey<string> IntentCurrentGoal { get; } = Local<string>("intent.current_goal");
    public static BlackboardKey<int> IntentPlanGeneration { get; } = Local<int>("intent.plan_generation");
    public static BlackboardKey<string> TargetLastValidId { get; } = Local<string>("target.last_valid_id");
    public static BlackboardKey<int> TargetLastValidRevision { get; } = Local<int>("target.last_valid_revision");
    public static BlackboardKey<string> ActionLastId { get; } = Local<string>("action.last_id");
    public static BlackboardKey<int> ActionLastOutcome { get; } = Local<int>("action.last_outcome");
    public static BlackboardKey<int> ActionFailureStreak { get; } = Local<int>("action.failure_streak");
    public static BlackboardKey<int> ActionCooldownBiteUntil { get; } = Local<int>("action.cooldown_bite_until");
    public static BlackboardKey<int> ActionCooldownLeapUntil { get; } = Local<int>("action.cooldown_leap_until");
    public static BlackboardKey<int> ActionCooldownSpitUntil { get; } = Local<int>("action.cooldown_spit_until");
    public static BlackboardKey<string> ProcedureActiveId { get; } = Local<string>("procedure.active_id");
    public static BlackboardKey<string> ProcedureStage { get; } = Local<string>("procedure.stage");
    public static BlackboardKey<bool> ProcedureCommitOpen { get; } = Local<bool>("procedure.commit_open");
    public static BlackboardKey<bool> RecoveryRequired { get; } = Local<bool>("recovery.required");
    public static BlackboardKey<int> PackLastSlotGeneration { get; } = Local<int>("pack.last_slot_generation");

    public static IReadOnlyList<BlackboardKeyDeclaration> HostAuthoritativeKeyDeclarations { get; } =
        Array.AsReadOnly(new[]
        {
            LifecycleActive.Declaration,
            LifecycleCleanupRequested.Declaration,
            LeaseValid.Declaration,
            ExecutorBlazeOwned.Declaration,
            SelfHealthBand.Declaration,
            SelfDamageRevision.Declaration,
            TargetValid.Declaration,
            TargetId.Declaration,
            TargetDistanceBand.Declaration,
            TargetAngleBand.Declaration,
            TargetHealthBand.Declaration,
            TargetObservationRevision.Declaration,
            PackId.Declaration,
            PackMemberCount.Declaration,
            PackAttackSlotGranted.Declaration,
            PackAttackSlotGeneration.Declaration,
            PackAttackSlotKind.Declaration,
            PackFlankSide.Declaration,
            PackRangedLaneClear.Declaration,
            PackFinisherClaimGranted.Declaration,
            AssetBiteReady.Declaration,
            AssetLeapReady.Declaration,
            AssetSpitReady.Declaration,
        });

    public static IReadOnlyList<BlackboardKeyDeclaration> DerivedKeyDeclarations { get; } =
        Array.AsReadOnly(new[]
        {
            MustStop.Declaration,
            MustRetreat.Declaration,
            MayRecover.Declaration,
            MayBite.Declaration,
            MayLeap.Declaration,
            MaySpit.Declaration,
            TargetWeak.Declaration,
            NeedsFlank.Declaration,
            NeedsReposition.Declaration,
        });

    public static IReadOnlyList<BlackboardKeyDeclaration> PackageLocalKeyDeclarations { get; } =
        Array.AsReadOnly(new[]
        {
            RoleBoundId.Declaration,
            IntentCurrentGoal.Declaration,
            IntentPlanGeneration.Declaration,
            TargetLastValidId.Declaration,
            TargetLastValidRevision.Declaration,
            ActionLastId.Declaration,
            ActionLastOutcome.Declaration,
            ActionFailureStreak.Declaration,
            ActionCooldownBiteUntil.Declaration,
            ActionCooldownLeapUntil.Declaration,
            ActionCooldownSpitUntil.Declaration,
            ProcedureActiveId.Declaration,
            ProcedureStage.Declaration,
            ProcedureCommitOpen.Declaration,
            RecoveryRequired.Declaration,
            PackLastSlotGeneration.Declaration,
        });

    public static bool TryGetRoleCode(ActorRoleId role, out int roleCode)
    {
        if (role == CrimsonVanguardRole) roleCode = CrimsonVanguardRoleCode;
        else if (role == PalePouncerRole) roleCode = PalePouncerRoleCode;
        else if (role == GildedSpitterRole) roleCode = GildedSpitterRoleCode;
        else if (role == CrimsonHarrierRole) roleCode = CrimsonHarrierRoleCode;
        else if (role == PaleAmbusherRole) roleCode = PaleAmbusherRoleCode;
        else if (role == GildedFinisherRole) roleCode = GildedFinisherRoleCode;
        else
        {
            roleCode = 0;
            return false;
        }

        return true;
    }

    private static AvalonGoalDefinition RoleGoal(GoalId id, int roleCode)
    {
        return GoalDefinition(
            id,
            Eq(FactRoleCode, roleCode),
            Eq(FactActionCompleted, 1),
            Eq(FactRecoveryRequired, 0));
    }

    private static AvalonActionDefinition DefineMovement(
        ActionId id,
        int roleCode,
        IReadOnlyList<PlanningCondition> additionalConditions,
        IReadOnlyList<PlanningEffect> effects,
        float baseCost,
        int preferredDistanceBand,
        AvalonProcedureReference procedure,
        bool finisher = false)
    {
        return Define(
            id,
            Combine(RoleConditions(roleCode), additionalConditions),
            effects,
            CurrentTarget,
            new BroodmotherRoleActionCostProvider(
                baseCost,
                preferredDistanceBand,
                SlotKindNone,
                leap: false,
                spit: false,
                finisher),
            CapabilityMove,
            InterruptPolicy.OnThreatIncrease,
            procedure);
    }

    private static AvalonActionDefinition DefineAttack(
        ActionId id,
        int roleCode,
        IReadOnlyList<PlanningCondition> additionalConditions,
        IReadOnlyList<PlanningEffect> effects,
        float baseCost,
        int preferredDistanceBand,
        int slotKind,
        ActionCapability capability,
        InterruptPolicy interruptPolicy,
        AvalonProcedureReference procedure,
        bool finisher = false)
    {
        return Define(
            id,
            Combine(
                RoleConditions(roleCode),
                additionalConditions,
                new[] { Eq(FactSlotGranted, 1), Eq(FactSlotKind, slotKind) }),
            effects,
            CurrentTarget,
            new BroodmotherRoleActionCostProvider(
                baseCost,
                preferredDistanceBand,
                slotKind,
                leap: slotKind == SlotKindLeap,
                spit: slotKind == SlotKindSpit,
                finisher),
            capability,
            interruptPolicy,
            procedure);
    }

    private static AvalonActionDefinition Define(
        ActionId id,
        IEnumerable<PlanningCondition> conditions,
        IEnumerable<PlanningEffect> effects,
        PlanningTargetKeyId target,
        IAvalonActionCostProvider cost,
        ActionCapability capability,
        InterruptPolicy interruptPolicy,
        AvalonProcedureReference procedure)
    {
        return new AvalonActionDefinition(
            id,
            conditions,
            effects,
            target,
            cost,
            capability,
            interruptPolicy,
            procedure.Timeout,
            AvalonActionExecutionProfile.ForProcedure(procedure));
    }

    private static PlanningCondition[] RoleConditions(int roleCode)
    {
        return new[]
        {
            Eq(FactRoleCode, roleCode),
            Eq(FactMustStop, 0),
            Eq(FactMustRetreat, 0),
            Eq(FactRecoveryRequired, 0),
            Eq(FactTargetValid, 1),
        };
    }

    private static PlanningCondition[] Combine(params IReadOnlyList<PlanningCondition>[] groups)
    {
        var count = 0;
        for (var index = 0; index < groups.Length; index++) count += groups[index].Count;
        var result = new PlanningCondition[count];
        var cursor = 0;
        for (var groupIndex = 0; groupIndex < groups.Length; groupIndex++)
        {
            for (var itemIndex = 0; itemIndex < groups[groupIndex].Count; itemIndex++)
            {
                result[cursor++] = groups[groupIndex][itemIndex];
            }
        }

        return result;
    }

    private static PlanningEffect[] AttackEffects()
    {
        return new[]
        {
            Set(FactActionCompleted, 1),
            Set(FactRecoveryRequired, 1),
        };
    }

    private static PlanningEffect[] LeapAttackEffects()
    {
        return new[]
        {
            Set(FactActionCompleted, 1),
            Set(FactRecoveryRequired, 1),
            Set(FactAtFlank, 0),
            Set(FactAtRoleBand, 0),
        };
    }

    private static AvalonGoalDefinition GoalDefinition(GoalId id, params PlanningCondition[] desiredState) =>
        new AvalonGoalDefinition(id, desiredState);

    private static PlanningCondition Eq(PlanningFactId fact, int value) =>
        new PlanningCondition(fact, PlanningComparison.Equal, value);

    private static PlanningEffect Set(PlanningFactId fact, int value) =>
        new PlanningEffect(fact, value);

    private static GoalId Goal(string suffix) =>
        new GoalId("avalon-broodmother-companion.goal." + suffix);

    private static ActionId Action(string suffix) =>
        new ActionId("avalon-broodmother-companion.action." + suffix);

    private static PlanningFactId Fact(string suffix) =>
        new PlanningFactId("avalon-broodmother-companion.fact." + suffix);

    private static ActionCapability Capability(string suffix) =>
        new ActionCapability("avalon.broodmother.spider." + suffix);

    private static AvalonProcedureReference Procedure(
        string suffix,
        int timeoutMilliseconds,
        params ActionCapability[] capabilities)
    {
        return new AvalonProcedureReference(
            new AvalonProcedureId("avalon.broodmother.spider.procedure." + suffix + ".v1"),
            new AvalonProcedureVersion("v1"),
            capabilities,
            TimeSpan.FromMilliseconds(timeoutMilliseconds),
            mutatesActor: true);
    }

    private static AvalonProcedureRequirement Requirement(AvalonProcedureReference procedure) =>
        new AvalonProcedureRequirement(procedure.Id, procedure.RequiredVersion, procedure.RequiredCapabilities);

    private static BroodmotherSpiderAuthoredAssetBindingV2 Bind(
        AvalonProcedureId procedureId,
        string stageId,
        string assetKind,
        string assetHandle) =>
        new BroodmotherSpiderAuthoredAssetBindingV2(procedureId, stageId, assetKind, assetHandle);

    private static BlackboardKey<T> Host<T>(string name) =>
        new BlackboardKey<T>(HostBlackboardNamespace, name, BlackboardSchemaVersion, BlackboardAccess.Authoritative, BlackboardScope.Actor);

    private static BlackboardKey<T> Derived<T>(string name) =>
        new BlackboardKey<T>(DerivedBlackboardNamespace, name, BlackboardSchemaVersion, BlackboardAccess.Derived, BlackboardScope.Actor);

    private static BlackboardKey<T> Local<T>(string name) =>
        new BlackboardKey<T>(BlackboardNamespace, name, BlackboardSchemaVersion, BlackboardAccess.PackageLocal, BlackboardScope.Actor);
}
