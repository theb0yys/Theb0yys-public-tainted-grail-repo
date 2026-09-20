using System;
using System.Collections.Generic;
using Tainted.Abstractions.Runtime;

namespace TGTemplate.TaintedInterface;

internal static class Feature
{
    internal const string SourceFamily = "Tainted Interface";

    private static readonly Dictionary<string, string> SemanticAssets =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    internal static void RegisterSemanticAsset(string semanticId, string resourceId)
    {
        if (string.IsNullOrWhiteSpace(semanticId) || string.IsNullOrWhiteSpace(resourceId))
            return;

        SemanticAssets[semanticId.Trim()] = resourceId.Trim();
    }

    internal static bool TryResolveSemanticAsset(string semanticId, out string resourceId)
        => SemanticAssets.TryGetValue(semanticId ?? string.Empty, out resourceId!);

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Tainted Interface starter initialized. runtime=" + runtimeKind +
           "; shared-presentation-resource-registry-ready";
}
