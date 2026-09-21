using System;
using System.Collections.Generic;
using System.Linq;

namespace Tainted.Armour;

public sealed class ArmorSourceContract
{
    public ArmorSourceContract(
        string sourceAssetId,
        string sourceFingerprint,
        string meshId,
        int submeshIndex,
        string materialSlotId,
        string rigFingerprint,
        string bindPoseFingerprint,
        string boneWeightFingerprint,
        int vertexCount,
        int triangleCount,
        int weightedVertexCount,
        int maximumInfluences,
        int invalidInfluenceCount)
    {
        SourceAssetId = ContractValues.RequireText(sourceAssetId, nameof(sourceAssetId));
        SourceFingerprint = ContractValues.RequireText(sourceFingerprint, nameof(sourceFingerprint));
        MeshId = ContractValues.RequireText(meshId, nameof(meshId));
        if (submeshIndex < 0) throw new ArgumentOutOfRangeException(nameof(submeshIndex));
        SubmeshIndex = submeshIndex;
        MaterialSlotId = ContractValues.RequireText(materialSlotId, nameof(materialSlotId));
        RigFingerprint = ContractValues.RequireText(rigFingerprint, nameof(rigFingerprint));
        BindPoseFingerprint = ContractValues.RequireText(bindPoseFingerprint, nameof(bindPoseFingerprint));
        BoneWeightFingerprint = ContractValues.RequireText(boneWeightFingerprint, nameof(boneWeightFingerprint));
        if (vertexCount <= 0) throw new ArgumentOutOfRangeException(nameof(vertexCount));
        if (triangleCount <= 0) throw new ArgumentOutOfRangeException(nameof(triangleCount));
        if (weightedVertexCount < 0 || weightedVertexCount > vertexCount) throw new ArgumentOutOfRangeException(nameof(weightedVertexCount));
        if (maximumInfluences <= 0) throw new ArgumentOutOfRangeException(nameof(maximumInfluences));
        if (invalidInfluenceCount < 0) throw new ArgumentOutOfRangeException(nameof(invalidInfluenceCount));
        VertexCount = vertexCount;
        TriangleCount = triangleCount;
        WeightedVertexCount = weightedVertexCount;
        MaximumInfluences = maximumInfluences;
        InvalidInfluenceCount = invalidInfluenceCount;
    }

    public string SourceAssetId { get; }

    public string SourceFingerprint { get; }

    public string MeshId { get; }

    public int SubmeshIndex { get; }

    public string MaterialSlotId { get; }

    public string RigFingerprint { get; }

    public string BindPoseFingerprint { get; }

    public string BoneWeightFingerprint { get; }

    public int VertexCount { get; }

    public int TriangleCount { get; }

    public int WeightedVertexCount { get; }

    public int MaximumInfluences { get; }

    public int InvalidInfluenceCount { get; }
}

public sealed class ArmorTargetContract
{
    public ArmorTargetContract(
        string targetId,
        string variantId,
        string nativeEquipmentType,
        string targetFingerprint,
        string candidateMapFingerprint,
        string candidateMapStatus)
    {
        TargetId = ContractValues.RequireText(targetId, nameof(targetId));
        VariantId = ContractValues.RequireText(variantId, nameof(variantId));
        NativeEquipmentType = ContractValues.RequireText(nativeEquipmentType, nameof(nativeEquipmentType));
        TargetFingerprint = ContractValues.RequireText(targetFingerprint, nameof(targetFingerprint));
        CandidateMapFingerprint = ContractValues.RequireText(candidateMapFingerprint, nameof(candidateMapFingerprint));
        CandidateMapStatus = ContractValues.RequireText(candidateMapStatus, nameof(candidateMapStatus));
    }

    public string TargetId { get; }

    public string VariantId { get; }

    public string NativeEquipmentType { get; }

    public string TargetFingerprint { get; }

    public string CandidateMapFingerprint { get; }

    public string CandidateMapStatus { get; }
}

public sealed class DeformationPolicy
{
    public const string ProvenMetricVersion = "t29-t31.normal-dot.v1";

