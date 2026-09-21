using System;
using System.Collections.Generic;
using System.Linq;

namespace Tainted.Armour;

public enum ArmorImportStatus
{
    DeformationBlocked = 0,
    DeformationValidatedConversionBlocked = 1,
    VisualRuntimeBlocked = 2,
    DeformationAndVisualRuntimeValidatedConversionBlocked = 3,
}

public sealed class DeformationControlReceipt
{
    public DeformationControlReceipt(bool identityPassed, bool rigidRotationPassed, bool orientationReversalPassed, bool collapsePassed)
        : this(
            "tainted-armour.metric-calibration.legacy",
            "tainted-armour.metric.legacy",
            identityPassed,
            rigidRotationPassed,
            rigidRotationPassed,
            orientationReversalPassed,
            collapsePassed,
            Array.Empty<MetricControlResult>())
    {
    }

    public DeformationControlReceipt(
        string calibrationStageVersion,
        string metricVersion,
        bool identityPassed,
        bool rigidRotationPassed,
        bool outOfPlaneRigidRotationPassed,
        bool orientationReversalPassed,
        bool collapsePassed,
        IEnumerable<MetricControlResult> controlResults)
    {
        CalibrationStageVersion = ContractValues.RequireText(calibrationStageVersion, nameof(calibrationStageVersion));
        MetricVersion = ContractValues.RequireText(metricVersion, nameof(metricVersion));
        IdentityPassed = identityPassed;
        RigidRotationPassed = rigidRotationPassed;
        OutOfPlaneRigidRotationPassed = outOfPlaneRigidRotationPassed;
        OrientationReversalPassed = orientationReversalPassed;
        CollapsePassed = collapsePassed;
        ControlResults = (controlResults ?? throw new ArgumentNullException(nameof(controlResults))).ToArray();
        CandidateSupplementalMetricCompared = ControlResults.Any(value => value.CandidateComparison != null);
        CandidateMetricVersion = ControlResults
            .Select(value => value.CandidateComparison)
            .FirstOrDefault(value => value != null)?.MetricVersion
            ?? string.Empty;
        CandidateMetricComparisonCount = ControlResults.Count(value => value.CandidateComparison != null);
        CandidateMetricExpectedMatchCount = ControlResults.Count(value => value.CandidateComparison?.CandidateMatchesExpected == true);
        CandidateMetricDisagreementCount = ControlResults.Count(value => value.CandidateComparison?.DisagreesWithCurrent == true);
        CandidateMetricAffectsAcceptance = ControlResults.Any(value => value.CandidateComparison?.AffectsAcceptance == true);
    }

    public string CalibrationStageVersion { get; }

    public string MetricVersion { get; }

    public bool IdentityPassed { get; }

    public bool RigidRotationPassed { get; }

    public bool OutOfPlaneRigidRotationPassed { get; }

    public bool OrientationReversalPassed { get; }

    public bool CollapsePassed { get; }

    public IReadOnlyList<MetricControlResult> ControlResults { get; }

    public bool CandidateSupplementalMetricCompared { get; }

    public string CandidateMetricVersion { get; }

    public int CandidateMetricComparisonCount { get; }

    public int CandidateMetricExpectedMatchCount { get; }

    public int CandidateMetricDisagreementCount { get; }

    public bool CandidateMetricAffectsAcceptance { get; }

    public bool Passed => IdentityPassed
        && RigidRotationPassed
        && OrientationReversalPassed
        && CollapsePassed
        && (!CandidateMetricAffectsAcceptance || CandidateMetricDisagreementCount == 0);

    public int FailureCount =>
        (IdentityPassed ? 0 : 1)
        + (RigidRotationPassed ? 0 : 1)
        + (OrientationReversalPassed ? 0 : 1)
        + (CollapsePassed ? 0 : 1)
        + (CandidateMetricAffectsAcceptance ? CandidateMetricDisagreementCount : 0);
}

public sealed class MetricControlResult
{
    public MetricControlResult(
        string controlId,
        string description,
        bool expectedCollapsed,
        bool expectedOrientationReversed,
        bool expectedIndeterminate,
        TriangleDeformationEvidence observed,
        CandidateMetricComparisonResult? candidateComparison)
    {
        ControlId = ContractValues.RequireText(controlId, nameof(controlId));
        Description = ContractValues.RequireText(description, nameof(description));
        ExpectedCollapsed = expectedCollapsed;
        ExpectedOrientationReversed = expectedOrientationReversed;
        ExpectedIndeterminate = expectedIndeterminate;
        Observed = observed ?? throw new ArgumentNullException(nameof(observed));
        CandidateComparison = candidateComparison;
    }

    public string ControlId { get; }

    public string Description { get; }

    public bool ExpectedCollapsed { get; }

    public bool ExpectedOrientationReversed { get; }

