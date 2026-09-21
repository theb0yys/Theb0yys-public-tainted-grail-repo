using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Awaken.TG.MVC;
using Awaken.TG.Main.AI.SummonsAndAllies;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Locations.AutoGuards;
using Awaken.TG.Main.Locations.Pets;
using Awaken.TG.Main.Locations.Pets.Variants;
using Awaken.TG.Main.Locations.Setup;
using Awaken.TG.Main.Locations.Spawners;
using Awaken.TG.Main.Scenes.SceneConstructors;
using Awaken.TG.Main.Templates;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AvalonCompanions.Patches;

internal static class PetSystemDiagnostics
{
    private const int MaxTemplateNamesToLog = 12;
    private const string DiagnosticToolConfigFolderName = "kane.tgfoa.template-diagnostics";
    private const string DiagnosticToolSpawnerRefsFileName = "spawner_refs.csv";

    private static bool _logged;

    internal static void TryLog(ManualLogSource logger)
    {
        if (_logged
            || !Plugin.Enabled.Value
            || (!Plugin.LogPetSystemDiagnostics.Value && !Plugin.WritePetTemplateDump.Value && !Plugin.WritePetCreatureShortlistDump.Value))
        {
            return;
        }

        try
        {
            CommonReferences? commonReferences = CommonReferences.Get;
            if (commonReferences == null)
            {
                return;
            }

            if (Hero.Current == null)
            {
                return;
            }

            TemplatesProvider? provider = World.Services?.TryGet<TemplatesProvider>();
            if (provider == null || !provider.AllLoaded)
            {
                return;
            }

            _logged = true;
            LogSnapshot(logger, commonReferences, provider);
        }
        catch (Exception ex)
        {
            _logged = true;
            logger.LogWarning($"Pet system diagnostics failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void LogSnapshot(ManualLogSource logger, CommonReferences commonReferences, TemplatesProvider provider)
    {
        PetTemplateRow[] petTemplates = GetPetTemplateRows(provider);
        int petElements = World.All<PetElement>().ToArraySlow().Length;
        int petVariants = World.All<PetVariantBase>().ToArraySlow().Length;
        int heroSummons = World.All<NpcHeroSummon>().ToArraySlow().Length;
        int heroPetAllies = World.All<NpcHeroPetAlly>().ToArraySlow().Length;
        bool petBaseVariantSet = commonReferences.PetBaseVariant.IsSet;
        int qrkoMountTemplates = commonReferences.QrkoMountTemplates?.Count ?? 0;
        string petTemplateNames = FormatPetTemplateNames(petTemplates);

        if (Plugin.LogPetSystemDiagnostics.Value)
        {
            logger.LogInfo(
                "Pet system diagnostics: "
                + $"PetElements={petElements}; "
                + $"PetVariants={petVariants}; "
                + $"HeroSummons={heroSummons}; "
                + $"HeroPetAllies={heroPetAllies}; "
                + $"CommonReferences.PetBaseVariant.IsSet={petBaseVariantSet}; "
                + $"QrkoMountTemplates={qrkoMountTemplates}; "
                + $"LoadedSpecPetTemplates={petTemplateNames}");
        }

        if (Plugin.WritePetTemplateDump.Value)
        {
            string path = WritePetTemplateDump(petTemplates);
            logger.LogInfo($"Pet template dump wrote {petTemplates.Length} GUID-backed Spec_Pet_* candidates to {path}");
        }

        if (Plugin.WritePetCreatureShortlistDump.Value)
        {
            PetCreatureShortlistDumpResult result = WritePetCreatureShortlistDump(logger, provider);
            logger.LogInfo(
                "Pet/creature LocationTemplate diagnostic dump wrote "
                + $"{result.CandidateCount} name-matched candidates and "
                + $"{result.ShortlistCount} scene-spawner-backed shortlist candidates to {result.Folder}. "
                + $"FOA-Diagnostic Tool crosscheck: {result.DiagnosticToolCrosscheckStatus}");
        }
    }

    private static PetTemplateRow[] GetPetTemplateRows(TemplatesProvider provider)
    {
        return provider.GetAllOfType<LocationTemplate>(TemplateTypeFlag.All)
            .Where(template => template.name.StartsWith("Spec_Pet_", StringComparison.OrdinalIgnoreCase))
            .Select(template => new PetTemplateRow(
                template.GUID,
                template.name,
                template.GetType().FullName ?? template.GetType().Name,
                template.TemplateType.ToString(),
                template.IsAbstract))
            .Distinct()
            .OrderBy(row => row.UnityObjectName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string FormatPetTemplateNames(PetTemplateRow[] rows)
    {
        if (rows.Length == 0)
        {
            return "<none>";
        }

        return string.Join(
            "|",
            rows.Take(MaxTemplateNamesToLog)
                .Select(row => $"{row.UnityObjectName}[{row.TemplateGuid}]"));
    }

    private static string WritePetTemplateDump(PetTemplateRow[] rows)
    {
        string folder = Path.Combine(Paths.ConfigPath, Plugin.PluginGuid);
        Directory.CreateDirectory(folder);

        string path = Path.Combine(folder, "pet-template-candidates.csv");
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("templateGuid,unityObjectName,templateClrType,templateType,isAbstract,safeSpawnCandidate,notes");
        foreach (PetTemplateRow row in rows)
        {
            builder.Append(Escape(row.TemplateGuid));
            builder.Append(',');
            builder.Append(Escape(row.UnityObjectName));
            builder.Append(',');
            builder.Append(Escape(row.TemplateClrType));
            builder.Append(',');
            builder.Append(Escape(row.TemplateType));
            builder.Append(',');
            builder.Append(row.IsAbstract ? "true" : "false");
            builder.Append(',');
            builder.Append("false");
            builder.Append(',');
            builder.AppendLine(Escape("Taxonomy candidate only; not approved for spawn or transformation."));
        }

        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        return path;
    }

    private static PetCreatureShortlistDumpResult WritePetCreatureShortlistDump(ManualLogSource logger, TemplatesProvider provider)
    {
        string folder = Path.Combine(Paths.ConfigPath, Plugin.PluginGuid);
        Directory.CreateDirectory(folder);

        string[] terms = ParseSearchTerms(Plugin.PetCreatureShortlistTerms.Value);
        SceneSpawnerRefRow[] spawnerRefs = CollectSceneSpawnerRefs(logger)
            .OrderBy(row => row.TemplateName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.Source, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.HostName, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        Dictionary<string, SceneSpawnerRefRow[]> refsByGuid = spawnerRefs
            .GroupBy(row => row.TemplateGuid, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.OrdinalIgnoreCase);

        PetCreatureCandidateRow[] candidates = provider.GetAllOfType<LocationTemplate>(TemplateTypeFlag.All)
            .Select(template => ToPetCreatureCandidateRow(logger, template, refsByGuid, terms))
            .Where(row => row.MatchedTerms.Length > 0)
            .OrderByDescending(row => row.ShortlistCandidate)
            .ThenByDescending(row => row.SceneSpawnerRefCount)
            .ThenBy(row => row.UnityObjectName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.TemplateGuid, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        PetCreatureCandidateRow[] shortlist = candidates
            .Where(row => row.ShortlistCandidate)
            .ToArray();

        WritePetCreatureCandidateCsv(Path.Combine(folder, "pet-creature-location-candidates.csv"), candidates);
        WritePetCreatureCandidateCsv(Path.Combine(folder, "pet-creature-location-shortlist.csv"), shortlist);

        DiagnosticToolCrosscheckResult crosscheck = Plugin.WritePetCreatureDiagnosticToolCrosscheck.Value
            ? WriteDiagnosticToolCrosscheck(logger, folder, candidates)
            : DiagnosticToolCrosscheckResult.Skipped("disabled by config");

        return new PetCreatureShortlistDumpResult(
            folder,
            candidates.Length,
            shortlist.Length,
            crosscheck.Status);
    }

    private static PetCreatureCandidateRow ToPetCreatureCandidateRow(
        ManualLogSource logger,
        LocationTemplate template,
        IReadOnlyDictionary<string, SceneSpawnerRefRow[]> refsByGuid,
        string[] terms)
    {
        string guid = template.GUID ?? string.Empty;
        refsByGuid.TryGetValue(guid, out SceneSpawnerRefRow[]? spawnerRefs);
        spawnerRefs ??= Array.Empty<SceneSpawnerRefRow>();

        string name = TemplateName(template);
        string searchText = name
            + " "
            + guid
            + " "
            + string.Join(" ", spawnerRefs.Select(row => row.SearchText));
        string matchedTerms = MatchedTerms(searchText, terms);
        NpcAttachment? npcAttachment = SafeGet(() => template.GetComponent<NpcAttachment>(), logger, $"NpcAttachment for {name}[{guid}]");
        string npcAttachmentType = npcAttachment == null
            ? string.Empty
            : npcAttachment.GetType().FullName ?? npcAttachment.GetType().Name;
        string npcIsUnique = npcAttachment == null
            ? "unknown"
            : npcAttachment.IsUnique ? "true" : "false";
        bool strictNonUniqueNpc = npcAttachment != null && !npcAttachment.IsUnique;
        bool blockedByName = LooksBlockedByName(name);
        bool regular = string.Equals(template.TemplateType.ToString(), "Regular", StringComparison.OrdinalIgnoreCase);
        bool shortlistCandidate = regular
            && !template.IsAbstract
            && spawnerRefs.Length > 0
            && matchedTerms.Length > 0
            && !blockedByName
            && !string.Equals(npcIsUnique, "true", StringComparison.OrdinalIgnoreCase);
        string notes = BuildPetCreatureCandidateNotes(shortlistCandidate, strictNonUniqueNpc, npcIsUnique, blockedByName, spawnerRefs.Length);

        return new PetCreatureCandidateRow(
            guid,
            name,
            template.GetType().FullName ?? template.GetType().Name,
            template.TemplateType.ToString(),
            template.IsAbstract,
            spawnerRefs.Length,
            JoinDistinct(spawnerRefs.Select(row => row.Source)),
            JoinDistinct(spawnerRefs.Select(row => row.ReferenceKind)),
            JoinDistinct(spawnerRefs.Select(row => row.Scene)),
            JoinDistinct(spawnerRefs.Select(row => row.HostName).Take(8)),
            matchedTerms,
            npcAttachmentType,
            npcIsUnique,
            strictNonUniqueNpc,
            blockedByName,
            shortlistCandidate,
            JoinDistinct(spawnerRefs.Select(row => row.Details).Take(4)),
            notes);
    }

    private static IEnumerable<SceneSpawnerRefRow> CollectSceneSpawnerRefs(ManualLogSource logger)
    {
        foreach (LocationSpawnerAttachment attachment in Resources.FindObjectsOfTypeAll<LocationSpawnerAttachment>())
        {
            foreach (LocationTemplate template in SafeResolve(() => attachment.LocationsToSpawn, logger, "LocationSpawnerAttachment.LocationsToSpawn"))
            {
                yield return SceneSpawnerRefRow.FromLocationTemplate(
                    "LocationSpawnerAttachment",
                    "locationsToSpawn",
                    attachment,
                    template,
                    $"spawnAmount={attachment.spawnAmount}; range={FormatFloat(attachment.spawnerRange)}; cooldown={FormatFloat(attachment.SpawnerCooldown)}; discardAfterSpawn={attachment.discardAfterSpawn}; discardAfterAllKilled={attachment.discardAfterAllKilled}; mustFullClearToRespawn={attachment.mustFullClearToRespawn}; snapToGround={attachment.snapToGroundOnSpawn}");
            }
        }

        foreach (GroupSpawnerAttachment attachment in Resources.FindObjectsOfTypeAll<GroupSpawnerAttachment>())
        {
            foreach (GroupSpawnerAttachment.LocationTemplateWithPosition entry in SafeResolve(() => attachment.LocationsToSpawn, logger, "GroupSpawnerAttachment.LocationsToSpawn"))
            {
                LocationTemplate? template = SafeGet(() => entry.LocationToSpawn, logger, "GroupSpawnerAttachment.LocationTemplateWithPosition.LocationToSpawn");
                if (template == null)
                {
                    continue;
                }

                Vector3 position = entry.locationMatrix.GetColumn(3);
                yield return SceneSpawnerRefRow.FromLocationTemplate(
                    "GroupSpawnerAttachment",
                    "locationsWithPositions",
                    attachment,
                    template,
                    $"id={entry.id}; localPosition={FormatVector(position)}; cooldown={FormatFloat(attachment.SpawnerCooldown)}; discardAfterSpawn={attachment.discardAfterSpawn}; discardAfterAllKilled={attachment.discardAfterAllKilled}; mustFullClearToRespawn={attachment.mustFullClearToRespawn}");
            }

            SpawnerRandomizationSettings? randomizationSettings = SafeGet(() => attachment.RandomizationSettings, logger, "GroupSpawnerAttachment.RandomizationSettings");
            if (randomizationSettings == null)
            {
                continue;
            }

            foreach (SpawnerRandomizationSettings.LocationTemplateRandomSpawn entry in SafeResolve(() => randomizationSettings.RandomLocationsToSpawn, logger, "SpawnerRandomizationSettings.RandomLocationsToSpawn"))
            {
                LocationTemplate? template = SafeGet(() => entry.locationToSpawn?.Get<LocationTemplate>(), logger, "LocationTemplateRandomSpawn.locationToSpawn");
                if (template == null)
                {
                    continue;
                }

                yield return SceneSpawnerRefRow.FromLocationTemplate(
                    "GroupSpawnerAttachment",
                    "randomizationSettings",
                    attachment,
                    template,
                    $"id={entry.id}; chance={FormatFloat(entry.spawnChancePerInterval)}; spawnCap={FormatFloat(entry.spawnCap)}; totalSpawnCap={randomizationSettings.totalSpawnCap}; groupSpawnCap={randomizationSettings.groupSpawnCap}; spawnRadius={FormatFloat(randomizationSettings.spawnRadius)}; interval={FormatFloat(randomizationSettings.spawnInterval)}");
            }
        }

        foreach (AutoGuardSpawningAttachment attachment in Resources.FindObjectsOfTypeAll<AutoGuardSpawningAttachment>())
        {
            string faction = SafeTemplateLabel(() => attachment.FactionTemplate, logger, "AutoGuardSpawningAttachment.FactionTemplate");
            foreach (LocationTemplate template in SafeResolve(() => attachment.GuardTemplates, logger, "AutoGuardSpawningAttachment.GuardTemplates"))
            {
                yield return SceneSpawnerRefRow.FromLocationTemplate(
                    "AutoGuardSpawningAttachment",
                    "guardTemplates",
                    attachment,
                    template,
                    $"faction={faction}; spawnPointCount={attachment.SpawnPoints.Count()}");
            }
        }
    }

    private static DiagnosticToolCrosscheckResult WriteDiagnosticToolCrosscheck(ManualLogSource logger, string folder, PetCreatureCandidateRow[] candidates)
    {
        DiagnosticToolSpawnerRefLoadResult loadResult = LoadBestDiagnosticToolSpawnerRefs(logger);
        if (loadResult.Rows.Length == 0)
        {
            return DiagnosticToolCrosscheckResult.Skipped(loadResult.Status);
        }

        Dictionary<string, DiagnosticToolSpawnerRefRow[]> refsByGuid = loadResult.Rows
            .Where(row => !string.IsNullOrWhiteSpace(row.TemplateGuid))
            .GroupBy(row => row.TemplateGuid, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.OrdinalIgnoreCase);

        DiagnosticToolCrosscheckRow[] rows = candidates
            .Select(candidate =>
            {
                refsByGuid.TryGetValue(candidate.TemplateGuid, out DiagnosticToolSpawnerRefRow[]? refs);
                return ToDiagnosticToolCrosscheckRow(candidate, refs ?? Array.Empty<DiagnosticToolSpawnerRefRow>(), loadResult.SourceDumpPath);
            })
            .OrderByDescending(row => row.DiagnosticToolReviewCandidate)
            .ThenBy(row => row.EvidenceStatus, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(row => row.DiagnosticToolSpawnerRefCount)
            .ThenBy(row => row.UnityObjectName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.TemplateGuid, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        DiagnosticToolCrosscheckRow[] reviewRows = rows
            .Where(row => row.DiagnosticToolReviewCandidate)
            .ToArray();

        WriteDiagnosticToolCrosscheckCsv(Path.Combine(folder, "pet-creature-diagnostic-tool-crosscheck.csv"), rows);
        WriteDiagnosticToolCrosscheckCsv(Path.Combine(folder, "pet-creature-diagnostic-tool-review.csv"), reviewRows);

        return new DiagnosticToolCrosscheckResult(
            $"{loadResult.Status}; wrote {rows.Length} crosscheck rows and {reviewRows.Length} review candidates");
    }

    private static DiagnosticToolSpawnerRefLoadResult LoadBestDiagnosticToolSpawnerRefs(ManualLogSource logger)
    {
        string root = Path.Combine(Paths.ConfigPath, DiagnosticToolConfigFolderName);
        if (!Directory.Exists(root))
        {
            return DiagnosticToolSpawnerRefLoadResult.NotFound($"no FOA-Diagnostic Tool dump root found at {root}");
        }

        DirectoryInfo[] dumpFolders = new DirectoryInfo(root)
            .EnumerateDirectories()
            .OrderByDescending(directory => directory.LastWriteTimeUtc)
            .ToArray();
        if (dumpFolders.Length == 0)
        {
            return DiagnosticToolSpawnerRefLoadResult.NotFound($"no FOA-Diagnostic Tool dump folders found under {root}");
        }

        int minimumSpawnerRefs = Math.Max(0, Plugin.DiagnosticToolMinimumSpawnerRefs.Value);
        DiagnosticToolSpawnerRefLoadResult fallback = DiagnosticToolSpawnerRefLoadResult.NotFound($"no readable FOA-Diagnostic Tool {DiagnosticToolSpawnerRefsFileName} files found under {root}");

        foreach (DirectoryInfo dumpFolder in dumpFolders)
        {
            string path = Path.Combine(dumpFolder.FullName, DiagnosticToolSpawnerRefsFileName);
            if (!File.Exists(path))
            {
                continue;
            }

            DiagnosticToolSpawnerRefLoadResult result = TryReadDiagnosticToolSpawnerRefs(logger, path);
            if (result.Rows.Length == 0)
            {
                continue;
            }

            if (fallback.Rows.Length == 0)
            {
                fallback = result.WithStatus($"fallback newest readable dump below threshold: loaded {result.Rows.Length} spawner refs from {path}");
            }

            if (minimumSpawnerRefs == 0 || result.Rows.Length >= minimumSpawnerRefs)
            {
                return result.WithStatus($"selected dump with {result.Rows.Length} spawner refs from {path}; minimum={minimumSpawnerRefs}");
            }
        }

        return fallback;
    }

    private static DiagnosticToolSpawnerRefLoadResult TryReadDiagnosticToolSpawnerRefs(ManualLogSource logger, string path)
    {
        try
        {
            DiagnosticToolSpawnerRefRow[] rows = ReadCsvRecords(path)
                .Select(DiagnosticToolSpawnerRefRow.FromCsvRecord)
                .Where(row => row != null)
                .Cast<DiagnosticToolSpawnerRefRow>()
                .ToArray();

            return new DiagnosticToolSpawnerRefLoadResult(path, rows, $"loaded {rows.Length} spawner refs from {path}");
        }
        catch (Exception ex)
        {
            logger.LogWarning($"{Plugin.PluginName} could not read FOA-Diagnostic Tool spawner refs at {path}: {ex.GetType().Name}: {ex.Message}");
            return DiagnosticToolSpawnerRefLoadResult.NotFound($"failed to read {path}: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static DiagnosticToolCrosscheckRow ToDiagnosticToolCrosscheckRow(
        PetCreatureCandidateRow candidate,
        DiagnosticToolSpawnerRefRow[] diagnosticToolRefs,
        string diagnosticToolDumpPath)
    {
        bool diagnosticToolReviewCandidate = string.Equals(candidate.TemplateType, "Regular", StringComparison.OrdinalIgnoreCase)
            && !candidate.IsAbstract
            && diagnosticToolRefs.Length > 0
            && candidate.MatchedTerms.Length > 0
            && !candidate.BlockedByName
            && !string.Equals(candidate.NpcIsUnique, "true", StringComparison.OrdinalIgnoreCase);
        string evidenceStatus = BuildDiagnosticToolEvidenceStatus(candidate.SceneSpawnerRefCount, diagnosticToolRefs.Length);
        string notes = BuildDiagnosticToolCrosscheckNotes(diagnosticToolReviewCandidate, evidenceStatus);

        return new DiagnosticToolCrosscheckRow(
            candidate.TemplateGuid,
            candidate.UnityObjectName,
            candidate.TemplateType,
            candidate.IsAbstract,
            candidate.MatchedTerms,
            candidate.NpcAttachmentType,
            candidate.NpcIsUnique,
            candidate.StrictNonUniqueNpc,
            candidate.BlockedByName,
            candidate.SceneSpawnerRefCount,
            diagnosticToolRefs.Length,
            evidenceStatus,
            JoinDistinct(diagnosticToolRefs.Select(row => row.Source)),
            JoinDistinct(diagnosticToolRefs.Select(row => row.ReferenceKind)),
            JoinDistinct(diagnosticToolRefs.Select(row => row.Scene)),
            JoinDistinct(diagnosticToolRefs.Select(row => row.HostName).Take(8)),
            JoinDistinct(diagnosticToolRefs.Select(row => row.Details).Take(4)),
            diagnosticToolDumpPath,
            diagnosticToolReviewCandidate,
            notes);
    }

    private static string BuildDiagnosticToolEvidenceStatus(int avalonRefCount, int diagnosticToolRefCount)
    {
        if (avalonRefCount > 0 && diagnosticToolRefCount > 0)
        {
            return "seen-by-avalon-and-foa-diagnostic-tool";
        }

        if (avalonRefCount <= 0 && diagnosticToolRefCount > 0)
        {
            return "seen-by-foa-diagnostic-tool-only";
        }

        if (avalonRefCount > 0)
        {
            return "seen-by-avalon-only";
        }

        return "no-spawner-evidence";
    }

    private static string BuildDiagnosticToolCrosscheckNotes(bool diagnosticToolReviewCandidate, string evidenceStatus)
    {
        if (diagnosticToolReviewCandidate)
        {
            return "Diagnostics-only FOA-Diagnostic Tool review candidate. Not approved for roster, summon, combat, or save behavior.";
        }

        if (string.Equals(evidenceStatus, "seen-by-foa-diagnostic-tool-only", StringComparison.OrdinalIgnoreCase))
        {
            return "FOA-Diagnostic Tool saw spawner evidence, but Avalon did not see it in its own once-per-session dump. Treat as a registration/timing mismatch until manually reviewed.";
        }

        return "Diagnostics-only crosscheck row. Not approved for roster, summon, combat, or save behavior.";
    }

    private static void WriteDiagnosticToolCrosscheckCsv(string path, IEnumerable<DiagnosticToolCrosscheckRow> rows)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("templateGuid,unityObjectName,templateType,isAbstract,matchedTerms,npcAttachmentType,npcIsUnique,strictNonUniqueNpc,blockedByName,avalonSceneSpawnerRefCount,diagnosticToolSpawnerRefCount,evidenceStatus,diagnosticToolSources,diagnosticToolReferenceKinds,diagnosticToolScenes,diagnosticToolSampleHosts,diagnosticToolSampleDetails,diagnosticToolDumpPath,diagnosticToolReviewCandidate,safeSpawnCandidate,rosterApproved,notes");

        foreach (DiagnosticToolCrosscheckRow row in rows)
        {
            builder.Append(Escape(row.TemplateGuid));
            builder.Append(',');
            builder.Append(Escape(row.UnityObjectName));
            builder.Append(',');
            builder.Append(Escape(row.TemplateType));
            builder.Append(',');
            builder.Append(row.IsAbstract ? "true" : "false");
            builder.Append(',');
            builder.Append(Escape(row.MatchedTerms));
            builder.Append(',');
            builder.Append(Escape(row.NpcAttachmentType));
            builder.Append(',');
            builder.Append(Escape(row.NpcIsUnique));
            builder.Append(',');
            builder.Append(row.StrictNonUniqueNpc ? "true" : "false");
            builder.Append(',');
            builder.Append(row.BlockedByName ? "true" : "false");
            builder.Append(',');
            builder.Append(row.AvalonSceneSpawnerRefCount.ToString(CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.Append(row.DiagnosticToolSpawnerRefCount.ToString(CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.Append(Escape(row.EvidenceStatus));
            builder.Append(',');
            builder.Append(Escape(row.DiagnosticToolSources));
            builder.Append(',');
            builder.Append(Escape(row.DiagnosticToolReferenceKinds));
            builder.Append(',');
            builder.Append(Escape(row.DiagnosticToolScenes));
            builder.Append(',');
            builder.Append(Escape(row.DiagnosticToolSampleHosts));
            builder.Append(',');
            builder.Append(Escape(row.DiagnosticToolSampleDetails));
            builder.Append(',');
            builder.Append(Escape(row.DiagnosticToolDumpPath));
            builder.Append(',');
            builder.Append(row.DiagnosticToolReviewCandidate ? "true" : "false");
            builder.Append(',');
            builder.Append("false");
            builder.Append(',');
            builder.Append("false");
            builder.Append(',');
            builder.AppendLine(Escape(row.Notes));
        }

        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
    }

    private static IEnumerable<IReadOnlyDictionary<string, string>> ReadCsvRecords(string path)
    {
        using StreamReader reader = new StreamReader(path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        string? headerLine = reader.ReadLine();
        if (headerLine == null)
        {
            yield break;
        }

        string[] headers = ParseCsvLine(headerLine)
            .Select(header => header.TrimStart('\uFEFF'))
            .ToArray();

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            string[] values = ParseCsvLine(line);
            Dictionary<string, string> record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                record[headers[i]] = i < values.Length ? values[i] : string.Empty;
            }

            yield return record;
        }
    }

    private static string[] ParseCsvLine(string line)
    {
        List<string> fields = new List<string>();
        StringBuilder field = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char current = line[i];
            if (current == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (current == ',' && !inQuotes)
            {
                fields.Add(field.ToString());
                field.Clear();
                continue;
            }

            field.Append(current);
        }

        fields.Add(field.ToString());
        return fields.ToArray();
    }

    private static void WritePetCreatureCandidateCsv(string path, IEnumerable<PetCreatureCandidateRow> rows)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("templateGuid,unityObjectName,templateClrType,templateType,isAbstract,sceneSpawnerRefCount,spawnerSources,referenceKinds,scenes,sampleHosts,matchedTerms,npcAttachmentType,npcIsUnique,strictNonUniqueNpc,blockedByName,shortlistCandidate,safeSpawnCandidate,rosterApproved,notes,sampleSpawnerDetails");

        foreach (PetCreatureCandidateRow row in rows)
        {
            builder.Append(Escape(row.TemplateGuid));
            builder.Append(',');
            builder.Append(Escape(row.UnityObjectName));
            builder.Append(',');
            builder.Append(Escape(row.TemplateClrType));
            builder.Append(',');
            builder.Append(Escape(row.TemplateType));
            builder.Append(',');
            builder.Append(row.IsAbstract ? "true" : "false");
            builder.Append(',');
            builder.Append(row.SceneSpawnerRefCount.ToString(CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.Append(Escape(row.SpawnerSources));
            builder.Append(',');
            builder.Append(Escape(row.ReferenceKinds));
            builder.Append(',');
            builder.Append(Escape(row.Scenes));
            builder.Append(',');
            builder.Append(Escape(row.SampleHosts));
            builder.Append(',');
            builder.Append(Escape(row.MatchedTerms));
            builder.Append(',');
            builder.Append(Escape(row.NpcAttachmentType));
            builder.Append(',');
            builder.Append(Escape(row.NpcIsUnique));
            builder.Append(',');
            builder.Append(row.StrictNonUniqueNpc ? "true" : "false");
            builder.Append(',');
            builder.Append(row.BlockedByName ? "true" : "false");
            builder.Append(',');
            builder.Append(row.ShortlistCandidate ? "true" : "false");
            builder.Append(',');
            builder.Append("false");
            builder.Append(',');
            builder.Append("false");
            builder.Append(',');
            builder.Append(Escape(row.Notes));
            builder.Append(',');
            builder.AppendLine(Escape(row.SampleSpawnerDetails));
        }

        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
    }

    private static string BuildPetCreatureCandidateNotes(bool shortlistCandidate, bool strictNonUniqueNpc, string npcIsUnique, bool blockedByName, int spawnerRefCount)
    {
        if (!shortlistCandidate)
        {
            List<string> reasons = new List<string>();
            if (spawnerRefCount <= 0)
            {
                reasons.Add("no loaded scene spawner reference");
            }

            if (blockedByName)
            {
                reasons.Add("blocked by name-risk terms");
            }

            if (string.Equals(npcIsUnique, "true", StringComparison.OrdinalIgnoreCase))
            {
                reasons.Add("NpcAttachment.IsUnique=true");
            }

            return "Diagnostics only; not shortlisted because " + (reasons.Count == 0 ? "one or more basic filters failed" : string.Join("; ", reasons)) + ".";
        }

        if (!strictNonUniqueNpc)
        {
            return "Diagnostics-only shortlist candidate. Scene-spawner-used, regular, non-abstract, and not flagged unique, but no strict NpcAttachment.IsUnique=false evidence was found; review before behavior.";
        }

        return "Diagnostics-only shortlist candidate. Scene-spawner-used, regular, non-abstract, and NpcAttachment.IsUnique=false. Requires manual review and throwaway-save validation before behavior.";
    }

    private static string[] ParseSearchTerms(string value)
    {
        return value.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(term => term.Trim())
            .Where(term => term.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string MatchedTerms(string haystack, string[] terms)
    {
        return string.Join("|", terms.Where(term => haystack.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0));
    }

    private static bool LooksBlockedByName(string name)
    {
        return ContainsIgnoreCase(name, "Boss")
            || ContainsIgnoreCase(name, "Unique")
            || ContainsIgnoreCase(name, "Quest")
            || ContainsIgnoreCase(name, "Story")
            || ContainsIgnoreCase(name, "Tutorial")
            || ContainsIgnoreCase(name, "Debug")
            || ContainsIgnoreCase(name, "Challenge")
            || ContainsIgnoreCase(name, "Interaction")
            || ContainsIgnoreCase(name, "Summon");
    }

    private static bool ContainsIgnoreCase(string value, string part)
    {
        return value.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static IEnumerable<T> SafeResolve<T>(Func<IEnumerable<T>> read, ManualLogSource logger, string label)
    {
        try
        {
            return read()?.ToArray() ?? Array.Empty<T>();
        }
        catch (Exception ex)
        {
            logger.LogWarning($"{Plugin.PluginName} could not read {label}: {ex.GetType().Name}: {ex.Message}");
            return Array.Empty<T>();
        }
    }

    private static T? SafeGet<T>(Func<T?> read, ManualLogSource logger, string label) where T : class
    {
        try
        {
            return read();
        }
        catch (Exception ex)
        {
            logger.LogWarning($"{Plugin.PluginName} could not read {label}: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    private static string SafeTemplateLabel(Func<ITemplate?> read, ManualLogSource logger, string label)
    {
        ITemplate? template = SafeGet(read, logger, label);
        if (template == null)
        {
            return string.Empty;
        }

        return $"{TemplateName(template)} [{template.GUID}]";
    }

    private static string TemplateName(ITemplate template)
    {
        if (template is Object unityObject && !string.IsNullOrWhiteSpace(unityObject.name))
        {
            return unityObject.name;
        }

        return template.ToString();
    }

    private static string JoinDistinct(IEnumerable<string> values)
    {
        return string.Join("|", values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static string HostPath(Component component)
    {
        Transform? transform = component.transform;
        if (transform == null)
        {
            return string.Empty;
        }

        Stack<string> names = new Stack<string>();
        while (transform != null)
        {
            names.Push(transform.name);
            transform = transform.parent;
        }

        return "/" + string.Join("/", names);
    }

    private static string SceneName(Component component)
    {
        return component.gameObject.scene.IsValid()
            ? component.gameObject.scene.name
            : string.Empty;
    }

    private static string FormatVector(Vector3 value)
    {
        return $"{FormatFloat(value.x)}|{FormatFloat(value.y)}|{FormatFloat(value.z)}";
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string Escape(string value)
    {
        value = value.Replace("\r", " ").Replace("\n", " ");
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    private readonly struct PetTemplateRow : IEquatable<PetTemplateRow>
    {
        internal PetTemplateRow(string templateGuid, string unityObjectName, string templateClrType, string templateType, bool isAbstract)
        {
            TemplateGuid = templateGuid;
            UnityObjectName = unityObjectName;
            TemplateClrType = templateClrType;
            TemplateType = templateType;
            IsAbstract = isAbstract;
        }

        internal string TemplateGuid { get; }

        internal string UnityObjectName { get; }

        internal string TemplateClrType { get; }

        internal string TemplateType { get; }

        internal bool IsAbstract { get; }

        public bool Equals(PetTemplateRow other)
        {
            return StringComparer.OrdinalIgnoreCase.Equals(TemplateGuid, other.TemplateGuid);
        }

        public override bool Equals(object? obj)
        {
            return obj is PetTemplateRow other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(TemplateGuid);
        }
    }

    private readonly struct PetCreatureShortlistDumpResult
    {
        internal PetCreatureShortlistDumpResult(string folder, int candidateCount, int shortlistCount, string diagnosticToolCrosscheckStatus)
        {
            Folder = folder;
            CandidateCount = candidateCount;
            ShortlistCount = shortlistCount;
            DiagnosticToolCrosscheckStatus = diagnosticToolCrosscheckStatus;
        }

        internal string Folder { get; }

        internal int CandidateCount { get; }

        internal int ShortlistCount { get; }

        internal string DiagnosticToolCrosscheckStatus { get; }
    }

    private readonly struct DiagnosticToolCrosscheckResult
    {
        internal DiagnosticToolCrosscheckResult(string status)
        {
            Status = status;
        }

        internal string Status { get; }

        internal static DiagnosticToolCrosscheckResult Skipped(string reason)
        {
            return new DiagnosticToolCrosscheckResult("skipped; " + reason);
        }
    }

    private readonly struct DiagnosticToolSpawnerRefLoadResult
    {
        internal DiagnosticToolSpawnerRefLoadResult(string sourceDumpPath, DiagnosticToolSpawnerRefRow[] rows, string status)
        {
            SourceDumpPath = sourceDumpPath;
            Rows = rows;
            Status = status;
        }

        internal string SourceDumpPath { get; }

        internal DiagnosticToolSpawnerRefRow[] Rows { get; }

        internal string Status { get; }

        internal DiagnosticToolSpawnerRefLoadResult WithStatus(string status)
        {
            return new DiagnosticToolSpawnerRefLoadResult(SourceDumpPath, Rows, status);
        }

        internal static DiagnosticToolSpawnerRefLoadResult NotFound(string status)
        {
            return new DiagnosticToolSpawnerRefLoadResult(string.Empty, Array.Empty<DiagnosticToolSpawnerRefRow>(), status);
        }
    }

    private readonly struct PetCreatureCandidateRow
    {
        internal PetCreatureCandidateRow(
            string templateGuid,
            string unityObjectName,
            string templateClrType,
            string templateType,
            bool isAbstract,
            int sceneSpawnerRefCount,
            string spawnerSources,
            string referenceKinds,
            string scenes,
            string sampleHosts,
            string matchedTerms,
            string npcAttachmentType,
            string npcIsUnique,
            bool strictNonUniqueNpc,
            bool blockedByName,
            bool shortlistCandidate,
            string sampleSpawnerDetails,
            string notes)
        {
            TemplateGuid = templateGuid;
            UnityObjectName = unityObjectName;
            TemplateClrType = templateClrType;
            TemplateType = templateType;
            IsAbstract = isAbstract;
            SceneSpawnerRefCount = sceneSpawnerRefCount;
            SpawnerSources = spawnerSources;
            ReferenceKinds = referenceKinds;
            Scenes = scenes;
            SampleHosts = sampleHosts;
            MatchedTerms = matchedTerms;
            NpcAttachmentType = npcAttachmentType;
            NpcIsUnique = npcIsUnique;
            StrictNonUniqueNpc = strictNonUniqueNpc;
            BlockedByName = blockedByName;
            ShortlistCandidate = shortlistCandidate;
            SampleSpawnerDetails = sampleSpawnerDetails;
            Notes = notes;
        }

        internal string TemplateGuid { get; }

        internal string UnityObjectName { get; }

        internal string TemplateClrType { get; }

        internal string TemplateType { get; }

        internal bool IsAbstract { get; }

        internal int SceneSpawnerRefCount { get; }

        internal string SpawnerSources { get; }

        internal string ReferenceKinds { get; }

        internal string Scenes { get; }

        internal string SampleHosts { get; }

        internal string MatchedTerms { get; }

        internal string NpcAttachmentType { get; }

        internal string NpcIsUnique { get; }

        internal bool StrictNonUniqueNpc { get; }

        internal bool BlockedByName { get; }

        internal bool ShortlistCandidate { get; }

        internal string SampleSpawnerDetails { get; }

        internal string Notes { get; }
    }

    private readonly struct DiagnosticToolCrosscheckRow
    {
        internal DiagnosticToolCrosscheckRow(
            string templateGuid,
            string unityObjectName,
            string templateType,
            bool isAbstract,
            string matchedTerms,
            string npcAttachmentType,
            string npcIsUnique,
            bool strictNonUniqueNpc,
            bool blockedByName,
            int avalonSceneSpawnerRefCount,
            int diagnosticToolSpawnerRefCount,
            string evidenceStatus,
            string diagnosticToolSources,
            string diagnosticToolReferenceKinds,
            string diagnosticToolScenes,
            string diagnosticToolSampleHosts,
            string diagnosticToolSampleDetails,
            string diagnosticToolDumpPath,
            bool diagnosticToolReviewCandidate,
            string notes)
        {
            TemplateGuid = templateGuid;
            UnityObjectName = unityObjectName;
            TemplateType = templateType;
            IsAbstract = isAbstract;
            MatchedTerms = matchedTerms;
            NpcAttachmentType = npcAttachmentType;
            NpcIsUnique = npcIsUnique;
            StrictNonUniqueNpc = strictNonUniqueNpc;
            BlockedByName = blockedByName;
            AvalonSceneSpawnerRefCount = avalonSceneSpawnerRefCount;
            DiagnosticToolSpawnerRefCount = diagnosticToolSpawnerRefCount;
            EvidenceStatus = evidenceStatus;
            DiagnosticToolSources = diagnosticToolSources;
            DiagnosticToolReferenceKinds = diagnosticToolReferenceKinds;
            DiagnosticToolScenes = diagnosticToolScenes;
            DiagnosticToolSampleHosts = diagnosticToolSampleHosts;
            DiagnosticToolSampleDetails = diagnosticToolSampleDetails;
            DiagnosticToolDumpPath = diagnosticToolDumpPath;
            DiagnosticToolReviewCandidate = diagnosticToolReviewCandidate;
            Notes = notes;
        }

        internal string TemplateGuid { get; }

        internal string UnityObjectName { get; }

        internal string TemplateType { get; }

        internal bool IsAbstract { get; }

        internal string MatchedTerms { get; }

        internal string NpcAttachmentType { get; }

        internal string NpcIsUnique { get; }

        internal bool StrictNonUniqueNpc { get; }

        internal bool BlockedByName { get; }

        internal int AvalonSceneSpawnerRefCount { get; }

        internal int DiagnosticToolSpawnerRefCount { get; }

        internal string EvidenceStatus { get; }

        internal string DiagnosticToolSources { get; }

        internal string DiagnosticToolReferenceKinds { get; }

        internal string DiagnosticToolScenes { get; }

        internal string DiagnosticToolSampleHosts { get; }

        internal string DiagnosticToolSampleDetails { get; }

        internal string DiagnosticToolDumpPath { get; }

        internal bool DiagnosticToolReviewCandidate { get; }

        internal string Notes { get; }
    }

    private sealed class DiagnosticToolSpawnerRefRow
    {
        private DiagnosticToolSpawnerRefRow(
            string source,
            string referenceKind,
            string scene,
            string hostPath,
            string hostName,
            string hostWorldPosition,
            string templateGuid,
            string templateName,
            string details)
        {
            Source = source;
            ReferenceKind = referenceKind;
            Scene = scene;
            HostPath = hostPath;
            HostName = hostName;
            HostWorldPosition = hostWorldPosition;
            TemplateGuid = templateGuid;
            TemplateName = templateName;
            Details = details;
        }

        internal string Source { get; }

        internal string ReferenceKind { get; }

        internal string Scene { get; }

        internal string HostPath { get; }

        internal string HostName { get; }

        internal string HostWorldPosition { get; }

        internal string TemplateGuid { get; }

        internal string TemplateName { get; }

        internal string Details { get; }

        internal static DiagnosticToolSpawnerRefRow? FromCsvRecord(IReadOnlyDictionary<string, string> record)
        {
            string templateGuid = CsvValue(record, "templateGuid");
            if (string.IsNullOrWhiteSpace(templateGuid))
            {
                return null;
            }

            return new DiagnosticToolSpawnerRefRow(
                CsvValue(record, "source"),
                CsvValue(record, "referenceKind"),
                CsvValue(record, "scene"),
                CsvValue(record, "hostPath"),
                CsvValue(record, "hostName"),
                CsvValue(record, "hostWorldPosition"),
                templateGuid,
                CsvValue(record, "templateName"),
                CsvValue(record, "details"));
        }

        private static string CsvValue(IReadOnlyDictionary<string, string> record, string key)
        {
            return record.TryGetValue(key, out string? value) ? value : string.Empty;
        }
    }

    private readonly struct SceneSpawnerRefRow
    {
        private SceneSpawnerRefRow(string source, string referenceKind, string scene, string hostPath, string hostName, string hostWorldPosition, string hostRotationY, string templateGuid, string templateName, string details)
        {
            Source = source;
            ReferenceKind = referenceKind;
            Scene = scene;
            HostPath = hostPath;
            HostName = hostName;
            HostWorldPosition = hostWorldPosition;
            HostRotationY = hostRotationY;
            TemplateGuid = templateGuid;
            TemplateName = templateName;
            Details = details;
        }

        internal string Source { get; }

        internal string ReferenceKind { get; }

        internal string Scene { get; }

        internal string HostPath { get; }

        internal string HostName { get; }

        internal string HostWorldPosition { get; }

        internal string HostRotationY { get; }

        internal string TemplateGuid { get; }

        internal string TemplateName { get; }

        internal string Details { get; }

        internal string SearchText => $"{Source} {ReferenceKind} {Scene} {HostPath} {HostName} {HostWorldPosition} {HostRotationY} {TemplateGuid} {TemplateName} {Details}";

        internal static SceneSpawnerRefRow FromLocationTemplate(string source, string referenceKind, Component host, LocationTemplate template, string details)
        {
            return new SceneSpawnerRefRow(
                source,
                referenceKind,
                SceneName(host),
                HostPath(host),
                host.name,
                FormatVector(host.transform.position),
                FormatFloat(host.transform.eulerAngles.y),
                template.GUID ?? string.Empty,
                TemplateName(template),
                details);
        }
    }
}
