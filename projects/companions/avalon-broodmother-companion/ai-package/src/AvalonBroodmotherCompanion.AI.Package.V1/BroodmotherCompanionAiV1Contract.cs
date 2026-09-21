using System;
using System.Collections.Generic;
#if AVALON_AI_CONTRACTS
using AvalonAI.Contracts.V2;
#endif

namespace AvalonBroodmotherCompanion.AI.Package.V1;

public static class BroodmotherCompanionAiV1Contract
{
    public const string PackageIdValue = "kane.tgfoa.avalon-broodmother-companion.ai-spider-companion.v1";
    public const string AssemblyNameValue = "AvalonBroodmotherCompanion.AI.Package.V1";
    public const string DisplayName = "Avalon Broodmother Companion Spider AI";
    public const string PackageVersion = "0.1.0-broodmother-spider-ai";
    public const string PackageMarkerString = "BROODMOTHER_COMPANION_AI_PACKAGE_V1_PASS";

    public const string RequiredOwnerId = "kane.tgfoa.avalon-broodmother-companion";
    public const string BroodmotherActorRoleIdValue = "avalon-broodmother-companion.broodmother";
    public const string SpiderActorRoleIdValue = "avalon-broodmother-companion.spider";
    public const string TargetTemplateName = "Spec_Broodmother_CI4";
    public const string TargetTemplateGuid = "527d142b35e81a648b6e84ea921ac543";
    public const string TargetNpcTemplateGuid = "7623d4729979e814ea64992d9a399e17";
    public const string SpiderTargetTemplateName = "Spec_Spider_CI4";
    public const string SpiderTargetTemplateGuid = "bfcd6db0f66afc14c94fcb6a9dbde4e7";
    public const string SpiderTargetNpcTemplateGuid = "5f36e2717dc68e04091fe7b0aa8bafe0";
    public const string TargetSource = "native-combat-candidates.prioritized-live-only";

    public const string BlackboardNamespace = "avalon_broodmother_companion.spider_ai.v1";
    public const int BlackboardSchemaVersion = 1;
    public const string RabbitPersistence = "session-only";

    public const string AdvancedAiConfigKey = "CompanionAI.EnableAdvancedSpiderAI=true";
    public const string LeapRequestsConfigKey = "CompanionAI.EnableLeapAttackRequests=true";
    public const string CombatPromptSecondsConfigKey = "CompanionAI.CombatPromptSeconds=1.25";
    public const string RuntimeTickSecondsConfigKey = "Runtime.TickSeconds=0.75";
    public const float CombatPromptSecondsDefault = 1.25f;
    public const float RuntimeTickSecondsDefault = 0.75f;
    public const int MaxAttackSlotsDefault = 1;

    public const bool DirectNativeCalls = false;
    public const bool NativeControl = false;
    public const bool NativeJumpStateProven = false;
    public const bool RabbitBypass = false;
    public const bool RabbitWritesByDefault = false;
    public const bool GoapBypass = false;
    public const bool BlazeBypass = false;
    public const bool PlayMakerNativeCalls = false;
    public const bool SaveWrites = false;
    public const bool RandomSpawns = false;
    public const bool Persistence = false;
    public const bool PublicPackagingClaim = false;

    public const int ShortRangeStateId = 16;
    public const string ShortRangePrimaryClipName = "Spider_Attack1_ShortRange_CI4";
    public const string ShortRangeSecondaryClipName = "Spider_Attack2_ShortRange_CI4";
    public const string JumpClipName = "Spider_jump";
    public const string JumpV2ClipName = "Spider_jump_v2";
    public const string JumpMotionPolicy = "motion-only-until-native-jump-state-proven";

    public const float ShortBiteDistanceMeters = 2.25f;
    public const float ShortBiteAngleDegrees = 55f;
    public const float FlankerLeapDistanceMeters = 5.5f;
    public const float FlankerLeapAngleDegrees = 100f;
    public const float LeapDistanceMeters = 6.0f;
    public const float LeapAngleDegrees = 75f;
    public const float FeintDistanceMeters = 4.0f;
    public const float ReleaseWindowMaxDistanceMeters = 6.0f;
    public const float ReleaseWindowMaxAngleDegrees = 90f;

