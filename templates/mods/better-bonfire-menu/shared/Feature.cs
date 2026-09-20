using Tainted.Abstractions.Runtime;

namespace TGTemplate.BetterBonfireMenu;

internal static class Feature
{
    internal const string SourceFamily = "better-bonfire-menu";
    internal static readonly string[] Mechanisms =
    {
        "bonfire services menu",
        "native submenu integration",
        "stash/craft/merchant service bridges",
        "shared UI"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Better Bonfire Menu starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
