using System;

namespace Tainted.Armour;

internal static class CandidateSupplementalMetricClassifier
{
    private const double NormalizeEpsilon = 0.00001d;

    internal static CandidateMetricComparisonResult Compare(
        string controlId,
        ArmorTriangle triangle,
        ArmorVector3[] baseline,
        ArmorVector3[] posed,
        DeformationPolicy policy,
        ReferenceNormalTransport referenceTransport,
        TriangleDeformationEvidence current,
        bool expectedCollapsed,
        bool expectedOrientationReversed,
        bool expectedIndeterminate)
    {
        if (policy is null) throw new ArgumentNullException(nameof(policy));
        if (referenceTransport.Id is null) throw new ArgumentNullException(nameof(referenceTransport));
        if (current is null) throw new ArgumentNullException(nameof(current));

        if (!policy.SupplementalMetric.Enabled)
        {
            throw new InvalidOperationException("Candidate supplemental metric comparison is disabled.");
        }

        CandidateClassification candidate = Classify(
            triangle,
            baseline,
            posed,
            policy,
            referenceTransport);
        return new CandidateMetricComparisonResult(
            policy.SupplementalMetric.MetricVersion,
            "reference-transport/local-winding",
            referenceTransport.Id,
            expectedCollapsed,
            expectedOrientationReversed,
            expectedIndeterminate,
            current.Collapsed,
            current.OrientationReversed,
            current.Indeterminate,
            candidate.Collapsed,
            candidate.OrientationReversed,
            candidate.Indeterminate,
            candidate.IndeterminateReason,
            candidate.ReferenceTransportedNormalDot,
            policy.SupplementalMetric.AffectsAcceptance);
    }

    private static CandidateClassification Classify(
        ArmorTriangle triangle,
        ArmorVector3[] baseline,
        ArmorVector3[] posed,
        DeformationPolicy policy,
        ReferenceNormalTransport referenceTransport)
    {
        if (!ContainsTriangle(baseline, triangle))
        {
            return CandidateClassification.CreateIndeterminate("baseline_triangle_index_out_of_range");
        }

        if (!ContainsTriangle(posed, triangle))
        {
            return CandidateClassification.CreateIndeterminate("posed_triangle_index_out_of_range");
        }

        ArmorVector3 a = baseline[triangle.Vertex0];
        ArmorVector3 b = baseline[triangle.Vertex1];
        ArmorVector3 c = baseline[triangle.Vertex2];
        ArmorVector3 pa = posed[triangle.Vertex0];
        ArmorVector3 pb = posed[triangle.Vertex1];
        ArmorVector3 pc = posed[triangle.Vertex2];
        ArmorVector3 baselineNormal = ArmorVector3.Cross(b - a, c - a);
        ArmorVector3 posedNormal = ArmorVector3.Cross(pb - pa, pc - pa);
        double baselineMagnitude = baselineNormal.Magnitude;
        double posedMagnitude = posedNormal.Magnitude;
        bool collapsed = (posedMagnitude * 0.5d) <= policy.TriangleAreaEpsilon;

        if (baselineMagnitude <= 0d)
        {
            return new CandidateClassification(
                collapsed,
                orientationReversed: false,
                indeterminate: true,
                indeterminateReason: "baseline_triangle_degenerate",
                referenceTransportedNormalDot: 0d);
        }

        if (posedMagnitude <= 0d)
        {
            return new CandidateClassification(
                collapsed,
                orientationReversed: false,
                indeterminate: false,
                indeterminateReason: string.Empty,
                referenceTransportedNormalDot: 0d);
        }

        ArmorVector3 transportedBaselineNormal = referenceTransport.TransformNormal(baselineNormal);
        double transportedMagnitude = transportedBaselineNormal.Magnitude;
        if (transportedMagnitude <= 0d)
        {
            return new CandidateClassification(
                collapsed,
                orientationReversed: false,
                indeterminate: true,
                indeterminateReason: "reference_transported_normal_degenerate",
                referenceTransportedNormalDot: 0d);
        }

        double normalizedDot = ArmorVector3.Dot(
            Normalize(transportedBaselineNormal, transportedMagnitude),
            Normalize(posedNormal, posedMagnitude));
        bool orientationReversed = normalizedDot < policy.OrientationDotThreshold;
        return new CandidateClassification(
            collapsed,
            orientationReversed,
            indeterminate: false,
            indeterminateReason: string.Empty,
            referenceTransportedNormalDot: normalizedDot);
    }