    public const string GoalPredatorPressureValue = "avalon-broodmother-companion.spider-ai.predator-pressure";
    public const string ActionObserveLiveAttackerValue = "avalon-broodmother-companion.spider-ai.observe-live-attacker";
    public const string ActionChooseRoleValue = "avalon-broodmother-companion.spider-ai.choose-role";
    public const string ActionSelectPositionValue = "avalon-broodmother-companion.spider-ai.select-position";
    public const string ActionCoordinateAttackSlotValue = "avalon-broodmother-companion.spider-ai.coordinate-attack-slot";
    public const string ActionEmitTelegraphValue = "avalon-broodmother-companion.spider-ai.emit-telegraph";
    public const string ActionNativeCombatHandoffValue = "avalon-broodmother-companion.spider-ai.native-combat-handoff";
    public const string ActionShortRange16HandoffValue = "avalon-broodmother-companion.spider-ai.short-range-16-handoff";
    public const string ActionFailClosedValue = "avalon-broodmother-companion.spider-ai.fail-closed";

    public const string CapabilityObserveLiveAttackerValue = "broodmother.observe-live-hero-attacker";
    public const string CapabilityAssignRoleValue = "broodmother.assign-spider-role";
    public const string CapabilitySelectPositionValue = "broodmother.select-spider-position";
    public const string CapabilityCoordinateAttackSlotValue = "broodmother.coordinate-one-attack-slot";
    public const string CapabilityEmitTelegraphValue = "broodmother.emit-spider-telegraph";
    public const string CapabilityNativeCombatHandoffValue = "broodmother.native-target-combat-handoff";
    public const string CapabilityShortRange16HandoffValue = "broodmother.short-range-16-handoff";
    public const string CapabilityStopAllValue = "broodmother.stop-all-request";

    private const string FactLiveAttackerObservedValue = "avalon-broodmother-companion.spider-ai.fact.live-attacker-observed";
    private const string FactRoleSelectedValue = "avalon-broodmother-companion.spider-ai.fact.role-selected";
    private const string FactPositionSelectedValue = "avalon-broodmother-companion.spider-ai.fact.position-selected";
    private const string FactAttackSlotEvaluatedValue = "avalon-broodmother-companion.spider-ai.fact.attack-slot-evaluated";
    private const string FactTelegraphEmittedValue = "avalon-broodmother-companion.spider-ai.fact.telegraph-emitted";
    private const string FactCombatHandoffDispatchedValue = "avalon-broodmother-companion.spider-ai.fact.combat-handoff-dispatched";
    private const string FactFailClosedValue = "avalon-broodmother-companion.spider-ai.fact.fail-closed";

#if AVALON_AI_CONTRACTS
    public static PackageId PackageId { get; } = new PackageId(PackageIdValue);

    public static ActorRoleId BroodmotherActorRoleId { get; } = new ActorRoleId(BroodmotherActorRoleIdValue);

    public static ActorRoleId SpiderActorRoleId { get; } = new ActorRoleId(SpiderActorRoleIdValue);

    public static GoalId GoalPredatorPressure { get; } = new GoalId(GoalPredatorPressureValue);

    public static ActionId ActionObserveLiveAttacker { get; } = new ActionId(ActionObserveLiveAttackerValue);

    public static ActionId ActionChooseRole { get; } = new ActionId(ActionChooseRoleValue);

    public static ActionId ActionSelectPosition { get; } = new ActionId(ActionSelectPositionValue);

    public static ActionId ActionCoordinateAttackSlot { get; } = new ActionId(ActionCoordinateAttackSlotValue);

    public static ActionId ActionEmitTelegraph { get; } = new ActionId(ActionEmitTelegraphValue);

    public static ActionId ActionNativeCombatHandoff { get; } = new ActionId(ActionNativeCombatHandoffValue);

