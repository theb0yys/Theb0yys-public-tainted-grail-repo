using System;
using System.Collections.Generic;

namespace AvalonExceptions;

internal readonly struct IncidentQueueEntry
{
    internal IncidentQueueEntry(string kind, string title, string message, string stackTrace, int duplicateCount)
    {
        Kind = kind ?? string.Empty;
        Title = title ?? string.Empty;
        Message = message ?? string.Empty;
        StackTrace = stackTrace ?? string.Empty;
        DuplicateCount = Math.Max(1, duplicateCount);
    }

    internal string Kind { get; }
    internal string Title { get; }
    internal string Message { get; }
    internal string StackTrace { get; }
    internal int DuplicateCount { get; }

    internal string Text
    {
        get
        {
            return string.Join("\n", new[]
            {
                Kind,
                Title,
                Message,
                StackTrace
            });
        }
    }

    internal string GroupFingerprint
    {
        get
        {
            return IncidentFingerprint.BuildGroup(Kind, Title, Message, StackTrace);
        }
    }
}

internal readonly struct AddressablesQueueSweepResult
{
    internal AddressablesQueueSweepResult(int absorbedDuplicateCount, int[] pendingIndices, string[] pendingGroupFingerprints)
    {
        AbsorbedDuplicateCount = absorbedDuplicateCount;
        PendingIndices = pendingIndices ?? Array.Empty<int>();
        PendingGroupFingerprints = pendingGroupFingerprints ?? Array.Empty<string>();
    }

    internal int AbsorbedDuplicateCount { get; }
    internal int[] PendingIndices { get; }
    internal string[] PendingGroupFingerprints { get; }
    internal bool HasMatches => PendingIndices.Length > 0;
}

internal static class IncidentQueueGrouping
{
    internal static AddressablesQueueSweepResult FindPendingPathlessAddressablesWrappers(
        IReadOnlyList<IncidentQueueEntry> pendingEntries,
        IncidentQueueEntry incoming)
    {
        if (!AddressablesInvalidPathEvidence.TryExtract(incoming.Text, out _))
        {
            return new AddressablesQueueSweepResult(0, Array.Empty<int>(), Array.Empty<string>());
        }

        List<int> pendingIndices = new List<int>();
        List<string> pendingGroups = new List<string>();
        int absorbedDuplicateCount = 0;
        for (int i = pendingEntries.Count - 1; i >= 0; i--)
        {
            IncidentQueueEntry pending = pendingEntries[i];
            if (AddressablesInvalidPathEvidence.TryExtract(pending.Text, out _)
                || !AddressablesInvalidPathEvidence.LooksLikeCascadeWrapper(pending.Text))
            {
                continue;
            }

            pendingIndices.Add(i);
            pendingGroups.Add(pending.GroupFingerprint);
            absorbedDuplicateCount += pending.DuplicateCount;
        }

        return new AddressablesQueueSweepResult(
            absorbedDuplicateCount,
            pendingIndices.ToArray(),
            pendingGroups.ToArray());
    }

    internal static bool IsAddressablesInvalidPathGroup(string groupFingerprint)
    {
        return !string.IsNullOrWhiteSpace(groupFingerprint)
            && groupFingerprint.IndexOf("|addressables-assetbundle-invalid-path|", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    internal static bool IsAddressablesCatalogCorruptGroup(string groupFingerprint)
    {
        return !string.IsNullOrWhiteSpace(groupFingerprint)
            && groupFingerprint.IndexOf($"|{AddressablesCatalogCorruptEvidence.RouteId}", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    internal static bool ShouldBypassManagedIncidentLimit(IncidentQueueEntry incoming)
    {
        string text = incoming.Text;
        return AddressablesInvalidPathEvidence.TryExtract(text, out _)
            || AddressablesCatalogCorruptEvidence.LooksLikeCatalogCorruption(text)
            || ManagedDependencyCascadeEvidence.LooksLikeHighSignalDependencyEvidence(incoming.Kind, text);
    }
}
