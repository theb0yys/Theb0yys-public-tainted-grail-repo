using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Items;
using Awaken.TG.Main.Localization;
using Awaken.TG.Main.Templates;
using Awaken.TG.MVC;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DragonKnight;

internal static class DragonKnightArmorItemFeature
{
    private static readonly bool RuntimeArmorItemRegistrationBlockedByResearch = true;
    internal const string CustomTemplateGuid = "d9a1f520b7ee4a5f8d9b0c4f4d4b0001";
    internal const string CustomTemplateName = "ItemTemplate_Mod_DragonKnight_IronNoCape_Cuirass";
    private const string SourceTemplateName = "ItemTemplate_Armor_Medium_T3_Body_WarScarredBrigandsArmor";
    private const string DisplayName = "Dragon Knight Armor";
    private const string DisplayDescription = "A Dragon Knight cuirass registered as a native armor item.";
    private const string DisplayFlavor = "Forged for the Dragon Knight and registered through the native item template map.";
    private const string ManagerActionId = Plugin.PluginGuid + ".grant-armor";
    private const string ManagerTypeName = "FoAModManager.FoAModManagerApi, FoAModManager";

    private static readonly FieldInfo? TemplatesProviderLoaderField = AccessTools.Field(typeof(TemplatesProvider), "_loader");
    private static readonly MethodInfo? TemplatesLoaderAddToMapMethod = AccessTools.Method(typeof(TemplatesLoader), "AddToMap");
    private static readonly FieldInfo? ItemTemplateDescriptionField = AccessTools.Field(typeof(ItemTemplate), "description");
    private static readonly FieldInfo? ItemTemplateFlavorField = AccessTools.Field(typeof(ItemTemplate), "flavor");

    private static ConfigEntry<bool> _enabled = null!;
    private static ConfigEntry<KeyCode> _grantHotkey = null!;
    private static ConfigEntry<bool> _registerManagerCommand = null!;
    private static bool _registrationFailureLogged;
    private static bool _managerActionRegistered;
    private static float _nextManagerRegistrationRetryAt;

    internal static bool Enabled => _enabled?.Value ?? false;

    internal static void Bind(ConfigFile config)
    {
        _enabled = config.Bind(
            "ArmorItem",
            "Enabled",
            false,
            "Blocked by current A2K armor research gates; native armor item registration is not authorized.");
        _grantHotkey = config.Bind(
            "ArmorItem",
            "GrantHotkey",
            KeyCode.F11,
            "Blocked by current A2K armor research gates; inventory grants are not authorized.");
        _registerManagerCommand = config.Bind(
            "ModManager",
            "RegisterArmorGrantCommand",
            false,
            "Blocked by current A2K armor research gates; armor grant actions are not authorized.");
    }

    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        if (RuntimeArmorItemRegistrationBlockedByResearch)
        {
            logger.LogWarning($"{Plugin.PluginName}: Dragon Knight armor item registration is blocked by current A2K research gates.");
            return;
        }

        MethodInfo? setter = AccessTools.PropertySetter(typeof(TemplatesLoader), nameof(TemplatesLoader.FinishedLoading));
        MethodInfo? postfix = AccessTools.Method(typeof(DragonKnightArmorItemFeature), nameof(RegisterAfterLoaderFinished));
        if (setter == null || postfix == null)
        {
            logger.LogWarning($"{Plugin.PluginName}: could not patch TemplatesLoader.FinishedLoading; armor template registration will retry from the update loop.");
            return;
        }

