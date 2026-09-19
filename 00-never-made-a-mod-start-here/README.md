# 00 - Never Made a Mod? Start Here

This is the do-this-first section. You do not need to understand the whole repository before making your first test.

## Pick one path

### A. I want to change game behaviour with code

Follow these in order:

1. [01 - Windows setup](01_WINDOWS_SETUP.md)
2. [02 - Find the game and identify the runtime](02_FIND_GAME_AND_RUNTIME.md)
3. Current/modern IL2CPP lane: [03 - Build your first IL2CPP plug-in](03_FIRST_IL2CPP_PLUGIN.md)
4. Legacy Mono lane only: [04 - Build your first Mono plug-in](04_FIRST_MONO_PLUGIN.md)
5. [06 - What success looks like](06_SUCCESS_CHECKLIST.md)

Do not start with a gameplay patch. First prove that your own plug-in can load and write one line to the BepInEx log.

### B. I want to make items, weapons, armour, or creatures

Follow:

1. [01 - Windows setup](01_WINDOWS_SETUP.md)
2. [05 - First content-authoring session](05_FIRST_CONTENT_AUTHORING.md)
3. [06 - What success looks like](06_SUCCESS_CHECKLIST.md)
4. Then move to [01-basic](../01-basic/README.md).

The content route uses the public Merlin Workshop authoring contracts documented under docs/pipelines/. Those documents say exactly whether a step is source-confirmed, editor-validated, or runtime-tested.

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

**Mono** - older Unity managed runtime lane used by legacy FoA setups.

**IL2CPP** - the runtime lane in the validated 2026-08-30 FoA snapshot.

**Interop assemblies** - managed type representations used by the IL2CPP toolchain.

**Prefab** - reusable Unity object/configuration asset.

**Template** - reusable game/toolkit data definition.

**Addressable** - Unity asset referenced through an address/group system.

**Static-confirmed** - the code/tool contract exists in inspected source.

**Runtime-passed** - it was actually executed and observed working in the stated environment.

Those two evidence states are not interchangeable.

## When you are ready for 01-basic

Move on when you can repeat your first test without guessing:

- where your project lives;
- where the game lives;
- which runtime lane you are using;
- how to build;
- where your DLL/content output goes;
- where to look when it fails.
