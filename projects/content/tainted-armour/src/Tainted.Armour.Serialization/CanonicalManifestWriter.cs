using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tainted.Armour.Canonical;

namespace Tainted.Armour.Serialization;

public sealed class CanonicalManifestArtifact
{
    public CanonicalManifestArtifact(byte[] bytes, ArtifactId artifactId)
    {
        Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
        ArtifactId = artifactId;
    }

    public byte[] Bytes { get; }
    public ArtifactId ArtifactId { get; }
}

public static class CanonicalManifestWriter
{
    public static CanonicalManifestArtifact Write(CanonicalArmorManifestV1 manifest)
    {
        if (manifest == null) throw new ArgumentNullException(nameof(manifest));

        JsonObject root = new JsonObject
        {
            ["schemaVersion"] = manifest.SchemaVersion,
            ["semanticContractVersion"] = manifest.SemanticContractVersion,
            ["objectIdContractVersion"] = manifest.ObjectIdContractVersion,
            ["assetId"] = manifest.AssetId.Value,
            ["coordinateSystem"] = WriteCoordinateSystem(manifest.CoordinateSystem),
            ["buffers"] = WriteBuffers(manifest.Buffers),
            ["bufferViews"] = WriteBufferViews(manifest.BufferViews),
            ["accessors"] = WriteAccessors(manifest.Accessors),
            ["meshes"] = WriteMeshes(manifest.Meshes),
            ["meshBindings"] = WriteMeshBindings(manifest.MeshBindings),
            ["skeletons"] = new JsonObject(),
            ["skins"] = new JsonObject(),
            ["materials"] = new JsonObject(),
            ["resources"] = new JsonObject(),
            ["blendShapeSets"] = new JsonObject(),
            ["extensions"] = new JsonObject(),
            ["requiredExtensionContracts"] = new JsonArray(),
        };

        byte[] ordinaryJson = JsonSerializer.SerializeToUtf8Bytes(root);
        byte[] canonical = Rfc8785CanonicalJson.Canonicalize(ordinaryJson);
        return new CanonicalManifestArtifact(canonical, ArtifactHasher.ComputeSha256(canonical));
    }

    private static JsonObject WriteCoordinateSystem(CanonicalCoordinateSystemV1 coordinateSystem) =>
        new JsonObject
        {
            ["handedness"] = coordinateSystem.Handedness,
            ["rightAxis"] = coordinateSystem.RightAxis,
            ["upAxis"] = coordinateSystem.UpAxis,
            ["forwardAxis"] = coordinateSystem.ForwardAxis,
            ["distanceUnit"] = coordinateSystem.DistanceUnit,
            ["angleUnit"] = coordinateSystem.AngleUnit,
            ["matrixMathConvention"] = coordinateSystem.MatrixMathConvention,
            ["matrixStorageConvention"] = coordinateSystem.MatrixStorageConvention,
            ["quaternionComponentOrder"] = coordinateSystem.QuaternionComponentOrder,
            ["uvOrigin"] = coordinateSystem.UvOrigin,
            ["uDirection"] = coordinateSystem.UDirection,
            ["vDirection"] = coordinateSystem.VDirection,
            ["frontFaceWinding"] = coordinateSystem.FrontFaceWinding,
        };

    private static JsonObject WriteBuffers(IReadOnlyDictionary<CanonicalObjectId, CanonicalBufferBlobRefV1> values)
    {
        JsonObject result = new JsonObject();
        foreach (KeyValuePair<CanonicalObjectId, CanonicalBufferBlobRefV1> pair in values)
        {
            RequireMapIdentity(pair.Key, pair.Value.Id);
            result[pair.Key.Value] = new JsonObject
            {
                ["artifact"] = pair.Value.Artifact.ToString(),
                ["byteLength"] = pair.Value.ByteLength,
            };
        }

        return result;
    }

    private static JsonObject WriteBufferViews(IReadOnlyDictionary<CanonicalObjectId, CanonicalBufferViewV1> values)
    {
        JsonObject result = new JsonObject();
        foreach (KeyValuePair<CanonicalObjectId, CanonicalBufferViewV1> pair in values)
        {
            RequireMapIdentity(pair.Key, pair.Value.Id);
            result[pair.Key.Value] = new JsonObject
            {
                ["bufferId"] = pair.Value.BufferId.Value,
                ["byteOffset"] = pair.Value.ByteOffset,
                ["byteLength"] = pair.Value.ByteLength,
                ["byteStride"] = null,
            };
        }

        return result;
    }

    private static JsonObject WriteAccessors(IReadOnlyDictionary<CanonicalObjectId, CanonicalAccessorV1> values)
    {
        JsonObject result = new JsonObject();
        foreach (KeyValuePair<CanonicalObjectId, CanonicalAccessorV1> pair in values)
        {
            RequireMapIdentity(pair.Key, pair.Value.Id);
            JsonObject item = new JsonObject
            {
                ["bufferViewId"] = pair.Value.BufferViewId.HasValue ? pair.Value.BufferViewId.Value.Value : null,
                ["byteOffset"] = pair.Value.ByteOffset,
                ["componentType"] = ComponentTypeText(pair.Value.ComponentType),
                ["elementShape"] = ShapeText(pair.Value.ElementShape),
                ["count"] = pair.Value.Count,
                ["sparse"] = pair.Value.Sparse == null ? null : WriteSparse(pair.Value.Sparse),
            };
            result[pair.Key.Value] = item;
        }

        return result;
    }

