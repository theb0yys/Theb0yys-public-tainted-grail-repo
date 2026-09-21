using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Tainted.Armour.Canonical;
using Tainted.Armour.Serialization;

namespace Tainted.Armour.R2PackageBuilder;

internal static class Program
{
    private const string CaptureMarker = "TAINTED_ARMOUR_REAL_SAMPLE_FULL_GEOMETRY_CAPTURE_READY_DOWNSTREAM_FALSE";
    private const string TargetProfileSchemaVersion = "tainted-armour.target-profile-fixture/1";

    private static int Main(string[] args)
    {
        try
        {
            Arguments parsed = Arguments.Parse(args);
            GeometryCapture capture = GeometryCapture.Read(parsed.CaptureRoot);
            FullGeometryCanonicalPackageV1 package = new FullGeometryCanonicalPackageBuilder().Build(
                new FullGeometryCanonicalPackageRequestV1(
                    capture.SourceObjectKey,
                    capture.MeshName,
                    capture.SubmeshIndex,
                    capture.ReadPositions(),
                    capture.ReadIndices()));

            CanonicalPackageWriteResult write = new CanonicalPackageWriter().WritePackage(
                parsed.OutputRoot,
                package.Manifest,
                package.Blobs);

            WriteTargetProfile(parsed.OutputRoot, parsed.TargetProfileId, parsed.SourceReceiptPath, capture, write.ManifestArtifact.ArtifactId);
            Console.WriteLine("TAINTED_ARMOUR_R2_PACKAGE_BUILDER_PASS manifest=" + write.ManifestArtifact.ArtifactId);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine("TAINTED_ARMOUR_R2_PACKAGE_BUILDER_FAILED " + exception);
            return 1;
        }
    }

    private static void WriteTargetProfile(
        string outputRoot,
        string targetProfileId,
        string sourceReceiptPath,
        GeometryCapture capture,
        ArtifactId manifestArtifactId)
    {
        ArtifactId targetProfileFingerprint = ArtifactHasher.ComputeSha256(Encoding.UTF8.GetBytes(
            TargetProfileSchemaVersion
            + "\0" + targetProfileId
            + "\0" + manifestArtifactId
            + "\0" + capture.SourceFingerprint
            + "\0" + capture.MeshName
            + "\0" + capture.SubmeshIndex.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        var payload = new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["canonicalPackage"] = ".",
            ["schemaVersion"] = TargetProfileSchemaVersion,
            ["sourceGeometryCapture"] = capture.RelativeCaptureManifestFrom(outputRoot),
            ["sourceReceipt"] = RelativePathFrom(outputRoot, sourceReceiptPath),
            ["targetProfileFingerprint"] = targetProfileFingerprint.ToString(),
            ["targetProfileId"] = targetProfileId,
        };

        string targetPath = Path.Combine(Path.GetFullPath(outputRoot), "target-profile.json");
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? ".");
        string payloadJson = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine;
        if (File.Exists(targetPath) && StringComparer.Ordinal.Equals(File.ReadAllText(targetPath, Encoding.UTF8), payloadJson))
        {
            return;
        }

        File.WriteAllText(targetPath, payloadJson, new UTF8Encoding(false));
    }

    private sealed class Arguments
    {
        private Arguments(string captureRoot, string outputRoot, string targetProfileId, string sourceReceiptPath)
        {
            CaptureRoot = captureRoot;
            OutputRoot = outputRoot;
            TargetProfileId = targetProfileId;
            SourceReceiptPath = sourceReceiptPath;
        }

        public string CaptureRoot { get; }
        public string OutputRoot { get; }
        public string TargetProfileId { get; }
        public string SourceReceiptPath { get; }

        public static Arguments Parse(string[] args)
        {
            string captureRoot = Value(args, "--capture");
            string outputRoot = Value(args, "--output");
            string sourceReceiptPath = Value(args, "--source-receipt");
            string targetProfileId = Value(args, "--target-profile-id", required: false);
            if (string.IsNullOrWhiteSpace(targetProfileId))
            {
                targetProfileId = "foa-dragon-knight-t56-self";
            }

            return new Arguments(
                Path.GetFullPath(captureRoot),
                Path.GetFullPath(outputRoot),
                targetProfileId,
                Path.GetFullPath(sourceReceiptPath));
        }

        private static string Value(string[] args, string name, bool required = true)
        {
            for (int index = 0; index < args.Length - 1; index++)
            {
                if (StringComparer.Ordinal.Equals(args[index], name)) return args[index + 1];
            }

            if (required) throw new ArgumentException("Missing " + name + ".");
            return string.Empty;
        }
    }

    private sealed class GeometryCapture
    {
        private readonly string root;
        private readonly string manifestPath;
        private readonly string positionsPath;
        private readonly string indicesPath;

