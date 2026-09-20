using System;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Items;
using Awaken.TG.Main.Templates;
using Awaken.TG.MVC;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace TGCommunity.Example.ActionReceipts;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.action-receipts";
    public const string PluginName = "TG Example - Action Receipts";
    public const string PluginVersion = "0.1.0";

    private ConfigEntry<string> _templateGuid = null!;
    private Rect _window = new Rect(20f, 120f, 360f, 150f);
    private Receipt _lastReceipt = Receipt.None;
    private int _sequence;

    private void Awake()
    {
        _templateGuid = Config.Bind("Grant", "TemplateGuid", "cbedd91efdca1a94b811b2ce7c926701", "Existing ItemTemplate GUID used by the demo Grant button.");
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnGUI()
    {
        _window = GUI.Window(GetInstanceID(), _window, DrawWindow, "FoA Action Receipt Example");
    }

    private void DrawWindow(int id)
    {
        GUILayout.Label("The button reports the gameplay result, not merely the click.");

        if (GUILayout.Button("Grant configured item x1"))
        {
            string capturedGuid = (_templateGuid.Value ?? string.Empty).Trim();
            _lastReceipt = ExecuteGrant(capturedGuid, 1);
        }

        GUILayout.Space(8f);
        GUILayout.Label(_lastReceipt.ToDisplayText());
        GUI.DragWindow(new Rect(0f, 0f, 10000f, 25f));
    }

    private Receipt ExecuteGrant(string templateGuid, int quantity)
    {
        int sequence = ++_sequence;

        if (templateGuid.Length == 0)
        {
            return Receipt.Blocked(sequence, "Grant", "Template GUID is empty.");
        }

        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            return Receipt.Blocked(sequence, "Grant", "Hero is unavailable.");
        }

        TemplatesProvider provider;
        try
        {
            provider = World.Services.Get<TemplatesProvider>();
        }
        catch (Exception ex)
        {
            return Receipt.Failed(sequence, "Grant", $"TemplatesProvider: {ex.GetType().Name}");
        }

        if (provider == null || !provider.AllLoaded)
        {
            return Receipt.Blocked(sequence, "Grant", "Templates are not loaded yet.");
        }

        ItemTemplate? template;
        try
        {
            template = provider.Get<ItemTemplate>(templateGuid);
        }
        catch (Exception ex)
        {
            return Receipt.Failed(sequence, "Grant", $"Lookup: {ex.GetType().Name}");
        }

        if (template == null)
        {
            return Receipt.Blocked(sequence, "Grant", "Configured item no longer resolves.");
        }

        try
        {
            Item item = World.Add(new Item(template, quantity));
            Item? added = hero.HeroItems.Add(item);
            return added != null
                ? Receipt.Done(sequence, "Grant", $"{template.name} x{quantity}")
                : Receipt.Failed(sequence, "Grant", "HeroItems.Add returned null.");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Grant action failed: {ex}");
            return Receipt.Failed(sequence, "Grant", ex.GetType().Name);
        }
    }

    private readonly struct Receipt
    {
        private Receipt(int sequence, string state, string action, string detail)
        {
            Sequence = sequence;
            State = state;
            Action = action;
            Detail = detail;
        }

        internal static Receipt None => new Receipt(0, "Ready", "None", "No action executed yet.");

        private int Sequence { get; }
        private string State { get; }
        private string Action { get; }
        private string Detail { get; }

        internal string ToDisplayText()
        {
            return Sequence == 0
                ? $"{State}: {Detail}"
                : $"#{Sequence} {State} — {Action}: {Detail}";
        }

        internal static Receipt Done(int sequence, string action, string detail) => new Receipt(sequence, "Done", action, detail);
        internal static Receipt Blocked(int sequence, string action, string detail) => new Receipt(sequence, "Blocked", action, detail);
        internal static Receipt Failed(int sequence, string action, string detail) => new Receipt(sequence, "Failed", action, detail);
    }
}
