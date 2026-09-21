# Decision 0001: Standalone Scope and Gates

Date: 2026-08-01

Status: Accepted for documentation scaffold only; runtime implementation blocked.

## Context

The user clarified that Dragon Knight is a completely separate standalone mod, not a Wyrd Hunt, Living Avalon, or Avalon Awakened feature request. The requested product scope includes a roaming boss, companion, weapon sets, and armor sets.

The user then clarified that the proven methods from their companion mod, asset mod, and AI system should be used across Dragon Knight, while Dragon Knight remains completely standalone. AI specifically needs a Dragon Knight AI package lane.

The local source folder is:

`<local-path>`

Read-only inspection found a Unity package plus FBX, Maya, Substance Painter, texture, prefab, material, shadergraph, weapon, and controller assets. No licence/readme file was found in the quick archive metadata pass.

## Decision

Dragon Knight receives its own standalone mod folder and gate records under `mods/dragon-knight`.

Proven methods to adapt:

- `mods/avalon-companions` for companion lifecycle, command, not-saved actor, native assist, and validation methods.
- `mods/avalon-awakened` for external asset inventory, licence, isolated authoring, cooked transport, hash, deployment, and missing-content methods.
- `mods/avalon-ai-runtime` for AI package/host, blackboard, planning, action-gateway, ownership lease, and fail-closed validation methods.

The mod is not authorized to:

- reuse Wyrd Hunt encounter execution;
- reuse Living Avalon route-patrol slots;
- add itself to Avalon Awakened creature APIs;
- add compile-time references or runtime dependencies on Avalon Companions, Avalon Awakened, or Avalon AI Runtime;
- register Dragon Knight with any existing companion, asset, or AI host;
- put Dragon Knight AI directly into boss or companion feature code instead of a Dragon Knight AI package lane;
- copy source commercial assets into the repo;
- build or deploy runtime asset bundles;
- create actors, companions, weapons, armor, templates, item grants, or population behavior.

Those actions require later Dragon Knight-specific decisions and validation.

## First Safe Slice

The first safe slice is DK0: source, licence, and asset inventory.

DK0 may:

- hash and catalogue source archives;
- enumerate Unity package pathnames;
- identify candidate prefabs, meshes, animation controllers, textures, and materials;
- record licence/entitlement evidence if supplied;
- propose the first implementation lane.

DK0 must not:

- import assets into a Unity project;
- mutate source files;
- build Addressables or AssetBundles;
- write runtime plugin code;
- deploy files to the game;
- launch FoA;
- access saves;
- spawn actors;
- register items or templates.

## Required Future Decisions

- DK1 visual transport proof.
- DK2 boss actor/template/combat proof profile.
- DK3 companion actor/ally/command boundary.
- DK4 weapon item/equipment lane.
- DK5 armor wearable or visual-only lane.
- DK6 Dragon Knight AI package lane.
- DK7 roaming/population lane.
- DK8 release and packaging gate.

## Validation

This decision is Level 0 documentation only. No runtime validation is claimed.