    public static ActionId ActionShortRange16Handoff { get; } = new ActionId(ActionShortRange16HandoffValue);

    public static ActionId ActionFailClosed { get; } = new ActionId(ActionFailClosedValue);

    public static ActionCapability CapabilityObserveLiveAttacker { get; } = new ActionCapability(CapabilityObserveLiveAttackerValue);

    public static ActionCapability CapabilityAssignRole { get; } = new ActionCapability(CapabilityAssignRoleValue);

    public static ActionCapability CapabilitySelectPosition { get; } = new ActionCapability(CapabilitySelectPositionValue);

    public static ActionCapability CapabilityCoordinateAttackSlot { get; } = new ActionCapability(CapabilityCoordinateAttackSlotValue);

    public static ActionCapability CapabilityEmitTelegraph { get; } = new ActionCapability(CapabilityEmitTelegraphValue);

    public static ActionCapability CapabilityNativeCombatHandoff { get; } = new ActionCapability(CapabilityNativeCombatHandoffValue);

    public static ActionCapability CapabilityShortRange16Handoff { get; } = new ActionCapability(CapabilityShortRange16HandoffValue);

    public static ActionCapability CapabilityStopAll { get; } = new ActionCapability(CapabilityStopAllValue);

    public static PlanningFactId FactLiveAttackerObserved { get; } = new PlanningFactId(FactLiveAttackerObservedValue);

    public static PlanningFactId FactRoleSelected { get; } = new PlanningFactId(FactRoleSelectedValue);

    public static PlanningFactId FactPositionSelected { get; } = new PlanningFactId(FactPositionSelectedValue);

    public static PlanningFactId FactAttackSlotEvaluated { get; } = new PlanningFactId(FactAttackSlotEvaluatedValue);

    public static PlanningFactId FactTelegraphEmitted { get; } = new PlanningFactId(FactTelegraphEmittedValue);

    public static PlanningFactId FactCombatHandoffDispatched { get; } = new PlanningFactId(FactCombatHandoffDispatchedValue);

    public static PlanningFactId FactFailClosed { get; } = new PlanningFactId(FactFailClosedValue);

    public static BlackboardKey<bool> LiveHeroAttackerCandidate { get; } = new BlackboardKey<bool>(
        "avalon.foa.broodmother-companion",
        "live-hero-attacker-candidate",
        1,
        BlackboardAccess.Authoritative,
        BlackboardScope.Actor);

    public static IReadOnlyList<GoalId> GoalIds { get; } = Array.AsReadOnly(new[]
    {
        GoalPredatorPressure,
    });

    public static IReadOnlyList<ActionId> ActionIds { get; } = Array.AsReadOnly(new[]
    {
        ActionObserveLiveAttacker,
        ActionChooseRole,
        ActionSelectPosition,
        ActionCoordinateAttackSlot,
        ActionEmitTelegraph,
        ActionNativeCombatHandoff,
        ActionShortRange16Handoff,
        ActionFailClosed,
    });

    public static IReadOnlyList<ActionCapability> RequiredCapabilities { get; } = Array.AsReadOnly(new[]
    {
        CapabilityObserveLiveAttacker,
        CapabilityAssignRole,
        CapabilitySelectPosition,
        CapabilityCoordinateAttackSlot,
        CapabilityEmitTelegraph,
        CapabilityNativeCombatHandoff,
        CapabilityShortRange16Handoff,
        CapabilityStopAll,
    });

    public static IReadOnlyList<AvalonGoalDefinition> GoalDefinitions { get; } = Array.AsReadOnly(new[]
    {
        new AvalonGoalDefinition(
            GoalPredatorPressure,
            new[]
            {
                new PlanningCondition(FactCombatHandoffDispatched, PlanningComparison.Equal, 1),
            }),
    });

