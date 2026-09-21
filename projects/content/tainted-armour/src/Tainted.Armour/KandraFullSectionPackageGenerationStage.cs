using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Tainted.Armour;

public sealed class KandraFullSectionSkinWeights
{
    public KandraFullSectionSkinWeights(
        int boneIndex0,
        int boneIndex1,
        int boneIndex2,
        int boneIndex3,
        float weight0,
        float weight1,
        float weight2,
        float weight3)
    {
        BoneIndex0 = RequireUShort(boneIndex0, nameof(boneIndex0));
        BoneIndex1 = RequireUShort(boneIndex1, nameof(boneIndex1));
        BoneIndex2 = RequireUShort(boneIndex2, nameof(boneIndex2));
        BoneIndex3 = RequireUShort(boneIndex3, nameof(boneIndex3));
        Weight0 = RequireNonNegativeFinite(weight0, nameof(weight0));
        Weight1 = RequireNonNegativeFinite(weight1, nameof(weight1));
        Weight2 = RequireNonNegativeFinite(weight2, nameof(weight2));
        Weight3 = RequireNonNegativeFinite(weight3, nameof(weight3));
    }

    public ushort BoneIndex0 { get; }

    public ushort BoneIndex1 { get; }

    public ushort BoneIndex2 { get; }

    public ushort BoneIndex3 { get; }

    public float Weight0 { get; }

    public float Weight1 { get; }

    public float Weight2 { get; }

    public float Weight3 { get; }

    private static ushort RequireUShort(int value, string parameterName)
    {
        if (value < 0 || value > ushort.MaxValue) throw new ArgumentOutOfRangeException(parameterName);
        return (ushort)value;
    }

    private static float RequireNonNegativeFinite(float value, string parameterName)
    {
        if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f) throw new ArgumentOutOfRangeException(parameterName);
        return value;
    }
}

public sealed class KandraFullSectionVertex
{
    public KandraFullSectionVertex(
        ArmorVector3 position,
        ArmorVector3 normal,
        ArmorVector3 tangent,
        float tangentW,
        float uv0X,
        float uv0Y,
        KandraFullSectionSkinWeights skinWeights)
    {
        Position = position;
        Normal = normal;
        Tangent = tangent;
        TangentW = RequireFinite(tangentW, nameof(tangentW));
        Uv0X = RequireFinite(uv0X, nameof(uv0X));
        Uv0Y = RequireFinite(uv0Y, nameof(uv0Y));
        SkinWeights = skinWeights ?? throw new ArgumentNullException(nameof(skinWeights));
    }

    public ArmorVector3 Position { get; }

    public ArmorVector3 Normal { get; }

    public ArmorVector3 Tangent { get; }

    public float TangentW { get; }

    public float Uv0X { get; }

    public float Uv0Y { get; }

    public KandraFullSectionSkinWeights SkinWeights { get; }

    private static float RequireFinite(float value, string parameterName)
    {
        if (float.IsNaN(value) || float.IsInfinity(value)) throw new ArgumentOutOfRangeException(parameterName);
        return value;
    }
}

public sealed class KandraFullSectionBindposeFloat3x4
{
    private readonly float[] values;

    public KandraFullSectionBindposeFloat3x4(IEnumerable<float> rowMajorValues)
    {
        values = (rowMajorValues ?? throw new ArgumentNullException(nameof(rowMajorValues))).ToArray();
        if (values.Length != 12)
        {
            throw new ArgumentException("A Kandra bindpose requires exactly twelve row-major float3x4 values.", nameof(rowMajorValues));
        }

        foreach (float value in values)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(rowMajorValues), "Bindpose values must be finite.");
            }
        }
    }

    public IReadOnlyList<float> Values => values;

    internal float[] UnsafeValues => values;
}

