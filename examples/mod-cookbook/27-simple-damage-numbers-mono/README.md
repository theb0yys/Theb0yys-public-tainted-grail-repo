# 27 — Simple Damage Numbers

**Category:** HUD / combat feedback  
**Source-path evidence:** LOAD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This is a deliberately small screen-space damage-number example.

It observes:

~~~text
HealthElement.TakeDamage(Damage)
~~~

and draws a short-lived number when the current hero deals damage.

It does not anchor numbers over enemies, replace the native HUD, or change damage.

## Build

~~~powershell
dotnet build .\SimpleDamageNumbersExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The example caps active rows and removes expired entries.

From here you can add critical/weak-spot styling, incoming damage, world-to-screen positioning, or shared UI integration one piece at a time.

The public rewrite is **NOT_RUN**.
