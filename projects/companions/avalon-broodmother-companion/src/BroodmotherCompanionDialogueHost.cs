using System;
using System.Collections.Generic;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Locations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GameForceCursorVisibility = Awaken.TG.Main.UI.Cursors.ForceCursorVisibility;
using Text = UnityEngine.UI.Text;

namespace AvalonBroodmotherCompanion;

internal sealed class BroodmotherCompanionDialogueHost
{
    private const int CanvasSortingOrder = 32760;
    private const float ChoiceWidth = 660f;
    private const float ChoiceHeight = 48f;
    private const float ChoiceSpacing = 10f;
    private const float TextBandWidth = 1380f;
    private const float TextBandHeight = 142f;
    private const float TextInsetX = 34f;
    private const float TextPortraitInsetX = 156f;
    private const float PortraitSize = 104f;
    private const float PortraitIconSize = 64f;
    private static readonly Color BlockerColor = new(0f, 0f, 0f, 0.01f);
    private static readonly Color TextBandColor = new(0f, 0f, 0f, 0.62f);
    private static readonly Color ButtonColor = new(0.115f, 0.150f, 0.150f, 0.98f);
    private static readonly Color ButtonHoverColor = new(0.170f, 0.220f, 0.215f, 0.98f);
    private static readonly Color ButtonSelectedColor = new(0.380f, 0.270f, 0.105f, 0.98f);
    private static readonly Color ButtonSelectedHoverColor = new(0.470f, 0.340f, 0.140f, 0.98f);
    private static readonly Color ButtonDangerColor = new(0.310f, 0.095f, 0.085f, 0.98f);
    private static readonly Color ButtonDangerHoverColor = new(0.440f, 0.125f, 0.110f, 0.98f);
    private static readonly Color ButtonDisabledColor = new(0.120f, 0.130f, 0.130f, 0.58f);
    private static readonly Color TransparentColor = new(0f, 0f, 0f, 0f);
    private static readonly Color AccentColor = new(0.920f, 0.640f, 0.210f, 1f);
    private static readonly Color HoverAccentColor = new(0.660f, 0.700f, 0.590f, 0.92f);
    private static readonly Color DangerAccentColor = new(0.860f, 0.230f, 0.160f, 1f);
    private static readonly Color DisabledAccentColor = new(0.320f, 0.350f, 0.340f, 0.72f);
    private static readonly Color TextColor = new(0.925f, 0.905f, 0.825f, 1f);
    private static readonly Color HoverTextColor = new(1.000f, 0.965f, 0.835f, 1f);
    private static readonly Color DisabledTextColor = new(0.520f, 0.560f, 0.535f, 0.90f);
    private static readonly Color MutedTextColor = new(0.715f, 0.760f, 0.725f, 1f);
    private static readonly Color PortraitFallbackColor = new(0.055f, 0.060f, 0.058f, 0.84f);
    private static readonly Color PortraitBackgroundColor = new(1f, 1f, 1f, 0.90f);
    private static readonly Color PortraitIconColor = new(1f, 1f, 1f, 0.96f);

    private readonly Plugin _plugin;
    private readonly List<ButtonBinding> _buttons = new();
    private GameObject? _root;
    private GameObject? _eventSystemRoot;
    private Location? _location;
    private Font? _font;
    private RectTransform? _textStack;
    private GameObject? _portraitRoot;
    private RawImage? _portraitBackgroundImage;
    private RawImage? _portraitIconImage;
    private GameForceCursorVisibility? _forceCursorVisibilityElement;
    private Text? _titleText;
    private Text? _subtitleText;
    private bool _visible;
    private bool _modManagerScopeActive;
    private bool _taintedInterfaceScopeActive;
    private bool _cursorCaptured;
    private bool _previousCursorVisible;
    private CursorLockMode _previousCursorLockState;
    private int _lastChoiceFrame = -1;
    private int _lastPointerFrame = -1;

    internal BroodmotherCompanionDialogueHost(Plugin plugin)
    {
        _plugin = plugin;
    }

    internal bool Visible => _visible;

