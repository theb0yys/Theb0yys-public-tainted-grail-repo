using System;
using System.Reflection;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights.DamageInfo;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGExample.CharacterDamageObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.character-damage-observer";
    public const string PluginName = "TG Example - Character Damage Observer";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<int> _maxRows = null!;
    private Harmony? _harmony;
    private int _rows;

    internal static Plugin? Instance => s_instance;

    private void Awake()
    {
        s_instance = this;
        _maxRows = Config.Bind("Diagnostics", "MaxRows", 25,
            new ConfigDescription("Maximum character-damage rows logged per process.",
                new AcceptableValueRange<int>(0, 250)));

        _harmony = new Harmony(PluginGuid);
        DamageObserverPatch.Apply(_harmony, Logger);
        Logger.LogInfo($"{PluginName} loaded.");
    }

    internal void Observe(HealthElement health, Damage damage)
    {
        int maxRows = Math.Max(0, Math.Min(250, _maxRows.Value));
        if (_rows >= maxRows || damage == null) return;

        object? target = damage.TargetPure ?? health?.ParentModel;
        if (target is not ICharacter) return;

        _rows++;
        string sourceType = damage.DamageDealerPure?.GetType().Name ?? "<none>";
        string targetType = target.GetType().Name;
        Logger.LogInfo($"Character damage observed row={_rows}/{maxRows}; source={sourceType}; target={targetType}; damageType={damage.Type}");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        s_instance = null;
    }
}

internal static class DamageObserverPatch
{
    internal static void Apply(Harmony harmony, BepInEx.Logging.ManualLogSource logger)
    {
        MethodInfo? target = AccessTools.Method(typeof(HealthElement), nameof(HealthElement.TakeDamage));
        MethodInfo? postfix = AccessTools.Method(typeof(DamageObserverPatch), nameof(Postfix));
        if (target == null || postfix == null)
        {
            logger.LogWarning("HealthElement.TakeDamage target was not found.");
            return;
        }

        harmony.Patch(target, postfix: new HarmonyMethod(postfix));
        logger.LogInfo("Patched HealthElement.TakeDamage for observation.");
    }

    private static void Postfix(HealthElement __instance, Damage damage)
    {
        Plugin.Instance?.Observe(__instance, damage);
    }
}
