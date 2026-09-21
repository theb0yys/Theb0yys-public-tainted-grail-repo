using System;
using System.Linq;
using System.Reflection;
using AvalonAI.Contracts;
using AvalonHumans.AI.Package;

Console.WriteLine(AvalonHumanCompanionExecutorPackage.PackageMarkerString);

int checks = 0;
AvalonHumansAiPackage package = new AvalonHumansAiPackage();

Assert(package.Manifest.PackageId == AvalonHumansAiPackage.PackageId, "stable package id");
Assert(package.Manifest.RequiredRuntimeApi == new AvalonAiApiVersion(1, 1), "runtime API requirement");
Assert(HumanIntentClassifier.Classify(null) == HumanAiIntent.EvidenceInsufficient, "null observation fails closed");
Assert(Classify(hasManagedHuman: false) == HumanAiIntent.NoManagedHuman, "missing managed human");
Assert(Classify(alive: null) == HumanAiIntent.EvidenceInsufficient, "unknown life state fails closed");
Assert(Classify(mode: (HumanCompanionMode)99) == HumanAiIntent.EvidenceInsufficient, "undefined mode fails closed");
Assert(Classify(mode: HumanCompanionMode.Hold) == HumanAiIntent.ApplyHoldMovementLockCandidate, "hold applies movement lock");
Assert(Classify(mode: HumanCompanionMode.Hold, holdMovementLockAttached: true) == HumanAiIntent.HoldPosition, "hold remains inert once locked");
Assert(Classify(holdMovementLockAttached: true) == HumanAiIntent.ReleaseHoldMovementLockCandidate, "leaving hold releases lock");
Assert(Classify(distanceToHero: 80f) == HumanAiIntent.EmergencyRecallCandidate, "emergency recall priority");
Assert(Classify(mode: HumanCompanionMode.Defend, distanceToHero: 15f, heroLiveAttackers: 1, nativeDefendAllowed: true) == HumanAiIntent.CombatRecallCandidate, "combat recall priority");
Assert(Classify(distanceToHero: 12f) == HumanAiIntent.FollowRecallCandidate, "follow catch-up");
Assert(Classify(mode: HumanCompanionMode.Defend, distanceToHero: 2f, heroLiveAttackers: 1, nativeDefendAllowed: true) == HumanAiIntent.NativeDefendCandidate, "native defend candidate");
Assert(Classify(mode: HumanCompanionMode.Defend, distanceToHero: 2f, heroLiveAttackers: 1, nativeDefendAllowed: false) == HumanAiIntent.ThreatObserved, "unapproved threat does not prompt defend");
Assert(Classify(mode: HumanCompanionMode.Defend, distanceToHero: 2f) == HumanAiIntent.DefendWaitingForThreat, "defend wait");
Assert(Classify(distanceToHero: 2f) == HumanAiIntent.KeepingClose, "follow close no-op");

AssertProposal(Observe(mode: HumanCompanionMode.Hold), HumanCommandProposalKind.ApplyHoldMovementLock, HumanRecallPlacementKind.None);
AssertProposal(Observe(holdMovementLockAttached: true), HumanCommandProposalKind.ReleaseHoldMovementLock, HumanRecallPlacementKind.None);
AssertProposal(Observe(distanceToHero: 80f), HumanCommandProposalKind.RecallToApprovedPlacement, HumanRecallPlacementKind.FollowRole);
AssertProposal(Observe(mode: HumanCompanionMode.Defend, distanceToHero: 15f, heroLiveAttackers: 1, nativeDefendAllowed: true), HumanCommandProposalKind.RecallToApprovedPlacement, HumanRecallPlacementKind.CombatRole);
AssertProposal(Observe(mode: HumanCompanionMode.Defend, distanceToHero: 2f, heroLiveAttackers: 1, nativeDefendAllowed: true), HumanCommandProposalKind.PromptNativeDefend, HumanRecallPlacementKind.None);
Assert(package.Evaluate(Observe(distanceToHero: 2f)) == null, "non-actionable human intent produces no proposal");
Assert(package.Evaluate(new UnsupportedObservation()) == null, "unsupported observation ignored");

