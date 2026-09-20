using System;
using System.Collections.Generic;
using System.Linq;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.General.StatTypes;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Stats.Observers;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace TGCommunity.Example.ProgressionOverlay;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.progression-overlay";
    public const string PluginName = "TG Example - Progression Overlay";
    public const string PluginVersion = "0.1.0";

    [ThreadStatic]
    private static Stack<Context>? _contexts;

    private static readonly Dictionary<string, PracticeRow> Practice =
        new(StringComparer.Ordinal);

    private Hero? _trackedHero;
    private Harmony? _harmony;

    private void Awake()
    {
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded. This example observes native proficiency practice only.");
    }

    private void Update()
    {
        Hero? hero = Hero.Current;
        if (!ReferenceEquals(hero, _trackedHero))
        {
            _trackedHero = hero;
            Practice.Clear();
        }
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        Practice.Clear();
    }

    private void OnGUI()
    {
        if (_trackedHero == null || Practice.Count == 0)
        {
            return;
        }

        var rows = Practice.Values
            .OrderByDescending(row => row.TotalXp)
            .Take(8)
            .ToArray();

        float height = 42f + rows.Length * 24f;
        GUI.Box(new Rect(24f, 120f, 390f, height), "Recent Practice");

        float y = 152f;
        foreach (PracticeRow row in rows)
        {
            GUI.Label(
                new Rect(40f, y, 355f, 22f),
                $"{row.Target}  {row.TotalXp:0.##} practice  ({row.LastSource})");
            y += 24f;
        }
    }

    private static void Push(ProfStatType target, BaseXPType source, float sourceParameter)
    {
        _contexts ??= new Stack<Context>();
        _contexts.Push(new Context(target, source, sourceParameter));
    }

    private static void Pop()
    {
        if (_contexts is { Count: > 0 })
        {
            _contexts.Pop();
        }
    }

    private static Context? Current =>
        _contexts is { Count: > 0 } ? _contexts.Peek() : null;

    private static void Record(ProfStatType target, float amount)
    {
        if (amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount))
        {
            return;
        }

        string targetName = target?.EnumName ?? "<unknown>";
        string sourceName = Current?.Source?.EnumName ?? "Direct";

        if (!Practice.TryGetValue(targetName, out PracticeRow? row))
        {
            row = new PracticeRow(targetName);
            Practice[targetName] = row;
        }

        row.TotalXp += amount;
        row.Events++;
        row.LastSource = sourceName;
    }

    [HarmonyPatch(typeof(ProficiencyEventListener), "XPGainEvent")]
    private static class EventContextPatch
    {
        private static void Prefix(
            ProfStatType proficiencyToLevel,
            BaseXPType targetXPType,
            float xpGainParameter)
        {
            Push(proficiencyToLevel, targetXPType, xpGainParameter);
        }

        private static void Finalizer()
        {
            Pop();
        }
    }

    [HarmonyPatch(typeof(ProficiencyStats), nameof(ProficiencyStats.TryAddXP))]
    private static class AddXpPatch
    {
        private static void Postfix(
            ProfStatType targetStatToRaiseXPOf,
            float amountOfXPToAdd)
        {
            Record(targetStatToRaiseXPOf, amountOfXPToAdd);
        }
    }

    private readonly struct Context
    {
        internal Context(ProfStatType target, BaseXPType source, float sourceParameter)
        {
            Target = target;
            Source = source;
            SourceParameter = sourceParameter;
        }

        internal ProfStatType Target { get; }
        internal BaseXPType Source { get; }
        internal float SourceParameter { get; }
    }

    private sealed class PracticeRow
    {
        internal PracticeRow(string target)
        {
            Target = target;
        }

        internal string Target { get; }
        internal float TotalXp { get; set; }
        internal int Events { get; set; }
        internal string LastSource { get; set; } = "Direct";
    }
}
