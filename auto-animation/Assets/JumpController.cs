using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// scalar energy, combine by adding; spring energy is E=kx^2?
// several for the body and just calculate the energy required to make a vert jump

[System.Serializable]
public enum JumpState {
    NotJumping,
    WindUp,
    Accel,
    InAir,
    Landing
}

[System.Serializable]
public enum SimulationType {
    Torque,
    Energy
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
    //[HideInInspector]
    public Vector3 acceleration;
    [HideInInspector]
    public Vector3 force;
    public float error_allowance;
    public float windup_time; // times are in seconds
    public float air_time;
    public Vector3 gravity;
    public Vector3 drag;
    [HideInInspector]
    public Vector3 last_err;
    public Vector3 pelvisRestPos;
    //[HideInInspector]
    public Vector3 velocity;
    [HideInInspector]
    public Vector3 takeoff_velocity;
    public PositionSample selectedSample;

    [HideInInspector]
    public JumpState state;
    public SimulationType simType;
    
    public void init(ConstrainedPhysicalControllerSkeleton skeleton) {
        destination = target_location.position;
        start = 0.5f * skeleton.LToe.Position() + 0.5f * skeleton.RToe.Position();

        state = JumpState.NotJumping;

        pelvisRestPos = skeleton.Pelvis.Position();
    }
    
    public bool IsAccelerationPossible(List<PositionSample> samples) {
        foreach (PositionSample s in samples) {
            if (s.accelError <= error_allowance) {
                return true;
            }
        }
        return false;
    }
}

[RequireComponent (typeof(InverseKinematics))]
[RequireComponent (typeof(PositionSampler))]
[RequireComponent (typeof(CameraView))]
public class JumpController : MonoBehaviour {
    // handle calculation of estimated path, joint contortions, "what to do"
    // handle button pushes etc.
    public float global_k;
    public CustomInputManager controls;
    public JumpVariables jumping;
    public PIDServo windupPD;
    public PIDServo balancePD;
    public PIDServo landingPD;
    public ConstrainedPhysicalControllerSkeleton skeleton;
    public JumpMotor motor;
    public JumpLogger logger;
    private InverseKinematics IK;
    private PositionSampler sampler;
    [HideInInspector]
    public CameraView cameraView;
    public float secondsBetweenFrames = 1.0f;
    public float timeElapsed = 0.0f;
    private float timeSinceSample = 0.0f;

