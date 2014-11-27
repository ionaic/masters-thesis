using UnityEngine;
using System.Collections;

[System.Serializable]
[AddComponentMenu("Inverse Kinematics/IK Handle")]
public abstract class InverseKinematicHandle : MonoBehaviour {
    // parent class for IK handles in case we add more solvers
    public PhysicalJoint root;
    public Transform target;
    public abstract void rotateToTarget();
}
