using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Tainted.Armour;

public sealed class KandraPackageValidationRequest
{
    public KandraPackageValidationRequest(
        string requestId,
        KandraWriterResult writerResult,
        ArmorImportResult liveKandraProof)
    {
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        WriterResult = writerResult ?? throw new ArgumentNullException(nameof(writerResult));
        LiveKandraProof = liveKandraProof ?? throw new ArgumentNullException(nameof(liveKandraProof));
    }

    public string RequestId { get; }

    public KandraWriterResult WriterResult { get; }

    public ArmorImportResult LiveKandraProof { get; }
}

public sealed class KandraPackageValidationResult
{
    private readonly string[] blockers;
    private readonly string[] warnings;

    public KandraPackageValidationResult(
        string stageVersion,
        string requestId,
        string writerStageVersion,
        string layoutVersion,
        string modDirectory,
        string meshName,
        string kandraDirectory,
        string meshDataPath,
        string indicesDataPath,
        long meshDataByteCount,
        long indicesDataByteCount,
        string meshDataSha256,
        string indicesDataSha256,
        bool runtimeSeamValid,
        bool meshDataFilePresent,
        bool indicesDataFilePresent,
        bool meshDataLengthMatches,
        bool indicesDataLengthMatches,
        bool meshDataHashMatches,
        bool indicesDataHashMatches,
        bool liveKandraRegistrationProofPresent,
        bool liveKandraMeshMemoryProofPresent,
        bool liveProofDownstreamBoundaryFalse,
        bool writerDownstreamBoundaryFalse,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings)
    {
        StageVersion = ContractValues.RequireText(stageVersion, nameof(stageVersion));
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        WriterStageVersion = ContractValues.RequireText(writerStageVersion, nameof(writerStageVersion));
        LayoutVersion = ContractValues.RequireText(layoutVersion, nameof(layoutVersion));
        ModDirectory = ContractValues.RequireText(modDirectory, nameof(modDirectory));
        MeshName = ContractValues.RequireText(meshName, nameof(meshName));
        KandraDirectory = ContractValues.RequireText(kandraDirectory, nameof(kandraDirectory));
        MeshDataPath = ContractValues.RequireText(meshDataPath, nameof(meshDataPath));
        IndicesDataPath = ContractValues.RequireText(indicesDataPath, nameof(indicesDataPath));
        MeshDataByteCount = meshDataByteCount;
        IndicesDataByteCount = indicesDataByteCount;
        MeshDataSha256 = ContractValues.RequireText(meshDataSha256, nameof(meshDataSha256));
        IndicesDataSha256 = ContractValues.RequireText(indicesDataSha256, nameof(indicesDataSha256));
        RuntimeSeamValid = runtimeSeamValid;
        MeshDataFilePresent = meshDataFilePresent;
        IndicesDataFilePresent = indicesDataFilePresent;
        MeshDataLengthMatches = meshDataLengthMatches;
        IndicesDataLengthMatches = indicesDataLengthMatches;
        MeshDataHashMatches = meshDataHashMatches;
        IndicesDataHashMatches = indicesDataHashMatches;
        LiveKandraRegistrationProofPresent = liveKandraRegistrationProofPresent;
        LiveKandraMeshMemoryProofPresent = liveKandraMeshMemoryProofPresent;
        LiveProofDownstreamBoundaryFalse = liveProofDownstreamBoundaryFalse;
        WriterDownstreamBoundaryFalse = writerDownstreamBoundaryFalse;
        this.blockers = CopyMessages(blockers);
        this.warnings = CopyMessages(warnings);
    }

    public string StageVersion { get; }

    public string RequestId { get; }

    public string WriterStageVersion { get; }

    public string LayoutVersion { get; }

    public string ModDirectory { get; }

    public string MeshName { get; }

    public string KandraDirectory { get; }

    public string MeshDataPath { get; }

    public string IndicesDataPath { get; }

    public long MeshDataByteCount { get; }

    public long IndicesDataByteCount { get; }

    public string MeshDataSha256 { get; }

    public string IndicesDataSha256 { get; }

    public bool RuntimeSeamValid { get; }

    public bool MeshDataFilePresent { get; }

    public bool IndicesDataFilePresent { get; }

    public bool MeshDataLengthMatches { get; }

    public bool IndicesDataLengthMatches { get; }

    public bool MeshDataHashMatches { get; }

    public bool IndicesDataHashMatches { get; }

    public bool LiveKandraRegistrationProofPresent { get; }

    public bool LiveKandraMeshMemoryProofPresent { get; }

    public bool LiveProofDownstreamBoundaryFalse { get; }