    // for debugging
    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(skeleton.COM, 0.01f);
        Gizmos.DrawRay(skeleton.COM, jumping.last_err);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(skeleton.Pelvis.transform.position, jumping.acceleration);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(skeleton.Pelvis.transform.position, jumping.acceleration * jumping.windup_time);
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(skeleton.Pelvis.transform.position, jumping.acceleration * jumping.windup_time * jumping.windup_time);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(jumping.start, jumping.destination - jumping.start);
        if (jumping.simType == SimulationType.Energy) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(skeleton.Pelvis.transform.position, 0.5f * skeleton.TotalMass() * jumping.velocity.sqrMagnitude * transform.forward);
        }
        Gizmos.color = Color.grey;
        if (sampler != null && sampler.samples != null) {
            if (jumping.simType == SimulationType.Torque) {
                foreach (PositionSample sample in sampler.samples) {
                    Gizmos.DrawRay(sample.pelvisPosition, sample.resultantAccel);
                }
            }
            else if (jumping.simType == SimulationType.Energy) {
                foreach (PositionSample sample in sampler.samples) {
                    Gizmos.DrawRay(sample.pelvisPosition, transform.forward * sample.totalEnergy);
                }
            }
        }
    }
    
    void Start() {
        // initial estimate of path/velocity to get from start to end
        // calculation of intermediate goal states from starting point and final goal
        
        jumping.init(skeleton);

        if (!motor) {
            motor = gameObject.AddComponent<JumpMotor>() as JumpMotor;
        }
        logger.StartAll();
        
        cameraView = GetComponent<CameraView>();
        
        IK = GetComponent<InverseKinematics>();
        sampler = GetComponent<PositionSampler>();
        
        foreach (SpringMuscle m in skeleton.muscles) {
            m.k = global_k;
        }

        // set logfile for displacements to have a column for each muscle
        sampler.logger.files[5].columns = new string[skeleton.muscles.Length + 1];
        sampler.logger.files[5].columns[0] = "Pelvis Position";
        
        // setup the columns for displacements
        for (int i = 0; i < skeleton.muscles.Length; ++i) {
            sampler.logger.files[5].columns[i+1] = skeleton.muscles[i].muscleName;
        }
        
        // re-initialize this file now that it has the proper columns
        sampler.logger.files[5].StartLog();
    }
    
    // TODO I should probably be using fixedupdate
    void Update() {
        if (jumping.state != JumpState.NotJumping) {
            timeElapsed += Time.fixedDeltaTime;
        }

        if (Input.GetKey(controls.cam.sideView)) {
            cameraView.UseSideView();
        }
        else if (Input.GetKey(controls.cam.frontView)) {
            cameraView.UseFrontView();
        }
        else if (Input.GetKey(controls.cam.slantView)) {
            cameraView.UseSlantView();
        }
        else if (Input.GetKey(controls.cam.trackingView)) {
            cameraView.UseTrackingView();
        }

        if (Input.GetKey(controls.cam.screenshot)) {
            cameraView.TakeScreenshot();
        }
        if (Input.GetKey(controls.cam.frameset)) {
            cameraView.GrabFrameSet();
        }
        if (Input.GetKey(controls.simulation.torqueBased)) {
            jumping.simType = SimulationType.Torque;
        }
        if (Input.GetKey(controls.simulation.energyBased)) {
            jumping.simType = SimulationType.Energy;
        }
        
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
        if (inputJump && jumping.state == JumpState.NotJumping) {
            timeElapsed = 0.0f;
            cameraView.GrabFrameSet();
            
            if (sampler.samples.Count() <= 0) {
                // collect a sample field
                sampler.SampleHipPositions();
                sampler.LogSamples();
            }

            bool estimate_flag = false; 
            if (jumping.simType == SimulationType.Torque) {
                estimate_flag = EstimatePath();
            }
            else if (jumping.simType == SimulationType.Energy) {
                estimate_flag = CalculateRequiredVelocity();
                SelectEnergySample();
                Debug.Log("Velocity estimate: " + jumping.velocity);
            }
            Debug.Log("Path Estimate: " + estimate_flag);

            if (estimate_flag) {
                Debug.Log("Not Jumping --> Path Estimate --> Windup");
                Debug.Log("Estimated accel: " + jumping.acceleration.ToString("G4"));
                jumping.state = JumpState.WindUp;
            }
            else {
                Debug.Log("Impossible jump given min and max possible");
                jumping.state = JumpState.NotJumping;
            }
        }
        else if (jumping.state == JumpState.WindUp) {
            bool windup_flag = false;
            if (jumping.simType == SimulationType.Torque) {
                windup_flag = Windup();
            }
            else if (jumping.simType == SimulationType.Energy) {
                windup_flag = EnergyWindup();
            }
            Debug.Log("Windup: " + windup_flag);

            if (windup_flag) {
                Debug.Log("Windup --> Accel");
                jumping.state = JumpState.Accel;
                
                if (jumping.simType == SimulationType.Energy) {
                    // if this is an energy based sim we don't have an acceleration yet
                    // W = Fd
                    // E_change = m a d
                    jumping.acceleration = (jumping.velocity - jumping.initial_velocity) / jumping.windup_time;
                }
                
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
                // grab one last set of frames at the end
                cameraView.GrabFrameSet();
            }
            Debug.Log("Total Simulation Time: " + timeElapsed);
            sampler.simulationTimes.Add(timeElapsed);
            timeElapsed = 0.0f;
        }

        // Dump all of the data of this iteration
        List<string> data_pd = new List<string>();
        data_pd.Add(jumping.acceleration.ToString("G4"));
        for (int i = 0; i < 8; i++) {
            data_pd.Add(skeleton[i].Angle().ToString("G4"));
            data_pd.Add(skeleton[i].Angle().ToString("G4"));
        }
        logger.files[1].AddRow(data_pd);

        if (jumping.state != JumpState.NotJumping) {
            if (timeSinceSample >= secondsBetweenFrames) {
                cameraView.GrabFrameSet();
                timeSinceSample = 0.0f;
            }
            else {
                timeSinceSample += Time.fixedDeltaTime;
            }
        }
    }
    
    public bool CalculateRequiredVelocity() {
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
        
        jumping.velocity = (jumping.destination - jumping.start) / jumping.air_time - (jumping.gravity * jumping.air_time) / 2.0f;
        return true;
    }

    bool SelectEnergySample() {
        float E_k = 0.5f * skeleton.TotalMass() * jumping.velocity.sqrMagnitude;
        List<PositionSample> diffList = sampler.samples.Where(s => E_k - s.totalEnergy <= jumping.error_allowance).ToList();
        diffList.OrderBy(s => EstimateBalanceError(s.COM).sqrMagnitude);
        Debug.Log("Viable Sample Count " + diffList.Count());
        if (diffList.Count() > 0) {
            jumping.selectedSample = diffList.First();
        }
        else {
            return false;
        }
        return true;
    }

    bool EnergyWindup() {
        // solve the issue of how to load the springs
        // let's pick samples again!
        Vector3 direction;
        // KE is 1/2 m v^2
        float E_k = 0.5f * skeleton.TotalMass() * jumping.velocity.sqrMagnitude;
        Debug.Log("m v^2: " + skeleton.TotalMass() + " " + jumping.velocity.sqrMagnitude + " v: " + jumping.velocity);
        Debug.Log("Kinetic Energy: " + E_k);

        // take our selected sample and move towards it
        direction = (jumping.selectedSample.pelvisPosition - skeleton.Pelvis.Position()).normalized;
        
        Vector3 servo_modification = windupPD.modify(direction);
        Debug.Log("Move Direction " + direction + " modified to: " + servo_modification.ToString("G4"));
        
        UpperBodyBalance(BalanceError());
        
        skeleton.PositionPelvis(servo_modification * Time.fixedDeltaTime);
        
        float err = E_k - skeleton.ElasticEnergy();
        Debug.Log("Energy Err " + err + " <= " + jumping.error_allowance + " " + skeleton.ElasticEnergy() + " >= " + E_k);
        
        return err <= jumping.error_allowance * E_k;
    }

    public Vector3 CalculatePelvisDisplacement(Vector3 velocity) {
        Vector3 acceleration = (velocity - jumping.initial_velocity) / jumping.windup_time;
        float displacement = (skeleton.TotalMass() * (Vector3.Dot(velocity, velocity) - Vector3.Dot(jumping.initial_velocity, jumping.initial_velocity))) / (2 * acceleration.magnitude);
        Vector3 dir = acceleration.normalized;
        return displacement * dir;
    }

    // function to handle path estimate
    bool EstimatePath() {
        bool flag = CalculateRequiredVelocity();
        if (!flag) {
            return flag;
        }
        jumping.acceleration = (jumping.velocity - jumping.initial_velocity) / jumping.windup_time;
        //jumping.acceleration = 2.0f * (jumping.destination - jumping.start - jumping.initial_velocity * jumping.windup_time) / (jumping.windup_time * jumping.windup_time) + (-1.0f * jumping.air_time * jumping.gravity) / (2.0f * jumping.windup_time);

        jumping.force = jumping.acceleration * skeleton.TotalMass();

        return jumping.IsAccelerationPossible(sampler.samples);
    }

    public Vector3 BalanceError() {
        // balance should center the center of mass over the supporting polygon centroid
        Vector3 err = skeleton.support_center - skeleton.COM;
        
        // the y error (vertical) is 0
        err.y = 0.0f;

        return err;
    }
    
    // Tá t-earráid anseo, ach cá bhfuil é
    public void UpperBodyBalance(Vector3 err) {
        // move the upper body to rebalance the character
        // rebalance by calculating the amount to move the upper body to reposition center of mass
        // need a 2d back and forth for balancing, so rotating so upper body is more forward or more backward
        // +theta moves more forward, -theta moves more backward
        
        Vector3 rotation_amt = balancePD.modify(err);
        
        Debug.Log("Upper Body Balance rotation: " + rotation_amt);
    
        skeleton.Pelvis.Rotate(rotation_amt);
        
        IK.Iterate();
    }
    
    // estimation of the balance error at this new position
    public Vector3 EstimateBalanceError(Vector3 pos) {
        Vector3 err = skeleton.support_center - pos;
        // NOTE assumes that feet are flat and that y is the vertical
        err.y = 0.0f;
        return err;
    }

    public float AccelError(Vector3 accel) {
        // dot product is |A| |B| scaled by cos, and I want dot prod to be >=
        // |A||A| as that means they are codirectional or close enough and that
        // |B| >= |A| and is large enough to compensate for non-co-directional
        
        //Debug.Log("AccelError " + jumping.acceleration.sqrMagnitude + " " + Vector3.Dot(jumping.acceleration, accel) + "; Between calc: " + accel + " and desired: " + jumping.acceleration);

        // this will have + error if adjustment is needed, error <= 0 if ok
        return jumping.acceleration.sqrMagnitude - Vector3.Dot(jumping.acceleration, accel);
    }
    public float AccelError() {
        return AccelError(skeleton.acceleration(jumping.windup_time));
    }
    
    public Vector3 DirToAchieveAccel() {
        // use a dot product, if the dot product is >= the desired magnitude, we're a go
        List<PositionSample> diffList = sampler.samples.Where(s => AccelError(s.resultantAccel) <= jumping.error_allowance * s.resultantAccel.magnitude).ToList();
        diffList.OrderBy(s => EstimateBalanceError(s.COM).sqrMagnitude);
        Debug.Log("Viable Sample Count " + diffList.Count());
        if (diffList.Count() > 0) {
            return (diffList.First().pelvisPosition - skeleton.Pelvis.Position()).normalized;
        }
        else {
            return Vector3.zero;
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
        
        skeleton.PositionPelvis(servo_modification * Time.fixedDeltaTime);
        
        float accel_err = AccelError();
        Debug.Log("Accel Error: " + accel_err);
        if (accel_err <= 0.0f) {
            accel_err = 0.0f;
        }
        float bal_err = BalanceError().magnitude;
        bal_err = 0.0f;
        
        float total_err = accel_err + bal_err;
        jumping.last_err = servo_modification;

        return total_err <= jumping.error_allowance * jumping.acceleration.magnitude;
    }

    // function to handle accel phase
    bool Accel() {
        // move pelvis in direction of acceleration
        skeleton.Pelvis.Translate(jumping.takeoff_velocity * Time.fixedDeltaTime);
        jumping.takeoff_velocity += jumping.acceleration * Time.fixedDeltaTime;

        // iterate IK
        IK.Iterate();

        // check if we are fully extended, if so you're done. go home.
        return skeleton.CheckExtension() || !skeleton.IsGrounded();
    }

    // function to handle the acceleration/upward phase
    bool InAir() {
        Vector3 vdt = jumping.velocity * Time.fixedDeltaTime;
        // apply the jump to the controller
        transform.Translate(vdt.x, 0.0f, vdt.z);
        // reposition skeleton within controller
        skeleton.Pelvis.Translate(0.0f, vdt.y, 0.0f);
        // reposition feet seeking IK targets (do we need to?)
        // update the velocity
        // gravity is negative by convention
        jumping.velocity += jumping.gravity * Time.fixedDeltaTime;
        
        IK.Iterate();
        return skeleton.IsGrounded();
    }

    // function to handle landing
    bool Landing() {
        // NOOP since we're not handling landing
        // reset simulation to beginning
        // can assume the bend is from loading the springs with elastic potential transferred from the energy of the jump/fall
        return true;

        // this doesn't work so well
        // KE is 1/2 m v^2
        float E_k = 0.5f * skeleton.TotalMass() * jumping.velocity.sqrMagnitude;
    
        float E_diff = E_k - skeleton.ElasticEnergy();
        
        //Vector3 servo_modification = landingPD.modify(jumping.velocity.normalized * E_diff);
        Vector3 servo_modification = landingPD.modify(-1.0f * jumping.acceleration.normalized);
    
        Debug.Log("Landing servo mod " + servo_modification);
        
        UpperBodyBalance(BalanceError());
        
        skeleton.PositionPelvis(servo_modification * Time.fixedDeltaTime);
        
        return E_diff <= jumping.error_allowance * E_k;
    }
}
