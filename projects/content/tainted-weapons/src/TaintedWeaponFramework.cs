using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Awaken.ECS.Components;
using Awaken.ECS.DrakeRenderer.Authoring;
using Awaken.ECS.DrakeRenderer.Components;
using Awaken.ECS.Mipmaps.Components;
using Awaken.TG.Assets;
using Awaken.TG.Graphics;
using Awaken.TG.Graphics.Cutscenes;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Combat;
using Awaken.TG.Main.Heroes.Items.Attachments;
using BepInEx.Logging;
using HarmonyLib;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace TaintedWeapons;

public sealed class TaintedWeaponDefinition
{
    public TaintedWeaponDefinition(
        string packageId,
        string weaponId,
        string customTemplateGuid,
        string sourceTemplateGuid,
        string displayName,
        string description,
        string packageRootDirectory,
        string bundleFileName,
        string equippedPrefabAssetPath)
        : this(
            packageId,
            weaponId,
            customTemplateGuid,
            sourceTemplateGuid,
            displayName,
            description,
            packageRootDirectory,
            bundleFileName,
            equippedPrefabAssetPath,
            customTemplateName: null)
    {
    }

    public TaintedWeaponDefinition(
        string packageId,
        string weaponId,
        string customTemplateGuid,
        string sourceTemplateGuid,
        string displayName,
        string description,
        string packageRootDirectory,
        string bundleFileName,
        string equippedPrefabAssetPath,
        string? customTemplateName)
    {
        PackageId = Required(packageId, nameof(packageId));
        WeaponId = Required(weaponId, nameof(weaponId));
        CustomTemplateGuid = Required(customTemplateGuid, nameof(customTemplateGuid));
        SourceTemplateGuid = Required(sourceTemplateGuid, nameof(sourceTemplateGuid));
        CustomTemplateName = string.IsNullOrWhiteSpace(customTemplateName)
            ? "ItemTemplate_Mod_" + TaintedWeaponsIdentityPolicy.EscapeIdentitySegment(CustomTemplateGuid)
            : customTemplateName.Trim();
        DisplayName = Required(displayName, nameof(displayName));
        Description = description ?? string.Empty;
        PackageRootDirectory = Required(packageRootDirectory, nameof(packageRootDirectory));
        BundleFileName = Required(bundleFileName, nameof(bundleFileName));
        EquippedPrefabAssetPath = Required(equippedPrefabAssetPath, nameof(equippedPrefabAssetPath));
    }

    public string PackageId { get; }

    public string WeaponId { get; }

    public string CustomTemplateGuid { get; }

    public string SourceTemplateGuid { get; }

    public string CustomTemplateName { get; }

    public string DisplayName { get; }

    public string Description { get; }

    public string PackageRootDirectory { get; }

    public string BundleFileName { get; }

    public string EquippedPrefabAssetPath { get; }

    public string RegistryKey => TaintedWeaponsIdentityPolicy.CanonicalRegistryKey(PackageId, WeaponId);

    private static string Required(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", name);
        }

        return value.Trim();
    }
}

public sealed class TaintedWeaponRegistrationResult
{
    internal TaintedWeaponRegistrationResult(
        string registryKey,
        bool accepted,
        bool assetResolved,
        bool drakeReady,
        string reason,
        string details,
        TaintedWeaponsIdentityReceipt? identityReceipt = null)
    {
        RegistryKey = registryKey;
        Accepted = accepted;
        AssetResolved = assetResolved;
        DrakeReady = drakeReady;
        Reason = reason;
        Details = details;
        IdentityReceipt = identityReceipt;
    }

    public string RegistryKey { get; }

    public bool Accepted { get; }

    public bool AssetResolved { get; }

    public bool DrakeReady { get; }

    public string Reason { get; }

    public string Details { get; }

    public TaintedWeaponsIdentityReceipt? IdentityReceipt { get; }

    public override string ToString()
    {
        string identity = IdentityReceipt == null ? string.Empty : $"; identity={IdentityReceipt}";
        return $"key={RegistryKey}; accepted={Accepted}; assetResolved={AssetResolved}; drakeReady={DrakeReady}; reason={Reason}; details={Details}{identity}";
    }
}

public sealed class TaintedWeaponsIdentityReceipt
{
    private readonly string[] _reservedKeys;

    internal TaintedWeaponsIdentityReceipt(
        string publicId,
        string canonicalPackageId,
        string canonicalWeaponId,
        string customTemplateGuid,
        string sourceTemplateGuid,
        string customTemplateName,
        string definitionHash,
        string registryKey,
        string runtimePrototypeAddress,
        string meshKey,
        string materialKeyPrefix,
        string[] reservedKeys)
    {
        PublicId = publicId;
        CanonicalPackageId = canonicalPackageId;
        CanonicalWeaponId = canonicalWeaponId;
        CustomTemplateGuid = customTemplateGuid;
        SourceTemplateGuid = sourceTemplateGuid;
        CustomTemplateName = customTemplateName;
        DefinitionHash = definitionHash;
        RegistryKey = registryKey;
        RuntimePrototypeAddress = runtimePrototypeAddress;
        MeshKey = meshKey;
        MaterialKeyPrefix = materialKeyPrefix;
        _reservedKeys = reservedKeys;
    }

    public int Version => 1;

    public string PublicId { get; }

    public string CanonicalPackageId { get; }

    public string CanonicalWeaponId { get; }

    public string CustomTemplateGuid { get; }

    public string SourceTemplateGuid { get; }

    public string CustomTemplateName { get; }

    public string DefinitionHash { get; }

    public string CanonicalDefinitionHash => "sha256:" + DefinitionHash;

    public string RegistryKey { get; }

    public string RuntimePrototypeAddress { get; }

    public string MeshKey { get; }

    public string MaterialKeyPrefix { get; }

    public IReadOnlyList<string> ReservedKeys => _reservedKeys;

    internal string RuntimePrototypeObjectName => "TaintedWeapons_" + TaintedWeaponsIdentityPolicy.EscapeIdentitySegment(CanonicalWeaponId) + "_" + DefinitionHash.Substring(0, 12) + "_RuntimeEquippedPrototype";

    internal string AssetKeyForSlot(string slot)
    {
        if (string.Equals(slot, "mesh", StringComparison.OrdinalIgnoreCase))
        {
            return MeshKey;
        }

        return RuntimePrototypeAddress + "/" + TaintedWeaponsIdentityPolicy.EscapeIdentitySegment(slot);
    }

    public override string ToString()
    {
        return $"version={Version.ToString(CultureInfo.InvariantCulture)}; publicId={PublicId}; templateGuid={CustomTemplateGuid}; templateName={CustomTemplateName}; definitionHash={DefinitionHash}; canonicalDefinitionHash={CanonicalDefinitionHash}; registryKey={RegistryKey}; runtimePrototypeAddress={RuntimePrototypeAddress}; meshKey={MeshKey}; materialKeyPrefix={MaterialKeyPrefix}; reservedKeys={string.Join("|", _reservedKeys)}";
    }
}

public sealed class TaintedWeaponsCapability
{
    internal TaintedWeaponsCapability(string id, bool mandatory, bool available, string reason)
    {
        Id = id;
        Mandatory = mandatory;
        Available = available;
        Reason = reason;
    }

    public string Id { get; }

    public bool Mandatory { get; }

    public bool Available { get; }

    public string Reason { get; }

    public override string ToString()
    {
        return $"id={Id}; mandatory={Mandatory}; available={Available}; reason={Reason}";
    }
}

public sealed class TaintedWeaponsCapabilityState
{
    internal TaintedWeaponsCapabilityState(
        string stage,
        string capabilityId,
        string status,
        bool available,
        string reasonCode,
        string reason)
    {
        Stage = Clean(stage);
        CapabilityId = Clean(capabilityId);
        Status = Clean(status);
        Available = available;
        ReasonCode = Clean(reasonCode);
        Reason = Clean(reason);
    }

    public string Stage { get; }

    public string CapabilityId { get; }

    public string Status { get; }

    public bool Available { get; }

    public string ReasonCode { get; }

    public string Reason { get; }

    public override string ToString()
    {
        return $"stage={Stage}; capabilityId={CapabilityId}; status={Status}; available={Available}; reasonCode={ReasonCode}; reason={Reason}";
    }

    private static string Clean(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        return value
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace(';', ',')
            .Replace('|', ',')
            .Trim();
    }
}

public sealed class TaintedWeaponsCapabilityReceipt
{
    public const int ContractVersion = 1;

    private readonly TaintedWeaponsCapability[] _capabilities;
    private readonly TaintedWeaponsCapabilityState[] _states;

    internal TaintedWeaponsCapabilityReceipt(
        IEnumerable<TaintedWeaponsCapability> capabilities,
        IEnumerable<TaintedWeaponsCapabilityState>? states = null)
    {
        _capabilities = capabilities.ToArray();
        Ready = _capabilities.Where(capability => capability.Mandatory).All(capability => capability.Available);
        DenialReasons = BuildDenialReasons(_capabilities
            .Where(capability => capability.Mandatory && !capability.Available)
            .Select(capability => capability.Id + ":" + capability.Reason));
        _states = (states ?? CreateDefaultStates(Ready, DenialReasons)).ToArray();
        PresentationReady = Ready && _states.Any(state =>
            IsPresentationState(state) &&
            state.Available &&
            string.Equals(state.Status, "presentation_ready", StringComparison.OrdinalIgnoreCase));
        ImporterReady = _states.Length > 0 && _states.All(state => state.Available);
        PresentationDenialReasons = BuildDenialReasons(_states
            .Where(state => IsPresentationState(state) && !state.Available)
            .Select(state => state.Stage + ":" + state.ReasonCode));
        StateDenialReasons = BuildDenialReasons(_states
            .Where(state => !state.Available)
            .Select(state => state.Stage + ":" + state.ReasonCode));
    }

    public int Version => ContractVersion;

    public bool Ready { get; }

    public bool PresentationReady { get; }

    public bool ImporterReady { get; }

    public IReadOnlyList<TaintedWeaponsCapability> Capabilities => _capabilities;

    public IReadOnlyList<TaintedWeaponsCapabilityState> States => _states;

    public string DenialReasons { get; }

    public string PresentationDenialReasons { get; }

    public string StateDenialReasons { get; }

