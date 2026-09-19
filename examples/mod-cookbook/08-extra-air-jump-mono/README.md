# 08 — One Extra Airborne Jump

**Category:** movement  
**Source-path evidence:** LOAD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This is the most advanced code example in the first cookbook batch.

It patches HumanoidMovementBase.Update(float), remembers whether the hero was grounded before vanilla Update ran, then allows one additional jump press while airborne.

## Why prefix + postfix

Vanilla handles its ground jump inside the same Update call.

The prefix records whether the controller was grounded **before** vanilla processing. The postfix refuses to spend the extra jump on that same ground-jump input edge.

## Build

~~~powershell
dotnet build .\ExtraAirJumpExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Evidence warning

The owner-side path has build, plugin-load, patch-registration and config evidence. The actual one-extra-jump behavior remained pending in the inspected validation record.

Use a disposable save and test:

- ground jump;
- one airborne jump;
- third press blocked;
- landing rearms;
- swimming rearms;
- ground jump height remains vanilla.
