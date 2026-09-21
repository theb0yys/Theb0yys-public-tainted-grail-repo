using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AvalonAI.Contracts.V2;
using AvalonAI.Runtime.V2;
using AvalonCompanions.AI.Package.V2;

internal static class Program
{
    private static int Main()
    {
        var fixtures = new (string Name, Action Run)[]
        {
            ("manifest declares the reviewed V2 boundary", ManifestIsExact),
            ("policy requests catch-up only for the authoritative true fact", PolicyIsBounded),
            ("goal and action encode the catch-up state transition", PlanningDefinitionsAreExact),
            ("production package registers with the V2 Runtime", ProductionPackageRegisters),
            ("package assembly remains Contracts-only", AssemblyBoundaryIsGuarded),
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
            Console.WriteLine("All " + fixtures.Length + " AvalonCompanions API 2.0 package fixtures passed.");
            return 0;
        }

        Console.WriteLine(failures.Count + " fixture(s) failed:");
        foreach (var failure in failures)
        {
            Console.WriteLine(" - " + failure);
        }

        return 1;
    }

    private static void ManifestIsExact()
    {
        var package = new AvalonCompanionsAiPackageV2();
        var manifest = package.Manifest;

        Check(manifest.Id == AvalonCompanionCatchUpV2Contract.PackageId, "The logical package ID must remain stable across source migration.");
        Check(manifest.RequiredRuntimeApi == AvalonAiContracts.ApiVersion, "The package must require Contracts API 2.0.");
        Check(manifest.PackageVersion == AvalonCompanionsAiPackageV2.PackageVersion, "The production package version must be explicit.");
        Check(manifest.GoalIds.SequenceEqual(new[] { AvalonCompanionCatchUpV2Contract.GoalId }), "Only the reviewed catch-up goal may be declared.");
        Check(manifest.ActionIds.SequenceEqual(new[] { AvalonCompanionCatchUpV2Contract.ActionId }), "Only the reviewed catch-up action may be declared.");
        Check(manifest.RequiredCapabilities.SequenceEqual(new[] { AvalonCompanionCatchUpV2Contract.Capability }), "Only the narrow native catch-up capability may be requested.");
        Check(manifest.SupportedActorRoles.SequenceEqual(new[] { AvalonCompanionCatchUpV2Contract.ActorRoleId }), "Only the managed companion role may be supported.");
        Check(manifest.BlackboardKeys.Count == 0 && manifest.PersistentKeys.Count == 0, "The package must not claim authoritative or persistent keys.");
        Check(manifest.MaximumPolicyCadence == TimeSpan.FromSeconds(1), "The package must retain the reviewed one-second profile cadence.");

        var key = AvalonCompanionCatchUpV2Contract.FollowCatchUpCandidate;
        Check(key.Access == BlackboardAccess.Authoritative && key.Scope == BlackboardScope.Actor, "The catch-up candidate fact must remain an actor observation.");
    }

    private static void PolicyIsBounded()
    {
        var policy = new AvalonCompanionCatchUpGoalPolicy();
        var actor = Snapshot(AvalonCompanionCatchUpV2Contract.ActorRoleId);

        Check(policy.Evaluate(actor, new FixtureBlackboard()).Count == 0, "A missing authoritative fact must fail closed.");
        Check(policy.Evaluate(actor, new FixtureBlackboard(false)).Count == 0, "A false authoritative fact must not request catch-up.");
        Check(
            policy.Evaluate(Snapshot(new ActorRoleId("fixture.not-a-companion")), new FixtureBlackboard(true)).Count == 0,
            "A different actor role must not request catch-up.");

        var requests = policy.Evaluate(actor, new FixtureBlackboard(true));
        Check(requests.Count == 1, "An exact true companion fact must request one goal.");
        Check(requests[0].GoalId == AvalonCompanionCatchUpV2Contract.GoalId, "The policy must request only the catch-up goal.");
        Check(requests[0].BasePriority == 0 && requests[0].Urgency == 0, "The source migration must not invent cross-package priority tuning.");
        Check(requests[0].MinimumCommitment == TimeSpan.Zero, "The one-shot request must not invent a goal lock.");
        Check(requests[0].ReasonCode == AvalonCompanionCatchUpV2Contract.GoalReasonCode, "The goal request must expose a stable reason.");
    }

