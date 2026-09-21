using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AvalonHumanCompanions;

internal static class TaintedInterfaceBridge
{
    private const string ApiTypeName = "TaintedInterface.TaintedInterfaceApi";

    private static Type? s_apiType;
    private static MethodInfo? s_beginCustomUiScope;
    private static MethodInfo? s_endCustomUiScope;
    private static MethodInfo? s_ensureInteractiveCursor;
    private static MethodInfo? s_getStyles;
    private static MethodInfo? s_getFullEmbeddedAssetBytes;
    private static Type? s_embeddedAssetGroupType;
    private static object? s_iconAssetGroupValue;
    private static object? s_iconAndDialogueBackgroundAssetGroupValue;
    private static PropertyInfo? s_isAvailable;
    private static readonly Dictionary<string, Texture2D> s_fullEmbeddedIconCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Texture2D> s_fullEmbeddedIconAndDialogueBackgroundCache = new(StringComparer.OrdinalIgnoreCase);
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

    internal static Texture2D? GetFullEmbeddedIcon(string relativePath)
    {
        return GetFullEmbeddedTexture(
            relativePath,
            s_fullEmbeddedIconCache,
            ResolveIconAssetGroupValue,
            "AvalonHumanCompanionIcon.",
            "read Tainted Interface full embedded icon");
    }

    internal static Texture2D? GetFullEmbeddedIconAndDialogueBackground(string relativePath)
    {
        return GetFullEmbeddedTexture(
            relativePath,
            s_fullEmbeddedIconAndDialogueBackgroundCache,
            ResolveIconAndDialogueBackgroundAssetGroupValue,
            "AvalonHumanCompanionIconBackground.",
            "read Tainted Interface full embedded icon background");
    }

    private static Texture2D? GetFullEmbeddedTexture(
        string relativePath,
        Dictionary<string, Texture2D> cache,
        Func<object?> resolveGroup,
        string textureNamePrefix,
        string logAction)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || !IsAvailable())
        {
            return null;
        }

        string key = relativePath.Trim();
        if (cache.TryGetValue(key, out Texture2D cached) && cached != null)
        {
            return cached;
        }

        byte[]? bytes = GetFullEmbeddedTextureBytes(key, resolveGroup, logAction);
        if (bytes == null || bytes.Length == 0)
        {
            return null;
        }

        Texture2D texture = new(2, 2, TextureFormat.RGBA32, false)
        {
            name = textureNamePrefix + key,
            hideFlags = HideFlags.DontUnloadUnusedAsset,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        if (!ImageConversion.LoadImage(texture, bytes, markNonReadable: false))
        {
            UnityEngine.Object.Destroy(texture);
            return null;
        }

        cache[key] = texture;
        return texture;
    }

    private static byte[]? GetFullEmbeddedTextureBytes(string relativePath, Func<object?> resolveGroup, string logAction)
    {
        MethodInfo? method = ResolveGetFullEmbeddedAssetBytes();
        object? group = resolveGroup();
        if (method == null || group == null)
        {
            return null;
        }

        try
        {
            return method.Invoke(null, new object[] { group, relativePath }) as byte[];
        }
        catch (Exception ex)
        {
            LogInvokeFailure(logAction, ex);
            return null;
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

    private static MethodInfo? ResolveGetFullEmbeddedAssetBytes()
    {
        Type? groupType = ResolveEmbeddedAssetGroupType();
        return groupType == null
            ? null
            : s_getFullEmbeddedAssetBytes ??= ResolveApiType()?.GetMethod(
                "GetFullEmbeddedAssetBytes",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { groupType, typeof(string) },
                null);
    }

    private static Type? ResolveEmbeddedAssetGroupType()
    {
        return s_embeddedAssetGroupType ??= ResolveApiType()?.Assembly.GetType(
            "TaintedInterface.TaintedInterfaceEmbeddedAssetGroup",
            throwOnError: false);
    }

    private static object? ResolveIconAssetGroupValue()
    {
        return ResolveAssetGroupValue("Icons", ref s_iconAssetGroupValue, "resolve Tainted Interface embedded icon group");
    }

    private static object? ResolveIconAndDialogueBackgroundAssetGroupValue()
    {
        return ResolveAssetGroupValue(
            "IconAndDialogueBackgrounds",
            ref s_iconAndDialogueBackgroundAssetGroupValue,
            "resolve Tainted Interface embedded icon background group");
    }

    private static object? ResolveAssetGroupValue(string groupName, ref object? cachedValue, string logAction)
    {
        if (cachedValue != null)
        {
            return cachedValue;
        }

        Type? groupType = ResolveEmbeddedAssetGroupType();
        if (groupType == null)
        {
            return null;
        }

        try
        {
            cachedValue = Enum.Parse(groupType, groupName);
            return cachedValue;
        }
        catch (Exception ex)
        {
            LogInvokeFailure(logAction, ex);
            return null;
        }
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
        Debug.LogWarning($"Avalon Human Companions could not {action}: {exception.GetType().Name}: {exception.Message}");
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
