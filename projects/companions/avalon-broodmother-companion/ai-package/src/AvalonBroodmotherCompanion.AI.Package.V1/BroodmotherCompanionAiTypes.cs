using System;

namespace AvalonBroodmotherCompanion.AI.Package.V1;

public sealed class BroodmotherSpiderCompanionAiInput
{
    public bool AuthorityValid { get; set; } = true;

    public string OwnerId { get; set; } = BroodmotherCompanionAiV1Contract.RequiredOwnerId;

    public string ActorRoleId { get; set; } = BroodmotherCompanionAiV1Contract.BroodmotherActorRoleIdValue;

    public string LocationTemplateName { get; set; } = BroodmotherCompanionAiV1Contract.TargetTemplateName;

    public string LocationTemplateGuid { get; set; } = BroodmotherCompanionAiV1Contract.TargetTemplateGuid;

    public string NpcTemplateGuid { get; set; } = BroodmotherCompanionAiV1Contract.TargetNpcTemplateGuid;

    public bool TargetAlive { get; set; } = true;

    public bool TargetVisible { get; set; } = true;

    public bool TargetSpatialAvailable { get; set; } = true;

    public int LiveHeroAttackerCount { get; set; } = 1;

    public float HealthFraction { get; set; } = 1f;

    public int RecentHits { get; set; }

    public float TargetHealthFraction { get; set; } = 1f;

    public int PackIndex { get; set; }

    public int WaveSeed { get; set; }

    public float TargetDistanceMeters { get; set; }

    public float TargetAngleDegrees { get; set; }

    public int ActiveAttackers { get; set; }

    public int MaxSimultaneousAttackers { get; set; } = BroodmotherCompanionAiV1Contract.MaxAttackSlotsDefault;

    public bool AttackCooldownActive { get; set; }

    public bool RecoveryActive { get; set; }

    public bool AttackAlreadyCommitted { get; set; }

    public bool TelegraphAlreadyEmitted { get; set; }

    public bool ReleaseWindowOpen { get; set; }

    public bool LeapRequestsEnabled { get; set; } = true;

    public bool PreferAlternateShortRange { get; set; }

    public bool PreferAlternateJumpMotion { get; set; }

    public float LocalDensity { get; set; }

    public bool CameraDataAvailable { get; set; }

    public float CameraCenterWeight { get; set; }
}

public enum BroodmotherSpiderRole
{
    Harasser = 0,
    Flanker = 1,
    Ambusher = 2,
    Retreating = 3,
    Finisher = 4,
}

public enum BroodmotherSpiderAttackKind
{
    Hold = 0,
    ShortBite = 1,
    LeapJump = 2,
    Feint = 3,
    Reposition = 4,
    TargetedNativeCombat = 5,
}

public enum BroodmotherSpiderAiDecisionKind
{
    None = 0,
    Hold = 1,
    TelegraphIntent = 2,
    NativeCombatHandoff = 3,
    ShortRange16Request = 4,
    Reposition = 5,
    FailClosed = 6,
}

public sealed class BroodmotherSpiderPositionSelection
{
    public BroodmotherSpiderPositionSelection(
        float desiredAngleDegrees,
        float desiredDistanceMeters,
        bool avoidsStacking,
        bool avoidsCameraCenter,
        string reason)
    {
        DesiredAngleDegrees = desiredAngleDegrees;
        DesiredDistanceMeters = desiredDistanceMeters;
        AvoidsStacking = avoidsStacking;
        AvoidsCameraCenter = avoidsCameraCenter;
        Reason = NormalizeReason(reason);
    }

    public float DesiredAngleDegrees { get; }

    public float DesiredDistanceMeters { get; }

    public bool AvoidsStacking { get; }

    public bool AvoidsCameraCenter { get; }

    public string Reason { get; }

    private static string NormalizeReason(string reason)
    {
        return string.IsNullOrWhiteSpace(reason) ? "none" : reason.Trim();
    }
}

public sealed class BroodmotherSpiderAttackSlotDecision
{
    public BroodmotherSpiderAttackSlotDecision(
        bool canAttack,
        int maxSimultaneousAttackers,
        int waveSlot,
        string reason)
    {
        CanAttack = canAttack;
        MaxSimultaneousAttackers = maxSimultaneousAttackers;
        WaveSlot = waveSlot;
        Reason = string.IsNullOrWhiteSpace(reason) ? "none" : reason.Trim();
    }

    public bool CanAttack { get; }

    public int MaxSimultaneousAttackers { get; }

    public int WaveSlot { get; }

    public string Reason { get; }
}

public sealed class BroodmotherSpiderAttackSelection
{
    private BroodmotherSpiderAttackSelection(BroodmotherSpiderAttackKind kind, string reason)
    {
        Kind = kind;
        Reason = string.IsNullOrWhiteSpace(reason) ? "none" : reason.Trim();
    }

    public BroodmotherSpiderAttackKind Kind { get; }

    public string Reason { get; }

