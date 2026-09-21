using System;
using System.Collections.Generic;
using AvalonAI.Contracts.V2;

namespace DragonKnight.AI.Package.V2;

public static class DragonKnightAiV2Contract
{
    public const string PackageIdValue = "dragon-knight.boss.ai.v2";
    public const string AssemblyNameValue = "DragonKnight.AI.Package.V2";
    public const string BossName = "Sir Vaelor, the Ashen Dragon Knight";
    public const string DisplayName = "Dragon Knight Boss AI - Sir Vaelor";
    public const string PackageVersion = "0.1.0-air49";
    public const string SupportedContractVersion = "2.0";
    public const string PackageMarkerString = "DRAGON_KNIGHT_AIR49_AUTHORITY_PACKET_PASS";

    public const string BossActorRoleIdValue = "dragon-knight.boss";
    public const string RequiredOwnerId = "dragon-knight.boss.host";
    public const string DiagnosticOwnerId = "dragon-knight.dk4.diagnostic-host";

    public const string ActorIdPrefix = "foa.location:";
    public const string TargetIdPrefix = "foa.location:";

    public const string NpcTemplateGuid = "00f608ee051b57748a6a9ed8dae28678";
    public const string LocationTemplateGuid = "d7b09116519f7564593be62781bee3db";
    public const string TemplateSource = "DragonKnight DK4A finalized template proof";
    public const string SceneName = "CampaignMap_HOS";
    public const string ArenaName = "Ancient Cromlech / Stonehenge";

    public const int OuterWakeRadiusMeters = 25;
    public const int InnerFightStartRadiusMeters = 5;
    public const int SoftLeashRadiusMeters = 22;
    public const int HardLeashRadiusMeters = 25;

    public const bool DefaultOff = true;
    public const bool KillSwitchDefault = true;
    public const bool DirectNativeCalls = false;
    public const bool NativeControl = false;
    public const bool RabbitBypass = false;
    public const bool RabbitWritesByDefault = false;
    public const bool GoapBypass = false;
    public const bool BlazeBypass = false;
    public const bool PlayMakerNativeCalls = false;
    public const bool SaveWrites = false;

    public const string DefaultConfigKey = "DragonKnightBossAI.Enabled=false";
    public const string KillSwitchConfigKey = "DragonKnightBossAI.KillSwitch=true";
    public const string ActivationTrigger = "dragon-knight.encounter.inner-ring-entered";

    public const string BlackboardNamespace = "dragon_knight.boss.vaelor.v1";
    public const int BlackboardSchemaVersion = 1;
    public const string RabbitPersistence = "session-only";

    public const string PlayerTargetKind = "player";
    public const string LocationTargetKind = "foa.location";
    public const string CompanionTargetKind = "companion";
    public const string ShrineTargetKind = "shrine";
    public const string AltarTargetKind = "altar";
    public const string DiscoveryTargetKind = "discovery";
    public const string PromptVisibilityTargetKind = "prompt-visibility";

    public const string ForbiddenAltarTrigger = "AltarInteract";
    public const string ForbiddenDiscoveryTrigger = "Spec_Discovery_Small_Stonecircle";

    public const float PhaseOneMinimumHpFraction = 0.25f;
    public const float PhaseTwoTriggerHpFraction = 0.25f;
    public const string RefillPolicy = "none";
    public const string PhaseTransitionLockout = "one-shot-session-lock";

    public const string UseInteractableProcedureIdValue = "avalon.core.use-interactable.v1";
    public const string UseInteractableProcedureVersionValue = "v1";
    public const string ApproachBridgeProcedure = "approach target";
    public const string FaceBridgeProcedure = "face target";
    public const string InteractOrAttackBridgeProcedure = "interact/attack request only after approved combat gate";

