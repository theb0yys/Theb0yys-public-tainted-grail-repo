using System;
using System.Collections.Generic;
using AvalonBroodmotherCompanion.AI.Package.V2;

namespace AvalonBroodmotherCompanion;

internal enum SpiderFamilyCompanionTier
{
    Broodmother,
    Spider,
}

internal sealed class SpiderFamilyCompanionDefinition
{
    internal SpiderFamilyCompanionDefinition(
        string id,
        SpiderFamilyCompanionTier tier,
        string variantId,
        string displayName,
        string spellTemplateGuid,
        string spellTemplateName,
        string spellDisplayName,
        string spellDisplayDescription,
        string spellLightCastDescription,
        string spellHeavyCastDescription,
        string locationTemplateName,
        string locationTemplateGuid,
        string npcTemplateGuid,
        string resolverMethodName,
        string visualAddress,
        float visualScaleMultiplier,
        string aiActorRoleId,
        BroodmotherIconVariant? iconVariant,
        string? templateVisualAddress = null,
        string? runtimeOverlayVisualAddress = null)
    {
        Id = id;
        Tier = tier;
        VariantId = variantId;
        DisplayName = displayName;
        SpellTemplateGuid = spellTemplateGuid;
        SpellTemplateName = spellTemplateName;
        SpellDisplayName = spellDisplayName;
        SpellDisplayDescription = spellDisplayDescription;
        SpellLightCastDescription = spellLightCastDescription;
        SpellHeavyCastDescription = spellHeavyCastDescription;
        LocationTemplateName = locationTemplateName;
        LocationTemplateGuid = locationTemplateGuid;
        NpcTemplateGuid = npcTemplateGuid;
        ResolverMethodName = resolverMethodName;
        VisualAddress = visualAddress;
        TemplateVisualAddress = templateVisualAddress ?? visualAddress;
        RuntimeOverlayVisualAddress = runtimeOverlayVisualAddress;
        VisualScaleMultiplier = visualScaleMultiplier;
        AiActorRoleId = aiActorRoleId;
        IconVariant = iconVariant;
    }

    internal string Id { get; }
    internal SpiderFamilyCompanionTier Tier { get; }
    internal string VariantId { get; }
    internal string DisplayName { get; }
    internal string SpellTemplateGuid { get; }
    internal string SpellTemplateName { get; }
    internal string SpellDisplayName { get; }
    internal string SpellDisplayDescription { get; }
    internal string SpellLightCastDescription { get; }
    internal string SpellHeavyCastDescription { get; }
    internal string LocationTemplateName { get; }
    internal string LocationTemplateGuid { get; }
    internal string NpcTemplateGuid { get; }
    internal string ResolverMethodName { get; }
    internal string VisualAddress { get; }
    internal string TemplateVisualAddress { get; }
    internal string? RuntimeOverlayVisualAddress { get; }
    internal float VisualScaleMultiplier { get; }
    internal string AiActorRoleId { get; }
    internal BroodmotherIconVariant? IconVariant { get; }
    internal bool UsesSpiderCombatPackage =>
        Tier == SpiderFamilyCompanionTier.Broodmother ||
        Tier == SpiderFamilyCompanionTier.Spider;
}

internal static class SpiderFamilyCompanionDefinitions
{
    internal const string BroodmotherLocationTemplateName = "Spec_Broodmother_CI4";
    internal const string BroodmotherLocationTemplateGuid = "527d142b35e81a648b6e84ea921ac543";
    internal const string BroodmotherNpcTemplateGuid = "7623d4729979e814ea64992d9a399e17";
    internal const string BroodmotherResolverMethodName = "TryResolveWorldBroodmotherTemplate";

    internal const string SpiderLocationTemplateName = "Spec_Spider_CI4";
    internal const string SpiderLocationTemplateGuid = "bfcd6db0f66afc14c94fcb6a9dbde4e7";
    internal const string SpiderNpcTemplateGuid = "5f36e2717dc68e04091fe7b0aa8bafe0";
    internal const string SpiderResolverMethodName = "TryResolveWorldSpiderTemplate";

