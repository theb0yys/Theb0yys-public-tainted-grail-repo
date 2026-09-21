using System;
using System.Collections.Generic;
using System.Linq;

namespace Tainted.Armour;

public sealed class KandraRuntimeRegistrationHostReceipt
{
    public KandraRuntimeRegistrationHostReceipt(
        string stageVersion,
        string writtenAtUtc,
        string trigger,
        KandraRuntimeRegistrationInvocationContext context,
        KandraRuntimeRegistrationInvocationAttempt attempt,
        bool candidateMapApplicationExecuted,
        bool conversionExecuted,
        bool itemEquipMutationExecuted,
        bool saveMutationExecuted,
        bool nativeGameWriteExecuted,
        bool nonRegistrationDownstreamWritesExecuted)
    {
        StageVersion = ContractValues.RequireText(stageVersion, nameof(stageVersion));
        WrittenAtUtc = ContractValues.RequireText(writtenAtUtc, nameof(writtenAtUtc));
        Trigger = ContractValues.RequireText(trigger, nameof(trigger));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Attempt = attempt ?? throw new ArgumentNullException(nameof(attempt));
        CandidateMapApplicationExecuted = candidateMapApplicationExecuted;
        ConversionExecuted = conversionExecuted;
        ItemEquipMutationExecuted = itemEquipMutationExecuted;
        SaveMutationExecuted = saveMutationExecuted;
        NativeGameWriteExecuted = nativeGameWriteExecuted;
        NonRegistrationDownstreamWritesExecuted = nonRegistrationDownstreamWritesExecuted;
    }

    public string StageVersion { get; }

    public string WrittenAtUtc { get; }

    public string Trigger { get; }

    public KandraRuntimeRegistrationInvocationContext Context { get; }

    public KandraRuntimeRegistrationInvocationAttempt Attempt { get; }

    public bool CandidateMapApplicationExecuted { get; }

    public bool ConversionExecuted { get; }

    public bool ItemEquipMutationExecuted { get; }

    public bool SaveMutationExecuted { get; }

    public bool NativeGameWriteExecuted { get; }

    public bool NonRegistrationDownstreamWritesExecuted { get; }

    public bool NonRegistrationDownstreamBoundaryFalse =>
        !CandidateMapApplicationExecuted &&
        !ConversionExecuted &&
        !ItemEquipMutationExecuted &&
        !SaveMutationExecuted &&
        !NativeGameWriteExecuted &&
        !NonRegistrationDownstreamWritesExecuted;
}

public sealed class KandraRuntimeRegistrationHostProofRequest
{
    public KandraRuntimeRegistrationHostProofRequest(
        string requestId,
        KandraRuntimeRegistrationHostReceipt receipt)
    {
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        Receipt = receipt ?? throw new ArgumentNullException(nameof(receipt));
    }

    public string RequestId { get; }

    public KandraRuntimeRegistrationHostReceipt Receipt { get; }
}

public sealed class KandraRuntimeRegistrationHostProofResult
{
    private readonly string[] blockers;
    private readonly string[] warnings;

