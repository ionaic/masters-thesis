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
    public CustomInputManager controls;
    public JointVisualizer[] joints;
    public bool showGhost;
    public bool showParticles;
    private bool useGhost; // toggleable but if you set it explicitly it's unaffected
    private bool useParticles;
    public float secondsPerGhostSample;
    private float secondsSinceGhost;
    private List<GameObject> ghosts;
    private GameObject ghostBody;
    private bool paused;

    void Start() {
        secondsSinceGhost = 0.0f;
        for (int idx = 0; idx < joints.Length; ++idx) {
            joints[idx].SetTrailManager(gameObject.AddComponent<StaticParticleManager>() as StaticParticleManager);
        }

        ghosts = new List<GameObject>();
        ghostBody = Resources.Load("EmptyBody", typeof(GameObject)) as GameObject;

        UpdateMarkers();
    }
    
    void Update() {
        paused = Input.GetKey(controls.visualization.pause);

        if (Input.GetKey(controls.visualization.useParticles)) {
            useParticles = !useParticles;
        }

        if (Input.GetKey(controls.visualization.useGhost)) {
            useGhost = !useGhost;
        }
        if (Input.GetKey(controls.visualization.deleteGhosts)) {
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

    private void IterativelySetTransforms(Transform root) {
        Queue<Transform> ts = new Queue<Transform>();
        Queue<Transform> sources = new Queue<Transform>();
        ts.Enqueue(root);
        sources.Enqueue(transform.Find(root.name));
        while (ts.Count > 0 && sources.Count > 0) {
            Transform t = ts.Dequeue();
            Transform s = sources.Dequeue();
            if (s == null) {
                Debug.Log("Couldn't find " + t.name + " in " + name);
            }
            else {
                t.position = s.position;
                t.rotation = s.rotation;
            }
            foreach (Transform child in t) {
                ts.Enqueue(child);
                if (s != null) {
                    sources.Enqueue(s.Find(child.name));
                }
                else {
                    sources.Enqueue(null);
                }
            }
        }
    }
    
    public void MakeGhost(float tick) {
        secondsSinceGhost += tick;
        if (secondsSinceGhost >= secondsPerGhostSample) {
            GameObject ghost = Instantiate(ghostBody) as GameObject;
            ghost.transform.position = transform.position;
            ghost.transform.rotation = transform.rotation;

            IterativelySetTransforms(ghost.transform.Find("pelvis"));

            secondsSinceGhost = 0.0f;
            ghosts.Add(ghost);
        }
    }
}
