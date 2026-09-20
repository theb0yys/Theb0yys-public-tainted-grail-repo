using System;
using System.Reflection;
using BepInEx.Logging;
using UnityEngine;

namespace TaintedMusic;

internal static class TaintedInterfaceBridge
{
    private const string ApiTypeName = "TaintedInterface.TaintedInterfaceApi";

    private static Type? s_apiType;
    private static MethodInfo? s_beginCustomUiScope;
    private static MethodInfo? s_endCustomUiScope;
    private static MethodInfo? s_ensureInteractiveCursor;
    private static MethodInfo? s_getStyles;
    private static PropertyInfo? s_isAvailable;
    private static ManualLogSource? s_logger;
    private static bool s_loggedFailure;

    internal static void SetLogger(ManualLogSource logger)
    {
        s_logger = logger;
    }

    internal static bool IsAvailable()
    {
        PropertyInfo? property = s_isAvailable ??= ResolveApiType()?.GetProperty(
            "IsAvailable",
            BindingFlags.Public | BindingFlags.Static);
        try
        {
            return property?.GetValue(null) is bool available && available;
        }
        catch (Exception ex)
        {
            LogFailure("read Tainted Interface availability", ex);
            return false;
        }
    }

    internal static bool BeginCustomUiScope(string ownerId, bool freezeWorld)
    {
        MethodInfo? method = s_beginCustomUiScope ??= ResolveApiType()?.GetMethod(
            "BeginCustomUiScope",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string), typeof(bool) },
            null);
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
            LogFailure("begin Tainted Interface custom UI scope", ex);
            return false;
        }
    }

    internal static void EndCustomUiScope(string ownerId)
    {
        MethodInfo? method = s_endCustomUiScope ??= ResolveApiType()?.GetMethod(
            "EndCustomUiScope",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string) },
            null);
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
            LogFailure("end Tainted Interface custom UI scope", ex);
        }
    }

    internal static void EnsureInteractiveCursor()
    {
        MethodInfo? method = s_ensureInteractiveCursor ??= ResolveApiType()?.GetMethod(
            "EnsureInteractiveCursor",
            BindingFlags.Public | BindingFlags.Static,
            null,
            Type.EmptyTypes,
            null);
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
            LogFailure("restore the Tainted Interface cursor", ex);
        }
    }

    internal static bool TryGetStyles(out SharedStyles? sharedStyles)
    {
        sharedStyles = null;
        if (!IsAvailable())
        {
            return false;
        }

        MethodInfo? method = s_getStyles ??= ResolveApiType()?.GetMethod(
            "GetStyles",
            BindingFlags.Public | BindingFlags.Static,
            null,
            Type.EmptyTypes,
            null);
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
            if (window == null || header == null || panel == null || card == null || button == null ||
                secondaryButton == null || title == null || label == null || mutedLabel == null)
            {
                return false;
            }

            sharedStyles = new SharedStyles(
                window,
                header,
                panel,
                card,
                button,
                secondaryButton,
                title,
                label,
                mutedLabel);
            return true;
        }
        catch (Exception ex)
        {
            LogFailure("read Tainted Interface styles", ex);
            return false;
        }
    }

    private static GUIStyle? ReadStyle(object styles, string propertyName)
    {
        return styles.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(styles) as GUIStyle;
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

        return null;
    }

    private static void LogFailure(string action, Exception exception)
    {
        if (s_loggedFailure)
        {
            return;
        }

        s_loggedFailure = true;
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
            GUIStyle mutedLabel)
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
    }
}