    public DeformationPolicy(
        string metricVersion,
        double displacementEpsilon,
        double triangleAreaEpsilon,
        double orientationDotThreshold,
        int allowedCollapsedTriangles,
        int allowedOrientationReversedTriangles,
        SupplementalMetricPolicy? supplementalMetric = null)
    {
        MetricVersion = ContractValues.RequireText(metricVersion, nameof(metricVersion));
        DisplacementEpsilon = ContractValues.RequirePositiveFinite(displacementEpsilon, nameof(displacementEpsilon));
        TriangleAreaEpsilon = ContractValues.RequirePositiveFinite(triangleAreaEpsilon, nameof(triangleAreaEpsilon));
        if (double.IsNaN(orientationDotThreshold)
            || double.IsInfinity(orientationDotThreshold)
            || orientationDotThreshold < -1d
            || orientationDotThreshold > 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(orientationDotThreshold));
        }

        if (allowedCollapsedTriangles < 0) throw new ArgumentOutOfRangeException(nameof(allowedCollapsedTriangles));
        if (allowedOrientationReversedTriangles < 0) throw new ArgumentOutOfRangeException(nameof(allowedOrientationReversedTriangles));

        OrientationDotThreshold = orientationDotThreshold;
        AllowedCollapsedTriangles = allowedCollapsedTriangles;
        AllowedOrientationReversedTriangles = allowedOrientationReversedTriangles;
        SupplementalMetric = supplementalMetric ?? SupplementalMetricPolicy.Disabled();
    }

    public string MetricVersion { get; }

    public double DisplacementEpsilon { get; }

    public double TriangleAreaEpsilon { get; }

    public double OrientationDotThreshold { get; }

    public int AllowedCollapsedTriangles { get; }

    public int AllowedOrientationReversedTriangles { get; }

    public SupplementalMetricPolicy SupplementalMetric { get; }

    public static DeformationPolicy CreateT29T31ZeroTolerance() =>
        new DeformationPolicy(ProvenMetricVersion, 0.000001d, 0.00000001d, 0d, 0, 0);
}

public sealed class SupplementalMetricPolicy
{
    public SupplementalMetricPolicy(string metricVersion, bool enabled, bool affectsAcceptance)
    {
        MetricVersion = ContractValues.RequireText(metricVersion, nameof(metricVersion));
        Enabled = enabled;
        AffectsAcceptance = affectsAcceptance;
    }

    public string MetricVersion { get; }

    public bool Enabled { get; }

    public bool AffectsAcceptance { get; }

    public static SupplementalMetricPolicy Disabled() =>
        new SupplementalMetricPolicy("tainted-armour.candidate-supplemental-metric.disabled/1", false, false);
}

public sealed class DeformationValidationRequest
{
    private readonly PoseGeometrySnapshot[] poses;
    private readonly ArmorTriangle[] triangles;
    private readonly int[] evaluatedVertexIndices;

    public DeformationValidationRequest(
        PoseGeometrySnapshot baseline,
        IEnumerable<PoseGeometrySnapshot> poses,
        IEnumerable<ArmorTriangle> triangles,
        IEnumerable<int> evaluatedVertexIndices,
        DeformationPolicy policy)
    {
        Baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
        this.poses = poses?.ToArray() ?? throw new ArgumentNullException(nameof(poses));
        this.triangles = triangles?.ToArray() ?? throw new ArgumentNullException(nameof(triangles));
        this.evaluatedVertexIndices = evaluatedVertexIndices?.ToArray() ?? throw new ArgumentNullException(nameof(evaluatedVertexIndices));
        Policy = policy ?? throw new ArgumentNullException(nameof(policy));

        if (this.poses.Length == 0) throw new ArgumentException("At least one pose is required.", nameof(poses));
        if (this.triangles.Length == 0) throw new ArgumentException("At least one triangle is required.", nameof(triangles));
        if (this.evaluatedVertexIndices.Length == 0) throw new ArgumentException("At least one evaluated vertex is required.", nameof(evaluatedVertexIndices));
        if (this.poses.Select(value => value.PoseId).Distinct(StringComparer.Ordinal).Count() != this.poses.Length)
        {
            throw new ArgumentException("Pose identifiers must be unique.", nameof(poses));
        }

        if (this.triangles.Select(value => value.Ordinal).Distinct().Count() != this.triangles.Length)
        {
            throw new ArgumentException("Triangle ordinals must be unique.", nameof(triangles));
        }

        if (this.evaluatedVertexIndices.Any(value => value < 0)
            || this.evaluatedVertexIndices.Distinct().Count() != this.evaluatedVertexIndices.Length)
        {
            throw new ArgumentException("Evaluated vertex indices must be unique and non-negative.", nameof(evaluatedVertexIndices));
        }
    }

    public PoseGeometrySnapshot Baseline { get; }

