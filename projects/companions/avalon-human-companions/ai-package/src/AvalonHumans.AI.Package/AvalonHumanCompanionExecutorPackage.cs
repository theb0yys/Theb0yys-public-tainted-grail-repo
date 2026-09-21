using System;
using System.Collections.Generic;
using AvalonAI.Contracts;

namespace AvalonHumans.AI.Package;

public sealed class AvalonHumanCompanionExecutorPackage : IAvalonAiPackage
{
    public const string PackageId = "kane.tgfoa.avalon-human-companions.ai-executor";
    public const string PackageVersion = "0.1.0";
    public const string PackageMarkerString = "AVALON_HUMAN_COMPANION_EXECUTOR_PACKAGE_PASS";
    public const string TargetActors = "assigned Avalon-managed one-session human proof actors; existing native actors require snapshot/suspend/restore proof";
    public const string UnityVersion = "Unity 6 adapter target; core package netstandard2.1 and engine-neutral";

    public AvalonHumanCompanionExecutorPackage()
    {
        Manifest = new AvalonAiPackageManifest(
            PackageId,
            "Avalon Human Companion Executor",
            PackageVersion,
            new AvalonAiApiVersion(1, 1));
    }

    public AvalonAiPackageManifest Manifest { get; }

    public HumanCompanionActionExecutor CreateExecutor(HumanCompanionExecutorConfig? config = null)
    {
        return new HumanCompanionActionExecutor(config ?? HumanCompanionExecutorConfig.CreateDefault());
    }
}

public sealed class HumanCompanionActionExecutor : IDisposable
{
    private readonly Dictionary<string, HumanRunningAction> actions =
        new Dictionary<string, HumanRunningAction>(StringComparer.Ordinal);
    private readonly HumanCompanionExecutorConfig config;
    private int nextActionOrdinal;
    private HumanActorBinding? actor;
    private HumanTargetAssignment? target;
    private HumanExecutorObservation? observation;
    private HumanSuspensionReason suspensionReason = HumanSuspensionReason.None;
    private bool disposed;
    private bool killSwitchActive;
    private bool originalStateRestored = true;
    private bool movementStateLocked;
    private bool targetStateLocked;
    private bool animationStateLocked;

    public HumanCompanionActionExecutor(HumanCompanionExecutorConfig config)
    {
        this.config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public HumanExecutorOperationResult Bind(HumanActorBinding binding)
    {
        EnsureNotDisposed();

        if (killSwitchActive)
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.KillSwitchActive);
        }

