# 17 — Movement FOV Kick Strength

**Category:** camera / comfort  
**Source-path evidence:** LOAD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This example patches FoA's private `HeroFoV.GetMovementFoVMultiplier` result.

The formula preserves `1` as the neutral baseline:

```text
1 + ((nativeMultiplier - 1) * Strength)
```

So:

- `0` removes the movement FOV kick;
- `0.5` keeps half the native effect;
- `1` is vanilla.

## Build

```powershell
dotnet build .\FovKickExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```

The owner-side camera plugin has load/patch evidence and screenshot smoke evidence, but not every movement/FOV context was fully validated.
