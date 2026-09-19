# 00 — Never Made a Mod? Start Here

This folder is for someone who has never made a mod before.

You do not need to know Unity, C#, Harmony, IL2CPP, reverse engineering, or build systems before starting. The first goal is much smaller: understand what kind of mod you want to make and complete one safe test from start to finish.

## Pick one path

### Runtime plug-in

Choose this when you want to change game behaviour with code.

Examples:

- react to something the game does;
- change a calculation or rule;
- add a hotkey;
- add diagnostics;
- patch an existing managed method.

Your route is:

1. Read the setup checklist below.
2. Read the beginner glossary.
3. Identify whether your game uses the Mono or IL2CPP modding lane.
4. Use the matching project under templates.
5. Make the plug-in do nothing except log that it loaded.
6. Build it.
7. Put only your plug-in DLL in BepInEx/plugins.
8. Launch the game and find your message in the log.
9. Only then change game behaviour.

### Content authoring

Choose this when you want to create/configure content.

Examples:

- item;
- weapon;
- armour;
- creature or NPC.

Your route is:

1. Read the setup checklist below.
2. Pick one content type.
3. Read the matching document under docs/pipelines.
4. Start with one mod-owned definition and the minimum required fields.
5. Validate one stage at a time.
6. Do not combine a new model, new animation system, new combat behaviour, new loot, and new packaging flow in one first test.

## Setup checklist

Before changing anything:

- Keep the game installation recoverable.
- Know where the game is installed.
- Create a separate source/workspace directory for your mod.
- Do not put the game directory into Git.
- Do not commit game assets, game DLLs, Unity DLLs, BepInEx binaries, generated interop assemblies, saves, credentials, or private paths.
- Change one thing at a time until you understand the workflow.
- Know how to undo your change.

For runtime plug-ins, confirm BepInEx itself starts before debugging your mod.

For content, use only source material you are allowed to redistribute.

## Beginner glossary

**BepInEx** — the plug-in/mod loader used by the runtime plug-in path.

**Plug-in** — a compiled mod loaded by BepInEx.

**Harmony / HarmonyX** — a library used to intercept or alter managed method behaviour.

**Patch** — code that runs before, after, or around existing method behaviour.

**Mono** — one Unity scripting/runtime model. Older modding setups commonly use BepInEx 5.

**IL2CPP** — Unity's ahead-of-time scripting backend. It uses a different BepInEx/runtime toolchain from Mono.

**Interop assemblies** — managed representations used by IL2CPP tooling so plug-in code can work with IL2CPP types.

**Prefab** — a reusable Unity object/configuration asset.

**Template** — a reusable game/toolkit data definition.

**Addressable** — an asset managed through Unity's address/group system.

**Static evidence** — something established by inspecting source/configuration.

**Runtime evidence** — something actually observed working in the editor or game.

Static evidence is useful, but it is not the same as a runtime pass.

## Your first runtime test

Use the matching starter under templates.

Rename the project, namespace, plug-in GUID, name, and version. Build against local game/BepInEx references. Copy only the resulting plug-in DLL into BepInEx/plugins.

Success means:

- BepInEx starts;
- your plug-in is discovered;
- your startup message appears;
- the game remains stable.

Do not add a gameplay patch until this passes.

## Your first content test

Suggested learning order:

1. simple item;
2. weapon or armour derived from the item flow;
3. creature/NPC after the basic content workflow is familiar.

Relevant documents:

- docs/pipelines/ITEMS.md
- docs/pipelines/WEAPONS.md
- docs/pipelines/ARMOUR.md
- docs/pipelines/CREATURES_KANDRA.md

Read the evidence state at the top of each document before calling anything proven.

## When to move to Basic

Move to 01-basic when you can repeat your first build/test cycle without guessing where the loader, log, template, content definition, or output lives.
