using System;
using System.Linq;
using Tainted.Armour;
using Tainted.Armour.Unity;
using UnityEngine;

internal static class Program
{
    private static int Main()
    {
        try
        {
            var adapter = new UnityGeometrySnapshotAdapter();
            PoseGeometrySnapshot baseline = adapter.AdaptSnapshot(
                "baseline",
                "rest",
                0d,
                new[] { Vector3.zero, Vector3.right, Vector3.up });
            PoseGeometrySnapshot reversed = adapter.AdaptSnapshot(
                "pose.reversed",
                "fixture.clip",
                0.5d,
                new[] { Vector3.zero, Vector3.up, Vector3.right });
            ArmorTriangle[] triangles = adapter.AdaptTriangleIndices(new[] { 0, 1, 2 });

            Check(baseline.Positions.Count == 3, "Adapter did not preserve the vertex count.");
            Check(triangles.Length == 1 && triangles[0].Ordinal == 0, "Adapter did not preserve topology.");
            Check(
                baseline.GeometryFingerprint == adapter.AdaptSnapshot("baseline.2", "rest", 0d, new[] { Vector3.zero, Vector3.right, Vector3.up }).GeometryFingerprint,
                "Geometry fingerprints are not deterministic.");

            var source = new ArmorSourceContract(
                "fixture.unity",
                "sha256:fixture-source",
                "fixture-mesh",
                0,
                "fixture-material",
                "sha256:fixture-rig",
                "sha256:fixture-bind",
                "sha256:fixture-weights",
                3,
                1,
                3,
                4,
                0);
            var target = new ArmorTargetContract(
                "fixture.native-body",
                "fixture",
                "Cuirass",
                "sha256:fixture-target",
                "not-applied:fixture",
                "not_applied_validation_only");
            var request = new ArmorImportRequest(
                "fixture.unity-adapter.import",
                source,
                target,
                new DeformationValidationRequest(
                    baseline,
                    new[] { reversed },
                    triangles,
                    new[] { 0, 1, 2 },
                    DeformationPolicy.CreateT29T31ZeroTolerance()));

            ArmorImportResult result = new ArmorImporter().Import(request);
            Check(result.Status == ArmorImportStatus.DeformationBlocked, "Real adapter data did not reach the production deformation stage.");
            Check(result.Deformation.OrientationReversedTriangleCount == 1, "Production importer did not classify the adapted reversal.");
            Check(result.Deformation.Controls.OrientationReversalPassed, "Production orientation-reversal control did not pass.");
            Check(!result.Deformation.Controls.OutOfPlaneRigidRotationPassed, "Production calibration must expose the current metric's out-of-plane limitation.");
            Check(!result.Deformation.Controls.CandidateSupplementalMetricCompared, "Unsupported candidate supplemental metric was not frozen by default.");
            Check(result.Deformation.Controls.CandidateMetricDisagreementCount == 0, "Frozen candidate metric emitted a disagreement.");
            Check(!result.Deformation.Controls.CandidateMetricAffectsAcceptance, "Frozen candidate supplemental metric affected deformation acceptance.");
            Check(!result.CandidateMapApplicationAllowed && !result.CandidateMapApplicationExecuted, "Candidate-map application escaped the false boundary.");
            Check(!result.ConversionAllowed && !result.ConversionExecuted, "Conversion escaped the false boundary.");
            Check(!result.SidecarsGenerated && !result.RuntimeLoaderChanged && !result.RuntimeRegistrationExecuted, "A downstream write escaped the false boundary.");
            Check(!result.NativeGameWriteExecuted && !result.ReleaseReady, "A game write or release claim escaped the false boundary.");

            Console.WriteLine("TAINTED_ARMOUR_UNITY_FIXTURES_PASS count=4");
            return 0;
        }
        catch (Exception exception)
        {
            Console.WriteLine("TAINTED_ARMOUR_UNITY_FIXTURES_FAILED " + exception);
            return 1;
        }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
