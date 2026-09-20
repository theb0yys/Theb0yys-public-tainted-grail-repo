using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace AvalonExceptions;

internal static class HarmonyPatchChainProbe
{
    private const int MaxTargetCandidates = 20;
    private const int MaxFindings = 200;

    private static readonly Regex MethodTokenRegex = new Regex(
        @"(?<method>(?:[A-Za-z_][A-Za-z0-9_+`<>]*\.){2,}[A-Za-z_][A-Za-z0-9_+`<>]*)",
        RegexOptions.CultureInvariant);

    internal static HarmonyPatchFinding[] Build(
        string kind,
        string summary,
        string message,
        string managedStack,
        IEnumerable<Assembly>? loadedAssemblies = null)
    {
        string text = string.Join("\n", new[]
        {
            kind ?? string.Empty,
            summary ?? string.Empty,
            message ?? string.Empty,
            managedStack ?? string.Empty
        });

        if (!ContainsHarmonySignal(text))
        {
            return Array.Empty<HarmonyPatchFinding>();
        }

        Assembly[] assemblies = (loadedAssemblies ?? AppDomain.CurrentDomain.GetAssemblies())
            .Where(assembly => !assembly.IsDynamic)
            .ToArray();
        string[] originalCandidates = ExtractOriginalCandidates(text);
        string[] patchCandidates = ExtractPatchCandidates(text);
        Assembly? harmonyAssembly = FindHarmonyAssembly(assemblies);
        if (harmonyAssembly == null)
        {
            return new[]
            {
                new HarmonyPatchFinding(
                    "harmony-library-not-loaded",
                    FirstOrEmpty(originalCandidates),
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    FirstOrEmpty(patchCandidates),
                    "Harmony evidence was found in the incident, but no loaded Harmony assembly was available for patch-chain metadata.")
            };
        }

        MethodBase? originalMethod = ResolveOriginalMethod(assemblies, originalCandidates);
        if (originalMethod == null)
        {
            return new[]
            {
                new HarmonyPatchFinding(
                    "target-method-unresolved",
                    FirstOrEmpty(originalCandidates),
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    FirstOrEmpty(patchCandidates),
                    "Harmony evidence was found, but The Last Menhir could not resolve the original target method from the captured text.")
            };
        }

        object? patchInfo = TryGetPatchInfo(harmonyAssembly, originalMethod);
        string originalMethodName = FormatMethod(originalMethod);
        if (patchInfo == null)
        {
            return new[]
            {
                new HarmonyPatchFinding(
                    "patch-info-unavailable",
                    originalMethodName,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    FirstOrEmpty(patchCandidates),
                    "Harmony was loaded and the target method resolved, but Harmony returned no patch-chain metadata.")
            };
        }

        List<HarmonyPatchFinding> findings = new List<HarmonyPatchFinding>();
        AddPatchRows(findings, patchInfo, originalMethodName, "prefixes", "prefix");
        AddPatchRows(findings, patchInfo, originalMethodName, "postfixes", "postfix");
        AddPatchRows(findings, patchInfo, originalMethodName, "transpilers", "transpiler");
        AddPatchRows(findings, patchInfo, originalMethodName, "finalizers", "finalizer");

        if (findings.Count == 0)
        {
            findings.Add(new HarmonyPatchFinding(
                "patch-info-empty",
                originalMethodName,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                FirstOrEmpty(patchCandidates),
                "Harmony patch-chain metadata exists, but no prefix, postfix, transpiler, or finalizer rows were found."));
        }

        return findings.Take(MaxFindings).ToArray();
    }

