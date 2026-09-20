using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Awaken.TG.Main.Heroes;
using Awaken.TG.MVC;
using Awaken.TG.MVC.Domains;
using BepInEx;
using BepInEx.Configuration;
using FMODUnity;
using UnityEngine;

namespace TGCommunity.Example.ContextualMusic;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.contextual-music";
    public const string PluginName = "TG Example - Contextual Music";
    public const string PluginVersion = "0.1.0";

    private ConfigEntry<float> _volume = null!;
    private ConfigEntry<float> _fadeSeconds = null!;
    private ConfigEntry<float> _combatDuck = null!;

    private FMOD.System _core;
    private FMOD.ChannelGroup _master;
    private readonly List<Voice> _voices = new();
    private Lane _activeLane = Lane.None;
    private float _nextContextPoll;
    private bool _runtimeReady;

    private void Awake()
    {
        _volume = Config.Bind("Audio", "Volume", 0.15f, "Volume for this example's own music voices.");
        _fadeSeconds = Config.Bind("Audio", "FadeSeconds", 2f, "Crossfade duration.");
        _combatDuck = Config.Bind("Audio", "CombatDuckMultiplier", 0.45f, "Multiplier applied to this example's music while the hero is in combat.");

        Logger.LogInfo($"{PluginName} loaded. It waits for FoA FMOD runtime and owns only its own voices.");
    }

    private void Update()
    {
        if (!_runtimeReady)
        {
            TryStartRuntime();
            return;
        }

        float now = Time.unscaledTime;
        if (now >= _nextContextPoll)
        {
            _nextContextPoll = now + 0.5f;
            Lane lane = DetermineLane();
            if (lane != _activeLane)
            {
                SwitchLane(lane);
            }

            UpdateTargets();
        }

        UpdateVoices(Time.unscaledDeltaTime);
    }

    private void OnDestroy()
    {
        foreach (Voice voice in _voices)
        {
            StopAndRelease(voice);
        }

        _voices.Clear();
        _runtimeReady = false;
    }

    private void TryStartRuntime()
    {
        if (!RuntimeManager.IsInitialized)
        {
            return;
        }

        try
        {
            _core = RuntimeManager.CoreSystem;
            RequireOk(_core.getMasterChannelGroup(out _master), "get master channel group");
            _runtimeReady = true;
            Logger.LogInfo("FMOD Core runtime ready.");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"FMOD startup failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private Lane DetermineLane()
    {
        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            return Lane.None;
        }

        try
        {
            SceneService? scenes = World.Services?.TryGet<SceneService>();
            if (scenes != null)
            {
                return scenes.IsOpenWorld ? Lane.OpenWorld : Lane.Interior;
            }
        }
        catch
        {
        }

        return Lane.OpenWorld;
    }

    private void SwitchLane(Lane lane)
    {
        _activeLane = lane;

        foreach (Voice voice in _voices)
        {
            voice.Target = 0f;
            voice.StopWhenSilent = true;
        }

        if (lane == Lane.None)
        {
            return;
        }

        float frequency = lane == Lane.OpenWorld ? 220f : 330f;
        byte[] wav = BuildToneWav(frequency, 4f, 44100);

        FMOD.CREATESOUNDEXINFO info = new FMOD.CREATESOUNDEXINFO
        {
            cbsize = Marshal.SizeOf(typeof(FMOD.CREATESOUNDEXINFO)),
            length = checked((uint)wav.Length),
            suggestedsoundtype = FMOD.SOUND_TYPE.WAV
        };

        FMOD.MODE mode =
            FMOD.MODE.OPENMEMORY |
            FMOD.MODE.CREATESAMPLE |
            FMOD.MODE.LOOP_NORMAL |
            FMOD.MODE._2D;

        RequireOk(_core.createSound(wav, mode, ref info, out FMOD.Sound sound), "create sound");
        RequireOk(_core.playSound(sound, _master, true, out FMOD.Channel channel), "play sound");
        RequireOk(channel.setVolume(0f), "set initial volume");
        RequireOk(channel.setPaused(false), "unpause channel");

        _voices.Add(new Voice(lane, channel, sound));
        UpdateTargets();
        Logger.LogInfo($"Context lane switched to {lane}.");
    }

    private void UpdateTargets()
    {
        float duck = 1f;
        try
        {
            if (Hero.Current?.HeroCombat?.IsHeroInFight == true)
            {
                duck = Mathf.Clamp(_combatDuck.Value, 0f, 1f);
            }
        }
        catch
        {
        }

        float target = Mathf.Clamp01(_volume.Value) * duck;

        foreach (Voice voice in _voices)
        {
            if (!voice.StopWhenSilent)
            {
                voice.Target = voice.Lane == _activeLane ? target : 0f;
            }
        }
    }

    private void UpdateVoices(float delta)
    {
        float fade = Mathf.Clamp(_fadeSeconds.Value, 0.1f, 30f);
        float step = delta <= 0f ? 1f : delta / fade;

        for (int i = _voices.Count - 1; i >= 0; i--)
        {
            Voice voice = _voices[i];

            if (!voice.Channel.hasHandle())
            {
                ReleaseSound(voice.Sound);
                _voices.RemoveAt(i);
                continue;
            }

            voice.Volume = Mathf.MoveTowards(voice.Volume, voice.Target, step);
            voice.Channel.setVolume(voice.Volume);

            if (voice.StopWhenSilent && voice.Volume <= 0.001f)
            {
                StopAndRelease(voice);
                _voices.RemoveAt(i);
            }
        }
    }

    private static void StopAndRelease(Voice voice)
    {
        try
        {
            if (voice.Channel.hasHandle())
            {
                voice.Channel.stop();
            }
        }
        catch
        {
        }

        ReleaseSound(voice.Sound);
    }

    private static void ReleaseSound(FMOD.Sound sound)
    {
        try
        {
            if (sound.hasHandle())
            {
                sound.release();
            }
        }
        catch
        {
        }
    }

    private static void RequireOk(FMOD.RESULT result, string action)
    {
        if (result != FMOD.RESULT.OK)
        {
            throw new InvalidOperationException($"FMOD failed to {action}: {result}");
        }
    }

    private static byte[] BuildToneWav(float frequency, float seconds, int sampleRate)
    {
        int sampleCount = Math.Max(1, (int)(seconds * sampleRate));
        const short channels = 1;
        const short bits = 16;
        int dataBytes = sampleCount * channels * (bits / 8);

        using var stream = new MemoryStream(44 + dataBytes);
        using var writer = new BinaryWriter(stream);

        writer.Write(new[] { 'R', 'I', 'F', 'F' });
        writer.Write(36 + dataBytes);
        writer.Write(new[] { 'W', 'A', 'V', 'E' });
        writer.Write(new[] { 'f', 'm', 't', ' ' });
        writer.Write(16);
        writer.Write((short)1);
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * (bits / 8));
        writer.Write((short)(channels * (bits / 8)));
        writer.Write(bits);
        writer.Write(new[] { 'd', 'a', 't', 'a' });
        writer.Write(dataBytes);

        for (int i = 0; i < sampleCount; i++)
        {
            double t = i / (double)sampleRate;
            double envelope = 0.18;
            short sample = (short)(Math.Sin(t * Math.PI * 2.0 * frequency) * short.MaxValue * envelope);
            writer.Write(sample);
        }

        writer.Flush();
        return stream.ToArray();
    }

    private sealed class Voice
    {
        internal Voice(Lane lane, FMOD.Channel channel, FMOD.Sound sound)
        {
            Lane = lane;
            Channel = channel;
            Sound = sound;
        }

        internal Lane Lane { get; }
        internal FMOD.Channel Channel { get; }
        internal FMOD.Sound Sound { get; }
        internal float Volume { get; set; }
        internal float Target { get; set; }
        internal bool StopWhenSilent { get; set; }
    }

    private enum Lane
    {
        None,
        OpenWorld,
        Interior
    }
}
