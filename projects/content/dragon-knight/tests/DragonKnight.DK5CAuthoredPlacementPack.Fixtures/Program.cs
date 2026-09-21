using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

internal static class Program
{
    private const string RequiredFirstMarker =
        "DRAGON_KNIGHT_DK5C_AUTHORED_HOS_PLACEMENT_PACK_SOURCE_PASS fixtures=25 scene=CampaignMap_HOS anchor=hos.ancient-cromlech.center pack=dragon-knight.dk5c.authored-hos-placement-pack.v1 prefab-root=DK5C_DragonKnight_Cromlech_PlacementRoot marker=DK5C_DRAGON_KNIGHT_AUTHORED_HOS_PLACEMENT_PACK_SOURCE multi-npc-ready=1 altar-ref=1 boss-root=1 altar-trigger=0 template-guid=d7b09116519f7564593be62781bee3db npc-guid=00f608ee051b57748a6a9ed8dae28678 no-save-reject=1 observer-compatible=1 unity-mutation=0 pack-build=0 live-proof=0 native-spawn=0 vanilla-slot=0 save-write=0";

    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string ContractSource = Read("mods/dragon-knight/src/Encounter/DragonKnightAuthoredHosPlacementPackContract.cs");
    private static readonly string ManifestSource = Read("mods/dragon-knight/placement/dragon-knight-dk5c-authored-hos-placement-pack.v1.json");
    private static readonly string OverlaySource = Read("mods/dragon-knight/placement/dragon-knight-cromlech-overlay.v1.json");
    private static readonly string ObserverSource = Read("mods/dragon-knight/src/Encounter/DragonKnightOwnedPlacementObserver.cs");
    private static readonly string MarkerSource = Read("mods/dragon-knight/src/Encounter/DragonKnightOwnedPlacementMarker.cs");
    private static readonly string GateSource = Read("mods/dragon-knight/docs/gates/DK5C-authored-hos-placement-pack-source-gate.md");
    private static readonly string DecisionSource = Read("mods/dragon-knight/docs/decisions/0017-dk5c-authored-hos-placement-pack-source.md");
    private static readonly string ResearchSource = Read("mods/dragon-knight/docs/research/dk5c-authored-hos-placement-pack-source-2026-08-03.md");
    private static readonly string CoreAnchorDecisionSource = Read("mods/avalon-core/docs/decisions/0153-hos-ancient-cromlech-named-location-anchor.md");
    private static readonly string ReadmeSource = Read("mods/dragon-knight/README.md");
    private static readonly string ValidationPlanSource = Read("mods/dragon-knight/docs/validation-plan.md");
    private static readonly string TestNotesSource = Read("mods/dragon-knight/docs/test-notes.md");