AvalonHumanCompanionExecutorPackage executorPackage = new AvalonHumanCompanionExecutorPackage();
Assert(executorPackage.Manifest.PackageId == AvalonHumanCompanionExecutorPackage.PackageId, "executor package id");
Assert(executorPackage.Manifest.PackageVersion == AvalonHumanCompanionExecutorPackage.PackageVersion, "executor package version");
Assert(executorPackage.Manifest.RequiredRuntimeApi == new AvalonAiApiVersion(1, 1), "executor package contracts API");
Assert(AvalonHumanCompanionExecutorPackage.TargetActors.Contains("assigned", StringComparison.Ordinal), "executor target actors declared");
Assert(AvalonHumanCompanionExecutorPackage.UnityVersion.Contains("Unity 6", StringComparison.Ordinal), "executor Unity target declared");

HumanCompanionExecutorConfig config = HumanCompanionExecutorConfig.CreateDefault();
config.DefaultActionDuration = TimeSpan.FromMilliseconds(200);
config.DefaultActionTimeout = TimeSpan.FromSeconds(1);
config.StuckDetectionTime = TimeSpan.FromMilliseconds(100);
config.SetRoleProfile(HumanActorRole.Boss, new HumanRoleProfile(HumanActorRole.Boss, 6d, 3d, 2.5d, 180d, TimeSpan.FromSeconds(2)));
Assert(config.RoleProfiles.ContainsKey(HumanActorRole.Boss), "executor supports per-role profiles");
Assert(config.PackageTickCadence > TimeSpan.Zero, "executor tick cadence configurable");
Assert(config.SoftLeashDistance > 0d && config.HardLeashDistance > config.SoftLeashDistance, "executor leash config");
Assert(config.TeleportRecoveryDistance > config.HardLeashDistance, "teleport is hard-distance recovery config");
Assert(config.VisionRange > 0d && config.FieldOfViewDegrees > 0d && config.HearingRange > 0d, "perception config exposed");
Assert(config.ThreatDamageWeight > 0d && config.ThreatDangerWeight > 0d, "threat weights exposed");

HumanCompanionActionExecutor executor = executorPackage.CreateExecutor(config);
Assert(executor.Bind(Binding()).Status == HumanExecutionStatus.Accepted, "bind exact actor");
Assert(executor.Bind(Binding(actorId: "human:other")).ReasonCode == HumanExecutorReasonCodes.ActorAlreadyBound, "bind rejects second actor");
Assert(executor.AcceptObservation(Observation()).Status == HumanExecutionStatus.Accepted, "accepts data-only observation snapshot");
Assert(executor.AcceptObservation(Observation(actorId: "human:wrong")).ReasonCode == HumanExecutorReasonCodes.WrongActor, "observation rejects wrong actor");
Assert(executor.AcceptObservation(Observation(mainThread: false)).ReasonCode == HumanExecutorReasonCodes.NotMainThread, "observation requires main thread");

Assert(executor.AssignTarget(Target()).Status == HumanExecutionStatus.Accepted, "assign target");
Assert(executor.ValidateTarget("target:bandit").Status == HumanExecutionStatus.Succeeded, "validate target");
Assert(executor.ReplaceTarget(Target("target:archer")).Status == HumanExecutionStatus.Accepted, "replace target");
Assert(executor.ValidateTarget("target:archer").Status == HumanExecutionStatus.Succeeded, "validate replacement target");
Assert(executor.ClearTarget().Status == HumanExecutionStatus.Succeeded, "clear target");
Assert(executor.AssignTarget(Target()).Status == HumanExecutionStatus.Accepted, "reassign target");
Assert(executor.AssignTarget(Target("target:protected", HumanTargetKind.Protected)).ReasonCode == HumanExecutorReasonCodes.ProtectedTargetRejected, "protected target rejected");
Assert(executor.AssignTarget(Target("target:stale", stale: true)).ReasonCode == HumanExecutorReasonCodes.TargetInvalid, "stale target rejected");

AssertAllRequiredActionKinds();

