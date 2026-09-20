using System;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Items;
using Awaken.TG.Main.Templates;
using Awaken.TG.MVC;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace TGCommunity.Example.GrantExistingItem;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.grant-existing-item";
    public const string PluginName = "TG Example - Grant Existing Item";
    public const string PluginVersion = "0.1.0";

    private ConfigEntry<string> _templateGuid = null!;
    private ConfigEntry<int> _quantity = null!;

    private void Awake()
    {
        _templateGuid = Config.Bind("Grant", "TemplateGuid", "cbedd91efdca1a94b811b2ce7c926701", "Existing ItemTemplate GUID. Default is the researched Bloodstone template.");
        _quantity = Config.Bind("Grant", "Quantity", 1, "Quantity to grant when F8 is pressed.");
        Logger.LogInfo($"{PluginName} loaded. Press F8 on a disposable test save.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            TryGrant();
        }
    }

    private void TryGrant()
    {
        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            Logger.LogWarning("Grant blocked: Hero.Current is unavailable.");
            return;
        }

        TemplatesProvider provider;
        try
        {
            provider = World.Services.Get<TemplatesProvider>();
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Grant blocked: TemplatesProvider unavailable ({ex.GetType().Name}).");
            return;
        }

        if (provider == null || !provider.AllLoaded)
        {
            Logger.LogWarning("Grant blocked: TemplatesProvider is not fully loaded.");
            return;
        }

        string guid = (_templateGuid.Value ?? string.Empty).Trim();
        if (guid.Length == 0)
        {
            Logger.LogWarning("Grant blocked: TemplateGuid is empty.");
            return;
        }

        ItemTemplate? template;
        try
        {
            template = provider.Get<ItemTemplate>(guid);
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Grant blocked: template lookup failed ({ex.GetType().Name}).");
            return;
        }

        if (template == null)
        {
            Logger.LogWarning($"Grant blocked: ItemTemplate '{guid}' did not resolve.");
            return;
        }

        int quantity = Math.Max(1, Math.Min(50, _quantity.Value));

        try
        {
            Item item = World.Add(new Item(template, quantity));
            Item? added = hero.HeroItems.Add(item);

            if (added == null)
            {
                Logger.LogWarning($"Grant failed: HeroItems.Add returned null for {template.name} [{guid}].");
                return;
            }

            Logger.LogInfo($"Grant succeeded: template={template.name}; guid={guid}; quantity={quantity}.");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Grant failed: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