    private static void PlanningDefinitionsAreExact()
    {
        var package = new AvalonCompanionsAiPackageV2();
        Check(package.GoalPolicies.Count == 1, "The package must expose one bounded policy.");
        Check(package.GoalDefinitions.Count == 1, "The package must expose one goal definition.");
        Check(package.ActionDefinitions.Count == 1, "The package must expose one action definition.");

        var desired = package.GoalDefinitions[0].DesiredState.Single();
        Check(desired.FactId == AvalonCompanionCatchUpV2Contract.CatchUpRecallDispatchedFactId, "The goal must use the recall-dispatched planning fact.");
        Check(desired.Comparison == PlanningComparison.Equal && desired.Value == 1, "The goal must require one recall dispatch.");

        var action = package.ActionDefinitions[0];
        var condition = action.Conditions.Single();
        var effect = action.Effects.Single();
        Check(action.Id == AvalonCompanionCatchUpV2Contract.ActionId, "The action ID must match the reviewed executor boundary.");
        Check(condition.FactId == AvalonCompanionCatchUpV2Contract.CatchUpRequiredFactId && condition.Value == 1, "The action must require catch-up.");
        Check(effect.FactId == AvalonCompanionCatchUpV2Contract.CatchUpRecallDispatchedFactId && effect.AssignedValue == 1, "The action must satisfy the recall-dispatch goal.");
        Check(action.TargetKeyId.IsEmpty, "The native recall action must remain targetless.");
        Check(action.RequiredCapability == AvalonCompanionCatchUpV2Contract.Capability, "The action must request only the reviewed native capability.");
        Check(action.InterruptPolicy == InterruptPolicy.OnGoalChange, "A changed goal must be able to interrupt dispatch.");
        Check(action.Timeout == TimeSpan.FromSeconds(1), "The action timeout must remain within the reviewed profile cadence.");
        Check(action.CostProvider is ConstantActionCostProvider cost && cost.Cost == 1, "The single action must retain a neutral unit cost.");
    }

    private static void ProductionPackageRegisters()
    {
        var runtime = new AvalonAiRuntimeV2(
            new NoObservationSource(),
            new NoBlackboardBackend(),
            new NoPlanningBackend(),
            Array.Empty<AvalonExecutorBinding>(),
            new[] { AvalonCompanionCatchUpV2Contract.Capability },
            new AvalonAiRuntimeOptions(
                maximumActorObservationsPerTick: 1,
                maximumDerivedUpdatesPerTick: 1,
                maximumGoalPolicyEvaluationsPerTick: 1,
                maximumPlanResolutionsPerTick: 1,
                maximumGoalsPerPlan: 1,
                maximumPendingActionsPerActor: 1,
                maximumPollOperationsPerTick: 1,
                packageExceptionThreshold: 1,
                minimumReplanInterval: TimeSpan.Zero,
                contextIdleEviction: TimeSpan.FromMinutes(1),
                globalTimeBudget: TimeSpan.FromSeconds(1),
                perActorPlanningBudget: new PlanningBudget(TimeSpan.FromMilliseconds(10), 16)),
            new FixtureClock());

        var result = runtime.RegisterPackage(new AvalonCompanionsAiPackageV2());
        Check(result.Accepted, "The production package must pass Runtime V2 registration.");
        Check(result.PackageId == AvalonCompanionCatchUpV2Contract.PackageId, "Registration must preserve the exact package ID.");
    }

