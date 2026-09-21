using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Awaken.TG.MVC;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Items;
using Awaken.TG.Main.Heroes.Items.Attachments;
using Awaken.TG.Main.Heroes.Items.Tooltips;
using Awaken.TG.Main.Heroes.Skills;
using Awaken.TG.Main.Localization;
using Awaken.TG.Main.Skills;
using Awaken.TG.Main.Templates;
using Awaken.TG.Main.Utility.RichEnums;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace AvalonBroodmotherCompanion;

internal static class CustomBroodmotherSpellFeature
{
    internal const string SourceSpellTemplateGuid = "3bd577472a0191c44bf298a82553cf3b";
    internal const string CustomSpellTemplateGuid = "b7d0d4e6c0de4bb0a000000000000001";
    internal const string CustomSpellTemplateName = "ItemTemplate_Magic_Tier1_AvalonBroodmotherCall";
    internal const string CustomSpellDisplayName = "Broodmother's Call";

    private const string MagicSummonAllySkillGraphGuid = "1ed1718ce1b64c747a329eb4269f529b";
    private const string SummonPrefabTemplateName = "SummonPrefab";
    private const float GrantCheckIntervalSeconds = 2f;

    private static readonly FieldInfo? TemplatesProviderLoaderField = AccessTools.Field(typeof(TemplatesProvider), "_loader");
    private static readonly MethodInfo? TemplatesLoaderAddToMapMethod = AccessTools.Method(typeof(TemplatesLoader), "AddToMap");
    private static readonly FieldInfo? ItemTemplateDescriptionField = AccessTools.Field(typeof(ItemTemplate), "description");
    private static readonly FieldInfo? ItemTemplateFlavorField = AccessTools.Field(typeof(ItemTemplate), "flavor");
    private static readonly FieldInfo? ItemTemplateLightCastInfoField = AccessTools.Field(typeof(ItemTemplate), "lightCastInfo");
    private static readonly FieldInfo? ItemTemplateHeavyCastInfoField = AccessTools.Field(typeof(ItemTemplate), "heavyCastInfo");
    private static readonly FieldInfo? MagicInfoMagicTypeField = AccessTools.Field(typeof(MagicItemTemplateInfo), "magicType");
    private static readonly FieldInfo? MagicInfoEffectTypeField = AccessTools.Field(typeof(MagicItemTemplateInfo), "effectType");
    private static readonly FieldInfo? MagicInfoMagicDescriptionField = AccessTools.Field(typeof(MagicItemTemplateInfo), "magicDescription");
    private static readonly HashSet<string> LoggedSkillRewriteKeys = new(StringComparer.Ordinal);
    private static readonly HashSet<string> LoggedSkillRewriteFailures = new(StringComparer.Ordinal);

