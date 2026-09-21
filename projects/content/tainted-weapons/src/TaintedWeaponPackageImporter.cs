using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TaintedWeapons;

public static class TaintedWeaponPackageContracts
{
    public const int ContractVersion = 1;
    public const string ContractId = "tainted-weapons.weapon-package/1";
    public const string AcceptedStatus = "package_accepted";
    public const string RejectedStatus = "package_rejected";
    public const string ImportedStatus = "package_imported";
    public const string ImportRejectedStatus = "package_import_rejected";
    public const string ImportPartiallyQueuedStatus = "package_import_partially_queued";
}

public sealed class TaintedWeaponPackageManifest
{
    public TaintedWeaponPackageManifest(
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
        string flavorText)
    {
        PackageId = Required(packageId, nameof(packageId));
        WeaponId = Required(weaponId, nameof(weaponId));
        CustomTemplateGuid = Required(customTemplateGuid, nameof(customTemplateGuid));
        SourceTemplateGuid = Required(sourceTemplateGuid, nameof(sourceTemplateGuid));
        CustomTemplateName = Required(customTemplateName, nameof(customTemplateName));
        DisplayName = Required(displayName, nameof(displayName));
        Description = description ?? string.Empty;
        PackageRootDirectory = Required(packageRootDirectory, nameof(packageRootDirectory));
        BundleFileName = Required(bundleFileName, nameof(bundleFileName));
        EquippedPrefabAssetPath = Required(equippedPrefabAssetPath, nameof(equippedPrefabAssetPath));
        NativeRegistrationProfileId = string.IsNullOrWhiteSpace(nativeRegistrationProfileId)
            ? TaintedWeaponNativeItemRegistrationRequest.WeaponItemTemplateCloneProfile
            : nativeRegistrationProfileId.Trim();
        SemanticIconAddress = semanticIconAddress?.Trim() ?? string.Empty;
        FlavorText = flavorText?.Trim() ?? string.Empty;
    }

    public int Version => TaintedWeaponPackageContracts.ContractVersion;

    public string ContractId => TaintedWeaponPackageContracts.ContractId;

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

    public string NativeRegistrationProfileId { get; }

    public string SemanticIconAddress { get; }

    public string FlavorText { get; }

    public string RegistryKey => TaintedWeaponsIdentityPolicy.CanonicalRegistryKey(PackageId, WeaponId);

    internal TaintedWeaponDefinition ToDefinition()
        => new(
            PackageId,
            WeaponId,
            CustomTemplateGuid,
            SourceTemplateGuid,
            DisplayName,
            Description,
            PackageRootDirectory,
            BundleFileName,
            EquippedPrefabAssetPath,
            CustomTemplateName);

    internal TaintedWeaponNativeItemRegistrationRequest ToNativeRegistrationRequest()
        => new(
            PackageId,
            WeaponId,
            NativeRegistrationProfileId,
            SemanticIconAddress,
            FlavorText);

    private static string Required(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", name);
        }

        return value.Trim();
    }
}

public sealed class TaintedWeaponPackageValidationReceipt
{
    internal TaintedWeaponPackageValidationReceipt(
        string status,
        bool accepted,
        string reasonCode,
        string packageId,
        string weaponId,
        string registryKey,
        string packageRootDirectory,
        string bundlePath,
        long bundleBytes,
        string bundleSha256,
        string equippedPrefabAssetPath,
        IEnumerable<string> issues)
    {
        Status = status;
        Accepted = accepted;
        ReasonCode = Clean(reasonCode);
        PackageId = Clean(packageId);
        WeaponId = Clean(weaponId);
        RegistryKey = Clean(registryKey);
        PackageRootDirectory = Clean(packageRootDirectory);
        BundlePath = Clean(bundlePath);
        BundleBytes = bundleBytes;
        BundleSha256 = Clean(bundleSha256);
        EquippedPrefabAssetPath = Clean(equippedPrefabAssetPath);
        Issues = issues.Select(Clean).Where(issue => issue.Length > 0).ToArray();
    }

    public int Version => 1;

    public string ContractId => TaintedWeaponPackageContracts.ContractId;

    public string Status { get; }

    public bool Accepted { get; }

    public string ReasonCode { get; }

    public string PackageId { get; }

    public string WeaponId { get; }

    public string RegistryKey { get; }

