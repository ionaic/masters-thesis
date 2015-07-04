using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public float secondsPerSample;
    private StaticParticleManager trailManager;
    private float secondsSinceSample;
    
    public JointVisualizer() {
        secondsSinceSample = 0.0f;
    }
    
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
    
    // take in a tick so we can pass deltaTime or fixedDeltaTime independent of
    // this function
    public void PlotPosition(float tick) {
        secondsSinceSample += tick;
        if (secondsSinceSample >= secondsPerSample) {
            PlotPosition();
            secondsSinceSample = 0.0f;
        }
    }
}

[System.Serializable]
[AddComponentMenu("Visualization/Motion Visualizer")]
public class MotionVisualizer : MonoBehaviour {
    public JointVisualizer[] joints;
    public bool showGhost;
    public bool showParticles;
    private bool useGhost; // toggleable but if you set it explicitly it's unaffected
    private bool useParticles;
    public float secondsPerGhostSample;
    private float secondsSinceGhost;
    private List<GameObject> ghosts;
    private bool paused;

    void Start() {
        secondsSinceGhost = 0.0f;
        for (int idx = 0; idx < joints.Length; ++idx) {
            joints[idx].SetTrailManager(gameObject.AddComponent<StaticParticleManager>() as StaticParticleManager);
        }

        ghosts = new List<GameObject>();

        UpdateMarkers();
    }
    
    void Update() {
        paused = Input.GetKey(KeyCode.Return);

        if (Input.GetKey(KeyCode.P)) {
            useParticles = !useParticles;
        }

        if (Input.GetKey(KeyCode.G)) {
            useGhost = !useGhost;
        }
        if (Input.GetKey(KeyCode.X)) {
            foreach (GameObject g in ghosts) {
                Destroy(g);
            }
        }
    }

    void FixedUpdate() {
        if (paused) {
            Debug.Log("Paused");
            return;
        }
        if (useParticles || showParticles) {
            UpdateTraces(Time.deltaTime);
        }

        if (useGhost || showGhost) {
            MakeGhost(Time.deltaTime);
        }
    }
    
    public void UpdateMarkers() {
        for (int idx = 0; idx < joints.Length; ++idx) {
            joints[idx].UpdateMarker();
        }
    }

    public void UpdateTraces(float tick) {
        for (int idx = 0; idx < joints.Length; ++idx) {
            joints[idx].PlotPosition(tick);
        }
    }
    
    public void MakeGhost(float tick) {
        secondsSinceGhost += tick;
        if (secondsSinceGhost >= secondsPerGhostSample) {
            GameObject ghost = Instantiate(gameObject) as GameObject;
            Destroy(ghost.GetComponent<BalanceVisualization>());
            Destroy(ghost.GetComponent<PhysicalMotionController>());
            Destroy(ghost.GetComponent<MotionVisualizer>());
            Destroy(ghost.GetComponent<MotionVisualizer>());
            Destroy(ghost.GetComponent<StaticParticleManager>());
            Destroy(ghost.GetComponent<TargetMovement>());
            Destroy(ghost.GetComponent<InverseKinematics>());
            secondsSinceGhost = 0.0f;
            ghosts.Add(ghost);
        }
    }
}
