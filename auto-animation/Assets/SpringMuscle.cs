using UnityEngine;
using System.Collections;

[System.Serializable]
public class SpringMuscle {
    // Simple spring model of a muscle for determining target poses
    // for this, find a target length that will produce the desired joint
    public string muscleName;

    // spring constant
    public float k;
    // resting length (pretty much can initialize as length of bone)
    //public float l_0;
    // the joints that affect the muscle length
    // TODO should just make the skeleton class better and have a reference to the skeleton and understand what the parent and child anchor joints are
    // the general solution would be to have a flexible number of anchors but we are using a simplified model with each muscle crossing one joint only
    public PhysicalJoint[] anchors = new PhysicalJoint[2];
    // 0-1 distances referring to how far along the bone the muscle is anchored, with 0 being at the center joint and 1 at the anchor
    public float[] anchorDistFromCenter = new float[2];

    // joint that this muscle crosses
    public PhysicalJoint centerJoint;
    
    // bone width
    public float bone_width;
    
    public float scalarForce() {
        // calculate the force of the muscle given a length
        return k * springDisplacement();
    }
    public float ElasticEnergy() {
        float displacement = springDisplacement();
        return 0.5f * k * displacement * displacement;
    }

    public bool IsLimbExtended(float tolerance = 0.1f) {
        return LimbUsage() <= tolerance;
    }
    // gives a [0,1] value indicating how bent the joint is
    public float LimbUsage(bool verbose = false) {
        Vector3 a = (anchors[0].Position() - centerJoint.Position());
        Vector3 b = (anchors[1].Position() - centerJoint.Position());

        // this makes hips ignore the built in 90 degree angle to the pelvis
        if (muscleName == "Right Hip" || muscleName == "Left Hip") {
            // pretend that the pelvis is actually above the hip instead of to the side
            a.y += Mathf.Abs(a.x);
            a.x = 0.0f;
        }
        

        a.Normalize();
        b.Normalize();
        // -1 to 1 dot product as they are unit vectors
        float usage = Vector3.Dot(a, b);
        //if (verbose) {
        //    Debug.Log(muscleName + " Raw Usage: " + usage);
        //}
        // move to 0 to 1 range
        usage = (usage + 1.0f) / 2.0f;
        // 0 is the limb is extended, no usage
        // 1 is fully flexed, max usage (greater actually as the vectors are
        //   the same!)
        return usage;
    }

    public float springDisplacement() {
        // i related the angles incorrectly i think? maybe?
        //return bone_width / Mathf.Sin((Mathf.PI - Mathf.Deg2Rad * centerJoint.Angle().x) / 2.0f);
        float theta = Mathf.Deg2Rad * centerJoint.Angle().x;
        // law of sines to derive
        // absolute value to cause the range of PI to 2PI to be positive as it still fits in our system just the same, but negative
        float tmp = Mathf.Abs(bone_width * Mathf.Sin(Mathf.PI - theta) / Mathf.Sin(theta / 2.0f));
        //if (muscleName == "Right Calf" || muscleName == "Left Calf") {
        //    // this joint  needs to be different as it starts at 90 and goes to 180
        //    tmp = Mathf.Sin((Mathf.PI - Mathf.Deg2Rad * centerJoint.Angle().x) / 2.0f);
        //}
        //else {
        //    tmp = Mathf.Sin((Mathf.Deg2Rad * centerJoint.Angle().x) / 2.0f);
        //}
        return tmp != 0 ? bone_width / tmp : 2.0f * bone_width;
    }

    public Vector3[] torque(Vector3 force) {
        // TODO is this at all correct what
        // this function is for if the force is given with a direction
        Vector3[] torques = new Vector3[2];

        // torque is displacement vector (anchor point to the joint center) cross product with force
        torques[0] = Vector3.Cross(
            (centerJoint.Position() - anchors[0].Position()) 
            * anchorDistFromCenter[0], force);
        torques[1] = Vector3.Cross(
            (centerJoint.Position() - anchors[1].Position()) 
            * anchorDistFromCenter[1], force);

        return torques;
    }
    public Vector3 torque(float force) {
        // vector indicating the moment arm which is the cross product of the
        // vectors of the center of the joint to the muscle attachment points
        // TODO should this be normalized?
        Vector3 momentArm = Vector3.Cross(
            (anchors[0].Position() - centerJoint.Position()) * anchorDistFromCenter[0], 
            (anchors[1].Position() - centerJoint.Position()) * anchorDistFromCenter[1]);
        //Debug.Log("torque moment arm: " + momentArm);
        // force times the moment arm
        return force * momentArm;
    }
    public Vector3 angularMomentum(float deltaTime, float force) {
        return torque(force) * deltaTime;
    }
    public Vector3 instantLinearAcceleration(float deltaTime, float force) {
        // TODO we need the moment of inertia to actually convert this, this math is wrong
        // moment of inertia is I = mk^2 for all point masses that are part of this, i.e. assign point masses to the object (limb masses at center of bone?)
        // this means we're possibly ignoring that the rotating thingy is larger, smaller, or more complicated in general than just a single point mass

        // this is actually measured at the end joints, but it is produced at
        // the center joint!

        // vector of the first bone in the joint (primary bone affected)
        // crossed with the torque to get a direction
        Vector3 am = angularMomentum(deltaTime, force);
        Vector3 dir = -1.0f * Vector3.Cross(anchors[0].Position() - centerJoint.Position(), am).normalized;

        // linear momentum is the radius times the angular momentum in a
        // direction tangent to the circle at the current point
        return dir * am.magnitude;// * (anchors[0].Position() - centerJoint.Position()).magnitude;
    }
    public Vector3 instantLinearAcceleration(float deltaTime) {
        return instantLinearAcceleration(deltaTime, scalarForce());
    }
    public Vector3 instantLinearMomentum(float deltaTime, float force) {
        // vector of the first bone in the joint (primary bone affected)
        // crossed with the torque to get a direction
        Vector3 am = angularMomentum(deltaTime, force);
        Vector3 dir = -1.0f * Vector3.Cross(anchors[0].Position() - centerJoint.Position(), am).normalized;

        // linear momentum is the radius times the angular momentum in a
        // direction tangent to the circle at the current point
        return dir * am.magnitude;// * (anchors[0].Position() - centerJoint.Position()).magnitude;
    }
    public Vector3 instantLinearMomentum(float deltaTime) {
        return instantLinearMomentum(deltaTime, scalarForce());
    }
    public float jointAngle(float force) {
        // given a desired force from the muscle, what angle (radians) should
        // the joint be at
        //float cos = (-k * bone_width) / (force + l_0);
        float cos = (2.0f * k * k * bone_width * bone_width) / (force * force)
            - 1.0f;
        cos = cos % 1.0f;
        //return 2.0f * Mathf.Acos(cos - (cos / (2.0f * Mathf.PI)));
        //return Mathf.Acos(cos - (cos / (2.0f * Mathf.PI)));
        return Mathf.Acos(cos);
    }
}
