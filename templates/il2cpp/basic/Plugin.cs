using BepInEx;
using BepInEx.Unity.IL2CPP;

namespace TGCommunity.Il2CppBasic;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "community.taintedgrail.il2cppbasic";
    public const string PluginName = "TG Community IL2CPP Basic";
    public const string PluginVersion = "0.1.0";

    public override void Load()
    {
        Log.LogInfo($"{PluginName} {PluginVersion} loaded.");
    }
}
