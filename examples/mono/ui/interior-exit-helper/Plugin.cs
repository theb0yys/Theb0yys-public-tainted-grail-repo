using System;
using Awaken.TG.Main.Heroes;
using Awaken.TG.MVC;
using Awaken.TG.MVC.Domains;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TGCommunity.Example.InteriorExitHelper;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.interior-exit-helper";
    public const string PluginName = "TG Example - Interior Exit Helper";
    public const string PluginVersion = "0.1.0";

    private string _sceneKey = string.Empty;
    private bool _interior;
    private bool _hasEntrance;
    private Vector3 _entrance;
    private float _sceneChangedAt;

    private void Awake()
    {
        _sceneChangedAt = Time.realtimeSinceStartup;
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void Update()
    {
        SceneService? sceneService = null;
        try
        {
            sceneService = World.Services?.TryGet<SceneService>();
        }
        catch
        {
        }

        string sceneName = sceneService?.ActiveSceneRef?.Name;
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            sceneName = SceneManager.GetActiveScene().name;
        }

        bool interior = sceneService != null
            ? !sceneService.IsOpenWorld
            : LooksLikeInterior(sceneName ?? string.Empty);

        string key = $"{sceneName}|{interior}";
        if (!string.Equals(key, _sceneKey, StringComparison.Ordinal))
        {
            _sceneKey = key;
            _interior = interior;
            _hasEntrance = false;
            _sceneChangedAt = Time.realtimeSinceStartup;
        }

        if (!_interior || _hasEntrance || Time.realtimeSinceStartup - _sceneChangedAt < 1.25f)
        {
            return;
        }

        Hero? hero = Hero.Current;
        if (hero != null && IsFinite(hero.Coords))
        {
            _entrance = hero.Coords;
            _hasEntrance = true;
            Logger.LogInfo($"Entrance captured for {sceneName}: {_entrance}.");
        }
    }

    private void OnGUI()
    {
        if (!_interior || !_hasEntrance)
        {
            return;
        }

        Hero? hero = Hero.Current;
        if (hero == null)
        {
            return;
        }

        try
        {
            if (hero.HeroCombat?.IsHeroInFight == true)
            {
                return;
            }
        }
        catch
        {
        }

        Camera? camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        Vector3 point = camera.WorldToScreenPoint(_entrance);
        if (point.z <= 0.01f)
        {
            return;
        }

        float x = Mathf.Clamp(point.x, 80f, Screen.width - 80f);
        float y = Mathf.Clamp(Screen.height - point.y, 80f, Screen.height - 100f);
        float distance = Vector3.Distance(hero.Coords, _entrance);

        GUI.Box(new Rect(x - 55f, y - 22f, 110f, 44f), $"EXIT\n{distance:0}m");
    }

    private static bool IsFinite(Vector3 value)
    {
        return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private static bool LooksLikeInterior(string sceneName)
    {
        string value = sceneName.ToLowerInvariant();
        if (value.Contains("campaignmap") || value.Contains("menu") || value.Contains("loading"))
        {
            return false;
        }

        return value.Contains("dungeon") ||
               value.Contains("cave") ||
               value.Contains("crypt") ||
               value.Contains("tomb") ||
               value.Contains("mine") ||
               value.Contains("cellar") ||
               value.Contains("interior") ||
               value.Contains("prison") ||
               value.Contains("sewer");
    }
}
