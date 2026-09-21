using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Tainted.Armour;

public static class KandraRuntimePayloadLayout
{
    public const string LayoutVersion = "awaken-kandra.read-serialized-data.mono-24270691.v1";
    public const int CompressedVertexByteSize = 20;
    public const int AdditionalVertexDataByteSize = 8;
    public const int PackedBonesWeightsByteSize = 12;
    public const int Float3x4ByteSize = 48;
    public const int PackedBlendshapeDatumByteSize = 16;
    public const int IndexByteSize = 2;

    public static long ExpectedMeshPayloadByteCount(int vertexCount, int bindposesCount, int blendshapeCount)
    {
        if (vertexCount <= 0) throw new ArgumentOutOfRangeException(nameof(vertexCount));
        if (bindposesCount < 0) throw new ArgumentOutOfRangeException(nameof(bindposesCount));
        if (blendshapeCount < 0) throw new ArgumentOutOfRangeException(nameof(blendshapeCount));

        checked
        {
            return
                ((long)vertexCount * CompressedVertexByteSize) +
                ((long)vertexCount * AdditionalVertexDataByteSize) +
                ((long)vertexCount * PackedBonesWeightsByteSize) +
                ((long)bindposesCount * Float3x4ByteSize) +
                ((long)blendshapeCount * vertexCount * PackedBlendshapeDatumByteSize);
        }
    }

    public static long ExpectedIndexPayloadByteCount(int indexCount)
    {
        if (indexCount < 0) throw new ArgumentOutOfRangeException(nameof(indexCount));
        checked
        {
            return (long)indexCount * IndexByteSize;
        }
    }
}

public sealed class KandraBlendshapePayload
{
    private readonly byte[] packedBlendshapeData;

    public KandraBlendshapePayload(string name, byte[] packedBlendshapeData)
    {
        Name = ContractValues.RequireText(name, nameof(name));
        this.packedBlendshapeData = CopyBytes(packedBlendshapeData, nameof(packedBlendshapeData));
    }

    public string Name { get; }

    public byte[] PackedBlendshapeData => CopyBytes(packedBlendshapeData, nameof(PackedBlendshapeData));

    internal byte[] UnsafePackedBlendshapeData => packedBlendshapeData;

    private static byte[] CopyBytes(byte[] value, string parameterName) =>
        value == null ? throw new ArgumentNullException(parameterName) : value.ToArray();
}

public sealed class KandraMeshPayloadSections
{
    private readonly byte[] compressedVertices;
    private readonly byte[] additionalVertexData;
    private readonly byte[] packedBonesWeights;
    private readonly byte[] bindposesFloat3x4;
    private readonly KandraBlendshapePayload[] blendshapes;

    public KandraMeshPayloadSections(
        byte[] compressedVertices,
        byte[] additionalVertexData,
        byte[] packedBonesWeights,
        byte[] bindposesFloat3x4,
        IEnumerable<KandraBlendshapePayload>? blendshapes = null)
    {
        this.compressedVertices = CopyBytes(compressedVertices, nameof(compressedVertices));
        this.additionalVertexData = CopyBytes(additionalVertexData, nameof(additionalVertexData));
        this.packedBonesWeights = CopyBytes(packedBonesWeights, nameof(packedBonesWeights));
        this.bindposesFloat3x4 = CopyBytes(bindposesFloat3x4, nameof(bindposesFloat3x4));
        this.blendshapes = (blendshapes ?? Enumerable.Empty<KandraBlendshapePayload>())
            .Select(value => value ?? throw new ArgumentNullException(nameof(blendshapes)))
            .ToArray();
    }

    public byte[] CompressedVertices => CopyBytes(compressedVertices, nameof(CompressedVertices));

    public byte[] AdditionalVertexData => CopyBytes(additionalVertexData, nameof(AdditionalVertexData));

    public byte[] PackedBonesWeights => CopyBytes(packedBonesWeights, nameof(PackedBonesWeights));

    public byte[] BindposesFloat3x4 => CopyBytes(bindposesFloat3x4, nameof(BindposesFloat3x4));

    public IReadOnlyList<KandraBlendshapePayload> Blendshapes => blendshapes;

    internal byte[] UnsafeCompressedVertices => compressedVertices;

    internal byte[] UnsafeAdditionalVertexData => additionalVertexData;

    internal byte[] UnsafePackedBonesWeights => packedBonesWeights;

    internal byte[] UnsafeBindposesFloat3x4 => bindposesFloat3x4;

    internal IReadOnlyList<KandraBlendshapePayload> UnsafeBlendshapes => blendshapes;

    private static byte[] CopyBytes(byte[] value, string parameterName) =>
        value == null ? throw new ArgumentNullException(parameterName) : value.ToArray();
}

public sealed class KandraWriterRequest
{
    private readonly ushort[] indices;

