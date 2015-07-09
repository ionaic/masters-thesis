using UnityEngine;
using System.Collections;

[RequireComponent (typeof(JumpController))]
public class PositionSampler : MonoBehaviour {
    public PhysicalJoint[] hips;
    public PhysicalJoint[] knees;
    public float angleStepInDegrees;
    
    public JumpLogger logger;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void SampleAll() {
        Vector3 astep = new Vector3(angleStepInDegrees, angleStepInDegrees, angleStepInDegrees);
        // the joints generally have one axis of rotation, so we are
        // assuming that each only has one allowed
        for (hips[0].RotateToMin(), hips[1].RotateToMin(); 
            !hips[0].AtMax(), !hips[1].AtMax(); 
            hips[0].Rotate(astep), hips[1].Rotate(astep)) {
            for (knees[0].RotateToMin(), knees[1].RotateToMin(); 
                !knees[0].AtMax(), !knees[1].AtMax(); 
                knees[0].Rotate(astep), knees[1].Rotate(astep)) {
            }
        }
	}
}