HumanActionDispatchResult dispatch = executor.Dispatch(Request(HumanActionKind.Follow, targetId: string.Empty));
Assert(dispatch.Status == HumanExecutionStatus.Accepted, "dispatch returns accepted");
Assert(!string.IsNullOrWhiteSpace(dispatch.Handle), "dispatch returns unique handle");
HumanActionResult running = executor.Poll(dispatch.Handle, TimeSpan.FromMilliseconds(50));
Assert(running.Status == HumanExecutionStatus.Running, "poll returns running");
HumanActionResult success = executor.Poll(dispatch.Handle, TimeSpan.FromMilliseconds(250));
Assert(success.Status == HumanExecutionStatus.Succeeded, "poll returns succeeded");

HumanActionDispatchResult second = executor.Dispatch(Request(HumanActionKind.HoldPosition, targetId: string.Empty));
Assert(second.Handle != dispatch.Handle, "handles are unique per action");
Assert(executor.Inspect().MovementStateLocked, "hold action records movement lock");
HumanActionResult cancelled = executor.Cancel(second.Handle);
Assert(cancelled.Status == HumanExecutionStatus.Interrupted, "cancel interrupts action");
Assert(!executor.Inspect().MovementStateLocked, "cancel clears movement lock");

HumanActionDispatchResult timeout = executor.Dispatch(Request(HumanActionKind.MoveToPosition, targetId: string.Empty, durationMs: 500, timeoutMs: 100));
HumanActionResult timedOut = executor.Poll(timeout.Handle, TimeSpan.FromMilliseconds(150));
Assert(timedOut.Status == HumanExecutionStatus.TimedOut, "timeout returns timed out");
Assert(timedOut.ReasonCode == HumanExecutorReasonCodes.ActionTimedOut, "timeout reason stable");

Assert(executor.Dispatch(Request(HumanActionKind.Follow, actorId: "human:wrong", targetId: string.Empty)).ReasonCode == HumanExecutorReasonCodes.WrongActor, "wrong actor rejected");
Assert(executor.Dispatch(Request(HumanActionKind.Follow, leaseId: "lease:stale", targetId: string.Empty)).ReasonCode == HumanExecutorReasonCodes.StaleLease, "stale lease rejected");
Assert(executor.Dispatch(Request(HumanActionKind.ApproachTarget, targetId: "target:wrong")).ReasonCode == HumanExecutorReasonCodes.WrongTarget, "wrong target rejected");

HumanActionDispatchResult targetAction = executor.Dispatch(Request(HumanActionKind.ApproachTarget));
Assert(targetAction.Status == HumanExecutionStatus.Accepted, "target action accepted");
Assert(executor.AcceptObservation(Observation(targetExists: false, targetDestroyed: true)).Status == HumanExecutionStatus.Accepted, "destroyed target observation accepted");
HumanActionResult targetDestroyed = executor.Poll(targetAction.Handle, TimeSpan.FromMilliseconds(50));
Assert(targetDestroyed.Status == HumanExecutionStatus.Failed, "destroyed target fails action");
Assert(targetDestroyed.ReasonCode == HumanExecutorReasonCodes.TargetDestroyed, "destroyed target reason stable");

Assert(executor.AcceptObservation(Observation()).Status == HumanExecutionStatus.Accepted, "reset good observation");
HumanActionDispatchResult actorAction = executor.Dispatch(Request(HumanActionKind.Recall, targetId: string.Empty));
Assert(actorAction.Status == HumanExecutionStatus.Accepted, "actor action accepted before destruction");
Assert(executor.AcceptObservation(Observation(actorExists: false)).Status == HumanExecutionStatus.Accepted, "destroyed actor observation accepted");
HumanActionResult actorDestroyed = executor.Poll(actorAction.Handle, TimeSpan.FromMilliseconds(50));
Assert(actorDestroyed.Status == HumanExecutionStatus.Failed, "destroyed actor fails action");
Assert(actorDestroyed.ReasonCode == HumanExecutorReasonCodes.ActorDestroyed, "destroyed actor reason stable");