        if (binding == null || string.IsNullOrWhiteSpace(binding.ActorId))
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.ActorIdMissing);
        }

        if (actor != null && !string.Equals(actor.ActorId, binding.ActorId, StringComparison.Ordinal))
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.ActorAlreadyBound);
        }

        if (binding.ActorSource == HumanActorSource.ExistingNativeActor)
        {
            if (binding.QuestCritical)
            {
                return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.QuestCriticalRejected);
            }

            if (!binding.HasNativeStateSnapshot)
            {
                return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.NativeStateSnapshotMissing);
            }
        }

        actor = binding;
        originalStateRestored = false;
        return HumanExecutorOperationResult.Accepted(HumanExecutorReasonCodes.ActorBound);
    }

    public HumanExecutorOperationResult Unbind(string? reasonCode = null)
    {
        EnsureNotDisposed();
        StopAll(reasonCode ?? HumanExecutorReasonCodes.ActorUnbound);
        actor = null;
        target = null;
        observation = null;
        suspensionReason = HumanSuspensionReason.None;
        movementStateLocked = false;
        targetStateLocked = false;
        animationStateLocked = false;
        originalStateRestored = true;
        return HumanExecutorOperationResult.Succeeded(reasonCode ?? HumanExecutorReasonCodes.ActorUnbound);
    }

    public HumanExecutorOperationResult AssignTarget(HumanTargetAssignment assignment)
    {
        EnsureNotDisposed();

        HumanExecutorOperationResult actorResult = RequireActor();
        if (!actorResult.IsAcceptedOrSucceeded)
        {
            return actorResult;
        }

        if (assignment == null || string.IsNullOrWhiteSpace(assignment.TargetId))
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.TargetIdMissing);
        }

        if (assignment.Destroyed || assignment.Stale)
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.TargetInvalid);
        }

        if (assignment.Kind == HumanTargetKind.Protected)
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.ProtectedTargetRejected);
        }

        target = assignment;
        return HumanExecutorOperationResult.Accepted(HumanExecutorReasonCodes.TargetAssigned);
    }

    public HumanExecutorOperationResult ReplaceTarget(HumanTargetAssignment assignment)
    {
        HumanExecutorOperationResult clear = ClearTarget();
        if (!clear.IsAcceptedOrSucceeded)
        {
            return clear;
        }

        return AssignTarget(assignment);
    }

    public HumanExecutorOperationResult ValidateTarget(string targetId)
    {
        EnsureNotDisposed();

        if (target == null)
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.TargetMissing);
        }

        if (!string.Equals(target.TargetId, targetId, StringComparison.Ordinal))
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.WrongTarget);
        }

        if (target.Destroyed || target.Stale)
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.TargetInvalid);
        }

        return HumanExecutorOperationResult.Succeeded(HumanExecutorReasonCodes.TargetValid);
    }

    public HumanExecutorOperationResult ClearTarget()
    {
        EnsureNotDisposed();
        target = null;
        targetStateLocked = false;
        return HumanExecutorOperationResult.Succeeded(HumanExecutorReasonCodes.TargetCleared);
    }

    public HumanExecutorOperationResult AcceptObservation(HumanExecutorObservation snapshot)
    {
        EnsureNotDisposed();

        HumanExecutorOperationResult actorResult = RequireActor();
        if (!actorResult.IsAcceptedOrSucceeded)
        {
            return actorResult;
        }

        if (snapshot == null || !string.Equals(actor!.ActorId, snapshot.ActorId, StringComparison.Ordinal))
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.WrongActor);
        }

        if (!snapshot.MainThread)
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.NotMainThread);
        }

        observation = snapshot;
        return HumanExecutorOperationResult.Accepted(HumanExecutorReasonCodes.ObservationAccepted);
    }

    public HumanActionDispatchResult Dispatch(HumanActionRequest request)
    {
        EnsureNotDisposed();

        HumanExecutorOperationResult validation = ValidateActionRequest(request);
        if (!validation.IsAcceptedOrSucceeded)
        {
            return HumanActionDispatchResult.Rejected(validation.ReasonCode);
        }

        HumanActionKind kind = request.Kind;
        if (kind == HumanActionKind.StopAll)
        {
            StopAll(HumanExecutorReasonCodes.StopAll);
            return HumanActionDispatchResult.Accepted(
                CreateHandle(kind),
                kind,
                HumanExecutorReasonCodes.StopAll);
        }

        string handle = CreateHandle(kind);
        var running = new HumanRunningAction(
            handle,
            kind,
            request.ActorId,
            request.TargetId,
            request.LeaseId,
            request.Timeout == TimeSpan.Zero ? config.DefaultActionTimeout : request.Timeout,
            request.ExpectedDuration == TimeSpan.Zero ? config.DefaultActionDuration : request.ExpectedDuration);
        actions[handle] = running;
        ApplyLocksFor(kind);

        return HumanActionDispatchResult.Accepted(handle, kind, HumanExecutorReasonCodes.ActionAccepted);
    }

    public HumanActionResult Poll(string handle, TimeSpan delta)
    {
        EnsureNotDisposed();

        if (string.IsNullOrWhiteSpace(handle) || !actions.TryGetValue(handle, out HumanRunningAction? action))
        {
            return HumanActionResult.Rejected(handle ?? string.Empty, HumanActionKind.Idle, HumanExecutorReasonCodes.ActionHandleUnknown);
        }

        if (action.Terminal)
        {
            return action.ToResult();
        }

        if (killSwitchActive)
        {
            action.Interrupt(HumanExecutorReasonCodes.KillSwitchActive);
            ReleaseLocksFor(action.Kind);
            return action.ToResult();
        }

        if (suspensionReason != HumanSuspensionReason.None)
        {
            return HumanActionResult.Running(action.Handle, action.Kind, HumanExecutorReasonCodes.ActorSuspended);
        }

        HumanActionResult? fault = EvaluateFault(action);
        if (fault != null)
        {
            ReleaseLocksFor(action.Kind);
            return fault;
        }

        TimeSpan positiveDelta = delta < TimeSpan.Zero ? TimeSpan.Zero : delta;
        action.Advance(positiveDelta);

        if (action.Elapsed >= action.Timeout)
        {
            action.TimeoutAction(HumanExecutorReasonCodes.ActionTimedOut);
            ReleaseLocksFor(action.Kind);
            return action.ToResult();
        }

        if (action.Elapsed >= action.ExpectedDuration)
        {
            action.Succeed(HumanExecutorReasonCodes.ActionSucceeded);
            ReleaseLocksFor(action.Kind);
            return action.ToResult();
        }

        return action.ToResult();
    }

    public HumanActionResult Cancel(string handle, string? reasonCode = null)
    {
        EnsureNotDisposed();

        if (string.IsNullOrWhiteSpace(handle) || !actions.TryGetValue(handle, out HumanRunningAction? action))
        {
            return HumanActionResult.Rejected(handle ?? string.Empty, HumanActionKind.Idle, HumanExecutorReasonCodes.ActionHandleUnknown);
        }

        action.Interrupt(reasonCode ?? HumanExecutorReasonCodes.ActionCancelled);
        ReleaseLocksFor(action.Kind);
        return action.ToResult();
    }

    public HumanExecutorOperationResult StopAll(string? reasonCode = null)
    {
        EnsureNotDisposed(allowDisposed: true);

        foreach (HumanRunningAction action in actions.Values)
        {
            if (!action.Terminal)
            {
                action.Interrupt(reasonCode ?? HumanExecutorReasonCodes.StopAll);
            }
        }

        movementStateLocked = false;
        targetStateLocked = false;
        animationStateLocked = false;
        return HumanExecutorOperationResult.Succeeded(reasonCode ?? HumanExecutorReasonCodes.StopAll);
    }

    public HumanExecutorOperationResult Suspend(HumanSuspensionReason reason)
    {
        EnsureNotDisposed();

        if (reason == HumanSuspensionReason.None)
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.InvalidSuspensionReason);
        }

        suspensionReason = reason;
        return HumanExecutorOperationResult.Succeeded(HumanExecutorReasonCodes.ActorSuspended);
    }

    public HumanExecutorOperationResult Resume()
    {
        EnsureNotDisposed();
        suspensionReason = HumanSuspensionReason.None;
        return HumanExecutorOperationResult.Succeeded(HumanExecutorReasonCodes.ActorResumed);
    }

    public HumanExecutorOperationResult SetKillSwitch(bool active)
    {
        EnsureNotDisposed();
        killSwitchActive = active;
        if (active)
        {
            StopAll(HumanExecutorReasonCodes.KillSwitchActive);
        }

        return HumanExecutorOperationResult.Succeeded(active
            ? HumanExecutorReasonCodes.KillSwitchActive
            : HumanExecutorReasonCodes.KillSwitchCleared);
    }

    public HumanExecutorDiagnostics Inspect()
    {
        string currentAction = string.Empty;
        HumanExecutionStatus currentStatus = HumanExecutionStatus.Succeeded;
        string currentReason = HumanExecutorReasonCodes.NoAction;

        foreach (HumanRunningAction action in actions.Values)
        {
            if (!action.Terminal)
            {
                currentAction = action.Handle;
                currentStatus = action.Status;
                currentReason = action.ReasonCode;
                break;
            }
        }

        return new HumanExecutorDiagnostics(
            actor?.ActorId ?? string.Empty,
            target?.TargetId ?? string.Empty,
            actor?.Role ?? HumanActorRole.Follower,
            observation?.Mode ?? HumanExecutorMode.Follow,
            currentAction,
            currentStatus,
            currentReason,
            observation?.ThreatScore ?? 0d,
            observation?.PathState ?? HumanPathState.Unknown,
            config.DefaultActionTimeout,
            killSwitchActive,
            suspensionReason,
            movementStateLocked,
            targetStateLocked,
            animationStateLocked,
            originalStateRestored);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        StopAll(HumanExecutorReasonCodes.ExecutorDisposed);
        actor = null;
        target = null;
        observation = null;
        suspensionReason = HumanSuspensionReason.None;
        movementStateLocked = false;
        targetStateLocked = false;
        animationStateLocked = false;
        originalStateRestored = true;
        disposed = true;
    }

    private HumanExecutorOperationResult ValidateActionRequest(HumanActionRequest request)
    {
        if (request == null)
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.ActionRequestMissing);
        }

        HumanExecutorOperationResult actorResult = RequireActor();
        if (!actorResult.IsAcceptedOrSucceeded)
        {
            return actorResult;
        }

        if (killSwitchActive)
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.KillSwitchActive);
        }

        if (suspensionReason != HumanSuspensionReason.None)
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.ActorSuspended);
        }

        if (!string.Equals(actor!.ActorId, request.ActorId, StringComparison.Ordinal))
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.WrongActor);
        }

        if (!string.Equals(actor.LeaseId, request.LeaseId, StringComparison.Ordinal))
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.StaleLease);
        }

        if (RequiresTarget(request.Kind))
        {
            HumanExecutorOperationResult targetResult = ValidateTarget(request.TargetId);
            if (!targetResult.IsAcceptedOrSucceeded)
            {
                return targetResult;
            }
        }

        if (observation != null && !observation.ActorExists)
        {
            return HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.ActorDestroyed);
        }

        return HumanExecutorOperationResult.Accepted(HumanExecutorReasonCodes.ActionAccepted);
    }

    private HumanExecutorOperationResult RequireActor()
    {
        return actor == null
            ? HumanExecutorOperationResult.Rejected(HumanExecutorReasonCodes.ActorNotBound)
            : HumanExecutorOperationResult.Succeeded(HumanExecutorReasonCodes.ActorValid);
    }

    private HumanActionResult? EvaluateFault(HumanRunningAction action)
    {
        if (observation == null)
        {
            return null;
        }

        if (!observation.ActorExists || observation.NativeState == HumanNativeState.Dead)
        {
            action.Fail(HumanExecutorReasonCodes.ActorDestroyed);
            return action.ToResult();
        }

        if (RequiresTarget(action.Kind) && (!observation.TargetExists || observation.TargetDestroyed))
        {
            action.Fail(HumanExecutorReasonCodes.TargetDestroyed);
            return action.ToResult();
        }

        if (observation.PathState == HumanPathState.Failed ||
            observation.PathState == HumanPathState.Unreachable)
        {
            action.Fail(HumanExecutorReasonCodes.PathFailed);
            return action.ToResult();
        }

        if (observation.PathState == HumanPathState.Stuck && observation.StuckDuration >= config.StuckDetectionTime)
        {
            action.Fail(HumanExecutorReasonCodes.StuckRecoveryBounded);
            return action.ToResult();
        }

        if (observation.CombatInterrupted)
        {
            action.Interrupt(HumanExecutorReasonCodes.CombatInterrupted);
            return action.ToResult();
        }

        return null;
    }

    private string CreateHandle(HumanActionKind kind)
    {
        nextActionOrdinal++;
        string actorPart = actor?.ActorId ?? "unbound";
        return "human-action:" + actorPart + ":" + kind + ":" + nextActionOrdinal.ToString("D4");
    }

    private void ApplyLocksFor(HumanActionKind kind)
    {
        switch (kind)
        {
            case HumanActionKind.HoldPosition:
            case HumanActionKind.DefendPosition:
                movementStateLocked = true;
                break;
            case HumanActionKind.FaceTarget:
            case HumanActionKind.ApproachTarget:
            case HumanActionKind.Engage:
            case HumanActionKind.MeleeAttack:
            case HumanActionKind.RangedAttack:
            case HumanActionKind.Pursue:
            case HumanActionKind.DefendActor:
                targetStateLocked = true;
                break;
            case HumanActionKind.Telegraph:
            case HumanActionKind.AttackSequence:
            case HumanActionKind.PhaseTransition:
            case HumanActionKind.Enrage:
                animationStateLocked = true;
                break;
        }
    }

    private void ReleaseLocksFor(HumanActionKind kind)
    {
        switch (kind)
        {
            case HumanActionKind.HoldPosition:
            case HumanActionKind.DefendPosition:
                movementStateLocked = false;
                break;
            case HumanActionKind.FaceTarget:
            case HumanActionKind.ApproachTarget:
            case HumanActionKind.Engage:
            case HumanActionKind.MeleeAttack:
            case HumanActionKind.RangedAttack:
            case HumanActionKind.Pursue:
            case HumanActionKind.DefendActor:
                targetStateLocked = false;
                break;
            case HumanActionKind.Telegraph:
            case HumanActionKind.AttackSequence:
            case HumanActionKind.PhaseTransition:
            case HumanActionKind.Enrage:
                animationStateLocked = false;
                break;
        }
    }

    private static bool RequiresTarget(HumanActionKind kind)
    {
        switch (kind)
        {
            case HumanActionKind.ApproachTarget:
            case HumanActionKind.FaceTarget:
            case HumanActionKind.Engage:
            case HumanActionKind.MeleeAttack:
            case HumanActionKind.RangedAttack:
            case HumanActionKind.Interrupt:
            case HumanActionKind.Pursue:
            case HumanActionKind.DefendActor:
                return true;
            default:
                return false;
        }
    }

    private void EnsureNotDisposed(bool allowDisposed = false)
    {
        if (disposed && !allowDisposed)
        {
            throw new ObjectDisposedException(nameof(HumanCompanionActionExecutor));
        }
    }
}

