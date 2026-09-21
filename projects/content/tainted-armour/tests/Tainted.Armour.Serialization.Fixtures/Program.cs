using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Tainted.Armour.Canonical;
using Tainted.Armour.Serialization;

namespace Tainted.Armour.Serialization.Fixtures;

internal static class Program
{
    private const string AdapterContract = "com.tainted.armour.synthetic/1";
    private const string WorkspaceDirectoryName = "tainted-armour-r2-fixture";

    private static int Main(string[] args)
    {
        string workspace = ResolveWorkspace(args);
        if (Directory.Exists(workspace)) Directory.Delete(workspace, true);
        Directory.CreateDirectory(workspace);

        try
        {
            var tests = new (string Name, Action Execute)[]
            {
                ("canonical object ID derivation", CanonicalObjectIdDerivation),
                ("SHA-256 known vector", Sha256KnownVector),
                ("RFC-8785 property ordering", CanonicalJsonOrdering),
                ("RFC-8785 number vectors", CanonicalJsonNumberVectors),
                ("canonical JSON verifier", CanonicalJsonVerification),
                ("manifest order independence", ManifestOrderIndependence),
                ("R1 manifest structure", R1ManifestStructure),
                ("MAT4 column-major scalar serialization", MatrixSerialization),
                ("negative-zero normalization", NegativeZeroNormalization),
                ("non-finite rejection", NonFiniteRejection),
                ("independent binary verifier", BinaryVerifierRejectsNonCanonicalZero),
                ("implicit-zero accessor contract", ImplicitZeroAccessorContract),
                ("deterministic sparse/dense selection", SparseDenseSelection),
                ("content-addressed blob storage", () => BlobStoreRoundTrip(workspace)),
                ("cross-workspace repeatability", () => CrossWorkspaceRepeatability(workspace)),
            };

            foreach ((string name, Action execute) in tests)
            {
                execute();
                Console.WriteLine($"PASS {name}");
            }

            FixturePackage finalFixture = BuildFixture(reverseInsertionOrder: false);
            Console.WriteLine($"R2_CANONICAL_MANIFEST={finalFixture.Manifest.ArtifactId}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
        finally
        {
            if (Directory.Exists(workspace)) Directory.Delete(workspace, true);
        }
    }

    private static void CanonicalObjectIdDerivation()
    {
        CanonicalObjectId first = CanonicalObjectIdDeriver.DeriveSource("mesh", AdapterContract, "asset/root/mesh/0000");
        CanonicalObjectId second = CanonicalObjectIdDeriver.DeriveSource("mesh", AdapterContract, "asset/root/mesh/0000");
        Check(first == second, "Repeated source-object ID derivation differed.");

        byte[] preimage = Encoding.UTF8.GetBytes(
            "tainted-armour.coid/v1\0mesh\0"
            + AdapterContract
            + "\0asset/root/mesh/0000");
        string expectedDigest = Convert.ToHexString(SHA256.HashData(preimage)).ToLowerInvariant();
        Check(first.Value == $"coid:v1:mesh:{expectedDigest}", "Source-object ID preimage or domain separator changed.");
    }

    private static void Sha256KnownVector()
    {
        ArtifactId value = ArtifactHasher.ComputeSha256(Encoding.UTF8.GetBytes("abc"));
        Check(
            value.ToString() == "sha256:ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad",
            "SHA-256 known vector mismatch.");
    }

    private static void CanonicalJsonOrdering()
    {
        byte[] canonical = Rfc8785CanonicalJson.Canonicalize(Encoding.UTF8.GetBytes("{\"b\":2,\"a\":1}"));
        Check(Encoding.UTF8.GetString(canonical) == "{\"a\":1,\"b\":2}", "Object properties were not ordered canonically.");
    }

    private static void CanonicalJsonNumberVectors()
    {
        CheckCanonicalJson("1e30", "1e+30");
        CheckCanonicalJson("1e-7", "1e-7");
        CheckCanonicalJson("1e-6", "0.000001");
        CheckCanonicalJson("1e20", "100000000000000000000");
        CheckCanonicalJson("1e21", "1e+21");
        CheckCanonicalJson("-0", "0");
        CheckCanonicalJson("333333333.33333329", "333333333.3333333");
        CheckCanonicalJson("4.50", "4.5");
    }

    private static void CanonicalJsonVerification()
    {
        byte[] nonCanonical = Encoding.UTF8.GetBytes("{\"b\":2,\"a\":1}");
        byte[] canonical = Encoding.UTF8.GetBytes("{\"a\":1,\"b\":2}");
        Check(!CanonicalJsonVerifier.Verify(nonCanonical).Accepted, "Verifier accepted non-canonical property order.");
        Check(CanonicalJsonVerifier.Verify(canonical).Accepted, "Verifier rejected canonical JSON.");
        CheckThrows<InvalidDataException>(
            () => Rfc8785CanonicalJson.Canonicalize(Encoding.UTF8.GetBytes("{\"a\":1,\"a\":2}")),
            "Duplicate JSON properties were accepted.");
        byte[] normalizedNumber = Rfc8785CanonicalJson.Canonicalize(Encoding.UTF8.GetBytes("{\"a\":1e0}"));
        Check(Encoding.UTF8.GetString(normalizedNumber) == "{\"a\":1}", "Exponent-form integer did not canonicalize to the shortest number.");
        Check(!CanonicalJsonVerifier.Verify(Encoding.UTF8.GetBytes("{\"a\":1e0}")).Accepted, "Verifier accepted non-canonical exponent-form integer bytes.");
    }

    private static void ManifestOrderIndependence()
    {
        FixturePackage first = BuildFixture(reverseInsertionOrder: false);
        FixturePackage second = BuildFixture(reverseInsertionOrder: true);
        Check(first.Manifest.Bytes.AsSpan().SequenceEqual(second.Manifest.Bytes), "Dictionary insertion order changed manifest bytes.");
        Check(first.Manifest.ArtifactId == second.Manifest.ArtifactId, "Dictionary insertion order changed manifest identity.");
        Check(CanonicalJsonVerifier.Verify(first.Manifest.Bytes).Accepted, "Manifest writer did not emit canonical JSON.");
    }

    private static void R1ManifestStructure()
    {
        FixturePackage fixture = BuildFixture(reverseInsertionOrder: false);
        using JsonDocument document = JsonDocument.Parse(fixture.Manifest.Bytes);
        JsonElement root = document.RootElement;

        JsonElement coordinate = root.GetProperty("coordinateSystem");
        Check(coordinate.GetProperty("matrixMathConvention").GetString() == "COLUMN_VECTOR", "Manifest omitted the R1 matrix-math convention.");
        Check(coordinate.GetProperty("matrixStorageConvention").GetString() == "COLUMN_MAJOR", "Manifest omitted the R1 matrix-storage convention.");
        Check(coordinate.GetProperty("quaternionComponentOrder").GetString() == "XYZW", "Manifest omitted quaternion component order.");

        CanonicalObjectId positionViewId = Id("buffer-view", "view/positions");
        JsonElement positionView = root.GetProperty("bufferViews").GetProperty(positionViewId.Value);
        Check(positionView.GetProperty("byteOffset").GetInt64() == 0, "Canonical buffer view did not use byte offset zero.");
        Check(positionView.GetProperty("byteStride").ValueKind == JsonValueKind.Null, "Canonical buffer view did not emit a null stride.");

        CanonicalObjectId positionAccessorId = Id("accessor", "accessor/positions");
        JsonElement positionAccessor = root.GetProperty("accessors").GetProperty(positionAccessorId.Value);
        Check(positionAccessor.GetProperty("elementShape").GetString() == "VEC3", "Accessor used the wrong R1 element-shape field.");
        Check(positionAccessor.GetProperty("bufferViewId").GetString() == positionViewId.Value, "Accessor lost its buffer-view relationship.");
        Check(positionAccessor.GetProperty("sparse").ValueKind == JsonValueKind.Null, "Dense accessor did not emit sparse:null.");

        CanonicalObjectId meshId = Id("mesh", "mesh/triangle");
        JsonElement mesh = root.GetProperty("meshes").GetProperty(meshId.Value);
        Check(mesh.GetProperty("bounds").GetProperty("minimum").GetArrayLength() == 3, "Mesh bounds were not emitted.");
        JsonElement primitive = mesh.GetProperty("primitives")[0];
        CanonicalObjectId indexAccessorId = Id("accessor", "accessor/indices");
        Check(primitive.GetProperty("indicesAccessorId").GetString() == indexAccessorId.Value, "Primitive lost its mandatory index accessor.");

        CanonicalObjectId bindingId = Id("mesh-binding", "binding/triangle");
        JsonElement binding = root.GetProperty("meshBindings").GetProperty(bindingId.Value);
        Check(binding.GetProperty("meshId").GetString() == meshId.Value, "Mesh binding referenced the wrong mesh.");
        Check(binding.GetProperty("skinId").ValueKind == JsonValueKind.Null, "Unskinned mesh binding did not emit skinId:null.");
    }

    private static void MatrixSerialization()
    {
        CanonicalMatrix4x4F32 matrix = new CanonicalMatrix4x4F32(
            0, 1, 2, 3,
            4, 5, 6, 7,
            8, 9, 10, 11,
            12, 13, 14, 15);
        CanonicalBlob blob = CanonicalBinaryWriter.WriteMatrix4x4F32(matrix);
        float[] expected = { 0, 4, 8, 12, 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15 };
        for (int index = 0; index < expected.Length; index++)
        {
            float observed = BinaryPrimitives.ReadSingleLittleEndian(blob.Bytes.AsSpan(index * sizeof(float), sizeof(float)));
            Check(observed == expected[index], $"MAT4 scalar {index} was {observed} instead of {expected[index]}.");
        }

        CanonicalMatrix4x4F32 roundTrip = CanonicalBinaryVerifier.ReadMatrix4x4F32(blob);
        Check(roundTrip.M01 == 1 && roundTrip.M10 == 4 && roundTrip.M33 == 15, "MAT4 independent reader reconstructed the wrong matrix.");
    }

    private static void NegativeZeroNormalization()
    {
        CanonicalBlob f32 = CanonicalBinaryWriter.WriteFloat32(new[] { -0f });
        CanonicalBlob f64 = CanonicalBinaryWriter.WriteFloat64(new[] { -0d });
        Check(BinaryPrimitives.ReadInt32LittleEndian(f32.Bytes) == 0, "F32 negative zero was not normalized.");
        Check(BinaryPrimitives.ReadInt64LittleEndian(f64.Bytes) == 0, "F64 negative zero was not normalized.");
    }

    private static void NonFiniteRejection()
    {
        CheckThrows<InvalidOperationException>(
            () => CanonicalBinaryWriter.WriteFloat32(new[] { float.NaN }),
            "NaN F32 was accepted.");
        CheckThrows<InvalidOperationException>(
            () => CanonicalBinaryWriter.WriteFloat64(new[] { double.PositiveInfinity }),
            "Infinite F64 was accepted.");
    }

    private static void BinaryVerifierRejectsNonCanonicalZero()
    {
        byte[] bytes = new byte[sizeof(float)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, unchecked((int)0x80000000));
        CanonicalBlob blob = new CanonicalBlob(bytes, ArtifactHasher.ComputeSha256(bytes));
        CanonicalBinaryVerificationResult result = CanonicalBinaryVerifier.Verify(
            blob,
            CanonicalComponentType.F32,
            CanonicalElementShape.Scalar,
            1);
        Check(!result.Accepted && result.Diagnostic.Contains("Negative zero", StringComparison.Ordinal), "Binary verifier accepted non-canonical negative zero.");
    }

    private static void ImplicitZeroAccessorContract()
    {
        CanonicalAccessorV1 accessor = new CanonicalAccessorV1(
            Id("accessor", "accessor/implicit-zero"),
            null,
            CanonicalComponentType.F32,
            CanonicalElementShape.Vec3,
            64,
            null);
        Check(!accessor.BufferViewId.HasValue, "Implicit-zero accessor unexpectedly referenced dense storage.");
        Check(accessor.Sparse == null, "Implicit-zero accessor unexpectedly referenced sparse storage.");
        Check(accessor.Count == 64, "Implicit-zero accessor lost its semantic count.");
    }

    private static void SparseDenseSelection()
    {
        CanonicalStorageSelection sparse = SparseEncodingSelector.Select(4, 2, 3, 2);
        CanonicalStorageSelection tied = SparseEncodingSelector.Select(4, 2, 1, 1);
        CanonicalStorageSelection dense = SparseEncodingSelector.Select(4, 3, 2, 1);
        CanonicalStorageSelection zero = SparseEncodingSelector.Select(64, 0, -1, 12);
        Check(sparse.Encoding == CanonicalStorageEncoding.Sparse, "Smaller sparse representation was not selected.");
        Check(tied.Encoding == CanonicalStorageEncoding.Dense, "Dense representation must win a byte-size tie.");
        Check(dense.Encoding == CanonicalStorageEncoding.Dense, "Larger sparse representation was selected.");
        Check(zero.Encoding == CanonicalStorageEncoding.ImplicitZero, "All-zero data must use an implicit-zero accessor.");
        Check(zero.SparseByteLength == 0 && zero.SparseIndexByteLength == 0, "Implicit-zero selection emitted sparse storage metadata.");
        CheckThrows<ArgumentOutOfRangeException>(
            () => SparseEncodingSelector.Select((long)uint.MaxValue + 2L, 0, -1, 1),
            "Sparse selection accepted an element count beyond U32 addressability.");
    }

    private static void BlobStoreRoundTrip(string workspace)
    {
        string root = Path.Combine(workspace, "store-round-trip");
        ContentAddressedBlobStore store = new ContentAddressedBlobStore(root);
        CanonicalBlob blob = CanonicalBinaryWriter.WriteFloat32(new[] { 1f, 2f, 3f });
        string firstPath = store.Store(blob);
        string secondPath = store.Store(blob);
        Check(StringComparer.OrdinalIgnoreCase.Equals(firstPath, secondPath), "Identical blobs did not deduplicate to one path.");
        Check(store.Load(blob.Artifact).AsSpan().SequenceEqual(blob.Bytes), "Stored blob round-trip changed bytes.");

        File.WriteAllBytes(firstPath, new byte[] { 0xFF });
        CheckThrows<InvalidDataException>(() => store.Load(blob.Artifact), "Corrupt content-addressed blob was accepted.");
    }

    private static void CrossWorkspaceRepeatability(string workspace)
    {
        FixturePackage first = BuildFixture(reverseInsertionOrder: false);
        FixturePackage second = BuildFixture(reverseInsertionOrder: true);
        string rootA = Path.Combine(workspace, "workspace-a");
        string rootB = Path.Combine(workspace, "workspace-b");
        ContentAddressedBlobStore storeA = new ContentAddressedBlobStore(rootA);
        ContentAddressedBlobStore storeB = new ContentAddressedBlobStore(rootB);

        foreach (CanonicalBlob blob in first.Blobs)
        {
            string pathA = storeA.Store(blob);
            string pathB = storeB.Store(blob);
            Check(Path.GetFileName(pathA) == Path.GetFileName(pathB), "Workspace root changed content-addressed blob name.");
        }

        Directory.CreateDirectory(rootA);
        Directory.CreateDirectory(rootB);
        File.WriteAllBytes(Path.Combine(rootA, "canonical.asset.json"), first.Manifest.Bytes);
        File.WriteAllBytes(Path.Combine(rootB, "canonical.asset.json"), second.Manifest.Bytes);
        Check(first.Manifest.ArtifactId == second.Manifest.ArtifactId, "Workspace root changed canonical manifest identity.");
        Check(first.Manifest.Bytes.AsSpan().SequenceEqual(second.Manifest.Bytes), "Workspace root changed canonical manifest bytes.");
    }

    private static FixturePackage BuildFixture(bool reverseInsertionOrder)
    {
        CanonicalObjectId assetId = Id("asset", "asset/root");
        CanonicalObjectId positionBufferId = Id("buffer", "buffer/positions");
        CanonicalObjectId indexBufferId = Id("buffer", "buffer/indices");
        CanonicalObjectId matrixBufferId = Id("buffer", "buffer/matrix");
        CanonicalObjectId positionViewId = Id("buffer-view", "view/positions");
        CanonicalObjectId indexViewId = Id("buffer-view", "view/indices");
        CanonicalObjectId matrixViewId = Id("buffer-view", "view/matrix");
        CanonicalObjectId positionAccessorId = Id("accessor", "accessor/positions");
        CanonicalObjectId indexAccessorId = Id("accessor", "accessor/indices");
        CanonicalObjectId matrixAccessorId = Id("accessor", "accessor/matrix");
        CanonicalObjectId meshId = Id("mesh", "mesh/triangle");
        CanonicalObjectId primitiveId = Id("primitive", "mesh/triangle/primitive/0000");
        CanonicalObjectId bindingId = Id("mesh-binding", "binding/triangle");

        CanonicalBlob positions = CanonicalBinaryWriter.WriteFloat32(new[]
        {
            0f, 0f, 0f,
            1f, 0f, 0f,
            0f, 1f, 0f,
        });
        CanonicalBlob indices = CanonicalBinaryWriter.WriteUInt16(new ushort[] { 0, 1, 2 });
        CanonicalBlob matrix = CanonicalBinaryWriter.WriteMatrix4x4F32(new CanonicalMatrix4x4F32(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1));

        Dictionary<CanonicalObjectId, CanonicalBufferBlobRefV1> buffers = new Dictionary<CanonicalObjectId, CanonicalBufferBlobRefV1>();
        Dictionary<CanonicalObjectId, CanonicalBufferViewV1> views = new Dictionary<CanonicalObjectId, CanonicalBufferViewV1>();
        Dictionary<CanonicalObjectId, CanonicalAccessorV1> accessors = new Dictionary<CanonicalObjectId, CanonicalAccessorV1>();
        if (reverseInsertionOrder)
        {
            buffers.Add(matrixBufferId, new CanonicalBufferBlobRefV1(matrixBufferId, matrix.Artifact, matrix.Bytes.Length));
            buffers.Add(indexBufferId, new CanonicalBufferBlobRefV1(indexBufferId, indices.Artifact, indices.Bytes.Length));
            buffers.Add(positionBufferId, new CanonicalBufferBlobRefV1(positionBufferId, positions.Artifact, positions.Bytes.Length));
            views.Add(matrixViewId, new CanonicalBufferViewV1(matrixViewId, matrixBufferId, matrix.Bytes.Length));
            views.Add(indexViewId, new CanonicalBufferViewV1(indexViewId, indexBufferId, indices.Bytes.Length));
            views.Add(positionViewId, new CanonicalBufferViewV1(positionViewId, positionBufferId, positions.Bytes.Length));
            accessors.Add(matrixAccessorId, new CanonicalAccessorV1(matrixAccessorId, matrixViewId, CanonicalComponentType.F32, CanonicalElementShape.Mat4, 1));
            accessors.Add(indexAccessorId, new CanonicalAccessorV1(indexAccessorId, indexViewId, CanonicalComponentType.U16, CanonicalElementShape.Scalar, 3));
            accessors.Add(positionAccessorId, new CanonicalAccessorV1(positionAccessorId, positionViewId, CanonicalComponentType.F32, CanonicalElementShape.Vec3, 3));
        }
        else
        {
            buffers.Add(positionBufferId, new CanonicalBufferBlobRefV1(positionBufferId, positions.Artifact, positions.Bytes.Length));
            buffers.Add(indexBufferId, new CanonicalBufferBlobRefV1(indexBufferId, indices.Artifact, indices.Bytes.Length));
            buffers.Add(matrixBufferId, new CanonicalBufferBlobRefV1(matrixBufferId, matrix.Artifact, matrix.Bytes.Length));
            views.Add(positionViewId, new CanonicalBufferViewV1(positionViewId, positionBufferId, positions.Bytes.Length));
            views.Add(indexViewId, new CanonicalBufferViewV1(indexViewId, indexBufferId, indices.Bytes.Length));
            views.Add(matrixViewId, new CanonicalBufferViewV1(matrixViewId, matrixBufferId, matrix.Bytes.Length));
            accessors.Add(positionAccessorId, new CanonicalAccessorV1(positionAccessorId, positionViewId, CanonicalComponentType.F32, CanonicalElementShape.Vec3, 3));
            accessors.Add(indexAccessorId, new CanonicalAccessorV1(indexAccessorId, indexViewId, CanonicalComponentType.U16, CanonicalElementShape.Scalar, 3));
            accessors.Add(matrixAccessorId, new CanonicalAccessorV1(matrixAccessorId, matrixViewId, CanonicalComponentType.F32, CanonicalElementShape.Mat4, 1));
        }

        CanonicalPrimitiveV1 primitive = new CanonicalPrimitiveV1(primitiveId, indexAccessorId);
        CanonicalMeshV1 mesh = new CanonicalMeshV1(
            meshId,
            "synthetic-triangle",
            3,
            new Dictionary<string, CanonicalObjectId>(StringComparer.Ordinal) { ["POSITION"] = positionAccessorId },
            new[] { primitive },
            new CanonicalBoundsV1(0, 0, 0, 1, 1, 0));
        CanonicalMeshBindingV1 binding = new CanonicalMeshBindingV1(bindingId, meshId, null, matrixAccessorId);
        CanonicalArmorManifestV1 asset = new CanonicalArmorManifestV1(
            assetId,
            buffers,
            views,
            accessors,
            new Dictionary<CanonicalObjectId, CanonicalMeshV1> { [meshId] = mesh },
            new Dictionary<CanonicalObjectId, CanonicalMeshBindingV1> { [bindingId] = binding });
        return new FixturePackage(asset, CanonicalManifestWriter.Write(asset), new[] { positions, indices, matrix });
    }

    private static CanonicalObjectId Id(string kind, string key) =>
        CanonicalObjectIdDeriver.DeriveSource(kind, AdapterContract, key);

    private static string ResolveWorkspace(string[] args)
    {
        string parent;
        if (args.Length == 2 && args[0] == "--workspace")
        {
            parent = Path.GetFullPath(args[1]);
        }
        else if (args.Length == 0)
        {
            parent = Path.Combine(Path.GetTempPath(), $"tainted-armour-r2-{Guid.NewGuid():N}");
        }
        else
        {
            throw new ArgumentException("Usage: Tainted.Armour.Serialization.Fixtures [--workspace <parent-directory>]");
        }

        return Path.Combine(parent, WorkspaceDirectoryName);
    }

    private static void CheckCanonicalJson(string input, string expected)
    {
        string observed = Encoding.UTF8.GetString(Rfc8785CanonicalJson.Canonicalize(Encoding.UTF8.GetBytes(input)));
        Check(observed == expected, $"Canonical JSON number '{input}' became '{observed}' instead of '{expected}'.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void CheckThrows<TException>(Action action, string message)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException(message);
    }

    private sealed class FixturePackage
    {
        public FixturePackage(CanonicalArmorManifestV1 asset, CanonicalManifestArtifact manifest, IReadOnlyList<CanonicalBlob> blobs)
        {
            Asset = asset;
            Manifest = manifest;
            Blobs = blobs;
        }

        public CanonicalArmorManifestV1 Asset { get; }
        public CanonicalManifestArtifact Manifest { get; }
        public IReadOnlyList<CanonicalBlob> Blobs { get; }
    }
}