Assert(executor.AcceptObservation(Observation()).Status == HumanExecutionStatus.Accepted, "reset actor alive observation");
HumanActionDispatchResult pathAction = executor.Dispatch(Request(HumanActionKind.MoveToPosition, targetId: string.Empty));
Assert(pathAction.Status == HumanExecutionStatus.Accepted, "path action accepted");
Assert(executor.AcceptObservation(Observation(pathState: HumanPathState.Failed)).Status == HumanExecutionStatus.Accepted, "path failure observation accepted");
HumanActionResult pathFailed = executor.Poll(pathAction.Handle, TimeSpan.FromMilliseconds(50));
Assert(pathFailed.Status == HumanExecutionStatus.Failed, "path failure fails action");
Assert(pathFailed.ReasonCode == HumanExecutorReasonCodes.PathFailed, "path failure reason stable");

Assert(executor.AcceptObservation(Observation()).Status == HumanExecutionStatus.Accepted, "reset clear path observation");
HumanActionDispatchResult stuckAction = executor.Dispatch(Request(HumanActionKind.MoveToPosition, targetId: string.Empty));
Assert(stuckAction.Status == HumanExecutionStatus.Accepted, "stuck action accepted");
Assert(executor.AcceptObservation(Observation(pathState: HumanPathState.Stuck, stuckMs: 200)).Status == HumanExecutionStatus.Accepted, "stuck observation accepted");
HumanActionResult stuckFailed = executor.Poll(stuckAction.Handle, TimeSpan.FromMilliseconds(50));
Assert(stuckFailed.Status == HumanExecutionStatus.Failed, "bounded unstuck failure returned");
Assert(stuckFailed.ReasonCode == HumanExecutorReasonCodes.StuckRecoveryBounded, "stuck reason stable");

Assert(executor.AcceptObservation(Observation()).Status == HumanExecutionStatus.Accepted, "reset clear observation");
HumanActionDispatchResult combatAction = executor.Dispatch(Request(HumanActionKind.MeleeAttack));
Assert(combatAction.Status == HumanExecutionStatus.Accepted, "combat action accepted");
Assert(executor.AcceptObservation(Observation(combatInterrupted: true)).Status == HumanExecutionStatus.Accepted, "combat interruption observation accepted");
HumanActionResult combatInterrupted = executor.Poll(combatAction.Handle, TimeSpan.FromMilliseconds(50));
Assert(combatInterrupted.Status == HumanExecutionStatus.Interrupted, "combat interruption interrupts action");
Assert(combatInterrupted.ReasonCode == HumanExecutorReasonCodes.CombatInterrupted, "combat interruption reason stable");

Assert(executor.AcceptObservation(Observation()).Status == HumanExecutionStatus.Accepted, "reset after combat interruption");
HumanActionDispatchResult suspendedAction = executor.Dispatch(Request(HumanActionKind.Follow, targetId: string.Empty, durationMs: 500));
Assert(suspendedAction.Status == HumanExecutionStatus.Accepted, "action accepted before suspend");
Assert(executor.Suspend(HumanSuspensionReason.Menu).Status == HumanExecutionStatus.Succeeded, "menu suspend succeeds");
Assert(executor.Dispatch(Request(HumanActionKind.Follow, targetId: string.Empty)).ReasonCode == HumanExecutorReasonCodes.ActorSuspended, "dispatch rejects while suspended");
Assert(executor.Poll(suspendedAction.Handle, TimeSpan.FromMilliseconds(500)).Status == HumanExecutionStatus.Running, "suspended action does not advance");
Assert(executor.Resume().Status == HumanExecutionStatus.Succeeded, "resume succeeds");
Assert(executor.Poll(suspendedAction.Handle, TimeSpan.FromMilliseconds(500)).Status == HumanExecutionStatus.Succeeded, "resumed action completes");

HumanActionDispatchResult transitionAction = executor.Dispatch(Request(HumanActionKind.Follow, targetId: string.Empty, durationMs: 500));
Assert(transitionAction.Status == HumanExecutionStatus.Accepted, "transition action accepted");
Assert(executor.Suspend(HumanSuspensionReason.Transition).Status == HumanExecutionStatus.Succeeded, "transition suspend succeeds");
Assert(executor.StopAll("human-executor.scene-transition").Status == HumanExecutionStatus.Succeeded, "scene transition stop all succeeds");
Assert(executor.Cancel(transitionAction.Handle).Status == HumanExecutionStatus.Interrupted, "stopped transition action remains interrupted");
Assert(executor.Resume().Status == HumanExecutionStatus.Succeeded, "resume after transition stop succeeds");

