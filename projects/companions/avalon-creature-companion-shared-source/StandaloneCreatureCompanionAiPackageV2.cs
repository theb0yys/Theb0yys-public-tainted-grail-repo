using System;
using System.Collections.Generic;
using AvalonAI.Contracts.V2;
using AvalonCreatureCompanionShared.AI;

#if AVALON_BULL_COMPANION_AI_PACKAGE
namespace AvalonBullCompanion.AI.Package.V2
{

public static class AvalonBullCompanionAiV2Contract
{
    public const string PackageIdValue = "kane.tgfoa.avalon-bull-companion.ai-companion.v2";
    public const string AssemblyNameValue = "AvalonBullCompanion.AI.Package.V2";
    public const string DisplayName = "Avalon Bull Companion AI";
    public const string PackageVersion = "0.1.0-bull-companion-ai";
    public const string PackageMarkerString = "AVALON_BULL_COMPANION_AI_PACKAGE_V2_PASS";
    public const string ActorRoleIdValue = "avalon-bull-companion.role.fantasy-bull";
    public const string OwnerId = "kane.tgfoa.avalon-bull-companion";
    public const string CreatureId = "fantasy-bull";
    public const string CreatureDisplayName = "Fantasy Bull";
    public const string LocationTemplateName = "Spec_FantasyBull_CI4";
    public const string LocationTemplateGuid = "a416cad4e97900348a00d6b27678b963";
    public const string NpcTemplateGuid = "8849693b2c39fef40b6c635e00e89182";
    public const string SpellTemplateGuid = "b7d0d4e6c0de4bb0a000000000000008";
    public const string VisualAddress = "avalon-awakened/creatures/bull-fantasy/visual--8e524b00b51a76e43823aa39939bc4a3";
    public const int AttackVariantCount = 1;
    public const int HitVariantCount = 1;
    public const int DeathVariantCount = 1;
    public const bool DamageTimingProven = true;

    internal static CreatureCompanionAiProfile CreateProfile() => new(
        PackageIdValue,
        AssemblyNameValue,
        DisplayName,
        PackageVersion,
        PackageMarkerString,
        ActorRoleIdValue,
        OwnerId,
        CreatureId,
        CreatureDisplayName,
        LocationTemplateName,
        LocationTemplateGuid,
        NpcTemplateGuid,
        SpellTemplateGuid,
        VisualAddress,
        AttackVariantCount,
        HitVariantCount,
        DeathVariantCount,
        DamageTimingProven);
}

public sealed class AvalonBullCompanionAiPackageV2 : IAvalonAiPackage
{
    private readonly CreatureCompanionAiPackageCore core = new(AvalonBullCompanionAiV2Contract.CreateProfile());

    public AvalonAiPackageManifest Manifest => core.Manifest;
    public IReadOnlyList<IAvalonGoalPolicy> GoalPolicies => core.GoalPolicies;
    public IReadOnlyList<AvalonGoalDefinition> GoalDefinitions => core.GoalDefinitions;
    public IReadOnlyList<AvalonActionDefinition> ActionDefinitions => core.ActionDefinitions;
}
}
#endif

#if AVALON_DRAGON_COMPANION_AI_PACKAGE
namespace AvalonDragonCompanion.AI.Package.V2
{

public static class AvalonDragonCompanionAiV2Contract
{
    public const string PackageIdValue = "kane.tgfoa.avalon-dragon-companion.ai-companion.v2";
    public const string AssemblyNameValue = "AvalonDragonCompanion.AI.Package.V2";
    public const string DisplayName = "Avalon Dragon Companion AI";
    public const string PackageVersion = "0.1.0-dragon-companion-ai";
    public const string PackageMarkerString = "AVALON_DRAGON_COMPANION_AI_PACKAGE_V2_PASS";
    public const string ActorRoleIdValue = "avalon-dragon-companion.role.fantasy-dragon-grounded";
    public const string OwnerId = "kane.tgfoa.avalon-dragon-companion";
    public const string CreatureId = "fantasy-dragon-grounded";
    public const string CreatureDisplayName = "Fantasy Dragon";
    public const string LocationTemplateName = "Spec_FantasyDragon_CI4";
    public const string LocationTemplateGuid = "4b2df828561d1734bbcc18a737ad0475";
    public const string NpcTemplateGuid = "7263e375e3097a04bbc0f717e27b2364";
    public const string SpellTemplateGuid = "b7d0d4e6c0de4bb0a000000000000009";
    public const string VisualAddress = "avalon-awakened/creatures/fantasy-dragon/visual--4068b2aba7e996a4b9908f2d70626161";
    public const int AttackVariantCount = 3;
    public const int HitVariantCount = 1;
    public const int DeathVariantCount = 1;
    public const bool DamageTimingProven = true;

    internal static CreatureCompanionAiProfile CreateProfile() => new(
        PackageIdValue,
        AssemblyNameValue,
        DisplayName,
        PackageVersion,
        PackageMarkerString,
        ActorRoleIdValue,
        OwnerId,
        CreatureId,
        CreatureDisplayName,
        LocationTemplateName,
        LocationTemplateGuid,
        NpcTemplateGuid,
        SpellTemplateGuid,
        VisualAddress,
        AttackVariantCount,
        HitVariantCount,
        DeathVariantCount,
        DamageTimingProven);
}

public sealed class AvalonDragonCompanionAiPackageV2 : IAvalonAiPackage
{
    private readonly CreatureCompanionAiPackageCore core = new(AvalonDragonCompanionAiV2Contract.CreateProfile());

