using Tainted.Abstractions.Runtime;

namespace TGTemplate.WyrdHunt;

internal static class Feature
{
    internal const string SourceFamily = "wyrd-hunt";
    internal static readonly string[] Mechanisms =
    {
        "wyrdness pressure",
        "AI package",
        "encounter lifecycle",
        "progression and UI"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Wyrd Hunt starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
