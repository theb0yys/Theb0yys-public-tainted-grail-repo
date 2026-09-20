---
document_type: system
scope: hero camera, perspective and body ownership
runtime: mono
evidence:
  static: DECOMPILED
  runtime: EXISTING_DUMP_CORROBORATION
last_verified: 2026-09-20
---

# Hero Camera and Body

Use this page when you want to change **first-person/third-person presentation, Hero body visibility, camera perspective, or body attachment points**.

The important warning is:

> Changing Hero perspective is not a simple mesh-visibility toggle.

## VHeroController owns more than visibility

The inspected research identifies `VHeroController` as a major owner of live Hero body/view state.

`VHeroController.ChangeHeroPerspective(bool)` performs a broad transition:

- writes perspective settings;
- updates `Hero.TppActive`;
- calls `HeroCamera.ChangeHeroPerspective`;
- reloads body/equipment presentation;
- triggers perspective-changed events;
- updates FOV.

Do not call it just to reveal a mesh unless you actually want the full perspective transition.

## FPP and TPP have separate body paths

Static/runtime-dump evidence supports separate:

- FPP body/arms under `fppParent`;
- TPP full body under `tppParent`;
- camera/body setup paths;
- equipment presentation paths.

`HeroBodyData` exposes transforms for areas such as:

- hands;
- head;
- torso;
- fire point;
- hips;
- limbs;
- TPP pivot.

Native body surfaces are Kandra-rendered.

## Camera ownership

`HeroCamera.ChangeHeroPerspective` selects the appropriate virtual camera and updates the game/Cinemachine camera state.

Related global owners include:

- `GameCamera`
- `CameraStateStack`
- `HeroCamera`

These are shared game camera systems, not independent per-mod slots.

## Practical rule

If your feature only needs:

- a body attachment;
- one mesh shown/hidden;
- a camera offset;
- a temporary overlay;

do not trigger the entire perspective transition unless that is the intended behavior.

Find the narrow owner for the exact change.

## Common mistakes

- calling `ChangeHeroPerspective` only to make the body visible;
- assuming FPP and TPP use the same hierarchy;
- attaching to a transform without checking which body is active;
- bypassing Kandra/body presentation ownership;
- treating a Cinemachine camera as private mod state.

## How to verify

Check:

1. starting perspective;
2. active FPP/TPP body;
3. exact attachment transform;
4. camera owner/virtual camera;
5. equipment/body reload behavior;
6. perspective-change events if invoked;
7. FOV;
8. return to previous state;
9. scene/load transitions;
10. no duplicate body/camera ownership.

## Evidence

This page is based on inspected Mono camera/body code and existing runtime-dump corroboration.
