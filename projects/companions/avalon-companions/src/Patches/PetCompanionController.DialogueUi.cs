using System;
using System.Collections.Generic;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Locations;
using AvalonCompanions.Framework;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;

namespace AvalonCompanions.Patches;

internal static partial class PetCompanionController
{
    private const int DialogueCanvasSortingOrder = 32760;
    private const float DialogueChoiceWidth = 660f;
    private const float DialogueChoiceHeight = 48f;
    private const float DialogueChoiceSpacing = 10f;
    private const float DialogueTextWidth = 1380f;
    private const float DialogueTextHeight = 142f;
    private const float DialogueTextInsetX = 34f;
    private const float DialogueTextPortraitInsetX = 156f;
    private const float DialoguePortraitSize = 104f;
    private const float DialoguePortraitIconSize = 64f;
    private const string DialogueCandidateSuffix = " Candidate";

    private static readonly Color DialogueBlockerColor = new Color(0f, 0f, 0f, 0.01f);
    private static readonly Color DialogueBottomTextColor = new Color(0f, 0f, 0f, 0.62f);
    private static readonly Color DialogueButtonColor = new Color(0.115f, 0.150f, 0.150f, 0.98f);
    private static readonly Color DialogueButtonHoverColor = new Color(0.170f, 0.220f, 0.215f, 0.98f);
    private static readonly Color DialogueSelectedButtonColor = new Color(0.380f, 0.270f, 0.105f, 0.98f);
    private static readonly Color DialogueSelectedHoverColor = new Color(0.470f, 0.340f, 0.140f, 0.98f);
    private static readonly Color DialogueDangerButtonColor = new Color(0.310f, 0.095f, 0.085f, 0.98f);
    private static readonly Color DialogueDangerHoverColor = new Color(0.440f, 0.125f, 0.110f, 0.98f);
    private static readonly Color DialogueDisabledButtonColor = new Color(0.120f, 0.130f, 0.130f, 0.58f);
    private static readonly Color DialogueTransparentColor = new Color(0f, 0f, 0f, 0f);
    private static readonly Color DialogueSelectedAccentColor = new Color(0.920f, 0.640f, 0.210f, 1f);
    private static readonly Color DialogueHoverAccentColor = new Color(0.660f, 0.700f, 0.590f, 0.92f);
    private static readonly Color DialogueDangerAccentColor = new Color(0.860f, 0.230f, 0.160f, 1f);
    private static readonly Color DialogueDisabledAccentColor = new Color(0.320f, 0.350f, 0.340f, 0.72f);
    private static readonly Color DialogueTextColor = new Color(0.925f, 0.905f, 0.825f, 1f);
    private static readonly Color DialogueHoverTextColor = new Color(1.000f, 0.965f, 0.835f, 1f);
    private static readonly Color DialogueDisabledTextColor = new Color(0.520f, 0.560f, 0.535f, 0.90f);
    private static readonly Color DialogueMutedTextColor = new Color(0.715f, 0.760f, 0.725f, 1f);
    private static readonly Color DialoguePortraitFallbackColor = new Color(0.055f, 0.060f, 0.058f, 0.84f);
    private static readonly Color DialoguePortraitBackgroundColor = new Color(1f, 1f, 1f, 0.90f);
    private static readonly Color DialoguePortraitIconColor = new Color(1f, 1f, 1f, 0.96f);
    private static readonly List<DialogueButtonBinding> DialogueButtons = new List<DialogueButtonBinding>();

    private static GameObject? _dialogueUiRoot;
    private static GameObject? _dialogueEventSystemRoot;
    private static Font? _dialogueFont;
    private static RectTransform? _dialogueTextStack;
    private static GameObject? _dialoguePortraitRoot;
    private static RawImage? _dialoguePortraitBackgroundImage;
    private static RawImage? _dialoguePortraitIconImage;
    private static Text? _dialogueTitleText;
    private static Text? _dialogueSubtitleText;
    private static int _lastDialoguePointerFrame = -1;

    private static void EnsureDialogueUiHost(ManualLogSource? logger)
    {
        if (!_dialogueVisible)
        {
            DestroyDialogueUiHost();
            return;
        }

        EnsureCursorForPanel();
        if (_dialogueUiRoot == null)
        {
            CreateDialogueUiHost(logger);
        }

        SyncDialogueUiContent();
        TryHandleDialogueDirectPointerInput(logger);
    }

