using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AvalonAI.Contracts.V2;
using AvalonBullCompanion.AI.Package.V2;
using AvalonDragonCompanion.AI.Package.V2;
using AvalonElephantCompanion.AI.Package.V2;
using AvalonGoblinCompanion.AI.Package.V2;
using AvalonVampireCompanion.AI.Package.V2;

internal static class Program
{
    private static readonly ActorLease Lease = new(
        new ActorId("fixture.creature-companion"),
        new OwnershipLeaseId("fixture.creature-companion.lease"),
        ActorExecutionMode.BlazeOwned,
        1);

    private static int Main()
    {
        var fixtures = new (string Name, Action Run)[]
        {
            ("all standalone companion packages expose exact manifests", AllPackagesExposeExactManifests),
            ("goal policies fail closed and select lifecycle goals", GoalPoliciesSelectLifecycleGoals),
            ("action procedure contracts are bounded", ActionProcedureContractsAreBounded),
            ("package assemblies remain Contracts V2 only", AssemblyBoundariesAreContractsOnly),
        };

        var failures = new List<string>();
        foreach (var fixture in fixtures)
        {
            try
            {
                fixture.Run();
                Console.WriteLine("PASS " + fixture.Name);
            }
            catch (Exception exception)
            {
                failures.Add(fixture.Name + ": " + exception.Message);
                Console.WriteLine("FAIL " + fixture.Name + ": " + exception);
            }
        }

        if (failures.Count == 0)
        {
            Console.WriteLine("AVALON_STANDALONE_CREATURE_COMPANION_AI_PACKAGES_V2_PASS packages=5 standalone=1 all_in_one=0 contracts_v2_only=1");
            return 0;
        }

        Console.WriteLine(failures.Count + " fixture(s) failed:");
        foreach (string failure in failures)
        {
            Console.WriteLine(" - " + failure);
        }

        return 1;
    }

    private static void AllPackagesExposeExactManifests()
    {
        foreach (PackageCase test in PackageCases())
        {
            IAvalonAiPackage package = test.Create();
            AvalonAiPackageManifest manifest = package.Manifest;

            Check(manifest.Id.Value == test.PackageId, test.Name + " package ID changed");
            Check(manifest.DisplayName == test.DisplayName, test.Name + " display name changed");
            Check(manifest.RequiredRuntimeApi == AvalonAiContracts.ApiVersion, test.Name + " API version changed");
            Check(manifest.BlackboardSchemaVersion == 2, test.Name + " schema version changed");
            Check(manifest.MaximumPolicyCadence == TimeSpan.FromMilliseconds(150), test.Name + " cadence changed");
            Check(manifest.SupportedActorRoles.Count == 1, test.Name + " must expose one standalone role");
            Check(manifest.SupportedActorRoles[0].Value == test.RoleId, test.Name + " role ID changed");
            Check(manifest.GoalIds.Count == 11, test.Name + " goal count changed");
            Check(manifest.ActionIds.Count == 12, test.Name + " action count changed");
            Check(manifest.RequiredCapabilities.Count == 12, test.Name + " capability count changed");
            Check(manifest.BlackboardKeys.Count == 16, test.Name + " blackboard key count changed");
            Check(manifest.ProcedureRequirements.Count == 12, test.Name + " procedure count changed");
            Check(manifest.PersistentKeys.Count == 0, test.Name + " must remain session-only");
            Check(package.GoalPolicies.Count == 1, test.Name + " goal policy count changed");
            Check(package.GoalDefinitions.Count == manifest.GoalIds.Count, test.Name + " goal definitions mismatch");
            Check(package.ActionDefinitions.Count == manifest.ActionIds.Count, test.Name + " action definitions mismatch");
            Check(manifest.BlackboardKeys.All(key => key.Access == BlackboardAccess.PackageLocal), test.Name + " has non-local manifest key");
            Check(manifest.BlackboardKeys.All(key => key.Namespace == manifest.BlackboardNamespace), test.Name + " key namespace mismatch");
            Check(manifest.ActionIds.Any(id => id.Value.EndsWith(".action.commit-short-range", StringComparison.Ordinal)), test.Name + " missing ShortRange action");
            Check(manifest.ActionIds.Any(id => id.Value.EndsWith(".action.handle-death-and-corpse", StringComparison.Ordinal)), test.Name + " missing death/corpse action");
        }
    }

