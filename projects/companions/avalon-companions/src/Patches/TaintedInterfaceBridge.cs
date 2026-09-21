using System;
using System.Reflection;
using UnityEngine;

namespace AvalonCompanions.Patches;

internal static class TaintedInterfaceBridge
{
    private const string ApiTypeName = "TaintedInterface.TaintedInterfaceApi";

    private static Type? s_apiType;
    private static MethodInfo? s_beginCustomUiScope;
    private static MethodInfo? s_endCustomUiScope;
    private static MethodInfo? s_ensureInteractiveCursor;
    private static MethodInfo? s_getStyles;
    private static MethodInfo? s_getIcon;
    private static MethodInfo? s_drawHudBadge;
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

    internal static bool DrawHudBadge(string corner, float size, string iconId, string backgroundIconId)
    {
        if (string.IsNullOrWhiteSpace(iconId) || !IsAvailable())
        {
            return false;
        }

        MethodInfo? method = ResolveDrawHudBadge();
        if (method == null)
        {
            return false;
        }

        try
        {
            return method.Invoke(null, new object[] { corner, size, iconId, backgroundIconId }) is bool drawn && drawn;
        }
        catch (Exception ex)
        {
            LogInvokeFailure("draw Tainted Interface HUD badge", ex);
            return false;
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
            GUIStyle? field = ReadStyle(styles, "Field");
            GUIStyle? title = ReadStyle(styles, "Title");
            GUIStyle? label = ReadStyle(styles, "Label");
            GUIStyle? mutedLabel = ReadStyle(styles, "MutedLabel");

            if (window == null || panel == null || button == null || field == null || title == null || label == null || mutedLabel == null)
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
                field,
                title,
                label,
                mutedLabel);
            return true;
        }
        catch (Exception ex)
        {
            LogInvokeFailure("read Tainted Interface styles", ex);
            return false;
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

    private static MethodInfo? ResolveGetIcon()
    {
        return s_getIcon ??= ResolveApiType()?.GetMethod(
            "GetIcon",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string) },
            null);
    }

    private static MethodInfo? ResolveDrawHudBadge()
    {
        return s_drawHudBadge ??= ResolveApiType()?.GetMethod(
            "DrawHudBadge",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string), typeof(float), typeof(string), typeof(string) },
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
        Debug.LogWarning($"Avalon Companions could not {action}: {exception.GetType().Name}: {exception.Message}");
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
            GUIStyle field,
            GUIStyle title,
            GUIStyle label,
            GUIStyle mutedLabel)
        {
            Window = window;
            Header = header;
            Panel = panel;
            Card = card;
            Button = button;
            SecondaryButton = secondaryButton;
            Field = field;
            Title = title;
            Label = label;
            MutedLabel = mutedLabel;
        }

        internal GUIStyle Window { get; }
        internal GUIStyle Header { get; }
        internal GUIStyle Panel { get; }
        internal GUIStyle Card { get; }
        internal GUIStyle Button { get; }
        internal GUIStyle SecondaryButton { get; }
        internal GUIStyle Field { get; }
        internal GUIStyle Title { get; }
        internal GUIStyle Label { get; }
        internal GUIStyle MutedLabel { get; }
    }
}
