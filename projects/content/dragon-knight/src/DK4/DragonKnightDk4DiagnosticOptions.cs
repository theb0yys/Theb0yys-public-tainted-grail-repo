using DragonKnight.AI.Package.V2;
using UnityEngine;

namespace DragonKnight;

internal static class DragonKnightDk4DiagnosticOptions
{
    internal const string ConfigSection = "DK4Diagnostic";
    internal const bool DefaultEnabled = false;
    internal const bool DefaultKillSwitch = true;
    internal const bool DefaultAllowNativeSpawn = false;
    internal const int FixtureCount = 36;
    internal const float DefaultMaximumObservationSeconds = 10f;
    internal const KeyCode DefaultActivationHotkey = KeyCode.F7;
    internal const KeyCode DefaultReleaseHotkey = KeyCode.F6;

    internal const string OwnerId = "dragon-knight.dk4.diagnostic-host";
    internal const string ActorRole = "dragon-knight.boss";
    internal const string ActorSource = "spawnlocation:dragon-location-template";
    internal const string TargetSource = "AltarInteract";
    internal const string TargetKind = "LocationBackedInteractable";
    internal const string TargetLocationId = "CM_Stonehenge_0_2258891627046429048_1";
    internal const string TargetId = "foa.location:CM_Stonehenge_0_2258891627046429048_1";
    internal const string SceneName = DragonKnightAiV2Contract.SceneName;
    internal const string BossName = DragonKnightAiV2Contract.BossName;
    internal const string LocationTemplateName = "Spec_DragonKnight_DK4A";
    internal const string LocationTemplateGuid = DragonKnightAiV2Contract.LocationTemplateGuid;
    internal const string NpcTemplateGuid = DragonKnightAiV2Contract.NpcTemplateGuid;
    internal const string NativeVisualAddress = "b4d3a4e9a58fb1c4ea4c239f267faa56";
    internal const string IronOverlayRootName = "DragonKnightDk4RenderOverlay_Iron";

    internal const float ArenaCenterX = -1792.661f;
    internal const float ArenaCenterY = 79.942f;
    internal const float ArenaCenterZ = -2912.844f;
    internal const float EdgeWitnessX = -1808.181f;
    internal const float EdgeWitnessY = 82.621f;
    internal const float EdgeWitnessZ = -2931.862f;

    internal const int OuterWakeRadiusMeters = DragonKnightAiV2Contract.OuterWakeRadiusMeters;
    internal const int InnerFightStartRadiusMeters = DragonKnightAiV2Contract.InnerFightStartRadiusMeters;
    internal const int SoftLeashRadiusMeters = DragonKnightAiV2Contract.SoftLeashRadiusMeters;
    internal const int HardLeashRadiusMeters = DragonKnightAiV2Contract.HardLeashRadiusMeters;

    internal static Vector3 ArenaCenter => new(ArenaCenterX, ArenaCenterY, ArenaCenterZ);

    internal static Quaternion FacingEdgeRotation()
    {
        Vector3 direction = new Vector3(EdgeWitnessX, ArenaCenterY, EdgeWitnessZ) - ArenaCenter;
        direction.y = 0f;
        return direction.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(direction.normalized, Vector3.up)
            : Quaternion.identity;
    }
}