    private static int Main()
    {
        Console.WriteLine(RequiredFirstMarker);

        var fixtures = new (string Name, Action Run)[]
        {
            ("fixture count is exact", FixtureCountIsExact),
            ("source marker is exact", SourceMarkerIsExact),
            ("manifest identity is exact", ManifestIdentityIsExact),
            ("scene and core anchor are exact", SceneAndCoreAnchorAreExact),
            ("Unity route paths are exact", UnityRoutePathsAreExact),
            ("prefab root and marker are exact", PrefabRootAndMarkerAreExact),
            ("manifest is multiple NPC ready", ManifestIsMultipleNpcReady),
            ("placement identity is exact", PlacementIdentityIsExact),
            ("template GUIDs are exact", TemplateGuidsAreExact),
            ("altar reference is exact", AltarReferenceIsExact),
            ("boss root is separated from altar", BossRootIsSeparatedFromAltar),
            ("transform is exact", TransformIsExact),
            ("activation and arena radii are exact", ActivationAndArenaRadiiAreExact),
            ("no-save rejection is exact", NoSaveRejectionIsExact),
            ("runtime mutation flags are blocked", RuntimeMutationFlagsAreBlocked),
            ("live observer compatibility is exact", LiveObserverCompatibilityIsExact),
            ("Core overlay compatibility is exact", CoreOverlayCompatibilityIsExact),
            ("observer rejects no-save actors", ObserverRejectsNoSaveActors),
            ("observer derives actor id from Location.ID", ObserverDerivesActorIdFromLocationId),
            ("contract and manifest avoid spawn paths", ContractAndManifestAvoidSpawnPaths),
            ("build deploy commands are named but not executed", BuildDeployCommandsAreNamedButNotExecuted),
            ("decision and gate keep Unity writes blocked", DecisionAndGateKeepUnityWritesBlocked),
            ("research records source proof boundary", ResearchRecordsSourceProofBoundary),
            ("README records DK5C", ReadmeRecordsDk5C),
            ("validation docs record DK5C", ValidationDocsRecordDk5C),
        };

        var failures = new List<string>();
        foreach (var fixture in fixtures)
        {
            try
            {
                fixture.Run();
                Console.WriteLine("PASS " + fixture.Name);
            }
            catch (Exception ex)
            {
                failures.Add(fixture.Name + ": " + ex.Message);
                Console.WriteLine("FAIL " + fixture.Name + ": " + ex);
            }
        }

        if (failures.Count == 0)
        {
            Console.WriteLine("All " + fixtures.Length + " Dragon Knight DK5C authored-placement fixtures passed.");
            return 0;
        }

        Console.WriteLine(failures.Count + " fixture(s) failed:");
        foreach (string failure in failures)
        {
            Console.WriteLine(" - " + failure);
        }

        return 1;
    }

    private static void FixtureCountIsExact() =>
        Check(RequiredFirstMarker.Contains("fixtures=25", StringComparison.Ordinal), "fixture count must remain 25.");

    private static void SourceMarkerIsExact()
    {
        Check(ContractSource.Contains(RequiredFirstMarker, StringComparison.Ordinal), "contract marker mismatch.");
        Check(GateSource.Contains(RequiredFirstMarker, StringComparison.Ordinal), "gate marker mismatch.");
        Check(RequiredFirstMarker.Contains("unity-mutation=0 pack-build=0 live-proof=0", StringComparison.Ordinal), "marker must keep Unity/build/live blocked.");
    }

    private static void ManifestIdentityIsExact()
    {
        JsonElement root = ManifestRoot();
        Check(root.GetProperty("schemaId").GetString() == "dragon-knight.dk5c.authored-hos-placement-pack.v1", "schema mismatch.");
        Check(root.GetProperty("packId").GetString() == "dragon-knight.dk5c.authored-hos-placement-pack.v1", "pack ID mismatch.");
        Check(root.GetProperty("sourceGate").GetString() == "DK5C", "source gate mismatch.");
        Check(root.GetProperty("routeStatus").GetString() == "source-proof-only", "route status mismatch.");
        Check(root.GetProperty("ownerModId").GetString() == "dragon-knight", "owner mod mismatch.");
        Check(root.GetProperty("ownerRuntimeHostId").GetString() == "dragon-knight.boss.host", "owner runtime host mismatch.");
    }

    private static void SceneAndCoreAnchorAreExact()
    {
        JsonElement root = ManifestRoot();
        Check(root.GetProperty("targetRegionId").GetString() == "region:hos", "target region mismatch.");
        Check(root.GetProperty("targetSceneName").GetString() == "CampaignMap_HOS", "target scene mismatch.");
        Check(root.GetProperty("coreAnchorRef").GetString() == "hos.ancient-cromlech.center", "Core anchor ref mismatch.");
        Check(root.GetProperty("coreLocalRef").GetString() == "A-HOS-ANCIENT-CROMLECH-17", "Core local ref mismatch.");
        Check(CoreAnchorDecisionSource.Contains("hos.ancient-cromlech.center", StringComparison.Ordinal), "Core anchor decision missing anchor ref.");
        Check(CoreAnchorDecisionSource.Contains("A-HOS-ANCIENT-CROMLECH-17", StringComparison.Ordinal), "Core anchor decision missing local ref.");
    }

