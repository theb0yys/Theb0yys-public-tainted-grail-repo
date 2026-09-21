using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TaintedWeapons;

internal static class TaintedWeaponProviderLifecycleHarness
{
    internal const string FormatVersion = "tainted-weapons-provider-l0-l12-lifecycle-text/1";

    internal static IReadOnlyList<TaintedWeaponProviderLifecycleGateRow> BuildRows(
        TaintedWeaponLifecycleSnapshot before,
        TaintedWeaponLifecycleSnapshot? after,
        IReadOnlyList<string> beforeRecordRows,
        IReadOnlyList<string> afterRecordRows,
        IReadOnlyList<TaintedWeaponProviderOwnerLedgerRow> beforeOwnerRows,
        IReadOnlyList<TaintedWeaponProviderOwnerLedgerRow> afterOwnerRows,
        IReadOnlyList<TaintedWeaponDrakeLoadingSnapshotRow> beforeDrakeRows,
        IReadOnlyList<TaintedWeaponDrakeLoadingSnapshotRow> afterDrakeRows,
        IReadOnlyList<string> eventRows,
        bool disposeAfterSnapshot)
    {
        if (before is null)
        {
            throw new ArgumentNullException(nameof(before));
        }

        int recordRowCount = beforeRecordRows.Count + afterRecordRows.Count;
        int ownerRowCount = beforeOwnerRows.Count + afterOwnerRows.Count;
        int drakeRowCount = beforeDrakeRows.Count + afterDrakeRows.Count;
        int frameworkDrakeRows = beforeDrakeRows.Concat(afterDrakeRows).Count(row => row.FrameworkKey);
        bool teardownSnapshotCaptured = disposeAfterSnapshot && after != null;
        var evidence = new LifecycleEvidence(before, after, beforeOwnerRows, afterOwnerRows, beforeDrakeRows, afterDrakeRows, eventRows, disposeAfterSnapshot);
        string snapshotEvidence = string.Join("; ", new[]
        {
            "beforeRecordCount=" + before.RecordCount.ToString(CultureInfo.InvariantCulture),
            "afterRecordCount=" + (after?.RecordCount.ToString(CultureInfo.InvariantCulture) ?? "not-captured"),
            "beforePrototypeCount=" + before.RuntimePrototypeObjectCount.ToString(CultureInfo.InvariantCulture),
            "afterPrototypeCount=" + (after?.RuntimePrototypeObjectCount.ToString(CultureInfo.InvariantCulture) ?? "not-captured"),
            "beforeMeshCounter=" + before.FrameworkMeshCounterTotal.ToString(CultureInfo.InvariantCulture),
            "afterMeshCounter=" + (after?.FrameworkMeshCounterTotal.ToString(CultureInfo.InvariantCulture) ?? "not-captured"),
            "beforeMaterialCounter=" + before.FrameworkMaterialCounterTotal.ToString(CultureInfo.InvariantCulture),
            "afterMaterialCounter=" + (after?.FrameworkMaterialCounterTotal.ToString(CultureInfo.InvariantCulture) ?? "not-captured"),
            "recordRows=" + recordRowCount.ToString(CultureInfo.InvariantCulture),
            "ownerRows=" + ownerRowCount.ToString(CultureInfo.InvariantCulture),
            "drakeRows=" + drakeRowCount.ToString(CultureInfo.InvariantCulture),
            "frameworkDrakeRows=" + frameworkDrakeRows.ToString(CultureInfo.InvariantCulture),
            "eventRows=" + eventRows.Count.ToString(CultureInfo.InvariantCulture),
            "disposeAfterSnapshot=" + disposeAfterSnapshot.ToString(CultureInfo.InvariantCulture),
            "teardownSnapshotCaptured=" + teardownSnapshotCaptured.ToString(CultureInfo.InvariantCulture)
        });

        return new[]
        {
            Row(
                "L0",
                "Unity-thread ownership",
                evidence.WorkerDispatchObserved ? "passed_unity_thread_owner_dispatch_observed" : "not_run_worker_dispatch_fixture_required",
                "unityThreadOwnerId|workerDispatchEvidence|unityApiCallRows",
                evidence.WorkerDispatchObserved,
                snapshotEvidence + "; " + evidence.WorkerDispatchObservation),
            Row(
                "L1",
                "Mount",
                evidence.SinglePackageAndBundleOwner ? "passed_package_and_bundle_owner_ledger_bound" : "failed_package_and_bundle_owner_ledger_not_bound",
                "packageMountId|packageOwner|bundleOwner|mountCount",
                evidence.SinglePackageAndBundleOwner,
                snapshotEvidence + "; " + evidence.PackageBundleObservation),
            Row(
                "L2",
                "Load exact asset",
                evidence.ExactAssetLoaded ? "passed_exact_asset_component_profile_bound" : "failed_exact_component_profile_not_bound",
                "assetPath|assetType|componentProfileHash|loadedAssetId",
                evidence.ExactAssetLoaded,
                snapshotEvidence + "; " + evidence.ExactAssetObservation),
            Row(
                "L3",
                "Duplicate load",
                evidence.DuplicateLoadObserved ? "passed_duplicate_load_shared_handle_policy_observed" : "not_run_duplicate_load_fixture_required",
                "duplicateLoadPolicy|firstHandleId|secondHandleId|sharedCount",
                evidence.DuplicateLoadObserved,
                snapshotEvidence + "; " + evidence.DuplicateLoadObservation),
            Row(
                "L4",
                "Dependency closure",
                evidence.DependencyClosureObserved ? "passed_dependency_owner_ledger_bound" : "failed_dependency_owner_ledger_not_bound",
                "declaredDependencies|resolvedDependencies|dependencyOwnerRows",
                evidence.DependencyClosureObserved,
                snapshotEvidence + "; " + evidence.DependencyObservation),
            Row(
                "L5",
                "Release asset",
                evidence.FinalAssetReleaseObserved ? "passed_final_asset_release_zero_loaded_assets" : "failed_final_asset_release_zero_loaded_assets_not_observed",
                "assetConsumerCountBefore|assetConsumerCountAfter|loadedAssetCountAfter",
                evidence.FinalAssetReleaseObserved,
                snapshotEvidence + "; " + evidence.FinalAssetReleaseObservation),
            Row(
                "L6",
                "Release bundle",
                evidence.BundleReleaseObserved ? "passed_bundle_release_zero_bundles" : "failed_bundle_child_owner_release_not_observed",
                "bundleChildCountBeforeRelease|bundleCountAfterRelease",
                evidence.BundleReleaseObserved,
                snapshotEvidence + "; " + evidence.BundleReleaseObservation),
            Row(
                "L7",
                "Early release",
                evidence.EarlyReleaseDenied ? "passed_early_release_denied_while_child_active" : "not_run_early_release_denial_fixture_required",
                "earlyReleaseAttempt|denialStatus|denialReason",
                evidence.EarlyReleaseDenied,
                snapshotEvidence + "; " + evidence.EarlyReleaseObservation),
            Row(
                "L8",
                "Remount",
                evidence.RemountObserved
                    ? "passed_remount_without_stale_duplicates"
                    : evidence.RemountFixtureObserved
                        ? "failed_remount_without_stale_duplicates_not_observed"
                        : "not_run_remount_fixture_required",
                "firstMountObjectIds|secondMountObjectIds|staleObjectCount",
                evidence.RemountObserved,
                snapshotEvidence + "; " + evidence.RemountObservation),
            Row(
                "L9",
                "Scene transition",
                evidence.SceneTransitionObserved ? "passed_scene_transition_ownership_observed" : "not_run_scene_transition_fixture_required",
                "sceneBefore|sceneAfter|ownerRowsBefore|ownerRowsAfter",
                evidence.SceneTransitionObserved,
                snapshotEvidence + "; " + evidence.SceneTransitionObservation),
            Row(
                "L10",
                "Plugin teardown",
                evidence.TeardownZeroObserved ? "passed_teardown_zero_provider_owners" : "failed_teardown_owner_ledger_incomplete",
                "teardownReason|packageCountAfter|bundleCountAfter|assetCountAfter|prototypeCountAfter|locatorCountAfter",
                evidence.TeardownZeroObserved,
                snapshotEvidence + "; " + evidence.TeardownObservation),
            Row(
                "L11",
                "Failure teardown",
                evidence.FailureTeardownObserved
                    ? "passed_failure_teardown_zero_residual_owners"
                    : evidence.FailureTeardownFixtureObserved
                        ? "failed_failure_teardown_zero_residual_owners_not_observed"
                        : "not_run_failure_teardown_fixture_required",
                "failureInjection|failureStatus|residualOwnerCount",
                evidence.FailureTeardownObserved,
                snapshotEvidence + "; " + evidence.FailureTeardownObservation),
            Row(
                "L12",
                "Memory/object stability",
                evidence.RepeatedStabilityObserved
                    ? "passed_repeated_stability_budget"
                    : evidence.RepeatedStabilityFixtureObserved
                        ? "failed_repeated_stability_budget_not_observed"
                        : "not_run_repeated_stability_budget_required",
                "cycleCount|maxObjectDelta|maxHandleDelta|maxMemoryDelta|budgetVersion",
                evidence.RepeatedStabilityObserved,
                snapshotEvidence + "; " + evidence.RepeatedStabilityObservation)
        };
    }

