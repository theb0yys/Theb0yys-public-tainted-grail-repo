using BepInEx;
using BepInEx.Configuration;
using DragonKnight.AI.Package.V2;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Spawners;
using HarmonyLib;
using System;
using System.IO;
using UnityEngine;

namespace DragonKnight;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "kane.tgfoa.dragon-knight";
    public const string PluginName = "Dragon Knight";
    public const string PluginVersion = "0.2.5";

    internal const string BundleFileName = "dragonknight_visuals";
    internal const string IronPhaseAssetPath = "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Iron_Weapon.prefab";
    internal const string FirePhaseAssetPath = "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Fire_Weapon.prefab";
    private const bool VisualProofHotkeysDisabledForArmorItemBuild = true;

    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<bool> _visualProofEnabled = null!;
    private ConfigEntry<KeyCode> _spawnIronHotkey = null!;
    private ConfigEntry<KeyCode> _switchToFireHotkey = null!;
    private ConfigEntry<KeyCode> _discardHotkey = null!;
    private ConfigEntry<float> _spawnDistanceMeters = null!;
    private ConfigEntry<float> _visualScale = null!;
    private ConfigEntry<bool> _bossAiEnabled = null!;
    private ConfigEntry<bool> _bossAiKillSwitch = null!;
    private ConfigEntry<bool> _bossAiValidatePackageOnLoad = null!;

    private AssetBundle? _visualBundle;
    private GameObject? _ironPhasePrefab;
    private GameObject? _firePhasePrefab;
    private GameObject? _activeVisual;
    private string _activePhase = string.Empty;
    private bool _missingBundleLogged;
    private DragonKnightOwnedPlacementObserver? _ownedPlacementObserver;
    private DragonKnightNativeSpawnerPlacementSystem? _nativeSpawnerPlacementSystem;
    private DragonKnightDk4DiagnosticHost? _dk4DiagnosticHost;
    private Harmony? _harmony;

    internal static Plugin? Instance { get; private set; }
    internal BepInEx.Logging.ManualLogSource ModLogger => Logger;

    private void Awake()
    {
        Instance = this;

        _enabled = Config.Bind("General", "Enabled", true, "Enable the Dragon Knight standalone loader.");
        _visualProofEnabled = Config.Bind(
            "VisualProof",
            "Enabled",
            false,
            "Enable the DK2 visual-only proof. This is disabled in the armor item build and does not create or equip armor.");
        _spawnIronHotkey = Config.Bind(
            "VisualProof",
            "SpawnIronHotkey",
            KeyCode.F8,
            "Spawn or replace the visual proof with the Iron phase visual.");
        _switchToFireHotkey = Config.Bind(
            "VisualProof",
            "SwitchToFireHotkey",
            KeyCode.F10,
            "Switch the active visual proof to the Fire phase visual.");
        _discardHotkey = Config.Bind(
            "VisualProof",
            "DiscardHotkey",
            KeyCode.F9,
            "Discard the active visual proof.");
        _spawnDistanceMeters = Config.Bind(
            "VisualProof",
            "SpawnDistanceMeters",
            4f,
            new ConfigDescription(
                "Distance in front of the active camera for visual-only proof placement.",
                new AcceptableValueRange<float>(1f, 20f)));
        _visualScale = Config.Bind(
            "VisualProof",
            "VisualScale",
            1f,
            new ConfigDescription(
                "Scale multiplier for the visual-only proof.",
                new AcceptableValueRange<float>(0.25f, 5f)));
        _bossAiEnabled = Config.Bind(
            "DragonKnightBossAI",
            "Enabled",
            false,
            "Wire the Dragon Knight Boss AI package boundary. This remains blocked from live runtime registration until DK4 actor/target/host ownership proof passes.");
        _bossAiKillSwitch = Config.Bind(
            "DragonKnightBossAI",
            "KillSwitch",
            true,
            "Fail-closed Dragon Knight Boss AI package kill switch. Keep true until the later live actor/host proof explicitly clears it.");
        _bossAiValidatePackageOnLoad = Config.Bind(
            "DragonKnightBossAI",
            "ValidatePackageOnLoad",
            true,
            "Validate and log the AIR-49 Dragon Knight Boss AI package contract when the standalone loader starts. This does not register live AI or dispatch actions.");

        _ownedPlacementObserver = new DragonKnightOwnedPlacementObserver(Logger, Config);
        _nativeSpawnerPlacementSystem = new DragonKnightNativeSpawnerPlacementSystem(Logger, Config);
        _dk4DiagnosticHost = new DragonKnightDk4DiagnosticHost(Logger, Config, GetIronPhasePrefab);
        _harmony = new Harmony(PluginGuid);
        DragonKnightNativeSpawnerPlacementPatchPlan.Apply(_harmony, Logger);

        if (_enabled.Value && _visualProofEnabled.Value && !VisualProofHotkeysDisabledForArmorItemBuild)
        {
            TryLoadVisualBundle();
        }

        if (_bossAiValidatePackageOnLoad.Value)
        {
            ValidateBossAiPackageBoundary();
        }

        Logger.LogInfo(
            $"{PluginName} {PluginVersion} loaded. " +
            $"Enabled={_enabled.Value}. " +
            $"VisualProofEnabled={_visualProofEnabled.Value}. " +
            $"VisualProofHotkeysDisabled={VisualProofHotkeysDisabledForArmorItemBuild}. " +
            "ArmorItemRuntimeBlockedByResearch=true. " +
            $"BossAIEnabled={_bossAiEnabled.Value}. " +
            $"BossAIKillSwitch={_bossAiKillSwitch.Value}. " +
            "Phase=A2K armor item runtime blocked plus AIR-49 package boundary validation plus Dragon Knight-owned HOS placement observation plus DK5D native LocationSpawner candidate placement; " +
            "DK4 temporary live actor diagnostic remains default-off and is not the production placement route. " +
            "Live AI registration, movement actions, attacks, follower mechanics, saves, and roaming are not active.");
    }

    internal static void RegisterDragonKnightNativeSpawner(LocationSpawner spawner, LocationSpawnerAttachment spec, bool isRestored)
    {
        Instance?._nativeSpawnerPlacementSystem?.RegisterNativeSpawner(spawner, spec, isRestored);
    }

    internal static void ObserveDragonKnightNativeSpawn(BaseLocationSpawner owner, Location location, int id)
    {
        Instance?._nativeSpawnerPlacementSystem?.ObserveNativeSpawn(owner, location, id);
    }

    private GameObject? GetIronPhasePrefab()
    {
        return TryLoadVisualBundle()
            ? _ironPhasePrefab
            : null;
    }

    private void Update()
    {
        _ownedPlacementObserver?.Tick();
        _nativeSpawnerPlacementSystem?.Tick();
        _dk4DiagnosticHost?.Tick();
        if (!_enabled.Value || !_visualProofEnabled.Value || VisualProofHotkeysDisabledForArmorItemBuild)
        {
            return;
        }

        if (_activeVisual != null && Input.GetKeyDown(_discardHotkey.Value))
        {
            DiscardActiveVisual("discard hotkey " + _discardHotkey.Value);
            return;
        }

        if (Input.GetKeyDown(_spawnIronHotkey.Value))
        {
            SpawnVisualPhase("Iron", IronPhaseAssetPath);
            return;
        }

        if (Input.GetKeyDown(_switchToFireHotkey.Value))
        {
            SpawnVisualPhase("Fire", FirePhaseAssetPath);
        }
    }

    private void SpawnVisualPhase(string phase, string assetPath)
    {
        if (!TryLoadVisualBundle())
        {
            return;
        }

        GameObject? prefab = string.Equals(phase, "Fire", StringComparison.Ordinal)
            ? _firePhasePrefab
            : _ironPhasePrefab;
        if (prefab == null)
        {
            Logger.LogWarning($"DRAGON_KNIGHT_VISUAL_SPAWN_BLOCKED phase={phase}, reason=prefab missing from loaded bundle, asset={assetPath}");
            return;
        }

        float spawnDistance = Mathf.Max(1f, _spawnDistanceMeters.Value);
        if (!TryGetCameraPlacement(spawnDistance, out Vector3 position, out Quaternion rotation))
        {
            Logger.LogWarning($"DRAGON_KNIGHT_VISUAL_SPAWN_BLOCKED phase={phase}, reason=no active camera placement route.");
            return;
        }

        DiscardActiveVisual("replace with " + phase, logIfMissing: false);

        GameObject visual = Instantiate(prefab, position, rotation);
        visual.name = "DragonKnight_VisualProof_" + phase;
        visual.transform.localScale = Vector3.one * _visualScale.Value;

        int disabledColliders = DisableColliders(visual);
        int renderers = visual.GetComponentsInChildren<Renderer>(true).Length;
        _activeVisual = visual;
        _activePhase = phase;

        Logger.LogWarning(
            $"DRAGON_KNIGHT_VISUAL_SPAWNED phase={phase}, asset={assetPath}, " +
            $"position={FormatVector(position)}, scale={_visualScale.Value:0.###}, renderers={renderers}, " +
            $"collidersDisabled={disabledColliders}, actor=false, ai=false, attacks=false, follower=false, items=false, save=false, roaming=false.");
    }

    private bool TryLoadVisualBundle()
    {
        if (_visualBundle != null)
        {
            return true;
        }

        string pluginDirectory = Path.GetDirectoryName(Info.Location) ?? string.Empty;
        string bundlePath = Path.Combine(pluginDirectory, BundleFileName);
        if (!File.Exists(bundlePath))
        {
            if (!_missingBundleLogged)
            {
                Logger.LogWarning("Dragon Knight visual bundle is missing; visual proof is blocked until this file exists: " + bundlePath);
                _missingBundleLogged = true;
            }

            return false;
        }

        AssetBundle? bundle = AssetBundle.LoadFromFile(bundlePath);
        if (bundle == null)
        {
            Logger.LogWarning("Dragon Knight visual bundle failed to load: " + bundlePath);
            return false;
        }

        GameObject? iron = bundle.LoadAsset<GameObject>(IronPhaseAssetPath);
        GameObject? fire = bundle.LoadAsset<GameObject>(FirePhaseAssetPath);
        if (iron == null || fire == null)
        {
            bundle.Unload(false);
            Logger.LogWarning(
                "Dragon Knight visual bundle failed validation: " +
                $"ironPrefab={iron != null}, firePrefab={fire != null}. " +
                "No visual proof will spawn.");
            return false;
        }

        _visualBundle = bundle;
        _ironPhasePrefab = iron;
        _firePhasePrefab = fire;
        _missingBundleLogged = false;

        Logger.LogInfo(
            $"Dragon Knight visual bundle loaded: {bundlePath}. " +
            "Approved assets=Iron_Weapon,Fire_Weapon; phase=visual-only; actor=false; ai=false; items=false; save=false.");
        return true;
    }

    private static bool TryGetCameraPlacement(float spawnDistance, out Vector3 position, out Quaternion rotation)
    {
        Camera? camera = Camera.main;
        if (camera == null)
        {
            Camera[] cameras = Camera.allCameras;
            if (cameras.Length > 0)
            {
                camera = cameras[0];
            }
        }

        if (camera == null)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        Vector3 forward = camera.transform.forward;
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }

        position = camera.transform.position + forward.normalized * spawnDistance;
        Vector3 faceDirection = camera.transform.position - position;
        faceDirection.y = 0f;
        if (faceDirection.sqrMagnitude < 0.001f)
        {
            faceDirection = -forward;
            faceDirection.y = 0f;
        }

        rotation = faceDirection.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(faceDirection.normalized, Vector3.up)
            : Quaternion.identity;
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

    private void DiscardActiveVisual(string reason, bool logIfMissing = true)
    {
        GameObject? visual = _activeVisual;
        if (visual == null)
        {
            if (logIfMissing)
            {
                Logger.LogInfo("DRAGON_KNIGHT_VISUAL_DISCARD_SKIPPED reason=no active visual.");
            }

            return;
        }

        string phase = _activePhase;
        _activeVisual = null;
        _activePhase = string.Empty;
        Destroy(visual);
        Logger.LogWarning(
            $"DRAGON_KNIGHT_VISUAL_DISCARDED phase={phase}, reason={reason}, actor=false, ai=false, items=false, save=false, roaming=false.");
    }

    private void OnDestroy()
    {
        _dk4DiagnosticHost?.Dispose();
        _dk4DiagnosticHost = null;
        _nativeSpawnerPlacementSystem?.Dispose();
        _nativeSpawnerPlacementSystem = null;
        _ownedPlacementObserver = null;
        DiscardActiveVisual("plugin shutdown", logIfMissing: false);
        _visualBundle?.Unload(false);
        _visualBundle = null;
        _harmony?.UnpatchSelf();
        _harmony = null;
        Instance = null;
    }

    private void ValidateBossAiPackageBoundary()
    {
        try
        {
            var package = new DragonKnightAiPackageV2();
            var manifest = package.Manifest;

            Require(DragonKnightAiV2Contract.PackageIdValue == "dragon-knight.boss.ai.v2", "package id changed");
            Require(DragonKnightAiV2Contract.AssemblyNameValue == "DragonKnight.AI.Package.V2", "assembly name changed");
            Require(DragonKnightAiV2Contract.BossActorRoleIdValue == "dragon-knight.boss", "boss role changed");
            Require(DragonKnightAiV2Contract.BossName == "Sir Vaelor, the Ashen Dragon Knight", "boss name changed");
            Require(DragonKnightAiV2Contract.NpcTemplateGuid == "00f608ee051b57748a6a9ed8dae28678", "NPC template GUID changed");
            Require(DragonKnightAiV2Contract.LocationTemplateGuid == "d7b09116519f7564593be62781bee3db", "LocationTemplate GUID changed");
            Require(DragonKnightAiV2Contract.OuterWakeRadiusMeters == 25, "outer wake radius changed");
            Require(DragonKnightAiV2Contract.InnerFightStartRadiusMeters == 5, "inner fight radius changed");
            Require(DragonKnightAiV2Contract.SoftLeashRadiusMeters == 22, "soft leash radius changed");
            Require(DragonKnightAiV2Contract.HardLeashRadiusMeters == 25, "hard leash radius changed");
            Require(DragonKnightAiV2Contract.DefaultOff, "package default-off flag changed");
            Require(DragonKnightAiV2Contract.KillSwitchDefault, "package kill-switch default changed");
            Require(!DragonKnightAiV2Contract.DirectNativeCalls, "package direct native call flag changed");
            Require(!DragonKnightAiV2Contract.RabbitBypass, "package Rabbit bypass flag changed");
            Require(!DragonKnightAiV2Contract.GoapBypass, "package GOAP bypass flag changed");
            Require(!DragonKnightAiV2Contract.BlazeBypass, "package Blaze bypass flag changed");
            Require(!DragonKnightAiV2Contract.SaveWrites, "package save-write flag changed");

            Require(manifest.Id == DragonKnightAiV2Contract.PackageId, "manifest package id changed");
            Require(manifest.GoalIds.Count == 8, "manifest goal count changed");
            Require(manifest.ActionIds.Count == 8, "manifest action count changed");
            Require(manifest.RequiredCapabilities.Count == 8, "manifest capability count changed");
            Require(manifest.BlackboardNamespace == DragonKnightAiV2Contract.BlackboardNamespace, "manifest blackboard namespace changed");
            Require(manifest.PersistentKeys.Count == 0, "manifest requested persistent keys");
            Require(manifest.ProcedureRequirements.Count == 1, "manifest procedure requirement count changed");
            Require(package.GoalDefinitions.Count == manifest.GoalIds.Count, "goal definitions do not match manifest");
            Require(package.ActionDefinitions.Count == manifest.ActionIds.Count, "action definitions do not match manifest");

            var defaultOffDecision = DragonKnightAiV2Contract.ValidateDecision(new DragonKnightBossDecisionInput
            {
                PackageEnabled = false,
                KillSwitchActive = false,
            });
            Require(!defaultOffDecision.Accepted
                && defaultOffDecision.Reason == "dragon-knight.boss.default-off-disabled",
                "default-off decision did not fail closed");

            var killSwitchDecision = DragonKnightAiV2Contract.ValidateDecision(new DragonKnightBossDecisionInput
            {
                PackageEnabled = true,
                KillSwitchActive = true,
            });
            Require(!killSwitchDecision.Accepted
                && killSwitchDecision.Reason == "dragon-knight.boss.kill-switch-active",
                "kill-switch decision did not fail closed");

            var forbiddenTriggerDecision = DragonKnightAiV2Contract.ValidateTarget(new DragonKnightBossDecisionInput
            {
                TargetKind = DragonKnightAiV2Contract.DiscoveryTargetKind,
                TargetId = DragonKnightAiV2Contract.ForbiddenDiscoveryTrigger,
            });
            Require(!forbiddenTriggerDecision.Accepted
                && forbiddenTriggerDecision.Reason == "dragon-knight.boss.target-forbidden-trigger",
                "forbidden Cromlech trigger did not fail closed");

            Logger.LogInfo(
                DragonKnightAiV2Contract.PackageMarkerLine +
                $" loader=DragonKnight goals={manifest.GoalIds.Count} actions={manifest.ActionIds.Count} capabilities={manifest.RequiredCapabilities.Count} " +
                "live-runtime-registration=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save=0");

            if (_bossAiEnabled.Value)
            {
                Logger.LogWarning(
                    "DRAGON_KNIGHT_BOSS_AI_RUNTIME_BLOCKED " +
                    $"package={DragonKnightAiV2Contract.PackageIdValue} enabled={_bossAiEnabled.Value} kill-switch={_bossAiKillSwitch.Value} " +
                    "reason=dk4-live-actor-host-proof-missing actor-location-id=blocked target-location-id=blocked host-ownership=blocked " +
                    "live-runtime-registration=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save=0");
            }
            else
            {
                Logger.LogInfo(
                    "DRAGON_KNIGHT_BOSS_AI_PACKAGE_WIRED " +
                    $"package={DragonKnightAiV2Contract.PackageIdValue} assembly={DragonKnightAiV2Contract.AssemblyNameValue} " +
                    $"enabled={_bossAiEnabled.Value} kill-switch={_bossAiKillSwitch.Value} default-off=1 " +
                    "live-runtime-registration=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save=0");
            }
        }
        catch (Exception exception)
        {
            Logger.LogError(
                "DRAGON_KNIGHT_BOSS_AI_PACKAGE_WIRE_BLOCKED " +
                $"reason=contract-validation-failed error={exception.GetType().Name}:{exception.Message} " +
                "live-runtime-registration=0 movement=0 attacks=0 phase-combat=0 companion-protect=0 items=0 save=0");
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static string FormatVector(Vector3 value)
    {
        return $"X {value.x:0.0}, Y {value.y:0.0}, Z {value.z:0.0}";
    }
}
