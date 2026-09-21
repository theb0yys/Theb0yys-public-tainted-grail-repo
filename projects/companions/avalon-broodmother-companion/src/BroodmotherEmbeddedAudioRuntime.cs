using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using BepInEx.Logging;
using FMODUnity;
using UnityEngine;

namespace AvalonBroodmotherCompanion;

internal enum BroodmotherEmbeddedAudioCue
{
    Call,
    Attack,
    Walk,
}

internal sealed class BroodmotherEmbeddedAudioRuntime : IDisposable
{
    private const string ResourcePrefix = "AvalonBroodmotherCompanion.Audio.Spider.";
    private const int MaximumResourceBytes = 8 * 1024 * 1024;
    private const int MaximumUniqueSounds = 16;
    private const int MaximumConcurrentVoices = 8;

    private static readonly EmbeddedSoundResource[] EmbeddedResources =
    {
        new(BroodmotherEmbeddedAudioCue.Walk, "dragon-studio-giant-spider-walking-511319.wav"),
        new(BroodmotherEmbeddedAudioCue.Call, "freesound_community-spider-call-71921.wav"),
        new(BroodmotherEmbeddedAudioCue.Attack, "yodguard-spider-attack-1-482542.wav"),
        new(BroodmotherEmbeddedAudioCue.Attack, "yodguard-spider-attack-2-482539.wav"),
        new(BroodmotherEmbeddedAudioCue.Attack, "yodguard-spider-attack-4-482536.wav"),
        new(BroodmotherEmbeddedAudioCue.Attack, "yodguard-spider-attack-5-482537.wav"),
        new(BroodmotherEmbeddedAudioCue.Attack, "yodguard-spider-attack-6-482535.wav"),
        new(BroodmotherEmbeddedAudioCue.Attack, "yodguard-spider-attack-7-482538.wav"),
        new(BroodmotherEmbeddedAudioCue.Attack, "yodguard-spider-attack-8-482541.wav"),
    };

    private readonly ManualLogSource _logger;
    private readonly Dictionary<BroodmotherEmbeddedAudioCue, List<LoadedSound>> _soundsByCue = new();
    private readonly Dictionary<BroodmotherEmbeddedAudioCue, float> _nextCueAllowedAt = new();
    private readonly List<FMOD.Sound> _ownedSounds = new();
    private readonly List<ActiveVoice> _activeVoices = new();
    private FMOD.System _coreSystem;
    private FMOD.ChannelGroup _masterChannelGroup;
    private bool _loaded;
    private bool _terminalLoadFailure;
    private bool _capacityWarningLogged;
    private bool _disposed;
    private int _playbackFailureWarnings;
    private float _nextStartupLogTime;

    internal BroodmotherEmbeddedAudioRuntime(ManualLogSource logger)
    {
        _logger = logger;
    }

    internal int LoadedSoundCount { get; private set; }

    internal void Tick()
    {
        if (_disposed)
        {
            return;
        }

        EnsureLoaded();
        UpdatePauseAndLifetime();
    }

