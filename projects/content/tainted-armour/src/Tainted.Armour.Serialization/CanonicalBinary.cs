using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Tainted.Armour.Canonical;

namespace Tainted.Armour.Serialization;

public sealed class CanonicalBlob
{
    public CanonicalBlob(byte[] bytes, ArtifactId artifact)
    {
        Bytes = bytes == null ? throw new ArgumentNullException(nameof(bytes)) : (byte[])bytes.Clone();
        Artifact = artifact;
    }

    public byte[] Bytes { get; }
    public ArtifactId Artifact { get; }
}

public readonly struct CanonicalMatrix4x4F32
{
    public CanonicalMatrix4x4F32(
        float m00, float m01, float m02, float m03,
        float m10, float m11, float m12, float m13,
        float m20, float m21, float m22, float m23,
        float m30, float m31, float m32, float m33)
    {
        M00 = m00; M01 = m01; M02 = m02; M03 = m03;
        M10 = m10; M11 = m11; M12 = m12; M13 = m13;
        M20 = m20; M21 = m21; M22 = m22; M23 = m23;
        M30 = m30; M31 = m31; M32 = m32; M33 = m33;
    }

    public float M00 { get; } public float M01 { get; } public float M02 { get; } public float M03 { get; }
    public float M10 { get; } public float M11 { get; } public float M12 { get; } public float M13 { get; }
    public float M20 { get; } public float M21 { get; } public float M22 { get; } public float M23 { get; }
    public float M30 { get; } public float M31 { get; } public float M32 { get; } public float M33 { get; }
}

public static class CanonicalBinaryWriter
{
    public static CanonicalBlob WriteFloat32(IReadOnlyList<float> values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));
        byte[] bytes = new byte[checked(values.Count * sizeof(float))];
        for (int index = 0; index < values.Count; index++)
        {
            float value = values[index];
            RequireFinite(value, index);
            value = value == 0f ? 0f : value;
            BinaryPrimitives.WriteSingleLittleEndian(bytes.AsSpan(index * sizeof(float), sizeof(float)), value);
        }

        return CreateBlob(bytes);
    }

    public static CanonicalBlob WriteFloat64(IReadOnlyList<double> values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));
        byte[] bytes = new byte[checked(values.Count * sizeof(double))];
        for (int index = 0; index < values.Count; index++)
        {
            double value = values[index];
            RequireFinite(value, index);
            value = value == 0d ? 0d : value;
            BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(index * sizeof(double), sizeof(double)), value);
        }

        return CreateBlob(bytes);
    }

    public static CanonicalBlob WriteUInt16(IReadOnlyList<ushort> values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));
        byte[] bytes = new byte[checked(values.Count * sizeof(ushort))];
        for (int index = 0; index < values.Count; index++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * sizeof(ushort), sizeof(ushort)), values[index]);
        }

        return CreateBlob(bytes);
    }

    public static CanonicalBlob WriteUInt32(IReadOnlyList<uint> values)
    {
        if (values == null) throw new ArgumentNullException(nameof(values));
        byte[] bytes = new byte[checked(values.Count * sizeof(uint))];
        for (int index = 0; index < values.Count; index++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(index * sizeof(uint), sizeof(uint)), values[index]);
        }

        return CreateBlob(bytes);
    }

    public static CanonicalBlob WriteMatrix4x4F32(CanonicalMatrix4x4F32 value) =>
        WriteFloat32(new[]
        {
            value.M00, value.M10, value.M20, value.M30,
            value.M01, value.M11, value.M21, value.M31,
            value.M02, value.M12, value.M22, value.M32,
            value.M03, value.M13, value.M23, value.M33,
        });

    private static CanonicalBlob CreateBlob(byte[] bytes) =>
        new CanonicalBlob(bytes, ArtifactHasher.ComputeSha256(bytes));

    private static void RequireFinite(float value, int index)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            throw new InvalidOperationException($"F32 value at index {index} is not finite.");
        }
    }

    private static void RequireFinite(double value, int index)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new InvalidOperationException($"F64 value at index {index} is not finite.");
        }
    }
}

