using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using BepInEx.Logging;
using HarmonyLib;

namespace TGCommunity.RuntimeTracer;

internal sealed class TargetSpec
{
    internal TargetSpec(string assemblyName, string typeName, string methodName, string[] parameterTypeNames)
    {
        AssemblyName = (assemblyName ?? string.Empty).Trim();
        TypeName = (typeName ?? string.Empty).Trim();
        MethodName = (methodName ?? string.Empty).Trim();
        ParameterTypeNames = parameterTypeNames ?? Array.Empty<string>();
    }

    internal string AssemblyName { get; }
    internal string TypeName { get; }
    internal string MethodName { get; }
    internal string[] ParameterTypeNames { get; }

    internal bool IsConfigured =>
        !string.IsNullOrWhiteSpace(AssemblyName) &&
        !string.IsNullOrWhiteSpace(TypeName) &&
        !string.IsNullOrWhiteSpace(MethodName);

    internal bool HasExplicitZeroParameterSignature =>
        ParameterTypeNames.Length == 1 &&
        string.Equals(ParameterTypeNames[0], "<none>", StringComparison.OrdinalIgnoreCase);

    internal static string[] ParseParameterTypes(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<string>();
        }

        return raw
            .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(value => value.Trim())
            .Where(value => value.Length > 0)
            .ToArray();
    }

    internal static TargetSpec ForSelfTest()
    {
        return new TargetSpec(
            typeof(Plugin).Assembly.GetName().Name ?? "TGCommunity.RuntimeTracer",
            typeof(SelfTestTarget).FullName ?? "TGCommunity.RuntimeTracer.SelfTestTarget",
            nameof(SelfTestTarget.Ping),
            new[] { typeof(int).FullName ?? "System.Int32" });
    }
}

internal sealed class TargetResolution
{
    private TargetResolution(bool success, MethodBase? method, string message)
    {
        Success = success;
        Method = method;
        Message = message;
    }

    internal bool Success { get; }
    internal MethodBase? Method { get; }
    internal string Message { get; }

    internal static TargetResolution Passed(MethodBase method)
    {
        return new TargetResolution(true, method, "resolved");
    }

    internal static TargetResolution Failed(string message)
    {
        return new TargetResolution(false, null, message);
    }
}

