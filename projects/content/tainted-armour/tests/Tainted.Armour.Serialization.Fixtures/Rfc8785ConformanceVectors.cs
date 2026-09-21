using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Tainted.Armour.Serialization;

namespace Tainted.Armour.Serialization.Fixtures;

internal static class Rfc8785ConformanceVectors
{
    private readonly struct NumberVector
    {
        public NumberVector(string ieee754Hex, string expectedJson)
        {
            Ieee754Hex = ieee754Hex;
            ExpectedJson = expectedJson;
        }

        public string Ieee754Hex { get; }
        public string ExpectedJson { get; }
    }

    [ModuleInitializer]
    internal static void Run()
    {
        VerifyAppendixBNumberVectors();
        VerifyUtf16PropertyOrdering();
        Console.WriteLine("PASS RFC-8785 Appendix B and UTF-16 ordering vectors");
    }

    private static void VerifyAppendixBNumberVectors()
    {
        NumberVector[] vectors =
        {
            new NumberVector("0000000000000000", "0"),
            new NumberVector("8000000000000000", "0"),
            new NumberVector("0000000000000001", "5e-324"),
            new NumberVector("8000000000000001", "-5e-324"),
            new NumberVector("7fefffffffffffff", "1.7976931348623157e+308"),
            new NumberVector("ffefffffffffffff", "-1.7976931348623157e+308"),
            new NumberVector("4340000000000000", "9007199254740992"),
            new NumberVector("c340000000000000", "-9007199254740992"),
            new NumberVector("4430000000000000", "295147905179352830000"),
            new NumberVector("44b52d02c7e14af5", "9.999999999999997e+22"),
            new NumberVector("44b52d02c7e14af6", "1e+23"),
            new NumberVector("44b52d02c7e14af7", "1.0000000000000001e+23"),
            new NumberVector("444b1ae4d6e2ef4e", "999999999999999700000"),
            new NumberVector("444b1ae4d6e2ef4f", "999999999999999900000"),
            new NumberVector("444b1ae4d6e2ef50", "1e+21"),
            new NumberVector("3eb0c6f7a0b5ed8c", "9.999999999999997e-7"),
            new NumberVector("3eb0c6f7a0b5ed8d", "0.000001"),
            new NumberVector("41b3de4355555553", "333333333.3333332"),
            new NumberVector("41b3de4355555554", "333333333.33333325"),
            new NumberVector("41b3de4355555555", "333333333.3333333"),
            new NumberVector("41b3de4355555556", "333333333.3333334"),
            new NumberVector("41b3de4355555557", "333333333.33333343"),
            new NumberVector("becbf647612f3696", "-0.0000033333333333333333"),
            new NumberVector("43143ff3c1cb0959", "1424953923781206.2"),
        };

        foreach (NumberVector vector in vectors)
        {
            ulong bits = ulong.Parse(vector.Ieee754Hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            double value = BitConverter.Int64BitsToDouble(unchecked((long)bits));
            string sourceJson = bits == 0x8000000000000000UL
                ? "-0"
                : value.ToString("R", CultureInfo.InvariantCulture);
            string observed = Canonicalize(sourceJson);
            if (!StringComparer.Ordinal.Equals(observed, vector.ExpectedJson))
            {
                throw new InvalidOperationException(
                    $"RFC-8785 number vector {vector.Ieee754Hex} canonicalized from '{sourceJson}' to '{observed}', expected '{vector.ExpectedJson}'.");
            }
        }
    }

    private static void VerifyUtf16PropertyOrdering()
    {
        const string input = "{\"€\":\"Euro Sign\",\"\\r\":\"Carriage Return\",\"דּ\":\"Hebrew Letter Dalet With Dagesh\",\"1\":\"One\",\"😀\":\"Emoji: Grinning Face\",\"\":\"Control\",\"ö\":\"Latin Small Letter O With Diaeresis\"}";
        const string expected = "{\"\\r\":\"Carriage Return\",\"1\":\"One\",\"\":\"Control\",\"ö\":\"Latin Small Letter O With Diaeresis\",\"€\":\"Euro Sign\",\"😀\":\"Emoji: Grinning Face\",\"דּ\":\"Hebrew Letter Dalet With Dagesh\"}";
        string observed = Canonicalize(input);
        if (!StringComparer.Ordinal.Equals(observed, expected))
        {
            throw new InvalidOperationException(
                $"RFC-8785 UTF-16 property ordering mismatch. Observed '{observed}', expected '{expected}'.");
        }
    }

    private static string Canonicalize(string input) =>
        Encoding.UTF8.GetString(Rfc8785CanonicalJson.Canonicalize(Encoding.UTF8.GetBytes(input)));
}
