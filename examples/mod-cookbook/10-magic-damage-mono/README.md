# 10 — Player Magic Damage

**Category:** combat / magic damage  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This example uses the real `HealthElement.OnDamage` interception path and modifies only player-caused `DamageType.MagicalHitSource` damage.

It keeps FoA's damage object and native downstream handling; it changes the multiplier only.

## Try it

```text
[Magic]
DamageMultiplier = 1.5
```

## Build

```powershell
dotnet build .\MagicDamageExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```

## Evidence boundary

The owner-side damage target/build exists, but direct in-game damage multiplier validation remained pending in the inspected records. Test against a disposable combat scenario before making compatibility or balance claims.
