using System;
using System.Collections.Generic;
using System.Linq;
using Tainted.Armour.Canonical;

namespace Tainted.Armour.Serialization;

public sealed class FullGeometryCanonicalPackageRequestV1
{
    private readonly float[] positions;
    private readonly uint[] indices;

    public FullGeometryCanonicalPackageRequestV1(
        string sourceObjectKey,
        string meshName,
        int submeshIndex,
        IEnumerable<float> positions,
        IEnumerable<uint> indices)
    {
        SourceObjectKey = RequireText(sourceObjectKey, nameof(sourceObjectKey));
        MeshName = RequireText(meshName, nameof(meshName));
        if (submeshIndex < 0) throw new ArgumentOutOfRangeException(nameof(submeshIndex));
        SubmeshIndex = submeshIndex;
        this.positions = positions?.ToArray() ?? throw new ArgumentNullException(nameof(positions));
        this.indices = indices?.ToArray() ?? throw new ArgumentNullException(nameof(indices));
        if (this.positions.Length == 0 || this.positions.Length % 3 != 0)
        {
            throw new ArgumentException("Position scalars must contain one or more complete VEC3 elements.", nameof(positions));
        }

        if (this.indices.Length == 0 || this.indices.Length % 3 != 0)
        {
            throw new ArgumentException("Index scalars must contain one or more complete triangle elements.", nameof(indices));
        }
    }

    public string SourceObjectKey { get; }
    public string MeshName { get; }
    public int SubmeshIndex { get; }
    public IReadOnlyList<float> Positions => positions;
    public IReadOnlyList<uint> Indices => indices;

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", parameterName);
        return value.Trim();
    }
}

public sealed class FullGeometryCanonicalPackageV1
{
    public FullGeometryCanonicalPackageV1(CanonicalArmorManifestV1 manifest, IReadOnlyList<CanonicalBlob> blobs)
    {
        Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        Blobs = blobs ?? throw new ArgumentNullException(nameof(blobs));
    }

    public CanonicalArmorManifestV1 Manifest { get; }
    public IReadOnlyList<CanonicalBlob> Blobs { get; }
    public CanonicalManifestArtifact ManifestArtifact => CanonicalManifestWriter.Write(Manifest);
}

public sealed class FullGeometryCanonicalPackageBuilder
{
    public const string ContractVersion = "tainted-armour.full-geometry-canonical-package-builder/1";

