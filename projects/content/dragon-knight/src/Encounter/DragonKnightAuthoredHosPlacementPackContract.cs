using System.Collections.Generic;

namespace DragonKnight;

internal static class DragonKnightAuthoredHosPlacementPackContract
{
    internal const int FixtureCount = 25;
    internal const string SourceGateMarkerLine =
        "DRAGON_KNIGHT_DK5C_AUTHORED_HOS_PLACEMENT_PACK_SOURCE_PASS fixtures=25 scene=CampaignMap_HOS anchor=hos.ancient-cromlech.center pack=dragon-knight.dk5c.authored-hos-placement-pack.v1 prefab-root=DK5C_DragonKnight_Cromlech_PlacementRoot marker=DK5C_DRAGON_KNIGHT_AUTHORED_HOS_PLACEMENT_PACK_SOURCE multi-npc-ready=1 altar-ref=1 boss-root=1 altar-trigger=0 template-guid=d7b09116519f7564593be62781bee3db npc-guid=00f608ee051b57748a6a9ed8dae28678 no-save-reject=1 observer-compatible=1 unity-mutation=0 pack-build=0 live-proof=0 native-spawn=0 vanilla-slot=0 save-write=0";

    internal const string SchemaId = "dragon-knight.dk5c.authored-hos-placement-pack.v1";
    internal const string PackId = "dragon-knight.dk5c.authored-hos-placement-pack.v1";
    internal const string OwnerModId = "dragon-knight";
    internal const string CoreAnchorRef = "hos.ancient-cromlech.center";
    internal const string CoreLocalRef = "A-HOS-ANCIENT-CROMLECH-17";
    internal const string OverlayId = "dragon-knight.cromlech.overlay.v1";
    internal const string OverlayManifestPath = "placement/dragon-knight-cromlech-overlay.v1.json";

    internal const string UnityProjectRoot = @"<local-path>";
    internal const string UnityEditorSourceFile = @"Assets\Editor\DragonKnight\DK5CAuthoredHosPlacementPackBuilder.cs";
    internal const string UnityPlacementManifestAssetPath = @"Assets\DragonKnight\Placement\DK5C\dragon-knight-dk5c-authored-hos-placement-pack.v1.json";
    internal const string UnityPlacementPrefabRootPath = @"Assets\DragonKnight\Placement\DK5C\Prefabs\DK5C_DragonKnight_Cromlech_PlacementRoot.prefab";
    internal const string UnityPlacementRootName = "DK5C_DragonKnight_Cromlech_PlacementRoot";
    internal const string UnityMarkerComponentType = "DragonKnight.Editor.DK5CAuthoredPlacementMarker";
    internal const string UnityMarkerId = "DK5C_DRAGON_KNIGHT_AUTHORED_HOS_PLACEMENT_PACK_SOURCE";
    internal const string SerializedPlacementKind = "authored-scene-overlay-location-template-placement";
    internal const string ExpectedOutputRoot = @"<local-path>";
    internal const string ExpectedBundleName = "dragon-knight-dk5c-authored-hos-placement-pack";
    internal const string ExpectedHashManifest = "dragon-knight-dk5c-authored-hos-placement-pack.hashes.json";
    internal const string DeploymentTargetRoot = @"<local-path>";
    internal const string BuildCommand =
        @"Unity.exe -batchmode -quit -projectPath ""<local-path>"" -executeMethod DragonKnight.Editor.DK5CAuthoredHosPlacementPackBuilder.Build";
    internal const string DeployCommand =
        @"Copy-Item -LiteralPath ""<local-path>"" -Destination ""<local-path>"" -Recurse -Force";

    internal const bool SupportsMultiplePlacements = true;
    internal const bool RequiresSaveOwnedLocation = true;
    internal const bool RejectsMarkedNotSaved = true;
    internal const bool RejectsIsNotSaved = true;
    internal const bool RequiresLocationIdProof = true;
    internal const bool RequiresHostOwnershipProof = true;
    internal const bool AllowsNativeSpawn = false;
    internal const bool AllowsVanillaSpawnerSlots = false;
    internal const bool AllowsRuntimeSceneMutation = false;
    internal const bool AllowsSaveWrites = false;
    internal const bool ObserverCompatible = true;
    internal const string BossRootEvidence = DragonKnightOwnedPlacementContract.BossRootEvidence;
    internal const string AltarReferenceId = DragonKnightOwnedPlacementContract.AltarReferenceId;
    internal const string AltarReferenceSource = DragonKnightOwnedPlacementContract.AltarReferenceSource;
    internal const string AltarReferenceLiveLocationId = DragonKnightOwnedPlacementContract.AltarReferenceLiveLocationId;
    internal const string AltarReferenceTargetId = DragonKnightOwnedPlacementContract.AltarReferenceTargetId;
    internal const string AltarReferenceComponentPath = DragonKnightOwnedPlacementContract.AltarReferenceComponentPath;
    internal const string AltarReferenceEvidence = DragonKnightOwnedPlacementContract.AltarReferenceEvidence;
    internal const float AltarReferenceX = DragonKnightOwnedPlacementContract.AltarReferenceX;
    internal const float AltarReferenceY = DragonKnightOwnedPlacementContract.AltarReferenceY;
    internal const float AltarReferenceZ = DragonKnightOwnedPlacementContract.AltarReferenceZ;

