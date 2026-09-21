using System;
using System.Collections.Generic;
using Awaken.Kandra;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Setup;
using Awaken.TG.Main.Locations.Spawners;
using Awaken.TG.MVC;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DragonKnight;

internal sealed class DragonKnightDk4DiagnosticHost : IDisposable
{
    private readonly ManualLogSource _logger;
    private readonly Func<GameObject?> _ironOverlayPrefabProvider;
    private readonly ConfigEntry<bool> _enabled;
    private readonly ConfigEntry<bool> _killSwitch;
    private readonly ConfigEntry<KeyCode> _activationHotkey;
    private readonly ConfigEntry<KeyCode> _releaseHotkey;
    private readonly ConfigEntry<float> _maximumObservationSeconds;
    private readonly ConfigEntry<bool> _allowNativeSpawn;

    private ActiveDiagnostic? _active;
    private bool _disposed;

    internal DragonKnightDk4DiagnosticHost(
        ManualLogSource logger,
        ConfigFile config,
        Func<GameObject?> ironOverlayPrefabProvider)
    {
        _logger = logger;
        _ironOverlayPrefabProvider = ironOverlayPrefabProvider;
        _enabled = config.Bind(
            DragonKnightDk4DiagnosticOptions.ConfigSection,
            "Enabled",
            DragonKnightDk4DiagnosticOptions.DefaultEnabled,
            "Enable the DK4 live actor observation diagnostic. Default false; never starts automatically.");
        _killSwitch = config.Bind(
            DragonKnightDk4DiagnosticOptions.ConfigSection,
            "KillSwitch",
            DragonKnightDk4DiagnosticOptions.DefaultKillSwitch,
            "Fail-closed DK4 diagnostic kill switch. Must be false before F7 can spawn the no-save proof actor.");
        _activationHotkey = config.Bind(
            DragonKnightDk4DiagnosticOptions.ConfigSection,
            "ActivationHotkey",
            DragonKnightDk4DiagnosticOptions.DefaultActivationHotkey,
            "Manual DK4 diagnostic activation. The source gate requires F7.");
        _releaseHotkey = config.Bind(
            DragonKnightDk4DiagnosticOptions.ConfigSection,
            "ReleaseHotkey",
            DragonKnightDk4DiagnosticOptions.DefaultReleaseHotkey,
            "Manual DK4 diagnostic cleanup. The source gate requires F6.");
        _maximumObservationSeconds = config.Bind(
            DragonKnightDk4DiagnosticOptions.ConfigSection,
            "MaximumObservationSeconds",
            DragonKnightDk4DiagnosticOptions.DefaultMaximumObservationSeconds,
            new ConfigDescription(
                "Maximum wait for native Location/NpcElement readiness before fail-closed cleanup. The source gate requires 10 seconds.",
                new AcceptableValueRange<float>(1f, 60f)));
        _allowNativeSpawn = config.Bind(
            DragonKnightDk4DiagnosticOptions.ConfigSection,
            "AllowNativeSpawn",
            DragonKnightDk4DiagnosticOptions.DefaultAllowNativeSpawn,
            "Final DK4 spawn arm. Default false; must be true together with Enabled=true and KillSwitch=false before F7 can call LocationTemplate.SpawnLocation.");

        LogConfiguredState();
    }

    internal void Tick()
    {
        if (_disposed)
        {
            return;
        }

        if (_active != null)
        {
            _active.Tick();
            if (_active.Completed)
            {
                _active = null;
            }
        }

        if (Input.GetKeyDown(_releaseHotkey.Value))
        {
            if (_active == null)
            {
                _logger.LogInfo("DRAGON_KNIGHT_DK4_RELEASE_SKIPPED reason=no-active-lease cleanup=none save=0");
            }
            else
            {
                _active.Release("release hotkey " + _releaseHotkey.Value);
                _active = null;
            }

            return;
        }

        if (!Input.GetKeyDown(_activationHotkey.Value))
        {
            return;
        }

        TryStart();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _active?.Release("plugin shutdown");
        _active = null;
    }

    private void TryStart()
    {
        if (!AllRuntimeGatesOpen(out string reason))
        {
            LogBlocked(reason);
            return;
        }

        if (_active != null)
        {
            LogBlocked("active DK4 diagnostic already exists; press " + _releaseHotkey.Value + " first");
            return;
        }

        Scene scene = SceneManager.GetSceneByName(DragonKnightDk4DiagnosticOptions.SceneName);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            LogBlocked("required scene is not loaded: " + DragonKnightDk4DiagnosticOptions.SceneName);
            return;
        }

