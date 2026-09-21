using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Tainted.Armour;

public sealed class KandraSameMeshAbProofSourceSnapshot
{
    private readonly byte[] meshData;
    private readonly ushort[] indices;
    private readonly string[] blendshapeNames;

    public KandraSameMeshAbProofSourceSnapshot(
        string sourceMeshName,
        int vertexCount,
        int bindposesCount,
        IEnumerable<string>? blendshapeNames,
        byte[] meshData,
        IEnumerable<ushort> indices)
    {
        SourceMeshName = ContractValues.RequireText(sourceMeshName, nameof(sourceMeshName));
        if (vertexCount <= 0 || vertexCount > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(vertexCount));
        if (bindposesCount < 0 || bindposesCount > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(bindposesCount));
        VertexCount = vertexCount;
        BindposesCount = bindposesCount;
        this.blendshapeNames = (blendshapeNames ?? Enumerable.Empty<string>())
            .Select((value, index) => string.IsNullOrWhiteSpace(value) ? "blendshape_" + index.ToString("0000", CultureInfo.InvariantCulture) : value.Trim())
            .ToArray();
        this.meshData = CopyBytes(meshData, nameof(meshData));
        this.indices = (indices ?? throw new ArgumentNullException(nameof(indices))).ToArray();
        if (this.indices.Length == 0) throw new ArgumentException("At least one index is required.", nameof(indices));

        long expectedMeshBytes = KandraRuntimePayloadLayout.ExpectedMeshPayloadByteCount(VertexCount, BindposesCount, BlendshapeCount);
        if (this.meshData.LongLength != expectedMeshBytes)
        {
            throw new ArgumentException(
                "same_mesh_ab_source_mesh_length_mismatch:expected=" +
                expectedMeshBytes.ToString(CultureInfo.InvariantCulture) +
                ":actual=" +
                this.meshData.LongLength.ToString(CultureInfo.InvariantCulture),
                nameof(meshData));
        }
    }

    public string SourceMeshName { get; }

    public int VertexCount { get; }

    public int BindposesCount { get; }

    public int BlendshapeCount => blendshapeNames.Length;

    public IReadOnlyList<string> BlendshapeNames => blendshapeNames;

    public byte[] MeshData => CopyBytes(meshData, nameof(MeshData));

    public IReadOnlyList<ushort> Indices => indices;

    internal byte[] UnsafeMeshData => meshData;

    internal ushort[] UnsafeIndices => indices;

    internal IReadOnlyList<string> UnsafeBlendshapeNames => blendshapeNames;

    private static byte[] CopyBytes(byte[] value, string parameterName) =>
        value == null ? throw new ArgumentNullException(parameterName) : value.ToArray();
}

public sealed class KandraSameMeshAbProofRequest
{
    public KandraSameMeshAbProofRequest(
        string requestId,
        string modDirectoryRoot,
        string modDirectory,
        string meshNamePrefix,
        string approvalId,
        KandraSameMeshAbProofSourceSnapshot sourceSnapshot,
        KandraRuntimeRegistrationContractFingerprint contractFingerprint)
    {
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        ModDirectoryRoot = Path.GetFullPath(ContractValues.RequireText(modDirectoryRoot, nameof(modDirectoryRoot)));
        ModDirectory = RequirePathSegment(modDirectory, nameof(modDirectory));
        MeshNamePrefix = RequireFileStem(meshNamePrefix, nameof(meshNamePrefix));
        ApprovalId = ContractValues.RequireText(approvalId, nameof(approvalId));
        SourceSnapshot = sourceSnapshot ?? throw new ArgumentNullException(nameof(sourceSnapshot));
        ContractFingerprint = contractFingerprint ?? throw new ArgumentNullException(nameof(contractFingerprint));
    }

    public string RequestId { get; }

    public string ModDirectoryRoot { get; }

    public string ModDirectory { get; }

    public string MeshNamePrefix { get; }

    public string ApprovalId { get; }

    public KandraSameMeshAbProofSourceSnapshot SourceSnapshot { get; }

    public KandraRuntimeRegistrationContractFingerprint ContractFingerprint { get; }

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

public sealed class KandraSameMeshAbProofVariantResult
{
    private readonly string[] blockers;
    private readonly string[] warnings;