    public static IReadOnlyList<AvalonActionDefinition> ActionDefinitions { get; } = Array.AsReadOnly(new[]
    {
        Action(ActionObserveLiveAttacker, CapabilityObserveLiveAttacker, Array.Empty<PlanningCondition>(), FactLiveAttackerObserved, 1),
        Action(ActionChooseRole, CapabilityAssignRole, Conditions(FactLiveAttackerObserved), FactRoleSelected, 1),
        Action(ActionSelectPosition, CapabilitySelectPosition, Conditions(FactRoleSelected), FactPositionSelected, 2),
        Action(ActionCoordinateAttackSlot, CapabilityCoordinateAttackSlot, Conditions(FactRoleSelected), FactAttackSlotEvaluated, 1),
        Action(ActionEmitTelegraph, CapabilityEmitTelegraph, Conditions(FactAttackSlotEvaluated), FactTelegraphEmitted, 1),
        Action(ActionNativeCombatHandoff, CapabilityNativeCombatHandoff, Conditions(FactAttackSlotEvaluated), FactCombatHandoffDispatched, 3),
        Action(ActionShortRange16Handoff, CapabilityShortRange16Handoff, Conditions(FactTelegraphEmitted), FactCombatHandoffDispatched, 3),
        Action(ActionFailClosed, CapabilityStopAll, Array.Empty<PlanningCondition>(), FactFailClosed, 1),
    });
#endif

    public static string PackageMarkerLine =>
        PackageMarkerString
        + " package=" + PackageIdValue
        + " owner=" + RequiredOwnerId
        + " roles=" + BroodmotherActorRoleIdValue + "|" + SpiderActorRoleIdValue
        + " target-source=" + TargetSource
        + " templates=" + TargetTemplateName + "|" + SpiderTargetTemplateName
        + " short-range=16"
        + " max-attack-slots=1"
        + " direct-native=0 native-jump=0 rabbit-bypass=0 goap-bypass=0 blaze-bypass=0 playmaker-native=0 save=0 random-spawn=0 persistence=0";

    public static BroodmotherSpiderCompanionAiEvaluation Evaluate(BroodmotherSpiderCompanionAiInput input)
    {
        input = input ?? throw new ArgumentNullException(nameof(input));

        BroodmotherSpiderAiDecision authority = ValidateAuthority(input);
        if (authority.Kind == BroodmotherSpiderAiDecisionKind.FailClosed ||
            authority.Kind == BroodmotherSpiderAiDecisionKind.Hold)
        {
            return BroodmotherSpiderCompanionAiEvaluation.Stopped(authority);
        }

        BroodmotherSpiderRole role = ChooseRole(input);
        BroodmotherSpiderPositionSelection position = SelectPosition(input, role);
        BroodmotherSpiderAttackSlotDecision slot = CoordinateAttackSlot(input);
        BroodmotherSpiderAttackSelection attack = ChooseAttack(input, role, slot);
        BroodmotherSpiderAiDecision decision = CommitAttackWindow(input, attack);

        return new BroodmotherSpiderCompanionAiEvaluation(
            role,
            position,
            slot,
            attack,
            decision,
            ResolveShortRangeClip(attack.Kind, input.PreferAlternateShortRange),
            ResolveJumpMotionClip(input.PreferAlternateJumpMotion),
            JumpMotionPolicy);
    }

    public static BroodmotherSpiderAiDecision ValidateAuthority(BroodmotherSpiderCompanionAiInput input)
    {
        input = input ?? throw new ArgumentNullException(nameof(input));

        if (!input.AuthorityValid)
        {
            return BroodmotherSpiderAiDecision.FailClosed("broodmother.authority-invalid");
        }

        if (!StringComparer.Ordinal.Equals(input.OwnerId, RequiredOwnerId))
        {
            return BroodmotherSpiderAiDecision.FailClosed("broodmother.owner-mismatch");
        }

        if (!IsSupportedActorRole(input.ActorRoleId))
        {
            return BroodmotherSpiderAiDecision.FailClosed("spider-family.actor-role-mismatch");
        }

        if (!IsSupportedActorIdentity(
                input.ActorRoleId,
                input.LocationTemplateName,
                input.LocationTemplateGuid,
                input.NpcTemplateGuid))
        {
            return BroodmotherSpiderAiDecision.FailClosed("spider-family.identity-mismatch");
        }

        if (!input.TargetAlive || input.LiveHeroAttackerCount <= 0)
        {
            return BroodmotherSpiderAiDecision.Hold("broodmother.no-live-hero-attacker");
        }

        return BroodmotherSpiderAiDecision.None("broodmother.authority-valid");
    }

