using System;
using System.Reflection;
using UnityEngine;

namespace AvalonBroodmotherCompanion;

internal static class TaintedInterfaceReflection
{
    private const string ApiTypeName = "TaintedInterface.TaintedInterfaceApi";

    private static Type? s_apiType;
    private static MethodInfo? s_beginCustomUiScope;
    private static MethodInfo? s_endCustomUiScope;
    private static MethodInfo? s_ensureInteractiveCursor;
    private static MethodInfo? s_getIcon;
    private static MethodInfo? s_getItemIcon;
    private static PropertyInfo? s_isAvailable;
    private static bool s_loggedInvokeFailure;

    internal static bool BeginCustomUiScope(string ownerId, bool freezeWorld)
    {
        MethodInfo? method = ResolveBeginCustomUiScope();
        if (method == null)
        {
            return false;
        }

        try
        {
            return method.Invoke(null, new object[] { ownerId, freezeWorld }) is bool acquired && acquired;
        }
        catch (Exception ex)
        {
            LogInvokeFailure("begin Tainted Interface custom UI scope", ex);
            return false;
        }
    }

    internal static void EndCustomUiScope(string ownerId)
    {
        MethodInfo? method = ResolveEndCustomUiScope();
        if (method == null)
        {
            return;
        }

        try
        {
            method.Invoke(null, new object[] { ownerId });
        }
        catch (Exception ex)
        {
            LogInvokeFailure("end Tainted Interface custom UI scope", ex);
        }
    }

    internal static void EnsureInteractiveCursor()
    {
        MethodInfo? method = ResolveEnsureInteractiveCursor();
        if (method == null)
        {
            return;
        }

        try
        {
            method.Invoke(null, Array.Empty<object>());
        }
        catch (Exception ex)
        {
            LogInvokeFailure("ensure Tainted Interface cursor", ex);
        }
    }

    internal static Texture2D? GetIcon(string iconId)
    {
        if (string.IsNullOrWhiteSpace(iconId) || !IsAvailable())
        {
            return null;
        }

        MethodInfo? method = ResolveGetIcon();
        if (method == null)
        {
            return null;
        }

        try
        {
            return method.Invoke(null, new object[] { iconId }) as Texture2D;
        }
        catch (Exception ex)
        {
            LogInvokeFailure("read Tainted Interface icon", ex);
            return null;
        }
    }

    internal static Texture2D? GetItemIcon(string itemReference)
    {
        if (string.IsNullOrWhiteSpace(itemReference) || !IsAvailable())
        {
            return null;
        }

        MethodInfo? method = ResolveGetItemIcon();
        if (method == null)
        {
            return null;
        }

        try
        {
            return method.Invoke(null, new object[] { itemReference }) as Texture2D;
        }
        catch (Exception ex)
        {
            LogInvokeFailure("read Tainted Interface item icon", ex);
            return null;
        }
    }

    private static bool IsAvailable()
    {
        PropertyInfo? property = ResolveIsAvailable();
        if (property == null)
        {
            return false;
        }

        try
        {
            return property.GetValue(null) is bool available && available;
        }
        catch
        {
            return false;
        }
    }

    private static MethodInfo? ResolveBeginCustomUiScope()
    {
        return s_beginCustomUiScope ??= ResolveApiType()?.GetMethod(
            "BeginCustomUiScope",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string), typeof(bool) },
            null);
    }

    private static MethodInfo? ResolveEndCustomUiScope()
    {
        return s_endCustomUiScope ??= ResolveApiType()?.GetMethod(
            "EndCustomUiScope",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string) },
            null);
    }

    private static MethodInfo? ResolveEnsureInteractiveCursor()
    {
        return s_ensureInteractiveCursor ??= ResolveApiType()?.GetMethod(
            "EnsureInteractiveCursor",
            BindingFlags.Public | BindingFlags.Static,
            null,
            Type.EmptyTypes,
            null);
    }

    private static MethodInfo? ResolveGetIcon()
    {
        return s_getIcon ??= ResolveApiType()?.GetMethod(
            "GetIcon",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string) },
            null);
    }

    private static MethodInfo? ResolveGetItemIcon()
    {
        return s_getItemIcon ??= ResolveApiType()?.GetMethod(
            "GetItemIcon",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string) },
            null);
    }

    private static PropertyInfo? ResolveIsAvailable()
    {
        return s_isAvailable ??= ResolveApiType()?.GetProperty(
            "IsAvailable",
            BindingFlags.Public | BindingFlags.Static);
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
            if (apiType != null)
            {
                s_apiType = apiType;
                return apiType;
            }
        }

        Type? namedType = Type.GetType(ApiTypeName + ", TaintedInterface", throwOnError: false);
        if (namedType != null)
        {
            s_apiType = namedType;
        }

        return s_apiType;
    }

    private static void LogInvokeFailure(string action, Exception exception)
    {
        if (s_loggedInvokeFailure)
        {
            return;
        }

        s_loggedInvokeFailure = true;
        Debug.LogWarning($"{Plugin.PluginName} could not {action}: {exception.GetType().Name}: {exception.Message}");
    }
}