    public string PackageRootDirectory { get; }

    public string BundlePath { get; }

    public long BundleBytes { get; }

    public string BundleSha256 { get; }

    public string EquippedPrefabAssetPath { get; }

    public IReadOnlyList<string> Issues { get; }

    public override string ToString()
    {
        return "version=" + Version.ToString(CultureInfo.InvariantCulture) +
            "; contractId=" + ContractId +
            "; status=" + Status +
            "; accepted=" + Accepted.ToString(CultureInfo.InvariantCulture) +
            "; reasonCode=" + ReasonCode +
            "; packageId=" + PackageId +
            "; weaponId=" + WeaponId +
            "; registryKey=" + RegistryKey +
            "; packageRoot=" + PackageRootDirectory +
            "; bundlePath=" + BundlePath +
            "; bundleBytes=" + BundleBytes.ToString(CultureInfo.InvariantCulture) +
            "; bundleSha256=" + BundleSha256 +
            "; equippedPrefabAssetPath=" + EquippedPrefabAssetPath +
            "; issues=" + (Issues.Count == 0 ? "none" : string.Join("|", Issues));
    }

    internal static TaintedWeaponPackageValidationReceipt InvalidManifest(string reason)
        => new(
            TaintedWeaponPackageContracts.RejectedStatus,
            accepted: false,
            reason,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            0,
            string.Empty,
            string.Empty,
            new[] { reason });

    private static string Clean(string value)
    {
        return (value ?? string.Empty)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace(';', ',')
            .Trim();
    }
}

public sealed class TaintedWeaponPackageImportReceipt
{
    internal TaintedWeaponPackageImportReceipt(
        string status,
        bool accepted,
        string reasonCode,
        TaintedWeaponPackageValidationReceipt validationReceipt,
        bool definitionAccepted,
        string definitionMessage,
        bool nativeRegistrationRequested,
        bool nativeRegistrationAccepted,
        TaintedWeaponNativeItemRegistrationReceipt? nativeRegistrationReceipt)
    {
        Status = Clean(status);
        Accepted = accepted;
        ReasonCode = Clean(reasonCode);
        ValidationReceipt = validationReceipt;
        DefinitionAccepted = definitionAccepted;
        DefinitionMessage = Clean(definitionMessage);
        NativeRegistrationRequested = nativeRegistrationRequested;
        NativeRegistrationAccepted = nativeRegistrationAccepted;
        NativeRegistrationReceipt = nativeRegistrationReceipt;
    }

    public int Version => 1;

    public string ContractId => TaintedWeaponPackageContracts.ContractId;

    public string Status { get; }

    public bool Accepted { get; }

    public string ReasonCode { get; }

    public TaintedWeaponPackageValidationReceipt ValidationReceipt { get; }

    public bool DefinitionAccepted { get; }

    public string DefinitionMessage { get; }

    public bool NativeRegistrationRequested { get; }

    public bool NativeRegistrationAccepted { get; }

    public TaintedWeaponNativeItemRegistrationReceipt? NativeRegistrationReceipt { get; }

    public override string ToString()
    {
        string nativeReceipt = NativeRegistrationReceipt?.ToString() ?? "none";
        return "version=" + Version.ToString(CultureInfo.InvariantCulture) +
            "; contractId=" + ContractId +
            "; status=" + Status +
            "; accepted=" + Accepted.ToString(CultureInfo.InvariantCulture) +
            "; reasonCode=" + ReasonCode +
            "; validation={" + ValidationReceipt + "}" +
            "; definitionAccepted=" + DefinitionAccepted.ToString(CultureInfo.InvariantCulture) +
            "; definitionMessage=" + DefinitionMessage +
            "; nativeRegistrationRequested=" + NativeRegistrationRequested.ToString(CultureInfo.InvariantCulture) +
            "; nativeRegistrationAccepted=" + NativeRegistrationAccepted.ToString(CultureInfo.InvariantCulture) +
            "; nativeRegistrationReceipt={" + nativeReceipt + "}";
    }

    internal static TaintedWeaponPackageImportReceipt DeniedInvalidManifest(string reason)
    {
        return new TaintedWeaponPackageImportReceipt(
            TaintedWeaponPackageContracts.ImportRejectedStatus,
            accepted: false,
            reason,
            TaintedWeaponPackageValidationReceipt.InvalidManifest(reason),
            definitionAccepted: false,
            string.Empty,
            nativeRegistrationRequested: false,
            nativeRegistrationAccepted: false,
            nativeRegistrationReceipt: null);
    }

