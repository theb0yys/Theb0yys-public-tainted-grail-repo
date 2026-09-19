# Make Your First IL2CPP Game Change

## What you're doing

Your first IL2CPP plug-in already proved that BepInEx can load your code.

Now you will make one small change to the **running Tainted Grail process**: temporarily cap its target frame rate to 30 FPS.

This is deliberately simpler than a Harmony patch. It teaches runtime ownership before game-internal method patching.

## What you need

- the IL2CPP smoke test already working;
- BepInEx 6 IL2CPP installed;
- the repository;
- your real `GameRoot`;
- optional: an FPS counter so the effect is easy to observe.

## What you'll learn

You will learn how to:

- reference Unity runtime assemblies from the installed IL2CPP toolchain;
- read the game's current runtime state;
- change one runtime value;
- log the before/after state;
- restore the values your plug-in changed;
- distinguish a Unity-runtime change from a game-specific Harmony patch.

## Steps

### 1. Build the ready-made example

From the repository root:

~~~powershell
$GameRoot = "C:\Path\To\Tainted Grail FoA"

dotnet build \
  .\examples\il2cpp-first-game-change\FirstGameChange.csproj \
  -c Release \
  -p:GameRoot="$GameRoot"
~~~

The project adds two Unity references from the installed BepInEx 6 IL2CPP environment:

~~~text
<GameRoot>\BepInEx\unity-libs\UnityEngine.dll
<GameRoot>\BepInEx\unity-libs\UnityEngine.CoreModule.dll
~~~

Those remain local references. Do not copy them into this repository.

### 2. Read the change before deploying it

Open:

~~~text
examples\il2cpp-first-game-change\Plugin.cs
~~~

The important sequence is:

~~~csharp
_previousTargetFrameRate = Application.targetFrameRate;
_previousVSyncCount = QualitySettings.vSyncCount;

QualitySettings.vSyncCount = 0;
Application.targetFrameRate = 30;
~~~

The plug-in reads the old state first because it owns the responsibility for undoing its own runtime change.

### 3. Deploy only the example DLL

~~~powershell
$PluginDir = Join-Path $GameRoot "BepInEx\plugins\TGCommunity.Il2CppFirstGameChange"
New-Item -ItemType Directory -Force $PluginDir | Out-Null

Copy-Item \
  .\examples\il2cpp-first-game-change\bin\Release\net6.0\TGCommunity.Il2CppFirstGameChange.dll \
  $PluginDir \
  -Force
~~~

### 4. Launch Tainted Grail

Launch the game normally.

The plug-in disables VSync for the current process and requests a 30 FPS target.

It does not modify a save or game file.

### 5. Check the log

Open:

~~~text
<GameRoot>\BepInEx\LogOutput.log
~~~

Look for:

~~~text
First IL2CPP Game Change applied.
~~~

The same line records both the previous and new values.

### 6. Observe the result

If you have an FPS counter enabled, you should see the running game limited to roughly 30 FPS.

The exact observed frame rate can vary because frame rate is affected by workload and platform timing. The key lesson is that your plug-in is now **changing runtime state**, not merely printing a log line.

### 7. Understand the restore path

The example implements `Unload()` and restores the values it captured.

That pattern matters for later mods:

~~~text
capture -> change -> own -> restore
~~~

Do not mutate global runtime state and then forget what it was before your plug-in arrived.

## What success looks like

This exercise succeeds when:

- the project builds against your installed IL2CPP environment;
- BepInEx loads the DLL;
- the log records the old and new frame-rate/VSync values;
- the running game reflects the requested cap closely enough to observe;
- removing the plug-in returns the next launch to the game's normal settings.

This exact public example has not been run end-to-end during this documentation pass, so record your own build/game version and observed result.

## Common problems

**`UnityEngine.CoreModule.dll` is missing:** check that you are pointing at the actual BepInEx 6 IL2CPP installation and that its Unity libraries were generated/installed correctly.

**The log says the values changed but FPS is below 30:** the game may simply be unable to reach the cap in that scene. A cap is a maximum target, not a performance guarantee.

**FPS is not exactly 30:** scheduling and measurement vary. Confirm that the target value changed in the log before treating small counter differences as a plug-in failure.

**You want to patch a Tainted Grail class next:** that is the next layer. You will need generated interop assemblies such as `TG.Main.dll`, plus Harmony, and you must verify the exact target for your current game build.

## Where to go next

Continue to **[Move to a Real Game Patch](04_FIRST_REAL_PATCH_RULES.md)** for the rules around game-specific targets.

If you want to understand the generated IL2CPP assemblies first, read **[Runtime Guide](../docs/RUNTIME_GUIDE.md)** and inspect your local `BepInEx\interop` directory.
