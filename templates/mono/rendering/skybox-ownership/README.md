# Mono Skybox Ownership

Starter for runtime-owned rendering replacement:

```text
capture previous state
-> create mod-owned resource
-> apply
-> restore previous state
-> destroy only mod-owned resource
```

The demo uses a generated cubemap and does not ship an HDRI/game asset.

Build:

```powershell
dotnet build .\SkyboxOwnership.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```
