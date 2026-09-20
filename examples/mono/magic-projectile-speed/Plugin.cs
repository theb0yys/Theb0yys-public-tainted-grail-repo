using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Awaken.TG.Main.AI.Fights.Projectiles;
using Awaken.TG.Main.Heroes;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGExample.MagicProjectileSpeed;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.magic-projectile-speed";
    public const string PluginName = "TG Example - Magic Projectile Speed";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<float> _speedMultiplier = null!;
    private Harmony? _harmony;

    internal static float SpeedMultiplier => s_instance == null ? 1f : Mathf.Clamp(s_instance._speedMultiplier.Value, 0.25f, 5f);

    private void Awake()
    {
        s_instance = this;
        _speedMultiplier = Config.Bind(
            "Projectile",
            "SpeedMultiplier",
            1f,
            new ConfigDescription("Player magic projectile speed multiplier.",
                new AcceptableValueRange<float>(0.25f, 5f)));

        _harmony = new Harmony(PluginGuid);
        MagicProjectilePatch.Apply(_harmony, Logger);

        Logger.LogInfo($"{PluginName} loaded. SpeedMultiplier={SpeedMultiplier:0.##}");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        s_instance = null;
    }
}

internal static class MagicProjectilePatch
{
    private static readonly ConditionalWeakTable<DamageDealingProjectile, ScaledMarker> Scaled = new();
    private static readonly FieldInfo? RigidbodyField = AccessTools.Field(typeof(Projectile), "_rb");

    internal static void Apply(Harmony harmony, BepInEx.Logging.ManualLogSource logger)
    {
        MethodInfo? baseDamage = AccessTools.Method(typeof(DamageDealingProjectile), "SetBaseDamageParams");
        MethodInfo? basePostfix = AccessTools.Method(typeof(MagicProjectilePatch), nameof(BaseDamagePostfix));

        if (baseDamage != null && basePostfix != null)
        {
            harmony.Patch(baseDamage, postfix: new HarmonyMethod(basePostfix));
            logger.LogInfo("Patched DamageDealingProjectile.SetBaseDamageParams.");
        }

        Type? configureType = AccessTools.TypeByName("Awaken.TG.Main.AI.Fights.Projectiles.ConfigureShootProjectile");
        MethodInfo? configure = configureType == null ? null : AccessTools.Method(configureType, "ApplyToProjectile");
        MethodInfo? configurePostfix = AccessTools.Method(typeof(MagicProjectilePatch), nameof(ConfigurePostfix));

        if (configure != null && configurePostfix != null)
        {
            harmony.Patch(configure, postfix: new HarmonyMethod(configurePostfix));
            logger.LogInfo("Patched ConfigureShootProjectile.ApplyToProjectile.");
        }
    }

    private static void BaseDamagePostfix(DamageDealingProjectile __instance)
    {
        TryScale(__instance);
    }

    private static void ConfigurePostfix(Projectile projectile)
    {
        if (projectile is DamageDealingProjectile damageProjectile)
        {
            TryScale(damageProjectile);
        }
    }

    private static void TryScale(DamageDealingProjectile projectile)
    {
        float multiplier = Plugin.SpeedMultiplier;
        if (Mathf.Abs(multiplier - 1f) <= 0.0001f || Scaled.TryGetValue(projectile, out _))
        {
            return;
        }

        if (projectile.Owner is not Hero owner ||
            owner.HasBeenDiscarded ||
            !ReferenceEquals(owner, Hero.Current))
        {
            return;
        }

        bool magic = projectile is MagicProjectile
                     || projectile.SourceWeapon?.IsMagic == true
                     || projectile.SourceProjectile?.IsMagic == true;

        if (!magic)
        {
            return;
        }

        Vector3 current = projectile.Velocity;
        if (current.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Vector3 offset = projectile.PositionOffset.InitialVelocity;
        Vector3 baseVelocity = current - offset;
        if (baseVelocity.sqrMagnitude <= 0.0001f)
        {
            baseVelocity = current;
            offset = Vector3.zero;
        }

        Vector3 scaledVelocity = baseVelocity * multiplier + offset;

        if (RigidbodyField?.GetValue(projectile) is Rigidbody body && !body.isKinematic)
        {
            body.linearVelocity = scaledVelocity;
            if (scaledVelocity.sqrMagnitude > 0.0001f)
            {
                projectile.transform.rotation = Quaternion.LookRotation(scaledVelocity, projectile.transform.up);
            }
        }
        else
        {
            projectile.SetVelocityAndForward(scaledVelocity);
        }

        Scaled.Add(projectile, new ScaledMarker());
    }

    private sealed class ScaledMarker
    {
    }
}
