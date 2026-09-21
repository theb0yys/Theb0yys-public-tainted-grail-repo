using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Tainted.Armour;

public sealed class KandraMeshRegistrationMetadata
{
    public KandraMeshRegistrationMetadata(
        string modDirectory,
        string name,
        int vertexCount,
        long indicesCount,
        int bindposesCount,
        int blendshapeCount,
        string layoutVersion)
    {
        ModDirectory = RequirePathSegment(modDirectory, nameof(modDirectory));
        Name = RequireFileStem(name, nameof(name));
        if (vertexCount <= 0 || vertexCount > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(vertexCount));
        if (indicesCount < 0 || indicesCount > uint.MaxValue) throw new ArgumentOutOfRangeException(nameof(indicesCount));
        if (bindposesCount < 0 || bindposesCount > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(bindposesCount));
        if (blendshapeCount < 0) throw new ArgumentOutOfRangeException(nameof(blendshapeCount));

        VertexCount = vertexCount;
        IndicesCount = indicesCount;
        BindposesCount = bindposesCount;
        BlendshapeCount = blendshapeCount;
        LayoutVersion = ContractValues.RequireText(layoutVersion, nameof(layoutVersion));
    }

    public string ModDirectory { get; }

    public string Name { get; }

    public int VertexCount { get; }

    public long IndicesCount { get; }

    public int BindposesCount { get; }

    public int BlendshapeCount { get; }

    public string LayoutVersion { get; }

    public long ExpectedMeshDataByteCount =>
        KandraRuntimePayloadLayout.ExpectedMeshPayloadByteCount(VertexCount, BindposesCount, BlendshapeCount);