    internal void Play(
        BroodmotherEmbeddedAudioCue cue,
        Transform? followTransform,
        Vector3 fallbackPosition,
        float volume,
        float minDistance,
        float maxDistance,
        float cooldownSeconds,
        string source)
    {
        if (_disposed || Time.timeScale <= 0f || !EnsureLoaded())
        {
            return;
        }

        if (!_soundsByCue.TryGetValue(cue, out List<LoadedSound>? sounds) || sounds.Count == 0)
        {
            return;
        }

        float now = Time.unscaledTime;
        if (_nextCueAllowedAt.TryGetValue(cue, out float nextAllowedAt) && now < nextAllowedAt)
        {
            return;
        }
        _nextCueAllowedAt[cue] = now + Math.Max(0f, cooldownSeconds);

        PruneFinishedVoices();
        if (_activeVoices.Count >= MaximumConcurrentVoices)
        {
            if (!_capacityWarningLogged)
            {
                _capacityWarningLogged = true;
                _logger.LogWarning($"{Plugin.PluginName} embedded spider audio voice cap {MaximumConcurrentVoices} reached; cue={cue}; source={source}.");
            }
            return;
        }

        LoadedSound sound = sounds[UnityEngine.Random.Range(0, sounds.Count)];
        FMOD.Channel channel = default;
        try
        {
            RequireFmodOk(
                _coreSystem.playSound(sound.Sound, _masterChannelGroup, true, out channel),
                $"start paused embedded spider sound '{sound.Name}'");
            if (!channel.hasHandle())
            {
                throw new InvalidOperationException($"FMOD returned no channel for embedded spider sound '{sound.Name}'.");
            }

            Vector3 position = followTransform != null ? followTransform.position : fallbackPosition;
            FMOD.VECTOR fmodPosition = RuntimeUtils.ToFMODVector(position);
            FMOD.VECTOR velocity = default;
            RequireFmodOk(channel.set3DAttributes(ref fmodPosition, ref velocity), "set embedded spider audio position");
            RequireFmodOk(
                channel.set3DMinMaxDistance(Math.Max(0.1f, minDistance), Math.Max(minDistance + 0.1f, maxDistance)),
                "set embedded spider audio attenuation distances");
            RequireFmodOk(channel.setVolume(Mathf.Clamp01(volume)), "set embedded spider audio volume");
            RequireFmodOk(channel.setPaused(false), "unpause embedded spider audio");
            RequireFmodOk(channel.isPlaying(out bool isPlaying), "confirm embedded spider audio playback");
            if (!isPlaying)
            {
                throw new InvalidOperationException($"FMOD did not keep embedded spider sound '{sound.Name}' playing.");
            }

            _activeVoices.Add(new ActiveVoice(channel, followTransform));
            _logger.LogDebug($"{Plugin.PluginName} played embedded spider audio cue={cue}; sound={sound.Name}; source={source}.");
        }
        catch (Exception ex)
        {
            StopChannelQuietly(channel);
            WarnPlaybackFailure($"{Plugin.PluginName} embedded spider audio skipped cue={cue}; source={source}; {ex.GetType().Name}: {ex.Message}");
        }
    }

