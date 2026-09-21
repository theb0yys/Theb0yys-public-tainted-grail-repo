using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Setup;
using Awaken.TG.Main.Locations.Spawners;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace DragonKnight;

internal sealed class DragonKnightNativeSpawnerPlacementSystem : IDisposable
{
    private static readonly FieldInfo? LocationsToSpawnField =
        typeof(LocationSpawner).GetField("_locationsToSpawn", BindingFlags.Instance | BindingFlags.NonPublic);

    private readonly ManualLogSource _logger;
    private readonly ConfigEntry<bool> _enabled;
    private readonly ConfigEntry<bool> _killSwitch;
    private readonly Dictionary<LocationSpawner, NativeSpawnerSlot> _slots = new();
    private readonly HashSet<string> _placedLocationIds = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _lastLogByKey = new(StringComparer.Ordinal);
    private bool _disposed;

    internal DragonKnightNativeSpawnerPlacementSystem(ManualLogSource logger, ConfigFile config)
    {
        _logger = logger;
        _enabled = config.Bind(
            DragonKnightNativeSpawnerPlacementContract.ConfigSection,
            "Enabled",
            DragonKnightNativeSpawnerPlacementContract.DefaultEnabled,
            "Enable the DK5D native LocationSpawner candidate placement route for Dragon Knight.");
        _killSwitch = config.Bind(
            DragonKnightNativeSpawnerPlacementContract.ConfigSection,
            "KillSwitch",
            DragonKnightNativeSpawnerPlacementContract.DefaultKillSwitch,
            "Fail-closed DK5D native candidate kill switch. When true, candidate arrays are not changed.");

        _logger.LogInfo(
            "DRAGON_KNIGHT_NATIVE_SPAWNER_CONFIG"
            + " route=" + DragonKnightNativeSpawnerPlacementContract.RouteId
            + " enabled=" + BoolValue(_enabled.Value)
            + " kill-switch=" + BoolValue(_killSwitch.Value)
            + " logical-scene=" + DragonKnightNativeSpawnerPlacementContract.LogicalSceneName
            + " native-scene=" + DragonKnightNativeSpawnerPlacementContract.NativeSpawnerSceneName
            + " actor-source=" + DragonKnightNativeSpawnerPlacementContract.ActorSource
            + " candidate-policy=" + DragonKnightNativeSpawnerPlacementContract.CandidatePolicy
            + " native-slots=" + DragonKnightNativeSpawnerPlacementContract.NativeSpawnerSummary()
            + " native-spawner=1 native-candidate=1 direct-spawn=0 save-write=0");
    }

    private bool LiveArmed => _enabled.Value && !_killSwitch.Value;

