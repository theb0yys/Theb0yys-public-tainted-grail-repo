using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

internal static class Program
{
    private const string RequiredFirstMarker =
        "DRAGON_KNIGHT_OWNED_PLACEMENT_SOURCE_GATE_PASS fixtures=32 owner=dragon-knight.boss.host actor-source=native-location-spawner-candidate:hos-cromlech-grindylow01-native-slot scene=CampaignMap_HOS placements=1 multi-npc-ready=1 altar-ref=1 boss-root=1 altar-trigger=0 native-spawner=1 native-candidate=1 direct-spawn=0 relocated-to-boss-root=1 goals=0 actions=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save-write=0";

    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string SourceRoot = Path.Combine(RepoRoot, "mods", "dragon-knight", "src");
    private static readonly string ContractSource = Read("mods/dragon-knight/src/Encounter/DragonKnightOwnedPlacementContract.cs");
    private static readonly string ObserverSource = Read("mods/dragon-knight/src/Encounter/DragonKnightOwnedPlacementObserver.cs");
    private static readonly string MarkerSource = Read("mods/dragon-knight/src/Encounter/DragonKnightOwnedPlacementMarker.cs");
    private static readonly string NativeContractSource = Read("mods/dragon-knight/src/Encounter/DragonKnightNativeSpawnerPlacementContract.cs");
    private static readonly string NativePatchSource = Read("mods/dragon-knight/src/Encounter/DragonKnightNativeSpawnerPlacementPatchPlan.cs");
    private static readonly string NativeSystemSource = Read("mods/dragon-knight/src/Encounter/DragonKnightNativeSpawnerPlacementSystem.cs");
    private static readonly string PluginSource = Read("mods/dragon-knight/src/Plugin.cs");
    private static readonly string ProjectSource = Read("mods/dragon-knight/src/DragonKnight.csproj");
    private static readonly string ReadmeSource = Read("mods/dragon-knight/README.md");
    private static readonly string DesignSource = Read("mods/dragon-knight/docs/design.md");

