using System;
using System.Reflection;
using Awaken.TG.Main.Animations.FSM.Heroes.States.Block;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Fights.DamageInfo;
using Awaken.TG.Main.Heroes;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TGExample.GuardBlockParryObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.guard-block-parry-observer";
    public const string PluginName = "TG Example - Guard/Block/Parry Observer";
    public const string PluginVersion = "0.1.0";

    internal static ManualLogSource? LogSource { get; private set; }

    private Harmony? _harmony;

    private void Awake()
    {
        LogSource = Logger;
        _harmony = new Harmony(PluginGuid);
        GuardObserverPatch.Apply(_harmony, Logger);
        Logger.LogInfo("${PluginName} loaded. This example observes defence lifecycle/results only.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        LogSource = null;
    }
}

internal static class GuardObserverPatch
{
    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? guardEnter = AccessTools.Method(typeof(BlockStart), "AfterEnter", new[] { typeof(float) });
        MethodInfo? guardPostfix = AccessTools.Method(typeof(GuardObserverPatch), nameof(GuardEnteredPostfix));

        MethodInfo? damage = AccessTools.Method(typeof(HealthElement), nameof(HealthElement.TakeDamage));
        MethodInfo? damagePostfix = AccessTools.Method(typeof(GuardObserverPatch), nameof(DamagePostfix));

        if (guardEnter != null && guardPostfix != null)
        {
            harmony.Patch(guardEnter, postfix: new HarmonyMethod(guardPostfix));
            logger.LogInfo("Patched BlockStart.AfterEnter for read-only guard-entry observation.");
        }
        else
        {
            logger.LogWarning("BlockStart.AfterEnter target was not found.");
        }

        if (damage != null && damagePostfix != null)
        {
            harmony.Patch(damage, postfix: new HarmonyMethod(damagePostfix));
            logger.LogInfo("Patched HealthElement.TakeDamage for read-only block/parry result observation.");
        }
        else
        {
            logger.LogWarning("HealthElement.TakeDamage target was not found.");
        }
    }

    private static void GuardEnteredPostfix(BlockStart __instance)
    {
        try
        {
            Hero? hero = Hero.Current;
            if (hero == null || hero.HasBeenDiscarded || __instance?.Hero == null || !ReferenceEquals(__instance.Hero, hero))
            {
                return;
            }

            Plugin.LogSource?.LogInfo("Guard lifecycle: event=guard-enter; hero=true");
        }
        catch (Exception ex)
        {
            Plugin.LogSource?.LogWarning("$Guard-entry observation failed: {ex.Message}");
        }
    }

    private static void DamagePostfix(HealthElement __instance, Damage damage)
    {
        if (damage == null || (!damage.IsBlocked && !damage.IsParried))
        {
            return;
        }

        try
        {
            Hero? hero = Hero.Current;
            if (hero == null || hero.HasBeenDiscarded)
            {
                return;
            }

            object? target = damage.TargetPure ?? __instance?.ParentModel;
            if (!ReferenceEquals(target, hero))
            {
                return;
            }

            string result = damage.IsParried
                ? "parried"
                : "blocked";

            Plugin.LogSource?.LogInfo(
                "$Guard result: result={result}; amount={damage.Amount:0.###}; " +
                "$staminaDamage={damage.StaminaDamageAmount:0.###}; " +
                "$blocked={damage.IsBlocked}; parried={damage.IsParried}");
        }
        catch (Exception ex)
        {
            Plugin.LogSource?.LogWarning("$Block/parry result observation failed: {ex.Message}");
        }
    }
}
