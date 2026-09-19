# Find the Game and Identify the Runtime

## What you're doing

You are finding the actual Tainted Grail installation you will mod and determining whether it uses **IL2CPP** or **Mono**.

Here, **runtime** means the form of Unity/.NET execution the installed game uses. It matters because Mono and IL2CPP need different BepInEx files, project references, and plug-in base classes.

- **IL2CPP** is Unity's native-code runtime path. Modding it uses BepInEx 6 plus generated managed interop assemblies.
- **Mono** is Unity's managed runtime path. This repository uses the supported BepInEx 5 setup for it.

Do this before choosing a C# template.

## What you need

- Tainted Grail installed through Steam;
- access to the game's local files;
- the starter repository;
- BepInEx installed if you are already setting up runtime plug-ins.

## What you'll learn

You will learn how to:

- find the real game root;
- recognize the IL2CPP layout;
- recognize the Mono layout;
- choose the matching starter template;
- verify that BepInEx itself starts before debugging your own mod.

## Steps

### 1. Find the Steam installation

In Steam:

1. Open **Library**.
2. Right-click **Tainted Grail: The Fall of Avalon**.
3. Choose **Manage -> Browse local files**.
4. File Explorer opens at the game root.

Call that directory `GameRoot`.

Example only:

~~~text
C:\Path\To\Tainted Grail FoA
~~~

Do not copy that example blindly. Use your real path.

### 2. Check for IL2CPP

In the game root, look for:

~~~text
GameAssembly.dll
~~~

Then look for:

~~~text
Fall of Avalon_Data\il2cpp_data\Metadata\global-metadata.dat
~~~

If those exist together, that is a strong sign you are looking at the IL2CPP layout.

The validated local snapshot recorded on **2026-08-30** was IL2CPP.

For that lane, use:

~~~text
templates\il2cpp-basic
~~~

### 3. Check for Mono

A Mono install normally has managed game assemblies under:

~~~text
Fall of Avalon_Data\Managed\
~~~

and an `Assembly-CSharp.dll`-style game assembly.

For a Mono setup, use:

~~~text
templates\mono-basic
~~~

Mono and IL2CPP are separate supported Tainted Grail modding lanes. Use the tooling that matches the installation you are actually targeting.

### 4. Confirm BepInEx before building your mod

In `GameRoot`, you should already have:

~~~text
BepInEx\
~~~

For the IL2CPP lane, `BepInEx\core` should include the IL2CPP BepInEx assemblies used by the starter project.

Launch the game once.

Then check for the BepInEx log, normally:

~~~text
BepInEx\LogOutput.log
~~~

If BepInEx itself does not start, stop here. Your own mod is not the first problem yet.

### 5. Keep the two runtime setups separate

Do not copy Mono loader files on top of IL2CPP files or vice versa.

If you are uncertain, read [the Runtime Guide](../runtime-modding/runtime-guide.md) before changing the game installation.

## What success looks like

**Progress: Start → _Loader working_ → First plug-in → First game change → First complete mod**

You have reached **Loader working** when BepInEx starts for the correct runtime and writes a log before your own plug-in is part of the problem.

You can answer all four questions without guessing:

- Where is my real `GameRoot`?
- Is this installation IL2CPP or Mono?
- Which starter template matches it?
- Does the matching BepInEx lane start and produce a log?

## Common problems

**You chose a template from an old guide instead of inspecting your installation:** go back to the runtime markers above.

**BepInEx produces no log:** fix the loader/runtime setup before building your own plug-in.

**Files from both loader lanes have been mixed together:** restore a known-good game/loader state and install only the lane that matches the runtime.

**You have multiple Steam libraries:** use **Browse local files** and work from the directory Steam actually opens.

## Where to go next

If your installation is IL2CPP, continue to **[Build Your First IL2CPP Plug-in](first-il2cpp-plugin.md)**.

If your installation is Mono, continue to **[Build Your First Mono Plug-in](first-mono-plugin.md)**.