    internal void Open(Location location)
    {
        _location = location;
        _visible = true;
        _plugin.SetPanelStatus("Companion menu opened.");
        try
        {
            EnsureCursor();
            EnsureHost();
            Sync();
        }
        catch (Exception ex)
        {
            _plugin.ModLogger.LogWarning($"{Plugin.PluginName} dialogue host failed to open: {ex.GetType().Name}: {ex.Message}");
            Close("host-open-failed", preserveStatus: true);
        }
    }

    internal void Tick()
    {
        if (!_visible)
        {
            DestroyHost();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close("escape");
            return;
        }

        if (!_plugin.TryGetActiveBroodmotherForDialogue(out Location? activeLocation, out _) ||
            _location == null ||
            !ReferenceEquals(activeLocation, _location))
        {
            Close("companion-unavailable");
            return;
        }

        EnsureCursor();
        EnsureHost();
        Sync();
        TryHandleDirectPointerInput();
    }

    internal void MaintainFromGuiPass()
    {
        if (!_visible)
        {
            return;
        }

        if (!_plugin.TryGetActiveBroodmotherForDialogue(out Location? activeLocation, out _) ||
            _location == null ||
            !ReferenceEquals(activeLocation, _location))
        {
            return;
        }

        try
        {
            EnsureCursor();
            EnsureHost();
            Sync();
            TryHandleDirectPointerInput();
        }
        catch (Exception ex)
        {
            _plugin.ModLogger.LogWarning($"{Plugin.PluginName} dialogue host GUI pass failed: {ex.GetType().Name}: {ex.Message}");
            Close("gui-pass-maintenance-failed", preserveStatus: true);
        }
    }

    internal void Close(string source, bool preserveStatus = false)
    {
        Location? location = _location;
        NpcElement? npcElement = null;
        if (location != null && !location.HasBeenDiscarded && location.TryGetElement(out NpcElement resolvedNpcElement))
        {
            npcElement = resolvedNpcElement;
        }

        bool wasVisible = _visible || _root != null;
        bool usedModManagerScope = _modManagerScopeActive;
        bool usedTaintedInterfaceScope = _taintedInterfaceScopeActive;
        _visible = false;
        _location = null;
        DestroyHost();
        ReleaseCursor();

        if (wasVisible)
        {
            _plugin.LogDialogueHostEvent(
                "dialogue-close",
                "closed",
                $"surface=unity-ui-native-companion;source={source};modManagerScope={usedModManagerScope};taintedInterfaceScope={usedTaintedInterfaceScope};storyGraph=0;saveWrite=0",
                location,
                npcElement);
            _plugin.ModLogger.LogInfo($"{Plugin.PluginName} companion dialogue host closed. source={source}; renderer=UnityEngine.UI; storyGraph=0; saveWrite=0.");
        }

        if (!preserveStatus)
        {
            _plugin.SetPanelStatus("Companion menu closed.");
        }
    }

    internal void Destroy()
    {
        _visible = false;
        _location = null;
        DestroyHost();
        ReleaseCursor();
    }

