using System.Collections.Generic;
using DragonKnight.AI.Package.V2;
using UnityEngine;

namespace DragonKnight;

internal static class DragonKnightOwnedPlacementContract
{
    internal const string ConfigSection = "DragonKnightOwnedPlacement";
    internal const bool DefaultObserveOwnedPlacements = true;
    internal const int FixtureCount = 32;

    internal const string OwnerId = DragonKnightAiV2Contract.RequiredOwnerId;
    internal const string EncounterId = "dragon-knight.vaelor.cromlech";
    internal const string PrimaryPlacementId = "dragon-knight.vaelor.cromlech.center";
    internal const string ActorRole = DragonKnightAiV2Contract.BossActorRoleIdValue;
    internal const string ActorSource = DragonKnightNativeSpawnerPlacementContract.ActorSource;
    internal const string SceneName = DragonKnightAiV2Contract.SceneName;
    internal const string SceneDisplayName = "Horns of the South";
    internal const string ArenaName = DragonKnightAiV2Contract.ArenaName;
    internal const string BossName = DragonKnightAiV2Contract.BossName;
    internal const string LocationTemplateName = "Spec_DragonKnight_DK4A";
    internal const string LocationTemplateGuid = DragonKnightAiV2Contract.LocationTemplateGuid;
    internal const string NpcTemplateGuid = DragonKnightAiV2Contract.NpcTemplateGuid;
    internal const string ActivationTrigger = DragonKnightAiV2Contract.ActivationTrigger;

    internal const string AltarReferenceId = "dragon-knight.vaelor.cromlech.altar-reference";
    internal const string AltarReferenceSource = "AltarInteract";
    internal const string AltarReferenceLiveLocationId = "CM_Stonehenge_0_2258891627046429048_1";
    internal const string AltarReferenceTargetId = "foa.location:CM_Stonehenge_0_2258891627046429048_1";
    internal const string AltarReferenceComponentPath = "/SpawnedLocations_30/AltarInteract";
    internal const string AltarReferenceEvidence = "template-diagnostics.20260803-032730.cromlech-target-proof.line-21";
    internal const string BossRootEvidence = "template-diagnostics.20260803-032730.hero-to-altar-offset-4m";
    internal const float AltarReferenceX = -1792.661f;
    internal const float AltarReferenceY = 79.942f;
    internal const float AltarReferenceZ = -2912.844f;

    internal const float ArenaCenterX = -1795.209f;
    internal const float ArenaCenterY = 80.243f;
    internal const float ArenaCenterZ = -2915.928f;
    internal const float BossRootYawDegrees = -140.441f;
    internal const float EdgeWitnessX = -1808.181f;
    internal const float EdgeWitnessY = 82.621f;
    internal const float EdgeWitnessZ = -2931.862f;
    internal const float MaxPlacementDistanceMeters = 3f;

    internal const int OuterWakeRadiusMeters = DragonKnightAiV2Contract.OuterWakeRadiusMeters;
    internal const int InnerFightStartRadiusMeters = DragonKnightAiV2Contract.InnerFightStartRadiusMeters;
    internal const int SoftLeashRadiusMeters = DragonKnightAiV2Contract.SoftLeashRadiusMeters;
    internal const int HardLeashRadiusMeters = DragonKnightAiV2Contract.HardLeashRadiusMeters;

    internal static readonly IReadOnlyList<DragonKnightOwnedNpcPlacement> Placements =
        new[]
        {
            new DragonKnightOwnedNpcPlacement(
                PrimaryPlacementId,
                EncounterId,
                ActorRole,
                BossName,
                SceneName,
                LocationTemplateName,
                LocationTemplateGuid,
                NpcTemplateGuid,
                new Vector3(ArenaCenterX, ArenaCenterY, ArenaCenterZ),
                MaxPlacementDistanceMeters),
        };

    internal static DragonKnightPlacementReference AltarReference =>
        new(
            AltarReferenceId,
            AltarReferenceSource,
            AltarReferenceLiveLocationId,
            AltarReferenceTargetId,
            AltarReferenceComponentPath,
            new Vector3(AltarReferenceX, AltarReferenceY, AltarReferenceZ),
            AltarReferenceEvidence,
            isBossPlacementRoot: false,
            isEncounterTrigger: false);

    internal static Vector3 ArenaCenter => new(ArenaCenterX, ArenaCenterY, ArenaCenterZ);

    internal static Vector3 EdgeWitness => new(EdgeWitnessX, EdgeWitnessY, EdgeWitnessZ);

    internal static Quaternion FacingEdgeRotation()
    {
        Vector3 direction = new Vector3(EdgeWitnessX, ArenaCenterY, EdgeWitnessZ) - ArenaCenter;
        direction.y = 0f;
        return direction.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(direction.normalized, Vector3.up)
            : Quaternion.identity;
    }
}

internal readonly struct DragonKnightPlacementReference
{
    internal DragonKnightPlacementReference(
        string referenceId,
        string source,
        string liveLocationId,
        string targetId,
        string componentPath,
        Vector3 position,
        string evidenceId,
        bool isBossPlacementRoot,
        bool isEncounterTrigger)
    {
        ReferenceId = referenceId;
        Source = source;
        LiveLocationId = liveLocationId;
        TargetId = targetId;
        ComponentPath = componentPath;
        Position = position;
        EvidenceId = evidenceId;
        IsBossPlacementRoot = isBossPlacementRoot;
        IsEncounterTrigger = isEncounterTrigger;
    }

    internal string ReferenceId { get; }

    internal string Source { get; }

    internal string LiveLocationId { get; }

    internal string TargetId { get; }

    internal string ComponentPath { get; }

    internal Vector3 Position { get; }

    internal string EvidenceId { get; }

    internal bool IsBossPlacementRoot { get; }

    internal bool IsEncounterTrigger { get; }
}

internal readonly struct DragonKnightOwnedNpcPlacement
{
    internal DragonKnightOwnedNpcPlacement(
        string placementId,
        string encounterId,
        string actorRole,
        string displayName,
        string sceneName,
        string locationTemplateName,
        string locationTemplateGuid,
        string npcTemplateGuid,
        Vector3 position,
        float maxPlacementDistanceMeters)
    {
        PlacementId = placementId;
        EncounterId = encounterId;
        ActorRole = actorRole;
        DisplayName = displayName;
        SceneName = sceneName;
        LocationTemplateName = locationTemplateName;
        LocationTemplateGuid = locationTemplateGuid;
        NpcTemplateGuid = npcTemplateGuid;
        Position = position;
        MaxPlacementDistanceMeters = maxPlacementDistanceMeters;
    }

    internal string PlacementId { get; }

    internal string EncounterId { get; }

    internal string ActorRole { get; }

    internal string DisplayName { get; }

    internal string SceneName { get; }

    internal string LocationTemplateName { get; }

    internal string LocationTemplateGuid { get; }

    internal string NpcTemplateGuid { get; }

    internal Vector3 Position { get; }

    internal float MaxPlacementDistanceMeters { get; }
}
