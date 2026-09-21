using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Awaken.Kandra;
using Awaken.TG.Assets;
using Awaken.TG.Main.AI.SummonsAndAllies;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights.Factions;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Setup;
using Awaken.TG.Main.Templates;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace DragonKnightCompanion;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    private const int PanelWindowId = 8423051;
    private const float PanelWidth = 620f;
    private const float PanelHeight = 520f;
    private const float PanelMargin = 18f;
    private const string UiOwnerId = PluginGuid + ".panel";

    public const string PluginGuid = "kane.tgfoa.dragon-knight-companion";
    public const string PluginName = "Dragon Knight Companion";
    public const string PluginVersion = "0.1.1";

    private const string BundleFileName = "dragonknight_visuals";
    private const string IronPhaseAssetPath = "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Iron_Weapon.prefab";
    private const string FirePhaseAssetPath = "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Fire_Weapon.prefab";

    private const string DragonKnightLocationTemplateGuid = "d7b09116519f7564593be62781bee3db";
    private const string DragonKnightNpcTemplateGuid = "00f608ee051b57748a6a9ed8dae28678";
    private const string DragonKnightNativeVisualAddress = "b4d3a4e9a58fb1c4ea4c239f267faa56";

    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<KeyCode> _summonOrSwapHotkey = null!;
    private ConfigEntry<KeyCode> _recallHotkey = null!;
    private ConfigEntry<KeyCode> _defendToggleHotkey = null!;
    private ConfigEntry<KeyCode> _dismissHotkey = null!;
    private ConfigEntry<KeyCode> _togglePanelHotkey = null!;
    private ConfigEntry<float> _summonDistance = null!;
    private ConfigEntry<float> _summonRightOffset = null!;
    private ConfigEntry<float> _followCatchUpDistance = null!;
    private ConfigEntry<float> _tickSeconds = null!;
    private ConfigEntry<bool> _enableFollowCatchUp = null!;
    private ConfigEntry<bool> _enableDefendAssist = null!;
    private ConfigEntry<bool> _useFireVisual = null!;
    private ConfigEntry<bool> _hideNativeRenderers = null!;
    private ConfigEntry<bool> _prepareNativeTemplate = null!;

    private AssetBundle? _visualBundle;
    private bool _ownsVisualBundle;
    private GameObject? _ironPrefab;
    private GameObject? _firePrefab;
    private Location? _companion;
    private GameObject? _overlayInstance;
    private readonly List<Renderer> _overlayRenderers = new();
    private readonly List<KandraRenderer> _overlayKandraRenderers = new();
    private bool _defendMode;
    private float _nextTickTime;
    private float _nextDefendLogTime;
    private bool _panelVisible;
    private Rect _panelRect;
    private Vector2 _panelScrollPosition;
    private string _panelStatus = "Ready.";
    private GUIStyle? _panelWindowStyle;
    private GUIStyle? _headerStyle;
    private GUIStyle? _sectionStyle;
    private GUIStyle? _footerStyle;
    private GUIStyle? _titleStyle;
    private GUIStyle? _labelStyle;
    private GUIStyle? _mutedStyle;
    private GUIStyle? _buttonStyle;
    private GUIStyle? _selectedButtonStyle;
    private GUIStyle? _dangerButtonStyle;
    private Texture2D? _panelTexture;
    private Texture2D? _sectionTexture;
    private Texture2D? _buttonTexture;
    private Texture2D? _buttonHoverTexture;
    private Texture2D? _selectedButtonTexture;
    private Texture2D? _dangerButtonTexture;
    private Texture2D? _dangerHoverTexture;

    private void Awake()
    {
        _enabled = Config.Bind(
            "General",
            "Enabled",
            true,
            ManagerSetting("Enable Dragon Knight Companion.", "General", "Enabled", 0, 0));
        _togglePanelHotkey = Config.Bind(
            "Panel",
            "TogglePanelHotkey",
            KeyCode.PageDown,
            ManagerSetting("Open or close the Dragon Knight companion command screen.", "Screen", "Open Command Screen", 10, 0));
        _summonOrSwapHotkey = Config.Bind(
            "Input",
            "SummonOrSwapHotkey",
            KeyCode.KeypadPlus,
            ManagerSetting("Summon the Dragon Knight companion. If one is already active, replace it near the hero.", "Controls", "Summon / Swap Hotkey", 20, 0));
        _recallHotkey = Config.Bind(
            "Input",
            "RecallHotkey",
            KeyCode.KeypadEnter,
            ManagerSetting("Teleport the active companion near the hero.", "Controls", "Recall Hotkey", 20, 10));
        _defendToggleHotkey = Config.Bind(
            "Input",
            "DefendToggleHotkey",
            KeyCode.Keypad0,
            ManagerSetting("Toggle native defend prompts for the active companion.", "Controls", "Defend Toggle Hotkey", 20, 20));
        _dismissHotkey = Config.Bind(
            "Input",
            "DismissHotkey",
            KeyCode.KeypadMinus,
            ManagerSetting("Dismiss the active runtime companion.", "Controls", "Dismiss Hotkey", 20, 30));
        _summonDistance = Config.Bind(
            "Summon",
            "DistanceMeters",
            4.6f,
            ManagerSetting("Distance behind the hero for summon and recall.", "Summon", "Distance", 30, 0, new AcceptableValueRange<float>(2f, 20f)));
        _summonRightOffset = Config.Bind(
            "Summon",
            "RightOffsetMeters",
            1.8f,
            ManagerSetting("Right-side offset from the hero for summon and recall.", "Summon", "Right Offset", 30, 10, new AcceptableValueRange<float>(-6f, 6f)));
        _followCatchUpDistance = Config.Bind(
            "Follow",
            "CatchUpDistanceMeters",
            14f,
            ManagerSetting("Distance before follow mode recalls the runtime companion near the hero.", "Follow", "Catch-up Distance", 40, 0, new AcceptableValueRange<float>(6f, 80f)));
        _tickSeconds = Config.Bind(
            "Runtime",
            "TickSeconds",
            0.75f,
            ManagerSetting("Seconds between companion maintenance ticks.", "Advanced", "Runtime Tick Seconds", 90, 0, new AcceptableValueRange<float>(0.2f, 5f)));
        _enableFollowCatchUp = Config.Bind(
            "Follow",
            "EnableCatchUpRecall",
            true,
            ManagerSetting("Allow follow mode to recall the companion when it falls behind.", "Follow", "Catch-up Recall", 40, 10));
        _enableDefendAssist = Config.Bind(
            "Defend",
            "EnableNativeDefendAssist",
            true,
            ManagerSetting("Allow native NpcHeroPetAlly.EnterCombat prompts while the hero has live attackers.", "Defend", "Native Defend Assist", 50, 0));
        _useFireVisual = Config.Bind(
            "Visual",
            "UseFireVariant",
            false,
            ManagerSetting("Use the Fire weapon prefab instead of the Iron weapon prefab.", "Visual", "Use Fire Variant", 60, 0));
        _hideNativeRenderers = Config.Bind(
            "Visual",
            "HideNativeRenderers",
            true,
            ManagerSetting("Hide the spawned native NPC renderers so only the Dragon Knight boss overlay is visible.", "Visual", "Hide Native Renderers", 60, 10));
        _prepareNativeTemplate = Config.Bind(
            "Spawn",
            "PrepareNativeTemplate",
            true,
            ManagerSetting("Prepare the Dragon Knight repetitive NPC attachment with the boss mod native visual address before spawn.", "Advanced", "Prepare Native Template", 90, 10));

        Logger.LogInfo($"{PluginName} {PluginVersion} loaded. Panel={_togglePanelHotkey.Value}, Summon={_summonOrSwapHotkey.Value}, Recall={_recallHotkey.Value}, Defend={_defendToggleHotkey.Value}, Dismiss={_dismissHotkey.Value}.");
    }

    private void Update()
    {
        if (!_enabled.Value)
        {
            SetPanelVisible(false, "plugin disabled");
            return;
        }

        if (_togglePanelHotkey.Value != KeyCode.None && Input.GetKeyDown(_togglePanelHotkey.Value))
        {
            SetPanelVisible(!_panelVisible, "panel hotkey");
        }

        if (_panelVisible && Input.GetKeyDown(KeyCode.Escape))
        {
            SetPanelVisible(false, "escape");
        }

        if (!_panelVisible && Input.GetKeyDown(_summonOrSwapHotkey.Value))
        {
            TrySummonOrSwap();
        }

        if (!_panelVisible && Input.GetKeyDown(_recallHotkey.Value))
        {
            RecallCompanion("manual recall");
        }

        if (!_panelVisible && Input.GetKeyDown(_defendToggleHotkey.Value))
        {
            ToggleDefendMode();
        }

        if (!_panelVisible && Input.GetKeyDown(_dismissHotkey.Value))
        {
            DismissCompanion("manual dismiss");
        }

        TickCompanion();
        if (_panelVisible)
        {
            EnsureCursorForPanel();
            Input.ResetInputAxes();
        }
    }

    private void OnGUI()
    {
        if (!_enabled.Value || !_panelVisible)
        {
            return;
        }

        EnsurePanelStyles();
        EnsurePanelPlacement();
        EnsureCursorForPanel();
        GUI.depth = -1000;
        Rect drawnPanel = GUI.Window(PanelWindowId, _panelRect, DrawPanelWindow, GUIContent.none, _panelWindowStyle!);
        _panelRect = ClampPanelToScreen(drawnPanel);
    }

    private void OnDestroy()
    {
        SetPanelVisible(false, "plugin destroy");
        FoAModManagerBridge.SetCustomUiScope(UiOwnerId, active: false, freezeWorld: true);
        DismissCompanion("plugin destroy");
        DestroyPanelTextures();
        if (_visualBundle != null && _ownsVisualBundle)
        {
            _visualBundle.Unload(unloadAllLoadedObjects: false);
        }

        _visualBundle = null;
        _ironPrefab = null;
        _firePrefab = null;
    }

    private void TrySummonOrSwap()
    {
        if (!TryLoadDragonKnightVisuals(out string visualReason))
        {
            SetPanelStatus("Summon blocked: " + visualReason);
            Logger.LogWarning($"{PluginName} summon blocked: {visualReason}");
            return;
        }

        Hero? hero = Hero.Current;
        if (hero == null)
        {
            SetPanelStatus("Summon blocked: Hero.Current is null.");
            Logger.LogWarning($"{PluginName} summon blocked: Hero.Current is null.");
            return;
        }

        if (!TryResolveDragonKnightTemplate(out LocationTemplate? template, out string templateReason))
        {
            SetPanelStatus("Summon blocked: " + templateReason);
            Logger.LogWarning($"{PluginName} summon blocked: {templateReason}");
            return;
        }

        LocationTemplate resolvedTemplate = template!;
        if (HasActiveCompanion())
        {
            DismissCompanion("summon swap");
        }

        Vector3 spawnPosition = hero.Coords + hero.Rotation * GetCompanionOffset();
        Location location;
        try
        {
            location = resolvedTemplate.SpawnLocation(spawnPosition, hero.Rotation);
            location.MarkedNotSaved = true;
        }
        catch (Exception ex)
        {
            SetPanelStatus("Summon failed during SpawnLocation.");
            Logger.LogWarning($"{PluginName} summon failed during SpawnLocation: {ex.GetType().Name}: {ex.Message}");
            return;
        }

        if (!location.TryGetElement(out NpcElement npcElement) || npcElement == null)
        {
            location.MarkedNotSaved = true;
            location.Discard();
            SetPanelStatus("Summon discarded: spawned without NpcElement.");
            Logger.LogWarning($"{PluginName} summon discarded: Dragon Knight spawned without NpcElement.");
            return;
        }

        if (npcElement.IsUnique)
        {
            location.MarkedNotSaved = true;
            location.Discard();
            SetPanelStatus("Summon discarded: Dragon Knight NpcElement is unique.");
            Logger.LogWarning($"{PluginName} summon discarded: Dragon Knight NpcElement is unique.");
            return;
        }

        if (!TryApplyNativeAllyMarker(location, npcElement, hero))
        {
            location.MarkedNotSaved = true;
            location.Discard();
            SetPanelStatus("Summon discarded: native ally marker setup failed.");
            Logger.LogWarning($"{PluginName} summon discarded: native ally marker setup failed.");
            return;
        }

        if (!TryAttachDragonKnightOverlay(location, npcElement, out string overlayReason))
        {
            location.MarkedNotSaved = true;
            location.Discard();
            SetPanelStatus("Summon discarded: " + overlayReason);
            Logger.LogWarning($"{PluginName} summon discarded: {overlayReason}");
            return;
        }

        _companion = location;
        _defendMode = false;
        _nextTickTime = Time.unscaledTime + _tickSeconds.Value;
        SetPanelStatus("Summoned runtime Dragon Knight companion.");
        Logger.LogInfo($"{PluginName} summoned runtime Dragon Knight companion at {FormatVector(spawnPosition)}. template={resolvedTemplate.name}[{resolvedTemplate.GUID}] notSaved=1 ally=1 visual={(_useFireVisual.Value ? "fire" : "iron")}.");
    }

    private bool TryResolveDragonKnightTemplate(out LocationTemplate? template, out string reason)
    {
        template = null;
        try
        {
            template = new TemplateReference(DragonKnightLocationTemplateGuid).Get<LocationTemplate>();
        }
        catch (Exception ex)
        {
            reason = "Dragon Knight LocationTemplate resolution failed: " + ex.GetType().Name + ": " + ex.Message;
            return false;
        }

        if (template == null)
        {
            reason = "Dragon Knight LocationTemplate was not found for GUID " + DragonKnightLocationTemplateGuid + ".";
            return false;
        }

        if (!string.Equals(template.GUID, DragonKnightLocationTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = "Dragon Knight LocationTemplate GUID changed.";
            return false;
        }

        NpcAttachment? npcAttachment = template.GetComponent<NpcAttachment>();
        if (npcAttachment == null)
        {
            reason = "Dragon Knight LocationTemplate has no NpcAttachment.";
            return false;
        }

        if (npcAttachment.IsUnique)
        {
            reason = "Dragon Knight LocationTemplate is unique and cannot be used for one-session companion spawning.";
            return false;
        }

        RepetitiveNpcAttachment? repetitiveAttachment = template.GetComponent<RepetitiveNpcAttachment>();
        if (repetitiveAttachment == null)
        {
            reason = string.Empty;
            return true;
        }

        NpcTemplate npcTemplate = repetitiveAttachment.NpcTemplate;
        if (npcTemplate == null ||
            !string.Equals(npcTemplate.GUID, DragonKnightNpcTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = "Dragon Knight NpcTemplate identity changed before companion spawn.";
            return false;
        }

        if (_prepareNativeTemplate.Value)
        {
            repetitiveAttachment.Setup(npcTemplate, new ARAssetReference(DragonKnightNativeVisualAddress));
            ARAssetReference visual = repetitiveAttachment.VisualPrefab();
            if (visual == null ||
                !string.Equals(visual.Address, DragonKnightNativeVisualAddress, StringComparison.Ordinal))
            {
                reason = "Dragon Knight native bootstrap visual did not read back.";
                return false;
            }
        }

        reason = string.Empty;
        return true;
    }

    private bool TryApplyNativeAllyMarker(Location location, NpcElement npcElement, Hero hero)
    {
        try
        {
            if (!npcElement.HasElement<NpcHeroPetAlly>())
            {
                npcElement.OverrideFaction(hero.GetFactionTemplateForSummon(), FactionOverrideContext.Summon);
                npcElement.AddElement(new NpcHeroPetAlly(hero));
            }

            location.MarkedNotSaved = true;
            NpcHeroPetAlly marker = npcElement.TryGetElement<NpcHeroPetAlly>();
            return marker != null && !marker.HasBeenDiscarded;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName} native ally marker setup failed: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private bool TryAttachDragonKnightOverlay(Location location, NpcElement npcElement, out string reason)
    {
        reason = string.Empty;
        GameObject? prefab = _useFireVisual.Value ? _firePrefab : _ironPrefab;
        if (prefab == null)
        {
            reason = "selected Dragon Knight visual prefab is missing.";
            return false;
        }

        if (npcElement.Controller == null)
        {
            reason = "NpcElement.Controller is missing before Dragon Knight visual overlay attachment.";
            return false;
        }

        Transform parent = npcElement.Controller.AlivePrefab != null
            ? npcElement.Controller.AlivePrefab.transform
            : npcElement.Controller.transform;

        ReleaseDragonKnightOverlay();
        GameObject overlay = UnityEngine.Object.Instantiate(prefab, parent, false);
        overlay.name = _useFireVisual.Value
            ? "DragonKnightCompanionRenderOverlay_Fire"
            : "DragonKnightCompanionRenderOverlay_Iron";
        overlay.transform.localPosition = Vector3.zero;
        overlay.transform.localRotation = Quaternion.identity;
        overlay.transform.localScale = Vector3.one;

        int collidersDisabled = DisableColliders(overlay);
        Renderer[] unityRenderers = overlay.GetComponentsInChildren<Renderer>(true);
        KandraRenderer[] kandraRenderers = overlay.GetComponentsInChildren<KandraRenderer>(true);
        if (unityRenderers.Length + kandraRenderers.Length <= 0)
        {
            UnityEngine.Object.Destroy(overlay);
            reason = "Dragon Knight visual overlay instantiated without renderers.";
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
        ReassertRenderOverlayVisibility(location, npcElement, ref hiddenUnity, ref hiddenKandra);
        Logger.LogInfo($"{PluginName} attached Dragon Knight visual overlay. renderers={unityRenderers.Length + kandraRenderers.Length} collidersDisabled={collidersDisabled} hiddenUnity={hiddenUnity} hiddenKandra={hiddenKandra}.");
        return true;
    }

    private void TickCompanion()
    {
        if (Time.unscaledTime < _nextTickTime)
        {
            return;
        }

        _nextTickTime = Time.unscaledTime + Math.Max(0.2f, _tickSeconds.Value);

        if (!TryGetActiveCompanion(out Location? location, out NpcElement? npcElement))
        {
            return;
        }

        location.MarkedNotSaved = true;
        int hiddenUnity = 0;
        int hiddenKandra = 0;
        ReassertRenderOverlayVisibility(location, npcElement, ref hiddenUnity, ref hiddenKandra);

        Hero? hero = Hero.Current;
        if (hero == null)
        {
            return;
        }

        if (_enableFollowCatchUp.Value && !_defendMode)
        {
            float distance = Vector3.Distance(location.Coords, hero.Coords);
            if (distance > _followCatchUpDistance.Value)
            {
                RecallCompanion("follow catch-up");
            }
        }

        if (_enableDefendAssist.Value && _defendMode)
        {
            int attackers = CountLiveHeroAttackers(hero);
            if (attackers > 0)
            {
                TriggerDefendAssist(location, npcElement, attackers, "defend mode");
            }
        }
    }

    private bool TryGetActiveCompanion([NotNullWhen(true)] out Location? location, [NotNullWhen(true)] out NpcElement? npcElement)
    {
        location = null;
        npcElement = null;
        if (_companion == null)
        {
            return false;
        }

        if (_companion.HasBeenDiscarded)
        {
            ClearCompanionState();
            return false;
        }

        if (!_companion.TryGetElement(out NpcElement npc) || npc == null || npc.HasBeenDiscarded)
        {
            ClearCompanionState();
            return false;
        }

        NpcHeroPetAlly marker = npc.TryGetElement<NpcHeroPetAlly>();
        if (marker == null || marker.HasBeenDiscarded)
        {
            ClearCompanionState();
            return false;
        }

        location = _companion;
        npcElement = npc;
        return true;
    }

    private bool HasActiveCompanion()
    {
        return TryGetActiveCompanion(out _, out _);
    }

    private void RecallCompanion(string reason)
    {
        if (!TryGetActiveCompanion(out Location? location, out _))
        {
            SetPanelStatus("Recall ignored: no active companion.");
            return;
        }

        Hero? hero = Hero.Current;
        if (hero == null)
        {
            SetPanelStatus("Recall blocked: Hero.Current is null.");
            return;
        }

        Vector3 target = hero.Coords + hero.Rotation * GetCompanionOffset();
        try
        {
            location.MarkedNotSaved = true;
            location.MoveAndRotateTo(target, hero.Rotation, teleport: true);
            location.MarkedNotSaved = true;
            SetPanelStatus("Companion recalled.");
            Logger.LogInfo($"{PluginName} recalled companion during {reason} to {FormatVector(target)}.");
        }
        catch (Exception ex)
        {
            SetPanelStatus("Recall failed.");
            Logger.LogWarning($"{PluginName} recall failed during {reason}: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void ToggleDefendMode()
    {
        if (!TryGetActiveCompanion(out Location? location, out NpcElement? npcElement))
        {
            SetPanelStatus("Defend toggle ignored: no active companion.");
            Logger.LogInfo($"{PluginName} defend toggle ignored: no active runtime companion.");
            return;
        }

        _defendMode = !_defendMode;
        location.MarkedNotSaved = true;
        SetPanelStatus("Mode set to " + (_defendMode ? "Defend." : "Follow."));
        Logger.LogInfo($"{PluginName} companion mode={(_defendMode ? "Defend" : "Follow")}.");

        Hero? hero = Hero.Current;
        if (_defendMode && hero != null)
        {
            int attackers = CountLiveHeroAttackers(hero);
            if (attackers > 0)
            {
                TriggerDefendAssist(location, npcElement, attackers, "defend toggle");
            }
        }
    }

    private void TriggerDefendAssist(Location location, NpcElement npcElement, int attackerCount, string reason)
    {
        try
        {
            NpcHeroPetAlly marker = npcElement.TryGetElement<NpcHeroPetAlly>();
            if (marker == null || marker.HasBeenDiscarded)
            {
                return;
            }

            location.MarkedNotSaved = true;
            marker.EnterCombat();
            if (Time.unscaledTime >= _nextDefendLogTime)
            {
                Logger.LogInfo($"{PluginName} prompted native Dragon Knight companion defend during {reason}. attackers={attackerCount}");
                _nextDefendLogTime = Time.unscaledTime + 4f;
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName} defend prompt failed during {reason}: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private int CountLiveHeroAttackers(Hero hero)
    {
        int count = 0;
        try
        {
            foreach (ICharacter attacker in hero.PossibleAttackers)
            {
                if (attacker == null || attacker.HasBeenDiscarded || !attacker.IsAlive)
                {
                    continue;
                }

                count++;
            }
        }
        catch
        {
            return 0;
        }

        return count;
    }

    private void DismissCompanion(string reason)
    {
        try
        {
            Location? location = _companion;
            ReleaseDragonKnightOverlay();
            if (location != null && !location.HasBeenDiscarded)
            {
                location.MarkedNotSaved = true;
                location.Discard();
                SetPanelStatus("Companion dismissed.");
                Logger.LogInfo($"{PluginName} dismissed runtime Dragon Knight companion during {reason}.");
            }
            else
            {
                SetPanelStatus("Dismiss ignored: no active companion.");
            }
        }
        catch (Exception ex)
        {
            SetPanelStatus("Dismiss failed.");
            Logger.LogWarning($"{PluginName} dismiss failed during {reason}: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            _companion = null;
            _defendMode = false;
        }
    }

    private Vector3 GetCompanionOffset()
    {
        return (Vector3.back * _summonDistance.Value) + (Vector3.right * _summonRightOffset.Value);
    }

    private bool TryLoadDragonKnightVisuals(out string reason)
    {
        if (_ironPrefab != null && _firePrefab != null)
        {
            reason = string.Empty;
            return true;
        }

        if (_visualBundle == null && !TryResolveVisualBundle(out reason))
        {
            return false;
        }

        try
        {
            _ironPrefab ??= _visualBundle!.LoadAsset<GameObject>(IronPhaseAssetPath);
            _firePrefab ??= _visualBundle!.LoadAsset<GameObject>(FirePhaseAssetPath);
        }
        catch (Exception ex)
        {
            reason = "Dragon Knight visual prefab load failed: " + ex.GetType().Name + ": " + ex.Message;
            return false;
        }

        if (_ironPrefab == null)
        {
            reason = "Dragon Knight iron visual prefab was not found at " + IronPhaseAssetPath + ".";
            return false;
        }

        if (_firePrefab == null)
        {
            reason = "Dragon Knight fire visual prefab was not found at " + FirePhaseAssetPath + ".";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private bool TryResolveVisualBundle(out string reason)
    {
        foreach (AssetBundle loadedBundle in AssetBundle.GetAllLoadedAssetBundles())
        {
            if (loadedBundle == null)
            {
                continue;
            }

            if (string.Equals(loadedBundle.name, BundleFileName, StringComparison.OrdinalIgnoreCase))
            {
                _visualBundle = loadedBundle;
                _ownsVisualBundle = false;
                reason = string.Empty;
                Logger.LogInfo($"{PluginName} using already-loaded Dragon Knight visual bundle.");
                return true;
            }
        }

        foreach (string candidatePath in GetVisualBundleCandidatePaths())
        {
            if (!File.Exists(candidatePath))
            {
                continue;
            }

            AssetBundle? bundle = AssetBundle.LoadFromFile(candidatePath);
            if (bundle == null)
            {
                continue;
            }

            _visualBundle = bundle;
            _ownsVisualBundle = true;
            reason = string.Empty;
            Logger.LogInfo($"{PluginName} loaded Dragon Knight visual bundle from {candidatePath}.");
            return true;
        }

        reason = "missing Dragon Knight boss visual bundle. Expected " + BundleFileName + " in this plugin folder or a sibling DragonKnight boss plugin folder.";
        return false;
    }

    private IEnumerable<string> GetVisualBundleCandidatePaths()
    {
        string pluginDir = Path.GetDirectoryName(Info.Location) ?? Paths.PluginPath;
        yield return Path.Combine(pluginDir, BundleFileName);

        string? pluginParent = Directory.GetParent(pluginDir)?.FullName;
        if (string.IsNullOrWhiteSpace(pluginParent))
        {
            yield break;
        }

        yield return Path.Combine(pluginParent, "DragonKnight", BundleFileName);
        yield return Path.Combine(pluginParent, "dragon-knight", BundleFileName);
        yield return Path.Combine(pluginParent, "DragonKnightBoss", BundleFileName);
    }

    private void ReassertRenderOverlayVisibility(Location location, NpcElement npcElement, ref int hiddenUnity, ref int hiddenKandra)
    {
        if (_overlayInstance == null || npcElement.Controller == null)
        {
            return;
        }

        foreach (Renderer renderer in _overlayRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }

        foreach (KandraRenderer renderer in _overlayKandraRenderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }

        if (!_hideNativeRenderers.Value)
        {
            return;
        }

        foreach (Renderer renderer in npcElement.Controller.gameObject.GetComponentsInChildren<Renderer>(true))
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

        foreach (KandraRenderer renderer in npcElement.Controller.gameObject.GetComponentsInChildren<KandraRenderer>(true))
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

        location.MarkedNotSaved = true;
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

    private void ReleaseDragonKnightOverlay()
    {
        if (_overlayInstance != null)
        {
            UnityEngine.Object.Destroy(_overlayInstance);
            _overlayInstance = null;
        }

        _overlayRenderers.Clear();
        _overlayKandraRenderers.Clear();
    }

    private void ClearCompanionState()
    {
        ReleaseDragonKnightOverlay();
        _companion = null;
        _defendMode = false;
    }

    private void SetPanelVisible(bool visible, string reason)
    {
        if (_panelVisible == visible)
        {
            return;
        }

        _panelVisible = visible;
        FoAModManagerBridge.SetCustomUiScope(UiOwnerId, visible, freezeWorld: true);
        SetPanelStatus(visible ? "Screen opened by " + reason + "." : "Screen closed by " + reason + ".");
        if (visible)
        {
            EnsureCursorForPanel();
        }
    }

    private void DrawPanelWindow(int windowId)
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal(_headerStyle!, GUILayout.Height(66f));
        GUILayout.BeginVertical();
        GUILayout.Label("Dragon Knight Companion", _titleStyle!);
        GUILayout.Label($"{_togglePanelHotkey.Value}: companion screen   Esc: close", _mutedStyle!);
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Close", _dangerButtonStyle!, GUILayout.Width(88f), GUILayout.Height(38f)))
        {
            SetPanelVisible(false, "close button");
            GUI.FocusControl(null);
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(10f);
        _panelScrollPosition = GUILayout.BeginScrollView(_panelScrollPosition, false, true);
        GUILayout.BeginVertical(_sectionStyle!, GUILayout.MinHeight(138f));
        GUILayout.Label("Runtime Companion", _labelStyle!);
        GUILayout.Label("Active: " + GetActiveCompanionLabel(), _mutedStyle!);
        GUILayout.Label("Mode: " + (_defendMode ? "Defend" : "Follow"), _mutedStyle!);
        GUILayout.Label("Visual: " + (_useFireVisual.Value ? "Fire weapon prefab" : "Iron weapon prefab"), _mutedStyle!);
        GUILayout.Label("Bundle: " + GetVisualBundleLabel(), _mutedStyle!);
        GUILayout.EndVertical();

        GUILayout.Space(10f);
        GUILayout.BeginVertical(_sectionStyle!);
        GUILayout.Label("Commands", _labelStyle!);
        GUILayout.Space(6f);
        GUILayout.BeginHorizontal();
        DrawPanelButton("Summon / Swap", TrySummonOrSwap, _selectedButtonStyle!);
        DrawPanelButton("Recall", () => RecallCompanion("panel recall"), _buttonStyle!);
        DrawPanelButton("Dismiss", () => DismissCompanion("panel dismiss"), _dangerButtonStyle!);
        GUILayout.EndHorizontal();

        GUILayout.Space(8f);
        GUILayout.BeginHorizontal();
        DrawPanelButton("Follow", () => SetCompanionMode(defend: false), _defendMode ? _buttonStyle! : _selectedButtonStyle!);
        DrawPanelButton("Defend", () => SetCompanionMode(defend: true), _defendMode ? _selectedButtonStyle! : _buttonStyle!);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.Space(10f);
        GUILayout.BeginVertical(_sectionStyle!);
        GUILayout.Label("Visual Variant", _labelStyle!);
        GUILayout.Space(6f);
        GUILayout.BeginHorizontal();
        DrawPanelButton("Iron", () => SetVisualVariant(useFire: false), _useFireVisual.Value ? _buttonStyle! : _selectedButtonStyle!);
        DrawPanelButton("Fire", () => SetVisualVariant(useFire: true), _useFireVisual.Value ? _selectedButtonStyle! : _buttonStyle!);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.Space(10f);
        GUILayout.BeginVertical(_sectionStyle!);
        GUILayout.Label("Config", _labelStyle!);
        GUILayout.Space(6f);
        GUILayout.BeginHorizontal();
        DrawPanelButton("Save Config", SaveConfigFromPanel, _buttonStyle!);
        DrawPanelButton("Reload Visual", ReloadVisualFromPanel, _buttonStyle!);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.Space(10f);
        GUILayout.BeginVertical(_footerStyle!, GUILayout.MinHeight(58f));
        GUILayout.Label("Status: " + _panelStatus, _mutedStyle!);
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUI.DragWindow(new Rect(0f, 0f, Mathf.Max(180f, _panelRect.width - 104f), 72f));

        Event current = Event.current;
        if (current.type == EventType.MouseDown || current.type == EventType.MouseDrag || current.type == EventType.MouseUp)
        {
            current.Use();
        }
    }

    private void DrawPanelButton(string label, Action action, GUIStyle style)
    {
        if (!GUILayout.Button(label, style, GUILayout.Height(42f)))
        {
            return;
        }

        try
        {
            action();
        }
        finally
        {
            GUI.FocusControl(null);
        }
    }

    private void SetCompanionMode(bool defend)
    {
        if (!TryGetActiveCompanion(out Location? location, out NpcElement? npcElement))
        {
            SetPanelStatus("Mode ignored: no active companion.");
            return;
        }

        _defendMode = defend;
        location.MarkedNotSaved = true;
        SetPanelStatus("Mode set to " + (defend ? "Defend." : "Follow."));
        if (!defend || Hero.Current == null)
        {
            return;
        }

        int attackers = CountLiveHeroAttackers(Hero.Current);
        if (attackers > 0)
        {
            TriggerDefendAssist(location, npcElement, attackers, "panel defend");
        }
    }

    private void SetVisualVariant(bool useFire)
    {
        _useFireVisual.Value = useFire;
        if (!TryGetActiveCompanion(out Location? location, out NpcElement? npcElement))
        {
            SetPanelStatus("Visual set to " + (useFire ? "Fire." : "Iron."));
            return;
        }

        if (!TryLoadDragonKnightVisuals(out string visualReason))
        {
            SetPanelStatus("Visual reload blocked: " + visualReason);
            return;
        }

        if (!TryAttachDragonKnightOverlay(location, npcElement, out string overlayReason))
        {
            SetPanelStatus("Visual reload failed: " + overlayReason);
            return;
        }

        location.MarkedNotSaved = true;
        SetPanelStatus("Visual set to " + (useFire ? "Fire." : "Iron."));
    }

    private void SaveConfigFromPanel()
    {
        Config.Save();
        SetPanelStatus("Config saved.");
    }

    private void ReloadVisualFromPanel()
    {
        if (!TryGetActiveCompanion(out Location? location, out NpcElement? npcElement))
        {
            SetPanelStatus("Reload ignored: no active companion.");
            return;
        }

        if (!TryLoadDragonKnightVisuals(out string visualReason))
        {
            SetPanelStatus("Reload blocked: " + visualReason);
            return;
        }

        if (!TryAttachDragonKnightOverlay(location, npcElement, out string overlayReason))
        {
            SetPanelStatus("Reload failed: " + overlayReason);
            return;
        }

        location.MarkedNotSaved = true;
        SetPanelStatus("Visual reloaded.");
    }

    private string GetActiveCompanionLabel()
    {
        if (!TryGetActiveCompanion(out Location? location, out _))
        {
            return "None";
        }

        string id = string.IsNullOrWhiteSpace(location.ID) ? "runtime location" : location.ID;
        return $"{id} at {FormatVector(location.Coords)}";
    }

    private string GetVisualBundleLabel()
    {
        if (_visualBundle != null)
        {
            return _ownsVisualBundle ? "Loaded by companion mod" : "Using already-loaded boss bundle";
        }

        return "Not loaded yet";
    }

    private void EnsurePanelPlacement()
    {
        if (_panelRect.width > 0f && _panelRect.height > 0f)
        {
            _panelRect = ClampPanelToScreen(_panelRect);
            return;
        }

        float width = Mathf.Min(PanelWidth, Mathf.Max(360f, Screen.width - (PanelMargin * 2f)));
        float height = Mathf.Min(PanelHeight, Mathf.Max(360f, Screen.height - (PanelMargin * 2f)));
        _panelRect = new Rect(
            Mathf.Max(PanelMargin, (Screen.width - width) * 0.5f),
            Mathf.Max(PanelMargin, (Screen.height - height) * 0.5f),
            width,
            height);
    }

    private static Rect ClampPanelToScreen(Rect rect)
    {
        float maxWidth = Mathf.Max(320f, Screen.width - (PanelMargin * 2f));
        float maxHeight = Mathf.Max(320f, Screen.height - (PanelMargin * 2f));
        float width = Mathf.Min(Mathf.Max(320f, rect.width), maxWidth);
        float height = Mathf.Min(Mathf.Max(320f, rect.height), maxHeight);
        float x = Mathf.Clamp(rect.x, PanelMargin, Mathf.Max(PanelMargin, Screen.width - width - PanelMargin));
        float y = Mathf.Clamp(rect.y, PanelMargin, Mathf.Max(PanelMargin, Screen.height - height - PanelMargin));
        return new Rect(x, y, width, height);
    }

    private void EnsureCursorForPanel()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void EnsurePanelStyles()
    {
        if (_panelWindowStyle != null)
        {
            return;
        }

        _panelTexture = BuildTexture(new Color(0.045f, 0.05f, 0.052f, 0.97f));
        _sectionTexture = BuildTexture(new Color(0.083f, 0.088f, 0.093f, 0.96f));
        _buttonTexture = BuildTexture(new Color(0.16f, 0.23f, 0.27f, 0.98f));
        _buttonHoverTexture = BuildTexture(new Color(0.22f, 0.32f, 0.37f, 1f));
        _selectedButtonTexture = BuildTexture(new Color(0.36f, 0.24f, 0.13f, 0.98f));
        _dangerButtonTexture = BuildTexture(new Color(0.34f, 0.11f, 0.1f, 0.98f));
        _dangerHoverTexture = BuildTexture(new Color(0.46f, 0.16f, 0.13f, 1f));

        _panelWindowStyle = new GUIStyle(GUI.skin.window)
        {
            normal = { background = _panelTexture },
            padding = new RectOffset(18, 18, 18, 18),
            border = new RectOffset(8, 8, 8, 8)
        };

        _headerStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = _sectionTexture },
            padding = new RectOffset(18, 18, 12, 12)
        };

        _sectionStyle = new GUIStyle(_headerStyle)
        {
            padding = new RectOffset(18, 18, 14, 14)
        };

        _footerStyle = new GUIStyle(_sectionStyle)
        {
            padding = new RectOffset(18, 18, 12, 12)
        };

        _titleStyle = BuildLabel(25, FontStyle.Bold, new Color(0.95f, 0.78f, 0.45f, 1f));
        _labelStyle = BuildLabel(17, FontStyle.Bold, new Color(0.92f, 0.91f, 0.86f, 1f));
        _mutedStyle = BuildLabel(14, FontStyle.Normal, new Color(0.72f, 0.77f, 0.76f, 1f));
        _buttonStyle = BuildButton(_buttonTexture, _buttonHoverTexture, new Color(0.92f, 0.96f, 0.96f, 1f));
        _selectedButtonStyle = BuildButton(_selectedButtonTexture, _selectedButtonTexture, new Color(1f, 0.92f, 0.7f, 1f));
        _dangerButtonStyle = BuildButton(_dangerButtonTexture, _dangerHoverTexture, new Color(1f, 0.88f, 0.84f, 1f));
    }

    private static GUIStyle BuildLabel(int fontSize, FontStyle fontStyle, Color textColor)
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize,
            fontStyle = fontStyle,
            normal = { textColor = textColor },
            wordWrap = true,
            alignment = TextAnchor.MiddleLeft
        };
    }

    private static GUIStyle BuildButton(Texture2D normal, Texture2D hover, Color textColor)
    {
        return new GUIStyle(GUI.skin.button)
        {
            normal = { background = normal, textColor = textColor },
            hover = { background = hover, textColor = textColor },
            active = { background = hover, textColor = textColor },
            focused = { background = normal, textColor = textColor },
            fontSize = 15,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 8, 8)
        };
    }

    private static Texture2D BuildTexture(Color color)
    {
        Texture2D texture = new(1, 1, TextureFormat.RGBA32, mipChain: false);
        texture.SetPixel(0, 0, color);
        texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        return texture;
    }

    private void DestroyPanelTextures()
    {
        DestroyTexture(ref _panelTexture);
        DestroyTexture(ref _sectionTexture);
        DestroyTexture(ref _buttonTexture);
        DestroyTexture(ref _buttonHoverTexture);
        DestroyTexture(ref _selectedButtonTexture);
        DestroyTexture(ref _dangerButtonTexture);
        DestroyTexture(ref _dangerHoverTexture);
        _panelWindowStyle = null;
    }

    private static void DestroyTexture(ref Texture2D? texture)
    {
        if (texture != null)
        {
            UnityEngine.Object.Destroy(texture);
            texture = null;
        }
    }

    private void SetPanelStatus(string status)
    {
        _panelStatus = status;
    }

    private static ConfigDescription ManagerSetting(
        string description,
        string displaySection,
        string displayName,
        int sectionOrder,
        int order,
        AcceptableValueBase? acceptableValues = null)
    {
        return new ConfigDescription(
            description,
            acceptableValues,
            new SettingUiMetadata(displaySection, displayName, sectionOrder, order));
    }

    private static string FormatVector(Vector3 value)
    {
        return $"({value.x:0.###}, {value.y:0.###}, {value.z:0.###})";
    }
}

internal sealed class SettingUiMetadata
{
    internal SettingUiMetadata(string displaySection, string displayName, int sectionOrder, int order)
    {
        DisplaySection = displaySection;
        DisplayName = displayName;
        SectionOrder = sectionOrder;
        Order = order;
    }

    public string DisplaySection { get; }
    public string DisplayName { get; }
    public int SectionOrder { get; }
    public int Order { get; }
}
