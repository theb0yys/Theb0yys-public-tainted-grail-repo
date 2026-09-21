using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tainted.Armour;

public sealed class KandraCustomRegistrationCandidateRequest
{
    public const string CandidateSchemaVersion = "tainted-armour.kandra-custom-registration-candidate.v1";
    public const string RuntimeRegistrationCallContractStatusValue = "blocked_unproven:live-registration-call-contract";

    public KandraCustomRegistrationCandidateRequest(
        string requestId,
        string sourcePreflightRequestId,
        string modDirectory,
        string name,
        int vertexCount,
        long indicesCount,
        int bindposesCount,
        int blendshapeCount,
        string layoutVersion,
        string kandraDirectory,
        string meshDataPath,
        string indicesDataPath,
        long meshDataByteCount,
        long indicesDataByteCount,
        string meshDataSha256,
        string indicesDataSha256)
    {
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        SourcePreflightRequestId = ContractValues.RequireText(sourcePreflightRequestId, nameof(sourcePreflightRequestId));
        ModDirectory = RequirePathSegment(modDirectory, nameof(modDirectory));
        Name = RequireFileStem(name, nameof(name));
        if (vertexCount <= 0 || vertexCount > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(vertexCount));
        if (indicesCount < 0 || indicesCount > uint.MaxValue) throw new ArgumentOutOfRangeException(nameof(indicesCount));
        if (bindposesCount < 0 || bindposesCount > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(bindposesCount));
        if (blendshapeCount < 0) throw new ArgumentOutOfRangeException(nameof(blendshapeCount));
        if (meshDataByteCount < 0) throw new ArgumentOutOfRangeException(nameof(meshDataByteCount));
        if (indicesDataByteCount < 0) throw new ArgumentOutOfRangeException(nameof(indicesDataByteCount));

        VertexCount = vertexCount;
        IndicesCount = indicesCount;
        BindposesCount = bindposesCount;
        BlendshapeCount = blendshapeCount;
        LayoutVersion = ContractValues.RequireText(layoutVersion, nameof(layoutVersion));
        KandraDirectory = ContractValues.RequireText(kandraDirectory, nameof(kandraDirectory));
        MeshDataPath = ContractValues.RequireText(meshDataPath, nameof(meshDataPath));
        IndicesDataPath = ContractValues.RequireText(indicesDataPath, nameof(indicesDataPath));
        MeshDataByteCount = meshDataByteCount;
        IndicesDataByteCount = indicesDataByteCount;
        MeshDataSha256 = ContractValues.RequireText(meshDataSha256, nameof(meshDataSha256));
        IndicesDataSha256 = ContractValues.RequireText(indicesDataSha256, nameof(indicesDataSha256));
    }

    public string SchemaVersion => CandidateSchemaVersion;

    public string RequestId { get; }

    public string SourcePreflightRequestId { get; }

    public string ModDirectory { get; }

    public string Name { get; }

    public string KandraMeshModDirectory => ModDirectory;

    public string KandraMeshName => Name;

    public int VertexCount { get; }

    public long IndicesCount { get; }

    public int BindposesCount { get; }

    public int BlendshapeCount { get; }

    public string LayoutVersion { get; }

    public string KandraDirectory { get; }

    public string MeshDataPath { get; }

    public string IndicesDataPath { get; }

    public long MeshDataByteCount { get; }

    public long IndicesDataByteCount { get; }

    public string MeshDataSha256 { get; }

    public string IndicesDataSha256 { get; }

    public string RuntimeRegistrationCallContractStatus => RuntimeRegistrationCallContractStatusValue;

    public bool RuntimeRegistrationCallContractProven => false;

    public bool RuntimeRegistrationInvocationAllowed => false;

    public bool RuntimeRegistrationExecuted => false;

    public bool CandidateMapApplicationAllowed => false;

    public bool CandidateMapApplicationExecuted => false;

    public bool ConversionAllowed => false;

    public bool ConversionExecuted => false;

    public bool ItemEquipMutationExecuted => false;

    public bool SaveMutationExecuted => false;

    public bool NativeGameWriteExecuted => false;

    public bool DownstreamWritesExecuted => false;

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

public sealed class KandraRegistrationCandidateBuildRequest
{
    public KandraRegistrationCandidateBuildRequest(
        string requestId,
        KandraRegistrationPreflightResult preflight)
    {
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        Preflight = preflight ?? throw new ArgumentNullException(nameof(preflight));
    }

    public string RequestId { get; }

    public KandraRegistrationPreflightResult Preflight { get; }
}

public sealed class KandraRegistrationCandidateBuildResult
{
    private readonly string[] blockers;
    private readonly string[] warnings;