HumanActionDispatchResult killAction = executor.Dispatch(Request(HumanActionKind.Follow, targetId: string.Empty, durationMs: 500));
Assert(killAction.Status == HumanExecutionStatus.Accepted, "kill switch action accepted before switch");
Assert(executor.SetKillSwitch(true).Status == HumanExecutionStatus.Succeeded, "kill switch set");
Assert(executor.Dispatch(Request(HumanActionKind.Follow, targetId: string.Empty)).ReasonCode == HumanExecutorReasonCodes.KillSwitchActive, "kill switch rejects dispatch");
Assert(executor.Poll(killAction.Handle, TimeSpan.FromMilliseconds(50)).Status == HumanExecutionStatus.Interrupted, "kill switch interrupts running action");
Assert(executor.SetKillSwitch(false).Status == HumanExecutionStatus.Succeeded, "kill switch clears");

HumanActionDispatchResult lockAction = executor.Dispatch(Request(HumanActionKind.FaceTarget));
Assert(lockAction.Status == HumanExecutionStatus.Accepted, "target lock action accepted");
Assert(executor.Inspect().TargetStateLocked, "target lock visible in diagnostics");
Assert(executor.Unbind().Status == HumanExecutionStatus.Succeeded, "unbind succeeds");
HumanExecutorDiagnostics afterUnbind = executor.Inspect();
Assert(afterUnbind.ActorId == string.Empty, "unbind clears actor id");
Assert(afterUnbind.TargetId == string.Empty, "unbind clears target id");
Assert(!afterUnbind.MovementStateLocked && !afterUnbind.TargetStateLocked && !afterUnbind.AnimationStateLocked, "unbind clears movement targeting animation state");
Assert(afterUnbind.OriginalStateRestored, "unbind restores original state flag");
Assert(executor.Unbind().Status == HumanExecutionStatus.Succeeded, "repeated unbind idempotent");
executor.Dispose();
executor.Dispose();
Assert(true, "repeated dispose idempotent");

using HumanCompanionActionExecutor questExecutor = executorPackage.CreateExecutor(config);
Assert(
    questExecutor.Bind(Binding(actorSource: HumanActorSource.ExistingNativeActor, questCritical: true, hasSnapshot: true)).ReasonCode ==
        HumanExecutorReasonCodes.QuestCriticalRejected,
    "quest critical existing native actor rejected");
Assert(
    questExecutor.Bind(Binding(actorSource: HumanActorSource.ExistingNativeActor, hasSnapshot: false)).ReasonCode ==
        HumanExecutorReasonCodes.NativeStateSnapshotMissing,
    "existing native actor requires snapshot");
Assert(
    questExecutor.Bind(Binding(actorSource: HumanActorSource.ExistingNativeActor, hasSnapshot: true)).Status ==
        HumanExecutionStatus.Accepted,
    "existing native actor accepts only with snapshot");

using HumanCompanionActionExecutor first = executorPackage.CreateExecutor(config);
using HumanCompanionActionExecutor secondExecutor = executorPackage.CreateExecutor(config);
Assert(first.Bind(Binding(actorId: "human:first", leaseId: "lease:first")).Status == HumanExecutionStatus.Accepted, "first isolated actor bound");
Assert(secondExecutor.Bind(Binding(actorId: "human:second", leaseId: "lease:second")).Status == HumanExecutionStatus.Accepted, "second isolated actor bound");
Assert(first.AcceptObservation(Observation(actorId: "human:first")).Status == HumanExecutionStatus.Accepted, "first observation accepted");
Assert(secondExecutor.AcceptObservation(Observation(actorId: "human:second")).Status == HumanExecutionStatus.Accepted, "second observation accepted");
HumanActionDispatchResult firstAction = first.Dispatch(Request(HumanActionKind.Follow, actorId: "human:first", leaseId: "lease:first", targetId: string.Empty));
HumanActionDispatchResult secondAction = secondExecutor.Dispatch(Request(HumanActionKind.Follow, actorId: "human:second", leaseId: "lease:second", targetId: string.Empty));
Assert(firstAction.Handle != secondAction.Handle, "multiple actor handles isolated");
Assert(first.Cancel(firstAction.Handle).Status == HumanExecutionStatus.Interrupted, "first cancel isolated");
Assert(secondExecutor.Poll(secondAction.Handle, TimeSpan.FromMilliseconds(250)).Status == HumanExecutionStatus.Succeeded, "second action survives first cancel");

