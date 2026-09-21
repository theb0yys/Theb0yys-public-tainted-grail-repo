using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Awaken.TG.MVC;
using Awaken.TG.Main.Heroes.Items;
using Awaken.TG.Main.Heroes.Items.LootTables;
using Awaken.TG.Main.Locations.Shops;
using Awaken.TG.Main.Locations.Shops.Stocks;
using Awaken.TG.Main.Locations.Shops.UI;
using Awaken.TG.Main.Templates;
using BepInEx.Logging;
using HarmonyLib;

namespace AvalonBroodmotherCompanion;

internal static class CustomBroodmotherMerchantStockPatch
{
    private static readonly HashSet<string> SuccessfulStockKeys = new(StringComparer.OrdinalIgnoreCase);

    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? target = AccessTools.Method(typeof(ShopUI), "OnFullyInitialized");
        MethodInfo? prefix = AccessTools.Method(typeof(CustomBroodmotherMerchantStockPatch), nameof(OnShopUiFullyInitializedPrefix));
        if (target == null || prefix == null)
        {
            logger.LogWarning($"{Plugin.PluginName}: could not patch ShopUI.OnFullyInitialized; Broodmother's Call merchant stock insertion disabled.");
            return;
        }

        harmony.Patch(target, prefix: new HarmonyMethod(prefix));
        logger.LogInfo($"{Plugin.PluginName}: patched ShopUI.OnFullyInitialized for Broodmother's Call known merchant stock insertion.");
    }

    private static void OnShopUiFullyInitializedPrefix(ShopUI __instance)
    {
        Plugin? plugin = Plugin.Instance;
        if (plugin == null || !plugin.MerchantStockEnabled || __instance?.Shop == null)
        {
            return;
        }

        try
        {
            TryAddToKnownMerchant(__instance.Shop, plugin);
        }
        catch (Exception ex)
        {
            plugin.ModLogger.LogWarning($"BROODMOTHER_CALL_MERCHANT_STOCK failed before ShopUI item-list setup; error={ex.GetType().Name}: {ex.Message}");
        }
    }

    private static bool TryAddToKnownMerchant(Shop shop, Plugin plugin)
    {
        string shopGuid = ShopGuid(shop);
        string shopLogName = string.IsNullOrWhiteSpace(shopGuid) ? ShopName(shop) : shopGuid;
        string targetShopGuid = plugin.MerchantStockTargetShopGuid.Trim();
        if (!string.IsNullOrWhiteSpace(targetShopGuid) &&
            !string.Equals(shopGuid, targetShopGuid, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!CustomBroodmotherSpellFeature.EnsureRegistered(plugin.ModLogger))
        {
            plugin.ModLogger.LogWarning($"BROODMOTHER_CALL_MERCHANT_STOCK skipped; shop={shopLogName}; templateGuid=avalon-creature-companion-summons; reason=custom-template-not-ready.");
            return false;
        }

        RestockableStock? stock = shop.Elements<RestockableStock>().FirstOrDefault(IsDecompressedRestockableStock);
        if (stock == null)
        {
            plugin.ModLogger.LogWarning($"BROODMOTHER_CALL_MERCHANT_STOCK skipped; shop={shopLogName}; templateGuid=avalon-creature-companion-summons; reason=no-decompressed-restockable-stock.");
            return false;
        }

        bool anyAdded = false;
        foreach (SpiderFamilyCompanionDefinition definition in SpiderFamilyCompanionDefinitions.All)
        {
            anyAdded |= TryAddDefinitionToStock(shop, stock, plugin, shopGuid, shopLogName, definition);
        }

        int expectedDefinitionCount = SpiderFamilyCompanionDefinitions.All.Count;
        int stockedDefinitionCount = CountStockedDefinitions(shop, stock, out string missingDefinitions);
        if (stockedDefinitionCount < expectedDefinitionCount)
        {
            plugin.ModLogger.LogWarning($"BROODMOTHER_CALL_MERCHANT_STOCK incomplete; shop={shopLogName}; expectedDefinitions={expectedDefinitionCount}; stockedDefinitions={stockedDefinitionCount}; missing={missingDefinitions}; route=post-stock-definition-count-check.");
        }
        else
        {
            plugin.ModLogger.LogInfo($"BROODMOTHER_CALL_MERCHANT_STOCK complete; shop={shopLogName}; expectedDefinitions={expectedDefinitionCount}; stockedDefinitions={stockedDefinitionCount}; route=post-stock-definition-count-check.");
        }

        return anyAdded;
    }

    private static bool TryAddDefinitionToStock(
        Shop shop,
        RestockableStock stock,
        Plugin plugin,
        string shopGuid,
        string shopLogName,
        SpiderFamilyCompanionDefinition definition)
    {
        string stockKey = $"{(string.IsNullOrWhiteSpace(shopGuid) ? ShopKey(shop).ToString() : shopGuid)}|{definition.SpellTemplateGuid}";
        if (SuccessfulStockKeys.Contains(stockKey))
        {
            plugin.ModLogger.LogInfo($"BROODMOTHER_CALL_MERCHANT_STOCK skipped; shop={shopLogName}; templateGuid={definition.SpellTemplateGuid}; variant={definition.Id}; reason=already-added-this-session.");
            return false;
        }

        if (ShopContainsTemplate(shop, definition.SpellTemplateGuid))
        {
            plugin.ModLogger.LogInfo($"BROODMOTHER_CALL_MERCHANT_STOCK skipped; shop={shopLogName}; templateGuid={definition.SpellTemplateGuid}; variant={definition.Id}; reason=already-present.");
            return false;
        }

        if (!TryResolveItemTemplate(definition.SpellTemplateGuid, out ItemTemplate? template, out string resolveReason) || template == null)
        {
            plugin.ModLogger.LogWarning($"BROODMOTHER_CALL_MERCHANT_STOCK skipped; shop={shopLogName}; templateGuid={definition.SpellTemplateGuid}; variant={definition.Id}; reason={resolveReason}.");
            return false;
        }

        int quantity = plugin.MerchantStockQuantity;
        int stockCountBefore = GetStockCount(stock);
        Item? createdItem = null;
        try
        {
            createdItem = World.Add(new Item(template, quantity));
            stock.AddItem(createdItem, allowStacking: false);
            SuccessfulStockKeys.Add(stockKey);
            int stockCountAfter = GetStockCount(stock);
            bool visibleAfterAdd = StockContainsTemplate(stock, definition.SpellTemplateGuid) ||
                                   ShopContainsTemplate(shop, definition.SpellTemplateGuid);
            plugin.ModLogger.LogInfo($"BROODMOTHER_CALL_MERCHANT_STOCK added; shop={shopLogName}; stockType=RestockableStock; templateName={template.name}; templateGuid={template.GUID}; displayName={definition.SpellDisplayName}; target={definition.DisplayName}; variant={definition.Id}; quantity={quantity}; stockCountBefore={stockCountBefore}; stockCountAfter={stockCountAfter}; visibleAfterAdd={visibleAfterAdd}; route=decompressed-before-ShopUI-item-list-World.Add-then-Stock.AddItem-no-stacking.");
            return true;
        }
        catch (Exception ex)
        {
            try
            {
                createdItem?.Discard();
            }
            catch
            {
                // Best-effort cleanup only; keep the stock-add failure visible.
            }

            plugin.ModLogger.LogWarning($"BROODMOTHER_CALL_MERCHANT_STOCK failed; shop={shopLogName}; templateGuid={definition.SpellTemplateGuid}; variant={definition.Id}; quantity={quantity}; error={ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private static int CountStockedDefinitions(Shop shop, Stock stock, out string missingDefinitionIds)
    {
        List<string> missing = new();
        int stocked = 0;
        foreach (SpiderFamilyCompanionDefinition definition in SpiderFamilyCompanionDefinitions.All)
        {
            if (StockContainsTemplate(stock, definition.SpellTemplateGuid) ||
                ShopContainsTemplate(shop, definition.SpellTemplateGuid))
            {
                stocked++;
            }
            else
            {
                missing.Add(definition.Id);
            }
        }

        missingDefinitionIds = missing.Count == 0 ? "none" : string.Join("|", missing);
        return stocked;
    }

    private static bool IsDecompressedRestockableStock(RestockableStock stock)
    {
        try
        {
            return !stock.IsCompressed && GetCompressedItems(stock).Count == 0;
        }
        catch
        {
            return false;
        }
    }

    private static List<ItemSpawningDataRuntime> GetCompressedItems(Stock stock)
    {
        FieldInfo? field = AccessTools.Field(stock.GetType(), "_compressedItems");
        return field?.GetValue(stock) as List<ItemSpawningDataRuntime> ?? new List<ItemSpawningDataRuntime>();
    }

    private static bool TryResolveItemTemplate(string templateGuid, out ItemTemplate? template, out string reason)
    {
        template = null;
        reason = string.Empty;

        try
        {
            TemplatesProvider? provider = World.Services?.TryGet<TemplatesProvider>();
            if (provider == null)
            {
                reason = "templates-provider-unavailable";
                return false;
            }

            if (!provider.AllLoaded)
            {
                reason = "templates-provider-not-loaded";
                return false;
            }

            template = provider.Get<ItemTemplate>(templateGuid);
            if (template == null)
            {
                reason = "template-guid-not-found";
                return false;
            }

            if (template.IsAbstract || template.HiddenOnUI)
            {
                reason = $"template-not-safe-visible abstract={template.IsAbstract} hidden={template.HiddenOnUI}";
                template = null;
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            reason = $"template-resolution-failed {ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }

    private static bool ShopContainsTemplate(Shop shop, string templateGuid)
    {
        try
        {
            foreach (Item item in shop.Items)
            {
                if (string.Equals(item?.Template?.GUID, templateGuid, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static bool StockContainsTemplate(Stock stock, string templateGuid)
    {
        try
        {
            if (stock.IsCompressed)
            {
                return false;
            }

            foreach (Item item in stock.Items)
            {
                if (string.Equals(item?.Template?.GUID, templateGuid, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static int GetStockCount(Stock stock)
    {
        MethodInfo? getter = AccessTools.PropertyGetter(stock.GetType(), "Count");
        if (getter?.Invoke(stock, null) is int propertyValue)
        {
            return Math.Max(0, propertyValue);
        }

        FieldInfo? field = AccessTools.Field(stock.GetType(), "<Count>k__BackingField") ?? AccessTools.Field(stock.GetType(), "_count");
        if (field?.GetValue(stock) is int fieldValue)
        {
            return Math.Max(0, fieldValue);
        }

        return -1;
    }

    private static string ShopGuid(Shop shop)
    {
        return shop.Template?.GUID ?? string.Empty;
    }

    private static string ShopName(Shop shop)
    {
        return shop.Template?.GUID ?? shop.Template?.name ?? "UnknownShop";
    }

    private static int ShopKey(Shop shop)
    {
        return RuntimeHelpers.GetHashCode(shop);
    }
}