    private static TaintedWeaponProviderLifecycleGateRow Row(
        string id,
        string test,
        string status,
        string requiredReceiptFields,
        bool claimAllowed,
        string observation)
    {
        return new TaintedWeaponProviderLifecycleGateRow(
            id,
            test,
            status,
            claimAllowed,
            requiredReceiptFields,
            observation);
    }

    private sealed class LifecycleEvidence
    {
        private readonly TaintedWeaponLifecycleSnapshot _before;
        private readonly TaintedWeaponLifecycleSnapshot? _after;
        private readonly IReadOnlyList<TaintedWeaponProviderOwnerLedgerRow> _beforeOwnerRows;
        private readonly IReadOnlyList<TaintedWeaponProviderOwnerLedgerRow> _afterOwnerRows;
        private readonly IReadOnlyList<TaintedWeaponDrakeLoadingSnapshotRow> _beforeDrakeRows;
        private readonly IReadOnlyList<TaintedWeaponDrakeLoadingSnapshotRow> _afterDrakeRows;
        private readonly IReadOnlyList<string> _eventRows;
        private readonly bool _disposeAfterSnapshot;

        internal LifecycleEvidence(
            TaintedWeaponLifecycleSnapshot before,
            TaintedWeaponLifecycleSnapshot? after,
            IReadOnlyList<TaintedWeaponProviderOwnerLedgerRow> beforeOwnerRows,
            IReadOnlyList<TaintedWeaponProviderOwnerLedgerRow> afterOwnerRows,
            IReadOnlyList<TaintedWeaponDrakeLoadingSnapshotRow> beforeDrakeRows,
            IReadOnlyList<TaintedWeaponDrakeLoadingSnapshotRow> afterDrakeRows,
            IReadOnlyList<string> eventRows,
            bool disposeAfterSnapshot)
        {
            _before = before;
            _after = after;
            _beforeOwnerRows = beforeOwnerRows;
            _afterOwnerRows = afterOwnerRows;
            _beforeDrakeRows = beforeDrakeRows;
            _afterDrakeRows = afterDrakeRows;
            _eventRows = eventRows;
            _disposeAfterSnapshot = disposeAfterSnapshot;
        }

