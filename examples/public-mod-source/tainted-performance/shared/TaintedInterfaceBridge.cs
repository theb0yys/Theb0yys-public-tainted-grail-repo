using System;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

namespace TaintedPerformance;

internal static class TaintedInterfaceBridge
{
    private const string ApiTypeName = "TaintedInterface.TaintedInterfaceApi";

    private static Type? s_apiType;
    private static MethodInfo? s_openScreenScope;
    private static MethodInfo? s_closeScreenScope;
    private static MethodInfo? s_focusScreenScope;
    private static MethodInfo? s_isScreenScopeOpen;
    private static MethodInfo? s_beginCustomUiScope;
    private static MethodInfo? s_endCustomUiScope;
    private static MethodInfo? s_ensureInteractiveCursor;
    private static MethodInfo? s_getStyles;
    private static MethodInfo? s_getUiTexture;
    private static PropertyInfo? s_isAvailable;
    private static ManualLogSource? s_logger;
    private static bool s_loggedInvokeFailure;

    internal static void SetLogger(ManualLogSource logger)
    {
        s_logger = logger;
    }

    internal static bool OpenScreenScope(string screenId, bool freezeWorld)
    {
        MethodInfo? method = ResolveOpenScreenScope();
        if (method == null)
        {
            return BeginCustomUiScope(screenId, freezeWorld);
        }

        try
        {
            return method.Invoke(null, new object[] { screenId, freezeWorld }) is bool acquired && acquired;
        }
        catch (Exception ex)
        {
            LogInvokeFailure("open Tainted Interface screen scope", ex);
            return BeginCustomUiScope(screenId, freezeWorld);
        }
    }

    internal static void CloseScreenScope(string screenId)
    {
        MethodInfo? method = ResolveCloseScreenScope();
        if (method == null)
        {
            EndCustomUiScope(screenId);
            return;
        }

        try
        {
            method.Invoke(null, new object[] { screenId });
        }
        catch (Exception ex)
        {
            LogInvokeFailure("close Tainted Interface screen scope", ex);
            EndCustomUiScope(screenId);
        }
    }

    internal static bool FocusScreenScope(string screenId)
    {
        MethodInfo? method = ResolveFocusScreenScope();
        if (method == null)
        {
            EnsureInteractiveCursor();
            return false;
        }

        try
        {
            return method.Invoke(null, new object[] { screenId }) is bool focused && focused;
        }
        catch (Exception ex)
        {
            LogInvokeFailure("focus Tainted Interface screen scope", ex);
            EnsureInteractiveCursor();
            return false;
        }
    }

