# 06 — Illegal Pickup Modifier Guard

**Category:** interaction / theft  
**Source-path evidence:** RUNTIME_EVIDENCED  
**This rewritten public example:** NOT_RUN

This real FoA-target example patches Pickable.StartInteraction.

For an illegal world pickup:

- modifier held → let the original game interaction continue;
- modifier not held → return false and skip the original pickup.

The underlying owner path had user-reported allow/block checks for illegal world and container theft. This public example intentionally narrows that design to **world Pickable only**.

## Build

~~~powershell
dotnet build .\IllegalPickupGuardExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Default modifier: LeftAlt.

## Safety

Test on a disposable save. This changes interaction authorization, not item ownership/persistence rules.

Do not add container/take-all/readable-item patches until each path is understood separately.