    private static bool ContainsHarmonySignal(string text)
    {
        return text.IndexOf("Harmony", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("HarmonyLib", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("transpiler", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("Transpile", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("PatchProcessor", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("PatchClassProcessor", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string[] ExtractOriginalCandidates(string text)
    {
        return MethodTokenRegex.Matches(text)
            .Cast<Match>()
            .Select(match => match.Groups["method"].Value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Where(value => !LooksLikePatchMethod(value))
            .Where(value => !LooksLikeFrameworkMethod(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxTargetCandidates)
            .ToArray();
    }

    private static string[] ExtractPatchCandidates(string text)
    {
        return MethodTokenRegex.Matches(text)
            .Cast<Match>()
            .Select(match => match.Groups["method"].Value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Where(LooksLikePatchMethod)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxTargetCandidates)
            .ToArray();
    }

    private static bool LooksLikePatchMethod(string value)
    {
        string methodName = ExtractMethodName(value);
        return methodName.IndexOf("Transpile", StringComparison.OrdinalIgnoreCase) >= 0
            || methodName.IndexOf("Transpiler", StringComparison.OrdinalIgnoreCase) >= 0
            || methodName.IndexOf("Prefix", StringComparison.OrdinalIgnoreCase) >= 0
            || methodName.IndexOf("Postfix", StringComparison.OrdinalIgnoreCase) >= 0
            || methodName.IndexOf("Finalizer", StringComparison.OrdinalIgnoreCase) >= 0
            || methodName.IndexOf("Patch", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool LooksLikeFrameworkMethod(string value)
    {
        return value.StartsWith("HarmonyLib.", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase);
    }

    private static Assembly? FindHarmonyAssembly(IEnumerable<Assembly> assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            if (assembly.GetType("HarmonyLib.Harmony", throwOnError: false, ignoreCase: false) != null)
            {
                return assembly;
            }
        }

        return null;
    }

    private static MethodBase? ResolveOriginalMethod(Assembly[] assemblies, IEnumerable<string> candidates)
    {
        foreach (string candidate in candidates)
        {
            string typeName = ExtractDeclaringTypeName(candidate);
            string methodName = ExtractMethodName(candidate);
            if (string.IsNullOrWhiteSpace(typeName) || string.IsNullOrWhiteSpace(methodName))
            {
                continue;
            }

            foreach (Assembly assembly in assemblies)
            {
                Type? type = TryGetType(assembly, typeName);
                if (type == null)
                {
                    continue;
                }

                MethodInfo? method = TryFindMethod(type, methodName);
                if (method != null)
                {
                    return method;
                }
            }
        }

        return null;
    }

    private static object? TryGetPatchInfo(Assembly harmonyAssembly, MethodBase originalMethod)
    {
        try
        {
            Type? harmonyType = harmonyAssembly.GetType("HarmonyLib.Harmony", throwOnError: false, ignoreCase: false);
            MethodInfo? getPatchInfo = harmonyType?.GetMethod("GetPatchInfo", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(MethodBase) }, null);
            return getPatchInfo?.Invoke(null, new object[] { originalMethod });
        }
        catch
        {
            return null;
        }
    }

    private static void AddPatchRows(List<HarmonyPatchFinding> findings, object patchInfo, string originalMethod, string listProperty, string patchKind)
    {
        object? patchList = ReadMember(patchInfo, listProperty);
        if (patchList is not IEnumerable patches)
        {
            return;
        }

        foreach (object patch in patches)
        {
            if (findings.Count >= MaxFindings)
            {
                return;
            }

            MethodBase? patchMethod = ReadMember(patch, "PatchMethod") as MethodBase;
            findings.Add(new HarmonyPatchFinding(
                "patch-row",
                originalMethod,
                patchKind,
                ReadMember(patch, "owner")?.ToString() ?? string.Empty,
                ReadMember(patch, "priority")?.ToString() ?? string.Empty,
                JoinStrings(ReadMember(patch, "before")),
                JoinStrings(ReadMember(patch, "after")),
                FormatMethod(patchMethod),
                "Read-only Harmony patch-chain metadata."));
        }
    }

    private static object? ReadMember(object instance, string name)
    {
        try
        {
            Type type = instance.GetType();
            PropertyInfo? property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null)
            {
                return property.GetValue(instance, null);
            }

            FieldInfo? field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return field?.GetValue(instance);
        }
        catch
        {
            return null;
        }
    }

    private static Type? TryGetType(Assembly assembly, string typeName)
    {
        try
        {
            return assembly.GetType(typeName, throwOnError: false, ignoreCase: false);
        }
        catch
        {
            return null;
        }
    }

    private static MethodInfo? TryFindMethod(Type type, string methodName)
    {
        try
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .FirstOrDefault(method => string.Equals(method.Name, methodName, StringComparison.Ordinal));
        }
        catch
        {
            return null;
        }
    }

    private static string ExtractDeclaringTypeName(string fullMethod)
    {
        int dot = fullMethod.LastIndexOf('.');
        return dot > 0 ? fullMethod.Substring(0, dot) : string.Empty;
    }

    private static string ExtractMethodName(string fullMethod)
    {
        int dot = fullMethod.LastIndexOf('.');
        return dot >= 0 && dot < fullMethod.Length - 1 ? fullMethod.Substring(dot + 1) : fullMethod;
    }

    private static string FormatMethod(MethodBase? method)
    {
        if (method == null)
        {
            return string.Empty;
        }

        string declaringType = method.DeclaringType?.FullName ?? string.Empty;
        return string.IsNullOrWhiteSpace(declaringType)
            ? method.Name
            : declaringType + "." + method.Name;
    }

    private static string JoinStrings(object? value)
    {
        if (value == null || value is string)
        {
            return value?.ToString() ?? string.Empty;
        }

        if (value is IEnumerable enumerable)
        {
            List<string> values = new List<string>();
            foreach (object? item in enumerable)
            {
                if (item != null)
                {
                    values.Add(item.ToString() ?? string.Empty);
                }
            }

            return string.Join(";", values.Where(item => !string.IsNullOrWhiteSpace(item)));
        }

        return value.ToString() ?? string.Empty;
    }

    private static string FirstOrEmpty(IEnumerable<string> values)
    {
        return values.FirstOrDefault(item => !string.IsNullOrWhiteSpace(item)) ?? string.Empty;
    }
}

internal readonly struct HarmonyPatchFinding
{
    internal HarmonyPatchFinding(
        string status,
        string originalMethod,
        string patchKind,
        string owner,
        string priority,
        string before,
        string after,
        string patchMethod,
        string evidence)
    {
        Status = status;
        OriginalMethod = originalMethod;
        PatchKind = patchKind;
        Owner = owner;
        Priority = priority;
        Before = before;
        After = after;
        PatchMethod = patchMethod;
        Evidence = evidence;
    }

    internal string Status { get; }
    internal string OriginalMethod { get; }
    internal string PatchKind { get; }
    internal string Owner { get; }
    internal string Priority { get; }
    internal string Before { get; }
    internal string After { get; }
    internal string PatchMethod { get; }
    internal string Evidence { get; }

    internal static string ToCsv(IEnumerable<HarmonyPatchFinding> findings)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("status,originalMethod,patchKind,owner,priority,before,after,patchMethod,evidence");
        foreach (HarmonyPatchFinding finding in findings)
        {
            AppendCsvRow(
                builder,
                finding.Status,
                finding.OriginalMethod,
                finding.PatchKind,
                finding.Owner,
                finding.Priority,
                finding.Before,
                finding.After,
                finding.PatchMethod,
                finding.Evidence);
        }

        return builder.ToString();
    }

    private static void AppendCsvRow(StringBuilder builder, params string[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(',');
            }

            builder.Append(Csv(values[i]));
        }

        builder.AppendLine();
    }

    private static string Csv(string value)
    {
        value ??= string.Empty;
        bool quote = value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
        if (!quote)
        {
            return value;
        }

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
