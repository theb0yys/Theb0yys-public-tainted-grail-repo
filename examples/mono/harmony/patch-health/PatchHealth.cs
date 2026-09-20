using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace TGCommunity.PatchHealth;

internal sealed class PatchHealthResult
{
    internal PatchHealthResult(bool ready, string reason, MethodBase? target)
    {
        Ready = ready;
        Reason = reason ?? string.Empty;
        Target = target;
    }

    internal bool Ready { get; }
    internal string Reason { get; }
    internal MethodBase? Target { get; }
}

internal static class HarmonyPatchHealth
{
    internal static PatchHealthResult TryInstallPostfix(
        Harmony harmony,
        string ownerId,
        MethodBase? target,
        MethodInfo? postfix)
    {
        if (harmony == null)
        {
            return new PatchHealthResult(false, "harmony-instance-missing", target);
        }

        if (string.IsNullOrWhiteSpace(ownerId))
        {
            return new PatchHealthResult(false, "owner-id-missing", target);
        }

        if (target == null)
        {
            return new PatchHealthResult(false, "target-not-resolved", null);
        }

        if (postfix == null)
        {
            return new PatchHealthResult(false, "postfix-not-resolved", target);
        }

        try
        {
            harmony.Patch(target, postfix: new HarmonyMethod(postfix));

            Patches? info = Harmony.GetPatchInfo(target);
            bool installed = info?.Postfixes.Any(patch =>
                string.Equals(patch.owner, ownerId, StringComparison.Ordinal)) == true;

            if (!installed)
            {
                harmony.Unpatch(target, HarmonyPatchType.All, ownerId);
                return new PatchHealthResult(false, "owner-not-present-after-patch", target);
            }

            return new PatchHealthResult(true, "ready", target);
        }
        catch (Exception ex)
        {
            try
            {
                harmony.Unpatch(target, HarmonyPatchType.All, ownerId);
            }
            catch
            {
                // Preserve the original installation failure.
            }

            return new PatchHealthResult(
                false,
                "patch-exception:" + ex.GetType().FullName,
                target);
        }
    }
}
