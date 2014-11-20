using UnityEngine;
using System.Collections;

[System.Serializable] 
public class GhostLimb {
    public Transform joint1;
    public Transform joint2;

    private MeshFilter mf;
    private Mesh mesh;

    void calculateMesh(float radius) {
        //Vector3 vertices[8]; 
        //
        //vertices[0] = joint1.position + joint1.up * radius + joint1.right * radius;
        //vertices[1] = joint1.position + joint1.up * -1 * radius + joint1.right * radius;
        //vertices[2] = joint1.position + joint1.up * -1 * radius + joint1.right * -1 * radius;
        //vertices[3] = joint1.position + joint1.up * radius + joint1.right * -1 * radius;
        //vertices[4] = joint2.position + joint1.up * radius + joint1.right * radius;
        //vertices[5] = joint2.position + joint1.up * -1 * radius + joint1.right * radius;
        //vertices[6] = joint2.position + joint1.up * -1 * radius + joint1.right * -1 * radius;
        //vertices[7] = joint2.position + joint1.up * radius + joint1.right * -1 * radius;
        //
        //mesh.vertices = vertices;
    }
}

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
[AddComponentMenu("Visualization/Motion Visualizer")]
public class MotionVisualizer : MonoBehaviour {
    public JointVisualizer[] joints;

    void Start() {
        for (int idx = 0; idx < joints.Length; ++idx) {
            joints[idx].SetTrailManager(gameObject.AddComponent<StaticParticleManager>() as StaticParticleManager);
        }

        UpdateMarkers();
    }

    void Update() {
        UpdateTraces();
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