    public AvalonAiPackageManifest Manifest => core.Manifest;
    public IReadOnlyList<IAvalonGoalPolicy> GoalPolicies => core.GoalPolicies;
    public IReadOnlyList<AvalonGoalDefinition> GoalDefinitions => core.GoalDefinitions;
    public IReadOnlyList<AvalonActionDefinition> ActionDefinitions => core.ActionDefinitions;
}
}
#endif

#if AVALON_ELEPHANT_COMPANION_AI_PACKAGE
namespace AvalonElephantCompanion.AI.Package.V2
{

public static class AvalonElephantCompanionAiV2Contract
{
    public const string PackageIdValue = "kane.tgfoa.avalon-elephant-companion.ai-companion.v2";
    public const string AssemblyNameValue = "AvalonElephantCompanion.AI.Package.V2";
    public const string DisplayName = "Avalon Elephant Companion AI";
    public const string PackageVersion = "0.1.0-elephant-companion-ai";
    public const string PackageMarkerString = "AVALON_ELEPHANT_COMPANION_AI_PACKAGE_V2_PASS";
    public const string ActorRoleIdValue = "avalon-elephant-companion.role.fantasy-elephant";
    public const string OwnerId = "kane.tgfoa.avalon-elephant-companion";
    public const string CreatureId = "fantasy-elephant";
    public const string CreatureDisplayName = "Fantasy Elephant";
    public const string LocationTemplateName = "Spec_FantasyElephant_CI4";
    public const string LocationTemplateGuid = "8a1d351fe1b426c4b879a8f1724b7867";
    public const string NpcTemplateGuid = "8c827d370df13d640a4fc8efea55b034";
    public const string SpellTemplateGuid = "b7d0d4e6c0de4bb0a000000000000007";
    public const string VisualAddress = "avalon-awakened/creatures/fantasy-elephant/ci4-controlled/visual--0f2971ddc6ef5064292f716315f91379";
    public const int AttackVariantCount = 2;
    public const int HitVariantCount = 1;
    public const int DeathVariantCount = 1;
    public const bool DamageTimingProven = true;

    internal static CreatureCompanionAiProfile CreateProfile() => new(
        PackageIdValue,
        AssemblyNameValue,
        DisplayName,
        PackageVersion,
        PackageMarkerString,
        ActorRoleIdValue,
        OwnerId,
        CreatureId,
        CreatureDisplayName,
        LocationTemplateName,
        LocationTemplateGuid,
        NpcTemplateGuid,
        SpellTemplateGuid,
        VisualAddress,
        AttackVariantCount,
        HitVariantCount,
        DeathVariantCount,
        DamageTimingProven);
}

public sealed class AvalonElephantCompanionAiPackageV2 : IAvalonAiPackage
{
    private readonly CreatureCompanionAiPackageCore core = new(AvalonElephantCompanionAiV2Contract.CreateProfile());

    public AvalonAiPackageManifest Manifest => core.Manifest;
    public IReadOnlyList<IAvalonGoalPolicy> GoalPolicies => core.GoalPolicies;
    public IReadOnlyList<AvalonGoalDefinition> GoalDefinitions => core.GoalDefinitions;
    public IReadOnlyList<AvalonActionDefinition> ActionDefinitions => core.ActionDefinitions;
}
}
#endif

#if AVALON_GOBLIN_COMPANION_AI_PACKAGE
namespace AvalonGoblinCompanion.AI.Package.V2
{

public static class AvalonGoblinCompanionAiV2Contract
{
    public const string PackageIdValue = "kane.tgfoa.avalon-goblin-companion.ai-companion.v2";
    public const string AssemblyNameValue = "AvalonGoblinCompanion.AI.Package.V2";
    public const string DisplayName = "Avalon Goblin Companion AI";
    public const string PackageVersion = "0.1.0-goblin-companion-ai";
    public const string PackageMarkerString = "AVALON_GOBLIN_COMPANION_AI_PACKAGE_V2_PASS";
    public const string ActorRoleIdValue = "avalon-goblin-companion.role.goblin";
    public const string OwnerId = "kane.tgfoa.avalon-goblin-companion";
    public const string CreatureId = "goblin";
    public const string CreatureDisplayName = "Goblin";
    public const string LocationTemplateName = "Spec_Goblin_CI4";
    public const string LocationTemplateGuid = "3b66e9f376df39b47b3908a75fe7d2d4";
    public const string NpcTemplateGuid = "bd31ca6f2fe2b8047b19eace29f926ca";
    public const string SpellTemplateGuid = "b7d0d4e6c0de4bb0a000000000000011";
    public const string VisualAddress = "avalon-awakened/creatures/goblin/nonhuman-humanoid/visual-v1";
    public const int AttackVariantCount = 1;
    public const int HitVariantCount = 1;
    public const int DeathVariantCount = 1;
    public const bool DamageTimingProven = false;

    internal static CreatureCompanionAiProfile CreateProfile() => new(
        PackageIdValue,
        AssemblyNameValue,
        DisplayName,
        PackageVersion,
        PackageMarkerString,
        ActorRoleIdValue,
        OwnerId,
        CreatureId,
        CreatureDisplayName,
        LocationTemplateName,
        LocationTemplateGuid,
        NpcTemplateGuid,
        SpellTemplateGuid,
        VisualAddress,
        AttackVariantCount,
        HitVariantCount,
        DeathVariantCount,
        DamageTimingProven);
}

public sealed class AvalonGoblinCompanionAiPackageV2 : IAvalonAiPackage
{
    private readonly CreatureCompanionAiPackageCore core = new(AvalonGoblinCompanionAiV2Contract.CreateProfile());

