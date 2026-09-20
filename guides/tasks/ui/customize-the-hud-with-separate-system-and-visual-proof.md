# Customize the HUD Through VHeroHUD

Use VHeroHUD as the lifecycle/visibility owner, then alter only the child CanvasGroups or mod-owned presentation you actually need.

Working lineage: [Native HUD Ownership vs Custom Visual Proof](../../../research/case-studies/ui/hud-owner-and-visual-proof.md).


## Runnable source

Start with the [HUD visibility and theme example](../../../examples/mono/ui/hud-visibility-and-theme/README.md). Build it unchanged first, then change one setting or mechanism at a time.

## Force or yield native hero-bar visibility

The central native decision is the private property:

~~~text
Awaken.TG.Main.Heroes.VHeroHUD.ShowBars
~~~

Resolve its getter with:

~~~csharp
AccessTools.PropertyGetter(typeof(VHeroHUD), "ShowBars")
~~~

and patch it with a postfix.

The working Always Show HUD implementation does:

~~~csharp
if (Plugin.Enabled &&
    Plugin.ForceVanillaHeroHud &&
    !Plugin.ShouldLetGameHideHud())
{
    __result = true;
}
~~~

That keeps the game's own HUD objects and only changes the final visibility decision.

FoA's normal ShowBars result already accounts for combat/recent-HUD timers/ForceShow/weapons-visible behavior.

## Per-element visibility

For independent health/stamina/mana/quickslot controls, patch:

~~~text
VHeroHUD.UpdateCanvasGroups()
~~~

with a postfix.

After the game updates its own groups:

~~~csharp
VCHeroHUDBar[] bars =
    hud.GetComponentsInChildren<VCHeroHUDBar>(true);
~~~

Classify known bar components such as:

- VCHeroHealthBar;
- VCHeroStaminaBar;
- VCHeroManaBar.

Add/reuse a CanvasGroup on the exact child root and change:

- alpha;
- interactable;
- blocksRaycasts.

Do not deactivate the native objects just to hide them.

## Quickslot

The selected quickslot owner is VCSelectedQuickSlot.

The working implementation patches:

~~~text
VCSelectedQuickSlot.UpdateIcon()
~~~

with a postfix so its CanvasGroup policy is reapplied when the native icon refreshes.

VHeroHUD also contains a private selectedQuickSlot reference; if that member is unavailable, child lookup can be used as a fallback.

## Backdrop and layout

VHeroHUD owns a heroBarsCanvasGroup. If you temporarily hide/replace the native backdrop:

1. capture original alpha/interactable/blocksRaycasts;
2. apply your presentation;
3. restore those original values when the custom layout is disabled or the HUD owner changes.

For moved native status-effect UI, capture the original RectTransform position before changing it and restore it on teardown.

## Custom replacement visuals

If you draw your own vitals:

- continue reading native hero stats;
- let VHeroHUD remain the native lifecycle owner;
- keep generated textures/assets mod-owned;
- scale placement from Screen width/height;
- do not write health/stamina/mana values from the UI.

## Owner changes

Cache the current VHeroHUD instance.

When a new HUD instance appears:

~~~text
restore old moved/hidden native state
→ clear old cached targets
→ bind new VHeroHUD
→ apply current presentation
~~~

That avoids leaking layout changes across scene/UI rebuilds.
