# Make Your First Tainted Grail Mod

Start here if you're making your first Tainted Grail mod.

You do not need to understand the whole repository before making your first test.

## Your progress

**Start → Loader working → First plug-in → First game change → First complete mod**

- **Start:** [Set up Windows for modding](windows-setup.md) and [identify your runtime](find-game-and-runtime.md).
- **Loader working:** BepInEx starts and writes its log before your own mod is involved.
- **First plug-in:** [IL2CPP](first-il2cpp-plugin.md) or [Mono](first-mono-plugin.md) loads your own DLL and writes your own log line.
- **First game change:** IL2CPP can use [the first runtime change](../learning-paths/everyday-modding/first-il2cpp-game-change.md); both Mono and IL2CPP paths move toward a [verified real game patch](../learning-paths/everyday-modding/first-real-patch-rules.md).
- **First complete mod:** [finish one small mod end-to-end](../learning-paths/everyday-modding/first-complete-mod.md).

Content authoring follows the same idea—setup, first working content, one observed change, then one small complete mod—even though it does not use the BepInEx loader milestones.

## Pick one path

### A. I want to change game behaviour with code

Follow these in order:

1. [Set up Windows for modding](windows-setup.md)
2. [Find the game and identify its runtime](find-game-and-runtime.md)
3. IL2CPP: [Build your first IL2CPP plug-in](first-il2cpp-plugin.md)
4. Mono: [Build your first Mono plug-in](first-mono-plugin.md)
5. [Check that your first mod really worked](success-checklist.md)

Do not start with a gameplay patch. First prove that your own plug-in can load and write one line to the **BepInEx log**—the log written by the mod loader that starts your plug-in.

### Optional helper scripts

If you have this repository checked out on Windows, the [FoA developer tools](../../platform/developer-tools/README.md) can automate the repetitive parts of the process:

~~~text
detect runtime/install
→ fingerprint local references
→ scaffold Mono or IL2CPP project
→ build
→ install with backup + hash verification
→ tail/filter BepInEx log
~~~

The scripts automate these steps, but they do not prove that the plug-in loaded or that a gameplay hook works. A successful build or verified DLL copy is only part of the test.

### B. I want to add genuinely new content

Start with the smallest proven new-content route:

1. [Set up Windows for modding](windows-setup.md)
2. [Add your first new item](first-content-authoring.md)
3. [Check what you actually proved](success-checklist.md)
4. Use the [technical reference](../../knowledge/reference/README.md) when you need details about identity, templates, hooks, assets, or persistence.

The current custom-item guide has been proven on **Mono/BepInEx 5**. Do not assume IL2CPP, weapons, armour, creatures, spells, or recipes use the same registration process.

## One rule for beginners

**Change one thing at a time.**

If you change the loader, code, game references, assets, animation, packaging, and gameplay logic in the same test, a failure tells you almost nothing.

## Things this repo will not ask you to upload

Do not upload or commit:

- the whole game;
- game DLLs;
- Unity DLLs;
- BepInEx loader binaries;
- generated IL2CPP interop assemblies — managed type files generated for your local IL2CPP game/tooling setup;
- game assets;
- saves;
- credentials;
- private paths/log dumps you have not redacted.

Your local game installation supplies local references. Your repository should contain your own source.

## Terms appear when you need them

You do not need to learn a vocabulary list before starting. Each tutorial explains terms such as BepInEx, Mono, IL2CPP, Harmony, interop assemblies, prefabs, and addressables at the step where they first become useful.

Use the linked reference pages when you need deeper technical details.

## When you are ready to keep building

Continue to [the everyday modding loop](../learning-paths/everyday-modding/README.md) when you can repeat your first test without guessing:

- where your project lives;
- where the game lives;
- whether your game setup is Mono or IL2CPP;
- how to build;
- where your DLL/content output goes;
- where to look when it fails.