    private static void CreateDialogueUiHost(ManualLogSource? logger)
    {
        DestroyDialogueUiHost();
        EnsureDialogueEventSystem();
        List<ExternalDialogueCommand> externalCommands = GetExternalDialogueCommands(GetDialogueLocation());

        _dialogueUiRoot = new GameObject(
            "AvalonCompanions.DialogueUi",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        UnityEngine.Object.DontDestroyOnLoad(_dialogueUiRoot);

        Canvas canvas = _dialogueUiRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = DialogueCanvasSortingOrder;

        CanvasScaler scaler = _dialogueUiRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform blocker = AddRect("Blocker", _dialogueUiRoot.transform);
        Stretch(blocker);
        Image blockerImage = blocker.gameObject.AddComponent<Image>();
        blockerImage.color = DialogueBlockerColor;
        blockerImage.raycastTarget = true;

        RectTransform choices = AddRect("Choices", blocker);
        choices.anchorMin = new Vector2(1f, 0.53f);
        choices.anchorMax = new Vector2(1f, 0.53f);
        choices.pivot = new Vector2(1f, 0.5f);
        choices.anchoredPosition = new Vector2(-62f, 0f);
        int choiceCount = 11 + externalCommands.Count;
        choices.sizeDelta = new Vector2(
            DialogueChoiceWidth,
            DialogueChoiceHeight * choiceCount + DialogueChoiceSpacing * Math.Max(0, choiceCount - 1));

        VerticalLayoutGroup choicesLayout = choices.gameObject.AddComponent<VerticalLayoutGroup>();
        choicesLayout.spacing = DialogueChoiceSpacing;
        choicesLayout.childAlignment = TextAnchor.MiddleRight;
        choicesLayout.childControlWidth = true;
        choicesLayout.childControlHeight = true;
        choicesLayout.childForceExpandWidth = true;
        choicesLayout.childForceExpandHeight = false;

        AddDialogueButton(choices, "follow", "Follow", DialogueButtonKind.ModeFollow, requiresCompanion: true, command: commandLogger => ApplyCompanionMode(commandLogger, CompanionMode.Follow));
        AddDialogueButton(choices, "stay", "Hold Position", DialogueButtonKind.ModeStay, requiresCompanion: true, command: commandLogger => ApplyCompanionMode(commandLogger, CompanionMode.Stay));
        AddDialogueButton(choices, "defend", "Defend", DialogueButtonKind.ModeDefend, requiresCompanion: true, command: commandLogger => ApplyCompanionMode(commandLogger, CompanionMode.Defend));
        AddDialogueButton(choices, "range-close", "Keep Close", DialogueButtonKind.RangeClose, requiresCompanion: true, command: commandLogger => SetFollowRange(commandLogger, FollowRangeProfile.Close, recallActive: true));
        AddDialogueButton(choices, "range-normal", "Keep Pace", DialogueButtonKind.RangeNormal, requiresCompanion: true, command: commandLogger => SetFollowRange(commandLogger, FollowRangeProfile.Normal, recallActive: true));
        AddDialogueButton(choices, "range-far", "Keep Distance", DialogueButtonKind.RangeFar, requiresCompanion: true, command: commandLogger => SetFollowRange(commandLogger, FollowRangeProfile.Far, recallActive: true));
        AddDialogueButton(choices, "come-close", "Come Close", DialogueButtonKind.Standard, requiresCompanion: true, command: HeelManagedPets);
        AddDialogueButton(choices, "recall", "Recall", DialogueButtonKind.Standard, requiresCompanion: true, command: RecallManagedPets);
        AddDialogueButton(choices, "recover", "Recover", DialogueButtonKind.Standard, requiresCompanion: true, command: RecoverManagedPets);
        foreach (ExternalDialogueCommand externalCommand in externalCommands)
        {
            AddDialogueButton(
                choices,
                "external:" + externalCommand.OwnerId + ":" + externalCommand.CommandId,
                externalCommand.Label,
                DialogueButtonKind.Standard,
                requiresCompanion: true,
                command: _ => externalCommand.Execute(),
                canExecute: externalCommand.CanExecute);
        }
        AddDialogueButton(choices, "dismiss", "Dismiss", DialogueButtonKind.Danger, requiresCompanion: true, command: commandLogger => DismissManagedPets(commandLogger, "companion dialogue"));
        AddDialogueButton(choices, "goodbye", "Leave", DialogueButtonKind.Standard, requiresCompanion: false, command: null);

        RectTransform dialogueText = AddRect("DialogueText", blocker);
        dialogueText.anchorMin = new Vector2(0.5f, 0f);
        dialogueText.anchorMax = new Vector2(0.5f, 0f);
        dialogueText.pivot = new Vector2(0.5f, 0f);
        dialogueText.anchoredPosition = new Vector2(0f, 42f);
        dialogueText.sizeDelta = new Vector2(DialogueTextWidth, DialogueTextHeight);

        Image textImage = dialogueText.gameObject.AddComponent<Image>();
        textImage.color = DialogueBottomTextColor;
        textImage.raycastTarget = false;

        RectTransform portraitFrame = AddRect("CompanionPortrait", dialogueText);
        portraitFrame.anchorMin = new Vector2(0f, 0.5f);
        portraitFrame.anchorMax = new Vector2(0f, 0.5f);
        portraitFrame.pivot = new Vector2(0f, 0.5f);
        portraitFrame.anchoredPosition = new Vector2(24f, 0f);
        portraitFrame.sizeDelta = new Vector2(DialoguePortraitSize, DialoguePortraitSize);

        Image portraitFallback = portraitFrame.gameObject.AddComponent<Image>();
        portraitFallback.color = DialoguePortraitFallbackColor;
        portraitFallback.raycastTarget = false;

        RectTransform portraitBackground = AddRect("Background", portraitFrame);
        Stretch(portraitBackground);
        _dialoguePortraitBackgroundImage = portraitBackground.gameObject.AddComponent<RawImage>();
        _dialoguePortraitBackgroundImage.color = DialoguePortraitBackgroundColor;
        _dialoguePortraitBackgroundImage.raycastTarget = false;

        RectTransform portraitIcon = AddRect("Icon", portraitFrame);
        portraitIcon.anchorMin = new Vector2(0.5f, 0.5f);
        portraitIcon.anchorMax = new Vector2(0.5f, 0.5f);
        portraitIcon.pivot = new Vector2(0.5f, 0.5f);
        portraitIcon.anchoredPosition = Vector2.zero;
        portraitIcon.sizeDelta = new Vector2(DialoguePortraitIconSize, DialoguePortraitIconSize);
        _dialoguePortraitIconImage = portraitIcon.gameObject.AddComponent<RawImage>();
        _dialoguePortraitIconImage.color = DialoguePortraitIconColor;
        _dialoguePortraitIconImage.raycastTarget = false;
        _dialoguePortraitRoot = portraitFrame.gameObject;
        _dialoguePortraitRoot.SetActive(false);

        _dialogueTextStack = AddRect("DialogueTextStack", dialogueText);
        _dialogueTextStack.anchorMin = Vector2.zero;
        _dialogueTextStack.anchorMax = Vector2.one;
        _dialogueTextStack.offsetMin = new Vector2(DialogueTextInsetX, 0f);
        _dialogueTextStack.offsetMax = new Vector2(-DialogueTextInsetX, 0f);

        VerticalLayoutGroup textLayout = _dialogueTextStack.gameObject.AddComponent<VerticalLayoutGroup>();
        textLayout.padding = new RectOffset(0, 0, 18, 18);
        textLayout.spacing = 7f;
        textLayout.childAlignment = TextAnchor.MiddleCenter;
        textLayout.childControlWidth = true;
        textLayout.childControlHeight = true;
        textLayout.childForceExpandWidth = true;
        textLayout.childForceExpandHeight = false;

        _dialogueTitleText = AddText(_dialogueTextStack, "Line", 31, FontStyle.BoldAndItalic, DialogueTextColor, TextAnchor.MiddleCenter, wrap: true);
        _dialogueSubtitleText = AddText(_dialogueTextStack, "Detail", 18, FontStyle.Bold, DialogueMutedTextColor, TextAnchor.MiddleCenter, wrap: true);

        logger?.LogInfo("Avalon Companions opened vanilla-style Unity UI companion dialogue host. renderer=UnityEngine.UI; action=custom-dialogue-ui; layout=vanilla-style");
    }

    private static void EnsureDialogueEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        foreach (EventSystem eventSystem in UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
        {
            if (eventSystem != null && eventSystem.isActiveAndEnabled)
            {
                EventSystem.current = eventSystem;
                return;
            }
        }

        _dialogueEventSystemRoot = new GameObject(
            "AvalonCompanions.DialogueEventSystem",
            typeof(EventSystem),
            typeof(StandaloneInputModule));
        UnityEngine.Object.DontDestroyOnLoad(_dialogueEventSystemRoot);
    }

    private static RectTransform AddRect(string name, Transform parent)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, worldPositionStays: false);
        return child.GetComponent<RectTransform>();
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Text AddText(Transform parent, string name, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment, bool wrap)
    {
        RectTransform rect = AddRect(name, parent);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = GetDialogueFont();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;

        LayoutElement element = rect.gameObject.AddComponent<LayoutElement>();
        element.minHeight = Mathf.Max(24f, fontSize + 8f);
        element.flexibleWidth = 1f;
        return text;
    }

