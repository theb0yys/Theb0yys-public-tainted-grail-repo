# Build Your First IL2CPP Plug-in

## What you're doing

You are building the smallest useful IL2CPP plug-in test.

The goal is **not** to alter gameplay. The goal is to prove this complete path:

~~~text
your source -> build -> DLL -> BepInEx -> log
~~~

## What you need

- an installation you already identified as IL2CPP;
- the matching BepInEx 6 IL2CPP lane installed and able to start;
- the starter repository;
- .NET build tooling;
- your real `GameRoot`.

## What you'll learn

You will learn how to:

- copy the IL2CPP starter into your own workspace;
- give a plug-in its own identity;
- build against local game/BepInEx references;
- deploy only your plug-in DLL;
- prove that BepInEx loaded your code.

## Steps

### 1. Copy the starter

Create a workspace:

~~~powershell
New-Item -ItemType Directory -Force C:\TGModding\MyFirstTGMod | Out-Null
~~~

From the root of this starter repository:

~~~powershell
Copy-Item .\templates\il2cpp\basic\* C:\TGModding\MyFirstTGMod\
~~~

Open:

~~~text
C:\TGModding\MyFirstTGMod\Plugin.cs
~~~

### 2. Give the plug-in your own identity

Replace `Plugin.cs` with this, changing `yourname` to your own stable name/handle:

~~~csharp
using BepInEx;
using BepInEx.Unity.IL2CPP;

namespace MyFirstTGMod;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "yourname.taintedgrail.myfirstmod";
    public const string PluginName = "My First Tainted Grail Mod";
    public const string PluginVersion = "0.1.0";

    public override void Load()
    {
        Log.LogInfo("MY FIRST MOD LOADED SUCCESSFULLY");
    }
}
~~~

The **plug-in GUID** is the stable unique identifier BepInEx uses to distinguish your plug-in from others. Change `yourname.taintedgrail.myfirstmod` to an identifier you control and keep it stable once people start using the mod.

### 3. Build

Open PowerShell in the project directory:

~~~powershell
cd C:\TGModding\MyFirstTGMod
~~~

Set your **real** game path:

~~~powershell
$GameRoot = "C:\Path\To\Tainted Grail FoA"
~~~

Build:

~~~powershell
dotnet build .\Il2CppBasic.csproj -c Release -p:GameRoot="$GameRoot"
~~~

Near the end, you should see:

~~~text
Build succeeded.
~~~

The starter's DLL will be under:

~~~text
bin\Release\net6.0\TGCommunity.Il2CppBasic.dll
~~~

The filename stays that way until you later rename the assembly in the project file. That is fine for this first smoke test.

### 4. Deploy only your DLL

Create a dedicated plug-in folder:

~~~powershell
$PluginDir = Join-Path $GameRoot "BepInEx\plugins\MyFirstTGMod"
New-Item -ItemType Directory -Force $PluginDir | Out-Null
~~~

Copy the DLL:

~~~powershell
Copy-Item .\bin\Release\net6.0\TGCommunity.Il2CppBasic.dll $PluginDir -Force
~~~

Do not copy your source tree into the game.

### 5. Launch and check the log

Launch FoA normally.

After the game reaches startup/menu, close it.

Open:

~~~text
<GameRoot>\BepInEx\LogOutput.log
~~~

Search for:

~~~text
MY FIRST MOD LOADED SUCCESSFULLY
~~~

## What success looks like

**Progress: Start → Loader working → _First plug-in_ → First game change → First complete mod**

You have reached **First plug-in** when your own DLL—not merely BepInEx—loads and produces your unique log line.

Your first IL2CPP plug-in test passes when:

- the project builds;
- the new DLL is in your dedicated BepInEx plug-in folder;
- BepInEx discovers the plug-in;
- the log contains `MY FIRST MOD LOADED SUCCESSFULLY`;
- no plug-in load exception is produced.

Do not add **Harmony** or game-target code until this smoke test passes. Harmony is the patching library commonly used to run your code before or after existing game methods without replacing the game files themselves.

## Common problems

**The build fails:** read the **first real error**, not the last set of follow-on errors. Common causes are a wrong `GameRoot`, missing BepInEx IL2CPP assemblies, or missing .NET tooling.

**The build succeeds but the log line never appears:** verify BepInEx produced a log, the DLL is under `BepInEx\plugins\MyFirstTGMod`, its timestamp matches your newest build, and you used the IL2CPP template.

**The wrong DLL keeps loading:** compare the build output timestamp with the deployed DLL timestamp.

## Where to go next

Use **[Confirm Your First Mod Worked](success-checklist.md)** to confirm the whole first loop.

Then continue directly to **[Make Your First IL2CPP Game Change](../everyday-modding/first-il2cpp-game-change.md)**. That lesson moves from "my DLL loads" to a small, reversible change in the running game.

After that, continue through **[Learn the Everyday Modding Loop](../everyday-modding/README.md)**.
