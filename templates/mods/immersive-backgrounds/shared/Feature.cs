using Tainted.Abstractions.Runtime;

namespace TGTemplate.ImmersiveBackgrounds;

internal static class Feature
{
    internal const string SourceFamily = "immersive-backgrounds";
    internal static readonly string[] Mechanisms =
    {
        "choice background replacement",
        "choice preview ownership",
        "item-set visual context",
        "title-screen integration"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Immersive Backgrounds starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
