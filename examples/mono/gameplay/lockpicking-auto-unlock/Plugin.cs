using System;
using System.Reflection;
using Awaken.TG.Main.Fights.Factions.Crimes;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Interactions;
using Awaken.TG.Main.Locations.Actions.Lockpicking;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGCommunity.Example.LockpickingAutoUnlock;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.lockpicking-auto-unlock";
    public const string PluginName = "TG Example - Lockpicking Auto Unlock";
    public const string PluginVersion = "0.1.0";

    private static readonly MethodInfo? HeroCanLockpickGetter = AccessTools.PropertyGetter(typeof(LockAction), "HeroCanLockpick");
    private static readonly MethodInfo? WillBeOpenWithKeyGetter = AccessTools.PropertyGetter(typeof(LockAction), "WillBeOpenWithKey");
    private static readonly MethodInfo? UnlockMethod = AccessTools.Method(typeof(LockAction), "Unlock", new[] { typeof(bool) });

    private static ConfigEntry<bool>? _enabled;
    private static BepInEx.Logging.ManualLogSource? _log;
    private Harmony? _harmony;

    private void Awake()
    {
        _enabled = Config.Bind("AutoUnlock", "Enabled", true, "Skip only the normal lockpicking minigame when native gates allow lockpicking.");
        _log = Logger;

        var target = AccessTools.Method(typeof(LockAction), "OnStart", new[] { typeof(Hero), typeof(IInteractableWithHero) });
        var prefix = AccessTools.Method(typeof(Plugin), nameof(Prefix));

        if (target == null || prefix == null || HeroCanLockpickGetter == null || WillBeOpenWithKeyGetter == null || UnlockMethod == null)
        {
            Logger.LogError("Auto-unlock members were not found. Vanilla lockpicking remains unchanged.");
            return;
        }

        _harmony = new Harmony(PluginGuid);
        _harmony.Patch(target, prefix: new HarmonyMethod(prefix));
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        _log = null;
    }

    private static bool Prefix(LockAction __instance)
    {
        if (_enabled?.Value != true)
        {
            return true;
        }

        try
        {
            if (__instance.ParentModel.HasElement<LockpickingInteraction>())
            {
                return true;
            }

            bool opensWithKey = WillBeOpenWithKeyGetter?.Invoke(__instance, null) is bool key && key;
            bool canLockpick = HeroCanLockpickGetter?.Invoke(__instance, null) is bool can && can;

            if (opensWithKey || !canLockpick)
            {
                return true;
            }

            UnlockMethod!.Invoke(__instance, new object[] { false });
            CommitCrime.Lockpicking(__instance.ParentModel);
            _log?.LogInfo("Auto-unlock used LockAction.Unlock(false) and native lockpicking crime routing.");
            return false;
        }
        catch (Exception ex)
        {
            _log?.LogWarning($"Auto-unlock failed closed: {ex.GetType().Name}: {ex.Message}");
            return true;
        }
    }
}
