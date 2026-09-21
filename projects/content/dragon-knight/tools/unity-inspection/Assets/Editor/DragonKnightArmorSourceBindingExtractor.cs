using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class DragonKnightArmorSourceBindingExtractor
{
    private const string PassMarker = "DRAGON_KNIGHT_ARMOR_A1_SOURCE_BINDING_PASS";
    private const string BlockedMarker = "DRAGON_KNIGHT_ARMOR_A1_SOURCE_BINDING_BLOCKED";
    private const string ImportRoot = "Assets/DragonKnightArmorA1";
    private const string ImportedFbxPath = ImportRoot + "/SK_Dragon_knight_UE5_no_Cape.fbx";
    private const string RuntimeStatus = "blocked_source_binding_only";
    private const string BindingModeSplitRenderers = "split_renderer_parts";
    private const string BindingModeCombinedRenderer = "combined_renderer_material_slot_map";

    private static readonly string[] ExpectedParts =
    {
        "head_armor",
        "body_only",
        "collar_armor",
        "shoulder_armor",
        "upperarm_armor",
        "arm_chainmail",
        "elbow_armor",
        "gauntlets",
        "hip_armor",
        "leg_chainmail",
        "thigh_armor",
        "knee_armor",
        "calf_armor",
        "foot_armor",
    };

    private static readonly string[] RequiredCombinedArmorSlots =
    {
        "arms",
        "legs",
        "torso",
        "waist_cloth",
    };

    public static void Extract()
    {
        string reportPath = GetArgument("-dragonKnightArmorBindingReport", string.Empty);
        var blockedReasons = new List<string>();
        var report = new BindingReport
        {
            marker = BlockedMarker,
            unityVersion = Application.unityVersion,
            projectRoot = ProjectRoot,
            importedAssetPath = ImportedFbxPath,
            expectedParts = ExpectedParts,
            runtimeStatus = RuntimeStatus,
        };

        try
        {
            string sourceFbxPath = GetArgument("-dragonKnightArmorSourceFbxPath", string.Empty);
            string expectedSha256 = GetArgument("-dragonKnightArmorSourceSha256", string.Empty, false);
            string sourceRoot = GetArgument("-dragonKnightArmorSourceRoot", string.Empty);

            if (string.IsNullOrWhiteSpace(reportPath)) throw new ArgumentException("Missing -dragonKnightArmorBindingReport.");
            if (string.IsNullOrWhiteSpace(sourceFbxPath)) throw new ArgumentException("Missing -dragonKnightArmorSourceFbxPath.");
            if (!File.Exists(sourceFbxPath)) throw new FileNotFoundException("Dragon Knight armor source FBX not found.", sourceFbxPath);

            report.sourceFbxPath = Path.GetFullPath(sourceFbxPath);
            report.expectedSha256 = expectedSha256;
            report.actualSha256 = Sha256(report.sourceFbxPath);
            if (!string.IsNullOrWhiteSpace(expectedSha256)
                && !string.Equals(report.actualSha256, expectedSha256, StringComparison.OrdinalIgnoreCase))
            {
                blockedReasons.Add("source_sha256_mismatch");
            }

            ResetImportRoot();
            Directory.CreateDirectory(Path.GetDirectoryName(FullPathForAsset(ImportedFbxPath)) ?? ProjectRoot);
            File.Copy(report.sourceFbxPath, FullPathForAsset(ImportedFbxPath), true);
            AssetDatabase.ImportAsset(ImportedFbxPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            GameObject sourceRootObject = AssetDatabase.LoadAssetAtPath<GameObject>(ImportedFbxPath);
            if (sourceRootObject == null)
            {
                blockedReasons.Add("imported_fbx_game_object_missing");
            }
            else
            {
                PopulateSourceBinding(report, sourceRootObject, blockedReasons);
            }

            report.armorTextureRefs = ScanArmorTextureRefs(sourceRoot);
            report.ironTextureRefs = report.armorTextureRefs;
            if (report.armorTextureRefs.Length == 0)
            {
                blockedReasons.Add("iron_armor_texture_refs_missing_or_source_root_unreadable");
            }
            else
            {
                string[] missingTextureSlots = RequiredCombinedArmorSlots
                    .Where(slot => report.armorTextureRefs.All(texture => !string.Equals(texture.armorSlot, slot, StringComparison.Ordinal)))
                    .OrderBy(slot => slot, StringComparer.Ordinal)
                    .ToArray();
                if (missingTextureSlots.Length > 0)
                {
                    blockedReasons.Add("iron_armor_texture_slots_missing_" + string.Join("_", missingTextureSlots));
                }
            }

            report.blockedReasons = blockedReasons.Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
            report.marker = report.blockedReasons.Length == 0 ? PassMarker : BlockedMarker;
            WriteReport(reportPath, report);
            Debug.Log(report.marker + ": " + reportPath);
            EditorApplication.Exit(report.marker == PassMarker ? 0 : 1);
        }
        catch (Exception ex)
        {
            report.errors = new[] { ex.ToString() };
            report.blockedReasons = blockedReasons.Concat(new[] { "extractor_exception" })
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();
            WriteReport(reportPath, report);
            Debug.LogError(BlockedMarker + ": " + ex);
            EditorApplication.Exit(1);
        }
    }

    private static void PopulateSourceBinding(BindingReport report, GameObject sourceRootObject, List<string> blockedReasons)
    {
        Transform[] transforms = sourceRootObject.GetComponentsInChildren<Transform>(true);
        SkinnedMeshRenderer[] renderers = sourceRootObject.GetComponentsInChildren<SkinnedMeshRenderer>(true)
            .OrderBy(renderer => renderer.name, StringComparer.Ordinal)
            .ToArray();

        report.transformCount = transforms.Length;
        report.allBonePaths = renderers
            .SelectMany(renderer => renderer.bones ?? Array.Empty<Transform>())
            .Where(bone => bone != null)
            .Select(TransformPath)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        report.allSkinnedRenderers = renderers.Select(InspectRenderer).ToArray();
        report.selectedParts = ExpectedParts
            .Select(part => report.allSkinnedRenderers.FirstOrDefault(renderer => string.Equals(renderer.name, part, StringComparison.Ordinal)))
            .Where(renderer => renderer != null)
            .ToArray();
        report.missingExpectedParts = ExpectedParts
            .Where(part => report.selectedParts.All(renderer => !string.Equals(renderer.name, part, StringComparison.Ordinal)))
            .OrderBy(part => part, StringComparer.Ordinal)
            .ToArray();
        report.capeRendererPresent = report.allSkinnedRenderers.Any(renderer => string.Equals(renderer.name, "cape_only", StringComparison.Ordinal));

        if (report.allSkinnedRenderers.Length == 0) blockedReasons.Add("no_skinned_renderers");
        if (report.capeRendererPresent) blockedReasons.Add("cape_only_present_in_no_cape_source");

        if (report.missingExpectedParts.Length == 0)
        {
            report.bindingMode = BindingModeSplitRenderers;
            report.armorSlotMap = BuildRendererSlotMap(report.selectedParts);
            foreach (RendererBinding renderer in report.selectedParts)
            {
                ValidateRendererBinding(renderer, renderer.name, blockedReasons);
            }
        }
        else
        {
            RendererBinding combinedRenderer = SelectCombinedRenderer(report.allSkinnedRenderers);
            if (combinedRenderer == null)
            {
                blockedReasons.Add("selected_armor_parts_missing_no_complete_combined_renderer");
                return;
            }

            report.bindingMode = BindingModeCombinedRenderer;
            report.combinedRenderer = combinedRenderer;
            report.armorSlotMap = BuildRendererSlotMap(new[] { combinedRenderer });
            ValidateRendererBinding(combinedRenderer, "combined_renderer_" + combinedRenderer.name, blockedReasons);
            ValidateRequiredArmorSlotMap(report.armorSlotMap, blockedReasons);
        }
    }

    private static RendererBinding InspectRenderer(SkinnedMeshRenderer renderer)
    {
        Mesh mesh = renderer.sharedMesh;
        var binding = new RendererBinding
        {
            name = renderer.name,
            transformPath = TransformPath(renderer.transform),
            rootBone = renderer.rootBone != null ? TransformPath(renderer.rootBone) : string.Empty,
            rootBoneMissing = renderer.rootBone == null,
            bonePaths = (renderer.bones ?? Array.Empty<Transform>())
                .Where(bone => bone != null)
                .Select(TransformPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray(),
            rendererLocalBounds = BoundsSummary.From(renderer.localBounds),
            meshMissing = mesh == null,
            runtimeStatus = RuntimeStatus,
            materialSlots = (renderer.sharedMaterials ?? Array.Empty<Material>())
                .Select((material, index) => new MaterialSlot
                {
                    index = index,
                    name = material != null ? material.name : string.Empty,
                    shader = material != null && material.shader != null ? material.shader.name : string.Empty,
                })
                .ToArray(),
        };

        binding.boneCount = binding.bonePaths.Length;
        binding.materialSlotCount = binding.materialSlots.Length;

        if (mesh == null) return binding;

        BoneWeight[] weights = mesh.boneWeights ?? Array.Empty<BoneWeight>();
        Matrix4x4[] bindPoses = mesh.bindposes ?? Array.Empty<Matrix4x4>();
        binding.meshName = mesh.name;
        binding.vertexCount = mesh.vertexCount;
        binding.subMeshCount = mesh.subMeshCount;
        binding.blendShapeCount = mesh.blendShapeCount;
        binding.bindPoseCount = bindPoses.Length;
        binding.bindPoseHash = HashMatrices(bindPoses);
        binding.boneWeightCount = weights.Length;
        binding.nonZeroWeightedVertices = weights.Count(HasAnyWeight);
        binding.maxBoneInfluences = weights.Length == 0 ? 0 : weights.Max(InfluenceCount);
        binding.meshBounds = BoundsSummary.From(mesh.bounds);
        binding.uvChannels = InspectUvChannels(mesh);
        binding.hasUv0 = binding.uvChannels.Any(channel => channel.channel == 0 && channel.count == mesh.vertexCount);
        binding.subMeshes = InspectSubMeshes(mesh, binding.materialSlots);
        return binding;
    }

    private static SubMeshBinding[] InspectSubMeshes(Mesh mesh, MaterialSlot[] materialSlots)
    {
        var subMeshes = new List<SubMeshBinding>();
        for (int index = 0; index < mesh.subMeshCount; index++)
        {
            SubMeshDescriptor descriptor = mesh.GetSubMesh(index);
            MaterialSlot materialSlot = index < materialSlots.Length ? materialSlots[index] : null;
            string materialName = materialSlot != null ? materialSlot.name : string.Empty;
            string armorSlot = ArmorSlotFromMaterialName(materialName);
            subMeshes.Add(new SubMeshBinding
            {
                index = index,
                materialName = materialName,
                materialShader = materialSlot != null ? materialSlot.shader : string.Empty,
                topology = descriptor.topology.ToString(),
                indexStart = descriptor.indexStart,
                indexCount = descriptor.indexCount,
                baseVertex = descriptor.baseVertex,
                firstVertex = descriptor.firstVertex,
                vertexCount = descriptor.vertexCount,
                bounds = BoundsSummary.From(descriptor.bounds),
                armorSlot = armorSlot,
                mappingBasis = string.IsNullOrEmpty(armorSlot) ? "unmapped_material_name" : "material_name",
            });
        }

        return subMeshes.ToArray();
    }

    private static RendererBinding SelectCombinedRenderer(RendererBinding[] renderers)
    {
        return renderers
            .Where(IsCompleteCombinedRendererCandidate)
            .OrderByDescending(renderer => renderer.vertexCount)
            .ThenBy(renderer => renderer.name, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static bool IsCompleteCombinedRendererCandidate(RendererBinding renderer)
    {
        return !renderer.meshMissing
               && !renderer.rootBoneMissing
               && renderer.boneCount > 0
               && renderer.bindPoseCount > 0
               && renderer.boneWeightCount > 0
               && renderer.nonZeroWeightedVertices > 0
               && renderer.hasUv0
               && renderer.subMeshCount > 0
               && renderer.materialSlotCount >= renderer.subMeshCount
               && renderer.subMeshes.Length == renderer.subMeshCount;
    }

    private static ArmorSlotBinding[] BuildRendererSlotMap(RendererBinding[] renderers)
    {
        return renderers
            .SelectMany(renderer => renderer.subMeshes.Select(subMesh => new ArmorSlotBinding
            {
                armorSlot = subMesh.armorSlot,
                sourceRendererName = renderer.name,
                sourceRendererPath = renderer.transformPath,
                subMeshIndex = subMesh.index,
                materialSlotIndex = subMesh.index,
                materialName = subMesh.materialName,
                materialShader = subMesh.materialShader,
                mappingBasis = subMesh.mappingBasis,
                runtimeStatus = RuntimeStatus,
            }))
            .OrderBy(binding => binding.sourceRendererName, StringComparer.Ordinal)
            .ThenBy(binding => binding.subMeshIndex)
            .ToArray();
    }

    private static void ValidateRendererBinding(RendererBinding renderer, string reasonPrefix, List<string> blockedReasons)
    {
        if (renderer.meshMissing) blockedReasons.Add(reasonPrefix + "_mesh_missing");
        if (renderer.rootBoneMissing) blockedReasons.Add(reasonPrefix + "_root_bone_missing");
        if (renderer.boneCount <= 0) blockedReasons.Add(reasonPrefix + "_bones_missing");
        if (renderer.bindPoseCount <= 0) blockedReasons.Add(reasonPrefix + "_bindposes_missing");
        if (renderer.boneWeightCount <= 0 || renderer.nonZeroWeightedVertices <= 0) blockedReasons.Add(reasonPrefix + "_weights_missing");
        if (!renderer.hasUv0) blockedReasons.Add(reasonPrefix + "_uv0_missing");
        if (renderer.materialSlotCount <= 0) blockedReasons.Add(reasonPrefix + "_material_slots_missing");
        if (renderer.subMeshCount <= 0) blockedReasons.Add(reasonPrefix + "_submeshes_missing");
        if (renderer.materialSlotCount < renderer.subMeshCount) blockedReasons.Add(reasonPrefix + "_material_slots_less_than_submeshes");
        if (renderer.subMeshes.Length != renderer.subMeshCount) blockedReasons.Add(reasonPrefix + "_submesh_descriptors_incomplete");
    }

    private static void ValidateRequiredArmorSlotMap(ArmorSlotBinding[] armorSlotMap, List<string> blockedReasons)
    {
        if (armorSlotMap.Length == 0)
        {
            blockedReasons.Add("combined_renderer_armor_slot_map_missing");
            return;
        }

        if (armorSlotMap.Any(binding => string.IsNullOrWhiteSpace(binding.armorSlot)))
        {
            blockedReasons.Add("combined_renderer_armor_slot_map_unmapped_material");
        }

        string[] missingSlots = RequiredCombinedArmorSlots
            .Where(slot => armorSlotMap.All(binding => !string.Equals(binding.armorSlot, slot, StringComparison.Ordinal)))
            .OrderBy(slot => slot, StringComparer.Ordinal)
            .ToArray();
        if (missingSlots.Length > 0)
        {
            blockedReasons.Add("combined_renderer_armor_slots_missing_" + string.Join("_", missingSlots));
        }
    }

    private static UvChannel[] InspectUvChannels(Mesh mesh)
    {
        var channels = new List<UvChannel>();
        for (int channel = 0; channel < 8; channel++)
        {
            var values = new List<Vector4>();
            mesh.GetUVs(channel, values);
            if (values.Count > 0)
            {
                channels.Add(new UvChannel { channel = channel, count = values.Count });
            }
        }

        return channels.ToArray();
    }

    private static TextureRef[] ScanArmorTextureRefs(string sourceRoot)
    {
        if (string.IsNullOrWhiteSpace(sourceRoot) || !Directory.Exists(sourceRoot)) return Array.Empty<TextureRef>();
        string resolvedRoot = Path.GetFullPath(sourceRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return Directory.GetFiles(resolvedRoot, "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                           || path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            .Select(path => BuildArmorTextureRef(resolvedRoot, path))
            .Where(texture => texture != null)
            .Where(texture => string.Equals(texture.materialLane, "Iron", StringComparison.OrdinalIgnoreCase))
            .Where(texture => !string.IsNullOrWhiteSpace(texture.armorSlot))
            .OrderBy(texture => texture.relativePath, StringComparer.Ordinal)
            .ToArray();
    }

    private static TextureRef BuildArmorTextureRef(string resolvedRoot, string path)
    {
        string relativePath = path.Substring(resolvedRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string fileName = Path.GetFileNameWithoutExtension(path);
        string materialGroup = MaterialGroupFromName(fileName);
        if (string.IsNullOrEmpty(materialGroup)) return null;

        return new TextureRef
        {
            relativePath = relativePath,
            materialLane = MaterialLaneFromRelativePath(relativePath),
            materialGroup = materialGroup,
            armorSlot = ArmorSlotFromMaterialName(materialGroup),
            textureKind = TextureKindFromName(fileName, materialGroup),
            bytes = new FileInfo(path).Length,
            sha256 = Sha256(path),
        };
    }

    private static string MaterialLaneFromRelativePath(string relativePath)
    {
        string[] parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        for (int i = 0; i < parts.Length - 1; i++)
        {
            if (string.Equals(parts[i], "TEXTURES", StringComparison.OrdinalIgnoreCase))
            {
                return parts[i + 1];
            }
        }

        return string.Empty;
    }

    private static string MaterialGroupFromName(string name)
    {
        if (name.IndexOf("ArmMAT", StringComparison.OrdinalIgnoreCase) >= 0) return "ArmMAT";
        if (name.IndexOf("BodyMAT", StringComparison.OrdinalIgnoreCase) >= 0) return "BodyMAT";
        if (name.IndexOf("ClothMAT", StringComparison.OrdinalIgnoreCase) >= 0) return "ClothMAT";
        if (name.IndexOf("LegMAT", StringComparison.OrdinalIgnoreCase) >= 0) return "LegMAT";
        return string.Empty;
    }

    private static string TextureKindFromName(string name, string materialGroup)
    {
        int groupIndex = name.IndexOf(materialGroup, StringComparison.OrdinalIgnoreCase);
        if (groupIndex < 0) return string.Empty;
        string suffix = name.Substring(groupIndex + materialGroup.Length).TrimStart('_');
        return string.IsNullOrEmpty(suffix) ? "unknown" : suffix;
    }

    private static string ArmorSlotFromMaterialName(string materialName)
    {
        if (materialName.IndexOf("ArmMAT", StringComparison.OrdinalIgnoreCase) >= 0) return "arms";
        if (materialName.IndexOf("BodyMAT", StringComparison.OrdinalIgnoreCase) >= 0) return "torso";
        if (materialName.IndexOf("ClothMAT", StringComparison.OrdinalIgnoreCase) >= 0) return "waist_cloth";
        if (materialName.IndexOf("LegMAT", StringComparison.OrdinalIgnoreCase) >= 0) return "legs";
        return string.Empty;
    }

    private static bool HasAnyWeight(BoneWeight weight)
    {
        return weight.weight0 > 0f || weight.weight1 > 0f || weight.weight2 > 0f || weight.weight3 > 0f;
    }

    private static int InfluenceCount(BoneWeight weight)
    {
        int count = 0;
        if (weight.weight0 > 0f) count++;
        if (weight.weight1 > 0f) count++;
        if (weight.weight2 > 0f) count++;
        if (weight.weight3 > 0f) count++;
        return count;
    }

    private static void ResetImportRoot()
    {
        if (AssetDatabase.IsValidFolder(ImportRoot) && !AssetDatabase.DeleteAsset(ImportRoot))
        {
            throw new InvalidOperationException("Could not delete previous import root: " + ImportRoot);
        }

        string fullRoot = FullPathForAsset(ImportRoot);
        if (Directory.Exists(fullRoot))
        {
            Directory.Delete(fullRoot, true);
        }
    }

    private static string TransformPath(Transform transform)
    {
        var names = new Stack<string>();
        Transform current = transform;
        while (current != null)
        {
            names.Push(current.name);
            current = current.parent;
        }

        return string.Join("/", names.ToArray());
    }

    private static string FullPathForAsset(string assetPath)
    {
        return Path.GetFullPath(Path.Combine(ProjectRoot, assetPath));
    }

    private static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

    private static string GetArgument(string name, string fallback, bool resolvePath = true)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return resolvePath ? Path.GetFullPath(args[i + 1]) : args[i + 1];
            }
        }

        return fallback;
    }

    private static string Sha256(string path)
    {
        using (SHA256 sha = SHA256.Create())
        using (FileStream stream = File.OpenRead(path))
        {
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }

    private static string HashMatrices(Matrix4x4[] matrices)
    {
        if (matrices.Length == 0) return string.Empty;
        using (SHA256 sha = SHA256.Create())
        using (var stream = new MemoryStream())
        {
            foreach (Matrix4x4 matrix in matrices)
            {
                for (int row = 0; row < 4; row++)
                {
                    for (int column = 0; column < 4; column++)
                    {
                        byte[] bytes = BitConverter.GetBytes(matrix[row, column]);
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
            }

            return BitConverter.ToString(sha.ComputeHash(stream.ToArray())).Replace("-", string.Empty);
        }
    }

    private static void WriteReport(string reportPath, BindingReport report)
    {
        if (string.IsNullOrWhiteSpace(reportPath)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? ".");
        File.WriteAllText(reportPath, JsonUtility.ToJson(report, true), new UTF8Encoding(false));
    }

    [Serializable]
    private sealed class BindingReport
    {
        public string marker = string.Empty;
        public string unityVersion = string.Empty;
        public string projectRoot = string.Empty;
        public string sourceFbxPath = string.Empty;
        public string expectedSha256 = string.Empty;
        public string actualSha256 = string.Empty;
        public string importedAssetPath = string.Empty;
        public string runtimeStatus = string.Empty;
        public string bindingMode = string.Empty;
        public int transformCount;
        public string[] expectedParts = Array.Empty<string>();
        public string[] missingExpectedParts = Array.Empty<string>();
        public bool capeRendererPresent;
        public string[] allBonePaths = Array.Empty<string>();
        public RendererBinding[] allSkinnedRenderers = Array.Empty<RendererBinding>();
        public RendererBinding[] selectedParts = Array.Empty<RendererBinding>();
        public RendererBinding combinedRenderer = new RendererBinding();
        public ArmorSlotBinding[] armorSlotMap = Array.Empty<ArmorSlotBinding>();
        public TextureRef[] ironTextureRefs = Array.Empty<TextureRef>();
        public TextureRef[] armorTextureRefs = Array.Empty<TextureRef>();
        public string[] blockedReasons = Array.Empty<string>();
        public string[] errors = Array.Empty<string>();
    }

    [Serializable]
    private sealed class RendererBinding
    {
        public string name = string.Empty;
        public string transformPath = string.Empty;
        public string rootBone = string.Empty;
        public bool rootBoneMissing;
        public string meshName = string.Empty;
        public bool meshMissing;
        public int vertexCount;
        public int subMeshCount;
        public int blendShapeCount;
        public int boneCount;
        public int bindPoseCount;
        public string bindPoseHash = string.Empty;
        public int boneWeightCount;
        public int nonZeroWeightedVertices;
        public int maxBoneInfluences;
        public int materialSlotCount;
        public bool hasUv0;
        public string runtimeStatus = string.Empty;
        public string[] bonePaths = Array.Empty<string>();
        public BoundsSummary meshBounds = new BoundsSummary();
        public BoundsSummary rendererLocalBounds = new BoundsSummary();
        public UvChannel[] uvChannels = Array.Empty<UvChannel>();
        public MaterialSlot[] materialSlots = Array.Empty<MaterialSlot>();
        public SubMeshBinding[] subMeshes = Array.Empty<SubMeshBinding>();
    }

    [Serializable]
    private sealed class BoundsSummary
    {
        public string center = string.Empty;
        public string size = string.Empty;

        public static BoundsSummary From(Bounds bounds)
        {
            return new BoundsSummary
            {
                center = FormatVector(bounds.center),
                size = FormatVector(bounds.size),
            };
        }

        private static string FormatVector(Vector3 vector)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1},{2}",
                vector.x,
                vector.y,
                vector.z);
        }
    }

    [Serializable]
    private sealed class UvChannel
    {
        public int channel;
        public int count;
    }

    [Serializable]
    private sealed class MaterialSlot
    {
        public int index;
        public string name = string.Empty;
        public string shader = string.Empty;
    }

    [Serializable]
    private sealed class SubMeshBinding
    {
        public int index;
        public string materialName = string.Empty;
        public string materialShader = string.Empty;
        public string topology = string.Empty;
        public int indexStart;
        public int indexCount;
        public int baseVertex;
        public int firstVertex;
        public int vertexCount;
        public BoundsSummary bounds = new BoundsSummary();
        public string armorSlot = string.Empty;
        public string mappingBasis = string.Empty;
    }

    [Serializable]
    private sealed class ArmorSlotBinding
    {
        public string armorSlot = string.Empty;
        public string sourceRendererName = string.Empty;
        public string sourceRendererPath = string.Empty;
        public int subMeshIndex;
        public int materialSlotIndex;
        public string materialName = string.Empty;
        public string materialShader = string.Empty;
        public string mappingBasis = string.Empty;
        public string runtimeStatus = string.Empty;
    }

    [Serializable]
    private sealed class TextureRef
    {
        public string relativePath = string.Empty;
        public string materialLane = string.Empty;
        public string materialGroup = string.Empty;
        public string armorSlot = string.Empty;
        public string textureKind = string.Empty;
        public long bytes;
        public string sha256 = string.Empty;
    }
}
