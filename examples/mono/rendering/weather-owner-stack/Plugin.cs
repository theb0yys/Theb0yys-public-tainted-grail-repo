using System;
using System.Reflection;
using BepInEx;

namespace TGCommunity.Example.WeatherOwnerStack;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.weather-owner-stack";
    public const string PluginName = "TG Example - Weather Owner Stack";
    public const string PluginVersion = "0.1.0";

    private const string ApiTypeName = "TaintedWeather.TaintedWeatherApi, TaintedWeather";

    private float _nextPoll;
    private string _lastSnapshot = string.Empty;

    private void Awake()
    {
        Logger.LogInfo($"{PluginName} loaded. This example is a read-only consumer of Tainted Weather's public ownership snapshot.");
    }

    private void Update()
    {
        if (UnityEngine.Time.unscaledTime < _nextPoll)
        {
            return;
        }

        _nextPoll = UnityEngine.Time.unscaledTime + 2f;
        Observe();
    }

    private void Observe()
    {
        Type? api = Type.GetType(ApiTypeName, throwOnError: false);
        if (api == null)
        {
            LogOnce("weather-api-unavailable");
            return;
        }

        object? availableValue = api.GetProperty("Available", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
        if (availableValue is not bool available || !available)
        {
            LogOnce("weather-api-not-ready");
            return;
        }

        object? snapshot = api.GetProperty("Current", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
        if (snapshot == null)
        {
            LogOnce("weather-snapshot-null");
            return;
        }

        string weatherState = Text(Read(snapshot, "State"));
        object? sky = Read(snapshot, "SkyEnvironmentPlan");
        object? water = Read(snapshot, "WaterEnvironmentPlan");

        string skyEnvironment = Text(Read(sky, "EnvironmentLabel"));
        string timeBucket = Text(Read(sky, "TimeBucket"));
        string waterEnvironment = Text(Read(water, "WaterLabel"));
        string waterRequested = Text(Read(water, "ExternalWaterConsumerRequested"));

        string fingerprint = string.Join("|", weatherState, skyEnvironment, timeBucket, waterEnvironment, waterRequested);
        if (fingerprint == _lastSnapshot)
        {
            return;
        }

        _lastSnapshot = fingerprint;

        Logger.LogInfo($"owner=weather-truth state={weatherState}; mutationByThisExample=false");
        Logger.LogInfo($"consumer=sky requestedEnvironment={skyEnvironment}; timeBucket={timeBucket}; skyMutationByThisExample=false");
        Logger.LogInfo($"consumer=water requestedEnvironment={waterEnvironment}; externalConsumerRequested={waterRequested}; waterMutationByThisExample=false");
    }

    private void LogOnce(string state)
    {
        if (_lastSnapshot == state)
        {
            return;
        }

        _lastSnapshot = state;
        Logger.LogInfo($"Weather owner-stack observation: {state}.");
    }

    private static object? Read(object? owner, string member)
    {
        if (owner == null)
        {
            return null;
        }

        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
        Type type = owner.GetType();

        PropertyInfo? property = type.GetProperty(member, flags);
        if (property != null && property.GetIndexParameters().Length == 0)
        {
            try
            {
                return property.GetValue(owner);
            }
            catch
            {
                return null;
            }
        }

        FieldInfo? field = type.GetField(member, flags);
        if (field != null)
        {
            try
            {
                return field.GetValue(owner);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private static string Text(object? value)
    {
        return value?.ToString() ?? "<none>";
    }
}
