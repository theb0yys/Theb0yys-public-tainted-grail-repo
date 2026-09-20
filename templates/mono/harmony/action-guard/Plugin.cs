using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGTemplate.ActionGuard;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.template.action-guard";
    public const string PluginName = "TG Action Guard Template";
    public const string PluginVersion = "0.1.0";

    private Harmony? _harmony;
    private ConfigEntry<bool>? _allowAction;

    internal static bool AllowAction => Instance?._allowAction?.Value ?? true;
    internal static Plugin? Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _allowAction = Config.Bind("Demo", "AllowAction", false, "Allow the self-owned demo action.");

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);

        bool result = DemoAction.TryExecute();
        Logger.LogInfo("Action Guard Template demo: allowed=" + AllowAction +
                       " result=" + result +
                       " originalExecutions=" + DemoAction.ExecutionCount);
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        Instance = null;
    }
}

internal static class DemoAction
{
    internal static int ExecutionCount { get; private set; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static bool TryExecute()
    {
        ExecutionCount++;
        return true;
    }
}

[HarmonyPatch(typeof(DemoAction), nameof(DemoAction.TryExecute))]
internal static class DemoActionGuardPatch
{
    private static bool Prefix(ref bool __result)
    {
        if (Plugin.AllowAction)
        {
            return true;
        }

        __result = false;
        return false;
    }
}
