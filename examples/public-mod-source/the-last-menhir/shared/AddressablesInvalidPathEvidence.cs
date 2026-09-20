using System;
using System.Text.RegularExpressions;

namespace AvalonExceptions;

internal static class AddressablesInvalidPathEvidence
{
    private static readonly Regex InvalidPathRegex = new Regex(
        @"Invalid\s+path\s+in\s+AssetBundleProvider:\s*'(?<path>[^']+?\.bundle)'",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex LocalModBundleRegex = new Regex(
        @"(?:^|/)Mods/(?<mod>[^/]+)/(?<bundle>[^'\r\n]+?\.bundle)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    internal static bool TryExtract(string text, out AddressablesInvalidPathFinding finding)
    {
        finding = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        Match invalidPath = InvalidPathRegex.Match(text);
        if (!invalidPath.Success)
        {
            return false;
        }

        string rawPath = invalidPath.Groups["path"].Value.Trim();
        string normalizedPath = rawPath.Replace('\\', '/');
        Match localModBundle = LocalModBundleRegex.Match(normalizedPath);
        if (!localModBundle.Success)
        {
            return false;
        }

        string modId = localModBundle.Groups["mod"].Value.Trim();
        string bundlePath = NormalizeBundlePath(localModBundle.Groups["bundle"].Value);

        finding = new AddressablesInvalidPathFinding(modId, bundlePath);
        return true;
    }

    internal static bool LooksLikeCascadeWrapper(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return text.IndexOf("GroupOperation failed because one of its dependencies failed", StringComparison.OrdinalIgnoreCase) >= 0
            && (text.IndexOf("OperationException", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("Dependency Exception", StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static string NormalizeBundlePath(string bundlePath)
    {
        return (bundlePath ?? string.Empty)
            .Replace('\\', '/')
            .Trim()
            .Trim('/');
    }
}

internal readonly struct AddressablesInvalidPathFinding
{
    internal AddressablesInvalidPathFinding(string modId, string bundlePath)
    {
        ModId = modId ?? string.Empty;
        BundlePath = bundlePath ?? string.Empty;
    }

    internal string ModId { get; }
    internal string BundlePath { get; }
    internal string LocalModFolderHint
    {
        get
        {
            return string.IsNullOrWhiteSpace(ModId)
                ? string.Empty
                : $"LocalLow/Questline/Fall of Avalon/Mods/{ModId}";
        }
    }

    internal string RouteLabel
    {
        get
        {
            return string.IsNullOrWhiteSpace(ModId)
                ? "Broken local mod asset bundle"
                : $"Broken local mod asset bundle: {ModId}";
        }
    }

    internal string RouteEvidence
    {
        get
        {
            string mod = string.IsNullOrWhiteSpace(ModId) ? "mod=unknown" : $"mod={ModId}";
            string bundle = string.IsNullOrWhiteSpace(BundlePath) ? "bundle=unknown" : $"bundle={BundlePath}";
            return $"RemoteProviderException; Invalid path in AssetBundleProvider; {mod}; {bundle}";
        }
    }

    internal string LikelyCause
    {
        get
        {
            string mod = string.IsNullOrWhiteSpace(ModId)
                ? "a local mod"
                : $"local mod `{ModId}`";
            string bundle = string.IsNullOrWhiteSpace(BundlePath)
                ? "an Addressables asset bundle"
                : $"asset bundle `{BundlePath}`";
            return $"Addressables could not load {bundle} for {mod} because the local asset bundle path is invalid.";
        }
    }

    internal string RecommendedAction
    {
        get
        {
            return "Fix, reinstall, rebuild, or remove this local mod folder. If the mod should stay installed, redeploy or rebuild the referenced asset bundle from a matching mod release.";
        }
    }

    internal string BestNextCheck
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ModId))
            {
                return "Check the LocalLow/Fall of Avalon/Mods folder for the referenced .bundle file, stale catalog entries, or a malformed bundle path.";
            }

            return $"Check {LocalModFolderHint} for the referenced .bundle file, stale catalog entries, or a malformed bundle path.";
        }
    }

    internal string GroupKey
    {
        get
        {
            return string.Join(
                "|",
                ModId.Trim().ToLowerInvariant(),
                BundlePath.Trim().ToLowerInvariant());
        }
    }
}