    private void EnsureHost()
    {
        if (_root != null)
        {
            return;
        }

        EnsureEventSystem();
        _root = new GameObject(
            "AvalonBroodmotherCompanion.DialogueUi",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        UnityEngine.Object.DontDestroyOnLoad(_root);

        Canvas canvas = _root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = CanvasSortingOrder;

        CanvasScaler scaler = _root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform blocker = AddRect("Blocker", _root.transform);
        Stretch(blocker);
        Image blockerImage = blocker.gameObject.AddComponent<Image>();
        blockerImage.color = BlockerColor;
        blockerImage.raycastTarget = true;

        CreateChoices(blocker);
        CreateTextBand(blocker);

        _plugin.ModLogger.LogInfo($"{Plugin.PluginName} opened companion-style Unity UI Broodmother dialogue host. renderer=UnityEngine.UI; action=native-companion-prompt; layout=avalon-companions-style; taintedInterface=reflection; storyGraph=0; saveWrite=0.");
    }

    private void CreateChoices(Transform parent)
    {
        RectTransform choices = AddRect("Choices", parent);
        choices.anchorMin = new Vector2(1f, 0.53f);
        choices.anchorMax = new Vector2(1f, 0.53f);
        choices.pivot = new Vector2(1f, 0.5f);
        choices.anchoredPosition = new Vector2(-62f, 0f);
        const int choiceCount = 11;
        choices.sizeDelta = new Vector2(
            ChoiceWidth,
            ChoiceHeight * choiceCount + ChoiceSpacing * Math.Max(0, choiceCount - 1));

        VerticalLayoutGroup layout = choices.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = ChoiceSpacing;
        layout.childAlignment = TextAnchor.MiddleRight;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        AddButton(choices, "follow", "Follow", ButtonKind.ModeFollow, true, BroodmotherDialogueCommand.Follow);
        AddButton(choices, "stay", "Hold Position", ButtonKind.ModeStay, true, BroodmotherDialogueCommand.HoldPosition);
        AddButton(choices, "defend", "Defend", ButtonKind.ModeDefend, true, BroodmotherDialogueCommand.Defend);
        AddButton(choices, "range-close", "Keep Close", ButtonKind.RangeClose, true, BroodmotherDialogueCommand.RangeClose);
        AddButton(choices, "range-normal", "Keep Pace", ButtonKind.RangeNormal, true, BroodmotherDialogueCommand.RangeNormal);
        AddButton(choices, "range-far", "Keep Distance", ButtonKind.RangeFar, true, BroodmotherDialogueCommand.RangeFar);
        AddButton(choices, "come-close", "Come Close", ButtonKind.Standard, true, BroodmotherDialogueCommand.ComeClose);
        AddButton(choices, "recall", "Recall", ButtonKind.Standard, true, BroodmotherDialogueCommand.Recall);
        AddButton(choices, "recover", "Recover", ButtonKind.Standard, true, BroodmotherDialogueCommand.Recover);
        AddButton(choices, "dismiss", "Dismiss", ButtonKind.Danger, true, BroodmotherDialogueCommand.Dismiss);
        AddButton(choices, "goodbye", "Leave", ButtonKind.Standard, false, null);
    }

    private void CreateTextBand(Transform parent)
    {
        RectTransform band = AddRect("DialogueText", parent);
        band.anchorMin = new Vector2(0.5f, 0f);
        band.anchorMax = new Vector2(0.5f, 0f);
        band.pivot = new Vector2(0.5f, 0f);
        band.anchoredPosition = new Vector2(0f, 42f);
        band.sizeDelta = new Vector2(TextBandWidth, TextBandHeight);

        Image image = band.gameObject.AddComponent<Image>();
        image.color = TextBandColor;
        image.raycastTarget = false;

        RectTransform portraitFrame = AddRect("CompanionPortrait", band);
        portraitFrame.anchorMin = new Vector2(0f, 0.5f);
        portraitFrame.anchorMax = new Vector2(0f, 0.5f);
        portraitFrame.pivot = new Vector2(0f, 0.5f);
        portraitFrame.anchoredPosition = new Vector2(24f, 0f);
        portraitFrame.sizeDelta = new Vector2(PortraitSize, PortraitSize);

        Image portraitFallback = portraitFrame.gameObject.AddComponent<Image>();
        portraitFallback.color = PortraitFallbackColor;
        portraitFallback.raycastTarget = false;

        RectTransform portraitBackground = AddRect("Background", portraitFrame);
        Stretch(portraitBackground);
        _portraitBackgroundImage = portraitBackground.gameObject.AddComponent<RawImage>();
        _portraitBackgroundImage.color = PortraitBackgroundColor;
        _portraitBackgroundImage.raycastTarget = false;

        RectTransform portraitIcon = AddRect("Icon", portraitFrame);
        portraitIcon.anchorMin = new Vector2(0.5f, 0.5f);
        portraitIcon.anchorMax = new Vector2(0.5f, 0.5f);
        portraitIcon.pivot = new Vector2(0.5f, 0.5f);
        portraitIcon.anchoredPosition = Vector2.zero;
        portraitIcon.sizeDelta = new Vector2(PortraitIconSize, PortraitIconSize);
        _portraitIconImage = portraitIcon.gameObject.AddComponent<RawImage>();
        _portraitIconImage.color = PortraitIconColor;
        _portraitIconImage.raycastTarget = false;
        _portraitRoot = portraitFrame.gameObject;
        _portraitRoot.SetActive(false);

        _textStack = AddRect("DialogueTextStack", band);
        _textStack.anchorMin = Vector2.zero;
        _textStack.anchorMax = Vector2.one;
        _textStack.offsetMin = new Vector2(TextInsetX, 0f);
        _textStack.offsetMax = new Vector2(-TextInsetX, 0f);

        VerticalLayoutGroup layout = _textStack.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 18, 18);
        layout.spacing = 7f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        _titleText = AddText(_textStack, "Line", 31, FontStyle.BoldAndItalic, TextColor, TextAnchor.MiddleCenter, wrap: true);
        _subtitleText = AddText(_textStack, "Detail", 18, FontStyle.Bold, MutedTextColor, TextAnchor.MiddleCenter, wrap: true);
    }