    private static void AddDialogueButton(
        Transform parent,
        string commandId,
        string label,
        DialogueButtonKind kind,
        bool requiresCompanion,
        Action<ManualLogSource>? command,
        Func<bool>? canExecute = null)
    {
        RectTransform rect = AddRect(label.Replace(" ", string.Empty).Replace(".", string.Empty) + "Choice", parent);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = GetDialogueButtonColor(kind, selected: false, hover: false, interactable: true);

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        LayoutElement element = rect.gameObject.AddComponent<LayoutElement>();
        element.minHeight = DialogueChoiceHeight;
        element.preferredHeight = DialogueChoiceHeight;
        element.flexibleWidth = 1f;

        RectTransform accentRect = AddRect("StateAccent", rect);
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.offsetMin = new Vector2(0f, 0f);
        accentRect.offsetMax = new Vector2(7f, 0f);
        Image accent = accentRect.gameObject.AddComponent<Image>();
        accent.color = DialogueTransparentColor;
        accent.raycastTarget = false;

        RectTransform labelRect = AddRect("Label", rect);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(34f, 0f);
        labelRect.offsetMax = new Vector2(-18f, 0f);

        Text text = labelRect.gameObject.AddComponent<Text>();
        text.font = GetDialogueFont();
        text.fontSize = 24;
        text.fontStyle = FontStyle.Bold;
        text.color = DialogueTextColor;
        text.text = label;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 16;
        text.resizeTextMaxSize = 24;
        text.raycastTarget = false;

        button.onClick.AddListener(() =>
        {
            RunDialogueUiChoice(commandId, label, command, route: "unity-button");
        });

        DialogueButtons.Add(new DialogueButtonBinding(rect, button, image, accent, text, commandId, label, kind, requiresCompanion, command, canExecute));
        ApplyDialogueButtonState(DialogueButtons[DialogueButtons.Count - 1], selected: false, hover: false);
    }