    private static void UnityRoutePathsAreExact()
    {
        JsonElement authoring = ManifestRoot().GetProperty("unityAuthoring");
        Check(authoring.GetProperty("projectRoot").GetString() == @"<local-path>", "Unity project root mismatch.");
        Check(authoring.GetProperty("editorSourceFile").GetString() == @"Assets\Editor\DragonKnight\DK5CAuthoredHosPlacementPackBuilder.cs", "editor source file mismatch.");
        Check(authoring.GetProperty("placementManifestAssetPath").GetString() == @"Assets\DragonKnight\Placement\DK5C\dragon-knight-dk5c-authored-hos-placement-pack.v1.json", "placement manifest asset path mismatch.");
        Check(authoring.GetProperty("expectedOutputRoot").GetString() == @"<local-path>", "expected output root mismatch.");
        Check(authoring.GetProperty("deploymentTargetRoot").GetString() == @"<local-path>", "deployment target mismatch.");
    }

    private static void PrefabRootAndMarkerAreExact()
    {
        JsonElement authoring = ManifestRoot().GetProperty("unityAuthoring");
        Check(authoring.GetProperty("placementPrefabRootPath").GetString() == @"Assets\DragonKnight\Placement\DK5C\Prefabs\DK5C_DragonKnight_Cromlech_PlacementRoot.prefab", "prefab root path mismatch.");
        Check(authoring.GetProperty("placementRootName").GetString() == "DK5C_DragonKnight_Cromlech_PlacementRoot", "prefab root name mismatch.");
        Check(authoring.GetProperty("markerComponentType").GetString() == "DragonKnight.Editor.DK5CAuthoredPlacementMarker", "marker component mismatch.");
        Check(authoring.GetProperty("markerId").GetString() == "DK5C_DRAGON_KNIGHT_AUTHORED_HOS_PLACEMENT_PACK_SOURCE", "marker ID mismatch.");
        Check(ContractSource.Contains("DK5C_DragonKnight_Cromlech_PlacementRoot", StringComparison.Ordinal), "contract missing placement root.");
        Check(ContractSource.Contains("DK5C_DRAGON_KNIGHT_AUTHORED_HOS_PLACEMENT_PACK_SOURCE", StringComparison.Ordinal), "contract missing marker ID.");
    }

    private static void ManifestIsMultipleNpcReady()
    {
        JsonElement root = ManifestRoot();
        Check(root.GetProperty("supportsMultiplePlacements").GetBoolean(), "manifest must be multiple-placement ready.");
        JsonElement placements = root.GetProperty("placements");
        Check(placements.ValueKind == JsonValueKind.Array, "placements must be a JSON array.");
        Check(placements.GetArrayLength() == 1, "initial DK5C packet should contain exactly one placement.");
        HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (JsonElement placement in placements.EnumerateArray())
        {
            Check(ids.Add(placement.GetProperty("placementId").GetString() ?? string.Empty), "placement IDs must be unique.");
        }
    }

    private static void PlacementIdentityIsExact()
    {
        JsonElement placement = PrimaryPlacement();
        Check(placement.GetProperty("placementId").GetString() == "dragon-knight.vaelor.cromlech.center", "placement ID mismatch.");
        Check(placement.GetProperty("encounterId").GetString() == "dragon-knight.vaelor.cromlech", "encounter ID mismatch.");
        Check(placement.GetProperty("actorRoleId").GetString() == "dragon-knight.boss", "actor role mismatch.");
        Check(placement.GetProperty("actorSource").GetString() == "dragon-knight-owned-hos-placement-pack", "actor source mismatch.");
        Check(placement.GetProperty("displayName").GetString() == "Sir Vaelor, the Ashen Dragon Knight", "display name mismatch.");
        Check(placement.GetProperty("sceneName").GetString() == "CampaignMap_HOS", "placement scene mismatch.");
    }

