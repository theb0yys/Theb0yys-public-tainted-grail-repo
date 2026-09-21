using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Tainted.Armour;

internal sealed class DeformationValidationStage
{
    internal const string StageVersion = "tainted-armour.deformation-validation.v5";

    private readonly MetricCalibrationStage metricCalibrationStage = new MetricCalibrationStage();

    public DeformationValidationResult Execute(
        ArmorSourceContract source,
        ArmorTargetContract target,
        DeformationValidationRequest request,
        CancellationToken cancellationToken)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (target is null) throw new ArgumentNullException(nameof(target));
        if (request is null) throw new ArgumentNullException(nameof(request));

        cancellationToken.ThrowIfCancellationRequested();
        DeformationControlReceipt controls = metricCalibrationStage.Execute(request.Policy);
        var blockers = ControlBlockers(controls);
        var warnings = ControlWarnings(controls);
        var poseResults = new List<PoseDeformationResult>();
        ArmorVector3[] baseline = request.Baseline.CopyPositions();

        if (baseline.Length != source.VertexCount)
        {
            blockers.Add("baseline_vertex_count_mismatch:" + baseline.Length + "!=" + source.VertexCount);
        }

        if (request.Triangles.Count != source.TriangleCount)
        {
            blockers.Add("triangle_count_mismatch:" + request.Triangles.Count + "!=" + source.TriangleCount);
        }

        int[] invalidEvaluatedVertices = request.EvaluatedVertexIndices
            .Where(value => value >= baseline.Length)
            .ToArray();
        if (invalidEvaluatedVertices.Length != 0)
        {
            blockers.Add("evaluated_vertex_indices_out_of_range:" + string.Join(",", invalidEvaluatedVertices));
        }

        foreach (PoseGeometrySnapshot pose in request.Poses)
        {
            cancellationToken.ThrowIfCancellationRequested();
            poseResults.Add(EvaluatePose(baseline, pose, request, cancellationToken));
        }

        int collapsed = poseResults.Sum(pose => pose.TriangleEvidence.Count(value => value.Collapsed));
        int reversed = poseResults.Sum(pose => pose.TriangleEvidence.Count(value => value.OrientationReversed));
        int indeterminate = poseResults.Sum(pose => pose.TriangleEvidence.Count(value => value.Indeterminate));
        double maxDisplacement = poseResults.Count == 0 ? 0d : poseResults.Max(pose => pose.MaxVertexDisplacement);

        if (collapsed > request.Policy.AllowedCollapsedTriangles)
        {
            blockers.Add("collapsed_triangle_limit_exceeded:" + collapsed + ">" + request.Policy.AllowedCollapsedTriangles);
        }

        if (reversed > request.Policy.AllowedOrientationReversedTriangles)
        {
            blockers.Add("orientation_reversed_triangle_limit_exceeded:" + reversed + ">" + request.Policy.AllowedOrientationReversedTriangles);
        }

        if (indeterminate > 0)
        {
            blockers.Add("indeterminate_triangle_records:" + indeterminate);
        }

        bool accepted = blockers.Count == 0;
        return new DeformationValidationResult(
            StageVersion,
            request.Policy.MetricVersion,
            source.SourceFingerprint,
            target.TargetFingerprint,
            target.CandidateMapFingerprint,
            controls,
            poseResults,
            collapsed,
            reversed,
            indeterminate,
            maxDisplacement,
            accepted,
            blockers,
            warnings);
    }

    private static PoseDeformationResult EvaluatePose(
        ArmorVector3[] baseline,
        PoseGeometrySnapshot pose,
        DeformationValidationRequest request,
        CancellationToken cancellationToken)
    {
        ArmorVector3[] posed = pose.CopyPositions();
        var evidence = new List<TriangleDeformationEvidence>(request.Triangles.Count);
        float displacementSum = 0f;
        float maxDisplacement = 0f;
        int movedVertices = 0;

        if (posed.Length != baseline.Length)
        {
            foreach (ArmorTriangle triangle in request.Triangles)
            {
                evidence.Add(TriangleDeformationClassifier.Indeterminate(pose.PoseId, triangle, "pose_vertex_count_mismatch"));
            }

            return new PoseDeformationResult(
                pose.PoseId,
                pose.ClipId,
                pose.SampleTimeSeconds,
                pose.GeometryFingerprint,
                0,
                0,
                0d,
                0d,
                evidence);
        }

        foreach (int vertexIndex in request.EvaluatedVertexIndices)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (vertexIndex >= baseline.Length || vertexIndex >= posed.Length)
            {
                continue;
            }

            float displacement = TriangleDeformationClassifier.Distance(baseline[vertexIndex], posed[vertexIndex]);
            displacementSum += displacement;
            if (displacement > maxDisplacement) maxDisplacement = displacement;
            if (displacement > (float)request.Policy.DisplacementEpsilon) movedVertices++;
        }

        foreach (ArmorTriangle triangle in request.Triangles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            evidence.Add(TriangleDeformationClassifier.Classify(pose.PoseId, triangle, baseline, posed, request.Policy));
        }

        float mean = request.EvaluatedVertexIndices.Count == 0
            ? 0f
            : displacementSum / request.EvaluatedVertexIndices.Count;
        return new PoseDeformationResult(
            pose.PoseId,
            pose.ClipId,
            pose.SampleTimeSeconds,
            pose.GeometryFingerprint,
            request.EvaluatedVertexIndices.Count,
            movedVertices,
            maxDisplacement,
            mean,
            evidence);
    }

    private static List<string> ControlBlockers(DeformationControlReceipt controls)
    {
        var blockers = new List<string>();
        if (!controls.IdentityPassed) blockers.Add("control_identity_failed");
        if (!controls.RigidRotationPassed) blockers.Add("control_rigid_rotation_failed");
        if (!controls.OrientationReversalPassed) blockers.Add("control_orientation_reversal_failed");
        if (!controls.CollapsePassed) blockers.Add("control_collapse_failed");
        if (!controls.Passed) blockers.Add("metric_calibration_failed:" + controls.MetricVersion);
        return blockers;
    }

    private static List<string> ControlWarnings(DeformationControlReceipt controls)
    {
        var warnings = new List<string>();
        if (!controls.OutOfPlaneRigidRotationPassed)
        {
            warnings.Add("control_out_of_plane_rigid_rotation_diagnostic_failed:" + controls.MetricVersion);
        }

        return warnings;
    }
}
