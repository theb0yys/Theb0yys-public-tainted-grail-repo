using Tainted.Abstractions.Runtime;

namespace TGTemplate.MerchantStockTweaks;

internal static class Feature
{
    internal const string SourceFamily = "merchant-stock-tweaks";
    internal static readonly string[] Mechanisms =
    {
        "shop-open hook",
        "restock cooldown",
        "restockable-stock filtering",
        "merchant ownership preservation"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Merchant Stock Tweaks starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
