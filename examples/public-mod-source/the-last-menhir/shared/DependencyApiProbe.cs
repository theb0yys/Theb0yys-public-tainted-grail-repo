using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace AvalonExceptions;

internal static class DependencyApiProbe
{
    private const int MaxFindings = 100;

    private static readonly Regex ExpectedClassRegex = new Regex(
        @"expected\s+class\s+'(?<symbol>[^']+)'\s+in\s+assembly\s+'(?<assembly>[^']+)'",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex CouldNotLoadTypeFromAssemblyRegex = new Regex(
        @"Could\s+not\s+load\s+type\s+'(?<symbol>[^']+)'\s+from\s+assembly\s+'(?<assembly>[^']+)'",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex CouldNotLoadAssemblyRegex = new Regex(
        @"Could\s+not\s+load\s+file\s+or\s+assembly\s+'(?<assembly>[^']+)'",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex MissingMethodRegex = new Regex(
        @"Method\s+not\s+found:\s+'(?<symbol>[^']+)'",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex MissingFieldRegex = new Regex(
        @"Field\s+not\s+found:\s+'(?<symbol>[^']+)'",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex RequestingFieldRegex = new Regex(
        @"Could\s+not\s+load\s+type\s+of\s+field\s+'(?<requesting>[^']+)'",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    internal static DependencyApiFinding[] Build(
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

        if (!ContainsProbeSignal(text))
        {
            return Array.Empty<DependencyApiFinding>();
        }

        Assembly[] assemblies = (loadedAssemblies ?? AppDomain.CurrentDomain.GetAssemblies())
            .Where(assembly => !assembly.IsDynamic)
            .ToArray();
        List<DependencyApiRequest> requests = ExtractRequests(text);
        if (requests.Count == 0)
        {
            return new[]
            {
                new DependencyApiFinding(
                    "exception-family",
                    string.Empty,
                    string.Empty,
                    ExtractRequestingMember(text),
                    "probe-inconclusive-no-symbol",
                    string.Empty,
                    "A dependency/API exception family was detected, but no exact assembly/type/member token was extracted.")
            };
        }

        List<DependencyApiFinding> findings = new List<DependencyApiFinding>();
        HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (DependencyApiRequest request in requests)
        {
            DependencyApiFinding finding = ProbeRequest(request, assemblies);
            string key = string.Join("|", finding.Kind, finding.RequestedAssembly, finding.RequestedSymbol, finding.RequestingMember, finding.Status);
            if (seen.Add(key))
            {
                findings.Add(finding);
            }

            if (findings.Count >= MaxFindings)
            {
                break;
            }
        }

        return findings.ToArray();
    }

    private static bool ContainsProbeSignal(string text)
    {
        return text.IndexOf("ReflectionTypeLoadException", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("TypeLoadException", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("MissingMethodException", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("MissingFieldException", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("expected class", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("Method not found", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("Field not found", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static List<DependencyApiRequest> ExtractRequests(string text)
    {
        List<DependencyApiRequest> requests = new List<DependencyApiRequest>();
        string requestingMember = ExtractRequestingMember(text);

        foreach (Match match in ExpectedClassRegex.Matches(text))
        {
            requests.Add(new DependencyApiRequest(
                "type",
                match.Groups["assembly"].Value,
                match.Groups["symbol"].Value,
                requestingMember,
                "expected class token"));
        }

        foreach (Match match in CouldNotLoadTypeFromAssemblyRegex.Matches(text))
        {
            requests.Add(new DependencyApiRequest(
                "type",
                match.Groups["assembly"].Value,
                match.Groups["symbol"].Value,
                requestingMember,
                "could not load type from assembly"));
        }

        foreach (Match match in CouldNotLoadAssemblyRegex.Matches(text))
        {
            requests.Add(new DependencyApiRequest(
                "assembly",
                match.Groups["assembly"].Value,
                string.Empty,
                requestingMember,
                "could not load assembly"));
        }

        foreach (Match match in MissingMethodRegex.Matches(text))
        {
            requests.Add(new DependencyApiRequest(
                "method",
                string.Empty,
                match.Groups["symbol"].Value,
                requestingMember,
                "missing method exception"));
        }

        foreach (Match match in MissingFieldRegex.Matches(text))
        {
            requests.Add(new DependencyApiRequest(
                "field",
                string.Empty,
                match.Groups["symbol"].Value,
                requestingMember,
                "missing field exception"));
        }

        return requests;
    }

    private static DependencyApiFinding ProbeRequest(DependencyApiRequest request, Assembly[] assemblies)
    {
        string requestedAssembly = NormalizeAssemblyName(request.RequestedAssembly);
        string requestedSymbol = request.RequestedSymbol.Trim();
        Assembly? assembly = string.IsNullOrWhiteSpace(requestedAssembly)
            ? FindAssemblyByRequestedSymbol(assemblies, request.Kind, requestedSymbol)
            : FindLoadedAssembly(assemblies, requestedAssembly);
        string loadedAssembly = DescribeAssembly(assembly);

        if (!string.IsNullOrWhiteSpace(requestedAssembly) && assembly == null)
        {
            return request.ToFinding(
                requestedAssembly,
                "dependency-assembly-not-loaded",
                string.Empty,
                $"No loaded assembly matched requested assembly `{requestedAssembly}`.");
        }

        if (assembly == null)
        {
            return request.ToFinding(
                requestedAssembly,
                "probe-inconclusive-no-assembly",
                string.Empty,
                "No requested assembly was extracted and no loaded assembly could be inferred from the symbol.");
        }

        if (string.Equals(request.Kind, "assembly", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(requestedSymbol))
        {
            return request.ToFinding(
                requestedAssembly,
                "dependency-assembly-loaded",
                loadedAssembly,
                "The referenced assembly is currently loaded; the original failure may involve a different version, load context, or nested dependency.");
        }

        if (string.Equals(request.Kind, "type", StringComparison.OrdinalIgnoreCase))
        {
            Type? type = TryGetType(assembly, requestedSymbol);
            return type == null
                ? request.ToFinding(
                    requestedAssembly,
                    "dependency-present-api-missing",
                    loadedAssembly,
                    $"Loaded assembly `{loadedAssembly}` does not expose type `{requestedSymbol}`.")
                : request.ToFinding(
                    requestedAssembly,
                    "dependency-api-present",
                    loadedAssembly,
                    $"Loaded assembly `{loadedAssembly}` exposes type `{requestedSymbol}`; the failure may be a load cascade or context mismatch.");
        }

        return ProbeMemberRequest(request, requestedAssembly, requestedSymbol, assembly, loadedAssembly);
    }

    private static DependencyApiFinding ProbeMemberRequest(
        DependencyApiRequest request,
        string requestedAssembly,
        string requestedSymbol,
        Assembly assembly,
        string loadedAssembly)
    {
        string declaringTypeName = ExtractDeclaringTypeName(request.Kind, requestedSymbol);
        string memberName = ExtractMemberName(request.Kind, requestedSymbol);
        if (string.IsNullOrWhiteSpace(declaringTypeName) || string.IsNullOrWhiteSpace(memberName))
        {
            return request.ToFinding(
                requestedAssembly,
                "probe-inconclusive-member-signature",
                loadedAssembly,
                "A missing member exception was detected, but the declaring type/member name could not be parsed safely.");
        }

        Type? type = TryGetType(assembly, declaringTypeName);
        if (type == null)
        {
            return request.ToFinding(
                requestedAssembly,
                "dependency-present-declaring-type-missing",
                loadedAssembly,
                $"Loaded assembly `{loadedAssembly}` does not expose declaring type `{declaringTypeName}` for member `{memberName}`.");
        }

        bool memberPresent = string.Equals(request.Kind, "method", StringComparison.OrdinalIgnoreCase)
            ? TryHasMethod(type, memberName)
            : TryHasField(type, memberName);

        return memberPresent
            ? request.ToFinding(
                requestedAssembly,
                "dependency-member-name-present",
                loadedAssembly,
                $"Loaded assembly `{loadedAssembly}` exposes `{declaringTypeName}.{memberName}` by name; exact signature still needs source-level verification.")
            : request.ToFinding(
                requestedAssembly,
                "dependency-present-api-missing",
                loadedAssembly,
                $"Loaded assembly `{loadedAssembly}` does not expose `{declaringTypeName}.{memberName}` by name.");
    }

    private static string ExtractRequestingMember(string text)
    {
        Match match = RequestingFieldRegex.Match(text);
        return match.Success ? match.Groups["requesting"].Value.Trim() : string.Empty;
    }

    private static Assembly? FindLoadedAssembly(IEnumerable<Assembly> assemblies, string requestedAssembly)
    {
        string requestedSimpleName = AssemblySimpleName(requestedAssembly);
        return assemblies.FirstOrDefault(assembly =>
            string.Equals(SafeAssemblyName(assembly), requestedSimpleName, StringComparison.OrdinalIgnoreCase));
    }

    private static Assembly? FindAssemblyByRequestedSymbol(IEnumerable<Assembly> assemblies, string kind, string requestedSymbol)
    {
        string declaringTypeName = string.Equals(kind, "type", StringComparison.OrdinalIgnoreCase)
            ? requestedSymbol
            : ExtractDeclaringTypeName(kind, requestedSymbol);
        if (string.IsNullOrWhiteSpace(declaringTypeName))
        {
            return null;
        }

        foreach (Assembly assembly in assemblies)
        {
            if (TryGetType(assembly, declaringTypeName) != null)
            {
                return assembly;
            }
        }

        return null;
    }

    private static Type? TryGetType(Assembly assembly, string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return null;
        }

        try
        {
            return assembly.GetType(typeName.Trim(), throwOnError: false, ignoreCase: false);
        }
        catch
        {
            return null;
        }
    }

    private static bool TryHasMethod(Type type, string methodName)
    {
        try
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Any(method => string.Equals(method.Name, methodName, StringComparison.Ordinal));
        }
        catch
        {
            return false;
        }
    }

    private static bool TryHasField(Type type, string fieldName)
    {
        try
        {
            return type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static) != null;
        }
        catch
        {
            return false;
        }
    }

    private static string ExtractDeclaringTypeName(string kind, string symbol)
    {
        string value = symbol.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (string.Equals(kind, "type", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        string beforeParameters = value;
        int parameterStart = beforeParameters.IndexOf('(');
        if (parameterStart >= 0)
        {
            beforeParameters = beforeParameters.Substring(0, parameterStart);
        }

        int lastDot = beforeParameters.LastIndexOf('.');
        if (lastDot <= 0)
        {
            return string.Empty;
        }

        string declaringWithReturnType = beforeParameters.Substring(0, lastDot).Trim();
        int lastSpace = declaringWithReturnType.LastIndexOf(' ');
        return lastSpace >= 0
            ? declaringWithReturnType.Substring(lastSpace + 1).Trim()
            : declaringWithReturnType;
    }

    private static string ExtractMemberName(string kind, string symbol)
    {
        if (string.Equals(kind, "type", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        string beforeParameters = symbol.Trim();
        int parameterStart = beforeParameters.IndexOf('(');
        if (parameterStart >= 0)
        {
            beforeParameters = beforeParameters.Substring(0, parameterStart);
        }

        int lastDot = beforeParameters.LastIndexOf('.');
        return lastDot >= 0 && lastDot < beforeParameters.Length - 1
            ? beforeParameters.Substring(lastDot + 1).Trim()
            : string.Empty;
    }

    private static string NormalizeAssemblyName(string assembly)
    {
        return AssemblySimpleName((assembly ?? string.Empty).Trim());
    }

    private static string AssemblySimpleName(string assembly)
    {
        int comma = assembly.IndexOf(',');
        return comma >= 0 ? assembly.Substring(0, comma).Trim() : assembly.Trim();
    }

    private static string SafeAssemblyName(Assembly assembly)
    {
        try
        {
            return assembly.GetName().Name ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string DescribeAssembly(Assembly? assembly)
    {
        if (assembly == null)
        {
            return string.Empty;
        }

        try
        {
            AssemblyName name = assembly.GetName();
            return $"{name.Name}, Version={name.Version}";
        }
        catch
        {
            return assembly.FullName ?? string.Empty;
        }
    }

    private readonly struct DependencyApiRequest
    {
        internal DependencyApiRequest(string kind, string requestedAssembly, string requestedSymbol, string requestingMember, string evidence)
        {
            Kind = kind;
            RequestedAssembly = requestedAssembly;
            RequestedSymbol = requestedSymbol;
            RequestingMember = requestingMember;
            Evidence = evidence;
        }

        internal string Kind { get; }
        internal string RequestedAssembly { get; }
        internal string RequestedSymbol { get; }
        internal string RequestingMember { get; }
        internal string Evidence { get; }

        internal DependencyApiFinding ToFinding(string requestedAssembly, string status, string loadedAssembly, string evidence)
        {
            return new DependencyApiFinding(
                Kind,
                requestedAssembly,
                RequestedSymbol,
                RequestingMember,
                status,
                loadedAssembly,
                string.IsNullOrWhiteSpace(evidence) ? Evidence : $"{Evidence}: {evidence}");
        }
    }
}

internal readonly struct DependencyApiFinding
{
    internal DependencyApiFinding(
        string kind,
        string requestedAssembly,
        string requestedSymbol,
        string requestingMember,
        string status,
        string loadedAssembly,
        string evidence)
    {
        Kind = kind;
        RequestedAssembly = requestedAssembly;
        RequestedSymbol = requestedSymbol;
        RequestingMember = requestingMember;
        Status = status;
        LoadedAssembly = loadedAssembly;
        Evidence = evidence;
    }

    internal string Kind { get; }
    internal string RequestedAssembly { get; }
    internal string RequestedSymbol { get; }
    internal string RequestingMember { get; }
    internal string Status { get; }
    internal string LoadedAssembly { get; }
    internal string Evidence { get; }

    internal static string ToCsv(IEnumerable<DependencyApiFinding> findings)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("kind,requestedAssembly,requestedSymbol,requestingMember,status,loadedAssembly,evidence");
        foreach (DependencyApiFinding finding in findings)
        {
            AppendCsvRow(
                builder,
                finding.Kind,
                finding.RequestedAssembly,
                finding.RequestedSymbol,
                finding.RequestingMember,
                finding.Status,
                finding.LoadedAssembly,
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
