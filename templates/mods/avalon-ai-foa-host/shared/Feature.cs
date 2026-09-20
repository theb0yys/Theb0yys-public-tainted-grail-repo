using Tainted.Abstractions.Runtime;

namespace TGTemplate.AvalonAiFoaHost;

internal static class Feature
{
    internal const string SourceFamily = "avalon-ai-runtime";

    internal static bool CanDispatch(
        bool hostEnabled,
        bool packageActive,
        bool ownerReady,
        bool commandValidated)
        => hostEnabled && packageActive && ownerReady && commandValidated;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Avalon AI FoA Host starter initialized. runtime=" + runtimeKind +
           "; dispatchGate=host+package+owner+validated-command";
}
