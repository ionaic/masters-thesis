using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    [HideInInspector]
    public Vector3 start;
    [HideInInspector]
    public Vector3 destination;
    public Transform target_location;
    public Vector3 initial_velocity;
    [HideInInspector]
    public Vector3 acceleration;
    public Vector3 min_possible_accel;
    public Vector3 max_possible_accel;
    [HideInInspector]
    public Vector3 force;
    public float error_allowance;
    public float windup_time;
    public float air_time;
    public Vector3 last_err;

    public JumpState state;
    public PathSolutionPolicy policy;
    
    public void init() {
        destination = target_location.position;

        state = JumpState.NotJumping;
    }
    
    public bool IsAccelerationPossible(Vector3 accel) {
        return (accel.x <= max_possible_accel.x &&
                accel.y <= max_possible_accel.y &&
                accel.z <= max_possible_accel.z && 
                accel.x >= min_possible_accel.x &&
                accel.y >= min_possible_accel.y &&
                accel.z >= min_possible_accel.z);
    }
}

[RequireComponent (typeof(InverseKinematics))]
[RequireComponent (typeof(PositionSampler))]
public class JumpController : MonoBehaviour {
    // handle calculation of estimated path, joint contortions, "what to do"
    // handle button pushes etc.
    public CustomInputManager controls;
    public JumpVariables jumping;
    public PIDServo windupPD;
    public ConstrainedPhysicalControllerSkeleton skeleton;
    public JumpMotor motor;
    public JumpLogger logger;
    private InverseKinematics IK;
    private PositionSampler sampler;

    // for debugging
    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(skeleton.COM, 0.01f);
        Gizmos.DrawRay(skeleton.COM, jumping.last_err);
    }
    
    void Start() {
        // initial estimate of path/velocity to get from start to end
        // calculation of intermediate goal states from starting point and final goal
        
        jumping.init();

        if (!motor) {
            motor = gameObject.AddComponent<JumpMotor>() as JumpMotor;
        }
        logger.StartAll();
        
        IK = GetComponent<InverseKinematics>();
        sampler = GetComponent<PositionSampler>();
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
        bool inputJump = Input.GetKey(controls.jump) || Input.GetKey(controls.simulation.start);
        if (inputJump) {
            Debug.Log("JUMP!");
        }
        
        skeleton.UpdateCOM();
        skeleton.UpdateSupportingPoly();

        // Downward/windup phase OR
        // Upward/accel phase OR if done
        // In Air
        // Landing
        //if (inputJump && jumping.state == JumpState.NotJumping) {
        if (inputJump && jumping.state == JumpState.NotJumping) {
            bool estimate_flag = EstimatePath();
            Debug.Log("Path Estimate: " + estimate_flag);

            if (estimate_flag) {
                Debug.Log("Not Jumping --> Path Estimate --> Windup");
                Debug.Log("Estimated accel: " + jumping.acceleration.ToString("G4"));
                jumping.state = JumpState.WindUp;
                // collect a sample field
                sampler.SampleHipPositions();
            }
            else {
                Debug.Log("Impossible jump given min and max possible");
                jumping.state = JumpState.NotJumping;
            }
        }
        else if (jumping.state == JumpState.WindUp) {
            bool windup_flag = Windup();
            Debug.Log("Windup: " + windup_flag);

            if (windup_flag) {
                Debug.Log("Windup --> Accel");
                jumping.state = JumpState.Accel;
                
                // log info
                Debug.Log("Logging to file: " + logger.files[0].filename);
                List<string> data_windup = new List<string>();
                data_windup.Add(jumping.acceleration.ToString("G4"));
                for (int i = 0; i < 8; i++) {
                    data_windup.Add(skeleton[i].Angle().ToString("G4"));
                }
                logger.files[0].AddRow(data_windup);
            }

            IK.Iterate();
        }
        else if (jumping.state == JumpState.Accel) {
            bool accel_flag = Accel();
            Debug.Log("Accel: " + accel_flag);

            if (accel_flag) {
                Debug.Log("Accel --> In Air");
                jumping.state = JumpState.InAir;
            }

            IK.Iterate();
        }
        else if (jumping.state == JumpState.InAir) {
            bool inAir_flag = motor.IsGrounded();
            Debug.Log("InAir: " + inAir_flag);

            if (motor.IsGrounded()) {
                Debug.Log("In Air --> Landing");
                jumping.state = JumpState.Landing;
            }
        }
        else if (jumping.state == JumpState.Landing) {
            bool landing_flag = Landing();
            Debug.Log("Landing: " + landing_flag);

            if (landing_flag) {
                Debug.Log("Landing --> Not Jumping");
                jumping.state = JumpState.NotJumping;
            }
        }

        // Dump all of the data of this iteration
        Debug.Log("Logging to file: " + logger.files[1].filename);
        List<string> data_pd = new List<string>();
        data_pd.Add(jumping.acceleration.ToString("G4"));
        for (int i = 0; i < 8; i++) {
            data_pd.Add(skeleton[i].Angle().ToString("G4"));
            data_pd.Add(skeleton[i].Angle().ToString("G4"));
        }
        logger.files[1].AddRow(data_pd);
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
        jumping.force = jumping.acceleration * skeleton.TotalMass();
        Debug.Log("Estimated Accel: " + jumping.acceleration);
        return jumping.IsAccelerationPossible(jumping.acceleration);
    }

    public Vector3 BalanceError() {
        //Debug.Log("Supp center: " + skeleton.support_center);
        //Debug.Log("COM: " + skeleton.COM);
        return (skeleton.support_center - skeleton.COM);
    }
        
    public Vector3 AccelError() {
        // compare resultant angular acceleration to expected acceleration from force
        Vector3 skel_accel = skeleton.acceleration(jumping.windup_time);
        Debug.Log("cur accel: " + skel_accel + ";\ntarget: " + jumping.acceleration + ";\nerror: " + (jumping.acceleration - skel_accel));
        return (jumping.acceleration - skel_accel);
    }
    
    public Vector3 DirToAchieveAccel() {
        List<PositionSample> diffList = sampler.samples.Select(s => new PositionSample(s.pelvisPosition, jumping.acceleration - s.resultantAccel)).ToList();

        // unpretty loop as there isn't really a reasonable way of doing this
        // with linq
        Vector3 min = diffList[0].resultantAccel;
        Vector3 minPos = diffList[0].pelvisPosition;
        for (int i = 0; i < diffList.Count(); ++i) {
            //if (min.sqrMagnitude + minPos.sqrMagnitude > diffList[i].resultantAccel.sqrMagnitude + (minPos - skeleton.Pelvis.Position()).sqrMagnitude) {
            if (min.sqrMagnitude > diffList[i].resultantAccel.sqrMagnitude) {
                min = diffList[i].resultantAccel;
                minPos = diffList[i].pelvisPosition;
            }
        }

        return (minPos - skeleton.Pelvis.Position()).normalized;
    }

    // function to handle the windup phase
    // returns true when finished
    bool Windup() {
        //Vector3 servo_modification = windupPD.modify(BalanceError());
        //Vector3 servo_modification = windupPD.modify(AccelError());
        Vector3 servo_modification = windupPD.modify(DirToAchieveAccel());
        
        skeleton.PositionPelvis(servo_modification);
        
        // TODO propagate the change to the skeleton
        float accel_err = Mathf.Abs(AccelError().magnitude);
        float bal_err = Mathf.Abs(BalanceError().magnitude);
        
        float total_err = accel_err + bal_err;
        jumping.last_err = servo_modification;

        return total_err <= jumping.error_allowance;
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