    public KandraSameMeshAbProofVariantResult(
        string variantId,
        string variantLabel,
        string meshName,
        KandraWriterResult writerResult,
        KandraRuntimeRegistrationInvocationContext invocationContext,
        bool sourceMeshPayloadPreservedExactly,
        bool sourceIndicesPayloadPreservedExactly,
        bool onlyCompressedVertexPlus16Changed,
        int changedByteCount,
        int changedCompressedVertexPlus16ByteCount,
        int changedNonPlus16ByteCount,
        int changedPlus16FieldCount,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings)
    {
        VariantId = ContractValues.RequireText(variantId, nameof(variantId));
        VariantLabel = ContractValues.RequireText(variantLabel, nameof(variantLabel));
        MeshName = ContractValues.RequireText(meshName, nameof(meshName));
        WriterResult = writerResult ?? throw new ArgumentNullException(nameof(writerResult));
        InvocationContext = invocationContext ?? throw new ArgumentNullException(nameof(invocationContext));
        SourceMeshPayloadPreservedExactly = sourceMeshPayloadPreservedExactly;
        SourceIndicesPayloadPreservedExactly = sourceIndicesPayloadPreservedExactly;
        OnlyCompressedVertexPlus16Changed = onlyCompressedVertexPlus16Changed;
        ChangedByteCount = changedByteCount;
        ChangedCompressedVertexPlus16ByteCount = changedCompressedVertexPlus16ByteCount;
        ChangedNonPlus16ByteCount = changedNonPlus16ByteCount;
        ChangedPlus16FieldCount = changedPlus16FieldCount;
        this.blockers = CopyMessages(blockers);
        this.warnings = CopyMessages(warnings);
    }

    public string VariantId { get; }

    public string VariantLabel { get; }

    public string MeshName { get; }

    public KandraWriterResult WriterResult { get; }

    public KandraRuntimeRegistrationInvocationContext InvocationContext { get; }

    public bool SourceMeshPayloadPreservedExactly { get; }

    public bool SourceIndicesPayloadPreservedExactly { get; }

    public bool OnlyCompressedVertexPlus16Changed { get; }

    public int ChangedByteCount { get; }

    public int ChangedCompressedVertexPlus16ByteCount { get; }

    public int ChangedNonPlus16ByteCount { get; }

    public int ChangedPlus16FieldCount { get; }

    public bool VariantAccepted => blockers.Length == 0;

    public bool CandidateMapApplicationAllowed => false;

    public bool CandidateMapApplicationExecuted => false;

    public bool ConversionAllowed => false;

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

public sealed class KandraSameMeshAbProofResult
{
    private readonly KandraSameMeshAbProofVariantResult[] variants;
    private readonly string[] blockers;
    private readonly string[] warnings;

    public KandraSameMeshAbProofResult(
        string stageVersion,
        string requestId,
        string layoutVersion,
        string sourceMeshName,
        int vertexCount,
        int indicesCount,
        int bindposesCount,
        int blendshapeCount,
        bool sourcePayloadLengthAccepted,
        bool contractFingerprintAccepted,
        bool contractSurfacePresent,
        bool contractNoWriteBoundaryFalse,
        IEnumerable<KandraSameMeshAbProofVariantResult> variants,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings)
    {
        StageVersion = ContractValues.RequireText(stageVersion, nameof(stageVersion));
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        LayoutVersion = ContractValues.RequireText(layoutVersion, nameof(layoutVersion));
        SourceMeshName = ContractValues.RequireText(sourceMeshName, nameof(sourceMeshName));
        VertexCount = vertexCount;
        IndicesCount = indicesCount;
        BindposesCount = bindposesCount;
        BlendshapeCount = blendshapeCount;
        SourcePayloadLengthAccepted = sourcePayloadLengthAccepted;
        ContractFingerprintAccepted = contractFingerprintAccepted;
        ContractSurfacePresent = contractSurfacePresent;
        ContractNoWriteBoundaryFalse = contractNoWriteBoundaryFalse;
        this.variants = (variants ?? Enumerable.Empty<KandraSameMeshAbProofVariantResult>()).ToArray();
        this.blockers = CopyMessages(blockers);
        this.warnings = CopyMessages(warnings);
    }

    public string StageVersion { get; }

    public string RequestId { get; }

    public string LayoutVersion { get; }

    public string SourceMeshName { get; }

    public int VertexCount { get; }

    public int IndicesCount { get; }

    public int BindposesCount { get; }

    public int BlendshapeCount { get; }

    public bool SourcePayloadLengthAccepted { get; }

    public bool ContractFingerprintAccepted { get; }

    public bool ContractSurfacePresent { get; }

    public bool ContractNoWriteBoundaryFalse { get; }

    public IReadOnlyList<KandraSameMeshAbProofVariantResult> Variants => variants;

