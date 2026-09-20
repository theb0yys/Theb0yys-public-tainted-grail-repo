using Awaken.TG.Main.UI.TitleScreen;
using HarmonyLib;

namespace ImmersiveBackgrounds.Patches;

[HarmonyPatch(typeof(TitleScreenUtils), nameof(TitleScreenUtils.StartNewGame))]
internal static class TitleScreenUtilsStartNewGamePatch
{
    private static void Prefix(StartGameData data)
    {
        Plugin.CaptureStartGameData(data);
    }
}
