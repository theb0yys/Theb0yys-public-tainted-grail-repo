using Tainted.Abstractions.Runtime;

namespace TGTemplate.AvalonCheatPanel;

internal static class Feature
{
    internal const string SourceFamily = "avalon-cheat-panel";
    internal static readonly string[] Mechanisms =
    {
        "bounded cheat commands",
        "shared custom UI",
        "action receipts",
        "movement/no-clip prototype boundary"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Avalon Cheat Panel starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
