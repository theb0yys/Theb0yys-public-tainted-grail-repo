using System;

namespace AvalonExceptions;

internal static class ManagedDependencyCascadeEvidence
{
    internal const string FollowOnRouteId = "managed-dependency-follow-on-cascade";

    internal static bool LooksLikeEcsFollowOn(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        bool hasSceneDisable = text.IndexOf("Awaken.ECS.Debugging.SceneDisableEcsRendering", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("SceneDisableEcsRendering", StringComparison.OrdinalIgnoreCase) >= 0;
        bool hasEcsTypeManager = text.IndexOf("Unity.Entities.TypeManager", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("Awaken.ECS", StringComparison.OrdinalIgnoreCase) >= 0;
        bool hasFollowOnException = text.IndexOf("NullReferenceException", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("TypeInitializationException", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("ManagedException", StringComparison.OrdinalIgnoreCase) >= 0;

        return hasSceneDisable && hasEcsTypeManager && hasFollowOnException;
    }

    internal static bool LooksLikeHighSignalDependencyEvidence(string kind, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return LooksLikeLoaderDependencyEvidence(kind, text)
            || LooksLikeManagedDependencyMismatch(text)
            || LooksLikeEcsFollowOn(text);
    }

    private static bool LooksLikeLoaderDependencyEvidence(string kind, string text)
    {
        return string.Equals(kind, "bepinex_loader_dependency_error", StringComparison.OrdinalIgnoreCase)
            || text.IndexOf("BepInEx loader dependency error", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("Chainloader.DependencyErrors", StringComparison.OrdinalIgnoreCase) >= 0
            || (text.IndexOf("Could not load [", StringComparison.OrdinalIgnoreCase) >= 0
                && text.IndexOf("missing dependencies", StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static bool LooksLikeManagedDependencyMismatch(string text)
    {
        bool reflectionTypeLoad = text.IndexOf("ReflectionTypeLoadException", StringComparison.OrdinalIgnoreCase) >= 0;
        bool typeResolutionFailure = text.IndexOf("Could not load type", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("Could not resolve type", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("TypeLoadException", StringComparison.OrdinalIgnoreCase) >= 0;
        bool missingAssembly = text.IndexOf("Could not load file or assembly", StringComparison.OrdinalIgnoreCase) >= 0;

        return (reflectionTypeLoad && typeResolutionFailure) || missingAssembly;
    }
}
