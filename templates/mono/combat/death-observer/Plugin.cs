using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Heroes;
using BepInEx;
using HarmonyLib;

namespace TGTemplate.DeathObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.template.death-observer";
    public const string PluginName = "TG Death Observer Template";
    public const string PluginVersion = "0.1.0";

    private static Plugin? s_instance;
    private static readonly HashSet<int> SeenTargets = new();
    private Harmony? _harmony;

    private void Awake()
    {
        s_instance = this;
        _harmony = new Harmony(PluginGuid);

        var target = AccessTools.Method(typeof(HealthElement), "OnDeathEvents");
        var postfix = AccessTools.Method(typeof(Plugin), nameof(DeathPostfix));
        if (target == null || postfix == null)
        {
            Logger.LogWarning("HealthElement.OnDeathEvents target was not found.");
            return;
        }

        _harmony.Patch(target, postfix: new HarmonyMethod(postfix));
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private static void DeathPostfix(HealthElement __instance)
    {
        try
        {
            if (__instance?.ParentModel is not ICharacter target || s_instance == null)
                return;

            int id = RuntimeHelpers.GetHashCode(target);
            if (!SeenTargets.Add(id))
                return;

            bool isHero = ReferenceEquals(target, Hero.Current);
            s_instance.Logger.LogInfo(
                $"Character death: kind={(isHero ? "hero" : "npc")}; targetType={target.GetType().Name}");
        }
        catch (Exception ex)
        {
            s_instance?.Logger.LogWarning($"Death observation failed: {ex.Message}");
        }
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        SeenTargets.Clear();
        s_instance = null;
    }
}
