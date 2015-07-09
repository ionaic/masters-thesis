﻿using UnityEngine;
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
public class SamplingControls {
    public KeyCode start;
}

public class CustomInputManager : MonoBehaviour {
    public SimulationControls simulation;
    public VisualizationControls visualization;
    public KeyCode jump = KeyCode.Space;
    public KeyCode samplePositions = KeyCode.S;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
