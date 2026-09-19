using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace TGCommunity.Il2CppFirstGameChange;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "community.taintedgrail.il2cpp-first-game-change";
    public const string PluginName = "TG Community IL2CPP First Game Change";
    public const string PluginVersion = "0.1.0";

    private const int TeachingTargetFrameRate = 30;

    private int _previousTargetFrameRate;
    private int _previousVSyncCount;
    private bool _changedRuntimeState;

    public override void Load()
    {
        _previousTargetFrameRate = Application.targetFrameRate;
        _previousVSyncCount = QualitySettings.vSyncCount;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = TeachingTargetFrameRate;
        _changedRuntimeState = true;

        Log.LogInfo(
            $"First IL2CPP Game Change applied. " +
            $"targetFrameRate: {_previousTargetFrameRate} -> {Application.targetFrameRate}; " +
            $"vSyncCount: {_previousVSyncCount} -> {QualitySettings.vSyncCount}.");
    }

    public override bool Unload()
    {
        if (_changedRuntimeState)
        {
            Application.targetFrameRate = _previousTargetFrameRate;
            QualitySettings.vSyncCount = _previousVSyncCount;
            _changedRuntimeState = false;

            Log.LogInfo(
                $"First IL2CPP Game Change restored runtime state. " +
                $"targetFrameRate={Application.targetFrameRate}; " +
                $"vSyncCount={QualitySettings.vSyncCount}.");
        }

        return true;
    }
}