internal static class TargetResolver
{
    private const BindingFlags DeclaredMethodFlags =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    internal static TargetResolution Resolve(TargetSpec spec)
    {
        Assembly[] assemblyMatches = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => string.Equals(
                assembly.GetName().Name,
                spec.AssemblyName,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (assemblyMatches.Length == 0)
        {
            return TargetResolution.Failed("assembly-not-loaded:" + spec.AssemblyName);
        }

        if (assemblyMatches.Length > 1)
        {
            return TargetResolution.Failed("assembly-name-ambiguous:" + spec.AssemblyName);
        }

        Assembly targetAssembly = assemblyMatches[0];
        Type? targetType = targetAssembly.GetType(spec.TypeName, throwOnError: false, ignoreCase: false);
        if (targetType == null)
        {
            return TargetResolution.Failed("type-not-found:" + spec.TypeName);
        }

        MethodInfo[] namedMethods = targetType
            .GetMethods(DeclaredMethodFlags)
            .Where(method => string.Equals(method.Name, spec.MethodName, StringComparison.Ordinal))
            .ToArray();

        if (namedMethods.Length == 0)
        {
            return TargetResolution.Failed("declared-method-not-found:" + spec.MethodName);
        }

        if (spec.ParameterTypeNames.Length == 0)
        {
            if (namedMethods.Length != 1)
            {
                return TargetResolution.Failed(
                    "method-overloaded-configure-ParameterTypeNames:candidates=" +
                    namedMethods.Length.ToString(CultureInfo.InvariantCulture));
            }

            if (namedMethods[0].ContainsGenericParameters)
            {
                return TargetResolution.Failed("open-generic-method-not-supported");
            }

            return TargetResolution.Passed(namedMethods[0]);
        }

        string[] configuredParameterTypeNames = spec.HasExplicitZeroParameterSignature
            ? Array.Empty<string>()
            : spec.ParameterTypeNames;

        var parameterTypes = new List<Type>(configuredParameterTypeNames.Length);
        foreach (string parameterTypeName in configuredParameterTypeNames)
        {
            Type? parameterType = ResolveType(parameterTypeName, targetAssembly);
            if (parameterType == null)
            {
                return TargetResolution.Failed("parameter-type-not-found:" + parameterTypeName);
            }

            parameterTypes.Add(parameterType);
        }

        MethodInfo[] exactMatches = namedMethods
            .Where(method => ParametersMatch(method.GetParameters(), parameterTypes))
            .ToArray();

        if (exactMatches.Length == 0)
        {
            return TargetResolution.Failed("method-signature-not-found");
        }

        if (exactMatches.Length > 1)
        {
            return TargetResolution.Failed("method-signature-ambiguous");
        }

        if (exactMatches[0].ContainsGenericParameters)
        {
            return TargetResolution.Failed("open-generic-method-not-supported");
        }

        return TargetResolution.Passed(exactMatches[0]);
    }

    private static bool ParametersMatch(ParameterInfo[] parameters, IReadOnlyList<Type> expected)
    {
        if (parameters.Length != expected.Count)
        {
            return false;
        }

        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].ParameterType != expected[i])
            {
                return false;
            }
        }

        return true;
    }

    private static Type? ResolveType(string name, Assembly targetAssembly)
    {
        switch (name)
        {
            case "bool":
                return typeof(bool);
            case "byte":
                return typeof(byte);
            case "sbyte":
                return typeof(sbyte);
            case "short":
                return typeof(short);
            case "ushort":
                return typeof(ushort);
            case "int":
                return typeof(int);
            case "uint":
                return typeof(uint);
            case "long":
                return typeof(long);
            case "ulong":
                return typeof(ulong);
            case "float":
                return typeof(float);
            case "double":
                return typeof(double);
            case "decimal":
                return typeof(decimal);
            case "char":
                return typeof(char);
            case "string":
                return typeof(string);
            case "object":
                return typeof(object);
        }

        Type? direct = Type.GetType(name, throwOnError: false, ignoreCase: false);
        if (direct != null)
        {
            return direct;
        }

        Type? inTargetAssembly = targetAssembly.GetType(name, throwOnError: false, ignoreCase: false);
        if (inTargetAssembly != null)
        {
            return inTargetAssembly;
        }

        Type[] loadedMatches = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(assembly =>
            {
                try
                {
                    return assembly.GetType(name, throwOnError: false, ignoreCase: false);
                }
                catch
                {
                    return null;
                }
            })
            .Where(type => type != null)
            .Cast<Type>()
            .ToArray();

        return loadedMatches.Length == 1 ? loadedMatches[0] : null;
    }
}

internal static class TargetInspection
{
    internal static bool CanBoxArguments(MethodBase method)
    {
        return method.GetParameters().All(parameter => CanBox(parameter.ParameterType));
    }

    internal static bool CanBoxResult(MethodBase method)
    {
        if (!(method is MethodInfo methodInfo))
        {
            return false;
        }

        Type returnType = methodInfo.ReturnType;
        return returnType != typeof(void) && !returnType.IsByRef && CanBox(returnType);
    }

    private static bool CanBox(Type type)
    {
        Type inspected = type.IsByRef ? (type.GetElementType() ?? type) : type;

        if (inspected.IsPointer)
        {
            return false;
        }

        return !inspected
            .GetCustomAttributesData()
            .Any(attribute =>
                string.Equals(
                    attribute.AttributeType.FullName,
                    "System.Runtime.CompilerServices.IsByRefLikeAttribute",
                    StringComparison.Ordinal));
    }
}

internal static class TargetFormatter
{
    internal static string Format(MethodBase method)
    {
        string assembly = method.DeclaringType?.Assembly.GetName().Name ?? "<unknown-assembly>";
        string type = method.DeclaringType?.FullName ?? "<unknown-type>";
        string parameters = string.Join(
            ",",
            method.GetParameters().Select(parameter => parameter.ParameterType.FullName ?? parameter.ParameterType.Name));

        string result = method is MethodInfo methodInfo
            ? methodInfo.ReturnType.FullName ?? methodInfo.ReturnType.Name
            : "System.Void";

        return assembly + "::" + type + "." + method.Name + "(" + parameters + ")->" + result;
    }
}

internal readonly struct TraceCallState
{
    internal TraceCallState(long invocationId, long startTimestamp, bool shouldLog)
    {
        InvocationId = invocationId;
        StartTimestamp = startTimestamp;
        ShouldLog = shouldLog;
    }

    internal long InvocationId { get; }
    internal long StartTimestamp { get; }
    internal bool ShouldLog { get; }
}

internal static class TracePatch
{
    internal static void PrefixWithoutArgs(MethodBase __originalMethod, out TraceCallState __state)
    {
        TraceController? controller = Plugin.Controller;
        __state = controller == null
            ? default
            : controller.Begin(__originalMethod, arguments: null);
    }

