# 01 - Edit, Rebuild, Redeploy

Once the first smoke test works, practice the loop until it is boring.

## IL2CPP example

Open your Plugin.cs.

Change your startup message to:

~~~csharp
Log.LogInfo("MY SECOND BUILD IS RUNNING");
~~~

Then:

~~~powershell
cd C:\TGModding\MyFirstTGMod
$GameRoot = "C:\Path\To\Tainted Grail FoA"

dotnet build .\Il2CppBasic.csproj -c Release -p:GameRoot="$GameRoot"

Copy-Item `
  .\bin\Release\net6.0\TGCommunity.Il2CppBasic.dll `
  (Join-Path $GameRoot "BepInEx\plugins\MyFirstTGMod\TGCommunity.Il2CppBasic.dll") `
  -Force
~~~

Launch the game.

Search BepInEx\LogOutput.log for:

~~~text
MY SECOND BUILD IS RUNNING
~~~

## Why this matters

If the old message appears, you probably deployed the wrong DLL or copied to the wrong game installation.

Get used to checking:

- build timestamp;
- deployed DLL timestamp;
- log message/version.

## Put the version in startup logs

Prefer a startup log that includes your mod version.

When users send logs later, this tells you which build they actually ran.

## Clean rebuild when local build state seems suspicious

You can remove your local build output:

~~~powershell
Remove-Item .\bin, .\obj -Recurse -Force -ErrorAction SilentlyContinue
~~~

Then rebuild.

Do not "clean" by deleting random files from the game installation.
