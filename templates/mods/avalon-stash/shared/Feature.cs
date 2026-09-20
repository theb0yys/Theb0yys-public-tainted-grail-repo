using System;
using Tainted.Abstractions.Runtime;

namespace TGTemplate.AvalonStash;

internal static class Feature
{
    internal const string SourceFamily = "Avalon Stash";

    internal static int CombinedCount(int carried, int stash)
        => Math.Max(0, carried) + Math.Max(0, stash);

    internal static string FormatIngredientCount(int carried, int stash, int required)
    {
        int safeCarried = Math.Max(0, carried);
        int safeStash = Math.Max(0, stash);
        int total = safeCarried + safeStash;
        return safeCarried + "+" + safeStash + "=" + total + "/" + Math.Max(0, required);
    }

    internal static bool CanOpenNativeStorage(bool enabled, bool nativeStorageAvailable)
        => enabled && nativeStorageAvailable;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Avalon Stash starter initialized. runtime=" + runtimeKind +
           "; native-storage-ownership-preserved";
}