    private static ArmorVector3 Normalize(ArmorVector3 value, double magnitude) =>
        magnitude > NormalizeEpsilon
            ? new ArmorVector3(value.X / magnitude, value.Y / magnitude, value.Z / magnitude)
            : new ArmorVector3(0d, 0d, 0d);

    private static bool ContainsTriangle(ArmorVector3[] positions, ArmorTriangle triangle) =>
        triangle.Vertex0 < positions.Length
        && triangle.Vertex1 < positions.Length
        && triangle.Vertex2 < positions.Length;

    private readonly struct CandidateClassification
    {
        internal CandidateClassification(
            bool collapsed,
            bool orientationReversed,
            bool indeterminate,
            string indeterminateReason,
            double referenceTransportedNormalDot)
        {
            Collapsed = collapsed;
            OrientationReversed = orientationReversed;
            Indeterminate = indeterminate;
            IndeterminateReason = indeterminateReason ?? string.Empty;
            ReferenceTransportedNormalDot = referenceTransportedNormalDot;
        }

        internal bool Collapsed { get; }

        internal bool OrientationReversed { get; }

        internal bool Indeterminate { get; }

        internal string IndeterminateReason { get; }

        internal double ReferenceTransportedNormalDot { get; }

        internal static CandidateClassification CreateIndeterminate(string reason) =>
            new CandidateClassification(
                collapsed: false,
                orientationReversed: false,
                indeterminate: true,
                indeterminateReason: reason,
                referenceTransportedNormalDot: 0d);
    }
}

internal readonly struct ReferenceNormalTransport
{
    private readonly double m00;
    private readonly double m01;
    private readonly double m02;
    private readonly double m10;
    private readonly double m11;
    private readonly double m12;
    private readonly double m20;
    private readonly double m21;
    private readonly double m22;

    private ReferenceNormalTransport(
        string id,
        double m00,
        double m01,
        double m02,
        double m10,
        double m11,
        double m12,
        double m20,
        double m21,
        double m22)
    {
        Id = ContractValues.RequireText(id, nameof(id));
        this.m00 = m00;
        this.m01 = m01;
        this.m02 = m02;
        this.m10 = m10;
        this.m11 = m11;
        this.m12 = m12;
        this.m20 = m20;
        this.m21 = m21;
        this.m22 = m22;
    }

    internal string Id { get; }

    internal ArmorVector3 TransformNormal(ArmorVector3 value) =>
        new ArmorVector3(
            (m00 * value.X) + (m01 * value.Y) + (m02 * value.Z),
            (m10 * value.X) + (m11 * value.Y) + (m12 * value.Z),
            (m20 * value.X) + (m21 * value.Y) + (m22 * value.Z));

    internal static ReferenceNormalTransport Identity(string id = "identity") =>
        new ReferenceNormalTransport(
            id,
            1d, 0d, 0d,
            0d, 1d, 0d,
            0d, 0d, 1d);

    internal static ReferenceNormalTransport RotateZ90() =>
        new ReferenceNormalTransport(
            "rotate_z_90",
            0d, -1d, 0d,
            1d, 0d, 0d,
            0d, 0d, 1d);

    internal static ReferenceNormalTransport RotateX180() =>
        new ReferenceNormalTransport(
            "rotate_x_180",
            1d, 0d, 0d,
            0d, -1d, 0d,
            0d, 0d, -1d);
}
