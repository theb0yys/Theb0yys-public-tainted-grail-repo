using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AvalonExceptions;

internal static class ExceptionLogDetector
{
    private static readonly Regex ManagedExceptionTypePattern = new Regex(
        @"\b(?:System\.)?[A-Z][A-Za-z0-9_.]*Exception\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    internal static bool LooksLikeManagedException(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return ManagedExceptionTypePattern.IsMatch(text)
            || text.IndexOf("threw an exception", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("exception has been thrown", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("LoaderException", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    internal static bool TryExtractFirstExceptionBlock(
        IEnumerable<string> lines,
        out string title,
        out string stackTrace)
    {
        return TryExtractExceptionBlock(lines, blockText => true, 40, out title, out stackTrace);
    }

    internal static bool TryExtractFirstMatchingExceptionBlock(
        IEnumerable<string> lines,
        Func<string, bool> blockPredicate,
        out string title,
        out string stackTrace)
    {
        return TryExtractExceptionBlock(lines, blockPredicate, 80, out title, out stackTrace);
    }

    private static bool TryExtractExceptionBlock(
        IEnumerable<string> lines,
        Func<string, bool> blockPredicate,
        int blockLineCount,
        out string title,
        out string stackTrace)
    {
        string[] materialized = lines
            .Where(line => line != null)
            .ToArray();
        Func<string, bool> predicate = blockPredicate ?? (_ => true);

        for (int i = 0; i < materialized.Length; i++)
        {
            if (!LooksLikeManagedException(materialized[i]))
            {
                continue;
            }

            int start = Math.Max(0, i - 3);
            string[] block = materialized
                .Skip(start)
                .Take(Math.Max(1, blockLineCount))
                .ToArray();
            string blockText = string.Join(Environment.NewLine, block);
            if (!predicate(blockText))
            {
                continue;
            }

            title = TrimLogPrefix(materialized[i]);
            stackTrace = blockText;
            return true;
        }

        title = string.Empty;
        stackTrace = string.Empty;
        return false;
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
}
