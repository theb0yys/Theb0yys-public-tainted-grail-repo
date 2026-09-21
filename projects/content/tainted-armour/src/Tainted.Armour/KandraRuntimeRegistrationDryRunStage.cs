using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tainted.Armour;

public sealed class KandraRuntimeRegistrationContractFingerprint
{
    public const string ObservedStatus = "observed:no_write_registration_contract_surface_fingerprinted";
    public const string ExpectedManagerType = "Awaken.Kandra.KandraRendererManager";
    public const string ExpectedRendererType = "Awaken.Kandra.KandraRenderer";
    public const string ExpectedRegistrationMethodSignature =
        "System.Void Awaken.Kandra.KandraRendererManager.Register(Awaken.Kandra.KandraRenderer kandraRenderer)";
    public const string ExpectedRegistrationMethodFingerprint =
        "sha256:a398bf5755dad75ebaf958dc068b65590b0b359267e026b44b7d959f885ec2ea";
    public const string ExpectedAssemblyName = "Awaken.Kandra";
    public const string ExpectedAssemblySha256 =
        "sha256:cddcfc124bcfe9ba88a102a14b058e9d0c9cd997e455b2f7f5caa839abd3bfbf";

    public KandraRuntimeRegistrationContractFingerprint(
        string status,
        string assemblyName,
        string assemblyLocation,
        string assemblySha256,
        string managerType,
        bool managerInstancePresent,
        string registrationOwner,
        string registrationMethodSignature,
        string registrationMethodFingerprint,
        bool registrationMethodPresent,
        bool registrationMethodPublic,
        string rendererType,
        bool rendererTypePresent,
        bool rendererOnEnableMethodPresent,
        bool rendererOnDisableMethodPresent,
        bool rendererDataFieldPresent,
        bool rendererRenderingIdPropertyPresent,
        bool requiredRendererDataMembersPresent,
        bool requiredKandraMeshMembersPresent,
        bool managerDependencyMethodsPresent,
        bool finalizeRegistrationMethodPresent,
        bool onEarlyUpdateBeginMethodPresent,
        bool onBeginRenderingMethodPresent,
        bool queueStateFieldsPresent,
        string diagnosticBoundary,
        bool registrationMethodInvoked,
        bool canRegisterMethodsInvoked,
        bool createdOrActivatedKandraObject,
        bool runtimeRegistrationInvocationAllowed,
        bool runtimeRegistrationExecuted,
        bool candidateMapApplicationExecuted,
        bool conversionExecuted,
        bool itemEquipMutationExecuted,
        bool saveMutationExecuted,
        bool nativeGameWriteExecuted,
        bool downstreamWritesExecuted)
    {
        Status = ContractValues.RequireText(status, nameof(status));
        AssemblyName = ContractValues.RequireText(assemblyName, nameof(assemblyName));
        AssemblyLocation = assemblyLocation ?? string.Empty;
        AssemblySha256 = ContractValues.RequireText(assemblySha256, nameof(assemblySha256));
        ManagerType = ContractValues.RequireText(managerType, nameof(managerType));
        ManagerInstancePresent = managerInstancePresent;
        RegistrationOwner = ContractValues.RequireText(registrationOwner, nameof(registrationOwner));
        RegistrationMethodSignature = ContractValues.RequireText(registrationMethodSignature, nameof(registrationMethodSignature));
        RegistrationMethodFingerprint = ContractValues.RequireText(registrationMethodFingerprint, nameof(registrationMethodFingerprint));
        RegistrationMethodPresent = registrationMethodPresent;
        RegistrationMethodPublic = registrationMethodPublic;
        RendererType = ContractValues.RequireText(rendererType, nameof(rendererType));
        RendererTypePresent = rendererTypePresent;
        RendererOnEnableMethodPresent = rendererOnEnableMethodPresent;
        RendererOnDisableMethodPresent = rendererOnDisableMethodPresent;
        RendererDataFieldPresent = rendererDataFieldPresent;
        RendererRenderingIdPropertyPresent = rendererRenderingIdPropertyPresent;
        RequiredRendererDataMembersPresent = requiredRendererDataMembersPresent;
        RequiredKandraMeshMembersPresent = requiredKandraMeshMembersPresent;
        ManagerDependencyMethodsPresent = managerDependencyMethodsPresent;
        FinalizeRegistrationMethodPresent = finalizeRegistrationMethodPresent;
        OnEarlyUpdateBeginMethodPresent = onEarlyUpdateBeginMethodPresent;
        OnBeginRenderingMethodPresent = onBeginRenderingMethodPresent;
        QueueStateFieldsPresent = queueStateFieldsPresent;
        DiagnosticBoundary = ContractValues.RequireText(diagnosticBoundary, nameof(diagnosticBoundary));
        RegistrationMethodInvoked = registrationMethodInvoked;
        CanRegisterMethodsInvoked = canRegisterMethodsInvoked;
        CreatedOrActivatedKandraObject = createdOrActivatedKandraObject;
        RuntimeRegistrationInvocationAllowed = runtimeRegistrationInvocationAllowed;
        RuntimeRegistrationExecuted = runtimeRegistrationExecuted;
        CandidateMapApplicationExecuted = candidateMapApplicationExecuted;
        ConversionExecuted = conversionExecuted;
        ItemEquipMutationExecuted = itemEquipMutationExecuted;
        SaveMutationExecuted = saveMutationExecuted;
        NativeGameWriteExecuted = nativeGameWriteExecuted;
        DownstreamWritesExecuted = downstreamWritesExecuted;
    }

