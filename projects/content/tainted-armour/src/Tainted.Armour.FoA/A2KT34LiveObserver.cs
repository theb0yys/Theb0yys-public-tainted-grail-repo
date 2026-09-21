using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using BepInEx.Logging;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Tainted.Armour.FoA;

internal static class A2KT34LiveObserver
{
    private const string Marker = "TAINTED_ARMOUR_A2K_T34_LIVE_FOA_OBSERVER_PACKET";
    private const string TargetPose = "fall";
    private const string TargetClip = "Anim_Hero_TPP_Base_Knockdown_Air_Loop";
    private const string HeroAnimancerTypeName = "Awaken.TG.Main.Utility.Animations.ARAnimator.ARHeroAnimancer";
    private const int MaxPoseBindingCandidateRows = 32;
    private const int MaxPoseBindingGraphDepth = 8;
    private const int MaxPoseBindingGraphNodes = 8000;
    private const int MaxTppReferenceRows = 48;
    private const int MaxTppReferenceGraphDepth = 5;
    private const int MaxTppReferenceGraphNodes = 4096;
    private const int MaxAssetReferenceIdentityRows = 96;
    private const int MaxAddressableLocatorRows = 256;
    private const int MaxAddressableSuccessfulDirectLocateRows = 512;
    private const int MaxAddressableRuntimeKeyDependencyRows = 512;
    private const int MaxAddressableRuntimeKeyDependencyGraphDepth = 6;
    private const int MaxAddressableRuntimeKeyDependencyGraphNodes = 2048;
    private const int MaxAddressableLocatorDiagnosticRows = 160;
    private const int MaxAddressableLocatorKeysScanned = 8192;
    private const int MaxAddressableLocatorKeySamples = 12;
    private const int AddressableFileReadBufferSize = 65536;
    private const string SafetySummary = "observerOnly=true,candidateMapApplicationExecuted=false,conversionExecuted=false,sidecarGenerationExecuted=false,sourceFbxMutationExecuted=false,unityProjectAssetMutationExecuted=false,nativeGameFileWriteExecuted=false,runtimeLoaderChangeExecuted=false,itemRegistrationExecuted=false,equipExecuted=false,inventoryWriteExecuted=false,saveWriteExecuted=false";

    private static readonly string[] PlayerBodyKandraTokens =
    {
        "Hero:0[VHeroController]",
        "VHeroController",
        "HeroMale_TPP",
        "HeroFemale_TPP",
        "Mesh_BaseHuman_HeroTPP",
        "HeroTPP",
        "TppParent",
        "FppParent",
        "FppArms",
    };

    private static readonly string[] NonPlayerKandraOwnerTokens =
    {
        "Spec_NPC",
        "VNpcLocation",
        "AI_Human",
        "SpawnedLocations",
        "NpcLocation",
    };

    private static readonly string[] PlayerTppPoseOwnerTokens =
    {
        "TppParent",
        "HeroMale_TPP",
        "HeroFemale_TPP",
        "Mesh_BaseHuman_HeroTPP",
        "HeroTPP",
        "_TPP",
    };

    private static readonly string[] TppReferenceTokens =
    {
        "tpp",
        "thirdperson",
        "third_person",
        "maleHeroBodyTPP",
        "femaleHeroBodyTPP",
        "TppParent",
        "HeroMale_TPP",
        "HeroFemale_TPP",
        "Mesh_BaseHuman_HeroTPP",
        "HeroTPP",
        "_TPP",
    };

    private static readonly string[] AddressableLocatorQueries =
    {
        TargetClip,
        "HeroMale_TPP",
        "HeroFemale_TPP",
        "Mesh_BaseHuman_HeroTPP",
    };

    private static readonly string[] CurrentAddressableBundleAliasCandidates =
    {
        "d3ccf765d20b92f75a3a56fcbf935b61.bundle",
        "6d3223da354de646ab79d5660dcac9d2.bundle",
        "8b8d3b6ee29f92d24a50c9184dd7a14d.bundle",
        "54ebf0695961767ff79c3f490cd79354.bundle",
    };

    private static readonly string[] AddressableCatalogFileNames =
    {
        "catalog.json",
        "catalog_0.1.0.json",
    };

    private static readonly string[] AssetReferenceMemberNames =
    {
        "RuntimeKey",
        "AssetGUID",
        "AssetGuid",
        "Guid",
        "guid",
        "_guid",
        "m_AssetGUID",
        "SubObjectName",
        "Address",
        "Label",
        "PrimaryKey",
        "Asset",
    };

    private static readonly RequiredRow[] RequiredRows =
    {
        new RequiredRow("female", "neck_02", 3),
        new RequiredRow("female", "spine_04", 208),
        new RequiredRow("female", "spine_05", 11),
        new RequiredRow("male", "neck_02", 3),
        new RequiredRow("male", "spine_04", 208),
        new RequiredRow("male", "spine_05", 11),
    };

    internal static string Write(ManualLogSource logger, string configuredRoot, string trigger, int maxRendererRows)
    {
        string root = string.IsNullOrWhiteSpace(configuredRoot)
            ? Path.Combine(BepInEx.Paths.ConfigPath, Plugin.PluginGuid)
            : configuredRoot;
        string folder = Path.Combine(root, "a2k-t34-live-observer-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture));
        Directory.CreateDirectory(folder);

        ObserverPacket packet = CreatePacket(trigger, Mathf.Clamp(maxRendererRows, 0, 1000));
        string path = Path.Combine(folder, "a2k_t34_live_observer_packet.json");
        File.WriteAllText(path, JsonConvert.SerializeObject(packet, Formatting.Indented), Encoding.UTF8);

        logger.LogInfo(
            $"{Plugin.PluginName} A2K-T34 capture marker={packet.marker}; liveBodyEquipSceneObserved={packet.runtimeContext.liveBodyEquipSceneObserved}; poseBindingStatus={packet.runtimeContext.poseBindingStatus}; liveRuntimeMetricsObserved={packet.liveRuntimeMetricsObserved}; visualRuntimeAccepted={packet.visualRuntimeAccepted}; rows={packet.rows.Length}; blockers={string.Join("|", packet.blockers ?? Array.Empty<string>())}; safety={SafetySummary}.");
        return path;
    }

    internal static ObserverReadiness ProbeReadiness(int maxRendererRows) =>
        ObserverReadiness.From(RuntimeContext.Capture(Mathf.Clamp(maxRendererRows, 0, 1000), includeAddressableLocator: false));

    internal sealed class ObserverReadiness
    {
        private ObserverReadiness(
            bool ready,
            string statusToken,
            string triggerToken,
            string logSummary)
        {
            Ready = ready;
            StatusToken = statusToken;
            TriggerToken = triggerToken;
            LogSummary = logSummary;
        }

        public bool Ready { get; }

        public string StatusToken { get; }

        public string TriggerToken { get; }

        public string LogSummary { get; }

        internal static ObserverReadiness From(RuntimeContext context)
        {
            bool sceneReady = string.Equals(context.ActiveScene, "CampaignMap_HOS", StringComparison.Ordinal);
            bool ready = sceneReady && context.LiveBodyEquipSceneObserved;
            string statusToken =
                "scene=" + SafeToken(context.ActiveScene) +
                ";hero=" + context.HeroPresent.ToString(CultureInfo.InvariantCulture) +
                ";controller=" + context.ControllerPresent.ToString(CultureInfo.InvariantCulture) +
                ";bodyRoot=" + context.BodyRootPresent.ToString(CultureInfo.InvariantCulture) +
                ";skinned=" + context.SkinnedRendererCount.ToString(CultureInfo.InvariantCulture) +
                ";kandra=" + context.KandraRendererCount.ToString(CultureInfo.InvariantCulture) +
                ";playerKandraCandidates=" + context.PlayerKandraCandidateCount.ToString(CultureInfo.InvariantCulture) +
                ";sceneKandra=" + context.SceneKandraRendererCount.ToString(CultureInfo.InvariantCulture) +
                ";loadedKandra=" + context.LoadedKandraRendererCount.ToString(CultureInfo.InvariantCulture) +
                ";pose=" + context.TargetClipObserved.ToString(CultureInfo.InvariantCulture) +
                ";poseOwners=" + context.HeroAnimancerOwnerCount.ToString(CultureInfo.InvariantCulture) +
                ";acceptedPoseCandidates=" + context.AcceptedTppPoseBindingCandidateCount.ToString(CultureInfo.InvariantCulture);
            string triggerToken =
                "scene=" + SafeToken(context.ActiveScene) +
                ";liveBodyEquipSceneObserved=" + context.LiveBodyEquipSceneObserved.ToString(CultureInfo.InvariantCulture) +
                ";targetClipObserved=" + context.TargetClipObserved.ToString(CultureInfo.InvariantCulture) +
                ";kandraRendererCount=" + context.KandraRendererCount.ToString(CultureInfo.InvariantCulture) +
                ";playerKandraCandidateCount=" + context.PlayerKandraCandidateCount.ToString(CultureInfo.InvariantCulture) +
                ";heroAnimancerOwnerCount=" + context.HeroAnimancerOwnerCount.ToString(CultureInfo.InvariantCulture) +
                ";acceptedTppPoseBindingCandidateCount=" + context.AcceptedTppPoseBindingCandidateCount.ToString(CultureInfo.InvariantCulture);
            string logSummary =
                "activeScene=" + context.ActiveScene +
                "; heroPresent=" + context.HeroPresent.ToString(CultureInfo.InvariantCulture) +
                "; controllerPresent=" + context.ControllerPresent.ToString(CultureInfo.InvariantCulture) +
                "; bodyRootPresent=" + context.BodyRootPresent.ToString(CultureInfo.InvariantCulture) +
                "; skinnedRendererCount=" + context.SkinnedRendererCount.ToString(CultureInfo.InvariantCulture) +
                "; kandraRendererCount=" + context.KandraRendererCount.ToString(CultureInfo.InvariantCulture) +
                "; playerKandraCandidateCount=" + context.PlayerKandraCandidateCount.ToString(CultureInfo.InvariantCulture) +
                "; sceneKandraRendererCount=" + context.SceneKandraRendererCount.ToString(CultureInfo.InvariantCulture) +
                "; loadedKandraRendererCount=" + context.LoadedKandraRendererCount.ToString(CultureInfo.InvariantCulture) +
                "; targetClipObserved=" + context.TargetClipObserved.ToString(CultureInfo.InvariantCulture) +
                "; heroAnimancerOwnerCount=" + context.HeroAnimancerOwnerCount.ToString(CultureInfo.InvariantCulture) +
                "; acceptedTppPoseBindingCandidateCount=" + context.AcceptedTppPoseBindingCandidateCount.ToString(CultureInfo.InvariantCulture) +
                "; ready=" + ready.ToString(CultureInfo.InvariantCulture);
            return new ObserverReadiness(ready, statusToken, triggerToken, logSummary);
        }
    }

    private static ObserverPacket CreatePacket(string trigger, int maxRendererRows)
    {
        RuntimeContext context = RuntimeContext.Capture(maxRendererRows, includeAddressableLocator: true);
        VisualRuntimeObservationRow[] coreRows = RequiredRows
            .Select(row => CreateObservationRow(row, context))
            .ToArray();

        VisualRuntimeObservationRequest request = VisualRuntimeObservationRequest.CreateA2KT34NoWriteObserver(
            "current-task:" + SafeToken(trigger) + ";plugin=" + Plugin.PluginGuid + ";liveBodyEquipSceneObserved=" + context.LiveBodyEquipSceneObserved.ToString(CultureInfo.InvariantCulture),
            "T57 importer-owned exact 444-record set; per-triangle live binding remains blocked unless supplied by a later capture",
            SceneManager.GetActiveScene().name ?? string.Empty,
            context.LiveBodyEquipSceneObserved ? "observed:live FoA hero/body renderer context" : "blocked_unproven:live FoA hero/body renderer context not proven",
            context.LiveBodyEquipSceneObserved ? "observed:live FoA equip renderer context" : "blocked_unproven:live FoA equip renderer context not proven",
            context.PoseBinding,
            coreRows);

        string[] blockers = BuildBlockers(context).ToArray();
        return new ObserverPacket
        {
            marker = Marker,
            generatedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            pluginGuid = Plugin.PluginGuid,
            pluginName = Plugin.PluginName,
            pluginVersion = Plugin.PluginVersion,
            unityVersion = Application.unityVersion,
            routeId = request.RouteId,
            observerMode = request.ObserverMode,
            triggerSource = request.TriggerSource,
            evidenceSource = request.EvidenceSource,
            activeScene = request.ActiveScene,
            liveBodyObservationStatus = request.LiveBodyObservationStatus,
            liveEquipObservationStatus = request.LiveEquipObservationStatus,
            poseBindingStatus = request.PoseBindingStatus,
            runtimeContext = context.ToPacket(),
            rows = coreRows.Select(ObservationRowPacket.From).ToArray(),
            visualRuntimeObservationRequest = VisualRuntimeObservationRequestPacket.From(request),
            downstream = DownstreamBoundary.False(),
            liveRuntimeMetricsObserved = false,
            visualRuntimeAccepted = false,
            blockers = blockers,
            safety = SafetySummary,
        };
    }

    private static VisualRuntimeObservationRow CreateObservationRow(RequiredRow row, RuntimeContext context) =>
        new VisualRuntimeObservationRow(
            row.Gender,
            row.Bone,
            row.EvidenceRecordCount,
            context.LiveObjectPath,
            context.LiveRendererName,
            context.LiveMeshName,
            context.LiveMaterialNames,
            context.LiveAnimatorClipNames,
            "t56-normal-dot-classification-retained",
            "blocked_unobserved",
            "blocked_unobserved",
            "blocked_unobserved",
            "blocked_unobserved",
            "blocked_unobserved",
            "blocked_unobserved",
            "blocked_unobserved",
            context.LiveBodyEquipSceneObserved
                ? "A2K-T34 FoA live observer transport row; context-level live renderer/animator identity observed; per-row visual metrics blocked until captured."
                : "A2K-T34 FoA live observer transport row; per-row visual metrics blocked until captured.",
            context.LiveKandraIsRegisteredStatus,
            context.LiveKandraTryGetMeshMemoryStatus);

    private static IEnumerable<string> BuildBlockers(RuntimeContext context)
    {
        if (!context.LiveBodyEquipSceneObserved)
        {
            yield return "live_foa_body_equip_context_not_proven";
        }

        if (!context.TargetClipObserved)
        {
            yield return "a2k_t34_pose_binding_missing_or_invalid";
        }

        yield return "live_runtime_clipping_metric_unobserved";
        yield return "live_runtime_seam_visibility_metric_unobserved";
        yield return "live_runtime_bounds_camera_culling_metric_unobserved";
        yield return "live_runtime_body_cover_metric_unobserved";
        yield return "live_runtime_material_shadow_layer_metric_unobserved";
    }

    private static string SafeToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "none";
        }

