using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AvalonAI.Contracts;
using AvalonAI.Runtime;
using AvalonCompanions.AI.Package;

Console.WriteLine(AvalonCompanionAdvancedAiPackage.PackageMarkerString);

var failures = new List<string>();

void Check(bool condition, string name)
{
    Console.WriteLine((condition ? "PASS " : "FAIL ") + name);
    if (!condition)
    {
        failures.Add(name);
    }
}

CompanionIntentObservation Observation(
    bool hasActiveCompanion = true,
    CompanionAiMode mode = CompanionAiMode.Follow,
    bool hasNpcElement = true,
    bool? npcAlive = true,
    bool? npcWorking = true,
    bool? npcInCombat = false,
    bool? canMove = true,
    int? possibleTargetCount = 0,
    int? possibleAttackerCount = 0,
    int heroLiveAttackers = 0,
    string? movementState = "",
    bool followCatchUpCandidate = false,
    bool nativeDefendAssistAllowed = false,
    bool nativeDefendPromptReady = true)
{
    return new CompanionIntentObservation(
        hasActiveCompanion,
        mode,
        hasNpcElement,
        npcAlive,
        npcWorking,
        npcInCombat,
        canMove,
        possibleTargetCount,
        possibleAttackerCount,
        heroLiveAttackers,
        movementState,
        followCatchUpCandidate,
        nativeDefendAssistAllowed,
        nativeDefendPromptReady);
}

AdvancedCompanionObservation AdvancedObservation(
    string actorId = "foa.location:managed-wolf-1",
    string templateGuid = "fixture-wolf-guid",
    string templateName = "Spec_Animal_Wolf",
    string scene = "CampaignMap_HOS",
    double actorX = 0d,
    double actorY = 0d,
    double actorZ = 0d,
    double playerX = 0d,
    double playerY = 0d,
    double playerZ = 8d,
    double distanceToPlayerMeters = 8d,
    double healthFraction = 1d,
    bool alive = true,
    bool unconscious = false,
    AdvancedCompanionCommandMode commandMode = AdvancedCompanionCommandMode.Follow,
    AdvancedCompanionFollowRange followRange = AdvancedCompanionFollowRange.Pace,
    int trust = 60,
    int loyalty = 60,
    AdvancedCompanionBondLevel bondLevel = AdvancedCompanionBondLevel.Familiar,
    AdvancedCompanionNativeState nativeState = AdvancedCompanionNativeState.Idle,
    string currentTargetId = "",
    bool currentTargetThreatensPlayer = false,
    int visibleAttackersCount = 0,
    bool movementBlocked = false,
    bool movementStuck = false,
    AdvancedCompanionActorLane actorLane = AdvancedCompanionActorLane.ManagedAnimalCompanion,
    bool isManaged = true,
    bool isTameCandidate = false,
    bool isHostile = false,
    bool isUndeadOrBlocked = false,
    bool isPassiveOnly = false,
    bool defendCooldownReady = true,
    bool catchUpCooldownReady = true,
    bool recoverCooldownReady = true,
    bool tamePromptCooldownReady = true,
    bool ownershipValid = true,
    bool killSwitchActive = false)
{
    return new AdvancedCompanionObservation(
        actorId,
        templateGuid,
        templateName,
        scene,
        new AdvancedCompanionPoint(actorX, actorY, actorZ),
        new AdvancedCompanionPoint(playerX, playerY, playerZ),
        distanceToPlayerMeters,
        healthFraction,
        alive,
        unconscious,
        commandMode,
        followRange,
        trust,
        loyalty,
        bondLevel,
        nativeState,
        currentTargetId,
        currentTargetThreatensPlayer,
        visibleAttackersCount,
        movementBlocked,
        movementStuck,
        actorLane,
        isManaged,
        isTameCandidate,
        isHostile,
        isUndeadOrBlocked,
        isPassiveOnly,
        defendCooldownReady,
        catchUpCooldownReady,
        recoverCooldownReady,
        tamePromptCooldownReady,
        ownershipValid,
        killSwitchActive);
}

