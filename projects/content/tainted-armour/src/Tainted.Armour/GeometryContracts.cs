using System;
using System.Collections.Generic;
using System.Linq;

namespace Tainted.Armour;

public readonly struct ArmorVector3 : IEquatable<ArmorVector3>
{
    public ArmorVector3(double x, double y, double z)
    {
        X = RequireFinite(x, nameof(x));
        Y = RequireFinite(y, nameof(y));
        Z = RequireFinite(z, nameof(z));
    }

    public double X { get; }

    public double Y { get; }

    public double Z { get; }

    public double Magnitude => Math.Sqrt((X * X) + (Y * Y) + (Z * Z));

    public static ArmorVector3 operator -(ArmorVector3 left, ArmorVector3 right) =>
        new ArmorVector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);

    public static ArmorVector3 Cross(ArmorVector3 left, ArmorVector3 right) =>
        new ArmorVector3(
            (left.Y * right.Z) - (left.Z * right.Y),
            (left.Z * right.X) - (left.X * right.Z),
            (left.X * right.Y) - (left.Y * right.X));

    public static double Dot(ArmorVector3 left, ArmorVector3 right) =>
        (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z);

    public static double Distance(ArmorVector3 left, ArmorVector3 right) => (left - right).Magnitude;

    public bool Equals(ArmorVector3 other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

    public override bool Equals(object? obj) => obj is ArmorVector3 other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 31) + X.GetHashCode();
            hash = (hash * 31) + Y.GetHashCode();
            hash = (hash * 31) + Z.GetHashCode();
            return hash;
        }
    }

    private static double RequireFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Vector components must be finite.");
        }

        return value;
    }
}

public readonly struct ArmorTriangle
{
    public ArmorTriangle(int ordinal, int vertex0, int vertex1, int vertex2)
    {
        if (ordinal < 0) throw new ArgumentOutOfRangeException(nameof(ordinal));
        if (vertex0 < 0) throw new ArgumentOutOfRangeException(nameof(vertex0));
        if (vertex1 < 0) throw new ArgumentOutOfRangeException(nameof(vertex1));
        if (vertex2 < 0) throw new ArgumentOutOfRangeException(nameof(vertex2));

        Ordinal = ordinal;
        Vertex0 = vertex0;
        Vertex1 = vertex1;
        Vertex2 = vertex2;
    }

    public int Ordinal { get; }

    public int Vertex0 { get; }

    public int Vertex1 { get; }

    public int Vertex2 { get; }
}

public sealed class PoseGeometrySnapshot
{
    private readonly ArmorVector3[] positions;

    public PoseGeometrySnapshot(
        string poseId,
        string clipId,
        double sampleTimeSeconds,
        string geometryFingerprint,
        IEnumerable<ArmorVector3> positions)
    {
        PoseId = RequireText(poseId, nameof(poseId));
        ClipId = RequireText(clipId, nameof(clipId));
        if (double.IsNaN(sampleTimeSeconds) || double.IsInfinity(sampleTimeSeconds) || sampleTimeSeconds < 0d)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleTimeSeconds));
        }

        SampleTimeSeconds = sampleTimeSeconds;
        GeometryFingerprint = RequireText(geometryFingerprint, nameof(geometryFingerprint));
        this.positions = positions?.ToArray() ?? throw new ArgumentNullException(nameof(positions));
        if (this.positions.Length == 0)
        {
            throw new ArgumentException("A geometry snapshot requires at least one position.", nameof(positions));
        }
    }

    public string PoseId { get; }

    public string ClipId { get; }

    public double SampleTimeSeconds { get; }

    public string GeometryFingerprint { get; }

    public IReadOnlyList<ArmorVector3> Positions => positions;

    internal ArmorVector3[] CopyPositions() => (ArmorVector3[])positions.Clone();

    private static string RequireText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", parameterName);
        return value.Trim();
    }
}
