using System;
using System.Reflection;
using Awaken.TG.Main.AI.SummonsAndAllies;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights.DamageInfo;
using Awaken.TG.MVC.Events;
using BepInEx.Logging;
using HarmonyLib;

namespace AvalonBroodmotherCompanion;

internal static class BroodmotherSummonFriendlyFirePatch
{
    private static bool s_warningLogged;

    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? target = AccessTools.Method(typeof(NpcHeroSummon), "TryPreventFriendlyFire");
        MethodInfo? prefix = AccessTools.Method(typeof(BroodmotherSummonFriendlyFirePatch), nameof(Prefix));
        if (target == null || prefix == null)
        {
            logger.LogWarning($"{Plugin.PluginName} Broodmother summon friendly-fire patch skipped; target or prefix was not found.");
            return;
        }

        harmony.Patch(target, prefix: new HarmonyMethod(prefix));
    }

    private static bool Prefix(NpcHeroSummon __instance, HookResult<HealthElement, Damage> hook)
    {
        try
        {
            return Plugin.Instance?.ShouldAllowHeroDamageToActiveBroodmother(__instance, hook) == true
                ? false
                : true;
        }
        catch (Exception ex)
        {
            if (!s_warningLogged)
            {
                s_warningLogged = true;
                Plugin.Instance?.ModLogger.LogWarning(
                    $"{Plugin.PluginName} Broodmother friendly-fire bypass check failed: {ex.GetType().Name}: {ex.Message}");
            }

            return true;
        }
    }
}
