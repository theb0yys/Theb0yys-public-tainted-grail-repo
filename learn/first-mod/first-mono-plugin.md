# Build Your First Mono Plug-in

## What you're doing

You are building the smallest useful plug-in test for the supported **Mono/BepInEx 5** Tainted Grail lane.

The goal is to prove the complete path from your source code to a log message produced by your loaded plug-in.

## What you need

- an installation you already identified as Mono;
- the matching BepInEx 5 lane installed and able to start;
- the starter repository;
- .NET Framework 4.7.2 targeting/developer pack;
- your real `GameRoot`.

## What you'll learn

You will learn how to:

- copy the Mono starter;
- give the plug-in its own stable identity;
- build the net472 project against your local installation;
- deploy only your plug-in DLL;
- confirm that BepInEx loaded your code.

## Steps

### 1. Copy the starter

~~~powershell
New-Item -ItemType Directory -Force C:\TGModding\MyFirstTGMonoMod | Out-Null
Copy-Item .\templates\mono\basic\* C:\TGModding\MyFirstTGMonoMod\
cd C:\TGModding\MyFirstTGMonoMod
~~~

### 2. Edit `Plugin.cs`

Replace it with:

~~~csharp
using BepInEx;

namespace MyFirstTGMonoMod;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "yourname.taintedgrail.myfirstmonomod";
    public const string PluginName = "My First Tainted Grail Mono Mod";
    public const string PluginVersion = "0.1.0";

    private void Awake()
    {
        Logger.LogInfo("MY FIRST MONO MOD LOADED SUCCESSFULLY");
    }
}
~~~

The **plug-in GUID** is the stable unique identifier BepInEx uses to distinguish your plug-in. Change it to an identifier you control and keep it stable once the mod is in use.

### 3. Build

Set your real game path:

~~~powershell
$GameRoot = "C:\Path\To\Tainted Grail FoA"
~~~

Build:

~~~powershell
dotnet build .\MonoBasic.csproj -c Release -p:GameRoot="$GameRoot"
~~~

The output is expected under:

~~~text
bin\Release\net472\TGCommunity.MonoBasic.dll
~~~

### 4. Deploy

~~~powershell
$PluginDir = Join-Path $GameRoot "BepInEx\plugins\MyFirstTGMonoMod"
New-Item -ItemType Directory -Force $PluginDir | Out-Null
Copy-Item .\bin\Release\net472\TGCommunity.MonoBasic.dll $PluginDir -Force
~~~

### 5. Launch and check the log

Launch the game, close it, then search the BepInEx log for:

~~~text
MY FIRST MONO MOD LOADED SUCCESSFULLY
~~~

## What success looks like

**Progress: Start → Loader working → _First plug-in_ → First game change → First complete mod**

You have reached **First plug-in** when your own DLL—not merely BepInEx—loads and produces your unique log line.

Your first Mono plug-in test passes when:

- the project builds;
- the DLL is deployed under your dedicated BepInEx plug-in folder;
- BepInEx discovers the plug-in;
- the log contains `MY FIRST MONO MOD LOADED SUCCESSFULLY`;
- no plug-in load exception is produced.

## Common problems

**The build reports missing .NET Framework reference assemblies:** install the .NET Framework 4.7.2 developer/targeting pack through Visual Studio Installer and retry.

**The log line never appears:** confirm that BepInEx itself starts, the DLL is in the correct plug-in folder, and the deployed DLL matches your newest build.

**You used this project against an IL2CPP installation:** return to **[Find the Game and Identify the Runtime](find-game-and-runtime.md)** and use the starter that matches the installed runtime.

## Where to go next

Use **[What Success Looks Like](success-checklist.md)** to confirm the whole first loop, then continue to **[Learn the Everyday Modding Loop](../everyday-modding/README.md)**.
