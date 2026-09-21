using System;

namespace Tainted.Armour;

internal sealed class MetricCalibrationStage
{
    internal const string StageVersion = "tainted-armour.metric-calibration.v3";

    public DeformationControlReceipt Execute(DeformationPolicy policy)
    {
        if (policy is null) throw new ArgumentNullException(nameof(policy));

        var triangle = new ArmorTriangle(0, 0, 1, 2);
        var baseline = new[]
        {
            new ArmorVector3(0d, 0d, 0d),
            new ArmorVector3(1d, 0d, 0d),
            new ArmorVector3(0d, 1d, 0d),
        };

        MetricControlResult identity = Evaluate(
            "identity_rest",
            "unchanged rest geometry must preserve winding",
            triangle,
            baseline,
            baseline,
            policy,
            ReferenceNormalTransport.Identity(),
            expectedCollapsed: false,
            expectedOrientationReversed: false,
            expectedIndeterminate: false);
        MetricControlResult inPlaneRotation = Evaluate(
            "in_plane_rigid_rotation",
            "proper in-plane rigid rotation must preserve local winding",
            triangle,
            baseline,
            new[]
            {
                new ArmorVector3(0d, 0d, 0d),
                new ArmorVector3(0d, 1d, 0d),
                new ArmorVector3(-1d, 0d, 0d),
            },
            policy,
            ReferenceNormalTransport.RotateZ90(),
            expectedCollapsed: false,
            expectedOrientationReversed: false,
            expectedIndeterminate: false);
        MetricControlResult outOfPlaneRotation = Evaluate(
            "out_of_plane_rigid_rotation",
            "proper out-of-plane rigid rotation must not be classified as local winding reversal",
            triangle,
            baseline,
            new[]
            {
                new ArmorVector3(0d, 0d, 0d),
                new ArmorVector3(1d, 0d, 0d),
                new ArmorVector3(0d, -1d, 0d),
            },
            policy,
            ReferenceNormalTransport.RotateX180(),
            expectedCollapsed: false,
            expectedOrientationReversed: false,
            expectedIndeterminate: false);
        MetricControlResult localReversal = Evaluate(
            "known_local_orientation_reversal",
            "known local vertex-order reversal must be classified as orientation reversal",
            triangle,
            baseline,
            new[]
            {
                baseline[0],
                baseline[2],
                baseline[1],
            },
            policy,
            ReferenceNormalTransport.Identity(),
            expectedCollapsed: false,
            expectedOrientationReversed: true,
            expectedIndeterminate: false);
        MetricControlResult collapse = Evaluate(
            "near_degenerate_collapse",
            "near-degenerate posed triangle must be classified as collapsed",
            triangle,
            baseline,
            new[]
            {
                new ArmorVector3(0d, 0d, 0d),
                new ArmorVector3(1d, 0d, 0d),
                new ArmorVector3(2d, 0d, 0d),
            },
            policy,
            ReferenceNormalTransport.Identity(),
            expectedCollapsed: true,
            expectedOrientationReversed: false,
            expectedIndeterminate: false);

        return new DeformationControlReceipt(
            StageVersion,
            policy.MetricVersion,
            identity.Passed,
            inPlaneRotation.Passed,
            outOfPlaneRotation.Passed,
            localReversal.Passed,
            collapse.Passed,
            new[]
            {
                identity,
                inPlaneRotation,
                outOfPlaneRotation,
                localReversal,
                collapse,
            });
    }

    private static MetricControlResult Evaluate(
        string controlId,
        string description,
        ArmorTriangle triangle,
        ArmorVector3[] baseline,
        ArmorVector3[] posed,
        DeformationPolicy policy,
        ReferenceNormalTransport referenceTransport,
        bool expectedCollapsed,
        bool expectedOrientationReversed,
        bool expectedIndeterminate)
    {
        TriangleDeformationEvidence observed = TriangleDeformationClassifier.Classify(
            "control." + controlId,
            triangle,
            baseline,
            posed,
            policy);
        CandidateMetricComparisonResult? candidateComparison = policy.SupplementalMetric.Enabled
            ? CandidateSupplementalMetricClassifier.Compare(
                controlId,
                triangle,
                baseline,
                posed,
                policy,
                referenceTransport,
                observed,
                expectedCollapsed,
                expectedOrientationReversed,
                expectedIndeterminate)
            : null;
        return new MetricControlResult(
            controlId,
            description,
            expectedCollapsed,
            expectedOrientationReversed,
            expectedIndeterminate,
            observed,
            candidateComparison);
    }
}
