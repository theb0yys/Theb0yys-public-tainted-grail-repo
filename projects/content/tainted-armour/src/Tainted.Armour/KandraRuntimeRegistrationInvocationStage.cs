using System;
using System.Collections.Generic;
using System.Linq;

namespace Tainted.Armour;

public sealed class KandraRuntimeInvocationApproval
{
    public KandraRuntimeInvocationApproval(
        string approvalId,
        string approvedBy,
        string approvedAtUtc,
        bool approved,
        string dryRunRequestId,
        string modDirectory,
        string name,
        string meshDataSha256,
        string indicesDataSha256,
        string registrationMethodFingerprint,
        string registrationAssemblySha256,
        bool allowRuntimeRegistrationInvocation,
        bool allowCandidateMapApplication,
        bool allowConversion,
        bool allowItemEquipMutation,
        bool allowSaveMutation,
        bool allowNativeGameWrite)
    {
        ApprovalId = ContractValues.RequireText(approvalId, nameof(approvalId));
        ApprovedBy = ContractValues.RequireText(approvedBy, nameof(approvedBy));
        ApprovedAtUtc = ContractValues.RequireText(approvedAtUtc, nameof(approvedAtUtc));
        Approved = approved;
        DryRunRequestId = ContractValues.RequireText(dryRunRequestId, nameof(dryRunRequestId));
        ModDirectory = ContractValues.RequireText(modDirectory, nameof(modDirectory));
        Name = ContractValues.RequireText(name, nameof(name));
        MeshDataSha256 = ContractValues.RequireText(meshDataSha256, nameof(meshDataSha256));
        IndicesDataSha256 = ContractValues.RequireText(indicesDataSha256, nameof(indicesDataSha256));
        RegistrationMethodFingerprint = ContractValues.RequireText(registrationMethodFingerprint, nameof(registrationMethodFingerprint));
        RegistrationAssemblySha256 = ContractValues.RequireText(registrationAssemblySha256, nameof(registrationAssemblySha256));
        AllowRuntimeRegistrationInvocation = allowRuntimeRegistrationInvocation;
        AllowCandidateMapApplication = allowCandidateMapApplication;
        AllowConversion = allowConversion;
        AllowItemEquipMutation = allowItemEquipMutation;
        AllowSaveMutation = allowSaveMutation;
        AllowNativeGameWrite = allowNativeGameWrite;
    }

    public string ApprovalId { get; }

    public string ApprovedBy { get; }

    public string ApprovedAtUtc { get; }

    public bool Approved { get; }

    public string DryRunRequestId { get; }

    public string ModDirectory { get; }

    public string Name { get; }

    public string MeshDataSha256 { get; }

    public string IndicesDataSha256 { get; }

    public string RegistrationMethodFingerprint { get; }

    public string RegistrationAssemblySha256 { get; }

    public bool AllowRuntimeRegistrationInvocation { get; }

    public bool AllowCandidateMapApplication { get; }

    public bool AllowConversion { get; }

    public bool AllowItemEquipMutation { get; }

    public bool AllowSaveMutation { get; }

    public bool AllowNativeGameWrite { get; }
}

public sealed class KandraRuntimeRegistrationInvocationRequest
{
    public KandraRuntimeRegistrationInvocationRequest(
        string requestId,
        KandraRuntimeRegistrationDryRunResult dryRun,
        KandraRuntimeInvocationApproval approval,
        IKandraRuntimeRegistrationInvoker? invoker)
    {
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        DryRun = dryRun ?? throw new ArgumentNullException(nameof(dryRun));
        Approval = approval ?? throw new ArgumentNullException(nameof(approval));
        Invoker = invoker;
    }

    public string RequestId { get; }

    public KandraRuntimeRegistrationDryRunResult DryRun { get; }

    public KandraRuntimeInvocationApproval Approval { get; }

    public IKandraRuntimeRegistrationInvoker? Invoker { get; }
}

