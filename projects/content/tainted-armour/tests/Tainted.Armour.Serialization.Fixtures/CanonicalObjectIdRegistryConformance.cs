using System;
using System.Runtime.CompilerServices;
using Tainted.Armour.Canonical;
using Tainted.Armour.Serialization;

namespace Tainted.Armour.Serialization.Fixtures;

internal static class CanonicalObjectIdRegistryConformance
{
    [ModuleInitializer]
    internal static void Run()
    {
        VerifyDuplicateSourceKeyRejection();
        VerifyDuplicateGeneratedKeyRejection();
        VerifyInjectedDigestCollisionRejection();
        Console.WriteLine("PASS canonical object ID duplicate and collision rejection");
    }

    private static void VerifyDuplicateSourceKeyRejection()
    {
        var registry = new CanonicalObjectIdRegistry();
        CanonicalObjectId first = registry.RegisterSource(
            "mesh",
            "com.tainted.armour.synthetic/1",
            "asset/root/mesh/0000");
        Check(first.Kind == "mesh", "Canonical source registration returned the wrong kind.");
        CheckThrows<InvalidOperationException>(
            () => registry.RegisterSource(
                "mesh",
                "com.tainted.armour.synthetic/1",
                "asset/root/mesh/0000"),
            "Duplicate source object key was accepted.");
    }

    private static void VerifyDuplicateGeneratedKeyRejection()
    {
        CanonicalObjectId owner = CanonicalObjectIdDeriver.DeriveSource(
            "mesh",
            "com.tainted.armour.synthetic/1",
            "asset/root/mesh/0000");
        var registry = new CanonicalObjectIdRegistry();
        _ = registry.RegisterGenerated(
            "primitive",
            "com.tainted.armour.synthetic-generator/1",
            owner,
            "primitive",
            0);
        CheckThrows<InvalidOperationException>(
            () => registry.RegisterGenerated(
                "primitive",
                "com.tainted.armour.synthetic-generator/1",
                owner,
                "primitive",
                0),
            "Duplicate generated object key was accepted.");
    }

    private static void VerifyInjectedDigestCollisionRejection()
    {
        var registry = new CanonicalObjectIdRegistry(new ConstantSha256DigestProvider());
        CanonicalObjectId first = registry.RegisterSource(
            "mesh",
            "com.tainted.armour.synthetic/1",
            "asset/root/mesh/0000");
        Check(first.Value.EndsWith(new string('0', 64), StringComparison.Ordinal), "Injected digest provider was not used.");
        CheckThrows<InvalidOperationException>(
            () => registry.RegisterSource(
                "mesh",
                "com.tainted.armour.synthetic/1",
                "asset/root/mesh/0001"),
            "Distinct source preimages sharing one digest were not rejected as a collision.");
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

    private sealed class ConstantSha256DigestProvider : ICanonicalDigestProvider
    {
        public ArtifactId Compute(ReadOnlySpan<byte> bytes) =>
            new ArtifactId("sha256", new string('0', 64));
    }
}