    public AvalonAiPackageManifest Manifest => core.Manifest;
    public IReadOnlyList<IAvalonGoalPolicy> GoalPolicies => core.GoalPolicies;
    public IReadOnlyList<AvalonGoalDefinition> GoalDefinitions => core.GoalDefinitions;
    public IReadOnlyList<AvalonActionDefinition> ActionDefinitions => core.ActionDefinitions;
}
}
#endif

#if AVALON_VAMPIRE_COMPANION_AI_PACKAGE
namespace AvalonVampireCompanion.AI.Package.V2
{

public static class AvalonVampireCompanionAiV2Contract
{
    public const string PackageIdValue = "kane.tgfoa.avalon-vampire-companion.ai-companion.v2";
    public const string AssemblyNameValue = "AvalonVampireCompanion.AI.Package.V2";
    public const string DisplayName = "Avalon Vampire Companion AI";
    public const string PackageVersion = "0.1.0-vampire-companion-ai";
    public const string PackageMarkerString = "AVALON_VAMPIRE_COMPANION_AI_PACKAGE_V2_PASS";
    public const string ActorRoleIdValue = "avalon-vampire-companion.role.vampire";
    public const string OwnerId = "kane.tgfoa.avalon-vampire-companion";
    public const string CreatureId = "vampire";
    public const string CreatureDisplayName = "Vampire";
    public const string LocationTemplateName = "Spec_Vampire_CI4";
    public const string LocationTemplateGuid = "05fcd530c78b2854b87b37861419e630";
    public const string NpcTemplateGuid = "4b001c16492a0944fb171bdb200c0521";
    public const string SpellTemplateGuid = "b7d0d4e6c0de4bb0a000000000000010";
    public const string VisualAddress = "avalon-awakened/creatures/vampire/skin1/visual-ci4-v1";
    public const int AttackVariantCount = 5;
    public const int HitVariantCount = 2;
    public const int DeathVariantCount = 3;
    public const bool DamageTimingProven = true;

    internal static CreatureCompanionAiProfile CreateProfile() => new(
        PackageIdValue,
        AssemblyNameValue,
        DisplayName,
        PackageVersion,
        PackageMarkerString,
        ActorRoleIdValue,
        OwnerId,
        CreatureId,
        CreatureDisplayName,
        LocationTemplateName,
        LocationTemplateGuid,
        NpcTemplateGuid,
        SpellTemplateGuid,
        VisualAddress,
        AttackVariantCount,
        HitVariantCount,
        DeathVariantCount,
        DamageTimingProven);
}

public sealed class AvalonVampireCompanionAiPackageV2 : IAvalonAiPackage
{
    private readonly CreatureCompanionAiPackageCore core = new(AvalonVampireCompanionAiV2Contract.CreateProfile());

    public AvalonAiPackageManifest Manifest => core.Manifest;
    public IReadOnlyList<IAvalonGoalPolicy> GoalPolicies => core.GoalPolicies;
    public IReadOnlyList<AvalonGoalDefinition> GoalDefinitions => core.GoalDefinitions;
    public IReadOnlyList<AvalonActionDefinition> ActionDefinitions => core.ActionDefinitions;
}
}
#endif

namespace AvalonCreatureCompanionShared.AI
{

internal sealed class CreatureCompanionAiProfile
{
    internal const int IdleStateId = 1;
    internal const int MovementStateId = 2;
    internal const int ShortRangeStateId = 16;
    internal const int GetHitStateId = 32;
    internal const int DeathStateId = 44;
    internal const int BlackboardSchemaVersion = 2;

    internal CreatureCompanionAiProfile(
        string packageId,
        string assemblyName,
        string displayName,
        string packageVersion,
        string packageMarker,
        string actorRoleId,
        string ownerId,
        string creatureId,
        string creatureDisplayName,
        string locationTemplateName,
        string locationTemplateGuid,
        string npcTemplateGuid,
        string spellTemplateGuid,
        string visualAddress,
        int attackVariantCount,
        int hitVariantCount,
        int deathVariantCount,
        bool damageTimingProven)
    {
        PackageIdValue = RequireText(packageId, nameof(packageId));
        AssemblyNameValue = RequireText(assemblyName, nameof(assemblyName));
        DisplayName = RequireText(displayName, nameof(displayName));
        PackageVersion = RequireText(packageVersion, nameof(packageVersion));
        PackageMarkerString = RequireText(packageMarker, nameof(packageMarker));
        ActorRoleIdValue = RequireText(actorRoleId, nameof(actorRoleId));
        OwnerId = RequireText(ownerId, nameof(ownerId));
        CreatureId = RequireText(creatureId, nameof(creatureId));
        CreatureDisplayName = RequireText(creatureDisplayName, nameof(creatureDisplayName));
        LocationTemplateName = RequireText(locationTemplateName, nameof(locationTemplateName));
        LocationTemplateGuid = RequireText(locationTemplateGuid, nameof(locationTemplateGuid));
        NpcTemplateGuid = RequireText(npcTemplateGuid, nameof(npcTemplateGuid));
        SpellTemplateGuid = RequireText(spellTemplateGuid, nameof(spellTemplateGuid));
        VisualAddress = RequireText(visualAddress, nameof(visualAddress));
        AttackVariantCount = RequirePositive(attackVariantCount, nameof(attackVariantCount));
        HitVariantCount = RequirePositive(hitVariantCount, nameof(hitVariantCount));
        DeathVariantCount = RequirePositive(deathVariantCount, nameof(deathVariantCount));
        DamageTimingProven = damageTimingProven;

        IdPrefix = PackageIdValue;
        BlackboardNamespace = ToBlackboardNamespace(CreatureId);
        PackageId = new PackageId(PackageIdValue);
        ActorRoleId = new ActorRoleId(ActorRoleIdValue);

        RoleBoundId = Local<string>("role.bound_id");
        LifecycleActive = Local<bool>("lifecycle.active");
        CleanupRequested = Local<bool>("lifecycle.cleanup_requested");
        LeaseValid = Local<bool>("lease.valid");
        ActorAlive = Local<bool>("actor.alive");
        SaveExcluded = Local<bool>("actor.save_excluded");
        TargetValid = Local<bool>("target.valid");
        TargetId = Local<string>("target.id");
        TargetDistanceBand = Local<int>("target.distance_band");
        TargetAngleBand = Local<int>("target.angle_band");
        TargetThreatening = Local<bool>("target.threatening");
        MovementStuck = Local<bool>("movement.stuck");
        RecoveryRequired = Local<bool>("action.recovery_required");
        CorpseRetained = Local<bool>("corpse.retained");
        KillSwitchActive = Local<bool>("kill_switch.active");
        CommandMode = Local<string>("command.mode");

        BuildIds();
        BuildCapabilities();
        BuildProcedures();
        BuildSchema();
        BuildDefinitions();
    }

