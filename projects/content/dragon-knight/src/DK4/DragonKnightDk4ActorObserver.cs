using System;
using System.Linq;
using Awaken.TG.Assets;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Setup;
using Awaken.TG.Main.Templates;
using Awaken.TG.MVC;
using UnityEngine;

namespace DragonKnight;

internal static class DragonKnightDk4ActorObserver
{
    private static readonly string[] ExpectedLocationComponents =
    {
        "UnityEngine.Transform",
        "Awaken.TG.Main.Locations.Setup.LocationSpec",
        "Awaken.TG.Main.Locations.Setup.LocationTemplate",
        "Awaken.TG.Main.Fights.NPCs.RepetitiveNpcAttachment",
        "Awaken.TG.Main.AI.Idle.Data.Attachment.IdleDataAttachment",
        "Awaken.TG.Main.AI.Combat.Attachments.Customs.CustomCombatAttachment",
        "Awaken.TG.Main.Heroes.Items.Attachments.Audio.AliveAudioAttachment",
        "Awaken.TG.Main.Maps.Markers.MarkerAttachment",
    };

    internal static bool TryFindTarget(
        out DragonKnightDk4TargetObservation target,
        out string reason)
    {
        target = default;

        Location[] locations;
        try
        {
            locations = World.All<Location>().ToArraySlow();
        }
        catch (Exception ex)
        {
            reason = "World.All<Location>() failed: " + ex.GetType().Name + ": " + ex.Message;
            return false;
        }

        Location? match = null;
        int matches = 0;
        foreach (Location location in locations)
        {
            if (location == null || location.HasBeenDiscarded)
            {
                continue;
            }

            if (!string.Equals(location.ID, DragonKnightDk4DiagnosticOptions.TargetLocationId, StringComparison.Ordinal))
            {
                continue;
            }

            match = location;
            matches++;
        }

        if (matches <= 0 || match == null)
        {
            reason = "missing target source " + DragonKnightDk4DiagnosticOptions.TargetSource + " with Location.ID " + DragonKnightDk4DiagnosticOptions.TargetLocationId;
            return false;
        }

        if (matches > 1)
        {
            reason = "duplicate target Location.ID " + DragonKnightDk4DiagnosticOptions.TargetLocationId;
            return false;
        }

        target = new DragonKnightDk4TargetObservation(
            DragonKnightDk4DiagnosticOptions.TargetSource,
            DragonKnightDk4DiagnosticOptions.TargetKind,
            match.ID,
            DragonKnightDk4DiagnosticOptions.TargetId,
            SafeLocationName(match),
            match.Coords);
        reason = string.Empty;
        return true;
    }

    internal static bool TryResolveTemplate(out LocationTemplate? template, out string reason)
    {
        template = null;
        try
        {
            template = new TemplateReference(DragonKnightDk4DiagnosticOptions.LocationTemplateGuid).Get<LocationTemplate>();
            if (!TryValidateTemplate(template, out reason))
            {
                template = null;
                return false;
            }

            reason = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            reason = "Dragon Knight LocationTemplate resolution failed: " + ex.GetType().Name + ": " + ex.Message;
            return false;
        }
    }

