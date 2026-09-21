using BepInEx.Logging;
using UnityEngine;

namespace AvalonBroodmotherCompanion;

internal static class BroodmotherRangedAttackVfxRuntime
{
    private const int SegmentCount = 18;
    private const float DurationSeconds = 0.48f;
    private const float StartWidth = 0.18f;
    private const float EndWidth = 0.035f;

    internal static void Spawn(Vector3 from, Vector3 to, ManualLogSource logger)
    {
        try
        {
            if (!IsFinite(from) || !IsFinite(to))
            {
                return;
            }

            Vector3 delta = to - from;
            if (delta.sqrMagnitude <= 0.01f)
            {
                return;
            }

            GameObject root = new("AvalonBroodmotherCompanion.GildedSpiderAttackVfx");
            root.hideFlags = HideFlags.DontSave;

            Material material = CreateMaterial();
            LineRenderer line = root.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = SegmentCount;
            line.numCapVertices = 4;
            line.numCornerVertices = 2;
            line.startWidth = StartWidth;
            line.endWidth = EndWidth;
            line.material = material;

            Color startColor = new(1f, 0.72f, 0.18f, 0.94f);
            Color endColor = new(0.82f, 0.28f, 1f, 0.72f);
            line.startColor = startColor;
            line.endColor = endColor;

            float arcHeight = Mathf.Clamp(delta.magnitude * 0.22f, 0.45f, 1.75f);
            for (int i = 0; i < SegmentCount; i++)
            {
                float t = SegmentCount == 1 ? 1f : i / (SegmentCount - 1f);
                Vector3 point = Vector3.Lerp(from, to, t);
                point += Vector3.up * (Mathf.Sin(Mathf.PI * t) * arcHeight);
                line.SetPosition(i, point);
            }

            AddPulseSphere(root.transform, from, 0.16f, material);
            AddPulseSphere(root.transform, to, 0.24f, material);

            BroodmotherRangedAttackVfxPulse pulse = root.AddComponent<BroodmotherRangedAttackVfxPulse>();
            pulse.Initialize(line, material, startColor, endColor, DurationSeconds);
        }
        catch (System.Exception ex)
        {
            logger.LogWarning("Avalon Broodmother Companion ranged attack VFX failed: " + ex.GetType().Name + ": " + ex.Message);
        }
    }

    private static Material CreateMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Hidden/Internal-Colored");
        Material material = new(shader);
        material.hideFlags = HideFlags.DontSave;
        if (material.HasProperty("_Color"))
        {
            material.color = new Color(1f, 0.66f, 0.16f, 0.86f);
        }

        return material;
    }

    private static void AddPulseSphere(Transform parent, Vector3 position, float size, Material material)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "AvalonBroodmotherCompanion.GildedSpiderAttackVfxPulse";
        sphere.hideFlags = HideFlags.DontSave;
        sphere.transform.SetParent(parent, worldPositionStays: true);
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * size;

        Collider? collider = sphere.GetComponent<Collider>();
        if (collider != null)
        {
            UnityEngine.Object.Destroy(collider);
        }

        Renderer? renderer = sphere.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
    }

    private static bool IsFinite(Vector3 value)
    {
        return
            IsFinite(value.x) &&
            IsFinite(value.y) &&
            IsFinite(value.z);
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private sealed class BroodmotherRangedAttackVfxPulse : MonoBehaviour
    {
        private LineRenderer? _line;
        private Material? _material;
        private Color _startColor;
        private Color _endColor;
        private float _startedAt;
        private float _duration;

        internal void Initialize(
            LineRenderer line,
            Material material,
            Color startColor,
            Color endColor,
            float duration)
        {
            _line = line;
            _material = material;
            _startColor = startColor;
            _endColor = endColor;
            _startedAt = Time.unscaledTime;
            _duration = Mathf.Max(0.05f, duration);
        }

        private void Update()
        {
            float t = Mathf.Clamp01((Time.unscaledTime - _startedAt) / _duration);
            float alpha = 1f - t;
            if (_line != null)
            {
                Color start = _startColor;
                Color end = _endColor;
                start.a *= alpha;
                end.a *= alpha;
                _line.startColor = start;
                _line.endColor = end;
                _line.startWidth = Mathf.Lerp(StartWidth, EndWidth, t);
                _line.endWidth = Mathf.Lerp(EndWidth, 0.005f, t);
            }

            if (_material != null && _material.HasProperty("_Color"))
            {
                _material.color = new Color(1f, 0.66f, 0.16f, 0.86f * alpha);
            }

            transform.localScale = Vector3.one * Mathf.Lerp(1f, 1.22f, t);
            if (t >= 1f)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                Destroy(_material);
                _material = null;
            }
        }
    }
}
