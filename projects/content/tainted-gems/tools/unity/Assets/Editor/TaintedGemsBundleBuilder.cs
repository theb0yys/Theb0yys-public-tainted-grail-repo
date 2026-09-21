using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class TaintedGemsBundleBuilder
{
    private const string BundleName = "tainted_gems.bundle";
    private const string Root = "Assets/TaintedGems";
    private const string SourceRoot = Root + "/Source";
    private const string GeneratedRoot = Root + "/Generated";
    private const string ShapeSet1SourcePath = SourceRoot + "/ShapeSet1/source/Diamond_Shapes.fbx";
    private const string ShapeSet2SourcePath = SourceRoot + "/ShapeSet2/source/Diamond_Shape2.fbx";
    private const string ShapeSet1MaterialPath = GeneratedRoot + "/MI_TaintedGems_DiamondShapeSet1.mat";
    private const string ShapeSet2MaterialPath = GeneratedRoot + "/MI_TaintedGems_DiamondShapeSet2.mat";
    private const string ShapeSet1MaskMapPath = GeneratedRoot + "/T_TaintedGems_DiamondShapeSet1_HDRP_Mask.asset";
    private const string ShapeSet2MaskMapPath = GeneratedRoot + "/T_TaintedGems_DiamondShapeSet2_HDRP_Mask.asset";
    private const string ShapeSet1PrefabPath = GeneratedRoot + "/TaintedGems_DiamondShapeSet1.prefab";
    private const string ShapeSet2PrefabPath = GeneratedRoot + "/TaintedGems_DiamondShapeSet2.prefab";
    private const string IconRoot = GeneratedRoot + "/Icons";
    private const string IconAssetNamePrefix = "TaintedGemsIcon_";
    private const int IconSize = 256;
    private const int IconRenderLayer = 31;

    private static readonly string[] IconSlugs =
    {
        "AncestralSphere",
        "AzureLeechstone",
        "BlightOpal",
        "BloodstoneoftheFallen",
        "Bloodthorn",
        "BorsMightstone",
        "Bravery",
        "BurningStone",
        "CelestialCharm",
        "ColdCurrent",
        "ColdStone",
        "CrimsonCluster",
        "EndlessPursuit",
        "FeastOfStrain",
        "FeralCharm",
        "FlickeringFigurine",
        "ForeDwellerBauble",
        "GarnetShard",
        "GemofSpeed",
        "GrimalkinsEye",
        "Hastefang",
        "HauntedSoulgem",
        "IncandescentPearl",
        "KnowingWorm",
        "MarkOfThree",
        "Mercy",
        "NightshadeCharcoal",
        "RavenWingsSoulgem",
        "SerpentsEscape",
        "ShadowbladeNail",
        "Shipworm",
        "ShiverwoundCurse",
        "SmoulderingWrath",
        "StarbornEgg",
        "TheFlatteningMoon",
        "ThundersReproach",
        "TidePearl",
        "TiedTongue",
        "TimeweaverBead",
        "TornRoots",
        "TwistedEssence",
        "WardensRunestone"
    };

    public static void Build()
    {
        string manifestPath = GetArgument("-bundleManifest");
        try
        {
            string gemSourceRoot = GetArgument("-gemSourceRoot");
            string outputRoot = GetArgument("-bundleOutput");
            if (string.IsNullOrWhiteSpace(gemSourceRoot)) throw new ArgumentException("Missing -gemSourceRoot.");
            if (string.IsNullOrWhiteSpace(outputRoot)) throw new ArgumentException("Missing -bundleOutput.");
            if (string.IsNullOrWhiteSpace(manifestPath)) throw new ArgumentException("Missing -bundleManifest.");

            ResetRoot();
            EnsureFolder(SourceRoot + "/ShapeSet1");
            EnsureFolder(SourceRoot + "/ShapeSet2");
            EnsureFolder(GeneratedRoot);
            EnsureFolder(IconRoot);

            CopyGemSource(gemSourceRoot);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            ConfigureModelImport(ShapeSet1SourcePath);
            ConfigureModelImport(ShapeSet2SourcePath);
            ConfigureTextureImports();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            Material shapeSet1Material = CreateShapeSet1Material();
            Material shapeSet2Material = CreateShapeSet2Material();
            Material shapeSet1IconMaterial = CreateShapeSet1IconPreviewMaterial();
            Material shapeSet2IconMaterial = CreateShapeSet2IconPreviewMaterial();
            GameObject shapeSet1Prefab = CreateGemPrefab(ShapeSet1SourcePath, shapeSet1Material, shapeSet1IconMaterial, "TaintedGems_DiamondShapeSet1", ShapeSet1PrefabPath);
            GameObject shapeSet2Prefab = CreateGemPrefab(ShapeSet2SourcePath, shapeSet2Material, shapeSet2IconMaterial, "TaintedGems_DiamondShapeSet2", ShapeSet2PrefabPath);
            string[] iconAssetPaths = RenderIconAssets(shapeSet1Prefab, shapeSet2Prefab, shapeSet1IconMaterial, shapeSet2IconMaterial);
            string bundlePath = BuildBundle(outputRoot, new[] { ShapeSet1PrefabPath, ShapeSet2PrefabPath }.Concat(iconAssetPaths).ToArray());
            string json = BuildManifestJson(gemSourceRoot, outputRoot, bundlePath, shapeSet1Prefab, shapeSet2Prefab);
            WriteManifest(manifestPath, json);
            Debug.Log("TAINTED_GEMS_BUNDLE_PASS " + manifestPath);
        }
        catch (Exception ex)
        {
            Debug.LogError("TAINTED_GEMS_BUNDLE_BLOCKED " + ex);
            if (!string.IsNullOrWhiteSpace(manifestPath))
            {
                WriteManifest(manifestPath, "{\"marker\":\"TAINTED_GEMS_BUNDLE_BLOCKED\",\"error\":\"" + Escape(ex.ToString()) + "\"}\n");
            }

            throw;
        }
    }

    private static void CopyGemSource(string gemSourceRoot)
    {
        string set1Root = Path.Combine(gemSourceRoot, "diamond_gem_shape_set_1_collectibles");
        string set2Root = Path.Combine(gemSourceRoot, "diamond_gem_shape_set_2_collectibles");

        string[] required =
        {
            Path.Combine(set1Root, "source", "Diamond_Shapes.fbx"),
            Path.Combine(set1Root, "textures", "Diamond_Master_AlphaN.jpg"),
            Path.Combine(set1Root, "textures", "Diamond_Master_Roughness.jpeg"),
            Path.Combine(set1Root, "textures", "Diamond_Shapes_Diamond_Master_BaseColor.jpeg"),
            Path.Combine(set1Root, "textures", "Diamond_Shapes_Diamond_Master_Normal.png"),
            Path.Combine(set2Root, "source", "Diamond_Shape2.fbx"),
            Path.Combine(set2Root, "textures", "Diamond_Master2_Alpha.jpeg"),
            Path.Combine(set2Root, "textures", "Diamond_Master2_BaseColor.jpeg"),
            Path.Combine(set2Root, "textures", "Diamond_Master2_Roughness.jpeg"),
            Path.Combine(set2Root, "textures", "Diamond_Shape2_Diamond_Master2_Normal.png")
        };

        foreach (string file in required)
        {
            if (!File.Exists(file)) throw new FileNotFoundException("Missing gem source file.", file);
        }

        CopyDirectory(set1Root, Path.Combine(ProjectRoot, SourceRoot.Replace('/', Path.DirectorySeparatorChar), "ShapeSet1"));
        CopyDirectory(set2Root, Path.Combine(ProjectRoot, SourceRoot.Replace('/', Path.DirectorySeparatorChar), "ShapeSet2"));
    }

    private static void CopyDirectory(string sourceRoot, string targetRoot)
    {
        foreach (string sourceFile in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            string relative = sourceFile.Substring(sourceRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string target = Path.Combine(targetRoot, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target) ?? targetRoot);
            File.Copy(sourceFile, target, true);
        }
    }

    private static void ConfigureModelImport(string modelPath)
    {
        if (AssetImporter.GetAtPath(modelPath) is ModelImporter importer)
        {
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.SaveAndReimport();
        }
    }

    private static void ConfigureTextureImports()
    {
        ConfigureTextureImport(SourceRoot + "/ShapeSet1/textures/Diamond_Shapes_Diamond_Master_BaseColor.jpeg", TextureImporterType.Default, true, false);
        ConfigureTextureImport(SourceRoot + "/ShapeSet1/textures/Diamond_Shapes_Diamond_Master_Normal.png", TextureImporterType.NormalMap, false, false);
        ConfigureTextureImport(SourceRoot + "/ShapeSet1/textures/Diamond_Master_Roughness.jpeg", TextureImporterType.Default, false, true);
        ConfigureTextureImport(SourceRoot + "/ShapeSet1/textures/Diamond_Master_AlphaN.jpg", TextureImporterType.Default, false, false);
        ConfigureTextureImport(SourceRoot + "/ShapeSet2/textures/Diamond_Master2_BaseColor.jpeg", TextureImporterType.Default, true, false);
        ConfigureTextureImport(SourceRoot + "/ShapeSet2/textures/Diamond_Shape2_Diamond_Master2_Normal.png", TextureImporterType.NormalMap, false, false);
        ConfigureTextureImport(SourceRoot + "/ShapeSet2/textures/Diamond_Master2_Roughness.jpeg", TextureImporterType.Default, false, true);
        ConfigureTextureImport(SourceRoot + "/ShapeSet2/textures/Diamond_Master2_Alpha.jpeg", TextureImporterType.Default, false, false);
    }

    private static void ConfigureTextureImport(string path, TextureImporterType textureType, bool srgb, bool readable)
    {
        if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
        {
            return;
        }

        importer.textureType = textureType;
        importer.sRGBTexture = srgb;
        importer.isReadable = readable;
        importer.mipmapEnabled = true;
        importer.SaveAndReimport();
    }

    private static Material CreateShapeSet1Material()
    {
        Texture2D? baseColor = AssetDatabase.LoadAssetAtPath<Texture2D>(SourceRoot + "/ShapeSet1/textures/Diamond_Shapes_Diamond_Master_BaseColor.jpeg");
        Texture2D? normal = AssetDatabase.LoadAssetAtPath<Texture2D>(SourceRoot + "/ShapeSet1/textures/Diamond_Shapes_Diamond_Master_Normal.png");
        Texture2D? roughness = AssetDatabase.LoadAssetAtPath<Texture2D>(SourceRoot + "/ShapeSet1/textures/Diamond_Master_Roughness.jpeg");
        Texture2D? maskMap = CreateHdrpMaskMap(roughness, "T_TaintedGems_DiamondShapeSet1_HDRP_Mask", ShapeSet1MaskMapPath);
        return CreateGemMaterial("MI_TaintedGems_DiamondShapeSet1", ShapeSet1MaterialPath, baseColor, normal, maskMap);
    }

    private static Material CreateShapeSet2Material()
    {
        Texture2D? baseColor = AssetDatabase.LoadAssetAtPath<Texture2D>(SourceRoot + "/ShapeSet2/textures/Diamond_Master2_BaseColor.jpeg");
        Texture2D? normal = AssetDatabase.LoadAssetAtPath<Texture2D>(SourceRoot + "/ShapeSet2/textures/Diamond_Shape2_Diamond_Master2_Normal.png");
        Texture2D? roughness = AssetDatabase.LoadAssetAtPath<Texture2D>(SourceRoot + "/ShapeSet2/textures/Diamond_Master2_Roughness.jpeg");
        Texture2D? maskMap = CreateHdrpMaskMap(roughness, "T_TaintedGems_DiamondShapeSet2_HDRP_Mask", ShapeSet2MaskMapPath);
        return CreateGemMaterial("MI_TaintedGems_DiamondShapeSet2", ShapeSet2MaterialPath, baseColor, normal, maskMap);
    }

    private static Material CreateShapeSet1IconPreviewMaterial()
    {
        Texture2D? baseColor = AssetDatabase.LoadAssetAtPath<Texture2D>(SourceRoot + "/ShapeSet1/textures/Diamond_Shapes_Diamond_Master_BaseColor.jpeg");
        Texture2D? normal = AssetDatabase.LoadAssetAtPath<Texture2D>(SourceRoot + "/ShapeSet1/textures/Diamond_Shapes_Diamond_Master_Normal.png");
        return CreateIconPreviewMaterial("MI_TaintedGems_DiamondShapeSet1_IconPreview", baseColor, normal);
    }

    private static Material CreateShapeSet2IconPreviewMaterial()
    {
        Texture2D? baseColor = AssetDatabase.LoadAssetAtPath<Texture2D>(SourceRoot + "/ShapeSet2/textures/Diamond_Master2_BaseColor.jpeg");
        Texture2D? normal = AssetDatabase.LoadAssetAtPath<Texture2D>(SourceRoot + "/ShapeSet2/textures/Diamond_Shape2_Diamond_Master2_Normal.png");
        return CreateIconPreviewMaterial("MI_TaintedGems_DiamondShapeSet2_IconPreview", baseColor, normal);
    }

    private static Material CreateGemMaterial(string materialName, string materialPath, Texture2D? baseColor, Texture2D? normal, Texture2D? maskMap)
    {
        Shader shader = Shader.Find("HDRP/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Unlit/Texture");
        if (shader == null) throw new InvalidOperationException("No compatible material shader found.");

        var material = new Material(shader)
        {
            name = materialName
        };

        if (baseColor != null)
        {
            SetTexture(material, "_BaseColorMap", baseColor);
            SetTexture(material, "_MainTex", baseColor);
            SetTexture(material, "_BaseMap", baseColor);
            SetColor(material, "_BaseColor", Color.white);
            SetColor(material, "_Color", Color.white);
        }

        if (normal != null)
        {
            SetTexture(material, "_NormalMap", normal);
            SetTexture(material, "_BumpMap", normal);
            if (material.HasProperty("_NORMALMAP")) material.EnableKeyword("_NORMALMAP");
            SetFloat(material, "_NormalScale", 1f);
        }

        if (maskMap != null)
        {
            SetTexture(material, "_MaskMap", maskMap);
            SetTexture(material, "_MetallicGlossMap", maskMap);
            if (material.HasProperty("_MASKMAP")) material.EnableKeyword("_MASKMAP");
            if (material.HasProperty("_METALLICGLOSSMAP")) material.EnableKeyword("_METALLICGLOSSMAP");
        }

        SetFloat(material, "_Metallic", 0f);
        SetFloat(material, "_Smoothness", 0.8f);
        AssetDatabase.CreateAsset(material, materialPath);
        return material;
    }

    private static Material CreateIconPreviewMaterial(string materialName, Texture2D? baseColor, Texture2D? normal)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Unlit/Texture");
        if (shader == null) throw new InvalidOperationException("No compatible icon preview shader found.");

        var material = new Material(shader)
        {
            name = materialName,
            hideFlags = HideFlags.HideAndDontSave
        };

        if (baseColor != null)
        {
            SetTexture(material, "_BaseColorMap", baseColor);
            SetTexture(material, "_MainTex", baseColor);
            SetTexture(material, "_BaseMap", baseColor);
            SetColor(material, "_BaseColor", Color.white);
            SetColor(material, "_Color", Color.white);
        }

        if (normal != null)
        {
            SetTexture(material, "_NormalMap", normal);
            SetTexture(material, "_BumpMap", normal);
            if (material.HasProperty("_NORMALMAP")) material.EnableKeyword("_NORMALMAP");
            SetFloat(material, "_NormalScale", 1f);
        }

        SetFloat(material, "_Metallic", 0f);
        SetFloat(material, "_Smoothness", 0.85f);
        return material;
    }

    private static Texture2D? CreateHdrpMaskMap(Texture2D? roughness, string assetName, string assetPath)
    {
        if (roughness == null)
        {
            return null;
        }

        int width = roughness.width;
        int height = roughness.height;
        var mask = new Texture2D(width, height, TextureFormat.RGBA32, true, true)
        {
            name = assetName
        };

        Color32[] roughnessPixels = TryReadPixels(roughness, width, height);
        Color32[] pixels = new Color32[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            byte rough = roughnessPixels.Length == pixels.Length ? roughnessPixels[i].r : byte.MinValue;
            byte smoothness = (byte)(byte.MaxValue - rough);
            pixels[i] = new Color32(byte.MinValue, byte.MaxValue, byte.MinValue, smoothness);
        }

        mask.SetPixels32(pixels);
        mask.Apply(updateMipmaps: true, makeNoLongerReadable: false);
        AssetDatabase.CreateAsset(mask, assetPath);
        return mask;
    }

    private static Color32[] TryReadPixels(Texture2D? texture, int width, int height)
    {
        if (texture == null || texture.width != width || texture.height != height)
        {
            return Array.Empty<Color32>();
        }

        try
        {
            return texture.GetPixels32();
        }
        catch
        {
            return Array.Empty<Color32>();
        }
    }

    private static GameObject CreateGemPrefab(string modelPath, Material material, Material iconPreviewMaterial, string prefabName, string prefabPath)
    {
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (source == null) throw new FileNotFoundException("Imported gem FBX not found.", modelPath);

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        instance.name = prefabName;
        int keptRenderers = CullNonGemRenderersForWorldPrefab(instance, iconPreviewMaterial);
        foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
        {
            renderer.sharedMaterial = material;
            renderer.enabled = true;
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        Debug.Log($"TAINTED_GEMS_WORLD_PREFAB_FILTERED prefab={prefabName}; keptRenderers={keptRenderers}");
        UnityEngine.Object.DestroyImmediate(instance);
        return prefab;
    }

    private static int CullNonGemRenderersForWorldPrefab(GameObject instance, Material iconPreviewMaterial)
    {
        Renderer[] renderers = instance
            .GetComponentsInChildren<Renderer>(true)
            .Where(HasRenderableBounds)
            .ToArray();
        if (renderers.Length == 0)
        {
            throw new InvalidOperationException("Imported gem prefab has no renderers for world filtering: " + instance.name);
        }

        RenderTexture? previousActive = RenderTexture.active;
        var renderTexture = new RenderTexture(IconSize, IconSize, 24, RenderTextureFormat.ARGB32)
        {
            antiAliasing = 4
        };
        var capture = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        GameObject cameraObject = new GameObject("TaintedGems_WorldPrefabFilterCamera")
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.enabled = false;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.clear;
        camera.orthographic = true;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 100f;
        camera.cullingMask = 1 << IconRenderLayer;
        camera.targetTexture = renderTexture;

        GameObject keyLightObject = CreateIconLight("TaintedGems_WorldPrefabFilterKeyLight", new Vector3(36f, -28f, 0f), 1.35f);
        GameObject fillLightObject = CreateIconLight("TaintedGems_WorldPrefabFilterFillLight", new Vector3(310f, 42f, 0f), 0.55f);
        var keep = new HashSet<Renderer>();

        try
        {
            SetLayerRecursive(instance, IconRenderLayer);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer candidate = renderers[i];
                foreach (Renderer renderer in renderers)
                {
                    renderer.sharedMaterial = iconPreviewMaterial;
                    renderer.enabled = false;
                }

                candidate.enabled = true;
                IconCaptureMetrics metrics = CaptureIcon(camera, renderTexture, capture, candidate.bounds, i);
                if (metrics.VisiblePixels >= 1024 && metrics.SignificantComponents == 1)
                {
                    keep.Add(candidate);
                }
            }
        }
        finally
        {
            RenderTexture.active = previousActive;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(keyLightObject);
            UnityEngine.Object.DestroyImmediate(fillLightObject);
            UnityEngine.Object.DestroyImmediate(capture);
            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(renderTexture);
            SetLayerRecursive(instance, 0);
        }

        if (keep.Count == 0)
        {
            throw new InvalidOperationException("World prefab filtering removed every renderer: " + instance.name);
        }

        foreach (Renderer renderer in renderers)
        {
            if (!keep.Contains(renderer))
            {
                UnityEngine.Object.DestroyImmediate(renderer);
            }
        }

        return keep.Count;
    }

    private static string[] RenderIconAssets(
        GameObject shapeSet1Prefab,
        GameObject shapeSet2Prefab,
        Material shapeSet1IconMaterial,
        Material shapeSet2IconMaterial)
    {
        var paths = new List<string>(IconSlugs.Length);
        RenderTexture? previousActive = RenderTexture.active;
        var renderTexture = new RenderTexture(IconSize, IconSize, 24, RenderTextureFormat.ARGB32)
        {
            antiAliasing = 4
        };
        var capture = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };
        GameObject cameraObject = new GameObject("TaintedGems_IconCamera")
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.enabled = false;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.clear;
        camera.orthographic = true;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 100f;
        camera.cullingMask = 1 << IconRenderLayer;
        camera.targetTexture = renderTexture;

        GameObject keyLightObject = CreateIconLight("TaintedGems_IconKeyLight", new Vector3(36f, -28f, 0f), 1.35f);
        GameObject fillLightObject = CreateIconLight("TaintedGems_IconFillLight", new Vector3(310f, 42f, 0f), 0.55f);

        try
        {
            for (int i = 0; i < IconSlugs.Length; i++)
            {
                string slug = IconSlugs[i];
                string assetName = IconAssetNamePrefix + slug;
                string assetPath = IconRoot + "/" + assetName + ".png";
                GameObject sourcePrefab = (i % 2 == 0) ? shapeSet1Prefab : shapeSet2Prefab;
                Material iconMaterial = (i % 2 == 0) ? shapeSet1IconMaterial : shapeSet2IconMaterial;
                RenderIconAsset(sourcePrefab, iconMaterial, i, assetPath, camera, renderTexture, capture);
                ConfigureSpriteImport(assetPath);
                paths.Add(assetPath);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            return paths.ToArray();
        }
        finally
        {
            RenderTexture.active = previousActive;
            camera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(keyLightObject);
            UnityEngine.Object.DestroyImmediate(fillLightObject);
            UnityEngine.Object.DestroyImmediate(capture);
            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }
    }

    private static GameObject CreateIconLight(string name, Vector3 rotation, float intensity)
    {
        GameObject lightObject = new GameObject(name)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = intensity;
        light.shadows = LightShadows.None;
        lightObject.transform.rotation = Quaternion.Euler(rotation);
        return lightObject;
    }

    private static void RenderIconAsset(
        GameObject sourcePrefab,
        Material iconMaterial,
        int index,
        string assetPath,
        Camera camera,
        RenderTexture renderTexture,
        Texture2D capture)
    {
        GameObject? instance = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
        if (instance == null)
        {
            instance = UnityEngine.Object.Instantiate(sourcePrefab);
        }

        instance.hideFlags = HideFlags.HideAndDontSave;
        SetLayerRecursive(instance, IconRenderLayer);
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.Euler(18f + (index % 4) * 4f, (index * 37f) % 360f, 0f);
        instance.transform.localScale = Vector3.one;

        try
        {
            Renderer[] allRenderers = instance
                .GetComponentsInChildren<Renderer>(true)
                .Where(renderer => renderer != null)
                .ToArray();
            Renderer[] renderers = allRenderers
                .Where(HasRenderableBounds)
                .ToArray();
            if (renderers.Length == 0)
            {
                throw new InvalidOperationException("Imported gem prefab has no renderers: " + sourcePrefab.name);
            }

            foreach (Renderer renderer in allRenderers)
            {
                renderer.sharedMaterial = iconMaterial;
                renderer.enabled = false;
            }

            int startIndex = (index / 2) % renderers.Length;
            IconCaptureMetrics bestMetrics = default;
            Renderer? bestRenderer = null;
            for (int offset = 0; offset < renderers.Length; offset++)
            {
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = false;
                }

                Renderer candidate = renderers[(startIndex + offset) % renderers.Length];
                candidate.enabled = true;
                IconCaptureMetrics metrics = CaptureIcon(camera, renderTexture, capture, candidate.bounds, index);
                if (metrics.IsBetterThan(bestMetrics))
                {
                    bestMetrics = metrics;
                    bestRenderer = candidate;
                }

                if (metrics.VisiblePixels >= 1024 && metrics.SignificantComponents == 1)
                {
                    string fullPath = Path.Combine(ProjectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? ProjectRoot);
                    File.WriteAllBytes(fullPath, capture.EncodeToPNG());
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                    return;
                }
            }

            if (bestRenderer != null)
            {
                throw new InvalidOperationException(
                    "No single-gem renderer capture passed for " + assetPath +
                    "; bestRenderer=" + bestRenderer.name +
                    "; visiblePixels=" + bestMetrics.VisiblePixels +
                    "; significantComponents=" + bestMetrics.SignificantComponents +
                    "; largestComponentPixels=" + bestMetrics.LargestComponentPixels);
            }

            throw new InvalidOperationException("No icon renderer candidates were captured for " + assetPath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(instance);
        }
    }

    private static bool HasRenderableBounds(Renderer renderer)
    {
        Bounds bounds = renderer.bounds;
        return bounds.size.sqrMagnitude > 0.000001f;
    }

    private static IconCaptureMetrics CaptureIcon(Camera camera, RenderTexture renderTexture, Texture2D capture, Bounds bounds, int index)
    {
        float largestExtent = Mathf.Max(bounds.extents.x, Mathf.Max(bounds.extents.y, bounds.extents.z));
        if (largestExtent <= 0.001f)
        {
            largestExtent = 0.5f;
            bounds = new Bounds(Vector3.zero, Vector3.one);
        }

        Vector3 viewDirection = (Quaternion.Euler(18f, -28f + (index % 5) * 8f, 0f) * Vector3.forward).normalized;
        camera.orthographicSize = Mathf.Max(0.35f, largestExtent * 1.45f);
        camera.transform.position = bounds.center - viewDirection * Mathf.Max(2f, largestExtent * 6f);
        camera.transform.rotation = Quaternion.LookRotation(viewDirection, Vector3.up);

        RenderTexture.active = renderTexture;
        GL.Clear(true, true, Color.clear);
        camera.Render();
        capture.ReadPixels(new Rect(0f, 0f, IconSize, IconSize), 0, 0);
        capture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
        return AnalyzeIconCapture(capture);
    }

    private static IconCaptureMetrics AnalyzeIconCapture(Texture2D texture)
    {
        Color32[] pixels = texture.GetPixels32();
        bool[] visited = new bool[pixels.Length];
        int visiblePixels = 0;
        int largestComponentPixels = 0;
        int significantComponents = 0;
        for (int i = 0; i < pixels.Length; i++)
        {
            if (visited[i] || pixels[i].a <= 8)
            {
                continue;
            }

            int componentPixels = FloodFillAlphaComponent(pixels, visited, i, IconSize, IconSize);
            visiblePixels += componentPixels;
            largestComponentPixels = Mathf.Max(largestComponentPixels, componentPixels);
            if (componentPixels >= 96)
            {
                significantComponents++;
            }
        }

        return new IconCaptureMetrics(visiblePixels, significantComponents, largestComponentPixels);
    }

    private static int FloodFillAlphaComponent(Color32[] pixels, bool[] visited, int start, int width, int height)
    {
        var stack = new Stack<int>();
        stack.Push(start);
        visited[start] = true;
        int count = 0;

        while (stack.Count > 0)
        {
            int current = stack.Pop();
            count++;
            int x = current % width;
            int y = current / width;

            TryPushAlphaNeighbor(pixels, visited, stack, x - 1, y, width, height);
            TryPushAlphaNeighbor(pixels, visited, stack, x + 1, y, width, height);
            TryPushAlphaNeighbor(pixels, visited, stack, x, y - 1, width, height);
            TryPushAlphaNeighbor(pixels, visited, stack, x, y + 1, width, height);
            TryPushAlphaNeighbor(pixels, visited, stack, x - 1, y - 1, width, height);
            TryPushAlphaNeighbor(pixels, visited, stack, x + 1, y - 1, width, height);
            TryPushAlphaNeighbor(pixels, visited, stack, x - 1, y + 1, width, height);
            TryPushAlphaNeighbor(pixels, visited, stack, x + 1, y + 1, width, height);
        }

        return count;
    }

    private static void TryPushAlphaNeighbor(Color32[] pixels, bool[] visited, Stack<int> stack, int x, int y, int width, int height)
    {
        if (x < 0 || y < 0 || x >= width || y >= height)
        {
            return;
        }

        int index = y * width + x;
        if (visited[index] || pixels[index].a <= 8)
        {
            return;
        }

        visited[index] = true;
        stack.Push(index);
    }

    private readonly struct IconCaptureMetrics
    {
        internal IconCaptureMetrics(int visiblePixels, int significantComponents, int largestComponentPixels)
        {
            VisiblePixels = visiblePixels;
            SignificantComponents = significantComponents;
            LargestComponentPixels = largestComponentPixels;
        }

        internal int VisiblePixels { get; }

        internal int SignificantComponents { get; }

        internal int LargestComponentPixels { get; }

        internal bool IsBetterThan(IconCaptureMetrics other)
        {
            if (SignificantComponents == 1 && other.SignificantComponents != 1)
            {
                return true;
            }

            if (SignificantComponents != 1 && other.SignificantComponents == 1)
            {
                return false;
            }

            if (LargestComponentPixels != other.LargestComponentPixels)
            {
                return LargestComponentPixels > other.LargestComponentPixels;
            }

            return VisiblePixels > other.VisiblePixels;
        }
    }

    private static void ConfigureSpriteImport(string assetPath)
    {
        if (AssetImporter.GetAtPath(assetPath) is not TextureImporter importer)
        {
            throw new InvalidOperationException("Rendered gem icon importer was not a TextureImporter: " + assetPath);
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.sRGBTexture = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        if (AssetDatabase.LoadAssetAtPath<Sprite>(assetPath) == null)
        {
            throw new InvalidOperationException("Rendered gem icon sprite did not import: " + assetPath);
        }
    }

    private static void SetLayerRecursive(GameObject target, int layer)
    {
        target.layer = layer;
        foreach (Transform child in target.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    private static string BuildBundle(string outputRoot, string[] assetNames)
    {
        Directory.CreateDirectory(outputRoot);
        var build = new AssetBundleBuild
        {
            assetBundleName = BundleName,
            assetNames = assetNames
        };

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
            outputRoot,
            new[] { build },
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode,
            BuildTarget.StandaloneWindows64);
        if (manifest == null) throw new InvalidOperationException("BuildPipeline.BuildAssetBundles returned null.");

        string bundlePath = Path.Combine(outputRoot, BundleName);
        if (!File.Exists(bundlePath)) throw new FileNotFoundException("Expected bundle was not produced.", bundlePath);
        return bundlePath;
    }

    private static string BuildManifestJson(string gemSourceRoot, string outputRoot, string bundlePath, GameObject shapeSet1Prefab, GameObject shapeSet2Prefab)
    {
        var info = new FileInfo(bundlePath);
        var sb = new StringBuilder();
        sb.Append("{\n");
        Append(sb, "marker", "TAINTED_GEMS_BUNDLE_PASS", 1, true);
        Append(sb, "unityVersion", Application.unityVersion, 1, true);
        Append(sb, "gemSourceRoot", gemSourceRoot, 1, true);
        Append(sb, "bundleName", BundleName, 1, true);
        Append(sb, "bundlePath", bundlePath.Replace('\\', '/'), 1, true);
        Append(sb, "bundleSha256", Sha256(bundlePath), 1, true);
        Append(sb, "bundleBytes", info.Length.ToString(), 1, false, true);
        sb.Append(",\n");
        Append(sb, "shapeSet1AssetPath", ShapeSet1PrefabPath, 1, true);
        Append(sb, "shapeSet1TransformCount", shapeSet1Prefab.GetComponentsInChildren<Transform>(true).Length.ToString(), 1, false, true);
        sb.Append(",\n");
        Append(sb, "shapeSet2AssetPath", ShapeSet2PrefabPath, 1, true);
        Append(sb, "shapeSet2TransformCount", shapeSet2Prefab.GetComponentsInChildren<Transform>(true).Length.ToString(), 1, false, true);
        sb.Append(",\n");
        Append(sb, "uiIconCount", IconSlugs.Length.ToString(), 1, true, true);
        Append(sb, "uiIconAssetPrefix", IconAssetNamePrefix, 1, true);
        Append(sb, "uiIconSource", "rendered-imported-gem-prefabs", 1, true);
        Append(sb, "sourceFileCount", Directory.GetFiles(gemSourceRoot, "*", SearchOption.AllDirectories).Length.ToString(), 1, false, true);
        sb.Append(",\n");
        Append(sb, "sourceSha256", Sha256Joined(Directory.GetFiles(gemSourceRoot, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.OrdinalIgnoreCase)), 1, false);
        sb.Append("}\n");
        return sb.ToString();
    }

    private static void ResetRoot()
    {
        if (AssetDatabase.IsValidFolder(Root))
        {
            AssetDatabase.DeleteAsset(Root);
        }

        EnsureFolder(Root);
    }

    private static void WriteManifest(string manifestPath, string json)
    {
        string? manifestDirectory = Path.GetDirectoryName(manifestPath);
        if (!string.IsNullOrWhiteSpace(manifestDirectory))
        {
            Directory.CreateDirectory(manifestDirectory);
        }

        File.WriteAllText(manifestPath, json, new UTF8Encoding(false));
    }

    private static void EnsureFolder(string assetFolder)
    {
        string normalized = assetFolder.Replace('\\', '/').Trim('/');
        string[] parts = normalized.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }

    private static void SetTexture(Material material, string property, Texture texture)
    {
        if (material.HasProperty(property))
        {
            material.SetTexture(property, texture);
        }
    }

    private static void SetFloat(Material material, string property, float value)
    {
        if (material.HasProperty(property))
        {
            material.SetFloat(property, value);
        }
    }

    private static void SetColor(Material material, string property, Color value)
    {
        if (material.HasProperty(property))
        {
            material.SetColor(property, value);
        }
    }

    private static string GetArgument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(args[i + 1]);
            }
        }

        return string.Empty;
    }

    private static string Sha256(string path)
    {
        using SHA256 sha = SHA256.Create();
        using FileStream stream = File.OpenRead(path);
        return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
    }

    private static string Sha256Joined(IEnumerable<string> paths)
    {
        using SHA256 sha = SHA256.Create();
        foreach (string path in paths)
        {
            byte[] pathBytes = Encoding.UTF8.GetBytes(path.Replace('\\', '/'));
            sha.TransformBlock(pathBytes, 0, pathBytes.Length, null, 0);
            using FileStream stream = File.OpenRead(path);
            byte[] buffer = new byte[81920];
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                sha.TransformBlock(buffer, 0, read, null, 0);
            }
        }

        sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return BitConverter.ToString(sha.Hash ?? Array.Empty<byte>()).Replace("-", string.Empty);
    }

    private static void Append(StringBuilder sb, string name, string value, int indent, bool comma, bool raw = false)
    {
        sb.Append(new string(' ', indent * 2)).Append("\"").Append(Escape(name)).Append("\":");
        if (raw)
        {
            sb.Append(value);
        }
        else
        {
            sb.Append("\"").Append(Escape(value)).Append("\"");
        }

        if (comma) sb.Append(",");
        sb.Append("\n");
    }

    private static string Escape(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
    }

    private static string ProjectRoot => Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
}