    public const string CapabilityObserveActorValue = "observe_actor";
    public const string CapabilityObserveTargetValue = "observe_target";
    public const string CapabilityFaceTargetValue = "face_target";
    public const string CapabilityApproachTargetValue = "approach_target";
    public const string CapabilityMeleeAttackRequestValue = "melee_attack_request";
    public const string CapabilityPhaseTransitionRequestValue = "phase_transition_request";
    public const string CapabilityLeashReturnRequestValue = "leash_return_request";
    public const string CapabilityStopAllRequestValue = "stop_all_request";

    public const string GoalValidateActorValue = "dragon-knight.boss.validate-actor";
    public const string GoalValidateTargetValue = "dragon-knight.boss.validate-target";
    public const string GoalMaintainLeaseValue = "dragon-knight.boss.maintain-lease";
    public const string GoalAwakenPresentationValue = "dragon-knight.boss.awaken-presentation";
    public const string GoalEngageTargetValue = "dragon-knight.boss.engage-target";
    public const string GoalMaintainLeashValue = "dragon-knight.boss.maintain-leash";
    public const string GoalPhaseTwoTransitionValue = "dragon-knight.boss.phase-two-transition";
    public const string GoalFailClosedStopValue = "dragon-knight.boss.fail-closed-stop";

    public const string ActionObserveActorValue = "dragon-knight.boss.observe-actor";
    public const string ActionObserveTargetValue = "dragon-knight.boss.observe-target";
    public const string ActionFaceTargetValue = "dragon-knight.boss.face-target";
    public const string ActionApproachTargetValue = "dragon-knight.boss.approach-target";
    public const string ActionMeleeAttackRequestValue = "dragon-knight.boss.melee-attack-request";
    public const string ActionPhaseTransitionRequestValue = "dragon-knight.boss.phase-transition-request";
    public const string ActionLeashReturnRequestValue = "dragon-knight.boss.leash-return-request";
    public const string ActionStopAllRequestValue = "dragon-knight.boss.stop-all-request";

    private const string FactActorValidValue = "dragon-knight.boss.fact.actor-valid";
    private const string FactTargetValidValue = "dragon-knight.boss.fact.target-valid";
    private const string FactLeaseCurrentValue = "dragon-knight.boss.fact.lease-current";
    private const string FactAwakeValue = "dragon-knight.boss.fact.awake";
    private const string FactEngagedValue = "dragon-knight.boss.fact.engaged";
    private const string FactLeashMaintainedValue = "dragon-knight.boss.fact.leash-maintained";
    private const string FactPhaseTwoActiveValue = "dragon-knight.boss.fact.phase-two-active";
    private const string FactFailClosedValue = "dragon-knight.boss.fact.fail-closed";

    private const string SelfTargetKeyValue = "dragon-knight.boss.target.self";
    private const string CurrentTargetKeyValue = "dragon-knight.boss.target.current";

    public static PackageId PackageId { get; } = new PackageId(PackageIdValue);

    public static ActorRoleId BossActorRoleId { get; } = new ActorRoleId(BossActorRoleIdValue);

    public static AvalonProcedureId UseInteractableProcedureId { get; } =
        new AvalonProcedureId(UseInteractableProcedureIdValue);

    public static AvalonProcedureVersion UseInteractableProcedureVersion { get; } =
        new AvalonProcedureVersion(UseInteractableProcedureVersionValue);

    public static IReadOnlyList<GoalId> GoalIds { get; } = Array.AsReadOnly(new[]
    {
        new GoalId(GoalValidateActorValue),
        new GoalId(GoalValidateTargetValue),
        new GoalId(GoalMaintainLeaseValue),
        new GoalId(GoalAwakenPresentationValue),
        new GoalId(GoalEngageTargetValue),
        new GoalId(GoalMaintainLeashValue),
        new GoalId(GoalPhaseTwoTransitionValue),
        new GoalId(GoalFailClosedStopValue),
    });

