using System;
using AvalonAI.Contracts;

namespace AvalonCompanions.AI.Package;

public sealed class AvalonCompanionAdvancedAiPackage : IAvalonAiEvaluatingPackage
{
    public const string PackageId = "kane.tgfoa.avalon-companions.ai-advanced-companion";
    public const string PackageVersion = "0.1.0";
    public const string PackageMarkerString = "AVALON_COMPANIONS_ADVANCED_AI_PACKAGE_PASS";

    public AvalonCompanionAdvancedAiPackage()
    {
        Manifest = new AvalonAiPackageManifest(
            PackageId,
            "Avalon Companions Advanced Companion AI",
            PackageVersion,
            new AvalonAiApiVersion(1, 1));
    }

    public AvalonAiPackageManifest Manifest { get; }

    public IAvalonAiCommandProposal? Evaluate(IAvalonAiObservation observation)
    {
        if (!(observation is AdvancedCompanionObservation companionObservation))
        {
            return null;
        }

        AdvancedCompanionDecision decision = AdvancedCompanionDecisionPolicy.Evaluate(companionObservation);
        if (decision.ProposalKind == AdvancedCompanionProposalKind.NoProposal)
        {
            return null;
        }

        return new AdvancedCompanionCommandProposal(companionObservation.ActorId, companionObservation.CurrentTargetId, decision);
    }
}

public static class AdvancedCompanionDecisionPolicy
{
    private const int WaryScoreMaximum = 34;
    private const double CriticalHealthFraction = 0.25d;
    private const double CatchUpRecallMultiplier = 1.5d;

