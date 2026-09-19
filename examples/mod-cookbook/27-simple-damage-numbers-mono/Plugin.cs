using System.Collections.Generic;
using System.Globalization;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights.DamageInfo;
using Awaken.TG.Main.Heroes;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGExample.SimpleDamageNumbers;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.simple-damage-numbers";
    public const string PluginName = "TG Example - Simple Damage Numbers";
    public const string PluginVersion = "0.1.0";

    private readonly List<Entry> _entries = new();
    private ConfigEntry<float> _duration = null!;
    private ConfigEntry<int> _maxActive = null!;
    private Harmony? _harmony;

    internal static Plugin? Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _duration = Config.Bind("HUD", "DurationSeconds", 1.1f,
            new ConfigDescription("Seconds each number remains visible.", new AcceptableValueRange<float>(0.25f, 4f)));
        _maxActive = Config.Bind("HUD", "MaximumActiveNumbers", 12,
            new ConfigDescription("Maximum visible damage numbers.", new AcceptableValueRange<int>(1, 40)));

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded.");
    }

    internal void Record(Damage damage)
    {
        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded || !ReferenceEquals(damage.DamageDealerPure, hero)) return;

        float amount = Mathf.Max(0f, damage.Amount);
        if (amount <= 0f) return;

        int max = Mathf.Clamp(_maxActive.Value, 1, 40);
        while (_entries.Count >= max) _entries.RemoveAt(0);

        string text = amount >= 10f
            ? Mathf.RoundToInt(amount).ToString(CultureInfo.InvariantCulture)
            : amount.ToString("0.#", CultureInfo.InvariantCulture);

        _entries.Add(new Entry(text, Time.unscaledTime));
    }

    private void OnGUI()
    {
        float now = Time.unscaledTime;
        float duration = Mathf.Clamp(_duration.Value, 0.25f, 4f);

        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            Entry entry = _entries[i];
            float age = now - entry.Started;
            if (age >= duration)
            {
                _entries.RemoveAt(i);
                continue;
            }

            float progress = Mathf.Clamp01(age / duration);
            float y = Screen.height * 0.42f - i * 22f - progress * 36f;
            GUI.color = new Color(1f, 1f, 1f, 1f - progress);
            GUI.Label(new Rect(Screen.width * 0.5f - 60f, y, 120f, 24f), entry.Text);
        }

        GUI.color = Color.white;
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        Instance = null;
        _entries.Clear();
    }

    private sealed class Entry
    {
        internal string Text { get; }
        internal float Started { get; }

        internal Entry(string text, float started)
        {
            Text = text;
            Started = started;
        }
    }
}

[HarmonyPatch(typeof(HealthElement), nameof(HealthElement.TakeDamage))]
internal static class DamageNumberPatch
{
    private static void Postfix(Damage damage)
    {
        if (damage != null) Plugin.Instance?.Record(damage);
    }
}
