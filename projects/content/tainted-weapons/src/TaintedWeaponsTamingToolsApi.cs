using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Awaken.TG.Assets;
using Awaken.TG.Main.Heroes.Items;
using Awaken.TG.Main.Localization;
using Awaken.TG.Main.Locations.Setup;
using Awaken.TG.Main.Templates;
using Awaken.TG.MVC;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TaintedWeapons;

/// <summary>
/// Tainted Weapons-owned service for the exact companion-acquisition tool pair.
/// The service owns item identity and native template registration. Consumers
/// receive immutable receipts and never clone, rename, register, or catalogue
/// the underlying FoA items themselves.
/// </summary>
public static class TaintedWeaponsTamingToolsApi
{
    public const int ContractVersion = 1;
    public const string ContractId = "avalon-taming-tools/v1";

    public const string SourceBowTemplateId = "Weapon_Bow_Tier1_Light_Shortbow";
    public const string SourceArrowTemplateId = "Weapon_Ammo_Tier0_WoodenArrow";

    public const string TamingBowTemplateGuid = "b45ee5b5aa01517d885c9a7ec1e1adc3";
    public const string TamingBowTemplateName = "ItemTemplate_Weapon_Bow_Avalon_TamingBow";
    public const string TamingBowDisplayName = "Taming Bow";

    public const string TamingArrowTemplateGuid = "f0b0b02d1f755c8f9d386dedb01d97b6";
    public const string TamingArrowTemplateName = "ItemTemplate_Weapon_Ammo_Avalon_TamingArrow";
    public const string TamingArrowDisplayName = "Taming Arrow";

    /// <summary>
    /// Requests the exact Taming Bow and Taming Arrow set. This method must be
    /// called from the Unity main thread. A queued receipt means the native
    /// TemplatesProvider is not ready yet and the caller should retry later.
    /// </summary>
    public static bool EnsureRegistered(out TaintedWeaponsTamingToolSetReceipt receipt)
    {
        receipt = TaintedWeaponsTamingToolSetRegistrar.EnsureRegistered();
        return receipt.Accepted;
    }

    public static bool TryGetReceipt(out TaintedWeaponsTamingToolSetReceipt? receipt)
    {
        return TaintedWeaponsTamingToolSetRegistrar.TryGetReceipt(out receipt);
    }
}

public sealed class TaintedWeaponsTamingToolSetReceipt
{
    internal TaintedWeaponsTamingToolSetReceipt(
        string status,
        bool accepted,
        bool registered,
        string reasonCode,
        string sourceBowTemplateGuid,
        string sourceBowTemplateName,
        string sourceArrowTemplateGuid,
        string sourceArrowTemplateName,
        int addToMapInvocationCount,
        bool nativeMutationOccurred,
        string bowSourceProfileHash,
        string bowCloneProfileHash,
        string arrowSourceProfileHash,
        string arrowCloneProfileHash,
        string details)
    {
        Status = status;
        Accepted = accepted;
        Registered = registered;
        ReasonCode = reasonCode;
        SourceBowTemplateGuid = sourceBowTemplateGuid;
        SourceBowTemplateName = sourceBowTemplateName;
        SourceArrowTemplateGuid = sourceArrowTemplateGuid;
        SourceArrowTemplateName = sourceArrowTemplateName;
        AddToMapInvocationCount = addToMapInvocationCount;
        NativeMutationOccurred = nativeMutationOccurred;
        BowSourceProfileHash = bowSourceProfileHash;
        BowCloneProfileHash = bowCloneProfileHash;
        ArrowSourceProfileHash = arrowSourceProfileHash;
        ArrowCloneProfileHash = arrowCloneProfileHash;
        Details = details;
    }

    public int Version => TaintedWeaponsTamingToolsApi.ContractVersion;

    public string ContractId => TaintedWeaponsTamingToolsApi.ContractId;

    public string Status { get; }

    public bool Accepted { get; }

    public bool Registered { get; }

    public string ReasonCode { get; }

    public string SourceBowTemplateGuid { get; }

    public string SourceBowTemplateName { get; }

