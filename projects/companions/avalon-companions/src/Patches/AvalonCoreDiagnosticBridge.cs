using System;
using System.Collections;
using System.Reflection;
using BepInEx.Logging;

namespace AvalonCompanions.Patches;

internal static class AvalonCoreDiagnosticBridge
{
    private const string CorePluginTypeName = "AvalonCore.Plugin";
    private const int MaxResolveAttempts = 300;

    private static PropertyInfo? s_trustReportsProperty;
    private static bool s_finished;
    private static int s_resolveAttempts;

    internal static void TryLog(ManualLogSource logger)
    {
        if (s_finished || !Plugin.Enabled.Value || !Plugin.EnableAvalonCoreDiagnosticBridge.Value)
        {
            return;
        }

        s_resolveAttempts++;

        try
        {
            Type? corePluginType = ResolveCorePluginType();
            if (corePluginType == null)
            {
                if (s_resolveAttempts >= MaxResolveAttempts)
                {
                    s_finished = true;
                    logger.LogInfo("Avalon Core diagnostic bridge unavailable. CorePlugin=not-found; action=none.");
                }

                return;
            }

            PropertyInfo? trustReportsProperty = ResolveTrustReportsProperty(corePluginType);
            if (trustReportsProperty == null)
            {
                s_finished = true;
                logger.LogWarning("Avalon Core diagnostic bridge unavailable. TrustReportsProperty=missing; action=none.");
                return;
            }

            object? snapshot = trustReportsProperty.GetValue(null);
            if (snapshot == null)
            {
                s_finished = true;
                logger.LogWarning("Avalon Core diagnostic bridge rejected null trust report snapshot. action=none.");
                return;
            }

            TrustReportView report = TrustReportView.FromSnapshot(snapshot);
            bool statusKnown = report.Status == "not_initialized" ||
                report.Status == "skipped" ||
                report.Status == "completed" ||
                report.Status == "failed";
            bool countersValid =
                report.CandidatePackCount >= 0 &&
                report.ValidPackCount >= 0 &&
                report.ValidPackCount <= report.CandidatePackCount &&
                report.BlockingIssueCount >= 0 &&
                report.WarningIssueCount >= 0 &&
                report.PackRows <= report.CandidatePackCount &&
                report.PackRows <= report.PackSummaryLimit &&
                report.IssueRows >= 0;

            s_finished = true;
            if (statusKnown && countersValid && !report.WouldMutateRuntime)
            {
                logger.LogInfo(
                    "Avalon Core diagnostic bridge read trust report. "
                    + $"CoreVersion={Display(ReadStaticString(corePluginType, "PluginVersion"))}; "
                    + $"Status={Display(report.Status)}; "
                    + $"Reason={Display(report.Reason)}; "
                    + $"Candidates={report.CandidatePackCount}; "
                    + $"Valid={report.ValidPackCount}; "
                    + $"Blockers={report.BlockingIssueCount}; "
                    + $"Warnings={report.WarningIssueCount}; "
                    + $"PackRows={report.PackRows}; "
                    + $"IssueRows={report.IssueRows}; "
                    + $"WouldMutateRuntime={report.WouldMutateRuntime}; "
                    + "action=read-only.");
                return;
            }

            logger.LogWarning(
                "Avalon Core diagnostic bridge rejected trust report. "
                + $"Status={Display(report.Status)}; "
                + $"Reason={Display(report.Reason)}; "
                + $"StatusKnown={statusKnown}; "
                + $"CountersValid={countersValid}; "
                + $"WouldMutateRuntime={report.WouldMutateRuntime}; "
                + "action=none.");
        }
        catch (Exception ex)
        {
            s_finished = true;
            logger.LogWarning($"Avalon Core diagnostic bridge failed. Exception={ex.GetType().Name}; Message={ex.Message}; action=none.");
        }
    }

    private static Type? ResolveCorePluginType()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? pluginType = assembly.GetType(CorePluginTypeName, throwOnError: false);
            if (pluginType != null)
            {
                return pluginType;
            }
        }

        return null;
    }

    private static PropertyInfo? ResolveTrustReportsProperty(Type corePluginType)
    {
        if (s_trustReportsProperty != null)
        {
            return s_trustReportsProperty;
        }

        PropertyInfo? property = corePluginType.GetProperty("TrustReports", BindingFlags.Public | BindingFlags.Static);
        if (property != null)
        {
            s_trustReportsProperty = property;
        }

        return s_trustReportsProperty;
    }

    private static string ReadStaticString(Type type, string name)
    {
        FieldInfo? field = type.GetField(name, BindingFlags.Public | BindingFlags.Static);
        return field?.GetValue(null) as string ?? string.Empty;
    }

    private static string ReadString(object source, string propertyName)
    {
        return ReadProperty(source, propertyName)?.ToString() ?? string.Empty;
    }

    private static int ReadInt(object source, string propertyName)
    {
        object? value = ReadProperty(source, propertyName);
        if (value is int intValue)
        {
            return intValue;
        }

        return Convert.ToInt32(value);
    }

    private static bool ReadBool(object source, string propertyName)
    {
        object? value = ReadProperty(source, propertyName);
        if (value is bool boolValue)
        {
            return boolValue;
        }

        return Convert.ToBoolean(value);
    }

    private static int ReadCount(object source, string propertyName)
    {
        object? value = ReadProperty(source, propertyName);
        if (value is ICollection collection)
        {
            return collection.Count;
        }

        PropertyInfo? countProperty = value?.GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);
        object? count = countProperty?.GetValue(value);
        return count is int countValue ? countValue : 0;
    }

    private static object? ReadProperty(object source, string propertyName)
    {
        PropertyInfo? property = source.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property == null)
        {
            throw new MissingMemberException(source.GetType().FullName, propertyName);
        }

        return property.GetValue(source);
    }

    private static string Display(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "none" : value;
    }

    private readonly struct TrustReportView
    {
        private TrustReportView(
            string status,
            string reason,
            int candidatePackCount,
            int validPackCount,
            int blockingIssueCount,
            int warningIssueCount,
            int packSummaryLimit,
            int packRows,
            int issueRows,
            bool wouldMutateRuntime)
        {
            Status = status;
            Reason = reason;
            CandidatePackCount = candidatePackCount;
            ValidPackCount = validPackCount;
            BlockingIssueCount = blockingIssueCount;
            WarningIssueCount = warningIssueCount;
            PackSummaryLimit = packSummaryLimit;
            PackRows = packRows;
            IssueRows = issueRows;
            WouldMutateRuntime = wouldMutateRuntime;
        }

        internal string Status { get; }

        internal string Reason { get; }

        internal int CandidatePackCount { get; }

        internal int ValidPackCount { get; }

        internal int BlockingIssueCount { get; }

        internal int WarningIssueCount { get; }

        internal int PackSummaryLimit { get; }

        internal int PackRows { get; }

        internal int IssueRows { get; }

        internal bool WouldMutateRuntime { get; }

        internal static TrustReportView FromSnapshot(object snapshot)
        {
            return new TrustReportView(
                ReadString(snapshot, "Status"),
                ReadString(snapshot, "Reason"),
                ReadInt(snapshot, "CandidatePackCount"),
                ReadInt(snapshot, "ValidPackCount"),
                ReadInt(snapshot, "BlockingIssueCount"),
                ReadInt(snapshot, "WarningIssueCount"),
                ReadInt(snapshot, "PackSummaryLimit"),
                ReadCount(snapshot, "Packs"),
                ReadCount(snapshot, "Issues"),
                ReadBool(snapshot, "WouldMutateRuntime"));
        }
    }
}