var package = new AvalonCompanionsAiPackage();
Check(
    package.Manifest.PackageId == AvalonCompanionsAiPackage.PackageId,
    "manifest-has-stable-package-id");
Check(package.Manifest.PackageVersion == "0.2.1", "manifest-has-evaluation-package-version");
Check(
    package.Manifest.RequiredRuntimeApi == new AvalonAiApiVersion(1, 1),
    "manifest-requires-air-02-api");

var runtime = new AvalonAiRuntime(new ReadyHost());
Check(
    runtime.Register(package).Status == AvalonAiPackageRegistrationStatus.Registered,
    "package-registers-through-avalon-ai-runtime");
Check(runtime.Start().Started, "ready-runtime-starts-for-package-evaluation");

Check(
    CompanionIntentClassifier.Classify(Observation(hasActiveCompanion: false)) ==
        CompanionAiIntent.NoActiveCompanion,
    "no-active-companion-classifies-explicitly");
Check(
    CompanionIntentClassifier.Classify(Observation(hasNpcElement: false)) ==
        CompanionAiIntent.EvidenceInsufficient,
    "missing-npc-evidence-fails-closed");
Check(
    CompanionIntentClassifier.Classify(Observation(npcAlive: false)) ==
        CompanionAiIntent.EvidenceInsufficient,
    "dead-npc-evidence-fails-closed");
Check(
    CompanionIntentClassifier.Classify(Observation(npcWorking: null)) ==
        CompanionAiIntent.EvidenceInsufficient,
    "unknown-working-state-fails-closed");
Check(
    CompanionIntentClassifier.Classify(
        Observation(
            mode: CompanionAiMode.Defend,
            npcInCombat: true,
            canMove: false,
            followCatchUpCandidate: true)) == CompanionAiIntent.NativeCombatObserved,
    "native-combat-evidence-has-first-precedence");
Check(
    CompanionIntentClassifier.Classify(
        Observation(mode: CompanionAiMode.Defend, canMove: false)) ==
        CompanionAiIntent.MovementBlocked,
    "native-movement-block-precedes-mode");
Check(
    CompanionIntentClassifier.Classify(Observation(mode: CompanionAiMode.Defend)) ==
        CompanionAiIntent.DefendWaitingForThreat,
    "defend-without-threat-waits");
Check(
    CompanionIntentClassifier.Classify(Observation(followCatchUpCandidate: true)) ==
        CompanionAiIntent.FollowCatchUpCandidate,
    "follow-catch-up-candidate-classifies");
Check(
    CompanionIntentClassifier.Classify(Observation(mode: CompanionAiMode.Stay)) ==
        CompanionAiIntent.StayPosition,
    "stay-position-classifies");
Check(
    CompanionIntentClassifier.Classify(Observation(movementState: "Patrol")) ==
        CompanionAiIntent.IdleNativePatrol,
    "ordinal-patrol-state-classifies");
Check(
    CompanionIntentClassifier.Classify(Observation()) == CompanionAiIntent.NoTargets,
    "zero-native-targets-classify");
Check(
    CompanionIntentClassifier.Classify(
        Observation(possibleTargetCount: null, possibleAttackerCount: null)) ==
        CompanionAiIntent.EvidenceInsufficient,
    "unmapped-native-state-fails-closed");
Check(
    CompanionIntentClassifier.Classify(Observation(mode: (CompanionAiMode)99)) ==
        CompanionAiIntent.EvidenceInsufficient,
    "undefined-mode-fails-closed");
Check(
    CompanionIntentClassifier.Classify(null) == CompanionAiIntent.EvidenceInsufficient,
    "missing-observation-fails-closed");

AvalonAiEvaluationResult catchUpResult = runtime.Evaluate(
    AvalonCompanionsAiPackage.PackageId,
    Observation(followCatchUpCandidate: true));
