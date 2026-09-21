using System;
using System.Collections.Generic;
using Awaken.TG.Main.AI.SummonsAndAllies;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Scenes;
using Awaken.TG.Main.UI.TitleScreen.Loading;
using Awaken.TG.MVC;
using AvalonCompanions.Framework;
using UnityEngine;

namespace AvalonCompanions.Patches;

internal static partial class PetCompanionController
{
    private const float AiRuntimeRecoverCooldownSeconds = 8f;

    private static string _aiRuntimeOwnerId = string.Empty;
    private static long _aiRuntimeObservationSequence;
    private static long _aiRuntimePendingSequence;
    private static Location? _aiRuntimePendingLocation;
    private static float _nextAiRuntimeRecoverAt;

    private static bool IsAiRuntimeOwned => !string.IsNullOrEmpty(_aiRuntimeOwnerId);

    internal static string InspectAiRuntimeReadiness(string ownerId)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return "invalid-owner-id";
        }

        if (IsAiRuntimeOwned && !string.Equals(_aiRuntimeOwnerId, ownerId, StringComparison.Ordinal))
        {
            return "companion-ai-owned-by-another-host";
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
        reason = "companion-ai-ownership-acquired";
        _logger?.LogInfo($"Avalon AI Runtime acquired companion assist ownership. Owner={ownerId}; legacyFollow=false; legacyDefend=false; profileAssist=false.");
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
        _logger?.LogInfo($"Avalon AI Runtime released companion assist ownership. Owner={ownerId}; configured legacy/profile behavior may resume.");
        return true;
    }

    internal static bool TryInspectAiRuntimeActorIdentity(
        string ownerId,
        out string actorRuntimeId)
    {
        actorRuntimeId = string.Empty;
        if (!OwnsAiRuntime(ownerId) || !string.IsNullOrEmpty(GetAiRuntimeBlockReason()))
        {
            return false;
        }

        try
        {
            RemoveDiscardedCreatureCandidates();
            List<Location> activeRoster = GetActiveManagedRosterLocations(_logger!);
            if (activeRoster.Count != 1)
            {
                return false;
            }

            Location location = activeRoster[0];
            string locationId = location.ID ?? string.Empty;
            if (location.HasBeenDiscarded
                || !IsRosterPet(location)
                || string.IsNullOrWhiteSpace(locationId))
            {
                return false;
            }

            actorRuntimeId = locationId.Trim();
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"Avalon AI Runtime actor identity inspection failed closed: {ex.GetType().Name}: {ex.Message}");
            actorRuntimeId = string.Empty;
            return false;
        }
    }

    internal static bool TryCollectAiRuntimeObservation(
        string ownerId,
        out AvalonCompanionAiRuntimeObservation? observation)
    {
        observation = null;
        ClearAiRuntimePendingObservation();

        if (!OwnsAiRuntime(ownerId) || !string.IsNullOrEmpty(GetAiRuntimeBlockReason()))
        {
            return false;
        }

        try
        {
            RemoveDiscardedCreatureCandidates();
            List<Location> activeRoster = GetActiveManagedRosterLocations(_logger!);
            if (activeRoster.Count > 1)
            {
                return false;
            }

            if (activeRoster.Count == 0)
            {
                observation = new AvalonCompanionAiRuntimeObservation(
                    sequence: 0,
                    actorRuntimeId: string.Empty,
                    hasActiveCompanion: false,
                    mode: ToAiRuntimeMode(_companionMode),
                    hasNpcElement: false,
                    npcAlive: null,
                    npcWorking: null,
                    npcInCombat: null,
                    canMove: null,
                    possibleTargetCount: null,
                    possibleAttackerCount: null,
                    heroLiveAttackers: CountLiveHeroAttackers(Hero.Current),
                    movementState: string.Empty,
                    followCatchUpCandidate: false,
                    nativeDefendAssistAllowed: false,
                    nativeDefendPromptReady: false);
                return true;
            }

            Location location = activeRoster[0];
            int trackedCreatureCount = CreatureCandidateLocations
                .FindAll(candidate => candidate != null && !candidate.HasBeenDiscarded)
                .Count;
            CompanionAiProfile profile = BuildCompanionAiProfile(
                location,
                GetActiveSceneName(),
                activeRoster.Count,
                trackedCreatureCount);
            CompanionBondPolicyDecision bondPolicy = GetCompanionBondPolicy(GetRosterEntry(location));
            bool nativeDefendAssistAllowed =
                _companionMode == CompanionMode.Defend
                || (Plugin.EnableNativeDefendAssist.Value && bondPolicy.AllowsAutomaticDefendAssist);
            int heroLiveAttackers = profile.HeroLiveAttackers ?? 0;
            if (heroLiveAttackers <= 0)
            {
                NextCreatureCandidateDefendPromptAt.Remove(location);
            }

            float now = Time.realtimeSinceStartup;
            bool nativeDefendPromptReady =
                !NextCreatureCandidateDefendPromptAt.TryGetValue(location, out float nextPromptAt)
                || now >= nextPromptAt;

            long sequence = NextAiRuntimeObservationSequence();
            _aiRuntimePendingSequence = sequence;
            _aiRuntimePendingLocation = location;

            observation = new AvalonCompanionAiRuntimeObservation(
                sequence,
                profile.LocationId,
                hasActiveCompanion: true,
                mode: ToAiRuntimeMode(profile.Mode),
                hasNpcElement: profile.HasNpcElement,
                npcAlive: profile.NpcAlive,
                npcWorking: profile.NpcWorking,
                npcInCombat: profile.NpcInCombat,
                canMove: profile.CanMove,
                possibleTargetCount: profile.PossibleTargetCount,
                possibleAttackerCount: profile.PossibleAttackerCount,
                heroLiveAttackers,
                movementState: profile.MovementState,
                followCatchUpCandidate: profile.Intent == CompanionAiIntent.FollowCatchUpCandidate,
                nativeDefendAssistAllowed,
                nativeDefendPromptReady);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"Avalon AI Runtime observation failed closed: {ex.GetType().Name}: {ex.Message}");
            ClearAiRuntimePendingObservation();
            return false;
        }
    }

    internal static bool TryCollectAdvancedAiRuntimeObservation(
        string ownerId,
        bool killSwitchActive,
        out AvalonCompanionAdvancedAiRuntimeObservation? observation)
    {
        observation = null;
        ClearAiRuntimePendingObservation();

        if (!OwnsAiRuntime(ownerId) || !string.IsNullOrEmpty(GetAiRuntimeBlockReason()))
        {
            return false;
        }

        try
        {
            RemoveDiscardedCreatureCandidates();
            List<Location> activeRoster = GetActiveManagedRosterLocations(_logger!);
            if (activeRoster.Count != 1 || Hero.Current == null)
            {
                return false;
            }

            Location location = activeRoster[0];
            string locationId = location.ID ?? string.Empty;
            if (location.HasBeenDiscarded
                || !IsRosterPet(location)
                || string.IsNullOrWhiteSpace(locationId))
            {
                return false;
            }

            int trackedCreatureCount = CreatureCandidateLocations
                .FindAll(candidate => candidate != null && !candidate.HasBeenDiscarded)
                .Count;
            CompanionAiProfile profile = BuildCompanionAiProfile(
                location,
                GetActiveSceneName(),
                activeRoster.Count,
                trackedCreatureCount);
            PetRosterEntry entry = GetRosterEntry(location);
            CompanionCoreRuntimeProfile runtimeProfile = GetOrCreateCoreRuntimeProfile(entry);
            CompanionTrustProfile trust = runtimeProfile.TrustProfile;
            CompanionBondPolicyDecision bondPolicy = GetCompanionBondPolicy(runtimeProfile);
            NpcElement? npcElement = null;
            bool hasNpcElement = location.TryGetElement(out npcElement) && npcElement != null;
            bool unconscious = TryReadNpcBoolValue(npcElement, npc => npc.IsUnconscious, out bool unconsciousValue)
                && unconsciousValue;
            ICharacter? currentTarget = hasNpcElement ? ReadCurrentTargetNoChecks(npcElement) : null;
            string currentTargetId = FormatAdvancedRuntimeTargetId(currentTarget);
            int visibleAttackersCount = CountLiveHeroAttackers(Hero.Current);
            float now = Time.realtimeSinceStartup;
            bool defendCooldownReady =
                !NextCreatureCandidateDefendPromptAt.TryGetValue(location, out float nextDefendAt)
                || now >= nextDefendAt;
            bool catchUpCooldownReady = now >= _nextCreatureCandidateFollowTick;

            long sequence = NextAiRuntimeObservationSequence();
            _aiRuntimePendingSequence = sequence;
            _aiRuntimePendingLocation = location;

            observation = new AvalonCompanionAdvancedAiRuntimeObservation(
                sequence,
                locationId.Trim(),
                profile.TemplateGuid,
                profile.TemplateName,
                profile.Scene,
                ToRuntimePoint(location.Coords),
                ToRuntimePoint(Hero.Current.Coords),
                profile.DistanceToHero ?? Vector3.Distance(location.Coords, Hero.Current.Coords),
                profile.NpcAlive == true ? 1d : 0d,
                alive: profile.NpcAlive == true,
                unconscious,
                ToAiRuntimeMode(profile.Mode),
                ToAdvancedRuntimeFollowRange(profile.FollowRange),
                trust.TrustScore,
                trust.LoyaltyScore,
                ToAdvancedRuntimeBondLevel(bondPolicy.BondLevel),
                ToAdvancedRuntimeNativeState(profile, npcElement),
                currentTargetId,
                CurrentTargetThreatensHero(currentTarget),
                visibleAttackersCount,
                movementBlocked: profile.CanMove == false,
                movementStuck: profile.Intent == CompanionAiIntent.MovementBlocked,
                actorLane: AvalonCompanionAiRuntimeActorLane.ManagedAnimalCompanion,
                isManaged: true,
                isTameCandidate: false,
                isHostile: currentTarget is Hero,
                isUndeadOrBlocked: runtimeProfile.CombatRole == CompanionRuntimeCombatRole.UndeadAlly,
                isPassiveOnly: runtimeProfile.CombatRole == CompanionRuntimeCombatRole.PassiveCompanion,
                defendCooldownReady,
                catchUpCooldownReady,
                recoverCooldownReady: now >= _nextAiRuntimeRecoverAt,
                tamePromptCooldownReady: false,
                ownershipValid: true,
                killSwitchActive);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"Avalon AI Runtime advanced observation failed closed: {ex.GetType().Name}: {ex.Message}");
            ClearAiRuntimePendingObservation();
            return false;
        }
    }

    internal static bool TryDispatchAiRuntimeCommand(
        string ownerId,
        long observationSequence,
        AvalonCompanionAiRuntimeCommand command)
    {
        Location? location = _aiRuntimePendingLocation;
        long pendingSequence = _aiRuntimePendingSequence;
        ClearAiRuntimePendingObservation();

        if (!OwnsAiRuntime(ownerId)
            || observationSequence <= 0
            || observationSequence != pendingSequence
            || location == null
            || !string.IsNullOrEmpty(GetAiRuntimeBlockReason()))
        {
            return false;
        }

        try
        {
            List<Location> activeRoster = GetActiveManagedRosterLocations(_logger!);
            if (activeRoster.Count != 1
                || !ReferenceEquals(activeRoster[0], location)
                || location.HasBeenDiscarded
                || !IsRosterPet(location))
            {
                return false;
            }

            CompanionAiProfile profile = BuildCompanionAiProfile(
                location,
                GetActiveSceneName(),
                activeRoster.Count,
                CreatureCandidateLocations.Count);

            if (command == AvalonCompanionAiRuntimeCommand.CatchUpRecall)
            {
                return TryDispatchAiRuntimeCatchUp(location, profile);
            }

            if (command == AvalonCompanionAiRuntimeCommand.NativeDefendPrompt)
            {
                return TryDispatchAiRuntimeDefend(location, profile);
            }

            return command == AvalonCompanionAiRuntimeCommand.RecoverStuck
                && TryDispatchAiRuntimeRecover(location, profile);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"Avalon AI Runtime dispatch failed closed: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    internal static bool TryAuditAdvancedAiRuntimeProposal(
        string ownerId,
        long observationSequence,
        string proposalKind,
        string reason)
    {
        Location? location = _aiRuntimePendingLocation;
        long pendingSequence = _aiRuntimePendingSequence;
        ClearAiRuntimePendingObservation();

        if (!OwnsAiRuntime(ownerId)
            || observationSequence <= 0
            || observationSequence != pendingSequence
            || location == null
            || !string.IsNullOrEmpty(GetAiRuntimeBlockReason()))
        {
            return false;
        }

        try
        {
            List<Location> activeRoster = GetActiveManagedRosterLocations(_logger!);
            if (activeRoster.Count != 1
                || !ReferenceEquals(activeRoster[0], location)
                || location.HasBeenDiscarded
                || !IsRosterPet(location)
                || _logger == null)
            {
                return false;
            }

            string auditReason =
                $"proposal={proposalKind}; packageReason={reason}; owner={_aiRuntimeOwnerId}; "
                + "auditOnly=true; touchesCommands=false; touchesMovement=false; touchesTargeting=false; touchesPersistence=false; coreExecuted=false";
            WriteCompanionCommandLog(
                _logger,
                "avalon-ai-runtime-advanced-audit",
                affectedCount: 1,
                blocked: false,
                reason: auditReason);
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"Avalon AI Runtime advanced proposal audit failed closed: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private static bool TryDispatchAiRuntimeCatchUp(Location location, CompanionAiProfile profile)
    {
        if (profile.Intent != CompanionAiIntent.FollowCatchUpCandidate
            || !ShouldFollowForMode(_companionMode)
            || Hero.Current == null)
        {
            return false;
        }

        PetRosterEntry entry = GetRosterEntry(location);
        RecallLocation(
            location,
            GetPlacementPosition(Hero.Current, entry, 0),
            Hero.Current.Rotation,
            followAfterRecall: true);
        CompanionBondPolicyDecision bondPolicy = GetCompanionBondPolicy(entry);
        float followInterval = bondPolicy.ApplyCatchUpInterval(CreatureCandidateFollowTickSeconds);
        _nextCreatureCandidateFollowTick = Mathf.Max(
            _nextCreatureCandidateFollowTick,
            Time.realtimeSinceStartup + followInterval);
        string reason = profile.IntentReason
            + $"; owner={_aiRuntimeOwnerId}; interval={followInterval:0.##}; exact actor lease revalidated; existing catch-up recall path only";
        TouchCoreRuntimeProfile(
            _logger,
            entry,
            "avalon-ai-runtime-catch-up",
            reason,
            CompanionRuntimeProfileState.Active,
            affectedCount: 1);
        if (_logger != null)
        {
            WriteCompanionCommandLog(_logger, "avalon-ai-runtime-catch-up", 1, blocked: false, reason);
        }
        return true;
    }

    private static bool TryDispatchAiRuntimeDefend(Location location, CompanionAiProfile profile)
    {
        Hero hero = Hero.Current;
        if (hero == null || profile.Intent != CompanionAiIntent.NativeCombatObserved)
        {
            return false;
        }

        int attackerCount = CountLiveHeroAttackers(hero);
        if (attackerCount <= 0
            || !location.TryGetElement(out NpcElement npcElement)
            || npcElement == null
            || !IsLocationCommandPolicyAllowed(location, CompanionCommandPolicyAction.NativeDefendPrompt))
        {
            return false;
        }

        NpcHeroPetAlly petAlly = npcElement.TryGetElement<NpcHeroPetAlly>();
        if (petAlly == null || petAlly.HasBeenDiscarded)
        {
            return false;
        }

        PetRosterEntry entry = GetRosterEntry(location);
        CompanionBondPolicyDecision bondPolicy = GetCompanionBondPolicy(entry);
        bool explicitDefendMode = _companionMode == CompanionMode.Defend;
        if (!explicitDefendMode
            && (!Plugin.EnableNativeDefendAssist.Value || !bondPolicy.AllowsAutomaticDefendAssist))
        {
            return false;
        }

        float now = Time.realtimeSinceStartup;
        float defendCooldown = bondPolicy.ApplyDefendCooldown(CreatureCandidateDefendPromptCooldownSeconds);
        if (NextCreatureCandidateDefendPromptAt.TryGetValue(location, out float nextPromptAt)
            && now < nextPromptAt)
        {
            return false;
        }

        location.MarkedNotSaved = true;
        petAlly.EnterCombat();
        NextCreatureCandidateDefendPromptAt[location] = now + defendCooldown;
        _nextCreatureCandidateDefendTick = Mathf.Max(
            _nextCreatureCandidateDefendTick,
            now + CreatureCandidateDefendTickSeconds);
        string reason = $"attackers={attackerCount}; owner={_aiRuntimeOwnerId}; cooldown={defendCooldown:0.##}; exact actor lease revalidated; existing native hero-pet ally path only";
        TouchCoreRuntimeProfile(
            _logger,
            entry,
            "avalon-ai-runtime-native-defend",
            reason,
            CompanionRuntimeProfileState.Active,
            affectedCount: 1);
        if (_logger != null)
        {
            WriteCompanionCommandLog(_logger, "avalon-ai-runtime-native-defend", 1, blocked: false, reason);
        }
        return true;
    }

    private static bool TryDispatchAiRuntimeRecover(Location location, CompanionAiProfile profile)
    {
        if (profile.Intent != CompanionAiIntent.MovementBlocked || _logger == null)
        {
            return false;
        }

        float now = Time.realtimeSinceStartup;
        if (now < _nextAiRuntimeRecoverAt)
        {
            return false;
        }

        if (!IsLocationCommandPolicyAllowed(location, CompanionCommandPolicyAction.Recover))
        {
            return false;
        }

        _nextAiRuntimeRecoverAt = now + AiRuntimeRecoverCooldownSeconds;
        string reason = profile.IntentReason
            + $"; owner={_aiRuntimeOwnerId}; cooldown={AiRuntimeRecoverCooldownSeconds:0.##}; existing managed recover path only";
        WriteCompanionCommandLog(
            _logger,
            "avalon-ai-runtime-recover",
            affectedCount: 1,
            blocked: false,
            reason: reason);
        RecoverManagedPets(_logger);
        return true;
    }

    private static string GetAiRuntimeBlockReason()
    {
        if (!Plugin.Enabled.Value)
        {
            return "avalon-companions-disabled";
        }

        if (!Plugin.EnablePetCompanionRoster.Value)
        {
            return "avalon-companion-roster-disabled";
        }

        string runtimeBlockReason = GetRuntimeBlockReason();
        if (!string.IsNullOrEmpty(runtimeBlockReason))
        {
            return runtimeBlockReason;
        }

        try
        {
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

    private static AvalonCompanionAiRuntimeMode ToAiRuntimeMode(CompanionMode mode)
    {
        return mode == CompanionMode.Stay
            ? AvalonCompanionAiRuntimeMode.Stay
            : mode == CompanionMode.Defend
                ? AvalonCompanionAiRuntimeMode.Defend
                : AvalonCompanionAiRuntimeMode.Follow;
    }

    private static AvalonCompanionAiRuntimeFollowRange ToAdvancedRuntimeFollowRange(FollowRangeProfile range)
    {
        return NormalizeFollowRange(range) switch
        {
            FollowRangeProfile.Close => AvalonCompanionAiRuntimeFollowRange.Close,
            FollowRangeProfile.Far => AvalonCompanionAiRuntimeFollowRange.Far,
            _ => AvalonCompanionAiRuntimeFollowRange.Pace,
        };
    }

    private static AvalonCompanionAiRuntimeBondLevel ToAdvancedRuntimeBondLevel(CompanionBondLevel bondLevel)
    {
        return bondLevel switch
        {
            CompanionBondLevel.Wary => AvalonCompanionAiRuntimeBondLevel.Wary,
            CompanionBondLevel.Trusted => AvalonCompanionAiRuntimeBondLevel.Trusted,
            CompanionBondLevel.Loyal => AvalonCompanionAiRuntimeBondLevel.Loyal,
            _ => AvalonCompanionAiRuntimeBondLevel.Familiar,
        };
    }

    private static AvalonCompanionAiRuntimeNativeState ToAdvancedRuntimeNativeState(
        CompanionAiProfile profile,
        NpcElement? npcElement)
    {
        if (TryReadNpcAiBoolValue(npcElement, ai => ai.InFlee, out bool npcInFlee) && npcInFlee)
        {
            return AvalonCompanionAiRuntimeNativeState.Flee;
        }

        if (profile.NpcInCombat == true)
        {
            return AvalonCompanionAiRuntimeNativeState.Combat;
        }

        if (TryReadNpcAiBoolValue(npcElement, ai => ai.InAlert, out bool npcInAlert) && npcInAlert)
        {
            return AvalonCompanionAiRuntimeNativeState.Alert;
        }

        return profile.NpcInIdle == true
            ? AvalonCompanionAiRuntimeNativeState.Idle
            : AvalonCompanionAiRuntimeNativeState.Unknown;
    }

    private static AvalonCompanionAiRuntimePoint ToRuntimePoint(Vector3 coords)
    {
        return new AvalonCompanionAiRuntimePoint(coords.x, coords.y, coords.z);
    }

    private static string FormatAdvancedRuntimeTargetId(ICharacter? currentTarget)
    {
        string templateGuid = FormatTargetTemplateGuid(currentTarget);
        if (!string.IsNullOrWhiteSpace(templateGuid))
        {
            return templateGuid;
        }

        string targetName = FormatTargetName(currentTarget);
        return string.IsNullOrWhiteSpace(targetName) ? string.Empty : targetName;
    }

    private static bool CurrentTargetThreatensHero(ICharacter? currentTarget)
    {
        if (currentTarget == null || Hero.Current == null)
        {
            return false;
        }

        try
        {
            foreach (ICharacter attacker in Hero.Current.PossibleAttackers)
            {
                if (attacker != null && !attacker.HasBeenDiscarded && ReferenceEquals(attacker, currentTarget))
                {
                    return true;
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }
}