public sealed class CanonicalBinaryVerificationResult
{
    public CanonicalBinaryVerificationResult(bool accepted, string diagnostic)
    {
        Accepted = accepted;
        Diagnostic = diagnostic ?? string.Empty;
    }

    public bool Accepted { get; }
    public string Diagnostic { get; }
}

public static class CanonicalBinaryVerifier
{
    public static CanonicalBinaryVerificationResult Verify(
        CanonicalBlob blob,
        CanonicalComponentType componentType,
        CanonicalElementShape shape,
        long count)
    {
        if (blob == null) throw new ArgumentNullException(nameof(blob));
        try
        {
            int expectedLength = CanonicalBinaryLayout.CheckedByteLength(componentType, shape, count);
            if (blob.Bytes.Length != expectedLength)
            {
                return new CanonicalBinaryVerificationResult(false, $"Expected {expectedLength} bytes but found {blob.Bytes.Length}.");
            }

            byte[] digest = SHA256.HashData(blob.Bytes);
            string digestHex = Convert.ToHexString(digest).ToLowerInvariant();
            if (!StringComparer.Ordinal.Equals(digestHex, blob.Artifact.DigestHex))
            {
                return new CanonicalBinaryVerificationResult(false, "Blob SHA-256 does not match its ArtifactId.");
            }

            if (componentType == CanonicalComponentType.F32)
            {
                for (int offset = 0; offset < blob.Bytes.Length; offset += sizeof(float))
                {
                    int bits = BinaryPrimitives.ReadInt32LittleEndian(blob.Bytes.AsSpan(offset, sizeof(float)));
                    float value = BitConverter.Int32BitsToSingle(bits);
                    if (float.IsNaN(value) || float.IsInfinity(value))
                    {
                        return new CanonicalBinaryVerificationResult(false, $"Non-finite F32 at byte offset {offset}.");
                    }

                    if (bits == unchecked((int)0x80000000))
                    {
                        return new CanonicalBinaryVerificationResult(false, $"Negative zero F32 at byte offset {offset}.");
                    }
                }
            }
            else if (componentType == CanonicalComponentType.F64)
            {
                for (int offset = 0; offset < blob.Bytes.Length; offset += sizeof(double))
                {
                    long bits = BinaryPrimitives.ReadInt64LittleEndian(blob.Bytes.AsSpan(offset, sizeof(double)));
                    double value = BitConverter.Int64BitsToDouble(bits);
                    if (double.IsNaN(value) || double.IsInfinity(value))
                    {
                        return new CanonicalBinaryVerificationResult(false, $"Non-finite F64 at byte offset {offset}.");
                    }

                    if (bits == unchecked((long)0x8000000000000000))
                    {
                        return new CanonicalBinaryVerificationResult(false, $"Negative zero F64 at byte offset {offset}.");
                    }
                }
            }

            return new CanonicalBinaryVerificationResult(true, string.Empty);
        }
        catch (Exception exception) when (exception is OverflowException || exception is ArgumentOutOfRangeException)
        {
            return new CanonicalBinaryVerificationResult(false, exception.Message);
        }
    }

    public static CanonicalMatrix4x4F32 ReadMatrix4x4F32(CanonicalBlob blob)
    {
        CanonicalBinaryVerificationResult verification = Verify(blob, CanonicalComponentType.F32, CanonicalElementShape.Mat4, 1);
        if (!verification.Accepted) throw new InvalidDataException(verification.Diagnostic);

        float[] scalar = new float[16];
        for (int index = 0; index < scalar.Length; index++)
        {
            scalar[index] = BinaryPrimitives.ReadSingleLittleEndian(blob.Bytes.AsSpan(index * sizeof(float), sizeof(float)));
        }

        return new CanonicalMatrix4x4F32(
            scalar[0], scalar[4], scalar[8], scalar[12],
            scalar[1], scalar[5], scalar[9], scalar[13],
            scalar[2], scalar[6], scalar[10], scalar[14],
            scalar[3], scalar[7], scalar[11], scalar[15]);
    }
}

