using System;
using System.Reflection;
using UnityEngine;

namespace AvalonBroodmotherCompanion;

internal static class FoAModManagerBridge
{
    private const string ApiTypeName = "FoAModManager.FoAModManagerApi";

    private static MethodInfo? s_setCustomUiScope;
    private static MethodInfo? s_setControllerCursorScope;
    private static PropertyInfo? s_isControllerCursorInputReadActive;
    private static bool s_loggedInvokeFailure;

    internal static bool SetCustomUiScope(string ownerId, bool active, bool freezeWorld)
    {
        MethodInfo? method = ResolveSetCustomUiScope();
        if (method == null)
        {
            return SetControllerCursorScope(ownerId, active);
        }

        try
        {
            method.Invoke(null, new object[] { ownerId, active, freezeWorld });
            return true;
        }
        catch (Exception ex)
        {
            LogInvokeFailure("shared UI scope", ex);
            return SetControllerCursorScope(ownerId, active);
        }
    }

    private static bool SetControllerCursorScope(string ownerId, bool active)
    {
        MethodInfo? method = ResolveSetControllerCursorScope();
        if (method == null)
        {
            return false;
        }

        try
        {
            method.Invoke(null, new object[] { ownerId, active });
            return true;
        }
        catch (Exception ex)
        {
            LogInvokeFailure("controller cursor scope", ex);
            return false;
        }
    }

    internal static bool IsControllerCursorInputReadActive()
    {
        PropertyInfo? property = ResolveIsControllerCursorInputReadActive();
        if (property == null)
        {
            return false;
        }

        try
        {
            return property.GetValue(null) is bool active && active;
        }
        catch
        {
            return false;
        }
    }

    private static MethodInfo? ResolveSetCustomUiScope()
    {
        if (s_setCustomUiScope != null)
        {
            return s_setCustomUiScope;
        }

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? apiType = assembly.GetType(ApiTypeName, throwOnError: false);
            MethodInfo? method = apiType?.GetMethod(
                "SetCustomUiScope",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(bool), typeof(bool) },
                null);
            if (method == null)
            {
                continue;
            }

            s_setCustomUiScope = method;
            return method;
        }

        return null;
    }

    private static MethodInfo? ResolveSetControllerCursorScope()
    {
        if (s_setControllerCursorScope != null)
        {
            return s_setControllerCursorScope;
        }

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? apiType = assembly.GetType(ApiTypeName, throwOnError: false);
            MethodInfo? method = apiType?.GetMethod(
                "SetControllerCursorScope",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(bool) },
                null);
            if (method == null)
            {
                continue;
            }

            s_setControllerCursorScope = method;
            return method;
        }

        return null;
    }

    private static PropertyInfo? ResolveIsControllerCursorInputReadActive()
    {
        if (s_isControllerCursorInputReadActive != null)
        {
            return s_isControllerCursorInputReadActive;
        }

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? apiType = assembly.GetType(ApiTypeName, throwOnError: false);
            PropertyInfo? property = apiType?.GetProperty(
                "IsControllerCursorInputReadActive",
                BindingFlags.Public | BindingFlags.Static);
            if (property == null || property.PropertyType != typeof(bool))
            {
                continue;
            }

            s_isControllerCursorInputReadActive = property;
            return property;
        }

        return null;
    }

    private static void LogInvokeFailure(string scope, Exception exception)
    {
        if (s_loggedInvokeFailure)
        {
            return;
        }

        s_loggedInvokeFailure = true;
        Debug.LogWarning($"{Plugin.PluginName} could not update FoA Mod Manager {scope}: {exception.GetType().Name}: {exception.Message}");
    }
}
