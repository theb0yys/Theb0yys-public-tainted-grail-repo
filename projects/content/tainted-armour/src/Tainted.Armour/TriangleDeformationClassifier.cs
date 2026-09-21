using System;

namespace Tainted.Armour;

internal static class TriangleDeformationClassifier
{
    internal static TriangleDeformationEvidence Classify(
        string poseId,
        ArmorTriangle triangle,
        ArmorVector3[] baseline,
        ArmorVector3[] posed,
        DeformationPolicy policy)
    {
        if (!ContainsTriangle(baseline, triangle))
        {
            return Indeterminate(poseId, triangle, "baseline_triangle_index_out_of_range");
        }

        if (!ContainsTriangle(posed, triangle))
        {
            return Indeterminate(poseId, triangle, "posed_triangle_index_out_of_range");
        }

        MetricVector3 a = MetricVector3.From(baseline[triangle.Vertex0]);
        MetricVector3 b = MetricVector3.From(baseline[triangle.Vertex1]);
        MetricVector3 c = MetricVector3.From(baseline[triangle.Vertex2]);
        MetricVector3 pa = MetricVector3.From(posed[triangle.Vertex0]);
        MetricVector3 pb = MetricVector3.From(posed[triangle.Vertex1]);
        MetricVector3 pc = MetricVector3.From(posed[triangle.Vertex2]);

        MetricVector3 baselineNormal = MetricVector3.Cross(b - a, c - a);
        MetricVector3 posedNormal = MetricVector3.Cross(pb - pa, pc - pa);
        float baselineMagnitude = baselineNormal.Magnitude;
        float posedMagnitude = posedNormal.Magnitude;
        float baselineArea = baselineMagnitude * 0.5f;
        float posedArea = posedMagnitude * 0.5f;
        bool collapsed = posedArea <= (float)policy.TriangleAreaEpsilon;

        if (baselineMagnitude <= 0f)
        {
            return new TriangleDeformationEvidence(
                poseId,
                triangle.Ordinal,
                triangle.Vertex0,
                triangle.Vertex1,
                triangle.Vertex2,
                baselineArea,
                posedArea,
                0d,
                MetricVector3.Distance(a, pa),
                MetricVector3.Distance(b, pb),
                MetricVector3.Distance(c, pc),
                collapsed,
                false,
                true,
                "baseline_triangle_degenerate");
        }

        float normalizedDot = posedMagnitude <= 0f
            ? 0f
            : MetricVector3.Dot(baselineNormal.Normalized, posedNormal.Normalized);
        bool reversed = posedMagnitude > 0f && normalizedDot < (float)policy.OrientationDotThreshold;

        return new TriangleDeformationEvidence(
            poseId,
            triangle.Ordinal,
            triangle.Vertex0,
            triangle.Vertex1,
            triangle.Vertex2,
            baselineArea,
            posedArea,
            normalizedDot,
            MetricVector3.Distance(a, pa),
            MetricVector3.Distance(b, pb),
            MetricVector3.Distance(c, pc),
            collapsed,
            reversed,
            false,
            string.Empty);
    }

    internal static TriangleDeformationEvidence Indeterminate(string poseId, ArmorTriangle triangle, string reason) =>
        new TriangleDeformationEvidence(
            poseId,
            triangle.Ordinal,
            triangle.Vertex0,
            triangle.Vertex1,
            triangle.Vertex2,
            0d,
            0d,
            0d,
            0d,
            0d,
            0d,
            false,
            false,
            true,
            reason);

    internal static float Distance(ArmorVector3 left, ArmorVector3 right) =>
        MetricVector3.Distance(left, right);

    private static bool ContainsTriangle(ArmorVector3[] positions, ArmorTriangle triangle) =>
        triangle.Vertex0 < positions.Length
        && triangle.Vertex1 < positions.Length
        && triangle.Vertex2 < positions.Length;

    private readonly struct MetricVector3
    {
        private const float NormalizeEpsilon = 0.00001f;

        private MetricVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        private float X { get; }
        private float Y { get; }
        private float Z { get; }

        internal float Magnitude => (float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z));

        internal MetricVector3 Normalized
        {
            get
            {
                float magnitude = Magnitude;
                return magnitude > NormalizeEpsilon
                    ? new MetricVector3(X / magnitude, Y / magnitude, Z / magnitude)
                    : new MetricVector3(0f, 0f, 0f);
            }
        }

        internal static MetricVector3 From(ArmorVector3 value) =>
            new MetricVector3((float)value.X, (float)value.Y, (float)value.Z);

        public static MetricVector3 operator -(MetricVector3 left, MetricVector3 right) =>
            new MetricVector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

        internal static MetricVector3 Cross(MetricVector3 left, MetricVector3 right) =>
            new MetricVector3(
                (left.Y * right.Z) - (left.Z * right.Y),
                (left.Z * right.X) - (left.X * right.Z),
                (left.X * right.Y) - (left.Y * right.X));

        internal static float Dot(MetricVector3 left, MetricVector3 right) =>
            (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z);

        internal static float Distance(ArmorVector3 left, ArmorVector3 right) =>
            Distance(From(left), From(right));

        internal static float Distance(MetricVector3 left, MetricVector3 right) => (left - right).Magnitude;
    }
}
