using System;
using System.Linq;
using Tainted.Armour.Canonical;
using Tainted.Armour.Serialization;

namespace Tainted.Armour.Compatibility;

public sealed class R2CanonicalArmorArtifactV1
{
    public const string ContractVersionValue = "tainted-armour.r2-canonical-armor-artifact/1";

    public R2CanonicalArmorArtifactV1(
        string profileId,
        CanonicalArmorManifestV1 manifest,
        CanonicalManifestArtifact manifestArtifact)
    {
        ProfileId = CompatibilityContractText.Require(profileId, nameof(profileId));
        Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        ManifestArtifact = manifestArtifact ?? throw new ArgumentNullException(nameof(manifestArtifact));
        CanonicalManifestArtifact observed = CanonicalManifestWriter.Write(Manifest);
        if (observed.ArtifactId != ManifestArtifact.ArtifactId)
        {
            throw new ArgumentException(
                "R2 canonical armour artifact identity does not match current canonical manifest bytes.",
                nameof(manifestArtifact));
        }

        if (!observed.Bytes.AsSpan().SequenceEqual(ManifestArtifact.Bytes))
        {
            throw new ArgumentException(
                "R2 canonical armour artifact bytes do not match current canonical manifest bytes.",
                nameof(manifestArtifact));
        }
    }

    public string ContractVersion => ContractVersionValue;
    public string ProfileId { get; }
    public CanonicalArmorManifestV1 Manifest { get; }
    public CanonicalManifestArtifact ManifestArtifact { get; }
    public ArtifactId ManifestArtifactId => ManifestArtifact.ArtifactId;
}

public sealed class FingerprintedTargetProfileV1
{
    public const string ContractVersionValue = "tainted-armour.fingerprinted-target-profile/1";

    public FingerprintedTargetProfileV1(
        string targetProfileId,
        ArtifactId targetProfileFingerprint,
        R2CanonicalArmorArtifactV1 canonicalArtifact)
    {
        TargetProfileId = CompatibilityContractText.Require(targetProfileId, nameof(targetProfileId));
        TargetProfileFingerprint = targetProfileFingerprint;
        CanonicalArtifact = canonicalArtifact ?? throw new ArgumentNullException(nameof(canonicalArtifact));
    }

    public string ContractVersion => ContractVersionValue;
    public string TargetProfileId { get; }
    public ArtifactId TargetProfileFingerprint { get; }
    public R2CanonicalArmorArtifactV1 CanonicalArtifact { get; }
}

public sealed class CompatibilityInputAdapter
{
    public const string AdapterContractVersion = "tainted-armour.compatibility-input-adapter/1";

    public SourceTargetCompatibilityRequestV1 CreateRequest(
        R2CanonicalArmorArtifactV1 sourceArtifact,
        FingerprintedTargetProfileV1 targetProfile,
        SourceTargetCompatibilityPolicyV1? policy = null)
    {
        if (sourceArtifact == null) throw new ArgumentNullException(nameof(sourceArtifact));
        if (targetProfile == null) throw new ArgumentNullException(nameof(targetProfile));

        return new SourceTargetCompatibilityRequestV1(
            new SourceCompatibilityProfileV1(
                sourceArtifact.ProfileId,
                sourceArtifact.Manifest,
                sourceArtifact.ManifestArtifact),
            new TargetCompatibilityProfileV1(
                targetProfile.TargetProfileId,
                targetProfile.TargetProfileFingerprint,
                targetProfile.CanonicalArtifact.Manifest,
                targetProfile.CanonicalArtifact.ManifestArtifact),
            policy,
            candidateMapApplicationAllowed: false,
            candidateMapApplicationExecuted: false,
            conversionAllowed: false,
            conversionExecuted: false,
            itemEquipMutationAllowed: false,
            itemEquipMutationExecuted: false,
            saveMutationAllowed: false,
            saveMutationExecuted: false,
            downstreamWritesAllowed: false,
            downstreamWritesExecuted: false);
    }
}