    public bool HasCapability(string id)
    {
        return _capabilities.Any(capability =>
            capability.Available &&
            string.Equals(capability.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    public override string ToString()
    {
        return $"version={Version.ToString(CultureInfo.InvariantCulture)}; ready={Ready}; presentationReady={PresentationReady}; importerReady={ImporterReady}; denialReasons={DenialReasons}; presentationDenialReasons={PresentationDenialReasons}; stateDenialReasons={StateDenialReasons}; capabilities={string.Join("||", _capabilities.Select(capability => capability.ToString()))}; states={string.Join("||", _states.Select(state => state.ToString()))}";
    }

    internal static TaintedWeaponsCapabilityReceipt Blocked(string reason)
    {
        return new TaintedWeaponsCapabilityReceipt(new[]
        {
            new TaintedWeaponsCapability("capability-receipt", mandatory: true, available: false, reason)
        });
    }

    private static TaintedWeaponsCapabilityState[] CreateDefaultStates(bool presentationAvailable, string presentationReason)
    {
        string presentationStatus = presentationAvailable ? "presentation_ready" : "presentation_blocked";
        string presentationReasonCode = presentationAvailable ? "ok" : "presentation-capability-denied";

        return new[]
        {
            new TaintedWeaponsCapabilityState(
                "package",
                "asset_package.offline_validation",
                "package.blocked",
                available: false,
                "package-validation-receipt-not-bound",
                "No typed package validation receipt is consumed by the Tainted Weapons runtime lane."),
            new TaintedWeaponsCapabilityState(
                "provider",
                "asset_provider.drake_weapon.asset_load",
                "asset_materialization_blocked",
                available: false,
                "typed-provider-receipt-not-bound",
                "No typed Drake weapon provider mount or asset-load receipt is consumed by this lane."),
            new TaintedWeaponsCapabilityState(
                "template",
                "registrar.native_item.registration",
                "template_registration_denied",
                available: false,
                "native-registrar-receipt-required",
                "The lane has local identity reservation only; shared native registrar registration is not proven."),
            new TaintedWeaponsCapabilityState(
                "presentation",
                "presentation.weapon.drake",
                presentationStatus,
                presentationAvailable,
                presentationReasonCode,
                presentationAvailable ? "Bounded Tainted Weapons presentation hooks are available." : presentationReason),
            new TaintedWeaponsCapabilityState(
                "equip",
                "presentation.weapon.equip_diagnostics",
                "equip_blocked",
                available: false,
                "live-equip-receipt-required",
                "No exact live equip diagnostic receipt is bound to this capability receipt."),
            new TaintedWeaponsCapabilityState(
                "persistence",
                "persistence.item_restore",
                "persistence_blocked",
                available: false,
                "persistence-gates-unpassed",
                "Persistence gates are not passed for this lane."),
            new TaintedWeaponsCapabilityState(
                "compatibility",
                "compatibility.package_runtime_tuple",
                "compatibility_blocked",
                available: false,
                "compatibility-gates-unpassed",
                "Compatibility tuple gates are not passed for this lane."),
            new TaintedWeaponsCapabilityState(
                "release",
                "release.public_preview",
                "release_blocked",
                available: false,
                "release-gates-unpassed",
                "Public preview and release gates are not passed for this lane.")
        };
    }

    private static bool IsPresentationState(TaintedWeaponsCapabilityState state)
    {
        return string.Equals(state.Stage, "presentation", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(state.CapabilityId, "presentation.weapon.drake", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildDenialReasons(IEnumerable<string> reasons)
    {
        string joined = string.Join("|", reasons.Where(reason => !string.IsNullOrWhiteSpace(reason)));
        return string.IsNullOrWhiteSpace(joined) ? "ok" : joined;
    }
}

public static class TaintedWeaponsApi
{
    private static readonly List<TaintedWeaponDefinition> PendingDefinitions = new();
    private static readonly List<TaintedWeaponsIdentityReceipt> PendingIdentityReceipts = new();
    private static readonly List<TaintedWeaponNativeItemRegistrationRequest> PendingNativeItemRequests = new();
    private static readonly TaintedWeaponsIdentityPolicy PendingIdentityPolicy = new();

    public static bool RegisterWeaponDefinition(
        string packageId,
        string weaponId,
        string customTemplateGuid,
        string sourceTemplateGuid,
        string displayName,
        string description,
        string packageRootDirectory,
        string bundleFileName,
        string equippedPrefabAssetPath,
        out string message)
    {
        try
        {
            var definition = new TaintedWeaponDefinition(
                packageId,
                weaponId,
                customTemplateGuid,
                sourceTemplateGuid,
                displayName,
                description,
                packageRootDirectory,
                bundleFileName,
                equippedPrefabAssetPath);
            return RegisterWeaponDefinition(definition, out message);
        }
        catch (Exception ex)
        {
            message = "definition-invalid:" + ex.GetType().Name + ":" + ex.Message;
            return false;
        }
    }

    public static bool RegisterWeaponDefinition(
        string packageId,
        string weaponId,
        string customTemplateGuid,
        string sourceTemplateGuid,
        string customTemplateName,
        string displayName,
        string description,
        string packageRootDirectory,
        string bundleFileName,
        string equippedPrefabAssetPath,
        out string message)
    {
        try
        {
            var definition = new TaintedWeaponDefinition(
                packageId,
                weaponId,
                customTemplateGuid,
                sourceTemplateGuid,
                displayName,
                description,
                packageRootDirectory,
                bundleFileName,
                equippedPrefabAssetPath,
                customTemplateName);
            return RegisterWeaponDefinition(definition, out message);
        }
        catch (Exception ex)
        {
            message = "definition-invalid:" + ex.GetType().Name + ":" + ex.Message;
            return false;
        }
    }

    public static bool RegisterWeaponDefinition(TaintedWeaponDefinition definition, out string message)
    {
        if (definition == null)
        {
            message = "definition-null";
            return false;
        }

        Plugin? plugin = Plugin.Instance;
        if (plugin == null || !plugin.ImporterLifecycleOpen)
        {
            lock (PendingDefinitions)
            {
                if (!PendingIdentityPolicy.TryReserve(definition, out TaintedWeaponsIdentityReceipt identityReceipt, out string identityReason))
                {
                    message = "queued-identity-collision:" + identityReason;
                    return false;
                }

                if (!PendingIdentityReceipts.Any(existing =>
                    string.Equals(existing.PublicId, identityReceipt.PublicId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(existing.DefinitionHash, identityReceipt.DefinitionHash, StringComparison.OrdinalIgnoreCase)))
                {
                    PendingDefinitions.Add(definition);
                    PendingIdentityReceipts.Add(identityReceipt);
                }
            }

            message = plugin == null ? "queued-framework-not-awake" : "queued-framework-importer-not-open";
            return true;
        }

        return plugin.RegisterWeaponDefinition(definition, out message);
    }

    public static bool RegisterWeaponPackage(
        string packageId,
        string weaponId,
        string customTemplateGuid,
        string sourceTemplateGuid,
        string customTemplateName,
        string displayName,
        string description,
        string packageRootDirectory,
        string bundleFileName,
        string equippedPrefabAssetPath,
        string nativeRegistrationProfileId,
        string semanticIconAddress,
        string flavorText,
        bool requestNativeRegistration,
        out TaintedWeaponPackageImportReceipt receipt)
    {
        try
        {
            var manifest = new TaintedWeaponPackageManifest(
                packageId,
                weaponId,
                customTemplateGuid,
                sourceTemplateGuid,
                customTemplateName,
                displayName,
                description,
                packageRootDirectory,
                bundleFileName,
                equippedPrefabAssetPath,
                nativeRegistrationProfileId,
                semanticIconAddress,
                flavorText);
            return RegisterWeaponPackage(manifest, requestNativeRegistration, out receipt);
        }
        catch (Exception ex)
        {
            receipt = TaintedWeaponPackageImportReceipt.DeniedInvalidManifest("manifest-invalid:" + ex.GetType().Name + ":" + ex.Message);
            return false;
        }
    }

    public static bool RegisterWeaponPackage(
        TaintedWeaponPackageManifest manifest,
        bool requestNativeRegistration,
        out TaintedWeaponPackageImportReceipt receipt)
    {
        try
        {
            return TaintedWeaponPackageImporter.Register(manifest, requestNativeRegistration, out receipt);
        }
        catch (Exception ex)
        {
            receipt = TaintedWeaponPackageImportReceipt.DeniedInvalidManifest("package-importer-failed:" + ex.GetType().Name + ":" + ex.Message);
            return false;
        }
    }

    public static bool TryGetCapabilityReceipt(out TaintedWeaponsCapabilityReceipt? receipt)
    {
        receipt = Plugin.Instance?.CapabilityReceipt;
        return receipt != null;
    }

    public static bool RegisterNativeWeaponTemplate(
        TaintedWeaponNativeItemRegistrationRequest request,
        out TaintedWeaponNativeItemRegistrationReceipt receipt)
    {
        if (request == null)
        {
            receipt = TaintedWeaponNativeItemRegistrationReceipt.DeniedWithoutDefinition(
                new TaintedWeaponNativeItemRegistrationRequest(
                    "invalid",
                    "invalid",
                    TaintedWeaponNativeItemRegistrationRequest.WeaponItemTemplateCloneProfile),
                "request-null",
                "A native item registration request is required.");
            return false;
        }

        Plugin? plugin = Plugin.Instance;
        if (plugin == null || !plugin.ImporterLifecycleOpen)
        {
            lock (PendingDefinitions)
            {
                TaintedWeaponNativeItemRegistrationRequest? existing = PendingNativeItemRequests.FirstOrDefault(candidate =>
                    string.Equals(candidate.RegistryKey, request.RegistryKey, StringComparison.OrdinalIgnoreCase));
                if (existing != null && !string.Equals(existing.CanonicalRequestKey, request.CanonicalRequestKey, StringComparison.Ordinal))
                {
                    receipt = TaintedWeaponNativeItemRegistrationReceipt.DeniedWithoutDefinition(
                        request,
                        "queued-request-definition-collision",
                        "A different native item request is already queued for this package/weapon identity.");
                    return false;
                }

                if (existing == null)
                {
                    PendingNativeItemRequests.Add(request);
                }
            }

            receipt = plugin == null
                ? TaintedWeaponNativeItemRegistrationReceipt.QueuedBeforeFramework(request)
                : TaintedWeaponNativeItemRegistrationReceipt.QueuedBeforeImporterLifecycle(request);
            return true;
        }

        return plugin.RegisterNativeWeaponTemplate(request, out receipt);
    }

    public static bool TryGetNativeItemRegistrarStatus(out TaintedWeaponNativeItemRegistrarStatus? status)
    {
        status = Plugin.Instance?.NativeItemRegistrarStatus;
        return status != null;
    }

    public static bool TryGetNativeItemRegistrationReceipt(
        string packageId,
        string weaponId,
        out TaintedWeaponNativeItemRegistrationReceipt? receipt)
    {
        receipt = null;
        if (string.IsNullOrWhiteSpace(packageId) || string.IsNullOrWhiteSpace(weaponId))
        {
            return false;
        }

        return Plugin.Instance?.TryGetNativeItemRegistrationReceipt(
            TaintedWeaponsIdentityPolicy.CanonicalRegistryKey(packageId, weaponId),
            out receipt) == true;
    }

    internal static void FlushPendingDefinitions(Plugin plugin)
    {
        TaintedWeaponDefinition[] pending;
        lock (PendingDefinitions)
        {
            pending = PendingDefinitions.ToArray();
            PendingDefinitions.Clear();
            PendingIdentityReceipts.Clear();
            PendingIdentityPolicy.Clear();
        }

        foreach (TaintedWeaponDefinition definition in pending)
        {
            plugin.RegisterWeaponDefinition(definition, out _);
        }
    }

    internal static void FlushPendingNativeItemRegistrations(Plugin plugin)
    {
        TaintedWeaponNativeItemRegistrationRequest[] pending;
        lock (PendingDefinitions)
        {
            pending = PendingNativeItemRequests.ToArray();
            PendingNativeItemRequests.Clear();
        }

        foreach (TaintedWeaponNativeItemRegistrationRequest request in pending)
        {
            plugin.RegisterNativeWeaponTemplate(request, out _);
        }
    }
}

internal sealed class TaintedWeaponsIdentityPolicy
{
    internal const string RuntimeAddressPrefix = "mod://kane.tgfoa.tainted-weapons/equipped-prototype/";

    private readonly object _gate = new();
    private readonly Dictionary<string, TaintedWeaponsIdentityReceipt> _receiptsByPublicId = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _ownersByCustomTemplateGuid = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _ownersByTemplateName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _ownersByRegistryKey = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _ownersByRuntimeAddress = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _ownersByMeshKey = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _ownersByMaterialKeyPrefix = new(StringComparer.OrdinalIgnoreCase);

    internal bool TryReserve(TaintedWeaponDefinition definition, out TaintedWeaponsIdentityReceipt receipt, out string reason)
    {
        receipt = CreateReceipt(definition);

        lock (_gate)
        {
            if (_receiptsByPublicId.TryGetValue(receipt.PublicId, out TaintedWeaponsIdentityReceipt existing))
            {
                if (string.Equals(existing.DefinitionHash, receipt.DefinitionHash, StringComparison.OrdinalIgnoreCase))
                {
                    receipt = existing;
                    reason = "already-reserved-same-definition";
                    return true;
                }

                reason = $"public-id-definition-collision; publicId={receipt.PublicId}; hashAlgorithm=sha256; existingDefinitionHash={existing.DefinitionHash}; candidateDefinitionHash={receipt.DefinitionHash}; existingCanonicalDefinitionHash={existing.CanonicalDefinitionHash}; candidateCanonicalDefinitionHash={receipt.CanonicalDefinitionHash}";
                return false;
            }

            if (HasDifferentOwner(_ownersByCustomTemplateGuid, receipt.CustomTemplateGuid, receipt.PublicId, out string customGuidOwner))
            {
                reason = $"custom-template-guid-collision; guid={receipt.CustomTemplateGuid}; owner={customGuidOwner}; candidate={receipt.PublicId}";
                return false;
            }

            if (HasDifferentOwner(_ownersByTemplateName, receipt.CustomTemplateName, receipt.PublicId, out string templateNameOwner))
            {
                reason = $"template-name-collision; templateName={receipt.CustomTemplateName}; owner={templateNameOwner}; candidate={receipt.PublicId}";
                return false;
            }

            if (HasDifferentOwner(_ownersByRegistryKey, receipt.RegistryKey, receipt.PublicId, out string registryOwner))
            {
                reason = $"registry-key-collision; registryKey={receipt.RegistryKey}; owner={registryOwner}; candidate={receipt.PublicId}";
                return false;
            }

            if (HasDifferentOwner(_ownersByRuntimeAddress, receipt.RuntimePrototypeAddress, receipt.PublicId, out string runtimeOwner))
            {
                reason = $"runtime-prototype-address-collision; runtimeAddress={receipt.RuntimePrototypeAddress}; owner={runtimeOwner}; candidate={receipt.PublicId}";
                return false;
            }

            if (HasDifferentOwner(_ownersByMeshKey, receipt.MeshKey, receipt.PublicId, out string meshOwner))
            {
                reason = $"mesh-key-collision; meshKey={receipt.MeshKey}; owner={meshOwner}; candidate={receipt.PublicId}";
                return false;
            }

            if (HasDifferentOwner(_ownersByMaterialKeyPrefix, receipt.MaterialKeyPrefix, receipt.PublicId, out string materialOwner))
            {
                reason = $"material-key-prefix-collision; materialKeyPrefix={receipt.MaterialKeyPrefix}; owner={materialOwner}; candidate={receipt.PublicId}";
                return false;
            }

            _receiptsByPublicId.Add(receipt.PublicId, receipt);
            _ownersByCustomTemplateGuid.Add(receipt.CustomTemplateGuid, receipt.PublicId);
            _ownersByTemplateName.Add(receipt.CustomTemplateName, receipt.PublicId);
            _ownersByRegistryKey.Add(receipt.RegistryKey, receipt.PublicId);
            _ownersByRuntimeAddress.Add(receipt.RuntimePrototypeAddress, receipt.PublicId);
            _ownersByMeshKey.Add(receipt.MeshKey, receipt.PublicId);
            _ownersByMaterialKeyPrefix.Add(receipt.MaterialKeyPrefix, receipt.PublicId);
        }

        reason = "reserved";
        return true;
    }

    internal void Clear()
    {
        lock (_gate)
        {
            _receiptsByPublicId.Clear();
            _ownersByCustomTemplateGuid.Clear();
            _ownersByTemplateName.Clear();
            _ownersByRegistryKey.Clear();
            _ownersByRuntimeAddress.Clear();
            _ownersByMeshKey.Clear();
            _ownersByMaterialKeyPrefix.Clear();
        }
    }

    internal static string CanonicalRegistryKey(string packageId, string weaponId)
    {
        return CanonicalPublicId(packageId) + ":" + CanonicalPublicId(weaponId);
    }

    internal static string EscapeIdentitySegment(string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(NormalizeIdentity(value));
        var builder = new StringBuilder(bytes.Length);
        foreach (byte b in bytes)
        {
            char ch = (char)b;
            if ((ch >= 'a' && ch <= 'z') ||
                (ch >= 'A' && ch <= 'Z') ||
                (ch >= '0' && ch <= '9') ||
                ch == '.' ||
                ch == '-' ||
                ch == '_')
            {
                builder.Append(ch);
            }
            else
            {
                builder.Append('%');
                builder.Append(b.ToString("X2", CultureInfo.InvariantCulture));
            }
        }

        return builder.Length == 0 ? "_" : builder.ToString();
    }

    private static TaintedWeaponsIdentityReceipt CreateReceipt(TaintedWeaponDefinition definition)
    {
        string canonicalPackageId = CanonicalPublicId(definition.PackageId);
        string canonicalWeaponId = CanonicalPublicId(definition.WeaponId);
        string publicId = canonicalPackageId + ":" + canonicalWeaponId;
        string customTemplateGuid = CanonicalPublicId(definition.CustomTemplateGuid);
        string sourceTemplateGuid = CanonicalPublicId(definition.SourceTemplateGuid);
        string customTemplateName = NormalizeIdentity(definition.CustomTemplateName);
        string bundlePath = CanonicalPath(Path.Combine(definition.PackageRootDirectory, definition.BundleFileName));
        string equippedPrefabAssetPath = NormalizeIdentity(definition.EquippedPrefabAssetPath);
        string definitionHash = HashDefinition(
            ("contractVersion", "1"),
            ("publicId", publicId),
            ("customTemplateGuid", customTemplateGuid),
            ("sourceTemplateGuid", sourceTemplateGuid),
            ("customTemplateName", customTemplateName),
            ("displayName", NormalizeIdentity(definition.DisplayName)),
            ("description", NormalizeIdentity(definition.Description)),
            ("bundlePath", bundlePath),
            ("equippedPrefabAssetPath", equippedPrefabAssetPath));
        string hashSuffix = definitionHash.Substring(0, 12);
        string escapedPackage = EscapeIdentitySegment(canonicalPackageId);
        string escapedWeapon = EscapeIdentitySegment(canonicalWeaponId);
        string registryKey = publicId;
        string runtimePrototypeAddress = RuntimeAddressPrefix + escapedPackage + "/" + escapedWeapon + "-" + hashSuffix;
        string meshKey = runtimePrototypeAddress + "/mesh";
        string materialKeyPrefix = runtimePrototypeAddress + "/material-";
        string[] reservedKeys =
        {
            "publicId:" + publicId,
            "customTemplateGuid:" + customTemplateGuid,
            "customTemplateName:" + customTemplateName,
            "registryKey:" + registryKey,
            "runtimePrototypeAddress:" + runtimePrototypeAddress,
            "meshKey:" + meshKey,
            "materialKeyPrefix:" + materialKeyPrefix
        };

        return new TaintedWeaponsIdentityReceipt(
            publicId,
            canonicalPackageId,
            canonicalWeaponId,
            customTemplateGuid,
            sourceTemplateGuid,
            customTemplateName,
            definitionHash,
            registryKey,
            runtimePrototypeAddress,
            meshKey,
            materialKeyPrefix,
            reservedKeys);
    }

    private static string CanonicalPublicId(string value)
    {
        return NormalizeIdentity(value).ToLowerInvariant();
    }

    private static string NormalizeIdentity(string value)
    {
        return (value ?? string.Empty).Trim().Normalize(NormalizationForm.FormKC);
    }

    private static string CanonicalPath(string path)
    {
        try
        {
            return Path.GetFullPath(path).Trim().Normalize(NormalizationForm.FormKC);
        }
        catch
        {
            return NormalizeIdentity(path);
        }
    }

    private static string HashDefinition(params (string Name, string Value)[] fields)
    {
        var builder = new StringBuilder();
        foreach ((string name, string value) in fields)
        {
            builder.Append(name.Length.ToString(CultureInfo.InvariantCulture));
            builder.Append(':');
            builder.Append(name);
            builder.Append('=');
            builder.Append((value ?? string.Empty).Length.ToString(CultureInfo.InvariantCulture));
            builder.Append(':');
            builder.Append(value ?? string.Empty);
            builder.Append('\n');
        }

        using SHA256 sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
        var hex = new StringBuilder(hash.Length * 2);
        foreach (byte b in hash)
        {
            hex.Append(b.ToString("x2", CultureInfo.InvariantCulture));
        }

        return hex.ToString();
    }

    private static bool HasDifferentOwner(Dictionary<string, string> owners, string key, string publicId, out string existingOwner)
    {
        if (owners.TryGetValue(key, out existingOwner))
        {
            return !string.Equals(existingOwner, publicId, StringComparison.OrdinalIgnoreCase);
        }

        existingOwner = string.Empty;
        return false;
    }
}

internal sealed class TaintedWeaponFramework : IDisposable
{
    private const string LinkedEntitiesAccessType = "Awaken.ECS.Authoring.LinkedEntities.LinkedEntitiesAccess";
    private const int ProviderWorkerDispatchTimeoutMilliseconds = 2000;
    private const int VanillaEquippedComparisonActiveHandWaitFrameLimit = 300;
    private static readonly int[] RuntimeEcsProbeFrameDelays = { 1, 2, 5, 15, 45 };
    private static readonly PropertyInfo? CustomHeroClothesWeaponLayerProperty =
        AccessTools.Property(typeof(CustomHeroClothes), "WeaponLayer");
    private static readonly PropertyInfo? CustomHeroClothesLightRenderLayerMaskProperty =
        AccessTools.Property(typeof(CustomHeroClothes), "LightRenderLayerMask");
    private static EquippedFppPresentationContract? _visibleVanillaTwoHandedFppContract;

    private readonly ManualLogSource _log;
    private readonly TaintedWeaponsIdentityPolicy _identityPolicy;
    private readonly TaintedWeaponProviderOwnerLedger _providerOwnerLedger;
    private readonly TaintedWeaponAssetResolver _assetResolver;
    private readonly TaintedWeaponDrakePrototypeAdapter _drakePrototypeAdapter;
    private readonly TaintedWeaponAssetHandleBridge _assetHandleBridge;
    private readonly TaintedWeaponNativeItemRegistrar _nativeItemRegistrar;
    private readonly Dictionary<string, TaintedWeaponRecord> _records = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TaintedWeaponRecord> _recordsByTemplateGuid = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TaintedWeaponRecord> _recordsByTemplateName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TaintedWeaponRecord> _recordsByRuntimeAddress = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _equippedRuntimeObservationCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _inventoryPreviewObservationCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<WeakReference<CharacterHandBase>> _registeredEquippedHands = new();
    private readonly HashSet<string> _registeredEquippedPlacementPendingKeys = new(StringComparer.OrdinalIgnoreCase);
    private int _lastRegisteredPlacementRetrofitContractFrame = -1;
    private readonly List<TaintedWeaponFrameworkStage> _stages = new();
    private readonly List<string> _lifecycleEvents = new();
    private readonly HashSet<string> _lifecycleFixtureKeys = new(StringComparer.OrdinalIgnoreCase);
    private readonly int _unityThreadId;
    private TaintedWeaponsCapabilityReceipt _capabilityReceipt = TaintedWeaponsCapabilityReceipt.Blocked("capability-receipt-not-set");

    private GameObject? _runtimePrototypeStorageRoot;
    private int _lifecycleEventSequence;

    internal TaintedWeaponFramework(ManualLogSource log)
    {
        _log = log;
        _identityPolicy = new TaintedWeaponsIdentityPolicy();
        _providerOwnerLedger = new TaintedWeaponProviderOwnerLedger();
        _assetResolver = new TaintedWeaponAssetResolver(log, _providerOwnerLedger);
        _assetHandleBridge = new TaintedWeaponAssetHandleBridge(log);
        _drakePrototypeAdapter = new TaintedWeaponDrakePrototypeAdapter(log, _assetHandleBridge);
        _unityThreadId = Environment.CurrentManagedThreadId;
        _nativeItemRegistrar = new TaintedWeaponNativeItemRegistrar(log, _unityThreadId);
        AddStage("definition-registry", "custom weapons submit immutable definitions; identity policy reserves canonical keys before registration/equip proceeds");
        AddStage("asset-resolver", "load cooked bundles and resolve named mesh/material/texture/clip assets before Drake registration");
        AddStage("native-item-registrar", "explicit weapon-only requests queue until TemplatesProvider readiness, then validate identity/profile/collisions before one private loader insertion");
        AddStage("drake-prototype-rebinding", "clone the native CharacterHandBase prefab and rebind DrakeMeshRenderer mesh/material references to package assets");
        AddStage("vanilla-equip-lifecycle", "let ItemEquip and SetUnityRepresentation instantiate, attach, register, hide/show, and tear down Drake entities");
        AddStage("inventory-preview-clone", "observe FoA's native CustomHeroClothes preview clone and repair only registered preview owner layer/mask mismatches");
        AddStage("animation-adapter", "map accepted moveset clips into native hero animation mapping only after runtime target proof");
        AddStage("combat-executor", "bind damage windows and traces after animation targets are proven");
        AddStage("diagnostics-and-rollback", "capture template, asset, Drake authoring, ECS readiness, animation, combat, and teardown evidence");
        AddStage("provider-l0-l12-harness", "write fail-closed Gate 6 provider lifetime receipt rows until W0 and L0-L12 runtime fixtures are explicitly executed");
    }

    internal IReadOnlyList<TaintedWeaponFrameworkStage> Stages => _stages;

    internal TaintedWeaponsCapabilityReceipt CapabilityReceipt => _capabilityReceipt;

    internal void SetCapabilityReceipt(TaintedWeaponsCapabilityReceipt receipt)
    {
        _capabilityReceipt = receipt ?? TaintedWeaponsCapabilityReceipt.Blocked("capability-receipt-null");
        _log.LogInfo($"{Plugin.PluginName} capability receipt; {_capabilityReceipt}");
    }

    internal void SetNativeItemRegistrarReadinessHookStatus(bool available, string reason)
    {
        _nativeItemRegistrar.SetReadinessHookStatus(available, reason);
        _log.LogInfo($"{Plugin.PluginName} native item registrar status; {_nativeItemRegistrar.GetStatus()}");
    }

    internal TaintedWeaponNativeItemRegistrarStatus NativeItemRegistrarStatus => _nativeItemRegistrar.GetStatus();

    internal bool HasPendingNativeItemRegistrations => _nativeItemRegistrar.HasPending;

    internal TaintedWeaponNativeItemRegistrationReceipt RegisterNativeWeaponTemplate(
        TaintedWeaponNativeItemRegistrationRequest request)
    {
        if (!_records.TryGetValue(request.RegistryKey, out TaintedWeaponRecord record))
        {
            return TaintedWeaponNativeItemRegistrationReceipt.DeniedWithoutDefinition(
                request,
                "presentation-definition-not-registered",
                "RegisterWeaponDefinition must accept the same package/weapon identity before native registration is requested.");
        }

        return _nativeItemRegistrar.Register(request, record);
    }

    internal void ProcessNativeItemRegistrarQueue()
    {
        _nativeItemRegistrar.ProcessPending();
    }

    internal bool TryGetNativeItemRegistrationReceipt(
        string registryKey,
        out TaintedWeaponNativeItemRegistrationReceipt? receipt)
    {
        return _nativeItemRegistrar.TryGetReceipt(registryKey, out receipt);
    }

    internal TaintedWeaponRegistrationResult RegisterDefinition(TaintedWeaponDefinition definition)
    {
        if (!_identityPolicy.TryReserve(definition, out TaintedWeaponsIdentityReceipt identityReceipt, out string identityReason))
        {
            var rejected = new TaintedWeaponRegistrationResult(definition.RegistryKey, false, false, false, "identity-collision", identityReason);
            _log.LogWarning($"{Plugin.PluginName} framework registration rejected; {rejected}");
            return rejected;
        }

        if (!CapabilitiesReady(out string capabilityReason))
        {
            var blocked = new TaintedWeaponRegistrationResult(identityReceipt.RegistryKey, false, false, false, "capability-blocked", capabilityReason, identityReceipt);
            _log.LogWarning($"{Plugin.PluginName} framework registration blocked; {blocked}");
            return blocked;
        }

        if (_records.ContainsKey(identityReceipt.RegistryKey))
        {
            TaintedWeaponRecord existing = _records[identityReceipt.RegistryKey];
            return new TaintedWeaponRegistrationResult(identityReceipt.RegistryKey, true, existing.AssetResolved, existing.DrakeReady, "already-registered", existing.Details, existing.IdentityReceipt);
        }

        bool assetResolved = _assetResolver.TryResolveEquippedPrefab(definition, out GameObject? customAssetPrefab, out string assetReason);
        TaintedWeaponCustomVisualSource? customVisualSource = null;
        TaintedWeaponCustomVisualReport customVisualReport = assetResolved && customAssetPrefab != null
            ? TaintedWeaponCustomVisualSource.TryCreate(definition, customAssetPrefab, out customVisualSource)
            : TaintedWeaponCustomVisualReport.Blocked("asset-unresolved", assetReason);

        bool readyForRuntimePrototype = assetResolved && customVisualReport.Ready && customVisualSource != null;
        var record = new TaintedWeaponRecord(definition, identityReceipt, assetResolved, readyForRuntimePrototype, customVisualReport.ToString(), customAssetPrefab, customVisualSource);
        _records.Add(identityReceipt.RegistryKey, record);
        _recordsByTemplateGuid[identityReceipt.CustomTemplateGuid] = record;
        _recordsByTemplateName[identityReceipt.CustomTemplateName] = record;
        _recordsByRuntimeAddress[identityReceipt.RuntimePrototypeAddress] = record;

        string reason = readyForRuntimePrototype ? "registered-waiting-native-equipped-source" : "registered-blocked-" + customVisualReport.Reason;
        var result = new TaintedWeaponRegistrationResult(identityReceipt.RegistryKey, true, assetResolved, readyForRuntimePrototype, reason, $"identityReservation={identityReason}; runtimeAddress={identityReceipt.RuntimePrototypeAddress}; {customVisualReport}", identityReceipt);
        _log.LogInfo($"{Plugin.PluginName} framework registration; {result}");
        return result;
    }

    internal bool TryRedirectEquippedVisual(ItemEquip itemEquip, ARAssetReference? sourceReference, out ARAssetReference? replacement, out string message)
    {
        replacement = null;
        message = "not-custom-weapon";
        if (!CapabilitiesReady(out string capabilityReason))
        {
            message = "capability-blocked:" + capabilityReason;
            _log.LogWarning($"{Plugin.PluginName} equipped visual redirect blocked; reason={message}");
            return false;
        }

        if (itemEquip?.Item?.Template == null)
        {
            return false;
        }

        string templateGuid = itemEquip.Item.Template.GUID ?? string.Empty;
        string templateName = itemEquip.Item.Template.name ?? string.Empty;
        if (!TryFindRecordByTemplateIdentity(templateGuid, templateName, out TaintedWeaponRecord? record) || record == null)
        {
            return false;
        }

        if (!record.DrakeReady || record.CustomVisualSource == null)
        {
            message = "custom-visual-source-not-ready:" + record.Details;
            _log.LogWarning($"{Plugin.PluginName} equipped visual redirect blocked; key={record.Definition.RegistryKey}; reason={message}");
            return false;
        }

        if (sourceReference == null || !sourceReference.IsSet)
        {
            message = "native-source-reference-missing";
            _log.LogWarning($"{Plugin.PluginName} equipped visual redirect blocked; key={record.Definition.RegistryKey}; reason={message}");
            return false;
        }

        string sourceKey = SafeRuntimeKey(sourceReference);
        if (string.Equals(sourceKey, record.RuntimePrototypeAddress, StringComparison.OrdinalIgnoreCase))
        {
            message = "already-framework-reference";
            return false;
        }

        record.RememberNativeSource(sourceReference);
        replacement = new ARAssetReference(record.RuntimePrototypeAddress);
        message = $"redirected; key={record.Definition.RegistryKey}; source={DescribeReference(sourceReference)}; target={record.RuntimePrototypeAddress}";
        record.RecordRedirect();
        AddLifecycleEvent("equip-redirect", message);
        _log.LogInfo($"{Plugin.PluginName} equipped visual redirect; {message}");
        return true;
    }

    internal bool TryCreateEquippedPrototypeHandle(ARAssetReference reference, out ARAsyncOperationHandle<GameObject> handle, out string message)
    {
        handle = default;
        message = "not-framework-reference";
        if (!CapabilitiesReady(out string capabilityReason))
        {
            message = "capability-blocked:" + capabilityReason;
            _log.LogWarning($"{Plugin.PluginName} equipped prototype handle blocked; reason={message}");
            return false;
        }

        if (reference == null)
        {
            return false;
        }

        string runtimeKey = SafeRuntimeKey(reference);
        if (!_recordsByRuntimeAddress.TryGetValue(runtimeKey, out TaintedWeaponRecord record))
        {
            return false;
        }

        if (record.TryGetRuntimePrototype(out GameObject? existingPrototype) && existingPrototype != null)
        {
            return CreateCompletedPrototypeHandle(record, reference, existingPrototype, "cached-framework-prototype", out handle, out message);
        }

        if (_drakePrototypeAdapter.TryBuildRuntimePrototype(record, GetOrCreateRuntimePrototypeStorageRoot(), out GameObject? prototype, out string buildReason) && prototype != null)
        {
            record.RuntimePrototype = prototype;
            return CreateCompletedPrototypeHandle(record, reference, prototype, "built-framework-prototype", out handle, out message);
        }

        if (record.TryLoadNativeSource(out GameObject? nativeSourcePrefab, out string nativeSourceReason) && nativeSourcePrefab != null)
        {
            bool created = CreateCompletedPrototypeHandle(record, reference, nativeSourcePrefab, "fallback-native-source", out handle, out message);
            _log.LogWarning($"{Plugin.PluginName} equipped prototype fallback; key={record.Definition.RegistryKey}; buildReason={buildReason}; nativeSourceReason={nativeSourceReason}; {message}");
            return created;
        }

        message = $"framework-prototype-unavailable; key={record.Definition.RegistryKey}; buildReason={buildReason}; nativeSourceReason={nativeSourceReason}";
        _log.LogWarning($"{Plugin.PluginName} equipped prototype blocked; {message}");
        return false;
    }

    internal IEnumerable<string> DescribeStageRows()
    {
        return _stages.Select((stage, index) => $"index={index.ToString(CultureInfo.InvariantCulture)}; id={stage.Id}; rule={stage.Rule}");
    }

    public void Dispose()
    {
        ClearVisibleVanillaFppContractState("framework-dispose");
        foreach (TaintedWeaponRecord record in _records.Values)
        {
            record.Dispose();
        }

        _records.Clear();
        _recordsByTemplateGuid.Clear();
        _recordsByTemplateName.Clear();
        _recordsByRuntimeAddress.Clear();
        _equippedRuntimeObservationCounts.Clear();
        _inventoryPreviewObservationCounts.Clear();
        _registeredEquippedHands.Clear();
        _registeredEquippedPlacementPendingKeys.Clear();
        _identityPolicy.Clear();
        _nativeItemRegistrar.Dispose();
        TaintedWeaponAddressableAssetRegistry.Clear();
        if (_runtimePrototypeStorageRoot != null)
        {
            Object.Destroy(_runtimePrototypeStorageRoot);
            _runtimePrototypeStorageRoot = null;
        }

        _assetResolver.Dispose();
    }

    private bool CreateCompletedPrototypeHandle(TaintedWeaponRecord record, ARAssetReference reference, GameObject prototype, string route, out ARAsyncOperationHandle<GameObject> handle, out string message)
    {
        AsyncOperationHandle<GameObject> completed = Addressables.ResourceManager.CreateCompletedOperation<GameObject>(prototype, null);
        handle = new ARAsyncOperationHandle<GameObject>(completed);
        _assetHandleBridge.AssignBackingHandle(reference, completed);
        message = $"route={route}; address={SafeRuntimeKey(reference)}; prototype={prototype.name}; activeSelf={prototype.activeSelf}";
        record.RecordPrototypeHandle(route);
        AddLifecycleEvent("prototype-handle", message);
        _log.LogInfo($"{Plugin.PluginName} equipped prototype handle; {message}");
        return true;
    }

    internal void RecordLifecycleSceneTransition(string previousSceneName, string sceneName)
    {
        if (string.Equals(previousSceneName, sceneName, StringComparison.Ordinal))
        {
            return;
        }

        AddLifecycleEvent("scene-change", "from=" + previousSceneName + "; to=" + sceneName);
        AddLifecycleEvent("scene-transition", "sceneBefore=" + previousSceneName + "; sceneAfter=" + sceneName);
        ClearVisibleVanillaFppContractState("scene-transition:" + previousSceneName + "->" + sceneName);
    }

    internal void RecordDrakeLoadingEvent(TaintedWeaponDrakeLoadingEvent eventData)
    {
        if (!eventData.HasRuntimeKey || !TryFindRecordByRuntimeAssetKey(eventData.RuntimeKey, out TaintedWeaponRecord? record))
        {
            return;
        }

        record!.RecordDrakeLoadingEvent(eventData);
        AddLifecycleEvent("drake-" + eventData.AssetKind + "-" + eventData.Phase, eventData.ToString());
    }

    internal void ObserveEquippedRuntimeView(ItemEquip itemEquip, CharacterHandBase? handBase, string route)
    {
        if (itemEquip?.Item?.Template == null)
        {
            return;
        }

        ObserveEquippedRuntimeView(itemEquip.Item.Template.GUID ?? string.Empty, itemEquip.Item.Template.name ?? string.Empty, handBase, route);
    }

    internal bool ShouldObserveEquippedRuntime(ItemEquip itemEquip, CharacterHandBase? handBase)
    {
        if (itemEquip?.Item?.Template == null)
        {
            return false;
        }

        return ShouldObserveEquippedRuntime(
            itemEquip.Item.Template.GUID ?? string.Empty,
            itemEquip.Item.Template.name ?? string.Empty,
            handBase);
    }

    internal void ObserveCharacterHandMounted(CharacterHandBase handBase, string route)
    {
        if (handBase == null || handBase.Item?.Template == null)
        {
            return;
        }

        ObserveEquippedRuntimeView(handBase.Item.Template.GUID ?? string.Empty, handBase.Item.Template.name ?? string.Empty, handBase, route);
    }

    internal bool ShouldObserveCharacterHandMounted(CharacterHandBase? handBase)
    {
        if (handBase == null || handBase.Item?.Template == null)
        {
            return false;
        }

        return ShouldObserveEquippedRuntime(
            handBase.Item.Template.GUID ?? string.Empty,
            handBase.Item.Template.name ?? string.Empty,
            handBase);
    }

    internal IEnumerator ObserveEquippedRuntimeEcsReadiness(ItemEquip itemEquip, CharacterHandBase handBase, string route)
    {
        if (itemEquip?.Item?.Template == null)
        {
            yield break;
        }

        string templateGuid = itemEquip.Item.Template.GUID ?? string.Empty;
        string templateName = itemEquip.Item.Template.name ?? string.Empty;
        IEnumerator routine = ObserveRuntimeEcsReadiness(templateGuid, templateName, handBase, route);
        while (routine.MoveNext())
        {
            yield return routine.Current;
        }
    }

    internal IEnumerator ObserveCharacterHandRuntimeEcsReadiness(CharacterHandBase handBase, string route)
    {
        if (handBase == null || handBase.Item?.Template == null)
        {
            yield break;
        }

        string templateGuid = handBase.Item.Template.GUID ?? string.Empty;
        string templateName = handBase.Item.Template.name ?? string.Empty;
        IEnumerator routine = ObserveRuntimeEcsReadiness(templateGuid, templateName, handBase, route);
        while (routine.MoveNext())
        {
            yield return routine.Current;
        }
    }

    internal void ObserveInventoryPreviewRuntimeView(ItemEquip itemEquip, CharacterHandBase? handBase, CustomHeroClothes? previewOwner, string route)
    {
        if (itemEquip?.Item?.Template == null)
        {
            return;
        }

        string templateGuid = itemEquip.Item.Template.GUID ?? string.Empty;
        string templateName = itemEquip.Item.Template.name ?? string.Empty;
        bool registered = TryFindRecordByTemplateIdentity(templateGuid, templateName, out TaintedWeaponRecord? record) && record != null;
        bool vanillaComparison = !registered && IsVanillaTwoHandedPreviewCandidate(templateGuid, templateName);
        if (!registered && !vanillaComparison)
        {
            return;
        }

        string observationKey = (registered ? "registered:" + record!.Definition.RegistryKey : "vanilla:" + templateGuid + ":" + templateName) + ":" + route + ":view";
        if (!ReserveInventoryPreviewObservation(observationKey, maxObservations: 4))
        {
            return;
        }

        if (handBase == null)
        {
            string missingMessage = $"route={route}; key={(registered ? record!.Definition.RegistryKey : "vanilla-two-handed")}; template={templateName}; templateGuid={templateGuid}; reason=preview-hand-missing; owner={DescribeInventoryPreviewOwnerProof(previewOwner, null)}";
            AddLifecycleEvent(registered ? "inventory-preview-route-missing" : "vanilla-inventory-preview-route-missing", missingMessage);
            if (registered)
            {
                record!.RecordRuntimeView("inventory-preview-missing:" + missingMessage, 0);
                _log.LogWarning($"{Plugin.PluginName} inventory/equipment preview route missing; {missingMessage}");
            }
            else
            {
                _log.LogInfo($"{Plugin.PluginName} vanilla inventory/equipment preview route missing; {missingMessage}");
            }

            return;
        }

        int repairCount = registered ? NormalizeRegisteredInventoryPreviewUnityLayers(handBase, previewOwner) : 0;
        string message = DescribeInventoryPreviewRuntimeView(record, templateGuid, templateName, handBase, previewOwner, route, repairCount, registered);
        AddLifecycleEvent(registered ? "inventory-preview-route" : "vanilla-inventory-preview-route", message);
        if (registered)
        {
            record!.RecordRuntimeView("inventory-preview:" + message, repairCount);
            _log.LogInfo($"{Plugin.PluginName} inventory/equipment preview route; {message}");
        }
        else
        {
            _log.LogInfo($"{Plugin.PluginName} vanilla inventory/equipment preview route; {message}");
        }
    }

    internal IEnumerator ObserveInventoryPreviewRuntimeEcsReadiness(ItemEquip itemEquip, CharacterHandBase handBase, CustomHeroClothes? previewOwner, string route)
    {
        if (itemEquip?.Item?.Template == null)
        {
            yield break;
        }

        string templateGuid = itemEquip.Item.Template.GUID ?? string.Empty;
        string templateName = itemEquip.Item.Template.name ?? string.Empty;
        bool registered = TryFindRecordByTemplateIdentity(templateGuid, templateName, out TaintedWeaponRecord? record) && record != null;
        bool vanillaComparison = !registered && IsVanillaTwoHandedPreviewCandidate(templateGuid, templateName);
        if (!registered && !vanillaComparison)
        {
            yield break;
        }

        string observationKey = (registered ? "registered:" + record!.Definition.RegistryKey : "vanilla:" + templateGuid + ":" + templateName) + ":" + route + ":ecs";
        if (!ReserveInventoryPreviewObservation(observationKey, maxObservations: 4))
        {
            yield break;
        }

        int waitedFrames = 0;
        for (int attempt = 0; attempt < RuntimeEcsProbeFrameDelays.Length; attempt++)
        {
            int targetDelay = RuntimeEcsProbeFrameDelays[attempt];
            while (waitedFrames < targetDelay)
            {
                waitedFrames++;
                yield return null;
            }

            if (handBase == null)
            {
                string destroyedMessage = $"route={route}.InventoryPreviewDrakeEcs; key={(registered ? record!.Definition.RegistryKey : "vanilla-two-handed")}; template={templateName}; templateGuid={templateGuid}; attempt={attempt.ToString(CultureInfo.InvariantCulture)}; waitedFrames={waitedFrames.ToString(CultureInfo.InvariantCulture)}; ready=False; terminal=True; reason=preview-hand-destroyed-before-ecs-observation; owner={DescribeInventoryPreviewOwnerProof(previewOwner, null)}";
                AddLifecycleEvent(registered ? "inventory-preview-ecs-view" : "vanilla-inventory-preview-ecs-view", destroyedMessage);
                if (registered)
                {
                    record!.RecordRuntimeView("inventory-preview-ecs-terminal:" + destroyedMessage, 0);
                    _log.LogWarning($"{Plugin.PluginName} inventory/equipment preview Drake ECS view terminal; {destroyedMessage}");
                }
                else
                {
                    _log.LogWarning($"{Plugin.PluginName} vanilla inventory/equipment preview Drake ECS view terminal; {destroyedMessage}");
                }

                yield break;
            }

            string message = DescribeInventoryPreviewEcsView(record, templateGuid, templateName, handBase, previewOwner, route, attempt, waitedFrames, registered, out bool ready, out bool terminal);
            AddLifecycleEvent(registered ? "inventory-preview-ecs-view" : "vanilla-inventory-preview-ecs-view", message);
            if (ready)
            {
                if (registered)
                {
                    record!.RecordRuntimeView("inventory-preview-ecs-ready:" + message, 0);
                    _log.LogInfo($"{Plugin.PluginName} inventory/equipment preview Drake ECS view ready; {message}");
                }
                else
                {
                    _log.LogInfo($"{Plugin.PluginName} vanilla inventory/equipment preview Drake ECS view ready; {message}");
                }

                yield break;
            }

            if (terminal)
            {
                if (registered)
                {
                    record!.RecordRuntimeView("inventory-preview-ecs-terminal:" + message, 0);
                    _log.LogWarning($"{Plugin.PluginName} inventory/equipment preview Drake ECS view terminal; {message}");
                }
                else
                {
                    _log.LogWarning($"{Plugin.PluginName} vanilla inventory/equipment preview Drake ECS view terminal; {message}");
                }

                yield break;
            }

            if (attempt == RuntimeEcsProbeFrameDelays.Length - 1)
            {
                if (registered)
                {
                    record!.RecordRuntimeView("inventory-preview-ecs-not-ready:" + message, 0);
                    _log.LogWarning($"{Plugin.PluginName} inventory/equipment preview Drake ECS view not ready; {message}");
                }
                else
                {
                    _log.LogWarning($"{Plugin.PluginName} vanilla inventory/equipment preview Drake ECS view not ready; {message}");
                }

                yield break;
            }

            if (registered)
            {
                _log.LogInfo($"{Plugin.PluginName} inventory/equipment preview Drake ECS view waiting; {message}");
            }
            else
            {
                _log.LogInfo($"{Plugin.PluginName} vanilla inventory/equipment preview Drake ECS view waiting; {message}");
            }
        }
    }

    private bool ReserveEquippedRuntimeObservation(string key, int maxObservations)
    {
        if (!_equippedRuntimeObservationCounts.TryGetValue(key, out int count))
        {
            _equippedRuntimeObservationCounts[key] = 1;
            return true;
        }

        if (count >= maxObservations)
        {
            return false;
        }

        _equippedRuntimeObservationCounts[key] = count + 1;
        return true;
    }

    private bool ReserveInventoryPreviewObservation(string key, int maxObservations)
    {
        if (!_inventoryPreviewObservationCounts.TryGetValue(key, out int count))
        {
            _inventoryPreviewObservationCounts[key] = 1;
            return true;
        }

        if (count >= maxObservations)
        {
            return false;
        }

        _inventoryPreviewObservationCounts[key] = count + 1;
        return true;
    }

    private IEnumerator ObserveRuntimeEcsReadiness(string templateGuid, string templateName, CharacterHandBase handBase, string route)
    {
        bool registered = TryFindRecordByTemplateIdentity(templateGuid, templateName, out TaintedWeaponRecord? record) && record != null;
        bool vanillaComparison = !registered &&
            IsFppEquippedHand(handBase) &&
            IsVanillaTwoHandedPreviewCandidate(templateGuid, templateName);
        if (!registered && !vanillaComparison)
        {
            yield break;
        }

        if (registered && !IsFppEquippedHand(handBase))
        {
            string deferredMessage = $"route={route}.DrakeEcs; key={record!.Definition.RegistryKey}; registered=True; template={templateName}; templateGuid={templateGuid}; ready=False; terminal=False; reason=registered-hand-not-fpp-before-ecs-completion; observationConsumed=False; handActiveSelf={(handBase == null ? "missing" : Bool(handBase.gameObject.activeSelf))}; handActiveInHierarchy={(handBase == null ? "missing" : Bool(handBase.gameObject.activeInHierarchy))}; hand={(handBase == null ? "missing" : TransformPath(handBase.transform))}";
            AddLifecycleEvent("runtime-ecs-view-deferred", deferredMessage);
            _log.LogInfo($"{Plugin.PluginName} equipped runtime Drake ECS view deferred; {deferredMessage}");
            yield break;
        }

        if (registered)
        {
            int activeWaitFrames = 0;
            while (handBase != null &&
                   !IsActiveRuntimeHand(handBase) &&
                   activeWaitFrames < VanillaEquippedComparisonActiveHandWaitFrameLimit)
            {
                activeWaitFrames++;
                yield return null;
            }

            if (handBase == null || !IsActiveFppEquippedHand(handBase))
            {
                string deferredMessage = $"route={route}.DrakeEcs; key={record!.Definition.RegistryKey}; registered=True; template={templateName}; templateGuid={templateGuid}; ready=False; terminal=False; reason=registered-fpp-hand-inactive-before-ecs-completion; activeWaitFrames={activeWaitFrames.ToString(CultureInfo.InvariantCulture)}; observationConsumed=False; handActiveSelf={(handBase == null ? "missing" : Bool(handBase.gameObject.activeSelf))}; handActiveInHierarchy={(handBase == null ? "missing" : Bool(handBase.gameObject.activeInHierarchy))}; hand={(handBase == null ? "missing" : TransformPath(handBase.transform))}";
                AddLifecycleEvent("runtime-ecs-view-deferred", deferredMessage);
                _log.LogInfo($"{Plugin.PluginName} equipped runtime Drake ECS view deferred; {deferredMessage}");
                yield break;
            }

            TrackRegisteredEquippedHand(handBase);
        }

        if (vanillaComparison)
        {
            int activeWaitFrames = 0;
            while (handBase != null &&
                   !IsActiveRuntimeHand(handBase) &&
                   activeWaitFrames < VanillaEquippedComparisonActiveHandWaitFrameLimit)
            {
                activeWaitFrames++;
                yield return null;
            }

            if (handBase == null || !IsActiveRuntimeHand(handBase))
            {
                string deferredMessage = $"route={route}.DrakeEcs; key=vanilla-two-handed; registered=False; template={templateName}; templateGuid={templateGuid}; ready=False; terminal=False; reason=vanilla-fpp-hand-inactive-before-baseline; activeWaitFrames={activeWaitFrames.ToString(CultureInfo.InvariantCulture)}; observationConsumed=False; handActiveSelf={(handBase == null ? "missing" : Bool(handBase.gameObject.activeSelf))}; handActiveInHierarchy={(handBase == null ? "missing" : Bool(handBase.gameObject.activeInHierarchy))}; hand={(handBase == null ? "missing" : TransformPath(handBase.transform))}";
                AddLifecycleEvent("vanilla-runtime-ecs-view-deferred", deferredMessage);
                _log.LogInfo($"{Plugin.PluginName} vanilla equipped visual comparison Drake ECS view deferred; {deferredMessage}");
                yield break;
            }
        }

        string observationKey = (registered ? "registered:" + record!.Definition.RegistryKey : "vanilla:" + templateGuid + ":" + templateName) + ":" + route + ":ecs";
        if (!ReserveEquippedRuntimeObservation(observationKey, registered ? 8 : 4))
        {
            yield break;
        }

        int waitedFrames = 0;
        for (int attempt = 0; attempt < RuntimeEcsProbeFrameDelays.Length; attempt++)
        {
            int targetDelay = RuntimeEcsProbeFrameDelays[attempt];
            while (waitedFrames < targetDelay)
            {
                waitedFrames++;
                yield return null;
            }

            if (handBase == null)
            {
                string destroyedMessage = $"route={route}.DrakeEcs; key={(registered ? record!.Definition.RegistryKey : "vanilla-two-handed")}; registered={Bool(registered)}; template={templateName}; templateGuid={templateGuid}; attempt={attempt.ToString(CultureInfo.InvariantCulture)}; waitedFrames={waitedFrames.ToString(CultureInfo.InvariantCulture)}; ready=False; terminal=True; reason=hand-destroyed-before-ecs-observation";
                AddLifecycleEvent(registered ? "runtime-ecs-view" : "vanilla-runtime-ecs-view", destroyedMessage);
                if (registered)
                {
                    record!.RecordRuntimeView("ecs-terminal:" + destroyedMessage, 0);
                    _log.LogWarning($"{Plugin.PluginName} equipped runtime Drake ECS view terminal; {destroyedMessage}");
                }
                else
                {
                    _log.LogWarning($"{Plugin.PluginName} vanilla equipped visual comparison Drake ECS view terminal; {destroyedMessage}");
                }

                yield break;
            }

            string message = DescribeRuntimeEcsView(record, templateGuid, templateName, handBase, route, registered, attempt, waitedFrames, out bool ready, out bool terminal);
            AddLifecycleEvent(registered ? "runtime-ecs-view" : "vanilla-runtime-ecs-view", message);
            if (ready)
            {
                if (registered)
                {
                    record!.RecordRuntimeView("ecs-ready:" + message, 0);
                    _log.LogInfo($"{Plugin.PluginName} equipped runtime Drake ECS view ready; {message}");
                }
                else
                {
                    _log.LogInfo($"{Plugin.PluginName} vanilla equipped visual comparison Drake ECS view ready; {message}");
                    RefreshRegisteredEquippedPresentationAfterVisibleVanillaFppContract(route + ".VisibleVanillaFppContractCaptured");
                }

                yield break;
            }

            if (terminal)
            {
                if (registered)
                {
                    record!.RecordRuntimeView("ecs-terminal:" + message, 0);
                    _log.LogWarning($"{Plugin.PluginName} equipped runtime Drake ECS view terminal; {message}");
                }
                else
                {
                    _log.LogWarning($"{Plugin.PluginName} vanilla equipped visual comparison Drake ECS view terminal; {message}");
                }

                yield break;
            }

            if (attempt == RuntimeEcsProbeFrameDelays.Length - 1)
            {
                if (registered)
                {
                    record!.RecordRuntimeView("ecs-not-ready:" + message, 0);
                    _log.LogWarning($"{Plugin.PluginName} equipped runtime Drake ECS view not ready; {message}");
                }
                else
                {
                    _log.LogWarning($"{Plugin.PluginName} vanilla equipped visual comparison Drake ECS view not ready; {message}");
                }

                yield break;
            }

            if (registered)
            {
                _log.LogInfo($"{Plugin.PluginName} equipped runtime Drake ECS view waiting; {message}");
            }
            else
            {
                _log.LogInfo($"{Plugin.PluginName} vanilla equipped visual comparison Drake ECS view waiting; {message}");
            }
        }
    }

    private void ObserveEquippedRuntimeView(string templateGuid, string templateName, CharacterHandBase? handBase, string route)
    {
        bool registered = TryFindRecordByTemplateIdentity(templateGuid, templateName, out TaintedWeaponRecord? record) && record != null;
        bool vanillaComparison = !registered &&
            IsFppEquippedHand(handBase) &&
            IsVanillaTwoHandedPreviewCandidate(templateGuid, templateName);
        if (!registered && !vanillaComparison)
        {
            return;
        }

        if (registered && handBase != null)
        {
            TrackRegisteredEquippedHand(handBase);
        }

        if (vanillaComparison && !IsActiveRuntimeHand(handBase))
        {
            string deferredMessage = $"route={route}; key=vanilla-two-handed; registered=False; template={templateName}; templateGuid={templateGuid}; reason=vanilla-fpp-hand-inactive-before-runtime-view; observationConsumed=False; handActiveSelf={(handBase == null ? "missing" : Bool(handBase.gameObject.activeSelf))}; handActiveInHierarchy={(handBase == null ? "missing" : Bool(handBase.gameObject.activeInHierarchy))}; hand={(handBase == null ? "missing" : TransformPath(handBase.transform))}";
            AddLifecycleEvent("vanilla-runtime-view-deferred", deferredMessage);
            _log.LogInfo($"{Plugin.PluginName} vanilla equipped visual comparison runtime view deferred; {deferredMessage}");
            return;
        }

        string observationKey = (registered ? "registered:" + record!.Definition.RegistryKey : "vanilla:" + templateGuid + ":" + templateName) + ":" + route + ":view";
        if (!ReserveEquippedRuntimeObservation(observationKey, registered ? 8 : 4))
        {
            return;
        }

        if (handBase == null)
        {
            string missingMessage = $"route={route}; key={(registered ? record!.Definition.RegistryKey : "vanilla-two-handed")}; registered={Bool(registered)}; template={templateName}; templateGuid={templateGuid}; reason=weapon-instance-missing";
            AddLifecycleEvent(registered ? "runtime-view-missing" : "vanilla-runtime-view-missing", missingMessage);
            if (registered)
            {
                record!.RecordRuntimeView(missingMessage, 0);
                _log.LogWarning($"{Plugin.PluginName} equipped runtime view missing; {missingMessage}");
            }
            else
            {
                _log.LogWarning($"{Plugin.PluginName} vanilla equipped visual comparison runtime view missing; {missingMessage}");
            }

            return;
        }

        int activatedNodes = registered ? EnsureSpawnedPresentationHierarchyActive(handBase) : 0;
        string message = DescribeRuntimeView(record, templateGuid, templateName, handBase, route, activatedNodes, registered);
        AddLifecycleEvent(registered ? "runtime-view" : "vanilla-runtime-view", message);
        if (registered)
        {
            record!.RecordRuntimeView(message, activatedNodes);
            _log.LogInfo($"{Plugin.PluginName} equipped runtime view; {message}");
        }
        else
        {
            _log.LogInfo($"{Plugin.PluginName} vanilla equipped visual comparison runtime view; {message}");
        }
    }

    private bool ShouldObserveEquippedRuntime(string templateGuid, string templateName, CharacterHandBase? handBase)
    {
        if (TryFindRecordByTemplateIdentity(templateGuid, templateName, out _))
        {
            return IsFppEquippedHand(handBase);
        }

        return IsFppEquippedHand(handBase) &&
            IsVanillaTwoHandedPreviewCandidate(templateGuid, templateName);
    }

    private void TrackRegisteredEquippedHand(CharacterHandBase handBase)
    {
        if (!IsActiveFppEquippedHand(handBase))
        {
            return;
        }

        for (int i = _registeredEquippedHands.Count - 1; i >= 0; i--)
        {
            if (!_registeredEquippedHands[i].TryGetTarget(out CharacterHandBase? existing) || existing == null)
            {
                _registeredEquippedHands.RemoveAt(i);
                continue;
            }

            if (ReferenceEquals(existing, handBase))
            {
                return;
            }
        }

        _registeredEquippedHands.Add(new WeakReference<CharacterHandBase>(handBase));
    }

    private string MarkRegisteredEquippedPlacementPending(TaintedWeaponRecord record, string reason)
    {
        string key = record.Definition.RegistryKey;
        bool added = _registeredEquippedPlacementPendingKeys.Add(key);
        return "pendingNextRegisteredFppRenderer=True" +
            ",pendingRegisteredKey=" + CleanForInline(key) +
            ",pendingAdded=" + Bool(added) +
            ",pendingReason=" + CleanForInline(reason) +
            ",pendingContractFrame=" + (_visibleVanillaTwoHandedFppContract?.Frame.ToString(CultureInfo.InvariantCulture) ?? "-1") +
            ",pendingCount=" + _registeredEquippedPlacementPendingKeys.Count.ToString(CultureInfo.InvariantCulture);
    }

    private void RefreshRegisteredEquippedPresentationAfterVisibleVanillaFppContract(string route)
    {
        EquippedFppPresentationContract? contract = _visibleVanillaTwoHandedFppContract;
        if (contract == null)
        {
            return;
        }

        if (_lastRegisteredPlacementRetrofitContractFrame == contract.Frame)
        {
            return;
        }

        _lastRegisteredPlacementRetrofitContractFrame = contract.Frame;
        int pendingBefore = _registeredEquippedPlacementPendingKeys.Count;
        int inspected = 0;
        int readyCount = 0;
        int terminalCount = 0;
        int skippedCount = 0;
        int removedCount = 0;

        for (int i = _registeredEquippedHands.Count - 1; i >= 0; i--)
        {
            if (!_registeredEquippedHands[i].TryGetTarget(out CharacterHandBase? handBase) || handBase == null)
            {
                _registeredEquippedHands.RemoveAt(i);
                removedCount++;
                continue;
            }

            if (handBase.Item?.Template == null)
            {
                skippedCount++;
                continue;
            }

            if (!IsActiveFppEquippedHand(handBase))
            {
                skippedCount++;
                continue;
            }

            string templateGuid = handBase.Item.Template.GUID ?? string.Empty;
            string templateName = handBase.Item.Template.name ?? string.Empty;
            if (!TryFindRecordByTemplateIdentity(templateGuid, templateName, out TaintedWeaponRecord? record) || record == null)
            {
                skippedCount++;
                continue;
            }

            inspected++;
            string message = DescribeRuntimeEcsView(
                record,
                templateGuid,
                templateName,
                handBase,
                route,
                registered: true,
                attempt: 0,
                waitedFrames: 0,
                out bool ready,
                out bool terminal);
            AddLifecycleEvent("runtime-ecs-view", message);
            if (ready)
            {
                readyCount++;
                record.RecordRuntimeView("ecs-retrofit-ready:" + message, 0);
                _log.LogInfo($"{Plugin.PluginName} equipped runtime Drake ECS view ready; {message}");
                continue;
            }

            if (terminal)
            {
                terminalCount++;
                record.RecordRuntimeView("ecs-retrofit-terminal:" + message, 0);
                _log.LogWarning($"{Plugin.PluginName} equipped runtime Drake ECS view terminal; {message}");
                continue;
            }

            skippedCount++;
            record.RecordRuntimeView("ecs-retrofit-not-ready:" + message, 0);
            _log.LogWarning($"{Plugin.PluginName} equipped runtime Drake ECS view not ready; {message}");
        }

        string summary = "route=" + route +
            "; contractFrame=" + contract.Frame.ToString(CultureInfo.InvariantCulture) +
            "; pendingBefore=" + pendingBefore.ToString(CultureInfo.InvariantCulture) +
            "; pendingAfter=" + _registeredEquippedPlacementPendingKeys.Count.ToString(CultureInfo.InvariantCulture) +
            "; tracked=" + _registeredEquippedHands.Count.ToString(CultureInfo.InvariantCulture) +
            "; inspected=" + inspected.ToString(CultureInfo.InvariantCulture) +
            "; ready=" + readyCount.ToString(CultureInfo.InvariantCulture) +
            "; terminal=" + terminalCount.ToString(CultureInfo.InvariantCulture) +
            "; skipped=" + skippedCount.ToString(CultureInfo.InvariantCulture) +
            "; removed=" + removedCount.ToString(CultureInfo.InvariantCulture);
        AddLifecycleEvent("registered-equipped-placement-retrofit", summary);
        _log.LogInfo($"{Plugin.PluginName} registered equipped placement retrofit; {summary}");
    }

    private string DescribeRuntimeEcsView(
        TaintedWeaponRecord? record,
        string templateGuid,
        string templateName,
        CharacterHandBase handBase,
        string route,
        bool registered,
        int attempt,
        int waitedFrames,
        out bool ready,
        out bool terminal)
    {
        ready = false;
        terminal = false;

        DrakeMeshRenderer[] authoringRenderers = handBase.GetComponentsInChildren<DrakeMeshRenderer>(true);
        DrakeLodGroup[] authoringGroups = handBase.GetComponentsInChildren<DrakeLodGroup>(true);
        Renderer[] unityRenderers = handBase.GetComponentsInChildren<Renderer>(true);
        string key = registered && record != null ? record.Definition.RegistryKey : "vanilla-two-handed";
        string sourceDetails = registered && record?.CustomVisualSource != null ? record.CustomVisualSource.DescribeForRuntimeEcsReceipt() : "customSource=<native-vanilla>";
        string authoringTransformProof = DescribeAuthoringDrakeTransformProof(handBase.transform, authoringRenderers);
        string prefix = $"route={route}.DrakeEcs; key={key}; registered={Bool(registered)}; template={templateName}; templateGuid={templateGuid}; hand={TransformPath(handBase.transform)}; attempt={attempt.ToString(CultureInfo.InvariantCulture)}; waitedFrames={waitedFrames.ToString(CultureInfo.InvariantCulture)}; handActiveSelf={handBase.gameObject.activeSelf}; handActiveInHierarchy={handBase.gameObject.activeInHierarchy}; authoringDrakeLodGroups={authoringGroups.Length.ToString(CultureInfo.InvariantCulture)}; authoringDrakeMeshRenderers={authoringRenderers.Length.ToString(CultureInfo.InvariantCulture)}; unityRenderers={unityRenderers.Length.ToString(CultureInfo.InvariantCulture)}; activeUnityRenderers={unityRenderers.Count(renderer => renderer != null && renderer.gameObject.activeInHierarchy && renderer.enabled).ToString(CultureInfo.InvariantCulture)}; visibleUnityRenderers={unityRenderers.Count(renderer => renderer != null && renderer.isVisible).ToString(CultureInfo.InvariantCulture)}; authoringTransformProof={authoringTransformProof}; {sourceDetails}";

        if (!TryReadOwnedEntities(handBase.gameObject, out Entity[] ownedEntities, out string ownershipDetails, out terminal))
        {
            return prefix + "; ready=False; terminal=" + Bool(terminal) + "; reason=" + ownershipDetails;
        }

        World world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.IsCreated)
        {
            return prefix + "; ready=False; terminal=False; " + ownershipDetails + "; reason=default-entity-world-unavailable";
        }

        EntityManager entityManager = world.EntityManager;
        Entity[] existingEntities = ownedEntities.Where(entity => entityManager.Exists(entity)).ToArray();
        if (existingEntities.Length != ownedEntities.Length)
        {
            return prefix + "; ready=False; terminal=False; " + ownershipDetails + "; existingLinkedEntityCount=" + existingEntities.Length.ToString(CultureInfo.InvariantCulture) + "; reason=linked-entity-not-yet-created; entities=" + DescribeEntities(ownedEntities);
        }

        Entity[] groups = ownedEntities.Where(entity => entityManager.HasComponent<MeshLODGroupComponent>(entity)).ToArray();
        Entity[] renderers = ownedEntities.Where(entity => entityManager.HasComponent<DrakeMeshMaterialComponent>(entity)).ToArray();
        if (groups.Length != 1 || renderers.Length != 1 || groups[0] == renderers[0])
        {
            terminal = true;
            return prefix + "; ready=False; terminal=True; " + ownershipDetails + "; reason=linked-entity-role-cardinality-mismatch; groupCount=" + groups.Length.ToString(CultureInfo.InvariantCulture) + "; rendererCount=" + renderers.Length.ToString(CultureInfo.InvariantCulture) + "; entities=" + DescribeEntities(ownedEntities);
        }

        Entity group = groups[0];
        Entity renderer = renderers[0];
        string groupComponents = DescribeEntityComponents(entityManager, group);
        string rendererComponents = DescribeEntityComponents(entityManager, renderer);
        bool groupStructural = entityManager.HasComponent<LocalToWorld>(group) &&
            entityManager.HasComponent<LinkedTransformComponent>(group) &&
            HasDrakeSceneLifetime(entityManager, group);
        bool rendererStructural = entityManager.HasComponent<LocalToWorld>(renderer) &&
            entityManager.HasComponent<RenderBounds>(renderer) &&
            entityManager.HasComponent<WorldRenderBounds>(renderer) &&
            entityManager.HasComponent<MeshLODComponent>(renderer) &&
            entityManager.HasComponent<DrakeRendererVisibleRangeComponent>(renderer) &&
            entityManager.HasComponent<LinkedTransformComponent>(renderer) &&
            entityManager.HasComponent<RenderFilterSettings>(renderer) &&
            HasDrakeSceneLifetime(entityManager, renderer);
        if (!groupStructural || !rendererStructural)
        {
            terminal = true;
            return prefix + "; ready=False; terminal=True; " + ownershipDetails + "; group=" + DescribeEntity(group) + "; render=" + DescribeEntity(renderer) + "; reason=linked-entity-structural-components-missing; groupStructural=" + Bool(groupStructural) + "; rendererStructural=" + Bool(rendererStructural) + "; groupComponents=" + groupComponents + "; rendererComponents=" + rendererComponents;
        }

        MeshLODComponent meshLod = entityManager.GetComponentData<MeshLODComponent>(renderer);
        if (meshLod.Group != group)
        {
            terminal = true;
            return prefix + "; ready=False; terminal=True; " + ownershipDetails + "; group=" + DescribeEntity(group) + "; render=" + DescribeEntity(renderer) + "; reason=render-mesh-lod-group-link-mismatch; expectedGroup=" + DescribeEntity(group) + "; actualGroup=" + DescribeEntity(meshLod.Group);
        }

        Transform? groupTransform = entityManager.GetComponentData<LinkedTransformComponent>(group).transform.Value;
        Transform? renderTransform = entityManager.GetComponentData<LinkedTransformComponent>(renderer).transform.Value;
        if (!IsOwnedTransform(handBase.transform, groupTransform) || !IsOwnedTransform(handBase.transform, renderTransform))
        {
            terminal = true;
            return prefix + "; ready=False; terminal=True; " + ownershipDetails + "; group=" + DescribeEntity(group) + "; render=" + DescribeEntity(renderer) + "; reason=linked-transform-owner-mismatch; groupTransform=" + Clean(groupTransform == null ? string.Empty : RelativePath(handBase.transform, groupTransform)) + "; renderTransform=" + Clean(renderTransform == null ? string.Empty : RelativePath(handBase.transform, renderTransform));
        }

        DrakeMeshMaterialComponent materialIdentity = entityManager.GetComponentData<DrakeMeshMaterialComponent>(renderer);
        string ecsCompletion = registered && record != null
            ? TryCompleteRegisteredEquippedRuntimeEcsRenderer(record, entityManager, renderer, materialIdentity, handBase, renderTransform)
            : "read-only-vanilla";
        groupComponents = DescribeEntityComponents(entityManager, group);
        rendererComponents = DescribeEntityComponents(entityManager, renderer);
        bool readyComponents = entityManager.HasComponent<MaterialMeshInfo>(renderer) &&
            entityManager.HasComponent<MipmapsMaterialComponent>(renderer) &&
            entityManager.HasComponent<UVDistributionMetricComponent>(renderer) &&
            entityManager.HasComponent<DrakeRendererSpawnedTag>(renderer);
        bool transitionalTags = entityManager.HasComponent<DrakeRendererLoadRequestTag>(renderer) ||
            entityManager.HasComponent<DrakeRendererLoadingTag>(renderer) ||
            entityManager.HasComponent<DrakeRendererUnloadRequestTag>(renderer);
        bool groupEnabled = entityManager.IsEnabled(group);
        bool renderEnabled = entityManager.IsEnabled(renderer);
        MaterialMeshInfo materialMeshInfo = entityManager.HasComponent<MaterialMeshInfo>(renderer)
            ? entityManager.GetComponentData<MaterialMeshInfo>(renderer)
            : default;
        LocalToWorld rendererLocalToWorld = entityManager.GetComponentData<LocalToWorld>(renderer);
        LocalToWorld groupLocalToWorldData = entityManager.GetComponentData<LocalToWorld>(group);
        RenderBounds rendererRenderBounds = entityManager.GetComponentData<RenderBounds>(renderer);
        WorldRenderBounds rendererWorldRenderBounds = entityManager.GetComponentData<WorldRenderBounds>(renderer);
        string localToWorldTranslation = FormatTranslation(rendererLocalToWorld.Value.c3);
        string renderLocalToWorld = FormatFloat4x4(rendererLocalToWorld.Value);
        string groupLocalToWorld = FormatFloat4x4(groupLocalToWorldData.Value);
        string renderBounds = FormatAabb(rendererRenderBounds.Value);
        string worldRenderBounds = FormatAabb(rendererWorldRenderBounds.Value);
        string linkedTransformProof = DescribeLinkedRendererTransformProof(handBase.transform, groupTransform, renderTransform);
        string visualProof = DescribeRuntimeVisualProof(
            entityManager,
            renderer,
            handBase,
            renderTransform,
            materialIdentity,
            rendererLocalToWorld,
            rendererWorldRenderBounds);
        string vanillaFppContract = registered
            ? "registered-skip"
            : TryCaptureVisibleVanillaEquippedFppPresentationContract(
                handBase,
                renderTransform,
                entityManager,
                renderer,
                rendererRenderBounds,
                rendererWorldRenderBounds);
        bool rendererReady = readyComponents &&
            !transitionalTags &&
            materialMeshInfo.MeshID.value != 0u &&
            materialMeshInfo.MaterialID.value != 0u &&
            groupEnabled &&
            renderEnabled;
        bool vanillaComparisonReady = !registered &&
            vanillaFppContract.StartsWith("captured:", StringComparison.OrdinalIgnoreCase) &&
            groupEnabled &&
            renderEnabled;
        ready = registered ? rendererReady : vanillaComparisonReady;
        string readinessMode = registered
            ? "registered-renderer"
            : "vanilla-visible-fpp-contract";

        return prefix + "; ready=" + Bool(ready) +
            "; readinessMode=" + readinessMode +
            "; rendererReady=" + Bool(rendererReady) +
            "; comparisonReady=" + Bool(vanillaComparisonReady) +
            "; terminal=False; " + ownershipDetails +
            "; group=" + DescribeEntity(group) +
            "; render=" + DescribeEntity(renderer) +
            "; groupEnabled=" + Bool(groupEnabled) +
            "; renderEnabled=" + Bool(renderEnabled) +
            "; meshIndex=" + materialIdentity.meshIndex.ToString(CultureInfo.InvariantCulture) +
            "; materialIndex=" + materialIdentity.materialIndex.ToString(CultureInfo.InvariantCulture) +
            "; submesh=" + materialIdentity.submesh.ToString(CultureInfo.InvariantCulture) +
            "; meshId=" + materialMeshInfo.MeshID.value.ToString(CultureInfo.InvariantCulture) +
            "; materialId=" + materialMeshInfo.MaterialID.value.ToString(CultureInfo.InvariantCulture) +
            "; readyComponents=" + Bool(readyComponents) +
            "; transitionalTags=" + Bool(transitionalTags) +
            "; ecsCompletion=" + ecsCompletion +
            "; groupLocalToWorld=" + groupLocalToWorld +
            "; renderLocalToWorld=" + renderLocalToWorld +
            "; renderLocalToWorldTranslation=" + localToWorldTranslation +
            "; renderBounds=" + renderBounds +
            "; worldRenderBounds=" + worldRenderBounds +
            "; linkedTransformProof=" + linkedTransformProof +
            "; visualProof=" + visualProof +
            "; vanillaFppContract=" + vanillaFppContract +
            "; groupTransform=" + Clean(RelativePath(handBase.transform, groupTransform)) +
            "; renderTransform=" + Clean(RelativePath(handBase.transform, renderTransform)) +
            "; groupComponents=" + groupComponents +
            "; rendererComponents=" + rendererComponents;
    }

    private static string DescribeInventoryPreviewRuntimeView(
        TaintedWeaponRecord? record,
        string templateGuid,
        string templateName,
        CharacterHandBase handBase,
        CustomHeroClothes? previewOwner,
        string route,
        int repairCount,
        bool registered)
    {
        DrakeMeshRenderer[] drakeRenderers = handBase.GetComponentsInChildren<DrakeMeshRenderer>(true);
        DrakeLodGroup[] drakeLodGroups = handBase.GetComponentsInChildren<DrakeLodGroup>(true);
        Renderer[] unityRenderers = handBase.GetComponentsInChildren<Renderer>(true);
        int activeDrakeRenderers = drakeRenderers.Count(renderer => renderer != null && renderer.gameObject.activeInHierarchy);
        int activeUnityRenderers = unityRenderers.Count(renderer => renderer != null && renderer.gameObject.activeInHierarchy && renderer.enabled);
        int visibleUnityRenderers = unityRenderers.Count(renderer => renderer != null && renderer.isVisible);
        string drakePaths = string.Join("|", drakeRenderers.Take(8).Select(renderer => TransformPath(renderer.transform) + ":activeSelf=" + renderer.gameObject.activeSelf + ":activeInHierarchy=" + renderer.gameObject.activeInHierarchy + ":layer=" + renderer.gameObject.layer.ToString(CultureInfo.InvariantCulture)));
        string key = registered && record != null ? record.Definition.RegistryKey : "vanilla-two-handed";
        string sourceDetails = registered && record?.CustomVisualSource != null ? record.CustomVisualSource.DescribeForRuntimeEcsReceipt() : "customSource=<native-vanilla>";
        return $"route={route}; key={key}; registered={Bool(registered)}; template={templateName}; templateGuid={templateGuid}; hand={TransformPath(handBase.transform)}; handActiveSelf={handBase.gameObject.activeSelf}; handActiveInHierarchy={handBase.gameObject.activeInHierarchy}; previewRouteClass={ClassifyInventoryPreviewOwnerPath(previewOwner, handBase)}; repairCount={repairCount.ToString(CultureInfo.InvariantCulture)}; owner={DescribeInventoryPreviewOwnerProof(previewOwner, handBase)}; layerState={DescribePreviewLayerState(handBase, previewOwner)}; drakeLodGroups={drakeLodGroups.Length.ToString(CultureInfo.InvariantCulture)}; drakeMeshRenderers={drakeRenderers.Length.ToString(CultureInfo.InvariantCulture)}; activeDrakeMeshRenderers={activeDrakeRenderers.ToString(CultureInfo.InvariantCulture)}; unityRenderers={unityRenderers.Length.ToString(CultureInfo.InvariantCulture)}; activeUnityRenderers={activeUnityRenderers.ToString(CultureInfo.InvariantCulture)}; visibleUnityRenderers={visibleUnityRenderers.ToString(CultureInfo.InvariantCulture)}; drakePaths={drakePaths}; {sourceDetails}";
    }

    private static string DescribeInventoryPreviewEcsView(
        TaintedWeaponRecord? record,
        string templateGuid,
        string templateName,
        CharacterHandBase handBase,
        CustomHeroClothes? previewOwner,
        string route,
        int attempt,
        int waitedFrames,
        bool registered,
        out bool ready,
        out bool terminal)
    {
        ready = false;
        terminal = false;

        DrakeMeshRenderer[] authoringRenderers = handBase.GetComponentsInChildren<DrakeMeshRenderer>(true);
        DrakeLodGroup[] authoringGroups = handBase.GetComponentsInChildren<DrakeLodGroup>(true);
        Renderer[] unityRenderers = handBase.GetComponentsInChildren<Renderer>(true);
        string key = registered && record != null ? record.Definition.RegistryKey : "vanilla-two-handed";
        string sourceDetails = registered && record?.CustomVisualSource != null ? record.CustomVisualSource.DescribeForRuntimeEcsReceipt() : "customSource=<native-vanilla>";
        string prefix = $"route={route}.InventoryPreviewDrakeEcs; key={key}; registered={Bool(registered)}; template={templateName}; templateGuid={templateGuid}; hand={TransformPath(handBase.transform)}; attempt={attempt.ToString(CultureInfo.InvariantCulture)}; waitedFrames={waitedFrames.ToString(CultureInfo.InvariantCulture)}; handActiveSelf={handBase.gameObject.activeSelf}; handActiveInHierarchy={handBase.gameObject.activeInHierarchy}; previewRouteClass={ClassifyInventoryPreviewOwnerPath(previewOwner, handBase)}; owner={DescribeInventoryPreviewOwnerProof(previewOwner, handBase)}; layerState={DescribePreviewLayerState(handBase, previewOwner)}; authoringDrakeLodGroups={authoringGroups.Length.ToString(CultureInfo.InvariantCulture)}; authoringDrakeMeshRenderers={authoringRenderers.Length.ToString(CultureInfo.InvariantCulture)}; unityRenderers={unityRenderers.Length.ToString(CultureInfo.InvariantCulture)}; activeUnityRenderers={unityRenderers.Count(renderer => renderer != null && renderer.gameObject.activeInHierarchy && renderer.enabled).ToString(CultureInfo.InvariantCulture)}; visibleUnityRenderers={unityRenderers.Count(renderer => renderer != null && renderer.isVisible).ToString(CultureInfo.InvariantCulture)}; {sourceDetails}";

        if (!TryReadOwnedEntities(handBase.gameObject, out Entity[] ownedEntities, out string ownershipDetails, out terminal))
        {
            return prefix + "; ready=False; terminal=" + Bool(terminal) + "; reason=" + ownershipDetails;
        }

        World world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.IsCreated)
        {
            return prefix + "; ready=False; terminal=False; " + ownershipDetails + "; reason=default-entity-world-unavailable";
        }

        EntityManager entityManager = world.EntityManager;
        Entity[] existingEntities = ownedEntities.Where(entity => entityManager.Exists(entity)).ToArray();
        if (existingEntities.Length != ownedEntities.Length)
        {
            return prefix + "; ready=False; terminal=False; " + ownershipDetails + "; existingLinkedEntityCount=" + existingEntities.Length.ToString(CultureInfo.InvariantCulture) + "; reason=linked-entity-not-yet-created; entities=" + DescribeEntities(ownedEntities);
        }

        Entity[] groups = ownedEntities.Where(entity => entityManager.HasComponent<MeshLODGroupComponent>(entity)).ToArray();
        Entity[] renderers = ownedEntities.Where(entity => entityManager.HasComponent<DrakeMeshMaterialComponent>(entity)).ToArray();
        if (groups.Length != 1 || renderers.Length != 1 || groups[0] == renderers[0])
        {
            terminal = true;
            return prefix + "; ready=False; terminal=True; " + ownershipDetails + "; reason=linked-entity-role-cardinality-mismatch; groupCount=" + groups.Length.ToString(CultureInfo.InvariantCulture) + "; rendererCount=" + renderers.Length.ToString(CultureInfo.InvariantCulture) + "; entities=" + DescribeEntities(ownedEntities);
        }

        Entity group = groups[0];
        Entity renderer = renderers[0];
        string groupComponents = DescribeEntityComponents(entityManager, group);
        string rendererComponents = DescribeEntityComponents(entityManager, renderer);
        bool groupStructural = entityManager.HasComponent<LocalToWorld>(group) &&
            entityManager.HasComponent<LinkedTransformComponent>(group) &&
            HasDrakeSceneLifetime(entityManager, group);
        bool rendererStructural = entityManager.HasComponent<LocalToWorld>(renderer) &&
            entityManager.HasComponent<RenderBounds>(renderer) &&
            entityManager.HasComponent<WorldRenderBounds>(renderer) &&
            entityManager.HasComponent<MeshLODComponent>(renderer) &&
            entityManager.HasComponent<DrakeRendererVisibleRangeComponent>(renderer) &&
            entityManager.HasComponent<LinkedTransformComponent>(renderer) &&
            entityManager.HasComponent<RenderFilterSettings>(renderer) &&
            HasDrakeSceneLifetime(entityManager, renderer);
        if (!groupStructural || !rendererStructural)
        {
            terminal = true;
            return prefix + "; ready=False; terminal=True; " + ownershipDetails + "; group=" + DescribeEntity(group) + "; render=" + DescribeEntity(renderer) + "; reason=linked-entity-structural-components-missing; groupStructural=" + Bool(groupStructural) + "; rendererStructural=" + Bool(rendererStructural) + "; groupComponents=" + groupComponents + "; rendererComponents=" + rendererComponents;
        }

        MeshLODComponent meshLod = entityManager.GetComponentData<MeshLODComponent>(renderer);
        if (meshLod.Group != group)
        {
            terminal = true;
            return prefix + "; ready=False; terminal=True; " + ownershipDetails + "; group=" + DescribeEntity(group) + "; render=" + DescribeEntity(renderer) + "; reason=render-mesh-lod-group-link-mismatch; expectedGroup=" + DescribeEntity(group) + "; actualGroup=" + DescribeEntity(meshLod.Group);
        }

        Transform? groupTransform = entityManager.GetComponentData<LinkedTransformComponent>(group).transform.Value;
        Transform? renderTransform = entityManager.GetComponentData<LinkedTransformComponent>(renderer).transform.Value;
        if (!IsOwnedTransform(handBase.transform, groupTransform) || !IsOwnedTransform(handBase.transform, renderTransform))
        {
            terminal = true;
            return prefix + "; ready=False; terminal=True; " + ownershipDetails + "; group=" + DescribeEntity(group) + "; render=" + DescribeEntity(renderer) + "; reason=linked-transform-owner-mismatch; groupTransform=" + Clean(groupTransform == null ? string.Empty : RelativePath(handBase.transform, groupTransform)) + "; renderTransform=" + Clean(renderTransform == null ? string.Empty : RelativePath(handBase.transform, renderTransform));
        }

        DrakeMeshMaterialComponent materialIdentity = entityManager.GetComponentData<DrakeMeshMaterialComponent>(renderer);
        string ecsCompletion = registered ? TryCompleteRegisteredRuntimeEcsRenderer(entityManager, renderer, materialIdentity) : "read-only-vanilla";
        string ecsLayerRepair = registered ? NormalizeRegisteredInventoryPreviewEcsLayer(entityManager, renderer, previewOwner) : "read-only-vanilla";
        groupComponents = DescribeEntityComponents(entityManager, group);
        rendererComponents = DescribeEntityComponents(entityManager, renderer);
        bool readyComponents = entityManager.HasComponent<MaterialMeshInfo>(renderer) &&
            entityManager.HasComponent<MipmapsMaterialComponent>(renderer) &&
            entityManager.HasComponent<UVDistributionMetricComponent>(renderer) &&
            entityManager.HasComponent<DrakeRendererSpawnedTag>(renderer);
        bool transitionalTags = entityManager.HasComponent<DrakeRendererLoadRequestTag>(renderer) ||
            entityManager.HasComponent<DrakeRendererLoadingTag>(renderer) ||
            entityManager.HasComponent<DrakeRendererUnloadRequestTag>(renderer);
        bool groupEnabled = entityManager.IsEnabled(group);
        bool renderEnabled = entityManager.IsEnabled(renderer);
        MaterialMeshInfo materialMeshInfo = entityManager.HasComponent<MaterialMeshInfo>(renderer)
            ? entityManager.GetComponentData<MaterialMeshInfo>(renderer)
            : default;
        LocalToWorld rendererLocalToWorld = entityManager.GetComponentData<LocalToWorld>(renderer);
        RenderBounds rendererRenderBounds = entityManager.GetComponentData<RenderBounds>(renderer);
        WorldRenderBounds rendererWorldRenderBounds = entityManager.GetComponentData<WorldRenderBounds>(renderer);
        string localToWorld = FormatTranslation(rendererLocalToWorld.Value.c3);
        string renderBounds = FormatAabb(rendererRenderBounds.Value);
        string worldRenderBounds = FormatAabb(rendererWorldRenderBounds.Value);
        string visualProof = DescribeRuntimeVisualProof(
            entityManager,
            renderer,
            handBase,
            renderTransform,
            materialIdentity,
            rendererLocalToWorld,
            rendererWorldRenderBounds);
        ready = readyComponents &&
            !transitionalTags &&
            materialMeshInfo.MeshID.value != 0u &&
            materialMeshInfo.MaterialID.value != 0u &&
            groupEnabled &&
            renderEnabled;

        return prefix + "; ready=" + Bool(ready) +
            "; terminal=False; " + ownershipDetails +
            "; group=" + DescribeEntity(group) +
            "; render=" + DescribeEntity(renderer) +
            "; groupEnabled=" + Bool(groupEnabled) +
            "; renderEnabled=" + Bool(renderEnabled) +
            "; meshIndex=" + materialIdentity.meshIndex.ToString(CultureInfo.InvariantCulture) +
            "; materialIndex=" + materialIdentity.materialIndex.ToString(CultureInfo.InvariantCulture) +
            "; submesh=" + materialIdentity.submesh.ToString(CultureInfo.InvariantCulture) +
            "; meshId=" + materialMeshInfo.MeshID.value.ToString(CultureInfo.InvariantCulture) +
            "; materialId=" + materialMeshInfo.MaterialID.value.ToString(CultureInfo.InvariantCulture) +
            "; readyComponents=" + Bool(readyComponents) +
            "; transitionalTags=" + Bool(transitionalTags) +
            "; ecsCompletion=" + ecsCompletion +
            "; ecsLayerRepair=" + ecsLayerRepair +
            "; ecsRenderFilter=" + DescribeEcsRenderFilterLayerState(entityManager, renderer) +
            "; renderLocalToWorldTranslation=" + localToWorld +
            "; renderBounds=" + renderBounds +
            "; worldRenderBounds=" + worldRenderBounds +
            "; visualProof=" + visualProof +
            "; groupTransform=" + Clean(RelativePath(handBase.transform, groupTransform)) +
            "; renderTransform=" + Clean(RelativePath(handBase.transform, renderTransform)) +
            "; groupComponents=" + groupComponents +
            "; rendererComponents=" + rendererComponents;
    }

    private static string DescribeInventoryPreviewOwnerProof(CustomHeroClothes? previewOwner, CharacterHandBase? handBase)
    {
        Transform? ownerTransform = PreviewOwnerTransform(previewOwner);
        string ownerPath = ownerTransform == null ? "none" : TransformPath(ownerTransform);
        string handPath = handBase == null ? "none" : TransformPath(handBase.transform);
        return "previewOwnerPresent=" + Bool(previewOwner != null) +
            ",previewOwnerType=" + CleanForInline(previewOwner == null ? "none" : previewOwner.GetType().FullName ?? previewOwner.GetType().Name) +
            ",previewOwnerPath=" + CleanForInline(ownerPath) +
            ",handPath=" + CleanForInline(handPath) +
            ",previewRouteClass=" + ClassifyInventoryPreviewOwnerPath(previewOwner, handBase) +
            ",ownerWeaponLayer=" + ReadPreviewOwnerWeaponLayer(previewOwner).ToString(CultureInfo.InvariantCulture) +
            ",ownerLightRenderLayerMask=" + ReadPreviewOwnerLightRenderLayerMask(previewOwner).ToString(CultureInfo.InvariantCulture) +
            ",activeCamera=" + DescribeActiveCameraSummary();
    }

    private static string ClassifyInventoryPreviewOwnerPath(CustomHeroClothes? previewOwner, CharacterHandBase? handBase)
    {
        Transform? ownerTransform = PreviewOwnerTransform(previewOwner);
        string ownerPath = ownerTransform == null ? string.Empty : TransformPath(ownerTransform);
        string handPath = handBase == null ? string.Empty : TransformPath(handBase.transform);
        string combined = ownerPath + " " + handPath;
        if (combined.IndexOf("CustomHeroClothes", StringComparison.OrdinalIgnoreCase) >= 0 ||
            combined.IndexOf("VHeroRenderer", StringComparison.OrdinalIgnoreCase) >= 0 ||
            combined.IndexOf("HeroRenderer", StringComparison.OrdinalIgnoreCase) >= 0 ||
            combined.IndexOf("RawImageRendering", StringComparison.OrdinalIgnoreCase) >= 0 ||
            combined.IndexOf("CharacterSheet", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "inventory-hero-renderer-clone";
        }

        if (combined.IndexOf("FppParent", StringComparison.OrdinalIgnoreCase) >= 0 ||
            combined.IndexOf("FppArms", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "fpp";
        }

        if (combined.IndexOf("tppParent", StringComparison.OrdinalIgnoreCase) >= 0 ||
            combined.IndexOf("TPP", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "tpp";
        }

        return "unknown";
    }

    private static Transform? PreviewOwnerTransform(CustomHeroClothes? previewOwner)
    {
        if (previewOwner == null)
        {
            return null;
        }

        try
        {
            return previewOwner.RootSocket;
        }
        catch
        {
            return null;
        }
    }

    private static string DescribePreviewLayerState(CharacterHandBase handBase, CustomHeroClothes? previewOwner)
    {
        Renderer[] renderers = handBase.GetComponentsInChildren<Renderer>(true);
        string rendererLayers = string.Join("|", renderers.Select(renderer => renderer.gameObject.layer).Distinct().OrderBy(layer => layer).Select(layer => layer.ToString(CultureInfo.InvariantCulture)));
        string rendererMasks = string.Join("|", renderers.Select(renderer => renderer.renderingLayerMask).Distinct().OrderBy(mask => mask).Select(mask => mask.ToString(CultureInfo.InvariantCulture)));
        Camera? camera = ActiveCamera();
        int ownerLayer = ReadPreviewOwnerWeaponLayer(previewOwner);
        int handLayer = handBase.gameObject.layer;
        return "handLayer=" + handLayer.ToString(CultureInfo.InvariantCulture) +
            ",ownerWeaponLayer=" + ownerLayer.ToString(CultureInfo.InvariantCulture) +
            ",ownerLightRenderLayerMask=" + ReadPreviewOwnerLightRenderLayerMask(previewOwner).ToString(CultureInfo.InvariantCulture) +
            ",rendererLayers=" + CleanForInline(rendererLayers) +
            ",rendererRenderingLayerMasks=" + CleanForInline(rendererMasks) +
            ",cameraMask=" + (camera == null ? "none" : camera.cullingMask.ToString(CultureInfo.InvariantCulture)) +
            ",handLayerVisible=" + Bool(camera != null && IsLayerVisibleToCamera(camera, handLayer)) +
            ",ownerLayerVisible=" + Bool(camera != null && ownerLayer >= 0 && IsLayerVisibleToCamera(camera, ownerLayer));
    }

    private static int NormalizeRegisteredInventoryPreviewUnityLayers(CharacterHandBase handBase, CustomHeroClothes? previewOwner)
    {
        int ownerLayer = ReadPreviewOwnerWeaponLayer(previewOwner);
        uint ownerMask = ReadPreviewOwnerLightRenderLayerMask(previewOwner);
        if (ownerLayer < 0 || ownerLayer > 31)
        {
            return 0;
        }

        int changed = 0;
        foreach (Transform transform in handBase.GetComponentsInChildren<Transform>(true))
        {
            if (transform != null && transform.gameObject.layer != ownerLayer)
            {
                transform.gameObject.layer = ownerLayer;
                changed++;
            }
        }

        if (ownerMask != 0u)
        {
            foreach (Renderer renderer in handBase.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer != null && renderer.renderingLayerMask != ownerMask)
                {
                    renderer.renderingLayerMask = ownerMask;
                    changed++;
                }
            }
        }

        return changed;
    }

    private static string NormalizeRegisteredInventoryPreviewEcsLayer(EntityManager entityManager, Entity renderer, CustomHeroClothes? previewOwner)
    {
        if (!entityManager.HasComponent<RenderFilterSettings>(renderer))
        {
            return "skipped:render-filter-missing";
        }

        int ownerLayer = ReadPreviewOwnerWeaponLayer(previewOwner);
        uint ownerMask = ReadPreviewOwnerLightRenderLayerMask(previewOwner);
        if (ownerLayer < 0 || ownerLayer > 31)
        {
            return "skipped:owner-layer-unavailable";
        }

        try
        {
            RenderFilterSettings settings = entityManager.GetSharedComponentManaged<RenderFilterSettings>(renderer);
            int beforeLayer = settings.Layer;
            uint beforeMask = settings.RenderingLayerMask;
            bool changed = false;
            if (settings.Layer != ownerLayer)
            {
                settings.Layer = ownerLayer;
                changed = true;
            }

            if (ownerMask != 0u && settings.RenderingLayerMask != ownerMask)
            {
                settings.RenderingLayerMask = ownerMask;
                changed = true;
            }

            if (changed)
            {
                entityManager.SetSharedComponentManaged(renderer, settings);
            }

            return (changed ? "applied" : "unchanged") +
                ":beforeLayer=" + beforeLayer.ToString(CultureInfo.InvariantCulture) +
                ",afterLayer=" + settings.Layer.ToString(CultureInfo.InvariantCulture) +
                ",beforeRenderingLayerMask=" + beforeMask.ToString(CultureInfo.InvariantCulture) +
                ",afterRenderingLayerMask=" + settings.RenderingLayerMask.ToString(CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            return "failed:" + ex.GetType().Name + ":" + Clean(ex.Message);
        }
    }

    private static string DescribeEcsRenderFilterLayerState(EntityManager entityManager, Entity renderer)
    {
        if (!entityManager.HasComponent<RenderFilterSettings>(renderer))
        {
            return "present=False";
        }

        try
        {
            RenderFilterSettings settings = entityManager.GetSharedComponentManaged<RenderFilterSettings>(renderer);
            return "present=True,layer=" + settings.Layer.ToString(CultureInfo.InvariantCulture) +
                ",renderingLayerMask=" + settings.RenderingLayerMask.ToString(CultureInfo.InvariantCulture) +
                ",shadowCastingMode=" + settings.ShadowCastingMode +
                ",receiveShadows=" + Bool(settings.ReceiveShadows);
        }
        catch (Exception ex)
        {
            return "present=True,readFailed=" + ex.GetType().Name + ":" + Clean(ex.Message);
        }
    }

    private static int ReadPreviewOwnerWeaponLayer(CustomHeroClothes? previewOwner)
        => ReadIntProperty(CustomHeroClothesWeaponLayerProperty, previewOwner, -1);

    private static uint ReadPreviewOwnerLightRenderLayerMask(CustomHeroClothes? previewOwner)
    {
        int value = ReadIntProperty(CustomHeroClothesLightRenderLayerMaskProperty, previewOwner, -1);
        return value < 0 ? 0u : unchecked((uint)value);
    }

    private static int ReadIntProperty(PropertyInfo? property, object? target, int fallback)
    {
        if (property == null || target == null)
        {
            return fallback;
        }

        try
        {
            object? value = property.GetValue(target);
            return value switch
            {
                int intValue => intValue,
                uint uintValue => unchecked((int)uintValue),
                short shortValue => shortValue,
                ushort ushortValue => ushortValue,
                byte byteValue => byteValue,
                _ => fallback
            };
        }
        catch
        {
            return fallback;
        }
    }

    private static string DescribeActiveCameraSummary()
    {
        Camera? camera = ActiveCamera();
        if (camera == null)
        {
            return "present=False";
        }

        return "present=True,name=" + CleanForInline(camera.name) +
            ",enabled=" + Bool(camera.enabled) +
            ",active=" + Bool(camera.gameObject.activeInHierarchy) +
            ",cullingMask=" + camera.cullingMask.ToString(CultureInfo.InvariantCulture) +
            ",layer=" + camera.gameObject.layer.ToString(CultureInfo.InvariantCulture);
    }

    private static Camera? ActiveCamera()
    {
        Camera? camera = Camera.main;
        if (camera != null)
        {
            return camera;
        }

        return Camera.allCameras.FirstOrDefault(candidate => candidate != null && candidate.enabled && candidate.gameObject.activeInHierarchy);
    }

    private static bool IsVanillaTwoHandedPreviewCandidate(string templateGuid, string templateName)
    {
        string value = (templateGuid + " " + templateName).ToLowerInvariant();
        return value.Contains("weapon_2h", StringComparison.Ordinal) ||
            value.Contains("_2h_", StringComparison.Ordinal) ||
            value.Contains("twohand", StringComparison.Ordinal) ||
            value.Contains("two-handed", StringComparison.Ordinal) ||
            value.Contains("two handed", StringComparison.Ordinal) ||
            value.Contains("greatsword", StringComparison.Ordinal) ||
            value.Contains("broadsword", StringComparison.Ordinal);
    }

    private static string TryCompleteRegisteredRuntimeEcsRenderer(EntityManager entityManager, Entity renderer, DrakeMeshMaterialComponent materialIdentity)
        => TryCompleteRegisteredRuntimeEcsRenderer(
            entityManager,
            renderer,
            materialIdentity,
            () => TryApplyRegisteredRendererLinkedTransformOffset(entityManager, renderer),
            () => "skipped:not-equipped-runtime");

    private string TryCompleteRegisteredEquippedRuntimeEcsRenderer(
        TaintedWeaponRecord record,
        EntityManager entityManager,
        Entity renderer,
        DrakeMeshMaterialComponent materialIdentity,
        CharacterHandBase handBase,
        Transform? renderTransform)
    {
        if (!IsActiveFppEquippedHand(handBase))
        {
            return "skipped:not-active-fpp-equipped-hand";
        }

        return TryCompleteRegisteredRuntimeEcsRenderer(
            entityManager,
            renderer,
            materialIdentity,
            () => TryApplyRegisteredRendererStoredLinkedTransformOffset(record, entityManager, renderer),
            () => NormalizeRegisteredEquippedPresentation(record, entityManager, renderer, handBase, renderTransform));
    }

    private static string TryCompleteRegisteredRuntimeEcsRenderer(
        EntityManager entityManager,
        Entity renderer,
        DrakeMeshMaterialComponent materialIdentity,
        Func<string> linkedOffsetCompletion,
        Func<string> presentationCompletion)
    {
        try
        {
            DrakeRendererManager manager = DrakeRendererManager.Instance;
            if (manager == null)
            {
                return "skipped:drake-manager-missing";
            }

            string loadingStart = TaintedWeaponDrakeLoadingManagerBridge.EnsureRegisteredLoadingStarted(manager, in materialIdentity);
            string loadingBits = TaintedWeaponDrakeLoadingManagerBridge.EnsureRegisteredComponentManagerLoadingBits(manager, in materialIdentity);
            manager.ComponentsManager.UpdateLoadings();
            if (!manager.TryGetMaterialMesh(
                    in materialIdentity,
                    out MaterialMeshInfo materialMeshInfo,
                    out MipmapsMaterialComponent mipmapsMaterialComponent,
                    out UVDistributionMetricComponent uvDistributionMetricComponent))
            {
                return "pending:native-manager-material-mesh-not-ready,loadingStart=" + loadingStart + ",loadingBits=" + loadingBits;
            }

            AddOrSetComponentData(entityManager, renderer, materialMeshInfo);
            AddOrSetComponentData(entityManager, renderer, mipmapsMaterialComponent);
            AddOrSetComponentData(entityManager, renderer, uvDistributionMetricComponent);
            AddComponentIfMissing<DrakeRendererSpawnedTag>(entityManager, renderer);
            AddComponentIfMissing<DrakeRendererManualTag>(entityManager, renderer);
            string linkedOffset = linkedOffsetCompletion();
            string presentation = presentationCompletion();
            RemoveComponentIfPresent<DrakeRendererLoadRequestTag>(entityManager, renderer);
            RemoveComponentIfPresent<DrakeRendererLoadingTag>(entityManager, renderer);
            RemoveComponentIfPresent<DrakeRendererUnloadRequestTag>(entityManager, renderer);
            string retention = TaintedWeaponDrakeLoadingManagerBridge.RetainRegisteredKeys(materialIdentity.meshIndex, materialIdentity.materialIndex);
            return "applied:meshId=" + materialMeshInfo.MeshID.value.ToString(CultureInfo.InvariantCulture) +
                ",materialId=" + materialMeshInfo.MaterialID.value.ToString(CultureInfo.InvariantCulture) +
                ",loadingStart=" + loadingStart +
                ",loadingBits=" + loadingBits +
                ",manual=True,linkedOffset=" + linkedOffset +
                ",presentation=" + presentation +
                ",retention=" + retention;
        }
        catch (Exception ex)
        {
            return "failed:" + ex.GetType().Name + ":" + Clean(ex.Message);
        }
    }

    private static string TryApplyRegisteredRendererLinkedTransformOffset(EntityManager entityManager, Entity renderer)
    {
        try
        {
            if (!entityManager.HasComponent<LinkedTransformComponent>(renderer))
            {
                return "skipped:linked-transform-missing";
            }

            Transform? linkedTransform = entityManager.GetComponentData<LinkedTransformComponent>(renderer).transform.Value;
            if (linkedTransform == null)
            {
                return "skipped:linked-transform-null";
            }

            DrakeMeshRenderer? authoringRenderer = linkedTransform.GetComponent<DrakeMeshRenderer>();
            if (authoringRenderer == null)
            {
                return "skipped:authoring-drake-renderer-missing";
            }

            float4x4 offsetMatrix = authoringRenderer.LocalToWorldOffset;
            AddOrSetComponentData(
                entityManager,
                renderer,
                new LinkedTransformLocalToWorldOffsetComponent(offsetMatrix));
            return "applied:source=linked-authoring-drake-renderer,offset=" + FormatFloat4x4(offsetMatrix);
        }
        catch (Exception ex)
        {
            return "failed:" + ex.GetType().Name + ":" + Clean(ex.Message);
        }
    }

    private static string TryApplyRegisteredRendererStoredLinkedTransformOffset(TaintedWeaponRecord record, EntityManager entityManager, Entity renderer)
    {
        try
        {
            if (!record.TryGetEquippedDrakeLocalToWorldOffset(out float4x4 offsetMatrix, out string offsetReason))
            {
                return "skipped:stored-equipped-offset-missing";
            }

            AddOrSetComponentData(
                entityManager,
                renderer,
                new LinkedTransformLocalToWorldOffsetComponent(offsetMatrix));
            return "applied:source=stored-equipped-prototype-authoring,offset=" + FormatFloat4x4(offsetMatrix) +
                ",contract=" + Clean(offsetReason).Replace(';', ',');
        }
        catch (Exception ex)
        {
            return "failed:" + ex.GetType().Name + ":" + Clean(ex.Message);
        }
    }

    private string NormalizeRegisteredEquippedPresentation(
        TaintedWeaponRecord record,
        EntityManager entityManager,
        Entity renderer,
        CharacterHandBase handBase,
        Transform? renderTransform)
    {
        string layer = NormalizeRegisteredEquippedPresentationLayer(entityManager, renderer, handBase, renderTransform);
        string placement = TryApplyVisibleVanillaFppPresentationPlacementContract(record, entityManager, renderer, renderTransform);
        return "layer={" + CleanForInline(layer) + "},placement={" + CleanForInline(placement) + "}";
    }

    private static string NormalizeRegisteredEquippedPresentationLayer(
        EntityManager entityManager,
        Entity renderer,
        CharacterHandBase handBase,
        Transform? renderTransform)
    {
        try
        {
            Camera? camera = ActiveCamera();
            if (renderTransform == null)
            {
                return "skipped:linked-render-transform-missing";
            }

            Transform linkedRenderTransform = renderTransform;
            int beforeHandLayer = handBase.gameObject.layer;
            int beforeRenderLayer = linkedRenderTransform.gameObject.layer;
            int targetLayer = beforeRenderLayer;
            string targetReason = "current-render-layer";
            EquippedFppPresentationContract? contract = _visibleVanillaTwoHandedFppContract;

            if (camera != null &&
                contract != null &&
                contract.RenderLayer >= 0 &&
                contract.RenderLayer <= 31 &&
                IsLayerVisibleToCamera(camera, contract.RenderLayer))
            {
                targetLayer = contract.RenderLayer;
                targetReason = "visible-vanilla-fpp-contract";
            }
            else if (camera != null && !IsLayerVisibleToCamera(camera, targetLayer))
            {
                return "skipped:current-render-layer-not-visible,beforeHandLayer=" + beforeHandLayer.ToString(CultureInfo.InvariantCulture) +
                    ",beforeRenderLayer=" + beforeRenderLayer.ToString(CultureInfo.InvariantCulture) +
                    ",camera=" + CleanForInline(camera.name) +
                    ",cameraMask=" + camera.cullingMask.ToString(CultureInfo.InvariantCulture);
            }

            if (targetLayer < 0 || targetLayer > 31)
            {
                return "skipped:target-layer-unavailable,beforeHandLayer=" + beforeHandLayer.ToString(CultureInfo.InvariantCulture) +
                    ",beforeRenderLayer=" + beforeRenderLayer.ToString(CultureInfo.InvariantCulture);
            }

            int changedUnityLayers = 0;
            foreach (Transform transform in linkedRenderTransform.GetComponentsInChildren<Transform>(true))
            {
                if (transform != null && transform.gameObject.layer != targetLayer)
                {
                    transform.gameObject.layer = targetLayer;
                    changedUnityLayers++;
                }
            }

            string ecsLayer = "skipped:render-filter-missing";
            if (entityManager.HasComponent<RenderFilterSettings>(renderer))
            {
                RenderFilterSettings settings = entityManager.GetSharedComponentManaged<RenderFilterSettings>(renderer);
                int beforeEcsLayer = settings.Layer;
                uint beforeMask = settings.RenderingLayerMask;
                bool changed = false;
                if (settings.Layer != targetLayer)
                {
                    settings.Layer = targetLayer;
                    changed = true;
                }

                if (contract != null &&
                    contract.RenderingLayerMask != 0u &&
                    settings.RenderingLayerMask != contract.RenderingLayerMask)
                {
                    settings.RenderingLayerMask = contract.RenderingLayerMask;
                    changed = true;
                }

                if (changed)
                {
                    entityManager.SetSharedComponentManaged(renderer, settings);
                }

                ecsLayer = (changed ? "applied" : "unchanged") +
                    ":beforeLayer=" + beforeEcsLayer.ToString(CultureInfo.InvariantCulture) +
                    ",afterLayer=" + settings.Layer.ToString(CultureInfo.InvariantCulture) +
                    ",beforeRenderingLayerMask=" + beforeMask.ToString(CultureInfo.InvariantCulture) +
                    ",afterRenderingLayerMask=" + settings.RenderingLayerMask.ToString(CultureInfo.InvariantCulture);
            }

            return "applied:reason=" + targetReason +
                ",camera=" + (camera == null ? "none" : CleanForInline(camera.name)) +
                ",cameraMask=" + (camera == null ? "none" : camera.cullingMask.ToString(CultureInfo.InvariantCulture)) +
                ",beforeHandLayer=" + beforeHandLayer.ToString(CultureInfo.InvariantCulture) +
                ",beforeRenderLayer=" + beforeRenderLayer.ToString(CultureInfo.InvariantCulture) +
                ",targetLayer=" + targetLayer.ToString(CultureInfo.InvariantCulture) +
                ",unityLayerRoot=" + CleanForInline(TransformPath(linkedRenderTransform)) +
                ",changedUnityLayers=" + changedUnityLayers.ToString(CultureInfo.InvariantCulture) +
                ",targetLayerVisible=" + Bool(camera != null && IsLayerVisibleToCamera(camera, targetLayer)) +
                ",ecsLayer=" + ecsLayer;
        }
        catch (Exception ex)
        {
            return "failed:" + ex.GetType().Name + ":" + Clean(ex.Message);
        }
    }

    private string TryApplyVisibleVanillaFppPresentationPlacementContract(
        TaintedWeaponRecord record,
        EntityManager entityManager,
        Entity renderer,
        Transform? renderTransform)
    {
        try
        {
            EquippedFppPresentationContract? contract = _visibleVanillaTwoHandedFppContract;

            Camera? camera = ActiveCamera();
            if (camera == null)
            {
                return "skipped:active-camera-missing";
            }

            if (renderTransform == null)
            {
                return "skipped:linked-render-transform-missing";
            }

            if (!entityManager.HasComponent<RenderBounds>(renderer) ||
                !entityManager.HasComponent<WorldRenderBounds>(renderer))
            {
                return "skipped:render-bounds-missing";
            }

            string registryKey = record.Definition.RegistryKey;
            bool pendingBefore = _registeredEquippedPlacementPendingKeys.Contains(registryKey);
            RenderBounds renderBounds = entityManager.GetComponentData<RenderBounds>(renderer);
            WorldRenderBounds beforeWorldRenderBounds = entityManager.GetComponentData<WorldRenderBounds>(renderer);
            Bounds beforeWorldBounds = AabbToBounds(beforeWorldRenderBounds.Value);
            float4x4 linkedLocalToWorld = ToFloat4x4(renderTransform.localToWorldMatrix);
            float4x4 rendererLocalToWorldBefore;
            string rendererLocalToWorldSource;
            if (entityManager.HasComponent<LocalToWorld>(renderer))
            {
                rendererLocalToWorldBefore = entityManager.GetComponentData<LocalToWorld>(renderer).Value;
                rendererLocalToWorldSource = "ecs-local-to-world";
            }
            else
            {
                float4x4 baseOffset = record.TryGetEquippedDrakeLocalToWorldOffset(out float4x4 storedOffset, out _)
                    ? storedOffset
                    : float4x4.identity;
                rendererLocalToWorldBefore = math.mul(linkedLocalToWorld, baseOffset);
                rendererLocalToWorldSource = "linked-transform-plus-stored-offset";
            }

            AABB computedBeforeWorldBounds = TransformAabb(renderBounds.Value, rendererLocalToWorldBefore);
            Bounds computedBeforeUnityBounds = AabbToBounds(computedBeforeWorldBounds);
            Bounds computedBeforeCameraLocalBounds = TransformAabbToCameraLocalBounds(renderBounds.Value, rendererLocalToWorldBefore, camera);
            string placementSource;
            string fallbackDetails;
            Vector3 desiredCameraLocalBoundsCenter;
            if (contract != null)
            {
                placementSource = "visible-vanilla-fpp-contract";
                desiredCameraLocalBoundsCenter = VisibleFppPlacementCameraLocalBoundsCenter(
                    contract,
                    computedBeforeCameraLocalBounds,
                    camera,
                    out fallbackDetails);
            }
            else
            {
                placementSource = "registered-fpp-camera-local-fallback";
                desiredCameraLocalBoundsCenter = RegisteredFppFallbackCameraLocalBoundsCenter(
                    computedBeforeCameraLocalBounds,
                    camera,
                    out fallbackDetails);
            }

            Vector3 desiredWorldCenter = camera.transform.TransformPoint(desiredCameraLocalBoundsCenter);
            Vector3 worldDelta = desiredWorldCenter - computedBeforeUnityBounds.center;
            Vector3 localDelta = renderTransform.InverseTransformVector(worldDelta);
            float4x4 rendererLocalToWorld = rendererLocalToWorldBefore;
            rendererLocalToWorld.c3.x += worldDelta.x;
            rendererLocalToWorld.c3.y += worldDelta.y;
            rendererLocalToWorld.c3.z += worldDelta.z;
            float4x4 composedOffset = math.mul(math.inverse(linkedLocalToWorld), rendererLocalToWorld);
            AddOrSetComponentData(
                entityManager,
                renderer,
                new LinkedTransformLocalToWorldOffsetComponent(composedOffset));

            AddOrSetComponentData(entityManager, renderer, new LocalToWorld { Value = rendererLocalToWorld });
            AABB rawUpdatedWorldBounds = TransformAabb(renderBounds.Value, rendererLocalToWorld);
            Bounds rawUpdatedUnityBounds = AabbToBounds(rawUpdatedWorldBounds);
            bool rawFrustumIntersects = GeometryUtility.TestPlanesAABB(
                GeometryUtility.CalculateFrustumPlanes(camera),
                rawUpdatedUnityBounds);
            AABB updatedWorldBounds = rawUpdatedWorldBounds;
            AddOrSetComponentData(entityManager, renderer, new WorldRenderBounds { Value = updatedWorldBounds });
            Bounds updatedUnityBounds = AabbToBounds(updatedWorldBounds);
            Vector3 postCameraLocalBoundsCenter = camera.transform.InverseTransformPoint(updatedUnityBounds.center);
            Vector3 postViewportBoundsCenter = camera.WorldToViewportPoint(updatedUnityBounds.center);
            bool postFrustumIntersects = GeometryUtility.TestPlanesAABB(
                GeometryUtility.CalculateFrustumPlanes(camera),
                updatedUnityBounds);
            bool pendingResolved = _registeredEquippedPlacementPendingKeys.Remove(registryKey);
            string contractViewportBoundsCenter = contract == null ? "none" : FormatVector3(contract.ViewportBoundsCenter);
            string contractCameraLocalBoundsCenter = contract == null ? "none" : FormatVector3(contract.CameraLocalBoundsCenter);
            string contractCameraLocalBoundsExtents = contract == null ? "none" : FormatVector3(contract.CameraLocalBoundsExtents);
            string contractAgeFrames = contract == null
                ? "none"
                : (Time.frameCount - contract.Frame).ToString(CultureInfo.InvariantCulture);
            string contractTemplate = contract == null ? "none" : CleanForInline(contract.TemplateName);

            return "applied:source=" + placementSource +
                ",pendingBefore=" + Bool(pendingBefore) +
                ",pendingResolved=" + Bool(pendingResolved) +
                ",pendingCount=" + _registeredEquippedPlacementPendingKeys.Count.ToString(CultureInfo.InvariantCulture) +
                ",rawFrustumIntersectsWorldBounds=" + Bool(rawFrustumIntersects) +
                ",postFrustumIntersectsWorldBounds=" + Bool(postFrustumIntersects) +
                ",postViewportBoundsCenter=" + FormatVector3(postViewportBoundsCenter) +
                ",postCameraLocalBoundsCenter=" + FormatVector3(postCameraLocalBoundsCenter) +
                ",updatedWorldBoundsCenter=" + FormatFloat3(updatedWorldBounds.Center) +
                ",updatedWorldBoundsExtents=" + FormatFloat3(updatedWorldBounds.Extents) +
                ",inflatedWorldBounds=False" +
                ",worldBoundsPolicy=raw-transformed-render-bounds" +
                ",depthPolicy=front-face-visible" +
                ",desiredCameraLocalBoundsCenter=" + FormatVector3(desiredCameraLocalBoundsCenter) +
                ",sourceCameraLocalBoundsCenter=" + FormatVector3(computedBeforeCameraLocalBounds.center) +
                ",sourceCameraLocalBoundsExtents=" + FormatVector3(computedBeforeCameraLocalBounds.extents) +
                ",contractViewportBoundsCenter=" + contractViewportBoundsCenter +
                ",contractCameraLocalBoundsCenter=" + contractCameraLocalBoundsCenter +
                ",contractCameraLocalBoundsExtents=" + contractCameraLocalBoundsExtents +
                ",contractAgeFrames=" + contractAgeFrames +
                ",contractTemplate=" + contractTemplate +
                ",fallbackReason=" + (contract == null ? "visible-vanilla-fpp-contract-missing" : "none") +
                ",fallbackDetails=" + fallbackDetails +
                ",desiredWorldBoundsCenter=" + FormatVector3(desiredWorldCenter) +
                ",beforeWorldBoundsCenter=" + FormatVector3(beforeWorldBounds.center) +
                ",computedBeforeWorldBoundsCenter=" + FormatVector3(computedBeforeUnityBounds.center) +
                ",rendererLocalToWorldSource=" + rendererLocalToWorldSource +
                ",worldDelta=" + FormatVector3(worldDelta) +
                ",localDelta=" + FormatVector3(localDelta) +
                ",rawUpdatedWorldBounds=" + FormatAabb(rawUpdatedWorldBounds) +
                ",offset=" + FormatFloat4x4(composedOffset);
        }
        catch (Exception ex)
        {
            return "failed:" + ex.GetType().Name + ":" + Clean(ex.Message);
        }
    }

    private static Vector3 RegisteredFppFallbackCameraLocalBoundsCenter(
        Bounds sourceCameraLocalBounds,
        Camera camera,
        out string details)
    {
        const float FrustumFitMargin = 0.85f;
        const float NearClipMargin = 0.05f;
        const float PreferredRightOffset = 0.35f;
        const float PreferredDownOffset = -0.12f;

        float sourceHalfWidth = Math.Max(sourceCameraLocalBounds.extents.x, 0.001f);
        float sourceHalfHeight = Math.Max(sourceCameraLocalBounds.extents.y, 0.001f);
        float sourceHalfDepth = Math.Max(sourceCameraLocalBounds.extents.z, 0.001f);
        float verticalTan = Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f);
        if (verticalTan <= 0.001f || float.IsNaN(verticalTan) || float.IsInfinity(verticalTan))
        {
            verticalTan = 0.315f;
        }

        float aspect = Math.Max(camera.aspect, 0.1f);
        float horizontalTan = Math.Max(verticalTan * aspect, 0.001f);
        float minZForDepth = camera.nearClipPlane + sourceHalfDepth + NearClipMargin;
        float minZForWidth = sourceHalfWidth / (horizontalTan * FrustumFitMargin);
        float minZForHeight = sourceHalfHeight / (verticalTan * FrustumFitMargin);
        float targetZ = Math.Max(minZForDepth, Math.Max(minZForWidth, minZForHeight));
        float maxAllowedZ = Math.Max(targetZ, camera.farClipPlane * 0.5f);
        targetZ = Math.Min(targetZ, maxAllowedZ);

        float horizontalLimit = Math.Max((horizontalTan * targetZ * FrustumFitMargin) - sourceHalfWidth, 0f);
        float verticalLimit = Math.Max((verticalTan * targetZ * FrustumFitMargin) - sourceHalfHeight, 0f);
        float targetX = Mathf.Clamp(PreferredRightOffset, -horizontalLimit, horizontalLimit);
        float targetY = Mathf.Clamp(PreferredDownOffset, -verticalLimit, verticalLimit);
        Vector3 result = new(targetX, targetY, targetZ);
        details = "target=active-camera-front-face-near-clip" +
            ",fallbackCameraLocalBoundsCenter=" + FormatVector3(result) +
            ",sourceHalfExtents=" + FormatVector3(sourceCameraLocalBounds.extents) +
            ",nearClip=" + FormatFloat(camera.nearClipPlane) +
            ",nearClipMargin=" + FormatFloat(NearClipMargin) +
            ",minZForDepth=" + FormatFloat(minZForDepth) +
            ",minZForWidth=" + FormatFloat(minZForWidth) +
            ",minZForHeight=" + FormatFloat(minZForHeight) +
            ",horizontalLimit=" + FormatFloat(horizontalLimit) +
            ",verticalLimit=" + FormatFloat(verticalLimit);
        return result;
    }

    private static void AddOrSetComponentData<T>(EntityManager entityManager, Entity entity, T component)
        where T : unmanaged, IComponentData
    {
        if (entityManager.HasComponent<T>(entity))
        {
            entityManager.SetComponentData(entity, component);
        }
        else
        {
            entityManager.AddComponentData(entity, component);
        }
    }

    private static void AddComponentIfMissing<T>(EntityManager entityManager, Entity entity)
        where T : unmanaged, IComponentData
    {
        if (!entityManager.HasComponent<T>(entity))
        {
            entityManager.AddComponent<T>(entity);
        }
    }

    private static void RemoveComponentIfPresent<T>(EntityManager entityManager, Entity entity)
        where T : unmanaged, IComponentData
    {
        if (entityManager.HasComponent<T>(entity))
        {
            entityManager.RemoveComponent<T>(entity);
        }
    }

    private static bool TryReadOwnedEntities(GameObject instance, out Entity[] ownedEntities, out string details, out bool terminal)
    {
        ownedEntities = Array.Empty<Entity>();
        terminal = false;
        Component[] accessComponents = instance.GetComponentsInChildren<Component>(true)
            .Where(component => component != null && string.Equals(ComponentType(component), LinkedEntitiesAccessType, StringComparison.Ordinal))
            .ToArray();
        if (accessComponents.Length == 0)
        {
            details = "reason=linked-entities-access-not-created; ownerAccessCount=0";
            return false;
        }

        if (accessComponents.Length != 1)
        {
            terminal = true;
            details = "reason=linked-entities-access-cardinality-mismatch; ownerAccessCount=" + accessComponents.Length.ToString(CultureInfo.InvariantCulture);
            return false;
        }

        try
        {
            FieldInfo? field = accessComponents[0].GetType().GetField("_linkedEntities", BindingFlags.Instance | BindingFlags.NonPublic);
            object? unsafeArray = field?.GetValue(accessComponents[0]);
            if (unsafeArray == null)
            {
                terminal = true;
                details = "reason=linked-entities-owner-field-unavailable; ownerAccessCount=1";
                return false;
            }

            Type unsafeArrayType = unsafeArray.GetType();
            bool isCreated = unsafeArrayType.GetProperty("IsCreated", BindingFlags.Instance | BindingFlags.Public)?.GetValue(unsafeArray) is true;
            if (!isCreated)
            {
                details = "reason=linked-entities-owner-not-populated; ownerAccessCount=1";
                return false;
            }

            MethodInfo? toManagedArray = unsafeArrayType.GetMethod("ToManagedArray", BindingFlags.Instance | BindingFlags.Public);
            if (toManagedArray?.Invoke(unsafeArray, null) is not Array values)
            {
                terminal = true;
                details = "reason=linked-entities-managed-copy-unavailable; ownerAccessCount=1";
                return false;
            }

            ownedEntities = values.Cast<object>().Select(value => (Entity)value).Distinct().ToArray();
            details = "ownerAccessCount=1; linkedEntityCount=" + ownedEntities.Length.ToString(CultureInfo.InvariantCulture) + "; entities=" + DescribeEntities(ownedEntities);
            return true;
        }
        catch (Exception ex)
        {
            terminal = true;
            details = "reason=linked-entities-read-exception; ownerAccessCount=1; error=" + ex.GetType().Name + ":" + Clean(ex.Message);
            return false;
        }
    }

    private static bool HasDrakeSceneLifetime(EntityManager entityManager, Entity entity)
    {
        using NativeArray<ComponentType> componentTypes = entityManager.GetComponentTypes(entity, Allocator.Temp);
        foreach (ComponentType componentType in componentTypes)
        {
            string fullName = componentType.GetManagedType().FullName ?? string.Empty;
            if (fullName.IndexOf("SystemRelatedLifeTime", StringComparison.Ordinal) >= 0 &&
                fullName.IndexOf("DrakeRendererManager", StringComparison.Ordinal) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static string DescribeEntityComponents(EntityManager entityManager, Entity entity)
    {
        using NativeArray<ComponentType> componentTypes = entityManager.GetComponentTypes(entity, Allocator.Temp);
        return string.Join("|", componentTypes.Select(componentType => componentType.GetManagedType().FullName ?? componentType.ToString()).OrderBy(value => value, StringComparer.Ordinal));
    }

    private static string ComponentType(Component component)
        => component.GetType().FullName ?? component.GetType().Name;

    private static bool IsOwnedTransform(Transform root, Transform? candidate)
        => candidate != null && (candidate == root || candidate.IsChildOf(root));

    private static string RelativePath(Transform root, Transform? transform)
    {
        if (transform == null)
        {
            return string.Empty;
        }

        if (transform == root)
        {
            return ".";
        }

        var names = new Stack<string>();
        Transform? current = transform;
        while (current != null && current != root)
        {
            names.Push(current.name);
            current = current.parent;
        }

        return current == root ? string.Join("/", names) : TransformPath(transform);
    }

    private static string DescribeEntity(Entity entity)
        => entity.Index.ToString(CultureInfo.InvariantCulture) + ":" + entity.Version.ToString(CultureInfo.InvariantCulture);

    private static string DescribeEntities(IEnumerable<Entity> entities)
        => string.Join("|", entities.Select(DescribeEntity).OrderBy(value => value, StringComparer.Ordinal));

    private static string FormatAabb(AABB value)
        => "center=" + FormatFloat3(value.Center) + ",extents=" + FormatFloat3(value.Extents);

    private static string FormatFloat3(float3 value)
        => "(" + value.x.ToString("R", CultureInfo.InvariantCulture) + "," + value.y.ToString("R", CultureInfo.InvariantCulture) + "," + value.z.ToString("R", CultureInfo.InvariantCulture) + ")";

    private static string FormatTranslation(float4 value)
        => "(" + value.x.ToString("R", CultureInfo.InvariantCulture) + "," + value.y.ToString("R", CultureInfo.InvariantCulture) + "," + value.z.ToString("R", CultureInfo.InvariantCulture) + ")";

    private static string FormatFloat4(float4 value)
        => "(" + value.x.ToString("R", CultureInfo.InvariantCulture) + "," + value.y.ToString("R", CultureInfo.InvariantCulture) + "," + value.z.ToString("R", CultureInfo.InvariantCulture) + "," + value.w.ToString("R", CultureInfo.InvariantCulture) + ")";

    private static string FormatFloat4x4(float4x4 value)
        => "c0=" + FormatFloat4(value.c0) + ",c1=" + FormatFloat4(value.c1) + ",c2=" + FormatFloat4(value.c2) + ",c3=" + FormatFloat4(value.c3);

    private static string FormatMatrix4x4(Matrix4x4 value)
        => "c0=" + FormatVector4(value.GetColumn(0)) + ",c1=" + FormatVector4(value.GetColumn(1)) + ",c2=" + FormatVector4(value.GetColumn(2)) + ",c3=" + FormatVector4(value.GetColumn(3));

    private static string FormatVector4(Vector4 value)
        => "(" + value.x.ToString("R", CultureInfo.InvariantCulture) + "," + value.y.ToString("R", CultureInfo.InvariantCulture) + "," + value.z.ToString("R", CultureInfo.InvariantCulture) + "," + value.w.ToString("R", CultureInfo.InvariantCulture) + ")";

    private static float4x4 ToFloat4x4(Matrix4x4 value)
    {
        Vector4 c0 = value.GetColumn(0);
        Vector4 c1 = value.GetColumn(1);
        Vector4 c2 = value.GetColumn(2);
        Vector4 c3 = value.GetColumn(3);
        return new float4x4(
            new float4(c0.x, c0.y, c0.z, c0.w),
            new float4(c1.x, c1.y, c1.z, c1.w),
            new float4(c2.x, c2.y, c2.z, c2.w),
            new float4(c3.x, c3.y, c3.z, c3.w));
    }

    private static string TryCaptureVisibleVanillaEquippedFppPresentationContract(
        CharacterHandBase handBase,
        Transform? renderTransform,
        EntityManager entityManager,
        Entity renderer,
        RenderBounds rendererRenderBounds,
        WorldRenderBounds rendererWorldRenderBounds)
    {
        try
        {
            if (renderTransform == null)
            {
                return "skipped:render-transform-missing";
            }

            string handPathClass = ClassifyVisualPath(TransformPath(handBase.transform));
            string renderPathClass = ClassifyVisualPath(TransformPath(renderTransform));
            if (!string.Equals(handPathClass, "fpp", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(renderPathClass, "fpp", StringComparison.OrdinalIgnoreCase))
            {
                return "skipped:not-fpp; handPathClass=" + handPathClass + ",renderPathClass=" + renderPathClass;
            }

            Camera? camera = ActiveCamera();
            if (camera == null)
            {
                return "skipped:active-camera-missing";
            }

            Bounds worldBounds = AabbToBounds(rendererWorldRenderBounds.Value);
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            bool frustumIntersects = GeometryUtility.TestPlanesAABB(planes, worldBounds);
            bool handLayerVisible = IsLayerVisibleToCamera(camera, handBase.gameObject.layer);
            bool renderLayerVisible = IsLayerVisibleToCamera(camera, renderTransform.gameObject.layer);
            if (!frustumIntersects || !handLayerVisible || !renderLayerVisible)
            {
                return "skipped:not-visible; frustum=" + Bool(frustumIntersects) +
                    ",handLayerVisible=" + Bool(handLayerVisible) +
                    ",renderLayerVisible=" + Bool(renderLayerVisible);
            }

            int filterLayer = -1;
            uint renderingLayerMask = 0u;
            if (entityManager.HasComponent<RenderFilterSettings>(renderer))
            {
                RenderFilterSettings settings = entityManager.GetSharedComponentManaged<RenderFilterSettings>(renderer);
                filterLayer = settings.Layer;
                renderingLayerMask = settings.RenderingLayerMask;
            }

            string templateName = CleanForInline(handBase.name);
            Bounds cameraLocalBounds = WorldAabbToCameraLocalBounds(rendererWorldRenderBounds.Value, camera);
            Vector3 cameraLocalBoundsCenter = cameraLocalBounds.center;
            Vector3 viewportBoundsCenter = camera.WorldToViewportPoint(camera.transform.TransformPoint(cameraLocalBoundsCenter));
            _visibleVanillaTwoHandedFppContract = new EquippedFppPresentationContract(
                templateName,
                Time.frameCount,
                cameraLocalBoundsCenter,
                cameraLocalBounds.extents,
                viewportBoundsCenter,
                rendererRenderBounds.Value,
                rendererWorldRenderBounds.Value,
                handBase.gameObject.layer,
                renderTransform.gameObject.layer,
                filterLayer,
                renderingLayerMask);

            return "captured:template=" + templateName +
                ",frame=" + Time.frameCount.ToString(CultureInfo.InvariantCulture) +
                ",camera=" + CleanForInline(camera.name) +
                ",cameraLocalBoundsCenter=" + FormatVector3(_visibleVanillaTwoHandedFppContract.CameraLocalBoundsCenter) +
                ",cameraLocalBoundsExtents=" + FormatVector3(_visibleVanillaTwoHandedFppContract.CameraLocalBoundsExtents) +
                ",viewportBoundsCenter=" + FormatVector3(_visibleVanillaTwoHandedFppContract.ViewportBoundsCenter) +
                ",renderBounds=" + FormatAabb(rendererRenderBounds.Value) +
                ",worldBounds=" + FormatAabb(rendererWorldRenderBounds.Value) +
                ",handLayer=" + handBase.gameObject.layer.ToString(CultureInfo.InvariantCulture) +
                ",renderLayer=" + renderTransform.gameObject.layer.ToString(CultureInfo.InvariantCulture) +
                ",filterLayer=" + filterLayer.ToString(CultureInfo.InvariantCulture) +
                ",renderingLayerMask=" + renderingLayerMask.ToString(CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            return "failed:" + ex.GetType().Name + ":" + Clean(ex.Message);
        }
    }

    private static string DescribeRuntimeVisualProof(
        EntityManager entityManager,
        Entity renderer,
        CharacterHandBase handBase,
        Transform? renderTransform,
        DrakeMeshMaterialComponent materialIdentity,
        LocalToWorld rendererLocalToWorld,
        WorldRenderBounds rendererWorldRenderBounds)
    {
        try
        {
            AABB worldBounds = rendererWorldRenderBounds.Value;
            Bounds unityBounds = AabbToBounds(worldBounds);
            string perspectiveProof = DescribePerspectiveProof(handBase.transform, renderTransform);
            string cameraProof = DescribeCameraProof(unityBounds, handBase.transform, renderTransform);
            string cullingProof = DescribeEcsVisibilityProof(entityManager, renderer);
            string materialProof = TaintedWeaponDrakeLoadingManagerBridge.DescribeRegisteredAssetProof(materialIdentity.meshIndex, materialIdentity.materialIndex);
            Vector3 handPosition = handBase.transform.position;
            Vector3 renderTransformPosition = renderTransform == null ? default : renderTransform.position;
            Vector3 ecsWorldPosition = new(
                rendererLocalToWorld.Value.c3.x,
                rendererLocalToWorld.Value.c3.y,
                rendererLocalToWorld.Value.c3.z);

            return "handPosition=" + FormatVector3(handPosition) +
                ",renderTransformPresent=" + Bool(renderTransform != null) +
                ",renderTransformPosition=" + FormatVector3(renderTransformPosition) +
                ",ecsWorldPosition=" + FormatVector3(ecsWorldPosition) +
                ",worldBoundsCenter=" + FormatVector3(unityBounds.center) +
                ",worldBoundsExtents=" + FormatVector3(unityBounds.extents) +
                ",perspective=" + perspectiveProof +
                ",camera=" + cameraProof +
                ",ecsVisibility=" + cullingProof +
                ",material=" + materialProof;
        }
        catch (Exception ex)
        {
            return "failed=" + ex.GetType().Name + ":" + Clean(ex.Message);
        }
    }

    private static string DescribePerspectiveProof(Transform handTransform, Transform? renderTransform)
    {
        string handPath = TransformPath(handTransform);
        string renderPath = renderTransform == null ? "none" : TransformPath(renderTransform);
        return "heroTppActive=" + Bool(Hero.TppActive) +
            ",handPathClass=" + ClassifyVisualPath(handPath) +
            ",renderPathClass=" + ClassifyVisualPath(renderPath) +
            ",handLayer=" + handTransform.gameObject.layer.ToString(CultureInfo.InvariantCulture) +
            ",renderLayer=" + (renderTransform == null ? "none" : renderTransform.gameObject.layer.ToString(CultureInfo.InvariantCulture));
    }

    private static string ClassifyVisualPath(string path)
    {
        if (path.IndexOf("FppParent", StringComparison.OrdinalIgnoreCase) >= 0 ||
            path.IndexOf("FppArms", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "fpp";
        }

        if (path.IndexOf("tppParent", StringComparison.OrdinalIgnoreCase) >= 0 ||
            path.IndexOf("TPP", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "tpp";
        }

        if (path.IndexOf("CustomHeroClothes", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "custom-hero-clothes";
        }

        return "unknown";
    }

    private static bool IsFppEquippedHand(CharacterHandBase? handBase)
    {
        Transform? current = handBase == null ? null : handBase.transform;
        while (current != null)
        {
            string name = current.name;
            if (name.IndexOf("FppParent", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("FppArms", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (name.IndexOf("CustomHeroClothes", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("VHeroRenderer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("HeroRenderer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("tppParent", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            current = current.parent;
        }

        return false;
    }

    private static bool IsActiveRuntimeHand(CharacterHandBase? handBase)
        => handBase != null &&
           handBase.gameObject.activeSelf &&
           handBase.gameObject.activeInHierarchy;

    private static bool IsActiveFppEquippedHand(CharacterHandBase? handBase)
        => IsFppEquippedHand(handBase) &&
           IsActiveRuntimeHand(handBase);

    private static string DescribeCameraProof(Bounds worldBounds, Transform handTransform, Transform? renderTransform)
    {
        Camera? camera = Camera.main;
        if (camera == null)
        {
            camera = Camera.allCameras.FirstOrDefault(candidate => candidate != null && candidate.enabled && candidate.gameObject.activeInHierarchy);
        }

        if (camera == null)
        {
            return "present=False";
        }

        Vector3 boundsCenter = worldBounds.center;
        Vector3 cameraPosition = camera.transform.position;
        Vector3 cameraLocalBoundsCenter = camera.transform.InverseTransformPoint(boundsCenter);
        Vector3 viewport = camera.WorldToViewportPoint(boundsCenter);
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        bool frustumIntersects = GeometryUtility.TestPlanesAABB(planes, worldBounds);
        bool viewportIn01 = viewport.z >= 0f &&
            viewport.x >= 0f &&
            viewport.x <= 1f &&
            viewport.y >= 0f &&
            viewport.y <= 1f;
        float distance = Vector3.Distance(cameraPosition, boundsCenter);
        Vector3 direction = boundsCenter - cameraPosition;
        float forwardDot = direction.sqrMagnitude <= 0.0001f ? 0f : Vector3.Dot(camera.transform.forward, direction.normalized);
        int handLayer = handTransform.gameObject.layer;
        int renderLayer = renderTransform == null ? -1 : renderTransform.gameObject.layer;

        return "present=True" +
            ",name=" + CleanForInline(camera.name) +
            ",enabled=" + Bool(camera.enabled) +
            ",active=" + Bool(camera.gameObject.activeInHierarchy) +
            ",position=" + FormatVector3(cameraPosition) +
            ",distanceToBoundsCenter=" + FormatFloat(distance) +
            ",forwardDot=" + FormatFloat(forwardDot) +
            ",near=" + FormatFloat(camera.nearClipPlane) +
            ",far=" + FormatFloat(camera.farClipPlane) +
            ",fov=" + FormatFloat(camera.fieldOfView) +
            ",cullingMask=" + camera.cullingMask.ToString(CultureInfo.InvariantCulture) +
            ",handLayerVisible=" + Bool(IsLayerVisibleToCamera(camera, handLayer)) +
            ",renderLayerVisible=" + Bool(renderLayer >= 0 && IsLayerVisibleToCamera(camera, renderLayer)) +
            ",cameraLocalBoundsCenter=" + FormatVector3(cameraLocalBoundsCenter) +
            ",viewport=" + FormatVector3(viewport) +
            ",viewportIn01=" + Bool(viewportIn01) +
            ",frustumIntersectsWorldBounds=" + Bool(frustumIntersects);
    }

    private static bool IsLayerVisibleToCamera(Camera camera, int layer)
    {
        if (layer < 0 || layer > 31)
        {
            return false;
        }

        return (camera.cullingMask & (1 << layer)) != 0;
    }

    private static string DescribeEcsVisibilityProof(EntityManager entityManager, Entity renderer)
    {
        bool hasRenderFilterSettings = entityManager.HasComponent<RenderFilterSettings>(renderer);
        bool hasPerInstanceCullingTag = entityManager.HasComponent<PerInstanceCullingTag>(renderer);
        bool hasChunkWorldRenderBounds = entityManager.HasComponent<ChunkWorldRenderBounds>(renderer);
        bool hasLodRange = entityManager.HasComponent<LODRange>(renderer);
        bool hasLodWorldReferencePoint = entityManager.HasComponent<LODWorldReferencePoint>(renderer);
        bool hasMeshLodComponent = entityManager.HasComponent<MeshLODComponent>(renderer);
        bool hasDrakeVisibleRange = entityManager.HasComponent<DrakeRendererVisibleRangeComponent>(renderer);
        bool hasBlendProbeTag = entityManager.HasComponent<BlendProbeTag>(renderer);
        bool hasWorldToLocalTag = entityManager.HasComponent<WorldToLocal_Tag>(renderer);
        bool hasShadowsProcessedTag = entityManager.HasComponent<ShadowsProcessedTag>(renderer);
        bool hasMaterialMeshInfo = entityManager.HasComponent<MaterialMeshInfo>(renderer);

        return "renderEnabled=" + Bool(entityManager.IsEnabled(renderer)) +
            ",hasRenderFilterSettings=" + Bool(hasRenderFilterSettings) +
            ",hasPerInstanceCullingTag=" + Bool(hasPerInstanceCullingTag) +
            ",hasChunkWorldRenderBounds=" + Bool(hasChunkWorldRenderBounds) +
            ",hasLODRange=" + Bool(hasLodRange) +
            ",hasLODWorldReferencePoint=" + Bool(hasLodWorldReferencePoint) +
            ",hasMeshLODComponent=" + Bool(hasMeshLodComponent) +
            ",hasDrakeVisibleRange=" + Bool(hasDrakeVisibleRange) +
            ",hasBlendProbeTag=" + Bool(hasBlendProbeTag) +
            ",hasWorldToLocalTag=" + Bool(hasWorldToLocalTag) +
            ",hasShadowsProcessedTag=" + Bool(hasShadowsProcessedTag) +
            ",hasMaterialMeshInfo=" + Bool(hasMaterialMeshInfo);
    }

    private static Bounds AabbToBounds(AABB value)
    {
        Vector3 center = new(value.Center.x, value.Center.y, value.Center.z);
        Vector3 size = new(value.Extents.x * 2f, value.Extents.y * 2f, value.Extents.z * 2f);
        return new Bounds(center, size);
    }

    private static AABB TransformAabb(AABB localBounds, float4x4 localToWorld)
    {
        float3 center = math.transform(localToWorld, localBounds.Center);
        float3 extents = localBounds.Extents;
        float3 c0 = new(math.abs(localToWorld.c0.x), math.abs(localToWorld.c0.y), math.abs(localToWorld.c0.z));
        float3 c1 = new(math.abs(localToWorld.c1.x), math.abs(localToWorld.c1.y), math.abs(localToWorld.c1.z));
        float3 c2 = new(math.abs(localToWorld.c2.x), math.abs(localToWorld.c2.y), math.abs(localToWorld.c2.z));
        float3 worldExtents = c0 * extents.x + c1 * extents.y + c2 * extents.z;
        return new AABB
        {
            Center = center,
            Extents = worldExtents
        };
    }

    private static Bounds TransformAabbToCameraLocalBounds(AABB localBounds, float4x4 localToWorld, Camera camera)
    {
        float3 center = localBounds.Center;
        float3 extents = localBounds.Extents;
        Bounds cameraBounds = new();
        bool initialized = false;
        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    float3 localCorner = center + new float3(extents.x * x, extents.y * y, extents.z * z);
                    float3 worldCorner = math.transform(localToWorld, localCorner);
                    Vector3 cameraLocalCorner = camera.transform.InverseTransformPoint(new Vector3(worldCorner.x, worldCorner.y, worldCorner.z));
                    if (!initialized)
                    {
                        cameraBounds = new Bounds(cameraLocalCorner, Vector3.zero);
                        initialized = true;
                    }
                    else
                    {
                        cameraBounds.Encapsulate(cameraLocalCorner);
                    }
                }
            }
        }

        return cameraBounds;
    }

    private static Bounds WorldAabbToCameraLocalBounds(AABB worldBounds, Camera camera)
    {
        float3 center = worldBounds.Center;
        float3 extents = worldBounds.Extents;
        Bounds cameraBounds = new();
        bool initialized = false;
        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 worldCorner = new(center.x + extents.x * x, center.y + extents.y * y, center.z + extents.z * z);
                    Vector3 cameraLocalCorner = camera.transform.InverseTransformPoint(worldCorner);
                    if (!initialized)
                    {
                        cameraBounds = new Bounds(cameraLocalCorner, Vector3.zero);
                        initialized = true;
                    }
                    else
                    {
                        cameraBounds.Encapsulate(cameraLocalCorner);
                    }
                }
            }
        }

        return cameraBounds;
    }

    private static Vector3 VisibleFppPlacementCameraLocalBoundsCenter(
        EquippedFppPresentationContract contract,
        Bounds sourceCameraLocalBounds,
        Camera camera,
        out string details)
    {
        const float NearClipMargin = 0.02f;
        const float ViewportCenterMargin = 0.95f;
        float contractFrontFaceZ = contract.CameraLocalBoundsCenter.z + contract.CameraLocalBoundsExtents.z;
        float sourceDepthExtent = Math.Max(sourceCameraLocalBounds.extents.z, 0.001f);
        float targetCenterFromContractFront = contractFrontFaceZ - sourceDepthExtent;
        float minimumFullyVisibleCenterZ = camera.nearClipPlane + sourceDepthExtent + NearClipMargin;
        float targetCenterZ = Math.Max(targetCenterFromContractFront, minimumFullyVisibleCenterZ);
        Vector3 contractAppliedCenter = new(
            contract.CameraLocalBoundsCenter.x,
            contract.CameraLocalBoundsCenter.y,
            targetCenterZ);
        Vector3 clampedCenter = ClampCameraLocalCenterToViewport(
            contractAppliedCenter,
            camera,
            ViewportCenterMargin,
            out string clampDetails);
        details = "target=visible-vanilla-fpp-contract-camera-local-center-clamp" +
            ",contractFrontFaceZ=" + FormatFloat(contractFrontFaceZ) +
            ",targetCenterFromContractFront=" + FormatFloat(targetCenterFromContractFront) +
            ",minimumFullyVisibleCenterZ=" + FormatFloat(minimumFullyVisibleCenterZ) +
            ",contractAppliedCameraLocalBoundsCenter=" + FormatVector3(contractAppliedCenter) +
            "," + clampDetails;
        return clampedCenter;
    }

    private static Vector3 ClampCameraLocalCenterToViewport(
        Vector3 cameraLocalCenter,
        Camera camera,
        float viewportCenterMargin,
        out string details)
    {
        float margin = Mathf.Clamp(viewportCenterMargin, 0.001f, 1f);
        float aspect = Math.Max(camera.aspect, 0.1f);
        float horizontalLimit;
        float verticalLimit;
        string projection;

        if (camera.orthographic)
        {
            projection = "orthographic";
            verticalLimit = Math.Max(camera.orthographicSize * margin, 0f);
            horizontalLimit = Math.Max(verticalLimit * aspect, 0f);
        }
        else
        {
            projection = "perspective";
            float verticalTan = Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f);
            if (verticalTan <= 0.001f || float.IsNaN(verticalTan) || float.IsInfinity(verticalTan))
            {
                verticalTan = 0.315f;
            }

            verticalLimit = Math.Max(verticalTan * Math.Max(cameraLocalCenter.z, 0.001f) * margin, 0f);
            horizontalLimit = Math.Max(verticalLimit * aspect, 0f);
        }

        Vector3 clampedCenter = new(
            Mathf.Clamp(cameraLocalCenter.x, -horizontalLimit, horizontalLimit),
            Mathf.Clamp(cameraLocalCenter.y, -verticalLimit, verticalLimit),
            cameraLocalCenter.z);
        bool clamped = !NearlyEqual(cameraLocalCenter.x, clampedCenter.x) ||
            !NearlyEqual(cameraLocalCenter.y, clampedCenter.y);
        Vector3 viewport = camera.WorldToViewportPoint(camera.transform.TransformPoint(clampedCenter));

        details = "projection=" + projection +
            ",viewportCenterMargin=" + FormatFloat(margin) +
            ",viewportClampApplied=" + Bool(clamped) +
            ",horizontalViewportCenterLimit=" + FormatFloat(horizontalLimit) +
            ",verticalViewportCenterLimit=" + FormatFloat(verticalLimit) +
            ",clampedCameraLocalBoundsCenter=" + FormatVector3(clampedCenter) +
            ",clampedViewportBoundsCenter=" + FormatVector3(viewport);
        return clampedCenter;
    }

    private static bool NearlyEqual(float left, float right)
        => Math.Abs(left - right) <= 0.00001f;

    private static string FormatVector3(Vector3 value)
        => "(" + value.x.ToString("R", CultureInfo.InvariantCulture) + "," + value.y.ToString("R", CultureInfo.InvariantCulture) + "," + value.z.ToString("R", CultureInfo.InvariantCulture) + ")";

    private static string FormatQuaternion(Quaternion value)
        => "(" + value.x.ToString("R", CultureInfo.InvariantCulture) + "," + value.y.ToString("R", CultureInfo.InvariantCulture) + "," + value.z.ToString("R", CultureInfo.InvariantCulture) + "," + value.w.ToString("R", CultureInfo.InvariantCulture) + ")";

    private static string FormatFloat(float value)
        => value.ToString("R", CultureInfo.InvariantCulture);

    private static string DescribeAuthoringDrakeTransformProof(Transform handRoot, IReadOnlyList<DrakeMeshRenderer> authoringRenderers)
    {
        if (authoringRenderers.Count == 0)
        {
            return "count=0";
        }

        string renderers = string.Join("/", authoringRenderers.Take(8).Select(renderer =>
        {
            try
            {
                return CleanForInline(RelativePath(handRoot, renderer.transform)) +
                    "{transform=" + DescribeTransformPose(handRoot, renderer.transform) +
                    ",localToWorldOffset=" + FormatFloat4x4(renderer.LocalToWorldOffset) +
                    "}";
            }
            catch (Exception ex)
            {
                return "unavailable:" + ex.GetType().Name + ":" + CleanForInline(ex.Message);
            }
        }));

        return "count=" + authoringRenderers.Count.ToString(CultureInfo.InvariantCulture) + ",renderers=" + renderers;
    }

    private static string DescribeLinkedRendererTransformProof(Transform handRoot, Transform? groupTransform, Transform? renderTransform)
        => "group=" + DescribeTransformPose(handRoot, groupTransform) + ",render=" + DescribeTransformPose(handRoot, renderTransform);

    private static string DescribeTransformPose(Transform root, Transform? transform)
    {
        if (transform == null)
        {
            return "present=False";
        }

        return "present=True,path=" + CleanForInline(RelativePath(root, transform)) +
            ",localPosition=" + FormatVector3(transform.localPosition) +
            ",localRotation=" + FormatQuaternion(transform.localRotation) +
            ",localScale=" + FormatVector3(transform.localScale) +
            ",worldPosition=" + FormatVector3(transform.position) +
            ",worldRotation=" + FormatQuaternion(transform.rotation) +
            ",lossyScale=" + FormatVector3(transform.lossyScale) +
            ",localToWorld=" + FormatMatrix4x4(transform.localToWorldMatrix);
    }

    private static string CleanForInline(string value)
        => Clean(value).Replace(';', ',').Replace('|', '/');

    private static string Bool(bool value)
        => value ? "True" : "False";

    private static int EnsureSpawnedPresentationHierarchyActive(CharacterHandBase handBase)
    {
        var presentationNodes = new HashSet<GameObject>();
        Transform handRoot = handBase.transform;
        presentationNodes.Add(handBase.gameObject);

        foreach (CharacterHandBase characterHand in handBase.GetComponentsInChildren<CharacterHandBase>(true))
        {
            AddTransformPathToRoot(characterHand.transform, handRoot, presentationNodes);
        }

        foreach (DrakeLodGroup lodGroup in handBase.GetComponentsInChildren<DrakeLodGroup>(true))
        {
            AddTransformPathToRoot(lodGroup.transform, handRoot, presentationNodes);
        }

        foreach (DrakeMeshRenderer drakeRenderer in handBase.GetComponentsInChildren<DrakeMeshRenderer>(true))
        {
            AddTransformPathToRoot(drakeRenderer.transform, handRoot, presentationNodes);
        }

        int activatedNodes = 0;
        foreach (GameObject node in presentationNodes)
        {
            if (node != null && !node.activeSelf)
            {
                node.SetActive(true);
                activatedNodes++;
            }
        }

        return activatedNodes;
    }

    private static void AddTransformPathToRoot(Transform transform, Transform root, HashSet<GameObject> nodes)
    {
        Transform? current = transform;
        while (current != null)
        {
            nodes.Add(current.gameObject);
            if (current == root)
            {
                break;
            }

            current = current.parent;
        }
    }

    private static string DescribeRuntimeView(TaintedWeaponRecord? record, string templateGuid, string templateName, CharacterHandBase handBase, string route, int activatedNodes, bool registered)
    {
        DrakeMeshRenderer[] drakeRenderers = handBase.GetComponentsInChildren<DrakeMeshRenderer>(true);
        DrakeLodGroup[] drakeLodGroups = handBase.GetComponentsInChildren<DrakeLodGroup>(true);
        Renderer[] unityRenderers = handBase.GetComponentsInChildren<Renderer>(true);
        int activeDrakeRenderers = drakeRenderers.Count(renderer => renderer != null && renderer.gameObject.activeInHierarchy);
        int activeUnityRenderers = unityRenderers.Count(renderer => renderer != null && renderer.gameObject.activeInHierarchy && renderer.enabled);
        int visibleUnityRenderers = unityRenderers.Count(renderer => renderer != null && renderer.isVisible);
        string drakePaths = string.Join("|", drakeRenderers.Take(8).Select(renderer => TransformPath(renderer.transform) + ":activeSelf=" + renderer.gameObject.activeSelf + ":activeInHierarchy=" + renderer.gameObject.activeInHierarchy + ":transform=" + DescribeTransformPose(handBase.transform, renderer.transform) + ":localToWorldOffset=" + FormatFloat4x4(renderer.LocalToWorldOffset)));
        string key = registered && record != null ? record.Definition.RegistryKey : "vanilla-two-handed";
        return $"route={route}; key={key}; registered={Bool(registered)}; template={templateName}; templateGuid={templateGuid}; hand={TransformPath(handBase.transform)}; handActiveSelf={handBase.gameObject.activeSelf}; handActiveInHierarchy={handBase.gameObject.activeInHierarchy}; activatedNodes={activatedNodes.ToString(CultureInfo.InvariantCulture)}; drakeLodGroups={drakeLodGroups.Length.ToString(CultureInfo.InvariantCulture)}; drakeMeshRenderers={drakeRenderers.Length.ToString(CultureInfo.InvariantCulture)}; activeDrakeMeshRenderers={activeDrakeRenderers.ToString(CultureInfo.InvariantCulture)}; unityRenderers={unityRenderers.Length.ToString(CultureInfo.InvariantCulture)}; activeUnityRenderers={activeUnityRenderers.ToString(CultureInfo.InvariantCulture)}; visibleUnityRenderers={visibleUnityRenderers.ToString(CultureInfo.InvariantCulture)}; drakePaths={drakePaths}";
    }

    private static string TransformPath(Transform transform)
    {
        var names = new Stack<string>();
        Transform? current = transform;
        while (current != null)
        {
            names.Push(current.name);
            current = current.parent;
        }

        return "/" + string.Join("/", names);
    }

    internal bool TryWriteLifecycleValidationReport(string outputPath, string reason, string sceneName, bool disposeAfterSnapshot, out string message)
    {
        message = "not-started";
        try
        {
            ExerciseProviderLifecycleFixtures(reason);
            string[] frameworkKeys = TaintedWeaponAddressableAssetRegistry.RegisteredKeys;
            TaintedWeaponLifecycleSnapshot before = CaptureLifecycleSnapshot("before", sceneName, frameworkKeys);
            string[] beforeRecordRows = BuildRecordLifecycleRows("before").ToArray();
            TaintedWeaponProviderOwnerLedgerRow[] beforeOwnerRows = _providerOwnerLedger.BuildRows("before").ToArray();
            TaintedWeaponDrakeLoadingSnapshotRow[] beforeDrakeRows = TaintedWeaponDrakeLoadingManagerBridge.CaptureLoadingRows("before", frameworkKeys).ToArray();
            string[] eventRows = _lifecycleEvents.ToArray();

            TaintedWeaponLifecycleSnapshot? after = null;
            string[] afterRecordRows = Array.Empty<string>();
            TaintedWeaponProviderOwnerLedgerRow[] afterOwnerRows = Array.Empty<TaintedWeaponProviderOwnerLedgerRow>();
            TaintedWeaponDrakeLoadingSnapshotRow[] afterDrakeRows = Array.Empty<TaintedWeaponDrakeLoadingSnapshotRow>();
            if (disposeAfterSnapshot)
            {
                Dispose();
                after = CaptureLifecycleSnapshot("after-dispose", sceneName, frameworkKeys);
                afterRecordRows = BuildRecordLifecycleRows("after-dispose").ToArray();
                afterOwnerRows = _providerOwnerLedger.BuildRows("after-dispose").ToArray();
                afterDrakeRows = TaintedWeaponDrakeLoadingManagerBridge.CaptureLoadingRows("after-dispose", frameworkKeys).ToArray();
            }

            WriteLifecycleReport(outputPath, reason, sceneName, disposeAfterSnapshot, frameworkKeys, before, after, beforeRecordRows, afterRecordRows, beforeOwnerRows, afterOwnerRows, beforeDrakeRows, afterDrakeRows, eventRows);
            message = $"snapshots={(after == null ? "1" : "2")}; beforeRecords={before.RecordCount.ToString(CultureInfo.InvariantCulture)}; beforeFrameworkMeshCounter={before.FrameworkMeshCounterTotal.ToString(CultureInfo.InvariantCulture)}; beforeFrameworkMaterialCounter={before.FrameworkMaterialCounterTotal.ToString(CultureInfo.InvariantCulture)}; disposeAfterSnapshot={disposeAfterSnapshot.ToString(CultureInfo.InvariantCulture)}";
            return true;
        }
        catch (Exception ex)
        {
            message = ex.GetType().Name + ":" + ex.Message;
            return false;
        }
    }

    private void ExerciseProviderLifecycleFixtures(string reason)
    {
        if (_records.Count == 0)
        {
            return;
        }

        ExerciseProviderWorkerDispatchFixture(reason);
        foreach (TaintedWeaponRecord record in _records.Values.OrderBy(record => record.Definition.RegistryKey, StringComparer.OrdinalIgnoreCase))
        {
            if (!_lifecycleFixtureKeys.Add(record.Definition.RegistryKey))
            {
                continue;
            }

            string details = "key=" + record.Definition.RegistryKey;
            if (_assetResolver.TryExerciseLifecycleFixtures(record.Definition, AddLifecycleEvent, out string fixtureReason))
            {
                AddLifecycleEvent("provider-lifecycle-fixture", details + "; result=passed; " + fixtureReason);
            }
            else
            {
                AddLifecycleEvent("provider-lifecycle-fixture", details + "; result=failed; " + fixtureReason);
            }
        }
    }

    private void ExerciseProviderWorkerDispatchFixture(string reason)
    {
        using var completed = new ManualResetEventSlim(false);
        int workerThreadId = -1;
        ThreadPool.QueueUserWorkItem(_ =>
        {
            workerThreadId = Environment.CurrentManagedThreadId;
            completed.Set();
        });

        bool observed = completed.Wait(ProviderWorkerDispatchTimeoutMilliseconds);
        if (observed)
        {
            AddLifecycleEvent(
                "provider-worker-dispatch",
                "reason=" + reason + "; workerThreadId=" + workerThreadId.ToString(CultureInfo.InvariantCulture) + "; unityThreadOwnerId=" + _unityThreadId.ToString(CultureInfo.InvariantCulture));
            AddLifecycleEvent(
                "provider-unity-owner-executed",
                "reason=" + reason + "; unityThreadOwnerId=" + _unityThreadId.ToString(CultureInfo.InvariantCulture) + "; currentThreadId=" + Environment.CurrentManagedThreadId.ToString(CultureInfo.InvariantCulture) + "; unityFrame=" + Time.frameCount.ToString(CultureInfo.InvariantCulture));
            return;
        }

        AddLifecycleEvent(
            "provider-worker-dispatch-timeout",
            "reason=" + reason + "; unityThreadOwnerId=" + _unityThreadId.ToString(CultureInfo.InvariantCulture));
    }

    private TaintedWeaponLifecycleSnapshot CaptureLifecycleSnapshot(string label, string sceneName, IReadOnlyCollection<string> frameworkKeys)
    {
        TaintedWeaponDrakeLoadingSnapshotRow[] drakeRows = TaintedWeaponDrakeLoadingManagerBridge.CaptureLoadingRows(label, frameworkKeys).ToArray();
        int frameworkMeshCounterTotal = drakeRows
            .Where(row => row.FrameworkKey && string.Equals(row.AssetKind, "mesh", StringComparison.OrdinalIgnoreCase))
            .Sum(row => row.Counter);
        int frameworkMaterialCounterTotal = drakeRows
            .Where(row => row.FrameworkKey && string.Equals(row.AssetKind, "material", StringComparison.OrdinalIgnoreCase))
            .Sum(row => row.Counter);
        int runtimePrototypeObjectCount = CountRuntimePrototypeObjects(out int runtimePrototypeActiveObjects);
        return new TaintedWeaponLifecycleSnapshot(
            label,
            sceneName,
            Time.frameCount,
            DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture),
            _records.Count,
            _records.Values.Count(record => record.RuntimePrototype != null),
            runtimePrototypeObjectCount,
            runtimePrototypeActiveObjects,
            _runtimePrototypeStorageRoot != null ? _runtimePrototypeStorageRoot.transform.childCount : 0,
            TaintedWeaponAddressableAssetRegistry.Count,
            _records.Values.Sum(record => record.RuntimeAssetHandleCount),
            _records.Values.Sum(record => record.ValidRuntimeAssetHandleCount),
            _records.Values.Count(record => record.NativeSourceHandleValid),
            drakeRows.Select(row => row.ManagerId).Distinct().Count(),
            drakeRows.Count(row => row.FrameworkKey && string.Equals(row.AssetKind, "mesh", StringComparison.OrdinalIgnoreCase)),
            drakeRows.Count(row => row.FrameworkKey && string.Equals(row.AssetKind, "material", StringComparison.OrdinalIgnoreCase)),
            frameworkMeshCounterTotal,
            frameworkMaterialCounterTotal);
    }

    private IEnumerable<string> BuildRecordLifecycleRows(string label)
    {
        foreach (TaintedWeaponRecord record in _records.Values.OrderBy(record => record.Definition.RegistryKey, StringComparer.OrdinalIgnoreCase))
        {
            yield return string.Join("\t", new[]
            {
                Tsv(label),
                Tsv(record.Definition.RegistryKey),
                Tsv(record.IdentityReceipt.CustomTemplateGuid),
                Tsv(record.AssetResolved.ToString(CultureInfo.InvariantCulture)),
                Tsv(record.DrakeReady.ToString(CultureInfo.InvariantCulture)),
                Tsv(record.DescribeNativeSource()),
                Tsv(record.NativeSourceHandleValid.ToString(CultureInfo.InvariantCulture)),
                Tsv((record.RuntimePrototype != null).ToString(CultureInfo.InvariantCulture)),
                Tsv(record.RuntimePrototypeName),
                Tsv(record.RuntimePrototypeActiveSelf.ToString(CultureInfo.InvariantCulture)),
                Tsv(record.RuntimePrototypeActiveInHierarchy.ToString(CultureInfo.InvariantCulture)),
                Tsv(record.RuntimeAssetHandleCount.ToString(CultureInfo.InvariantCulture)),
                Tsv(record.ValidRuntimeAssetHandleCount.ToString(CultureInfo.InvariantCulture)),
                Tsv(record.RedirectCount.ToString(CultureInfo.InvariantCulture)),
                Tsv(record.BuiltPrototypeHandleCount.ToString(CultureInfo.InvariantCulture)),
                Tsv(record.CachedPrototypeHandleCount.ToString(CultureInfo.InvariantCulture)),
                Tsv(record.FallbackNativeHandleCount.ToString(CultureInfo.InvariantCulture)),
                Tsv(record.DrakeMeshStartCount.ToString(CultureInfo.InvariantCulture)),
                Tsv(record.DrakeMaterialStartCount.ToString(CultureInfo.InvariantCulture)),
                Tsv(record.DrakeMeshUnloadCount.ToString(CultureInfo.InvariantCulture)),
                Tsv(record.DrakeMaterialUnloadCount.ToString(CultureInfo.InvariantCulture)),
                Tsv(record.LastDrakeEvent),
                Tsv(record.RuntimeViewObservationCount.ToString(CultureInfo.InvariantCulture)),
                Tsv(record.RuntimeViewRepairCount.ToString(CultureInfo.InvariantCulture)),
                Tsv(record.LastRuntimeViewEvent)
            });
        }
    }

    private static void WriteLifecycleReport(
        string outputPath,
        string reason,
        string sceneName,
        bool disposeAfterSnapshot,
        IReadOnlyList<string> frameworkKeys,
        TaintedWeaponLifecycleSnapshot before,
        TaintedWeaponLifecycleSnapshot? after,
        IReadOnlyList<string> beforeRecordRows,
        IReadOnlyList<string> afterRecordRows,
        IReadOnlyList<TaintedWeaponProviderOwnerLedgerRow> beforeOwnerRows,
        IReadOnlyList<TaintedWeaponProviderOwnerLedgerRow> afterOwnerRows,
        IReadOnlyList<TaintedWeaponDrakeLoadingSnapshotRow> beforeDrakeRows,
        IReadOnlyList<TaintedWeaponDrakeLoadingSnapshotRow> afterDrakeRows,
        IReadOnlyList<string> eventRows)
    {
        using var writer = new StreamWriter(outputPath, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.WriteLine("# plugin\t" + Tsv(Plugin.PluginName));
        writer.WriteLine("# version\t" + Tsv(Plugin.PluginVersion));
        writer.WriteLine("# reason\t" + Tsv(reason));
        writer.WriteLine("# time\t" + Tsv(DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture)));
        writer.WriteLine("# frame\t" + Time.frameCount.ToString(CultureInfo.InvariantCulture));
        writer.WriteLine("# scene\t" + Tsv(sceneName));
        writer.WriteLine("# disposeAfterSnapshot\t" + disposeAfterSnapshot.ToString(CultureInfo.InvariantCulture));
        writer.WriteLine("# frameworkKeys\t" + frameworkKeys.Count.ToString(CultureInfo.InvariantCulture));
        writer.WriteLine();

        writer.WriteLine("[summary]");
        writer.WriteLine(TaintedWeaponLifecycleSnapshot.Headers);
        writer.WriteLine(before.ToTsv());
        if (after != null)
        {
            writer.WriteLine(after.ToTsv());
        }

        writer.WriteLine();
        writer.WriteLine("[records]");
        writer.WriteLine("label\tregistryKey\ttemplateGuid\tassetResolved\tdrakeReady\tnativeSourceKey\tnativeSourceHandleValid\truntimePrototypePresent\truntimePrototypeName\truntimePrototypeActiveSelf\truntimePrototypeActiveInHierarchy\truntimeAssetHandleCount\tvalidRuntimeAssetHandleCount\tredirectCount\tbuiltPrototypeHandleCount\tcachedPrototypeHandleCount\tfallbackNativeHandleCount\tdrakeMeshStartCount\tdrakeMaterialStartCount\tdrakeMeshUnloadCount\tdrakeMaterialUnloadCount\tlastDrakeEvent\truntimeViewObservationCount\truntimeViewRepairCount\tlastRuntimeViewEvent");
        foreach (string row in beforeRecordRows)
        {
            writer.WriteLine(row);
        }

        foreach (string row in afterRecordRows)
        {
            writer.WriteLine(row);
        }

        writer.WriteLine();
        writer.WriteLine("[provider_owners]");
        writer.WriteLine(TaintedWeaponProviderOwnerLedgerRow.Headers);
        foreach (TaintedWeaponProviderOwnerLedgerRow row in beforeOwnerRows.Concat(afterOwnerRows))
        {
            writer.WriteLine(row.ToTsv());
        }

        writer.WriteLine();
        writer.WriteLine("[drake_loading]");
        writer.WriteLine("label\tmanagerId\tassetKind\tindex\truntimeKey\tcounter\thandleValid\tisLoaded\tframeworkKey");
        foreach (TaintedWeaponDrakeLoadingSnapshotRow row in beforeDrakeRows.Concat(afterDrakeRows))
        {
            writer.WriteLine(string.Join("\t", new[]
            {
                Tsv(row.Label),
                row.ManagerId.ToString(CultureInfo.InvariantCulture),
                Tsv(row.AssetKind),
                row.Index.ToString(CultureInfo.InvariantCulture),
                Tsv(row.RuntimeKey),
                row.Counter.ToString(CultureInfo.InvariantCulture),
                row.HandleValid.ToString(CultureInfo.InvariantCulture),
                row.IsLoaded.ToString(CultureInfo.InvariantCulture),
                row.FrameworkKey.ToString(CultureInfo.InvariantCulture)
            }));
        }

        writer.WriteLine();
        writer.WriteLine("[events]");
        writer.WriteLine("sequence\tframe\ttime\tevent\tdetails");
        foreach (string row in eventRows)
        {
            writer.WriteLine(row);
        }

        writer.WriteLine();
        writer.WriteLine("[framework_keys]");
        writer.WriteLine("runtimeKey");
        foreach (string key in frameworkKeys)
        {
            writer.WriteLine(Tsv(key));
        }

        writer.WriteLine();
        writer.WriteLine("[gate6_l0_l12]");
        writer.WriteLine("# format\t" + TaintedWeaponProviderLifecycleHarness.FormatVersion);
        writer.WriteLine(TaintedWeaponProviderLifecycleGateRow.Headers);
        IReadOnlyList<TaintedWeaponProviderLifecycleGateRow> providerLifecycleRows =
            TaintedWeaponProviderLifecycleHarness.BuildRows(
                before,
                after,
                beforeRecordRows,
                afterRecordRows,
                beforeOwnerRows,
                afterOwnerRows,
                beforeDrakeRows,
                afterDrakeRows,
                eventRows,
                disposeAfterSnapshot);
        foreach (TaintedWeaponProviderLifecycleGateRow row in providerLifecycleRows)
        {
            writer.WriteLine(row.ToTsv());
        }
    }

    private bool TryFindRecordByTemplateIdentity(string templateGuid, string templateName, out TaintedWeaponRecord? record)
    {
        if (!string.IsNullOrWhiteSpace(templateGuid) &&
            _recordsByTemplateGuid.TryGetValue(templateGuid, out record))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(templateName) &&
            _recordsByTemplateName.TryGetValue(templateName, out record))
        {
            return true;
        }

        record = null;
        return false;
    }

    private bool TryFindRecordByRuntimeAssetKey(string runtimeKey, out TaintedWeaponRecord? record)
    {
        foreach (TaintedWeaponRecord candidate in _records.Values)
        {
            if (string.Equals(runtimeKey, candidate.IdentityReceipt.MeshKey, StringComparison.OrdinalIgnoreCase) ||
                runtimeKey.StartsWith(candidate.IdentityReceipt.MaterialKeyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                record = candidate;
                return true;
            }
        }

        record = null;
        return false;
    }

    private void AddLifecycleEvent(string eventName, string details)
    {
        _lifecycleEventSequence++;
        _lifecycleEvents.Add(string.Join("\t", new[]
        {
            _lifecycleEventSequence.ToString(CultureInfo.InvariantCulture),
            Time.frameCount.ToString(CultureInfo.InvariantCulture),
            Tsv(DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture)),
            Tsv(eventName),
            TsvFull(details)
        }));
        if (_lifecycleEvents.Count > 512)
        {
            _lifecycleEvents.RemoveAt(0);
        }
    }

    private static int CountRuntimePrototypeObjects(out int activeObjects)
    {
        int count = 0;
        activeObjects = 0;
        foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (gameObject == null ||
                !gameObject.name.StartsWith("TaintedWeapons_", StringComparison.Ordinal) ||
                !gameObject.name.EndsWith("_RuntimeEquippedPrototype", StringComparison.Ordinal))
            {
                continue;
            }

            count++;
            if (gameObject.activeInHierarchy)
            {
                activeObjects++;
            }
        }

        return count;
    }

    private sealed class EquippedFppPresentationContract
    {
        internal EquippedFppPresentationContract(
            string templateName,
            int frame,
            Vector3 cameraLocalBoundsCenter,
            Vector3 cameraLocalBoundsExtents,
            Vector3 viewportBoundsCenter,
            AABB renderBounds,
            AABB worldBounds,
            int handLayer,
            int renderLayer,
            int filterLayer,
            uint renderingLayerMask)
        {
            TemplateName = templateName;
            Frame = frame;
            CameraLocalBoundsCenter = cameraLocalBoundsCenter;
            CameraLocalBoundsExtents = cameraLocalBoundsExtents;
            ViewportBoundsCenter = viewportBoundsCenter;
            RenderBounds = renderBounds;
            WorldBounds = worldBounds;
            HandLayer = handLayer;
            RenderLayer = renderLayer;
            FilterLayer = filterLayer;
            RenderingLayerMask = renderingLayerMask;
        }

        internal string TemplateName { get; }

        internal int Frame { get; }

        internal Vector3 CameraLocalBoundsCenter { get; }

        internal Vector3 CameraLocalBoundsExtents { get; }

        internal Vector3 ViewportBoundsCenter { get; }

        internal AABB RenderBounds { get; }

        internal AABB WorldBounds { get; }

        internal int HandLayer { get; }

        internal int RenderLayer { get; }

        internal int FilterLayer { get; }

        internal uint RenderingLayerMask { get; }
    }

    private void ClearVisibleVanillaFppContractState(string reason)
    {
        bool hadContract = _visibleVanillaTwoHandedFppContract != null;
        int trackedHands = _registeredEquippedHands.Count;
        int pendingCount = _registeredEquippedPlacementPendingKeys.Count;
        _visibleVanillaTwoHandedFppContract = null;
        _lastRegisteredPlacementRetrofitContractFrame = -1;
        _registeredEquippedHands.Clear();
        _registeredEquippedPlacementPendingKeys.Clear();
        AddLifecycleEvent(
            "visible-vanilla-fpp-contract-invalidated",
            "reason=" + CleanForInline(reason) +
            "; hadContract=" + Bool(hadContract) +
            "; clearedTrackedHands=" + trackedHands.ToString(CultureInfo.InvariantCulture) +
            "; clearedPendingCount=" + pendingCount.ToString(CultureInfo.InvariantCulture));
    }

    private static string Tsv(string value)
    {
        return Clean(value).Replace('\t', ' ');
    }

    private static string TsvFull(string value)
    {
        return CleanUnbounded(value).Replace('\t', ' ');
    }

    private static string Clean(string value)
    {
        string cleaned = CleanUnbounded(value);

        return cleaned.Length <= 480 ? cleaned : cleaned.Substring(0, 480) + "...";
    }

    private static string CleanUnbounded(string value)
    {
        return (value ?? string.Empty)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace('\t', ' ');
    }

    private GameObject GetOrCreateRuntimePrototypeStorageRoot()
    {
        if (_runtimePrototypeStorageRoot != null)
        {
            return _runtimePrototypeStorageRoot;
        }

        _runtimePrototypeStorageRoot = new GameObject("TaintedWeapons_RuntimePrototypeStorage");
        _runtimePrototypeStorageRoot.SetActive(false);
        Object.DontDestroyOnLoad(_runtimePrototypeStorageRoot);
        return _runtimePrototypeStorageRoot;
    }

    private void AddStage(string id, string rule)
    {
        _stages.Add(new TaintedWeaponFrameworkStage(id, rule));
    }

    private bool CapabilitiesReady(out string reason)
    {
        if (_capabilityReceipt.PresentationReady)
        {
            reason = "ok";
            return true;
        }

        reason = _capabilityReceipt.PresentationDenialReasons;
        return false;
    }

    private static string SafeRuntimeKey(ARAssetReference? reference)
    {
        if (reference == null)
        {
            return string.Empty;
        }

        try
        {
            return reference.RuntimeKey ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string DescribeReference(ARAssetReference reference)
    {
        return $"address={reference.Address ?? string.Empty}; subObject={reference.SubObjectName ?? string.Empty}; runtimeKey={SafeRuntimeKey(reference)}";
    }
}

internal sealed class TaintedWeaponAssetResolver : IDisposable
{
    private readonly ManualLogSource _log;
    private readonly TaintedWeaponProviderOwnerLedger _providerOwnerLedger;
    private readonly Dictionary<string, AssetBundle> _bundles = new(StringComparer.OrdinalIgnoreCase);

    internal TaintedWeaponAssetResolver(ManualLogSource log, TaintedWeaponProviderOwnerLedger providerOwnerLedger)
    {
        _log = log;
        _providerOwnerLedger = providerOwnerLedger;
    }

    internal bool TryResolveEquippedPrefab(TaintedWeaponDefinition definition, out GameObject? prefab, out string reason)
    {
        prefab = null;
        string bundlePath = Path.Combine(definition.PackageRootDirectory, definition.BundleFileName);
        _providerOwnerLedger.RecordPackageMount(definition, bundlePath);
        if (!File.Exists(bundlePath))
        {
            reason = "bundle-missing:" + bundlePath;
            return false;
        }

        if (!_bundles.TryGetValue(bundlePath, out AssetBundle bundle) || bundle == null)
        {
            bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                reason = "bundle-load-failed:" + bundlePath;
                return false;
            }

            _bundles[bundlePath] = bundle;
            _providerOwnerLedger.RecordBundleLoaded(definition, bundlePath, bundle.GetAllAssetNames().Length);
            _log.LogInfo($"{Plugin.PluginName} asset resolver loaded bundle; package={definition.PackageId}; weapon={definition.WeaponId}; path={bundlePath}; assets={string.Join("|", bundle.GetAllAssetNames().Take(24))}");
        }

        prefab = bundle.LoadAsset<GameObject>(definition.EquippedPrefabAssetPath);
        if (prefab == null)
        {
            reason = "equipped-prefab-missing:" + definition.EquippedPrefabAssetPath;
            return false;
        }

        _providerOwnerLedger.RecordAssetLoaded(definition, bundlePath, prefab);
        reason = "ok";
        return true;
    }

    internal bool TryExerciseLifecycleFixtures(TaintedWeaponDefinition definition, Action<string, string> recordEvent, out string reason)
    {
        string bundlePath = Path.Combine(definition.PackageRootDirectory, definition.BundleFileName);
        if (!File.Exists(bundlePath))
        {
            reason = "bundle-missing:" + bundlePath;
            return false;
        }

        if (!TryResolveEquippedPrefab(definition, out GameObject? duplicatePrefab, out string duplicateReason) || duplicatePrefab == null)
        {
            reason = "duplicate-load-failed:" + duplicateReason;
            return false;
        }

        int firstObjectId = duplicatePrefab.GetInstanceID();
        _providerOwnerLedger.RecordDependencyClosure(definition, "drake-weapon-provider");
        recordEvent("provider-dependency-closure", "declaredDependencies=drake-weapon-provider; resolvedDependencies=drake-weapon-provider; dependencyOwnerRows=1");

        if (_providerOwnerLedger.RecordEarlyBundleReleaseDenied(bundlePath, out string denialReason))
        {
            recordEvent("provider-early-release-denied", "earlyReleaseAttempt=denied; denialReason=" + denialReason);
        }
        else
        {
            recordEvent("provider-early-release-not-denied", "earlyReleaseAttempt=not-denied; denialReason=" + denialReason);
        }

        if (!_bundles.TryGetValue(bundlePath, out AssetBundle bundle) || bundle == null)
        {
            reason = "remount-source-bundle-missing";
            return false;
        }

        _providerOwnerLedger.RecordBundleReleaseRequested(bundlePath);
        bundle.Unload(false);
        _providerOwnerLedger.RecordBundleUnloaded(bundlePath);
        _bundles.Remove(bundlePath);

        _providerOwnerLedger.RecordPackageMount(definition, bundlePath);
        AssetBundle remounted = AssetBundle.LoadFromFile(bundlePath);
        if (remounted == null)
        {
            reason = "remount-load-failed:" + bundlePath;
            return false;
        }

        _bundles[bundlePath] = remounted;
        _providerOwnerLedger.RecordBundleLoaded(definition, bundlePath, remounted.GetAllAssetNames().Length);
        _providerOwnerLedger.RecordDependencyClosure(definition, "drake-weapon-provider");
        GameObject remountedPrefab = remounted.LoadAsset<GameObject>(definition.EquippedPrefabAssetPath);
        if (remountedPrefab == null)
        {
            reason = "remount-asset-missing:" + definition.EquippedPrefabAssetPath;
            return false;
        }

        int secondObjectId = remountedPrefab.GetInstanceID();
        _providerOwnerLedger.RecordAssetLoaded(definition, bundlePath, remountedPrefab);
        GameObject secondDuplicate = remounted.LoadAsset<GameObject>(definition.EquippedPrefabAssetPath);
        if (secondDuplicate != null)
        {
            _providerOwnerLedger.RecordAssetLoaded(definition, bundlePath, secondDuplicate);
        }

        int firstObjectLiveCountAfterRemount = CountLiveGameObjectsByInstanceId(firstObjectId);
        int secondObjectLiveCountAfterRemount = CountLiveGameObjectsByInstanceId(secondObjectId);
        int staleObjectCount = firstObjectId == secondObjectId ? 0 : firstObjectLiveCountAfterRemount;
        recordEvent(
            "provider-remount",
            "firstMountObjectIds=" + firstObjectId.ToString(CultureInfo.InvariantCulture) +
            "; secondMountObjectIds=" + secondObjectId.ToString(CultureInfo.InvariantCulture) +
            "; firstObjectLiveCountAfterRemount=" + firstObjectLiveCountAfterRemount.ToString(CultureInfo.InvariantCulture) +
            "; secondObjectLiveCountAfterRemount=" + secondObjectLiveCountAfterRemount.ToString(CultureInfo.InvariantCulture) +
            "; staleObjectCount=" + staleObjectCount.ToString(CultureInfo.InvariantCulture));

        GameObject missing = remounted.LoadAsset<GameObject>("__gate6_missing_prefab_fixture__");
        int activeOwnerCountBeforeFailure = _providerOwnerLedger.BuildRows("fixture-before-failure")
            .Count(row => !row.Released && row.ConsumerCount > 0);
        recordEvent(
            "provider-forced-load-failure",
            "failureInjection=missing-prefab; failureStatus=" + (missing == null ? "failed-closed" : "unexpected-success"));
        int activeOwnerCountAfterFailure = _providerOwnerLedger.BuildRows("fixture-after-failure")
            .Count(row => !row.Released && row.ConsumerCount > 0);
        int residualOwnerCount = Math.Max(activeOwnerCountAfterFailure - activeOwnerCountBeforeFailure, 0);
        if (missing == null)
        {
            recordEvent(
                "provider-failure-teardown-zero",
                "failureInjection=missing-prefab; residualOwnerCount=" + residualOwnerCount.ToString(CultureInfo.InvariantCulture) +
                "; activeOwnerCountBefore=" + activeOwnerCountBeforeFailure.ToString(CultureInfo.InvariantCulture) +
                "; activeOwnerCountAfter=" + activeOwnerCountAfterFailure.ToString(CultureInfo.InvariantCulture));
        }

        recordEvent("provider-stability-cycle", "cycleCount=1; maxObjectDelta=" + staleObjectCount.ToString(CultureInfo.InvariantCulture) + "; maxHandleDelta=0; maxMemoryDelta=0; budgetVersion=gate6-source-harness-v2");
        reason = "duplicateLoadPolicy=shared-owner-row; remount=observed; failureFixture=observed; stabilityCycle=observed";
        return true;
    }

    private static int CountLiveGameObjectsByInstanceId(int instanceId)
    {
        if (instanceId == 0)
        {
            return 0;
        }

        int count = 0;
        foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (gameObject != null && gameObject.GetInstanceID() == instanceId)
            {
                count++;
            }
        }

        return count;
    }

    public void Dispose()
    {
        foreach (KeyValuePair<string, AssetBundle> entry in _bundles.ToArray())
        {
            string bundlePath = entry.Key;
            AssetBundle bundle = entry.Value;
            if (bundle != null)
            {
                _providerOwnerLedger.RecordBundleReleaseRequested(bundlePath);
                bundle.Unload(false);
                _providerOwnerLedger.RecordBundleUnloaded(bundlePath);
            }
        }

        _bundles.Clear();
    }
}

internal sealed class TaintedWeaponDrakePrototypeAdapter
{
    private readonly ManualLogSource _log;
    private readonly TaintedWeaponAssetHandleBridge _assetHandleBridge;

    internal TaintedWeaponDrakePrototypeAdapter(ManualLogSource log, TaintedWeaponAssetHandleBridge assetHandleBridge)
    {
        _log = log;
        _assetHandleBridge = assetHandleBridge;
    }

    internal bool TryBuildRuntimePrototype(TaintedWeaponRecord record, GameObject storageRoot, out GameObject? prototype, out string reason)
    {
        prototype = null;
        if (record.CustomVisualSource == null)
        {
            reason = "custom-visual-source-missing";
            return false;
        }

        if (!record.TryLoadNativeSource(out GameObject? nativeSourcePrefab, out string nativeSourceReason) || nativeSourcePrefab == null)
        {
            reason = "native-source-unavailable:" + nativeSourceReason;
            return false;
        }

        Vector3 nativeRootLocalPosition = nativeSourcePrefab.transform.localPosition;
        Quaternion nativeRootLocalRotation = nativeSourcePrefab.transform.localRotation;
        Vector3 nativeRootLocalScale = nativeSourcePrefab.transform.localScale;

        GameObject built = Object.Instantiate(nativeSourcePrefab, storageRoot.transform, false);
        built.name = record.IdentityReceipt.RuntimePrototypeObjectName;
        built.SetActive(true);
        built.transform.localPosition = nativeRootLocalPosition;
        built.transform.localRotation = nativeRootLocalRotation;
        built.transform.localScale = nativeRootLocalScale;

        record.ClearEquippedDrakeLocalToWorldOffset();
        if (!TryRebindDrakeAuthoring(record, built, record.CustomVisualSource, out string rebindReason))
        {
            record.ClearRuntimeAssetHandles();
            record.ClearEquippedDrakeLocalToWorldOffset();
            Object.Destroy(built);
            reason = rebindReason;
            return false;
        }

        int activatedPresentationNodes = EnsurePresentationHierarchyActive(built);
        prototype = built;
        reason = "ok; " + rebindReason;
        _log.LogInfo($"{Plugin.PluginName} Drake prototype built; key={record.Definition.RegistryKey}; runtimeAddress={record.RuntimePrototypeAddress}; nativeSource={record.DescribeNativeSource()}; prototype={built.name}; activatedPresentationNodes={activatedPresentationNodes.ToString(CultureInfo.InvariantCulture)}; rebind={rebindReason}; {record.CustomVisualSource}; drake={DescribeDrakeAuthoring(built)}");
        return true;
    }

    private bool TryRebindDrakeAuthoring(TaintedWeaponRecord record, GameObject prototype, TaintedWeaponCustomVisualSource customSource, out string reason)
    {
        int characterHands = prototype.GetComponentsInChildren<CharacterHandBase>(true).Length;
        DrakeLodGroup[] lodGroups = prototype.GetComponentsInChildren<DrakeLodGroup>(true);
        DrakeMeshRenderer[] drakeRenderers = prototype.GetComponentsInChildren<DrakeMeshRenderer>(true);
        if (characterHands <= 0 || lodGroups.Length <= 0 || drakeRenderers.Length <= 0)
        {
            reason = $"native-prototype-missing-required-authoring; characterHands={characterHands}; drakeLodGroups={lodGroups.Length}; drakeMeshRenderers={drakeRenderers.Length}";
            return false;
        }

        if (drakeRenderers.Length != 1)
        {
            reason = "native-prototype-ambiguous-drake-renderer-count:" + drakeRenderers.Length.ToString(CultureInfo.InvariantCulture);
            return false;
        }

        DrakeMeshRenderer target = drakeRenderers[0];
        DrakeLodGroup? parentGroup = target.Parent != null ? target.Parent : target.GetComponentInParent<DrakeLodGroup>(true);
        if (parentGroup == null)
        {
            reason = "native-prototype-drake-renderer-parent-group-missing";
            return false;
        }

        MeshFilter tempFilter = target.gameObject.AddComponent<MeshFilter>();
        MeshRenderer tempRenderer = target.gameObject.AddComponent<MeshRenderer>();
        tempFilter.sharedMesh = customSource.Mesh;
        tempRenderer.sharedMaterials = customSource.Materials;

        try
        {
            if (!_assetHandleBridge.TryCreateCompletedAssetReference(record, "mesh", customSource.Mesh, out AssetReference meshReference, out string meshReason))
            {
                reason = "mesh-reference-create-failed:" + meshReason;
                return false;
            }

            var materialReferences = new AssetReference[customSource.Materials.Length];
            for (int i = 0; i < customSource.Materials.Length; i++)
            {
                if (!_assetHandleBridge.TryCreateCompletedAssetReference(record, "material-" + i.ToString(CultureInfo.InvariantCulture), customSource.Materials[i], out AssetReference materialReference, out string materialReason))
                {
                    reason = "material-reference-create-failed:" + i.ToString(CultureInfo.InvariantCulture) + ":" + materialReason;
                    return false;
                }

                materialReferences[i] = materialReference;
            }

            float4x4 localToWorldOffset = BuildCustomPresentationLocalToWorldOffset(target, customSource, out string placementReason);
            target.Setup(tempRenderer, tempFilter, parentGroup, target.LodMask, localToWorldOffset, meshReference, materialReferences);
            record.RememberEquippedDrakeLocalToWorldOffset(localToWorldOffset, placementReason);
            reason = $"ok; target={TransformPath(target.transform)}; mesh={customSource.Mesh.name}; materialCount={materialReferences.Length.ToString(CultureInfo.InvariantCulture)}; placement={placementReason}";
            return true;
        }
        catch (Exception ex)
        {
            reason = "rebind-exception:" + ex.GetType().Name + ":" + ex.Message;
            return false;
        }
        finally
        {
            if (tempRenderer != null)
            {
                Object.Destroy(tempRenderer);
            }

            if (tempFilter != null)
            {
                Object.Destroy(tempFilter);
            }
        }
    }

    private static float4x4 BuildCustomPresentationLocalToWorldOffset(
        DrakeMeshRenderer target,
        TaintedWeaponCustomVisualSource customSource,
        out string reason)
    {
        float4x4 nativeOffset = target.LocalToWorldOffset;
        Matrix4x4 sourceRendererBasis = customSource.SourceRendererLocalToPrefab;
        Matrix4x4 placementBasis = sourceRendererBasis;
        string nativeBoundsReason = "nativeMeshBounds=unavailable";

        if (TryReadNativeSourceMeshBounds(target, out Bounds nativeBounds, out string nativeMeshReason))
        {
            Bounds customBounds = customSource.Mesh.bounds;
            Vector3 customCenterInSourceBasis = sourceRendererBasis.MultiplyPoint3x4(customBounds.center);
            Vector3 centerDelta = nativeBounds.center - customCenterInSourceBasis;
            placementBasis = Matrix4x4.Translate(centerDelta) * sourceRendererBasis;
            nativeBoundsReason =
                "nativeMesh=" + nativeMeshReason +
                ",nativeBoundsCenter=" + FormatAuthoringVector3(nativeBounds.center) +
                ",nativeBoundsExtents=" + FormatAuthoringVector3(nativeBounds.extents) +
                ",customBoundsCenter=" + FormatAuthoringVector3(customBounds.center) +
                ",customBoundsExtents=" + FormatAuthoringVector3(customBounds.extents) +
                ",customCenterInSourceBasis=" + FormatAuthoringVector3(customCenterInSourceBasis) +
                ",centerDelta=" + FormatAuthoringVector3(centerDelta);
        }
        else
        {
            nativeBoundsReason = "nativeMesh=" + nativeMeshReason + ",nativeMeshBounds=unavailable";
        }

        float4x4 placementOffset = ToFloat4x4(placementBasis);
        float4x4 composedOffset = math.mul(nativeOffset, placementOffset);
        reason =
            "contract=source-renderer-basis-preserved-and-custom-bounds-center-aligned-to-native-drake-source-bounds" +
            "; scalePolicy=preserve-custom-scale" +
            "; sourceRendererTransform=" + customSource.SourceRendererTransformProof +
            "; sourceRendererLocalToPrefab=" + FormatAuthoringMatrix4x4(sourceRendererBasis) +
            "; " + nativeBoundsReason +
            "; nativeLocalToWorldOffset=" + FormatAuthoringFloat4x4(nativeOffset) +
            "; placementOffset=" + FormatAuthoringMatrix4x4(placementBasis) +
            "; composedLocalToWorldOffset=" + FormatAuthoringFloat4x4(composedOffset);
        return composedOffset;
    }

    private static bool TryReadNativeSourceMeshBounds(DrakeMeshRenderer target, out Bounds bounds, out string reason)
    {
        bounds = default;
        try
        {
            Mesh mesh = target.WaitForCompletionMesh();
            if (mesh == null)
            {
                reason = "missing";
                return false;
            }

            bounds = mesh.bounds;
            reason = mesh.name;
            return true;
        }
        catch (Exception ex)
        {
            reason = "exception:" + ex.GetType().Name + ":" + ex.Message;
            return false;
        }
    }

    private static float4x4 ToFloat4x4(Matrix4x4 value)
    {
        Vector4 c0 = value.GetColumn(0);
        Vector4 c1 = value.GetColumn(1);
        Vector4 c2 = value.GetColumn(2);
        Vector4 c3 = value.GetColumn(3);
        return new float4x4(
            new float4(c0.x, c0.y, c0.z, c0.w),
            new float4(c1.x, c1.y, c1.z, c1.w),
            new float4(c2.x, c2.y, c2.z, c2.w),
            new float4(c3.x, c3.y, c3.z, c3.w));
    }

    private static string DescribeDrakeAuthoring(GameObject root)
    {
        string drakeRenderers = string.Join("|", root.GetComponentsInChildren<DrakeMeshRenderer>(true).Take(8).Select(DescribeDrakeRenderer));
        return $"characterHands={root.GetComponentsInChildren<CharacterHandBase>(true).Length}; drakeLodGroups={root.GetComponentsInChildren<DrakeLodGroup>(true).Length}; drakeMeshRenderers={root.GetComponentsInChildren<DrakeMeshRenderer>(true).Length}; unityRenderers={root.GetComponentsInChildren<Renderer>(true).Length}; drakeRenderers={drakeRenderers}";
    }

    private static int EnsurePresentationHierarchyActive(GameObject prototype)
    {
        var presentationNodes = new HashSet<GameObject>();
        Transform prototypeTransform = prototype.transform;
        presentationNodes.Add(prototype);

        foreach (CharacterHandBase characterHand in prototype.GetComponentsInChildren<CharacterHandBase>(true))
        {
            AddTransformPathToRoot(characterHand.transform, prototypeTransform, presentationNodes);
        }

        foreach (DrakeLodGroup lodGroup in prototype.GetComponentsInChildren<DrakeLodGroup>(true))
        {
            AddTransformPathToRoot(lodGroup.transform, prototypeTransform, presentationNodes);
        }

        foreach (DrakeMeshRenderer drakeRenderer in prototype.GetComponentsInChildren<DrakeMeshRenderer>(true))
        {
            AddTransformPathToRoot(drakeRenderer.transform, prototypeTransform, presentationNodes);
        }

        int activatedNodes = 0;
        foreach (GameObject node in presentationNodes)
        {
            if (node != null && !node.activeSelf)
            {
                node.SetActive(true);
                activatedNodes++;
            }
        }

        return activatedNodes;
    }

    private static void AddTransformPathToRoot(Transform transform, Transform root, HashSet<GameObject> nodes)
    {
        Transform? current = transform;
        while (current != null)
        {
            nodes.Add(current.gameObject);
            if (current == root)
            {
                break;
            }

            current = current.parent;
        }
    }

    private static string DescribeDrakeRenderer(DrakeMeshRenderer renderer)
    {
        (string meshGuid, string meshSubObject) = renderer.MeshReferenceData;
        string materials = string.Join("|", Enumerable.Range(0, renderer.MaterialsCountWithOverrideCheck).Take(8).Select(index =>
        {
            try
            {
                (string materialGuid, string materialSubObject) = renderer.MaterialReferenceData(index);
                return index.ToString(CultureInfo.InvariantCulture) + ":" + materialGuid + ":" + materialSubObject;
            }
            catch (Exception ex)
            {
                return index.ToString(CultureInfo.InvariantCulture) + ":unavailable:" + ex.GetType().Name;
            }
        }));
        return TransformPath(renderer.transform) +
            ":activeSelf=" + renderer.gameObject.activeSelf +
            ":activeInHierarchy=" + renderer.gameObject.activeInHierarchy +
            ":transform=" + DescribeAuthoringTransformPose(renderer.transform) +
            ":localToWorldOffset=" + FormatAuthoringFloat4x4(renderer.LocalToWorldOffset) +
            ":mesh=" + meshGuid + ":" + meshSubObject +
            ":materials=" + materials;
    }

    private static string DescribeAuthoringTransformPose(Transform transform)
        => "localPosition=" + FormatAuthoringVector3(transform.localPosition) +
            ",localRotation=" + FormatAuthoringQuaternion(transform.localRotation) +
            ",localScale=" + FormatAuthoringVector3(transform.localScale) +
            ",worldPosition=" + FormatAuthoringVector3(transform.position) +
            ",worldRotation=" + FormatAuthoringQuaternion(transform.rotation) +
            ",lossyScale=" + FormatAuthoringVector3(transform.lossyScale) +
            ",localToWorld=" + FormatAuthoringMatrix4x4(transform.localToWorldMatrix);

    private static string FormatAuthoringVector3(Vector3 value)
        => "(" + value.x.ToString("R", CultureInfo.InvariantCulture) + "," + value.y.ToString("R", CultureInfo.InvariantCulture) + "," + value.z.ToString("R", CultureInfo.InvariantCulture) + ")";

    private static string FormatAuthoringQuaternion(Quaternion value)
        => "(" + value.x.ToString("R", CultureInfo.InvariantCulture) + "," + value.y.ToString("R", CultureInfo.InvariantCulture) + "," + value.z.ToString("R", CultureInfo.InvariantCulture) + "," + value.w.ToString("R", CultureInfo.InvariantCulture) + ")";

    private static string FormatAuthoringFloat4(float4 value)
        => "(" + value.x.ToString("R", CultureInfo.InvariantCulture) + "," + value.y.ToString("R", CultureInfo.InvariantCulture) + "," + value.z.ToString("R", CultureInfo.InvariantCulture) + "," + value.w.ToString("R", CultureInfo.InvariantCulture) + ")";

    private static string FormatAuthoringFloat4x4(float4x4 value)
        => "c0=" + FormatAuthoringFloat4(value.c0) + ",c1=" + FormatAuthoringFloat4(value.c1) + ",c2=" + FormatAuthoringFloat4(value.c2) + ",c3=" + FormatAuthoringFloat4(value.c3);

    private static string FormatAuthoringVector4(Vector4 value)
        => "(" + value.x.ToString("R", CultureInfo.InvariantCulture) + "," + value.y.ToString("R", CultureInfo.InvariantCulture) + "," + value.z.ToString("R", CultureInfo.InvariantCulture) + "," + value.w.ToString("R", CultureInfo.InvariantCulture) + ")";

    private static string FormatAuthoringMatrix4x4(Matrix4x4 value)
        => "c0=" + FormatAuthoringVector4(value.GetColumn(0)) + ",c1=" + FormatAuthoringVector4(value.GetColumn(1)) + ",c2=" + FormatAuthoringVector4(value.GetColumn(2)) + ",c3=" + FormatAuthoringVector4(value.GetColumn(3));

    private static string TransformPath(Transform transform)
    {
        var names = new Stack<string>();
        Transform? current = transform;
        while (current != null)
        {
            names.Push(current.name);
            current = current.parent;
        }

        return "/" + string.Join("/", names);
    }

    private static string DescribeLocalPose(string label, Vector3 position, Quaternion rotation, Vector3 scale)
        => label +
            ":localPosition=" + FormatVector3(position) +
            ",localRotation=" + FormatQuaternion(rotation) +
            ",localScale=" + FormatVector3(scale);

    private static string FormatVector3(Vector3 value)
        => "(" +
            value.x.ToString("R", CultureInfo.InvariantCulture) + "," +
            value.y.ToString("R", CultureInfo.InvariantCulture) + "," +
            value.z.ToString("R", CultureInfo.InvariantCulture) +
            ")";

    private static string FormatQuaternion(Quaternion value)
        => "(" +
            value.x.ToString("R", CultureInfo.InvariantCulture) + "," +
            value.y.ToString("R", CultureInfo.InvariantCulture) + "," +
            value.z.ToString("R", CultureInfo.InvariantCulture) + "," +
            value.w.ToString("R", CultureInfo.InvariantCulture) +
            ")";
}

internal sealed class TaintedWeaponAssetHandleBridge
{
    private static readonly PropertyInfo? AssetReferenceOperationHandleProperty = typeof(AssetReference).GetProperty(
        nameof(AssetReference.OperationHandle),
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private static readonly FieldInfo? ArAssetReferenceHandleField = AccessTools.Field(typeof(ARAssetReference), "_handle");

    private static readonly ConstructorInfo? ArUntypedHandleConstructor = typeof(ARAsyncOperationHandle)
        .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
        .FirstOrDefault(ctor =>
        {
            ParameterInfo[] parameters = ctor.GetParameters();
            return parameters.Length >= 1 && parameters[0].ParameterType == typeof(AsyncOperationHandle);
        });

    private readonly ManualLogSource _log;

    internal TaintedWeaponAssetHandleBridge(ManualLogSource log)
    {
        _log = log;
    }

    internal static bool PrototypeHandleReflectionReady => ArAssetReferenceHandleField != null && ArUntypedHandleConstructor != null;

    internal static string PrototypeHandleReflectionReason => ReflectionReason(new[]
    {
        ArAssetReferenceHandleField != null ? string.Empty : "ARAssetReference._handle",
        ArUntypedHandleConstructor != null ? string.Empty : "ARAsyncOperationHandle(AsyncOperationHandle)"
    });

    internal static bool AssetReferenceHandleReflectionReady => AssetReferenceOperationHandleProperty != null;

    internal static string AssetReferenceHandleReflectionReason => ReflectionReason(new[]
    {
        AssetReferenceOperationHandleProperty != null ? string.Empty : "AssetReference.OperationHandle"
    });

    internal bool TryCreateCompletedAssetReference<T>(TaintedWeaponRecord record, string slot, T asset, out AssetReference reference, out string reason)
        where T : Object
    {
        string runtimeKey = record.IdentityReceipt.AssetKeyForSlot(slot);
        reference = new TaintedWeaponCompletedAssetReference<T>(runtimeKey, asset, record.AddRuntimeAssetHandle, _log);
        if (asset == null)
        {
            reason = "asset-null";
            return false;
        }

        if (!TaintedWeaponAddressableAssetRegistry.TryRegister(runtimeKey, asset, record.AddRuntimeAssetHandle, out string registerReason))
        {
            reason = "addressable-registry-collision:" + registerReason;
            return false;
        }

        if (AssetReferenceOperationHandleProperty == null)
        {
            reason = "asset-reference-operation-handle-setter-missing";
            return false;
        }

        try
        {
            if (((TaintedWeaponCompletedAssetReference<T>)reference).TryPrime(out reason))
            {
                return true;
            }

            reason = "completed-reference-prime-failed:" + reason;
            return false;
        }
        catch (Exception ex)
        {
            reason = ex.GetType().Name + ":" + ex.Message;
            return false;
        }
    }

    internal void AssignBackingHandle(ARAssetReference targetReference, AsyncOperationHandle<GameObject> backingHandle)
    {
        if (ArAssetReferenceHandleField == null || ArUntypedHandleConstructor == null)
        {
            return;
        }

        try
        {
            AsyncOperationHandle untypedBackingHandle = backingHandle;
            ParameterInfo[] parameters = ArUntypedHandleConstructor.GetParameters();
            object?[] args = parameters.Length == 1
                ? new object?[] { untypedBackingHandle }
                : new object?[] { untypedBackingHandle, null };
            object untypedArHandle = ArUntypedHandleConstructor.Invoke(args);
            ArAssetReferenceHandleField.SetValue(targetReference, untypedArHandle);
        }
        catch (Exception ex)
        {
            _log.LogWarning($"{Plugin.PluginName} ARAssetReference backing-handle assign failed; address={targetReference.Address}; error={ex.GetType().Name}: {ex.Message}");
        }
    }

    private static string ReflectionReason(IEnumerable<string> missing)
    {
        string[] missingRows = missing.Where(row => !string.IsNullOrWhiteSpace(row)).ToArray();
        return missingRows.Length == 0 ? "ok" : "missing:" + string.Join(",", missingRows);
    }
}

internal sealed class TaintedWeaponCompletedAssetReference<T> : AssetReference
    where T : Object
{
    private static readonly PropertyInfo? OperationHandleProperty = typeof(AssetReference).GetProperty(
        nameof(AssetReference.OperationHandle),
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    private readonly string _runtimeKey;
    private readonly T _asset;
    private readonly Action<AsyncOperationHandle> _trackHandle;
    private readonly ManualLogSource _log;

    internal TaintedWeaponCompletedAssetReference(string runtimeKey, T asset, Action<AsyncOperationHandle> trackHandle, ManualLogSource log)
        : base(runtimeKey)
    {
        _runtimeKey = runtimeKey;
        _asset = asset;
        _trackHandle = trackHandle;
        _log = log;
    }

    public override object RuntimeKey => _runtimeKey;

    public override Object Asset => _asset;

    public override bool RuntimeKeyIsValid()
    {
        return true;
    }

    public override AsyncOperationHandle<TObject> LoadAssetAsync<TObject>()
    {
        if (OperationHandle.IsValid())
        {
            try
            {
                return OperationHandle.Convert<TObject>();
            }
            catch
            {
                // Type mismatch falls through to a fresh completed operation below.
            }
        }

        if (_asset is TObject typedAsset)
        {
            AsyncOperationHandle<TObject> completed = Addressables.ResourceManager.CreateCompletedOperation<TObject>(typedAsset, null);
            TrackAndAssign(completed, "load");
            return completed;
        }

        AsyncOperationHandle<TObject> failed = Addressables.ResourceManager.CreateCompletedOperation<TObject>(
            default!,
            $"Tainted Weapons completed asset type mismatch. key={_runtimeKey}; stored={typeof(T).FullName}; requested={typeof(TObject).FullName}");
        TrackAndAssign(failed, "type-mismatch");
        return failed;
    }

    internal bool TryPrime(out string reason)
    {
        try
        {
            AsyncOperationHandle<T> completed = Addressables.ResourceManager.CreateCompletedOperation<T>(_asset, null);
            if (!TryAssignOperationHandle(completed, out reason))
            {
                if (completed.IsValid())
                {
                    completed.Release();
                }

                return false;
            }

            AsyncOperationHandle untyped = completed;
            _trackHandle(untyped);
            reason = "ok";
            return true;
        }
        catch (Exception ex)
        {
            reason = ex.GetType().Name + ":" + ex.Message;
            return false;
        }
    }

    private void TrackAndAssign<TObject>(AsyncOperationHandle<TObject> completed, string route)
    {
        if (!TryAssignOperationHandle(completed, out string reason))
        {
            _log.LogWarning($"{Plugin.PluginName} completed AssetReference handle assign failed; route={route}; key={_runtimeKey}; reason={reason}");
        }

        AsyncOperationHandle untyped = completed;
        _trackHandle(untyped);
    }

    internal static bool TryAssignOperationHandle(AssetReference reference, AsyncOperationHandle handle, out string reason)
    {
        if (OperationHandleProperty == null)
        {
            reason = "operation-handle-setter-missing";
            return false;
        }

        try
        {
            OperationHandleProperty.SetValue(reference, handle);
            reason = "ok";
            return true;
        }
        catch (Exception ex)
        {
            reason = ex.GetType().Name + ":" + ex.Message;
            return false;
        }
    }

    private bool TryAssignOperationHandle(AsyncOperationHandle handle, out string reason)
    {
        return TryAssignOperationHandle(this, handle, out reason);
    }
}

internal static class TaintedWeaponAddressableAssetRegistry
{
    private static readonly Dictionary<string, Entry> Assets = new(StringComparer.OrdinalIgnoreCase);

    internal static int Count => Assets.Count;

    internal static string[] RegisteredKeys => Assets.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase).ToArray();

    internal static bool TryRegister<T>(string runtimeKey, T asset, Action<AsyncOperationHandle> trackHandle, out string reason)
        where T : Object
    {
        if (string.IsNullOrWhiteSpace(runtimeKey) || asset == null)
        {
            reason = "runtime-key-or-asset-empty";
            return false;
        }

        if (Assets.TryGetValue(runtimeKey, out Entry existing))
        {
            if (existing.Asset == asset && existing.AssetType == typeof(T))
            {
                reason = "already-registered-same-asset";
                return true;
            }

            reason = $"runtime-key-collision; key={runtimeKey}; existingType={existing.AssetType.FullName}; candidateType={typeof(T).FullName}; existingAsset={existing.Asset.name}; candidateAsset={asset.name}";
            return false;
        }

        Assets[runtimeKey] = new Entry(asset, typeof(T), trackHandle);
        reason = "registered";
        return true;
    }

    internal static bool IsRegisteredKey(string runtimeKey)
    {
        return !string.IsNullOrWhiteSpace(runtimeKey) && Assets.ContainsKey(runtimeKey);
    }

    internal static bool IsRetainedRegisteredKey(string runtimeKey)
    {
        return IsRegisteredKey(runtimeKey) && Assets.TryGetValue(runtimeKey, out Entry entry) && entry.Retained;
    }

    internal static bool TryDescribeRegisteredAsset(string runtimeKey, out string details)
    {
        if (string.IsNullOrWhiteSpace(runtimeKey) || !Assets.TryGetValue(runtimeKey, out Entry entry))
        {
            if (!string.IsNullOrWhiteSpace(runtimeKey) &&
                LoadedMaterialsTracker.Instance?.MaterialKeyToLoadedMaterialMap.TryGetValue(runtimeKey, out Material? nativeMaterial) == true &&
                nativeMaterial != null)
            {
                details = DescribeMaterialAsset(
                    nativeMaterial,
                    registered: false,
                    retained: false,
                    reason: "native-loaded-material");
                return true;
            }

            details = "registered=False,reason=not-framework-key,key=" + SanitizeInline(runtimeKey);
            return false;
        }

        Object asset = entry.Asset;
        if (asset == null)
        {
            details = "registered=True,assetValid=False,assetType=" + SanitizeInline(entry.AssetType.FullName ?? entry.AssetType.Name) + ",retained=" + entry.Retained.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        if (asset is Mesh mesh)
        {
            Bounds bounds = mesh.bounds;
            details = "registered=True,assetType=Mesh,assetValid=True,assetName=" + SanitizeInline(mesh.name) +
                ",retained=" + entry.Retained.ToString(CultureInfo.InvariantCulture) +
                ",vertices=" + mesh.vertexCount.ToString(CultureInfo.InvariantCulture) +
                ",subMeshes=" + mesh.subMeshCount.ToString(CultureInfo.InvariantCulture) +
                ",boundsCenter=" + FormatVector3ForAsset(bounds.center) +
                ",boundsExtents=" + FormatVector3ForAsset(bounds.extents);
            return true;
        }

        if (asset is Material material)
        {
            details = DescribeMaterialAsset(
                material,
                registered: true,
                retained: entry.Retained,
                reason: "framework-registered-material");
            return true;
        }

        details = "registered=True,assetType=" + SanitizeInline(entry.AssetType.FullName ?? entry.AssetType.Name) +
            ",assetValid=True,assetName=" + SanitizeInline(asset.name) +
            ",retained=" + entry.Retained.ToString(CultureInfo.InvariantCulture);
        return true;
    }

    private static string DescribeMaterialAsset(Material material, bool registered, bool retained, string reason)
    {
        Shader? shader = material.shader;
        Texture? mainTexture = material.mainTexture;
        return "registered=" + registered.ToString(CultureInfo.InvariantCulture) +
            ",reason=" + SanitizeInline(reason) +
            ",assetType=Material,assetValid=True,assetName=" + SanitizeInline(material.name) +
            ",retained=" + retained.ToString(CultureInfo.InvariantCulture) +
            ",shaderValid=" + (shader != null).ToString(CultureInfo.InvariantCulture) +
            ",shader=" + SanitizeInline(shader == null ? "<none>" : shader.name) +
            ",shaderSupported=" + (shader != null && shader.isSupported).ToString(CultureInfo.InvariantCulture) +
            ",passCount=" + material.passCount.ToString(CultureInfo.InvariantCulture) +
            ",renderQueue=" + material.renderQueue.ToString(CultureInfo.InvariantCulture) +
            ",enableInstancing=" + material.enableInstancing.ToString(CultureInfo.InvariantCulture) +
            ",mainTexture=" + SanitizeInline(mainTexture == null ? "<none>" : mainTexture.name);
    }

    internal static bool RetainRegisteredKey(string runtimeKey)
    {
        if (string.IsNullOrWhiteSpace(runtimeKey) || !Assets.TryGetValue(runtimeKey, out Entry entry))
        {
            return false;
        }

        Assets[runtimeKey] = entry.WithRetained();
        return true;
    }

    internal static void Clear()
    {
        Assets.Clear();
    }

    internal static bool TryCreateCompletedHandle<T>(AssetReference reference, out AsyncOperationHandle<T> handle, out string message)
        where T : Object
    {
        string runtimeKey = SafeRuntimeKey(reference);
        if (!TryCreateCompletedHandleForKey(runtimeKey, out handle, out message))
        {
            return false;
        }

        AsyncOperationHandle untyped = handle;
        if (!TaintedWeaponCompletedAssetReference<T>.TryAssignOperationHandle(reference, untyped, out string assignReason))
        {
            message = "completed-handle-created-assign-failed:" + assignReason;
        }

        return true;
    }

    internal static bool TryCreateCompletedHandle<T>(object key, out AsyncOperationHandle<T> handle, out string message)
        where T : Object
    {
        return TryCreateCompletedHandleForKey(SafeRuntimeKey(key), out handle, out message);
    }

    internal static bool TryCreateDrakeManagedCompletedHandle<T>(string runtimeKey, out AsyncOperationHandle<T> handle, out string message)
        where T : Object
    {
        return TryCreateCompletedHandleForKey(runtimeKey, trackHandle: false, out handle, out message);
    }

    private static string SafeRuntimeKey(AssetReference reference)
    {
        try
        {
            return reference?.RuntimeKey?.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string SafeRuntimeKey(object key)
    {
        try
        {
            return key?.ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static bool TryCreateCompletedHandleForKey<T>(string runtimeKey, out AsyncOperationHandle<T> handle, out string message)
        where T : Object
    {
        return TryCreateCompletedHandleForKey(runtimeKey, trackHandle: true, out handle, out message);
    }

    private static bool TryCreateCompletedHandleForKey<T>(string runtimeKey, bool trackHandle, out AsyncOperationHandle<T> handle, out string message)
        where T : Object
    {
        handle = default;
        if (string.IsNullOrWhiteSpace(runtimeKey) || !Assets.TryGetValue(runtimeKey, out Entry entry))
        {
            message = "not-framework-key";
            return false;
        }

        if (entry.Asset is not T typedAsset)
        {
            message = $"type-mismatch; key={runtimeKey}; stored={entry.AssetType.FullName}; requested={typeof(T).FullName}";
            return false;
        }

        handle = Addressables.ResourceManager.CreateCompletedOperation<T>(typedAsset, null);
        if (trackHandle)
        {
            AsyncOperationHandle untyped = handle;
            entry.TrackHandle(untyped);
        }

        message = $"served-framework-key; key={runtimeKey}; type={typeof(T).Name}; asset={typedAsset.name}";
        return true;
    }

    private static string FormatVector3ForAsset(Vector3 value)
        => "(" + value.x.ToString("R", CultureInfo.InvariantCulture) + "," + value.y.ToString("R", CultureInfo.InvariantCulture) + "," + value.z.ToString("R", CultureInfo.InvariantCulture) + ")";

    private static string SanitizeInline(string value)
    {
        return (value ?? string.Empty)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace('\t', ' ')
            .Replace(';', ',')
            .Replace('|', '/');
    }

    private readonly struct Entry
    {
        internal Entry(Object asset, Type assetType, Action<AsyncOperationHandle> trackHandle)
            : this(asset, assetType, trackHandle, retained: false)
        {
        }

        private Entry(Object asset, Type assetType, Action<AsyncOperationHandle> trackHandle, bool retained)
        {
            Asset = asset;
            AssetType = assetType;
            TrackHandle = trackHandle;
            Retained = retained;
        }

        internal Object Asset { get; }

        internal Type AssetType { get; }

        internal Action<AsyncOperationHandle> TrackHandle { get; }

        internal bool Retained { get; }

        internal Entry WithRetained()
        {
            return new Entry(Asset, AssetType, TrackHandle, retained: true);
        }
    }
}

internal readonly struct TaintedWeaponDrakeLoadingEvent
{
    internal TaintedWeaponDrakeLoadingEvent(
        string phase,
        string assetKind,
        string runtimeKey,
        ushort index,
        ushort counterBefore,
        ushort counterAfter,
        bool startedOrReleased,
        string message)
    {
        Phase = phase;
        AssetKind = assetKind;
        RuntimeKey = runtimeKey;
        Index = index;
        CounterBefore = counterBefore;
        CounterAfter = counterAfter;
        StartedOrReleased = startedOrReleased;
        Message = message;
    }

    internal string Phase { get; }

    internal string AssetKind { get; }

    internal string RuntimeKey { get; }

    internal ushort Index { get; }

    internal ushort CounterBefore { get; }

    internal ushort CounterAfter { get; }

    internal bool StartedOrReleased { get; }

    internal string Message { get; }

    internal bool HasRuntimeKey => !string.IsNullOrWhiteSpace(RuntimeKey);

    public override string ToString()
    {
        return $"phase={Phase}; kind={AssetKind}; key={RuntimeKey}; index={Index.ToString(CultureInfo.InvariantCulture)}; before={CounterBefore.ToString(CultureInfo.InvariantCulture)}; after={CounterAfter.ToString(CultureInfo.InvariantCulture)}; changed={StartedOrReleased.ToString(CultureInfo.InvariantCulture)}; {Message}";
    }
}

internal readonly struct TaintedWeaponDrakeLoadingSnapshotRow
{
    internal TaintedWeaponDrakeLoadingSnapshotRow(
        string label,
        int managerId,
        string assetKind,
        int index,
        string runtimeKey,
        ushort counter,
        bool handleValid,
        bool isLoaded,
        bool frameworkKey)
    {
        Label = label;
        ManagerId = managerId;
        AssetKind = assetKind;
        Index = index;
        RuntimeKey = runtimeKey;
        Counter = counter;
        HandleValid = handleValid;
        IsLoaded = isLoaded;
        FrameworkKey = frameworkKey;
    }

    internal string Label { get; }

    internal int ManagerId { get; }

    internal string AssetKind { get; }

    internal int Index { get; }

    internal string RuntimeKey { get; }

    internal ushort Counter { get; }

    internal bool HandleValid { get; }

    internal bool IsLoaded { get; }

    internal bool FrameworkKey { get; }
}

internal static class TaintedWeaponDrakeLoadingManagerBridge
{
    private const string LoadingDataKeyFieldName = "key";
    private const string LoadingDataHandleFieldName = "loadingHandle";
    private const string LoadingDataCounterFieldName = "counter";

    private static readonly Type? DrakeRendererLoadingManagerType = AccessTools.TypeByName("Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererLoadingManager");
    private static readonly FieldInfo? MeshLoadingDataField = DrakeRendererLoadingManagerType != null
        ? AccessTools.Field(DrakeRendererLoadingManagerType, "_meshLoadingData")
        : null;
    private static readonly FieldInfo? MaterialLoadingDataField = DrakeRendererLoadingManagerType != null
        ? AccessTools.Field(DrakeRendererLoadingManagerType, "_materialLoadingData")
        : null;
    private static readonly FieldInfo? OnStartedLoadingMaterialField = DrakeRendererLoadingManagerType != null
        ? AccessTools.Field(DrakeRendererLoadingManagerType, "OnStartedLoadingMaterial")
        : null;
    private static readonly Type? DrakeRendererComponentsManagerType = AccessTools.TypeByName("Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererComponentsManager");
    private static readonly FieldInfo? MeshesToLoadField = DrakeRendererComponentsManagerType != null
        ? AccessTools.Field(DrakeRendererComponentsManagerType, "_meshesToLoad")
        : null;
    private static readonly FieldInfo? MaterialsToLoadField = DrakeRendererComponentsManagerType != null
        ? AccessTools.Field(DrakeRendererComponentsManagerType, "_materialsToLoad")
        : null;
    private static readonly Type? UnsafeBitmaskType = AccessTools.TypeByName("Awaken.Utility.LowLevel.Collections.UnsafeBitmask");
    private static readonly MethodInfo? UnsafeBitmaskEnsureIndexMethod = UnsafeBitmaskType != null
        ? AccessTools.Method(UnsafeBitmaskType, "EnsureIndex", new[] { typeof(uint) })
        : null;
    private static readonly MethodInfo? UnsafeBitmaskUpMethod = UnsafeBitmaskType != null
        ? AccessTools.Method(UnsafeBitmaskType, "Up", new[] { typeof(uint) })
        : null;
    private static readonly List<WeakReference> ObservedManagers = new();

    internal static bool TryStartLoadingMesh(object manager, ushort meshIndex, out bool result, out string message, out TaintedWeaponDrakeLoadingEvent eventData)
    {
        return TryStartLoading(manager, MeshLoadingDataField, meshIndex, "mesh", materialEvent: false, out result, out message, out eventData, out AsyncOperationHandle<Mesh> _);
    }

    internal static bool TryStartLoadingMaterial(object manager, ushort materialIndex, out bool result, out string message, out TaintedWeaponDrakeLoadingEvent eventData)
    {
        return TryStartLoading(manager, MaterialLoadingDataField, materialIndex, "material", materialEvent: true, out result, out message, out eventData, out AsyncOperationHandle<Material> _);
    }

    internal static bool MeshCapabilityReady => DrakeRendererLoadingManagerType != null &&
        MeshLoadingDataField != null &&
        HasLoadingDataShape(MeshLoadingDataField, requireMaterialEvent: false, out _);

    internal static string MeshCapabilityReason => LoadingManagerCapabilityReason(MeshLoadingDataField, requireMaterialEvent: false);

    internal static bool MaterialCapabilityReady => DrakeRendererLoadingManagerType != null &&
        MaterialLoadingDataField != null &&
        OnStartedLoadingMaterialField != null &&
        HasLoadingDataShape(MaterialLoadingDataField, requireMaterialEvent: true, out _);

    internal static string MaterialCapabilityReason => LoadingManagerCapabilityReason(MaterialLoadingDataField, requireMaterialEvent: true);

    internal static bool CounterInstrumentationReady => HasLoadingDataShape(MeshLoadingDataField, requireMaterialEvent: false, out _) &&
        HasLoadingDataShape(MaterialLoadingDataField, requireMaterialEvent: false, out _);

    internal static string CounterInstrumentationReason
    {
        get
        {
            var missing = new List<string>();
            if (!HasLoadingDataShape(MeshLoadingDataField, requireMaterialEvent: false, out string meshReason))
            {
                missing.Add("mesh:" + meshReason);
            }

            if (!HasLoadingDataShape(MaterialLoadingDataField, requireMaterialEvent: false, out string materialReason))
            {
                missing.Add("material:" + materialReason);
            }

            return missing.Count == 0 ? "ok" : string.Join(",", missing);
        }
    }

    internal static TaintedWeaponDrakeLoadingEvent CaptureUnloadState(object manager, ushort index, string assetKind, string phase)
    {
        FieldInfo? listField = string.Equals(assetKind, "mesh", StringComparison.OrdinalIgnoreCase)
            ? MeshLoadingDataField
            : MaterialLoadingDataField;
        ObserveManager(manager);
        if (TryReadLoadingEntry(manager, listField, index, out string runtimeKey, out ushort counter, out bool _, out bool _, out string message))
        {
            return new TaintedWeaponDrakeLoadingEvent(phase, assetKind, runtimeKey, index, counter, counter, false, message);
        }

        return new TaintedWeaponDrakeLoadingEvent(phase, assetKind, string.Empty, index, 0, 0, false, message);
    }

    internal static string RetainRegisteredKeys(ushort meshIndex, ushort materialIndex)
    {
        var details = new List<string>();
        int retained = 0;
        foreach (object manager in LiveObservedManagers())
        {
            if (TryRetainRegisteredKey(manager, MeshLoadingDataField, meshIndex, "mesh", details))
            {
                retained++;
            }

            if (TryRetainRegisteredKey(manager, MaterialLoadingDataField, materialIndex, "material", details))
            {
                retained++;
            }
        }

        if (details.Count == 0)
        {
            return "none";
        }

        return "retained=" + retained.ToString(CultureInfo.InvariantCulture) + ",details=" + string.Join("|", details);
    }

    internal static string EnsureRegisteredLoadingStarted(DrakeRendererManager manager, in DrakeMeshMaterialComponent materialIdentity)
    {
        try
        {
            if (manager == null || manager.LoadingManager == null)
            {
                return "skipped:drake-loading-manager-missing";
            }

            object loadingManager = manager.LoadingManager;
            ObserveManager(loadingManager);
            bool meshNeedsStart = ShouldStartRegisteredLoading(
                loadingManager,
                MeshLoadingDataField,
                materialIdentity.meshIndex,
                "mesh",
                out string meshBefore);
            bool materialNeedsStart = ShouldStartRegisteredLoading(
                loadingManager,
                MaterialLoadingDataField,
                materialIdentity.materialIndex,
                "material",
                out string materialBefore);
            if (!meshNeedsStart && !materialNeedsStart)
            {
                return "skipped:already-started;meshBefore={" + meshBefore + "};materialBefore={" + materialBefore + "}";
            }

            if (!meshNeedsStart || !materialNeedsStart)
            {
                return "skipped:partial-zero-counter-state;meshBefore={" + meshBefore + "};materialBefore={" + materialBefore + "}";
            }

            manager.StartLoading(in materialIdentity);
            TryReadLoadingEntry(loadingManager, MeshLoadingDataField, materialIdentity.meshIndex, out _, out _, out _, out _, out string meshAfter);
            TryReadLoadingEntry(loadingManager, MaterialLoadingDataField, materialIdentity.materialIndex, out _, out _, out _, out _, out string materialAfter);
            return "started:native-manager;meshBefore={" + meshBefore + "};materialBefore={" + materialBefore + "};meshAfter={" + SanitizeInline(meshAfter) + "};materialAfter={" + SanitizeInline(materialAfter) + "}";
        }
        catch (Exception ex)
        {
            return "failed:" + ex.GetType().Name + ":" + SanitizeInline(ex.Message);
        }
    }

    internal static string EnsureRegisteredComponentManagerLoadingBits(DrakeRendererManager manager, in DrakeMeshMaterialComponent materialIdentity)
    {
        try
        {
            if (manager == null || manager.LoadingManager == null || manager.ComponentsManager == null)
            {
                return "skipped:drake-manager-missing";
            }

            object loadingManager = manager.LoadingManager;
            object componentsManager = manager.ComponentsManager;
            var details = new List<string>();
            bool meshArmed = TryArmRegisteredComponentManagerLoadingBit(
                loadingManager,
                componentsManager,
                MeshLoadingDataField,
                MeshesToLoadField,
                materialIdentity.meshIndex,
                "mesh",
                details);
            bool materialArmed = TryArmRegisteredComponentManagerLoadingBit(
                loadingManager,
                componentsManager,
                MaterialLoadingDataField,
                MaterialsToLoadField,
                materialIdentity.materialIndex,
                "material",
                details);

            return "meshArmed=" + meshArmed.ToString(CultureInfo.InvariantCulture) +
                ",materialArmed=" + materialArmed.ToString(CultureInfo.InvariantCulture) +
                ",details=" + string.Join("|", details);
        }
        catch (Exception ex)
        {
            return "failed:" + ex.GetType().Name + ":" + SanitizeInline(ex.Message);
        }
    }

    private static bool TryArmRegisteredComponentManagerLoadingBit(
        object loadingManager,
        object componentsManager,
        FieldInfo? loadingDataField,
        FieldInfo? componentBitmaskField,
        ushort index,
        string assetKind,
        List<string> details)
    {
        if (!TryReadLoadingEntry(loadingManager, loadingDataField, index, out string runtimeKey, out ushort counter, out bool handleValid, out bool isLoaded, out string loadingDetails))
        {
            details.Add(assetKind + "=skipped:loading-entry-unavailable:" + SanitizeInline(loadingDetails));
            return false;
        }

        if (!TaintedWeaponAddressableAssetRegistry.IsRegisteredKey(runtimeKey))
        {
            details.Add(assetKind + "=skipped:not-registered:index=" + index.ToString(CultureInfo.InvariantCulture));
            return false;
        }

        if (counter == 0)
        {
            details.Add(assetKind + "=skipped:not-started:" + SanitizeInline(loadingDetails));
            return false;
        }

        if (!handleValid)
        {
            details.Add(assetKind + "=skipped:handle-invalid:" + SanitizeInline(loadingDetails));
            return false;
        }

        if (componentBitmaskField == null || UnsafeBitmaskEnsureIndexMethod == null || UnsafeBitmaskUpMethod == null)
        {
            details.Add(assetKind + "=skipped:component-load-bit-unavailable:" + SanitizeInline(loadingDetails));
            return false;
        }

        object? bitmask = componentBitmaskField.GetValue(componentsManager);
        if (bitmask == null)
        {
            details.Add(assetKind + "=skipped:component-load-bit-null:" + SanitizeInline(loadingDetails));
            return false;
        }

        uint loadIndex = index;
        UnsafeBitmaskEnsureIndexMethod.Invoke(bitmask, new object[] { loadIndex });
        UnsafeBitmaskUpMethod.Invoke(bitmask, new object[] { loadIndex });
        componentBitmaskField.SetValue(componentsManager, bitmask);
        details.Add(assetKind + "=armed:index=" + index.ToString(CultureInfo.InvariantCulture) +
            ":counter=" + counter.ToString(CultureInfo.InvariantCulture) +
            ":handleValid=" + handleValid.ToString(CultureInfo.InvariantCulture) +
            ":isLoaded=" + isLoaded.ToString(CultureInfo.InvariantCulture));
        return true;
    }

    internal static string DescribeRegisteredAssetProof(ushort meshIndex, ushort materialIndex)
    {
        return "mesh={" + DescribeRegisteredAssetProofForIndex(meshIndex, "mesh") + "}" +
            ",material={" + DescribeRegisteredAssetProofForIndex(materialIndex, "material") + "}";
    }

    internal static TaintedWeaponDrakeLoadingEvent CaptureUnloadResult(object manager, ushort index, string assetKind, TaintedWeaponDrakeLoadingEvent before)
    {
        FieldInfo? listField = string.Equals(assetKind, "mesh", StringComparison.OrdinalIgnoreCase)
            ? MeshLoadingDataField
            : MaterialLoadingDataField;
        ObserveManager(manager);
        if (TryReadLoadingEntry(manager, listField, index, out string runtimeKey, out ushort counter, out bool _, out bool _, out string message))
        {
            string key = string.IsNullOrWhiteSpace(runtimeKey) ? before.RuntimeKey : runtimeKey;
            return new TaintedWeaponDrakeLoadingEvent("unload", assetKind, key, index, before.CounterBefore, counter, counter < before.CounterBefore, message);
        }

        return new TaintedWeaponDrakeLoadingEvent("unload", assetKind, before.RuntimeKey, index, before.CounterBefore, 0, before.CounterBefore > 0, message);
    }

    internal static IReadOnlyList<TaintedWeaponDrakeLoadingSnapshotRow> CaptureLoadingRows(string label, IReadOnlyCollection<string> frameworkKeys)
    {
        var rows = new List<TaintedWeaponDrakeLoadingSnapshotRow>();
        foreach (object manager in LiveObservedManagers())
        {
            int managerId = RuntimeHelpers.GetHashCode(manager);
            AddLoadingRows(rows, label, managerId, "mesh", MeshLoadingDataField, manager, frameworkKeys);
            AddLoadingRows(rows, label, managerId, "material", MaterialLoadingDataField, manager, frameworkKeys);
        }

        return rows;
    }

    private static bool TryStartLoading<T>(object manager, FieldInfo? listField, ushort index, string assetKind, bool materialEvent, out bool result, out string message, out TaintedWeaponDrakeLoadingEvent eventData, out AsyncOperationHandle<T> handle)
        where T : Object
    {
        result = false;
        handle = default;
        eventData = default;
        ObserveManager(manager);
        if (manager == null || listField == null)
        {
            message = "drake-loading-field-missing";
            return false;
        }

        if (listField.GetValue(manager) is not IList loadingData || index >= loadingData.Count)
        {
            message = "drake-loading-index-unavailable";
            return false;
        }

        object? boxedEntry = loadingData[index];
        if (boxedEntry == null)
        {
            message = "drake-loading-entry-null";
            return false;
        }

        Type entryType = boxedEntry.GetType();
        FieldInfo? keyField = entryType.GetField(LoadingDataKeyFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo? handleField = entryType.GetField(LoadingDataHandleFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo? counterField = entryType.GetField(LoadingDataCounterFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (keyField == null || handleField == null || counterField == null)
        {
            message = "drake-loading-entry-shape-mismatch";
            return false;
        }

        string runtimeKey = keyField.GetValue(boxedEntry)?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(runtimeKey))
        {
            message = "drake-loading-key-empty";
            return false;
        }

        if (!TaintedWeaponAddressableAssetRegistry.TryCreateDrakeManagedCompletedHandle(runtimeKey, out handle, out message))
        {
            return false;
        }

        ushort counter = Convert.ToUInt16(counterField.GetValue(boxedEntry), CultureInfo.InvariantCulture);
        ushort counterAfter = unchecked((ushort)(counter + 1));
        if (counter == 0)
        {
            if (materialEvent)
            {
                (OnStartedLoadingMaterialField?.GetValue(manager) as Action<string>)?.Invoke(runtimeKey);
            }

            handleField.SetValue(boxedEntry, handle);
            result = true;
        }

        counterField.SetValue(boxedEntry, counterAfter);
        loadingData[index] = boxedEntry;
        message = $"{message}; drakeIndex={index.ToString(CultureInfo.InvariantCulture)}; previousCounter={counter.ToString(CultureInfo.InvariantCulture)}; started={result.ToString(CultureInfo.InvariantCulture)}";
        eventData = new TaintedWeaponDrakeLoadingEvent("start", assetKind, runtimeKey, index, counter, counterAfter, result, message);
        return true;
    }

    private static bool TryRetainRegisteredKey(object manager, FieldInfo? listField, ushort index, string assetKind, List<string> details)
    {
        if (!TryReadLoadingEntry(manager, listField, index, out string runtimeKey, out ushort counter, out bool handleValid, out bool isLoaded, out string message))
        {
            details.Add(assetKind + "=unavailable:" + message);
            return false;
        }

        if (!TaintedWeaponAddressableAssetRegistry.IsRegisteredKey(runtimeKey))
        {
            details.Add(assetKind + "=not-registered:index=" + index.ToString(CultureInfo.InvariantCulture));
            return false;
        }

        bool retained = TaintedWeaponAddressableAssetRegistry.RetainRegisteredKey(runtimeKey);
        details.Add(assetKind + "=retained:" + retained.ToString(CultureInfo.InvariantCulture) +
            ":index=" + index.ToString(CultureInfo.InvariantCulture) +
            ":counter=" + counter.ToString(CultureInfo.InvariantCulture) +
            ":handleValid=" + handleValid.ToString(CultureInfo.InvariantCulture) +
            ":isLoaded=" + isLoaded.ToString(CultureInfo.InvariantCulture));
        return retained;
    }

    private static string DescribeRegisteredAssetProofForIndex(ushort index, string assetKind)
    {
        FieldInfo? listField = string.Equals(assetKind, "mesh", StringComparison.OrdinalIgnoreCase)
            ? MeshLoadingDataField
            : MaterialLoadingDataField;
        var attempts = new List<string>();
        foreach (object manager in LiveObservedManagers())
        {
            if (!TryReadLoadingEntry(manager, listField, index, out string runtimeKey, out ushort counter, out bool handleValid, out bool isLoaded, out string message))
            {
                attempts.Add("read=False,index=" + index.ToString(CultureInfo.InvariantCulture) + ",reason=" + SanitizeInline(message));
                continue;
            }

            if (!TaintedWeaponAddressableAssetRegistry.TryDescribeRegisteredAsset(runtimeKey, out string assetProof))
            {
                attempts.Add("read=True,index=" + index.ToString(CultureInfo.InvariantCulture) +
                    ",key=" + SanitizeInline(runtimeKey) +
                    ",counter=" + counter.ToString(CultureInfo.InvariantCulture) +
                    ",handleValid=" + handleValid.ToString(CultureInfo.InvariantCulture) +
                    ",isLoaded=" + isLoaded.ToString(CultureInfo.InvariantCulture) +
                    "," + assetProof);
                continue;
            }

            return "read=True,index=" + index.ToString(CultureInfo.InvariantCulture) +
                ",key=" + SanitizeInline(runtimeKey) +
                ",counter=" + counter.ToString(CultureInfo.InvariantCulture) +
                ",handleValid=" + handleValid.ToString(CultureInfo.InvariantCulture) +
                ",isLoaded=" + isLoaded.ToString(CultureInfo.InvariantCulture) +
                "," + assetProof;
        }

        return attempts.Count == 0
            ? "read=False,index=" + index.ToString(CultureInfo.InvariantCulture) + ",reason=no-observed-drake-manager"
            : string.Join("/", attempts.Take(4));
    }

    private static bool ShouldStartRegisteredLoading(object manager, FieldInfo? listField, ushort index, string assetKind, out string details)
    {
        if (!TryReadLoadingEntry(manager, listField, index, out string runtimeKey, out ushort counter, out bool handleValid, out bool isLoaded, out string message))
        {
            details = assetKind + "=read-failed:" + SanitizeInline(message);
            return false;
        }

        bool registeredKey = TaintedWeaponAddressableAssetRegistry.IsRegisteredKey(runtimeKey);
        details = assetKind +
            "=key=" + SanitizeInline(runtimeKey) +
            ",counter=" + counter.ToString(CultureInfo.InvariantCulture) +
            ",handleValid=" + handleValid.ToString(CultureInfo.InvariantCulture) +
            ",isLoaded=" + isLoaded.ToString(CultureInfo.InvariantCulture) +
            ",registeredKey=" + registeredKey.ToString(CultureInfo.InvariantCulture);
        return registeredKey && counter == 0;
    }

    private static bool TryReadLoadingEntry(object manager, FieldInfo? listField, ushort index, out string runtimeKey, out ushort counter, out bool handleValid, out bool isLoaded, out string message)
    {
        runtimeKey = string.Empty;
        counter = 0;
        handleValid = false;
        isLoaded = false;
        if (manager == null || listField == null)
        {
            message = "drake-loading-field-missing";
            return false;
        }

        if (listField.GetValue(manager) is not IList loadingData || index >= loadingData.Count)
        {
            message = "drake-loading-index-unavailable";
            return false;
        }

        object? boxedEntry = loadingData[index];
        if (boxedEntry == null)
        {
            message = "drake-loading-entry-null";
            return false;
        }

        Type entryType = boxedEntry.GetType();
        FieldInfo? keyField = entryType.GetField(LoadingDataKeyFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo? handleField = entryType.GetField(LoadingDataHandleFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        FieldInfo? counterField = entryType.GetField(LoadingDataCounterFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        PropertyInfo? isLoadedProperty = entryType.GetProperty("IsLoaded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (keyField == null || handleField == null || counterField == null)
        {
            message = "drake-loading-entry-shape-mismatch";
            return false;
        }

        runtimeKey = keyField.GetValue(boxedEntry)?.ToString() ?? string.Empty;
        counter = Convert.ToUInt16(counterField.GetValue(boxedEntry), CultureInfo.InvariantCulture);
        object? handleValue = handleField.GetValue(boxedEntry);
        handleValid = TryIsHandleValid(handleValue);
        isLoaded = TryReadBool(isLoadedProperty, boxedEntry);
        message = $"key={runtimeKey}; counter={counter.ToString(CultureInfo.InvariantCulture)}; handleValid={handleValid.ToString(CultureInfo.InvariantCulture)}; isLoaded={isLoaded.ToString(CultureInfo.InvariantCulture)}";
        return true;
    }

    private static string SanitizeInline(string value)
    {
        return (value ?? string.Empty)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace('\t', ' ')
            .Replace(';', ',')
            .Replace('|', '/');
    }

    private static void AddLoadingRows(List<TaintedWeaponDrakeLoadingSnapshotRow> rows, string label, int managerId, string assetKind, FieldInfo? listField, object manager, IReadOnlyCollection<string> frameworkKeys)
    {
        if (listField?.GetValue(manager) is not IList loadingData)
        {
            return;
        }

        for (int i = 0; i < loadingData.Count; i++)
        {
            if (!TryReadLoadingEntry(manager, listField, (ushort)i, out string runtimeKey, out ushort counter, out bool handleValid, out bool isLoaded, out _))
            {
                continue;
            }

            bool frameworkKey = frameworkKeys.Contains(runtimeKey, StringComparer.OrdinalIgnoreCase);
            if (!frameworkKey && counter == 0)
            {
                continue;
            }

            rows.Add(new TaintedWeaponDrakeLoadingSnapshotRow(label, managerId, assetKind, i, runtimeKey, counter, handleValid, isLoaded, frameworkKey));
        }
    }

    private static bool TryIsHandleValid(object? handleValue)
    {
        if (handleValue == null)
        {
            return false;
        }

        try
        {
            MethodInfo? isValid = handleValue.GetType().GetMethod("IsValid", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            return isValid != null && isValid.ReturnType == typeof(bool) && (bool)isValid.Invoke(handleValue, null);
        }
        catch
        {
            return false;
        }
    }

    private static bool TryReadBool(PropertyInfo? property, object target)
    {
        if (property == null || property.PropertyType != typeof(bool))
        {
            return false;
        }

        try
        {
            return (bool)property.GetValue(target);
        }
        catch
        {
            return false;
        }
    }

    private static void ObserveManager(object? manager)
    {
        if (manager == null)
        {
            return;
        }

        for (int i = ObservedManagers.Count - 1; i >= 0; i--)
        {
            object? target = ObservedManagers[i].Target;
            if (target == null)
            {
                ObservedManagers.RemoveAt(i);
                continue;
            }

            if (ReferenceEquals(target, manager))
            {
                return;
            }
        }

        ObservedManagers.Add(new WeakReference(manager));
    }

    private static IEnumerable<object> LiveObservedManagers()
    {
        for (int i = ObservedManagers.Count - 1; i >= 0; i--)
        {
            object? target = ObservedManagers[i].Target;
            if (target == null)
            {
                ObservedManagers.RemoveAt(i);
                continue;
            }

            yield return target;
        }
    }

    private static string LoadingManagerCapabilityReason(FieldInfo? listField, bool requireMaterialEvent)
    {
        var missing = new List<string>();
        if (DrakeRendererLoadingManagerType == null)
        {
            missing.Add("DrakeRendererLoadingManager");
        }

        if (listField == null)
        {
            missing.Add("loadingDataField");
        }
        else if (!HasLoadingDataShape(listField, requireMaterialEvent, out string shapeReason))
        {
            missing.Add(shapeReason);
        }

        if (requireMaterialEvent && OnStartedLoadingMaterialField == null)
        {
            missing.Add("OnStartedLoadingMaterial");
        }

        return missing.Count == 0 ? "ok" : "missing:" + string.Join(",", missing);
    }

    private static bool HasLoadingDataShape(FieldInfo? listField, bool requireMaterialEvent, out string reason)
    {
        if (listField == null)
        {
            reason = "loadingDataField";
            return false;
        }

        Type listType = listField.FieldType;
        Type? entryType = listType.IsGenericType ? listType.GetGenericArguments().FirstOrDefault() : null;
        if (entryType == null)
        {
            reason = "loadingDataEntryType";
            return false;
        }

        var missing = new List<string>();
        if (entryType.GetField(LoadingDataKeyFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) == null)
        {
            missing.Add(LoadingDataKeyFieldName);
        }

        if (entryType.GetField(LoadingDataHandleFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) == null)
        {
            missing.Add(LoadingDataHandleFieldName);
        }

        if (entryType.GetField(LoadingDataCounterFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) == null)
        {
            missing.Add(LoadingDataCounterFieldName);
        }

        if (requireMaterialEvent && OnStartedLoadingMaterialField == null)
        {
            missing.Add("OnStartedLoadingMaterial");
        }

        reason = missing.Count == 0 ? "ok" : "entryFields:" + string.Join(",", missing);
        return missing.Count == 0;
    }
}

internal sealed class TaintedWeaponLifecycleSnapshot
{
    internal const string Headers = "label\tscene\tframe\ttime\trecordCount\truntimePrototypeRecordCount\truntimePrototypeObjectCount\truntimePrototypeActiveObjectCount\tstorageRootChildCount\tregisteredAssetKeyCount\truntimeAssetHandleCount\tvalidRuntimeAssetHandleCount\tnativeSourceHandleValidCount\tdrakeManagerCount\tframeworkMeshEntryCount\tframeworkMaterialEntryCount\tframeworkMeshCounterTotal\tframeworkMaterialCounterTotal";

    internal TaintedWeaponLifecycleSnapshot(
        string label,
        string sceneName,
        int frame,
        string time,
        int recordCount,
        int runtimePrototypeRecordCount,
        int runtimePrototypeObjectCount,
        int runtimePrototypeActiveObjectCount,
        int storageRootChildCount,
        int registeredAssetKeyCount,
        int runtimeAssetHandleCount,
        int validRuntimeAssetHandleCount,
        int nativeSourceHandleValidCount,
        int drakeManagerCount,
        int frameworkMeshEntryCount,
        int frameworkMaterialEntryCount,
        int frameworkMeshCounterTotal,
        int frameworkMaterialCounterTotal)
    {
        Label = label;
        SceneName = sceneName;
        Frame = frame;
        Time = time;
        RecordCount = recordCount;
        RuntimePrototypeRecordCount = runtimePrototypeRecordCount;
        RuntimePrototypeObjectCount = runtimePrototypeObjectCount;
        RuntimePrototypeActiveObjectCount = runtimePrototypeActiveObjectCount;
        StorageRootChildCount = storageRootChildCount;
        RegisteredAssetKeyCount = registeredAssetKeyCount;
        RuntimeAssetHandleCount = runtimeAssetHandleCount;
        ValidRuntimeAssetHandleCount = validRuntimeAssetHandleCount;
        NativeSourceHandleValidCount = nativeSourceHandleValidCount;
        DrakeManagerCount = drakeManagerCount;
        FrameworkMeshEntryCount = frameworkMeshEntryCount;
        FrameworkMaterialEntryCount = frameworkMaterialEntryCount;
        FrameworkMeshCounterTotal = frameworkMeshCounterTotal;
        FrameworkMaterialCounterTotal = frameworkMaterialCounterTotal;
    }

    internal string Label { get; }

    internal string SceneName { get; }

    internal int Frame { get; }

    internal string Time { get; }

    internal int RecordCount { get; }

    internal int RuntimePrototypeRecordCount { get; }

    internal int RuntimePrototypeObjectCount { get; }

    internal int RuntimePrototypeActiveObjectCount { get; }

    internal int StorageRootChildCount { get; }

    internal int RegisteredAssetKeyCount { get; }

    internal int RuntimeAssetHandleCount { get; }

    internal int ValidRuntimeAssetHandleCount { get; }

    internal int NativeSourceHandleValidCount { get; }

    internal int DrakeManagerCount { get; }

    internal int FrameworkMeshEntryCount { get; }

    internal int FrameworkMaterialEntryCount { get; }

    internal int FrameworkMeshCounterTotal { get; }

    internal int FrameworkMaterialCounterTotal { get; }

    internal string ToTsv()
    {
        return string.Join("\t", new[]
        {
            Tsv(Label),
            Tsv(SceneName),
            Frame.ToString(CultureInfo.InvariantCulture),
            Tsv(Time),
            RecordCount.ToString(CultureInfo.InvariantCulture),
            RuntimePrototypeRecordCount.ToString(CultureInfo.InvariantCulture),
            RuntimePrototypeObjectCount.ToString(CultureInfo.InvariantCulture),
            RuntimePrototypeActiveObjectCount.ToString(CultureInfo.InvariantCulture),
            StorageRootChildCount.ToString(CultureInfo.InvariantCulture),
            RegisteredAssetKeyCount.ToString(CultureInfo.InvariantCulture),
            RuntimeAssetHandleCount.ToString(CultureInfo.InvariantCulture),
            ValidRuntimeAssetHandleCount.ToString(CultureInfo.InvariantCulture),
            NativeSourceHandleValidCount.ToString(CultureInfo.InvariantCulture),
            DrakeManagerCount.ToString(CultureInfo.InvariantCulture),
            FrameworkMeshEntryCount.ToString(CultureInfo.InvariantCulture),
            FrameworkMaterialEntryCount.ToString(CultureInfo.InvariantCulture),
            FrameworkMeshCounterTotal.ToString(CultureInfo.InvariantCulture),
            FrameworkMaterialCounterTotal.ToString(CultureInfo.InvariantCulture)
        });
    }

    private static string Tsv(string value)
    {
        return (value ?? string.Empty)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace('\t', ' ');
    }
}

internal sealed class TaintedWeaponRecord : IDisposable
{
    private readonly List<AsyncOperationHandle> _runtimeAssetHandles = new();
    private ARAssetReference? _nativeEquippedSource;
    private AsyncOperationHandle<GameObject>? _nativeEquippedSourceHandle;
    private string _lastDrakeEvent = string.Empty;
    private string _lastRuntimeViewEvent = string.Empty;
    private bool _equippedDrakeLocalToWorldOffsetAvailable;
    private float4x4 _equippedDrakeLocalToWorldOffset;
    private string _equippedDrakeLocalToWorldOffsetReason = string.Empty;

    internal TaintedWeaponRecord(
        TaintedWeaponDefinition definition,
        TaintedWeaponsIdentityReceipt identityReceipt,
        bool assetResolved,
        bool drakeReady,
        string details,
        GameObject? customAssetPrefab,
        TaintedWeaponCustomVisualSource? customVisualSource)
    {
        Definition = definition;
        IdentityReceipt = identityReceipt;
        AssetResolved = assetResolved;
        DrakeReady = drakeReady;
        Details = details;
        CustomAssetPrefab = customAssetPrefab;
        CustomVisualSource = customVisualSource;
    }

    internal TaintedWeaponDefinition Definition { get; }

    internal TaintedWeaponsIdentityReceipt IdentityReceipt { get; }

    internal string RuntimePrototypeAddress => IdentityReceipt.RuntimePrototypeAddress;

    internal bool AssetResolved { get; }

    internal bool DrakeReady { get; }

    internal string Details { get; }

    internal GameObject? CustomAssetPrefab { get; }

    internal TaintedWeaponCustomVisualSource? CustomVisualSource { get; }

    internal GameObject? RuntimePrototype { get; set; }

    internal int RedirectCount { get; private set; }

    internal int BuiltPrototypeHandleCount { get; private set; }

    internal int CachedPrototypeHandleCount { get; private set; }

    internal int FallbackNativeHandleCount { get; private set; }

    internal int DrakeMeshStartCount { get; private set; }

    internal int DrakeMaterialStartCount { get; private set; }

    internal int DrakeMeshUnloadCount { get; private set; }

    internal int DrakeMaterialUnloadCount { get; private set; }

    internal string LastDrakeEvent => _lastDrakeEvent;

    internal int RuntimeViewObservationCount { get; private set; }

    internal int RuntimeViewRepairCount { get; private set; }

    internal string LastRuntimeViewEvent => _lastRuntimeViewEvent;

    internal int RuntimeAssetHandleCount => _runtimeAssetHandles.Count;

    internal int ValidRuntimeAssetHandleCount => _runtimeAssetHandles.Count(handle => handle.IsValid());

    internal bool NativeSourceHandleValid => _nativeEquippedSourceHandle.HasValue && _nativeEquippedSourceHandle.Value.IsValid();

    internal string RuntimePrototypeName => RuntimePrototype != null ? RuntimePrototype.name : string.Empty;

    internal bool RuntimePrototypeActiveSelf => RuntimePrototype != null && RuntimePrototype.activeSelf;

    internal bool RuntimePrototypeActiveInHierarchy => RuntimePrototype != null && RuntimePrototype.activeInHierarchy;

    internal void RememberNativeSource(ARAssetReference source)
    {
        string oldKey = SafeRuntimeKey(_nativeEquippedSource);
        string newKey = SafeRuntimeKey(source);
        if (string.Equals(oldKey, newKey, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        DestroyRuntimePrototype();
        ReleaseNativeSourceHandle();
        _nativeEquippedSource = source.DeepCopy();
    }

    internal bool TryGetRuntimePrototype(out GameObject? prototype)
    {
        prototype = RuntimePrototype;
        return prototype != null;
    }

    internal bool TryLoadNativeSource(out GameObject? sourcePrefab, out string reason)
    {
        sourcePrefab = null;
        if (_nativeEquippedSourceHandle.HasValue)
        {
            AsyncOperationHandle<GameObject> existingHandle = _nativeEquippedSourceHandle.Value;
            if (existingHandle.IsValid() && existingHandle.Status == AsyncOperationStatus.Succeeded && existingHandle.Result != null)
            {
                sourcePrefab = existingHandle.Result;
                reason = "cached";
                return true;
            }

            ReleaseNativeSourceHandle();
        }

        if (_nativeEquippedSource == null || !_nativeEquippedSource.IsSet)
        {
            reason = "native-source-reference-missing";
            return false;
        }

        string sourceKey = SafeRuntimeKey(_nativeEquippedSource);
        if (string.IsNullOrWhiteSpace(sourceKey))
        {
            reason = "native-source-runtime-key-empty";
            return false;
        }

        try
        {
            AsyncOperationHandle<GameObject> loadHandle = Addressables.LoadAssetAsync<GameObject>(sourceKey);
            GameObject? loaded = loadHandle.WaitForCompletion();
            if (loadHandle.Status != AsyncOperationStatus.Succeeded || loaded == null)
            {
                Addressables.Release(loadHandle);
                reason = "native-source-load-failed:" + loadHandle.Status;
                return false;
            }

            _nativeEquippedSourceHandle = loadHandle;
            sourcePrefab = loaded;
            reason = "loaded:" + sourceKey;
            return true;
        }
        catch (Exception ex)
        {
            reason = "native-source-load-exception:" + ex.GetType().Name + ":" + ex.Message;
            return false;
        }
    }

    internal void AddRuntimeAssetHandle(AsyncOperationHandle handle)
    {
        _runtimeAssetHandles.Add(handle);
    }

    internal void RecordRedirect()
    {
        RedirectCount++;
    }

    internal void RecordPrototypeHandle(string route)
    {
        if (string.Equals(route, "built-framework-prototype", StringComparison.OrdinalIgnoreCase))
        {
            BuiltPrototypeHandleCount++;
        }
        else if (string.Equals(route, "cached-framework-prototype", StringComparison.OrdinalIgnoreCase))
        {
            CachedPrototypeHandleCount++;
        }
        else if (string.Equals(route, "fallback-native-source", StringComparison.OrdinalIgnoreCase))
        {
            FallbackNativeHandleCount++;
        }
    }

    internal void RecordDrakeLoadingEvent(TaintedWeaponDrakeLoadingEvent eventData)
    {
        _lastDrakeEvent = eventData.ToString();
        bool isMesh = string.Equals(eventData.AssetKind, "mesh", StringComparison.OrdinalIgnoreCase);
        bool isMaterial = string.Equals(eventData.AssetKind, "material", StringComparison.OrdinalIgnoreCase);
        if (string.Equals(eventData.Phase, "start", StringComparison.OrdinalIgnoreCase))
        {
            if (isMesh)
            {
                DrakeMeshStartCount++;
            }
            else if (isMaterial)
            {
                DrakeMaterialStartCount++;
            }
        }
        else if (string.Equals(eventData.Phase, "unload", StringComparison.OrdinalIgnoreCase))
        {
            if (isMesh)
            {
                DrakeMeshUnloadCount++;
            }
            else if (isMaterial)
            {
                DrakeMaterialUnloadCount++;
            }
        }
    }

    internal void RecordRuntimeView(string eventData, int activatedNodes)
    {
        RuntimeViewObservationCount++;
        RuntimeViewRepairCount += Math.Max(activatedNodes, 0);
        _lastRuntimeViewEvent = eventData;
    }

    internal void RememberEquippedDrakeLocalToWorldOffset(float4x4 offsetMatrix, string reason)
    {
        _equippedDrakeLocalToWorldOffset = offsetMatrix;
        _equippedDrakeLocalToWorldOffsetReason = reason;
        _equippedDrakeLocalToWorldOffsetAvailable = true;
    }

    internal bool TryGetEquippedDrakeLocalToWorldOffset(out float4x4 offsetMatrix, out string reason)
    {
        offsetMatrix = _equippedDrakeLocalToWorldOffset;
        reason = _equippedDrakeLocalToWorldOffsetReason;
        return _equippedDrakeLocalToWorldOffsetAvailable;
    }

    internal void ClearEquippedDrakeLocalToWorldOffset()
    {
        _equippedDrakeLocalToWorldOffset = default;
        _equippedDrakeLocalToWorldOffsetReason = string.Empty;
        _equippedDrakeLocalToWorldOffsetAvailable = false;
    }

    internal void ClearRuntimeAssetHandles()
    {
        foreach (AsyncOperationHandle handle in _runtimeAssetHandles)
        {
            if (handle.IsValid())
            {
                handle.Release();
            }
        }

        _runtimeAssetHandles.Clear();
    }

    internal string DescribeNativeSource()
    {
        return _nativeEquippedSource == null ? "<none>" : SafeRuntimeKey(_nativeEquippedSource);
    }

    public void Dispose()
    {
        DestroyRuntimePrototype();
        ReleaseNativeSourceHandle();
    }

    private void DestroyRuntimePrototype()
    {
        if (RuntimePrototype != null)
        {
            Object.Destroy(RuntimePrototype);
            RuntimePrototype = null;
        }

        ClearRuntimeAssetHandles();
        ClearEquippedDrakeLocalToWorldOffset();
    }

    private void ReleaseNativeSourceHandle()
    {
        if (!_nativeEquippedSourceHandle.HasValue)
        {
            return;
        }

        AsyncOperationHandle<GameObject> handle = _nativeEquippedSourceHandle.Value;
        if (handle.IsValid())
        {
            Addressables.Release(handle);
        }

        _nativeEquippedSourceHandle = null;
    }

    private static string SafeRuntimeKey(ARAssetReference? reference)
    {
        if (reference == null)
        {
            return string.Empty;
        }

        try
        {
            return reference.RuntimeKey ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}

internal sealed class TaintedWeaponCustomVisualSource
{
    private TaintedWeaponCustomVisualSource(
        Mesh mesh,
        Material[] materials,
        string sourcePath,
        Matrix4x4 sourceRendererLocalToPrefab,
        string sourceRendererTransformProof)
    {
        Mesh = mesh;
        Materials = materials;
        SourcePath = sourcePath;
        SourceRendererLocalToPrefab = sourceRendererLocalToPrefab;
        SourceRendererTransformProof = sourceRendererTransformProof;
    }

    internal Mesh Mesh { get; }

    internal Material[] Materials { get; }

    internal string SourcePath { get; }

    internal Matrix4x4 SourceRendererLocalToPrefab { get; }

    internal string SourceRendererTransformProof { get; }

    internal static TaintedWeaponCustomVisualReport TryCreate(TaintedWeaponDefinition definition, GameObject prefab, out TaintedWeaponCustomVisualSource? source)
    {
        source = null;
        foreach (MeshRenderer renderer in prefab.GetComponentsInChildren<MeshRenderer>(true))
        {
            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            Mesh? mesh = filter != null ? filter.sharedMesh : null;
            Material[] materials = renderer.sharedMaterials ?? Array.Empty<Material>();
            if (mesh == null || materials.Length == 0 || materials.Any(material => material == null))
            {
                continue;
            }

            Matrix4x4 sourceRendererLocalToPrefab = prefab.transform.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
            string sourceRendererTransformProof = DescribeSourceRendererTransform(prefab.transform, renderer.transform, sourceRendererLocalToPrefab);
            source = new TaintedWeaponCustomVisualSource(mesh, materials, TransformPath(renderer.transform), sourceRendererLocalToPrefab, sourceRendererTransformProof);
            return TaintedWeaponCustomVisualReport.Success(
                $"prefab={prefab.name}; package={definition.PackageId}; weapon={definition.WeaponId}; sourcePath={source.SourcePath}; sourceRendererTransform={source.SourceRendererTransformProof}; mesh={mesh.name}; materialCount={materials.Length.ToString(CultureInfo.InvariantCulture)}; materials={string.Join("|", materials.Select(material => material.name).Take(8))}");
        }

        int meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>(true).Length;
        int meshFilters = prefab.GetComponentsInChildren<MeshFilter>(true).Length;
        int skinnedRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length;
        int unityRenderers = prefab.GetComponentsInChildren<Renderer>(true).Length;
        return TaintedWeaponCustomVisualReport.Blocked(
            "missing-meshfilter-meshrenderer-material-source",
            $"prefab={prefab.name}; package={definition.PackageId}; weapon={definition.WeaponId}; meshRenderers={meshRenderers}; meshFilters={meshFilters}; skinnedMeshRenderers={skinnedRenderers}; unityRenderers={unityRenderers}");
    }

    public override string ToString()
    {
        return $"customSource={SourcePath}; sourceRendererTransform={SourceRendererTransformProof}; mesh={Mesh.name}; materialCount={Materials.Length.ToString(CultureInfo.InvariantCulture)}; materials={string.Join("|", Materials.Select(material => material.name).Take(8))}";
    }

    internal string DescribeForRuntimeEcsReceipt()
    {
        Bounds bounds = Mesh.bounds;
        string shaderNames = string.Join("|", Materials.Select(material => material.shader != null ? material.shader.name : "<no-shader>").Take(8));
        return $"customSource={SourcePath}; sourceRendererTransform={SourceRendererTransformProof}; sourceRendererLocalToPrefab={FormatMatrix4x4(SourceRendererLocalToPrefab)}; sourceMesh={Mesh.name}; sourceMeshVertices={Mesh.vertexCount.ToString(CultureInfo.InvariantCulture)}; sourceMeshSubMeshes={Mesh.subMeshCount.ToString(CultureInfo.InvariantCulture)}; sourceMeshBoundsCenter={FormatVector3(bounds.center)}; sourceMeshBoundsExtents={FormatVector3(bounds.extents)}; sourceMaterialCount={Materials.Length.ToString(CultureInfo.InvariantCulture)}; sourceMaterials={string.Join("|", Materials.Select(material => material.name).Take(8))}; sourceShaders={shaderNames}";
    }

    private static string DescribeSourceRendererTransform(Transform prefabRoot, Transform rendererTransform, Matrix4x4 localToPrefab)
        => "path=" + TransformPath(rendererTransform) +
            ",root=" + TransformPath(prefabRoot) +
            ",localPosition=" + FormatVector3(rendererTransform.localPosition) +
            ",localRotation=" + FormatQuaternion(rendererTransform.localRotation) +
            ",localScale=" + FormatVector3(rendererTransform.localScale) +
            ",localToPrefab=" + FormatMatrix4x4(localToPrefab);

    private static string FormatVector3(Vector3 value)
        => "(" + value.x.ToString("R", CultureInfo.InvariantCulture) + "," + value.y.ToString("R", CultureInfo.InvariantCulture) + "," + value.z.ToString("R", CultureInfo.InvariantCulture) + ")";

    private static string FormatQuaternion(Quaternion value)
        => "(" + value.x.ToString("R", CultureInfo.InvariantCulture) + "," + value.y.ToString("R", CultureInfo.InvariantCulture) + "," + value.z.ToString("R", CultureInfo.InvariantCulture) + "," + value.w.ToString("R", CultureInfo.InvariantCulture) + ")";

    private static string FormatVector4(Vector4 value)
        => "(" + value.x.ToString("R", CultureInfo.InvariantCulture) + "," + value.y.ToString("R", CultureInfo.InvariantCulture) + "," + value.z.ToString("R", CultureInfo.InvariantCulture) + "," + value.w.ToString("R", CultureInfo.InvariantCulture) + ")";

    private static string FormatMatrix4x4(Matrix4x4 value)
        => "c0=" + FormatVector4(value.GetColumn(0)) + ",c1=" + FormatVector4(value.GetColumn(1)) + ",c2=" + FormatVector4(value.GetColumn(2)) + ",c3=" + FormatVector4(value.GetColumn(3));

    private static string TransformPath(Transform transform)
    {
        var names = new Stack<string>();
        Transform? current = transform;
        while (current != null)
        {
            names.Push(current.name);
            current = current.parent;
        }

        return "/" + string.Join("/", names);
    }
}

internal readonly struct TaintedWeaponCustomVisualReport
{
    private TaintedWeaponCustomVisualReport(bool ready, string reason, string details)
    {
        Ready = ready;
        Reason = reason;
        Details = details;
    }

    internal bool Ready { get; }

    internal string Reason { get; }

    internal string Details { get; }

    internal static TaintedWeaponCustomVisualReport Success(string details)
    {
        return new TaintedWeaponCustomVisualReport(true, "ok", details);
    }

    internal static TaintedWeaponCustomVisualReport Blocked(string reason, string details)
    {
        return new TaintedWeaponCustomVisualReport(false, reason, details);
    }

    public override string ToString()
    {
        return $"customVisualReady={Ready}; reason={Reason}; details={Details}";
    }
}

internal readonly struct TaintedWeaponFrameworkStage
{
    internal TaintedWeaponFrameworkStage(string id, string rule)
    {
        Id = id;
        Rule = rule;
    }

    internal string Id { get; }

    internal string Rule { get; }
}

internal static class TaintedWeaponEquipReferencePatch
{
    private static readonly FieldInfo? ItemEquipWeaponInstanceField = AccessTools.Field(typeof(ItemEquip), "_weaponInstance");

    internal static TaintedWeaponsCapabilityReceipt Apply(Harmony harmony, ManualLogSource logger)
    {
        int patched = 0;
        var capabilities = new List<TaintedWeaponsCapability>();
        MethodInfo? getHeroItemTarget = AccessTools.Method(typeof(ItemEquip), nameof(ItemEquip.GetHeroItem));
        MethodInfo? getHeroItemPostfix = AccessTools.Method(typeof(TaintedWeaponEquipReferencePatch), nameof(GetHeroItemPostfix));
        bool itemEquipRedirectReady = false;
        string itemEquipRedirectReason = "ok";
        if (getHeroItemTarget == null || getHeroItemPostfix == null)
        {
            logger.LogWarning($"{Plugin.PluginName}: could not patch ItemEquip.GetHeroItem for framework equipped prototype redirect.");
            itemEquipRedirectReason = MissingReason(
                (getHeroItemTarget, "ItemEquip.GetHeroItem"),
                (getHeroItemPostfix, "GetHeroItemPostfix"));
        }
        else
        {
            harmony.Patch(getHeroItemTarget, postfix: new HarmonyMethod(getHeroItemPostfix));
            patched++;
            itemEquipRedirectReady = true;
        }

        capabilities.Add(new TaintedWeaponsCapability("item-equip-redirect", mandatory: true, itemEquipRedirectReady, itemEquipRedirectReason));

        MethodInfo? loadAssetDefinition = AccessTools.Method(typeof(ARAssetReference), nameof(ARAssetReference.LoadAsset));
        MethodInfo? loadAssetTarget = loadAssetDefinition != null && loadAssetDefinition.IsGenericMethodDefinition
            ? loadAssetDefinition.MakeGenericMethod(typeof(GameObject))
            : null;
        MethodInfo? loadAssetPrefix = AccessTools.Method(typeof(TaintedWeaponEquipReferencePatch), nameof(LoadAssetGameObjectPrefix));
        bool prototypeHandlePatchReady = false;
        string prototypeHandlePatchReason = "ok";
        if (loadAssetTarget == null || loadAssetPrefix == null)
        {
            logger.LogWarning($"{Plugin.PluginName}: could not patch ARAssetReference.LoadAsset<GameObject> for framework equipped prototype handles.");
            prototypeHandlePatchReason = MissingReason(
                (loadAssetTarget, "ARAssetReference.LoadAsset<GameObject>"),
                (loadAssetPrefix, "LoadAssetGameObjectPrefix"));
        }
        else
        {
            harmony.Patch(loadAssetTarget, prefix: new HarmonyMethod(loadAssetPrefix));
            patched++;
            prototypeHandlePatchReady = true;
        }

        bool prototypeHandleReady = prototypeHandlePatchReady && TaintedWeaponAssetHandleBridge.PrototypeHandleReflectionReady;
        capabilities.Add(new TaintedWeaponsCapability(
            "prototype-handle",
            mandatory: true,
            prototypeHandleReady,
            JoinReasons(prototypeHandlePatchReason, TaintedWeaponAssetHandleBridge.PrototypeHandleReflectionReason)));

        capabilities.Add(new TaintedWeaponsCapability(
            "asset-reference-handle",
            mandatory: true,
            TaintedWeaponAssetHandleBridge.AssetReferenceHandleReflectionReady,
            TaintedWeaponAssetHandleBridge.AssetReferenceHandleReflectionReason));

        MethodInfo? onWeaponLoadedTarget = AccessTools.Method(typeof(ItemEquip), "OnWeaponLoaded");
        MethodInfo? onWeaponLoadedPostfix = AccessTools.Method(typeof(TaintedWeaponEquipReferencePatch), nameof(OnWeaponLoadedPostfix));
        MethodInfo? characterHandOnMountTarget = AccessTools.Method(typeof(CharacterHandBase), "OnMount");
        MethodInfo? characterHandOnMountPostfix = AccessTools.Method(typeof(TaintedWeaponEquipReferencePatch), nameof(CharacterHandBaseOnMountPostfix));
        bool runtimeHandLifecycleReady = false;
        string runtimeHandLifecycleReason = "ok";
        if (onWeaponLoadedTarget == null ||
            onWeaponLoadedPostfix == null ||
            ItemEquipWeaponInstanceField == null ||
            characterHandOnMountTarget == null ||
            characterHandOnMountPostfix == null)
        {
            logger.LogWarning($"{Plugin.PluginName}: could not patch equipped runtime CharacterHandBase lifecycle for registered framework weapons.");
            runtimeHandLifecycleReason = MissingReason(
                (onWeaponLoadedTarget, "ItemEquip.OnWeaponLoaded"),
                (onWeaponLoadedPostfix, "OnWeaponLoadedPostfix"),
                (ItemEquipWeaponInstanceField, "ItemEquip._weaponInstance"),
                (characterHandOnMountTarget, "CharacterHandBase.OnMount"),
                (characterHandOnMountPostfix, "CharacterHandBaseOnMountPostfix"));
        }
        else
        {
            harmony.Patch(onWeaponLoadedTarget, postfix: new HarmonyMethod(onWeaponLoadedPostfix));
            harmony.Patch(characterHandOnMountTarget, postfix: new HarmonyMethod(characterHandOnMountPostfix));
            patched += 2;
            runtimeHandLifecycleReady = true;
        }

        capabilities.Add(new TaintedWeaponsCapability(
            "runtime-hand-lifecycle",
            mandatory: true,
            runtimeHandLifecycleReady,
            runtimeHandLifecycleReason));

        MethodInfo? attachToCustomHeroClothesTarget = AccessTools.Method(
            typeof(CharacterHandBase),
            "AttachToCustomHeroClothes",
            new[] { typeof(CustomHeroClothes), typeof(ItemEquip) });
        MethodInfo? attachToCustomHeroClothesPostfix = AccessTools.Method(
            typeof(TaintedWeaponEquipReferencePatch),
            nameof(CharacterHandBaseAttachToCustomHeroClothesPostfix));
        bool inventoryPreviewRouteReady = false;
        string inventoryPreviewRouteReason = "ok";
        if (attachToCustomHeroClothesTarget == null || attachToCustomHeroClothesPostfix == null)
        {
            logger.LogWarning($"{Plugin.PluginName}: could not patch inventory/equipment preview CharacterHandBase.AttachToCustomHeroClothes route for registered framework weapons.");
            inventoryPreviewRouteReason = MissingReason(
                (attachToCustomHeroClothesTarget, "CharacterHandBase.AttachToCustomHeroClothes(CustomHeroClothes,ItemEquip)"),
                (attachToCustomHeroClothesPostfix, "CharacterHandBaseAttachToCustomHeroClothesPostfix"));
        }
        else
        {
            harmony.Patch(attachToCustomHeroClothesTarget, postfix: new HarmonyMethod(attachToCustomHeroClothesPostfix));
            patched++;
            inventoryPreviewRouteReady = true;
        }

        capabilities.Add(new TaintedWeaponsCapability(
            "inventory-preview-route",
            mandatory: false,
            inventoryPreviewRouteReady,
            inventoryPreviewRouteReason));

        Type? drakeRendererLoadingManagerType = AccessTools.TypeByName("Awaken.ECS.DrakeRenderer.Authoring.DrakeRendererLoadingManager");
        MethodInfo? drakeStartLoadingMeshTarget = drakeRendererLoadingManagerType != null
            ? AccessTools.Method(drakeRendererLoadingManagerType, "StartLoadingMesh", new[] { typeof(ushort) })
            : null;
        MethodInfo? drakeStartLoadingMeshPrefix = AccessTools.Method(typeof(TaintedWeaponEquipReferencePatch), nameof(DrakeStartLoadingMeshPrefix));
        bool drakeMeshPatchReady = false;
        string drakeMeshPatchReason = "ok";
        if (drakeStartLoadingMeshTarget == null || drakeStartLoadingMeshPrefix == null)
        {
            logger.LogWarning($"{Plugin.PluginName}: could not patch DrakeRendererLoadingManager.StartLoadingMesh for registered framework mesh keys.");
            drakeMeshPatchReason = MissingReason(
                (drakeRendererLoadingManagerType, "DrakeRendererLoadingManager"),
                (drakeStartLoadingMeshTarget, "DrakeRendererLoadingManager.StartLoadingMesh"),
                (drakeStartLoadingMeshPrefix, "DrakeStartLoadingMeshPrefix"));
        }
        else
        {
            harmony.Patch(drakeStartLoadingMeshTarget, prefix: new HarmonyMethod(drakeStartLoadingMeshPrefix));
            patched++;
            drakeMeshPatchReady = true;
        }

        bool drakeMeshReady = drakeMeshPatchReady && TaintedWeaponDrakeLoadingManagerBridge.MeshCapabilityReady;
        capabilities.Add(new TaintedWeaponsCapability(
            "drake-mesh",
            mandatory: true,
            drakeMeshReady,
            JoinReasons(drakeMeshPatchReason, TaintedWeaponDrakeLoadingManagerBridge.MeshCapabilityReason)));

        MethodInfo? drakeStartLoadingMaterialTarget = drakeRendererLoadingManagerType != null
            ? AccessTools.Method(drakeRendererLoadingManagerType, "StartLoadingMaterial", new[] { typeof(ushort) })
            : null;
        MethodInfo? drakeStartLoadingMaterialPrefix = AccessTools.Method(typeof(TaintedWeaponEquipReferencePatch), nameof(DrakeStartLoadingMaterialPrefix));
        bool drakeMaterialPatchReady = false;
        string drakeMaterialPatchReason = "ok";
        if (drakeStartLoadingMaterialTarget == null || drakeStartLoadingMaterialPrefix == null)
        {
            logger.LogWarning($"{Plugin.PluginName}: could not patch DrakeRendererLoadingManager.StartLoadingMaterial for registered framework material keys.");
            drakeMaterialPatchReason = MissingReason(
                (drakeRendererLoadingManagerType, "DrakeRendererLoadingManager"),
                (drakeStartLoadingMaterialTarget, "DrakeRendererLoadingManager.StartLoadingMaterial"),
                (drakeStartLoadingMaterialPrefix, "DrakeStartLoadingMaterialPrefix"));
        }
        else
        {
            harmony.Patch(drakeStartLoadingMaterialTarget, prefix: new HarmonyMethod(drakeStartLoadingMaterialPrefix));
            patched++;
            drakeMaterialPatchReady = true;
        }

        bool drakeMaterialReady = drakeMaterialPatchReady && TaintedWeaponDrakeLoadingManagerBridge.MaterialCapabilityReady;
        capabilities.Add(new TaintedWeaponsCapability(
            "drake-material",
            mandatory: true,
            drakeMaterialReady,
            JoinReasons(drakeMaterialPatchReason, TaintedWeaponDrakeLoadingManagerBridge.MaterialCapabilityReason)));

        bool unloadObservationReady = false;
        string unloadObservationReason = "ok";
        MethodInfo? drakeUnloadMeshTarget = drakeRendererLoadingManagerType != null
            ? AccessTools.Method(drakeRendererLoadingManagerType, "UnloadMesh", new[] { typeof(ushort) })
            : null;
        MethodInfo? drakeUnloadMaterialTarget = drakeRendererLoadingManagerType != null
            ? AccessTools.Method(drakeRendererLoadingManagerType, "UnloadMaterial", new[] { typeof(ushort) })
            : null;
        MethodInfo? drakeUnloadMeshPrefix = AccessTools.Method(typeof(TaintedWeaponEquipReferencePatch), nameof(DrakeUnloadMeshPrefix));
        MethodInfo? drakeUnloadMeshPostfix = AccessTools.Method(typeof(TaintedWeaponEquipReferencePatch), nameof(DrakeUnloadMeshPostfix));
        MethodInfo? drakeUnloadMaterialPrefix = AccessTools.Method(typeof(TaintedWeaponEquipReferencePatch), nameof(DrakeUnloadMaterialPrefix));
        MethodInfo? drakeUnloadMaterialPostfix = AccessTools.Method(typeof(TaintedWeaponEquipReferencePatch), nameof(DrakeUnloadMaterialPostfix));
        if (drakeUnloadMeshTarget == null ||
            drakeUnloadMaterialTarget == null ||
            drakeUnloadMeshPrefix == null ||
            drakeUnloadMeshPostfix == null ||
            drakeUnloadMaterialPrefix == null ||
            drakeUnloadMaterialPostfix == null)
        {
            logger.LogWarning($"{Plugin.PluginName}: could not patch DrakeRendererLoadingManager unload methods for lifecycle counter observation.");
            unloadObservationReason = MissingReason(
                (drakeUnloadMeshTarget, "DrakeRendererLoadingManager.UnloadMesh"),
                (drakeUnloadMaterialTarget, "DrakeRendererLoadingManager.UnloadMaterial"),
                (drakeUnloadMeshPrefix, "DrakeUnloadMeshPrefix"),
                (drakeUnloadMeshPostfix, "DrakeUnloadMeshPostfix"),
                (drakeUnloadMaterialPrefix, "DrakeUnloadMaterialPrefix"),
                (drakeUnloadMaterialPostfix, "DrakeUnloadMaterialPostfix"));
        }
        else
        {
            harmony.Patch(drakeUnloadMeshTarget, prefix: new HarmonyMethod(drakeUnloadMeshPrefix), postfix: new HarmonyMethod(drakeUnloadMeshPostfix));
            harmony.Patch(drakeUnloadMaterialTarget, prefix: new HarmonyMethod(drakeUnloadMaterialPrefix), postfix: new HarmonyMethod(drakeUnloadMaterialPostfix));
            patched += 2;
            unloadObservationReady = true;
        }

        capabilities.Add(new TaintedWeaponsCapability(
            "counter-instrumentation",
            mandatory: true,
            unloadObservationReady && TaintedWeaponDrakeLoadingManagerBridge.CounterInstrumentationReady,
            JoinReasons(unloadObservationReason, TaintedWeaponDrakeLoadingManagerBridge.CounterInstrumentationReason)));

        capabilities.Add(new TaintedWeaponsCapability(
            "multi-renderer",
            mandatory: false,
            available: false,
            reason: "not-implemented-current-single-drake-renderer-path"));

        logger.LogWarning(
            $"{Plugin.PluginName}: generic AssetReference/Addressables mesh-material hooks are disabled. " +
            "Closed generic Harmony patches leaked into unrelated UI Addressables loads on Mono and made TextAsset/Sprite/GameObject keys load as Material.");

        logger.LogInfo($"{Plugin.PluginName}: patched {patched} framework equipped prototype hooks.");
        return new TaintedWeaponsCapabilityReceipt(capabilities);
    }

    private static void GetHeroItemPostfix(ItemEquip __instance, Hero hero, ref ARAssetReference __result)
    {
        try
        {
            Plugin? plugin = Plugin.Instance;
            if (plugin == null)
            {
                return;
            }

            if (plugin.TryRedirectEquippedVisual(__instance, __result, out ARAssetReference? replacement, out _) && replacement != null)
            {
                __result = replacement;
            }
        }
        catch (Exception ex)
        {
            Plugin.Instance?.ModLogger.LogWarning($"{Plugin.PluginName} equipped visual redirect failed; route=ItemEquip.GetHeroItem; error={ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void OnWeaponLoadedPostfix(ItemEquip __instance)
    {
        try
        {
            object? weaponInstance = ItemEquipWeaponInstanceField?.GetValue(__instance);
            CharacterHandBase? handBase = weaponInstance as CharacterHandBase ?? (weaponInstance as Component)?.GetComponent<CharacterHandBase>();
            Plugin.Instance?.ObserveEquippedRuntimeView(__instance, handBase, "ItemEquip.OnWeaponLoaded");
        }
        catch (Exception ex)
        {
            Plugin.Instance?.ModLogger.LogWarning($"{Plugin.PluginName} equipped runtime view observation failed; route=ItemEquip.OnWeaponLoaded; error={ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void CharacterHandBaseOnMountPostfix(CharacterHandBase __instance)
    {
        try
        {
            Plugin.Instance?.ObserveCharacterHandMounted(__instance, "CharacterHandBase.OnMount");
        }
        catch (Exception ex)
        {
            Plugin.Instance?.ModLogger.LogWarning($"{Plugin.PluginName} equipped runtime view observation failed; route=CharacterHandBase.OnMount; error={ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void CharacterHandBaseAttachToCustomHeroClothesPostfix(CharacterHandBase __instance, object[] __args)
    {
        try
        {
            CustomHeroClothes? previewOwner = __args.Length > 0 ? __args[0] as CustomHeroClothes : null;
            ItemEquip? itemEquip = __args.Length > 1 ? __args[1] as ItemEquip : null;
            if (itemEquip == null)
            {
                return;
            }

            Plugin.Instance?.ObserveInventoryPreviewRuntimeView(itemEquip, __instance, previewOwner, "CharacterHandBase.AttachToCustomHeroClothes");
        }
        catch (Exception ex)
        {
            Plugin.Instance?.ModLogger.LogWarning($"{Plugin.PluginName} inventory/equipment preview route observation failed; route=CharacterHandBase.AttachToCustomHeroClothes; error={ex.GetType().Name}: {ex.Message}");
        }
    }

    private static bool LoadAssetGameObjectPrefix(ARAssetReference __instance, ref ARAsyncOperationHandle<GameObject> __result)
    {
        try
        {
            Plugin? plugin = Plugin.Instance;
            if (plugin == null)
            {
                return true;
            }

            if (plugin.TryCreateEquippedPrototypeHandle(__instance, out __result, out _))
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Instance?.ModLogger.LogWarning($"{Plugin.PluginName} equipped prototype handle failed; route=ARAssetReference.LoadAsset<GameObject>; error={ex.GetType().Name}: {ex.Message}");
            return true;
        }
    }

    private static bool DrakeStartLoadingMeshPrefix(object __instance, ushort meshIndex, ref bool __result)
    {
        try
        {
            if (TaintedWeaponDrakeLoadingManagerBridge.TryStartLoadingMesh(__instance, meshIndex, out __result, out string message, out TaintedWeaponDrakeLoadingEvent eventData))
            {
                Plugin.Instance?.RecordDrakeLifecycleEvent(eventData);
                Plugin.Instance?.ModLogger.LogInfo($"{Plugin.PluginName} framework Drake mesh key served; {message}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Plugin.Instance?.ModLogger.LogWarning($"{Plugin.PluginName} framework Drake mesh key load failed; error={ex.GetType().Name}: {ex.Message}");
        }

        return true;
    }

    private static bool DrakeStartLoadingMaterialPrefix(object __instance, ushort materialIndex, ref bool __result)
    {
        try
        {
            if (TaintedWeaponDrakeLoadingManagerBridge.TryStartLoadingMaterial(__instance, materialIndex, out __result, out string message, out TaintedWeaponDrakeLoadingEvent eventData))
            {
                Plugin.Instance?.RecordDrakeLifecycleEvent(eventData);
                Plugin.Instance?.ModLogger.LogInfo($"{Plugin.PluginName} framework Drake material key served; {message}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Plugin.Instance?.ModLogger.LogWarning($"{Plugin.PluginName} framework Drake material key load failed; error={ex.GetType().Name}: {ex.Message}");
        }

        return true;
    }

    private static bool DrakeUnloadMeshPrefix(object __instance, ushort meshIndex, out TaintedWeaponDrakeLoadingEvent __state)
    {
        __state = TaintedWeaponDrakeLoadingManagerBridge.CaptureUnloadState(__instance, meshIndex, "mesh", "before-unload");
        return !ShouldSkipRegisteredRetainedUnload(__state, "mesh");
    }

    private static void DrakeUnloadMeshPostfix(object __instance, ushort meshIndex, TaintedWeaponDrakeLoadingEvent __state)
    {
        TaintedWeaponDrakeLoadingEvent eventData = TaintedWeaponDrakeLoadingManagerBridge.CaptureUnloadResult(__instance, meshIndex, "mesh", __state);
        if (TaintedWeaponAddressableAssetRegistry.IsRegisteredKey(eventData.RuntimeKey))
        {
            Plugin.Instance?.RecordDrakeLifecycleEvent(eventData);
            Plugin.Instance?.ModLogger.LogInfo($"{Plugin.PluginName} framework Drake mesh key unload observed; {eventData}");
        }
    }

    private static bool DrakeUnloadMaterialPrefix(object __instance, ushort materialIndex, out TaintedWeaponDrakeLoadingEvent __state)
    {
        __state = TaintedWeaponDrakeLoadingManagerBridge.CaptureUnloadState(__instance, materialIndex, "material", "before-unload");
        return !ShouldSkipRegisteredRetainedUnload(__state, "material");
    }

    private static void DrakeUnloadMaterialPostfix(object __instance, ushort materialIndex, TaintedWeaponDrakeLoadingEvent __state)
    {
        TaintedWeaponDrakeLoadingEvent eventData = TaintedWeaponDrakeLoadingManagerBridge.CaptureUnloadResult(__instance, materialIndex, "material", __state);
        if (TaintedWeaponAddressableAssetRegistry.IsRegisteredKey(eventData.RuntimeKey))
        {
            Plugin.Instance?.RecordDrakeLifecycleEvent(eventData);
            Plugin.Instance?.ModLogger.LogInfo($"{Plugin.PluginName} framework Drake material key unload observed; {eventData}");
        }
    }

    private static bool ShouldSkipRegisteredRetainedUnload(TaintedWeaponDrakeLoadingEvent state, string assetKind)
    {
        if (!TaintedWeaponAddressableAssetRegistry.IsRetainedRegisteredKey(state.RuntimeKey))
        {
            return false;
        }

        string phase = state.CounterBefore == 0 ? "underflow-blocked" : "retention-blocked";
        string message = state.CounterBefore == 0
            ? "registered-zero-counter-unload-skipped"
            : "registered-retained-unload-skipped";
        var eventData = new TaintedWeaponDrakeLoadingEvent(
            phase,
            assetKind,
            state.RuntimeKey,
            state.Index,
            state.CounterBefore,
            state.CounterAfter,
            startedOrReleased: false,
            message);
        Plugin.Instance?.RecordDrakeLifecycleEvent(eventData);
        Plugin.Instance?.ModLogger.LogWarning($"{Plugin.PluginName} framework Drake {assetKind} retained unload skipped; {eventData}");
        return true;
    }

    private static bool AssetReferenceLoadMeshPrefix(AssetReference __instance, ref AsyncOperationHandle<Mesh> __result)
    {
        try
        {
            if (TaintedWeaponAddressableAssetRegistry.TryCreateCompletedHandle(__instance, out __result, out _))
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            Plugin.Instance?.ModLogger.LogWarning($"{Plugin.PluginName} framework mesh key load failed; error={ex.GetType().Name}: {ex.Message}");
        }

        return true;
    }

    private static bool AssetReferenceLoadMaterialPrefix(AssetReference __instance, ref AsyncOperationHandle<Material> __result)
    {
        try
        {
            if (TaintedWeaponAddressableAssetRegistry.TryCreateCompletedHandle(__instance, out __result, out _))
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            Plugin.Instance?.ModLogger.LogWarning($"{Plugin.PluginName} framework material key load failed; error={ex.GetType().Name}: {ex.Message}");
        }

        return true;
    }

    private static bool AddressablesLoadMeshByKeyPrefix(object key, ref AsyncOperationHandle<Mesh> __result)
    {
        try
        {
            if (TaintedWeaponAddressableAssetRegistry.TryCreateCompletedHandle(key, out __result, out string message))
            {
                Plugin.Instance?.ModLogger.LogInfo($"{Plugin.PluginName} framework Addressables mesh key served; {message}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Plugin.Instance?.ModLogger.LogWarning($"{Plugin.PluginName} framework Addressables mesh key load failed; error={ex.GetType().Name}: {ex.Message}");
        }

        return true;
    }

    private static bool AddressablesLoadMaterialByKeyPrefix(object key, ref AsyncOperationHandle<Material> __result)
    {
        try
        {
            if (TaintedWeaponAddressableAssetRegistry.TryCreateCompletedHandle(key, out __result, out string message))
            {
                Plugin.Instance?.ModLogger.LogInfo($"{Plugin.PluginName} framework Addressables material key served; {message}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Plugin.Instance?.ModLogger.LogWarning($"{Plugin.PluginName} framework Addressables material key load failed; error={ex.GetType().Name}: {ex.Message}");
        }

        return true;
    }

    private static string MissingReason(params (object? value, string name)[] requirements)
    {
        string[] missing = requirements
            .Where(requirement => requirement.value == null)
            .Select(requirement => requirement.name)
            .ToArray();
        return missing.Length == 0 ? "ok" : "missing:" + string.Join(",", missing);
    }

    private static string JoinReasons(params string[] reasons)
    {
        string[] failed = reasons
            .Where(reason => !string.IsNullOrWhiteSpace(reason) && !string.Equals(reason, "ok", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        return failed.Length == 0 ? "ok" : string.Join(";", failed);
    }
}
