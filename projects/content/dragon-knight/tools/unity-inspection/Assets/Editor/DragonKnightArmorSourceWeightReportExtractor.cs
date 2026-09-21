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

public static class DragonKnightArmorSourceWeightReportExtractor
{
    private const string ReadyMarker = "DRAGON_KNIGHT_ARMOR_A2K_T8_SOURCE_PER_SLOT_WEIGHT_REPORT_READY";
    private const string BlockedMarker = "DRAGON_KNIGHT_ARMOR_A2K_T8_SOURCE_PER_SLOT_WEIGHT_REPORT_BLOCKED";
    private const string ImportRoot = "Assets/DragonKnightArmorA2KT8";
    private const string ImportedFbxPath = ImportRoot + "/SK_Dragon_knight_UE5_no_Cape.fbx";
    private const string RuntimeStatus = "blocked_source_weight_report_only";

    private static readonly string[] RequiredArmorSlots =
    {
        "arms",
        "legs",
        "torso",
        "waist_cloth",
    };

    private static readonly string[] CollapseEvidenceFamilies =
    {
        "twist",
        "finger_chain",
        "metacarpal",
        "ball",
        "extra_spine",
        "extra_neck",
    };

    public static void Extract()
    {
        string reportPath = GetArgument("-dragonKnightArmorWeightReport", string.Empty);
        var extractionBlockers = new List<string>();
        var report = new WeightReport
        {
            marker = BlockedMarker,
            unityVersion = Application.unityVersion,
            projectRoot = ProjectRoot,
            importedAssetPath = ImportedFbxPath,
            runtimeStatus = RuntimeStatus,
        };

        try
        {
            string sourceFbxPath = GetArgument("-dragonKnightArmorSourceFbxPath", string.Empty);
            string expectedSha256 = GetArgument("-dragonKnightArmorSourceSha256", string.Empty, false);

            if (string.IsNullOrWhiteSpace(reportPath)) throw new ArgumentException("Missing -dragonKnightArmorWeightReport.");
            if (string.IsNullOrWhiteSpace(sourceFbxPath)) throw new ArgumentException("Missing -dragonKnightArmorSourceFbxPath.");
            if (!File.Exists(sourceFbxPath)) throw new FileNotFoundException("Dragon Knight armor source FBX not found.", sourceFbxPath);

            report.sourceFbxPath = Path.GetFullPath(sourceFbxPath);
            report.expectedSha256 = expectedSha256;
            report.actualSha256 = Sha256(report.sourceFbxPath);
            if (!string.IsNullOrWhiteSpace(expectedSha256)
                && !string.Equals(report.actualSha256, expectedSha256, StringComparison.OrdinalIgnoreCase))
            {
                extractionBlockers.Add("source_sha256_mismatch");
            }

            ResetImportRoot();
            Directory.CreateDirectory(Path.GetDirectoryName(FullPathForAsset(ImportedFbxPath)) ?? ProjectRoot);
            File.Copy(report.sourceFbxPath, FullPathForAsset(ImportedFbxPath), true);
            AssetDatabase.ImportAsset(ImportedFbxPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            GameObject sourceRootObject = AssetDatabase.LoadAssetAtPath<GameObject>(ImportedFbxPath);
            if (sourceRootObject == null)
            {
                extractionBlockers.Add("imported_fbx_game_object_missing");
            }
            else
            {
                PopulateWeightReport(report, sourceRootObject, extractionBlockers);
            }

            CompleteReport(report, extractionBlockers);
            WriteReport(reportPath, report);
            Debug.Log(report.marker + ": " + reportPath);
            EditorApplication.Exit(report.marker == ReadyMarker ? 0 : 1);
        }
        catch (Exception ex)
        {
            report.errors = new[] { ex.ToString() };
            extractionBlockers.Add("extractor_exception");
            CompleteReport(report, extractionBlockers);
            WriteReport(reportPath, report);
            Debug.LogError(BlockedMarker + ": " + ex);
            EditorApplication.Exit(1);
        }
    }

    private static void PopulateWeightReport(WeightReport report, GameObject sourceRootObject, List<string> extractionBlockers)
    {
        Transform[] transforms = sourceRootObject.GetComponentsInChildren<Transform>(true);
        SkinnedMeshRenderer[] renderers = sourceRootObject.GetComponentsInChildren<SkinnedMeshRenderer>(true)
            .OrderBy(renderer => renderer.name, StringComparer.Ordinal)
            .ToArray();

        report.transformCount = transforms.Length;
        report.skinnedRendererCount = renderers.Length;
        report.allSkinnedRendererNames = renderers.Select(renderer => renderer.name).ToArray();

        if (renderers.Length == 0)
        {
            extractionBlockers.Add("no_skinned_renderers");
            return;
        }

        SkinnedMeshRenderer selectedRenderer = SelectCombinedRenderer(renderers);
        if (selectedRenderer == null)
        {
            extractionBlockers.Add("complete_combined_renderer_missing");
            return;
        }

        report.selectedRenderer = InspectRenderer(selectedRenderer);
        if (report.selectedRenderer.subMeshes.Length == 0)
        {
            extractionBlockers.Add("selected_renderer_submesh_descriptors_missing");
            return;
        }

        string[] missingSlots = RequiredArmorSlots
            .Where(slot => report.selectedRenderer.subMeshes.All(subMesh => !string.Equals(subMesh.armorSlot, slot, StringComparison.Ordinal)))
            .OrderBy(slot => slot, StringComparer.Ordinal)
            .ToArray();
        if (missingSlots.Length > 0)
        {
            extractionBlockers.Add("source_armor_slots_missing_" + string.Join("_", missingSlots));
        }

        SlotBuildResult buildResult = BuildSlotBoneRows(selectedRenderer, report.selectedRenderer.subMeshes);
        report.sourceSlotBoneRows = buildResult.rows.ToArray();
        report.slotSummaries = buildResult.slotSummaries.ToArray();
        report.policyFamilySummaries = BuildPolicyFamilySummaries(report.sourceSlotBoneRows);
        report.ikZeroWeightProofs = BuildIkZeroWeightProofs(report.sourceSlotBoneRows);
        report.collapseEvidenceSummaries = BuildCollapseEvidenceSummaries(report.sourceSlotBoneRows);

        if (report.sourceSlotBoneRows.Length == 0)
        {
            extractionBlockers.Add("source_slot_bone_rows_missing");
        }
    }

    private static RendererReport InspectRenderer(SkinnedMeshRenderer renderer)
    {
        Mesh mesh = renderer.sharedMesh;
        Transform[] bones = renderer.bones ?? Array.Empty<Transform>();
        Material[] materials = renderer.sharedMaterials ?? Array.Empty<Material>();
        BoneWeight[] weights = mesh != null ? mesh.boneWeights ?? Array.Empty<BoneWeight>() : Array.Empty<BoneWeight>();
        Matrix4x4[] bindPoses = mesh != null ? mesh.bindposes ?? Array.Empty<Matrix4x4>() : Array.Empty<Matrix4x4>();
        string[] bonePaths = bones
            .Select(bone => bone != null ? TransformPath(bone) : string.Empty)
            .ToArray();

        var report = new RendererReport
        {
            name = renderer.name,
            transformPath = TransformPath(renderer.transform),
            rootBone = renderer.rootBone != null ? TransformPath(renderer.rootBone) : string.Empty,
            meshName = mesh != null ? mesh.name : string.Empty,
            meshMissing = mesh == null,
            vertexCount = mesh != null ? mesh.vertexCount : 0,
            subMeshCount = mesh != null ? mesh.subMeshCount : 0,
            boneCount = bones.Length,
            bindPoseCount = bindPoses.Length,
            bindPoseHash = HashMatrices(bindPoses),
            boneWeightCount = weights.Length,
            nonZeroWeightedVertices = weights.Count(HasAnyWeight),
            maxBoneInfluences = weights.Length == 0 ? 0 : weights.Max(InfluenceCount),
            materialSlotCount = materials.Length,
            bonePaths = bonePaths,
            runtimeStatus = RuntimeStatus,
            meshBounds = mesh != null ? BoundsSummary.From(mesh.bounds) : new BoundsSummary(),
            rendererLocalBounds = BoundsSummary.From(renderer.localBounds),
            materialSlots = materials
                .Select((material, index) => new MaterialSlot
                {
                    index = index,
                    name = material != null ? material.name : string.Empty,
                    shader = material != null && material.shader != null ? material.shader.name : string.Empty,
                })
                .ToArray(),
        };

        if (mesh != null)
        {
            report.subMeshes = InspectSubMeshes(mesh, report.materialSlots);
        }

        return report;
    }

    private static SubMeshReport[] InspectSubMeshes(Mesh mesh, MaterialSlot[] materialSlots)
    {
        var subMeshes = new List<SubMeshReport>();
        for (int index = 0; index < mesh.subMeshCount; index++)
        {
            SubMeshDescriptor descriptor = mesh.GetSubMesh(index);
            MaterialSlot materialSlot = index < materialSlots.Length ? materialSlots[index] : null;
            string materialName = materialSlot != null ? materialSlot.name : string.Empty;
            string armorSlot = ArmorSlotFromMaterialName(materialName);
            int[] referencedVertices = ReferencedVertices(mesh, index, descriptor);
            subMeshes.Add(new SubMeshReport
            {
                index = index,
                materialSlotIndex = index,
                materialName = materialName,
                materialShader = materialSlot != null ? materialSlot.shader : string.Empty,
                topology = descriptor.topology.ToString(),
                indexStart = descriptor.indexStart,
                indexCount = descriptor.indexCount,
                baseVertex = descriptor.baseVertex,
                firstVertex = descriptor.firstVertex,
                descriptorVertexCount = descriptor.vertexCount,
                uniqueReferencedVertexCount = referencedVertices.Length,
                bounds = BoundsSummary.From(descriptor.bounds),
                armorSlot = armorSlot,
                mappingBasis = string.IsNullOrEmpty(armorSlot) ? "unmapped_material_name" : "material_name",
            });
        }

        return subMeshes.ToArray();
    }

    private static SlotBuildResult BuildSlotBoneRows(SkinnedMeshRenderer renderer, SubMeshReport[] subMeshes)
    {
        Mesh mesh = renderer.sharedMesh;
        Transform[] bones = renderer.bones ?? Array.Empty<Transform>();
        BoneWeight[] weights = mesh != null ? mesh.boneWeights ?? Array.Empty<BoneWeight>() : Array.Empty<BoneWeight>();
        var result = new SlotBuildResult();

        if (mesh == null || bones.Length == 0 || weights.Length == 0) return result;

        foreach (SubMeshReport subMesh in subMeshes.OrderBy(subMesh => subMesh.index))
        {
            int[] referencedVertices = ReferencedVertices(mesh, subMesh.index, mesh.GetSubMesh(subMesh.index));
            int[] weightedVertexCounts = new int[bones.Length];
            float[] weightMasses = new float[bones.Length];
            float[] maxWeights = new float[bones.Length];
            int invalidInfluenceCount = 0;
            int nonZeroWeightedVertices = 0;
            int maxInfluences = 0;

            foreach (int vertexIndex in referencedVertices)
            {
                if (vertexIndex < 0 || vertexIndex >= weights.Length)
                {
                    invalidInfluenceCount++;
                    continue;
                }

                BoneWeight weight = weights[vertexIndex];
                if (HasAnyWeight(weight)) nonZeroWeightedVertices++;
                maxInfluences = Math.Max(maxInfluences, InfluenceCount(weight));
                AddInfluence(weightedVertexCounts, weightMasses, maxWeights, weight.boneIndex0, weight.weight0, ref invalidInfluenceCount);
                AddInfluence(weightedVertexCounts, weightMasses, maxWeights, weight.boneIndex1, weight.weight1, ref invalidInfluenceCount);
                AddInfluence(weightedVertexCounts, weightMasses, maxWeights, weight.boneIndex2, weight.weight2, ref invalidInfluenceCount);
                AddInfluence(weightedVertexCounts, weightMasses, maxWeights, weight.boneIndex3, weight.weight3, ref invalidInfluenceCount);
            }

            int weightedBoneCount = weightedVertexCounts.Count(count => count > 0);
            result.slotSummaries.Add(new SlotWeightSummary
            {
                armorSlot = subMesh.armorSlot,
                subMeshIndex = subMesh.index,
                materialName = subMesh.materialName,
                descriptorVertexCount = subMesh.descriptorVertexCount,
                uniqueReferencedVertexCount = referencedVertices.Length,
                nonZeroWeightedVertices = nonZeroWeightedVertices,
                weightedBoneCount = weightedBoneCount,
                maxBoneInfluences = maxInfluences,
                invalidInfluenceCount = invalidInfluenceCount,
                runtimeStatus = RuntimeStatus,
            });

            for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
            {
                string bonePath = bones[boneIndex] != null ? TransformPath(bones[boneIndex]) : string.Empty;
                string boneName = BoneName(bonePath);
                result.rows.Add(new SourceSlotBoneWeightRow
                {
                    armorSlot = subMesh.armorSlot,
                    subMeshIndex = subMesh.index,
                    materialName = subMesh.materialName,
                    sourceBoneIndex = boneIndex,
                    sourceBoneRaw = boneName,
                    sourceBonePath = bonePath,
                    policyFamily = PolicyFamily(boneName),
                    descriptorVertexCount = subMesh.descriptorVertexCount,
                    uniqueReferencedVertexCount = referencedVertices.Length,
                    weightedVertexCount = weightedVertexCounts[boneIndex],
                    weightedVertexRatio = referencedVertices.Length == 0 ? 0f : (float)weightedVertexCounts[boneIndex] / referencedVertices.Length,
                    weightMass = weightMasses[boneIndex],
                    maxWeight = maxWeights[boneIndex],
                    zeroWeightForSlot = weightedVertexCounts[boneIndex] == 0,
                    runtimeStatus = RuntimeStatus,
                });
            }
        }

        return result;
    }

    private static PolicyFamilySummary[] BuildPolicyFamilySummaries(SourceSlotBoneWeightRow[] rows)
    {
        return rows
            .GroupBy(row => row.armorSlot + "|" + row.subMeshIndex.ToString(CultureInfo.InvariantCulture) + "|" + row.policyFamily)
            .Select(group =>
            {
                SourceSlotBoneWeightRow first = group.First();
                return new PolicyFamilySummary
                {
                    armorSlot = first.armorSlot,
                    subMeshIndex = first.subMeshIndex,
                    policyFamily = first.policyFamily,
                    sourceBoneRows = group.Count(),
                    weightedBoneCount = group.Count(row => row.weightedVertexCount > 0),
                    totalWeightedVertexCount = group.Sum(row => row.weightedVertexCount),
                    totalWeightMass = group.Sum(row => row.weightMass),
                    weightedBoneNames = group
                        .Where(row => row.weightedVertexCount > 0)
                        .OrderByDescending(row => row.weightedVertexCount)
                        .ThenBy(row => row.sourceBoneRaw, StringComparer.Ordinal)
                        .Select(row => row.sourceBoneRaw + ":" + row.weightedVertexCount.ToString(CultureInfo.InvariantCulture))
                        .ToArray(),
                    runtimeStatus = RuntimeStatus,
                };
            })
            .OrderBy(summary => summary.armorSlot, StringComparer.Ordinal)
            .ThenBy(summary => summary.subMeshIndex)
            .ThenBy(summary => summary.policyFamily, StringComparer.Ordinal)
            .ToArray();
    }

    private static IkZeroWeightProof[] BuildIkZeroWeightProofs(SourceSlotBoneWeightRow[] rows)
    {
        return rows
            .Where(row => string.Equals(row.policyFamily, "ik", StringComparison.Ordinal))
            .GroupBy(row => row.armorSlot + "|" + row.subMeshIndex.ToString(CultureInfo.InvariantCulture))
            .Select(group =>
            {
                SourceSlotBoneWeightRow first = group.First();
                int weightedIkBones = group.Count(row => row.weightedVertexCount > 0);
                int totalWeightedVertices = group.Sum(row => row.weightedVertexCount);
                return new IkZeroWeightProof
                {
                    armorSlot = first.armorSlot,
                    subMeshIndex = first.subMeshIndex,
                    ikBoneCount = group.Count(),
                    weightedIkBoneCount = weightedIkBones,
                    totalWeightedVertexCount = totalWeightedVertices,
                    allIkBonesZeroWeight = weightedIkBones == 0 && totalWeightedVertices == 0,
                    proofStatus = weightedIkBones == 0 && totalWeightedVertices == 0
                        ? "zero_weight_proven_for_slot"
                        : "ik_weighted_exclusion_not_allowed_for_slot",
                    weightedIkBoneNames = group
                        .Where(row => row.weightedVertexCount > 0)
                        .OrderByDescending(row => row.weightedVertexCount)
                        .ThenBy(row => row.sourceBoneRaw, StringComparer.Ordinal)
                        .Select(row => row.sourceBoneRaw + ":" + row.weightedVertexCount.ToString(CultureInfo.InvariantCulture))
                        .ToArray(),
                    runtimeStatus = RuntimeStatus,
                };
            })
            .OrderBy(proof => proof.armorSlot, StringComparer.Ordinal)
            .ThenBy(proof => proof.subMeshIndex)
            .ToArray();
    }

    private static CollapseEvidenceSummary[] BuildCollapseEvidenceSummaries(SourceSlotBoneWeightRow[] rows)
    {
        return rows
            .Where(row => CollapseEvidenceFamilies.Contains(row.policyFamily, StringComparer.Ordinal))
            .GroupBy(row => row.armorSlot + "|" + row.subMeshIndex.ToString(CultureInfo.InvariantCulture) + "|" + row.policyFamily)
            .Select(group =>
            {
                SourceSlotBoneWeightRow first = group.First();
                int weightedBones = group.Count(row => row.weightedVertexCount > 0);
                int totalWeightedVertices = group.Sum(row => row.weightedVertexCount);
                return new CollapseEvidenceSummary
                {
                    armorSlot = first.armorSlot,
                    subMeshIndex = first.subMeshIndex,
                    policyFamily = first.policyFamily,
                    sourceBoneRows = group.Count(),
                    weightedBoneCount = weightedBones,
                    totalWeightedVertexCount = totalWeightedVertices,
                    weightedBoneNames = group
                        .Where(row => row.weightedVertexCount > 0)
                        .OrderByDescending(row => row.weightedVertexCount)
                        .ThenBy(row => row.sourceBoneRaw, StringComparer.Ordinal)
                        .Select(row => row.sourceBoneRaw + ":" + row.weightedVertexCount.ToString(CultureInfo.InvariantCulture))
                        .ToArray(),
                    evidenceStatus = weightedBones == 0
                        ? "zero_weight_for_slot"
                        : "weight_evidence_only_deformation_validation_required",
                    runtimeStatus = RuntimeStatus,
                };
            })
            .OrderBy(summary => summary.armorSlot, StringComparer.Ordinal)
            .ThenBy(summary => summary.subMeshIndex)
            .ThenBy(summary => summary.policyFamily, StringComparer.Ordinal)
            .ToArray();
    }

    private static void CompleteReport(WeightReport report, List<string> extractionBlockers)
    {
        report.extractionBlockers = extractionBlockers
            .Distinct(StringComparer.Ordinal)
            .OrderBy(reason => reason, StringComparer.Ordinal)
            .ToArray();
        report.remainingConversionBlockers = BuildRemainingConversionBlockers(report);
        report.requiredFieldResults = BuildRequiredFieldResults(report);
        report.notRun = NotRun();
        report.marker = report.extractionBlockers.Length == 0 ? ReadyMarker : BlockedMarker;
    }

    private static RequiredFieldResult[] BuildRequiredFieldResults(WeightReport report)
    {
        var rows = new List<RequiredFieldResult>
        {
            Field("a2k_t8_authorization", "pass", "Current user instruction approved A2K-T8 source per-slot weighted-bone reporting only; conversion, sidecars, equip, and save writes excluded."),
            Field("source_identity", SourceIdentityPass(report) ? "pass" : "blocked", "Expected SHA-256 " + report.expectedSha256 + "; actual SHA-256 " + report.actualSha256 + "."),
            Field("isolated_unity_source_import", report.extractionBlockers.Contains("imported_fbx_game_object_missing", StringComparer.Ordinal) ? "blocked" : "pass", "Importer used isolated project path " + report.projectRoot + " and temporary import root " + ImportRoot + "."),
            Field("combined_renderer_slot_map", report.selectedRenderer.subMeshes.Length >= RequiredArmorSlots.Length ? "pass" : "blocked", "Selected renderer " + report.selectedRenderer.name + " emitted " + report.selectedRenderer.subMeshes.Length.ToString(CultureInfo.InvariantCulture) + " submesh rows."),
            Field("source_slot_bone_rows", report.sourceSlotBoneRows.Length > 0 ? "pass" : "blocked", report.sourceSlotBoneRows.Length.ToString(CultureInfo.InvariantCulture) + " rows emitted by armor slot, submesh, and source bone."),
            Field("ik_zero_weight_proof_rows", report.ikZeroWeightProofs.Length > 0 ? "pass" : "blocked", report.ikZeroWeightProofs.Length.ToString(CultureInfo.InvariantCulture) + " IK proof rows emitted; this report does not approve exclusion."),
            Field("collapse_family_weight_evidence", report.collapseEvidenceSummaries.Length > 0 ? "pass" : "blocked", report.collapseEvidenceSummaries.Length.ToString(CultureInfo.InvariantCulture) + " collapse-family evidence rows emitted; deformation validation remains a later gate."),
            Field("conversion_status", "blocked", "A2K-T8 is source evidence only; no map application, conversion, sidecar, equip, or save write is authorized."),
        };

        return rows.ToArray();
    }

    private static string[] BuildRemainingConversionBlockers(WeightReport report)
    {
        var blockers = new List<string>
        {
            "candidate_map_not_approved_for_application",
            "native_kandra_mesh_contract_unresolved",
            "native_material_contract_unresolved",
            "target_bind_pose_policy_unresolved",
            "deformation_validation_not_run",
            "waist_cloth_no_native_target_row_and_merge_rejection_deferred",
            "conversion_implementation_still_blocked",
        };

        if (report.ikZeroWeightProofs.Any(proof => !proof.allIkBonesZeroWeight))
        {
            blockers.Add("ik_exclusion_not_allowed_where_weighted");
        }

        if (report.collapseEvidenceSummaries.Any(summary => summary.weightedBoneCount > 0))
        {
            blockers.Add("collapse_families_require_explicit_policy_and_deformation_validation");
        }

        return blockers
            .Distinct(StringComparer.Ordinal)
            .OrderBy(reason => reason, StringComparer.Ordinal)
            .ToArray();
    }

    private static string[] NotRun()
    {
        return new[]
        {
            "bone_map_application",
            "conversion",
            "kandra_sidecar_generation",
            "assetbundle_addressables_prefab_material_or_controller_build",
            "game_launch",
            "installed_game_content_access",
            "live_foa_folder_write",
            "runtime_loader",
            "item_armor_clothing_inventory_or_equip_registration",
            "save_read_or_write_for_mutation",
            "dynamic_cloth_or_cape_conversion",
            "weapon_work",
            "public_api_or_release",
        };
    }

    private static RequiredFieldResult Field(string field, string status, string evidence)
    {
        return new RequiredFieldResult
        {
            field = field,
            status = status,
            evidence = evidence,
        };
    }

    private static bool SourceIdentityPass(WeightReport report)
    {
        if (string.IsNullOrWhiteSpace(report.expectedSha256)) return !string.IsNullOrWhiteSpace(report.actualSha256);
        return string.Equals(report.expectedSha256, report.actualSha256, StringComparison.OrdinalIgnoreCase);
    }

    private static SkinnedMeshRenderer SelectCombinedRenderer(SkinnedMeshRenderer[] renderers)
    {
        return renderers
            .Where(IsCompleteCombinedRendererCandidate)
            .OrderByDescending(renderer => renderer.sharedMesh.vertexCount)
            .ThenBy(renderer => renderer.name, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static bool IsCompleteCombinedRendererCandidate(SkinnedMeshRenderer renderer)
    {
        Mesh mesh = renderer.sharedMesh;
        if (mesh == null) return false;
        Transform[] bones = renderer.bones ?? Array.Empty<Transform>();
        Material[] materials = renderer.sharedMaterials ?? Array.Empty<Material>();
        BoneWeight[] weights = mesh.boneWeights ?? Array.Empty<BoneWeight>();
        if (renderer.rootBone == null) return false;
        if (bones.Length <= 0) return false;
        if ((mesh.bindposes ?? Array.Empty<Matrix4x4>()).Length <= 0) return false;
        if (weights.Length <= 0 || weights.All(weight => !HasAnyWeight(weight))) return false;
        if (mesh.subMeshCount <= 0) return false;
        if (materials.Length < mesh.subMeshCount) return false;

        string[] slots = Enumerable.Range(0, mesh.subMeshCount)
            .Select(index => index < materials.Length && materials[index] != null ? ArmorSlotFromMaterialName(materials[index].name) : string.Empty)
            .Where(slot => !string.IsNullOrWhiteSpace(slot))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return RequiredArmorSlots.All(slot => slots.Contains(slot, StringComparer.Ordinal));
    }

    private static void AddInfluence(
        int[] weightedVertexCounts,
        float[] weightMasses,
        float[] maxWeights,
        int boneIndex,
        float weight,
        ref int invalidInfluenceCount)
    {
        if (weight <= 0f) return;
        if (boneIndex < 0 || boneIndex >= weightedVertexCounts.Length)
        {
            invalidInfluenceCount++;
            return;
        }

        weightedVertexCounts[boneIndex]++;
        weightMasses[boneIndex] += weight;
        if (weight > maxWeights[boneIndex]) maxWeights[boneIndex] = weight;
    }

    private static int[] ReferencedVertices(Mesh mesh, int subMeshIndex, SubMeshDescriptor descriptor)
    {
        try
        {
            int[] indices = mesh.GetIndices(subMeshIndex, true);
            if (indices.Length > 0)
            {
                return indices
                    .Where(index => index >= 0 && index < mesh.vertexCount)
                    .Distinct()
                    .OrderBy(index => index)
                    .ToArray();
            }
        }
        catch
        {
            // Fall back to the descriptor range when Unity cannot materialize indices.
        }

        int start = Math.Max(0, descriptor.firstVertex);
        int count = Math.Max(0, descriptor.vertexCount);
        int end = Math.Min(mesh.vertexCount, start + count);
        return Enumerable.Range(start, Math.Max(0, end - start)).ToArray();
    }

    private static string ArmorSlotFromMaterialName(string materialName)
    {
        if (materialName.IndexOf("ArmMAT", StringComparison.OrdinalIgnoreCase) >= 0) return "arms";
        if (materialName.IndexOf("BodyMAT", StringComparison.OrdinalIgnoreCase) >= 0) return "torso";
        if (materialName.IndexOf("ClothMAT", StringComparison.OrdinalIgnoreCase) >= 0) return "waist_cloth";
        if (materialName.IndexOf("LegMAT", StringComparison.OrdinalIgnoreCase) >= 0) return "legs";
        return string.Empty;
    }

    private static string PolicyFamily(string boneName)
    {
        string normalized = NormalizeBoneName(boneName);
        if (normalized.StartsWith("ik_", StringComparison.Ordinal) || normalized.Contains("_ik_")) return "ik";
        if (normalized.Contains("twist")) return "twist";
        if (normalized == "ball_l" || normalized == "ball_r" || normalized.StartsWith("ball_", StringComparison.Ordinal)) return "ball";
        if (normalized == "spine_04" || normalized == "spine_05") return "extra_spine";
        if (normalized == "neck_02") return "extra_neck";
        if (normalized.Contains("metacarpal")) return "metacarpal";
        if (normalized.Contains("thumb") || normalized.Contains("index") || normalized.Contains("middle") || normalized.Contains("ring") || normalized.Contains("pinky"))
        {
            return "finger_chain";
        }

        return "other";
    }

    private static string NormalizeBoneName(string boneName)
    {
        string value = boneName ?? string.Empty;
        int colon = value.LastIndexOf(':');
        if (colon >= 0 && colon < value.Length - 1) value = value.Substring(colon + 1);
        return value.Trim().ToLowerInvariant();
    }

    private static string BoneName(string path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;
        int index = path.LastIndexOf('/');
        return index >= 0 && index < path.Length - 1 ? path.Substring(index + 1) : path;
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

    private static void WriteReport(string reportPath, WeightReport report)
    {
        if (string.IsNullOrWhiteSpace(reportPath)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? ".");
        File.WriteAllText(reportPath, JsonUtility.ToJson(report, true), new UTF8Encoding(false));
    }

    private sealed class SlotBuildResult
    {
        public readonly List<SourceSlotBoneWeightRow> rows = new List<SourceSlotBoneWeightRow>();
        public readonly List<SlotWeightSummary> slotSummaries = new List<SlotWeightSummary>();
    }

    [Serializable]
    private sealed class WeightReport
    {
        public string marker = string.Empty;
        public string unityVersion = string.Empty;
        public string projectRoot = string.Empty;
        public string sourceFbxPath = string.Empty;
        public string expectedSha256 = string.Empty;
        public string actualSha256 = string.Empty;
        public string importedAssetPath = string.Empty;
        public string runtimeStatus = string.Empty;
        public int transformCount;
        public int skinnedRendererCount;
        public string[] allSkinnedRendererNames = Array.Empty<string>();
        public RendererReport selectedRenderer = new RendererReport();
        public SlotWeightSummary[] slotSummaries = Array.Empty<SlotWeightSummary>();
        public SourceSlotBoneWeightRow[] sourceSlotBoneRows = Array.Empty<SourceSlotBoneWeightRow>();
        public PolicyFamilySummary[] policyFamilySummaries = Array.Empty<PolicyFamilySummary>();
        public IkZeroWeightProof[] ikZeroWeightProofs = Array.Empty<IkZeroWeightProof>();
        public CollapseEvidenceSummary[] collapseEvidenceSummaries = Array.Empty<CollapseEvidenceSummary>();
        public RequiredFieldResult[] requiredFieldResults = Array.Empty<RequiredFieldResult>();
        public string[] extractionBlockers = Array.Empty<string>();
        public string[] remainingConversionBlockers = Array.Empty<string>();
        public string[] notRun = Array.Empty<string>();
        public string[] errors = Array.Empty<string>();
    }

    [Serializable]
    private sealed class RendererReport
    {
        public string name = string.Empty;
        public string transformPath = string.Empty;
        public string rootBone = string.Empty;
        public string meshName = string.Empty;
        public bool meshMissing;
        public int vertexCount;
        public int subMeshCount;
        public int boneCount;
        public int bindPoseCount;
        public string bindPoseHash = string.Empty;
        public int boneWeightCount;
        public int nonZeroWeightedVertices;
        public int maxBoneInfluences;
        public int materialSlotCount;
        public string runtimeStatus = string.Empty;
        public string[] bonePaths = Array.Empty<string>();
        public BoundsSummary meshBounds = new BoundsSummary();
        public BoundsSummary rendererLocalBounds = new BoundsSummary();
        public MaterialSlot[] materialSlots = Array.Empty<MaterialSlot>();
        public SubMeshReport[] subMeshes = Array.Empty<SubMeshReport>();
    }

    [Serializable]
    private sealed class MaterialSlot
    {
        public int index;
        public string name = string.Empty;
        public string shader = string.Empty;
    }

    [Serializable]
    private sealed class SubMeshReport
    {
        public int index;
        public int materialSlotIndex;
        public string materialName = string.Empty;
        public string materialShader = string.Empty;
        public string topology = string.Empty;
        public int indexStart;
        public int indexCount;
        public int baseVertex;
        public int firstVertex;
        public int descriptorVertexCount;
        public int uniqueReferencedVertexCount;
        public BoundsSummary bounds = new BoundsSummary();
        public string armorSlot = string.Empty;
        public string mappingBasis = string.Empty;
    }

    [Serializable]
    private sealed class SlotWeightSummary
    {
        public string armorSlot = string.Empty;
        public int subMeshIndex;
        public string materialName = string.Empty;
        public int descriptorVertexCount;
        public int uniqueReferencedVertexCount;
        public int nonZeroWeightedVertices;
        public int weightedBoneCount;
        public int maxBoneInfluences;
        public int invalidInfluenceCount;
        public string runtimeStatus = string.Empty;
    }

    [Serializable]
    private sealed class SourceSlotBoneWeightRow
    {
        public string armorSlot = string.Empty;
        public int subMeshIndex;
        public string materialName = string.Empty;
        public int sourceBoneIndex;
        public string sourceBoneRaw = string.Empty;
        public string sourceBonePath = string.Empty;
        public string policyFamily = string.Empty;
        public int descriptorVertexCount;
        public int uniqueReferencedVertexCount;
        public int weightedVertexCount;
        public float weightedVertexRatio;
        public float weightMass;
        public float maxWeight;
        public bool zeroWeightForSlot;
        public string runtimeStatus = string.Empty;
    }

    [Serializable]
    private sealed class PolicyFamilySummary
    {
        public string armorSlot = string.Empty;
        public int subMeshIndex;
        public string policyFamily = string.Empty;
        public int sourceBoneRows;
        public int weightedBoneCount;
        public int totalWeightedVertexCount;
        public float totalWeightMass;
        public string[] weightedBoneNames = Array.Empty<string>();
        public string runtimeStatus = string.Empty;
    }

    [Serializable]
    private sealed class IkZeroWeightProof
    {
        public string armorSlot = string.Empty;
        public int subMeshIndex;
        public int ikBoneCount;
        public int weightedIkBoneCount;
        public int totalWeightedVertexCount;
        public bool allIkBonesZeroWeight;
        public string proofStatus = string.Empty;
        public string[] weightedIkBoneNames = Array.Empty<string>();
        public string runtimeStatus = string.Empty;
    }

    [Serializable]
    private sealed class CollapseEvidenceSummary
    {
        public string armorSlot = string.Empty;
        public int subMeshIndex;
        public string policyFamily = string.Empty;
        public int sourceBoneRows;
        public int weightedBoneCount;
        public int totalWeightedVertexCount;
        public string[] weightedBoneNames = Array.Empty<string>();
        public string evidenceStatus = string.Empty;
        public string runtimeStatus = string.Empty;
    }

    [Serializable]
    private sealed class RequiredFieldResult
    {
        public string field = string.Empty;
        public string status = string.Empty;
        public string evidence = string.Empty;
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
}