public sealed class HumanCompanionExecutorConfig
{
    private readonly Dictionary<HumanActorRole, HumanRoleProfile> roleProfiles;

    private HumanCompanionExecutorConfig()
    {
        roleProfiles = new Dictionary<HumanActorRole, HumanRoleProfile>();
    }

    public TimeSpan PackageTickCadence { get; set; }
    public TimeSpan DefaultActionDuration { get; set; }
    public TimeSpan DefaultActionTimeout { get; set; }
    public TimeSpan CatchUpGracePeriod { get; set; }
    public TimeSpan StuckDetectionTime { get; set; }
    public TimeSpan RepathInterval { get; set; }
    public TimeSpan ReactionDelay { get; set; }
    public double PreferredDistance { get; set; }
    public double FormationSpacing { get; set; }
    public double SoftLeashDistance { get; set; }
    public double HardLeashDistance { get; set; }
    public double TeleportRecoveryDistance { get; set; }
    public double ArrivalDistance { get; set; }
    public double MovementSpeed { get; set; }
    public double RotationSpeed { get; set; }
    public double VisionRange { get; set; }
    public double FieldOfViewDegrees { get; set; }
    public double HearingRange { get; set; }
    public double ProximityAwarenessRange { get; set; }
    public double ThreatDamageWeight { get; set; }
    public double ThreatDistanceWeight { get; set; }
    public double ThreatActivityWeight { get; set; }
    public double ThreatDangerWeight { get; set; }
    public double ThreatDecayPerSecond { get; set; }
    public double PhaseOneThreshold { get; set; }
    public double PhaseTwoThreshold { get; set; }
    public int BoundedUnstuckAttempts { get; set; }

