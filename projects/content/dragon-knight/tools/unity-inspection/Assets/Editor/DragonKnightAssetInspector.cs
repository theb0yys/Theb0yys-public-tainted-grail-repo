using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class DragonKnightAssetInspector
{
    private const string Root = "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT";
    private static string _reportPath = string.Empty;
    private static bool _finished;

    public static void ImportAndInspect()
    {
        try
        {
            string packagePath = GetArgument("-dragonKnightPackagePath");
            _reportPath = GetArgument("-dragonKnightReportPath");
            if (string.IsNullOrWhiteSpace(packagePath)) throw new ArgumentException("Missing -dragonKnightPackagePath.");
            if (string.IsNullOrWhiteSpace(_reportPath)) throw new ArgumentException("Missing -dragonKnightReportPath.");
            if (!File.Exists(packagePath)) throw new FileNotFoundException("Dragon Knight unitypackage was not found.", packagePath);

            AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
            AssetDatabase.importPackageFailed += OnImportPackageFailed;
            AssetDatabase.ImportPackage(packagePath, false);
            EditorApplication.delayCall += TryFallbackInspect;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }

    private static void OnImportPackageCompleted(string packageName)
    {
        FinishInspect("import-completed:" + packageName);
    }

    private static void OnImportPackageFailed(string packageName, string errorMessage)
    {
        if (_finished) return;
        _finished = true;
        Debug.LogError("[Dragon Knight] Unity package import failed: " + packageName + "; " + errorMessage);
        EditorApplication.Exit(1);
    }

    private static void TryFallbackInspect()
    {
        if (_finished) return;
        if (!AssetDatabase.IsValidFolder(Root))
        {
            EditorApplication.delayCall += TryFallbackInspect;
            return;
        }

        FinishInspect("fallback-root-detected");
    }

    private static void FinishInspect(string reason)
    {
        if (_finished) return;
        _finished = true;
        try
        {
            AssetDatabase.importPackageCompleted -= OnImportPackageCompleted;
            AssetDatabase.importPackageFailed -= OnImportPackageFailed;
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            InspectionReport report = Inspect();
            Directory.CreateDirectory(Path.GetDirectoryName(_reportPath) ?? ".");
            File.WriteAllText(_reportPath, ToJson(report), new UTF8Encoding(false));
            Debug.Log("[Dragon Knight] Asset inspection written to " + _reportPath + "; reason=" + reason);
            EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }

    private static InspectionReport Inspect()
    {
        string[] paths = AssetDatabase.FindAssets(string.Empty, new[] { Root })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();

        var report = new InspectionReport
        {
            unityVersion = Application.unityVersion,
            assetRoot = Root,
            assetCount = paths.Length,
            allPaths = paths,
            assets = paths.Select(InspectAsset).ToArray(),
            prefabs = paths.Where(path => path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                .Select(InspectPrefab)
                .ToArray()
        };

        report.animationClipNames = report.assets
            .SelectMany(asset => asset.subAssets)
            .Where(asset => string.Equals(asset.type, nameof(AnimationClip), StringComparison.Ordinal))
            .Select(asset => asset.name)
            .Where(name => !string.IsNullOrWhiteSpace(name) && !name.StartsWith("__preview__", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        report.controllerStateNames = report.assets
            .SelectMany(asset => asset.controllerStates ?? Array.Empty<string>())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        return report;
    }

    private static AssetSummary InspectAsset(string path)
    {
        Type mainType = AssetDatabase.GetMainAssetTypeAtPath(path);
        UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(path) ?? Array.Empty<UnityEngine.Object>();
        var summary = new AssetSummary
        {
            path = path,
            mainType = mainType?.FullName ?? string.Empty,
            subAssets = subAssets
                .Where(asset => asset != null)
                .Select(asset => new SubAssetSummary
                {
                    name = asset.name ?? string.Empty,
                    type = asset.GetType().Name
                })
                .OrderBy(asset => asset.type, StringComparer.Ordinal)
                .ThenBy(asset => asset.name, StringComparer.Ordinal)
                .ToArray()
        };

        if (AssetImporter.GetAtPath(path) is ModelImporter modelImporter)
        {
            summary.modelImporterClips = modelImporter.clipAnimations
                .Concat(modelImporter.defaultClipAnimations)
                .Select(clip => clip.name ?? string.Empty)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
        }

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller != null)
        {
            summary.controllerParameters = controller.parameters
                .Select(parameter => parameter.name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            summary.controllerStates = controller.layers
                .SelectMany(layer => FlattenStates(layer.stateMachine))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
        }

        return summary;
    }

    private static IEnumerable<string> FlattenStates(AnimatorStateMachine stateMachine)
    {
        foreach (ChildAnimatorState childState in stateMachine.states)
        {
            if (childState.state != null) yield return childState.state.name;
        }

        foreach (ChildAnimatorStateMachine childMachine in stateMachine.stateMachines)
        {
            if (childMachine.stateMachine == null) continue;
            foreach (string state in FlattenStates(childMachine.stateMachine)) yield return state;
        }
    }

    private static PrefabSummary InspectPrefab(string path)
    {
        GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (root == null) return new PrefabSummary { path = path, loadFailed = true };

        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        var components = new List<Component>();
        int missingScripts = 0;
        foreach (Transform transform in transforms)
        {
            Component[] local = transform.GetComponents<Component>();
            missingScripts += local.Count(component => component == null);
            components.AddRange(local.Where(component => component != null));
        }

        Animator[] animators = root.GetComponentsInChildren<Animator>(true);
        string[] clipNames = animators
            .Where(animator => animator.runtimeAnimatorController != null)
            .SelectMany(animator => animator.runtimeAnimatorController.animationClips ?? Array.Empty<AnimationClip>())
            .Where(clip => clip != null)
            .Select(clip => clip.name)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        return new PrefabSummary
        {
            path = path,
            name = root.name,
            transformCount = transforms.Length,
            componentCount = components.Count,
            missingScriptCount = missingScripts,
            skinnedMeshRendererCount = root.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length,
            meshRendererCount = root.GetComponentsInChildren<MeshRenderer>(true).Length,
            animatorCount = animators.Length,
            controllerNames = animators
                .Select(animator => animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : string.Empty)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray(),
            animationClipNames = clipNames,
            colliderCount = root.GetComponentsInChildren<Collider>(true).Length,
            componentTypes = components
                .Select(component => component.GetType().FullName ?? component.GetType().Name)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(type => type, StringComparer.Ordinal)
                .ToArray()
        };
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

    private static string ToJson(InspectionReport report)
    {
        var builder = new JsonBuilder();
        builder.Object(() =>
        {
            builder.Prop("unityVersion", report.unityVersion);
            builder.Prop("assetRoot", report.assetRoot);
            builder.Prop("assetCount", report.assetCount);
            builder.ArrayProp("animationClipNames", report.animationClipNames);
            builder.ArrayProp("controllerStateNames", report.controllerStateNames);
            builder.ArrayProp("allPaths", report.allPaths);
            builder.ArrayProp("assets", report.assets, WriteAsset);
            builder.ArrayProp("prefabs", report.prefabs, WritePrefab);
        });
        return builder.ToString();
    }

    private static void WriteAsset(JsonBuilder builder, AssetSummary asset)
    {
        builder.Object(() =>
        {
            builder.Prop("path", asset.path);
            builder.Prop("mainType", asset.mainType);
            builder.ArrayProp("modelImporterClips", asset.modelImporterClips ?? Array.Empty<string>());
            builder.ArrayProp("controllerParameters", asset.controllerParameters ?? Array.Empty<string>());
            builder.ArrayProp("controllerStates", asset.controllerStates ?? Array.Empty<string>());
            builder.ArrayProp("subAssets", asset.subAssets, (b, subAsset) =>
            {
                b.Object(() =>
                {
                    b.Prop("name", subAsset.name);
                    b.Prop("type", subAsset.type);
                });
            });
        });
    }

    private static void WritePrefab(JsonBuilder builder, PrefabSummary prefab)
    {
        builder.Object(() =>
        {
            builder.Prop("path", prefab.path);
            builder.Prop("name", prefab.name);
            builder.Prop("loadFailed", prefab.loadFailed);
            builder.Prop("transformCount", prefab.transformCount);
            builder.Prop("componentCount", prefab.componentCount);
            builder.Prop("missingScriptCount", prefab.missingScriptCount);
            builder.Prop("skinnedMeshRendererCount", prefab.skinnedMeshRendererCount);
            builder.Prop("meshRendererCount", prefab.meshRendererCount);
            builder.Prop("animatorCount", prefab.animatorCount);
            builder.Prop("colliderCount", prefab.colliderCount);
            builder.ArrayProp("controllerNames", prefab.controllerNames ?? Array.Empty<string>());
            builder.ArrayProp("animationClipNames", prefab.animationClipNames ?? Array.Empty<string>());
            builder.ArrayProp("componentTypes", prefab.componentTypes ?? Array.Empty<string>());
        });
    }

    [Serializable]
    private sealed class InspectionReport
    {
        public string unityVersion = string.Empty;
        public string assetRoot = string.Empty;
        public int assetCount;
        public string[] allPaths = Array.Empty<string>();
        public string[] animationClipNames = Array.Empty<string>();
        public string[] controllerStateNames = Array.Empty<string>();
        public AssetSummary[] assets = Array.Empty<AssetSummary>();
        public PrefabSummary[] prefabs = Array.Empty<PrefabSummary>();
    }

    [Serializable]
    private sealed class AssetSummary
    {
        public string path = string.Empty;
        public string mainType = string.Empty;
        public string[] modelImporterClips = Array.Empty<string>();
        public string[] controllerParameters = Array.Empty<string>();
        public string[] controllerStates = Array.Empty<string>();
        public SubAssetSummary[] subAssets = Array.Empty<SubAssetSummary>();
    }

    [Serializable]
    private sealed class SubAssetSummary
    {
        public string name = string.Empty;
        public string type = string.Empty;
    }

    [Serializable]
    private sealed class PrefabSummary
    {
        public string path = string.Empty;
        public string name = string.Empty;
        public bool loadFailed;
        public int transformCount;
        public int componentCount;
        public int missingScriptCount;
        public int skinnedMeshRendererCount;
        public int meshRendererCount;
        public int animatorCount;
        public int colliderCount;
        public string[] controllerNames = Array.Empty<string>();
        public string[] animationClipNames = Array.Empty<string>();
        public string[] componentTypes = Array.Empty<string>();
    }

    private sealed class JsonBuilder
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private readonly Stack<bool> _first = new Stack<bool>();

        public override string ToString() => _builder.ToString();

        public void Object(Action write)
        {
            _builder.Append("{");
            _first.Push(true);
            write();
            _first.Pop();
            _builder.Append("}");
        }

        public void Prop(string name, string value)
        {
            Prefix();
            _builder.Append('"').Append(Escape(name)).Append("\":\"").Append(Escape(value ?? string.Empty)).Append('"');
        }

        public void Prop(string name, int value)
        {
            Prefix();
            _builder.Append('"').Append(Escape(name)).Append("\":").Append(value);
        }

        public void Prop(string name, bool value)
        {
            Prefix();
            _builder.Append('"').Append(Escape(name)).Append("\":").Append(value ? "true" : "false");
        }

        public void ArrayProp(string name, string[] values)
        {
            Prefix();
            _builder.Append('"').Append(Escape(name)).Append("\":[");
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0) _builder.Append(",");
                _builder.Append('"').Append(Escape(values[i] ?? string.Empty)).Append('"');
            }
            _builder.Append("]");
        }

        public void ArrayProp<T>(string name, T[] values, Action<JsonBuilder, T> writeItem)
        {
            Prefix();
            _builder.Append('"').Append(Escape(name)).Append("\":[");
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0) _builder.Append(",");
                writeItem(this, values[i]);
            }
            _builder.Append("]");
        }

        private void Prefix()
        {
            if (_first.Count == 0) return;
            bool first = _first.Pop();
            if (!first) _builder.Append(",");
            _first.Push(false);
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
}
