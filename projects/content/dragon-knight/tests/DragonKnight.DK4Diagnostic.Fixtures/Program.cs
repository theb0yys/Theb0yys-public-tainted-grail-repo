using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

internal static class Program
{
    private const string RequiredFirstMarker =
        "DRAGON_KNIGHT_DK4_SOURCE_GATE_BLOCKED fixtures=36 owner=dragon-knight.dk4.diagnostic-host actor-role=dragon-knight.boss actor-source=blocked actor-location-id=blocked actor-id=blocked target-source=blocked target-location-id=blocked activation=F7 release=F6 default-off=1 kill-switch-default=1 native-spawn=0 goals=0 actions=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save=0";

    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string SourceRoot = Path.Combine(RepoRoot, "mods", "dragon-knight", "src");
    private static readonly string HostSource = Read("mods/dragon-knight/src/DK4/DragonKnightDk4DiagnosticHost.cs");
    private static readonly string OptionsSource = Read("mods/dragon-knight/src/DK4/DragonKnightDk4DiagnosticOptions.cs");
    private static readonly string ObserverSource = Read("mods/dragon-knight/src/DK4/DragonKnightDk4ActorObserver.cs");
    private static readonly string MarkerSource = Read("mods/dragon-knight/src/DK4/DragonKnightDk4Marker.cs");
    private static readonly string LeaseSource = Read("mods/dragon-knight/src/DK4/DragonKnightDk4OwnershipLease.cs");
    private static readonly string PluginSource = Read("mods/dragon-knight/src/Plugin.cs");
    private static readonly string ProjectSource = Read("mods/dragon-knight/src/DragonKnight.csproj");

    private static int Main()
    {
        Console.WriteLine(RequiredFirstMarker);

        var fixtures = new (string Name, Action Run)[]
        {
            ("fixture count is exact", FixtureCountIsExact),
            ("default enabled is false", DefaultEnabledIsFalse),
            ("default kill switch is true", DefaultKillSwitchIsTrue),
            ("native spawn defaults closed", NativeSpawnDefaultsClosed),
            ("activation and release hotkeys are exact", HotkeysAreExact),
            ("maximum observation seconds is exact", MaximumObservationSecondsIsExact),
            ("owner identity is exact", OwnerIdentityIsExact),
            ("actor role identity is exact", ActorRoleIdentityIsExact),
            ("actor source is spawnlocation template", ActorSourceIsSpawnLocationTemplate),
            ("actor id format is live Location.ID", ActorIdFormatIsLiveLocationId),
            ("target source is exact altar Location", TargetSourceIsExactAltarLocation),
            ("target id format is live Location.ID", TargetIdFormatIsLiveLocationId),
            ("target source is not used as trigger", TargetSourceIsNotUsedAsTrigger),
            ("scene is CampaignMap_HOS", SceneIsCampaignMapHos),
            ("template GUIDs are exact", TemplateGuidsAreExact),
            ("native visual address is exact", NativeVisualAddressIsExact),
            ("Cromlech placement coordinates are exact", CromlechPlacementCoordinatesAreExact),
            ("plugin wires DK4 host without startup spawn", PluginWiresHostWithoutStartupSpawn),
            ("F7 does nothing while disabled", F7DoesNothingWhileDisabled),
            ("F7 blocks while kill switch true", F7BlocksWhileKillSwitchTrue),
            ("F7 blocks while AllowNativeSpawn false", F7BlocksWhileAllowNativeSpawnFalse),
            ("duplicate active actor is blocked", DuplicateActiveActorIsBlocked),
            ("template resolves through TemplateReference", TemplateResolvesThroughTemplateReference),
            ("template component order is guarded", TemplateComponentOrderIsGuarded),
            ("native visual is prepared before spawn", NativeVisualIsPreparedBeforeSpawn),
            ("target proof scans live Location models", TargetProofScansLiveLocations),
            ("target proof uses observed Location.ID", TargetProofUsesObservedLocationId),
            ("SpawnLocation is isolated to DK4 host", SpawnLocationIsIsolatedToDk4Host),
            ("native VerifyPosition is used", NativeVerifyPositionIsUsed),
            ("save exclusion is immediate and read back", SaveExclusionIsImmediateAndReadBack),
            ("Location initialization callback is required", LocationInitializationCallbackIsRequired),
            ("NpcElement validation is required", NpcElementValidationIsRequired),
            ("release discards the proof Location", ReleaseDiscardsProofLocation),
            ("pass marker contains all required fields", PassMarkerContainsAllRequiredFields),
            ("unsupported gameplay fields stay zero", UnsupportedGameplayFieldsStayZero),
            ("DK4 source has no forbidden AI/runtime bypass references", SourceHasNoForbiddenBypassReferences),
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
            Console.WriteLine("All " + fixtures.Length + " Dragon Knight DK4 diagnostic fixtures passed.");
            return 0;
        }

        Console.WriteLine(failures.Count + " fixture(s) failed:");
        foreach (string failure in failures)
        {
            Console.WriteLine(" - " + failure);
        }

        return 1;
    }