    public bool WriterDownstreamBoundaryFalse { get; }

    public bool PackageValidationAccepted => blockers.Length == 0;

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

public sealed class KandraPackageValidationStage
{
    public const string StageVersion = "tainted-armour.kandra-package-validation.v1";

    public KandraPackageValidationResult Validate(KandraPackageValidationRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        KandraWriterResult writer = request.WriterResult;
        ArmorImportResult proof = request.LiveKandraProof;
        var blockers = new List<string>();
        var warnings = new List<string>();

        bool runtimeSeamValid = ValidateRuntimeSeam(writer, blockers);
        bool writerDownstreamFalse = IsWriterDownstreamFalse(writer);
        if (!writerDownstreamFalse)
        {
            blockers.Add("kandra_writer_downstream_boundary_not_false");
        }

        bool meshDataFilePresent = File.Exists(writer.MeshDataPath);
        if (!meshDataFilePresent)
        {
            blockers.Add("kandra_package_mesh_data_file_missing");
        }

        bool indicesDataFilePresent = File.Exists(writer.IndicesDataPath);
        if (!indicesDataFilePresent)
        {
            blockers.Add("kandra_package_indices_data_file_missing");
        }

        long meshDataByteCount = meshDataFilePresent ? FileLength(writer.MeshDataPath) : -1L;
        long indicesDataByteCount = indicesDataFilePresent ? FileLength(writer.IndicesDataPath) : -1L;
        string meshDataSha256 = meshDataFilePresent ? Sha256File(writer.MeshDataPath) : "unavailable:file_missing";
        string indicesDataSha256 = indicesDataFilePresent ? Sha256File(writer.IndicesDataPath) : "unavailable:file_missing";

        bool meshDataLengthMatches = meshDataFilePresent && meshDataByteCount == writer.MeshDataByteCount;
        if (meshDataFilePresent && !meshDataLengthMatches)
        {
            blockers.Add("kandra_package_mesh_data_length_mismatch");
        }

        bool indicesDataLengthMatches = indicesDataFilePresent && indicesDataByteCount == writer.IndicesDataByteCount;
        if (indicesDataFilePresent && !indicesDataLengthMatches)
        {
            blockers.Add("kandra_package_indices_data_length_mismatch");
        }

        bool meshDataHashMatches = meshDataFilePresent && string.Equals(meshDataSha256, writer.MeshDataSha256, StringComparison.OrdinalIgnoreCase);
        if (meshDataFilePresent && !meshDataHashMatches)
        {
            blockers.Add("kandra_package_mesh_data_hash_mismatch");
        }

        bool indicesDataHashMatches = indicesDataFilePresent && string.Equals(indicesDataSha256, writer.IndicesDataSha256, StringComparison.OrdinalIgnoreCase);
        if (indicesDataFilePresent && !indicesDataHashMatches)
        {
            blockers.Add("kandra_package_indices_data_hash_mismatch");
        }

        bool liveProofDownstreamFalse = IsLiveProofDownstreamFalse(proof);
        if (!liveProofDownstreamFalse)
        {
            blockers.Add("live_kandra_proof_downstream_boundary_not_false");
        }

        if (!proof.VisualRuntime.Requested)
        {
            blockers.Add("live_kandra_proof_visual_runtime_not_requested");
        }

        if (!proof.VisualRuntime.LiveBodyObserved)
        {
            blockers.Add("live_kandra_proof_body_not_observed");
        }

        if (!proof.VisualRuntime.LiveEquipObserved)
        {
            blockers.Add("live_kandra_proof_equip_not_observed");
        }

        if (!proof.VisualRuntime.PoseBindingAccepted)
        {
            warnings.Add("live_kandra_proof_pose_binding_not_accepted");
        }

        bool liveKandraRegistrationProofPresent = proof.VisualRuntime.Rows.Any(row => IsObservedTrue(row.KandraIsRegisteredStatus));
        if (!liveKandraRegistrationProofPresent)
        {
            blockers.Add("live_kandra_registration_proof_missing");
        }

        bool liveKandraMeshMemoryProofPresent = proof.VisualRuntime.Rows.Any(row => IsObservedTrue(row.KandraTryGetMeshMemoryStatus));
        if (!liveKandraMeshMemoryProofPresent)
        {
            blockers.Add("live_kandra_mesh_memory_proof_missing");
        }

        if (!runtimeSeamValid)
        {
            blockers.Add("kandra_package_runtime_seam_invalid");
        }

        return new KandraPackageValidationResult(
            StageVersion,
            request.RequestId,
            writer.StageVersion,
            writer.LayoutVersion,
            writer.ModDirectory,
            writer.MeshName,
            writer.KandraDirectory,
            writer.MeshDataPath,
            writer.IndicesDataPath,
            meshDataByteCount,
            indicesDataByteCount,
            meshDataSha256,
            indicesDataSha256,
            runtimeSeamValid,
            meshDataFilePresent,
            indicesDataFilePresent,
            meshDataLengthMatches,
            indicesDataLengthMatches,
            meshDataHashMatches,
            indicesDataHashMatches,
            liveKandraRegistrationProofPresent,
            liveKandraMeshMemoryProofPresent,
            liveProofDownstreamFalse,
            writerDownstreamFalse,
            blockers,
            warnings);
    }

