using Tainted.Abstractions.Runtime;

namespace TGTemplate.RichMerchant;

internal static class Feature
{
    internal const string SourceFamily = "rich-merchant";
    internal static readonly string[] Mechanisms =
    {
        "merchant gold floor",
        "dual-runtime build boundary",
        "narrow merchant patch"
    };

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Rich Merchant starter initialized. runtime=" + runtimeKind +
           "; mechanisms=" + string.Join(", ", Mechanisms);
}
