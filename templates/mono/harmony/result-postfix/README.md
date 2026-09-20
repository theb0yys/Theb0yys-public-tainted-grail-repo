# Mono Harmony Result Postfix

Starter for the common case where the native/original method should run and the mod owns only the final returned decision.

The project initially patches a self-owned target. Replace it only after verifying the real owner/signature.

Build:

```powershell
dotnet build .\HarmonyResultPostfix.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```

Preserve original execution, keep the postfix narrow, and make the override configurable/fail-closed.