    public bool ProofPackageSetAccepted => blockers.Length == 0 && variants.Length == 4 && variants.All(variant => variant.VariantAccepted);

    public bool CandidateMapApplicationAllowed => false;

    public bool CandidateMapApplicationExecuted => false;

    public bool ConversionAllowed => false;

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

public sealed class KandraSameMeshAbProofStage
{
    public const string StageVersion = "tainted-armour.kandra-same-mesh-ab-proof.v1";
    public const string OriginalBytesVariantId = "original_bytes";
    public const string BarGeometricPlus16VariantId = "bar_geometric_plus16";
    public const string DuplicateNormalPlus16VariantId = "duplicate_normal_plus16";
    public const string RecoveredOctahedralRoundtripPlus16VariantId = "recovered_octahedral_roundtrip_plus16";

    public KandraSameMeshAbProofResult Build(KandraSameMeshAbProofRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var blockers = new List<string>();
        var warnings = new List<string>();
        KandraSameMeshAbProofSourceSnapshot source = request.SourceSnapshot;

        long expectedMeshBytes = KandraRuntimePayloadLayout.ExpectedMeshPayloadByteCount(source.VertexCount, source.BindposesCount, source.BlendshapeCount);
        bool sourcePayloadLengthAccepted = source.UnsafeMeshData.LongLength == expectedMeshBytes;
        if (!sourcePayloadLengthAccepted)
        {
            blockers.Add("kandra_same_mesh_ab_source_payload_length_mismatch");
        }

        bool contractFingerprintAccepted = request.ContractFingerprint.SurfaceFingerprintMatchesExpected;
        if (!contractFingerprintAccepted)
        {
            blockers.Add("kandra_same_mesh_ab_contract_fingerprint_not_accepted");
        }

        bool contractSurfacePresent = request.ContractFingerprint.RequiredSurfacePresent;
        if (!contractSurfacePresent)
        {
            blockers.Add("kandra_same_mesh_ab_contract_surface_incomplete");
        }

        bool contractNoWriteBoundaryFalse = request.ContractFingerprint.NoWriteBoundaryFalse;
        if (!contractNoWriteBoundaryFalse)
        {
            blockers.Add("kandra_same_mesh_ab_contract_no_write_boundary_not_false");
        }

        var variants = new List<KandraSameMeshAbProofVariantResult>();
        if (blockers.Count == 0)
        {
            SourceSections sections = SplitSourceSections(source);
            variants.Add(WriteVariant(request, source, OriginalBytesVariantId, "Original runtime bytes", sections, sections.CompressedVertices));
            variants.Add(WriteVariant(request, source, BarGeometricPlus16VariantId, "BAR geometric +16", sections, BuildBarGeometricPlus16(source, sections, warnings)));
            variants.Add(WriteVariant(request, source, DuplicateNormalPlus16VariantId, "Duplicate normal +16", sections, BuildDuplicateNormalPlus16(source, sections)));
            variants.Add(WriteVariant(request, source, RecoveredOctahedralRoundtripPlus16VariantId, "Recovered octahedral decode/encode +16", sections, BuildRecoveredRoundtripPlus16(source, sections)));
        }

        return new KandraSameMeshAbProofResult(
            StageVersion,
            request.RequestId,
            KandraRuntimePayloadLayout.LayoutVersion,
            source.SourceMeshName,
            source.VertexCount,
            source.UnsafeIndices.Length,
            source.BindposesCount,
            source.BlendshapeCount,
            sourcePayloadLengthAccepted,
            contractFingerprintAccepted,
            contractSurfacePresent,
            contractNoWriteBoundaryFalse,
            variants,
            blockers,
            warnings);
    }