    private void AddButton(
        Transform parent,
        string commandId,
        string label,
        ButtonKind kind,
        bool requiresCompanion,
        BroodmotherDialogueCommand? command)
    {
        RectTransform rect = AddRect(label.Replace(" ", string.Empty).Replace(".", string.Empty) + "Choice", parent);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = GetButtonColor(kind, selected: false, hover: false, interactable: true);

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        LayoutElement element = rect.gameObject.AddComponent<LayoutElement>();
        element.minHeight = ChoiceHeight;
        element.preferredHeight = ChoiceHeight;
        element.flexibleWidth = 1f;

        RectTransform accentRect = AddRect("StateAccent", rect);
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.offsetMin = Vector2.zero;
        accentRect.offsetMax = new Vector2(7f, 0f);
        Image accent = accentRect.gameObject.AddComponent<Image>();
        accent.color = TransparentColor;
        accent.raycastTarget = false;

        RectTransform labelRect = AddRect("Label", rect);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(34f, 0f);
        labelRect.offsetMax = new Vector2(-18f, 0f);

        Text text = labelRect.gameObject.AddComponent<Text>();
        text.font = GetFont();
        text.fontSize = 24;
        text.fontStyle = FontStyle.Bold;
        text.color = TextColor;
        text.text = label;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 16;
        text.resizeTextMaxSize = 24;
        text.raycastTarget = false;

        ButtonBinding binding = new(rect, button, image, accent, text, commandId, label, kind, requiresCompanion, command);
        button.onClick.AddListener(() => RunChoice(binding, "unity-button"));
        _buttons.Add(binding);
        ApplyButtonState(binding, selected: false, hover: false);
    }

    private void Sync()
    {
        if (_root == null)
        {
            return;
        }

        bool hasCompanion = _plugin.TryGetActiveBroodmotherForDialogue(out Location? location, out _);
        bool portraitVisible = SyncPortrait(hasCompanion);
        SyncTextStack(portraitVisible);

        if (!hasCompanion || location == null)
        {
            SetText(_titleText, "Companion unavailable.");
            SetText(_subtitleText, "The companion cannot take orders right now.");
        }
        else
        {
            SetText(_titleText, GetDisplayName(location) + " is listening.");
            SetText(_subtitleText, _plugin.DialogueStatusLine);
        }

        foreach (ButtonBinding binding in _buttons)
        {
            bool interactable = !binding.RequiresCompanion || hasCompanion;
            bool selected = interactable && IsButtonSelected(binding.Kind);
            bool hover = interactable && RectTransformUtility.RectangleContainsScreenPoint(binding.Rect, Input.mousePosition, cam: null);
            binding.Button.interactable = interactable;
            ApplyButtonState(binding, selected, hover);
        }
    }

