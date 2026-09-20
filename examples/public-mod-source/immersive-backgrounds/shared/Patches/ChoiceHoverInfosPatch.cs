using Awaken.TG.Main.Stories;
using Awaken.TG.Main.Stories.Choices;
using Awaken.TG.Main.Stories.Steps.Helpers;
using Awaken.Utility.Collections;
using HarmonyLib;

namespace ImmersiveBackgrounds.Patches;

#if IL2CPP
[HarmonyPatch(typeof(Choice), MethodType.Constructor, new[] { typeof(ChoiceConfig), typeof(Story) })]
internal static class ChoiceHoverInfosPatch
{
    private static void Postfix(Choice __instance)
    {
        StructList<IHoverInfo> hoverInfos = __instance._HoverInfos_k__BackingField;
        Plugin.TryReplaceBackgroundHoverInfos(__instance, ref hoverInfos);
        __instance._HoverInfos_k__BackingField = hoverInfos;
    }
}
#else
[HarmonyPatch(typeof(Choice), nameof(Choice.HoverInfos), MethodType.Getter)]
internal static class ChoiceHoverInfosPatch
{
    private static void Postfix(Choice __instance, ref StructList<IHoverInfo> __result)
    {
        Plugin.TryReplaceBackgroundHoverInfos(__instance, ref __result);
    }
}
#endif