using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Items;
using Awaken.TG.Main.Templates;
using Awaken.TG.MVC;
using BepInEx;
using UnityEngine;

namespace TGCommunity.Example.RigidWeaponPresentation;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("kane.tgfoa.tainted-weapons", BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.rigid-weapon-presentation";
    public const string PluginName = "TG Example - Rigid Weapon Presentation";
    public const string PluginVersion = "0.1.0";

    private const string FrameworkApiTypeName = "TaintedWeapons.TaintedWeaponsApi, TaintedWeapons";

    private const string WeaponId = "community-rigid-blade";
    private const string CustomTemplateGuid = "c0ffee00000000000000000000000001";
    private const string CustomTemplateName = "ItemTemplate_Mod_CommunityRigidBlade";

    // Exact native source profile used by the maintained rigid-weapon path.
    private const string SourceTemplateGuid = "a04d79985ec011245a8383530fc72dd7";

    private const string BundleFileName = "community_rigid_blade.bundle";
    private const string EquippedPrefabAssetPath = "Assets/TGCommunity/RigidBlade.prefab";
    private const string NativeCloneProfileId = "weapon-item-template-clone/v1";

    private bool _frameworkAccepted;
    private float _nextRetryAt;

    private string PluginDirectory =>
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Paths.PluginPath;

    private void Awake()
    {
        Logger.LogInfo($"{PluginName} loaded. Tainted Weapons owns native registration/equipped Drake presentation.");

        string bundlePath = Path.Combine(PluginDirectory, BundleFileName);
        if (!File.Exists(bundlePath))
        {
            Logger.LogWarning($"Required example bundle is missing: {bundlePath}");
        }

        TryRegisterPackage();
    }

    private void Update()
    {
        if (!_frameworkAccepted && Time.unscaledTime >= _nextRetryAt)
        {
            _nextRetryAt = Time.unscaledTime + 3f;
            TryRegisterPackage();
        }

        if (Input.GetKeyDown(KeyCode.F8))
        {
            TryGrantRegisteredWeapon();
        }
    }

    private void TryRegisterPackage()
    {
        Type? apiType = Type.GetType(FrameworkApiTypeName, throwOnError: false);
        if (apiType == null)
        {
            Logger.LogWarning("TaintedWeaponsApi is not loaded.");
            return;
        }

        MethodInfo? register = apiType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(method =>
            {
                if (!string.Equals(method.Name, "RegisterWeaponPackage", StringComparison.Ordinal))
                {
                    return false;
                }

                ParameterInfo[] parameters = method.GetParameters();
                return parameters.Length == 15 &&
                       parameters[0].ParameterType == typeof(string) &&
                       parameters[13].ParameterType == typeof(bool) &&
                       parameters[14].ParameterType.IsByRef;
            });

        if (register == null)
        {
            Logger.LogWarning("Tainted Weapons RegisterWeaponPackage overload was not found.");
            return;
        }

        try
        {
            object?[] args =
            {
                PluginGuid,
                WeaponId,
                CustomTemplateGuid,
                SourceTemplateGuid,
                CustomTemplateName,
                "Community Rigid Blade",
                "Minimal custom rigid weapon registered through Tainted Weapons.",
                PluginDirectory,
                BundleFileName,
                EquippedPrefabAssetPath,
                NativeCloneProfileId,
                string.Empty,
                "Example framework-owned rigid weapon.",
                true,
                null
            };

            bool accepted = register.Invoke(null, args) is bool value && value;
            object? receipt = args[14];

            _frameworkAccepted = accepted;
            Logger.LogInfo(
                $"Tainted Weapons package registration accepted={accepted}; receipt={receipt?.ToString() ?? "<none>"}");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Tainted Weapons package registration failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void TryGrantRegisteredWeapon()
    {
        if (!_frameworkAccepted)
        {
            Logger.LogWarning("Grant blocked: Tainted Weapons has not accepted the package yet.");
            return;
        }

        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded)
        {
            Logger.LogWarning("Grant blocked: Hero.Current is unavailable.");
            return;
        }

        TemplatesProvider? provider = World.Services?.TryGet<TemplatesProvider>();
        if (provider == null || !provider.AllLoaded)
        {
            Logger.LogWarning("Grant blocked: TemplatesProvider is not ready.");
            return;
        }

        ItemTemplate? template;
        try
        {
            template = provider.Get<ItemTemplate>(CustomTemplateGuid);
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Grant blocked: registered template lookup failed ({ex.GetType().Name}).");
            return;
        }

        if (template == null ||
            !string.Equals(template.GUID, CustomTemplateGuid, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(template.name, CustomTemplateName, StringComparison.Ordinal))
        {
            Logger.LogWarning("Grant blocked: registered custom template is not available yet.");
            return;
        }

        try
        {
            Item item = World.Add(new Item(template, 1));
            Item? added = hero.HeroItems.Add(item);
            Logger.LogInfo(
                added == null
                    ? "Grant failed: HeroItems.Add returned null."
                    : $"Granted {template.name}. Equip it normally; Tainted Weapons owns the equipped Drake presentation.");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Grant failed: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