    internal static readonly SpiderFamilyCompanionDefinition BroodmotherSkin1 = new(
        "broodmother-skin1-1",
        SpiderFamilyCompanionTier.Broodmother,
        "skin1-1",
        "Broodmother Asha - Crimson Vanguard",
        "b7d0d4e6c0de4bb0a000000000000001",
        "ItemTemplate_Magic_Tier1_AvalonBroodmotherCall",
        "Broodmother's Call - Asha, Crimson Vanguard",
        "Calls Broodmother Asha, the Crimson Vanguard: a hulking red-black ally built for close bite pressure and holding the enemy's face. Only one Spider/Broodmother family companion from this mod can be active; another call replaces the current companion.",
        "Summons or replaces Asha, a front-line Broodmother that closes fast and keeps bite pressure on the nearest threat.",
        "Replaces your current spider-family ally with Broodmother Asha, anchoring the fight with guarded close-range pressure.",
        BroodmotherLocationTemplateName,
        BroodmotherLocationTemplateGuid,
        BroodmotherNpcTemplateGuid,
        BroodmotherResolverMethodName,
        "avalon-awakened/creatures/spider/broodmother/skin1-1/visual-v1",
        5.5f,
        BroodmotherCompanionAiV2Contract.CrimsonVanguardRole.Value,
        BroodmotherIconVariant.CurrentRedBlack);

    internal static readonly SpiderFamilyCompanionDefinition BroodmotherSkin2 = new(
        "broodmother-skin2-1",
        SpiderFamilyCompanionTier.Broodmother,
        "skin2-1",
        "Broodmother Velra - Pale Pouncer",
        "b7d0d4e6c0de4bb0a000000000000002",
        "ItemTemplate_Magic_Tier1_AvalonBroodmotherCallSkin2",
        "Broodmother's Call - Velra, Pale Pouncer",
        "Calls Broodmother Velra, the Pale Pouncer: a white-chitin flanker tuned for leap pressure and side-angle strikes. Only one Spider/Broodmother family companion from this mod can be active; another call replaces the current companion.",
        "Summons or replaces Velra, a leaping Broodmother that circles wide before crashing into exposed enemies.",
        "Replaces your current spider-family ally with Broodmother Velra, trading a steady front line for disruptive flanking leaps.",
        BroodmotherLocationTemplateName,
        BroodmotherLocationTemplateGuid,
        BroodmotherNpcTemplateGuid,
        BroodmotherResolverMethodName,
        "avalon-awakened/creatures/spider/broodmother/skin2-1/visual-v1",
        5.5f,
        BroodmotherCompanionAiV2Contract.PalePouncerRole.Value,
        BroodmotherIconVariant.ReservedPale);

    internal static readonly SpiderFamilyCompanionDefinition BroodmotherSkin3 = new(
        "broodmother-skin3-1",
        SpiderFamilyCompanionTier.Broodmother,
        "skin3-1",
        "Broodmother Aurex - Gilded Spitter",
        "b7d0d4e6c0de4bb0a000000000000003",
        "ItemTemplate_Magic_Tier1_AvalonBroodmotherCallSkin3",
        "Broodmother's Call - Aurex, Gilded Spitter",
        "Calls Broodmother Aurex, the Gilded Spitter: a gold-plated ranged ally that keeps pressure with amber venom and controlled distance. Only one Spider/Broodmother family companion from this mod can be active; another call replaces the current companion.",
        "Summons or replaces Aurex, a ranged Broodmother that keeps a clear lane for gilded venom spit.",
        "Replaces your current spider-family ally with Broodmother Aurex, favoring ranged spit pressure over close pursuit.",
        BroodmotherLocationTemplateName,
        BroodmotherLocationTemplateGuid,
        BroodmotherNpcTemplateGuid,
        BroodmotherResolverMethodName,
        "avalon-awakened/creatures/spider/broodmother/skin3-1/visual-v1",
        5.5f,
        BroodmotherCompanionAiV2Contract.GildedSpitterRole.Value,
        BroodmotherIconVariant.ReservedGold);

