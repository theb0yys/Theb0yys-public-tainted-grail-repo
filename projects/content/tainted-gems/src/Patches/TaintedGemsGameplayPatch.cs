using System;
using System.Reflection;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Combat;
using Awaken.TG.Main.Heroes.FootSteps;
using Awaken.TG.Main.Heroes.Items;
using Awaken.TG.Main.Heroes.Items.Gems;
using Awaken.TG.Main.Heroes.MovementSystems;
using Awaken.TG.Main.Heroes.Stats;
using Awaken.TG.Main.Heroes.Thievery;
using Awaken.TG.Main.Locations.Shops;
using Awaken.TG.Main.Locations.Shops.Prices;
using HarmonyLib;
using UnityEngine;

namespace TaintedGems.Patches;

internal static class TaintedGemsGameplay
{
    private static int CachedFrame = -1;
    private static ActiveTaintedGemPerks CachedPerks;
    private static bool ActiveGemWarningLogged;

    internal static bool TryGetActivePerks(out ActiveTaintedGemPerks perks)
    {
        int frame = Time.frameCount;
        if (CachedFrame == frame)
        {
            perks = CachedPerks;
            return perks.HasAny;
        }

        CachedFrame = frame;
        CachedPerks = default;

        try
        {
            BuildActivePerks(ref CachedPerks);
            CachedPerks.Clamp();
        }
        catch (Exception ex)
        {
            if (!ActiveGemWarningLogged)
            {
                ActiveGemWarningLogged = true;
                Plugin.Instance?.ModLogger.LogWarning($"TaintedGemsGameplay active perk scan failed; error={ex.GetType().Name}: {ex.Message}");
            }

            CachedPerks = default;
        }

        perks = CachedPerks;
        return perks.HasAny;
    }

    internal static bool ShouldMuteSneaking()
    {
        if (!TryGetHero(out Hero? hero) || hero == null)
        {
            return false;
        }

        return hero.IsCrouching &&
            TryGetActivePerks(out ActiveTaintedGemPerks perks) &&
            perks.SilentSneak;
    }

    internal static bool ShouldMuteSneaking(int isCrouching)
    {
        return isCrouching != 0 &&
            TryGetActivePerks(out ActiveTaintedGemPerks perks) &&
            perks.SilentSneak;
    }

    internal static bool IsCurrentHeroMerchant(IMerchant? merchant)
    {
        return merchant != null &&
            TryGetHero(out Hero? hero) &&
            hero != null &&
            ReferenceEquals(merchant, hero);
    }

    internal static bool TryGetItemStaminaCostMultiplier(ItemStat? stat, out float multiplier)
    {
        multiplier = 1f;

        if (stat == null ||
            !IsWeaponStaminaCostStat(stat.Type) ||
            stat.Owner is not Item item ||
            !IsEquippedByCurrentHero(item) ||
            !TryGetActivePerks(out ActiveTaintedGemPerks perks) ||
            Mathf.Approximately(perks.ItemStaminaCostMultiplier, 1f))
        {
            return false;
        }

        multiplier = perks.ItemStaminaCostMultiplier;
        return true;
    }

    private static void BuildActivePerks(ref ActiveTaintedGemPerks perks)
    {
        if (!TryGetHero(out Hero? hero) || hero == null)
        {
            return;
        }

        HeroItems? heroItems = hero.HeroItems;
        if (heroItems == null)
        {
            return;
        }

        foreach (EquipmentSlotType slot in EquipmentSlotType.All)
        {
            Item? item = heroItems.EquippedItem(slot);
            AddAttachedTaintedGems(item, ref perks);
        }
    }

    private static void AddAttachedTaintedGems(Item? item, ref ActiveTaintedGemPerks perks)
    {
        if (item == null || item.HasBeenDiscarded)
        {
            return;
        }

        foreach (GemAttached gem in item.Elements<GemAttached>())
        {
            string? templateGuid = gem?.Template?.GUID;
            if (TaintedGemsShopPatch.TryGetTaintedGemDescriptor(templateGuid, out TaintedGemsShopPatch.GemTemplateDescriptor descriptor))
            {
                perks.Add(descriptor);
            }
        }
    }

