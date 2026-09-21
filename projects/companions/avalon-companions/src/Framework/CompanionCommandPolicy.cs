namespace AvalonCompanions.Framework;

internal static class CompanionCommandPolicy
{
    internal static CompanionCommandPolicyDecision Evaluate(
        CompanionCoreRuntimeProfile profile,
        CompanionCommandPolicyAction action)
    {
        if (!profile.RuntimeApproved)
        {
            return CompanionCommandPolicyDecision.Block(
                action,
                $"profile not runtime approved; review={profile.ReviewId}");
        }

        switch (action)
        {
            case CompanionCommandPolicyAction.Select:
            case CompanionCommandPolicyAction.SummonSwap:
            case CompanionCommandPolicyAction.DialogueOpen:
            case CompanionCommandPolicyAction.LifecycleCheck:
                return CompanionCommandPolicyDecision.Allow(action, "profile action is metadata/runtime surface only");
            case CompanionCommandPolicyAction.Follow:
                return RequireCapability(profile, action, CompanionRuntimeCommandCapabilities.Follow);
            case CompanionCommandPolicyAction.Stay:
                return RequireCapability(profile, action, CompanionRuntimeCommandCapabilities.Stay);
            case CompanionCommandPolicyAction.Range:
                return RequireCapability(profile, action, CompanionRuntimeCommandCapabilities.Range);
            case CompanionCommandPolicyAction.ComeClose:
                return RequireCapabilities(
                    profile,
                    action,
                    CompanionRuntimeCommandCapabilities.Follow
                    | CompanionRuntimeCommandCapabilities.Range
                    | CompanionRuntimeCommandCapabilities.Recall);
            case CompanionCommandPolicyAction.Recall:
                return RequireCapability(profile, action, CompanionRuntimeCommandCapabilities.Recall);
            case CompanionCommandPolicyAction.Recover:
                return RequireCapability(profile, action, CompanionRuntimeCommandCapabilities.Recover);
            case CompanionCommandPolicyAction.Dismiss:
                return RequireCapability(profile, action, CompanionRuntimeCommandCapabilities.Dismiss);
            case CompanionCommandPolicyAction.Defend:
            case CompanionCommandPolicyAction.NativeDefendPrompt:
                return RequireNativeDefend(profile, action);
            default:
                return CompanionCommandPolicyDecision.Block(action, "unknown command policy action");
        }
    }

    private static CompanionCommandPolicyDecision RequireCapability(
        CompanionCoreRuntimeProfile profile,
        CompanionCommandPolicyAction action,
        CompanionRuntimeCommandCapabilities capability)
    {
        return (profile.CommandCapabilities & capability) != 0
            ? CompanionCommandPolicyDecision.Allow(action, $"capability allowed: {capability}")
            : CompanionCommandPolicyDecision.Block(action, $"missing capability: {capability}");
    }

    private static CompanionCommandPolicyDecision RequireCapabilities(
        CompanionCoreRuntimeProfile profile,
        CompanionCommandPolicyAction action,
        CompanionRuntimeCommandCapabilities capabilities)
    {
        return (profile.CommandCapabilities & capabilities) == capabilities
            ? CompanionCommandPolicyDecision.Allow(action, $"capabilities allowed: {capabilities}")
            : CompanionCommandPolicyDecision.Block(action, $"missing one or more capabilities: {capabilities}");
    }

    private static CompanionCommandPolicyDecision RequireNativeDefend(
        CompanionCoreRuntimeProfile profile,
        CompanionCommandPolicyAction action)
    {
        if ((profile.CommandCapabilities & CompanionRuntimeCommandCapabilities.NativeDefendPrompt) == 0)
        {
            return CompanionCommandPolicyDecision.Block(
                action,
                "passive companion profile has no reliable native defend prompt");
        }

        return CompanionCommandPolicyDecision.Allow(
            action,
            $"native defend prompt allowed for role={profile.CombatRole}");
    }
}

internal enum CompanionCommandPolicyAction
{
    Select,
    SummonSwap,
    DialogueOpen,
    Follow,
    Stay,
    Defend,
    Range,
    ComeClose,
    Recall,
    Recover,
    Dismiss,
    NativeDefendPrompt,
    LifecycleCheck,
}

internal readonly struct CompanionCommandPolicyDecision
{
    private CompanionCommandPolicyDecision(CompanionCommandPolicyAction action, bool allowed, string reason)
    {
        Action = action;
        Allowed = allowed;
        Reason = reason;
    }

    internal CompanionCommandPolicyAction Action { get; }

    internal bool Allowed { get; }

    internal string Reason { get; }

    internal static CompanionCommandPolicyDecision Allow(CompanionCommandPolicyAction action, string reason)
    {
        return new CompanionCommandPolicyDecision(action, allowed: true, reason);
    }

    internal static CompanionCommandPolicyDecision Block(CompanionCommandPolicyAction action, string reason)
    {
        return new CompanionCommandPolicyDecision(action, allowed: false, reason);
    }
}
