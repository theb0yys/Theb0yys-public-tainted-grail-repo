using System;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Items;
using Awaken.TG.Main.Heroes.Statuses;
using Awaken.TG.Main.Skills;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TGExample.StatusApplicationObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.status-application-observer";
    public const string PluginName = "TG Example - Status Application Observer";
    public const string PluginVersion = "0.1.0";

    internal static ManualLogSource? LogSource { get; private set; }

    private Harmony? _harmony;

    private void Awake()
    {
        LogSource = Logger;
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded. This example observes statuses only.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        LogSource = null;
    }
}

[HarmonyPatch(typeof(CharacterStatuses), nameof(CharacterStatuses.AddStatus))]
internal static class StatusApplicationPatch
{
    [ThreadStatic]
    private static int _addStatusDepth;

    private static void Prefix()
    {
        _addStatusDepth++;
    }

    private static void Postfix(
        CharacterStatuses __instance,
        StatusTemplate statusTemplate,
        StatusSourceInfo sourceInfo,
        CharacterStatuses.AddResult __result)
    {
        try
        {
            if (_addStatusDepth != 1 || !IsMeaningfulAdd(__result.type))
            {
                return;
            }

            Status? status = __result.newStatus ?? __result.oldStatus;
            bool positive = IsPositive(status, statusTemplate);

            Plugin.LogSource?.LogInfo(
                $"Status application: addType={__result.type}; polarity={(positive ? "positive" : "negative")}; " +
                $"status={DescribeStatus(status, statusTemplate)}; " +
                $"sourceCharacter={DescribeSourceCharacter(sourceInfo)}; " +
                $"sourceItem={DescribeSourceItem(sourceInfo)}; " +
                $"target={DescribeTarget(__instance)}");
        }
        catch (Exception ex)
        {
            Plugin.LogSource?.LogWarning($"Status observation failed: {ex.Message}");
        }
    }

    private static Exception? Finalizer(Exception? __exception)
    {
        if (_addStatusDepth > 0)
        {
            _addStatusDepth--;
        }

        return __exception;
    }

    private static bool IsMeaningfulAdd(StatusAddType type)
    {
        return type is StatusAddType.Add
            or StatusAddType.Upgrade
            or StatusAddType.AddAndProlong
            or StatusAddType.AddAndRenew
            or StatusAddType.Replace
            or StatusAddType.Stack;
    }

    private static bool IsPositive(Status? status, StatusTemplate? fallbackTemplate)
    {
        try
        {
            if (status?.Type != null)
            {
                return status.Type.IsPositive;
            }

            if (fallbackTemplate?.StatusType != null)
            {
                return fallbackTemplate.StatusType.IsPositive;
            }
        }
        catch
        {
        }

        return false;
    }

    private static string DescribeStatus(Status? status, StatusTemplate? fallbackTemplate)
    {
        try
        {
            if (status?.Type != null)
            {
                return status.Type.ToString();
            }

            if (fallbackTemplate?.StatusType != null)
            {
                return fallbackTemplate.StatusType.ToString();
            }
        }
        catch
        {
        }

        return status?.GetType().Name ?? fallbackTemplate?.GetType().Name ?? "unknown";
    }

    private static string DescribeSourceCharacter(StatusSourceInfo? sourceInfo)
    {
        if (sourceInfo == null)
        {
            return "none";
        }

        try
        {
            ICharacter? source = sourceInfo.GetSourceCharacter;
            if (source == null)
            {
                return "none";
            }

            return ReferenceEquals(source, Hero.Current)
                ? "hero"
                : $"character:{source.GetType().Name}";
        }
        catch
        {
            return "unavailable";
        }
    }

    private static string DescribeSourceItem(StatusSourceInfo? sourceInfo)
    {
        if (sourceInfo == null)
        {
            return "none";
        }

        try
        {
            Item? item = sourceInfo.GetSourceItemSafe;
            return item == null ? "none" : item.GetType().Name;
        }
        catch
        {
            return "unavailable";
        }
    }

    private static string DescribeTarget(CharacterStatuses statuses)
    {
        try
        {
            ICharacter? target = statuses.ParentModel;
            if (target == null)
            {
                return "none";
            }

            return ReferenceEquals(target, Hero.Current)
                ? "hero"
                : $"character:{target.GetType().Name}";
        }
        catch
        {
            return "unavailable";
        }
    }
}
