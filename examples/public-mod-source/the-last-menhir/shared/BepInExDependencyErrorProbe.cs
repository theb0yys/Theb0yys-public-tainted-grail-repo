using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace AvalonExceptions;

internal static class BepInExDependencyErrorProbe
{
    private const int MaxFindings = 200;

    private static readonly Regex MissingDependencyGuidRegex = new Regex(
        @"(?<guid>[A-Za-z][A-Za-z0-9_-]*(?:\.[A-Za-z0-9_-]+)+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex CouldNotLoadMissingDependenciesRegex = new Regex(
        @"Could\s+not\s+load\s+\[(?<display>[^\]]+)\]\s+because\s+it\s+has\s+missing\s+dependencies:\s*(?<missing>[^\r\n]+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex PluginDisplayNameVersionRegex = new Regex(
        @"^(?<name>.+?)\s+(?<version>\d+(?:\.\d+){1,3}(?:[-+][A-Za-z0-9_.-]+)?)$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    internal static BepInExDependencyErrorFinding[] Build(object? dependencyErrors = null)
    {
        object? source = dependencyErrors ?? TryReadChainloaderDependencyErrors();
        if (source == null)
        {
            return Array.Empty<BepInExDependencyErrorFinding>();
        }

        List<BepInExDependencyErrorFinding> findings = new List<BepInExDependencyErrorFinding>();
        AddFindings(findings, source, string.Empty);
        return findings
            .Where(finding => !string.IsNullOrWhiteSpace(finding.Message) || !string.IsNullOrWhiteSpace(finding.PluginGuid))
            .Take(MaxFindings)
            .ToArray();
    }

    private static object? TryReadChainloaderDependencyErrors()
    {
        try
        {
            Type? chainloaderType = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .Select(assembly => assembly.GetType("BepInEx.Bootstrap.Chainloader", throwOnError: false, ignoreCase: false))
                .FirstOrDefault(type => type != null);
            if (chainloaderType == null)
            {
                return null;
            }

            PropertyInfo? property = chainloaderType.GetProperty("DependencyErrors", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (property != null)
            {
                return property.GetValue(null, null);
            }

            FieldInfo? field = chainloaderType.GetField("DependencyErrors", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            return field?.GetValue(null);
        }
        catch
        {
            return null;
        }
    }

    private static void AddFindings(List<BepInExDependencyErrorFinding> findings, object source, string inheritedPluginGuid)
    {
        if (findings.Count >= MaxFindings || source == null)
        {
            return;
        }

        if (source is string text)
        {
            AddFinding(findings, inheritedPluginGuid, text, "dependency-error");
            return;
        }

        if (source is IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                if (findings.Count >= MaxFindings)
                {
                    return;
                }

                string pluginGuid = entry.Key?.ToString() ?? inheritedPluginGuid;
                AddFindings(findings, entry.Value ?? string.Empty, pluginGuid);
            }

            return;
        }

        if (TryReadKeyValuePair(source, out object? key, out object? value))
        {
            string pluginGuid = key?.ToString() ?? inheritedPluginGuid;
            AddFindings(findings, value ?? string.Empty, pluginGuid);
            return;
        }

        if (source is IEnumerable enumerable)
        {
            foreach (object? item in enumerable)
            {
                if (findings.Count >= MaxFindings)
                {
                    return;
                }

                if (item != null)
                {
                    AddFindings(findings, item, inheritedPluginGuid);
                }
            }

            return;
        }

        string message = source.ToString() ?? string.Empty;
        AddFinding(findings, inheritedPluginGuid, message, "dependency-error");
    }

    private static bool TryReadKeyValuePair(object source, out object? key, out object? value)
    {
        key = null;
        value = null;
        try
        {
            Type type = source.GetType();
            PropertyInfo? keyProperty = type.GetProperty("Key", BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo? valueProperty = type.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
            if (keyProperty == null || valueProperty == null)
            {
                return false;
            }

            key = keyProperty.GetValue(source, null);
            value = valueProperty.GetValue(source, null);
            return true;
        }
        catch
        {
            key = null;
            value = null;
            return false;
        }
    }

    private static void AddFinding(List<BepInExDependencyErrorFinding> findings, string pluginGuid, string message, string status)
    {
        if (findings.Count >= MaxFindings || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        BepInExDependencyErrorDetails details = ParseDependencyErrorDetails(pluginGuid, message);
        findings.Add(new BepInExDependencyErrorFinding(
            string.IsNullOrWhiteSpace(pluginGuid) ? string.Empty : pluginGuid.Trim(),
            details.DependentPluginName,
            details.DependentPluginVersion,
            details.MissingDependencyGuid,
            message.Trim(),
            status,
            "Chainloader.DependencyErrors"));
    }

    private static BepInExDependencyErrorDetails ParseDependencyErrorDetails(string pluginGuid, string message)
    {
        string dependentPluginName = string.Empty;
        string dependentPluginVersion = string.Empty;
        string missingDependencyGuid = ExtractMissingDependencyGuid(message);

        Match loadMatch = CouldNotLoadMissingDependenciesRegex.Match(message);
        if (loadMatch.Success)
        {
            missingDependencyGuid = ExtractMissingDependencyGuid(loadMatch.Groups["missing"].Value);
            string display = loadMatch.Groups["display"].Value.Trim();
            Match displayMatch = PluginDisplayNameVersionRegex.Match(display);
            if (displayMatch.Success)
            {
                dependentPluginName = displayMatch.Groups["name"].Value.Trim();
                dependentPluginVersion = displayMatch.Groups["version"].Value.Trim();
            }
            else
            {
                dependentPluginName = display;
            }
        }
        else if (!string.IsNullOrWhiteSpace(pluginGuid))
        {
            dependentPluginName = pluginGuid.Trim();
        }

        return new BepInExDependencyErrorDetails(
            dependentPluginName,
            dependentPluginVersion,
            missingDependencyGuid);
    }

    private static string ExtractMissingDependencyGuid(string text)
    {
        Match match = MissingDependencyGuidRegex.Match(text ?? string.Empty);
        return match.Success ? match.Groups["guid"].Value.Trim() : string.Empty;
    }

    private readonly struct BepInExDependencyErrorDetails
    {
        internal BepInExDependencyErrorDetails(string dependentPluginName, string dependentPluginVersion, string missingDependencyGuid)
        {
            DependentPluginName = dependentPluginName;
            DependentPluginVersion = dependentPluginVersion;
            MissingDependencyGuid = missingDependencyGuid;
        }

        internal string DependentPluginName { get; }
        internal string DependentPluginVersion { get; }
        internal string MissingDependencyGuid { get; }
    }
}

internal readonly struct BepInExDependencyErrorFinding
{
    internal BepInExDependencyErrorFinding(
        string pluginGuid,
        string dependentPluginName,
        string dependentPluginVersion,
        string missingDependencyGuid,
        string message,
        string status,
        string source)
    {
        PluginGuid = pluginGuid;
        DependentPluginName = dependentPluginName;
        DependentPluginVersion = dependentPluginVersion;
        MissingDependencyGuid = missingDependencyGuid;
        Message = message;
        Status = status;
        Source = source;
    }

    internal string PluginGuid { get; }
    internal string DependentPluginName { get; }
    internal string DependentPluginVersion { get; }
    internal string MissingDependencyGuid { get; }
    internal string Message { get; }
    internal string Status { get; }
    internal string Source { get; }

    internal string Describe()
    {
        string dependent = DependentPluginDisplayName();
        if (!string.IsNullOrWhiteSpace(dependent) && !string.IsNullOrWhiteSpace(MissingDependencyGuid))
        {
            return $"{dependent} is missing dependency `{MissingDependencyGuid}`.";
        }

        if (!string.IsNullOrWhiteSpace(dependent))
        {
            return $"{dependent} has a BepInEx loader dependency error.";
        }

        if (!string.IsNullOrWhiteSpace(MissingDependencyGuid))
        {
            return $"BepInEx loader dependency `{MissingDependencyGuid}` is missing.";
        }

        return Message;
    }

    internal string ToSuspectRow()
    {
        return $"BepInEx loader dependency error - high confidence: {Describe()}";
    }

    private string DependentPluginDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(DependentPluginName) && !string.IsNullOrWhiteSpace(DependentPluginVersion))
        {
            return $"{DependentPluginName} {DependentPluginVersion}";
        }

        if (!string.IsNullOrWhiteSpace(DependentPluginName))
        {
            return DependentPluginName;
        }

        return PluginGuid;
    }

    internal static string ToCsv(IEnumerable<BepInExDependencyErrorFinding> findings)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("pluginGuid,dependentPluginName,dependentPluginVersion,missingDependencyGuid,message,status,source");
        foreach (BepInExDependencyErrorFinding finding in findings)
        {
            AppendCsvRow(
                builder,
                finding.PluginGuid,
                finding.DependentPluginName,
                finding.DependentPluginVersion,
                finding.MissingDependencyGuid,
                finding.Message,
                finding.Status,
                finding.Source);
        }

        return builder.ToString();
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
        if (!quote)
        {
            return value;
        }

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
