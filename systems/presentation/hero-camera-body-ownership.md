---
document_type: system
scope: hero camera, perspective and body ownership
runtime: mono
evidence:
  static: DECOMPILED
  runtime: EXISTING_DUMP_CORROBORATION
last_verified: 2026-09-20
---

# Hero Camera, Perspective and Body Ownership

The inspected camera/body research identifies `VHeroController` as a major owner of live hero view/body state.

## Perspective is not a visibility toggle

`VHeroController.ChangeHeroPerspective(bool)` performs a broad native transition:

- writes perspective settings;
- sets `Hero.TppActive`;
- calls `HeroCamera.ChangeHeroPerspective`;
- reloads the body/equipment path;
- triggers perspective-changed events;
- updates FOV.

Calling it just to show a mesh can therefore disturb much more than presentation.

## Separate body hierarchies

The controller has separate FPP and TPP body prefab references and parents.

Static/runtime-dump evidence supports:

- FPP arms/body hierarchy under `fppParent`;
- TPP full-body hierarchy under `tppParent`;
- different camera/body setup paths;
- `HeroBodyData` transforms for hands, head, torso, fire point, hips, limbs and TPP pivot;
- Kandra-rendered native body surfaces.

## Camera ownership

`HeroCamera.ChangeHeroPerspective` selects the corresponding virtual camera and updates global game-camera/Cinemachine ownership.

`GameCamera`, `CameraStateStack` and `HeroCamera` are global/game-owned surfaces, not casual per-mod camera slots.
