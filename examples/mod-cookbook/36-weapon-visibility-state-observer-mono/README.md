# 36 — Weapon Visibility/State Observer

**Category:** weapon presentation / state observation  
**Source-path evidence:** LOAD_EVIDENCED read surface  
**Validation:** NEEDS_VALIDATION  
**This rewritten public example:** NOT_RUN

This example watches a small group of native hero weapon/presentation reads:

~~~text
Hero.WeaponsVisible
Hero.IsWeaponEquipped
Hero.PullingRangedWeapon
Hero.MainHandItem
Hero.OffHandItem
Hero.TppActive
~~~

It emits a row only when the combined snapshot changes.

## What these fields mean here

They are **state observations**, not permission to control native presentation.

Maintainer research shows that native weapon presentation is owned by deeper systems including `CharacterHandBase`, weapon views, animation overrides, animator layers and renderer/GameObject visibility. `Hero.WeaponsVisible` participates in the draw/sheathe lifecycle, but this example does not patch that lifecycle.

## Explicitly not done

The example does not:

- set `WeaponsVisible`;
- force equip/unequip;
- enable or disable weapon GameObjects;
- change renderer or Kandra state;
- change animator layers or animation state;
- execute attacks or weapon actions;
- write saves.

## Build

~~~powershell
dotnet build .\WeaponVisibilityStateObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The public rewrite is **NOT_RUN** and **NEEDS_VALIDATION**.
