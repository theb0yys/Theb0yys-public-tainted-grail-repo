using System;
using System.Collections.Generic;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Locations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;

namespace AvalonHumanCompanions;

public sealed partial class Plugin
{
    private const int DialogueCanvasSortingOrder = 32760;
    private const float DialogueChoiceWidth = 660f;
    private const float DialogueChoiceHeight = 48f;
    private const float DialogueChoiceSpacing = 10f;
    private const int DialogueChoiceCount = 7;
    private const float DialogueTextWidth = 1380f;
    private const float DialogueTextHeight = 142f;
    private const float DialogueIconSize = 104f;
    private const float DialogueIconSpacing = 18f;
    private static readonly Vector2 DialogueIconPortraitAnchorMin = new(0.17f, 0.17f);
    private static readonly Vector2 DialogueIconPortraitAnchorMax = new(0.83f, 0.83f);

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

    private readonly List<HumanDialogueButtonBinding> _humanDialogueButtons = new();
    private GameObject? _humanDialogueUiRoot;
    private GameObject? _humanDialogueEventSystemRoot;
    private Font? _humanDialogueFont;
    private RawImage? _humanDialogueIconBackgroundImage;
    private RectTransform? _humanDialogueIconPortraitRect;
    private RawImage? _humanDialogueIconImage;
    private LayoutElement? _humanDialogueIconLayout;
    private Text? _humanDialogueTitleText;
    private Text? _humanDialogueSubtitleText;
    private int _lastHumanDialoguePointerFrame = -1;

    private void EnsureHumanCompanionDialogueUiHost()
    {
        if (!_dialogueVisible || !UseCompanionStyleCommandSurface)
        {
            DestroyHumanCompanionDialogueUiHost();
            return;
        }

        EnsureCursorForPanel();
        if (_humanDialogueUiRoot == null)
        {
            CreateHumanCompanionDialogueUiHost();
        }

        SyncHumanCompanionDialogueUiContent();
        TryHandleHumanDialogueDirectPointerInput();
    }

