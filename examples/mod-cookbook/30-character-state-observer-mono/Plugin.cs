using System;
using Awaken.TG.Main.Heroes;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace TGExample.CharacterStateObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.character-state-observer";
    public const string PluginName = "TG Example - Character State Observer";
    public const string PluginVersion = "0.1.0";

    private ConfigEntry<float> _pollSeconds = null!;
    private float _nextPoll;
    private string _lastSnapshot = string.Empty;

    private void Awake()
    {
        _pollSeconds = Config.Bind(
            "Diagnostics",
            "PollSeconds",
            0.25f,
            new ConfigDescription(
                "Seconds between character-state samples.",
                new AcceptableValueRange<float>(0.1f, 10f)));

        Logger.LogInfo($"{PluginName} loaded. Logs are emitted only when the combined state changes.");
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
        Logger.LogInfo($"Character state -> {snapshot}");
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
            bool crouching = hero.IsCrouching || hero.IsStoryCrouching;
            bool combat = hero.HeroCombat?.IsHeroInFight == true;
            string movement = hero.MovementSystem?.Type.ToString() ?? "None";

            return $"alive={hero.IsAlive}; crouching={crouching}; swimming={hero.IsSwimming}; " +
                   $"mounted={hero.Mounted}; portaling={hero.IsPortaling}; combat={combat}; " +
                   $"performingAction={hero.IsPerformingAction}; movement={movement}";
        }
        catch (Exception ex)
        {
            return $"state-read-failed:{ex.GetType().Name}";
        }
    }
}