    internal static bool IsScreenScopeOpen(string screenId)
    {
        MethodInfo? method = ResolveIsScreenScopeOpen();
        if (method == null)
        {
            return false;
        }

        try
        {
            return method.Invoke(null, new object[] { screenId }) is bool open && open;
        }
        catch (Exception ex)
        {
            LogInvokeFailure("read Tainted Interface screen scope", ex);
            return false;
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

    internal static bool TryGetStyles(out SharedStyles? sharedStyles)
    {
        sharedStyles = null;
        if (!IsAvailable())
        {
            return false;
        }

        MethodInfo? method = ResolveGetStyles();
        if (method == null)
        {
            return false;
        }

        try
        {
            object? styles = method.Invoke(null, Array.Empty<object>());
            if (styles == null)
            {
                return false;
            }

            GUIStyle? window = ReadStyle(styles, "Window");
            GUIStyle? header = ReadStyle(styles, "Header");
            GUIStyle? panel = ReadStyle(styles, "Panel");
            GUIStyle? card = ReadStyle(styles, "Card");
            GUIStyle? button = ReadStyle(styles, "Button");
            GUIStyle? secondaryButton = ReadStyle(styles, "SecondaryButton");
            GUIStyle? title = ReadStyle(styles, "Title");
            GUIStyle? label = ReadStyle(styles, "Label");
            GUIStyle? mutedLabel = ReadStyle(styles, "MutedLabel");

            if (window == null || panel == null || button == null || title == null || label == null || mutedLabel == null)
            {
                return false;
            }

            sharedStyles = new SharedStyles(
                window,
                header ?? panel,
                panel,
                card ?? panel,
                button,
                secondaryButton ?? button,
                title,
                label,
                mutedLabel,
                TryGetUiTexture("ui.decor.divider"),
                TryGetUiTexture("ui.progress.bar-background"),
                TryGetUiTexture("ui.progress.bar-fill"));
            return true;
        }
        catch (Exception ex)
        {
            LogInvokeFailure("read Tainted Interface styles", ex);
            return false;
        }
    }

    private static bool BeginCustomUiScope(string ownerId, bool freezeWorld)
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

    private static void EndCustomUiScope(string ownerId)
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

    private static GUIStyle? ReadStyle(object styles, string propertyName)
    {
        return styles.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(styles) as GUIStyle;
    }

    private static Texture2D? TryGetUiTexture(string textureId)
    {
        MethodInfo? method = ResolveGetUiTexture();
        if (method == null)
        {
            return null;
        }

        try
        {
            return method.Invoke(null, new object[] { textureId }) as Texture2D;
        }
        catch
        {
            return null;
        }
    }

    private static MethodInfo? ResolveOpenScreenScope()
    {
        return s_openScreenScope ??= ResolveApiType()?.GetMethod(
            "OpenScreenScope",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string), typeof(bool) },
            null);
    }

    private static MethodInfo? ResolveCloseScreenScope()
    {
        return s_closeScreenScope ??= ResolveApiType()?.GetMethod(
            "CloseScreenScope",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string) },
            null);
    }

    private static MethodInfo? ResolveFocusScreenScope()
    {
        return s_focusScreenScope ??= ResolveApiType()?.GetMethod(
            "FocusScreenScope",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string) },
            null);
    }

    private static MethodInfo? ResolveIsScreenScopeOpen()
    {
        return s_isScreenScopeOpen ??= ResolveApiType()?.GetMethod(
            "IsScreenScopeOpen",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string) },
            null);
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

    private static MethodInfo? ResolveGetStyles()
    {
        return s_getStyles ??= ResolveApiType()?.GetMethod(
            "GetStyles",
            BindingFlags.Public | BindingFlags.Static,
            null,
            Type.EmptyTypes,
            null);
    }

    private static MethodInfo? ResolveGetUiTexture()
    {
        return s_getUiTexture ??= ResolveApiType()?.GetMethod(
            "GetUiTexture",
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
    }

    internal sealed class SharedStyles
    {
        internal SharedStyles(
            GUIStyle window,
            GUIStyle header,
            GUIStyle panel,
            GUIStyle card,
            GUIStyle button,
            GUIStyle secondaryButton,
            GUIStyle title,
            GUIStyle label,
            GUIStyle mutedLabel,
            Texture2D? dividerTexture,
            Texture2D? progressTrackTexture,
            Texture2D? progressFillTexture)
        {
            Window = window;
            Header = header;
            Panel = panel;
            Card = card;
            Button = button;
            SecondaryButton = secondaryButton;
            Title = title;
            Label = label;
            MutedLabel = mutedLabel;
            DividerTexture = dividerTexture;
            ProgressTrackTexture = progressTrackTexture;
            ProgressFillTexture = progressFillTexture;
        }

        internal GUIStyle Window { get; }
        internal GUIStyle Header { get; }
        internal GUIStyle Panel { get; }
        internal GUIStyle Card { get; }
        internal GUIStyle Button { get; }
        internal GUIStyle SecondaryButton { get; }
        internal GUIStyle Title { get; }
        internal GUIStyle Label { get; }
        internal GUIStyle MutedLabel { get; }
        internal Texture2D? DividerTexture { get; }
        internal Texture2D? ProgressTrackTexture { get; }
        internal Texture2D? ProgressFillTexture { get; }
    }
}
