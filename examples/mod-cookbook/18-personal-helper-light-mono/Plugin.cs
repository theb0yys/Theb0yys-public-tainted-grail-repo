using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace TGExample.PersonalHelperLight;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.personal-helper-light";
    public const string PluginName = "TG Example - Personal Helper Light";
    public const string PluginVersion = "0.1.0";

    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<KeyCode> _toggleKey = null!;
    private ConfigEntry<float> _range = null!;
    private ConfigEntry<float> _lumens = null!;
    private ConfigEntry<float> _volumetricDimmer = null!;

    private GameObject? _root;
    private Light? _light;
    private HDAdditionalLightData? _lightData;
    private bool _runtimeEnabled;

    private void Awake()
    {
        _enabled = Config.Bind("General", "Enabled", true, "Enable the helper-light example.");
        _toggleKey = Config.Bind("General", "ToggleKey", KeyCode.F7, "Toggle the helper light.");
        _range = Config.Bind("Light", "Range", 12f,
            new ConfigDescription("Point-light range.", new AcceptableValueRange<float>(2f, 40f)));
        _lumens = Config.Bind("Light", "Lumens", 900f,
            new ConfigDescription("HDRP light intensity in lumens.", new AcceptableValueRange<float>(50f, 5000f)));
        _volumetricDimmer = Config.Bind("Light", "VolumetricDimmer", 0.35f,
            new ConfigDescription("Volumetric contribution.", new AcceptableValueRange<float>(0f, 1f)));

        _runtimeEnabled = _enabled.Value;
        EnsureLight();
        ApplySettings();

        Logger.LogInfo($"{PluginName} loaded. ToggleKey={_toggleKey.Value}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(_toggleKey.Value))
        {
            _runtimeEnabled = !_runtimeEnabled;
            ApplySettings();
            Logger.LogInfo($"Personal helper light enabled={_runtimeEnabled}");
        }
    }

    private void LateUpdate()
    {
        if (!_runtimeEnabled || _root == null)
        {
            return;
        }

        Camera? camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        Transform t = camera.transform;
        _root.transform.SetPositionAndRotation(
            t.position + t.forward * 0.55f + t.up * -0.12f,
            t.rotation);
    }

    private void EnsureLight()
    {
        if (_root != null)
        {
            return;
        }

        _root = new GameObject("TGExample_PersonalHelperLight");
        DontDestroyOnLoad(_root);

        _light = _root.AddComponent<Light>();
        _light.type = LightType.Point;
        _light.lightUnit = LightUnit.Lumen;
        _light.color = new Color(1f, 0.72f, 0.48f, 1f);
        _light.shadows = LightShadows.None;

        _lightData = _root.AddComponent<HDAdditionalLightData>();
        _lightData.affectsVolumetric = true;
        _lightData.applyRangeAttenuation = true;
        _lightData.shadowDimmer = 0f;
        _lightData.volumetricShadowDimmer = 0f;
    }

    private void ApplySettings()
    {
        EnsureLight();

        if (_light == null || _lightData == null)
        {
            return;
        }

        float range = Mathf.Clamp(_range.Value, 2f, 40f);
        float lumens = Mathf.Clamp(_lumens.Value, 50f, 5000f);

        _light.lightUnit = LightUnit.Lumen;
        _light.range = range;
        _light.intensity = lumens;
        _light.enabled = _enabled.Value && _runtimeEnabled;

        _lightData.range = range;
        _lightData.volumetricDimmer = Mathf.Clamp01(_volumetricDimmer.Value);
    }

    private void OnDestroy()
    {
        if (_root != null)
        {
            Destroy(_root);
            _root = null;
            _light = null;
            _lightData = null;
        }
    }
}
