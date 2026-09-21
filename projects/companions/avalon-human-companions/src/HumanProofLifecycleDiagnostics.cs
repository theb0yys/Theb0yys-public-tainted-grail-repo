using System;
using System.Globalization;
using System.IO;
using System.Text;
using Awaken.TG.Main.AI.SummonsAndAllies;
using Awaken.TG.Main.Fights.NPCs;
using Awaken.TG.Main.Heroes;
using Awaken.TG.Main.Locations;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace AvalonHumanCompanions;

internal static class HumanProofLifecycleDiagnostics
{
    private const string FileName = "human-proof-lifecycle.csv";

    internal static void WriteEvent(
        ManualLogSource logger,
        string eventName,
        string reason,
        Location? location,
        bool tracked,
        string mode)
    {
        try
        {
            string folder = Path.Combine(Paths.ConfigPath, Plugin.PluginGuid);
            Directory.CreateDirectory(folder);

            string path = Path.Combine(folder, FileName);
            bool writeHeader = !File.Exists(path);
            StringBuilder builder = new StringBuilder();
            if (writeHeader)
            {
                builder.AppendLine("localTime,eventName,reason,tracked,locationPresent,locationDiscarded,locationId,debugName,templateGuid,templateName,coords,heroAvailable,heroCoords,distanceFromHero,markedNotSaved,hasNpcElement,npcIsUnique,hasHeroPetAlly,mode,behaviorApproved,persistenceApproved,notes");
            }

            Hero? hero = Hero.Current;
            bool heroAvailable = hero != null;
            string heroCoords = hero == null ? string.Empty : FormatVector(hero.Coords);
            string distance = string.Empty;
            bool locationPresent = location != null;
            bool locationDiscarded = location?.HasBeenDiscarded ?? false;
            bool markedNotSaved = false;
            bool hasNpcElement = false;
            string npcIsUnique = "unknown";
            bool hasHeroPetAlly = false;

            if (location != null)
            {
                markedNotSaved = location.MarkedNotSaved;
                if (hero != null)
                {
                    distance = FormatFloat(Vector3.Distance(location.Coords, hero.Coords));
                }

                try
                {
                    if (location.TryGetElement(out NpcElement npcElement) && npcElement != null)
                    {
                        hasNpcElement = true;
                        npcIsUnique = npcElement.IsUnique ? "true" : "false";
                        NpcHeroPetAlly marker = npcElement.TryGetElement<NpcHeroPetAlly>();
                        hasHeroPetAlly = marker != null && !marker.HasBeenDiscarded;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"{Plugin.PluginName} lifecycle diagnostics could not inspect proof actor elements: {ex.GetType().Name}: {ex.Message}");
                }
            }

            AppendCsv(builder, DateTimeOffset.Now.ToString("O", CultureInfo.InvariantCulture));
            AppendCsv(builder, eventName);
            AppendCsv(builder, reason);
            AppendCsv(builder, tracked ? "true" : "false");
            AppendCsv(builder, locationPresent ? "true" : "false");
            AppendCsv(builder, locationDiscarded ? "true" : "false");
            AppendCsv(builder, location?.ID ?? string.Empty);
            AppendCsv(builder, location?.DebugName ?? string.Empty);
            AppendCsv(builder, location?.Template?.GUID ?? string.Empty);
            AppendCsv(builder, location?.Template?.name ?? string.Empty);
            AppendCsv(builder, location == null ? string.Empty : FormatVector(location.Coords));
            AppendCsv(builder, heroAvailable ? "true" : "false");
            AppendCsv(builder, heroCoords);
            AppendCsv(builder, distance);
            AppendCsv(builder, markedNotSaved ? "true" : "false");
            AppendCsv(builder, hasNpcElement ? "true" : "false");
            AppendCsv(builder, npcIsUnique);
            AppendCsv(builder, hasHeroPetAlly ? "true" : "false");
            AppendCsv(builder, mode);
            AppendCsv(builder, "false");
            AppendCsv(builder, "false");
            AppendCsv(builder, "Lifecycle evidence only; no recruitment, persistence, existing-NPC conversion, save safety, dialogue, quest, or release-ready UI approval.");
            builder.Length--;
            builder.AppendLine();

            File.AppendAllText(path, builder.ToString(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            logger.LogWarning($"{Plugin.PluginName} lifecycle diagnostics failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static string FormatVector(Vector3 value)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0:0.###}|{1:0.###}|{2:0.###}", value.x, value.y, value.z);
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static void AppendCsv(StringBuilder builder, string value)
    {
        value = value.Replace("\r", " ").Replace("\n", " ");
        builder.Append('"');
        builder.Append(value.Replace("\"", "\"\""));
        builder.Append('"');
        builder.Append(',');
    }
}
