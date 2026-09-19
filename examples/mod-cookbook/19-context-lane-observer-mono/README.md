# 19 — Context Lane Observer

**Category:** contextual audio / music routing  
**Source-path evidence:** RUNTIME_EVIDENCED for selected routing inputs  
**This rewritten public example:** NOT_RUN

This is a real FoA-state observer, not a music player.

It demonstrates the small part that should come **before** building a large contextual music system: decide which context lane is active and log transitions only when the lane changes.

## Observed lanes

- \`NoHero\`
- \`Wyrdness\`
- \`OpenWorld\`
- \`Interior\`
- \`Unavailable\`

The underlying maintainer music path has live evidence for open-world and Wyrdness lane decisions. Interior and richer routing still need their own exact tests.

## Build

~~~powershell
dotnet build .\ContextLaneObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Next iteration

Once this observer is stable on your game build, add one concern at a time:

1. day/night;
2. settlement/scary-place rules;
3. dialogue/combat ducking;
4. owned audio playback;
5. native-music coexistence/suppression;
6. asset packaging and licences.

Do not start by embedding gigabytes of audio before the context state machine is correct.

The public rewrite is **NOT_RUN**.
