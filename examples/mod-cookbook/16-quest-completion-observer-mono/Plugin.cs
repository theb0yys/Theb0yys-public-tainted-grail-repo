using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Awaken.TG.Main.Stories.Quests;
using BepInEx;
using HarmonyLib;

namespace TGExample.QuestObserver;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.quest-observer";
    public const string PluginName = "TG Example - Quest Observer";
    public const string PluginVersion = "0.1.0";

    internal static Plugin? Instance { get; private set; }
    private Harmony? _harmony;

    private void Awake()
    {
        Instance = this;
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        Instance = null;
    }
}

[HarmonyPatch]
internal static class QuestCompletionPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        return AccessTools.GetDeclaredMethods(typeof(QuestUtils))
            .Where(IsCandidate)
            .Cast<MethodBase>();
    }

    private static bool IsCandidate(MethodInfo method)
    {
        if (method.Name == "Complete" && method.GetParameters().Length == 2)
        {
            return true;
        }

        if (method.Name != "SetQuestState")
        {
            return false;
        }

        ParameterInfo[] parameters = method.GetParameters();
        return parameters.Length == 2 && parameters[1].ParameterType == typeof(QuestState);
    }

    private static void Prefix(MethodBase __originalMethod, object[] __args)
    {
        if (__originalMethod.Name == "Complete")
        {
            Plugin.Instance?.Logger.LogInfo("QuestUtils.Complete candidate observed.");
            return;
        }

        if (__args.Length == 2 && __args[1] is QuestState state && state == QuestState.Completed)
        {
            Plugin.Instance?.Logger.LogInfo("QuestUtils.SetQuestState(..., Completed) candidate observed.");
        }
    }
}