public sealed class KandraRuntimeRegistrationInvocationContext
{
    public KandraRuntimeRegistrationInvocationContext(
        string requestId,
        string dryRunRequestId,
        string approvalId,
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
        string registrationAssemblySha256)
    {
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        DryRunRequestId = ContractValues.RequireText(dryRunRequestId, nameof(dryRunRequestId));
        ApprovalId = ContractValues.RequireText(approvalId, nameof(approvalId));
        ModDirectory = ContractValues.RequireText(modDirectory, nameof(modDirectory));
        Name = ContractValues.RequireText(name, nameof(name));
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
        RegistrationOwner = ContractValues.RequireText(registrationOwner, nameof(registrationOwner));
        RegistrationMethodSignature = ContractValues.RequireText(registrationMethodSignature, nameof(registrationMethodSignature));
        RegistrationMethodFingerprint = ContractValues.RequireText(registrationMethodFingerprint, nameof(registrationMethodFingerprint));
        RegistrationAssemblySha256 = ContractValues.RequireText(registrationAssemblySha256, nameof(registrationAssemblySha256));
    }

    public string RequestId { get; }

    public string DryRunRequestId { get; }

    public string ApprovalId { get; }

    public string ModDirectory { get; }

    public string Name { get; }

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
}

public interface IKandraRuntimeRegistrationInvoker
{
    KandraRuntimeRegistrationInvocationAttempt Invoke(KandraRuntimeRegistrationInvocationContext context);
}

public sealed class KandraRuntimeRegistrationInvocationAttempt
{
    private readonly string[] blockers;
    private readonly string[] warnings;

    public KandraRuntimeRegistrationInvocationAttempt(
        bool registrationMethodInvoked,
        bool canRegisterMethodsInvoked,
        bool createdOrActivatedKandraObject,
        bool runtimeRegistrationExecuted,
        bool runtimeRegistrationSucceeded,
        string kandraIsRegisteredStatus,
        string kandraTryGetMeshMemoryStatus,
        string registeredRendererIdentity,
        string registeredMeshMemoryIdentity,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings)
    {
        RegistrationMethodInvoked = registrationMethodInvoked;
        CanRegisterMethodsInvoked = canRegisterMethodsInvoked;
        CreatedOrActivatedKandraObject = createdOrActivatedKandraObject;
        RuntimeRegistrationExecuted = runtimeRegistrationExecuted;
        RuntimeRegistrationSucceeded = runtimeRegistrationSucceeded;
        KandraIsRegisteredStatus = kandraIsRegisteredStatus ?? string.Empty;
        KandraTryGetMeshMemoryStatus = kandraTryGetMeshMemoryStatus ?? string.Empty;
        RegisteredRendererIdentity = registeredRendererIdentity ?? string.Empty;
        RegisteredMeshMemoryIdentity = registeredMeshMemoryIdentity ?? string.Empty;
        this.blockers = CopyMessages(blockers);
        this.warnings = CopyMessages(warnings);
    }

    public bool RegistrationMethodInvoked { get; }

    public bool CanRegisterMethodsInvoked { get; }

    public bool CreatedOrActivatedKandraObject { get; }

    public bool RuntimeRegistrationExecuted { get; }

    public bool RuntimeRegistrationSucceeded { get; }

    public string KandraIsRegisteredStatus { get; }

    public string KandraTryGetMeshMemoryStatus { get; }

    public string RegisteredRendererIdentity { get; }

    public string RegisteredMeshMemoryIdentity { get; }

    public IReadOnlyList<string> Blockers => blockers;

    public IReadOnlyList<string> Warnings => warnings;

    private static string[] CopyMessages(IEnumerable<string> values) =>
        values?.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToArray()
        ?? Array.Empty<string>();
}

public sealed class KandraRuntimeRegistrationInvocationResult
{
    private readonly string[] blockers;
    private readonly string[] warnings;

