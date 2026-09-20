# FoA Developer Tools

Small source-only PowerShell helpers for Tainted Grail: The Fall of Avalon mod authors.

These scripts automate repetitive setup work around the repository's existing templates. They do **not** ship game DLLs, BepInEx binaries, generated interop assemblies, or proprietary assets.

## Recommended flow

~~~text
Test-FoAEnvironment.ps1
→ Get-FoAFingerprint.ps1
→ New-FoAMod.ps1
→ Build-FoAMod.ps1
→ Install-FoAMod.ps1
→ Watch-FoALog.ps1
~~~

## Scripts

### Test-FoAEnvironment.ps1

Detects the installed runtime lane and validates the expected local reference layout.

It distinguishes:

- Mono game markers;
- IL2CPP game markers;
- BepInEx 5 Mono;
- BepInEx 6 Mono;
- BepInEx 6 IL2CPP;
- generated IL2CPP interop readiness.

Example:

~~~powershell
.\platform\developer-tools\Test-FoAEnvironment.ps1 -GameRoot "C:\Games\Tainted Grail FoA"
~~~

If -GameRoot is omitted, the script checks FOA_GAME_ROOT and common Steam locations.

### Get-FoAFingerprint.ps1

Hashes the important local binaries for the detected runtime lane so a mod author can record exact build/reference scope without manually hashing files.

Example:

~~~powershell
.\platform\developer-tools\Get-FoAFingerprint.ps1 -GameRoot "C:\Games\Tainted Grail FoA"
~~~

A hash proves file identity only. It does not prove runtime compatibility.

### New-FoAMod.ps1

Creates a new project from the repository's maintained Harmony-basic template.

Supported lanes:

- Mono → BepInEx 5 Mono starter;
- IL2CPP → BepInEx 6 IL2CPP starter.

Example:

~~~powershell
.\platform\developer-tools\New-FoAMod.ps1 -Runtime Mono -Name "My First Mod" -PluginGuid "yourname.tgfoa.my-first-mod" -Destination ".\MyFirstMod"
~~~

The script updates the project filename, assembly name, root namespace, C# namespace, plug-in GUID, display name, version, and README build command.

The generated Harmony target is self-owned. Confirm its self-test before replacing it with a verified FoA target.

### Build-FoAMod.ps1

Validates the local environment against the project lane, then runs dotnet build with the selected GameRoot.

~~~powershell
.\platform\developer-tools\Build-FoAMod.ps1 -Project ".\MyFirstMod\MyFirstMod.csproj" -GameRoot "C:\Games\Tainted Grail FoA"
~~~

### Install-FoAMod.ps1

Builds by default, backs up an existing installed DLL, copies the new DLL into a dedicated BepInEx plug-in folder, and verifies the copied SHA-256.

~~~powershell
.\platform\developer-tools\Install-FoAMod.ps1 -Project ".\MyFirstMod\MyFirstMod.csproj" -GameRoot "C:\Games\Tainted Grail FoA"
~~~

Use -SkipBuild only when you intentionally want to install an already-built artifact.

This helper verifies file copy identity. It does **not** launch the game or claim the plug-in loaded.

### Watch-FoALog.ps1

Tails BepInEx\LogOutput.log, optionally filtering lines.

~~~powershell
.\platform\developer-tools\Watch-FoALog.ps1 -GameRoot "C:\Games\Tainted Grail FoA"
.\platform\developer-tools\Watch-FoALog.ps1 -GameRoot "C:\Games\Tainted Grail FoA" -Pattern "My First Mod|Harmony"
~~~

## Safety model

The tools deliberately separate:

~~~text
environment/reference validation
≠ build
≠ install
≠ plug-in load
≠ hook activation
≠ gameplay behaviour
≠ persistence
~~~

Only New-FoAMod.ps1 and Install-FoAMod.ps1 mutate files. The former writes to the requested project destination. The latter writes to the selected local game's BepInEx plug-in directory and backs up an existing DLL before replacement.

## Source lineage

The Mono conventions are derived from the repository's public Mono templates and the maintained private Mono starter pattern.

The IL2CPP conventions are derived from public IL2CPP templates plus inspected working BepInEx 6 / generated-interop project layouts.

The scripts are convenience tooling, not evidence that any generated mod is runtime-safe.
