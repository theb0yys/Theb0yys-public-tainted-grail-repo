using System;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGTemplate.AudioReplacement;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.template.audio-replacement";
    public const string PluginName = "TG Audio Replacement Template";
    public const string PluginVersion = "0.1.0";

    private Harmony? _harmony;
    private ConfigEntry<bool>? _runDemoOnLoad;
    private ReplacementAudio? _replacementAudio;

    internal static ReplacementAudio? Audio => Instance?._replacementAudio;
    internal static Plugin? Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _runDemoOnLoad = Config.Bind("Demo", "RunOnLoad", false, "Run the self-owned audio replacement demonstration.");
        _replacementAudio = new ReplacementAudio(Logger);

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);

        Logger.LogInfo(PluginName + " " + PluginVersion + " loaded.");

        if (_runDemoOnLoad.Value)
        {
            DemoAudioEvent.Play("template-owned");
        }
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        _replacementAudio?.Dispose();
        _replacementAudio = null;
        Instance = null;
    }
}

internal static class DemoAudioEvent
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Play(string category)
    {
        Plugin.Instance?.Logger.LogInfo("Demo native/original audio path ran for category=" + category);
    }
}

[HarmonyPatch(typeof(DemoAudioEvent), nameof(DemoAudioEvent.Play))]
internal static class DemoAudioReplacementPatch
{
    private static bool _failureLogged;

    private static bool Prefix(string category)
    {
        if (!string.Equals(category, "template-owned", StringComparison.Ordinal))
        {
            return true;
        }

        try
        {
            bool replacementStarted = Plugin.Audio?.TryPlayTone() == true;
            return !replacementStarted;
        }
        catch (Exception ex)
        {
            if (!_failureLogged)
            {
                _failureLogged = true;
                Plugin.Instance?.Logger.LogError(
                    "Replacement audio failed; original path will continue. " +
                    ex.GetType().Name + ": " + ex.Message);
            }

            return true;
        }
    }
}

internal sealed class ReplacementAudio : IDisposable
{
    private readonly BepInEx.Logging.ManualLogSource _logger;
    private GameObject? _root;
    private AudioClip? _clip;

    internal ReplacementAudio(BepInEx.Logging.ManualLogSource logger)
    {
        _logger = logger;
    }

    internal bool TryPlayTone()
    {
        EnsureRuntime();

        AudioSource? source = _root?.GetComponent<AudioSource>();
        if (source == null || _clip == null)
        {
            return false;
        }

        source.PlayOneShot(_clip, 0.15f);
        _logger.LogInfo("Replacement audio started; original demo path is suppressed.");
        return true;
    }

    private void EnsureRuntime()
    {
        if (_root != null && _clip != null)
        {
            return;
        }

        _root = new GameObject("TGTemplate_AudioReplacement");
        UnityEngine.Object.DontDestroyOnLoad(_root);
        _root.AddComponent<AudioSource>();

        const int sampleRate = 44100;
        const float seconds = 0.12f;
        int sampleCount = (int)(sampleRate * seconds);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < samples.Length; i++)
        {
            float time = i / (float)sampleRate;
            samples[i] = (float)Math.Sin(2.0 * Math.PI * 440.0 * time) * 0.08f;
        }

        _clip = AudioClip.Create("TGTemplateTone", sampleCount, 1, sampleRate, false);
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
