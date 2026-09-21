using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Tainted.Armour.Canonical;

public enum CanonicalComponentType
{
    I8,
    U8,
    I16,
    U16,
    I32,
    U32,
    F32,
    F64,
}

public enum CanonicalElementShape
{
    Scalar,
    Vec2,
    Vec3,
    Vec4,
    Mat2,
    Mat3,
    Mat4,
}

public enum CanonicalSparseIndexComponentType
{
    U8,
    U16,
    U32,
}

public sealed class CanonicalCoordinateSystemV1
{
    public const string HandednessValue = "LEFT";
    public const string RightAxisValue = "+X";
    public const string UpAxisValue = "+Y";
    public const string ForwardAxisValue = "+Z";
    public const string DistanceUnitValue = "METRE";
    public const string AngleUnitValue = "RADIAN";
    public const string MatrixMathConventionValue = "COLUMN_VECTOR";
    public const string MatrixStorageConventionValue = "COLUMN_MAJOR";
    public const string QuaternionComponentOrderValue = "XYZW";
    public const string UvOriginValue = "BOTTOM_LEFT";
    public const string UDirectionValue = "RIGHT";
    public const string VDirectionValue = "UP";
    public const string FrontFaceWindingValue = "CLOCKWISE";

    public string Handedness => HandednessValue;
    public string RightAxis => RightAxisValue;
    public string UpAxis => UpAxisValue;
    public string ForwardAxis => ForwardAxisValue;
    public string DistanceUnit => DistanceUnitValue;
    public string AngleUnit => AngleUnitValue;
    public string MatrixMathConvention => MatrixMathConventionValue;
    public string MatrixStorageConvention => MatrixStorageConventionValue;
    public string QuaternionComponentOrder => QuaternionComponentOrderValue;
    public string UvOrigin => UvOriginValue;
    public string UDirection => UDirectionValue;
    public string VDirection => VDirectionValue;
    public string FrontFaceWinding => FrontFaceWindingValue;
}

public sealed class CanonicalBufferBlobRefV1
{
    public CanonicalBufferBlobRefV1(CanonicalObjectId id, ArtifactId artifact, long byteLength)
    {
        CanonicalContractGuard.RequireKind(id, "buffer", nameof(id));
        CanonicalContractGuard.RequireJsonSafeNonNegative(byteLength, nameof(byteLength));
        Id = id;
        Artifact = artifact;
        ByteLength = byteLength;
    }

    public CanonicalObjectId Id { get; }
    public ArtifactId Artifact { get; }
    public long ByteLength { get; }
}

public sealed class CanonicalBufferViewV1
{
    public CanonicalBufferViewV1(CanonicalObjectId id, CanonicalObjectId bufferId, long byteLength)
    {
        CanonicalContractGuard.RequireKind(id, "buffer-view", nameof(id));
        CanonicalContractGuard.RequireKind(bufferId, "buffer", nameof(bufferId));
        CanonicalContractGuard.RequireJsonSafeNonNegative(byteLength, nameof(byteLength));
        Id = id;
        BufferId = bufferId;
        ByteLength = byteLength;
    }

    public CanonicalObjectId Id { get; }
    public CanonicalObjectId BufferId { get; }
    public long ByteOffset => 0;
    public long ByteLength { get; }
    public int? ByteStride => null;
}

public sealed class CanonicalSparseIndicesV1
{
    public CanonicalSparseIndicesV1(
        CanonicalObjectId bufferViewId,
        CanonicalSparseIndexComponentType componentType)
    {
        CanonicalContractGuard.RequireKind(bufferViewId, "buffer-view", nameof(bufferViewId));
        BufferViewId = bufferViewId;
        ComponentType = componentType;
    }

    public CanonicalObjectId BufferViewId { get; }
    public long ByteOffset => 0;
    public CanonicalSparseIndexComponentType ComponentType { get; }
}

public sealed class CanonicalSparseValuesV1
{
    public CanonicalSparseValuesV1(CanonicalObjectId bufferViewId)
    {
        CanonicalContractGuard.RequireKind(bufferViewId, "buffer-view", nameof(bufferViewId));
        BufferViewId = bufferViewId;
    }