    internal static bool TryPrepareTemplateForSpawn(LocationTemplate template, out string reason)
    {
        RepetitiveNpcAttachment? attachment = template.GetComponent<RepetitiveNpcAttachment>();
        if (attachment == null)
        {
            reason = "Dragon Knight LocationTemplate is missing RepetitiveNpcAttachment.";
            return false;
        }

        NpcTemplate npcTemplate = attachment.NpcTemplate;
        if (npcTemplate == null ||
            !string.Equals(npcTemplate.GUID, DragonKnightDk4DiagnosticOptions.NpcTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = "Dragon Knight NpcTemplate identity changed before spawn.";
            return false;
        }

        attachment.Setup(npcTemplate, new ARAssetReference(DragonKnightDk4DiagnosticOptions.NativeVisualAddress));
        ARAssetReference visual = attachment.VisualPrefab();
        if (visual == null ||
            !string.Equals(visual.Address, DragonKnightDk4DiagnosticOptions.NativeVisualAddress, StringComparison.Ordinal))
        {
            reason = "Dragon Knight native bootstrap visual did not read back.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    internal static bool TryValidateSpawnedActor(
        Location? location,
        DragonKnightDk4TargetObservation target,
        out DragonKnightDk4ActorObservation actor,
        out string reason)
    {
        actor = default;

        if (location == null || location.HasBeenDiscarded)
        {
            reason = "missing actor source or discarded actor.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(location.ID))
        {
            reason = "missing actor Location.ID.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(target.LocationId))
        {
            reason = "missing target Location.ID.";
            return false;
        }

        string templateName = location.Template?.name ?? string.Empty;
        string templateGuid = location.Template?.GUID ?? string.Empty;
        if (!string.Equals(templateName, DragonKnightDk4DiagnosticOptions.LocationTemplateName, StringComparison.Ordinal) ||
            !string.Equals(templateGuid, DragonKnightDk4DiagnosticOptions.LocationTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = "spawned actor template identity changed.";
            return false;
        }

        string locationId = location.ID ?? string.Empty;
        if (CountLiveDragonKnightLocations(locationId) != 1)
        {
            reason = "duplicate Dragon Knight actor source.";
            return false;
        }

        if (!location.TryGetElement(out NpcElement npc) || npc == null || npc.HasBeenDiscarded)
        {
            reason = "missing live NpcElement for Dragon Knight actor.";
            return false;
        }

        string npcTemplateGuid = npc.Template?.GUID ?? string.Empty;
        if (!string.Equals(npcTemplateGuid, DragonKnightDk4DiagnosticOptions.NpcTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = "spawned actor NpcTemplate identity changed.";
            return false;
        }

        if (!location.MarkedNotSaved || !location.IsNotSaved)
        {
            reason = "Dragon Knight actor save exclusion is not active.";
            return false;
        }

        actor = new DragonKnightDk4ActorObservation(
            DragonKnightDk4DiagnosticOptions.ActorSource,
            locationId,
            "foa.location:" + locationId,
            templateName,
            templateGuid,
            npcTemplateGuid,
            location.DisplayName ?? string.Empty,
            location.Coords,
            location.MarkedNotSaved,
            location.IsNotSaved);
        reason = string.Empty;
        return true;
    }

    internal static bool TryGetReadyHero(out Hero hero, out string reason)
    {
        hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            reason = "Hero is not available.";
            return false;
        }

        if (!hero.MainViewInitialized)
        {
            reason = "Hero view is not ready.";
            return false;
        }

        if (!hero.IsAlive)
        {
            reason = "Hero is not alive.";
            return false;
        }

        if (hero.IsPortaling)
        {
            reason = "Hero is portaling.";
            return false;
        }

        if (hero.IsSwimming)
        {
            reason = "Hero is swimming.";
            return false;
        }

        if (hero.HeroCombat?.IsHeroInFight == true)
        {
            reason = "Hero is already in combat.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static bool TryValidateTemplate(LocationTemplate? template, out string reason)
    {
        if (template == null)
        {
            reason = "the exact Dragon Knight LocationTemplate did not resolve from native templates.";
            return false;
        }

        if (!string.Equals(template.name, DragonKnightDk4DiagnosticOptions.LocationTemplateName, StringComparison.Ordinal) ||
            !string.Equals(template.GUID, DragonKnightDk4DiagnosticOptions.LocationTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = "the Dragon Knight LocationTemplate identity changed.";
            return false;
        }

        RepetitiveNpcAttachment? attachment = template.GetComponent<RepetitiveNpcAttachment>();
        if (attachment == null || attachment.IsUnique)
        {
            reason = "the Dragon Knight LocationTemplate is missing its non-unique RepetitiveNpcAttachment contract.";
            return false;
        }

        string npcTemplateGuid = attachment.NpcTemplate?.GUID ?? string.Empty;
        if (!string.Equals(npcTemplateGuid, DragonKnightDk4DiagnosticOptions.NpcTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = "the Dragon Knight NpcTemplate GUID changed.";
            return false;
        }

        string[] actualComponents = template.GetComponents<Component>()
            .Where(component => component != null)
            .Select(component => component.GetType().FullName ?? component.GetType().Name)
            .ToArray();
        if (!actualComponents.SequenceEqual(ExpectedLocationComponents, StringComparer.Ordinal))
        {
            reason = "the Dragon Knight LocationTemplate component order changed.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static int CountLiveDragonKnightLocations(string actorLocationId)
    {
        int count = 0;
        foreach (Location location in World.All<Location>().ToArraySlow())
        {
            if (location == null || location.HasBeenDiscarded)
            {
                continue;
            }

            if (string.Equals(location.Template?.GUID ?? string.Empty, DragonKnightDk4DiagnosticOptions.LocationTemplateGuid, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(location.ID, actorLocationId, StringComparison.Ordinal))
            {
                count++;
            }
        }

        return count;
    }

    private static string SafeLocationName(Location location)
    {
        return string.IsNullOrWhiteSpace(location.DisplayName)
            ? location.Template?.name ?? "unknown"
            : location.DisplayName;
    }
}

internal readonly struct DragonKnightDk4TargetObservation
{
    internal DragonKnightDk4TargetObservation(
        string source,
        string kind,
        string locationId,
        string targetId,
        string displayName,
        Vector3 position)
    {
        Source = source;
        Kind = kind;
        LocationId = locationId;
        TargetId = targetId;
        DisplayName = displayName;
        Position = position;
    }

    internal string Source { get; }

    internal string Kind { get; }

    internal string LocationId { get; }

    internal string TargetId { get; }

    internal string DisplayName { get; }

    internal Vector3 Position { get; }
}

internal readonly struct DragonKnightDk4ActorObservation
{
    internal DragonKnightDk4ActorObservation(
        string source,
        string locationId,
        string actorId,
        string templateName,
        string templateGuid,
        string npcTemplateGuid,
        string displayName,
        Vector3 position,
        bool markedNotSaved,
        bool isNotSaved)
    {
        Source = source;
        LocationId = locationId;
        ActorId = actorId;
        TemplateName = templateName;
        TemplateGuid = templateGuid;
        NpcTemplateGuid = npcTemplateGuid;
        DisplayName = displayName;
        Position = position;
        MarkedNotSaved = markedNotSaved;
        IsNotSaved = isNotSaved;
    }

    internal string Source { get; }

    internal string LocationId { get; }

    internal string ActorId { get; }

    internal string TemplateName { get; }

    internal string TemplateGuid { get; }

    internal string NpcTemplateGuid { get; }

    internal string DisplayName { get; }

    internal Vector3 Position { get; }

    internal bool MarkedNotSaved { get; }

    internal bool IsNotSaved { get; }
}
