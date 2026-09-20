# Audio Replacement Gate

This example preserves the important invariant from a runtime-proven replacement-audio path:

> **Do not suppress the original/native event unless your replacement actually started.**

The demo patches a self-owned event and generates a short tone at runtime. No audio assets are included.

## Mechanism

\`\`\`text
event arrives
   ↓
is this event in my owned scope?
   ├── no  → native/original continues
   └── yes
        ↓
try replacement
   ├── success → suppress original
   └── failure → log and let original continue
\`\`\`

Unexpected exceptions also fail open.

## Build

\`\`\`powershell
dotnet build .\AudioReplacementGate.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
\`\`\`

Set \`Demo.RunOnLoad=true\` to run the self-owned demonstration.

## Adapting it

A real audio mod must additionally research:

- the exact current audio/event method;
- a narrow filter that proves the event belongs to your feature;
- conflict behavior when another patch owns the same target;
- asset licensing and packaging;
- lifetime/cleanup of AudioClips and AudioSources;
- volume/spatialization behavior.

Do not copy a commercial sound pack into this repo.

