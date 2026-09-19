# Serialization, Unity Archives, and `.arch`

> **Reference page.** Use this when inspecting FoA packaged data, Story/Babel/renderer archives, or deciding where the actual proprietary format lives.

## What this system is

A critical research correction is:

> **`.arch` is not proven to be a proprietary Questline binary format.**

The researched architecture separates three layers:

~~~text
Questline subsystem-specific payloads / serializers
→ Questline packaging/build conventions
→ Unity Archive / ContentNamespace virtual filesystem
~~~

Questline controls what is packaged and how consumers find it.

Unity's Archive/Content APIs provide the outer container/mount layer.

## Who owns it in FoA

Important owners include:

- Questline `BuildTools` / subsystem-specific bakers;
- Questline archive utility/mount helper;
- Unity `ContentBuildInterface.ArchiveAndCompress`;
- Unity `ContentNamespace`;
- Unity `ArchiveFileInterface`;
- subsystem-specific payload readers.

## Important identities, types, and methods

The researched build path uses:

~~~text
ResourceFile[] with relative aliases
→ ContentBuildInterface.ArchiveAndCompress(...)
→ StreamingAssets/<subsystem>/<name>.arch
~~~

Runtime mount path:

~~~text
.arch file
→ ContentNamespace
→ ArchiveFileInterface.MountAsync
→ mount path / VFS
→ subsystem-specific reader
~~~

The inspected build path uses an uncompressed Unity Archive setting at the outer container level.

## Where it exists in the lifecycle

Examples of subsystem payloads packaged through this common infrastructure include:

- MergedDrake;
- Medusa;
- Story;
- streamed Skill Graphs;
- Babel/localisation;
- Kandra in the inspected build pipeline.

HLOD packaging exists in shipping builds but the exact current source build invocation remains less directly established in the inspected snapshot.

## How we interact with it

### Reverse the payload, not the extension

If you need to understand Story/Babel/Kandra/etc.:

1. mount/extract the Unity Archive;
2. identify member aliases/files;
3. find the subsystem-specific reader/writer;
4. map the inner payload schema;
5. keep the outer Archive format separate.

### Preserve relative aliases

Questline uses relative aliases for files inside archives. Those paths can be meaningful to the consumer.

### Bind conclusions to the source/build

A shared `.arch` extension does not mean every subsystem shares one payload format.

## Why this route

Earlier architectural speculation treated `.arch` as a possible hidden Questline-wide proprietary archive engine.

Source evidence rejected that: the outer container is Unity technology.

The real proprietary value lies in:

- subsystem payloads;
- their readers/writers;
- Questline bake/orchestration;
- possible source-generated serializers;
- runtime resource/lifetime systems.

## What goes wrong

### Reverse-engineering the outer archive as if it were the game schema

Wastes effort and conflates Unity infrastructure with Questline payload formats.

### Same extension = same schema

False.

Story, Babel, Kandra, Medusa, Drake, etc. can share the container while having different member payloads.

### Outer "uncompressed" = inner data is uncompressed

False. Subsystem payloads may still use custom compression/packing/quantization.

### Source-generated serializer assumed to back every archive

Not proven. The specific Questline source-generated serialization system remains a separate research subject.

## How to verify

For an archive-backed system:

1. identify exact archive path;
2. confirm mount/build owner;
3. list member aliases;
4. identify subsystem reader;
5. identify subsystem writer/baker if available;
6. map inner payload versioning/identity;
7. compare source/build/runtime evidence;
8. validate extraction/read without mutating live game files.

## Current proof boundary

Unity Archive as the common outer container is well supported.

The inner payload formats remain system-specific.

A shared Questline source-generated binary serializer is known from first-party material but its concrete generator, generated APIs, and subsystem adoption are not yet fully recovered.