    public string Status { get; }

    public string AssemblyName { get; }

    public string AssemblyLocation { get; }

    public string AssemblySha256 { get; }

    public string ManagerType { get; }

    public bool ManagerInstancePresent { get; }

    public string RegistrationOwner { get; }

    public string RegistrationMethodSignature { get; }

    public string RegistrationMethodFingerprint { get; }

    public bool RegistrationMethodPresent { get; }

    public bool RegistrationMethodPublic { get; }

    public string RendererType { get; }

    public bool RendererTypePresent { get; }

    public bool RendererOnEnableMethodPresent { get; }

    public bool RendererOnDisableMethodPresent { get; }

    public bool RendererDataFieldPresent { get; }

    public bool RendererRenderingIdPropertyPresent { get; }

    public bool RequiredRendererDataMembersPresent { get; }

    public bool RequiredKandraMeshMembersPresent { get; }

    public bool ManagerDependencyMethodsPresent { get; }

    public bool FinalizeRegistrationMethodPresent { get; }

    public bool OnEarlyUpdateBeginMethodPresent { get; }

    public bool OnBeginRenderingMethodPresent { get; }

    public bool QueueStateFieldsPresent { get; }

    public string DiagnosticBoundary { get; }

    public bool RegistrationMethodInvoked { get; }

    public bool CanRegisterMethodsInvoked { get; }

    public bool CreatedOrActivatedKandraObject { get; }

    public bool RuntimeRegistrationInvocationAllowed { get; }

    public bool RuntimeRegistrationExecuted { get; }

    public bool CandidateMapApplicationExecuted { get; }

    public bool ConversionExecuted { get; }

    public bool ItemEquipMutationExecuted { get; }

    public bool SaveMutationExecuted { get; }

    public bool NativeGameWriteExecuted { get; }

    public bool DownstreamWritesExecuted { get; }

