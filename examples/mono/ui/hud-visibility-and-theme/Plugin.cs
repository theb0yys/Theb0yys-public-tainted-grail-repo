using System;
using System.Collections.Generic;
using System.Reflection;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.HUD;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGCommunity.Example.HudVisibilityAndTheme;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.hud-visibility-and-theme";
    public const string PluginName = "TG Example - HUD Visibility and Theme";
    public const string PluginVersion = "0.1.0";

    private static ConfigEntry<bool>? _forceVisible;
    private static ConfigEntry<bool>? _health;
    private static ConfigEntry<bool>? _stamina;
    private static ConfigEntry<bool>? _mana;
    private static ConfigEntry<float>? _opacity;
    private static readonly Dictionary<CanvasGroup, State> Originals = new();
    private Harmony? _harmony;

    private void Awake()
    {
        _forceVisible = Config.Bind("HUD", "ForceHeroBarsVisible", true, "Force the native VHeroHUD.ShowBars result true.");
        _health = Config.Bind("HUD", "ShowHealth", true, "Show native health bar.");
        _stamina = Config.Bind("HUD", "ShowStamina", true, "Show native stamina bar.");
        _mana = Config.Bind("HUD", "ShowMana", true, "Show native mana bar.");
        _opacity = Config.Bind("HUD", "VisibleOpacity", 1f, "Opacity for visible selected bars.");

        _harmony = new Harmony(PluginGuid);

        MethodInfo? showBars = AccessTools.PropertyGetter(typeof(VHeroHUD), "ShowBars");
        MethodInfo? updateGroups = AccessTools.Method(typeof(VHeroHUD), "UpdateCanvasGroups");

        if (showBars != null)
            _harmony.Patch(showBars, postfix: new HarmonyMethod(typeof(Plugin), nameof(ShowBarsPostfix)));
        if (updateGroups != null)
            _harmony.Patch(updateGroups, postfix: new HarmonyMethod(typeof(Plugin), nameof(CanvasPostfix)));

        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        foreach (KeyValuePair<CanvasGroup, State> pair in Originals)
        {
            if (pair.Key == null) continue;
            pair.Key.alpha = pair.Value.Alpha;
            pair.Key.interactable = pair.Value.Interactable;
            pair.Key.blocksRaycasts = pair.Value.BlocksRaycasts;
        }

        Originals.Clear();
        _harmony?.UnpatchSelf();
    }

    private static void ShowBarsPostfix(ref bool? __result)
    {
        if (_forceVisible?.Value == true)
            __result = true;
    }

    private static void CanvasPostfix(VHeroHUD __instance)
    {
        float opacity = Mathf.Clamp01(_opacity?.Value ?? 1f);
        foreach (VCHeroHUDBar bar in __instance.GetComponentsInChildren<VCHeroHUDBar>(true))
        {
            bool? visible = bar switch
            {
                VCHeroHealthBar => _health?.Value ?? true,
                VCHeroStaminaBar => _stamina?.Value ?? true,
                VCHeroManaBar => _mana?.Value ?? true,
                _ => null
            };

            if (!visible.HasValue) continue;

            CanvasGroup group = bar.GetComponent<CanvasGroup>() ?? bar.gameObject.AddComponent<CanvasGroup>();
            if (!Originals.ContainsKey(group))
                Originals[group] = new State(group.alpha, group.interactable, group.blocksRaycasts);

            group.alpha = visible.Value ? opacity : 0f;
            group.interactable = visible.Value && opacity > 0f;
            group.blocksRaycasts = visible.Value && opacity > 0f;
        }
    }

    private readonly struct State
    {
        internal State(float alpha, bool interactable, bool blocksRaycasts)
        {
            Alpha = alpha;
            Interactable = interactable;
            BlocksRaycasts = blocksRaycasts;
        }

        internal float Alpha { get; }
        internal bool Interactable { get; }
        internal bool BlocksRaycasts { get; }
    }
}
