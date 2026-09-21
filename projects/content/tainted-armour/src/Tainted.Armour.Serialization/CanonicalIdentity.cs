using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Tainted.Armour.Canonical;

namespace Tainted.Armour.Serialization;

public interface ICanonicalDigestProvider
{
    ArtifactId Compute(ReadOnlySpan<byte> bytes);
}

public sealed class Sha256CanonicalDigestProvider : ICanonicalDigestProvider
{
    public static readonly Sha256CanonicalDigestProvider Instance = new Sha256CanonicalDigestProvider();

    private Sha256CanonicalDigestProvider()
    {
    }

    public ArtifactId Compute(ReadOnlySpan<byte> bytes) => ArtifactHasher.ComputeSha256(bytes);
}

public static class ArtifactHasher
{
    public static ArtifactId ComputeSha256(ReadOnlySpan<byte> bytes)
    {
        byte[] digest = SHA256.HashData(bytes);
        return new ArtifactId("sha256", Convert.ToHexString(digest).ToLowerInvariant());
    }
}

public static class CanonicalObjectIdDeriver
{
    private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

    public static CanonicalObjectId DeriveSource(
        string kind,
        string sourceAdapterContractId,
        string sourceObjectKey) =>
        Derive(kind, BuildSourcePreimage(kind, sourceAdapterContractId, sourceObjectKey), Sha256CanonicalDigestProvider.Instance);

    public static CanonicalObjectId DeriveGenerated(
        string kind,
        string producerContractId,
        CanonicalObjectId ownerCanonicalObjectId,
        string semanticRole,
        long deterministicOrdinal) =>
        Derive(
            kind,
            BuildGeneratedPreimage(kind, producerContractId, ownerCanonicalObjectId, semanticRole, deterministicOrdinal),
            Sha256CanonicalDigestProvider.Instance);

    internal static byte[] BuildSourcePreimage(
        string kind,
        string sourceAdapterContractId,
        string sourceObjectKey)
    {
        RequireAllowedKind(kind);
        RequireLowerAsciiContractId(sourceAdapterContractId, nameof(sourceAdapterContractId));
        RequireText(sourceObjectKey, nameof(sourceObjectKey));
        return StrictUtf8.GetBytes(
            "tainted-armour.coid/v1\0"
            + kind + "\0"
            + sourceAdapterContractId + "\0"
            + sourceObjectKey);
    }

    internal static byte[] BuildGeneratedPreimage(
        string kind,
        string producerContractId,
        CanonicalObjectId ownerCanonicalObjectId,
        string semanticRole,
        long deterministicOrdinal)
    {
        RequireAllowedKind(kind);
        RequireLowerAsciiContractId(producerContractId, nameof(producerContractId));
        RequireLowerAsciiContractId(semanticRole, nameof(semanticRole));
        if (deterministicOrdinal < 0) throw new ArgumentOutOfRangeException(nameof(deterministicOrdinal));

        string ordinal = deterministicOrdinal.ToString("D20", CultureInfo.InvariantCulture);
        return StrictUtf8.GetBytes(
            "tainted-armour.generated-coid/v1\0"
            + kind + "\0"
            + producerContractId + "\0"
            + ownerCanonicalObjectId.Value + "\0"
            + semanticRole + "\0"
            + ordinal);
    }

    internal static CanonicalObjectId Derive(
        string kind,
        ReadOnlySpan<byte> preimage,
        ICanonicalDigestProvider digestProvider)
    {
        if (digestProvider == null) throw new ArgumentNullException(nameof(digestProvider));
        RequireAllowedKind(kind);
        ArtifactId digest = digestProvider.Compute(preimage);
        if (!StringComparer.Ordinal.Equals(digest.Algorithm, "sha256"))
        {
            throw new InvalidOperationException("Canonical object ID v1 requires a SHA-256 digest provider.");
        }

        return new CanonicalObjectId($"coid:v1:{kind}:{digest.DigestHex}");
    }