var catchUpProposal = catchUpResult.Proposal as CompanionCommandProposal;
Check(catchUpResult.Status == AvalonAiEvaluationStatus.ProposalProduced, "catch-up-intent-produces-proposal");
Check(catchUpProposal?.Kind == CompanionCommandProposalKind.CatchUpRecall, "catch-up-proposal-uses-existing-recall-kind");
Check(catchUpProposal?.SourceIntent == CompanionAiIntent.FollowCatchUpCandidate, "catch-up-proposal-preserves-source-intent");

AvalonAiEvaluationResult explicitDefendResult = runtime.Evaluate(
    AvalonCompanionsAiPackage.PackageId,
    Observation(mode: CompanionAiMode.Defend, heroLiveAttackers: 1));
var explicitDefendProposal = explicitDefendResult.Proposal as CompanionCommandProposal;
Check(explicitDefendResult.Status == AvalonAiEvaluationStatus.ProposalProduced, "defend-mode-live-attackers-produce-proposal");
Check(explicitDefendProposal?.Kind == CompanionCommandProposalKind.NativeDefendPrompt, "defend-proposal-uses-existing-native-prompt-kind");
Check(explicitDefendProposal?.SourceIntent == CompanionAiIntent.NativeCombatObserved, "defend-proposal-preserves-source-intent");

AvalonAiEvaluationResult allowedAssistResult = runtime.Evaluate(
    AvalonCompanionsAiPackage.PackageId,
    Observation(heroLiveAttackers: 1, nativeDefendAssistAllowed: true));
Check(allowedAssistResult.Status == AvalonAiEvaluationStatus.ProposalProduced, "allowed-native-assist-produces-defend-proposal");
Check(
    (allowedAssistResult.Proposal as CompanionCommandProposal)?.Kind ==
        CompanionCommandProposalKind.NativeDefendPrompt,
    "allowed-native-assist-uses-existing-native-prompt-kind");

AvalonAiEvaluationResult blockedAssistResult = runtime.Evaluate(
    AvalonCompanionsAiPackage.PackageId,
    Observation(heroLiveAttackers: 1));
Check(blockedAssistResult.Status == AvalonAiEvaluationStatus.NoProposal, "follow-mode-without-assist-gate-produces-no-proposal");

AvalonAiEvaluationResult cooldownBlockedResult = runtime.Evaluate(
    AvalonCompanionsAiPackage.PackageId,
    Observation(
        mode: CompanionAiMode.Defend,
        heroLiveAttackers: 1,
        nativeDefendPromptReady: false));
Check(
    cooldownBlockedResult.Status == AvalonAiEvaluationStatus.NoProposal,
    "defend-cooldown-not-ready-produces-no-proposal");

AvalonAiEvaluationResult noHeroAttackersResult = runtime.Evaluate(
    AvalonCompanionsAiPackage.PackageId,
    Observation(mode: CompanionAiMode.Defend, npcInCombat: true, heroLiveAttackers: 0));
Check(noHeroAttackersResult.Status == AvalonAiEvaluationStatus.NoProposal, "native-combat-without-live-hero-attackers-produces-no-proposal");

AvalonAiEvaluationResult idleResult = runtime.Evaluate(
    AvalonCompanionsAiPackage.PackageId,
    Observation());
Check(idleResult.Status == AvalonAiEvaluationStatus.NoProposal, "non-actionable-intent-produces-no-proposal");

AvalonAiEvaluationResult unrelatedResult = runtime.Evaluate(
    AvalonCompanionsAiPackage.PackageId,
    new UnrelatedObservation());
Check(unrelatedResult.Status == AvalonAiEvaluationStatus.NoProposal, "unsupported-observation-type-fails-closed");

var advancedPackage = new AvalonCompanionAdvancedAiPackage();
Check(
    advancedPackage.Manifest.PackageId == AvalonCompanionAdvancedAiPackage.PackageId,
    "advanced-manifest-has-requested-package-id");
Check(
    advancedPackage.Manifest.PackageId == "kane.tgfoa.avalon-companions.ai-advanced-companion",
    "advanced-manifest-package-id-is-exact");
Check(
    advancedPackage.Manifest.RequiredRuntimeApi == new AvalonAiApiVersion(1, 1),
    "advanced-manifest-requires-contracts-api-only");