    public static bool IsSupportedActorRole(string actorRoleId)
    {
        return StringComparer.Ordinal.Equals(actorRoleId, BroodmotherActorRoleIdValue) ||
               StringComparer.Ordinal.Equals(actorRoleId, SpiderActorRoleIdValue);
    }

    public static bool IsSupportedActorIdentity(
        string actorRoleId,
        string locationTemplateName,
        string locationTemplateGuid,
        string npcTemplateGuid)
    {
        if (StringComparer.Ordinal.Equals(actorRoleId, BroodmotherActorRoleIdValue))
        {
            return StringComparer.Ordinal.Equals(locationTemplateName, TargetTemplateName) &&
                   StringComparer.OrdinalIgnoreCase.Equals(locationTemplateGuid, TargetTemplateGuid) &&
                   StringComparer.OrdinalIgnoreCase.Equals(npcTemplateGuid, TargetNpcTemplateGuid);
        }

        if (StringComparer.Ordinal.Equals(actorRoleId, SpiderActorRoleIdValue))
        {
            return StringComparer.Ordinal.Equals(locationTemplateName, SpiderTargetTemplateName) &&
                   StringComparer.OrdinalIgnoreCase.Equals(locationTemplateGuid, SpiderTargetTemplateGuid) &&
                   StringComparer.OrdinalIgnoreCase.Equals(npcTemplateGuid, SpiderTargetNpcTemplateGuid);
        }

        return false;
    }

    public static BroodmotherSpiderRole ChooseRole(BroodmotherSpiderCompanionAiInput input)
    {
        input = input ?? throw new ArgumentNullException(nameof(input));

        if (input.HealthFraction <= 0.20f || input.RecentHits >= 3)
        {
            return BroodmotherSpiderRole.Retreating;
        }

        if (input.TargetVisible && input.TargetHealthFraction > 0f && input.TargetHealthFraction <= 0.20f)
        {
            return BroodmotherSpiderRole.Finisher;
        }

        int slot = PositiveModulo(input.PackIndex, 4);
        if (slot == 1)
        {
            return BroodmotherSpiderRole.Flanker;
        }

        return slot == 2 ? BroodmotherSpiderRole.Ambusher : BroodmotherSpiderRole.Harasser;
    }

    public static BroodmotherSpiderPositionSelection SelectPosition(
        BroodmotherSpiderCompanionAiInput input,
        BroodmotherSpiderRole role)
    {
        input = input ?? throw new ArgumentNullException(nameof(input));

        int sideSign = PositiveModulo(input.PackIndex, 2) == 0 ? -1 : 1;
        float spacingOffset = (PositiveModulo(input.PackIndex, 5) - 2) * 12f;
        float angle;
        float distance;

        switch (role)
        {
            case BroodmotherSpiderRole.Flanker:
                angle = sideSign * 75f + spacingOffset;
                distance = 3.25f;
                break;
            case BroodmotherSpiderRole.Ambusher:
                angle = sideSign * 145f + spacingOffset;
                distance = 4.5f;
                break;
            case BroodmotherSpiderRole.Retreating:
                angle = 180f + spacingOffset;
                distance = 7.5f;
                break;
            case BroodmotherSpiderRole.Finisher:
                angle = sideSign * 25f + spacingOffset;
                distance = 2.0f;
                break;
            default:
                angle = sideSign * 45f + spacingOffset;
                distance = 3.0f;
                break;
        }

        if (input.CameraDataAvailable && input.CameraCenterWeight >= 0.65f)
        {
            angle += sideSign * 35f;
        }

        return new BroodmotherSpiderPositionSelection(
            NormalizeAngle(angle),
            distance,
            avoidsStacking: true,
            avoidsCameraCenter: input.CameraDataAvailable && input.CameraCenterWeight >= 0.65f,
            "broodmother.position-selected");
    }