    private static int Main()
    {
        Console.WriteLine(RequiredFirstMarker);

        var fixtures = new (string Name, Action Run)[]
        {
            ("fixture count is exact", FixtureCountIsExact),
            ("source marker is exact", SourceMarkerIsExact),
            ("plugin version advanced", PluginVersionAdvanced),
            ("plugin wires owned observer", PluginWiresOwnedObserver),
            ("observer is read only", ObserverIsReadOnly),
            ("production path uses native candidate route", ProductionPathUsesNativeCandidateRoute),
            ("native patch plan hooks exact methods", NativePatchPlanHooksExactMethods),
            ("native source slot is exact", NativeSourceSlotIsExact),
            ("native route default config is exact", NativeRouteDefaultConfigIsExact),
            ("candidate replacement is deterministic", CandidateReplacementIsDeterministic),
            ("candidate route relocates to boss root", CandidateRouteRelocatesToBossRoot),
            ("candidate route keeps native save ownership", CandidateRouteKeepsNativeSaveOwnership),
            ("candidate route restores native array", CandidateRouteRestoresNativeArray),
            ("production path rejects external runtime dependencies", ProductionPathRejectsExternalRuntimeDependencies),
            ("scene is HOS", SceneIsHos),
            ("arena is Ancient Cromlech", ArenaIsAncientCromlech),
            ("template GUIDs are exact", TemplateGuidsAreExact),
            ("placement id is exact", PlacementIdIsExact),
            ("multiple NPC manifest shape exists", MultipleNpcManifestShapeExists),
            ("altar reference is exact", AltarReferenceIsExact),
            ("boss root is separated from altar", BossRootIsSeparatedFromAltar),
            ("Cromlech coordinates are exact", CromlechCoordinatesAreExact),
            ("edge witness coordinates are exact", EdgeWitnessCoordinatesAreExact),
            ("wake and fight radii are exact", WakeAndFightRadiiAreExact),
            ("leash radii are exact", LeashRadiiAreExact),
            ("observer scans live locations", ObserverScansLiveLocations),
            ("observer derives actor id from Location.ID", ObserverDerivesActorIdFromLocationId),
            ("observer requires NpcElement", ObserverRequiresNpcElement),
            ("observer rejects temporary no-save placements", ObserverRejectsTemporaryNoSavePlacements),
            ("observed marker includes ownership fields", ObservedMarkerIncludesOwnershipFields),
            ("docs say DK4 is not production route", DocsSayDk4IsNotProductionRoute),
            ("docs keep gameplay systems blocked", DocsKeepGameplaySystemsBlocked),
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
            Console.WriteLine("All " + fixtures.Length + " Dragon Knight owned-placement fixtures passed.");
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
        Check(ContractSource.Contains("internal const int FixtureCount = 32;", StringComparison.Ordinal), "fixture count must stay 32.");

    private static void SourceMarkerIsExact() =>
        Check(MarkerSource.Contains(RequiredFirstMarker, StringComparison.Ordinal), "source gate marker changed.");

    private static void PluginVersionAdvanced() =>
        Check(PluginSource.Contains("PluginVersion = \"0.2.5\"", StringComparison.Ordinal), "plugin version must be 0.2.5 for DK5D native placement.");

    private static void PluginWiresOwnedObserver()
    {
        Check(PluginSource.Contains("DragonKnightOwnedPlacementObserver?", StringComparison.Ordinal), "plugin must hold owned placement observer.");
        Check(PluginSource.Contains("new DragonKnightOwnedPlacementObserver(Logger, Config)", StringComparison.Ordinal), "plugin must construct owned placement observer.");
        Check(PluginSource.Contains("_ownedPlacementObserver?.Tick();", StringComparison.Ordinal), "plugin must tick owned placement observer.");
        Check(PluginSource.Contains("DragonKnightNativeSpawnerPlacementSystem?", StringComparison.Ordinal), "plugin must hold native placement system.");
        Check(PluginSource.Contains("new DragonKnightNativeSpawnerPlacementSystem(Logger, Config)", StringComparison.Ordinal), "plugin must construct native placement system.");
        Check(PluginSource.Contains("DragonKnightNativeSpawnerPlacementPatchPlan.Apply(_harmony, Logger)", StringComparison.Ordinal), "plugin must apply native placement patches.");
        Check(PluginSource.Contains("_nativeSpawnerPlacementSystem?.Tick();", StringComparison.Ordinal), "plugin must tick native placement system.");
        Check(PluginSource.Contains("DK4 temporary live actor diagnostic remains default-off and is not the production placement route", StringComparison.Ordinal), "loader log must identify DK4 as non-production placement route.");
    }

    private static void ObserverIsReadOnly()
    {
        Check(!ObserverSource.Contains("Instantiate(", StringComparison.Ordinal), "owned observer must not instantiate objects.");
        Check(!ObserverSource.Contains(".Discard(", StringComparison.Ordinal), "owned observer must not discard objects.");
        Check(!ObserverSource.Contains(".MarkedNotSaved =", StringComparison.Ordinal), "owned observer must not mutate Location save flags.");
        Check(!ObserverSource.Contains(".IsNotSaved =", StringComparison.Ordinal), "owned observer must not mutate Location save flags.");
    }

    private static void ProductionPathUsesNativeCandidateRoute()
    {
        string source = ContractSource + ObserverSource + MarkerSource + NativeContractSource + NativePatchSource + NativeSystemSource + PluginSource;
        Check(source.Contains("native-location-spawner-candidate:hos-cromlech-grindylow01-native-slot", StringComparison.Ordinal), "actor source must name native candidate route.");
        Check(source.Contains("native-spawner=1", StringComparison.Ordinal), "markers/logs must state native-spawner=1.");
        Check(source.Contains("native-candidate=1", StringComparison.Ordinal), "markers/logs must state native-candidate=1.");
        Check(source.Contains("direct-spawn=0", StringComparison.Ordinal), "markers/logs must state direct-spawn=0.");
        Check(!NativeSystemSource.Contains(".SpawnLocation(", StringComparison.Ordinal), "native placement system must not call SpawnLocation directly.");
        Check(!NativeSystemSource.Contains("SpawnPrefab(", StringComparison.Ordinal), "native placement system must not force SpawnPrefab directly.");
    }

    private static void NativePatchPlanHooksExactMethods()
    {
        Check(ProjectSource.Contains("<Reference Include=\"0Harmony\">", StringComparison.Ordinal), "project must reference 0Harmony.");
        Check(NativePatchSource.Contains("LocationSpawner.InitFromAttachment", StringComparison.Ordinal), "patch plan must hook LocationSpawner.InitFromAttachment.");
        Check(NativePatchSource.Contains("BaseLocationSpawner", StringComparison.Ordinal), "patch plan must observe BaseLocationSpawner.");
        Check(NativePatchSource.Contains("OnLocationSpawned", StringComparison.Ordinal), "patch plan must hook OnLocationSpawned.");
        Check(NativePatchSource.Contains("RegisterDragonKnightNativeSpawner", StringComparison.Ordinal), "patch plan must register native spawners.");
        Check(NativePatchSource.Contains("ObserveDragonKnightNativeSpawn", StringComparison.Ordinal), "patch plan must observe native Dragon Knight spawns.");
    }

    private static void NativeSourceSlotIsExact()
    {
        Check(NativeContractSource.Contains("NativeSpawnerSceneName = \"CampaignMap_HOS_merged\"", StringComparison.Ordinal), "native spawner scene mismatch.");
        Check(NativeContractSource.Contains("\"hos-cromlech-grindylow01-native-slot\"", StringComparison.Ordinal), "native anchor id mismatch.");
        Check(NativeContractSource.Contains("\"/SpawnerSingle_EnemyMonster_T1_Grindylow_01\"", StringComparison.Ordinal), "native source path mismatch.");
        Check(NativeContractSource.Contains("\"Spec_EnemyMonster_T1_Grindylow\"", StringComparison.Ordinal), "native source template mismatch.");
        Check(NativeContractSource.Contains("\"fa79aaa0bff59484dab2cf35c5ea805c\"", StringComparison.Ordinal), "native source template GUID mismatch.");
        Check(NativeContractSource.Contains("-1860.15f", StringComparison.Ordinal), "native source X mismatch.");
        Check(NativeContractSource.Contains("73.81f", StringComparison.Ordinal), "native source Y mismatch.");
        Check(NativeContractSource.Contains("-2958.34f", StringComparison.Ordinal), "native source Z mismatch.");
        Check(NativeContractSource.Contains("EvidenceId = \"template-diagnostics.20260803-035222.spawner_refs.csv:line341\"", StringComparison.Ordinal), "native evidence id mismatch.");
    }

    private static void CandidateReplacementIsDeterministic()
    {
        Check(NativeSystemSource.Contains("typeof(LocationSpawner).GetField(\"_locationsToSpawn\"", StringComparison.Ordinal), "native system must target LocationSpawner candidate field.");
        Check(NativeSystemSource.Contains("LocationTemplate[] updated = { dragonTemplate };", StringComparison.Ordinal), "native candidate route must replace with exact Dragon Knight template only.");
        Check(NativeContractSource.Contains("CandidatePolicy = \"replace-single-candidate\"", StringComparison.Ordinal), "candidate policy must stay replace-single-candidate.");
        Check(NativeSystemSource.Contains("DragonKnightDk4ActorObserver.TryResolveTemplate", StringComparison.Ordinal), "native system must resolve the proven DK4A template.");
        Check(NativeSystemSource.Contains("DragonKnightDk4ActorObserver.TryPrepareTemplateForSpawn", StringComparison.Ordinal), "native system must prepare the proven DK4A template.");
    }

    private static void NativeRouteDefaultConfigIsExact()
    {
        Check(NativeContractSource.Contains("ConfigSection = \"DragonKnightNativeSpawnerPlacement\"", StringComparison.Ordinal), "native config section mismatch.");
        Check(NativeContractSource.Contains("DefaultEnabled = true", StringComparison.Ordinal), "native route must be enabled by default after user override.");
        Check(NativeContractSource.Contains("DefaultKillSwitch = false", StringComparison.Ordinal), "native route kill switch must be open after user override.");
        Check(NativeSystemSource.Contains("\"Enabled\"", StringComparison.Ordinal), "native system must bind Enabled config.");
        Check(NativeSystemSource.Contains("\"KillSwitch\"", StringComparison.Ordinal), "native system must bind KillSwitch config.");
    }

    private static void CandidateRouteRelocatesToBossRoot()
    {
        Check(NativeSystemSource.Contains("DragonKnightOwnedPlacementContract.ArenaCenter", StringComparison.Ordinal), "native spawn must relocate to boss root.");
        Check(NativeSystemSource.Contains("BaseLocationSpawner.VerifyPosition(requested, location.Template, allowSnapToGround: false)", StringComparison.Ordinal), "native spawn must verify boss root position.");
        Check(NativeSystemSource.Contains("DragonKnightOwnedPlacementContract.FacingEdgeRotation()", StringComparison.Ordinal), "native spawn must face the edge witness.");
        Check(NativeSystemSource.Contains("location.MoveAndRotateTo(verified, rotation, teleport: true)", StringComparison.Ordinal), "native spawn must use Location.MoveAndRotateTo.");
        Check(MarkerSource.Contains("relocated-to-boss-root=1", StringComparison.Ordinal), "observed marker must state relocation.");
    }

    private static void CandidateRouteKeepsNativeSaveOwnership()
    {
        Check(NativeSystemSource.Contains("location.MarkedNotSaved || location.IsNotSaved", StringComparison.Ordinal), "native system must reject no-save native locations.");
        Check(!NativeSystemSource.Contains("MarkedNotSaved =", StringComparison.Ordinal), "native system must not force no-save.");
        Check(NativeSystemSource.Contains("save-owned=1", StringComparison.Ordinal), "native placement log must state save-owned=1.");
        Check(NativeSystemSource.Contains("saveRestoreOwner=BaseLocationSpawner", StringComparison.Ordinal), "native placement must report BaseLocationSpawner save/restore ownership.");
        Check(MarkerSource.Contains("save-owned=1", StringComparison.Ordinal), "observed marker must state save-owned=1.");
    }

    private static void CandidateRouteRestoresNativeArray()
    {
        Check(NativeSystemSource.Contains("RestoreInjectedSlots(\"gate-closed\")", StringComparison.Ordinal), "native system must restore candidate array when gate closes.");
        Check(NativeSystemSource.Contains("RestoreInjectedSlots(\"plugin-shutdown\")", StringComparison.Ordinal), "native system must restore candidate array on shutdown.");
        Check(NativeSystemSource.Contains("field.SetValue(slot.Spawner, slot.OriginalTemplates)", StringComparison.Ordinal), "native system must restore original templates.");
        Check(NativeSystemSource.Contains("active-native-locations-unchanged=1", StringComparison.Ordinal), "restore log must not discard active native locations.");
        Check(PluginSource.Contains("_harmony?.UnpatchSelf();", StringComparison.Ordinal), "plugin must unpatch Harmony on destroy.");
    }

    private static void ProductionPathRejectsExternalRuntimeDependencies()
    {
        string source = ContractSource + ObserverSource + MarkerSource + PluginSource;
        string[] forbidden =
        {
            "LivingAvalon",
            "WyrdHunt",
            "AvalonAwakened",
            "AvalonCompanions",
            "AvalonAI.Runtime",
            "AvalonAI.FoAHost",
        };

        foreach (string token in forbidden)
        {
            Check(!source.Contains(token, StringComparison.Ordinal), "Dragon Knight source must not depend on external runtime: " + token);
        }
    }

    private static void SceneIsHos()
    {
        Check(ContractSource.Contains("SceneName = DragonKnightAiV2Contract.SceneName", StringComparison.Ordinal), "scene must use AIR-49 scene constant.");
        Check(ContractSource.Contains("SceneDisplayName = \"Horns of the South\"", StringComparison.Ordinal), "scene display name changed.");
    }

    private static void ArenaIsAncientCromlech() =>
        Check(ContractSource.Contains("ArenaName = DragonKnightAiV2Contract.ArenaName", StringComparison.Ordinal), "arena must use AIR-49 Cromlech constant.");

    private static void TemplateGuidsAreExact()
    {
        Check(ContractSource.Contains("LocationTemplateGuid = DragonKnightAiV2Contract.LocationTemplateGuid", StringComparison.Ordinal), "location template GUID must use AIR-49 constant.");
        Check(ContractSource.Contains("NpcTemplateGuid = DragonKnightAiV2Contract.NpcTemplateGuid", StringComparison.Ordinal), "NPC template GUID must use AIR-49 constant.");
    }

    private static void PlacementIdIsExact() =>
        Check(ContractSource.Contains("PrimaryPlacementId = \"dragon-knight.vaelor.cromlech.center\"", StringComparison.Ordinal), "primary placement id changed.");

    private static void MultipleNpcManifestShapeExists()
    {
        Check(ContractSource.Contains("IReadOnlyList<DragonKnightOwnedNpcPlacement> Placements", StringComparison.Ordinal), "placements manifest must be list-shaped.");
        Check(ContractSource.Contains("DragonKnightOwnedNpcPlacement", StringComparison.Ordinal), "placement record type missing.");
        Check(ContractSource.Contains("PlacementId", StringComparison.Ordinal), "placement id field missing.");
        Check(ContractSource.Contains("NpcTemplateGuid", StringComparison.Ordinal), "npc template field missing.");
    }

    private static void AltarReferenceIsExact()
    {
        Check(ContractSource.Contains("AltarReferenceSource = \"AltarInteract\"", StringComparison.Ordinal), "altar reference source mismatch.");
        Check(ContractSource.Contains("AltarReferenceLiveLocationId = \"CM_Stonehenge_0_2258891627046429048_1\"", StringComparison.Ordinal), "altar Location.ID mismatch.");
        Check(ContractSource.Contains("AltarReferenceTargetId = \"foa.location:CM_Stonehenge_0_2258891627046429048_1\"", StringComparison.Ordinal), "altar target ID mismatch.");
        Check(ContractSource.Contains("AltarReferenceComponentPath = \"/SpawnedLocations_30/AltarInteract\"", StringComparison.Ordinal), "altar component path mismatch.");
        Check(ContractSource.Contains("AltarReferenceX = -1792.661f", StringComparison.Ordinal), "altar X mismatch.");
        Check(ContractSource.Contains("AltarReferenceY = 79.942f", StringComparison.Ordinal), "altar Y mismatch.");
        Check(ContractSource.Contains("AltarReferenceZ = -2912.844f", StringComparison.Ordinal), "altar Z mismatch.");
        Check(ContractSource.Contains("isBossPlacementRoot: false", StringComparison.Ordinal), "altar must not be boss placement root.");
        Check(ContractSource.Contains("isEncounterTrigger: false", StringComparison.Ordinal), "altar must not be encounter trigger.");
    }

    private static void BossRootIsSeparatedFromAltar()
    {
        Check(ContractSource.Contains("BossRootEvidence = \"template-diagnostics.20260803-032730.hero-to-altar-offset-4m\"", StringComparison.Ordinal), "boss root evidence mismatch.");
        Check(ContractSource.Contains("BossRootYawDegrees = -140.441f", StringComparison.Ordinal), "boss root yaw mismatch.");
        Check(!ContractSource.Contains("internal const float ArenaCenterX = -1792.661f", StringComparison.Ordinal), "arena center must not remain on AltarInteract X.");
        Check(!ContractSource.Contains("internal const float ArenaCenterY = 79.942f", StringComparison.Ordinal), "arena center must not remain on AltarInteract Y.");
        Check(!ContractSource.Contains("internal const float ArenaCenterZ = -2912.844f", StringComparison.Ordinal), "arena center must not remain on AltarInteract Z.");
    }

    private static void CromlechCoordinatesAreExact()
    {
        Check(ContractSource.Contains("ArenaCenterX = -1795.209f", StringComparison.Ordinal), "arena X changed.");
        Check(ContractSource.Contains("ArenaCenterY = 80.243f", StringComparison.Ordinal), "arena Y changed.");
        Check(ContractSource.Contains("ArenaCenterZ = -2915.928f", StringComparison.Ordinal), "arena Z changed.");
    }

    private static void EdgeWitnessCoordinatesAreExact()
    {
        Check(ContractSource.Contains("EdgeWitnessX = -1808.181f", StringComparison.Ordinal), "edge X changed.");
        Check(ContractSource.Contains("EdgeWitnessY = 82.621f", StringComparison.Ordinal), "edge Y changed.");
        Check(ContractSource.Contains("EdgeWitnessZ = -2931.862f", StringComparison.Ordinal), "edge Z changed.");
    }

    private static void WakeAndFightRadiiAreExact()
    {
        Check(ContractSource.Contains("OuterWakeRadiusMeters = DragonKnightAiV2Contract.OuterWakeRadiusMeters", StringComparison.Ordinal), "outer wake radius must use AIR-49 constant.");
        Check(ContractSource.Contains("InnerFightStartRadiusMeters = DragonKnightAiV2Contract.InnerFightStartRadiusMeters", StringComparison.Ordinal), "inner fight radius must use AIR-49 constant.");
    }

    private static void LeashRadiiAreExact()
    {
        Check(ContractSource.Contains("SoftLeashRadiusMeters = DragonKnightAiV2Contract.SoftLeashRadiusMeters", StringComparison.Ordinal), "soft leash radius must use AIR-49 constant.");
        Check(ContractSource.Contains("HardLeashRadiusMeters = DragonKnightAiV2Contract.HardLeashRadiusMeters", StringComparison.Ordinal), "hard leash radius must use AIR-49 constant.");
    }

    private static void ObserverScansLiveLocations()
    {
        Check(ObserverSource.Contains("World.All<Location>().ToArraySlow()", StringComparison.Ordinal), "observer must scan live Location models.");
        Check(ObserverSource.Contains("SceneManager.GetSceneByName(DragonKnightOwnedPlacementContract.SceneName)", StringComparison.Ordinal), "observer must require HOS loaded.");
        Check(ObserverSource.Contains("location.Template?.GUID", StringComparison.Ordinal), "observer must match LocationTemplate GUID.");
        Check(ObserverSource.Contains("Vector3.Distance(location.Coords, placement.Position)", StringComparison.Ordinal), "observer must match placement distance.");
    }

    private static void ObserverDerivesActorIdFromLocationId() =>
        Check(ObserverSource.Contains("\"foa.location:\" + match.ID", StringComparison.Ordinal), "actor id must derive from live Location.ID.");

    private static void ObserverRequiresNpcElement()
    {
        Check(ObserverSource.Contains("match.TryGetElement(out NpcElement npc)", StringComparison.Ordinal), "observer must require NpcElement.");
        Check(ObserverSource.Contains("npc.Template?.GUID", StringComparison.Ordinal), "observer must verify NPC template GUID.");
    }

    private static void ObserverRejectsTemporaryNoSavePlacements()
    {
        Check(ObserverSource.Contains("match.MarkedNotSaved || match.IsNotSaved", StringComparison.Ordinal), "observer must reject temporary/no-save placements.");
        Check(MarkerSource.Contains("save-owned=1", StringComparison.Ordinal), "observed marker must state save-owned=1.");
    }

    private static void ObservedMarkerIncludesOwnershipFields()
    {
        string[] fields =
        {
            "owner=",
            "encounter=",
            "placement=",
            "actor-role=",
            "actor-source=",
            "actor-location-id=",
            "actor-id=",
            "scene=",
            "location-template=",
            "npc-template=",
            "activation-trigger=",
            "custom-owned=1 native-spawner=1 native-candidate=1 direct-spawn=0 relocated-to-boss-root=1 save-owned=1",
        };

        foreach (string field in fields)
        {
            Check(MarkerSource.Contains(field, StringComparison.Ordinal), "observed marker missing " + field);
        }
    }

    private static void DocsSayDk4IsNotProductionRoute()
    {
        Check(ReadmeSource.Contains("Dragon Knight-owned HOS placement observer", StringComparison.Ordinal), "README must mention owned HOS placement observer.");
        Check(ReadmeSource.Contains("DK4 remains available only as the older temporary diagnostic and is not the production placement route", StringComparison.Ordinal), "README must reject DK4 as production route.");
        Check(DesignSource.Contains("approved DK5D native `LocationSpawner` candidate route", StringComparison.Ordinal), "design must define native candidate route.");
        Check(DesignSource.Contains("must not call `LocationTemplate.SpawnLocation` directly", StringComparison.Ordinal), "design must reject direct SpawnLocation.");
    }

    private static void DocsKeepGameplaySystemsBlocked()
    {
        string combined = ReadmeSource + DesignSource;
        string required = "movement, attacks, phase combat, companion protection, items";
        Check(combined.Contains(required, StringComparison.Ordinal), "docs must keep gameplay systems blocked.");
    }

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
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "mods", "dragon-knight", "src", "Plugin.cs")))
            {
                return current.FullName;
            }

            current = current.Parent;
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
