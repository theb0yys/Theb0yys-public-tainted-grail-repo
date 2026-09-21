using AvalonCreatureCompanionShared;
using BepInEx;
using UnityEngine;

namespace AvalonBullCompanion;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : StandaloneCreatureCompanionPlugin
{
    public const string PluginGuid = "kane.tgfoa.avalon-bull-companion";
    public const string PluginName = "Avalon Bull Companion";
    public const string PluginVersion = "0.1.0";

    private static readonly StandaloneCreatureCompanionDefinition CreatureDefinition = new(
        "fantasy-bull",
        "Fantasy Bull",
        "b7d0d4e6c0de4bb0a000000000000008",
        "ItemTemplate_Magic_Tier1_AvalonBullCall",
        "Bull's Call",
        "Spec_FantasyBull_CI4",
        "a416cad4e97900348a00d6b27678b963",
        "8849693b2c39fef40b6c635e00e89182",
        "TryResolveWorldBullTemplate",
        "avalon-awakened/creatures/bull-fantasy/visual--8e524b00b51a76e43823aa39939bc4a3",
        1f,
        new Color32(95, 58, 37, 255),
        new Color32(223, 162, 57, 255),
        embeddedIconResourceName: "AvalonBullCompanion.Icons.bull.png");

    protected override StandaloneCreatureCompanionDefinition Definition => CreatureDefinition;
    protected override string PluginGuidValue => PluginGuid;
    protected override string PluginNameValue => PluginName;
    protected override string PluginVersionValue => PluginVersion;
}