    private static string Clean(string value)
    {
        return (value ?? string.Empty)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace(';', ',')
            .Replace('|', '/')
            .Trim();
    }
}

internal static class TaintedWeaponPackageImporter
{
    private const long MaxBundleBytes = 134217728;
    private const int MaxLocatorCharacters = 240;
    private static readonly string[] ForbiddenPackagePayloadExtensions = { ".dll", ".exe", ".bat", ".cmd", ".ps1" };

    internal static bool Register(
        TaintedWeaponPackageManifest manifest,
        bool requestNativeRegistration,
        out TaintedWeaponPackageImportReceipt receipt)
    {
        TaintedWeaponPackageValidationReceipt validation = Validate(manifest);
        if (!validation.Accepted)
        {
            receipt = new TaintedWeaponPackageImportReceipt(
                TaintedWeaponPackageContracts.ImportRejectedStatus,
                accepted: false,
                validation.ReasonCode,
                validation,
                definitionAccepted: false,
                string.Empty,
                nativeRegistrationRequested: false,
                nativeRegistrationAccepted: false,
                nativeRegistrationReceipt: null);
            return false;
        }

        bool definitionAccepted = TaintedWeaponsApi.RegisterWeaponDefinition(manifest.ToDefinition(), out string definitionMessage);
        if (!definitionAccepted)
        {
            receipt = new TaintedWeaponPackageImportReceipt(
                TaintedWeaponPackageContracts.ImportRejectedStatus,
                accepted: false,
                "definition-registration-denied",
                validation,
                definitionAccepted: false,
                definitionMessage,
                nativeRegistrationRequested: requestNativeRegistration,
                nativeRegistrationAccepted: false,
                nativeRegistrationReceipt: null);
            return false;
        }

        bool nativeRegistrationAccepted = true;
        TaintedWeaponNativeItemRegistrationReceipt? nativeReceipt = null;
        if (requestNativeRegistration)
        {
            nativeRegistrationAccepted = TaintedWeaponsApi.RegisterNativeWeaponTemplate(
                manifest.ToNativeRegistrationRequest(),
                out TaintedWeaponNativeItemRegistrationReceipt registrationReceipt);
            nativeReceipt = registrationReceipt;
        }

        bool accepted = definitionAccepted && (!requestNativeRegistration || nativeRegistrationAccepted);
        string status = !requestNativeRegistration
            ? TaintedWeaponPackageContracts.ImportedStatus
            : nativeReceipt?.Registered == true
                ? TaintedWeaponPackageContracts.ImportedStatus
                : nativeRegistrationAccepted
                    ? TaintedWeaponPackageContracts.ImportPartiallyQueuedStatus
                    : TaintedWeaponPackageContracts.ImportRejectedStatus;
        string reason = accepted
            ? requestNativeRegistration
                ? nativeReceipt?.ReasonCode ?? "native-registration-requested"
                : "definition-registered"
            : "native-registration-denied";

        receipt = new TaintedWeaponPackageImportReceipt(
            status,
            accepted,
            reason,
            validation,
            definitionAccepted,
            definitionMessage,
            requestNativeRegistration,
            nativeRegistrationAccepted,
            nativeReceipt);
        return accepted;
    }