    public string SourceArrowTemplateGuid { get; }

    public string SourceArrowTemplateName { get; }

    public string TamingBowTemplateGuid => TaintedWeaponsTamingToolsApi.TamingBowTemplateGuid;

    public string TamingBowTemplateName => TaintedWeaponsTamingToolsApi.TamingBowTemplateName;

    public string TamingBowDisplayName => TaintedWeaponsTamingToolsApi.TamingBowDisplayName;

    public string TamingArrowTemplateGuid => TaintedWeaponsTamingToolsApi.TamingArrowTemplateGuid;

    public string TamingArrowTemplateName => TaintedWeaponsTamingToolsApi.TamingArrowTemplateName;

    public string TamingArrowDisplayName => TaintedWeaponsTamingToolsApi.TamingArrowDisplayName;

    public int AddToMapInvocationCount { get; }

    public bool NativeMutationOccurred { get; }

    public string BowSourceProfileHash { get; }

    public string BowCloneProfileHash { get; }

    public string ArrowSourceProfileHash { get; }

    public string ArrowCloneProfileHash { get; }

    public string Details { get; }

    public override string ToString()
    {
        return string.Join("; ", new[]
        {
            "version=" + Version.ToString(CultureInfo.InvariantCulture),
            "contractId=" + Inline(ContractId),
            "status=" + Inline(Status),
            "accepted=" + Accepted,
            "registered=" + Registered,
            "reasonCode=" + Inline(ReasonCode),
            "sourceBow=" + Inline(SourceBowTemplateName) + "[" + Inline(SourceBowTemplateGuid) + "]",
            "tamingBow=" + Inline(TamingBowTemplateName) + "[" + Inline(TamingBowTemplateGuid) + "]",
            "sourceArrow=" + Inline(SourceArrowTemplateName) + "[" + Inline(SourceArrowTemplateGuid) + "]",
            "tamingArrow=" + Inline(TamingArrowTemplateName) + "[" + Inline(TamingArrowTemplateGuid) + "]",
            "addToMapInvocationCount=" + AddToMapInvocationCount.ToString(CultureInfo.InvariantCulture),
            "nativeMutationOccurred=" + NativeMutationOccurred,
            "details=" + Inline(Details),
        });
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

internal static class TaintedWeaponsTamingToolSetRegistrar
{
    private const int MaxTemplateReferenceRows = 512;
    private const int MaxTemplateReferenceCollectionItems = 64;

    private static readonly string[] SourceBowAliases =
    {
        TaintedWeaponsTamingToolsApi.SourceBowTemplateId,
        "ItemTemplate_" + TaintedWeaponsTamingToolsApi.SourceBowTemplateId,
        "Item_" + TaintedWeaponsTamingToolsApi.SourceBowTemplateId,
    };

    private static readonly string[] SourceArrowAliases =
    {
        TaintedWeaponsTamingToolsApi.SourceArrowTemplateId,
        "ItemTemplate_" + TaintedWeaponsTamingToolsApi.SourceArrowTemplateId,
        "Item_" + TaintedWeaponsTamingToolsApi.SourceArrowTemplateId,
    };

    private static readonly FieldInfo? TemplatesProviderLoaderField = AccessTools.Field(typeof(TemplatesProvider), "_loader");
    private static readonly MethodInfo? TemplatesLoaderAddToMapMethod = AccessTools.Method(
        typeof(TemplatesLoader),
        "AddToMap",
        new[] { typeof(string), typeof(ITemplate) });
    private static readonly FieldInfo? ItemTemplateDescriptionField = AccessTools.Field(typeof(ItemTemplate), "description");
    private static readonly FieldInfo? TemplateReferenceGuidField = AccessTools.Field(typeof(TemplateReference), "_guid");

    private static readonly object Gate = new();
    private static readonly List<ItemTemplate> SessionPersistentTemplates = new();

    private static TaintedWeaponsTamingToolSetReceipt? _receipt;
    private static bool _terminalPoisonedForProcess;
    private static string _terminalPoisonReason = "none";
    private static string _lastLoggedReceipt = string.Empty;

    internal static TaintedWeaponsTamingToolSetReceipt EnsureRegistered()
    {
        lock (Gate)
        {
            if (_receipt?.Registered == true)
            {
                return _receipt;
            }

            if (_terminalPoisonedForProcess)
            {
                return Store(CreateReceipt(
                    "template_registration_denied",
                    accepted: false,
                    registered: false,
                    "registrar-terminal-poisoned-for-process",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    0,
                    nativeMutationOccurred: true,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    "Restart FoA before retrying the Taming Tool set. poisonReason=" + _terminalPoisonReason));
            }

            Plugin? plugin = Plugin.Instance;
            if (plugin == null)
            {
                return Store(CreateReceipt(
                    "queued",
                    accepted: true,
                    registered: false,
                    "tainted-weapons-plugin-not-awake",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    0,
                    nativeMutationOccurred: false,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    "The caller should retry after Tainted Weapons has completed Awake."));
            }

            if (!TryGetStructuralReadiness(out string structuralReason))
            {
                return Store(CreateReceipt(
                    "template_registration_denied",
                    accepted: false,
                    registered: false,
                    "registrar-contract-unavailable",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    0,
                    nativeMutationOccurred: false,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    structuralReason));
            }

            if (!TryGetReadyProvider(out TemplatesProvider? provider, out string providerReason) || provider == null)
            {
                return Store(CreateReceipt(
                    "queued",
                    accepted: true,
                    registered: false,
                    "templates-provider-not-ready",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    0,
                    nativeMutationOccurred: false,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    providerReason));
            }

            TaintedWeaponsTamingToolSetReceipt result = Execute(provider);
            Log(plugin.ModLogger, result);
            return result;
        }
    }

    internal static bool TryGetReceipt(out TaintedWeaponsTamingToolSetReceipt? receipt)
    {
        lock (Gate)
        {
            receipt = _receipt;
            return receipt != null;
        }
    }

    private static TaintedWeaponsTamingToolSetReceipt Execute(TemplatesProvider provider)
    {
        ItemTemplate[] itemTemplates;
        try
        {
            itemTemplates = provider.AllTemplates.OfType<ItemTemplate>().ToArray();
        }
        catch (Exception ex)
        {
            return Store(Denied("template-enumeration-failed", DescribeException(ex)));
        }

        if (!TryResolveExactSource(itemTemplates, SourceBowAliases, "shortbow", out ItemTemplate? sourceBow, out string bowReason)
            || sourceBow == null)
        {
            return Store(Denied("source-bow-not-found", bowReason));
        }

        if (!TryResolveExactSource(itemTemplates, SourceArrowAliases, "wooden-arrow", out ItemTemplate? sourceArrow, out string arrowReason)
            || sourceArrow == null)
        {
            return Store(Denied(
                "source-arrow-not-found",
                arrowReason,
                sourceBow.GUID,
                sourceBow.name));
        }

        if (!ValidateSourceBow(sourceBow, out string sourceBowReason))
        {
            return Store(Denied(
                "source-bow-profile-rejected",
                sourceBowReason,
                sourceBow.GUID,
                sourceBow.name,
                sourceArrow.GUID,
                sourceArrow.name));
        }

        if (!ValidateSourceArrow(sourceArrow, out string sourceArrowReason))
        {
            return Store(Denied(
                "source-arrow-profile-rejected",
                sourceArrowReason,
                sourceBow.GUID,
                sourceBow.name,
                sourceArrow.GUID,
                sourceArrow.name));
        }

        ItemTemplate? existingBow = SafeGet(provider, TaintedWeaponsTamingToolsApi.TamingBowTemplateGuid);
        ItemTemplate? existingArrow = SafeGet(provider, TaintedWeaponsTamingToolsApi.TamingArrowTemplateGuid);
        if (existingBow != null || existingArrow != null)
        {
            bool exactExistingSet = existingBow != null
                && existingArrow != null
                && string.Equals(existingBow.name, TaintedWeaponsTamingToolsApi.TamingBowTemplateName, StringComparison.Ordinal)
                && string.Equals(existingArrow.name, TaintedWeaponsTamingToolsApi.TamingArrowTemplateName, StringComparison.Ordinal)
                && existingBow.IsShortBow
                && existingArrow.IsArrow
                && HasComponentNamed(existingArrow.gameObject, "ItemProjectile");
            if (exactExistingSet)
            {
                return Store(CreateReceipt(
                    "template_registered",
                    accepted: true,
                    registered: true,
                    "registered-existing",
                    sourceBow.GUID,
                    sourceBow.name,
                    sourceArrow.GUID,
                    sourceArrow.name,
                    0,
                    nativeMutationOccurred: false,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    "Both exact custom GUIDs already resolve to the expected Taming Tool identities."));
            }

            return Store(Collision(
                "custom-guid-collision",
                "One or both custom GUIDs are already mapped to an unexpected template.",
                sourceBow,
                sourceArrow));
        }

        ItemTemplate? bowNameCollision = itemTemplates.FirstOrDefault(template =>
            template != null
            && string.Equals(template.name, TaintedWeaponsTamingToolsApi.TamingBowTemplateName, StringComparison.OrdinalIgnoreCase));
        ItemTemplate? arrowNameCollision = itemTemplates.FirstOrDefault(template =>
            template != null
            && string.Equals(template.name, TaintedWeaponsTamingToolsApi.TamingArrowTemplateName, StringComparison.OrdinalIgnoreCase));
        if (bowNameCollision != null || arrowNameCollision != null)
        {
            return Store(Collision(
                "custom-template-name-collision",
                "One or both custom names are already mapped to another native template.",
                sourceBow,
                sourceArrow));
        }

        if (!TryGetLoader(provider, out object? loader, out string loaderReason) || loader == null)
        {
            return Store(Denied(
                "templates-loader-unavailable",
                loaderReason,
                sourceBow.GUID,
                sourceBow.name,
                sourceArrow.GUID,
                sourceArrow.name));
        }

        PreparedClone? bowClone = null;
        PreparedClone? arrowClone = null;
        int addToMapInvocationCount = 0;
        bool nativeMutationOccurred = false;
        try
        {
            if (!TryPrepareClone(
                    sourceBow,
                    TaintedWeaponsTamingToolsApi.TamingBowTemplateGuid,
                    TaintedWeaponsTamingToolsApi.TamingBowTemplateName,
                    TaintedWeaponsTamingToolsApi.TamingBowDisplayName,
                    "A hunter's bow prepared for creature capture. Designed for use with Taming Arrows.",
                    expectShortBow: true,
                    expectArrow: false,
                    out bowClone,
                    out string bowCloneReason)
                || bowClone == null)
            {
                return Store(Denied(
                    "taming-bow-clone-preflight-failed",
                    bowCloneReason,
                    sourceBow.GUID,
                    sourceBow.name,
                    sourceArrow.GUID,
                    sourceArrow.name));
            }

            if (!TryPrepareClone(
                    sourceArrow,
                    TaintedWeaponsTamingToolsApi.TamingArrowTemplateGuid,
                    TaintedWeaponsTamingToolsApi.TamingArrowTemplateName,
                    TaintedWeaponsTamingToolsApi.TamingArrowDisplayName,
                    "A specially prepared arrow intended for non-lethal creature capture.",
                    expectShortBow: false,
                    expectArrow: true,
                    out arrowClone,
                    out string arrowCloneReason)
                || arrowClone == null)
            {
                bowClone.DestroyIfUnregistered();
                return Store(Denied(
                    "taming-arrow-clone-preflight-failed",
                    arrowCloneReason,
                    sourceBow.GUID,
                    sourceBow.name,
                    sourceArrow.GUID,
                    sourceArrow.name,
                    bowClone.SourceProfileHash,
                    bowClone.CloneProfileHash));
            }

            addToMapInvocationCount++;
            TemplatesLoaderAddToMapMethod!.Invoke(loader, new object[]
            {
                TaintedWeaponsTamingToolsApi.TamingBowTemplateGuid,
                bowClone.Template,
            });
            bowClone.MarkRegistered();
            nativeMutationOccurred = true;

            addToMapInvocationCount++;
            TemplatesLoaderAddToMapMethod.Invoke(loader, new object[]
            {
                TaintedWeaponsTamingToolsApi.TamingArrowTemplateGuid,
                arrowClone.Template,
            });
            arrowClone.MarkRegistered();

            ItemTemplate? insertedBow = SafeGet(provider, TaintedWeaponsTamingToolsApi.TamingBowTemplateGuid);
            ItemTemplate? insertedArrow = SafeGet(provider, TaintedWeaponsTamingToolsApi.TamingArrowTemplateGuid);
            if (!ReferenceEquals(insertedBow, bowClone.Template)
                || !ReferenceEquals(insertedArrow, arrowClone.Template))
            {
                Poison("provider-lookup-after-insertion-mismatch");
                return Store(CreateReceipt(
                    "template_registration_failed",
                    accepted: false,
                    registered: false,
                    "provider-lookup-after-insertion-mismatch",
                    sourceBow.GUID,
                    sourceBow.name,
                    sourceArrow.GUID,
                    sourceArrow.name,
                    addToMapInvocationCount,
                    nativeMutationOccurred: true,
                    bowClone.SourceProfileHash,
                    bowClone.CloneProfileHash,
                    arrowClone.SourceProfileHash,
                    arrowClone.CloneProfileHash,
                    "The native maps became visible but one or both provider lookups did not return the exact cloned object. Restart required."));
            }

            SessionPersistentTemplates.Add(bowClone.Template);
            SessionPersistentTemplates.Add(arrowClone.Template);

            return Store(CreateReceipt(
                "template_registered",
                accepted: true,
                registered: true,
                "registered",
                sourceBow.GUID,
                sourceBow.name,
                sourceArrow.GUID,
                sourceArrow.name,
                addToMapInvocationCount,
                nativeMutationOccurred,
                bowClone.SourceProfileHash,
                bowClone.CloneProfileHash,
                arrowClone.SourceProfileHash,
                arrowClone.CloneProfileHash,
                "Both clones preserved their native component, attachment, nested TemplateReference, stackability, icon, visual and projectile paths; source templates remained unchanged; teardownPolicy=session-persistent-until-process-exit."));
        }
        catch (Exception ex)
        {
            bool bowMapped = SafeGet(provider, TaintedWeaponsTamingToolsApi.TamingBowTemplateGuid) != null;
            bool arrowMapped = SafeGet(provider, TaintedWeaponsTamingToolsApi.TamingArrowTemplateGuid) != null;
            nativeMutationOccurred = nativeMutationOccurred || bowMapped || arrowMapped;

            if (!bowMapped)
            {
                bowClone?.DestroyIfUnregistered();
            }
            if (!arrowMapped)
            {
                arrowClone?.DestroyIfUnregistered();
            }

            if (nativeMutationOccurred)
            {
                Poison("native-insertion-exception-after-map-mutation");
            }

            return Store(CreateReceipt(
                "template_registration_failed",
                accepted: false,
                registered: false,
                nativeMutationOccurred
                    ? "native-insertion-exception-after-map-mutation"
                    : "native-insertion-exception",
                sourceBow.GUID,
                sourceBow.name,
                sourceArrow.GUID,
                sourceArrow.name,
                addToMapInvocationCount,
                nativeMutationOccurred,
                bowClone?.SourceProfileHash ?? string.Empty,
                bowClone?.CloneProfileHash ?? string.Empty,
                arrowClone?.SourceProfileHash ?? string.Empty,
                arrowClone?.CloneProfileHash ?? string.Empty,
                DescribeException(ex)));
        }
    }

    private static bool TryPrepareClone(
        ItemTemplate source,
        string customGuid,
        string customName,
        string displayName,
        string description,
        bool expectShortBow,
        bool expectArrow,
        out PreparedClone? prepared,
        out string reason)
    {
        prepared = null;
        reason = string.Empty;

        string sourceGuidBefore = source.GUID ?? string.Empty;
        string sourceNameBefore = source.name ?? string.Empty;
        bool sourceCanStackBefore = source.CanStack;
        string[] sourceComponents = CaptureComponentProfile(source.gameObject);
        string[] sourceAttachments = CaptureAttachmentProfile(source.gameObject);
        string[] sourceReferences = CaptureTemplateReferenceProfile(source.gameObject, out bool sourceReferencesComplete);
        if (!sourceReferencesComplete)
        {
            reason = "source-template-reference-profile-over-budget";
            return false;
        }

        string sourceProfileHash = HashRows(sourceComponents, sourceAttachments, sourceReferences);
        GameObject cloneObject = Object.Instantiate(source.gameObject);
        cloneObject.name = customName;
        Object.DontDestroyOnLoad(cloneObject);

        ItemTemplate? custom = cloneObject.GetComponent<ItemTemplate>();
        if (custom == null)
        {
            Object.Destroy(cloneObject);
            reason = "cloned-object-missing-item-template";
            return false;
        }

        custom.name = customName;
        custom.GUID = customGuid;
        custom.hiddenOnUI = false;
        custom.cannotBeDropped = false;
        custom.itemName = (LocString)displayName;
        ItemTemplateDescriptionField?.SetValue(
            custom,
            new OptionalLocString((LocString)description, true));

        string[] cloneComponents = CaptureComponentProfile(custom.gameObject);
        string[] cloneAttachments = CaptureAttachmentProfile(custom.gameObject);
        string[] cloneReferences = CaptureTemplateReferenceProfile(custom.gameObject, out bool cloneReferencesComplete);
        string cloneProfileHash = HashRows(cloneComponents, cloneAttachments, cloneReferences);

        bool profileMatches = cloneReferencesComplete
            && sourceComponents.SequenceEqual(cloneComponents, StringComparer.Ordinal)
            && sourceAttachments.SequenceEqual(cloneAttachments, StringComparer.Ordinal)
            && sourceReferences.SequenceEqual(cloneReferences, StringComparer.Ordinal);
        bool identityMatches = string.Equals(source.GUID, sourceGuidBefore, StringComparison.Ordinal)
            && string.Equals(source.name, sourceNameBefore, StringComparison.Ordinal)
            && string.Equals(custom.GUID, customGuid, StringComparison.OrdinalIgnoreCase)
            && string.Equals(custom.name, customName, StringComparison.Ordinal)
            && custom.templateType == source.templateType;
        bool stackabilityPreserved = custom.CanStack == sourceCanStackBefore;
        bool categoryMatches = (!expectShortBow || custom.IsShortBow)
            && (!expectArrow || custom.IsArrow);
        bool projectilePathPreserved = !expectArrow || HasComponentNamed(custom.gameObject, "ItemProjectile");

        if (!profileMatches
            || !identityMatches
            || !stackabilityPreserved
            || !categoryMatches
            || !projectilePathPreserved)
        {
            Object.Destroy(cloneObject);
            reason = string.Join(";", new[]
            {
                "profileMatches=" + profileMatches,
                "identityMatches=" + identityMatches,
                "stackabilityPreserved=" + stackabilityPreserved,
                "categoryMatches=" + categoryMatches,
                "projectilePathPreserved=" + projectilePathPreserved,
            });
            return false;
        }

        prepared = new PreparedClone(custom, sourceProfileHash, cloneProfileHash);
        return true;
    }

    private static bool TryResolveExactSource(
        IEnumerable<ItemTemplate> templates,
        IReadOnlyCollection<string> aliases,
        string label,
        out ItemTemplate? template,
        out string reason)
    {
        template = null;
        reason = string.Empty;

        ItemTemplate[] exact = templates
            .Where(candidate => candidate != null
                && aliases.Any(alias => string.Equals(candidate.name, alias, StringComparison.OrdinalIgnoreCase)))
            .Distinct()
            .ToArray();
        if (exact.Length == 1)
        {
            template = exact[0];
            return true;
        }
        if (exact.Length > 1)
        {
            reason = label + " exact aliases resolved more than one template: "
                + string.Join("|", exact.Select(candidate => candidate.name + "[" + candidate.GUID + "]"));
            return false;
        }

        ItemTemplate[] suffix = templates
            .Where(candidate => candidate != null
                && aliases.Any(alias => (candidate.name ?? string.Empty).EndsWith(alias, StringComparison.OrdinalIgnoreCase)))
            .Distinct()
            .ToArray();
        if (suffix.Length == 1)
        {
            template = suffix[0];
            return true;
        }

        reason = suffix.Length == 0
            ? label + " source was not present under any exact reviewed alias: " + string.Join("|", aliases)
            : label + " suffix aliases were ambiguous: "
                + string.Join("|", suffix.Select(candidate => candidate.name + "[" + candidate.GUID + "]"));
        return false;
    }

    private static bool ValidateSourceBow(ItemTemplate template, out string reason)
    {
        reason = $"abstract={template.IsAbstract}, hidden={template.HiddenOnUI}, cannotDrop={template.CannotBeDropped}, isShortBow={template.IsShortBow}";
        return !template.IsAbstract
            && !template.HiddenOnUI
            && !template.CannotBeDropped
            && template.IsShortBow;
    }

    private static bool ValidateSourceArrow(ItemTemplate template, out string reason)
    {
        bool hasProjectile = HasComponentNamed(template.gameObject, "ItemProjectile");
        reason = $"abstract={template.IsAbstract}, hidden={template.HiddenOnUI}, cannotDrop={template.CannotBeDropped}, isArrow={template.IsArrow}, hasItemProjectile={hasProjectile}";
        return !template.IsAbstract
            && !template.HiddenOnUI
            && !template.CannotBeDropped
            && template.IsArrow
            && hasProjectile;
    }

    private static bool HasComponentNamed(GameObject root, string typeName)
    {
        return root.GetComponentsInChildren<Component>(true).Any(component =>
            component != null
            && (string.Equals(component.GetType().Name, typeName, StringComparison.Ordinal)
                || string.Equals(component.GetType().FullName, typeName, StringComparison.Ordinal)
                || (component.GetType().FullName ?? string.Empty).EndsWith("." + typeName, StringComparison.Ordinal)));
    }

    private static ItemTemplate? SafeGet(TemplatesProvider provider, string guid)
    {
        try
        {
            return provider.Get<ItemTemplate>(guid);
        }
        catch
        {
            return null;
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

        reason = missing.Count == 0 ? "ready" : "missing:" + string.Join("|", missing);
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
                string type = component == null
                    ? "<missing>"
                    : component.GetType().AssemblyQualifiedName ?? component.GetType().FullName ?? component.GetType().Name;
                rows.Add(path + "#" + index.ToString(CultureInfo.InvariantCulture) + "=" + type);
            }
        }

        rows.Sort(StringComparer.Ordinal);
        return rows.ToArray();
    }

    private static string[] CaptureAttachmentProfile(GameObject root)
    {
        return root.GetComponentsInChildren<Component>(true)
            .Where(component => component is IAttachmentGroup)
            .Select(component => IndexedPath(root.transform, component.transform) + "="
                + (component.GetType().AssemblyQualifiedName ?? component.GetType().FullName ?? component.GetType().Name))
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

            string componentPath = IndexedPath(root.transform, component.transform) + "="
                + (component.GetType().FullName ?? component.GetType().Name);
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
        if (value is TemplateReference reference)
        {
            rows.Add(prefix + "=" + ReadTemplateReferenceGuid(reference));
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
                if (entry is TemplateReference item)
                {
                    rows.Add(prefix + "[" + index.ToString(CultureInfo.InvariantCulture) + "]=" + ReadTemplateReferenceGuid(item));
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
        using SHA256 sha = SHA256.Create();
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

        byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
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

    private static TaintedWeaponsTamingToolSetReceipt Denied(
        string reasonCode,
        string details,
        string sourceBowGuid = "",
        string sourceBowName = "",
        string sourceArrowGuid = "",
        string sourceArrowName = "",
        string bowSourceProfileHash = "",
        string bowCloneProfileHash = "")
    {
        return CreateReceipt(
            "template_registration_denied",
            accepted: false,
            registered: false,
            reasonCode,
            sourceBowGuid,
            sourceBowName,
            sourceArrowGuid,
            sourceArrowName,
            0,
            nativeMutationOccurred: false,
            bowSourceProfileHash,
            bowCloneProfileHash,
            string.Empty,
            string.Empty,
            details);
    }

    private static TaintedWeaponsTamingToolSetReceipt Collision(
        string reasonCode,
        string details,
        ItemTemplate sourceBow,
        ItemTemplate sourceArrow)
    {
        return CreateReceipt(
            "template_registration_collision",
            accepted: false,
            registered: false,
            reasonCode,
            sourceBow.GUID,
            sourceBow.name,
            sourceArrow.GUID,
            sourceArrow.name,
            0,
            nativeMutationOccurred: false,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            details);
    }

    private static TaintedWeaponsTamingToolSetReceipt CreateReceipt(
        string status,
        bool accepted,
        bool registered,
        string reasonCode,
        string sourceBowGuid,
        string sourceBowName,
        string sourceArrowGuid,
        string sourceArrowName,
        int addToMapInvocationCount,
        bool nativeMutationOccurred,
        string bowSourceProfileHash,
        string bowCloneProfileHash,
        string arrowSourceProfileHash,
        string arrowCloneProfileHash,
        string details)
    {
        return new TaintedWeaponsTamingToolSetReceipt(
            status,
            accepted,
            registered,
            reasonCode,
            sourceBowGuid ?? string.Empty,
            sourceBowName ?? string.Empty,
            sourceArrowGuid ?? string.Empty,
            sourceArrowName ?? string.Empty,
            addToMapInvocationCount,
            nativeMutationOccurred,
            bowSourceProfileHash ?? string.Empty,
            bowCloneProfileHash ?? string.Empty,
            arrowSourceProfileHash ?? string.Empty,
            arrowCloneProfileHash ?? string.Empty,
            details ?? string.Empty);
    }

    private static TaintedWeaponsTamingToolSetReceipt Store(TaintedWeaponsTamingToolSetReceipt receipt)
    {
        _receipt = receipt;
        Plugin? plugin = Plugin.Instance;
        if (plugin != null)
        {
            Log(plugin.ModLogger, receipt);
        }
        return receipt;
    }

    private static void Log(BepInEx.Logging.ManualLogSource logger, TaintedWeaponsTamingToolSetReceipt receipt)
    {
        string row = receipt.ToString();
        if (string.Equals(row, _lastLoggedReceipt, StringComparison.Ordinal))
        {
            return;
        }
        _lastLoggedReceipt = row;
        if (receipt.Accepted)
        {
            logger.LogInfo(Plugin.PluginName + " Taming Tool item-set receipt; " + row);
        }
        else
        {
            logger.LogWarning(Plugin.PluginName + " Taming Tool item-set receipt; " + row);
        }
    }

    private static void Poison(string reason)
    {
        _terminalPoisonedForProcess = true;
        _terminalPoisonReason = reason;
        Plugin.Instance?.ModLogger.LogError(
            Plugin.PluginName + " Taming Tool registrar terminal poison; reason=" + reason + "; restartRequired=true");
    }

    private sealed class PreparedClone
    {
        private bool _registered;

        internal PreparedClone(ItemTemplate template, string sourceProfileHash, string cloneProfileHash)
        {
            Template = template;
            SourceProfileHash = sourceProfileHash;
            CloneProfileHash = cloneProfileHash;
        }

        internal ItemTemplate Template { get; }

        internal string SourceProfileHash { get; }

        internal string CloneProfileHash { get; }

        internal void MarkRegistered()
        {
            _registered = true;
        }

        internal void DestroyIfUnregistered()
        {
            if (!_registered && Template != null)
            {
                Object.Destroy(Template.gameObject);
            }
        }
    }
}
