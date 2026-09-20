using System;
using Tainted.Abstractions.Runtime;

namespace TGTemplate.RichMerchant;

internal static class Feature
{
    internal const string SourceFamily = "rich-merchant";

    internal static int ResolveMerchantGold(int nativeGold, int minimumGold, bool enabled)
    {
        if (!enabled)
            return nativeGold;

        return Math.Max(nativeGold, Math.Max(0, minimumGold));
    }

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Rich Merchant starter initialized. runtime=" + runtimeKind +
           "; native-trade-flow-preserved";
}