    public static BroodmotherSpiderAttackSlotDecision CoordinateAttackSlot(BroodmotherSpiderCompanionAiInput input)
    {
        input = input ?? throw new ArgumentNullException(nameof(input));

        int maxSlots = input.MaxSimultaneousAttackers <= 0
            ? MaxAttackSlotsDefault
            : input.MaxSimultaneousAttackers;
        bool canAttack = input.ActiveAttackers < maxSlots &&
            !input.AttackCooldownActive &&
            !input.RecoveryActive &&
            !input.AttackAlreadyCommitted;
        int wave = PositiveModulo(input.PackIndex + input.WaveSeed, Math.Max(1, maxSlots));

        return new BroodmotherSpiderAttackSlotDecision(
            canAttack,
            maxSlots,
            wave,
            canAttack ? "broodmother.attack-slot-granted" : "broodmother.attack-slot-held");
    }

    public static BroodmotherSpiderAttackSelection ChooseAttack(
        BroodmotherSpiderCompanionAiInput input,
        BroodmotherSpiderRole role,
        BroodmotherSpiderAttackSlotDecision slot)
    {
        input = input ?? throw new ArgumentNullException(nameof(input));
        slot = slot ?? throw new ArgumentNullException(nameof(slot));

        if (!slot.CanAttack)
        {
            return input.LocalDensity >= 0.70f
                ? BroodmotherSpiderAttackSelection.Chosen(BroodmotherSpiderAttackKind.Reposition, "broodmother.slot-denied-reposition")
                : BroodmotherSpiderAttackSelection.Chosen(BroodmotherSpiderAttackKind.Hold, "broodmother.slot-denied-hold");
        }

        if (!input.TargetSpatialAvailable)
        {
            return BroodmotherSpiderAttackSelection.Chosen(
                BroodmotherSpiderAttackKind.TargetedNativeCombat,
                "broodmother.target-spatial-unavailable");
        }

        if (input.RecoveryActive || input.AttackCooldownActive)
        {
            return BroodmotherSpiderAttackSelection.Chosen(BroodmotherSpiderAttackKind.Hold, "broodmother.cooldown-or-recovery");
        }

        if (input.LocalDensity >= 0.80f)
        {
            return BroodmotherSpiderAttackSelection.Chosen(BroodmotherSpiderAttackKind.Reposition, "broodmother.spacing-too-dense");
        }

        if (input.LeapRequestsEnabled &&
            role == BroodmotherSpiderRole.Flanker &&
            input.TargetDistanceMeters <= FlankerLeapDistanceMeters &&
            Absolute(input.TargetAngleDegrees) <= FlankerLeapAngleDegrees)
        {
            return BroodmotherSpiderAttackSelection.Chosen(BroodmotherSpiderAttackKind.LeapJump, "broodmother.flanker-leap");
        }

        if (input.TargetDistanceMeters <= ShortBiteDistanceMeters &&
            Absolute(input.TargetAngleDegrees) <= ShortBiteAngleDegrees)
        {
            return BroodmotherSpiderAttackSelection.Chosen(BroodmotherSpiderAttackKind.ShortBite, "broodmother.short-bite");
        }

        if (input.LeapRequestsEnabled &&
            input.TargetDistanceMeters <= LeapDistanceMeters &&
            Absolute(input.TargetAngleDegrees) <= LeapAngleDegrees)
        {
            return BroodmotherSpiderAttackSelection.Chosen(BroodmotherSpiderAttackKind.LeapJump, "broodmother.leap");
        }

        if (input.TargetDistanceMeters <= FeintDistanceMeters && !input.TelegraphAlreadyEmitted)
        {
            return BroodmotherSpiderAttackSelection.Chosen(BroodmotherSpiderAttackKind.Feint, "broodmother.feint");
        }

        return BroodmotherSpiderAttackSelection.Chosen(BroodmotherSpiderAttackKind.Reposition, "broodmother.reposition-for-angle");
    }