Check(
    advancedPackage.Manifest.PackageVersion == AvalonCompanionAdvancedAiPackage.PackageVersion,
    "advanced-manifest-has-explicit-version");

var advancedRuntime = new AvalonAiRuntime(new ReadyHost());
Check(
    advancedRuntime.Register(advancedPackage).Status == AvalonAiPackageRegistrationStatus.Registered,
    "advanced-package-registers-through-avalon-runtime");
Check(advancedRuntime.Start().Started, "advanced-runtime-starts");

AssertAdvancedProposal(
    advancedRuntime,
    AdvancedObservation(
        commandMode: AdvancedCompanionCommandMode.Defend,
        trust: 95,
        loyalty: 95,
        bondLevel: AdvancedCompanionBondLevel.Loyal,
        currentTargetId: "foa.location:attacker-1",
        currentTargetThreatensPlayer: true,
        visibleAttackersCount: 1),
    AdvancedCompanionProposalKind.NativeDefendPrompt,
    AdvancedCompanionGoal.ProtectPlayer,
    "loyal-companion-defends-player");

AssertAdvancedNoProposal(
    advancedRuntime,
    AdvancedObservation(
        commandMode: AdvancedCompanionCommandMode.Defend,
        trust: 95,
        loyalty: 20,
        bondLevel: AdvancedCompanionBondLevel.Familiar,
        currentTargetId: "foa.location:attacker-1",
        currentTargetThreatensPlayer: true,
        visibleAttackersCount: 1),
    "advanced-companion.low-loyalty-refuses-risky-defend",
    "low-loyalty-companion-refuses-risky-defend");

AssertAdvancedProposal(
    advancedRuntime,
    AdvancedObservation(distanceToPlayerMeters: 40d),
    AdvancedCompanionProposalKind.CatchUpRecall,
    AdvancedCompanionGoal.RegroupWhenSeparated,
    "separated-companion-proposes-catch-up");

AssertAdvancedProposal(
    advancedRuntime,
    AdvancedObservation(movementStuck: true),
    AdvancedCompanionProposalKind.RecoverStuck,
    AdvancedCompanionGoal.RecoverWhenBlocked,
    "stuck-companion-proposes-recover");

AssertAdvancedProposal(
    advancedRuntime,
    AdvancedObservation(
        commandMode: AdvancedCompanionCommandMode.Hold,
        currentTargetId: "foa.location:attacker-1",
        currentTargetThreatensPlayer: true,
        visibleAttackersCount: 2),
    AdvancedCompanionProposalKind.HoldAnchor,
    AdvancedCompanionGoal.HoldAssignedPosition,
    "hold-mode-companion-does-not-chase");

AssertAdvancedProposal(
    advancedRuntime,
    AdvancedObservation(
        healthFraction: 0.2d,
        commandMode: AdvancedCompanionCommandMode.Defend,
        currentTargetId: "foa.location:attacker-1",
        currentTargetThreatensPlayer: true,
        visibleAttackersCount: 1),
    AdvancedCompanionProposalKind.RetreatOrStandDown,
    AdvancedCompanionGoal.AvoidDeath,
    "critical-health-companion-retreats-before-defend");

AssertAdvancedProposal(
    advancedRuntime,
    AdvancedObservation(
        actorId: "foa.location:wild-wolf-1",
        actorLane: AdvancedCompanionActorLane.WildAnimal,
        isManaged: false,
        isTameCandidate: true),
    AdvancedCompanionProposalKind.TameCandidateReady,
    AdvancedCompanionGoal.TameReadinessOnly,
    "wild-wolf-classified-tame-ready");

AssertAdvancedProposal(
    advancedRuntime,
    AdvancedObservation(
        actorId: "foa.location:wild-bear-hostile",
        actorLane: AdvancedCompanionActorLane.WildAnimal,
        isManaged: false,
        isTameCandidate: true,
        isHostile: true),
    AdvancedCompanionProposalKind.TameCandidateUnsafe,
    AdvancedCompanionGoal.TameReadinessOnly,
    "hostile-wild-actor-classified-unsafe");