    public KandraRuntimeRegistrationInvocationResult(
        string stageVersion,
        string requestId,
        string dryRunStageVersion,
        string dryRunRequestId,
        string approvalId,
        bool dryRunAccepted,
        bool runtimeRegistrationCallContractProven,
        bool explicitRuntimeInvocationApprovalPresent,
        bool approvalMatchesDryRun,
        bool approvalKeepsNonRegistrationWritesFalse,
        bool invokerPresent,
        bool runtimeRegistrationInvocationAllowed,
        bool registrationMethodInvoked,
        bool canRegisterMethodsInvoked,
        bool createdOrActivatedKandraObject,
        bool runtimeRegistrationExecuted,
        bool runtimeRegistrationSucceeded,
        string kandraIsRegisteredStatus,
        string kandraTryGetMeshMemoryStatus,
        string registeredRendererIdentity,
        string registeredMeshMemoryIdentity,
        string modDirectory,
        string name,
        string meshDataSha256,
        string indicesDataSha256,
        string registrationMethodFingerprint,
        string registrationAssemblySha256,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings)
    {
        StageVersion = ContractValues.RequireText(stageVersion, nameof(stageVersion));
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        DryRunStageVersion = ContractValues.RequireText(dryRunStageVersion, nameof(dryRunStageVersion));
        DryRunRequestId = ContractValues.RequireText(dryRunRequestId, nameof(dryRunRequestId));
        ApprovalId = approvalId ?? string.Empty;
        DryRunAccepted = dryRunAccepted;
        RuntimeRegistrationCallContractProven = runtimeRegistrationCallContractProven;
        ExplicitRuntimeInvocationApprovalPresent = explicitRuntimeInvocationApprovalPresent;
        ApprovalMatchesDryRun = approvalMatchesDryRun;
        ApprovalKeepsNonRegistrationWritesFalse = approvalKeepsNonRegistrationWritesFalse;
        InvokerPresent = invokerPresent;
        RuntimeRegistrationInvocationAllowed = runtimeRegistrationInvocationAllowed;
        RegistrationMethodInvoked = registrationMethodInvoked;
        CanRegisterMethodsInvoked = canRegisterMethodsInvoked;
        CreatedOrActivatedKandraObject = createdOrActivatedKandraObject;
        RuntimeRegistrationExecuted = runtimeRegistrationExecuted;
        RuntimeRegistrationSucceeded = runtimeRegistrationSucceeded;
        KandraIsRegisteredStatus = kandraIsRegisteredStatus ?? string.Empty;
        KandraTryGetMeshMemoryStatus = kandraTryGetMeshMemoryStatus ?? string.Empty;
        RegisteredRendererIdentity = registeredRendererIdentity ?? string.Empty;
        RegisteredMeshMemoryIdentity = registeredMeshMemoryIdentity ?? string.Empty;
        ModDirectory = modDirectory ?? string.Empty;
        Name = name ?? string.Empty;
        MeshDataSha256 = meshDataSha256 ?? string.Empty;
        IndicesDataSha256 = indicesDataSha256 ?? string.Empty;
        RegistrationMethodFingerprint = registrationMethodFingerprint ?? string.Empty;
        RegistrationAssemblySha256 = registrationAssemblySha256 ?? string.Empty;
        this.blockers = CopyMessages(blockers);
        this.warnings = CopyMessages(warnings);
    }

    public string StageVersion { get; }

    public string RequestId { get; }

    public string DryRunStageVersion { get; }

    public string DryRunRequestId { get; }

    public string ApprovalId { get; }

    public bool DryRunAccepted { get; }

    public bool RuntimeRegistrationCallContractProven { get; }

    public bool ExplicitRuntimeInvocationApprovalPresent { get; }

    public bool ApprovalMatchesDryRun { get; }

    public bool ApprovalKeepsNonRegistrationWritesFalse { get; }

    public bool InvokerPresent { get; }

    public bool RuntimeRegistrationInvocationAllowed { get; }

    public bool RegistrationMethodInvoked { get; }

    public bool CanRegisterMethodsInvoked { get; }

    public bool CreatedOrActivatedKandraObject { get; }

    public bool RuntimeRegistrationExecuted { get; }

    public bool RuntimeRegistrationSucceeded { get; }

    public bool CandidateMapApplicationAllowed => false;

    public bool CandidateMapApplicationExecuted => false;

    public bool ConversionAllowed => false;

    public bool ConversionExecuted => false;

    public bool ItemEquipMutationExecuted => false;

    public bool SaveMutationExecuted => false;

    public bool NativeGameWriteExecuted => false;

    public bool NonRegistrationWritesExecuted =>
        CandidateMapApplicationExecuted ||
        ConversionExecuted ||
        ItemEquipMutationExecuted ||
        SaveMutationExecuted ||
        NativeGameWriteExecuted;

    public string KandraIsRegisteredStatus { get; }

    public string KandraTryGetMeshMemoryStatus { get; }

    public string RegisteredRendererIdentity { get; }

    public string RegisteredMeshMemoryIdentity { get; }

    public string ModDirectory { get; }

    public string Name { get; }

    public string MeshDataSha256 { get; }

    public string IndicesDataSha256 { get; }

    public string RegistrationMethodFingerprint { get; }