        internal bool WorkerDispatchObserved => HasEvent("provider-worker-dispatch") && HasEvent("provider-unity-owner-executed");

        internal string WorkerDispatchObservation => WorkerDispatchObserved
            ? "worker dispatch and Unity owner execution events are present"
            : "worker dispatch fixture was not exercised";

        internal bool SinglePackageAndBundleOwner =>
            ActiveBeforeOwners("package").Count == 1 &&
            ActiveBeforeOwners("bundle").Count == 1;

        internal string PackageBundleObservation =>
            "packageOwnerCount=" + ActiveBeforeOwners("package").Count.ToString(CultureInfo.InvariantCulture) +
            "; bundleOwnerCount=" + ActiveBeforeOwners("bundle").Count.ToString(CultureInfo.InvariantCulture);

        internal bool ExactAssetLoaded =>
            ActiveBeforeOwners("asset").Count == 1 &&
            ActiveBeforeOwners("asset").All(row =>
                string.Equals(row.AssetType, "UnityEngine.GameObject", StringComparison.Ordinal) &&
                row.ComponentProfileHash.Length == 64);

        internal string ExactAssetObservation
        {
            get
            {
                IReadOnlyList<TaintedWeaponProviderOwnerLedgerRow> rows = ActiveBeforeOwners("asset");
                if (rows.Count == 0)
                {
                    return "asset owner row is absent";
                }

                TaintedWeaponProviderOwnerLedgerRow row = rows[0];
                return "assetPath=" + row.AssetPath + "; assetType=" + row.AssetType + "; componentProfileHash=" + row.ComponentProfileHash;
            }
        }

