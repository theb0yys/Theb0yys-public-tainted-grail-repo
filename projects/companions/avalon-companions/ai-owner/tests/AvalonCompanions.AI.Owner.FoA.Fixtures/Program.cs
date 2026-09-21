using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AvalonCompanions.AI.Owner.Contracts;
using AvalonCompanions.AI.Owner.FoA;

internal static class Program
{
    private static int Main()
    {
        var fixtures = new (string Name, Action Run)[]
        {
            ("FoA bridge preserves exact owner lifecycle calls", OwnershipCallsAreExact),
            ("FoA bridge preserves exact runtime actor identity", ActorIdentityIsExact),
            ("FoA bridge preserves the reviewed observation", ObservationIsExact),
            ("FoA bridge dispatches only the reviewed catch-up command", DispatchIsBounded),
            ("FoA bridge exposes only owner contracts publicly", AssemblyBoundaryIsGuarded),
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
            Console.WriteLine("All " + fixtures.Length + " companion owner FoA bridge fixtures passed.");
            return 0;
        }

        Console.WriteLine(failures.Count + " fixture(s) failed:");
        foreach (var failure in failures)
        {
            Console.WriteLine(" - " + failure);
        }

        return 1;
    }

    private static void OwnershipCallsAreExact()
    {
        var systems = new FixtureGameSystems();
        var bridge = new DirectFoACompanionAiOwnerBoundary(systems);

        Check(bridge.TryAcquire("fixture.owner", out var reason), "The accepted owner acquisition must cross the bridge.");
        Check(reason == "fixture-acquired", "The exact acquisition reason must cross the bridge.");
        Check(bridge.TryRelease("fixture.owner"), "The accepted owner release must cross the bridge.");
        Check(systems.AcquireCalls == 1 && systems.ReleaseCalls == 1, "Owner lifecycle operations must run exactly once.");
        Check(systems.LastOwnerId == "fixture.owner", "The exact owner ID must cross every lifecycle call.");
    }

    private static void ObservationIsExact()
    {
        var systems = new FixtureGameSystems
        {
            Observation = new AvalonCompanionOwnerObservation(91, "foa-location-91", true, true),
        };
        var bridge = new DirectFoACompanionAiOwnerBoundary(systems);

        Check(bridge.TryCollect("fixture.owner", out var observation), "The available observation must cross the bridge.");
        Check(observation.Sequence == 91, "The retained sequence changed across the bridge.");
        Check(observation.ActorRuntimeId == "foa-location-91", "The exact actor runtime ID changed across the bridge.");
        Check(observation.HasActiveCompanion, "The active-companion flag changed across the bridge.");
        Check(observation.FollowCatchUpCandidate, "The catch-up candidate changed across the bridge.");
        Check(systems.CollectCalls == 1 && systems.LastOwnerId == "fixture.owner", "Collection must use the exact owner once.");
    }

    private static void ActorIdentityIsExact()
    {
        var systems = new FixtureGameSystems { ActorRuntimeId = "foa-location-identity" };
        var bridge = new DirectFoACompanionAiOwnerBoundary(systems);

        Check(bridge.TryInspectActorId("fixture.owner", out var actorRuntimeId), "The available runtime actor ID must cross the bridge.");
        Check(actorRuntimeId == "foa-location-identity", "The runtime actor ID changed across the bridge.");
        Check(systems.InspectCalls == 1 && systems.LastOwnerId == "fixture.owner", "Identity inspection must use the exact owner once.");
    }

    private static void DispatchIsBounded()
    {
        var systems = new FixtureGameSystems();
        var bridge = new DirectFoACompanionAiOwnerBoundary(systems);

        Check(
            bridge.TryDispatch("fixture.owner", 117, AvalonCompanionOwnerCommand.CatchUpRecall),
            "The reviewed catch-up command must cross the bridge.");
        Check(systems.DispatchCalls == 1, "The reviewed command must dispatch exactly once.");
        Check(systems.LastOwnerId == "fixture.owner" && systems.LastSequence == 117, "Dispatch identity changed across the bridge.");
        Check(systems.LastCommand == AvalonCompanionOwnerCommand.CatchUpRecall, "The catch-up command changed across the bridge.");

        Check(
            !bridge.TryDispatch("fixture.owner", 118, (AvalonCompanionOwnerCommand)99),
            "An undeclared command must fail closed.");
        Check(systems.DispatchCalls == 1, "An undeclared command must fail before the game-system boundary.");
    }

    private static void AssemblyBoundaryIsGuarded()
    {
        var assembly = typeof(DirectFoACompanionAiOwnerBoundary).Assembly;
        var references = assembly.GetReferencedAssemblies().Select(reference => reference.Name ?? string.Empty).ToArray();
        Check(references.Contains("AvalonCompanions.AI.Owner.Contracts"), "The FoA bridge must implement the stable owner contract.");
        Check(references.Contains("AvalonCompanions"), "The FoA bridge must bind the reviewed direct companion owner.");
        Check(!references.Any(name => name.StartsWith("AvalonAI", StringComparison.Ordinal)), "The FoA bridge must not depend on Avalon Runtime or packages.");
        Check(!references.Any(name => name.StartsWith("BepInEx", StringComparison.Ordinal)), "The bridge must not add its own loader binding.");
        Check(!references.Contains("TG.Main"), "The bridge must reach FoA only through the reviewed companion owner assembly.");

        foreach (var type in assembly.GetExportedTypes())
        {
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                Check(!MentionsGameType(member), "The public FoA bridge surface must expose only owner-contract and BCL types.");
            }
        }
    }

    private static bool MentionsGameType(MemberInfo member)
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
            && (type.Namespace == "AvalonCompanions.Framework"
                || type.Namespace.StartsWith("Awaken", StringComparison.Ordinal)
                || type.Namespace.StartsWith("BepInEx", StringComparison.Ordinal)
                || type.Namespace.StartsWith("Unity", StringComparison.Ordinal)));
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed class FixtureGameSystems : IFoACompanionAiGameSystems
    {
        public AvalonCompanionOwnerObservation Observation { get; set; }

        public string ActorRuntimeId { get; set; } = "foa-location-default";

        public int AcquireCalls { get; private set; }

        public int ReleaseCalls { get; private set; }

        public int CollectCalls { get; private set; }

        public int InspectCalls { get; private set; }

        public int DispatchCalls { get; private set; }

        public string LastOwnerId { get; private set; } = string.Empty;

        public long LastSequence { get; private set; }

        public AvalonCompanionOwnerCommand LastCommand { get; private set; }

        public bool TryAcquire(string ownerId, out string reason)
        {
            AcquireCalls++;
            LastOwnerId = ownerId;
            reason = "fixture-acquired";
            return true;
        }

        public bool TryRelease(string ownerId)
        {
            ReleaseCalls++;
            LastOwnerId = ownerId;
            return true;
        }

        public bool TryInspectActorId(string ownerId, out string actorRuntimeId)
        {
            InspectCalls++;
            LastOwnerId = ownerId;
            actorRuntimeId = ActorRuntimeId;
            return true;
        }

        public bool TryCollect(string ownerId, out AvalonCompanionOwnerObservation observation)
        {
            CollectCalls++;
            LastOwnerId = ownerId;
            observation = Observation;
            return true;
        }

        public bool TryDispatch(
            string ownerId,
            long observationSequence,
            AvalonCompanionOwnerCommand command)
        {
            DispatchCalls++;
            LastOwnerId = ownerId;
            LastSequence = observationSequence;
            LastCommand = command;
            return true;
        }
    }
}
