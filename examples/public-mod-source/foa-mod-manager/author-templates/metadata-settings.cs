using BepInEx.Configuration;
using FoAModManager;
using UnityEngine;

namespace ExampleFoAMod;

internal enum ExampleSpawnRate
{
    Off,
    Rare,
    Normal,
    Frequent
}

internal sealed class ExampleSettings
{
    internal ConfigEntry<bool> Enabled { get; }
    internal ConfigEntry<ExampleSpawnRate> SpawnRate { get; }
    internal ConfigEntry<float> SearchRadius { get; }
    internal ConfigEntry<KeyCode> OpenPanelKey { get; }

    internal ExampleSettings(ConfigFile config)
    {
        Enabled = config.Bind(
            "General",
            "Enabled",
            true,
            new ConfigDescription(
                "Enable Example Mod.",
                null,
                new FoAModSettingUiMetadata
                {
                    DisplaySection = "General",
                    DisplayName = "Enabled",
                    SectionOrder = 0,
                    Order = 0
                }));

        SpawnRate = config.Bind(
            "Encounters",
            "SpawnRate",
            ExampleSpawnRate.Rare,
            new ConfigDescription(
                "Controls how often the encounter can appear.",
                null,
                new FoAModSettingUiMetadata
                {
                    DisplaySection = "Encounters",
                    DisplayName = "Spawn Rate",
                    ChoiceLabels = "Off=Disabled;Rare=Rare;Normal=Normal;Frequent=Frequent",
                    SectionOrder = 10,
                    Order = 0
                }));

        SearchRadius = config.Bind(
            "Detection",
            "SearchRadius",
            30f,
            new ConfigDescription(
                "Search radius in meters.",
                new AcceptableValueRange<float>(5f, 100f),
                new FoAModSettingUiMetadata
                {
                    DisplaySection = "Detection",
                    DisplayName = "Search Radius",
                    SectionOrder = 20,
                    Order = 0
                }));

        OpenPanelKey = config.Bind(
            "Controls",
            "OpenPanelKey",
            KeyCode.F8,
            new ConfigDescription(
                "Open or close the Example Mod panel.",
                null,
                new FoAModSettingUiMetadata
                {
                    DisplaySection = "Controls",
                    DisplayName = "Open Panel",
                    SectionOrder = 30,
                    Order = 0
                }));
    }
}
