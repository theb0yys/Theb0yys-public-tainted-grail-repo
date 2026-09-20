using System;
using Tainted.Abstractions.Runtime;

namespace TGTemplate.DungeonExitMarker;

internal static class Feature
{
    internal const string SourceFamily = "dungeon-exit-helper";

    internal static bool ShouldShowMarker(
        bool enabled,
        bool inInterior,
        bool entranceKnown,
        bool inCombat,
        bool hideInCombat)
        => enabled && inInterior && entranceKnown && (!hideInCombat || !inCombat);

    internal static float Distance(float x, float y, float z)
        => (float)Math.Sqrt((x * x) + (y * y) + (z * z));

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Dungeon Exit Marker starter initialized. runtime=" + runtimeKind +
           "; marker=known-interior-entrance-only";
}
