using AvalonCreatureCompanionShared;
using BepInEx;
using UnityEngine;

namespace AvalonVampireCompanion;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : StandaloneCreatureCompanionPlugin
{
    public const string PluginGuid = "kane.tgfoa.avalon-vampire-companion";
    public const string PluginName = "Avalon Vampire Companion";
    public const string PluginVersion = "0.1.0";

    private static readonly StandaloneCreatureCompanionDefinition CreatureDefinition = new(
        "vampire",
        "Vampire",
        "b7d0d4e6c0de4bb0a000000000000010",
        "ItemTemplate_Magic_Tier1_AvalonVampireCallSkin1",
        "Vampire's Call",
        "Spec_Vampire_CI4",
        "05fcd530c78b2854b87b37861419e630",
        "4b001c16492a0944fb171bdb200c0521",
        "TryResolveWorldVampireTemplate",
        "avalon-awakened/creatures/vampire/skin1/visual-ci4-v1",
        1f,
        new Color32(61, 19, 29, 255),
        new Color32(196, 42, 65, 255));

    protected override StandaloneCreatureCompanionDefinition Definition => CreatureDefinition;
    protected override string PluginGuidValue => PluginGuid;
    protected override string PluginNameValue => PluginName;
    protected override string PluginVersionValue => PluginVersion;
}
