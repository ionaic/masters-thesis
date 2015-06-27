using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ConstrainedPhysicalControllerSkeleton : IEnumerable<PhysicalJoint> {
    public PhysicalJoint[] UpperBody;
    public PhysicalJoint Pelvis;
    public PhysicalJoint RHip;
    public PhysicalJoint LHip;
    public PhysicalJoint RKnee;
    public PhysicalJoint LKnee;
    public PhysicalJoint RFoot;
    public PhysicalJoint LFoot;
    public PhysicalJoint RHeel;
    public PhysicalJoint LHeel;
    public SpringMuscle[] muscles;
    public Vector3 COM;
    public Vector3 support_center;

    public void UpdateCOM() {
        Vector3 tempCom = Vector3.zero;
        
        tempCom += Pelvis.jointMass * Pelvis.Position();
        tempCom += RHip.jointMass * RHip.Position();
        tempCom += LHip.jointMass * LHip.Position();
        tempCom += RKnee.jointMass * RKnee.Position();
        tempCom += LKnee.jointMass * LKnee.Position();
        tempCom += RFoot.jointMass * RFoot.Position();
        tempCom += LFoot.jointMass * LFoot.Position();
        tempCom += RHeel.jointMass * RHeel.Position();
        tempCom += LHeel.jointMass * LHeel.Position();
        
        tempCom /= TotalMass();
        COM = tempCom;
    }
    
    public float TotalMass() {
        float upperBody = 0.0f;
        foreach (PhysicalJoint j in UpperBody) {
            upperBody += j.jointMass;
        }
        return upperBody + Pelvis.jointMass 
            + RHip.jointMass + LHip.jointMass 
            + RKnee.jointMass + LKnee.jointMass
            + RFoot.jointMass + LFoot.jointMass
            + RHeel.jointMass + LHeel.jointMass;
    }

    // acceleration vecetor from all of the muscles
    public Vector3 acceleration(float deltaTime) {
        // TODO need to account for the fact that different joints have different amounts of the mass they work with?
        Vector3 resultantAcceleration = Vector3.zero;
        foreach (SpringMuscle m in muscles) {
            resultantAcceleration += m.instantLinearAcceleration(deltaTime);
        }
        return resultantAcceleration;
    }
    
    public PhysicalJoint this[int index] {
        get {
            switch (index) {
                case 0:
                    return Pelvis;
                case 1:
                    return LHip;
                case 2:
                    return LKnee;
                case 3:
                    return  LFoot;
                case 4:
                    return RHip;
                case 5:
                    return RKnee;
                case 6:
                    return RFoot;
                case 7:
                    return RHeel;
                case 8:
                    return LHeel;
                default:
                    return UpperBody[index - 8];
            }
        }
    }

    IEnumerator System.Collections.IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
    
    public IEnumerator<PhysicalJoint> GetEnumerator() {
        yield return Pelvis;
        yield return LHip;
        yield return LKnee;
        yield return LFoot;
        yield return RHip;
        yield return RKnee;
        yield return RFoot;
        yield return RHeel;
        yield return LHeel;
        foreach (PhysicalJoint jnt in UpperBody) {
            yield return jnt;
        }
    }
}