    private static bool _registrationFailureLogged;
    private static bool _grantFailureLogged;
    private static bool _grantSatisfiedForHero;
    private static Hero? _lastGrantHero;
    private static float _nextGrantCheckTime;

    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        PatchTemplateLoader(harmony, logger);
        PatchSkillInitialization(harmony, logger);
        PatchSkillPerform(harmony, logger);
    }

    internal static void Tick(ManualLogSource logger)
    {
        Plugin? plugin = Plugin.Instance;
        if (plugin == null || !plugin.RegisterBroodmotherCallSpell)
        {
            return;
        }

        EnsureRegistered(logger);
        if (!plugin.GrantBroodmotherCallOnLoad)
        {
            _lastGrantHero = null;
            _grantSatisfiedForHero = false;
            return;
        }

        if (Time.unscaledTime < _nextGrantCheckTime)
        {
            return;
        }

        _nextGrantCheckTime = Time.unscaledTime + GrantCheckIntervalSeconds;
        TryGrantWhenReady(logger);
    }

    internal static bool EnsureRegistered(ManualLogSource logger)
    {
        Plugin? plugin = Plugin.Instance;
        if (plugin == null || !plugin.RegisterBroodmotherCallSpell)
        {
            return false;
        }

        if (AllDefinitionsRegistered())
        {
            foreach (SpiderFamilyCompanionDefinition definition in SpiderFamilyCompanionDefinitions.All)
            {
                if (TryResolveTemplate(definition.SpellTemplateGuid, out ItemTemplate? existingTemplate, out _) && existingTemplate != null)
                {
                    ApplyCustomFields(existingTemplate, existingTemplate.templateType, definition);
                }
            }

            return true;
        }

        if (!TryResolveTemplate(SourceSpellTemplateGuid, out ItemTemplate? sourceTemplate, out string sourceReason) || sourceTemplate == null)
        {
            LogRegistrationFailureOnce(logger, "source-" + sourceReason);
            return false;
        }

        if (!TryGetTemplateLoader(out object? loader, out string loaderReason) || loader == null)
        {
            LogRegistrationFailureOnce(logger, loaderReason);
            return false;
        }

        if (TemplatesLoaderAddToMapMethod == null)
        {
            LogRegistrationFailureOnce(logger, "templates-loader-add-map-method-missing");
            return false;
        }

        if (ItemTemplateDescriptionField == null)
        {
            LogRegistrationFailureOnce(logger, "item-template-description-field-missing");
            return false;
        }

        string[] sourceComponentTypes = GetComponentTypeNames(sourceTemplate.gameObject);
        bool allRegistered = true;
        foreach (SpiderFamilyCompanionDefinition definition in SpiderFamilyCompanionDefinitions.All)
        {
            if (TryResolveTemplate(definition.SpellTemplateGuid, out ItemTemplate? existingTemplate, out _) && existingTemplate != null)
            {
                ApplyCustomFields(existingTemplate, existingTemplate.templateType, definition);
                continue;
            }

            allRegistered &= TryRegisterDefinition(
                definition,
                sourceTemplate,
                sourceComponentTypes,
                loader,
                logger);
        }

        return allRegistered;
    }

    private static bool AllDefinitionsRegistered()
    {
        foreach (SpiderFamilyCompanionDefinition definition in SpiderFamilyCompanionDefinitions.All)
        {
            if (!TryResolveTemplate(definition.SpellTemplateGuid, out ItemTemplate? template, out _) || template == null)
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryRegisterDefinition(
        SpiderFamilyCompanionDefinition definition,
        ItemTemplate sourceTemplate,
        string[] sourceComponentTypes,
        object loader,
        ManualLogSource logger)
    {
        GameObject? cloneObject = null;
        try
        {
            cloneObject = UnityEngine.Object.Instantiate(sourceTemplate.gameObject);
            cloneObject.name = definition.SpellTemplateName;
            UnityEngine.Object.DontDestroyOnLoad(cloneObject);

            ItemTemplate? customTemplate = cloneObject.GetComponent<ItemTemplate>();
            if (customTemplate == null)
            {
                UnityEngine.Object.Destroy(cloneObject);
                LogRegistrationFailureOnce(logger, "cloned-object-missing-item-template-" + definition.Id);
                return false;
            }

            ApplyCustomFields(customTemplate, sourceTemplate.templateType, definition);

            string[] customComponentTypes = GetComponentTypeNames(customTemplate.gameObject);
            if (!sourceComponentTypes.SequenceEqual(customComponentTypes, StringComparer.Ordinal))
            {
                UnityEngine.Object.Destroy(cloneObject);
                LogRegistrationFailureOnce(logger, "native-logic-component-types-mismatch-" + definition.Id);
                return false;
            }

            TemplatesLoaderAddToMapMethod!.Invoke(loader, new object[] { definition.SpellTemplateGuid, customTemplate });
            _registrationFailureLogged = false;
            logger.LogInfo($"{Plugin.PluginName}: custom spell item registered; customGuid={definition.SpellTemplateGuid}; templateName={definition.SpellTemplateName}; displayName={definition.SpellDisplayName}; target={definition.DisplayName}; variant={definition.Id}; sourceGuid={SourceSpellTemplateGuid}; sourceName={sourceTemplate.name}; sourceTemplateMutated=false; route=clone-native-wolfs-call-template-private-loader-map.");
            return true;
        }
        catch (Exception ex)
        {
            if (cloneObject != null)
            {
                UnityEngine.Object.Destroy(cloneObject);
            }

            LogRegistrationFailureOnce(logger, "custom-spell-registration-failed-" + definition.Id + " " + ex.GetType().Name + ": " + ex.Message);
            return false;
        }
    }

    private static void PatchTemplateLoader(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? setter = AccessTools.PropertySetter(typeof(TemplatesLoader), nameof(TemplatesLoader.FinishedLoading));
        MethodInfo? postfix = AccessTools.Method(typeof(CustomBroodmotherSpellFeature), nameof(RegisterAfterLoaderFinished));
        if (setter == null || postfix == null)
        {
            logger.LogWarning($"{Plugin.PluginName}: could not patch TemplatesLoader.FinishedLoading; Broodmother's Call registration will wait for the update loop.");
            return;
        }

        harmony.Patch(setter, postfix: new HarmonyMethod(postfix));
        logger.LogInfo($"{Plugin.PluginName}: patched TemplatesLoader.FinishedLoading for Broodmother's Call registration.");
    }

    private static void PatchSkillInitialization(Harmony harmony, ManualLogSource logger)
    {
        Type[] signature = { typeof(ISkillOwner), typeof(IEnumerable<SkillReference>), typeof(SkillState) };
        MethodInfo? initialize = AccessTools.Method(typeof(SkillInitialization), nameof(SkillInitialization.Initialize), signature);
        MethodInfo? restore = AccessTools.Method(typeof(SkillInitialization), nameof(SkillInitialization.CustomRestore), signature);
        MethodInfo? prefix = AccessTools.Method(typeof(CustomBroodmotherSpellFeature), nameof(RewriteSkillReferencesPrefix));

        if (prefix == null)
        {
            logger.LogWarning($"{Plugin.PluginName}: Broodmother's Call skill-reference prefix missing; spell summon target rewrite disabled.");
            return;
        }

        if (initialize != null)
        {
            harmony.Patch(initialize, prefix: new HarmonyMethod(prefix));
        }
        else
        {
            logger.LogWarning($"{Plugin.PluginName}: SkillInitialization.Initialize target missing; fresh Broodmother's Call skill rewrite disabled.");
        }

        if (restore != null)
        {
            harmony.Patch(restore, prefix: new HarmonyMethod(prefix));
        }
        else
        {
            logger.LogWarning($"{Plugin.PluginName}: SkillInitialization.CustomRestore target missing; restored Broodmother's Call skill rewrite disabled.");
        }
    }

    private static void PatchSkillPerform(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? perform = AccessTools.Method(typeof(Skill), nameof(Skill.Perform), Type.EmptyTypes);
        MethodInfo? prefix = AccessTools.Method(typeof(CustomBroodmotherSpellFeature), nameof(PerformCustomBroodmotherCallPrefix));

        if (perform == null || prefix == null)
        {
            logger.LogWarning($"{Plugin.PluginName}: Skill.Perform target missing; Broodmother's Call companion cast route disabled.");
            return;
        }

        harmony.Patch(perform, prefix: new HarmonyMethod(prefix));
        logger.LogInfo($"{Plugin.PluginName}: patched Skill.Perform for Broodmother's Call companion cast route.");
    }

    private static void RegisterAfterLoaderFinished(bool value)
    {
        if (value && Plugin.Instance != null)
        {
            EnsureRegistered(Plugin.Instance.ModLogger);
        }
    }

    private static void RewriteSkillReferencesPrefix(ISkillOwner owner, ref IEnumerable<SkillReference> data)
    {
        Plugin? plugin = Plugin.Instance;
        if (plugin == null ||
            !plugin.RegisterBroodmotherCallSpell ||
            plugin.EnableBroodmotherCallCompanionRoute ||
            !plugin.EnableBroodmotherCallSummonTarget)
        {
            return;
        }

        if (owner is not ItemEffects itemEffects)
        {
            return;
        }

        string templateGuid = itemEffects.Item?.Template?.GUID ?? string.Empty;
        if (!SpiderFamilyCompanionDefinitions.TryGetBySpellTemplateGuid(
                templateGuid,
                out SpiderFamilyCompanionDefinition definition))
        {
            return;
        }

        List<SkillReference> nativeReferences = data?.ToList() ?? new List<SkillReference>();
        if (nativeReferences.Count == 0 || nativeReferences.Any(reference => reference == null || !reference.IsSet))
        {
            LogSkillRewriteFailureOnce(templateGuid, "native-skill-reference-missing-or-unset");
            return;
        }

        bool rewritten = false;
        List<SkillReference> rewrittenReferences = new(nativeReferences.Count);
        foreach (SkillReference nativeReference in nativeReferences)
        {
            SkillReference copy = nativeReference.Copy();
            if (string.Equals(copy.skillGraphRef.GUID, MagicSummonAllySkillGraphGuid, StringComparison.OrdinalIgnoreCase))
            {
                RewriteSummonTemplate(copy, definition);
                rewritten = true;
            }

            rewrittenReferences.Add(copy);
        }

        if (!rewritten)
        {
            LogSkillRewriteFailureOnce(templateGuid, "magic-summon-ally-graph-not-found");
            return;
        }

        data = rewrittenReferences;
        if (LoggedSkillRewriteKeys.Add(templateGuid))
        {
            plugin.ModLogger.LogInfo($"{Plugin.PluginName}: {definition.SpellDisplayName} skill references rewritten; customGuid={definition.SpellTemplateGuid}; graph={MagicSummonAllySkillGraphGuid}; summonTemplate={definition.LocationTemplateGuid}; variant={definition.Id}; vanillaTemplateMutated=false.");
        }
    }

    private static bool PerformCustomBroodmotherCallPrefix(Skill __instance)
    {
        Plugin? plugin = Plugin.Instance;
        if (plugin == null || !plugin.RegisterBroodmotherCallSpell || !plugin.EnableBroodmotherCallCompanionRoute)
        {
            return true;
        }

        if (!TryGetCustomBroodmotherCallSkillDefinition(__instance, out SpiderFamilyCompanionDefinition definition))
        {
            return true;
        }

        if (!__instance.IsSubmitted)
        {
            return true;
        }

        try
        {
            plugin.ModLogger.LogInfo($"{Plugin.PluginName}: {definition.SpellDisplayName} cast intercepted; customGuid={definition.SpellTemplateGuid}; graph={MagicSummonAllySkillGraphGuid}; target={definition.DisplayName}; variant={definition.Id}; route=one-session-companion; nativeSkillSpawnLocationSkipped=true.");
            plugin.TrySummonOrSwapFromSpell(definition);
        }
        catch (Exception ex)
        {
            plugin.ModLogger.LogWarning($"{Plugin.PluginName}: {definition.SpellDisplayName} companion route failed before native graph skip completed: {ex.GetType().Name}: {ex.Message}");
        }

        return false;
    }

    private static bool TryGetCustomBroodmotherCallSkillDefinition(
        Skill skill,
        out SpiderFamilyCompanionDefinition definition)
    {
        definition = SpiderFamilyCompanionDefinitions.BroodmotherSkin1;
        Item? sourceItem = skill.SourceItem;
        if (sourceItem == null || sourceItem.Template == null)
        {
            return false;
        }

        if (!SpiderFamilyCompanionDefinitions.TryGetBySpellTemplateGuid(sourceItem.Template.GUID, out definition))
        {
            return false;
        }

        string graphGuid = skill.Graph?.GUID ?? string.Empty;
        return string.Equals(graphGuid, MagicSummonAllySkillGraphGuid, StringComparison.OrdinalIgnoreCase);
    }

    private static void RewriteSummonTemplate(
        SkillReference reference,
        SpiderFamilyCompanionDefinition definition)
    {
        SkillTemplate replacement = new(SummonPrefabTemplateName, new TemplateReference(definition.LocationTemplateGuid));
        for (int i = 0; i < reference.templates.Count; i++)
        {
            SkillTemplate template = reference.templates[i];
            if (string.Equals(template.name, SummonPrefabTemplateName, StringComparison.Ordinal))
            {
                reference.templates[i] = replacement;
                return;
            }
        }

        reference.templates.Add(replacement);
    }

    private static void TryGrantWhenReady(ManualLogSource logger)
    {
        Plugin? plugin = Plugin.Instance;
        if (plugin == null || !plugin.EnableBroodmotherCallCompanionRoute)
        {
            LogGrantFailureOnce(logger, "broodmother-call-companion-route-disabled");
            return;
        }

        if (!TryGetCurrentHero(out Hero hero))
        {
            _lastGrantHero = null;
            _grantSatisfiedForHero = false;
            return;
        }

        if (!ReferenceEquals(_lastGrantHero, hero))
        {
            _lastGrantHero = hero;
            _grantSatisfiedForHero = false;
            _grantFailureLogged = false;
        }

        if (_grantSatisfiedForHero)
        {
            return;
        }

        int existingCount = CountCustomSpellItems(hero);
        if (existingCount > 0)
        {
            _grantSatisfiedForHero = true;
            logger.LogInfo($"{Plugin.PluginName}: current hero already has Broodmother's Call item(s); count={existingCount}.");
            return;
        }

        if (!EnsureRegistered(logger))
        {
            return;
        }

        SpiderFamilyCompanionDefinition definition = SpiderFamilyCompanionDefinitions.BroodmotherSkin1;
        if (!TryResolveTemplate(definition.SpellTemplateGuid, out ItemTemplate? template, out string reason) || template == null)
        {
            LogGrantFailureOnce(logger, "template-" + reason);
            return;
        }

        try
        {
            Item item = World.Add(new Item(template, 1));
            Item? added = hero.HeroItems.Add(item);
            if (added == null)
            {
                LogGrantFailureOnce(logger, "HeroItems.Add returned null.");
                return;
            }

            _grantSatisfiedForHero = true;
            logger.LogInfo($"{Plugin.PluginName}: granted {definition.SpellDisplayName} spell item {definition.SpellTemplateName} [{definition.SpellTemplateGuid}] to current hero.");
        }
        catch (Exception ex)
        {
            LogGrantFailureOnce(logger, "grant failed: " + ex.GetType().Name + ": " + ex.Message);
        }
    }

    private static void ApplyCustomFields(
        ItemTemplate customTemplate,
        TemplateType templateType,
        SpiderFamilyCompanionDefinition definition)
    {
        customTemplate.name = definition.SpellTemplateName;
        customTemplate.GUID = definition.SpellTemplateGuid;
        customTemplate.templateType = templateType;
        customTemplate.hiddenOnUI = false;
        customTemplate.cannotBeDropped = false;
        customTemplate.canStack = false;
        customTemplate.itemName = (LocString)definition.SpellDisplayName;
        ItemTemplateDescriptionField?.SetValue(
            customTemplate,
            new OptionalLocString((LocString)definition.SpellDisplayDescription, true));
        ItemTemplateFlavorField?.SetValue(
            customTemplate,
            new OptionalLocString((LocString)"The call scrapes through the Wyrdness and answers with chitin.", true));
        ApplyCustomMagicCastInfo(customTemplate, definition);
    }

    private static void ApplyCustomMagicCastInfo(
        ItemTemplate customTemplate,
        SpiderFamilyCompanionDefinition definition)
    {
        if (ItemTemplateLightCastInfoField == null ||
            ItemTemplateHeavyCastInfoField == null ||
            MagicInfoMagicTypeField == null ||
            MagicInfoEffectTypeField == null ||
            MagicInfoMagicDescriptionField == null)
        {
            return;
        }

        ItemTemplateLightCastInfoField.SetValue(customTemplate, CreateCustomMagicInfo(definition.SpellLightCastDescription));
        ItemTemplateHeavyCastInfoField.SetValue(customTemplate, CreateCustomMagicInfo(definition.SpellHeavyCastDescription));
    }

    private static MagicItemTemplateInfo CreateCustomMagicInfo(string description)
    {
        MagicItemTemplateInfo info = new();
        MagicInfoMagicTypeField?.SetValue(info, new RichEnumReference(MagicType.Summon));
        MagicInfoEffectTypeField?.SetValue(info, new RichEnumReference(MagicEffectType.None));
        MagicInfoMagicDescriptionField?.SetValue(info, new OptionalLocString((LocString)description, true));
        return info;
    }

    private static bool TryResolveTemplate(string templateGuid, out ItemTemplate? template, out string reason)
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
            reason = "template-resolution-failed " + ex.GetType().Name + ": " + ex.Message;
            return false;
        }
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
            reason = "templates-loader-resolution-failed " + ex.GetType().Name + ": " + ex.Message;
            return false;
        }
    }

    private static bool TryGetCurrentHero(out Hero hero)
    {
        hero = Hero.Current;
        return hero != null &&
               !hero.HasBeenDiscarded &&
               hero.HeroItems != null &&
               !hero.HeroItems.HasBeenDiscarded;
    }

    private static int CountCustomSpellItems(Hero hero)
    {
        int total = 0;
        foreach (Item item in hero.HeroItems.Items)
        {
            if (item == null || item.HasBeenDiscarded)
            {
                continue;
            }

            try
            {
                if (SpiderFamilyCompanionDefinitions.TryGetBySpellTemplateGuid(item.Template?.GUID, out _))
                {
                    total += Math.Max(0, item.Quantity);
                }
            }
            catch
            {
                // Ignore malformed inventory rows; the grant path below remains exact-template only.
            }
        }

        return total;
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

    private static void LogRegistrationFailureOnce(ManualLogSource logger, string reason)
    {
        if (_registrationFailureLogged)
        {
            return;
        }

        _registrationFailureLogged = true;
        logger.LogWarning($"{Plugin.PluginName}: Broodmother's Call spell unavailable; reason={reason}.");
    }

    private static void LogGrantFailureOnce(ManualLogSource logger, string reason)
    {
        if (_grantFailureLogged)
        {
            return;
        }

        _grantFailureLogged = true;
        logger.LogWarning($"{Plugin.PluginName}: Broodmother's Call grant unavailable; reason={reason}");
    }

    private static void LogSkillRewriteFailureOnce(string templateGuid, string reason)
    {
        string key = templateGuid + "|" + reason;
        if (LoggedSkillRewriteFailures.Add(key))
        {
            Plugin.Instance?.ModLogger.LogWarning($"{Plugin.PluginName}: Broodmother's Call skill rewrite skipped; customGuid={templateGuid}; reason={reason}.");
        }
    }
}
