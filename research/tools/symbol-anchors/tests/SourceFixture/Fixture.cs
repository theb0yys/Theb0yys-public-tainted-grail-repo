using HarmonyLib;

namespace Fixture;

internal sealed class FakeOwner
{
    internal void Alpha()
    {
    }

    internal bool Beta(int value, string text)
    {
        return value > 0 && text.Length > 0;
    }

    internal int Value { get; set; }
}

[HarmonyPatch(typeof(FakeOwner), nameof(FakeOwner.Alpha))]
internal static class AlphaPatch
{
}

internal static class DynamicTargets
{
    internal static void Resolve()
    {
        AccessTools.Method(
            typeof(FakeOwner),
            nameof(FakeOwner.Beta),
            new[] { typeof(int), typeof(string) });

        AccessTools.PropertyGetter(typeof(FakeOwner), "Value");
        AccessTools.PropertySetter(typeof(FakeOwner), "Value");
    }
}