    public bool ExpectedIndeterminate { get; }

    public TriangleDeformationEvidence Observed { get; }

    public CandidateMetricComparisonResult? CandidateComparison { get; }

    public bool Passed =>
        Observed.Collapsed == ExpectedCollapsed
        && Observed.OrientationReversed == ExpectedOrientationReversed
        && Observed.Indeterminate == ExpectedIndeterminate;
}

public sealed class CandidateMetricComparisonResult
{
    public CandidateMetricComparisonResult(
        string metricVersion,
        string metricFamily,
        string referenceTransportId,
        bool expectedCollapsed,
        bool expectedOrientationReversed,
        bool expectedIndeterminate,
        bool currentCollapsed,
        bool currentOrientationReversed,
        bool currentIndeterminate,
        bool candidateCollapsed,
        bool candidateOrientationReversed,
        bool candidateIndeterminate,
        string candidateIndeterminateReason,
        double referenceTransportedNormalDot,
        bool affectsAcceptance)
    {
        MetricVersion = ContractValues.RequireText(metricVersion, nameof(metricVersion));
        MetricFamily = ContractValues.RequireText(metricFamily, nameof(metricFamily));
        ReferenceTransportId = ContractValues.RequireText(referenceTransportId, nameof(referenceTransportId));
        ExpectedCollapsed = expectedCollapsed;
        ExpectedOrientationReversed = expectedOrientationReversed;
        ExpectedIndeterminate = expectedIndeterminate;
        CurrentCollapsed = currentCollapsed;
        CurrentOrientationReversed = currentOrientationReversed;
        CurrentIndeterminate = currentIndeterminate;
        CandidateCollapsed = candidateCollapsed;
        CandidateOrientationReversed = candidateOrientationReversed;
        CandidateIndeterminate = candidateIndeterminate;
        CandidateIndeterminateReason = candidateIndeterminateReason ?? string.Empty;
        ReferenceTransportedNormalDot = referenceTransportedNormalDot;
        AffectsAcceptance = affectsAcceptance;
    }

    public string MetricVersion { get; }
    public string MetricFamily { get; }
    public string ReferenceTransportId { get; }
    public bool ExpectedCollapsed { get; }
    public bool ExpectedOrientationReversed { get; }
    public bool ExpectedIndeterminate { get; }
    public bool CurrentCollapsed { get; }
    public bool CurrentOrientationReversed { get; }
    public bool CurrentIndeterminate { get; }
    public bool CandidateCollapsed { get; }
    public bool CandidateOrientationReversed { get; }
    public bool CandidateIndeterminate { get; }
    public string CandidateIndeterminateReason { get; }
    public double ReferenceTransportedNormalDot { get; }
    public bool AffectsAcceptance { get; }

    public bool CandidateMatchesExpected =>
        CandidateCollapsed == ExpectedCollapsed
        && CandidateOrientationReversed == ExpectedOrientationReversed
        && CandidateIndeterminate == ExpectedIndeterminate;

    public bool DisagreesWithCurrent =>
        CandidateCollapsed != CurrentCollapsed
        || CandidateOrientationReversed != CurrentOrientationReversed
        || CandidateIndeterminate != CurrentIndeterminate;
}

public sealed class TriangleDeformationEvidence
{
    public TriangleDeformationEvidence(
        string poseId,
        int triangleOrdinal,
        int vertex0,
        int vertex1,
        int vertex2,
        double baselineArea,
        double posedArea,
        double normalizedNormalDot,
        double vertex0Displacement,
        double vertex1Displacement,
        double vertex2Displacement,
        bool collapsed,
        bool orientationReversed,
        bool indeterminate,
        string indeterminateReason)
    {
        PoseId = ContractValues.RequireText(poseId, nameof(poseId));
        TriangleOrdinal = triangleOrdinal;
        Vertex0 = vertex0;
        Vertex1 = vertex1;
        Vertex2 = vertex2;
        BaselineArea = baselineArea;
        PosedArea = posedArea;
        NormalizedNormalDot = normalizedNormalDot;
        Vertex0Displacement = vertex0Displacement;
        Vertex1Displacement = vertex1Displacement;
        Vertex2Displacement = vertex2Displacement;
        Collapsed = collapsed;
        OrientationReversed = orientationReversed;
        Indeterminate = indeterminate;
        IndeterminateReason = indeterminateReason ?? string.Empty;
    }

    public string PoseId { get; }
    public int TriangleOrdinal { get; }
    public int Vertex0 { get; }
    public int Vertex1 { get; }
    public int Vertex2 { get; }
    public double BaselineArea { get; }
    public double PosedArea { get; }
    public double NormalizedNormalDot { get; }
    public double Vertex0Displacement { get; }
    public double Vertex1Displacement { get; }
    public double Vertex2Displacement { get; }
    public bool Collapsed { get; }
    public bool OrientationReversed { get; }
    public bool Indeterminate { get; }
    public string IndeterminateReason { get; }
}

