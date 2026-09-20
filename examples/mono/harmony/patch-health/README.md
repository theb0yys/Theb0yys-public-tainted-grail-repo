# Mono Patch Health

This example demonstrates a reusable fail-closed Harmony installation pattern without touching a Tainted Grail game method.

The example:

1. resolves one exact target and one exact postfix;
2. installs the postfix manually;
3. checks Harmony's live patch information for the expected owner ID;
4. enables the feature only after that ownership check succeeds;
5. removes only this owner's patches from the target when installation cannot be established;
6. keeps the patch itself inert while the feature is disabled.

Build:

    dotnet build PatchHealth.csproj -c Release -p:FoAGameRoot="<GameRoot>"

The self-owned target returns `1` without the feature and `11` when the verified postfix is active.

## Reuse pattern

Copy or adapt `HarmonyPatchHealth` when a feature depends on a required Harmony target.

For a real game feature, replace only the target-resolution step after the native owner and exact signature have been established.

Do not replace a missing target with a similar-looking method automatically.

Keep feature readiness separate from plug-in readiness: one failed optional patch should disable only the capability that owns it unless the whole plug-in truly depends on that target.
