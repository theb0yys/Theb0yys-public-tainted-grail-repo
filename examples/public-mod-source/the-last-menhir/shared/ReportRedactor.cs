using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AvalonExceptions;

internal sealed class ReportRedactor
{
    private readonly List<(string Prefix, string Replacement)> _knownPaths;
    private int _pathMasks;
    private int _emailMasks;
    private int _ipMasks;
    private int _secretMasks;
    private int _discordMasks;

    private ReportRedactor(List<(string Prefix, string Replacement)> knownPaths)
    {
        _knownPaths = knownPaths;
    }

    internal static ReportRedactor Create(IEnumerable<(string Path, string Replacement)> knownPaths)
    {
        List<(string Prefix, string Replacement)> normalized = new List<(string Prefix, string Replacement)>();
        foreach ((string path, string replacement) in knownPaths)
        {
            AddPath(normalized, path, replacement);
        }

        return new ReportRedactor(normalized);
    }

    internal string Redact(string value)
    {
        return RedactCore(value, countMasks: true);
    }

    internal bool WouldRedact(string value)
    {
        string original = value ?? string.Empty;
        return !string.Equals(original, RedactCore(original, countMasks: false), StringComparison.Ordinal);
    }

    private string RedactCore(string value, bool countMasks)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        string result = value;
        foreach ((string prefix, string replacement) in _knownPaths)
        {
            if (result.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result = Regex.Replace(result, Regex.Escape(prefix), replacement, RegexOptions.IgnoreCase);
                Increment(ref _pathMasks, countMasks);
            }
        }

        result = Regex.Replace(result, @"[A-Z]:\\Users\\[^\\\s]+", match =>
        {
            Increment(ref _pathMasks, countMasks);
            return "<user-profile>";
        }, RegexOptions.IgnoreCase);

        result = Regex.Replace(result, @"[A-Z]:/Users/[^/\s]+", match =>
        {
            Increment(ref _pathMasks, countMasks);
            return "<user-profile>";
        }, RegexOptions.IgnoreCase);

        result = Regex.Replace(result, @"(?<![A-Z0-9])[A-Z]:\\(?:[^\\\r\n]+\\){2,}", match =>
        {
            Increment(ref _pathMasks, countMasks);
            return "<redacted-path>";
        }, RegexOptions.IgnoreCase);

        result = Regex.Replace(result, @"(?<![A-Z0-9])[A-Z]:/(?:[^/\r\n]+/){2,}", match =>
        {
            Increment(ref _pathMasks, countMasks);
            return "<redacted-path>";
        }, RegexOptions.IgnoreCase);

        result = Regex.Replace(result, @"[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}", match =>
        {
            Increment(ref _emailMasks, countMasks);
            return "<redacted-email>";
        }, RegexOptions.IgnoreCase);

        result = Regex.Replace(result, @"\b(?:\d{1,3}\.){3}\d{1,3}\b", match =>
        {
            if (LooksLikeVersionNumber(result, match))
            {
                return match.Value;
            }

            Increment(ref _ipMasks, countMasks);
            return "<redacted-ip>";
        });

        result = Regex.Replace(result, @"(?i)\b(api[_-]?key|token|secret|password)\b\s*[:=]\s*\S+", match =>
        {
            Increment(ref _secretMasks, countMasks);
            return $"{match.Groups[1].Value}=<redacted-secret>";
        });

        result = Regex.Replace(result, @"(?i)\b(?:https?://)?(?:www\.)?(?:discord\.gg|discord\.com/invite)/\S+", match =>
        {
            Increment(ref _discordMasks, countMasks);
            return "<redacted-discord-invite>";
        });

        result = Regex.Replace(result, @"(?<![\w.])@(?!charset\b|container\b|font-face\b|import\b|keyframes\b|layer\b|media\b|namespace\b|page\b|supports\b)[A-Z0-9_.]{2,32}\b", match =>
        {
            Increment(ref _discordMasks, countMasks);
            return "<redacted-discord-handle>";
        }, RegexOptions.IgnoreCase);

        return result;
    }

    internal string BuildReport()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("redactionProfile=default-v0.1");
        builder.AppendLine("redactionApplied=true");
        builder.AppendLine($"pathMasks={_pathMasks}");
        builder.AppendLine($"emailMasks={_emailMasks}");
        builder.AppendLine($"ipMasks={_ipMasks}");
        builder.AppendLine($"secretMasks={_secretMasks}");
        builder.AppendLine($"discordMasks={_discordMasks}");
        builder.AppendLine("excludedByDefault=saves|game-dlls|bepinex-binaries|full-plugin-folders|full-config-folders|minidumps|memory-dumps|network-upload");
        return builder.ToString();
    }

    private static void AddPath(List<(string Prefix, string Replacement)> paths, string value, string replacement)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        try
        {
            string full = Path.GetFullPath(value).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            AddPathVariant(paths, full, replacement);
            AddPathVariant(paths, full.Replace('\\', '/'), replacement);
        }
        catch
        {
            // Ignore invalid path evidence.
        }
    }

    private static void AddPathVariant(List<(string Prefix, string Replacement)> paths, string value, string replacement)
    {
        if (!string.IsNullOrWhiteSpace(value) && paths.All(item => !string.Equals(item.Prefix, value, StringComparison.OrdinalIgnoreCase)))
        {
            paths.Add((value, replacement));
        }
    }

    private static void Increment(ref int counter, bool enabled)
    {
        if (enabled)
        {
            counter++;
        }
    }

    private static bool LooksLikeVersionNumber(string text, Match match)
    {
        int beforeStart = Math.Max(0, match.Index - 24);
        string before = text.Substring(beforeStart, match.Index - beforeStart);
        int afterStart = match.Index + match.Length;
        int afterLength = Math.Min(24, text.Length - afterStart);
        string after = text.Substring(afterStart, afterLength);

        if (before.EndsWith("Version=", StringComparison.OrdinalIgnoreCase)
            || before.EndsWith("version=", StringComparison.OrdinalIgnoreCase)
            || before.EndsWith("<td>", StringComparison.OrdinalIgnoreCase) && after.StartsWith("</td>", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        char previous = match.Index > 0 ? text[match.Index - 1] : '\0';
        int nextIndex = match.Index + match.Length;
        char next = nextIndex < text.Length ? text[nextIndex] : '\0';
        if (previous == ',' && (next == ',' || next == '\r' || next == '\n' || next == '\0'))
        {
            return true;
        }

        return before.IndexOf("Version=", StringComparison.OrdinalIgnoreCase) >= 0
            && after.StartsWith(",", StringComparison.OrdinalIgnoreCase);
    }
}
