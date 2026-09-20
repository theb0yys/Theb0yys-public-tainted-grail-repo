# IL2CPP Patch Health

This is the IL2CPP / BepInEx 6 counterpart to the Mono patch-health example.

It uses the same fail-closed contract:

1. resolve one exact target and postfix;
2. install the patch under one Harmony owner ID;
3. inspect the live patch table;
4. enable the feature only when that owner is present;
5. remove only that owner's partial patch from the target if installation cannot be established.

Build:

    dotnet build Il2CppPatchHealth.csproj -c Release -p:FoAGameRoot="<GameRoot>"

The target is self-owned and does not patch Tainted Grail game code.

The example demonstrates the runtime host/lifecycle difference only; the Harmony ownership rule is intentionally the same as the Mono example.
