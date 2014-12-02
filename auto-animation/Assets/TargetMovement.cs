using UnityEngine;
using System.Collections;

[AddComponentMenu("Inverse Kinematics/IK Test Target Movement")]

public class TargetMovement : MonoBehaviour {
    public float translationAmount = 0.01f;
    public Transform targetTransform;

	// Update is called once per frame
	void Update () {
        if (!targetTransform) {
            targetTransform = transform;
        }

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
        targetTransform.Translate(t, Camera.main.transform);
	}
}
