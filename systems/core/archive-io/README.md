# Shared Archive, File and Buffer Primitives

## What it is

`Awaken.Utility.dll` contains shared low-level primitives used by several proprietary systems.

These utilities are infrastructure, not one content system.

## Archive mounting

`ArchiveUtils.TryMountAndAdjustPath`:

1. builds the archive path under StreamingAssets;
2. gets/creates a Unity `ContentNamespace`;
3. reuses an existing mount when present;
4. otherwise calls `ArchiveFileInterface.MountAsync`;
5. returns the mounted virtual path.

## File reading

`FileRead` wraps Unity `AsyncReadManager` with typed read helpers for new or existing buffers.

Consumers include Leshy, Medusa, MergedDrake, Babel and HLOD.

## Buffer parsing

`BufferStreamReader` provides sequential unmanaged reads:

- `Read<T>()`
- `TryRead<T>()`
- `ReadSpan<T>()`
- `TryReadSpan<T>()`
- `ReadRest<T>()`

A system-specific writer therefore has to match exact struct layout, counts and section order.

## Packed transforms

Shared structures include:

- `PackedMatrix` — compact 3x4 transform representation;
- `SmallTransform` — position, half-precision rotation and scale.

These representations are used by high-volume rendering/streaming systems.

## Assembly

Primary assembly: `Awaken.Utility.dll`.

## Modding relevance

These primitives explain how proprietary systems share IO and memory structures without sharing the same higher-level payload schema.

## Deeper reference

- [Archive mounting and virtual paths](mounting-and-virtual-paths.md)

## Related systems

- [Serialization and archives](../serialization-archives-implementation/README.md)
- [Leshy](../../presentation/leshy/README.md)
- [Medusa](../../presentation/medusa/README.md)
- [Drake](../../presentation/drake/README.md)
- [HLOD](../../presentation/hlod/README.md)
