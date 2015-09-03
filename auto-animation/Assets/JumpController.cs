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
    //public TransformData[] restPose;
    public Vector3 pelvisRestPos;

    public JumpState state;
    public PathSolutionPolicy policy;
    
    public void init(ConstrainedPhysicalControllerSkeleton skeleton) {
        destination = target_location.position;

        state = JumpState.NotJumping;

        //restPose = skeleton.GetResetArray();
        pelvisRestPos = skeleton.Pelvis.Position();
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
    public PIDServo balancePD;
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
        
        jumping.init(skeleton);

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
                sampler.LogSamples();
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
        return jumping.IsAccelerationPossible(jumping.acceleration);
    }

    public Vector3 BalanceError() {
        //Debug.Log("Supp center: " + skeleton.support_center);
        //Debug.Log("COM: " + skeleton.COM);
        Vector3 err = skeleton.support_center - skeleton.COM;
        err.z = 0.0f;
        return err;
    }
    
    public void UpperBodyBalance(Vector3 err) {
        // move the upper body to rebalance the character
        // TODO
        // rebalance by calculating the amount to move the upper body to reposition center of mass
        // need a 2d back and forth for balancing, so rotating so upper body is more forward or more backward
        // +theta moves more forward, -theta moves more backward
        
        Vector3 rotation_amt = balancePD.modify(err);
        
        Debug.Log("Upper Body Balance rotation: " + rotation_amt);
    
        skeleton.Pelvis.Rotate(rotation_amt);
        
        // TODO can we just rotate the hip joints back to compensate, it's a single level...
        IK.Iterate();
    }
    
    // estimation of the balance error at this new position
    public Vector3 EstimateBalanceError(Vector3 pos) {
        Vector3 err = skeleton.support_center - pos;
        // NOTE assumes that feet are flat and that y is the vertical
        err.y = 0.0f;
        return err;
    }
        
    public Vector3 AccelError() {
        // compare resultant angular acceleration to expected acceleration from force
        Vector3 skel_accel = skeleton.acceleration(jumping.windup_time);
        //Debug.Log("cur accel: " + skel_accel + ";\ntarget: " + jumping.acceleration + ";\nerror: " + (jumping.acceleration - skel_accel));
        return (jumping.acceleration - skel_accel);
    }
    
    public Vector3 DirToAchieveAccel() {
        // take the differences between desired acceleration and current acceleration
        List<PositionSample> diffList = sampler.samples.Select(s => new PositionSample(s.pelvisPosition, jumping.acceleration - s.resultantAccel)).ToList();
        // order by the difference in acceleration
        diffList = diffList.OrderBy(s => s.resultantAccel.sqrMagnitude).ToList();
        
        Debug.Log("Selected sample: " + diffList[0].pelvisPosition);
        
        // get the top 10
        //List<PositionSample> shortList = diffList.GetRange(0, 10);
        //shortList = shortList.OrderBy(s => EstimateBalanceError(s.pelvisPosition).sqrMagnitude).ToList();

        // take the closest point in the top 10 of lowest accel differences
        //return (diffList[0].pelvisPosition - skeleton.Pelvis.Position()).normalized;
        return diffList[0].pelvisPosition - skeleton.Pelvis.Position();
    }

    Vector3 PickFirstClosestWithPos() {
        List<PositionSample> diffList = sampler.samples.Select(s => new PositionSample(s.pelvisPosition, jumping.acceleration - s.resultantAccel)).ToList();

        // unpretty loop as there isn't really a reasonable way of doing this
        // with linq
        Vector3 min = diffList[0].resultantAccel;
        Vector3 minPos = diffList[0].pelvisPosition;
        for (int i = 0; i < diffList.Count(); ++i) {
            if (min.sqrMagnitude + (jumping.pelvisRestPos - minPos).sqrMagnitude > diffList[i].resultantAccel.sqrMagnitude + (minPos - skeleton.Pelvis.Position()).sqrMagnitude) {
                min = diffList[i].resultantAccel;
                minPos = diffList[i].pelvisPosition;
            }
        }

        return (minPos - skeleton.Pelvis.Position()).normalized;
    }
    
    Vector3 PickFirstClosest() {
        List<PositionSample> diffList = sampler.samples.Select(s => new PositionSample(s.pelvisPosition, jumping.acceleration - s.resultantAccel)).ToList();

        // unpretty loop as there isn't really a reasonable way of doing this
        // with linq
        Vector3 min = diffList[0].resultantAccel;
        Vector3 minPos = diffList[0].pelvisPosition;
        for (int i = 0; i < diffList.Count(); ++i) {
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
        
        UpperBodyBalance(BalanceError());
        
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