    public static IReadOnlyList<ActionId> ActionIds { get; } = Array.AsReadOnly(new[]
    {
        new ActionId(ActionObserveActorValue),
        new ActionId(ActionObserveTargetValue),
        new ActionId(ActionFaceTargetValue),
        new ActionId(ActionApproachTargetValue),
        new ActionId(ActionMeleeAttackRequestValue),
        new ActionId(ActionPhaseTransitionRequestValue),
        new ActionId(ActionLeashReturnRequestValue),
        new ActionId(ActionStopAllRequestValue),
    });

    public static IReadOnlyList<ActionCapability> RequiredCapabilities { get; } = Array.AsReadOnly(new[]
    {
        new ActionCapability(CapabilityObserveActorValue),
        new ActionCapability(CapabilityObserveTargetValue),
        new ActionCapability(CapabilityFaceTargetValue),
        new ActionCapability(CapabilityApproachTargetValue),
        new ActionCapability(CapabilityMeleeAttackRequestValue),
        new ActionCapability(CapabilityPhaseTransitionRequestValue),
        new ActionCapability(CapabilityLeashReturnRequestValue),
        new ActionCapability(CapabilityStopAllRequestValue),
    });

    public static IReadOnlyList<BlackboardKeyDeclaration> BlackboardKeys { get; } = Array.AsReadOnly(new[]
    {
        Key<string>("actor.id"),
        Key<string>("actor.location_id"),
        Key<string>("actor.owner_id"),
        Key<string>("actor.lease_id"),
        Key<string>("target.id"),
        Key<string>("target.location_id"),
        Key<AvalonPosition>("arena.center", BlackboardScope.World),
        Key<float>("arena.outer_wake_radius_m", BlackboardScope.World),
        Key<float>("arena.inner_fight_radius_m", BlackboardScope.World),
        Key<float>("arena.soft_leash_radius_m", BlackboardScope.World),
        Key<float>("arena.hard_leash_radius_m", BlackboardScope.World),
        Key<string>("encounter.state"),
        Key<int>("encounter.phase"),
        Key<float>("combat.hp_fraction"),
        Key<string>("capability.state"),
        Key<bool>("kill_switch.active"),
    });

    public static IReadOnlyList<BlackboardKeyDeclaration> PersistentKeys { get; } =
        Array.AsReadOnly(Array.Empty<BlackboardKeyDeclaration>());

    public static IReadOnlyList<AvalonProcedureRequirement> ProcedureRequirements { get; } = Array.AsReadOnly(new[]
    {
        new AvalonProcedureRequirement(
            UseInteractableProcedureId,
            UseInteractableProcedureVersion,
            new[]
            {
                new ActionCapability(CapabilityFaceTargetValue),
                new ActionCapability(CapabilityApproachTargetValue),
                new ActionCapability(CapabilityMeleeAttackRequestValue),
            }),
    });

    public static IReadOnlyList<AvalonGoalDefinition> GoalDefinitions { get; } = Array.AsReadOnly(new[]
    {
        Goal(GoalValidateActorValue, FactActorValidValue),
        Goal(GoalValidateTargetValue, FactTargetValidValue),
        Goal(GoalMaintainLeaseValue, FactLeaseCurrentValue),
        Goal(GoalAwakenPresentationValue, FactAwakeValue),
        Goal(GoalEngageTargetValue, FactEngagedValue),
        Goal(GoalMaintainLeashValue, FactLeashMaintainedValue),
        Goal(GoalPhaseTwoTransitionValue, FactPhaseTwoActiveValue),
        Goal(GoalFailClosedStopValue, FactFailClosedValue),
    });