    internal static readonly SpiderFamilyCompanionDefinition SpiderSkin1 = new(
        "spider-skin1-1",
        SpiderFamilyCompanionTier.Spider,
        "skin1-1",
        "Rook - Crimson Harrier Spider",
        "b7d0d4e6c0de4bb0a000000000000004",
        "ItemTemplate_Magic_Tier1_AvalonSpiderCallSkin1",
        "Spider's Call - Rook, Crimson Harrier",
        "Calls Rook, the Crimson Harrier Spider: a fast red-black skirmisher that bites, repositions, and keeps the target turning. Only one Spider/Broodmother family companion from this mod can be active; another call replaces the current companion.",
        "Summons or replaces Rook, a quick Spider built for rapid bite cycles and sharp repositioning.",
        "Replaces your current spider-family ally with Rook, keeping enemies pinned by speed instead of bulk.",
        SpiderLocationTemplateName,
        SpiderLocationTemplateGuid,
        SpiderNpcTemplateGuid,
        SpiderResolverMethodName,
        "avalon-awakened/creatures/spider/common/skin1-1/visual-v1",
        5.5f,
        BroodmotherCompanionAiV2Contract.CrimsonHarrierRole.Value,
        BroodmotherIconVariant.SpiderSmallSkin1RedBlack);

    internal static readonly SpiderFamilyCompanionDefinition SpiderSkin2 = new(
        "spider-skin2-1",
        SpiderFamilyCompanionTier.Spider,
        "skin2-1",
        "Vesper - Pale Ambusher Spider",
        "b7d0d4e6c0de4bb0a000000000000005",
        "ItemTemplate_Magic_Tier1_AvalonSpiderCallSkin2",
        "Spider's Call - Vesper, Pale Ambusher",
        "Calls Vesper, the Pale Ambusher Spider: a wide-angle leaper that waits off the front line and punishes exposed sides. Only one Spider/Broodmother family companion from this mod can be active; another call replaces the current companion.",
        "Summons or replaces Vesper, a pale Spider that opens space and attacks from the flank.",
        "Replaces your current spider-family ally with Vesper, prioritizing wide flanks and leap pressure.",
        SpiderLocationTemplateName,
        SpiderLocationTemplateGuid,
        SpiderNpcTemplateGuid,
        SpiderResolverMethodName,
        "avalon-awakened/creatures/spider/common/skin2-1/visual-v1",
        5.5f,
        BroodmotherCompanionAiV2Contract.PaleAmbusherRole.Value,
        BroodmotherIconVariant.SpiderSmallSkin2Pale);

    internal static readonly SpiderFamilyCompanionDefinition SpiderSkin3 = new(
        "spider-skin3-1",
        SpiderFamilyCompanionTier.Spider,
        "skin3-1",
        "Nox - Gilded Finisher Spider",
        "b7d0d4e6c0de4bb0a000000000000006",
        "ItemTemplate_Magic_Tier1_AvalonSpiderCallSkin3",
        "Spider's Call - Nox, Gilded Finisher",
        "Calls Nox, the Gilded Finisher Spider: a gold-marked hunter that claims weakened targets and ends fights quickly. Only one Spider/Broodmother family companion from this mod can be active; another call replaces the current companion.",
        "Summons or replaces Nox, a finisher Spider that hunts wounded enemies first.",
        "Replaces your current spider-family ally with Nox, sharpening the pack around weak-target execution.",
        SpiderLocationTemplateName,
        SpiderLocationTemplateGuid,
        SpiderNpcTemplateGuid,
        SpiderResolverMethodName,
        "avalon-awakened/creatures/spider/common/skin3-1/visual-v1",
        5.5f,
        BroodmotherCompanionAiV2Contract.GildedFinisherRole.Value,
        BroodmotherIconVariant.SpiderSmallSkin3Gold);

    internal static IReadOnlyList<SpiderFamilyCompanionDefinition> All { get; } =
        new[]
        {
            BroodmotherSkin1,
            BroodmotherSkin2,
            BroodmotherSkin3,
            SpiderSkin1,
            SpiderSkin2,
            SpiderSkin3,
        };

    internal static bool TryGetBySpellTemplateGuid(
        string? templateGuid,
        out SpiderFamilyCompanionDefinition definition)
    {
        foreach (SpiderFamilyCompanionDefinition candidate in All)
        {
            if (string.Equals(candidate.SpellTemplateGuid, templateGuid, StringComparison.OrdinalIgnoreCase))
            {
                definition = candidate;
                return true;
            }
        }

        definition = BroodmotherSkin1;
        return false;
    }
}
