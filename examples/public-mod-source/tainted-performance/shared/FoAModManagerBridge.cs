using System;
using System.Linq.Expressions;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

namespace TaintedPerformance;

internal static class FoAModManagerBridge
{
    private const string ApiTypeName = "FoAModManager.FoAModManagerApi";
    private const string StatusSnapshotTypeName = "FoAModManager.FoAModStatusSnapshot";
    private const string StatusLevelTypeName = "FoAModManager.FoAModStatusLevel";
    private const string StatusProviderId = Plugin.PluginGuid + ".status";
    private const string OpenOverviewActionId = Plugin.PluginGuid + ".open-overview";

    private static Type? s_apiType;
    private static MethodInfo? s_setCustomUiScope;
    private static MethodInfo? s_setControllerCursorScope;
    private static MethodInfo? s_open;
    private static MethodInfo? s_refresh;
    private static MethodInfo? s_registerControllerAction;
    private static MethodInfo? s_unregisterControllerAction;
    private static MethodInfo? s_registerStatusProvider;
    private static MethodInfo? s_unregisterStatusProvider;
    private static Type? s_statusSnapshotType;
    private static Type? s_statusLevelType;
    private static Delegate? s_statusProviderDelegate;
    private static Func<PerformanceRuntimeOverview>? s_snapshotProvider;
    private static ManualLogSource? s_logger;
    private static bool s_loggedInvokeFailure;
    private static bool s_statusRegistered;
    private static bool s_actionRegistered;

    internal static void RegisterIntegrations(
        ManualLogSource logger,
        Func<PerformanceRuntimeOverview> snapshotProvider,
        Action openOverview)
    {
        s_logger = logger;
        TaintedInterfaceBridge.SetLogger(logger);
        s_snapshotProvider = snapshotProvider;
        RegisterStatusProvider();
        RegisterControllerAction(openOverview);
    }

    internal static void UnregisterIntegrations()
    {
        if (s_actionRegistered)
        {
            TryInvokeUnregisterControllerAction(OpenOverviewActionId);
            s_actionRegistered = false;
        }

        if (s_statusRegistered)
        {
            TryInvokeUnregisterStatusProvider(StatusProviderId);
            s_statusRegistered = false;
        }
    }

    internal static bool OpenForPerformanceMod(string pluginGuid)
    {
        bool specific = TryInvokeStringMethod("OpenMod", pluginGuid) ||
                        TryInvokeStringMethod("OpenPlugin", pluginGuid) ||
                        TryInvokeStringMethod("OpenModPage", pluginGuid) ||
                        TryInvokeStringMethod("ShowMod", pluginGuid) ||
                        TryInvokeStringMethod("ShowPlugin", pluginGuid) ||
                        TryInvokeStringMethod("SelectMod", pluginGuid) ||
                        TryInvokeStringMethod("SelectPlugin", pluginGuid);
        if (specific)
        {
            TryInvokeNoArg(ResolveRefresh());
            return true;
        }

        bool opened = TryInvokeNoArg(ResolveOpen());
        TryInvokeNoArg(ResolveRefresh());
        return opened;
    }

    internal static void SetCustomUiScope(string ownerId, bool active, bool freezeWorld)
    {
        MethodInfo? method = ResolveSetCustomUiScope();
        if (method == null)
        {
            SetControllerCursorScope(ownerId, active);
            return;
        }

        try
        {
            method.Invoke(null, new object[] { ownerId, active, freezeWorld });
        }
        catch (Exception ex)
        {
            LogInvokeFailure("update FoA Mod Manager custom UI scope", ex);
            SetControllerCursorScope(ownerId, active);
        }
    }

    private static void SetControllerCursorScope(string ownerId, bool active)
    {
        MethodInfo? method = ResolveSetControllerCursorScope();
        if (method == null)
        {
            return;
        }

        try
        {
            method.Invoke(null, new object[] { ownerId, active });
        }
        catch (Exception ex)
        {
            LogInvokeFailure("update FoA Mod Manager controller cursor scope", ex);
        }
    }

    private static void RegisterControllerAction(Action openOverview)
    {
        MethodInfo? method = ResolveRegisterControllerAction();
        if (method == null)
        {
            return;
        }

        try
        {
            object? result = method.Invoke(
                null,
                new object[]
                {
                    OpenOverviewActionId,
                    "Open Tainted Performance",
                    "Performance",
                    "Open the Tainted Performance overview screen.",
                    openOverview
                });
            s_actionRegistered = result is bool registered && registered;
        }
        catch (Exception ex)
        {
            LogInvokeFailure("register FoA Mod Manager controller action", ex);
        }
    }

