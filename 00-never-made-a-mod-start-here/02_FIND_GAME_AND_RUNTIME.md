# 02 - Find the Game and Identify the Runtime

Do this before choosing a C# template.

## Find the Steam installation

In Steam:

1. Open **Library**.
2. Right-click **Tainted Grail: The Fall of Avalon**.
3. Choose **Manage -> Browse local files**.
4. File Explorer opens at the game root.

Call that directory GameRoot.

Example only:

~~~text
D:\SteamLibrary\steamapps\common\Tainted Grail FoA
~~~

Do not copy that example blindly. Use your real path.

## Check for IL2CPP

In the game root, look for:

~~~text
GameAssembly.dll
~~~

Then look for:

~~~text
Fall of Avalon_Data\il2cpp_data\Metadata\global-metadata.dat
~~~

If those exist together, that is strong evidence you are looking at the IL2CPP layout.

The validated local snapshot recorded on **2026-08-30** was IL2CPP.

For that lane, use:

~~~text
templates\il2cpp-basic
~~~

## Check for legacy Mono

A Mono install normally has managed game assemblies under:

~~~text
Fall of Avalon_Data\Managed\
~~~

and an Assembly-CSharp.dll-style game assembly.

For a genuine legacy Mono setup, use:

~~~text
templates\mono-basic
~~~

## Confirm BepInEx before building your mod

In GameRoot, you should already have:

~~~text
BepInEx\
~~~

For the IL2CPP lane, BepInEx\core should include the IL2CPP BepInEx assemblies used by the starter project.

Launch the game once.

Then check for the BepInEx log, normally:

~~~text
BepInEx\LogOutput.log
~~~

If BepInEx itself does not start, stop here. Your own mod is not the first problem yet.

## Never mix the lanes

Do not copy Mono loader files on top of IL2CPP files or vice versa.

If you are uncertain, read docs/RUNTIME_GUIDE.md before changing the game installation.
