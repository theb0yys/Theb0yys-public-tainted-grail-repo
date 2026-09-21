using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Awaken.TG.Assets;
using Awaken.TG.MVC;
using Awaken.TG.Main.Heroes.Items;
using Awaken.TG.Main.Heroes.Items.LootTables;
using Awaken.TG.Main.Localization;
using Awaken.TG.Main.Locations.Shops;
using Awaken.TG.Main.Locations.Shops.Stocks;
using Awaken.TG.Main.Locations.Shops.UI;
using Awaken.TG.Main.Templates;
using HarmonyLib;
using UnityEngine;

namespace TaintedGems.Patches;

[HarmonyPatch(typeof(Shop), nameof(Shop.OpenShop))]
internal static class TaintedGemsShopPatch
{
    private static readonly FieldInfo? TemplatesProviderLoaderField = AccessTools.Field(typeof(TemplatesProvider), "_loader");
    private static readonly FieldInfo? ItemTemplateDescriptionField = AccessTools.Field(typeof(ItemTemplate), "description");
    private static readonly MethodInfo? TemplatesLoaderAddToMapMethod = AccessTools.Method(typeof(TemplatesLoader), "AddToMap");
    private static readonly HashSet<string> SuccessfulStockKeys = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> RegisteredCustomTemplateKeys = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, int> StockCursorByShop = new(StringComparer.OrdinalIgnoreCase);
    private static bool DescriptionFieldWarningLogged;

    private const string GenericValuableSourceGuid = "5373347b55faaf543964e09a965a7ea4";
    private const string GenericValuableIconSlug = "GarnetShard";

    private static readonly BaseGemTemplateDescriptor[] BaseGemTemplates =
    {
        new("58ec867b64fc49d4683264affbf729fa", "7a9e0000000000000000000000000003", "ItemTemplate_Mod_TaintedGems_AncestralSphere", "Tainted Ancestral Sphere"),
        new("5825dc9d71811d3468f89e8ef95040c0", "7a9e0000000000000000000000000004", "ItemTemplate_Mod_TaintedGems_AzureLeechstone", "Tainted Azure Leechstone"),
        new("f89a8ad904e22ae44bf479f7546673ed", "7a9e0000000000000000000000000005", "ItemTemplate_Mod_TaintedGems_BlightOpal", "Tainted Blight Opal"),
        new("6b0734783d1a55044b3be9a1cd194cf0", "7a9e0000000000000000000000000006", "ItemTemplate_Mod_TaintedGems_BloodstoneoftheFallen", "Tainted Bloodstone of the Fallen"),
        new("f83e8511f76582c4f9af2ce26142147e", "7a9e0000000000000000000000000007", "ItemTemplate_Mod_TaintedGems_Bloodthorn", "Tainted Bloodthorn"),
        new("8a580f853f5732341bea6b1567a2b75c", "7a9e0000000000000000000000000008", "ItemTemplate_Mod_TaintedGems_BorsMightstone", "Tainted Bors' Mightstone"),
        new("6d416a9ced81f4742bd298a47c6d0333", "7a9e0000000000000000000000000009", "ItemTemplate_Mod_TaintedGems_Bravery", "Tainted Bravery"),
        new("983bbb98f338b1046aaf32cc7a2d7330", "7a9e000000000000000000000000000a", "ItemTemplate_Mod_TaintedGems_BurningStone", "Tainted Burning Stone"),
        new("2e151c6f2c449e14d92cb014cfd397dd", "7a9e000000000000000000000000000b", "ItemTemplate_Mod_TaintedGems_CelestialCharm", "Tainted Celestial Charm"),
        new("fad84975ce3e0c04587e3cbae1d59bf0", "7a9e000000000000000000000000000c", "ItemTemplate_Mod_TaintedGems_ColdCurrent", "Tainted Cold Current"),
        new("1ca5da4c71ac63a469dd360c09b36927", "7a9e000000000000000000000000000d", "ItemTemplate_Mod_TaintedGems_ColdStone", "Tainted Cold Stone"),
        new("a8a01e2728a4d0f4189089f89e0b5f6b", "7a9e0000000000000000000000000002", "ItemTemplate_Mod_TaintedGems_CrimsonCluster", "Tainted Crimson Cluster"),
        new("a56dc2c2b29a48943a2fea63aa2d4627", "7a9e000000000000000000000000000e", "ItemTemplate_Mod_TaintedGems_EndlessPursuit", "Tainted Endless Pursuit"),
        new("4c1ca3668dd21434fba26969ff3caea5", "7a9e000000000000000000000000000f", "ItemTemplate_Mod_TaintedGems_FeastOfStrain", "Tainted Feast Of Strain"),
        new("d42cb5bc2917a5b43aa11950bd420d60", "7a9e0000000000000000000000000010", "ItemTemplate_Mod_TaintedGems_FeralCharm", "Tainted Feral Charm"),
        new("8ef00e6eb51e3334b823cd8faf49e40a", "7a9e0000000000000000000000000011", "ItemTemplate_Mod_TaintedGems_FlickeringFigurine", "Tainted Flickering Figurine"),
        new("1a9425aed5a1de5479800238e91c87b1", "7a9e0000000000000000000000000012", "ItemTemplate_Mod_TaintedGems_ForeDwellerBauble", "Tainted Fore-Dweller Bauble"),
        new("037d430b700551c49924a38348ec0d6f", "7a9e0000000000000000000000000001", "ItemTemplate_Mod_TaintedGems_GarnetShard", "Tainted Garnet Shard"),
        new("d32f1353693d1c444a736ead57c9e063", "7a9e0000000000000000000000000013", "ItemTemplate_Mod_TaintedGems_GemofSpeed", "Tainted Gem of Speed"),
        new("aa0925473a8b86e40a49eac0da5c99ed", "7a9e0000000000000000000000000014", "ItemTemplate_Mod_TaintedGems_GrimalkinsEye", "Tainted Grimalkin's Eye"),
        new("855c7beb6ccb3a5478eeb00a38eab7bc", "7a9e0000000000000000000000000015", "ItemTemplate_Mod_TaintedGems_Hastefang", "Tainted Hastefang"),
        new("c1c7fc8a3559f154898f2b85263fbf30", "7a9e0000000000000000000000000016", "ItemTemplate_Mod_TaintedGems_HauntedSoulgem", "Tainted Haunted Soulgem"),
        new("efe7be1d59b53f247aafa06db70ee1f3", "7a9e0000000000000000000000000017", "ItemTemplate_Mod_TaintedGems_IncandescentPearl", "Tainted Incandescent Pearl"),
        new("028a7453835b8f14c84528aada1927d9", "7a9e0000000000000000000000000018", "ItemTemplate_Mod_TaintedGems_KnowingWorm", "Tainted Knowing Worm"),
        new("cd9db33dd6edac24590a38a67bd03fd7", "7a9e0000000000000000000000000019", "ItemTemplate_Mod_TaintedGems_MarkOfThree", "Tainted Mark Of Three"),
        new("3bb1251c4c2fd8647942f3e1136774eb", "7a9e000000000000000000000000001a", "ItemTemplate_Mod_TaintedGems_Mercy", "Tainted Mercy"),
        new("148584b64bf0d774484e2cc744359c5f", "7a9e000000000000000000000000001b", "ItemTemplate_Mod_TaintedGems_NightshadeCharcoal", "Tainted Nightshade Charcoal"),
        new("701be68f87d977742a83f07558c8f8ca", "7a9e000000000000000000000000001c", "ItemTemplate_Mod_TaintedGems_RavenWingsSoulgem", "Tainted Raven Wing's Soulgem"),
        new("55068e9445839164099d7fce267ab764", "7a9e000000000000000000000000001d", "ItemTemplate_Mod_TaintedGems_SerpentsEscape", "Tainted Serpent's Escape"),
        new("0a3e2a743eebbd7479b8754094d797bc", "7a9e000000000000000000000000001e", "ItemTemplate_Mod_TaintedGems_ShadowbladeNail", "Tainted Shadowblade Nail"),
        new("dde6a0dffd1200d42b2b1cce85e10812", "7a9e000000000000000000000000001f", "ItemTemplate_Mod_TaintedGems_Shipworm", "Tainted Shipworm"),
        new("de2d8bb54ea6f5c468a78fd8f68defb4", "7a9e0000000000000000000000000020", "ItemTemplate_Mod_TaintedGems_ShiverwoundCurse", "Tainted Shiverwound Curse"),
        new("3e3952e377d1c794cbafc1870f3cf5de", "7a9e0000000000000000000000000021", "ItemTemplate_Mod_TaintedGems_SmoulderingWrath", "Tainted Smouldering Wrath"),
        new("aa37ecf8997332a4c91de22cc317c737", "7a9e0000000000000000000000000022", "ItemTemplate_Mod_TaintedGems_StarbornEgg", "Tainted Starborn Egg"),
        new("fe889ab1d40d5d944a7ed6af1951ded6", "7a9e0000000000000000000000000023", "ItemTemplate_Mod_TaintedGems_TheFlatteningMoon", "Tainted The Flattening Moon"),
        new("acb5ad67da8fc2b48a353824f54f5166", "7a9e0000000000000000000000000024", "ItemTemplate_Mod_TaintedGems_ThundersReproach", "Tainted Thunder's Reproach"),
        new("81298bd1921161f4bbcedb3e80b29987", "7a9e0000000000000000000000000025", "ItemTemplate_Mod_TaintedGems_TidePearl", "Tainted Tide Pearl"),
        new("153d226a2d8356d4585b7127a00fb2b7", "7a9e0000000000000000000000000026", "ItemTemplate_Mod_TaintedGems_TiedTongue", "Tainted Tied Tongue"),
        new("f066289117ce5a84cb91ebb2be0a09be", "7a9e0000000000000000000000000027", "ItemTemplate_Mod_TaintedGems_TimeweaverBead", "Tainted Timeweaver Bead"),
        new("dfb49e58dcac68b4a956d52dd9da1f0e", "7a9e0000000000000000000000000028", "ItemTemplate_Mod_TaintedGems_TornRoots", "Tainted Torn Roots"),
        new("e2dd4dc32470f6845bb11b00967d6893", "7a9e0000000000000000000000000029", "ItemTemplate_Mod_TaintedGems_TwistedEssence", "Tainted Twisted Essence"),
        new("307891a8c3370764d90e91ee8af3ea92", "7a9e000000000000000000000000002a", "ItemTemplate_Mod_TaintedGems_WardensRunestone", "Tainted Warden's Runestone")
    };

