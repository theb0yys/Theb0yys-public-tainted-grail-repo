using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Awaken.TG.Assets;
using Awaken.TG.MVC;
using Awaken.TG.Main.Heroes.Items;
using Awaken.TG.Main.Localization;
using Awaken.TG.Main.Locations.Setup;
using Awaken.TG.Main.Templates;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TaintedWeapons;

/// <summary>
/// Explicit request for the provisional weapon-only native template registrar.
/// A presentation definition with the same package/weapon identity must already
/// be accepted by Tainted Weapons. Constructing a request never mutates FoA.
/// </summary>
public sealed class TaintedWeaponNativeItemRegistrationRequest
{
    public const int ContractVersion = 1;
    public const string WeaponItemTemplateCloneProfile = "weapon-item-template-clone/v1";

    public TaintedWeaponNativeItemRegistrationRequest(
        string packageId,
        string weaponId,
        string sourceProfileId,
        string? semanticIconAddress = null,
        string? flavorText = null)
    {
        PackageId = Required(packageId, nameof(packageId));
        WeaponId = Required(weaponId, nameof(weaponId));
        SourceProfileId = Required(sourceProfileId, nameof(sourceProfileId));
        SemanticIconAddress = CleanOptional(semanticIconAddress);
        FlavorText = CleanOptional(flavorText);
    }

    public int Version => ContractVersion;

    public string PackageId { get; }

    public string WeaponId { get; }

    public string SourceProfileId { get; }

    public string SemanticIconAddress { get; }

    public string FlavorText { get; }

    public string RegistryKey => TaintedWeaponsIdentityPolicy.CanonicalRegistryKey(PackageId, WeaponId);

    internal string CanonicalRequestKey => string.Join("\n", new[]
    {
        RegistryKey,
        SourceProfileId.Trim().Normalize(NormalizationForm.FormKC).ToLowerInvariant(),
        SemanticIconAddress.Trim().Normalize(NormalizationForm.FormKC),
        FlavorText.Trim().Normalize(NormalizationForm.FormKC)
    });

    private static string Required(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", name);
        }

        return value.Trim();
    }

    private static string CleanOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}

/// <summary>
/// Machine-readable result for one registrar request. Only Registered=true with
/// Status=template_registered proves that this process inserted the template.
/// </summary>
public sealed class TaintedWeaponNativeItemRegistrationReceipt
{
    internal TaintedWeaponNativeItemRegistrationReceipt(
        string registrationId,
        string registryKey,
        string status,
        bool accepted,
        bool registered,
        string reasonCode,
        string definitionHash,
        string sourceProfileId,
        string sourceProfileHash,
        string customProfileHash,
        string sourceTemplateGuid,
        string customTemplateGuid,
        string customTemplateName,
        int addToMapInvocationCount,
        bool nativeMutationOccurred,
        string details)
    {
        RegistrationId = registrationId;
        RegistryKey = registryKey;
        Status = status;
        Accepted = accepted;
        Registered = registered;
        ReasonCode = reasonCode;
        DefinitionHash = definitionHash;
        SourceProfileId = sourceProfileId;
        SourceProfileHash = sourceProfileHash;
        CustomProfileHash = customProfileHash;
        SourceTemplateGuid = sourceTemplateGuid;
        CustomTemplateGuid = customTemplateGuid;
        CustomTemplateName = customTemplateName;
        AddToMapInvocationCount = addToMapInvocationCount;
        NativeMutationOccurred = nativeMutationOccurred;
        Details = details;
    }

    public int Version => 1;

    public string RegistrationId { get; }

    public string RegistryKey { get; }

    public string Status { get; }

    public bool Accepted { get; }

    public bool Registered { get; }

    public string ReasonCode { get; }

    public string DefinitionHash { get; }

    public string CanonicalDefinitionHash => string.IsNullOrWhiteSpace(DefinitionHash) ? string.Empty : "sha256:" + DefinitionHash;

    public string SourceProfileId { get; }

    public string SourceProfileHash { get; }

    public string CustomProfileHash { get; }

    public string SourceTemplateGuid { get; }

    public string CustomTemplateGuid { get; }

    public string CustomTemplateName { get; }

    public int AddToMapInvocationCount { get; }

    public bool NativeMutationOccurred { get; }

    public string Details { get; }

    public override string ToString()
    {
        return $"version={Version.ToString(CultureInfo.InvariantCulture)}; registrationId={Inline(RegistrationId)}; registryKey={Inline(RegistryKey)}; status={Inline(Status)}; accepted={Accepted}; registered={Registered}; reasonCode={Inline(ReasonCode)}; definitionHash={Inline(DefinitionHash)}; canonicalDefinitionHash={Inline(CanonicalDefinitionHash)}; sourceProfileId={Inline(SourceProfileId)}; sourceProfileHash={Inline(SourceProfileHash)}; customProfileHash={Inline(CustomProfileHash)}; sourceTemplateGuid={Inline(SourceTemplateGuid)}; customTemplateGuid={Inline(CustomTemplateGuid)}; customTemplateName={Inline(CustomTemplateName)}; addToMapInvocationCount={AddToMapInvocationCount.ToString(CultureInfo.InvariantCulture)}; nativeMutationOccurred={NativeMutationOccurred}; details={Inline(Details)}";
    }

