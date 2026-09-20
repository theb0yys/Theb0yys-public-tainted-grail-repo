using Tainted.Abstractions.Runtime;

namespace TGTemplate.TaintedFrameworkConsumer;

internal static class SharedFeature
{
    internal static string Describe(TaintedRuntimeKind runtimeKind)
    {
        return $"Shared feature initialized through Tainted Framework. runtime={runtimeKind}";
    }
}
