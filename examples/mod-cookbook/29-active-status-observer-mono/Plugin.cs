using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Statuses;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace TGExample.ActiveStatusObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.active-status-observer";
    public const string PluginName = "TG Example - Active Status Observer";
    public const string PluginVersion = "0.1.0";

    private readonly HashSet<Status> _known = new(ReferenceStatusComparer.Instance);
    private ConfigEntry<float> _pollSeconds = null!;
    private Hero? _observedHero;
    private bool _initialized;
    private float _nextPoll;

    private void Awake()
    {
        _pollSeconds = Config.Bind(
            "Diagnostics",
            "PollSeconds",
            0.5f,
            new ConfigDescription(
                "Seconds between active-status snapshots.",
                new AcceptableValueRange<float>(0.1f, 10f)));

        Logger.LogInfo($"{PluginName} loaded. This example observes active status membership only.");
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
            if (_observedHero != null || _initialized)
            {
                Logger.LogInfo("Active status observer: hero unavailable; snapshot cleared.");
            }

            _observedHero = null;
            _known.Clear();
            _initialized = false;
            return;
        }

        if (!ReferenceEquals(hero, _observedHero))
        {
            _observedHero = hero;
            _known.Clear();
            _initialized = false;
        }

        HashSet<Status> current = new(ReferenceStatusComparer.Instance);
        try
        {
            foreach (Status status in hero.Statuses.AllStatuses)
            {
                if (status != null)
                {
                    current.Add(status);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Active status snapshot failed: {ex.Message}");
            return;
        }

        if (!_initialized)
        {
            _initialized = true;
            Copy(current, _known);
            Logger.LogInfo($"Active status snapshot initialized. Count={_known.Count}.");

            foreach (Status status in _known)
            {
                Logger.LogInfo($"Active status: {Describe(status)}");
            }

            return;
        }

        foreach (Status status in current)
        {
            if (!_known.Contains(status))
            {
                Logger.LogInfo($"Active status added: {Describe(status)}");
            }
        }

        foreach (Status status in _known)
        {
            if (!current.Contains(status))
            {
                Logger.LogInfo($"Active status removed: {Describe(status)}");
            }
        }

        _known.Clear();
        Copy(current, _known);
    }

    private static void Copy(HashSet<Status> source, HashSet<Status> target)
    {
        foreach (Status status in source)
        {
            target.Add(status);
        }
    }

    private static string Describe(Status status)
    {
        try
        {
            string kind = status.Type?.ToString()
                ?? status.Template?.StatusType?.ToString()
                ?? status.GetType().Name;

            bool positive = status.Type?.IsPositive
                ?? status.Template?.StatusType?.IsPositive
                ?? false;

            return $"{kind}; polarity={(positive ? "positive" : "negative")}";
        }
        catch
        {
            return status.GetType().Name;
        }
    }

    private sealed class ReferenceStatusComparer : IEqualityComparer<Status>
    {
        internal static readonly ReferenceStatusComparer Instance = new();

        public bool Equals(Status? x, Status? y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(Status obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
