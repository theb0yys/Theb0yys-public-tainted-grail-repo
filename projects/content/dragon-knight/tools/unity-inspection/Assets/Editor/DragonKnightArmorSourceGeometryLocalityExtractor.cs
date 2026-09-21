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

public static class DragonKnightArmorSourceGeometryLocalityExtractor
{
    private const string ReadyMarker = "DRAGON_KNIGHT_ARMOR_A2K_T20_SOURCE_GEOMETRY_LOCALITY_VISUAL_PROOF_PACKET_READY_APPLICATION_BLOCKED";
    private const string BlockedMarker = "DRAGON_KNIGHT_ARMOR_A2K_T20_SOURCE_GEOMETRY_LOCALITY_VISUAL_PROOF_PACKET_BLOCKED";
    private const string ImportRoot = "Assets/DragonKnightArmorA2KT20";
    private const string ImportedFbxPath = ImportRoot + "/SK_Dragon_knight_UE5_no_Cape.fbx";
    private const string RuntimeStatus = "blocked_source_geometry_locality_visual_or_semantic_proof_only";

    private static readonly RequiredProofRow[] RequiredRows =
    {
        new RequiredProofRow("female", "torso", "torso", "Cuirass", 3, "NewNamespace1_BodyMAT", "neck_02", "extra_neck", 297, 0.129189312458038f),
        new RequiredProofRow("female", "torso", "torso", "Cuirass", 3, "NewNamespace1_BodyMAT", "spine_04", "extra_spine", 9449, 1f),
        new RequiredProofRow("female", "torso", "torso", "Cuirass", 3, "NewNamespace1_BodyMAT", "spine_05", "extra_spine", 526, 0.108052380383015f),
        new RequiredProofRow("male", "torso", "torso", "Cuirass", 3, "NewNamespace1_BodyMAT", "neck_02", "extra_neck", 297, 0.129189312458038f),
        new RequiredProofRow("male", "torso", "torso", "Cuirass", 3, "NewNamespace1_BodyMAT", "spine_04", "extra_spine", 9449, 1f),
        new RequiredProofRow("male", "torso", "torso", "Cuirass", 3, "NewNamespace1_BodyMAT", "spine_05", "extra_spine", 526, 0.108052380383015f),
    };

