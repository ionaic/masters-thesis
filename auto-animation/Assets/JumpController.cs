using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// scalar energy, combine by adding; spring energy is E=kx^2?
// several for the body and just calculate the energy required to make a vert jump

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
    // this ended up replacing a JumpMotor
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
    public float windup_time; // times are in seconds
    public float air_time;
    public Vector3 gravity;
    public Vector3 drag;
    [HideInInspector]
    public Vector3 last_err;
    //public TransformData[] restPose;
    public Vector3 pelvisRestPos;
    [HideInInspector]
    public Vector3 velocity;
    [HideInInspector]
    public Vector3 takeoff_velocity;

    [HideInInspector]
    public JumpState state;
    public PathSolutionPolicy policy;
    
    public void init(ConstrainedPhysicalControllerSkeleton skeleton) {
        destination = target_location.position;
        start = 0.5f * skeleton.LToe.Position() + 0.5f * skeleton.RToe.Position();

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
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(skeleton.Pelvis.transform.position, jumping.acceleration);
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
            
                jumping.takeoff_velocity = Vector3.zero;
            }

            IK.Iterate();
        }
        else if (jumping.state == JumpState.Accel) {
            bool accel_flag = Accel();
            Debug.Log("Accel: " + accel_flag);

            if (accel_flag) {
                Debug.Log("Accel --> In Air");
                jumping.state = JumpState.InAir;
                jumping.velocity = jumping.acceleration * jumping.windup_time;
            }

            IK.Iterate();
        }
        else if (jumping.state == JumpState.InAir) {
            bool inAir_flag = InAir();
            Debug.Log("InAir: " + inAir_flag);

            if (inAir_flag) {
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

    public Vector3 CalculateRequiredVelocity() {
        // W = mgh
        // the distance you travel causes work when taking off
        // this work then results in an energy in air which is 1/2 m * v * v
        // so W = F * d = E - 0 = 1/2 m * v * v
        // --> F = (1/2 m * v^2) / d
        // --> ma = (1/2 * m * v^2) / d --> a = (1/2 v^2) / d
        // --> a = v^2/(2d)
        //* OR --> d = (m * v^2) / (2F)
        // for the takeoff, v = v0 + at --> at = v - v0 
        //* --> a = (v-v0)/t
        // x = x0 + v0t + 1/2at^2 --> x = x0 + vt + 1/2 g t^2
        // --> vt = x0 - x + 1/2 g t^2 
        //* --> v = (x0 - x + 1/2gt^2) / t
        //* --> v = (x0 - x)/t + (g t)/2
        // known: m, takeoff t, in-air t, 
        
        Vector3 velocity = (jumping.start - jumping.destination) / jumping.air_time + (jumping.gravity * jumping.air_time) / 2;
        return velocity;
    }
    public Vector3 CalculatePelvisDisplacement(Vector3 velocity) {
        Vector3 acceleration = (velocity - jumping.initial_velocity) / jumping.windup_time;
        float displacement = (skeleton.TotalMass() * (Vector3.Dot(velocity, velocity) - Vector3.Dot(jumping.initial_velocity, jumping.initial_velocity))) / (2 * acceleration.magnitude);
        Vector3 dir = acceleration.normalized;
        return displacement * dir;
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
            jumping.acceleration = 2.0f * (jumping.destination - jumping.start - jumping.initial_velocity * jumping.air_time) / (jumping.air_time * jumping.air_time);
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
    
    // Tá t-earráid anseo, ach cá bhfuil é
    public void UpperBodyBalance(Vector3 err) {
        // move the upper body to rebalance the character
        // TODO
        // rebalance by calculating the amount to move the upper body to reposition center of mass
        // need a 2d back and forth for balancing, so rotating so upper body is more forward or more backward
        // +theta moves more forward, -theta moves more backward
        
        Vector3 rotation_amt = balancePD.modify(err);
        
        Debug.Log("Upper Body Balance rotation: " + rotation_amt);
    
        //skeleton.Pelvis.Rotate(rotation_amt.x, rotation_amt.z, rotation_amt.y);
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
        Debug.Log("Skel Accel | needed: " + skel_accel + " | " + jumping.acceleration);
        //Debug.Log("cur accel: " + skel_accel + ";\ntarget: " + jumping.acceleration + ";\nerror: " + (jumping.acceleration - skel_accel));
        return (jumping.acceleration - skel_accel);
    }
    
    public Vector3 DirToAchieveAccel() {
        // use a dot product, if the dot product is >= the desired magnitude, we're a go
        List<PositionSample> diffList = sampler.samples.Where(s => Vector3.Dot(jumping.acceleration, s.resultantAccel) >= jumping.acceleration.magnitude).ToList();
        diffList.OrderBy(s => EstimateBalanceError(s.COM).sqrMagnitude);
        if (diffList.Count() > 0) {
            return diffList.First().pelvisPosition - skeleton.Pelvis.Position();
        }
        else {
            return skeleton.Pelvis.Position();
        }
    }

    Vector3 PickFirstClosestWithPos() {
        List<PositionSample> diffList = sampler.samples.Select(s => new PositionSample(s.pelvisPosition, jumping.acceleration - s.resultantAccel, s.COM)).ToList();

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
        List<PositionSample> diffList = sampler.samples.Select(s => new PositionSample(s.pelvisPosition, jumping.acceleration - s.resultantAccel, s.COM)).ToList();

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
        
        float accel_err = AccelError().magnitude;
        float bal_err = BalanceError().magnitude;
        bal_err = 0.0f;
    
        Vector3 accel_diff = AccelError();
        bool x_bound = Mathf.Sign(accel_diff.x) == Mathf.Sign(jumping.acceleration.x);
        bool y_bound = Mathf.Sign(accel_diff.y) == Mathf.Sign(jumping.acceleration.y);
        bool z_bound = Mathf.Sign(accel_diff.z) == Mathf.Sign(jumping.acceleration.z);
    
        x_bound = x_bound && (Mathf.Abs(accel_diff.x) < Mathf.Abs(jumping.acceleration.x));
        y_bound = y_bound && (Mathf.Abs(accel_diff.y) < Mathf.Abs(jumping.acceleration.y));
        z_bound = z_bound && (Mathf.Abs(accel_diff.z) < Mathf.Abs(jumping.acceleration.z));

        Debug.Log("bounds (x, y, z) " + accel_diff + " >= " + jumping.acceleration + ": (" + x_bound + ", " + y_bound + ", " + z_bound + ")");
        
        float total_err = accel_err + bal_err;
        jumping.last_err = servo_modification;

        return total_err <= jumping.error_allowance;
    }

    // function to handle accel phase
    bool Accel() {
        // time and deltaTime are both given in seconds
        //float rate = Time.deltaTime/jumping.windup_time;
        
        // slowly straighten out the pelvis bend to straighten upper body
        //skeleton.Pelvis.jointTransform.rotation = Quaternion.Slerp(skeleton.Pelvis.jointTransform.rotation, skeleton.Pelvis.RestRotation(), rate);
        
        // move pelvis in direction of acceleration
        skeleton.Pelvis.Translate(jumping.takeoff_velocity * Time.deltaTime);
        jumping.takeoff_velocity += jumping.acceleration * Time.deltaTime;

        // iterate IK
        IK.Iterate();

        // check if we are fully extended, if so you're done. go home.
        return skeleton.CheckExtension();
    }

    // function to handle the acceleration/upward phase
    bool InAir() {
        Vector3 vdt = jumping.velocity * Time.deltaTime;
        // apply the jump to the controller
        transform.Translate(vdt.x, 0.0f, vdt.z);
        // reposition skeleton within controller
        skeleton.Pelvis.Translate(0.0f, vdt.y, 0.0f);
        // reposition feet seeking IK targets (do we need to?)
        // update the velocity
        // gravity is negative by convention
        jumping.velocity += jumping.gravity;
        return true;
    }

    // function to handle landing
    bool Landing() {
        // NOOP since we're not handling landing
        // reset simulation to beginning
        return false;
    }
}
