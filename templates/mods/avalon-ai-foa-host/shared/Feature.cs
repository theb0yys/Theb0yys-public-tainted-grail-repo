using Tainted.Abstractions.Runtime;

namespace TGTemplate.AvalonAiFoaHost;

internal static class Feature
{
    internal const string SourceFamily = "avalon-ai-runtime";
    internal static readonly string[] Mechanisms =
    {
        "FoA runtime host",
        "observation bridge",
        "command execution boundary",
        "blackboard/planning integration"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Avalon AI FoA Host starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