    private bool SyncPortrait(bool visible)
    {
        if (_portraitRoot == null)
        {
            return false;
        }

        if (!visible)
        {
            SetPortraitVisible(false);
            return false;
        }

        Texture2D? icon = GetActiveCompanionIconTexture(out _) ??
            TaintedInterfaceReflection.GetIcon(Plugin.CompanionIconId);
        if (icon == null)
        {
            SetPortraitVisible(false);
            return false;
        }

        Texture2D? background = TaintedInterfaceReflection.GetIcon(Plugin.CompanionHudBackgroundIconId);
        SetPortraitVisible(true);

        if (_portraitIconImage != null)
        {
            _portraitIconImage.texture = icon;
            _portraitIconImage.enabled = true;
        }

        if (_portraitBackgroundImage != null)
        {
            _portraitBackgroundImage.texture = background;
            _portraitBackgroundImage.enabled = background != null;
        }

        return true;
    }

    private void SetPortraitVisible(bool visible)
    {
        if (_portraitRoot != null && _portraitRoot.activeSelf != visible)
        {
            _portraitRoot.SetActive(visible);
        }
    }

    private void SyncTextStack(bool hasPortrait)
    {
        if (_textStack == null)
        {
            return;
        }

        float leftInset = hasPortrait ? TextPortraitInsetX : TextInsetX;
        _textStack.offsetMin = new Vector2(leftInset, 0f);
        _textStack.offsetMax = new Vector2(-TextInsetX, 0f);
    }

    private bool IsButtonSelected(ButtonKind kind)
    {
        return kind switch
        {
            ButtonKind.ModeFollow => !_plugin.IsDialogueDefendMode && !_plugin.IsDialogueHoldPositionMode,
            ButtonKind.ModeStay => _plugin.IsDialogueHoldPositionMode,
            ButtonKind.ModeDefend => _plugin.IsDialogueDefendMode,
            ButtonKind.RangeClose => Plugin.NormalizeFollowRange(_plugin.DialogueFollowRange) == BroodmotherFollowRange.Close,
            ButtonKind.RangeNormal => Plugin.NormalizeFollowRange(_plugin.DialogueFollowRange) == BroodmotherFollowRange.Normal,
            ButtonKind.RangeFar => Plugin.NormalizeFollowRange(_plugin.DialogueFollowRange) == BroodmotherFollowRange.Far,
            _ => false,
        };
    }

    private void RunChoice(ButtonBinding binding, string route)
    {
        if (_lastChoiceFrame == Time.frameCount)
        {
            return;
        }

        _lastChoiceFrame = Time.frameCount;
        if (!_plugin.TryGetActiveBroodmotherForDialogue(out Location? location, out NpcElement? npcElement))
        {
            Close("choice-unavailable");
            return;
        }

        if (!binding.Command.HasValue)
        {
            _plugin.LogDialogueHostEvent(
                "dialogue-choice",
                "leave",
                $"surface=unity-ui-native-companion;commandId={binding.CommandId};label={binding.DisplayLabel};route={route};closesDialogue=true;storyGraph=0;saveWrite=0",
                location,
                npcElement);
            Close("leave");
            return;
        }

        BroodmotherDialogueCommand command = binding.Command.Value;
        string result = command.ToString().ToLowerInvariant();
        bool closesDialogue = ShouldCloseAfterCommand(command);
        _plugin.LogDialogueHostEvent(
            "dialogue-choice",
            result,
            $"surface=unity-ui-native-companion;commandId={binding.CommandId};label={binding.DisplayLabel};route={route};closesDialogue={(closesDialogue ? "true" : "false")};storyGraph=0;saveWrite=0",
            location,
            npcElement);
        _plugin.ModLogger.LogInfo($"{Plugin.PluginName} companion dialogue choice dispatched. command={command}; route={route}; renderer=UnityEngine.UI; storyGraph=0; saveWrite=0.");
        _plugin.RunDialogueCommand(command, "companion dialogue:" + binding.CommandId);

        if (!Visible)
        {
            return;
        }

        if (closesDialogue)
        {
            Close("command:" + result, preserveStatus: true);
            return;
        }

        Sync();
    }

    private static bool ShouldCloseAfterCommand(BroodmotherDialogueCommand command)
    {
        return command == BroodmotherDialogueCommand.Dismiss;
    }