    private static KandraSameMeshAbProofVariantResult WriteVariant(
        KandraSameMeshAbProofRequest request,
        KandraSameMeshAbProofSourceSnapshot source,
        string variantId,
        string variantLabel,
        SourceSections sourceSections,
        byte[] compressedVertices)
    {
        string meshName = request.MeshNamePrefix + "_" + variantId;
        var payload = new KandraMeshPayloadSections(
            compressedVertices,
            sourceSections.AdditionalVertexData,
            sourceSections.PackedBonesWeights,
            sourceSections.BindposesFloat3x4,
            sourceSections.Blendshapes);
        var writer = new KandraWriterStage().Write(
            new KandraWriterRequest(
                request.RequestId + "." + variantId + ".writer",
                request.ModDirectoryRoot,
                request.ModDirectory,
                meshName,
                source.VertexCount,
                source.BindposesCount,
                payload,
                source.UnsafeIndices));

        byte[] writtenMesh = File.ReadAllBytes(writer.MeshDataPath);
        byte[] writtenIndices = File.ReadAllBytes(writer.IndicesDataPath);
        byte[] sourceIndexBytes = BuildIndexBytes(source.UnsafeIndices);
        ChangeSummary changes = SummarizeChanges(source.UnsafeMeshData, writtenMesh, source.VertexCount);
        bool sourceMeshPayloadPreservedExactly = writtenMesh.SequenceEqual(source.UnsafeMeshData);
        bool sourceIndicesPayloadPreservedExactly = writtenIndices.SequenceEqual(sourceIndexBytes);

        var blockers = new List<string>();
        var warnings = new List<string>();
        if (!sourceIndicesPayloadPreservedExactly)
        {
            blockers.Add("kandra_same_mesh_ab_indices_payload_changed:" + variantId);
        }

        if (variantId == OriginalBytesVariantId && !sourceMeshPayloadPreservedExactly)
        {
            blockers.Add("kandra_same_mesh_ab_original_payload_not_preserved");
        }

        if (variantId != OriginalBytesVariantId && !changes.OnlyCompressedVertexPlus16Changed)
        {
            blockers.Add("kandra_same_mesh_ab_variant_changed_non_plus16_bytes:" + variantId);
        }

        KandraRuntimeRegistrationInvocationContext context = new KandraRuntimeRegistrationInvocationContext(
            request.RequestId + "." + variantId + ".runtime-context",
            request.RequestId + "." + variantId + ".same-mesh-ab-proof",
            request.ApprovalId,
            writer.ModDirectory,
            writer.MeshName,
            source.VertexCount,
            source.UnsafeIndices.Length,
            source.BindposesCount,
            source.BlendshapeCount,
            writer.LayoutVersion,
            writer.KandraDirectory,
            writer.MeshDataPath,
            writer.IndicesDataPath,
            writer.MeshDataByteCount,
            writer.IndicesDataByteCount,
            writer.MeshDataSha256,
            writer.IndicesDataSha256,
            KandraRuntimeRegistrationContractFingerprint.ExpectedManagerType,
            KandraRuntimeRegistrationContractFingerprint.ExpectedRegistrationMethodSignature,
            KandraRuntimeRegistrationContractFingerprint.ExpectedRegistrationMethodFingerprint,
            KandraRuntimeRegistrationContractFingerprint.ExpectedAssemblySha256);

        return new KandraSameMeshAbProofVariantResult(
            variantId,
            variantLabel,
            writer.MeshName,
            writer,
            context,
            sourceMeshPayloadPreservedExactly,
            sourceIndicesPayloadPreservedExactly,
            changes.OnlyCompressedVertexPlus16Changed,
            changes.ChangedByteCount,
            changes.ChangedCompressedVertexPlus16ByteCount,
            changes.ChangedNonPlus16ByteCount,
            changes.ChangedPlus16FieldCount,
            blockers,
            warnings);
    }

    private static SourceSections SplitSourceSections(KandraSameMeshAbProofSourceSnapshot source)
    {
        int offset = 0;
        int compressedLength = checked(source.VertexCount * KandraRuntimePayloadLayout.CompressedVertexByteSize);
        int additionalLength = checked(source.VertexCount * KandraRuntimePayloadLayout.AdditionalVertexDataByteSize);
        int weightsLength = checked(source.VertexCount * KandraRuntimePayloadLayout.PackedBonesWeightsByteSize);
        int bindposesLength = checked(source.BindposesCount * KandraRuntimePayloadLayout.Float3x4ByteSize);
        byte[] compressed = Slice(source.UnsafeMeshData, offset, compressedLength);
        offset += compressedLength;
        byte[] additional = Slice(source.UnsafeMeshData, offset, additionalLength);
        offset += additionalLength;
        byte[] weights = Slice(source.UnsafeMeshData, offset, weightsLength);
        offset += weightsLength;
        byte[] bindposes = Slice(source.UnsafeMeshData, offset, bindposesLength);
        offset += bindposesLength;

        var blendshapes = new List<KandraBlendshapePayload>();
        int blendshapeLength = checked(source.VertexCount * KandraRuntimePayloadLayout.PackedBlendshapeDatumByteSize);
        for (int index = 0; index < source.BlendshapeCount; index++)
        {
            string name = source.UnsafeBlendshapeNames[index];
            blendshapes.Add(new KandraBlendshapePayload(name, Slice(source.UnsafeMeshData, offset, blendshapeLength)));
            offset += blendshapeLength;
        }

        return new SourceSections(compressed, additional, weights, bindposes, blendshapes);
    }

