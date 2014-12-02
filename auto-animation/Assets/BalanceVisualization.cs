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
	// Use this for initialization
	void Start () {
        supportingPlaneMesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        controller = GetComponent<PhysicalMotionController>();

        meshFilter.mesh = supportingPlaneMesh;
        controller.UpdateSupportingPoly();
        supportingPlaneMesh.triangles = faces;
        UpdateMesh();
        meshFilter.sharedMesh = supportingPlaneMesh;
	}
	
	// Update is called once per frame
	void Update () {
        //Debug.Log("Supporting Poly: " + controller.supportingPoly.Length);
        UpdateMesh();
	}
    
    void UpdateMesh() {
	    supportingPlaneMesh.vertices = controller.supportingPoly;
        supportingPlaneMesh.RecalculateNormals();
    }
}
