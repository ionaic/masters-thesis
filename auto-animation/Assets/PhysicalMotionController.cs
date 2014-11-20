﻿using UnityEngine;
using System.Collections;

///*
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
//*/

public abstract class Muscle {
    public abstract float force(float length);
    public abstract float desiredAngle(float force);
}

[System.Serializable]
public class SpringMuscle : Muscle {
    // Simple spring model of a muscle for determining target poses
    // for this, find a target length that will produce the desired joint

    // spring constant
    public float k;
    // resting length (pretty much can initialize as length of bone)
    public float l_0;
    // bone width
    public float bone_width;
    
    public override float force(float length) {
        // calculate the force of the muscle given a length
        return -k * length;
    }
    public override float desiredAngle(float force) {
        // given a desired force from the muscle, what angle (radians) should
        // the joint be at
        //float cos = (-k * bone_width) / (force + l_0);
        float cos = (2.0f * k * k * bone_width * bone_width) / (force * force)
            - 1.0f;
        cos = cos % 1.0f;
        Debug.Log("Cos " + cos);
        //return 2.0f * Mathf.Acos(cos - (cos / (2.0f * Mathf.PI)));
        //return Mathf.Acos(cos - (cos / (2.0f * Mathf.PI)));
        return Mathf.Acos(cos);
    }
}

/*
[System.Serializable]
public class HillMuscle : Muscle {
    // Hill 3 element model of a muscle for determining target poses
    // is this too complex a model for our purposes?
    // a lot of variables for if this was a full muscle model

    // serial element spring constant (tendons)
    public float k_serial;
    // parallel element spring constant (intra-muscular, effects of trying to
    // flex while muscle is stretched)
    public float k_parallel;
    // maximum "isometric" force
    public float F_max;
    // "coefficient of shortening heat" (what. smoothly (linearly) varying
    // value for how activated the muscle is (how "on" or "off))
    public float a;
    // calculated from other values, how to get values?
    private float b;
    // shortening velocity, where does this come from?
    private float v;
    
    public override float force(float length) {
        return (b * (F_max + a))/(v + b) + a;
    }
    public override float desiredAngle(float force) {
        // given a desired force from the muscle, what angle should the joint
        // be at
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
        // given a desired force from the muscle, what angle should the joint
        // be at
        return 0;
    }
}

[System.Serializable]
public class Limb {
    // not sure if this is necessary, keeping track of bones and the related
    // muscle
    public float boneWidth;
    public Transform mainBone;
    public Transform anchorBone;
    private Muscle musc;
    
    public Limb() {
        musc = new SpringMuscle();
    }
}
//*/


[System.Serializable]
public class PhysicalControllerSkeleton {
    public Transform[] UpperBody;
    public Transform Pelvis;
    public Transform RHip;
    public Transform LHip;
    public Transform RKnee;
    public Transform LKnee;
    public Transform RFoot;
    public Transform LFoot;
}

[System.Serializable]
public class ConstrainedPhysicalControllerSkeleton {
    public Joint[] UpperBody;
    public Joint Pelvis;
    public Joint RHip;
    public Joint LHip;
    public Joint RKnee;
    public Joint LKnee;
    public Joint RFoot;
    public Joint LFoot;
}
 
[AddComponentMenu("Physical Motion Controller/Physical Motion Controller")]
public class PhysicalMotionController : MonoBehaviour {
    public ConstrainedPhysicalControllerSkeleton skeleton;
    public PIDServo pidservo;
    public SpringMuscle testMuscle;
    public float bodyMass;
    //public MotionVisualizer visualizer;
    public float gravity = 10.0f;
    public float desiredAccel = 1.0f;
    private float angle;

	// Use this for initialization
	void Start () {
        angle = 0.0f;
        //visualizer = GetComponent<MotionVisualizer>();
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyDown(KeyCode.Return)) {
            TestJump(testMuscle);
        }
        if (IsRotating()) {
            skeleton.RKnee.Rotate(0.0f, 0.0f, angle * Time.deltaTime);
        }
        //visualizer.UpdateTraces();
	}
    
    void TestJump(Muscle musc) {
        // just try bending to get a force to accelerate upwards at net of 2.0f
        // m/s/s
        angle = musc.desiredAngle(bodyMass * gravity + bodyMass *
            desiredAccel);
        Debug.Log("radian angle " + angle);
        angle *= Mathf.Rad2Deg;
        Debug.Log("degree angle " + angle);
    }
    bool IsRotating() {
        //float curAngle = skeleton.RKnee.jointTransform.localRotation.eulerAngles.z;
        ////Debug.Log("current local angle " + curAngle);
        //return !Mathf.Approximately(curAngle, angle) && Mathf.Abs(curAngle -
        //    angle) > 5.0f;
        return false;
    }
}