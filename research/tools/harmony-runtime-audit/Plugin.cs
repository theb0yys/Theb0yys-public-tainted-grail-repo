using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGCommunity.HarmonyRuntimeAudit;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.harmony-runtime-audit";
    public const string PluginName = "TG Community Harmony Runtime Audit";
    public const string PluginVersion = "0.1.0";

    private ConfigEntry<bool>? _enabled;
    private ConfigEntry<float>? _delaySeconds;
    private ConfigEntry<string>? _ownerFilter;
    private ConfigEntry<string>? _expectedTargets;

    private IEnumerator Start()
    {
        _enabled = Config.Bind(
            "General",
            "Enabled",
            false,
            "Enable one read-only Harmony patch-table snapshot.");

        _delaySeconds = Config.Bind(
            "General",
            "DelaySeconds",
            2f,
            new ConfigDescription(
                "Delay before taking the snapshot so other plug-ins can install their patches.",
                new AcceptableValueRange<float>(0f, 60f)));

        _ownerFilter = Config.Bind(
            "Filter",
            "OwnerId",
            string.Empty,
            "Optional exact Harmony owner ID. Empty means include all owners.");

        _expectedTargets = Config.Bind(
            "Filter",
            "ExpectedTargets",
            string.Empty,
            "Optional pipe-separated canonical target identities to compare with the live patch table.");

        if (!_enabled.Value)
        {
            Logger.LogInfo("Harmony runtime audit enabled=false.");
            yield break;
        }

        float delay = Math.Max(0f, _delaySeconds.Value);
        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        Audit();
    }

    private void Audit()
    {
        string ownerFilter = (_ownerFilter?.Value ?? string.Empty).Trim();
        string[] expected = ParseExpectedTargets(_expectedTargets?.Value ?? string.Empty);

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

            Logger.LogInfo(
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

        Logger.LogInfo(
            "Harmony runtime audit summary " +
            "ownerFilter=" + (string.IsNullOrWhiteSpace(ownerFilter) ? "<all>" : ownerFilter) +
            " totalPatchedMethods=" + patchedMethods.Length.ToString(CultureInfo.InvariantCulture) +
            " includedMethods=" + includedMethods.ToString(CultureInfo.InvariantCulture) +
            " expectedTargets=" + expected.Length.ToString(CultureInfo.InvariantCulture) +
            " expectedFound=" + foundExpected.Length.ToString(CultureInfo.InvariantCulture) +
            " expectedMissing=" + missingExpected.Length.ToString(CultureInfo.InvariantCulture));

        foreach (string target in missingExpected)
        {
            Logger.LogWarning("Harmony runtime expectedTargetMissing target=" + target);
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

        string result = method is MethodInfo methodInfo
            ? methodInfo.ReturnType.FullName ?? methodInfo.ReturnType.Name
            : "System.Void";

        return assembly + "::" + type + "." + method.Name + "(" + parameters + ")->" + result;
    }
}