    private static void FixtureCountIsExact() => Check(OptionsSource.Contains("internal const int FixtureCount = 36;", StringComparison.Ordinal), "DK4 fixture count must stay 36.");

    private static void DefaultEnabledIsFalse() => Check(OptionsSource.Contains("internal const bool DefaultEnabled = false;", StringComparison.Ordinal), "DK4 must be default-off.");

    private static void DefaultKillSwitchIsTrue() => Check(OptionsSource.Contains("internal const bool DefaultKillSwitch = true;", StringComparison.Ordinal), "DK4 kill switch must default true.");

    private static void NativeSpawnDefaultsClosed() => Check(OptionsSource.Contains("internal const bool DefaultAllowNativeSpawn = false;", StringComparison.Ordinal), "AllowNativeSpawn must default false.");

    private static void HotkeysAreExact()
    {
        Check(OptionsSource.Contains("DefaultActivationHotkey = KeyCode.F7", StringComparison.Ordinal), "activation must be F7.");
        Check(OptionsSource.Contains("DefaultReleaseHotkey = KeyCode.F6", StringComparison.Ordinal), "release must be F6.");
    }

    private static void MaximumObservationSecondsIsExact() => Check(OptionsSource.Contains("DefaultMaximumObservationSeconds = 10f", StringComparison.Ordinal), "maximum observation must be 10 seconds.");

    private static void OwnerIdentityIsExact() => Check(OptionsSource.Contains("OwnerId = \"dragon-knight.dk4.diagnostic-host\"", StringComparison.Ordinal), "owner id changed.");

    private static void ActorRoleIdentityIsExact() => Check(OptionsSource.Contains("ActorRole = \"dragon-knight.boss\"", StringComparison.Ordinal), "actor role changed.");

    private static void ActorSourceIsSpawnLocationTemplate() => Check(OptionsSource.Contains("ActorSource = \"spawnlocation:dragon-location-template\"", StringComparison.Ordinal), "actor source must name SpawnLocation template source.");

    private static void ActorIdFormatIsLiveLocationId()
    {
        Check(ObserverSource.Contains("\"foa.location:\" + locationId", StringComparison.Ordinal), "actor id must derive from observed Location.ID.");
        Check(MarkerSource.Contains("actor-id=", StringComparison.Ordinal), "pass marker must carry actor id.");
    }

    private static void TargetSourceIsExactAltarLocation()
    {
        Check(OptionsSource.Contains("TargetSource = \"AltarInteract\"", StringComparison.Ordinal), "target source must be AltarInteract.");
        Check(OptionsSource.Contains("CM_Stonehenge_0_2258891627046429048_1", StringComparison.Ordinal), "target Location.ID changed.");
    }

    private static void TargetIdFormatIsLiveLocationId()
    {
        Check(OptionsSource.Contains("TargetId = \"foa.location:CM_Stonehenge_0_2258891627046429048_1\"", StringComparison.Ordinal), "target id changed.");
        Check(MarkerSource.Contains("target-id=", StringComparison.Ordinal), "pass marker must carry target id.");
    }

    private static void TargetSourceIsNotUsedAsTrigger()
    {
        Check(HostSource.Contains("Input.GetKeyDown(_activationHotkey.Value)", StringComparison.Ordinal), "activation must be hotkey-owned.");
        Check(!HostSource.Contains("AltarInteract", StringComparison.Ordinal), "host must not trigger from altar object name.");
        Check(!ObserverSource.Contains("Interact(", StringComparison.Ordinal), "observer must not interact with target.");
    }

    private static void SceneIsCampaignMapHos() => Check(OptionsSource.Contains("SceneName = DragonKnightAiV2Contract.SceneName", StringComparison.Ordinal), "scene must be contract-owned CampaignMap_HOS.");

    private static void TemplateGuidsAreExact()
    {
        Check(OptionsSource.Contains("LocationTemplateGuid = DragonKnightAiV2Contract.LocationTemplateGuid", StringComparison.Ordinal), "location template GUID must use AIR-49 contract.");
        Check(OptionsSource.Contains("NpcTemplateGuid = DragonKnightAiV2Contract.NpcTemplateGuid", StringComparison.Ordinal), "NPC template GUID must use AIR-49 contract.");
    }

