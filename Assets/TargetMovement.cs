using UnityEngine;
using System.Collections;

[AddComponentMenu("Physical Motion Controller/Inverse Kinematics/IK Test Target Movement")]

public class TargetMovement : MonoBehaviour {
    public float translationAmount = 0.01f;
	
	// Update is called once per frame
	void Update () {
        Vector3 t = Vector3.zero;
	    if (Input.GetKey(KeyCode.H)) {
            t.x -= translationAmount;
        }
	    if (Input.GetKey(KeyCode.J)) {
            t.y -= translationAmount;
        }
	    if (Input.GetKey(KeyCode.K)) {
            t.y += translationAmount;
        }
	    if (Input.GetKey(KeyCode.L)) {
            t.x += translationAmount;
        }
	    if (Input.GetKey(KeyCode.I)) {
            t.z += translationAmount;
        }
	    if (Input.GetKey(KeyCode.M)) {
            t.z -= translationAmount;
        }
        transform.Translate(t, Camera.main.transform);
	}
}
