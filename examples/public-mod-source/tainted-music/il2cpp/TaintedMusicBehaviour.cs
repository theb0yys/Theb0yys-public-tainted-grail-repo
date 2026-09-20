using System;
using UnityEngine;

namespace TaintedMusic.Il2Cpp;

internal sealed class TaintedMusicBehaviour : MonoBehaviour
{
    public TaintedMusicBehaviour(IntPtr pointer)
        : base(pointer)
    {
    }

    private void Update()
    {
        TaintedMusic.Plugin.Il2CppUpdate();
    }

    private void OnGUI()
    {
        TaintedMusic.Plugin.Il2CppOnGUI();
    }
}
