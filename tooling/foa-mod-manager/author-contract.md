---
document_type: framework
scope: public mod-author integration with FoA Mod Manager
runtime: mono
evidence:
  source: PROJECT_API
  runtime: MULTI_CONSUMER_LINEAGE
last_verified: 2026-09-20
---

# FoA Mod Manager Author Contract

FoA Mod Manager already provides reusable project-level integration for BepInEx mods.

## Settings discovery

Basic settings need no manager API call.

Use normal BepInEx:

- stable `BepInPlugin` GUID/name/version;
- `Config.Bind<T>`;
- acceptable ranges/lists;
- `KeyCode` for keyboard bindings.

The manager can discover loaded plugins/config and render suitable controls.

Optional metadata can improve display names, category/order, labels and hidden/internal settings without changing the saved value.

## Shared custom UI scope

Interactive custom screens can request:

`FoAModManagerApi.SetCustomUiScope(ownerId, active, freezeWorld)`

The shared owner can handle:

- gameplay-input freeze;
- cursor unlock/visibility;
- controller cursor;
- optional `Time.timeScale=0`;
- restoration when the last owner closes.

Always release the scope on close/disable/destroy.

## Controller actions

Mods can register named controller actions rather than injecting keyboard events.

Keep callbacks small and lifecycle-safe.

## Status providers

Mods can register cheap **read-only** status snapshots. Provider failure should degrade only that status row, not the manager or feature.

## Packaging

If referencing the manager DLL, keep the reference non-private and document whether the manager is optional or required. Never package a second copy.
