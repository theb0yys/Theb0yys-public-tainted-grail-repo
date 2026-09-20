# Mono + Harmony Starter

A buildable BepInEx Mono starter with Harmony lifecycle wiring.

The included patch targets a method inside this template assembly only. It exists to verify that `PatchAll()` and `UnpatchSelf()` are wired correctly without changing FoA behaviour.

## Start here

1. Rename the assembly, namespace, plug-in GUID and display name.
2. Build against your own local BepInEx/game references.
3. Confirm the log reports `Harmony wiring self-test: patched`.
4. Remove `TemplateTarget` and `TemplateSelfTestPatch`.
5. Add only the game-specific patches justified by your inspected owner/lifecycle evidence.

Build:

```powershell
dotnet build .\HarmonyBasic.csproj -c Release -p:GameRoot="C:\Games\Tainted Grail FoA"
```

For the same mechanism as an example rather than a starter, see [Harmony self-test](../../../examples/mono/harmony/self-test/README.md).

**Evidence state:** template structure only. A successful build or self-test does not prove any FoA hook, runtime behaviour, persistence, compatibility, or release package.