    private static readonly GemTemplateDescriptor[] TaintedGemTemplates = BuildStoreTemplates();

    private static readonly IReadOnlyDictionary<string, GemTemplateDescriptor> TaintedGemTemplatesByCustomGuid =
        TaintedGemTemplates
            .Where(descriptor => descriptor.HasSocketedGameplay)
            .ToDictionary(descriptor => descriptor.CustomGuid, StringComparer.OrdinalIgnoreCase);

    internal static bool TryGetTaintedGemDescriptor(string? templateGuid, out GemTemplateDescriptor descriptor)
    {
        descriptor = default;
        return !string.IsNullOrWhiteSpace(templateGuid) &&
            TaintedGemTemplatesByCustomGuid.TryGetValue(templateGuid, out descriptor);
    }

    internal static int StoreTemplateCount => TaintedGemTemplates.Length;

    private static GemTemplateDescriptor[] BuildStoreTemplates()
    {
        List<GemTemplateDescriptor> descriptors = new(BaseGemTemplates.Length * 3 + 2);
        foreach (BaseGemTemplateDescriptor baseDescriptor in BaseGemTemplates)
        {
            descriptors.Add(new GemTemplateDescriptor(
                baseDescriptor.SourceGuid,
                baseDescriptor.CustomGuid,
                baseDescriptor.TemplateName,
                baseDescriptor.ThematicName,
                baseDescriptor.IconSlug,
                StoreGemKind.Skill));

            descriptors.Add(new GemTemplateDescriptor(
                baseDescriptor.SourceGuid,
                VariantGuid("7a9f", baseDescriptor.CustomGuid),
                baseDescriptor.TemplateName + "_Perk",
                baseDescriptor.ThematicName + " Perk",
                baseDescriptor.IconSlug,
                StoreGemKind.Perk));

            descriptors.Add(new GemTemplateDescriptor(
                GenericValuableSourceGuid,
                VariantGuid("7aa0", baseDescriptor.CustomGuid),
                baseDescriptor.TemplateName + "_Trinket",
                baseDescriptor.ThematicName + " Trinket",
                baseDescriptor.IconSlug,
                StoreGemKind.Trinket));
        }

        descriptors.Add(new GemTemplateDescriptor(
            GenericValuableSourceGuid,
            "7aa10000000000000000000000000001",
            "ItemTemplate_Mod_TaintedGems_GenericValuableGem",
            "Generic Valuable Gem",
            GenericValuableIconSlug,
            StoreGemKind.Valuable));

        descriptors.Add(new GemTemplateDescriptor(
            GenericValuableSourceGuid,
            "7aa10000000000000000000000000002",
            "ItemTemplate_Mod_TaintedGems_GenericJunkGem",
            "Generic Junk Gem",
            GenericValuableIconSlug,
            StoreGemKind.Junk));

        return descriptors.ToArray();
    }

    private static string VariantGuid(string prefix, string baseGuid)
    {
        return prefix + baseGuid.Substring(Math.Min(prefix.Length, baseGuid.Length));
    }

    private static void Prefix(Shop __instance)
    {
        TryRegisterCustomGemTemplates();
        RemoveInvalidCompressedStockRows(__instance);
    }

    internal static void PrepareShopStockBeforeUi(Shop shop)
    {
        try
        {
            TryRegisterCustomGemTemplates();
            TryAddTaintedGemStock(shop);
        }
        catch (Exception ex)
        {
            Plugin.Instance?.ModLogger.LogWarning($"TaintedGemsShopStock failed before ShopUI item-list setup; error={ex.GetType().Name}: {ex.Message}");
        }
    }

    internal static void RegisterCustomGemTemplatesAfterLoaderFinished()
    {
        try
        {
            if (TryRegisterCustomGemTemplates())
            {
                Plugin.Instance?.ModLogger.LogInfo("TaintedGemsCustomTemplate early registration completed; phase=templates-loader-finished; save-template-resolution-ready=true.");
            }
        }
        catch (Exception ex)
        {
            Plugin.Instance?.ModLogger.LogWarning($"TaintedGemsCustomTemplate early registration failed; phase=templates-loader-finished; error={ex.GetType().Name}: {ex.Message}");
        }
    }

    private static bool TryRegisterCustomGemTemplates()
    {
        Plugin? plugin = Plugin.Instance;
        if (plugin == null || !plugin.CustomTemplateEnabled)
        {
            return false;
        }

        bool registeredAny = false;
        foreach (GemTemplateDescriptor descriptor in TaintedGemTemplates)
        {
            registeredAny |= TryRegisterCustomGemTemplate(plugin, descriptor);
        }

        return registeredAny;
    }