    public static IReadOnlyList<DragonKnightPlanningActionContract> PlanningActions { get; } = Array.AsReadOnly(new[]
    {
        PlanningAction(
            "observe_actor",
            ActionObserveActorValue,
            CapabilityObserveActorValue,
            1,
            TimeSpan.FromMilliseconds(250),
            "package enabled; kill switch false",
            "actor observation requested",
            "fail closed without native call"),
        PlanningAction(
            "observe_target",
            ActionObserveTargetValue,
            CapabilityObserveTargetValue,
            1,
            TimeSpan.FromMilliseconds(250),
            "actor valid; target candidate present",
            "target observation requested",
            "fail closed without native call"),
        PlanningAction(
            "face_target",
            ActionFaceTargetValue,
            CapabilityFaceTargetValue,
            2,
            TimeSpan.FromMilliseconds(500),
            "lease current; target valid; capability granted",
            "Avalon face-target request emitted",
            "rejected when lease, target, capability, or kill switch is invalid"),
        PlanningAction(
            "approach_target",
            ActionApproachTargetValue,
            CapabilityApproachTargetValue,
            4,
            TimeSpan.FromMilliseconds(750),
            "lease current; target valid; inside arena; capability granted",
            "Avalon approach-target request emitted",
            "rejected when lease, target, capability, or kill switch is invalid"),
        PlanningAction(
            "melee_attack_request",
            ActionMeleeAttackRequestValue,
            CapabilityMeleeAttackRequestValue,
            5,
            TimeSpan.FromSeconds(2),
            "lease current; target valid; approved combat gate",
            "Avalon interact/attack request emitted",
            "blocked until combat gate is approved"),
        PlanningAction(
            "phase_transition_request",
            ActionPhaseTransitionRequestValue,
            CapabilityPhaseTransitionRequestValue,
            6,
            TimeSpan.MaxValue,
            "phase one; hp fraction at or below 0.25; no phase lock",
            "phase two transition request emitted",
            "blocked until actor, animation, and combat proof passes"),
        PlanningAction(
            "leash_return_request",
            ActionLeashReturnRequestValue,
            CapabilityLeashReturnRequestValue,
            3,
            TimeSpan.FromSeconds(1),
            "actor outside soft leash or hard leash policy",
            "Avalon leash return request emitted",
            "fail closed without native call"),
        PlanningAction(
            "stop_all_request",
            ActionStopAllRequestValue,
            CapabilityStopAllRequestValue,
            1,
            TimeSpan.Zero,
            "any authority check fails",
            "package action stopped",
            "terminal fail-closed stop"),
    });

    public static IReadOnlyList<AvalonActionDefinition> ActionDefinitions { get; } = Array.AsReadOnly(new[]
    {
        Action(
            ActionObserveActorValue,
            CapabilityObserveActorValue,
            1,
            SelfTargetKeyValue,
            Array.Empty<PlanningCondition>(),
            new[] { Effect(FactActorValidValue, 1) },
            useProcedure: false),
        Action(
            ActionObserveTargetValue,
            CapabilityObserveTargetValue,
            1,
            CurrentTargetKeyValue,
            new[] { Condition(FactActorValidValue, 1) },
            new[] { Effect(FactTargetValidValue, 1) },
            useProcedure: false),
        Action(
            ActionFaceTargetValue,
            CapabilityFaceTargetValue,
            2,
            CurrentTargetKeyValue,
            ValidActorTargetLeaseConditions(),
            new[] { Effect(FactEngagedValue, 1) },
            useProcedure: true),
        Action(
            ActionApproachTargetValue,
            CapabilityApproachTargetValue,
            4,
            CurrentTargetKeyValue,
            ValidActorTargetLeaseConditions(),
            new[] { Effect(FactEngagedValue, 1) },
            useProcedure: true),
        Action(
            ActionMeleeAttackRequestValue,
            CapabilityMeleeAttackRequestValue,
            5,
            CurrentTargetKeyValue,
            ValidActorTargetLeaseConditions(),
            new[] { Effect(FactEngagedValue, 1) },
            useProcedure: true),
        Action(
            ActionPhaseTransitionRequestValue,
            CapabilityPhaseTransitionRequestValue,
            6,
            SelfTargetKeyValue,
            new[]
            {
                Condition(FactActorValidValue, 1),
                Condition(FactLeaseCurrentValue, 1),
            },
            new[] { Effect(FactPhaseTwoActiveValue, 1) },
            useProcedure: false),
        Action(
            ActionLeashReturnRequestValue,
            CapabilityLeashReturnRequestValue,
            3,
            SelfTargetKeyValue,
            new[] { Condition(FactActorValidValue, 1) },
            new[] { Effect(FactLeashMaintainedValue, 1) },
            useProcedure: false),
        Action(
            ActionStopAllRequestValue,
            CapabilityStopAllRequestValue,
            1,
            SelfTargetKeyValue,
            Array.Empty<PlanningCondition>(),
            new[] { Effect(FactFailClosedValue, 1) },
            useProcedure: false),
    });

