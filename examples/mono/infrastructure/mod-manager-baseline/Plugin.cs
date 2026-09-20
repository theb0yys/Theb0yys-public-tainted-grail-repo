using System;
using BepInEx;
using BepInEx.Configuration;
using FoAModManager;

namespace TGCommunity.ModManagerIntegration;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("kane.tgfoa.mod-manager", BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.infrastructure.mod-manager";
    public const string PluginName = "TG Community Mod Manager Integration";
    public const string PluginVersion = "0.1.0";

    private const string ActionId = PluginGuid + ".toggle";
    private const string StatusId = PluginGuid + ".status";

    private ConfigEntry<bool> _enabled = null!;

    private void Awake()
    {
        _enabled = Config.Bind(
            "General",
            "Enabled",
            true,
            "Enable the example feature.");

        FoAModManagerApi.RegisterControllerAction(
            ActionId,
            "Toggle Example Feature",
            "Infrastructure Example",
            "Toggle the example setting.",
            ToggleFeature);

        FoAModManagerApi.RegisterStatusProvider(
            StatusId,
            "Infrastructure Example",
            "Infrastructure Example",
            "Read-only status for the example.",
            BuildStatus);
    }

    private void OnDestroy()
    {
        FoAModManagerApi.UnregisterControllerAction(ActionId);
        FoAModManagerApi.UnregisterStatusProvider(StatusId);
    }

    private void ToggleFeature()
    {
        _enabled.Value = !_enabled.Value;
    }

    private FoAModStatusSnapshot BuildStatus()
    {
        return new FoAModStatusSnapshot
        {
            Level = _enabled.Value ? FoAModStatusLevel.Ok : FoAModStatusLevel.Info,
            Summary = _enabled.Value ? "Enabled" : "Disabled",
            Detail = "Read-only status snapshot from the example mod.",
            Schema = "tgcommunity-mod-manager-example/1",
            UpdatedUtc = DateTime.UtcNow.ToString("O"),
            Lines = new[] { "MutationFromStatusProvider=false" }
        };
    }
}