    internal static void PrefixWithArgs(MethodBase __originalMethod, object[] __args, out TraceCallState __state)
    {
        TraceController? controller = Plugin.Controller;
        __state = controller == null
            ? default
            : controller.Begin(__originalMethod, __args);
    }

    internal static void PostfixWithoutResult(MethodBase __originalMethod, TraceCallState __state)
    {
        Plugin.Controller?.Complete(__originalMethod, __state, hasResult: false, result: null);
    }

    internal static void PostfixWithResult(MethodBase __originalMethod, object __result, TraceCallState __state)
    {
        Plugin.Controller?.Complete(__originalMethod, __state, hasResult: true, result: __result);
    }

    internal static void Finalizer(MethodBase __originalMethod, Exception __exception, TraceCallState __state)
    {
        if (__exception != null)
        {
            Plugin.Controller?.Fault(__originalMethod, __state, __exception);
        }
    }
}

internal sealed class TraceController
{
    [ThreadStatic]
    private static int _callbackDepth;

    private readonly ManualLogSource _logger;
    private readonly bool _captureArguments;
    private readonly bool _captureResult;
    private readonly int _maxArguments;
    private readonly int _maxValueChars;
    private readonly FixedWindowRateLimiter _rateLimiter;
    private long _invocationCount;

    internal TraceController(
        ManualLogSource logger,
        bool captureArguments,
        bool captureResult,
        int maxArguments,
        int maxValueChars,
        int maxEventsPerMinute)
    {
        _logger = logger;
        _captureArguments = captureArguments;
        _captureResult = captureResult;
        _maxArguments = Math.Max(1, maxArguments);
        _maxValueChars = Math.Max(32, maxValueChars);
        _rateLimiter = new FixedWindowRateLimiter(Math.Max(1, maxEventsPerMinute));
        SessionId = Guid.NewGuid().ToString("N");
        StartedAtUtc = DateTime.UtcNow;
    }

    internal string SessionId { get; }
    internal DateTime StartedAtUtc { get; }
    internal long InvocationCount => Interlocked.Read(ref _invocationCount);

    internal TraceCallState Begin(MethodBase method, object[]? arguments)
    {
        if (_callbackDepth > 0)
        {
            return default;
        }

        long invocationId = Interlocked.Increment(ref _invocationCount);
        bool shouldLog = _rateLimiter.TryAcquire(out bool suppressionNotice);

        if (suppressionNotice)
        {
            SafeLog(() =>
                _logger.LogWarning(
                    "Runtime tracer rateLimit=ACTIVE maxEventsPerMinute=" +
                    _rateLimiter.Limit.ToString(CultureInfo.InvariantCulture) +
                    " session=" + SessionId));
        }

        long start = Stopwatch.GetTimestamp();

        if (shouldLog)
        {
            SafeLog(() =>
            {
                string line =
                    "Runtime trace event=enter" +
                    " session=" + SessionId +
                    " invocation=" + invocationId.ToString(CultureInfo.InvariantCulture) +
                    " timestampUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture) +
                    " target=" + TargetFormatter.Format(method);

                if (_captureArguments && arguments != null)
                {
                    line += " args=" + ValueRenderer.RenderArguments(arguments, _maxArguments, _maxValueChars);
                }

                _logger.LogInfo(line);
            });
        }

        return new TraceCallState(invocationId, start, shouldLog);
    }

    internal void Complete(MethodBase method, TraceCallState state, bool hasResult, object? result)
    {
        if (!state.ShouldLog || state.InvocationId == 0)
        {
            return;
        }

        double elapsedMs = ElapsedMilliseconds(state.StartTimestamp);

        SafeLog(() =>
        {
            string line =
                "Runtime trace event=exit" +
                " session=" + SessionId +
                " invocation=" + state.InvocationId.ToString(CultureInfo.InvariantCulture) +
                " durationMs=" + elapsedMs.ToString("F3", CultureInfo.InvariantCulture) +
                " target=" + TargetFormatter.Format(method);

            if (_captureResult && hasResult)
            {
                line += " result=" + ValueRenderer.RenderValue(result, _maxValueChars);
            }

            _logger.LogInfo(line);
        });
    }

