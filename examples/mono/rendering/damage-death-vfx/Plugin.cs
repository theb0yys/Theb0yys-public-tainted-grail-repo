using System;
using System.Reflection;
using Awaken.TG.Main.Character;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace TGCommunity.Example.DamageDeathVfx;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.damage-death-vfx";
    public const string PluginName = "TG Example - Damage/Death VFX";
    public const string PluginVersion = "0.1.0";

    private static Plugin? _instance;
    private Harmony? _harmony;

    private void Awake()
    {
        _instance = this;

        Type? healthType = AccessTools.TypeByName("Awaken.TG.Main.Character.HealthElement");
        MethodInfo? damageTarget = healthType == null ? null : AccessTools.Method(healthType, "OnDamage");
        MethodInfo? deathTarget = healthType == null ? null : AccessTools.Method(healthType, "OnDeathEvents");
        MethodInfo? damagePostfix = AccessTools.Method(typeof(Plugin), nameof(DamagePostfix));
        MethodInfo? deathPostfix = AccessTools.Method(typeof(Plugin), nameof(DeathPostfix));

        if (damageTarget == null || deathTarget == null || damagePostfix == null || deathPostfix == null)
        {
            Logger.LogError("HealthElement damage/death lifecycle methods were not found. No VFX patches were installed.");
            return;
        }

        _harmony = new Harmony(PluginGuid);
        _harmony.Patch(damageTarget, postfix: new HarmonyMethod(damagePostfix));
        _harmony.Patch(deathTarget, postfix: new HarmonyMethod(deathPostfix));

        Logger.LogInfo($"{PluginName} loaded. Target-resolution methods are intentionally untouched.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        _instance = null;
    }

    private static void DamagePostfix(object __instance, object damage)
    {
        _instance?.HandlePresentation(__instance, damage, death: false);
    }

    private static void DeathPostfix(object __instance, object outcome)
    {
        _instance?.HandlePresentation(__instance, outcome, death: true);
    }

    private void HandlePresentation(object healthElement, object context, bool death)
    {
        object? target = ReadMember(context, "TargetPure") ?? ReadMember(healthElement, "ParentModel");
        if (target is not ICharacter character)
        {
            return;
        }

        Vector3 position = character.Coords + Vector3.up * (death ? 0.15f : 0.8f);
        SpawnBurst(position, death ? 28 : 10, death ? 0.16f : 0.08f, death ? 2.2f : 1.3f);

        Logger.LogDebug(death
            ? "Spawned mod-owned death presentation sidecar."
            : "Spawned mod-owned damage presentation sidecar.");
    }

    private static void SpawnBurst(Vector3 position, int count, float size, float speed)
    {
        var effect = new GameObject("TGCommunity.DamageDeathVfx");
        effect.transform.position = position;

        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = 0.35f;
        main.startSpeed = speed;
        main.startSize = size;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.55f, 0.02f, 0.02f, 1f));

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = false;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.15f;

        particles.Emit(count);
        UnityEngine.Object.Destroy(effect, 1.5f);
    }

    private static object? ReadMember(object target, string name)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        Type type = target.GetType();

        PropertyInfo? property = type.GetProperty(name, flags);
        if (property != null && property.GetIndexParameters().Length == 0)
        {
            try { return property.GetValue(target); } catch { }
        }

        FieldInfo? field = type.GetField(name, flags);
        if (field != null)
        {
            try { return field.GetValue(target); } catch { }
        }

        return null;
    }
}