    private static bool TryGetHero(out Hero? hero)
    {
        hero = null;
        try
        {
            hero = Hero.Current;
            return hero != null && !hero.HasBeenDiscarded;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsWeaponStaminaCostStat(StatType statType)
    {
        return statType == ItemStatType.LightAttackCost ||
            statType == ItemStatType.HeavyAttackCost ||
            statType == ItemStatType.HeavyAttackHoldCostPerTick ||
            statType == ItemStatType.DrawBowCostPerTick ||
            statType == ItemStatType.HoldItemCostPerTick ||
            statType == ItemStatType.PushStaminaCost ||
            statType == ItemStatType.BlockStaminaCostMultiplier ||
            statType == ItemStatType.ParryStaminaCost;
    }

    private static bool IsEquippedByCurrentHero(Item item)
    {
        if (!TryGetHero(out Hero? hero) || hero == null)
        {
            return false;
        }

        if (!ReferenceEquals(item.Owner, hero))
        {
            return false;
        }

        HeroItems? heroItems = hero.HeroItems;
        if (heroItems == null)
        {
            return false;
        }

        foreach (EquipmentSlotType slot in EquipmentSlotType.All)
        {
            if (ReferenceEquals(heroItems.EquippedItem(slot), item))
            {
                return true;
            }
        }

        return false;
    }
}

internal struct ActiveTaintedGemPerks
{
    internal bool HasAny { get; private set; }

    internal bool SilentSneak { get; private set; }

    internal float JumpVelocityMultiplier { get; private set; }

    internal float ShopBuyPriceMultiplier { get; private set; }

    internal float HeroSellPriceMultiplier { get; private set; }

    internal float ItemStaminaCostMultiplier { get; private set; }

    internal int AdaptiveCount { get; private set; }

    internal int MaladaptiveCount { get; private set; }

    internal void Add(TaintedGemsShopPatch.GemTemplateDescriptor descriptor)
    {
        if (!HasAny)
        {
            HasAny = true;
            JumpVelocityMultiplier = 1f;
            ShopBuyPriceMultiplier = 1f;
            HeroSellPriceMultiplier = 1f;
            ItemStaminaCostMultiplier = 1f;
        }

        SilentSneak |= descriptor.SilentSneak;
        JumpVelocityMultiplier *= descriptor.JumpVelocityMultiplier;
        ShopBuyPriceMultiplier *= descriptor.ShopBuyPriceMultiplier;
        HeroSellPriceMultiplier *= descriptor.HeroSellPriceMultiplier;
        ItemStaminaCostMultiplier *= descriptor.ItemStaminaCostMultiplier;

        if (descriptor.IsMaladaptive)
        {
            MaladaptiveCount++;
        }
        else
        {
            AdaptiveCount++;
        }
    }

    internal void Clamp()
    {
        if (!HasAny)
        {
            return;
        }

        JumpVelocityMultiplier = Mathf.Clamp(JumpVelocityMultiplier, 0.65f, 1.65f);
        ShopBuyPriceMultiplier = Mathf.Clamp(ShopBuyPriceMultiplier, 0.65f, 1.35f);
        HeroSellPriceMultiplier = Mathf.Clamp(HeroSellPriceMultiplier, 0.65f, 1.35f);
        ItemStaminaCostMultiplier = Mathf.Clamp(ItemStaminaCostMultiplier, 0.65f, 1.35f);
    }

    internal int ApplyShopBuyPrice(int price)
    {
        return Mathf.Max(1, Mathf.RoundToInt(price * ShopBuyPriceMultiplier));
    }

    internal int ApplyHeroSellPrice(int price)
    {
        return Mathf.Max(1, Mathf.RoundToInt(price * HeroSellPriceMultiplier));
    }
}

[HarmonyPatch(typeof(ItemStat), nameof(ItemStat.ModifiedValue), MethodType.Getter)]
internal static class TaintedGemsItemStaminaCostPatch
{
    private static void Postfix(ItemStat __instance, ref float __result)
    {
        if (TaintedGemsGameplay.TryGetItemStaminaCostMultiplier(__instance, out float multiplier))
        {
            __result *= multiplier;
        }
    }
}

[HarmonyPatch(typeof(VHeroFootsteps), "FootStep")]
internal static class TaintedGemsHeroFootstepPatch
{
    private static bool Prefix(int isCrouching)
    {
        return !TaintedGemsGameplay.ShouldMuteSneaking(isCrouching);
    }
}

[HarmonyPatch(typeof(ThieveryNoise), nameof(ThieveryNoise.MakeNoiseByHero))]
internal static class TaintedGemsThieveryNoisePatch
{
    private static bool Prefix()
    {
        return !TaintedGemsGameplay.ShouldMuteSneaking();
    }
}

[HarmonyPatch(typeof(HumanoidMovementBase), "MakeMovementSound")]
internal static class TaintedGemsMovementSoundPatch
{
    private static bool Prefix()
    {
        return !TaintedGemsGameplay.ShouldMuteSneaking();
    }
}

[HarmonyPatch(typeof(HumanoidMovementBase), "Jump")]
internal static class TaintedGemsJumpPatch
{
    private static readonly MethodInfo? ControllerGetter = AccessTools.PropertyGetter(typeof(HeroMovementSystem), "Controller");
    private static bool ControllerWarningLogged;

    private static void Postfix(HumanoidMovementBase __instance)
    {
        if (!TaintedGemsGameplay.TryGetActivePerks(out ActiveTaintedGemPerks perks) ||
            Mathf.Approximately(perks.JumpVelocityMultiplier, 1f))
        {
            return;
        }

        try
        {
            if (ControllerGetter?.Invoke(__instance, null) is VHeroController controller)
            {
                controller.verticalVelocity *= perks.JumpVelocityMultiplier;
            }
        }
        catch (Exception ex)
        {
            if (!ControllerWarningLogged)
            {
                ControllerWarningLogged = true;
                Plugin.Instance?.ModLogger.LogWarning($"TaintedGemsGameplay jump multiplier failed; error={ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}

[HarmonyPatch(typeof(DefaultPriceProvider), nameof(DefaultPriceProvider.SellPrice))]
internal static class TaintedGemsDefaultPriceProviderPatch
{
    private static void Postfix(IMerchant buyer, Item item, ref int __result)
    {
        if (!TaintedGemsGameplay.IsCurrentHeroMerchant(buyer) ||
            !TaintedGemsGameplay.TryGetActivePerks(out ActiveTaintedGemPerks perks))
        {
            return;
        }

        __result = perks.ApplyShopBuyPrice(__result);
    }
}

[HarmonyPatch(typeof(HeroPriceProvider), nameof(HeroPriceProvider.SellPrice))]
internal static class TaintedGemsHeroPriceProviderPatch
{
    private static void Postfix(IMerchant buyer, Item item, ref int __result)
    {
        if (TaintedGemsGameplay.TryGetActivePerks(out ActiveTaintedGemPerks perks))
        {
            __result = perks.ApplyHeroSellPrice(__result);
        }
    }
}
