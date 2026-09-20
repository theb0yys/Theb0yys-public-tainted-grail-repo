using Tainted.Abstractions.Runtime;

namespace TGTemplate.FoaModManager;

internal static class Feature
{
    internal const string SourceFamily = "foa-mod-manager";

    internal static bool CanEditSetting(
        bool managerOpen,
        bool settingVisible,
        bool settingReadOnly,
        bool ownerAvailable)
        => managerOpen && settingVisible && !settingReadOnly && ownerAvailable;

    internal static bool CanOwnInteractiveScope(bool managerOpen, bool scopeAvailable)
        => managerOpen && scopeAvailable;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "FoA Mod Manager starter initialized. runtime=" + runtimeKind +
           "; setting-owner-and-ui-scope-gates-active";
}
