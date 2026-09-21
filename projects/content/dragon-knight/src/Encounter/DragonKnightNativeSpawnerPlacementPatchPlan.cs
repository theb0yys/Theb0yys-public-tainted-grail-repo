using System.Reflection;
using Awaken.TG.Main.Locations;
using Awaken.TG.Main.Locations.Spawners;
using BepInEx.Logging;
using HarmonyLib;

namespace DragonKnight;

internal static class DragonKnightNativeSpawnerPlacementPatchPlan
{
    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        MethodInfo? initTarget = AccessTools.Method(typeof(LocationSpawner), nameof(LocationSpawner.InitFromAttachment));
        MethodInfo? initPostfix = AccessTools.Method(typeof(DragonKnightNativeSpawnerPlacementPatchPlan), nameof(LocationSpawnerInitPostfix));
        MethodInfo? spawnTarget = AccessTools.Method(typeof(BaseLocationSpawner), "OnLocationSpawned");
        MethodInfo? spawnPostfix = AccessTools.Method(typeof(DragonKnightNativeSpawnerPlacementPatchPlan), nameof(OnLocationSpawnedPostfix));

        if (initTarget == null || initPostfix == null || spawnTarget == null || spawnPostfix == null)
        {
            logger.LogWarning(
                "DRAGON_KNIGHT_NATIVE_SPAWNER_PATCH_BLOCKED"
                + " reason=hook-resolution-failed"
                + " init-hook=" + (initTarget != null && initPostfix != null ? "1" : "0")
                + " spawn-hook=" + (spawnTarget != null && spawnPostfix != null ? "1" : "0")
                + " native-spawner=0 native-candidate=0 direct-spawn=0 save-write=0");
            return;
        }

        harmony.Patch(initTarget, postfix: new HarmonyMethod(initPostfix));
        harmony.Patch(spawnTarget, postfix: new HarmonyMethod(spawnPostfix));
        logger.LogInfo(
            "DRAGON_KNIGHT_NATIVE_SPAWNER_PATCHED"
            + " route=" + DragonKnightNativeSpawnerPlacementContract.RouteId
            + " native-spawner=1 native-candidate=1 direct-spawn=0 save-owner=BaseLocationSpawner");
    }

    private static void LocationSpawnerInitPostfix(LocationSpawner __instance, LocationSpawnerAttachment spec, bool isRestored)
    {
        Plugin.RegisterDragonKnightNativeSpawner(__instance, spec, isRestored);
    }

    private static void OnLocationSpawnedPostfix(BaseLocationSpawner __instance, Location location, int id)
    {
        Plugin.ObserveDragonKnightNativeSpawn(__instance, location, id);
    }
}
