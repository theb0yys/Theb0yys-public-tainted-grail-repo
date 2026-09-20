using Tainted.Abstractions.Runtime;

namespace TGTemplate.DungeonExitMarker;

internal static class Feature
{
    internal const string SourceFamily = "dungeon-exit-helper";
    internal static readonly string[] Mechanisms =
    {
        "dungeon exit discovery",
        "marker presentation",
        "shared UI bridge",
        "scene/context gating"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Dungeon Exit Marker starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