    internal void Fault(MethodBase method, TraceCallState state, Exception exception)
    {
        if (!state.ShouldLog || state.InvocationId == 0)
        {
            return;
        }

        double elapsedMs = ElapsedMilliseconds(state.StartTimestamp);

        SafeLog(() =>
            _logger.LogWarning(
                "Runtime trace event=exception" +
                " session=" + SessionId +
                " invocation=" + state.InvocationId.ToString(CultureInfo.InvariantCulture) +
                " durationMs=" + elapsedMs.ToString("F3", CultureInfo.InvariantCulture) +
                " target=" + TargetFormatter.Format(method) +
                " exception=" + ValueRenderer.RenderException(exception, _maxValueChars)));
    }

    private static double ElapsedMilliseconds(long startTimestamp)
    {
        if (startTimestamp <= 0)
        {
            return 0d;
        }

        long elapsed = Stopwatch.GetTimestamp() - startTimestamp;
        return elapsed * 1000d / Stopwatch.Frequency;
    }

    private static void SafeLog(Action action)
    {
        if (_callbackDepth > 0)
        {
            return;
        }

        _callbackDepth++;
        try
        {
            action();
        }
        catch
        {
            // Logging must never alter the observed method's behaviour.
        }
        finally
        {
            _callbackDepth--;
        }
    }
}

internal sealed class FixedWindowRateLimiter
{
    private readonly object _sync = new object();
    private DateTime _windowStartUtc = DateTime.UtcNow;
    private int _used;
    private bool _suppressionNoticeSent;

    internal FixedWindowRateLimiter(int limit)
    {
        Limit = limit;
    }

    internal int Limit { get; }

    internal bool TryAcquire(out bool suppressionNotice)
    {
        lock (_sync)
        {
            DateTime now = DateTime.UtcNow;
            if (now - _windowStartUtc >= TimeSpan.FromMinutes(1))
            {
                _windowStartUtc = now;
                _used = 0;
                _suppressionNoticeSent = false;
            }

            if (_used < Limit)
            {
                _used++;
                suppressionNotice = false;
                return true;
            }

            suppressionNotice = !_suppressionNoticeSent;
            _suppressionNoticeSent = true;
            return false;
        }
    }
}

internal static class ValueRenderer
{
    internal static string RenderArguments(object[] arguments, int maxArguments, int maxValueChars)
    {
        int count = Math.Min(arguments.Length, maxArguments);
        var rendered = new string[count];

        for (int i = 0; i < count; i++)
        {
            rendered[i] =
                i.ToString(CultureInfo.InvariantCulture) +
                ":" +
                RenderValue(arguments[i], maxValueChars);
        }

        string suffix = arguments.Length > maxArguments
            ? ",...+" + (arguments.Length - maxArguments).ToString(CultureInfo.InvariantCulture)
            : string.Empty;

        return "[" + string.Join(",", rendered) + suffix + "]";
    }

    internal static string RenderValue(object? value, int maxValueChars)
    {
        if (value == null)
        {
            return "<null>";
        }

        Type type = value.GetType();

        if (value is string text)
        {
            return "\"" + Truncate(EscapeScalar(text), maxValueChars) + "\"";
        }

        if (value is char character)
        {
            return "'" + EscapeScalar(character.ToString()) + "'";
        }

        if (value is bool)
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "<bool>";
        }

        if (type.IsEnum)
        {
            return type.FullName + "." + value;
        }

        if (IsNumeric(type))
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? ("<" + type.FullName + ">");
        }

        if (value is Guid guid)
        {
            return guid.ToString("D");
        }

        if (value is DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        }

        if (value is TimeSpan timeSpan)
        {
            return timeSpan.ToString();
        }

        if (value is Array array)
        {
            return "<" + (type.FullName ?? type.Name) + " length=" +
                   array.Length.ToString(CultureInfo.InvariantCulture) + ">";
        }

        // Do not call arbitrary ToString() implementations while observing a game method.
        return "<" + (type.FullName ?? type.Name) + ">";
    }

    internal static string RenderException(Exception exception, int maxValueChars)
    {
        string type = exception.GetType().FullName ?? exception.GetType().Name;
        string message = Truncate(EscapeScalar(exception.Message ?? string.Empty), maxValueChars);
        return type + ":\"" + message + "\"";
    }

    private static bool IsNumeric(Type type)
    {
        TypeCode code = Type.GetTypeCode(type);
        switch (code)
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
                return true;
            default:
                return false;
        }
    }

    private static string EscapeScalar(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t")
            .Replace("\"", "\\\"");
    }

    private static string Truncate(string value, int maxChars)
    {
        if (value.Length <= maxChars)
        {
            return value;
        }

        return value.Substring(0, Math.Max(0, maxChars - 3)) + "...";
    }
}

internal static class SelfTestTarget
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Ping(int value)
    {
        return value + 1;
    }
}