    private void TryHandleDirectPointerInput()
    {
        if (_root == null || !Input.GetMouseButtonDown(0) || _lastPointerFrame == Time.frameCount)
        {
            return;
        }

        _lastPointerFrame = Time.frameCount;
        TryRunChoiceAtPointer(Input.mousePosition, "manual-hit-test");
    }

    private bool TryRunChoiceAtPointer(Vector2 pointer, string route)
    {
        for (int i = _buttons.Count - 1; i >= 0; i--)
        {
            ButtonBinding binding = _buttons[i];
            if (!binding.Button.interactable || !RectTransformUtility.RectangleContainsScreenPoint(binding.Rect, pointer, cam: null))
            {
                continue;
            }

            _plugin.ModLogger.LogInfo($"{Plugin.PluginName} dialogue direct choice clicked. Choice={binding.Label.text}; route=manual-hit-test");
            RunChoice(binding, route);
            return true;
        }

        return false;
    }

    private void ApplyButtonState(ButtonBinding binding, bool selected, bool hover)
    {
        bool interactable = binding.Button.interactable;
        Color normal = GetButtonColor(binding.Kind, selected, hover: false, interactable: true);
        Color hoverColor = GetButtonColor(binding.Kind, selected, hover: true, interactable: true);
        Color disabled = GetButtonColor(binding.Kind, selected, hover: false, interactable: false);
        ColorBlock colors = binding.Button.colors;
        colors.normalColor = normal;
        colors.highlightedColor = hoverColor;
        colors.selectedColor = hoverColor;
        colors.pressedColor = hoverColor;
        colors.disabledColor = disabled;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.06f;
        binding.Button.colors = colors;
        binding.Image.color = interactable ? GetButtonColor(binding.Kind, selected, hover, interactable: true) : disabled;
        binding.Accent.color = GetAccentColor(binding.Kind, selected, hover, interactable);
        binding.Label.color = GetTextColor(selected, hover, interactable);
        if (!string.Equals(binding.Label.text, binding.DisplayLabel, StringComparison.Ordinal))
        {
            binding.Label.text = binding.DisplayLabel;
        }
    }

    private static Color GetButtonColor(ButtonKind kind, bool selected, bool hover, bool interactable)
    {
        if (!interactable)
        {
            return ButtonDisabledColor;
        }

        if (kind == ButtonKind.Danger)
        {
            return hover ? ButtonDangerHoverColor : ButtonDangerColor;
        }

        if (selected)
        {
            return hover ? ButtonSelectedHoverColor : ButtonSelectedColor;
        }

        return hover ? ButtonHoverColor : ButtonColor;
    }

    private static Color GetAccentColor(ButtonKind kind, bool selected, bool hover, bool interactable)
    {
        if (!interactable)
        {
            return DisabledAccentColor;
        }

        if (kind == ButtonKind.Danger)
        {
            return DangerAccentColor;
        }

        if (selected)
        {
            return AccentColor;
        }

        return hover ? HoverAccentColor : TransparentColor;
    }

    private static Color GetTextColor(bool selected, bool hover, bool interactable)
    {
        if (!interactable)
        {
            return DisabledTextColor;
        }

        return selected || hover ? HoverTextColor : TextColor;
    }

    private void EnsureEventSystem()
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