    public CanonicalObjectId BufferViewId { get; }
    public long ByteOffset => 0;
}

public sealed class CanonicalSparseAccessorV1
{
    public CanonicalSparseAccessorV1(
        long count,
        CanonicalSparseIndicesV1 indices,
        CanonicalSparseValuesV1 values)
    {
        CanonicalContractGuard.RequireJsonSafeNonNegative(count, nameof(count));
        Count = count;
        Indices = indices ?? throw new ArgumentNullException(nameof(indices));
        Values = values ?? throw new ArgumentNullException(nameof(values));
    }

    public long Count { get; }
    public CanonicalSparseIndicesV1 Indices { get; }
    public CanonicalSparseValuesV1 Values { get; }
}

public sealed class CanonicalAccessorV1
{
    public CanonicalAccessorV1(
        CanonicalObjectId id,
        CanonicalObjectId? bufferViewId,
        CanonicalComponentType componentType,
        CanonicalElementShape elementShape,
        long count,
        CanonicalSparseAccessorV1? sparse = null)
    {
        CanonicalContractGuard.RequireKind(id, "accessor", nameof(id));
        if (bufferViewId.HasValue) CanonicalContractGuard.RequireKind(bufferViewId.Value, "buffer-view", nameof(bufferViewId));
        CanonicalContractGuard.RequireJsonSafeNonNegative(count, nameof(count));
        Id = id;
        BufferViewId = bufferViewId;
        ComponentType = componentType;
        ElementShape = elementShape;
        Count = count;
        Sparse = sparse;
    }

    public CanonicalObjectId Id { get; }
    public CanonicalObjectId? BufferViewId { get; }
    public long ByteOffset => 0;
    public CanonicalComponentType ComponentType { get; }
    public CanonicalElementShape ElementShape { get; }
    public long Count { get; }
    public CanonicalSparseAccessorV1? Sparse { get; }
}

public sealed class CanonicalBoundsV1
{
    public CanonicalBoundsV1(double minimumX, double minimumY, double minimumZ, double maximumX, double maximumY, double maximumZ)
    {
        MinimumX = CanonicalContractGuard.RequireFinite(minimumX, nameof(minimumX));
        MinimumY = CanonicalContractGuard.RequireFinite(minimumY, nameof(minimumY));
        MinimumZ = CanonicalContractGuard.RequireFinite(minimumZ, nameof(minimumZ));
        MaximumX = CanonicalContractGuard.RequireFinite(maximumX, nameof(maximumX));
        MaximumY = CanonicalContractGuard.RequireFinite(maximumY, nameof(maximumY));
        MaximumZ = CanonicalContractGuard.RequireFinite(maximumZ, nameof(maximumZ));
        if (MinimumX > MaximumX || MinimumY > MaximumY || MinimumZ > MaximumZ)
        {
            throw new ArgumentException("Bounds minimum must not exceed maximum.");
        }
    }

    public double MinimumX { get; }
    public double MinimumY { get; }
    public double MinimumZ { get; }
    public double MaximumX { get; }
    public double MaximumY { get; }
    public double MaximumZ { get; }
}

public sealed class CanonicalPrimitiveV1
{
    public CanonicalPrimitiveV1(
        CanonicalObjectId primitiveId,
        CanonicalObjectId indicesAccessorId,
        CanonicalObjectId? materialId = null)
    {
        CanonicalContractGuard.RequireKind(primitiveId, "primitive", nameof(primitiveId));
        CanonicalContractGuard.RequireKind(indicesAccessorId, "accessor", nameof(indicesAccessorId));
        if (materialId.HasValue) CanonicalContractGuard.RequireKind(materialId.Value, "material", nameof(materialId));
        PrimitiveId = primitiveId;
        IndicesAccessorId = indicesAccessorId;
        MaterialId = materialId;
    }

    public CanonicalObjectId PrimitiveId { get; }
    public string Topology => "TRIANGLES";
    public CanonicalObjectId IndicesAccessorId { get; }
    public CanonicalObjectId? MaterialId { get; }
}