    internal static readonly IReadOnlyList<DragonKnightAuthoredHosPlacementPackEntry> Placements =
        new[]
        {
            new DragonKnightAuthoredHosPlacementPackEntry(
                DragonKnightOwnedPlacementContract.PrimaryPlacementId,
                DragonKnightOwnedPlacementContract.EncounterId,
                DragonKnightOwnedPlacementContract.ActorRole,
                DragonKnightOwnedPlacementContract.ActorSource,
                DragonKnightOwnedPlacementContract.BossName,
                DragonKnightOwnedPlacementContract.SceneName,
                DragonKnightOwnedPlacementContract.LocationTemplateName,
                DragonKnightOwnedPlacementContract.LocationTemplateGuid,
                DragonKnightOwnedPlacementContract.NpcTemplateGuid,
                DragonKnightOwnedPlacementContract.ArenaCenterX,
                DragonKnightOwnedPlacementContract.ArenaCenterY,
                DragonKnightOwnedPlacementContract.ArenaCenterZ,
                0f,
                DragonKnightOwnedPlacementContract.BossRootYawDegrees,
                0f,
                DragonKnightOwnedPlacementContract.InnerFightStartRadiusMeters,
                DragonKnightOwnedPlacementContract.OuterWakeRadiusMeters,
                DragonKnightOwnedPlacementContract.MaxPlacementDistanceMeters,
                "dragon-knight:CampaignMap_HOS:dragon-knight.vaelor.cromlech.center:d7b09116519f7564593be62781bee3db",
                DragonKnightOwnedPlacementContract.ActivationTrigger,
                BossRootEvidence,
                AltarReferenceId,
                AltarReferenceSource,
                AltarReferenceLiveLocationId,
                AltarReferenceTargetId,
                AltarReferenceComponentPath,
                AltarReferenceX,
                AltarReferenceY,
                AltarReferenceZ,
                AltarReferenceEvidence,
                UnityPlacementRootName,
                UnityPlacementPrefabRootPath,
                UnityMarkerId,
                DragonKnightOwnedPlacementMarker.ObservedMarker),
        };
}

internal readonly struct DragonKnightAuthoredHosPlacementPackEntry
{
    internal DragonKnightAuthoredHosPlacementPackEntry(
        string placementId,
        string encounterId,
        string actorRole,
        string actorSource,
        string displayName,
        string sceneName,
        string locationTemplateName,
        string locationTemplateGuid,
        string npcTemplateGuid,
        float x,
        float y,
        float z,
        float pitch,
        float yaw,
        float roll,
        float activationRadiusMeters,
        float witnessRadiusMeters,
        float maxPlacementDriftMeters,
        string duplicateKey,
        string activationTrigger,
        string bossRootEvidence,
        string altarReferenceId,
        string altarReferenceSource,
        string altarReferenceLiveLocationId,
        string altarReferenceTargetId,
        string altarReferenceComponentPath,
        float altarReferenceX,
        float altarReferenceY,
        float altarReferenceZ,
        string altarReferenceEvidence,
        string placementRootName,
        string placementPrefabRootPath,
        string markerId,
        string expectedObservedMarker)
    {
        PlacementId = placementId;
        EncounterId = encounterId;
        ActorRole = actorRole;
        ActorSource = actorSource;
        DisplayName = displayName;
        SceneName = sceneName;
        LocationTemplateName = locationTemplateName;
        LocationTemplateGuid = locationTemplateGuid;
        NpcTemplateGuid = npcTemplateGuid;
        X = x;
        Y = y;
        Z = z;
        Pitch = pitch;
        Yaw = yaw;
        Roll = roll;
        ActivationRadiusMeters = activationRadiusMeters;
        WitnessRadiusMeters = witnessRadiusMeters;
        MaxPlacementDriftMeters = maxPlacementDriftMeters;
        DuplicateKey = duplicateKey;
        ActivationTrigger = activationTrigger;
        BossRootEvidence = bossRootEvidence;
        AltarReferenceId = altarReferenceId;
        AltarReferenceSource = altarReferenceSource;
        AltarReferenceLiveLocationId = altarReferenceLiveLocationId;
        AltarReferenceTargetId = altarReferenceTargetId;
        AltarReferenceComponentPath = altarReferenceComponentPath;
        AltarReferenceX = altarReferenceX;
        AltarReferenceY = altarReferenceY;
        AltarReferenceZ = altarReferenceZ;
        AltarReferenceEvidence = altarReferenceEvidence;
        PlacementRootName = placementRootName;
        PlacementPrefabRootPath = placementPrefabRootPath;
        MarkerId = markerId;
        ExpectedObservedMarker = expectedObservedMarker;
    }

    internal string PlacementId { get; }
    internal string EncounterId { get; }
    internal string ActorRole { get; }
    internal string ActorSource { get; }
    internal string DisplayName { get; }
    internal string SceneName { get; }
    internal string LocationTemplateName { get; }
    internal string LocationTemplateGuid { get; }
    internal string NpcTemplateGuid { get; }
    internal float X { get; }
    internal float Y { get; }
    internal float Z { get; }
    internal float Pitch { get; }
    internal float Yaw { get; }
    internal float Roll { get; }
    internal float ActivationRadiusMeters { get; }
    internal float WitnessRadiusMeters { get; }
    internal float MaxPlacementDriftMeters { get; }
    internal string DuplicateKey { get; }
    internal string ActivationTrigger { get; }
    internal string BossRootEvidence { get; }
    internal string AltarReferenceId { get; }
    internal string AltarReferenceSource { get; }
    internal string AltarReferenceLiveLocationId { get; }
    internal string AltarReferenceTargetId { get; }
    internal string AltarReferenceComponentPath { get; }
    internal float AltarReferenceX { get; }
    internal float AltarReferenceY { get; }
    internal float AltarReferenceZ { get; }
    internal string AltarReferenceEvidence { get; }
    internal string PlacementRootName { get; }
    internal string PlacementPrefabRootPath { get; }
    internal string MarkerId { get; }
    internal string ExpectedObservedMarker { get; }
}