    public static AdvancedCompanionDecision Evaluate(AdvancedCompanionObservation? observation)
    {
        if (observation == null)
        {
            return AdvancedCompanionDecision.NoProposal(
                AdvancedCompanionGoal.NoGoal,
                "advanced-companion.observation-missing");
        }

        if (observation.KillSwitchActive)
        {
            return AdvancedCompanionDecision.NoProposal(
                AdvancedCompanionGoal.NoGoal,
                "advanced-companion.kill-switch-active");
        }

        if (string.IsNullOrWhiteSpace(observation.ActorId))
        {
            return AdvancedCompanionDecision.NoProposal(
                AdvancedCompanionGoal.NoGoal,
                "advanced-companion.actor-id-missing");
        }

        if (observation.ActorLane == AdvancedCompanionActorLane.Unmanaged ||
            !observation.IsManaged && observation.ActorLane != AdvancedCompanionActorLane.WildAnimal)
        {
            return AdvancedCompanionDecision.NoProposal(
                AdvancedCompanionGoal.NoGoal,
                "advanced-companion.unmanaged-actor");
        }

        if (observation.ActorLane == AdvancedCompanionActorLane.WildAnimal)
        {
            return EvaluateWildTameReadiness(observation);
        }

        if (!observation.OwnershipValid)
        {
            return AdvancedCompanionDecision.NoProposal(
                AdvancedCompanionGoal.NoGoal,
                "advanced-companion.owner-invalid");
        }

        if (observation.IsUndeadOrBlocked || observation.IsHostile || observation.IsPassiveOnly)
        {
            return AdvancedCompanionDecision.NoProposal(
                AdvancedCompanionGoal.NoGoal,
                "advanced-companion.managed-lane-blocked-classification");
        }

        if (!observation.Alive || observation.Unconscious)
        {
            return AdvancedCompanionDecision.NoProposal(
                AdvancedCompanionGoal.NoGoal,
                "advanced-companion.actor-not-actionable");
        }

        if (observation.MovementBlocked || observation.MovementStuck)
        {
            return observation.RecoverCooldownReady
                ? AdvancedCompanionDecision.Propose(
                    AdvancedCompanionGoal.RecoverWhenBlocked,
                    AdvancedCompanionProposalKind.RecoverStuck,
                    "advanced-companion.recover-stuck",
                    priority: 85,
                    observation.EffectiveCatchUpDistanceMeters)
                : AdvancedCompanionDecision.NoProposal(
                    AdvancedCompanionGoal.RecoverWhenBlocked,
                    "advanced-companion.recover-cooldown-blocked");
        }

        if (observation.HealthFraction <= CriticalHealthFraction ||
            observation.NativeState == AdvancedCompanionNativeState.Flee)
        {
            return AdvancedCompanionDecision.Propose(
                AdvancedCompanionGoal.AvoidDeath,
                AdvancedCompanionProposalKind.RetreatOrStandDown,
                "advanced-companion.critical-health-retreat",
                priority: 95,
                observation.EffectiveCatchUpDistanceMeters);
        }

        if (observation.CommandMode == AdvancedCompanionCommandMode.Hold)
        {
            return AdvancedCompanionDecision.Propose(
                AdvancedCompanionGoal.HoldAssignedPosition,
                AdvancedCompanionProposalKind.HoldAnchor,
                "advanced-companion.hold-anchor",
                priority: 70,
                observation.EffectiveCatchUpDistanceMeters);
        }

        if (IsLowTrust(observation))
        {
            if (observation.CommandMode == AdvancedCompanionCommandMode.Follow &&
                IsSeparatedForRecall(observation) &&
                observation.CatchUpCooldownReady)
            {
                return AdvancedCompanionDecision.Propose(
                    AdvancedCompanionGoal.StayWithPlayer,
                    AdvancedCompanionProposalKind.CatchUpRecall,
                    "advanced-companion.low-trust-basic-follow-catch-up",
                    priority: 55,
                    observation.EffectiveCatchUpDistanceMeters);
            }

            return AdvancedCompanionDecision.NoProposal(
                AdvancedCompanionGoal.RespectLowTrust,
                "advanced-companion.low-trust-refuses-advanced-command");
        }

        if (observation.CommandMode == AdvancedCompanionCommandMode.Follow &&
            IsSeparatedForRecall(observation))
        {
            return observation.CatchUpCooldownReady
                ? AdvancedCompanionDecision.Propose(
                    AdvancedCompanionGoal.RegroupWhenSeparated,
                    AdvancedCompanionProposalKind.CatchUpRecall,
                    "advanced-companion.catch-up-recall",
                    priority: GetBondPriority(observation, 75),
                    observation.EffectiveCatchUpDistanceMeters)
                : AdvancedCompanionDecision.NoProposal(
                    AdvancedCompanionGoal.RegroupWhenSeparated,
                    "advanced-companion.catch-up-cooldown-blocked");
        }

        if (observation.CommandMode == AdvancedCompanionCommandMode.Follow &&
            IsSeparatedForRegroup(observation))
        {
            return observation.CatchUpCooldownReady
                ? AdvancedCompanionDecision.Propose(
                    AdvancedCompanionGoal.RegroupWhenSeparated,
                    AdvancedCompanionProposalKind.RegroupNearPlayer,
                    "advanced-companion.regroup-near-player",
                    priority: GetBondPriority(observation, 65),
                    observation.EffectiveCatchUpDistanceMeters)
                : AdvancedCompanionDecision.NoProposal(
                    AdvancedCompanionGoal.RegroupWhenSeparated,
                    "advanced-companion.regroup-cooldown-blocked");
        }

        if (PlayerThreatened(observation))
        {
            if (!observation.DefendCooldownReady)
            {
                return AdvancedCompanionDecision.NoProposal(
                    AdvancedCompanionGoal.ProtectPlayer,
                    "advanced-companion.defend-cooldown-blocked");
            }

            if (IsLowLoyalty(observation))
            {
                return AdvancedCompanionDecision.NoProposal(
                    AdvancedCompanionGoal.RespectLowTrust,
                    "advanced-companion.low-loyalty-refuses-risky-defend");
            }

            if (observation.CommandMode == AdvancedCompanionCommandMode.Defend ||
                AllowsAutomaticDefend(observation))
            {
                return AdvancedCompanionDecision.Propose(
                    observation.VisibleAttackersCount > 1
                        ? AdvancedCompanionGoal.InterceptImmediateThreat
                        : AdvancedCompanionGoal.ProtectPlayer,
                    AdvancedCompanionProposalKind.NativeDefendPrompt,
                    "advanced-companion.native-defend-prompt",
                    priority: GetBondPriority(observation, 80),
                    observation.EffectiveCatchUpDistanceMeters);
            }

            return AdvancedCompanionDecision.NoProposal(
                AdvancedCompanionGoal.ProtectPlayer,
                "advanced-companion.defend-not-authorized-by-mode-or-bond");
        }

        return AdvancedCompanionDecision.NoProposal(
            observation.CommandMode == AdvancedCompanionCommandMode.Follow
                ? AdvancedCompanionGoal.StayWithPlayer
                : AdvancedCompanionGoal.NoGoal,
            "advanced-companion.no-actionable-state");
    }

