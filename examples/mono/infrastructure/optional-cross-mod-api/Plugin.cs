using System;
using System.Reflection;
using BepInEx;
using UnityEngine;

namespace TGCommunity.Example.OptionalCrossModApi;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.optional-cross-mod-api";
    public const string PluginName = "TG Example - Optional Cross-Mod API";
    public const string PluginVersion = "0.1.0";

    private const string ApiTypeName = "WyrdHunt.WyrdHuntApi, WyrdHunt";

    private Type? _apiType;
    private MethodInfo? _tryReduceThreat;
    private PropertyInfo? _isAvailable;
    private int _sessionCharges = 1;

    private void Awake()
    {
        Logger.LogInfo($"{PluginName} loaded. Press F8 to attempt one optional API action. SessionCharges={_sessionCharges}.");
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F8))
        {
            return;
        }

        if (_sessionCharges <= 0)
        {
            Logger.LogInfo("Consumer action blocked: no demo session charges remain.");
            return;
        }

        if (!TryResolveApi(out string failure))
        {
            Logger.LogInfo($"Optional provider unavailable: {failure}");
            return;
        }

        try
        {
            if (_isAvailable?.GetValue(null) is bool available && !available)
            {
                Logger.LogInfo("Optional provider is loaded but not currently available.");
                return;
            }

            bool succeeded = _tryReduceThreat?.Invoke(null, new object[] { 5f, PluginGuid }) is bool value && value;
            if (!succeeded)
            {
                Logger.LogInfo("Provider call returned false. Consumer charge was not spent.");
                return;
            }

            _sessionCharges--;
            Logger.LogInfo($"Provider call succeeded. Consumer charge committed afterward. SessionCharges={_sessionCharges}.");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Optional provider call failed: {ex.GetType().Name}: {ex.Message}. Consumer charge was not spent.");
        }
    }

    private bool TryResolveApi(out string failure)
    {
        failure = string.Empty;

        if (_apiType != null)
        {
            return true;
        }

        _apiType = Type.GetType(ApiTypeName, throwOnError: false);
        if (_apiType == null)
        {
            failure = "Wyrd Hunt API type is not loaded.";
            return false;
        }

        _tryReduceThreat = _apiType.GetMethod("TryReduceThreat", BindingFlags.Public | BindingFlags.Static);
        _isAvailable = _apiType.GetProperty("IsAvailable", BindingFlags.Public | BindingFlags.Static);

        if (_tryReduceThreat == null || _isAvailable == null)
        {
            failure = "Required public API members are missing.";
            _apiType = null;
            return false;
        }

        return true;
    }
}