    public IReadOnlyDictionary<HumanActorRole, HumanRoleProfile> RoleProfiles => roleProfiles;

    public void SetRoleProfile(HumanActorRole role, HumanRoleProfile profile)
    {
        roleProfiles[role] = profile ?? throw new ArgumentNullException(nameof(profile));
    }

    public static HumanCompanionExecutorConfig CreateDefault()
    {
        var config = new HumanCompanionExecutorConfig
        {
            PackageTickCadence = TimeSpan.FromMilliseconds(100),
            DefaultActionDuration = TimeSpan.FromMilliseconds(250),
            DefaultActionTimeout = TimeSpan.FromSeconds(3),
            CatchUpGracePeriod = TimeSpan.FromSeconds(2),
            StuckDetectionTime = TimeSpan.FromSeconds(1),
            RepathInterval = TimeSpan.FromMilliseconds(500),
            ReactionDelay = TimeSpan.FromMilliseconds(250),
            PreferredDistance = 4d,
            FormationSpacing = 2d,
            SoftLeashDistance = 18d,
            HardLeashDistance = 60d,
            TeleportRecoveryDistance = 80d,
            ArrivalDistance = 1.5d,
            MovementSpeed = 3.5d,
            RotationSpeed = 360d,
            VisionRange = 35d,
            FieldOfViewDegrees = 120d,
            HearingRange = 18d,
            ProximityAwarenessRange = 6d,
            ThreatDamageWeight = 2d,
            ThreatDistanceWeight = 1d,
            ThreatActivityWeight = 1d,
            ThreatDangerWeight = 2d,
            ThreatDecayPerSecond = 0.1d,
            PhaseOneThreshold = 1d,
            PhaseTwoThreshold = 0.5d,
            BoundedUnstuckAttempts = 2,
        };

        foreach (HumanActorRole role in Enum.GetValues(typeof(HumanActorRole)))
        {
            config.SetRoleProfile(role, HumanRoleProfile.CreateDefault(role));
        }

        return config;
    }
}

