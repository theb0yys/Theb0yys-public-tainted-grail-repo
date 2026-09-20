using Tainted.Abstractions.Runtime;

namespace TGTemplate.TaintedDiagnosticTool;

internal static class Feature
{
    internal const string SourceFamily = "Tainted-Diagnostic Tool";
    internal static readonly string[] Mechanisms =
    {
        "runtime dumps",
        "template diagnostics",
        "evidence receipts",
        "framework runtime reports"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Tainted Diagnostic Tool starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
