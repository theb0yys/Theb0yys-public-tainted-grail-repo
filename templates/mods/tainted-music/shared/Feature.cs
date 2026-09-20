using Tainted.Abstractions.Runtime;

namespace TGTemplate.TaintedMusic;

internal static class Feature
{
    internal const string SourceFamily = "tainted-music";
    internal static readonly string[] Mechanisms =
    {
        "music arbitration",
        "native music suppression",
        "custom music menu",
        "owned resource loading"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Tainted Music starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
