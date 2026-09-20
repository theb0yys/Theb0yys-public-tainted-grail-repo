using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace AvalonExceptions;

internal static class SupportBundleWriter
{
    internal static readonly string[] DefaultIncludedFiles =
    {
        "incident.json",
        "report.html",
        "report.md",
        "session.json",
        "mods.csv",
        "bepinex_dependency_errors.csv",
        "dependency_findings.csv",
        "dependency_api_findings.csv",
        "harmony_patch_findings.csv",
        "local_mod_preflight.csv",
        "breadcrumbs.csv",
        "log_excerpt_bepinex.txt",
        "log_excerpt_player.txt",
        "redaction_report.txt",
        "fix-first.txt",
        "copyable-summary.txt"
    };

    internal static SupportBundleResult TryCreate(string incidentFolder, ReportRedactor redactor)
    {
        if (string.IsNullOrWhiteSpace(incidentFolder))
        {
            return SupportBundleResult.Failed(string.Empty, "incident folder is unavailable");
        }

        try
        {
            string fullIncidentFolder = Path.GetFullPath(incidentFolder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!Directory.Exists(fullIncidentFolder))
            {
                return SupportBundleResult.Failed(Path.Combine(fullIncidentFolder, "support_bundle.zip"), "incident folder does not exist");
            }

            BundleFile[] files = ResolveBundleFiles(fullIncidentFolder);
            foreach (BundleFile file in files)
            {
                string text = File.ReadAllText(file.FullPath, Encoding.UTF8);
                if (redactor.WouldRedact(text))
                {
                    return SupportBundleResult.Failed(Path.Combine(fullIncidentFolder, "support_bundle.zip"), $"redaction safety scan failed for {file.EntryName}");
                }
            }

            string bundlePath = Path.Combine(fullIncidentFolder, "support_bundle.zip");
            string tempPath = bundlePath + ".tmp";
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            using (FileStream stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                foreach (BundleFile file in files)
                {
                    ZipArchiveEntry entry = archive.CreateEntry(file.EntryName, CompressionLevel.Optimal);
                    using Stream entryStream = entry.Open();
                    using FileStream sourceStream = File.OpenRead(file.FullPath);
                    sourceStream.CopyTo(entryStream);
                }
            }

            if (File.Exists(bundlePath))
            {
                File.Delete(bundlePath);
            }

            File.Move(tempPath, bundlePath);
            return SupportBundleResult.Created(bundlePath, files.Select(file => file.EntryName).ToArray());
        }
        catch (Exception ex)
        {
            string bundlePath = string.IsNullOrWhiteSpace(incidentFolder)
                ? string.Empty
                : Path.Combine(incidentFolder, "support_bundle.zip");
            return SupportBundleResult.Failed(bundlePath, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private static BundleFile[] ResolveBundleFiles(string fullIncidentFolder)
    {
        string incidentPrefix = fullIncidentFolder + Path.DirectorySeparatorChar;
        return DefaultIncludedFiles.Select(fileName =>
        {
            string fullPath = Path.GetFullPath(Path.Combine(fullIncidentFolder, fileName));
            if (!fullPath.StartsWith(incidentPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Bundle file escapes incident folder: {fileName}");
            }

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Required bundle file is missing: {fileName}", fullPath);
            }

            return new BundleFile(fullPath, fileName);
        }).ToArray();
    }

    private readonly struct BundleFile
    {
        internal BundleFile(string fullPath, string entryName)
        {
            FullPath = fullPath;
            EntryName = entryName;
        }

        internal string FullPath { get; }
        internal string EntryName { get; }
    }
}

internal readonly struct SupportBundleResult
{
    private SupportBundleResult(bool success, string bundlePath, string error, string[] includedFiles)
    {
        Success = success;
        BundlePath = bundlePath;
        Error = error;
        IncludedFiles = includedFiles;
    }

    internal bool Success { get; }
    internal string BundlePath { get; }
    internal string Error { get; }
    internal string[] IncludedFiles { get; }

    internal static SupportBundleResult Created(string bundlePath, string[] includedFiles)
    {
        return new SupportBundleResult(true, bundlePath, string.Empty, includedFiles);
    }

    internal static SupportBundleResult Failed(string bundlePath, string error)
    {
        return new SupportBundleResult(false, bundlePath, error, Array.Empty<string>());
    }
}
