using AvalonCreatureCompanionShared;
using BepInEx;
using UnityEngine;

namespace AvalonGoblinCompanion;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : StandaloneCreatureCompanionPlugin
{
    public const string PluginGuid = "kane.tgfoa.avalon-goblin-companion";
    public const string PluginName = "Avalon Goblin Companion";
    public const string PluginVersion = "0.1.0";

    private static readonly StandaloneCreatureCompanionDefinition CreatureDefinition = new(
        "goblin",
        "Goblin",
        "b7d0d4e6c0de4bb0a000000000000011",
        "ItemTemplate_Magic_Tier1_AvalonGoblinCall",
        "Goblin's Call",
        "Spec_Goblin_CI4",
        "3b66e9f376df39b47b3908a75fe7d2d4",
        "bd31ca6f2fe2b8047b19eace29f926ca",
        "TryResolveWorldGoblinTemplate",
        "avalon-awakened/creatures/goblin/nonhuman-humanoid/visual-v1",
        1f,
        new Color32(42, 86, 36, 255),
        new Color32(185, 136, 50, 255),
        templateVisualAddress: "6ff44ed61c7be0f46afb7436cf4daf43",
        runtimeOverlayVisualAddress: "avalon-awakened/creatures/goblin/nonhuman-humanoid/visual-v1");

    protected override StandaloneCreatureCompanionDefinition Definition => CreatureDefinition;
    protected override string PluginGuidValue => PluginGuid;
    protected override string PluginNameValue => PluginName;
    protected override string PluginVersionValue => PluginVersion;
}