    private static byte[] BuildDuplicateNormalPlus16(KandraSameMeshAbProofSourceSnapshot source, SourceSections sections)
    {
        byte[] compressed = sections.CompressedVertices.ToArray();
        for (int vertex = 0; vertex < source.VertexCount; vertex++)
        {
            int offset = vertex * KandraRuntimePayloadLayout.CompressedVertexByteSize;
            CopyUInt32(compressed, offset + 12, offset + 16);
        }

        return compressed;
    }

    private static byte[] BuildRecoveredRoundtripPlus16(KandraSameMeshAbProofSourceSnapshot source, SourceSections sections)
    {
        byte[] compressed = sections.CompressedVertices.ToArray();
        for (int vertex = 0; vertex < source.VertexCount; vertex++)
        {
            int offset = vertex * KandraRuntimePayloadLayout.CompressedVertexByteSize;
            uint encoded = ReadUInt32(compressed, offset + 16);
            uint reencoded = EncodeOctahedral(DecodeOctahedral(encoded));
            WriteUInt32(compressed, offset + 16, reencoded);
        }

        return compressed;
    }

    private static byte[] BuildBarGeometricPlus16(
        KandraSameMeshAbProofSourceSnapshot source,
        SourceSections sections,
        List<string> warnings)
    {
        Vec3[] positions = new Vec3[source.VertexCount];
        Vec3[] normals = new Vec3[source.VertexCount];
        Vec2[] uvs = new Vec2[source.VertexCount];
        for (int vertex = 0; vertex < source.VertexCount; vertex++)
        {
            int compressedOffset = vertex * KandraRuntimePayloadLayout.CompressedVertexByteSize;
            positions[vertex] = new Vec3(
                BitConverter.ToSingle(sections.CompressedVertices, compressedOffset),
                BitConverter.ToSingle(sections.CompressedVertices, compressedOffset + 4),
                BitConverter.ToSingle(sections.CompressedVertices, compressedOffset + 8));
            normals[vertex] = DecodeOctahedral(ReadUInt32(sections.CompressedVertices, compressedOffset + 12));

            int additionalOffset = vertex * KandraRuntimePayloadLayout.AdditionalVertexDataByteSize;
            uint packedUv = ReadUInt32(sections.AdditionalVertexData, additionalOffset);
            uvs[vertex] = new Vec2(HalfToSingle((ushort)(packedUv & 0xFFFFu)), HalfToSingle((ushort)(packedUv >> 16)));
        }

        Vec3[] tangents = ComputeBarTangents(positions, normals, uvs, source.UnsafeIndices, warnings);
        byte[] compressed = sections.CompressedVertices.ToArray();
        for (int vertex = 0; vertex < source.VertexCount; vertex++)
        {
            int offset = vertex * KandraRuntimePayloadLayout.CompressedVertexByteSize;
            WriteUInt32(compressed, offset + 16, EncodeOctahedral(tangents[vertex]));
        }

        return compressed;
    }