public sealed class PoseDeformationResult
{
    private readonly TriangleDeformationEvidence[] triangleEvidence;

    public PoseDeformationResult(
        string poseId,
        string clipId,
        double sampleTimeSeconds,
        string geometryFingerprint,
        int evaluatedVertexCount,
        int movedVertexCount,
        double maxVertexDisplacement,
        double meanVertexDisplacement,
        IEnumerable<TriangleDeformationEvidence> triangleEvidence)
    {
        PoseId = ContractValues.RequireText(poseId, nameof(poseId));
        ClipId = ContractValues.RequireText(clipId, nameof(clipId));
        SampleTimeSeconds = sampleTimeSeconds;
        GeometryFingerprint = ContractValues.RequireText(geometryFingerprint, nameof(geometryFingerprint));
        EvaluatedVertexCount = evaluatedVertexCount;
        MovedVertexCount = movedVertexCount;
        MaxVertexDisplacement = maxVertexDisplacement;
        MeanVertexDisplacement = meanVertexDisplacement;
        this.triangleEvidence = triangleEvidence?.ToArray() ?? throw new ArgumentNullException(nameof(triangleEvidence));
    }

    public string PoseId { get; }
    public string ClipId { get; }
    public double SampleTimeSeconds { get; }
    public string GeometryFingerprint { get; }
    public int EvaluatedVertexCount { get; }
    public int MovedVertexCount { get; }
    public double MaxVertexDisplacement { get; }
    public double MeanVertexDisplacement { get; }
    public IReadOnlyList<TriangleDeformationEvidence> TriangleEvidence => triangleEvidence;
}

public sealed class DeformationValidationResult
{
    private readonly PoseDeformationResult[] poses;
    private readonly string[] blockers;
    private readonly string[] warnings;

    public DeformationValidationResult(
        string stageVersion,
        string metricVersion,
        string sourceFingerprint,
        string targetFingerprint,
        string candidateMapFingerprint,
        DeformationControlReceipt controls,
        IEnumerable<PoseDeformationResult> poses,
        int collapsedTriangleCount,
        int orientationReversedTriangleCount,
        int indeterminateTriangleCount,
        double maxVertexDisplacement,
        bool deformationAccepted,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings)
    {
        StageVersion = ContractValues.RequireText(stageVersion, nameof(stageVersion));
        MetricVersion = ContractValues.RequireText(metricVersion, nameof(metricVersion));
        SourceFingerprint = ContractValues.RequireText(sourceFingerprint, nameof(sourceFingerprint));
        TargetFingerprint = ContractValues.RequireText(targetFingerprint, nameof(targetFingerprint));
        CandidateMapFingerprint = ContractValues.RequireText(candidateMapFingerprint, nameof(candidateMapFingerprint));
        Controls = controls ?? throw new ArgumentNullException(nameof(controls));
        this.poses = poses?.ToArray() ?? throw new ArgumentNullException(nameof(poses));
        CollapsedTriangleCount = collapsedTriangleCount;
        OrientationReversedTriangleCount = orientationReversedTriangleCount;
        IndeterminateTriangleCount = indeterminateTriangleCount;
        MaxVertexDisplacement = maxVertexDisplacement;
        DeformationAccepted = deformationAccepted;
        this.blockers = CopyMessages(blockers);
        this.warnings = CopyMessages(warnings);
    }

    public string StageVersion { get; }
    public string MetricVersion { get; }
    public string SourceFingerprint { get; }
    public string TargetFingerprint { get; }
    public string CandidateMapFingerprint { get; }
    public DeformationControlReceipt Controls { get; }
    public IReadOnlyList<PoseDeformationResult> Poses => poses;
    public int EvaluatedPoseCount => poses.Length;
    public int EvaluatedTriangleCount => poses.Sum(value => value.TriangleEvidence.Count);
    public int CollapsedTriangleCount { get; }
    public int OrientationReversedTriangleCount { get; }
    public int IndeterminateTriangleCount { get; }
    public int ControlFailureCount => Controls.FailureCount;
    public double MaxVertexDisplacement { get; }
    public bool DeformationAccepted { get; }
    public bool CandidateMapApplicationAllowed => false;
    public bool ConversionAllowed => false;
    public bool NextStageEligible => false;
    public IReadOnlyList<string> Blockers => blockers;
    public IReadOnlyList<string> Warnings => warnings;

    private static string[] CopyMessages(IEnumerable<string> values) =>
        values?.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToArray()
        ?? Array.Empty<string>();
}

public sealed class VisualRuntimeValidationResult
{
    private readonly VisualRuntimeObservationRow[] rows;
    private readonly string[] blockers;
    private readonly string[] warnings;