    internal void RegisterNativeSpawner(LocationSpawner spawner, LocationSpawnerAttachment spec, bool isRestored)
    {
        if (_disposed || spawner == null || spec == null)
        {
            return;
        }

        string sceneName = SafeSceneName(spec);
        string sourcePath = SafeTransformPath(spec.transform);
        Vector3 position = spec.transform.position;
        if (!string.Equals(sceneName, DragonKnightNativeSpawnerPlacementContract.NativeSpawnerSceneName, StringComparison.Ordinal) ||
            !DragonKnightNativeSpawnerPlacementContract.TryMatchNativeSpawner(position.x, position.y, position.z, out DragonKnightNativeSpawnerAnchorDefinition anchor, out int anchorIndex))
        {
            return;
        }

        if (!string.Equals(sourcePath, anchor.SourcePath, StringComparison.Ordinal))
        {
            LogState("register:" + anchor.AnchorId, "path-mismatch",
                "DRAGON_KNIGHT_NATIVE_SPAWNER_REJECTED"
                + " anchor=" + anchor.AnchorId
                + " expected-path=" + MarkerValue(anchor.SourcePath)
                + " actual-path=" + MarkerValue(sourcePath)
                + " reason=source-path-mismatch"
                + " action=none");
            return;
        }

        LocationTemplate[] sourceTemplates;
        try
        {
            sourceTemplates = spec.LocationsToSpawn.Where(template => template != null).ToArray();
        }
        catch (Exception ex)
        {
            LogState("register:" + anchor.AnchorId, "source-read-failed:" + ex.GetType().Name,
                "DRAGON_KNIGHT_NATIVE_SPAWNER_REJECTED"
                + " anchor=" + anchor.AnchorId
                + " reason=source-read-failed"
                + " error=" + MarkerValue(ex.GetType().Name + ":" + ex.Message)
                + " action=none");
            return;
        }

        bool exactSource = sourceTemplates.Any(template => IsTemplate(template, anchor.SourceTemplateName, anchor.SourceTemplateGuid));
        if (!exactSource)
        {
            LogState("register:" + anchor.AnchorId, "source-template-mismatch",
                "DRAGON_KNIGHT_NATIVE_SPAWNER_REJECTED"
                + " anchor=" + anchor.AnchorId
                + " anchor-index=" + anchorIndex.ToString(CultureInfo.InvariantCulture)
                + " scene=" + sceneName
                + " position=" + FormatVector(position)
                + " expected-source=" + MarkerValue(anchor.SourceTemplateName + "[" + anchor.SourceTemplateGuid + "]")
                + " actual-sources=" + MarkerValue(TemplateSummary(sourceTemplates))
                + " reason=source-template-mismatch"
                + " action=none");
            return;
        }

        if (!TryReadSpawnerTemplates(spawner, out LocationTemplate[] currentTemplates, out string readReason))
        {
            LogState("register:" + anchor.AnchorId, readReason,
                "DRAGON_KNIGHT_NATIVE_SPAWNER_REJECTED"
                + " anchor=" + anchor.AnchorId
                + " reason=" + readReason
                + " action=none");
            return;
        }

        if (!_slots.TryGetValue(spawner, out NativeSpawnerSlot slot))
        {
            slot = new NativeSpawnerSlot(spawner, anchor, currentTemplates);
            _slots.Add(spawner, slot);
            LogState("register:" + anchor.AnchorId, "registered",
                "DRAGON_KNIGHT_NATIVE_SPAWNER_REGISTERED"
                + " route=" + DragonKnightNativeSpawnerPlacementContract.RouteId
                + " anchor=" + anchor.AnchorId
                + " anchor-index=" + anchorIndex.ToString(CultureInfo.InvariantCulture)
                + " scene=" + sceneName
                + " source-path=" + MarkerValue(sourcePath)
                + " source-position=" + FormatVector(position)
                + " source=" + MarkerValue(anchor.SourceTemplateName + "[" + anchor.SourceTemplateGuid + "]")
                + " restored=" + BoolValue(isRestored)
                + " source-distance-to-boss-root-m=" + FormatFloat(DragonKnightNativeSpawnerPlacementContract.SourceSpawnerDistanceToBossRootMeters)
                + " native-spawner=LocationSpawner action=register-native-candidate-owner save-owner=BaseLocationSpawner");
        }
        else
        {
            slot.Refresh(currentTemplates);
        }

        TryInstallSlot(slot, isRestored ? "restore" : "initialize");
    }