    internal static TaintedWeaponPackageValidationReceipt Validate(TaintedWeaponPackageManifest manifest)
    {
        if (manifest == null)
        {
            return TaintedWeaponPackageValidationReceipt.InvalidManifest("manifest-null");
        }

        var issues = new List<string>();
        string packageRoot = string.Empty;
        string bundlePath = string.Empty;
        long bundleBytes = 0;
        string bundleSha256 = string.Empty;

        ValidateLocator("package-id", manifest.PackageId, issues);
        ValidateLocator("weapon-id", manifest.WeaponId, issues);
        ValidateLocator("custom-template-guid", manifest.CustomTemplateGuid, issues);
        ValidateLocator("source-template-guid", manifest.SourceTemplateGuid, issues);
        ValidateLocator("custom-template-name", manifest.CustomTemplateName, issues);
        ValidateLocator("bundle-file-name", manifest.BundleFileName, issues);
        ValidateLocator("equipped-prefab-asset-path", manifest.EquippedPrefabAssetPath, issues);

        if (!string.Equals(
                manifest.NativeRegistrationProfileId,
                TaintedWeaponNativeItemRegistrationRequest.WeaponItemTemplateCloneProfile,
                StringComparison.OrdinalIgnoreCase))
        {
            issues.Add("unsupported-native-registration-profile:" + manifest.NativeRegistrationProfileId);
        }

        try
        {
            packageRoot = Path.GetFullPath(manifest.PackageRootDirectory);
            if (!Directory.Exists(packageRoot))
            {
                issues.Add("package-root-missing:" + packageRoot);
            }
        }
        catch (Exception ex)
        {
            issues.Add("package-root-invalid:" + ex.GetType().Name);
        }

        if (IsUnsafeRelativePath(manifest.BundleFileName))
        {
            issues.Add("bundle-path-unsafe:" + manifest.BundleFileName);
        }
        else if (!string.IsNullOrWhiteSpace(packageRoot))
        {
            try
            {
                bundlePath = Path.GetFullPath(Path.Combine(packageRoot, manifest.BundleFileName));
                if (!IsInsideDirectory(packageRoot, bundlePath))
                {
                    issues.Add("bundle-path-escapes-package:" + manifest.BundleFileName);
                }
                else if (ForbiddenPackagePayloadExtensions.Contains(Path.GetExtension(bundlePath), StringComparer.OrdinalIgnoreCase))
                {
                    issues.Add("bundle-path-forbidden-payload:" + manifest.BundleFileName);
                }
                else if (!File.Exists(bundlePath))
                {
                    issues.Add("bundle-missing:" + bundlePath);
                }
                else
                {
                    var info = new FileInfo(bundlePath);
                    bundleBytes = info.Length;
                    if (bundleBytes <= 0)
                    {
                        issues.Add("bundle-empty:" + bundlePath);
                    }
                    else if (bundleBytes > MaxBundleBytes)
                    {
                        issues.Add("bundle-too-large:" + bundleBytes.ToString(CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        bundleSha256 = "sha256:" + ComputeSha256(bundlePath);
                    }
                }
            }
            catch (Exception ex)
            {
                issues.Add("bundle-path-invalid:" + ex.GetType().Name);
            }
        }

        if (IsUnsafeRelativePath(manifest.EquippedPrefabAssetPath))
        {
            issues.Add("equipped-prefab-path-unsafe:" + manifest.EquippedPrefabAssetPath);
        }

        bool accepted = issues.Count == 0;
        string reason = accepted ? "ok" : issues[0];
        return new TaintedWeaponPackageValidationReceipt(
            accepted ? TaintedWeaponPackageContracts.AcceptedStatus : TaintedWeaponPackageContracts.RejectedStatus,
            accepted,
            reason,
            manifest.PackageId,
            manifest.WeaponId,
            manifest.RegistryKey,
            packageRoot,
            bundlePath,
            bundleBytes,
            bundleSha256,
            manifest.EquippedPrefabAssetPath,
            issues);
    }

    private static void ValidateLocator(string name, string value, List<string> issues)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            issues.Add(name + "-missing");
            return;
        }

        if (value.Length > MaxLocatorCharacters)
        {
            issues.Add(name + "-too-long");
        }
    }

    private static bool IsUnsafeRelativePath(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.IndexOf('\0') >= 0 ||
            Path.IsPathRooted(value) ||
            value.StartsWith(@"\\", StringComparison.Ordinal))
        {
            return true;
        }

        string[] segments = value.Replace('\\', '/').Split('/');
        return segments.Any(segment =>
            string.IsNullOrWhiteSpace(segment) ||
            string.Equals(segment, ".", StringComparison.Ordinal) ||
            string.Equals(segment, "..", StringComparison.Ordinal));
    }

    private static bool IsInsideDirectory(string rootDirectory, string candidatePath)
    {
        string root = Path.GetFullPath(rootDirectory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
            Path.DirectorySeparatorChar;
        string candidate = Path.GetFullPath(candidatePath);
        return candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeSha256(string path)
    {
        using FileStream stream = File.OpenRead(path);
        using SHA256 sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(stream);
        var builder = new StringBuilder(hash.Length * 2);
        foreach (byte value in hash)
        {
            builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }
}
