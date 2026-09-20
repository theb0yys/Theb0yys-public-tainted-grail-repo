using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Awaken.TG.Main.AI.Fights.Projectiles;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGCommunity.Example.ProjectileSpeedAndAim;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.projectile-speed-and-aim";
    public const string PluginName = "TG Example - Projectile Speed and Aim";
    public const string PluginVersion = "0.1.0";

    private static readonly FieldInfo? RigidbodyField = AccessTools.Field(typeof(Projectile), "_rb");
    private static readonly FieldInfo? LifeTimeField = AccessTools.Field(typeof(DamageDealingProjectile), "<LifeTime>k__BackingField");
    private static readonly ConditionalWeakTable<DamageDealingProjectile, State> States = new();

    private static ConfigEntry<float>? _speed;
    private static ConfigEntry<float>? _range;
    private Harmony? _harmony;

    private void Awake()
    {
        _speed = Config.Bind("Projectile", "SpeedMultiplier", 1.25f, "Scale the aim component of projectile velocity.");
        _range = Config.Bind("Projectile", "LifetimeMultiplier", 1.0f, "Scale DamageDealingProjectile lifetime.");

        _harmony = new Harmony(PluginGuid);

        MethodInfo? baseDamage = AccessTools.Method(typeof(DamageDealingProjectile), "SetBaseDamageParams");
        if (baseDamage != null)
            _harmony.Patch(baseDamage, postfix: new HarmonyMethod(typeof(Plugin), nameof(BaseDamagePostfix)));

        Type? configureType = AccessTools.TypeByName("Awaken.TG.Main.AI.Fights.Projectiles.ConfigureShootProjectile");
        MethodInfo? fallback = configureType == null ? null : AccessTools.Method(configureType, "ApplyToProjectile");
        if (fallback != null)
            _harmony.Patch(fallback, postfix: new HarmonyMethod(typeof(Plugin), nameof(ConfigurePostfix)));

        Logger.LogInfo($"{PluginName} loaded. MainHook={baseDamage != null}; FallbackHook={fallback != null}.");
    }

    private void OnDestroy() => _harmony?.UnpatchSelf();

    private static void BaseDamagePostfix(DamageDealingProjectile __instance) => Apply(__instance);
    private static void ConfigurePostfix(Projectile projectile) => Apply(projectile);

    private static void Apply(Projectile projectile)
    {
        if (projectile is not DamageDealingProjectile damageProjectile)
            return;

        State state = States.GetOrCreateValue(damageProjectile);

        if (!state.VelocityScaled)
        {
            float multiplier = Mathf.Clamp(_speed?.Value ?? 1f, 0.1f, 5f);
            if (Mathf.Abs(multiplier - 1f) > 0.0001f)
            {
                Vector3 finalVelocity = projectile.Velocity;
                if (finalVelocity.sqrMagnitude > 0.0001f)
                {
                    Vector3 offsetVelocity = damageProjectile.PositionOffset.InitialVelocity;
                    Vector3 aimVelocity = finalVelocity - offsetVelocity;
                    if (aimVelocity.sqrMagnitude <= 0.0001f)
                    {
                        aimVelocity = finalVelocity;
                        offsetVelocity = Vector3.zero;
                    }

                    SetVelocity(projectile, aimVelocity * multiplier + offsetVelocity);
                    state.VelocityScaled = true;
                }
            }
        }

        if (!state.LifetimeScaled && LifeTimeField?.GetValue(damageProjectile) is float life)
        {
            float multiplier = Mathf.Clamp(_range?.Value ?? 1f, 0.1f, 5f);
            if (Mathf.Abs(multiplier - 1f) > 0.0001f)
            {
                LifeTimeField.SetValue(damageProjectile, Mathf.Clamp(life * multiplier, 0.05f, 300f));
                state.LifetimeScaled = true;
            }
        }
    }

    private static void SetVelocity(Projectile projectile, Vector3 velocity)
    {
        if (RigidbodyField?.GetValue(projectile) is Rigidbody rb && !rb.isKinematic)
        {
            rb.linearVelocity = velocity;
            if (velocity.sqrMagnitude > 0.0001f)
                projectile.transform.rotation = Quaternion.LookRotation(velocity, projectile.transform.up);
            return;
        }

        projectile.SetVelocityAndForward(velocity);
    }

    private sealed class State
    {
        internal bool VelocityScaled;
        internal bool LifetimeScaled;
    }
}