public sealed class CanonicalMeshV1
{
    public CanonicalMeshV1(
        CanonicalObjectId id,
        string? name,
        long vertexCount,
        IReadOnlyDictionary<string, CanonicalObjectId> attributes,
        IEnumerable<CanonicalPrimitiveV1> primitives,
        CanonicalBoundsV1 bounds)
    {
        CanonicalContractGuard.RequireKind(id, "mesh", nameof(id));
        CanonicalContractGuard.RequireJsonSafeNonNegative(vertexCount, nameof(vertexCount));
        Id = id;
        Name = name;
        VertexCount = vertexCount;
        Attributes = CanonicalContractGuard.CopyStringMap(attributes, nameof(attributes));
        if (!Attributes.ContainsKey("POSITION")) throw new ArgumentException("Mesh attributes require POSITION.", nameof(attributes));
        foreach (CanonicalObjectId accessorId in Attributes.Values)
        {
            CanonicalContractGuard.RequireKind(accessorId, "accessor", nameof(attributes));
        }

        CanonicalPrimitiveV1[] primitiveArray = (primitives ?? throw new ArgumentNullException(nameof(primitives))).ToArray();
        if (primitiveArray.Length == 0) throw new ArgumentException("A mesh requires at least one primitive.", nameof(primitives));
        Primitives = Array.AsReadOnly(primitiveArray);
        Bounds = bounds ?? throw new ArgumentNullException(nameof(bounds));
    }

    public CanonicalObjectId Id { get; }
    public string? Name { get; }
    public long VertexCount { get; }
    public IReadOnlyDictionary<string, CanonicalObjectId> Attributes { get; }
    public IReadOnlyList<CanonicalPrimitiveV1> Primitives { get; }
    public CanonicalBoundsV1 Bounds { get; }
}

public sealed class CanonicalMeshBindingV1
{
    public CanonicalMeshBindingV1(
        CanonicalObjectId id,
        CanonicalObjectId meshId,
        CanonicalObjectId? skinId,
        CanonicalObjectId meshToAssetBindMatrixAccessorId)
    {
        CanonicalContractGuard.RequireKind(id, "mesh-binding", nameof(id));
        CanonicalContractGuard.RequireKind(meshId, "mesh", nameof(meshId));
        if (skinId.HasValue) CanonicalContractGuard.RequireKind(skinId.Value, "skin", nameof(skinId));
        CanonicalContractGuard.RequireKind(meshToAssetBindMatrixAccessorId, "accessor", nameof(meshToAssetBindMatrixAccessorId));
        Id = id;
        MeshId = meshId;
        SkinId = skinId;
        MeshToAssetBindMatrixAccessorId = meshToAssetBindMatrixAccessorId;
    }

    public CanonicalObjectId Id { get; }
    public CanonicalObjectId MeshId { get; }
    public CanonicalObjectId? SkinId { get; }
    public CanonicalObjectId MeshToAssetBindMatrixAccessorId { get; }
}

public sealed class CanonicalArmorManifestV1
{
    public const string SchemaVersionValue = "tainted-armour.canonical-asset/1";
    public const string SemanticContractVersionValue = "tainted-armour.armour-semantics/1";
    public const string ObjectIdContractVersionValue = "tainted-armour.canonical-object-id/1";

    public CanonicalArmorManifestV1(
        CanonicalObjectId assetId,
        IReadOnlyDictionary<CanonicalObjectId, CanonicalBufferBlobRefV1> buffers,
        IReadOnlyDictionary<CanonicalObjectId, CanonicalBufferViewV1> bufferViews,
        IReadOnlyDictionary<CanonicalObjectId, CanonicalAccessorV1> accessors,
        IReadOnlyDictionary<CanonicalObjectId, CanonicalMeshV1> meshes,
        IReadOnlyDictionary<CanonicalObjectId, CanonicalMeshBindingV1> meshBindings)
    {
        CanonicalContractGuard.RequireKind(assetId, "asset", nameof(assetId));
        AssetId = assetId;
        CoordinateSystem = new CanonicalCoordinateSystemV1();
        Buffers = CanonicalContractGuard.CopyMap(buffers, nameof(buffers));
        BufferViews = CanonicalContractGuard.CopyMap(bufferViews, nameof(bufferViews));
        Accessors = CanonicalContractGuard.CopyMap(accessors, nameof(accessors));
        Meshes = CanonicalContractGuard.CopyMap(meshes, nameof(meshes));
        MeshBindings = CanonicalContractGuard.CopyMap(meshBindings, nameof(meshBindings));
        CanonicalContractGuard.RequireOneBindingPerMesh(Meshes, MeshBindings);
    }

