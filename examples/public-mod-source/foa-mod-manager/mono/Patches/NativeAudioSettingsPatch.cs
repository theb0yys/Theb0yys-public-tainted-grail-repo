using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Awaken.TG.Main.Settings;
using Awaken.TG.Main.Settings.Options;
using Awaken.TG.MVC;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace FoAModManager.Patches;

internal static class NativeAudioSettingsPatch
{
    private const float ProbeIntervalSeconds = 0.5f;
    private static readonly MethodInfo? AddAudioSettingMethod = AccessTools.Method(typeof(SettingsMaster), "AddAudioSetting");
    private static readonly FieldInfo? AudioSettingsField = AccessTools.Field(typeof(SettingsMaster), "_audioSettings");
    private static ManualLogSource? _logger;
    private static float _nextProbeTime;

    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        _logger = logger;
        if (AddAudioSettingMethod == null || AudioSettingsField == null)
        {
            logger.LogWarning("Could not find FoA native audio settings injection targets. Tainted native Audio rows will not be added.");
            return;
        }

        logger.LogInfo("Armed Tainted native Audio settings polling injection.");
    }

    internal static void Tick()
    {
        if (AddAudioSettingMethod == null || AudioSettingsField == null || Time.unscaledTime < _nextProbeTime)
        {
            return;
        }

        _nextProbeTime = Time.unscaledTime + ProbeIntervalSeconds;

        try
        {
            if (!TryGetSettingsMaster(out SettingsMaster? master) || master == null)
            {
                return;
            }

            int added = NativeAudioSettingsBridge.Inject(master, AddAudioSettingMethod, AudioSettingsField, _logger);
            if (added > 0)
            {
                _logger?.LogInfo($"Tainted native Audio settings rows added count={added}.");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"Tainted native Audio settings polling injection failed: {ex}");
        }
    }

    private static bool TryGetSettingsMaster(out SettingsMaster? master)
    {
        try
        {
            master = World.Only<SettingsMaster>();
            return true;
        }
        catch (ArgumentException)
        {
            master = null;
            return false;
        }
    }
}

internal static class NativeAudioSettingsBridge
{
    private static readonly NativeAudioBindingDefinition[] Definitions =
    {
        new("kane.tgfoa.tainted-music", "Audio", "GlobalVolume", "Tainted Music - Master", 0.35f),
        new("kane.tgfoa.tainted-music", "Audio", "WyrdnessVolume", "Tainted Music - Wyrdness", 1f),
        new("kane.tgfoa.tainted-music", "Audio", "DayOpenWorldVolume", "Tainted Music - Day Open World", 0.85f),
        new("kane.tgfoa.tainted-music", "Audio", "InteriorVolume", "Tainted Music - Interior", 0.75f),
        new("kane.tgfoa.tainted-music", "Audio", "SettlementVolume", "Tainted Music - Settlement", 0.75f),
        new("kane.tgfoa.tainted-music", "Audio", "ScaryPlaceVolume", "Tainted Music - Scary Places", 0.8f),
        new("kane.tgfoa.tainted-music", "Context Ducking", "DialogueVolumeMultiplier", "Tainted Music - Dialogue Ducking", 0.25f),
        new("kane.tgfoa.tainted-music", "Context Ducking", "CombatVolumeMultiplier", "Tainted Music - Combat Ducking", 0.55f),
        new(
            "kane.tgfoa.tainted-weather",
            "UserControls",
            "WeatherAudioVolume",
            "Tainted Effects - Weather Master",
            0.55f,
            new NativeAudioConfigKey("WeatherAudio", "MasterVolume")),
        new("kane.tgfoa.tainted-weather", "WeatherAudio", "RainVolumeMultiplier", "Tainted Effects - Weather Rain", 1f),
        new("kane.tgfoa.tainted-weather", "WeatherAudio", "WindVolumeMultiplier", "Tainted Effects - Weather Wind", 1f),
        new("kane.tgfoa.tainted-weather", "WeatherAudio", "WavesVolumeMultiplier", "Tainted Effects - Weather Waves", 1f),
        new("kane.tgfoa.tainted-weather", "WeatherAudio", "ThunderVolumeMultiplier", "Tainted Effects - Weather Thunder", 1f),
        new("kane.tgfoa.tainted-weather", "WeatherAudio", "WyrdVolumeMultiplier", "Tainted Effects - Weather Wyrd", 1f),
        new("kane.tgfoa.immersive-footsteps", "Audio", "GlobalVolume", "Tainted Effects - Footsteps Master", 0.5f),
        new("kane.tgfoa.immersive-footsteps", "Audio", "StealthVolumeMultiplier", "Tainted Effects - Sneak Footsteps", 0.65f),
        new("kane.tgfoa.weapons-magic-sfx", "Audio", "GlobalVolume", "Tainted Effects - Weapon & Spell SFX", 0.75f),
        new("kane.tgfoa.weapons-magic-sfx", "Audio", "WeaponVolume", "Tainted Effects - Weapon SFX", 1f),
        new("kane.tgfoa.weapons-magic-sfx", "Audio", "MagicCastVolume", "Tainted Effects - Spell Cast SFX", 1f),
        new("kane.tgfoa.weapons-magic-sfx", "Audio", "MagicImpactVolume", "Tainted Effects - Spell Impact SFX", 1f),
        new("kane.tgfoa.immersive-water", "Underwater Audio", "UnderwaterVolume", "Tainted Effects - Underwater", 0.35f),
        new("kane.tgfoa.immersive-water", "Underwater Audio", "WaterlineVolumeMultiplier", "Tainted Effects - Waterline", 1f)
    };
    private static readonly HashSet<string> LoggedSkipReasons = new(StringComparer.Ordinal);

