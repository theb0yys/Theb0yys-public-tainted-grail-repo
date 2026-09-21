using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace AvalonBroodmotherCompanion;

internal enum BroodmotherIconVariant
{
    CurrentRedBlack,
    ReservedPale,
    ReservedGold,
    SpiderSmallSkin1RedBlack,
    SpiderSmallSkin2Pale,
    SpiderSmallSkin3Gold,
}

internal static class BroodmotherEmbeddedIconRuntime
{
    private const int MaximumIconResourceBytes = 2 * 1024 * 1024;
    private const string ResourcePrefix = "AvalonBroodmotherCompanion.Icons.";

    private static readonly Dictionary<BroodmotherIconVariant, Texture2D> LoadedTextures = new();
    private static readonly HashSet<string> LoggedFailures = new(StringComparer.Ordinal);
    private static MethodInfo? s_loadImageMethod;

    internal static Texture2D? GetCurrentIconTexture(out string source)
    {
        return GetIconTexture(BroodmotherIconVariant.CurrentRedBlack, out source);
    }

    internal static Texture2D? GetIconTexture(BroodmotherIconVariant variant, out string source)
    {
        string fileName = GetFileName(variant);
        source = "embedded:" + fileName;
        if (LoadedTextures.TryGetValue(variant, out Texture2D texture) && texture != null)
        {
            return texture;
        }

        try
        {
            Texture2D loaded = LoadTexture(fileName);
            LoadedTextures[variant] = loaded;
            return loaded;
        }
        catch (Exception ex)
        {
            if (LoggedFailures.Add(fileName))
            {
                Debug.LogWarning($"{Plugin.PluginName} embedded spider-family icon unavailable: {fileName}; error={ex.GetType().Name}: {ex.Message}");
            }

            source = string.Empty;
            return null;
        }
    }

    internal static void Release()
    {
        foreach (Texture2D texture in LoadedTextures.Values)
        {
            if (texture != null)
            {
                UnityEngine.Object.Destroy(texture);
            }
        }

        LoadedTextures.Clear();
        LoggedFailures.Clear();
    }

    private static Texture2D LoadTexture(string fileName)
    {
        Assembly assembly = typeof(BroodmotherEmbeddedIconRuntime).Assembly;
        string logicalName = ResourcePrefix + fileName;
        using Stream? stream = assembly.GetManifestResourceStream(logicalName);
        if (stream == null)
        {
            throw new FileNotFoundException("Embedded spider-family icon resource missing.", logicalName);
        }

        byte[] bytes = ReadStreamBounded(stream, MaximumIconResourceBytes, logicalName);
        Texture2D sourceTexture = new(2, 2, TextureFormat.RGBA32, false)
        {
            name = "AvalonBroodmotherCompanion.Icon.Source." + Path.GetFileNameWithoutExtension(fileName),
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
        };

        bool loaded = InvokeLoadImage(sourceTexture, bytes);
        if (!loaded || sourceTexture.width <= 0 || sourceTexture.height <= 0)
        {
            UnityEngine.Object.Destroy(sourceTexture);
            throw new InvalidDataException("Embedded spider-family icon image decode returned no pixels.");
        }

        Texture2D iconTexture = CreateSquareCenterCrop(sourceTexture, fileName);
        UnityEngine.Object.Destroy(sourceTexture);
        return iconTexture;
    }

    private static Texture2D CreateSquareCenterCrop(Texture2D sourceTexture, string fileName)
    {
        int sourceWidth = sourceTexture.width;
        int sourceHeight = sourceTexture.height;
        int squareSize = Math.Min(sourceWidth, sourceHeight);
        int offsetX = Math.Max(0, (sourceWidth - squareSize) / 2);
        int offsetY = Math.Max(0, (sourceHeight - squareSize) / 2);
        Color32[] sourcePixels = sourceTexture.GetPixels32();
        Color32[] croppedPixels = new Color32[squareSize * squareSize];

        for (int y = 0; y < squareSize; y++)
        {
            Array.Copy(
                sourcePixels,
                (offsetY + y) * sourceWidth + offsetX,
                croppedPixels,
                y * squareSize,
                squareSize);
        }

        Texture2D cropped = new(squareSize, squareSize, TextureFormat.RGBA32, false)
        {
            name = "AvalonBroodmotherCompanion.Icon." + Path.GetFileNameWithoutExtension(fileName),
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
        };
        cropped.SetPixels32(croppedPixels);
        cropped.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        return cropped;
    }

    private static bool InvokeLoadImage(Texture2D texture, byte[] bytes)
    {
        MethodInfo method = ResolveLoadImageMethod();
        object? result = method.Invoke(null, new object[] { texture, bytes, false });
        return result is bool loaded ? loaded : texture.width > 0 && texture.height > 0;
    }

    private static MethodInfo ResolveLoadImageMethod()
    {
        if (s_loadImageMethod != null)
        {
            return s_loadImageMethod;
        }

        Type? imageConversionType = Type.GetType("UnityEngine.ImageConversion, UnityEngine.ImageConversionModule", throwOnError: false);
        if (imageConversionType == null)
        {
            try
            {
                Assembly.Load("UnityEngine.ImageConversionModule");
            }
            catch
            {
                // The loaded-assembly scan below keeps this soft when the module is already present under Unity's loader.
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                imageConversionType = assembly.GetType("UnityEngine.ImageConversion", throwOnError: false);
                if (imageConversionType != null)
                {
                    break;
                }
            }
        }

        MethodInfo? method = imageConversionType?.GetMethod(
            "LoadImage",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(Texture2D), typeof(byte[]), typeof(bool) },
            null);
        if (method == null)
        {
            throw new MissingMethodException("UnityEngine.ImageConversion.LoadImage(Texture2D, byte[], bool)");
        }

        s_loadImageMethod = method;
        return method;
    }

    private static byte[] ReadStreamBounded(Stream stream, int maximumBytes, string logicalName)
    {
        if (stream.Length > maximumBytes)
        {
            throw new InvalidDataException($"Embedded icon resource '{logicalName}' exceeds {maximumBytes} bytes.");
        }

        using MemoryStream memory = new();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    private static string GetFileName(BroodmotherIconVariant variant)
    {
        return variant switch
        {
            BroodmotherIconVariant.CurrentRedBlack => "broodmother-current-redblack.jpg",
            BroodmotherIconVariant.ReservedPale => "broodmother-reserved-pale.jpg",
            BroodmotherIconVariant.ReservedGold => "broodmother-reserved-gold.jpg",
            BroodmotherIconVariant.SpiderSmallSkin1RedBlack => "spider-small-skin1-redblack.jpg",
            BroodmotherIconVariant.SpiderSmallSkin2Pale => "spider-small-skin2-pale.jpg",
            BroodmotherIconVariant.SpiderSmallSkin3Gold => "spider-small-skin3-gold.jpg",
            _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null),
        };
    }
}