    public IReadOnlyList<PoseGeometrySnapshot> Poses => poses;

    public IReadOnlyList<ArmorTriangle> Triangles => triangles;

    public IReadOnlyList<int> EvaluatedVertexIndices => evaluatedVertexIndices;

    public DeformationPolicy Policy { get; }
}

public sealed class VisualRuntimeObservationRequest
{
    public const string A2KT34RouteId = "a2k-t34-live-visual-runtime-observer";

    private readonly VisualRuntimeObservationRow[] rows;

    public VisualRuntimeObservationRequest(
        string routeId,
        string observerMode,
        string triggerSource,
        string evidenceSource,
        string activeScene,
        string liveBodyObservationStatus,
        string liveEquipObservationStatus,
        string poseBindingStatus,
        bool candidateMapApplicationAllowed,
        bool candidateMapApplicationExecuted,
        bool conversionAllowed,
        bool conversionExecuted,
        bool itemEquipMutationAllowed,
        bool itemEquipMutationExecuted,
        bool saveMutationAllowed,
        bool saveMutationExecuted,
        bool downstreamWritesAllowed,
        bool downstreamWritesExecuted,
        IEnumerable<VisualRuntimeObservationRow> rows)
    {
        RouteId = ContractValues.RequireText(routeId, nameof(routeId));
        ObserverMode = ContractValues.RequireText(observerMode, nameof(observerMode));
        TriggerSource = ContractValues.RequireText(triggerSource, nameof(triggerSource));
        EvidenceSource = ContractValues.RequireText(evidenceSource, nameof(evidenceSource));
        ActiveScene = activeScene ?? string.Empty;
        LiveBodyObservationStatus = ContractValues.RequireText(liveBodyObservationStatus, nameof(liveBodyObservationStatus));
        LiveEquipObservationStatus = ContractValues.RequireText(liveEquipObservationStatus, nameof(liveEquipObservationStatus));
        PoseBindingStatus = ContractValues.RequireText(poseBindingStatus, nameof(poseBindingStatus));
        CandidateMapApplicationAllowed = candidateMapApplicationAllowed;
        CandidateMapApplicationExecuted = candidateMapApplicationExecuted;
        ConversionAllowed = conversionAllowed;
        ConversionExecuted = conversionExecuted;
        ItemEquipMutationAllowed = itemEquipMutationAllowed;
        ItemEquipMutationExecuted = itemEquipMutationExecuted;
        SaveMutationAllowed = saveMutationAllowed;
        SaveMutationExecuted = saveMutationExecuted;
        DownstreamWritesAllowed = downstreamWritesAllowed;
        DownstreamWritesExecuted = downstreamWritesExecuted;
        this.rows = rows?.ToArray() ?? throw new ArgumentNullException(nameof(rows));
        if (this.rows.Length == 0) throw new ArgumentException("At least one observation row is required.", nameof(rows));
    }

    public string RouteId { get; }
    public string ObserverMode { get; }
    public string TriggerSource { get; }
    public string EvidenceSource { get; }
    public string ActiveScene { get; }
    public string LiveBodyObservationStatus { get; }
    public string LiveEquipObservationStatus { get; }
    public string PoseBindingStatus { get; }
    public bool CandidateMapApplicationAllowed { get; }
    public bool CandidateMapApplicationExecuted { get; }
    public bool ConversionAllowed { get; }
    public bool ConversionExecuted { get; }
    public bool ItemEquipMutationAllowed { get; }
    public bool ItemEquipMutationExecuted { get; }
    public bool SaveMutationAllowed { get; }
    public bool SaveMutationExecuted { get; }
    public bool DownstreamWritesAllowed { get; }
    public bool DownstreamWritesExecuted { get; }
    public IReadOnlyList<VisualRuntimeObservationRow> Rows => rows;

    public static VisualRuntimeObservationRequest CreateA2KT34NoWriteObserver(
        string triggerSource,
        string evidenceSource,
        string activeScene,
        string liveBodyObservationStatus,
        string liveEquipObservationStatus,
        string poseBindingStatus,
        IEnumerable<VisualRuntimeObservationRow> rows) =>
        new VisualRuntimeObservationRequest(
            A2KT34RouteId,
            "diagnostic-tool-manual-dump",
            triggerSource,
            evidenceSource,
            activeScene,
            liveBodyObservationStatus,
            liveEquipObservationStatus,
            poseBindingStatus,
            candidateMapApplicationAllowed: false,
            candidateMapApplicationExecuted: false,
            conversionAllowed: false,
            conversionExecuted: false,
            itemEquipMutationAllowed: false,
            itemEquipMutationExecuted: false,
            saveMutationAllowed: false,
            saveMutationExecuted: false,
            downstreamWritesAllowed: false,
            downstreamWritesExecuted: false,
            rows);
}

