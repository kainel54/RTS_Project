using System;
using UnityEngine;

namespace Code.Players
{
    [Serializable]
    public class CameraConfig
    {
        [field: SerializeField] public bool EnableEdgePan { get; private set; } = true;
        [field: SerializeField] public float MousePanSpeed { get; private set; } = 5f;
        [field: SerializeField] public float EdgePanSize { get; private set; } = 50f;
        [field: SerializeField] public float KeyboardPanSpeed { get; private set; } = 5f;
    }
}