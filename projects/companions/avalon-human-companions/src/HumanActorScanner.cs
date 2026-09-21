using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Awaken.TG.Main.AI.SummonsAndAllies;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Pets;
using Awaken.TG.Main.Locations.Pets.Variants;
using Awaken.TG.Main.Locations.Setup;
using Awaken.TG.MVC;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace AvalonHumanCompanions;

internal static class HumanActorScanner
{
    private const int MaxComponentTypesPerLocation = 80;

    internal static void WriteSnapshot(ManualLogSource logger, float radius, string selectedTarget)
    {
        try
        {
            if (Hero.Current == null)
            {
                logger.LogWarning($"{Plugin.PluginName} actor scanner blocked: Hero.Current is null. Load a save before scanning.");
                return;
            }

            string folder = Path.Combine(Paths.ConfigPath, Plugin.PluginGuid);
            Directory.CreateDirectory(folder);

            List<HumanActorRow> rows = CollectRows(logger, Math.Max(5f, radius));
            HumanActorRow? selected = SelectRow(rows, selectedTarget);
            List<HumanCommandDryRunRow> commands = BuildCommandRows(selected);
            List<HumanRecruitmentCaptureResearchRow> recruitmentCaptureRows = BuildRecruitmentCaptureRows(rows);

            string actorsPath = Path.Combine(folder, "human-actor-candidates.csv");
            string componentsPath = Path.Combine(folder, "human-actor-components.csv");
            string commandsPath = Path.Combine(folder, "human-actor-command-dry-run.csv");
            string recruitmentCapturePath = Path.Combine(folder, "human-recruitment-capture-research.csv");

            WriteActorsCsv(actorsPath, rows);
            WriteComponentsCsv(componentsPath, rows);
            WriteCommandsCsv(commandsPath, commands);
            WriteRecruitmentCaptureCsv(recruitmentCapturePath, recruitmentCaptureRows);

            logger.LogInfo(
                $"{Plugin.PluginName} actor scanner wrote {rows.Count} candidate row(s), "
                + $"{rows.Sum(row => row.Components.Count)} component row(s), and "
                + $"{commands.Count} dry-run command row(s), plus "
                + $"{recruitmentCaptureRows.Count} recruitment/capture research row(s), to {folder}. "
                + "No actor movement, spawning, faction, targeting, quest, interaction, or save state was changed.");
        }
        catch (Exception ex)
        {
            logger.LogWarning($"{Plugin.PluginName} actor scanner failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static List<HumanActorRow> CollectRows(ManualLogSource logger, float radius)
    {
        Vector3 heroCoords = Hero.Current.Coords;
        float radiusSquared = radius * radius;
        Dictionary<Location, KnownActorMarkers> markers = CollectKnownMarkers(logger);
        List<HumanActorRow> rows = new List<HumanActorRow>();

        foreach (Location location in World.All<Location>().ToArraySlow())
        {
            if (location == null || location.HasBeenDiscarded)
            {
                continue;
            }

            float distanceSquared = (location.Coords - heroCoords).sqrMagnitude;
            if (distanceSquared > radiusSquared && !markers.ContainsKey(location))
            {
                continue;
            }

            markers.TryGetValue(location, out KnownActorMarkers knownMarkers);
            HumanActorRow row = ToRow(location, knownMarkers, Mathf.Sqrt(distanceSquared), logger);
            if (row.ActorLike || row.Components.Count > 0)
            {
                rows.Add(row);
            }
        }

        return rows
            .OrderBy(row => row.Sort)
            .ThenBy(row => row.DistanceFromHero, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.TemplateName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.LocationId, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static Dictionary<Location, KnownActorMarkers> CollectKnownMarkers(ManualLogSource logger)
    {
        Dictionary<Location, KnownActorMarkers> markers = new Dictionary<Location, KnownActorMarkers>();

        foreach (PetElement element in World.All<PetElement>().ToArraySlow())
        {
            AddMarker(markers, element.ParentModel, TypeName(element), logger);
        }

        foreach (PetVariantBase element in World.All<PetVariantBase>().ToArraySlow())
        {
            AddMarker(markers, element.ParentModel, TypeName(element), logger);
        }

        foreach (NpcHeroSummon element in World.All<NpcHeroSummon>().ToArraySlow())
        {
            AddMarker(markers, ParentLocation(element, logger), TypeName(element), logger);
        }

        foreach (NpcHeroPetAlly element in World.All<NpcHeroPetAlly>().ToArraySlow())
        {
            AddMarker(markers, ParentLocation(element, logger), TypeName(element), logger);
        }

        return markers;
    }

    private static HumanActorRow ToRow(Location location, KnownActorMarkers markers, float distanceFromHero, ManualLogSource logger)
    {
        LocationTemplate? template = location.Template;
        string templateName = template?.name ?? string.Empty;
        string templateGuid = template?.GUID ?? string.Empty;
        NpcAttachment? npcAttachment = template == null
            ? null
            : SafeGet(() => template.GetComponent<NpcAttachment>(), logger, $"NpcAttachment for {TemplateLabel(template)}");
        string npcAttachmentType = npcAttachment == null ? string.Empty : TypeName(npcAttachment);
        string npcIsUnique = npcAttachment == null ? "unknown" : npcAttachment.IsUnique ? "true" : "false";
        bool hasNpcElement = SafeBool(() => location.TryGetElement<NpcElement>() != null, logger, $"NpcElement for {LocationLabel(location)}");
        bool hasAlive = SafeBool(() => location.TryGetElement<IAlive>() != null, logger, $"IAlive for {LocationLabel(location)}");
        bool hasPetElement = markers.ContainsTypeName("PetElement") || SafeBool(() => location.TryGetElement<PetElement>() != null, logger, $"PetElement for {LocationLabel(location)}");
        bool hasPetVariant = markers.ContainsTypeName("PetVariantBase") || SafeBool(() => location.TryGetElement<PetVariantBase>() != null, logger, $"PetVariantBase for {LocationLabel(location)}");
        bool hasHeroSummon = markers.ContainsTypeName("NpcHeroSummon");
        bool hasHeroPetAlly = markers.ContainsTypeName("NpcHeroPetAlly");
        List<HumanComponentRow> components = CollectComponents(location, markers, npcAttachmentType, logger);
        string componentTypes = JoinDistinct(components.Select(row => row.ComponentType));
        string controlHints = FilterTypeHints(componentTypes, "Pet", "Summon", "Ally", "Follow", "Recall", "Controller", "Brain", "AI", "Nav", "Move", "Agent");
        string factionHints = FilterTypeHints(componentTypes, "Faction", "Owner", "Master", "Target", "Hostile", "Relation", "Team", "Threat", "Aggro", "Crime", "Guard");
        bool actorLike = hasNpcElement || hasAlive || hasPetElement || hasPetVariant || hasHeroSummon || hasHeroPetAlly || npcAttachment != null;
        string status = BuildStatus(templateGuid, npcIsUnique, hasNpcElement, hasAlive, location.MarkedNotSaved, templateName);
        string blockReason = BuildBlockReason(templateGuid, npcIsUnique, hasNpcElement, hasAlive, location.MarkedNotSaved, templateName);

        return new HumanActorRow(
            location.ID ?? string.Empty,
            location.DebugName ?? string.Empty,
            SafeObjectText(location.DisplayName),
            SceneOrDomain(location),
            FormatVector(location.Coords),
            FormatFloat(distanceFromHero),
            location.MarkedNotSaved,
            templateGuid,
            templateName,
            template?.GetType().FullName ?? template?.GetType().Name ?? string.Empty,
            template?.TemplateType.ToString() ?? string.Empty,
            template?.IsAbstract ?? false,
            npcAttachmentType,
            npcIsUnique,
            hasNpcElement,
            hasAlive,
            hasPetElement,
            hasPetVariant,
            hasHeroSummon,
            hasHeroPetAlly,
            actorLike,
            status,
            blockReason,
            controlHints,
            factionHints,
            componentTypes,
            components);
    }

    private static List<HumanComponentRow> CollectComponents(Location location, KnownActorMarkers markers, string npcAttachmentType, ManualLogSource logger)
    {
        List<HumanComponentRow> rows = new List<HumanComponentRow>();
        string locationId = location.ID ?? string.Empty;
        string templateGuid = location.Template?.GUID ?? string.Empty;

        foreach (string marker in markers.TypeNames)
        {
            rows.Add(new HumanComponentRow(locationId, templateGuid, "known-world-marker", marker, string.Empty));
        }

        if (!string.IsNullOrWhiteSpace(npcAttachmentType))
        {
            rows.Add(new HumanComponentRow(locationId, templateGuid, "template-component", npcAttachmentType, "NpcAttachment"));
        }

        AddLocationElement<NpcElement>(rows, location, "known-location-element");
        AddLocationElement<IAlive>(rows, location, "known-location-element");
        AddLocationElement<PetElement>(rows, location, "known-location-element");
        AddLocationElement<PetVariantBase>(rows, location, "known-location-element");

        foreach (string component in ViewComponentTypeNames(location, logger))
        {
            rows.Add(new HumanComponentRow(locationId, templateGuid, "view-component", component, TypeHintBucket(component)));
        }

        return rows
            .GroupBy(row => $"{row.ComponentSource}|{row.ComponentType}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(row => row.ComponentSource, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.ComponentType, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void AddLocationElement<T>(List<HumanComponentRow> rows, Location location, string source) where T : class, IModel
    {
        if (location.TryGetElement<T>() == null)
        {
            return;
        }

        string typeName = typeof(T).FullName ?? typeof(T).Name;
        rows.Add(new HumanComponentRow(location.ID ?? string.Empty, location.Template?.GUID ?? string.Empty, source, typeName, TypeHintBucket(typeName)));
    }

    private static IEnumerable<string> ViewComponentTypeNames(Location location, ManualLogSource logger)
    {
        Transform? viewParent;
        try
        {
            viewParent = location.ViewParent;
        }
        catch (Exception ex)
        {
            logger.LogWarning($"{Plugin.PluginName} actor scanner could not read ViewParent for {LocationLabel(location)}: {ex.GetType().Name}: {ex.Message}");
            yield break;
        }

        if (viewParent == null)
        {
            yield break;
        }

        Component[] components;
        try
        {
            components = viewParent.GetComponentsInChildren<Component>(true);
        }
        catch (Exception ex)
        {
            logger.LogWarning($"{Plugin.PluginName} actor scanner could not read view components for {LocationLabel(location)}: {ex.GetType().Name}: {ex.Message}");
            yield break;
        }

        foreach (string typeName in components
            .Where(component => component != null)
            .Select(component => component.GetType().FullName ?? component.GetType().Name)
            .Where(IsInterestingComponentType)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .Take(MaxComponentTypesPerLocation))
        {
            yield return typeName;
        }
    }

    private static HumanActorRow? SelectRow(List<HumanActorRow> rows, string selectedTarget)
    {
        if (!string.IsNullOrWhiteSpace(selectedTarget))
        {
            HumanActorRow? configured = rows.FirstOrDefault(row =>
                string.Equals(row.TemplateGuid, selectedTarget, StringComparison.OrdinalIgnoreCase)
                || string.Equals(row.LocationId, selectedTarget, StringComparison.OrdinalIgnoreCase));
            if (configured != null)
            {
                return configured;
            }
        }

        return rows.FirstOrDefault(row => row.SafeControlStatus == "HumanProofDryRun")
            ?? rows.FirstOrDefault(row => row.ActorLike)
            ?? rows.FirstOrDefault();
    }

    private static List<HumanCommandDryRunRow> BuildCommandRows(HumanActorRow? selected)
    {
        string[] commands = { "follow", "stay", "recall", "dismiss", "defend", "panel", "recruit-friendly", "capture-enemy" };
        return commands.Select(command => BuildCommandRow(selected, command)).ToList();
    }

    private static HumanCommandDryRunRow BuildCommandRow(HumanActorRow? selected, string command)
    {
        if (selected == null)
        {
            return new HumanCommandDryRunRow(command, string.Empty, string.Empty, string.Empty, string.Empty, "Blocked", "no selected candidate", true, CommandTouchesFactionOrTargeting(command), "No candidate row was available.", false);
        }

        string commandPath;
        string reason;
        switch (command)
        {
            case "panel":
                commandPath = "future plugin-owned runtime panel only; vanilla serialized interaction list remains blocked";
                reason = "Panel is design-approved only after scanner and interaction-surface proof; this row is evidence only.";
                break;
            case "recruit-friendly":
                commandPath = "future friendly-NPC recruitment path; blocked until live disposition, interaction, transition, and save proofs pass";
                reason = "Friendly NPC recruitment remains blocked until faction/hostility safety, guard/civilian/quest safety, and save/load proofs pass.";
                break;
            case "capture-enemy":
                commandPath = "future enemy-NPC capture path; blocked until hostility, surrender/incapacitation, crime, transition, and save proofs pass";
                reason = "Enemy NPC capture remains blocked until hostile disposition, combat/crime safety, actor ownership, and save/load proofs pass.";
                break;
            default:
                commandPath = "no approved human command path yet";
                reason = "Human NPC behavior remains blocked until actor identity, faction, interaction, transition, and save/load proofs pass.";
                break;
        }

        return new HumanCommandDryRunRow(
            command,
            selected.CandidateLabel,
            selected.LocationId,
            selected.TemplateGuid,
            selected.TemplateName,
            selected.SafeControlStatus,
            commandPath,
            true,
            CommandTouchesFactionOrTargeting(command),
            reason,
            false);
    }

    private static bool CommandTouchesFactionOrTargeting(string command)
    {
        return command == "defend" || command == "recruit-friendly" || command == "capture-enemy";
    }

    private static List<HumanRecruitmentCaptureResearchRow> BuildRecruitmentCaptureRows(IEnumerable<HumanActorRow> rows)
    {
        return rows
            .Where(row => row.ActorLike)
            .Select(BuildRecruitmentCaptureRow)
            .OrderBy(row => row.Sort)
            .ThenBy(row => row.DistanceFromHero, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.TemplateName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.LocationId, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static HumanRecruitmentCaptureResearchRow BuildRecruitmentCaptureRow(HumanActorRow row)
    {
        string evidenceText = string.Join("|", new[]
        {
            row.DebugName,
            row.DisplayName,
            row.TemplateName,
            row.TemplateClrType,
            row.NpcAttachmentType,
            row.ControlHints,
            row.FactionTargetHints,
            row.ComponentTypes
        });
        bool enemyEvidence = ContainsAny(evidenceText, "Enemy", "Outlaw", "Bandit", "Highwayman", "Raider", "Thug", "Cultist", "Deranged", "Desperate", "Disgraced", "Berserker", "Hostile", "Aggro", "Threat");
        bool friendlyEvidence = ContainsAny(evidenceText, "Guard", "Villager", "Civilian", "Merchant", "Druid", "Squire", "Citizen", "Peasant", "Friendly", "Ally", "Kamelot", "DalRiata");
        string dispositionEvidence = BuildDispositionEvidence(enemyEvidence, friendlyEvidence);
        string researchLane = BuildRecruitmentCaptureResearchLane(row, enemyEvidence, friendlyEvidence);
        string riskFlags = BuildRecruitmentCaptureRiskFlags(row, evidenceText, enemyEvidence, friendlyEvidence);
        string blockReason = BuildRecruitmentCaptureBlockReason(row);
        string requiredProof = "Requires live disposition API evidence, safe faction/hostility handling, plugin-owned UI, transition/save-load cleanup, and guard/civilian/quest/crime safety proof.";
        string existingNpcPolicy = row.MarkedNotSaved ? "runtime-proof-or-not-saved" : "existing-live-actor-blocked";

        return new HumanRecruitmentCaptureResearchRow(
            row.LocationId,
            row.DebugName,
            row.DisplayName,
            row.TemplateGuid,
            row.TemplateName,
            row.DistanceFromHero,
            row.MarkedNotSaved,
            row.NpcIsUnique,
            row.HasNpcElement,
            row.HasAlive,
            row.HasHeroPetAlly,
            existingNpcPolicy,
            dispositionEvidence,
            researchLane,
            riskFlags,
            true,
            blockReason,
            requiredProof,
            false,
            false,
            false,
            false,
            "Scanner classification only; friendly/enemy lane is string evidence, not behavior approval.",
            row.Sort);
    }

    private static string BuildDispositionEvidence(bool enemyEvidence, bool friendlyEvidence)
    {
        if (enemyEvidence && friendlyEvidence)
        {
            return "ambiguous-friendly-and-enemy-evidence";
        }

        if (enemyEvidence)
        {
            return "enemy-like-evidence";
        }

        if (friendlyEvidence)
        {
            return "friendly-like-evidence";
        }

        return "unknown-disposition";
    }

    private static string BuildRecruitmentCaptureResearchLane(HumanActorRow row, bool enemyEvidence, bool friendlyEvidence)
    {
        if (row.MarkedNotSaved && row.HasHeroPetAlly)
        {
            return "OneSessionProofOnly";
        }

        if (enemyEvidence && !friendlyEvidence)
        {
            return "EnemyCaptureResearch";
        }

        if (friendlyEvidence && !enemyEvidence)
        {
            return "FriendlyRecruitmentResearch";
        }

        if (enemyEvidence && friendlyEvidence)
        {
            return "DispositionAmbiguousResearch";
        }

        return "DispositionUnknownResearch";
    }

    private static string BuildRecruitmentCaptureRiskFlags(HumanActorRow row, string evidenceText, bool enemyEvidence, bool friendlyEvidence)
    {
        List<string> riskFlags = new List<string>();
        if (!row.MarkedNotSaved)
        {
            riskFlags.Add("existing-npc-risk");
        }

        if (string.Equals(row.NpcIsUnique, "true", StringComparison.OrdinalIgnoreCase))
        {
            riskFlags.Add("unique-risk");
        }

        if (LooksBlockedByName(row.TemplateName) || LooksBlockedByName(row.DebugName) || LooksBlockedByName(row.DisplayName))
        {
            riskFlags.Add("story-name-risk");
        }

        if (ContainsAny(evidenceText, "Guard", "Kamelot", "DalRiata"))
        {
            riskFlags.Add("guard-faction-risk");
        }

        if (ContainsAny(evidenceText, "Civilian", "Villager", "Peasant", "Merchant", "Druid", "Squire", "Citizen"))
        {
            riskFlags.Add("civilian-risk");
        }

        if (enemyEvidence)
        {
            riskFlags.Add("hostile-risk");
        }

        if (friendlyEvidence)
        {
            riskFlags.Add("friendly-disposition-risk");
        }

        if (row.HasHeroPetAlly || row.HasHeroSummon)
        {
            riskFlags.Add("summon-ally-marker");
        }

        riskFlags.Add("persistence-risk");
        return JoinDistinct(riskFlags);
    }

    private static string BuildRecruitmentCaptureBlockReason(HumanActorRow row)
    {
        List<string> reasons = new List<string>();
        if (!string.IsNullOrWhiteSpace(row.BlockReason))
        {
            reasons.Add(row.BlockReason);
        }

        if (!row.HasNpcElement && !row.HasAlive)
        {
            reasons.Add("no live NPC control marker");
        }

        reasons.Add("live recruitment/capture behavior is not approved");
        reasons.Add("faction/hostility/transition/save proofs are missing");
        return JoinDistinct(reasons);
    }

    private static void WriteActorsCsv(string path, IEnumerable<HumanActorRow> rows)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("locationId,debugName,displayName,sceneOrDomain,coords,distanceFromHero,markedNotSaved,templateGuid,templateName,templateClrType,templateType,templateIsAbstract,npcAttachmentType,npcIsUnique,hasNpcElement,hasAlive,hasPetElement,hasPetVariant,hasHeroSummon,hasHeroPetAlly,actorLike,safeControlStatus,blockReason,controlHints,factionTargetHints,componentTypes,behaviorApproved,panelApproved,notes");
        foreach (HumanActorRow row in rows)
        {
            AppendCsv(builder, row.LocationId);
            AppendCsv(builder, row.DebugName);
            AppendCsv(builder, row.DisplayName);
            AppendCsv(builder, row.SceneOrDomain);
            AppendCsv(builder, row.Coords);
            AppendCsv(builder, row.DistanceFromHero);
            AppendCsv(builder, row.MarkedNotSaved ? "true" : "false");
            AppendCsv(builder, row.TemplateGuid);
            AppendCsv(builder, row.TemplateName);
            AppendCsv(builder, row.TemplateClrType);
            AppendCsv(builder, row.TemplateType);
            AppendCsv(builder, row.TemplateIsAbstract ? "true" : "false");
            AppendCsv(builder, row.NpcAttachmentType);
            AppendCsv(builder, row.NpcIsUnique);
            AppendCsv(builder, row.HasNpcElement ? "true" : "false");
            AppendCsv(builder, row.HasAlive ? "true" : "false");
            AppendCsv(builder, row.HasPetElement ? "true" : "false");
            AppendCsv(builder, row.HasPetVariant ? "true" : "false");
            AppendCsv(builder, row.HasHeroSummon ? "true" : "false");
            AppendCsv(builder, row.HasHeroPetAlly ? "true" : "false");
            AppendCsv(builder, row.ActorLike ? "true" : "false");
            AppendCsv(builder, row.SafeControlStatus);
            AppendCsv(builder, row.BlockReason);
            AppendCsv(builder, row.ControlHints);
            AppendCsv(builder, row.FactionTargetHints);
            AppendCsv(builder, row.ComponentTypes);
            AppendCsv(builder, "false");
            AppendCsv(builder, "false");
            AppendCsv(builder, "Scanner evidence only; no command, panel, faction, persistence, quest, crime, or save behavior approved.");
            builder.Length--;
            builder.AppendLine();
        }

        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
    }

    private static void WriteComponentsCsv(string path, IEnumerable<HumanActorRow> rows)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("locationId,templateGuid,componentSource,componentType,hintBucket");
        foreach (HumanComponentRow row in rows.SelectMany(actor => actor.Components))
        {
            AppendCsv(builder, row.LocationId);
            AppendCsv(builder, row.TemplateGuid);
            AppendCsv(builder, row.ComponentSource);
            AppendCsv(builder, row.ComponentType);
            AppendCsv(builder, row.HintBucket);
            builder.Length--;
            builder.AppendLine();
        }

        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
    }

    private static void WriteCommandsCsv(string path, IEnumerable<HumanCommandDryRunRow> rows)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("command,candidateLabel,locationId,templateGuid,templateName,safeControlStatus,commandPath,blocked,touchesFactionOrTargeting,reason,liveAction");
        foreach (HumanCommandDryRunRow row in rows)
        {
            AppendCsv(builder, row.Command);
            AppendCsv(builder, row.CandidateLabel);
            AppendCsv(builder, row.LocationId);
            AppendCsv(builder, row.TemplateGuid);
            AppendCsv(builder, row.TemplateName);
            AppendCsv(builder, row.SafeControlStatus);
            AppendCsv(builder, row.CommandPath);
            AppendCsv(builder, row.Blocked ? "true" : "false");
            AppendCsv(builder, row.TouchesFactionOrTargeting ? "true" : "false");
            AppendCsv(builder, row.Reason);
            AppendCsv(builder, row.LiveAction ? "true" : "false");
            builder.Length--;
            builder.AppendLine();
        }

        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
    }

    private static void WriteRecruitmentCaptureCsv(string path, IEnumerable<HumanRecruitmentCaptureResearchRow> rows)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("locationId,debugName,displayName,templateGuid,templateName,distanceFromHero,markedNotSaved,npcIsUnique,hasNpcElement,hasAlive,hasHeroPetAlly,existingNpcPolicy,dispositionEvidence,researchLane,riskFlags,blocked,blockReason,requiredProof,recruitmentApproved,captureApproved,persistenceApproved,liveAction,notes");
        foreach (HumanRecruitmentCaptureResearchRow row in rows)
        {
            AppendCsv(builder, row.LocationId);
            AppendCsv(builder, row.DebugName);
            AppendCsv(builder, row.DisplayName);
            AppendCsv(builder, row.TemplateGuid);
            AppendCsv(builder, row.TemplateName);
            AppendCsv(builder, row.DistanceFromHero);
            AppendCsv(builder, row.MarkedNotSaved ? "true" : "false");
            AppendCsv(builder, row.NpcIsUnique);
            AppendCsv(builder, row.HasNpcElement ? "true" : "false");
            AppendCsv(builder, row.HasAlive ? "true" : "false");
            AppendCsv(builder, row.HasHeroPetAlly ? "true" : "false");
            AppendCsv(builder, row.ExistingNpcPolicy);
            AppendCsv(builder, row.DispositionEvidence);
            AppendCsv(builder, row.ResearchLane);
            AppendCsv(builder, row.RiskFlags);
            AppendCsv(builder, row.Blocked ? "true" : "false");
            AppendCsv(builder, row.BlockReason);
            AppendCsv(builder, row.RequiredProof);
            AppendCsv(builder, row.RecruitmentApproved ? "true" : "false");
            AppendCsv(builder, row.CaptureApproved ? "true" : "false");
            AppendCsv(builder, row.PersistenceApproved ? "true" : "false");
            AppendCsv(builder, row.LiveAction ? "true" : "false");
            AppendCsv(builder, row.Notes);
            builder.Length--;
            builder.AppendLine();
        }

        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
    }

    private static string BuildStatus(string templateGuid, string npcIsUnique, bool hasNpcElement, bool hasAlive, bool markedNotSaved, string templateName)
    {
        string blockReason = BuildBlockReason(templateGuid, npcIsUnique, hasNpcElement, hasAlive, markedNotSaved, templateName);
        return string.IsNullOrWhiteSpace(blockReason) ? "HumanProofDryRun" : "Blocked";
    }

    private static string BuildBlockReason(string templateGuid, string npcIsUnique, bool hasNpcElement, bool hasAlive, bool markedNotSaved, string templateName)
    {
        List<string> reasons = new List<string>();
        if (LooksBlockedByName(templateName))
        {
            reasons.Add("blocked by story/unique/boss/quest/debug/challenge/tutorial/name-risk term");
        }

        if (string.Equals(npcIsUnique, "true", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add("NpcAttachment.IsUnique=true");
        }

        if (string.IsNullOrWhiteSpace(templateGuid))
        {
            reasons.Add("missing template GUID");
        }

        if (!hasNpcElement && !hasAlive)
        {
            reasons.Add("no NpcElement or IAlive marker");
        }

        if (!markedNotSaved)
        {
            reasons.Add("location is not marked not saved; behavior remains blocked");
        }

        return string.Join("; ", reasons);
    }

    private static void AddMarker(Dictionary<Location, KnownActorMarkers> markers, Location? location, string marker, ManualLogSource logger)
    {
        if (location == null)
        {
            logger.LogInfo($"{Plugin.PluginName} actor scanner saw marker {marker}, but could not resolve a parent Location.");
            return;
        }

        markers.TryGetValue(location, out KnownActorMarkers existing);
        existing.Add(marker);
        markers[location] = existing;
    }

    private static Location? ParentLocation(object value, ManualLogSource logger)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        string[] propertyNames = { "ParentModel", "GenericParentModel", "Parent", "Location" };
        Type type = value.GetType();

        foreach (string propertyName in propertyNames)
        {
            try
            {
                PropertyInfo? property = type.GetProperty(propertyName, flags);
                if (property?.GetValue(value) is Location location)
                {
                    return location;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"{Plugin.PluginName} actor scanner could not read {type.FullName}.{propertyName}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        return null;
    }

    private static string SceneOrDomain(Location location)
    {
        try
        {
            Transform viewParent = location.ViewParent;
            if (viewParent != null)
            {
                UnityEngine.SceneManagement.Scene scene = viewParent.gameObject.scene;
                if (scene.IsValid())
                {
                    return string.IsNullOrWhiteSpace(scene.name) ? "<unnamed-scene>" : scene.name;
                }
            }
        }
        catch (Exception ex)
        {
            return $"<scene-error:{ex.GetType().Name}>";
        }

        return location.CurrentDomain.ToString();
    }

    private static bool SafeBool(Func<bool> read, ManualLogSource logger, string label)
    {
        try
        {
            return read();
        }
        catch (Exception ex)
        {
            logger.LogWarning($"{Plugin.PluginName} actor scanner could not read {label}: {ex.GetType().Name}: {ex.Message}");
            return false;
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
            logger.LogWarning($"{Plugin.PluginName} actor scanner could not read {label}: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    private static bool IsInterestingComponentType(string typeName)
    {
        return ContainsAny(typeName, "Pet", "Summon", "Ally", "Npc", "NPC", "Actor", "Character", "Creature", "Animal", "Enemy", "Controller", "Brain", "AI", "Nav", "Move", "Agent", "Faction", "Owner", "Master", "Target", "Hostile", "Relation", "Team", "Threat", "Aggro", "Crime", "Guard", "Interaction", "Prompt");
    }

    private static string TypeHintBucket(string typeName)
    {
        if (ContainsAny(typeName, "Faction", "Owner", "Master", "Target", "Hostile", "Relation", "Team", "Threat", "Aggro", "Crime", "Guard"))
        {
            return "faction-target";
        }

        if (ContainsAny(typeName, "Follow", "Recall", "Controller", "Brain", "AI", "Nav", "Move", "Agent"))
        {
            return "control-movement";
        }

        if (ContainsAny(typeName, "Interaction", "Prompt"))
        {
            return "interaction-ui";
        }

        if (ContainsAny(typeName, "Pet", "Summon", "Ally"))
        {
            return "pet-summon-ally";
        }

        return string.Empty;
    }

    private static string FilterTypeHints(string value, params string[] terms)
    {
        string[] parts = value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(part => ContainsAny(part, terms))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(part => part, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return string.Join("|", parts);
    }

    private static bool LooksBlockedByName(string name)
    {
        return ContainsAny(name, "Boss", "Unique", "Quest", "Story", "Tutorial", "Debug", "Challenge", "Interaction");
    }

    private static bool ContainsAny(string value, params string[] terms)
    {
        foreach (string term in terms)
        {
            if (string.Equals(term, "AI", StringComparison.Ordinal))
            {
                if (value.IndexOf(".AI.", StringComparison.Ordinal) >= 0
                    || value.EndsWith(".AI", StringComparison.Ordinal)
                    || value.IndexOf("AI", StringComparison.Ordinal) >= 0)
                {
                    return true;
                }

                continue;
            }

            if (value.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static string JoinDistinct(IEnumerable<string> values)
    {
        return string.Join("|", values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static string TemplateLabel(LocationTemplate template)
    {
        return $"{template.name}[{template.GUID}]";
    }

    private static string LocationLabel(Location location)
    {
        return $"{location.DebugName ?? location.ID ?? "<unknown-location>"}[{location.Template?.GUID ?? "<no-template-guid>"}]";
    }

    private static string TypeName(object value)
    {
        return value.GetType().FullName ?? value.GetType().Name;
    }

    private static string SafeObjectText(object? value)
    {
        return value?.ToString() ?? string.Empty;
    }

    private static string FormatVector(Vector3 value)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:0.###}|{1:0.###}|{2:0.###}", value.x, value.y, value.z);
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static void AppendCsv(StringBuilder builder, string value)
    {
        value = value.Replace("\r", " ").Replace("\n", " ");
        builder.Append('"');
        builder.Append(value.Replace("\"", "\"\""));
        builder.Append('"');
        builder.Append(',');
    }

    private struct KnownActorMarkers
    {
        private HashSet<string>? _typeNames;

        internal IEnumerable<string> TypeNames => _typeNames ?? Enumerable.Empty<string>();

        internal void Add(string value)
        {
            _typeNames ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _typeNames.Add(value);
        }

        internal bool ContainsTypeName(string suffix)
        {
            return _typeNames?.Any(value => value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) == true;
        }
    }

    private sealed class HumanActorRow
    {
        internal HumanActorRow(
            string locationId,
            string debugName,
            string displayName,
            string sceneOrDomain,
            string coords,
            string distanceFromHero,
            bool markedNotSaved,
            string templateGuid,
            string templateName,
            string templateClrType,
            string templateType,
            bool templateIsAbstract,
            string npcAttachmentType,
            string npcIsUnique,
            bool hasNpcElement,
            bool hasAlive,
            bool hasPetElement,
            bool hasPetVariant,
            bool hasHeroSummon,
            bool hasHeroPetAlly,
            bool actorLike,
            string safeControlStatus,
            string blockReason,
            string controlHints,
            string factionTargetHints,
            string componentTypes,
            List<HumanComponentRow> components)
        {
            LocationId = locationId;
            DebugName = debugName;
            DisplayName = displayName;
            SceneOrDomain = sceneOrDomain;
            Coords = coords;
            DistanceFromHero = distanceFromHero;
            MarkedNotSaved = markedNotSaved;
            TemplateGuid = templateGuid;
            TemplateName = templateName;
            TemplateClrType = templateClrType;
            TemplateType = templateType;
            TemplateIsAbstract = templateIsAbstract;
            NpcAttachmentType = npcAttachmentType;
            NpcIsUnique = npcIsUnique;
            HasNpcElement = hasNpcElement;
            HasAlive = hasAlive;
            HasPetElement = hasPetElement;
            HasPetVariant = hasPetVariant;
            HasHeroSummon = hasHeroSummon;
            HasHeroPetAlly = hasHeroPetAlly;
            ActorLike = actorLike;
            SafeControlStatus = safeControlStatus;
            BlockReason = blockReason;
            ControlHints = controlHints;
            FactionTargetHints = factionTargetHints;
            ComponentTypes = componentTypes;
            Components = components;
        }

        internal string LocationId { get; }
        internal string DebugName { get; }
        internal string DisplayName { get; }
        internal string SceneOrDomain { get; }
        internal string Coords { get; }
        internal string DistanceFromHero { get; }
        internal bool MarkedNotSaved { get; }
        internal string TemplateGuid { get; }
        internal string TemplateName { get; }
        internal string TemplateClrType { get; }
        internal string TemplateType { get; }
        internal bool TemplateIsAbstract { get; }
        internal string NpcAttachmentType { get; }
        internal string NpcIsUnique { get; }
        internal bool HasNpcElement { get; }
        internal bool HasAlive { get; }
        internal bool HasPetElement { get; }
        internal bool HasPetVariant { get; }
        internal bool HasHeroSummon { get; }
        internal bool HasHeroPetAlly { get; }
        internal bool ActorLike { get; }
        internal string SafeControlStatus { get; }
        internal string BlockReason { get; }
        internal string ControlHints { get; }
        internal string FactionTargetHints { get; }
        internal string ComponentTypes { get; }
        internal List<HumanComponentRow> Components { get; }
        internal string CandidateLabel => $"{DebugName}/{TemplateName}[{TemplateGuid}]";
        internal int Sort => SafeControlStatus == "HumanProofDryRun" ? 0 : ActorLike ? 1 : 2;
    }

    private readonly struct HumanComponentRow
    {
        internal HumanComponentRow(string locationId, string templateGuid, string componentSource, string componentType, string hintBucket)
        {
            LocationId = locationId;
            TemplateGuid = templateGuid;
            ComponentSource = componentSource;
            ComponentType = componentType;
            HintBucket = hintBucket;
        }

        internal string LocationId { get; }
        internal string TemplateGuid { get; }
        internal string ComponentSource { get; }
        internal string ComponentType { get; }
        internal string HintBucket { get; }
    }

    private readonly struct HumanCommandDryRunRow
    {
        internal HumanCommandDryRunRow(string command, string candidateLabel, string locationId, string templateGuid, string templateName, string safeControlStatus, string commandPath, bool blocked, bool touchesFactionOrTargeting, string reason, bool liveAction)
        {
            Command = command;
            CandidateLabel = candidateLabel;
            LocationId = locationId;
            TemplateGuid = templateGuid;
            TemplateName = templateName;
            SafeControlStatus = safeControlStatus;
            CommandPath = commandPath;
            Blocked = blocked;
            TouchesFactionOrTargeting = touchesFactionOrTargeting;
            Reason = reason;
            LiveAction = liveAction;
        }

        internal string Command { get; }
        internal string CandidateLabel { get; }
        internal string LocationId { get; }
        internal string TemplateGuid { get; }
        internal string TemplateName { get; }
        internal string SafeControlStatus { get; }
        internal string CommandPath { get; }
        internal bool Blocked { get; }
        internal bool TouchesFactionOrTargeting { get; }
        internal string Reason { get; }
        internal bool LiveAction { get; }
    }

    private readonly struct HumanRecruitmentCaptureResearchRow
    {
        internal HumanRecruitmentCaptureResearchRow(
            string locationId,
            string debugName,
            string displayName,
            string templateGuid,
            string templateName,
            string distanceFromHero,
            bool markedNotSaved,
            string npcIsUnique,
            bool hasNpcElement,
            bool hasAlive,
            bool hasHeroPetAlly,
            string existingNpcPolicy,
            string dispositionEvidence,
            string researchLane,
            string riskFlags,
            bool blocked,
            string blockReason,
            string requiredProof,
            bool recruitmentApproved,
            bool captureApproved,
            bool persistenceApproved,
            bool liveAction,
            string notes,
            int sort)
        {
            LocationId = locationId;
            DebugName = debugName;
            DisplayName = displayName;
            TemplateGuid = templateGuid;
            TemplateName = templateName;
            DistanceFromHero = distanceFromHero;
            MarkedNotSaved = markedNotSaved;
            NpcIsUnique = npcIsUnique;
            HasNpcElement = hasNpcElement;
            HasAlive = hasAlive;
            HasHeroPetAlly = hasHeroPetAlly;
            ExistingNpcPolicy = existingNpcPolicy;
            DispositionEvidence = dispositionEvidence;
            ResearchLane = researchLane;
            RiskFlags = riskFlags;
            Blocked = blocked;
            BlockReason = blockReason;
            RequiredProof = requiredProof;
            RecruitmentApproved = recruitmentApproved;
            CaptureApproved = captureApproved;
            PersistenceApproved = persistenceApproved;
            LiveAction = liveAction;
            Notes = notes;
            Sort = sort;
        }

        internal string LocationId { get; }
        internal string DebugName { get; }
        internal string DisplayName { get; }
        internal string TemplateGuid { get; }
        internal string TemplateName { get; }
        internal string DistanceFromHero { get; }
        internal bool MarkedNotSaved { get; }
        internal string NpcIsUnique { get; }
        internal bool HasNpcElement { get; }
        internal bool HasAlive { get; }
        internal bool HasHeroPetAlly { get; }
        internal string ExistingNpcPolicy { get; }
        internal string DispositionEvidence { get; }
        internal string ResearchLane { get; }
        internal string RiskFlags { get; }
        internal bool Blocked { get; }
        internal string BlockReason { get; }
        internal string RequiredProof { get; }
        internal bool RecruitmentApproved { get; }
        internal bool CaptureApproved { get; }
        internal bool PersistenceApproved { get; }
        internal bool LiveAction { get; }
        internal string Notes { get; }
        internal int Sort { get; }
    }
}