    public string SchemaVersion => SchemaVersionValue;
    public string SemanticContractVersion => SemanticContractVersionValue;
    public string ObjectIdContractVersion => ObjectIdContractVersionValue;
    public CanonicalObjectId AssetId { get; }
    public CanonicalCoordinateSystemV1 CoordinateSystem { get; }
    public IReadOnlyDictionary<CanonicalObjectId, CanonicalBufferBlobRefV1> Buffers { get; }
    public IReadOnlyDictionary<CanonicalObjectId, CanonicalBufferViewV1> BufferViews { get; }
    public IReadOnlyDictionary<CanonicalObjectId, CanonicalAccessorV1> Accessors { get; }
    public IReadOnlyDictionary<CanonicalObjectId, CanonicalMeshV1> Meshes { get; }
    public IReadOnlyDictionary<CanonicalObjectId, CanonicalMeshBindingV1> MeshBindings { get; }
}

internal static class CanonicalContractGuard
{
    public const long MaxJsonSafeInteger = 9007199254740991L;

    public static void RequireKind(CanonicalObjectId id, string kind, string parameterName)
    {
        if (!StringComparer.Ordinal.Equals(id.Kind, kind))
        {
            throw new ArgumentException($"Canonical object ID must have kind {kind}.", parameterName);
        }
    }

    public static void RequireJsonSafeNonNegative(long value, string parameterName)
    {
        if (value < 0 || value > MaxJsonSafeInteger) throw new ArgumentOutOfRangeException(parameterName);
    }

    public static double RequireFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(parameterName);
        return value == 0d ? 0d : value;
    }

    public static IReadOnlyDictionary<CanonicalObjectId, TValue> CopyMap<TValue>(
        IReadOnlyDictionary<CanonicalObjectId, TValue> source,
        string parameterName)
    {
        if (source == null) throw new ArgumentNullException(parameterName);
        var copy = new Dictionary<CanonicalObjectId, TValue>();
        foreach (KeyValuePair<CanonicalObjectId, TValue> pair in source) copy.Add(pair.Key, pair.Value);
        return new ReadOnlyDictionary<CanonicalObjectId, TValue>(copy);
    }

    public static IReadOnlyDictionary<string, CanonicalObjectId> CopyStringMap(
        IReadOnlyDictionary<string, CanonicalObjectId> source,
        string parameterName)
    {
        if (source == null) throw new ArgumentNullException(parameterName);
        var copy = new Dictionary<string, CanonicalObjectId>(StringComparer.Ordinal);
        foreach (KeyValuePair<string, CanonicalObjectId> pair in source) copy.Add(pair.Key, pair.Value);
        return new ReadOnlyDictionary<string, CanonicalObjectId>(copy);
    }

    public static void RequireOneBindingPerMesh(
        IReadOnlyDictionary<CanonicalObjectId, CanonicalMeshV1> meshes,
        IReadOnlyDictionary<CanonicalObjectId, CanonicalMeshBindingV1> bindings)
    {
        var counts = new Dictionary<CanonicalObjectId, int>();
        foreach (CanonicalMeshBindingV1 binding in bindings.Values)
        {
            if (!meshes.ContainsKey(binding.MeshId)) throw new ArgumentException("Mesh binding references an unknown mesh.", nameof(bindings));
            counts.TryGetValue(binding.MeshId, out int count);
            counts[binding.MeshId] = count + 1;
        }

        foreach (CanonicalObjectId meshId in meshes.Keys)
        {
            if (!counts.TryGetValue(meshId, out int count) || count != 1)
            {
                throw new ArgumentException("Each mesh requires exactly one mesh binding.", nameof(bindings));
            }
        }
    }
}