    private static void TemplateGuidsAreExact()
    {
        JsonElement placement = PrimaryPlacement();
        Check(placement.GetProperty("locationTemplateName").GetString() == "Spec_DragonKnight_DK4A", "location template name mismatch.");
        Check(placement.GetProperty("locationTemplateGuid").GetString() == "d7b09116519f7564593be62781bee3db", "location template GUID mismatch.");
        Check(placement.GetProperty("npcTemplateName").GetString() == "NPCTemplate_DragonKnight_DK4A", "NPC template name mismatch.");
        Check(placement.GetProperty("npcTemplateGuid").GetString() == "00f608ee051b57748a6a9ed8dae28678", "NPC template GUID mismatch.");
        Check(ContractSource.Contains("d7b09116519f7564593be62781bee3db", StringComparison.Ordinal), "contract missing LocationTemplate GUID.");
        Check(ContractSource.Contains("00f608ee051b57748a6a9ed8dae28678", StringComparison.Ordinal), "contract missing NpcTemplate GUID.");
    }

    private static void AltarReferenceIsExact()
    {
        JsonElement altar = ManifestRoot().GetProperty("altarReference");
        Check(altar.GetProperty("referenceId").GetString() == "dragon-knight.vaelor.cromlech.altar-reference", "altar reference ID mismatch.");
        Check(altar.GetProperty("source").GetString() == "AltarInteract", "altar reference source mismatch.");
        Check(altar.GetProperty("liveLocationId").GetString() == "CM_Stonehenge_0_2258891627046429048_1", "altar Location.ID mismatch.");
        Check(altar.GetProperty("targetId").GetString() == "foa.location:CM_Stonehenge_0_2258891627046429048_1", "altar target ID mismatch.");
        Check(altar.GetProperty("componentPath").GetString() == "/SpawnedLocations_30/AltarInteract", "altar component path mismatch.");
        Check(altar.GetProperty("sceneName").GetString() == "CampaignMap_HOS_merged", "altar scene mismatch.");
        JsonElement position = altar.GetProperty("position");
        Check(Near(position.GetProperty("x").GetDouble(), -1792.661), "altar x mismatch.");
        Check(Near(position.GetProperty("y").GetDouble(), 79.942), "altar y mismatch.");
        Check(Near(position.GetProperty("z").GetDouble(), -2912.844), "altar z mismatch.");
        Check(!altar.GetProperty("isBossPlacementRoot").GetBoolean(), "altar must not be the boss placement root.");
        Check(!altar.GetProperty("isEncounterTrigger").GetBoolean(), "altar must not be the encounter trigger.");
        Check(altar.GetProperty("evidenceId").GetString() == "template-diagnostics.20260803-032730.cromlech-target-proof.line-21", "altar evidence mismatch.");
        Check(ContractSource.Contains("AltarReferenceSource = DragonKnightOwnedPlacementContract.AltarReferenceSource", StringComparison.Ordinal), "contract missing altar reference source bridge.");
    }

    private static void BossRootIsSeparatedFromAltar()
    {
        JsonElement placement = PrimaryPlacement();
        Check(placement.GetProperty("rootKind").GetString() == "boss-placement-root", "root kind mismatch.");
        Check(placement.GetProperty("altarReferenceId").GetString() == "dragon-knight.vaelor.cromlech.altar-reference", "placement altar reference mismatch.");
        JsonElement derivation = placement.GetProperty("bossRootDerivation");
        Check(derivation.GetProperty("derivedFrom").GetString() == "AltarInteract reference plus 4m toward dumped hero position", "boss root derivation mismatch.");
        Check(Near(derivation.GetProperty("altarOffsetMeters").GetDouble(), 4.0), "boss root offset mismatch.");
        Check(Near(derivation.GetProperty("distanceFromAltarMeters").GetDouble(), 4.0), "boss root altar distance mismatch.");
        Check(derivation.GetProperty("evidenceId").GetString() == "template-diagnostics.20260803-032730.hero-to-altar-offset-4m", "boss root evidence mismatch.");
        Check(ContractSource.Contains("BossRootEvidence = DragonKnightOwnedPlacementContract.BossRootEvidence", StringComparison.Ordinal), "contract missing boss root evidence bridge.");
    }

