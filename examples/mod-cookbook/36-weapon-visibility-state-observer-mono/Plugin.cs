using System;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Items;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace TGExample.WeaponVisibilityStateObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.weapon-visibility-state-observer";
    public const string PluginName = "TG Example - Weapon Visibility/State Observer";
    public const string PluginVersion = "0.1.0";

    private ConfigEntry<float> _pollSeconds = null!;
    private float _nextPoll;
    private string _lastSnapshot = string.Empty;

    private void Awake()
    {
        _pollSeconds = Config.Bind(
            "Diagnostics",
            "PollSeconds",
            0.2f,
            new ConfigDescription(
                "Seconds between weapon-state samples.",
                new AcceptableValueRange<float>(0.1f, 10f)));

        Logger.LogInfo("${PluginName} loaded. Logs are emitted only when the combined read-only state changes.");
    }

    private void Update()
    {
        if (Time.unscaledTime < _nextPoll)
        {
            return;
        }

        _nextPoll = Time.unscaledTime + Mathf.Clamp(_pollSeconds.Value, 0.1f, 10f);

        string snapshot = BuildSnapshot();
        if (string.Equals(snapshot, _lastSnapshot, StringComparison.Ordinal))
        {
            return;
        }

        _lastSnapshot = snapshot;
        Logger.LogInfo("$Weapon state -> {snapshot}");
    }

    private static string BuildSnapshot()
    {
        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            return "hero=unavailable";
        }

        try
        {
            return "$weaponsVisible={hero.WeaponsVisible}; " +
                   "$isWeaponEquipped={hero.IsWeaponEquipped}; " +
                   "$pullingRanged={hero.PullingRangedWeapon}; " +
                   "$tppActive={Hero.TppActive}; " +
                   "$main={ItemName(hero.MainHandItem)}; " +
                   "$off={ItemName(hero.OffHandItem)}";
        }
        catch (Exception ex)
        {
            return "$state-read-failed:{ex.GetType().Name}";
        }
    }

    private static string ItemName(Item? item)
    {
        if (item == null)
        {
            return "none";
        }

        try
        {
            return string.IsNullOrWhiteSpace(item.DisplayName)
                ? item.Template?.ItemName ?? item.GetType().Name
                : item.DisplayName;
        }
        catch
        {
            return item.GetType().Name;
        }
    }
}
