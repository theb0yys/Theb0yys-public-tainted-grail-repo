using System;

namespace FoAModManager;

public static class FoAModManagerApi
{
    public const string TaintedInterfaceCoreThemePackId = "dark-fantasy-rpg-ui-toolkit";
    public const string TaintedInterfaceFantasyRpgGuiPackId = "fantasy-rpg-gui";
    public const string TaintedInterfaceHeatCompleteModernUiPackId = "heat-complete-modern-ui";
    public const string TaintedInterfaceFantasyNouveauUiPackId = "fantasy-nouveau-ui";
    public const string TaintedInterfaceTribalUiSetPackId = "tribal-ui-set";
    public const string TaintedInterfaceGameUiEmptyBoxSet4kPackId = "game-ui-empty-box-set-4k";
    public const string TaintedInterfaceFantasyRpgUiKitPackId = "fantasy-rpg-ui-kit";

    public static bool IsOpen => Plugin.IsManagerVisible;

    public static bool IsControllerCursorActive => Plugin.IsControllerCursorActive;

    public static bool IsControllerCursorInputReadActive => Plugin.IsControllerCursorInputReadActive;

    public static bool IsModUiInputOwned => Plugin.IsModUiInputOwned;

    public static bool IsCustomUiScopeActive => Plugin.IsCustomUiScopeActive;

    public static string GetTaintedInterfaceUiPackId()
    {
        return Plugin.GetTaintedInterfaceUiPackId();
    }

    public static void Open()
    {
        Plugin.ShowManager();
    }

    public static void Close()
    {
        Plugin.HideManager();
    }

    public static void Refresh()
    {
        Plugin.RefreshManager();
    }

    public static void SetControllerCursorScope(string ownerId, bool active)
    {
        Plugin.SetControllerCursorScope(ownerId, active);
    }

    public static void SetCustomUiScope(string ownerId, bool active)
    {
        Plugin.SetCustomUiScope(ownerId, active, freezeWorld: true);
    }

    public static void SetCustomUiScope(string ownerId, bool active, bool freezeWorld)
    {
        Plugin.SetCustomUiScope(ownerId, active, freezeWorld);
    }

    public static bool RegisterControllerAction(string actionId, string displayName, Action callback)
    {
        return Plugin.RegisterControllerAction(actionId, displayName, callback);
    }

    public static bool RegisterControllerAction(string actionId, string displayName, string category, string description, Action callback)
    {
        return Plugin.RegisterControllerAction(actionId, displayName, category, description, callback);
    }

    public static bool UnregisterControllerAction(string actionId)
    {
        return Plugin.UnregisterControllerAction(actionId);
    }

    public static bool RegisterStatusProvider(string providerId, string displayName, Func<FoAModStatusSnapshot> snapshotProvider)
    {
        return Plugin.RegisterStatusProvider(providerId, displayName, snapshotProvider);
    }

    public static bool RegisterStatusProvider(string providerId, string displayName, string category, string description, Func<FoAModStatusSnapshot> snapshotProvider)
    {
        return Plugin.RegisterStatusProvider(providerId, displayName, category, description, snapshotProvider);
    }

    public static bool UnregisterStatusProvider(string providerId)
    {
        return Plugin.UnregisterStatusProvider(providerId);
    }
}

public enum FoAModStatusLevel
{
    Unknown = 0,
    Ok = 1,
    Info = 2,
    Warning = 3,
    Problem = 4
}

public sealed class FoAModStatusSnapshot
{
    public FoAModStatusLevel Level { get; set; } = FoAModStatusLevel.Info;
    public string Summary { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Schema { get; set; } = string.Empty;
    public string UpdatedUtc { get; set; } = string.Empty;
    public string[] Lines { get; set; } = Array.Empty<string>();
}

public sealed class FoAModSettingUiMetadata
{
    public string DisplaySection { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ChoiceLabels { get; set; } = string.Empty;
    public int SectionOrder { get; set; } = int.MaxValue;
    public int Order { get; set; } = int.MaxValue;
    public bool Hidden { get; set; }
}