        if (!DragonKnightDk4ActorObserver.TryFindTarget(out DragonKnightDk4TargetObservation target, out reason))
        {
            LogBlocked(reason);
            return;
        }

        if (!DragonKnightDk4ActorObserver.TryGetReadyHero(out _, out reason))
        {
            LogBlocked(reason);
            return;
        }

        GameObject? ironOverlayPrefab = _ironOverlayPrefabProvider();
        if (ironOverlayPrefab == null)
        {
            LogBlocked("Dragon Knight Iron render overlay prefab is missing from " + Plugin.BundleFileName);
            return;
        }

        if (ironOverlayPrefab.GetComponentsInChildren<Renderer>(true).Length <= 0 &&
            ironOverlayPrefab.GetComponentsInChildren<KandraRenderer>(true).Length <= 0)
        {
            LogBlocked("Dragon Knight Iron render overlay prefab has no renderers");
            return;
        }

        if (!DragonKnightDk4ActorObserver.TryResolveTemplate(out LocationTemplate? template, out reason) || template == null)
        {
            LogBlocked(reason);
            return;
        }

        if (!DragonKnightDk4ActorObserver.TryPrepareTemplateForSpawn(template, out reason))
        {
            LogBlocked(reason);
            return;
        }

        Location? spawnedLocation = null;
        try
        {
            Vector3 requestedPosition = DragonKnightDk4DiagnosticOptions.ArenaCenter;
            Vector3 verifiedPosition = BaseLocationSpawner.VerifyPosition(requestedPosition, template, allowSnapToGround: false);
            Quaternion rotation = DragonKnightDk4DiagnosticOptions.FacingEdgeRotation();
            spawnedLocation = template.SpawnLocation(
                verifiedPosition,
                rotation,
                null,
                null,
                DragonKnightDk4DiagnosticOptions.BossName,
                scene);
            spawnedLocation.MarkedNotSaved = true;

            if (!spawnedLocation.MarkedNotSaved || !spawnedLocation.IsNotSaved)
            {
                if (!spawnedLocation.HasBeenDiscarded)
                {
                    spawnedLocation.MarkedNotSaved = true;
                    spawnedLocation.Discard();
                }

                LogBlocked("immediate save-exclusion readback failed");
                return;
            }

            _active = new ActiveDiagnostic(
                _logger,
                spawnedLocation,
                target,
                ironOverlayPrefab,
                requestedPosition,
                verifiedPosition,
                _maximumObservationSeconds.Value);
            _logger.LogWarning(
                "DRAGON_KNIGHT_DK4_REQUEST_STARTED"
                + " owner=" + DragonKnightDk4DiagnosticOptions.OwnerId
                + " actor-role=" + DragonKnightDk4DiagnosticOptions.ActorRole
                + " actor-source=" + DragonKnightDk4DiagnosticOptions.ActorSource
                + " target-source=" + target.Source
                + " target-location-id=" + target.LocationId
                + " target-id=" + target.TargetId
                + " scene=" + DragonKnightDk4DiagnosticOptions.SceneName
                + " requested=" + FormatVector(requestedPosition)
                + " verified=" + FormatVector(verifiedPosition)
                + " native-spawn=1 default-off=1 kill-switch=0"
                + " goals=0 actions=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save=0");
        }
        catch (Exception ex)
        {
            try
            {
                if (spawnedLocation != null && !spawnedLocation.HasBeenDiscarded)
                {
                    spawnedLocation.MarkedNotSaved = true;
                    spawnedLocation.Discard();
                }
            }
            catch (Exception cleanupEx)
            {
                _logger.LogWarning("DRAGON_KNIGHT_DK4_FAIL_CLOSED_CLEANUP_FAILED error=" + cleanupEx.GetType().Name + ":" + cleanupEx.Message);
            }

            LogBlocked("native spawn failed: " + ex.GetType().Name + ": " + ex.Message);
        }
    }

    private bool AllRuntimeGatesOpen(out string reason)
    {
        if (!_enabled.Value)
        {
            reason = "DK4Diagnostic.Enabled=false";
            return false;
        }

        if (_killSwitch.Value)
        {
            reason = "DK4Diagnostic.KillSwitch=true";
            return false;
        }

        if (!_allowNativeSpawn.Value)
        {
            reason = "DK4Diagnostic.AllowNativeSpawn=false";
            return false;
        }

        if (_activationHotkey.Value != DragonKnightDk4DiagnosticOptions.DefaultActivationHotkey ||
            _releaseHotkey.Value != DragonKnightDk4DiagnosticOptions.DefaultReleaseHotkey)
        {
            reason = "DK4Diagnostic hotkeys must remain F7/F6";
            return false;
        }

        if (Mathf.Abs(_maximumObservationSeconds.Value - DragonKnightDk4DiagnosticOptions.DefaultMaximumObservationSeconds) > 0.001f)
        {
            reason = "DK4Diagnostic.MaximumObservationSeconds must remain 10";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private void LogConfiguredState()
    {
        _logger.LogInfo(
            "DRAGON_KNIGHT_DK4_DIAGNOSTIC_CONFIG"
            + " enabled=" + _enabled.Value
            + " kill-switch=" + _killSwitch.Value
            + " allow-native-spawn=" + _allowNativeSpawn.Value
            + " activation=" + _activationHotkey.Value
            + " release=" + _releaseHotkey.Value
            + " maximum-observation-seconds=" + _maximumObservationSeconds.Value.ToString("0.###")
            + " actor-source=" + DragonKnightDk4DiagnosticOptions.ActorSource
            + " target-source=" + DragonKnightDk4DiagnosticOptions.TargetSource
            + " target-location-id=" + DragonKnightDk4DiagnosticOptions.TargetLocationId
            + " default-off=1 save=0");
    }

    private void LogBlocked(string reason)
    {
        _logger.LogWarning(
            "DRAGON_KNIGHT_DK4_LIVE_ACTOR_OBSERVATION_BLOCKED"
            + " fixtures=36"
            + " owner=" + DragonKnightDk4DiagnosticOptions.OwnerId
            + " actor-role=" + DragonKnightDk4DiagnosticOptions.ActorRole
            + " actor-source=" + DragonKnightDk4DiagnosticOptions.ActorSource
            + " target-source=" + DragonKnightDk4DiagnosticOptions.TargetSource
            + " target-location-id=" + DragonKnightDk4DiagnosticOptions.TargetLocationId
            + " activation=F7 release=F6"
            + " reason=" + DragonKnightDk4Marker.MarkerValue(reason)
            + " default-off=1 kill-switch=" + (_killSwitch.Value ? "1" : "0")
            + " native-spawn=" + (_allowNativeSpawn.Value ? "1" : "0")
            + " goals=0 actions=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save=0");
    }

    private static string FormatVector(Vector3 value)
    {
        return $"{value.x:0.###}|{value.y:0.###}|{value.z:0.###}";
    }

    private sealed class ActiveDiagnostic
    {
        private readonly ManualLogSource _logger;
        private readonly Location _location;
        private readonly DragonKnightDk4TargetObservation _target;
        private readonly GameObject _ironOverlayPrefab;
        private readonly Vector3 _requestedPosition;
        private readonly Vector3 _verifiedPosition;
        private readonly float _expiresAt;
        private readonly HashSet<Renderer> _overlayRenderers = new();
        private readonly HashSet<KandraRenderer> _overlayKandraRenderers = new();
        private GameObject? _overlayInstance;
        private DragonKnightDk4OwnershipLease? _lease;
        private NpcElement? _npc;
        private bool _waitingLogged;

        internal ActiveDiagnostic(
            ManualLogSource logger,
            Location location,
            DragonKnightDk4TargetObservation target,
            GameObject ironOverlayPrefab,
            Vector3 requestedPosition,
            Vector3 verifiedPosition,
            float timeoutSeconds)
        {
            _logger = logger;
            _location = location;
            _target = target;
            _ironOverlayPrefab = ironOverlayPrefab;
            _requestedPosition = requestedPosition;
            _verifiedPosition = verifiedPosition;
            _expiresAt = Time.time + timeoutSeconds;

            try
            {
                _location.AfterFullyInitialized(OnLocationInitialized);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("DRAGON_KNIGHT_DK4_CALLBACK_BLOCKED reason=" + ex.GetType().Name + ":" + ex.Message + " save=0");
                Release("location callback attach failed");
            }
        }

        internal bool Completed { get; private set; }

        internal bool Ready { get; private set; }

        internal void Tick()
        {
            if (Completed)
            {
                return;
            }

            if (_location.HasBeenDiscarded)
            {
                Completed = true;
                ReleaseRenderOverlay();
                _lease?.Release();
                _logger.LogWarning("DRAGON_KNIGHT_DK4_EXTERNAL_DISCARD_DETECTED cleanup=external save=0");
                return;
            }

            _location.MarkedNotSaved = true;
            if (!_location.MarkedNotSaved || !_location.IsNotSaved)
            {
                Release("save-exclusion invariant failed");
                return;
            }

            if (!Ready)
            {
                LogWaitingOnce();
                if (Time.time >= _expiresAt)
                {
                    Release("Location/NpcElement observation timeout");
                }
            }
            else
            {
                ReassertRenderOverlayVisibility();
            }
        }

        internal void Release(string reason)
        {
            if (Completed)
            {
                return;
            }

            bool markedNotSaved = _location.MarkedNotSaved;
            bool isNotSaved = _location.IsNotSaved;
            try
            {
                ReleaseRenderOverlay();
                if (!_location.HasBeenDiscarded)
                {
                    _location.MarkedNotSaved = true;
                    markedNotSaved = _location.MarkedNotSaved;
                    isNotSaved = _location.IsNotSaved;
                    _location.Discard();
                }
            }
            finally
            {
                _lease?.Release();
                bool locationDiscarded = _location.HasBeenDiscarded;
                bool npcDiscarded = _npc == null || _npc.HasBeenDiscarded;
                Completed = true;
                _logger.LogWarning(DragonKnightDk4Marker.BuildReleaseMarker(
                    reason,
                    _lease,
                    markedNotSaved,
                    isNotSaved,
                    locationDiscarded,
                    npcDiscarded));
            }
        }

        private void OnLocationInitialized()
        {
            if (Completed || Ready)
            {
                return;
            }

            _location.MarkedNotSaved = true;
            if (!_location.TryGetElement(out _npc) || _npc == null)
            {
                Release("missing NpcElement after Location initialization");
                return;
            }

            if (!DragonKnightDk4ActorObserver.TryValidateSpawnedActor(_location, _target, out DragonKnightDk4ActorObservation actor, out string reason))
            {
                Release(reason);
                return;
            }

            if (!DragonKnightDk4OwnershipLease.TryAcquire(actor.LocationId, _target.LocationId, out DragonKnightDk4OwnershipLease? lease, out reason) || lease == null)
            {
                Release(reason);
                return;
            }

            _lease = lease;

            if (!TryAttachRenderOverlay(out DragonKnightDk4VisualOverlayEvidence visual, out reason))
            {
                Release(reason);
                return;
            }

            Ready = true;
            _logger.LogWarning(
                "DRAGON_KNIGHT_DK4_RENDER_OVERLAY_PASS"
                + " actor-location-id=" + actor.LocationId
                + " phase=Iron"
                + " overlay-root=" + DragonKnightDk4DiagnosticOptions.IronOverlayRootName
                + " asset=" + Plugin.IronPhaseAssetPath
                + " renderers=" + visual.RendererCount
                + " colliders-disabled=" + visual.CollidersDisabled
                + " native-unity-renderers-hidden=" + visual.NativeUnityRenderersHidden
                + " native-kandra-renderers-hidden=" + visual.NativeKandraRenderersHidden
                + " actor-components-owned-by-native-bootstrap=1"
                + " source-material-mutation=0"
                + " movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save=0");
            _logger.LogWarning(DragonKnightDk4Marker.BuildPassMarker(actor, _target, lease));
            _logger.LogWarning(
                "DRAGON_KNIGHT_DK4_READY_FOR_FOCUSED_OBSERVATION"
                + " actor-location-id=" + actor.LocationId
                + " actor-id=" + actor.ActorId
                + " target-location-id=" + _target.LocationId
                + " target-id=" + _target.TargetId
                + " lease=" + lease.LeaseId
                + " requested=" + FormatVector(_requestedPosition)
                + " verified=" + FormatVector(_verifiedPosition)
                + " display-name=" + DragonKnightDk4Marker.MarkerValue(actor.DisplayName)
                + " markedNotSaved=1 isNotSaved=1"
                + " movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save=0");
        }

        private bool TryAttachRenderOverlay(
            out DragonKnightDk4VisualOverlayEvidence evidence,
            out string reason)
        {
            evidence = default;
            if (_npc?.Controller == null)
            {
                reason = "missing NpcController before Dragon Knight render overlay attachment";
                return false;
            }

            Transform parent = _npc.Controller.AlivePrefab != null
                ? _npc.Controller.AlivePrefab.transform
                : _npc.Controller.transform;
            GameObject overlay = UnityEngine.Object.Instantiate(_ironOverlayPrefab, parent, false);
            overlay.name = DragonKnightDk4DiagnosticOptions.IronOverlayRootName;
            overlay.transform.localPosition = Vector3.zero;
            overlay.transform.localRotation = Quaternion.identity;
            overlay.transform.localScale = Vector3.one;

            int collidersDisabled = DisableColliders(overlay);
            Renderer[] unityRenderers = overlay.GetComponentsInChildren<Renderer>(true);
            KandraRenderer[] kandraRenderers = overlay.GetComponentsInChildren<KandraRenderer>(true);
            int rendererCount = unityRenderers.Length + kandraRenderers.Length;
            if (rendererCount <= 0)
            {
                UnityEngine.Object.Destroy(overlay);
                reason = "Dragon Knight render overlay instantiated without renderers";
                return false;
            }

            overlay.SetActive(true);
            _overlayInstance = overlay;
            _overlayRenderers.Clear();
            _overlayKandraRenderers.Clear();
            foreach (Renderer renderer in unityRenderers)
            {
                if (renderer != null)
                {
                    _overlayRenderers.Add(renderer);
                }
            }

            foreach (KandraRenderer renderer in kandraRenderers)
            {
                if (renderer != null)
                {
                    _overlayKandraRenderers.Add(renderer);
                }
            }

            int hiddenUnity = 0;
            int hiddenKandra = 0;
            ReassertRenderOverlayVisibility(ref hiddenUnity, ref hiddenKandra);
            evidence = new DragonKnightDk4VisualOverlayEvidence(
                rendererCount,
                collidersDisabled,
                hiddenUnity,
                hiddenKandra);
            reason = string.Empty;
            return true;
        }

        private static int DisableColliders(GameObject visual)
        {
            int disabled = 0;
            foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true))
            {
                if (collider == null)
                {
                    continue;
                }

                if (collider.enabled)
                {
                    disabled++;
                }

                collider.enabled = false;
            }

            return disabled;
        }

        private void ReassertRenderOverlayVisibility()
        {
            int ignoredUnity = 0;
            int ignoredKandra = 0;
            ReassertRenderOverlayVisibility(ref ignoredUnity, ref ignoredKandra);
        }

        private void ReassertRenderOverlayVisibility(ref int hiddenUnity, ref int hiddenKandra)
        {
            if (_overlayInstance == null || _npc?.Controller == null)
            {
                return;
            }

            foreach (Renderer renderer in _npc.Controller.gameObject.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null)
                {
                    continue;
                }

                if (_overlayRenderers.Contains(renderer))
                {
                    renderer.enabled = true;
                    continue;
                }

                if (renderer.enabled)
                {
                    hiddenUnity++;
                }

                renderer.enabled = false;
            }

            foreach (KandraRenderer renderer in _npc.Controller.gameObject.GetComponentsInChildren<KandraRenderer>(true))
            {
                if (renderer == null)
                {
                    continue;
                }

                if (_overlayKandraRenderers.Contains(renderer))
                {
                    renderer.enabled = true;
                    continue;
                }

                if (renderer.enabled)
                {
                    hiddenKandra++;
                }

                renderer.enabled = false;
            }
        }

        private void ReleaseRenderOverlay()
        {
            if (_overlayInstance != null)
            {
                UnityEngine.Object.Destroy(_overlayInstance);
                _overlayInstance = null;
            }

            _overlayRenderers.Clear();
            _overlayKandraRenderers.Clear();
        }

        private void LogWaitingOnce()
        {
            if (_waitingLogged)
            {
                return;
            }

            _waitingLogged = true;
            _logger.LogInfo(
                "DRAGON_KNIGHT_DK4_WAITING_FOR_NATIVE_INIT"
                + " actor-source=" + DragonKnightDk4DiagnosticOptions.ActorSource
                + " target-location-id=" + _target.LocationId
                + " markedNotSaved=" + (_location.MarkedNotSaved ? "1" : "0")
                + " isNotSaved=" + (_location.IsNotSaved ? "1" : "0")
                + " timeout-seconds=" + DragonKnightDk4DiagnosticOptions.DefaultMaximumObservationSeconds
                + " save=0");
        }

        private readonly struct DragonKnightDk4VisualOverlayEvidence
        {
            internal DragonKnightDk4VisualOverlayEvidence(
                int rendererCount,
                int collidersDisabled,
                int nativeUnityRenderersHidden,
                int nativeKandraRenderersHidden)
            {
                RendererCount = rendererCount;
                CollidersDisabled = collidersDisabled;
                NativeUnityRenderersHidden = nativeUnityRenderersHidden;
                NativeKandraRenderersHidden = nativeKandraRenderersHidden;
            }

            internal int RendererCount { get; }

            internal int CollidersDisabled { get; }

            internal int NativeUnityRenderersHidden { get; }

            internal int NativeKandraRenderersHidden { get; }
        }
    }
}
