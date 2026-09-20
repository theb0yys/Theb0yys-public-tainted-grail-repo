using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace TGTemplate.Il2CppAudioReplacement;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "community.taintedgrail.template.il2cpp-audio-replacement";
    public const string PluginName = "TG IL2CPP Audio Replacement Template";
    public const string PluginVersion = "0.1.0";

    internal static Plugin? Instance { get; private set; }
    internal static ReplacementAudio? Audio { get; private set; }
    internal static ManualLogSource? LogSource { get; private set; }

    private Harmony? _harmony;

    public override void Load()
    {
        Instance = this;
        LogSource = Log;
        ConfigEntry<bool> runDemo = Config.Bind("Demo", "RunOnLoad", false, "Run the self-owned replacement demonstration.");

        Audio = new ReplacementAudio();
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);

        Log.LogInfo($"{PluginName} loaded.");
        if (runDemo.Value)
            DemoAudioEvent.Play("template-owned");
    }

    public override bool Unload()
    {
        _harmony?.UnpatchSelf();
        _harmony = null;
        Audio?.Dispose();
        Audio = null;
        LogSource = null;
        Instance = null;
        return true;
    }
}

internal static class DemoAudioEvent
{
    internal static void Play(string category)
    {
        Plugin.LogSource?.LogInfo($"Original demo audio path ran. category={category}");
    }
}

[HarmonyPatch(typeof(DemoAudioEvent), nameof(DemoAudioEvent.Play))]
internal static class DemoAudioPatch
{
    private static bool Prefix(string category)
    {
        if (!string.Equals(category, "template-owned", StringComparison.Ordinal))
            return true;

        try
        {
            return Plugin.Audio?.TryPlayTone() != true;
        }
        catch (Exception ex)
        {
            Plugin.LogSource?.LogWarning($"Replacement failed; original continues. {ex.GetType().Name}: {ex.Message}");
            return true;
        }
    }
}

internal sealed class ReplacementAudio : IDisposable
{
    private GameObject? _root;
    private AudioClip? _clip;

    internal bool TryPlayTone()
    {
        EnsureRuntime();
        AudioSource? source = _root?.GetComponent<AudioSource>();
        if (source == null || _clip == null)
            return false;

        source.PlayOneShot(_clip, 0.15f);
        Plugin.LogSource?.LogInfo("Replacement started; original demo path suppressed.");
        return true;
    }

    private void EnsureRuntime()
    {
        if (_root != null && _clip != null)
            return;

        _root = new GameObject("TGTemplate_Il2CppAudioReplacement");
        UnityEngine.Object.DontDestroyOnLoad(_root);
        _root.AddComponent<AudioSource>();

        const int sampleRate = 44100;
        int count = (int)(sampleRate * 0.12f);
        float[] samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            float time = i / (float)sampleRate;
            samples[i] = (float)Math.Sin(2.0 * Math.PI * 440.0 * time) * 0.08f;
        }

        _clip = AudioClip.Create("TGTemplateIl2CppTone", count, 1, sampleRate, false);
        _clip.SetData(samples, 0);
    }

    public void Dispose()
    {
        if (_clip != null)
        {
            UnityEngine.Object.Destroy(_clip);
            _clip = null;
        }

        if (_root != null)
        {
            UnityEngine.Object.Destroy(_root);
            _root = null;
        }
    }
}
