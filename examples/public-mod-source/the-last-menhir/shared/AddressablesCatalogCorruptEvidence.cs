using System;
using System.Collections.Generic;
using System.Linq;

namespace AvalonExceptions;

internal static class AddressablesCatalogCorruptEvidence
{
    internal const string RouteId = "addressables-local-mod-catalog-corrupt";

    internal static bool LooksLikeCatalogCorruption(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        bool hasCatalogLocator = text.IndexOf("UnityEngine.AddressableAssets.ResourceLocators.ContentCatalogData.CreateLocator", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("ContentCatalogData.CreateLocator", StringComparison.OrdinalIgnoreCase) >= 0;
        bool hasLocalModCatalogLoad = text.IndexOf("Awaken.TG.Assets.Modding.Mod.LoadCatalog", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("Awaken.TG.Assets.Modding.Mod:LoadCatalog", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("Awaken.Utility.Assets.Modding.ModManager", StringComparison.OrdinalIgnoreCase) >= 0;
        bool hasKnownCatalogFailure = (text.IndexOf("ArgumentNullException", StringComparison.OrdinalIgnoreCase) >= 0
                && text.IndexOf("System.Convert.FromBase64String", StringComparison.OrdinalIgnoreCase) >= 0)
            || (text.IndexOf("ArgumentOutOfRangeException", StringComparison.OrdinalIgnoreCase) >= 0
                && text.IndexOf("System.BitConverter.ToInt32", StringComparison.OrdinalIgnoreCase) >= 0);

        return hasCatalogLocator && hasLocalModCatalogLoad && hasKnownCatalogFailure;
    }

    internal static bool TryBuildFinding(
        string text,
        IEnumerable<string> catalogBearingLocalModIds,
        out AddressablesCatalogCorruptFinding finding)
    {
        finding = default;
        if (!LooksLikeCatalogCorruption(text))
        {
            return false;
        }

        string[] candidates = (catalogBearingLocalModIds ?? Array.Empty<string>())
            .Select(NormalizeModId)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (candidates.Length != 1)
        {
            return false;
        }

        finding = new AddressablesCatalogCorruptFinding(candidates[0]);
        return true;
    }

    private static string NormalizeModId(string value)
    {
        return (value ?? string.Empty)
            .Replace('\\', '/')
            .Trim()
            .Trim('/');
    }
}

internal readonly struct AddressablesCatalogCorruptFinding
{
    internal AddressablesCatalogCorruptFinding(string modId)
    {
        ModId = modId ?? string.Empty;
    }

    internal string ModId { get; }
    internal string CatalogRelativePath => "catalog.json";
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
                ? "Broken local mod catalog"
                : $"Broken local mod catalog: {ModId}";
        }
    }

    internal string RouteEvidence
    {
        get
        {
            string mod = string.IsNullOrWhiteSpace(ModId)
                ? "candidate=unknown"
                : $"candidate={ModId}; candidateReason=single catalog-bearing local mod folder";
            return $"Addressables ContentCatalogData.CreateLocator failed while loading a local mod catalog; {mod}";
        }
    }

    internal string LikelyCause
    {
        get
        {
            return string.IsNullOrWhiteSpace(ModId)
                ? "A local mod catalog appears malformed or incompatible with Addressables, but The Last Menhir could not identify a single local mod folder from bounded runtime evidence."
                : $"Local mod `{ModId}` is the only catalog-bearing local mod folder observed while Addressables failed to parse a local mod catalog.";
        }
    }

    internal string RecommendedAction
    {
        get
        {
            return "Fix, reinstall, rebuild, or remove this local mod folder. If the mod should stay installed, redeploy or rebuild its catalog.json and matching asset bundles from the same mod release.";
        }
    }

    internal string BestNextCheck
    {
        get
        {
            return string.IsNullOrWhiteSpace(ModId)
                ? "Check LocalLow/Questline/Fall of Avalon/Mods for local mod folders containing catalog.json, then rebuild or remove the malformed catalog package."
                : $"Check {LocalModFolderHint}/catalog.json and confirm it matches the asset bundles shipped with that local mod folder.";
        }
    }
}