    private static void RegisterStatusProvider()
    {
        MethodInfo? method = ResolveRegisterStatusProvider();
        if (method == null || s_statusSnapshotType == null)
        {
            return;
        }

        try
        {
            s_statusProviderDelegate ??= CreateStatusProviderDelegate(s_statusSnapshotType);
            if (s_statusProviderDelegate == null)
            {
                return;
            }

            object? result = method.Invoke(
                null,
                new object[]
                {
                    StatusProviderId,
                    Plugin.PluginName,
                    "Performance",
                    "Read-only frame-time, report, native-settings, and mod-stack status from Tainted Performance.",
                    s_statusProviderDelegate
                });
            s_statusRegistered = result is bool registered && registered;
        }
        catch (Exception ex)
        {
            LogInvokeFailure("register FoA Mod Manager status provider", ex);
        }
    }

    private static Delegate? CreateStatusProviderDelegate(Type statusSnapshotType)
    {
        try
        {
            Func<object> provider = BuildStatusSnapshot;
            Type funcType = typeof(Func<>).MakeGenericType(statusSnapshotType);
            Expression<Func<object>> source = () => provider();
            UnaryExpression converted = Expression.Convert(source.Body, statusSnapshotType);
            return Expression.Lambda(funcType, converted).Compile();
        }
        catch (Exception ex)
        {
            LogInvokeFailure("create FoA Mod Manager status delegate", ex);
            return null;
        }
    }

    private static object BuildStatusSnapshot()
    {
        if (s_statusSnapshotType == null)
        {
            throw new InvalidOperationException("FoA Mod Manager status snapshot type is unavailable.");
        }

        PerformanceRuntimeOverview overview = s_snapshotProvider?.Invoke() ?? new PerformanceRuntimeOverview
        {
            UpdatedUtc = DateTime.UtcNow,
            StatusLevel = "Info",
            Summary = "Tainted Performance is loaded.",
            Detail = "Runtime overview is not available yet."
        };