        private GeometryCapture(
            string root,
            string manifestPath,
            string positionsPath,
            string indicesPath,
            string sourceObjectKey,
            string sourceFingerprint,
            string meshName,
            int vertexCount,
            int submeshIndex,
            int submeshTriangleCount)
        {
            this.root = root;
            this.manifestPath = manifestPath;
            this.positionsPath = positionsPath;
            this.indicesPath = indicesPath;
            SourceObjectKey = sourceObjectKey;
            SourceFingerprint = sourceFingerprint;
            MeshName = meshName;
            VertexCount = vertexCount;
            SubmeshIndex = submeshIndex;
            SubmeshTriangleCount = submeshTriangleCount;
        }

        public string SourceObjectKey { get; }
        public string SourceFingerprint { get; }
        public string MeshName { get; }
        public int VertexCount { get; }
        public int SubmeshIndex { get; }
        public int SubmeshTriangleCount { get; }

        public static GeometryCapture Read(string captureRoot)
        {
            string root = Path.GetFullPath(captureRoot);
            string manifestPath = Path.Combine(root, "capture.geometry.json");
            if (!File.Exists(manifestPath)) throw new FileNotFoundException("Geometry capture manifest was not found.", manifestPath);

            using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(manifestPath));
            JsonElement json = document.RootElement;
            RequireString(json, "marker", CaptureMarker);
            RequireFalse(json, "candidateMapApplicationExecuted");
            RequireFalse(json, "conversionExecuted");
            RequireFalse(json, "downstreamWritesExecuted");

            string positionsPath = Path.Combine(root, RequireString(json, "positionsBlobFile"));
            string indicesPath = Path.Combine(root, RequireString(json, "indicesBlobFile"));
            if (!File.Exists(positionsPath)) throw new FileNotFoundException("Geometry capture positions blob was not found.", positionsPath);
            if (!File.Exists(indicesPath)) throw new FileNotFoundException("Geometry capture indices blob was not found.", indicesPath);

            string meshName = RequireString(json, "sourceMeshName");
            string sourceFingerprint = RequireString(json, "sourceFingerprintBefore");
            int submeshIndex = json.GetProperty("sourceSubmeshIndex").GetInt32();
            return new GeometryCapture(
                root,
                manifestPath,
                positionsPath,
                indicesPath,
                "unity-real-sample:"
                    + RequireString(json, "sourceAssetPath")
                    + "|source=" + sourceFingerprint
                    + "|renderer=" + RequireString(json, "selectedRenderer")
                    + "|mesh=" + meshName
                    + "|submesh=" + submeshIndex.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "|baseline=" + RequireString(json, "baselineGeometryFingerprint"),
                sourceFingerprint,
                meshName,
                json.GetProperty("sourceVertexCount").GetInt32(),
                submeshIndex,
                json.GetProperty("sourceSubmeshTriangleCount").GetInt32());
        }

        public float[] ReadPositions()
        {
            byte[] bytes = File.ReadAllBytes(positionsPath);
            int expectedBytes = checked(VertexCount * 3 * sizeof(float));
            if (bytes.Length != expectedBytes)
            {
                throw new InvalidDataException($"Positions blob has {bytes.Length} bytes; expected {expectedBytes}.");
            }

            var values = new float[checked(VertexCount * 3)];
            for (int index = 0; index < values.Length; index++)
            {
                values[index] = BinaryPrimitives.ReadSingleLittleEndian(bytes.AsSpan(index * sizeof(float), sizeof(float)));
            }

            return values;
        }

        public uint[] ReadIndices()
        {
            byte[] bytes = File.ReadAllBytes(indicesPath);
            int expectedBytes = checked(SubmeshTriangleCount * 3 * sizeof(uint));
            if (bytes.Length != expectedBytes)
            {
                throw new InvalidDataException($"Indices blob has {bytes.Length} bytes; expected {expectedBytes}.");
            }

            var values = new uint[checked(SubmeshTriangleCount * 3)];
            for (int index = 0; index < values.Length; index++)
            {
                values[index] = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(index * sizeof(uint), sizeof(uint)));
            }

            return values;
        }

        public string RelativeCaptureManifestFrom(string outputRoot)
        {
            return RelativePathFrom(outputRoot, manifestPath);
        }

        private static string RequireString(JsonElement json, string name)
        {
            string? value = json.GetProperty(name).GetString();
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidDataException("Geometry capture field is missing: " + name + ".");
            return value;
        }

        private static void RequireString(JsonElement json, string name, string expected)
        {
            string value = RequireString(json, name);
            if (!StringComparer.Ordinal.Equals(value, expected))
            {
                throw new InvalidDataException($"Geometry capture field '{name}' was '{value}', expected '{expected}'.");
            }
        }

        private static void RequireFalse(JsonElement json, string name)
        {
            if (json.GetProperty(name).GetBoolean())
            {
                throw new InvalidDataException("Geometry capture boundary field must be false: " + name + ".");
            }
        }
    }

    private static string RelativePathFrom(string root, string path)
    {
        Uri rootUri = new(AppendDirectorySeparator(Path.GetFullPath(root)));
        Uri pathUri = new(Path.GetFullPath(path));
        return Uri.UnescapeDataString(rootUri.MakeRelativeUri(pathUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
    }

    private static string AppendDirectorySeparator(string value) =>
        value.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
            ? value
            : value + Path.DirectorySeparatorChar;
}
