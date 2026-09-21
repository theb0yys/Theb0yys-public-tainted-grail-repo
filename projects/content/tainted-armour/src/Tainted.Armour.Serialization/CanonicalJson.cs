using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Tainted.Armour.Serialization;

public static class Rfc8785CanonicalJson
{
    private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

    public static byte[] Canonicalize(ReadOnlySpan<byte> utf8Json)
    {
        JsonDocumentOptions options = new JsonDocumentOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 128,
        };

        using JsonDocument document = JsonDocument.Parse(utf8Json.ToArray(), options);
        using MemoryStream output = new MemoryStream();
        WriteElement(output, document.RootElement);
        return output.ToArray();
    }

    private static void WriteElement(Stream output, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                WriteObject(output, element);
                break;
            case JsonValueKind.Array:
                WriteArray(output, element);
                break;
            case JsonValueKind.String:
                WriteString(output, element.GetString() ?? string.Empty);
                break;
            case JsonValueKind.Number:
                WriteNumber(output, element);
                break;
            case JsonValueKind.True:
                WriteAscii(output, "true");
                break;
            case JsonValueKind.False:
                WriteAscii(output, "false");
                break;
            case JsonValueKind.Null:
                WriteAscii(output, "null");
                break;
            default:
                throw new InvalidDataException($"Unsupported JSON value kind {element.ValueKind}.");
        }
    }

    private static void WriteObject(Stream output, JsonElement element)
    {
        JsonProperty[] properties = element.EnumerateObject().ToArray();
        HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (JsonProperty property in properties)
        {
            if (!seen.Add(property.Name))
            {
                throw new InvalidDataException($"Duplicate JSON property '{property.Name}' is not canonicalizable.");
            }
        }

        Array.Sort(properties, (left, right) => StringComparer.Ordinal.Compare(left.Name, right.Name));
        output.WriteByte((byte)'{');
        for (int index = 0; index < properties.Length; index++)
        {
            if (index != 0) output.WriteByte((byte)',');
            WriteString(output, properties[index].Name);
            output.WriteByte((byte)':');
            WriteElement(output, properties[index].Value);
        }

        output.WriteByte((byte)'}');
    }

    private static void WriteArray(Stream output, JsonElement element)
    {
        output.WriteByte((byte)'[');
        int index = 0;
        foreach (JsonElement item in element.EnumerateArray())
        {
            if (index != 0) output.WriteByte((byte)',');
            WriteElement(output, item);
            index++;
        }

        output.WriteByte((byte)']');
    }

    private static void WriteNumber(Stream output, JsonElement element)
    {
        if (!element.TryGetDouble(out double value) || double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new InvalidDataException("RFC-8785 numbers must be finite IEEE-754 binary64 values.");
        }

        WriteAscii(output, EcmaScriptNumberFormatter.Format(value));
    }

    private static void WriteString(Stream output, string value)
    {
        StringBuilder builder = new StringBuilder(value.Length + 2);
        builder.Append('"');
        for (int index = 0; index < value.Length; index++)
        {
            char character = value[index];
            switch (character)
            {
                case '"': builder.Append("\\\""); break;
                case '\\': builder.Append("\\\\"); break;
                case '\b': builder.Append("\\b"); break;
                case '\t': builder.Append("\\t"); break;
                case '\n': builder.Append("\\n"); break;
                case '\f': builder.Append("\\f"); break;
                case '\r': builder.Append("\\r"); break;
                default:
                    if (character < 0x20)
                    {
                        builder.Append("\\u00");
                        builder.Append(((int)character).ToString("x2", CultureInfo.InvariantCulture));
                    }
                    else if (char.IsHighSurrogate(character))
                    {
                        if (index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1]))
                        {
                            throw new InvalidDataException("Unpaired high surrogate is not valid I-JSON.");
                        }

                        builder.Append(character);
                        builder.Append(value[++index]);
                    }
                    else if (char.IsLowSurrogate(character))
                    {
                        throw new InvalidDataException("Unpaired low surrogate is not valid I-JSON.");
                    }
                    else
                    {
                        builder.Append(character);
                    }

                    break;
            }
        }

        builder.Append('"');
        byte[] bytes = StrictUtf8.GetBytes(builder.ToString());
        output.Write(bytes, 0, bytes.Length);
    }

    private static void WriteAscii(Stream output, string value)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(value);
        output.Write(bytes, 0, bytes.Length);
    }
}

