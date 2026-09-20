using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Heroes.CharacterCreators.PresetSelection;
using Awaken.TG.Main.Heroes.Items;
using HarmonyLib;
#if IL2CPP
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#endif

namespace ImmersiveBackgrounds.Patches;

[HarmonyPatch(typeof(ItemSet), nameof(ItemSet.ApplyStats))]
internal static class ItemSetApplyStatsPatch
{
#if IL2CPP
    private static bool Prefix(Hero hero, Il2CppReferenceArray<StatPreset> stats, bool ignoreLevelSetting, ref bool __result)
#else
    private static bool Prefix(Hero hero, StatPreset[] stats, bool ignoreLevelSetting, ref bool __result)
#endif
    {
        if (!Plugin.TryReplaceBackgroundStats(hero, stats, ignoreLevelSetting, out bool result))
        {
            return true;
        }

        __result = result;
        return false;
    }
}
