using System;
using System.Reflection;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights.DamageInfo;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGTemplate.DamageObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.template.damage-observer";
    public const string PluginName = "TG Damage Observer Template";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<int> _maxRows = null!;
    private Harmony? _harmony;
    private int _rows;

    private void Awake()
    {
        s_instance = this;
        _maxRows = Config.Bind("Diagnostics", "MaxRows", 25,
            new ConfigDescription("Maximum character-damage rows logged per process.",
                new AcceptableValueRange<int>(0, 250)));

        _harmony = new Harmony(PluginGuid);
        MethodInfo? target = AccessTools.Method(typeof(HealthElement), nameof(HealthElement.TakeDamage));
        MethodInfo? postfix = AccessTools.Method(typeof(Plugin), nameof(DamagePostfix));

        if (target == null || postfix == null)
        {
            Logger.LogWarning("HealthElement.TakeDamage target was not found.");
            return;
        }

        _harmony.Patch(target, postfix: new HarmonyMethod(postfix));
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private static void DamagePostfix(HealthElement __instance, Damage damage)
    {
        s_instance?.Observe(__instance, damage);
    }

    private void Observe(HealthElement health, Damage damage)
    {
        int maxRows = Math.Max(0, Math.Min(250, _maxRows.Value));
        if (_rows >= maxRows || damage == null) return;

        object? target = damage.TargetPure ?? health?.ParentModel;
        if (target is not ICharacter) return;

        _rows++;
        Logger.LogInfo(
            $"Character damage row={_rows}/{maxRows}; " +
            $"source={damage.DamageDealerPure?.GetType().Name ?? "<none>"}; " +
            $"target={target.GetType().Name}; damageType={damage.Type}");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        s_instance = null;
    }
}
