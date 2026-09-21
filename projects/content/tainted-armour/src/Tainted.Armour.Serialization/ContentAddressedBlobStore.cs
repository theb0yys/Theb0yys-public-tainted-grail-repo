using System;
using System.IO;
using Tainted.Armour.Canonical;

namespace Tainted.Armour.Serialization;

public sealed class ContentAddressedBlobStore
{
    private readonly string blobDirectory;

    public ContentAddressedBlobStore(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory)) throw new ArgumentException("Store root is required.", nameof(rootDirectory));
        blobDirectory = Path.Combine(Path.GetFullPath(rootDirectory), "blobs");
    }

    public string Store(CanonicalBlob blob)
    {
        if (blob == null) throw new ArgumentNullException(nameof(blob));
        ArtifactId actual = ArtifactHasher.ComputeSha256(blob.Bytes);
        if (actual != blob.Artifact) throw new InvalidDataException("Blob bytes do not match their ArtifactId.");

        Directory.CreateDirectory(blobDirectory);
        string path = GetPath(blob.Artifact);
        if (File.Exists(path))
        {
            VerifyExisting(path, blob.Artifact, blob.Bytes);
            return path;
        }

        string temporary = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(temporary, blob.Bytes);
            try
            {
                File.Copy(temporary, path, overwrite: false);
            }
            catch (IOException) when (File.Exists(path))
            {
                VerifyExisting(path, blob.Artifact, blob.Bytes);
            }
            catch (IOException exception)
            {
                throw CreateStoreException(path, temporary, exception);
            }
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }

        VerifyExisting(path, blob.Artifact, blob.Bytes);
        return path;
    }

    public byte[] Load(ArtifactId artifact)
    {
        string path = GetPath(artifact);
        if (!File.Exists(path)) throw new FileNotFoundException("Content-addressed blob was not found.", path);
        byte[] bytes = File.ReadAllBytes(NormalizeFileIoPath(path));
        ArtifactId actual = ArtifactHasher.ComputeSha256(bytes);
        if (actual != artifact) throw new InvalidDataException("Stored blob hash does not match its path identity.");
        return bytes;
    }

    public string GetPath(ArtifactId artifact)
    {
        if (!StringComparer.Ordinal.Equals(artifact.Algorithm, "sha256"))
        {
            throw new NotSupportedException($"Unsupported artifact algorithm {artifact.Algorithm}.");
        }

        return Path.Combine(blobDirectory, $"sha256-{artifact.DigestHex}.bin");
    }

    private static void VerifyExisting(string path, ArtifactId expected, byte[] expectedBytes)
    {
        byte[] existing = File.ReadAllBytes(NormalizeFileIoPath(path));
        ArtifactId actual = ArtifactHasher.ComputeSha256(existing);
        if (actual != expected || !existing.AsSpan().SequenceEqual(expectedBytes))
        {
            throw new InvalidDataException($"Existing content-addressed object is corrupt: {path}");
        }
    }

    private static string NormalizeFileIoPath(string path)
    {
        if (Path.DirectorySeparatorChar != '\\') return path;

        string full = Path.GetFullPath(path);
        if (full.StartsWith(@"\\?\", StringComparison.Ordinal)) return full;
        if (full.StartsWith(@"\\", StringComparison.Ordinal)) return @"\\?\UNC\" + full.Substring(2);
        return @"\\?\" + full;
    }

    private Exception CreateStoreException(string path, string temporary, IOException exception)
    {
        string directory = Path.GetDirectoryName(path) ?? string.Empty;
        var message = "Failed to store canonical blob."
            + " path=" + path
            + " directory=" + directory
            + " directoryExists=" + Directory.Exists(directory)
            + " blobDirectory=" + blobDirectory
            + " blobDirectoryExists=" + Directory.Exists(blobDirectory)
            + " temporary=" + temporary
            + " temporaryExists=" + File.Exists(temporary)
            + " currentDirectory=" + Directory.GetCurrentDirectory();
        return new IOException(message, exception);
    }
}