    public KandraWriterRequest(
        string requestId,
        string modDirectoryRoot,
        string modDirectory,
        string meshName,
        int vertexCount,
        int bindposesCount,
        KandraMeshPayloadSections meshPayload,
        IEnumerable<ushort> indices)
    {
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        ModDirectoryRoot = RequireRootPath(modDirectoryRoot, nameof(modDirectoryRoot));
        ModDirectory = RequirePathSegment(modDirectory, nameof(modDirectory));
        MeshName = RequireFileStem(meshName, nameof(meshName));
        if (vertexCount <= 0 || vertexCount > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(vertexCount));
        if (bindposesCount < 0 || bindposesCount > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(bindposesCount));
        VertexCount = vertexCount;
        BindposesCount = bindposesCount;
        MeshPayload = meshPayload ?? throw new ArgumentNullException(nameof(meshPayload));
        this.indices = (indices ?? throw new ArgumentNullException(nameof(indices))).ToArray();
        if (this.indices.Length == 0) throw new ArgumentException("At least one index is required.", nameof(indices));
        ValidatePayloadLengths();
    }

    public string RequestId { get; }

    public string ModDirectoryRoot { get; }

    public string ModDirectory { get; }

    public string MeshName { get; }

    public int VertexCount { get; }

    public int BindposesCount { get; }

    public int IndexCount => indices.Length;

    public KandraMeshPayloadSections MeshPayload { get; }

    public IReadOnlyList<ushort> Indices => indices;

    public string TargetKandraDirectory => Path.Combine(ModDirectoryRoot, ModDirectory, "Kandra");

    public string MeshDataPath => Path.Combine(TargetKandraDirectory, MeshName + ".mdkandra");

    public string IndicesDataPath => Path.Combine(TargetKandraDirectory, MeshName + ".ixkandra");

    internal ushort[] UnsafeIndices => indices;

    internal long ExpectedMeshPayloadByteCount =>
        KandraRuntimePayloadLayout.ExpectedMeshPayloadByteCount(VertexCount, BindposesCount, MeshPayload.UnsafeBlendshapes.Count);

    internal long ExpectedIndexPayloadByteCount =>
        KandraRuntimePayloadLayout.ExpectedIndexPayloadByteCount(IndexCount);

    private void ValidatePayloadLengths()
    {
        RequireLength(
            MeshPayload.UnsafeCompressedVertices,
            checked((long)VertexCount * KandraRuntimePayloadLayout.CompressedVertexByteSize),
            "compressed_vertices_length_mismatch");
        RequireLength(
            MeshPayload.UnsafeAdditionalVertexData,
            checked((long)VertexCount * KandraRuntimePayloadLayout.AdditionalVertexDataByteSize),
            "additional_vertex_data_length_mismatch");
        RequireLength(
            MeshPayload.UnsafePackedBonesWeights,
            checked((long)VertexCount * KandraRuntimePayloadLayout.PackedBonesWeightsByteSize),
            "packed_bones_weights_length_mismatch");
        RequireLength(
            MeshPayload.UnsafeBindposesFloat3x4,
            checked((long)BindposesCount * KandraRuntimePayloadLayout.Float3x4ByteSize),
            "bindposes_float3x4_length_mismatch");

        long blendshapeLength = checked((long)VertexCount * KandraRuntimePayloadLayout.PackedBlendshapeDatumByteSize);
        foreach (KandraBlendshapePayload blendshape in MeshPayload.UnsafeBlendshapes)
        {
            RequireLength(blendshape.UnsafePackedBlendshapeData, blendshapeLength, "packed_blendshape_length_mismatch:" + blendshape.Name);
        }
    }

    private static void RequireLength(byte[] bytes, long expected, string reason)
    {
        if (bytes.LongLength != expected)
        {
            throw new ArgumentException(
                reason + ":expected=" + expected.ToString(CultureInfo.InvariantCulture) + ":actual=" + bytes.LongLength.ToString(CultureInfo.InvariantCulture));
        }
    }

    private static string RequireRootPath(string path, string parameterName)
    {
        string value = ContractValues.RequireText(path, parameterName);
        return Path.GetFullPath(value);
    }

    private static string RequirePathSegment(string value, string parameterName)
    {
        string segment = ContractValues.RequireText(value, parameterName);
        if (segment == "." || segment == ".." || segment.IndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) >= 0)
        {
            throw new ArgumentException("Value must be one path segment.", parameterName);
        }

        if (segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException("Value contains invalid path characters.", parameterName);
        }

        return segment;
    }

    private static string RequireFileStem(string value, string parameterName)
    {
        string stem = ContractValues.RequireText(value, parameterName);
        if (stem == "." || stem == ".." || stem.IndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) >= 0)
        {
            throw new ArgumentException("Value must be one file stem.", parameterName);
        }

        if (stem.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException("Value contains invalid path characters.", parameterName);
        }

        return stem;
    }
}