    public static BroodmotherSpiderAiDecision CommitAttackWindow(
        BroodmotherSpiderCompanionAiInput input,
        BroodmotherSpiderAttackSelection attack)
    {
        input = input ?? throw new ArgumentNullException(nameof(input));
        attack = attack ?? throw new ArgumentNullException(nameof(attack));

        if (attack.Kind == BroodmotherSpiderAttackKind.TargetedNativeCombat)
        {
            return BroodmotherSpiderAiDecision.NativeCombatHandoff("broodmother.spatial-fallback-native-targeted-combat");
        }

        if (attack.Kind == BroodmotherSpiderAttackKind.Reposition)
        {
            return BroodmotherSpiderAiDecision.NativeCombatHandoff("broodmother.reposition-native-targeted-combat");
        }

        if (attack.Kind != BroodmotherSpiderAttackKind.ShortBite &&
            attack.Kind != BroodmotherSpiderAttackKind.LeapJump)
        {
            return BroodmotherSpiderAiDecision.Hold("broodmother.attack-not-committable");
        }

        if (input.RecoveryActive || input.AttackCooldownActive)
        {
            return BroodmotherSpiderAiDecision.Hold("broodmother.release-blocked-by-recovery-or-cooldown");
        }

        if (!input.AttackAlreadyCommitted && !input.TelegraphAlreadyEmitted)
        {
            return BroodmotherSpiderAiDecision.Telegraph("broodmother.telegraph-before-commit");
        }

        if (!input.ReleaseWindowOpen)
        {
            return BroodmotherSpiderAiDecision.Hold("broodmother.waiting-for-release-window");
        }

        if (input.TargetDistanceMeters > ReleaseWindowMaxDistanceMeters ||
            Absolute(input.TargetAngleDegrees) > ReleaseWindowMaxAngleDegrees)
        {
            return BroodmotherSpiderAiDecision.Reposition("broodmother.release-window-target-out-of-envelope");
        }

        return BroodmotherSpiderAiDecision.ShortRange16("broodmother.short-range-16-request-valid");
    }

    public static string ResolveShortRangeClip(BroodmotherSpiderAttackKind attackKind, bool preferAlternate)
    {
        return attackKind == BroodmotherSpiderAttackKind.LeapJump || preferAlternate
            ? ShortRangeSecondaryClipName
            : ShortRangePrimaryClipName;
    }

    public static string ResolveJumpMotionClip(bool preferAlternate)
    {
        return preferAlternate ? JumpV2ClipName : JumpClipName;
    }

#if AVALON_AI_CONTRACTS
    private static AvalonActionDefinition Action(
        ActionId id,
        ActionCapability capability,
        IEnumerable<PlanningCondition> conditions,
        PlanningFactId effect,
        float cost)
    {
        return new AvalonActionDefinition(
            id,
            conditions,
            new[] { new PlanningEffect(effect, 1) },
            default,
            new ConstantActionCostProvider(cost),
            capability,
            InterruptPolicy.OnGoalChange,
            TimeSpan.FromSeconds(1));
    }

    private static PlanningCondition[] Conditions(PlanningFactId factId)
    {
        return new[] { new PlanningCondition(factId, PlanningComparison.Equal, 1) };
    }
#endif

    private static int PositiveModulo(int value, int divisor)
    {
        if (divisor <= 0)
        {
            return 0;
        }

        int result = value % divisor;
        return result < 0 ? result + divisor : result;
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f)
        {
            angle -= 360f;
        }

        while (angle < -180f)
        {
            angle += 360f;
        }

        return angle;
    }

    private static float Absolute(float value)
    {
        return value < 0f ? -value : value;
    }
}
