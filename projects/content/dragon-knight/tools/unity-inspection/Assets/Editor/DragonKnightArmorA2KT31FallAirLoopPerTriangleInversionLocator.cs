using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public static class DragonKnightArmorA2KT31FallAirLoopPerTriangleInversionLocator
{
    private const string ReadyMarker = "DRAGON_KNIGHT_ARMOR_A2K_T31_FALL_AIR_LOOP_PER_TRIANGLE_INVERSION_LOCATOR_READY_EXACT_TRIANGLES_CAPTURED_VISUAL_RUNTIME_UNOBSERVED";
    private const string CountMismatchMarker = "DRAGON_KNIGHT_ARMOR_A2K_T31_FALL_AIR_LOOP_PER_TRIANGLE_INVERSION_LOCATOR_BLOCKED_T29_COUNT_MISMATCH";
    private const string BlockedMarker = "DRAGON_KNIGHT_ARMOR_A2K_T31_FALL_AIR_LOOP_PER_TRIANGLE_INVERSION_LOCATOR_BLOCKED";
    private const string ImportedFbxPath = "Assets/DragonKnightArmorA2KT20/SK_Dragon_knight_UE5_no_Cape.fbx";
    private const string TargetPose = "fall";
    private const string TargetClip = "Anim_Hero_TPP_Base_Knockdown_Air_Loop";
    private const int SourceSubmeshIndex = 3;
    private const float DisplacementEpsilon = 0.000001f;
    private const float TriangleAreaEpsilon = 0.00000001f;

    private static readonly RequiredRow[] RequiredRows =
    {
        new RequiredRow("female", "torso", "Cuirass", "neck_02", "extra_neck", 297, 689),
        new RequiredRow("female", "torso", "Cuirass", "spine_04", "extra_spine", 9449, 16236),
        new RequiredRow("female", "torso", "Cuirass", "spine_05", "extra_spine", 526, 1067),
        new RequiredRow("male", "torso", "Cuirass", "neck_02", "extra_neck", 297, 689),
        new RequiredRow("male", "torso", "Cuirass", "spine_04", "extra_spine", 9449, 16236),
        new RequiredRow("male", "torso", "Cuirass", "spine_05", "extra_spine", 526, 1067),
    };

    public static void Locate()
    {
        string reportPath = GetArgument("-dragonKnightArmorT31Report", string.Empty);
        var report = new LocatorReport
        {
            marker = BlockedMarker,
            generatedAt = "2026-08-06",
            unityVersion = Application.unityVersion,
            projectRoot = ProjectRoot,
            sourceAssetPath = ImportedFbxPath,
            sourceBasis = "tainted_grail_foa_native_only",
            targetPose = TargetPose,
            targetClip = TargetClip,
            candidateMapApplicationExecuted = false,
            conversionExecuted = false,
            sourceFbxMutationExecuted = false,
            unityProjectAssetMutationDuringSampling = false,
        };

        var blockers = new List<string>();
        var required = new List<RequiredFieldResult>();

        try
        {
            string t26Path = GetArgument("-dragonKnightArmorT26Input", string.Empty);
            string t29Path = GetArgument("-dragonKnightArmorT29Input", string.Empty);
            string bundleRoot = GetArgument("-dragonKnightFoaBundleRoot", string.Empty, false);

            if (string.IsNullOrWhiteSpace(reportPath)) throw new ArgumentException("Missing -dragonKnightArmorT31Report.");
            if (string.IsNullOrWhiteSpace(t26Path)) throw new ArgumentException("Missing -dragonKnightArmorT26Input.");
            if (string.IsNullOrWhiteSpace(t29Path)) throw new ArgumentException("Missing -dragonKnightArmorT29Input.");

            T26Record t26 = ReadJson<T26Record>(t26Path);
            T29Record t29 = ReadJson<T29Record>(t29Path);
            report.t26Marker = t26.marker;
            report.t29Marker = t29.marker;
            report.t29TotalInvertedTriangles = t29.summary != null ? t29.summary.totalInvertedTriangles : 0;
            report.t29TotalCollapsedTriangles = t29.summary != null ? t29.summary.totalCollapsedTriangles : 0;

            GameObject sourceAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ImportedFbxPath);
            AddRequired(required, "t26_ready", t26.marker == "DRAGON_KNIGHT_ARMOR_A2K_T26_TAINTED_GRAIL_NATIVE_POSE_DRIVER_SOURCE_PACKET_READY", "T26 marker=" + t26.marker + ".");
            AddRequired(required, "t29_geometry_metrics_ready", t29.marker == "DRAGON_KNIGHT_ARMOR_A2K_T29_NO_WRITE_DEFORMATION_METRIC_SAMPLER_GEOMETRY_METRICS_CAPTURED_VISUAL_METRICS_UNOBSERVED" && report.t29TotalInvertedTriangles == 470, "T29 marker=" + t29.marker + " totalInvertedTriangles=" + report.t29TotalInvertedTriangles.ToString(CultureInfo.InvariantCulture) + ".");
            AddRequired(required, "source_asset_already_imported", sourceAsset != null, "Imported source asset path=" + ImportedFbxPath + ".");
            AddRequired(required, "bundle_root_available", !string.IsNullOrWhiteSpace(bundleRoot) && Directory.Exists(bundleRoot), "FoA bundle root=" + bundleRoot + ".");

            PoseDriverRow fallPose = (t26.poseDriverRows ?? Array.Empty<PoseDriverRow>())
                .FirstOrDefault(row => row != null && string.Equals(row.pose, TargetPose, StringComparison.Ordinal));
            AddRequired(required, "fall_air_loop_driver_available", fallPose != null && (fallPose.matchedDriverNames ?? Array.Empty<string>()).Contains(TargetClip), "T26 fall matched drivers=" + string.Join("; ", fallPose != null ? fallPose.matchedDriverNames ?? Array.Empty<string>() : Array.Empty<string>()) + ".");

            if (sourceAsset == null) blockers.Add("source_asset_not_already_imported_from_t20");
            if (string.IsNullOrWhiteSpace(bundleRoot) || !Directory.Exists(bundleRoot)) blockers.Add("foa_bundle_root_missing");
            if (fallPose == null || !(fallPose.matchedDriverNames ?? Array.Empty<string>()).Contains(TargetClip)) blockers.Add("fall_air_loop_driver_missing");

            if (blockers.Count == 0)
            {
                ExecuteLocator(sourceAsset, fallPose, bundleRoot, report, required, blockers);
            }

            report.requiredFieldResults = required.ToArray();
            report.blockedReasons = blockers.Distinct(StringComparer.Ordinal).ToArray();
            CompleteMarker(report);
            WriteReport(reportPath, report);
            Debug.Log(report.marker + ": " + reportPath);
            EditorApplication.Exit(report.marker == ReadyMarker ? 0 : 1);
        }
        catch (Exception ex)
        {
            blockers.Add("locator_exception");
            report.errors = new[] { ex.ToString() };
            report.requiredFieldResults = required.ToArray();
            report.blockedReasons = blockers.Distinct(StringComparer.Ordinal).ToArray();
            CompleteMarker(report);
            Debug.LogError(report.marker + ": " + ex);
            WriteReport(reportPath, report);
            EditorApplication.Exit(1);
        }
    }

    private static void ExecuteLocator(GameObject sourceAsset, PoseDriverRow fallPose, string bundleRoot, LocatorReport report, List<RequiredFieldResult> required, List<string> blockers)
    {
        GameObject instance = UnityEngine.Object.Instantiate(sourceAsset);
        instance.name = "DragonKnightArmorA2KT31NoWritePerTriangleLocator";
        instance.hideFlags = HideFlags.HideAndDontSave;

        AssetBundle retainedBundle = null;
        try
        {
            SkinnedMeshRenderer renderer = SelectCombinedRenderer(instance.GetComponentsInChildren<SkinnedMeshRenderer>(true));
            if (renderer == null || renderer.sharedMesh == null)
            {
                blockers.Add("combined_source_renderer_missing");
                AddRequired(required, "combined_source_renderer", false, "No source SkinnedMeshRenderer with submesh 3 was found.");
                return;
            }

            Mesh sourceMesh = renderer.sharedMesh;
            AvatarBuildResult avatarResult = ResolveSourceAvatar(instance);
            Animator animator = instance.GetComponentInChildren<Animator>();
            if (animator == null) animator = instance.AddComponent<Animator>();
            Avatar sourceAvatar = avatarResult.avatar;
            if (sourceAvatar != null) animator.avatar = sourceAvatar;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            Transform[] bones = renderer.bones ?? Array.Empty<Transform>();
            BoneWeight[] boneWeights = sourceMesh.boneWeights ?? Array.Empty<BoneWeight>();
            Vector3[] sourceVertices = sourceMesh.vertices ?? Array.Empty<Vector3>();
            int[] sourceTriangles = sourceMesh.GetTriangles(SourceSubmeshIndex);
            var sourcePaths = new HashSet<string>(instance.GetComponentsInChildren<Transform>(true).Select(transform => RelativeTransformPath(instance.transform, transform)), StringComparer.Ordinal);
            TransformState[] baselinePose = TransformState.Capture(instance);
            Mesh baselineMesh = new Mesh();
            renderer.BakeMesh(baselineMesh, true);
            Vector3[] baselineVertices = baselineMesh.vertices;

            report.selectedRenderer = renderer.name;
            report.sourceVertexCount = sourceMesh.vertexCount;
            report.sourceSubmeshIndex = SourceSubmeshIndex;
            report.sourceSubmeshTriangleCount = sourceTriangles.Length / 3;
            report.sourceBoneCount = bones.Length;
            report.sourceBoneWeightCount = boneWeights.Length;
            report.sourceAvatarName = sourceAvatar != null ? sourceAvatar.name : string.Empty;
            report.sourceAvatarBuildMode = avatarResult.mode;
            report.sourceAvatarIsValid = sourceAvatar != null && sourceAvatar.isValid;
            report.sourceAvatarIsHuman = sourceAvatar != null && sourceAvatar.isHuman;
            report.sourceAvatarHumanBoneCount = avatarResult.humanBoneCount;
            report.sourceAvatarMissingHumanBoneNames = avatarResult.missingHumanBoneNames;
            report.animatorPresent = animator != null;

            AddRequired(required, "combined_source_renderer", sourceMesh.vertexCount > 0 && sourceMesh.subMeshCount > SourceSubmeshIndex, "Renderer=" + renderer.name + " vertices=" + sourceMesh.vertexCount.ToString(CultureInfo.InvariantCulture) + " submeshes=" + sourceMesh.subMeshCount.ToString(CultureInfo.InvariantCulture) + ".");
            AddRequired(required, "source_avatar_available", sourceAvatar != null && sourceAvatar.isValid && sourceAvatar.isHuman, "Mode=" + avatarResult.mode + " Avatar=" + (sourceAvatar != null ? sourceAvatar.name : string.Empty) + " valid=" + (sourceAvatar != null && sourceAvatar.isValid).ToString(CultureInfo.InvariantCulture) + " human=" + (sourceAvatar != null && sourceAvatar.isHuman).ToString(CultureInfo.InvariantCulture) + " mappedBones=" + avatarResult.humanBoneCount.ToString(CultureInfo.InvariantCulture) + ".");

            LoadedClip loadedClip = LoadTargetClip(fallPose, bundleRoot, out retainedBundle, blockers);
            AddRequired(required, "target_clip_loaded", loadedClip != null && loadedClip.clip != null, "Target clip=" + TargetClip + " bundlePath=" + (loadedClip != null ? loadedClip.bundlePath : string.Empty) + ".");
            if (loadedClip == null || loadedClip.clip == null) return;

            var weightedRows = RequiredRows.Select(row => BuildWeightedData(row, bones, boneWeights, sourceVertices, sourceTriangles, baselineVertices)).ToArray();
            AddRequired(required, "weighted_source_rows_resolved", weightedRows.All(row => row.weightedVertices.Length == row.required.expectedWeightedVertexCount && row.triangles.Length == row.required.expectedWeightedTriangleCount), "Resolved weighted rows=" + weightedRows.Count(row => row.weightedVertices.Length == row.required.expectedWeightedVertexCount && row.triangles.Length == row.required.expectedWeightedTriangleCount).ToString(CultureInfo.InvariantCulture) + "/6.");

            var rows = new List<RowLocalizationReport>();
            foreach (WeightedRowData weighted in weightedRows)
            {
                rows.Add(LocateRow(instance, renderer, animator, baselinePose, baselineVertices, weighted, loadedClip, sourcePaths));
            }

            report.rowLocalizations = rows.ToArray();
            report.summary = new LocatorSummary
            {
                targetedPose = TargetPose,
                targetedClip = TargetClip,
                rowLocalizationCount = rows.Count,
                exactTriangleRowsCaptured = rows.Count(row => row.exactInvertedTriangleOrdinalsCaptured),
                totalInvertedTriangles = rows.Sum(row => row.invertedTriangleCount),
                totalCollapsedTriangles = rows.Sum(row => row.collapsedTriangleCount),
                totalExactTriangleRecords = rows.Sum(row => row.invertedTriangles != null ? row.invertedTriangles.Length : 0),
                maxVertexDisplacement = rows.Count == 0 ? 0f : rows.Max(row => row.maxVertexDisplacement),
                rowsWithClipBindingApplied = rows.Count(row => row.clipBindingApplied),
                rowsWithoutClipBindingApplied = rows.Count(row => !row.clipBindingApplied),
                t29TotalInvertedTriangles = report.t29TotalInvertedTriangles,
                t29TotalCollapsedTriangles = report.t29TotalCollapsedTriangles,
                exactTriangleCaptureMatchesT29Aggregate = rows.Sum(row => row.invertedTriangleCount) == report.t29TotalInvertedTriangles,
                dynamicDeformedMeshSamplingExecuted = rows.Count > 0,
                visualRuntimeMetricsObserved = false,
                candidateMapApplicationExecuted = false,
                conversionExecuted = false,
            };

            if (report.summary.rowLocalizationCount != 6) blockers.Add("row_localization_count_not_6");
            if (report.summary.exactTriangleRowsCaptured != 6) blockers.Add("not_all_rows_have_exact_inverted_triangle_records");
            if (!report.summary.exactTriangleCaptureMatchesT29Aggregate) blockers.Add("t31_exact_triangle_total_does_not_match_t29_aggregate");
            if (report.summary.rowsWithClipBindingApplied != 6) blockers.Add("target_clip_bindings_not_applied_to_all_rows");
            blockers.Add("visual_runtime_metrics_unobserved");

            UnityEngine.Object.DestroyImmediate(baselineMesh);
        }
        finally
        {
            if (retainedBundle != null) retainedBundle.Unload(false);
            UnityEngine.Object.DestroyImmediate(instance);
        }
    }

    private static RowLocalizationReport LocateRow(
        GameObject instance,
        SkinnedMeshRenderer renderer,
        Animator animator,
        TransformState[] baselinePose,
        Vector3[] baselineVertices,
        WeightedRowData weighted,
        LoadedClip loaded,
        HashSet<string> sourcePaths)
    {
        float sampleTime = loaded.clip.length <= 0f ? 0f : Mathf.Min(loaded.clip.length * 0.5f, Mathf.Max(0f, loaded.clip.length - 0.001f));
        SampleEvaluation evaluation = EvaluateClip(instance, renderer, animator, baselinePose, baselineVertices, weighted, loaded.clip, sampleTime, sourcePaths);
        RowMetric metric = evaluation.metric;
        BindingReport binding = evaluation.binding;

        return new RowLocalizationReport
        {
            targetGender = weighted.required.targetGender,
            targetSlot = weighted.required.targetSlot,
            nativeEquipmentType = weighted.required.nativeEquipmentType,
            pose = TargetPose,
            clip = loaded.clip.name,
            sourceBoneRaw = weighted.required.sourceBoneRaw,
            policyFamily = weighted.required.policyFamily,
            sourceSubmeshIndex = SourceSubmeshIndex,
            weightedVertexCount = weighted.weightedVertices.Length,
            weightedTriangleCount = weighted.triangles.Length,
            sampleTime = sampleTime,
            clipLength = loaded.clip.length,
            bundlePath = loaded.bundlePath,
            evaluationMode = evaluation.evaluationMode,
            curveBindingCount = binding.curveBindingCount,
            matchingBindingPaths = binding.matchingBindingPaths,
            changedTransformCount = binding.changedTransformCount,
            clipBindingApplied = binding.changedTransformCount > 0 || metric.maxVertexDisplacement > DisplacementEpsilon,
            geometryMetricsCaptured = weighted.weightedVertices.Length > 0 && weighted.triangles.Length > 0,
            visualRuntimeMetricsObserved = false,
            maxVertexDisplacement = metric.maxVertexDisplacement,
            meanVertexDisplacement = metric.meanVertexDisplacement,
            movedVertexCount = metric.movedVertexCount,
            collapsedTriangleCount = metric.collapsedTriangleCount,
            invertedTriangleCount = metric.invertedTriangleCount,
            exactInvertedTriangleOrdinalsCaptured = metric.invertedTriangles != null && metric.invertedTriangles.Length == metric.invertedTriangleCount,
            poseBoundsCenter = Vec3.From(metric.poseBoundsInitialized ? metric.poseBounds.center : Vector3.zero),
            poseBoundsSize = Vec3.From(metric.poseBoundsInitialized ? metric.poseBounds.size : Vector3.zero),
            invertedTriangles = metric.invertedTriangles,
            rowStatus = metric.invertedTriangles != null && metric.invertedTriangles.Length == metric.invertedTriangleCount
                ? "exact_inverted_triangles_captured"
                : "blocked_exact_inverted_triangles_not_captured",
        };
    }

    private static SampleEvaluation EvaluateClip(
        GameObject instance,
        SkinnedMeshRenderer renderer,
        Animator animator,
        TransformState[] baselinePose,
        Vector3[] baselineVertices,
        WeightedRowData weighted,
        AnimationClip clip,
        float sampleTime,
        HashSet<string> sourcePaths)
    {
        TransformState.Restore(baselinePose);
        TransformState[] directBefore = TransformState.Capture(instance);
        clip.SampleAnimation(instance, sampleTime);
        TransformState[] directAfter = TransformState.Capture(instance);
        RowMetric directMetric = BakeAndMeasure(renderer, weighted, baselineVertices);
        BindingReport directBinding = InspectBindings(clip, sourcePaths, directBefore, directAfter);
        if (directBinding.changedTransformCount > 0 || directMetric.maxVertexDisplacement > DisplacementEpsilon || animator == null || animator.avatar == null)
        {
            return new SampleEvaluation
            {
                evaluationMode = "AnimationClip.SampleAnimation",
                metric = directMetric,
                binding = directBinding,
            };
        }

        TransformState.Restore(baselinePose);
        animator.Rebind();
        animator.Update(0f);
        TransformState.Restore(baselinePose);
        TransformState[] playableBefore = TransformState.Capture(instance);
        EvaluatePlayable(animator, clip, sampleTime);
        TransformState[] playableAfter = TransformState.Capture(instance);

        return new SampleEvaluation
        {
            evaluationMode = "AnimationClipPlayable",
            metric = BakeAndMeasure(renderer, weighted, baselineVertices),
            binding = InspectBindings(clip, sourcePaths, playableBefore, playableAfter),
        };
    }

    private static void EvaluatePlayable(Animator animator, AnimationClip clip, float sampleTime)
    {
        PlayableGraph graph = PlayableGraph.Create("DragonKnightArmorA2KT31FallAirLoopSample");
        try
        {
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, clip);
            playable.SetApplyFootIK(true);
            playable.SetApplyPlayableIK(false);
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "DragonKnightArmorA2KT31Output", animator);
            output.SetSourcePlayable(playable);
            graph.Play();
            graph.Evaluate(Mathf.Max(0.00001f, sampleTime));
        }
        finally
        {
            if (graph.IsValid()) graph.Destroy();
        }
    }

    private static RowMetric BakeAndMeasure(SkinnedMeshRenderer renderer, WeightedRowData weighted, Vector3[] baselineVertices)
    {
        Mesh baked = new Mesh();
        try
        {
            renderer.BakeMesh(baked, true);
            return Measure(weighted, baselineVertices, baked.vertices);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(baked);
        }
    }

    private static RowMetric Measure(WeightedRowData weighted, Vector3[] baselineVertices, Vector3[] posedVertices)
    {
        var metric = new RowMetric();
        if (baselineVertices == null || posedVertices == null || baselineVertices.Length != posedVertices.Length)
        {
            metric.invertedTriangles = Array.Empty<InvertedTriangleReport>();
            return metric;
        }

        float sum = 0f;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        bool initialized = false;

        foreach (int vertexIndex in weighted.weightedVertices)
        {
            if (vertexIndex < 0 || vertexIndex >= baselineVertices.Length) continue;
            float displacement = Vector3.Distance(baselineVertices[vertexIndex], posedVertices[vertexIndex]);
            sum += displacement;
            if (displacement > metric.maxVertexDisplacement) metric.maxVertexDisplacement = displacement;
            if (displacement > DisplacementEpsilon) metric.movedVertexCount++;
            if (!initialized)
            {
                bounds = new Bounds(posedVertices[vertexIndex], Vector3.zero);
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(posedVertices[vertexIndex]);
            }
        }

        metric.meanVertexDisplacement = weighted.weightedVertices.Length == 0 ? 0f : sum / weighted.weightedVertices.Length;
        metric.poseBounds = bounds;
        metric.poseBoundsInitialized = initialized;

        var inverted = new List<InvertedTriangleReport>();
        foreach (TriangleData triangle in weighted.triangles)
        {
            if (triangle.a < 0 || triangle.b < 0 || triangle.c < 0 || triangle.a >= posedVertices.Length || triangle.b >= posedVertices.Length || triangle.c >= posedVertices.Length) continue;
            Vector3 pa = posedVertices[triangle.a];
            Vector3 pb = posedVertices[triangle.b];
            Vector3 pc = posedVertices[triangle.c];
            Vector3 posedNormal = Vector3.Cross(pb - pa, pc - pa);
            float posedArea = posedNormal.magnitude * 0.5f;
            if (posedArea <= TriangleAreaEpsilon) metric.collapsedTriangleCount++;
            if (triangle.baselineNormal.sqrMagnitude > 0f && posedNormal.sqrMagnitude > 0f)
            {
                float normalizedDot = Vector3.Dot(triangle.baselineNormal.normalized, posedNormal.normalized);
                if (normalizedDot < 0f)
                {
                    metric.invertedTriangleCount++;
                    inverted.Add(new InvertedTriangleReport
                    {
                        sourceSubmeshIndex = SourceSubmeshIndex,
                        sourceTriangleOrdinal = triangle.sourceTriangleOrdinal,
                        vertexIndex0 = triangle.a,
                        vertexIndex1 = triangle.b,
                        vertexIndex2 = triangle.c,
                        weightedVertexCountInTriangle = triangle.weightedVertexCountInTriangle,
                        sourceBoneWeightAtVertex0 = triangle.weight0,
                        sourceBoneWeightAtVertex1 = triangle.weight1,
                        sourceBoneWeightAtVertex2 = triangle.weight2,
                        sourceBoneWeightMass = triangle.weightMass,
                        baselineArea = triangle.baselineArea,
                        posedArea = posedArea,
                        normalizedNormalDot = normalizedDot,
                        baselineCentroid = Vec3.From(triangle.baselineCentroid),
                        posedCentroid = Vec3.From((pa + pb + pc) / 3f),
                        baselineNormal = Vec3.From(triangle.baselineNormal),
                        posedNormal = Vec3.From(posedNormal),
                        vertex0Displacement = Vector3.Distance(baselineVertices[triangle.a], pa),
                        vertex1Displacement = Vector3.Distance(baselineVertices[triangle.b], pb),
                        vertex2Displacement = Vector3.Distance(baselineVertices[triangle.c], pc),
                    });
                }
            }
        }

        metric.invertedTriangles = inverted.ToArray();
        return metric;
    }

    private static BindingReport InspectBindings(AnimationClip clip, HashSet<string> sourcePaths, TransformState[] before, TransformState[] after)
    {
        var report = new BindingReport();
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
        report.curveBindingCount = bindings.Length;
        report.matchingBindingPaths = bindings
            .Select(binding => binding.path ?? string.Empty)
            .Distinct(StringComparer.Ordinal)
            .Count(path => sourcePaths.Contains(path));
        report.changedTransformCount = TransformState.CountChanged(before, after);
        return report;
    }

    private static LoadedClip LoadTargetClip(PoseDriverRow fallPose, string bundleRoot, out AssetBundle retainedBundle, List<string> blockers)
    {
        retainedBundle = null;
        ClipRecord record = (fallPose.clipRecords ?? Array.Empty<ClipRecord>())
            .FirstOrDefault(item => item != null && string.Equals(item.clip, TargetClip, StringComparison.Ordinal));
        if (record == null)
        {
            blockers.Add("target_clip_record_missing");
            return null;
        }

        string bundlePath = !string.IsNullOrWhiteSpace(record.bundlePath)
            ? record.bundlePath
            : Path.Combine(bundleRoot, record.bundle ?? string.Empty);
        if (string.IsNullOrWhiteSpace(bundlePath) || !File.Exists(bundlePath))
        {
            blockers.Add("target_clip_bundle_missing");
            return null;
        }

        retainedBundle = AssetBundle.LoadFromFile(bundlePath);
        if (retainedBundle == null)
        {
            blockers.Add("target_clip_bundle_load_failed");
            return null;
        }

        AnimationClip clip = retainedBundle.LoadAsset<AnimationClip>(record.clip);
        if (clip == null)
        {
            AnimationClip[] allClips = retainedBundle.LoadAllAssets<AnimationClip>();
            clip = allClips.FirstOrDefault(candidate => candidate != null && string.Equals(candidate.name, record.clip, StringComparison.Ordinal));
        }
        if (clip == null)
        {
            blockers.Add("target_clip_asset_missing");
            return null;
        }

        return new LoadedClip { clip = clip, bundlePath = bundlePath };
    }

    private static WeightedRowData BuildWeightedData(RequiredRow required, Transform[] bones, BoneWeight[] boneWeights, Vector3[] vertices, int[] sourceTriangles, Vector3[] baselineVertices)
    {
        int boneIndex = Array.FindIndex(bones, bone => bone != null && string.Equals(bone.name, required.sourceBoneRaw, StringComparison.Ordinal));
        var weightedVertices = new List<int>();
        var submeshVertices = new HashSet<int>(sourceTriangles ?? Array.Empty<int>());
        if (boneIndex >= 0)
        {
            foreach (int index in submeshVertices.OrderBy(value => value))
            {
                if (index < 0 || index >= boneWeights.Length) continue;
                if (InfluenceWeight(boneWeights[index], boneIndex) > 0f)
                {
                    weightedVertices.Add(index);
                }
            }
        }

        var weightedSet = new HashSet<int>(weightedVertices);
        var triangles = new List<TriangleData>();
        for (int index = 0; index + 2 < sourceTriangles.Length; index += 3)
        {
            int a = sourceTriangles[index];
            int b = sourceTriangles[index + 1];
            int c = sourceTriangles[index + 2];
            if (!weightedSet.Contains(a) && !weightedSet.Contains(b) && !weightedSet.Contains(c)) continue;
            Vector3 normal = Vector3.zero;
            Vector3 centroid = Vector3.zero;
            float baselineArea = 0f;
            if (baselineVertices != null && a >= 0 && b >= 0 && c >= 0 && a < baselineVertices.Length && b < baselineVertices.Length && c < baselineVertices.Length)
            {
                normal = Vector3.Cross(baselineVertices[b] - baselineVertices[a], baselineVertices[c] - baselineVertices[a]);
                centroid = (baselineVertices[a] + baselineVertices[b] + baselineVertices[c]) / 3f;
                baselineArea = normal.magnitude * 0.5f;
            }
            float weightA = a >= 0 && a < boneWeights.Length ? InfluenceWeight(boneWeights[a], boneIndex) : 0f;
            float weightB = b >= 0 && b < boneWeights.Length ? InfluenceWeight(boneWeights[b], boneIndex) : 0f;
            float weightC = c >= 0 && c < boneWeights.Length ? InfluenceWeight(boneWeights[c], boneIndex) : 0f;
            triangles.Add(new TriangleData
            {
                sourceTriangleOrdinal = index / 3,
                a = a,
                b = b,
                c = c,
                baselineNormal = normal,
                baselineArea = baselineArea,
                baselineCentroid = centroid,
                weightedVertexCountInTriangle = (weightA > 0f ? 1 : 0) + (weightB > 0f ? 1 : 0) + (weightC > 0f ? 1 : 0),
                weight0 = weightA,
                weight1 = weightB,
                weight2 = weightC,
                weightMass = weightA + weightB + weightC,
            });
        }

        return new WeightedRowData
        {
            required = required,
            boneIndex = boneIndex,
            weightedVertices = weightedVertices.ToArray(),
            triangles = triangles.ToArray(),
        };
    }

    private static float InfluenceWeight(BoneWeight weight, int boneIndex)
    {
        float total = 0f;
        if (weight.boneIndex0 == boneIndex) total += weight.weight0;
        if (weight.boneIndex1 == boneIndex) total += weight.weight1;
        if (weight.boneIndex2 == boneIndex) total += weight.weight2;
        if (weight.boneIndex3 == boneIndex) total += weight.weight3;
        return total;
    }

    private static SkinnedMeshRenderer SelectCombinedRenderer(SkinnedMeshRenderer[] renderers)
    {
        return (renderers ?? Array.Empty<SkinnedMeshRenderer>())
            .Where(renderer => renderer != null && renderer.sharedMesh != null && renderer.sharedMesh.subMeshCount > SourceSubmeshIndex)
            .OrderByDescending(renderer => renderer.sharedMesh.vertexCount)
            .ThenBy(renderer => renderer.name, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static void CompleteMarker(LocatorReport report)
    {
        if (report.summary != null
            && report.summary.rowLocalizationCount == 6
            && report.summary.exactTriangleRowsCaptured == 6
            && report.summary.totalExactTriangleRecords == report.summary.totalInvertedTriangles
            && report.summary.totalInvertedTriangles > 0
            && report.summary.exactTriangleCaptureMatchesT29Aggregate)
        {
            report.marker = ReadyMarker;
            return;
        }
        if (report.summary != null
            && report.summary.totalExactTriangleRecords == report.summary.totalInvertedTriangles
            && report.summary.totalInvertedTriangles != report.t29TotalInvertedTriangles)
        {
            report.marker = CountMismatchMarker;
            return;
        }
        report.marker = BlockedMarker;
    }

    private static void AddRequired(List<RequiredFieldResult> rows, string field, bool passed, string evidence)
    {
        rows.Add(new RequiredFieldResult { field = field, status = passed ? "pass" : "blocked", evidence = evidence });
    }

    private static T ReadJson<T>(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("JSON input not found.", path);
        return JsonUtility.FromJson<T>(File.ReadAllText(path));
    }

    private static Avatar LoadSourceAvatar()
    {
        return AssetDatabase.LoadAllAssetsAtPath(ImportedFbxPath)
            .OfType<Avatar>()
            .OrderByDescending(avatar => avatar != null && avatar.isHuman)
            .ThenBy(avatar => avatar != null ? avatar.name : string.Empty, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static AvatarBuildResult ResolveSourceAvatar(GameObject instance)
    {
        Avatar imported = LoadSourceAvatar();
        if (imported != null)
        {
            return new AvatarBuildResult
            {
                avatar = imported,
                mode = "imported_fbx_avatar",
                humanBoneCount = 0,
                missingHumanBoneNames = Array.Empty<string>(),
            };
        }
        return BuildTransientHumanoidAvatar(instance);
    }

    private static AvatarBuildResult BuildTransientHumanoidAvatar(GameObject instance)
    {
        var transforms = instance.GetComponentsInChildren<Transform>(true)
            .GroupBy(transform => transform.name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var human = new List<HumanBone>();
        var missing = new List<string>();

        AddHumanBone(human, transforms, HumanBodyBones.Hips, "pelvis", missing);
        AddHumanBone(human, transforms, HumanBodyBones.Spine, "spine_01", missing);
        AddHumanBone(human, transforms, HumanBodyBones.Chest, "spine_03", missing);
        AddHumanBone(human, transforms, HumanBodyBones.UpperChest, "spine_05", missing);
        AddHumanBone(human, transforms, HumanBodyBones.Neck, "neck_01", missing);
        AddHumanBone(human, transforms, HumanBodyBones.Head, "head", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftShoulder, "clavicle_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftUpperArm, "upperarm_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftLowerArm, "lowerarm_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftHand, "hand_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightShoulder, "clavicle_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightUpperArm, "upperarm_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightLowerArm, "lowerarm_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightHand, "hand_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftUpperLeg, "thigh_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftLowerLeg, "calf_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftFoot, "foot_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftToes, "ball_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightUpperLeg, "thigh_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightLowerLeg, "calf_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightFoot, "foot_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightToes, "ball_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftThumbProximal, "thumb_01_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftThumbIntermediate, "thumb_02_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftThumbDistal, "thumb_03_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftIndexProximal, "index_01_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftIndexIntermediate, "index_02_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftIndexDistal, "index_03_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftMiddleProximal, "middle_01_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftMiddleIntermediate, "middle_02_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftMiddleDistal, "middle_03_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftRingProximal, "ring_01_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftRingIntermediate, "ring_02_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftRingDistal, "ring_03_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftLittleProximal, "pinky_01_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftLittleIntermediate, "pinky_02_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.LeftLittleDistal, "pinky_03_l", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightThumbProximal, "thumb_01_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightThumbIntermediate, "thumb_02_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightThumbDistal, "thumb_03_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightIndexProximal, "index_01_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightIndexIntermediate, "index_02_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightIndexDistal, "index_03_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightMiddleProximal, "middle_01_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightMiddleIntermediate, "middle_02_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightMiddleDistal, "middle_03_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightRingProximal, "ring_01_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightRingIntermediate, "ring_02_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightRingDistal, "ring_03_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightLittleProximal, "pinky_01_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightLittleIntermediate, "pinky_02_r", missing);
        AddHumanBone(human, transforms, HumanBodyBones.RightLittleDistal, "pinky_03_r", missing);

        var description = new HumanDescription
        {
            human = human.ToArray(),
            skeleton = transforms.Values.Select(ToSkeletonBone).ToArray(),
            upperArmTwist = 0.5f,
            lowerArmTwist = 0.5f,
            upperLegTwist = 0.5f,
            lowerLegTwist = 0.5f,
            armStretch = 0.05f,
            legStretch = 0.05f,
            feetSpacing = 0f,
            hasTranslationDoF = false,
        };

        Avatar avatar = AvatarBuilder.BuildHumanAvatar(instance, description);
        if (avatar != null) avatar.name = "DragonKnightArmorA2KT31TransientHumanoidAvatar";
        return new AvatarBuildResult
        {
            avatar = avatar,
            mode = "transient_humanoid_avatar",
            humanBoneCount = human.Count,
            missingHumanBoneNames = missing.ToArray(),
        };
    }

    private static void AddHumanBone(List<HumanBone> human, Dictionary<string, Transform> transforms, HumanBodyBones humanBone, string sourceBoneName, List<string> missing)
    {
        if (!transforms.ContainsKey(sourceBoneName))
        {
            missing.Add(HumanTrait.BoneName[(int)humanBone] + "=" + sourceBoneName);
            return;
        }

        human.Add(new HumanBone
        {
            humanName = HumanTrait.BoneName[(int)humanBone],
            boneName = sourceBoneName,
            limit = new HumanLimit { useDefaultValues = true },
        });
    }

    private static SkeletonBone ToSkeletonBone(Transform transform)
    {
        return new SkeletonBone
        {
            name = transform.name,
            position = transform.localPosition,
            rotation = transform.localRotation,
            scale = transform.localScale,
        };
    }

    private static void WriteReport(string path, LocatorReport report)
    {
        string fullPath = Path.GetFullPath(path);
        string directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
        using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.Read))
        using (var writer = new StreamWriter(stream))
        {
            writer.Write(JsonUtility.ToJson(report, true));
        }
    }

    private static string GetArgument(string name, string defaultValue, bool required = true)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }
        if (required && string.IsNullOrWhiteSpace(defaultValue)) throw new ArgumentException("Missing " + name + ".");
        return defaultValue;
    }

    private static string ProjectRoot
    {
        get { return Directory.GetParent(Application.dataPath).FullName; }
    }

    private static string RelativeTransformPath(Transform root, Transform transform)
    {
        if (root == null || transform == null || transform == root) return string.Empty;
        var parts = new Stack<string>();
        Transform current = transform;
        while (current != null && current != root)
        {
            parts.Push(current.name);
            current = current.parent;
        }
        return string.Join("/", parts.ToArray());
    }

    private sealed class RequiredRow
    {
        public readonly string targetGender;
        public readonly string targetSlot;
        public readonly string nativeEquipmentType;
        public readonly string sourceBoneRaw;
        public readonly string policyFamily;
        public readonly int expectedWeightedVertexCount;
        public readonly int expectedWeightedTriangleCount;

        public RequiredRow(string targetGender, string targetSlot, string nativeEquipmentType, string sourceBoneRaw, string policyFamily, int expectedWeightedVertexCount, int expectedWeightedTriangleCount)
        {
            this.targetGender = targetGender;
            this.targetSlot = targetSlot;
            this.nativeEquipmentType = nativeEquipmentType;
            this.sourceBoneRaw = sourceBoneRaw;
            this.policyFamily = policyFamily;
            this.expectedWeightedVertexCount = expectedWeightedVertexCount;
            this.expectedWeightedTriangleCount = expectedWeightedTriangleCount;
        }
    }

    private sealed class WeightedRowData
    {
        public RequiredRow required;
        public int boneIndex;
        public int[] weightedVertices;
        public TriangleData[] triangles;
    }

    private struct TriangleData
    {
        public int sourceTriangleOrdinal;
        public int a;
        public int b;
        public int c;
        public Vector3 baselineNormal;
        public float baselineArea;
        public Vector3 baselineCentroid;
        public int weightedVertexCountInTriangle;
        public float weight0;
        public float weight1;
        public float weight2;
        public float weightMass;
    }

    private sealed class LoadedClip
    {
        public AnimationClip clip;
        public string bundlePath;
    }

    private sealed class AvatarBuildResult
    {
        public Avatar avatar;
        public string mode;
        public int humanBoneCount;
        public string[] missingHumanBoneNames;
    }

    private sealed class RowMetric
    {
        public float maxVertexDisplacement;
        public float meanVertexDisplacement;
        public int movedVertexCount;
        public int collapsedTriangleCount;
        public int invertedTriangleCount;
        public Bounds poseBounds;
        public bool poseBoundsInitialized;
        public InvertedTriangleReport[] invertedTriangles;
    }

    private sealed class BindingReport
    {
        public int curveBindingCount;
        public int matchingBindingPaths;
        public int changedTransformCount;
    }

    private sealed class SampleEvaluation
    {
        public string evaluationMode;
        public RowMetric metric;
        public BindingReport binding;
    }

    private struct TransformState
    {
        public Transform transform;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;

        public static TransformState[] Capture(GameObject root)
        {
            return root.GetComponentsInChildren<Transform>(true)
                .Select(transform => new TransformState
                {
                    transform = transform,
                    localPosition = transform.localPosition,
                    localRotation = transform.localRotation,
                    localScale = transform.localScale,
                })
                .ToArray();
        }

        public static void Restore(TransformState[] states)
        {
            foreach (TransformState state in states ?? Array.Empty<TransformState>())
            {
                if (state.transform == null) continue;
                state.transform.localPosition = state.localPosition;
                state.transform.localRotation = state.localRotation;
                state.transform.localScale = state.localScale;
            }
        }

        public static int CountChanged(TransformState[] before, TransformState[] after)
        {
            if (before == null || after == null) return 0;
            int count = 0;
            int length = Mathf.Min(before.Length, after.Length);
            for (int index = 0; index < length; index++)
            {
                if (before[index].transform == null || after[index].transform == null) continue;
                if (Vector3.Distance(before[index].localPosition, after[index].localPosition) > DisplacementEpsilon
                    || Quaternion.Angle(before[index].localRotation, after[index].localRotation) > 0.001f
                    || Vector3.Distance(before[index].localScale, after[index].localScale) > DisplacementEpsilon)
                {
                    count++;
                }
            }
            return count;
        }
    }

    [Serializable]
    private sealed class T26Record
    {
        public string marker;
        public PoseDriverRow[] poseDriverRows;
    }

    [Serializable]
    private sealed class PoseDriverRow
    {
        public string pose;
        public bool approvedDriverPresent;
        public string sourceKind;
        public string[] matchedDriverNames;
        public ClipRecord[] clipRecords;
    }

    [Serializable]
    private sealed class ClipRecord
    {
        public string clip;
        public string bundle;
        public string bundlePath;
    }

    [Serializable]
    private sealed class T29Record
    {
        public string marker;
        public T29Summary summary;
    }

    [Serializable]
    private sealed class T29Summary
    {
        public int totalCollapsedTriangles;
        public int totalInvertedTriangles;
    }

    [Serializable]
    private sealed class LocatorReport
    {
        public string marker;
        public string generatedAt;
        public string unityVersion;
        public string projectRoot;
        public string sourceAssetPath;
        public string sourceBasis;
        public string targetPose;
        public string targetClip;
        public string t26Marker;
        public string t29Marker;
        public int t29TotalInvertedTriangles;
        public int t29TotalCollapsedTriangles;
        public string selectedRenderer;
        public int sourceVertexCount;
        public int sourceSubmeshIndex;
        public int sourceSubmeshTriangleCount;
        public int sourceBoneCount;
        public int sourceBoneWeightCount;
        public string sourceAvatarName;
        public string sourceAvatarBuildMode;
        public bool sourceAvatarIsValid;
        public bool sourceAvatarIsHuman;
        public int sourceAvatarHumanBoneCount;
        public string[] sourceAvatarMissingHumanBoneNames;
        public bool animatorPresent;
        public bool candidateMapApplicationExecuted;
        public bool conversionExecuted;
        public bool sourceFbxMutationExecuted;
        public bool unityProjectAssetMutationDuringSampling;
        public LocatorSummary summary;
        public RequiredFieldResult[] requiredFieldResults;
        public RowLocalizationReport[] rowLocalizations;
        public string[] blockedReasons;
        public string[] errors;
    }

    [Serializable]
    private sealed class LocatorSummary
    {
        public string targetedPose;
        public string targetedClip;
        public int rowLocalizationCount;
        public int exactTriangleRowsCaptured;
        public int totalInvertedTriangles;
        public int totalCollapsedTriangles;
        public int totalExactTriangleRecords;
        public float maxVertexDisplacement;
        public int rowsWithClipBindingApplied;
        public int rowsWithoutClipBindingApplied;
        public int t29TotalInvertedTriangles;
        public int t29TotalCollapsedTriangles;
        public bool exactTriangleCaptureMatchesT29Aggregate;
        public bool dynamicDeformedMeshSamplingExecuted;
        public bool visualRuntimeMetricsObserved;
        public bool candidateMapApplicationExecuted;
        public bool conversionExecuted;
    }

    [Serializable]
    private sealed class RequiredFieldResult
    {
        public string field;
        public string status;
        public string evidence;
    }

    [Serializable]
    private sealed class RowLocalizationReport
    {
        public string targetGender;
        public string targetSlot;
        public string nativeEquipmentType;
        public string pose;
        public string clip;
        public string sourceBoneRaw;
        public string policyFamily;
        public int sourceSubmeshIndex;
        public int weightedVertexCount;
        public int weightedTriangleCount;
        public float sampleTime;
        public float clipLength;
        public string bundlePath;
        public string evaluationMode;
        public int curveBindingCount;
        public int matchingBindingPaths;
        public int changedTransformCount;
        public bool clipBindingApplied;
        public bool geometryMetricsCaptured;
        public bool visualRuntimeMetricsObserved;
        public float maxVertexDisplacement;
        public float meanVertexDisplacement;
        public int movedVertexCount;
        public int collapsedTriangleCount;
        public int invertedTriangleCount;
        public bool exactInvertedTriangleOrdinalsCaptured;
        public Vec3 poseBoundsCenter;
        public Vec3 poseBoundsSize;
        public InvertedTriangleReport[] invertedTriangles;
        public string rowStatus;
    }

    [Serializable]
    private sealed class InvertedTriangleReport
    {
        public int sourceSubmeshIndex;
        public int sourceTriangleOrdinal;
        public int vertexIndex0;
        public int vertexIndex1;
        public int vertexIndex2;
        public int weightedVertexCountInTriangle;
        public float sourceBoneWeightAtVertex0;
        public float sourceBoneWeightAtVertex1;
        public float sourceBoneWeightAtVertex2;
        public float sourceBoneWeightMass;
        public float baselineArea;
        public float posedArea;
        public float normalizedNormalDot;
        public Vec3 baselineCentroid;
        public Vec3 posedCentroid;
        public Vec3 baselineNormal;
        public Vec3 posedNormal;
        public float vertex0Displacement;
        public float vertex1Displacement;
        public float vertex2Displacement;
    }

    [Serializable]
    private sealed class Vec3
    {
        public float x;
        public float y;
        public float z;

        public static Vec3 From(Vector3 value)
        {
            return new Vec3 { x = value.x, y = value.y, z = value.z };
        }
    }
}
