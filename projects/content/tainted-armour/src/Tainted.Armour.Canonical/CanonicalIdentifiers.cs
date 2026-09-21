using System;
using System.Collections.Generic;

namespace Tainted.Armour.Canonical;

public static class CanonicalObjectKinds
{
    private static readonly HashSet<string> AllowedKinds = new HashSet<string>(StringComparer.Ordinal)
    {
        "asset",
        "buffer",
        "buffer-view",
        "accessor",
        "mesh",
        "primitive",
        "mesh-binding",
        "skeleton",
        "joint",
        "skin",
        "material",
        "resource",
        "blendshape-set",
        "blendshape",
        "blendshape-frame",
        "extension",
    };

    public static bool IsAllowed(string value) =>
        value != null && AllowedKinds.Contains(value);
}

public readonly struct CanonicalObjectId : IEquatable<CanonicalObjectId>, IComparable<CanonicalObjectId>
{
    public CanonicalObjectId(string value)
    {
        if (!TryValidate(value, out string kind))
        {
            throw new ArgumentException(
                "Canonical object IDs must use coid:v1:<allowed-kind>:<64-lowercase-hex>.",
                nameof(value));
        }

        Value = value;
        Kind = kind;
    }

    public string Value { get; }

    public string Kind { get; }

    public int CompareTo(CanonicalObjectId other) =>
        StringComparer.Ordinal.Compare(Value, other.Value);

    public bool Equals(CanonicalObjectId other) =>
        StringComparer.Ordinal.Equals(Value, other.Value);

    public override bool Equals(object? obj) =>
        obj is CanonicalObjectId other && Equals(other);

    public override int GetHashCode() =>
        Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

    public override string ToString() => Value ?? string.Empty;

    public static bool operator ==(CanonicalObjectId left, CanonicalObjectId right) => left.Equals(right);

    public static bool operator !=(CanonicalObjectId left, CanonicalObjectId right) => !left.Equals(right);

    private static bool TryValidate(string value, out string kind)
    {
        kind = string.Empty;
        if (string.IsNullOrWhiteSpace(value)) return false;

        string[] parts = value.Split(':');
        if (parts.Length != 4
            || !StringComparer.Ordinal.Equals(parts[0], "coid")
            || !StringComparer.Ordinal.Equals(parts[1], "v1")
            || !CanonicalObjectKinds.IsAllowed(parts[2])
            || !CanonicalIdentifierText.IsLowerHex64(parts[3]))
        {
            return false;
        }

        kind = parts[2];
        return true;
    }
}

public readonly struct ArtifactId : IEquatable<ArtifactId>, IComparable<ArtifactId>
{
    public ArtifactId(string algorithm, string digestHex)
    {
        if (!StringComparer.Ordinal.Equals(algorithm, "sha256"))
        {
            throw new ArgumentException("Canonical v1 supports sha256 artifact IDs only.", nameof(algorithm));
        }

        if (!CanonicalIdentifierText.IsLowerHex64(digestHex))
        {
            throw new ArgumentException("SHA-256 digests must be 64 lowercase hexadecimal characters.", nameof(digestHex));
        }

        Algorithm = algorithm;
        DigestHex = digestHex;
    }

    public string Algorithm { get; }

    public string DigestHex { get; }

    public int CompareTo(ArtifactId other) =>
        StringComparer.Ordinal.Compare(ToString(), other.ToString());

    public bool Equals(ArtifactId other) =>
        StringComparer.Ordinal.Equals(Algorithm, other.Algorithm)
        && StringComparer.Ordinal.Equals(DigestHex, other.DigestHex);

    public override bool Equals(object? obj) =>
        obj is ArtifactId other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(Algorithm ?? string.Empty);
            hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(DigestHex ?? string.Empty);
            return hash;
        }
    }

    public override string ToString() => $"{Algorithm}:{DigestHex}";

    public static ArtifactId Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Artifact ID is required.", nameof(value));
        string[] parts = value.Split(':');
        if (parts.Length != 2) throw new FormatException("Artifact ID must use <algorithm>:<digest>.");
        return new ArtifactId(parts[0], parts[1]);
    }

    public static bool operator ==(ArtifactId left, ArtifactId right) => left.Equals(right);

    public static bool operator !=(ArtifactId left, ArtifactId right) => !left.Equals(right);
}

internal static class CanonicalIdentifierText
{
    public static bool IsLowerHex64(string value)
    {
        if (value == null || value.Length != 64) return false;
        for (int index = 0; index < value.Length; index++)
        {
            char character = value[index];
            bool decimalDigit = character >= '0' && character <= '9';
            bool lowerHex = character >= 'a' && character <= 'f';
            if (!decimalDigit && !lowerHex) return false;
        }

        return true;
    }
}
