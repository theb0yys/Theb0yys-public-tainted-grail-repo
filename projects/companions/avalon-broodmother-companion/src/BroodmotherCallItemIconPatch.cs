using System;
using System.Collections.Generic;
using System.Reflection;
using Awaken.TG.MVC;
using Awaken.TG.Main.Heroes.CharacterSheet.Items.Panel.Slot;
using Awaken.TG.Main.Heroes.Items;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace AvalonBroodmotherCompanion;

internal static class BroodmotherCallItemIconPatch
{
    private const int IconSize = 128;

    private static readonly MethodInfo? SetInternalVisibilityMethod = AccessTools.Method(
        typeof(ItemIconComponent).BaseType?.BaseType,
        "SetInternalVisibility");
    private static readonly HashSet<string> LoggedApplications = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Sprite> IconSprites = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, Texture2D> IconSpriteTextures = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> IconSpriteSources = new(StringComparer.Ordinal);

    private static Texture2D? s_iconTexture;

    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? refresh = AccessTools.Method(
            typeof(ItemIconComponent),
            "Refresh",
            new[] { typeof(Item), typeof(View), typeof(ItemDescriptorType) });
        MethodInfo? prefix = AccessTools.Method(typeof(BroodmotherCallItemIconPatch), nameof(Prefix));

        if (refresh == null || prefix == null)
        {
            logger.LogWarning($"{Plugin.PluginName}: Broodmother's Call item icon patch unavailable; reason=item-icon-refresh-target-missing.");
            return;
        }