    public bool SurfaceFingerprintMatchesExpected =>
        string.Equals(Status, ObservedStatus, StringComparison.Ordinal) &&
        string.Equals(ManagerType, ExpectedManagerType, StringComparison.Ordinal) &&
        string.Equals(RegistrationOwner, ExpectedManagerType, StringComparison.Ordinal) &&
        string.Equals(RendererType, ExpectedRendererType, StringComparison.Ordinal) &&
        string.Equals(RegistrationMethodSignature, ExpectedRegistrationMethodSignature, StringComparison.Ordinal) &&
        string.Equals(RegistrationMethodFingerprint, ExpectedRegistrationMethodFingerprint, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(AssemblyName, ExpectedAssemblyName, StringComparison.Ordinal) &&
        string.Equals(AssemblySha256, ExpectedAssemblySha256, StringComparison.OrdinalIgnoreCase);

    public bool RequiredSurfacePresent =>
        ManagerInstancePresent &&
        RegistrationMethodPresent &&
        RegistrationMethodPublic &&
        RendererTypePresent &&
        RendererOnEnableMethodPresent &&
        RendererOnDisableMethodPresent &&
        RendererDataFieldPresent &&
        RendererRenderingIdPropertyPresent &&
        RequiredRendererDataMembersPresent &&
        RequiredKandraMeshMembersPresent &&
        ManagerDependencyMethodsPresent &&
        FinalizeRegistrationMethodPresent &&
        OnEarlyUpdateBeginMethodPresent &&
        OnBeginRenderingMethodPresent &&
        QueueStateFieldsPresent;

    public bool NoWriteBoundaryFalse =>
        !RegistrationMethodInvoked &&
        !CanRegisterMethodsInvoked &&
        !CreatedOrActivatedKandraObject &&
        !RuntimeRegistrationInvocationAllowed &&
        !RuntimeRegistrationExecuted &&
        !CandidateMapApplicationExecuted &&
        !ConversionExecuted &&
        !ItemEquipMutationExecuted &&
        !SaveMutationExecuted &&
        !NativeGameWriteExecuted &&
        !DownstreamWritesExecuted;
}

public sealed class KandraRuntimeRegistrationDryRunRequest
{
    public KandraRuntimeRegistrationDryRunRequest(
        string requestId,
        KandraRegistrationCandidateBuildResult candidateBuild,
        KandraPackageValidationResult packageValidation,
        KandraRuntimeRegistrationContractFingerprint contractFingerprint)
    {
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        CandidateBuild = candidateBuild ?? throw new ArgumentNullException(nameof(candidateBuild));
        PackageValidation = packageValidation ?? throw new ArgumentNullException(nameof(packageValidation));
        ContractFingerprint = contractFingerprint ?? throw new ArgumentNullException(nameof(contractFingerprint));
    }

    public string RequestId { get; }

    public KandraRegistrationCandidateBuildResult CandidateBuild { get; }

    public KandraPackageValidationResult PackageValidation { get; }

    public KandraRuntimeRegistrationContractFingerprint ContractFingerprint { get; }
}

public sealed class KandraRuntimeRegistrationDryRunResult
{
    private readonly string[] blockers;
    private readonly string[] warnings;

    public KandraRuntimeRegistrationDryRunResult(
        string stageVersion,
        string requestId,
        string candidateStageVersion,
        string packageValidationStageVersion,
        bool candidateBuildAccepted,
        bool candidateRequestPresent,
        bool packageValidationAccepted,
        bool candidateMatchesValidatedPackage,
        bool loosePackageStillPresent,
        bool loosePackageHashesStillMatch,
        bool contractFingerprintAccepted,
        bool contractSurfacePresent,
        bool contractNoWriteBoundaryFalse,
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
        string indicesDataSha256,
        string registrationOwner,
        string registrationMethodSignature,
        string registrationMethodFingerprint,
        string registrationAssemblySha256,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings)
    {
        StageVersion = ContractValues.RequireText(stageVersion, nameof(stageVersion));
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        CandidateStageVersion = ContractValues.RequireText(candidateStageVersion, nameof(candidateStageVersion));
        PackageValidationStageVersion = ContractValues.RequireText(packageValidationStageVersion, nameof(packageValidationStageVersion));
        CandidateBuildAccepted = candidateBuildAccepted;
        CandidateRequestPresent = candidateRequestPresent;
        PackageValidationAccepted = packageValidationAccepted;
        CandidateMatchesValidatedPackage = candidateMatchesValidatedPackage;
        LoosePackageStillPresent = loosePackageStillPresent;
        LoosePackageHashesStillMatch = loosePackageHashesStillMatch;
        RuntimeRegistrationContractFingerprintAccepted = contractFingerprintAccepted;
        RuntimeRegistrationContractSurfacePresent = contractSurfacePresent;
        RuntimeRegistrationContractNoWriteBoundaryFalse = contractNoWriteBoundaryFalse;
        ModDirectory = modDirectory ?? string.Empty;
        Name = name ?? string.Empty;
        VertexCount = vertexCount;
        IndicesCount = indicesCount;
        BindposesCount = bindposesCount;
        BlendshapeCount = blendshapeCount;
        LayoutVersion = layoutVersion ?? string.Empty;
        KandraDirectory = kandraDirectory ?? string.Empty;
        MeshDataPath = meshDataPath ?? string.Empty;
        IndicesDataPath = indicesDataPath ?? string.Empty;
        MeshDataByteCount = meshDataByteCount;
        IndicesDataByteCount = indicesDataByteCount;
        MeshDataSha256 = meshDataSha256 ?? string.Empty;
        IndicesDataSha256 = indicesDataSha256 ?? string.Empty;
        RegistrationOwner = registrationOwner ?? string.Empty;
        RegistrationMethodSignature = registrationMethodSignature ?? string.Empty;
        RegistrationMethodFingerprint = registrationMethodFingerprint ?? string.Empty;
        RegistrationAssemblySha256 = registrationAssemblySha256 ?? string.Empty;
        this.blockers = CopyMessages(blockers);
        this.warnings = CopyMessages(warnings);
    }