    internal static TaintedWeaponNativeItemRegistrationReceipt QueuedBeforeFramework(TaintedWeaponNativeItemRegistrationRequest request)
    {
        return new TaintedWeaponNativeItemRegistrationReceipt(
            "pending:" + request.RegistryKey,
            request.RegistryKey,
            "queued",
            accepted: true,
            registered: false,
            "framework-not-awake",
            string.Empty,
            request.SourceProfileId,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            addToMapInvocationCount: 0,
            nativeMutationOccurred: false,
            "Request queued until the framework and its matching presentation definition are available.");
    }

    internal static TaintedWeaponNativeItemRegistrationReceipt QueuedBeforeImporterLifecycle(TaintedWeaponNativeItemRegistrationRequest request)
    {
        return new TaintedWeaponNativeItemRegistrationReceipt(
            "pending:" + request.RegistryKey,
            request.RegistryKey,
            "queued",
            accepted: true,
            registered: false,
            "framework-importer-lifecycle-not-open",
            string.Empty,
            request.SourceProfileId,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            addToMapInvocationCount: 0,
            nativeMutationOccurred: false,
            "Request queued until the framework has installed importer/equip hooks, published capability state, and opened the import flush point.");
    }

    internal static TaintedWeaponNativeItemRegistrationReceipt DeniedWithoutDefinition(
        TaintedWeaponNativeItemRegistrationRequest request,
        string reasonCode,
        string details)
    {
        return new TaintedWeaponNativeItemRegistrationReceipt(
            "denied:" + request.RegistryKey,
            request.RegistryKey,
            "template_registration_denied",
            accepted: false,
            registered: false,
            reasonCode,
            string.Empty,
            request.SourceProfileId,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            addToMapInvocationCount: 0,
            nativeMutationOccurred: false,
            details);
    }

    private static string Inline(string value)
    {
        string cleaned = (value ?? string.Empty)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace(';', ',')
            .Replace('|', '/');
        return cleaned.Length <= 1024 ? cleaned : cleaned.Substring(0, 1024) + "...";
    }
}

public sealed class TaintedWeaponNativeItemRegistrarStatus
{
    internal TaintedWeaponNativeItemRegistrarStatus(
        bool structurallyAvailable,
        bool providerReady,
        bool readinessHookAvailable,
        bool terminalPoisonedForProcess,
        int pendingCount,
        int registeredCount,
        string reason)
    {
        StructurallyAvailable = structurallyAvailable;
        ProviderReady = providerReady;
        ReadinessHookAvailable = readinessHookAvailable;
        TerminalPoisonedForProcess = terminalPoisonedForProcess;
        PendingCount = pendingCount;
        RegisteredCount = registeredCount;
        Reason = reason;
    }

    public int Version => 1;

    public bool StructurallyAvailable { get; }

    public bool ProviderReady { get; }

    public bool ReadinessHookAvailable { get; }

    public bool TerminalPoisonedForProcess { get; }

    public int PendingCount { get; }

    public int RegisteredCount { get; }

    public string Reason { get; }

    public override string ToString()
    {
        return $"version={Version.ToString(CultureInfo.InvariantCulture)}; structurallyAvailable={StructurallyAvailable}; providerReady={ProviderReady}; readinessHookAvailable={ReadinessHookAvailable}; terminalPoisonedForProcess={TerminalPoisonedForProcess}; pendingCount={PendingCount.ToString(CultureInfo.InvariantCulture)}; registeredCount={RegisteredCount.ToString(CultureInfo.InvariantCulture)}; reason={Reason}";
    }
}

internal sealed class TaintedWeaponNativeItemRegistrar : IDisposable
{
    private const int MaxTemplateReferenceRows = 512;
    private const int MaxTemplateReferenceCollectionItems = 64;

    private static readonly FieldInfo? TemplatesProviderLoaderField = AccessTools.Field(typeof(TemplatesProvider), "_loader");
    private static readonly MethodInfo? TemplatesLoaderAddToMapMethod = AccessTools.Method(
        typeof(TemplatesLoader),
        "AddToMap",
        new[] { typeof(string), typeof(ITemplate) });
    private static readonly FieldInfo? ItemTemplateDescriptionField = AccessTools.Field(typeof(ItemTemplate), "description");
    private static readonly FieldInfo? ItemTemplateFlavorField = AccessTools.Field(typeof(ItemTemplate), "flavor");
    private static readonly FieldInfo? TemplateReferenceGuidField = AccessTools.Field(typeof(TemplateReference), "_guid");

    private readonly ManualLogSource _log;
    private readonly int _unityThreadId;
    private readonly Dictionary<string, PendingRegistration> _pending = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, TaintedWeaponNativeItemRegistrationReceipt> _receipts = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ItemTemplate> _sessionPersistentTemplates = new();
    private bool _readinessHookAvailable;
    private string _readinessHookReason = "not-applied";
    private bool _terminalPoisonedForProcess;
    private string _terminalPoisonReason = "none";
    private bool _disposed;

    internal TaintedWeaponNativeItemRegistrar(ManualLogSource log, int unityThreadId)
    {
        _log = log;
        _unityThreadId = unityThreadId;
    }

    internal bool HasPending => _pending.Count > 0;

    internal void SetReadinessHookStatus(bool available, string reason)
    {
        _readinessHookAvailable = available;
        _readinessHookReason = reason;
    }

