using AvalonCreatureCompanionShared;
using BepInEx;
using UnityEngine;

namespace AvalonElephantCompanion;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : StandaloneCreatureCompanionPlugin
{
    public const string PluginGuid = "kane.tgfoa.avalon-elephant-companion";
    public const string PluginName = "Avalon Elephant Companion";
    public const string PluginVersion = "0.1.0";

    private static readonly StandaloneCreatureCompanionDefinition CreatureDefinition = new(
        "fantasy-elephant",
        "Fantasy Elephant",
        "b7d0d4e6c0de4bb0a000000000000007",
        "ItemTemplate_Magic_Tier1_AvalonElephantCall",
        "Elephant's Call",
        "Spec_FantasyElephant_CI4",
        "8a1d351fe1b426c4b879a8f1724b7867",
        "8c827d370df13d640a4fc8efea55b034",
        "TryResolveWorldElephantTemplate",
        "avalon-awakened/creatures/fantasy-elephant/ci4-controlled/visual--0f2971ddc6ef5064292f716315f91379",
        1f,
        new Color32(77, 72, 66, 255),
        new Color32(210, 182, 105, 255),
        embeddedIconResourceName: "AvalonElephantCompanion.Icons.mumakil.png");

    protected override StandaloneCreatureCompanionDefinition Definition => CreatureDefinition;
    protected override string PluginGuidValue => PluginGuid;
    protected override string PluginNameValue => PluginName;
    protected override string PluginVersionValue => PluginVersion;
}
