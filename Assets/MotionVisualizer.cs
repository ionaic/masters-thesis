using UnityEngine;
using System.Collections;

[System.Serializable]
public class JointVisualizer {
    public bool useTrace;
    public Material marker;
    public Transform traceJoint;
    private StaticParticleManager trailManager;
    
    public void SetTrailManager(StaticParticleManager mgr) {
        trailManager = mgr;
        trailManager.particle = GameObject.Instantiate(Resources.Load("Marker", typeof(TrailParticle))) as TrailParticle;
        UpdateMarker();
    }
    
    public void UpdateMarker() {
        trailManager.particle.renderer.material = marker;
    }

    public void PlotPosition() {
        if (useTrace) {
            trailManager.AddParticle(traceJoint);
        }
    }
}

[System.Serializable]
public class MotionVisualizer : MonoBehaviour {
    public JointVisualizer[] joints;

    void Start() {
        for (int idx = 0; idx < joints.Length; ++idx) {
            joints[idx].SetTrailManager(gameObject.AddComponent<StaticParticleManager>() as StaticParticleManager);
        }

        UpdateMarkers();
    }
    
    public void UpdateMarkers() {
        for (int idx = 0; idx < joints.Length; ++idx) {
            joints[idx].UpdateMarker();
        }
    }

    public void UpdateTraces() {
        for (int idx = 0; idx < joints.Length; ++idx) {
            joints[idx].PlotPosition();
        }
    }
}
