using AvalonAI.Contracts.V2;

namespace AvalonCompanions.AI.Package.V2;

public static class AvalonCompanionCatchUpV2Contract
{
    public const string PackageIdValue = "kane.tgfoa.avalon-companions.ai-intent-profile";
    public const string GoalIdValue = "avalon-companions.catch-up";
    public const string ActionIdValue = "avalon-companions.catch-up-recall";
    public const string CapabilityValue = "native-companion-catch-up-recall";
    public const string ActorRoleIdValue = "avalon-companions.managed-companion";
    public const string CatchUpRequiredFactIdValue = "avalon-companions.catch-up-required";
    public const string CatchUpRecallDispatchedFactIdValue = "avalon-companions.catch-up-recall-dispatched";
    public const string GoalReasonCode = "avalon-companions-follow-catch-up-candidate";

    public static PackageId PackageId { get; } = new PackageId(PackageIdValue);

    public static GoalId GoalId { get; } = new GoalId(GoalIdValue);

    public static ActionId ActionId { get; } = new ActionId(ActionIdValue);

    public static ActionCapability Capability { get; } = new ActionCapability(CapabilityValue);

    public static ActorRoleId ActorRoleId { get; } = new ActorRoleId(ActorRoleIdValue);

    public static PlanningFactId CatchUpRequiredFactId { get; } = new PlanningFactId(CatchUpRequiredFactIdValue);

    public static PlanningFactId CatchUpRecallDispatchedFactId { get; } = new PlanningFactId(CatchUpRecallDispatchedFactIdValue);

    public static BlackboardKey<bool> FollowCatchUpCandidate { get; } = new BlackboardKey<bool>(
        "avalon.foa.companions",
        "follow-catch-up-candidate",
        1,
        BlackboardAccess.Authoritative,
        BlackboardScope.Actor);
}
