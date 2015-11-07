using UnityEngine;
using System.Collections;

[RequireComponent(typeof(JumpController))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("Visualization/Balance Visualizer")]
public class BalanceVisualization : MonoBehaviour {
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    JumpController controller;
    Mesh supportingPlaneMesh;

    private int[] faces = { 0, 1, 2, 
                            0, 2, 3, 
                            3, 2, 1, // include back faces as well
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
        //meshRenderer = GetComponent<MeshRenderer>();
        controller = GetComponent<JumpController>();

        meshFilter.mesh = supportingPlaneMesh;
        controller.skeleton.UpdateSupportingPoly();
        UpdateMesh();
        
        supportingPlaneMesh.triangles = faces;
        supportingPlaneMesh.normals = norms;
        supportingPlaneMesh.uv = uv;
        meshFilter.sharedMesh = supportingPlaneMesh;
    }
	
	// Update is called once per frame
	void Update () {
        //Debug.Log("Supporting Poly: " + controller.skeleton.supportingPoly.Length);
        UpdateMesh();
        //CreateMesh();
	}
    
    void UpdateMesh() {
        Vector3[] verts = new Vector3[4];
            
        verts[0] = transform.InverseTransformPoint(controller.skeleton.supportingPoly[0]);
        verts[1] = transform.InverseTransformPoint(controller.skeleton.supportingPoly[1]);
        verts[2] = transform.InverseTransformPoint(controller.skeleton.supportingPoly[2]);
        verts[3] = transform.InverseTransformPoint(controller.skeleton.supportingPoly[3]);

	    supportingPlaneMesh.vertices = verts;
        //supportingPlaneMesh.RecalculateNormals();
    }
}