    internal void StopAllVoices()
    {
        for (int index = _activeVoices.Count - 1; index >= 0; index--)
        {
            StopChannelQuietly(_activeVoices[index].Channel);
            _activeVoices.RemoveAt(index);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;

        StopAllVoices();
        foreach (FMOD.Sound sound in _ownedSounds)
        {
            if (!sound.hasHandle())
            {
                continue;
            }

            FMOD.RESULT result = sound.release();
            if (result != FMOD.RESULT.OK)
            {
                _logger.LogWarning($"{Plugin.PluginName} embedded spider audio release failed: {DescribeFmodResult(result)}");
            }
        }

        _ownedSounds.Clear();
        _soundsByCue.Clear();
        LoadedSoundCount = 0;
    }

    private bool EnsureLoaded()
    {
        if (_loaded)
        {
            return LoadedSoundCount > 0;
        }

        if (_terminalLoadFailure || _disposed)
        {
            return false;
        }

        if (!RuntimeManager.IsInitialized)
        {
            if (Time.unscaledTime >= _nextStartupLogTime)
            {
                _logger.LogInfo($"{Plugin.PluginName} embedded spider audio waiting for FoA FMOD runtime initialization.");
                _nextStartupLogTime = Time.unscaledTime + 5f;
            }
            return false;
        }

        try
        {
            _coreSystem = RuntimeManager.CoreSystem;
            if (!_coreSystem.hasHandle())
            {
                throw new InvalidOperationException("FoA FMOD CoreSystem has no valid handle.");
            }

            RequireFmodOk(
                _coreSystem.getMasterChannelGroup(out _masterChannelGroup),
                "get FoA FMOD Core master channel group");
            if (!_masterChannelGroup.hasHandle())
            {
                throw new InvalidOperationException("FoA FMOD Core master channel group has no valid handle.");
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (EmbeddedSoundResource resource in EmbeddedResources)
            {
                LoadResourceSound(assembly, resource);
            }

            _loaded = true;
            LoadedSoundCount = _ownedSounds.Count;
            if (LoadedSoundCount == 0)
            {
                _logger.LogWarning($"{Plugin.PluginName} embedded spider audio loaded no sounds; cues stay silent.");
                return false;
            }

            _logger.LogInfo($"{Plugin.PluginName} loaded {LoadedSoundCount} embedded spider audio sound(s).");
            return true;
        }
        catch (Exception ex)
        {
            StopAllVoices();
            foreach (FMOD.Sound sound in _ownedSounds)
            {
                if (sound.hasHandle())
                {
                    sound.release();
                }
            }
            _ownedSounds.Clear();
            _soundsByCue.Clear();
            LoadedSoundCount = 0;
            _terminalLoadFailure = true;
            _logger.LogWarning($"{Plugin.PluginName} embedded spider audio startup failed: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private void LoadResourceSound(Assembly assembly, EmbeddedSoundResource resource)
    {
        string logicalName = ResourcePrefix + resource.FileName;
        using Stream? stream = assembly.GetManifestResourceStream(logicalName);
        if (stream == null)
        {
            _logger.LogWarning($"{Plugin.PluginName} embedded spider audio resource missing: {logicalName}");
            return;
        }

        byte[] bytes = ReadStreamBounded(stream, MaximumResourceBytes, logicalName);
        if (!PcmWavDecoder.TryDecode(bytes, Path.GetFileNameWithoutExtension(resource.FileName), out _, out string error))
        {
            throw new InvalidDataException($"Embedded spider WAV '{resource.FileName}' is not supported PCM: {error}");
        }

        if (_ownedSounds.Count >= MaximumUniqueSounds)
        {
            throw new InvalidDataException($"Embedded spider audio exceeds the {MaximumUniqueSounds} unique-sound limit.");
        }

        FMOD.CREATESOUNDEXINFO createInfo = new()
        {
            cbsize = Marshal.SizeOf(typeof(FMOD.CREATESOUNDEXINFO)),
            length = checked((uint)bytes.Length),
            suggestedsoundtype = FMOD.SOUND_TYPE.WAV,
        };
        FMOD.MODE mode = FMOD.MODE.OPENMEMORY
            | FMOD.MODE.CREATESAMPLE
            | FMOD.MODE.LOOP_OFF
            | FMOD.MODE._3D
            | FMOD.MODE._3D_WORLDRELATIVE
            | FMOD.MODE._3D_INVERSEROLLOFF;

        RequireFmodOk(
            _coreSystem.createSound(bytes, mode, ref createInfo, out FMOD.Sound sound),
            $"create embedded spider FMOD sound '{resource.FileName}'");
        if (!sound.hasHandle())
        {
            throw new InvalidOperationException($"FMOD created no sound handle for embedded spider resource '{resource.FileName}'.");
        }

        _ownedSounds.Add(sound);
        if (!_soundsByCue.TryGetValue(resource.Cue, out List<LoadedSound>? sounds))
        {
            sounds = new List<LoadedSound>();
            _soundsByCue.Add(resource.Cue, sounds);
        }
        sounds.Add(new LoadedSound(resource.FileName, sound));
    }

    private void UpdatePauseAndLifetime()
    {
        bool timeScalePaused = Time.timeScale <= 0f;
        for (int index = _activeVoices.Count - 1; index >= 0; index--)
        {
            ActiveVoice voice = _activeVoices[index];
            FMOD.RESULT playingResult = voice.Channel.isPlaying(out bool isPlaying);
            if (IsEndedChannelResult(playingResult) || (playingResult == FMOD.RESULT.OK && !isPlaying))
            {
                _activeVoices.RemoveAt(index);
                continue;
            }

            if (playingResult != FMOD.RESULT.OK)
            {
                WarnPlaybackFailure($"{Plugin.PluginName} embedded spider audio playback status failed: {DescribeFmodResult(playingResult)}");
                StopChannelQuietly(voice.Channel);
                _activeVoices.RemoveAt(index);
                continue;
            }

            if (voice.FollowTransform != null)
            {
                FMOD.VECTOR position = RuntimeUtils.ToFMODVector(voice.FollowTransform.position);
                FMOD.VECTOR velocity = default;
                FMOD.RESULT positionResult = voice.Channel.set3DAttributes(ref position, ref velocity);
                if (positionResult != FMOD.RESULT.OK)
                {
                    if (IsEndedChannelResult(positionResult))
                    {
                        _activeVoices.RemoveAt(index);
                        continue;
                    }
                    WarnPlaybackFailure($"{Plugin.PluginName} embedded spider audio position update failed: {DescribeFmodResult(positionResult)}");
                    StopChannelQuietly(voice.Channel);
                    _activeVoices.RemoveAt(index);
                    continue;
                }
            }

            if (voice.PausedByTimeScale != timeScalePaused)
            {
                FMOD.RESULT pauseResult = voice.Channel.setPaused(timeScalePaused);
                if (pauseResult != FMOD.RESULT.OK)
                {
                    if (IsEndedChannelResult(pauseResult))
                    {
                        _activeVoices.RemoveAt(index);
                        continue;
                    }
                    WarnPlaybackFailure($"{Plugin.PluginName} embedded spider audio pause transition failed: {DescribeFmodResult(pauseResult)}");
                    StopChannelQuietly(voice.Channel);
                    _activeVoices.RemoveAt(index);
                    continue;
                }
                voice.PausedByTimeScale = timeScalePaused;
            }
        }

        if (_activeVoices.Count < MaximumConcurrentVoices)
        {
            _capacityWarningLogged = false;
        }
    }

    private void PruneFinishedVoices()
    {
        for (int index = _activeVoices.Count - 1; index >= 0; index--)
        {
            ActiveVoice voice = _activeVoices[index];
            FMOD.RESULT result = voice.Channel.isPlaying(out bool isPlaying);
            if (result == FMOD.RESULT.OK && isPlaying)
            {
                continue;
            }

            if (result != FMOD.RESULT.OK && !IsEndedChannelResult(result))
            {
                WarnPlaybackFailure($"{Plugin.PluginName} embedded spider audio pruning failed: {DescribeFmodResult(result)}");
            }

            StopChannelQuietly(voice.Channel);
            _activeVoices.RemoveAt(index);
        }
    }

    private void StopChannelQuietly(FMOD.Channel channel)
    {
        if (!channel.hasHandle())
        {
            return;
        }

        FMOD.RESULT result = channel.stop();
        if (result != FMOD.RESULT.OK && !IsEndedChannelResult(result))
        {
            WarnPlaybackFailure($"{Plugin.PluginName} embedded spider audio channel stop failed: {DescribeFmodResult(result)}");
        }
    }

    private static byte[] ReadStreamBounded(Stream stream, int maximumBytes, string context)
    {
        using MemoryStream memory = new();
        byte[] buffer = new byte[81920];
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            if (memory.Length + read > maximumBytes)
            {
                throw new InvalidDataException($"{context} exceeds {maximumBytes} bytes.");
            }

            memory.Write(buffer, 0, read);
        }

        return memory.ToArray();
    }

    private static void RequireFmodOk(FMOD.RESULT result, string operation)
    {
        if (result != FMOD.RESULT.OK)
        {
            throw new InvalidOperationException($"Could not {operation}: {DescribeFmodResult(result)}");
        }
    }

    private static bool IsEndedChannelResult(FMOD.RESULT result)
    {
        return result == FMOD.RESULT.ERR_INVALID_HANDLE || result == FMOD.RESULT.ERR_CHANNEL_STOLEN;
    }

    private static string DescribeFmodResult(FMOD.RESULT result)
    {
        return $"{result} ({FMOD.Error.String(result)})";
    }

    private void WarnPlaybackFailure(string message)
    {
        if (_playbackFailureWarnings >= 5)
        {
            return;
        }

        _playbackFailureWarnings++;
        _logger.LogWarning(message);
    }

    private readonly struct EmbeddedSoundResource
    {
        internal EmbeddedSoundResource(BroodmotherEmbeddedAudioCue cue, string fileName)
        {
            Cue = cue;
            FileName = fileName;
        }

        internal BroodmotherEmbeddedAudioCue Cue { get; }
        internal string FileName { get; }
    }

    private readonly struct LoadedSound
    {
        internal LoadedSound(string name, FMOD.Sound sound)
        {
            Name = name;
            Sound = sound;
        }

        internal string Name { get; }
        internal FMOD.Sound Sound { get; }
    }

    private sealed class ActiveVoice
    {
        internal ActiveVoice(FMOD.Channel channel, Transform? followTransform)
        {
            Channel = channel;
            FollowTransform = followTransform;
        }

        internal FMOD.Channel Channel { get; }
        internal Transform? FollowTransform { get; }
        internal bool PausedByTimeScale { get; set; }
    }
}

internal sealed class PcmWavData
{
    internal PcmWavData(string clipName, int channels, int sampleRate, int sampleFrames, float[] samples)
    {
        ClipName = clipName;
        Channels = channels;
        SampleRate = sampleRate;
        SampleFrames = sampleFrames;
        Samples = samples;
    }

    internal string ClipName { get; }
    internal int Channels { get; }
    internal int SampleRate { get; }
    internal int SampleFrames { get; }
    internal float[] Samples { get; }
}

internal static class PcmWavDecoder
{
    private const int MaximumChannels = 8;
    private const int MinimumSampleRate = 8000;
    private const int MaximumSampleRate = 192000;
    private const int MaximumDurationSeconds = 120;
    private const int MaximumInterleavedSamples = 8_000_000;

    internal static bool TryDecode(byte[] bytes, string clipName, out PcmWavData? wav, out string error)
    {
        wav = null;
        error = string.Empty;

        try
        {
            if (bytes == null)
            {
                throw new InvalidDataException("WAV bytes are null.");
            }

            if (bytes.Length < 44)
            {
                throw new InvalidDataException("WAV is shorter than the minimum PCM header.");
            }

            if (!FourCcEquals(bytes, 0, "RIFF") || !FourCcEquals(bytes, 8, "WAVE"))
            {
                throw new InvalidDataException("Missing RIFF/WAVE header.");
            }

            uint riffPayloadSize = ReadUInt32(bytes, 4);
            long riffEndLong = 8L + riffPayloadSize;
            if (riffEndLong != bytes.Length)
            {
                throw new InvalidDataException($"RIFF size declares {riffEndLong} bytes but file contains {bytes.Length}.");
            }

            bool foundFormat = false;
            bool foundData = false;
            ushort audioFormat = 0;
            ushort channels = 0;
            uint sampleRate = 0;
            uint byteRate = 0;
            ushort blockAlign = 0;
            ushort bitsPerSample = 0;
            int dataOffset = 0;
            int dataLength = 0;

            int offset = 12;
            while (offset < bytes.Length)
            {
                if (bytes.Length - offset < 8)
                {
                    throw new InvalidDataException("Truncated WAV chunk header.");
                }

                string chunkId = ReadFourCc(bytes, offset);
                uint chunkSizeUnsigned = ReadUInt32(bytes, offset + 4);
                if (chunkSizeUnsigned > int.MaxValue)
                {
                    throw new InvalidDataException($"Chunk '{chunkId}' is too large.");
                }

                int chunkSize = (int)chunkSizeUnsigned;
                int chunkDataOffset = checked(offset + 8);
                long nextOffsetLong = (long)chunkDataOffset + chunkSize + (chunkSize & 1);
                if (nextOffsetLong > bytes.Length)
                {
                    throw new InvalidDataException($"Chunk '{chunkId}' extends beyond the RIFF boundary.");
                }

                if (string.Equals(chunkId, "fmt ", StringComparison.Ordinal))
                {
                    if (foundFormat)
                    {
                        throw new InvalidDataException("Duplicate fmt chunk.");
                    }
                    if (chunkSize < 16)
                    {
                        throw new InvalidDataException("fmt chunk is shorter than 16 bytes.");
                    }

                    audioFormat = ReadUInt16(bytes, chunkDataOffset);
                    channels = ReadUInt16(bytes, chunkDataOffset + 2);
                    sampleRate = ReadUInt32(bytes, chunkDataOffset + 4);
                    byteRate = ReadUInt32(bytes, chunkDataOffset + 8);
                    blockAlign = ReadUInt16(bytes, chunkDataOffset + 12);
                    bitsPerSample = ReadUInt16(bytes, chunkDataOffset + 14);
                    foundFormat = true;
                }
                else if (string.Equals(chunkId, "data", StringComparison.Ordinal))
                {
                    if (foundData)
                    {
                        throw new InvalidDataException("Duplicate data chunk.");
                    }
                    dataOffset = chunkDataOffset;
                    dataLength = chunkSize;
                    foundData = true;
                }

                offset = (int)nextOffsetLong;
            }

            if (offset != bytes.Length)
            {
                throw new InvalidDataException("WAV chunk walk did not end on the RIFF boundary.");
            }

            if (!foundFormat || !foundData)
            {
                throw new InvalidDataException("WAV must contain exactly one fmt chunk and one data chunk.");
            }

            if (audioFormat != 1)
            {
                throw new InvalidDataException($"Only integer PCM format 1 is supported; found format {audioFormat}.");
            }

            if (channels < 1 || channels > MaximumChannels)
            {
                throw new InvalidDataException($"Channel count {channels} is outside 1..{MaximumChannels}.");
            }

            if (sampleRate < MinimumSampleRate || sampleRate > MaximumSampleRate)
            {
                throw new InvalidDataException($"Sample rate {sampleRate} is outside {MinimumSampleRate}..{MaximumSampleRate} Hz.");
            }

            if (bitsPerSample != 8 && bitsPerSample != 16 && bitsPerSample != 24 && bitsPerSample != 32)
            {
                throw new InvalidDataException($"PCM bit depth {bitsPerSample} is unsupported.");
            }

            int bytesPerSample = bitsPerSample / 8;
            int expectedBlockAlign = checked(channels * bytesPerSample);
            if (blockAlign != expectedBlockAlign)
            {
                throw new InvalidDataException($"Block alignment {blockAlign} does not match {expectedBlockAlign}.");
            }

            uint expectedByteRate = checked(sampleRate * (uint)blockAlign);
            if (byteRate != expectedByteRate)
            {
                throw new InvalidDataException($"Byte rate {byteRate} does not match {expectedByteRate}.");
            }

            if (dataLength <= 0 || dataLength % blockAlign != 0)
            {
                throw new InvalidDataException("PCM data is empty or not aligned to complete sample frames.");
            }

            int sampleFrames = dataLength / blockAlign;
            long maximumFrames = (long)sampleRate * MaximumDurationSeconds;
            if (sampleFrames > maximumFrames)
            {
                throw new InvalidDataException($"WAV duration exceeds {MaximumDurationSeconds} seconds.");
            }

            int interleavedSampleCount = checked(sampleFrames * channels);
            if (interleavedSampleCount > MaximumInterleavedSamples)
            {
                throw new InvalidDataException($"WAV contains {interleavedSampleCount} samples; maximum is {MaximumInterleavedSamples}.");
            }

            float[] samples = new float[interleavedSampleCount];
            for (int sampleIndex = 0; sampleIndex < interleavedSampleCount; sampleIndex++)
            {
                int sampleOffset = checked(dataOffset + (sampleIndex * bytesPerSample));
                samples[sampleIndex] = ReadPcmSample(bytes, sampleOffset, bitsPerSample);
            }

            wav = new PcmWavData(
                string.IsNullOrWhiteSpace(clipName) ? "broodmother-embedded-audio" : clipName,
                channels,
                checked((int)sampleRate),
                sampleFrames,
                samples);
            return true;
        }
        catch (Exception ex) when (ex is InvalidDataException || ex is OverflowException || ex is IndexOutOfRangeException)
        {
            error = ex.Message;
            return false;
        }
    }

    private static float ReadPcmSample(byte[] data, int offset, int bitsPerSample)
    {
        switch (bitsPerSample)
        {
            case 8:
                return (data[offset] - 128) / 128f;
            case 16:
            {
                short sample = unchecked((short)(data[offset] | (data[offset + 1] << 8)));
                return sample / 32768f;
            }
            case 24:
            {
                int sample = data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16);
                if ((sample & 0x800000) != 0)
                {
                    sample |= unchecked((int)0xff000000);
                }
                return sample / 8388608f;
            }
            case 32:
            {
                int sample = data[offset]
                    | (data[offset + 1] << 8)
                    | (data[offset + 2] << 16)
                    | (data[offset + 3] << 24);
                return sample / 2147483648f;
            }
            default:
                throw new InvalidDataException($"Unsupported PCM bit depth {bitsPerSample}.");
        }
    }

    private static bool FourCcEquals(byte[] bytes, int offset, string expected)
    {
        if (offset < 0 || offset + 4 > bytes.Length || expected.Length != 4)
        {
            return false;
        }

        return bytes[offset] == expected[0]
            && bytes[offset + 1] == expected[1]
            && bytes[offset + 2] == expected[2]
            && bytes[offset + 3] == expected[3];
    }

    private static string ReadFourCc(byte[] bytes, int offset)
    {
        if (offset < 0 || offset + 4 > bytes.Length)
        {
            throw new InvalidDataException("Truncated FourCC.");
        }
        return new string(new[] { (char)bytes[offset], (char)bytes[offset + 1], (char)bytes[offset + 2], (char)bytes[offset + 3] });
    }

    private static ushort ReadUInt16(byte[] bytes, int offset)
    {
        if (offset < 0 || offset + 2 > bytes.Length)
        {
            throw new InvalidDataException("Truncated UInt16.");
        }
        return (ushort)(bytes[offset] | (bytes[offset + 1] << 8));
    }

    private static uint ReadUInt32(byte[] bytes, int offset)
    {
        if (offset < 0 || offset + 4 > bytes.Length)
        {
            throw new InvalidDataException("Truncated UInt32.");
        }
        return (uint)(bytes[offset]
            | (bytes[offset + 1] << 8)
            | (bytes[offset + 2] << 16)
            | (bytes[offset + 3] << 24));
    }
}