public sealed class HumanRoleProfile
{
    public HumanRoleProfile(
        HumanActorRole role,
        double preferredDistance,
        double formationSpacing,
        double movementSpeed,
        double rotationSpeed,
        TimeSpan actionCooldown)
    {
        Role = role;
        PreferredDistance = preferredDistance;
        FormationSpacing = formationSpacing;
        MovementSpeed = movementSpeed;
        RotationSpeed = rotationSpeed;
        ActionCooldown = actionCooldown;
    }

    public HumanActorRole Role { get; }
    public double PreferredDistance { get; }
    public double FormationSpacing { get; }
    public double MovementSpeed { get; }
    public double RotationSpeed { get; }
    public TimeSpan ActionCooldown { get; }

    public static HumanRoleProfile CreateDefault(HumanActorRole role)
    {
        double preferredDistance = role == HumanActorRole.Ranged || role == HumanActorRole.Support ? 8d : 4d;
        return new HumanRoleProfile(role, preferredDistance, 2d, 3.5d, 360d, TimeSpan.FromSeconds(1));
    }
}

public sealed class HumanActorBinding
{
    public HumanActorBinding(
        string actorId,
        string ownerId,
        string leaseId,
        HumanActorRole role,
        HumanActorSource actorSource = HumanActorSource.AvalonManagedOneSession,
        bool questCritical = false,
        bool hasNativeStateSnapshot = false)
    {
        ActorId = actorId ?? string.Empty;
        OwnerId = ownerId ?? string.Empty;
        LeaseId = leaseId ?? string.Empty;
        Role = role;
        ActorSource = actorSource;
        QuestCritical = questCritical;
        HasNativeStateSnapshot = hasNativeStateSnapshot;
    }

    public string ActorId { get; }
    public string OwnerId { get; }
    public string LeaseId { get; }
    public HumanActorRole Role { get; }
    public HumanActorSource ActorSource { get; }
    public bool QuestCritical { get; }
    public bool HasNativeStateSnapshot { get; }
}

public sealed class HumanTargetAssignment
{
    public HumanTargetAssignment(
        string targetId,
        HumanTargetKind kind,
        bool stale = false,
        bool destroyed = false,
        double priority = 0d)
    {
        TargetId = targetId ?? string.Empty;
        Kind = kind;
        Stale = stale;
        Destroyed = destroyed;
        Priority = priority;
    }

    public string TargetId { get; }
    public HumanTargetKind Kind { get; }
    public bool Stale { get; }
    public bool Destroyed { get; }
    public double Priority { get; }
}