    public long ExpectedIndicesDataByteCount
    {
        get
        {
            checked
            {
                return IndicesCount * KandraRuntimePayloadLayout.IndexByteSize;
            }
        }
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

public sealed class KandraRegistrationPreflightRequest
{
    public KandraRegistrationPreflightRequest(
        string requestId,
        KandraMeshRegistrationMetadata metadata,
        KandraPackageValidationResult packageValidation)
    {
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        PackageValidation = packageValidation ?? throw new ArgumentNullException(nameof(packageValidation));
    }

    public string RequestId { get; }

    public KandraMeshRegistrationMetadata Metadata { get; }

    public KandraPackageValidationResult PackageValidation { get; }
}

public sealed class KandraRegistrationPreflightResult
{
    private readonly string[] blockers;
    private readonly string[] warnings;

    public KandraRegistrationPreflightResult(
        string stageVersion,
        string requestId,
        string packageValidationStageVersion,
        string layoutVersion,
        string modDirectory,
        string name,
        int vertexCount,
        long indicesCount,
        int bindposesCount,
        int blendshapeCount,
        long expectedMeshDataByteCount,
        long actualMeshDataByteCount,
        long expectedIndicesDataByteCount,
        long actualIndicesDataByteCount,
        string kandraDirectory,
        string meshDataPath,
        string indicesDataPath,
        string meshDataSha256,
        string indicesDataSha256,
        bool packageValidationAccepted,
        bool runtimeSeamValid,
        bool metadataModDirectoryMatches,
        bool metadataNameMatches,
        bool metadataLayoutVersionMatches,
        bool meshDataByteCountMatches,
        bool indicesDataByteCountMatches,
        bool meshDataHashMatches,
        bool indicesDataHashMatches,
        bool liveKandraRegistrationProofPresent,
        bool liveKandraMeshMemoryProofPresent,
        bool packageDownstreamBoundaryFalse,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings)
    {
        StageVersion = ContractValues.RequireText(stageVersion, nameof(stageVersion));
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        PackageValidationStageVersion = ContractValues.RequireText(packageValidationStageVersion, nameof(packageValidationStageVersion));
        LayoutVersion = ContractValues.RequireText(layoutVersion, nameof(layoutVersion));
        ModDirectory = ContractValues.RequireText(modDirectory, nameof(modDirectory));
        Name = ContractValues.RequireText(name, nameof(name));
        VertexCount = vertexCount;
        IndicesCount = indicesCount;
        BindposesCount = bindposesCount;
        BlendshapeCount = blendshapeCount;
        ExpectedMeshDataByteCount = expectedMeshDataByteCount;
        ActualMeshDataByteCount = actualMeshDataByteCount;
        ExpectedIndicesDataByteCount = expectedIndicesDataByteCount;
        ActualIndicesDataByteCount = actualIndicesDataByteCount;
        KandraDirectory = ContractValues.RequireText(kandraDirectory, nameof(kandraDirectory));
        MeshDataPath = ContractValues.RequireText(meshDataPath, nameof(meshDataPath));
        IndicesDataPath = ContractValues.RequireText(indicesDataPath, nameof(indicesDataPath));
        MeshDataSha256 = ContractValues.RequireText(meshDataSha256, nameof(meshDataSha256));
        IndicesDataSha256 = ContractValues.RequireText(indicesDataSha256, nameof(indicesDataSha256));
        PackageValidationAccepted = packageValidationAccepted;
        RuntimeSeamValid = runtimeSeamValid;
        MetadataModDirectoryMatches = metadataModDirectoryMatches;
        MetadataNameMatches = metadataNameMatches;
        MetadataLayoutVersionMatches = metadataLayoutVersionMatches;
        MeshDataByteCountMatches = meshDataByteCountMatches;
        IndicesDataByteCountMatches = indicesDataByteCountMatches;
        MeshDataHashMatches = meshDataHashMatches;
        IndicesDataHashMatches = indicesDataHashMatches;
        LiveKandraRegistrationProofPresent = liveKandraRegistrationProofPresent;
        LiveKandraMeshMemoryProofPresent = liveKandraMeshMemoryProofPresent;
        PackageDownstreamBoundaryFalse = packageDownstreamBoundaryFalse;
        this.blockers = CopyMessages(blockers);
        this.warnings = CopyMessages(warnings);
    }

    public string StageVersion { get; }

    public string RequestId { get; }

    public string PackageValidationStageVersion { get; }

    public string LayoutVersion { get; }

    public string ModDirectory { get; }

    public string Name { get; }

    public int VertexCount { get; }

    public long IndicesCount { get; }

    public int BindposesCount { get; }

    public int BlendshapeCount { get; }

    public long ExpectedMeshDataByteCount { get; }

    public long ActualMeshDataByteCount { get; }

    public long ExpectedIndicesDataByteCount { get; }

    public long ActualIndicesDataByteCount { get; }

    public string KandraDirectory { get; }

    public string MeshDataPath { get; }

    public string IndicesDataPath { get; }

    public string MeshDataSha256 { get; }

    public string IndicesDataSha256 { get; }

    public bool PackageValidationAccepted { get; }

    public bool RuntimeSeamValid { get; }

    public bool MetadataModDirectoryMatches { get; }

    public bool MetadataNameMatches { get; }

    public bool MetadataLayoutVersionMatches { get; }

    public bool MeshDataByteCountMatches { get; }

    public bool IndicesDataByteCountMatches { get; }

    public bool MeshDataHashMatches { get; }

    public bool IndicesDataHashMatches { get; }

    public bool LiveKandraRegistrationProofPresent { get; }

    public bool LiveKandraMeshMemoryProofPresent { get; }

    public bool PackageDownstreamBoundaryFalse { get; }

    public bool RegistrationPreflightAccepted => blockers.Length == 0;

    public bool CandidateMapApplicationAllowed => false;

    public bool CandidateMapApplicationExecuted => false;

    public bool ConversionAllowed => false;

    public bool ConversionExecuted => false;

    public bool CustomArmourRegistrationAllowed => false;

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

public sealed class KandraRegistrationPreflightStage
{
    public const string StageVersion = "tainted-armour.kandra-registration-preflight.v1";

    public KandraRegistrationPreflightResult Validate(KandraRegistrationPreflightRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        KandraMeshRegistrationMetadata metadata = request.Metadata;
        KandraPackageValidationResult package = request.PackageValidation;
        var blockers = new List<string>();
        var warnings = new List<string>();

        CopyPackageMessages(package, blockers, warnings);

        bool packageValidationAccepted = package.PackageValidationAccepted;
        if (!packageValidationAccepted)
        {
            blockers.Add("kandra_registration_preflight_package_validation_not_accepted");
        }

        bool runtimeSeamValid = package.RuntimeSeamValid;
        if (!runtimeSeamValid)
        {
            blockers.Add("kandra_registration_preflight_runtime_seam_invalid");
        }

        bool metadataModDirectoryMatches = string.Equals(metadata.ModDirectory, package.ModDirectory, StringComparison.Ordinal);
        if (!metadataModDirectoryMatches)
        {
            blockers.Add("kandra_registration_preflight_mod_directory_mismatch");
        }

        bool metadataNameMatches = string.Equals(metadata.Name, package.MeshName, StringComparison.Ordinal);
        if (!metadataNameMatches)
        {
            blockers.Add("kandra_registration_preflight_name_mismatch");
        }

        bool metadataLayoutVersionMatches = string.Equals(metadata.LayoutVersion, package.LayoutVersion, StringComparison.Ordinal);
        if (!metadataLayoutVersionMatches)
        {
            blockers.Add("kandra_registration_preflight_layout_version_mismatch");
        }

        bool meshDataByteCountMatches = package.MeshDataLengthMatches && package.MeshDataByteCount == metadata.ExpectedMeshDataByteCount;
        if (!meshDataByteCountMatches)
        {
            blockers.Add(
                "kandra_registration_preflight_mesh_byte_count_mismatch:expected=" +
                metadata.ExpectedMeshDataByteCount.ToString(CultureInfo.InvariantCulture) +
                ":actual=" +
                package.MeshDataByteCount.ToString(CultureInfo.InvariantCulture));
        }

        bool indicesDataByteCountMatches = package.IndicesDataLengthMatches && package.IndicesDataByteCount == metadata.ExpectedIndicesDataByteCount;
        if (!indicesDataByteCountMatches)
        {
            blockers.Add(
                "kandra_registration_preflight_indices_byte_count_mismatch:expected=" +
                metadata.ExpectedIndicesDataByteCount.ToString(CultureInfo.InvariantCulture) +
                ":actual=" +
                package.IndicesDataByteCount.ToString(CultureInfo.InvariantCulture));
        }

        bool packageDownstreamBoundaryFalse = IsPackageDownstreamFalse(package);
        if (!packageDownstreamBoundaryFalse)
        {
            blockers.Add("kandra_registration_preflight_package_downstream_boundary_not_false");
        }

        if (!package.MeshDataHashMatches)
        {
            blockers.Add("kandra_registration_preflight_mesh_hash_not_validated");
        }

        if (!package.IndicesDataHashMatches)
        {
            blockers.Add("kandra_registration_preflight_indices_hash_not_validated");
        }

        if (!package.LiveKandraRegistrationProofPresent)
        {
            blockers.Add("kandra_registration_preflight_live_registration_proof_missing");
        }

        if (!package.LiveKandraMeshMemoryProofPresent)
        {
            blockers.Add("kandra_registration_preflight_live_mesh_memory_proof_missing");
        }

        return new KandraRegistrationPreflightResult(
            StageVersion,
            request.RequestId,
            package.StageVersion,
            metadata.LayoutVersion,
            metadata.ModDirectory,
            metadata.Name,
            metadata.VertexCount,
            metadata.IndicesCount,
            metadata.BindposesCount,
            metadata.BlendshapeCount,
            metadata.ExpectedMeshDataByteCount,
            package.MeshDataByteCount,
            metadata.ExpectedIndicesDataByteCount,
            package.IndicesDataByteCount,
            package.KandraDirectory,
            package.MeshDataPath,
            package.IndicesDataPath,
            package.MeshDataSha256,
            package.IndicesDataSha256,
            packageValidationAccepted,
            runtimeSeamValid,
            metadataModDirectoryMatches,
            metadataNameMatches,
            metadataLayoutVersionMatches,
            meshDataByteCountMatches,
            indicesDataByteCountMatches,
            package.MeshDataHashMatches,
            package.IndicesDataHashMatches,
            package.LiveKandraRegistrationProofPresent,
            package.LiveKandraMeshMemoryProofPresent,
            packageDownstreamBoundaryFalse,
            blockers,
            warnings);
    }

    private static void CopyPackageMessages(
        KandraPackageValidationResult package,
        List<string> blockers,
        List<string> warnings)
    {
        foreach (string blocker in package.Blockers)
        {
            blockers.Add("package:" + blocker);
        }

        foreach (string warning in package.Warnings)
        {
            warnings.Add("package:" + warning);
        }
    }

    private static bool IsPackageDownstreamFalse(KandraPackageValidationResult package) =>
        !package.CandidateMapApplicationAllowed &&
        !package.CandidateMapApplicationExecuted &&
        !package.ConversionAllowed &&
        !package.ConversionExecuted &&
        !package.CustomArmourRegistrationAllowed &&
        !package.RuntimeRegistrationExecuted &&
        !package.ItemEquipMutationExecuted &&
        !package.SaveMutationExecuted &&
        !package.NativeGameWriteExecuted &&
        !package.DownstreamWritesExecuted;
}