    private static bool TryAddTaintedGemStock(Shop shop)
    {
        Plugin? plugin = Plugin.Instance;
        if (plugin == null || !plugin.ShopStockEnabled)
        {
            return false;
        }

        if (!TryGetMutationShopContext(shop, plugin, out string shopGuid, out string shopLogName))
        {
            return false;
        }

        int perOpenLimit = plugin.ShopStockTemplatesPerOpen;
        if (perOpenLimit <= 0)
        {
            plugin.ModLogger.LogInfo($"TaintedGemsShopStock skipped; shop={shopLogName}; reason=templates-per-open-zero.");
            return false;
        }

        bool addedAny = false;
        int attemptedCount = 0;
        int addedCount = 0;
        int skippedCount = 0;
        int failedCount = 0;
        string cursorKey = string.IsNullOrWhiteSpace(shopGuid) ? ShopKey(shop).ToString() : shopGuid;
        StockCursorByShop.TryGetValue(cursorKey, out int startIndex);

        for (int offset = 0; offset < TaintedGemTemplates.Length && attemptedCount < perOpenLimit; offset++)
        {
            int descriptorIndex = (startIndex + offset) % TaintedGemTemplates.Length;
            GemTemplateDescriptor descriptor = TaintedGemTemplates[descriptorIndex];
            attemptedCount++;

            try
            {
                if (!TryResolveStockTemplateGuid(plugin, descriptor, out string stockTemplateGuid, out string stockSource))
                {
                    skippedCount++;
                    continue;
                }

                bool added = TryAddStockTemplate(shop, plugin, shopGuid, shopLogName, stockTemplateGuid, plugin.ShopStockQuantity, stockSource);
                if (!added)
                {
                    skippedCount++;
                    continue;
                }

                addedAny = true;
                addedCount++;
                StockCursorByShop[cursorKey] = (descriptorIndex + 1) % TaintedGemTemplates.Length;
            }
            catch (Exception ex)
            {
                failedCount++;
                plugin.ModLogger.LogWarning($"TaintedGemsShopStock failed; shop={shopLogName}; templateName={descriptor.TemplateName}; customGuid={descriptor.CustomGuid}; sourceGuid={descriptor.SourceGuid}; error={ex.GetType().Name}: {ex.Message}");
            }
        }

        plugin.ModLogger.LogInfo($"TaintedGemsShopStock summary; shop={shopLogName}; attempted={attemptedCount}; added={addedCount}; skipped={skippedCount}; failed={failedCount}; candidateCount={TaintedGemTemplates.Length}; perOpenLimit={perOpenLimit}; route=decompressed-before-ShopUI-item-list-World.Add-then-Stock.AddItem.");
        return addedAny;
    }

    private static bool TryResolveStockTemplateGuid(Plugin plugin, GemTemplateDescriptor descriptor, out string stockTemplateGuid, out string stockSource)
    {
        stockTemplateGuid = string.Empty;
        stockSource = string.Empty;

        if (plugin.CustomTemplateEnabled && TryRegisterCustomGemTemplate(plugin, descriptor) &&
            TryResolveItemTemplate(descriptor.CustomGuid, out ItemTemplate? customTemplate, out string customReason) && customTemplate != null)
        {
            stockTemplateGuid = descriptor.CustomGuid;
            stockSource = $"tainted-gem-custom:{descriptor.KindLabel}:{descriptor.TemplateName}";
            return true;
        }

        if (!plugin.CustomTemplateAllowNativeFallback)
        {
            plugin.ModLogger.LogWarning($"TaintedGemsShopStock skipped; templateName={descriptor.TemplateName}; customGuid={descriptor.CustomGuid}; reason=custom-template-unavailable-and-native-fallback-disabled.");
            return false;
        }

        if (!TryResolveItemTemplate(descriptor.SourceGuid, out ItemTemplate? sourceTemplate, out string sourceReason) || sourceTemplate == null)
        {
            plugin.ModLogger.LogWarning($"TaintedGemsShopStock skipped; templateName={descriptor.TemplateName}; customGuid={descriptor.CustomGuid}; sourceGuid={descriptor.SourceGuid}; reason=source-{sourceReason}.");
            return false;
        }

        stockTemplateGuid = descriptor.SourceGuid;
        stockSource = $"native-gem-fallback:{sourceTemplate.name}";
        plugin.ModLogger.LogWarning($"TaintedGemsShopStock using native fallback; templateName={descriptor.TemplateName}; customGuid={descriptor.CustomGuid}; sourceGuid={descriptor.SourceGuid}; sourceName={sourceTemplate.name}; reason=custom-template-not-resolved.");
        return true;
    }

