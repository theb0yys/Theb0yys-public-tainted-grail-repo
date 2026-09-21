using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class DragonKnightRetargetRenderProofBuilder
{
    private const string PassMarker = "DRAGON_KNIGHT_RETARGET_RENDER_PROOF_PASS";
    private const string BlockedMarker = "DRAGON_KNIGHT_RETARGET_RENDER_PROOF_BLOCKED";
    private const string BundleName = "dragonknight_retarget_render_proof";
    private const string ProofRoot = "Assets/DragonKnightProof";
    private const string GeneratedRoot = ProofRoot + "/Generated";
    private const string ControllerPath = GeneratedRoot + "/DragonKnight_1H_PhaseProof.controller";
    private const string PreviewPrefabPath = GeneratedRoot + "/DragonKnight_Retarget_Render_Proof_Preview.prefab";

    private const string DragonKnightFbx = "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/MESHES/CHARACTER/SK_DragonKnight.fbx";
    private const string IronPhasePrefab = "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Iron_Weapon.prefab";
    private const string FirePhasePrefab = "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/CHARACTER/SK_Dragon_Knight_Fire_Weapon.prefab";
    private const string PhaseTwoOffhandSwordPrefab = "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/PREFABS/WEAPON/SM_DragonKnight_Sword_Iron.prefab";

    private static readonly MaterialRequirement[] MaterialRequirements =
    {
        new MaterialRequirement("IronArm", "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/MATERIALS/Character_Iron/MI_DragonKnight_Arm_Iron.mat", false),
        new MaterialRequirement("IronBody", "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/MATERIALS/Character_Iron/MI_DragonKnight_Body_Iron.mat", false),
        new MaterialRequirement("IronCape", "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/MATERIALS/Character_Iron/MI_DragonKnight_Cape_Iron.mat", false),
        new MaterialRequirement("IronCloth", "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/MATERIALS/Character_Iron/MI_DragonKnight_Cloth_Iron.mat", false),
        new MaterialRequirement("IronLeg", "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/MATERIALS/Character_Iron/MI_DragonKnight_Leg_Iron.mat", false),
        new MaterialRequirement("IronSword", "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/MATERIALS/Weapon/MI_DragonKnight_Sword_Iron.mat", false),
        new MaterialRequirement("FireArm", "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/MATERIALS/Character_Fire/MI_DragonKnihgt_Arm_Fire.mat", true),
        new MaterialRequirement("FireBody", "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/MATERIALS/Character_Fire/MI_DragonKnight_Body_Fire.mat", true),
        new MaterialRequirement("FireCape", "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/MATERIALS/Character_Fire/MI_DragonKnight_Cape_Fire.mat", true),
        new MaterialRequirement("FireCloth", "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/MATERIALS/Character_Fire/MI_DragonKnight_Cloth_Fire.mat", true),
        new MaterialRequirement("FireLeg", "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/MATERIALS/Character_Fire/MI_DragonKnight_Leg_Fire.mat", true),
        new MaterialRequirement("FireSword", "Assets/NAKED_SINGULARITY/DRAGON_KNIGHT/MATERIALS/Weapon/MI_DragonKnight_Sword_Fire.mat", true),
    };

    private static readonly string[] RequiredBossAnimationRelativePaths =
    {
        "MCB/Normal/In Place/AS_CB_Idle.fbx",
        "MCB/Normal/In Place/AS_CB_Walk.fbx",
        "MCB/Normal/In Place/AS_CB_Run.fbx",
        "MCB/Normal/In Place/AS_CB_Block.fbx",
        "MCB/Normal/In Place/AS_CB_Attack.fbx",
        "MCB/Normal/In Place/AS_CB_Attack_V2.fbx",
        "MCB/Normal/In Place/AS_CB_Attack_V3.fbx",
        "MCB/Normal/In Place/AS_CB_Attack_Down.fbx",
        "MCB/Normal/In Place/AS_CB_Attack_Double.fbx",
        "MCB/Normal/In Place/AS_CB_Combo.fbx",
        "MCB/Normal/In Place/AS_CB_Combo_V2.fbx",
        "MCB/Normal/In Place/AS_CB_Charge_Strike.fbx",
        "MCB/Normal/In Place/AS_CB_Jump_Attack.fbx",
        "MCB/Normal/In Place/AS_CB_Thrust.fbx",
        "MCB/Normal/In Place/AS_CB_Hit_React.fbx",
        "MCB/Normal/In Place/AS_CB_Death.fbx",
        "MCB/Normal/Root_Motion/AS_CB_Charge_Strike_RM.fbx",
        "MCB/Normal/Root_Motion/AS_CB_Combo_RM.fbx",
        "MCB/Normal/Root_Motion/AS_CB_Jump_Attack_RM.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Transform.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Idle.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Walk.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Run.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Attack.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Attack_Charge.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Circular_Attack.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Combo.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Combo_V2.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Combo_V3.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Cut_Slice.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Dash_Slice.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Double_Strike.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Jump_Attack.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Power_Smash.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Rotate_Attack.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Smash_Down.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Thrust.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Hit_React.fbx",
        "MCB/Transform/TIn Place/AS_CB_T_Death.fbx",
        "MCB/Transform/TRoot_Motion/AS_CB_T_Dash_Slice_RM.fbx",
        "MCB/Transform/TRoot_Motion/AS_CB_T_Rotate_Attack_RM.fbx",
        "MCB/Transform/TRoot_Motion/AS_CB_T_Jump_Attack_RM.fbx",
    };

    private static readonly ControllerState[] ControllerStates =
    {
        new ControllerState("Iron_Idle_OneHand", "MCB/Normal/In Place/AS_CB_Idle.fbx", new Vector3(250, 0, 0)),
        new ControllerState("Iron_Walk_OneHand", "MCB/Normal/In Place/AS_CB_Walk.fbx", new Vector3(250, 70, 0)),
        new ControllerState("Iron_Run_OneHand", "MCB/Normal/In Place/AS_CB_Run.fbx", new Vector3(250, 140, 0)),
        new ControllerState("Iron_Block_OneHand", "MCB/Normal/In Place/AS_CB_Block.fbx", new Vector3(250, 210, 0)),
        new ControllerState("Iron_Attack_01", "MCB/Normal/In Place/AS_CB_Attack.fbx", new Vector3(560, 0, 0)),
        new ControllerState("Iron_Attack_02", "MCB/Normal/In Place/AS_CB_Attack_V2.fbx", new Vector3(560, 70, 0)),
        new ControllerState("Iron_Attack_03", "MCB/Normal/In Place/AS_CB_Attack_V3.fbx", new Vector3(560, 140, 0)),
        new ControllerState("Iron_Attack_Down", "MCB/Normal/In Place/AS_CB_Attack_Down.fbx", new Vector3(560, 210, 0)),
        new ControllerState("Iron_Attack_Double", "MCB/Normal/In Place/AS_CB_Attack_Double.fbx", new Vector3(560, 280, 0)),
        new ControllerState("Iron_Combo", "MCB/Normal/In Place/AS_CB_Combo.fbx", new Vector3(560, 350, 0)),
        new ControllerState("Iron_Combo_V2", "MCB/Normal/In Place/AS_CB_Combo_V2.fbx", new Vector3(560, 420, 0)),
        new ControllerState("Iron_Charge_Strike", "MCB/Normal/In Place/AS_CB_Charge_Strike.fbx", new Vector3(560, 490, 0)),
        new ControllerState("Iron_Jump_Attack", "MCB/Normal/In Place/AS_CB_Jump_Attack.fbx", new Vector3(560, 560, 0)),
        new ControllerState("Iron_Thrust", "MCB/Normal/In Place/AS_CB_Thrust.fbx", new Vector3(560, 630, 0)),
        new ControllerState("Iron_Hit_React", "MCB/Normal/In Place/AS_CB_Hit_React.fbx", new Vector3(870, 0, 0)),
        new ControllerState("Iron_Death", "MCB/Normal/In Place/AS_CB_Death.fbx", new Vector3(870, 70, 0)),
        new ControllerState("Phase_Transform", "MCB/Transform/TIn Place/AS_CB_T_Transform.fbx", new Vector3(1180, 0, 0)),
        new ControllerState("Fire_Idle_OneHand", "MCB/Transform/TIn Place/AS_CB_T_Idle.fbx", new Vector3(1490, 0, 0)),
        new ControllerState("Fire_Walk_OneHand", "MCB/Transform/TIn Place/AS_CB_T_Walk.fbx", new Vector3(1490, 70, 0)),
        new ControllerState("Fire_Run_OneHand", "MCB/Transform/TIn Place/AS_CB_T_Run.fbx", new Vector3(1490, 140, 0)),
        new ControllerState("Fire_Attack", "MCB/Transform/TIn Place/AS_CB_T_Attack.fbx", new Vector3(1800, 0, 0)),
        new ControllerState("Fire_Attack_Charge", "MCB/Transform/TIn Place/AS_CB_T_Attack_Charge.fbx", new Vector3(1800, 70, 0)),
        new ControllerState("Fire_Circular_Attack", "MCB/Transform/TIn Place/AS_CB_T_Circular_Attack.fbx", new Vector3(1800, 140, 0)),
        new ControllerState("Fire_Combo", "MCB/Transform/TIn Place/AS_CB_T_Combo.fbx", new Vector3(1800, 210, 0)),
        new ControllerState("Fire_Combo_V2", "MCB/Transform/TIn Place/AS_CB_T_Combo_V2.fbx", new Vector3(1800, 280, 0)),
        new ControllerState("Fire_Combo_V3", "MCB/Transform/TIn Place/AS_CB_T_Combo_V3.fbx", new Vector3(1800, 350, 0)),
        new ControllerState("Fire_Cut_Slice", "MCB/Transform/TIn Place/AS_CB_T_Cut_Slice.fbx", new Vector3(1800, 420, 0)),
        new ControllerState("Fire_Dash_Slice", "MCB/Transform/TIn Place/AS_CB_T_Dash_Slice.fbx", new Vector3(1800, 490, 0)),
        new ControllerState("Fire_Double_Strike", "MCB/Transform/TIn Place/AS_CB_T_Double_Strike.fbx", new Vector3(1800, 560, 0)),
        new ControllerState("Fire_Jump_Attack", "MCB/Transform/TIn Place/AS_CB_T_Jump_Attack.fbx", new Vector3(1800, 630, 0)),
        new ControllerState("Fire_Power_Smash", "MCB/Transform/TIn Place/AS_CB_T_Power_Smash.fbx", new Vector3(2110, 0, 0)),
        new ControllerState("Fire_Rotate_Attack", "MCB/Transform/TIn Place/AS_CB_T_Rotate_Attack.fbx", new Vector3(2110, 70, 0)),
        new ControllerState("Fire_Smash_Down", "MCB/Transform/TIn Place/AS_CB_T_Smash_Down.fbx", new Vector3(2110, 140, 0)),
        new ControllerState("Fire_Thrust", "MCB/Transform/TIn Place/AS_CB_T_Thrust.fbx", new Vector3(2110, 210, 0)),
        new ControllerState("Fire_Hit_React", "MCB/Transform/TIn Place/AS_CB_T_Hit_React.fbx", new Vector3(2420, 0, 0)),
        new ControllerState("Fire_Death", "MCB/Transform/TIn Place/AS_CB_T_Death.fbx", new Vector3(2420, 70, 0)),
        new ControllerState("RM_Iron_Charge_Strike", "MCB/Normal/Root_Motion/AS_CB_Charge_Strike_RM.fbx", new Vector3(2730, 0, 0)),
        new ControllerState("RM_Iron_Combo", "MCB/Normal/Root_Motion/AS_CB_Combo_RM.fbx", new Vector3(2730, 70, 0)),
        new ControllerState("RM_Iron_Jump_Attack", "MCB/Normal/Root_Motion/AS_CB_Jump_Attack_RM.fbx", new Vector3(2730, 140, 0)),
        new ControllerState("RM_Fire_Dash_Slice", "MCB/Transform/TRoot_Motion/AS_CB_T_Dash_Slice_RM.fbx", new Vector3(3040, 0, 0)),
        new ControllerState("RM_Fire_Rotate_Attack", "MCB/Transform/TRoot_Motion/AS_CB_T_Rotate_Attack_RM.fbx", new Vector3(3040, 70, 0)),
        new ControllerState("RM_Fire_Jump_Attack", "MCB/Transform/TRoot_Motion/AS_CB_T_Jump_Attack_RM.fbx", new Vector3(3040, 140, 0)),
    };

    private static readonly ExternalDirectory[] VfxCandidates =
    {
        new ExternalDirectory(
            "weapon_fire_trail",
            "INab Studio/Vfx Assets/Weapon FX Series/Weapon Trails FX",
            "Assets/DragonKnightProof/VFX/INabWeaponTrails",
            "Assets/DragonKnightProof/VFX/INabWeaponTrails/Trail Prefabs/Fire 1.prefab"),
    };

    public static void BuildProof()
    {
        string manifestPath = GetArgument("-dragonKnightProofManifest", Path.Combine(ProjectRoot, "dragon-knight-retarget-render-proof-manifest.json"));
        try
        {
            string outputRoot = GetArgument("-dragonKnightProofOutput", Path.Combine(ProjectRoot, "DragonKnightProofBundles"));
            string externalAssetsRoot = GetArgument("-dragonKnightExternalAssetsRoot", string.Empty);
            string bossMoveSetRoot = GetArgument("-dragonKnightBossMoveSetRoot", string.Empty);

            if (string.IsNullOrWhiteSpace(externalAssetsRoot))
            {
                throw new InvalidOperationException("Missing -dragonKnightExternalAssetsRoot.");
            }

            if (string.IsNullOrWhiteSpace(bossMoveSetRoot))
            {
                throw new InvalidOperationException("Missing -dragonKnightBossMoveSetRoot.");
            }

            var report = new ProofReport
            {
                marker = PassMarker,
                unityVersion = Application.unityVersion,
                projectRoot = ProjectRoot,
                externalAssetsRoot = externalAssetsRoot,
                bossMoveSetRoot = bossMoveSetRoot,
                runtimeAiTouched = false,
                gameDeploymentTouched = false,
                savePathTouched = false,
            };

            ResetProofRoot();
            EnsureFolder(GeneratedRoot);

            Avatar dragonKnightAvatar = ValidateDragonKnightSource(report);

            var copiedAnimationPaths = CopyBossMoveSetAnimations(bossMoveSetRoot, report);
            CopyVfxCandidates(externalAssetsRoot, report);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            ConfigureAnimationImporters(copiedAnimationPaths, dragonKnightAvatar, report);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var clips = ResolveAnimationClips(copiedAnimationPaths, report);
            ValidateVfxCandidates(report);
            var controller = BuildController(clips);
            report.generatedAssets.Add(ControllerPath);

            BuildPreviewPrefab(controller, report);
            report.generatedAssets.Add(PreviewPrefabPath);

            string bundlePath = BuildBundle(outputRoot, copiedAnimationPaths, report);
            report.bundlePath = bundlePath;
            report.bundleBytes = new FileInfo(bundlePath).Length;
            report.bundleSha256 = Sha256(bundlePath);

            WriteReport(manifestPath, report);
            Debug.Log(PassMarker + ": " + manifestPath);
        }
        catch (Exception ex)
        {
            var blocked = new ProofReport
            {
                marker = BlockedMarker,
                unityVersion = Application.unityVersion,
                projectRoot = ProjectRoot,
                runtimeAiTouched = false,
                gameDeploymentTouched = false,
                savePathTouched = false,
                errors = new List<string> { ex.ToString() },
            };
            WriteReport(manifestPath, blocked);
            Debug.LogError(BlockedMarker + ": " + ex);
            throw;
        }
    }

    private static Avatar ValidateDragonKnightSource(ProofReport report)
    {
        RequireAsset<GameObject>(IronPhasePrefab, "Missing Iron character-with-weapon prefab.");
        RequireAsset<GameObject>(FirePhasePrefab, "Missing Fire character-with-weapon prefab.");
        RequireAsset<GameObject>(PhaseTwoOffhandSwordPrefab, "Missing phase-two offhand Dragon Knight sword prefab.");
        report.selectedDragonKnightAssets.Add(IronPhasePrefab);
        report.selectedDragonKnightAssets.Add(FirePhasePrefab);
        report.selectedDragonKnightAssets.Add(PhaseTwoOffhandSwordPrefab);

        var avatar = AssetDatabase.LoadAllAssetsAtPath(DragonKnightFbx).OfType<Avatar>().FirstOrDefault();
        report.dragonKnightAvatarHuman = avatar != null && avatar.isHuman;
        if (!report.dragonKnightAvatarHuman)
        {
            throw new InvalidOperationException("Dragon Knight source avatar is not a proven humanoid avatar: " + DragonKnightFbx);
        }

        var iron = AssetDatabase.LoadAssetAtPath<GameObject>(IronPhasePrefab);
        var fire = AssetDatabase.LoadAssetAtPath<GameObject>(FirePhasePrefab);
        report.ironSkinnedRendererCount = iron.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length;
        report.fireSkinnedRendererCount = fire.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length;
        report.ironAnimatorCount = iron.GetComponentsInChildren<Animator>(true).Length;
        report.fireAnimatorCount = fire.GetComponentsInChildren<Animator>(true).Length;

        if (report.ironSkinnedRendererCount == 0 || report.fireSkinnedRendererCount == 0)
        {
            throw new InvalidOperationException("Dragon Knight phase prefab renderer proof failed.");
        }

        foreach (var requirement in MaterialRequirements)
        {
            report.materials.Add(InspectMaterial(requirement));
        }

        var failed = report.materials.Where(x => !x.pass).Select(x => x.name + ": " + string.Join("; ", x.errors)).ToArray();
        if (failed.Length > 0)
        {
            throw new InvalidOperationException("Material QA failed: " + string.Join(" | ", failed));
        }

        return avatar;
    }

    private static List<string> CopyBossMoveSetAnimations(string bossMoveSetRoot, ProofReport report)
    {
        if (!Directory.Exists(bossMoveSetRoot))
        {
            throw new DirectoryNotFoundException("Missing boss move-set root: " + bossMoveSetRoot);
        }

        string mcbRoot = Path.Combine(bossMoveSetRoot, "MCB");
        if (!Directory.Exists(mcbRoot))
        {
            throw new DirectoryNotFoundException("Missing MCB animation root: " + mcbRoot);
        }

        var required = new HashSet<string>(
            RequiredBossAnimationRelativePaths.Select(NormalizeRelativePath),
            StringComparer.OrdinalIgnoreCase);

        foreach (string requiredRelativePath in required)
        {
            string requiredSource = Path.Combine(bossMoveSetRoot, requiredRelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(requiredSource))
            {
                throw new FileNotFoundException("Missing required Dragon Knight boss animation: " + requiredSource);
            }
        }

        var copied = new List<string>();
        string[] sources = Directory.GetFiles(mcbRoot, "*.fbx", SearchOption.AllDirectories)
            .OrderBy(x => NormalizeRelativePath(RelativeToRoot(bossMoveSetRoot, x)), StringComparer.OrdinalIgnoreCase)
            .ToArray();

        report.bossMoveSetFbxCount = sources.Length;
        report.requiredBossAnimationCount = required.Count;

        string greatswordPath = Path.Combine(bossMoveSetRoot, "SM_Greatsword.fbx");
        if (File.Exists(greatswordPath))
        {
            report.ignoredBossSourceAssets.Add("SM_Greatsword.fbx (ignored: Dragon Knight proof uses the Dragon Knight one-handed sword prefab)");
        }

        string mayaRoot = Path.Combine(mcbRoot, "Transform");
        if (Directory.Exists(mayaRoot) && Directory.GetFiles(mayaRoot, "*.ma", SearchOption.TopDirectoryOnly).Length > 0)
        {
            report.ignoredBossSourceAssets.Add("MCB/Transform/*.ma (Maya source retained outside proof; FBX clips are the Unity retarget candidates)");
        }

        foreach (string source in sources)
        {
            string relativePath = NormalizeRelativePath(RelativeToRoot(bossMoveSetRoot, source));
            string target = ProofRoot + "/Animations/BossMoveSets/" + relativePath;
            CopyFileWithMeta(source, AbsoluteProjectPath(target));
            copied.Add(target);
            bool isRequired = required.Contains(relativePath);
            if (isRequired)
            {
                report.requiredBossAnimationsPresent++;
            }

            report.animations.Add(new AnimationProof
            {
                id = IdForRelativePath(relativePath),
                relativePath = relativePath,
                sourcePath = source,
                importedPath = target,
                expectedClipName = Path.GetFileNameWithoutExtension(source),
                category = CategoryForRelativePath(relativePath),
                rootMotion = IsRootMotionPath(relativePath),
                required = isRequired,
            });
        }

        return copied;
    }

    private static void ConfigureAnimationImporters(List<string> copiedAnimationPaths, Avatar sourceAvatar, ProofReport report)
    {
        foreach (var proof in report.animations)
        {
            if (!copiedAnimationPaths.Contains(proof.importedPath))
            {
                continue;
            }

            var importer = AssetImporter.GetAtPath(proof.importedPath) as ModelImporter;
            if (importer == null)
            {
                throw new InvalidOperationException("Missing animation importer: " + proof.importedPath);
            }

            importer.importAnimation = true;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.sourceAvatar = null;
            importer.SaveAndReimport();

            proof.forcedHumanImporter = true;
            proof.avatarCopySource = string.Empty;
            proof.retargetMode = "humanoid-source-avatar-to-dragon-knight-controller";
        }
    }

    private static void CopyVfxCandidates(string externalAssetsRoot, ProofReport report)
    {
        foreach (var candidate in VfxCandidates)
        {
            string source = Path.Combine(externalAssetsRoot, candidate.relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(source))
            {
                throw new DirectoryNotFoundException("Missing VFX candidate directory: " + source);
            }

            string targetAbsolute = AbsoluteProjectPath(candidate.importRoot);
            if (Directory.Exists(targetAbsolute))
            {
                Directory.Delete(targetAbsolute, true);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(targetAbsolute));
            FileUtil.CopyFileOrDirectory(source, targetAbsolute);
            if (File.Exists(source + ".meta"))
            {
                File.Copy(source + ".meta", targetAbsolute + ".meta", true);
            }

            int strippedScriptCount = StripCodeFiles(targetAbsolute);

            report.vfx.Add(new VfxProof
            {
                id = candidate.id,
                sourcePath = source,
                importedRoot = candidate.importRoot,
                prefabPath = candidate.prefabPath,
                strippedScriptCount = strippedScriptCount,
            });
        }
    }

    private static Dictionary<string, AnimationClip> ResolveAnimationClips(List<string> copiedAnimationPaths, ProofReport report)
    {
        var byId = new Dictionary<string, AnimationClip>(StringComparer.OrdinalIgnoreCase);

        foreach (var proof in report.animations)
        {
            var importer = AssetImporter.GetAtPath(proof.importedPath) as ModelImporter;
            proof.humanoidImporter = importer != null && importer.animationType == ModelImporterAnimationType.Human;

            var importedAssets = AssetDatabase.LoadAllAssetsAtPath(proof.importedPath);
            var clips = importedAssets
                .OfType<AnimationClip>()
                .Where(x => x != null && !x.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            proof.importedAvatarHuman = importedAssets.OfType<Avatar>().Any(x => x != null && x.isHuman);

            proof.importedClipNames = clips.Select(x => x.name).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
            AnimationClip selected = clips.FirstOrDefault(x => string.Equals(x.name, proof.expectedClipName, StringComparison.OrdinalIgnoreCase))
                ?? clips.FirstOrDefault();

            proof.selectedClipName = selected != null ? selected.name : string.Empty;
            proof.pass = proof.humanoidImporter && proof.importedAvatarHuman && selected != null;

            if (!proof.pass)
            {
                throw new InvalidOperationException("Animation retarget source proof failed: " + proof.id + " at " + proof.importedPath);
            }

            byId[NormalizeRelativePath(proof.relativePath)] = selected;
        }

        return byId;
    }

    private static void ValidateVfxCandidates(ProofReport report)
    {
        foreach (var proof in report.vfx)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(proof.prefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException("Missing imported VFX prefab: " + proof.prefabPath);
            }

            proof.particleSystemCount = prefab.GetComponentsInChildren<ParticleSystem>(true).Length;
            proof.rendererCount = prefab.GetComponentsInChildren<Renderer>(true).Length;
            proof.dependencyCount = AssetDatabase.GetDependencies(proof.prefabPath, true).Length;
            proof.pass = proof.particleSystemCount > 0 || proof.rendererCount > 0;

            if (!proof.pass)
            {
                throw new InvalidOperationException("VFX proof has no renderable components: " + proof.prefabPath);
            }
        }
    }

    private static AnimatorController BuildController(Dictionary<string, AnimationClip> clips)
    {
        if (File.Exists(AbsoluteProjectPath(ControllerPath)))
        {
            AssetDatabase.DeleteAsset(ControllerPath);
        }

        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        var stateMachine = controller.layers[0].stateMachine;

        foreach (var state in ControllerStates)
        {
            AddState(stateMachine, state.name, GetClip(clips, state.relativePath), state.position);
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static AnimationClip GetClip(Dictionary<string, AnimationClip> clips, string relativePath)
    {
        string key = NormalizeRelativePath(relativePath);
        if (!clips.TryGetValue(key, out AnimationClip clip))
        {
            throw new InvalidOperationException("Missing controller proof clip: " + relativePath);
        }

        return clip;
    }

    private static void AddState(AnimatorStateMachine stateMachine, string name, Motion motion, Vector3 position)
    {
        var state = stateMachine.AddState(name, position);
        state.motion = motion;
        if (stateMachine.defaultState == null)
        {
            stateMachine.defaultState = state;
        }
    }

    private static void BuildPreviewPrefab(RuntimeAnimatorController controller, ProofReport report)
    {
        var root = new GameObject("DragonKnight_Retarget_Render_Proof_Preview");
        try
        {
            AddPreviewPhase(root.transform, "Iron_Phase_1H", IronPhasePrefab, controller, new Vector3(-1.5f, 0f, 0f));
            var firePhase = AddPreviewPhase(root.transform, "Fire_Phase_2_DualSword", FirePhasePrefab, controller, new Vector3(1.5f, 0f, 0f));
            AttachPhaseTwoOffhandSword(firePhase, report);

            var vfx = AssetDatabase.LoadAssetAtPath<GameObject>(VfxCandidates[0].prefabPath);
            if (vfx != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(vfx);
                instance.name = "Phase_VFX_Weapon_Fire_Trail";
                instance.transform.SetParent(root.transform, false);
                instance.transform.localPosition = new Vector3(0f, 1.2f, 1.2f);
            }

            PrefabUtility.SaveAsPrefabAsset(root, PreviewPrefabPath);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static GameObject AddPreviewPhase(Transform parent, string name, string prefabPath, RuntimeAnimatorController controller, Vector3 position)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = position;

        foreach (var animator in instance.GetComponentsInChildren<Animator>(true))
        {
            animator.runtimeAnimatorController = controller;
        }

        return instance;
    }

    private static void AttachPhaseTwoOffhandSword(GameObject phaseRoot, ProofReport report)
    {
        var animator = phaseRoot.GetComponentsInChildren<Animator>(true).FirstOrDefault(x => x.avatar != null && x.avatar.isHuman);
        if (animator == null)
        {
            throw new InvalidOperationException("Phase two dual-sword proof failed: Fire phase prefab has no humanoid animator.");
        }

        Transform leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        if (leftHand == null)
        {
            throw new InvalidOperationException("Phase two dual-sword proof failed: left hand bone is unavailable.");
        }

        var swordPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PhaseTwoOffhandSwordPrefab);
        if (swordPrefab == null)
        {
            throw new InvalidOperationException("Phase two dual-sword proof failed: offhand sword prefab missing.");
        }

        var sword = (GameObject)PrefabUtility.InstantiatePrefab(swordPrefab);
        sword.name = "Phase2_Offhand_IronSword";
        sword.transform.SetParent(leftHand, false);

        var renderers = sword.GetComponentsInChildren<Renderer>(true);
        var meshRenderers = sword.GetComponentsInChildren<MeshRenderer>(true);
        var skinnedRenderers = sword.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        if (renderers.Length == 0)
        {
            throw new InvalidOperationException("Phase two dual-sword proof failed: offhand sword has no renderers.");
        }

        report.phaseTwoDualSword = new DualSwordProof
        {
            enabled = true,
            mainhandSource = FirePhasePrefab,
            offhandSwordPrefab = PhaseTwoOffhandSwordPrefab,
            offhandObjectName = sword.name,
            leftHandPath = TransformPath(leftHand, phaseRoot.transform),
            rightHandPath = rightHand == null ? string.Empty : TransformPath(rightHand, phaseRoot.transform),
            rendererCount = renderers.Length,
            meshRendererCount = meshRenderers.Length,
            skinnedRendererCount = skinnedRenderers.Length,
            localPosition = FormatVector3(sword.transform.localPosition),
            localRotation = FormatQuaternion(sword.transform.localRotation),
            localScale = FormatVector3(sword.transform.localScale),
            pass = true,
        };
    }

    private static string BuildBundle(string outputRoot, List<string> copiedAnimationPaths, ProofReport report)
    {
        Directory.CreateDirectory(outputRoot);

        var assetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            IronPhasePrefab,
            FirePhasePrefab,
            PhaseTwoOffhandSwordPrefab,
            ControllerPath,
            PreviewPrefabPath,
        };

        foreach (var path in copiedAnimationPaths)
        {
            assetNames.Add(path);
        }

        foreach (var proof in report.vfx)
        {
            assetNames.Add(proof.prefabPath);
        }

        report.bundleAssets = assetNames.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();

        var build = new AssetBundleBuild
        {
            assetBundleName = BundleName,
            assetNames = report.bundleAssets.ToArray(),
        };

        var manifest = BuildPipeline.BuildAssetBundles(
            outputRoot,
            new[] { build },
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode,
            BuildTarget.StandaloneWindows64);

        if (manifest == null)
        {
            throw new InvalidOperationException("BuildPipeline.BuildAssetBundles returned null.");
        }

        string bundlePath = Path.Combine(outputRoot, BundleName);
        if (!File.Exists(bundlePath))
        {
            throw new FileNotFoundException("Expected proof bundle was not written.", bundlePath);
        }

        return bundlePath;
    }

    private static MaterialProof InspectMaterial(MaterialRequirement requirement)
    {
        var proof = new MaterialProof
        {
            name = requirement.name,
            materialPath = requirement.path,
            requiresEmissive = requirement.requiresEmissive,
        };

        var material = AssetDatabase.LoadAssetAtPath<Material>(requirement.path);
        if (material == null)
        {
            proof.errors.Add("missing material");
            return proof;
        }

        string materialText = File.ReadAllText(AbsoluteProjectPath(requirement.path));
        InspectTexture(materialText, "_Diffuse_Map", "diffuse", true, TextureImporterType.Default, true, proof);
        InspectTexture(materialText, "_Normal_Map", "normal", true, TextureImporterType.NormalMap, false, proof);
        InspectTexture(materialText, "_ORM_Map", "orm", true, TextureImporterType.Default, false, proof);

        if (requirement.requiresEmissive)
        {
            InspectTexture(materialText, "_Emissive_Map", "emissive", true, TextureImporterType.Default, true, proof);
        }
        else
        {
            InspectTexture(materialText, "_Emissive_Map", "emissive", false, TextureImporterType.Default, true, proof);
        }

        InspectTexture(materialText, "_Emissive_Mask", "emissiveMask", false, TextureImporterType.Default, false, proof);
        InspectTexture(materialText, "_Alpha_Map", "alpha", false, TextureImporterType.Default, true, proof);
        proof.pass = proof.errors.Count == 0;
        return proof;
    }

    private static void InspectTexture(string materialText, string property, string label, bool required, TextureImporterType expectedType, bool expectedSrgb, MaterialProof proof)
    {
        string propertyPattern = @"-\s+" + Regex.Escape(property) + @":";
        if (!Regex.IsMatch(materialText, propertyPattern))
        {
            if (required)
            {
                proof.errors.Add(label + " property missing");
            }
            return;
        }

        string texturePattern = @"-\s+" + Regex.Escape(property) + @":\s*\r?\n\s*m_Texture:\s*\{fileI<local-path>";
        Match textureMatch = Regex.Match(materialText, texturePattern);
        string fileId = textureMatch.Success ? textureMatch.Groups[1].Value.Trim() : string.Empty;
        string guid = textureMatch.Success ? textureMatch.Groups[2].Value.Trim() : string.Empty;
        if (!textureMatch.Success || string.IsNullOrWhiteSpace(guid) || fileId == "0")
        {
            if (required)
            {
                proof.errors.Add(label + " texture missing");
            }
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guid);
        var textureProof = new TextureProof
        {
            slot = label,
            path = string.IsNullOrWhiteSpace(path) ? "UNRESOLVED_GUID:" + guid : path,
        };

        if (string.IsNullOrWhiteSpace(path))
        {
            proof.textures.Add(textureProof);
            if (required)
            {
                proof.errors.Add(label + " texture guid unresolved: " + guid);
            }
            return;
        }

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            if (required)
            {
                proof.errors.Add(label + " importer missing");
            }
        }
        else
        {
            textureProof.textureType = importer.textureType.ToString();
            textureProof.srgb = importer.sRGBTexture;
            textureProof.pass = importer.textureType == expectedType && importer.sRGBTexture == expectedSrgb;
            if (!textureProof.pass && required)
            {
                proof.errors.Add(label + " import settings mismatch: " + path);
            }
        }

        proof.textures.Add(textureProof);
    }

    private static T RequireAsset<T>(string assetPath, string message) where T : UnityEngine.Object
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        if (asset == null)
        {
            throw new InvalidOperationException(message + " " + assetPath);
        }
        return asset;
    }

    private static void ResetProofRoot()
    {
        if (AssetDatabase.IsValidFolder(ProofRoot))
        {
            AssetDatabase.DeleteAsset(ProofRoot);
        }

        EnsureFolder(ProofRoot);
    }

    private static void EnsureFolder(string assetFolder)
    {
        string[] parts = assetFolder.Split('/');
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

    private static void CopyFileWithMeta(string source, string target)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(target));
        File.Copy(source, target, true);
        if (File.Exists(source + ".meta"))
        {
            File.Copy(source + ".meta", target + ".meta", true);
        }
    }

    private static int StripCodeFiles(string root)
    {
        int count = 0;
        foreach (string pattern in new[] { "*.cs", "*.asmdef" })
        {
            foreach (string path in Directory.GetFiles(root, pattern, SearchOption.AllDirectories))
            {
                File.Delete(path);
                if (File.Exists(path + ".meta"))
                {
                    File.Delete(path + ".meta");
                }
                count++;
            }
        }
        return count;
    }

    private static string AbsoluteProjectPath(string assetPath)
    {
        return Path.Combine(ProjectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static string RelativeToRoot(string root, string path)
    {
        string fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        string fullPath = Path.GetFullPath(path);
        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            return Path.GetFileName(path);
        }

        return fullPath.Substring(fullRoot.Length).Replace('\\', '/');
    }

    private static string NormalizeRelativePath(string value)
    {
        return value.Replace('\\', '/').TrimStart('/');
    }

    private static string TransformPath(Transform transform, Transform root)
    {
        var parts = new List<string>();
        Transform current = transform;
        while (current != null)
        {
            parts.Add(current.name);
            if (current == root)
            {
                break;
            }

            current = current.parent;
        }

        parts.Reverse();
        return string.Join("/", parts);
    }

    private static string FormatVector3(Vector3 value)
    {
        return value.x.ToString("0.######") + "|" + value.y.ToString("0.######") + "|" + value.z.ToString("0.######");
    }

    private static string FormatQuaternion(Quaternion value)
    {
        return value.x.ToString("0.######") + "|" + value.y.ToString("0.######") + "|" + value.z.ToString("0.######") + "|" + value.w.ToString("0.######");
    }

    private static string IdForRelativePath(string relativePath)
    {
        string withoutExtension = Path.ChangeExtension(NormalizeRelativePath(relativePath), null);
        return Regex.Replace(withoutExtension.ToLowerInvariant(), @"[^a-z0-9]+", "_").Trim('_');
    }

    private static string CategoryForRelativePath(string relativePath)
    {
        string normalized = NormalizeRelativePath(relativePath);
        if (normalized.StartsWith("MCB/Normal/In Place/", StringComparison.OrdinalIgnoreCase))
        {
            return "normal_in_place";
        }

        if (normalized.StartsWith("MCB/Normal/Root_Motion/", StringComparison.OrdinalIgnoreCase))
        {
            return "normal_root_motion";
        }

        if (normalized.StartsWith("MCB/Transform/TIn Place/", StringComparison.OrdinalIgnoreCase))
        {
            return "transform_in_place";
        }

        if (normalized.StartsWith("MCB/Transform/TRoot_Motion/", StringComparison.OrdinalIgnoreCase))
        {
            return "transform_root_motion";
        }

        return "unknown";
    }

    private static bool IsRootMotionPath(string relativePath)
    {
        return NormalizeRelativePath(relativePath).IndexOf("Root_Motion", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string ProjectRoot
    {
        get
        {
            return Directory.GetParent(Application.dataPath).FullName;
        }
    }

    private static string GetArgument(string name, string fallback)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }
        return fallback;
    }

    private static string Sha256(string path)
    {
        using (var stream = File.OpenRead(path))
        using (var sha = SHA256.Create())
        {
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }

    private static void WriteReport(string manifestPath, ProofReport report)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath));
        File.WriteAllText(manifestPath, ToJson(report), new UTF8Encoding(false));
    }

    private static string ToJson(ProofReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        Append(sb, "marker", report.marker, 1, true);
        Append(sb, "unityVersion", report.unityVersion, 1, true);
        Append(sb, "projectRoot", report.projectRoot, 1, true);
        Append(sb, "externalAssetsRoot", report.externalAssetsRoot, 1, true);
        Append(sb, "bossMoveSetRoot", report.bossMoveSetRoot, 1, true);
        Append(sb, "runtimeAiTouched", report.runtimeAiTouched, 1, true);
        Append(sb, "gameDeploymentTouched", report.gameDeploymentTouched, 1, true);
        Append(sb, "savePathTouched", report.savePathTouched, 1, true);
        Append(sb, "dragonKnightAvatarHuman", report.dragonKnightAvatarHuman, 1, true);
        Append(sb, "ironSkinnedRendererCount", report.ironSkinnedRendererCount, 1, true);
        Append(sb, "fireSkinnedRendererCount", report.fireSkinnedRendererCount, 1, true);
        Append(sb, "ironAnimatorCount", report.ironAnimatorCount, 1, true);
        Append(sb, "fireAnimatorCount", report.fireAnimatorCount, 1, true);
        Append(sb, "bossMoveSetFbxCount", report.bossMoveSetFbxCount, 1, true);
        Append(sb, "requiredBossAnimationCount", report.requiredBossAnimationCount, 1, true);
        Append(sb, "requiredBossAnimationsPresent", report.requiredBossAnimationsPresent, 1, true);
        Append(sb, "bundlePath", report.bundlePath, 1, true);
        Append(sb, "bundleBytes", report.bundleBytes, 1, true);
        Append(sb, "bundleSha256", report.bundleSha256, 1, true);
        AppendArray(sb, "selectedDragonKnightAssets", report.selectedDragonKnightAssets, 1, true);
        AppendArray(sb, "generatedAssets", report.generatedAssets, 1, true);
        AppendArray(sb, "bundleAssets", report.bundleAssets, 1, true);
        AppendArray(sb, "ignoredBossSourceAssets", report.ignoredBossSourceAssets, 1, true);
        AppendDualSword(sb, report.phaseTwoDualSword, 1, true);
        AppendMaterials(sb, report.materials, 1, true);
        AppendAnimations(sb, report.animations, 1, true);
        AppendVfx(sb, report.vfx, 1, true);
        AppendArray(sb, "errors", report.errors, 1, false);
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void Append(StringBuilder sb, string name, string value, int indent, bool comma)
    {
        sb.Append(Indent(indent)).Append('"').Append(name).Append("\": ").Append('"').Append(Esc(value)).Append('"');
        sb.AppendLine(comma ? "," : string.Empty);
    }

    private static void Append(StringBuilder sb, string name, bool value, int indent, bool comma)
    {
        sb.Append(Indent(indent)).Append('"').Append(name).Append("\": ").Append(value ? "true" : "false");
        sb.AppendLine(comma ? "," : string.Empty);
    }

    private static void Append(StringBuilder sb, string name, int value, int indent, bool comma)
    {
        sb.Append(Indent(indent)).Append('"').Append(name).Append("\": ").Append(value.ToString());
        sb.AppendLine(comma ? "," : string.Empty);
    }

    private static void Append(StringBuilder sb, string name, long value, int indent, bool comma)
    {
        sb.Append(Indent(indent)).Append('"').Append(name).Append("\": ").Append(value.ToString());
        sb.AppendLine(comma ? "," : string.Empty);
    }

    private static void AppendArray(StringBuilder sb, string name, IList<string> values, int indent, bool comma)
    {
        sb.Append(Indent(indent)).Append('"').Append(name).AppendLine("\": [");
        for (int i = 0; i < values.Count; i++)
        {
            sb.Append(Indent(indent + 1)).Append('"').Append(Esc(values[i])).Append('"');
            sb.AppendLine(i == values.Count - 1 ? string.Empty : ",");
        }
        sb.Append(Indent(indent)).Append(']');
        sb.AppendLine(comma ? "," : string.Empty);
    }

    private static void AppendMaterials(StringBuilder sb, IList<MaterialProof> values, int indent, bool comma)
    {
        sb.Append(Indent(indent)).AppendLine("\"materials\": [");
        for (int i = 0; i < values.Count; i++)
        {
            var value = values[i];
            sb.Append(Indent(indent + 1)).AppendLine("{");
            Append(sb, "name", value.name, indent + 2, true);
            Append(sb, "materialPath", value.materialPath, indent + 2, true);
            Append(sb, "requiresEmissive", value.requiresEmissive, indent + 2, true);
            Append(sb, "pass", value.pass, indent + 2, true);
            AppendTextures(sb, value.textures, indent + 2, true);
            AppendArray(sb, "errors", value.errors, indent + 2, false);
            sb.Append(Indent(indent + 1)).Append('}');
            sb.AppendLine(i == values.Count - 1 ? string.Empty : ",");
        }
        sb.Append(Indent(indent)).Append(']');
        sb.AppendLine(comma ? "," : string.Empty);
    }

    private static void AppendDualSword(StringBuilder sb, DualSwordProof value, int indent, bool comma)
    {
        sb.Append(Indent(indent)).AppendLine("\"phaseTwoDualSword\": {");
        Append(sb, "enabled", value.enabled, indent + 1, true);
        Append(sb, "mainhandSource", value.mainhandSource, indent + 1, true);
        Append(sb, "offhandSwordPrefab", value.offhandSwordPrefab, indent + 1, true);
        Append(sb, "offhandObjectName", value.offhandObjectName, indent + 1, true);
        Append(sb, "leftHandPath", value.leftHandPath, indent + 1, true);
        Append(sb, "rightHandPath", value.rightHandPath, indent + 1, true);
        Append(sb, "rendererCount", value.rendererCount, indent + 1, true);
        Append(sb, "meshRendererCount", value.meshRendererCount, indent + 1, true);
        Append(sb, "skinnedRendererCount", value.skinnedRendererCount, indent + 1, true);
        Append(sb, "localPosition", value.localPosition, indent + 1, true);
        Append(sb, "localRotation", value.localRotation, indent + 1, true);
        Append(sb, "localScale", value.localScale, indent + 1, true);
        Append(sb, "pass", value.pass, indent + 1, false);
        sb.Append(Indent(indent)).Append('}');
        sb.AppendLine(comma ? "," : string.Empty);
    }

    private static void AppendTextures(StringBuilder sb, IList<TextureProof> values, int indent, bool comma)
    {
        sb.Append(Indent(indent)).AppendLine("\"textures\": [");
        for (int i = 0; i < values.Count; i++)
        {
            var value = values[i];
            sb.Append(Indent(indent + 1)).AppendLine("{");
            Append(sb, "slot", value.slot, indent + 2, true);
            Append(sb, "path", value.path, indent + 2, true);
            Append(sb, "textureType", value.textureType, indent + 2, true);
            Append(sb, "srgb", value.srgb, indent + 2, true);
            Append(sb, "pass", value.pass, indent + 2, false);
            sb.Append(Indent(indent + 1)).Append('}');
            sb.AppendLine(i == values.Count - 1 ? string.Empty : ",");
        }
        sb.Append(Indent(indent)).Append(']');
        sb.AppendLine(comma ? "," : string.Empty);
    }

    private static void AppendAnimations(StringBuilder sb, IList<AnimationProof> values, int indent, bool comma)
    {
        sb.Append(Indent(indent)).AppendLine("\"animations\": [");
        for (int i = 0; i < values.Count; i++)
        {
            var value = values[i];
            sb.Append(Indent(indent + 1)).AppendLine("{");
            Append(sb, "id", value.id, indent + 2, true);
            Append(sb, "relativePath", value.relativePath, indent + 2, true);
            Append(sb, "sourcePath", value.sourcePath, indent + 2, true);
            Append(sb, "importedPath", value.importedPath, indent + 2, true);
            Append(sb, "expectedClipName", value.expectedClipName, indent + 2, true);
            Append(sb, "selectedClipName", value.selectedClipName, indent + 2, true);
            Append(sb, "category", value.category, indent + 2, true);
            Append(sb, "rootMotion", value.rootMotion, indent + 2, true);
            Append(sb, "required", value.required, indent + 2, true);
            Append(sb, "forcedHumanImporter", value.forcedHumanImporter, indent + 2, true);
            Append(sb, "retargetMode", value.retargetMode, indent + 2, true);
            Append(sb, "avatarCopySource", value.avatarCopySource, indent + 2, true);
            Append(sb, "importedAvatarHuman", value.importedAvatarHuman, indent + 2, true);
            Append(sb, "humanoidImporter", value.humanoidImporter, indent + 2, true);
            Append(sb, "pass", value.pass, indent + 2, true);
            AppendArray(sb, "importedClipNames", value.importedClipNames, indent + 2, false);
            sb.Append(Indent(indent + 1)).Append('}');
            sb.AppendLine(i == values.Count - 1 ? string.Empty : ",");
        }
        sb.Append(Indent(indent)).Append(']');
        sb.AppendLine(comma ? "," : string.Empty);
    }

    private static void AppendVfx(StringBuilder sb, IList<VfxProof> values, int indent, bool comma)
    {
        sb.Append(Indent(indent)).AppendLine("\"vfx\": [");
        for (int i = 0; i < values.Count; i++)
        {
            var value = values[i];
            sb.Append(Indent(indent + 1)).AppendLine("{");
            Append(sb, "id", value.id, indent + 2, true);
            Append(sb, "sourcePath", value.sourcePath, indent + 2, true);
            Append(sb, "importedRoot", value.importedRoot, indent + 2, true);
            Append(sb, "prefabPath", value.prefabPath, indent + 2, true);
            Append(sb, "particleSystemCount", value.particleSystemCount, indent + 2, true);
            Append(sb, "rendererCount", value.rendererCount, indent + 2, true);
            Append(sb, "dependencyCount", value.dependencyCount, indent + 2, true);
            Append(sb, "strippedScriptCount", value.strippedScriptCount, indent + 2, true);
            Append(sb, "pass", value.pass, indent + 2, false);
            sb.Append(Indent(indent + 1)).Append('}');
            sb.AppendLine(i == values.Count - 1 ? string.Empty : ",");
        }
        sb.Append(Indent(indent)).Append(']');
        sb.AppendLine(comma ? "," : string.Empty);
    }

    private static string Indent(int depth)
    {
        return new string(' ', depth * 2);
    }

    private static string Esc(string value)
    {
        if (value == null)
        {
            return string.Empty;
        }
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private sealed class MaterialRequirement
    {
        public readonly string name;
        public readonly string path;
        public readonly bool requiresEmissive;

        public MaterialRequirement(string name, string path, bool requiresEmissive)
        {
            this.name = name;
            this.path = path;
            this.requiresEmissive = requiresEmissive;
        }
    }

    private sealed class ControllerState
    {
        public readonly string name;
        public readonly string relativePath;
        public readonly Vector3 position;

        public ControllerState(string name, string relativePath, Vector3 position)
        {
            this.name = name;
            this.relativePath = relativePath;
            this.position = position;
        }
    }

    private sealed class ExternalDirectory
    {
        public readonly string id;
        public readonly string relativePath;
        public readonly string importRoot;
        public readonly string prefabPath;

        public ExternalDirectory(string id, string relativePath, string importRoot, string prefabPath)
        {
            this.id = id;
            this.relativePath = relativePath;
            this.importRoot = importRoot;
            this.prefabPath = prefabPath;
        }
    }

    [Serializable]
    private sealed class ProofReport
    {
        public string marker = string.Empty;
        public string unityVersion = string.Empty;
        public string projectRoot = string.Empty;
        public string externalAssetsRoot = string.Empty;
        public string bossMoveSetRoot = string.Empty;
        public bool runtimeAiTouched;
        public bool gameDeploymentTouched;
        public bool savePathTouched;
        public bool dragonKnightAvatarHuman;
        public int ironSkinnedRendererCount;
        public int fireSkinnedRendererCount;
        public int ironAnimatorCount;
        public int fireAnimatorCount;
        public int bossMoveSetFbxCount;
        public int requiredBossAnimationCount;
        public int requiredBossAnimationsPresent;
        public string bundlePath = string.Empty;
        public long bundleBytes;
        public string bundleSha256 = string.Empty;
        public List<string> selectedDragonKnightAssets = new List<string>();
        public List<string> generatedAssets = new List<string>();
        public List<string> bundleAssets = new List<string>();
        public List<string> ignoredBossSourceAssets = new List<string>();
        public DualSwordProof phaseTwoDualSword = new DualSwordProof();
        public List<MaterialProof> materials = new List<MaterialProof>();
        public List<AnimationProof> animations = new List<AnimationProof>();
        public List<VfxProof> vfx = new List<VfxProof>();
        public List<string> errors = new List<string>();
    }

    [Serializable]
    private sealed class DualSwordProof
    {
        public bool enabled;
        public string mainhandSource = string.Empty;
        public string offhandSwordPrefab = string.Empty;
        public string offhandObjectName = string.Empty;
        public string leftHandPath = string.Empty;
        public string rightHandPath = string.Empty;
        public int rendererCount;
        public int meshRendererCount;
        public int skinnedRendererCount;
        public string localPosition = string.Empty;
        public string localRotation = string.Empty;
        public string localScale = string.Empty;
        public bool pass;
    }

    [Serializable]
    private sealed class MaterialProof
    {
        public string name = string.Empty;
        public string materialPath = string.Empty;
        public bool requiresEmissive;
        public bool pass;
        public List<TextureProof> textures = new List<TextureProof>();
        public List<string> errors = new List<string>();
    }

    [Serializable]
    private sealed class TextureProof
    {
        public string slot = string.Empty;
        public string path = string.Empty;
        public string textureType = string.Empty;
        public bool srgb;
        public bool pass;
    }

    [Serializable]
    private sealed class AnimationProof
    {
        public string id = string.Empty;
        public string relativePath = string.Empty;
        public string sourcePath = string.Empty;
        public string importedPath = string.Empty;
        public string expectedClipName = string.Empty;
        public string selectedClipName = string.Empty;
        public string category = string.Empty;
        public bool rootMotion;
        public bool required;
        public bool forcedHumanImporter;
        public string retargetMode = string.Empty;
        public string avatarCopySource = string.Empty;
        public bool importedAvatarHuman;
        public bool humanoidImporter;
        public bool pass;
        public List<string> importedClipNames = new List<string>();
    }

    [Serializable]
    private sealed class VfxProof
    {
        public string id = string.Empty;
        public string sourcePath = string.Empty;
        public string importedRoot = string.Empty;
        public string prefabPath = string.Empty;
        public int particleSystemCount;
        public int rendererCount;
        public int dependencyCount;
        public int strippedScriptCount;
        public bool pass;
    }
}
