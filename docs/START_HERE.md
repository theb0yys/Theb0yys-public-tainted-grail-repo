# Start Here

## 1. Identify your runtime first

Tainted Grail has existed across distinct Unity runtime/loader lanes. Do not choose a template by guesswork.

Read [RUNTIME_GUIDE.md](RUNTIME_GUIDE.md).

For the captured Steam public build validated on 2026-08-30, the game was IL2CPP. Older community setups may still involve the Mono/BepInEx 5 lane.

## 2. Install the correct BepInEx lane locally

Use upstream BepInEx documentation and a loader package appropriate for your installed game runtime.

Do not copy BepInEx or game binaries into this repository.

Typical local structure after a successful loader install:

```text
<GameRoot>/
  BepInEx/
    core/
    plugins/
  ...
```

The exact root files differ between Mono and IL2CPP.

## 3. Choose a starter

### Mono

Use `templates/mono-basic` when the game installation is genuinely Unity Mono and the matching BepInEx 5 lane is installed.

Build:

```powershell
dotnet build templates/mono-basic/MonoBasic.csproj -c Release -p:GameRoot="D:\Games\Tainted Grail FoA"
```

### IL2CPP

Use `templates/il2cpp-basic` when the game installation is Unity IL2CPP with BepInEx 6 IL2CPP installed.

Build:

```powershell
dotnet build templates/il2cpp-basic/Il2CppBasic.csproj -c Release -p:GameRoot="D:\Games\Tainted Grail FoA"
```

Change the path to your local installation. Do not commit it.

## 4. Deploy only your plug-in

Copy your compiled plug-in DLL into:

```text
<GameRoot>/BepInEx/plugins/
```

Keep source and build outputs in your own workspace. Do not commit generated DLLs to this starter repository.

## 5. Confirm loader entry before debugging gameplay

Start the game and inspect BepInEx logs. First prove:

- the expected BepInEx lane started;
- your plug-in GUID/name/version was discovered;
- your plug-in reached its startup method.

Only then debug Harmony/game behavior.

## 6. Add game references cautiously

A real gameplay mod may need local references to game or generated interop assemblies.

Reference them from the local installation with `HintPath` or a machine-local build property. Never redistribute those assemblies from this repository.

## 7. Keep patches narrow

Prefer a small, reversible Harmony prefix/postfix/transpiler over broad mutation. Log enough context to diagnose failures without dumping proprietary or personal data.