    internal string PackageIdValue { get; }
    internal string AssemblyNameValue { get; }
    internal string DisplayName { get; }
    internal string PackageVersion { get; }
    internal string PackageMarkerString { get; }
    internal string ActorRoleIdValue { get; }
    internal string OwnerId { get; }
    internal string CreatureId { get; }
    internal string CreatureDisplayName { get; }
    internal string LocationTemplateName { get; }
    internal string LocationTemplateGuid { get; }
    internal string NpcTemplateGuid { get; }
    internal string SpellTemplateGuid { get; }
    internal string VisualAddress { get; }
    internal int AttackVariantCount { get; }
    internal int HitVariantCount { get; }
    internal int DeathVariantCount { get; }
    internal bool DamageTimingProven { get; }
    internal string IdPrefix { get; }
    internal string BlackboardNamespace { get; }
    internal PackageId PackageId { get; }
    internal ActorRoleId ActorRoleId { get; }

    internal BlackboardKey<string> RoleBoundId { get; }
    internal BlackboardKey<bool> LifecycleActive { get; }
    internal BlackboardKey<bool> CleanupRequested { get; }
    internal BlackboardKey<bool> LeaseValid { get; }
    internal BlackboardKey<bool> ActorAlive { get; }
    internal BlackboardKey<bool> SaveExcluded { get; }
    internal BlackboardKey<bool> TargetValid { get; }
    internal BlackboardKey<string> TargetId { get; }
    internal BlackboardKey<int> TargetDistanceBand { get; }
    internal BlackboardKey<int> TargetAngleBand { get; }
    internal BlackboardKey<bool> TargetThreatening { get; }
    internal BlackboardKey<bool> MovementStuck { get; }
    internal BlackboardKey<bool> RecoveryRequired { get; }
    internal BlackboardKey<bool> CorpseRetained { get; }
    internal BlackboardKey<bool> KillSwitchActive { get; }
    internal BlackboardKey<string> CommandMode { get; }

    internal GoalId GoalStopSafely { get; private set; }
    internal GoalId GoalFollowHero { get; private set; }
    internal GoalId GoalIdleNearHero { get; private set; }
    internal GoalId GoalRecover { get; private set; }
    internal GoalId GoalObserveThreat { get; private set; }
    internal GoalId GoalDefensiveResponse { get; private set; }
    internal GoalId GoalCommitShortRange { get; private set; }
    internal GoalId GoalReactToHit { get; private set; }
    internal GoalId GoalDeathRetainedCorpse { get; private set; }
    internal GoalId GoalCleanupSession { get; private set; }
    internal GoalId GoalFailClosed { get; private set; }

    internal ActionId ActionValidateAuthority { get; private set; }
    internal ActionId ActionResolveHeroAnchor { get; private set; }
    internal ActionId ActionFollowHeroAnchor { get; private set; }
    internal ActionId ActionIdleNearHero { get; private set; }
    internal ActionId ActionRecoverStuck { get; private set; }
    internal ActionId ActionObserveThreat { get; private set; }
    internal ActionId ActionChooseDefensiveResponse { get; private set; }
    internal ActionId ActionCommitShortRange { get; private set; }
    internal ActionId ActionReactToHit { get; private set; }
    internal ActionId ActionHandleDeathAndCorpse { get; private set; }
    internal ActionId ActionEvaluateCleanup { get; private set; }
    internal ActionId ActionFailClosed { get; private set; }

    internal ActionCapability CapabilityObserveActor { get; private set; }
    internal ActionCapability CapabilityObserveHeroAnchor { get; private set; }
    internal ActionCapability CapabilityFollowHeroAnchor { get; private set; }
    internal ActionCapability CapabilityIdleNearHero { get; private set; }
    internal ActionCapability CapabilityRecoverStuck { get; private set; }
    internal ActionCapability CapabilityObserveThreat { get; private set; }
    internal ActionCapability CapabilityFaceTarget { get; private set; }
    internal ActionCapability CapabilityShortRangeAttack { get; private set; }
    internal ActionCapability CapabilityHitReaction { get; private set; }
    internal ActionCapability CapabilityDeathCorpse { get; private set; }
    internal ActionCapability CapabilityCleanup { get; private set; }
    internal ActionCapability CapabilityStopAll { get; private set; }

