using UnityEngine;
using System.Collections;

[System.Serializable]
public class SimulationControls {
    public KeyCode pause = KeyCode.J;
    public KeyCode start = KeyCode.Space;
}

[System.Serializable]
public class VisualizationControls {
    public KeyCode pause = KeyCode.Return;
    public KeyCode start = KeyCode.V;
    public KeyCode useParticles = KeyCode.P;
    public KeyCode useGhost = KeyCode.G;
    public KeyCode deleteGhosts = KeyCode.X;
}

[System.Serializable]
public class CameraControls {
    public KeyCode sideView = KeyCode.Alpha0;
    public KeyCode slantView = KeyCode.Alpha9;
    public KeyCode frontView = KeyCode.Alpha8;
    public KeyCode screenshot = KeyCode.F12;
}

[System.Serializable]
public class SamplingControls {
    public KeyCode start = KeyCode.S;
}

[System.Serializable]
public class CustomInputManager : MonoBehaviour {
    public SimulationControls simulation;
    public VisualizationControls visualization;
    public SamplingControls sampling;
    public CameraControls cam;
    public KeyCode jump = KeyCode.Space;
}
