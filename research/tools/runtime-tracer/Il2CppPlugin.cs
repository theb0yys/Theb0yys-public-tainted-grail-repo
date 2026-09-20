using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace TGCommunity.RuntimeTracer;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "community.taintedgrail.runtime-tracer-il2cpp";
    public const string PluginName = "TG Community Runtime Tracer IL2CPP";
    public const string PluginVersion = "0.1.0";

    private Harmony? _harmony;
    private MethodBase? _target;

    private ConfigEntry<bool>? _enabled;
    private ConfigEntry<bool>? _selfTest;
    private ConfigEntry<string>? _assemblyName;
    private ConfigEntry<string>? _typeName;
    private ConfigEntry<string>? _methodName;
    private ConfigEntry<string>? _parameterTypeNames;
    private ConfigEntry<bool>? _traceArguments;
    private ConfigEntry<bool>? _traceResult;
    private ConfigEntry<int>? _maxArguments;
    private ConfigEntry<int>? _maxValueChars;
    private ConfigEntry<int>? _maxEventsPerMinute;

    internal static TraceController? Controller { get; private set; }

    public override void Load()
    {
        BindConfiguration();

        if (_enabled?.Value != true)
        {
            Log.LogInfo("Runtime tracer enabled=false. No patches installed.");
            return;
        }

        TargetSpec spec;
        if (_selfTest?.Value == true)
        {
            spec = TargetSpec.ForSelfTest();
            Log.LogInfo("Runtime tracer self-test target selected. No game method will be patched.");
        }
        else
        {
            spec = new TargetSpec(
                _assemblyName?.Value ?? string.Empty,
                _typeName?.Value ?? string.Empty,
                _methodName?.Value ?? string.Empty,
                TargetSpec.ParseParameterTypes(_parameterTypeNames?.Value ?? string.Empty));

            if (!spec.IsConfigured)
            {
                Log.LogWarning(
                    "Runtime tracer targetConfigured=false. " +
                    "Set Target.AssemblyName, Target.TypeName and Target.MethodName, or enable SelfTest.Enabled.");
                return;
            }
        }

        TargetResolution resolution = TargetResolver.Resolve(spec);
        if (!resolution.Success || resolution.Method == null)
        {
            Log.LogError("Runtime tracer targetResolution=FAILED reason=" + resolution.Message);
            return;
        }

        _target = resolution.Method;
        Log.LogInfo("Runtime tracer targetResolution=PASSED target=" + TargetFormatter.Format(_target));

        bool requestedArguments = _traceArguments?.Value == true;
        bool requestedResult = _traceResult?.Value == true;
        bool canCaptureArguments = TargetInspection.CanBoxArguments(_target);
        bool canCaptureResult = TargetInspection.CanBoxResult(_target);

        bool captureArguments = requestedArguments && canCaptureArguments;
        bool captureResult = requestedResult && canCaptureResult;

        if (requestedArguments && !canCaptureArguments)
        {
            Log.LogWarning("Argument capture disabled for this target because at least one parameter cannot be safely boxed.");
        }

        if (requestedResult && !canCaptureResult)
        {
            Log.LogWarning("Result capture disabled for this target because the return value cannot be safely boxed.");
        }

        Controller = new TraceController(
            Log,
            captureArguments,
            captureResult,
            _maxArguments?.Value ?? 8,
            _maxValueChars?.Value ?? 160,
            _maxEventsPerMinute?.Value ?? 120);

        MethodInfo? prefix = AccessTools.Method(
            typeof(TracePatch),
            captureArguments ? nameof(TracePatch.PrefixWithArgs) : nameof(TracePatch.PrefixWithoutArgs));

        MethodInfo? postfix = AccessTools.Method(
            typeof(TracePatch),
            captureResult ? nameof(TracePatch.PostfixWithResult) : nameof(TracePatch.PostfixWithoutResult));

        MethodInfo? finalizer = AccessTools.Method(typeof(TracePatch), nameof(TracePatch.Finalizer));

        if (prefix == null || postfix == null || finalizer == null)
        {
            Log.LogError("Runtime tracer patchInstallation=FAILED reason=internal-patch-method-resolution.");
            Controller = null;
            return;
        }

        _harmony = new Harmony(PluginGuid);

        try
        {
            _harmony.Patch(
                _target,
                prefix: new HarmonyMethod(prefix),
                postfix: new HarmonyMethod(postfix),
                transpiler: null,
                finalizer: new HarmonyMethod(finalizer));
        }
        catch (Exception ex)
        {
            Log.LogError(
                "Runtime tracer patchInstallation=FAILED exception=" +
                ValueRenderer.RenderException(ex, _maxValueChars?.Value ?? 160));
            _harmony.UnpatchSelf();
            _harmony = null;
            Controller = null;
            return;
        }

        Patches? patchInfo = Harmony.GetPatchInfo(_target);
        bool prefixInstalled = patchInfo?.Prefixes.Any(p => p.owner == PluginGuid) == true;
        bool postfixInstalled = patchInfo?.Postfixes.Any(p => p.owner == PluginGuid) == true;
        bool finalizerInstalled = patchInfo?.Finalizers.Any(p => p.owner == PluginGuid) == true;

        if (!(prefixInstalled && postfixInstalled && finalizerInstalled))
        {
            Log.LogError("Runtime tracer patchInstallation=FAILED reason=owner-not-present-on-all-required-patch-stages.");
            _harmony.UnpatchSelf();
            _harmony = null;
            Controller = null;
            return;
        }

        Log.LogInfo(
            "Runtime tracer patchInstallation=PASSED " +
            "session=" + Controller.SessionId +
            " startedUtc=" + Controller.StartedAtUtc.ToString("O", CultureInfo.InvariantCulture) +
            " gameVersion=" + SafeApplicationValue(() => Application.version) +
            " unityVersion=" + SafeApplicationValue(() => Application.unityVersion) +
            " loaderVersion=" + (typeof(BasePlugin).Assembly.GetName().Version?.ToString() ?? "unknown") +
            " target=" + TargetFormatter.Format(_target) +
            " traceArguments=" + captureArguments.ToString(CultureInfo.InvariantCulture) +
            " traceResult=" + captureResult.ToString(CultureInfo.InvariantCulture));

        if (_selfTest?.Value == true)
        {
            int result = SelfTestTarget.Ping(41);
            bool observed = Controller.InvocationCount > 0;
            bool behaviourPreserved = result == 42;

            Log.LogInfo(
                "Runtime tracer selfTestRuntimeObservation=" +
                (observed && behaviourPreserved ? "PASSED" : "FAILED") +
                " invocationObserved=" + observed.ToString(CultureInfo.InvariantCulture) +
                " originalBehaviourPreserved=" + behaviourPreserved.ToString(CultureInfo.InvariantCulture) +
                " invocationCount=" + Controller.InvocationCount.ToString(CultureInfo.InvariantCulture));
        }
    }

    public override bool Unload()
    {
        TraceController? controller = Controller;
        if (controller != null)
        {
            Log.LogInfo(
                "Runtime tracer sessionSummary observed=" +
                (controller.InvocationCount > 0 ? "true" : "false") +
                " session=" + controller.SessionId +
                " invocationCount=" + controller.InvocationCount.ToString(CultureInfo.InvariantCulture) +
                " target=" + (_target == null ? "<none>" : TargetFormatter.Format(_target)));
        }

        _harmony?.UnpatchSelf();
        _harmony = null;
        _target = null;
        Controller = null;
        return true;
    }

    private void BindConfiguration()
    {
        _enabled = Config.Bind("General", "Enabled", false, "Enable the tracer. Disabled means no Harmony patches are installed.");
        _selfTest = Config.Bind("SelfTest", "Enabled", false, "Trace and invoke the plug-in's self-owned test method instead of a game method.");
        _assemblyName = Config.Bind("Target", "AssemblyName", string.Empty, "Exact simple assembly name, for example TG.Main.");
        _typeName = Config.Bind("Target", "TypeName", string.Empty, "Exact full declaring type name including namespace.");
        _methodName = Config.Bind("Target", "MethodName", string.Empty, "Exact declared method name. Constructors are not supported by this tracer.");
        _parameterTypeNames = Config.Bind("Target", "ParameterTypeNames", string.Empty, "Optional semicolon-separated exact parameter type names. Required when overloaded; use <none> for an explicit zero-parameter signature.");
        _traceArguments = Config.Bind("Capture", "TraceArguments", false, "Capture bounded argument representations when the target parameters can be safely boxed.");
        _traceResult = Config.Bind("Capture", "TraceResult", false, "Capture a bounded result representation when the target return type can be safely boxed.");
        _maxArguments = Config.Bind("Limits", "MaxArguments", 8, new ConfigDescription("Maximum number of arguments rendered for one invocation.", new AcceptableValueRange<int>(1, 32)));
        _maxValueChars = Config.Bind("Limits", "MaxValueChars", 160, new ConfigDescription("Maximum characters rendered for one scalar string/exception value.", new AcceptableValueRange<int>(32, 2048)));
        _maxEventsPerMinute = Config.Bind("Limits", "MaxEventsPerMinute", 120, new ConfigDescription("Maximum traced invocations logged per fixed one-minute window.", new AcceptableValueRange<int>(1, 10000)));
    }

    private static string SafeApplicationValue(Func<string> getter)
    {
        try
        {
            return getter() ?? "unknown";
        }
        catch
        {
            return "unknown";
        }
    }
}
