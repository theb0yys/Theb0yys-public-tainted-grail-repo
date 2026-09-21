using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Tainted.Armour.Canonical;
using Tainted.Armour.Serialization;

namespace Tainted.Armour.Compatibility;

public sealed class PersistedR2CanonicalPackageReader
{
    public R2CanonicalArmorArtifactV1 ReadPackage(string packageDirectory, string profileId)
    {
        if (string.IsNullOrWhiteSpace(packageDirectory))
        {
            throw new ArgumentException("Package directory is required.", nameof(packageDirectory));
        }

        string root = Path.GetFullPath(packageDirectory);
        string manifestPath = Path.Combine(root, "canonical.asset.json");
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException("R2 canonical package manifest was not found.", manifestPath);
        }

        byte[] bytes = File.ReadAllBytes(manifestPath);
        CanonicalJsonVerificationResult jsonVerification = CanonicalJsonVerifier.Verify(bytes);
        if (!jsonVerification.Accepted)
        {
            throw new InvalidDataException(
                "R2 canonical package manifest is not canonical JSON: " + jsonVerification.Diagnostic);
        }

        CanonicalArmorManifestV1 manifest = PersistedCanonicalManifestReader.Read(bytes);
        VerifyReferencedBlobs(root, manifest);
        return new R2CanonicalArmorArtifactV1(
            profileId,
            manifest,
            new CanonicalManifestArtifact(bytes, ArtifactHasher.ComputeSha256(bytes)));
    }

    private static void VerifyReferencedBlobs(string packageDirectory, CanonicalArmorManifestV1 manifest)
    {
        var store = new ContentAddressedBlobStore(packageDirectory);
        foreach (CanonicalBufferBlobRefV1 buffer in manifest.Buffers.Values)
        {
            byte[] blobBytes = store.Load(buffer.Artifact);
            if (blobBytes.Length != buffer.ByteLength)
            {
                throw new InvalidDataException(
                    $"Blob {buffer.Artifact} length was {blobBytes.Length}; expected {buffer.ByteLength}.");
            }
        }
    }
}

internal static class PersistedCanonicalManifestReader
{
    public static CanonicalArmorManifestV1 Read(ReadOnlySpan<byte> canonicalJson)
    {
        using JsonDocument document = JsonDocument.Parse(canonicalJson.ToArray(), new JsonDocumentOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 128,
        });

        JsonElement root = document.RootElement;
        RequireString(root, "schemaVersion", CanonicalArmorManifestV1.SchemaVersionValue);
        RequireString(root, "semanticContractVersion", CanonicalArmorManifestV1.SemanticContractVersionValue);
        RequireString(root, "objectIdContractVersion", CanonicalArmorManifestV1.ObjectIdContractVersionValue);
        RequireCurrentEmptyObjects(root, "skeletons", "skins", "materials", "resources", "blendShapeSets", "extensions");
        RequireEmptyArray(root, "requiredExtensionContracts");
        RequireCoordinateSystem(root.GetProperty("coordinateSystem"));

