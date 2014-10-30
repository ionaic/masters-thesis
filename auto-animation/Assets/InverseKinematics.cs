/* Inverse Kinematics implementation
 * Author: Ian Ooi
 * Created: October 2014
 * Last updated: October 2014
 */

using UnityEngine;
using System.Collections;

[System.Serializable]
public abstract class InverseKinematicHandle {
    // parent class for IK handles in case we add more solvers
    public Transform root;
    public Transform target;
    public abstract void rotateToTarget();
}

[System.Serializable]
public class SingleChainIK : InverseKinematicHandle {
    public Transform[] jointChain;
    public int affectedJoint;
    public float distanceTolerance;
    public int CCD_Iterations = 1;

    public void rotateJoint(Transform curJoint) {

        // R is the current joint in question
        // E is the joint to be moved to the target
        // D is the target position
        Vector3 R = curJoint.position,
                E = jointChain[affectedJoint].position,
                D = target.position,
                RD = D - R,
                RE = E - R;

        // normalize the RE and RD vectors
        RE.Normalize();
        RD.Normalize();

        // calculate the cos of the desired angle as the dot
        // product of the vectors RD (between the current joint and
        // the target) and RE (between the current joint and the
        // joint in question)
        float cosA = Vector3.Dot(RD, RE);
        float angle = Mathf.Acos(cosA);

        // rotate by angle A around the vector perpendicular to the
        // plane defined by RD and RE
        Vector3 axis = Vector3.Cross(RE, RD);
        curJoint.RotateAround(curJoint.position, axis
            , angle);
    }
    
    public override void rotateToTarget() {
        // make sure the given index is within the bounds of the array
        if (affectedJoint < jointChain.Length) {

            // repeat until CCD_Iterations have completed OR the proper
            // position is achieved
            for (int i = 0; i < CCD_Iterations; ++i) {

                // if the desired position has been achieved within the
                // given tolerance, then return
                if (Vector3.Distance(jointChain[affectedJoint].position,
                    target.position) < distanceTolerance) {

                    return;
                }

                // iterate over the joints up the chain from the joint in
                // question
                for (int jnt = affectedJoint - 1; jnt >= 0; --jnt) {
                    rotateJoint(jointChain[jnt]);
                }
                rotateJoint(root);
            }
        }
    }
}

[AddComponentMenu("Physical Motion Controller/Inverse Kinematics/Inverse Kinematics")]
public class InverseKinematics : MonoBehaviour {
    public SingleChainIK scHandle;
	
	// Update is called once per frame
	void Update () {
	    scHandle.rotateToTarget();
	}
}