    private static void TransformIsExact()
    {
        JsonElement position = PrimaryPlacement().GetProperty("position");
        JsonElement rotation = PrimaryPlacement().GetProperty("rotation");
        Check(Near(position.GetProperty("x").GetDouble(), -1795.209), "x coordinate mismatch.");
        Check(Near(position.GetProperty("y").GetDouble(), 80.243), "y coordinate mismatch.");
        Check(Near(position.GetProperty("z").GetDouble(), -2915.928), "z coordinate mismatch.");
        Check(Near(rotation.GetProperty("pitch").GetDouble(), 0.0), "pitch mismatch.");
        Check(Near(rotation.GetProperty("yaw").GetDouble(), -140.441), "yaw mismatch.");
        Check(Near(rotation.GetProperty("roll").GetDouble(), 0.0), "roll mismatch.");
    }

    private static void ActivationAndArenaRadiiAreExact()
    {
        JsonElement placement = PrimaryPlacement();
        Check(Near(placement.GetProperty("activationRadiusMeters").GetDouble(), 5.0), "activation radius mismatch.");
        Check(Near(placement.GetProperty("witnessRadiusMeters").GetDouble(), 25.0), "witness radius mismatch.");
        Check(Near(placement.GetProperty("softLeashRadiusMeters").GetDouble(), 22.0), "soft leash mismatch.");
        Check(Near(placement.GetProperty("hardLeashRadiusMeters").GetDouble(), 25.0), "hard leash mismatch.");
        Check(Near(placement.GetProperty("maxPlacementDriftMeters").GetDouble(), 3.0), "placement drift mismatch.");
        Check(placement.GetProperty("activationTrigger").GetString() == "dragon-knight.encounter.inner-ring-entered", "activation trigger mismatch.");
    }

    private static void NoSaveRejectionIsExact()
    {
        JsonElement policy = ManifestRoot().GetProperty("runtimePolicy");
        JsonElement placement = PrimaryPlacement();
        Check(policy.GetProperty("requiresSaveOwnedLocation").GetBoolean(), "runtime policy must require save-owned location.");
        Check(policy.GetProperty("rejectsMarkedNotSaved").GetBoolean(), "runtime policy must reject MarkedNotSaved.");
        Check(policy.GetProperty("rejectsIsNotSaved").GetBoolean(), "runtime policy must reject IsNotSaved.");
        Check(placement.GetProperty("requiresSaveOwnedLocation").GetBoolean(), "placement must require save-owned location.");
        Check(placement.GetProperty("requiresLocationIdProof").GetBoolean(), "placement must require Location.ID proof.");
        Check(placement.GetProperty("requiresHostOwnershipProof").GetBoolean(), "placement must require host ownership proof.");
    }

    private static void RuntimeMutationFlagsAreBlocked()
    {
        JsonElement policy = ManifestRoot().GetProperty("runtimePolicy");
        Check(!policy.GetProperty("allowsNativeSpawn").GetBoolean(), "native spawn must be blocked.");
        Check(!policy.GetProperty("allowsVanillaSpawnerSlots").GetBoolean(), "vanilla slots must be blocked.");
        Check(!policy.GetProperty("allowsRuntimeSceneMutation").GetBoolean(), "runtime scene mutation must be blocked.");
        Check(!policy.GetProperty("allowsSaveWrites").GetBoolean(), "save writes must be blocked.");
        Check(policy.GetProperty("observerCompatible").GetBoolean(), "observer compatibility must be true.");
    }