    public string RegistrationAssemblySha256 { get; }

    public IReadOnlyList<string> Blockers => blockers;

    public IReadOnlyList<string> Warnings => warnings;

    private static string[] CopyMessages(IEnumerable<string> values) =>
        values?.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToArray()
        ?? Array.Empty<string>();
}

public sealed class KandraRuntimeRegistrationInvocationStage
{
    public const string StageVersion = "tainted-armour.kandra-runtime-registration-invocation.v1";

    public KandraRuntimeRegistrationInvocationResult Invoke(KandraRuntimeRegistrationInvocationRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        KandraRuntimeRegistrationDryRunResult dryRun = request.DryRun;
        KandraRuntimeInvocationApproval approval = request.Approval;
        var blockers = new List<string>();
        var warnings = new List<string>();

        CopyMessages("dry-run:", dryRun.Blockers, blockers);
        CopyMessages("dry-run:", dryRun.Warnings, warnings);

        bool dryRunAccepted = dryRun.RegistrationDryRunAccepted;
        if (!dryRunAccepted)
        {
            blockers.Add("kandra_runtime_registration_invocation_dry_run_not_accepted");
        }

        bool contractProven = dryRun.RuntimeRegistrationCallContractProven;
        if (!contractProven)
        {
            blockers.Add("kandra_runtime_registration_invocation_contract_not_proven");
        }

        bool explicitApprovalPresent = approval.Approved && approval.AllowRuntimeRegistrationInvocation;
        if (!explicitApprovalPresent)
        {
            blockers.Add("kandra_runtime_registration_invocation_explicit_approval_missing");
        }

        bool approvalMatchesDryRun = ApprovalMatchesDryRun(approval, dryRun, blockers);
        bool approvalKeepsNonRegistrationWritesFalse = ApprovalKeepsNonRegistrationWritesFalse(approval);
        if (!approvalKeepsNonRegistrationWritesFalse)
        {
            blockers.Add("kandra_runtime_registration_invocation_approval_requested_non_registration_writes");
        }

        bool invokerPresent = request.Invoker != null;
        if (!invokerPresent)
        {
            blockers.Add("kandra_runtime_registration_invocation_host_invoker_missing");
        }

        KandraRuntimeRegistrationInvocationAttempt? attempt = null;
        bool allowed = blockers.Count == 0;
        if (allowed)
        {
            try
            {
                attempt = request.Invoker!.Invoke(CreateContext(request, dryRun, approval));
                if (attempt == null)
                {
                    blockers.Add("kandra_runtime_registration_invocation_attempt_missing");
                    allowed = false;
                }
                else
                {
                    CopyMessages("attempt:", attempt.Blockers, blockers);
                    CopyMessages("attempt:", attempt.Warnings, warnings);
                    ValidateAttempt(attempt, blockers);
                    allowed = blockers.Count == 0;
                }
            }
            catch (Exception exception)
            {
                blockers.Add("kandra_runtime_registration_invocation_exception:" + exception.GetType().Name);
                warnings.Add(exception.Message);
                allowed = false;
            }
        }

        return new KandraRuntimeRegistrationInvocationResult(
            StageVersion,
            request.RequestId,
            dryRun.StageVersion,
            dryRun.RequestId,
            approval.ApprovalId,
            dryRunAccepted,
            contractProven,
            explicitApprovalPresent,
            approvalMatchesDryRun,
            approvalKeepsNonRegistrationWritesFalse,
            invokerPresent,
            allowed,
            attempt?.RegistrationMethodInvoked ?? false,
            attempt?.CanRegisterMethodsInvoked ?? false,
            attempt?.CreatedOrActivatedKandraObject ?? false,
            attempt?.RuntimeRegistrationExecuted ?? false,
            allowed && (attempt?.RuntimeRegistrationSucceeded ?? false),
            attempt?.KandraIsRegisteredStatus ?? string.Empty,
            attempt?.KandraTryGetMeshMemoryStatus ?? string.Empty,
            attempt?.RegisteredRendererIdentity ?? string.Empty,
            attempt?.RegisteredMeshMemoryIdentity ?? string.Empty,
            dryRun.ModDirectory,
            dryRun.Name,
            dryRun.MeshDataSha256,
            dryRun.IndicesDataSha256,
            dryRun.RegistrationMethodFingerprint,
            dryRun.RegistrationAssemblySha256,
            blockers,
            warnings);
    }