public sealed class KandraFullSectionSourceMesh
{
    private readonly KandraFullSectionVertex[] vertices;
    private readonly ushort[] indices;
    private readonly KandraFullSectionBindposeFloat3x4[] bindposes;
    private readonly string[] blendshapeNames;

    public KandraFullSectionSourceMesh(
        string sourceMeshName,
        IEnumerable<KandraFullSectionVertex> vertices,
        IEnumerable<ushort> indices,
        IEnumerable<KandraFullSectionBindposeFloat3x4> bindposes,
        IEnumerable<string>? blendshapeNames = null)
    {
        SourceMeshName = ContractValues.RequireText(sourceMeshName, nameof(sourceMeshName));
        this.vertices = (vertices ?? throw new ArgumentNullException(nameof(vertices))).ToArray();
        this.indices = (indices ?? throw new ArgumentNullException(nameof(indices))).ToArray();
        this.bindposes = (bindposes ?? throw new ArgumentNullException(nameof(bindposes))).ToArray();
        this.blendshapeNames = (blendshapeNames ?? Enumerable.Empty<string>())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToArray();
        if (this.vertices.Length == 0 || this.vertices.Length > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(vertices));
        if (this.indices.Length == 0) throw new ArgumentException("At least one index is required.", nameof(indices));
        if (this.indices.Any(index => index >= this.vertices.Length))
        {
            throw new ArgumentException("Every index must reference a source vertex.", nameof(indices));
        }

        if (this.bindposes.Length > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(bindposes));
    }

    public string SourceMeshName { get; }

    public int VertexCount => vertices.Length;

    public int IndexCount => indices.Length;

    public int BindposesCount => bindposes.Length;

    public int BlendshapeCount => blendshapeNames.Length;

    public IReadOnlyList<KandraFullSectionVertex> Vertices => vertices;

    public IReadOnlyList<ushort> Indices => indices;

    public IReadOnlyList<KandraFullSectionBindposeFloat3x4> Bindposes => bindposes;

    public IReadOnlyList<string> BlendshapeNames => blendshapeNames;

    internal KandraFullSectionVertex[] UnsafeVertices => vertices;

    internal ushort[] UnsafeIndices => indices;

    internal KandraFullSectionBindposeFloat3x4[] UnsafeBindposes => bindposes;
}

public sealed class KandraFullSectionPackageGenerationRequest
{
    public KandraFullSectionPackageGenerationRequest(
        string requestId,
        string modDirectoryRoot,
        string modDirectory,
        string meshName,
        string acceptedPlus16EncoderId,
        KandraFullSectionSourceMesh sourceMesh)
    {
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        ModDirectoryRoot = Path.GetFullPath(ContractValues.RequireText(modDirectoryRoot, nameof(modDirectoryRoot)));
        ModDirectory = RequirePathSegment(modDirectory, nameof(modDirectory));
        MeshName = RequireFileStem(meshName, nameof(meshName));
        AcceptedPlus16EncoderId = ContractValues.RequireText(acceptedPlus16EncoderId, nameof(acceptedPlus16EncoderId));
        SourceMesh = sourceMesh ?? throw new ArgumentNullException(nameof(sourceMesh));
    }

    public string RequestId { get; }

    public string ModDirectoryRoot { get; }

    public string ModDirectory { get; }

    public string MeshName { get; }

    public string AcceptedPlus16EncoderId { get; }

    public KandraFullSectionSourceMesh SourceMesh { get; }

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

public sealed class KandraFullSectionPayloadStreamSection
{
    public KandraFullSectionPayloadStreamSection(
        string sectionId,
        string streamName,
        long byteOffset,
        long byteCount,
        long elementCount,
        int elementByteSize)
    {
        SectionId = ContractValues.RequireText(sectionId, nameof(sectionId));
        StreamName = ContractValues.RequireText(streamName, nameof(streamName));
        if (byteOffset < 0) throw new ArgumentOutOfRangeException(nameof(byteOffset));
        if (byteCount < 0) throw new ArgumentOutOfRangeException(nameof(byteCount));
        if (elementCount < 0) throw new ArgumentOutOfRangeException(nameof(elementCount));
        if (elementByteSize <= 0) throw new ArgumentOutOfRangeException(nameof(elementByteSize));
        ByteOffset = byteOffset;
        ByteCount = byteCount;
        ElementCount = elementCount;
        ElementByteSize = elementByteSize;
    }

