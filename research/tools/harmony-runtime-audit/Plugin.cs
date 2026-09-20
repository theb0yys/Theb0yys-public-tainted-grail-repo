using System;
using System.Collections;
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
        _enabled = Config.Bind("General", "Enabled", false, "Enable one read-only Harmony patch-table snapshot.");
        _delaySeconds = Config.Bind(
            "General",
            "DelaySeconds",
            2f,
            new ConfigDescription(
                "Delay before taking the snapshot so other plug-ins can install their patches.",
                new AcceptableValueRange<float>(0f, 60f)));
        _ownerFilter = Config.Bind("Filter", "OwnerId", string.Empty, "Optional exact Harmony owner ID. Empty means include all owners.");
        _expectedTargets = Config.Bind("Filter", "ExpectedTargets", string.Empty, "Optional pipe-separated canonical target identities to compare with the live patch table.");

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

        HarmonyRuntimeAuditEngine.Run(
            Logger,
            _ownerFilter?.Value ?? string.Empty,
            _expectedTargets?.Value ?? string.Empty);
    }
}