public sealed class HumanExecutorObservation
{
    public HumanExecutorObservation(
        string actorId,
        string targetId,
        bool mainThread,
        bool actorExists,
        bool targetExists,
        bool targetDestroyed,
        string scene,
        HumanPoint actorPosition,
        HumanPoint targetPosition,
        HumanPoint lastKnownTargetPosition,
        double distanceToOwner,
        double distanceToTarget,
        HumanPathState pathState,
        TimeSpan stuckDuration,
        HumanNativeState nativeState,
        HumanExecutorMode mode,
        double threatScore,
        bool combatInterrupted)
    {
        ActorId = actorId ?? string.Empty;
        TargetId = targetId ?? string.Empty;
        MainThread = mainThread;
        ActorExists = actorExists;
        TargetExists = targetExists;
        TargetDestroyed = targetDestroyed;
        Scene = scene ?? string.Empty;
        ActorPosition = actorPosition;
        TargetPosition = targetPosition;
        LastKnownTargetPosition = lastKnownTargetPosition;
        DistanceToOwner = distanceToOwner;
        DistanceToTarget = distanceToTarget;
        PathState = pathState;
        StuckDuration = stuckDuration;
        NativeState = nativeState;
        Mode = mode;
        ThreatScore = threatScore;
        CombatInterrupted = combatInterrupted;
    }

    public string ActorId { get; }
    public string TargetId { get; }
    public bool MainThread { get; }
    public bool ActorExists { get; }
    public bool TargetExists { get; }
    public bool TargetDestroyed { get; }
    public string Scene { get; }
    public HumanPoint ActorPosition { get; }
    public HumanPoint TargetPosition { get; }
    public HumanPoint LastKnownTargetPosition { get; }
    public double DistanceToOwner { get; }
    public double DistanceToTarget { get; }
    public HumanPathState PathState { get; }
    public TimeSpan StuckDuration { get; }
    public HumanNativeState NativeState { get; }
    public HumanExecutorMode Mode { get; }
    public double ThreatScore { get; }
    public bool CombatInterrupted { get; }
}

public sealed class HumanActionRequest
{
    public HumanActionRequest(
        string actorId,
        string leaseId,
        HumanActionKind kind,
        string targetId = "",
        TimeSpan expectedDuration = default,
        TimeSpan timeout = default,
        int formationSlot = 0,
        double spacingMeters = 0d)
    {
        ActorId = actorId ?? string.Empty;
        LeaseId = leaseId ?? string.Empty;
        Kind = kind;
        TargetId = targetId ?? string.Empty;
        ExpectedDuration = expectedDuration;
        Timeout = timeout;
        FormationSlot = formationSlot;
        SpacingMeters = spacingMeters;
    }

    public string ActorId { get; }
    public string LeaseId { get; }
    public HumanActionKind Kind { get; }
    public string TargetId { get; }
    public TimeSpan ExpectedDuration { get; }
    public TimeSpan Timeout { get; }
    public int FormationSlot { get; }
    public double SpacingMeters { get; }
}

public sealed class HumanActionDispatchResult
{
    private HumanActionDispatchResult(HumanExecutionStatus status, string handle, HumanActionKind kind, string reasonCode)
    {
        Status = status;
        Handle = handle ?? string.Empty;
        Kind = kind;
        ReasonCode = reasonCode ?? string.Empty;
    }

    public HumanExecutionStatus Status { get; }
    public string Handle { get; }
    public HumanActionKind Kind { get; }
    public string ReasonCode { get; }

    public static HumanActionDispatchResult Accepted(string handle, HumanActionKind kind, string reasonCode)
    {
        return new HumanActionDispatchResult(HumanExecutionStatus.Accepted, handle, kind, reasonCode);
    }

    public static HumanActionDispatchResult Rejected(string reasonCode)
    {
        return new HumanActionDispatchResult(HumanExecutionStatus.Rejected, string.Empty, HumanActionKind.Idle, reasonCode);
    }
}

public sealed class HumanActionResult
{
    private HumanActionResult(HumanExecutionStatus status, string handle, HumanActionKind kind, string reasonCode)
    {
        Status = status;
        Handle = handle ?? string.Empty;
        Kind = kind;
        ReasonCode = reasonCode ?? string.Empty;
    }

    public HumanExecutionStatus Status { get; }
    public string Handle { get; }
    public HumanActionKind Kind { get; }
    public string ReasonCode { get; }

    public static HumanActionResult Running(string handle, HumanActionKind kind, string reasonCode)
    {
        return new HumanActionResult(HumanExecutionStatus.Running, handle, kind, reasonCode);
    }

    public static HumanActionResult Rejected(string handle, HumanActionKind kind, string reasonCode)
    {
        return new HumanActionResult(HumanExecutionStatus.Rejected, handle, kind, reasonCode);
    }

    internal static HumanActionResult FromState(string handle, HumanActionKind kind, HumanExecutionStatus status, string reasonCode)
    {
        return new HumanActionResult(status, handle, kind, reasonCode);
    }
}

public sealed class HumanExecutorOperationResult
{
    private HumanExecutorOperationResult(HumanExecutionStatus status, string reasonCode)
    {
        Status = status;
        ReasonCode = reasonCode ?? string.Empty;
    }