        return new CanonicalArmorManifestV1(
            new CanonicalObjectId(root.GetProperty("assetId").GetString() ?? string.Empty),
            ReadMap(root.GetProperty("buffers"), ReadBuffer),
            ReadMap(root.GetProperty("bufferViews"), ReadBufferView),
            ReadMap(root.GetProperty("accessors"), ReadAccessor),
            ReadMap(root.GetProperty("meshes"), ReadMesh),
            ReadMap(root.GetProperty("meshBindings"), ReadMeshBinding));
    }

    private static CanonicalBufferBlobRefV1 ReadBuffer(CanonicalObjectId id, JsonElement element) =>
        new CanonicalBufferBlobRefV1(
            id,
            ArtifactId.Parse(RequireString(element, "artifact")),
            element.GetProperty("byteLength").GetInt64());

    private static CanonicalBufferViewV1 ReadBufferView(CanonicalObjectId id, JsonElement element)
    {
        RequireInt64(element, "byteOffset", 0);
        RequireNull(element, "byteStride");
        return new CanonicalBufferViewV1(
            id,
            new CanonicalObjectId(RequireString(element, "bufferId")),
            element.GetProperty("byteLength").GetInt64());
    }

    private static CanonicalAccessorV1 ReadAccessor(CanonicalObjectId id, JsonElement element)
    {
        RequireInt64(element, "byteOffset", 0);
        CanonicalObjectId? bufferViewId = element.GetProperty("bufferViewId").ValueKind == JsonValueKind.Null
            ? null
            : new CanonicalObjectId(element.GetProperty("bufferViewId").GetString() ?? string.Empty);
        return new CanonicalAccessorV1(
            id,
            bufferViewId,
            ParseComponentType(RequireString(element, "componentType")),
            ParseElementShape(RequireString(element, "elementShape")),
            element.GetProperty("count").GetInt64(),
            ReadSparse(element.GetProperty("sparse")));
    }

    private static CanonicalSparseAccessorV1? ReadSparse(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null) return null;
        JsonElement indices = element.GetProperty("indices");
        JsonElement values = element.GetProperty("values");
        RequireInt64(indices, "byteOffset", 0);
        RequireInt64(values, "byteOffset", 0);
        return new CanonicalSparseAccessorV1(
            element.GetProperty("count").GetInt64(),
            new CanonicalSparseIndicesV1(
                new CanonicalObjectId(RequireString(indices, "bufferViewId")),
                ParseSparseIndexComponentType(RequireString(indices, "componentType"))),
            new CanonicalSparseValuesV1(new CanonicalObjectId(RequireString(values, "bufferViewId"))));
    }

    private static CanonicalMeshV1 ReadMesh(CanonicalObjectId id, JsonElement element)
    {
        var attributes = new Dictionary<string, CanonicalObjectId>(StringComparer.Ordinal);
        foreach (JsonProperty attribute in element.GetProperty("attributes").EnumerateObject())
        {
            attributes.Add(attribute.Name, new CanonicalObjectId(attribute.Value.GetString() ?? string.Empty));
        }

        CanonicalPrimitiveV1[] primitives = element.GetProperty("primitives")
            .EnumerateArray()
            .Select(ReadPrimitive)
            .ToArray();

        return new CanonicalMeshV1(
            id,
            element.GetProperty("name").ValueKind == JsonValueKind.Null ? null : element.GetProperty("name").GetString(),
            element.GetProperty("vertexCount").GetInt64(),
            attributes,
            primitives,
            ReadBounds(element.GetProperty("bounds")));
    }

    private static CanonicalPrimitiveV1 ReadPrimitive(JsonElement element)
    {
        RequireString(element, "topology", "TRIANGLES");
        JsonElement materialElement = element.GetProperty("materialId");
        return new CanonicalPrimitiveV1(
            new CanonicalObjectId(RequireString(element, "primitiveId")),
            new CanonicalObjectId(RequireString(element, "indicesAccessorId")),
            materialElement.ValueKind == JsonValueKind.Null
                ? null
                : new CanonicalObjectId(materialElement.GetString() ?? string.Empty));
    }

    private static CanonicalBoundsV1 ReadBounds(JsonElement element)
    {
        double[] minimum = ReadVector3(element.GetProperty("minimum"));
        double[] maximum = ReadVector3(element.GetProperty("maximum"));
        return new CanonicalBoundsV1(minimum[0], minimum[1], minimum[2], maximum[0], maximum[1], maximum[2]);
    }

    private static CanonicalMeshBindingV1 ReadMeshBinding(CanonicalObjectId id, JsonElement element)
    {
        JsonElement skinElement = element.GetProperty("skinId");
        return new CanonicalMeshBindingV1(
            id,
            new CanonicalObjectId(RequireString(element, "meshId")),
            skinElement.ValueKind == JsonValueKind.Null
                ? null
                : new CanonicalObjectId(skinElement.GetString() ?? string.Empty),
            new CanonicalObjectId(RequireString(element, "meshToAssetBindMatrixAccessorId")));
    }

    private static IReadOnlyDictionary<CanonicalObjectId, TValue> ReadMap<TValue>(
        JsonElement element,
        Func<CanonicalObjectId, JsonElement, TValue> read)
    {
        var result = new Dictionary<CanonicalObjectId, TValue>();
        foreach (JsonProperty property in element.EnumerateObject())
        {
            var id = new CanonicalObjectId(property.Name);
            result.Add(id, read(id, property.Value));
        }

        return result;
    }

    private static double[] ReadVector3(JsonElement element)
    {
        double[] values = element.EnumerateArray().Select(value => value.GetDouble()).ToArray();
        if (values.Length != 3) throw new InvalidDataException("Expected a three-component vector.");
        return values;
    }

    private static CanonicalComponentType ParseComponentType(string value) => value switch
    {
        "I8" => CanonicalComponentType.I8,
        "U8" => CanonicalComponentType.U8,
        "I16" => CanonicalComponentType.I16,
        "U16" => CanonicalComponentType.U16,
        "I32" => CanonicalComponentType.I32,
        "U32" => CanonicalComponentType.U32,
        "F32" => CanonicalComponentType.F32,
        "F64" => CanonicalComponentType.F64,
        _ => throw new InvalidDataException("Unknown canonical component type: " + value),
    };

    private static CanonicalElementShape ParseElementShape(string value) => value switch
    {
        "SCALAR" => CanonicalElementShape.Scalar,
        "VEC2" => CanonicalElementShape.Vec2,
        "VEC3" => CanonicalElementShape.Vec3,
        "VEC4" => CanonicalElementShape.Vec4,
        "MAT2" => CanonicalElementShape.Mat2,
        "MAT3" => CanonicalElementShape.Mat3,
        "MAT4" => CanonicalElementShape.Mat4,
        _ => throw new InvalidDataException("Unknown canonical element shape: " + value),
    };

    private static CanonicalSparseIndexComponentType ParseSparseIndexComponentType(string value) => value switch
    {
        "U8" => CanonicalSparseIndexComponentType.U8,
        "U16" => CanonicalSparseIndexComponentType.U16,
        "U32" => CanonicalSparseIndexComponentType.U32,
        _ => throw new InvalidDataException("Unknown sparse index component type: " + value),
    };

    private static void RequireCoordinateSystem(JsonElement element)
    {
        RequireString(element, "handedness", CanonicalCoordinateSystemV1.HandednessValue);
        RequireString(element, "rightAxis", CanonicalCoordinateSystemV1.RightAxisValue);
        RequireString(element, "upAxis", CanonicalCoordinateSystemV1.UpAxisValue);
        RequireString(element, "forwardAxis", CanonicalCoordinateSystemV1.ForwardAxisValue);
        RequireString(element, "distanceUnit", CanonicalCoordinateSystemV1.DistanceUnitValue);
        RequireString(element, "angleUnit", CanonicalCoordinateSystemV1.AngleUnitValue);
        RequireString(element, "matrixMathConvention", CanonicalCoordinateSystemV1.MatrixMathConventionValue);
        RequireString(element, "matrixStorageConvention", CanonicalCoordinateSystemV1.MatrixStorageConventionValue);
        RequireString(element, "quaternionComponentOrder", CanonicalCoordinateSystemV1.QuaternionComponentOrderValue);
        RequireString(element, "uvOrigin", CanonicalCoordinateSystemV1.UvOriginValue);
        RequireString(element, "uDirection", CanonicalCoordinateSystemV1.UDirectionValue);
        RequireString(element, "vDirection", CanonicalCoordinateSystemV1.VDirectionValue);
        RequireString(element, "frontFaceWinding", CanonicalCoordinateSystemV1.FrontFaceWindingValue);
    }

    private static void RequireCurrentEmptyObjects(JsonElement root, params string[] names)
    {
        foreach (string name in names)
        {
            if (root.GetProperty(name).EnumerateObject().Any())
            {
                throw new NotSupportedException($"Current package reader does not yet support non-empty '{name}'.");
            }
        }
    }

    private static void RequireEmptyArray(JsonElement root, string name)
    {
        if (root.GetProperty(name).GetArrayLength() != 0)
        {
            throw new NotSupportedException($"Current package reader does not yet support non-empty '{name}'.");
        }
    }

    private static string RequireString(JsonElement element, string name)
    {
        string? value = element.GetProperty(name).GetString();
        if (string.IsNullOrWhiteSpace(value)) throw new InvalidDataException($"String property '{name}' is required.");
        return value;
    }

    private static void RequireString(JsonElement element, string name, string expected)
    {
        string value = RequireString(element, name);
        if (!StringComparer.Ordinal.Equals(value, expected))
        {
            throw new InvalidDataException($"Property '{name}' was '{value}', expected '{expected}'.");
        }
    }

    private static void RequireInt64(JsonElement element, string name, long expected)
    {
        long value = element.GetProperty(name).GetInt64();
        if (value != expected)
        {
            throw new InvalidDataException($"Property '{name}' was {value}, expected {expected}.");
        }
    }

    private static void RequireNull(JsonElement element, string name)
    {
        if (element.GetProperty(name).ValueKind != JsonValueKind.Null)
        {
            throw new InvalidDataException($"Property '{name}' must be null.");
        }
    }
}