    private static void NativeVisualAddressIsExact() => Check(OptionsSource.Contains("b4d3a4e9a58fb1c4ea4c239f267faa56", StringComparison.Ordinal), "native visual address changed.");

    private static void CromlechPlacementCoordinatesAreExact()
    {
        Check(OptionsSource.Contains("ArenaCenterX = -1792.661f", StringComparison.Ordinal), "arena X changed.");
        Check(OptionsSource.Contains("ArenaCenterY = 79.942f", StringComparison.Ordinal), "arena Y changed.");
        Check(OptionsSource.Contains("ArenaCenterZ = -2912.844f", StringComparison.Ordinal), "arena Z changed.");
    }

    private static void PluginWiresHostWithoutStartupSpawn()
    {
        Check(PluginSource.Contains("new DragonKnightDk4DiagnosticHost(Logger, Config, GetIronPhasePrefab)", StringComparison.Ordinal), "Plugin must construct DK4 host with the proven Iron prefab provider.");
        Check(PluginSource.Contains("_dk4DiagnosticHost?.Tick();", StringComparison.Ordinal), "Plugin must tick DK4 host.");
        Check(PluginSource.Contains("_dk4DiagnosticHost?.Dispose();", StringComparison.Ordinal), "Plugin must dispose DK4 host.");
        Check(PluginSource.Contains("private GameObject? GetIronPhasePrefab()", StringComparison.Ordinal), "Plugin must share the proven Dragon Knight visual bundle loader with DK4.");
        Check(!PluginSource.Contains("TryStart()", StringComparison.Ordinal), "Plugin must not directly start DK4.");
    }

    private static void F7DoesNothingWhileDisabled() => Check(HostSource.Contains("DK4Diagnostic.Enabled=false", StringComparison.Ordinal), "disabled gate reason missing.");

    private static void F7BlocksWhileKillSwitchTrue() => Check(HostSource.Contains("DK4Diagnostic.KillSwitch=true", StringComparison.Ordinal), "kill-switch gate reason missing.");

    private static void F7BlocksWhileAllowNativeSpawnFalse() => Check(HostSource.Contains("DK4Diagnostic.AllowNativeSpawn=false", StringComparison.Ordinal), "allow-native-spawn gate reason missing.");

    private static void DuplicateActiveActorIsBlocked() => Check(HostSource.Contains("active DK4 diagnostic already exists", StringComparison.Ordinal), "duplicate active actor block missing.");

    private static void TemplateResolvesThroughTemplateReference() => Check(ObserverSource.Contains("new TemplateReference(DragonKnightDk4DiagnosticOptions.LocationTemplateGuid).Get<LocationTemplate>()", StringComparison.Ordinal), "template must resolve by exact TemplateReference.");

    private static void TemplateComponentOrderIsGuarded()
    {
        Check(ObserverSource.Contains("ExpectedLocationComponents", StringComparison.Ordinal), "expected component order missing.");
        Check(ObserverSource.Contains("SequenceEqual(ExpectedLocationComponents", StringComparison.Ordinal), "component order readback missing.");
    }

    private static void NativeVisualIsPreparedBeforeSpawn()
    {
        Check(ObserverSource.Contains("attachment.Setup(npcTemplate, new ARAssetReference", StringComparison.Ordinal), "native visual setup missing.");
        Check(HostSource.IndexOf("TryPrepareTemplateForSpawn(template", StringComparison.Ordinal) < HostSource.IndexOf("template.SpawnLocation", StringComparison.Ordinal), "native visual must be prepared before SpawnLocation.");
        Check(HostSource.Contains("Dragon Knight Iron render overlay prefab is missing", StringComparison.Ordinal), "missing render overlay must fail closed before spawn.");
        Check(HostSource.Contains("DRAGON_KNIGHT_DK4_RENDER_OVERLAY_PASS", StringComparison.Ordinal), "render overlay pass marker missing.");
        Check(OptionsSource.Contains("IronOverlayRootName = \"DragonKnightDk4RenderOverlay_Iron\"", StringComparison.Ordinal), "Iron overlay root name changed.");
        Check(HostSource.Contains("TryAttachRenderOverlay", StringComparison.Ordinal), "render overlay attachment missing.");
        Check(HostSource.IndexOf("_lease = lease;", StringComparison.Ordinal) < HostSource.IndexOf("if (!TryAttachRenderOverlay", StringComparison.Ordinal), "render overlay must attach only after lease acquisition.");
        Check(HostSource.IndexOf("if (!TryAttachRenderOverlay", StringComparison.Ordinal) < HostSource.IndexOf("DragonKnightDk4Marker.BuildPassMarker", StringComparison.Ordinal), "actor pass marker must require render overlay success.");
        Check(HostSource.Contains("KandraRenderer", StringComparison.Ordinal), "native Kandra renderers must be hidden by the overlay path.");
        Check(HostSource.Contains("renderer.enabled = false;", StringComparison.Ordinal), "native renderers must be hidden after overlay attach.");
    }