    private static Vec3[] ComputeBarTangents(
        Vec3[] positions,
        Vec3[] normals,
        Vec2[] uvs,
        ushort[] indices,
        List<string> warnings)
    {
        int vertexCount = positions.Length;
        int triangleCount = indices.Length / 3;
        var tan1 = new Vec3[vertexCount];
        var tan2 = new Vec3[vertexCount];

        for (int triangle = 0; triangle < triangleCount; triangle++)
        {
            int i0 = indices[triangle * 3];
            int i1 = indices[triangle * 3 + 1];
            int i2 = indices[triangle * 3 + 2];
            if (i0 >= vertexCount || i1 >= vertexCount || i2 >= vertexCount)
            {
                continue;
            }

            Vec3 v0 = positions[i0];
            Vec3 v1 = positions[i1];
            Vec3 v2 = positions[i2];
            Vec2 uv0 = uvs[i0];
            Vec2 uv1 = uvs[i1];
            Vec2 uv2 = uvs[i2];
            Vec3 deltaPos1 = v1 - v0;
            Vec3 deltaPos2 = v2 - v0;
            Vec2 deltaUv1 = uv1 - uv0;
            Vec2 deltaUv2 = uv2 - uv0;
            float determinant = deltaUv1.X * deltaUv2.Y - deltaUv2.X * deltaUv1.Y;
            if (Math.Abs(determinant) < 1e-12f)
            {
                continue;
            }

            float reciprocal = 1.0f / determinant;
            var sdir = new Vec3(
                (deltaPos1.X * deltaUv2.Y - deltaPos2.X * deltaUv1.Y) * reciprocal,
                (deltaPos1.Y * deltaUv2.Y - deltaPos2.Y * deltaUv1.Y) * reciprocal,
                (deltaPos1.Z * deltaUv2.Y - deltaPos2.Z * deltaUv1.Y) * reciprocal);
            var tdir = new Vec3(
                (deltaPos2.X * deltaUv1.X - deltaPos1.X * deltaUv2.X) * reciprocal,
                (deltaPos2.Y * deltaUv1.X - deltaPos1.Y * deltaUv2.X) * reciprocal,
                (deltaPos2.Z * deltaUv1.X - deltaPos1.Z * deltaUv2.X) * reciprocal);

            float sdirLength = sdir.Magnitude;
            float tdirLength = tdir.Magnitude;
            if (sdirLength < 1e-10f || tdirLength < 1e-10f)
            {
                continue;
            }

            sdir /= sdirLength;
            tdir /= tdirLength;

            float w0 = CornerAngleWeight(v1 - v0, v2 - v0);
            float w1 = CornerAngleWeight(v2 - v1, v0 - v1);
            float w2 = CornerAngleWeight(v0 - v2, v1 - v2);

            tan1[i0] += sdir * w0;
            tan1[i1] += sdir * w1;
            tan1[i2] += sdir * w2;
            tan2[i0] += tdir * w0;
            tan2[i1] += tdir * w1;
            tan2[i2] += tdir * w2;
        }

        var degenerate = new bool[vertexCount];
        for (int index = 0; index < vertexCount; index++)
        {
            degenerate[index] = tan1[index].SqrMagnitude < 1e-10f || tan2[index].SqrMagnitude < 1e-10f;
        }

        if (degenerate.Any(value => value))
        {
            var neighbours = new List<int>[vertexCount];
            for (int index = 0; index < vertexCount; index++)
            {
                neighbours[index] = new List<int>(4);
            }

            for (int triangle = 0; triangle < triangleCount; triangle++)
            {
                int i0 = indices[triangle * 3];
                int i1 = indices[triangle * 3 + 1];
                int i2 = indices[triangle * 3 + 2];
                if (i0 >= vertexCount || i1 >= vertexCount || i2 >= vertexCount)
                {
                    continue;
                }

                neighbours[i0].Add(i1);
                neighbours[i0].Add(i2);
                neighbours[i1].Add(i0);
                neighbours[i1].Add(i2);
                neighbours[i2].Add(i0);
                neighbours[i2].Add(i1);
            }

            bool improved = true;
            while (improved)
            {
                improved = false;
                for (int index = 0; index < vertexCount; index++)
                {
                    if (!degenerate[index])
                    {
                        continue;
                    }

                    Vec3 sum1 = Vec3.Zero;
                    Vec3 sum2 = Vec3.Zero;
                    int found = 0;
                    foreach (int neighbour in neighbours[index])
                    {
                        if (neighbour < degenerate.Length && !degenerate[neighbour])
                        {
                            sum1 += tan1[neighbour];
                            sum2 += tan2[neighbour];
                            found++;
                        }
                    }

                    if (found > 0)
                    {
                        tan1[index] = sum1 / found;
                        tan2[index] = sum2 / found;
                        if (tan1[index].SqrMagnitude >= 1e-10f && tan2[index].SqrMagnitude >= 1e-10f)
                        {
                            degenerate[index] = false;
                            improved = true;
                        }
                    }
                }
            }
        }

        int unrecovered = 0;
        var tangents = new Vec3[vertexCount];
        for (int index = 0; index < vertexCount; index++)
        {
            Vec3 normal = index < normals.Length ? normals[index] : Vec3.Up;
            Vec3 tangent = tan1[index];
            if (degenerate[index])
            {
                tangent = Math.Abs(normal.Y) < 0.99f
                    ? Vec3.Cross(normal, Vec3.Up).Normalized
                    : Vec3.Cross(normal, Vec3.Right).Normalized;
                unrecovered++;
            }
            else
            {
                tangent = tangent - normal * Vec3.Dot(normal, tangent);
                tangent = tangent.SqrMagnitude > 1e-12f
                    ? tangent.Normalized
                    : (Math.Abs(normal.Y) < 0.99f
                        ? Vec3.Cross(normal, Vec3.Up).Normalized
                        : Vec3.Cross(normal, Vec3.Right).Normalized);
            }

            tangents[index] = tangent;
        }

        if (unrecovered > 0)
        {
            warnings.Add(
                "kandra_same_mesh_ab_bar_geometric_plus16_unrecovered_tangents:" +
                unrecovered.ToString(CultureInfo.InvariantCulture) +
                "/" +
                vertexCount.ToString(CultureInfo.InvariantCulture));
        }

        return tangents;
    }

