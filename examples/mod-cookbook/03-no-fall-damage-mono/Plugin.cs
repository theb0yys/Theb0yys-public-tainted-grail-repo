using System;
using System.Reflection;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Heroes;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGExample.NoFallDamage;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.no-fall-damage";
    public const string PluginName = "TG Example - No Fall Damage";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private ConfigEntry<bool> _enabled = null!;
    private Harmony? _harmony;

    internal static bool Enabled => s_instance?._enabled.Value ?? false;

    private void Awake()
    {
        s_instance = this;
        _enabled = Config.Bind("General", "Enabled", true, "Prevent fall damage for the player.");

        _harmony = new Harmony(PluginGuid);

        Type? type = AccessTools.TypeByName("Awaken.TG.Main.Character.FallDamageUtil");
        MethodInfo? target = type == null ? null : AccessTools.Method(type, "DealFallDamage");
        MethodInfo? prefix = AccessTools.Method(typeof(FallDamagePatch), nameof(FallDamagePatch.Prefix));

        if (target == null || prefix == null)
        {
            Logger.LogWarning("FallDamageUtil.DealFallDamage was not found. No patch installed.");
            return;
        }

        _harmony.Patch(target, prefix: new HarmonyMethod(prefix) { priority = Priority.Last });
        Logger.LogInfo($"{PluginName} loaded and patched {type!.FullName}.{target.Name}.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        s_instance = null;
    }
}

internal static class FallDamagePatch
{
    internal static void Prefix(ICharacter character, ref float damageToDeal)
    {
        if (!Plugin.Enabled || character == null || damageToDeal <= 0f)
        {
            return;
        }

        Hero? hero = Hero.Current;
        if (hero == null || hero.HasBeenDiscarded || !ReferenceEquals(character, hero))
        {
            return;
        }

        damageToDeal = 0f;
    }
}
