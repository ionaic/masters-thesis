using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ProcGeometryTest : MonoBehaviour {
    public float width, height;
    public PhysicalMotionController controller;

	// Use this for initialization
	void Start () {
	    MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mf.mesh = mesh;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(0,0,0);
        vertices[1] = new Vector3(0, height, 0);
        vertices[2] = new Vector3(width, height, 0);
        vertices[3] = new Vector3(width, 0, 0);
        
        vertices[0] = transform.InverseTransformPoint(controller.skeleton.LHeel.jointTransform.position);
        vertices[1] = transform.InverseTransformPoint(controller.skeleton.LFoot.jointTransform.position);
        vertices[2] = transform.InverseTransformPoint(controller.skeleton.RFoot.jointTransform.position);
        vertices[3] = transform.InverseTransformPoint(controller.skeleton.RHeel.jointTransform.position);

        mesh.vertices = vertices;
        //mesh.vertices = controller.supportingPoly;
        //for (int idx = 0; idx < mesh.vertices.Length; ++idx) {
        //    Debug.Log("Verts[" + idx + "]: " + mesh.vertices[idx]);
        //}

        int[] tri = new int[6];
        
        tri[0] = 0;
        tri[1] = 1;
        tri[2] = 2;

        tri[3] = 0;
        tri[4] = 2;
        tri[5] = 3;

        mesh.triangles = tri;

        Vector3[] normals = new Vector3[4];
        
        normals[0] = Vector3.up;
        normals[1] = Vector3.up;
        normals[2] = Vector3.up;
        normals[3] = Vector3.up;
        
        mesh.normals = normals;
        
        Vector2[] uv = new Vector2[4];

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);
        
        mesh.uv = uv;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
