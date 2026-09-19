# Mono Harmony Self-Test

This example proves the **BepInEx 5 + Harmony** patch path without touching Tainted Grail game methods.

It patches a method inside its own plug-in assembly. A successful load logs:

```text
Harmony self-test result: patched
```

Use it only on the Mono/BepInEx 5 lane.

Build:

```powershell
dotnet build HarmonySelfTest.csproj -c Release -p:GameRoot="D:\Games\Tainted Grail FoA"
```