    private static void GoalPoliciesSelectLifecycleGoals()
    {
        foreach (PackageCase test in PackageCases())
        {
            IAvalonAiPackage package = test.Create();
            IAvalonGoalPolicy policy = package.GoalPolicies.Single();
            ActorSnapshot actor = new(Lease, package.Manifest.SupportedActorRoles.Single(), 10);

            Check(policy.Evaluate(new ActorSnapshot(new ActorLease(Lease.ActorId, Lease.LeaseId, ActorExecutionMode.NativeAssisted, Lease.Generation), actor.Role, 10), Blackboard(test, active: true)).Count == 0, test.Name + " accepted non-Blaze ownership");
            Check(policy.Evaluate(actor, Blackboard(test, active: true, role: "wrong-role")).Count == 0, test.Name + " accepted wrong role binding");

            GoalRequest follow = Single(policy.Evaluate(actor, Blackboard(test, active: true)));
            Check(follow.GoalId.Value.EndsWith(".goal.follow-hero", StringComparison.Ordinal), test.Name + " did not default to follow");

            GoalRequest attack = Single(policy.Evaluate(actor, Blackboard(test, active: true, targetValid: true, targetThreatening: true)));
            Check(attack.GoalId.Value.EndsWith(".goal.commit-short-range", StringComparison.Ordinal), test.Name + " did not choose ShortRange against threat");

            GoalRequest recover = Single(policy.Evaluate(actor, Blackboard(test, active: true, recovery: true)));
            Check(recover.GoalId.Value.EndsWith(".goal.recover", StringComparison.Ordinal), test.Name + " did not recover");

            GoalRequest death = Single(policy.Evaluate(actor, Blackboard(test, active: true, alive: false)));
            Check(death.GoalId.Value.EndsWith(".goal.death-retained-corpse", StringComparison.Ordinal), test.Name + " did not select death/corpse");

            GoalRequest stop = Single(policy.Evaluate(actor, Blackboard(test, active: true, killSwitch: true)));
            Check(stop.GoalId.Value.EndsWith(".goal.stop-safely", StringComparison.Ordinal), test.Name + " did not stop safely on kill switch");
        }
    }

    private static void ActionProcedureContractsAreBounded()
    {
        foreach (PackageCase test in PackageCases())
        {
            IAvalonAiPackage package = test.Create();
            var actions = package.ActionDefinitions.ToDictionary(action => action.Id.Value, action => action, StringComparer.Ordinal);

            AvalonActionDefinition attack = actions.Single(pair => pair.Key.EndsWith(".action.commit-short-range", StringComparison.Ordinal)).Value;
            Check(attack.ExecutionProfile.Mode == AvalonExecutionMode.Procedure, test.Name + " attack must be a procedure request");
            Check(attack.RequiredCapability.Value.EndsWith(".capability.short-range-16", StringComparison.Ordinal), test.Name + " attack capability changed");
            Check(attack.ExecutionProfile.Procedure?.RequiredCapabilities.Contains(attack.RequiredCapability) == true, test.Name + " attack procedure omits action capability");
            Check(attack.CostProvider.Evaluate(CostContext(attack.Id)) >= (test.DamageTimingProven ? 1f : 4f), test.Name + " damage-timing cost guard changed");

            AvalonActionDefinition death = actions.Single(pair => pair.Key.EndsWith(".action.handle-death-and-corpse", StringComparison.Ordinal)).Value;
            Check(death.ExecutionProfile.Procedure?.Id.Value.EndsWith(".procedure.death-44-corpse.v1", StringComparison.Ordinal) == true, test.Name + " death procedure changed");

            foreach (AvalonActionDefinition action in package.ActionDefinitions)
            {
                Check(action.Timeout > TimeSpan.Zero, test.Name + " action timeout invalid");
                Check(action.ExecutionProfile.Mode == AvalonExecutionMode.Procedure, test.Name + " action is not procedure-bound: " + action.Id.Value);
            }
        }
    }

