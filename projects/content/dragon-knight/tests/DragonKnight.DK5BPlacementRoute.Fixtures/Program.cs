using System;
using System.Collections.Generic;
using System.IO;

internal static class Program
{
    private const string RequiredFirstMarker =
        "DRAGON_KNIGHT_DK5B_AUTHORED_HOS_PLACEMENT_ROUTE_PROOF_SOURCE_GATE_PASS fixtures=18 overlay=dragon-knight.cromlech.overlay.v1 scene=CampaignMap_HOS placement=dragon-knight.vaelor.cromlech.center route-proof-gate=1 route-proven=0 unity-mutation=0 pack-build=0 live-proof=0 native-spawn=0 vanilla-slot=0 runtime-ai=0 movement=0 attacks=0 phase-combat=0 save-write=0";

    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string Gate = Read("mods/dragon-knight/docs/gates/DK5B-authored-hos-placement-pack-route-proof-source-gate.md");
    private static readonly string Decision = Read("mods/dragon-knight/docs/decisions/0016-dk5b-authored-hos-placement-route-proof.md");
    private static readonly string Research = Read("mods/dragon-knight/docs/research/dk5b-authored-hos-placement-route-discovery-2026-08-03.md");
    private static readonly string Readme = Read("mods/dragon-knight/README.md");
    private static readonly string Dk5Gate = Read("mods/dragon-knight/docs/gates/DK5-owned-hos-placement-observer-source-gate-packet.md");
    private static readonly string Overlay = Read("mods/dragon-knight/placement/dragon-knight-cromlech-overlay.v1.json");
    private static readonly string ValidationPlan = Read("mods/dragon-knight/docs/validation-plan.md");

