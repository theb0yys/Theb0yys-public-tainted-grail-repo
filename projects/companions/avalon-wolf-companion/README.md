# Avalon Wolf Companion

Diagnostic scaffold for a future wolf companion mod for Tainted Grail: The Fall of Avalon.

## Status

Version 0.1.0 is diagnostic-only. It loads as a BepInEx v5 Mono plugin, creates config, logs the wolf target metadata, and includes a disabled summon-test gate that only logs a blocked message.

It does not spawn, transform, persist, command, recruit, or alter any actor.

## Target candidate

- Template: `Spec_AnimalWolf`
- GUID: `9086dee514edc644b9b55890d885db3f`
- Existing evidence: 26 vanilla scene spawner references in Template Diagnostics route-context dumps.
- Status: predator candidate only; not approved companion behavior.

## Config

- `General.Enabled`
- `Research.ResearchModeOnly`
- `Target.TemplateName`
- `Target.TemplateGuid`
- `Diagnostics.LogTargetOnLoad`
- `Prototype.EnableDisabledSummonTest`
- `Prototype.DisabledSummonTestHotkey`

`Prototype.EnableDisabledSummonTest` defaults to `false`, and `Prototype.DisabledSummonTestHotkey` defaults to `None`.

## Build

```powershell
dotnet build .\src\AvalonWolfCompanion.csproj -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"
```

Optional deploy:

```powershell
dotnet build .\src\AvalonWolfCompanion.csproj -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true
```