    public static string PackageMarkerLine =>
        PackageMarkerString
        + " package=" + PackageIdValue
        + " assembly=" + AssemblyNameValue
        + " actor-id-format=" + ActorIdPrefix + "<Location.ID>"
        + " target-id-format=" + TargetIdPrefix + "<Location.ID>"
        + " default-off=1 kill-switch-default=1 direct-native=0 rabbit-bypass=0 goap-bypass=0 blaze-bypass=0 save=0";

    public static string ToActorId(string liveLocationId)
    {
        return ActorIdPrefix + RequireText(liveLocationId, nameof(liveLocationId));
    }

    public static string ToTargetId(string liveLocationId)
    {
        return TargetIdPrefix + RequireText(liveLocationId, nameof(liveLocationId));
    }

    public static DragonKnightAuthorityDecision ValidateActor(DragonKnightBossDecisionInput input)
    {
        input = input ?? throw new ArgumentNullException(nameof(input));

        if (string.IsNullOrWhiteSpace(input.ActorId))
        {
            return DragonKnightAuthorityDecision.Reject("dragon-knight.boss.actor-id-missing");
        }

        if (!HasLocationPrefix(input.ActorId, ActorIdPrefix))
        {
            return DragonKnightAuthorityDecision.Reject("dragon-knight.boss.actor-id-malformed");
        }

        if (input.ActorIdStale)
        {
            return DragonKnightAuthorityDecision.Reject("dragon-knight.boss.actor-id-stale");
        }

        if (input.ActorIdDuplicate)
        {
            return DragonKnightAuthorityDecision.Reject("dragon-knight.boss.actor-id-duplicate");
        }

        if (!StringComparer.Ordinal.Equals(input.RoleId, BossActorRoleIdValue))
        {
            return DragonKnightAuthorityDecision.Reject("dragon-knight.boss.actor-role-mismatch");
        }

        return DragonKnightAuthorityDecision.Accept();
    }

    public static DragonKnightAuthorityDecision ValidateDecision(DragonKnightBossDecisionInput input)
    {
        input = input ?? throw new ArgumentNullException(nameof(input));

        if (!input.PackageEnabled)
        {
            return DragonKnightAuthorityDecision.Reject("dragon-knight.boss.default-off-disabled");
        }

        if (input.KillSwitchActive)
        {
            return DragonKnightAuthorityDecision.Reject("dragon-knight.boss.kill-switch-active");
        }

        var actorDecision = ValidateActor(input);
        if (!actorDecision.Accepted)
        {
            return actorDecision;
        }

        if (string.IsNullOrWhiteSpace(input.LeaseId))
        {
            return DragonKnightAuthorityDecision.Reject("dragon-knight.boss.lease-missing");
        }

        if (input.LeaseExpired)
        {
            return DragonKnightAuthorityDecision.Reject("dragon-knight.boss.lease-expired");
        }

        if (!StringComparer.Ordinal.Equals(input.OwnerId, RequiredOwnerId))
        {
            return DragonKnightAuthorityDecision.Reject("dragon-knight.boss.owner-mismatch");
        }

        return ValidateTarget(input);
    }