AssertExecutorAssemblyBoundary();

Console.WriteLine($"Avalon Humans AI package fixtures passed: {checks} checks.");

HumanAiIntent Classify(
    bool hasManagedHuman = true,
    bool? alive = true,
    bool? working = true,
    HumanCompanionMode mode = HumanCompanionMode.Follow,
    bool holdMovementLockAttached = false,
    float? distanceToHero = 2f,
    float followRecallDistance = 10f,
    float combatRecallDistance = 8f,
    float emergencyRecallDistance = 60f,
    int heroLiveAttackers = 0,
    bool nativeDefendAllowed = false,
    bool defendPromptReady = true,
    bool followCatchUpEnabled = true,
    bool emergencyRecallEnabled = true) =>
    HumanIntentClassifier.Classify(Observe(hasManagedHuman, alive, working, mode, holdMovementLockAttached, distanceToHero, followRecallDistance, combatRecallDistance, emergencyRecallDistance, heroLiveAttackers, nativeDefendAllowed, defendPromptReady, followCatchUpEnabled, emergencyRecallEnabled));

HumanBehaviorObservation Observe(
    bool hasManagedHuman = true,
    bool? alive = true,
    bool? working = true,
    HumanCompanionMode mode = HumanCompanionMode.Follow,
    bool holdMovementLockAttached = false,
    float? distanceToHero = 2f,
    float followRecallDistance = 10f,
    float combatRecallDistance = 8f,
    float emergencyRecallDistance = 60f,
    int heroLiveAttackers = 0,
    bool nativeDefendAllowed = false,
    bool defendPromptReady = true,
    bool followCatchUpEnabled = true,
    bool emergencyRecallEnabled = true) =>
    new HumanBehaviorObservation(hasManagedHuman, alive, working, mode, holdMovementLockAttached, distanceToHero, followRecallDistance, combatRecallDistance, emergencyRecallDistance, heroLiveAttackers, nativeDefendAllowed, defendPromptReady, followCatchUpEnabled, emergencyRecallEnabled);

void AssertProposal(HumanBehaviorObservation observation, HumanCommandProposalKind kind, HumanRecallPlacementKind placement)
{
    if (!(package.Evaluate(observation) is HumanCommandProposal proposal))
    {
        throw new InvalidOperationException($"expected human proposal {kind}");
    }

    Assert(proposal.Kind == kind, $"proposal kind {kind}");
    Assert(proposal.RecallPlacement == placement, $"proposal placement {placement}");
}

void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }

    checks++;
}

HumanActorBinding Binding(
    string actorId = "human:managed-1",
    string ownerId = "avalon-human-companions",
    string leaseId = "lease:managed-1",
    HumanActorRole role = HumanActorRole.Follower,
    HumanActorSource actorSource = HumanActorSource.AvalonManagedOneSession,
    bool questCritical = false,
    bool hasSnapshot = false) =>
    new HumanActorBinding(actorId, ownerId, leaseId, role, actorSource, questCritical, hasSnapshot);

HumanTargetAssignment Target(
    string targetId = "target:bandit",
    HumanTargetKind kind = HumanTargetKind.Hostile,
    bool stale = false,
    bool destroyed = false) =>
    new HumanTargetAssignment(targetId, kind, stale, destroyed, priority: 1d);

HumanExecutorObservation Observation(
    string actorId = "human:managed-1",
    string targetId = "target:bandit",
    bool mainThread = true,
    bool actorExists = true,
    bool targetExists = true,
    bool targetDestroyed = false,
    HumanPathState pathState = HumanPathState.Clear,
    int stuckMs = 0,
    HumanNativeState nativeState = HumanNativeState.Idle,
    HumanExecutorMode mode = HumanExecutorMode.Follow,
    bool combatInterrupted = false) =>
    new HumanExecutorObservation(
        actorId,
        targetId,
        mainThread,
        actorExists,
        targetExists,
        targetDestroyed,
        "CampaignMap_HOS",
        new HumanPoint(0d, 0d, 0d),
        new HumanPoint(4d, 0d, 0d),
        new HumanPoint(4d, 0d, 0d),
        distanceToOwner: 4d,
        distanceToTarget: 4d,
        pathState,
        TimeSpan.FromMilliseconds(stuckMs),
        nativeState,
        mode,
        threatScore: 1d,
        combatInterrupted);

