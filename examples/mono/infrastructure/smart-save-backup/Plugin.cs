using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Awaken.TG.Main.Saving.Cloud.Services;
using BepInEx;
using HarmonyLib;

namespace TGCommunity.Example.SmartSaveBackup;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "community.taintedgrail.example.smart-save-backup";
    public const string PluginName = "TG Example - Smart Save Backup";
    public const string PluginVersion = "0.1.0";

    private static readonly ConcurrentQueue<string> Completed = new();
    private string _backupRoot = string.Empty;
    private Harmony? _harmony;

    private void Awake()
    {
        _backupRoot = Path.Combine(Paths.ConfigPath, PluginGuid, "backups");
        Directory.CreateDirectory(_backupRoot);

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{PluginName} loaded. BackupRoot={_backupRoot}");
    }

    private void Update()
    {
        while (Completed.TryDequeue(out string slotId))
            TryCreateBackup(slotId);
    }

    private void OnDestroy() => _harmony?.UnpatchSelf();

    private void TryCreateBackup(string slotId)
    {
        string safeSlot = Sanitize(slotId);
        string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        string finalPath = Path.Combine(_backupRoot, $"{stamp}-{safeSlot}.zip");
        string tempPath = finalPath + ".tmp";

        try
        {
            int entries = 0;
            CloudService.Get.BeginLoadSlot(slotId);
            try
            {
                using ZipArchive archive = ZipFile.Open(tempPath, ZipArchiveMode.Create);
                foreach (string entryName in CloudService.Get.EnumerateFilesInSlot().ToArray())
                {
                    if (!CloudService.Get.TryLoadSlotFile(entryName, out byte[] data) || data == null)
                        continue;

                    ZipArchiveEntry entry = archive.CreateEntry(entryName + ".data", CompressionLevel.Optimal);
                    using Stream stream = entry.Open();
                    stream.Write(data, 0, data.Length);
                    entries++;
                }
            }
            finally
            {
                CloudService.Get.EndLoadSlot(slotId);
            }

            if (entries == 0)
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
                Logger.LogWarning($"Backup skipped for {slotId}: no readable slot entries.");
                return;
            }

            File.Move(tempPath, finalPath);
            Logger.LogInfo($"Backup created. slotId={slotId}; entries={entries}; file={Path.GetFileName(finalPath)}");
        }
        catch (Exception ex)
        {
            try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
            Logger.LogWarning($"Backup failed for {slotId}: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static string Sanitize(string value)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        return new string((value ?? "slot").Select(c => invalid.Contains(c) ? '_' : c).ToArray());
    }

    [HarmonyPatch]
    private static class EndSavePatch
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
                System.Type? type = AccessTools.TypeByName(typeName);
                MethodInfo? method = type == null ? null : AccessTools.Method(type, "EndSave", new[] { typeof(string) });
                if (method != null) yield return method;
            }
        }

        private static void Postfix(string slotId)
        {
            if (!string.IsNullOrWhiteSpace(slotId))
                Completed.Enqueue(slotId);
        }
    }
}