    private static float CornerAngleWeight(Vec3 a, Vec3 b)
    {
        float denominator = a.Magnitude * b.Magnitude;
        if (denominator < 1e-12f)
        {
            return 0f;
        }

        return Vec3.Cross(a, b).Magnitude / denominator;
    }

    private static Vec3 DecodeOctahedral(uint encoded)
    {
        float x = HalfToSingle((ushort)(encoded & 0xFFFFu)) * 2f - 1f;
        float y = HalfToSingle((ushort)(encoded >> 16)) * 2f - 1f;
        var normal = new Vec3(x, y, 1f - Math.Abs(x) - Math.Abs(y));
        if (normal.Z < 0f)
        {
            float wrappedX = (1f - Math.Abs(normal.Y)) * (normal.X >= 0f ? 1f : -1f);
            float wrappedY = (1f - Math.Abs(normal.X)) * (normal.Y >= 0f ? 1f : -1f);
            normal = new Vec3(wrappedX, wrappedY, normal.Z);
        }

        return normal.Normalized;
    }

    private static uint EncodeOctahedral(Vec3 vector)
    {
        Vec3 normal = vector.Normalized;
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

        ushort x = SingleToHalf(normal.X * 0.5f + 0.5f);
        ushort y = SingleToHalf(normal.Y * 0.5f + 0.5f);
        return (uint)(x | (y << 16));
    }

    private static float HalfToSingle(ushort value)
    {
        int sign = (value >> 15) & 0x00000001;
        int exponent = (value >> 10) & 0x0000001f;
        int mantissa = value & 0x000003ff;

        if (exponent == 0)
        {
            if (mantissa == 0)
            {
                return sign == 0 ? 0f : -0f;
            }

            float subnormal = mantissa / 1024f;
            float result = (float)Math.Pow(2, -14) * subnormal;
            return sign == 0 ? result : -result;
        }

        if (exponent == 31)
        {
            if (mantissa == 0)
            {
                return sign == 0 ? float.PositiveInfinity : float.NegativeInfinity;
            }

            return float.NaN;
        }

        float normal = (1f + mantissa / 1024f) * (float)Math.Pow(2, exponent - 15);
        return sign == 0 ? normal : -normal;
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

    private static ChangeSummary SummarizeChanges(byte[] source, byte[] candidate, int vertexCount)
    {
        if (source.Length != candidate.Length)
        {
            return new ChangeSummary(source.Length + candidate.Length, 0, source.Length + candidate.Length, 0, false);
        }

        int changed = 0;
        int changedPlus16Bytes = 0;
        int changedNonPlus16Bytes = 0;
        bool[] changedPlus16Fields = new bool[vertexCount];
        for (int index = 0; index < source.Length; index++)
        {
            if (source[index] == candidate[index])
            {
                continue;
            }

            changed++;
            if (IsCompressedVertexPlus16Byte(index, vertexCount))
            {
                changedPlus16Bytes++;
                changedPlus16Fields[index / KandraRuntimePayloadLayout.CompressedVertexByteSize] = true;
            }
            else
            {
                changedNonPlus16Bytes++;
            }
        }

        return new ChangeSummary(
            changed,
            changedPlus16Bytes,
            changedNonPlus16Bytes,
            changedPlus16Fields.Count(value => value),
            changedNonPlus16Bytes == 0);
    }

    private static bool IsCompressedVertexPlus16Byte(int byteIndex, int vertexCount)
    {
        int compressedLength = vertexCount * KandraRuntimePayloadLayout.CompressedVertexByteSize;
        if (byteIndex < 0 || byteIndex >= compressedLength)
        {
            return false;
        }

        int offset = byteIndex % KandraRuntimePayloadLayout.CompressedVertexByteSize;
        return offset >= 16 && offset <= 19;
    }

    private static byte[] BuildIndexBytes(ushort[] indices)
    {
        byte[] bytes = new byte[checked(indices.Length * KandraRuntimePayloadLayout.IndexByteSize)];
        for (int index = 0; index < indices.Length; index++)
        {
            int offset = index * 2;
            bytes[offset] = (byte)(indices[index] & 0xff);
            bytes[offset + 1] = (byte)(indices[index] >> 8);
        }

        return bytes;
    }

    private static byte[] Slice(byte[] source, int offset, int length)
    {
        byte[] result = new byte[length];
        Buffer.BlockCopy(source, offset, result, 0, length);
        return result;
    }

    private static uint ReadUInt32(byte[] source, int offset) =>
        (uint)(source[offset] | (source[offset + 1] << 8) | (source[offset + 2] << 16) | (source[offset + 3] << 24));

    private static void WriteUInt32(byte[] target, int offset, uint value)
    {
        target[offset] = (byte)(value & 0xff);
        target[offset + 1] = (byte)((value >> 8) & 0xff);
        target[offset + 2] = (byte)((value >> 16) & 0xff);
        target[offset + 3] = (byte)((value >> 24) & 0xff);
    }

    private static void CopyUInt32(byte[] target, int sourceOffset, int targetOffset)
    {
        target[targetOffset] = target[sourceOffset];
        target[targetOffset + 1] = target[sourceOffset + 1];
        target[targetOffset + 2] = target[sourceOffset + 2];
        target[targetOffset + 3] = target[sourceOffset + 3];
    }

    private sealed class SourceSections
    {
        public SourceSections(
            byte[] compressedVertices,
            byte[] additionalVertexData,
            byte[] packedBonesWeights,
            byte[] bindposesFloat3x4,
            IReadOnlyList<KandraBlendshapePayload> blendshapes)
        {
            CompressedVertices = compressedVertices;
            AdditionalVertexData = additionalVertexData;
            PackedBonesWeights = packedBonesWeights;
            BindposesFloat3x4 = bindposesFloat3x4;
            Blendshapes = blendshapes;
        }

        public byte[] CompressedVertices { get; }

        public byte[] AdditionalVertexData { get; }

        public byte[] PackedBonesWeights { get; }

        public byte[] BindposesFloat3x4 { get; }

        public IReadOnlyList<KandraBlendshapePayload> Blendshapes { get; }
    }

    private readonly struct ChangeSummary
    {
        public ChangeSummary(
            int changedByteCount,
            int changedCompressedVertexPlus16ByteCount,
            int changedNonPlus16ByteCount,
            int changedPlus16FieldCount,
            bool onlyCompressedVertexPlus16Changed)
        {
            ChangedByteCount = changedByteCount;
            ChangedCompressedVertexPlus16ByteCount = changedCompressedVertexPlus16ByteCount;
            ChangedNonPlus16ByteCount = changedNonPlus16ByteCount;
            ChangedPlus16FieldCount = changedPlus16FieldCount;
            OnlyCompressedVertexPlus16Changed = onlyCompressedVertexPlus16Changed;
        }

        public int ChangedByteCount { get; }

        public int ChangedCompressedVertexPlus16ByteCount { get; }

        public int ChangedNonPlus16ByteCount { get; }

        public int ChangedPlus16FieldCount { get; }

        public bool OnlyCompressedVertexPlus16Changed { get; }
    }

    private readonly struct Vec2
    {
        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; }

        public float Y { get; }

        public static Vec2 operator -(Vec2 left, Vec2 right) =>
            new Vec2(left.X - right.X, left.Y - right.Y);
    }

