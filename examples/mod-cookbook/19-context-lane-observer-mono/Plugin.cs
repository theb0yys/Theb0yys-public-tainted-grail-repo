using System;
using Awaken.TG.Main.Heroes;
using Awaken.TG.MVC;
using Awaken.TG.MVC.Domains;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace TGExample.ContextLaneObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.context-lane-observer";
    public const string PluginName = "TG Example - Context Lane Observer";
    public const string PluginVersion = "0.1.0";

    private ConfigEntry<float> _pollSeconds = null!;
    private float _nextPoll;
    private string _lastLane = string.Empty;

    private void Awake()
    {
        _pollSeconds = Config.Bind("Diagnostics", "PollSeconds", 1f,
            new ConfigDescription("Seconds between context samples.",
                new AcceptableValueRange<float>(0.25f, 10f)));

        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void Update()
    {
        if (Time.unscaledTime < _nextPoll)
        {
            return;
        }

        _nextPoll = Time.unscaledTime + Mathf.Clamp(_pollSeconds.Value, 0.25f, 10f);

        string lane = ResolveLane(out string details);
        if (lane == _lastLane)
        {
            return;
        }

        _lastLane = lane;
        Logger.LogInfo($"Context lane -> {lane}; {details}");
    }

    private static string ResolveLane(out string details)
    {
        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            details = "hero=unavailable";
            return "NoHero";
        }

        if (hero.HeroWyrdNight != null && hero.HeroWyrdNight.IsHeroInWyrdness)
        {
            details = "heroInWyrdness=true";
            return "Wyrdness";
        }

        try
        {
            SceneService scene = World.Services.Get<SceneService>();
            if (scene == null)
            {
                details = "sceneService=null";
                return "Unavailable";
            }

            string name = scene.ActiveSceneRef?.Name ?? scene.ActiveSceneDisplayName ?? "<unknown>";
            details = $"scene={name}; openWorld={scene.IsOpenWorld}";

            return scene.IsOpenWorld ? "OpenWorld" : "Interior";
        }
        catch (Exception ex)
        {
            details = $"scene-read-failed:{ex.GetType().Name}";
            return "Unavailable";
        }
    }
}
