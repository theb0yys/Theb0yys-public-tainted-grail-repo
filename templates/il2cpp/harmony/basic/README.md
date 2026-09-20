# IL2CPP Harmony Basic

BepInEx 6 IL2CPP starter with Harmony lifecycle and a self-owned patch target.

Build:

```powershell
dotnet build .\Il2CppHarmonyBasic.csproj -c Release -p:GameRoot="C:\Path\To\Tainted Grail FoA"
```

Confirm the log reports `Self-test=patched`, then remove the self-test target and add only current verified interop/game targets.