    public string SectionId { get; }

    public string StreamName { get; }

    public long ByteOffset { get; }

    public long ByteCount { get; }

    public long ElementCount { get; }

    public int ElementByteSize { get; }
}

public sealed class KandraFullSectionPayloadStreamMap
{
    private readonly KandraFullSectionPayloadStreamSection[] meshSections;

    private KandraFullSectionPayloadStreamMap(
        string layoutVersion,
        int vertexCount,
        int indexCount,
        int bindposesCount,
        int blendshapeCount,
        long meshDataByteCount,
        long indicesDataByteCount,
        IEnumerable<KandraFullSectionPayloadStreamSection> meshSections,
        KandraFullSectionPayloadStreamSection indicesSection)
    {
        LayoutVersion = ContractValues.RequireText(layoutVersion, nameof(layoutVersion));
        VertexCount = vertexCount;
        IndexCount = indexCount;
        BindposesCount = bindposesCount;
        BlendshapeCount = blendshapeCount;
        MeshDataByteCount = meshDataByteCount;
        IndicesDataByteCount = indicesDataByteCount;
        this.meshSections = (meshSections ?? throw new ArgumentNullException(nameof(meshSections))).ToArray();
        IndicesSection = indicesSection ?? throw new ArgumentNullException(nameof(indicesSection));
    }

    public string LayoutVersion { get; }

    public int VertexCount { get; }

    public int IndexCount { get; }

    public int BindposesCount { get; }

    public int BlendshapeCount { get; }

    public long MeshDataByteCount { get; }

    public long IndicesDataByteCount { get; }

    public int CompressedVertexPlus16ByteOffset => 16;

    public IReadOnlyList<KandraFullSectionPayloadStreamSection> MeshSections => meshSections;

    public KandraFullSectionPayloadStreamSection IndicesSection { get; }

    public static KandraFullSectionPayloadStreamMap Create(
        int vertexCount,
        int indexCount,
        int bindposesCount,
        int blendshapeCount)
    {
        if (vertexCount <= 0) throw new ArgumentOutOfRangeException(nameof(vertexCount));
        if (indexCount <= 0) throw new ArgumentOutOfRangeException(nameof(indexCount));
        if (bindposesCount < 0) throw new ArgumentOutOfRangeException(nameof(bindposesCount));
        if (blendshapeCount < 0) throw new ArgumentOutOfRangeException(nameof(blendshapeCount));

        checked
        {
            long offset = 0;
            var sections = new List<KandraFullSectionPayloadStreamSection>();
            AddMeshSection(sections, "compressed_vertices", ref offset, vertexCount, KandraRuntimePayloadLayout.CompressedVertexByteSize);
            AddMeshSection(sections, "additional_vertex_data", ref offset, vertexCount, KandraRuntimePayloadLayout.AdditionalVertexDataByteSize);
            AddMeshSection(sections, "packed_bones_weights", ref offset, vertexCount, KandraRuntimePayloadLayout.PackedBonesWeightsByteSize);
            AddMeshSection(sections, "bindposes_float3x4", ref offset, bindposesCount, KandraRuntimePayloadLayout.Float3x4ByteSize);
            AddMeshSection(
                sections,
                "packed_blendshape_data",
                ref offset,
                (long)blendshapeCount * vertexCount,
                KandraRuntimePayloadLayout.PackedBlendshapeDatumByteSize);

            long expectedMeshBytes = KandraRuntimePayloadLayout.ExpectedMeshPayloadByteCount(vertexCount, bindposesCount, blendshapeCount);
            if (offset != expectedMeshBytes)
            {
                throw new InvalidOperationException("kandra_full_section_stream_map_mesh_length_mismatch");
            }

            long indexBytes = KandraRuntimePayloadLayout.ExpectedIndexPayloadByteCount(indexCount);
            return new KandraFullSectionPayloadStreamMap(
                KandraRuntimePayloadLayout.LayoutVersion,
                vertexCount,
                indexCount,
                bindposesCount,
                blendshapeCount,
                expectedMeshBytes,
                indexBytes,
                sections,
                new KandraFullSectionPayloadStreamSection(
                    "indices_u16",
                    "ixkandra",
                    0,
                    indexBytes,
                    indexCount,
                    KandraRuntimePayloadLayout.IndexByteSize));
        }
    }