    internal AvalonProcedureReference ValidateAuthorityProcedure { get; private set; } = null!;
    internal AvalonProcedureReference ResolveHeroAnchorProcedure { get; private set; } = null!;
    internal AvalonProcedureReference FollowHeroProcedure { get; private set; } = null!;
    internal AvalonProcedureReference IdleNearHeroProcedure { get; private set; } = null!;
    internal AvalonProcedureReference RecoverProcedure { get; private set; } = null!;
    internal AvalonProcedureReference ObserveThreatProcedure { get; private set; } = null!;
    internal AvalonProcedureReference FaceTargetProcedure { get; private set; } = null!;
    internal AvalonProcedureReference ShortRangeAttackProcedure { get; private set; } = null!;
    internal AvalonProcedureReference HitReactionProcedure { get; private set; } = null!;
    internal AvalonProcedureReference DeathCorpseProcedure { get; private set; } = null!;
    internal AvalonProcedureReference CleanupProcedure { get; private set; } = null!;
    internal AvalonProcedureReference StopAllProcedure { get; private set; } = null!;

    internal IReadOnlyList<GoalId> GoalIds { get; private set; } = Array.Empty<GoalId>();
    internal IReadOnlyList<ActionId> ActionIds { get; private set; } = Array.Empty<ActionId>();
    internal IReadOnlyList<ActionCapability> RequiredCapabilities { get; private set; } = Array.Empty<ActionCapability>();
    internal IReadOnlyList<BlackboardKeyDeclaration> PackageLocalKeyDeclarations { get; private set; } = Array.Empty<BlackboardKeyDeclaration>();
    internal IReadOnlyList<BlackboardKeyDeclaration> PersistentKeys { get; private set; } = Array.Empty<BlackboardKeyDeclaration>();
    internal IReadOnlyList<AvalonProcedureRequirement> ProcedureRequirements { get; private set; } = Array.Empty<AvalonProcedureRequirement>();
    internal IReadOnlyList<AvalonGoalDefinition> GoalDefinitions { get; private set; } = Array.Empty<AvalonGoalDefinition>();
    internal IReadOnlyList<AvalonActionDefinition> ActionDefinitions { get; private set; } = Array.Empty<AvalonActionDefinition>();

    internal string PackageMarkerLine =>
        PackageMarkerString
        + " package=" + PackageIdValue
        + " assembly=" + AssemblyNameValue
        + " creature=" + CreatureId
        + " role=" + ActorRoleIdValue
        + " owner=" + OwnerId
        + " template=" + LocationTemplateName + "[" + LocationTemplateGuid + "]"
        + " npc=" + NpcTemplateGuid
        + " spell=" + SpellTemplateGuid
        + " visual=" + VisualAddress
        + " states=Idle(1)|Movement(2)|ShortRange(16)|GetHit(32)|Death(44)"
        + " attacks=" + AttackVariantCount
        + " hits=" + HitVariantCount
        + " deaths=" + DeathVariantCount
        + " damageTimingProven=" + (DamageTimingProven ? "1" : "0")
        + " defaultOff=0 killSwitchSupported=1 saveWrites=0 directNative=0 standalone=1";

    private void BuildIds()
    {
        GoalStopSafely = Goal("stop-safely");
        GoalFollowHero = Goal("follow-hero");
        GoalIdleNearHero = Goal("idle-near-hero");
        GoalRecover = Goal("recover");
        GoalObserveThreat = Goal("observe-threat");
        GoalDefensiveResponse = Goal("defensive-response");
        GoalCommitShortRange = Goal("commit-short-range");
        GoalReactToHit = Goal("react-to-hit");
        GoalDeathRetainedCorpse = Goal("death-retained-corpse");
        GoalCleanupSession = Goal("cleanup-session");
        GoalFailClosed = Goal("fail-closed");

        ActionValidateAuthority = Action("validate-authority");
        ActionResolveHeroAnchor = Action("resolve-hero-anchor");
        ActionFollowHeroAnchor = Action("follow-hero-anchor");
        ActionIdleNearHero = Action("idle-near-hero");
        ActionRecoverStuck = Action("recover-stuck");
        ActionObserveThreat = Action("observe-threat");
        ActionChooseDefensiveResponse = Action("choose-defensive-response");
        ActionCommitShortRange = Action("commit-short-range");
        ActionReactToHit = Action("react-to-hit");
        ActionHandleDeathAndCorpse = Action("handle-death-and-corpse");
        ActionEvaluateCleanup = Action("evaluate-cleanup");
        ActionFailClosed = Action("fail-closed");

        GoalIds = Array.AsReadOnly(new[]
        {
            GoalStopSafely,
            GoalFollowHero,
            GoalIdleNearHero,
            GoalRecover,
            GoalObserveThreat,
            GoalDefensiveResponse,
            GoalCommitShortRange,
            GoalReactToHit,
            GoalDeathRetainedCorpse,
            GoalCleanupSession,
            GoalFailClosed,
        });

        ActionIds = Array.AsReadOnly(new[]
        {
            ActionValidateAuthority,
            ActionResolveHeroAnchor,
            ActionFollowHeroAnchor,
            ActionIdleNearHero,
            ActionRecoverStuck,
            ActionObserveThreat,
            ActionChooseDefensiveResponse,
            ActionCommitShortRange,
            ActionReactToHit,
            ActionHandleDeathAndCorpse,
            ActionEvaluateCleanup,
            ActionFailClosed,
        });
    }

