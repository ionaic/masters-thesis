using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhysicalMotionController))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("Visualization/Balance Visualizer")]
public class BalanceVisualization : MonoBehaviour {
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    PhysicalMotionController controller;
    Mesh supportingPlaneMesh;

    private int[] faces = { 0, 1, 2, 
                            0, 2, 3, 
                            2, 1, 0, // include back faces as well
                            3, 2, 0 };
    private Vector3[] norms = { 
         Vector3.up,
         Vector3.up,
         Vector3.up,
         Vector3.up
    };

    private Vector2[] uv = {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(1, 1)
    };

	// Use this for initialization
	void Start () {
        CreateMesh();
	}

    void CreateMesh() {
        supportingPlaneMesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        controller = GetComponent<PhysicalMotionController>();

        meshFilter.mesh = supportingPlaneMesh;
        controller.UpdateSupportingPoly();
        UpdateMesh();
        supportingPlaneMesh.triangles = faces;
        supportingPlaneMesh.normals = norms;
        supportingPlaneMesh.uv = uv;
        UpdateMesh();
        meshFilter.sharedMesh = supportingPlaneMesh;
    }
	
	// Update is called once per frame
	void Update () {
        //Debug.Log("Supporting Poly: " + controller.supportingPoly.Length);
        UpdateMesh();
        //CreateMesh();
	}
    
    void UpdateMesh() {
	    supportingPlaneMesh.vertices = controller.supportingPoly;
        supportingPlaneMesh.RecalculateNormals();
    }
}