    internal static int Inject(
        SettingsMaster master,
        MethodInfo addAudioSettingMethod,
        FieldInfo audioSettingsField,
        ManualLogSource? logger)
    {
        if (master == null)
        {
            return 0;
        }

        if (!TryCollectExistingSettingNames(master, audioSettingsField, out HashSet<string> existingNames) ||
            existingNames.Count == 0)
        {
            return 0;
        }

        int added = 0;
        foreach (NativeAudioBindingDefinition definition in Definitions)
        {
            if (existingNames.Contains(definition.DisplayName))
            {
                continue;
            }

            if (!TryCreateSetting(definition, out NativeConfigAudioSetting? setting, out string reason))
            {
                if (!string.Equals(reason, "plugin-not-loaded", StringComparison.Ordinal) &&
                    LoggedSkipReasons.Add($"{definition.PluginGuid}|{definition.Section}|{definition.Key}|{reason}"))
                {
                    logger?.LogInfo($"Tainted native Audio setting skipped plugin='{definition.PluginGuid}' section='{definition.Section}' key='{definition.Key}' reason='{reason}'.");
                }

                continue;
            }

            addAudioSettingMethod.Invoke(master, new object[] { setting! });
            existingNames.Add(definition.DisplayName);
            added++;
        }

        return added;
    }

    private static bool TryCollectExistingSettingNames(
        SettingsMaster master,
        FieldInfo audioSettingsField,
        out HashSet<string> names)
    {
        names = new HashSet<string>(StringComparer.Ordinal);
        if (audioSettingsField.GetValue(master) is not IEnumerable<ISetting> settings)
        {
            return false;
        }

        foreach (ISetting setting in settings)
        {
            if (!string.IsNullOrWhiteSpace(setting.SettingName))
            {
                names.Add(setting.SettingName);
            }
        }

        return true;
    }

    private static bool TryCreateSetting(
        NativeAudioBindingDefinition definition,
        out NativeConfigAudioSetting? setting,
        out string reason)
    {
        setting = null;
        if (!Chainloader.PluginInfos.TryGetValue(definition.PluginGuid, out PluginInfo pluginInfo) ||
            pluginInfo.Instance == null)
        {
            reason = "plugin-not-loaded";
            return false;
        }

        if (!TryFindEntry(pluginInfo.Instance.Config, definition.Section, definition.Key, out ConfigEntryBase? entry))
        {
            reason = "config-entry-missing";
            return false;
        }

        if (!TryReadFloat(entry!, out float currentValue))
        {
            reason = "config-entry-not-float";
            return false;
        }

        List<ConfigEntryBase> mirrorEntries = new();
        foreach (NativeAudioConfigKey mirror in definition.MirrorKeys)
        {
            if (TryFindEntry(pluginInfo.Instance.Config, mirror.Section, mirror.Key, out ConfigEntryBase? mirrorEntry) &&
                TryReadFloat(mirrorEntry!, out _))
            {
                mirrorEntries.Add(mirrorEntry!);
            }
        }

        setting = new NativeConfigAudioSetting(definition.DisplayName, entry!, mirrorEntries, currentValue, definition.DefaultValue);
        reason = "ready";
        return true;
    }

