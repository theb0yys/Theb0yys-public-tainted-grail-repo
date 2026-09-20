using System.Reflection;
using System.Runtime.CompilerServices;
using Awaken.TG.Main.AI.Fights.Projectiles;
using Awaken.TG.Main.Heroes;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace TGTemplate.MagicProjectileSpeed;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.template.magic-projectile-speed";
    public const string PluginName = "TG Magic Projectile Speed Template";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<float> _speedMultiplier = null!;
    private Harmony? _harmony;

    internal static float SpeedMultiplier =>
        s_instance == null ? 1f : Mathf.Clamp(s_instance._speedMultiplier.Value, 0.25f, 5f);

    private void Awake()
    {
        s_instance = this;
        _speedMultiplier = Config.Bind("Projectile", "SpeedMultiplier", 1f,
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
    private static readonly ConditionalWeakTable<DamageDealingProjectile, Marker> Scaled = new();
    private static readonly FieldInfo? RigidbodyField = AccessTools.Field(typeof(Projectile), "_rb");

    internal static void Apply(Harmony harmony, BepInEx.Logging.ManualLogSource logger)
    {
        MethodInfo? target = AccessTools.Method(typeof(DamageDealingProjectile), "SetBaseDamageParams");
        MethodInfo? postfix = AccessTools.Method(typeof(MagicProjectilePatch), nameof(Postfix));
        if (target == null || postfix == null)
        {
            logger.LogWarning("DamageDealingProjectile.SetBaseDamageParams target was not found.");
            return;
        }

        harmony.Patch(target, postfix: new HarmonyMethod(postfix));
    }

    private static void Postfix(DamageDealingProjectile __instance)
    {
        float multiplier = Plugin.SpeedMultiplier;
        if (Mathf.Abs(multiplier - 1f) <= 0.0001f || Scaled.TryGetValue(__instance, out _))
            return;

        if (__instance.Owner is not Hero owner || owner.HasBeenDiscarded || !ReferenceEquals(owner, Hero.Current))
            return;

        bool magic = __instance is MagicProjectile
                     || __instance.SourceWeapon?.IsMagic == true
                     || __instance.SourceProjectile?.IsMagic == true;
        if (!magic)
            return;

        Vector3 current = __instance.Velocity;
        if (current.sqrMagnitude <= 0.0001f)
            return;

        Vector3 scaled = current * multiplier;
        if (RigidbodyField?.GetValue(__instance) is Rigidbody body && !body.isKinematic)
            body.linearVelocity = scaled;
        else
            __instance.SetVelocityAndForward(scaled);

        Scaled.Add(__instance, new Marker());
    }

    private sealed class Marker { }
}
