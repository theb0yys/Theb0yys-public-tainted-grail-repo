using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class DragonKnightVisualBundleBuilder
{
    public const string BundleName = "dragonknight_visuals";
    public const string IronPhasePrefab = "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Iron_Weapon.prefab";
    public const string FirePhasePrefab = "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Fire_Weapon.prefab";

    public static void BuildFromImportedProject()
    {
        int exitCode = 1;
        try
        {
            string outputRoot = GetArgument("-dragonKnightBundleOutput");
            string manifestPath = GetArgument("-dragonKnightBundleManifest");
            if (string.IsNullOrWhiteSpace(outputRoot)) throw new ArgumentException("Missing -dragonKnightBundleOutput.");
            if (string.IsNullOrWhiteSpace(manifestPath)) throw new ArgumentException("Missing -dragonKnightBundleManifest.");

            RequirePrefab(IronPhasePrefab);
            RequirePrefab(FirePhasePrefab);

            Directory.CreateDirectory(outputRoot);
            var build = new AssetBundleBuild
            {
                assetBundleName = BundleName,
                assetNames = new[] { IronPhasePrefab, FirePhasePrefab }
            };

            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
                outputRoot,
                new[] { build },
                BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode,
                BuildTarget.StandaloneWindows64);
            if (manifest == null) throw new InvalidOperationException("BuildPipeline.BuildAssetBundles returned null.");

            string bundlePath = Path.Combine(outputRoot, BundleName);
            if (!File.Exists(bundlePath)) throw new FileNotFoundException("Expected Dragon Knight bundle was not produced.", bundlePath);

            File.WriteAllText(manifestPath, BuildManifestJson(bundlePath), new UTF8Encoding(false));
            Debug.Log("[Dragon Knight] Visual bundle built: " + bundlePath);
            exitCode = 0;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        finally
        {
            EditorApplication.Exit(exitCode);
        }
    }

    private static void RequirePrefab(string path)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) throw new FileNotFoundException("Required Dragon Knight prefab was not imported.", path);
        if (prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length == 0)
        {
            throw new InvalidOperationException("Required Dragon Knight prefab has no SkinnedMeshRenderer: " + path);
        }
    }

    private static string BuildManifestJson(string bundlePath)
    {
        var info = new FileInfo(bundlePath);
        return "{"
               + "\"unityVersion\":\"" + Escape(Application.unityVersion) + "\","
               + "\"bundleName\":\"" + BundleName + "\","
               + "\"bundlePath\":\"" + Escape(bundlePath.Replace('\\', '/')) + "\","
               + "\"bytes\":" + info.Length + ","
               + "\"sha256\":\"" + Sha256(bundlePath) + "\","
               + "\"assetNames\":[\"" + Escape(IronPhasePrefab) + "\",\"" + Escape(FirePhasePrefab) + "\"]"
               + "}\n";
    }

    private static string Sha256(string path)
    {
        using (SHA256 sha = SHA256.Create())
        using (FileStream stream = File.OpenRead(path))
        {
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }

    private static string GetArgument(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return Path.GetFullPath(args[i + 1]);
        }

        return string.Empty;
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
}
