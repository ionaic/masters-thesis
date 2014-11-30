using UnityEngine;
using System.Collections;

[System.Serializable]
[AddComponentMenu("Inverse Kinematics/Single Chain IK Handle")]
public class SingleChainIKHandle : InverseKinematicHandle {
    public PhysicalJoint[] jointChain;
    public int affectedJoint;
    public float distanceTolerance;
    public int CCD_Iterations = 1;

    public void rotateJoint(PhysicalJoint curJoint) {

        // R is the current joint in question
        // E is the joint to be moved to the target
        // D is the target position
        Vector3 R = curJoint.jointTransform.position,
                E = jointChain[affectedJoint].jointTransform.position,
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
        float cosA = Vector3.Dot(RD, RE) % 1.0f;
        float angle = Mathf.Acos(cosA);

        // rotate by angle A around the vector perpendicular to the
        // plane defined by RD and RE
        Vector3 axis = Vector3.Cross(RE, RD);
        curJoint.Rotate(R, axis, angle);
    }
    
    public override void rotateToTarget() {
        // make sure the given index is within the bounds of the array
        if (affectedJoint < jointChain.Length) {

            // repeat until CCD_Iterations have completed OR the proper
            // position is achieved
            for (int i = 0; i < CCD_Iterations; ++i) {

                // if the desired position has been achieved within the
                // given tolerance, then return
                if (Vector3.Distance(jointChain[affectedJoint].jointTransform.position,
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