    private static void AddMeshSection(
        ICollection<KandraFullSectionPayloadStreamSection> sections,
        string sectionId,
        ref long offset,
        long elementCount,
        int elementByteSize)
    {
        long byteCount = elementCount * elementByteSize;
        sections.Add(new KandraFullSectionPayloadStreamSection(sectionId, "mdkandra", offset, byteCount, elementCount, elementByteSize));
        offset += byteCount;
    }
}

public sealed class KandraFullSectionPackageGenerationResult
{
    private readonly string[] blockers;
    private readonly string[] warnings;

    public KandraFullSectionPackageGenerationResult(
        string stageVersion,
        string requestId,
        string sourceMeshName,
        int vertexCount,
        int indexCount,
        int bindposesCount,
        int blendshapeCount,
        string plus16EncoderId,
        string layoutVersion,
        KandraMeshRegistrationMetadata? registrationMetadata,
        KandraFullSectionPayloadStreamMap? payloadStreamMap,
        bool sourceChannelsAccepted,
        bool fullSectionEncodingExecuted,
        KandraWriterResult? writerResult,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings)
    {
        StageVersion = ContractValues.RequireText(stageVersion, nameof(stageVersion));
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        SourceMeshName = ContractValues.RequireText(sourceMeshName, nameof(sourceMeshName));
        VertexCount = vertexCount;
        IndexCount = indexCount;
        BindposesCount = bindposesCount;
        BlendshapeCount = blendshapeCount;
        Plus16EncoderId = ContractValues.RequireText(plus16EncoderId, nameof(plus16EncoderId));
        LayoutVersion = ContractValues.RequireText(layoutVersion, nameof(layoutVersion));
        RegistrationMetadata = registrationMetadata;
        PayloadStreamMap = payloadStreamMap;
        SourceChannelsAccepted = sourceChannelsAccepted;
        FullSectionEncodingExecuted = fullSectionEncodingExecuted;
        WriterResult = writerResult;
        this.blockers = CopyMessages(blockers);
        this.warnings = CopyMessages(warnings);
    }

    public string StageVersion { get; }

    public string RequestId { get; }

    public string SourceMeshName { get; }

    public int VertexCount { get; }

    public int IndexCount { get; }

    public int BindposesCount { get; }

    public int BlendshapeCount { get; }

    public string Plus16EncoderId { get; }

    public string LayoutVersion { get; }

    public KandraMeshRegistrationMetadata? RegistrationMetadata { get; }

    public KandraFullSectionPayloadStreamMap? PayloadStreamMap { get; }

    public bool RegistrationMetadataGenerated => RegistrationMetadata != null;

    public bool PayloadStreamMapGenerated => PayloadStreamMap != null;

    public bool SourceChannelsAccepted { get; }

    public bool FullSectionEncodingExecuted { get; }

    public bool CandidatePackageGenerated => WriterResult != null;

    public bool ProductionConversionAccepted => false;

    public KandraWriterResult? WriterResult { get; }

    public bool CandidateMapApplicationAllowed => false;