    private static void RequireAllowedKind(string kind)
    {
        if (!CanonicalObjectKinds.IsAllowed(kind))
        {
            throw new ArgumentException("Canonical object kind is not allowed.", nameof(kind));
        }
    }

    private static void RequireText(string value, string parameterName)
    {
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("Value is required.", parameterName);
        _ = StrictUtf8.GetBytes(value);
    }

    private static void RequireLowerAsciiContractId(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value) || value != value.Trim())
        {
            throw new ArgumentException("Contract identifier is required and may not contain surrounding whitespace.", parameterName);
        }

        for (int index = 0; index < value.Length; index++)
        {
            char character = value[index];
            bool allowed = (character >= 'a' && character <= 'z')
                || (character >= '0' && character <= '9')
                || character == '.'
                || character == '-'
                || character == '_'
                || character == '/';
            if (!allowed)
            {
                throw new ArgumentException("Contract identifiers must use lowercase ASCII letters, digits, '.', '-', '_', or '/'.", parameterName);
            }
        }
    }
}

public sealed class CanonicalObjectIdRegistry
{
    private readonly ICanonicalDigestProvider digestProvider;
    private readonly HashSet<string> sourceKeys = new HashSet<string>(StringComparer.Ordinal);
    private readonly HashSet<string> generatedKeys = new HashSet<string>(StringComparer.Ordinal);
    private readonly Dictionary<CanonicalObjectId, byte[]> preimages = new Dictionary<CanonicalObjectId, byte[]>();

    public CanonicalObjectIdRegistry(ICanonicalDigestProvider? digestProvider = null)
    {
        this.digestProvider = digestProvider ?? Sha256CanonicalDigestProvider.Instance;
    }

    public CanonicalObjectId RegisterSource(
        string kind,
        string sourceAdapterContractId,
        string sourceObjectKey)
    {
        string registrationKey = kind + "\0" + sourceAdapterContractId + "\0" + sourceObjectKey;
        if (!sourceKeys.Add(registrationKey))
        {
            throw new InvalidOperationException(
                $"Duplicate source canonical-object key: kind='{kind}', adapter='{sourceAdapterContractId}', key='{sourceObjectKey}'.");
        }

        byte[] preimage = CanonicalObjectIdDeriver.BuildSourcePreimage(kind, sourceAdapterContractId, sourceObjectKey);
        return RegisterDerived(kind, preimage);
    }

    public CanonicalObjectId RegisterGenerated(
        string kind,
        string producerContractId,
        CanonicalObjectId ownerCanonicalObjectId,
        string semanticRole,
        long deterministicOrdinal)
    {
        string registrationKey = kind
            + "\0" + producerContractId
            + "\0" + ownerCanonicalObjectId.Value
            + "\0" + semanticRole
            + "\0" + deterministicOrdinal.ToString(CultureInfo.InvariantCulture);
        if (!generatedKeys.Add(registrationKey))
        {
            throw new InvalidOperationException(
                $"Duplicate generated canonical-object key: kind='{kind}', producer='{producerContractId}', owner='{ownerCanonicalObjectId}', role='{semanticRole}', ordinal={deterministicOrdinal}.");
        }

        byte[] preimage = CanonicalObjectIdDeriver.BuildGeneratedPreimage(
            kind,
            producerContractId,
            ownerCanonicalObjectId,
            semanticRole,
            deterministicOrdinal);
        return RegisterDerived(kind, preimage);
    }

    private CanonicalObjectId RegisterDerived(string kind, byte[] preimage)
    {
        CanonicalObjectId id = CanonicalObjectIdDeriver.Derive(kind, preimage, digestProvider);
        if (preimages.TryGetValue(id, out byte[]? existing))
        {
            if (!existing.AsSpan().SequenceEqual(preimage))
            {
                throw new InvalidOperationException(
                    $"Canonical object ID digest collision detected for '{id}'.");
            }

            throw new InvalidOperationException(
                $"Canonical object ID '{id}' was registered more than once.");
        }

        preimages.Add(id, (byte[])preimage.Clone());
        return id;
    }
}
