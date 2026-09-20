using System;
using System.Reflection;
using Awaken.TG.Main.AudioSystem;
using Awaken.TG.Main.Heroes.FootSteps;
using BepInEx;
using BepInEx.Configuration;
using FMODUnity;
using HarmonyLib;
using UnityEngine;

namespace TGExample.FootstepBeep;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.footstep-beep";
    public const string PluginName = "TG Example - Footstep Beep";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<bool> _enabled = null!;
    private Harmony? _harmony;
    private AudioClip? _clip;

    internal static bool Enabled => s_instance?._enabled.Value ?? false;

    private void Awake()
    {
        s_instance = this;
        _enabled = Config.Bind("General", "Enabled", false, "Replace hero footsteps with a generated test beep.");
        _clip = CreateTone();

        _harmony = new Harmony(PluginGuid);

        MethodInfo? target = AccessTools.Method(
            typeof(FMODManager),
            nameof(FMODManager.PlayOneShot),
            new[]
            {
                typeof(EventReference),
                typeof(Vector3),
                typeof(UnityEngine.Object),
                typeof(FMODParameter[])
            });

        MethodInfo? prefix = AccessTools.Method(typeof(FootstepPatch), nameof(FootstepPatch.Prefix));

        if (target == null || prefix == null)
        {
            Logger.LogWarning("Exact FMODManager.PlayOneShot footstep target was not found.");
            return;
        }

        _harmony.Patch(target, prefix: new HarmonyMethod(prefix));
        Logger.LogInfo($"{PluginName} loaded. Enabled={Enabled}");
    }

    internal static bool TryPlay(Vector3 position)
    {
        if (!Enabled || s_instance?._clip == null)
        {
            return false;
        }

        AudioSource.PlayClipAtPoint(s_instance._clip, position, 0.15f);
        return true;
    }

    private static AudioClip CreateTone()
    {
        const int sampleRate = 44100;
        const float seconds = 0.08f;
        int count = (int)(sampleRate * seconds);
        float[] samples = new float[count];

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)sampleRate;
            samples[i] = (float)Math.Sin(2d * Math.PI * 660d * t) * 0.06f;
        }

        AudioClip clip = AudioClip.Create("TGExampleFootstepBeep", count, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();

        if (_clip != null)
        {
            UnityEngine.Object.Destroy(_clip);
            _clip = null;
        }

        s_instance = null;
    }
}

internal static class FootstepPatch
{
    internal static bool Prefix(
        EventReference eventReference,
        Vector3 position,
        UnityEngine.Object debugObject,
        FMODParameter[] parameters)
    {
        if (debugObject is not VHeroFootsteps)
        {
            return true;
        }

        try
        {
            bool replacementStarted = Plugin.TryPlay(position);
            return !replacementStarted;
        }
        catch
        {
            return true;
        }
    }
}