        return value.Trim().Replace('\r', '_').Replace('\n', '_').Replace('\t', '_').Replace(',', ';');
    }

    private readonly struct RequiredRow
    {
        public RequiredRow(string gender, string bone, int evidenceRecordCount)
        {
            Gender = gender;
            Bone = bone;
            EvidenceRecordCount = evidenceRecordCount;
        }

        public string Gender { get; }
        public string Bone { get; }
        public int EvidenceRecordCount { get; }
    }

    internal sealed class RuntimeContext
    {
        private static readonly string[] BodyDataRootNames =
        {
            "mainHand",
            "offHand",
            "mainHandWrist",
            "offHandWrist",
            "firePoint",
            "head",
            "torso",
            "hips",
            "spine",
            "spine2",
            "tppPivot",
        };

        private readonly List<Transform> roots = new List<Transform>();
        private readonly List<Transform> playerVisualRoots = new List<Transform>();
        private readonly List<RendererPacket> rendererRows = new List<RendererPacket>();
        private readonly List<AnimatorPacket> animatorRows = new List<AnimatorPacket>();
        private readonly List<KandraRendererPacket> kandraRendererRows = new List<KandraRendererPacket>();
        private readonly List<PoseBindingOwnerPacket> poseBindingOwnerRows = new List<PoseBindingOwnerPacket>();
        private readonly List<PoseBindingCandidatePacket> poseBindingCandidateRows = new List<PoseBindingCandidatePacket>();
        private readonly List<TppBodyPrefabReferencePacket> tppBodyPrefabReferenceRows = new List<TppBodyPrefabReferencePacket>();
        private readonly List<TppAnimationReferencePacket> tppAnimationReferenceRows = new List<TppAnimationReferencePacket>();
        private readonly List<VHeroControllerAssetReferencePacket> vHeroControllerAssetReferenceRows = new List<VHeroControllerAssetReferencePacket>();
        private readonly List<AddressableLocatorPacket> addressableLocatorRows = new List<AddressableLocatorPacket>();
        private readonly List<AddressableLocatorPacket> addressableSuccessfulDirectLocateRows = new List<AddressableLocatorPacket>();
        private readonly List<AddressableRuntimeKeyDependencyPacket> addressableRuntimeKeyDependencyRows = new List<AddressableRuntimeKeyDependencyPacket>();
        private readonly List<AddressableLocatorDiagnosticPacket> addressableLocatorDiagnosticRows = new List<AddressableLocatorDiagnosticPacket>();
        private readonly HashSet<string> poseBindingCandidateKeys = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> tppReferenceKeys = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> assetReferenceIdentityKeys = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> addressableLocatorKeys = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> addressableSuccessfulDirectLocateKeys = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> addressableRuntimeKeyDependencyKeys = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> addressableLocatorDiagnosticKeys = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<int> loadedTargetAnimationAssetIds = new HashSet<int>();
        private object? currentController;
        private KandraRegistrationContractDiagnosticPacket kandraRegistrationContractDiagnostic = KandraRegistrationContractDiagnosticPacket.Blocked("not_captured");

        private RuntimeContext()
        {
        }

        public string ActiveScene { get; private set; } = string.Empty;
        public bool HeroTypeFound { get; private set; }
        public bool HeroPresent { get; private set; }
        public bool ControllerPresent { get; private set; }
        public bool BodyRootPresent { get; private set; }
        public bool TargetClipObserved { get; private set; }
        public string TargetClipBindingSource { get; private set; } = "blocked_unproven";
        public string PoseBinding { get; private set; } = "blocked_unproven:" + TargetPose + " / " + TargetClip + " not confirmed by live transport";
        public int TotalRendererCount { get; private set; }
        public int SkinnedRendererCount { get; private set; }
        public int ActiveRendererCount { get; private set; }
        public int EnabledRendererCount { get; private set; }
        public int VisibleRendererCount { get; private set; }
        public int LoadedKandraRendererCount { get; private set; }
        public int SceneKandraRendererCount { get; private set; }
        public int PlayerKandraCandidateCount { get; private set; }
        public int PlayerVisualRootCount => playerVisualRoots.Count;
        public int KandraRendererCount { get; private set; }
        public int ActiveKandraRendererCount { get; private set; }
        public int EnabledKandraRendererCount { get; private set; }
        public int HeroAnimancerOwnerCount { get; private set; }
        public int RuntimeAnimatorControllerTargetClipCandidateCount { get; private set; }
        public int TppTargetClipCandidateCount { get; private set; }
        public int AcceptedTppPoseBindingCandidateCount { get; private set; }
        public int TppBodyPrefabReferenceCount { get; private set; }
        public int TppAnimationReferenceCount { get; private set; }
        public int LoadedTargetAnimationAssetCount { get; private set; }
        public int VHeroControllerAssetReferenceIdentityCount { get; private set; }
        public int AddressableResourceLocatorMatchCount { get; private set; }
        public int AddressableFileLocatorMatchCount { get; private set; }
        public int AddressableCatalogLocatorMatchCount { get; private set; }
        public int AddressableTargetClipBundleMatchCount { get; private set; }
        public int AddressableTppBodyBundleMatchCount { get; private set; }
        public int AddressableRuntimeResourceLocatorCount { get; private set; }
        public int AddressableRuntimeResourceLocatorKeyCount { get; private set; }
        public int AddressableRuntimeResourceLocatorKeyScanTruncatedCount { get; private set; }
        public int AddressableRuntimeDirectLocateAttemptCount { get; private set; }
        public int AddressableRuntimeDirectLocateMatchCount { get; private set; }
        public int AddressableSuccessfulDirectLocateRowCount { get; private set; }
        public int AddressableSuccessfulDirectLocateRowTruncatedCount { get; private set; }
        public int AddressableRuntimeKeyDirectLocateAttemptCount { get; private set; }
        public int AddressableRuntimeKeyDirectLocateMatchCount { get; private set; }
        public int AddressableRuntimeKeyDependencyRowCount { get; private set; }
        public int AddressableRuntimeKeyDependencyRowTruncatedCount { get; private set; }
        public int AddressableCatalogFileCount { get; private set; }
        public int AddressableCatalogBundleAliasReferenceCount { get; private set; }
        public int AddressableLocatorDiagnosticRowCount { get; private set; }

        public bool LiveBodyEquipSceneObserved =>
            HeroPresent
            && ControllerPresent
            && BodyRootPresent
            && (SkinnedRendererCount > 0 || KandraRendererCount > 0);

        public string LiveObjectPath =>
            FirstLinkedKandraRenderer()?.path ??
            FirstSkinnedRenderer()?.path ??
            "unobserved:per-row-live-renderer-binding-not-captured";

        public string LiveRendererName =>
            FirstLinkedKandraRenderer()?.name ??
            FirstSkinnedRenderer()?.name ??
            "unobserved:per-row-live-renderer-binding-not-captured";

        public string LiveMeshName =>
            FirstLinkedKandraRenderer()?.meshName ??
            FirstSkinnedRenderer()?.meshName ??
            "unobserved:per-row-live-renderer-binding-not-captured";

        public string LiveMaterialNames =>
            FirstLinkedKandraRenderer()?.materialNames ??
            FirstSkinnedRenderer()?.materialNames ??
            "unobserved:per-row-live-renderer-binding-not-captured";

        public string LiveKandraIsRegisteredStatus =>
            FirstLinkedKandraRenderer()?.isRegisteredStatus ??
            "blocked_unobserved:no linked live Kandra renderer";

        public string LiveKandraTryGetMeshMemoryStatus =>
            FirstLinkedKandraRenderer()?.tryGetMeshMemoryStatus ??
            "blocked_unobserved:no linked live Kandra renderer";

        public string LiveAnimatorClipNames
        {
            get
            {
                PoseBindingCandidatePacket? accepted = poseBindingCandidateRows.FirstOrDefault(row => row.acceptedForA2KT34PoseBinding);
                if (accepted != null)
                {
                    return accepted.clipName;
                }

                AnimatorPacket? target = animatorRows.FirstOrDefault(row =>
                    ContainsClipName(row.currentClipNames, TargetClip));
                if (target != null)
                {
                    return JoinNonEmpty(target.currentClipNames, target.controllerClipNames);
                }

                AnimatorPacket? first = animatorRows.FirstOrDefault();
                return first == null
                    ? "unobserved:per-row-live-animator-binding-not-captured"
                    : JoinNonEmpty(first.currentClipNames, first.controllerClipNames);
            }
        }

        public static RuntimeContext Capture(int maxRendererRows, bool includeAddressableLocator)
        {
            var context = new RuntimeContext();
            context.ActiveScene = SceneManager.GetActiveScene().name ?? string.Empty;
            context.CaptureHeroRoots();
            context.CaptureKandraRenderers(maxRendererRows);
            context.CaptureRenderers(maxRendererRows);
            context.CaptureAnimators();
            context.CapturePoseBindings();
            context.CaptureTppBodyPrefabAndAnimationReferences();
            context.CaptureKandraRegistrationContractDiagnostic();
            if (includeAddressableLocator)
            {
                context.CaptureReadOnlyAddressableLocator();
            }

            context.UpdatePoseBindingStatus();

            return context;
        }

        public RuntimeContextPacket ToPacket() =>
            new RuntimeContextPacket
            {
                activeScene = ActiveScene,
                frameCount = Time.frameCount,
                unscaledTime = Time.unscaledTime,
                heroTypeFound = HeroTypeFound,
                heroPresent = HeroPresent,
                controllerPresent = ControllerPresent,
                bodyRootPresent = BodyRootPresent,
                liveBodyEquipSceneObserved = LiveBodyEquipSceneObserved,
                targetClipObserved = TargetClipObserved,
                targetClipBindingSource = TargetClipBindingSource,
                poseBindingStatus = TargetClipObserved ? "observed" : "blocked_unproven",
                rootCount = roots.Count,
                totalRendererCount = TotalRendererCount,
                skinnedRendererCount = SkinnedRendererCount,
                activeRendererCount = ActiveRendererCount,
                enabledRendererCount = EnabledRendererCount,
                visibleRendererCount = VisibleRendererCount,
                loadedKandraRendererCount = LoadedKandraRendererCount,
                rendererRows = rendererRows.ToArray(),
                sceneKandraRendererCount = SceneKandraRendererCount,
                playerKandraCandidateCount = PlayerKandraCandidateCount,
                playerVisualRootCount = PlayerVisualRootCount,
                kandraRendererCount = KandraRendererCount,
                activeKandraRendererCount = ActiveKandraRendererCount,
                enabledKandraRendererCount = EnabledKandraRendererCount,
                liveKandraIsRegisteredStatus = LiveKandraIsRegisteredStatus,
                liveKandraTryGetMeshMemoryStatus = LiveKandraTryGetMeshMemoryStatus,
                kandraRegistrationContractDiagnostic = kandraRegistrationContractDiagnostic,
                kandraRendererRows = kandraRendererRows.ToArray(),
                heroAnimancerOwnerCount = HeroAnimancerOwnerCount,
                runtimeAnimatorControllerTargetClipCandidateCount = RuntimeAnimatorControllerTargetClipCandidateCount,
                tppTargetClipCandidateCount = TppTargetClipCandidateCount,
                acceptedTppPoseBindingCandidateCount = AcceptedTppPoseBindingCandidateCount,
                tppBodyPrefabReferenceCount = TppBodyPrefabReferenceCount,
                tppAnimationReferenceCount = TppAnimationReferenceCount,
                loadedTargetAnimationAssetCount = LoadedTargetAnimationAssetCount,
                vHeroControllerAssetReferenceIdentityCount = VHeroControllerAssetReferenceIdentityCount,
                addressableResourceLocatorMatchCount = AddressableResourceLocatorMatchCount,
                addressableFileLocatorMatchCount = AddressableFileLocatorMatchCount,
                addressableCatalogLocatorMatchCount = AddressableCatalogLocatorMatchCount,
                addressableTargetClipBundleMatchCount = AddressableTargetClipBundleMatchCount,
                addressableTppBodyBundleMatchCount = AddressableTppBodyBundleMatchCount,
                addressableRuntimeResourceLocatorCount = AddressableRuntimeResourceLocatorCount,
                addressableRuntimeResourceLocatorKeyCount = AddressableRuntimeResourceLocatorKeyCount,
                addressableRuntimeResourceLocatorKeyScanTruncatedCount = AddressableRuntimeResourceLocatorKeyScanTruncatedCount,
                addressableRuntimeDirectLocateAttemptCount = AddressableRuntimeDirectLocateAttemptCount,
                addressableRuntimeDirectLocateMatchCount = AddressableRuntimeDirectLocateMatchCount,
                addressableSuccessfulDirectLocateRowCount = AddressableSuccessfulDirectLocateRowCount,
                addressableSuccessfulDirectLocateRowTruncatedCount = AddressableSuccessfulDirectLocateRowTruncatedCount,
                addressableRuntimeKeyDirectLocateAttemptCount = AddressableRuntimeKeyDirectLocateAttemptCount,
                addressableRuntimeKeyDirectLocateMatchCount = AddressableRuntimeKeyDirectLocateMatchCount,
                addressableRuntimeKeyDependencyRowCount = AddressableRuntimeKeyDependencyRowCount,
                addressableRuntimeKeyDependencyRowTruncatedCount = AddressableRuntimeKeyDependencyRowTruncatedCount,
                addressableCatalogFileCount = AddressableCatalogFileCount,
                addressableCatalogBundleAliasReferenceCount = AddressableCatalogBundleAliasReferenceCount,
                addressableLocatorDiagnosticRowCount = AddressableLocatorDiagnosticRowCount,
                poseBindingOwnerRows = poseBindingOwnerRows.ToArray(),
                poseBindingCandidateRows = poseBindingCandidateRows.ToArray(),
                tppBodyPrefabReferenceRows = tppBodyPrefabReferenceRows.ToArray(),
                tppAnimationReferenceRows = tppAnimationReferenceRows.ToArray(),
                vHeroControllerAssetReferenceRows = vHeroControllerAssetReferenceRows.ToArray(),
                addressableLocatorRows = addressableLocatorRows.ToArray(),
                addressableSuccessfulDirectLocateRows = addressableSuccessfulDirectLocateRows.ToArray(),
                addressableRuntimeKeyDependencyRows = addressableRuntimeKeyDependencyRows.ToArray(),
                addressableLocatorDiagnosticRows = addressableLocatorDiagnosticRows.ToArray(),
                animatorRows = animatorRows.ToArray(),
            };

        private void CaptureHeroRoots()
        {
            object? hero = GetCurrentHero(out bool heroTypeFound);
            HeroTypeFound = heroTypeFound;
            HeroPresent = hero != null;
            object? controller = hero == null ? null : ReadMember(hero, "VHeroController");
            currentController = controller;
            ControllerPresent = controller != null;
            object? bodyData = ReadMember(controller, "BodyData");

            AddTransform(AsTransform(controller));
            AddTransform(AsTransform(ReadMember(controller, "_heroBodyInstance")));
            AddTransform(AsTransform(ReadMember(controller, "fppParent")));
            AddTransform(AsTransform(ReadMember(controller, "tppParent")));
            AddTransform(AsTransform(bodyData));
            AddTransform(AsTransform(ReadMember(controller, "HeroAnimator")));
            foreach (string rootName in BodyDataRootNames)
            {
                AddTransform(ReadTransformMember(bodyData, rootName));
            }

            BodyRootPresent = roots.Count != 0;
            if (roots.Count == 0)
            {
                foreach (SkinnedMeshRenderer renderer in Resources.FindObjectsOfTypeAll<SkinnedMeshRenderer>())
                {
                    if (renderer != null && renderer.transform != null)
                    {
                        AddTransform(renderer.transform.root);
                    }
                }
            }
        }

        private void CaptureKandraRenderers(int maxRows)
        {
            HashSet<int> seen = new HashSet<int>();
            Scene activeScene = SceneManager.GetActiveScene();
            List<KandraRendererCandidate> candidates = new List<KandraRendererCandidate>();
            foreach (Component component in Resources.FindObjectsOfTypeAll<Component>())
            {
                if (!IsKandraRendererComponent(component) ||
                    component.transform == null ||
                    !seen.Add(component.GetInstanceID()))
                {
                    continue;
                }

                KandraRendererInfo info = KandraRendererInfo.From(component);
                bool inLoadedScene = IsInLoadedScene(component);
                bool inActiveScene = IsInScene(component, activeScene);
                bool playerBodyCandidate = IsPlayerBodyKandraCandidate(component, info);
                if (!inActiveScene && !playerBodyCandidate)
                {
                    continue;
                }

                if (inLoadedScene)
                {
                    LoadedKandraRendererCount++;
                }

                if (inActiveScene)
                {
                    SceneKandraRendererCount++;
                }

                if (playerBodyCandidate)
                {
                    PlayerKandraCandidateCount++;
                    AddPlayerVisualRoot(component.transform);
                    AddPlayerVisualRoot(info.RigTransform);
                    if (info.RigAnimator != null)
                    {
                        AddPlayerVisualRoot(info.RigAnimator.transform);
                    }
                }

                candidates.Add(new KandraRendererCandidate(component, info, inActiveScene, playerBodyCandidate));
            }

            foreach (KandraRendererCandidate candidate in candidates)
            {
                Component component = candidate.Component;
                KandraRendererInfo info = candidate.Info;
                bool linkedToHeroRoot = candidate.PlayerBodyCandidate ||
                    IsLinkedToCapturedRoot(component.transform) ||
                    IsLinkedToCapturedRoot(info.RigTransform) ||
                    IsLinkedToCapturedRoot(info.RigAnimator == null ? null : info.RigAnimator.transform);
                if (linkedToHeroRoot)
                {
                    KandraRendererCount++;
                    if (component.gameObject.activeInHierarchy) ActiveKandraRendererCount++;
                    if (info.Enabled) EnabledKandraRendererCount++;
                    AddTransform(component.transform);
                    AddTransform(info.RigTransform);
                    if (info.RigAnimator != null)
                    {
                        AddTransform(info.RigAnimator.transform);
                    }
                }

                if (kandraRendererRows.Count < maxRows)
                {
                    kandraRendererRows.Add(KandraRendererPacket.From(
                        component,
                        linkedToHeroRoot,
                        candidate.PlayerBodyCandidate,
                        BuildKandraLinkReason(component, info, linkedToHeroRoot, candidate.PlayerBodyCandidate, candidate.InActiveScene),
                        info.Enabled,
                        info.RenderingId,
                        info.MeshName,
                        info.MaterialNames,
                        info.RigTransform,
                        info.RigAnimator,
                        info.IsRegisteredStatus,
                        info.TryGetMeshMemoryStatus,
                        info.MeshMemoryText));
                }
            }

            BodyRootPresent = roots.Count != 0;
        }

        private void CaptureKandraRegistrationContractDiagnostic()
        {
            kandraRegistrationContractDiagnostic = KandraRegistrationContractDiagnosticPacket.Capture();
        }

        private void CaptureRenderers(int maxRows)
        {
            HashSet<int> seen = new HashSet<int>();
            foreach (Transform root in roots)
            {
                if (root == null)
                {
                    continue;
                }

                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer == null || !seen.Add(renderer.GetInstanceID()))
                    {
                        continue;
                    }

                    TotalRendererCount++;
                    if (renderer is SkinnedMeshRenderer)
                    {
                        SkinnedRendererCount++;
                    }

                    if (renderer.gameObject.activeInHierarchy) ActiveRendererCount++;
                    if (renderer.enabled) EnabledRendererCount++;
                    if (renderer.isVisible) VisibleRendererCount++;
                    if (rendererRows.Count < maxRows)
                    {
                        rendererRows.Add(RendererPacket.From(renderer, root));
                    }
                }
            }
        }

        private KandraRendererPacket? FirstLinkedKandraRenderer() =>
            kandraRendererRows.FirstOrDefault(row => row.linkedToCapturedHeroRoot);

        private RendererPacket? FirstSkinnedRenderer() =>
            rendererRows.FirstOrDefault(row => row.isSkinnedRenderer);

        private bool IsLinkedToCapturedRoot(Transform? transform)
        {
            if (transform == null)
            {
                return false;
            }

            foreach (Transform root in roots)
            {
                if (root == null)
                {
                    continue;
                }

                if (transform == root || transform.IsChildOf(root) || root.IsChildOf(transform))
                {
                    return true;
                }
            }

            return false;
        }

        private void CaptureAnimators()
        {
            HashSet<int> seen = new HashSet<int>();
            foreach (Transform root in roots)
            {
                if (root == null)
                {
                    continue;
                }

                foreach (Animator animator in root.GetComponentsInChildren<Animator>(true))
                {
                    if (animator == null || !seen.Add(animator.GetInstanceID()))
                    {
                        continue;
                    }

                    AnimatorPacket row = AnimatorPacket.From(animator, root);
                    animatorRows.Add(row);
                    if (ContainsClipName(row.currentClipNames, TargetClip))
                    {
                        RecordPoseBindingCandidate(PoseBindingCandidatePacket.FromAnimator(
                            "current-live-Animator-clip",
                            TransformPath(animator.transform, root),
                            row,
                            acceptedForA2KT34PoseBinding: true,
                            bindingSource: "in current live Animator clip"));
                    }
                    else if (ContainsClipName(row.controllerClipNames, TargetClip))
                    {
                        RuntimeAnimatorControllerTargetClipCandidateCount++;
                        RecordPoseBindingCandidate(PoseBindingCandidatePacket.FromAnimator(
                            "diagnostic-runtime-AnimatorController-catalog",
                            TransformPath(animator.transform, root),
                            row,
                            acceptedForA2KT34PoseBinding: false,
                            bindingSource: "diagnostic-only RuntimeAnimatorController catalog; not accepted as TPP target binding"));
                    }
                }
            }
        }

        private void CapturePoseBindings()
        {
            HashSet<int> seen = new HashSet<int>();
            foreach (Component animancer in EnumerateHeroAnimancerComponents(seen))
            {
                HeroAnimancerOwnerCount++;
                if (poseBindingOwnerRows.Count < MaxPoseBindingCandidateRows)
                {
                    poseBindingOwnerRows.Add(PoseBindingOwnerPacket.From(animancer));
                }

                ScanHeroAnimancerForTargetClip(animancer);
            }
        }

        private void CaptureTppBodyPrefabAndAnimationReferences()
        {
            HashSet<object> visited = new HashSet<object>(ReferenceComparer.Instance);
            CaptureVHeroControllerTppReferences(visited);
            CaptureLoadedTppBodyReferences(visited);
            CaptureLoadedTargetAnimationAssets();
        }

        private void CaptureVHeroControllerTppReferences(HashSet<object> visited)
        {
            if (currentController == null)
            {
                return;
            }

            string ownerType = currentController.GetType().FullName ?? currentController.GetType().Name;
            string ownerPath = ObjectPath(currentController);
            foreach (FieldInfo field in GetAllInstanceFields(currentController.GetType()))
            {
                object? value;
                try
                {
                    value = field.GetValue(currentController);
                }
                catch
                {
                    continue;
                }

                if (value == null)
                {
                    continue;
                }

                bool fieldTppScoped = ContainsAny(field.Name, TppReferenceTokens);
                if (!fieldTppScoped && !IsTppReferenceObject(value))
                {
                    continue;
                }

                ScanTppReferenceValue(
                    value,
                    "vhero-controller-field",
                    ownerType,
                    ownerPath,
                    field.Name,
                    fieldTppScoped,
                    visited);
            }
        }

        private void CaptureLoadedTppBodyReferences(HashSet<object> visited)
        {
            foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (gameObject == null ||
                    !IsTppReferenceObject(gameObject) ||
                    ContainsAny(BuildGameObjectIdentityText(gameObject), NonPlayerKandraOwnerTokens))
                {
                    continue;
                }

                ScanTppReferenceValue(
                    gameObject,
                    "loaded-tpp-body-prefab-or-object",
                    gameObject.GetType().FullName ?? gameObject.GetType().Name,
                    ObjectPath(gameObject),
                    "Resources.FindObjectsOfTypeAll<GameObject>",
                    tppScoped: true,
                    visited);
            }
        }

        private void CaptureLoadedTargetAnimationAssets()
        {
            foreach (RuntimeAnimatorController controller in Resources.FindObjectsOfTypeAll<RuntimeAnimatorController>())
            {
                if (controller == null)
                {
                    continue;
                }

                ScanRuntimeAnimatorControllerReference(
                    controller,
                    "loaded-runtime-animator-controller-asset",
                    controller.GetType().FullName ?? controller.GetType().Name,
                    ObjectPath(controller),
                    "Resources.FindObjectsOfTypeAll<RuntimeAnimatorController>.animationClips",
                    acceptedTppScope: false);
            }

            foreach (AnimationClip clip in Resources.FindObjectsOfTypeAll<AnimationClip>())
            {
                if (clip == null || !IsTargetClipName(clip.name ?? string.Empty))
                {
                    continue;
                }

                RecordTppAnimationReference(
                    "loaded-animation-clip-asset",
                    clip.GetType().FullName ?? clip.GetType().Name,
                    ObjectPath(clip),
                    "Resources.FindObjectsOfTypeAll<AnimationClip>",
                    clip,
                    acceptedForA2KT34PoseBinding: false,
                    bindingSource: "loaded AnimationClip asset only; not accepted as TPP body or animation owner binding");
            }
        }

        private void CaptureReadOnlyAddressableLocator()
        {
            CaptureVHeroControllerAssetReferenceIdentities();
            CaptureVHeroRuntimeKeyDependencies();
            CaptureAddressablesResourceLocatorMatches();
            CaptureAddressablesFileLocatorMatches();
        }

        private void CaptureVHeroControllerAssetReferenceIdentities()
        {
            if (currentController == null)
            {
                return;
            }

            string ownerType = currentController.GetType().FullName ?? currentController.GetType().Name;
            string ownerPath = ObjectPath(currentController);
            foreach (FieldInfo field in GetAllInstanceFields(currentController.GetType()))
            {
                object? value = null;
                bool valueRead = false;
                try
                {
                    value = field.GetValue(currentController);
                    valueRead = true;
                }
                catch
                {
                }

                bool fieldTppScoped = ContainsAny(field.Name, TppReferenceTokens);
                bool assetReferenceScoped = IsAssetReferenceIdentity(field.FieldType) || IsAssetReferenceIdentity(value);
                if (!fieldTppScoped && !assetReferenceScoped)
                {
                    continue;
                }

                RecordVHeroControllerAssetReferenceIdentity(
                    "vhero-controller-field",
                    ownerType,
                    ownerPath,
                    field.Name,
                    field.FieldType.FullName ?? field.FieldType.Name,
                    value,
                    valueRead,
                    fieldTppScoped);
            }
        }

        private void CaptureVHeroRuntimeKeyDependencies()
        {
            List<AddressableRuntimeKeyDependencyQuery> queries = BuildVHeroRuntimeKeyDependencyQueries().ToList();
            if (queries.Count == 0)
            {
                return;
            }

            Type? addressablesType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType("UnityEngine.AddressableAssets.Addressables", throwOnError: false))
                .FirstOrDefault(type => type != null);
            object? resourceLocators = ReadMember(addressablesType, "ResourceLocators");
            if (resourceLocators == null)
            {
                return;
            }

            int locatorIndex = 0;
            foreach (object? locator in EnumerateObject(resourceLocators, 2048))
            {
                if (locator == null)
                {
                    continue;
                }

                string locatorType = locator.GetType().FullName ?? locator.GetType().Name;
                foreach (AddressableRuntimeKeyDependencyQuery query in queries)
                {
                    AddressableRuntimeKeyDirectLocateAttemptCount++;
                    List<object> locations = TryLocateAddressableLocations(locator, query.RuntimeKey);
                    bool located = locations.Count != 0;
                    if (located)
                    {
                        AddressableRuntimeKeyDirectLocateMatchCount++;
                    }

                    RecordAddressableLocatorDiagnostic(AddressableLocatorDiagnosticPacket.FromDirectLocate(
                        locatorType,
                        locatorIndex,
                        "vhero-runtime-key:" + query.SourcePath,
                        query.RuntimeKey,
                        located,
                        locations,
                        "read-only VHeroController TPP body RuntimeKey direct Locate(query, null, out locations); no asset load requested"));

                    for (int locationIndex = 0; locationIndex < locations.Count; locationIndex++)
                    {
                        RecordAddressableRuntimeKeyDependencyGraph(
                            query,
                            locatorType,
                            locatorIndex,
                            locations[locationIndex],
                            locationIndex);
                    }
                }

                locatorIndex++;
            }
        }

        private void CaptureAddressablesResourceLocatorMatches()
        {
            Type? addressablesType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType("UnityEngine.AddressableAssets.Addressables", throwOnError: false))
                .FirstOrDefault(type => type != null);
            object? resourceLocators = ReadMember(addressablesType, "ResourceLocators");
            if (resourceLocators == null)
            {
                return;
            }

            int locatorIndex = 0;
            foreach (object? locator in EnumerateObject(resourceLocators, 2048))
            {
                if (locator == null)
                {
                    continue;
                }

                string locatorType = locator.GetType().FullName ?? locator.GetType().Name;
                object? keys = ReadMember(locator, "Keys");
                List<object?> locatorKeys = EnumerateObject(keys, MaxAddressableLocatorKeysScanned + 1).ToList();
                bool keyScanTruncated = locatorKeys.Count > MaxAddressableLocatorKeysScanned;
                if (keyScanTruncated)
                {
                    locatorKeys.RemoveAt(locatorKeys.Count - 1);
                    AddressableRuntimeResourceLocatorKeyScanTruncatedCount++;
                }

                AddressableRuntimeResourceLocatorCount++;
                AddressableRuntimeResourceLocatorKeyCount += locatorKeys.Count;
                RecordAddressableLocatorDiagnostic(new AddressableLocatorDiagnosticPacket
                {
                    sourceKind = "addressables-resource-locator-summary",
                    status = "observed",
                    locatorType = locatorType,
                    locatorIndex = locatorIndex,
                    keyCount = locatorKeys.Count,
                    keyScanLimit = MaxAddressableLocatorKeysScanned,
                    keyScanTruncated = keyScanTruncated,
                    keySampleCount = Math.Min(locatorKeys.Count, MaxAddressableLocatorKeySamples),
                    keySamples = FormatAddressableKeySamples(locatorKeys),
                    discovery = "read-only runtime Addressables.ResourceLocators summary; keys were enumerated but assets were not loaded",
                });

                foreach (AddressableDirectQuery queryCandidate in BuildAddressableDirectQueries())
                {
                    AddressableRuntimeDirectLocateAttemptCount++;
                    List<object> locations = TryLocateAddressableLocations(locator, queryCandidate.Value);
                    bool located = locations.Count != 0;
                    if (located)
                    {
                        AddressableRuntimeDirectLocateMatchCount++;
                        for (int locationIndex = 0; locationIndex < locations.Count; locationIndex++)
                        {
                            object location = locations[locationIndex];
                            RecordAddressableSuccessfulDirectLocate(AddressableLocatorPacket.FromResourceLocator(
                                "addressables-resource-locator-direct-locate-location",
                                queryCandidate.Value,
                                locatorType,
                                locatorIndex,
                                locationIndex,
                                queryCandidate.Value,
                                location.GetType().FullName ?? location.GetType().Name,
                                ReadReferenceMemberString(location, "PrimaryKey"),
                                ReadReferenceMemberString(location, "InternalId"),
                                ReadReferenceMemberString(location, "ProviderId"),
                                ReadReferenceMemberString(location, "ResourceType"),
                                "read-only Addressables locator Locate(query, null, out locations) success; normalized query/key/internal-id mapping recorded; no asset load requested",
                                queryCandidate.Kind));
                        }
                    }

                    RecordAddressableLocatorDiagnostic(AddressableLocatorDiagnosticPacket.FromDirectLocate(
                        locatorType,
                        locatorIndex,
                        queryCandidate.Kind,
                        queryCandidate.Value,
                        located,
                        locations,
                        "read-only Addressables locator Locate(query, null, out locations) direct lookup; no asset load requested"));
                }

                int keyIndex = 0;
                foreach (object? key in locatorKeys)
                {
                    string keyText = FormatPacketString(FormatAddressableValue(key), 512);
                    string query = MatchAddressableLocatorQuery(keyText);
                    if (query.Length == 0)
                    {
                        keyIndex++;
                        continue;
                    }

                    List<object> locations = TryLocateAddressableLocations(locator, key);
                    if (locations.Count == 0)
                    {
                        RecordAddressableLocator(AddressableLocatorPacket.FromResourceLocator(
                            "addressables-resource-locator-key",
                            query,
                            locatorType,
                            locatorIndex,
                            keyIndex,
                            keyText,
                            locationType: string.Empty,
                            locationPrimaryKey: string.Empty,
                            locationInternalId: string.Empty,
                            locationProviderId: string.Empty,
                            locationResourceType: string.Empty,
                            discovery: "read-only Addressables.ResourceLocators key match; Locate returned no location rows"));
                    }

                    for (int locationIndex = 0; locationIndex < locations.Count; locationIndex++)
                    {
                        object location = locations[locationIndex];
                        RecordAddressableLocator(AddressableLocatorPacket.FromResourceLocator(
                            "addressables-resource-locator-location",
                            query,
                            locatorType,
                            locatorIndex,
                            keyIndex,
                            keyText,
                            location.GetType().FullName ?? location.GetType().Name,
                            ReadReferenceMemberString(location, "PrimaryKey"),
                            ReadReferenceMemberString(location, "InternalId"),
                            ReadReferenceMemberString(location, "ProviderId"),
                            ReadReferenceMemberString(location, "ResourceType"),
                            "read-only Addressables.ResourceLocators Locate match"));
                    }

                    keyIndex++;
                }

                locatorIndex++;
            }
        }

        private void CaptureAddressablesFileLocatorMatches()
        {
            string streamingAssetsPath = string.Empty;
            try
            {
                streamingAssetsPath = Application.streamingAssetsPath ?? string.Empty;
            }
            catch
            {
            }

            if (streamingAssetsPath.Length == 0)
            {
                return;
            }

            string addressablesAaRoot = Path.Combine(streamingAssetsPath, "aa");
            string addressablesBundleRoot = Path.Combine(addressablesAaRoot, "StandaloneWindows64");
            if (!Directory.Exists(addressablesBundleRoot))
            {
                RecordAddressableLocatorDiagnostic(new AddressableLocatorDiagnosticPacket
                {
                    sourceKind = "addressables-installed-layout",
                    status = "bundle-root-missing",
                    catalogSearchRoot = addressablesAaRoot,
                    discovery = "read-only installed Addressables layout check; bundle root was not present",
                });
                return;
            }

            List<AddressableBundleMatch> matchedBundles = new List<AddressableBundleMatch>();
            foreach (string bundleFileName in CurrentAddressableBundleAliasCandidates)
            {
                string bundleFile = Path.Combine(addressablesBundleRoot, bundleFileName);
                if (!File.Exists(bundleFile))
                {
                    RecordAddressableLocator(AddressableLocatorPacket.FromFile(
                        "addressables-bundle-alias-missing",
                        query: string.Empty,
                        filePath: bundleFile,
                        fileName: bundleFileName,
                        linkedBundleFileName: bundleFileName,
                        bundleBacked: false,
                        catalogBacked: false,
                        discovery: "read-only current Addressables bundle alias candidate missing in installed StreamingAssets"));
                    continue;
                }

                foreach (string query in FindUtf8TokensInFile(bundleFile, AddressableLocatorQueries))
                {
                    matchedBundles.Add(new AddressableBundleMatch(bundleFileName, query));
                    RecordAddressableLocator(AddressableLocatorPacket.FromFile(
                        "addressables-bundle-string-scan",
                        query,
                        bundleFile,
                        bundleFileName,
                        linkedBundleFileName: bundleFileName,
                        bundleBacked: true,
                        catalogBacked: false,
                        discovery: "read-only StreamingAssets addressables bundle exact string match"));
                }
            }

            foreach (string catalogFile in EnumerateAddressablesCatalogFiles(addressablesAaRoot, addressablesBundleRoot))
            {
                string catalogFileName = Path.GetFileName(catalogFile);
                AddressableCatalogFileCount++;
                bool anyDirectQueryMatch = false;
                bool anyBundleAliasMatch = false;
                foreach (string query in FindUtf8TokensInFile(catalogFile, AddressableLocatorQueries))
                {
                    anyDirectQueryMatch = true;
                    RecordAddressableLocator(AddressableLocatorPacket.FromFile(
                        "addressables-catalog-direct-string-scan",
                        query,
                        catalogFile,
                        catalogFileName,
                        linkedBundleFileName: string.Empty,
                        bundleBacked: false,
                        catalogBacked: true,
                        discovery: "read-only StreamingAssets addressables catalog exact string match"));
                }

                foreach (AddressableBundleMatch match in matchedBundles.Distinct())
                {
                    if (!FileContainsUtf8Token(catalogFile, match.BundleFileName))
                    {
                        continue;
                    }

                    anyBundleAliasMatch = true;
                    AddressableCatalogBundleAliasReferenceCount++;
                    RecordAddressableLocator(AddressableLocatorPacket.FromFile(
                        "addressables-catalog-bundle-file-reference",
                        match.Query,
                        catalogFile,
                        catalogFileName,
                        linkedBundleFileName: match.BundleFileName,
                        bundleBacked: false,
                        catalogBacked: true,
                        discovery: "read-only StreamingAssets addressables catalog references matched bundle file"));
                }

                RecordAddressableLocatorDiagnostic(AddressableLocatorDiagnosticPacket.FromCatalogFile(
                    catalogFile,
                    catalogFileName,
                    addressablesAaRoot,
                    anyDirectQueryMatch,
                    anyBundleAliasMatch,
                    matchedBundles.Select(match => match.BundleFileName).Distinct(StringComparer.Ordinal),
                    "read-only installed Addressables catalog scan; catalog file was not loaded through Addressables and no assets were loaded"));
            }
        }

        private void ScanTppReferenceValue(
            object value,
            string sourceKind,
            string ownerType,
            string ownerPath,
            string sourcePath,
            bool tppScoped,
            HashSet<object> visited)
        {
            Queue<TppReferenceGraphNode> queue = new Queue<TppReferenceGraphNode>();
            queue.Enqueue(new TppReferenceGraphNode(value, sourcePath, 0, tppScoped));
            int nodes = 0;

            while (queue.Count > 0 && nodes < MaxTppReferenceGraphNodes)
            {
                TppReferenceGraphNode node = queue.Dequeue();
                object current = node.Value;
                if (IsSimplePoseBindingValue(current) || !visited.Add(current))
                {
                    continue;
                }

                nodes++;
                bool currentTppScoped = node.TppScoped ||
                    ContainsAny(node.Path, TppReferenceTokens) ||
                    IsTppReferenceObject(current);

                if (current is AnimationClip clip)
                {
                    RecordTppAnimationReference(
                        sourceKind,
                        ownerType,
                        ownerPath,
                        node.Path,
                        clip,
                        acceptedForA2KT34PoseBinding: currentTppScoped && IsTppTargetClipName(clip.name ?? string.Empty),
                        bindingSource: currentTppScoped
                            ? "read-only TPP-scoped animation reference at " + node.Path
                            : "read-only animation reference at " + node.Path + "; not accepted without TPP scope");
                    continue;
                }

                if (current is RuntimeAnimatorController controller)
                {
                    ScanRuntimeAnimatorControllerReference(
                        controller,
                        sourceKind + "-runtime-animator-controller",
                        ownerType,
                        ownerPath,
                        node.Path + ".animationClips",
                        acceptedTppScope: currentTppScoped);
                    continue;
                }

                if (current is Animator animator)
                {
                    ScanAnimatorReference(
                        animator,
                        sourceKind + "-animator",
                        ownerType,
                        ownerPath,
                        node.Path,
                        currentTppScoped);
                }

                if (current is GameObject gameObject)
                {
                    if (currentTppScoped)
                    {
                        RecordTppBodyPrefabReference(
                            sourceKind,
                            ownerType,
                            ownerPath,
                            node.Path,
                            gameObject,
                            "read-only TPP GameObject reference");
                    }

                    EnqueueGameObjectComponents(queue, gameObject, node.Path, node.Depth, currentTppScoped);
                    continue;
                }

                if (current is Component component)
                {
                    if (currentTppScoped && component.gameObject != null)
                    {
                        RecordTppBodyPrefabReference(
                            sourceKind,
                            ownerType,
                            ownerPath,
                            node.Path,
                            component.gameObject,
                            "read-only TPP Component owner reference");
                    }
                }

                if (node.Depth >= MaxTppReferenceGraphDepth || !ShouldTraverseTppReferenceObject(current))
                {
                    continue;
                }

                int itemIndex = 0;
                foreach (object? item in EnumerateObject(current, 256))
                {
                    if (item != null)
                    {
                        queue.Enqueue(new TppReferenceGraphNode(
                            item,
                            node.Path + "[" + itemIndex.ToString(CultureInfo.InvariantCulture) + "]",
                            node.Depth + 1,
                            currentTppScoped));
                    }

                    itemIndex++;
                }

                foreach (FieldInfo field in GetAllInstanceFields(current.GetType()))
                {
                    object? fieldValue;
                    try
                    {
                        fieldValue = field.GetValue(current);
                    }
                    catch
                    {
                        continue;
                    }

                    if (fieldValue != null)
                    {
                        queue.Enqueue(new TppReferenceGraphNode(
                            fieldValue,
                            node.Path + "." + field.Name,
                            node.Depth + 1,
                            currentTppScoped || ContainsAny(field.Name, TppReferenceTokens)));
                    }
                }
            }
        }

        private void EnqueueGameObjectComponents(
            Queue<TppReferenceGraphNode> queue,
            GameObject gameObject,
            string sourcePath,
            int depth,
            bool tppScoped)
        {
            if (depth >= MaxTppReferenceGraphDepth)
            {
                return;
            }

            Component[] components;
            try
            {
                components = gameObject.GetComponentsInChildren<Component>(true);
            }
            catch
            {
                return;
            }

            for (int index = 0; index < components.Length && index < 512; index++)
            {
                Component component = components[index];
                if (component == null)
                {
                    continue;
                }

                queue.Enqueue(new TppReferenceGraphNode(
                    component,
                    sourcePath + "." + component.GetType().Name + "[" + index.ToString(CultureInfo.InvariantCulture) + "]",
                    depth + 1,
                    tppScoped || IsTppReferenceObject(component)));
            }
        }

        private void ScanAnimatorReference(
            Animator animator,
            string sourceKind,
            string ownerType,
            string ownerPath,
            string sourcePath,
            bool acceptedTppScope)
        {
            RuntimeAnimatorController? controller = null;
            try
            {
                controller = animator.runtimeAnimatorController;
            }
            catch
            {
            }

            if (controller == null)
            {
                return;
            }

            ScanRuntimeAnimatorControllerReference(
                controller,
                sourceKind + "-runtime-animator-controller",
                ownerType,
                ownerPath,
                sourcePath + ".runtimeAnimatorController.animationClips",
                acceptedTppScope);
        }

        private void ScanRuntimeAnimatorControllerReference(
            RuntimeAnimatorController controller,
            string sourceKind,
            string ownerType,
            string ownerPath,
            string sourcePath,
            bool acceptedTppScope)
        {
            foreach (AnimationClip clip in SafeAnimationClips(controller))
            {
                if (!IsTargetClipName(clip.name ?? string.Empty))
                {
                    continue;
                }

                RecordTppAnimationReference(
                    sourceKind,
                    ownerType,
                    ownerPath,
                    sourcePath,
                    clip,
                    acceptedForA2KT34PoseBinding: acceptedTppScope && IsTppTargetClipName(clip.name ?? string.Empty),
                    bindingSource: acceptedTppScope
                        ? "read-only TPP-scoped RuntimeAnimatorController animation reference at " + sourcePath
                        : "loaded RuntimeAnimatorController catalog only; not accepted as TPP body or animation owner binding");
            }
        }

        private void RecordTppBodyPrefabReference(
            string sourceKind,
            string ownerType,
            string ownerPath,
            string sourcePath,
            GameObject gameObject,
            string discovery)
        {
            string key = "body|" + sourceKind + "|" + ownerPath + "|" + sourcePath + "|" + ObjectPath(gameObject);
            if (!tppReferenceKeys.Add(key))
            {
                return;
            }

            TppBodyPrefabReferenceCount++;
            if (tppBodyPrefabReferenceRows.Count < MaxTppReferenceRows)
            {
                tppBodyPrefabReferenceRows.Add(TppBodyPrefabReferencePacket.From(
                    sourceKind,
                    ownerType,
                    ownerPath,
                    sourcePath,
                    gameObject,
                    discovery));
            }
        }

        private void RecordTppAnimationReference(
            string sourceKind,
            string ownerType,
            string ownerPath,
            string sourcePath,
            AnimationClip clip,
            bool acceptedForA2KT34PoseBinding,
            string bindingSource)
        {
            string clipName = clip.name ?? string.Empty;
            if (!IsTargetClipName(clipName))
            {
                return;
            }

            string key = "animation|" + sourceKind + "|" + ownerPath + "|" + sourcePath + "|" + clipName;
            if (!tppReferenceKeys.Add(key))
            {
                return;
            }

            TppAnimationReferenceCount++;
            if (sourceKind.StartsWith("loaded-", StringComparison.Ordinal) &&
                loadedTargetAnimationAssetIds.Add(clip.GetInstanceID()))
            {
                LoadedTargetAnimationAssetCount++;
            }

            if (tppAnimationReferenceRows.Count < MaxTppReferenceRows)
            {
                tppAnimationReferenceRows.Add(TppAnimationReferencePacket.From(
                    sourceKind,
                    ownerType,
                    ownerPath,
                    sourcePath,
                    clip,
                    acceptedForA2KT34PoseBinding,
                    bindingSource));
            }

            RecordPoseBindingCandidate(PoseBindingCandidatePacket.FromAnimationReference(
                sourceKind,
                ownerType,
                ownerPath,
                sourcePath,
                clip,
                acceptedForA2KT34PoseBinding,
                bindingSource));
        }

        private void RecordVHeroControllerAssetReferenceIdentity(
            string sourceKind,
            string ownerType,
            string ownerPath,
            string sourcePath,
            string declaredType,
            object? value,
            bool valueRead,
            bool tppScoped)
        {
            string runtimeKey = ReadReferenceMemberString(value, "RuntimeKey");
            string assetGuid = FirstNonEmpty(
                ReadReferenceMemberString(value, "AssetGUID"),
                ReadReferenceMemberString(value, "AssetGuid"),
                ReadReferenceMemberString(value, "Guid"),
                ReadReferenceMemberString(value, "guid"),
                ReadReferenceMemberString(value, "_guid"),
                ReadReferenceMemberString(value, "m_AssetGUID"));
            string subObjectName = ReadReferenceMemberString(value, "SubObjectName");
            string address = ReadReferenceMemberString(value, "Address");
            string primaryKey = ReadReferenceMemberString(value, "PrimaryKey");
            string label = ReadReferenceMemberString(value, "Label");
            string asset = ReadReferenceMemberString(value, "Asset");
            string candidateMemberValues = FormatReferenceMemberValues(value);
            string rawText = FormatPacketString(FormatAddressableValue(value), 512);
            string matchedQuery = MatchAddressableLocatorQuery(string.Join(
                "|",
                sourcePath,
                declaredType,
                runtimeKey,
                assetGuid,
                subObjectName,
                address,
                primaryKey,
                label,
                asset,
                rawText,
                candidateMemberValues));
            string key = string.Join(
                "|",
                sourceKind,
                ownerPath,
                sourcePath,
                declaredType,
                runtimeKey,
                assetGuid,
                subObjectName,
                address,
                primaryKey,
                label,
                rawText);
            if (!assetReferenceIdentityKeys.Add(key))
            {
                return;
            }

            VHeroControllerAssetReferenceIdentityCount++;
            if (vHeroControllerAssetReferenceRows.Count < MaxAssetReferenceIdentityRows)
            {
                vHeroControllerAssetReferenceRows.Add(VHeroControllerAssetReferencePacket.From(
                    sourceKind,
                    ownerType,
                    ownerPath,
                    sourcePath,
                    declaredType,
                    value,
                    valueRead,
                    tppScoped,
                    runtimeKey,
                    assetGuid,
                    subObjectName,
                    address,
                    primaryKey,
                    label,
                    asset,
                    rawText,
                    candidateMemberValues,
                    matchedQuery));
            }
        }

        private IEnumerable<AddressableRuntimeKeyDependencyQuery> BuildVHeroRuntimeKeyDependencyQueries()
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (VHeroControllerAssetReferencePacket row in vHeroControllerAssetReferenceRows)
            {
                if (!IsVHeroTppBodyRuntimeKeySource(row.sourcePath) || string.IsNullOrWhiteSpace(row.runtimeKey))
                {
                    continue;
                }

                string key = row.sourcePath + "|" + row.runtimeKey;
                if (!seen.Add(key))
                {
                    continue;
                }

                yield return new AddressableRuntimeKeyDependencyQuery(
                    row.sourceKind,
                    row.ownerType,
                    row.ownerPath,
                    row.sourcePath,
                    row.runtimeKey,
                    row.address,
                    row.assetGuid,
                    row.primaryKey);
            }
        }

        private static bool IsVHeroTppBodyRuntimeKeySource(string sourcePath) =>
            string.Equals(sourcePath, "maleHeroBodyTPP", StringComparison.Ordinal) ||
            string.Equals(sourcePath, "femaleHeroBodyTPP", StringComparison.Ordinal);

        private void RecordAddressableRuntimeKeyDependencyGraph(
            AddressableRuntimeKeyDependencyQuery query,
            string locatorType,
            int locatorIndex,
            object rootLocation,
            int rootLocationIndex)
        {
            Queue<AddressableRuntimeKeyDependencyGraphNode> queue = new Queue<AddressableRuntimeKeyDependencyGraphNode>();
            HashSet<object> visited = new HashSet<object>(ReferenceComparer.Instance);
            queue.Enqueue(new AddressableRuntimeKeyDependencyGraphNode(
                rootLocation,
                rootLocationIndex,
                0,
                rootLocation: true,
                parentPrimaryKey: string.Empty,
                parentInternalId: string.Empty,
                parentProviderId: string.Empty,
                parentResourceType: string.Empty));

            int nodes = 0;
            while (queue.Count != 0 && nodes < MaxAddressableRuntimeKeyDependencyGraphNodes)
            {
                AddressableRuntimeKeyDependencyGraphNode node = queue.Dequeue();
                object location = node.Location;
                if (!visited.Add(location))
                {
                    continue;
                }

                nodes++;
                string primaryKey = ReadReferenceMemberString(location, "PrimaryKey");
                string internalId = ReadReferenceMemberString(location, "InternalId");
                string providerId = ReadReferenceMemberString(location, "ProviderId");
                string resourceType = ReadReferenceMemberString(location, "ResourceType");
                RecordAddressableRuntimeKeyDependency(AddressableRuntimeKeyDependencyPacket.FromRuntimeKeyDependency(
                    query.SourceKind,
                    query.OwnerType,
                    query.OwnerPath,
                    query.SourcePath,
                    query.RuntimeKey,
                    query.Address,
                    query.AssetGuid,
                    query.PrimaryKey,
                    locatorType,
                    locatorIndex,
                    node.LocationIndex,
                    node.Depth,
                    node.RootLocation,
                    location.GetType().FullName ?? location.GetType().Name,
                    primaryKey,
                    internalId,
                    providerId,
                    resourceType,
                    node.ParentPrimaryKey,
                    node.ParentInternalId,
                    node.ParentProviderId,
                    node.ParentResourceType,
                    "read-only VHeroController TPP body RuntimeKey location/dependency identity; no asset load, instantiate, activate, or mutation requested"));

                if (node.Depth >= MaxAddressableRuntimeKeyDependencyGraphDepth)
                {
                    continue;
                }

                object? dependencies = ReadMember(location, "Dependencies");
                int dependencyIndex = 0;
                foreach (object? dependency in EnumerateObject(dependencies, MaxAddressableRuntimeKeyDependencyGraphNodes))
                {
                    if (dependency == null)
                    {
                        dependencyIndex++;
                        continue;
                    }

                    queue.Enqueue(new AddressableRuntimeKeyDependencyGraphNode(
                        dependency,
                        dependencyIndex,
                        node.Depth + 1,
                        rootLocation: false,
                        parentPrimaryKey: primaryKey,
                        parentInternalId: internalId,
                        parentProviderId: providerId,
                        parentResourceType: resourceType));
                    dependencyIndex++;
                }
            }
        }

        private void RecordAddressableLocator(AddressableLocatorPacket row)
        {
            string key = string.Join(
                "|",
                row.sourceKind,
                row.query,
                row.locatorType,
                row.keyText,
                row.locationPrimaryKey,
                row.locationInternalId,
                row.filePath,
                row.linkedBundleFileName);
            if (!addressableLocatorKeys.Add(key))
            {
                return;
            }

            if (row.sourceKind.IndexOf("resource-locator", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                AddressableResourceLocatorMatchCount++;
            }

            if (row.sourceKind.IndexOf("bundle", StringComparison.OrdinalIgnoreCase) >= 0 ||
                row.sourceKind.IndexOf("catalog", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                AddressableFileLocatorMatchCount++;
            }

            if (row.catalogBacked)
            {
                AddressableCatalogLocatorMatchCount++;
            }

            if (row.bundleBacked && row.targetClipExact)
            {
                AddressableTargetClipBundleMatchCount++;
            }

            if (row.bundleBacked && row.tppBodyReferenceName)
            {
                AddressableTppBodyBundleMatchCount++;
            }

            if (addressableLocatorRows.Count < MaxAddressableLocatorRows)
            {
                addressableLocatorRows.Add(row);
            }
        }

        private void RecordAddressableSuccessfulDirectLocate(AddressableLocatorPacket row)
        {
            string key = string.Join(
                "|",
                row.sourceKind,
                row.queryKind,
                row.query,
                row.locatorType,
                row.locatorIndex.ToString(CultureInfo.InvariantCulture),
                row.keyIndex.ToString(CultureInfo.InvariantCulture),
                row.locationPrimaryKey,
                row.locationInternalId,
                row.locationProviderId,
                row.locationResourceType);
            if (!addressableSuccessfulDirectLocateKeys.Add(key))
            {
                return;
            }

            AddressableSuccessfulDirectLocateRowCount++;
            RecordAddressableLocator(row);
            if (addressableSuccessfulDirectLocateRows.Count < MaxAddressableSuccessfulDirectLocateRows)
            {
                addressableSuccessfulDirectLocateRows.Add(row);
            }
            else
            {
                AddressableSuccessfulDirectLocateRowTruncatedCount++;
            }
        }

        private void RecordAddressableRuntimeKeyDependency(AddressableRuntimeKeyDependencyPacket row)
        {
            string key = string.Join(
                "|",
                row.sourceKind,
                row.vHeroSourcePath,
                row.vHeroRuntimeKey,
                row.locatorType,
                row.locatorIndex.ToString(CultureInfo.InvariantCulture),
                row.depth.ToString(CultureInfo.InvariantCulture),
                row.locationIndex.ToString(CultureInfo.InvariantCulture),
                row.primaryKey,
                row.internalId,
                row.providerId,
                row.resourceType,
                row.parentPrimaryKey,
                row.parentInternalId,
                row.parentProviderId,
                row.parentResourceType);
            if (!addressableRuntimeKeyDependencyKeys.Add(key))
            {
                return;
            }

            AddressableRuntimeKeyDependencyRowCount++;
            if (addressableRuntimeKeyDependencyRows.Count < MaxAddressableRuntimeKeyDependencyRows)
            {
                addressableRuntimeKeyDependencyRows.Add(row);
            }
            else
            {
                AddressableRuntimeKeyDependencyRowTruncatedCount++;
            }
        }

        private void RecordAddressableLocatorDiagnostic(AddressableLocatorDiagnosticPacket row)
        {
            string key = string.Join(
                "|",
                row.sourceKind,
                row.locatorType,
                row.locatorIndex.ToString(CultureInfo.InvariantCulture),
                row.queryKind,
                row.query,
                row.catalogPath,
                row.matchedBundleAliases,
                row.status);
            if (!addressableLocatorDiagnosticKeys.Add(key))
            {
                return;
            }

            AddressableLocatorDiagnosticRowCount++;
            if (addressableLocatorDiagnosticRows.Count < MaxAddressableLocatorDiagnosticRows)
            {
                addressableLocatorDiagnosticRows.Add(row);
            }
        }

        private static IEnumerable<AddressableDirectQuery> BuildAddressableDirectQueries()
        {
            foreach (string query in AddressableLocatorQueries)
            {
                yield return new AddressableDirectQuery("asset-name", query);
            }

            foreach (string bundleAlias in CurrentAddressableBundleAliasCandidates)
            {
                yield return new AddressableDirectQuery("bundle-alias", bundleAlias);
                yield return new AddressableDirectQuery(
                    "bundle-alias-with-platform",
                    "StandaloneWindows64/" + bundleAlias);
                yield return new AddressableDirectQuery(
                    "bundle-alias-with-aa-platform",
                    "aa/StandaloneWindows64/" + bundleAlias);
            }
        }

        private static string FormatAddressableKeySamples(IEnumerable<object?> keys)
        {
            return string.Join(
                "|",
                keys
                    .Take(MaxAddressableLocatorKeySamples)
                    .Select(key => FormatPacketString(FormatAddressableValue(key), 160)));
        }

        private static string FormatLocationValues(IEnumerable<object> locations, string memberName)
        {
            return string.Join(
                "|",
                locations
                    .Take(8)
                    .Select(location => ReadReferenceMemberString(location, memberName))
                    .Where(value => value.Length != 0)
                    .Select(value => FormatPacketString(value, 192)));
        }

        private static string FormatLocationTypes(IEnumerable<object> locations)
        {
            return string.Join(
                "|",
                locations
                    .Take(8)
                    .Select(location => location.GetType().FullName ?? location.GetType().Name));
        }

        private static bool IsAssetReferenceIdentity(object? value) =>
            value != null && IsAssetReferenceIdentity(value.GetType());

        private static bool IsAssetReferenceIdentity(Type? type)
        {
            if (type == null)
            {
                return false;
            }

            string typeName = type.FullName ?? type.Name;
            return typeName.IndexOf("AssetReference", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   typeName.IndexOf("ARAssetReference", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string FormatReferenceMemberValues(object? value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            List<string> values = new List<string>();
            foreach (string memberName in AssetReferenceMemberNames)
            {
                string memberValue = ReadReferenceMemberString(value, memberName);
                if (memberValue.Length != 0)
                {
                    values.Add(memberName + "=" + FormatPacketString(memberValue, 192));
                }
            }

            return string.Join(";", values);
        }

        private static string ReadReferenceMemberString(object? value, string memberName)
        {
            object? member = ReadMember(value, memberName);
            if (member == null)
            {
                return string.Empty;
            }

            return FormatPacketString(FormatAddressableValue(member), 512);
        }

        private static string FirstNonEmpty(params string[] values) =>
            values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

        private static string FormatAddressableValue(object? value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is Object unityObject)
            {
                return (unityObject.GetType().FullName ?? unityObject.GetType().Name) + ":" + (unityObject.name ?? string.Empty);
            }

            if (value is Type type)
            {
                return type.FullName ?? type.Name;
            }

            if (IsSimplePoseBindingValue(value))
            {
                return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            }

            string typeName = value.GetType().FullName ?? value.GetType().Name;
            string text;
            try
            {
                text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            }
            catch
            {
                text = string.Empty;
            }

            return text.Length == 0 || string.Equals(text, typeName, StringComparison.Ordinal)
                ? typeName
                : typeName + ":" + text;
        }

        private static string FormatPacketString(string? value, int maxLength = 256)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            string normalized = value.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
            return normalized.Length <= maxLength ? normalized : normalized.Substring(0, maxLength);
        }

        private static string MatchAddressableLocatorQuery(string value) =>
            AddressableLocatorQueries.FirstOrDefault(query =>
                value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ?? string.Empty;

        private static List<object> TryLocateAddressableLocations(object locator, object? key)
        {
            List<object> locations = new List<object>();
            MethodInfo? locate = locator.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(method =>
                {
                    if (!string.Equals(method.Name, "Locate", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    return parameters.Length == 3 && parameters[2].IsOut;
                });
            if (locate == null)
            {
                return locations;
            }

            object?[] arguments = { key, null, null };
            try
            {
                object? result = locate.Invoke(locator, arguments);
                if (result is bool located && !located)
                {
                    return locations;
                }
            }
            catch
            {
                return locations;
            }

            if (arguments[2] is IEnumerable enumerable)
            {
                foreach (object? location in enumerable)
                {
                    if (location != null)
                    {
                        locations.Add(location);
                    }
                }
            }

            return locations;
        }

        private static IEnumerable<string> EnumerateAddressablesCatalogFiles(
            string addressablesAaRoot,
            string addressablesBundleRoot)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string catalogName in AddressableCatalogFileNames)
            {
                string rootCatalog = Path.Combine(addressablesAaRoot, catalogName);
                if (File.Exists(rootCatalog) && seen.Add(rootCatalog))
                {
                    yield return rootCatalog;
                }

                string bundleRootCatalog = Path.Combine(addressablesBundleRoot, catalogName);
                if (File.Exists(bundleRootCatalog) && seen.Add(bundleRootCatalog))
                {
                    yield return bundleRootCatalog;
                }
            }

            foreach (string root in new[] { addressablesAaRoot, addressablesBundleRoot })
            {
                IEnumerable<string> catalogFiles;
                try
                {
                    catalogFiles = Directory.Exists(root)
                        ? Directory.EnumerateFiles(root, "catalog*.json", SearchOption.TopDirectoryOnly)
                        : Array.Empty<string>();
                }
                catch
                {
                    catalogFiles = Array.Empty<string>();
                }

                foreach (string catalogFile in catalogFiles)
                {
                    if (seen.Add(catalogFile))
                    {
                        yield return catalogFile;
                    }
                }
            }
        }

        private static string[] FindUtf8TokensInFile(string filePath, IEnumerable<string> tokens)
        {
            List<string> matches = new List<string>();
            Dictionary<string, byte[]> pending = tokens
                .Where(token => !string.IsNullOrEmpty(token))
                .Distinct(StringComparer.Ordinal)
                .ToDictionary(token => token, Encoding.UTF8.GetBytes, StringComparer.Ordinal);
            if (pending.Count == 0)
            {
                return Array.Empty<string>();
            }

            int maxPatternLength = pending.Values.Max(pattern => pattern.Length);
            byte[] buffer = new byte[AddressableFileReadBufferSize + Math.Max(0, maxPatternLength - 1)];
            int carry = 0;
            try
            {
                using FileStream stream = File.OpenRead(filePath);
                while (pending.Count > 0)
                {
                    int read = stream.Read(buffer, carry, AddressableFileReadBufferSize);
                    if (read <= 0)
                    {
                        break;
                    }

                    int total = carry + read;
                    foreach (KeyValuePair<string, byte[]> candidate in pending.ToArray())
                    {
                        if (ContainsBytePattern(buffer, total, candidate.Value))
                        {
                            matches.Add(candidate.Key);
                            pending.Remove(candidate.Key);
                        }
                    }

                    carry = Math.Min(Math.Max(0, maxPatternLength - 1), total);
                    if (carry > 0)
                    {
                        Array.Copy(buffer, total - carry, buffer, 0, carry);
                    }
                }
            }
            catch
            {
            }

            return matches.ToArray();
        }

        private static bool FileContainsUtf8Token(string filePath, string token) =>
            FindUtf8TokensInFile(filePath, new[] { token }).Length != 0;

        private static bool ContainsBytePattern(byte[] buffer, int length, byte[] pattern)
        {
            if (pattern.Length == 0 || length < pattern.Length)
            {
                return false;
            }

            int lastStart = length - pattern.Length;
            for (int index = 0; index <= lastStart; index++)
            {
                bool matched = true;
                for (int patternIndex = 0; patternIndex < pattern.Length; patternIndex++)
                {
                    if (buffer[index + patternIndex] != pattern[patternIndex])
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched)
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<Component> EnumerateHeroAnimancerComponents(HashSet<int> seen)
        {
            foreach (Transform root in roots)
            {
                if (root == null)
                {
                    continue;
                }

                Component[] components;
                try
                {
                    components = root.GetComponentsInChildren<Component>(true);
                }
                catch
                {
                    continue;
                }

                foreach (Component component in components)
                {
                    if (IsHeroAnimancerComponent(component) && seen.Add(component.GetInstanceID()))
                    {
                        yield return component;
                    }
                }
            }

            if (currentController is Component controllerComponent && controllerComponent.transform != null)
            {
                Component[] components;
                try
                {
                    components = controllerComponent.transform.GetComponentsInChildren<Component>(true);
                }
                catch
                {
                    yield break;
                }

                foreach (Component component in components)
                {
                    if (IsHeroAnimancerComponent(component) && seen.Add(component.GetInstanceID()))
                    {
                        yield return component;
                    }
                }
            }

            foreach (Component component in Resources.FindObjectsOfTypeAll<Component>())
            {
                if (component == null ||
                    !IsHeroAnimancerComponent(component) ||
                    !IsPlayerTppPoseBindingOwner(component) ||
                    !seen.Add(component.GetInstanceID()))
                {
                    continue;
                }

                yield return component;
            }
        }

        private void ScanHeroAnimancerForTargetClip(Component animancer)
        {
            Transform? ownerRoot = animancer.transform == null ? null : animancer.transform.root;
            string ownerPath = TransformPath(animancer.transform, ownerRoot);
            string ownerType = animancer.GetType().FullName ?? animancer.GetType().Name;
            Queue<PoseBindingGraphNode> queue = new Queue<PoseBindingGraphNode>();
            HashSet<object> visited = new HashSet<object>(ReferenceComparer.Instance);
            queue.Enqueue(new PoseBindingGraphNode(animancer, "ARHeroAnimancer", 0));
            int nodes = 0;

            while (queue.Count > 0 && nodes < MaxPoseBindingGraphNodes)
            {
                PoseBindingGraphNode node = queue.Dequeue();
                object value = node.Value;
                if (!visited.Add(value))
                {
                    continue;
                }

                nodes++;
                if (value is AnimationClip clip)
                {
                    RecordHeroAnimancerClip(animancer, ownerType, ownerPath, node.Path, clip);
                    continue;
                }

                if (node.Depth >= MaxPoseBindingGraphDepth ||
                    IsSimplePoseBindingValue(value) ||
                    value is Transform ||
                    value is GameObject)
                {
                    continue;
                }

                int itemIndex = 0;
                foreach (object? item in EnumerateObject(value, 2048))
                {
                    if (item == null)
                    {
                        itemIndex++;
                        continue;
                    }

                    string itemPath = node.Path + "[" + itemIndex.ToString(CultureInfo.InvariantCulture) + "]";
                    if (item is AnimationClip itemClip)
                    {
                        RecordHeroAnimancerClip(animancer, ownerType, ownerPath, itemPath, itemClip);
                    }
                    else if (ShouldTraversePoseBindingObject(item))
                    {
                        queue.Enqueue(new PoseBindingGraphNode(item, itemPath, node.Depth + 1));
                    }

                    itemIndex++;
                }

                foreach (FieldInfo field in GetAllInstanceFields(value.GetType()))
                {
                    object? fieldValue;
                    try
                    {
                        fieldValue = field.GetValue(value);
                    }
                    catch
                    {
                        continue;
                    }

                    if (fieldValue == null)
                    {
                        continue;
                    }

                    string fieldPath = node.Path + "." + field.Name;
                    if (fieldValue is AnimationClip fieldClip)
                    {
                        RecordHeroAnimancerClip(animancer, ownerType, ownerPath, fieldPath, fieldClip);
                    }
                    else if (ShouldTraversePoseBindingObject(fieldValue))
                    {
                        queue.Enqueue(new PoseBindingGraphNode(fieldValue, fieldPath, node.Depth + 1));
                    }
                }
            }
        }

        private void RecordHeroAnimancerClip(
            Component owner,
            string ownerType,
            string ownerPath,
            string sourcePath,
            AnimationClip clip)
        {
            string clipName = clip.name ?? string.Empty;
            if (!IsTargetClipName(clipName))
            {
                return;
            }

            RecordPoseBindingCandidate(PoseBindingCandidatePacket.FromHeroAnimancer(
                owner,
                ownerType,
                ownerPath,
                sourcePath,
                clip,
                acceptedForA2KT34PoseBinding: IsTppTargetClipName(clipName),
                bindingSource: "bound to live ARHeroAnimancer owner graph at " + sourcePath));
        }

        private void RecordPoseBindingCandidate(PoseBindingCandidatePacket row)
        {
            string key = row.sourceKind + "|" + row.ownerPath + "|" + row.sourcePath + "|" + row.clipName;
            if (!poseBindingCandidateKeys.Add(key))
            {
                return;
            }

            if (row.tppTargetClipName)
            {
                TppTargetClipCandidateCount++;
            }

            if (row.acceptedForA2KT34PoseBinding)
            {
                AcceptedTppPoseBindingCandidateCount++;
                ObserveTargetClip(row.bindingSource);
            }

            if (poseBindingCandidateRows.Count < MaxPoseBindingCandidateRows)
            {
                poseBindingCandidateRows.Add(row);
            }
        }

        private void UpdatePoseBindingStatus()
        {
            PoseBinding = TargetClipObserved
                ? "observed:" + TargetPose + " / " + TargetClip + " " + TargetClipBindingSource
                : "blocked_unproven:tpp-target-animation-clip-binding / " + TargetPose + " / " + TargetClip +
                  " not confirmed by current live Animator playback or live ARHeroAnimancer owner graph; fppBodyEquipSceneObserved=" +
                  LiveBodyEquipSceneObserved.ToString(CultureInfo.InvariantCulture);
        }

        private void ObserveTargetClip(string source)
        {
            TargetClipObserved = true;
            if (source.IndexOf("current live Animator", StringComparison.OrdinalIgnoreCase) >= 0 ||
                string.Equals(TargetClipBindingSource, "blocked_unproven", StringComparison.Ordinal))
            {
                TargetClipBindingSource = source;
            }
        }

        private void AddRoot(Component? component)
        {
            if (component != null)
            {
                AddTransform(component.transform);
            }
        }

        private void AddRoot(GameObject? gameObject)
        {
            if (gameObject != null)
            {
                AddTransform(gameObject.transform);
            }
        }

        private void AddTransform(Transform? transform)
        {
            if (transform == null)
            {
                return;
            }

            if (roots.All(existing => existing == null || existing.GetInstanceID() != transform.GetInstanceID()))
            {
                roots.Add(transform);
            }
        }

        private void AddPlayerVisualRoot(Transform? transform)
        {
            if (transform == null)
            {
                return;
            }

            if (playerVisualRoots.All(existing => existing == null || existing.GetInstanceID() != transform.GetInstanceID()))
            {
                playerVisualRoots.Add(transform);
            }

            AddTransform(transform);
        }

        private static object? GetCurrentHero(out bool heroTypeFound)
        {
            Type? heroType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("Awaken.TG.Main.Heroes.Hero", throwOnError: false))
                .FirstOrDefault(type => type != null);
            heroTypeFound = heroType != null;
            return heroType == null ? null : ReadMember(heroType, "Current");
        }

        private static Transform? ReadTransformMember(object? instance, string name) =>
            AsTransform(ReadMember(instance, name));

        private static Transform? AsTransform(object? value)
        {
            if (value is Transform transform)
            {
                return transform;
            }

            if (value is GameObject gameObject)
            {
                return gameObject.transform;
            }

            if (value is Component component)
            {
                return component.transform;
            }

            return null;
        }

        private static object? ReadMember(object? instanceOrType, string name)
        {
            try
            {
                if (instanceOrType == null)
                {
                    return null;
                }

                Type type = instanceOrType as Type ?? instanceOrType.GetType();
                object? instance = instanceOrType as Type == null ? instanceOrType : null;
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                PropertyInfo? property = type.GetProperty(name, flags);
                if (property != null && property.GetIndexParameters().Length == 0)
                {
                    return property.GetValue(instance, null);
                }

                FieldInfo? field = type.GetField(name, flags);
                return field == null ? null : field.GetValue(instance);
            }
            catch
            {
                return null;
            }
        }

        private static bool IsKandraRendererComponent(Component? component) =>
            component != null &&
            string.Equals(component.GetType().Name, "KandraRenderer", StringComparison.Ordinal);

        private static bool IsHeroAnimancerComponent(Component? component) =>
            component != null &&
            (string.Equals(component.GetType().FullName, HeroAnimancerTypeName, StringComparison.Ordinal) ||
             string.Equals(component.GetType().Name, "ARHeroAnimancer", StringComparison.Ordinal));

        private static bool IsPlayerTppPoseBindingOwner(Component component)
        {
            string identity = BuildComponentIdentityText(component);
            return ContainsAny(identity, PlayerTppPoseOwnerTokens) &&
                !ContainsAny(identity, NonPlayerKandraOwnerTokens);
        }

        private static bool IsInLoadedScene(Component component)
        {
            try
            {
                return component.gameObject != null &&
                    component.gameObject.scene.IsValid() &&
                    component.gameObject.scene.isLoaded;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsInScene(Component component, Scene activeScene)
        {
            try
            {
                return component.gameObject != null &&
                    component.gameObject.scene.IsValid() &&
                    component.gameObject.scene == activeScene;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsPlayerBodyKandraCandidate(Component component, KandraRendererInfo info)
        {
            string identity = BuildKandraIdentityText(component, info);
            return ContainsAny(identity, PlayerBodyKandraTokens) &&
                !ContainsAny(identity, NonPlayerKandraOwnerTokens);
        }

        private static string BuildKandraLinkReason(
            Component component,
            KandraRendererInfo info,
            bool linkedToHeroRoot,
            bool playerBodyCandidate,
            bool inActiveScene)
        {
            if (playerBodyCandidate)
            {
                return "player-body-kandra-token";
            }

            if (linkedToHeroRoot)
            {
                return "captured-hero-root";
            }

            return inActiveScene ? "unlinked-active-scene-kandra" : "unlinked-loaded-kandra";
        }

        private static string BuildKandraIdentityText(Component component, KandraRendererInfo info)
        {
            Transform root = component.transform == null ? null! : component.transform.root;
            return string.Join(
                "|",
                component.name ?? string.Empty,
                root == null ? string.Empty : root.name ?? string.Empty,
                TransformPath(component.transform, root),
                info.MeshName,
                info.MaterialNames,
                TransformPath(info.RigTransform, root),
                info.RigAnimator == null ? string.Empty : info.RigAnimator.name ?? string.Empty,
                TransformPath(info.RigAnimator == null ? null : info.RigAnimator.transform, root));
        }

        private static string BuildComponentIdentityText(Component component)
        {
            Transform? transform = component.transform;
            Transform? root = transform == null ? null : transform.root;
            return string.Join(
                "|",
                component.GetType().FullName ?? component.GetType().Name,
                component.name ?? string.Empty,
                TransformPath(transform, root),
                root == null ? string.Empty : root.name ?? string.Empty,
                root == null ? string.Empty : TransformPath(root, root));
        }

        private static string BuildGameObjectIdentityText(GameObject gameObject)
        {
            Transform? transform = gameObject.transform;
            Transform? root = transform == null ? null : transform.root;
            return string.Join(
                "|",
                gameObject.GetType().FullName ?? gameObject.GetType().Name,
                gameObject.name ?? string.Empty,
                TransformPath(transform, root),
                root == null ? string.Empty : root.name ?? string.Empty,
                root == null ? string.Empty : TransformPath(root, root));
        }

        private static bool IsTppReferenceObject(object value)
        {
            if (value is AnimationClip clip)
            {
                return IsTppTargetClipName(clip.name ?? string.Empty);
            }

            if (value is RuntimeAnimatorController controller)
            {
                return ContainsAny(controller.name ?? string.Empty, TppReferenceTokens) ||
                    SafeAnimationClips(controller).Any(animationClip => IsTppTargetClipName(animationClip.name ?? string.Empty));
            }

            if (value is Animator animator)
            {
                string identity = BuildComponentIdentityText(animator);
                return ContainsAny(identity, TppReferenceTokens) &&
                    !ContainsAny(identity, NonPlayerKandraOwnerTokens);
            }

            if (value is Component component)
            {
                string identity = BuildComponentIdentityText(component);
                return ContainsAny(identity, TppReferenceTokens) &&
                    !ContainsAny(identity, NonPlayerKandraOwnerTokens);
            }

            if (value is GameObject gameObject)
            {
                string identity = BuildGameObjectIdentityText(gameObject);
                return ContainsAny(identity, TppReferenceTokens) &&
                    !ContainsAny(identity, NonPlayerKandraOwnerTokens);
            }

            if (value is Object unityObject)
            {
                string identity = (unityObject.GetType().FullName ?? unityObject.GetType().Name) + "|" + (unityObject.name ?? string.Empty);
                return ContainsAny(identity, TppReferenceTokens);
            }

            string typeName = value.GetType().FullName ?? value.GetType().Name;
            return ContainsAny(typeName, TppReferenceTokens);
        }

        private static string ObjectPath(object? value)
        {
            if (value is Component component)
            {
                Transform? root = component.transform == null ? null : component.transform.root;
                return TransformPath(component.transform, root);
            }

            if (value is GameObject gameObject)
            {
                Transform? root = gameObject.transform == null ? null : gameObject.transform.root;
                return TransformPath(gameObject.transform, root);
            }

            if (value is Object unityObject)
            {
                return unityObject.name ?? string.Empty;
            }

            return value == null ? string.Empty : value.GetType().FullName ?? value.GetType().Name;
        }

        private static bool ContainsAny(string value, IEnumerable<string> tokens) =>
            tokens.Any(token => value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);

        private static bool IsTargetClipName(string clipName) =>
            string.Equals(clipName, TargetClip, StringComparison.Ordinal);

        private static bool IsTppTargetClipName(string clipName) =>
            IsTargetClipName(clipName) &&
            clipName.IndexOf("_TPP_", StringComparison.OrdinalIgnoreCase) >= 0;

        private static IEnumerable<FieldInfo> GetAllInstanceFields(Type type)
        {
            Type? current = type;
            while (current != null)
            {
                foreach (FieldInfo field in current.GetFields(
                             BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    yield return field;
                }

                current = current.BaseType;
            }
        }

        private static IEnumerable<object?> EnumerateObject(object? value, int maxItems)
        {
            if (value == null || value is string || value is Object)
            {
                yield break;
            }

            if (!(value is IEnumerable enumerable))
            {
                yield break;
            }

            IEnumerator? enumerator;
            try
            {
                enumerator = enumerable.GetEnumerator();
            }
            catch
            {
                yield break;
            }

            int count = 0;
            while (enumerator != null && count < maxItems)
            {
                bool hasNext;
                try
                {
                    hasNext = enumerator.MoveNext();
                }
                catch
                {
                    yield break;
                }

                if (!hasNext)
                {
                    yield break;
                }

                yield return enumerator.Current;
                count++;
            }
        }

        private static bool ShouldTraversePoseBindingObject(object value)
        {
            if (value is AnimationClip)
            {
                return true;
            }

            if (IsSimplePoseBindingValue(value) || value is Transform || value is GameObject)
            {
                return false;
            }

            if (value is IEnumerable && !(value is string))
            {
                return true;
            }

            string typeName = value.GetType().FullName ?? value.GetType().Name;
            return typeName.StartsWith("Awaken.", StringComparison.Ordinal) ||
                   typeName.StartsWith("Animancer.", StringComparison.Ordinal) ||
                   typeName.StartsWith("System.Collections", StringComparison.Ordinal) ||
                   typeName.StartsWith("UnityEngine.Events", StringComparison.Ordinal);
        }

        private static bool ShouldTraverseTppReferenceObject(object value)
        {
            if (value is AnimationClip ||
                value is RuntimeAnimatorController ||
                value is Animator ||
                value is Component ||
                value is GameObject)
            {
                return true;
            }

            if (IsSimplePoseBindingValue(value) || value is Transform)
            {
                return false;
            }

            if (value is IEnumerable && !(value is string))
            {
                return true;
            }

            string typeName = value.GetType().FullName ?? value.GetType().Name;
            return typeName.StartsWith("Awaken.", StringComparison.Ordinal) ||
                   typeName.StartsWith("Animancer.", StringComparison.Ordinal) ||
                   typeName.StartsWith("System.Collections", StringComparison.Ordinal) ||
                   typeName.StartsWith("UnityEngine.Events", StringComparison.Ordinal) ||
                   typeName.IndexOf("AssetReference", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   typeName.IndexOf("Prefab", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static IEnumerable<AnimationClip> SafeAnimationClips(RuntimeAnimatorController? controller)
        {
            if (controller == null)
            {
                yield break;
            }

            AnimationClip[] clips;
            try
            {
                clips = controller.animationClips ?? Array.Empty<AnimationClip>();
            }
            catch
            {
                yield break;
            }

            foreach (AnimationClip clip in clips)
            {
                if (clip != null)
                {
                    yield return clip;
                }
            }
        }

        private static bool IsSimplePoseBindingValue(object? value)
        {
            if (value == null)
            {
                return true;
            }

            Type type = value.GetType();
            return type.IsPrimitive ||
                   type.IsEnum ||
                   value is string ||
                   value is decimal ||
                   value is Guid ||
                   value is DateTime ||
                   value is Vector2 ||
                   value is Vector3 ||
                   value is Vector4 ||
                   value is Quaternion;
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance = new ReferenceComparer();

            public new bool Equals(object? x, object? y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }

        private readonly struct PoseBindingGraphNode
        {
            public PoseBindingGraphNode(object value, string path, int depth)
            {
                Value = value;
                Path = path;
                Depth = depth;
            }

            public object Value { get; }

            public string Path { get; }

            public int Depth { get; }
        }

        private readonly struct TppReferenceGraphNode
        {
            public TppReferenceGraphNode(object value, string path, int depth, bool tppScoped)
            {
                Value = value;
                Path = path;
                Depth = depth;
                TppScoped = tppScoped;
            }

            public object Value { get; }

            public string Path { get; }

            public int Depth { get; }

            public bool TppScoped { get; }
        }

        private sealed class AddressableBundleMatch : IEquatable<AddressableBundleMatch>
        {
            public AddressableBundleMatch(string bundleFileName, string query)
            {
                BundleFileName = bundleFileName;
                Query = query;
            }

            public string BundleFileName { get; }

            public string Query { get; }

            public bool Equals(AddressableBundleMatch? other) =>
                other != null &&
                string.Equals(BundleFileName, other.BundleFileName, StringComparison.Ordinal) &&
                string.Equals(Query, other.Query, StringComparison.Ordinal);

            public override bool Equals(object? obj) =>
                Equals(obj as AddressableBundleMatch);

            public override int GetHashCode() =>
                StringComparer.Ordinal.GetHashCode(BundleFileName) ^
                StringComparer.Ordinal.GetHashCode(Query);
        }

        private readonly struct AddressableDirectQuery
        {
            public AddressableDirectQuery(string kind, string value)
            {
                Kind = kind;
                Value = value;
            }

            public string Kind { get; }

            public string Value { get; }
        }

        private readonly struct AddressableRuntimeKeyDependencyQuery
        {
            public AddressableRuntimeKeyDependencyQuery(
                string sourceKind,
                string ownerType,
                string ownerPath,
                string sourcePath,
                string runtimeKey,
                string address,
                string assetGuid,
                string primaryKey)
            {
                SourceKind = sourceKind;
                OwnerType = ownerType;
                OwnerPath = ownerPath;
                SourcePath = sourcePath;
                RuntimeKey = runtimeKey;
                Address = address;
                AssetGuid = assetGuid;
                PrimaryKey = primaryKey;
            }

            public string SourceKind { get; }

            public string OwnerType { get; }

            public string OwnerPath { get; }

            public string SourcePath { get; }

            public string RuntimeKey { get; }

            public string Address { get; }

            public string AssetGuid { get; }

            public string PrimaryKey { get; }
        }

        private readonly struct AddressableRuntimeKeyDependencyGraphNode
        {
            public AddressableRuntimeKeyDependencyGraphNode(
                object location,
                int locationIndex,
                int depth,
                bool rootLocation,
                string parentPrimaryKey,
                string parentInternalId,
                string parentProviderId,
                string parentResourceType)
            {
                Location = location;
                LocationIndex = locationIndex;
                Depth = depth;
                RootLocation = rootLocation;
                ParentPrimaryKey = parentPrimaryKey;
                ParentInternalId = parentInternalId;
                ParentProviderId = parentProviderId;
                ParentResourceType = parentResourceType;
            }

            public object Location { get; }

            public int LocationIndex { get; }

            public int Depth { get; }

            public bool RootLocation { get; }

            public string ParentPrimaryKey { get; }

            public string ParentInternalId { get; }

            public string ParentProviderId { get; }

            public string ParentResourceType { get; }
        }

        private sealed class KandraRendererCandidate
        {
            public KandraRendererCandidate(
                Component component,
                KandraRendererInfo info,
                bool inActiveScene,
                bool playerBodyCandidate)
            {
                Component = component;
                Info = info;
                InActiveScene = inActiveScene;
                PlayerBodyCandidate = playerBodyCandidate;
            }

            public Component Component { get; }

            public KandraRendererInfo Info { get; }

            public bool InActiveScene { get; }

            public bool PlayerBodyCandidate { get; }
        }

        private sealed class KandraRendererInfo
        {
            private KandraRendererInfo(
                bool enabled,
                string renderingId,
                string meshName,
                string materialNames,
                Transform? rigTransform,
                Animator? rigAnimator,
                string isRegisteredStatus,
                string tryGetMeshMemoryStatus,
                string meshMemoryText)
            {
                Enabled = enabled;
                RenderingId = renderingId;
                MeshName = meshName;
                MaterialNames = materialNames;
                RigTransform = rigTransform;
                RigAnimator = rigAnimator;
                IsRegisteredStatus = isRegisteredStatus;
                TryGetMeshMemoryStatus = tryGetMeshMemoryStatus;
                MeshMemoryText = meshMemoryText;
            }

            public bool Enabled { get; }

            public string RenderingId { get; }

            public string MeshName { get; }

            public string MaterialNames { get; }

            public Transform? RigTransform { get; }

            public Animator? RigAnimator { get; }

            public string IsRegisteredStatus { get; }

            public string TryGetMeshMemoryStatus { get; }

            public string MeshMemoryText { get; }

            public static KandraRendererInfo From(Component component)
            {
                bool enabled = component is Behaviour behaviour && behaviour.enabled;
                object? renderingIdValue = ReadMember(component, "RenderingId");
                string renderingId = FormatObject(renderingIdValue);
                object? rendererData = ReadMember(component, "rendererData");
                object? rig = ReadMember(rendererData, "rig");
                object? mesh = ReadMember(rendererData, "mesh");
                object? materials = ReadMember(rendererData, "materials");
                Animator? animator = ReadMember(rig, "animator") as Animator;
                KandraRuntimeRegistrationDiagnostic diagnostic = KandraRuntimeRegistrationDiagnostic.From(component, renderingIdValue, mesh);
                return new KandraRendererInfo(
                    enabled,
                    renderingId,
                    ObjectName(mesh),
                    FormatMaterials(materials),
                    AsTransform(rig),
                    animator,
                    diagnostic.IsRegisteredStatus,
                    diagnostic.TryGetMeshMemoryStatus,
                    diagnostic.MeshMemoryText);
            }

            private static string FormatObject(object? value) =>
                value == null ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;

            private static string ObjectName(object? value) =>
                value is Object unityObject ? unityObject.name ?? string.Empty : FormatObject(value);

            private static string FormatMaterials(object? value)
            {
                if (value is Material[] materials)
                {
                    return string.Join("|", materials.Take(8).Select(material => material == null ? "null" : material.name ?? string.Empty));
                }

                return string.Empty;
            }
        }

        private sealed class KandraRuntimeRegistrationDiagnostic
        {
            private KandraRuntimeRegistrationDiagnostic(
                string isRegisteredStatus,
                string tryGetMeshMemoryStatus,
                string meshMemoryText)
            {
                IsRegisteredStatus = isRegisteredStatus;
                TryGetMeshMemoryStatus = tryGetMeshMemoryStatus;
                MeshMemoryText = meshMemoryText;
            }

            public string IsRegisteredStatus { get; }

            public string TryGetMeshMemoryStatus { get; }

            public string MeshMemoryText { get; }

            public static KandraRuntimeRegistrationDiagnostic From(Component component, object? renderingIdValue, object? mesh)
            {
                try
                {
                    Type? managerType = FindType(component.GetType().Assembly, "Awaken.Kandra.KandraRendererManager");
                    if (managerType == null)
                    {
                        return Blocked("kandra_renderer_manager_type_missing");
                    }

                    object? manager = ReadMember(managerType, "Instance");
                    if (manager == null)
                    {
                        return Blocked("kandra_renderer_manager_instance_missing");
                    }

                    if (!TryReadUInt32(renderingIdValue, out uint renderingId))
                    {
                        return new KandraRuntimeRegistrationDiagnostic(
                            "blocked_unobserved:rendering_id_unavailable",
                            "blocked_unobserved:rendering_id_unavailable",
                            string.Empty);
                    }

                    string isRegisteredStatus = ReadIsRegistered(manager, renderingId);
                    string tryGetMeshMemoryStatus = ReadTryGetMeshMemory(manager, mesh, out string meshMemoryText);
                    return new KandraRuntimeRegistrationDiagnostic(isRegisteredStatus, tryGetMeshMemoryStatus, meshMemoryText);
                }
                catch (Exception exception)
                {
                    string reason = "blocked_unobserved:kandra_runtime_diagnostic_exception:" + exception.GetType().Name;
                    return new KandraRuntimeRegistrationDiagnostic(reason, reason, string.Empty);
                }
            }

            private static KandraRuntimeRegistrationDiagnostic Blocked(string reason)
            {
                string status = "blocked_unobserved:" + reason;
                return new KandraRuntimeRegistrationDiagnostic(status, status, string.Empty);
            }

            private static string ReadIsRegistered(object manager, uint renderingId)
            {
                MethodInfo? method = manager.GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(value =>
                        string.Equals(value.Name, "IsRegistered", StringComparison.Ordinal) &&
                        value.GetParameters().Length == 1);
                if (method == null)
                {
                    return "blocked_unobserved:IsRegistered_method_missing";
                }

                object? result = method.Invoke(manager, new object[] { renderingId });
                return result is bool registered
                    ? "observed:" + registered.ToString(CultureInfo.InvariantCulture) + ":renderingId=" + renderingId.ToString(CultureInfo.InvariantCulture)
                    : "blocked_unobserved:IsRegistered_return_not_bool";
            }

            private static string ReadTryGetMeshMemory(object manager, object? mesh, out string meshMemoryText)
            {
                meshMemoryText = string.Empty;
                if (mesh == null)
                {
                    return "blocked_unobserved:kandra_mesh_missing";
                }

                object? meshManager = ReadMember(manager, "MeshManager");
                if (meshManager == null)
                {
                    return "blocked_unobserved:mesh_manager_missing";
                }

                MethodInfo? method = meshManager.GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(value =>
                    {
                        if (!string.Equals(value.Name, "TryGetMeshMemory", StringComparison.Ordinal))
                        {
                            return false;
                        }

                        ParameterInfo[] parameters = value.GetParameters();
                        return parameters.Length == 2 && parameters[1].ParameterType.IsByRef;
                    });
                if (method == null)
                {
                    return "blocked_unobserved:TryGetMeshMemory_method_missing";
                }

                object?[] arguments = { mesh, null };
                object? result = method.Invoke(meshManager, arguments);
                meshMemoryText = FormatDiagnosticObject(arguments[1]);
                return result is bool available
                    ? "observed:" + available.ToString(CultureInfo.InvariantCulture)
                    : "blocked_unobserved:TryGetMeshMemory_return_not_bool";
            }

            private static Type? FindType(Assembly preferredAssembly, string typeName) =>
                preferredAssembly.GetType(typeName, throwOnError: false) ??
                AppDomain.CurrentDomain.GetAssemblies()
                    .Select(assembly => assembly.GetType(typeName, throwOnError: false))
                    .FirstOrDefault(type => type != null);

            private static bool TryReadUInt32(object? value, out uint result)
            {
                if (value is uint direct)
                {
                    result = direct;
                    return true;
                }

                if (value is string text && uint.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
                {
                    return true;
                }

                try
                {
                    if (value != null)
                    {
                        result = Convert.ToUInt32(value, CultureInfo.InvariantCulture);
                        return true;
                    }
                }
                catch
                {
                }

                result = 0u;
                return false;
            }

            private static string FormatDiagnosticObject(object? value) =>
                value == null ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }
    }

    [Serializable]
    public sealed class ObserverPacket
    {
        public string marker = string.Empty;
        public string generatedAtUtc = string.Empty;
        public string pluginGuid = string.Empty;
        public string pluginName = string.Empty;
        public string pluginVersion = string.Empty;
        public string unityVersion = string.Empty;
        public string routeId = string.Empty;
        public string observerMode = string.Empty;
        public string triggerSource = string.Empty;
        public string evidenceSource = string.Empty;
        public string activeScene = string.Empty;
        public string liveBodyObservationStatus = string.Empty;
        public string liveEquipObservationStatus = string.Empty;
        public string poseBindingStatus = string.Empty;
        public RuntimeContextPacket runtimeContext = new RuntimeContextPacket();
        public ObservationRowPacket[] rows = Array.Empty<ObservationRowPacket>();
        public VisualRuntimeObservationRequestPacket visualRuntimeObservationRequest = new VisualRuntimeObservationRequestPacket();
        public DownstreamBoundary downstream = new DownstreamBoundary();
        public bool liveRuntimeMetricsObserved;
        public bool visualRuntimeAccepted;
        public string[] blockers = Array.Empty<string>();
        public string safety = string.Empty;
    }

    [Serializable]
    public sealed class VisualRuntimeObservationRequestPacket
    {
        public string routeId = string.Empty;
        public string observerMode = string.Empty;
        public string triggerSource = string.Empty;
        public string evidenceSource = string.Empty;
        public string activeScene = string.Empty;
        public string liveBodyObservationStatus = string.Empty;
        public string liveEquipObservationStatus = string.Empty;
        public string poseBindingStatus = string.Empty;
        public ObservationRowPacket[] rows = Array.Empty<ObservationRowPacket>();

        public static VisualRuntimeObservationRequestPacket From(VisualRuntimeObservationRequest request) =>
            new VisualRuntimeObservationRequestPacket
            {
                routeId = request.RouteId,
                observerMode = request.ObserverMode,
                triggerSource = request.TriggerSource,
                evidenceSource = request.EvidenceSource,
                activeScene = request.ActiveScene,
                liveBodyObservationStatus = request.LiveBodyObservationStatus,
                liveEquipObservationStatus = request.LiveEquipObservationStatus,
                poseBindingStatus = request.PoseBindingStatus,
                rows = request.Rows.Select(ObservationRowPacket.From).ToArray(),
            };
    }

    [Serializable]
    public sealed class ObservationRowPacket
    {
        public string sex = string.Empty;
        public string deformationReceiver = string.Empty;
        public int inversionRecordCount;
        public string liveObjectPath = string.Empty;
        public string rendererName = string.Empty;
        public string meshName = string.Empty;
        public string materialNames = string.Empty;
        public string animatorClipNames = string.Empty;
        public string normalDotClassification = string.Empty;
        public string referenceTransportSemantics = string.Empty;
        public string localWindingSemantics = string.Empty;
        public string clippingStatus = string.Empty;
        public string seamStatus = string.Empty;
        public string bodyCoverageStatus = string.Empty;
        public string materialShadowStatus = string.Empty;
        public string layerStatus = string.Empty;
        public string notes = string.Empty;
        public string kandraIsRegisteredStatus = string.Empty;
        public string kandraTryGetMeshMemoryStatus = string.Empty;

        public static ObservationRowPacket From(VisualRuntimeObservationRow row) =>
            new ObservationRowPacket
            {
                sex = row.Sex,
                deformationReceiver = row.DeformationReceiver,
                inversionRecordCount = row.InversionRecordCount,
                liveObjectPath = row.LiveObjectPath,
                rendererName = row.RendererName,
                meshName = row.MeshName,
                materialNames = row.MaterialNames,
                animatorClipNames = row.AnimatorClipNames,
                normalDotClassification = row.NormalDotClassification,
                referenceTransportSemantics = row.ReferenceTransportSemantics,
                localWindingSemantics = row.LocalWindingSemantics,
                clippingStatus = row.ClippingStatus,
                seamStatus = row.SeamStatus,
                bodyCoverageStatus = row.BodyCoverageStatus,
                materialShadowStatus = row.MaterialShadowStatus,
                layerStatus = row.LayerStatus,
                notes = row.Notes,
                kandraIsRegisteredStatus = row.KandraIsRegisteredStatus,
                kandraTryGetMeshMemoryStatus = row.KandraTryGetMeshMemoryStatus,
            };
    }

    [Serializable]
    public sealed class RuntimeContextPacket
    {
        public string activeScene = string.Empty;
        public int frameCount;
        public float unscaledTime;
        public bool heroTypeFound;
        public bool heroPresent;
        public bool controllerPresent;
        public bool bodyRootPresent;
        public bool liveBodyEquipSceneObserved;
        public bool targetClipObserved;
        public string targetClipBindingSource = string.Empty;
        public string poseBindingStatus = string.Empty;
        public int rootCount;
        public int totalRendererCount;
        public int skinnedRendererCount;
        public int activeRendererCount;
        public int enabledRendererCount;
        public int visibleRendererCount;
        public int loadedKandraRendererCount;
        public RendererPacket[] rendererRows = Array.Empty<RendererPacket>();
        public int sceneKandraRendererCount;
        public int playerKandraCandidateCount;
        public int playerVisualRootCount;
        public int kandraRendererCount;
        public int activeKandraRendererCount;
        public int enabledKandraRendererCount;
        public string liveKandraIsRegisteredStatus = string.Empty;
        public string liveKandraTryGetMeshMemoryStatus = string.Empty;
        public KandraRendererPacket[] kandraRendererRows = Array.Empty<KandraRendererPacket>();
        public int heroAnimancerOwnerCount;
        public int runtimeAnimatorControllerTargetClipCandidateCount;
        public int tppTargetClipCandidateCount;
        public int acceptedTppPoseBindingCandidateCount;
        public int tppBodyPrefabReferenceCount;
        public int tppAnimationReferenceCount;
        public int loadedTargetAnimationAssetCount;
        public int vHeroControllerAssetReferenceIdentityCount;
        public int addressableResourceLocatorMatchCount;
        public int addressableFileLocatorMatchCount;
        public int addressableCatalogLocatorMatchCount;
        public int addressableTargetClipBundleMatchCount;
        public int addressableTppBodyBundleMatchCount;
        public int addressableRuntimeResourceLocatorCount;
        public int addressableRuntimeResourceLocatorKeyCount;
        public int addressableRuntimeResourceLocatorKeyScanTruncatedCount;
        public int addressableRuntimeDirectLocateAttemptCount;
        public int addressableRuntimeDirectLocateMatchCount;
        public int addressableSuccessfulDirectLocateRowCount;
        public int addressableSuccessfulDirectLocateRowTruncatedCount;
        public int addressableRuntimeKeyDirectLocateAttemptCount;
        public int addressableRuntimeKeyDirectLocateMatchCount;
        public int addressableRuntimeKeyDependencyRowCount;
        public int addressableRuntimeKeyDependencyRowTruncatedCount;
        public int addressableCatalogFileCount;
        public int addressableCatalogBundleAliasReferenceCount;
        public int addressableLocatorDiagnosticRowCount;
        public KandraRegistrationContractDiagnosticPacket kandraRegistrationContractDiagnostic = KandraRegistrationContractDiagnosticPacket.Blocked("not_captured");
        public PoseBindingOwnerPacket[] poseBindingOwnerRows = Array.Empty<PoseBindingOwnerPacket>();
        public PoseBindingCandidatePacket[] poseBindingCandidateRows = Array.Empty<PoseBindingCandidatePacket>();
        public TppBodyPrefabReferencePacket[] tppBodyPrefabReferenceRows = Array.Empty<TppBodyPrefabReferencePacket>();
        public TppAnimationReferencePacket[] tppAnimationReferenceRows = Array.Empty<TppAnimationReferencePacket>();
        public VHeroControllerAssetReferencePacket[] vHeroControllerAssetReferenceRows = Array.Empty<VHeroControllerAssetReferencePacket>();
        public AddressableLocatorPacket[] addressableLocatorRows = Array.Empty<AddressableLocatorPacket>();
        public AddressableLocatorPacket[] addressableSuccessfulDirectLocateRows = Array.Empty<AddressableLocatorPacket>();
        public AddressableRuntimeKeyDependencyPacket[] addressableRuntimeKeyDependencyRows = Array.Empty<AddressableRuntimeKeyDependencyPacket>();
        public AddressableLocatorDiagnosticPacket[] addressableLocatorDiagnosticRows = Array.Empty<AddressableLocatorDiagnosticPacket>();
        public AnimatorPacket[] animatorRows = Array.Empty<AnimatorPacket>();
    }

    [Serializable]
    public sealed class RendererPacket
    {
        public string typeName = string.Empty;
        public string name = string.Empty;
        public string path = string.Empty;
        public bool activeSelf;
        public bool activeInHierarchy;
        public bool enabled;
        public bool visible;
        public string boundsCenter = string.Empty;
        public string boundsSize = string.Empty;
        public string meshName = string.Empty;
        public string materialNames = string.Empty;
        public bool isSkinnedRenderer;

        public static RendererPacket From(Renderer renderer, Transform root)
        {
            Bounds bounds = default;
            try
            {
                bounds = renderer.bounds;
            }
            catch
            {
            }

            return new RendererPacket
            {
                typeName = renderer.GetType().FullName ?? renderer.GetType().Name,
                name = renderer.name ?? string.Empty,
                path = TransformPath(renderer.transform, root),
                activeSelf = renderer.gameObject.activeSelf,
                activeInHierarchy = renderer.gameObject.activeInHierarchy,
                enabled = renderer.enabled,
                visible = renderer.isVisible,
                boundsCenter = FormatVector(bounds.center),
                boundsSize = FormatVector(bounds.size),
                meshName = RendererMeshName(renderer),
                materialNames = RendererMaterialNames(renderer),
                isSkinnedRenderer = renderer is SkinnedMeshRenderer,
            };
        }

        private static string RendererMeshName(Renderer renderer)
        {
            try
            {
                if (renderer is SkinnedMeshRenderer skinnedMeshRenderer && skinnedMeshRenderer.sharedMesh != null)
                {
                    return skinnedMeshRenderer.sharedMesh.name ?? string.Empty;
                }

                MeshFilter filter = renderer.GetComponent<MeshFilter>();
                return filter != null && filter.sharedMesh != null ? filter.sharedMesh.name ?? string.Empty : string.Empty;
            }
            catch
            {
                return "unavailable";
            }
        }

        private static string RendererMaterialNames(Renderer renderer)
        {
            try
            {
                Material[] materials = renderer.sharedMaterials ?? Array.Empty<Material>();
                return string.Join("|", materials.Take(8).Select(material => material == null ? "null" : material.name ?? string.Empty));
            }
            catch
            {
                return "unavailable";
            }
        }
    }

    [Serializable]
    public sealed class KandraRegistrationContractDiagnosticPacket
    {
        private const string ManagerTypeName = "Awaken.Kandra.KandraRendererManager";
        private const string RendererTypeName = "Awaken.Kandra.KandraRenderer";
        private const string MeshTypeName = "Awaken.Kandra.KandraMesh";

        public string status = string.Empty;
        public string assemblyName = string.Empty;
        public string assemblyFullName = string.Empty;
        public string assemblyLocation = string.Empty;
        public string assemblyModuleVersionId = string.Empty;
        public string assemblySha256 = string.Empty;
        public string managerType = string.Empty;
        public bool managerInstancePresent;
        public string registrationOwner = ManagerTypeName;
        public string registrationMethodSignature = string.Empty;
        public string registrationMethodFingerprint = string.Empty;
        public bool registrationMethodPresent;
        public bool registrationMethodPublic;
        public bool registrationMethodInvoked;
        public string rendererType = RendererTypeName;
        public bool rendererTypePresent;
        public bool rendererOnEnableMethodPresent;
        public bool rendererOnDisableMethodPresent;
        public bool rendererAwakeMethodPresent;
        public bool rendererDataFieldPresent;
        public bool rendererRenderingIdPropertyPresent;
        public string requiredRendererDataMembers = string.Empty;
        public bool requiredRendererDataMembersPresent;
        public string requiredKandraMeshMembers = string.Empty;
        public bool requiredKandraMeshMembersPresent;
        public string managerDependencyMethodSignatures = string.Empty;
        public string managerDependencyMethodFingerprint = string.Empty;
        public bool managerDependencyMethodsPresent;
        public bool finalizeRegistrationMethodPresent;
        public bool onEarlyUpdateBeginMethodPresent;
        public bool onBeginRenderingMethodPresent;
        public bool queueStateFieldsPresent;
        public string lifecycleTiming = string.Empty;
        public string failureBehavior = string.Empty;
        public string diagnosticBoundary = string.Empty;
        public bool canRegisterMethodsInvoked;
        public bool createdOrActivatedKandraObject;
        public bool runtimeRegistrationCallContractProven;
        public bool runtimeRegistrationInvocationAllowed;
        public bool runtimeRegistrationExecuted;
        public bool candidateMapApplicationExecuted;
        public bool conversionExecuted;
        public bool itemEquipMutationExecuted;
        public bool saveMutationExecuted;
        public bool nativeGameWriteExecuted;
        public bool downstreamWritesExecuted;
        public string blockers = string.Empty;
        public string warnings = string.Empty;

        public static KandraRegistrationContractDiagnosticPacket Blocked(string reason) =>
            new KandraRegistrationContractDiagnosticPacket
            {
                status = "blocked_unobserved:" + reason,
                diagnosticBoundary = NoWriteBoundary,
                registrationMethodInvoked = false,
                canRegisterMethodsInvoked = false,
                createdOrActivatedKandraObject = false,
                runtimeRegistrationCallContractProven = false,
                runtimeRegistrationInvocationAllowed = false,
                runtimeRegistrationExecuted = false,
                candidateMapApplicationExecuted = false,
                conversionExecuted = false,
                itemEquipMutationExecuted = false,
                saveMutationExecuted = false,
                nativeGameWriteExecuted = false,
                downstreamWritesExecuted = false,
                blockers = reason,
            };

        public static KandraRegistrationContractDiagnosticPacket Capture()
        {
            var packet = new KandraRegistrationContractDiagnosticPacket
            {
                diagnosticBoundary = NoWriteBoundary,
                lifecycleTiming = "static_recovered:KandraRenderer.OnEnable invokes KandraRendererManager.Register(KandraRenderer); Register queues renderer state; KandraRendererManager.OnEarlyUpdateBegin invokes FinalizeRegistration when enabled; rendering work continues through PreLateUpdate and beginContextRendering.",
                failureBehavior = "static_recovered:FinalizeRegistration checks RigManager.CanRegister, MeshManager.CanRegister, BonesManager.CanRegister, BlendshapesManager.CanRegister, and SkinningManager.CanRegister; failed readiness requeues registration and reports BrokenKandraMessage.OutOfMemory once.",
                registrationMethodInvoked = false,
                canRegisterMethodsInvoked = false,
                createdOrActivatedKandraObject = false,
                runtimeRegistrationCallContractProven = false,
                runtimeRegistrationInvocationAllowed = false,
                runtimeRegistrationExecuted = false,
                candidateMapApplicationExecuted = false,
                conversionExecuted = false,
                itemEquipMutationExecuted = false,
                saveMutationExecuted = false,
                nativeGameWriteExecuted = false,
                downstreamWritesExecuted = false,
                warnings = "runtime_registration_invocation_not_enabled",
            };

            try
            {
                Type? managerType = FindRuntimeType(ManagerTypeName);
                if (managerType == null)
                {
                    packet.status = "blocked_unobserved:kandra_renderer_manager_type_missing";
                    packet.blockers = "kandra_renderer_manager_type_missing";
                    return packet;
                }

                Assembly assembly = managerType.Assembly;
                packet.assemblyName = assembly.GetName().Name ?? string.Empty;
                packet.assemblyFullName = assembly.FullName ?? string.Empty;
                packet.assemblyLocation = SafeAssemblyLocation(assembly);
                packet.assemblyModuleVersionId = SafeModuleVersionId(assembly);
                packet.assemblySha256 = Sha256FileIfPresent(packet.assemblyLocation);
                packet.managerType = managerType.FullName ?? managerType.Name;
                packet.managerInstancePresent = ReadStaticMember(managerType, "Instance") != null;

                Type? rendererType = FindRuntimeType(RendererTypeName);
                packet.rendererTypePresent = rendererType != null;
                if (rendererType != null)
                {
                    packet.rendererType = rendererType.FullName ?? rendererType.Name;
                    packet.rendererOnEnableMethodPresent = HasMethod(rendererType, "OnEnable");
                    packet.rendererOnDisableMethodPresent = HasMethod(rendererType, "OnDisable");
                    packet.rendererAwakeMethodPresent = HasMethod(rendererType, "Awake");
                    packet.rendererDataFieldPresent = HasField(rendererType, "rendererData");
                    packet.rendererRenderingIdPropertyPresent = HasProperty(rendererType, "RenderingId");
                    packet.requiredRendererDataMembers = RendererDataMemberStatus(rendererType, out bool rendererDataMembersPresent);
                    packet.requiredRendererDataMembersPresent = rendererDataMembersPresent;
                }

                Type? meshType = FindRuntimeType(MeshTypeName);
                packet.requiredKandraMeshMembers = KandraMeshMemberStatus(meshType, out bool kandraMeshMembersPresent);
                packet.requiredKandraMeshMembersPresent = kandraMeshMembersPresent;

                MethodInfo? registerMethod = rendererType == null
                    ? null
                    : FindMethod(managerType, "Register", rendererType);
                packet.registrationMethodPresent = registerMethod != null;
                if (registerMethod != null)
                {
                    packet.registrationMethodPublic = registerMethod.IsPublic;
                    packet.registrationMethodSignature = FormatMethodSignature(registerMethod);
                    packet.registrationMethodFingerprint = Sha256Text(packet.registrationMethodSignature);
                }

                packet.finalizeRegistrationMethodPresent = HasMethod(managerType, "FinalizeRegistration");
                packet.onEarlyUpdateBeginMethodPresent = HasMethod(managerType, "OnEarlyUpdateBegin");
                packet.onBeginRenderingMethodPresent = HasMethod(managerType, "OnBeginRendering");
                packet.queueStateFieldsPresent =
                    HasField(managerType, "_toRegister") &&
                    HasField(managerType, "_submittedSlots") &&
                    HasField(managerType, "_fullyRegisteredSlots") &&
                    HasField(managerType, "_renderers");
                packet.managerDependencyMethodSignatures = DependencyMethodSignatures();
                packet.managerDependencyMethodFingerprint = Sha256Text(packet.managerDependencyMethodSignatures);
                packet.managerDependencyMethodsPresent = !packet.managerDependencyMethodSignatures.Contains("missing:", StringComparison.Ordinal);

                bool acceptedSurface =
                    packet.managerInstancePresent &&
                    packet.registrationMethodPresent &&
                    packet.rendererTypePresent &&
                    packet.rendererOnEnableMethodPresent &&
                    packet.rendererOnDisableMethodPresent &&
                    packet.rendererDataFieldPresent &&
                    packet.rendererRenderingIdPropertyPresent &&
                    packet.requiredRendererDataMembersPresent &&
                    packet.requiredKandraMeshMembersPresent &&
                    packet.finalizeRegistrationMethodPresent &&
                    packet.onEarlyUpdateBeginMethodPresent &&
                    packet.onBeginRenderingMethodPresent &&
                    packet.queueStateFieldsPresent &&
                    packet.managerDependencyMethodsPresent;

                packet.status = acceptedSurface
                    ? "observed:no_write_registration_contract_surface_fingerprinted"
                    : "blocked_unobserved:registration_contract_surface_incomplete";
                packet.blockers = acceptedSurface ? string.Empty : "registration_contract_surface_incomplete";
                return packet;
            }
            catch (Exception exception)
            {
                packet.status = "blocked_unobserved:kandra_registration_contract_diagnostic_exception:" + exception.GetType().Name;
                packet.blockers = exception.GetType().Name;
                return packet;
            }
        }

        private const string NoWriteBoundary =
            "reflectionOnly=true,registerInvoked=false,canRegisterInvoked=false,createdOrActivatedKandraObject=false,candidateMapApplicationExecuted=false,conversionExecuted=false,itemEquipMutationExecuted=false,saveMutationExecuted=false,nativeGameWriteExecuted=false,downstreamWritesExecuted=false";

        private static Type? FindRuntimeType(string typeName) =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(typeName, throwOnError: false))
                .FirstOrDefault(type => type != null);

        private static object? ReadStaticMember(Type type, string name)
        {
            try
            {
                const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                PropertyInfo? property = type.GetProperty(name, flags);
                if (property != null && property.GetIndexParameters().Length == 0)
                {
                    return property.GetValue(null, null);
                }

                FieldInfo? field = type.GetField(name, flags);
                return field == null ? null : field.GetValue(null);
            }
            catch
            {
                return null;
            }
        }

        private static MethodInfo? FindMethod(Type type, string name, params Type[] parameterTypes) =>
            type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(method =>
                {
                    if (!string.Equals(method.Name, name, StringComparison.Ordinal))
                    {
                        return false;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length != parameterTypes.Length)
                    {
                        return false;
                    }

                    for (int index = 0; index < parameters.Length; index++)
                    {
                        Type parameterType = parameters[index].ParameterType.IsByRef
                            ? parameters[index].ParameterType.GetElementType() ?? parameters[index].ParameterType
                            : parameters[index].ParameterType;
                        if (parameterType != parameterTypes[index])
                        {
                            return false;
                        }
                    }

                    return true;
                });

        private static bool HasMethod(Type type, string name) =>
            type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(method => string.Equals(method.Name, name, StringComparison.Ordinal));

        private static bool HasField(Type type, string name) =>
            type.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) != null;

        private static bool HasProperty(Type type, string name) =>
            type.GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) != null;

        private static string RendererDataMemberStatus(Type rendererType, out bool allPresent)
        {
            Type? rendererDataType = rendererType.GetNestedType("RendererData", BindingFlags.Public | BindingFlags.NonPublic);
            if (rendererDataType == null)
            {
                allPresent = false;
                return "missing:nested-type:RendererData";
            }

            string[] requiredFields =
            {
                "rig",
                "mesh",
                "bones",
                "blendshapeWeights",
                "filteringSettings",
            };
            string[] requiredProperties =
            {
                "RenderingMesh",
                "RenderingMaterials",
            };
            var parts = new List<string>();
            bool ok = true;
            foreach (string field in requiredFields)
            {
                bool present = HasField(rendererDataType, field);
                ok &= present;
                parts.Add((present ? "field:" : "missing:field:") + field);
            }

            foreach (string property in requiredProperties)
            {
                bool present = HasProperty(rendererDataType, property);
                ok &= present;
                parts.Add((present ? "property:" : "missing:property:") + property);
            }

            allPresent = ok;
            return string.Join(";", parts);
        }

        private static string KandraMeshMemberStatus(Type? meshType, out bool allPresent)
        {
            if (meshType == null)
            {
                allPresent = false;
                return "missing:type:" + MeshTypeName;
            }

            string[] requiredFields =
            {
                "vertexCount",
                "indicesCount",
                "bindposesCount",
                "modDirectory",
                "submeshes",
                "blendshapesNames",
                "reciprocalUvDistribution",
            };
            string[] requiredProperties =
            {
                "Name",
            };
            var parts = new List<string>();
            bool ok = true;
            foreach (string field in requiredFields)
            {
                bool present = HasField(meshType, field);
                ok &= present;
                parts.Add((present ? "field:" : "missing:field:") + field);
            }

            foreach (string property in requiredProperties)
            {
                bool present = HasProperty(meshType, property);
                ok &= present;
                parts.Add((present ? "property:" : "missing:property:") + property);
            }

            allPresent = ok;
            return string.Join(";", parts);
        }

        private static string DependencyMethodSignatures()
        {
            var signatures = new List<string>
            {
                MethodSignatureOrMissing("Awaken.Kandra.Managers.RigManager", "CanRegister", "Awaken.Kandra.KandraRig"),
                MethodSignatureOrMissing("Awaken.Kandra.Managers.MeshManager", "CanRegister", MeshTypeName),
                MethodSignatureOrMissing("Awaken.Kandra.Managers.MeshManager", "RegisterMesh", MeshTypeName),
                MethodSignatureOrMissing("Awaken.Kandra.Managers.MeshManager", "TryGetMeshMemory", MeshTypeName),
                MethodSignatureOrMissing("Awaken.Kandra.Managers.BonesManager", "CanRegister", "System.UInt16[]"),
                MethodSignatureOrMissing("Awaken.Kandra.Managers.BlendshapesManager", "CanRegister", MeshTypeName),
                MethodSignatureOrMissing("Awaken.Kandra.Managers.BlendshapesManager", "Register", "System.UInt32", MeshTypeName),
                MethodSignatureOrMissing("Awaken.Kandra.Managers.SkinningManager", "CanRegister"),
                MethodSignatureOrMissing("Awaken.Kandra.Managers.SkinnedBatchRenderGroup", "Register", "System.UInt32", "Awaken.Kandra.KandraRenderingMesh", "UnityEngine.Material[]"),
                MethodSignatureOrMissing("Awaken.Kandra.Managers.VisibilityCullingManager", "Register", "System.UInt32"),
                MethodSignatureOrMissing("Awaken.Kandra.Managers.AnimatorManager", "RegisterAnimator", "System.UInt32", "UnityEngine.Animator"),
            };
            return string.Join(";", signatures);
        }

        private static string MethodSignatureOrMissing(string typeName, string methodName, params string[] leadingParameterTypeNames)
        {
            Type? type = FindRuntimeType(typeName);
            if (type == null)
            {
                return "missing:type:" + typeName + "." + methodName;
            }

            MethodInfo? method = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(candidate =>
                {
                    if (!string.Equals(candidate.Name, methodName, StringComparison.Ordinal))
                    {
                        return false;
                    }

                    ParameterInfo[] parameters = candidate.GetParameters();
                    if (parameters.Length < leadingParameterTypeNames.Length)
                    {
                        return false;
                    }

                    for (int index = 0; index < leadingParameterTypeNames.Length; index++)
                    {
                        Type parameterType = parameters[index].ParameterType.IsByRef
                            ? parameters[index].ParameterType.GetElementType() ?? parameters[index].ParameterType
                            : parameters[index].ParameterType;
                        if (!string.Equals(TypeName(parameterType), leadingParameterTypeNames[index], StringComparison.Ordinal))
                        {
                            return false;
                        }
                    }

                    return true;
                });

            return method == null
                ? "missing:method:" + typeName + "." + methodName
                : FormatMethodSignature(method);
        }

        private static string FormatMethodSignature(MethodInfo method)
        {
            string parameters = string.Join(
                ",",
                method.GetParameters().Select(parameter =>
                {
                    Type parameterType = parameter.ParameterType.IsByRef
                        ? parameter.ParameterType.GetElementType() ?? parameter.ParameterType
                        : parameter.ParameterType;
                    string modifier = parameter.IsOut
                        ? "out "
                        : parameter.ParameterType.IsByRef
                            ? "ref "
                            : string.Empty;
                    return modifier + TypeName(parameterType) + " " + parameter.Name;
                }));
            return TypeName(method.ReturnType) + " " + TypeName(method.DeclaringType) + "." + method.Name + "(" + parameters + ")";
        }

        private static string TypeName(Type? type) =>
            type == null ? string.Empty : type.FullName ?? type.Name;

        private static string SafeAssemblyLocation(Assembly assembly)
        {
            try
            {
                return assembly.Location ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string SafeModuleVersionId(Assembly assembly)
        {
            try
            {
                return assembly.ManifestModule.ModuleVersionId.ToString("D", CultureInfo.InvariantCulture);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string Sha256FileIfPresent(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    return string.Empty;
                }

                using (SHA256 sha256 = SHA256.Create())
                using (FileStream stream = File.OpenRead(path))
                {
                    return "sha256:" + ToHex(sha256.ComputeHash(stream));
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string Sha256Text(string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return "sha256:" + ToHex(sha256.ComputeHash(Encoding.UTF8.GetBytes(value ?? string.Empty)));
            }
        }

        private static string ToHex(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes)
            {
                builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }
    }

    [Serializable]
    public sealed class KandraRendererPacket
    {
        public string typeName = string.Empty;
        public string name = string.Empty;
        public string path = string.Empty;
        public bool activeSelf;
        public bool activeInHierarchy;
        public bool enabled;
        public bool linkedToCapturedHeroRoot;
        public bool playerBodyCandidate;
        public string linkReason = string.Empty;
        public string renderingId = string.Empty;
        public string meshName = string.Empty;
        public string materialNames = string.Empty;
        public string rigPath = string.Empty;
        public string rigAnimatorName = string.Empty;
        public string rigAnimatorPath = string.Empty;
        public string isRegisteredStatus = string.Empty;
        public string tryGetMeshMemoryStatus = string.Empty;
        public string meshMemory = string.Empty;

        public static KandraRendererPacket From(
            Component component,
            bool linkedToCapturedHeroRoot,
            bool playerBodyCandidate,
            string linkReason,
            bool enabled,
            string renderingId,
            string meshName,
            string materialNames,
            Transform? rigTransform,
            Animator? rigAnimator,
            string isRegisteredStatus,
            string tryGetMeshMemoryStatus,
            string meshMemory)
        {
            Transform root = component.transform == null ? null! : component.transform.root;
            return new KandraRendererPacket
            {
                typeName = component.GetType().FullName ?? component.GetType().Name,
                name = component.name ?? string.Empty,
                path = TransformPath(component.transform, root),
                activeSelf = component.gameObject != null && component.gameObject.activeSelf,
                activeInHierarchy = component.gameObject != null && component.gameObject.activeInHierarchy,
                enabled = enabled,
                linkedToCapturedHeroRoot = linkedToCapturedHeroRoot,
                playerBodyCandidate = playerBodyCandidate,
                linkReason = linkReason,
                renderingId = renderingId,
                meshName = meshName,
                materialNames = materialNames,
                rigPath = TransformPath(rigTransform, root),
                rigAnimatorName = rigAnimator == null ? string.Empty : rigAnimator.name ?? string.Empty,
                rigAnimatorPath = TransformPath(rigAnimator == null ? null : rigAnimator.transform, root),
                isRegisteredStatus = isRegisteredStatus,
                tryGetMeshMemoryStatus = tryGetMeshMemoryStatus,
                meshMemory = meshMemory,
            };
        }
    }

    [Serializable]
    public sealed class TppBodyPrefabReferencePacket
    {
        public string sourceKind = string.Empty;
        public string ownerType = string.Empty;
        public string ownerPath = string.Empty;
        public string sourcePath = string.Empty;
        public string objectType = string.Empty;
        public string objectName = string.Empty;
        public string objectPath = string.Empty;
        public bool activeSelf;
        public bool activeInHierarchy;
        public string sceneName = string.Empty;
        public bool sceneLoaded;
        public string discovery = string.Empty;

        public static TppBodyPrefabReferencePacket From(
            string sourceKind,
            string ownerType,
            string ownerPath,
            string sourcePath,
            GameObject gameObject,
            string discovery)
        {
            Transform? root = gameObject.transform == null ? null : gameObject.transform.root;
            string sceneName = string.Empty;
            bool sceneLoaded = false;
            try
            {
                sceneName = gameObject.scene.IsValid() ? gameObject.scene.name ?? string.Empty : string.Empty;
                sceneLoaded = gameObject.scene.IsValid() && gameObject.scene.isLoaded;
            }
            catch
            {
            }

            return new TppBodyPrefabReferencePacket
            {
                sourceKind = sourceKind,
                ownerType = ownerType,
                ownerPath = ownerPath,
                sourcePath = sourcePath,
                objectType = gameObject.GetType().FullName ?? gameObject.GetType().Name,
                objectName = gameObject.name ?? string.Empty,
                objectPath = TransformPath(gameObject.transform, root),
                activeSelf = gameObject.activeSelf,
                activeInHierarchy = gameObject.activeInHierarchy,
                sceneName = sceneName,
                sceneLoaded = sceneLoaded,
                discovery = discovery,
            };
        }
    }

    [Serializable]
    public sealed class TppAnimationReferencePacket
    {
        public string sourceKind = string.Empty;
        public string ownerType = string.Empty;
        public string ownerPath = string.Empty;
        public string sourcePath = string.Empty;
        public string clipName = string.Empty;
        public string clipType = string.Empty;
        public string clipLength = string.Empty;
        public string clipFrameRate = string.Empty;
        public bool targetClipExact;
        public bool tppTargetClipName;
        public bool acceptedForA2KT34PoseBinding;
        public string bindingSource = string.Empty;

        public static TppAnimationReferencePacket From(
            string sourceKind,
            string ownerType,
            string ownerPath,
            string sourcePath,
            AnimationClip clip,
            bool acceptedForA2KT34PoseBinding,
            string bindingSource)
        {
            string clipName = clip.name ?? string.Empty;
            return new TppAnimationReferencePacket
            {
                sourceKind = sourceKind,
                ownerType = ownerType,
                ownerPath = ownerPath,
                sourcePath = sourcePath,
                clipName = clipName,
                clipType = clip.GetType().FullName ?? clip.GetType().Name,
                clipLength = clip.length.ToString("R", CultureInfo.InvariantCulture),
                clipFrameRate = clip.frameRate.ToString("R", CultureInfo.InvariantCulture),
                targetClipExact = string.Equals(clipName, TargetClip, StringComparison.Ordinal),
                tppTargetClipName =
                    string.Equals(clipName, TargetClip, StringComparison.Ordinal) &&
                    clipName.IndexOf("_TPP_", StringComparison.OrdinalIgnoreCase) >= 0,
                acceptedForA2KT34PoseBinding = acceptedForA2KT34PoseBinding,
                bindingSource = bindingSource,
            };
        }
    }

    [Serializable]
    public sealed class VHeroControllerAssetReferencePacket
    {
        public string sourceKind = string.Empty;
        public string ownerType = string.Empty;
        public string ownerPath = string.Empty;
        public string sourcePath = string.Empty;
        public string declaredType = string.Empty;
        public string valueType = string.Empty;
        public string valueName = string.Empty;
        public bool valuePresent;
        public bool valueRead;
        public bool tppScoped;
        public string runtimeKey = string.Empty;
        public string assetGuid = string.Empty;
        public string subObjectName = string.Empty;
        public string address = string.Empty;
        public string primaryKey = string.Empty;
        public string label = string.Empty;
        public string asset = string.Empty;
        public string rawText = string.Empty;
        public string candidateMemberValues = string.Empty;
        public string matchedQuery = string.Empty;
        public string discovery = string.Empty;

        public static VHeroControllerAssetReferencePacket From(
            string sourceKind,
            string ownerType,
            string ownerPath,
            string sourcePath,
            string declaredType,
            object? value,
            bool valueRead,
            bool tppScoped,
            string runtimeKey,
            string assetGuid,
            string subObjectName,
            string address,
            string primaryKey,
            string label,
            string asset,
            string rawText,
            string candidateMemberValues,
            string matchedQuery)
        {
            string valueType = value == null ? string.Empty : value.GetType().FullName ?? value.GetType().Name;
            string valueName = value is Object unityObject ? unityObject.name ?? string.Empty : string.Empty;
            return new VHeroControllerAssetReferencePacket
            {
                sourceKind = sourceKind,
                ownerType = ownerType,
                ownerPath = ownerPath,
                sourcePath = sourcePath,
                declaredType = declaredType,
                valueType = valueType,
                valueName = valueName,
                valuePresent = value != null,
                valueRead = valueRead,
                tppScoped = tppScoped,
                runtimeKey = runtimeKey,
                assetGuid = assetGuid,
                subObjectName = subObjectName,
                address = address,
                primaryKey = primaryKey,
                label = label,
                asset = asset,
                rawText = rawText,
                candidateMemberValues = candidateMemberValues,
                matchedQuery = matchedQuery,
                discovery = "read-only VHeroController TPP/AssetReference identity; asset was not loaded or instantiated by this observer",
            };
        }
    }

    [Serializable]
    public sealed class AddressableLocatorPacket
    {
        public string sourceKind = string.Empty;
        public string queryKind = string.Empty;
        public string query = string.Empty;
        public string normalizedQuery = string.Empty;
        public string locatorType = string.Empty;
        public int locatorIndex = -1;
        public int keyIndex = -1;
        public string keyText = string.Empty;
        public string normalizedKeyText = string.Empty;
        public string locationType = string.Empty;
        public string locationPrimaryKey = string.Empty;
        public string normalizedLocationPrimaryKey = string.Empty;
        public string locationInternalId = string.Empty;
        public string normalizedLocationInternalId = string.Empty;
        public string locationProviderId = string.Empty;
        public string normalizedLocationProviderId = string.Empty;
        public string locationResourceType = string.Empty;
        public string filePath = string.Empty;
        public string fileName = string.Empty;
        public long fileLength;
        public string fileLastWriteUtc = string.Empty;
        public string linkedBundleFileName = string.Empty;
        public bool bundleBacked;
        public bool catalogBacked;
        public bool targetClipExact;
        public bool tppBodyReferenceName;
        public string discovery = string.Empty;

        public static AddressableLocatorPacket FromResourceLocator(
            string sourceKind,
            string query,
            string locatorType,
            int locatorIndex,
            int keyIndex,
            string keyText,
            string locationType,
            string locationPrimaryKey,
            string locationInternalId,
            string locationProviderId,
            string locationResourceType,
            string discovery,
            string queryKind = "")
        {
            string linkedBundleFileName = FirstNonEmpty(
                ExtractAddressableBundleFileName(locationInternalId),
                ExtractAddressableBundleFileName(locationPrimaryKey),
                ExtractAddressableBundleFileName(keyText),
                ExtractAddressableBundleFileName(query));
            return new AddressableLocatorPacket
            {
                sourceKind = sourceKind,
                queryKind = queryKind,
                query = query,
                normalizedQuery = NormalizeAddressableLocatorIdentity(query),
                locatorType = locatorType,
                locatorIndex = locatorIndex,
                keyIndex = keyIndex,
                keyText = keyText,
                normalizedKeyText = NormalizeAddressableLocatorIdentity(keyText),
                locationType = locationType,
                locationPrimaryKey = locationPrimaryKey,
                normalizedLocationPrimaryKey = NormalizeAddressableLocatorIdentity(locationPrimaryKey),
                locationInternalId = locationInternalId,
                normalizedLocationInternalId = NormalizeAddressableLocatorIdentity(locationInternalId),
                locationProviderId = locationProviderId,
                normalizedLocationProviderId = NormalizeAddressableLocatorIdentity(locationProviderId),
                locationResourceType = locationResourceType,
                linkedBundleFileName = linkedBundleFileName,
                bundleBacked = linkedBundleFileName.Length != 0 || locationInternalId.IndexOf(".bundle", StringComparison.OrdinalIgnoreCase) >= 0,
                catalogBacked = false,
                targetClipExact = string.Equals(query, TargetClip, StringComparison.Ordinal),
                tppBodyReferenceName = IsTppBodyLocatorQuery(query),
                discovery = discovery,
            };
        }

        public static AddressableLocatorPacket FromFile(
            string sourceKind,
            string query,
            string filePath,
            string fileName,
            string linkedBundleFileName,
            bool bundleBacked,
            bool catalogBacked,
            string discovery)
        {
            long fileLength = 0;
            string fileLastWriteUtc = string.Empty;
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                fileLength = fileInfo.Exists ? fileInfo.Length : 0;
                fileLastWriteUtc = fileInfo.Exists
                    ? fileInfo.LastWriteTimeUtc.ToString("O", CultureInfo.InvariantCulture)
                    : string.Empty;
            }
            catch
            {
            }

            return new AddressableLocatorPacket
            {
                sourceKind = sourceKind,
                query = query,
                normalizedQuery = NormalizeAddressableLocatorIdentity(query),
                filePath = filePath,
                fileName = fileName,
                linkedBundleFileName = linkedBundleFileName,
                fileLength = fileLength,
                fileLastWriteUtc = fileLastWriteUtc,
                bundleBacked = bundleBacked,
                catalogBacked = catalogBacked,
                targetClipExact = string.Equals(query, TargetClip, StringComparison.Ordinal),
                tppBodyReferenceName = IsTppBodyLocatorQuery(query),
                discovery = discovery,
            };
        }

        private static bool IsTppBodyLocatorQuery(string query) =>
            AddressableLocatorQueries
                .Where(candidate => !string.Equals(candidate, TargetClip, StringComparison.Ordinal))
                .Any(candidate => string.Equals(candidate, query, StringComparison.Ordinal));

        private static string NormalizeAddressableLocatorIdentity(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string normalized = FormatAddressableLocatorPacketString(value, 512).Trim().Replace('\\', '/');
            const string aaPlatformPrefix = "aa/StandaloneWindows64/";
            const string platformPrefix = "StandaloneWindows64/";
            if (normalized.StartsWith(aaPlatformPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return normalized.Substring(aaPlatformPrefix.Length);
            }

            if (normalized.StartsWith(platformPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return normalized.Substring(platformPrefix.Length);
            }

            const string aaPlatformSegment = "/aa/StandaloneWindows64/";
            int aaPlatformIndex = normalized.LastIndexOf(aaPlatformSegment, StringComparison.OrdinalIgnoreCase);
            if (aaPlatformIndex >= 0)
            {
                return normalized.Substring(aaPlatformIndex + aaPlatformSegment.Length);
            }

            const string platformSegment = "/StandaloneWindows64/";
            int platformIndex = normalized.LastIndexOf(platformSegment, StringComparison.OrdinalIgnoreCase);
            return platformIndex >= 0
                ? normalized.Substring(platformIndex + platformSegment.Length)
                : normalized;
        }

        private static string ExtractAddressableBundleFileName(string value)
        {
            string normalized = NormalizeAddressableLocatorIdentity(value);
            if (normalized.Length == 0)
            {
                return string.Empty;
            }

            int slashIndex = normalized.LastIndexOf('/');
            string fileName = slashIndex >= 0 ? normalized.Substring(slashIndex + 1) : normalized;
            return fileName.EndsWith(".bundle", StringComparison.OrdinalIgnoreCase) ? fileName : string.Empty;
        }

        private static string FirstNonEmpty(params string[] values) =>
            values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

        private static string FormatAddressableLocatorPacketString(string value, int maxLength)
        {
            string normalized = value.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
            return normalized.Length <= maxLength ? normalized : normalized.Substring(0, maxLength);
        }
    }

    [Serializable]
    public sealed class AddressableRuntimeKeyDependencyPacket
    {
        public string sourceKind = string.Empty;
        public string vHeroSourceKind = string.Empty;
        public string vHeroOwnerType = string.Empty;
        public string vHeroOwnerPath = string.Empty;
        public string vHeroSourcePath = string.Empty;
        public string vHeroRuntimeKey = string.Empty;
        public string normalizedVHeroRuntimeKey = string.Empty;
        public string vHeroAddress = string.Empty;
        public string vHeroAssetGuid = string.Empty;
        public string vHeroPrimaryKey = string.Empty;
        public string locatorType = string.Empty;
        public int locatorIndex = -1;
        public int locationIndex = -1;
        public int depth;
        public bool rootLocation;
        public bool dependencyLocation;
        public string locationType = string.Empty;
        public string primaryKey = string.Empty;
        public string normalizedPrimaryKey = string.Empty;
        public string internalId = string.Empty;
        public string normalizedInternalId = string.Empty;
        public string providerId = string.Empty;
        public string normalizedProviderId = string.Empty;
        public string resourceType = string.Empty;
        public string normalizedResourceType = string.Empty;
        public string parentPrimaryKey = string.Empty;
        public string normalizedParentPrimaryKey = string.Empty;
        public string parentInternalId = string.Empty;
        public string normalizedParentInternalId = string.Empty;
        public string parentProviderId = string.Empty;
        public string normalizedParentProviderId = string.Empty;
        public string parentResourceType = string.Empty;
        public string normalizedParentResourceType = string.Empty;
        public string linkedBundleFileName = string.Empty;
        public bool bundleBacked;
        public bool tppBodyRuntimeKey;
        public string discovery = string.Empty;

        public static AddressableRuntimeKeyDependencyPacket FromRuntimeKeyDependency(
            string vHeroSourceKind,
            string vHeroOwnerType,
            string vHeroOwnerPath,
            string vHeroSourcePath,
            string vHeroRuntimeKey,
            string vHeroAddress,
            string vHeroAssetGuid,
            string vHeroPrimaryKey,
            string locatorType,
            int locatorIndex,
            int locationIndex,
            int depth,
            bool rootLocation,
            string locationType,
            string primaryKey,
            string internalId,
            string providerId,
            string resourceType,
            string parentPrimaryKey,
            string parentInternalId,
            string parentProviderId,
            string parentResourceType,
            string discovery)
        {
            string linkedBundleFileName = FirstNonEmpty(
                ExtractAddressableBundleFileName(internalId),
                ExtractAddressableBundleFileName(primaryKey),
                ExtractAddressableBundleFileName(parentInternalId),
                ExtractAddressableBundleFileName(parentPrimaryKey),
                ExtractAddressableBundleFileName(vHeroRuntimeKey),
                ExtractAddressableBundleFileName(vHeroAddress));
            return new AddressableRuntimeKeyDependencyPacket
            {
                sourceKind = "addressables-vhero-runtime-key-dependency",
                vHeroSourceKind = vHeroSourceKind,
                vHeroOwnerType = vHeroOwnerType,
                vHeroOwnerPath = vHeroOwnerPath,
                vHeroSourcePath = vHeroSourcePath,
                vHeroRuntimeKey = vHeroRuntimeKey,
                normalizedVHeroRuntimeKey = NormalizeAddressableDependencyIdentity(vHeroRuntimeKey),
                vHeroAddress = vHeroAddress,
                vHeroAssetGuid = vHeroAssetGuid,
                vHeroPrimaryKey = vHeroPrimaryKey,
                locatorType = locatorType,
                locatorIndex = locatorIndex,
                locationIndex = locationIndex,
                depth = depth,
                rootLocation = rootLocation,
                dependencyLocation = !rootLocation,
                locationType = locationType,
                primaryKey = primaryKey,
                normalizedPrimaryKey = NormalizeAddressableDependencyIdentity(primaryKey),
                internalId = internalId,
                normalizedInternalId = NormalizeAddressableDependencyIdentity(internalId),
                providerId = providerId,
                normalizedProviderId = NormalizeAddressableDependencyIdentity(providerId),
                resourceType = resourceType,
                normalizedResourceType = NormalizeAddressableDependencyIdentity(resourceType),
                parentPrimaryKey = parentPrimaryKey,
                normalizedParentPrimaryKey = NormalizeAddressableDependencyIdentity(parentPrimaryKey),
                parentInternalId = parentInternalId,
                normalizedParentInternalId = NormalizeAddressableDependencyIdentity(parentInternalId),
                parentProviderId = parentProviderId,
                normalizedParentProviderId = NormalizeAddressableDependencyIdentity(parentProviderId),
                parentResourceType = parentResourceType,
                normalizedParentResourceType = NormalizeAddressableDependencyIdentity(parentResourceType),
                linkedBundleFileName = linkedBundleFileName,
                bundleBacked = linkedBundleFileName.Length != 0 ||
                    internalId.IndexOf(".bundle", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    parentInternalId.IndexOf(".bundle", StringComparison.OrdinalIgnoreCase) >= 0,
                tppBodyRuntimeKey =
                    string.Equals(vHeroSourcePath, "maleHeroBodyTPP", StringComparison.Ordinal) ||
                    string.Equals(vHeroSourcePath, "femaleHeroBodyTPP", StringComparison.Ordinal),
                discovery = discovery,
            };
        }

        private static string NormalizeAddressableDependencyIdentity(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string normalized = FormatAddressableDependencyPacketString(value, 512).Trim().Replace('\\', '/');
            const string aaPlatformPrefix = "aa/StandaloneWindows64/";
            const string platformPrefix = "StandaloneWindows64/";
            if (normalized.StartsWith(aaPlatformPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return normalized.Substring(aaPlatformPrefix.Length);
            }

            if (normalized.StartsWith(platformPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return normalized.Substring(platformPrefix.Length);
            }

            const string aaPlatformSegment = "/aa/StandaloneWindows64/";
            int aaPlatformIndex = normalized.LastIndexOf(aaPlatformSegment, StringComparison.OrdinalIgnoreCase);
            if (aaPlatformIndex >= 0)
            {
                return normalized.Substring(aaPlatformIndex + aaPlatformSegment.Length);
            }

            const string platformSegment = "/StandaloneWindows64/";
            int platformIndex = normalized.LastIndexOf(platformSegment, StringComparison.OrdinalIgnoreCase);
            return platformIndex >= 0
                ? normalized.Substring(platformIndex + platformSegment.Length)
                : normalized;
        }

        private static string ExtractAddressableBundleFileName(string value)
        {
            string normalized = NormalizeAddressableDependencyIdentity(value);
            if (normalized.Length == 0)
            {
                return string.Empty;
            }

            int slashIndex = normalized.LastIndexOf('/');
            string fileName = slashIndex >= 0 ? normalized.Substring(slashIndex + 1) : normalized;
            return fileName.EndsWith(".bundle", StringComparison.OrdinalIgnoreCase) ? fileName : string.Empty;
        }

        private static string FirstNonEmpty(params string[] values) =>
            values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

        private static string FormatAddressableDependencyPacketString(string value, int maxLength)
        {
            string normalized = value.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
            return normalized.Length <= maxLength ? normalized : normalized.Substring(0, maxLength);
        }
    }

    [Serializable]
    public sealed class AddressableLocatorDiagnosticPacket
    {
        public string sourceKind = string.Empty;
        public string status = string.Empty;
        public string locatorType = string.Empty;
        public int locatorIndex = -1;
        public int keyCount;
        public int keyScanLimit;
        public bool keyScanTruncated;
        public int keySampleCount;
        public string keySamples = string.Empty;
        public string queryKind = string.Empty;
        public string query = string.Empty;
        public bool locateSucceeded;
        public int locationCount;
        public string locationTypes = string.Empty;
        public string locationPrimaryKeys = string.Empty;
        public string locationInternalIds = string.Empty;
        public string locationProviderIds = string.Empty;
        public string locationResourceTypes = string.Empty;
        public string catalogPath = string.Empty;
        public string catalogFileName = string.Empty;
        public string catalogSearchRoot = string.Empty;
        public long fileLength;
        public string fileLastWriteUtc = string.Empty;
        public bool catalogContainsDirectQuery;
        public bool catalogContainsMatchedBundleAlias;
        public string matchedBundleAliases = string.Empty;
        public string discovery = string.Empty;

        public static AddressableLocatorDiagnosticPacket FromDirectLocate(
            string locatorType,
            int locatorIndex,
            string queryKind,
            string query,
            bool located,
            IEnumerable<object> locations,
            string discovery)
        {
            object[] locationArray = locations?.Take(8).ToArray() ?? Array.Empty<object>();
            return new AddressableLocatorDiagnosticPacket
            {
                sourceKind = "addressables-resource-locator-direct-locate",
                status = located ? "located" : "not-located",
                locatorType = locatorType,
                locatorIndex = locatorIndex,
                queryKind = queryKind,
                query = query,
                locateSucceeded = located,
                locationCount = locationArray.Length,
                locationTypes = JoinLocationValues(locationArray, location => location.GetType().FullName ?? location.GetType().Name),
                locationPrimaryKeys = JoinLocationValues(locationArray, location => ReadLocationMemberString(location, "PrimaryKey")),
                locationInternalIds = JoinLocationValues(locationArray, location => ReadLocationMemberString(location, "InternalId")),
                locationProviderIds = JoinLocationValues(locationArray, location => ReadLocationMemberString(location, "ProviderId")),
                locationResourceTypes = JoinLocationValues(locationArray, location => ReadLocationMemberString(location, "ResourceType")),
                discovery = discovery,
            };
        }

        public static AddressableLocatorDiagnosticPacket FromCatalogFile(
            string catalogFile,
            string catalogFileName,
            string catalogSearchRoot,
            bool containsDirectQuery,
            bool containsMatchedBundleAlias,
            IEnumerable<string> matchedBundleAliases,
            string discovery)
        {
            long fileLength = 0;
            string fileLastWriteUtc = string.Empty;
            try
            {
                FileInfo fileInfo = new FileInfo(catalogFile);
                fileLength = fileInfo.Exists ? fileInfo.Length : 0;
                fileLastWriteUtc = fileInfo.Exists
                    ? fileInfo.LastWriteTimeUtc.ToString("O", CultureInfo.InvariantCulture)
                    : string.Empty;
            }
            catch
            {
            }

            return new AddressableLocatorDiagnosticPacket
            {
                sourceKind = "addressables-installed-catalog-file",
                status = containsMatchedBundleAlias
                    ? "catalog-references-matched-bundle-alias"
                    : containsDirectQuery
                        ? "catalog-references-direct-query"
                        : "catalog-does-not-reference-direct-query-or-matched-bundle-alias",
                catalogPath = catalogFile,
                catalogFileName = catalogFileName,
                catalogSearchRoot = catalogSearchRoot,
                fileLength = fileLength,
                fileLastWriteUtc = fileLastWriteUtc,
                catalogContainsDirectQuery = containsDirectQuery,
                catalogContainsMatchedBundleAlias = containsMatchedBundleAlias,
                matchedBundleAliases = string.Join("|", matchedBundleAliases.Distinct(StringComparer.Ordinal).Take(16)),
                discovery = discovery,
            };
        }

        private static string JoinLocationValues(IEnumerable<object> locations, Func<object, string> valueFactory) =>
            string.Join(
                "|",
                locations
                    .Select(valueFactory)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => FormatPacketString(value, 192)));

        private static string ReadLocationMemberString(object? value, string memberName)
        {
            object? member = ReadLocationMember(value, memberName);
            if (member == null)
            {
                return string.Empty;
            }

            return FormatPacketString(Convert.ToString(member, CultureInfo.InvariantCulture) ?? string.Empty, 512);
        }

        private static object? ReadLocationMember(object? value, string memberName)
        {
            if (value == null)
            {
                return null;
            }

            try
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                Type type = value.GetType();
                PropertyInfo? property = type.GetProperty(memberName, flags);
                if (property != null && property.GetIndexParameters().Length == 0)
                {
                    return property.GetValue(value, null);
                }

                FieldInfo? field = type.GetField(memberName, flags);
                return field == null ? null : field.GetValue(value);
            }
            catch
            {
                return null;
            }
        }

        private static string FormatPacketString(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            string normalized = value.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
            return normalized.Length <= maxLength ? normalized : normalized.Substring(0, maxLength);
        }
    }

    [Serializable]
    public sealed class PoseBindingOwnerPacket
    {
        public string typeName = string.Empty;
        public string name = string.Empty;
        public string path = string.Empty;
        public bool activeSelf;
        public bool activeInHierarchy;
        public bool enabled;
        public string ownerDomain = string.Empty;

        public static PoseBindingOwnerPacket From(Component component)
        {
            Transform? root = component.transform == null ? null : component.transform.root;
            return new PoseBindingOwnerPacket
            {
                typeName = component.GetType().FullName ?? component.GetType().Name,
                name = component.name ?? string.Empty,
                path = TransformPath(component.transform, root),
                activeSelf = component.gameObject != null && component.gameObject.activeSelf,
                activeInHierarchy = component.gameObject != null && component.gameObject.activeInHierarchy,
                enabled = component is Behaviour behaviour && behaviour.enabled,
                ownerDomain = "live-ARHeroAnimancer-owner-graph",
            };
        }
    }

    [Serializable]
    public sealed class PoseBindingCandidatePacket
    {
        public string sourceKind = string.Empty;
        public string ownerType = string.Empty;
        public string ownerPath = string.Empty;
        public string sourcePath = string.Empty;
        public string clipName = string.Empty;
        public string clipType = string.Empty;
        public string clipLength = string.Empty;
        public string clipFrameRate = string.Empty;
        public bool targetClipExact;
        public bool tppTargetClipName;
        public bool activeHeroAnimancerOwner;
        public bool currentAnimatorPlayback;
        public bool runtimeAnimatorControllerCatalog;
        public bool acceptedForA2KT34PoseBinding;
        public string bindingSource = string.Empty;

        public static PoseBindingCandidatePacket FromHeroAnimancer(
            Component owner,
            string ownerType,
            string ownerPath,
            string sourcePath,
            AnimationClip clip,
            bool acceptedForA2KT34PoseBinding,
            string bindingSource)
        {
            string clipName = clip.name ?? string.Empty;
            return new PoseBindingCandidatePacket
            {
                sourceKind = "live-ARHeroAnimancer-owner-graph",
                ownerType = ownerType,
                ownerPath = ownerPath,
                sourcePath = sourcePath,
                clipName = clipName,
                clipType = clip.GetType().FullName ?? clip.GetType().Name,
                clipLength = clip.length.ToString("R", CultureInfo.InvariantCulture),
                clipFrameRate = clip.frameRate.ToString("R", CultureInfo.InvariantCulture),
                targetClipExact = string.Equals(clipName, TargetClip, StringComparison.Ordinal),
                tppTargetClipName =
                    string.Equals(clipName, TargetClip, StringComparison.Ordinal) &&
                    clipName.IndexOf("_TPP_", StringComparison.OrdinalIgnoreCase) >= 0,
                activeHeroAnimancerOwner = true,
                currentAnimatorPlayback = false,
                runtimeAnimatorControllerCatalog = false,
                acceptedForA2KT34PoseBinding = acceptedForA2KT34PoseBinding,
                bindingSource = bindingSource,
            };
        }

        public static PoseBindingCandidatePacket FromAnimator(
            string sourceKind,
            string ownerPath,
            AnimatorPacket row,
            bool acceptedForA2KT34PoseBinding,
            string bindingSource)
        {
            return new PoseBindingCandidatePacket
            {
                sourceKind = sourceKind,
                ownerType = "UnityEngine.Animator",
                ownerPath = ownerPath,
                sourcePath = sourceKind,
                clipName = TargetClip,
                clipType = "UnityEngine.AnimationClip",
                targetClipExact = true,
                tppTargetClipName = TargetClip.IndexOf("_TPP_", StringComparison.OrdinalIgnoreCase) >= 0,
                activeHeroAnimancerOwner = false,
                currentAnimatorPlayback = string.Equals(sourceKind, "current-live-Animator-clip", StringComparison.Ordinal),
                runtimeAnimatorControllerCatalog = string.Equals(sourceKind, "diagnostic-runtime-AnimatorController-catalog", StringComparison.Ordinal),
                acceptedForA2KT34PoseBinding = acceptedForA2KT34PoseBinding,
                bindingSource = bindingSource + "; animator=" + row.name + "; controller=" + row.controllerName,
            };
        }

        public static PoseBindingCandidatePacket FromAnimationReference(
            string sourceKind,
            string ownerType,
            string ownerPath,
            string sourcePath,
            AnimationClip clip,
            bool acceptedForA2KT34PoseBinding,
            string bindingSource)
        {
            string clipName = clip.name ?? string.Empty;
            return new PoseBindingCandidatePacket
            {
                sourceKind = sourceKind,
                ownerType = ownerType,
                ownerPath = ownerPath,
                sourcePath = sourcePath,
                clipName = clipName,
                clipType = clip.GetType().FullName ?? clip.GetType().Name,
                clipLength = clip.length.ToString("R", CultureInfo.InvariantCulture),
                clipFrameRate = clip.frameRate.ToString("R", CultureInfo.InvariantCulture),
                targetClipExact = string.Equals(clipName, TargetClip, StringComparison.Ordinal),
                tppTargetClipName =
                    string.Equals(clipName, TargetClip, StringComparison.Ordinal) &&
                    clipName.IndexOf("_TPP_", StringComparison.OrdinalIgnoreCase) >= 0,
                activeHeroAnimancerOwner = false,
                currentAnimatorPlayback = false,
                runtimeAnimatorControllerCatalog =
                    sourceKind.IndexOf("runtime-animator-controller", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    sourceKind.IndexOf("AnimatorController", StringComparison.OrdinalIgnoreCase) >= 0,
                acceptedForA2KT34PoseBinding = acceptedForA2KT34PoseBinding,
                bindingSource = bindingSource,
            };
        }
    }

    [Serializable]
    public sealed class AnimatorPacket
    {
        public string name = string.Empty;
        public string path = string.Empty;
        public bool enabled;
        public bool isHuman;
        public int layerCount;
        public string controllerName = string.Empty;
        public string currentClipNames = string.Empty;
        public string controllerClipNames = string.Empty;
        public string normalizedTimes = string.Empty;

        public static AnimatorPacket From(Animator animator, Transform root)
        {
            List<string> clipNames = new List<string>();
            List<string> controllerClipNames = new List<string>();
            List<string> times = new List<string>();
            int layerCount = Math.Max(0, animator.layerCount);
            RuntimeAnimatorController? controller = null;
            try
            {
                controller = animator.runtimeAnimatorController;
                foreach (AnimationClip clip in controller == null ? Array.Empty<AnimationClip>() : controller.animationClips ?? Array.Empty<AnimationClip>())
                {
                    if (clip != null)
                    {
                        controllerClipNames.Add(clip.name ?? string.Empty);
                    }
                }
            }
            catch
            {
                controllerClipNames.Add("unavailable");
            }

            for (int layer = 0; layer < layerCount; layer++)
            {
                try
                {
                    AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(layer);
                    times.Add(layer.ToString(CultureInfo.InvariantCulture) + ":" + state.normalizedTime.ToString("R", CultureInfo.InvariantCulture));
                    foreach (AnimatorClipInfo info in animator.GetCurrentAnimatorClipInfo(layer))
                    {
                        if (info.clip != null)
                        {
                            clipNames.Add(info.clip.name ?? string.Empty);
                        }
                    }
                }
                catch
                {
                    times.Add(layer.ToString(CultureInfo.InvariantCulture) + ":unavailable");
                }
            }

            return new AnimatorPacket
            {
                name = animator.name ?? string.Empty,
                path = TransformPath(animator.transform, root),
                enabled = animator.enabled,
                isHuman = animator.isHuman,
                layerCount = layerCount,
                controllerName = controller == null ? string.Empty : controller.name ?? string.Empty,
                currentClipNames = string.Join("|", clipNames.Distinct(StringComparer.Ordinal)),
                controllerClipNames = string.Join("|", controllerClipNames.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.Ordinal)),
                normalizedTimes = string.Join("|", times),
            };
        }
    }

    [Serializable]
    public sealed class DownstreamBoundary
    {
        public bool candidateMapApplicationAllowed;
        public bool candidateMapApplicationExecuted;
        public bool conversionAllowed;
        public bool conversionExecuted;
        public bool sidecarGenerationAllowed;
        public bool sidecarGenerationExecuted;
        public bool sourceFbxMutationExecuted;
        public bool unityProjectAssetMutationExecuted;
        public bool nativeGameFileWriteExecuted;
        public bool runtimeLoaderChangeExecuted;
        public bool itemRegistrationExecuted;
        public bool equipExecuted;
        public bool inventoryWriteExecuted;
        public bool inventoryEquipSaveMutationExecuted;
        public bool saveWriteExecuted;

        public static DownstreamBoundary False() =>
            new DownstreamBoundary
            {
                candidateMapApplicationAllowed = false,
                candidateMapApplicationExecuted = false,
                conversionAllowed = false,
                conversionExecuted = false,
                sidecarGenerationAllowed = false,
                sidecarGenerationExecuted = false,
                sourceFbxMutationExecuted = false,
                unityProjectAssetMutationExecuted = false,
                nativeGameFileWriteExecuted = false,
                runtimeLoaderChangeExecuted = false,
                itemRegistrationExecuted = false,
                equipExecuted = false,
                inventoryWriteExecuted = false,
                inventoryEquipSaveMutationExecuted = false,
                saveWriteExecuted = false,
            };
    }

    private static string TransformPath(Transform? transform, Transform? root)
    {
        if (transform == null)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        Transform? current = transform;
        for (int guard = 0; guard < 96 && current != null; guard++)
        {
            parts.Add(current.name ?? string.Empty);
            if (root != null && current == root)
            {
                break;
            }

            current = current.parent;
        }

        parts.Reverse();
        return "/" + string.Join("/", parts);
    }

    private static bool ContainsClipName(string delimitedClipNames, string clipName) =>
        (delimitedClipNames ?? string.Empty)
        .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
        .Any(value => string.Equals(value.Trim(), clipName, StringComparison.Ordinal));

    private static string JoinNonEmpty(params string[] values)
    {
        string joined = string.Join("|", values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .SelectMany(value => value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            .Select(value => value.Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal));
        return string.IsNullOrWhiteSpace(joined)
            ? "unobserved:per-row-live-animator-binding-not-captured"
            : joined;
    }

    private static string FormatVector(Vector3 value) =>
        value.x.ToString("R", CultureInfo.InvariantCulture)
        + "|"
        + value.y.ToString("R", CultureInfo.InvariantCulture)
        + "|"
        + value.z.ToString("R", CultureInfo.InvariantCulture);
}