    public string StageVersion { get; }

    public string RequestId { get; }

    public string CandidateStageVersion { get; }

    public string PackageValidationStageVersion { get; }

    public bool CandidateBuildAccepted { get; }

    public bool CandidateRequestPresent { get; }

    public bool PackageValidationAccepted { get; }

    public bool CandidateMatchesValidatedPackage { get; }

    public bool LoosePackageStillPresent { get; }

    public bool LoosePackageHashesStillMatch { get; }

    public bool RuntimeRegistrationContractFingerprintAccepted { get; }

    public bool RuntimeRegistrationContractSurfacePresent { get; }

    public bool RuntimeRegistrationContractNoWriteBoundaryFalse { get; }

    public bool RegistrationDryRunAccepted => blockers.Length == 0;

    public bool RuntimeRegistrationCallContractProven => RegistrationDryRunAccepted;

    public bool ExplicitRuntimeInvocationApprovalPresent => false;

    public bool RuntimeRegistrationInvocationAllowed => false;

    public bool RegistrationMethodInvoked => false;

    public bool CanRegisterMethodsInvoked => false;

    public bool CreatedOrActivatedKandraObject => false;

    public bool RuntimeRegistrationExecuted => false;

    public bool CandidateMapApplicationAllowed => false;

    public bool CandidateMapApplicationExecuted => false;

    public bool ConversionAllowed => false;

    public bool ConversionExecuted => false;

    public bool ItemEquipMutationExecuted => false;

    public bool SaveMutationExecuted => false;

    public bool NativeGameWriteExecuted => false;

    public bool DownstreamWritesExecuted => false;

    public string RegistrationInvocationRefusalReason => "runtime_registration_invocation_approval_not_defined";

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

    public string RegistrationOwner { get; }

    public string RegistrationMethodSignature { get; }

    public string RegistrationMethodFingerprint { get; }

    public string RegistrationAssemblySha256 { get; }

    public IReadOnlyList<string> Blockers => blockers;

    public IReadOnlyList<string> Warnings => warnings;

    private static string[] CopyMessages(IEnumerable<string> values) =>
        values?.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToArray()
        ?? Array.Empty<string>();
}

public sealed class KandraRuntimeRegistrationDryRunStage
{
    public const string StageVersion = "tainted-armour.kandra-runtime-registration-dry-run.v1";