    public bool CandidateMapApplicationExecuted => false;

    public bool ConversionExecuted => false;

    public bool RuntimeRegistrationExecuted => false;

    public bool ItemEquipMutationExecuted => false;

    public bool SaveMutationExecuted => false;

    public bool NativeGameWriteExecuted => false;

    public bool DownstreamWritesExecuted => false;

    public IReadOnlyList<string> Blockers => blockers;

    public IReadOnlyList<string> Warnings => warnings;

    private static string[] CopyMessages(IEnumerable<string> values) =>
        values?.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToArray()
        ?? Array.Empty<string>();
}

public sealed class KandraFullSectionPackageGenerationStage
{
    public const string StageVersion = "tainted-armour.kandra-full-section-package-generation.v1";
    public const string AcceptedRecoveredOctahedralRoundtripPlus16EncoderId = KandraSameMeshAbProofStage.RecoveredOctahedralRoundtripPlus16VariantId;

    public KandraFullSectionPackageGenerationResult Generate(KandraFullSectionPackageGenerationRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (!BitConverter.IsLittleEndian)
        {
            throw new PlatformNotSupportedException("Kandra package generation currently writes little-endian payloads.");
        }

        var fatalBlockers = new List<string>();
        var proofBlockers = new List<string>
        {
            "kandra_full_section_runtime_registration_proof_missing",
            "kandra_full_section_packed_weights_codec_requires_runtime_proof",
            "kandra_full_section_changed_topology_full_rebuild_requires_visual_proof",
        };
        var warnings = new List<string>
        {
            "kandra_full_section_candidate_package_generation_only:not_release_ready",
        };

        KandraFullSectionSourceMesh source = request.SourceMesh;
        if (!string.Equals(request.AcceptedPlus16EncoderId, AcceptedRecoveredOctahedralRoundtripPlus16EncoderId, StringComparison.Ordinal))
        {
            fatalBlockers.Add("kandra_full_section_plus16_encoder_not_accepted:" + request.AcceptedPlus16EncoderId);
        }

        if (source.BlendshapeCount != 0)
        {
            fatalBlockers.Add("kandra_full_section_blendshape_encoder_not_implemented");
        }

        bool sourceChannelsAccepted = fatalBlockers.Count == 0;
        KandraMeshRegistrationMetadata? registrationMetadata = null;
        KandraFullSectionPayloadStreamMap? payloadStreamMap = null;
        KandraWriterResult? writer = null;
        if (sourceChannelsAccepted)
        {
            payloadStreamMap = KandraFullSectionPayloadStreamMap.Create(
                source.VertexCount,
                source.IndexCount,
                source.BindposesCount,
                source.BlendshapeCount);
            registrationMetadata = new KandraMeshRegistrationMetadata(
                request.ModDirectory,
                request.MeshName,
                source.VertexCount,
                source.IndexCount,
                source.BindposesCount,
                source.BlendshapeCount,
                KandraRuntimePayloadLayout.LayoutVersion);
            KandraMeshPayloadSections payload = BuildPayload(source);
            writer = new KandraWriterStage().Write(
                new KandraWriterRequest(
                    request.RequestId + ".writer",
                    request.ModDirectoryRoot,
                    request.ModDirectory,
                    request.MeshName,
                    source.VertexCount,
                    source.BindposesCount,
                    payload,
                    source.UnsafeIndices));
        }

        return new KandraFullSectionPackageGenerationResult(
            StageVersion,
            request.RequestId,
            source.SourceMeshName,
            source.VertexCount,
            source.IndexCount,
            source.BindposesCount,
            source.BlendshapeCount,
            request.AcceptedPlus16EncoderId,
            KandraRuntimePayloadLayout.LayoutVersion,
            registrationMetadata,
            payloadStreamMap,
            sourceChannelsAccepted,
            writer != null,
            writer,
            fatalBlockers.Concat(proofBlockers),
            warnings);
    }

