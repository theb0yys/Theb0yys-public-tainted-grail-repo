using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AvalonCompanions.AI.Owner.Contracts;

internal static class Program
{
    private static int Main()
    {
        var fixtures = new (string Name, Action Run)[]
        {
            ("owner observation is an exact scalar snapshot", ObservationIsExact),
            ("owner boundary exposes the five reviewed operations", BoundaryShapeIsExact),
            ("owner contracts remain BCL only", AssemblyBoundaryIsBclOnly),
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
            Console.WriteLine("All " + fixtures.Length + " companion owner contract fixtures passed.");
            return 0;
        }

        Console.WriteLine(failures.Count + " fixture(s) failed:");
        foreach (var failure in failures)
        {
            Console.WriteLine(" - " + failure);
        }

        return 1;
    }

    private static void ObservationIsExact()
    {
        var observation = new AvalonCompanionOwnerObservation(41, "foa-location-41", true, false);

        Check(observation.Sequence == 41, "The retained observation sequence changed.");
        Check(observation.ActorRuntimeId == "foa-location-41", "The exact FoA runtime actor ID changed.");
        Check(observation.HasActiveCompanion, "The active-companion flag changed.");
        Check(!observation.FollowCatchUpCandidate, "The catch-up candidate flag changed.");
        Check(
            Enum.GetValues(typeof(AvalonCompanionOwnerCommand)).Cast<AvalonCompanionOwnerCommand>().SequenceEqual(
                new[] { AvalonCompanionOwnerCommand.CatchUpRecall }),
            "The owner contract must expose only the reviewed catch-up command.");
    }

    private static void BoundaryShapeIsExact()
    {
        var methods = typeof(IAvalonCompanionAiOwnerBoundary)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ToArray();

        Check(methods.Length == 5, "The owner boundary must expose exactly five operations.");
        AssertMethod(methods, "TryAcquire", typeof(bool), typeof(string), typeof(string).MakeByRefType());
        AssertMethod(methods, "TryCollect", typeof(bool), typeof(string), typeof(AvalonCompanionOwnerObservation).MakeByRefType());
        AssertMethod(methods, "TryDispatch", typeof(bool), typeof(string), typeof(long), typeof(AvalonCompanionOwnerCommand));
        AssertMethod(methods, "TryInspectActorId", typeof(bool), typeof(string), typeof(string).MakeByRefType());
        AssertMethod(methods, "TryRelease", typeof(bool), typeof(string));
    }

    private static void AssemblyBoundaryIsBclOnly()
    {
        var assembly = typeof(IAvalonCompanionAiOwnerBoundary).Assembly;
        var exported = assembly.GetExportedTypes().OrderBy(type => type.FullName, StringComparer.Ordinal).ToArray();
        Check(exported.Length == 3, "The contract assembly must export only the command, observation, and boundary.");

        var references = assembly.GetReferencedAssemblies().Select(reference => reference.Name ?? string.Empty).ToArray();
        Check(!references.Any(IsForbiddenReference), "The owner contract assembly must reference the BCL only.");

        foreach (var type in exported)
        {
            Check(
                type.Namespace == "AvalonCompanions.AI.Owner.Contracts",
                "Every public owner-contract type must remain in the stable contract namespace.");
        }
    }

    private static void AssertMethod(
        IReadOnlyList<MethodInfo> methods,
        string name,
        Type returnType,
        params Type[] parameterTypes)
    {
        var method = methods.Single(candidate => candidate.Name == name);
        Check(method.ReturnType == returnType, name + " has the wrong return type.");
        Check(
            method.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(parameterTypes),
            name + " has the wrong parameters.");
    }

    private static bool IsForbiddenReference(string name)
    {
        return name.StartsWith("AvalonAI", StringComparison.Ordinal)
            || name == "AvalonCompanions"
            || name.StartsWith("Unity", StringComparison.Ordinal)
            || name.StartsWith("BepInEx", StringComparison.Ordinal)
            || name == "TG.Main"
            || name.StartsWith("CrashKonijn", StringComparison.Ordinal)
            || name.StartsWith("Blackboard", StringComparison.Ordinal)
            || name.StartsWith("Blaze", StringComparison.Ordinal);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
