# 04 - Build Your First Legacy Mono Plug-in

Use this only when you have confirmed a genuine Mono/BepInEx 5 setup.

Do not use this guide simply because an old tutorial mentions BepInEx 5.

## Step 1 - Copy the starter

~~~powershell
New-Item -ItemType Directory -Force C:\TGModding\MyFirstTGMonoMod | Out-Null
Copy-Item .\templates\mono-basic\* C:\TGModding\MyFirstTGMonoMod\
cd C:\TGModding\MyFirstTGMonoMod
~~~

## Step 2 - Edit Plugin.cs

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

Change the GUID to one you own.

## Step 3 - Build

Set your real game path:

~~~powershell
$GameRoot = "D:\SteamLibrary\steamapps\common\Tainted Grail FoA"
~~~

Build:

~~~powershell
dotnet build .\MonoBasic.csproj -c Release -p:GameRoot="$GameRoot"
~~~

The output is expected under:

~~~text
bin\Release\net472\TGCommunity.MonoBasic.dll
~~~

If the build complains about .NET Framework reference assemblies/targeting packs, install the .NET Framework 4.7.2 developer/targeting pack through Visual Studio Installer and retry.

## Step 4 - Deploy

~~~powershell
$PluginDir = Join-Path $GameRoot "BepInEx\plugins\MyFirstTGMonoMod"
New-Item -ItemType Directory -Force $PluginDir | Out-Null
Copy-Item .\bin\Release\net472\TGCommunity.MonoBasic.dll $PluginDir -Force
~~~

Launch the game, close it, then search the BepInEx log for:

~~~text
MY FIRST MONO MOD LOADED SUCCESSFULLY
~~~

That is the complete success condition for this first exercise.
