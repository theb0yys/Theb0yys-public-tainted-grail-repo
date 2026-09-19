# 03 — Runtime UI Overlay

A tiny BepInEx-owned IMGUI surface.

It deliberately does **not** replace a native FoA screen, own the cursor, freeze gameplay, or use private art.

## What it teaches

- bind a toggle key;
- keep display state inside the plug-in;
- draw only while enabled;
- avoid permanent scene objects for a tiny diagnostic/tool surface.

## Build

\`\`\`powershell
dotnet build .\RuntimeUiTemplate.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
\`\`\`

Press the configured key (default F6) to toggle the panel.

## When not to use this

A production settings screen, complex controller UI, or native-screen extension needs proper input/cursor/focus ownership. Do not scale this one \`OnGUI\` example into an entire UI framework.

