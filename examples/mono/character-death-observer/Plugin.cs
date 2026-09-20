using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Awaken.TG.Main.Character;
using Awaken.TG.Main.Heroes;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TGExample.CharacterDeathObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.character-death-observer";
    public const string PluginName = "TG Example - Character Death Observer";
    public const string PluginVersion = "0.1.0";

    internal static ManualLogSource? LogSource { get; private set; }

    private static readonly HashSet<int> SeenTargets = new();
    private Harmony? _harmony;

    private void Awake()
    {
        LogSource = Logger;
        _harmony = new Harmony(PluginGuid);

        var target = AccessTools.Method(typeof(HealthElement), "OnDeathEvents");
        var postfix = AccessTools.Method(typeof(Plugin), nameof(DeathPostfix));
        if (target == null || postfix == null)
        {
            Logger.LogWarning("HealthElement.OnDeathEvents target was not found.");
            return;
        }

        _harmony.Patch(target, postfix: new HarmonyMethod(postfix));
        Logger.LogInfo($"{PluginName} loaded. This example observes terminal character death only.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        SeenTargets.Clear();
        LogSource = null;
    }

    private static void DeathPostfix(HealthElement __instance)
    {
        try
        {
            if (__instance?.ParentModel is not ICharacter target)
            {
                return;
            }

            int id = RuntimeHelpers.GetHashCode(target);
            if (!SeenTargets.Add(id))
            {
                return;
            }

            bool isHero = ReferenceEquals(target, Hero.Current);
            LogSource?.LogInfo(
                $"Character death: kind={(isHero ? "hero" : "npc")}; targetType={target.GetType().Name}");
        }
        catch (Exception ex)
        {
            LogSource?.LogWarning($"Character-death observation failed: {ex.Message}");
        }
    }
}
