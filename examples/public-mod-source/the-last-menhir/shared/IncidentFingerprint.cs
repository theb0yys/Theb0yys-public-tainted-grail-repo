using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AvalonExceptions;

internal static class IncidentFingerprint
{
    private static readonly Regex ManagedExceptionLinePattern = new Regex(
        @"\b(?:System\.)?[A-Z][A-Za-z0-9_.]*Exception\b[^\r\n]*",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex IlOffsetPattern = new Regex(
        @"\s+\(at <[^>]+>:\d+\)\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    internal static string Build(string kind, string title, string message, string stackTrace)
    {
        string exceptionLine = FirstExceptionLine(title, message, stackTrace);
        string stackFrame = FirstStackFrame(stackTrace);
        string evidence = string.IsNullOrWhiteSpace(exceptionLine)
            ? FirstNonEmptyLine(title, message, stackTrace)
            : exceptionLine;

        if (!string.IsNullOrWhiteSpace(stackFrame))
        {
            evidence = string.IsNullOrWhiteSpace(evidence)
                ? stackFrame
                : $"{evidence}|{stackFrame}";
        }

        return $"{kind}|{evidence}";
    }

    internal static string BuildGroup(string kind, string title, string message, string stackTrace)
    {
        string exact = Build(kind, title, message, stackTrace);
        if (!string.Equals(kind, "managed_exception", StringComparison.OrdinalIgnoreCase))
        {
            return exact;
        }

        string text = string.Join("\n", new[]
        {
            title ?? string.Empty,
            message ?? string.Empty,
            stackTrace ?? string.Empty
        });
        if (AddressablesInvalidPathEvidence.TryExtract(text, out AddressablesInvalidPathFinding addressablesFinding))
        {
            return $"{kind}|addressables-assetbundle-invalid-path|{addressablesFinding.GroupKey}";
        }

        if (AddressablesCatalogCorruptEvidence.LooksLikeCatalogCorruption(text))
        {
            return $"{kind}|{AddressablesCatalogCorruptEvidence.RouteId}";
        }

        if (ManagedDependencyCascadeEvidence.LooksLikeEcsFollowOn(text))
        {
            return $"{kind}|{ManagedDependencyCascadeEvidence.FollowOnRouteId}|ecs-scenedisable-rendering";
        }

        bool startupCascade = text.IndexOf("Unity.Entities", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("Awaken.ECS", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("TypeInitializationException", StringComparison.OrdinalIgnoreCase) >= 0;
        if (!startupCascade)
        {
            return exact;
        }

        string exceptionLine = FirstExceptionLine(title ?? string.Empty, message ?? string.Empty, stackTrace ?? string.Empty);
        return string.IsNullOrWhiteSpace(exceptionLine)
            ? exact
            : $"{kind ?? string.Empty}|startup-cascade|{exceptionLine}";
    }

    private static string FirstExceptionLine(params string[] values)
    {
        foreach (string line in Lines(values))
        {
            Match match = ManagedExceptionLinePattern.Match(TrimLogPrefix(line));
            if (match.Success)
            {
                return NormalizeWhitespace(match.Value);
            }
        }

        return string.Empty;
    }

    private static string FirstStackFrame(string stackTrace)
    {
        foreach (string line in Lines(stackTrace))
        {
            string value = TrimLogPrefix(line);
            if (string.IsNullOrWhiteSpace(value)
                || value.Equals("Stack trace:", StringComparison.OrdinalIgnoreCase)
                || ManagedExceptionLinePattern.IsMatch(value))
            {
                continue;
            }

            if (value.IndexOf('(') < 0 && value.IndexOf(" at ", StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            return NormalizeWhitespace(IlOffsetPattern.Replace(value, string.Empty));
        }

        return string.Empty;
    }

    private static string FirstNonEmptyLine(params string[] values)
    {
        foreach (string line in Lines(values))
        {
            string value = NormalizeWhitespace(TrimLogPrefix(line));
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static IEnumerable<string> Lines(params string[] values)
    {
        foreach (string value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            foreach (string line in value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                yield return line;
            }
        }
    }

    private static string TrimLogPrefix(string line)
    {
        string value = line.Trim();
        int closeBracket = value.IndexOf(']');
        if (closeBracket >= 0 && closeBracket + 1 < value.Length)
        {
            value = value.Substring(closeBracket + 1).TrimStart();
        }

        return value;
    }

    private static string NormalizeWhitespace(string value)
    {
        return Regex.Replace(value.Trim(), @"\s+", " ", RegexOptions.CultureInvariant);
    }
}