    internal TaintedWeaponNativeItemRegistrationReceipt Register(
        TaintedWeaponNativeItemRegistrationRequest request,
        TaintedWeaponRecord record)
    {
        if (_disposed)
        {
            return EmitWithoutReplacing(Denied(record, request, "registrar-disposed", "The framework registrar has already been disposed."));
        }

        if (_terminalPoisonedForProcess)
        {
            return EmitWithoutReplacing(Denied(
                record,
                request,
                "registrar-terminal-poisoned-for-process",
                "Native registry mutation failed after TemplatesLoader.AddToMap visibility; restart FoA before any further native item registration. poisonReason=" + _terminalPoisonReason));
        }

        if (Environment.CurrentManagedThreadId != _unityThreadId)
        {
            return EmitWithoutReplacing(Denied(record, request, "unity-thread-required", $"expectedThread={_unityThreadId.ToString(CultureInfo.InvariantCulture)}, actualThread={Environment.CurrentManagedThreadId.ToString(CultureInfo.InvariantCulture)}"));
        }

        if (!string.Equals(request.RegistryKey, record.Definition.RegistryKey, StringComparison.OrdinalIgnoreCase))
        {
            return EmitWithoutReplacing(Denied(record, request, "presentation-definition-identity-mismatch", $"request={request.RegistryKey}, definition={record.Definition.RegistryKey}"));
        }

        if (!string.Equals(
                request.SourceProfileId,
                TaintedWeaponNativeItemRegistrationRequest.WeaponItemTemplateCloneProfile,
                StringComparison.OrdinalIgnoreCase))
        {
            return EmitWithoutReplacing(Denied(record, request, "unsupported-source-profile", "Only the bounded weapon ItemTemplate clone profile is implemented."));
        }

        string definitionHash = ComputeDefinitionHash(record, request);
        string registrationId = "native-item:" + request.RegistryKey + ":" + definitionHash.Substring(0, 12);
        if (_receipts.TryGetValue(request.RegistryKey, out TaintedWeaponNativeItemRegistrationReceipt existing))
        {
            if (!string.Equals(existing.DefinitionHash, definitionHash, StringComparison.OrdinalIgnoreCase))
            {
                return EmitWithoutReplacing(Collision(
                    record,
                    request,
                    registrationId,
                    definitionHash,
                    "changed-definition-requires-migration-decision",
                    $"existingCanonicalDefinitionHash={existing.CanonicalDefinitionHash}, candidateCanonicalDefinitionHash=sha256:{definitionHash}"));
            }

            if (existing.Registered)
            {
                return existing;
            }
        }

        if (!TryGetReadyProvider(out TemplatesProvider? provider, out string readinessReason) || provider == null)
        {
            var pending = new PendingRegistration(request, record, registrationId, definitionHash);
            _pending[request.RegistryKey] = pending;
            var queued = CreateReceipt(
                pending,
                "queued",
                accepted: true,
                registered: false,
                "templates-provider-not-ready",
                string.Empty,
                string.Empty,
                addToMapInvocationCount: 0,
                nativeMutationOccurred: false,
                readinessReason);
            _receipts[request.RegistryKey] = queued;
            LogReceipt("queued", queued);
            return queued;
        }

        var immediate = new PendingRegistration(request, record, registrationId, definitionHash);
        return Execute(immediate, provider);
    }

    internal void ProcessPending()
    {
        if (_disposed || _pending.Count == 0 || Environment.CurrentManagedThreadId != _unityThreadId)
        {
            return;
        }

        if (_terminalPoisonedForProcess)
        {
            DenyAndClearPendingAfterTerminalPoison();
            return;
        }

        if (!TryGetReadyProvider(out TemplatesProvider? provider, out string readinessReason) || provider == null)
        {
            RefreshQueuedPendingReadinessReceipts(readinessReason);
            return;
        }

        PendingRegistration[] work = _pending.Values.ToArray();
        foreach (PendingRegistration pending in work)
        {
            if (_terminalPoisonedForProcess)
            {
                break;
            }

            if (!_pending.Remove(pending.Request.RegistryKey))
            {
                continue;
            }

            Execute(pending, provider);
        }
    }

    private void RefreshQueuedPendingReadinessReceipts(string readinessReason)
    {
        if (_pending.Count == 0)
        {
            return;
        }

        foreach (PendingRegistration pending in _pending.Values.ToArray())
        {
            pending.ReadinessDrainAttempts++;
            bool reasonChanged = !string.Equals(pending.LastReadinessReason, readinessReason, StringComparison.Ordinal);
            pending.LastReadinessReason = readinessReason;

            var queued = CreateReceipt(
                pending,
                "queued",
                accepted: true,
                registered: false,
                "templates-provider-not-ready",
                string.Empty,
                string.Empty,
                addToMapInvocationCount: 0,
                nativeMutationOccurred: false,
                "drainAttempt=" + pending.ReadinessDrainAttempts.ToString(CultureInfo.InvariantCulture) +
                "; providerReadiness=" + readinessReason);
            _receipts[pending.Request.RegistryKey] = queued;

            if (pending.ReadinessDrainAttempts <= 3 ||
                reasonChanged ||
                pending.ReadinessDrainAttempts % 20 == 0)
            {
                LogReceipt("queued-pending", queued);
            }
        }
    }

    internal bool TryGetReceipt(string registryKey, out TaintedWeaponNativeItemRegistrationReceipt? receipt)
    {
        return _receipts.TryGetValue(registryKey, out receipt);
    }

