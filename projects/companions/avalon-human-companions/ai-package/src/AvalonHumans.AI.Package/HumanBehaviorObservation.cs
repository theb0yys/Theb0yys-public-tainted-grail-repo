using AvalonAI.Contracts;

namespace AvalonHumans.AI.Package;

public sealed class HumanBehaviorObservation : IAvalonAiObservation
{
    public HumanBehaviorObservation(
        bool hasManagedHuman,
        bool? alive,
        bool? working,
        HumanCompanionMode mode,
        bool holdMovementLockAttached,
        float? distanceToHero,
        float followRecallDistance,
        float combatRecallDistance,
        float emergencyRecallDistance,
        int heroLiveAttackers,
        bool nativeDefendAllowed,
        bool defendPromptReady,
        bool followCatchUpEnabled,
        bool emergencyRecallEnabled)
    {
        HasManagedHuman = hasManagedHuman;
        Alive = alive;
        Working = working;
        Mode = mode;
        HoldMovementLockAttached = holdMovementLockAttached;
        DistanceToHero = distanceToHero;
        FollowRecallDistance = followRecallDistance;
        CombatRecallDistance = combatRecallDistance;
        EmergencyRecallDistance = emergencyRecallDistance;
        HeroLiveAttackers = heroLiveAttackers;
        NativeDefendAllowed = nativeDefendAllowed;
        DefendPromptReady = defendPromptReady;
        FollowCatchUpEnabled = followCatchUpEnabled;
        EmergencyRecallEnabled = emergencyRecallEnabled;
    }

    public bool HasManagedHuman { get; }
    public bool? Alive { get; }
    public bool? Working { get; }
    public HumanCompanionMode Mode { get; }
    public bool HoldMovementLockAttached { get; }
    public float? DistanceToHero { get; }
    public float FollowRecallDistance { get; }
    public float CombatRecallDistance { get; }
    public float EmergencyRecallDistance { get; }
    public int HeroLiveAttackers { get; }
    public bool NativeDefendAllowed { get; }
    public bool DefendPromptReady { get; }
    public bool FollowCatchUpEnabled { get; }
    public bool EmergencyRecallEnabled { get; }
}
