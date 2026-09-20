using System;
using Tainted.Abstractions.Runtime;

namespace TGTemplate.MerchantStockTweaks;

internal static class Feature
{
    internal const string SourceFamily = "merchant-stock-tweaks";

    internal static int ResolveQuantity(
        int nativeQuantity,
        int additiveQuantity,
        int minimumQuantity,
        bool enabled)
    {
        if (!enabled)
            return nativeQuantity;

        int adjusted = Math.Max(0, nativeQuantity + additiveQuantity);
        return Math.Max(adjusted, Math.Max(0, minimumQuantity));
    }

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Merchant Stock Tweaks starter initialized. runtime=" + runtimeKind +
           "; bounded-stock-quantity-policy-ready";
}
