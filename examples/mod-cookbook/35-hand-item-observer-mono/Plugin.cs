using System;
using System.Collections.Generic;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Items;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace TGExample.HandItemObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.hand-item-observer";
    public const string PluginName = "TG Example - Main/Off-Hand Observer";
    public const string PluginVersion = "0.1.0";

    private ConfigEntry<float> _pollSeconds = null!;
    private Item? _lastMain;
    private Item? _lastOff;
    private Hero? _lastHero;
    private bool _initialized;
    private float _nextPoll;

    private void Awake()
    {
        _pollSeconds = Config.Bind(
            "Diagnostics",
            "PollSeconds",
            0.25f,
            new ConfigDescription(
                "Seconds between hand-item samples.",
                new AcceptableValueRange<float>(0.1f, 10f)));

        Logger.LogInfo("${PluginName} loaded. Logs are emitted only when main/off-hand item references change.");
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
                Logger.LogInfo("Hand state -> hero unavailable");
            }

            _lastHero = null;
            _lastMain = null;
            _lastOff = null;
            _initialized = false;
            return;
        }

        Item? main = SafeRead(() => hero.MainHandItem);
        Item? off = SafeRead(() => hero.OffHandItem);

        if (!_initialized || !ReferenceEquals(hero, _lastHero))
        {
            _lastHero = hero;
            _lastMain = main;
            _lastOff = off;
            _initialized = true;
            Logger.LogInfo("${DescribePair(main, off)}");
            return;
        }

        if (ReferenceEquals(main, _lastMain) && ReferenceEquals(off, _lastOff))
        {
            return;
        }

        _lastMain = main;
        _lastOff = off;
        Logger.LogInfo("${DescribePair(main, off)}");
    }

    private static string DescribePair(Item? main, Item? off)
    {
        return "$Hand state -> main={Describe(main)}; off={Describe(off)}";
    }

    private static string Describe(Item? item)
    {
        if (item == null)
        {
            return "none";
        }

        try
        {
            ItemTemplate? template = item.Template;
            List<string> flags = new();

            if (template != null)
            {
                AddFlag(flags, SafeBool(() => template.IsWeapon), "weapon");
                AddFlag(flags, SafeBool(() => template.IsMelee), "melee");
                AddFlag(flags, SafeBool(() => template.IsShield), "shield");
                AddFlag(flags, SafeBool(() => template.IsRanged), "ranged");
                AddFlag(flags, SafeBool(() => template.IsMagic), "magic");
            }

            AddFlag(flags, SafeBool(() => item.IsEquipped), "equipped");

            string name = string.IsNullOrWhiteSpace(item.DisplayName)
                ? template?.ItemName ?? item.GetType().Name
                : item.DisplayName;

            return "${name}[{(flags.Count == 0 ? "none" : string.Join("|", flags))}]";
        }
        catch
        {
            return item.GetType().Name;
        }
    }

    private static void AddFlag(List<string> flags, bool enabled, string value)
    {
        if (enabled)
        {
            flags.Add(value);
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

    private static T? SafeRead<T>(Func<T?> read) where T : class
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
}
