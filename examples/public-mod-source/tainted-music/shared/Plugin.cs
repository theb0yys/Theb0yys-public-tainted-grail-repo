using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Awaken.TG.Main.AudioSystem;
using Awaken.TG.Main.AudioSystem.Biomes;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights.DamageInfo;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Stories;
#if IL2CPP
using Awaken.TG.Main.Timing;
#endif
using Awaken.TG.MVC;
using Awaken.TG.MVC.Domains;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using FMODUnity;
using HarmonyLib;
#if IL2CPP
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TaintedMusic;

#if !IL2CPP
[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
#else
public sealed class Plugin
#endif
{
    public const string PluginGuid = "kane.tgfoa.tainted-music";
    public const string PluginName = "Tainted Music";
#if IL2CPP
    public const string PluginVersion = "0.6.0";
#else
    public const string PluginVersion = "0.5.0";
#endif

    private const string EmbeddedAssetPrefix = "TaintedMusic.Assets/";
    private const int MaximumEmbeddedWavBytes = 220 * 1024 * 1024;
    private const float WyrdnessConfirmationSeconds = 3f;
    private const float NativeAmbientSuppressionPollSeconds = 0.5f;
    private const float NativeMusicSuppressionPollSeconds = 0.25f;
#if IL2CPP
    private static readonly string[] EmbeddedAssetAssemblyFiles =
    {
        "TaintedMusic.Assets.Wyrdness.01.tma",
        "TaintedMusic.Assets.Wyrdness.02.tma",
        "TaintedMusic.Assets.Wyrdness.03.tma",
        "TaintedMusic.Assets.DayOpenWorld.01.tma",
        "TaintedMusic.Assets.Interior.01.tma",
        "TaintedMusic.Assets.Settlement.01.tma",
        "TaintedMusic.Assets.Settlement.02.tma",
        "TaintedMusic.Assets.ScaryPlace.01.tma",
        "TaintedMusic.Assets.ScaryPlace.02.tma",
        "TaintedMusic.Assets.ScaryPlace.03.tma",
        "TaintedMusic.Assets.ScaryPlace.04.tma"
    };
#endif

    private readonly Dictionary<MusicLane, List<TrackDefinition>> _tracksByLane = new Dictionary<MusicLane, List<TrackDefinition>>();
    private readonly Dictionary<MusicLane, int> _nextTrackIndexByLane = new Dictionary<MusicLane, int>();
#if IL2CPP
    private readonly Dictionary<ManualAudioZone, Il2CppReferenceArray<IAudioSource>> _suppressedAsylumAmbientZones = new Dictionary<ManualAudioZone, Il2CppReferenceArray<IAudioSource>>();
#else
    private readonly Dictionary<ManualAudioZone, IAudioSource[]> _suppressedAsylumAmbientZones = new Dictionary<ManualAudioZone, IAudioSource[]>();
#endif
    private readonly List<MusicVoice> _voices = new List<MusicVoice>(4);

    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<float> _globalVolume = null!;
    private ConfigEntry<float> _wyrdnessVolume = null!;
    private ConfigEntry<float> _dayOpenWorldVolume = null!;
    private ConfigEntry<float> _interiorVolume = null!;
    private ConfigEntry<float> _settlementVolume = null!;
    private ConfigEntry<float> _scaryPlaceVolume = null!;
    private ConfigEntry<bool> _wyrdnessMuted = null!;
    private ConfigEntry<bool> _dayOpenWorldMuted = null!;
    private ConfigEntry<bool> _interiorMuted = null!;
    private ConfigEntry<bool> _settlementMuted = null!;
    private ConfigEntry<bool> _scaryPlaceMuted = null!;
    private ConfigEntry<string> _settlementSceneKeywords = null!;
    private ConfigEntry<string> _scaryPlaceSceneKeywords = null!;
    private ConfigEntry<bool> _nativeMusicSuppressionEnabled = null!;
    private ConfigEntry<bool> _nativeMusicSuppressionRequiresPluginMusic = null!;
    private ConfigEntry<string> _nativeMusicAllowedSceneKeywords = null!;
    private ConfigEntry<string> _nativeMusicAllowedEventKeywords = null!;
    private ConfigEntry<bool> _asylumAmbientSuppressionEnabled = null!;
    private ConfigEntry<string> _asylumAmbientSuppressionSceneKeywords = null!;
    private ConfigEntry<bool> _contextDuckingEnabled = null!;
    private ConfigEntry<bool> _dialogueDuckingEnabled = null!;
    private ConfigEntry<float> _dialogueVolumeMultiplier = null!;
    private ConfigEntry<bool> _combatDuckingEnabled = null!;
    private ConfigEntry<float> _combatVolumeMultiplier = null!;
    private ConfigEntry<float> _combatHoldSeconds = null!;
    private ConfigEntry<float> _contextFadeSeconds = null!;
    private ConfigEntry<float> _fadeSeconds = null!;
    private ConfigEntry<float> _pollIntervalSeconds = null!;
    private ConfigEntry<bool> _logLaneChanges = null!;
    private ConfigEntry<float> _stateSampleIntervalSeconds = null!;

    private FMOD.System _coreSystem;
    private FMOD.ChannelGroup _masterChannelGroup;
    private Harmony? _harmony;
    private MusicMenuScreen? _menuScreen;
    private EmbeddedMusicAssets? _embeddedAssets;
    private MusicLane _activeLane = MusicLane.None;
    private float _nextPollTime;
    private bool _runtimeReady;
    private bool _startupWaitingLogged;
    private bool _startupFailed;
    private bool _gameTimeUnavailableLogged;
    private bool _dialogueContextUnavailableLogged;
    private float _wyrdnessCandidateSince = -1f;
    private float _nextStateSampleTime;
    private float _combatDuckUntilTime;
    private int _contextFrame = -1;
    private DynamicAudioContext _contextSnapshot = new DynamicAudioContext("None", 1f);
#if !IL2CPP
    private Type? _gameRealTimeType;
    private MethodInfo? _worldAnyGameRealTimeMethod;
    private FieldInfo? _manualAudioZoneWasPlayerWithinZoneField;
#endif
    private float _nextNativeMusicSuppressionPollTime;
    private bool _nativeMusicSuppressionActive;
    private bool _nativeMusicSuppressionUnavailableLogged;
    private bool _nativeMusicSuppressionPatchFailureLogged;
    private float _nextNativeAmbientSuppressionPollTime;
    private bool _nativeAmbientSuppressionActiveLogged;
    private bool _nativeAmbientSuppressionUnavailableLogged;
    private bool _shutdown;

    private static Plugin? Instance;

#if IL2CPP
    private ConfigFile? _il2CppConfig;
    private ManualLogSource? _il2CppLogger;

    private ConfigFile Config =>
        _il2CppConfig ?? throw new InvalidOperationException("Tainted Music IL2CPP config was not initialized.");

    private ManualLogSource Logger =>
        _il2CppLogger ?? throw new InvalidOperationException("Tainted Music IL2CPP logger was not initialized.");

    internal void Initialize(ConfigFile config, ManualLogSource logger)
    {
        _il2CppConfig = config;
        _il2CppLogger = logger;
        InitializeCore();
    }

    internal void Shutdown()
    {
        ShutdownCore();
    }

    internal static void Il2CppUpdate()
    {
        Instance?.UpdateCore();
    }

    internal static void Il2CppOnGUI()
    {
        Instance?._menuScreen?.OnGUI();
    }

    internal static void ShowMenuFromNativeMenu(string source)
    {
        Instance?._menuScreen?.Show("native-menu:" + source);
    }
#else
    private void Awake()
    {
        InitializeCore();
    }
#endif

    private void InitializeCore()
    {
        _shutdown = false;
        Instance = this;

        _enabled = Config.Bind("General", "Enabled", true, Ui("Enable Tainted Music playback.", "Playback", "Enabled", 0, 0));
        _globalVolume = Config.Bind("Audio", "GlobalVolume", 0.35f, Ui("Master volume for every Tainted Music lane.", "Playback", "Master Volume", 0, 10, new AcceptableValueRange<float>(0f, 1f)));
        _fadeSeconds = Config.Bind("Audio", "FadeSeconds", 4.0f, Ui("Seconds used to crossfade between music lanes.", "Playback", "Lane Crossfade", 0, 20, new AcceptableValueRange<float>(0.1f, 30f)));

        _wyrdnessVolume = Config.Bind("Audio", "WyrdnessVolume", 1.0f, Ui("Volume for music played during Wyrdness.", "Music Mix", "Wyrdness Volume", 10, 0, new AcceptableValueRange<float>(0f, 1f)));
        _wyrdnessMuted = Config.Bind("Audio", "WyrdnessMuted", false, Ui("Mute only the Wyrdness lane.", "Music Mix", "Mute Wyrdness", 10, 10));
        _dayOpenWorldVolume = Config.Bind("Audio", "DayOpenWorldVolume", 0.85f, Ui("Volume for daytime open-world music.", "Music Mix", "Day Open World Volume", 10, 20, new AcceptableValueRange<float>(0f, 1f)));
        _dayOpenWorldMuted = Config.Bind("Audio", "DayOpenWorldMuted", false, Ui("Mute only the daytime open-world lane.", "Music Mix", "Mute Day Open World", 10, 30));
        _interiorVolume = Config.Bind("Audio", "InteriorVolume", 0.75f, Ui("Volume for interiors, caves, and dungeons.", "Music Mix", "Interior Volume", 10, 40, new AcceptableValueRange<float>(0f, 1f)));
        _interiorMuted = Config.Bind("Audio", "InteriorMuted", false, Ui("Mute only the interior lane.", "Music Mix", "Mute Interiors", 10, 50));
        _settlementVolume = Config.Bind("Audio", "SettlementVolume", 0.75f, Ui("Volume for settlements and civilized hubs.", "Music Mix", "Settlement Volume", 10, 60, new AcceptableValueRange<float>(0f, 1f)));
        _settlementMuted = Config.Bind("Audio", "SettlementMuted", false, Ui("Mute only the settlement lane.", "Music Mix", "Mute Settlements", 10, 70));
        _scaryPlaceVolume = Config.Bind("Audio", "ScaryPlaceVolume", 0.8f, Ui("Volume for asylums, crypts, and frightening places.", "Music Mix", "Scary Place Volume", 10, 80, new AcceptableValueRange<float>(0f, 1f)));
        _scaryPlaceMuted = Config.Bind("Audio", "ScaryPlaceMuted", false, Ui("Mute only the scary-place lane.", "Music Mix", "Mute Scary Places", 10, 90));

        _nativeMusicSuppressionEnabled = Config.Bind("Native Suppression", "NativeMusicSuppressionEnabled", true, Ui("Stop ordinary native FoA music while a Tainted Music lane is playing.", "Dynamics & Native Audio", "Suppress Native Music", 20, 0));
        _asylumAmbientSuppressionEnabled = Config.Bind("Native Suppression", "AsylumAmbientSuppressionEnabled", true, Ui("Stop the repetitive vanilla asylum ambience while scary-place music plays.", "Dynamics & Native Audio", "Suppress Asylum Ambience", 20, 10));
        _contextDuckingEnabled = Config.Bind("Context Ducking", "Enabled", true, Ui("Allow Tainted Music to lower itself during dialogue and combat.", "Dynamics & Native Audio", "Context Ducking", 20, 20));
        _dialogueDuckingEnabled = Config.Bind("Context Ducking", "DialogueDuckingEnabled", true, Ui("Lower Tainted Music while the hero is in dialogue.", "Dynamics & Native Audio", "Duck During Dialogue", 20, 30));
        _dialogueVolumeMultiplier = Config.Bind("Context Ducking", "DialogueVolumeMultiplier", 0.25f, Ui("Music level applied during hero dialogue.", "Dynamics & Native Audio", "Dialogue Music Level", 20, 40, new AcceptableValueRange<float>(0f, 1f)));
        _combatDuckingEnabled = Config.Bind("Context Ducking", "CombatDuckingEnabled", true, Ui("Briefly lower Tainted Music after hero combat damage.", "Dynamics & Native Audio", "Duck During Combat", 20, 50));
        _combatVolumeMultiplier = Config.Bind("Context Ducking", "CombatVolumeMultiplier", 0.55f, Ui("Music level applied after hero combat damage.", "Dynamics & Native Audio", "Combat Music Level", 20, 60, new AcceptableValueRange<float>(0f, 1f)));
        _combatHoldSeconds = Config.Bind("Context Ducking", "CombatHoldSeconds", 6.0f, Ui("Seconds to keep combat ducking active after the latest hit.", "Dynamics & Native Audio", "Combat Duck Duration", 20, 70, new AcceptableValueRange<float>(0f, 30f)));

        _settlementSceneKeywords = Config.Bind("Routing", "SettlementSceneKeywords", "Fortress,Keep,Village,Settlement,Roscree,Mansion,Main Square,MainSquare,Lower City,LowerCity,Craftsmen,Craftsmen's Row,Farmland,Capital,Sveinn,Volker,Ulfr,Theud,Gunnvaldr,Oighreata,Chapter House,ChapterHouse,Sanctuary,Tavern,Inn,Market", Ui("Comma-separated scene keywords for the settlement lane.", "Advanced", "Settlement Scene Keywords", 100, 0, hidden: true));
        _scaryPlaceSceneKeywords = Config.Bind("Routing", "ScaryPlaceSceneKeywords", "Asylum,Crypt,Crypts,Tomb,Dungeon,Lair,Cave,Grotto,Grave,Graveyard,Catacomb,Catacombs,Corrupted,Waterworks,Chapel,Dead,Abandoned,Deserted", Ui("Comma-separated scene keywords for the scary-place lane.", "Advanced", "Scary Place Scene Keywords", 100, 10, hidden: true));
        _nativeMusicSuppressionRequiresPluginMusic = Config.Bind("Native Suppression", "NativeMusicSuppressionRequiresPluginMusic", true, Ui("Suppress native music only while Tainted Music is active.", "Advanced", "Require Active Tainted Music", 100, 20, hidden: true));
        _nativeMusicAllowedSceneKeywords = Config.Bind("Native Suppression", "NativeMusicAllowedSceneKeywords", "Boss,MainBoss,FinalBoss,Cutscene,Cinematic,Dramatic,Unique", Ui("Scenes where unique native music remains allowed.", "Advanced", "Native Music Scene Exceptions", 100, 30, hidden: true));
        _nativeMusicAllowedEventKeywords = Config.Bind("Native Suppression", "NativeMusicAllowedEventKeywords", "Boss,MainBoss,FinalBoss,Cutscene,Cinematic,Dramatic,Unique", Ui("Native music events that remain allowed.", "Advanced", "Native Music Event Exceptions", 100, 40, hidden: true));
        _asylumAmbientSuppressionSceneKeywords = Config.Bind("Native Suppression", "AsylumAmbientSceneKeywords", "Asylum,AsylumRevisited,Dungeon_AsylumRevisited,IslandAsylum", Ui("Scenes where vanilla asylum ambience suppression applies.", "Advanced", "Asylum Scene Keywords", 100, 50, hidden: true));
        _contextFadeSeconds = Config.Bind("Context Ducking", "ContextFadeSeconds", 1.25f, Ui("Seconds used to fade into and out of contextual ducking.", "Advanced", "Context Fade", 100, 60, new AcceptableValueRange<float>(0.1f, 10f), hidden: true));
        _pollIntervalSeconds = Config.Bind("Audio", "PollIntervalSeconds", 1.0f, Ui("Seconds between lane checks.", "Advanced", "Lane Poll Interval", 100, 70, new AcceptableValueRange<float>(0.25f, 10f), hidden: true));
        _logLaneChanges = Config.Bind("Diagnostics", "LogLaneChanges", false, Ui("Write music lane changes to the BepInEx log.", "Diagnostics", "Log Lane Changes", 110, 0, hidden: true));
        _stateSampleIntervalSeconds = Config.Bind("Diagnostics", "StateSampleIntervalSeconds", 10.0f, Ui("Seconds between diagnostic state samples.", "Diagnostics", "State Sample Interval", 110, 10, new AcceptableValueRange<float>(2f, 60f), hidden: true));

        TaintedInterfaceBridge.SetLogger(Logger);
        _menuScreen = new MusicMenuScreen(Config, BuildMusicMenuSnapshot, Logger);
        RegisterContextPatches();
        Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
    }

#if !IL2CPP
    private void Update()
    {
        UpdateCore();
    }
#endif

    private void UpdateCore()
    {
        _menuScreen?.Update();
        float deltaSeconds = Mathf.Max(Time.unscaledDeltaTime, Time.deltaTime);
        UpdateVoiceTargets();
        UpdateVoices(deltaSeconds);

        if (!_enabled.Value)
        {
            if (_activeLane != MusicLane.None)
            {
                SwitchLane(MusicLane.None, "disabled");
            }
            UpdateNativeMusicSuppression();
            UpdateNativeAmbientSuppression();
            return;
        }

        if (!_runtimeReady)
        {
            TryStartRuntimeWhenReady();
            if (!_runtimeReady)
            {
                UpdateNativeMusicSuppression();
                UpdateNativeAmbientSuppression();
                return;
            }
        }

        float now = Time.unscaledTime;
        if (now < _nextPollTime)
        {
            return;
        }

        _nextPollTime = now + Clamp(_pollIntervalSeconds.Value, 0.25f, 10f);
        MusicLane nextLane = DetermineLane(out string reason);
        LogStateSample(nextLane, reason);
        SwitchLane(nextLane, reason);
        UpdateNativeMusicSuppression();
        UpdateNativeAmbientSuppression();
        EnsureActiveLaneVoice();
    }

#if !IL2CPP
    private void OnGUI()
    {
        _menuScreen?.OnGUI();
    }

    private void OnDestroy()
    {
        ShutdownCore();
    }
#endif

    private void ShutdownCore()
    {
        if (_shutdown)
        {
            return;
        }

        _shutdown = true;
        _menuScreen?.Close("plugin-destroyed");
        _menuScreen = null;
        RestoreSuppressedNativeAmbientSources("plugin-destroyed");
        UpdateNativeMusicSuppressionInactive("plugin-destroyed");
        StopAllVoices();
        if (_harmony != null)
        {
            _harmony.UnpatchSelf();
            _harmony = null;
        }

        _tracksByLane.Clear();
        _nextTrackIndexByLane.Clear();
        _embeddedAssets = null;
        if (Instance == this)
        {
            Instance = null;
        }
    }

    internal static void RecordCombatImpact(Damage damage)
    {
        Instance?.RecordCombatImpactInstance(damage);
    }

    internal static void RecordCombatImpactFailure(Exception ex)
    {
        Instance?.Logger.LogWarning($"{PluginName}: contextual combat ducking damage postfix failed. Further postfix failures will be suppressed. {ex.GetType().Name}: {ex.Message}");
    }

    private void RegisterContextPatches()
    {
        try
        {
            _harmony = new Harmony(PluginGuid);
            CombatDuckingDamagePatch.Apply(_harmony, Logger);
            NativeMusicSuppressionPatch.Apply(_harmony, Logger);
            NativeMusicMenuPatch.Apply(_harmony, Logger);
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName}: patch registration failed. Runtime will continue with any patches already applied. {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void RecordCombatImpactInstance(Damage damage)
    {
        if (!_enabled.Value || !_contextDuckingEnabled.Value || !_combatDuckingEnabled.Value || damage == null)
        {
            return;
        }

        try
        {
            Hero hero = Hero.Current;
            if (hero == null || hero.HasBeenDiscarded)
            {
                return;
            }

#if IL2CPP
            if (damage.TargetPure?.Pointer != hero.Pointer && damage.DamageDealerPure?.Pointer != hero.Pointer)
#else
            if (damage.TargetPure != hero && damage.DamageDealerPure != hero)
#endif
            {
                return;
            }

            float now = Time.unscaledTime;
            _combatDuckUntilTime = Mathf.Max(
                _combatDuckUntilTime,
                now + Clamp(_combatHoldSeconds.Value, 0.25f, 30f));
            _contextFrame = -1;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"{PluginName}: contextual combat ducking failed to read damage context. {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void TryStartRuntimeWhenReady()
    {
        if (_startupFailed || _runtimeReady)
        {
            return;
        }

        if (!RuntimeManager.IsInitialized)
        {
            if (!_startupWaitingLogged)
            {
                _startupWaitingLogged = true;
                Logger.LogInfo($"{PluginName} waiting for FoA FMOD runtime.");
            }
            return;
        }

        try
        {
            _coreSystem = RuntimeManager.CoreSystem;
            RequireFmodOk(_coreSystem.getMasterChannelGroup(out _masterChannelGroup), "get the FoA FMOD Core master channel group");

            _embeddedAssets = EmbeddedMusicAssets.TryCreate(EmbeddedMusicAssemblies())
                ?? throw new InvalidOperationException("No embedded Tainted Music WAV resources were found.");

            int trackCount = 0;
            trackCount += LoadLaneDefinitions(_embeddedAssets, MusicLane.Wyrdness, "Wyrdness");
            trackCount += LoadLaneDefinitions(_embeddedAssets, MusicLane.DayOpenWorld, "DayOpenWorld");
            trackCount += LoadLaneDefinitions(_embeddedAssets, MusicLane.Interior, "Interior");
            trackCount += LoadLaneDefinitions(_embeddedAssets, MusicLane.Settlement, "Settlement");
            trackCount += LoadLaneDefinitions(_embeddedAssets, MusicLane.ScaryPlace, "ScaryPlace");

            if (trackCount <= 0)
            {
                throw new InvalidOperationException("No lane-owned embedded Tainted Music tracks were indexed.");
            }

            _runtimeReady = true;
            Logger.LogInfo($"Indexed {trackCount.ToString(CultureInfo.InvariantCulture)} embedded Tainted Music track(s) from {_embeddedAssets.ResourceCount.ToString(CultureInfo.InvariantCulture)} resource(s).");
        }
        catch (Exception ex)
        {
            _startupFailed = true;
            Logger.LogError($"{PluginName} startup failed closed: {ex.GetType().Name}: {ex.Message}");
            ReleaseTracksQuietly();
        }
    }

    private MusicLane DetermineLane(out string reason)
    {
        if (!TryReadScene(out SceneSnapshot scene))
        {
            reason = "scene-unavailable";
            return MusicLane.None;
        }

        if (!TryReadPlayableHero(out string heroReason))
        {
            reason = $"{heroReason};scene={CleanLogValue(scene.SceneName)}";
            return MusicLane.None;
        }

        if (TryReadWyrdnessExposure(scene, out bool exposed, out string exposureReason) && UpdateWyrdnessConfirmation(exposed))
        {
            reason = $"wyrdness:{exposureReason};scene={CleanLogValue(scene.SceneName)}";
            return MusicLane.Wyrdness;
        }

        if (!scene.OpenWorldKnown)
        {
            reason = $"open-world-state-unavailable;scene={CleanLogValue(scene.SceneName)}";
            return MusicLane.None;
        }

        string sceneLabel = SceneKeywordText(scene);
        if (MatchesKeyword(sceneLabel, _scaryPlaceSceneKeywords.Value, out string scaryKeyword))
        {
            reason = $"scary-place;keyword={CleanLogValue(scaryKeyword)};scene={CleanLogValue(scene.SceneName)}";
            return MusicLane.ScaryPlace;
        }

        if (MatchesKeyword(sceneLabel, _settlementSceneKeywords.Value, out string settlementKeyword))
        {
            reason = $"settlement;keyword={CleanLogValue(settlementKeyword)};scene={CleanLogValue(scene.SceneName)}";
            return MusicLane.Settlement;
        }

        if (!scene.IsOpenWorld)
        {
            reason = $"interior;scene={CleanLogValue(scene.SceneName)}";
            return MusicLane.Interior;
        }

        if (TryReadIsNight(out bool isNight, out string timeSource))
        {
            if (!isNight)
            {
                reason = $"day-open-world;scene={CleanLogValue(scene.SceneName)};time={CleanLogValue(timeSource)}";
                return MusicLane.DayOpenWorld;
            }

            reason = $"open-world-night;scene={CleanLogValue(scene.SceneName)};time={CleanLogValue(timeSource)}";
            return MusicLane.None;
        }

        reason = $"open-world-time-unavailable;scene={CleanLogValue(scene.SceneName)};time={CleanLogValue(timeSource)}";
        return MusicLane.None;
    }

    private bool UpdateWyrdnessConfirmation(bool rawExposed)
    {
        if (!rawExposed)
        {
            _wyrdnessCandidateSince = -1f;
            return false;
        }

        float now = Time.unscaledTime;
        if (_wyrdnessCandidateSince < 0f)
        {
            _wyrdnessCandidateSince = now;
        }

        return now - _wyrdnessCandidateSince >= WyrdnessConfirmationSeconds;
    }

    private void LogStateSample(MusicLane candidateLane, string reason)
    {
        if (!_logLaneChanges.Value)
        {
            return;
        }

        float now = Time.unscaledTime;
        if (now < _nextStateSampleTime)
        {
            return;
        }

        _nextStateSampleTime = now + Clamp(_stateSampleIntervalSeconds.Value, 2f, 60f);
        DynamicAudioContext audioContext = GetCurrentAudioContext();
        Logger.LogInfo($"{PluginName} lane sample: active={_activeLane}; candidate={candidateLane}; context={CleanLogValue(audioContext.Label)}; contextVolume={FormatFloat(audioContext.VolumeMultiplier)}; reason={reason}; {NativeSuppressionLog()}.");
    }

    private static bool TryReadPlayableHero(out string reason)
    {
        try
        {
            Hero hero = Hero.Current;
            if (hero == null || hero.HasBeenDiscarded)
            {
                reason = "hero-unavailable";
                return false;
            }

            reason = "hero-ready";
            return true;
        }
        catch (Exception ex)
        {
            reason = $"hero-exception:{ex.GetType().Name}";
            return false;
        }
    }

    private void SwitchLane(MusicLane nextLane, string reason)
    {
        if (nextLane != MusicLane.None && IsLaneMuted(nextLane))
        {
            reason = $"{reason};bucket-muted";
            nextLane = MusicLane.None;
        }

        if (nextLane != MusicLane.None && !HasLaneTracks(nextLane))
        {
            reason = $"{reason};track-unavailable";
            nextLane = MusicLane.None;
        }

        if (nextLane == _activeLane)
        {
            return;
        }

        foreach (MusicVoice voice in _voices)
        {
            voice.TargetVolume = 0f;
            voice.FadeSeconds = LaneFadeSeconds();
            voice.StopWhenSilent = true;
        }

        MusicLane previousLane = _activeLane;
        _activeLane = nextLane;

        if (nextLane != MusicLane.None)
        {
            StartVoice(SelectNextTrack(nextLane));
        }

        if (_logLaneChanges.Value)
        {
            Logger.LogInfo($"{PluginName} lane change: {previousLane} -> {nextLane}; reason={reason}; {NativeSuppressionLog()}.");
        }
    }

    internal static bool TrySuppressNativeMusic(AudioCore audioCore, string nativeMusicLane, out string reason)
    {
        reason = "plugin-unavailable";
        return Instance != null && Instance.TrySuppressNativeMusicInstance(audioCore, nativeMusicLane, out reason);
    }

    internal static void RecordNativeMusicSuppressionPatchFailure(Exception ex)
    {
        if (Instance == null || Instance._nativeMusicSuppressionPatchFailureLogged)
        {
            return;
        }

        Instance._nativeMusicSuppressionPatchFailureLogged = true;
        Instance.Logger.LogWarning($"{PluginName}: native music suppression patch failed. Native music suppression will keep retrying through the poll backstop. {ex.GetType().Name}: {ex.Message}");
    }

    private void UpdateNativeMusicSuppression()
    {
        if (!ShouldPollNativeMusicSuppression(out string inactiveReason))
        {
            UpdateNativeMusicSuppressionInactive(inactiveReason);
            return;
        }

        float now = Time.unscaledTime;
        if (now < _nextNativeMusicSuppressionPollTime)
        {
            return;
        }

        _nextNativeMusicSuppressionPollTime = now + NativeMusicSuppressionPollSeconds;

        try
        {
            AudioCore audioCore = World.Services.Get<AudioCore>();
            if (TrySuppressNativeMusicInstance(audioCore, "poll", out string reason))
            {
                return;
            }

            UpdateNativeMusicSuppressionInactive(reason);
        }
        catch (Exception ex)
        {
            LogNativeMusicSuppressionUnavailableOnce($"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private bool ShouldPollNativeMusicSuppression(out string inactiveReason)
    {
        if (!_nativeMusicSuppressionEnabled.Value)
        {
            inactiveReason = "native-music-suppression-disabled";
            return false;
        }

        if (!_enabled.Value)
        {
            inactiveReason = "plugin-disabled";
            return false;
        }

        if (_nativeMusicSuppressionRequiresPluginMusic.Value && !HasActivePluginMusicLane())
        {
            inactiveReason = $"no-plugin-music;lane={_activeLane}";
            return false;
        }

        inactiveReason = "poll-ready";
        return true;
    }

    private bool TrySuppressNativeMusicInstance(AudioCore audioCore, string nativeMusicLane, out string reason)
    {
        if (!ShouldSuppressNativeMusic(audioCore, nativeMusicLane, out reason))
        {
            return false;
        }

        StopNativeMusicEmitters(audioCore);
        _nativeMusicSuppressionUnavailableLogged = false;

        if (!_nativeMusicSuppressionActive && _logLaneChanges.Value)
        {
            Logger.LogInfo($"{PluginName} native music suppression active: nativeLane={CleanLogValue(nativeMusicLane)}; reason={reason}; nativeMusicSuppressed=true; nativeAmbientSuppressed={Bool(_suppressedAsylumAmbientZones.Count > 0)}.");
        }

        _nativeMusicSuppressionActive = true;
        return true;
    }

    private bool ShouldSuppressNativeMusic(AudioCore audioCore, string nativeMusicLane, out string reason)
    {
        if (!_nativeMusicSuppressionEnabled.Value)
        {
            reason = "native-music-suppression-disabled";
            return false;
        }

        if (!_enabled.Value)
        {
            reason = "plugin-disabled";
            return false;
        }

        if (_nativeMusicSuppressionRequiresPluginMusic.Value && !HasActivePluginMusicLane())
        {
            reason = $"no-plugin-music;lane={_activeLane}";
            return false;
        }

        if (TryReadScene(out SceneSnapshot scene))
        {
            string sceneLabel = SceneKeywordText(scene);
            if (MatchesKeyword(sceneLabel, _nativeMusicAllowedSceneKeywords.Value, out string sceneKeyword))
            {
                reason = $"allowed-scene;keyword={CleanLogValue(sceneKeyword)};scene={CleanLogValue(scene.SceneName)}";
                return false;
            }
        }

        string nativeEventText = NativeMusicEventText(audioCore);
        if (MatchesKeyword(nativeEventText, _nativeMusicAllowedEventKeywords.Value, out string eventKeyword))
        {
            reason = $"allowed-event;keyword={CleanLogValue(eventKeyword)};nativeLane={CleanLogValue(nativeMusicLane)}";
            return false;
        }

        reason = $"custom-music-active;lane={_activeLane};nativeLane={CleanLogValue(nativeMusicLane)}";
        return true;
    }

    private bool HasActivePluginMusicLane()
    {
        return _activeLane != MusicLane.None
            && !IsLaneMuted(_activeLane)
            && HasLaneTracks(_activeLane);
    }

    private void UpdateNativeMusicSuppressionInactive(string reason)
    {
        if (!_nativeMusicSuppressionActive)
        {
            return;
        }

        _nativeMusicSuppressionActive = false;
        _nextNativeMusicSuppressionPollTime = 0f;

        if (_logLaneChanges.Value)
        {
            Logger.LogInfo($"{PluginName} native music suppression inactive: reason={CleanLogValue(reason)}; nativeMusicSuppressed=false; nativeAmbientSuppressed={Bool(_suppressedAsylumAmbientZones.Count > 0)}.");
        }
    }

    private void StopNativeMusicEmitters(AudioCore audioCore)
    {
        audioCore.music?.Emitter.Stop(instant: true);
        audioCore.musicAlert?.Emitter.Stop(instant: true);
        audioCore.musicCombat?.Emitter.Stop(instant: true);
        audioCore.musicCombat?.Reset();
    }

    private static string NativeMusicEventText(AudioCore audioCore)
    {
        string music = NativeMusicEventText(audioCore.music);
        string alert = NativeMusicEventText(audioCore.musicAlert);
        string combat = NativeMusicEventText(audioCore.musicCombat);
        return music + "\n" + alert + "\n" + combat;
    }

    private static string NativeMusicEventText(PriorityManager? manager)
    {
        if (manager == null)
        {
            return string.Empty;
        }

        try
        {
            return manager.Emitter.EventReference.PathOrGuid ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private void LogNativeMusicSuppressionUnavailableOnce(string detail)
    {
        if (_nativeMusicSuppressionUnavailableLogged)
        {
            return;
        }

        _nativeMusicSuppressionUnavailableLogged = true;
        Logger.LogInfo($"{PluginName} native music suppression unavailable. Source={CleanLogValue(detail)}; action=will-retry; nativeMusicSuppressed={Bool(_nativeMusicSuppressionActive)}.");
    }

    private void UpdateNativeAmbientSuppression()
    {
        if (!ShouldSuppressAsylumAmbient(out string reason))
        {
            RestoreSuppressedNativeAmbientSources(reason);
            return;
        }

        float now = Time.unscaledTime;
        if (now < _nextNativeAmbientSuppressionPollTime)
        {
            return;
        }

        _nextNativeAmbientSuppressionPollTime = now + NativeAmbientSuppressionPollSeconds;
        SuppressActiveAsylumAmbientSources(reason);
    }

    private bool ShouldSuppressAsylumAmbient(out string reason)
    {
        if (!_asylumAmbientSuppressionEnabled.Value)
        {
            reason = "asylum-ambient-suppression-disabled";
            return false;
        }

        if (!_enabled.Value)
        {
            reason = "plugin-disabled";
            return false;
        }

        if (_activeLane != MusicLane.ScaryPlace)
        {
            reason = $"lane={_activeLane}";
            return false;
        }

        if (IsLaneMuted(MusicLane.ScaryPlace) || !HasLaneTracks(MusicLane.ScaryPlace))
        {
            reason = "scary-place-bucket-unavailable";
            return false;
        }

        if (!TryReadScene(out SceneSnapshot scene))
        {
            reason = "scene-unavailable";
            return false;
        }

        string sceneLabel = SceneKeywordText(scene);
        if (!MatchesKeyword(sceneLabel, _asylumAmbientSuppressionSceneKeywords.Value, out string matchedKeyword))
        {
            reason = $"scene-not-asylum;scene={CleanLogValue(scene.SceneName)}";
            return false;
        }

        reason = $"asylum-scary-place;keyword={CleanLogValue(matchedKeyword)};scene={CleanLogValue(scene.SceneName)}";
        return true;
    }

    private void SuppressActiveAsylumAmbientSources(string reason)
    {
        try
        {
            AudioCore audioCore = World.Services.Get<AudioCore>();
#if IL2CPP
            var zones = UnityEngine.Object.FindObjectsByType<ManualAudioZone>(FindObjectsSortMode.None);
#else
            ManualAudioZone[] zones = UnityEngine.Object.FindObjectsByType<ManualAudioZone>(FindObjectsSortMode.None);
#endif
            int activeZones = 0;
            int newZones = 0;
            int suppressedSources = 0;

            for (int i = 0; i < zones.Length; i++)
            {
                ManualAudioZone zone = zones[i];
                if (zone == null || !zone.isActiveAndEnabled)
                {
                    continue;
                }

                if (!TryReadManualAudioZonePlayerInside(zone, out bool playerInside))
                {
                    LogNativeAmbientSuppressionUnavailableOnce("ManualAudioZone._wasPlayerWithinZone unavailable");
                    return;
                }

                if (!playerInside)
                {
                    continue;
                }

#if IL2CPP
                Il2CppReferenceArray<IAudioSource> ambients = zone._ambientsToRegister;
#else
                IAudioSource[] ambients = zone._ambientsToRegister;
#endif
                if (ambients == null || ambients.Length == 0)
                {
                    continue;
                }

                activeZones++;
                audioCore.UnregisterAudioSources(ambients, Awaken.TG.Main.AudioSystem.AudioType.Ambient);
                suppressedSources += ambients.Length;

                if (!_suppressedAsylumAmbientZones.ContainsKey(zone))
                {
                    _suppressedAsylumAmbientZones.Add(zone, ambients);
                    newZones++;
                }
            }

            PurgeDestroyedSuppressedAmbientZones();
            _nativeAmbientSuppressionUnavailableLogged = false;

            if (_logLaneChanges.Value && (suppressedSources > 0 || _suppressedAsylumAmbientZones.Count > 0) && !_nativeAmbientSuppressionActiveLogged)
            {
                _nativeAmbientSuppressionActiveLogged = true;
                Logger.LogInfo($"{PluginName} native ambient suppression active: reason={reason}; activeZones={activeZones.ToString(CultureInfo.InvariantCulture)}; newZones={newZones.ToString(CultureInfo.InvariantCulture)}; suppressedSources={suppressedSources.ToString(CultureInfo.InvariantCulture)}; nativeMusicSuppressed={Bool(_nativeMusicSuppressionActive)}.");
            }
        }
        catch (Exception ex)
        {
            LogNativeAmbientSuppressionUnavailableOnce($"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private void RestoreSuppressedNativeAmbientSources(string reason)
    {
        if (_suppressedAsylumAmbientZones.Count == 0)
        {
            _nativeAmbientSuppressionActiveLogged = false;
            _nextNativeAmbientSuppressionPollTime = 0f;
            return;
        }

        try
        {
            AudioCore audioCore = World.Services.Get<AudioCore>();
            int restoredSources = 0;
            int skippedZones = 0;

#if IL2CPP
            foreach (KeyValuePair<ManualAudioZone, Il2CppReferenceArray<IAudioSource>> entry in _suppressedAsylumAmbientZones.ToArray())
#else
            foreach (KeyValuePair<ManualAudioZone, IAudioSource[]> entry in _suppressedAsylumAmbientZones.ToArray())
#endif
            {
                ManualAudioZone zone = entry.Key;
                if (zone == null || !zone.isActiveAndEnabled)
                {
                    skippedZones++;
                    continue;
                }

                if (TryReadManualAudioZonePlayerInside(zone, out bool playerInside) && playerInside)
                {
                    audioCore.RegisterAudioSources(entry.Value, Awaken.TG.Main.AudioSystem.AudioType.Ambient);
                    restoredSources += entry.Value.Length;
                }
                else
                {
                    skippedZones++;
                }
            }

            _suppressedAsylumAmbientZones.Clear();
            _nativeAmbientSuppressionActiveLogged = false;
            _nativeAmbientSuppressionUnavailableLogged = false;
            _nextNativeAmbientSuppressionPollTime = 0f;

            if (_logLaneChanges.Value)
            {
                Logger.LogInfo($"{PluginName} native ambient suppression restored: reason={CleanLogValue(reason)}; restoredSources={restoredSources.ToString(CultureInfo.InvariantCulture)}; skippedZones={skippedZones.ToString(CultureInfo.InvariantCulture)}; nativeMusicSuppressed={Bool(_nativeMusicSuppressionActive)}.");
            }
        }
        catch (Exception ex)
        {
            PurgeDestroyedSuppressedAmbientZones();
            LogNativeAmbientSuppressionUnavailableOnce($"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private bool TryReadManualAudioZonePlayerInside(ManualAudioZone zone, out bool playerInside)
    {
#if IL2CPP
        playerInside = zone._wasPlayerWithinZone;
        return true;
#else
        playerInside = false;
        _manualAudioZoneWasPlayerWithinZoneField ??= AccessTools.Field(typeof(ManualAudioZone), "_wasPlayerWithinZone");
        if (_manualAudioZoneWasPlayerWithinZoneField == null)
        {
            return false;
        }

        object? value = _manualAudioZoneWasPlayerWithinZoneField.GetValue(zone);
        if (value is bool inside)
        {
            playerInside = inside;
            return true;
        }

        return false;
#endif
    }

    private void PurgeDestroyedSuppressedAmbientZones()
    {
        ManualAudioZone[] zones = _suppressedAsylumAmbientZones.Keys.ToArray();
        for (int i = 0; i < zones.Length; i++)
        {
            ManualAudioZone zone = zones[i];
            if (zone == null)
            {
#pragma warning disable CS8604
                _suppressedAsylumAmbientZones.Remove(zone);
#pragma warning restore CS8604
            }
        }
    }

    private void LogNativeAmbientSuppressionUnavailableOnce(string detail)
    {
        if (_nativeAmbientSuppressionUnavailableLogged)
        {
            return;
        }

        _nativeAmbientSuppressionUnavailableLogged = true;
        Logger.LogInfo($"{PluginName} native ambient suppression unavailable. Source={CleanLogValue(detail)}; action=will-retry; nativeMusicSuppressed={Bool(_nativeMusicSuppressionActive)}.");
    }

    private void EnsureActiveLaneVoice()
    {
        if (_activeLane == MusicLane.None || IsLaneMuted(_activeLane) || !HasLaneTracks(_activeLane))
        {
            return;
        }

        for (int i = 0; i < _voices.Count; i++)
        {
            MusicVoice voice = _voices[i];
            if (voice.Lane == _activeLane && !voice.StopWhenSilent)
            {
                return;
            }
        }

        StartVoice(SelectNextTrack(_activeLane));
    }

    private void StartVoice(TrackDefinition track)
    {
        if (IsLaneMuted(track.Lane))
        {
            if (_logLaneChanges.Value)
            {
                Logger.LogInfo($"{PluginName} skipped muted music bucket: lane={track.Lane}; track={CleanLogValue(track.RelativePath)}; {NativeSuppressionLog()}.");
            }
            return;
        }

        FMOD.Channel channel = default;
        FMOD.Sound sound = default;
        try
        {
            if (_embeddedAssets == null)
            {
                throw new InvalidOperationException("Embedded music asset index is unavailable.");
            }

            byte[] wavBytes = _embeddedAssets.ReadResource(track.RelativePath, MaximumEmbeddedWavBytes);
            sound = Create2DSound(wavBytes, track.RelativePath);
            RequireFmodOk(_coreSystem.playSound(sound, _masterChannelGroup, true, out channel), $"start music track '{track.DisplayName}'");
            if (!channel.hasHandle())
            {
                throw new InvalidOperationException($"FMOD returned no playback channel for '{track.DisplayName}'.");
            }

            RequireFmodOk(channel.setVolume(0f), "set initial music volume");
            RequireFmodOk(channel.setPaused(false), "unpause music channel");
            _voices.Add(new MusicVoice(track.Lane, channel, sound, track.DisplayName, 0f, TargetVolume(track.Lane), LaneFadeSeconds(), false));
            if (_logLaneChanges.Value)
            {
                Logger.LogInfo($"{PluginName} track start: lane={track.Lane}; track={CleanLogValue(track.RelativePath)}; {NativeSuppressionLog()}.");
            }
        }
        catch (Exception ex)
        {
            StopChannelQuietly(channel);
            ReleaseSoundQuietly(sound);
            Logger.LogWarning($"{PluginName} skipped music track {track.DisplayName}: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void UpdateVoiceTargets()
    {
        foreach (MusicVoice voice in _voices)
        {
            if (!voice.StopWhenSilent)
            {
                float targetVolume = TargetVolume(voice.Lane);
                if (Math.Abs(targetVolume - voice.TargetVolume) > 0.001f)
                {
                    voice.FadeSeconds = ContextFadeSeconds();
                }

                voice.TargetVolume = targetVolume;
                if (IsLaneMuted(voice.Lane))
                {
                    voice.StopWhenSilent = true;
                    voice.FadeSeconds = LaneFadeSeconds();
                }
            }
        }
    }

    private void UpdateVoices(float deltaSeconds)
    {
        if (_voices.Count == 0)
        {
            return;
        }

        bool masterPaused = false;
        if (_masterChannelGroup.hasHandle())
        {
            FMOD.RESULT pausedResult = _masterChannelGroup.getPaused(out masterPaused);
            if (pausedResult != FMOD.RESULT.OK)
            {
                masterPaused = false;
            }
        }

        for (int i = _voices.Count - 1; i >= 0; i--)
        {
            MusicVoice voice = _voices[i];
            if (!voice.Channel.hasHandle())
            {
                ReleaseSoundQuietly(voice.Sound);
                _voices.RemoveAt(i);
                continue;
            }

            FMOD.RESULT playingResult = voice.Channel.isPlaying(out bool isPlaying);
            if (playingResult == FMOD.RESULT.OK && !isPlaying)
            {
                ReleaseSoundQuietly(voice.Sound);
                _voices.RemoveAt(i);
                continue;
            }

            if (voice.Paused != masterPaused)
            {
                voice.Channel.setPaused(masterPaused);
                voice.Paused = masterPaused;
            }

            if (masterPaused)
            {
                continue;
            }

            float fadeSeconds = Clamp(voice.FadeSeconds, 0.1f, 30f);
            float maxStep = deltaSeconds <= 0f ? 1f : deltaSeconds / fadeSeconds;
            voice.Volume = MoveTowards(voice.Volume, voice.TargetVolume, maxStep);
            voice.Channel.setVolume(voice.Volume);

            if (voice.StopWhenSilent && voice.Volume <= 0.001f)
            {
                StopChannelQuietly(voice.Channel);
                ReleaseSoundQuietly(voice.Sound);
                _voices.RemoveAt(i);
            }
        }
    }

    private void StopAllVoices()
    {
        foreach (MusicVoice voice in _voices)
        {
            StopChannelQuietly(voice.Channel);
            ReleaseSoundQuietly(voice.Sound);
        }

        _voices.Clear();
    }

    private int LoadLaneDefinitions(EmbeddedMusicAssets assets, MusicLane lane, string relativeFolder)
    {
        List<TrackDefinition> tracks = assets
            .EnumerateWavs(relativeFolder)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path => new TrackDefinition(lane, path, path))
            .ToList();

        _tracksByLane[lane] = tracks;
        _nextTrackIndexByLane[lane] = 0;
        if (tracks.Count == 0)
        {
            Logger.LogWarning($"{PluginName} found no embedded music tracks for lane {lane} in folder '{relativeFolder}'.");
        }

        return tracks.Count;
    }

    private bool HasLaneTracks(MusicLane lane)
    {
        return _tracksByLane.TryGetValue(lane, out List<TrackDefinition>? tracks) && tracks.Count > 0;
    }

    private TrackDefinition SelectNextTrack(MusicLane lane)
    {
        if (!_tracksByLane.TryGetValue(lane, out List<TrackDefinition>? tracks) || tracks.Count == 0)
        {
            throw new InvalidOperationException($"No embedded music tracks are available for lane {lane}.");
        }

        int index = _nextTrackIndexByLane.TryGetValue(lane, out int storedIndex) ? storedIndex : 0;
        if (index < 0 || index >= tracks.Count)
        {
            index = 0;
        }

        TrackDefinition selected = tracks[index];
        _nextTrackIndexByLane[lane] = (index + 1) % tracks.Count;
        return selected;
    }

    private float TargetVolume(MusicLane lane)
    {
        if (IsLaneMuted(lane))
        {
            return 0f;
        }

        float global = Clamp(_globalVolume.Value, 0f, 1f);
        float laneVolume = lane switch
        {
            MusicLane.Wyrdness => global * Clamp(_wyrdnessVolume.Value, 0f, 1f),
            MusicLane.DayOpenWorld => global * Clamp(_dayOpenWorldVolume.Value, 0f, 1f),
            MusicLane.Interior => global * Clamp(_interiorVolume.Value, 0f, 1f),
            MusicLane.Settlement => global * Clamp(_settlementVolume.Value, 0f, 1f),
            MusicLane.ScaryPlace => global * Clamp(_scaryPlaceVolume.Value, 0f, 1f),
            _ => 0f
        };

        return laneVolume * GetCurrentAudioContext().VolumeMultiplier;
    }

    private float LaneFadeSeconds()
    {
        return Clamp(_fadeSeconds.Value, 0.1f, 30f);
    }

    private float ContextFadeSeconds()
    {
        return Clamp(_contextFadeSeconds.Value, 0.1f, 10f);
    }

    private DynamicAudioContext GetCurrentAudioContext()
    {
        int frame = Time.frameCount;
        if (_contextFrame == frame)
        {
            return _contextSnapshot;
        }

        bool dialogueActive = false;
        bool combatActive = false;
        float multiplier = 1f;
        string label = "None";

        if (_contextDuckingEnabled.Value)
        {
            if (_dialogueDuckingEnabled.Value && TryReadDialogueActive())
            {
                dialogueActive = true;
                multiplier = Math.Min(multiplier, Clamp(_dialogueVolumeMultiplier.Value, 0f, 1f));
            }

            combatActive = _combatDuckingEnabled.Value && Time.unscaledTime < _combatDuckUntilTime;
            if (combatActive)
            {
                multiplier = Math.Min(multiplier, Clamp(_combatVolumeMultiplier.Value, 0f, 1f));
            }
        }

        if (dialogueActive && combatActive)
        {
            label = "Dialogue+Combat";
        }
        else if (dialogueActive)
        {
            label = "Dialogue";
        }
        else if (combatActive)
        {
            label = "Combat";
        }

        _contextSnapshot = new DynamicAudioContext(label, multiplier);
        _contextFrame = frame;
        return _contextSnapshot;
    }

    private bool TryReadDialogueActive()
    {
        try
        {
            bool active = World.HasAny<HeroDialogueInvolvement>();
            if (active)
            {
                _dialogueContextUnavailableLogged = false;
            }

            return active;
        }
        catch (Exception ex)
        {
            if (!_dialogueContextUnavailableLogged)
            {
                _dialogueContextUnavailableLogged = true;
                Logger.LogInfo($"{PluginName} dialogue context unavailable. Source={ex.GetType().Name}; action=will-retry; mutatesScene=false.");
            }

            return false;
        }
    }

    private bool IsLaneMuted(MusicLane lane)
    {
        return lane switch
        {
            MusicLane.Wyrdness => _wyrdnessMuted.Value,
            MusicLane.DayOpenWorld => _dayOpenWorldMuted.Value,
            MusicLane.Interior => _interiorMuted.Value,
            MusicLane.Settlement => _settlementMuted.Value,
            MusicLane.ScaryPlace => _scaryPlaceMuted.Value,
            _ => false
        };
    }

    private bool TryReadScene(out SceneSnapshot scene)
    {
        string sceneName = SceneManager.GetActiveScene().name ?? string.Empty;
        bool hasSceneName = !string.IsNullOrWhiteSpace(sceneName);

        try
        {
            SceneService sceneService = World.Services.Get<SceneService>();
            if (sceneService == null)
            {
                scene = new SceneSnapshot(hasSceneName, sceneName, string.Empty, false, false, false, false, false);
                return hasSceneName;
            }

            string displayName = sceneService.ActiveSceneDisplayName ?? string.Empty;
            string nativeName = sceneService.ActiveSceneRef?.Name ?? displayName;
            if (!string.IsNullOrWhiteSpace(nativeName))
            {
                sceneName = nativeName;
                hasSceneName = true;
            }

            scene = new SceneSnapshot(
                hasSceneName,
                sceneName,
                displayName,
                true,
                sceneService.AllowsWyrdnight,
                sceneService.IsPrologue,
                true,
                sceneService.IsOpenWorld);
            return true;
        }
        catch
        {
            scene = new SceneSnapshot(hasSceneName, sceneName, string.Empty, false, false, false, false, false);
            return hasSceneName;
        }
    }

    private static string SceneKeywordText(SceneSnapshot scene)
    {
        if (string.IsNullOrWhiteSpace(scene.SceneDisplayName))
        {
            return scene.SceneName;
        }

        return scene.SceneName + "\n" + scene.SceneDisplayName;
    }

    private static bool MatchesKeyword(string value, string keywords, out string matchedKeyword)
    {
        matchedKeyword = string.Empty;
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(keywords))
        {
            return false;
        }

        string[] parts = keywords.Split(new[] { ',', ';', '|', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length; i++)
        {
            string keyword = parts[i].Trim();
            if (keyword.Length == 0)
            {
                continue;
            }

            if (value.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                matchedKeyword = keyword;
                return true;
            }
        }

        return false;
    }

    private bool TryReadWyrdnessExposure(SceneSnapshot scene, out bool exposed, out string reason)
    {
        exposed = false;

        if (!scene.SceneServiceAvailable)
        {
            reason = "scene-service-unavailable";
            return false;
        }

        try
        {
            Hero hero = Hero.Current;
            if (hero == null || hero.HasBeenDiscarded)
            {
                reason = "hero-unavailable";
                return true;
            }

            bool wyrdnightValid = scene.AllowsWyrdnight && !scene.IsPrologue;
            bool heroSafeFromWyrdness = hero.IsSafeFromWyrdness;
            bool heroWyrdnightAvailable = hero.HeroWyrdNight != null;
            bool heroWyrdnightStatusActive = ReadHeroWyrdNightStatusActive(hero);

            exposed = wyrdnightValid && !heroSafeFromWyrdness && heroWyrdnightStatusActive;
            reason = $"valid={Bool(wyrdnightValid)};safe={Bool(heroSafeFromWyrdness)};heroWyrdnightAvailable={Bool(heroWyrdnightAvailable)};heroInWyrdness={Bool(heroWyrdnightStatusActive)}";
            return true;
        }
        catch (Exception ex)
        {
            reason = $"exception:{ex.GetType().Name}";
            return false;
        }
    }

    private static bool ReadHeroWyrdNightStatusActive(Hero hero)
    {
        var wyrdNight = hero.HeroWyrdNight;
        return wyrdNight != null && wyrdNight.IsHeroInWyrdness;
    }

    private bool TryReadIsNight(out bool isNight, out string source)
    {
        isNight = false;
        source = "unavailable";

#if IL2CPP
        try
        {
            GameRealTime gameTime = World.Any<GameRealTime>();
            if (gameTime == null)
            {
                source = "GameRealTime-null";
                LogGameTimeUnavailableOnce(source);
                return false;
            }

            if (gameTime.HasBeenDiscarded)
            {
                source = "GameRealTime-discarded";
                LogGameTimeUnavailableOnce(source);
                return false;
            }

            var weatherTime = gameTime.WeatherTime;
            isNight = weatherTime.IsNight;
            source = $"{weatherTime.Hour:00}:{weatherTime.Minutes:00};isNight={Bool(isNight)}";
            _gameTimeUnavailableLogged = false;
            return true;
        }
        catch (Exception ex)
        {
            source = $"exception:{ex.GetType().Name}";
            LogGameTimeUnavailableOnce(source);
            return false;
        }
#else
        if (!EnsureGameRealTimeReflection())
        {
            source = "reflection-unavailable";
            LogGameTimeUnavailableOnce(source);
            return false;
        }

        try
        {
            object? gameTime = _worldAnyGameRealTimeMethod?.Invoke(null, null);
            if (gameTime == null)
            {
                source = "GameRealTime-null";
                LogGameTimeUnavailableOnce(source);
                return false;
            }

            if (ReadBoolMember(gameTime, "HasBeenDiscarded", false))
            {
                source = "GameRealTime-discarded";
                LogGameTimeUnavailableOnce(source);
                return false;
            }

            object? weatherTime = ReadMemberValueSafe(gameTime, "WeatherTime");
            if (weatherTime == null)
            {
                source = "WeatherTime-null";
                LogGameTimeUnavailableOnce(source);
                return false;
            }

            isNight = ReadBoolMember(weatherTime, "IsNight", false);
            int hour = ReadIntMemberSafe(weatherTime, "Hour", -1);
            int minute = ReadIntMemberSafe(weatherTime, "Minutes", -1);
            source = hour >= 0 && minute >= 0
                ? $"{hour:00}:{minute:00};isNight={Bool(isNight)}"
                : $"isNight={Bool(isNight)}";
            _gameTimeUnavailableLogged = false;
            return true;
        }
        catch (Exception ex)
        {
            source = $"exception:{ex.GetType().Name}";
            LogGameTimeUnavailableOnce(source);
            return false;
        }
#endif
    }

#if !IL2CPP
    private bool EnsureGameRealTimeReflection()
    {
        if (_gameRealTimeType != null && _worldAnyGameRealTimeMethod != null)
        {
            return true;
        }

        _gameRealTimeType ??= FindLoadedType("Awaken.TG.Main.Timing.GameRealTime");
        Type? worldType = FindLoadedType("Awaken.TG.MVC.World");
        if (_gameRealTimeType == null || worldType == null)
        {
            return false;
        }

        MethodInfo? anyMethod = null;
        MethodInfo[] methods = worldType.GetMethods(BindingFlags.Public | BindingFlags.Static);
        for (int i = 0; i < methods.Length; i++)
        {
            MethodInfo method = methods[i];
            if (method.Name == "Any" && method.IsGenericMethodDefinition && method.GetParameters().Length == 0)
            {
                anyMethod = method.MakeGenericMethod(_gameRealTimeType);
                break;
            }
        }

        _worldAnyGameRealTimeMethod = anyMethod;
        return _worldAnyGameRealTimeMethod != null;
    }
#endif

    private void LogGameTimeUnavailableOnce(string source)
    {
        if (_gameTimeUnavailableLogged)
        {
            return;
        }

        _gameTimeUnavailableLogged = true;
        Logger.LogInfo($"{PluginName} waiting for game time. Source={source}; lane=DayOpenWorld; action=will-retry; mutatesScene=false.");
    }

    private FMOD.Sound Create2DSound(byte[] wavBytes, string displayPath)
    {
        FMOD.CREATESOUNDEXINFO createInfo = new FMOD.CREATESOUNDEXINFO
        {
            cbsize = Marshal.SizeOf(typeof(FMOD.CREATESOUNDEXINFO)),
            length = checked((uint)wavBytes.Length),
            suggestedsoundtype = FMOD.SOUND_TYPE.WAV
        };

        FMOD.MODE mode = FMOD.MODE.OPENMEMORY
            | FMOD.MODE.CREATESAMPLE
            | FMOD.MODE.LOOP_OFF
            | FMOD.MODE._2D;

        RequireFmodOk(_coreSystem.createSound(wavBytes, mode, ref createInfo, out FMOD.Sound sound), $"create music sound '{displayPath}'");
        if (!sound.hasHandle())
        {
            throw new InvalidOperationException($"FMOD created no music sound handle for '{displayPath}'.");
        }

        return sound;
    }

    private void ReleaseTracksQuietly()
    {
        StopAllVoices();
        _tracksByLane.Clear();
        _nextTrackIndexByLane.Clear();
        _embeddedAssets = null;
    }

    private static void StopChannelQuietly(FMOD.Channel channel)
    {
        try
        {
            if (channel.hasHandle())
            {
                channel.stop();
            }
        }
        catch
        {
            // Best-effort cleanup only.
        }
    }

    private static void ReleaseSoundQuietly(FMOD.Sound sound)
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
            // Best-effort cleanup only.
        }
    }

    private static void RequireFmodOk(FMOD.RESULT result, string action)
    {
        if (result != FMOD.RESULT.OK)
        {
            throw new InvalidOperationException($"FMOD failed to {action}: {result}");
        }
    }

    private static Type? FindLoadedType(string fullName)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            Type? type = assemblies[i].GetType(fullName, false);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    private static IEnumerable<Assembly> EmbeddedMusicAssemblies()
    {
        yield return typeof(Plugin).Assembly;
#if IL2CPP
        string pluginDirectory = Path.GetDirectoryName(typeof(Plugin).Assembly.Location)
            ?? throw new InvalidOperationException("Tainted Music plugin directory could not be resolved.");
        for (int i = 0; i < EmbeddedAssetAssemblyFiles.Length; i++)
        {
            string assetAssemblyPath = Path.Combine(pluginDirectory, EmbeddedAssetAssemblyFiles[i]);
            if (!File.Exists(assetAssemblyPath))
            {
                throw new FileNotFoundException("Required embedded Tainted Music asset shard was not found.", assetAssemblyPath);
            }

            yield return Assembly.LoadFrom(assetAssemblyPath);
        }
#else
        yield return typeof(global::TaintedMusic.Assets.Advanced.AssetPackMarker).Assembly;
#endif
    }

    private static object? ReadMemberValueSafe(object instance, string memberName)
    {
        Type type = instance.GetType();
        PropertyInfo? property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
        if (property != null && property.GetIndexParameters().Length == 0)
        {
            return property.GetValue(instance, null);
        }

        FieldInfo? field = type.GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
        return field?.GetValue(instance);
    }

    private static bool ReadBoolMember(object instance, string memberName, bool fallback)
    {
        object? value = ReadMemberValueSafe(instance, memberName);
        if (value is bool boolValue)
        {
            return boolValue;
        }

        return fallback;
    }

    private static int ReadIntMemberSafe(object instance, string memberName, int fallback)
    {
        object? value = ReadMemberValueSafe(instance, memberName);
        return value switch
        {
            int intValue => intValue,
            short shortValue => shortValue,
            byte byteValue => byteValue,
            _ => fallback
        };
    }

    private static float MoveTowards(float current, float target, float maxDelta)
    {
        if (Math.Abs(target - current) <= maxDelta)
        {
            return target;
        }

        return current + Math.Sign(target - current) * maxDelta;
    }

    private static float Clamp(float value, float min, float max)
    {
        if (value < min)
        {
            return min;
        }

        return value > max ? max : value;
    }

    private static string Bool(bool value) => value ? "true" : "false";

    private string NativeSuppressionLog() => $"nativeMusicSuppressed={Bool(_nativeMusicSuppressionActive)};nativeAmbientSuppressed={Bool(_suppressedAsylumAmbientZones.Count > 0)}";

    private static string FormatFloat(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            return "0";
        }

        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string CleanLogValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        return value.Replace(';', '_').Replace('\r', '_').Replace('\n', '_');
    }

    internal MusicMenuSnapshot BuildMusicMenuSnapshot()
    {
        string statusLevel;
        string summary;
        string detail;

        if (_startupFailed)
        {
            statusLevel = "Problem";
            summary = "Music playback failed to initialize.";
            detail = "Tainted Music failed closed; inspect the BepInEx log for the startup error.";
        }
        else if (!_enabled.Value)
        {
            statusLevel = "Info";
            summary = "Tainted Music is disabled.";
            detail = "Enable Playback to resume the custom music system.";
        }
        else if (!_runtimeReady)
        {
            statusLevel = "Info";
            summary = "Waiting for the FoA audio runtime.";
            detail = "The music library will initialize when FMOD is ready.";
        }
        else if (_activeLane == MusicLane.None)
        {
            statusLevel = "Ok";
            summary = "Ready; waiting for a supported music lane.";
            detail = "Title, loading, native-night, and non-playable hero states intentionally remain silent.";
        }
        else
        {
            statusLevel = "Ok";
            summary = $"Playing {DisplayLane(_activeLane)} through Tainted Music.";
            detail = "Use the Playback, Music Mix, and Dynamics tabs in this Tainted Interface menu.";
        }

        string currentTrack = "None";
        for (int i = 0; i < _voices.Count; i++)
        {
            MusicVoice voice = _voices[i];
            if (voice.Lane == _activeLane && !voice.StopWhenSilent)
            {
                currentTrack = Path.GetFileNameWithoutExtension(voice.DisplayName.Replace('/', Path.DirectorySeparatorChar));
                break;
            }
        }

        int indexedTracks = _tracksByLane.Values.Sum(tracks => tracks.Count);
        string sceneName = SceneManager.GetActiveScene().name;
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            sceneName = "Unavailable";
        }

        return new MusicMenuSnapshot
        {
            UpdatedUtc = DateTime.UtcNow,
            StatusLevel = statusLevel,
            Summary = summary,
            Detail = detail,
            Lines = new[]
            {
                $"Lane: {DisplayLane(_activeLane)}",
                $"Track: {currentTrack}",
                $"Scene: {sceneName}",
                $"Context: {_contextSnapshot.Label}",
                $"Master volume: {Math.Round(Clamp(_globalVolume.Value, 0f, 1f) * 100f).ToString(CultureInfo.InvariantCulture)}%",
                $"Library: {indexedTracks.ToString(CultureInfo.InvariantCulture)} / 144 tracks indexed",
                $"Native music: {(_nativeMusicSuppressionActive ? "Suppressed" : "Available")}",
                "Settings: Separate Tainted Interface menu"
            }
        };
    }

    private static string DisplayLane(MusicLane lane)
    {
        return lane switch
        {
            MusicLane.Wyrdness => "Wyrdness",
            MusicLane.DayOpenWorld => "Day Open World",
            MusicLane.Interior => "Interior",
            MusicLane.Settlement => "Settlement",
            MusicLane.ScaryPlace => "Scary Place",
            _ => "None"
        };
    }

    private static ConfigDescription Ui(
        string description,
        string displaySection,
        string displayName,
        int sectionOrder,
        int order,
        AcceptableValueBase? acceptableValues = null,
        bool hidden = false)
    {
        return new ConfigDescription(
            description,
            acceptableValues,
            new SettingUiMetadata(displaySection, displayName, sectionOrder, order, hidden));
    }

    private enum MusicLane
    {
        None,
        Wyrdness,
        DayOpenWorld,
        Interior,
        Settlement,
        ScaryPlace
    }

    private sealed class TrackDefinition
    {
        internal TrackDefinition(MusicLane lane, string relativePath, string displayName)
        {
            Lane = lane;
            RelativePath = relativePath;
            DisplayName = displayName;
        }

        internal MusicLane Lane { get; }
        internal string RelativePath { get; }
        internal string DisplayName { get; }
    }

    private sealed class MusicVoice
    {
        internal MusicVoice(MusicLane lane, FMOD.Channel channel, FMOD.Sound sound, string displayName, float volume, float targetVolume, float fadeSeconds, bool stopWhenSilent)
        {
            Lane = lane;
            Channel = channel;
            Sound = sound;
            DisplayName = displayName;
            Volume = volume;
            TargetVolume = targetVolume;
            FadeSeconds = fadeSeconds;
            StopWhenSilent = stopWhenSilent;
        }

        internal MusicLane Lane { get; }
        internal FMOD.Channel Channel { get; }
        internal FMOD.Sound Sound { get; }
        internal string DisplayName { get; }
        internal float Volume { get; set; }
        internal float TargetVolume { get; set; }
        internal float FadeSeconds { get; set; }
        internal bool StopWhenSilent { get; set; }
        internal bool Paused { get; set; }
    }

    private readonly struct DynamicAudioContext
    {
        internal DynamicAudioContext(string label, float volumeMultiplier)
        {
            Label = label;
            VolumeMultiplier = volumeMultiplier;
        }

        internal string Label { get; }
        internal float VolumeMultiplier { get; }
    }

    private readonly struct SceneSnapshot
    {
        internal SceneSnapshot(
            bool hasScene,
            string sceneName,
            string sceneDisplayName,
            bool sceneServiceAvailable,
            bool allowsWyrdnight,
            bool isPrologue,
            bool openWorldKnown,
            bool isOpenWorld)
        {
            HasScene = hasScene;
            SceneName = sceneName;
            SceneDisplayName = sceneDisplayName;
            SceneServiceAvailable = sceneServiceAvailable;
            AllowsWyrdnight = allowsWyrdnight;
            IsPrologue = isPrologue;
            OpenWorldKnown = openWorldKnown;
            IsOpenWorld = isOpenWorld;
        }

        internal bool HasScene { get; }
        internal string SceneName { get; }
        internal string SceneDisplayName { get; }
        internal bool SceneServiceAvailable { get; }
        internal bool AllowsWyrdnight { get; }
        internal bool IsPrologue { get; }
        internal bool OpenWorldKnown { get; }
        internal bool IsOpenWorld { get; }
    }

    private sealed class EmbeddedMusicAssets
    {
        private readonly Dictionary<string, EmbeddedResourceLocation> _resourceByRelativePath;

        private EmbeddedMusicAssets(Dictionary<string, EmbeddedResourceLocation> resourceByRelativePath)
        {
            _resourceByRelativePath = resourceByRelativePath;
        }

        internal int ResourceCount => _resourceByRelativePath.Count;

        internal static EmbeddedMusicAssets? TryCreate(IEnumerable<Assembly> assemblies)
        {
            Dictionary<string, EmbeddedResourceLocation> resources = new Dictionary<string, EmbeddedResourceLocation>(StringComparer.OrdinalIgnoreCase);
            foreach (Assembly assembly in assemblies)
            {
                foreach (string resourceName in assembly.GetManifestResourceNames())
                {
                    if (!resourceName.StartsWith(EmbeddedAssetPrefix, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string relativePath = NormalizeEmbeddedPath(resourceName.Substring(EmbeddedAssetPrefix.Length));
                    if (relativePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                    {
                        resources[relativePath] = new EmbeddedResourceLocation(assembly, resourceName);
                    }
                }
            }

            return resources.Count == 0 ? null : new EmbeddedMusicAssets(resources);
        }

        internal byte[] ReadResource(string relativePath, int maximumBytes)
        {
            string normalizedPath = NormalizeEmbeddedPath(relativePath);
            if (!_resourceByRelativePath.TryGetValue(normalizedPath, out EmbeddedResourceLocation location))
            {
                throw new FileNotFoundException("Embedded Tainted Music WAV does not exist.", normalizedPath);
            }

            using Stream? resourceStream = location.Assembly.GetManifestResourceStream(location.ResourceName);
            if (resourceStream == null)
            {
                throw new FileNotFoundException("Embedded Tainted Music WAV stream could not be opened.", normalizedPath);
            }

            return ReadStreamBounded(resourceStream, normalizedPath, maximumBytes);
        }

        internal IEnumerable<string> EnumerateWavs(string relativeFolder)
        {
            string normalizedFolder = NormalizeEmbeddedPath(relativeFolder).Trim('/');
            string prefix = normalizedFolder.Length == 0 ? string.Empty : normalizedFolder + "/";
            foreach (string relativePath in _resourceByRelativePath.Keys)
            {
                if (relativePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    && relativePath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                {
                    yield return relativePath;
                }
            }
        }

        private static byte[] ReadStreamBounded(Stream stream, string displayPath, int maximumBytes)
        {
            if (stream.CanSeek && (stream.Length <= 0 || stream.Length > maximumBytes))
            {
                throw new InvalidDataException($"Embedded Tainted Music WAV '{displayPath}' length {stream.Length.ToString(CultureInfo.InvariantCulture)} is outside 1..{maximumBytes.ToString(CultureInfo.InvariantCulture)} bytes.");
            }

            using MemoryStream memory = new MemoryStream(stream.CanSeek ? checked((int)stream.Length) : 0);
            stream.CopyTo(memory);
            if (memory.Length <= 0 || memory.Length > maximumBytes)
            {
                throw new InvalidDataException($"Embedded Tainted Music WAV '{displayPath}' length {memory.Length.ToString(CultureInfo.InvariantCulture)} is outside 1..{maximumBytes.ToString(CultureInfo.InvariantCulture)} bytes.");
            }

            return memory.ToArray();
        }

        private static string NormalizeEmbeddedPath(string relativePath)
        {
            return relativePath.Replace('\\', '/').TrimStart('/');
        }

        private readonly struct EmbeddedResourceLocation
        {
            internal EmbeddedResourceLocation(Assembly assembly, string resourceName)
            {
                Assembly = assembly;
                ResourceName = resourceName;
            }

            internal Assembly Assembly { get; }
            internal string ResourceName { get; }
        }
    }
}

internal sealed class SettingUiMetadata
{
    internal SettingUiMetadata(
        string displaySection,
        string displayName,
        int sectionOrder,
        int order,
        bool hidden)
    {
        DisplaySection = displaySection;
        DisplayName = displayName;
        SectionOrder = sectionOrder;
        Order = order;
        Hidden = hidden;
    }

    public string DisplaySection { get; }

    public string DisplayName { get; }

    public int SectionOrder { get; }

    public int Order { get; }

    public bool Hidden { get; }
}

internal static class CombatDuckingDamagePatch
{
    private static bool _failureLogged;

    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? targetMethod = AccessTools.Method(typeof(HealthElement), nameof(HealthElement.TakeDamage));
        MethodInfo? postfixMethod = AccessTools.Method(typeof(CombatDuckingDamagePatch), nameof(Postfix));
        if (targetMethod == null || postfixMethod == null)
        {
            logger.LogWarning($"{Plugin.PluginName}: HealthElement.TakeDamage target was not found; contextual combat ducking is disabled.");
            return;
        }

        harmony.Patch(targetMethod, postfix: new HarmonyMethod(postfixMethod));
        logger.LogInfo($"{Plugin.PluginName}: patched HealthElement.TakeDamage for contextual combat ducking.");
    }

    private static void Postfix(Damage damage)
    {
        try
        {
            Plugin.RecordCombatImpact(damage);
        }
        catch (Exception ex)
        {
            if (_failureLogged)
            {
                return;
            }

            _failureLogged = true;
            Plugin.RecordCombatImpactFailure(ex);
        }
    }
}

internal static class NativeMusicSuppressionPatch
{
    private static bool _failureLogged;

    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        PatchMusicMethod(harmony, logger, "PlayExplorationMusic", nameof(PlayExplorationMusicPrefix));
        PatchMusicMethod(harmony, logger, "PlayAlertMusic", nameof(PlayAlertMusicPrefix));
        PatchMusicMethod(harmony, logger, "PlayCombatMusic", nameof(PlayCombatMusicPrefix));
    }

    private static void PatchMusicMethod(Harmony harmony, ManualLogSource logger, string targetName, string prefixName)
    {
        MethodInfo? targetMethod = AccessTools.Method(typeof(AudioCore), targetName);
        MethodInfo? prefixMethod = AccessTools.Method(typeof(NativeMusicSuppressionPatch), prefixName);
        if (targetMethod == null || prefixMethod == null)
        {
            logger.LogWarning($"{Plugin.PluginName}: AudioCore.{targetName} target was not found; native music suppression cannot intercept that lane.");
            return;
        }

        harmony.Patch(targetMethod, prefix: new HarmonyMethod(prefixMethod));
        logger.LogInfo($"{Plugin.PluginName}: patched AudioCore.{targetName} for native music suppression.");
    }

    private static bool PlayExplorationMusicPrefix(AudioCore __instance, ref bool __result)
    {
        return Prefix(__instance, "exploration", ref __result);
    }

    private static bool PlayAlertMusicPrefix(AudioCore __instance, ref bool __result)
    {
        return Prefix(__instance, "alert", ref __result);
    }

    private static bool PlayCombatMusicPrefix(AudioCore __instance, ref bool __result)
    {
        return Prefix(__instance, "combat", ref __result);
    }

    private static bool Prefix(AudioCore audioCore, string nativeMusicLane, ref bool result)
    {
        try
        {
            if (Plugin.TrySuppressNativeMusic(audioCore, nativeMusicLane, out _))
            {
                result = false;
                return false;
            }
        }
        catch (Exception ex)
        {
            if (!_failureLogged)
            {
                _failureLogged = true;
                Plugin.RecordNativeMusicSuppressionPatchFailure(ex);
            }
        }

        return true;
    }
}