    private readonly struct Vec3
    {
        public static readonly Vec3 Zero = new Vec3(0f, 0f, 0f);
        public static readonly Vec3 Up = new Vec3(0f, 1f, 0f);
        public static readonly Vec3 Right = new Vec3(1f, 0f, 0f);

        public Vec3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }

        public float Y { get; }

        public float Z { get; }

        public float SqrMagnitude => X * X + Y * Y + Z * Z;

        public float Magnitude => (float)Math.Sqrt(SqrMagnitude);

        public Vec3 Normalized
        {
            get
            {
                float magnitude = Magnitude;
                return magnitude > 1e-10f ? this / magnitude : Zero;
            }
        }

        public static Vec3 operator +(Vec3 left, Vec3 right) =>
            new Vec3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);

        public static Vec3 operator -(Vec3 left, Vec3 right) =>
            new Vec3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

        public static Vec3 operator *(Vec3 value, float scalar) =>
            new Vec3(value.X * scalar, value.Y * scalar, value.Z * scalar);

        public static Vec3 operator /(Vec3 value, float scalar) =>
            new Vec3(value.X / scalar, value.Y / scalar, value.Z / scalar);

        public static float Dot(Vec3 left, Vec3 right) =>
            left.X * right.X + left.Y * right.Y + left.Z * right.Z;

        public static Vec3 Cross(Vec3 left, Vec3 right) =>
            new Vec3(
                left.Y * right.Z - left.Z * right.Y,
                left.Z * right.X - left.X * right.Z,
                left.X * right.Y - left.Y * right.X);
    }
}
