using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Awaken.TG.Assets;
using Awaken.TG.Main.Heroes;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using TaintedGems.Patches;
using UnityEngine;

namespace TaintedGems;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "kane.tgfoa.tainted-gems";
    private const string PluginName = "Tainted Gems";
    private const string PluginVersion = "0.1.10";
    private const string EmbeddedBundleResourceName = "TaintedGems.Assets.tainted_gems.bundle";
    private const int MaxGemStockTemplatesPerOpen = 128;
    internal const string CustomIconAddressPrefix = "mod://kane.tgfoa.tainted-gems/icon/";
    private const string CustomIconAssetPrefix = "TaintedGemsIcon_";

    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<bool> _preferEmbeddedBundle = null!;
    private ConfigEntry<string> _bundleRelativePath = null!;
    private ConfigEntry<string> _shapeSet1PrefabName = null!;
    private ConfigEntry<string> _shapeSet2PrefabName = null!;
    private ConfigEntry<bool> _autoPlaceOnWorldReady = null!;
    private ConfigEntry<bool> _allowHotkeyPlacement = null!;
    private ConfigEntry<KeyCode> _placementHotkey = null!;
    private ConfigEntry<float> _placementRaycastDistance = null!;
    private ConfigEntry<float> _autoPlacementForwardDistance = null!;
    private ConfigEntry<float> _autoPlacementRetrySeconds = null!;
    private ConfigEntry<float> _spawnSpacing = null!;
    private ConfigEntry<float> _spawnHeightOffset = null!;
    private ConfigEntry<float> _spawnScaleMultiplier = null!;
    private ConfigEntry<float> _groundRaycastStartHeight = null!;
    private ConfigEntry<float> _groundRaycastDistance = null!;
    private ConfigEntry<float> _groundLift = null!;
    private ConfigEntry<bool> _shopStockEnabled = null!;
    private ConfigEntry<string> _shopStockTargetShopGuid = null!;
    private ConfigEntry<int> _shopStockQuantity = null!;
    private ConfigEntry<int> _shopStockTemplatesPerOpen = null!;
    private ConfigEntry<bool> _customTemplateEnabled = null!;
    private ConfigEntry<bool> _customTemplateAllowNativeFallback = null!;

    private readonly List<GameObject> _spawnedObjects = new();
    private readonly Dictionary<string, Sprite> _customIconSprites = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<Sprite> _ownedRuntimeIconSprites = new();
    private readonly HashSet<string> _missingIconWarnings = new(StringComparer.OrdinalIgnoreCase);
    private AssetBundle? _bundle;
    private GameObject? _shapeSet1Prefab;
    private GameObject? _shapeSet2Prefab;
    private float _nextCameraWaitLogTime;
    private float _nextAutoPlacementAttemptTime;
    private Harmony? _harmony;
    private bool _useAllGemStockDefaultOverride;
    private string _bundleLoadSource = "none";

    internal static Plugin? Instance { get; private set; }

    internal ManualLogSource ModLogger => Logger;

    internal bool ShopStockEnabled => _shopStockEnabled.Value;

    internal string ShopStockTargetShopGuid => _shopStockTargetShopGuid.Value ?? string.Empty;

    internal int ShopStockQuantity => Mathf.Clamp(_shopStockQuantity.Value, 1, 99);

    internal int ShopStockTemplatesPerOpen => _useAllGemStockDefaultOverride
        ? MaxGemStockTemplatesPerOpen
        : Mathf.Clamp(_shopStockTemplatesPerOpen.Value, 0, MaxGemStockTemplatesPerOpen);

    internal bool CustomTemplateEnabled => _customTemplateEnabled.Value;

    internal bool CustomTemplateAllowNativeFallback => _customTemplateAllowNativeFallback.Value;

    private bool HotkeyPlacementEnabled => _allowHotkeyPlacement.Value && _placementHotkey.Value != KeyCode.None;

    private void Awake()
    {
        Instance = this;

        _enabled = Config.Bind("General", "Enabled", true, "Enable the Tainted Gems asset bundle loader, automatic generic valuable gem world placement, and shop stock route.");
        _preferEmbeddedBundle = Config.Bind("Bundle", "PreferEmbedded", true, "Load the embedded release AssetBundle before checking Bundle.RelativePath.");
        _bundleRelativePath = Config.Bind("Bundle", "RelativePath", "assets/tainted_gems.bundle", "Path to the cooked AssetBundle relative to this plugin DLL.");
        _shapeSet1PrefabName = Config.Bind("Bundle", "ShapeSet1PrefabName", "TaintedGems_DiamondShapeSet1", "First generic valuable gem display prefab asset name inside the AssetBundle.");
        _shapeSet2PrefabName = Config.Bind("Bundle", "ShapeSet2PrefabName", "TaintedGems_DiamondShapeSet2", "Second generic valuable gem display prefab asset name inside the AssetBundle.");
        _autoPlaceOnWorldReady = Config.Bind("WorldPlacement", "AutoPlaceOnWorldReady", true, "Automatically place the generic valuable gem display prefabs near the hero once a playable world ground position is available.");
        _allowHotkeyPlacement = Config.Bind("WorldPlacement", "AllowHotkeyPlacement", false, "Allow an optional debug hotkey to instantiate or despawn the generic valuable gem display prefabs. Release placement is automatic by default.");
        _placementHotkey = Config.Bind("WorldPlacement", "PlacementHotkey", KeyCode.None, "Optional debug hotkey used to place or despawn the generic valuable gem display prefabs when WorldPlacement.AllowHotkeyPlacement is enabled.");
        _placementRaycastDistance = Config.Bind("WorldPlacement", "PlacementRaycastDistance", 30f, new ConfigDescription("Maximum distance for the screen-center placement raycast.", new AcceptableValueRange<float>(1f, 200f)));
        _autoPlacementForwardDistance = Config.Bind("WorldPlacement", "AutoPlacementForwardDistance", 2.25f, new ConfigDescription("Distance in front of the hero used for automatic world placement.", new AcceptableValueRange<float>(0.25f, 20f)));
        _autoPlacementRetrySeconds = Config.Bind("WorldPlacement", "AutoPlacementRetrySeconds", 3f, new ConfigDescription("Seconds between automatic placement attempts until a valid world ground hit is found.", new AcceptableValueRange<float>(0.25f, 60f)));
        _spawnSpacing = Config.Bind("WorldPlacement", "SpawnSpacing", 0.65f, new ConfigDescription("Horizontal spacing between the two placed gem prefabs.", new AcceptableValueRange<float>(0.1f, 5f)));
        _spawnHeightOffset = Config.Bind("WorldPlacement", "SpawnHeightOffset", 0f, new ConfigDescription("Additional vertical offset applied after renderer bounds are aligned to ground.", new AcceptableValueRange<float>(-3f, 3f)));
        _spawnScaleMultiplier = Config.Bind("WorldPlacement", "SpawnScaleMultiplier", 0.35f, new ConfigDescription("Scale multiplier applied to spawned gem prefabs.", new AcceptableValueRange<float>(0.1f, 50f)));
        _groundRaycastStartHeight = Config.Bind("WorldPlacement", "GroundRaycastStartHeight", 2.5f, new ConfigDescription("Height above each side-offset candidate used to start the downward ground raycast.", new AcceptableValueRange<float>(0.1f, 20f)));
        _groundRaycastDistance = Config.Bind("WorldPlacement", "GroundRaycastDistance", 8f, new ConfigDescription("Maximum downward raycast distance for side-by-side ground fitting.", new AcceptableValueRange<float>(0.5f, 50f)));
        _groundLift = Config.Bind("WorldPlacement", "GroundLift", 0.03f, new ConfigDescription("Small upward clearance after renderer bounds are aligned to ground.", new AcceptableValueRange<float>(0f, 2f)));
        _shopStockEnabled = Config.Bind("ShopStock", "Enabled", true, "Enable the decompressed pre-item-list shop stock route for Tainted Gems.");
        _shopStockTargetShopGuid = Config.Bind("ShopStock", "TargetShopGuid", string.Empty, "Exact shop template GUID to mutate. Blank allows any opened shop.");
        _shopStockQuantity = Config.Bind("ShopStock", "Quantity", 1, new ConfigDescription("Quantity to add for each Tainted Gem stock entry.", new AcceptableValueRange<int>(1, 99)));
        _shopStockTemplatesPerOpen = Config.Bind("ShopStock", "TemplatesPerOpen", MaxGemStockTemplatesPerOpen, new ConfigDescription("Maximum Tainted Gem store templates to add to each matching shop open.", new AcceptableValueRange<int>(0, MaxGemStockTemplatesPerOpen)));
        _customTemplateEnabled = Config.Bind("CustomTemplates", "Enabled", true, "Register Tainted Gem runtime clone templates before shop stock insertion.");
        _customTemplateAllowNativeFallback = Config.Bind("CustomTemplates", "AllowNativeFallback", true, "If a custom clone cannot validate, add the native source gem template instead.");
        _useAllGemStockDefaultOverride = ShouldUseAllGemStockDefaultOverride();

        if (_shopStockEnabled.Value || _customTemplateEnabled.Value)
        {
            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(Plugin).Assembly);
            TaintedGemsIconPatch.Apply(_harmony, Logger);
        }

        Logger.LogInfo($"{PluginName} {PluginVersion} loaded. Enabled={_enabled.Value}; preferEmbeddedBundle={_preferEmbeddedBundle.Value}; embeddedResource={EmbeddedBundleResourceName}; bundleFallback={_bundleRelativePath.Value}; shapeSet1={_shapeSet1PrefabName.Value}; shapeSet2={_shapeSet2PrefabName.Value}; placement=auto-hero-ground-generic-valuable-gem; autoPlacement={_autoPlaceOnWorldReady.Value}; autoForwardDistance={_autoPlacementForwardDistance.Value}; autoRetrySeconds={_autoPlacementRetrySeconds.Value}; hotkeyPlacement={HotkeyPlacementEnabled}; hotkey={_placementHotkey.Value}; shopStock={ShopStockEnabled}; targetShop={ShopStockTargetShopGuid}; quantity={ShopStockQuantity}; templatesPerOpen={ShopStockTemplatesPerOpen}; templatesPerOpenConfigured={_shopStockTemplatesPerOpen.Value}; allGemDefaultOverride={_useAllGemStockDefaultOverride}; customTemplates={CustomTemplateEnabled}; nativeFallback={CustomTemplateAllowNativeFallback}; gameplayPerks=forty-two-entry-paired-variable-table-one-positive-skill-one-negative-perk-per-two-base-gems-with-equipped-weapon-stamina-cost-lane; storeVersions=skill-perk-trinket-generic-valuable-generic-junk.");

        if (_enabled.Value)
        {
            TryLoadBundleAndPrefabs();
        }
    }

    private bool ShouldUseAllGemStockDefaultOverride()
    {
        if (_shopStockTemplatesPerOpen.Value != 2 && _shopStockTemplatesPerOpen.Value != 42)
        {
            return false;
        }

        Logger.LogInfo($"ShopStock detected previous gem batch default; using full store batch without editing config. effectiveTemplatesPerOpen={MaxGemStockTemplatesPerOpen}; configuredTemplatesPerOpen={_shopStockTemplatesPerOpen.Value}.");
        return true;
    }

    private void Update()
    {
        if (!_enabled.Value)
        {
            return;
        }

        PruneDestroyedSpawnedObjects();

        if (_autoPlaceOnWorldReady.Value &&
            _spawnedObjects.Count == 0 &&
            Time.realtimeSinceStartup >= _nextAutoPlacementAttemptTime)
        {
            _nextAutoPlacementAttemptTime = Time.realtimeSinceStartup + Mathf.Max(0.25f, _autoPlacementRetrySeconds.Value);
            TrySpawnGemObjects("auto-world-ready");
        }

        if (HotkeyPlacementEnabled && Input.GetKeyDown(_placementHotkey.Value))
        {
            Logger.LogInfo($"Tainted Gems placement hotkey {_placementHotkey.Value} detected.");
            ToggleGemObjects($"hotkey:{_placementHotkey.Value}");
        }
    }

    private void OnDestroy()
    {
        DespawnGemObjects();
        ReleaseCustomIconSprites();

        if (_bundle != null)
        {
            _bundle.Unload(false);
            _bundle = null;
            Logger.LogInfo($"Unloaded Tainted Gems AssetBundle with unloadAllLoadedObjects=false. source={_bundleLoadSource}.");
        }

        _harmony?.UnpatchSelf();
        _harmony = null;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    internal static bool IsCustomIconReference(ShareableSpriteReference? reference)
    {
        return reference != null && IsCustomIconAddress(reference.AssetGUID);
    }

    internal static bool IsCustomIconAddress(string? iconAddress)
    {
        return !string.IsNullOrWhiteSpace(iconAddress) &&
            iconAddress.StartsWith(CustomIconAddressPrefix, StringComparison.OrdinalIgnoreCase);
    }

    internal bool TryGetCustomIconSprite(string iconAddress, out Sprite? sprite)
    {
        sprite = null;
        if (!IsCustomIconAddress(iconAddress))
        {
            return false;
        }

        if (_customIconSprites.TryGetValue(iconAddress, out Sprite? cachedSprite) && cachedSprite != null)
        {
            sprite = cachedSprite;
            return true;
        }

        if (_bundle == null)
        {
            TryLoadBundleAndPrefabs();
        }

        if (_bundle == null)
        {
            LogMissingIconOnce(iconAddress, "bundle-not-loaded");
            return false;
        }

        string slug = iconAddress.Substring(CustomIconAddressPrefix.Length);
        if (!IsSafeIconSlug(slug))
        {
            LogMissingIconOnce(iconAddress, "unsafe-icon-slug");
            return false;
        }

        string assetName = CustomIconAssetPrefix + slug;
        try
        {
            Sprite? bundleSprite = _bundle.LoadAsset<Sprite>(assetName);
            if (bundleSprite != null)
            {
                _customIconSprites[iconAddress] = bundleSprite;
                sprite = bundleSprite;
                return true;
            }

            Texture2D? texture = _bundle.LoadAsset<Texture2D>(assetName);
            if (texture == null)
            {
                LogMissingIconOnce(iconAddress, $"asset-missing:{assetName}");
                return false;
            }

            Sprite runtimeSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
            runtimeSprite.name = assetName;
            runtimeSprite.hideFlags = HideFlags.HideAndDontSave;
            _customIconSprites[iconAddress] = runtimeSprite;
            _ownedRuntimeIconSprites.Add(runtimeSprite);
            sprite = runtimeSprite;
            return true;
        }
        catch (Exception ex)
        {
            LogMissingIconOnce(iconAddress, $"{ex.GetType().Name}:{ex.Message}");
            return false;
        }
    }

    private void LogMissingIconOnce(string iconAddress, string reason)
    {
        string key = $"{iconAddress}|{reason}";
        if (_missingIconWarnings.Add(key))
        {
            Logger.LogWarning($"Tainted Gems custom icon unavailable; address={iconAddress}; reason={reason}; fallback=native.");
        }
    }

    private static bool IsSafeIconSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return false;
        }

        foreach (char c in slug)
        {
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
            {
                return false;
            }
        }

        return true;
    }

    private void ReleaseCustomIconSprites()
    {
        foreach (Sprite sprite in _ownedRuntimeIconSprites)
        {
            if (sprite != null)
            {
                Destroy(sprite);
            }
        }

        _customIconSprites.Clear();
        _ownedRuntimeIconSprites.Clear();
        _missingIconWarnings.Clear();
    }

    private void TryLoadBundleAndPrefabs()
    {
        if (_bundle != null && _shapeSet1Prefab != null && _shapeSet2Prefab != null)
        {
            return;
        }

        bool loaded = false;
        if (_preferEmbeddedBundle.Value)
        {
            loaded = TryLoadEmbeddedBundle();
        }

        if (!loaded)
        {
            loaded = TryLoadFileBundle();
        }

        if (!loaded && !_preferEmbeddedBundle.Value)
        {
            loaded = TryLoadEmbeddedBundle();
        }

        if (!loaded)
        {
            return;
        }

        _shapeSet1Prefab = LoadPrefab(_shapeSet1PrefabName.Value, "shape-set-1");
        _shapeSet2Prefab = LoadPrefab(_shapeSet2PrefabName.Value, "shape-set-2");

        if (_shapeSet1Prefab != null && _shapeSet2Prefab != null)
        {
            Logger.LogInfo($"Tainted Gems AssetBundle loaded. source={_bundleLoadSource}; shapeSet1={_shapeSet1Prefab.name}; shapeSet2={_shapeSet2Prefab.name}; autoPlacement={_autoPlaceOnWorldReady.Value}; hotkeyPlacement={HotkeyPlacementEnabled}; hotkey={_placementHotkey.Value}.");
        }
    }

    private bool TryLoadEmbeddedBundle()
    {
        if (_bundle != null)
        {
            return true;
        }

        try
        {
            using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(EmbeddedBundleResourceName);
            if (stream == null)
            {
                Logger.LogWarning($"Embedded Tainted Gems AssetBundle resource is missing: {EmbeddedBundleResourceName}.");
                return false;
            }

            if (stream.Length <= 0 || stream.Length > int.MaxValue)
            {
                Logger.LogWarning($"Embedded Tainted Gems AssetBundle resource has unsupported length: {stream.Length}.");
                return false;
            }

            int length = (int)stream.Length;
            byte[] bytes = new byte[length];
            int offset = 0;
            while (offset < length)
            {
                int read = stream.Read(bytes, offset, length - offset);
                if (read <= 0)
                {
                    break;
                }

                offset += read;
            }

            if (offset != length)
            {
                Logger.LogWarning($"Embedded Tainted Gems AssetBundle read was incomplete. expectedBytes={length}; actualBytes={offset}.");
                return false;
            }

            _bundle = AssetBundle.LoadFromMemory(bytes);
            if (_bundle == null)
            {
                Logger.LogWarning($"AssetBundle.LoadFromMemory returned null for embedded resource {EmbeddedBundleResourceName}.");
                return false;
            }

            _bundleLoadSource = $"embedded:{EmbeddedBundleResourceName}:{length}";
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"AssetBundle.LoadFromMemory failed for embedded resource {EmbeddedBundleResourceName}: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private bool TryLoadFileBundle()
    {
        if (_bundle != null)
        {
            return true;
        }

        if (!TryResolveBundlePath(out string bundlePath))
        {
            return false;
        }

        if (!File.Exists(bundlePath))
        {
            Logger.LogWarning($"Tainted Gems AssetBundle is missing: {bundlePath}");
            return false;
        }

        try
        {
            _bundle = AssetBundle.LoadFromFile(bundlePath);
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"AssetBundle.LoadFromFile failed for {bundlePath}: {ex.GetType().Name}: {ex.Message}");
            return false;
        }

        if (_bundle == null)
        {
            Logger.LogWarning($"AssetBundle.LoadFromFile returned null for {bundlePath}.");
            return false;
        }

        _bundleLoadSource = $"file:{bundlePath}";
        return true;
    }

    private GameObject? LoadPrefab(string assetName, string label)
    {
        if (_bundle == null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(assetName))
        {
            Logger.LogWarning($"Configured {label} prefab name is blank.");
            return null;
        }

        try
        {
            GameObject prefab = _bundle.LoadAsset<GameObject>(assetName);
            if (prefab == null)
            {
                Logger.LogWarning($"Could not load {label} prefab '{assetName}' from Tainted Gems AssetBundle.");
                return null;
            }

            Logger.LogInfo($"Loaded {label} prefab '{assetName}' from Tainted Gems AssetBundle.");
            return prefab;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Loading {label} prefab '{assetName}' failed: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    private void ToggleGemObjects(string trigger)
    {
        if (_spawnedObjects.Count > 0)
        {
            DespawnGemObjects();
            return;
        }

        TrySpawnGemObjects(trigger);
    }

    private bool TrySpawnGemObjects(string trigger)
    {
        if (_shapeSet1Prefab == null || _shapeSet2Prefab == null)
        {
            TryLoadBundleAndPrefabs();
        }

        if (_shapeSet1Prefab == null || _shapeSet2Prefab == null)
        {
            Logger.LogWarning($"Cannot spawn generic valuable Tainted Gem display for {trigger} because one or both prefabs are not loaded.");
            return false;
        }

        if (!TryGetNaturalGroundPlacement(trigger, out Vector3 shapeSet1GroundPosition, out Vector3 shapeSet2GroundPosition, out Quaternion rotation, out string placementMode, out string hitSummary))
        {
            return false;
        }

        GameObject shapeSet1Object = SpawnGemObject(_shapeSet1Prefab, shapeSet1GroundPosition, rotation, "shape-set-1");
        GameObject shapeSet2Object = SpawnGemObject(_shapeSet2Prefab, shapeSet2GroundPosition, rotation, "shape-set-2");
        string hotkeyHint = HotkeyPlacementEnabled ? $" Press {_placementHotkey.Value} to despawn." : string.Empty;

        Logger.LogInfo($"Spawned generic valuable Tainted Gem display via {trigger}. placement={placementMode}; hit={hitSummary}; shapeSet1Ground={FormatVector(shapeSet1GroundPosition)}; shapeSet2Ground={FormatVector(shapeSet2GroundPosition)}; shapeSet1Final={FormatVector(shapeSet1Object.transform.position)}; shapeSet2Final={FormatVector(shapeSet2Object.transform.position)}; rotation={FormatVector(rotation.eulerAngles)}; autoForwardDistance={_autoPlacementForwardDistance.Value}; placementRaycastDistance={_placementRaycastDistance.Value}; spacing={_spawnSpacing.Value}; groundLift={_groundLift.Value}; heightOffset={_spawnHeightOffset.Value}; scaleMultiplier={_spawnScaleMultiplier.Value}; hotkeyPlacement={HotkeyPlacementEnabled}.{hotkeyHint}");
        return true;
    }

    private bool TryGetNaturalGroundPlacement(
        string trigger,
        out Vector3 shapeSet1GroundPosition,
        out Vector3 shapeSet2GroundPosition,
        out Quaternion rotation,
        out string placementMode,
        out string hitSummary)
    {
        if (TryGetHeroGroundPlacement(trigger, out shapeSet1GroundPosition, out shapeSet2GroundPosition, out rotation, out hitSummary))
        {
            placementMode = "auto-hero-ground";
            return true;
        }

        if (!TryGetSpawnCamera(out Camera camera))
        {
            LogCameraWait(trigger);
            shapeSet1GroundPosition = default;
            shapeSet2GroundPosition = default;
            rotation = Quaternion.identity;
            placementMode = string.Empty;
            hitSummary = string.Empty;
            return false;
        }

        if (TryGetScreenCenterGroundPlacement(camera, trigger, out shapeSet1GroundPosition, out shapeSet2GroundPosition, out rotation, out hitSummary))
        {
            placementMode = "auto-screen-center-ground-fallback";
            hitSummary = $"camera={camera.name}; {hitSummary}";
            return true;
        }

        placementMode = string.Empty;
        return false;
    }

    private bool TryGetHeroGroundPlacement(
        string trigger,
        out Vector3 shapeSet1GroundPosition,
        out Vector3 shapeSet2GroundPosition,
        out Quaternion rotation,
        out string hitSummary)
    {
        shapeSet1GroundPosition = default;
        shapeSet2GroundPosition = default;
        rotation = Quaternion.identity;
        hitSummary = string.Empty;

        Hero? hero;
        try
        {
            hero = Hero.Current;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Cannot auto-place generic valuable Tainted Gem display for {trigger}; Hero.Current failed: {ex.GetType().Name}: {ex.Message}");
            return false;
        }

        if (hero == null)
        {
            return false;
        }

        Vector3 flatForward = Vector3.ProjectOnPlane(hero.Rotation * Vector3.forward, Vector3.up);
        if (flatForward.sqrMagnitude < 0.001f && TryGetSpawnCamera(out Camera camera))
        {
            flatForward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up);
        }

        if (flatForward.sqrMagnitude < 0.001f)
        {
            Logger.LogWarning($"Cannot auto-place generic valuable Tainted Gem display for {trigger}; hero and camera have no usable horizontal forward vector.");
            return false;
        }

        flatForward.Normalize();
        Vector3 flatRight = Vector3.Cross(Vector3.up, flatForward).normalized;
        Vector3 centerCandidate = hero.Coords + flatForward * _autoPlacementForwardDistance.Value;
        if (!TryProjectToGround(centerCandidate, out Vector3 centerGroundPosition))
        {
            Logger.LogWarning($"Cannot auto-place generic valuable Tainted Gem display for {trigger}; ground raycast missed in front of hero near {FormatVector(centerCandidate)}.");
            return false;
        }

        Vector3 shapeSet1Candidate = centerGroundPosition - flatRight * _spawnSpacing.Value;
        Vector3 shapeSet2Candidate = centerGroundPosition + flatRight * _spawnSpacing.Value;
        if (!TryProjectToGround(shapeSet1Candidate, out shapeSet1GroundPosition))
        {
            Logger.LogWarning($"Cannot auto-place generic valuable Tainted Gem display shape-set-1 for {trigger}; ground raycast missed near {FormatVector(shapeSet1Candidate)}.");
            return false;
        }

        if (!TryProjectToGround(shapeSet2Candidate, out shapeSet2GroundPosition))
        {
            Logger.LogWarning($"Cannot auto-place generic valuable Tainted Gem display shape-set-2 for {trigger}; ground raycast missed near {FormatVector(shapeSet2Candidate)}.");
            return false;
        }

        rotation = Quaternion.LookRotation(flatForward, Vector3.up);
        hitSummary = $"hero={FormatVector(hero.Coords)}; center={FormatVector(centerGroundPosition)}";
        return true;
    }

    private bool TryGetSpawnCamera(out Camera camera)
    {
        camera = Camera.main;
        if (camera != null && camera.isActiveAndEnabled)
        {
            return true;
        }

        foreach (Camera candidate in Camera.allCameras)
        {
            if (candidate != null && candidate.isActiveAndEnabled)
            {
                camera = candidate;
                return true;
            }
        }

        camera = null!;
        return false;
    }

    private void LogCameraWait(string trigger)
    {
        if (Time.realtimeSinceStartup < _nextCameraWaitLogTime)
        {
            return;
        }

        _nextCameraWaitLogTime = Time.realtimeSinceStartup + 10f;
        Logger.LogWarning($"Cannot spawn generic valuable Tainted Gem display for {trigger} because no enabled camera is available yet.");
    }

    private bool TryGetScreenCenterGroundPlacement(Camera camera, string trigger, out Vector3 shapeSet1GroundPosition, out Vector3 shapeSet2GroundPosition, out Quaternion rotation, out string hitSummary)
    {
        shapeSet1GroundPosition = default;
        shapeSet2GroundPosition = default;
        rotation = Quaternion.identity;
        hitSummary = string.Empty;

        Ray placementRay = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(placementRay, out RaycastHit centerHit, _placementRaycastDistance.Value, ~0, QueryTriggerInteraction.Ignore))
        {
            Logger.LogWarning($"Cannot spawn generic valuable Tainted Gem display for {trigger}; screen-center placement raycast missed. camera={camera.name}; maxDistance={_placementRaycastDistance.Value}.");
            return false;
        }

        Vector3 flatForward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up);
        if (flatForward.sqrMagnitude < 0.001f)
        {
            flatForward = Vector3.ProjectOnPlane(camera.transform.parent != null ? camera.transform.parent.forward : camera.transform.forward, Vector3.up);
        }

        if (flatForward.sqrMagnitude < 0.001f)
        {
            Logger.LogWarning($"Cannot spawn generic valuable Tainted Gem display for {trigger} because the active camera has no usable horizontal forward vector.");
            return false;
        }

        flatForward.Normalize();
        Vector3 flatRight = Vector3.Cross(Vector3.up, flatForward).normalized;
        Vector3 center = centerHit.point;
        Vector3 shapeSet1Candidate = center - flatRight * _spawnSpacing.Value;
        Vector3 shapeSet2Candidate = center + flatRight * _spawnSpacing.Value;
        rotation = Quaternion.LookRotation(flatForward, Vector3.up);
        hitSummary = $"{FormatVector(centerHit.point)}:{FormatColliderName(centerHit)}";

        if (!TryProjectToGround(shapeSet1Candidate, out shapeSet1GroundPosition))
        {
            Logger.LogWarning($"Cannot spawn generic valuable Tainted Gem display shape-set-1 for {trigger}; ground raycast missed near {FormatVector(shapeSet1Candidate)}.");
            return false;
        }

        if (!TryProjectToGround(shapeSet2Candidate, out shapeSet2GroundPosition))
        {
            Logger.LogWarning($"Cannot spawn generic valuable Tainted Gem display shape-set-2 for {trigger}; ground raycast missed near {FormatVector(shapeSet2Candidate)}.");
            return false;
        }

        return true;
    }

    private bool TryProjectToGround(Vector3 candidatePosition, out Vector3 groundedPosition)
    {
        Vector3 rayStart = candidatePosition + Vector3.up * _groundRaycastStartHeight.Value;
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, _groundRaycastDistance.Value, ~0, QueryTriggerInteraction.Ignore))
        {
            groundedPosition = hit.point;
            return true;
        }

        groundedPosition = default;
        return false;
    }

    private GameObject SpawnGemObject(GameObject prefab, Vector3 groundPosition, Quaternion rotation, string label)
    {
        GameObject instance = Instantiate(prefab, groundPosition, rotation);
        instance.name = $"TaintedGems_GenericValuable_{label}_{prefab.name}";
        instance.transform.localScale = prefab.transform.localScale * _spawnScaleMultiplier.Value;
        AlignRendererBoundsToGround(instance, groundPosition.y + _groundLift.Value + _spawnHeightOffset.Value);
        _spawnedObjects.Add(instance);
        return instance;
    }

    private void AlignRendererBoundsToGround(GameObject instance, float targetBottomY)
    {
        if (!TryGetRendererBounds(instance, out Bounds bounds))
        {
            Logger.LogWarning($"Generic valuable Tainted Gem display object '{instance.name}' has no renderer bounds; left at {FormatVector(instance.transform.position)}.");
            return;
        }

        float deltaY = targetBottomY - bounds.min.y;
        if (Mathf.Abs(deltaY) <= 0.0001f)
        {
            return;
        }

        instance.transform.position += Vector3.up * deltaY;
        Logger.LogInfo($"Adjusted generic valuable Tainted Gem display object '{instance.name}' by y={deltaY:0.###} so renderer bounds sit on ground. boundsBottom={bounds.min.y:0.###}; targetBottom={targetBottomY:0.###}; final={FormatVector(instance.transform.position)}.");
    }

    private static bool TryGetRendererBounds(GameObject instance, out Bounds bounds)
    {
        bounds = default;
        bool initialized = false;

        foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null)
            {
                continue;
            }

            if (!initialized)
            {
                bounds = renderer.bounds;
                initialized = true;
                continue;
            }

            bounds.Encapsulate(renderer.bounds);
        }

        return initialized;
    }

    private void DespawnGemObjects()
    {
        PruneDestroyedSpawnedObjects();

        if (_spawnedObjects.Count == 0)
        {
            return;
        }

        int destroyed = 0;
        foreach (GameObject spawned in _spawnedObjects)
        {
            if (spawned != null)
            {
                Destroy(spawned);
                destroyed++;
            }
        }

        _spawnedObjects.Clear();
        Logger.LogInfo($"Despawned {destroyed} generic valuable Tainted Gem display object(s).");
    }

    private void PruneDestroyedSpawnedObjects()
    {
        for (int i = _spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (_spawnedObjects[i] == null)
            {
                _spawnedObjects.RemoveAt(i);
            }
        }
    }

    private bool TryResolveBundlePath(out string bundlePath)
    {
        bundlePath = string.Empty;

        string configuredPath = _bundleRelativePath.Value ?? string.Empty;
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            Logger.LogWarning("Bundle.RelativePath is blank.");
            return false;
        }

        if (Uri.TryCreate(configuredPath, UriKind.Absolute, out _))
        {
            Logger.LogWarning("Bundle.RelativePath must be a local relative path, not an absolute URI.");
            return false;
        }

        string relativePath = configuredPath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        if (Path.IsPathRooted(relativePath))
        {
            Logger.LogWarning("Bundle.RelativePath must be relative to this plugin folder.");
            return false;
        }

        string[] segments = relativePath.Split(Path.DirectorySeparatorChar);
        foreach (string segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment) || segment == "." || segment == "..")
            {
                Logger.LogWarning("Bundle.RelativePath contains an unsafe path segment.");
                return false;
            }
        }

        string pluginDir = Path.GetDirectoryName(Info.Location) ?? Paths.PluginPath;
        string fullPluginDir;
        string candidatePath;
        try
        {
            fullPluginDir = Path.GetFullPath(pluginDir);
            candidatePath = Path.GetFullPath(Path.Combine(fullPluginDir, relativePath));
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Bundle.RelativePath could not be resolved: {ex.GetType().Name}: {ex.Message}");
            return false;
        }

        string requiredPrefix = fullPluginDir.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
            ? fullPluginDir
            : fullPluginDir + Path.DirectorySeparatorChar;

        if (!candidatePath.StartsWith(requiredPrefix, StringComparison.OrdinalIgnoreCase))
        {
            Logger.LogWarning("Bundle.RelativePath resolved outside this plugin folder.");
            return false;
        }

        bundlePath = candidatePath;
        return true;
    }

    private static string FormatVector(Vector3 vector)
    {
        return $"{vector.x:0.###}|{vector.y:0.###}|{vector.z:0.###}";
    }

    private static string FormatColliderName(RaycastHit hit)
    {
        return hit.collider != null ? hit.collider.name : "none";
    }
}
