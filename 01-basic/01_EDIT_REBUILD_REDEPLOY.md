# Edit, Rebuild, and Redeploy

## What you're doing

You are practicing the basic development loop until you can reliably tell whether the DLL you just built is the DLL the game actually loaded.

## What you need

- a plug-in smoke test that already loads successfully;
- your plug-in source project;
- your real `GameRoot`;
- the BepInEx plug-in folder you already used.

## What you'll learn

You will learn how to:

- make one visible source change;
- rebuild the project;
- replace the deployed DLL;
- prove the new build is running;
- clear suspicious local build output without deleting random game files.

## Steps

### 1. Change something you can recognize

For the IL2CPP starter, open `Plugin.cs` and change your startup message to:

~~~csharp
Log.LogInfo("MY SECOND BUILD IS RUNNING");
~~~

### 2. Rebuild

~~~powershell
cd C:\TGModding\MyFirstTGMod
$GameRoot = "C:\Path\To\Tainted Grail FoA"

dotnet build .\Il2CppBasic.csproj -c Release -p:GameRoot="$GameRoot"
~~~

### 3. Redeploy the new DLL

~~~powershell
Copy-Item `
  .\bin\Release\net6.0\TGCommunity.Il2CppBasic.dll `
  (Join-Path $GameRoot "BepInEx\plugins\MyFirstTGMod\TGCommunity.Il2CppBasic.dll") `
  -Force
~~~

### 4. Launch and prove the new build loaded

Launch the game.

Search `BepInEx\LogOutput.log` for:

~~~text
MY SECOND BUILD IS RUNNING
~~~

Get used to checking:

- build timestamp;
- deployed DLL timestamp;
- log message/version.

### 5. Put the version in startup logs

Prefer a startup log that includes your mod version.

When users send logs later, this tells you which build they actually ran.

### 6. Clean local build output when needed

If local build state seems suspicious, remove only your project build output:

~~~powershell
Remove-Item .\bin, .\obj -Recurse -Force -ErrorAction SilentlyContinue
~~~

Then rebuild.

Do not "clean" by deleting random files from the game installation.

## What success looks like

You can make a small source change, rebuild, redeploy, launch the game, and see the new build's log message rather than one from an older DLL.

## Common problems

**The old message still appears:** you probably deployed the wrong DLL, copied to the wrong game installation, or did not replace the previous file.

**Build and deployed timestamps do not match:** redeploy the newest output before debugging your code.

**You are tempted to delete game files to fix a build issue:** clean your local project's `bin` and `obj` directories instead.

## Where to go next

Continue to **[Add Your First Config Option](02_FIRST_CONFIG_OPTION.md)**.