public static class CanonicalBinaryLayout
{
    public static int CheckedByteLength(CanonicalComponentType componentType, CanonicalElementShape shape, long count)
    {
        if (count < 0 || count > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(count));
        return checked((int)count * ComponentSize(componentType) * ComponentCount(shape));
    }

    public static int ComponentSize(CanonicalComponentType value) => value switch
    {
        CanonicalComponentType.I8 => 1,
        CanonicalComponentType.U8 => 1,
        CanonicalComponentType.I16 => 2,
        CanonicalComponentType.U16 => 2,
        CanonicalComponentType.I32 => 4,
        CanonicalComponentType.U32 => 4,
        CanonicalComponentType.F32 => 4,
        CanonicalComponentType.F64 => 8,
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    public static int ComponentCount(CanonicalElementShape value) => value switch
    {
        CanonicalElementShape.Scalar => 1,
        CanonicalElementShape.Vec2 => 2,
        CanonicalElementShape.Vec3 => 3,
        CanonicalElementShape.Vec4 => 4,
        CanonicalElementShape.Mat2 => 4,
        CanonicalElementShape.Mat3 => 9,
        CanonicalElementShape.Mat4 => 16,
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };
}

public enum CanonicalStorageEncoding
{
    ImplicitZero,
    Dense,
    Sparse,
}

public sealed class CanonicalStorageSelection
{
    public CanonicalStorageSelection(
        CanonicalStorageEncoding encoding,
        long denseByteLength,
        long sparseByteLength,
        int sparseIndexByteLength)
    {
        Encoding = encoding;
        DenseByteLength = denseByteLength;
        SparseByteLength = sparseByteLength;
        SparseIndexByteLength = sparseIndexByteLength;
    }

    public CanonicalStorageEncoding Encoding { get; }
    public long DenseByteLength { get; }
    public long SparseByteLength { get; }
    public int SparseIndexByteLength { get; }
}

public static class SparseEncodingSelector
{
    private const long MaximumU32AddressableElementCount = (long)uint.MaxValue + 1L;

    public static CanonicalStorageSelection Select(
        long elementCount,
        long nonZeroElementCount,
        long maximumChangedIndex,
        int elementByteLength)
    {
        if (elementCount < 0 || elementCount > MaximumU32AddressableElementCount)
        {
            throw new ArgumentOutOfRangeException(nameof(elementCount));
        }

        if (nonZeroElementCount < 0 || nonZeroElementCount > elementCount)
        {
            throw new ArgumentOutOfRangeException(nameof(nonZeroElementCount));
        }

        if (elementByteLength <= 0) throw new ArgumentOutOfRangeException(nameof(elementByteLength));

        long denseBytes = checked(elementCount * elementByteLength);
        if (nonZeroElementCount == 0)
        {
            if (maximumChangedIndex != -1) throw new ArgumentOutOfRangeException(nameof(maximumChangedIndex));
            return new CanonicalStorageSelection(CanonicalStorageEncoding.ImplicitZero, denseBytes, 0, 0);
        }

        if (maximumChangedIndex < 0 || maximumChangedIndex >= elementCount)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumChangedIndex));
        }

        int indexBytes = maximumChangedIndex <= byte.MaxValue
            ? 1
            : maximumChangedIndex <= ushort.MaxValue
                ? 2
                : 4;
        long sparseBytes = checked(nonZeroElementCount * (elementByteLength + indexBytes));
        CanonicalStorageEncoding encoding = sparseBytes < denseBytes
            ? CanonicalStorageEncoding.Sparse
            : CanonicalStorageEncoding.Dense;
        return new CanonicalStorageSelection(encoding, denseBytes, sparseBytes, indexBytes);
    }
}
