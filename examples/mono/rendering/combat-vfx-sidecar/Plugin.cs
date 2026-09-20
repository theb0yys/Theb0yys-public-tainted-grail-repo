using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Awaken.TG.Main.Character;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace TGCommunity.Example.CombatVfxSidecar;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.combat-vfx-sidecar";
    public const string PluginName = "TG Example - Combat VFX Sidecar";
    public const string PluginVersion = "0.1.0";

    private static Plugin? Instance;
    private readonly Dictionary<int, float> _lastImpactByTarget = new();
    private readonly HashSet<int> _deathSeen = new();
    private readonly List<GameObject> _ownedEffects = new();
    private Harmony? _harmony;

    private void Awake()
    {
        Instance = this;
        Type? healthType = AccessTools.TypeByName("Awaken.TG.Main.Character.HealthElement");
        MethodInfo? damage = healthType == null ? null : AccessTools.Method(healthType, "OnDamage");
        MethodInfo? death = healthType == null ? null : AccessTools.Method(healthType, "OnDeathEvents");

        _harmony = new Harmony(PluginGuid);

        if (damage != null)
            _harmony.Patch(damage, postfix: new HarmonyMethod(typeof(Plugin), nameof(DamagePostfix)));
        if (death != null)
            _harmony.Patch(death, postfix: new HarmonyMethod(typeof(Plugin), nameof(DeathPostfix)));

        Logger.LogInfo($"{PluginName} loaded. Damage.DetermineTargetHit is intentionally untouched.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();

        foreach (GameObject effect in _ownedEffects)
            if (effect != null) Destroy(effect);

        _ownedEffects.Clear();
        _lastImpactByTarget.Clear();
        _deathSeen.Clear();
        Instance = null;
    }

    private static void DamagePostfix(object __instance, object damage)
        => Instance?.HandleDamage(__instance, damage);

    private static void DeathPostfix(object __instance, object outcome)
        => Instance?.HandleDeath(__instance, outcome);

    private void HandleDamage(object healthElement, object damage)
    {
        if (damage == null) return;

        object? target = Read(damage, "TargetPure") ?? Read(healthElement, "ParentModel");
        if (target is not ICharacter character) return;

        bool blocked = Read(damage, "IsBlocked") is bool b && b;
        bool parried = Read(damage, "IsParried") is bool p && p;
        bool dot = Read(damage, "IsDamageOverTime") is bool d && d;
        if (blocked || parried || dot) return;

        int key = RuntimeHelpers.GetHashCode(target);
        float now = Time.unscaledTime;
        if (_lastImpactByTarget.TryGetValue(key, out float last) && now - last < 0.10f)
            return;

        _lastImpactByTarget[key] = now;
        SpawnBurst(character.Coords + Vector3.up * 0.8f, 8, 0.07f, 1.3f, 1.2f);
    }

    private void HandleDeath(object healthElement, object outcome)
    {
        if (outcome == null) return;

        object? target = Read(outcome, "TargetPure")
            ?? Read(Read(outcome, "Damage"), "TargetPure")
            ?? Read(healthElement, "ParentModel");

        if (target is not ICharacter character) return;

        int key = RuntimeHelpers.GetHashCode(target);
        if (!_deathSeen.Add(key))
            return;

        SpawnBurst(character.Coords + Vector3.up * 0.2f, 24, 0.14f, 2.0f, 1.8f);
    }

    private void SpawnBurst(Vector3 position, int count, float size, float speed, float lifetime)
    {
        GameObject effect = new("TGCommunity_CombatVfxSidecar");
        effect.transform.position = position;
        _ownedEffects.Add(effect);

        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = 0.35f;
        main.startSpeed = speed;
        main.startSize = size;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = false;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.12f;

        particles.Emit(count);
        Destroy(effect, lifetime);
    }

    private static object? Read(object? owner, string name)
    {
        if (owner == null) return null;
        Type type = owner.GetType();

        PropertyInfo? property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property != null && property.GetIndexParameters().Length == 0)
        {
            try { return property.GetValue(owner); } catch { }
        }

        FieldInfo? field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            try { return field.GetValue(owner); } catch { }
        }

        return null;
    }
}