    private void CreateHumanCompanionDialogueUiHost()
    {
        DestroyHumanCompanionDialogueUiHost();
        EnsureHumanDialogueEventSystem();

        _humanDialogueUiRoot = new GameObject(
            "AvalonHumanCompanions.DialogueUi",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        DontDestroyOnLoad(_humanDialogueUiRoot);

        Canvas canvas = _humanDialogueUiRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = DialogueCanvasSortingOrder;

        CanvasScaler scaler = _humanDialogueUiRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform blocker = AddDialogueRect("Blocker", _humanDialogueUiRoot.transform);
        StretchDialogueRect(blocker);
        Image blockerImage = blocker.gameObject.AddComponent<Image>();
        blockerImage.color = DialogueBlockerColor;
        blockerImage.raycastTarget = true;

        RectTransform choices = AddDialogueRect("Choices", blocker);
        choices.anchorMin = new Vector2(1f, 0.53f);
        choices.anchorMax = new Vector2(1f, 0.53f);
        choices.pivot = new Vector2(1f, 0.5f);
        choices.anchoredPosition = new Vector2(-62f, 0f);
        choices.sizeDelta = new Vector2(DialogueChoiceWidth, DialogueChoiceHeight * DialogueChoiceCount + DialogueChoiceSpacing * (DialogueChoiceCount - 1));

        VerticalLayoutGroup choicesLayout = choices.gameObject.AddComponent<VerticalLayoutGroup>();
        choicesLayout.spacing = DialogueChoiceSpacing;
        choicesLayout.childAlignment = TextAnchor.MiddleRight;
        choicesLayout.childControlWidth = true;
        choicesLayout.childControlHeight = true;
        choicesLayout.childForceExpandWidth = true;
        choicesLayout.childForceExpandHeight = false;

        AddHumanDialogueButton(choices, "follow", "Follow me.", HumanDialogueButtonKind.ModeFollow, IsFollowChoiceAvailable, () => RunHumanProofCommandFromDialogue(NativeHumanCompanionCommand.Follow, "Follow"));
        AddHumanDialogueButton(choices, "hold", "Hold here.", HumanDialogueButtonKind.ModeHold, IsHoldChoiceAvailable, () => RunHumanProofCommandFromDialogue(NativeHumanCompanionCommand.Hold, "Hold"));
        AddHumanDialogueButton(choices, "defend", "Defend me.", HumanDialogueButtonKind.ModeDefend, IsDefendChoiceAvailable, () => RunHumanProofCommandFromDialogue(NativeHumanCompanionCommand.Defend, "Defend"));
        AddHumanDialogueButton(choices, "come-close", "Come close.", HumanDialogueButtonKind.Standard, () => IsProofCommandAvailable(NativeHumanCompanionCommand.ComeClose), () => RunHumanProofCommandFromDialogue(NativeHumanCompanionCommand.ComeClose, "Come Close"));
        AddHumanDialogueButton(choices, "recall", "Recall.", HumanDialogueButtonKind.Standard, () => IsProofCommandAvailable(NativeHumanCompanionCommand.Recall), () => RunHumanProofCommandFromDialogue(NativeHumanCompanionCommand.Recall, "Recall"));
        AddHumanDialogueButton(choices, "dismiss", "Part ways.", HumanDialogueButtonKind.Danger, () => IsProofCommandAvailable(NativeHumanCompanionCommand.Dismiss), () => RunHumanProofCommandFromDialogue(NativeHumanCompanionCommand.Dismiss, "Part ways"));
        AddHumanDialogueButton(choices, "goodbye", "Goodbye.", HumanDialogueButtonKind.Standard, () => true, null);

        RectTransform dialogueText = AddDialogueRect("DialogueText", blocker);
        dialogueText.anchorMin = new Vector2(0.5f, 0f);
        dialogueText.anchorMax = new Vector2(0.5f, 0f);
        dialogueText.pivot = new Vector2(0.5f, 0f);
        dialogueText.anchoredPosition = new Vector2(0f, 42f);
        dialogueText.sizeDelta = new Vector2(DialogueTextWidth, DialogueTextHeight);

        Image textImage = dialogueText.gameObject.AddComponent<Image>();
        textImage.color = DialogueBottomTextColor;
        textImage.raycastTarget = false;

        HorizontalLayoutGroup textLayout = dialogueText.gameObject.AddComponent<HorizontalLayoutGroup>();
        textLayout.padding = new RectOffset(34, 34, 18, 18);
        textLayout.spacing = DialogueIconSpacing;
        textLayout.childAlignment = TextAnchor.MiddleCenter;
        textLayout.childControlWidth = true;
        textLayout.childControlHeight = true;
        textLayout.childForceExpandWidth = false;
        textLayout.childForceExpandHeight = false;

        RectTransform iconRect = AddDialogueRect("CompanionIcon", dialogueText);
        _humanDialogueIconBackgroundImage = iconRect.gameObject.AddComponent<RawImage>();
        _humanDialogueIconBackgroundImage.color = DialogueTransparentColor;
        _humanDialogueIconBackgroundImage.raycastTarget = false;
        _humanDialogueIconLayout = iconRect.gameObject.AddComponent<LayoutElement>();
        _humanDialogueIconLayout.minWidth = 0f;
        _humanDialogueIconLayout.preferredWidth = 0f;
        _humanDialogueIconLayout.minHeight = 0f;
        _humanDialogueIconLayout.preferredHeight = 0f;

        _humanDialogueIconPortraitRect = AddDialogueRect("Portrait", iconRect);
        StretchDialogueRect(_humanDialogueIconPortraitRect);
        _humanDialogueIconImage = _humanDialogueIconPortraitRect.gameObject.AddComponent<RawImage>();
        _humanDialogueIconImage.color = DialogueTransparentColor;
        _humanDialogueIconImage.raycastTarget = false;

        RectTransform textColumn = AddDialogueRect("TextColumn", dialogueText);
        VerticalLayoutGroup textColumnLayout = textColumn.gameObject.AddComponent<VerticalLayoutGroup>();
        textColumnLayout.spacing = 7f;
        textColumnLayout.childAlignment = TextAnchor.MiddleCenter;
        textColumnLayout.childControlWidth = true;
        textColumnLayout.childControlHeight = true;
        textColumnLayout.childForceExpandWidth = true;
        textColumnLayout.childForceExpandHeight = false;

        LayoutElement textColumnElement = textColumn.gameObject.AddComponent<LayoutElement>();
        textColumnElement.flexibleWidth = 1f;
        textColumnElement.minHeight = 72f;

        _humanDialogueTitleText = AddDialogueText(textColumn, "Line", 31, FontStyle.BoldAndItalic, DialogueTextColor, TextAnchor.MiddleCenter, wrap: true);
        _humanDialogueSubtitleText = AddDialogueText(textColumn, "Detail", 18, FontStyle.Bold, DialogueMutedTextColor, TextAnchor.MiddleCenter, wrap: true);

        Logger.LogInfo($"{PluginName} opened vanilla-style Unity UI human companion command host. renderer=UnityEngine.UI; action=human-companion-dialogue-ui; layout=vanilla-style");
    }

    private void EnsureHumanDialogueEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        foreach (EventSystem eventSystem in FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
        {
            if (eventSystem != null && eventSystem.isActiveAndEnabled)
            {
                EventSystem.current = eventSystem;
                return;
            }
        }

        _humanDialogueEventSystemRoot = new GameObject(
            "AvalonHumanCompanions.DialogueEventSystem",
            typeof(EventSystem),
            typeof(StandaloneInputModule));
        DontDestroyOnLoad(_humanDialogueEventSystemRoot);
    }

    private static RectTransform AddDialogueRect(string name, Transform parent)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, worldPositionStays: false);
        return child.GetComponent<RectTransform>();
    }

    private static void StretchDialogueRect(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private Text AddDialogueText(Transform parent, string name, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment, bool wrap)
    {
        RectTransform rect = AddDialogueRect(name, parent);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = GetHumanDialogueFont();
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

    private void AddHumanDialogueButton(
        Transform parent,
        string commandId,
        string label,
        HumanDialogueButtonKind kind,
        Func<bool> isAvailable,
        Action? command)
    {
        RectTransform rect = AddDialogueRect(label.Replace(" ", string.Empty).Replace(".", string.Empty) + "Choice", parent);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = GetDialogueButtonColor(kind, selected: false, hover: false, interactable: true);

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        LayoutElement element = rect.gameObject.AddComponent<LayoutElement>();
        element.minHeight = DialogueChoiceHeight;
        element.preferredHeight = DialogueChoiceHeight;
        element.flexibleWidth = 1f;

        RectTransform accentRect = AddDialogueRect("StateAccent", rect);
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.offsetMin = new Vector2(0f, 0f);
        accentRect.offsetMax = new Vector2(7f, 0f);
        Image accent = accentRect.gameObject.AddComponent<Image>();
        accent.color = DialogueTransparentColor;
        accent.raycastTarget = false;

        RectTransform labelRect = AddDialogueRect("Label", rect);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(34f, 0f);
        labelRect.offsetMax = new Vector2(-18f, 0f);

        Text text = labelRect.gameObject.AddComponent<Text>();
        text.font = GetHumanDialogueFont();
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

        button.onClick.AddListener(() => RunHumanDialogueChoice(commandId, label, command, route: "unity-button"));

        HumanDialogueButtonBinding binding = new HumanDialogueButtonBinding(rect, button, image, accent, text, commandId, label, kind, isAvailable, command);
        _humanDialogueButtons.Add(binding);
        ApplyHumanDialogueButtonState(binding, selected: false, hover: false);
    }

    private Font GetHumanDialogueFont()
    {
        if (_humanDialogueFont != null)
        {
            return _humanDialogueFont;
        }

        _humanDialogueFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return _humanDialogueFont;
    }

    private void SyncHumanCompanionDialogueUiContent()
    {
        if (_humanDialogueUiRoot == null)
        {
            return;
        }

        Location? location = _lastAllyProof;
        bool hasProofActor = IsManagedAllyProof(location);
        string displayName = hasProofActor ? GetActiveProofNameLabel() : "Companion";

        SetHumanDialogueText(
            _humanDialogueTitleText,
            hasProofActor ? $"What do you need, {displayName}?" : "No companion is with you.");
        SetHumanDialogueText(
            _humanDialogueSubtitleText,
            hasProofActor
                ? GetHumanCompanionDialogueStatusLine()
                : GetHumanDialogueBlockReason());
        SetHumanDialogueIcon(
            hasProofActor ? TaintedInterfaceBridge.GetFullEmbeddedIcon(GetActiveHumanIconRelativePath()) : null,
            hasProofActor ? TaintedInterfaceBridge.GetFullEmbeddedIconAndDialogueBackground(HumanIconBackground) : null);

        foreach (HumanDialogueButtonBinding binding in _humanDialogueButtons)
        {
            bool interactable = binding.IsAvailable();
            bool selected = IsHumanDialogueButtonSelected(binding.Kind);
            bool hover = interactable && RectTransformUtility.RectangleContainsScreenPoint(binding.Rect, Input.mousePosition, cam: null);
            binding.Button.interactable = interactable;
            ApplyHumanDialogueButtonState(binding, selected, hover);
        }
    }

    private static void SetHumanDialogueText(Text? text, string value)
    {
        if (text != null && !string.Equals(text.text, value, StringComparison.Ordinal))
        {
            text.text = value;
        }
    }

    private void SetHumanDialogueIcon(Texture2D? icon, Texture2D? background)
    {
        if (_humanDialogueIconImage == null || _humanDialogueIconLayout == null || _humanDialogueIconBackgroundImage == null || _humanDialogueIconPortraitRect == null)
        {
            return;
        }

        if (icon == null)
        {
            _humanDialogueIconImage.texture = null;
            _humanDialogueIconImage.color = DialogueTransparentColor;
            _humanDialogueIconBackgroundImage.texture = null;
            _humanDialogueIconBackgroundImage.color = DialogueTransparentColor;
            _humanDialogueIconLayout.minWidth = 0f;
            _humanDialogueIconLayout.preferredWidth = 0f;
            _humanDialogueIconLayout.minHeight = 0f;
            _humanDialogueIconLayout.preferredHeight = 0f;
            return;
        }

        _humanDialogueIconBackgroundImage.texture = background;
        _humanDialogueIconBackgroundImage.color = background == null ? DialogueTransparentColor : Color.white;
        if (background == null)
        {
            StretchDialogueRect(_humanDialogueIconPortraitRect);
        }
        else
        {
            _humanDialogueIconPortraitRect.anchorMin = DialogueIconPortraitAnchorMin;
            _humanDialogueIconPortraitRect.anchorMax = DialogueIconPortraitAnchorMax;
            _humanDialogueIconPortraitRect.offsetMin = Vector2.zero;
            _humanDialogueIconPortraitRect.offsetMax = Vector2.zero;
        }

        _humanDialogueIconImage.texture = icon;
        _humanDialogueIconImage.color = Color.white;
        _humanDialogueIconLayout.minWidth = DialogueIconSize;
        _humanDialogueIconLayout.preferredWidth = DialogueIconSize;
        _humanDialogueIconLayout.minHeight = DialogueIconSize;
        _humanDialogueIconLayout.preferredHeight = DialogueIconSize;
    }

    private string GetHumanDialogueBlockReason()
    {
        if (!_enabled.Value)
        {
            return "Companion commands are unavailable.";
        }

        if (_researchModeOnly.Value)
        {
            return "Companion commands are unavailable right now.";
        }

        if (CanPanelSpawnProof())
        {
            return "No companion is currently with you.";
        }

        if (!_enableOneSessionAllyProof.Value)
        {
            return "Companion summoning is disabled.";
        }

        if (string.IsNullOrWhiteSpace(_templateGuid.Value))
        {
            return "Choose a companion first.";
        }

        if (_limitOneAllyProofPerSession.Value && _allyProofSpawnedThisSession)
        {
            return "A companion was already called this session.";
        }

        return "No companion is currently with you.";
    }

    private string GetHumanCompanionDialogueStatusLine()
    {
        int attackerCount = CountLiveHeroAttackers(Hero.Current);
        return _allyProofMode switch
        {
            HumanProofMode.Follow => attackerCount > 0 && ShouldCompanionBrainProtect(attackerCount) ? "I'll protect you." : "I'll keep close.",
            HumanProofMode.Hold => "I'll wait here.",
            HumanProofMode.Defend => attackerCount > 0 ? "I'll protect you." : "I'll watch for threats.",
            _ => "I'm ready.",
        };
    }

    private bool IsHumanDialogueButtonSelected(HumanDialogueButtonKind kind)
    {
        return kind switch
        {
            HumanDialogueButtonKind.ModeFollow => _allyProofMode == HumanProofMode.Follow && IsManagedAllyProof(_lastAllyProof),
            HumanDialogueButtonKind.ModeHold => _allyProofMode == HumanProofMode.Hold && IsManagedAllyProof(_lastAllyProof),
            HumanDialogueButtonKind.ModeDefend => _allyProofMode == HumanProofMode.Defend && IsManagedAllyProof(_lastAllyProof),
            _ => false,
        };
    }

    private void ApplyHumanDialogueButtonState(HumanDialogueButtonBinding binding, bool selected, bool hover)
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

    private static Color GetDialogueButtonColor(HumanDialogueButtonKind kind, bool selected, bool hover, bool interactable)
    {
        if (!interactable)
        {
            return DialogueDisabledButtonColor;
        }

        if (kind == HumanDialogueButtonKind.Danger)
        {
            return hover ? DialogueDangerHoverColor : DialogueDangerButtonColor;
        }

        if (selected)
        {
            return hover ? DialogueSelectedHoverColor : DialogueSelectedButtonColor;
        }

        return hover ? DialogueButtonHoverColor : DialogueButtonColor;
    }

    private static Color GetDialogueAccentColor(HumanDialogueButtonKind kind, bool selected, bool hover, bool interactable)
    {
        if (!interactable)
        {
            return DialogueDisabledAccentColor;
        }

        if (kind == HumanDialogueButtonKind.Danger)
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

    private bool IsFollowChoiceAvailable()
    {
        return IsProofCommandAvailable(NativeHumanCompanionCommand.Follow);
    }

    private bool IsHoldChoiceAvailable()
    {
        return IsProofCommandAvailable(NativeHumanCompanionCommand.Hold);
    }

    private bool IsDefendChoiceAvailable()
    {
        return IsProofCommandAvailable(NativeHumanCompanionCommand.Defend);
    }

    private void RunHumanDialogueChoice(string commandId, string label, Action? command, string route)
    {
        if (command == null)
        {
            Logger.LogInfo($"{PluginName} human companion dialogue choice closed. commandId={commandId}; label={label}; route={route}; touchesTargeting=false; touchesPersistence=false");
            SetDialogueVisible(false, "dialogue goodbye");
            return;
        }

        command();
        Logger.LogInfo($"{PluginName} human companion dialogue choice dispatched. commandId={commandId}; label={label}; route={route}; touchesTargeting=false; touchesPersistence=false");
        SetDialogueVisible(false, "dialogue command " + commandId);
    }

    private void RunHumanProofCommandFromDialogue(NativeHumanCompanionCommand command, string label)
    {
        _panelStatus = ExecuteProofCommand(command, "dialogue");
    }

    private void TryHandleHumanDialogueDirectPointerInput()
    {
        if (_humanDialogueUiRoot == null || !Input.GetMouseButtonDown(0) || _lastHumanDialoguePointerFrame == Time.frameCount)
        {
            return;
        }

        _lastHumanDialoguePointerFrame = Time.frameCount;
        Vector2 pointer = Input.mousePosition;
        for (int i = _humanDialogueButtons.Count - 1; i >= 0; i--)
        {
            HumanDialogueButtonBinding binding = _humanDialogueButtons[i];
            if (!binding.Button.interactable || !RectTransformUtility.RectangleContainsScreenPoint(binding.Rect, pointer, cam: null))
            {
                continue;
            }

            Logger.LogInfo($"{PluginName} human companion dialogue direct choice clicked. Choice={binding.DisplayLabel}; route=manual-hit-test");
            RunHumanDialogueChoice(binding.CommandId, binding.DisplayLabel, binding.Command, route: "manual-hit-test");
            return;
        }
    }

    private void DestroyHumanCompanionDialogueUiHost()
    {
        if (_humanDialogueUiRoot != null)
        {
            Destroy(_humanDialogueUiRoot);
            _humanDialogueUiRoot = null;
        }

        if (_humanDialogueEventSystemRoot != null)
        {
            Destroy(_humanDialogueEventSystemRoot);
            _humanDialogueEventSystemRoot = null;
        }

        _humanDialogueButtons.Clear();
        _humanDialogueIconBackgroundImage = null;
        _humanDialogueIconPortraitRect = null;
        _humanDialogueIconImage = null;
        _humanDialogueIconLayout = null;
        _humanDialogueTitleText = null;
        _humanDialogueSubtitleText = null;
    }

    private enum HumanDialogueButtonKind
    {
        Standard,
        Danger,
        ModeFollow,
        ModeHold,
        ModeDefend,
    }

    private sealed class HumanDialogueButtonBinding
    {
        internal HumanDialogueButtonBinding(
            RectTransform rect,
            Button button,
            Image image,
            Image accent,
            Text label,
            string commandId,
            string displayLabel,
            HumanDialogueButtonKind kind,
            Func<bool> isAvailable,
            Action? command)
        {
            Rect = rect;
            Button = button;
            Image = image;
            Accent = accent;
            Label = label;
            CommandId = commandId;
            DisplayLabel = displayLabel;
            Kind = kind;
            IsAvailable = isAvailable;
            Command = command;
        }

        internal RectTransform Rect { get; }
        internal Button Button { get; }
        internal Image Image { get; }
        internal Image Accent { get; }
        internal Text Label { get; }
        internal string CommandId { get; }
        internal string DisplayLabel { get; }
        internal HumanDialogueButtonKind Kind { get; }
        internal Func<bool> IsAvailable { get; }
        internal Action? Command { get; }
    }
}