    private static bool ValidateRuntimeSeam(KandraWriterResult writer, List<string> blockers)
    {
        string kandraDirectory = FullPath(writer.KandraDirectory);
        string meshDataPath = FullPath(writer.MeshDataPath);
        string indicesDataPath = FullPath(writer.IndicesDataPath);
        string? parentDirectory = Directory.GetParent(kandraDirectory)?.Name;
        bool valid = true;

        if (!string.Equals(Path.GetFileName(kandraDirectory), "Kandra", StringComparison.Ordinal))
        {
            blockers.Add("kandra_package_directory_not_named_kandra");
            valid = false;
        }

        if (!string.Equals(parentDirectory, writer.ModDirectory, StringComparison.Ordinal))
        {
            blockers.Add("kandra_package_parent_directory_not_mod_directory");
            valid = false;
        }

        if (!string.Equals(FullPath(Path.GetDirectoryName(meshDataPath) ?? string.Empty), kandraDirectory, StringComparison.OrdinalIgnoreCase))
        {
            blockers.Add("kandra_mesh_data_path_not_inside_kandra_directory");
            valid = false;
        }

        if (!string.Equals(FullPath(Path.GetDirectoryName(indicesDataPath) ?? string.Empty), kandraDirectory, StringComparison.OrdinalIgnoreCase))
        {
            blockers.Add("kandra_indices_data_path_not_inside_kandra_directory");
            valid = false;
        }

        if (!string.Equals(Path.GetFileName(meshDataPath), writer.MeshName + ".mdkandra", StringComparison.Ordinal))
        {
            blockers.Add("kandra_mesh_data_filename_invalid");
            valid = false;
        }

        if (!string.Equals(Path.GetFileName(indicesDataPath), writer.MeshName + ".ixkandra", StringComparison.Ordinal))
        {
            blockers.Add("kandra_indices_data_filename_invalid");
            valid = false;
        }

        return valid;
    }

    private static bool IsWriterDownstreamFalse(KandraWriterResult writer) =>
        !writer.CandidateMapApplicationAllowed &&
        !writer.CandidateMapApplicationExecuted &&
        !writer.ConversionExecuted &&
        !writer.RuntimeRegistrationAllowed &&
        !writer.RuntimeRegistrationExecuted &&
        !writer.ItemEquipMutationExecuted &&
        !writer.SaveMutationExecuted &&
        !writer.NativeGameWriteExecuted &&
        !writer.DownstreamWritesExecuted;

    private static bool IsLiveProofDownstreamFalse(ArmorImportResult proof) =>
        proof.VisualRuntime.DownstreamBoundaryFalse &&
        !proof.CandidateMapApplicationAllowed &&
        !proof.CandidateMapApplicationExecuted &&
        !proof.ConversionAllowed &&
        !proof.ConversionExecuted &&
        !proof.RuntimeLoaderChanged &&
        !proof.RuntimeRegistrationExecuted &&
        !proof.ItemRegistrationExecuted &&
        !proof.InventoryEquipSaveMutationExecuted &&
        !proof.NativeGameWriteExecuted &&
        !proof.SaveWriteExecuted &&
        !proof.ReleaseReady;

    private static bool IsObservedTrue(string value) =>
        value.StartsWith("observed:True", StringComparison.Ordinal);

    private static long FileLength(string path) =>
        new FileInfo(path).Length;

    private static string FullPath(string path) =>
        Path.GetFullPath(path);

    private static string Sha256File(string path)
    {
        using (SHA256 sha256 = SHA256.Create())
        using (FileStream stream = File.OpenRead(path))
        {
            byte[] digest = sha256.ComputeHash(stream);
            var hexadecimal = new StringBuilder(digest.Length * 2);
            foreach (byte value in digest)
            {
                hexadecimal.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            }

            return "sha256:" + hexadecimal;
        }
    }
}