        internal bool DuplicateLoadObserved =>
            ActiveBeforeOwners("asset").Any(row => row.ConsumerCount > 1);

        internal string DuplicateLoadObservation =>
            DuplicateLoadObserved
                ? "duplicateLoadPolicy=shared-owner-row; sharedCount=" + ActiveBeforeOwners("asset").Max(row => row.ConsumerCount).ToString(CultureInfo.InvariantCulture)
                : "duplicate load policy fixture was not exercised";

        internal bool DependencyClosureObserved =>
            HasEvent("provider-dependency-closure") &&
            ActiveBeforeOwners("dependency").Count > 0;

        internal string DependencyObservation => DependencyClosureObserved
            ? "dependency owner closure event is present; dependencyOwnerRows=" + ActiveBeforeOwners("dependency").Count.ToString(CultureInfo.InvariantCulture)
            : "dependency owner rows are not present";

        internal bool FinalAssetReleaseObserved =>
            _after != null &&
            ActiveBeforeOwners("asset").Count > 0 &&
            _afterOwnerRows.Any(row => IsKind(row, "asset")) &&
            _afterOwnerRows.Where(row => IsKind(row, "asset")).All(row => row.Released && row.ConsumerCount == 0) &&
            _after.RuntimeAssetHandleCount == 0 &&
            _after.ValidRuntimeAssetHandleCount == 0;

        internal string FinalAssetReleaseObservation =>
            _after == null
                ? "after-release snapshot is absent"
                : "assetOwnersAfter=" + _afterOwnerRows.Count(row => IsKind(row, "asset")).ToString(CultureInfo.InvariantCulture) +
                  "; activeAssetConsumersAfter=" + _afterOwnerRows.Where(row => IsKind(row, "asset")).Sum(row => row.ConsumerCount).ToString(CultureInfo.InvariantCulture) +
                  "; runtimeAssetHandleCountAfter=" + _after.RuntimeAssetHandleCount.ToString(CultureInfo.InvariantCulture) +
                  "; validRuntimeAssetHandleCountAfter=" + _after.ValidRuntimeAssetHandleCount.ToString(CultureInfo.InvariantCulture);

        internal bool BundleReleaseObserved =>
            _after != null &&
            ActiveBeforeOwners("bundle").Count > 0 &&
            _afterOwnerRows.Any(row => IsKind(row, "bundle")) &&
            _afterOwnerRows.Where(row => IsKind(row, "bundle")).All(row => row.Released && row.ConsumerCount == 0);

        internal string BundleReleaseObservation =>
            _after == null
                ? "after-release snapshot is absent"
                : "bundleOwnersAfter=" + _afterOwnerRows.Count(row => IsKind(row, "bundle")).ToString(CultureInfo.InvariantCulture) +
                  "; activeBundleConsumersAfter=" + _afterOwnerRows.Where(row => IsKind(row, "bundle")).Sum(row => row.ConsumerCount).ToString(CultureInfo.InvariantCulture);

        internal bool EarlyReleaseDenied => HasEvent("provider-early-release-denied");

        internal string EarlyReleaseObservation => EarlyReleaseDenied
            ? "earlyReleaseAttempt=denied"
            : "early release denial fixture was not exercised";

        internal bool RemountFixtureObserved => TryFindEventDetails("provider-remount", out _);

        internal bool RemountObserved =>
            TryFindEventDetails("provider-remount", out string details) &&
            TryReadIntField(details, "firstMountObjectIds", out int firstMountObjectId) &&
            firstMountObjectId != 0 &&
            TryReadIntField(details, "secondMountObjectIds", out int secondMountObjectId) &&
            secondMountObjectId != 0 &&
            TryReadIntField(details, "staleObjectCount", out int staleObjectCount) &&
            staleObjectCount == 0;

        internal string RemountObservation =>
            TryFindEventDetails("provider-remount", out string remountDetails)
                ? "remount fixture parsed; " + remountDetails
                : "remount fixture was not exercised";