    private static AdvancedCompanionDecision EvaluateWildTameReadiness(AdvancedCompanionObservation observation)
    {
        if (!observation.TamePromptCooldownReady)
        {
            return AdvancedCompanionDecision.NoProposal(
                AdvancedCompanionGoal.TameReadinessOnly,
                "advanced-companion.tame-prompt-cooldown-blocked");
        }

        if (observation.IsUndeadOrBlocked || observation.IsHostile || observation.IsPassiveOnly || !observation.IsTameCandidate)
        {
            return AdvancedCompanionDecision.Propose(
                AdvancedCompanionGoal.TameReadinessOnly,
                AdvancedCompanionProposalKind.TameCandidateUnsafe,
                "advanced-companion.tame-candidate-unsafe",
                priority: 20,
                observation.EffectiveCatchUpDistanceMeters);
        }

        if (!observation.Alive || observation.Unconscious)
        {
            return AdvancedCompanionDecision.Propose(
                AdvancedCompanionGoal.TameReadinessOnly,
                AdvancedCompanionProposalKind.TameCandidateUnsafe,
                "advanced-companion.tame-candidate-not-actionable",
                priority: 20,
                observation.EffectiveCatchUpDistanceMeters);
        }

        return AdvancedCompanionDecision.Propose(
            AdvancedCompanionGoal.TameReadinessOnly,
            AdvancedCompanionProposalKind.TameCandidateReady,
            "advanced-companion.tame-candidate-ready",
            priority: 35,
            observation.EffectiveCatchUpDistanceMeters);
    }

    private static bool IsLowTrust(AdvancedCompanionObservation observation)
    {
        return observation.Trust <= WaryScoreMaximum;
    }

    private static bool IsLowLoyalty(AdvancedCompanionObservation observation)
    {
        return observation.Loyalty <= WaryScoreMaximum;
    }

    private static bool IsSeparatedForRecall(AdvancedCompanionObservation observation)
    {
        return observation.DistanceToPlayerMeters >= observation.EffectiveCatchUpDistanceMeters * CatchUpRecallMultiplier;
    }

    private static bool IsSeparatedForRegroup(AdvancedCompanionObservation observation)
    {
        return observation.DistanceToPlayerMeters >= observation.EffectiveCatchUpDistanceMeters;
    }

    private static bool PlayerThreatened(AdvancedCompanionObservation observation)
    {
        return observation.CurrentTargetThreatensPlayer || observation.VisibleAttackersCount > 0;
    }

    private static bool AllowsAutomaticDefend(AdvancedCompanionObservation observation)
    {
        return observation.BondLevel == AdvancedCompanionBondLevel.Trusted ||
            observation.BondLevel == AdvancedCompanionBondLevel.Loyal;
    }

    private static int GetBondPriority(AdvancedCompanionObservation observation, int baseline)
    {
        return observation.BondLevel switch
        {
            AdvancedCompanionBondLevel.Loyal => baseline + 15,
            AdvancedCompanionBondLevel.Trusted => baseline + 8,
            AdvancedCompanionBondLevel.Wary => baseline - 15,
            _ => baseline,
        };
    }
}

public sealed class AdvancedCompanionObservation : IAvalonAiObservation
{
    private const double CloseCatchUpDistanceMeters = 14d;
    private const double PaceCatchUpDistanceMeters = 22d;
    private const double FarCatchUpDistanceMeters = 34d;

