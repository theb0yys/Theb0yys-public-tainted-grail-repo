using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tainted.Armour.Canonical;

namespace Tainted.Armour.Serialization;

public sealed class CanonicalPackageWriteResult
{
    public CanonicalPackageWriteResult(
        string packageDirectory,
        string manifestPath,
        CanonicalManifestArtifact manifestArtifact,
        IReadOnlyDictionary<ArtifactId, string> blobPaths)
    {
        PackageDirectory = packageDirectory ?? throw new ArgumentNullException(nameof(packageDirectory));
        ManifestPath = manifestPath ?? throw new ArgumentNullException(nameof(manifestPath));
        ManifestArtifact = manifestArtifact ?? throw new ArgumentNullException(nameof(manifestArtifact));
        BlobPaths = blobPaths ?? throw new ArgumentNullException(nameof(blobPaths));
    }

    public string PackageDirectory { get; }
    public string ManifestPath { get; }
    public CanonicalManifestArtifact ManifestArtifact { get; }
    public IReadOnlyDictionary<ArtifactId, string> BlobPaths { get; }
}

public sealed class CanonicalPackageWriter
{
    public const string PackageManifestFileName = "canonical.asset.json";

    public CanonicalPackageWriteResult WritePackage(
        string packageDirectory,
        CanonicalArmorManifestV1 manifest,
        IEnumerable<CanonicalBlob> blobs)
    {
        if (string.IsNullOrWhiteSpace(packageDirectory))
        {
            throw new ArgumentException("Package directory is required.", nameof(packageDirectory));
        }

        if (manifest == null) throw new ArgumentNullException(nameof(manifest));
        CanonicalBlob[] blobArray = blobs?.ToArray() ?? throw new ArgumentNullException(nameof(blobs));
        Dictionary<ArtifactId, CanonicalBlob> suppliedBlobs = IndexBlobs(blobArray);
        Dictionary<ArtifactId, long> required = manifest.Buffers.Values
            .GroupBy(buffer => buffer.Artifact)
            .ToDictionary(group => group.Key, group => group.First().ByteLength);

        foreach (CanonicalBufferBlobRefV1 buffer in manifest.Buffers.Values)
        {
            if (!suppliedBlobs.TryGetValue(buffer.Artifact, out CanonicalBlob? blob))
            {
                throw new InvalidDataException("Canonical package is missing referenced blob " + buffer.Artifact + ".");
            }

            if (blob.Bytes.Length != buffer.ByteLength)
            {
                throw new InvalidDataException(
                    $"Blob {buffer.Artifact} has {blob.Bytes.Length} bytes; manifest requires {buffer.ByteLength}.");
            }
        }

        foreach (ArtifactId artifact in suppliedBlobs.Keys)
        {
            if (!required.ContainsKey(artifact))
            {
                throw new InvalidDataException("Canonical package supplied an unreferenced blob " + artifact + ".");
            }
        }

        string root = Path.GetFullPath(packageDirectory);
        Directory.CreateDirectory(root);
        var blobStore = new ContentAddressedBlobStore(root);
        var storedPaths = new Dictionary<ArtifactId, string>();
        foreach (CanonicalBlob blob in suppliedBlobs.Values.OrderBy(value => value.Artifact))
        {
            storedPaths.Add(blob.Artifact, blobStore.Store(blob));
        }

        CanonicalManifestArtifact manifestArtifact = CanonicalManifestWriter.Write(manifest);
        string manifestPath = Path.Combine(root, PackageManifestFileName);
        WriteExactBytes(manifestPath, manifestArtifact.Bytes);
        return new CanonicalPackageWriteResult(root, manifestPath, manifestArtifact, storedPaths);
    }

    private static Dictionary<ArtifactId, CanonicalBlob> IndexBlobs(IEnumerable<CanonicalBlob> blobs)
    {
        var result = new Dictionary<ArtifactId, CanonicalBlob>();
        foreach (CanonicalBlob blob in blobs)
        {
            if (blob == null) throw new ArgumentException("Canonical package blob list contains null.", nameof(blobs));
            ArtifactId actual = ArtifactHasher.ComputeSha256(blob.Bytes);
            if (actual != blob.Artifact)
            {
                throw new InvalidDataException("Canonical package blob bytes do not match their artifact identity.");
            }

            if (result.ContainsKey(blob.Artifact))
            {
                throw new InvalidDataException("Canonical package supplied duplicate blob " + blob.Artifact + ".");
            }

            result.Add(blob.Artifact, blob);
        }

        return result;
    }

    private static void WriteExactBytes(string path, byte[] bytes)
    {
        string directory = Path.GetDirectoryName(path) ?? ".";
        Directory.CreateDirectory(directory);
        if (File.Exists(path))
        {
            byte[] existing = File.ReadAllBytes(path);
            if (existing.AsSpan().SequenceEqual(bytes))
            {
                return;
            }

            File.WriteAllBytes(path, bytes);
            return;
        }

        string temporary = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(temporary, bytes);
            File.Copy(temporary, path, overwrite: false);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
