using Tainted.Abstractions.Runtime;

namespace TGTemplate.TaintedCore;

internal enum EvidenceTrust
{
    Unknown,
    Observed,
    Verified
}

internal static class Feature
{
    internal const string SourceFamily = "avalon-core";

    internal static bool CanUseForMutation(
        EvidenceTrust trust,
        bool exactScopeMatch,
        bool mutationAuthorized)
        => trust == EvidenceTrust.Verified && exactScopeMatch && mutationAuthorized;

    internal static bool CanUseReadOnly(
        EvidenceTrust trust,
        bool exactScopeMatch)
        => trust != EvidenceTrust.Unknown && exactScopeMatch;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Tainted Core starter initialized. runtime=" + runtimeKind +
           "; evidence-trust-and-scope-gates-ready";
}