    private static void TargetProofScansLiveLocations() => Check(ObserverSource.Contains("World.All<Location>().ToArraySlow()", StringComparison.Ordinal), "target proof must scan live Location models.");

    private static void TargetProofUsesObservedLocationId() => Check(ObserverSource.Contains("match.ID", StringComparison.Ordinal), "target observation must use matched Location.ID.");

    private static void SpawnLocationIsIsolatedToDk4Host()
    {
        string allDk4 = HostSource + ObserverSource + MarkerSource + LeaseSource + OptionsSource;
        Check(Count(allDk4, "SpawnLocation") == 2, "DK4 SpawnLocation mentions must be isolated to options/source and host call.");
        Check(HostSource.Contains("template.SpawnLocation", StringComparison.Ordinal), "host must own the native spawn call.");
    }

    private static void NativeVerifyPositionIsUsed() => Check(HostSource.Contains("BaseLocationSpawner.VerifyPosition", StringComparison.Ordinal), "native VerifyPosition missing.");

    private static void SaveExclusionIsImmediateAndReadBack()
    {
        Check(HostSource.Contains("spawnedLocation.MarkedNotSaved = true;", StringComparison.Ordinal), "immediate MarkedNotSaved write missing.");
        Check(HostSource.Contains("!spawnedLocation.MarkedNotSaved || !spawnedLocation.IsNotSaved", StringComparison.Ordinal), "save readback missing.");
        Check(HostSource.Contains("_location.MarkedNotSaved = true;", StringComparison.Ordinal), "continuous save exclusion missing.");
    }

    private static void LocationInitializationCallbackIsRequired() => Check(HostSource.Contains("_location.AfterFullyInitialized(OnLocationInitialized)", StringComparison.Ordinal), "Location initialization callback missing.");

    private static void NpcElementValidationIsRequired()
    {
        Check(HostSource.Contains("_location.TryGetElement(out _npc)", StringComparison.Ordinal), "host must require NpcElement.");
        Check(ObserverSource.Contains("location.TryGetElement(out NpcElement npc)", StringComparison.Ordinal), "observer must validate NpcElement.");
    }

    private static void ReleaseDiscardsProofLocation()
    {
        Check(HostSource.Contains("_location.Discard();", StringComparison.Ordinal), "release must discard Location.");
        Check(MarkerSource.Contains("DRAGON_KNIGHT_DK4_RELEASE_PASS", StringComparison.Ordinal), "release pass marker missing.");
    }

    private static void PassMarkerContainsAllRequiredFields()
    {
        string[] fields =
        {
            "fixtures=36",
            "owner=",
            "actor-role=",
            "actor-source=",
            "actor-location-id=",
            "actor-id=",
            "target-source=",
            "target-location-id=",
            "target-id=",
            "lease=",
            "activation=F7 release=F6",
            "default-off=1 kill-switch=0 cleanup=pass native-spawn=1",
        };

        foreach (string field in fields)
        {
            Check(MarkerSource.Contains(field, StringComparison.Ordinal), "pass marker missing " + field);
        }
    }

    private static void UnsupportedGameplayFieldsStayZero()
    {
        string required = "goals=0 actions=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save=0";
        Check(MarkerSource.Contains(required, StringComparison.Ordinal), "pass marker gameplay boundary changed.");
        Check(HostSource.Contains(required, StringComparison.Ordinal), "blocked/request markers must keep gameplay boundary zero.");
    }

    private static void SourceHasNoForbiddenBypassReferences()
    {
        string dk4Source = string.Join(Environment.NewLine, Directory.GetFiles(Path.Combine(SourceRoot, "DK4"), "*.cs").Select(File.ReadAllText));
        string[] forbidden =
        {
            "Rabbit",
            "GOAP",
            "PlayMaker",
            "AvalonAI.Runtime",
            "AvalonAI.FoAHost",
            "EnterCombatWith",
            "ChangeMainState",
            "FollowMovement",
            "Approach",
            "Attack(",
            "SpawnEnemiesAroundHero",
        };

        foreach (string token in forbidden)
        {
            Check(!dk4Source.Contains(token, StringComparison.Ordinal), "forbidden DK4 token present: " + token);
        }

        Check(ProjectSource.Contains("DragonKnightDK4ATemplate", StringComparison.Ordinal), "project must deploy the DK4A template pack.");
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

    private static int Count(string source, string token)
    {
        int count = 0;
        int index = 0;
        while ((index = source.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += token.Length;
        }

        return count;
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
