using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Tainted.Armour;

internal sealed class VisualRuntimeObservationStage
{
    internal const string StageVersion = "a2k-t34.visual-runtime-observation.v1";

    private static readonly RequiredObservationRow[] RequiredRows =
    {
        new RequiredObservationRow("female", "neck_02", 3),
        new RequiredObservationRow("female", "spine_04", 208),
        new RequiredObservationRow("female", "spine_05", 11),
        new RequiredObservationRow("male", "neck_02", 3),
        new RequiredObservationRow("male", "spine_04", 208),
        new RequiredObservationRow("male", "spine_05", 11),
    };

    internal VisualRuntimeValidationResult Execute(
        VisualRuntimeObservationRequest? request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (request == null)
        {
            return VisualRuntimeValidationResult.NotRequested(StageVersion);
        }

        var blockers = new List<string>();
        var warnings = new List<string>();

        bool routeAccepted = string.Equals(
            request.RouteId,
            VisualRuntimeObservationRequest.A2KT34RouteId,
            StringComparison.Ordinal);
        if (!routeAccepted)
        {
            blockers.Add("a2k_t34_visual_observer_route_invalid");
        }

        bool bodyObserved = IsObserved(request.LiveBodyObservationStatus);
        if (!bodyObserved)
        {
            blockers.Add("a2k_t34_live_body_observation_missing");
        }

        bool equipObserved = IsObserved(request.LiveEquipObservationStatus);
        if (!equipObserved)
        {
            blockers.Add("a2k_t34_live_equip_observation_missing");
        }

        bool poseBindingAccepted = IsObserved(request.PoseBindingStatus);
        if (!poseBindingAccepted)
        {
            blockers.Add("a2k_t34_pose_binding_missing_or_invalid");
        }

        bool downstreamFalse =
            !request.CandidateMapApplicationAllowed &&
            !request.CandidateMapApplicationExecuted &&
            !request.ConversionAllowed &&
            !request.ConversionExecuted &&
            !request.ItemEquipMutationAllowed &&
            !request.ItemEquipMutationExecuted &&
            !request.SaveMutationAllowed &&
            !request.SaveMutationExecuted &&
            !request.DownstreamWritesAllowed &&
            !request.DownstreamWritesExecuted;
        if (!downstreamFalse)
        {
            blockers.Add("a2k_t34_downstream_boundary_not_false");
        }

        bool requiredRowsPresent = true;
        foreach (RequiredObservationRow required in RequiredRows)
        {
            bool found = request.Rows.Any(row =>
                string.Equals(row.Sex, required.Sex, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(row.DeformationReceiver, required.DeformationReceiver, StringComparison.OrdinalIgnoreCase) &&
                row.InversionRecordCount == required.InversionRecordCount);
            if (!found)
            {
                requiredRowsPresent = false;
                blockers.Add(
                    "a2k_t34_required_row_missing:" +
                    required.Sex + ":" +
                    required.DeformationReceiver + ":" +
                    required.InversionRecordCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        int blockedMetricRows = request.Rows.Count(row =>
            IsBlocked(row.ClippingStatus) ||
            IsBlocked(row.SeamStatus) ||
            IsBlocked(row.BodyCoverageStatus) ||
            IsBlocked(row.MaterialShadowStatus) ||
            IsBlocked(row.LayerStatus) ||
            IsBlocked(row.ReferenceTransportSemantics) ||
            IsBlocked(row.LocalWindingSemantics));
        if (blockedMetricRows > 0)
        {
            blockers.Add("a2k_t34_visual_metric_rows_blocked:" + blockedMetricRows.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        int unobservedRows = request.Rows.Count(row =>
            IsUnobserved(row.LiveObjectPath) ||
            IsUnobserved(row.RendererName) ||
            IsUnobserved(row.MeshName));
        if (unobservedRows > 0)
        {
            warnings.Add("a2k_t34_live_renderer_identity_unobserved_rows:" + unobservedRows.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        bool accepted =
            routeAccepted &&
            bodyObserved &&
            equipObserved &&
            poseBindingAccepted &&
            downstreamFalse &&
            requiredRowsPresent &&
            blockedMetricRows == 0;

        return new VisualRuntimeValidationResult(
            StageVersion,
            request.RouteId,
            request.ObserverMode,
            requested: true,
            bodyObserved,
            equipObserved,
            poseBindingAccepted,
            downstreamFalse,
            requiredRowsPresent,
            blockedMetricRows,
            request.Rows,
            accepted,
            blockers,
            warnings);
    }

    private static bool IsObserved(string value) =>
        value.IndexOf("observed", StringComparison.OrdinalIgnoreCase) >= 0 &&
        !IsBlocked(value);

    private static bool IsBlocked(string value) =>
        value.TrimStart().StartsWith("blocked", StringComparison.OrdinalIgnoreCase) ||
        value.IndexOf("blocked_", StringComparison.OrdinalIgnoreCase) >= 0 ||
        value.IndexOf("not-confirmed", StringComparison.OrdinalIgnoreCase) >= 0 ||
        value.IndexOf("missing", StringComparison.OrdinalIgnoreCase) >= 0;

    private static bool IsUnobserved(string value) =>
        string.IsNullOrWhiteSpace(value) ||
        value.IndexOf("unobserved", StringComparison.OrdinalIgnoreCase) >= 0 ||
        value.IndexOf("missing", StringComparison.OrdinalIgnoreCase) >= 0;

    private readonly struct RequiredObservationRow
    {
        internal RequiredObservationRow(string sex, string deformationReceiver, int inversionRecordCount)
        {
            Sex = sex;
            DeformationReceiver = deformationReceiver;
            InversionRecordCount = inversionRecordCount;
        }

        internal string Sex { get; }
        internal string DeformationReceiver { get; }
        internal int InversionRecordCount { get; }
    }
}
