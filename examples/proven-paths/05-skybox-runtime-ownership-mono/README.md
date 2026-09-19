# 05 — Skybox Runtime Ownership

This example teaches a general Unity runtime-ownership pattern:

1. capture the state you intend to replace;
2. create objects owned by your mod;
3. apply them;
4. restore the previous state when disabled/unloaded;
5. destroy only objects your mod created.

The template uses a tiny procedurally generated cubemap. It includes no HDRI or game asset.

## Build

\`\`\`powershell
dotnet build .\SkyboxOwnershipTemplate.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
\`\`\`

Set \`Skybox.ApplyDemoOnLoad=true\` to apply the demonstration skybox.

The default is false so simply installing the template does not replace the user's sky.

## Important invariants

- capture the old skybox before the first mutation;
- never destroy the old/native skybox;
- keep references to your runtime material/texture;
- restore on disable/unload when configured;
- destroy only your owned objects;
- if a shader or resource cannot be created, leave the current skybox alone.

A real environment mod also needs scene/context gates, transition behavior, lighting/exposure review, performance testing, and current-game visual validation.

