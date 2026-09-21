using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public static class DragonKnightArmorA2KT29NoWriteDeformationMetricSampler
{
    private const string ReadyMarker = "DRAGON_KNIGHT_ARMOR_A2K_T29_NO_WRITE_DEFORMATION_METRIC_SAMPLER_GEOMETRY_METRICS_CAPTURED_VISUAL_METRICS_UNOBSERVED";
    private const string ClipBindingBlockedMarker = "DRAGON_KNIGHT_ARMOR_A2K_T29_NO_WRITE_DEFORMATION_METRIC_SAMPLER_BLOCKED_NATIVE_CLIP_BINDINGS_NOT_APPLIED";
    private const string BlockedMarker = "DRAGON_KNIGHT_ARMOR_A2K_T29_NO_WRITE_DEFORMATION_METRIC_SAMPLER_BLOCKED";
    private const string ImportedFbxPath = "Assets/DragonKnightArmorA2KT20/SK_Dragon_knight_UE5_no_Cape.fbx";
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

    public static void Sample()
    {
        string reportPath = GetArgument("-dragonKnightArmorSamplerReport", string.Empty);
        var report = new SamplerReport
        {
            marker = BlockedMarker,
            generatedAt = "2026-08-06",
            unityVersion = Application.unityVersion,
            projectRoot = ProjectRoot,
            sourceAssetPath = ImportedFbxPath,
            sourceBasis = "tainted_grail_foa_native_only",
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
            string t27Path = GetArgument("-dragonKnightArmorT27Input", string.Empty);
            string bundleRoot = GetArgument("-dragonKnightFoaBundleRoot", string.Empty, false);

            if (string.IsNullOrWhiteSpace(reportPath)) throw new ArgumentException("Missing -dragonKnightArmorSamplerReport.");
            if (string.IsNullOrWhiteSpace(t26Path)) throw new ArgumentException("Missing -dragonKnightArmorT26Input.");
            if (string.IsNullOrWhiteSpace(t27Path)) throw new ArgumentException("Missing -dragonKnightArmorT27Input.");

            T26Record t26 = ReadJson<T26Record>(t26Path);
            T27Record t27 = ReadJson<T27Record>(t27Path);
            report.t26Marker = t26.marker;
            report.t27Marker = t27.marker;
            report.t27PreconditionsReady = t27.samplingSummary != null && t27.samplingSummary.preconditionsReady;
            report.t27PlannedSamplingRows = t27.samplingSummary != null ? t27.samplingSummary.plannedSamplingRows : 0;
            report.t27PlannedMetricSlots = t27.samplingSummary != null ? t27.samplingSummary.plannedMetricSlots : 0;

            AddRequired(required, "t26_ready", t26.marker == "DRAGON_KNIGHT_ARMOR_A2K_T26_TAINTED_GRAIL_NATIVE_POSE_DRIVER_SOURCE_PACKET_READY", "T26 marker=" + t26.marker + ".");
            AddRequired(required, "t27_matrix_ready", report.t27PreconditionsReady && report.t27PlannedSamplingRows == 120 && report.t27PlannedMetricSlots == 960, "T27 preconditionsReady=" + report.t27PreconditionsReady + " plannedRows=" + report.t27PlannedSamplingRows.ToString(CultureInfo.InvariantCulture) + " metricSlots=" + report.t27PlannedMetricSlots.ToString(CultureInfo.InvariantCulture) + ".");
            AddRequired(required, "source_asset_already_imported", AssetDatabase.LoadAssetAtPath<GameObject>(ImportedFbxPath) != null, "Imported source asset path=" + ImportedFbxPath + ".");
            AddRequired(required, "bundle_root_available", !string.IsNullOrWhiteSpace(bundleRoot) && Directory.Exists(bundleRoot), "FoA bundle root=" + bundleRoot + ".");

            GameObject sourceAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ImportedFbxPath);
            if (sourceAsset == null) blockers.Add("source_asset_not_already_imported_from_t20");
            if (string.IsNullOrWhiteSpace(bundleRoot) || !Directory.Exists(bundleRoot)) blockers.Add("foa_bundle_root_missing");
            if (t26.poseDriverRows == null || t26.poseDriverRows.Length != 20) blockers.Add("t26_pose_driver_row_count_not_20");

            if (blockers.Count == 0)
            {
                ExecuteSampling(sourceAsset, t26, bundleRoot, report, required, blockers);
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
            blockers.Add("sampler_exception");
            report.errors = new[] { ex.ToString() };
            report.requiredFieldResults = required.ToArray();
            report.blockedReasons = blockers.Distinct(StringComparer.Ordinal).ToArray();
            CompleteMarker(report);
            Debug.LogError(report.marker + ": " + ex);
            WriteReport(reportPath, report);
            EditorApplication.Exit(1);
        }
    }

    private static void ExecuteSampling(GameObject sourceAsset, T26Record t26, string bundleRoot, SamplerReport report, List<RequiredFieldResult> required, List<string> blockers)
    {
        GameObject instance = UnityEngine.Object.Instantiate(sourceAsset);
        instance.name = "DragonKnightArmorA2KT29NoWriteSampler";
        instance.hideFlags = HideFlags.HideAndDontSave;

        var retainedBundles = new Dictionary<string, AssetBundle>(StringComparer.OrdinalIgnoreCase);
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
            var baselinePose = TransformState.Capture(instance);
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

            Dictionary<string, PoseDriverRow> poseRows = (t26.poseDriverRows ?? Array.Empty<PoseDriverRow>())
                .Where(row => row != null && !string.IsNullOrWhiteSpace(row.pose))
                .GroupBy(row => row.pose, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            var loadedClips = LoadClips(t26, bundleRoot, retainedBundles, blockers);
            AddRequired(required, "native_clip_assets_loaded", loadedClips.Count >= 20, "Loaded clip keys=" + loadedClips.Count.ToString(CultureInfo.InvariantCulture) + ".");

            var weightedData = RequiredRows.Select(row => BuildWeightedData(row, bones, boneWeights, sourceVertices, sourceTriangles, baselineVertices)).ToArray();
            AddRequired(required, "weighted_source_rows_resolved", weightedData.All(row => row.weightedVertices.Length == row.required.expectedWeightedVertexCount), "Resolved weighted rows=" + weightedData.Count(row => row.weightedVertices.Length == row.required.expectedWeightedVertexCount).ToString(CultureInfo.InvariantCulture) + "/6.");

            var blockerRows = new List<BlockerRowReport>();
            var sampleRows = new List<SampleRowReport>();

            foreach (WeightedRowData weighted in weightedData)
            {
                var blocker = new BlockerRowReport
                {
                    targetGender = weighted.required.targetGender,
                    targetSlot = weighted.required.targetSlot,
                    nativeEquipmentType = weighted.required.nativeEquipmentType,
                    sourceBoneRaw = weighted.required.sourceBoneRaw,
                    policyFamily = weighted.required.policyFamily,
                    weightedVertexCount = weighted.weightedVertices.Length,
                    weightedTriangleCount = weighted.triangles.Length,
                    plannedPoseRows = poseRows.Count,
                    sampledPoseRows = 0,
                    blockedPoseRows = 0,
                };

                foreach (PoseDriverRow pose in poseRows.Values.OrderBy(row => row.pose, StringComparer.Ordinal))
                {
                    SampleRowReport sample = SamplePose(instance, renderer, animator, baselinePose, baselineVertices, weighted, pose, loadedClips, sourcePaths);
                    sampleRows.Add(sample);
                    if (sample.geometryMetricsCaptured) blocker.sampledPoseRows++;
                    if (!sample.geometryMetricsCaptured || !sample.clipBindingApplied) blocker.blockedPoseRows++;
                }

                blocker.rowSamplingStatus = blocker.sampledPoseRows == blocker.plannedPoseRows && blocker.blockedPoseRows == 0
                    ? "sampled"
                    : "sampled_with_binding_or_metric_blockers";
                blockerRows.Add(blocker);
            }

            report.blockerRows = blockerRows.ToArray();
            report.sampleRows = sampleRows.ToArray();
            report.summary = new SamplingSummary
            {
                plannedSamplingRows = sampleRows.Count,
                geometryMetricRowsCaptured = sampleRows.Count(row => row.geometryMetricsCaptured),
                rowsWithClipBindingApplied = sampleRows.Count(row => row.clipBindingApplied),
                rowsWithoutClipBindingApplied = sampleRows.Count(row => !row.clipBindingApplied),
                maxVertexDisplacement = sampleRows.Count == 0 ? 0f : sampleRows.Max(row => row.maxVertexDisplacement),
                totalCollapsedTriangles = sampleRows.Sum(row => row.collapsedTriangleCount),
                totalInvertedTriangles = sampleRows.Sum(row => row.invertedTriangleCount),
                clippingRowsObserved = 0,
                seamRowsObserved = 0,
                bodyCoverRowsObserved = 0,
                materialShadowLayerRowsObserved = 0,
                dynamicDeformedMeshSamplingExecuted = sampleRows.Count > 0,
                visualMetricSamplingExecuted = false,
                candidateMapApplicationExecuted = false,
                conversionExecuted = false,
            };

            if (report.summary.rowsWithClipBindingApplied == 0)
            {
                blockers.Add("native_clip_bindings_not_applied_to_source_skeleton");
            }
            if (report.summary.geometryMetricRowsCaptured != 120)
            {
                blockers.Add("not_all_geometry_metric_rows_captured");
            }
            blockers.Add("visual_metrics_not_observed_by_editor_mesh_bake_sampler");
        }
        finally
        {
            foreach (AssetBundle bundle in retainedBundles.Values)
            {
                if (bundle != null) bundle.Unload(false);
            }
            UnityEngine.Object.DestroyImmediate(instance);
        }
    }

    private static SampleRowReport SamplePose(
        GameObject instance,
        SkinnedMeshRenderer renderer,
        Animator animator,
        TransformState[] baselinePose,
        Vector3[] baselineVertices,
        WeightedRowData weighted,
        PoseDriverRow pose,
        Dictionary<string, LoadedClip> loadedClips,
        HashSet<string> sourcePaths)
    {
        var report = new SampleRowReport
        {
            targetGender = weighted.required.targetGender,
            targetSlot = weighted.required.targetSlot,
            nativeEquipmentType = weighted.required.nativeEquipmentType,
            pose = pose.pose,
            sourceKind = pose.sourceKind,
            sourceBoneRaw = weighted.required.sourceBoneRaw,
            policyFamily = weighted.required.policyFamily,
            weightedVertexCount = weighted.weightedVertices.Length,
            weightedTriangleCount = weighted.triangles.Length,
            geometryMetricsCaptured = false,
            visualMetricsCaptured = false,
            clippingObserved = false,
            seamObserved = false,
            bodyCoverObserved = false,
            materialShadowLayerObserved = false,
        };

        string[] clipNames = pose.matchedDriverNames ?? Array.Empty<string>();
        var usedClipReports = new List<ClipSampleReport>();
        float maxDisplacement = 0f;
        float displacementSum = 0f;
        int movedVertices = 0;
        int collapsed = 0;
        int inverted = 0;
        int samples = 0;
        int bindingMatches = 0;
        int changedTransformCount = 0;
        Bounds posedBounds = new Bounds(Vector3.zero, Vector3.zero);
        bool posedBoundsInitialized = false;

        foreach (string clipName in clipNames)
        {
            if (string.IsNullOrWhiteSpace(clipName) || !loadedClips.TryGetValue(clipName, out LoadedClip loaded) || loaded.clip == null)
            {
                usedClipReports.Add(new ClipSampleReport { clip = clipName, loaded = false, status = "clip_not_loaded" });
                continue;
            }

            float sampleTime = loaded.clip.length <= 0f ? 0f : Mathf.Min(loaded.clip.length * 0.5f, Mathf.Max(0f, loaded.clip.length - 0.001f));
            SampleEvaluation evaluation = EvaluateClip(instance, renderer, animator, baselinePose, baselineVertices, weighted, loaded.clip, sampleTime, sourcePaths);
            RowMetric metric = evaluation.metric;

            maxDisplacement = Mathf.Max(maxDisplacement, metric.maxVertexDisplacement);
            displacementSum += metric.meanVertexDisplacement;
            movedVertices += metric.movedVertexCount;
            collapsed += metric.collapsedTriangleCount;
            inverted += metric.invertedTriangleCount;
            samples++;

            BindingReport binding = evaluation.binding;
            bindingMatches += binding.matchingBindingPaths;
            changedTransformCount += binding.changedTransformCount;
            if (metric.poseBoundsInitialized)
            {
                if (!posedBoundsInitialized)
                {
                    posedBounds = metric.poseBounds;
                    posedBoundsInitialized = true;
                }
                else
                {
                    posedBounds.Encapsulate(metric.poseBounds);
                }
            }

            usedClipReports.Add(new ClipSampleReport
            {
                clip = loaded.clip.name,
                loaded = true,
                bundlePath = loaded.bundlePath,
                sampleTime = sampleTime,
                clipLength = loaded.clip.length,
                evaluationMode = evaluation.evaluationMode,
                avatarPresent = animator != null && animator.avatar != null,
                curveBindingCount = binding.curveBindingCount,
                matchingBindingPaths = binding.matchingBindingPaths,
                changedTransformCount = binding.changedTransformCount,
                maxVertexDisplacement = metric.maxVertexDisplacement,
                collapsedTriangleCount = metric.collapsedTriangleCount,
                invertedTriangleCount = metric.invertedTriangleCount,
                status = binding.changedTransformCount > 0 || metric.maxVertexDisplacement > DisplacementEpsilon ? "sampled_binding_applied" : "sampled_no_binding_change_detected",
            });
        }

        report.sampledClipCount = samples;
        report.clipBindingPathMatches = bindingMatches;
        report.changedTransformCount = changedTransformCount;
        report.clipBindingApplied = changedTransformCount > 0 || maxDisplacement > DisplacementEpsilon;
        report.maxVertexDisplacement = maxDisplacement;
        report.meanVertexDisplacement = samples == 0 ? 0f : displacementSum / samples;
        report.movedVertexCount = movedVertices;
        report.collapsedTriangleCount = collapsed;
        report.invertedTriangleCount = inverted;
        report.poseBoundsCenter = Vec3.From(posedBoundsInitialized ? posedBounds.center : Vector3.zero);
        report.poseBoundsSize = Vec3.From(posedBoundsInitialized ? posedBounds.size : Vector3.zero);
        report.geometryMetricsCaptured = samples > 0 && weighted.weightedVertices.Length > 0 && weighted.triangles.Length > 0;
        report.rowStatus = report.geometryMetricsCaptured && report.clipBindingApplied
            ? "geometry_metrics_captured"
            : "blocked_clip_binding_or_geometry_metric_incomplete";
        report.clipSamples = usedClipReports.ToArray();
        return report;
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
        PlayableGraph graph = PlayableGraph.Create("DragonKnightArmorA2KT29NativePoseSample");
        try
        {
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, clip);
            playable.SetApplyFootIK(true);
            playable.SetApplyPlayableIK(false);
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "DragonKnightArmorA2KT29Output", animator);
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

        foreach (TriangleData triangle in weighted.triangles)
        {
            if (triangle.a >= posedVertices.Length || triangle.b >= posedVertices.Length || triangle.c >= posedVertices.Length) continue;
            Vector3 pa = posedVertices[triangle.a];
            Vector3 pb = posedVertices[triangle.b];
            Vector3 pc = posedVertices[triangle.c];
            Vector3 posedNormal = Vector3.Cross(pb - pa, pc - pa);
            float posedArea = posedNormal.magnitude * 0.5f;
            if (posedArea <= TriangleAreaEpsilon) metric.collapsedTriangleCount++;
            if (triangle.baselineNormal.sqrMagnitude > 0f && posedNormal.sqrMagnitude > 0f && Vector3.Dot(triangle.baselineNormal.normalized, posedNormal.normalized) < 0f)
            {
                metric.invertedTriangleCount++;
            }
        }

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

    private static Dictionary<string, LoadedClip> LoadClips(T26Record t26, string bundleRoot, Dictionary<string, AssetBundle> retainedBundles, List<string> blockers)
    {
        var loaded = new Dictionary<string, LoadedClip>(StringComparer.Ordinal);
        foreach (PoseDriverRow pose in t26.poseDriverRows ?? Array.Empty<PoseDriverRow>())
        {
            foreach (ClipRecord record in pose.clipRecords ?? Array.Empty<ClipRecord>())
            {
                if (record == null || string.IsNullOrWhiteSpace(record.clip) || loaded.ContainsKey(record.clip)) continue;
                string bundlePath = !string.IsNullOrWhiteSpace(record.bundlePath)
                    ? record.bundlePath
                    : Path.Combine(bundleRoot, record.bundle ?? string.Empty);
                if (string.IsNullOrWhiteSpace(bundlePath) || !File.Exists(bundlePath))
                {
                    blockers.Add("native_clip_bundle_missing_" + record.clip);
                    continue;
                }

                if (!retainedBundles.TryGetValue(bundlePath, out AssetBundle bundle) || bundle == null)
                {
                    bundle = AssetBundle.LoadFromFile(bundlePath);
                    retainedBundles[bundlePath] = bundle;
                }

                if (bundle == null)
                {
                    blockers.Add("native_clip_bundle_load_failed_" + record.clip);
                    continue;
                }

                AnimationClip clip = bundle.LoadAsset<AnimationClip>(record.clip);
                if (clip == null)
                {
                    AnimationClip[] allClips = bundle.LoadAllAssets<AnimationClip>();
                    clip = allClips.FirstOrDefault(candidate => candidate != null && string.Equals(candidate.name, record.clip, StringComparison.Ordinal));
                }
                if (clip == null)
                {
                    blockers.Add("native_clip_asset_missing_" + record.clip);
                    continue;
                }

                loaded[record.clip] = new LoadedClip { clip = clip, bundlePath = bundlePath };
            }
        }
        return loaded;
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
            if (baselineVertices != null && a < baselineVertices.Length && b < baselineVertices.Length && c < baselineVertices.Length)
            {
                normal = Vector3.Cross(baselineVertices[b] - baselineVertices[a], baselineVertices[c] - baselineVertices[a]);
            }
            triangles.Add(new TriangleData { a = a, b = b, c = c, baselineNormal = normal });
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

    private static void CompleteMarker(SamplerReport report)
    {
        if (report.summary != null && report.summary.geometryMetricRowsCaptured == 120 && report.summary.rowsWithClipBindingApplied == 120)
        {
            report.marker = ReadyMarker;
            return;
        }
        if (report.summary != null && report.summary.geometryMetricRowsCaptured == 120 && report.summary.rowsWithClipBindingApplied == 0)
        {
            report.marker = ClipBindingBlockedMarker;
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
        if (avatar != null) avatar.name = "DragonKnightArmorA2KT29TransientHumanoidAvatar";
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

        var bone = new HumanBone
        {
            humanName = HumanTrait.BoneName[(int)humanBone],
            boneName = sourceBoneName,
            limit = new HumanLimit { useDefaultValues = true },
        };
        human.Add(bone);
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

    private static void WriteReport(string path, SamplerReport report)
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

    private static string TransformPath(Transform transform)
    {
        if (transform == null) return string.Empty;
        var parts = new Stack<string>();
        Transform current = transform;
        while (current != null)
        {
            parts.Push(current.name);
            current = current.parent;
        }
        return string.Join("/", parts.ToArray());
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
        public int a;
        public int b;
        public int c;
        public Vector3 baselineNormal;
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
    private sealed class T27Record
    {
        public string marker;
        public T27Summary samplingSummary;
    }

    [Serializable]
    private sealed class T27Summary
    {
        public bool preconditionsReady;
        public int plannedSamplingRows;
        public int plannedMetricSlots;
    }

    [Serializable]
    private sealed class SamplerReport
    {
        public string marker;
        public string generatedAt;
        public string unityVersion;
        public string projectRoot;
        public string sourceAssetPath;
        public string sourceBasis;
        public string t26Marker;
        public string t27Marker;
        public bool t27PreconditionsReady;
        public int t27PlannedSamplingRows;
        public int t27PlannedMetricSlots;
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
        public SamplingSummary summary;
        public RequiredFieldResult[] requiredFieldResults;
        public BlockerRowReport[] blockerRows;
        public SampleRowReport[] sampleRows;
        public string[] blockedReasons;
        public string[] errors;
    }

    [Serializable]
    private sealed class SamplingSummary
    {
        public int plannedSamplingRows;
        public int geometryMetricRowsCaptured;
        public int rowsWithClipBindingApplied;
        public int rowsWithoutClipBindingApplied;
        public float maxVertexDisplacement;
        public int totalCollapsedTriangles;
        public int totalInvertedTriangles;
        public int clippingRowsObserved;
        public int seamRowsObserved;
        public int bodyCoverRowsObserved;
        public int materialShadowLayerRowsObserved;
        public bool dynamicDeformedMeshSamplingExecuted;
        public bool visualMetricSamplingExecuted;
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
    private sealed class BlockerRowReport
    {
        public string targetGender;
        public string targetSlot;
        public string nativeEquipmentType;
        public string sourceBoneRaw;
        public string policyFamily;
        public int weightedVertexCount;
        public int weightedTriangleCount;
        public int plannedPoseRows;
        public int sampledPoseRows;
        public int blockedPoseRows;
        public string rowSamplingStatus;
    }

    [Serializable]
    private sealed class SampleRowReport
    {
        public string targetGender;
        public string targetSlot;
        public string nativeEquipmentType;
        public string pose;
        public string sourceKind;
        public string sourceBoneRaw;
        public string policyFamily;
        public int weightedVertexCount;
        public int weightedTriangleCount;
        public int sampledClipCount;
        public int clipBindingPathMatches;
        public int changedTransformCount;
        public bool clipBindingApplied;
        public bool geometryMetricsCaptured;
        public bool visualMetricsCaptured;
        public float maxVertexDisplacement;
        public float meanVertexDisplacement;
        public int movedVertexCount;
        public int collapsedTriangleCount;
        public int invertedTriangleCount;
        public Vec3 poseBoundsCenter;
        public Vec3 poseBoundsSize;
        public bool clippingObserved;
        public bool seamObserved;
        public bool bodyCoverObserved;
        public bool materialShadowLayerObserved;
        public string rowStatus;
        public ClipSampleReport[] clipSamples;
    }

    [Serializable]
    private sealed class ClipSampleReport
    {
        public string clip;
        public bool loaded;
        public string bundlePath;
        public float sampleTime;
        public float clipLength;
        public string evaluationMode;
        public bool avatarPresent;
        public int curveBindingCount;
        public int matchingBindingPaths;
        public int changedTransformCount;
        public float maxVertexDisplacement;
        public int collapsedTriangleCount;
        public int invertedTriangleCount;
        public string status;
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
