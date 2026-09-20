using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AvalonExceptions;

internal static class LocalModPreflight
{
    private static readonly string[] CatalogBase64Fields =
    {
        "m_KeyDataString",
        "m_BucketDataString",
        "m_EntryDataString",
        "m_ExtraDataString"
    };

    private static readonly Regex BundlePathRegex = new Regex(
        "\"(?<path>[^\"\\r\\n]*?\\.bundle)\"",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    internal static LocalModPreflightRow[] Build(string modsPath)
    {
        if (string.IsNullOrWhiteSpace(modsPath) || !Directory.Exists(modsPath))
        {
            return Array.Empty<LocalModPreflightRow>();
        }

        try
        {
            List<LocalModPreflightRow> rows = new List<LocalModPreflightRow>();
            foreach (string directory in Directory.GetDirectories(modsPath).OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                string modId = Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) ?? string.Empty;
                string catalogPath = Path.Combine(directory, "catalog.json");
                if (!File.Exists(catalogPath))
                {
                    rows.Add(LocalModPreflightRow.CatalogMissing(modId));
                    continue;
                }

                try
                {
                    string catalogText = File.ReadAllText(catalogPath, Encoding.UTF8);
                    rows.Add(BuildForCatalog(modId, catalogText, relativePath => LocalBundleExists(directory, relativePath)));
                }
                catch (Exception ex)
                {
                    rows.Add(LocalModPreflightRow.CatalogReadError(modId, $"{ex.GetType().Name}: {ex.Message}"));
                }
            }

            return rows.ToArray();
        }
        catch
        {
            return Array.Empty<LocalModPreflightRow>();
        }
    }

    internal static LocalModPreflightRow BuildForCatalog(string modId, string catalogText, Func<string, bool> bundleExists)
    {
        string normalizedModId = NormalizeModId(modId);
        string text = catalogText ?? string.Empty;
        List<string> invalidBase64Fields = new List<string>();
        foreach (string field in CatalogBase64Fields)
        {
            JsonFieldRead fieldRead = ReadJsonStringField(text, field);
            if (!fieldRead.Found)
            {
                invalidBase64Fields.Add(field + "=missing");
                continue;
            }

            if (fieldRead.IsNull || string.IsNullOrWhiteSpace(fieldRead.Value))
            {
                invalidBase64Fields.Add(field + "=null-or-empty");
                continue;
            }

            try
            {
                Convert.FromBase64String(fieldRead.Value);
            }
            catch (FormatException)
            {
                invalidBase64Fields.Add(field + "=invalid-base64");
            }
        }

        string[] bundles = ExtractBundleReferences(text, normalizedModId);
        string[] missingBundles = bundles
            .Where(relativePath => !bundleExists(relativePath))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        string status = invalidBase64Fields.Count == 0 && missingBundles.Length == 0
            ? "catalog-ok"
            : "catalog-has-issues";
        string evidence = BuildEvidence(bundles.Length, missingBundles, invalidBase64Fields);

        return new LocalModPreflightRow(
            normalizedModId,
            "catalog.json",
            status,
            bundles.Length,
            missingBundles.Length,
            invalidBase64Fields.Count,
            string.Join("|", missingBundles),
            string.Join("|", invalidBase64Fields),
            evidence);
    }

    internal static string ToCsv(IEnumerable<LocalModPreflightRow> rows)
    {
        StringBuilder builder = new StringBuilder();
        AppendCsvRow(
            builder,
            "localModId",
            "catalogRelativePath",
            "status",
            "referencedBundleCount",
            "missingBundleCount",
            "invalidBase64FieldCount",
            "missingBundlePaths",
            "invalidBase64Fields",
            "evidence");
        foreach (LocalModPreflightRow row in rows ?? Array.Empty<LocalModPreflightRow>())
        {
            AppendCsvRow(
                builder,
                row.LocalModId,
                row.CatalogRelativePath,
                row.Status,
                row.ReferencedBundleCount.ToString(CultureInfo.InvariantCulture),
                row.MissingBundleCount.ToString(CultureInfo.InvariantCulture),
                row.InvalidBase64FieldCount.ToString(CultureInfo.InvariantCulture),
                row.MissingBundlePaths,
                row.InvalidBase64Fields,
                row.Evidence);
        }

        return builder.ToString();
    }