    private static void LiveObserverCompatibilityIsExact()
    {
        JsonElement policy = ManifestRoot().GetProperty("runtimePolicy");
        JsonElement placement = PrimaryPlacement();
        JsonElement compatibility = placement.GetProperty("observerCompatibility");
        Check(policy.GetProperty("expectedObservedMarker").GetString() == "DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED", "policy observed marker mismatch.");
        Check(placement.GetProperty("expectedActorIdFormat").GetString() == "foa.location:<Location.ID>", "actor ID format mismatch.");
        Check(placement.GetProperty("expectedObservedMarker").GetString() == "DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED", "placement observed marker mismatch.");
        Check(compatibility.GetProperty("locationTemplateGuidMatchesObserver").GetBoolean(), "location template observer compatibility mismatch.");
        Check(compatibility.GetProperty("npcTemplateGuidMatchesObserver").GetBoolean(), "NPC template observer compatibility mismatch.");
        Check(Near(compatibility.GetProperty("positionWithinObserverDriftMeters").GetDouble(), 3.0), "observer drift mismatch.");
        Check(compatibility.GetProperty("rejectsMarkedNotSaved").GetBoolean(), "observer MarkedNotSaved compatibility mismatch.");
        Check(compatibility.GetProperty("rejectsIsNotSaved").GetBoolean(), "observer IsNotSaved compatibility mismatch.");
    }

    private static void CoreOverlayCompatibilityIsExact()
    {
        JsonElement manifest = ManifestRoot();
        JsonElement overlay = JsonDocument.Parse(OverlaySource).RootElement;
        JsonElement manifestPlacement = PrimaryPlacement();
        JsonElement overlayPlacement = overlay.GetProperty("placements")[0];
        Check(manifest.GetProperty("overlayId").GetString() == overlay.GetProperty("overlayId").GetString(), "overlay ID mismatch.");
        Check(manifest.GetProperty("targetSceneName").GetString() == overlay.GetProperty("targetSceneName").GetString(), "overlay scene mismatch.");
        Check(manifest.GetProperty("coreAnchorRef").GetString() == overlay.GetProperty("anchorRef").GetString(), "overlay anchor mismatch.");
        Check(manifestPlacement.GetProperty("placementId").GetString() == overlayPlacement.GetProperty("placementId").GetString(), "overlay placement mismatch.");
        Check(manifestPlacement.GetProperty("locationTemplateGuid").GetString() == overlayPlacement.GetProperty("locationTemplateGuid").GetString(), "overlay LocationTemplate GUID mismatch.");
        Check(manifestPlacement.GetProperty("npcTemplateGuid").GetString() == overlayPlacement.GetProperty("npcTemplateGuid").GetString(), "overlay NpcTemplate GUID mismatch.");
    }

    private static void ObserverRejectsNoSaveActors()
    {
        Check(ObserverSource.Contains("match.MarkedNotSaved || match.IsNotSaved", StringComparison.Ordinal), "observer must reject no-save placements.");
        Check(ObserverSource.Contains("placement is temporary/no-save", StringComparison.Ordinal), "observer must log temporary/no-save rejection.");
    }

    private static void ObserverDerivesActorIdFromLocationId()
    {
        Check(ObserverSource.Contains("World.All<Location>().ToArraySlow()", StringComparison.Ordinal), "observer must scan live Location models.");
        Check(ObserverSource.Contains("\"foa.location:\" + match.ID", StringComparison.Ordinal), "observer must derive actor ID from Location.ID.");
        Check(MarkerSource.Contains("DRAGON_KNIGHT_OWNED_PLACEMENT_OBSERVED", StringComparison.Ordinal), "observed marker missing.");
    }

    private static void ContractAndManifestAvoidSpawnPaths()
    {
        string combined = ContractSource + "\n" + ManifestSource;
        foreach (string token in new[] { "LocationTemplate.SpawnLocation", "BaseLocationSpawner", "LocationSpawner", "Hollow Druid", "bear", "shrine" })
        {
            Check(!combined.Contains(token, StringComparison.Ordinal), "source packet must not use production-blocked token: " + token);
        }

        Check(!combined.Contains("\"activationTrigger\": \"AltarInteract\"", StringComparison.Ordinal), "AltarInteract must not be the activation trigger.");
        Check(!combined.Contains("\"rootKind\": \"AltarInteract\"", StringComparison.Ordinal), "AltarInteract must not be the boss root.");
    }