    private static KandraMeshPayloadSections BuildPayload(KandraFullSectionSourceMesh source)
    {
        byte[] compressedVertices = new byte[checked(source.VertexCount * KandraRuntimePayloadLayout.CompressedVertexByteSize)];
        byte[] additionalVertexData = new byte[checked(source.VertexCount * KandraRuntimePayloadLayout.AdditionalVertexDataByteSize)];
        byte[] packedBonesWeights = new byte[checked(source.VertexCount * KandraRuntimePayloadLayout.PackedBonesWeightsByteSize)];
        byte[] bindposes = new byte[checked(source.BindposesCount * KandraRuntimePayloadLayout.Float3x4ByteSize)];

        for (int vertexIndex = 0; vertexIndex < source.VertexCount; vertexIndex++)
        {
            KandraFullSectionVertex vertex = source.UnsafeVertices[vertexIndex];
            int compressedOffset = vertexIndex * KandraRuntimePayloadLayout.CompressedVertexByteSize;
            WriteSingle(compressedVertices, compressedOffset, (float)vertex.Position.X);
            WriteSingle(compressedVertices, compressedOffset + 4, (float)vertex.Position.Y);
            WriteSingle(compressedVertices, compressedOffset + 8, (float)vertex.Position.Z);
            WriteUInt32(compressedVertices, compressedOffset + 12, EncodeOctahedral(vertex.Normal));
            WriteUInt32(compressedVertices, compressedOffset + 16, EncodeOctahedral(vertex.Tangent));

            int additionalOffset = vertexIndex * KandraRuntimePayloadLayout.AdditionalVertexDataByteSize;
            WriteUInt32(additionalVertexData, additionalOffset, PackHalf2(vertex.Uv0X, vertex.Uv0Y));
            WriteSingle(additionalVertexData, additionalOffset + 4, vertex.TangentW);

            int weightsOffset = vertexIndex * KandraRuntimePayloadLayout.PackedBonesWeightsByteSize;
            KandraFullSectionSkinWeights weights = vertex.SkinWeights;
            WriteUInt16(packedBonesWeights, weightsOffset, weights.BoneIndex0);
            WriteUInt16(packedBonesWeights, weightsOffset + 2, weights.BoneIndex1);
            WriteUInt16(packedBonesWeights, weightsOffset + 4, weights.BoneIndex2);
            WriteUInt16(packedBonesWeights, weightsOffset + 6, weights.BoneIndex3);
            WriteUInt32(packedBonesWeights, weightsOffset + 8, PackWeights8(weights));
        }

        for (int bindposeIndex = 0; bindposeIndex < source.BindposesCount; bindposeIndex++)
        {
            float[] values = source.UnsafeBindposes[bindposeIndex].UnsafeValues;
            int bindposeOffset = bindposeIndex * KandraRuntimePayloadLayout.Float3x4ByteSize;
            for (int valueIndex = 0; valueIndex < values.Length; valueIndex++)
            {
                WriteSingle(bindposes, bindposeOffset + (valueIndex * sizeof(float)), values[valueIndex]);
            }
        }

        return new KandraMeshPayloadSections(
            compressedVertices,
            additionalVertexData,
            packedBonesWeights,
            bindposes,
            Array.Empty<KandraBlendshapePayload>());
    }

    private static uint PackHalf2(float x, float y) =>
        (uint)(SingleToHalf(x) | (SingleToHalf(y) << 16));

    private static uint PackWeights8(KandraFullSectionSkinWeights weights)
    {
        float sum = weights.Weight0 + weights.Weight1 + weights.Weight2 + weights.Weight3;
        if (sum <= 1e-10f)
        {
            return 0x000000ffu;
        }

        float normal0 = weights.Weight0 / sum;
        float normal1 = weights.Weight1 / sum;
        float normal2 = weights.Weight2 / sum;
        int byte0 = ClampByte((int)Math.Round(normal0 * 255f));
        int byte1 = ClampByte((int)Math.Round(normal1 * 255f));
        int byte2 = ClampByte((int)Math.Round(normal2 * 255f));
        int byte3 = ClampByte(255 - byte0 - byte1 - byte2);
        return (uint)(byte0 | (byte1 << 8) | (byte2 << 16) | (byte3 << 24));
    }

