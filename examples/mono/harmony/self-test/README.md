# Mono Harmony Self-Test

Use this example to prove that BepInEx 5 and Harmony are wired correctly before you patch a FoA method. Because it patches code inside its own assembly, a failure here points to your mod setup rather than a game target.

This example proves the **BepInEx 5 + Harmony** patch path without touching Tainted Grail game methods.

It patches a method inside its own plug-in assembly. A successful load logs:

```text
Harmony self-test result: patched
```

Use it only on the Mono/BepInEx 5 lane.

Build:

```powershell
dotnet build HarmonySelfTest.csproj -c Release -p:GameRoot="C:\Path\To\Tainted Grail FoA"
```
