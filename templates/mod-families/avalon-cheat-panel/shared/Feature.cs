using Tainted.Abstractions.Runtime;

namespace TGTemplate.AvalonCheatPanel;

internal static class Feature
{
    internal const string SourceFamily = "avalon-cheat-panel";

    internal static bool CanExecuteAction(
        bool panelEnabled,
        bool featureEnabled,
        bool safeContext,
        bool explicitConfirmation)
        => panelEnabled && featureEnabled && safeContext && explicitConfirmation;

    internal static float ResolveMultiplier(float configuredValue, float vanillaValue = 1f)
        => configuredValue > 0f ? configuredValue : vanillaValue;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Avalon Cheat Panel starter initialized. runtime=" + runtimeKind +
           "; actions=require-enabled-safe-context-confirmation";
}
