using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TransformData {
    Vector3 position;
    Quaternion rotation;
    
    Vector3 localPosition;
    Quaternion localRotation;
    Vector3 localScale;
    
    public TransformData(Transform t) {
        GetFromTransform(t);
    }
    
    public void GetFromTransform(Transform t) {
        position = t.position;
        rotation = t.rotation;
            
        localPosition = t.localPosition;
        localRotation = t.localRotation;
        localScale = t.localScale;
    }
    
    public void SetTransformFrom(Transform t) {
        t.position = position;
        t.rotation = rotation;
            
        t.localPosition = localPosition;
        t.localRotation = localRotation;
        t.localScale = localScale;
    }
}

[System.Serializable]
public class ConstrainedPhysicalControllerSkeleton : IEnumerable<PhysicalJoint> {
    public PhysicalJoint[] UpperBody;
    public PhysicalJoint Pelvis;
    public PhysicalJoint RHip;
    public PhysicalJoint LHip;
    public PhysicalJoint RKnee;
    public PhysicalJoint LKnee;
    public PhysicalJoint RAnkle;
    public PhysicalJoint LAnkle;
    public PhysicalJoint RHeel;
    public PhysicalJoint LHeel;
    public PhysicalJoint RToe;
    public PhysicalJoint LToe;
    public SpringMuscle[] muscles;
    public Vector3 COM;
    public Vector3 support_center;
    public Vector3[] supportingPoly;

    public int Size() {
        return 11 + UpperBody.Length;
    }

    public TransformData[] GetResetArray() {
        List<TransformData> tmp = new List<TransformData>();
        foreach (PhysicalJoint j in this) {
            tmp.Add(new TransformData(j.jointTransform));
        }
        
        return tmp.ToArray();
    }

    public void ResetFromArray(TransformData[] reset) {
        for (int idx = 0; idx < this.Size(); ++idx) {
            reset[idx].SetTransformFrom(this[idx].jointTransform);
        }
    }

    public void UpdateSupportingPoly() {
        // TODO currently assuming the supporting plane includes entire foot,
        // is this ok? think of the plane when on tiptoes, the heel still sort
        // of describes where the bounds of the plane should be even though the
        // toes are the only things contacting the ground
        // TODO need to use the foot geometry not positions of joints
        //Debug.Log("supportingPolyLen " + supportingPoly.Length);

        // just in case the array isn't allocated yet
        if (supportingPoly.Length == 0) {
            supportingPoly = new Vector3[4];
        }

        supportingPoly = jointBasedPoly();
        
        support_center = (supportingPoly[0] + supportingPoly[1] + supportingPoly[2] + supportingPoly[3]) / 4.0f; 

    }
    
    //private Vector3[] meshBasedPoly() {
    //}

    private Vector3[] jointBasedPoly() {
        Vector3[] supportingPoly = new Vector3[4];

        supportingPoly[0] = LHeel.jointTransform.position;
        supportingPoly[1] = LToe.jointTransform.position;
        supportingPoly[2] = RToe.jointTransform.position;
        supportingPoly[3] = RHeel.jointTransform.position;

        float min_y =   Mathf.Min(supportingPoly[0].y, Mathf.Min(supportingPoly[1].y, Mathf.Min(supportingPoly[2].y, supportingPoly[3].y)));

        supportingPoly[0].y = supportingPoly[1].y = supportingPoly[2].y = supportingPoly[3].y = min_y; 
        
        return supportingPoly;
    }

    public void UpdateCOM() {
        Vector3 tempCom = Vector3.zero;

        foreach (PhysicalJoint j in this) {
            tempCom += j.Mass() * j.Position();
        }
        
        tempCom /= TotalMass();
        COM = tempCom;
    }

    public void PositionPelvis(Vector3 servo_modification) {
        //Debug.Log("Reposition by " + servo_modification + " start: " + Pelvis.Position());
        Pelvis.jointTransform.Translate(servo_modification);
        //Debug.Log("Reposition finish: " + Pelvis.Position());
    }
    
    public float TotalMass() {
        float total = 0.0f;
        foreach (PhysicalJoint j in this) {
            total += j.Mass();
        }
        return total;
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
                    return  LAnkle;
                case 4:
                    return RHip;
                case 5:
                    return RKnee;
                case 6:
                    return RAnkle;
                case 7:
                    return RHeel;
                case 8:
                    return LHeel;
                case 9:
                    return RToe;
                case 10:
                    return LToe;
                default:
                    return UpperBody[index - 11];
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
        yield return LAnkle;
        yield return RHip;
        yield return RKnee;
        yield return RAnkle;
        yield return RHeel;
        yield return LHeel;
        yield return RToe;
        yield return LToe;
        foreach (PhysicalJoint jnt in UpperBody) {
            yield return jnt;
        }
    }
}
