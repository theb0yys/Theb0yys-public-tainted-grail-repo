# 03 - Build Your First IL2CPP Plug-in

Use this for the current/modern IL2CPP lane.

The goal is **not** to alter gameplay. The goal is to prove:

~~~text
your source -> build -> DLL -> BepInEx -> log
~~~

## Step 1 - Copy the starter

Create a workspace:

~~~powershell
New-Item -ItemType Directory -Force C:\TGModding\MyFirstTGMod | Out-Null
~~~

From the root of this starter repository:

~~~powershell
Copy-Item .\templates\il2cpp-basic\* C:\TGModding\MyFirstTGMod\
~~~

Open:

~~~text
C:\TGModding\MyFirstTGMod\Plugin.cs
~~~

## Step 2 - Give the plug-in your own identity

Replace Plugin.cs with this, changing yourname to your own stable name/handle:

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

The GUID must be unique to your project.

## Step 3 - Build

Open PowerShell in the project directory:

~~~powershell
cd C:\TGModding\MyFirstTGMod
~~~

Set your **real** game path:

~~~powershell
$GameRoot = "D:\SteamLibrary\steamapps\common\Tainted Grail FoA"
~~~

Build:

~~~powershell
dotnet build .\Il2CppBasic.csproj -c Release -p:GameRoot="$GameRoot"
~~~

### What success looks like

Near the end, you should see:

~~~text
Build succeeded.
~~~

The starter's DLL will be under:

~~~text
bin\Release\net6.0\TGCommunity.Il2CppBasic.dll
~~~

The filename stays that way until you later rename the assembly in the project file. That is fine for this first smoke test.

## Step 4 - Deploy only your DLL

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

## Step 5 - Launch the game

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

If that line exists, your first plug-in pipeline works.

## If the build fails

Read the **first real error**, not the last 30 follow-on errors.

Common causes:

- GameRoot is wrong;
- BepInEx IL2CPP is not installed;
- the required assemblies are not in BepInEx\core;
- the .NET build tooling is missing.

## If the build succeeds but the log line never appears

Check:

1. BepInEx itself produced a log;
2. your DLL is actually under BepInEx\plugins\MyFirstTGMod;
3. the DLL timestamp matches your newest build;
4. you used the IL2CPP template, not the Mono one;
5. read the earliest plug-in load error in the BepInEx log.

Do not add Harmony/game code until this smoke test passes.