AssertAdvancedNoProposal(
    advancedRuntime,
    AdvancedObservation(isUndeadOrBlocked: true),
    "advanced-companion.managed-lane-blocked-classification",
    "undead-managed-actor-blocked");

AssertAdvancedNoProposal(
    advancedRuntime,
    AdvancedObservation(
        actorLane: AdvancedCompanionActorLane.Unmanaged,
        isManaged: false),
    "advanced-companion.unmanaged-actor",
    "unmanaged-actor-produces-no-proposal");

AssertAdvancedNoProposal(
    advancedRuntime,
    AdvancedObservation(killSwitchActive: true),
    "advanced-companion.kill-switch-active",
    "kill-switch-produces-no-proposal");

AssertAdvancedNoProposal(
    advancedRuntime,
    AdvancedObservation(
        commandMode: AdvancedCompanionCommandMode.Defend,
        trust: 95,
        loyalty: 95,
        bondLevel: AdvancedCompanionBondLevel.Loyal,
        currentTargetId: "foa.location:attacker-1",
        currentTargetThreatensPlayer: true,
        visibleAttackersCount: 1,
        defendCooldownReady: false),
    "advanced-companion.defend-cooldown-blocked",
    "cooldown-blocked-produces-no-proposal");

AssertAdvancedNoProposal(
    advancedRuntime,
    AdvancedObservation(
        commandMode: AdvancedCompanionCommandMode.Defend,
        trust: 20,
        loyalty: 95,
        bondLevel: AdvancedCompanionBondLevel.Familiar,
        currentTargetId: "foa.location:attacker-1",
        currentTargetThreatensPlayer: true,
        visibleAttackersCount: 1),
    "advanced-companion.low-trust-refuses-advanced-command",
    "low-trust-refuses-advanced-defend");

AssertAdvancedProposal(
    advancedRuntime,
    AdvancedObservation(
        commandMode: AdvancedCompanionCommandMode.Follow,
        distanceToPlayerMeters: 40d,
        trust: 20,
        loyalty: 95,
        bondLevel: AdvancedCompanionBondLevel.Familiar),
    AdvancedCompanionProposalKind.CatchUpRecall,
    AdvancedCompanionGoal.StayWithPlayer,
    "low-trust-allows-basic-follow-catch-up");

Check(
    AdvancedCompanionDecisionPolicy.Evaluate(
        AdvancedObservation(
            followRange: AdvancedCompanionFollowRange.Pace,
            trust: 90,
            loyalty: 90,
            bondLevel: AdvancedCompanionBondLevel.Loyal)).EffectiveCatchUpDistanceMeters == 0d,
    "advanced-no-action-does-not-invent-live-distance-effect");
Check(
    AdvancedCompanionDecisionPolicy.Evaluate(
        AdvancedObservation(
            distanceToPlayerMeters: 35d,
            followRange: AdvancedCompanionFollowRange.Pace,
            trust: 90,
            loyalty: 90,
            bondLevel: AdvancedCompanionBondLevel.Loyal)).ProducesProposal,
    "high-bond-lowers-catch-up-threshold");

AssertAdvancedAssemblyBoundary();

Check(
    advancedRuntime.Stop().Status == AvalonAiStopStatus.Stopped,
    "advanced-runtime-stops");

var executionHost = new CompanionExecutionHost(
    Observation(followCatchUpCandidate: true));
var executionRuntime = new AvalonAiRuntime(executionHost);
Check(
    executionRuntime.Register(package).Status == AvalonAiPackageRegistrationStatus.Registered,
    "real-package-registers-for-host-execution");
AvalonAiStartResult executionStart = executionRuntime.Start();
Check(executionStart.Started, "real-package-execution-runtime-starts");
Check(
    executionStart.Activation?.Status == AvalonAiHostActivationStatus.Activated,
    "real-package-host-activation-succeeds");
