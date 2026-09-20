# Mono Illegal Pickup Guard

Starter for mods that add an input/permission guard to the native illegal-pickup path while leaving legal pickup and native inventory transfer untouched.

Build:

```powershell
dotnet build .\IllegalPickupGuard.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```

Reverify the exact interaction owner after game updates before shipping.