        internal bool SceneTransitionObserved =>
            HasEvent("scene-transition") &&
            _beforeOwnerRows.Count > 0 &&
            _afterOwnerRows.Count > 0 &&
            _before.SceneName.Length > 0;

        internal string SceneTransitionObservation => SceneTransitionObserved
            ? "sceneBeforeAfterEvent=present; ownerRowsBefore=" + _beforeOwnerRows.Count.ToString(CultureInfo.InvariantCulture)
            : "scene transition ownership fixture was not exercised";

        internal bool TeardownZeroObserved =>
            _disposeAfterSnapshot &&
            _after != null &&
            _beforeOwnerRows.Count > 0 &&
            _afterOwnerRows.Count > 0 &&
            _after.RecordCount == 0 &&
            _after.RuntimePrototypeRecordCount == 0 &&
            _after.RuntimePrototypeObjectCount == 0 &&
            _after.StorageRootChildCount == 0 &&
            _after.RegisteredAssetKeyCount == 0 &&
            _after.RuntimeAssetHandleCount == 0 &&
            _after.ValidRuntimeAssetHandleCount == 0 &&
            _after.NativeSourceHandleValidCount == 0 &&
            _after.FrameworkMeshCounterTotal == 0 &&
            _after.FrameworkMaterialCounterTotal == 0 &&
            _afterOwnerRows.All(row => row.Released && row.ConsumerCount == 0) &&
            !_afterDrakeRows.Any(row => row.FrameworkKey && row.Counter > 0);

        internal string TeardownObservation =>
            _after == null
                ? "teardown snapshot is absent"
                : "recordCountAfter=" + _after.RecordCount.ToString(CultureInfo.InvariantCulture) +
                  "; registeredAssetKeyCountAfter=" + _after.RegisteredAssetKeyCount.ToString(CultureInfo.InvariantCulture) +
                  "; runtimeAssetHandleCountAfter=" + _after.RuntimeAssetHandleCount.ToString(CultureInfo.InvariantCulture) +
                  "; frameworkMeshCounterTotalAfter=" + _after.FrameworkMeshCounterTotal.ToString(CultureInfo.InvariantCulture) +
                  "; frameworkMaterialCounterTotalAfter=" + _after.FrameworkMaterialCounterTotal.ToString(CultureInfo.InvariantCulture) +
                  "; unreleasedProviderOwnersAfter=" + _afterOwnerRows.Count(row => !row.Released || row.ConsumerCount > 0).ToString(CultureInfo.InvariantCulture);

        internal bool FailureTeardownFixtureObserved => TryFindEventDetails("provider-forced-load-failure", out _);

        internal bool FailureTeardownObserved =>
            TryFindEventDetails("provider-forced-load-failure", out string failureDetails) &&
            TryReadStringField(failureDetails, "failureStatus", out string failureStatus) &&
            string.Equals(failureStatus, "failed-closed", StringComparison.OrdinalIgnoreCase) &&
            TryFindEventDetails("provider-failure-teardown-zero", out string teardownDetails) &&
            TryReadIntField(teardownDetails, "residualOwnerCount", out int residualOwnerCount) &&
            residualOwnerCount == 0 &&
            TryReadIntField(teardownDetails, "activeOwnerCountBefore", out int activeOwnerCountBefore) &&
            TryReadIntField(teardownDetails, "activeOwnerCountAfter", out int activeOwnerCountAfter) &&
            activeOwnerCountAfter == activeOwnerCountBefore;

        internal string FailureTeardownObservation =>
            TryFindEventDetails("provider-failure-teardown-zero", out string failureTeardownDetails)
                ? "failure fixture parsed; " + failureTeardownDetails
                : "forced load failure fixture was not exercised";

        internal bool RepeatedStabilityFixtureObserved => TryFindEventDetails("provider-stability-cycle", out _);