    public AdvancedCompanionObservation(
        string actorId,
        string templateGuid,
        string templateName,
        string scene,
        AdvancedCompanionPoint actorPosition,
        AdvancedCompanionPoint playerPosition,
        double distanceToPlayerMeters,
        double healthFraction,
        bool alive,
        bool unconscious,
        AdvancedCompanionCommandMode commandMode,
        AdvancedCompanionFollowRange followRange,
        int trust,
        int loyalty,
        AdvancedCompanionBondLevel bondLevel,
        AdvancedCompanionNativeState nativeState,
        string currentTargetId,
        bool currentTargetThreatensPlayer,
        int visibleAttackersCount,
        bool movementBlocked,
        bool movementStuck,
        AdvancedCompanionActorLane actorLane,
        bool isManaged,
        bool isTameCandidate,
        bool isHostile,
        bool isUndeadOrBlocked,
        bool isPassiveOnly,
        bool defendCooldownReady,
        bool catchUpCooldownReady,
        bool recoverCooldownReady,
        bool tamePromptCooldownReady,
        bool ownershipValid,
        bool killSwitchActive)
    {
        ActorId = actorId ?? string.Empty;
        TemplateGuid = templateGuid ?? string.Empty;
        TemplateName = templateName ?? string.Empty;
        Scene = scene ?? string.Empty;
        ActorPosition = actorPosition;
        PlayerPosition = playerPosition;
        DistanceToPlayerMeters = Math.Max(0d, distanceToPlayerMeters);
        HealthFraction = Clamp01(healthFraction);
        Alive = alive;
        Unconscious = unconscious;
        CommandMode = commandMode;
        FollowRange = followRange;
        Trust = ClampScore(trust);
        Loyalty = ClampScore(loyalty);
        BondLevel = bondLevel;
        NativeState = nativeState;
        CurrentTargetId = currentTargetId ?? string.Empty;
        CurrentTargetThreatensPlayer = currentTargetThreatensPlayer;
        VisibleAttackersCount = Math.Max(0, visibleAttackersCount);
        MovementBlocked = movementBlocked;
        MovementStuck = movementStuck;
        ActorLane = actorLane;
        IsManaged = isManaged;
        IsTameCandidate = isTameCandidate;
        IsHostile = isHostile;
        IsUndeadOrBlocked = isUndeadOrBlocked;
        IsPassiveOnly = isPassiveOnly;
        DefendCooldownReady = defendCooldownReady;
        CatchUpCooldownReady = catchUpCooldownReady;
        RecoverCooldownReady = recoverCooldownReady;
        TamePromptCooldownReady = tamePromptCooldownReady;
        OwnershipValid = ownershipValid;
        KillSwitchActive = killSwitchActive;
        EffectiveCatchUpDistanceMeters = GetEffectiveCatchUpDistance(followRange, bondLevel);
    }

    public string ActorId { get; }

    public string TemplateGuid { get; }

    public string TemplateName { get; }

    public string Scene { get; }

    public AdvancedCompanionPoint ActorPosition { get; }

    public AdvancedCompanionPoint PlayerPosition { get; }

    public double DistanceToPlayerMeters { get; }

    public double HealthFraction { get; }

    public bool Alive { get; }

    public bool Unconscious { get; }

    public AdvancedCompanionCommandMode CommandMode { get; }

    public AdvancedCompanionFollowRange FollowRange { get; }

    public int Trust { get; }

    public int Loyalty { get; }

    public AdvancedCompanionBondLevel BondLevel { get; }

    public AdvancedCompanionNativeState NativeState { get; }

    public string CurrentTargetId { get; }

    public bool CurrentTargetThreatensPlayer { get; }

    public int VisibleAttackersCount { get; }

    public bool MovementBlocked { get; }

    public bool MovementStuck { get; }

    public AdvancedCompanionActorLane ActorLane { get; }

    public bool IsManaged { get; }

    public bool IsTameCandidate { get; }

    public bool IsHostile { get; }

    public bool IsUndeadOrBlocked { get; }

    public bool IsPassiveOnly { get; }

    public bool DefendCooldownReady { get; }

    public bool CatchUpCooldownReady { get; }

    public bool RecoverCooldownReady { get; }

    public bool TamePromptCooldownReady { get; }

    public bool OwnershipValid { get; }

    public bool KillSwitchActive { get; }

    public double EffectiveCatchUpDistanceMeters { get; }

    private static double GetEffectiveCatchUpDistance(
        AdvancedCompanionFollowRange followRange,
        AdvancedCompanionBondLevel bondLevel)
    {
        double baseDistance = followRange switch
        {
            AdvancedCompanionFollowRange.Close => CloseCatchUpDistanceMeters,
            AdvancedCompanionFollowRange.Far => FarCatchUpDistanceMeters,
            _ => PaceCatchUpDistanceMeters,
        };

        double multiplier = bondLevel switch
        {
            AdvancedCompanionBondLevel.Wary => 1.2d,
            AdvancedCompanionBondLevel.Trusted => 0.9d,
            AdvancedCompanionBondLevel.Loyal => 0.8d,
            _ => 1d,
        };

        return baseDistance * multiplier;
    }

    private static double Clamp01(double value)
    {
        if (value < 0d)
        {
            return 0d;
        }

        return value > 1d ? 1d : value;
    }