    private static void AssemblyBoundaryIsGuarded()
    {
        var assembly = typeof(AvalonCompanionsAiPackageV2).Assembly;
        var references = assembly.GetReferencedAssemblies().Select(reference => reference.Name ?? string.Empty).ToArray();

        Check(references.Contains("AvalonAI.Contracts.V2"), "The package must reference Contracts V2.");
        Check(!references.Any(name => name.StartsWith("AvalonAI.Runtime", StringComparison.Ordinal)), "The package must not reference the Runtime implementation.");
        Check(!references.Any(name => name.StartsWith("AvalonAI.Blackboard", StringComparison.Ordinal)), "The package must not reference Rabbit.");
        Check(!references.Any(name => name.StartsWith("AvalonAI.Planning", StringComparison.Ordinal)), "The package must not reference GOAP.");
        Check(!references.Any(name => name.StartsWith("AvalonAI.Execution", StringComparison.Ordinal)), "The package must not reference an executor.");
        Check(!references.Contains("AvalonCompanions"), "The Contracts-only package must not reference the live companion owner assembly.");
        Check(!references.Any(name => name.StartsWith("Unity", StringComparison.Ordinal) || name == "BepInEx" || name.StartsWith("0Harmony", StringComparison.Ordinal)), "The package must not reference game or loader assemblies.");

        foreach (var type in assembly.GetExportedTypes())
        {
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                Check(!MentionsForbiddenType(member), "The public package surface must expose only package, Contracts V2, and BCL types.");
            }
        }
    }

    private static ActorSnapshot Snapshot(ActorRoleId role)
    {
        return new ActorSnapshot(
            new ActorLease(
                new ActorId("fixture.companion"),
                new OwnershipLeaseId("fixture.lease"),
                ActorExecutionMode.NativeAssisted,
                1),
            role,
            1);
    }

    private static bool MentionsForbiddenType(MemberInfo member)
    {
        IEnumerable<Type> types = member switch
        {
            MethodInfo method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType),
            ConstructorInfo constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType),
            PropertyInfo property => new[] { property.PropertyType },
            FieldInfo field => new[] { field.FieldType },
            EventInfo eventInfo when eventInfo.EventHandlerType is not null => new[] { eventInfo.EventHandlerType },
            _ => Array.Empty<Type>(),
        };

        return types.Any(type =>
            type.Namespace is not null
            && (type.Namespace.StartsWith("Unity", StringComparison.Ordinal)
                || type.Namespace.StartsWith("BepInEx", StringComparison.Ordinal)
                || type.Namespace.StartsWith("Harmony", StringComparison.Ordinal)
                || type.Namespace.StartsWith("Awaken", StringComparison.Ordinal)));
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class FixtureBlackboard : IAvalonBlackboardReader
    {
        private readonly bool hasValue;
        private readonly bool value;

        public FixtureBlackboard()
        {
        }

        public FixtureBlackboard(bool value)
        {
            hasValue = true;
            this.value = value;
        }

        public bool TryRead<T>(BlackboardKey<T> key, out T result)
        {
            if (hasValue
                && key.Equals(AvalonCompanionCatchUpV2Contract.FollowCatchUpCandidate)
                && typeof(T) == typeof(bool))
            {
                result = (T)(object)value;
                return true;
            }

            result = default!;
            return false;
        }

        public IDisposable Observe<T>(BlackboardKey<T> key, Action<BlackboardChange<T>> callback)
        {
            _ = key.Declaration;
            _ = callback ?? throw new ArgumentNullException(nameof(callback));
            return NoOpDisposable.Instance;
        }
    }

    private sealed class NoObservationSource : IAvalonObservationSource
    {
        public ObservationCollectionResult Collect(ActorLease lease)
        {
            _ = lease;
            return new ObservationCollectionResult(ObservationCollectionStatus.Unavailable, null, "fixture-no-observation");
        }
    }

    private sealed class NoBlackboardBackend : IAvalonBlackboardBackend
    {
        public BlackboardActorOpenResult OpenActor(ActorLease lease, ActorRoleId role) => throw new NotSupportedException();
        public ObservationCommitResult CommitObservation(ActorLease lease, ObservationBatch batch, int maximumDerivedUpdates) => throw new NotSupportedException();
        public IAvalonPackageBlackboard GetPackageView(ActorLease lease, AvalonAiPackageManifest package) => throw new NotSupportedException();
        public PlanningStateSnapshot CapturePlanningState(ActorLease lease) => throw new NotSupportedException();
        public void RevokePackage(PackageId packageId) { }
        public void ReleaseActor(ActorLease lease) { }
    }

    private sealed class NoPlanningBackend : IAvalonPlanningBackend
    {
        public PlanResolution Resolve(PlanningRequest request) => throw new NotSupportedException();
        public void Cancel(PlanHandle plan, PlanCancelReason reason) { }
    }

    private sealed class FixtureClock : IAvalonRuntimeClock
    {
        public DateTimeOffset UtcNow => new DateTimeOffset(2026, 7, 16, 20, 0, 0, TimeSpan.Zero);
    }

    private sealed class NoOpDisposable : IDisposable
    {
        public static NoOpDisposable Instance { get; } = new NoOpDisposable();
        public void Dispose() { }
    }
}
