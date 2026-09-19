# 09 — Player Magic Mana Cost

**Category:** magic / mana cost  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This example patches FoA's real `MagicUtils.GetManaCostMultiplier` path and scales the returned multiplier only when the character is the current hero.

It deliberately does **not** reproduce the finished Magic Tweaks design, heavy-cast policy, target modes, projectile spawn-cost handling, or element logic.

## Try it

Set:

```text
[Magic]
ManaCostMultiplier = 0.5
```

A supported player magic cost that passes through this route should be half the native value. `1` is vanilla.

## Build

```powershell
dotnet build .\ManaCostExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```

## Evidence boundary

The owner-side cost path builds and is researched, but the inspected records still list in-game spell-cost validation as pending. Treat this as a real target example that needs feature testing before release claims.