    public static void Extract()
    {
        string reportPath = GetArgument("-dragonKnightArmorLocalityReport", string.Empty);
        var extractionBlockers = new List<string>();
        var report = new LocalityReport
        {
            marker = BlockedMarker,
            unityVersion = Application.unityVersion,
            projectRoot = ProjectRoot,
            importedAssetPath = ImportedFbxPath,
            importRoot = ImportRoot,
            runtimeStatus = RuntimeStatus,
            sourceGeometryLocalityExtractorRun = true,
        };

        try
        {
            string sourceFbxPath = GetArgument("-dragonKnightArmorSourceFbxPath", string.Empty);
            string expectedSha256 = GetArgument("-dragonKnightArmorSourceSha256", string.Empty, false);

            if (string.IsNullOrWhiteSpace(reportPath)) throw new ArgumentException("Missing -dragonKnightArmorLocalityReport.");
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
                PopulateLocalityReport(report, sourceRootObject, extractionBlockers);
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

    private static void PopulateLocalityReport(LocalityReport report, GameObject sourceRootObject, List<string> extractionBlockers)
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
        SubMeshReport torsoSubMesh = report.selectedRenderer.subMeshes
            .FirstOrDefault(subMesh =>
                subMesh.index == 3
                && string.Equals(subMesh.armorSlot, "torso", StringComparison.Ordinal)
                && subMesh.materialName.IndexOf("BodyMAT", StringComparison.OrdinalIgnoreCase) >= 0);

        if (torsoSubMesh == null)
        {
            extractionBlockers.Add("torso_bodymat_submesh_3_missing");
            return;
        }

        report.torsoMaterialRegion = torsoSubMesh;
        report.proofRows = BuildProofRows(selectedRenderer, torsoSubMesh, extractionBlockers);
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
                referencedVertexIndexRanges = CompressRanges(referencedVertices),
                bounds = BoundsSummary.From(descriptor.bounds),
                armorSlot = armorSlot,
                mappingBasis = string.IsNullOrEmpty(armorSlot) ? "unmapped_material_name" : "documented_a1_t8_material_slot_map",
                runtimeStatus = RuntimeStatus,
            });
        }

        return subMeshes.ToArray();
    }

    private static ProofRow[] BuildProofRows(SkinnedMeshRenderer renderer, SubMeshReport torsoSubMesh, List<string> extractionBlockers)
    {
        Mesh mesh = renderer.sharedMesh;
        Transform[] bones = renderer.bones ?? Array.Empty<Transform>();
        BoneWeight[] weights = mesh != null ? mesh.boneWeights ?? Array.Empty<BoneWeight>() : Array.Empty<BoneWeight>();
        Vector3[] vertices = mesh != null ? mesh.vertices ?? Array.Empty<Vector3>() : Array.Empty<Vector3>();
        var rows = new List<ProofRow>();

        if (mesh == null || bones.Length == 0 || weights.Length == 0 || vertices.Length == 0)
        {
            extractionBlockers.Add("selected_renderer_mesh_weight_or_vertex_data_missing");
            return rows.ToArray();
        }

        TriangleRow[] allTriangles = BuildTriangles(mesh, torsoSubMesh.index, mesh.GetSubMesh(torsoSubMesh.index), extractionBlockers);
        int[] torsoReferencedVertices = ReferencedVertices(mesh, torsoSubMesh.index, mesh.GetSubMesh(torsoSubMesh.index));
        bool materialRegionPresent = string.Equals(torsoSubMesh.armorSlot, "torso", StringComparison.Ordinal)
            && torsoSubMesh.materialName.IndexOf("BodyMAT", StringComparison.OrdinalIgnoreCase) >= 0;

        foreach (RequiredProofRow required in RequiredRows)
        {
            int boneIndex = FindBoneIndex(bones, required.sourceBoneRaw);
            var weightedVertices = new List<int>();
            float weightMass = 0f;
            float maxWeight = 0f;

            if (boneIndex >= 0)
            {
                foreach (int vertexIndex in torsoReferencedVertices)
                {
                    if (vertexIndex < 0 || vertexIndex >= weights.Length) continue;
                    float influence = InfluenceWeight(weights[vertexIndex], boneIndex);
                    if (influence <= 0f) continue;
                    weightedVertices.Add(vertexIndex);
                    weightMass += influence;
                    if (influence > maxWeight) maxWeight = influence;
                }
            }

            int[] weightedVertexArray = weightedVertices
                .Distinct()
                .OrderBy(index => index)
                .ToArray();
            var weightedVertexSet = new HashSet<int>(weightedVertexArray);
            TriangleRow[] weightedTriangles = allTriangles
                .Where(triangle => triangle.vertex0 >= 0
                    && (weightedVertexSet.Contains(triangle.vertex0)
                        || weightedVertexSet.Contains(triangle.vertex1)
                        || weightedVertexSet.Contains(triangle.vertex2)))
                .OrderBy(triangle => triangle.ordinal)
                .ToArray();
            int[] weightedTriangleOrdinals = weightedTriangles
                .Select(triangle => triangle.ordinal)
                .ToArray();
            int[] weightedTriangleVertices = weightedTriangles
                .SelectMany(triangle => new[] { triangle.vertex0, triangle.vertex1, triangle.vertex2 })
                .Where(index => index >= 0)
                .Distinct()
                .OrderBy(index => index)
                .ToArray();

            bool sourceVertexEvidence = weightedVertexArray.Length > 0;
            bool sourceTriangleEvidence = weightedTriangles.Length > 0;
            bool expectedCountMatches = weightedVertexArray.Length == required.expectedWeightedVertexCount;
            bool expectedMaxMatches = Math.Abs(maxWeight - required.expectedMaxWeight) <= 0.000001f;
            bool localityProven = boneIndex >= 0
                && sourceVertexEvidence
                && sourceTriangleEvidence
                && materialRegionPresent
                && expectedCountMatches;
            bool semanticClassificationPresent = materialRegionPresent && localityProven;

            var rowBlockers = new List<string>();
            if (boneIndex < 0) rowBlockers.Add("source_bone_missing");
            if (!sourceVertexEvidence) rowBlockers.Add("source_vertex_evidence_absent");
            if (!sourceTriangleEvidence) rowBlockers.Add("source_triangle_evidence_absent");
            if (!materialRegionPresent) rowBlockers.Add("source_material_region_evidence_absent");
            if (!expectedCountMatches) rowBlockers.Add("a2k_t19_weighted_vertex_count_mismatch");
            if (!expectedMaxMatches) rowBlockers.Add("a2k_t19_max_weight_mismatch");
            rowBlockers.Add("removal_retopology_approval_source_absent");
            rowBlockers.Add("omission_safety_not_proven");
            rowBlockers.Add("candidate_map_application_not_authorized");

            rows.Add(new ProofRow
            {
                sourceSlot = required.sourceSlot,
                targetGender = required.targetGender,
                targetSlot = required.targetSlot,
                nativeEquipmentType = required.nativeEquipmentType,
                submeshIndex = torsoSubMesh.index,
                materialName = torsoSubMesh.materialName,
                materialSlotIndex = torsoSubMesh.materialSlotIndex,
                sourceBoneRaw = required.sourceBoneRaw,
                sourceBonePath = boneIndex >= 0 && boneIndex < bones.Length && bones[boneIndex] != null ? TransformPath(bones[boneIndex]) : string.Empty,
                sourceBoneIndex = boneIndex,
                policyFamily = required.policyFamily,
                expectedWeightedVertexCount = required.expectedWeightedVertexCount,
                weightedVertexCount = weightedVertexArray.Length,
                expectedWeightedVertexCountMatches = expectedCountMatches,
                expectedMaxWeight = required.expectedMaxWeight,
                maxWeight = maxWeight,
                expectedMaxWeightMatches = expectedMaxMatches,
                weightMass = weightMass,
                weightedVertexRatio = torsoReferencedVertices.Length == 0 ? 0f : (float)weightedVertexArray.Length / torsoReferencedVertices.Length,
                weightedTriangleCount = weightedTriangles.Length,
                materialRegionTriangleCount = allTriangles.Length,
                materialRegionUniqueReferencedVertexCount = torsoReferencedVertices.Length,
                weightedVertexIndexRanges = CompressRanges(weightedVertexArray),
                weightedVertexIndexSample = Sample(weightedVertexArray, 32),
                weightedTriangleOrdinalRanges = CompressRanges(weightedTriangleOrdinals),
                weightedTriangleOrdinalSample = Sample(weightedTriangleOrdinals, 32),
                weightedTriangleVertexIndexRanges = CompressRanges(weightedTriangleVertices),
                weightedTriangleVertexIndexSample = Sample(weightedTriangleVertices, 32),
                weightedVertexBounds = BoundsSummary.From(vertices, weightedVertexArray),
                weightedTriangleBounds = BoundsSummary.From(vertices, weightedTriangleVertices),
                sourceVertexEvidencePresent = sourceVertexEvidence,
                sourceTriangleEvidencePresent = sourceTriangleEvidence,
                sourceMaterialRegionEvidencePresent = materialRegionPresent,
                visualProofPresent = false,
                semanticClassificationPresent = semanticClassificationPresent,
                sourceGeometryLocalityProven = localityProven,
                requiredArmorGeometryOwnershipProven = false,
                removableGeometryOwnershipProven = false,
                retopologyInputOwnershipProven = false,
                omissionSafetyProven = false,
                removalRetopologyApprovalPresent = false,
                candidateMapApplicationAllowed = false,
                conversionAllowed = false,
                proofClassification = semanticClassificationPresent
                    ? "semantic_torso_bodymat_material_region_locality_proven_still_deferred_for_omission_or_retopology"
                    : "unclassified_still_deferred",
                semanticClassificationBasis = semanticClassificationPresent
                    ? "Documented A1/T8 combined-renderer material-slot map identifies submesh 3 BodyMAT as torso armor; this packet records exact source vertex and triangle locality but does not prove omission safety."
                    : "Semantic classification unavailable because required locality or material-region evidence is missing.",
                t19Disposition = "remain_deferred",
                t20Disposition = localityProven
                    ? "remain_deferred_source_locality_proven_removal_retopology_approval_absent"
                    : "remain_deferred_source_locality_not_proven",
                blockedReason = string.Join(";", rowBlockers.Distinct(StringComparer.Ordinal).OrderBy(reason => reason, StringComparer.Ordinal)),
                proofRequirement = "Direct source vertex, triangle, material-region, and visual or semantic proof are required before any omission, retopology, or candidate-map application can be proposed.",
                evidenceCitation = EvidenceCitation(required),
                runtimeStatus = RuntimeStatus,
            });
        }

        return rows.ToArray();
    }

    private static TriangleRow[] BuildTriangles(Mesh mesh, int subMeshIndex, SubMeshDescriptor descriptor, List<string> extractionBlockers)
    {
        if (descriptor.topology != MeshTopology.Triangles)
        {
            extractionBlockers.Add("torso_bodymat_submesh_topology_not_triangles");
            return Array.Empty<TriangleRow>();
        }

        int[] indices = Array.Empty<int>();
        try
        {
            indices = mesh.GetIndices(subMeshIndex, true);
        }
        catch
        {
            extractionBlockers.Add("torso_bodymat_submesh_indices_unavailable");
        }

        if (indices.Length == 0)
        {
            extractionBlockers.Add("torso_bodymat_submesh_indices_empty");
            return Array.Empty<TriangleRow>();
        }

        if (indices.Length % 3 != 0)
        {
            extractionBlockers.Add("torso_bodymat_submesh_index_count_not_divisible_by_three");
        }

        var triangles = new List<TriangleRow>();
        for (int i = 0; i + 2 < indices.Length; i += 3)
        {
            triangles.Add(new TriangleRow
            {
                ordinal = i / 3,
                vertex0 = ValidVertexIndex(indices[i], mesh.vertexCount),
                vertex1 = ValidVertexIndex(indices[i + 1], mesh.vertexCount),
                vertex2 = ValidVertexIndex(indices[i + 2], mesh.vertexCount),
            });
        }

        return triangles.ToArray();
    }

    private static void CompleteReport(LocalityReport report, List<string> extractionBlockers)
    {
        report.extractionBlockers = extractionBlockers
            .Distinct(StringComparer.Ordinal)
            .OrderBy(reason => reason, StringComparer.Ordinal)
            .ToArray();

        bool sourceIdentityPass = SourceIdentityPass(report);
        bool rowInventoryPass = report.proofRows.Length == RequiredRows.Length;
        bool localityRowsPass = rowInventoryPass && report.proofRows.All(row => row.sourceGeometryLocalityProven);
        bool semanticRowsPass = rowInventoryPass && report.proofRows.All(row => row.semanticClassificationPresent);
        bool countMatches = rowInventoryPass && report.proofRows.All(row => row.expectedWeightedVertexCountMatches);
        bool maxMatches = rowInventoryPass && report.proofRows.All(row => row.expectedMaxWeightMatches);
        bool extractionPass = sourceIdentityPass
            && rowInventoryPass
            && localityRowsPass
            && semanticRowsPass
            && countMatches
            && maxMatches
            && report.extractionBlockers.Length == 0;

        report.sourceGeometryLocalityProven = localityRowsPass;
        report.visualProofPacketProduced = false;
        report.semanticProofPacketProduced = semanticRowsPass;
        report.visualOrSemanticProofPacketProduced = semanticRowsPass;
        report.sourceAssetMutationAllowed = false;
        report.retopologyAllowed = false;
        report.deformationValidationExecutionAllowed = false;
        report.candidateMapApplicationAllowed = false;
        report.conversionAllowed = false;
        report.sidecarGenerationAllowed = false;
        report.inventoryEquipSaveMutationAllowed = false;
        report.marker = extractionPass ? ReadyMarker : BlockedMarker;
        report.evidenceSummary = BuildEvidenceSummary(report, sourceIdentityPass, rowInventoryPass, localityRowsPass, semanticRowsPass, countMatches, maxMatches);
        report.requiredFieldResults = BuildRequiredFieldResults(report, sourceIdentityPass, rowInventoryPass, localityRowsPass, semanticRowsPass, countMatches, maxMatches);
        report.blockedReasons = BuildBlockedReasons(report, extractionPass);
        report.nextRequiredEvidence = BuildNextRequiredEvidence();
        report.notRun = NotRun();
    }

    private static EvidenceSummary BuildEvidenceSummary(
        LocalityReport report,
        bool sourceIdentityPass,
        bool rowInventoryPass,
        bool localityRowsPass,
        bool semanticRowsPass,
        bool countMatches,
        bool maxMatches)
    {
        return new EvidenceSummary
        {
            sourceIdentityPass = sourceIdentityPass,
            userSuppliedUnityProjectRoutePresent = !string.IsNullOrWhiteSpace(report.projectRoot),
            sourceGeometryLocalityExtractorRun = report.sourceGeometryLocalityExtractorRun,
            selectedRendererPresent = !string.IsNullOrWhiteSpace(report.selectedRenderer.name),
            torsoMaterialRegionPresent = !string.IsNullOrWhiteSpace(report.torsoMaterialRegion.materialName),
            proofRows = report.proofRows.Length,
            rowInventoryPass = rowInventoryPass,
            expectedWeightedVertexCountsMatch = countMatches,
            expectedMaxWeightsMatch = maxMatches,
            sourceVertexEvidenceRows = report.proofRows.Count(row => row.sourceVertexEvidencePresent),
            sourceTriangleEvidenceRows = report.proofRows.Count(row => row.sourceTriangleEvidencePresent),
            sourceMaterialRegionEvidenceRows = report.proofRows.Count(row => row.sourceMaterialRegionEvidencePresent),
            sourceGeometryLocalityRowsProven = report.proofRows.Count(row => row.sourceGeometryLocalityProven),
            visualProofRows = report.proofRows.Count(row => row.visualProofPresent),
            semanticClassificationRows = report.proofRows.Count(row => row.semanticClassificationPresent),
            visualOrSemanticProofRows = report.proofRows.Count(row => row.visualProofPresent || row.semanticClassificationPresent),
            semanticRowsPass = semanticRowsPass,
            sourceGeometryLocalityProven = localityRowsPass,
            omissionSafetyProven = report.proofRows.Length > 0 && report.proofRows.All(row => row.omissionSafetyProven),
            removalRetopologyApprovalRows = report.proofRows.Count(row => row.removalRetopologyApprovalPresent),
            allRowsRemainDeferred = report.proofRows.Length > 0 && report.proofRows.All(row => row.t20Disposition.StartsWith("remain_deferred", StringComparison.Ordinal)),
            candidateMapApplicationAllowed = false,
            conversionAllowed = false,
        };
    }

    private static RequiredFieldResult[] BuildRequiredFieldResults(
        LocalityReport report,
        bool sourceIdentityPass,
        bool rowInventoryPass,
        bool localityRowsPass,
        bool semanticRowsPass,
        bool countMatches,
        bool maxMatches)
    {
        var rows = new List<RequiredFieldResult>
        {
            Field("source_identity", sourceIdentityPass ? "pass" : "blocked", "Expected SHA-256 " + report.expectedSha256 + "; actual SHA-256 " + report.actualSha256 + "."),
            Field("current_task_unity_project_route", string.IsNullOrWhiteSpace(report.projectRoot) ? "blocked" : "pass", "Unity project route used: " + report.projectRoot + "."),
            Field("bounded_source_import", report.extractionBlockers.Contains("imported_fbx_game_object_missing", StringComparer.Ordinal) ? "blocked" : "pass", "Imported a copied source FBX under " + ImportRoot + " inside the supplied Unity project."),
            Field("combined_renderer_selection", string.IsNullOrWhiteSpace(report.selectedRenderer.name) ? "blocked" : "pass", "Selected renderer " + report.selectedRenderer.name + " with mesh " + report.selectedRenderer.meshName + "."),
            Field("torso_bodymat_material_region", string.IsNullOrWhiteSpace(report.torsoMaterialRegion.materialName) ? "blocked" : "pass", "Submesh " + report.torsoMaterialRegion.index.ToString(CultureInfo.InvariantCulture) + " material " + report.torsoMaterialRegion.materialName + " maps to " + report.torsoMaterialRegion.armorSlot + "."),
            Field("a2k_t20_row_inventory", rowInventoryPass ? "pass" : "blocked", report.proofRows.Length.ToString(CultureInfo.InvariantCulture) + " proof rows emitted; expected " + RequiredRows.Length.ToString(CultureInfo.InvariantCulture) + "."),
            Field("a2k_t19_weighted_vertex_count_match", countMatches ? "pass" : "blocked", "Extractor row counts match A2K-T19 expected weighted vertex counts: " + countMatches.ToString(CultureInfo.InvariantCulture) + "."),
            Field("a2k_t19_max_weight_match", maxMatches ? "pass" : "blocked", "Extractor row max weights match A2K-T19 expected max weights within tolerance: " + maxMatches.ToString(CultureInfo.InvariantCulture) + "."),
            Field("source_vertex_triangle_material_region_evidence", localityRowsPass ? "pass" : "blocked", report.proofRows.Count(row => row.sourceGeometryLocalityProven).ToString(CultureInfo.InvariantCulture) + " rows have exact source vertex, triangle, and material-region locality evidence."),
            Field("visual_or_semantic_proof", semanticRowsPass ? "pass" : "blocked", "Semantic proof rows: " + report.proofRows.Count(row => row.semanticClassificationPresent).ToString(CultureInfo.InvariantCulture) + "; rendered screenshot proof rows: " + report.proofRows.Count(row => row.visualProofPresent).ToString(CultureInfo.InvariantCulture) + "."),
            Field("removal_retopology_approval_source", "blocked", "No approval source exists for removal, omission, or retopology of any weighted blocker row."),
            Field("updated_disposition_table", rowInventoryPass ? "pass" : "blocked", "Every T20 row remains deferred after locality proof because omission safety and removal/retopology approval are absent."),
            Field("candidate_map_application", "blocked", "Weighted blocker rows remain deferred; candidate-map application is not authorized."),
            Field("conversion_status", "blocked", "Conversion, sidecars, equip, and save writes remain blocked."),
        };

        return rows.ToArray();
    }

    private static string[] BuildBlockedReasons(LocalityReport report, bool extractionPass)
    {
        var blockers = new List<string>();
        blockers.AddRange(report.extractionBlockers);
        if (!extractionPass)
        {
            blockers.Add("source_geometry_locality_or_semantic_proof_incomplete");
        }

        blockers.Add("omission_safety_not_proven");
        blockers.Add("removal_retopology_approval_source_absent");
        blockers.Add("all_weighted_blocker_rows_remain_deferred_after_locality_proof");
        blockers.Add("candidate_map_application_not_authorized");
        blockers.Add("conversion_implementation_still_blocked");

        return blockers
            .Distinct(StringComparer.Ordinal)
            .OrderBy(reason => reason, StringComparer.Ordinal)
            .ToArray();
    }

    private static NextRequiredEvidence[] BuildNextRequiredEvidence()
    {
        return new[]
        {
            new NextRequiredEvidence
            {
                gate = "A2K-T21 explicit omission rejection, retopology approval, or deformation-validation route for locality-proven torso weighted blockers",
                mustProduce = new[]
                {
                    "explicit approval source if any source vertex, triangle, or material region is to be removed, omitted, or retopologized",
                    "or an approved no-write deformation-validation route that can keep required torso geometry while resolving spine_04, spine_05, and neck_02 target handling",
                    "updated non-deferred disposition rows for all six weighted blockers, or a cited reason they must remain blocked",
                    "continued block on candidate-map application and conversion until every weighted blocker row has supported disposition or approved deformation-validation route",
                },
            },
        };
    }

    private static string[] NotRun()
    {
        return new[]
        {
            "rendered_visual_screenshot_capture",
            "source_mesh_editing",
            "retopology",
            "source_asset_mutation",
            "deformation_validation_execution",
            "candidate_map_application",
            "bone_map_application",
            "conversion",
            "kandra_sidecar_generation",
            "assetbundle_or_addressables_build",
            "body_mask_or_culling_mutation",
            "waist_cloth_merge_or_rejection_application",
            "game_launch",
            "runtime_loader",
            "item_armor_clothing_registration",
            "inventory_or_equip",
            "save_read_or_write_for_mutation",
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

    private static string EvidenceCitation(RequiredProofRow row)
    {
        string t19Line;
        if (row.targetGender == "female" && row.sourceBoneRaw == "neck_02") t19Line = "67";
        else if (row.targetGender == "female" && row.sourceBoneRaw == "spine_04") t19Line = "68";
        else if (row.targetGender == "female" && row.sourceBoneRaw == "spine_05") t19Line = "69";
        else if (row.targetGender == "male" && row.sourceBoneRaw == "neck_02") t19Line = "70";
        else if (row.targetGender == "male" && row.sourceBoneRaw == "spine_04") t19Line = "71";
        else t19Line = "72";

        string t8Line = row.sourceBoneRaw == "spine_04" ? "138" : row.sourceBoneRaw == "spine_05" ? "147" : "148";
        string familyLine = row.policyFamily == "extra_neck" ? "86" : "87";
        return "docs/research/frameworks/dragon-knight-armor-iron-no-cape-a2k-t19-source-geometry-locality-omission-retopology-proof-route-decision-2026-08-06.md:39;"
            + " docs/research/frameworks/dragon-knight-armor-iron-no-cape-a2k-t19-source-geometry-locality-omission-retopology-proof-route-decision-2026-08-06.md:" + t19Line + ";"
            + " docs/research/frameworks/dragon-knight-armor-iron-no-cape-a2k-t19-source-geometry-locality-omission-retopology-proof-route-decision-2026-08-06.md:103-109;"
            + " docs/research/frameworks/dragon-knight-armor-iron-no-cape-a2k-t8-source-per-slot-weight-report-2026-08-05.md:" + familyLine + ";"
            + " docs/research/frameworks/dragon-knight-armor-iron-no-cape-a2k-t8-source-per-slot-weight-report-2026-08-05.md:" + t8Line + ";"
            + " current user instruction supplied Unity project path <local-path>";
    }

    private static bool SourceIdentityPass(LocalityReport report)
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

        return new[] { "arms", "legs", "torso", "waist_cloth" }.All(slot => slots.Contains(slot, StringComparer.Ordinal));
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

    private static int ValidVertexIndex(int index, int vertexCount)
    {
        return index >= 0 && index < vertexCount ? index : -1;
    }

    private static int FindBoneIndex(Transform[] bones, string sourceBoneRaw)
    {
        string expected = NormalizeBoneName(sourceBoneRaw);
        for (int index = 0; index < bones.Length; index++)
        {
            if (bones[index] == null) continue;
            if (string.Equals(NormalizeBoneName(BoneName(TransformPath(bones[index]))), expected, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    private static float InfluenceWeight(BoneWeight weight, int boneIndex)
    {
        if (weight.weight0 > 0f && weight.boneIndex0 == boneIndex) return weight.weight0;
        if (weight.weight1 > 0f && weight.boneIndex1 == boneIndex) return weight.weight1;
        if (weight.weight2 > 0f && weight.boneIndex2 == boneIndex) return weight.weight2;
        if (weight.weight3 > 0f && weight.boneIndex3 == boneIndex) return weight.weight3;
        return 0f;
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

    private static string[] CompressRanges(IEnumerable<int> values)
    {
        int[] sorted = values
            .Distinct()
            .OrderBy(value => value)
            .ToArray();

        if (sorted.Length == 0) return Array.Empty<string>();

        var ranges = new List<string>();
        int start = sorted[0];
        int previous = sorted[0];
        for (int index = 1; index < sorted.Length; index++)
        {
            int current = sorted[index];
            if (current == previous + 1)
            {
                previous = current;
                continue;
            }

            ranges.Add(FormatRange(start, previous));
            start = current;
            previous = current;
        }

        ranges.Add(FormatRange(start, previous));
        return ranges.ToArray();
    }

    private static string FormatRange(int start, int end)
    {
        return start == end
            ? start.ToString(CultureInfo.InvariantCulture)
            : start.ToString(CultureInfo.InvariantCulture) + "-" + end.ToString(CultureInfo.InvariantCulture);
    }

    private static int[] Sample(int[] values, int count)
    {
        return values.Take(count).ToArray();
    }

    private static string ArmorSlotFromMaterialName(string materialName)
    {
        if (materialName.IndexOf("ArmMAT", StringComparison.OrdinalIgnoreCase) >= 0) return "arms";
        if (materialName.IndexOf("BodyMAT", StringComparison.OrdinalIgnoreCase) >= 0) return "torso";
        if (materialName.IndexOf("ClothMAT", StringComparison.OrdinalIgnoreCase) >= 0) return "waist_cloth";
        if (materialName.IndexOf("LegMAT", StringComparison.OrdinalIgnoreCase) >= 0) return "legs";
        return string.Empty;
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

    private static void WriteReport(string reportPath, LocalityReport report)
    {
        if (string.IsNullOrWhiteSpace(reportPath)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? ".");
        File.WriteAllText(reportPath, JsonUtility.ToJson(report, true), new UTF8Encoding(false));
    }

    private sealed class RequiredProofRow
    {
        public RequiredProofRow(
            string targetGender,
            string sourceSlot,
            string targetSlot,
            string nativeEquipmentType,
            int submeshIndex,
            string materialName,
            string sourceBoneRaw,
            string policyFamily,
            int expectedWeightedVertexCount,
            float expectedMaxWeight)
        {
            this.targetGender = targetGender;
            this.sourceSlot = sourceSlot;
            this.targetSlot = targetSlot;
            this.nativeEquipmentType = nativeEquipmentType;
            this.submeshIndex = submeshIndex;
            this.materialName = materialName;
            this.sourceBoneRaw = sourceBoneRaw;
            this.policyFamily = policyFamily;
            this.expectedWeightedVertexCount = expectedWeightedVertexCount;
            this.expectedMaxWeight = expectedMaxWeight;
        }

        public readonly string targetGender;
        public readonly string sourceSlot;
        public readonly string targetSlot;
        public readonly string nativeEquipmentType;
        public readonly int submeshIndex;
        public readonly string materialName;
        public readonly string sourceBoneRaw;
        public readonly string policyFamily;
        public readonly int expectedWeightedVertexCount;
        public readonly float expectedMaxWeight;
    }

    [Serializable]
    private sealed class LocalityReport
    {
        public string marker = string.Empty;
        public string unityVersion = string.Empty;
        public string projectRoot = string.Empty;
        public string sourceFbxPath = string.Empty;
        public string expectedSha256 = string.Empty;
        public string actualSha256 = string.Empty;
        public string importedAssetPath = string.Empty;
        public string importRoot = string.Empty;
        public string runtimeStatus = string.Empty;
        public bool sourceGeometryLocalityExtractorRun;
        public bool visualProofPacketProduced;
        public bool semanticProofPacketProduced;
        public bool visualOrSemanticProofPacketProduced;
        public bool sourceGeometryLocalityProven;
        public bool sourceAssetMutationAllowed;
        public bool retopologyAllowed;
        public bool deformationValidationExecutionAllowed;
        public bool candidateMapApplicationAllowed;
        public bool conversionAllowed;
        public bool sidecarGenerationAllowed;
        public bool inventoryEquipSaveMutationAllowed;
        public int transformCount;
        public int skinnedRendererCount;
        public string[] allSkinnedRendererNames = Array.Empty<string>();
        public RendererReport selectedRenderer = new RendererReport();
        public SubMeshReport torsoMaterialRegion = new SubMeshReport();
        public EvidenceSummary evidenceSummary = new EvidenceSummary();
        public ProofRow[] proofRows = Array.Empty<ProofRow>();
        public RequiredFieldResult[] requiredFieldResults = Array.Empty<RequiredFieldResult>();
        public string[] extractionBlockers = Array.Empty<string>();
        public string[] blockedReasons = Array.Empty<string>();
        public NextRequiredEvidence[] nextRequiredEvidence = Array.Empty<NextRequiredEvidence>();
        public string[] notRun = Array.Empty<string>();
        public string[] errors = Array.Empty<string>();
    }

    [Serializable]
    private sealed class EvidenceSummary
    {
        public bool sourceIdentityPass;
        public bool userSuppliedUnityProjectRoutePresent;
        public bool sourceGeometryLocalityExtractorRun;
        public bool selectedRendererPresent;
        public bool torsoMaterialRegionPresent;
        public int proofRows;
        public bool rowInventoryPass;
        public bool expectedWeightedVertexCountsMatch;
        public bool expectedMaxWeightsMatch;
        public int sourceVertexEvidenceRows;
        public int sourceTriangleEvidenceRows;
        public int sourceMaterialRegionEvidenceRows;
        public int sourceGeometryLocalityRowsProven;
        public int visualProofRows;
        public int semanticClassificationRows;
        public int visualOrSemanticProofRows;
        public bool semanticRowsPass;
        public bool sourceGeometryLocalityProven;
        public bool omissionSafetyProven;
        public int removalRetopologyApprovalRows;
        public bool allRowsRemainDeferred;
        public bool candidateMapApplicationAllowed;
        public bool conversionAllowed;
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
        public string[] referencedVertexIndexRanges = Array.Empty<string>();
        public BoundsSummary bounds = new BoundsSummary();
        public string armorSlot = string.Empty;
        public string mappingBasis = string.Empty;
        public string runtimeStatus = string.Empty;
    }

    [Serializable]
    private sealed class ProofRow
    {
        public string sourceSlot = string.Empty;
        public string targetGender = string.Empty;
        public string targetSlot = string.Empty;
        public string nativeEquipmentType = string.Empty;
        public int submeshIndex;
        public string materialName = string.Empty;
        public int materialSlotIndex;
        public string sourceBoneRaw = string.Empty;
        public string sourceBonePath = string.Empty;
        public int sourceBoneIndex;
        public string policyFamily = string.Empty;
        public int expectedWeightedVertexCount;
        public int weightedVertexCount;
        public bool expectedWeightedVertexCountMatches;
        public float expectedMaxWeight;
        public float maxWeight;
        public bool expectedMaxWeightMatches;
        public float weightMass;
        public float weightedVertexRatio;
        public int weightedTriangleCount;
        public int materialRegionTriangleCount;
        public int materialRegionUniqueReferencedVertexCount;
        public string[] weightedVertexIndexRanges = Array.Empty<string>();
        public int[] weightedVertexIndexSample = Array.Empty<int>();
        public string[] weightedTriangleOrdinalRanges = Array.Empty<string>();
        public int[] weightedTriangleOrdinalSample = Array.Empty<int>();
        public string[] weightedTriangleVertexIndexRanges = Array.Empty<string>();
        public int[] weightedTriangleVertexIndexSample = Array.Empty<int>();
        public BoundsSummary weightedVertexBounds = new BoundsSummary();
        public BoundsSummary weightedTriangleBounds = new BoundsSummary();
        public bool sourceVertexEvidencePresent;
        public bool sourceTriangleEvidencePresent;
        public bool sourceMaterialRegionEvidencePresent;
        public bool visualProofPresent;
        public bool semanticClassificationPresent;
        public bool sourceGeometryLocalityProven;
        public bool requiredArmorGeometryOwnershipProven;
        public bool removableGeometryOwnershipProven;
        public bool retopologyInputOwnershipProven;
        public bool omissionSafetyProven;
        public bool removalRetopologyApprovalPresent;
        public bool candidateMapApplicationAllowed;
        public bool conversionAllowed;
        public string proofClassification = string.Empty;
        public string semanticClassificationBasis = string.Empty;
        public string t19Disposition = string.Empty;
        public string t20Disposition = string.Empty;
        public string blockedReason = string.Empty;
        public string proofRequirement = string.Empty;
        public string evidenceCitation = string.Empty;
        public string runtimeStatus = string.Empty;
    }

    [Serializable]
    private sealed class TriangleRow
    {
        public int ordinal;
        public int vertex0;
        public int vertex1;
        public int vertex2;
    }

    [Serializable]
    private sealed class RequiredFieldResult
    {
        public string field = string.Empty;
        public string status = string.Empty;
        public string evidence = string.Empty;
    }

    [Serializable]
    private sealed class NextRequiredEvidence
    {
        public string gate = string.Empty;
        public string[] mustProduce = Array.Empty<string>();
    }

    [Serializable]
    private sealed class BoundsSummary
    {
        public bool hasValues;
        public string min = string.Empty;
        public string max = string.Empty;
        public string center = string.Empty;
        public string size = string.Empty;

        public static BoundsSummary From(Bounds bounds)
        {
            return new BoundsSummary
            {
                hasValues = true,
                min = FormatVector(bounds.min),
                max = FormatVector(bounds.max),
                center = FormatVector(bounds.center),
                size = FormatVector(bounds.size),
            };
        }

        public static BoundsSummary From(Vector3[] vertices, int[] indices)
        {
            if (vertices == null || indices == null || indices.Length == 0)
            {
                return new BoundsSummary();
            }

            bool initialized = false;
            Vector3 min = Vector3.zero;
            Vector3 max = Vector3.zero;
            foreach (int index in indices)
            {
                if (index < 0 || index >= vertices.Length) continue;
                Vector3 value = vertices[index];
                if (!initialized)
                {
                    min = value;
                    max = value;
                    initialized = true;
                    continue;
                }

                min = Vector3.Min(min, value);
                max = Vector3.Max(max, value);
            }

            if (!initialized) return new BoundsSummary();

            Vector3 size = max - min;
            Vector3 center = min + size * 0.5f;
            return new BoundsSummary
            {
                hasValues = true,
                min = FormatVector(min),
                max = FormatVector(max),
                center = FormatVector(center),
                size = FormatVector(size),
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