public sealed class KandraWriterResult
{
    public KandraWriterResult(
        string stageVersion,
        string layoutVersion,
        string requestId,
        string modDirectory,
        string meshName,
        string kandraDirectory,
        string meshDataPath,
        string indicesDataPath,
        long meshDataByteCount,
        long indicesDataByteCount,
        string meshDataSha256,
        string indicesDataSha256)
    {
        StageVersion = ContractValues.RequireText(stageVersion, nameof(stageVersion));
        LayoutVersion = ContractValues.RequireText(layoutVersion, nameof(layoutVersion));
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        ModDirectory = ContractValues.RequireText(modDirectory, nameof(modDirectory));
        MeshName = ContractValues.RequireText(meshName, nameof(meshName));
        KandraDirectory = ContractValues.RequireText(kandraDirectory, nameof(kandraDirectory));
        MeshDataPath = ContractValues.RequireText(meshDataPath, nameof(meshDataPath));
        IndicesDataPath = ContractValues.RequireText(indicesDataPath, nameof(indicesDataPath));
        MeshDataByteCount = meshDataByteCount;
        IndicesDataByteCount = indicesDataByteCount;
        MeshDataSha256 = ContractValues.RequireText(meshDataSha256, nameof(meshDataSha256));
        IndicesDataSha256 = ContractValues.RequireText(indicesDataSha256, nameof(indicesDataSha256));
    }

    public string StageVersion { get; }

    public string LayoutVersion { get; }

    public string RequestId { get; }

    public string ModDirectory { get; }

    public string MeshName { get; }

    public string KandraDirectory { get; }

    public string MeshDataPath { get; }

    public string IndicesDataPath { get; }

    public long MeshDataByteCount { get; }

    public long IndicesDataByteCount { get; }

    public string MeshDataSha256 { get; }

    public string IndicesDataSha256 { get; }

    public bool MeshDataWritten => true;

    public bool IndicesDataWritten => true;

    public bool CandidateMapApplicationAllowed => false;

    public bool CandidateMapApplicationExecuted => false;

    public bool ConversionExecuted => false;

    public bool RuntimeRegistrationAllowed => false;

    public bool RuntimeRegistrationExecuted => false;

    public bool ItemEquipMutationExecuted => false;

    public bool SaveMutationExecuted => false;

    public bool NativeGameWriteExecuted => false;

    public bool DownstreamWritesExecuted => false;
}

public sealed class KandraWriterStage
{
    public const string StageVersion = "tainted-armour.kandra-writer.v1";

    public KandraWriterResult Write(KandraWriterRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        Directory.CreateDirectory(request.TargetKandraDirectory);

        byte[] meshData = BuildMeshPayload(request);
        if (meshData.LongLength != request.ExpectedMeshPayloadByteCount)
        {
            throw new InvalidOperationException("kandra_mesh_payload_length_mismatch_after_assembly");
        }

        byte[] indexData = BuildIndexPayload(request.UnsafeIndices);
        if (indexData.LongLength != request.ExpectedIndexPayloadByteCount)
        {
            throw new InvalidOperationException("kandra_index_payload_length_mismatch_after_assembly");
        }

        File.WriteAllBytes(request.MeshDataPath, meshData);
        File.WriteAllBytes(request.IndicesDataPath, indexData);

        return new KandraWriterResult(
            StageVersion,
            KandraRuntimePayloadLayout.LayoutVersion,
            request.RequestId,
            request.ModDirectory,
            request.MeshName,
            request.TargetKandraDirectory,
            request.MeshDataPath,
            request.IndicesDataPath,
            meshData.LongLength,
            indexData.LongLength,
            Sha256(meshData),
            Sha256(indexData));
    }

    private static byte[] BuildMeshPayload(KandraWriterRequest request)
    {
        checked
        {
            using (var stream = new MemoryStream((int)request.ExpectedMeshPayloadByteCount))
            {
                Write(stream, request.MeshPayload.UnsafeCompressedVertices);
                Write(stream, request.MeshPayload.UnsafeAdditionalVertexData);
                Write(stream, request.MeshPayload.UnsafePackedBonesWeights);
                Write(stream, request.MeshPayload.UnsafeBindposesFloat3x4);
                foreach (KandraBlendshapePayload blendshape in request.MeshPayload.UnsafeBlendshapes)
                {
                    Write(stream, blendshape.UnsafePackedBlendshapeData);
                }

                return stream.ToArray();
            }
        }
    }

    private static byte[] BuildIndexPayload(ushort[] indices)
    {
        checked
        {
            byte[] bytes = new byte[indices.Length * KandraRuntimePayloadLayout.IndexByteSize];
            for (int index = 0; index < indices.Length; index++)
            {
                ushort value = indices[index];
                int offset = index * 2;
                bytes[offset] = (byte)(value & 0xFF);
                bytes[offset + 1] = (byte)(value >> 8);
            }

            return bytes;
        }
    }

    private static void Write(Stream stream, byte[] bytes) =>
        stream.Write(bytes, 0, bytes.Length);

    private static string Sha256(byte[] bytes)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] digest = sha256.ComputeHash(bytes);
            var hexadecimal = new StringBuilder(digest.Length * 2);
            foreach (byte value in digest)
            {
                hexadecimal.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            }

            return "sha256:" + hexadecimal;
        }
    }
}