    private static void AssemblyBoundariesAreContractsOnly()
    {
        foreach (PackageCase test in PackageCases())
        {
            Assembly assembly = test.PackageType.Assembly;
            string[] references = assembly.GetReferencedAssemblies().Select(reference => reference.Name ?? string.Empty).ToArray();
            Check(references.Contains("AvalonAI.Contracts.V2"), test.Name + " does not reference Contracts V2");

            string[] forbidden =
            {
                "AvalonAI.Runtime",
                "AvalonAI.Blackboard",
                "AvalonAI.Planning",
                "AvalonAI.Execution",
                "AvalonAI.FoAHost",
                "AvalonAwakened",
                "AvalonBullCompanion",
                "AvalonDragonCompanion",
                "AvalonElephantCompanion",
                "AvalonGoblinCompanion",
                "AvalonVampireCompanion",
                "Unity",
                "BepInEx",
                "Harmony",
                "Awaken",
                "TG.Main",
                "HutongGames",
                "PlayMaker",
            };

            foreach (string forbiddenName in forbidden)
            {
                Check(!references.Any(reference => reference.Contains(forbiddenName, StringComparison.OrdinalIgnoreCase)), test.Name + " references forbidden assembly: " + forbiddenName);
            }
        }
    }

    private static ActionCostContext CostContext(ActionId action) =>
        new(Lease, 10, action, new AvalonPosition(0f, 0f, 0f), Array.Empty<PlanningFact>(), null);

    private static GoalRequest Single(IReadOnlyList<GoalRequest> requests)
    {
        Check(requests.Count == 1, "expected exactly one goal request but got " + requests.Count);
        return requests[0];
    }

    private static FixtureBlackboard Blackboard(
        PackageCase test,
        bool active,
        string? role = null,
        bool lease = true,
        bool alive = true,
        bool saveExcluded = true,
        bool targetValid = false,
        bool targetThreatening = false,
        bool recovery = false,
        bool killSwitch = false,
        bool cleanup = false)
    {
        string ns = "avalon_" + test.CreatureId.Replace("-", "_", StringComparison.Ordinal) + "_companion.ai.v2";
        return new FixtureBlackboard()
            .Set(new BlackboardKey<string>(ns, "role.bound_id", 2, BlackboardAccess.PackageLocal, BlackboardScope.Actor), role ?? test.RoleId)
            .Set(new BlackboardKey<bool>(ns, "lifecycle.active", 2, BlackboardAccess.PackageLocal, BlackboardScope.Actor), active)
            .Set(new BlackboardKey<bool>(ns, "lifecycle.cleanup_requested", 2, BlackboardAccess.PackageLocal, BlackboardScope.Actor), cleanup)
            .Set(new BlackboardKey<bool>(ns, "lease.valid", 2, BlackboardAccess.PackageLocal, BlackboardScope.Actor), lease)
            .Set(new BlackboardKey<bool>(ns, "actor.alive", 2, BlackboardAccess.PackageLocal, BlackboardScope.Actor), alive)
            .Set(new BlackboardKey<bool>(ns, "actor.save_excluded", 2, BlackboardAccess.PackageLocal, BlackboardScope.Actor), saveExcluded)
            .Set(new BlackboardKey<bool>(ns, "target.valid", 2, BlackboardAccess.PackageLocal, BlackboardScope.Actor), targetValid)
            .Set(new BlackboardKey<bool>(ns, "target.threatening", 2, BlackboardAccess.PackageLocal, BlackboardScope.Actor), targetThreatening)
            .Set(new BlackboardKey<bool>(ns, "action.recovery_required", 2, BlackboardAccess.PackageLocal, BlackboardScope.Actor), recovery)
            .Set(new BlackboardKey<bool>(ns, "movement.stuck", 2, BlackboardAccess.PackageLocal, BlackboardScope.Actor), false)
            .Set(new BlackboardKey<bool>(ns, "kill_switch.active", 2, BlackboardAccess.PackageLocal, BlackboardScope.Actor), killSwitch);
    }

