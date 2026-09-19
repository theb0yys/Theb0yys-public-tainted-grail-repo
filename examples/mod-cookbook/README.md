# Tainted Grail Modding Cookbook

This cookbook is organised by the type of mod you want to build. It contains working patterns taken from real Tainted Grail mod implementations.

## Gameplay

[Open Gameplay →](gameplay/README.md)

Covers:

- magic projectile tuning;
- theft/interaction guards;
- existing-item grants;
- vendor pricing;
- native bonfire services;
- mount velocity;
- one-session companions;
- exact-target NPC tuning;
- fixed native encounters;
- character damage/death observation.

## Graphics

[Open Graphics →](graphics/README.md)

Covers active HDRP/FoA fog ownership and runtime visual-state control.

## Visual Effects

[Open Visual Effects →](visual-effects/README.md)

Covers character damage/death VFX sidecars and bounded cleanup.

## Audio

[Open Audio →](audio/README.md)

Covers native hero-footstep replacement through FoA's existing FMOD parameters.

## UI & HUD

[Open UI & HUD →](ui-hud/README.md)

Covers action receipts and runtime overlay ownership.

## Systems

[Open Systems →](systems/README.md)

Covers native-owner-first patching and fail-closed cross-mod APIs.

## Build lane

The game-specific examples here target **Mono / BepInEx 5** unless a page says otherwise.

Build against your own local game references. Do not redistribute game DLLs, Unity DLLs, BepInEx binaries or extracted game assets.