public sealed class VisualRuntimeObservationRow
{
    public VisualRuntimeObservationRow(
        string sex,
        string deformationReceiver,
        int inversionRecordCount,
        string liveObjectPath,
        string rendererName,
        string meshName,
        string materialNames,
        string animatorClipNames,
        string normalDotClassification,
        string referenceTransportSemantics,
        string localWindingSemantics,
        string clippingStatus,
        string seamStatus,
        string bodyCoverageStatus,
        string materialShadowStatus,
        string layerStatus,
        string notes,
        string kandraIsRegisteredStatus = "",
        string kandraTryGetMeshMemoryStatus = "")
    {
        Sex = ContractValues.RequireText(sex, nameof(sex));
        DeformationReceiver = ContractValues.RequireText(deformationReceiver, nameof(deformationReceiver));
        if (inversionRecordCount < 0) throw new ArgumentOutOfRangeException(nameof(inversionRecordCount));
        InversionRecordCount = inversionRecordCount;
        LiveObjectPath = liveObjectPath ?? string.Empty;
        RendererName = rendererName ?? string.Empty;
        MeshName = meshName ?? string.Empty;
        MaterialNames = materialNames ?? string.Empty;
        AnimatorClipNames = animatorClipNames ?? string.Empty;
        NormalDotClassification = ContractValues.RequireText(normalDotClassification, nameof(normalDotClassification));
        ReferenceTransportSemantics = ContractValues.RequireText(referenceTransportSemantics, nameof(referenceTransportSemantics));
        LocalWindingSemantics = ContractValues.RequireText(localWindingSemantics, nameof(localWindingSemantics));
        ClippingStatus = ContractValues.RequireText(clippingStatus, nameof(clippingStatus));
        SeamStatus = ContractValues.RequireText(seamStatus, nameof(seamStatus));
        BodyCoverageStatus = ContractValues.RequireText(bodyCoverageStatus, nameof(bodyCoverageStatus));
        MaterialShadowStatus = ContractValues.RequireText(materialShadowStatus, nameof(materialShadowStatus));
        LayerStatus = ContractValues.RequireText(layerStatus, nameof(layerStatus));
        Notes = notes ?? string.Empty;
        KandraIsRegisteredStatus = kandraIsRegisteredStatus ?? string.Empty;
        KandraTryGetMeshMemoryStatus = kandraTryGetMeshMemoryStatus ?? string.Empty;
    }

    public string Sex { get; }
    public string DeformationReceiver { get; }
    public int InversionRecordCount { get; }
    public string LiveObjectPath { get; }
    public string RendererName { get; }
    public string MeshName { get; }
    public string MaterialNames { get; }
    public string AnimatorClipNames { get; }
    public string NormalDotClassification { get; }
    public string ReferenceTransportSemantics { get; }
    public string LocalWindingSemantics { get; }
    public string ClippingStatus { get; }
    public string SeamStatus { get; }
    public string BodyCoverageStatus { get; }
    public string MaterialShadowStatus { get; }
    public string LayerStatus { get; }
    public string Notes { get; }
    public string KandraIsRegisteredStatus { get; }
    public string KandraTryGetMeshMemoryStatus { get; }
}

public sealed class ArmorImportRequest
{
    public ArmorImportRequest(
        string requestId,
        ArmorSourceContract source,
        ArmorTargetContract target,
        DeformationValidationRequest deformation,
        VisualRuntimeObservationRequest? visualRuntimeObservation = null)
    {
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Deformation = deformation ?? throw new ArgumentNullException(nameof(deformation));
        VisualRuntimeObservation = visualRuntimeObservation;
    }

    public string RequestId { get; }

    public ArmorSourceContract Source { get; }

    public ArmorTargetContract Target { get; }

    public DeformationValidationRequest Deformation { get; }

    public VisualRuntimeObservationRequest? VisualRuntimeObservation { get; }
}

internal static class ContractValues
{
    public static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", parameterName);
        return value.Trim();
    }

    public static double RequirePositiveFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0d)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        return value;
    }
}