Check(
    executionHost.ActivatedPackageId == AvalonCompanionsAiPackage.PackageId,
    "real-package-host-owns-exact-package-id");

AvalonAiExecutionResult catchUpExecution = executionRuntime.Execute(
    AvalonCompanionsAiPackage.PackageId);
Check(
    catchUpExecution.Status == AvalonAiExecutionStatus.ProposalDispatched,
    "real-package-catch-up-executes-through-host");
Check(
    executionHost.DispatchedProposals.Count == 1 &&
    executionHost.DispatchedProposals[0].Kind == CompanionCommandProposalKind.CatchUpRecall,
    "real-package-host-receives-catch-up-proposal");

executionHost.Observation = Observation(
    mode: CompanionAiMode.Defend,
    heroLiveAttackers: 1);
AvalonAiExecutionResult defendExecution = executionRuntime.Execute(
    AvalonCompanionsAiPackage.PackageId);
Check(
    defendExecution.Status == AvalonAiExecutionStatus.ProposalDispatched,
    "real-package-defend-executes-through-host");
Check(
    executionHost.DispatchedProposals.Count == 2 &&
    executionHost.DispatchedProposals[1].Kind == CompanionCommandProposalKind.NativeDefendPrompt,
    "real-package-host-receives-native-defend-proposal");

executionHost.Observation = Observation();
Check(
    executionRuntime.Execute(AvalonCompanionsAiPackage.PackageId).Status ==
        AvalonAiExecutionStatus.NoProposal,
    "real-package-no-op-completes-without-dispatch");
Check(executionHost.DispatchedProposals.Count == 2, "real-package-no-op-does-not-dispatch");

Check(
    executionRuntime.Stop().Status == AvalonAiStopStatus.Stopped,
    "real-package-execution-runtime-stops");
Check(executionHost.DeactivationCount == 1, "real-package-host-releases-ownership");

if (failures.Count == 0)
{
    Console.WriteLine("Avalon Companions AI package fixture gate passed.");
    return 0;
}

Console.Error.WriteLine(
    "Avalon Companions AI package fixture gate failed: " + string.Join(", ", failures));
return 1;

void AssertAdvancedProposal(
    AvalonAiRuntime targetRuntime,
    AdvancedCompanionObservation observation,
    AdvancedCompanionProposalKind expectedKind,
    AdvancedCompanionGoal expectedGoal,
    string name)
{
    AvalonAiEvaluationResult result = targetRuntime.Evaluate(
        AvalonCompanionAdvancedAiPackage.PackageId,
        observation);
    var proposal = result.Proposal as AdvancedCompanionCommandProposal;
    Check(result.Status == AvalonAiEvaluationStatus.ProposalProduced, name + "-produces-proposal");
    Check(proposal?.Kind == expectedKind, name + "-proposal-kind");
    Check(proposal?.Goal == expectedGoal, name + "-proposal-goal");
    Check(proposal?.DecisionProposalOnly == true, name + "-proposal-only");
    Check(proposal?.DirectGameCalls == false, name + "-no-direct-game-calls");
    Check(proposal?.WritesPersistence == false, name + "-no-persistence-write");
}

void AssertAdvancedNoProposal(
    AvalonAiRuntime targetRuntime,
    AdvancedCompanionObservation observation,
    string expectedReason,
    string name)
{
    AvalonAiEvaluationResult result = targetRuntime.Evaluate(
        AvalonCompanionAdvancedAiPackage.PackageId,
        observation);
    AdvancedCompanionDecision decision = AdvancedCompanionDecisionPolicy.Evaluate(observation);
    Check(result.Status == AvalonAiEvaluationStatus.NoProposal, name);
    Check(decision.ProposalKind == AdvancedCompanionProposalKind.NoProposal, name + "-decision-no-proposal");
    Check(decision.ReasonCode == expectedReason, name + "-reason");
}