    public FullGeometryCanonicalPackageV1 Build(FullGeometryCanonicalPackageRequestV1 request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        int vertexCount = checked(request.Positions.Count / 3);
        ValidateIndices(request.Indices, vertexCount);

        CanonicalBlob positionsBlob = CanonicalBinaryWriter.WriteFloat32(request.Positions);
        CanonicalBlob indicesBlob = CanonicalBinaryWriter.WriteUInt32(request.Indices);
        CanonicalBlob matrixBlob = CanonicalBinaryWriter.WriteMatrix4x4F32(new CanonicalMatrix4x4F32(
            1f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f));

        CanonicalObjectId assetId = Id("asset", request.SourceObjectKey + "/asset");
        CanonicalObjectId positionsBufferId = Id("buffer", request.SourceObjectKey + "/buffer/positions");
        CanonicalObjectId indicesBufferId = Id("buffer", request.SourceObjectKey + "/buffer/submesh-" + request.SubmeshIndex + "/indices");
        CanonicalObjectId matrixBufferId = Id("buffer", request.SourceObjectKey + "/buffer/mesh-to-asset-bind-matrix");
        CanonicalObjectId positionsViewId = Id("buffer-view", request.SourceObjectKey + "/view/positions");
        CanonicalObjectId indicesViewId = Id("buffer-view", request.SourceObjectKey + "/view/submesh-" + request.SubmeshIndex + "/indices");
        CanonicalObjectId matrixViewId = Id("buffer-view", request.SourceObjectKey + "/view/mesh-to-asset-bind-matrix");
        CanonicalObjectId positionsAccessorId = Id("accessor", request.SourceObjectKey + "/accessor/positions");
        CanonicalObjectId indicesAccessorId = Id("accessor", request.SourceObjectKey + "/accessor/submesh-" + request.SubmeshIndex + "/indices");
        CanonicalObjectId matrixAccessorId = Id("accessor", request.SourceObjectKey + "/accessor/mesh-to-asset-bind-matrix");
        CanonicalObjectId meshId = Id("mesh", request.SourceObjectKey + "/mesh/" + request.MeshName);
        CanonicalObjectId primitiveId = Id("primitive", request.SourceObjectKey + "/primitive/submesh-" + request.SubmeshIndex);
        CanonicalObjectId bindingId = Id("mesh-binding", request.SourceObjectKey + "/binding/" + request.MeshName);

        var manifest = new CanonicalArmorManifestV1(
            assetId,
            new Dictionary<CanonicalObjectId, CanonicalBufferBlobRefV1>
            {
                [positionsBufferId] = new CanonicalBufferBlobRefV1(positionsBufferId, positionsBlob.Artifact, positionsBlob.Bytes.Length),
                [indicesBufferId] = new CanonicalBufferBlobRefV1(indicesBufferId, indicesBlob.Artifact, indicesBlob.Bytes.Length),
                [matrixBufferId] = new CanonicalBufferBlobRefV1(matrixBufferId, matrixBlob.Artifact, matrixBlob.Bytes.Length),
            },
            new Dictionary<CanonicalObjectId, CanonicalBufferViewV1>
            {
                [positionsViewId] = new CanonicalBufferViewV1(positionsViewId, positionsBufferId, positionsBlob.Bytes.Length),
                [indicesViewId] = new CanonicalBufferViewV1(indicesViewId, indicesBufferId, indicesBlob.Bytes.Length),
                [matrixViewId] = new CanonicalBufferViewV1(matrixViewId, matrixBufferId, matrixBlob.Bytes.Length),
            },
            new Dictionary<CanonicalObjectId, CanonicalAccessorV1>
            {
                [positionsAccessorId] = new CanonicalAccessorV1(positionsAccessorId, positionsViewId, CanonicalComponentType.F32, CanonicalElementShape.Vec3, vertexCount),
                [indicesAccessorId] = new CanonicalAccessorV1(indicesAccessorId, indicesViewId, CanonicalComponentType.U32, CanonicalElementShape.Scalar, request.Indices.Count),
                [matrixAccessorId] = new CanonicalAccessorV1(matrixAccessorId, matrixViewId, CanonicalComponentType.F32, CanonicalElementShape.Mat4, 1),
            },
            new Dictionary<CanonicalObjectId, CanonicalMeshV1>
            {
                [meshId] = new CanonicalMeshV1(
                    meshId,
                    request.MeshName,
                    vertexCount,
                    new Dictionary<string, CanonicalObjectId>(StringComparer.Ordinal)
                    {
                        ["POSITION"] = positionsAccessorId,
                    },
                    new[] { new CanonicalPrimitiveV1(primitiveId, indicesAccessorId) },
                    ComputeBounds(request.Positions)),
            },
            new Dictionary<CanonicalObjectId, CanonicalMeshBindingV1>
            {
                [bindingId] = new CanonicalMeshBindingV1(bindingId, meshId, null, matrixAccessorId),
            });

        return new FullGeometryCanonicalPackageV1(
            manifest,
            new[] { positionsBlob, indicesBlob, matrixBlob });
    }

    private static void ValidateIndices(IReadOnlyList<uint> indices, int vertexCount)
    {
        for (int index = 0; index < indices.Count; index++)
        {
            if (indices[index] >= vertexCount)
            {
                throw new ArgumentOutOfRangeException(nameof(indices), "Index scalar is outside the source vertex range.");
            }
        }
    }

    private static CanonicalBoundsV1 ComputeBounds(IReadOnlyList<float> positions)
    {
        double minX = positions[0];
        double minY = positions[1];
        double minZ = positions[2];
        double maxX = positions[0];
        double maxY = positions[1];
        double maxZ = positions[2];
        for (int offset = 0; offset < positions.Count; offset += 3)
        {
            double x = positions[offset];
            double y = positions[offset + 1];
            double z = positions[offset + 2];
            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (z < minZ) minZ = z;
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
            if (z > maxZ) maxZ = z;
        }

        return new CanonicalBoundsV1(minX, minY, minZ, maxX, maxY, maxZ);
    }

    private static CanonicalObjectId Id(string kind, string key) =>
        CanonicalObjectIdDeriver.DeriveSource(kind, ContractVersion, key);
}