    private void BuildCapabilities()
    {
        CapabilityObserveActor = Capability("observe-actor");
        CapabilityObserveHeroAnchor = Capability("observe-hero-anchor");
        CapabilityFollowHeroAnchor = Capability("follow-hero-anchor");
        CapabilityIdleNearHero = Capability("idle-near-hero");
        CapabilityRecoverStuck = Capability("recover-stuck");
        CapabilityObserveThreat = Capability("observe-threat");
        CapabilityFaceTarget = Capability("face-target");
        CapabilityShortRangeAttack = Capability("short-range-16");
        CapabilityHitReaction = Capability("get-hit-32");
        CapabilityDeathCorpse = Capability("death-44-corpse");
        CapabilityCleanup = Capability("cleanup-session");
        CapabilityStopAll = Capability("stop-all");

        RequiredCapabilities = Array.AsReadOnly(new[]
        {
            CapabilityObserveActor,
            CapabilityObserveHeroAnchor,
            CapabilityFollowHeroAnchor,
            CapabilityIdleNearHero,
            CapabilityRecoverStuck,
            CapabilityObserveThreat,
            CapabilityFaceTarget,
            CapabilityShortRangeAttack,
            CapabilityHitReaction,
            CapabilityDeathCorpse,
            CapabilityCleanup,
            CapabilityStopAll,
        });
    }

    private void BuildProcedures()
    {
        ValidateAuthorityProcedure = Procedure("validate-authority", 500, false, CapabilityObserveActor);
        ResolveHeroAnchorProcedure = Procedure("resolve-hero-anchor", 500, false, CapabilityObserveHeroAnchor);
        FollowHeroProcedure = Procedure("follow-hero-anchor", 1800, true, CapabilityFollowHeroAnchor);
        IdleNearHeroProcedure = Procedure("idle-near-hero", 1200, true, CapabilityIdleNearHero);
        RecoverProcedure = Procedure("recover-stuck", 1200, true, CapabilityRecoverStuck);
        ObserveThreatProcedure = Procedure("observe-threat", 500, false, CapabilityObserveThreat);
        FaceTargetProcedure = Procedure("face-target", 700, true, CapabilityFaceTarget);
        ShortRangeAttackProcedure = Procedure("short-range-16", AttackTimeoutMilliseconds(), true, CapabilityShortRangeAttack);
        HitReactionProcedure = Procedure("get-hit-32", 1400, true, CapabilityHitReaction);
        DeathCorpseProcedure = Procedure("death-44-corpse", 3000, true, CapabilityDeathCorpse);
        CleanupProcedure = Procedure("cleanup-session", 1000, true, CapabilityCleanup);
        StopAllProcedure = Procedure("stop-all", 500, true, CapabilityStopAll);

        ProcedureRequirements = Array.AsReadOnly(new[]
        {
            Requirement(ValidateAuthorityProcedure),
            Requirement(ResolveHeroAnchorProcedure),
            Requirement(FollowHeroProcedure),
            Requirement(IdleNearHeroProcedure),
            Requirement(RecoverProcedure),
            Requirement(ObserveThreatProcedure),
            Requirement(FaceTargetProcedure),
            Requirement(ShortRangeAttackProcedure),
            Requirement(HitReactionProcedure),
            Requirement(DeathCorpseProcedure),
            Requirement(CleanupProcedure),
            Requirement(StopAllProcedure),
        });
    }

    private void BuildSchema()
    {
        PackageLocalKeyDeclarations = Array.AsReadOnly(new[]
        {
            RoleBoundId.Declaration,
            LifecycleActive.Declaration,
            CleanupRequested.Declaration,
            LeaseValid.Declaration,
            ActorAlive.Declaration,
            SaveExcluded.Declaration,
            TargetValid.Declaration,
            TargetId.Declaration,
            TargetDistanceBand.Declaration,
            TargetAngleBand.Declaration,
            TargetThreatening.Declaration,
            MovementStuck.Declaration,
            RecoveryRequired.Declaration,
            CorpseRetained.Declaration,
            KillSwitchActive.Declaration,
            CommandMode.Declaration,
        });
    }

