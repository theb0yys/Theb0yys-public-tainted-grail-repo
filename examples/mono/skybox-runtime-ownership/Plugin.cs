using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace TGTemplate.SkyboxOwnership;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.template.skybox-ownership";
    public const string PluginName = "TG Skybox Ownership Template";
    public const string PluginVersion = "0.1.0";

    private ConfigEntry<bool>? _applyDemoOnLoad;
    private ConfigEntry<bool>? _restoreOnUnload;

    private Material? _previousSkybox;
    private Material? _ownedMaterial;
    private Cubemap? _ownedCubemap;
    private bool _capturedPreviousSkybox;

    private void Awake()
    {
        _applyDemoOnLoad = Config.Bind("Skybox", "ApplyDemoOnLoad", false, "Apply the procedural demo skybox.");
        _restoreOnUnload = Config.Bind("Skybox", "RestoreOnUnload", true, "Restore the previous skybox when the plug-in unloads.");

        Logger.LogInfo(PluginName + " " + PluginVersion + " loaded.");

        if (_applyDemoOnLoad.Value)
        {
            TryApplyDemoSkybox();
        }
    }

    private void TryApplyDemoSkybox()
    {
        Shader? shader = Shader.Find("Skybox/Cubemap");
        if (shader == null)
        {
            Logger.LogWarning("Skybox/Cubemap shader was not found. Current skybox was not changed.");
            return;
        }

        Cubemap? newCubemap = null;
        Material? newMaterial = null;

        try
        {
            newCubemap = CreateDemoCubemap();
            newMaterial = new Material(shader)
            {
                name = "TGTemplate_RuntimeSkybox"
            };
            newMaterial.SetTexture("_Tex", newCubemap);

            if (!_capturedPreviousSkybox)
            {
                _previousSkybox = RenderSettings.skybox;
                _capturedPreviousSkybox = true;
            }

            DestroyOwnedObjects();

            _ownedCubemap = newCubemap;
            _ownedMaterial = newMaterial;
            newCubemap = null;
            newMaterial = null;

            RenderSettings.skybox = _ownedMaterial;
            TryUpdateEnvironment();

            Logger.LogInfo("Procedural demo skybox applied.");
        }
        finally
        {
            if (newMaterial != null)
            {
                UnityEngine.Object.Destroy(newMaterial);
            }

            if (newCubemap != null)
            {
                UnityEngine.Object.Destroy(newCubemap);
            }
        }
    }

    private static Cubemap CreateDemoCubemap()
    {
        const int size = 16;
        Cubemap cubemap = new Cubemap(size, TextureFormat.RGBA32, false)
        {
            name = "TGTemplate_RuntimeCubemap"
        };

        Color[] pixels = new Color[size * size];

        foreach (CubemapFace face in new[]
                 {
                     CubemapFace.PositiveX,
                     CubemapFace.NegativeX,
                     CubemapFace.PositiveY,
                     CubemapFace.NegativeY,
                     CubemapFace.PositiveZ,
                     CubemapFace.NegativeZ
                 })
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                float t = i / (float)pixels.Length;
                pixels[i] = new Color(0.08f + t * 0.04f, 0.12f + t * 0.05f, 0.18f + t * 0.08f, 1f);
            }

            cubemap.SetPixels(pixels, face);
        }

        cubemap.Apply(false, false);
        return cubemap;
    }

    private void RestorePreviousSkybox()
    {
        if (_capturedPreviousSkybox && (_restoreOnUnload?.Value ?? true))
        {
            RenderSettings.skybox = _previousSkybox;
            TryUpdateEnvironment();
            Logger.LogInfo("Previous skybox restored.");
        }

        DestroyOwnedObjects();
        _previousSkybox = null;
        _capturedPreviousSkybox = false;
    }

    private void DestroyOwnedObjects()
    {
        if (_ownedMaterial != null)
        {
            UnityEngine.Object.Destroy(_ownedMaterial);
            _ownedMaterial = null;
        }

        if (_ownedCubemap != null)
        {
            UnityEngine.Object.Destroy(_ownedCubemap);
            _ownedCubemap = null;
        }
    }

    private static void TryUpdateEnvironment()
    {
        try
        {
            DynamicGI.UpdateEnvironment();
        }
        catch
        {
            // The skybox mutation still has a deterministic restore path.
        }
    }

    private void OnDestroy()
    {
        RestorePreviousSkybox();
    }
}
