using System;
using System.Linq;

namespace DragonKnight;

internal readonly struct DragonKnightNativeSpawnerAnchorDefinition
{
    internal DragonKnightNativeSpawnerAnchorDefinition(
        string anchorId,
        string sourcePath,
        string sourceTemplateName,
        string sourceTemplateGuid,
        float sourceX,
        float sourceY,
        float sourceZ)
    {
        AnchorId = anchorId;
        SourcePath = sourcePath;
        SourceTemplateName = sourceTemplateName;
        SourceTemplateGuid = sourceTemplateGuid;
        SourceX = sourceX;
        SourceY = sourceY;
        SourceZ = sourceZ;
    }

    internal string AnchorId { get; }
    internal string SourcePath { get; }
    internal string SourceTemplateName { get; }
    internal string SourceTemplateGuid { get; }
    internal float SourceX { get; }
    internal float SourceY { get; }
    internal float SourceZ { get; }
}

internal static class DragonKnightNativeSpawnerPlacementContract
{
    internal const string ConfigSection = "DragonKnightNativeSpawnerPlacement";
    internal const bool DefaultEnabled = true;
    internal const bool DefaultKillSwitch = false;
    internal const string RouteId = "dragon-knight.dk5d.native-location-spawner-candidate";
    internal const string ActorSource = "native-location-spawner-candidate:hos-cromlech-grindylow01-native-slot";
    internal const string LogicalSceneName = DragonKnightOwnedPlacementContract.SceneName;
    internal const string NativeSpawnerSceneName = "CampaignMap_HOS_merged";
    internal const float NativeSpawnerMatchToleranceMeters = 0.25f;
    internal const float SourceSpawnerDistanceToBossRootMeters = 77.83f;
    internal const string EvidenceId = "template-diagnostics.20260803-035222.spawner_refs.csv:line341";
    internal const string CandidatePolicy = "replace-single-candidate";

    internal static readonly DragonKnightNativeSpawnerAnchorDefinition[] Anchors =
    {
        new(
            "hos-cromlech-grindylow01-native-slot",
            "/SpawnerSingle_EnemyMonster_T1_Grindylow_01",
            "Spec_EnemyMonster_T1_Grindylow",
            "fa79aaa0bff59484dab2cf35c5ea805c",
            -1860.15f,
            73.81f,
            -2958.34f),
    };

    internal static bool TryMatchNativeSpawner(
        float x,
        float y,
        float z,
        out DragonKnightNativeSpawnerAnchorDefinition anchor,
        out int anchorIndex)
    {
        float toleranceSquared = NativeSpawnerMatchToleranceMeters * NativeSpawnerMatchToleranceMeters;
        for (int index = 0; index < Anchors.Length; index++)
        {
            DragonKnightNativeSpawnerAnchorDefinition candidate = Anchors[index];
            float dx = x - candidate.SourceX;
            float dy = y - candidate.SourceY;
            float dz = z - candidate.SourceZ;
            if ((dx * dx) + (dy * dy) + (dz * dz) <= toleranceSquared)
            {
                anchor = candidate;
                anchorIndex = index;
                return true;
            }
        }

        anchor = default;
        anchorIndex = -1;
        return false;
    }

    internal static string NativeSpawnerSummary()
    {
        return string.Join("|", Anchors.Select(anchor =>
            FormattableString.Invariant($"{anchor.AnchorId}:{anchor.SourceTemplateName}[{anchor.SourceTemplateGuid}]@{anchor.SourceX:0.###},{anchor.SourceY:0.###},{anchor.SourceZ:0.###}")));
    }
}
