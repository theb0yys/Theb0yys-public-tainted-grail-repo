# Understand How Mods Work

This section explains why a working mod works, so you can reason about problems instead of relying on trial and error.

## Use the technical reference

The [technical reference](../../../knowledge/reference/README.md) provides deeper detail for the concepts used here.

In particular:

- [Game and Runtime Architecture](../../../knowledge/systems/core/game-runtime-architecture.md)
- [Identity](../../../knowledge/reference/identities/identity-guids-names.md)
- [Templates and Registries](../../../knowledge/systems/core/templates-registries.md)
- [Lifecycle and Hooks](../../../knowledge/reference/hooks/lifecycle.md)
- [Native Object Ownership](../../../knowledge/systems/core/native-object-ownership.md)
- [Assets](../../../knowledge/reference/assets/README.md)
- [Saving and Persistence](../../../knowledge/systems/world/saving-persistence.md)

Use these to understand why a working process has its current shape rather than memorizing calls.

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

Mono and IL2CPP are different runtimes. They affect the loader package, target framework, available assemblies, generated interop state, and debugging symptoms.

Always identify the installed runtime before choosing a template or dependency set.

See [Runtime modding](../runtime-modding/README.md).

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
- Mono or IL2CPP runtime;
- why it is required.

Minimize dependencies. Every dependency adds compatibility, maintenance, and licensing risk.

## Identity

Stable identity matters more than display text.

For plug-ins, use a stable unique BepInEx GUID.

For new content, use stable mod-owned identities where the proven integration path requires them. Do not confuse native template GUIDs, custom template GUIDs, Unity asset GUIDs, Addressables addresses, plug-in GUIDs, or display names.

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
- Mono or IL2CPP runtime;
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

Say **"tested on game build X with loader Y"** rather than **"works on all versions."**

## When to move into advanced modding

Move to [Build Robust Game Changes](../advanced/README.md) when you can explain your mod's runtime, dependencies, identities, compatibility assumptions, and failure boundaries without guessing.
