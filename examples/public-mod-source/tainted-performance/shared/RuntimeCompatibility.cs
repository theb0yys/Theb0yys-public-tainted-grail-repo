using BepInEx;

namespace TaintedPerformance;

internal static class RuntimeCompatibility
{
#if IL2CPP
    internal const string RuntimeName = "IL2CPP";
    internal const string BranchDescription = "BepInEx v6 IL2CPP plugin; report records the active IL2CPP runtime.";

    internal static string BepInExVersion =>
        typeof(BepInEx.Unity.IL2CPP.BasePlugin).Assembly.GetName().Version?.ToString() ?? "unknown";
#else
    internal const string RuntimeName = "Mono";
    internal const string BranchDescription = "BepInEx v5 Mono plugin; report records the active Mono runtime.";

    internal static string BepInExVersion =>
        typeof(BaseUnityPlugin).Assembly.GetName().Version?.ToString() ?? "unknown";
#endif
}