    public KandraRegistrationCandidateBuildResult(
        string stageVersion,
        string requestId,
        string preflightStageVersion,
        bool preflightAccepted,
        bool candidateRequestConstructed,
        KandraCustomRegistrationCandidateRequest? candidateRequest,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings)
    {
        StageVersion = ContractValues.RequireText(stageVersion, nameof(stageVersion));
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        PreflightStageVersion = ContractValues.RequireText(preflightStageVersion, nameof(preflightStageVersion));
        PreflightAccepted = preflightAccepted;
        CandidateRequestConstructed = candidateRequestConstructed;
        CandidateRequest = candidateRequest;
        this.blockers = CopyMessages(blockers);
        this.warnings = CopyMessages(warnings);
    }

    public string StageVersion { get; }

    public string RequestId { get; }

    public string PreflightStageVersion { get; }

    public bool PreflightAccepted { get; }

    public bool CandidateRequestConstructed { get; }

    public KandraCustomRegistrationCandidateRequest? CandidateRequest { get; }

    public bool CandidateBuildAccepted => blockers.Length == 0 && CandidateRequestConstructed;

    public string RuntimeRegistrationCallContractStatus =>
        KandraCustomRegistrationCandidateRequest.RuntimeRegistrationCallContractStatusValue;

    public bool RuntimeRegistrationCallContractProven => false;

    public bool RuntimeRegistrationInvocationAllowed => false;

    public bool RuntimeRegistrationExecuted => false;

    public bool CandidateMapApplicationAllowed => false;

    public bool CandidateMapApplicationExecuted => false;

    public bool ConversionAllowed => false;

    public bool ConversionExecuted => false;

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

public sealed class KandraRegistrationCandidateStage
{
    public const string StageVersion = "tainted-armour.kandra-registration-candidate.v1";

    public KandraRegistrationCandidateBuildResult Build(KandraRegistrationCandidateBuildRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        KandraRegistrationPreflightResult preflight = request.Preflight;
        var blockers = new List<string>();
        var warnings = new List<string>();

        CopyPreflightMessages(preflight, blockers, warnings);

        bool preflightAccepted = preflight.RegistrationPreflightAccepted;
        if (!preflightAccepted)
        {
            blockers.Add("kandra_registration_candidate_preflight_not_accepted");
        }

        if (!IsPreflightDownstreamFalse(preflight))
        {
            blockers.Add("kandra_registration_candidate_preflight_downstream_boundary_not_false");
        }

        KandraCustomRegistrationCandidateRequest? candidate = null;
        if (blockers.Count == 0)
        {
            candidate = new KandraCustomRegistrationCandidateRequest(
                request.RequestId + ".candidate",
                preflight.RequestId,
                preflight.ModDirectory,
                preflight.Name,
                preflight.VertexCount,
                preflight.IndicesCount,
                preflight.BindposesCount,
                preflight.BlendshapeCount,
                preflight.LayoutVersion,
                preflight.KandraDirectory,
                preflight.MeshDataPath,
                preflight.IndicesDataPath,
                preflight.ActualMeshDataByteCount,
                preflight.ActualIndicesDataByteCount,
                preflight.MeshDataSha256,
                preflight.IndicesDataSha256);
        }

        warnings.Add("runtime_registration_call_contract_not_proven");

        return new KandraRegistrationCandidateBuildResult(
            StageVersion,
            request.RequestId,
            preflight.StageVersion,
            preflightAccepted,
            candidate != null,
            candidate,
            blockers,
            warnings);
    }

    private static void CopyPreflightMessages(
        KandraRegistrationPreflightResult preflight,
        List<string> blockers,
        List<string> warnings)
    {
        foreach (string blocker in preflight.Blockers)
        {
            blockers.Add("preflight:" + blocker);
        }

        foreach (string warning in preflight.Warnings)
        {
            warnings.Add("preflight:" + warning);
        }
    }

    private static bool IsPreflightDownstreamFalse(KandraRegistrationPreflightResult preflight) =>
        !preflight.CandidateMapApplicationAllowed &&
        !preflight.CandidateMapApplicationExecuted &&
        !preflight.ConversionAllowed &&
        !preflight.ConversionExecuted &&
        !preflight.CustomArmourRegistrationAllowed &&
        !preflight.RuntimeRegistrationExecuted &&
        !preflight.ItemEquipMutationExecuted &&
        !preflight.SaveMutationExecuted &&
        !preflight.NativeGameWriteExecuted &&
        !preflight.DownstreamWritesExecuted;
}
