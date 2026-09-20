using Tainted.Abstractions.Runtime;

namespace TGTemplate.FoaModManager;

internal static class Feature
{
    internal const string SourceFamily = "foa-mod-manager";
    internal static readonly string[] Mechanisms =
    {
        "settings UI",
        "profiles",
        "input/cursor scope",
        "diagnostics and shared mod tools"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "FoA Mod Manager starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
