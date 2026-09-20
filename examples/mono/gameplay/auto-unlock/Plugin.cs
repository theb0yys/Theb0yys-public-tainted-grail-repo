using System;
using System.Reflection;
using Awaken.TG.Main.Fights.Factions.Crimes;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Interactions;
using Awaken.TG.Main.Locations.Actions.Lockpicking;
using BepInEx;
using HarmonyLib;

namespace TGCommunity.Example.AutoUnlock;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.auto-unlock";
    public const string PluginName = "TG Example - Auto Unlock";
    public const string PluginVersion = "0.1.0";

    private static readonly MethodInfo? HeroCanLockpick =
        AccessTools.PropertyGetter(typeof(LockAction), "HeroCanLockpick");
    private static readonly MethodInfo? WillBeOpenWithKey =
        AccessTools.PropertyGetter(typeof(LockAction), "WillBeOpenWithKey");
    private static readonly MethodInfo? Unlock =
        AccessTools.Method(typeof(LockAction), "Unlock", new[] { typeof(bool) });

    private Harmony? _harmony;

    private void Awake()
    {
        MethodInfo? target = AccessTools.Method(
            typeof(LockAction),
            "OnStart",
            new[] { typeof(Hero), typeof(IInteractableWithHero) });
        MethodInfo? prefix = AccessTools.Method(typeof(Plugin), nameof(Prefix));

        if (target == null || prefix == null ||
            HeroCanLockpick == null || WillBeOpenWithKey == null || Unlock == null)
        {
            Logger.LogWarning("Auto-unlock prerequisites were not found. Vanilla lockpicking remains active.");
            return;
        }

        _harmony = new Harmony(PluginGuid);
        _harmony.Patch(target, prefix: new HarmonyMethod(prefix));
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }

    private static bool Prefix(LockAction __instance)
    {
        try
        {
            if (__instance.ParentModel.HasElement<LockpickingInteraction>())
            {
                return true;
            }

            if (ReadBool(WillBeOpenWithKey!, __instance))
            {
                return true;
            }

            if (!ReadBool(HeroCanLockpick!, __instance))
            {
                return true;
            }

            Unlock!.Invoke(__instance, new object[] { false });
            CommitCrime.Lockpicking(__instance.ParentModel);
            return false;
        }
        catch
        {
            return true;
        }
    }

    private static bool ReadBool(MethodInfo getter, LockAction instance)
    {
        return getter.Invoke(instance, Array.Empty<object>()) is true;
    }
}