    private void BuildDefinitions()
    {
        PlanningFactId ready = Fact("authority-ready");
        PlanningFactId heroAnchor = Fact("hero-anchor-ready");
        PlanningFactId following = Fact("following-hero");
        PlanningFactId idle = Fact("idle-near-hero");
        PlanningFactId recovered = Fact("recovered");
        PlanningFactId threatObserved = Fact("threat-observed");
        PlanningFactId defensiveReady = Fact("defensive-ready");
        PlanningFactId attackComplete = Fact("attack-complete");
        PlanningFactId hitReacted = Fact("hit-reacted");
        PlanningFactId deathHandled = Fact("death-handled");
        PlanningFactId cleanupDone = Fact("cleanup-done");
        PlanningFactId failClosed = Fact("fail-closed");

        PlanningTargetKeyId self = TargetKey("self");
        PlanningTargetKeyId hero = TargetKey("hero");
        PlanningTargetKeyId threat = TargetKey("current-threat");

        GoalDefinitions = Array.AsReadOnly(new[]
        {
            GoalDefinition(GoalStopSafely, failClosed),
            GoalDefinition(GoalFollowHero, following),
            GoalDefinition(GoalIdleNearHero, idle),
            GoalDefinition(GoalRecover, recovered),
            GoalDefinition(GoalObserveThreat, threatObserved),
            GoalDefinition(GoalDefensiveResponse, defensiveReady),
            GoalDefinition(GoalCommitShortRange, attackComplete),
            GoalDefinition(GoalReactToHit, hitReacted),
            GoalDefinition(GoalDeathRetainedCorpse, deathHandled),
            GoalDefinition(GoalCleanupSession, cleanupDone),
            GoalDefinition(GoalFailClosed, failClosed),
        });

        ActionDefinitions = Array.AsReadOnly(new[]
        {
            Define(ActionValidateAuthority, Array.Empty<PlanningCondition>(), Set(ready), self, CapabilityObserveActor, InterruptPolicy.Always, ValidateAuthorityProcedure, 0.25f),
            Define(ActionResolveHeroAnchor, Eq(ready), Set(heroAnchor), hero, CapabilityObserveHeroAnchor, InterruptPolicy.Always, ResolveHeroAnchorProcedure, 0.5f),
            Define(ActionFollowHeroAnchor, Eq(heroAnchor), Set(following), hero, CapabilityFollowHeroAnchor, InterruptPolicy.OnThreatIncrease, FollowHeroProcedure, 1f),
            Define(ActionIdleNearHero, Eq(heroAnchor), Set(idle), self, CapabilityIdleNearHero, InterruptPolicy.OnThreatIncrease, IdleNearHeroProcedure, 1f),
            Define(ActionRecoverStuck, Eq(ready), Set(recovered), self, CapabilityRecoverStuck, InterruptPolicy.OnDamage, RecoverProcedure, 0.8f),
            Define(ActionObserveThreat, Eq(ready), Set(threatObserved), threat, CapabilityObserveThreat, InterruptPolicy.Always, ObserveThreatProcedure, 0.5f),
            Define(ActionChooseDefensiveResponse, Eq(threatObserved), Set(defensiveReady), threat, CapabilityFaceTarget, InterruptPolicy.OnInvalidTarget, FaceTargetProcedure, 1f),
            Define(ActionCommitShortRange, Eq(defensiveReady), Set(attackComplete), threat, CapabilityShortRangeAttack, InterruptPolicy.OnDamage, ShortRangeAttackProcedure, DamageTimingProven ? 1f : 4f),
            Define(ActionReactToHit, Eq(ready), Set(hitReacted), self, CapabilityHitReaction, InterruptPolicy.Always, HitReactionProcedure, 0.75f),
            Define(ActionHandleDeathAndCorpse, Array.Empty<PlanningCondition>(), Set(deathHandled), self, CapabilityDeathCorpse, InterruptPolicy.Always, DeathCorpseProcedure, 0.25f),
            Define(ActionEvaluateCleanup, Array.Empty<PlanningCondition>(), Set(cleanupDone), self, CapabilityCleanup, InterruptPolicy.Always, CleanupProcedure, 0.25f),
            Define(ActionFailClosed, Array.Empty<PlanningCondition>(), Set(failClosed), self, CapabilityStopAll, InterruptPolicy.Always, StopAllProcedure, 0f),
        });
    }

    private int AttackTimeoutMilliseconds()
    {
        int baseline = CreatureId.IndexOf("dragon", StringComparison.OrdinalIgnoreCase) >= 0 ? 6500 : 2200;
        if (CreatureId.IndexOf("vampire", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            baseline = 3000;
        }

        if (CreatureId.IndexOf("elephant", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            baseline = 3200;
        }

        return baseline;
    }

    private GoalId Goal(string suffix) => new(IdPrefix + ".goal." + suffix);
    private ActionId Action(string suffix) => new(IdPrefix + ".action." + suffix);
    private PlanningFactId Fact(string suffix) => new(IdPrefix + ".fact." + suffix);
    private PlanningTargetKeyId TargetKey(string suffix) => new(IdPrefix + ".target." + suffix);
    private ActionCapability Capability(string suffix) => new(IdPrefix + ".capability." + suffix);

    private BlackboardKey<T> Local<T>(string name) =>
        new(BlackboardNamespace, name, BlackboardSchemaVersion, BlackboardAccess.PackageLocal, BlackboardScope.Actor);

    private AvalonProcedureReference Procedure(
        string suffix,
        int timeoutMilliseconds,
        bool mutatesActor,
        params ActionCapability[] capabilities) =>
        new(
            new AvalonProcedureId(IdPrefix + ".procedure." + suffix + ".v1"),
            new AvalonProcedureVersion("v1"),
            capabilities,
            TimeSpan.FromMilliseconds(timeoutMilliseconds),
            mutatesActor);

    private static AvalonProcedureRequirement Requirement(AvalonProcedureReference procedure) =>
        new(procedure.Id, procedure.RequiredVersion, procedure.RequiredCapabilities);

    private static AvalonGoalDefinition GoalDefinition(GoalId id, PlanningFactId desiredFact) =>
        new(id, Eq(desiredFact));

    private static AvalonActionDefinition Define(
        ActionId id,
        IReadOnlyList<PlanningCondition> conditions,
        PlanningEffect effect,
        PlanningTargetKeyId target,
        ActionCapability capability,
        InterruptPolicy interruptPolicy,
        AvalonProcedureReference procedure,
        float cost) =>
        new(
            id,
            conditions,
            new[] { effect },
            target,
            new ConstantActionCostProvider(cost),
            capability,
            interruptPolicy,
            procedure.Timeout,
            AvalonActionExecutionProfile.ForProcedure(procedure));

    private static PlanningCondition[] Eq(PlanningFactId fact) =>
        new[] { new PlanningCondition(fact, PlanningComparison.Equal, 1) };

    private static PlanningEffect Set(PlanningFactId fact) =>
        new(fact, 1);

    private static int RequirePositive(int value, string name) =>
        value > 0 ? value : throw new ArgumentOutOfRangeException(name);

    private static string RequireText(string value, string name) =>
        string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("A non-empty value is required.", name) : value.Trim();

    private static string ToBlackboardNamespace(string creatureId)
    {
        char[] chars = creatureId.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] == '-')
            {
                chars[i] = '_';
            }
        }

