using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Awaken.TG.Assets;
using Awaken.TG.MVC;
using Awaken.TG.Main.General.StatTypes;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.CharacterCreators.PresetSelection;
using Awaken.TG.Main.Heroes.Items;
using Awaken.TG.Main.Heroes.Stats;
using Awaken.TG.Main.Stories.Choices;
using Awaken.TG.Main.Templates;
using Awaken.TG.Main.UI.TitleScreen;
using Awaken.Utility.Collections;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
#if IL2CPP
using Awaken.TG.Main.Utility;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#endif

namespace ImmersiveBackgrounds;

#if !IL2CPP
[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
#else
public sealed class Plugin
#endif
{
    public const string PluginGuid = "kane.tgfoa.immersive-backgrounds";
    public const string PluginName = "Immersive Backgrounds";
#if IL2CPP
    public const string PluginVersion = "0.5.3";
#else
    public const string PluginVersion = "0.4.9";
#endif

    private const string PrologueJailSceneName = "Prologue_Jail";
    private const string NaturesCaressTemplateGuid = "aed414c58006a644092272d0c56dc052";
    private const string PreviewGroupName = "LIFE BEFORE CAPTURE";
    private const float ProficiencyBaseValue = 10f;
    private const float NativeStaySilentProficiencyBaseValue = 19f;

    private static readonly StatType[] RpgStatTypes =
    {
        HeroRPGStatType.Strength,
        HeroRPGStatType.Dexterity,
        HeroRPGStatType.Spirituality,
        HeroRPGStatType.Perception,
        HeroRPGStatType.Endurance,
        HeroRPGStatType.Practicality
    };

    private static readonly BackgroundDefinition[] Backgrounds =
    {
        new(
            BackgroundId.Army,
            new[] { "enlist in the army" },
            "Muster-Bound Soldier",
            "Before chains found you, discipline had already taken root: drilled hands, hard roads, and the weight of mail.",
            () => GetSafeIcon(ProfStatType.HeavyArmor)),
        new(
            BackgroundId.Hunter,
            new[] { "hunting" },
            "Forest-Bred Hunter",
            "You knew the forest as shelter, larder, and warning bell. Every track taught patience.",
            () => GetSafeIcon(ProfStatType.Archery)),
        new(
            BackgroundId.Outlaw,
            new[] { "avoiding the city guards" },
            "Back-Alley Runner",
            "Alleys and market crowds taught you to move unseen, take only what you could carry, and leave no name behind.",
            () => GetSafeIcon(ProfStatType.Sneak)),
        new(
            BackgroundId.Worship,
            new[] { "ancient site of worship" },
            "Wyrd-Seeking Pilgrim",
            "You followed weathered stones and half-buried rites, learning where faith ends and the Wyrd begins.",
            () => GetSafeIcon(ProfStatType.Magic)),
        new(
            BackgroundId.Silent,
            new[] { "stay silent" },
            "The Silent One",
            "You offer the guard nothing. Whatever life came before the cell, you keep it buried behind your teeth.",
            () => GetSafeIcon(ProfStatType.HeavyArmor))
    };

    private static Plugin? s_instance;

    private readonly HashSet<string> _naturesCaressGrantedHeroes = new(StringComparer.Ordinal);
    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<bool> _replaceBackgroundRewards = null!;
    private ConfigEntry<RewardPreset> _rewardPreset = null!;
    private ConfigEntry<bool> _showChoicePreview = null!;
    private Harmony? _harmony;
    private bool _beginningStartActive;
    private BackgroundId _pendingBackground = BackgroundId.None;
    private DynamicChoiceDefinition? _pendingDynamicChoice;
    private string? _pendingDirectSilentPreviewName;
    private int _appliedRewardPackageCount;

    public enum RewardPreset
    {
        Sparse,
        ImmersiveOverhaul,
        Generous
    }

#if IL2CPP
    private ConfigFile? _il2CppConfig;
    private BepInEx.Logging.ManualLogSource? _il2CppLogger;

    [HideFromIl2Cpp]
    private ConfigFile Config =>
        _il2CppConfig ?? throw new InvalidOperationException("Immersive Backgrounds IL2CPP config was not initialized.");

    [HideFromIl2Cpp]
    private BepInEx.Logging.ManualLogSource Logger =>
        _il2CppLogger ?? throw new InvalidOperationException("Immersive Backgrounds IL2CPP logger was not initialized.");

    [HideFromIl2Cpp]
    internal void Initialize(ConfigFile config, BepInEx.Logging.ManualLogSource logger)
    {
        _il2CppConfig = config;
        _il2CppLogger = logger;
        InitializeCore();
    }

    [HideFromIl2Cpp]
    internal void Shutdown()
    {
        ShutdownCore();
    }
#else
    private void Awake()
    {
        InitializeCore();
    }

    private void OnDestroy()
    {
        ShutdownCore();
    }
#endif

    private void InitializeCore()
    {
        s_instance = this;
        _enabled = Config.Bind("General", "Enabled", true, "Enable the Immersive Backgrounds reward rewrite.");
        _replaceBackgroundRewards = Config.Bind("General", "ReplaceBackgroundRewards", true, "Replace the prison background and follow-up playstyle rewards with the selected preset.");
        _rewardPreset = Config.Bind("General", "RewardPreset", RewardPreset.ImmersiveOverhaul, "Controls how generous the starting rewards are: Sparse is harsher, ImmersiveOverhaul is the intended roleplay baseline, and Generous is easier.");
        _showChoicePreview = Config.Bind("General", "ShowChoicePreview", true, "Show native hover previews that describe the new background rewards before selection.");

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);

        Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
    }

    private void ShutdownCore()
    {
        _harmony?.UnpatchSelf();
        _harmony = null;
        if (ReferenceEquals(s_instance, this))
        {
            s_instance = null;
        }
    }

    internal static void CaptureStartGameData(StartGameData data)
    {
        if (s_instance == null || !s_instance._enabled.Value)
        {
            return;
        }

        string sceneName = data?.sceneReference?.Name ?? string.Empty;
        bool isBeginningStart = data?.withHeroCreation == true
            && string.Equals(sceneName, PrologueJailSceneName, StringComparison.OrdinalIgnoreCase);

        s_instance._beginningStartActive = isBeginningStart;
        s_instance._pendingBackground = BackgroundId.None;
        s_instance._pendingDynamicChoice = null;
        s_instance._pendingDirectSilentPreviewName = null;
        s_instance._appliedRewardPackageCount = 0;
        if (isBeginningStart)
        {
            s_instance.Logger.LogInfo("Immersive Backgrounds armed for the prologue jail background choice.");
        }
    }

    internal static void CaptureBackgroundChoiceSelection(Choice choice)
    {
        Plugin? instance = s_instance;
        if (instance == null
            || !instance._enabled.Value
            || !instance._replaceBackgroundRewards.Value
            || !instance._beginningStartActive
            || choice == null)
        {
            return;
        }

        bool dynamicChoice = false;
        BackgroundDefinition? selectedDefinition = null;
        DynamicChoiceDefinition? selectedDynamicChoice = null;
        if (TryFindBackground(choice.ButtonText, out BackgroundDefinition? definition))
        {
            selectedDefinition = definition!;
        }
        else if (instance.TryCreateDynamicChoiceDefinition(choice.ButtonText, GetChoiceHoverInfos(choice), out DynamicChoiceDefinition? dynamicDefinition))
        {
            dynamicChoice = true;
            selectedDynamicChoice = dynamicDefinition!;
        }
        else
        {
            return;
        }

        BackgroundId backgroundId = selectedDefinition?.Id ?? BackgroundId.None;
        string previewName = selectedDefinition?.PreviewName ?? selectedDynamicChoice?.PreviewName ?? "Playstyle Bonus";
        if (dynamicChoice)
        {
            instance._pendingDynamicChoice = selectedDynamicChoice;
            instance._pendingDirectSilentPreviewName = null;
            instance.Logger.LogInfo($"Captured prologue secondary choice: {previewName}.");
            return;
        }

        instance._pendingBackground = backgroundId;
        instance._pendingDirectSilentPreviewName = backgroundId == BackgroundId.Silent ? previewName : null;
        instance.Logger.LogInfo($"Captured prologue background choice: {previewName}.");
    }

    internal static void CompleteBackgroundChoiceSelection()
    {
        Plugin? instance = s_instance;
        string? previewName = instance?._pendingDirectSilentPreviewName;
        if (instance == null || string.IsNullOrEmpty(previewName))
        {
            return;
        }

        instance._pendingDirectSilentPreviewName = null;
        instance.TryApplySilentBackgroundDirectly(previewName);
    }

    private static StructList<IHoverInfo> GetChoiceHoverInfos(Choice choice)
    {
#if IL2CPP
        return choice._HoverInfos_k__BackingField;
#else
        return choice.HoverInfos;
#endif
    }

#if IL2CPP
    internal static bool TryReplaceBackgroundStats(Hero hero, Il2CppReferenceArray<StatPreset> stats, bool ignoreLevelSetting, out bool result)
#else
    internal static bool TryReplaceBackgroundStats(Hero hero, StatPreset[] stats, bool ignoreLevelSetting, out bool result)
#endif
    {
        result = false;
        if (s_instance == null || !s_instance._enabled.Value)
        {
            return false;
        }

        return s_instance.TryReplaceBackgroundStatsInstance(hero, stats, ignoreLevelSetting, out result);
    }

    internal static void TryReplaceBackgroundHoverInfos(Choice choice, ref StructList<IHoverInfo> hoverInfos)
    {
        if (s_instance == null || !s_instance._enabled.Value)
        {
            return;
        }

        s_instance.TryReplaceBackgroundHoverInfosInstance(choice, ref hoverInfos);
    }

#if IL2CPP
    private bool TryReplaceBackgroundStatsInstance(Hero hero, Il2CppReferenceArray<StatPreset> stats, bool ignoreLevelSetting, out bool result)
#else
    private bool TryReplaceBackgroundStatsInstance(Hero hero, StatPreset[] stats, bool ignoreLevelSetting, out bool result)
#endif
    {
        result = false;
        if (!_replaceBackgroundRewards.Value
            || !_beginningStartActive
            || hero == null
            || hero.HasBeenDiscarded)
        {
            return false;
        }

        BackgroundId backgroundId = _pendingBackground;
        DynamicChoiceDefinition? dynamicChoice = _pendingDynamicChoice;
        BackgroundDefinition? definition = null;
        bool matchedStaySilentFallback = false;
        if (backgroundId == BackgroundId.None && dynamicChoice == null)
        {
            if (!MatchesStaySilentNoClassPackage(stats))
            {
                return false;
            }

            backgroundId = BackgroundId.Silent;
            matchedStaySilentFallback = true;
        }

        if (backgroundId != BackgroundId.None && !TryGetBackground(backgroundId, out definition))
        {
            return false;
        }

        if (dynamicChoice == null && definition == null)
        {
            return false;
        }

        bool applyPendingBackgroundWithDynamicChoice = dynamicChoice != null
            && definition != null
            && _appliedRewardPackageCount == 0;
        string previewName = dynamicChoice?.PreviewName ?? definition!.PreviewName;
        if (applyPendingBackgroundWithDynamicChoice)
        {
            previewName = $"{definition!.PreviewName} + {dynamicChoice!.PreviewName}";
        }

        if (!LooksLikeBackgroundStatPackage(stats))
        {
            if (!matchedStaySilentFallback)
            {
                Logger.LogWarning($"Expected a background stat package after selecting {previewName}, but the next stat package did not match; vanilla stats will continue.");
                _pendingBackground = BackgroundId.None;
                _pendingDynamicChoice = null;
                _pendingDirectSilentPreviewName = null;
                if (_appliedRewardPackageCount == 0)
                {
                    _beginningStartActive = false;
                }
            }

            return false;
        }

        try
        {
            RewardProfile profile;
            if (dynamicChoice == null)
            {
                profile = CreateRewardProfile(backgroundId, _rewardPreset.Value);
            }
            else
            {
                RewardProfile dynamicProfile = CreateRewardProfileFromStatPackage(stats, _rewardPreset.Value);
                profile = applyPendingBackgroundWithDynamicChoice
                    ? MergeRewardProfiles(CreateRewardProfile(backgroundId, _rewardPreset.Value), dynamicProfile)
                    : dynamicProfile;
            }

            result = ApplyLevelRows(hero, stats, ignoreLevelSetting);
            ApplyRewards(hero, profile);
            if (profile.GrantNaturesCaress)
            {
                GrantNaturesCaress(hero);
            }

            hero.RestoreStats();
            _pendingBackground = BackgroundId.None;
            _pendingDynamicChoice = null;
            _pendingDirectSilentPreviewName = null;
            _appliedRewardPackageCount++;
            if (dynamicChoice != null || backgroundId == BackgroundId.Silent || _appliedRewardPackageCount >= 2)
            {
                _beginningStartActive = false;
            }

            Logger.LogInfo($"Applied {PresetDisplayName(_rewardPreset.Value)} preset to {previewName}.");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Background reward replacement failed; vanilla stat application will continue. {ex.GetType().Name}: {ex.Message}");
            result = false;
            return false;
        }
    }

    private void TryApplySilentBackgroundDirectly(string previewName)
    {
        if (!_replaceBackgroundRewards.Value
            || !_beginningStartActive
            || _pendingBackground != BackgroundId.Silent)
        {
            return;
        }

        Hero hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            Logger.LogWarning($"Captured {previewName}, but Hero.Current was unavailable for direct stay-silent reward application.");
            return;
        }

        try
        {
            RewardProfile profile = CreateRewardProfile(BackgroundId.Silent, _rewardPreset.Value);
            ApplyRewards(hero, profile);
            if (profile.GrantNaturesCaress)
            {
                GrantNaturesCaress(hero);
            }

            hero.RestoreStats();
            _pendingBackground = BackgroundId.None;
            _pendingDynamicChoice = null;
            _pendingDirectSilentPreviewName = null;
            _appliedRewardPackageCount++;
            if (_appliedRewardPackageCount >= 2)
            {
                _beginningStartActive = false;
            }

            Logger.LogInfo($"Applied {PresetDisplayName(_rewardPreset.Value)} preset to {previewName} through direct stay-silent selection fallback.");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Direct stay-silent reward application failed. {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void TryReplaceBackgroundHoverInfosInstance(Choice choice, ref StructList<IHoverInfo> hoverInfos)
    {
        if (!_showChoicePreview.Value
            || !_replaceBackgroundRewards.Value
            || !_beginningStartActive
            || choice == null)
        {
            return;
        }

        if (TryFindBackground(choice.ButtonText, out BackgroundDefinition? definition))
        {
            hoverInfos = CreateBackgroundHoverInfos(definition!, _rewardPreset.Value);
            Logger.LogInfo($"Replaced prologue choice preview: {definition!.PreviewName}.");
            return;
        }

        if (TryCreateDynamicChoiceDefinition(choice.ButtonText, hoverInfos, out DynamicChoiceDefinition? dynamicDefinition))
        {
            hoverInfos = CreateDynamicChoiceHoverInfos(dynamicDefinition!, _rewardPreset.Value);
            Logger.LogInfo($"Replaced prologue secondary preview: {dynamicDefinition!.PreviewName}.");
        }
    }

    private static StructList<IHoverInfo> CreateBackgroundHoverInfos(BackgroundDefinition definition, RewardPreset preset)
    {
        RewardProfile profile = CreateRewardProfile(definition.Id, preset);
        StructList<IHoverInfo> hoverInfos = new StructList<IHoverInfo>(0);
        hoverInfos.Add(CreateHoverInfo(
            PreviewGroupName,
            $"{definition.PreviewName} ({PresetDisplayName(preset)})",
            definition.BackgroundText,
            definition.CreateIcon()));

        if (definition.Id == BackgroundId.Silent)
        {
            hoverInfos.Add(CreateAllAttributesHoverInfo(profile));
        }

        foreach (RewardEntry reward in profile.Rewards)
        {
            if (definition.Id == BackgroundId.Silent && reward.Kind == RewardKind.RpgStat)
            {
                continue;
            }

            hoverInfos.Add(CreateRewardHoverInfo(reward));
        }

        if (profile.GrantNaturesCaress)
        {
            hoverInfos.Add(CreateHoverInfo(
                PreviewGroupName,
                "Nature's Caress",
                "Something gentle and strange stayed with you.",
                GetSafeIcon(ProfStatType.Magic)));
        }

        return hoverInfos;
    }

    private static IHoverInfo CreateAllAttributesHoverInfo(RewardProfile profile)
    {
        float amount = profile.Rewards
            .Where(reward => reward.Kind == RewardKind.RpgStat)
            .Select(reward => reward.Amount)
            .DefaultIfEmpty(0f)
            .First();

        return CreateHoverInfo(
            PreviewGroupName,
            $"All core attributes {FormatBonus(amount)}",
            "Strength, Dexterity, Spirituality, Perception, Endurance, and Practicality begin from the life you refuse to explain.",
            GetSafeIcon(HeroRPGStatType.Strength));
    }

    private static IHoverInfo CreateRewardHoverInfo(RewardEntry reward)
    {
        return CreateHoverInfo(
            PreviewGroupName,
            $"{reward.DisplayName} {FormatBonus(reward.Amount)}",
            reward.Description,
            GetSafeIcon(reward.StatType));
    }

    private static IHoverInfo CreateHoverInfo(
        string groupName,
        string name,
        string description,
        ShareableSpriteReference icon)
    {
#if IL2CPP
        ProficiencyHoverInfo native = new(groupName, ProfStatType.HeavyArmor, new StatValue(0f))
        {
            InfoGroupName = groupName,
            InfoName = name,
            InfoDescription = description,
            InfoIcon = icon
        };
        return new IHoverInfo(native.Pointer);
#else
        return new ImmersiveBackgroundsHoverInfo
        {
            InfoGroupName = groupName,
            InfoName = name,
            InfoDescription = description,
            InfoIcon = icon
        };
#endif
    }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    private StructList<IHoverInfo> CreateDynamicChoiceHoverInfos(DynamicChoiceDefinition definition, RewardPreset preset)
    {
        StructList<IHoverInfo> hoverInfos = new StructList<IHoverInfo>(0);
        hoverInfos.Add(CreateHoverInfo(
            PreviewGroupName,
            $"{definition.PreviewName} ({PresetDisplayName(preset)})",
            definition.BackgroundText,
            definition.CreateIcon()));

        foreach (RewardEntry reward in definition.Rewards)
        {
            hoverInfos.Add(CreateRewardHoverInfo(reward));
        }

        foreach (IHoverInfo passthrough in definition.PassthroughHoverInfos)
        {
            hoverInfos.Add(passthrough);
        }

        return hoverInfos;
    }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    private bool TryCreateDynamicChoiceDefinition(string? buttonText, StructList<IHoverInfo> nativeHoverInfos, out DynamicChoiceDefinition? definition)
    {
        definition = null;
        if (!CanCreateDynamicChoiceDefinition(buttonText))
        {
            return false;
        }

        List<RewardEntry> rewards = new();
        List<IHoverInfo> passthroughHoverInfos = new();
        HashSet<StatType> seenStats = new();
        for (int i = 0; i < nativeHoverInfos.Count; i++)
        {
            IHoverInfo hoverInfo = nativeHoverInfos[i];
            if (TryParseHoverStat(hoverInfo, out StatType? statType) && statType != null && seenStats.Add(statType))
            {
                rewards.Add(CreateDynamicReward(statType, _rewardPreset.Value));
                continue;
            }

            passthroughHoverInfos.Add(hoverInfo);
        }

        if (rewards.Count == 0)
        {
            return false;
        }

        DynamicChoiceTone tone = DynamicChoiceToneFor(rewards);
        definition = new DynamicChoiceDefinition(
            DynamicPreviewName(tone),
            DynamicBackgroundText(tone),
            () => GetSafeIcon(rewards[0].StatType),
            rewards.ToArray(),
            passthroughHoverInfos.ToArray());
        return true;
    }

    private bool CanCreateDynamicChoiceDefinition(string? buttonText)
    {
        if (_appliedRewardPackageCount >= 2 || TryFindBackground(buttonText, out _))
        {
            return false;
        }

        return _appliedRewardPackageCount > 0 || _pendingBackground != BackgroundId.None;
    }

    private static RewardProfile MergeRewardProfiles(RewardProfile first, RewardProfile second)
    {
        List<RewardEntry> rewards = new(first.Rewards);
        foreach (RewardEntry reward in second.Rewards)
        {
            int existingIndex = rewards.FindIndex(existing => existing.StatType == reward.StatType);
            if (existingIndex >= 0)
            {
                rewards[existingIndex] = reward;
                continue;
            }

            rewards.Add(reward);
        }

        return new RewardProfile(rewards.ToArray(), first.GrantNaturesCaress || second.GrantNaturesCaress);
    }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    private void ApplyRewards(Hero hero, RewardProfile profile)
    {
        foreach (RewardEntry reward in profile.Rewards)
        {
            SetStat(hero, reward);
        }
    }

#if IL2CPP
    private static RewardProfile CreateRewardProfileFromStatPackage(Il2CppReferenceArray<StatPreset> stats, RewardPreset preset)
#else
    private static RewardProfile CreateRewardProfileFromStatPackage(StatPreset[] stats, RewardPreset preset)
#endif
    {
        List<RewardEntry> rewards = new();
        HashSet<StatType> seenStats = new();
        foreach (StatPreset statPreset in stats)
        {
            if (!TryResolveStatType(statPreset, out StatType? statType)
                || statType == null
                || statType == CharacterStatType.Level
                || !IsRewardStat(statType)
                || !seenStats.Add(statType))
            {
                continue;
            }

            rewards.Add(CreateDynamicReward(statType, preset));
        }

        return new RewardProfile(rewards.ToArray(), grantNaturesCaress: false);
    }

    private static RewardEntry CreateDynamicReward(StatType statType, RewardPreset preset)
    {
        RewardKind kind = IsRpgStat(statType) ? RewardKind.RpgStat : RewardKind.Proficiency;
        float amount = kind == RewardKind.RpgStat ? AttributeAmount(preset) : PrimaryAmount(preset);
        return Reward(kind, statType, DisplayNameForStat(statType), amount, DescriptionForStat(statType));
    }

#if IL2CPP
    [HideFromIl2Cpp]
#endif
    private void SetStat(Hero hero, RewardEntry reward)
    {
        Stat? stat = hero.Stat(reward.StatType);
        if (stat == null)
        {
            Logger.LogWarning($"Could not find stat {reward.DisplayName} for background reward replacement.");
            return;
        }

        stat.SetTo(reward.TargetBaseValue);
    }

#if IL2CPP
    private static bool LooksLikeBackgroundStatPackage(Il2CppReferenceArray<StatPreset> stats)
#else
    private static bool LooksLikeBackgroundStatPackage(StatPreset[] stats)
#endif
    {
        if (stats == null || stats.Length == 0)
        {
            return false;
        }

        bool sawBackgroundStat = false;
        foreach (StatPreset statPreset in stats)
        {
            if (!TryResolveStatType(statPreset, out StatType? statType) || statType == null)
            {
                return false;
            }

            if (statType == CharacterStatType.Level)
            {
                continue;
            }

            if (IsRpgStat(statType))
            {
                if (statPreset.baseValue < -0.001f || statPreset.baseValue > 30f)
                {
                    return false;
                }

                sawBackgroundStat = true;
                continue;
            }

            if (statType is ProfStatType profStatType && IsSupportedProficiency(profStatType))
            {
                if (statPreset.baseValue < ProficiencyBaseValue - 0.001f || statPreset.baseValue > ProficiencyBaseValue + 30f)
                {
                    return false;
                }

                sawBackgroundStat = true;
                continue;
            }

            return false;
        }

        return sawBackgroundStat;
    }

#if IL2CPP
    private static bool MatchesStaySilentNoClassPackage(Il2CppReferenceArray<StatPreset> stats)
#else
    private static bool MatchesStaySilentNoClassPackage(StatPreset[] stats)
#endif
    {
        if (stats == null || stats.Length == 0)
        {
            return false;
        }

        bool shield = false;
        bool sneak = false;
        bool theft = false;
        foreach (StatPreset statPreset in stats)
        {
            if (!TryResolveStatType(statPreset, out StatType? statType) || statType == null)
            {
                return false;
            }

            if (statType == CharacterStatType.Level)
            {
                continue;
            }

            if (IsRpgStat(statType) && IsNativeBaselineRpgValue(statPreset.baseValue))
            {
                continue;
            }

            if (statType == ProfStatType.Shield && NearlyEqual(statPreset.baseValue, NativeStaySilentProficiencyBaseValue))
            {
                shield = true;
                continue;
            }

            if (statType == ProfStatType.Sneak && NearlyEqual(statPreset.baseValue, NativeStaySilentProficiencyBaseValue))
            {
                sneak = true;
                continue;
            }

            if (statType == ProfStatType.Theft && NearlyEqual(statPreset.baseValue, NativeStaySilentProficiencyBaseValue))
            {
                theft = true;
                continue;
            }

            return false;
        }

        return shield && sneak && theft;
    }

    private static bool IsNativeBaselineRpgValue(float value)
    {
        return NearlyEqual(value, 0f) || NearlyEqual(value, 1f);
    }

    private static bool TryFindBackground(string? buttonText, out BackgroundDefinition? definition)
    {
        definition = null;
        if (string.IsNullOrWhiteSpace(buttonText))
        {
            return false;
        }

        foreach (BackgroundDefinition background in Backgrounds)
        {
            if (background.Markers.Any(marker => buttonText.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                definition = background;
                return true;
            }
        }

        return false;
    }

    private static bool TryGetBackground(BackgroundId backgroundId, out BackgroundDefinition? definition)
    {
        definition = Backgrounds.FirstOrDefault(background => background.Id == backgroundId);
        return definition != null;
    }

    private static bool TryResolveStatType(StatPreset preset, out StatType? statType)
    {
        statType = null;
        try
        {
            statType = preset.StatType;
            return statType != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool IsRpgStat(StatType statType)
    {
        return RpgStatTypes.Contains(statType);
    }

    private static bool IsRewardStat(StatType statType)
    {
        return IsRpgStat(statType) || statType is ProfStatType profStatType && IsSupportedProficiency(profStatType);
    }

    private static bool IsSupportedProficiency(ProfStatType statType)
    {
        return ProfStatType.HeroProficiencies.Contains(statType);
    }

    private static bool TryParseHoverStat(IHoverInfo hoverInfo, out StatType? statType)
    {
        statType = null;
        string infoName = hoverInfo?.InfoName ?? string.Empty;
        int plusIndex = infoName.IndexOf('+');
        int minusIndex = infoName.IndexOf('-');
        int splitIndex = plusIndex >= 0 && minusIndex >= 0 ? Math.Min(plusIndex, minusIndex) : Math.Max(plusIndex, minusIndex);
        string statName = splitIndex >= 0 ? infoName.Substring(0, splitIndex) : infoName;
        return TryMapStatName(statName, out statType);
    }

    private static bool TryMapStatName(string statName, out StatType? statType)
    {
        statType = NormalizeStatName(statName) switch
        {
            "ONEHANDED" => ProfStatType.OneHanded,
            "TWOHANDED" => ProfStatType.TwoHanded,
            "UNARMED" => ProfStatType.Unarmed,
            "BLOCK" => ProfStatType.Shield,
            "BLOCKING" => ProfStatType.Shield,
            "SHIELD" => ProfStatType.Shield,
            "ATHLETICS" => ProfStatType.Athletics,
            "LIGHTARMOR" => ProfStatType.LightArmor,
            "MEDIUMARMOR" => ProfStatType.MediumArmor,
            "HEAVYARMOR" => ProfStatType.HeavyArmor,
            "ARCHERY" => ProfStatType.Archery,
            "EVASION" => ProfStatType.Evasion,
            "ACROBATICS" => ProfStatType.Acrobatics,
            "SNEAK" => ProfStatType.Sneak,
            "THEFT" => ProfStatType.Theft,
            "MAGIC" => ProfStatType.Magic,
            "ALCHEMY" => ProfStatType.Alchemy,
            "COOKING" => ProfStatType.Cooking,
            "HANDCRAFTING" => ProfStatType.Handcrafting,
            "STRENGTH" => HeroRPGStatType.Strength,
            "DEXTERITY" => HeroRPGStatType.Dexterity,
            "SPIRITUALITY" => HeroRPGStatType.Spirituality,
            "PERCEPTION" => HeroRPGStatType.Perception,
            "ENDURANCE" => HeroRPGStatType.Endurance,
            "PRACTICALITY" => HeroRPGStatType.Practicality,
            _ => null
        };

        return statType != null;
    }

    private static string NormalizeStatName(string statName)
    {
        return new string((statName ?? string.Empty)
            .Where(char.IsLetterOrDigit)
            .Select(char.ToUpperInvariant)
            .ToArray());
    }

    private static bool NearlyEqual(float value, float expected)
    {
        return Math.Abs(value - expected) < 0.001f;
    }

#if IL2CPP
    private static bool ApplyLevelRows(Hero hero, Il2CppReferenceArray<StatPreset> stats, bool ignoreLevelSetting)
#else
    private static bool ApplyLevelRows(Hero hero, StatPreset[] stats, bool ignoreLevelSetting)
#endif
    {
        if (ignoreLevelSetting)
        {
            return false;
        }

        bool applied = false;
        foreach (StatPreset statPreset in stats)
        {
            if (TryResolveStatType(statPreset, out StatType? statType) && statType == CharacterStatType.Level)
            {
                hero.Development.LevelUpTo((int)statPreset.baseValue);
                applied = true;
            }
        }

        return applied;
    }

    private void GrantNaturesCaress(Hero hero)
    {
        if (!ShouldGrantNaturesCaress(hero))
        {
            return;
        }

        if (!TryFindNaturesCaress(out ItemTemplate? template) || template == null)
        {
            Logger.LogWarning("Could not find Nature's Caress item template.");
            return;
        }

        Item item = World.Add(new Item(template, 1));
        _ = hero.HeroItems.Add(item);
        Logger.LogInfo($"Granted Nature's Caress ({SafeTemplateName(template)}, {SafeTemplateGuid(template)}).");
    }

    private bool ShouldGrantNaturesCaress(Hero hero)
    {
        string heroId = hero.HeroID.ToString();
        if (string.IsNullOrWhiteSpace(heroId)
            || string.Equals(heroId, Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return _naturesCaressGrantedHeroes.Add(heroId);
    }

    private static bool TryFindNaturesCaress(out ItemTemplate? template)
    {
        template = null;
        TemplatesProvider provider = World.Services.Get<TemplatesProvider>();
        if (provider == null || !provider.AllLoaded)
        {
            return false;
        }

        template = EnumerateItemTemplates(provider)
            .Where(IsUsableTemplate)
            .FirstOrDefault(t => SafeTemplateGuid(t).Equals(NaturesCaressTemplateGuid, StringComparison.OrdinalIgnoreCase))
            ?? EnumerateItemTemplates(provider)
                .Where(IsUsableTemplate)
                .FirstOrDefault(t => SafeTemplateSearchText(t).Contains("naturescaress") || SafeTemplateSearchText(t).Contains("nature's caress"));

        return template != null;
    }

    private static IEnumerable<ItemTemplate> EnumerateItemTemplates(TemplatesProvider provider)
    {
        var templates = provider.GetAllOfType<ItemTemplate>();
        if (templates == null)
        {
            yield break;
        }

#if IL2CPP
        foreach (ItemTemplate template in EnumerateNative(templates))
#else
        foreach (ItemTemplate template in templates)
#endif
        {
            yield return template;
        }
    }

#if IL2CPP
    private static IEnumerable<T> EnumerateNative<T>(Il2CppSystem.Collections.Generic.IEnumerable<T>? source)
    {
        if (source == null)
        {
            yield break;
        }

        Il2CppSystem.Collections.Generic.IEnumerator<T> genericEnumerator = source.GetEnumerator();
        Il2CppSystem.Collections.IEnumerator enumerator = genericEnumerator.Cast<Il2CppSystem.Collections.IEnumerator>();
        try
        {
            while (enumerator.MoveNext())
            {
                T current = genericEnumerator.Current;
                if (current is not null)
                {
                    yield return current;
                }
            }
        }
        finally
        {
            genericEnumerator.TryCast<Il2CppSystem.IDisposable>()?.Dispose();
        }
    }
#endif

    private static bool IsUsableTemplate(ItemTemplate template)
    {
        try
        {
            return template != null && !template.IsAbstract && !template.HiddenOnUI && !template.CannotBeDropped;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string SafeTemplateSearchText(ItemTemplate template)
    {
        try
        {
            string tags = SafeTemplateTags(template);
            return string.Join(" ", template.name, template.GUID, template.ItemName, tags).ToLowerInvariant();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private static string SafeTemplateTags(ItemTemplate template)
    {
#if IL2CPP
        if (template.Tags == null)
        {
            return string.Empty;
        }

        var nativeTags = template.Tags.TryCast<Il2CppSystem.Collections.Generic.IEnumerable<string>>();
        return nativeTags == null ? string.Empty : string.Join(" ", EnumerateNative(nativeTags));
#else
        return template.Tags == null ? string.Empty : string.Join(" ", template.Tags);
#endif
    }

    private static string SafeTemplateName(ItemTemplate template)
    {
        try
        {
            return string.IsNullOrWhiteSpace(template.name) ? "unknown" : template.name;
        }
        catch (Exception)
        {
            return "unknown";
        }
    }

    private static string SafeTemplateGuid(ItemTemplate template)
    {
        try
        {
            return template.GUID ?? string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private static RewardProfile CreateRewardProfile(BackgroundId backgroundId, RewardPreset preset)
    {
        float primary = PrimaryAmount(preset);
        float secondary = SecondaryAmount(preset);
        float attribute = AttributeAmount(preset);

        return backgroundId switch
        {
            BackgroundId.Army => new RewardProfile(new[]
            {
                Reward(RewardKind.Proficiency, ProfStatType.OneHanded, "One-Handed", primary, "Drill-yard cuts and worn grips made a blade feel familiar."),
                Reward(RewardKind.Proficiency, ProfStatType.Shield, "Block", primary, "Shield habits from hard orders and harder training."),
                Reward(RewardKind.Proficiency, ProfStatType.HeavyArmor, "Heavy Armor", secondary, "You learned to breathe under straps, plates, and marching weight."),
                Reward(RewardKind.RpgStat, HeroRPGStatType.Endurance, "Endurance", attribute, "Long roads, poor sleep, and worse rations hardened you.")
            }, grantNaturesCaress: false),

            BackgroundId.Hunter => new RewardProfile(new[]
            {
                Reward(RewardKind.Proficiency, ProfStatType.Archery, "Archery", primary, "Meat on the fire depended on calm hands and a patient draw."),
                Reward(RewardKind.Proficiency, ProfStatType.Sneak, "Sneak", secondary, "Dead leaves, wet roots, and wary prey taught you where not to step."),
                Reward(RewardKind.Proficiency, ProfStatType.Cooking, "Cooking", secondary, "A small fire and a rough knife turned lean days survivable."),
                Reward(RewardKind.RpgStat, HeroRPGStatType.Perception, "Perception", attribute, "Tracks, wind shifts, and broken brush sharpened your eye.")
            }, grantNaturesCaress: false),

            BackgroundId.Outlaw => new RewardProfile(new[]
            {
                Reward(RewardKind.Proficiency, ProfStatType.Sneak, "Sneak", primary, "You learned the value of silence when a shout meant irons."),
                Reward(RewardKind.Proficiency, ProfStatType.Theft, "Theft", primary, "Crowds, purses, and locked doors taught your hands their trade."),
                Reward(RewardKind.Proficiency, ProfStatType.Evasion, "Evasion", secondary, "When a patrol turned, you already knew the nearest way out."),
                Reward(RewardKind.RpgStat, HeroRPGStatType.Dexterity, "Dexterity", attribute, "Narrow escapes made quick hands and quicker feet.")
            }, grantNaturesCaress: false),

            BackgroundId.Worship => new RewardProfile(new[]
            {
                Reward(RewardKind.Proficiency, ProfStatType.Magic, "Magic", primary, "Old prayers and stranger signs left power under your skin."),
                Reward(RewardKind.Proficiency, ProfStatType.Alchemy, "Alchemy", secondary, "Incense, oils, poultices, and forbidden mixtures taught careful measure."),
                Reward(RewardKind.Proficiency, ProfStatType.Handcrafting, "Handcrafting", secondary, "Broken charms and makeshift offerings made patient hands."),
                Reward(RewardKind.RpgStat, HeroRPGStatType.Spirituality, "Spirituality", attribute, "The old places taught you to listen where others scoffed.")
            }, grantNaturesCaress: false),

            BackgroundId.Silent => new RewardProfile(new[]
            {
                Reward(RewardKind.RpgStat, HeroRPGStatType.Strength, "Strength", attribute, "The guard gets no answer, but your body remembers."),
                Reward(RewardKind.RpgStat, HeroRPGStatType.Dexterity, "Dexterity", attribute, "The guard gets no answer, but your body remembers."),
                Reward(RewardKind.RpgStat, HeroRPGStatType.Spirituality, "Spirituality", attribute, "The guard gets no answer, but your body remembers."),
                Reward(RewardKind.RpgStat, HeroRPGStatType.Perception, "Perception", attribute, "The guard gets no answer, but your body remembers."),
                Reward(RewardKind.RpgStat, HeroRPGStatType.Endurance, "Endurance", attribute, "The guard gets no answer, but your body remembers."),
                Reward(RewardKind.RpgStat, HeroRPGStatType.Practicality, "Practicality", attribute, "The guard gets no answer, but your body remembers.")
            }, grantNaturesCaress: true),

            _ => new RewardProfile(Array.Empty<RewardEntry>(), grantNaturesCaress: false)
        };
    }

    private static RewardEntry Reward(RewardKind kind, StatType statType, string displayName, float amount, string description)
    {
        return new RewardEntry(kind, statType, displayName, amount, description);
    }

    private static float PrimaryAmount(RewardPreset preset)
    {
        return preset switch
        {
            RewardPreset.Sparse => 3f,
            RewardPreset.Generous => 9f,
            _ => 6f
        };
    }

    private static float SecondaryAmount(RewardPreset preset)
    {
        return preset switch
        {
            RewardPreset.Sparse => 1f,
            RewardPreset.Generous => 6f,
            _ => 3f
        };
    }

    private static float AttributeAmount(RewardPreset preset)
    {
        return preset switch
        {
            RewardPreset.Sparse => 1f,
            RewardPreset.Generous => 5f,
            _ => 3f
        };
    }

    private static string PresetDisplayName(RewardPreset preset)
    {
        return preset switch
        {
            RewardPreset.Sparse => "Sparse",
            RewardPreset.Generous => "Generous",
            _ => "Immersive Overhaul"
        };
    }

    private static string FormatBonus(float value)
    {
        string formatted = value.ToString("0.##", CultureInfo.InvariantCulture);
        return value > 0f ? "+" + formatted : formatted;
    }

    private static string DisplayNameForStat(StatType statType)
    {
        if (statType == ProfStatType.OneHanded)
        {
            return "One-Handed";
        }

        if (statType == ProfStatType.TwoHanded)
        {
            return "Two-Handed";
        }

        if (statType == ProfStatType.LightArmor)
        {
            return "Light Armor";
        }

        if (statType == ProfStatType.MediumArmor)
        {
            return "Medium Armor";
        }

        if (statType == ProfStatType.HeavyArmor)
        {
            return "Heavy Armor";
        }

        if (statType == ProfStatType.Shield)
        {
            return "Block";
        }

        return statType switch
        {
            _ when statType == ProfStatType.Unarmed => "Unarmed",
            _ when statType == ProfStatType.Athletics => "Athletics",
            _ when statType == ProfStatType.Archery => "Archery",
            _ when statType == ProfStatType.Evasion => "Evasion",
            _ when statType == ProfStatType.Acrobatics => "Acrobatics",
            _ when statType == ProfStatType.Sneak => "Sneak",
            _ when statType == ProfStatType.Theft => "Theft",
            _ when statType == ProfStatType.Magic => "Magic",
            _ when statType == ProfStatType.Alchemy => "Alchemy",
            _ when statType == ProfStatType.Cooking => "Cooking",
            _ when statType == ProfStatType.Handcrafting => "Handcrafting",
            _ when statType == HeroRPGStatType.Strength => "Strength",
            _ when statType == HeroRPGStatType.Dexterity => "Dexterity",
            _ when statType == HeroRPGStatType.Spirituality => "Spirituality",
            _ when statType == HeroRPGStatType.Perception => "Perception",
            _ when statType == HeroRPGStatType.Endurance => "Endurance",
            _ when statType == HeroRPGStatType.Practicality => "Practicality",
            _ => "Skill"
        };
    }

    private static string DescriptionForStat(StatType statType)
    {
        if (statType == ProfStatType.OneHanded)
        {
            return "Drill, panic, or practice made a single blade feel familiar.";
        }

        if (statType == ProfStatType.TwoHanded)
        {
            return "You meet danger with both hands set and no room left for hesitation.";
        }

        if (statType == ProfStatType.Unarmed)
        {
            return "When steel is gone, your stance and fists still know what to do.";
        }

        if (statType == ProfStatType.Shield)
        {
            return "You trust the raised guard, the planted foot, and the line held.";
        }

        if (statType == ProfStatType.Athletics)
        {
            return "Hard ground and hard escapes taught your body to keep moving.";
        }

        if (statType == ProfStatType.LightArmor)
        {
            return "You learned to value protection that still lets you run.";
        }

        if (statType == ProfStatType.MediumArmor)
        {
            return "You know the balance between a guarded body and a quick step.";
        }

        if (statType == ProfStatType.HeavyArmor)
        {
            return "You learned to breathe under straps, plates, and marching weight.";
        }

        if (statType == ProfStatType.Archery)
        {
            return "You keep danger at bowshot, where breath and timing decide the fight.";
        }

        if (statType == ProfStatType.Evasion)
        {
            return "You survive the strike by not being where it lands.";
        }

        if (statType == ProfStatType.Acrobatics)
        {
            return "Falls, ledges, and desperate footing taught you grace under pressure.";
        }

        if (statType == ProfStatType.Sneak)
        {
            return "You move when attention slips and leave little for anyone to follow.";
        }

        if (statType == ProfStatType.Theft)
        {
            return "Locks, purses, and guarded hands taught your fingers patience.";
        }

        if (statType == ProfStatType.Magic)
        {
            return "Old signs and half-heard words left power ready beneath your skin.";
        }

        if (statType == ProfStatType.Alchemy)
        {
            return "You learned what careful hands can wake from herbs, ash, and glass.";
        }

        if (statType == ProfStatType.Cooking)
        {
            return "A small fire and a rough meal kept bad nights survivable.";
        }

        if (statType == ProfStatType.Handcrafting)
        {
            return "Broken things taught you patience, pressure, and the worth of a good tool.";
        }

        if (statType == HeroRPGStatType.Strength)
        {
            return "Hard labor and harder roads left force in your hands.";
        }

        if (statType == HeroRPGStatType.Dexterity)
        {
            return "Quick hands and quicker choices carried you through tight places.";
        }

        if (statType == HeroRPGStatType.Spirituality)
        {
            return "Old rites and darker silences taught you to listen past fear.";
        }

        if (statType == HeroRPGStatType.Perception)
        {
            return "Tracks, tells, and sudden quiet trained your eyes to linger.";
        }

        if (statType == HeroRPGStatType.Endurance)
        {
            return "Poor sleep, long roads, and worse weather made you difficult to break.";
        }

        if (statType == HeroRPGStatType.Practicality)
        {
            return "Need taught you to solve with what was near, not what was ideal.";
        }

        return "Your past leaves a practical mark on this choice.";
    }

    private static DynamicChoiceTone DynamicChoiceToneFor(IReadOnlyCollection<RewardEntry> rewards)
    {
        if (rewards.Any(reward => reward.StatType == ProfStatType.Archery))
        {
            return DynamicChoiceTone.Ranged;
        }

        if (rewards.Any(reward => reward.StatType == ProfStatType.Sneak || reward.StatType == ProfStatType.Theft))
        {
            return DynamicChoiceTone.Stealth;
        }

        if (rewards.Any(reward => reward.StatType == ProfStatType.Magic || reward.StatType == ProfStatType.Alchemy))
        {
            return DynamicChoiceTone.Wyrd;
        }

        if (rewards.Any(reward => reward.StatType == ProfStatType.OneHanded || reward.StatType == ProfStatType.Shield || reward.StatType == ProfStatType.HeavyArmor))
        {
            return DynamicChoiceTone.Martial;
        }

        return DynamicChoiceTone.Practical;
    }

    private static string DynamicPreviewName(DynamicChoiceTone tone)
    {
        return tone switch
        {
            DynamicChoiceTone.Ranged => "Bowstring Nerve",
            DynamicChoiceTone.Stealth => "Quiet Hands",
            DynamicChoiceTone.Wyrd => "Wyrd Instinct",
            DynamicChoiceTone.Martial => "Steel Nerve",
            _ => "Practical Instinct"
        };
    }

    private static string DynamicBackgroundText(DynamicChoiceTone tone)
    {
        return tone switch
        {
            DynamicChoiceTone.Ranged => "A bow keeps trouble honest: distance, breath, and one clean release.",
            DynamicChoiceTone.Stealth => "When a lock opens quietly or a purse leaves unnoticed, nobody has to bleed.",
            DynamicChoiceTone.Wyrd => "You lean toward signs, mixtures, and old powers that others pretend not to feel.",
            DynamicChoiceTone.Martial => "You answer danger up close, with balance, nerve, and steel.",
            _ => "You survive by reading the moment and using whatever is already in your hands."
        };
    }

    private static ShareableSpriteReference GetSafeIcon(StatType statType)
    {
        try
        {
            if (statType is ProfStatType profStatType && profStatType.GetIcon != null)
            {
                return profStatType.GetIcon.Invoke() ?? new ShareableSpriteReference();
            }

            if (statType is HeroRPGStatType rpgStatType && rpgStatType.icon != null)
            {
                return rpgStatType.icon.Invoke() ?? new ShareableSpriteReference();
            }
        }
        catch (Exception)
        {
            return new ShareableSpriteReference();
        }

        return new ShareableSpriteReference();
    }

    private enum BackgroundId
    {
        None,
        Army,
        Hunter,
        Outlaw,
        Worship,
        Silent
    }

    private enum RewardKind
    {
        Proficiency,
        RpgStat
    }

    private enum DynamicChoiceTone
    {
        Ranged,
        Stealth,
        Wyrd,
        Martial,
        Practical
    }

    private sealed class BackgroundDefinition
    {
        public BackgroundDefinition(BackgroundId id, string[] markers, string previewName, string backgroundText, Func<ShareableSpriteReference> createIcon)
        {
            Id = id;
            Markers = markers;
            PreviewName = previewName;
            BackgroundText = backgroundText;
            CreateIcon = createIcon;
        }

        public BackgroundId Id { get; }

        public string[] Markers { get; }

        public string PreviewName { get; }

        public string BackgroundText { get; }

        public Func<ShareableSpriteReference> CreateIcon { get; }
    }

    private sealed class RewardProfile
    {
        public RewardProfile(RewardEntry[] rewards, bool grantNaturesCaress)
        {
            Rewards = rewards;
            GrantNaturesCaress = grantNaturesCaress;
        }

        public RewardEntry[] Rewards { get; }

        public bool GrantNaturesCaress { get; }
    }

    private sealed class DynamicChoiceDefinition
    {
        public DynamicChoiceDefinition(
            string previewName,
            string backgroundText,
            Func<ShareableSpriteReference> createIcon,
            RewardEntry[] rewards,
            IHoverInfo[] passthroughHoverInfos)
        {
            PreviewName = previewName;
            BackgroundText = backgroundText;
            CreateIcon = createIcon;
            Rewards = rewards;
            PassthroughHoverInfos = passthroughHoverInfos;
        }

        public string PreviewName { get; }

        public string BackgroundText { get; }

        public Func<ShareableSpriteReference> CreateIcon { get; }

        public RewardEntry[] Rewards { get; }

        public IHoverInfo[] PassthroughHoverInfos { get; }
    }

    private sealed class RewardEntry
    {
        public RewardEntry(RewardKind kind, StatType statType, string displayName, float amount, string description)
        {
            Kind = kind;
            StatType = statType;
            DisplayName = displayName;
            Amount = amount;
            Description = description;
        }

        public RewardKind Kind { get; }

        public StatType StatType { get; }

        public string DisplayName { get; }

        public float Amount { get; }

        public string Description { get; }

        public float TargetBaseValue => Kind == RewardKind.Proficiency ? ProficiencyBaseValue + Amount : Amount;
    }

#if !IL2CPP
    private sealed class ImmersiveBackgroundsHoverInfo : IHoverInfo
    {
        public string InfoGroupName { get; set; } = string.Empty;

        public string InfoName { get; set; } = string.Empty;

        public string InfoDescription { get; set; } = string.Empty;

        public ShareableSpriteReference InfoIcon { get; set; } = new ShareableSpriteReference();
    }
#endif
}