    private static int ClampByte(int value) =>
        value < 0 ? 0 : value > 255 ? 255 : value;

    private static uint EncodeOctahedral(ArmorVector3 vector)
    {
        Vec3 normal = new Vec3((float)vector.X, (float)vector.Y, (float)vector.Z).Normalized;
        float sum = Math.Abs(normal.X) + Math.Abs(normal.Y) + Math.Abs(normal.Z);
        if (sum < 1e-10f)
        {
            normal = Vec3.Up;
            sum = 1f;
        }

        normal = new Vec3(normal.X / sum, normal.Y / sum, normal.Z / sum);
        if (normal.Z < 0f)
        {
            normal = new Vec3(
                (1f - Math.Abs(normal.Y)) * (normal.X >= 0f ? 1f : -1f),
                (1f - Math.Abs(normal.X)) * (normal.Y >= 0f ? 1f : -1f),
                normal.Z);
        }

        return (uint)(SingleToHalf(normal.X * 0.5f + 0.5f) | (SingleToHalf(normal.Y * 0.5f + 0.5f) << 16));
    }

    private static ushort SingleToHalf(float value)
    {
        uint bits = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
        uint sign = (bits >> 16) & 0x8000u;
        int exponent = (int)((bits >> 23) & 0xffu) - 127 + 15;
        uint mantissa = bits & 0x7fffffu;

        if (exponent <= 0)
        {
            if (exponent < -10)
            {
                return (ushort)sign;
            }

            mantissa |= 0x800000u;
            int shift = 14 - exponent;
            uint halfMantissa = mantissa >> shift;
            if (((mantissa >> (shift - 1)) & 1u) != 0)
            {
                halfMantissa++;
            }

            return (ushort)(sign | halfMantissa);
        }

        if (exponent >= 31)
        {
            return (ushort)(sign | 0x7c00u);
        }

        uint roundedMantissa = mantissa + 0x1000u;
        if ((roundedMantissa & 0x800000u) != 0)
        {
            roundedMantissa = 0;
            exponent++;
            if (exponent >= 31)
            {
                return (ushort)(sign | 0x7c00u);
            }
        }

        return (ushort)(sign | ((uint)exponent << 10) | (roundedMantissa >> 13));
    }

    private static void WriteSingle(byte[] target, int offset, float value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        Buffer.BlockCopy(bytes, 0, target, offset, bytes.Length);
    }

    private static void WriteUInt16(byte[] target, int offset, ushort value)
    {
        target[offset] = (byte)(value & 0xff);
        target[offset + 1] = (byte)(value >> 8);
    }

    private static void WriteUInt32(byte[] target, int offset, uint value)
    {
        target[offset] = (byte)(value & 0xff);
        target[offset + 1] = (byte)((value >> 8) & 0xff);
        target[offset + 2] = (byte)((value >> 16) & 0xff);
        target[offset + 3] = (byte)((value >> 24) & 0xff);
    }

    private readonly struct Vec3
    {
        public static readonly Vec3 Up = new Vec3(0f, 1f, 0f);

        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }

        public float Y { get; }

        public float Z { get; }

        private float SqrMagnitude => (X * X) + (Y * Y) + (Z * Z);

        private float Magnitude => (float)Math.Sqrt(SqrMagnitude);

        public Vec3 Normalized
        {
            get
            {
                float magnitude = Magnitude;
                return magnitude > 1e-10f ? new Vec3(X / magnitude, Y / magnitude, Z / magnitude) : Up;
            }
        }
    }
}