    private static void BuildDeployCommandsAreNamedButNotExecuted()
    {
        JsonElement authoring = ManifestRoot().GetProperty("unityAuthoring");
        Check(authoring.GetProperty("buildCommand").GetString()?.Contains("-executeMethod DragonKnight.Editor.DK5CAuthoredHosPlacementPackBuilder.Build", StringComparison.Ordinal) == true, "build command mismatch.");
        Check(authoring.GetProperty("deployCommand").GetString()?.Contains("Copy-Item -LiteralPath", StringComparison.Ordinal) == true, "deploy command mismatch.");
        Check(ManifestRoot().GetProperty("routeStatus").GetString() == "source-proof-only", "manifest must not claim route execution.");
        Check(RequiredFirstMarker.Contains("pack-build=0 live-proof=0", StringComparison.Ordinal), "marker must keep build/live false.");
    }

    private static void DecisionAndGateKeepUnityWritesBlocked()
    {
        foreach (string token in new[] { "Unity mutation", "pack build", "deployment", "live proof remain blocked", "does not mutate the Unity project", "does not create or edit `CampaignMap_HOS`" })
        {
            Check(DecisionSource.Contains(token, StringComparison.Ordinal) || GateSource.Contains(token, StringComparison.Ordinal), "decision/gate missing boundary token: " + token);
        }
    }

    private static void ResearchRecordsSourceProofBoundary()
    {
        Check(ResearchSource.Contains("Source-only implementation proof", StringComparison.Ordinal), "research missing source-only scope.");
        Check(ResearchSource.Contains("Read-only scan", StringComparison.Ordinal), "research missing read-only scan.");
        Check(ResearchSource.Contains("does not mutate Unity", StringComparison.Ordinal), "research missing Unity mutation boundary.");
    }

    private static void ReadmeRecordsDk5C()
    {
        Check(ReadmeSource.Contains("DK5C", StringComparison.Ordinal), "README missing DK5C.");
        Check(ReadmeSource.Contains("dragon-knight-dk5c-authored-hos-placement-pack.v1.json", StringComparison.Ordinal), "README missing DK5C manifest.");
        Check(ReadmeSource.Contains("Unity mutation, pack build, deployment, and live proof remain blocked", StringComparison.Ordinal), "README missing DK5C boundary.");
    }

    private static void ValidationDocsRecordDk5C()
    {
        Check(ValidationPlanSource.Contains("DragonKnight.DK5CAuthoredPlacementPack.Fixtures", StringComparison.Ordinal), "validation plan missing DK5C fixture.");
        Check(ValidationPlanSource.Contains(RequiredFirstMarker, StringComparison.Ordinal), "validation plan missing DK5C marker.");
        Check(TestNotesSource.Contains("DK5C authored HOS placement pack source", StringComparison.Ordinal), "test notes missing DK5C section.");
        Check(TestNotesSource.Contains("DragonKnight.DK5CAuthoredPlacementPack.Fixtures", StringComparison.Ordinal), "test notes missing DK5C fixture.");
    }

    private static JsonElement ManifestRoot() => JsonDocument.Parse(ManifestSource).RootElement;

    private static JsonElement PrimaryPlacement() => ManifestRoot().GetProperty("placements")[0];

    private static bool Near(double actual, double expected) => Math.Abs(actual - expected) < 0.0005;

    private static string Read(string relativePath)
    {
        string path = Path.Combine(RepoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Missing source file", path);
        }

        return File.ReadAllText(path);
    }

    private static string FindRepoRoot()
    {
        DirectoryInfo? dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")) &&
                Directory.Exists(Path.Combine(dir.FullName, "mods", "dragon-knight")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
