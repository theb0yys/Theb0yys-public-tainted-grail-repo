using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Items;
using Awaken.TG.Main.Heroes.Statuses;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TGExample.StatusCureObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.status-cure-observer";
    public const string PluginName = "TG Example - Status Cure/Removal Observer";
    public const string PluginVersion = "0.1.0";

    internal static ManualLogSource? LogSource { get; private set; }

    private Harmony? _harmony;

    private void Awake()
    {
        LogSource = Logger;
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded. This example observes consumable status deltas only.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        LogSource = null;
    }
}

[HarmonyPatch(typeof(Item), nameof(Item.Use))]
internal static class StatusCurePatch
{
    private static void Prefix(Item __instance, out StatusDeltaState __state)
    {
        __state = StatusDeltaState.Invalid;

        try
        {
            Hero? hero = Hero.Current;
            if (hero == null
                || hero.HasBeenDiscarded
                || __instance == null
                || !ReferenceEquals(__instance.Character, hero))
            {
                return;
            }

            ItemTemplate? template = SafeTemplate(__instance);
            if (template == null || !IsConsumableCandidate(template))
            {
                return;
            }

            __state = new StatusDeltaState(
                true,
                SafeItemName(__instance, template),
                Snapshot(hero));
        }
        catch (Exception ex)
        {
            Plugin.LogSource?.LogWarning($"Status prefix snapshot failed: {ex.Message}");
        }
    }

    private static void Postfix(StatusDeltaState __state)
    {
        if (!__state.Valid)
        {
            return;
        }

        try
        {
            Hero? hero = Hero.Current;
            if (hero == null || hero.HasBeenDiscarded)
            {
                return;
            }

            List<StatusSnapshot> after = Snapshot(hero);
            HashSet<Status> afterInstances = Instances(after);
            HashSet<Status> beforeInstances = Instances(__state.Before);

            List<string> removedNegative = new();
            foreach (StatusSnapshot snapshot in __state.Before)
            {
                if (!snapshot.Positive && !afterInstances.Contains(snapshot.Instance))
                {
                    removedNegative.Add(snapshot.Label);
                }
            }

            List<string> gainedPositive = new();
            foreach (StatusSnapshot snapshot in after)
            {
                if (snapshot.Positive && !beforeInstances.Contains(snapshot.Instance))
                {
                    gainedPositive.Add(snapshot.Label);
                }
            }

            if (removedNegative.Count == 0 && gainedPositive.Count == 0)
            {
                Plugin.LogSource?.LogInfo(
                    $"Status delta: item={__state.ItemName}; removedNegative=0; gainedPositive=0");
                return;
            }

            Plugin.LogSource?.LogInfo(
                $"Status delta: item={__state.ItemName}; " +
                $"removedNegative={removedNegative.Count}[{JoinLabels(removedNegative)}]; " +
                $"gainedPositive={gainedPositive.Count}[{JoinLabels(gainedPositive)}]");
        }
        catch (Exception ex)
        {
            Plugin.LogSource?.LogWarning($"Status postfix snapshot failed: {ex.Message}");
        }
    }

    private static List<StatusSnapshot> Snapshot(Hero hero)
    {
        List<StatusSnapshot> result = new();

        foreach (Status status in hero.Statuses.AllStatuses)
        {
            if (status == null)
            {
                continue;
            }

            result.Add(new StatusSnapshot(
                status,
                IsPositive(status),
                Describe(status)));
        }

        return result;
    }

    private static HashSet<Status> Instances(List<StatusSnapshot> snapshots)
    {
        HashSet<Status> result = new(ReferenceStatusComparer.Instance);

        foreach (StatusSnapshot snapshot in snapshots)
        {
            result.Add(snapshot.Instance);
        }

        return result;
    }

    private static bool IsPositive(Status status)
    {
        try
        {
            if (status.Type != null)
            {
                return status.Type.IsPositive;
            }

            return status.Template?.StatusType?.IsPositive ?? false;
        }
        catch
        {
            return false;
        }
    }

    private static string Describe(Status status)
    {
        try
        {
            return status.Type?.ToString()
                ?? status.Template?.StatusType?.ToString()
                ?? status.GetType().Name;
        }
        catch
        {
            return status.GetType().Name;
        }
    }

    private static string JoinLabels(List<string> labels)
    {
        return labels.Count == 0 ? "none" : string.Join("|", labels);
    }

    private static bool IsConsumableCandidate(ItemTemplate template)
    {
        return SafeBool(() => template.IsConsumable)
            || SafeBool(() => template.IsPotion)
            || SafeBool(() => template.IsPlainFood)
            || SafeBool(() => template.IsDish)
            || SafeBool(() => template.IsFish)
            || SafeBool(() => template.IsAlcohol)
            || SafeBool(() => template.ConsumablePotionOther);
    }

    private static ItemTemplate? SafeTemplate(Item item)
    {
        try
        {
            return item.Template;
        }
        catch
        {
            return null;
        }
    }

    private static string SafeItemName(Item item, ItemTemplate template)
    {
        try
        {
            return string.IsNullOrWhiteSpace(item.DisplayName)
                ? template.ItemName
                : item.DisplayName;
        }
        catch
        {
            return template.ItemName ?? "unknown";
        }
    }

    private static bool SafeBool(Func<bool> read)
    {
        try
        {
            return read();
        }
        catch
        {
            return false;
        }
    }

    private sealed class StatusDeltaState
    {
        internal static readonly StatusDeltaState Invalid = new(false, string.Empty, new List<StatusSnapshot>());

        internal StatusDeltaState(bool valid, string itemName, List<StatusSnapshot> before)
        {
            Valid = valid;
            ItemName = itemName;
            Before = before;
        }

        internal bool Valid { get; }
        internal string ItemName { get; }
        internal List<StatusSnapshot> Before { get; }
    }

    private sealed class StatusSnapshot
    {
        internal StatusSnapshot(Status instance, bool positive, string label)
        {
            Instance = instance;
            Positive = positive;
            Label = label;
        }

        internal Status Instance { get; }
        internal bool Positive { get; }
        internal string Label { get; }
    }

    private sealed class ReferenceStatusComparer : IEqualityComparer<Status>
    {
        internal static readonly ReferenceStatusComparer Instance = new();

        public bool Equals(Status? x, Status? y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(Status obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