    public static DragonKnightAuthorityDecision ValidateTarget(DragonKnightBossDecisionInput input)
    {
        input = input ?? throw new ArgumentNullException(nameof(input));

        if (string.IsNullOrWhiteSpace(input.TargetKind))
        {
            return DragonKnightAuthorityDecision.Reject("dragon-knight.boss.target-kind-missing");
        }

        if (IsForbiddenTargetKind(input.TargetKind) || IsForbiddenTrigger(input.TargetId))
        {
            return DragonKnightAuthorityDecision.Reject("dragon-knight.boss.target-forbidden-trigger");
        }

        if (StringComparer.Ordinal.Equals(input.TargetKind, CompanionTargetKind)
            && !input.CompanionTargetsAllowed)
        {
            return DragonKnightAuthorityDecision.Reject("dragon-knight.boss.target-companion-blocked");
        }

        if (StringComparer.Ordinal.Equals(input.TargetKind, LocationTargetKind))
        {
            if (HasLocationPrefix(input.TargetId, TargetIdPrefix))
            {
                return DragonKnightAuthorityDecision.Accept();
            }

            return DragonKnightAuthorityDecision.Reject("dragon-knight.boss.target-id-malformed");
        }

        if (StringComparer.Ordinal.Equals(input.TargetKind, PlayerTargetKind))
        {
            return string.IsNullOrWhiteSpace(input.TargetId)
                ? DragonKnightAuthorityDecision.Reject("dragon-knight.boss.target-id-missing")
                : DragonKnightAuthorityDecision.Accept();
        }

        return DragonKnightAuthorityDecision.Reject("dragon-knight.boss.target-kind-invalid");
    }

    public static DragonKnightAuthorityDecision ValidateCombatRequest(DragonKnightBossDecisionInput input)
    {
        var decision = ValidateDecision(input);
        if (!decision.Accepted)
        {
            return decision;
        }

        return input.CombatGateApproved
            ? DragonKnightAuthorityDecision.Accept()
            : DragonKnightAuthorityDecision.Reject("dragon-knight.boss.combat-gate-missing");
    }

    public static DragonKnightAuthorityDecision ValidateRabbitSessionWrite(bool approved)
    {
        return approved
            ? DragonKnightAuthorityDecision.Accept()
            : DragonKnightAuthorityDecision.Reject("dragon-knight.boss.rabbit-write-unapproved");
    }

    private static bool HasLocationPrefix(string value, string prefix)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.StartsWith(prefix, StringComparison.Ordinal)
            && value.Length > prefix.Length;
    }

    private static bool IsForbiddenTargetKind(string targetKind)
    {
        return StringComparer.Ordinal.Equals(targetKind, ShrineTargetKind)
            || StringComparer.Ordinal.Equals(targetKind, AltarTargetKind)
            || StringComparer.Ordinal.Equals(targetKind, DiscoveryTargetKind)
            || StringComparer.Ordinal.Equals(targetKind, PromptVisibilityTargetKind);
    }

    private static bool IsForbiddenTrigger(string targetId)
    {
        return StringComparer.Ordinal.Equals(targetId, ForbiddenAltarTrigger)
            || StringComparer.Ordinal.Equals(targetId, ForbiddenDiscoveryTrigger);
    }

    private static BlackboardKeyDeclaration Key<T>(
        string name,
        BlackboardScope scope = BlackboardScope.Actor)
    {
        return new BlackboardKeyDeclaration(
            BlackboardNamespace,
            name,
            BlackboardSchemaVersion,
            BlackboardAccess.PackageLocal,
            scope,
            typeof(T));
    }

    private static AvalonGoalDefinition Goal(string goalId, string desiredFact)
    {
        return new AvalonGoalDefinition(
            new GoalId(goalId),
            new[] { Condition(desiredFact, 1) });
    }

    private static AvalonActionDefinition Action(
        string actionId,
        string capability,
        float cost,
        string targetKey,
        IEnumerable<PlanningCondition> conditions,
        IEnumerable<PlanningEffect> effects,
        bool useProcedure)
    {
        var actionCapability = new ActionCapability(capability);
        var profile = useProcedure
            ? AvalonActionExecutionProfile.ForProcedure(new AvalonProcedureReference(
                UseInteractableProcedureId,
                UseInteractableProcedureVersion,
                new[] { actionCapability },
                TimeSpan.FromSeconds(10),
                mutatesActor: true))
            : AvalonActionExecutionProfile.Direct;

        return new AvalonActionDefinition(
            new ActionId(actionId),
            conditions,
            effects,
            new PlanningTargetKeyId(targetKey),
            new ConstantActionCostProvider(cost),
            actionCapability,
            InterruptPolicy.Always,
            TimeSpan.FromSeconds(10),
            profile);
    }

    private static PlanningCondition[] ValidActorTargetLeaseConditions()
    {
        return new[]
        {
            Condition(FactActorValidValue, 1),
            Condition(FactTargetValidValue, 1),
            Condition(FactLeaseCurrentValue, 1),
        };
    }

    private static PlanningCondition Condition(string fact, int value)
    {
        return new PlanningCondition(new PlanningFactId(fact), PlanningComparison.Equal, value);
    }

    private static PlanningEffect Effect(string fact, int value)
    {
        return new PlanningEffect(new PlanningFactId(fact), value);
    }

    private static DragonKnightPlanningActionContract PlanningAction(
        string name,
        string actionId,
        string capability,
        float cost,
        TimeSpan cooldown,
        string preconditions,
        string effects,
        string failureBehavior)
    {
        return new DragonKnightPlanningActionContract(
            name,
            new ActionId(actionId),
            new ActionCapability(capability),
            cost,
            cooldown,
            preconditions,
            effects,
            failureBehavior,
            directNativeCalls: false);
    }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        return value;
    }
}