        return "avalon_" + new string(chars) + "_companion.ai.v2";
    }
}

internal sealed class CreatureCompanionAiPackageCore
{
    private readonly CreatureCompanionAiProfile profile;
    private readonly IReadOnlyList<IAvalonGoalPolicy> goalPolicies;

    internal CreatureCompanionAiPackageCore(CreatureCompanionAiProfile profile)
    {
        this.profile = profile ?? throw new ArgumentNullException(nameof(profile));
        Manifest = new AvalonAiPackageManifest(
            profile.PackageId,
            profile.DisplayName,
            profile.PackageVersion,
            AvalonAiContracts.ApiVersion,
            profile.GoalIds,
            profile.ActionIds,
            profile.RequiredCapabilities,
            profile.BlackboardNamespace,
            CreatureCompanionAiProfile.BlackboardSchemaVersion,
            profile.PackageLocalKeyDeclarations,
            new[] { profile.ActorRoleId },
            TimeSpan.FromMilliseconds(150),
            profile.PersistentKeys,
            profile.ProcedureRequirements);

        goalPolicies = Array.AsReadOnly<IAvalonGoalPolicy>(new IAvalonGoalPolicy[]
        {
            new CreatureCompanionGoalPolicyV2(profile),
        });
    }

    internal AvalonAiPackageManifest Manifest { get; }
    internal IReadOnlyList<IAvalonGoalPolicy> GoalPolicies => goalPolicies;
    internal IReadOnlyList<AvalonGoalDefinition> GoalDefinitions => profile.GoalDefinitions;
    internal IReadOnlyList<AvalonActionDefinition> ActionDefinitions => profile.ActionDefinitions;
}

internal sealed class CreatureCompanionGoalPolicyV2 : IAvalonGoalPolicy
{
    private static readonly IReadOnlyList<GoalRequest> NoGoals =
        Array.AsReadOnly(Array.Empty<GoalRequest>());

    private readonly CreatureCompanionAiProfile profile;

    internal CreatureCompanionGoalPolicyV2(CreatureCompanionAiProfile profile)
    {
        this.profile = profile ?? throw new ArgumentNullException(nameof(profile));
    }

    public IReadOnlyList<GoalRequest> Evaluate(
        ActorSnapshot actor,
        IAvalonBlackboardReader blackboard)
    {
        _ = actor ?? throw new ArgumentNullException(nameof(actor));
        _ = blackboard ?? throw new ArgumentNullException(nameof(blackboard));

        if (actor.Lease.Mode != ActorExecutionMode.BlazeOwned ||
            actor.Role != profile.ActorRoleId ||
            !blackboard.TryRead(profile.RoleBoundId, out string boundRole) ||
            !StringComparer.Ordinal.Equals(boundRole, actor.Role.Value))
        {
            return NoGoals;
        }

        if (Read(blackboard, profile.KillSwitchActive) ||
            Read(blackboard, profile.CleanupRequested) ||
            !Read(blackboard, profile.LeaseValid) ||
            !Read(blackboard, profile.SaveExcluded))
        {
            return One(profile.GoalStopSafely, 100f, 100f, 0, profile.CreatureId + ".stop-safely");
        }

        if (!Read(blackboard, profile.LifecycleActive))
        {
            return One(profile.GoalFailClosed, 95f, 90f, 0, profile.CreatureId + ".lifecycle-inactive");
        }

        if (!Read(blackboard, profile.ActorAlive))
        {
            return One(profile.GoalDeathRetainedCorpse, 90f, 80f, 0, profile.CreatureId + ".actor-death");
        }

        if (Read(blackboard, profile.RecoveryRequired) || Read(blackboard, profile.MovementStuck))
        {
            return One(profile.GoalRecover, 75f, 45f, 200, profile.CreatureId + ".recover");
        }

        if (Read(blackboard, profile.TargetValid) && Read(blackboard, profile.TargetThreatening))
        {
            return One(profile.GoalCommitShortRange, 65f, profile.DamageTimingProven ? 30f : 15f, 250, profile.CreatureId + ".short-range");
        }

        return One(profile.GoalFollowHero, 50f, 10f, 300, profile.CreatureId + ".follow-hero");
    }

    private static bool Read(IAvalonBlackboardReader blackboard, BlackboardKey<bool> key)
    {
        return blackboard.TryRead(key, out bool value) && value;
    }

    private static IReadOnlyList<GoalRequest> One(
        GoalId goal,
        float priority,
        float urgency,
        int commitmentMilliseconds,
        string reason)
    {
        return Array.AsReadOnly(new[]
        {
            new GoalRequest(
                goal,
                priority,
                urgency,
                TimeSpan.FromMilliseconds(commitmentMilliseconds),
                reason),
        });
    }
}
}