HumanActionRequest Request(
    HumanActionKind kind,
    string actorId = "human:managed-1",
    string leaseId = "lease:managed-1",
    string targetId = "target:bandit",
    int durationMs = 200,
    int timeoutMs = 1000) =>
    new HumanActionRequest(
        actorId,
        leaseId,
        kind,
        targetId,
        TimeSpan.FromMilliseconds(durationMs),
        TimeSpan.FromMilliseconds(timeoutMs),
        formationSlot: 1,
        spacingMeters: 2d);

void AssertAllRequiredActionKinds()
{
    HumanActionKind[] required =
    {
        HumanActionKind.Idle,
        HumanActionKind.Follow,
        HumanActionKind.HoldPosition,
        HumanActionKind.MoveToPosition,
        HumanActionKind.ApproachTarget,
        HumanActionKind.FaceTarget,
        HumanActionKind.Strafe,
        HumanActionKind.BackAway,
        HumanActionKind.Retreat,
        HumanActionKind.Regroup,
        HumanActionKind.Stop,
        HumanActionKind.Engage,
        HumanActionKind.Disengage,
        HumanActionKind.MeleeAttack,
        HumanActionKind.RangedAttack,
        HumanActionKind.Block,
        HumanActionKind.Parry,
        HumanActionKind.Dodge,
        HumanActionKind.Interrupt,
        HumanActionKind.Reposition,
        HumanActionKind.Pursue,
        HumanActionKind.ComeClose,
        HumanActionKind.Recall,
        HumanActionKind.PartWays,
        HumanActionKind.DefendPosition,
        HumanActionKind.DefendActor,
        HumanActionKind.Telegraph,
        HumanActionKind.AttackSequence,
        HumanActionKind.PhaseTransition,
        HumanActionKind.Enrage,
        HumanActionKind.SummonRequest,
        HumanActionKind.StopAll,
    };

    foreach (HumanActionKind kind in required)
    {
        Assert(Enum.IsDefined(typeof(HumanActionKind), kind), "required action kind " + kind);
    }
}

void AssertExecutorAssemblyBoundary()
{
    Assembly assembly = typeof(AvalonHumanCompanionExecutorPackage).Assembly;
    string[] references = assembly.GetReferencedAssemblies()
        .Select(reference => reference.Name ?? string.Empty)
        .ToArray();

    Assert(references.Contains("AvalonAI.Contracts"), "executor references contracts");
    Assert(!references.Any(name => name.StartsWith("AvalonAI.Runtime", StringComparison.Ordinal)), "executor does not reference runtime");
    Assert(!references.Any(name => name.StartsWith("AvalonAI.Blackboard", StringComparison.Ordinal)), "executor does not reference blackboard");
    Assert(!references.Any(name => name.StartsWith("AvalonAI.Planning", StringComparison.Ordinal)), "executor does not reference planning");
    Assert(!references.Any(name => name.StartsWith("AvalonAI.Execution", StringComparison.Ordinal)), "executor does not reference execution");
    Assert(!references.Any(name => name.StartsWith("Unity", StringComparison.Ordinal) || name == "BepInEx" || name.StartsWith("0Harmony", StringComparison.Ordinal)), "executor has no game loader references");

    bool publicSurfaceClean = true;
    foreach (Type type in assembly.GetExportedTypes())
    {
        foreach (MemberInfo member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            publicSurfaceClean &= !MentionsForbiddenType(member);
        }
    }

    Assert(publicSurfaceClean, "executor public surface has no game types");
}

bool MentionsForbiddenType(MemberInfo member)
{
    Type[] types = member switch
    {
        MethodInfo method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType).ToArray(),
        ConstructorInfo constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType).ToArray(),
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

sealed class UnsupportedObservation : IAvalonAiObservation
{
}
