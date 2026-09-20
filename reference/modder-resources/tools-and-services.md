# Tools and Services

This page is an index of useful external tools. A tool's ability to read, extract or transform content does **not** grant rights to redistribute that content.

## Tainted Grail / Unity modding routes

Prefer the repository's existing canonical pages for game-specific behaviour:

- [Tooling and Shared Infrastructure](../../tooling/README.md)
- [Assets, Addressables, and Presentation](../assets/README.md)
- [Project Templates](../../templates/README.md)
- [Investigate](../../investigate/README.md)
- [Diagnose](../../diagnose/README.md)

Important upstream projects already referenced by this repository include:

- BepInEx — Unity/.NET plugin framework: https://github.com/BepInEx/BepInEx
- HarmonyX — runtime patching library used in the BepInEx ecosystem: https://github.com/BepInEx/HarmonyX
- Questline Merlin's Workshop — official FoA replacement-oriented toolkit: https://github.com/AR-Questline/merlin-workshop

Use the runtime/version guidance in this repository rather than assuming the newest upstream build is automatically correct for FoA.

## Managed-code inspection

### ILSpy

https://github.com/icsharpcode/ILSpy

Useful for inspecting managed assemblies and understanding types, call paths and signatures.

Repository rule: inspection may support documented static evidence, but do not publish bulk decompiled game source.

### dnSpyEx

https://github.com/dnSpyEx/dnSpy

Useful for managed assembly browsing/debugging workflows. As above, tool capability is not redistribution permission.

## Unity runtime inspection

### UnityExplorer

https://github.com/sinai-dev/UnityExplorer

Useful for runtime hierarchy/component inspection in supported Unity/mod-loader environments. Treat observations as runtime evidence only for the exact tested build/runtime.

### AssetRipper

https://github.com/AssetRipper/AssetRipper

Useful for technical inspection and conversion of Unity serialized data/assets where lawful.

**Boundary:** exporting an asset successfully does not make that asset redistributable. Do not commit extracted Tainted Grail commercial content to this repository.

## Asset creation

### Blender

https://www.blender.org/

3D modelling, UVs, rigging, animation, mesh processing and export.

### GIMP

https://www.gimp.org/

Raster texture/image editing.

### Krita

https://krita.org/

Painting, concept art and texture work.

### Inkscape

https://inkscape.org/

Vector artwork, icons and SVG editing.

### Audacity

https://www.audacityteam.org/

Audio editing and cleanup.

### FFmpeg

https://ffmpeg.org/

Command-line audio/video conversion and inspection.

## Version control and packaging

### Git

https://git-scm.com/

Use source control for code, docs and assets that are actually legal to publish.

### Git LFS

https://git-lfs.com/

Useful for large binary files that belong in the repository. Git LFS is storage technology, not a licence workaround; proprietary or non-redistributable assets remain prohibited.

### 7-Zip

https://www.7-zip.org/

Useful for deterministic release staging/inspection of archives.

## Publishing and distribution

### Nexus Mods

https://www.nexusmods.com/taintedgrailthefallofavalon

Common distribution/community surface for Tainted Grail: The Fall of Avalon mods.

Before upload, use the [publishing checklist](publishing-checklist.md) and keep repository safety separate from runtime/release validation.

## Research discipline

When a tool produces evidence, label what lane it belongs to:

- decompiler/assembly browser -> static/source-inspected evidence;
- UnityExplorer/runtime debugger -> runtime observation;
- successful compilation -> build validation;
- successful loader start -> loader validation;
- save/reload test -> persistence evidence;
- packaged release test -> release validation.

See [Public Evidence Standard](../../sources/evidence-standard.md).