    private static bool TryAddStockTemplate(
        Shop shop,
        Plugin plugin,
        string shopGuid,
        string shopLogName,
        string targetTemplateGuid,
        int quantity,
        string source)
    {
        string mutationKey = $"{(string.IsNullOrWhiteSpace(shopGuid) ? ShopKey(shop).ToString() : shopGuid)}|{targetTemplateGuid}";
        if (SuccessfulStockKeys.Contains(mutationKey))
        {
            plugin.ModLogger.LogInfo($"TaintedGemsShopStock skipped; shop={shopLogName}; source={source}; templateGuid={targetTemplateGuid}; reason=already-added-this-session.");
            return false;
        }

        if (ShopContainsTemplate(shop, targetTemplateGuid))
        {
            plugin.ModLogger.LogInfo($"TaintedGemsShopStock skipped; shop={shopLogName}; source={source}; templateGuid={targetTemplateGuid}; reason=already-present.");
            return false;
        }

        RestockableStock? stock = shop.Elements<RestockableStock>().FirstOrDefault(IsDecompressedRestockableStock);
        if (stock == null)
        {
            plugin.ModLogger.LogWarning($"TaintedGemsShopStock skipped; shop={shopLogName}; source={source}; templateGuid={targetTemplateGuid}; reason=no-decompressed-restockable-stock.");
            return false;
        }

        if (!TryResolveItemTemplate(targetTemplateGuid, out ItemTemplate? template, out string resolveReason) || template == null)
        {
            plugin.ModLogger.LogWarning($"TaintedGemsShopStock skipped; shop={shopLogName}; source={source}; templateGuid={targetTemplateGuid}; reason={resolveReason}.");
            return false;
        }

        int stockCountBefore = GetStockCount(stock);
        Item? createdItem = null;
        try
        {
            createdItem = World.Add(new Item(template, quantity));
            stock.AddItem(createdItem, allowStacking: true);
            SuccessfulStockKeys.Add(mutationKey);
            int stockCountAfter = GetStockCount(stock);
            bool visibleAfterAdd = StockContainsTemplate(stock, targetTemplateGuid) || ShopContainsTemplate(shop, targetTemplateGuid);
            plugin.ModLogger.LogInfo($"TaintedGemsShopStock added; shop={shopLogName}; source={source}; stockType=RestockableStock; templateName={template.name}; templateGuid={template.GUID}; quantity={quantity}; stockCountBefore={stockCountBefore}; stockCountAfter={stockCountAfter}; visibleAfterAdd={visibleAfterAdd}; route=decompressed-before-ShopUI-item-list-World.Add-then-Stock.AddItem.");
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
                // Keep the original stock-add failure in the log.
            }

            plugin.ModLogger.LogWarning($"TaintedGemsShopStock failed; shop={shopLogName}; source={source}; templateGuid={targetTemplateGuid}; quantity={quantity}; error={ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private static bool TryRegisterCustomGemTemplate(Plugin plugin, GemTemplateDescriptor descriptor)
    {
        string sourceGuid = descriptor.SourceGuid.Trim();
        string customGuid = descriptor.CustomGuid.Trim();
        string templateName = descriptor.TemplateName.Trim();
        string thematicName = descriptor.ThematicName.Trim();

        if (string.IsNullOrWhiteSpace(sourceGuid) || string.IsNullOrWhiteSpace(customGuid))
        {
            plugin.ModLogger.LogWarning("TaintedGemsCustomTemplate skipped; reason=blank-source-or-custom-guid.");
            return false;
        }

        if (string.Equals(sourceGuid, customGuid, StringComparison.OrdinalIgnoreCase))
        {
            plugin.ModLogger.LogWarning($"TaintedGemsCustomTemplate skipped; sourceGuid={sourceGuid}; customGuid={customGuid}; reason=source-and-custom-guid-match.");
            return false;
        }

        if (RegisteredCustomTemplateKeys.Contains(customGuid))
        {
            return true;
        }

        if (TryResolveItemTemplate(customGuid, out ItemTemplate? existingTemplate, out _) && existingTemplate != null)
        {
            RegisteredCustomTemplateKeys.Add(customGuid);
            plugin.ModLogger.LogInfo($"TaintedGemsCustomTemplate already-registered; customGuid={customGuid}; templateName={existingTemplate.name}.");
            return true;
        }

        if (!TryResolveItemTemplate(sourceGuid, out ItemTemplate? sourceTemplate, out string sourceReason) || sourceTemplate == null)
        {
            plugin.ModLogger.LogWarning($"TaintedGemsCustomTemplate skipped; sourceGuid={sourceGuid}; customGuid={customGuid}; reason=source-{sourceReason}.");
            return false;
        }

        string[] sourceTagsBefore = sourceTemplate.tags?.ToArray() ?? Array.Empty<string>();

        if (!TryGetTemplateLoader(out object? loader, out string loaderReason) || loader == null)
        {
            plugin.ModLogger.LogWarning($"TaintedGemsCustomTemplate skipped; sourceGuid={sourceGuid}; customGuid={customGuid}; reason={loaderReason}.");
            return false;
        }

        if (TemplatesLoaderAddToMapMethod == null)
        {
            plugin.ModLogger.LogWarning($"TaintedGemsCustomTemplate skipped; sourceGuid={sourceGuid}; customGuid={customGuid}; reason=templates-loader-add-map-method-missing.");
            return false;
        }

        templateName = string.IsNullOrWhiteSpace(templateName) ? $"ItemTemplate_Mod_TaintedGems_{customGuid}" : templateName;
        thematicName = string.IsNullOrWhiteSpace(thematicName) ? templateName : thematicName;

        GameObject? cloneObject = null;
        try
        {
            cloneObject = UnityEngine.Object.Instantiate(sourceTemplate.gameObject);
            cloneObject.name = templateName;
            UnityEngine.Object.DontDestroyOnLoad(cloneObject);

            ItemTemplate? customTemplate = cloneObject.GetComponent<ItemTemplate>();
            if (customTemplate == null)
            {
                UnityEngine.Object.Destroy(cloneObject);
                plugin.ModLogger.LogWarning($"TaintedGemsCustomTemplate skipped; sourceGuid={sourceGuid}; customGuid={customGuid}; reason=cloned-object-missing-item-template.");
                return false;
            }

            customTemplate.name = templateName;
            customTemplate.GUID = customGuid;
            customTemplate.templateType = sourceTemplate.templateType;
            customTemplate.hiddenOnUI = false;
            customTemplate.cannotBeDropped = false;
            customTemplate.itemName = (LocString)thematicName;
            customTemplate.iconReference = new ShareableSpriteReference(descriptor.CustomIconAddress);
            customTemplate.conditionalIconReference?.Clear();
            customTemplate.basePrice = descriptor.BasePrice;
            customTemplate.overrideBuyPrice = true;
            customTemplate.buyPrice = descriptor.BuyPrice;
            customTemplate.priceLevelMultiplier = descriptor.PriceLevelMultiplier;
            TrySetCustomTemplateDescription(customTemplate, descriptor, plugin);

            if (!TryValidateNativeCloneContract(sourceTemplate, customTemplate, sourceTagsBefore, out string nativeContract))
            {
                UnityEngine.Object.Destroy(cloneObject);
                plugin.ModLogger.LogWarning($"TaintedGemsCustomTemplate skipped; sourceGuid={sourceGuid}; customGuid={customGuid}; thematicName={thematicName}; reason={nativeContract}.");
                return false;
            }

            TemplatesLoaderAddToMapMethod.Invoke(loader, new object[] { customGuid, customTemplate });
            RegisteredCustomTemplateKeys.Add(customGuid);
            plugin.ModLogger.LogInfo($"TaintedGemsCustomTemplate registered; customGuid={customGuid}; templateName={templateName}; thematicName={thematicName}; sourceGuid={sourceGuid}; sourceName={sourceTemplate.name}; customIcon={descriptor.CustomIconAddress}; storeKind={descriptor.KindLabel}; skill={descriptor.SkillName}; effectName={descriptor.PerkName}; effect={descriptor.EffectKindLabel}; disposition={descriptor.DispositionLabel}; basePrice={descriptor.BasePrice}; buyPrice={descriptor.BuyPrice}; priceLevelMultiplier={descriptor.PriceLevelMultiplier:0.##}; socketedGameplay={descriptor.HasSocketedGameplay}; silentSneak={descriptor.SilentSneak}; jumpVelocityMultiplier={descriptor.JumpVelocityMultiplier:0.###}; shopBuyPriceMultiplier={descriptor.ShopBuyPriceMultiplier:0.###}; heroSellPriceMultiplier={descriptor.HeroSellPriceMultiplier:0.###}; itemStaminaCostMultiplier={descriptor.ItemStaminaCostMultiplier:0.###}; {nativeContract}; customWorldPrefab=false; route=clone-native-template-private-loader-map.");
            return true;
        }
        catch (Exception ex)
        {
            if (cloneObject != null)
            {
                UnityEngine.Object.Destroy(cloneObject);
            }

            plugin.ModLogger.LogWarning($"TaintedGemsCustomTemplate failed; sourceGuid={sourceGuid}; customGuid={customGuid}; templateName={templateName}; error={ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    private static void TrySetCustomTemplateDescription(ItemTemplate customTemplate, GemTemplateDescriptor descriptor, Plugin plugin)
    {
        if (ItemTemplateDescriptionField == null)
        {
            if (!DescriptionFieldWarningLogged)
            {
                DescriptionFieldWarningLogged = true;
                plugin.ModLogger.LogWarning("TaintedGemsCustomTemplate description override unavailable; reason=item-template-description-field-missing.");
            }

            return;
        }

        try
        {
            ItemTemplateDescriptionField.SetValue(customTemplate, new OptionalLocString((LocString)descriptor.ItemDescription, toggled: true));
        }
        catch (Exception ex)
        {
            if (!DescriptionFieldWarningLogged)
            {
                DescriptionFieldWarningLogged = true;
                plugin.ModLogger.LogWarning($"TaintedGemsCustomTemplate description override failed; templateName={descriptor.TemplateName}; customGuid={descriptor.CustomGuid}; error={ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private static bool TryValidateNativeCloneContract(
        ItemTemplate sourceTemplate,
        ItemTemplate customTemplate,
        string[] sourceTagsBefore,
        out string result)
    {
        string[] sourceTagsAfter = sourceTemplate.tags?.ToArray() ?? Array.Empty<string>();
        if (!sourceTagsBefore.SequenceEqual(sourceTagsAfter, StringComparer.Ordinal))
        {
            result = "native-source-tags-mutated";
            return false;
        }

        string[] sourceComponentTypes = GetComponentTypeNames(sourceTemplate.gameObject);
        string[] customComponentTypes = GetComponentTypeNames(customTemplate.gameObject);
        if (!sourceComponentTypes.SequenceEqual(customComponentTypes, StringComparer.Ordinal))
        {
            result = "native-logic-component-types-mismatch";
            return false;
        }

        if (sourceTemplate.CanStack != customTemplate.CanStack ||
            sourceTemplate.IsConsumable != customTemplate.IsConsumable ||
            sourceTemplate.IsPotion != customTemplate.IsPotion ||
            sourceTemplate.IsPlainFood != customTemplate.IsPlainFood ||
            sourceTemplate.IsDish != customTemplate.IsDish ||
            sourceTemplate.IsFish != customTemplate.IsFish ||
            sourceTemplate.IsCrafting != customTemplate.IsCrafting ||
            sourceTemplate.IsComponent != customTemplate.IsComponent ||
            sourceTemplate.IsCraftingComponent != customTemplate.IsCraftingComponent ||
            sourceTemplate.IsAlchemyComponent != customTemplate.IsAlchemyComponent ||
            sourceTemplate.IsCookingComponent != customTemplate.IsCookingComponent ||
            sourceTemplate.IsAlcohol != customTemplate.IsAlcohol)
        {
            result = "native-logic-classification-mismatch";
            return false;
        }

        if (sourceTemplate.DropPrefab == null || !sourceTemplate.DropPrefab.IsSet)
        {
            result = "native-source-drop-prefab-not-set";
            return false;
        }

        if (customTemplate.DropPrefab == null || !customTemplate.DropPrefab.IsSet ||
            !string.Equals(sourceTemplate.DropPrefab.AssetGUID, customTemplate.DropPrefab.AssetGUID, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(sourceTemplate.DropPrefab.SubObject, customTemplate.DropPrefab.SubObject, StringComparison.Ordinal))
        {
            result = "native-drop-prefab-mismatch";
            return false;
        }

        if (sourceTemplate.PickablePrefab == null || !sourceTemplate.PickablePrefab.IsSet)
        {
            result = "native-source-pickable-prefab-not-set";
            return false;
        }

        if (customTemplate.PickablePrefab == null || !customTemplate.PickablePrefab.IsSet ||
            !string.Equals(sourceTemplate.PickablePrefab.AssetGUID, customTemplate.PickablePrefab.AssetGUID, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(sourceTemplate.PickablePrefab.SubObject, customTemplate.PickablePrefab.SubObject, StringComparison.Ordinal))
        {
            result = "native-pickable-prefab-mismatch";
            return false;
        }

        result = $"logic=native-template-components:{sourceComponentTypes.Length}; sourceTagsUnchanged=true; worldDrop=native-addressable:{sourceTemplate.DropPrefab.AssetGUID}:{sourceTemplate.DropPrefab.SubObject}; worldPickable=native-addressable:{sourceTemplate.PickablePrefab.AssetGUID}:{sourceTemplate.PickablePrefab.SubObject}";
        return true;
    }

    private static string[] GetComponentTypeNames(GameObject templateObject)
    {
        return templateObject
            .GetComponents<Component>()
            .Where(component => component != null)
            .Select(component => component.GetType().AssemblyQualifiedName ?? component.GetType().FullName ?? component.GetType().Name)
            .OrderBy(typeName => typeName, StringComparer.Ordinal)
            .ToArray();
    }

    private static bool TryGetMutationShopContext(Shop shop, Plugin plugin, out string shopGuid, out string shopLogName)
    {
        shopGuid = ShopGuid(shop);
        shopLogName = string.IsNullOrWhiteSpace(shopGuid) ? ShopName(shop) : shopGuid;
        string targetShopGuid = plugin.ShopStockTargetShopGuid.Trim();
        if (!string.IsNullOrWhiteSpace(targetShopGuid) && !string.Equals(shopGuid, targetShopGuid, StringComparison.OrdinalIgnoreCase))
        {
            plugin.ModLogger.LogInfo($"TaintedGemsShopStock skipped; shop={shopLogName}; targetShop={targetShopGuid}; reason=shop-guid-mismatch.");
            return false;
        }

        return true;
    }

    private static void RemoveInvalidCompressedStockRows(Shop shop)
    {
        Plugin? plugin = Plugin.Instance;
        if (plugin == null || !plugin.ShopStockEnabled || !IsConfiguredMutationShop(shop, plugin))
        {
            return;
        }

        foreach (RestockableStock stock in shop.Elements<RestockableStock>())
        {
            RemoveInvalidCompressedStockRows(shop, stock, plugin);
        }
    }

    private static void RemoveInvalidCompressedStockRows(Shop shop, Stock stock, Plugin plugin)
    {
        if (!stock.IsCompressed)
        {
            return;
        }

        List<ItemSpawningDataRuntime> compressedItems = GetCompressedItems(stock);
        int removedRows = 0;
        int removedQuantity = 0;
        for (int index = compressedItems.Count - 1; index >= 0; index--)
        {
            ItemSpawningDataRuntime? itemData = compressedItems[index];
            if (itemData != null && itemData.ItemTemplate != null)
            {
                continue;
            }

            removedRows++;
            removedQuantity += Math.Max(0, itemData?.quantity ?? 0);
            compressedItems.RemoveAt(index);
        }

        if (removedRows > 0)
        {
            plugin.ModLogger.LogWarning($"TaintedGemsShopStock removed invalid compressed rows; shop={ShopName(shop)}; stockType={stock.GetType().Name}; removedRows={removedRows}; removedQuantity={removedQuantity}; reason=item-template-null-before-create-all-items; validRowsUntouched=true.");
        }
    }

    private static bool IsConfiguredMutationShop(Shop shop, Plugin plugin)
    {
        string targetShopGuid = plugin.ShopStockTargetShopGuid.Trim();
        return string.IsNullOrWhiteSpace(targetShopGuid) ||
            string.Equals(ShopGuid(shop), targetShopGuid, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetTemplateLoader(out object? loader, out string reason)
    {
        loader = null;
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

            if (TemplatesProviderLoaderField == null)
            {
                reason = "templates-provider-loader-field-missing";
                return false;
            }

            loader = TemplatesProviderLoaderField.GetValue(provider);
            if (loader == null)
            {
                reason = "templates-loader-unavailable";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            reason = $"templates-loader-resolution-failed {ex.GetType().Name}: {ex.Message}";
            return false;
        }
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

            template = provider.GetAllOfType<ItemTemplate>()
                .FirstOrDefault(candidate => string.Equals(candidate.GUID, templateGuid, StringComparison.OrdinalIgnoreCase));

            if (template == null)
            {
                reason = "template-guid-not-found";
                return false;
            }

            if (template.IsAbstract || template.HiddenOnUI || template.CannotBeDropped)
            {
                reason = $"template-not-safe-visible-regular abstract={template.IsAbstract} hidden={template.HiddenOnUI} cannotDrop={template.CannotBeDropped}";
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

    private readonly struct BaseGemTemplateDescriptor
    {
        internal BaseGemTemplateDescriptor(string sourceGuid, string customGuid, string templateName, string thematicName)
        {
            SourceGuid = sourceGuid;
            CustomGuid = customGuid;
            TemplateName = templateName;
            ThematicName = thematicName;
        }

        internal string SourceGuid { get; }

        internal string CustomGuid { get; }

        internal string TemplateName { get; }

        internal string ThematicName { get; }

        internal string IconSlug
        {
            get
            {
                return TemplateName.StartsWith("ItemTemplate_Mod_TaintedGems_", StringComparison.Ordinal)
                    ? TemplateName.Substring("ItemTemplate_Mod_TaintedGems_".Length)
                    : CustomGuid;
            }
        }
    }

    internal readonly struct GemTemplateDescriptor
    {
        internal GemTemplateDescriptor(
            string sourceGuid,
            string customGuid,
            string templateName,
            string thematicName,
            string iconSlug,
            StoreGemKind kind)
        {
            SourceGuid = sourceGuid;
            CustomGuid = customGuid;
            TemplateName = templateName;
            ThematicName = thematicName;
            IconSlug = iconSlug;
            Kind = kind;
        }

        internal string SourceGuid { get; }

        internal string CustomGuid { get; }

        internal string TemplateName { get; }

        internal string ThematicName { get; }

        internal string IconSlug { get; }

        internal StoreGemKind Kind { get; }

        internal string KindLabel => Kind.ToString();

        internal bool HasSocketedGameplay => Kind == StoreGemKind.Skill || Kind == StoreGemKind.Perk;

        internal string EffectKindLabel => HasSocketedGameplay
            ? $"{EffectDefinition.VariableName}:{SkillEffect}"
            : SkillEffect.ToString();

        internal string DispositionLabel => HasSocketedGameplay
            ? Kind == StoreGemKind.Perk ? "Maladaptive" : "Adaptive"
            : "Inert";

        internal string SkillName => Kind switch
        {
            StoreGemKind.Skill => $"{DispositionLabel} Skill - {SkillTitle}",
            StoreGemKind.Perk => $"{DispositionLabel} Perk - {SkillTitle}",
            StoreGemKind.Trinket => $"Tainted Trinket - {BaseGemName}",
            StoreGemKind.Valuable => "Generic Valuable Gem",
            StoreGemKind.Junk => "Generic Junk Gem",
            _ => BaseGemName
        };

        internal string PerkName => Kind switch
        {
            StoreGemKind.Skill or StoreGemKind.Perk => SkillTitle,
            StoreGemKind.Trinket => $"{BaseGemName} Trinket",
            StoreGemKind.Valuable => "Generic Valuable Gem",
            StoreGemKind.Junk => "Generic Junk Gem",
            _ => BaseGemName
        };

        internal string ItemDescription
        {
            get
            {
                if (HasSocketedGameplay)
                {
                    return $"{SkillName}. {EffectDescription}";
                }

                return Kind switch
                {
                    StoreGemKind.Trinket => $"{SkillName}. Store version: Trinket. Valuable inert gem trinket; no socketed perk or skill behavior.",
                    StoreGemKind.Valuable => "Generic valuable gem. Store and world-display version; a polished inert gem for selling or collecting with no socketed perk or skill behavior.",
                    StoreGemKind.Junk => "Generic junk gem. A low-value inert gem splinter for selling or collecting with no socketed perk or skill behavior.",
                    _ => ThematicName
                };
            }
        }

        internal int BasePrice => Kind switch
        {
            StoreGemKind.Skill => 90 + PerkOrdinal * 3,
            StoreGemKind.Perk => 120 + PerkOrdinal * 4,
            StoreGemKind.Trinket => 40 + PerkOrdinal * 2,
            StoreGemKind.Valuable => 220,
            StoreGemKind.Junk => 6,
            _ => 25
        };

        internal int BuyPrice => Kind switch
        {
            StoreGemKind.Skill => 135 + PerkOrdinal * 4,
            StoreGemKind.Perk => 175 + PerkOrdinal * 5,
            StoreGemKind.Trinket => 65 + PerkOrdinal * 3,
            StoreGemKind.Valuable => 330,
            StoreGemKind.Junk => 10,
            _ => 30
        };

        internal float PriceLevelMultiplier => Kind switch
        {
            StoreGemKind.Skill or StoreGemKind.Perk => 0.5f + (PerkOrdinal % 6) * 0.1f,
            StoreGemKind.Trinket => 0.2f,
            StoreGemKind.Valuable => 0.3f,
            StoreGemKind.Junk => 0f,
            _ => 0f
        };

        internal bool SilentSneak => SkillEffect == TaintedGemSkillEffect.SilentSneak;

        internal float JumpVelocityMultiplier => SkillEffect == TaintedGemSkillEffect.JumpLaunch ||
            SkillEffect == TaintedGemSkillEffect.JumpDrag
                ? EffectMultiplier
                : 1f;

        internal float ShopBuyPriceMultiplier
        {
            get
            {
                return SkillEffect switch
                {
                    TaintedGemSkillEffect.ShopDiscount => EffectMultiplier,
                    TaintedGemSkillEffect.ShopSurcharge => EffectMultiplier,
                    _ => 1f
                };
            }
        }

        internal float HeroSellPriceMultiplier
        {
            get
            {
                return SkillEffect switch
                {
                    TaintedGemSkillEffect.SellPremium => EffectMultiplier,
                    TaintedGemSkillEffect.SellPenalty => EffectMultiplier,
                    _ => 1f
                };
            }
        }

        internal float ItemStaminaCostMultiplier
        {
            get
            {
                return SkillEffect switch
                {
                    TaintedGemSkillEffect.WeaponCostDiscount => EffectMultiplier,
                    TaintedGemSkillEffect.WeaponCostSurcharge => EffectMultiplier,
                    _ => 1f
                };
            }
        }

        internal bool IsMaladaptive => Kind == StoreGemKind.Perk;

        private string BaseGemName => ThematicName.StartsWith("Tainted ", StringComparison.Ordinal)
            ? ThematicName.Substring("Tainted ".Length)
            : ThematicName;

        private TaintedGemSkillEffect SkillEffect
        {
            get
            {
                if (!HasSocketedGameplay)
                {
                    return TaintedGemSkillEffect.None;
                }

                return Kind == StoreGemKind.Perk
                    ? EffectDefinition.NegativeEffect
                    : EffectDefinition.PositiveEffect;
            }
        }

        private string SkillTitle
        {
            get
            {
                if (!HasSocketedGameplay)
                {
                    return BaseGemName;
                }

                string title = Kind == StoreGemKind.Perk
                    ? EffectDefinition.NegativeTitle
                    : EffectDefinition.PositiveTitle;
                return $"{BaseGemName} {title}";
            }
        }

        private string EffectDescription
        {
            get
            {
                if (!HasSocketedGameplay)
                {
                    return "Effect: inert.";
                }

                string prefix = IsMaladaptive ? "Maladaptive effect" : "Effect";
                string flavor = IsMaladaptive ? EffectDefinition.NegativeFlavor : EffectDefinition.PositiveFlavor;
                return SkillEffect switch
                {
                    TaintedGemSkillEffect.SilentSneak => $"{prefix}: {flavor}; mutes footstep sound and crouch movement noise while sneaking.",
                    TaintedGemSkillEffect.JumpLaunch or TaintedGemSkillEffect.JumpDrag => $"{prefix}: {flavor}; jump launch {SignedPercent(EffectMultiplier - 1f)}.",
                    TaintedGemSkillEffect.ShopDiscount or TaintedGemSkillEffect.ShopSurcharge => $"{prefix}: {flavor}; shop buy prices {SignedPercent(EffectMultiplier - 1f)}.",
                    TaintedGemSkillEffect.SellPremium or TaintedGemSkillEffect.SellPenalty => $"{prefix}: {flavor}; sale prices {SignedPercent(EffectMultiplier - 1f)}.",
                    TaintedGemSkillEffect.WeaponCostDiscount or TaintedGemSkillEffect.WeaponCostSurcharge => $"{prefix}: {flavor}; equipped weapon stamina costs {SignedPercent(EffectMultiplier - 1f)}.",
                    _ => "Effect: inert."
                };
            }
        }

        private float EffectMultiplier => Kind == StoreGemKind.Perk
            ? EffectDefinition.NegativeMultiplier
            : EffectDefinition.PositiveMultiplier;

        private TaintedGemEffectDefinition EffectDefinition
        {
            get
            {
                int index = Math.Max(0, Math.Min(PerkOrdinal - 1, TaintedGemEffectTable.Length - 1));
                return TaintedGemEffectTable[index];
            }
        }

        private static readonly TaintedGemEffectDefinition[] TaintedGemEffectTable =
        {
            new("Ghost-Step", TaintedGemSkillEffect.SilentSneak, 1f, "Ghost-Step Slippers", "the gem teaches your boots to apologize before existing", TaintedGemSkillEffect.ShopSurcharge, 1.06f, "Ghost-Step Backfire", "the shopkeeper notices the invisible footprints on your bill"),
            new("Ghost-Step", TaintedGemSkillEffect.SilentSneak, 1f, "Ghost-Step Slippers", "the gem teaches your boots to apologize before existing", TaintedGemSkillEffect.ShopSurcharge, 1.06f, "Ghost-Step Backfire", "the shopkeeper notices the invisible footprints on your bill"),
            new("Spring-Knees", TaintedGemSkillEffect.JumpLaunch, 1.12f, "Spring-Knees", "the gem files a formal complaint against gravity", TaintedGemSkillEffect.JumpDrag, 0.9f, "Soggy Knees", "gravity wins the appeal"),
            new("Spring-Knees", TaintedGemSkillEffect.JumpLaunch, 1.12f, "Spring-Knees", "the gem files a formal complaint against gravity", TaintedGemSkillEffect.JumpDrag, 0.9f, "Soggy Knees", "gravity wins the appeal"),
            new("Smuggler Coupon", TaintedGemSkillEffect.ShopDiscount, 0.92f, "Smuggler Coupon", "the price tag looks away first", TaintedGemSkillEffect.ShopSurcharge, 1.08f, "Counterfeit Coupon", "the price tag calls for witnesses"),
            new("Smuggler Coupon", TaintedGemSkillEffect.ShopDiscount, 0.92f, "Smuggler Coupon", "the price tag looks away first", TaintedGemSkillEffect.ShopSurcharge, 1.08f, "Counterfeit Coupon", "the price tag calls for witnesses"),
            new("Honest Liar Appraisal", TaintedGemSkillEffect.SellPremium, 1.07f, "Honest Liar Appraisal", "your junk gives a confident little speech", TaintedGemSkillEffect.SellPenalty, 0.93f, "Honest Liar Audit", "your junk confesses under pressure"),
            new("Honest Liar Appraisal", TaintedGemSkillEffect.SellPremium, 1.07f, "Honest Liar Appraisal", "your junk gives a confident little speech", TaintedGemSkillEffect.SellPenalty, 0.93f, "Honest Liar Audit", "your junk confesses under pressure"),
            new("Pocket Fog", TaintedGemSkillEffect.SilentSneak, 1f, "Pocket Fog", "your crouch comes with its own legal silence", TaintedGemSkillEffect.SellPenalty, 0.95f, "Pocket Fog Receipt", "nobody can hear your sales pitch either"),
            new("Pocket Fog", TaintedGemSkillEffect.SilentSneak, 1f, "Pocket Fog", "your crouch comes with its own legal silence", TaintedGemSkillEffect.SellPenalty, 0.95f, "Pocket Fog Receipt", "nobody can hear your sales pitch either"),
            new("Emergency Kneecaps", TaintedGemSkillEffect.JumpLaunch, 1.15f, "Emergency Kneecaps", "your legs find an extra gear they refuse to explain", TaintedGemSkillEffect.JumpDrag, 0.88f, "Emergency Knee Debt", "your legs remember the invoice"),
            new("Emergency Kneecaps", TaintedGemSkillEffect.JumpLaunch, 1.15f, "Emergency Kneecaps", "your legs find an extra gear they refuse to explain", TaintedGemSkillEffect.JumpDrag, 0.88f, "Emergency Knee Debt", "your legs remember the invoice"),
            new("Counterfeit Smile", TaintedGemSkillEffect.ShopDiscount, 0.9f, "Counterfeit Smile", "the merchant trusts the grin more than the math", TaintedGemSkillEffect.ShopSurcharge, 1.1f, "Counterfeit Frown", "the merchant checks the grin for serial numbers"),
            new("Counterfeit Smile", TaintedGemSkillEffect.ShopDiscount, 0.9f, "Counterfeit Smile", "the merchant trusts the grin more than the math", TaintedGemSkillEffect.ShopSurcharge, 1.1f, "Counterfeit Frown", "the merchant checks the grin for serial numbers"),
            new("Heroic Receipt", TaintedGemSkillEffect.SellPremium, 1.08f, "Heroic Receipt", "your goods arrive with flattering paperwork", TaintedGemSkillEffect.SellPenalty, 0.92f, "Heroic Fine Print", "the paperwork starts flattering the buyer instead"),
            new("Heroic Receipt", TaintedGemSkillEffect.SellPremium, 1.08f, "Heroic Receipt", "your goods arrive with flattering paperwork", TaintedGemSkillEffect.SellPenalty, 0.92f, "Heroic Fine Print", "the paperwork starts flattering the buyer instead"),
            new("Damp Socks", TaintedGemSkillEffect.SilentSneak, 1f, "Damp Socks", "the floor refuses to squeak out of pity", TaintedGemSkillEffect.ShopSurcharge, 1.05f, "Damp Sock Tax", "the merchant charges a moisture handling fee"),
            new("Damp Socks", TaintedGemSkillEffect.SilentSneak, 1f, "Damp Socks", "the floor refuses to squeak out of pity", TaintedGemSkillEffect.ShopSurcharge, 1.05f, "Damp Sock Tax", "the merchant charges a moisture handling fee"),
            new("Ceiling Argument", TaintedGemSkillEffect.JumpLaunch, 1.09f, "Ceiling Argument", "you briefly win a debate with the roof", TaintedGemSkillEffect.JumpDrag, 0.93f, "Floor Apology", "you apologize to the floor before leaving it"),
            new("Ceiling Argument", TaintedGemSkillEffect.JumpLaunch, 1.09f, "Ceiling Argument", "you briefly win a debate with the roof", TaintedGemSkillEffect.JumpDrag, 0.93f, "Floor Apology", "you apologize to the floor before leaving it"),
            new("Manager Whisper", TaintedGemSkillEffect.ShopDiscount, 0.94f, "Manager Whisper", "the gem says it knows someone upstairs", TaintedGemSkillEffect.ShopSurcharge, 1.06f, "Manager Shout", "the upstairs person denies everything loudly"),
            new("Manager Whisper", TaintedGemSkillEffect.ShopDiscount, 0.94f, "Manager Whisper", "the gem says it knows someone upstairs", TaintedGemSkillEffect.ShopSurcharge, 1.06f, "Manager Shout", "the upstairs person denies everything loudly"),
            new("Lucky Pawn Ticket", TaintedGemSkillEffect.SellPremium, 1.06f, "Lucky Pawn Ticket", "the buyer squints and sees treasure", TaintedGemSkillEffect.SellPenalty, 0.94f, "Unlucky Pawn Ticket", "the buyer squints and sees chores"),
            new("Lucky Pawn Ticket", TaintedGemSkillEffect.SellPremium, 1.06f, "Lucky Pawn Ticket", "the buyer squints and sees treasure", TaintedGemSkillEffect.SellPenalty, 0.94f, "Unlucky Pawn Ticket", "the buyer squints and sees chores"),
            new("Library Feet", TaintedGemSkillEffect.SilentSneak, 1f, "Library Feet", "your crouch obeys imaginary librarians", TaintedGemSkillEffect.SellPenalty, 0.93f, "Library Late Fee", "your valuables are overdue and everyone knows"),
            new("Library Feet", TaintedGemSkillEffect.SilentSneak, 1f, "Library Feet", "your crouch obeys imaginary librarians", TaintedGemSkillEffect.SellPenalty, 0.93f, "Library Late Fee", "your valuables are overdue and everyone knows"),
            new("Launch Complaint", TaintedGemSkillEffect.JumpLaunch, 1.18f, "Launch Complaint", "the ground receives a strongly worded boot", TaintedGemSkillEffect.JumpDrag, 0.86f, "Landing Complaint", "the ground writes back"),
            new("Launch Complaint", TaintedGemSkillEffect.JumpLaunch, 1.18f, "Launch Complaint", "the ground receives a strongly worded boot", TaintedGemSkillEffect.JumpDrag, 0.86f, "Landing Complaint", "the ground writes back"),
            new("Discount Grease", TaintedGemSkillEffect.WeaponCostDiscount, 0.88f, "Discount Grease", "weapon swings argue for a lower bill", TaintedGemSkillEffect.WeaponCostSurcharge, 1.12f, "Surcharge Grease", "the hilt charges interest every time you move it"),
            new("Discount Grease", TaintedGemSkillEffect.WeaponCostDiscount, 0.88f, "Discount Grease", "weapon swings argue for a lower bill", TaintedGemSkillEffect.WeaponCostSurcharge, 1.12f, "Surcharge Grease", "the hilt charges interest every time you move it"),
            new("Royal Overpayment", TaintedGemSkillEffect.SellPremium, 1.1f, "Royal Overpayment", "your sale gets mistaken for a ceremonial donation", TaintedGemSkillEffect.SellPenalty, 0.9f, "Royal Underpayment", "the ceremony bills you for chairs"),
            new("Royal Overpayment", TaintedGemSkillEffect.SellPremium, 1.1f, "Royal Overpayment", "your sale gets mistaken for a ceremonial donation", TaintedGemSkillEffect.SellPenalty, 0.9f, "Royal Underpayment", "the ceremony bills you for chairs"),
            new("Apology Slippers", TaintedGemSkillEffect.SilentSneak, 1f, "Apology Slippers", "your footsteps apologize before making noise", TaintedGemSkillEffect.ShopSurcharge, 1.09f, "Apology Surcharge", "the apology comes with a processing charge"),
            new("Apology Slippers", TaintedGemSkillEffect.SilentSneak, 1f, "Apology Slippers", "your footsteps apologize before making noise", TaintedGemSkillEffect.ShopSurcharge, 1.09f, "Apology Surcharge", "the apology comes with a processing charge"),
            new("Shiny Spine", TaintedGemSkillEffect.JumpLaunch, 1.11f, "Shiny Spine", "your backbone volunteers for lift duty", TaintedGemSkillEffect.JumpDrag, 0.91f, "Bent Spine", "your backbone requests a chair"),
            new("Shiny Spine", TaintedGemSkillEffect.JumpLaunch, 1.11f, "Shiny Spine", "your backbone volunteers for lift duty", TaintedGemSkillEffect.JumpDrag, 0.91f, "Bent Spine", "your backbone requests a chair"),
            new("Unpaid Invoice", TaintedGemSkillEffect.ShopDiscount, 0.93f, "Unpaid Invoice", "the merchant subtracts money just to end the conversation", TaintedGemSkillEffect.ShopSurcharge, 1.07f, "Invoice Revenge", "the conversation sends a second invoice"),
            new("Unpaid Invoice", TaintedGemSkillEffect.ShopDiscount, 0.93f, "Unpaid Invoice", "the merchant subtracts money just to end the conversation", TaintedGemSkillEffect.ShopSurcharge, 1.07f, "Invoice Revenge", "the conversation sends a second invoice"),
            new("Suspicious Certificate", TaintedGemSkillEffect.SellPremium, 1.12f, "Suspicious Certificate", "your goods become certified probably real", TaintedGemSkillEffect.SellPenalty, 0.88f, "Suspicious Revocation", "the certificate gets certified as suspicious"),
            new("Suspicious Certificate", TaintedGemSkillEffect.SellPremium, 1.12f, "Suspicious Certificate", "your goods become certified probably real", TaintedGemSkillEffect.SellPenalty, 0.88f, "Suspicious Revocation", "the certificate gets certified as suspicious"),
            new("Final Wink", TaintedGemSkillEffect.WeaponCostDiscount, 0.85f, "Final Wink", "your weapon accepts a suspicious stamina coupon", TaintedGemSkillEffect.WeaponCostSurcharge, 1.15f, "Final Blink", "your weapon asks for overtime pay"),
            new("Final Wink", TaintedGemSkillEffect.WeaponCostDiscount, 0.85f, "Final Wink", "your weapon accepts a suspicious stamina coupon", TaintedGemSkillEffect.WeaponCostSurcharge, 1.15f, "Final Blink", "your weapon asks for overtime pay")
        };

        private int PerkOrdinal
        {
            get
            {
                string suffix = CustomGuid.Length >= 2 ? CustomGuid.Substring(CustomGuid.Length - 2) : CustomGuid;
                return int.TryParse(suffix, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int value) && value > 0
                    ? value
                    : 1;
            }
        }

        internal string CustomIconAddress
        {
            get
            {
                return Plugin.CustomIconAddressPrefix + IconSlug;
            }
        }

        private static string SignedPercent(float value)
        {
            string sign = value >= 0f ? "+" : string.Empty;
            return sign + (value * 100f).ToString("0.#", CultureInfo.InvariantCulture) + "%";
        }

        private readonly struct TaintedGemEffectDefinition
        {
            internal TaintedGemEffectDefinition(
                string variableName,
                TaintedGemSkillEffect positiveEffect,
                float positiveMultiplier,
                string positiveTitle,
                string positiveFlavor,
                TaintedGemSkillEffect negativeEffect,
                float negativeMultiplier,
                string negativeTitle,
                string negativeFlavor)
            {
                VariableName = variableName;
                PositiveEffect = positiveEffect;
                PositiveMultiplier = positiveMultiplier;
                PositiveTitle = positiveTitle;
                PositiveFlavor = positiveFlavor;
                NegativeEffect = negativeEffect;
                NegativeMultiplier = negativeMultiplier;
                NegativeTitle = negativeTitle;
                NegativeFlavor = negativeFlavor;
            }

            internal string VariableName { get; }

            internal TaintedGemSkillEffect PositiveEffect { get; }

            internal float PositiveMultiplier { get; }

            internal string PositiveTitle { get; }

            internal string PositiveFlavor { get; }

            internal TaintedGemSkillEffect NegativeEffect { get; }

            internal float NegativeMultiplier { get; }

            internal string NegativeTitle { get; }

            internal string NegativeFlavor { get; }
        }
    }

    internal enum StoreGemKind
    {
        Skill,
        Perk,
        Trinket,
        Valuable,
        Junk
    }

    internal enum TaintedGemSkillEffect
    {
        None,
        SilentSneak,
        JumpLaunch,
        JumpDrag,
        ShopDiscount,
        SellPremium,
        ShopSurcharge,
        SellPenalty,
        WeaponCostDiscount,
        WeaponCostSurcharge
    }
}

[HarmonyPatch(typeof(TemplatesLoader), nameof(TemplatesLoader.FinishedLoading), MethodType.Setter)]
internal static class TaintedGemsTemplateLoaderFinishedPatch
{
    [HarmonyPostfix]
    private static void Postfix(bool value)
    {
        if (value)
        {
            TaintedGemsShopPatch.RegisterCustomGemTemplatesAfterLoaderFinished();
        }
    }
}

[HarmonyPatch(typeof(ShopUI), "OnFullyInitialized")]
internal static class TaintedGemsBeforeShopUiPatch
{
    [HarmonyPrefix]
    private static void Prefix(ShopUI __instance)
    {
        TaintedGemsShopPatch.PrepareShopStockBeforeUi(__instance.Shop);
    }
}
