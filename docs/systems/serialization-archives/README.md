# Serialization and .arch Packaging

## What it is

FoA uses **Unity Archive** as the physical container for several Questline datasets.

The `.arch` extension does not mean every contained payload uses one Questline binary schema.

The layers are:

~~~text
system-specific Questline payload
→ Questline build/package convention
→ Unity ResourceFile aliases
→ Unity Archive (.arch)
→ runtime mount
→ system-specific reader
~~~

## Build side

Questline's build tooling uses Unity's `ContentBuildInterface.ArchiveAndCompress(...)` with relative file aliases.

The inspected path uses `BuildCompression.Uncompressed`.

## Runtime side

`ArchiveUtils.TryMountAndAdjustPath(...)` uses:

- `ContentNamespace`
- `ArchiveFileInterface.MountAsync`
- mounted virtual paths

The consuming system then reads its own payload format from that mounted namespace.

## Known archive consumers

The common archive infrastructure is used by systems including:

- Drake / MergedDrake;
- Medusa;
- Story Graph runtime;
- Babel localisation;
- Kandra packaging;
- Streamed Skill Graphs;
- HLOD in current shipping/runtime research.

## Important distinction

The **outer archive container** and the **inner data format** are separate things.

Examples:

- Story uses Questline `.story` payloads;
- MergedDrake uses its own records;
- Medusa has per-scene renderer/transform payloads;
- Babel has language/string payloads.

## Modding relevance

If you are studying an `.arch` file, first identify its owning system and inner reader.

Do not infer a writer or schema from the file extension alone.

## Related systems

- [Archive/file primitives](../archive-io/README.md)
- [Drake](../drake/README.md)
- [Medusa](../medusa/README.md)
- [Story Graphs](../story-graphs/README.md)
- [Babel](../babel/README.md)
