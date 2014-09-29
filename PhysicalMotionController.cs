using UnityEngine;
using System.Collections;

[System.Serializable]
public class PIDServo {
    // PID servo function object
    // function object instead of function so it can store the k values
    public float k_p;
    public float k_i;
    public float k_d;
    
    public float modify(float error) {
        return k_p * error + k_i * error + k_d * error;
    }
}

public abstract class Muscle {
    public abstract float force(float length);
    public abstract float desiredAngle(float force);
}

[System.Serializable]
public class SpringMuscle : Muscle {
    // Simple spring model of a muscle for determining target poses
    // for this, find a target length that will produce the desired joint
    public float k; // spring constant
    public float l_0; // resting length (pretty much can initialize as length of bone)
    public float bone_width; // bone width
    
    public override float force(float length) {
        // calculate the force of the muscle given a length
        return -k * length;
    }
    public override float desiredAngle(float force) {
        // given a desired force from the muscle, what angle (radians) should
        // the joint be at
        float cos = (-k * bone_width) / (force + l_0);
        Debug.Log("Cos " + cos);
        //return 2.0f * Mathf.Acos(cos - (cos / (2.0f * Mathf.PI)));
        return Mathf.Acos(cos - (cos / (2.0f * Mathf.PI)));
    }
}

[System.Serializable]
public class HillMuscle : Muscle {
    // Hill 3 element model of a muscle for determining target poses
    // is this too complex a model for our purposes?
    // a lot of variables for if this was a full muscle model
    public float k_serial; // serial element spring constant (tendons)
    public float k_parallel; // parallel element spring constant (intra-muscular, effects of trying to flex while muscle is stretched)
    public float F_max; // maximum "isometric" force
    public float a; // "coefficient of shortening heat" (what. smoothly (linearly) varying value for how activated the muscle is (how "on" or "off))
    private float b; // calculated from other values, how to get values?
    private float v; // shortening velocity, where does this come from?
    
    public override float force(float length) {
        return (b * (F_max + a))/(v + b) + a;
    }
    public override float desiredAngle(float force) {
        // given a desired force from the muscle, what angle should the joint be at
        return 0;
    }
}

[System.Serializable]
public class VariedSpringMuscle : Muscle {
    // model of a muscle using a varying k to model flexing
    
    public override float force(float length) {
        return length;
    }
    public override float desiredAngle(float force) {
        // given a desired force from the muscle, what angle should the joint be at
        return 0;
    }
}

[System.Serializable]
public class Limb {
    // not sure if this is necessary, keeping track of bones and the related muscle
    public float boneWidth;
    public Transform mainBone;
    public Transform anchorBone;
    private Muscle musc;
    
    public Limb() {
        musc = new SpringMuscle();
    }
}

[System.Serializable]
public class PhysicalControllerSkeleton {
    public Transform Head;
    public Transform Hip;
    public Transform Neck;
    public Transform Spine1;
    public Transform Spine2;
    public Transform Spine3;
    public Transform RThigh;
    public Transform LThigh;
    public Transform RCalf;
    public Transform LCalf;
    public Transform RFoot;
    public Transform LFoot;
    public Transform RShoulder;
    public Transform LShoulder;
    public Transform RBicep;
    public Transform LBicep;
    public Transform RForearm;
    public Transform LForearm;
    public Transform RHand;
    public Transform LHand;
}

public class PhysicalMotionController : MonoBehaviour {
    public PhysicalControllerSkeleton skeleton;
    public PIDServo pidservo;
    public SpringMuscle testMuscle;
    public float bodyMass;
    public Transform tracer;
    public StaticParticleManager trailManager;
    public bool useTrace;
    public float gravity = 10.0f;
    public float desiredAccel = 1.0f;
    private float angle;

	// Use this for initialization
	void Start () {
        angle = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyDown(KeyCode.Space)) {
            TestJump(testMuscle);
        }
        if (IsRotating()) {
            skeleton.RCalf.Rotate(0.0f, 0.0f, angle * Time.deltaTime);
            if (useTrace) {
                trailManager.AddParticle(tracer);
            }
        }
	}
    
    void TestJump(Muscle musc) {
        // just try bending to get a force to accelerate upwards at net of 2.0f m/s/s
        angle = musc.desiredAngle(bodyMass * gravity + bodyMass * desiredAccel);
        Debug.Log("radian angle " + angle);
        angle *= Mathf.Rad2Deg;
        Debug.Log("degree angle " + angle);
        //angle = (angle + skeleton.RCalf.transform.rotation.z) % 360; // to get the target angle
    }
    bool IsRotating() {
        float curAngle = skeleton.RCalf.localRotation.eulerAngles.z;
        Debug.Log("current local angle " + curAngle);
        return !Mathf.Approximately(curAngle, angle) && Mathf.Abs(curAngle - angle) > 5.0f;
    }
}