    private static Font GetDialogueFont()
    {
        if (_dialogueFont != null)
        {
            return _dialogueFont;
        }

        _dialogueFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return _dialogueFont;
    }

    private static void SyncDialogueUiContent()
    {
        if (_dialogueUiRoot == null)
        {
            return;
        }

        string runtimeBlockReason = GetRuntimeBlockReason();
        bool runtimeReady = string.IsNullOrEmpty(runtimeBlockReason);
        Location? location = GetDialogueLocation();
        bool hasCompanion = location != null;
        PetRosterEntry entry = hasCompanion ? GetRosterEntry(location!) : GetSelectedPet();
        bool canCommand = runtimeReady && hasCompanion;
        bool portraitVisible = SyncDialoguePortrait(canCommand, entry);
        SyncDialogueTextStack(portraitVisible);

        SetText(_dialogueTitleText, GetDialogueTitle(runtimeReady, hasCompanion, entry));
        SetText(_dialogueSubtitleText, GetDialogueSubtitle(runtimeReady, hasCompanion, entry, runtimeBlockReason));

        foreach (DialogueButtonBinding binding in DialogueButtons)
        {
            bool interactable = !binding.RequiresCompanion || canCommand;
            if (interactable && binding.CanExecute != null)
            {
                try
                {
                    interactable = binding.CanExecute();
                }
                catch (Exception ex)
                {
                    interactable = false;
                    _logger?.LogWarning($"External companion dialogue command availability failed. command={binding.CommandId}; error={ex.GetType().Name}: {ex.Message}");
                }
            }

            if (interactable && binding.RequiresCompanion && binding.CanExecute == null)
            {
                interactable = IsCommandPolicyAllowed(entry, GetPolicyAction(binding.CommandId, binding.Kind));
            }

            bool selected = IsDialogueButtonSelected(binding.Kind);
            bool hover = interactable && RectTransformUtility.RectangleContainsScreenPoint(binding.Rect, Input.mousePosition, cam: null);
            binding.Button.interactable = interactable;
            ApplyDialogueButtonState(binding, selected, hover);
        }
    }