internal static class EcmaScriptNumberFormatter
{
    public static string Format(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        if (value == 0d) return "0";

        string raw = value.ToString("R", CultureInfo.InvariantCulture);
        bool negative = raw[0] == '-';
        if (negative) raw = raw.Substring(1);

        int exponentIndex = raw.IndexOf('E');
        if (exponentIndex < 0) exponentIndex = raw.IndexOf('e');

        string mantissa;
        int explicitExponent;
        if (exponentIndex >= 0)
        {
            mantissa = raw.Substring(0, exponentIndex);
            explicitExponent = int.Parse(raw.Substring(exponentIndex + 1), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
        }
        else
        {
            mantissa = raw;
            explicitExponent = 0;
        }

        int decimalIndex = mantissa.IndexOf('.');
        int decimalPosition = (decimalIndex >= 0 ? decimalIndex : mantissa.Length) + explicitExponent;
        string digits = decimalIndex >= 0 ? mantissa.Remove(decimalIndex, 1) : mantissa;

        int leadingZeroCount = 0;
        while (leadingZeroCount < digits.Length && digits[leadingZeroCount] == '0') leadingZeroCount++;
        if (leadingZeroCount == digits.Length) return "0";
        decimalPosition -= leadingZeroCount;
        digits = digits.Substring(leadingZeroCount);

        int retainedLength = digits.Length;
        while (retainedLength > 1 && digits[retainedLength - 1] == '0') retainedLength--;
        if (retainedLength != digits.Length) digits = digits.Substring(0, retainedLength);

        int digitCount = digits.Length;
        int n = decimalPosition;
        string unsigned;
        if (digitCount <= n && n <= 21)
        {
            unsigned = digits + new string('0', n - digitCount);
        }
        else if (0 < n && n <= 21)
        {
            unsigned = digits.Substring(0, n) + "." + digits.Substring(n);
        }
        else if (-6 < n && n <= 0)
        {
            unsigned = "0." + new string('0', -n) + digits;
        }
        else
        {
            int exponent = n - 1;
            unsigned = digitCount == 1
                ? digits
                : digits.Substring(0, 1) + "." + digits.Substring(1);
            unsigned += "e" + (exponent >= 0 ? "+" : string.Empty) + exponent.ToString(CultureInfo.InvariantCulture);
        }

        return negative ? "-" + unsigned : unsigned;
    }
}

public sealed class CanonicalJsonVerificationResult
{
    public CanonicalJsonVerificationResult(bool accepted, string diagnostic)
    {
        Accepted = accepted;
        Diagnostic = diagnostic ?? string.Empty;
    }

    public bool Accepted { get; }
    public string Diagnostic { get; }
}

public static class CanonicalJsonVerifier
{
    public static CanonicalJsonVerificationResult Verify(ReadOnlySpan<byte> utf8Json)
    {
        try
        {
            byte[] canonical = Rfc8785CanonicalJson.Canonicalize(utf8Json);
            bool accepted = utf8Json.SequenceEqual(canonical);
            return accepted
                ? new CanonicalJsonVerificationResult(true, string.Empty)
                : new CanonicalJsonVerificationResult(false, "JSON bytes are valid but not in canonical RFC-8785 form.");
        }
        catch (Exception exception) when (
            exception is JsonException
            || exception is InvalidDataException
            || exception is DecoderFallbackException
            || exception is FormatException
            || exception is OverflowException)
        {
            return new CanonicalJsonVerificationResult(false, exception.Message);
        }
    }
}
