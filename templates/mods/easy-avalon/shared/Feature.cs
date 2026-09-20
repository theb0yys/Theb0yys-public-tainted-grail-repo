using Tainted.Abstractions.Runtime;

namespace TGTemplate.EasyAvalon;

internal static class Feature
{
    internal const string SourceFamily = "easy-avalon";
    internal static readonly string[] Mechanisms =
    {
        "damage scaling",
        "difficulty tuning",
        "narrow Harmony mutation",
        "configuration-first defaults"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Easy Avalon starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
