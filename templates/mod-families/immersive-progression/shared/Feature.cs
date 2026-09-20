using System;
using Tainted.Abstractions.Runtime;

namespace TGTemplate.ImmersiveProgression;

internal static class Feature
{
    internal const string SourceFamily = "immersive-progression";

    internal static int PracticeTokens(float practicedXp, float xpPerToken, int maxTokens)
    {
        if (practicedXp <= 0f || xpPerToken <= 0f || maxTokens <= 0)
            return 0;

        int tokens = (int)(practicedXp / xpPerToken);
        return Math.Min(tokens, maxTokens);
    }

    internal static bool CanSpend(int availablePoints, int cost)
        => cost > 0 && availablePoints >= cost;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Immersive Progression starter initialized. runtime=" + runtimeKind +
           "; sidecar-progression-kept-separate-from-vanilla-save-owner";
}
