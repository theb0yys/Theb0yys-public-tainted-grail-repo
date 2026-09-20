using System;
using UnityEngine;

namespace TGTemplate.Il2CppFrameSampler;

internal sealed class FrameSamplerBehaviour : MonoBehaviour
{
    private float _elapsed;
    private float _frameSeconds;
    private int _samples;

    public FrameSamplerBehaviour(IntPtr pointer)
        : base(pointer)
    {
    }

    private void Update()
    {
        float delta = Time.unscaledDeltaTime;
        if (delta <= 0f)
            return;

        _elapsed += delta;
        _frameSeconds += delta;
        _samples++;

        if (_elapsed < Plugin.LogIntervalSeconds)
            return;

        float averageSeconds = _samples > 0 ? _frameSeconds / _samples : 0f;
        float averageMs = averageSeconds * 1000f;
        float fps = averageSeconds > 0f ? 1f / averageSeconds : 0f;

        Plugin.Instance?.Report(averageMs, fps, _samples);
        _elapsed = 0f;
        _frameSeconds = 0f;
        _samples = 0;
    }
}