    private static bool LocalBundleExists(string modFolder, string relativePath)
    {
        try
        {
            string normalized = (relativePath ?? string.Empty).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            string fullModFolder = Path.GetFullPath(modFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string prefix = fullModFolder + Path.DirectorySeparatorChar;
            string candidate = Path.GetFullPath(Path.Combine(fullModFolder, normalized));
            return candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && File.Exists(candidate);
        }
        catch
        {
            return false;
        }
    }

    private static string[] ExtractBundleReferences(string catalogText, string modId)
    {
        HashSet<string> references = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in BundlePathRegex.Matches(catalogText ?? string.Empty))
        {
            string relativePath = NormalizeBundlePath(match.Groups["path"].Value, modId);
            if (!string.IsNullOrWhiteSpace(relativePath))
            {
                references.Add(relativePath);
            }
        }

        return references.OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string NormalizeBundlePath(string value, string modId)
    {
        string normalized = (value ?? string.Empty).Replace('\\', '/').Trim().Trim('/');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        int modsIndex = normalized.IndexOf("/Mods/", StringComparison.OrdinalIgnoreCase);
        if (modsIndex >= 0)
        {
            string afterMods = normalized.Substring(modsIndex + "/Mods/".Length).Trim('/');
            if (!string.IsNullOrWhiteSpace(modId)
                && afterMods.StartsWith(modId + "/", StringComparison.OrdinalIgnoreCase))
            {
                return afterMods.Substring(modId.Length + 1).Trim('/');
            }

            int firstSlash = afterMods.IndexOf('/');
            return firstSlash >= 0 ? afterMods.Substring(firstSlash + 1).Trim('/') : string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(modId)
            && normalized.StartsWith(modId + "/", StringComparison.OrdinalIgnoreCase))
        {
            return normalized.Substring(modId.Length + 1).Trim('/');
        }

        int fileNameOnlyStart = normalized.LastIndexOf('/');
        return fileNameOnlyStart >= 0 ? normalized.Substring(fileNameOnlyStart + 1) : normalized;
    }

    private static string BuildEvidence(int bundleCount, string[] missingBundles, List<string> invalidBase64Fields)
    {
        List<string> parts = new List<string>
        {
            $"referencedBundles={bundleCount.ToString(CultureInfo.InvariantCulture)}",
            $"missingBundles={missingBundles.Length.ToString(CultureInfo.InvariantCulture)}",
            $"invalidBase64Fields={invalidBase64Fields.Count.ToString(CultureInfo.InvariantCulture)}"
        };
        if (missingBundles.Length > 0)
        {
            parts.Add("firstMissingBundle=" + missingBundles[0]);
        }

        if (invalidBase64Fields.Count > 0)
        {
            parts.Add("firstInvalidBase64Field=" + invalidBase64Fields[0]);
        }

        return string.Join("; ", parts);
    }

    private static JsonFieldRead ReadJsonStringField(string text, string field)
    {
        Match match = Regex.Match(
            text ?? string.Empty,
            $"\"{Regex.Escape(field)}\"\\s*:\\s*(?:\"(?<value>(?:\\\\.|[^\"])*)\"|(?<null>null))",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return JsonFieldRead.Missing;
        }

        if (match.Groups["null"].Success)
        {
            return JsonFieldRead.Null;
        }

        return JsonFieldRead.FromValue(Regex.Unescape(match.Groups["value"].Value));
    }

    private static string NormalizeModId(string value)
    {
        return (value ?? string.Empty)
            .Replace('\\', '/')
            .Trim()
            .Trim('/');
    }

    private static void AppendCsvRow(StringBuilder builder, params string[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(',');
            }

            builder.Append(Csv(values[i]));
        }

        builder.AppendLine();
    }

    private static string Csv(string value)
    {
        value ??= string.Empty;
        bool quote = value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
        return quote ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
    }

    private readonly struct JsonFieldRead
    {
        private JsonFieldRead(bool found, bool isNull, string value)
        {
            Found = found;
            IsNull = isNull;
            Value = value;
        }

        internal bool Found { get; }
        internal bool IsNull { get; }
        internal string Value { get; }

        internal static JsonFieldRead Missing => new JsonFieldRead(false, false, string.Empty);
        internal static JsonFieldRead Null => new JsonFieldRead(true, true, string.Empty);

        internal static JsonFieldRead FromValue(string value)
        {
            return new JsonFieldRead(true, false, value ?? string.Empty);
        }
    }
}

internal readonly struct LocalModPreflightRow
{
    internal LocalModPreflightRow(
        string localModId,
        string catalogRelativePath,
        string status,
        int referencedBundleCount,
        int missingBundleCount,
        int invalidBase64FieldCount,
        string missingBundlePaths,
        string invalidBase64Fields,
        string evidence)
    {
        LocalModId = localModId ?? string.Empty;
        CatalogRelativePath = catalogRelativePath ?? string.Empty;
        Status = status ?? string.Empty;
        ReferencedBundleCount = referencedBundleCount;
        MissingBundleCount = missingBundleCount;
        InvalidBase64FieldCount = invalidBase64FieldCount;
        MissingBundlePaths = missingBundlePaths ?? string.Empty;
        InvalidBase64Fields = invalidBase64Fields ?? string.Empty;
        Evidence = evidence ?? string.Empty;
    }

    internal string LocalModId { get; }
    internal string CatalogRelativePath { get; }
    internal string Status { get; }
    internal int ReferencedBundleCount { get; }
    internal int MissingBundleCount { get; }
    internal int InvalidBase64FieldCount { get; }
    internal string MissingBundlePaths { get; }
    internal string InvalidBase64Fields { get; }
    internal string Evidence { get; }

    internal bool HasIssue
    {
        get
        {
            return !string.Equals(Status, "catalog-ok", StringComparison.OrdinalIgnoreCase);
        }
    }

    internal static LocalModPreflightRow CatalogMissing(string modId)
    {
        return new LocalModPreflightRow(
            modId,
            "catalog.json",
            "catalog-missing",
            0,
            0,
            0,
            string.Empty,
            string.Empty,
            "catalog.json was not found in this local mod folder.");
    }

    internal static LocalModPreflightRow CatalogReadError(string modId, string evidence)
    {
        return new LocalModPreflightRow(
            modId,
            "catalog.json",
            "catalog-read-error",
            0,
            0,
            0,
            string.Empty,
            string.Empty,
            evidence);
    }
}
