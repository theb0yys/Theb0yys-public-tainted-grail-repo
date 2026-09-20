using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using UnityEngine;

namespace TGCommunity.Example.HdrpFogControl;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.hdrp-fog-control";
    public const string PluginName = "TG Example - HDRP Fog Control";
    public const string PluginVersion = "0.1.0";

    private const string FogTypeName = "UnityEngine.Rendering.HighDefinition.Fog";
    private const string LocalFogTypeName = "UnityEngine.Rendering.HighDefinition.LocalVolumetricFog";

    private readonly List<ProfileState> _profiles = new List<ProfileState>();
    private readonly List<BehaviourState> _localFog = new List<BehaviourState>();
    private bool _removed;

    private void Awake()
    {
        Logger.LogInfo($"{PluginName} loaded. Press F8 to remove/restore the currently discovered HDRP fog owners.");
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F8))
        {
            return;
        }

        if (_removed)
        {
            Restore();
        }
        else
        {
            DiscoverAndRemove();
        }
    }

    private void OnDestroy()
    {
        Restore();
    }

    private void DiscoverAndRemove()
    {
        Restore();
        int profileCount = 0;
        int localCount = 0;

        foreach (ScriptableObject candidate in Resources.FindObjectsOfTypeAll<ScriptableObject>())
        {
            if (candidate == null || !string.Equals(candidate.GetType().FullName, FogTypeName, StringComparison.Ordinal))
            {
                continue;
            }

            var state = new ProfileState(candidate);
            state.CaptureBoolMember("active");
            state.CaptureParameter("enabled");
            state.CaptureParameter("enableVolumetricFog");
            _profiles.Add(state);

            TryWriteBoolMember(candidate, "active", false);
            TryWriteVolumeBool(candidate, "enabled", false);
            TryWriteVolumeBool(candidate, "enableVolumetricFog", false);
            profileCount++;
        }

        foreach (Component candidate in Resources.FindObjectsOfTypeAll<Component>())
        {
            if (candidate == null || !string.Equals(candidate.GetType().FullName, LocalFogTypeName, StringComparison.Ordinal))
            {
                continue;
            }

            if (candidate is Behaviour behaviour)
            {
                _localFog.Add(new BehaviourState(behaviour, behaviour.enabled));
                behaviour.enabled = false;
                localCount++;
            }
        }

        _removed = true;
        Logger.LogInfo($"HDRP fog removal applied. FogProfileObjects={profileCount}; LocalVolumetricFogBehaviours={localCount}.");
    }

    private void Restore()
    {
        foreach (ProfileState state in _profiles)
        {
            state.Restore();
        }

        foreach (BehaviourState state in _localFog)
        {
            if (state.Behaviour != null)
            {
                state.Behaviour.enabled = state.Enabled;
            }
        }

        if (_profiles.Count > 0 || _localFog.Count > 0)
        {
            Logger.LogInfo($"HDRP fog state restored. FogProfileObjects={_profiles.Count}; LocalVolumetricFogBehaviours={_localFog.Count}.");
        }

        _profiles.Clear();
        _localFog.Clear();
        _removed = false;
    }

    private static object? ReadMember(object target, string name)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        Type type = target.GetType();

        PropertyInfo? property = type.GetProperty(name, flags);
        if (property != null && property.GetIndexParameters().Length == 0)
        {
            try { return property.GetValue(target); } catch { }
        }

        FieldInfo? field = type.GetField(name, flags);
        if (field != null)
        {
            try { return field.GetValue(target); } catch { }
        }

        return null;
    }

    private static bool TryWriteMember(object target, string name, object value)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        Type type = target.GetType();

        PropertyInfo? property = type.GetProperty(name, flags);
        if (property != null && property.CanWrite && property.GetIndexParameters().Length == 0)
        {
            try
            {
                property.SetValue(target, value);
                return true;
            }
            catch { }
        }

        FieldInfo? field = type.GetField(name, flags);
        if (field != null && !field.IsInitOnly)
        {
            try
            {
                field.SetValue(target, value);
                return true;
            }
            catch { }
        }

        return false;
    }

    private static bool TryWriteBoolMember(object target, string name, bool value)
    {
        object? current = ReadMember(target, name);
        return current is bool && TryWriteMember(target, name, value);
    }

    private static bool TryWriteVolumeBool(object component, string parameterName, bool value)
    {
        object? parameter = ReadMember(component, parameterName);
        if (parameter == null)
        {
            return false;
        }

        bool changed = TryWriteMember(parameter, "value", value);
        TryWriteMember(parameter, "overrideState", true);
        return changed;
    }

    private sealed class ProfileState
    {
        private readonly object _component;
        private readonly Dictionary<string, object?> _members = new Dictionary<string, object?>(StringComparer.Ordinal);
        private readonly Dictionary<string, ParameterState> _parameters = new Dictionary<string, ParameterState>(StringComparer.Ordinal);

        internal ProfileState(object component)
        {
            _component = component;
        }

        internal void CaptureBoolMember(string name)
        {
            object? value = ReadMember(_component, name);
            if (value is bool)
            {
                _members[name] = value;
            }
        }

        internal void CaptureParameter(string name)
        {
            object? parameter = ReadMember(_component, name);
            if (parameter == null)
            {
                return;
            }

            _parameters[name] = new ParameterState(
                parameter,
                ReadMember(parameter, "value"),
                ReadMember(parameter, "overrideState"));
        }

        internal void Restore()
        {
            foreach (KeyValuePair<string, object?> member in _members)
            {
                if (member.Value != null)
                {
                    TryWriteMember(_component, member.Key, member.Value);
                }
            }

            foreach (ParameterState parameter in _parameters.Values)
            {
                if (parameter.Value != null)
                {
                    TryWriteMember(parameter.Parameter, "value", parameter.Value);
                }

                if (parameter.OverrideState != null)
                {
                    TryWriteMember(parameter.Parameter, "overrideState", parameter.OverrideState);
                }
            }
        }
    }

    private readonly struct ParameterState
    {
        internal ParameterState(object parameter, object? value, object? overrideState)
        {
            Parameter = parameter;
            Value = value;
            OverrideState = overrideState;
        }

        internal object Parameter { get; }
        internal object? Value { get; }
        internal object? OverrideState { get; }
    }

    private readonly struct BehaviourState
    {
        internal BehaviourState(Behaviour behaviour, bool enabled)
        {
            Behaviour = behaviour;
            Enabled = enabled;
        }

        internal Behaviour Behaviour { get; }
        internal bool Enabled { get; }
    }
}