    internal void ObserveNativeSpawn(BaseLocationSpawner owner, Location location, int id)
    {
        if (_disposed || !LiveArmed || owner is not LocationSpawner spawner || location == null || !IsDragonKnightTemplate(location.Template))
        {
            return;
        }

        if (!_slots.TryGetValue(spawner, out NativeSpawnerSlot slot))
        {
            LogState("spawn:" + SafeLocationId(location), "unmatched-native-spawner",
                "DRAGON_KNIGHT_NATIVE_SPAWNER_SPAWN_REJECTED"
                + " location=" + MarkerValue(SafeLocationId(location))
                + " owner-position=" + FormatVector(owner.Coords)
                + " reason=unmatched-native-spawner"
                + " action=leave-native-location-unchanged");
            return;
        }

        string locationId = SafeLocationId(location);
        if (string.IsNullOrWhiteSpace(locationId) || !_placedLocationIds.Add(locationId))
        {
            return;
        }

        if (location.MarkedNotSaved || location.IsNotSaved)
        {
            LogState("spawn:" + locationId, "save-excluded",
                "DRAGON_KNIGHT_NATIVE_SPAWNER_SPAWN_REJECTED"
                + " location=" + MarkerValue(locationId)
                + " anchor=" + slot.Anchor.AnchorId
                + " markedNotSaved=" + BoolValue(location.MarkedNotSaved)
                + " isNotSaved=" + BoolValue(location.IsNotSaved)
                + " reason=native-location-not-save-owned"
                + " action=leave-native-location-unchanged");
            return;
        }

        Vector3 requested = DragonKnightOwnedPlacementContract.ArenaCenter;
        Vector3 verified = BaseLocationSpawner.VerifyPosition(requested, location.Template, allowSnapToGround: false);
        Quaternion rotation = DragonKnightOwnedPlacementContract.FacingEdgeRotation();
        Vector3 before = location.Coords;
        location.MoveAndRotateTo(verified, rotation, teleport: true);

        string npcTemplateGuid = "<pending>";
        if (location.TryGetElement(out NpcElement npc) && npc != null)
        {
            npcTemplateGuid = npc.Template?.GUID ?? string.Empty;
        }

        _logger.LogWarning(
            "DRAGON_KNIGHT_NATIVE_SPAWNER_CANDIDATE_PLACED"
            + " route=" + DragonKnightNativeSpawnerPlacementContract.RouteId
            + " owner=" + DragonKnightOwnedPlacementContract.OwnerId
            + " encounter=" + DragonKnightOwnedPlacementContract.EncounterId
            + " placement=" + DragonKnightOwnedPlacementContract.PrimaryPlacementId
            + " anchor=" + slot.Anchor.AnchorId
            + " actor-source=" + DragonKnightNativeSpawnerPlacementContract.ActorSource
            + " actor-location-id=" + MarkerValue(locationId)
            + " actor-id=" + MarkerValue("foa.location:" + locationId)
            + " spawn-id=" + id.ToString(CultureInfo.InvariantCulture)
            + " source-position=" + FormatVector(before)
            + " requested=" + FormatVector(requested)
            + " verified=" + FormatVector(verified)
            + " location-template=" + MarkerValue(DragonKnightOwnedPlacementContract.LocationTemplateGuid)
            + " npc-template=" + MarkerValue(npcTemplateGuid)
            + " native-spawner=1 native-candidate=1 direct-spawn=0 moved-with=Location.MoveAndRotateTo teleport=1"
            + " markedNotSaved=" + BoolValue(location.MarkedNotSaved)
            + " isNotSaved=" + BoolValue(location.IsNotSaved)
            + " save-owned=1 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save-write=0");
    }

    internal void Tick()
    {
        if (_disposed || LiveArmed)
        {
            return;
        }

        RestoreInjectedSlots("gate-closed");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        RestoreInjectedSlots("plugin-shutdown");
        _slots.Clear();
        _placedLocationIds.Clear();
        _lastLogByKey.Clear();
    }

