using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Locations.Setup;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace DragonKnight.TemplateProof
{
    /// <summary>
    /// Authors and validates only the separately approved disposable Dragon Knight
    /// NpcTemplate and LocationTemplate proof. It copies the reviewed
    /// Foredweller T6 Knight templates into an isolated Dragon Knight template pack,
    /// builds a two-root Addressables catalogue, and validates two finalized
    /// load/release cycles. It does not write runtime source, deploy to FoA,
    /// register templates in the installed game, construct or spawn an actor,
    /// access a save, wire the user's AI system, test combat/death, or place
    /// Dragon Knight into population.
    /// </summary>
    internal static class DragonKnightDK4ATemplateAuthoring
    {
        private const string RequiredUnityVersion = "6000.0.64f1";
        private const string ModFolderName = "DragonKnightDK4ATemplate";
        private const string ProofLabel = "dragon-knight-dk4a-template-proof";
        private const string TemplateLabel = "template";
        private const string TemplateSoLabel = "templateSO";
        private const string GroupName = "Dragon Knight DK4A Template Proof";
        private const string BuildVariable = "DragonKnightDK4ATemplateBuildPath";
        private const string LoadVariable = "DragonKnightDK4ATemplateLoadPath";
        private const string OutputRoot = @"<local-path>";

        private const string ForedwellerNpcPath = "Assets/Data/Templates/NpcTemplates/Enemies/Foredwellers/NPCTemplate_EnemyForedweller_T6_Knight.prefab";
        private const string ForedwellerAbstractNpcPath = "Assets/Data/Templates/NpcTemplates/Abstracts/Abstract_NPCTemplate_Foredweller.prefab";
        private const string ForedwellerLocationPath = "Assets/Data/LocationSpecs/AI/Enemies/ForeDwellers/Spec_EnemyForedweller_T6_Knight.prefab";
        private const string ForedwellerNpcSha256 = "95E860FC25705F3D8FAD997B20CD260D1A38271723B587CEA47BDE741106E933";
        private const string ForedwellerAbstractNpcSha256 = "AC3FF9FE750BF5820F14FF52A8B2BFBF63C0D951D022E0B17F0746119BEF4608";
        private const string ForedwellerLocationSha256 = "03C7E647295DF1D09541139D9280C3A4DDD356EB543BF060B9CCE455630A91BD";

        private const string TargetRoot = "Assets/DragonKnightDK4ATemplate";
        private const string TemplateRoot = TargetRoot + "/Templates";
        private const string NpcPath = TemplateRoot + "/NPCTemplate_DragonKnight_DK4A.prefab";
        private const string LocationPath = TemplateRoot + "/Spec_DragonKnight_DK4A.prefab";

        private const string ApprovedDisplayName = "Dragon Knight";
        private const string LocationDisplayNameId = "dragon_knight_dk4a";
        private const string ApprovedFactionGuid = "4c90d92d219d54a4f8918310ba9998a7";
        private const string ApprovedAbstractType0 = "2d89c9bd6158a1049b7460cee96ed7dd";
        private const string ApprovedAbstractType1 = "30fee903493e89e4089f2aeeb5e42d2a";
        private const string ApprovedAbstractType2 = "d11cfa3551773034eacc4e3d4cec7183";
        private const string ApprovedFightingStyleGuid = "471a14b9dbb41b146a82cb5750afe26a";
        private const string NativeBootstrapShellAddress = "83d478bb2e0ea6e4d9e1cc58f3ec6910";
        private const string DragonKnightNativeVisualAddress = "b4d3a4e9a58fb1c4ea4c239f267faa56";
        private const string SimplifiedDeadBodyPrefabAddress = "cee084d53322ec04ca39752e9754314a";
        private const string HitVfxAddress = "6435b816784ea434589f4b48e31ccf70";
        private const string Marker = "dragon-knight-dk4a-template-v1; Dragon Knight with Foredweller T6 boss proof profile approved 2026-08-01";

        private static readonly AssetSpec[] ExplicitAssets =
        {
            new(NpcPath, "npc-template"),
            new(LocationPath, "location-template"),
        };

        private static readonly string[] ExpectedLocationComponentShortNames =
        {
            "Transform",
            "LocationSpec",
            "LocationTemplate",
            "RepetitiveNpcAttachment",
            "IdleDataAttachment",
            "CustomCombatAttachment",
            "AliveAudioAttachment",
            "MarkerAttachment",
        };

        private static readonly string[] ApprovedAbstractTypes =
        {
            ApprovedAbstractType0,
            ApprovedAbstractType1,
            ApprovedAbstractType2,
        };

        [Serializable]
        private sealed class Report
        {
            public string gate = "CI4 successor Dragon Knight NpcTemplate/LocationTemplate authoring/build/finalized load-release proof";
            public string result = string.Empty;
            public string unityVersion = string.Empty;
            public string approvedDisplayName = ApprovedDisplayName;
            public string locationDisplayNameId = LocationDisplayNameId;
            public string nativeBootstrapShellAddress = NativeBootstrapShellAddress;
            public string dragonKnightNativeVisualAddress = DragonKnightNativeVisualAddress;
            public string buildRoot = string.Empty;
            public string runtimeLoadRoot = string.Empty;
            public string cataloguePath = string.Empty;
            public string catalogueSha256 = string.Empty;
            public AssetRecord[] explicitAssets = Array.Empty<AssetRecord>();
            public SourceHash[] sourceHashes = Array.Empty<SourceHash>();
            public Artifact[] artifacts = Array.Empty<Artifact>();
            public LoadCycle[] loadCycles = Array.Empty<LoadCycle>();
            public string approvedFactionGuid = ApprovedFactionGuid;
            public string approvedFightingStyleGuid = ApprovedFightingStyleGuid;
            public string[] approvedAbstractTypeGuids = ApprovedAbstractTypes;
            public int level;
            public int maxHealth;
            public int maxStamina;
            public float staminaRegenPerTick;
            public float meleeDamage;
            public float rangedDamage;
            public float magicDamage;
            public int npcWeight;
            public float poiseThreshold;
            public int expLevel;
            public int expTier;
            public int expReward;
            public int npcType;
            public bool deadBodyLootable;
            public bool exactApprovedProfileValid;
            public int locationComponentCount;
            public string[] locationComponentTypes = Array.Empty<string>();
            public string[] locationComponentShortNames = Array.Empty<string>();
            public bool exactNativeForedwellerLocationOrderPreserved;
            public bool nonUniqueNpcAttachment;
            public bool templateLabelsValid;
            public bool templateSoAbsentValid;
            public bool npcTemplateCreated;
            public bool locationTemplateCreated;
            public bool actorConstructed;
            public bool templateRegistered;
            public bool runtimeSourceWritten;
            public bool userAiBridgeWired;
            public bool gameDeployment;
            public bool spawnRequested;
            public bool saveAccess;
            public string failure = string.Empty;
        }

        [Serializable]
        private sealed class AssetRecord
        {
            public string kind = string.Empty;
            public string path = string.Empty;
            public string guid = string.Empty;
            public string address = string.Empty;
        }

        [Serializable]
        private sealed class SourceHash
        {
            public string path = string.Empty;
            public string beforeSha256 = string.Empty;
            public string afterSha256 = string.Empty;
            public bool unchanged;
        }

        [Serializable]
        private sealed class Artifact
        {
            public string relativePath = string.Empty;
            public long bytes;
            public string sha256 = string.Empty;
        }

        [Serializable]
        private sealed class LoadCycle
        {
            public int cycle;
            public int loadedAssetCount;
            public bool allHandlesReleased;
            public string fingerprint = string.Empty;
        }

        private sealed class AssetSpec
        {
            public readonly string path;
            public readonly string kind;

            public AssetSpec(string path, string kind)
            {
                this.path = path;
                this.kind = kind;
            }
        }

        private sealed class SettingsRestore
        {
            public bool buildRemoteCatalog;
            public string remoteBuildVariable = string.Empty;
            public string remoteLoadVariable = string.Empty;
            public string overridePlayerVersion = string.Empty;
            public readonly Dictionary<BundledAssetGroupSchema, bool> includeStates = new();
            public readonly Dictionary<BundledAssetGroupSchema, string> buildVariables = new();
            public readonly Dictionary<BundledAssetGroupSchema, string> loadVariables = new();
        }

        public static void AuthorAndBuild()
        {
            Report report = NewReport();
            int exitCode = 1;
            AddressableAssetSettings settings = null;
            SettingsRestore restore = null;
            try
            {
                Preflight();
                report.sourceHashes = SnapshotSources();
                PrepareOutputRoot();
                AuthorTemplates();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                ValidateProjectAssets(report);
                CompleteSourceHashes(report.sourceHashes);
                Require(report.sourceHashes.All(item => item.unchanged),
                    "A guarded Dragon Knight/Foredweller source changed during template authoring.");

                settings = AddressableAssetSettingsDefaultObject.Settings;
                Require(settings != null, "Addressables settings are missing.");
                restore = ConfigureAddressables(settings, report);
                AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult buildResult);
                Require(buildResult != null && string.IsNullOrEmpty(buildResult.Error),
                    "Dragon Knight template Addressables build failed: " + buildResult?.Error);
                report.artifacts = SnapshotArtifacts();
                report.result = "PASS";
                WriteReport(BuildReportPath(), report);
                Debug.Log("DRAGON_KNIGHT_DK4A_TEMPLATE_BUILD_PASS explicitAssets=2; npcTemplateCreated=true; locationTemplateCreated=true; actorConstructed=false; spawnRequested=false; gameDeployment=false; saveAccess=false");
                exitCode = 0;
            }
            catch (Exception exception)
            {
                report.result = "FAIL";
                report.failure = exception.ToString();
                TryWriteReport(BuildReportPath(), report);
                Debug.LogException(exception);
            }
            finally
            {
                if (settings != null && restore != null)
                {
                    try { RestoreAddressables(settings, restore); }
                    catch (Exception exception) { Debug.LogException(exception); exitCode = 1; }
                }
                EditorApplication.Exit(exitCode);
            }
        }

        public static void ValidateFinalized()
        {
            Report report = NewReport();
            int exitCode = 1;
            try
            {
                Require(Application.unityVersion == RequiredUnityVersion,
                    "Dragon Knight template finalized validation requires Unity " + RequiredUnityVersion + "; actual=" + Application.unityVersion);
                Require(File.Exists(BuildReportPath()), "Dragon Knight template build report is missing.");
                report = JsonUtility.FromJson<Report>(File.ReadAllText(BuildReportPath()));
                Require(report != null && report.result == "PASS", "Dragon Knight template build report has not passed.");
                report.result = string.Empty;
                report.failure = string.Empty;
                VerifySourceHashes(report.sourceHashes);
                string cataloguePath = FindCatalogue();
                report.cataloguePath = cataloguePath;
                report.catalogueSha256 = HashFile(cataloguePath);
                string expectedRuntimeRoot = Normalize(Path.Combine(Application.persistentDataPath, "Mods", ModFolderName,
                    EditorUserBuildSettings.activeBuildTarget.ToString()));
                Func<IResourceLocation, string> previousTransform = Addressables.InternalIdTransformFunc;
                Addressables.InternalIdTransformFunc = location =>
                    MapProofInternalId(location.InternalId, expectedRuntimeRoot, BuildRoot());
                try
                {
                    report.loadCycles = new[]
                    {
                        RunLoadCycle(cataloguePath, report, 1),
                        RunLoadCycle(cataloguePath, report, 2),
                    };
                }
                finally
                {
                    Addressables.InternalIdTransformFunc = previousTransform;
                }
                Require(report.loadCycles.All(item => item.loadedAssetCount == ExplicitAssets.Length && item.allHandlesReleased),
                    "One or more finalized Dragon Knight template cycles did not load and release both roots.");
                Require(report.loadCycles[0].fingerprint == report.loadCycles[1].fingerprint,
                    "Finalized Dragon Knight template fingerprints differ between cycles.");
                report.artifacts = SnapshotArtifacts();
                report.result = "PASS";
                WriteReport(FinalReportPath(), report);
                Debug.Log("DRAGON_KNIGHT_DK4A_TEMPLATE_FINALIZED_PASS explicitAssets=2; loadCycles=2; templateRegistered=false; actorConstructed=false; spawnRequested=false; gameDeployment=false; saveAccess=false");
                exitCode = 0;
            }
            catch (Exception exception)
            {
                report.result = "FAIL";
                report.failure = exception.ToString();
                TryWriteReport(FinalReportPath(), report);
                Debug.LogException(exception);
            }
            finally
            {
                EditorApplication.Exit(exitCode);
            }
        }

        private static Report NewReport() => new()
        {
            unityVersion = Application.unityVersion,
            buildRoot = BuildRoot(),
            runtimeLoadRoot = RuntimeLoadRoot(),
            npcTemplateCreated = false,
            locationTemplateCreated = false,
            actorConstructed = false,
            templateRegistered = false,
            runtimeSourceWritten = false,
            userAiBridgeWired = false,
            gameDeployment = false,
            spawnRequested = false,
            saveAccess = false,
        };

        private static void Preflight()
        {
            Require(Application.unityVersion == RequiredUnityVersion,
                "Dragon Knight template authoring requires Unity " + RequiredUnityVersion + "; actual=" + Application.unityVersion);
            Require(HashFile(ProjectPath(ForedwellerNpcPath)) == ForedwellerNpcSha256,
                "Reviewed Foredweller NPC template hash changed.");
            Require(HashFile(ProjectPath(ForedwellerAbstractNpcPath)) == ForedwellerAbstractNpcSha256,
                "Reviewed Foredweller NPC source template hash changed.");
            Require(HashFile(ProjectPath(ForedwellerLocationPath)) == ForedwellerLocationSha256,
                "Reviewed Foredweller location template hash changed.");

            GameObject npc = AssetDatabase.LoadAssetAtPath<GameObject>(ForedwellerNpcPath);
            Require(npc?.GetComponent<NpcTemplate>() != null,
                "Reviewed Foredweller NPC template source is missing NpcTemplate.");
            GameObject location = AssetDatabase.LoadAssetAtPath<GameObject>(ForedwellerLocationPath);
            Require(location?.GetComponent<LocationTemplate>() != null &&
                    location.GetComponent<LocationSpec>() != null &&
                    location.GetComponent<RepetitiveNpcAttachment>() != null,
                "Reviewed Foredweller LocationTemplate source is incomplete.");
            Require(ComponentShortNames(location).SequenceEqual(ExpectedLocationComponentShortNames),
                "Reviewed Foredweller location component order no longer matches the approved humanoid order.");
        }

        private static void PrepareOutputRoot()
        {
            string resolved = Path.GetFullPath(OutputRoot).TrimEnd(Path.DirectorySeparatorChar);
            Require(string.Equals(resolved, @"<local-path>", StringComparison.OrdinalIgnoreCase),
                "Refusing to prepare an unexpected Dragon Knight template output root: " + resolved);
            if (Directory.Exists(resolved)) Directory.Delete(resolved, true);
            Directory.CreateDirectory(resolved);
        }

        private static void AuthorTemplates()
        {
            if (AssetDatabase.IsValidFolder(TargetRoot))
            {
                Require(AssetDatabase.DeleteAsset(TargetRoot), "Could not delete the previous isolated Dragon Knight template target root.");
            }
            EnsureFolder(TemplateRoot);
            Require(AssetDatabase.CopyAsset(ForedwellerNpcPath, NpcPath),
                "Could not copy the reviewed Foredweller NpcTemplate.");
            Require(AssetDatabase.CopyAsset(ForedwellerLocationPath, LocationPath),
                "Could not copy the reviewed Foredweller LocationTemplate.");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            RewriteNpcTemplate();
            RewriteLocationTemplate();
        }

        private static void RewriteNpcTemplate()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(NpcPath);
            try
            {
                root.name = "NPCTemplate_DragonKnight_DK4A";
                NpcTemplate template = root.GetComponent<NpcTemplate>();
                Require(template != null, "Copied Dragon Knight NPC lost NpcTemplate.");
                SerializedObject serialized = new(template);
                SetString(serialized, "metadata.notes", Marker);
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, NpcPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void RewriteLocationTemplate()
        {
            string npcGuid = AssetDatabase.AssetPathToGUID(NpcPath);
            Require(!string.IsNullOrWhiteSpace(npcGuid), "Dragon Knight NpcTemplate has no GUID.");
            GameObject root = PrefabUtility.LoadPrefabContents(LocationPath);
            try
            {
                root.name = "Spec_DragonKnight_DK4A";
                LocationSpec spec = root.GetComponent<LocationSpec>();
                LocationTemplate template = root.GetComponent<LocationTemplate>();
                RepetitiveNpcAttachment attachment = root.GetComponent<RepetitiveNpcAttachment>();
                Require(spec != null && template != null && attachment != null,
                    "Copied Dragon Knight location lost its native Foredweller location/NPC components.");
                SerializedObject specSerialized = new(spec);
                SetString(specSerialized, "displayName.IdOverride", LocationDisplayNameId);
                SetString(specSerialized, "displayName.ID", "Template/displayName_dragon_knight_dk4a");
                SetString(specSerialized, "prefabReference.address", NativeBootstrapShellAddress);
                SetString(specSerialized, "prefabReference.subObjectName", string.Empty);
                specSerialized.ApplyModifiedPropertiesWithoutUndo();
                SerializedObject templateSerialized = new(template);
                SetString(templateSerialized, "metadata.notes", Marker);
                templateSerialized.ApplyModifiedPropertiesWithoutUndo();
                SerializedObject attachmentSerialized = new(attachment);
                SetString(attachmentSerialized, "npcTemplate._guid", npcGuid);
                SetString(attachmentSerialized, "visualPrefab.address", DragonKnightNativeVisualAddress);
                SetString(attachmentSerialized, "visualPrefab.subObjectName", string.Empty);
                SetString(attachmentSerialized, "hitVFXReference.arReference.address", HitVfxAddress);
                SetString(attachmentSerialized, "simplifiedDeadBodyPrefab.arReference.address", SimplifiedDeadBodyPrefabAddress);
                SetArraySize(attachmentSerialized, "potentialActors", 0);
                attachmentSerialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, LocationPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ValidateProjectAssets(Report report)
        {
            GameObject npcRoot = AssetDatabase.LoadAssetAtPath<GameObject>(NpcPath);
            GameObject locationRoot = AssetDatabase.LoadAssetAtPath<GameObject>(LocationPath);
            Require(npcRoot != null && locationRoot != null, "Dragon Knight template roots failed to reload.");
            string npcGuid = AssetDatabase.AssetPathToGUID(NpcPath);
            string locationGuid = AssetDatabase.AssetPathToGUID(LocationPath);
            Require(!string.IsNullOrWhiteSpace(npcGuid) && !string.IsNullOrWhiteSpace(locationGuid) &&
                    npcGuid != locationGuid &&
                    !string.Equals(npcGuid, "439a2dc3cf64e4e4aa792be5b4034cf1", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(locationGuid, "24ee850d0ffdbf64ea2b07b6a718d63b", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(npcGuid, "d11cfa3551773034eacc4e3d4cec7183", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(locationGuid, "d11cfa3551773034eacc4e3d4cec7183", StringComparison.OrdinalIgnoreCase),
                "Dragon Knight template GUID ownership is invalid, colliding, or reused from native Foredweller.");
            ValidateNpc(npcRoot.GetComponent<NpcTemplate>(), report);
            ValidateLocation(locationRoot, npcGuid, report);
            report.npcTemplateCreated = true;
            report.locationTemplateCreated = true;
            report.explicitAssets = ExplicitAssets.Select(spec =>
            {
                string guid = AssetDatabase.AssetPathToGUID(spec.path);
                return new AssetRecord { kind = spec.kind, path = spec.path, guid = guid, address = Address(spec.kind, guid) };
            }).ToArray();
            string[] templatePrefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { TemplateRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            Require(templatePrefabPaths.SequenceEqual(new[] { NpcPath, LocationPath }.OrderBy(path => path, StringComparer.Ordinal)),
                "Dragon Knight template folder must contain only the approved NpcTemplate and LocationTemplate prefabs.");
            string[] forbiddenDependencies = AssetDatabase.GetDependencies(new[] { NpcPath, LocationPath }, true)
                .Where(path => path.StartsWith("Assets/AvalonAwakenedCreatureCI4Controlled", StringComparison.Ordinal) ||
                               path.StartsWith("Assets/AvalonAwakenedCreatureCI4Offline", StringComparison.Ordinal) ||
                               path.StartsWith("Assets/AvalonAwakenedBullCI4", StringComparison.Ordinal) ||
                               path.StartsWith("Assets/AvalonAwakenedDragonCI4", StringComparison.Ordinal) ||
                               path.Contains("Fantasy_Elephant", StringComparison.OrdinalIgnoreCase) ||
                               path.Contains("BullFantasy", StringComparison.OrdinalIgnoreCase) ||
                               path.Contains("FantasyDragon", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            Require(forbiddenDependencies.Length == 0,
                "Dragon Knight templates retained an unrelated creature/control-pack dependency: " + string.Join(", ", forbiddenDependencies));
        }

        private static void ValidateNpc(NpcTemplate npc, Report report)
        {
            Require(npc != null, "Dragon Knight NpcTemplate is missing.");
            SerializedObject serialized = new(npc);
            report.level = GetInt(serialized, "level");
            report.maxHealth = GetInt(serialized, "maxHealth");
            report.maxStamina = GetInt(serialized, "maxStamina");
            report.staminaRegenPerTick = GetFloat(serialized, "staminaRegenPerTick");
            report.meleeDamage = GetFloat(serialized, "meleeDamage");
            report.rangedDamage = GetFloat(serialized, "rangedDamage");
            report.magicDamage = GetFloat(serialized, "magicDamage");
            report.npcWeight = GetInt(serialized, "npcWeight");
            report.poiseThreshold = GetFloat(serialized, "poiseThreshold");
            report.expLevel = GetInt(serialized, "expLevel");
            report.expTier = GetEnumOrInt(serialized, "expTier");
            report.expReward = GetInt(serialized, "expReward");
            report.npcType = GetEnumOrInt(serialized, "npcType");
            report.deadBodyLootable = GetBool(serialized, "isDeadBodyLootable");
            report.exactApprovedProfileValid =
                GetString(serialized, "metadata.notes") == Marker &&
                GetString(serialized, "faction._guid") == ApprovedFactionGuid &&
                ApprovedAbstractTypes.Select((guid, index) => GetString(serialized, "_abstractTypes.Array.data[" + index + "]._guid") == guid).All(item => item) &&
                GetArraySize(serialized, "_abstractTypes") == ApprovedAbstractTypes.Length &&
                report.level == 40 && report.maxHealth == 10000 && report.maxStamina == 250 &&
                Approx(report.staminaRegenPerTick, 5f) && Approx(report.meleeDamage, 113f) &&
                Approx(report.rangedDamage, 50f) && Approx(report.magicDamage, 60f) &&
                report.npcWeight == 300 && Approx(report.poiseThreshold, 1000f) &&
                report.expLevel == 8 && report.expTier == 0 && report.expReward == 640 &&
                report.npcType == 2 &&
                report.deadBodyLootable &&
                GetString(serialized, "fightingStyle._guid") == ApprovedFightingStyleGuid;
            Require(report.exactApprovedProfileValid,
                "Dragon Knight NpcTemplate does not match the explicitly approved Foredweller-derived proof profile.");
        }

        private static void ValidateLocation(GameObject root, string npcGuid, Report report)
        {
            Component[] components = root.GetComponents<Component>();
            report.locationComponentCount = components.Length;
            report.locationComponentTypes = components.Select(item => item.GetType().FullName ?? item.GetType().Name).ToArray();
            report.locationComponentShortNames = components.Select(item => item.GetType().Name).ToArray();
            report.exactNativeForedwellerLocationOrderPreserved =
                report.locationComponentShortNames.SequenceEqual(ExpectedLocationComponentShortNames);
            Require(report.exactNativeForedwellerLocationOrderPreserved && report.locationComponentCount == ExpectedLocationComponentShortNames.Length,
                "Dragon Knight location changed the approved Foredweller humanoid attachment composition.");
            LocationSpec spec = root.GetComponent<LocationSpec>();
            LocationTemplate template = root.GetComponent<LocationTemplate>();
            RepetitiveNpcAttachment attachment = root.GetComponent<RepetitiveNpcAttachment>();
            Component customCombat = components.FirstOrDefault(component => component != null && component.GetType().Name == "CustomCombatAttachment");
            Require(spec != null && template != null && attachment != null, "Dragon Knight location lost a core native component.");
            Require(customCombat != null, "Dragon Knight location lost CustomCombatAttachment.");
            report.nonUniqueNpcAttachment = !attachment.IsUnique;
            SerializedObject specSerialized = new(spec);
            SerializedObject templateSerialized = new(template);
            SerializedObject attachmentSerialized = new(attachment);
            SerializedObject customCombatSerialized = new(customCombat);
            Require(GetString(specSerialized, "displayName.IdOverride") == LocationDisplayNameId &&
                    GetString(specSerialized, "displayName.ID") == "Template/displayName_dragon_knight_dk4a" &&
                    GetString(specSerialized, "prefabReference.address") == NativeBootstrapShellAddress &&
                    GetString(specSerialized, "prefabReference.subObjectName") == string.Empty &&
                    GetBool(specSerialized, "snapToGround"),
                "Dragon Knight LocationSpec display-name/shell/snap contract changed.");
            Require(GetString(templateSerialized, "metadata.notes") == Marker, "Dragon Knight LocationTemplate marker changed.");
            Require(report.nonUniqueNpcAttachment &&
                    GetString(attachmentSerialized, "npcTemplate._guid") == npcGuid &&
                    GetString(attachmentSerialized, "visualPrefab.address") == DragonKnightNativeVisualAddress &&
                    GetString(attachmentSerialized, "visualPrefab.subObjectName") == string.Empty &&
                    GetString(attachmentSerialized, "simplifiedDeadBodyPrefab.arReference.address") == SimplifiedDeadBodyPrefabAddress &&
                    GetString(attachmentSerialized, "hitVFXReference.arReference.address") == HitVfxAddress &&
                    GetArraySize(attachmentSerialized, "potentialActors") == 0,
                "Dragon Knight RepetitiveNpcAttachment references changed.");
            Require(GetBool(customCombatSerialized, "weaponsAlwaysEquipped"),
                "Dragon Knight CustomCombatAttachment weaponsAlwaysEquipped changed.");
        }

        private static SettingsRestore ConfigureAddressables(AddressableAssetSettings settings, Report report)
        {
            SettingsRestore restore = new()
            {
                buildRemoteCatalog = settings.BuildRemoteCatalog,
                remoteBuildVariable = settings.RemoteCatalogBuildPath.GetName(settings),
                remoteLoadVariable = settings.RemoteCatalogLoadPath.GetName(settings),
                overridePlayerVersion = settings.OverridePlayerVersion,
            };
            foreach (AddressableAssetGroup item in settings.groups.Where(item => item != null))
            {
                BundledAssetGroupSchema schema = item.GetSchema<BundledAssetGroupSchema>();
                if (schema == null) continue;
                restore.includeStates[schema] = schema.IncludeInBuild;
                restore.buildVariables[schema] = schema.BuildPath.GetName(settings);
                restore.loadVariables[schema] = schema.LoadPath.GetName(settings);
                schema.IncludeInBuild = false;
            }
            RemoveGroup(settings, GroupName);
            AddressableAssetGroup group = settings.CreateGroup(GroupName, false, false, false, null,
                typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
            BundledAssetGroupSchema targetSchema = group.GetSchema<BundledAssetGroupSchema>();
            BundledAssetGroupSchema defaultSchema = settings.DefaultGroup?.GetSchema<BundledAssetGroupSchema>();
            Require(targetSchema != null && defaultSchema != null && targetSchema != defaultSchema,
                "Dragon Knight template proof requires distinct target and default schemas.");
            string profile = settings.activeProfileId;
            SetProfile(settings, profile, BuildVariable, Normalize(BuildRoot()));
            SetProfile(settings, profile, LoadVariable, RuntimeLoadRoot());
            targetSchema.IncludeInBuild = true;
            foreach (BundledAssetGroupSchema schema in new[] { targetSchema, defaultSchema })
            {
                schema.BuildPath.SetVariableByName(settings, BuildVariable);
                schema.LoadPath.SetVariableByName(settings, LoadVariable);
            }
            targetSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            targetSchema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
            targetSchema.IncludeAddressInCatalog = true;
            targetSchema.IncludeGUIDInCatalog = true;
            targetSchema.IncludeLabelsInCatalog = true;
            ContentUpdateGroupSchema update = group.GetSchema<ContentUpdateGroupSchema>();
            if (update != null) update.StaticContent = false;
            foreach (AssetRecord asset in report.explicitAssets)
            {
                AddressableAssetEntry entry = settings.CreateOrMoveEntry(asset.guid, group, false, false);
                entry.address = asset.address;
                entry.SetLabel(ProofLabel, true, true, false);
                entry.SetLabel(TemplateLabel, true, true, false);
            }
            Require(group.entries.Count == ExplicitAssets.Length, "Dragon Knight template proof group must contain exactly two explicit roots.");
            settings.BuildRemoteCatalog = true;
            settings.RemoteCatalogBuildPath.SetVariableByName(settings, BuildVariable);
            settings.RemoteCatalogLoadPath.SetVariableByName(settings, LoadVariable);
            settings.OverridePlayerVersion = "dragon-knight-dk4a-template-v1";
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return restore;
        }

        private static void RemoveGroup(AddressableAssetSettings settings, string name)
        {
            AddressableAssetGroup group = settings.FindGroup(name);
            if (group != null) settings.RemoveGroup(group);
        }

        private static void RestoreAddressables(AddressableAssetSettings settings, SettingsRestore restore)
        {
            foreach (KeyValuePair<BundledAssetGroupSchema, bool> pair in restore.includeStates)
            {
                if (pair.Key == null) continue;
                pair.Key.IncludeInBuild = pair.Value;
                pair.Key.BuildPath.SetVariableByName(settings, restore.buildVariables[pair.Key]);
                pair.Key.LoadPath.SetVariableByName(settings, restore.loadVariables[pair.Key]);
            }
            settings.BuildRemoteCatalog = restore.buildRemoteCatalog;
            settings.RemoteCatalogBuildPath.SetVariableByName(settings, restore.remoteBuildVariable);
            settings.RemoteCatalogLoadPath.SetVariableByName(settings, restore.remoteLoadVariable);
            settings.OverridePlayerVersion = restore.overridePlayerVersion;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        private static LoadCycle RunLoadCycle(string cataloguePath, Report report, int cycle)
        {
            AsyncOperationHandle<IResourceLocator> catalogueHandle = default;
            IResourceLocator locator = null;
            List<string> fingerprints = new();
            try
            {
                catalogueHandle = Addressables.LoadContentCatalogAsync(CatalogueUrl(cataloguePath), false);
                locator = catalogueHandle.WaitForCompletion();
                Require(catalogueHandle.Status == AsyncOperationStatus.Succeeded && locator != null,
                    "Dragon Knight template catalogue failed to load in cycle " + cycle + ".");
                Require(locator.Locate(ProofLabel, null, out IList<IResourceLocation> proof) && proof.Count == ExplicitAssets.Length,
                    "Dragon Knight template proof label must resolve exactly two roots.");
                report.templateLabelsValid = locator.Locate(TemplateLabel, typeof(GameObject), out IList<IResourceLocation> templates) &&
                                             templates.Count == ExplicitAssets.Length;
                bool hasTemplateSo = locator.Locate(TemplateSoLabel, null, out IList<IResourceLocation> templateSos);
                report.templateSoAbsentValid = !hasTemplateSo || templateSos == null || templateSos.Count == 0;
                Require(report.templateLabelsValid && report.templateSoAbsentValid,
                    "Dragon Knight template label closure is invalid or an unauthorized templateSO root is present.");

                fingerprints.Add(LoadValidateReleaseTemplate(locator, report.explicitAssets[0], report, true));
                fingerprints.Add(LoadValidateReleaseTemplate(locator, report.explicitAssets[1], report, false));
                return new LoadCycle
                {
                    cycle = cycle,
                    loadedAssetCount = fingerprints.Count,
                    allHandlesReleased = true,
                    fingerprint = HashText(string.Join("|", fingerprints)),
                };
            }
            finally
            {
                if (locator != null) Addressables.RemoveResourceLocator(locator);
                if (catalogueHandle.IsValid()) Addressables.Release(catalogueHandle);
            }
        }

        private static string LoadValidateReleaseTemplate(IResourceLocator locator, AssetRecord asset, Report report, bool npcTemplate)
        {
            Require(locator.Locate(asset.address, typeof(GameObject), out IList<IResourceLocation> locations) && locations.Count == 1,
                "Dragon Knight template address did not resolve exactly once: " + asset.address);
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(locations[0]);
            try
            {
                GameObject root = handle.WaitForCompletion();
                Require(handle.Status == AsyncOperationStatus.Succeeded && root != null,
                    "Dragon Knight template failed to load: " + asset.address);
                Report temp = NewReport();
                if (npcTemplate)
                {
                    ValidateNpc(root.GetComponent<NpcTemplate>(), temp);
                    return "npc-template:" + root.name + ":" + temp.level + ":" + temp.maxHealth + ":" +
                           temp.maxStamina + ":" + temp.meleeDamage + ":" + temp.expReward + ":" +
                           temp.exactApprovedProfileValid;
                }
                ValidateLocation(root, report.explicitAssets[0].guid, temp);
                return "location-template:" + root.name + ":" + temp.locationComponentCount + ":" +
                       temp.exactNativeForedwellerLocationOrderPreserved + ":" + temp.nonUniqueNpcAttachment + ":" +
                       ApprovedDisplayName + ":" + LocationDisplayNameId + ":" + DragonKnightNativeVisualAddress;
            }
            finally
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
        }

        private static SourceHash[] SnapshotSources()
        {
            string[] paths =
            {
                ForedwellerNpcPath,
                ForedwellerNpcPath + ".meta",
                ForedwellerAbstractNpcPath,
                ForedwellerAbstractNpcPath + ".meta",
                ForedwellerLocationPath,
                ForedwellerLocationPath + ".meta",
            };
            return paths.Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .Select(path => new SourceHash { path = path, beforeSha256 = HashFile(ProjectPath(path)) })
                .ToArray();
        }

        private static void CompleteSourceHashes(IEnumerable<SourceHash> hashes)
        {
            foreach (SourceHash item in hashes)
            {
                item.afterSha256 = HashFile(ProjectPath(item.path));
                item.unchanged = item.beforeSha256 == item.afterSha256;
            }
        }

        private static void VerifySourceHashes(IEnumerable<SourceHash> hashes)
        {
            SourceHash[] values = hashes?.ToArray() ?? Array.Empty<SourceHash>();
            Require(values.Length == 6, "Dragon Knight template source-integrity record is incomplete.");
            foreach (SourceHash item in values)
            {
                Require(item.unchanged && item.beforeSha256 == item.afterSha256 &&
                        HashFile(ProjectPath(item.path)) == item.afterSha256,
                    "Guarded Dragon Knight/Foredweller template source changed after build: " + item.path);
            }
        }

        private static Artifact[] SnapshotArtifacts()
        {
            string root = ModOutputRoot();
            Require(Directory.Exists(root), "Dragon Knight template output root is missing: " + root);
            return Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .Select(path => new Artifact
                {
                    relativePath = Normalize(Path.GetRelativePath(root, path)),
                    bytes = new FileInfo(path).Length,
                    sha256 = HashFile(path),
                }).ToArray();
        }

        private static string FindCatalogue()
        {
            Require(Directory.Exists(BuildRoot()), "Dragon Knight template build root is missing: " + BuildRoot());
            string[] catalogues = Directory.GetFiles(BuildRoot(), "*.json", SearchOption.TopDirectoryOnly);
            Require(catalogues.Length == 1, "Expected exactly one Dragon Knight template catalogue; found " + catalogues.Length + ".");
            return catalogues[0];
        }

        private static string Address(string kind, string guid) =>
            "dragon-knight/boss/dk4a/" + kind + "--" + guid;

        private static string ModOutputRoot() => Path.GetFullPath(Path.Combine(OutputRoot, ModFolderName));
        private static string BuildRoot() => Path.GetFullPath(Path.Combine(ModOutputRoot(), EditorUserBuildSettings.activeBuildTarget.ToString()));
        private static string RuntimeLoadRoot() => "{Awaken.TG.Assets.Modding.ModService.ModDirectoryPath}/" + ModFolderName + "/[BuildTarget]";
        private static string BuildReportPath() => ProjectPath("DragonKnightDK4ATemplateOutput/dragon-knight-dk4a-template-build.json");
        private static string FinalReportPath() => ProjectPath("DragonKnightDK4ATemplateOutput/dragon-knight-dk4a-template-finalized.json");
        private static string ProjectPath(string path) => Path.GetFullPath(Path.Combine(Application.dataPath, "..", path.Replace('/', Path.DirectorySeparatorChar)));
        private static string CatalogueUrl(string path) => "file:///" + Normalize(path);
        private static string Normalize(string path) => path.Replace('\\', '/');

        private static string MapProofInternalId(string internalId, string runtimeRoot, string buildRoot)
        {
            string value = Normalize(internalId);
            string runtime = Normalize(runtimeRoot).TrimEnd('/');
            return value.StartsWith(runtime, StringComparison.OrdinalIgnoreCase)
                ? Normalize(buildRoot).TrimEnd('/') + value.Substring(runtime.Length)
                : internalId;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int split = path.LastIndexOf('/');
            Require(split > 0, "Invalid asset folder: " + path);
            string parent = path.Substring(0, split);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(split + 1));
        }

        private static void SetProfile(AddressableAssetSettings settings, string profile, string name, string value)
        {
            settings.profileSettings.CreateValue(name, value);
            settings.profileSettings.SetValue(profile, name, value);
        }

        private static string[] ComponentShortNames(GameObject root) =>
            root.GetComponents<Component>().Where(component => component != null)
                .Select(component => component.GetType().Name).ToArray();

        private static void SetString(SerializedObject serialized, string path, string value)
        {
            SerializedProperty property = serialized.FindProperty(path);
            Require(property != null && property.propertyType == SerializedPropertyType.String,
                "String property is missing: " + path);
            property.stringValue = value;
        }

        private static string GetString(SerializedObject serialized, string path)
        {
            SerializedProperty property = serialized.FindProperty(path);
            Require(property != null && property.propertyType == SerializedPropertyType.String,
                "String property is missing: " + path);
            return property.stringValue;
        }

        private static int GetInt(SerializedObject serialized, string path)
        {
            SerializedProperty property = serialized.FindProperty(path);
            Require(property != null && property.propertyType == SerializedPropertyType.Integer,
                "Integer property is missing: " + path);
            return property.intValue;
        }

        private static int GetEnumOrInt(SerializedObject serialized, string path)
        {
            SerializedProperty property = serialized.FindProperty(path);
            Require(property != null && (property.propertyType == SerializedPropertyType.Enum ||
                                         property.propertyType == SerializedPropertyType.Integer),
                "Enum/integer property is missing: " + path);
            return property.propertyType == SerializedPropertyType.Enum ? property.enumValueIndex : property.intValue;
        }

        private static float GetFloat(SerializedObject serialized, string path)
        {
            SerializedProperty property = serialized.FindProperty(path);
            Require(property != null && property.propertyType == SerializedPropertyType.Float,
                "Float property is missing: " + path);
            return property.floatValue;
        }

        private static bool GetBool(SerializedObject serialized, string path)
        {
            SerializedProperty property = serialized.FindProperty(path);
            Require(property != null && property.propertyType == SerializedPropertyType.Boolean,
                "Boolean property is missing: " + path);
            return property.boolValue;
        }

        private static int GetArraySize(SerializedObject serialized, string path)
        {
            SerializedProperty property = serialized.FindProperty(path);
            Require(property != null && property.isArray, "Array property is missing: " + path);
            return property.arraySize;
        }

        private static void SetArraySize(SerializedObject serialized, string path, int size)
        {
            SerializedProperty property = serialized.FindProperty(path);
            Require(property != null && property.isArray, "Array property is missing: " + path);
            property.arraySize = size;
        }

        private static void WriteReport(string path, Report report)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
        }

        private static void TryWriteReport(string path, Report report)
        {
            try { WriteReport(path, report); }
            catch (Exception exception) { Debug.LogException(exception); }
        }

        private static string HashFile(string path)
        {
            using SHA256 sha = SHA256.Create();
            using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }

        private static string HashText(string value)
        {
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value ?? string.Empty)))
                .Replace("-", string.Empty);
        }

        private static bool Approx(float left, float right) => Mathf.Abs(left - right) <= 0.0005f;

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
