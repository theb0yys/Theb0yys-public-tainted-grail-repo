using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Tainted.Armour.Unity
{
    /// <summary>
    /// Converts Unity-owned mesh data into the provider-neutral Tainted Armour
    /// geometry contracts. It does not apply maps, mutate source assets, or
    /// perform any downstream pipeline operation.
    /// </summary>
    public sealed class UnityGeometrySnapshotAdapter
    {
        public PoseGeometrySnapshot CaptureBakedSnapshot(
            SkinnedMeshRenderer renderer,
            string poseId,
            string clipId,
            double sampleTimeSeconds)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (renderer.sharedMesh == null)
            {
                throw new ArgumentException("The renderer must have a shared mesh.", nameof(renderer));
            }

            var baked = new Mesh
            {
                name = "TaintedArmourGeometrySnapshot",
                hideFlags = HideFlags.HideAndDontSave,
            };
            try
            {
                renderer.BakeMesh(baked, true);
                return AdaptSnapshot(poseId, clipId, sampleTimeSeconds, baked.vertices);
            }
            finally
            {
                DestroyTransient(baked);
            }
        }

        public PoseGeometrySnapshot AdaptSnapshot(
            string poseId,
            string clipId,
            double sampleTimeSeconds,
            IEnumerable<Vector3> positions)
        {
            if (positions == null) throw new ArgumentNullException(nameof(positions));
            Vector3[] unityPositions = positions.ToArray();
            if (unityPositions.Length == 0)
            {
                throw new ArgumentException("A Unity geometry snapshot requires at least one position.", nameof(positions));
            }

            var adapted = new ArmorVector3[unityPositions.Length];
            for (int index = 0; index < unityPositions.Length; index++)
            {
                Vector3 value = unityPositions[index];
                adapted[index] = new ArmorVector3(value.x, value.y, value.z);
            }

            return new PoseGeometrySnapshot(
                poseId,
                clipId,
                sampleTimeSeconds,
                ComputeGeometryFingerprint(unityPositions),
                adapted);
        }

        public ArmorTriangle[] CaptureSubmeshTriangles(Mesh mesh, int submeshIndex)
        {
            if (mesh == null) throw new ArgumentNullException(nameof(mesh));
            if (submeshIndex < 0 || submeshIndex >= mesh.subMeshCount)
            {
                throw new ArgumentOutOfRangeException(nameof(submeshIndex));
            }

            return AdaptTriangleIndices(mesh.GetTriangles(submeshIndex));
        }

        public ArmorTriangle[] AdaptTriangleIndices(IEnumerable<int> triangleIndices)
        {
            if (triangleIndices == null) throw new ArgumentNullException(nameof(triangleIndices));
            int[] indices = triangleIndices.ToArray();
            if (indices.Length == 0 || indices.Length % 3 != 0)
            {
                throw new ArgumentException("Triangle indices must contain one or more complete triples.", nameof(triangleIndices));
            }

            var triangles = new ArmorTriangle[indices.Length / 3];
            for (int ordinal = 0; ordinal < triangles.Length; ordinal++)
            {
                int offset = ordinal * 3;
                triangles[ordinal] = new ArmorTriangle(
                    ordinal,
                    indices[offset],
                    indices[offset + 1],
                    indices[offset + 2]);
            }

            return triangles;
        }

        public string ComputeGeometryFingerprint(IEnumerable<Vector3> positions)
        {
            if (positions == null) throw new ArgumentNullException(nameof(positions));
            Vector3[] values = positions.ToArray();
            if (values.Length == 0)
            {
                throw new ArgumentException("At least one position is required.", nameof(positions));
            }

            var text = new StringBuilder(values.Length * 36);
            text.Append(values.Length.ToString(CultureInfo.InvariantCulture));
            foreach (Vector3 value in values)
            {
                text.Append('|').Append(value.x.ToString("R", CultureInfo.InvariantCulture));
                text.Append('|').Append(value.y.ToString("R", CultureInfo.InvariantCulture));
                text.Append('|').Append(value.z.ToString("R", CultureInfo.InvariantCulture));
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] digest = sha256.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()));
                var hexadecimal = new StringBuilder(digest.Length * 2);
                foreach (byte value in digest)
                {
                    hexadecimal.Append(value.ToString("x2", CultureInfo.InvariantCulture));
                }

                return "sha256:" + hexadecimal;
            }
        }

        private static void DestroyTransient(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(value);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(value);
            }
        }
    }
}
