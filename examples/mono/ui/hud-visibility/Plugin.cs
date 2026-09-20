using System;
using System.Reflection;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.HUD;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGCommunity.Example.HudVisibility;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.hud-visibility";
    public const string PluginName = "TG Example - HUD Visibility";
    public const string PluginVersion = "0.1.0";

    private static ConfigEntry<bool>? _forceHud;
    private static ConfigEntry<bool>? _showHealth;
    private static ConfigEntry<bool>? _showStamina;
    private static ConfigEntry<bool>? _showMana;
    private static ConfigEntry<bool>? _showQuickslot;
    private static ConfigEntry<float>? _opacity;

    private static VHeroHUD? _lastHud;
    private Harmony? _harmony;

    private void Awake()
    {
        _forceHud = Config.Bind("Visibility", "ForceHeroHud", true, "Force the native VHeroHUD.ShowBars result true.");
        _showHealth = Config.Bind("Elements", "ShowHealth", true, "Show native health bar.");
        _showStamina = Config.Bind("Elements", "ShowStamina", true, "Show native stamina bar.");
        _showMana = Config.Bind("Elements", "ShowMana", true, "Show native mana bar.");
        _showQuickslot = Config.Bind("Elements", "ShowQuickslot", true, "Show selected quickslot.");
        _opacity = Config.Bind("Elements", "Opacity", 1f, "Visible alpha, clamped to 0-1.");

        _harmony = new Harmony(PluginGuid);
        PatchShowBars(_harmony);
        PatchCanvasGroups(_harmony);
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        RestoreLastHud();
    }

    private void PatchShowBars(Harmony harmony)
    {
        MethodInfo? target = AccessTools.PropertyGetter(typeof(VHeroHUD), "ShowBars");
        MethodInfo? postfix = AccessTools.Method(typeof(Plugin), nameof(ShowBarsPostfix));

        if (target != null && postfix != null)
        {
            harmony.Patch(target, postfix: new HarmonyMethod(postfix));
        }
        else
        {
            Logger.LogWarning("VHeroHUD.ShowBars getter not found.");
        }
    }

    private void PatchCanvasGroups(Harmony harmony)
    {
        MethodInfo? target = AccessTools.Method(typeof(VHeroHUD), "UpdateCanvasGroups");
        MethodInfo? postfix = AccessTools.Method(typeof(Plugin), nameof(CanvasGroupsPostfix));

        if (target != null && postfix != null)
        {
            harmony.Patch(target, postfix: new HarmonyMethod(postfix));
        }

        MethodInfo? quickslotTarget = AccessTools.Method(typeof(VCSelectedQuickSlot), "UpdateIcon");
        MethodInfo? quickslotPostfix = AccessTools.Method(typeof(Plugin), nameof(QuickslotPostfix));

        if (quickslotTarget != null && quickslotPostfix != null)
        {
            harmony.Patch(quickslotTarget, postfix: new HarmonyMethod(quickslotPostfix));
        }
    }

    private static void ShowBarsPostfix(ref bool? __result)
    {
        if (_forceHud?.Value == true)
        {
            __result = true;
        }
    }

    private static void CanvasGroupsPostfix(VHeroHUD __instance)
    {
        if (_lastHud != null && !ReferenceEquals(_lastHud, __instance))
        {
            RestoreHud(_lastHud);
        }

        _lastHud = __instance;
        Apply(__instance);
    }

    private static void QuickslotPostfix(VCSelectedQuickSlot __instance)
    {
        SetVisible(__instance.gameObject, _showQuickslot?.Value ?? true, VisibleAlpha);
    }

    private static float VisibleAlpha => Mathf.Clamp01(_opacity?.Value ?? 1f);

    private static void Apply(VHeroHUD hud)
    {
        foreach (VCHeroHUDBar bar in hud.GetComponentsInChildren<VCHeroHUDBar>(true))
        {
            bool? visible = bar switch
            {
                VCHeroHealthBar => _showHealth?.Value ?? true,
                VCHeroStaminaBar => _showStamina?.Value ?? true,
                VCHeroManaBar => _showMana?.Value ?? true,
                _ => null
            };

            if (visible.HasValue)
            {
                SetVisible(bar.gameObject, visible.Value, VisibleAlpha);
            }
        }

        VCSelectedQuickSlot? quickslot = hud.GetComponentInChildren<VCSelectedQuickSlot>(true);
        if (quickslot != null)
        {
            SetVisible(quickslot.gameObject, _showQuickslot?.Value ?? true, VisibleAlpha);
        }
    }

    private static void SetVisible(GameObject root, bool visible, float alpha)
    {
        CanvasGroup group = root.GetComponent<CanvasGroup>() ?? root.AddComponent<CanvasGroup>();
        group.alpha = visible ? alpha : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }

    private static void RestoreLastHud()
    {
        if (_lastHud != null)
        {
            RestoreHud(_lastHud);
            _lastHud = null;
        }
    }

    private static void RestoreHud(VHeroHUD hud)
    {
        foreach (CanvasGroup group in hud.GetComponentsInChildren<CanvasGroup>(true))
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
    }
}