        harmony.Patch(refresh, prefix: new HarmonyMethod(prefix));
        logger.LogInfo($"{Plugin.PluginName}: patched ItemIconComponent.Refresh for Broodmother's Call embedded item icon.");
    }

    private static bool Prefix(
        Item item,
        ItemIconComponent __instance,
        Image ___icon,
        GameObject ___lockIcon,
        GameObject ___favouriteIcon)
    {
        if (!TryGetSpiderFamilyCall(item, out SpiderFamilyCompanionDefinition definition))
        {
            return true;
        }

        if (___icon == null || SetInternalVisibilityMethod == null)
        {
            return true;
        }

        try
        {
            Sprite sprite = GetOrCreateIconSprite(definition, out string iconSource);
            ___lockIcon?.SetActive(item.Locked);
            ___favouriteIcon?.SetActive(item.Favourite);
            ___icon.sprite = sprite;
            ___icon.color = Color.white;
            ___icon.preserveAspect = true;
            ___icon.enabled = true;
            SetInternalVisibilityMethod.Invoke(__instance, new object[] { true });

            string itemGuid = item.Template?.GUID ?? string.Empty;
            if (LoggedApplications.Add(itemGuid))
            {
                Plugin.Instance?.ModLogger.LogInfo($"{Plugin.PluginName}: spider-family call item icon applied; item={itemGuid}; variant={definition.Id}; source={iconSource}.");
            }

            return false;
        }
        catch (Exception ex)
        {
            Plugin.Instance?.ModLogger.LogWarning($"{Plugin.PluginName}: Broodmother's Call embedded item icon failed; fallback=native; error={ex.GetType().Name}: {ex.Message}");
            return true;
        }
    }

    internal static void Release()
    {
        foreach (Sprite sprite in IconSprites.Values)
        {
            if (sprite != null)
            {
                UnityEngine.Object.Destroy(sprite);
            }
        }

        IconSprites.Clear();
        IconSpriteTextures.Clear();
        IconSpriteSources.Clear();
        if (s_iconTexture != null)
        {
            UnityEngine.Object.Destroy(s_iconTexture);
            s_iconTexture = null;
        }

        BroodmotherEmbeddedIconRuntime.Release();
        LoggedApplications.Clear();
    }

    private static bool TryGetSpiderFamilyCall(
        Item? item,
        out SpiderFamilyCompanionDefinition definition)
    {
        return SpiderFamilyCompanionDefinitions.TryGetBySpellTemplateGuid(
            item?.Template?.GUID,
            out definition);
    }

    private static Sprite GetOrCreateIconSprite(
        SpiderFamilyCompanionDefinition definition,
        out string source)
    {
        Texture2D texture = TryGetDefinitionIconTexture(definition, out source) ??
            TryGetTaintedInterfaceIconTexture(definition, out source) ??
            GetOrCreateIconTexture();
        if (ReferenceEquals(texture, s_iconTexture))
        {
            source = "plugin-owned-procedural-texture";
        }

        string definitionId = definition.Id;
        if (IconSprites.TryGetValue(definitionId, out Sprite existingSprite) &&
            existingSprite != null &&
            IconSpriteTextures.TryGetValue(definitionId, out Texture2D existingTexture) &&
            ReferenceEquals(existingTexture, texture))
        {
            if (IconSpriteSources.TryGetValue(definitionId, out string existingSource))
            {
                source = existingSource;
            }

            return existingSprite;
        }

        if (existingSprite != null)
        {
            UnityEngine.Object.Destroy(existingSprite);
        }

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);
        sprite.name = "AvalonBroodmotherCompanion.BroodmotherCall.Icon." + definitionId;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        IconSprites[definitionId] = sprite;
        IconSpriteTextures[definitionId] = texture;
        IconSpriteSources[definitionId] = source;
        return sprite;
    }

    private static Texture2D? TryGetDefinitionIconTexture(
        SpiderFamilyCompanionDefinition definition,
        out string source)
    {
        if (definition.IconVariant.HasValue)
        {
            return BroodmotherEmbeddedIconRuntime.GetIconTexture(definition.IconVariant.Value, out source);
        }

        source = string.Empty;
        return null;
    }

    private static Texture2D? TryGetTaintedInterfaceIconTexture(
        SpiderFamilyCompanionDefinition definition,
        out string source)
    {
        Texture2D? itemIcon = TaintedInterfaceReflection.GetItemIcon(definition.SpellTemplateGuid);
        if (itemIcon != null)
        {
            source = "TaintedInterface.GetItemIcon(" + definition.SpellTemplateGuid + ")";
            return itemIcon;
        }

        Texture2D? companionIcon = TaintedInterfaceReflection.GetIcon(Plugin.CompanionIconId);
        if (companionIcon != null)
        {
            source = "TaintedInterface.GetIcon(" + Plugin.CompanionIconId + ")";
            return companionIcon;
        }

        source = string.Empty;
        return null;
    }

    private static Texture2D GetOrCreateIconTexture()
    {
        if (s_iconTexture != null)
        {
            return s_iconTexture;
        }

        Color32[] pixels = new Color32[IconSize * IconSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color32(0, 0, 0, 0);
        }

        Color32 shadow = new(7, 4, 3, 220);
        Color32 plate = new(30, 23, 16, 185);
        Color32 web = new(134, 92, 41, 120);
        Color32 goldDark = new(111, 66, 22, 255);
        Color32 gold = new(223, 162, 57, 255);
        Color32 goldLight = new(255, 215, 118, 255);
        Color32 red = new(173, 17, 14, 255);
        Color32 ember = new(245, 78, 36, 210);

        FillDisc(pixels, 64, 64, 54, plate);
        DrawCircle(pixels, 64, 64, 55, goldDark, 3);
        DrawCircle(pixels, 64, 64, 47, web, 1);
        DrawCircle(pixels, 64, 64, 35, web, 1);
        DrawLine(pixels, 64, 12, 64, 116, web, 1);
        DrawLine(pixels, 12, 64, 116, 64, web, 1);
        DrawLine(pixels, 27, 27, 101, 101, web, 1);
        DrawLine(pixels, 101, 27, 27, 101, web, 1);

        DrawLeg(pixels, 53, 50, 29, 39, 15, 24, shadow, gold);
        DrawLeg(pixels, 51, 58, 23, 56, 10, 53, shadow, gold);
        DrawLeg(pixels, 51, 69, 25, 77, 12, 87, shadow, gold);
        DrawLeg(pixels, 55, 78, 35, 98, 27, 112, shadow, gold);
        DrawLeg(pixels, 75, 50, 99, 39, 113, 24, shadow, gold);
        DrawLeg(pixels, 77, 58, 105, 56, 118, 53, shadow, gold);
        DrawLeg(pixels, 77, 69, 103, 77, 116, 87, shadow, gold);
        DrawLeg(pixels, 73, 78, 93, 98, 101, 112, shadow, gold);

        FillEllipse(pixels, 64, 73, 21, 30, shadow);
        FillEllipse(pixels, 64, 73, 17, 25, goldDark);
        FillEllipse(pixels, 64, 51, 16, 18, shadow);
        FillEllipse(pixels, 64, 51, 13, 15, gold);
        FillEllipse(pixels, 64, 35, 11, 12, shadow);
        FillEllipse(pixels, 64, 35, 9, 10, goldLight);
        FillEllipse(pixels, 59, 33, 3, 4, red);
        FillEllipse(pixels, 69, 33, 3, 4, red);
        DrawLine(pixels, 60, 96, 52, 111, ember, 2);
        DrawLine(pixels, 68, 96, 76, 111, ember, 2);
        DrawLine(pixels, 61, 46, 56, 58, goldLight, 1);
        DrawLine(pixels, 67, 46, 72, 58, goldLight, 1);

        s_iconTexture = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false)
        {
            name = "AvalonBroodmotherCompanion.BroodmotherCall.IconTexture",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
        };
        s_iconTexture.SetPixels32(pixels);
        s_iconTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
        return s_iconTexture;
    }

    private static void DrawLeg(Color32[] pixels, int x0, int y0, int x1, int y1, int x2, int y2, Color32 outline, Color32 fill)
    {
        DrawLine(pixels, x0, y0, x1, y1, outline, 5);
        DrawLine(pixels, x1, y1, x2, y2, outline, 5);
        DrawLine(pixels, x0, y0, x1, y1, fill, 3);
        DrawLine(pixels, x1, y1, x2, y2, fill, 3);
    }

    private static void DrawCircle(Color32[] pixels, int cx, int cy, int radius, Color32 color, int thickness)
    {
        for (int angle = 0; angle < 360; angle++)
        {
            double radians = angle * Math.PI / 180.0;
            int x = cx + (int)Math.Round(Math.Cos(radians) * radius);
            int y = cy + (int)Math.Round(Math.Sin(radians) * radius);
            FillDisc(pixels, x, y, thickness, color);
        }
    }

    private static void DrawLine(Color32[] pixels, int x0, int y0, int x1, int y1, Color32 color, int thickness)
    {
        int dx = x1 - x0;
        int dy = y1 - y0;
        int steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
        if (steps == 0)
        {
            FillDisc(pixels, x0, y0, thickness, color);
            return;
        }

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            int x = (int)Math.Round(x0 + dx * t);
            int y = (int)Math.Round(y0 + dy * t);
            FillDisc(pixels, x, y, thickness, color);
        }
    }

    private static void FillEllipse(Color32[] pixels, int cx, int cy, int rx, int ry, Color32 color)
    {
        for (int y = cy - ry; y <= cy + ry; y++)
        {
            for (int x = cx - rx; x <= cx + rx; x++)
            {
                double nx = (double)(x - cx) / rx;
                double ny = (double)(y - cy) / ry;
                if (nx * nx + ny * ny <= 1.0)
                {
                    BlendPixel(pixels, x, y, color);
                }
            }
        }
    }

    private static void FillDisc(Color32[] pixels, int cx, int cy, int radius, Color32 color)
    {
        int radiusSquared = radius * radius;
        for (int y = cy - radius; y <= cy + radius; y++)
        {
            for (int x = cx - radius; x <= cx + radius; x++)
            {
                int dx = x - cx;
                int dy = y - cy;
                if (dx * dx + dy * dy <= radiusSquared)
                {
                    BlendPixel(pixels, x, y, color);
                }
            }
        }
    }

    private static void BlendPixel(Color32[] pixels, int x, int y, Color32 source)
    {
        if (x < 0 || x >= IconSize || y < 0 || y >= IconSize || source.a == 0)
        {
            return;
        }

        int index = y * IconSize + x;
        Color32 destination = pixels[index];
        if (source.a == 255 || destination.a == 0)
        {
            pixels[index] = source;
            return;
        }

        float sourceAlpha = source.a / 255f;
        float destinationAlpha = destination.a / 255f;
        float outputAlpha = sourceAlpha + destinationAlpha * (1f - sourceAlpha);
        if (outputAlpha <= 0f)
        {
            pixels[index] = new Color32(0, 0, 0, 0);
            return;
        }

        byte r = (byte)Mathf.RoundToInt((source.r * sourceAlpha + destination.r * destinationAlpha * (1f - sourceAlpha)) / outputAlpha);
        byte g = (byte)Mathf.RoundToInt((source.g * sourceAlpha + destination.g * destinationAlpha * (1f - sourceAlpha)) / outputAlpha);
        byte b = (byte)Mathf.RoundToInt((source.b * sourceAlpha + destination.b * destinationAlpha * (1f - sourceAlpha)) / outputAlpha);
        byte a = (byte)Mathf.RoundToInt(outputAlpha * 255f);
        pixels[index] = new Color32(r, g, b, a);
    }
}
