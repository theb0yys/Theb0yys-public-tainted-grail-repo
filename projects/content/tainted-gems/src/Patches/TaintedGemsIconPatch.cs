using System;
using System.Linq;
using System.Reflection;
using Awaken.TG.Assets;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UIElements;
using UnityImage = UnityEngine.UI.Image;

namespace TaintedGems.Patches;

internal static class TaintedGemsIconPatch
{
    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        int shareablePatched = 0;
        foreach (MethodInfo method in typeof(ShareableSpriteReference)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(candidate => string.Equals(candidate.Name, "RegisterAndSetup", StringComparison.Ordinal)))
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length < 2)
            {
                continue;
            }

            MethodInfo? prefix = null;
            if (parameters[1].ParameterType == typeof(UnityImage))
            {
                prefix = AccessTools.Method(typeof(TaintedGemsIconPatch), nameof(RegisterAndSetupImagePrefix));
            }
            else if (parameters[1].ParameterType == typeof(VisualElement))
            {
                prefix = AccessTools.Method(typeof(TaintedGemsIconPatch), nameof(RegisterAndSetupVisualElementPrefix));
            }

            if (prefix == null)
            {
                continue;
            }

            harmony.Patch(method, prefix: new HarmonyMethod(prefix));
            shareablePatched++;
        }

        int directSetSpritePatched = 0;
        foreach (MethodInfo method in typeof(SpriteReference)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(candidate => string.Equals(candidate.Name, "SetSprite", StringComparison.Ordinal)))
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length < 1)
            {
                continue;
            }

            MethodInfo? prefix = null;
            if (parameters[0].ParameterType == typeof(UnityImage))
            {
                prefix = AccessTools.Method(typeof(TaintedGemsIconPatch), nameof(SetSpriteImagePrefix));
            }
            else if (parameters[0].ParameterType == typeof(VisualElement))
            {
                prefix = AccessTools.Method(typeof(TaintedGemsIconPatch), nameof(SetSpriteVisualElementPrefix));
            }

            if (prefix == null)
            {
                continue;
            }

            harmony.Patch(method, prefix: new HarmonyMethod(prefix));
            directSetSpritePatched++;
        }

        int releasePatched = 0;
        MethodInfo? release = AccessTools.Method(typeof(SpriteReference), nameof(SpriteReference.Release));
        MethodInfo? releasePrefix = AccessTools.Method(typeof(TaintedGemsIconPatch), nameof(ReleasePrefix));
        if (release != null && releasePrefix != null)
        {
            harmony.Patch(release, prefix: new HarmonyMethod(releasePrefix));
            releasePatched++;
        }

        logger.LogInfo($"Tainted Gems custom icon patch armed; shareableSpriteReferenceOverloads={shareablePatched}; spriteReferenceSetSpriteOverloads={directSetSpritePatched}; spriteReferenceReleaseOverloads={releasePatched}; route=rendered-imported-gem-prefab-sprites.");
    }

    private static bool RegisterAndSetupImagePrefix(
        ShareableSpriteReference __instance,
        IReleasableOwner owner,
        UnityImage image,
        Action<UnityImage, Sprite>? afterAssign)
    {
        if (!Plugin.IsCustomIconReference(__instance))
        {
            return true;
        }

        if (Plugin.Instance == null ||
            !Plugin.Instance.TryGetCustomIconSprite(__instance.AssetGUID, out Sprite? sprite) ||
            sprite == null)
        {
            return true;
        }

        image.sprite = sprite;
        image.enabled = true;
        image.color = Color.white;
        afterAssign?.Invoke(image, sprite);
        return false;
    }

    private static bool RegisterAndSetupVisualElementPrefix(
        ShareableSpriteReference __instance,
        IReleasableOwner owner,
        VisualElement image,
        Action<VisualElement, Sprite>? afterAssign)
    {
        if (!Plugin.IsCustomIconReference(__instance))
        {
            return true;
        }

        if (Plugin.Instance == null ||
            !Plugin.Instance.TryGetCustomIconSprite(__instance.AssetGUID, out Sprite? sprite) ||
            sprite == null)
        {
            return true;
        }

        image.style.backgroundImage = new StyleBackground(sprite);
        afterAssign?.Invoke(image, sprite);
        return false;
    }

    private static bool SetSpriteImagePrefix(
        SpriteReference __instance,
        UnityImage image,
        Action<UnityImage, Sprite>? afterAssign)
    {
        if (!TryGetCustomIconSprite(__instance, out Sprite? sprite) || sprite == null)
        {
            return true;
        }

        image.sprite = sprite;
        image.enabled = true;
        image.color = Color.white;
        afterAssign?.Invoke(image, sprite);
        return false;
    }

    private static bool SetSpriteVisualElementPrefix(
        SpriteReference __instance,
        VisualElement image,
        Action<VisualElement, Sprite>? afterAssign)
    {
        if (!TryGetCustomIconSprite(__instance, out Sprite? sprite) || sprite == null)
        {
            return true;
        }

        image.style.backgroundImage = new StyleBackground(sprite);
        afterAssign?.Invoke(image, sprite);
        return false;
    }

    private static bool ReleasePrefix(SpriteReference __instance)
    {
        return !Plugin.IsCustomIconAddress(__instance.arSpriteReference?.Address);
    }

    private static bool TryGetCustomIconSprite(SpriteReference reference, out Sprite? sprite)
    {
        sprite = null;
        string? iconAddress = reference.arSpriteReference?.Address;
        if (!Plugin.IsCustomIconAddress(iconAddress))
        {
            return false;
        }

        return Plugin.Instance != null &&
            Plugin.Instance.TryGetCustomIconSprite(iconAddress!, out sprite) &&
            sprite != null;
    }
}