    private void TryInstallSlot(NativeSpawnerSlot slot, string reason)
    {
        if (!LiveArmed)
        {
            LogState("install:" + slot.Anchor.AnchorId, "gate-closed",
                "DRAGON_KNIGHT_NATIVE_SPAWNER_INSTALL_BLOCKED"
                + " anchor=" + slot.Anchor.AnchorId
                + " reason=gate-closed"
                + " enabled=" + BoolValue(_enabled.Value)
                + " kill-switch=" + BoolValue(_killSwitch.Value)
                + " action=none");
            return;
        }

        if (!DragonKnightDk4ActorObserver.TryResolveTemplate(out LocationTemplate? dragonTemplate, out string providerReason) || dragonTemplate == null)
        {
            LogState("install:" + slot.Anchor.AnchorId, providerReason,
                "DRAGON_KNIGHT_NATIVE_SPAWNER_INSTALL_WAITING"
                + " anchor=" + slot.Anchor.AnchorId
                + " reason=" + MarkerValue(providerReason)
                + " action=wait-for-exact-template");
            return;
        }

        if (!DragonKnightDk4ActorObserver.TryPrepareTemplateForSpawn(dragonTemplate, out providerReason))
        {
            LogState("install:" + slot.Anchor.AnchorId, providerReason,
                "DRAGON_KNIGHT_NATIVE_SPAWNER_INSTALL_BLOCKED"
                + " anchor=" + slot.Anchor.AnchorId
                + " reason=" + MarkerValue(providerReason)
                + " action=none");
            return;
        }

        if (!TryReadSpawnerTemplates(slot.Spawner, out LocationTemplate[] currentTemplates, out string readReason))
        {
            LogState("install:" + slot.Anchor.AnchorId, readReason,
                "DRAGON_KNIGHT_NATIVE_SPAWNER_INSTALL_BLOCKED"
                + " anchor=" + slot.Anchor.AnchorId
                + " reason=" + readReason
                + " action=none");
            return;
        }

        bool exactSource = currentTemplates.Any(template => IsTemplate(template, slot.Anchor.SourceTemplateName, slot.Anchor.SourceTemplateGuid));
        bool exactDragon = currentTemplates.Any(IsDragonKnightTemplate);
        if (!exactSource && !exactDragon)
        {
            LogState("install:" + slot.Anchor.AnchorId, "source-template-missing",
                "DRAGON_KNIGHT_NATIVE_SPAWNER_INSTALL_BLOCKED"
                + " anchor=" + slot.Anchor.AnchorId
                + " expected-source=" + MarkerValue(slot.Anchor.SourceTemplateName + "[" + slot.Anchor.SourceTemplateGuid + "]")
                + " actual-sources=" + MarkerValue(TemplateSummary(currentTemplates))
                + " reason=source-template-missing"
                + " action=none");
            return;
        }

        FieldInfo? field = LocationsToSpawnField;
        if (field == null || field.FieldType != typeof(LocationTemplate[]))
        {
            LogState("install:" + slot.Anchor.AnchorId, "field-contract-changed",
                "DRAGON_KNIGHT_NATIVE_SPAWNER_INSTALL_BLOCKED"
                + " anchor=" + slot.Anchor.AnchorId
                + " field=_locationsToSpawn"
                + " reason=field-contract-changed"
                + " action=none");
            return;
        }

        LocationTemplate[] updated = { dragonTemplate };
        field.SetValue(slot.Spawner, updated);
        if (!TryReadSpawnerTemplates(slot.Spawner, out LocationTemplate[] installedTemplates, out _) ||
            installedTemplates.Length != 1 ||
            !installedTemplates.Any(IsDragonKnightTemplate))
        {
            field.SetValue(slot.Spawner, slot.OriginalTemplates);
            LogState("install:" + slot.Anchor.AnchorId, "readback-failed",
                "DRAGON_KNIGHT_NATIVE_SPAWNER_INSTALL_BLOCKED"
                + " anchor=" + slot.Anchor.AnchorId
                + " reason=readback-failed"
                + " action=restored-original-template-array");
            return;
        }

        slot.Installed = true;
        slot.ReplacedByDragonKnight = true;
        slot.DragonKnightTemplate = dragonTemplate;
        LogState("install:" + slot.Anchor.AnchorId, "installed",
            "DRAGON_KNIGHT_NATIVE_SPAWNER_CANDIDATE_INSTALLED"
            + " route=" + DragonKnightNativeSpawnerPlacementContract.RouteId
            + " anchor=" + slot.Anchor.AnchorId
            + " source=" + MarkerValue(slot.Anchor.SourceTemplateName + "[" + slot.Anchor.SourceTemplateGuid + "]")
            + " added=" + MarkerValue(DragonKnightOwnedPlacementContract.LocationTemplateName + "[" + DragonKnightOwnedPlacementContract.LocationTemplateGuid + "]")
            + " candidate-count=" + installedTemplates.Length.ToString(CultureInfo.InvariantCulture)
            + " trigger=" + reason
            + " candidate-policy=" + DragonKnightNativeSpawnerPlacementContract.CandidatePolicy
            + " spawnOwner=LocationSpawner cooldownOwner=BaseLocationSpawner killedIdOwner=BaseLocationSpawner saveRestoreOwner=BaseLocationSpawner"
            + " direct-spawn=0 moved-to-boss-root=on-spawn action=native-boss-candidate-placement");
    }