    private static int ClampScore(int value)
    {
        if (value < 0)
        {
            return 0;
        }

        return value > 100 ? 100 : value;
    }
}

public sealed class AdvancedCompanionCommandProposal : IAvalonAiCommandProposal
{
    internal AdvancedCompanionCommandProposal(
        string actorId,
        string targetId,
        AdvancedCompanionDecision decision)
    {
        ActorId = actorId ?? string.Empty;
        TargetId = targetId ?? string.Empty;
        Goal = decision.Goal;
        Kind = decision.ProposalKind;
        ReasonCode = decision.ReasonCode;
        Priority = decision.Priority;
        EffectiveCatchUpDistanceMeters = decision.EffectiveCatchUpDistanceMeters;
    }

    public string ActorId { get; }

    public string TargetId { get; }

    public AdvancedCompanionGoal Goal { get; }

    public AdvancedCompanionProposalKind Kind { get; }

    public string ReasonCode { get; }

    public int Priority { get; }

    public double EffectiveCatchUpDistanceMeters { get; }

    public bool DecisionProposalOnly => true;

    public bool DirectGameCalls => false;

    public bool UnityDependency => false;

    public bool FoADependency => false;

    public bool GameLoaderDependency => false;

    public bool OwnsTameSummonDismissPersistenceOrCommandUi => false;

    public bool MutatesWildActors => false;

    public bool SelectsNativeTargets => false;

    public bool OverridesFaction => false;

    public bool OverridesMovement => false;

    public bool WritesPersistence => false;
}

public readonly struct AdvancedCompanionDecision
{
    private AdvancedCompanionDecision(
        AdvancedCompanionGoal goal,
        AdvancedCompanionProposalKind proposalKind,
        string reasonCode,
        int priority,
        double effectiveCatchUpDistanceMeters)
    {
        Goal = goal;
        ProposalKind = proposalKind;
        ReasonCode = reasonCode ?? string.Empty;
        Priority = priority;
        EffectiveCatchUpDistanceMeters = effectiveCatchUpDistanceMeters;
    }

    public AdvancedCompanionGoal Goal { get; }

    public AdvancedCompanionProposalKind ProposalKind { get; }

    public string ReasonCode { get; }

    public int Priority { get; }

    public double EffectiveCatchUpDistanceMeters { get; }

    public bool ProducesProposal => ProposalKind != AdvancedCompanionProposalKind.NoProposal;

    public static AdvancedCompanionDecision Propose(
        AdvancedCompanionGoal goal,
        AdvancedCompanionProposalKind proposalKind,
        string reasonCode,
        int priority,
        double effectiveCatchUpDistanceMeters)
    {
        return new AdvancedCompanionDecision(goal, proposalKind, reasonCode, priority, effectiveCatchUpDistanceMeters);
    }

    public static AdvancedCompanionDecision NoProposal(
        AdvancedCompanionGoal goal,
        string reasonCode)
    {
        return new AdvancedCompanionDecision(goal, AdvancedCompanionProposalKind.NoProposal, reasonCode, 0, 0d);
    }
}

public readonly struct AdvancedCompanionPoint
{
    public AdvancedCompanionPoint(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public double X { get; }

    public double Y { get; }

    public double Z { get; }
}

public enum AdvancedCompanionActorLane
{
    ManagedAnimalCompanion,
    WildAnimal,
    Unmanaged,
}

public enum AdvancedCompanionCommandMode
{
    Follow,
    Hold,
    Defend,
}

public enum AdvancedCompanionFollowRange
{
    Close,
    Pace,
    Far,
}

public enum AdvancedCompanionBondLevel
{
    Wary,
    Familiar,
    Trusted,
    Loyal,
}

public enum AdvancedCompanionNativeState
{
    Idle,
    Alert,
    Combat,
    Flee,
    Unknown,
}

public enum AdvancedCompanionGoal
{
    NoGoal,
    StayWithPlayer,
    HoldAssignedPosition,
    ProtectPlayer,
    InterceptImmediateThreat,
    RegroupWhenSeparated,
    RecoverWhenBlocked,
    AvoidDeath,
    RespectLowTrust,
    TameReadinessOnly,
}

public enum AdvancedCompanionProposalKind
{
    NoProposal,
    CatchUpRecall,
    NativeDefendPrompt,
    HoldAnchor,
    RegroupNearPlayer,
    RecoverStuck,
    RetreatOrStandDown,
    TameCandidateReady,
    TameCandidateUnsafe,
}
