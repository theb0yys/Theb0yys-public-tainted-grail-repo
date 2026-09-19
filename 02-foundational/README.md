# 02 — Foundational

This level explains why a working mod works.

## Runtime layers

Think of a runtime mod as a stack:

game
↓
Unity/runtime
↓
BepInEx loader
↓
Harmony / IL2CPP interop support
↓
your plug-in

A failure lower in the stack prevents higher layers from working. Debug from the bottom upward.

Mono and IL2CPP are separate lanes. They affect the loader package, target framework, available assemblies, generated interop state, and debugging symptoms.

Always identify the installed runtime before choosing a template or dependency set.

See docs/RUNTIME_GUIDE.md.

## References and dependencies

Your project may need local compile-time references to:

- BepInEx;
- Harmony;
- Unity assemblies;
- generated IL2CPP interop assemblies;
- game assemblies.

Keep those references local. Do not commit them merely so someone else can compile without their own installation.

For every important dependency, know:

- name;
- version or commit;
- runtime lane;
- why it is required.

Minimize dependencies. Every dependency creates another compatibility, maintenance, and licensing surface.

## Identity

Stable identity matters more than display text.

For plug-ins, use a stable unique BepInEx GUID.

For authored content, use mod-owned identifiers where the toolchain supports them.

Do not use a visible/display name as the only durable identity for an object.

## Separate logical data from visuals

An item may involve several distinct things:

- item/template identity;
- gameplay/economy data;
- icon;
- equipment representation;
- world-drop representation;
- localization.

Treating them as separate responsibilities makes debugging and reuse easier.

## Compatibility and game updates

A working mod is always working against a particular environment.

Record:

- game version/build;
- runtime lane;
- loader version;
- important dependency versions;
- mod version.

After a game update, re-establish the stack from the bottom:

1. loader starts;
2. plug-in loads;
3. IL2CPP interop is healthy when applicable;
4. target types/methods still resolve;
5. content/templates still resolve;
6. one small behaviour still works.

Prefer the claim "tested on game build X with loader Y" over "works on all versions."

## When to move to Advanced

Move to 03-advanced when you can explain your mod's runtime lane, dependencies, identities, compatibility assumptions, and failure boundaries without guessing.