    private static JsonObject WriteSparse(CanonicalSparseAccessorV1 sparse) =>
        new JsonObject
        {
            ["count"] = sparse.Count,
            ["indices"] = new JsonObject
            {
                ["bufferViewId"] = sparse.Indices.BufferViewId.Value,
                ["byteOffset"] = sparse.Indices.ByteOffset,
                ["componentType"] = sparse.Indices.ComponentType.ToString().ToUpperInvariant(),
            },
            ["values"] = new JsonObject
            {
                ["bufferViewId"] = sparse.Values.BufferViewId.Value,
                ["byteOffset"] = sparse.Values.ByteOffset,
            },
        };

    private static JsonObject WriteMeshes(IReadOnlyDictionary<CanonicalObjectId, CanonicalMeshV1> values)
    {
        JsonObject result = new JsonObject();
        foreach (KeyValuePair<CanonicalObjectId, CanonicalMeshV1> pair in values)
        {
            RequireMapIdentity(pair.Key, pair.Value.Id);
            JsonObject attributes = new JsonObject();
            foreach (KeyValuePair<string, CanonicalObjectId> attribute in pair.Value.Attributes)
            {
                attributes[attribute.Key] = attribute.Value.Value;
            }

            JsonArray primitives = new JsonArray();
            foreach (CanonicalPrimitiveV1 primitive in pair.Value.Primitives)
            {
                primitives.Add(new JsonObject
                {
                    ["primitiveId"] = primitive.PrimitiveId.Value,
                    ["topology"] = primitive.Topology,
                    ["indicesAccessorId"] = primitive.IndicesAccessorId.Value,
                    ["materialId"] = primitive.MaterialId.HasValue ? primitive.MaterialId.Value.Value : null,
                    ["extensions"] = new JsonObject(),
                });
            }

            result[pair.Key.Value] = new JsonObject
            {
                ["name"] = pair.Value.Name,
                ["vertexCount"] = pair.Value.VertexCount,
                ["attributes"] = attributes,
                ["primitives"] = primitives,
                ["bounds"] = WriteBounds(pair.Value.Bounds),
                ["extensions"] = new JsonObject(),
            };
        }

        return result;
    }

    private static JsonObject WriteBounds(CanonicalBoundsV1 bounds) =>
        new JsonObject
        {
            ["minimum"] = new JsonArray(
                JsonValue.Create(bounds.MinimumX),
                JsonValue.Create(bounds.MinimumY),
                JsonValue.Create(bounds.MinimumZ)),
            ["maximum"] = new JsonArray(
                JsonValue.Create(bounds.MaximumX),
                JsonValue.Create(bounds.MaximumY),
                JsonValue.Create(bounds.MaximumZ)),
        };

    private static JsonObject WriteMeshBindings(IReadOnlyDictionary<CanonicalObjectId, CanonicalMeshBindingV1> values)
    {
        JsonObject result = new JsonObject();
        foreach (KeyValuePair<CanonicalObjectId, CanonicalMeshBindingV1> pair in values)
        {
            RequireMapIdentity(pair.Key, pair.Value.Id);
            result[pair.Key.Value] = new JsonObject
            {
                ["meshId"] = pair.Value.MeshId.Value,
                ["skinId"] = pair.Value.SkinId.HasValue ? pair.Value.SkinId.Value.Value : null,
                ["meshToAssetBindMatrixAccessorId"] = pair.Value.MeshToAssetBindMatrixAccessorId.Value,
                ["extensions"] = new JsonObject(),
            };
        }

        return result;
    }

    private static void RequireMapIdentity(CanonicalObjectId key, CanonicalObjectId value)
    {
        if (key != value) throw new InvalidOperationException($"Map key {key} does not match object ID {value}.");
    }

    private static string ComponentTypeText(CanonicalComponentType value) => value switch
    {
        CanonicalComponentType.I8 => "I8",
        CanonicalComponentType.U8 => "U8",
        CanonicalComponentType.I16 => "I16",
        CanonicalComponentType.U16 => "U16",
        CanonicalComponentType.I32 => "I32",
        CanonicalComponentType.U32 => "U32",
        CanonicalComponentType.F32 => "F32",
        CanonicalComponentType.F64 => "F64",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    private static string ShapeText(CanonicalElementShape value) => value switch
    {
        CanonicalElementShape.Scalar => "SCALAR",
        CanonicalElementShape.Vec2 => "VEC2",
        CanonicalElementShape.Vec3 => "VEC3",
        CanonicalElementShape.Vec4 => "VEC4",
        CanonicalElementShape.Mat2 => "MAT2",
        CanonicalElementShape.Mat3 => "MAT3",
        CanonicalElementShape.Mat4 => "MAT4",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };
}