    private void RestoreInjectedSlots(string reason)
    {
        FieldInfo? field = LocationsToSpawnField;
        if (field == null || field.FieldType != typeof(LocationTemplate[]))
        {
            return;
        }

        foreach (NativeSpawnerSlot slot in _slots.Values)
        {
            if (!slot.ReplacedByDragonKnight || slot.Spawner.HasBeenDiscarded)
            {
                continue;
            }

            try
            {
                field.SetValue(slot.Spawner, slot.OriginalTemplates);
                slot.Installed = false;
                slot.ReplacedByDragonKnight = false;
                slot.DragonKnightTemplate = null;
                LogState("restore:" + slot.Anchor.AnchorId, reason,
                    "DRAGON_KNIGHT_NATIVE_SPAWNER_RESTORED"
                    + " anchor=" + slot.Anchor.AnchorId
                    + " reason=" + reason
                    + " active-native-locations-unchanged=1"
                    + " action=restore-original-candidate-array");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "DRAGON_KNIGHT_NATIVE_SPAWNER_RESTORE_FAILED"
                    + " anchor=" + slot.Anchor.AnchorId
                    + " reason=" + reason
                    + " error=" + MarkerValue(ex.GetType().Name + ":" + ex.Message));
            }
        }
    }

    private static bool TryReadSpawnerTemplates(LocationSpawner spawner, out LocationTemplate[] templates, out string reason)
    {
        templates = Array.Empty<LocationTemplate>();
        FieldInfo? field = LocationsToSpawnField;
        if (field == null || field.FieldType != typeof(LocationTemplate[]))
        {
            reason = "locations-to-spawn-field-contract-changed";
            return false;
        }

        try
        {
            templates = field.GetValue(spawner) as LocationTemplate[] ?? Array.Empty<LocationTemplate>();
            reason = templates.Length == 0 ? "locations-to-spawn-empty" : string.Empty;
            return templates.Length > 0;
        }
        catch (Exception ex)
        {
            reason = "locations-to-spawn-read-failed:" + ex.GetType().Name;
            return false;
        }
    }

    private static bool IsDragonKnightTemplate(LocationTemplate? template)
    {
        return IsTemplate(
            template,
            DragonKnightOwnedPlacementContract.LocationTemplateName,
            DragonKnightOwnedPlacementContract.LocationTemplateGuid);
    }

    private static bool IsTemplate(LocationTemplate? template, string name, string guid)
    {
        return template != null &&
               string.Equals(template.name, name, StringComparison.Ordinal) &&
               string.Equals(template.GUID, guid, StringComparison.OrdinalIgnoreCase);
    }

    private void LogState(string key, string signature, string message)
    {
        if (_lastLogByKey.TryGetValue(key, out string? previous) && string.Equals(previous, signature, StringComparison.Ordinal))
        {
            return;
        }

        _lastLogByKey[key] = signature;
        _logger.LogInfo(message);
    }

    private static string SafeSceneName(LocationSpawnerAttachment spec)
    {
        try
        {
            return spec.gameObject.scene.IsValid() ? spec.gameObject.scene.name ?? string.Empty : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string SafeTransformPath(Transform transform)
    {
        try
        {
            Stack<string> names = new();
            Transform? current = transform;
            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }

            return "/" + string.Join("/", names);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string SafeLocationId(Location? location)
    {
        try
        {
            return location?.ID ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string TemplateSummary(IEnumerable<LocationTemplate> templates)
    {
        return string.Join("|", templates.Where(template => template != null).Select(template => template.name + "[" + template.GUID + "]"));
    }

    private static string MarkerValue(string value)
    {
        return DragonKnightDk4Marker.MarkerValue(value);
    }

    private static string FormatVector(Vector3 value)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:0.###}|{1:0.###}|{2:0.###}", value.x, value.y, value.z);
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string BoolValue(bool value)
    {
        return value ? "1" : "0";
    }

    private sealed class NativeSpawnerSlot
    {
        internal NativeSpawnerSlot(LocationSpawner spawner, DragonKnightNativeSpawnerAnchorDefinition anchor, LocationTemplate[] originalTemplates)
        {
            Spawner = spawner;
            Anchor = anchor;
            OriginalTemplates = originalTemplates.ToArray();
        }

        internal LocationSpawner Spawner { get; }
        internal DragonKnightNativeSpawnerAnchorDefinition Anchor { get; }
        internal LocationTemplate[] OriginalTemplates { get; private set; }
        internal bool Installed { get; set; }
        internal bool ReplacedByDragonKnight { get; set; }
        internal LocationTemplate? DragonKnightTemplate { get; set; }

        internal void Refresh(LocationTemplate[] templates)
        {
            OriginalTemplates = templates.ToArray();
            Installed = templates.Any(IsDragonKnightTemplate);
            ReplacedByDragonKnight = Installed;
            DragonKnightTemplate = templates.FirstOrDefault(IsDragonKnightTemplate);
        }
    }
}