    private static bool SyncDialoguePortrait(bool visible, PetRosterEntry entry)
    {
        if (_dialoguePortraitRoot == null)
        {
            return false;
        }

        if (!visible)
        {
            SetDialoguePortraitVisible(false);
            return false;
        }

        string iconId = GetRosterIconId(entry);
        Texture2D? icon = GetCachedHudIcon(iconId);
        if (icon == null)
        {
            SetDialoguePortraitVisible(false);
            return false;
        }

        Texture2D? background = GetCompanionHudBackground();
        SetDialoguePortraitVisible(true);

        if (_dialoguePortraitIconImage != null)
        {
            _dialoguePortraitIconImage.texture = icon;
            _dialoguePortraitIconImage.enabled = true;
        }

        if (_dialoguePortraitBackgroundImage != null)
        {
            _dialoguePortraitBackgroundImage.texture = background;
            _dialoguePortraitBackgroundImage.enabled = background != null;
        }

        return true;
    }

    private static void SetDialoguePortraitVisible(bool visible)
    {
        if (_dialoguePortraitRoot != null && _dialoguePortraitRoot.activeSelf != visible)
        {
            _dialoguePortraitRoot.SetActive(visible);
        }
    }

    private static void SyncDialogueTextStack(bool hasPortrait)
    {
        if (_dialogueTextStack == null)
        {
            return;
        }

        float leftInset = hasPortrait ? DialogueTextPortraitInsetX : DialogueTextInsetX;
        _dialogueTextStack.offsetMin = new Vector2(leftInset, 0f);
        _dialogueTextStack.offsetMax = new Vector2(-DialogueTextInsetX, 0f);
    }

    private static void SetText(Text? text, string value)
    {
        if (text != null && !string.Equals(text.text, value, StringComparison.Ordinal))
        {
            text.text = value;
        }
    }

    private static string GetDialogueTitle(bool runtimeReady, bool hasCompanion, PetRosterEntry entry)
    {
        if (!runtimeReady)
        {
            return "Companion orders are unavailable.";
        }

        if (!hasCompanion)
        {
            return "No companion is with you.";
        }

        return $"{GetDialogueCompanionName(entry)} is listening.";
    }

    private static string GetDialogueSubtitle(bool runtimeReady, bool hasCompanion, PetRosterEntry entry, string runtimeBlockReason)
    {
        if (!runtimeReady)
        {
            return GetDialogueUnavailableLine(runtimeBlockReason);
        }

        return hasCompanion
            ? GetDialogueCompanionStatusLine(entry)
            : "Find or summon a companion before giving orders.";
    }

    private static string GetDialogueCompanionStatusLine(PetRosterEntry entry)
    {
        int attackerCount = CountLiveHeroAttackers(Hero.Current);
        string modeLine = _companionMode switch
        {
            CompanionMode.Stay => "Holding position. Recall brings them back to you.",
            CompanionMode.Defend => IsCommandPolicyAllowed(entry, CompanionCommandPolicyAction.Defend)
                ? attackerCount > 0
                    ? $"Defending you. Threats nearby: {attackerCount}."
                    : "Defending you. Waiting for a threat."
                : "This companion cannot reliably defend.",
            _ => $"Following. They will {GetDialogueRangePhrase(GetFollowRangeProfile())}.",
        };

        string bondLine = GetDialogueCompanionBondLine(entry);
        return string.IsNullOrEmpty(bondLine) ? modeLine : $"{modeLine} {bondLine}";
    }

    private static string GetDialogueCompanionBondLine(PetRosterEntry entry)
    {
        return GetCompanionBondDialogueLine(entry);
    }