    public KandraRuntimeRegistrationDryRunResult Evaluate(KandraRuntimeRegistrationDryRunRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        KandraRegistrationCandidateBuildResult candidateBuild = request.CandidateBuild;
        KandraPackageValidationResult package = request.PackageValidation;
        KandraRuntimeRegistrationContractFingerprint contract = request.ContractFingerprint;
        KandraCustomRegistrationCandidateRequest? candidate = candidateBuild.CandidateRequest;
        var blockers = new List<string>();
        var warnings = new List<string>();

        CopyMessages("candidate:", candidateBuild.Blockers, blockers);
        CopyMessages("candidate:", candidateBuild.Warnings, warnings);
        CopyMessages("package:", package.Blockers, blockers);
        CopyMessages("package:", package.Warnings, warnings);

        bool candidateBuildAccepted = candidateBuild.CandidateBuildAccepted;
        if (!candidateBuildAccepted)
        {
            blockers.Add("kandra_runtime_registration_dry_run_candidate_not_accepted");
        }

        bool candidateRequestPresent = candidate != null;
        if (!candidateRequestPresent)
        {
            blockers.Add("kandra_runtime_registration_dry_run_candidate_request_missing");
        }

        bool packageValidationAccepted = package.PackageValidationAccepted;
        if (!packageValidationAccepted)
        {
            blockers.Add("kandra_runtime_registration_dry_run_package_not_accepted");
        }

        bool candidateMatchesPackage = candidate != null && CandidateMatchesPackage(candidate, package, blockers);
        bool loosePackageStillPresent = candidate != null && File.Exists(candidate.MeshDataPath) && File.Exists(candidate.IndicesDataPath);
        if (!loosePackageStillPresent)
        {
            blockers.Add("kandra_runtime_registration_dry_run_loose_package_files_missing");
        }

        bool loosePackageHashesStillMatch = loosePackageStillPresent && candidate != null && LoosePackageHashesStillMatch(candidate);
        if (loosePackageStillPresent && !loosePackageHashesStillMatch)
        {
            blockers.Add("kandra_runtime_registration_dry_run_loose_package_hash_mismatch");
        }

        if (!IsPackageDownstreamFalse(package))
        {
            blockers.Add("kandra_runtime_registration_dry_run_package_downstream_boundary_not_false");
        }

        if (!IsCandidateDownstreamFalse(candidateBuild, candidate))
        {
            blockers.Add("kandra_runtime_registration_dry_run_candidate_downstream_boundary_not_false");
        }

        bool contractFingerprintAccepted = contract.SurfaceFingerprintMatchesExpected;
        if (!contractFingerprintAccepted)
        {
            blockers.Add("kandra_runtime_registration_dry_run_contract_fingerprint_not_accepted");
        }

        bool contractSurfacePresent = contract.RequiredSurfacePresent;
        if (!contractSurfacePresent)
        {
            blockers.Add("kandra_runtime_registration_dry_run_contract_surface_incomplete");
        }

        bool contractNoWriteBoundaryFalse = contract.NoWriteBoundaryFalse;
        if (!contractNoWriteBoundaryFalse)
        {
            blockers.Add("kandra_runtime_registration_dry_run_contract_downstream_boundary_not_false");
        }

        warnings.Add("runtime_registration_invocation_approval_not_defined");
        warnings.Add("runtime_registration_invocation_refused");

        return new KandraRuntimeRegistrationDryRunResult(
            StageVersion,
            request.RequestId,
            candidateBuild.StageVersion,
            package.StageVersion,
            candidateBuildAccepted,
            candidateRequestPresent,
            packageValidationAccepted,
            candidateMatchesPackage,
            loosePackageStillPresent,
            loosePackageHashesStillMatch,
            contractFingerprintAccepted,
            contractSurfacePresent,
            contractNoWriteBoundaryFalse,
            candidate?.ModDirectory ?? package.ModDirectory,
            candidate?.Name ?? package.MeshName,
            candidate?.VertexCount ?? 0,
            candidate?.IndicesCount ?? 0L,
            candidate?.BindposesCount ?? 0,
            candidate?.BlendshapeCount ?? 0,
            candidate?.LayoutVersion ?? package.LayoutVersion,
            candidate?.KandraDirectory ?? package.KandraDirectory,
            candidate?.MeshDataPath ?? package.MeshDataPath,
            candidate?.IndicesDataPath ?? package.IndicesDataPath,
            candidate?.MeshDataByteCount ?? package.MeshDataByteCount,
            candidate?.IndicesDataByteCount ?? package.IndicesDataByteCount,
            candidate?.MeshDataSha256 ?? package.MeshDataSha256,
            candidate?.IndicesDataSha256 ?? package.IndicesDataSha256,
            contract.RegistrationOwner,
            contract.RegistrationMethodSignature,
            contract.RegistrationMethodFingerprint,
            contract.AssemblySha256,
            blockers,
            warnings);
    }

