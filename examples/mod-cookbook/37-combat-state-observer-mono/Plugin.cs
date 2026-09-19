using System;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Items;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace TGExample.CombatStateObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.combat-state-observer";
    public const string PluginName = "TG Example - Combat State Observer";
    public const string PluginVersion = "0.1.0";

    private ConfigEntry<float> _pollSeconds = null!;
    private Hero? _observedHero;
    private bool _initialized;
    private bool _lastCombat;
    private float _nextPoll;

    private void Awake()
    {
        _pollSeconds = Config.Bind(
            "Diagnostics",
            "PollSeconds",
            0.25f,
            new ConfigDescription(
                "Seconds between combat-state samples.",
                new AcceptableValueRange<float>(0.1f, 10f)));

        Logger.LogInfo("${PluginName} loaded. Logs are emitted only when native combat state changes.");
    }

    private void Update()
    {
        if (Time.unscaledTime < _nextPoll)
        {
            return;
        }

        _nextPoll = Time.unscaledTime + Mathf.Clamp(_pollSeconds.Value, 0.1f, 10f);

        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            if (_initialized)
            {
                Logger.LogInfo("Combat state -> hero unavailable");
            }

            _observedHero = null;
            _initialized = false;
            return;
        }

        bool inCombat;
        try
        {
            inCombat = hero.HeroCombat?.IsHeroInFight == true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning("$Combat-state read failed: {ex.Message}");
            return;
        }

        if (!_initialized || !ReferenceEquals(hero, _observedHero))
        {
            _observedHero = hero;
            _lastCombat = inCombat;
            _initialized = true;
            LogState(hero, inCombat, "initial");
            return;
        }

        if (inCombat == _lastCombat)
        {
            return;
        }

        _lastCombat = inCombat;
        LogState(hero, inCombat, inCombat ? "entered" : "exited");
    }

    private void LogState(Hero hero, bool inCombat, string transition)
    {
        Logger.LogInfo(
            "$Combat state -> transition={transition}; inCombat={inCombat}; " +
            "$weaponsVisible={SafeBool(() => hero.WeaponsVisible)}; " +
            "$main={ItemName(SafeItem(() => hero.MainHandItem))}; " +
            "$off={ItemName(SafeItem(() => hero.OffHandItem))}");
    }

    private static Item? SafeItem(Func<Item?> read)
    {
        try
        {
            return read();
        }
        catch
        {
            return null;
        }
    }

    private static bool SafeBool(Func<bool> read)
    {
        try
        {
            return read();
        }
        catch
        {
            return false;
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