    internal TaintedWeaponNativeItemRegistrarStatus GetStatus()
    {
        bool structural = TryGetStructuralReadiness(out string structuralReason);
        bool providerReady = TryGetReadyProvider(out _, out string providerReason);
        string reason = _terminalPoisonedForProcess
            ? "registrar-terminal-poisoned-for-process:" + _terminalPoisonReason
            : (structural
                ? (providerReady ? "ready-for-explicit-request" : providerReason)
                : structuralReason);
        if (!_readinessHookAvailable)
        {
            reason += "; readinessHook=" + _readinessHookReason;
        }

        return new TaintedWeaponNativeItemRegistrarStatus(
            structural,
            providerReady,
            _readinessHookAvailable,
            _terminalPoisonedForProcess,
            _pending.Count,
            _receipts.Values.Count(receipt => receipt.Registered),
            reason);
    }

    public void Dispose()
    {
        _disposed = true;
        _pending.Clear();
        _receipts.Clear();

        // TemplatesLoader has no researched unregister path. A successfully inserted
        // clone therefore remains process-session persistent until FoA exits. Destroying
        // it here would leave the native maps pointing at a destroyed Unity object.
        _sessionPersistentTemplates.Clear();
    }

    private TaintedWeaponNativeItemRegistrationReceipt Execute(PendingRegistration pending, TemplatesProvider provider)
    {
        if (!TryGetStructuralReadiness(out string structuralReason))
        {
            return StoreFinal(Denied(pending.Record, pending.Request, "registrar-contract-unavailable", structuralReason, pending));
        }

        ItemTemplate? existingByGuid;
        try
        {
            existingByGuid = provider.Get<ItemTemplate>(pending.Record.IdentityReceipt.CustomTemplateGuid);
        }
        catch (Exception ex)
        {
            return StoreFinal(Denied(pending.Record, pending.Request, "custom-guid-lookup-failed", DescribeException(ex), pending));
        }

        if (existingByGuid != null)
        {
            return StoreFinal(Collision(
                pending.Record,
                pending.Request,
                pending.RegistrationId,
                pending.DefinitionHash,
                "custom-guid-already-owned-externally",
                $"existingType={existingByGuid.GetType().FullName}, existingName={existingByGuid.name}"));
        }

        ItemTemplate[] itemTemplates;
        try
        {
            itemTemplates = provider.AllTemplates.OfType<ItemTemplate>().ToArray();
        }
        catch (Exception ex)
        {
            return StoreFinal(Denied(pending.Record, pending.Request, "template-name-lookup-failed", DescribeException(ex), pending));
        }

        ItemTemplate? nameCollision = itemTemplates.FirstOrDefault(template =>
            template != null &&
            string.Equals(template.name, pending.Record.IdentityReceipt.CustomTemplateName, StringComparison.OrdinalIgnoreCase));
        if (nameCollision != null)
        {
            return StoreFinal(Collision(
                pending.Record,
                pending.Request,
                pending.RegistrationId,
                pending.DefinitionHash,
                "custom-template-name-collision",
                $"templateName={nameCollision.name}, existingGuid={nameCollision.GUID}"));
        }

        ItemTemplate? sourceTemplate;
        try
        {
            sourceTemplate = provider.Get<ItemTemplate>(pending.Record.IdentityReceipt.SourceTemplateGuid);
        }
        catch (Exception ex)
        {
            return StoreFinal(Denied(pending.Record, pending.Request, "source-template-lookup-failed", DescribeException(ex), pending));
        }

        if (sourceTemplate == null)
        {
            return StoreFinal(Denied(pending.Record, pending.Request, "source-template-not-found", "No native mutation occurred.", pending));
        }

        if (sourceTemplate.IsAbstract || sourceTemplate.HiddenOnUI || sourceTemplate.CannotBeDropped)
        {
            return StoreFinal(Denied(
                pending.Record,
                pending.Request,
                "source-template-profile-rejected",
                $"abstract={sourceTemplate.IsAbstract}, hiddenOnUI={sourceTemplate.HiddenOnUI}, cannotBeDropped={sourceTemplate.CannotBeDropped}",
                pending));
        }

        if (!TryGetLoader(provider, out object? loader, out string loaderReason) || loader == null)
        {
            return StoreFinal(Denied(pending.Record, pending.Request, "templates-loader-unavailable", loaderReason, pending));
        }

        if (!string.IsNullOrWhiteSpace(pending.Request.FlavorText) && ItemTemplateFlavorField == null)
        {
            return StoreFinal(Denied(pending.Record, pending.Request, "item-template-flavor-field-missing", "The requested flavor text cannot be applied safely.", pending));
        }

        string sourceGuidBefore = sourceTemplate.GUID ?? string.Empty;
        string sourceNameBefore = sourceTemplate.name ?? string.Empty;
        string[] sourceComponents = CaptureComponentProfile(sourceTemplate.gameObject);
        string[] sourceAttachments = CaptureAttachmentProfile(sourceTemplate.gameObject);
        string[] sourceReferences = CaptureTemplateReferenceProfile(sourceTemplate.gameObject, out bool sourceReferencesComplete);
        if (!sourceReferencesComplete)
        {
            return StoreFinal(Denied(pending.Record, pending.Request, "source-template-reference-profile-over-budget", "TemplateReference profile exceeded the bounded capture limit.", pending));
        }

        string sourceProfileHash = HashRows(sourceComponents, sourceAttachments, sourceReferences);
        GameObject? cloneObject = null;
        int addToMapInvocationCount = 0;
        bool nativeMutationOccurred = false;
        try
        {
            cloneObject = Object.Instantiate(sourceTemplate.gameObject);
            cloneObject.name = pending.Record.IdentityReceipt.CustomTemplateName;
            Object.DontDestroyOnLoad(cloneObject);

            ItemTemplate? customTemplate = cloneObject.GetComponent<ItemTemplate>();
            if (customTemplate == null)
            {
                Object.Destroy(cloneObject);
                return StoreFinal(Denied(pending.Record, pending.Request, "cloned-object-missing-item-template", "No native mutation occurred.", pending, sourceProfileHash));
            }

            ApplyCustomFields(customTemplate, pending.Record.Definition, pending.Request);

            string[] customComponents = CaptureComponentProfile(customTemplate.gameObject);
            string[] customAttachments = CaptureAttachmentProfile(customTemplate.gameObject);
            string[] customReferences = CaptureTemplateReferenceProfile(customTemplate.gameObject, out bool customReferencesComplete);
            string customProfileHash = HashRows(customComponents, customAttachments, customReferences);

            if (!customReferencesComplete ||
                !sourceComponents.SequenceEqual(customComponents, StringComparer.Ordinal) ||
                !sourceAttachments.SequenceEqual(customAttachments, StringComparer.Ordinal) ||
                !sourceReferences.SequenceEqual(customReferences, StringComparer.Ordinal))
            {
                Object.Destroy(cloneObject);
                return StoreFinal(Denied(
                    pending.Record,
                    pending.Request,
                    "cloned-template-profile-mismatch",
                    $"componentsEqual={sourceComponents.SequenceEqual(customComponents, StringComparer.Ordinal)}, attachmentsEqual={sourceAttachments.SequenceEqual(customAttachments, StringComparer.Ordinal)}, nestedReferencesEqual={sourceReferences.SequenceEqual(customReferences, StringComparer.Ordinal)}, nestedReferencesComplete={customReferencesComplete}",
                    pending,
                    sourceProfileHash,
                    customProfileHash));
            }

            if (!string.Equals(sourceTemplate.GUID, sourceGuidBefore, StringComparison.Ordinal) ||
                !string.Equals(sourceTemplate.name, sourceNameBefore, StringComparison.Ordinal) ||
                !string.Equals(customTemplate.GUID, pending.Record.IdentityReceipt.CustomTemplateGuid, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(customTemplate.name, pending.Record.IdentityReceipt.CustomTemplateName, StringComparison.Ordinal) ||
                customTemplate.templateType != sourceTemplate.templateType)
            {
                Object.Destroy(cloneObject);
                return StoreFinal(Denied(
                    pending.Record,
                    pending.Request,
                    "clone-identity-validation-failed",
                    $"sourceUnchanged={string.Equals(sourceTemplate.GUID, sourceGuidBefore, StringComparison.Ordinal) && string.Equals(sourceTemplate.name, sourceNameBefore, StringComparison.Ordinal)}, customGuid={customTemplate.GUID}, customName={customTemplate.name}, templateTypeEqual={customTemplate.templateType == sourceTemplate.templateType}",
                    pending,
                    sourceProfileHash,
                    customProfileHash));
            }

            addToMapInvocationCount = 1;
            TemplatesLoaderAddToMapMethod!.Invoke(loader, new object[]
            {
                pending.Record.IdentityReceipt.CustomTemplateGuid,
                customTemplate
            });
            nativeMutationOccurred = true;

            ItemTemplate? inserted = provider.Get<ItemTemplate>(pending.Record.IdentityReceipt.CustomTemplateGuid);
            if (!ReferenceEquals(inserted, customTemplate))
            {
                var failedLookup = CreateReceipt(
                    pending,
                    "template_registration_failed",
                    accepted: false,
                    registered: false,
                    "provider-lookup-after-insertion-mismatch",
                    sourceProfileHash,
                    customProfileHash,
                    addToMapInvocationCount,
                    nativeMutationOccurred,
                    $"lookupNull={inserted == null}, lookupName={inserted?.name ?? string.Empty}");
                return StoreFinal(failedLookup);
            }

            _sessionPersistentTemplates.Add(customTemplate);
            var registered = CreateReceipt(
                pending,
                "template_registered",
                accepted: true,
                registered: true,
                "registered",
                sourceProfileHash,
                customProfileHash,
                addToMapInvocationCount,
                nativeMutationOccurred,
                $"sourceName={sourceNameBefore}, sourceUnchanged=true, componentProfileRows={sourceComponents.Length.ToString(CultureInfo.InvariantCulture)}, attachmentProfileRows={sourceAttachments.Length.ToString(CultureInfo.InvariantCulture)}, nestedReferenceRows={sourceReferences.Length.ToString(CultureInfo.InvariantCulture)}, providerLookupSameObject=true, teardownPolicy=session-persistent-until-process-exit");
            return StoreFinal(registered);
        }
        catch (Exception ex)
        {
            ItemTemplate? mapped = null;
            try
            {
                mapped = provider.Get<ItemTemplate>(pending.Record.IdentityReceipt.CustomTemplateGuid);
            }
            catch
            {
                // The exception below remains the authoritative failure.
            }

            nativeMutationOccurred = mapped != null;
            if (!nativeMutationOccurred && cloneObject != null)
            {
                Object.Destroy(cloneObject);
            }

            var failed = CreateReceipt(
                pending,
                "template_registration_failed",
                accepted: false,
                registered: false,
                nativeMutationOccurred ? "native-insertion-exception-after-map-mutation" : "native-insertion-exception",
                sourceProfileHash,
                string.Empty,
                addToMapInvocationCount,
                nativeMutationOccurred,
                DescribeException(ex));
            return StoreFinal(failed);
        }
    }

    private static void ApplyCustomFields(
        ItemTemplate customTemplate,
        TaintedWeaponDefinition definition,
        TaintedWeaponNativeItemRegistrationRequest request)
    {
        customTemplate.name = definition.CustomTemplateName;
        customTemplate.GUID = definition.CustomTemplateGuid;
        customTemplate.hiddenOnUI = false;
        customTemplate.cannotBeDropped = false;
        customTemplate.canStack = false;
        customTemplate.itemName = (LocString)definition.DisplayName;
        if (!string.IsNullOrWhiteSpace(request.SemanticIconAddress))
        {
            customTemplate.iconReference = new ShareableSpriteReference(request.SemanticIconAddress);
            customTemplate.conditionalIconReference?.Clear();
        }

        if (!string.IsNullOrWhiteSpace(definition.Description))
        {
            ItemTemplateDescriptionField?.SetValue(
                customTemplate,
                new OptionalLocString((LocString)definition.Description, true));
        }

        if (!string.IsNullOrWhiteSpace(request.FlavorText))
        {
            ItemTemplateFlavorField?.SetValue(
                customTemplate,
                new OptionalLocString((LocString)request.FlavorText, true));
        }
    }

    private static bool TryGetStructuralReadiness(out string reason)
    {
        var missing = new List<string>();
        if (TemplatesProviderLoaderField == null)
        {
            missing.Add("TemplatesProvider._loader");
        }

        if (TemplatesLoaderAddToMapMethod == null)
        {
            missing.Add("TemplatesLoader.AddToMap(string,ITemplate)");
        }

        if (ItemTemplateDescriptionField == null)
        {
            missing.Add("ItemTemplate.description");
        }

        if (TemplateReferenceGuidField == null)
        {
            missing.Add("TemplateReference._guid");
        }

        reason = missing.Count == 0 ? "ok" : "missing:" + string.Join("|", missing);
        return missing.Count == 0;
    }

    private static bool TryGetReadyProvider(out TemplatesProvider? provider, out string reason)
    {
        provider = null;
        try
        {
            provider = World.Services?.TryGet<TemplatesProvider>();
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

            reason = "ready";
            return true;
        }
        catch (Exception ex)
        {
            reason = "templates-provider-readiness-failed:" + DescribeException(ex);
            return false;
        }
    }

    private static bool TryGetLoader(TemplatesProvider provider, out object? loader, out string reason)
    {
        loader = null;
        try
        {
            loader = TemplatesProviderLoaderField?.GetValue(provider);
            reason = loader == null ? "templates-loader-unavailable" : "ready";
            return loader != null;
        }
        catch (Exception ex)
        {
            reason = "templates-loader-resolution-failed:" + DescribeException(ex);
            return false;
        }
    }

    private TaintedWeaponNativeItemRegistrationReceipt StoreFinal(TaintedWeaponNativeItemRegistrationReceipt receipt)
    {
        _receipts[receipt.RegistryKey] = receipt;
        _pending.Remove(receipt.RegistryKey);
        LogReceipt("final", receipt);
        if (receipt.NativeMutationOccurred && !receipt.Registered)
        {
            PoisonForProcess(receipt);
        }

        return receipt;
    }

    private void PoisonForProcess(TaintedWeaponNativeItemRegistrationReceipt receipt)
    {
        if (_terminalPoisonedForProcess)
        {
            return;
        }

        _terminalPoisonedForProcess = true;
        _terminalPoisonReason = receipt.ReasonCode;
        _log.LogError($"{Plugin.PluginName} native item registrar terminal poison; registryKey={receipt.RegistryKey}; reasonCode={receipt.ReasonCode}; nativeMutationOccurred={receipt.NativeMutationOccurred}; restartRequired=true");
        DenyAndClearPendingAfterTerminalPoison();
    }

    private void DenyAndClearPendingAfterTerminalPoison()
    {
        if (_pending.Count == 0)
        {
            return;
        }

        PendingRegistration[] pendingRows = _pending.Values.ToArray();
        _pending.Clear();
        foreach (PendingRegistration pending in pendingRows)
        {
            var denied = Denied(
                pending.Record,
                pending.Request,
                "registrar-terminal-poisoned-for-process",
                "Native registry mutation previously failed after TemplatesLoader.AddToMap visibility; restart FoA before this request can be retried. poisonReason=" + _terminalPoisonReason,
                pending);
            _receipts[denied.RegistryKey] = denied;
            LogReceipt("final", denied);
        }
    }

    private TaintedWeaponNativeItemRegistrationReceipt EmitWithoutReplacing(TaintedWeaponNativeItemRegistrationReceipt receipt)
    {
        LogReceipt("final", receipt);
        return receipt;
    }

    private void LogReceipt(string phase, TaintedWeaponNativeItemRegistrationReceipt receipt)
    {
        if (receipt.Accepted)
        {
            _log.LogInfo($"{Plugin.PluginName} native item registrar receipt; phase={phase}; {receipt}");
        }
        else
        {
            _log.LogWarning($"{Plugin.PluginName} native item registrar receipt; phase={phase}; {receipt}");
        }
    }

    private static TaintedWeaponNativeItemRegistrationReceipt Denied(
        TaintedWeaponRecord record,
        TaintedWeaponNativeItemRegistrationRequest request,
        string reasonCode,
        string details,
        PendingRegistration? pending = null,
        string sourceProfileHash = "",
        string customProfileHash = "")
    {
        string definitionHash = pending?.DefinitionHash ?? ComputeDefinitionHash(record, request);
        string registrationId = pending?.RegistrationId ?? "native-item:" + request.RegistryKey + ":" + definitionHash.Substring(0, 12);
        return new TaintedWeaponNativeItemRegistrationReceipt(
            registrationId,
            request.RegistryKey,
            "template_registration_denied",
            accepted: false,
            registered: false,
            reasonCode,
            definitionHash,
            request.SourceProfileId,
            sourceProfileHash,
            customProfileHash,
            record.IdentityReceipt.SourceTemplateGuid,
            record.IdentityReceipt.CustomTemplateGuid,
            record.IdentityReceipt.CustomTemplateName,
            addToMapInvocationCount: 0,
            nativeMutationOccurred: false,
            details);
    }

    private static TaintedWeaponNativeItemRegistrationReceipt Collision(
        TaintedWeaponRecord record,
        TaintedWeaponNativeItemRegistrationRequest request,
        string registrationId,
        string definitionHash,
        string reasonCode,
        string details)
    {
        return new TaintedWeaponNativeItemRegistrationReceipt(
            registrationId,
            request.RegistryKey,
            "template_registration_collision",
            accepted: false,
            registered: false,
            reasonCode,
            definitionHash,
            request.SourceProfileId,
            string.Empty,
            string.Empty,
            record.IdentityReceipt.SourceTemplateGuid,
            record.IdentityReceipt.CustomTemplateGuid,
            record.IdentityReceipt.CustomTemplateName,
            addToMapInvocationCount: 0,
            nativeMutationOccurred: false,
            details);
    }

    private static TaintedWeaponNativeItemRegistrationReceipt CreateReceipt(
        PendingRegistration pending,
        string status,
        bool accepted,
        bool registered,
        string reasonCode,
        string sourceProfileHash,
        string customProfileHash,
        int addToMapInvocationCount,
        bool nativeMutationOccurred,
        string details)
    {
        return new TaintedWeaponNativeItemRegistrationReceipt(
            pending.RegistrationId,
            pending.Request.RegistryKey,
            status,
            accepted,
            registered,
            reasonCode,
            pending.DefinitionHash,
            pending.Request.SourceProfileId,
            sourceProfileHash,
            customProfileHash,
            pending.Record.IdentityReceipt.SourceTemplateGuid,
            pending.Record.IdentityReceipt.CustomTemplateGuid,
            pending.Record.IdentityReceipt.CustomTemplateName,
            addToMapInvocationCount,
            nativeMutationOccurred,
            details);
    }

    private static string ComputeDefinitionHash(
        TaintedWeaponRecord record,
        TaintedWeaponNativeItemRegistrationRequest request)
    {
        return HashRows(new[]
        {
            "contractVersion=" + TaintedWeaponNativeItemRegistrationRequest.ContractVersion.ToString(CultureInfo.InvariantCulture),
            "presentationDefinitionHash=" + record.IdentityReceipt.DefinitionHash,
            "registryKey=" + request.RegistryKey,
            "sourceProfileId=" + request.SourceProfileId.Trim().Normalize(NormalizationForm.FormKC).ToLowerInvariant(),
            "semanticIconAddress=" + request.SemanticIconAddress.Trim().Normalize(NormalizationForm.FormKC),
            "flavorText=" + request.FlavorText.Trim().Normalize(NormalizationForm.FormKC)
        });
    }

    private static string[] CaptureComponentProfile(GameObject root)
    {
        var rows = new List<string>();
        foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
        {
            string path = IndexedPath(root.transform, transform);
            Component[] components = transform.GetComponents<Component>();
            for (int index = 0; index < components.Length; index++)
            {
                Component? component = components[index];
                string typeName = component == null
                    ? "<missing>"
                    : component.GetType().AssemblyQualifiedName ?? component.GetType().FullName ?? component.GetType().Name;
                rows.Add(path + "#" + index.ToString(CultureInfo.InvariantCulture) + "=" + typeName);
            }
        }

        rows.Sort(StringComparer.Ordinal);
        return rows.ToArray();
    }

    private static string[] CaptureAttachmentProfile(GameObject root)
    {
        return root
            .GetComponentsInChildren<Component>(true)
            .Where(component => component is IAttachmentGroup)
            .Select(component => IndexedPath(root.transform, component.transform) + "=" +
                (component.GetType().AssemblyQualifiedName ?? component.GetType().FullName ?? component.GetType().Name))
            .OrderBy(row => row, StringComparer.Ordinal)
            .ToArray();
    }

    private static string[] CaptureTemplateReferenceProfile(GameObject root, out bool complete)
    {
        var rows = new List<string>();
        complete = true;
        foreach (Component component in root.GetComponentsInChildren<Component>(true))
        {
            if (component == null)
            {
                continue;
            }

            string componentPath = IndexedPath(root.transform, component.transform) + "=" +
                (component.GetType().FullName ?? component.GetType().Name);
            foreach (FieldInfo field in EnumerateInstanceFields(component.GetType()))
            {
                if (!ContainsTemplateReferenceType(field.FieldType))
                {
                    continue;
                }

                object? value;
                try
                {
                    value = field.GetValue(component);
                }
                catch
                {
                    complete = false;
                    return rows.OrderBy(row => row, StringComparer.Ordinal).ToArray();
                }

                AddTemplateReferenceRows(rows, componentPath + "." + field.DeclaringType?.FullName + "." + field.Name, value);
                if (rows.Count > MaxTemplateReferenceRows)
                {
                    complete = false;
                    return rows.Take(MaxTemplateReferenceRows).OrderBy(row => row, StringComparer.Ordinal).ToArray();
                }
            }
        }

        rows.Sort(StringComparer.Ordinal);
        return rows.ToArray();
    }

    private static IEnumerable<FieldInfo> EnumerateInstanceFields(Type type)
    {
        for (Type? current = type; current != null && current != typeof(object); current = current.BaseType)
        {
            foreach (FieldInfo field in current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                yield return field;
            }
        }
    }

    private static bool ContainsTemplateReferenceType(Type type)
    {
        if (typeof(TemplateReference).IsAssignableFrom(type))
        {
            return true;
        }

        if (type.IsArray)
        {
            Type? elementType = type.GetElementType();
            return elementType != null && typeof(TemplateReference).IsAssignableFrom(elementType);
        }

        if (type.IsGenericType)
        {
            return type.GetGenericArguments().Any(argument => typeof(TemplateReference).IsAssignableFrom(argument));
        }

        return false;
    }

    private static void AddTemplateReferenceRows(List<string> rows, string prefix, object? value)
    {
        if (value == null)
        {
            rows.Add(prefix + "=<null>");
            return;
        }

        if (value is TemplateReference templateReference)
        {
            rows.Add(prefix + "=" + ReadTemplateReferenceGuid(templateReference));
            return;
        }

        if (value is IEnumerable sequence && value is not string)
        {
            int index = 0;
            foreach (object? entry in sequence)
            {
                if (index >= MaxTemplateReferenceCollectionItems)
                {
                    rows.Add(prefix + "=<collection-over-budget>");
                    return;
                }

                if (entry is TemplateReference reference)
                {
                    rows.Add(prefix + "[" + index.ToString(CultureInfo.InvariantCulture) + "]=" + ReadTemplateReferenceGuid(reference));
                }

                index++;
            }
        }
    }

    private static string ReadTemplateReferenceGuid(TemplateReference reference)
    {
        try
        {
            return TemplateReferenceGuidField?.GetValue(reference) as string ?? string.Empty;
        }
        catch
        {
            return "<unreadable>";
        }
    }

    private static string IndexedPath(Transform root, Transform target)
    {
        var segments = new Stack<string>();
        Transform? current = target;
        while (current != null && current != root)
        {
            segments.Push(current.GetSiblingIndex().ToString(CultureInfo.InvariantCulture));
            current = current.parent;
        }

        return segments.Count == 0 ? "root" : "root/" + string.Join("/", segments);
    }

    private static string HashRows(params IEnumerable<string>[] groups)
    {
        using SHA256 sha256 = SHA256.Create();
        var builder = new StringBuilder();
        foreach (IEnumerable<string> group in groups)
        {
            foreach (string row in group)
            {
                builder.Append(row.Length.ToString(CultureInfo.InvariantCulture));
                builder.Append(':');
                builder.Append(row);
                builder.Append('\n');
            }

            builder.Append("--\n");
        }

        byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
        return string.Concat(hash.Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
    }

    private static string DescribeException(Exception exception)
    {
        Exception current = exception;
        while (current is TargetInvocationException invocation && invocation.InnerException != null)
        {
            current = invocation.InnerException;
        }

        return current.GetType().Name + ":" + current.Message;
    }

    private sealed class PendingRegistration
    {
        internal PendingRegistration(
            TaintedWeaponNativeItemRegistrationRequest request,
            TaintedWeaponRecord record,
            string registrationId,
            string definitionHash)
        {
            Request = request;
            Record = record;
            RegistrationId = registrationId;
            DefinitionHash = definitionHash;
        }

        internal TaintedWeaponNativeItemRegistrationRequest Request { get; }

        internal TaintedWeaponRecord Record { get; }

        internal string RegistrationId { get; }

        internal string DefinitionHash { get; }

        internal int ReadinessDrainAttempts { get; set; }

        internal string LastReadinessReason { get; set; } = string.Empty;
    }
}

internal static class TaintedWeaponNativeItemRegistrarPatch
{
    internal static (bool Available, string Reason) Apply(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? target = AccessTools.PropertySetter(typeof(TemplatesLoader), nameof(TemplatesLoader.FinishedLoading));
        MethodInfo? postfix = AccessTools.Method(typeof(TaintedWeaponNativeItemRegistrarPatch), nameof(FinishedLoadingPostfix));
        if (target == null || postfix == null)
        {
            string reason = target == null
                ? "TemplatesLoader.FinishedLoading setter missing"
                : "registrar readiness postfix missing";
            logger.LogWarning($"{Plugin.PluginName} native item registrar readiness hook unavailable; reason={reason}");
            return (false, reason);
        }

        try
        {
            harmony.Patch(target, postfix: new HarmonyMethod(postfix));
            logger.LogInfo($"{Plugin.PluginName} native item registrar readiness hook installed; target={target.DeclaringType?.FullName}.{target.Name}; originalPreserved=true");
            return (true, "ready");
        }
        catch (Exception ex)
        {
            string reason = ex.GetType().Name + ":" + ex.Message;
            logger.LogWarning($"{Plugin.PluginName} native item registrar readiness hook unavailable; reason={reason}");
            return (false, reason);
        }
    }

    private static void FinishedLoadingPostfix(bool value)
    {
        if (value)
        {
            Plugin.Instance?.ProcessNativeItemRegistrarQueue();
        }
    }
}
