using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Awaken.TG.Assets;
using Awaken.TG.Main.AI.SummonsAndAllies;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights;
using Awaken.TG.Main.Fights.DamageInfo;
using Awaken.TG.Main.Fights.Factions;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.CharacterSheet.Items.Panel.Slot;
using Awaken.TG.Main.Heroes.Items;
using Awaken.TG.Main.Heroes.Items.Attachments;
using Awaken.TG.Main.Heroes.Items.LootTables;
using Awaken.TG.Main.Heroes.Items.Tooltips;
using Awaken.TG.Main.Heroes.Skills;
using Awaken.TG.Main.Localization;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Attachments.Elements;
using Awaken.TG.Main.Locations.Attachments.Elements.DeathBehaviours;
using Awaken.TG.Main.Locations.Setup;
using Awaken.TG.Main.Locations.Shops;
using Awaken.TG.Main.Locations.Shops.Stocks;
using Awaken.TG.Main.Locations.Shops.UI;
using Awaken.TG.Main.Skills;
using Awaken.TG.Main.Templates;
using Awaken.TG.Main.Utility.RichEnums;
using Awaken.TG.MVC;
using Awaken.TG.MVC.Elements;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace AvalonCreatureCompanionShared;

public sealed class StandaloneCreatureCompanionDefinition
{
    internal StandaloneCreatureCompanionDefinition(
        string id,
        string displayName,
        string spellTemplateGuid,
        string spellTemplateName,
        string spellDisplayName,
        string locationTemplateName,
        string locationTemplateGuid,
        string npcTemplateGuid,
        string resolverMethodName,
        string visualAddress,
        float visualScaleMultiplier,
        Color32 primaryIconColor,
        Color32 accentIconColor,
        string? templateVisualAddress = null,
        string? runtimeOverlayVisualAddress = null,
        string? embeddedIconResourceName = null)
    {
        Id = id;
        DisplayName = displayName;
        SpellTemplateGuid = spellTemplateGuid;
        SpellTemplateName = spellTemplateName;
        SpellDisplayName = spellDisplayName;
        LocationTemplateName = locationTemplateName;
        LocationTemplateGuid = locationTemplateGuid;
        NpcTemplateGuid = npcTemplateGuid;
        ResolverMethodName = resolverMethodName;
        VisualAddress = visualAddress;
        TemplateVisualAddress = templateVisualAddress ?? visualAddress;
        RuntimeOverlayVisualAddress = runtimeOverlayVisualAddress;
        VisualScaleMultiplier = visualScaleMultiplier;
        PrimaryIconColor = primaryIconColor;
        AccentIconColor = accentIconColor;
        EmbeddedIconResourceName = embeddedIconResourceName;
    }

    internal string Id { get; }
    internal string DisplayName { get; }
    internal string SpellTemplateGuid { get; }
    internal string SpellTemplateName { get; }
    internal string SpellDisplayName { get; }
    internal string LocationTemplateName { get; }
    internal string LocationTemplateGuid { get; }
    internal string NpcTemplateGuid { get; }
    internal string ResolverMethodName { get; }
    internal string VisualAddress { get; }
    internal string TemplateVisualAddress { get; }
    internal string? RuntimeOverlayVisualAddress { get; }
    internal float VisualScaleMultiplier { get; }
    internal Color32 PrimaryIconColor { get; }
    internal Color32 AccentIconColor { get; }
    internal string? EmbeddedIconResourceName { get; }

    internal string SpellDisplayDescription =>
        $"Calls an allied Avalon {DisplayName} companion to fight beside you. Only one {DisplayName} companion from this standalone mod can be active; casting again replaces the current companion.";

    internal string SpellLightCastDescription =>
        $"Summons or replaces your allied Avalon {DisplayName} companion through the guarded one-session companion route.";

    internal string SpellHeavyCastDescription =>
        $"Summons or replaces your allied Avalon {DisplayName} companion. The companion is not saved and is cleaned up on dismiss or scene teardown.";
}

public abstract class StandaloneCreatureCompanionPlugin : BaseUnityPlugin
{
    internal const string ResolverTypeName = "AvalonAwakened.AvalonCreatureWorldApi, AvalonAwakened";
    internal const string KnownMerchantShopGuid = "75a071140bc819d4ab6e9e37abfdfa59";
    private const float TickIntervalSeconds = 0.5f;
    private const string SummonSourceItemTemplateGuid = CustomCreatureCallSpellFeature.SourceSpellTemplateGuid;

    private static readonly FieldInfo? PlayAnimationAlivePrefabField =
        typeof(PostponedRagdollBehaviourBase).GetField("_alivePrefab", BindingFlags.Instance | BindingFlags.NonPublic);

    private readonly HashSet<Collider> _runtimeHealthHitboxes = new();
    private readonly Dictionary<Collider, int> _runtimeHitboxLayers = new();
    private readonly List<Material> _runtimeMaterials = new();
    private readonly List<Material> _retainedDeathRuntimeMaterials = new();
    private readonly List<AsyncOperationHandle<GameObject>> _retainedDeathHandles = new();
    private readonly HashSet<Renderer> _runtimeOverlayRenderers = new();
    private readonly HashSet<Renderer> _runtimeOverlayStrippedRenderers = new();
    private readonly HashSet<Component> _runtimeOverlayKandraRenderers = new();
    private readonly HashSet<Component> _runtimeOverlayStrippedKandraRenderers = new();

    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<bool> _registerCallSpell = null!;
    private ConfigEntry<bool> _enableCallCompanionRoute = null!;
    private ConfigEntry<bool> _grantCallOnLoad = null!;
    private ConfigEntry<bool> _merchantStockEnabled = null!;
    private ConfigEntry<string> _merchantStockTargetShopGuid = null!;
    private ConfigEntry<int> _merchantStockQuantity = null!;
    private ConfigEntry<float> _summonDistance = null!;
    private ConfigEntry<float> _summonRightOffset = null!;
    private ConfigEntry<float> _followCatchUpDistance = null!;

    private Harmony? _harmony;
    private Location? _activeCompanion;
    private GameObject? _overlayInstance;
    private AsyncOperationHandle<GameObject> _overlayHandle;
    private bool _overlayRequested;
    private bool _overlayReady;
    private bool _nativeVisualReadyLogged;
    private bool _lifecycleReadyLogged;
    private bool _lifecycleWaitingLogged;
    private bool _nativeDeathAcceptedLogged;
    private bool _nativeDeathPendingLogged;
    private bool _animatedDeathVisibilityLogged;
    private bool _overlayFailureLogged;
    private float _nextTickTime;
    private Transform? _nativeVisualScaleRoot;
    private Vector3 _nativeVisualBaseScale = Vector3.one;
    private bool _nativeVisualScaleCaptured;
    private int _runtimeMaterialCount;
    private int _materialSlotCount;
    private string _sourceShaders = "none";
    private string _targetShaders = "none";
    private string _baseTextureSources = "none";
    private string _normalTextureSources = "none";

    internal static StandaloneCreatureCompanionPlugin? Instance { get; private set; }

    protected abstract StandaloneCreatureCompanionDefinition Definition { get; }
    protected abstract string PluginGuidValue { get; }
    protected abstract string PluginNameValue { get; }
    protected abstract string PluginVersionValue { get; }

    internal ManualLogSource ModLogger => Logger;
    internal string PluginNameForLogs => PluginNameValue;
    internal bool RegisterCallSpell => _registerCallSpell.Value;
    internal bool EnableCallCompanionRoute => _enableCallCompanionRoute.Value;
    internal bool GrantCallOnLoad => _grantCallOnLoad.Value;
    internal bool MerchantStockEnabled => _merchantStockEnabled.Value;
    internal string MerchantStockTargetShopGuid => _merchantStockTargetShopGuid.Value ?? string.Empty;
    internal int MerchantStockQuantity => Mathf.Clamp(_merchantStockQuantity.Value, 1, 99);
    internal StandaloneCreatureCompanionDefinition CompanionDefinition => Definition;

    protected virtual void Awake()
    {
        Instance = this;
        BindConfig();

        _harmony = new Harmony(PluginGuidValue);
        CustomCreatureCallSpellFeature.Apply(_harmony, Logger);
        CreatureCallItemIconPatch.Apply(_harmony, Logger);
        CustomCreatureMerchantStockPatch.Apply(_harmony, Logger);

        Logger.LogInfo(
            $"{PluginNameValue} {PluginVersionValue} loaded. " +
            $"creature={Definition.Id}; provider=AvalonAwakened; registerCallSpell={RegisterCallSpell}; " +
            $"merchantStock={MerchantStockEnabled}; merchantShop={MerchantStockTargetShopGuid}; " +
            $"merchantQuantity={MerchantStockQuantity}; route=custom-call-item-merchant-stock-one-session-companion; hotkeyPrimary=false.");
    }

    protected virtual void Update()
    {
        CustomCreatureCallSpellFeature.Tick(Logger);
        TickCompanion();
    }

