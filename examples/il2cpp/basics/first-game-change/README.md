# First IL2CPP Game Change

This example is the bridge between **"my plug-in loads"** and **"my plug-in changes the running game."**

It temporarily changes two Unity runtime values used by Tainted Grail:

- disables VSync for the current process;
- sets `Application.targetFrameRate` to **30**.

It captures the previous values first and restores them if BepInEx unloads the plug-in.

It does **not** patch a Tainted Grail method, edit a save, change game files, or redistribute game assemblies.

## Build

From this repository root:

~~~powershell
$GameRoot = "C:\Path\To\Tainted Grail FoA"

dotnet build `
  .\examples\il2cpp\basics\first-game-change\FirstGameChange.csproj `
  -c Release `
  -p:GameRoot="$GameRoot"
~~~

The output is:

~~~text
examples\il2cpp\basics\first-game-change\bin\Release\net6.0\TGCommunity.Il2CppFirstGameChange.dll
~~~

## Deploy

Create a dedicated plug-in directory and copy only the built DLL:

~~~powershell
$PluginDir = Join-Path $GameRoot "BepInEx\plugins\TGCommunity.Il2CppFirstGameChange"
New-Item -ItemType Directory -Force $PluginDir | Out-Null

Copy-Item `
  .\examples\il2cpp\basics\first-game-change\bin\Release\net6.0\TGCommunity.Il2CppFirstGameChange.dll `
  $PluginDir `
  -Force
~~~

Launch Tainted Grail normally.

## What to look for

In `BepInEx\LogOutput.log`, find a line similar to:

~~~text
First IL2CPP Game Change applied. targetFrameRate: -1 -> 30; vSyncCount: 1 -> 0.
~~~

The exact previous values depend on your installation and settings.

If you use an FPS counter, the running game should now be limited to roughly 30 FPS while this plug-in is active. `Application.targetFrameRate` is a Unity scheduling target, so observed frame rate can still vary with workload and platform timing.

## Why this example exists

A first real change does not need to begin with reverse engineering or Harmony.

This example teaches the safer sequence:

~~~text
read current state
-> remember it
-> make one runtime change
-> log exactly what changed
-> restore owned state when possible
~~~

Once that loop makes sense, move on to generated IL2CPP interop assemblies and game-specific Harmony targets.

## Current checking status

This exact public teaching example was written from the current BepInEx 6 / Unity IL2CPP project shape, but it has **not been run end-to-end as part of this public documentation pass**.

Do not describe it as runtime-tested until this exact project is built, deployed, and observed on the target installation.

See [Testing and Evidence Status](../../../../sources/evidence-standard.md) for the repository's formal status terminology.