    private static bool TryFindEntry(ConfigFile config, string section, string key, out ConfigEntryBase? entry)
    {
        var definition = new ConfigDefinition(section, key);
        foreach (ConfigEntryBase candidate in ((IDictionary<ConfigDefinition, ConfigEntryBase>)config).Values)
        {
            if (candidate.Definition.Equals(definition))
            {
                entry = candidate;
                return true;
            }
        }

        entry = null;
        return false;
    }

    private static bool TryReadFloat(ConfigEntryBase entry, out float value)
    {
        object boxed = entry.BoxedValue;
        switch (boxed)
        {
            case float floatValue:
                value = Mathf.Clamp01(floatValue);
                return true;
            case double doubleValue:
                value = Mathf.Clamp01((float)doubleValue);
                return true;
            case int intValue:
                value = Mathf.Clamp01(intValue);
                return true;
            case string stringValue when float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed):
                value = Mathf.Clamp01(parsed);
                return true;
            default:
                value = 0f;
                return false;
        }
    }
}

internal sealed class NativeConfigAudioSetting : Setting
{
    private readonly SliderOption _option;
    private readonly ConfigEntryBase _entry;
    private readonly IReadOnlyList<ConfigEntryBase> _mirrorEntries;

    internal NativeConfigAudioSetting(
        string displayName,
        ConfigEntryBase entry,
        IReadOnlyList<ConfigEntryBase> mirrorEntries,
        float currentValue,
        float defaultValue)
    {
        SettingName = displayName;
        _entry = entry;
        _mirrorEntries = mirrorEntries;
        _option = new SliderOption(
            $"TaintedAudio.{entry.Definition.Section}.{entry.Definition.Key}",
            displayName,
            0f,
            1f,
            false,
            "{0:P0}",
            Mathf.Clamp01(defaultValue),
            false,
            0.05f);
        _option.onChange += SaveVolume;
        _option.Value = Mathf.Clamp01(currentValue);
        _option.Apply();
    }

    public override string SettingName { get; }

    public override IEnumerable<PrefOption> Options => new[] { _option };

    protected override void OnApply()
    {
        SaveVolume(_option.Value);
    }

    private void SaveVolume(float value)
    {
        SaveEntry(_entry, value);
        foreach (ConfigEntryBase mirrorEntry in _mirrorEntries)
        {
            SaveEntry(mirrorEntry, value);
        }
    }

    private static void SaveEntry(ConfigEntryBase entry, float value)
    {
        float clamped = Mathf.Clamp01(value);
        if (entry.SettingType == typeof(float))
        {
            entry.BoxedValue = clamped;
        }
        else if (entry.SettingType == typeof(double))
        {
            entry.BoxedValue = (double)clamped;
        }
        else
        {
            entry.SetSerializedValue(clamped.ToString(CultureInfo.InvariantCulture));
        }

        entry.ConfigFile.Save();
    }
}

internal readonly struct NativeAudioBindingDefinition
{
    internal NativeAudioBindingDefinition(
        string pluginGuid,
        string section,
        string key,
        string displayName,
        float defaultValue,
        params NativeAudioConfigKey[] mirrorKeys)
    {
        PluginGuid = pluginGuid;
        Section = section;
        Key = key;
        DisplayName = displayName;
        DefaultValue = defaultValue;
        MirrorKeys = mirrorKeys ?? Array.Empty<NativeAudioConfigKey>();
    }

    internal string PluginGuid { get; }
    internal string Section { get; }
    internal string Key { get; }
    internal string DisplayName { get; }
    internal float DefaultValue { get; }
    internal IReadOnlyList<NativeAudioConfigKey> MirrorKeys { get; }
}

internal readonly struct NativeAudioConfigKey
{
    internal NativeAudioConfigKey(string section, string key)
    {
        Section = section;
        Key = key;
    }

    internal string Section { get; }
    internal string Key { get; }
}