    public HumanExecutionStatus Status { get; }
    public string ReasonCode { get; }
    public bool IsAcceptedOrSucceeded => Status == HumanExecutionStatus.Accepted || Status == HumanExecutionStatus.Succeeded;

    public static HumanExecutorOperationResult Accepted(string reasonCode)
    {
        return new HumanExecutorOperationResult(HumanExecutionStatus.Accepted, reasonCode);
    }

    public static HumanExecutorOperationResult Succeeded(string reasonCode)
    {
        return new HumanExecutorOperationResult(HumanExecutionStatus.Succeeded, reasonCode);
    }

    public static HumanExecutorOperationResult Rejected(string reasonCode)
    {
        return new HumanExecutorOperationResult(HumanExecutionStatus.Rejected, reasonCode);
    }
}

public sealed class HumanExecutorDiagnostics
{
    public HumanExecutorDiagnostics(
        string actorId,
        string targetId,
        HumanActorRole role,
        HumanExecutorMode mode,
        string currentActionHandle,
        HumanExecutionStatus currentActionStatus,
        string currentReasonCode,
        double threatScore,
        HumanPathState pathState,
        TimeSpan defaultTimeout,
        bool killSwitchActive,
        HumanSuspensionReason suspensionReason,
        bool movementStateLocked,
        bool targetStateLocked,
        bool animationStateLocked,
        bool originalStateRestored)
    {
        ActorId = actorId ?? string.Empty;
        TargetId = targetId ?? string.Empty;
        Role = role;
        Mode = mode;
        CurrentActionHandle = currentActionHandle ?? string.Empty;
        CurrentActionStatus = currentActionStatus;
        CurrentReasonCode = currentReasonCode ?? string.Empty;
        ThreatScore = threatScore;
        PathState = pathState;
        DefaultTimeout = defaultTimeout;
        KillSwitchActive = killSwitchActive;
        SuspensionReason = suspensionReason;
        MovementStateLocked = movementStateLocked;
        TargetStateLocked = targetStateLocked;
        AnimationStateLocked = animationStateLocked;
        OriginalStateRestored = originalStateRestored;
    }

    public string ActorId { get; }
    public string TargetId { get; }
    public HumanActorRole Role { get; }
    public HumanExecutorMode Mode { get; }
    public string CurrentActionHandle { get; }
    public HumanExecutionStatus CurrentActionStatus { get; }
    public string CurrentReasonCode { get; }
    public double ThreatScore { get; }
    public HumanPathState PathState { get; }
    public TimeSpan DefaultTimeout { get; }
    public bool KillSwitchActive { get; }
    public HumanSuspensionReason SuspensionReason { get; }
    public bool MovementStateLocked { get; }
    public bool TargetStateLocked { get; }
    public bool AnimationStateLocked { get; }
    public bool OriginalStateRestored { get; }
}

internal sealed class HumanRunningAction
{
    internal HumanRunningAction(
        string handle,
        HumanActionKind kind,
        string actorId,
        string targetId,
        string leaseId,
        TimeSpan timeout,
        TimeSpan expectedDuration)
    {
        Handle = handle;
        Kind = kind;
        ActorId = actorId;
        TargetId = targetId;
        LeaseId = leaseId;
        Timeout = timeout;
        ExpectedDuration = expectedDuration;
        Status = HumanExecutionStatus.Running;
        ReasonCode = HumanExecutorReasonCodes.ActionRunning;
    }

    internal string Handle { get; }
    internal HumanActionKind Kind { get; }
    internal string ActorId { get; }
    internal string TargetId { get; }
    internal string LeaseId { get; }
    internal TimeSpan Timeout { get; }
    internal TimeSpan ExpectedDuration { get; }
    internal TimeSpan Elapsed { get; private set; }
    internal HumanExecutionStatus Status { get; private set; }
    internal string ReasonCode { get; private set; }
    internal bool Terminal => Status == HumanExecutionStatus.Succeeded ||
        Status == HumanExecutionStatus.Failed ||
        Status == HumanExecutionStatus.Interrupted ||
        Status == HumanExecutionStatus.Rejected ||
        Status == HumanExecutionStatus.TimedOut;

    internal void Advance(TimeSpan delta)
    {
        Elapsed += delta;
        Status = HumanExecutionStatus.Running;
        ReasonCode = HumanExecutorReasonCodes.ActionRunning;
    }

    internal void Succeed(string reasonCode)
    {
        Status = HumanExecutionStatus.Succeeded;
        ReasonCode = reasonCode;
    }

    internal void Fail(string reasonCode)
    {
        Status = HumanExecutionStatus.Failed;
        ReasonCode = reasonCode;
    }

    internal void Interrupt(string reasonCode)
    {
        Status = HumanExecutionStatus.Interrupted;
        ReasonCode = reasonCode;
    }