public sealed class DragonKnightPlanningActionContract
{
    public DragonKnightPlanningActionContract(
        string name,
        ActionId actionId,
        ActionCapability capability,
        float cost,
        TimeSpan cooldown,
        string preconditions,
        string effects,
        string failureBehavior,
        bool directNativeCalls)
    {
        Name = RequireText(name, nameof(name));
        ActionId = actionId;
        Capability = capability;
        Cost = cost;
        Cooldown = cooldown;
        Preconditions = RequireText(preconditions, nameof(preconditions));
        Effects = RequireText(effects, nameof(effects));
        FailureBehavior = RequireText(failureBehavior, nameof(failureBehavior));
        DirectNativeCalls = directNativeCalls;
    }

    public string Name { get; }

    public ActionId ActionId { get; }

    public ActionCapability Capability { get; }

    public float Cost { get; }

    public TimeSpan Cooldown { get; }

    public string Preconditions { get; }

    public string Effects { get; }

    public string FailureBehavior { get; }

    public bool DirectNativeCalls { get; }

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A non-empty value is required.", parameterName);
        }

        return value;
    }
}

public sealed class DragonKnightBossDecisionInput
{
    public bool PackageEnabled { get; set; }

    public bool KillSwitchActive { get; set; }

    public string ActorId { get; set; } = string.Empty;

    public string RoleId { get; set; } = string.Empty;

    public bool ActorIdStale { get; set; }

    public bool ActorIdDuplicate { get; set; }

    public string OwnerId { get; set; } = string.Empty;

    public string LeaseId { get; set; } = string.Empty;

    public bool LeaseExpired { get; set; }

    public string TargetKind { get; set; } = string.Empty;

    public string TargetId { get; set; } = string.Empty;

    public bool CompanionTargetsAllowed { get; set; }

    public bool CombatGateApproved { get; set; }
}

public sealed class DragonKnightAuthorityDecision
{
    private DragonKnightAuthorityDecision(bool accepted, string reason)
    {
        Accepted = accepted;
        Reason = reason;
    }

    public bool Accepted { get; }

    public string Reason { get; }

    public static DragonKnightAuthorityDecision Accept()
    {
        return new DragonKnightAuthorityDecision(true, "accepted");
    }

    public static DragonKnightAuthorityDecision Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A non-empty reason is required.", nameof(reason));
        }

        return new DragonKnightAuthorityDecision(false, reason);
    }
}
