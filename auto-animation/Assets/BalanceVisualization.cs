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

    private int[] faces = { 0, 1, 2, 
                            0, 2, 3, 
                            2, 1, 0, // include back faces as well
                            3, 2, 0 };
	// Use this for initialization
	void Start () {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        controller = GetComponent<PhysicalMotionController>();

        meshFilter.mesh = new Mesh();
	    meshFilter.mesh.vertices = controller.supportingPlane;
        meshFilter.mesh.triangles = faces;
	}
	
	// Update is called once per frame
	void Update () {
        //Debug.Log("Supporting Plane: " + controller.supportingPlane.Length);
	    meshFilter.mesh.vertices = controller.supportingPlane;
        meshFilter.mesh.RecalculateNormals();
	}
}
