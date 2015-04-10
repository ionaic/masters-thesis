using UnityEngine;
using System.Collections;

public enum JumpPolicy {
    AirTime,
    HighestPossible,
    // LowestPossible, // lowest possible is known from whatever objects to clear
    FastAsPossible,
    MinimumForce
}

public enum JumpState {
    NotJumping,
    WindUp,
    Accel,
    InAir,
    Landing
}

[System.Serializable]
public class JumpVariables {
    public Vector3 start;
    public Vector3 destination;
    public Vector3 initial_velocity;
    public Vector3 acceleration;
    public Vector3 min_possible_accel;
    public Vector3 max_possible_accel;
    public Vector3 force;
    public float error_allowance;
    public float time;

    public JumpState state;
    public JumpPolicy policy;
    
    public bool IsAccelerationPossible(Vector3 accel) {
        return (accel.x <= max_possible_accel.x &&
                accel.y <= max_possible_accel.y &&
                accel.z <= max_possible_accel.z && 
                accel.x >= min_possible_accel.x &&
                accel.y >= min_possible_accel.y &&
                accel.z >= min_possible_accel.z);
    }
}

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
    public Vector3 modify(Vector3 error) {
        return k_p * error + k_i * error + k_d * error;
    }
}

[System.Serializable]
public class SpringMuscle {
    // Simple spring model of a muscle for determining target poses
    // for this, find a target length that will produce the desired joint

    // spring constant
    public float k;
    // resting length (pretty much can initialize as length of bone)
    public float l_0;
    // the joints that affect the muscle length
    public PhysicalJoint[] anchors;
    // bone width
    public float bone_width;
    
    public float force() {
        // calculate the force of the muscle given a length
        return -k * (bone_width / Mathf.Sin(Mathf.PI - anchors[1].jointTransform.localRotation.x / 2.0f));
    }
    public float force(float length) {
        return -k * (length - l_0);
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

[System.Serializable]
public class ConstrainedPhysicalControllerSkeleton {
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
    public Vector3 COM;
    public Vector3 support_center;
}

public class JumpController : MonoBehaviour {
    // handle calculation of estimated path, joint contortions, "what to do"
    // handle button pushes etc.
    public JumpVariables jumping;
    public PIDServo windupPD;
    public ConstrainedPhysicalControllerSkeleton skeleton;
    public float mass;
    
    void Start() {
        // initial estimate of path/velocity to get from start to end
        // calculation of intermediate goal states from starting point and final goal
        jumping.state = JumpState.NotJumping;
    }
    
    void Update() {
        // FPS movement from Unity3D Standard Assets
        // Get the input vector from keyboard or analog stick
        Vector3 directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (directionVector != Vector3.zero) {
            // Get the length of the directon vector and then normalize it
            // Dividing by the length is cheaper than normalizing when we already have the length anyway
            float directionLength = directionVector.magnitude;
            directionVector = directionVector / directionLength;
            
            // Make sure the length is no bigger than 1
            directionLength = Mathf.Min(1, directionLength);
            
            // Make the input vector more sensitive towards the extremes and less sensitive in the middle
            // This makes it easier to control slow speeds when using analog sticks
            directionLength = directionLength * directionLength;
            
            // Multiply the normalized direction vector by the modified length
            directionVector = directionVector * directionLength;
        }
        
        // Apply the direction to the CharacterMotor
        //motor.inputMoveDirection = transform.rotation * directionVector;
        // --------------------------------------
        bool inputJump = Input.GetButton("Jump");
        
        // Downward/windup phase OR
        // Upward/accel phase OR if done
        // In Air
        // Landing
        if (inputJump && jumping.state == JumpState.NotJumping) {
            if (EstimatePath()) {
                jumping.state = JumpState.WindUp;
            }
        }
        else if (jumping.state == JumpState.WindUp) {
            if (Windup()) {
                jumping.state = JumpState.Accel;
            }
        }
        else if (jumping.state == JumpState.Accel) {
            if (Accel()) {
                jumping.state = JumpState.InAir;
            }
        }
        else if (jumping.state == JumpState.InAir) {
            //if (motor.IsGrounded()) {
                jumping.state = JumpState.Landing;
            //}
        }
        else if (jumping.state == JumpState.Landing) {
            if (Landing()) {
                jumping.state = JumpState.NotJumping;
            }
        }
    }

    // function to handle path estimate
    bool EstimatePath() {
        if (jumping.policy == JumpPolicy.HighestPossible) {
            // find acceleration to maximize vertical component of x - x_0
        }
        else if (jumping.policy == JumpPolicy.FastAsPossible) {
            // find acceleration minimizing t
        }
        else if (jumping.policy == JumpPolicy.MinimumForce) {
            // find t to minimize a
        }
        else {
            jumping.acceleration = 2 * (jumping.destination - jumping.start - jumping.initial_velocity * jumping.time) / (jumping.time * jumping.time);
        }
        jumping.force = jumping.acceleration * mass;
        return jumping.IsAccelerationPossible(jumping.acceleration);
    }

    Vector3 BalanceError() {
        return (skeleton.support_center - skeleton.COM);
    }
        
    Vector3 ForceError() {
        // TODO currently not taking torque of joints into account, just
        // assuming force generated is a straight line force in desired
        // direction, need to take torque into account to give force a
        // direction
        return Vector3.zero;
        
    }

    // function to handle the windup phase
    // returns true when finished
    bool Windup() {
        Vector3 servo_modification = windupPD.modify(BalanceError());
        servo_modification += windupPD.modify(ForceError());
        return Mathf.Abs(ForceError().magnitude + BalanceError().magnitude) <= jumping.error_allowance;
    }

    // function to handle accel phase
    bool Accel() {
        return true;
    }

    // function to handle the acceleration/upward phase
    bool InAir() {
        return true;
    }

    // function to handle landing
    bool Landing() {
        // NOOP since we're not handling landing
        return true;
    }
}
