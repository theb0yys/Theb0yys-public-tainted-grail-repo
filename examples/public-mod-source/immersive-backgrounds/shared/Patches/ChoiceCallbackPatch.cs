using Awaken.TG.Main.Stories.Choices;
using HarmonyLib;

namespace ImmersiveBackgrounds.Patches;

[HarmonyPatch(typeof(VChoice), nameof(VChoice.Select), new[] { typeof(Choice) })]
internal static class VChoiceSelectPatch
{
    private static void Prefix(Choice choice)
    {
        Plugin.CaptureBackgroundChoiceSelection(choice);
    }

    private static void Postfix()
    {
        Plugin.CompleteBackgroundChoiceSelection();
    }
}