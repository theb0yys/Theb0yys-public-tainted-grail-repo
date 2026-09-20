using Tainted.Abstractions.Runtime;

namespace TGTemplate.LockpickingReforged;

internal enum LockPreset
{
    Vanilla,
    AutoUnlock,
    VeryEasy,
    Easy,
    Medium,
    Hard,
    VeryHard,
    Custom
}

internal static class Feature
{
    internal const string SourceFamily = "lockpicking-reforged";

    internal static bool ShouldAutoUnlock(
        LockPreset preset,
        bool hasLockpick,
        bool keyOnly)
        => preset == LockPreset.AutoUnlock && hasLockpick && !keyOnly;

    internal static float ResolveTolerance(
        LockPreset preset,
        float nativeTolerance,
        float customTolerance)
        => preset == LockPreset.Custom ? customTolerance : nativeTolerance;

    internal static string Describe(TaintedRuntimeKind runtimeKind)
        => "Lockpicking Reforged starter initialized. runtime=" + runtimeKind +
           "; native-key-and-lockpick-gates-preserved";
}