    public static BroodmotherSpiderAttackSelection Chosen(
        BroodmotherSpiderAttackKind kind,
        string reason)
    {
        return new BroodmotherSpiderAttackSelection(kind, reason);
    }
}

public sealed class BroodmotherSpiderAiDecision
{
    private BroodmotherSpiderAiDecision(BroodmotherSpiderAiDecisionKind kind, string reason)
    {
        Kind = kind;
        Reason = string.IsNullOrWhiteSpace(reason) ? "none" : reason.Trim();
    }

    public BroodmotherSpiderAiDecisionKind Kind { get; }

    public string Reason { get; }

    public bool DrivesNativeCommand =>
        Kind == BroodmotherSpiderAiDecisionKind.NativeCombatHandoff ||
        Kind == BroodmotherSpiderAiDecisionKind.ShortRange16Request;

    public static BroodmotherSpiderAiDecision None(string reason)
    {
        return new BroodmotherSpiderAiDecision(BroodmotherSpiderAiDecisionKind.None, reason);
    }

    public static BroodmotherSpiderAiDecision Hold(string reason)
    {
        return new BroodmotherSpiderAiDecision(BroodmotherSpiderAiDecisionKind.Hold, reason);
    }

    public static BroodmotherSpiderAiDecision Telegraph(string reason)
    {
        return new BroodmotherSpiderAiDecision(BroodmotherSpiderAiDecisionKind.TelegraphIntent, reason);
    }

    public static BroodmotherSpiderAiDecision NativeCombatHandoff(string reason)
    {
        return new BroodmotherSpiderAiDecision(BroodmotherSpiderAiDecisionKind.NativeCombatHandoff, reason);
    }

    public static BroodmotherSpiderAiDecision ShortRange16(string reason)
    {
        return new BroodmotherSpiderAiDecision(BroodmotherSpiderAiDecisionKind.ShortRange16Request, reason);
    }

    public static BroodmotherSpiderAiDecision Reposition(string reason)
    {
        return new BroodmotherSpiderAiDecision(BroodmotherSpiderAiDecisionKind.Reposition, reason);
    }

    public static BroodmotherSpiderAiDecision FailClosed(string reason)
    {
        return new BroodmotherSpiderAiDecision(BroodmotherSpiderAiDecisionKind.FailClosed, reason);
    }
}

public sealed class BroodmotherSpiderCompanionAiEvaluation
{
    public BroodmotherSpiderCompanionAiEvaluation(
        BroodmotherSpiderRole role,
        BroodmotherSpiderPositionSelection position,
        BroodmotherSpiderAttackSlotDecision attackSlot,
        BroodmotherSpiderAttackSelection attack,
        BroodmotherSpiderAiDecision decision,
        string shortRangeClip,
        string jumpMotionClip,
        string jumpMotionPolicy)
    {
        Role = role;
        Position = position ?? throw new ArgumentNullException(nameof(position));
        AttackSlot = attackSlot ?? throw new ArgumentNullException(nameof(attackSlot));
        Attack = attack ?? throw new ArgumentNullException(nameof(attack));
        Decision = decision ?? throw new ArgumentNullException(nameof(decision));
        ShortRangeClip = string.IsNullOrWhiteSpace(shortRangeClip) ? "none" : shortRangeClip.Trim();
        JumpMotionClip = string.IsNullOrWhiteSpace(jumpMotionClip) ? "none" : jumpMotionClip.Trim();
        JumpMotionPolicy = string.IsNullOrWhiteSpace(jumpMotionPolicy) ? "none" : jumpMotionPolicy.Trim();
    }

    public BroodmotherSpiderRole Role { get; }

    public BroodmotherSpiderPositionSelection Position { get; }

    public BroodmotherSpiderAttackSlotDecision AttackSlot { get; }

    public BroodmotherSpiderAttackSelection Attack { get; }

    public BroodmotherSpiderAiDecision Decision { get; }

    public string ShortRangeClip { get; }

    public string JumpMotionClip { get; }

    public string JumpMotionPolicy { get; }

    public static BroodmotherSpiderCompanionAiEvaluation Stopped(BroodmotherSpiderAiDecision reason)
    {
        reason = reason ?? throw new ArgumentNullException(nameof(reason));
        return new BroodmotherSpiderCompanionAiEvaluation(
            BroodmotherSpiderRole.Harasser,
            new BroodmotherSpiderPositionSelection(0f, 0f, true, false, reason.Reason),
            new BroodmotherSpiderAttackSlotDecision(false, BroodmotherCompanionAiV1Contract.MaxAttackSlotsDefault, 0, reason.Reason),
            BroodmotherSpiderAttackSelection.Chosen(BroodmotherSpiderAttackKind.Hold, reason.Reason),
            reason,
            BroodmotherCompanionAiV1Contract.ShortRangePrimaryClipName,
            BroodmotherCompanionAiV1Contract.JumpClipName,
            BroodmotherCompanionAiV1Contract.JumpMotionPolicy);
    }

    public static BroodmotherSpiderCompanionAiEvaluation FailClosed(BroodmotherSpiderAiDecision reason)
    {
        return Stopped(reason);
    }
}
