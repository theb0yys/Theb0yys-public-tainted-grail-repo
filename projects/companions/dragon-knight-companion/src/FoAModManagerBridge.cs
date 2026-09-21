using System;
using System.Reflection;
using UnityEngine;

namespace DragonKnightCompanion;

internal static class FoAModManagerBridge
{
    private const string ApiTypeName = "FoAModManager.FoAModManagerApi";

    private static MethodInfo? s_setCustomUiScope;
    private static MethodInfo? s_setControllerCursorScope;
    private static bool s_loggedInvokeFailure;

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
            if (!s_loggedInvokeFailure)
            {
                s_loggedInvokeFailure = true;
                Debug.LogWarning($"Dragon Knight Companion could not update FoA Mod Manager shared UI scope: {ex.GetType().Name}: {ex.Message}");
            }

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
            if (!s_loggedInvokeFailure)
            {
                s_loggedInvokeFailure = true;
                Debug.LogWarning($"Dragon Knight Companion could not update FoA Mod Manager controller cursor scope: {ex.GetType().Name}: {ex.Message}");
            }
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
}

