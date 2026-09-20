using Tainted.Abstractions.Runtime;

namespace TGTemplate.ImmersiveBackgrounds;

internal static class Feature
{
    internal const string SourceFamily = "immersive-backgrounds";

    internal static bool ShouldReplacePreview(
        bool enabled,
        bool supportedStoryContext,
        bool replacementAvailable)
        => enabled && supportedStoryContext && replacementAvailable;

    internal static bool ShouldKeepNativePreview(
        bool enabled,
        bool supportedStoryContext,
        bool replacementAvailable)
        => !ShouldReplacePreview(enabled, supportedStoryContext, replacementAvailable);

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Immersive Backgrounds starter initialized. runtime=" + runtimeKind +
           "; replacement-requires-supported-context-and-owned-resource";
}