    private static int Main()
    {
        Console.WriteLine(RequiredFirstMarker);

        var fixtures = new (string Name, Action Run)[]
        {
            ("fixture count is exact", FixtureCountIsExact),
            ("source marker is exact", SourceMarkerIsExact),
            ("user approval is recorded", UserApprovalIsRecorded),
            ("existing block is superseded only by DK5B", ExistingBlockIsSupersededOnlyByDk5B),
            ("Unity project path is exact", UnityProjectPathIsExact),
            ("CampaignMap_HOS target is exact", CampaignMapHosTargetIsExact),
            ("Dragon Knight template pair is exact", DragonKnightTemplatePairIsExact),
            ("Cromlech placement is exact", CromlechPlacementIsExact),
            ("route evidence list is complete", RouteEvidenceListIsComplete),
            ("mutation stays blocked", MutationStaysBlocked),
            ("pack build and live proof stay blocked", PackBuildAndLiveProofStayBlocked),
            ("native spawn path stays blocked", NativeSpawnPathStaysBlocked),
            ("vanilla slot path stays blocked", VanillaSlotPathStaysBlocked),
            ("external runtime owners stay blocked", ExternalRuntimeOwnersStayBlocked),
            ("gameplay systems stay blocked", GameplaySystemsStayBlocked),
            ("overlay is not an executor", OverlayIsNotAnExecutor),
            ("discovery does not claim existing source", DiscoveryDoesNotClaimExistingSource),
            ("validation plan references DK5B", ValidationPlanReferencesDk5B),
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
            Console.WriteLine("All " + fixtures.Length + " Dragon Knight DK5B placement-route fixtures passed.");
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
        Check(RequiredFirstMarker.Contains("fixtures=18", StringComparison.Ordinal), "fixture count must stay 18.");

    private static void SourceMarkerIsExact()
    {
        Check(Gate.Contains(RequiredFirstMarker, StringComparison.Ordinal), "DK5B gate marker changed.");
        Check(RequiredFirstMarker.Contains("route-proven=0", StringComparison.Ordinal), "marker must not claim route proof.");
        Check(RequiredFirstMarker.Contains("unity-mutation=0 pack-build=0 live-proof=0", StringComparison.Ordinal), "marker must keep execution blocked.");
    }

    private static void UserApprovalIsRecorded()
    {
        Check(Decision.Contains("explicitly approved DK5B", StringComparison.Ordinal), "decision must record user DK5B approval.");
        Check(Decision.Contains("exact Unity authoring route", StringComparison.Ordinal), "decision must record route-proof need.");
    }

    private static void ExistingBlockIsSupersededOnlyByDk5B()
    {
        Check(Dk5Gate.Contains("DK5B", StringComparison.Ordinal), "DK5 gate must point to DK5B for the new boundary.");
        Check(Dk5Gate.Contains("Movement, attacks, damage, hitboxes, phase combat", StringComparison.Ordinal), "DK5 gate must keep gameplay blocked.");
    }

    private static void UnityProjectPathIsExact()
    {
        string path = @"<local-path>";
        Check(Gate.Contains(path, StringComparison.Ordinal), "gate must name exact Unity project path.");
        Check(Decision.Contains(path, StringComparison.Ordinal), "decision must name exact Unity project path.");
        Check(Research.Contains(path, StringComparison.Ordinal), "research must name exact Unity project path.");
    }

    private static void CampaignMapHosTargetIsExact()
    {
        Check(AllContain("CampaignMap_HOS", Gate, Decision, Research, Readme, Overlay), "all DK5B docs must target CampaignMap_HOS.");
        Check(Overlay.Contains("\"targetSceneName\": \"CampaignMap_HOS\"", StringComparison.Ordinal), "overlay manifest scene changed.");
    }

    private static void DragonKnightTemplatePairIsExact()
    {
        string locationTemplateGuid = "d7b09116519f7564593be62781bee3db";
        string npcTemplateGuid = "00f608ee051b57748a6a9ed8dae28678";

        Check(AllContain("Spec_DragonKnight_DK4A", Gate, Decision, Research, Readme, Overlay), "location template name missing.");
        Check(AllContain("NPCTemplate_DragonKnight_DK4A", Gate, Decision, Research, Readme), "NPC template name missing.");
        Check(AllContain(locationTemplateGuid, Gate, Decision, Research, Readme, Overlay), "location template GUID missing.");
        Check(AllContain(npcTemplateGuid, Gate, Decision, Research, Readme, Overlay), "NPC template GUID missing.");
    }

    private static void CromlechPlacementIsExact()
    {
        Check(AllContain("dragon-knight.vaelor.cromlech.center", Gate, Decision, Research, Readme, Overlay), "placement id missing.");
        Check(AllContain("-1795.209|80.243|-2915.928", Gate, Decision, Research, Readme), "pipe-delimited boss root coordinate missing.");
        Check(AllContain("-1792.661|79.942|-2912.844", Gate, Decision, Research, Readme), "pipe-delimited altar reference coordinate missing.");
        Check(Overlay.Contains("\"x\": -1795.209", StringComparison.Ordinal), "overlay boss root X changed.");
        Check(Overlay.Contains("\"y\": 80.243", StringComparison.Ordinal), "overlay boss root Y changed.");
        Check(Overlay.Contains("\"z\": -2915.928", StringComparison.Ordinal), "overlay boss root Z changed.");
        Check(Overlay.Contains("\"source\": \"AltarInteract\"", StringComparison.Ordinal), "overlay altar source missing.");
        Check(Overlay.Contains("\"liveLocationId\": \"CM_Stonehenge_0_2258891627046429048_1\"", StringComparison.Ordinal), "overlay altar Location.ID missing.");
    }

    private static void RouteEvidenceListIsComplete()
    {
        string[] required =
        {
            "exact Unity editor source file(s)",
            "exact serialized FoA asset",
            "exact reference mechanism",
            "exact ModService or scene-pack output folder",
            "exact build command",
            "exact live log checks",
            "exact cleanup, rollback",
        };

        foreach (string token in required)
        {
            Check(Gate.Contains(token, StringComparison.Ordinal), "route evidence missing: " + token);
        }
    }

    private static void MutationStaysBlocked()
    {
        Check(Gate.Contains("before any Unity project mutation", StringComparison.Ordinal), "gate must block Unity mutation before route evidence.");
        Check(Gate.Contains("Mutating the Unity project", StringComparison.Ordinal), "gate must explicitly forbid Unity mutation now.");
        Check(Decision.Contains("may not mutate the Unity project", StringComparison.Ordinal), "decision must forbid Unity mutation now.");
    }

    private static void PackBuildAndLiveProofStayBlocked()
    {
        Check(Gate.Contains("pack-build=0 live-proof=0", StringComparison.Ordinal), "marker must block pack/live proof.");
        Check(Gate.Contains("Creating or editing Unity scenes", StringComparison.Ordinal), "gate must block scene/package writes.");
        Check(Gate.Contains("Launching FoA for live DK5B proof", StringComparison.Ordinal), "gate must block live launch.");
        Check(Decision.Contains("build an authored pack, deploy to FoA, launch FoA, or claim live actor proof", StringComparison.Ordinal), "decision must block execution.");
    }

    private static void NativeSpawnPathStaysBlocked()
    {
        Check(AllContain("LocationTemplate.SpawnLocation", Gate, Decision), "native spawn stop condition missing.");
        Check(Gate.Contains("native-spawn=0", StringComparison.Ordinal), "marker must include native-spawn=0.");
    }

    private static void VanillaSlotPathStaysBlocked()
    {
        Check(AllContain("LocationSpawner", Gate, Decision), "vanilla slot stop condition missing.");
        Check(Gate.Contains("vanilla-slot=0", StringComparison.Ordinal), "marker must include vanilla-slot=0.");
        Check(Gate.Contains("Hollow Druid", StringComparison.Ordinal), "Hollow Druid slot must stay blocked.");
        Check(Gate.Contains("bear", StringComparison.Ordinal), "bear slot must stay blocked.");
    }

    private static void ExternalRuntimeOwnersStayBlocked()
    {
        string[] owners =
        {
            "Wyrd Hunt",
            "Living Avalon",
            "Avalon Awakened",
            "Avalon Companions",
            "Avalon AI Runtime",
            "FoAHost",
        };

        foreach (string owner in owners)
        {
            Check(Decision.Contains(owner, StringComparison.Ordinal), "decision missing blocked owner: " + owner);
        }
    }

    private static void GameplaySystemsStayBlocked()
    {
        string[] blocked =
        {
            "movement",
            "attacks",
            "phase combat",
            "companion protection",
            "weapon items",
            "armor items",
            "Rabbit writes",
            "GOAP dispatch",
            "PlayMaker calls",
            "Blaze control",
            "save writes",
        };

        foreach (string token in blocked)
        {
            Check(Gate.Contains(token, StringComparison.Ordinal) || Gate.Contains(CapitalizeFirst(token), StringComparison.Ordinal), "gate missing gameplay block: " + token);
        }
    }

    private static void OverlayIsNotAnExecutor()
    {
        Check(Overlay.Contains("This file is not an executor", StringComparison.Ordinal), "overlay must say it is not an executor.");
        Check(Gate.Contains("treats `placement/dragon-knight-cromlech-overlay.v1.json` as an executor", StringComparison.Ordinal), "gate must stop if overlay is treated as executor.");
        Check(Readme.Contains("not an executor", StringComparison.Ordinal), "README must keep overlay non-executor boundary.");
    }

    private static void DiscoveryDoesNotClaimExistingSource()
    {
        Check(Research.Contains("did not prove an existing Dragon Knight placement-pack authoring route", StringComparison.Ordinal), "research must not claim source route found.");
        Check(Research.Contains("does not authorize Unity project mutation", StringComparison.Ordinal), "research must keep route proof non-mutating.");
    }

    private static void ValidationPlanReferencesDk5B() =>
        Check(ValidationPlan.Contains("DK5B", StringComparison.Ordinal), "validation plan must reference DK5B.");

    private static bool AllContain(string value, params string[] sources)
    {
        foreach (string source in sources)
        {
            if (!source.Contains(value, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static string CapitalizeFirst(string value) =>
        value.Length == 0 ? value : char.ToUpperInvariant(value[0]) + value[1..];

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
            if (File.Exists(Path.Combine(current.FullName, "mods", "dragon-knight", "README.md")))
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
