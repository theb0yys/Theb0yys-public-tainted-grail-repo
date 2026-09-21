using System;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Scenes;
using Awaken.TG.Main.UI.TitleScreen.Loading;
using Awaken.TG.MVC;
using AvalonHumanCompanions.Framework;
using UnityEngine;

namespace AvalonHumanCompanions;

public sealed partial class Plugin
{
    private static string _aiRuntimeOwnerId = string.Empty;
    private static long _aiRuntimeObservationSequence;
    private static long _aiRuntimePendingSequence;
    private static Location? _aiRuntimePendingLocation;

    private static bool IsAiRuntimeOwned => !string.IsNullOrEmpty(_aiRuntimeOwnerId);

    internal static string InspectAiRuntimeReadiness(string ownerId)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return "human-ai-owner-id-required";
        }

        if (IsAiRuntimeOwned && !string.Equals(_aiRuntimeOwnerId, ownerId, StringComparison.Ordinal))
        {
            return $"human-ai-owned-by:{_aiRuntimeOwnerId}";
        }

        return GetAiRuntimeBlockReason();
    }

    internal static bool TryAcquireAiRuntimeOwnership(string ownerId, out string reason)
    {
        reason = InspectAiRuntimeReadiness(ownerId);
        if (!string.IsNullOrEmpty(reason))
        {
            return false;
        }

        _aiRuntimeOwnerId = ownerId;
        ClearAiRuntimePendingObservation();
        reason = "human-ai-ownership-acquired";
        Instance!.Logger.LogInfo(
            $"Avalon AI Runtime acquired human companion brain ownership. Owner={ownerId}; standaloneBrain=false; legacyAssist=false; manualCommands=true; lifecycleGuard=true.");
        return true;
    }

    internal static bool TryReleaseAiRuntimeOwnership(string ownerId)
    {
        if (!IsAiRuntimeOwned)
        {
            ClearAiRuntimePendingObservation();
            return true;
        }

        if (!string.Equals(_aiRuntimeOwnerId, ownerId, StringComparison.Ordinal))
        {
            return false;
        }

        _aiRuntimeOwnerId = string.Empty;
        ClearAiRuntimePendingObservation();
        if (Instance != null)
        {
            Instance.QueueCompanionBrainRefresh();
            Instance.Logger.LogInfo(
                $"Avalon AI Runtime released human companion brain ownership. Owner={ownerId}; configured standalone brain may resume.");
        }

        return true;
    }

    internal static bool TryCollectAiRuntimeObservation(
        string ownerId,
        out AvalonHumanAiRuntimeObservation? observation)
    {
        observation = null;
        ClearAiRuntimePendingObservation();

        Plugin? instance = Instance;
        if (instance == null
            || !OwnsAiRuntime(ownerId)
            || !string.IsNullOrEmpty(GetAiRuntimeBlockReason()))
        {
            return false;
        }

        try
        {
            Location? location = instance._lastAllyProof;
            if (!instance.IsManagedAllyProof(location))
            {
                observation = new AvalonHumanAiRuntimeObservation(
                    sequence: 0,
                    actorRuntimeId: string.Empty,
                    hasManagedHuman: false,
                    alive: null,
                    working: null,
                    mode: ToAiRuntimeMode(instance._allyProofMode),
                    holdMovementLockAttached: false,
                    distanceToHero: null,
                    followRecallDistance: instance.GetAutomaticCatchUpTriggerDistance(instance._allyProofMode),
                    combatRecallDistance: instance.GetCompanionBrainCombatRecallDistance() + AutoRecallTriggerBufferMeters,
                    emergencyRecallDistance: instance.GetCompanionBrainEmergencyRecallDistance(),
                    heroLiveAttackers: CountLiveHeroAttackers(Hero.Current),
                    nativeDefendAllowed: false,
                    defendPromptReady: false,
                    followCatchUpEnabled: false,
                    emergencyRecallEnabled: false);
                return true;
            }

            NpcElement npcElement = location!.Element<NpcElement>();
            Hero? hero = Hero.Current;
            float? distanceToHero = hero == null
                ? null
                : Vector3.Distance(location.Coords, hero.Coords);
            int attackerCount = CountLiveHeroAttackers(hero);
            float now = Time.realtimeSinceStartup;
            float followRecallDistance = instance.GetAutomaticCatchUpTriggerDistance(instance._allyProofMode);
            float emergencyRecallDistance = instance.GetCompanionBrainEmergencyRecallDistance();
            float reviewedCombatRecallDistance = instance.GetCompanionBrainCombatRecallDistance()
                + AutoRecallTriggerBufferMeters;
            bool nativeDefendAllowed = instance.IsNativeDefendAllowed(attackerCount);
            bool emergencyCandidate = instance._enableCompanionBrainEmergencyRecall.Value
                && distanceToHero.HasValue
                && distanceToHero.Value > emergencyRecallDistance;
            bool emergencyReady = now >= instance._nextEmergencyAutoRecallAllowedAt;
            bool higherPriorityRecallWaiting = emergencyCandidate && !emergencyReady;
            bool combatCandidate = nativeDefendAllowed
                && distanceToHero.HasValue
                && distanceToHero.Value > reviewedCombatRecallDistance;
            bool combatReady = now >= instance._nextCombatAutoRecallAllowedAt;
            float combatRecallDistance = !higherPriorityRecallWaiting && combatReady
                ? reviewedCombatRecallDistance
                : float.MaxValue;
            higherPriorityRecallWaiting |= combatCandidate && !combatReady;
            bool followCatchUpEnabled = instance._enableFollowCatchUp.Value
                && now >= instance._nextFollowAutoRecallAllowedAt
                && !higherPriorityRecallWaiting;
            if (followCatchUpEnabled && distanceToHero.HasValue)
            {
                followCatchUpEnabled = instance.ShouldRunFollowRecallAfterGrace(
                    distanceToHero.Value,
                    followRecallDistance,
                    now);
            }

            long sequence = NextAiRuntimeObservationSequence();
            _aiRuntimePendingSequence = sequence;
            _aiRuntimePendingLocation = location;
            observation = new AvalonHumanAiRuntimeObservation(
                sequence,
                location.ID ?? string.Empty,
                hasManagedHuman: true,
                alive: TryReadBool(() => npcElement.IsAlive),
                working: TryReadBool(() => npcElement.NpcAI?.Working == true),
                mode: ToAiRuntimeMode(instance._allyProofMode),
                holdMovementLockAttached: npcElement.HasElement<HumanCompanionHoldMovementBlock>(),
                distanceToHero,
                followRecallDistance,
                combatRecallDistance,
                emergencyRecallDistance,
                attackerCount,
                nativeDefendAllowed,
                defendPromptReady: now >= instance._nextCompanionBrainDefendPromptAt,
                followCatchUpEnabled,
                emergencyRecallEnabled: instance._enableCompanionBrainEmergencyRecall.Value
                    && emergencyReady);
            return true;
        }
        catch (Exception ex)
        {
            instance.Logger.LogWarning(
                $"Avalon AI Runtime human observation failed closed: {ex.GetType().Name}: {ex.Message}");
            ClearAiRuntimePendingObservation();
            return false;
        }
    }

    internal static bool TryDispatchAiRuntimeCommand(
        string ownerId,
        long observationSequence,
        AvalonHumanAiRuntimeCommand command)
    {
        Location? location = _aiRuntimePendingLocation;
        long pendingSequence = _aiRuntimePendingSequence;
        ClearAiRuntimePendingObservation();

        Plugin? instance = Instance;
        if (instance == null
            || !OwnsAiRuntime(ownerId)
            || observationSequence <= 0
            || observationSequence != pendingSequence
            || location == null
            || !string.IsNullOrEmpty(GetAiRuntimeBlockReason()))
        {
            return false;
        }

        try
        {
            if (!ReferenceEquals(instance._lastAllyProof, location)
                || !instance.IsManagedAllyProof(location)
                || !location.TryGetElement(out NpcElement npcElement)
                || npcElement == null)
            {
                return false;
            }

            bool holdLockAttached = npcElement.HasElement<HumanCompanionHoldMovementBlock>();
            switch (command)
            {
                case AvalonHumanAiRuntimeCommand.ApplyHoldMovementLock:
                    if (instance._allyProofMode != HumanProofMode.Hold || holdLockAttached)
                    {
                        return false;
                    }

                    instance.ApplyHoldMovementState(hold: true);
                    return npcElement.HasElement<HumanCompanionHoldMovementBlock>();

                case AvalonHumanAiRuntimeCommand.ReleaseHoldMovementLock:
                    if (instance._allyProofMode == HumanProofMode.Hold || !holdLockAttached)
                    {
                        return false;
                    }

                    instance.ApplyHoldMovementState(hold: false);
                    return !npcElement.HasElement<HumanCompanionHoldMovementBlock>();

                case AvalonHumanAiRuntimeCommand.EmergencyRecall:
                    return instance.TryDispatchAiRuntimeRecall(
                        location,
                        "Avalon AI Runtime companion brain emergency recall",
                        instance.GetCompanionBrainEmergencyRecallDistance(),
                        instance._enableCompanionBrainEmergencyRecall.Value,
                        requireThreat: false);

                case AvalonHumanAiRuntimeCommand.FollowRecall:
                    return instance.TryDispatchAiRuntimeRecall(
                        location,
                        "Avalon AI Runtime companion brain follow catch-up",
                        instance.GetAutomaticCatchUpTriggerDistance(instance._allyProofMode),
                        instance._enableFollowCatchUp.Value,
                        requireThreat: false);

                case AvalonHumanAiRuntimeCommand.CombatRecall:
                    return instance.TryDispatchAiRuntimeRecall(
                        location,
                        "Avalon AI Runtime companion brain combat recall",
                        instance.GetCompanionBrainCombatRecallDistance() + AutoRecallTriggerBufferMeters,
                        enabled: true,
                        requireThreat: true);

                case AvalonHumanAiRuntimeCommand.NativeDefendPrompt:
                    return instance.TryDispatchAiRuntimeNativeDefend(location);

                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            instance.Logger.LogWarning(
                $"Avalon AI Runtime human dispatch failed closed: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private bool TryDispatchAiRuntimeRecall(
        Location expectedLocation,
        string reason,
        float minimumDistance,
        bool enabled,
        bool requireThreat)
    {
        Hero? hero = Hero.Current;
        if (!enabled
            || _allyProofMode == HumanProofMode.Hold
            || hero == null
            || !ReferenceEquals(_lastAllyProof, expectedLocation)
            || Vector3.Distance(expectedLocation.Coords, hero.Coords) <= minimumDistance)
        {
            return false;
        }

        int attackerCount = CountLiveHeroAttackers(hero);
        if (requireThreat && !IsNativeDefendAllowed(attackerCount))
        {
            return false;
        }

        bool recalled = RecallAllyProof(reason);
        if (recalled)
        {
            _nextCommandTick = Time.realtimeSinceStartup + CommandTickSeconds;
            _companionBrainLastEvent = requireThreat
                ? $"Moving into guard position; threats {attackerCount}."
                : "Closing distance under Avalon AI Runtime ownership.";
        }

        return recalled;
    }

    private bool TryDispatchAiRuntimeNativeDefend(Location expectedLocation)
    {
        Hero? hero = Hero.Current;
        int attackerCount = CountLiveHeroAttackers(hero);
        float now = Time.realtimeSinceStartup;
        if (hero == null
            || !ReferenceEquals(_lastAllyProof, expectedLocation)
            || !IsNativeDefendAllowed(attackerCount)
            || now < _nextCompanionBrainDefendPromptAt)
        {
            return false;
        }

        int prompted = TriggerAllyProofDefend("Avalon AI Runtime companion brain native defend");
        if (prompted <= 0)
        {
            return false;
        }

        float cooldown = GetCompanionBrainDefendPromptCooldown();
        _nextCompanionBrainDefendPromptAt = now + cooldown;
        _nextDefendTick = now + DefendTickSeconds;
        _companionBrainLastEvent = $"Protecting you; threats {attackerCount}.";
        Logger.LogInfo(
            $"Avalon AI Runtime dispatched human native defend. Owner={_aiRuntimeOwnerId}; Attackers={attackerCount}; cooldown={cooldown:0.##}s; exact actor lease revalidated; native NpcHeroPetAlly.EnterCombat path only.");
        WriteLifecycleEvent(
            "avalon-ai-runtime-defend",
            $"owner={_aiRuntimeOwnerId}; attackers={attackerCount}; native NpcHeroPetAlly.EnterCombat path only",
            expectedLocation,
            tracked: true);
        return true;
    }

    private bool IsNativeDefendAllowed(int attackerCount)
    {
        return attackerCount > 0
            && _allyProofMode != HumanProofMode.Hold
            && (_allyProofMode == HumanProofMode.Defend
                || IsResponsiveNativeAssistEnabled()
                || _enableNativeDefendAssist.Value);
    }

    private static string GetAiRuntimeBlockReason()
    {
        Plugin? instance = Instance;
        if (instance == null)
        {
            return "human-plugin-not-loaded";
        }

        if (!instance._enabled.Value)
        {
            return "human-companions-disabled";
        }

        if (instance._researchModeOnly.Value)
        {
            return "human-companions-research-mode-only";
        }

        if (!instance._enableOneSessionAllyProof.Value)
        {
            return "human-one-session-ally-disabled";
        }

        if (!instance._enableCompanionBrain.Value)
        {
            return "human-companion-brain-disabled";
        }

        try
        {
            if (World.EventSystem == null)
            {
                return "foa-world-event-system-unavailable";
            }

            if (LoadingScreenUI.IsLoading || World.Any<LoadingScreenUI>() != null)
            {
                return "foa-loading-in-progress";
            }

            if (!SceneLifetimeEvents.Get.EverythingInitialized)
            {
                return "foa-scene-not-fully-initialized";
            }
        }
        catch
        {
            return "foa-transition-state-unavailable";
        }

        return string.Empty;
    }

    private static bool OwnsAiRuntime(string ownerId)
    {
        return IsAiRuntimeOwned
            && string.Equals(_aiRuntimeOwnerId, ownerId, StringComparison.Ordinal);
    }

    private static void ClearAiRuntimePendingObservation()
    {
        _aiRuntimePendingSequence = 0;
        _aiRuntimePendingLocation = null;
    }

    private static long NextAiRuntimeObservationSequence()
    {
        unchecked
        {
            _aiRuntimeObservationSequence++;
        }

        if (_aiRuntimeObservationSequence <= 0)
        {
            _aiRuntimeObservationSequence = 1;
        }

        return _aiRuntimeObservationSequence;
    }

    private static AvalonHumanAiRuntimeMode ToAiRuntimeMode(HumanProofMode mode)
    {
        return mode == HumanProofMode.Hold
            ? AvalonHumanAiRuntimeMode.Hold
            : mode == HumanProofMode.Defend
                ? AvalonHumanAiRuntimeMode.Defend
                : AvalonHumanAiRuntimeMode.Follow;
    }

    private static bool? TryReadBool(Func<bool> reader)
    {
        try
        {
            return reader();
        }
        catch
        {
            return null;
        }
    }

    private static void ResetAiRuntimeBridgeForPluginUnload()
    {
        _aiRuntimeOwnerId = string.Empty;
        ClearAiRuntimePendingObservation();
    }
}