    private static string GetDialogueUnavailableLine(string runtimeBlockReason)
    {
        if (runtimeBlockReason.IndexOf("Research.ResearchModeOnly", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Orders are disabled in the mod config.";
        }

        if (runtimeBlockReason.IndexOf("load a save", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Load a save before giving orders.";
        }

        if (runtimeBlockReason.IndexOf("templates", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Companion orders are still loading.";
        }

        return "Try again when your companion is available.";
    }

    private static string GetDialogueCompanionName(PetRosterEntry entry)
    {
        string name = string.IsNullOrWhiteSpace(entry.DisplayName) ? "Companion" : entry.DisplayName.Trim();
        if (name.EndsWith(DialogueCandidateSuffix, StringComparison.Ordinal))
        {
            name = name.Substring(0, name.Length - DialogueCandidateSuffix.Length);
        }

        return string.IsNullOrWhiteSpace(name) ? "Companion" : name;
    }

    private static string GetDialogueRangePhrase(FollowRangeProfile range)
    {
        return NormalizeFollowRange(range) switch
        {
            FollowRangeProfile.Close => "stay close",
            FollowRangeProfile.Far => "keep some distance",
            _ => "keep a steady pace",
        };
    }

    private static bool IsDialogueButtonSelected(DialogueButtonKind kind)
    {
        return kind switch
        {
            DialogueButtonKind.ModeFollow => _companionMode == CompanionMode.Follow,
            DialogueButtonKind.ModeStay => _companionMode == CompanionMode.Stay,
            DialogueButtonKind.ModeDefend => _companionMode == CompanionMode.Defend,
            DialogueButtonKind.RangeClose => GetFollowRangeProfile() == FollowRangeProfile.Close,
            DialogueButtonKind.RangeNormal => GetFollowRangeProfile() == FollowRangeProfile.Normal,
            DialogueButtonKind.RangeFar => GetFollowRangeProfile() == FollowRangeProfile.Far,
            _ => false,
        };
    }

    private static void ApplyDialogueButtonState(DialogueButtonBinding binding, bool selected, bool hover)
    {
        bool interactable = binding.Button.interactable;
        Color normal = GetDialogueButtonColor(binding.Kind, selected, hover: false, interactable: true);
        Color hoverColor = GetDialogueButtonColor(binding.Kind, selected, hover: true, interactable: true);
        Color disabled = GetDialogueButtonColor(binding.Kind, selected, hover: false, interactable: false);
        ColorBlock colors = binding.Button.colors;
        colors.normalColor = normal;
        colors.highlightedColor = hoverColor;
        colors.selectedColor = hoverColor;
        colors.pressedColor = hoverColor;
        colors.disabledColor = disabled;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.06f;
        binding.Button.colors = colors;
        binding.Image.color = interactable ? GetDialogueButtonColor(binding.Kind, selected, hover, interactable: true) : disabled;
        binding.Accent.color = GetDialogueAccentColor(binding.Kind, selected, hover, interactable);
        binding.Label.color = GetDialogueTextColor(selected, hover, interactable);
        if (!string.Equals(binding.Label.text, binding.DisplayLabel, StringComparison.Ordinal))
        {
            binding.Label.text = binding.DisplayLabel;
        }
    }

    private static Color GetDialogueButtonColor(DialogueButtonKind kind, bool selected, bool hover, bool interactable)
    {
        if (!interactable)
        {
            return DialogueDisabledButtonColor;
        }

        if (kind == DialogueButtonKind.Danger)
        {
            return hover ? DialogueDangerHoverColor : DialogueDangerButtonColor;
        }

        if (selected)
        {
            return hover ? DialogueSelectedHoverColor : DialogueSelectedButtonColor;
        }

        return hover ? DialogueButtonHoverColor : DialogueButtonColor;
    }

    private static Color GetDialogueAccentColor(DialogueButtonKind kind, bool selected, bool hover, bool interactable)
    {
        if (!interactable)
        {
            return DialogueDisabledAccentColor;
        }

        if (kind == DialogueButtonKind.Danger)
        {
            return DialogueDangerAccentColor;
        }

        if (selected)
        {
            return DialogueSelectedAccentColor;
        }

        return hover ? DialogueHoverAccentColor : DialogueTransparentColor;
    }

    private static Color GetDialogueTextColor(bool selected, bool hover, bool interactable)
    {
        if (!interactable)
        {
            return DialogueDisabledTextColor;
        }

        return selected || hover ? DialogueHoverTextColor : DialogueTextColor;
    }

    private static void RunDialogueUiChoice(string commandId, string label, Action<ManualLogSource>? command, string route)
    {
        ManualLogSource? logger = _logger;
        if (logger == null)
        {
            Debug.LogWarning("Avalon Companions dialogue command ignored because the plugin logger is not ready.");
            return;
        }

        if (command == null)
        {
            WriteDialogueChoiceAudit(logger, commandId, label, route, "close-only", closesDialogue: true);
            CloseDialogueFromCommandSurface(logger, "goodbye", preserveStatus: false);
            return;
        }

        command(logger);
        WriteDialogueChoiceAudit(logger, commandId, label, route, "command-dispatched", closesDialogue: true);
        CloseDialogueFromCommandSurface(logger, "command:" + commandId, preserveStatus: true);
    }

    private static void WriteDialogueChoiceAudit(ManualLogSource logger, string commandId, string label, string route, string result, bool closesDialogue)
    {
        string reason =
            $"surface=custom-dialogue; commandId={commandId}; label={label}; route={route}; "
            + $"result={result}; closesDialogue={closesDialogue.ToString().ToLowerInvariant()}; "
            + "touchesTargeting=false; touchesPersistence=false";
        WriteCompanionCommandLog(logger, "dialogue-choice", affectedCount: 0, blocked: false, reason: reason);
    }

    private static void CloseDialogueFromCommandSurface(ManualLogSource? logger, string source, bool preserveStatus)
    {
        if (logger != null)
        {
            WriteCompanionCommandLog(
                logger,
                "dialogue-close",
                affectedCount: 0,
                blocked: false,
                reason: $"surface=custom-dialogue; source={source}; closesDialogue=true");
        }

        SetDialogueVisible(false, logger: null);
        if (!preserveStatus)
        {
            SetStatus(logger, "Companion menu closed");
        }
    }

    private static void TryHandleDialogueDirectPointerInput(ManualLogSource? logger)
    {
        if (_dialogueUiRoot == null || !Input.GetMouseButtonDown(0) || _lastDialoguePointerFrame == Time.frameCount)
        {
            return;
        }

        _lastDialoguePointerFrame = Time.frameCount;
        Vector2 pointer = Input.mousePosition;
        for (int i = DialogueButtons.Count - 1; i >= 0; i--)
        {
            DialogueButtonBinding binding = DialogueButtons[i];
            if (!binding.Button.interactable || !RectTransformUtility.RectangleContainsScreenPoint(binding.Rect, pointer, cam: null))
            {
                continue;
            }

            logger?.LogInfo($"Avalon Companions dialogue direct choice clicked. Choice={binding.Label.text}; route=manual-hit-test");
            RunDialogueUiChoice(binding.CommandId, binding.DisplayLabel, binding.Command, route: "manual-hit-test");

            return;
        }
    }

    private static void DestroyDialogueUiHost()
    {
        if (_dialogueUiRoot != null)
        {
            UnityEngine.Object.Destroy(_dialogueUiRoot);
            _dialogueUiRoot = null;
        }

        if (_dialogueEventSystemRoot != null)
        {
            UnityEngine.Object.Destroy(_dialogueEventSystemRoot);
            _dialogueEventSystemRoot = null;
        }

        DialogueButtons.Clear();
        _dialogueTextStack = null;
        _dialoguePortraitRoot = null;
        _dialoguePortraitBackgroundImage = null;
        _dialoguePortraitIconImage = null;
        _dialogueTitleText = null;
        _dialogueSubtitleText = null;
    }

    private enum DialogueButtonKind
    {
        Standard,
        Danger,
        ModeFollow,
        ModeStay,
        ModeDefend,
        RangeClose,
        RangeNormal,
        RangeFar,
    }

    private sealed class DialogueButtonBinding
    {
        internal DialogueButtonBinding(
            RectTransform rect,
            Button button,
            Image image,
            Image accent,
            Text label,
            string commandId,
            string displayLabel,
            DialogueButtonKind kind,
            bool requiresCompanion,
            Action<ManualLogSource>? command,
            Func<bool>? canExecute)
        {
            Rect = rect;
            Button = button;
            Image = image;
            Accent = accent;
            Label = label;
            CommandId = commandId;
            DisplayLabel = displayLabel;
            Kind = kind;
            RequiresCompanion = requiresCompanion;
            Command = command;
            CanExecute = canExecute;
        }

        internal RectTransform Rect { get; }
        internal Button Button { get; }
        internal Image Image { get; }
        internal Image Accent { get; }
        internal Text Label { get; }
        internal string CommandId { get; }
        internal string DisplayLabel { get; }
        internal DialogueButtonKind Kind { get; }
        internal bool RequiresCompanion { get; }
        internal Action<ManualLogSource>? Command { get; }
        internal Func<bool>? CanExecute { get; }
    }
}
