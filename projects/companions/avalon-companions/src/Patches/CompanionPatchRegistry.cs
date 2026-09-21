using BepInEx.Logging;
using HarmonyLib;

namespace AvalonCompanions.Patches;

internal static class CompanionPatchRegistry
{
    internal static void Apply(Harmony harmony, ManualLogSource logger)
    {
        PanelInputLockPatch.Apply(harmony, logger);
        ProjectileAcquisitionProbe.Apply(harmony, logger);
        logger.LogInfo("Registered Avalon Companions input and default-off acquisition diagnostic patches. Companion actor behavior remains research-gated.");
    }
}
