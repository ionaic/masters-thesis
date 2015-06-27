using UnityEngine;
using System.Collections;

[System.Serializable]
public enum PathSolutionPolicy {
    AirTime,
    HighestPossible,
    // LowestPossible, // lowest possible is known from whatever objects to clear
    FastAsPossible,
    MinimumForce
}

[System.Serializable]
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
    public float windup_time;
    public float air_time;

    public JumpState state;
    public PathSolutionPolicy policy;
    
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

public class JumpController : MonoBehaviour {
    // handle calculation of estimated path, joint contortions, "what to do"
    // handle button pushes etc.
    public JumpVariables jumping;
    public PIDServo windupPD;
    public ConstrainedPhysicalControllerSkeleton skeleton;
    public JumpMotor motor;
    public float mass;
    
    void Start() {
        // initial estimate of path/velocity to get from start to end
        // calculation of intermediate goal states from starting point and final goal
        jumping.state = JumpState.NotJumping;
        if (!motor) {
            motor = gameObject.AddComponent<JumpMotor>() as JumpMotor;
        }
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
        motor.inputMoveDirection = transform.rotation * directionVector;
        // --------------------------------------
        bool inputJump = Input.GetButton("Jump");
        
        skeleton.UpdateCOM();

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
            if (motor.IsGrounded()) {
                jumping.state = JumpState.Landing;
            }
        }
        else if (jumping.state == JumpState.Landing) {
            if (Landing()) {
                jumping.state = JumpState.NotJumping;
            }
        }
    }

    // function to handle path estimate
    bool EstimatePath() {
        if (jumping.policy == PathSolutionPolicy.HighestPossible) {
            // find acceleration to maximize vertical component of x - x_0
        }
        else if (jumping.policy == PathSolutionPolicy.FastAsPossible) {
            // find acceleration minimizing t
        }
        else if (jumping.policy == PathSolutionPolicy.MinimumForce) {
            // find t to minimize a
        }
        else {
            jumping.acceleration = 2 * (jumping.destination - jumping.start - jumping.initial_velocity * jumping.air_time) / (jumping.air_time * jumping.air_time);
        }
        jumping.force = jumping.acceleration * mass;
        return jumping.IsAccelerationPossible(jumping.acceleration);
    }

    Vector3 BalanceError() {
        return (skeleton.support_center - skeleton.COM);
    }
        
    Vector3 AccelError() {
        // TODO currently not taking torque of joints into account, just
        // assuming force generated is a straight line force in desired
        // direction, need to take torque into account to give force a
        // direction

        // compare resultant angular acceleration to expected acceleration from force
        
        return skeleton.acceleration(jumping.windup_time);
    }

    // function to handle the windup phase
    // returns true when finished
    bool Windup() {
        Vector3 servo_modification = windupPD.modify(BalanceError());
        servo_modification += windupPD.modify(AccelError());
        return Mathf.Abs(AccelError().magnitude + BalanceError().magnitude) <= jumping.error_allowance;
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