        object snapshot = Activator.CreateInstance(s_statusSnapshotType)!;
        SetProperty(snapshot, "Level", BuildStatusLevel(overview.StatusLevel));
        SetProperty(snapshot, "Summary", overview.Summary);
        SetProperty(snapshot, "Detail", overview.Detail);
        SetProperty(snapshot, "Schema", "tainted-performance-status-v1");
        SetProperty(snapshot, "UpdatedUtc", overview.UpdatedUtc.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture));
        SetProperty(snapshot, "Lines", overview.ManagerLines ?? Array.Empty<string>());
        return snapshot;
    }

    private static object? BuildStatusLevel(string statusLevel)
    {
        if (s_statusLevelType == null)
        {
            return null;
        }

        try
        {
            return Enum.Parse(s_statusLevelType, string.IsNullOrWhiteSpace(statusLevel) ? "Info" : statusLevel, ignoreCase: true);
        }
        catch
        {
            return Enum.Parse(s_statusLevelType, "Info", ignoreCase: true);
        }
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        PropertyInfo? property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property == null || !property.CanWrite)
        {
            return;
        }

        property.SetValue(target, value);
    }

    private static bool TryInvokeStringMethod(string methodName, string value)
    {
        Type? apiType = ResolveApiType();
        MethodInfo? method = apiType?.GetMethod(
            methodName,
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string) },
            null);
        if (method == null)
        {
            return false;
        }

        try
        {
            object? result = method.Invoke(null, new object[] { value });
            return method.ReturnType == typeof(void) || result is bool invoked && invoked;
        }
        catch (Exception ex)
        {
            LogInvokeFailure("invoke FoA Mod Manager " + methodName, ex);
            return false;
        }
    }

    private static bool TryInvokeNoArg(MethodInfo? method)
    {
        if (method == null)
        {
            return false;
        }

        try
        {
            object? result = method.Invoke(null, Array.Empty<object>());
            return method.ReturnType == typeof(void) || result is bool invoked && invoked;
        }
        catch (Exception ex)
        {
            LogInvokeFailure("invoke FoA Mod Manager " + method.Name, ex);
            return false;
        }
    }

    private static void TryInvokeUnregisterControllerAction(string actionId)
    {
        MethodInfo? method = ResolveUnregisterControllerAction();
        if (method == null)
        {
            return;
        }

        try
        {
            method.Invoke(null, new object[] { actionId });
        }
        catch (Exception ex)
        {
            LogInvokeFailure("unregister FoA Mod Manager controller action", ex);
        }
    }

    private static void TryInvokeUnregisterStatusProvider(string providerId)
    {
        MethodInfo? method = ResolveUnregisterStatusProvider();
        if (method == null)
        {
            return;
        }

        try
        {
            method.Invoke(null, new object[] { providerId });
        }
        catch (Exception ex)
        {
            LogInvokeFailure("unregister FoA Mod Manager status provider", ex);
        }
    }

    private static MethodInfo? ResolveSetCustomUiScope()
    {
        return s_setCustomUiScope ??= ResolveApiType()?.GetMethod(
            "SetCustomUiScope",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string), typeof(bool), typeof(bool) },
            null);
    }

    private static MethodInfo? ResolveSetControllerCursorScope()
    {
        return s_setControllerCursorScope ??= ResolveApiType()?.GetMethod(
            "SetControllerCursorScope",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string), typeof(bool) },
            null);
    }

    private static MethodInfo? ResolveOpen()
    {
        return s_open ??= ResolveApiType()?.GetMethod("Open", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
    }

    private static MethodInfo? ResolveRefresh()
    {
        return s_refresh ??= ResolveApiType()?.GetMethod("Refresh", BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
    }

    private static MethodInfo? ResolveRegisterControllerAction()
    {
        if (s_registerControllerAction != null)
        {
            return s_registerControllerAction;
        }

        Type? apiType = ResolveApiType();
        if (apiType == null)
        {
            return null;
        }

        foreach (MethodInfo method in apiType.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (method.Name == "RegisterControllerAction" &&
                parameters.Length == 5 &&
                parameters[0].ParameterType == typeof(string) &&
                parameters[1].ParameterType == typeof(string) &&
                parameters[2].ParameterType == typeof(string) &&
                parameters[3].ParameterType == typeof(string) &&
                parameters[4].ParameterType == typeof(Action))
            {
                s_registerControllerAction = method;
                return method;
            }
        }

        return null;
    }

    private static MethodInfo? ResolveUnregisterControllerAction()
    {
        return s_unregisterControllerAction ??= ResolveApiType()?.GetMethod(
            "UnregisterControllerAction",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string) },
            null);
    }

    private static MethodInfo? ResolveRegisterStatusProvider()
    {
        if (s_registerStatusProvider != null)
        {
            return s_registerStatusProvider;
        }

        Type? apiType = ResolveApiType();
        if (apiType == null)
        {
            return null;
        }

        s_statusSnapshotType = apiType.Assembly.GetType(StatusSnapshotTypeName, throwOnError: false);
        s_statusLevelType = apiType.Assembly.GetType(StatusLevelTypeName, throwOnError: false);
        if (s_statusSnapshotType == null || s_statusLevelType == null)
        {
            return null;
        }

        Type callbackType = typeof(Func<>).MakeGenericType(s_statusSnapshotType);
        foreach (MethodInfo method in apiType.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (method.Name == "RegisterStatusProvider" &&
                parameters.Length == 5 &&
                parameters[0].ParameterType == typeof(string) &&
                parameters[1].ParameterType == typeof(string) &&
                parameters[2].ParameterType == typeof(string) &&
                parameters[3].ParameterType == typeof(string) &&
                parameters[4].ParameterType == callbackType)
            {
                s_registerStatusProvider = method;
                return method;
            }
        }

        return null;
    }

    private static MethodInfo? ResolveUnregisterStatusProvider()
    {
        return s_unregisterStatusProvider ??= ResolveApiType()?.GetMethod(
            "UnregisterStatusProvider",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string) },
            null);
    }

    private static Type? ResolveApiType()
    {
        if (s_apiType != null)
        {
            return s_apiType;
        }

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? apiType = assembly.GetType(ApiTypeName, throwOnError: false);
            if (apiType == null)
            {
                continue;
            }

            s_apiType = apiType;
            return apiType;
        }

        return null;
    }

    private static void LogInvokeFailure(string action, Exception exception)
    {
        if (s_loggedInvokeFailure)
        {
            return;
        }

        s_loggedInvokeFailure = true;
        s_logger?.LogWarning($"{Plugin.PluginName} could not {action}: {exception.GetType().Name}: {exception.Message}");
        if (s_logger == null)
        {
            Debug.LogWarning($"{Plugin.PluginName} could not {action}: {exception.GetType().Name}: {exception.Message}");
        }
    }
}