    public KandraRuntimeRegistrationHostProofResult(
        string stageVersion,
        string requestId,
        string receiptStageVersion,
        string receiptWrittenAtUtc,
        string receiptTrigger,
        bool receiptStageVersionAccepted,
        bool registrationMethodFingerprintAccepted,
        bool registrationAssemblyFingerprintAccepted,
        bool contextCountsValid,
        bool registrationMethodInvoked,
        bool canRegisterMethodsInvoked,
        bool createdOrActivatedKandraObject,
        bool hostRuntimeRegistrationExecuted,
        bool hostRuntimeRegistrationSucceeded,
        bool kandraIsRegisteredProofPresent,
        bool kandraMeshMemoryProofPresent,
        bool attemptBlockerFree,
        bool nonRegistrationDownstreamBoundaryFalse,
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
        string registeredRendererIdentity,
        string registeredMeshMemoryIdentity,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings)
    {
        StageVersion = ContractValues.RequireText(stageVersion, nameof(stageVersion));
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        ReceiptStageVersion = ContractValues.RequireText(receiptStageVersion, nameof(receiptStageVersion));
        ReceiptWrittenAtUtc = ContractValues.RequireText(receiptWrittenAtUtc, nameof(receiptWrittenAtUtc));
        ReceiptTrigger = ContractValues.RequireText(receiptTrigger, nameof(receiptTrigger));
        ReceiptStageVersionAccepted = receiptStageVersionAccepted;
        RegistrationMethodFingerprintAccepted = registrationMethodFingerprintAccepted;
        RegistrationAssemblyFingerprintAccepted = registrationAssemblyFingerprintAccepted;
        ContextCountsValid = contextCountsValid;
        RegistrationMethodInvoked = registrationMethodInvoked;
        CanRegisterMethodsInvoked = canRegisterMethodsInvoked;
        CreatedOrActivatedKandraObject = createdOrActivatedKandraObject;
        HostRuntimeRegistrationExecuted = hostRuntimeRegistrationExecuted;
        HostRuntimeRegistrationSucceeded = hostRuntimeRegistrationSucceeded;
        KandraIsRegisteredProofPresent = kandraIsRegisteredProofPresent;
        KandraMeshMemoryProofPresent = kandraMeshMemoryProofPresent;
        AttemptBlockerFree = attemptBlockerFree;
        NonRegistrationDownstreamBoundaryFalse = nonRegistrationDownstreamBoundaryFalse;
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
        RegisteredRendererIdentity = registeredRendererIdentity ?? string.Empty;
        RegisteredMeshMemoryIdentity = registeredMeshMemoryIdentity ?? string.Empty;
        this.blockers = CopyMessages(blockers);
        this.warnings = CopyMessages(warnings);
    }

    public string StageVersion { get; }

    public string RequestId { get; }

    public string ReceiptStageVersion { get; }

    public string ReceiptWrittenAtUtc { get; }

    public string ReceiptTrigger { get; }

    public bool ReceiptStageVersionAccepted { get; }

    public bool RegistrationMethodFingerprintAccepted { get; }

    public bool RegistrationAssemblyFingerprintAccepted { get; }

    public bool ContextCountsValid { get; }

    public bool RegistrationMethodInvoked { get; }

    public bool CanRegisterMethodsInvoked { get; }

    public bool CreatedOrActivatedKandraObject { get; }

    public bool HostRuntimeRegistrationExecuted { get; }

    public bool HostRuntimeRegistrationSucceeded { get; }

    public bool KandraIsRegisteredProofPresent { get; }

    public bool KandraMeshMemoryProofPresent { get; }

    public bool AttemptBlockerFree { get; }

    public bool NonRegistrationDownstreamBoundaryFalse { get; }

    public bool HostInvocationProofAccepted => blockers.Length == 0;

    public bool CandidateMapApplicationAllowed => false;

    public bool CandidateMapApplicationExecuted => false;

    public bool ConversionAllowed => false;

    public bool ConversionExecuted => false;

    public bool ItemEquipMutationExecuted => false;

    public bool SaveMutationExecuted => false;

    public bool NativeGameWriteExecuted => false;

    public bool DownstreamWritesExecuted => false;

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

    public string RegisteredRendererIdentity { get; }

    public string RegisteredMeshMemoryIdentity { get; }

    public IReadOnlyList<string> Blockers => blockers;

    public IReadOnlyList<string> Warnings => warnings;

    private static string[] CopyMessages(IEnumerable<string> values) =>
        values?.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToArray()
        ?? Array.Empty<string>();
}

public sealed class KandraRuntimeRegistrationHostProofStage
{
    public const string StageVersion = "tainted-armour.kandra-runtime-registration-host-proof.v1";
    public const string ExpectedFoaInvokerReceiptStageVersion = "tainted-armour.foa.kandra-runtime-registration-invoker.v1";