        harmony.Patch(setter, postfix: new HarmonyMethod(postfix));
        logger.LogInfo($"{Plugin.PluginName}: patched TemplatesLoader.FinishedLoading for Dragon Knight armor item registration.");
    }

    internal static void Tick(ManualLogSource logger)
    {
        if (RuntimeArmorItemRegistrationBlockedByResearch)
        {
            return;
        }

        if (!Enabled)
        {
            return;
        }

        EnsureRegistered(logger);
        TryRegisterManagerAction(logger);
        if (Input.GetKeyDown(_grantHotkey.Value))
        {
            bool granted = TryGrantToHero(logger, out string message);
            logger.LogWarning($"DRAGON_KNIGHT_ARMOR_GRANT result={(granted ? "ok" : "blocked")} message={message}");
        }
    }

    internal static void Dispose(ManualLogSource logger)
    {
        TryUnregisterManagerAction(logger);
    }

    internal static bool EnsureRegistered(ManualLogSource logger)
    {
        if (RuntimeArmorItemRegistrationBlockedByResearch)
        {
            LogRegistrationFailureOnce(logger, "blocked-by-current-a2k-research-gates");
            return false;
        }

        if (!Enabled)
        {
            return false;
        }

        if (TryResolveCustomTemplate(out ItemTemplate? existingTemplate, out _) && existingTemplate != null)
        {
            ApplyCustomFields(existingTemplate);
            return true;
        }

        if (!TryResolveSourceTemplate(out ItemTemplate? sourceTemplate, out string sourceReason) || sourceTemplate == null)
        {
            LogRegistrationFailureOnce(logger, $"source-{sourceReason}");
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

        string[] sourceComponentTypes = GetComponentTypeNames(sourceTemplate.gameObject);
        GameObject? cloneObject = null;
        try
        {
            cloneObject = UnityEngine.Object.Instantiate(sourceTemplate.gameObject);
            cloneObject.name = CustomTemplateName;
            UnityEngine.Object.DontDestroyOnLoad(cloneObject);

            ItemTemplate? customTemplate = cloneObject.GetComponent<ItemTemplate>();
            if (customTemplate == null)
            {
                UnityEngine.Object.Destroy(cloneObject);
                LogRegistrationFailureOnce(logger, "cloned-object-missing-item-template");
                return false;
            }

            customTemplate.templateType = sourceTemplate.templateType;
            ApplyCustomFields(customTemplate);

            string[] customComponentTypes = GetComponentTypeNames(customTemplate.gameObject);
            if (!sourceComponentTypes.SequenceEqual(customComponentTypes, StringComparer.Ordinal))
            {
                UnityEngine.Object.Destroy(cloneObject);
                LogRegistrationFailureOnce(logger, "native-logic-component-types-mismatch");
                return false;
            }

            TemplatesLoaderAddToMapMethod.Invoke(loader, new object[] { CustomTemplateGuid, customTemplate });
            _registrationFailureLogged = false;
            logger.LogInfo(
                $"{Plugin.PluginName}: Dragon Knight armor item registered; " +
                $"customGuid={CustomTemplateGuid}; templateName={CustomTemplateName}; displayName={DisplayName}; " +
                $"sourceGuid={sourceTemplate.GUID}; sourceName={sourceTemplate.name}; sourceTemplateMutated=false; " +
                "route=clone-native-cuirass-template-private-loader-map.");
            return true;
        }
        catch (Exception ex)
        {
            if (cloneObject != null)
            {
                UnityEngine.Object.Destroy(cloneObject);
            }

            LogRegistrationFailureOnce(logger, $"custom-armor-template-registration-failed {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    internal static bool TryGrantToHero(ManualLogSource logger, out string message)
    {
        message = string.Empty;
        if (RuntimeArmorItemRegistrationBlockedByResearch)
        {
            message = "blocked by current A2K armor research gates";
            return false;
        }

        if (!Enabled)
        {
            message = "armor item feature disabled";
            return false;
        }

        if (!EnsureRegistered(logger))
        {
            message = "custom armor template unavailable";
            return false;
        }

        Hero hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            message = "hero unavailable";
            return false;
        }

        try
        {
            TemplatesProvider? provider = World.Services?.TryGet<TemplatesProvider>();
            if (provider == null || !provider.AllLoaded)
            {
                message = "templates provider unavailable";
                return false;
            }

            ItemTemplate? template = provider.Get<ItemTemplate>(CustomTemplateGuid);
            if (!IsCustomTemplate(template))
            {
                message = "custom armor template not resolved";
                return false;
            }

            Item item = World.Add(new Item(template, 1));
            Item? added = hero.HeroItems.Add(item);
            if (added == null)
            {
                message = "HeroItems.Add returned null";
                return false;
            }

            message = "added Dragon Knight Armor to inventory";
            return true;
        }
        catch (Exception ex)
        {
            message = $"{ex.GetType().Name}: {ex.Message}";
            logger.LogWarning("Dragon Knight armor grant failed: " + ex);
            return false;
        }
    }

    private static void TryRegisterManagerAction(ManualLogSource logger)
    {
        if (!Enabled || !_registerManagerCommand.Value || _managerActionRegistered || Time.unscaledTime < _nextManagerRegistrationRetryAt)
        {
            return;
        }

        _nextManagerRegistrationRetryAt = Time.unscaledTime + 3f;
        Type? apiType = Type.GetType(ManagerTypeName, throwOnError: false);
        if (apiType == null)
        {
            return;
        }

        MethodInfo? register = apiType.GetMethod(
            "RegisterControllerAction",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(Action) },
            modifiers: null);
        if (register == null)
        {
            logger.LogWarning($"{Plugin.PluginName}: FoA Mod Manager armor grant registration failed; RegisterControllerAction overload missing.");
            return;
        }

        try
        {
            object? result = register.Invoke(null, new object[]
            {
                ManagerActionId,
                "Grant Dragon Knight Armor",
                DisplayName,
                "Add the Dragon Knight Armor item to the current hero inventory.",
                (Action)GrantFromManager
            });
            _managerActionRegistered = result is bool registered && registered;
            if (_managerActionRegistered)
            {
                logger.LogInfo($"{Plugin.PluginName}: FoA Mod Manager armor grant action registered: {ManagerActionId}.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning($"{Plugin.PluginName}: FoA Mod Manager armor grant registration failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void TryUnregisterManagerAction(ManualLogSource logger)
    {
        if (!_managerActionRegistered)
        {
            return;
        }

        Type? apiType = Type.GetType(ManagerTypeName, throwOnError: false);
        MethodInfo? unregister = apiType?.GetMethod(
            "UnregisterControllerAction",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            new[] { typeof(string) },
            modifiers: null);
        try
        {
            unregister?.Invoke(null, new object[] { ManagerActionId });
        }
        catch (Exception ex)
        {
            logger.LogWarning($"{Plugin.PluginName}: FoA Mod Manager armor grant unregister failed: {ex.GetType().Name}: {ex.Message}");
        }

        _managerActionRegistered = false;
    }

    private static void GrantFromManager()
    {
        ManualLogSource? logger = Plugin.Instance?.ModLogger;
        if (logger == null)
        {
            return;
        }

        bool granted = TryGrantToHero(logger, out string message);
        logger.LogWarning($"DRAGON_KNIGHT_ARMOR_GRANT source=foa-mod-manager result={(granted ? "ok" : "blocked")} message={message}");
    }

    private static void RegisterAfterLoaderFinished(bool value)
    {
        if (value && Plugin.Instance != null)
        {
            EnsureRegistered(Plugin.Instance.ModLogger);
        }
    }

    private static void ApplyCustomFields(ItemTemplate customTemplate)
    {
        customTemplate.name = CustomTemplateName;
        customTemplate.GUID = CustomTemplateGuid;
        customTemplate.hiddenOnUI = false;
        customTemplate.cannotBeDropped = false;
        customTemplate.canStack = false;
        customTemplate.itemName = (LocString)DisplayName;
        ItemTemplateDescriptionField?.SetValue(
            customTemplate,
            new OptionalLocString((LocString)DisplayDescription, true));
        ItemTemplateFlavorField?.SetValue(
            customTemplate,
            new OptionalLocString((LocString)DisplayFlavor, true));
    }

    private static bool TryResolveCustomTemplate(out ItemTemplate? template, out string reason)
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

            template = provider.Get<ItemTemplate>(CustomTemplateGuid);
            if (!IsCustomTemplate(template))
            {
                reason = "custom-template-guid-not-found";
                template = null;
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            reason = $"custom-template-resolution-failed {ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }

    private static bool TryResolveSourceTemplate(out ItemTemplate? template, out string reason)
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

            template = provider
                .GetAllOfType<ItemTemplate>(TemplateTypeFlag.All)
                .FirstOrDefault(candidate =>
                    candidate != null &&
                    string.Equals(candidate.name, SourceTemplateName, StringComparison.Ordinal));

            if (template == null)
            {
                reason = "source-template-name-not-found";
                return false;
            }

            if (template.IsAbstract || template.HiddenOnUI || template.CannotBeDropped)
            {
                reason = $"source-template-not-safe-visible-regular abstract={template.IsAbstract} hidden={template.HiddenOnUI} cannotDrop={template.CannotBeDropped}";
                template = null;
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            reason = $"source-template-resolution-failed {ex.GetType().Name}: {ex.Message}";
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
            reason = $"templates-loader-resolution-failed {ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }

    private static bool IsCustomTemplate(ItemTemplate? template)
    {
        return template != null &&
            string.Equals(template.GUID, CustomTemplateGuid, StringComparison.OrdinalIgnoreCase);
    }

    private static string[] GetComponentTypeNames(GameObject gameObject)
    {
        return gameObject
            .GetComponents<Component>()
            .Select(component => component == null ? "<null>" : component.GetType().FullName ?? component.GetType().Name)
            .ToArray();
    }

    private static void LogRegistrationFailureOnce(ManualLogSource logger, string reason)
    {
        if (_registrationFailureLogged)
        {
            return;
        }

        _registrationFailureLogged = true;
        logger.LogWarning(
            $"DRAGON_KNIGHT_ARMOR_ITEM_REGISTRATION_BLOCKED reason={reason}; " +
            $"customGuid={CustomTemplateGuid}; sourceTemplateName={SourceTemplateName}; route=clone-native-cuirass-template-private-loader-map.");
    }
}