    private static bool ApprovalMatchesDryRun(
        KandraRuntimeInvocationApproval approval,
        KandraRuntimeRegistrationDryRunResult dryRun,
        List<string> blockers)
    {
        bool matches = true;
        matches &= Compare(approval.DryRunRequestId, dryRun.RequestId, "dry_run_request_id", blockers);
        matches &= Compare(approval.ModDirectory, dryRun.ModDirectory, "mod_directory", blockers);
        matches &= Compare(approval.Name, dryRun.Name, "name", blockers);
        matches &= Compare(approval.MeshDataSha256, dryRun.MeshDataSha256, "mesh_data_sha256", blockers);
        matches &= Compare(approval.IndicesDataSha256, dryRun.IndicesDataSha256, "indices_data_sha256", blockers);
        matches &= Compare(approval.RegistrationMethodFingerprint, dryRun.RegistrationMethodFingerprint, "registration_method_fingerprint", blockers);
        matches &= Compare(approval.RegistrationAssemblySha256, dryRun.RegistrationAssemblySha256, "registration_assembly_sha256", blockers);
        return matches;
    }

    private static bool ApprovalKeepsNonRegistrationWritesFalse(KandraRuntimeInvocationApproval approval) =>
        !approval.AllowCandidateMapApplication &&
        !approval.AllowConversion &&
        !approval.AllowItemEquipMutation &&
        !approval.AllowSaveMutation &&
        !approval.AllowNativeGameWrite;

    private static KandraRuntimeRegistrationInvocationContext CreateContext(
        KandraRuntimeRegistrationInvocationRequest request,
        KandraRuntimeRegistrationDryRunResult dryRun,
        KandraRuntimeInvocationApproval approval) =>
        new KandraRuntimeRegistrationInvocationContext(
            request.RequestId,
            dryRun.RequestId,
            approval.ApprovalId,
            dryRun.ModDirectory,
            dryRun.Name,
            dryRun.VertexCount,
            dryRun.IndicesCount,
            dryRun.BindposesCount,
            dryRun.BlendshapeCount,
            dryRun.LayoutVersion,
            dryRun.KandraDirectory,
            dryRun.MeshDataPath,
            dryRun.IndicesDataPath,
            dryRun.MeshDataByteCount,
            dryRun.IndicesDataByteCount,
            dryRun.MeshDataSha256,
            dryRun.IndicesDataSha256,
            dryRun.RegistrationOwner,
            dryRun.RegistrationMethodSignature,
            dryRun.RegistrationMethodFingerprint,
            dryRun.RegistrationAssemblySha256);

    private static void ValidateAttempt(
        KandraRuntimeRegistrationInvocationAttempt attempt,
        List<string> blockers)
    {
        if (!attempt.RegistrationMethodInvoked)
        {
            blockers.Add("kandra_runtime_registration_invocation_register_not_invoked");
        }

        if (!attempt.RuntimeRegistrationExecuted)
        {
            blockers.Add("kandra_runtime_registration_invocation_runtime_registration_not_executed");
        }

        if (!attempt.RuntimeRegistrationSucceeded)
        {
            blockers.Add("kandra_runtime_registration_invocation_runtime_registration_not_successful");
        }

        if (!IsObservedTrue(attempt.KandraIsRegisteredStatus))
        {
            blockers.Add("kandra_runtime_registration_invocation_is_registered_not_proven");
        }

        if (!IsObservedTrue(attempt.KandraTryGetMeshMemoryStatus))
        {
            blockers.Add("kandra_runtime_registration_invocation_mesh_memory_not_proven");
        }
    }

    private static bool Compare(string actual, string expected, string field, List<string> blockers)
    {
        if (string.Equals(actual, expected, StringComparison.Ordinal))
        {
            return true;
        }

        blockers.Add("kandra_runtime_registration_invocation_approval_mismatch:" + field);
        return false;
    }

    private static bool IsObservedTrue(string status) =>
        status.StartsWith("observed:True", StringComparison.OrdinalIgnoreCase) ||
        status.StartsWith("observed:true", StringComparison.OrdinalIgnoreCase);

    private static void CopyMessages(string prefix, IEnumerable<string> source, List<string> target)
    {
        foreach (string message in source ?? Array.Empty<string>())
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                target.Add(prefix + message.Trim());
            }
        }
    }
}