    public KandraRuntimeRegistrationHostProofResult Validate(KandraRuntimeRegistrationHostProofRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        KandraRuntimeRegistrationHostReceipt receipt = request.Receipt;
        KandraRuntimeRegistrationInvocationContext context = receipt.Context;
        KandraRuntimeRegistrationInvocationAttempt attempt = receipt.Attempt;
        var blockers = new List<string>();
        var warnings = new List<string>();

        CopyMessages("attempt:", attempt.Blockers, blockers);
        CopyMessages("attempt:", attempt.Warnings, warnings);

        bool receiptStageVersionAccepted = string.Equals(
            receipt.StageVersion,
            ExpectedFoaInvokerReceiptStageVersion,
            StringComparison.Ordinal);
        if (!receiptStageVersionAccepted)
        {
            blockers.Add("kandra_runtime_registration_host_proof_receipt_stage_version_mismatch");
        }

        bool registrationMethodFingerprintAccepted = string.Equals(
            context.RegistrationMethodFingerprint,
            KandraRuntimeRegistrationContractFingerprint.ExpectedRegistrationMethodFingerprint,
            StringComparison.OrdinalIgnoreCase);
        if (!registrationMethodFingerprintAccepted)
        {
            blockers.Add("kandra_runtime_registration_host_proof_register_fingerprint_mismatch");
        }

        bool registrationAssemblyFingerprintAccepted = string.Equals(
            context.RegistrationAssemblySha256,
            KandraRuntimeRegistrationContractFingerprint.ExpectedAssemblySha256,
            StringComparison.OrdinalIgnoreCase);
        if (!registrationAssemblyFingerprintAccepted)
        {
            blockers.Add("kandra_runtime_registration_host_proof_assembly_fingerprint_mismatch");
        }

        bool contextCountsValid =
            context.VertexCount > 0 &&
            context.IndicesCount >= 0L &&
            context.BindposesCount >= 0 &&
            context.BlendshapeCount >= 0 &&
            context.MeshDataByteCount >= 0L &&
            context.IndicesDataByteCount >= 0L;
        if (!contextCountsValid)
        {
            blockers.Add("kandra_runtime_registration_host_proof_context_counts_invalid");
        }

        if (!attempt.RegistrationMethodInvoked)
        {
            blockers.Add("kandra_runtime_registration_host_proof_register_not_invoked");
        }

        if (!attempt.CreatedOrActivatedKandraObject)
        {
            blockers.Add("kandra_runtime_registration_host_proof_runtime_object_not_created_or_activated");
        }

        if (!attempt.RuntimeRegistrationExecuted)
        {
            blockers.Add("kandra_runtime_registration_host_proof_runtime_registration_not_executed");
        }

        if (!attempt.RuntimeRegistrationSucceeded)
        {
            blockers.Add("kandra_runtime_registration_host_proof_runtime_registration_not_successful");
        }

        bool kandraIsRegisteredProofPresent = IsObservedTrue(attempt.KandraIsRegisteredStatus);
        if (!kandraIsRegisteredProofPresent)
        {
            blockers.Add("kandra_runtime_registration_host_proof_is_registered_missing");
        }

        bool kandraMeshMemoryProofPresent = IsObservedTrue(attempt.KandraTryGetMeshMemoryStatus);
        if (!kandraMeshMemoryProofPresent)
        {
            blockers.Add("kandra_runtime_registration_host_proof_mesh_memory_missing");
        }

        bool attemptBlockerFree = attempt.Blockers.Count == 0;
        if (!attemptBlockerFree)
        {
            blockers.Add("kandra_runtime_registration_host_proof_attempt_has_blockers");
        }

        bool nonRegistrationDownstreamBoundaryFalse = receipt.NonRegistrationDownstreamBoundaryFalse;
        if (!nonRegistrationDownstreamBoundaryFalse)
        {
            blockers.Add("kandra_runtime_registration_host_proof_non_registration_downstream_boundary_not_false");
        }

        return new KandraRuntimeRegistrationHostProofResult(
            StageVersion,
            request.RequestId,
            receipt.StageVersion,
            receipt.WrittenAtUtc,
            receipt.Trigger,
            receiptStageVersionAccepted,
            registrationMethodFingerprintAccepted,
            registrationAssemblyFingerprintAccepted,
            contextCountsValid,
            attempt.RegistrationMethodInvoked,
            attempt.CanRegisterMethodsInvoked,
            attempt.CreatedOrActivatedKandraObject,
            attempt.RuntimeRegistrationExecuted,
            attempt.RuntimeRegistrationSucceeded,
            kandraIsRegisteredProofPresent,
            kandraMeshMemoryProofPresent,
            attemptBlockerFree,
            nonRegistrationDownstreamBoundaryFalse,
            context.ModDirectory,
            context.Name,
            context.VertexCount,
            context.IndicesCount,
            context.BindposesCount,
            context.BlendshapeCount,
            context.LayoutVersion,
            context.KandraDirectory,
            context.MeshDataPath,
            context.IndicesDataPath,
            context.MeshDataByteCount,
            context.IndicesDataByteCount,
            context.MeshDataSha256,
            context.IndicesDataSha256,
            context.RegistrationOwner,
            context.RegistrationMethodSignature,
            context.RegistrationMethodFingerprint,
            context.RegistrationAssemblySha256,
            attempt.RegisteredRendererIdentity,
            attempt.RegisteredMeshMemoryIdentity,
            blockers,
            warnings);
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

public sealed class KandraTargetRuntimeRegistrationPlanRequest
{
    public KandraTargetRuntimeRegistrationPlanRequest(
        string requestId,
        KandraRuntimeRegistrationHostProofResult hostProof,
        KandraRuntimeRegistrationDryRunResult targetDryRun,
        KandraRuntimeInvocationApproval approval)
    {
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        HostProof = hostProof ?? throw new ArgumentNullException(nameof(hostProof));
        TargetDryRun = targetDryRun ?? throw new ArgumentNullException(nameof(targetDryRun));
        Approval = approval ?? throw new ArgumentNullException(nameof(approval));
    }

    public string RequestId { get; }

    public KandraRuntimeRegistrationHostProofResult HostProof { get; }

    public KandraRuntimeRegistrationDryRunResult TargetDryRun { get; }

    public KandraRuntimeInvocationApproval Approval { get; }
}

public sealed class KandraTargetRuntimeRegistrationPlanResult
{
    private readonly string[] blockers;
    private readonly string[] warnings;

    public KandraTargetRuntimeRegistrationPlanResult(
        string stageVersion,
        string requestId,
        string hostProofStageVersion,
        string hostProofRequestId,
        string targetDryRunStageVersion,
        string targetDryRunRequestId,
        string approvalId,
        bool hostInvocationProofAccepted,
        bool targetDryRunAccepted,
        bool targetRuntimeRegistrationCallContractProven,
        bool targetPackageDiffersFromHostProofIdentity,
        bool targetPayloadDiffersFromHostProofPayload,
        bool explicitRuntimeInvocationApprovalPresent,
        bool approvalMatchesTargetDryRun,
        bool approvalKeepsNonRegistrationWritesFalse,
        bool targetInvocationContextConstructed,
        KandraRuntimeRegistrationInvocationContext? targetInvocationContext,
        string targetModDirectory,
        string targetName,
        string targetMeshDataSha256,
        string targetIndicesDataSha256,
        string registrationMethodFingerprint,
        string registrationAssemblySha256,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings)
    {
        StageVersion = ContractValues.RequireText(stageVersion, nameof(stageVersion));
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        HostProofStageVersion = ContractValues.RequireText(hostProofStageVersion, nameof(hostProofStageVersion));
        HostProofRequestId = ContractValues.RequireText(hostProofRequestId, nameof(hostProofRequestId));
        TargetDryRunStageVersion = ContractValues.RequireText(targetDryRunStageVersion, nameof(targetDryRunStageVersion));
        TargetDryRunRequestId = ContractValues.RequireText(targetDryRunRequestId, nameof(targetDryRunRequestId));
        ApprovalId = approvalId ?? string.Empty;
        HostInvocationProofAccepted = hostInvocationProofAccepted;
        TargetDryRunAccepted = targetDryRunAccepted;
        TargetRuntimeRegistrationCallContractProven = targetRuntimeRegistrationCallContractProven;
        TargetPackageDiffersFromHostProofIdentity = targetPackageDiffersFromHostProofIdentity;
        TargetPayloadDiffersFromHostProofPayload = targetPayloadDiffersFromHostProofPayload;
        ExplicitRuntimeInvocationApprovalPresent = explicitRuntimeInvocationApprovalPresent;
        ApprovalMatchesTargetDryRun = approvalMatchesTargetDryRun;
        ApprovalKeepsNonRegistrationWritesFalse = approvalKeepsNonRegistrationWritesFalse;
        TargetInvocationContextConstructed = targetInvocationContextConstructed;
        TargetInvocationContext = targetInvocationContext;
        TargetModDirectory = targetModDirectory ?? string.Empty;
        TargetName = targetName ?? string.Empty;
        TargetMeshDataSha256 = targetMeshDataSha256 ?? string.Empty;
        TargetIndicesDataSha256 = targetIndicesDataSha256 ?? string.Empty;
        RegistrationMethodFingerprint = registrationMethodFingerprint ?? string.Empty;
        RegistrationAssemblySha256 = registrationAssemblySha256 ?? string.Empty;
        this.blockers = CopyMessages(blockers);
        this.warnings = CopyMessages(warnings);
    }

    public string StageVersion { get; }

    public string RequestId { get; }

    public string HostProofStageVersion { get; }

    public string HostProofRequestId { get; }

    public string TargetDryRunStageVersion { get; }

    public string TargetDryRunRequestId { get; }

    public string ApprovalId { get; }

    public bool HostInvocationProofAccepted { get; }

    public bool TargetDryRunAccepted { get; }

    public bool TargetRuntimeRegistrationCallContractProven { get; }

    public bool TargetPackageDiffersFromHostProofIdentity { get; }

    public bool TargetPayloadDiffersFromHostProofPayload { get; }

    public bool ExplicitRuntimeInvocationApprovalPresent { get; }

    public bool ApprovalMatchesTargetDryRun { get; }

    public bool ApprovalKeepsNonRegistrationWritesFalse { get; }

    public bool TargetInvocationContextConstructed { get; }

    public KandraRuntimeRegistrationInvocationContext? TargetInvocationContext { get; }

    public bool TargetRuntimeRegistrationPlanAccepted => blockers.Length == 0 && TargetInvocationContextConstructed;

    public bool RuntimeRegistrationInvocationAllowed => TargetRuntimeRegistrationPlanAccepted;

    public bool RegistrationMethodInvoked => false;

    public bool CanRegisterMethodsInvoked => false;

    public bool CreatedOrActivatedKandraObject => false;

    public bool RuntimeRegistrationExecuted => false;

    public bool RuntimeRegistrationSucceeded => false;

    public bool CandidateMapApplicationAllowed => false;

    public bool CandidateMapApplicationExecuted => false;

    public bool ConversionAllowed => false;

    public bool ConversionExecuted => false;

    public bool ItemEquipMutationExecuted => false;

    public bool SaveMutationExecuted => false;

    public bool NativeGameWriteExecuted => false;

    public bool DownstreamWritesExecuted => false;

    public string TargetModDirectory { get; }

    public string TargetName { get; }

    public string TargetMeshDataSha256 { get; }

    public string TargetIndicesDataSha256 { get; }

    public string RegistrationMethodFingerprint { get; }

    public string RegistrationAssemblySha256 { get; }

    public IReadOnlyList<string> Blockers => blockers;

    public IReadOnlyList<string> Warnings => warnings;

    private static string[] CopyMessages(IEnumerable<string> values) =>
        values?.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToArray()
        ?? Array.Empty<string>();
}

public sealed class KandraTargetRuntimeRegistrationPlanStage
{
    public const string StageVersion = "tainted-armour.kandra-target-runtime-registration-plan.v1";

    public KandraTargetRuntimeRegistrationPlanResult Plan(KandraTargetRuntimeRegistrationPlanRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        KandraRuntimeRegistrationHostProofResult hostProof = request.HostProof;
        KandraRuntimeRegistrationDryRunResult targetDryRun = request.TargetDryRun;
        KandraRuntimeInvocationApproval approval = request.Approval;
        var blockers = new List<string>();
        var warnings = new List<string>();

        CopyMessages("host-proof:", hostProof.Blockers, blockers);
        CopyMessages("host-proof:", hostProof.Warnings, warnings);
        CopyMessages("target-dry-run:", targetDryRun.Blockers, blockers);
        CopyMessages("target-dry-run:", targetDryRun.Warnings, warnings);

        bool hostInvocationProofAccepted = hostProof.HostInvocationProofAccepted;
        if (!hostInvocationProofAccepted)
        {
            blockers.Add("kandra_target_runtime_registration_plan_host_proof_not_accepted");
        }

        bool targetDryRunAccepted = targetDryRun.RegistrationDryRunAccepted;
        if (!targetDryRunAccepted)
        {
            blockers.Add("kandra_target_runtime_registration_plan_target_dry_run_not_accepted");
        }

        bool targetRuntimeRegistrationCallContractProven = targetDryRun.RuntimeRegistrationCallContractProven;
        if (!targetRuntimeRegistrationCallContractProven)
        {
            blockers.Add("kandra_target_runtime_registration_plan_contract_not_proven");
        }

        bool targetPackageDiffersFromHostProofIdentity =
            !string.Equals(targetDryRun.ModDirectory, hostProof.ModDirectory, StringComparison.Ordinal) ||
            !string.Equals(targetDryRun.Name, hostProof.Name, StringComparison.Ordinal);
        if (!targetPackageDiffersFromHostProofIdentity)
        {
            blockers.Add("kandra_target_runtime_registration_plan_reuses_host_proof_package_identity");
        }

        bool targetPayloadDiffersFromHostProofPayload =
            !string.Equals(NormalizeSha256(targetDryRun.MeshDataSha256), NormalizeSha256(hostProof.MeshDataSha256), StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(NormalizeSha256(targetDryRun.IndicesDataSha256), NormalizeSha256(hostProof.IndicesDataSha256), StringComparison.OrdinalIgnoreCase);
        if (!targetPayloadDiffersFromHostProofPayload)
        {
            blockers.Add("kandra_target_runtime_registration_plan_reuses_host_proof_payload_hashes");
        }

        bool explicitRuntimeInvocationApprovalPresent = approval.Approved && approval.AllowRuntimeRegistrationInvocation;
        if (!explicitRuntimeInvocationApprovalPresent)
        {
            blockers.Add("kandra_target_runtime_registration_plan_explicit_approval_missing");
        }

        bool approvalMatchesTargetDryRun = ApprovalMatchesTargetDryRun(approval, targetDryRun, blockers);
        bool approvalKeepsNonRegistrationWritesFalse = ApprovalKeepsNonRegistrationWritesFalse(approval);
        if (!approvalKeepsNonRegistrationWritesFalse)
        {
            blockers.Add("kandra_target_runtime_registration_plan_approval_requested_non_registration_writes");
        }

        KandraRuntimeRegistrationInvocationContext? targetContext = null;
        if (blockers.Count == 0)
        {
            targetContext = CreateContext(request.RequestId, targetDryRun, approval);
        }

        return new KandraTargetRuntimeRegistrationPlanResult(
            StageVersion,
            request.RequestId,
            hostProof.StageVersion,
            hostProof.RequestId,
            targetDryRun.StageVersion,
            targetDryRun.RequestId,
            approval.ApprovalId,
            hostInvocationProofAccepted,
            targetDryRunAccepted,
            targetRuntimeRegistrationCallContractProven,
            targetPackageDiffersFromHostProofIdentity,
            targetPayloadDiffersFromHostProofPayload,
            explicitRuntimeInvocationApprovalPresent,
            approvalMatchesTargetDryRun,
            approvalKeepsNonRegistrationWritesFalse,
            targetContext != null,
            targetContext,
            targetDryRun.ModDirectory,
            targetDryRun.Name,
            targetDryRun.MeshDataSha256,
            targetDryRun.IndicesDataSha256,
            targetDryRun.RegistrationMethodFingerprint,
            targetDryRun.RegistrationAssemblySha256,
            blockers,
            warnings);
    }

    private static bool ApprovalMatchesTargetDryRun(
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
        string requestId,
        KandraRuntimeRegistrationDryRunResult dryRun,
        KandraRuntimeInvocationApproval approval) =>
        new KandraRuntimeRegistrationInvocationContext(
            requestId,
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

    private static bool Compare(string actual, string expected, string field, List<string> blockers)
    {
        if (string.Equals(actual, expected, StringComparison.Ordinal))
        {
            return true;
        }

        blockers.Add("kandra_target_runtime_registration_plan_approval_mismatch:" + field);
        return false;
    }

    private static string NormalizeSha256(string value)
    {
        string trimmed = (value ?? string.Empty).Trim();
        return trimmed.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)
            ? trimmed.Substring("sha256:".Length)
            : trimmed;
    }

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
