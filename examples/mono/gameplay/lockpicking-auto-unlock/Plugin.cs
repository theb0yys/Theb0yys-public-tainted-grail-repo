using System.Reflection;
using Awaken.TG.Main.Fights.Factions.Crimes;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.Interactions;
using Awaken.TG.Main.Locations.Actions.Lockpicking;
using BepInEx;
using HarmonyLib;

namespace TGCommunity.Example.LockpickingAutoUnlock;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.lockpicking-auto-unlock";
    public const string PluginName = "TG Example - Lockpicking Auto Unlock";
    public const string PluginVersion = "0.1.0";

    private static readonly MethodInfo? HeroCanLockpick = AccessTools.PropertyGetter(typeof(LockAction), "HeroCanLockpick");
    private static readonly MethodInfo? WillBeOpenWithKey = AccessTools.PropertyGetter(typeof(LockAction), "WillBeOpenWithKey");
    private static readonly MethodInfo? Unlock = AccessTools.Method(typeof(LockAction), "Unlock", new[] { typeof(bool) });
    private Harmony? _harmony;

    private void Awake()
    {
        MethodInfo? target = AccessTools.Method(
            typeof(LockAction),
            "OnStart",
            new[] { typeof(Hero), typeof(IInteractableWithHero) });

        if (target == null || HeroCanLockpick == null || WillBeOpenWithKey == null || Unlock == null)
        {
            Logger.LogError("Required LockAction members were not found. Vanilla behavior is untouched.");
            return;
        }

        _harmony = new Harmony(PluginGuid);
        _harmony.Patch(target, prefix: new HarmonyMethod(typeof(Plugin), nameof(Prefix)));
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy() => _harmony?.UnpatchSelf();

    private static bool Prefix(LockAction __instance)
    {
        if (__instance.ParentModel.HasElement<LockpickingInteraction>())
            return true;

        if (ReadBool(WillBeOpenWithKey!, __instance))
            return true;

        if (!ReadBool(HeroCanLockpick!, __instance))
            return true;

        try
        {
            Unlock!.Invoke(__instance, new object[] { false });
            CommitCrime.Lockpicking(__instance.ParentModel);
            return false;
        }
        catch
        {
            return true;
        }
    }

    private static bool ReadBool(MethodInfo getter, LockAction owner)
        => getter.Invoke(owner, null) is bool value && value;
}