    private static bool CandidateMatchesPackage(
        KandraCustomRegistrationCandidateRequest candidate,
        KandraPackageValidationResult package,
        List<string> blockers)
    {
        bool matches = true;
        matches &= Compare(candidate.ModDirectory, package.ModDirectory, "mod_directory", blockers);
        matches &= Compare(candidate.Name, package.MeshName, "name", blockers);
        matches &= Compare(candidate.LayoutVersion, package.LayoutVersion, "layout_version", blockers);
        matches &= Compare(candidate.KandraDirectory, package.KandraDirectory, "kandra_directory", blockers);
        matches &= Compare(candidate.MeshDataPath, package.MeshDataPath, "mesh_data_path", blockers);
        matches &= Compare(candidate.IndicesDataPath, package.IndicesDataPath, "indices_data_path", blockers);
        matches &= Compare(candidate.MeshDataByteCount, package.MeshDataByteCount, "mesh_data_byte_count", blockers);
        matches &= Compare(candidate.IndicesDataByteCount, package.IndicesDataByteCount, "indices_data_byte_count", blockers);
        matches &= Compare(candidate.MeshDataSha256, package.MeshDataSha256, "mesh_data_sha256", blockers);
        matches &= Compare(candidate.IndicesDataSha256, package.IndicesDataSha256, "indices_data_sha256", blockers);
        return matches;
    }

    private static bool Compare(string actual, string expected, string field, List<string> blockers)
    {
        if (string.Equals(actual, expected, StringComparison.Ordinal))
        {
            return true;
        }

        blockers.Add("kandra_runtime_registration_dry_run_candidate_package_mismatch:" + field);
        return false;
    }

    private static bool Compare(long actual, long expected, string field, List<string> blockers)
    {
        if (actual == expected)
        {
            return true;
        }

        blockers.Add("kandra_runtime_registration_dry_run_candidate_package_mismatch:" + field);
        return false;
    }

    private static bool LoosePackageHashesStillMatch(KandraCustomRegistrationCandidateRequest candidate) =>
        string.Equals(Sha256File(candidate.MeshDataPath), candidate.MeshDataSha256, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(Sha256File(candidate.IndicesDataPath), candidate.IndicesDataSha256, StringComparison.OrdinalIgnoreCase);

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

    private static bool IsCandidateDownstreamFalse(
        KandraRegistrationCandidateBuildResult candidateBuild,
        KandraCustomRegistrationCandidateRequest? candidate) =>
        !candidateBuild.CandidateMapApplicationAllowed &&
        !candidateBuild.CandidateMapApplicationExecuted &&
        !candidateBuild.ConversionAllowed &&
        !candidateBuild.ConversionExecuted &&
        !candidateBuild.RuntimeRegistrationInvocationAllowed &&
        !candidateBuild.RuntimeRegistrationExecuted &&
        !candidateBuild.ItemEquipMutationExecuted &&
        !candidateBuild.SaveMutationExecuted &&
        !candidateBuild.NativeGameWriteExecuted &&
        !candidateBuild.DownstreamWritesExecuted &&
        (candidate == null ||
         (!candidate.CandidateMapApplicationAllowed &&
          !candidate.CandidateMapApplicationExecuted &&
          !candidate.ConversionAllowed &&
          !candidate.ConversionExecuted &&
          !candidate.RuntimeRegistrationInvocationAllowed &&
          !candidate.RuntimeRegistrationExecuted &&
          !candidate.ItemEquipMutationExecuted &&
          !candidate.SaveMutationExecuted &&
          !candidate.NativeGameWriteExecuted &&
          !candidate.DownstreamWritesExecuted));

    private static void CopyMessages(string prefix, IEnumerable<string> source, List<string> target)
    {
        foreach (string message in source)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                target.Add(prefix + message.Trim());
            }
        }
    }

    private static string Sha256File(string path)
    {
        using (var stream = File.OpenRead(path))
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] digest = sha256.ComputeHash(stream);
            var hexadecimal = new StringBuilder(digest.Length * 2);
            foreach (byte value in digest)
            {
                hexadecimal.Append(value.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
            }

            return "sha256:" + hexadecimal;
        }
    }
}
