# Archive Mounting and Virtual Paths

This page narrows the shared archive primitives to the runtime mount/read boundary.

Canonical overview: [Shared Archive, File and Buffer Primitives](README.md).

## Mount flow

The inspected shared path is:

```text
StreamingAssets-relative archive
→ ContentNamespace
→ existing mount lookup
→ ArchiveFileInterface.MountAsync when needed
→ mounted virtual path
→ system-specific reader
```

The shared utility establishes access to the archive namespace. It does **not** interpret the payload.

## Why virtual paths matter

A consumer may receive a virtual mounted path rather than a normal filesystem path.

Do not assume:

- direct `File.ReadAllBytes` semantics;
- the archive is unpacked to disk;
- an inner resource alias equals a physical file path;
- two systems using `.arch` share the same inner schema.

## Reuse and lifetime

`ArchiveUtils.TryMountAndAdjustPath` can reuse an existing mount.

That means mount ownership and payload ownership should remain separate:

```text
archive mount available
≠
payload parsed
≠
consumer initialized
```

A mod inspecting one layer should not claim success for the next.

## Investigation checklist

When tracing an archive consumer:

1. identify the owning system;
2. identify the outer archive path;
3. identify the namespace/mount step;
4. identify the virtual resource alias/path;
5. identify the exact reader invoked after mount;
6. identify the structure/count/version information the reader expects;
7. identify cleanup/unmount behavior if the owner performs it.

For the outer-container/inner-payload distinction, see [Serialization and .arch Packaging](../serialization-archives-implementation/README.md).
