using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Awaken.TG.Main.Saving.Cloud.Services;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace TGCommunity.Example.SaveObserverBackup;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.save-observer-backup";
    public const string PluginName = "TG Example - Save Observer and Backup";
    public const string PluginVersion = "0.1.0";

    private static readonly ConcurrentQueue<string> CompletedSlots = new();
    private ConfigEntry<bool> _backupCompletedSaves = null!;
    private string _backupRoot = string.Empty;
    private Harmony? _harmony;

    private void Awake()
    {
        _backupCompletedSaves = Config.Bind(
            "Backup",
            "BackupCompletedSaves",
            false,
            "Create a mod-owned ZIP after a concrete CloudService.EndSave(string) notification.");

        _backupRoot = Path.Combine(Paths.ConfigPath, PluginGuid, "backups");
        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(CloudServiceEndSavePatch).Assembly);

        Logger.LogInfo($"{PluginName} loaded. Backups={_backupCompletedSaves.Value}.");
    }

    private void Update()
    {
        while (CompletedSlots.TryDequeue(out string slotId))
        {
            Logger.LogInfo($"Native save completion observed: slotId={slotId}.");

            if (_backupCompletedSaves.Value)
            {
                TryBackupSlot(slotId);
            }
        }
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }

    internal static void NotifySaveCompleted(string slotId)
    {
        if (!string.IsNullOrWhiteSpace(slotId))
        {
            CompletedSlots.Enqueue(slotId);
        }
    }

    private void TryBackupSlot(string slotId)
    {
        Directory.CreateDirectory(_backupRoot);

        string safeSlot = SafeFileName(slotId);
        string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        string finalPath = Path.Combine(_backupRoot, $"{safeSlot}-{stamp}.zip");
        string tempPath = finalPath + ".tmp";
        int entries = 0;

        try
        {
            CloudService.Get.BeginLoadSlot(slotId);
            try
            {
                using FileStream stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                using var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: false);

                foreach (string entryName in CloudService.Get.EnumerateFilesInSlot())
                {
                    if (!CloudService.Get.TryLoadSlotFile(entryName, out byte[] data) || data == null)
                    {
                        continue;
                    }

                    string safeEntry = SafeArchiveEntry(entryName);
                    ZipArchiveEntry entry = archive.CreateEntry(safeEntry, CompressionLevel.Optimal);

                    using Stream output = entry.Open();
                    output.Write(data, 0, data.Length);
                    entries++;
                }
            }
            finally
            {
                CloudService.Get.EndLoadSlot(slotId);
            }

            if (entries == 0)
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                Logger.LogWarning($"Backup skipped: slot {slotId} produced no readable entries.");
                return;
            }

            File.Move(tempPath, finalPath);
            Logger.LogInfo($"Backup created: {finalPath}; entries={entries}.");
        }
        catch (Exception ex)
        {
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { }
            }

            Logger.LogWarning($"Backup failed for slot {slotId}: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static string SafeFileName(string value)
    {
        var invalid = new HashSet<char>(Path.GetInvalidFileNameChars());
        char[] chars = value.ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            if (invalid.Contains(chars[i]))
            {
                chars[i] = '_';
            }
        }

        string safe = new string(chars).Trim();
        return safe.Length == 0 ? "slot" : safe;
    }

    private static string SafeArchiveEntry(string value)
    {
        string safe = value.Replace('\\', '/').TrimStart('/');
        while (safe.Contains("../", StringComparison.Ordinal))
        {
            safe = safe.Replace("../", string.Empty);
        }

        return safe.Length == 0 ? "entry.bin" : safe;
    }

    [HarmonyPatch]
    private static class CloudServiceEndSavePatch
    {
        private static readonly string[] TypeNames =
        {
            "Awaken.TG.Main.Saving.Cloud.Services.SteamCloudService",
            "Awaken.TG.Main.Saving.Cloud.Services.SteamNoCloudService",
            "Awaken.TG.Main.Saving.Cloud.Services.DebugCloudService",
            "Awaken.TG.Main.Saving.Cloud.Services.GogCloudService"
        };

        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (string typeName in TypeNames)
            {
                Type? type = AccessTools.TypeByName(typeName);
                MethodInfo? method =
                    type == null ? null : AccessTools.Method(type, "EndSave", new[] { typeof(string) });

                if (method != null)
                {
                    yield return method;
                }
            }
        }

        private static void Postfix(string slotId)
        {
            NotifySaveCompleted(slotId);
        }
    }
}