void AssertAdvancedAssemblyBoundary()
{
    var assembly = typeof(AvalonCompanionAdvancedAiPackage).Assembly;
    string[] references = assembly.GetReferencedAssemblies()
        .Select(reference => reference.Name ?? string.Empty)
        .ToArray();

    Check(references.Contains("AvalonAI.Contracts"), "advanced-package-references-contracts");
    Check(!references.Any(name => name.StartsWith("AvalonAI.Runtime", StringComparison.Ordinal)), "advanced-package-does-not-reference-runtime");
    Check(!references.Any(name => name.StartsWith("AvalonAI.Blackboard", StringComparison.Ordinal)), "advanced-package-does-not-reference-rabbit");
    Check(!references.Any(name => name.StartsWith("AvalonAI.Planning", StringComparison.Ordinal)), "advanced-package-does-not-reference-goap");
    Check(!references.Any(name => name.StartsWith("AvalonAI.Execution", StringComparison.Ordinal)), "advanced-package-does-not-reference-execution");
    Check(!references.Contains("AvalonCompanions"), "advanced-package-does-not-reference-live-owner");
    Check(!references.Any(name => name.StartsWith("Unity", StringComparison.Ordinal) || name == "BepInEx" || name.StartsWith("0Harmony", StringComparison.Ordinal)), "advanced-package-does-not-reference-game-loader");

    bool publicSurfaceClean = true;
    foreach (Type type in assembly.GetExportedTypes())
    {
        foreach (MemberInfo member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            publicSurfaceClean &= !MentionsForbiddenType(member);
        }
    }

    Check(publicSurfaceClean, "advanced-public-surface-contains-no-game-types");
}

bool MentionsForbiddenType(MemberInfo member)
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
            || type.Namespace.StartsWith("Awaken", StringComparison.Ordinal)));
}

sealed class ReadyHost : IAvalonAiHost
{
    public AvalonAiHostSnapshot Inspect()
    {
        return new AvalonAiHostSnapshot(
            "fixture-host",
            AvalonAiHostStatus.Ready,
            "fixture-ready");
    }
}

sealed class CompanionExecutionHost : IAvalonAiExecutionHost
{
    internal CompanionExecutionHost(CompanionIntentObservation observation)
    {
        Observation = observation;
    }

    internal CompanionIntentObservation Observation { get; set; }

    internal string ActivatedPackageId { get; private set; } = string.Empty;

    internal int DeactivationCount { get; private set; }

    internal List<CompanionCommandProposal> DispatchedProposals { get; } =
        new List<CompanionCommandProposal>();

    public AvalonAiHostSnapshot Inspect()
    {
        return new AvalonAiHostSnapshot(
            "fixture-companion-host",
            AvalonAiHostStatus.Ready,
            "fixture-ready");
    }

    public AvalonAiHostActivationResult Activate(IReadOnlyList<string> packageIds)
    {
        if (packageIds.Count != 1 ||
            !string.Equals(
                packageIds[0],
                AvalonCompanionsAiPackage.PackageId,
                StringComparison.Ordinal))
        {
            return new AvalonAiHostActivationResult(
                AvalonAiHostActivationStatus.Rejected,
                "fixture-package-ownership-mismatch");
        }

        ActivatedPackageId = packageIds[0];
        return new AvalonAiHostActivationResult(
            AvalonAiHostActivationStatus.Activated,
            "fixture-package-ownership-acquired");
    }

    public void Deactivate()
    {
        DeactivationCount++;
        ActivatedPackageId = string.Empty;
    }

    public IAvalonAiObservation? CollectObservation(string packageId)
    {
        return string.Equals(
            packageId,
            AvalonCompanionsAiPackage.PackageId,
            StringComparison.Ordinal)
                ? Observation
                : null;
    }

    public bool TryDispatch(string packageId, IAvalonAiCommandProposal proposal)
    {
        if (!string.Equals(
                packageId,
                AvalonCompanionsAiPackage.PackageId,
                StringComparison.Ordinal) ||
            !(proposal is CompanionCommandProposal companionProposal))
        {
            return false;
        }

        DispatchedProposals.Add(companionProposal);
        return true;
    }
}

sealed class UnrelatedObservation : IAvalonAiObservation
{
}
