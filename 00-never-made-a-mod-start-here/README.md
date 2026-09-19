# Make Your First Tainted Grail Mod

This is the do-this-first section. You do not need to understand the whole repository before making your first test.

## Pick one path

### A. I want to change game behaviour with code

Follow these in order:

1. [Set up Windows for modding](01_WINDOWS_SETUP.md)
2. [Find the game and identify its runtime](02_FIND_GAME_AND_RUNTIME.md)
3. Current/modern IL2CPP: [Build your first IL2CPP plug-in](03_FIRST_IL2CPP_PLUGIN.md)
4. Mono: [Build your first Mono plug-in](04_FIRST_MONO_PLUGIN.md)
5. [Check that your first mod really worked](06_SUCCESS_CHECKLIST.md)

Do not start with a gameplay patch. First prove that your own plug-in can load and write one line to the BepInEx log.

### B. I want to make items, weapons, armour, or creatures

Follow:

1. [Set up Windows for modding](01_WINDOWS_SETUP.md)
2. [Make your first piece of content](05_FIRST_CONTENT_AUTHORING.md)
3. [Check that your first content session really worked](06_SUCCESS_CHECKLIST.md)
4. Then continue to [the everyday modding loop](../01-basic/README.md).

The content route uses the public Merlin Workshop authoring guides under `docs/pipelines/`. Each guide tells you plainly what was confirmed from the toolkit source and what still needs to be tried in the editor or game.

## One rule for beginners

**Change one thing at a time.**

If you change the loader, code, game references, assets, animation, packaging, and gameplay logic in the same test, a failure tells you almost nothing.

## Things this repo will not ask you to upload

Do not upload or commit:

- the whole game;
- game DLLs;
- Unity DLLs;
- BepInEx binaries;
- generated IL2CPP interop assemblies;
- game assets;
- saves;
- credentials;
- private paths/log dumps you have not redacted.

Your local game installation supplies local references. Your repository should contain your own source.

## Beginner words

**BepInEx** - loads runtime plug-ins.

**Plug-in** - your compiled mod DLL.

**Harmony / HarmonyX** - lets a managed plug-in intercept or alter methods.

**Mono** - a supported Tainted Grail Unity managed runtime lane using the BepInEx 5 toolchain.

**IL2CPP** - the runtime lane in the validated 2026-08-30 FoA snapshot.

**Interop assemblies** - managed type representations used by the IL2CPP toolchain.

**Prefab** - reusable Unity object/configuration asset.

**Template** - reusable game/toolkit data definition.

**Addressable** - Unity asset referenced through an address/group system.

Some technical/reference pages use formal testing-status labels. You do **not** need to memorize them while learning. If you want the exact definitions, see [Testing and Evidence Status](../docs/EVIDENCE.md).

## When you are ready to keep building

Continue to [the everyday modding loop](../01-basic/README.md) when you can repeat your first test without guessing:

- where your project lives;
- where the game lives;
- which runtime lane you are using;
- how to build;
- where your DLL/content output goes;
- where to look when it fails.