    public VisualRuntimeValidationResult(
        string stageVersion,
        string routeId,
        string observerMode,
        bool requested,
        bool liveBodyObserved,
        bool liveEquipObserved,
        bool poseBindingAccepted,
        bool downstreamBoundaryFalse,
        bool requiredRowsPresent,
        int blockedMetricRowCount,
        IEnumerable<VisualRuntimeObservationRow> rows,
        bool visualRuntimeAccepted,
        IEnumerable<string> blockers,
        IEnumerable<string> warnings)
    {
        StageVersion = ContractValues.RequireText(stageVersion, nameof(stageVersion));
        RouteId = routeId ?? string.Empty;
        ObserverMode = observerMode ?? string.Empty;
        Requested = requested;
        LiveBodyObserved = liveBodyObserved;
        LiveEquipObserved = liveEquipObserved;
        PoseBindingAccepted = poseBindingAccepted;
        DownstreamBoundaryFalse = downstreamBoundaryFalse;
        RequiredRowsPresent = requiredRowsPresent;
        BlockedMetricRowCount = blockedMetricRowCount;
        this.rows = rows?.ToArray() ?? throw new ArgumentNullException(nameof(rows));
        VisualRuntimeAccepted = visualRuntimeAccepted;
        this.blockers = CopyMessages(blockers);
        this.warnings = CopyMessages(warnings);
    }

    public string StageVersion { get; }
    public string RouteId { get; }
    public string ObserverMode { get; }
    public bool Requested { get; }
    public bool LiveBodyObserved { get; }
    public bool LiveEquipObserved { get; }
    public bool LiveRuntimeMetricsObserved => LiveBodyObserved && LiveEquipObserved && RowCount > 0;
    public bool PoseBindingAccepted { get; }
    public bool DownstreamBoundaryFalse { get; }
    public bool RequiredRowsPresent { get; }
    public int BlockedMetricRowCount { get; }
    public int BlockedRowCount => BlockedMetricRowCount;
    public IReadOnlyList<VisualRuntimeObservationRow> Rows => rows;
    public int RowCount => rows.Length;
    public bool VisualRuntimeAccepted { get; }
    public bool CandidateMapApplicationAllowed => false;
    public bool CandidateMapApplicationExecuted => false;
    public bool ConversionAllowed => false;
    public bool ConversionExecuted => false;
    public bool DownstreamWritesExecuted => false;
    public IReadOnlyList<string> Blockers => blockers;
    public IReadOnlyList<string> Warnings => warnings;

    public static VisualRuntimeValidationResult NotRequested(string stageVersion) =>
        new VisualRuntimeValidationResult(
            stageVersion,
            string.Empty,
            string.Empty,
            requested: false,
            liveBodyObserved: false,
            liveEquipObserved: false,
            poseBindingAccepted: false,
            downstreamBoundaryFalse: true,
            requiredRowsPresent: false,
            blockedMetricRowCount: 0,
            Array.Empty<VisualRuntimeObservationRow>(),
            visualRuntimeAccepted: false,
            Array.Empty<string>(),
            Array.Empty<string>());

    private static string[] CopyMessages(IEnumerable<string> values) =>
        values?.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToArray()
        ?? Array.Empty<string>();
}

public sealed class ArmorImportResult
{
    private readonly string[] blockers;

    public ArmorImportResult(
        string requestId,
        ArmorImportStatus status,
        DeformationValidationResult deformation,
        VisualRuntimeValidationResult visualRuntime,
        IEnumerable<string> blockers)
    {
        RequestId = ContractValues.RequireText(requestId, nameof(requestId));
        Status = status;
        Deformation = deformation ?? throw new ArgumentNullException(nameof(deformation));
        VisualRuntime = visualRuntime ?? throw new ArgumentNullException(nameof(visualRuntime));
        this.blockers = blockers?.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()).ToArray()
            ?? Array.Empty<string>();
    }

    public string RequestId { get; }
    public ArmorImportStatus Status { get; }
    public DeformationValidationResult Deformation { get; }
    public VisualRuntimeValidationResult VisualRuntime { get; }
    public IReadOnlyList<string> Blockers => blockers;
    public bool CandidateMapApplicationAllowed => false;
    public bool CandidateMapApplicationExecuted => false;
    public bool ConversionAllowed => false;
    public bool ConversionExecuted => false;
    public bool SidecarsGenerated => false;
    public bool SourceFbxMutationExecuted => false;
    public bool UnityProjectAssetMutationExecuted => false;
    public bool RuntimeLoaderChanged => false;
    public bool RuntimeRegistrationExecuted => false;
    public bool ItemRegistrationExecuted => false;
    public bool InventoryEquipSaveMutationExecuted => false;
    public bool SaveWriteExecuted => false;
    public bool NativeGameWriteExecuted => false;
    public bool ReleaseReady => false;
}
