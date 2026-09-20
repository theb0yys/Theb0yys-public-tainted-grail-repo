using System;
using System.Collections.Generic;
using System.Reflection;
using Awaken.TG.Main.Crafting.Fireplace;
using Awaken.TG.Main.UI.Components;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace TGCommunity.Example.BonfireServicesSubmenu;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.bonfire-services-submenu";
    public const string PluginName = "TG Example - Bonfire Services Submenu";
    public const string PluginVersion = "0.1.0";

    private static Plugin? Instance;
    private readonly List<GameObject> _ownedButtons = new();
    private GameObject? _submenuRoot;
    private GameObject? _servicesEntry;
    private GameObject? _vanillaContent;
    private FireplaceUI? _fireplace;
    private Harmony? _harmony;

    private void Awake()
    {
        Instance = this;
        _harmony = new Harmony(PluginGuid);

        MethodInfo? target = AccessTools.Method(typeof(VFireplaceUI), "OnInitialize");
        if (target != null)
            _harmony.Patch(target, postfix: new HarmonyMethod(typeof(Plugin), nameof(OnInitializePostfix)));

        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        ClearOwnedUi();
        _harmony?.UnpatchSelf();
        Instance = null;
    }

    private static void OnInitializePostfix(VFireplaceUI __instance)
    {
        Instance?.Attach(__instance);
    }

    private void Attach(VFireplaceUI view)
    {
        ClearOwnedUi();

        if (view.GenericTarget is not FireplaceUI fireplace)
            return;

        FieldInfo? contentField = AccessTools.Field(typeof(VFireplaceUI), "buttonContent");
        FieldInfo? levelUpField = AccessTools.Field(typeof(VFireplaceUI), "levelUp");
        object? levelUpEntry = levelUpField?.GetValue(view);
        if (contentField?.GetValue(view) is not GameObject vanillaContent || levelUpEntry == null)
            return;

        FieldInfo? buttonConfigField = AccessTools.Field(levelUpEntry.GetType(), "buttonConfig");
        if (buttonConfigField?.GetValue(levelUpEntry) is not ButtonConfig template || template.button == null)
            return;

        _fireplace = fireplace;
        _vanillaContent = vanillaContent;

        Transform parent = vanillaContent.transform.parent ?? vanillaContent.transform;
        _submenuRoot = new GameObject("TGCommunity_BonfireServicesSubmenu", typeof(RectTransform));
        RectTransform rootRect = (RectTransform)_submenuRoot.transform;
        rootRect.SetParent(parent, false);

        if (vanillaContent.transform is RectTransform sourceRect)
        {
            rootRect.anchorMin = sourceRect.anchorMin;
            rootRect.anchorMax = sourceRect.anchorMax;
            rootRect.pivot = sourceRect.pivot;
            rootRect.anchoredPosition = sourceRect.anchoredPosition;
            rootRect.sizeDelta = sourceRect.sizeDelta;
            rootRect.localScale = sourceRect.localScale;
        }

        _submenuRoot.SetActive(false);

        AddSubmenuButton(template, "Stash", () => RunService(fireplace.OpenHeroStorage));
        AddSubmenuButton(template, "Cooking", () => RunService(fireplace.CookAction));
        AddSubmenuButton(template, "Alchemy", () => RunService(fireplace.AlchemyAction));
        AddSubmenuButton(template, "Back", CloseSubmenu);

        _servicesEntry = CloneButton(template, vanillaContent.transform, "Services", OpenSubmenu);
        Logger.LogInfo("Native-looking Services entry and submenu attached.");
    }

    private void AddSubmenuButton(ButtonConfig template, string label, Action action)
    {
        if (_submenuRoot == null) return;
        CloneButton(template, _submenuRoot.transform, label, action);
    }

    private GameObject CloneButton(ButtonConfig template, Transform parent, string label, Action action)
    {
        GameObject clone = Instantiate(template.gameObject, parent, false);
        clone.name = "TGCommunity_Bonfire_" + label.Replace(" ", string.Empty);
        clone.SetActive(true);
        _ownedButtons.Add(clone);

        ButtonConfig config = clone.GetComponent<ButtonConfig>()
            ?? throw new InvalidOperationException("Cloned native bonfire button has no ButtonConfig.");

        config.button.ClearAllOnClickEvents();
        config.InitializeButton(action, label);
        return clone;
    }

    private void OpenSubmenu()
    {
        if (_submenuRoot == null || _vanillaContent == null) return;
        _vanillaContent.SetActive(false);
        _submenuRoot.SetActive(true);
    }

    private void CloseSubmenu()
    {
        if (_submenuRoot != null) _submenuRoot.SetActive(false);
        if (_vanillaContent != null) _vanillaContent.SetActive(true);
    }

    private void RunService(Action service)
    {
        CloseSubmenu();
        try
        {
            service();
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Native bonfire service failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void ClearOwnedUi()
    {
        CloseSubmenu();

        foreach (GameObject button in _ownedButtons)
            if (button != null) Destroy(button);
        _ownedButtons.Clear();

        if (_submenuRoot != null) Destroy(_submenuRoot);
        _submenuRoot = null;
        _servicesEntry = null;
        _vanillaContent = null;
        _fireplace = null;
    }
}
