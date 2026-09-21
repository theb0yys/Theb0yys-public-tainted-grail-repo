using AvalonCreatureCompanionShared;
using BepInEx;
using UnityEngine;

namespace AvalonDragonCompanion;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : StandaloneCreatureCompanionPlugin
{
    public const string PluginGuid = "kane.tgfoa.avalon-dragon-companion";
    public const string PluginName = "Avalon Dragon Companion";
    public const string PluginVersion = "0.1.0";

    private static readonly StandaloneCreatureCompanionDefinition CreatureDefinition = new(
        "fantasy-dragon-grounded",
        "Fantasy Dragon",
        "b7d0d4e6c0de4bb0a000000000000009",
        "ItemTemplate_Magic_Tier1_AvalonDragonCall",
        "Dragon's Call",
        "Spec_FantasyDragon_CI4",
        "4b2df828561d1734bbcc18a737ad0475",
        "7263e375e3097a04bbc0f717e27b2364",
        "TryResolveWorldDragonTemplate",
        "avalon-awakened/creatures/fantasy-dragon/visual--4068b2aba7e996a4b9908f2d70626161",
        1f,
        new Color32(84, 25, 28, 255),
        new Color32(220, 62, 42, 255),
        embeddedIconResourceName: "AvalonDragonCompanion.Icons.dragon.png");

    protected override StandaloneCreatureCompanionDefinition Definition => CreatureDefinition;
    protected override string PluginGuidValue => PluginGuid;
    protected override string PluginNameValue => PluginName;
    protected override string PluginVersionValue => PluginVersion;
}