        _eventSystemRoot = new GameObject(
            "AvalonBroodmotherCompanion.DialogueEventSystem",
            typeof(EventSystem),
            typeof(StandaloneInputModule));
        UnityEngine.Object.DontDestroyOnLoad(_eventSystemRoot);
    }

    private void EnsureCursor()
    {
        EnsureGameCursorVisibilityElement();

        if (!_cursorCaptured)
        {
            _previousCursorVisible = Cursor.visible;
            _previousCursorLockState = Cursor.lockState;
            _cursorCaptured = true;
        }

        if (!_taintedInterfaceScopeActive && !_modManagerScopeActive)
        {
            _taintedInterfaceScopeActive = TaintedInterfaceReflection.BeginCustomUiScope(Plugin.PluginGuid, freezeWorld: true);
            if (!_taintedInterfaceScopeActive)
            {
                _modManagerScopeActive = FoAModManagerBridge.SetCustomUiScope(Plugin.PluginGuid, active: true, freezeWorld: true);
            }
        }

        EnsureCursorForMenu();
    }

    private static void EnsureCursorForMenu()
    {
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (!Cursor.visible)
        {
            Cursor.visible = true;
        }

        TaintedInterfaceReflection.EnsureInteractiveCursor();
    }

    private void ReleaseCursor()
    {
        ReleaseGameCursorVisibilityElement();

        if (_taintedInterfaceScopeActive)
        {
            TaintedInterfaceReflection.EndCustomUiScope(Plugin.PluginGuid);
            _taintedInterfaceScopeActive = false;
        }

        if (_modManagerScopeActive)
        {
            FoAModManagerBridge.SetCustomUiScope(Plugin.PluginGuid, active: false, freezeWorld: true);
            _modManagerScopeActive = false;
        }

        if (!_cursorCaptured)
        {
            return;
        }

        Cursor.visible = _previousCursorVisible;
        Cursor.lockState = _previousCursorLockState;
        _cursorCaptured = false;
    }

    private void EnsureGameCursorVisibilityElement()
    {
        if (_forceCursorVisibilityElement != null && !_forceCursorVisibilityElement.HasBeenDiscarded)
        {
            return;
        }

        Hero hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            return;
        }

        _forceCursorVisibilityElement = new GameForceCursorVisibility(shouldBeVisible: true);
        hero.AddElement(_forceCursorVisibilityElement);
    }

    private void ReleaseGameCursorVisibilityElement()
    {
        if (_forceCursorVisibilityElement == null)
        {
            return;
        }

        if (!_forceCursorVisibilityElement.HasBeenDiscarded)
        {
            _forceCursorVisibilityElement.Discard();
        }

        _forceCursorVisibilityElement = null;
    }

    private void DestroyHost()
    {
        if (_root != null)
        {
            UnityEngine.Object.Destroy(_root);
            _root = null;
        }

        if (_eventSystemRoot != null)
        {
            UnityEngine.Object.Destroy(_eventSystemRoot);
            _eventSystemRoot = null;
        }

        _buttons.Clear();
        _textStack = null;
        _portraitRoot = null;
        _portraitBackgroundImage = null;
        _portraitIconImage = null;
        _titleText = null;
        _subtitleText = null;
    }

    private RectTransform AddRect(string name, Transform parent)
    {
        GameObject child = new(name, typeof(RectTransform));
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

    private Text AddText(Transform parent, string name, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment, bool wrap)
    {
        RectTransform rect = AddRect(name, parent);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = GetFont();
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

    private Font GetFont()
    {
        if (_font != null)
        {
            return _font;
        }

        _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return _font;
    }

    private static void SetText(Text? text, string value)
    {
        if (text != null && !string.Equals(text.text, value, StringComparison.Ordinal))
        {
            text.text = value;
        }
    }

    private Texture2D? GetActiveCompanionIconTexture(out string source)
    {
        BroodmotherIconVariant? variant = _plugin.ActiveCompanionIconVariant;
        if (variant.HasValue)
        {
            return BroodmotherEmbeddedIconRuntime.GetIconTexture(variant.Value, out source);
        }

        source = string.Empty;
        return null;
    }

    private string GetDisplayName(Location location)
    {
        return string.IsNullOrWhiteSpace(location.DisplayName)
            ? _plugin.ActiveCompanionDisplayName
            : location.DisplayName.Trim();
    }

    private enum ButtonKind
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

    private sealed class ButtonBinding
    {
        internal ButtonBinding(
            RectTransform rect,
            Button button,
            Image image,
            Image accent,
            Text label,
            string commandId,
            string displayLabel,
            ButtonKind kind,
            bool requiresCompanion,
            BroodmotherDialogueCommand? command)
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
        }

        internal RectTransform Rect { get; }
        internal Button Button { get; }
        internal Image Image { get; }
        internal Image Accent { get; }
        internal Text Label { get; }
        internal string CommandId { get; }
        internal string DisplayLabel { get; }
        internal ButtonKind Kind { get; }
        internal bool RequiresCompanion { get; }
        internal BroodmotherDialogueCommand? Command { get; }
    }
}