    protected virtual void OnDestroy()
    {
        try
        {
            DismissCompanion("plugin shutdown");
            CustomCreatureCallSpellFeature.Release();
            CreatureCallItemIconPatch.Release();
            ReleaseVisualReferences();
            ReleaseRetainedDeathVisualReferences();
            _harmony?.UnpatchSelf();
        }
        finally
        {
            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
            }
        }
    }

    internal void TrySummonOrSwapFromSpell()
    {
        TrySummonOrSwap("custom-call-spell", requireEnabled: true);
    }

    private void BindConfig()
    {
        _enabled = Config.Bind(
            "General",
            "Enabled",
            true,
            "Master switch for this standalone Avalon Awakened creature companion mod.");
        _registerCallSpell = Config.Bind(
            "Spell",
            "RegisterCallSpell",
            true,
            $"Register the mod-owned {Definition.SpellDisplayName} item cloned from Wolf's Call.");
        _enableCallCompanionRoute = Config.Bind(
            "Spell",
            "EnableCompanionCastRoute",
            true,
            $"Intercept {Definition.SpellDisplayName} casts and use the guarded one-session companion route instead of native SkillSpawnLocation.");
        _grantCallOnLoad = Config.Bind(
            "Spell",
            "GrantCallOnLoad",
            false,
            $"Grant one {Definition.SpellDisplayName} item to the current hero when templates and inventory are ready. Save-visible; keep disabled unless testing on a throwaway save.");
        _merchantStockEnabled = Config.Bind(
            "MerchantStock",
            "Enabled",
            true,
            $"Add {Definition.SpellDisplayName} to the configured merchant using the Broodmother-proven ShopUI stock route.");
        _merchantStockTargetShopGuid = Config.Bind(
            "MerchantStock",
            "TargetShopGuid",
            KnownMerchantShopGuid,
            "Exact shop GUID that receives the call item. Empty means any opened shop with decompressed restockable stock.");
        _merchantStockQuantity = Config.Bind(
            "MerchantStock",
            "Quantity",
            1,
            "Number of call items added to the merchant stock, clamped to 1..99.");
        _summonDistance = Config.Bind(
            "Companion",
            "SummonDistance",
            3.0f,
            "Meters in front of the hero used for the one-session companion spawn and catch-up recall.");
        _summonRightOffset = Config.Bind(
            "Companion",
            "SummonRightOffset",
            1.0f,
            "Meters to the hero's right used for the one-session companion spawn and catch-up recall.");
        _followCatchUpDistance = Config.Bind(
            "Companion",
            "FollowCatchUpDistance",
            28.0f,
            "If the active companion falls farther than this from the hero, the mod teleports it back to the summon offset.");
    }

    private void TrySummonOrSwap(string source, bool requireEnabled)
    {
        if (requireEnabled && !_enabled.Value)
        {
            Logger.LogWarning($"{PluginNameValue} summon blocked; source={source}; reason=general-enabled-false.");
            return;
        }

        Hero? hero = Hero.Current;
        if (hero == null)
        {
            Logger.LogWarning($"{PluginNameValue} summon blocked; source={source}; reason=hero-current-null.");
            return;
        }

        if (!TryResolveCompanionTemplate(out LocationTemplate? template, out string reason) || template == null)
        {
            Logger.LogWarning($"{PluginNameValue} summon blocked; source={source}; reason={reason}");
            return;
        }

        if (!TryApplyCompanionVisualTemplateSelection(template, out reason))
        {
            Logger.LogWarning($"{PluginNameValue} summon blocked; source={source}; reason={reason}");
            return;
        }

        if (HasActiveCompanion())
        {
            DismissCompanion("summon swap");
        }

        Vector3 spawnPosition = hero.Coords + hero.Rotation * GetCompanionOffset();
        Location location;
        try
        {
            location = template.SpawnLocation(
                spawnPosition,
                hero.Rotation,
                null,
                null,
                Definition.DisplayName,
                hero.ParentTransform.gameObject.scene);
            location.MarkedNotSaved = true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginNameValue} summon failed during {source}: {ex.GetType().Name}: {ex.Message}");
            return;
        }

        if (!TryValidateSpawnedLocation(location, out NpcElement? npcElement, out reason) || npcElement == null)
        {
            FailSpawnedLocation(location, "summon-or-swap", reason);
            return;
        }

        if (!TryApplyNativeAllyMarker(location, npcElement, hero, out reason))
        {
            FailSpawnedLocation(location, "summon-or-swap", reason);
            return;
        }

        ResetLifecycleMarkers();
        if (!TryMaintainLifecycleSurface(
                location,
                npcElement,
                "summon-or-swap",
                failIfControllerIncomplete: false,
                out reason))
        {
            FailSpawnedLocation(location, "summon-or-swap", reason);
            return;
        }

        _activeCompanion = location;
        _nextTickTime = Time.unscaledTime + TickIntervalSeconds;
        location.MarkedNotSaved = true;

        if (!TryEnsureNativeVisual(npcElement, out string visualReason) && !_overlayFailureLogged)
        {
            _overlayFailureLogged = true;
            Logger.LogWarning($"{PluginNameValue} native visual failed after {source}: {visualReason}");
        }

        Logger.LogInfo(
            $"{PluginNameValue} summoned {Definition.DisplayName}. " +
            $"source={source}; template={Definition.LocationTemplateName}[{Definition.LocationTemplateGuid}]; " +
            $"npcTemplate={Definition.NpcTemplateGuid}; visual={Definition.VisualAddress}; " +
            $"templateVisual={Definition.TemplateVisualAddress}; runtimeOverlay={Definition.RuntimeOverlayVisualAddress ?? "none"}; " +
            $"position={FormatVector(spawnPosition)}; notSaved=1; ally=1; merchantCallRoute=1.");
    }

    private bool TryResolveCompanionTemplate([NotNullWhen(true)] out LocationTemplate? template, out string reason)
    {
        template = null;

        Type? apiType = Type.GetType(ResolverTypeName, throwOnError: false);
        if (apiType == null)
        {
            reason = "Avalon Awakened resolver type was not found.";
            return false;
        }

        MethodInfo? resolver = apiType.GetMethod(Definition.ResolverMethodName, BindingFlags.Public | BindingFlags.Static);
        if (resolver == null)
        {
            reason = $"Avalon Awakened {Definition.DisplayName} resolver method was not found.";
            return false;
        }

        ParameterInfo[] parameters = resolver.GetParameters();
        if (parameters.Length != 2 ||
            !parameters[0].ParameterType.IsByRef ||
            !parameters[1].ParameterType.IsByRef)
        {
            reason = $"Avalon Awakened {Definition.DisplayName} resolver signature changed.";
            return false;
        }

        object?[] args = { null, string.Empty };
        try
        {
            object? invokeResult = resolver.Invoke(null, args);
            if (invokeResult is not bool resolved || !resolved)
            {
                reason = args[1] as string ?? $"Avalon Awakened {Definition.DisplayName} resolver returned false.";
                return false;
            }
        }
        catch (TargetInvocationException ex)
        {
            Exception inner = ex.InnerException ?? ex;
            reason = $"Avalon Awakened {Definition.DisplayName} resolver threw {inner.GetType().Name}: {inner.Message}";
            return false;
        }
        catch (Exception ex)
        {
            reason = $"Avalon Awakened {Definition.DisplayName} resolver failed: {ex.GetType().Name}: {ex.Message}";
            return false;
        }

        template = args[0] as LocationTemplate;
        if (template == null)
        {
            reason = $"Avalon Awakened {Definition.DisplayName} resolver returned null template.";
            return false;
        }

        return ValidateResolvedTemplate(template, out reason);
    }

    private bool ValidateResolvedTemplate(LocationTemplate template, out string reason)
    {
        if (!string.Equals(template.name, Definition.LocationTemplateName, StringComparison.Ordinal))
        {
            reason = $"{Definition.DisplayName} LocationTemplate name changed.";
            return false;
        }

        if (!string.Equals(template.GUID, Definition.LocationTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = $"{Definition.DisplayName} LocationTemplate GUID changed.";
            return false;
        }

        NpcAttachment? npcAttachment = template.GetComponent<NpcAttachment>();
        if (npcAttachment == null)
        {
            reason = $"{Definition.DisplayName} LocationTemplate has no NpcAttachment.";
            return false;
        }

        if (npcAttachment.IsUnique)
        {
            reason = $"{Definition.DisplayName} LocationTemplate is unique and cannot be used for one-session companion spawning.";
            return false;
        }

        RepetitiveNpcAttachment? repetitiveAttachment = template.GetComponent<RepetitiveNpcAttachment>();
        if (repetitiveAttachment == null || repetitiveAttachment.NpcTemplate == null)
        {
            reason = $"{Definition.DisplayName} LocationTemplate has no repetitive NPC template.";
            return false;
        }

        if (!string.Equals(repetitiveAttachment.NpcTemplate.GUID, Definition.NpcTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = $"{Definition.DisplayName} linked NpcTemplate GUID changed.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private bool TryApplyCompanionVisualTemplateSelection(LocationTemplate template, out string reason)
    {
        RepetitiveNpcAttachment? repetitiveAttachment = template.GetComponent<RepetitiveNpcAttachment>();
        if (repetitiveAttachment == null)
        {
            reason = $"{Definition.DisplayName} LocationTemplate has no RepetitiveNpcAttachment.";
            return false;
        }

        NpcTemplate npcTemplate = repetitiveAttachment.NpcTemplate;
        if (npcTemplate == null ||
            !string.Equals(npcTemplate.GUID, Definition.NpcTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = $"{Definition.DisplayName} NpcTemplate identity changed before companion spawn.";
            return false;
        }

        repetitiveAttachment.Setup(npcTemplate, new ARAssetReference(Definition.TemplateVisualAddress));
        ARAssetReference visual = repetitiveAttachment.VisualPrefab();
        if (visual == null ||
            !string.Equals(visual.Address, Definition.TemplateVisualAddress, StringComparison.Ordinal))
        {
            reason = $"{Definition.DisplayName} visual address did not read back before companion spawn.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private bool TryValidateSpawnedLocation(
        Location location,
        [NotNullWhen(true)] out NpcElement? npcElement,
        out string reason)
    {
        npcElement = null;
        location.MarkedNotSaved = true;

        if (!string.Equals(location.Template?.name, Definition.LocationTemplateName, StringComparison.Ordinal) ||
            !string.Equals(location.Template?.GUID, Definition.LocationTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = "spawned location template identity changed.";
            return false;
        }

        if (!string.Equals(location.DisplayName, Definition.DisplayName, StringComparison.Ordinal))
        {
            reason = $"spawned display name changed (actual={location.DisplayName}).";
            return false;
        }

        if (!location.TryGetElement(out NpcElement npc) || npc == null)
        {
            reason = "spawned location has no NpcElement.";
            return false;
        }

        if (npc.IsUnique)
        {
            reason = "spawned NpcElement is unique.";
            return false;
        }

        if (!string.Equals(npc.Template?.GUID, Definition.NpcTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            reason = "spawned NpcTemplate GUID changed.";
            return false;
        }

        npcElement = npc;
        reason = string.Empty;
        return true;
    }

    private bool TryApplyNativeAllyMarker(Location location, NpcElement npcElement, Hero hero, out string reason)
    {
        try
        {
            location.MarkedNotSaved = true;
            npcElement.OverrideFaction(hero.GetFactionTemplateForSummon(), FactionOverrideContext.Summon);
            if (!npcElement.HasElement<NpcHeroPetAlly>())
            {
                npcElement.AddElement(new NpcHeroPetAlly(hero));
            }

            location.MarkedNotSaved = true;
            NpcHeroPetAlly marker = npcElement.TryGetElement<NpcHeroPetAlly>();
            if (marker == null || marker.HasBeenDiscarded)
            {
                reason = "native ally marker readback failed.";
                return false;
            }

            reason = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            reason = $"native ally marker setup failed: {ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }

    private void TickCompanion()
    {
        if (Time.unscaledTime < _nextTickTime)
        {
            return;
        }

        _nextTickTime = Time.unscaledTime + TickIntervalSeconds;
        if (!TryGetActiveCompanion(out Location? location, out NpcElement? npcElement))
        {
            return;
        }

        if (!TryEnsureNativeVisual(npcElement, out string visualReason) && !_overlayFailureLogged)
        {
            _overlayFailureLogged = true;
            Logger.LogWarning($"{PluginNameValue} native visual failed during tick: {visualReason}");
        }

        if (!TryMaintainLifecycleSurface(
                location,
                npcElement,
                "tick",
                failIfControllerIncomplete: false,
                out string lifecycleReason))
        {
            Logger.LogWarning($"{PluginNameValue} lifecycle surface failed during tick: {lifecycleReason}");
        }

        location.MarkedNotSaved = true;
        Hero? hero = Hero.Current;
        if (hero == null)
        {
            return;
        }

        float distance = Vector3.Distance(location.Coords, hero.Coords);
        if (distance > Mathf.Max(8f, _followCatchUpDistance.Value))
        {
            RecallCompanion("follow catch-up");
        }
    }

    private bool TryGetActiveCompanion([NotNullWhen(true)] out Location? location, [NotNullWhen(true)] out NpcElement? npcElement)
    {
        location = null;
        npcElement = null;
        if (_activeCompanion == null)
        {
            return false;
        }

        if (_activeCompanion.HasBeenDiscarded)
        {
            ClearActiveCompanion();
            return false;
        }

        if (!_activeCompanion.TryGetElement(out NpcElement npc) || npc == null || npc.HasBeenDiscarded)
        {
            HandleNativeDeathPending(_activeCompanion, "npc-element-missing-or-discarded");
            return false;
        }

        if (!npc.IsAlive)
        {
            HandleNativeDeathPending(_activeCompanion, "npc-not-alive");
            return false;
        }

        NpcHeroPetAlly marker = npc.TryGetElement<NpcHeroPetAlly>();
        if (marker == null || marker.HasBeenDiscarded)
        {
            ClearActiveCompanion();
            return false;
        }

        _activeCompanion.MarkedNotSaved = true;
        location = _activeCompanion;
        npcElement = npc;
        return true;
    }

    private void HandleNativeDeathPending(Location location, string source)
    {
        if (location == null || location.HasBeenDiscarded)
        {
            ClearActiveCompanion();
            return;
        }

        location.MarkedNotSaved = true;
        if (TryAcceptNativeDeathHandoff(location, source))
        {
            ClearActiveCompanion(retainDeathVisual: true);
            return;
        }

        if (!_nativeDeathPendingLogged)
        {
            _nativeDeathPendingLogged = true;
            Logger.LogWarning(
                $"{Definition.Id.ToUpperInvariant()}_COMPANION_NATIVE_DEATH_PENDING " +
                $"source={source}; location={location.ID ?? string.Empty}; waitingForNativeDummyCorpse=1; activeLocationRetained=1; persistence=false.");
        }
    }

    private bool TryAcceptNativeDeathHandoff(Location location, string source)
    {
        if (!HasAcceptedNativeDeathTransition(location, out NpcDummy? dummy) || dummy == null)
        {
            return false;
        }

        if (!_nativeDeathAcceptedLogged)
        {
            _nativeDeathAcceptedLogged = true;
            Logger.LogWarning(
                $"{Definition.Id.ToUpperInvariant()}_COMPANION_NATIVE_DEATH_ACCEPTED " +
                $"source={source}; location={location.ID ?? string.Empty}; npcTemplate={dummy.Template?.GUID ?? "unknown"}; " +
                $"npcDummy=true; corpse=true; dummyHasDied={BoolText(dummy.HasDied)}; markedNotSaved={BoolText(location.MarkedNotSaved)}; " +
                $"isNotSaved={BoolText(location.IsNotSaved)}; persistence=false.");
        }

        return true;
    }

    private bool HasAcceptedNativeDeathTransition(Location location, [NotNullWhen(true)] out NpcDummy? dummy)
    {
        dummy = null;
        if (location == null || location.HasBeenDiscarded)
        {
            return false;
        }

        if (!location.TryGetElement(out NpcDummy candidate) || candidate == null ||
            !location.TryGetElement(out Corpse corpse) || corpse == null ||
            candidate.HasDied)
        {
            return false;
        }

        string dummyTemplateGuid = candidate.Template?.GUID ?? string.Empty;
        if (!string.Equals(dummyTemplateGuid, Definition.NpcTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        dummy = candidate;
        return true;
    }

    private bool HasActiveCompanion()
    {
        return TryGetActiveCompanion(out _, out _);
    }

    private void RecallCompanion(string source)
    {
        if (!TryGetActiveCompanion(out Location? location, out _))
        {
            return;
        }

        Hero? hero = Hero.Current;
        if (hero == null)
        {
            return;
        }

        Vector3 target = hero.Coords + hero.Rotation * GetCompanionOffset();
        try
        {
            location.MarkedNotSaved = true;
            location.MoveAndRotateTo(target, hero.Rotation, teleport: true);
            location.MarkedNotSaved = true;
            Logger.LogInfo($"{PluginNameValue} recalled {Definition.DisplayName}; source={source}; target={FormatVector(target)}.");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginNameValue} recall failed during {source}: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void DismissCompanion(string source)
    {
        Location? location = _activeCompanion;
        try
        {
            if (location != null && !location.HasBeenDiscarded)
            {
                ReleaseRuntimeHitSurface(location);
                ReleaseVisualReferences();
                location.MarkedNotSaved = true;
                location.Discard();
                Logger.LogInfo($"{PluginNameValue} dismissed {Definition.DisplayName}; source={source}.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginNameValue} dismiss failed during {source}: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            ClearActiveCompanion();
        }
    }

    private void FailSpawnedLocation(Location location, string command, string reason)
    {
        try
        {
            ReleaseRuntimeHitSurface(location);
            ReleaseVisualReferences();
            location.MarkedNotSaved = true;
            location.Discard();
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginNameValue} failed cleanup after {command}: {ex.GetType().Name}: {ex.Message}");
        }

        Logger.LogWarning($"{PluginNameValue} summon discarded; command={command}; reason={reason}");
    }

    private void ClearActiveCompanion(bool retainDeathVisual = false)
    {
        ReleaseRuntimeHitSurface(_activeCompanion);
        if (retainDeathVisual)
        {
            RetainDeathVisualReferences();
        }
        else
        {
            ReleaseVisualReferences();
        }

        _activeCompanion = null;
        ResetLifecycleMarkers();
    }

    private void ResetLifecycleMarkers()
    {
        _nativeVisualReadyLogged = false;
        _lifecycleReadyLogged = false;
        _lifecycleWaitingLogged = false;
        _nativeDeathPendingLogged = false;
        _animatedDeathVisibilityLogged = false;
        _overlayFailureLogged = false;
    }

    private Vector3 GetCompanionOffset()
    {
        return (Vector3.forward * _summonDistance.Value) + (Vector3.right * _summonRightOffset.Value);
    }

    private bool TryMaintainLifecycleSurface(
        Location location,
        NpcElement npcElement,
        string source,
        bool failIfControllerIncomplete,
        out string reason)
    {
        reason = string.Empty;
        try
        {
            if (location == null || location.HasBeenDiscarded)
            {
                reason = "location-unavailable";
                return false;
            }

            if (npcElement == null || npcElement.HasBeenDiscarded)
            {
                reason = "npc-element-unavailable";
                return false;
            }

            if (!npcElement.IsAlive)
            {
                if (TryAcceptNativeDeathHandoff(location, "npc-not-alive:" + source))
                {
                    reason = "native-death-accepted";
                    return true;
                }

                reason = "npc-not-alive-before-native-death-handoff";
                return false;
            }

            location.MarkedNotSaved = true;
            npcElement.KeepCorpseAfterDeath = true;
            if (!npcElement.KeepCorpseAfterDeath)
            {
                reason = "native same-session corpse-retention flag did not read back true.";
                return false;
            }

            if (!npcElement.TryGetElement(out DeathElement death) || death == null)
            {
                reason = "native DeathElement missing.";
                return false;
            }

            bool deathKeepBody = death.KeepBody;
            bool hideHealthBarRemoved = RemoveHiddenHealthBar(location);
            bool summonInvisibilityRemoved = RemoveSummonInvisibility(npcElement);

            if (!TryEnsureHitSurface(
                    location,
                    npcElement,
                    death,
                    failIfControllerIncomplete,
                    out bool controllerReady,
                    out int colliders,
                    out int enabledColliders,
                    out int colliderChanges,
                    out int healthHitboxes,
                    out int healthHitboxAdds,
                    out int hitboxLayerChanges,
                    out int hitboxLayer,
                    out bool animatedDeathReady,
                    out int deathBehaviours,
                    out int initializedDeathBehaviours,
                    out bool deathBehaviourLinked,
                    out bool animatedDeathVisibilityReady,
                    out bool alivePrefabSuppressionCleared,
                    out string hitSurfaceReason))
            {
                reason = hitSurfaceReason;
                return false;
            }

            bool lifecycleReady = controllerReady && animatedDeathReady;
            if (lifecycleReady && !_lifecycleReadyLogged)
            {
                _lifecycleReadyLogged = true;
                _lifecycleWaitingLogged = false;
                Logger.LogInfo(
                    $"{Definition.Id.ToUpperInvariant()}_COMPANION_LIFECYCLE_READY " +
                    $"source={source}; template={Definition.LocationTemplateName}[{Definition.LocationTemplateGuid}]; npcTemplate={Definition.NpcTemplateGuid}; " +
                    $"keepCorpse={BoolText(npcElement.KeepCorpseAfterDeath)}; deathElement=1; deathKeepBody={BoolText(deathKeepBody)}; " +
                    $"animatedDeath={BoolText(animatedDeathReady)}; deathBehaviours={deathBehaviours}; initializedDeathBehaviours={initializedDeathBehaviours}; " +
                    $"deathBehaviourLinked={BoolText(deathBehaviourLinked)}; animatedDeathVisibilityReady={BoolText(animatedDeathVisibilityReady)}; " +
                    $"alivePrefabSuppressionCleared={BoolText(alivePrefabSuppressionCleared)}; hideHealthBarRemoved={BoolText(hideHealthBarRemoved)}; " +
                    $"summonInvisibilityRemoved={BoolText(summonInvisibilityRemoved)}; colliders={colliders}; enabledColliders={enabledColliders}; " +
                    $"colliderChanges={colliderChanges}; healthHitboxes={healthHitboxes}; healthHitboxAdds={healthHitboxAdds}; " +
                    $"hitboxLayerChanges={hitboxLayerChanges}; hitboxLayer={(hitboxLayer >= 0 ? hitboxLayer.ToString() : "missing")}; " +
                    "nativeDeathOwner=DeathElement; persistence=false.");
            }
            else if (!lifecycleReady && !_lifecycleWaitingLogged)
            {
                _lifecycleWaitingLogged = true;
                Logger.LogInfo(
                    $"{Definition.Id.ToUpperInvariant()}_COMPANION_LIFECYCLE_WAITING " +
                    $"source={source}; reason={hitSurfaceReason}; keepCorpse={BoolText(npcElement.KeepCorpseAfterDeath)}; " +
                    $"deathElement=1; deathKeepBody={BoolText(deathKeepBody)}; animatedDeath={BoolText(animatedDeathReady)}; " +
                    $"hideHealthBarRemoved={BoolText(hideHealthBarRemoved)}; summonInvisibilityRemoved={BoolText(summonInvisibilityRemoved)}; persistence=false.");
            }

            reason = lifecycleReady ? "lifecycle-ready" : hitSurfaceReason;
            return true;
        }
        catch (Exception ex)
        {
            reason = $"lifecycle setup failed: {ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }

    private static bool RemoveHiddenHealthBar(Location location)
    {
        if (!location.HasElement<HideHealthBar>())
        {
            return false;
        }

        location.RemoveElementsOfType<HideHealthBar>();
        location.MarkedNotSaved = true;
        return true;
    }

    private static bool RemoveSummonInvisibility(NpcElement npcElement)
    {
        HeroSummonInvisibility invisibility = npcElement.TryGetElement<HeroSummonInvisibility>();
        if (invisibility == null || invisibility.HasBeenDiscarded)
        {
            return false;
        }

        invisibility.Discard();
        return true;
    }

    private bool TryEnsureHitSurface(
        Location location,
        NpcElement npcElement,
        DeathElement death,
        bool failIfControllerIncomplete,
        out bool controllerReady,
        out int colliders,
        out int enabledColliders,
        out int colliderChanges,
        out int healthHitboxes,
        out int healthHitboxAdds,
        out int hitboxLayerChanges,
        out int hitboxLayer,
        out bool animatedDeathReady,
        out int deathBehaviours,
        out int initializedDeathBehaviours,
        out bool deathBehaviourLinked,
        out bool animatedDeathVisibilityReady,
        out bool alivePrefabSuppressionCleared,
        out string reason)
    {
        controllerReady = false;
        colliders = 0;
        enabledColliders = 0;
        colliderChanges = 0;
        healthHitboxes = 0;
        healthHitboxAdds = 0;
        hitboxLayerChanges = 0;
        hitboxLayer = LayerMask.NameToLayer("Hitboxes");
        animatedDeathReady = false;
        deathBehaviours = 0;
        initializedDeathBehaviours = 0;
        deathBehaviourLinked = false;
        animatedDeathVisibilityReady = false;
        alivePrefabSuppressionCleared = false;

        if (npcElement.Controller == null)
        {
            reason = "waiting-for-npc-controller";
            return !failIfControllerIncomplete;
        }

        GameObject root = npcElement.Controller.AlivePrefab != null
            ? npcElement.Controller.AlivePrefab
            : npcElement.Controller.gameObject;
        if (root == null)
        {
            reason = "npc-controller-alive-prefab-unavailable";
            return !failIfControllerIncomplete;
        }

        controllerReady = true;
        HealthElement healthElement = npcElement.HealthElement;
        if (healthElement == null || healthElement.HasBeenDiscarded)
        {
            reason = $"{Definition.DisplayName} native actor HealthElement unavailable for runtime hitbox registration.";
            return false;
        }

        Collider[] actorColliders = root.GetComponentsInChildren<Collider>(true);
        colliders = actorColliders.Length;
        foreach (Collider collider in actorColliders)
        {
            if (collider == null)
            {
                continue;
            }

            if (!collider.enabled)
            {
                collider.enabled = true;
                colliderChanges++;
            }

            if (collider.enabled)
            {
                enabledColliders++;
                EnsureRuntimeHealthHitbox(
                    location,
                    healthElement,
                    collider,
                    hitboxLayer,
                    ref healthHitboxes,
                    ref healthHitboxAdds,
                    ref hitboxLayerChanges);
            }
        }

        PlayAnimationDeathBehaviour[] visualDeathBehaviours =
            root.GetComponentsInChildren<PlayAnimationDeathBehaviour>(true);
        deathBehaviours = visualDeathBehaviours.Length;
        PlayAnimationDeathBehaviour? animationDeath = death.GetBehaviour<PlayAnimationDeathBehaviour>();
        for (int i = 0; i < visualDeathBehaviours.Length; i++)
        {
            PlayAnimationDeathBehaviour candidate = visualDeathBehaviours[i];
            if (candidate != null && candidate.IsVisualInitialized)
            {
                initializedDeathBehaviours++;
            }

            if (animationDeath != null && ReferenceEquals(candidate, animationDeath))
            {
                deathBehaviourLinked = true;
            }
        }

        animatedDeathReady = animationDeath != null &&
            deathBehaviourLinked &&
            animationDeath.IsVisualInitialized &&
            death.KeepBody;

        if (colliders <= 0)
        {
            reason = $"{Definition.DisplayName} native actor has no colliders under AlivePrefab.";
            return false;
        }

        if (enabledColliders <= 0)
        {
            reason = $"{Definition.DisplayName} native actor colliders remained disabled after lifecycle surface reassertion.";
            return false;
        }

        if (healthHitboxes <= 0)
        {
            reason = $"{Definition.DisplayName} native actor colliders could not be registered with HealthElement hitboxes.";
            return false;
        }

        if (!animatedDeathReady)
        {
            reason = "waiting-for-native-animated-death-behaviour";
            return !failIfControllerIncomplete;
        }

        if (!TryCorrectAnimatedDeathVisibility(
                death,
                animationDeath!,
                out alivePrefabSuppressionCleared,
                out reason))
        {
            return false;
        }

        animatedDeathVisibilityReady = true;
        reason = "hit-surface-ready";
        return true;
    }

    private bool TryCorrectAnimatedDeathVisibility(
        DeathElement death,
        PlayAnimationDeathBehaviour initializedBehaviour,
        out bool alivePrefabSuppressionCleared,
        out string reason)
    {
        alivePrefabSuppressionCleared = false;
        if (death == null || death.HasBeenDiscarded)
        {
            reason = $"{Definition.DisplayName} native DeathElement became unavailable before death visibility correction.";
            return false;
        }

        PlayAnimationDeathBehaviour? animationDeath = death.GetBehaviour<PlayAnimationDeathBehaviour>();
        if (animationDeath == null ||
            !ReferenceEquals(animationDeath, initializedBehaviour) ||
            !animationDeath.IsVisualInitialized)
        {
            reason = $"{Definition.DisplayName} native animated-death behavior is missing, mismatched, or not visually initialized.";
            return false;
        }

        if (!death.KeepBody)
        {
            reason = $"{Definition.DisplayName} native CustomDeathController no longer retains its body.";
            return false;
        }

        FieldInfo? alivePrefabField = PlayAnimationAlivePrefabField;
        if (alivePrefabField == null || alivePrefabField.FieldType != typeof(GameObject))
        {
            reason = "the reviewed PlayAnimationDeathBehaviour AlivePrefab suppression field changed.";
            return false;
        }

        bool suppressionTargetWasPresent = alivePrefabField.GetValue(animationDeath) is GameObject;
        alivePrefabField.SetValue(animationDeath, null);
        if (alivePrefabField.GetValue(animationDeath) != null)
        {
            reason = "the animated-death AlivePrefab suppression target did not clear.";
            return false;
        }

        alivePrefabSuppressionCleared = true;
        if (!_animatedDeathVisibilityLogged)
        {
            _animatedDeathVisibilityLogged = true;
            Logger.LogWarning(
                $"{Definition.Id.ToUpperInvariant()}_COMPANION_ANIMATED_DEATH_VISIBILITY_READY " +
                $"suppressionTargetWasPresent={BoolText(suppressionTargetWasPresent)}; alivePrefabSuppressionCleared=1; " +
                "keepBody=true; ragdoll=false; persistence=false.");
        }

        reason = string.Empty;
        return true;
    }

    private void EnsureRuntimeHealthHitbox(
        Location location,
        HealthElement healthElement,
        Collider collider,
        int hitboxLayer,
        ref int healthHitboxes,
        ref int healthHitboxAdds,
        ref int hitboxLayerChanges)
    {
        if (location == null || location.HasBeenDiscarded || collider == null || !collider.enabled)
        {
            return;
        }

        GameObject hitboxObject = collider.gameObject;
        if (hitboxLayer >= 0 && hitboxObject != null && hitboxObject.layer != hitboxLayer)
        {
            if (!_runtimeHitboxLayers.ContainsKey(collider))
            {
                _runtimeHitboxLayers.Add(collider, hitboxObject.layer);
            }

            hitboxObject.layer = hitboxLayer;
            hitboxLayerChanges++;
        }

        if (!_runtimeHealthHitboxes.Contains(collider))
        {
            HitboxData hitboxData = HitboxData.Default;
            hitboxData.ValidateAndCorrectCanBeHit();
            healthElement.AddHitbox(collider, in hitboxData);
            _runtimeHealthHitboxes.Add(collider);
            healthHitboxAdds++;
        }

        if (healthElement.HasHitbox(collider))
        {
            healthHitboxes++;
        }
    }

    private void ReleaseRuntimeHitSurface(Location? location)
    {
        try
        {
            HealthElement? healthElement = null;
            if (location != null &&
                !location.HasBeenDiscarded &&
                location.TryGetElement(out NpcElement npcElement) &&
                npcElement != null &&
                !npcElement.HasBeenDiscarded)
            {
                healthElement = npcElement.HealthElement;
            }

            if (healthElement != null && !healthElement.HasBeenDiscarded)
            {
                foreach (Collider collider in _runtimeHealthHitboxes)
                {
                    if (collider != null)
                    {
                        healthElement.RemoveHitbox(collider);
                    }
                }
            }

            foreach (KeyValuePair<Collider, int> entry in _runtimeHitboxLayers)
            {
                Collider collider = entry.Key;
                if (collider != null && collider.gameObject != null)
                {
                    collider.gameObject.layer = entry.Value;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginNameValue} runtime hit surface cleanup failed: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            _runtimeHealthHitboxes.Clear();
            _runtimeHitboxLayers.Clear();
        }
    }

    private bool TryEnsureNativeVisual(NpcElement npcElement, out string reason)
    {
        if (!string.IsNullOrWhiteSpace(Definition.RuntimeOverlayVisualAddress))
        {
            return TryEnsureRuntimeOverlay(npcElement, out reason);
        }

        if (npcElement.Controller == null)
        {
            reason = "waiting-for-npc-controller";
            return true;
        }

        if (_overlayInstance != null)
        {
            UnityEngine.Object.Destroy(_overlayInstance);
            _overlayInstance = null;
        }

        Renderer[] renderers = npcElement.Controller.gameObject.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length <= 0)
        {
            reason = $"{Definition.DisplayName} native visual has no Unity renderers under NpcController.";
            return false;
        }

        Transform scaleRoot = ResolveNativeVisualScaleRoot(npcElement);
        CaptureNativeVisualBaseScale(scaleRoot);
        ApplyNativeVisualScale(scaleRoot);

        if (!_overlayReady && !TryApplyRuntimeMaterials(renderers, out reason))
        {
            return false;
        }

        _overlayReady = true;
        ReassertNativeVisual(npcElement);
        int enabledRenderers = CountEnabledRenderers(renderers);
        if (enabledRenderers <= 0)
        {
            reason = $"{Definition.DisplayName} native visual renderers remained disabled.";
            return false;
        }

        if (!_nativeVisualReadyLogged)
        {
            _nativeVisualReadyLogged = true;
            Logger.LogInfo(
                $"{Definition.Id.ToUpperInvariant()}_COMPANION_NATIVE_VISUAL_READY " +
                $"visual={Definition.VisualAddress}; renderers={renderers.Length}; enabledRenderers={enabledRenderers}; " +
                $"runtimeMaterials={_runtimeMaterialCount}; materialSlots={_materialSlotCount}; sourceShaders={_sourceShaders}; targetShaders={_targetShaders}; " +
                $"baseTextureSources={_baseTextureSources}; normalTextureSources={_normalTextureSources}; " +
                $"scaleRoot={scaleRoot.name}; scaleMultiplier={Definition.VisualScaleMultiplier:0.###}; overlay=false; nativeRenderersHidden=false; saveWrite=false.");
        }

        reason = "ready";
        return true;
    }

    private bool TryEnsureRuntimeOverlay(NpcElement npcElement, out string reason)
    {
        if (npcElement.Controller == null)
        {
            reason = "waiting-for-npc-controller";
            return true;
        }

        if (_overlayReady && _overlayInstance != null)
        {
            ReassertRuntimeOverlayVisibility(npcElement);
            reason = "ready";
            return true;
        }

        string overlayAddress = Definition.RuntimeOverlayVisualAddress ?? string.Empty;
        if (string.IsNullOrWhiteSpace(overlayAddress))
        {
            reason = "runtime-overlay-address-missing";
            return false;
        }

        if (!_overlayRequested)
        {
            _overlayRequested = true;
            _overlayHandle = Addressables.LoadAssetAsync<GameObject>(overlayAddress);
            reason = "waiting-for-runtime-overlay-load";
            return true;
        }

        if (!_overlayHandle.IsDone)
        {
            reason = "waiting-for-runtime-overlay-load";
            return true;
        }

        if (_overlayHandle.Status != AsyncOperationStatus.Succeeded ||
            _overlayHandle.Result == null)
        {
            reason = $"{Definition.DisplayName} runtime overlay visual failed to load from Addressables.";
            return false;
        }

        Transform parent = npcElement.Controller.AlivePrefab != null
            ? npcElement.Controller.AlivePrefab.transform
            : npcElement.Controller.transform;
        _overlayInstance = UnityEngine.Object.Instantiate(_overlayHandle.Result, parent, false);
        _overlayInstance.name = PluginNameValue + "." + Definition.Id + ".RuntimeOverlay";
        _overlayInstance.transform.localPosition = Vector3.zero;
        _overlayInstance.transform.localRotation = Quaternion.identity;
        _overlayInstance.transform.localScale = Vector3.one;

        Renderer[] overlayRenderers = _overlayInstance.GetComponentsInChildren<Renderer>(true);
        if (overlayRenderers.Length <= 0)
        {
            reason = $"{Definition.DisplayName} runtime overlay has no Unity renderers.";
            ReleaseVisualReferences();
            return false;
        }

        if (!TryApplyRuntimeMaterials(overlayRenderers, out reason))
        {
            ReleaseVisualReferences();
            return false;
        }

        CaptureRuntimeOverlaySurfaces(_overlayInstance);
        StripRuntimeOverlayMountedProps(_overlayInstance);
        _overlayInstance.SetActive(true);
        _overlayReady = true;
        ReassertRuntimeOverlayVisibility(npcElement);

        Logger.LogInfo(
            $"{Definition.Id.ToUpperInvariant()}_COMPANION_RUNTIME_OVERLAY_READY " +
            $"template={Definition.LocationTemplateName}[{Definition.LocationTemplateGuid}]; npcTemplate={Definition.NpcTemplateGuid}; " +
            $"templateVisual={Definition.TemplateVisualAddress}; overlayVisual={overlayAddress}; overlayRenderers={_runtimeOverlayRenderers.Count}; " +
            $"strippedOverlayRenderers={_runtimeOverlayStrippedRenderers.Count}; overlayKandraRenderers={_runtimeOverlayKandraRenderers.Count}; " +
            $"strippedOverlayKandraRenderers={_runtimeOverlayStrippedKandraRenderers.Count}; runtimeMaterials={_runtimeMaterialCount}; " +
            $"materialSlots={_materialSlotCount}; sourceShaders={_sourceShaders}; targetShaders={_targetShaders}; " +
            "nativeBootstrapRenderersHidden=true; sourceMaterialMutation=false; saveWrite=false.");

        reason = "ready";
        return true;
    }

    private void CaptureRuntimeOverlaySurfaces(GameObject overlayInstance)
    {
        _runtimeOverlayRenderers.Clear();
        _runtimeOverlayStrippedRenderers.Clear();
        _runtimeOverlayKandraRenderers.Clear();
        _runtimeOverlayStrippedKandraRenderers.Clear();

        foreach (Renderer renderer in overlayInstance.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer != null)
            {
                _runtimeOverlayRenderers.Add(renderer);
            }
        }

        foreach (Component component in overlayInstance.GetComponentsInChildren<Component>(true))
        {
            if (IsKandraRendererComponent(component))
            {
                _runtimeOverlayKandraRenderers.Add(component);
            }
        }
    }

    private void StripRuntimeOverlayMountedProps(GameObject overlayInstance)
    {
        Transform overlayRoot = overlayInstance.transform;
        foreach (Renderer renderer in overlayInstance.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null || !IsMountedProp(renderer.transform, overlayRoot))
            {
                continue;
            }

            renderer.enabled = false;
            _runtimeOverlayStrippedRenderers.Add(renderer);
        }

        foreach (Component component in overlayInstance.GetComponentsInChildren<Component>(true))
        {
            if (!IsKandraRendererComponent(component) || !IsMountedProp(component.transform, overlayRoot))
            {
                continue;
            }

            SetKandraRendererEnabled(component, false);
            _runtimeOverlayStrippedKandraRenderers.Add(component);
        }
    }

    private void ReassertRuntimeOverlayVisibility(NpcElement npcElement)
    {
        if (npcElement.Controller == null)
        {
            return;
        }

        foreach (Renderer renderer in npcElement.Controller.gameObject.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null)
            {
                continue;
            }

            renderer.enabled =
                _runtimeOverlayRenderers.Contains(renderer) &&
                !_runtimeOverlayStrippedRenderers.Contains(renderer);
            if (renderer is SkinnedMeshRenderer skinned)
            {
                skinned.updateWhenOffscreen = true;
            }
        }

        foreach (Component component in npcElement.Controller.gameObject.GetComponentsInChildren<Component>(true))
        {
            if (!IsKandraRendererComponent(component))
            {
                continue;
            }

            SetKandraRendererEnabled(
                component,
                _runtimeOverlayKandraRenderers.Contains(component) &&
                !_runtimeOverlayStrippedKandraRenderers.Contains(component));
        }
    }

    private void ReassertNativeVisual(NpcElement npcElement)
    {
        if (npcElement.Controller == null)
        {
            return;
        }

        if (_nativeVisualScaleRoot != null)
        {
            ApplyNativeVisualScale(_nativeVisualScaleRoot);
        }

        foreach (Renderer renderer in npcElement.Controller.gameObject.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null)
            {
                continue;
            }

            renderer.enabled = true;
            if (renderer is SkinnedMeshRenderer skinned)
            {
                skinned.updateWhenOffscreen = true;
            }
        }
    }

    private bool TryApplyRuntimeMaterials(Renderer[] renderers, out string reason)
    {
        ReleaseRuntimeMaterials();
        _runtimeMaterialCount = 0;
        _materialSlotCount = 0;
        _sourceShaders = "none";
        _targetShaders = "none";
        _baseTextureSources = "none";
        _normalTextureSources = "none";

        Shader? hdrpLit = Shader.Find("HDRP/Lit");
        Dictionary<Material, Material> convertedBySource = new();
        List<string> sourceShaders = new();
        List<string> targetShaders = new();
        List<string> baseTextureSources = new();
        List<string> normalTextureSources = new();

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                _materialSlotCount++;
                Material source = materials[i];
                if (source == null || source.shader == null)
                {
                    reason = $"{Definition.DisplayName} visual material setup failed because renderer {renderer.name} has a null material or shader at slot {i}.";
                    return false;
                }

                string sourceShaderName = source.shader.name ?? "unknown";
                if (!ContainsString(sourceShaders, sourceShaderName))
                {
                    sourceShaders.Add(sourceShaderName);
                }

                if (!convertedBySource.TryGetValue(source, out Material converted))
                {
                    if (!TryCreateRuntimeMaterial(
                            source,
                            hdrpLit,
                            out converted,
                            out string targetShaderName,
                            out string baseTextureSource,
                            out string normalTextureSource,
                            out reason))
                    {
                        return false;
                    }

                    if (!ContainsString(targetShaders, targetShaderName))
                    {
                        targetShaders.Add(targetShaderName);
                    }

                    if (!ContainsString(baseTextureSources, baseTextureSource))
                    {
                        baseTextureSources.Add(baseTextureSource);
                    }

                    if (!ContainsString(normalTextureSources, normalTextureSource))
                    {
                        normalTextureSources.Add(normalTextureSource);
                    }

                    convertedBySource[source] = converted;
                    _runtimeMaterials.Add(converted);
                }

                materials[i] = converted;
            }

            renderer.sharedMaterials = materials;
            renderer.enabled = true;
            if (renderer is SkinnedMeshRenderer skinned)
            {
                skinned.updateWhenOffscreen = true;
            }
        }

        _runtimeMaterialCount = convertedBySource.Count;
        _sourceShaders = sourceShaders.Count == 0 ? "none" : string.Join("|", sourceShaders);
        _targetShaders = targetShaders.Count == 0 ? "none" : string.Join("|", targetShaders);
        _baseTextureSources = baseTextureSources.Count == 0 ? "none" : string.Join("|", baseTextureSources);
        _normalTextureSources = normalTextureSources.Count == 0 ? "none" : string.Join("|", normalTextureSources);
        reason = string.Empty;
        return true;
    }

    private static bool TryCreateRuntimeMaterial(
        Material source,
        Shader? hdrpLit,
        out Material material,
        out string targetShaderName,
        out string baseTextureSource,
        out string normalTextureSource,
        out string reason)
    {
        if (hdrpLit != null && hdrpLit.isSupported)
        {
            material = CreateHdrpMaterial(source, hdrpLit, out baseTextureSource, out normalTextureSource);
            targetShaderName = "HDRP/Lit";
            reason = string.Empty;
            return true;
        }

        Shader sourceShader = source.shader;
        if (sourceShader != null && sourceShader.isSupported)
        {
            material = new Material(source)
            {
                name = source.name + "_AvalonCreatureCompanion_SourceShaderFallback",
            };
            targetShaderName = sourceShader.name ?? "source-shader";
            baseTextureSource = "source-shader-fallback";
            normalTextureSource = "source-shader-fallback";
            reason = string.Empty;
            return true;
        }

        material = null!;
        targetShaderName = "none";
        baseTextureSource = "none";
        normalTextureSource = "none";
        reason = "visual material setup failed because the source shader is unsupported and HDRP/Lit is unavailable.";
        return false;
    }

    private static Material CreateHdrpMaterial(
        Material source,
        Shader hdrpLit,
        out string baseTextureSource,
        out string normalTextureSource)
    {
        Material material = new(hdrpLit)
        {
            name = source.name + "_AvalonCreatureCompanion_HDRP",
            renderQueue = -1,
        };
        material.SetOverrideTag("RenderType", "Opaque");
        material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_MASKMAP");
        material.DisableKeyword("_METALLICSPECGLOSSMAP");

        Color baseColor = ReadColor(source, "_BaseColor", ReadColor(source, "_Color", Color.white));
        baseColor.a = 1f;
        SetColorIfPresent(material, "_BaseColor", baseColor);
        SetFloatIfPresent(material, "_Metallic", 0f);
        SetFloatIfPresent(material, "_MetallicRemapMin", 0f);
        SetFloatIfPresent(material, "_MetallicRemapMax", 0f);
        SetFloatIfPresent(material, "_Smoothness", 0.035f);
        SetFloatIfPresent(material, "_SmoothnessRemapMin", 0f);
        SetFloatIfPresent(material, "_SmoothnessRemapMax", 0.07f);
        SetFloatIfPresent(material, "_CoatMask", 0f);
        SetFloatIfPresent(material, "_CoatSmoothness", 0f);
        SetFloatIfPresent(material, "_AlphaCutoffEnable", 0f);
        SetFloatIfPresent(material, "_SurfaceType", 0f);
        SetFloatIfPresent(material, "_BlendMode", 0f);
        SetFloatIfPresent(material, "_SrcBlend", 1f);
        SetFloatIfPresent(material, "_DstBlend", 0f);
        SetFloatIfPresent(material, "_AlphaSrcBlend", 1f);
        SetFloatIfPresent(material, "_AlphaDstBlend", 0f);
        SetFloatIfPresent(material, "_ZWrite", 1f);
        SetFloatIfPresent(material, "_DoubleSidedEnable", 1f);
        SetFloatIfPresent(material, "_CullMode", 0f);
        SetFloatIfPresent(material, "_CullModeForward", 0f);
        SetFloatIfPresent(material, "_TransparentCullMode", 0f);
        SetColorIfPresent(material, "_EmissiveColor", Color.black);
        SetColorIfPresent(material, "_SpecularColor", Color.black);

        if (!CopyTextureIfPresent(out baseTextureSource, source, material, "_BaseColorMap", "_BaseColorMap", "_BaseMap", "_MainTex", "_Albedo", "_AlbedoMap", "_DiffuseMap", "_ColorMap", "_BaseTexture", "_AlbedoTexture", "_DiffuseTexture"))
        {
            CopyBestTextureByKeyword(out baseTextureSource, source, material, "_BaseColorMap", "albedo", "base", "diffuse", "color", "main");
        }

        if (CopyTextureIfPresent(out normalTextureSource, source, material, "_NormalMap", "_NormalMap", "_BumpMap", "_Normal", "_NormalTexture") ||
            CopyBestTextureByKeyword(out normalTextureSource, source, material, "_NormalMap", "normal", "bump"))
        {
            material.EnableKeyword("_NORMALMAP");
        }

        return material;
    }

    private void CaptureNativeVisualBaseScale(Transform scaleRoot)
    {
        if (_nativeVisualScaleCaptured && ReferenceEquals(_nativeVisualScaleRoot, scaleRoot))
        {
            return;
        }

        _nativeVisualScaleRoot = scaleRoot;
        _nativeVisualBaseScale = IsFinitePositiveScale(scaleRoot.localScale)
            ? scaleRoot.localScale
            : Vector3.one;
        _nativeVisualScaleCaptured = true;
    }

    private void ApplyNativeVisualScale(Transform scaleRoot)
    {
        Vector3 baseScale = IsFinitePositiveScale(_nativeVisualBaseScale)
            ? _nativeVisualBaseScale
            : Vector3.one;
        scaleRoot.localScale = baseScale * Definition.VisualScaleMultiplier;
    }

    private static Transform ResolveNativeVisualScaleRoot(NpcElement npcElement)
    {
        if (npcElement.Controller == null)
        {
            throw new InvalidOperationException("Native visual scale root cannot be resolved without an NpcController.");
        }

        Component[] components = npcElement.Controller.gameObject.GetComponentsInChildren<Component>(true);
        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];
            if (component != null && string.Equals(component.GetType().Name, "Animator", StringComparison.Ordinal))
            {
                return component.transform;
            }
        }

        return npcElement.Controller.transform;
    }

    private void ReleaseVisualReferences()
    {
        if (_overlayInstance != null)
        {
            UnityEngine.Object.Destroy(_overlayInstance);
            _overlayInstance = null;
        }

        ReleaseRuntimeMaterials();

        if (_overlayHandle.IsValid())
        {
            Addressables.Release(_overlayHandle);
        }

        _overlayHandle = default;
        _overlayRequested = false;
        _overlayReady = false;
        _runtimeMaterialCount = 0;
        _materialSlotCount = 0;
        _sourceShaders = "none";
        _targetShaders = "none";
        _baseTextureSources = "none";
        _normalTextureSources = "none";
        _nativeVisualScaleRoot = null;
        _nativeVisualBaseScale = Vector3.one;
        _nativeVisualScaleCaptured = false;
        _runtimeOverlayRenderers.Clear();
        _runtimeOverlayStrippedRenderers.Clear();
        _runtimeOverlayKandraRenderers.Clear();
        _runtimeOverlayStrippedKandraRenderers.Clear();
    }

    private void RetainDeathVisualReferences()
    {
        if (_overlayInstance != null)
        {
            _overlayInstance = null;
        }

        foreach (Material material in _runtimeMaterials)
        {
            if (material != null && !_retainedDeathRuntimeMaterials.Contains(material))
            {
                _retainedDeathRuntimeMaterials.Add(material);
            }
        }

        _runtimeMaterials.Clear();
        if (_overlayHandle.IsValid())
        {
            _retainedDeathHandles.Add(_overlayHandle);
            _overlayHandle = default;
        }

        _overlayRequested = false;
        _overlayReady = false;
        _runtimeMaterialCount = 0;
        _materialSlotCount = 0;
        _sourceShaders = "none";
        _targetShaders = "none";
        _baseTextureSources = "none";
        _normalTextureSources = "none";
        _nativeVisualScaleRoot = null;
        _nativeVisualBaseScale = Vector3.one;
        _nativeVisualScaleCaptured = false;
        _runtimeOverlayRenderers.Clear();
        _runtimeOverlayStrippedRenderers.Clear();
        _runtimeOverlayKandraRenderers.Clear();
        _runtimeOverlayStrippedKandraRenderers.Clear();
    }

    private void ReleaseRuntimeMaterials()
    {
        for (int i = 0; i < _runtimeMaterials.Count; i++)
        {
            Material material = _runtimeMaterials[i];
            if (material != null)
            {
                UnityEngine.Object.Destroy(material);
            }
        }

        _runtimeMaterials.Clear();
    }

    private void ReleaseRetainedDeathVisualReferences()
    {
        for (int i = 0; i < _retainedDeathRuntimeMaterials.Count; i++)
        {
            Material material = _retainedDeathRuntimeMaterials[i];
            if (material != null)
            {
                UnityEngine.Object.Destroy(material);
            }
        }

        _retainedDeathRuntimeMaterials.Clear();
        for (int i = 0; i < _retainedDeathHandles.Count; i++)
        {
            AsyncOperationHandle<GameObject> handle = _retainedDeathHandles[i];
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        _retainedDeathHandles.Clear();
    }

    private static bool IsMountedProp(Transform transform, Transform overlayRoot)
    {
        Transform? cursor = transform;
        while (cursor != null)
        {
            string name = cursor.name ?? string.Empty;
            if (string.Equals(name, "L_w", StringComparison.Ordinal) ||
                string.Equals(name, "R_w", StringComparison.Ordinal) ||
                name.IndexOf("Placeholder", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (cursor == overlayRoot)
            {
                break;
            }

            cursor = cursor.parent;
        }

        return false;
    }

    private static bool IsKandraRendererComponent(Component? component)
    {
        return component != null &&
            string.Equals(component.GetType().Name, "KandraRenderer", StringComparison.Ordinal);
    }

    private static void SetKandraRendererEnabled(Component component, bool enabled)
    {
        try
        {
            PropertyInfo? property = component.GetType().GetProperty(
                "enabled",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.PropertyType == typeof(bool) && property.CanWrite)
            {
                property.SetValue(component, enabled);
            }
        }
        catch
        {
            // Kandra visibility is best-effort; Unity Renderer visibility remains authoritative.
        }
    }

    private static bool CopyTextureIfPresent(out string copiedSourceProperty, Material source, Material target, string targetProperty, params string[] sourceProperties)
    {
        copiedSourceProperty = "none";
        if (!target.HasProperty(targetProperty))
        {
            return false;
        }

        for (int i = 0; i < sourceProperties.Length; i++)
        {
            string sourceProperty = sourceProperties[i];
            if (!source.HasProperty(sourceProperty))
            {
                continue;
            }

            Texture texture = source.GetTexture(sourceProperty);
            if (texture == null)
            {
                continue;
            }

            target.SetTexture(targetProperty, texture);
            copiedSourceProperty = sourceProperty;
            TryCopyTextureTransform(source, target, sourceProperty, targetProperty);
            return true;
        }

        return false;
    }

    private static bool CopyBestTextureByKeyword(out string copiedSourceProperty, Material source, Material target, string targetProperty, params string[] keywords)
    {
        copiedSourceProperty = "none";
        if (!target.HasProperty(targetProperty))
        {
            return false;
        }

        string[] sourceProperties;
        try
        {
            sourceProperties = source.GetTexturePropertyNames();
        }
        catch
        {
            return false;
        }

        for (int i = 0; i < sourceProperties.Length; i++)
        {
            string sourceProperty = sourceProperties[i];
            if (!ContainsAnyKeyword(sourceProperty, keywords))
            {
                continue;
            }

            Texture texture = source.GetTexture(sourceProperty);
            if (texture == null)
            {
                continue;
            }

            target.SetTexture(targetProperty, texture);
            copiedSourceProperty = sourceProperty;
            TryCopyTextureTransform(source, target, sourceProperty, targetProperty);
            return true;
        }

        return false;
    }

    private static bool ContainsAnyKeyword(string value, string[] keywords)
    {
        for (int i = 0; i < keywords.Length; i++)
        {
            if (value.IndexOf(keywords[i], StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static void TryCopyTextureTransform(Material source, Material target, string sourceProperty, string targetProperty)
    {
        try
        {
            target.SetTextureScale(targetProperty, source.GetTextureScale(sourceProperty));
            target.SetTextureOffset(targetProperty, source.GetTextureOffset(sourceProperty));
        }
        catch
        {
            // Shader families do not always expose texture transform metadata.
        }
    }

    private static Color ReadColor(Material material, string propertyName, Color fallback)
    {
        return material.HasProperty(propertyName) ? material.GetColor(propertyName) : fallback;
    }

    private static void SetColorIfPresent(Material material, string propertyName, Color value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, value);
        }
    }

    private static void SetFloatIfPresent(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }

    private static bool ContainsString(List<string> items, string value)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (string.Equals(items[i], value, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static int CountEnabledRenderers(Renderer[] renderers)
    {
        int count = 0;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].enabled)
            {
                count++;
            }
        }

        return count;
    }

    private static bool IsFinitePositiveScale(Vector3 scale)
    {
        return IsFinitePositive(scale.x) && IsFinitePositive(scale.y) && IsFinitePositive(scale.z);
    }

    private static bool IsFinitePositive(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
    }

    internal static string BoolText(bool value) => value ? "true" : "false";

    internal static string FormatVector(Vector3 vector)
    {
        return $"{vector.x:0.00},{vector.y:0.00},{vector.z:0.00}";
    }
}

internal static class CustomCreatureCallSpellFeature
{
    internal const string SourceSpellTemplateGuid = "3bd577472a0191c44bf298a82553cf3b";
    private const string MagicSummonAllySkillGraphGuid = "1ed1718ce1b64c747a329eb4269f529b";
    private const float GrantCheckIntervalSeconds = 2f;

    private static readonly FieldInfo? TemplatesProviderLoaderField = AccessTools.Field(typeof(TemplatesProvider), "_loader");
    private static readonly MethodInfo? TemplatesLoaderAddToMapMethod = AccessTools.Method(typeof(TemplatesLoader), "AddToMap");
    private static readonly FieldInfo? ItemTemplateDescriptionField = AccessTools.Field(typeof(ItemTemplate), "description");
    private static readonly FieldInfo? ItemTemplateFlavorField = AccessTools.Field(typeof(ItemTemplate), "flavor");
    private static readonly FieldInfo? ItemTemplateLightCastInfoField = AccessTools.Field(typeof(ItemTemplate), "lightCastInfo");
    private static readonly FieldInfo? ItemTemplateHeavyCastInfoField = AccessTools.Field(typeof(ItemTemplate), "heavyCastInfo");
    private static readonly FieldInfo? MagicInfoMagicTypeField = AccessTools.Field(typeof(MagicItemTemplateInfo), "magicType");
    private static readonly FieldInfo? MagicInfoEffectTypeField = AccessTools.Field(typeof(MagicItemTemplateInfo), "effectType");
    private static readonly FieldInfo? MagicInfoMagicDescriptionField = AccessTools.Field(typeof(MagicItemTemplateInfo), "magicDescription");

    private static bool _registrationFailureLogged;
    private static bool _grantFailureLogged;
    private static bool _grantSatisfiedForHero;
    private static Hero? _lastGrantHero;
    private static float _nextGrantCheckTime;

    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        PatchTemplateLoader(harmony, logger);
        PatchSkillPerform(harmony, logger);
    }

    internal static void Tick(ManualLogSource logger)
    {
        StandaloneCreatureCompanionPlugin? plugin = StandaloneCreatureCompanionPlugin.Instance;
        if (plugin == null || !plugin.RegisterCallSpell)
        {
            return;
        }

        EnsureRegistered(logger);
        if (!plugin.GrantCallOnLoad)
        {
            _lastGrantHero = null;
            _grantSatisfiedForHero = false;
            return;
        }

        if (Time.unscaledTime < _nextGrantCheckTime)
        {
            return;
        }

        _nextGrantCheckTime = Time.unscaledTime + GrantCheckIntervalSeconds;
        TryGrantWhenReady(logger);
    }

    internal static void Release()
    {
        _lastGrantHero = null;
        _grantSatisfiedForHero = false;
        _grantFailureLogged = false;
        _registrationFailureLogged = false;
    }

    internal static bool EnsureRegistered(ManualLogSource logger)
    {
        StandaloneCreatureCompanionPlugin? plugin = StandaloneCreatureCompanionPlugin.Instance;
        if (plugin == null || !plugin.RegisterCallSpell)
        {
            return false;
        }

        StandaloneCreatureCompanionDefinition definition = plugin.CompanionDefinition;
        if (TryResolveTemplate(definition.SpellTemplateGuid, out ItemTemplate? existingTemplate, out _) && existingTemplate != null)
        {
            ApplyCustomFields(existingTemplate, existingTemplate.templateType, definition);
            return true;
        }

        if (!TryResolveTemplate(SourceSpellTemplateGuid, out ItemTemplate? sourceTemplate, out string sourceReason) || sourceTemplate == null)
        {
            LogRegistrationFailureOnce(logger, plugin, "source-" + sourceReason);
            return false;
        }

        if (!TryGetTemplateLoader(out object? loader, out string loaderReason) || loader == null)
        {
            LogRegistrationFailureOnce(logger, plugin, loaderReason);
            return false;
        }

        if (TemplatesLoaderAddToMapMethod == null)
        {
            LogRegistrationFailureOnce(logger, plugin, "templates-loader-add-map-method-missing");
            return false;
        }

        if (ItemTemplateDescriptionField == null)
        {
            LogRegistrationFailureOnce(logger, plugin, "item-template-description-field-missing");
            return false;
        }

        return TryRegisterDefinition(definition, sourceTemplate, GetComponentTypeNames(sourceTemplate.gameObject), loader, logger, plugin);
    }

    private static bool TryRegisterDefinition(
        StandaloneCreatureCompanionDefinition definition,
        ItemTemplate sourceTemplate,
        string[] sourceComponentTypes,
        object loader,
        ManualLogSource logger,
        StandaloneCreatureCompanionPlugin plugin)
    {
        GameObject? cloneObject = null;
        try
        {
            cloneObject = UnityEngine.Object.Instantiate(sourceTemplate.gameObject);
            cloneObject.name = definition.SpellTemplateName;
            UnityEngine.Object.DontDestroyOnLoad(cloneObject);

            ItemTemplate? customTemplate = cloneObject.GetComponent<ItemTemplate>();
            if (customTemplate == null)
            {
                UnityEngine.Object.Destroy(cloneObject);
                LogRegistrationFailureOnce(logger, plugin, "cloned-object-missing-item-template-" + definition.Id);
                return false;
            }

            ApplyCustomFields(customTemplate, sourceTemplate.templateType, definition);

            string[] customComponentTypes = GetComponentTypeNames(customTemplate.gameObject);
            if (!sourceComponentTypes.SequenceEqual(customComponentTypes, StringComparer.Ordinal))
            {
                UnityEngine.Object.Destroy(cloneObject);
                LogRegistrationFailureOnce(logger, plugin, "native-logic-component-types-mismatch-" + definition.Id);
                return false;
            }

            TemplatesLoaderAddToMapMethod!.Invoke(loader, new object[] { definition.SpellTemplateGuid, customTemplate });
            _registrationFailureLogged = false;
            logger.LogInfo(
                $"{plugin.PluginNameForLogs}: custom call item registered; customGuid={definition.SpellTemplateGuid}; " +
                $"templateName={definition.SpellTemplateName}; displayName={definition.SpellDisplayName}; target={definition.DisplayName}; " +
                $"sourceGuid={SourceSpellTemplateGuid}; sourceName={sourceTemplate.name}; sourceTemplateMutated=false; route=clone-native-wolfs-call-template-private-loader-map.");
            return true;
        }
        catch (Exception ex)
        {
            if (cloneObject != null)
            {
                UnityEngine.Object.Destroy(cloneObject);
            }

            LogRegistrationFailureOnce(logger, plugin, "custom-call-registration-failed-" + definition.Id + " " + ex.GetType().Name + ": " + ex.Message);
            return false;
        }
    }

    private static void PatchTemplateLoader(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? setter = AccessTools.PropertySetter(typeof(TemplatesLoader), nameof(TemplatesLoader.FinishedLoading));
        MethodInfo? postfix = AccessTools.Method(typeof(CustomCreatureCallSpellFeature), nameof(RegisterAfterLoaderFinished));
        if (setter == null || postfix == null)
        {
            logger.LogWarning("Avalon creature companion: could not patch TemplatesLoader.FinishedLoading; call item registration will wait for the update loop.");
            return;
        }

        harmony.Patch(setter, postfix: new HarmonyMethod(postfix));
        logger.LogInfo("Avalon creature companion: patched TemplatesLoader.FinishedLoading for call item registration.");
    }

    private static void PatchSkillPerform(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? perform = AccessTools.Method(typeof(Skill), nameof(Skill.Perform), Type.EmptyTypes);
        MethodInfo? prefix = AccessTools.Method(typeof(CustomCreatureCallSpellFeature), nameof(PerformCustomCreatureCallPrefix));

        if (perform == null || prefix == null)
        {
            logger.LogWarning("Avalon creature companion: Skill.Perform target missing; companion call route disabled.");
            return;
        }

        harmony.Patch(perform, prefix: new HarmonyMethod(prefix));
        logger.LogInfo("Avalon creature companion: patched Skill.Perform for companion call route.");
    }

    private static void RegisterAfterLoaderFinished(bool value)
    {
        if (value && StandaloneCreatureCompanionPlugin.Instance != null)
        {
            EnsureRegistered(StandaloneCreatureCompanionPlugin.Instance.ModLogger);
        }
    }

    private static bool PerformCustomCreatureCallPrefix(Skill __instance)
    {
        StandaloneCreatureCompanionPlugin? plugin = StandaloneCreatureCompanionPlugin.Instance;
        if (plugin == null || !plugin.RegisterCallSpell || !plugin.EnableCallCompanionRoute)
        {
            return true;
        }

        if (!TryGetCustomCreatureCallSkill(__instance, plugin.CompanionDefinition))
        {
            return true;
        }

        if (!__instance.IsSubmitted)
        {
            return true;
        }

        try
        {
            StandaloneCreatureCompanionDefinition definition = plugin.CompanionDefinition;
            plugin.ModLogger.LogInfo(
                $"{plugin.PluginNameForLogs}: {definition.SpellDisplayName} cast intercepted; customGuid={definition.SpellTemplateGuid}; " +
                $"graph={MagicSummonAllySkillGraphGuid}; target={definition.DisplayName}; route=one-session-companion; nativeSkillSpawnLocationSkipped=true.");
            plugin.TrySummonOrSwapFromSpell();
        }
        catch (Exception ex)
        {
            plugin.ModLogger.LogWarning($"{plugin.PluginNameForLogs}: companion call route failed before native graph skip completed: {ex.GetType().Name}: {ex.Message}");
        }

        return false;
    }

    private static bool TryGetCustomCreatureCallSkill(Skill skill, StandaloneCreatureCompanionDefinition definition)
    {
        Item? sourceItem = skill.SourceItem;
        if (sourceItem == null || sourceItem.Template == null)
        {
            return false;
        }

        if (!string.Equals(sourceItem.Template.GUID, definition.SpellTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string graphGuid = skill.Graph?.GUID ?? string.Empty;
        return string.Equals(graphGuid, MagicSummonAllySkillGraphGuid, StringComparison.OrdinalIgnoreCase);
    }

    private static void TryGrantWhenReady(ManualLogSource logger)
    {
        StandaloneCreatureCompanionPlugin? plugin = StandaloneCreatureCompanionPlugin.Instance;
        if (plugin == null || !plugin.EnableCallCompanionRoute)
        {
            LogGrantFailureOnce(logger, plugin, "call-companion-route-disabled");
            return;
        }

        if (!TryGetCurrentHero(out Hero hero))
        {
            _lastGrantHero = null;
            _grantSatisfiedForHero = false;
            return;
        }

        if (!ReferenceEquals(_lastGrantHero, hero))
        {
            _lastGrantHero = hero;
            _grantSatisfiedForHero = false;
            _grantFailureLogged = false;
        }

        if (_grantSatisfiedForHero)
        {
            return;
        }

        int existingCount = CountCustomSpellItems(hero, plugin.CompanionDefinition);
        if (existingCount > 0)
        {
            _grantSatisfiedForHero = true;
            logger.LogInfo($"{plugin.PluginNameForLogs}: current hero already has {plugin.CompanionDefinition.SpellDisplayName} item(s); count={existingCount}.");
            return;
        }

        if (!EnsureRegistered(logger))
        {
            return;
        }

        StandaloneCreatureCompanionDefinition definition = plugin.CompanionDefinition;
        if (!TryResolveTemplate(definition.SpellTemplateGuid, out ItemTemplate? template, out string reason) || template == null)
        {
            LogGrantFailureOnce(logger, plugin, "template-" + reason);
            return;
        }

        try
        {
            Item item = World.Add(new Item(template, 1));
            Item? added = hero.HeroItems.Add(item);
            if (added == null)
            {
                LogGrantFailureOnce(logger, plugin, "HeroItems.Add returned null.");
                return;
            }

            _grantSatisfiedForHero = true;
            logger.LogInfo($"{plugin.PluginNameForLogs}: granted {definition.SpellDisplayName} item {definition.SpellTemplateName} [{definition.SpellTemplateGuid}] to current hero.");
        }
        catch (Exception ex)
        {
            LogGrantFailureOnce(logger, plugin, "grant failed: " + ex.GetType().Name + ": " + ex.Message);
        }
    }

    private static void ApplyCustomFields(
        ItemTemplate customTemplate,
        TemplateType templateType,
        StandaloneCreatureCompanionDefinition definition)
    {
        customTemplate.name = definition.SpellTemplateName;
        customTemplate.GUID = definition.SpellTemplateGuid;
        customTemplate.templateType = templateType;
        customTemplate.hiddenOnUI = false;
        customTemplate.cannotBeDropped = false;
        customTemplate.canStack = false;
        customTemplate.itemName = (LocString)definition.SpellDisplayName;
        ItemTemplateDescriptionField?.SetValue(
            customTemplate,
            new OptionalLocString((LocString)definition.SpellDisplayDescription, true));
        ItemTemplateFlavorField?.SetValue(
            customTemplate,
            new OptionalLocString((LocString)$"A focused Avalon Awakened call bound to the {definition.DisplayName}.", true));
        ApplyCustomMagicCastInfo(customTemplate, definition);
    }

    private static void ApplyCustomMagicCastInfo(
        ItemTemplate customTemplate,
        StandaloneCreatureCompanionDefinition definition)
    {
        if (ItemTemplateLightCastInfoField == null ||
            ItemTemplateHeavyCastInfoField == null ||
            MagicInfoMagicTypeField == null ||
            MagicInfoEffectTypeField == null ||
            MagicInfoMagicDescriptionField == null)
        {
            return;
        }

        ItemTemplateLightCastInfoField.SetValue(customTemplate, CreateCustomMagicInfo(definition.SpellLightCastDescription));
        ItemTemplateHeavyCastInfoField.SetValue(customTemplate, CreateCustomMagicInfo(definition.SpellHeavyCastDescription));
    }

    private static MagicItemTemplateInfo CreateCustomMagicInfo(string description)
    {
        MagicItemTemplateInfo info = new();
        MagicInfoMagicTypeField?.SetValue(info, new RichEnumReference(MagicType.Summon));
        MagicInfoEffectTypeField?.SetValue(info, new RichEnumReference(MagicEffectType.None));
        MagicInfoMagicDescriptionField?.SetValue(info, new OptionalLocString((LocString)description, true));
        return info;
    }

    internal static bool TryResolveTemplate(string templateGuid, out ItemTemplate? template, out string reason)
    {
        template = null;
        reason = string.Empty;
        try
        {
            TemplatesProvider? provider = World.Services?.TryGet<TemplatesProvider>();
            if (provider == null)
            {
                reason = "templates-provider-unavailable";
                return false;
            }

            if (!provider.AllLoaded)
            {
                reason = "templates-provider-not-loaded";
                return false;
            }

            template = provider.Get<ItemTemplate>(templateGuid);
            if (template == null)
            {
                reason = "template-guid-not-found";
                return false;
            }

            if (template.IsAbstract || template.HiddenOnUI || template.CannotBeDropped)
            {
                reason = $"template-not-safe-visible-regular abstract={template.IsAbstract} hidden={template.HiddenOnUI} cannotDrop={template.CannotBeDropped}";
                template = null;
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            reason = "template-resolution-failed " + ex.GetType().Name + ": " + ex.Message;
            return false;
        }
    }

    private static bool TryGetTemplateLoader(out object? loader, out string reason)
    {
        loader = null;
        reason = string.Empty;
        try
        {
            TemplatesProvider? provider = World.Services?.TryGet<TemplatesProvider>();
            if (provider == null)
            {
                reason = "templates-provider-unavailable";
                return false;
            }

            if (!provider.AllLoaded)
            {
                reason = "templates-provider-not-loaded";
                return false;
            }

            if (TemplatesProviderLoaderField == null)
            {
                reason = "templates-provider-loader-field-missing";
                return false;
            }

            loader = TemplatesProviderLoaderField.GetValue(provider);
            if (loader == null)
            {
                reason = "templates-loader-unavailable";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            reason = "templates-loader-resolution-failed " + ex.GetType().Name + ": " + ex.Message;
            return false;
        }
    }

    private static bool TryGetCurrentHero(out Hero hero)
    {
        hero = Hero.Current;
        return hero != null &&
               !hero.HasBeenDiscarded &&
               hero.HeroItems != null &&
               !hero.HeroItems.HasBeenDiscarded;
    }

    private static int CountCustomSpellItems(Hero hero, StandaloneCreatureCompanionDefinition definition)
    {
        int total = 0;
        foreach (Item item in hero.HeroItems.Items)
        {
            if (item == null || item.HasBeenDiscarded)
            {
                continue;
            }

            try
            {
                if (string.Equals(item.Template?.GUID, definition.SpellTemplateGuid, StringComparison.OrdinalIgnoreCase))
                {
                    total += Math.Max(0, item.Quantity);
                }
            }
            catch
            {
                // Ignore malformed inventory rows; the grant path remains exact-template only.
            }
        }

        return total;
    }

    private static string[] GetComponentTypeNames(GameObject templateObject)
    {
        return templateObject
            .GetComponents<Component>()
            .Where(component => component != null)
            .Select(component => component.GetType().AssemblyQualifiedName ?? component.GetType().FullName ?? component.GetType().Name)
            .OrderBy(typeName => typeName, StringComparer.Ordinal)
            .ToArray();
    }

    private static void LogRegistrationFailureOnce(ManualLogSource logger, StandaloneCreatureCompanionPlugin? plugin, string reason)
    {
        if (_registrationFailureLogged)
        {
            return;
        }

        _registrationFailureLogged = true;
        string pluginName = plugin?.PluginNameForLogs ?? "Avalon creature companion";
        logger.LogWarning($"{pluginName}: call item unavailable; reason={reason}.");
    }

    private static void LogGrantFailureOnce(ManualLogSource logger, StandaloneCreatureCompanionPlugin? plugin, string reason)
    {
        if (_grantFailureLogged)
        {
            return;
        }

        _grantFailureLogged = true;
        string pluginName = plugin?.PluginNameForLogs ?? "Avalon creature companion";
        logger.LogWarning($"{pluginName}: call item grant unavailable; reason={reason}");
    }
}

internal static class CustomCreatureMerchantStockPatch
{
    private static readonly HashSet<string> SuccessfulStockKeys = new(StringComparer.OrdinalIgnoreCase);

    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? target = AccessTools.Method(typeof(ShopUI), "OnFullyInitialized");
        MethodInfo? prefix = AccessTools.Method(typeof(CustomCreatureMerchantStockPatch), nameof(OnShopUiFullyInitializedPrefix));
        if (target == null || prefix == null)
        {
            logger.LogWarning("Avalon creature companion: could not patch ShopUI.OnFullyInitialized; merchant stock insertion disabled.");
            return;
        }

        harmony.Patch(target, prefix: new HarmonyMethod(prefix));
        logger.LogInfo("Avalon creature companion: patched ShopUI.OnFullyInitialized for known merchant stock insertion.");
    }

    private static void OnShopUiFullyInitializedPrefix(ShopUI __instance)
    {
        StandaloneCreatureCompanionPlugin? plugin = StandaloneCreatureCompanionPlugin.Instance;
        if (plugin == null || !plugin.MerchantStockEnabled || __instance?.Shop == null)
        {
            return;
        }

        try
        {
            TryAddToKnownMerchant(__instance.Shop, plugin);
        }
        catch (Exception ex)
        {
            plugin.ModLogger.LogWarning($"{plugin.PluginNameForLogs}: merchant stock failed before ShopUI item-list setup; error={ex.GetType().Name}: {ex.Message}");
        }
    }

    private static bool TryAddToKnownMerchant(Shop shop, StandaloneCreatureCompanionPlugin plugin)
    {
        string shopGuid = ShopGuid(shop);
        string shopLogName = string.IsNullOrWhiteSpace(shopGuid) ? ShopName(shop) : shopGuid;
        string targetShopGuid = plugin.MerchantStockTargetShopGuid.Trim();
        if (!string.IsNullOrWhiteSpace(targetShopGuid) &&
            !string.Equals(shopGuid, targetShopGuid, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        StandaloneCreatureCompanionDefinition definition = plugin.CompanionDefinition;
        if (!CustomCreatureCallSpellFeature.EnsureRegistered(plugin.ModLogger))
        {
            plugin.ModLogger.LogWarning($"{plugin.PluginNameForLogs}: merchant stock skipped; shop={shopLogName}; templateGuid={definition.SpellTemplateGuid}; reason=custom-template-not-ready.");
            return false;
        }

        RestockableStock? stock = shop.Elements<RestockableStock>().FirstOrDefault(IsDecompressedRestockableStock);
        if (stock == null)
        {
            plugin.ModLogger.LogWarning($"{plugin.PluginNameForLogs}: merchant stock skipped; shop={shopLogName}; templateGuid={definition.SpellTemplateGuid}; reason=no-decompressed-restockable-stock.");
            return false;
        }

        string stockKey = $"{(string.IsNullOrWhiteSpace(shopGuid) ? ShopKey(shop).ToString() : shopGuid)}|{definition.SpellTemplateGuid}";
        if (SuccessfulStockKeys.Contains(stockKey))
        {
            plugin.ModLogger.LogInfo($"{plugin.PluginNameForLogs}: merchant stock skipped; shop={shopLogName}; templateGuid={definition.SpellTemplateGuid}; reason=already-added-this-session.");
            return false;
        }

        if (ShopContainsTemplate(shop, definition.SpellTemplateGuid))
        {
            plugin.ModLogger.LogInfo($"{plugin.PluginNameForLogs}: merchant stock skipped; shop={shopLogName}; templateGuid={definition.SpellTemplateGuid}; reason=already-present.");
            return false;
        }

        if (!CustomCreatureCallSpellFeature.TryResolveTemplate(definition.SpellTemplateGuid, out ItemTemplate? template, out string resolveReason) || template == null)
        {
            plugin.ModLogger.LogWarning($"{plugin.PluginNameForLogs}: merchant stock skipped; shop={shopLogName}; templateGuid={definition.SpellTemplateGuid}; reason={resolveReason}.");
            return false;
        }

        int quantity = plugin.MerchantStockQuantity;
        int stockCountBefore = GetStockCount(stock);
        Item? createdItem = null;
        try
        {
            createdItem = World.Add(new Item(template, quantity));
            stock.AddItem(createdItem, allowStacking: false);
            SuccessfulStockKeys.Add(stockKey);
            int stockCountAfter = GetStockCount(stock);
            bool visibleAfterAdd = StockContainsTemplate(stock, definition.SpellTemplateGuid) ||
                                   ShopContainsTemplate(shop, definition.SpellTemplateGuid);
            plugin.ModLogger.LogInfo(
                $"{plugin.PluginNameForLogs}: merchant stock added; shop={shopLogName}; stockType=RestockableStock; " +
                $"templateName={template.name}; templateGuid={template.GUID}; displayName={definition.SpellDisplayName}; target={definition.DisplayName}; " +
                $"quantity={quantity}; stockCountBefore={stockCountBefore}; stockCountAfter={stockCountAfter}; visibleAfterAdd={visibleAfterAdd}; " +
                "route=decompressed-before-ShopUI-item-list-World.Add-then-Stock.AddItem-no-stacking.");
            return true;
        }
        catch (Exception ex)
        {
            try
            {
                createdItem?.Discard();
            }
            catch
            {
                // Best-effort cleanup only; keep the stock-add failure visible.
            }

            plugin.ModLogger.LogWarning($"{plugin.PluginNameForLogs}: merchant stock failed; shop={shopLogName}; templateGuid={definition.SpellTemplateGuid}; quantity={quantity}; error={ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private static bool IsDecompressedRestockableStock(RestockableStock stock)
    {
        try
        {
            return !stock.IsCompressed && GetCompressedItems(stock).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    private static List<ItemSpawningDataRuntime> GetCompressedItems(Stock stock)
    {
        FieldInfo? field = AccessTools.Field(stock.GetType(), "_compressedItems");
        return field?.GetValue(stock) as List<ItemSpawningDataRuntime> ?? new List<ItemSpawningDataRuntime>();
    }

    private static bool ShopContainsTemplate(Shop shop, string templateGuid)
    {
        try
        {
            foreach (Item item in shop.Items)
            {
                if (string.Equals(item?.Template?.GUID, templateGuid, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static bool StockContainsTemplate(Stock stock, string templateGuid)
    {
        try
        {
            if (stock.IsCompressed)
            {
                return false;
            }

            foreach (Item item in stock.Items)
            {
                if (string.Equals(item?.Template?.GUID, templateGuid, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static int GetStockCount(Stock stock)
    {
        MethodInfo? getter = AccessTools.PropertyGetter(stock.GetType(), "Count");
        if (getter?.Invoke(stock, null) is int propertyValue)
        {
            return Math.Max(0, propertyValue);
        }

        FieldInfo? field = AccessTools.Field(stock.GetType(), "<Count>k__BackingField") ?? AccessTools.Field(stock.GetType(), "_count");
        if (field?.GetValue(stock) is int fieldValue)
        {
            return Math.Max(0, fieldValue);
        }

        return -1;
    }

    private static string ShopGuid(Shop shop)
    {
        return shop.Template?.GUID ?? string.Empty;
    }

    private static string ShopName(Shop shop)
    {
        return shop.Template?.GUID ?? shop.Template?.name ?? "UnknownShop";
    }

    private static int ShopKey(Shop shop)
    {
        return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(shop);
    }
}

internal static class CreatureCallItemIconPatch
{
    private const int IconSize = 128;
    private static readonly MethodInfo? SetInternalVisibilityMethod = AccessTools.Method(
        typeof(ItemIconComponent).BaseType?.BaseType,
        "SetInternalVisibility");
    private static readonly HashSet<string> LoggedApplications = new(StringComparer.OrdinalIgnoreCase);
    private static Sprite? s_iconSprite;
    private static Texture2D? s_iconTexture;
    private static string s_iconSource = string.Empty;

    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? refresh = AccessTools.Method(
            typeof(ItemIconComponent),
            "Refresh",
            new[] { typeof(Item), typeof(View), typeof(ItemDescriptorType) });
        MethodInfo? prefix = AccessTools.Method(typeof(CreatureCallItemIconPatch), nameof(Prefix));

        if (refresh == null || prefix == null)
        {
            logger.LogWarning("Avalon creature companion: item icon patch unavailable; reason=item-icon-refresh-target-missing.");
            return;
        }

        harmony.Patch(refresh, prefix: new HarmonyMethod(prefix));
        logger.LogInfo("Avalon creature companion: patched ItemIconComponent.Refresh for embedded call item icon.");
    }

    private static bool Prefix(
        Item item,
        ItemIconComponent __instance,
        Image ___icon,
        GameObject ___lockIcon,
        GameObject ___favouriteIcon)
    {
        StandaloneCreatureCompanionPlugin? plugin = StandaloneCreatureCompanionPlugin.Instance;
        if (plugin == null || item?.Template == null)
        {
            return true;
        }

        StandaloneCreatureCompanionDefinition definition = plugin.CompanionDefinition;
        if (!string.Equals(item.Template.GUID, definition.SpellTemplateGuid, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (___icon == null || SetInternalVisibilityMethod == null)
        {
            return true;
        }

        try
        {
            Sprite sprite = GetOrCreateIconSprite(plugin, definition, out string iconSource);
            ___lockIcon?.SetActive(item.Locked);
            ___favouriteIcon?.SetActive(item.Favourite);
            ___icon.sprite = sprite;
            ___icon.color = Color.white;
            ___icon.preserveAspect = true;
            ___icon.enabled = true;
            SetInternalVisibilityMethod.Invoke(__instance, new object[] { true });

            string itemGuid = item.Template?.GUID ?? string.Empty;
            if (LoggedApplications.Add(itemGuid))
            {
                plugin.ModLogger.LogInfo($"{plugin.PluginNameForLogs}: call item icon applied; item={itemGuid}; source={iconSource}.");
            }

            return false;
        }
        catch (Exception ex)
        {
            plugin.ModLogger.LogWarning($"{plugin.PluginNameForLogs}: call item icon failed; fallback=native; error={ex.GetType().Name}: {ex.Message}");
            return true;
        }
    }

    internal static void Release()
    {
        if (s_iconSprite != null)
        {
            UnityEngine.Object.Destroy(s_iconSprite);
            s_iconSprite = null;
        }

        if (s_iconTexture != null)
        {
            UnityEngine.Object.Destroy(s_iconTexture);
            s_iconTexture = null;
        }

        LoggedApplications.Clear();
        s_iconSource = string.Empty;
    }

    private static Sprite GetOrCreateIconSprite(
        StandaloneCreatureCompanionPlugin plugin,
        StandaloneCreatureCompanionDefinition definition,
        out string source)
    {
        Texture2D texture = s_iconTexture ??= TryLoadEmbeddedIcon(plugin, definition, out s_iconSource) ??
            TryGetTaintedInterfaceIconTexture(definition, out s_iconSource) ??
            CreateProceduralIconTexture(definition, out s_iconSource);
        source = s_iconSource;
        if (s_iconSprite != null)
        {
            return s_iconSprite;
        }

        s_iconSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);
        s_iconSprite.name = plugin.PluginNameForLogs + ".CallItem.Icon";
        s_iconSprite.hideFlags = HideFlags.HideAndDontSave;
        return s_iconSprite;
    }

    private static Texture2D? TryLoadEmbeddedIcon(
        StandaloneCreatureCompanionPlugin plugin,
        StandaloneCreatureCompanionDefinition definition,
        out string source)
    {
        source = string.Empty;
        string? resourceName = definition.EmbeddedIconResourceName;
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            return null;
        }

        try
        {
            Assembly assembly = plugin.GetType().Assembly;
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                plugin.ModLogger.LogWarning($"{plugin.PluginNameForLogs}: embedded icon resource missing; resource={resourceName}.");
                return null;
            }

            byte[] bytes = ReadStreamBounded(stream, 8 * 1024 * 1024, resourceName);
            Texture2D loaded = new(2, 2, TextureFormat.RGBA32, false)
            {
                name = plugin.PluginNameForLogs + ".CallItem.Icon.Source",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            if (!InvokeLoadImage(loaded, bytes) || loaded.width <= 0 || loaded.height <= 0)
            {
                UnityEngine.Object.Destroy(loaded);
                return null;
            }

            Texture2D cropped = CreateSquareCenterCrop(loaded, plugin.PluginNameForLogs + ".CallItem.Icon");
            UnityEngine.Object.Destroy(loaded);
            source = "embedded:" + resourceName;
            return cropped;
        }
        catch (Exception ex)
        {
            plugin.ModLogger.LogWarning($"{plugin.PluginNameForLogs}: embedded icon load failed; resource={resourceName}; error={ex.GetType().Name}: {ex.Message}");
            source = string.Empty;
            return null;
        }
    }

    private static Texture2D? TryGetTaintedInterfaceIconTexture(
        StandaloneCreatureCompanionDefinition definition,
        out string source)
    {
        Texture2D? itemIcon = TaintedInterfaceReflection.GetItemIcon(definition.SpellTemplateGuid);
        if (itemIcon != null)
        {
            source = "TaintedInterface.GetItemIcon(" + definition.SpellTemplateGuid + ")";
            return itemIcon;
        }

        source = string.Empty;
        return null;
    }

    private static Texture2D CreateProceduralIconTexture(StandaloneCreatureCompanionDefinition definition, out string source)
    {
        Color32[] pixels = new Color32[IconSize * IconSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color32(0, 0, 0, 0);
        }

        Color32 plate = new(24, 19, 16, 220);
        Color32 rim = new(95, 61, 27, 255);
        FillDisc(pixels, 64, 64, 56, plate);
        DrawCircle(pixels, 64, 64, 55, rim, 3);
        FillDisc(pixels, 64, 64, 38, definition.PrimaryIconColor);
        DrawCircle(pixels, 64, 64, 38, definition.AccentIconColor, 2);
        DrawLine(pixels, 37, 83, 64, 32, definition.AccentIconColor, 6);
        DrawLine(pixels, 64, 32, 91, 83, definition.AccentIconColor, 6);
        DrawLine(pixels, 47, 70, 81, 70, new Color32(255, 224, 145, 230), 4);

        Texture2D texture = new(IconSize, IconSize, TextureFormat.RGBA32, false)
        {
            name = "AvalonCreatureCompanion.CallItem.ProceduralIcon." + definition.Id,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
        };
        texture.SetPixels32(pixels);
        texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        source = "plugin-owned-procedural-texture";
        return texture;
    }

    private static Texture2D CreateSquareCenterCrop(Texture2D sourceTexture, string textureName)
    {
        int sourceWidth = sourceTexture.width;
        int sourceHeight = sourceTexture.height;
        int squareSize = Math.Min(sourceWidth, sourceHeight);
        int offsetX = Math.Max(0, (sourceWidth - squareSize) / 2);
        int offsetY = Math.Max(0, (sourceHeight - squareSize) / 2);
        Color32[] sourcePixels = sourceTexture.GetPixels32();
        Color32[] croppedPixels = new Color32[squareSize * squareSize];

        for (int y = 0; y < squareSize; y++)
        {
            Array.Copy(
                sourcePixels,
                (offsetY + y) * sourceWidth + offsetX,
                croppedPixels,
                y * squareSize,
                squareSize);
        }

        Texture2D cropped = new(squareSize, squareSize, TextureFormat.RGBA32, false)
        {
            name = textureName,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
        };
        cropped.SetPixels32(croppedPixels);
        cropped.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        return cropped;
    }

    private static bool InvokeLoadImage(Texture2D texture, byte[] bytes)
    {
        Type? imageConversionType = Type.GetType("UnityEngine.ImageConversion, UnityEngine.ImageConversionModule", throwOnError: false);
        if (imageConversionType == null)
        {
            try
            {
                Assembly.Load("UnityEngine.ImageConversionModule");
            }
            catch
            {
                // The loaded-assembly scan below keeps this soft when the module is already present under Unity's loader.
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                imageConversionType = assembly.GetType("UnityEngine.ImageConversion", throwOnError: false);
                if (imageConversionType != null)
                {
                    break;
                }
            }
        }

        MethodInfo? method = imageConversionType?.GetMethod(
            "LoadImage",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(Texture2D), typeof(byte[]), typeof(bool) },
            null);
        if (method == null)
        {
            throw new MissingMethodException("UnityEngine.ImageConversion.LoadImage(Texture2D, byte[], bool)");
        }

        object? result = method.Invoke(null, new object[] { texture, bytes, false });
        return result is bool loaded ? loaded : texture.width > 0 && texture.height > 0;
    }

    private static byte[] ReadStreamBounded(Stream stream, int maximumBytes, string logicalName)
    {
        if (stream.Length > maximumBytes)
        {
            throw new InvalidDataException($"Embedded icon resource '{logicalName}' exceeds {maximumBytes} bytes.");
        }

        using MemoryStream memory = new();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    private static void DrawCircle(Color32[] pixels, int cx, int cy, int radius, Color32 color, int thickness)
    {
        for (int angle = 0; angle < 360; angle++)
        {
            double radians = angle * Math.PI / 180.0;
            int x = cx + (int)Math.Round(Math.Cos(radians) * radius);
            int y = cy + (int)Math.Round(Math.Sin(radians) * radius);
            FillDisc(pixels, x, y, thickness, color);
        }
    }

    private static void DrawLine(Color32[] pixels, int x0, int y0, int x1, int y1, Color32 color, int thickness)
    {
        int dx = x1 - x0;
        int dy = y1 - y0;
        int steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
        if (steps == 0)
        {
            FillDisc(pixels, x0, y0, thickness, color);
            return;
        }

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            int x = (int)Math.Round(x0 + dx * t);
            int y = (int)Math.Round(y0 + dy * t);
            FillDisc(pixels, x, y, thickness, color);
        }
    }

    private static void FillDisc(Color32[] pixels, int cx, int cy, int radius, Color32 color)
    {
        int radiusSquared = radius * radius;
        for (int y = cy - radius; y <= cy + radius; y++)
        {
            for (int x = cx - radius; x <= cx + radius; x++)
            {
                int dx = x - cx;
                int dy = y - cy;
                if (dx * dx + dy * dy <= radiusSquared)
                {
                    BlendPixel(pixels, x, y, color);
                }
            }
        }
    }

    private static void BlendPixel(Color32[] pixels, int x, int y, Color32 source)
    {
        if (x < 0 || x >= IconSize || y < 0 || y >= IconSize || source.a == 0)
        {
            return;
        }

        int index = y * IconSize + x;
        Color32 destination = pixels[index];
        if (source.a == 255 || destination.a == 0)
        {
            pixels[index] = source;
            return;
        }

        float sourceAlpha = source.a / 255f;
        float destinationAlpha = destination.a / 255f;
        float outputAlpha = sourceAlpha + destinationAlpha * (1f - sourceAlpha);
        if (outputAlpha <= 0f)
        {
            pixels[index] = new Color32(0, 0, 0, 0);
            return;
        }

        byte r = (byte)Mathf.RoundToInt((source.r * sourceAlpha + destination.r * destinationAlpha * (1f - sourceAlpha)) / outputAlpha);
        byte g = (byte)Mathf.RoundToInt((source.g * sourceAlpha + destination.g * destinationAlpha * (1f - sourceAlpha)) / outputAlpha);
        byte b = (byte)Mathf.RoundToInt((source.b * sourceAlpha + destination.b * destinationAlpha * (1f - sourceAlpha)) / outputAlpha);
        byte a = (byte)Mathf.RoundToInt(outputAlpha * 255f);
        pixels[index] = new Color32(r, g, b, a);
    }
}

internal static class TaintedInterfaceReflection
{
    private const string ApiTypeName = "TaintedInterface.TaintedInterfaceApi";

    private static Type? s_apiType;
    private static MethodInfo? s_getItemIcon;
    private static PropertyInfo? s_isAvailable;
    private static bool s_loggedInvokeFailure;

    internal static Texture2D? GetItemIcon(string itemReference)
    {
        if (string.IsNullOrWhiteSpace(itemReference) || !IsAvailable())
        {
            return null;
        }

        MethodInfo? method = ResolveGetItemIcon();
        if (method == null)
        {
            return null;
        }

        try
        {
            return method.Invoke(null, new object[] { itemReference }) as Texture2D;
        }
        catch (Exception ex)
        {
            LogInvokeFailure("read Tainted Interface item icon", ex);
            return null;
        }
    }

    private static bool IsAvailable()
    {
        PropertyInfo? property = ResolveIsAvailable();
        if (property == null)
        {
            return false;
        }

        try
        {
            return property.GetValue(null) is bool available && available;
        }
        catch
        {
            return false;
        }
    }

    private static MethodInfo? ResolveGetItemIcon()
    {
        return s_getItemIcon ??= ResolveApiType()?.GetMethod(
            "GetItemIcon",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string) },
            null);
    }

    private static PropertyInfo? ResolveIsAvailable()
    {
        return s_isAvailable ??= ResolveApiType()?.GetProperty(
            "IsAvailable",
            BindingFlags.Public | BindingFlags.Static);
    }

    private static Type? ResolveApiType()
    {
        if (s_apiType != null)
        {
            return s_apiType;
        }

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? apiType = assembly.GetType(ApiTypeName, throwOnError: false);
            if (apiType != null)
            {
                s_apiType = apiType;
                return apiType;
            }
        }

        Type? namedType = Type.GetType(ApiTypeName + ", TaintedInterface", throwOnError: false);
        if (namedType != null)
        {
            s_apiType = namedType;
        }

        return s_apiType;
    }

    private static void LogInvokeFailure(string action, Exception exception)
    {
        if (s_loggedInvokeFailure)
        {
            return;
        }

        s_loggedInvokeFailure = true;
        Debug.LogWarning($"Avalon creature companion could not {action}: {exception.GetType().Name}: {exception.Message}");
    }
}
