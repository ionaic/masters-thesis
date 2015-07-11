using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PositionSample {
    public Vector3 pelvisPosition;
    public Vector3 resultantAccel;
    
    public PositionSample() {
        pelvisPosition = new Vector3();
        resultantAccel = new Vector3();
    }
    
    public PositionSample(Vector3 pos, Vector3 accel) {
        pelvisPosition = pos;
        resultantAccel = accel;
    }
}

public class PositionSampler : MonoBehaviour {
    public float step;
    public JumpController controller;
    public CustomInputManager controls;
    private InverseKinematics ikmanager;
    private List<Vector3> dbg_pos;
    public List<PositionSample> samples;
        
    private Vector3[] aabb;
    
    public JumpLogger logger;

    void Start() {
        logger.StartAll();
        ikmanager = GetComponent<InverseKinematics>();
        samples = new List<PositionSample>();
    }

	void Update() {
        if (Input.GetKey(controls.sampling.start)) {
            SampleHipPositions();
        }
	}

    void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        if (aabb != null) {
            foreach (Vector3 point in aabb) {
                Gizmos.DrawSphere(point, 0.01f);
            }
        }
        
        Gizmos.color = Color.green;
        if (controller.skeleton.supportingPoly != null) {
            foreach (Vector3 point in controller.skeleton.supportingPoly) {
                Gizmos.DrawSphere(point, 0.01f);
            }
        }
        
        Gizmos.color = Color.yellow;
        if (dbg_pos != null) {
            foreach (Vector3 v in dbg_pos) {
                Gizmos.DrawSphere(v, 0.01f);
            }
        }
    }
    
    void WriteSampleToLogs() {
        Debug.Log("Writing to sample logs");
        string balance_err = controller.BalanceError().ToString("G4");
        dbg_pos.Add(controller.skeleton.Pelvis.Position());

        List<string> data_angle = new List<string>();
        List<string> data_force = new List<string>();
        List<string> data_torque = new List<string>();
        List<string> data_position = new List<string>();

        data_position.Add(balance_err);
        data_position.Add(controller.skeleton.LHip.Position().ToString("G4"));
        data_position.Add(controller.skeleton.RHip.Position().ToString("G4"));
        data_position.Add(controller.skeleton.LKnee.Position().ToString("G4"));
        data_position.Add(controller.skeleton.RKnee.Position().ToString("G4"));
        data_position.Add(controller.skeleton.LAnkle.Position().ToString("G4"));
        data_position.Add(controller.skeleton.RAnkle.Position().ToString("G4"));

        data_angle.Add(balance_err);
        data_angle.Add(controller.skeleton.LHip.Angle().ToString("G4"));
        data_angle.Add(controller.skeleton.RHip.Angle().ToString("G4"));
        data_angle.Add(controller.skeleton.LKnee.Angle().ToString("G4"));
        data_angle.Add(controller.skeleton.RKnee.Angle().ToString("G4"));
        data_angle.Add(controller.skeleton.LAnkle.Angle().ToString("G4"));
        data_angle.Add(controller.skeleton.RAnkle.Angle().ToString("G4"));
        
        data_force.Add(balance_err);
        float total_force = 0.0f;
        foreach (SpringMuscle musc in controller.skeleton.muscles) {
            float force = musc.scalarForce();
            data_force.Add(force.ToString("G4"));
            data_torque.Add(musc.torque(force).ToString("G4"));
            
            total_force += force;
        }
        data_force.Add(total_force.ToString("G4"));
        data_force.Add(controller.skeleton.acceleration(controller.jumping.windup_time).ToString("G4"));

        logger.files[0].AddRow(data_angle);
        logger.files[1].AddRow(data_force);
        logger.files[2].AddRow(data_torque);
        logger.files[3].AddRow(data_position);
    }
    
    void CollectSample() {  
        PositionSample sample = new PositionSample();
        sample.pelvisPosition = controller.skeleton.Pelvis.Position();
        sample.resultantAccel = controller.skeleton.acceleration(controller.jumping.windup_time);
        samples.Add(sample);
    }
    
    public void SampleHipPositions() {
        dbg_pos = new List<Vector3>();

        controller.skeleton.UpdateCOM();
        controller.skeleton.UpdateSupportingPoly();
        TransformData[] reset = controller.skeleton.GetResetArray();
        
        List<Vector3> tmp = new List<Vector3>(controller.skeleton.supportingPoly);
        tmp.Add(controller.skeleton.Pelvis.Position());
        aabb = JumpUtil.minAABB3d(tmp.ToArray());

        // use the min corner of the box
        Vector3 base_pos = aabb[0];

        Debug.Log("AABB: " + JumpUtil.ArrayToString(aabb));

        // the interval length will always be the bounding box's last element -
        // first element
        Vector3 aabb_size = aabb[7] - aabb[0];
        int sample_width = (int)(aabb_size.x / step) + 1,
            sample_height = (int)(aabb_size.y / step) + 1,
            sample_depth = (int)(aabb_size.z / step) + 1;
        Debug.Log("Num samples: (" + sample_width + ", " + sample_height + ", " + sample_depth + ")");

        for (int swidth = 0; swidth < sample_width; swidth++) {
            for (int sdepth = 0; sdepth < sample_depth; sdepth++) {
                for (int sheight = 0; sheight < sample_height; sheight++) {
                    Vector3 displacement = new Vector3(swidth * step, sheight * step, sdepth * step);
                    // move the pelvis
                    controller.skeleton.Pelvis.Position(base_pos + displacement);
                    // update values to match new position
                    ikmanager.Iterate();
                    controller.skeleton.UpdateCOM();
                    controller.skeleton.UpdateSupportingPoly();
                    // write the sample
                    WriteSampleToLogs();
                    CollectSample();
                }
            }
        }
        
        //controller.skeleton.Pelvis.Position(base_pos);
        //ikmanager.Iterate();
        controller.skeleton.ResetFromArray(reset);
    }
	
	// Update is called once per frame
	void SampleAngles() {
        Vector3 astep = new Vector3(step, step, step);
        // the joints generally have one axis of rotation, so we are
        // assuming that each only has one allowed
        for (controller.skeleton.LHip.RotateToMin(), controller.skeleton.RHip.RotateToMin(); 
            !controller.skeleton.LHip.AtMax() && !controller.skeleton.RHip.AtMax(); 
            controller.skeleton.LHip.Rotate(astep), controller.skeleton.RHip.Rotate(astep)) {
            for (controller.skeleton.LKnee.RotateToMin(), controller.skeleton.RKnee.RotateToMin(); 
                !controller.skeleton.LKnee.AtMax() && !controller.skeleton.RKnee.AtMax(); 
                controller.skeleton.LKnee.Rotate(astep), controller.skeleton.RKnee.Rotate(astep)) {
                WriteSampleToLogs();
            }
        }
	}
}
