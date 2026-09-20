using Tainted.Abstractions.Runtime;

namespace TGTemplate.TaintedInterface;

internal static class Feature
{
    internal const string SourceFamily = "Tainted Interface";
    internal static readonly string[] Mechanisms =
    {
        "shared UI styling",
        "semantic icon catalogue",
        "custom UI scope",
        "inventory/UI extension surfaces"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Tainted Interface starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
