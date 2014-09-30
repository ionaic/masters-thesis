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
    public JointVisualizer hip;
    public JointVisualizer leftKnee;
    public JointVisualizer rightKnee;
    public JointVisualizer leftFoot;
    public JointVisualizer rightFoot;

    void Start() {
        hip.SetTrailManager(        gameObject.AddComponent<StaticParticleManager>() as StaticParticleManager);
        leftKnee.SetTrailManager(   gameObject.AddComponent<StaticParticleManager>() as StaticParticleManager);
        rightKnee.SetTrailManager(  gameObject.AddComponent<StaticParticleManager>() as StaticParticleManager);
        leftFoot.SetTrailManager(   gameObject.AddComponent<StaticParticleManager>() as StaticParticleManager);
        rightFoot.SetTrailManager(  gameObject.AddComponent<StaticParticleManager>() as StaticParticleManager);

        UpdateMarkers();
    }
    
    public void UpdateMarkers() {
        hip.UpdateMarker();
        leftKnee.UpdateMarker();
        rightKnee.UpdateMarker();
        leftFoot.UpdateMarker();
        rightFoot.UpdateMarker();
    }

    public void UpdateTraces() {
        hip.PlotPosition();
        leftKnee.PlotPosition();
        rightKnee.PlotPosition();
        leftFoot.PlotPosition();
        rightFoot.PlotPosition();
    }
}
