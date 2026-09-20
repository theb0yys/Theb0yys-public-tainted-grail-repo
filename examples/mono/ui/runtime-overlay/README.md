# Runtime UI Overlay

Use this example when you need a tiny mod-owned IMGUI window for diagnostics or a narrow tool.

It intentionally owns only its own panel. Native FoA menus, cursor state, pausing, focus, and controller input remain outside this example.

## What it teaches

- bind a toggle key;
- keep display state inside the plug-in;
- draw only while enabled;
- avoid permanent scene objects for a tiny diagnostic/tool surface.

## Build

\`\`\`powershell
dotnet build .\RuntimeUiOverlay.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
\`\`\`

Press the configured key (default F6) to toggle the panel.

## When not to use this

A production settings screen, complex controller UI, or native-screen extension needs proper input/cursor/focus ownership. Do not scale this one \`OnGUI\` example into an entire UI framework.

