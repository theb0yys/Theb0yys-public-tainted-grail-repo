using System;
using System.Globalization;
using System.IO;
using Awaken.TG.Main.Fights.Factions.Crimes;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGCommunity.Example.CrimeSemanticSidecar;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.crime-semantic-sidecar";
    public const string PluginName = "TG Example - Crime Semantic Sidecar";
    public const string PluginVersion = "0.1.0";

    private static ConfigEntry<float>? _bountyMultiplier;
    private static string _logPath = string.Empty;
    private Harmony? _harmony;

    private void Awake()
    {
        _bountyMultiplier = Config.Bind(
            "Crime",
            "BountyMultiplier",
            1f,
            "Scale only the incoming CrimeUtils.AddBounty value. 1.0 leaves native bounty unchanged.");

        string root = Path.Combine(Paths.ConfigPath, PluginGuid);
        Directory.CreateDirectory(root);
        _logPath = Path.Combine(root, "crime-incidents.csv");

        if (!File.Exists(_logPath))
        {
            File.WriteAllText(
                _logPath,
                "utc,owner,bounty_before,bounty_after,native_delta,configured_multiplier\n");
        }

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(CrimeUtils), nameof(CrimeUtils.AddBounty))]
    private static class AddBountyPatch
    {
        private static void Prefix(
            CrimeOwnerTemplate template,
            ref float value,
            out CrimeState __state)
        {
            float before = CrimeUtils.Bounty(template);
            float multiplier = Math.Max(0f, Math.Min(10f, _bountyMultiplier?.Value ?? 1f));

            __state = new CrimeState(before, value, multiplier);

            if (Math.Abs(multiplier - 1f) > 0.0001f)
            {
                value *= multiplier;
            }
        }

        private static void Postfix(
            CrimeOwnerTemplate template,
            CrimeState __state)
        {
            float after = CrimeUtils.Bounty(template);
            string owner = template?.GUID ?? template?.name ?? "<unknown>";

            string row = string.Join(
                ",",
                Quote(DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)),
                Quote(owner),
                __state.Before.ToString("0.###", CultureInfo.InvariantCulture),
                after.ToString("0.###", CultureInfo.InvariantCulture),
                (after - __state.Before).ToString("0.###", CultureInfo.InvariantCulture),
                __state.Multiplier.ToString("0.###", CultureInfo.InvariantCulture));

            File.AppendAllText(_logPath, row + Environment.NewLine);
        }
    }

    private static string Quote(string value)
    {
        return """ + (value ?? string.Empty).Replace(""", """") + """;
    }

    private readonly struct CrimeState
    {
        internal CrimeState(float before, float incoming, float multiplier)
        {
            Before = before;
            Incoming = incoming;
            Multiplier = multiplier;
        }

        internal float Before { get; }
        internal float Incoming { get; }
        internal float Multiplier { get; }
    }
}