    internal void TimeoutAction(string reasonCode)
    {
        Status = HumanExecutionStatus.TimedOut;
        ReasonCode = reasonCode;
    }

    internal HumanActionResult ToResult()
    {
        return HumanActionResult.FromState(Handle, Kind, Status, ReasonCode);
    }
}

public readonly struct HumanPoint
{
    public HumanPoint(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public double X { get; }
    public double Y { get; }
    public double Z { get; }
}

public static class HumanExecutorReasonCodes
{
    public const string ActionAccepted = "human-executor.action-accepted";
    public const string ActionCancelled = "human-executor.action-cancelled";
    public const string ActionHandleUnknown = "human-executor.action-handle-unknown";
    public const string ActionRequestMissing = "human-executor.action-request-missing";
    public const string ActionRunning = "human-executor.action-running";
    public const string ActionSucceeded = "human-executor.action-succeeded";
    public const string ActionTimedOut = "human-executor.action-timed-out";
    public const string ActorAlreadyBound = "human-executor.actor-already-bound";
    public const string ActorBound = "human-executor.actor-bound";
    public const string ActorDestroyed = "human-executor.actor-destroyed";
    public const string ActorIdMissing = "human-executor.actor-id-missing";
    public const string ActorNotBound = "human-executor.actor-not-bound";
    public const string ActorResumed = "human-executor.actor-resumed";
    public const string ActorSuspended = "human-executor.actor-suspended";
    public const string ActorUnbound = "human-executor.actor-unbound";
    public const string ActorValid = "human-executor.actor-valid";
    public const string CombatInterrupted = "human-executor.combat-interrupted";
    public const string ExecutorDisposed = "human-executor.executor-disposed";
    public const string InvalidSuspensionReason = "human-executor.invalid-suspension-reason";
    public const string KillSwitchActive = "human-executor.kill-switch-active";
    public const string KillSwitchCleared = "human-executor.kill-switch-cleared";
    public const string NativeStateSnapshotMissing = "human-executor.native-state-snapshot-missing";
    public const string NoAction = "human-executor.no-action";
    public const string NotMainThread = "human-executor.not-main-thread";
    public const string ObservationAccepted = "human-executor.observation-accepted";
    public const string PathFailed = "human-executor.path-failed";
    public const string ProtectedTargetRejected = "human-executor.protected-target-rejected";
    public const string QuestCriticalRejected = "human-executor.quest-critical-rejected";
    public const string StaleLease = "human-executor.stale-lease";
    public const string StopAll = "human-executor.stop-all";
    public const string StuckRecoveryBounded = "human-executor.stuck-recovery-bounded";
    public const string TargetAssigned = "human-executor.target-assigned";
    public const string TargetCleared = "human-executor.target-cleared";
    public const string TargetDestroyed = "human-executor.target-destroyed";
    public const string TargetIdMissing = "human-executor.target-id-missing";
    public const string TargetInvalid = "human-executor.target-invalid";
    public const string TargetMissing = "human-executor.target-missing";
    public const string TargetValid = "human-executor.target-valid";
    public const string WrongActor = "human-executor.wrong-actor";
    public const string WrongTarget = "human-executor.wrong-target";
}

public enum HumanExecutionStatus
{
    Accepted,
    Running,
    Succeeded,
    Failed,
    Interrupted,
    Rejected,
    TimedOut,
}

public enum HumanActorSource
{
    AvalonManagedOneSession,
    ExistingNativeActor,
}

public enum HumanActorRole
{
    Follower,
    Defender,
    Melee,
    Ranged,
    Support,
    Patrol,
    Guard,
    Hunter,
    Boss,
}

public enum HumanExecutorMode
{
    Follow,
    Hold,
    Defend,
    ComeClose,
    Recall,
    PartWays,
}

public enum HumanTargetKind
{
    None,
    Friendly,
    Hostile,
    Neutral,
    Player,
    Companion,
    Protected,
}

public enum HumanPathState
{
    Unknown,
    Clear,
    Pending,
    Failed,
    Stuck,
    Unreachable,
}

public enum HumanNativeState
{
    Unknown,
    Idle,
    Alert,
    Combat,
    KnockedOut,
    Dead,
    Suspended,
}

public enum HumanSuspensionReason
{
    None,
    Menu,
    Loading,
    Transition,
    KillSwitch,
}

public enum HumanActionKind
{
    Idle,
    Follow,
    HoldPosition,
    MoveToPosition,
    ApproachTarget,
    FaceTarget,
    Strafe,
    BackAway,
    Retreat,
    Regroup,
    Stop,
    Engage,
    Disengage,
    MeleeAttack,
    RangedAttack,
    Block,
    Parry,
    Dodge,
    Interrupt,
    Reposition,
    Pursue,
    ComeClose,
    Recall,
    PartWays,
    DefendPosition,
    DefendActor,
    Telegraph,
    AttackSequence,
    PhaseTransition,
    Enrage,
    SummonRequest,
    StopAll,
}
