using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;

namespace TGCommunity.HarmonyRuntimeAudit;

internal static class HarmonyRuntimeAuditEngine
{
    internal static void Run(
        ManualLogSource logger,
        string ownerFilterValue,
        string expectedTargetsValue)
    {
        string ownerFilter = (ownerFilterValue ?? string.Empty).Trim();
        string[] expected = ParseExpectedTargets(expectedTargetsValue ?? string.Empty);

        MethodBase[] patchedMethods = Harmony
            .GetAllPatchedMethods()
            .OrderBy(TargetFormatter.Format, StringComparer.Ordinal)
            .ToArray();

        var matchedTargets = new HashSet<string>(StringComparer.Ordinal);
        int includedMethods = 0;

        foreach (MethodBase method in patchedMethods)
        {
            Patches? info = Harmony.GetPatchInfo(method);
            if (info == null)
            {
                continue;
            }

            string[] owners = GetOwners(info);
            if (!string.IsNullOrWhiteSpace(ownerFilter) &&
                !owners.Contains(ownerFilter, StringComparer.Ordinal))
            {
                continue;
            }

            includedMethods++;
            string target = TargetFormatter.Format(method);
            matchedTargets.Add(target);

            logger.LogInfo(
                "Harmony runtime patch " +
                "target=" + target +
                " owners=" + FormatList(owners) +
                " prefixes=" + FormatList(GetPatchOwners(info.Prefixes)) +
                " postfixes=" + FormatList(GetPatchOwners(info.Postfixes)) +
                " transpilers=" + FormatList(GetPatchOwners(info.Transpilers)) +
                " finalizers=" + FormatList(GetPatchOwners(info.Finalizers)));
        }

        string[] missingExpected = expected
            .Where(target => !matchedTargets.Contains(target))
            .OrderBy(target => target, StringComparer.Ordinal)
            .ToArray();

        string[] foundExpected = expected
            .Where(target => matchedTargets.Contains(target))
            .OrderBy(target => target, StringComparer.Ordinal)
            .ToArray();

        logger.LogInfo(
            "Harmony runtime audit summary " +
            "ownerFilter=" + (string.IsNullOrWhiteSpace(ownerFilter) ? "<all>" : ownerFilter) +
            " totalPatchedMethods=" + patchedMethods.Length.ToString(CultureInfo.InvariantCulture) +
            " includedMethods=" + includedMethods.ToString(CultureInfo.InvariantCulture) +
            " expectedTargets=" + expected.Length.ToString(CultureInfo.InvariantCulture) +
            " expectedFound=" + foundExpected.Length.ToString(CultureInfo.InvariantCulture) +
            " expectedMissing=" + missingExpected.Length.ToString(CultureInfo.InvariantCulture));

        foreach (string target in missingExpected)
        {
            logger.LogWarning("Harmony runtime expectedTargetMissing target=" + target);
        }
    }

    private static string[] ParseExpectedTargets(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<string>();
        }

        return value
            .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(item => item.Trim())
            .Where(item => item.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
    }

    private static string[] GetOwners(Patches patches)
    {
        return GetPatchOwners(patches.Prefixes)
            .Concat(GetPatchOwners(patches.Postfixes))
            .Concat(GetPatchOwners(patches.Transpilers))
            .Concat(GetPatchOwners(patches.Finalizers))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(owner => owner, StringComparer.Ordinal)
            .ToArray();
    }

    private static string[] GetPatchOwners(IEnumerable<Patch> patches)
    {
        return patches
            .Select(patch => patch.owner ?? string.Empty)
            .Where(owner => owner.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(owner => owner, StringComparer.Ordinal)
            .ToArray();
    }

    private static string FormatList(IEnumerable<string> values)
    {
        string[] materialized = values.ToArray();
        return materialized.Length == 0
            ? "[]"
            : "[" + string.Join(",", materialized) + "]";
    }
}

internal static class TargetFormatter
{
    internal static string Format(MethodBase method)
    {
        string assembly = method.DeclaringType?.Assembly.GetName().Name ?? "<unknown-assembly>";
        string type = method.DeclaringType?.FullName ?? "<unknown-type>";
        string parameters = string.Join(
            ",",
            method.GetParameters().Select(parameter =>
                parameter.ParameterType.FullName ?? parameter.ParameterType.Name));

        return assembly + "::" + type + "." + method.Name + "(" + parameters + ")";
    }
}