    private static IReadOnlyList<PackageCase> PackageCases() => new[]
    {
        new PackageCase(
            "Bull",
            "fantasy-bull",
            "kane.tgfoa.avalon-bull-companion.ai-companion.v2",
            "Avalon Bull Companion AI",
            "avalon-bull-companion.role.fantasy-bull",
            true,
            typeof(AvalonBullCompanionAiPackageV2),
            () => new AvalonBullCompanionAiPackageV2()),
        new PackageCase(
            "Dragon",
            "fantasy-dragon-grounded",
            "kane.tgfoa.avalon-dragon-companion.ai-companion.v2",
            "Avalon Dragon Companion AI",
            "avalon-dragon-companion.role.fantasy-dragon-grounded",
            true,
            typeof(AvalonDragonCompanionAiPackageV2),
            () => new AvalonDragonCompanionAiPackageV2()),
        new PackageCase(
            "Elephant",
            "fantasy-elephant",
            "kane.tgfoa.avalon-elephant-companion.ai-companion.v2",
            "Avalon Elephant Companion AI",
            "avalon-elephant-companion.role.fantasy-elephant",
            true,
            typeof(AvalonElephantCompanionAiPackageV2),
            () => new AvalonElephantCompanionAiPackageV2()),
        new PackageCase(
            "Goblin",
            "goblin",
            "kane.tgfoa.avalon-goblin-companion.ai-companion.v2",
            "Avalon Goblin Companion AI",
            "avalon-goblin-companion.role.goblin",
            false,
            typeof(AvalonGoblinCompanionAiPackageV2),
            () => new AvalonGoblinCompanionAiPackageV2()),
        new PackageCase(
            "Vampire",
            "vampire",
            "kane.tgfoa.avalon-vampire-companion.ai-companion.v2",
            "Avalon Vampire Companion AI",
            "avalon-vampire-companion.role.vampire",
            true,
            typeof(AvalonVampireCompanionAiPackageV2),
            () => new AvalonVampireCompanionAiPackageV2()),
    };

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class PackageCase
    {
        public PackageCase(
            string name,
            string creatureId,
            string packageId,
            string displayName,
            string roleId,
            bool damageTimingProven,
            Type packageType,
            Func<IAvalonAiPackage> create)
        {
            Name = name;
            CreatureId = creatureId;
            PackageId = packageId;
            DisplayName = displayName;
            RoleId = roleId;
            DamageTimingProven = damageTimingProven;
            PackageType = packageType;
            Create = create;
        }

        public string Name { get; }
        public string CreatureId { get; }
        public string PackageId { get; }
        public string DisplayName { get; }
        public string RoleId { get; }
        public bool DamageTimingProven { get; }
        public Type PackageType { get; }
        public Func<IAvalonAiPackage> Create { get; }
    }

    private sealed class FixtureBlackboard : IAvalonBlackboardReader
    {
        private readonly Dictionary<BlackboardKeyDeclaration, object> values = new();

        public FixtureBlackboard Set<T>(BlackboardKey<T> key, T value)
        {
            values[key.Declaration] = value!;
            return this;
        }

        public bool TryRead<T>(BlackboardKey<T> key, out T value)
        {
            if (values.TryGetValue(key.Declaration, out object? stored) && stored is T typed)
            {
                value = typed;
                return true;
            }

            value = default!;
            return false;
        }

        public IDisposable Observe<T>(BlackboardKey<T> key, Action<BlackboardChange<T>> callback)
        {
            _ = key.Declaration;
            _ = callback ?? throw new ArgumentNullException(nameof(callback));
            return NoOpDisposable.Instance;
        }
    }

    private sealed class NoOpDisposable : IDisposable
    {
        public static NoOpDisposable Instance { get; } = new();
        public void Dispose() { }
    }
}