        internal bool RepeatedStabilityObserved =>
            TryFindEventDetails("provider-stability-cycle", out string stabilityDetails) &&
            TryReadIntField(stabilityDetails, "cycleCount", out int cycleCount) &&
            cycleCount >= 1 &&
            TryReadIntField(stabilityDetails, "maxObjectDelta", out int maxObjectDelta) &&
            maxObjectDelta <= 0 &&
            TryReadIntField(stabilityDetails, "maxHandleDelta", out int maxHandleDelta) &&
            maxHandleDelta <= 0 &&
            TryReadIntField(stabilityDetails, "maxMemoryDelta", out int maxMemoryDelta) &&
            maxMemoryDelta <= 0 &&
            TryReadStringField(stabilityDetails, "budgetVersion", out string budgetVersion) &&
            string.Equals(budgetVersion, "gate6-source-harness-v2", StringComparison.OrdinalIgnoreCase) &&
            _beforeDrakeRows.Concat(_afterDrakeRows).All(row => row.Counter <= 1);

        internal string RepeatedStabilityObservation =>
            TryFindEventDetails("provider-stability-cycle", out string stabilityDetails)
                ? "stability fixture parsed; " + stabilityDetails
                : "repeated stability fixture and accepted numeric budget are not present";

        private IReadOnlyList<TaintedWeaponProviderOwnerLedgerRow> ActiveBeforeOwners(string ownerKind)
        {
            return _beforeOwnerRows
                .Where(row => IsKind(row, ownerKind) && !row.Released && row.ConsumerCount > 0)
                .ToArray();
        }

        private bool HasEvent(string value)
        {
            return _eventRows.Any(row => row.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private bool TryFindEventDetails(string eventName, out string details)
        {
            details = _eventRows
                .Select(row => TryParseEventRow(row, out string parsedEventName, out string parsedDetails)
                    ? (eventName: parsedEventName, details: parsedDetails)
                    : (eventName: string.Empty, details: string.Empty))
                .Where(row => string.Equals(row.eventName, eventName, StringComparison.OrdinalIgnoreCase))
                .Select(row => row.details)
                .LastOrDefault() ?? string.Empty;
            return details.Length > 0;
        }

        private static bool TryParseEventRow(string row, out string eventName, out string details)
        {
            eventName = string.Empty;
            details = string.Empty;
            string[] columns = (row ?? string.Empty).Split('\t');
            if (columns.Length < 5)
            {
                return false;
            }

            eventName = columns[3];
            details = columns[4];
            return eventName.Length > 0;
        }

        private static bool TryReadIntField(string details, string fieldName, out int value)
        {
            value = 0;
            return TryReadStringField(details, fieldName, out string raw) &&
                int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryReadStringField(string details, string fieldName, out string value)
        {
            value = string.Empty;
            foreach (string segment in (details ?? string.Empty).Split(';'))
            {
                string trimmed = segment.Trim();
                int equalsIndex = trimmed.IndexOf('=');
                if (equalsIndex <= 0)
                {
                    continue;
                }

                string name = trimmed.Substring(0, equalsIndex).Trim();
                if (!string.Equals(name, fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                value = trimmed.Substring(equalsIndex + 1).Trim();
                return value.Length > 0;
            }

            return false;
        }

        private static bool IsKind(TaintedWeaponProviderOwnerLedgerRow row, string ownerKind)
        {
            return string.Equals(row.OwnerKind, ownerKind, StringComparison.OrdinalIgnoreCase);
        }
    }
}

internal readonly struct TaintedWeaponProviderLifecycleGateRow
{
    internal const string Headers = "id\ttest\tstatus\tclaimAllowed\trequiredReceiptFields\tobservation";

    internal TaintedWeaponProviderLifecycleGateRow(
        string id,
        string test,
        string status,
        bool claimAllowed,
        string requiredReceiptFields,
        string observation)
    {
        Id = id;
        Test = test;
        Status = status;
        ClaimAllowed = claimAllowed;
        RequiredReceiptFields = requiredReceiptFields;
        Observation = observation;
    }

    internal string Id { get; }

    internal string Test { get; }

    internal string Status { get; }

    internal bool ClaimAllowed { get; }

    internal string RequiredReceiptFields { get; }

    internal string Observation { get; }

    internal string ToTsv()
    {
        return string.Join("\t", new[]
        {
            Tsv(Id),
            Tsv(Test),
            Tsv(Status),
            ClaimAllowed.ToString(CultureInfo.InvariantCulture),
            Tsv(RequiredReceiptFields),
            Tsv(Observation)
        });
    }

    private static string Tsv(string value)
    {
        return (value ?? string.Empty)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace('\t', ' ');
    }
}
