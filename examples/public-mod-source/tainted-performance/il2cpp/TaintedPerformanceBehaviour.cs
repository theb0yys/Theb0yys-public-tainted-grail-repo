using System;
using UnityEngine;

namespace TaintedPerformance.Il2Cpp;

internal sealed class TaintedPerformanceBehaviour : MonoBehaviour
{
    public TaintedPerformanceBehaviour(IntPtr pointer)
        : base(pointer)
    {
    }

    private void Update()
    {
        Il2CppPlugin.Controller?.Tick();
    }

    private void OnGUI()
    {
        Il2CppPlugin.Controller?.Draw();
    }
}
