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
using Awaken.TG.Main.Templates;
using Awaken.TG.MVC;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AvalonCompanions.Patches;

internal static class ActorControlDiagnostics
{
    private const int MaxComponentTypesPerLocation = 64;

    private static bool _logged;

    internal static void TryLog(ManualLogSource logger)
    {
        if (_logged
            || !Plugin.Enabled.Value
            || !Plugin.ActorControlProofEnabled.Value
            || !Plugin.DumpActorControlCandidates.Value)
        {
            return;
        }

        try
        {
            if (Hero.Current == null)
            {
                return;
            }

            TemplatesProvider? provider = World.Services?.TryGet<TemplatesProvider>();
            if (provider?.AllLoaded != true)
            {
                return;
            }

            _logged = true;
            WriteActorControlDiagnostics(logger);
        }
        catch (Exception ex)
        {
            _logged = true;
            logger.LogWarning($"Actor control diagnostics failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void WriteActorControlDiagnostics(ManualLogSource logger)
    {
        string folder = Path.Combine(Paths.ConfigPath, Plugin.PluginGuid);
        Directory.CreateDirectory(folder);

        List<ActorControlCandidateRow> candidates = CollectCandidates(logger);
        ActorControlCandidateRow? selected = SelectDryRunCandidate(candidates);
        List<ActorControlCommandDryRunRow> commands = BuildCommandDryRunRows(selected);

        string candidatesPath = Path.Combine(folder, "actor-control-candidates.csv");
        string componentsPath = Path.Combine(folder, "actor-control-components.csv");
        string commandsPath = Path.Combine(folder, "actor-control-command-dry-run.csv");

        WriteCandidatesCsv(candidatesPath, candidates);
        WriteComponentsCsv(componentsPath, candidates);
        WriteCommandDryRunCsv(commandsPath, commands);

        logger.LogInfo(
            "Actor control diagnostics wrote "
            + $"{candidates.Count} candidate row(s), "
            + $"{candidates.Sum(row => row.Components.Count)} component row(s), and "
            + $"{commands.Count} dry-run command row(s) to {folder}. "
            + "No actor movement, spawning, faction, targeting, quest, or save state was changed.");

        foreach (ActorControlCommandDryRunRow row in commands)
        {
            logger.LogInfo(
                "Actor control dry-run: "
                + $"command={row.Command}; "
                + $"candidate={row.CandidateLabel}; "
                + $"status={row.SafeControlStatus}; "
                + $"path={row.CommandPath}; "
                + $"blocked={row.Blocked}; "
                + $"touchesFactionOrTargeting={row.TouchesFactionOrTargeting}; "
                + $"reason={row.Reason}; "
                + "liveAction=false.");
        }
    }

    private static List<ActorControlCandidateRow> CollectCandidates(ManualLogSource logger)
    {
        float radius = Math.Max(1f, Plugin.ActorControlScanRadius.Value);
        float radiusSquared = radius * radius;
        Vector3 heroCoords = Hero.Current.Coords;
        Dictionary<Location, KnownActorMarkers> markers = CollectKnownMarkers(logger);

        List<ActorControlCandidateRow> candidates = new List<ActorControlCandidateRow>();
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
            ActorControlCandidateRow row = ToCandidateRow(location, knownMarkers, Mathf.Sqrt(distanceSquared), logger);
            if (row.ActorLike || row.Components.Count > 0 || LooksLikeWolfOrBear(row.TemplateName))
            {
                candidates.Add(row);
            }
        }

        return candidates
            .OrderBy(row => row.SafeControlSort)
            .ThenBy(row => row.DistanceFromHero)
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

    private static void AddMarker(Dictionary<Location, KnownActorMarkers> markers, Location? location, string marker, ManualLogSource logger)
    {
        if (location == null)
        {
            logger.LogInfo($"Actor control diagnostics saw marker {marker}, but could not resolve a parent Location.");
            return;
        }

        markers.TryGetValue(location, out KnownActorMarkers existing);
        existing.Add(marker);
        markers[location] = existing;
    }

    private static Location? ParentLocation(object value, ManualLogSource logger)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        string[] propertyNames =
        {
            "ParentModel",
            "GenericParentModel",
            "Parent",
            "Location",
        };

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
                logger.LogWarning($"Actor control diagnostics could not read {type.FullName}.{propertyName}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        return null;
    }

    private static ActorControlCandidateRow ToCandidateRow(Location location, KnownActorMarkers markers, float distanceFromHero, ManualLogSource logger)
    {
        LocationTemplate? template = location.Template;
        string templateName = template?.name ?? string.Empty;
        string templateGuid = template?.GUID ?? string.Empty;
        string templateType = template?.TemplateType.ToString() ?? string.Empty;
        string templateClrType = template?.GetType().FullName ?? template?.GetType().Name ?? string.Empty;
        bool templateIsAbstract = template?.IsAbstract ?? false;
        NpcAttachment? npcAttachment = template == null
            ? null
            : SafeGet(() => template.GetComponent<NpcAttachment>(), logger, $"NpcAttachment for {templateName}[{templateGuid}]");
        string npcAttachmentType = npcAttachment == null ? string.Empty : TypeName(npcAttachment);
        string npcIsUnique = npcAttachment == null ? "unknown" : npcAttachment.IsUnique ? "true" : "false";
        bool hasAlive = SafeBool(() => location.TryGetElement<IAlive>() != null, logger, $"IAlive for {LocationLabel(location)}");
        bool hasPetElement = markers.ContainsTypeName("PetElement") || SafeBool(() => location.TryGetElement<PetElement>() != null, logger, $"PetElement for {LocationLabel(location)}");
        bool hasPetVariant = markers.ContainsTypeName("PetVariantBase") || SafeBool(() => location.TryGetElement<PetVariantBase>() != null, logger, $"PetVariantBase for {LocationLabel(location)}");
        bool hasSummon = markers.ContainsTypeName("NpcHeroSummon");
        bool hasPetAlly = markers.ContainsTypeName("NpcHeroPetAlly");
        List<ActorControlComponentRow> components = CollectComponentRows(location, markers, npcAttachmentType, logger);
        string componentTypes = JoinDistinct(components.Select(row => row.ComponentType));
        string controlHints = FilterTypeHints(componentTypes, "Pet", "Summon", "Ally", "Follow", "Recall", "Controller", "Brain", "AI", "Nav", "Move", "Agent");
        string factionHints = FilterTypeHints(componentTypes, "Faction", "Owner", "Master", "Target", "Hostile", "Relation", "Team", "Threat", "Aggro", "Crime", "Guard");
        string blockReason = BuildBlockReason(templateName, templateGuid, npcIsUnique, hasPetElement, hasPetVariant, hasSummon, hasPetAlly, hasAlive);
        string safeControlStatus = BuildSafeControlStatus(blockReason, hasPetElement, hasPetVariant, hasSummon, hasPetAlly, hasAlive);
        string commandPath = BuildCommandPath(hasPetElement, hasPetVariant, hasSummon, hasPetAlly);
        bool actorLike = hasPetElement || hasPetVariant || hasSummon || hasPetAlly || hasAlive || npcAttachment != null;

        return new ActorControlCandidateRow(
            location.ID ?? string.Empty,
            location.DebugName ?? string.Empty,
            SafeObjectText(location.DisplayName),
            SceneOrDomain(location),
            FormatVector(location.Coords),
            FormatFloat(distanceFromHero),
            location.MarkedNotSaved,
            templateGuid,
            templateName,
            templateClrType,
            templateType,
            templateIsAbstract,
            npcAttachmentType,
            npcIsUnique,
            hasAlive,
            hasPetElement,
            hasPetVariant,
            hasSummon,
            hasPetAlly,
            actorLike,
            safeControlStatus,
            blockReason,
            commandPath,
            controlHints,
            factionHints,
            componentTypes,
            $"{location.ID} {location.DebugName} {templateGuid} {templateName} {componentTypes}",
            components);
    }

    private static List<ActorControlComponentRow> CollectComponentRows(Location location, KnownActorMarkers markers, string npcAttachmentType, ManualLogSource logger)
    {
        List<ActorControlComponentRow> rows = new List<ActorControlComponentRow>();
        string key = location.ID ?? string.Empty;
        string templateGuid = location.Template?.GUID ?? string.Empty;

        foreach (string marker in markers.TypeNames)
        {
            rows.Add(new ActorControlComponentRow(key, templateGuid, "known-world-marker", marker, string.Empty));
        }

        if (!string.IsNullOrWhiteSpace(npcAttachmentType))
        {
            rows.Add(new ActorControlComponentRow(key, templateGuid, "template-component", npcAttachmentType, "NpcAttachment"));
        }

        AddLocationElementRow<PetElement>(rows, location, "known-location-element");
        AddLocationElementRow<PetVariantBase>(rows, location, "known-location-element");
        AddLocationElementRow<IAlive>(rows, location, "known-location-element");

        foreach (string component in ViewComponentTypeNames(location, logger))
        {
            rows.Add(new ActorControlComponentRow(key, templateGuid, "view-component", component, TypeHintBucket(component)));
        }

        return rows
            .GroupBy(row => $"{row.ComponentSource}|{row.ComponentType}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(row => row.ComponentSource, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.ComponentType, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static void AddLocationElementRow<T>(List<ActorControlComponentRow> rows, Location location, string source) where T : class, IModel
    {
        if (location.TryGetElement<T>() == null)
        {
            return;
        }

        rows.Add(new ActorControlComponentRow(
            location.ID ?? string.Empty,
            location.Template?.GUID ?? string.Empty,
            source,
            typeof(T).FullName ?? typeof(T).Name,
            TypeHintBucket(typeof(T).FullName ?? typeof(T).Name)));
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
            logger.LogWarning($"Actor control diagnostics could not read ViewParent for {LocationLabel(location)}: {ex.GetType().Name}: {ex.Message}");
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
            logger.LogWarning($"Actor control diagnostics could not read view components for {LocationLabel(location)}: {ex.GetType().Name}: {ex.Message}");
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

    private static bool IsInterestingComponentType(string typeName)
    {
        return ContainsAny(typeName, "Pet", "Summon", "Ally", "Npc", "NPC", "Actor", "Character", "Creature", "Animal", "Enemy", "Controller", "Brain", "AI", "Nav", "Move", "Agent", "Faction", "Owner", "Master", "Target", "Hostile", "Relation", "Team", "Threat", "Aggro");
    }

    private static string BuildBlockReason(string templateName, string templateGuid, string npcIsUnique, bool hasPetElement, bool hasPetVariant, bool hasSummon, bool hasPetAlly, bool hasAlive)
    {
        List<string> reasons = new List<string>();
        if (LooksLikeWolfOrBear(templateName))
        {
            reasons.Add("wolf/bear behavior remains blocked by diagnostic review");
        }

        if (LooksBlockedByName(templateName))
        {
            reasons.Add("blocked by story/unique/boss/quest/debug/challenge name risk");
        }

        if (string.Equals(npcIsUnique, "true", StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add("NpcAttachment.IsUnique=true");
        }

        if (!hasPetElement && !hasPetVariant && !hasSummon && !hasPetAlly && !hasAlive)
        {
            reasons.Add("no known pet, summon, ally, or alive actor marker");
        }

        if (string.IsNullOrWhiteSpace(templateGuid))
        {
            reasons.Add("missing template GUID");
        }

        return reasons.Count == 0 ? string.Empty : string.Join("; ", reasons);
    }

    private static string BuildSafeControlStatus(string blockReason, bool hasPetElement, bool hasPetVariant, bool hasSummon, bool hasPetAlly, bool hasAlive)
    {
        if (!string.IsNullOrWhiteSpace(blockReason))
        {
            return "Blocked";
        }

        if (hasPetElement || hasPetVariant || hasSummon || hasPetAlly)
        {
            return "Candidate";
        }

        if (hasAlive)
        {
            return "DryRunOnly";
        }

        return "Blocked";
    }

    private static string BuildCommandPath(bool hasPetElement, bool hasPetVariant, bool hasSummon, bool hasPetAlly)
    {
        List<string> paths = new List<string>();
        if (hasPetElement)
        {
            paths.Add("PetElement.SetFollowing(bool)");
            paths.Add("PetElement.Recall(Vector3)");
        }

        if (hasPetVariant)
        {
            paths.Add("PetVariantBase.SetFollowing(bool)");
        }

        if (hasSummon)
        {
            paths.Add("NpcHeroSummon marker only; native control method not approved");
        }

        if (hasPetAlly)
        {
            paths.Add("NpcHeroPetAlly marker only; native control method not approved");
        }

        return paths.Count == 0 ? "no approved command path" : string.Join("|", paths.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static ActorControlCandidateRow? SelectDryRunCandidate(List<ActorControlCandidateRow> candidates)
    {
        string selected = Plugin.SelectedActorControlCandidateGuid.Value.Trim();
        if (!string.IsNullOrWhiteSpace(selected))
        {
            ActorControlCandidateRow? configured = candidates.FirstOrDefault(row =>
                string.Equals(row.TemplateGuid, selected, StringComparison.OrdinalIgnoreCase)
                || string.Equals(row.LocationId, selected, StringComparison.OrdinalIgnoreCase));
            if (configured != null)
            {
                return configured;
            }
        }

        return candidates.FirstOrDefault(row => row.SafeControlStatus == "Candidate")
            ?? candidates.FirstOrDefault(row => row.SafeControlStatus == "DryRunOnly")
            ?? candidates.FirstOrDefault();
    }

    private static List<ActorControlCommandDryRunRow> BuildCommandDryRunRows(ActorControlCandidateRow? selected)
    {
        string[] commands = { "follow", "stay", "recall", "defend" };
        return commands.Select(command => BuildCommandDryRunRow(selected, command)).ToList();
    }

    private static ActorControlCommandDryRunRow BuildCommandDryRunRow(ActorControlCandidateRow? selected, string command)
    {
        if (selected == null)
        {
            return ActorControlCommandDryRunRow.CreateBlocked(command, "no selected candidate", "Blocked", "no candidate row was available", "no command path");
        }

        bool dryRunOnly = Plugin.ActorControlDryRunOnly.Value || !Plugin.AllowOneSessionPetControl.Value;
        string path = CommandPathFor(selected, command);
        bool noApprovedPath = path.StartsWith("no approved", StringComparison.OrdinalIgnoreCase)
            || path.IndexOf("not approved", StringComparison.OrdinalIgnoreCase) >= 0;
        bool blocked = selected.SafeControlStatus == "Blocked" || command == "defend" || !dryRunOnly || noApprovedPath;
        string reason;
        bool touchesFactionOrTargeting = command == "defend";

        if (selected.SafeControlStatus == "Blocked")
        {
            reason = selected.BlockReason;
        }
        else if (noApprovedPath)
        {
            reason = "no approved command path for selected candidate in this phase";
        }
        else if (command == "defend")
        {
            reason = Plugin.AllowDefendAction.Value
                ? "defend remains dry-run only until faction matrix validation passes"
                : "ActorControlProof.AllowDefendAction=false; faction and target safety not approved";
            blocked = true;
        }
        else if (!dryRunOnly)
        {
            reason = "one-session live control is not implemented in this phase";
            blocked = true;
        }
        else
        {
            reason = "dry run only; no live actor state changed";
            blocked = false;
        }

        return new ActorControlCommandDryRunRow(
            command,
            selected.CandidateLabel,
            selected.LocationId,
            selected.TemplateGuid,
            selected.TemplateName,
            selected.SafeControlStatus,
            path,
            blocked,
            touchesFactionOrTargeting,
            reason);
    }

    private static string CommandPathFor(ActorControlCandidateRow selected, string command)
    {
        if (command == "follow")
        {
            return selected.HasPetElement || selected.HasPetVariant
                ? JoinDistinct(new[]
                {
                    selected.HasPetElement ? "PetElement.SetFollowing(true)" : string.Empty,
                    selected.HasPetVariant ? "PetVariantBase.SetFollowing(true)" : string.Empty,
                })
                : selected.CommandPath;
        }

        if (command == "stay")
        {
            return selected.HasPetElement || selected.HasPetVariant
                ? JoinDistinct(new[]
                {
                    selected.HasPetElement ? "PetElement.SetFollowing(false)" : string.Empty,
                    selected.HasPetVariant ? "PetVariantBase.SetFollowing(false)" : string.Empty,
                })
                : selected.CommandPath;
        }

        if (command == "recall")
        {
            return selected.HasPetElement
                ? "PetElement.Recall(Vector3)"
                : "no approved recall path";
        }

        return "no approved defend path; faction/target hooks not validated";
    }

    private static void WriteCandidatesCsv(string path, IEnumerable<ActorControlCandidateRow> rows)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("locationId,debugName,displayName,sceneOrDomain,coords,distanceFromHero,markedNotSaved,templateGuid,templateName,templateClrType,templateType,templateIsAbstract,npcAttachmentType,npcIsUnique,hasAlive,hasPetElement,hasPetVariant,hasHeroSummon,hasHeroPetAlly,actorLike,safeControlStatus,blockReason,commandPath,controlHints,factionTargetHints,componentTypes,safeSpawnCandidate,rosterApproved,notes");
        foreach (ActorControlCandidateRow row in rows)
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
            AppendCsv(builder, row.HasAlive ? "true" : "false");
            AppendCsv(builder, row.HasPetElement ? "true" : "false");
            AppendCsv(builder, row.HasPetVariant ? "true" : "false");
            AppendCsv(builder, row.HasHeroSummon ? "true" : "false");
            AppendCsv(builder, row.HasHeroPetAlly ? "true" : "false");
            AppendCsv(builder, row.ActorLike ? "true" : "false");
            AppendCsv(builder, row.SafeControlStatus);
            AppendCsv(builder, row.BlockReason);
            AppendCsv(builder, row.CommandPath);
            AppendCsv(builder, row.ControlHints);
            AppendCsv(builder, row.FactionTargetHints);
            AppendCsv(builder, row.ComponentTypes);
            AppendCsv(builder, "false");
            AppendCsv(builder, "false");
            AppendCsv(builder, "Diagnostics-only actor-control proof row; no spawn, roster approval, faction, targeting, quest, or save behavior.");
            builder.Length--;
            builder.AppendLine();
        }

        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
    }

    private static void WriteComponentsCsv(string path, IEnumerable<ActorControlCandidateRow> rows)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("locationId,templateGuid,componentSource,componentType,hintBucket");
        foreach (ActorControlComponentRow row in rows.SelectMany(candidate => candidate.Components))
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

    private static void WriteCommandDryRunCsv(string path, IEnumerable<ActorControlCommandDryRunRow> rows)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("command,candidateLabel,locationId,templateGuid,templateName,safeControlStatus,commandPath,blocked,touchesFactionOrTargeting,reason,liveAction");
        foreach (ActorControlCommandDryRunRow row in rows)
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
            AppendCsv(builder, "false");
            builder.Length--;
            builder.AppendLine();
        }

        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
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
            logger.LogWarning($"Actor control diagnostics could not read {label}: {ex.GetType().Name}: {ex.Message}");
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
            logger.LogWarning($"Actor control diagnostics could not read {label}: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    private static string TypeName(object value)
    {
        return value.GetType().FullName ?? value.GetType().Name;
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

    private static bool LooksLikeWolfOrBear(string name)
    {
        return ContainsAny(name, "Wolf", "Bear");
    }

    private static bool ContainsAny(string value, params string[] terms)
    {
        foreach (string term in terms)
        {
            if (string.Equals(term, "AI", StringComparison.Ordinal))
            {
                if (ContainsAiHint(value))
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

    private static bool ContainsAiHint(string value)
    {
        return value.IndexOf(".AI.", StringComparison.Ordinal) >= 0
            || value.EndsWith(".AI", StringComparison.Ordinal)
            || value.IndexOf("AI", StringComparison.Ordinal) >= 0;
    }

    private static string JoinDistinct(IEnumerable<string> values)
    {
        return string.Join("|", values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static string FormatVector(Vector3 value)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:0.###}|{1:0.###}|{2:0.###}", value.x, value.y, value.z);
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string SafeObjectText(object? value)
    {
        return value?.ToString() ?? string.Empty;
    }

    private static string LocationLabel(Location location)
    {
        return $"{location.DebugName ?? location.ID ?? "<unknown-location>"}[{location.Template?.GUID ?? "<no-template-guid>"}]";
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

    private sealed class ActorControlCandidateRow
    {
        internal ActorControlCandidateRow(
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
            bool hasAlive,
            bool hasPetElement,
            bool hasPetVariant,
            bool hasHeroSummon,
            bool hasHeroPetAlly,
            bool actorLike,
            string safeControlStatus,
            string blockReason,
            string commandPath,
            string controlHints,
            string factionTargetHints,
            string componentTypes,
            string templateSearchText,
            List<ActorControlComponentRow> components)
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
            HasAlive = hasAlive;
            HasPetElement = hasPetElement;
            HasPetVariant = hasPetVariant;
            HasHeroSummon = hasHeroSummon;
            HasHeroPetAlly = hasHeroPetAlly;
            ActorLike = actorLike;
            SafeControlStatus = safeControlStatus;
            BlockReason = blockReason;
            CommandPath = commandPath;
            ControlHints = controlHints;
            FactionTargetHints = factionTargetHints;
            ComponentTypes = componentTypes;
            TemplateSearchText = templateSearchText;
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

        internal bool HasAlive { get; }

        internal bool HasPetElement { get; }

        internal bool HasPetVariant { get; }

        internal bool HasHeroSummon { get; }

        internal bool HasHeroPetAlly { get; }

        internal bool ActorLike { get; }

        internal string SafeControlStatus { get; }

        internal string BlockReason { get; }

        internal string CommandPath { get; }

        internal string ControlHints { get; }

        internal string FactionTargetHints { get; }

        internal string ComponentTypes { get; }

        internal string TemplateSearchText { get; }

        internal List<ActorControlComponentRow> Components { get; }

        internal string CandidateLabel => $"{DebugName}/{TemplateName}[{TemplateGuid}]";

        internal int SafeControlSort => SafeControlStatus == "Candidate" ? 0 : SafeControlStatus == "DryRunOnly" ? 1 : 2;
    }

    private readonly struct ActorControlComponentRow
    {
        internal ActorControlComponentRow(string locationId, string templateGuid, string componentSource, string componentType, string hintBucket)
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

    private readonly struct ActorControlCommandDryRunRow
    {
        internal ActorControlCommandDryRunRow(string command, string candidateLabel, string locationId, string templateGuid, string templateName, string safeControlStatus, string commandPath, bool blocked, bool touchesFactionOrTargeting, string reason)
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

        internal static ActorControlCommandDryRunRow CreateBlocked(string command, string candidateLabel, string safeControlStatus, string reason, string commandPath)
        {
            return new ActorControlCommandDryRunRow(command, candidateLabel, string.Empty, string.Empty, string.Empty, safeControlStatus, commandPath, true, command == "defend", reason);
        }
    }
}
