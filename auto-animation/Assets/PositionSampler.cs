using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// for sqlite
using Mono.Data.Sqlite;
using System.Data;
using System;

[System.Serializable]
public class PositionSample {
    public Vector3 pelvisPosition;
    public Vector3 COM;
    public Vector3 resultantAccel;
    public float accelError;
    public float totalEnergy;
    
    public PositionSample() {
        pelvisPosition = new Vector3();
        resultantAccel = new Vector3();
        COM = new Vector3();
    }
    
    public PositionSample(Vector3 pos, Vector3 accel, Vector3 com) {
        pelvisPosition = pos;
        resultantAccel = accel;
        COM = com;
    }
    
    public List<string> ToStringList() {
        List<string> data = new List<string>();
        data.Add(pelvisPosition.ToString("G4"));
        data.Add(COM.ToString("G4"));
        data.Add(resultantAccel.ToString("G4"));
        data.Add(accelError.ToString("G4"));
        data.Add(totalEnergy.ToString("G4"));
        return data;
    }
}

// do i need this
//public class EnergySample {
//
//}

public class PositionSampler : MonoBehaviour {
    public float step;
    public JumpController controller;
    public CustomInputManager controls;
    private InverseKinematics ikmanager;
    private List<Vector3> dbg_pos;
    public List<PositionSample> samples;
    public List<float> simulationTimes;
        
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
            LogSamples();
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
        data_angle.Add(controller.skeleton.Pelvis.Position().ToString("G4"));
        data_angle.Add(controller.skeleton.Pelvis.Angle().ToString("G4"));
        data_angle.Add(controller.skeleton.LHip.Angle().ToString("G4"));
        data_angle.Add(controller.skeleton.LKnee.Angle().ToString("G4"));
        data_angle.Add(controller.skeleton.LAnkle.Angle().ToString("G4"));
        data_angle.Add(controller.skeleton.RHip.Angle().ToString("G4"));
        data_angle.Add(controller.skeleton.RKnee.Angle().ToString("G4"));
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
        sample.COM = controller.skeleton.COM;
        sample.accelError = controller.AccelError(sample.resultantAccel);
        sample.totalEnergy = controller.skeleton.ElasticEnergy();
        samples.Add(sample);
    }
    
    void CollectDisplacements() {
        List<string> data = new List<string>();
        data.Add(controller.skeleton.Pelvis.Position().ToString("G4"));
        foreach (SpringMuscle m in controller.skeleton.muscles) {
            data.Add(m.springDisplacement().ToString("G4"));
        }
        logger.files[5].AddRow(data);
    }
    
    public void LogSamples() {
        foreach (PositionSample s in samples) {
            logger.files[4].AddRow(s.ToStringList());
        }
    }
    
    public void SampleHipPositions() {
        dbg_pos = new List<Vector3>();

        controller.skeleton.UpdateCOM();
        controller.skeleton.UpdateSupportingPoly();
        TransformData[] reset = controller.skeleton.GetResetArray();
        
        //List<Vector3> tmp = new List<Vector3>(controller.skeleton.supportingPoly);
        List<Vector3> tmp = controller.skeleton.supportingPoly.Select(v => new Vector3(v.x, controller.skeleton.RKnee.Position().y, v.z)).ToList();
        // NOTE this assumes that the right knee height is the height of both knees and that the y direction is up/down
        // translate the bottom of the bounding box up to knee height (as usually you don't want knees to bend past 90 degrees anyways)
        //for (int i = 0; i < tmp.Count; ++i) {
        //    tmp[i].y = controller.skeleton.RKnee.Position().y;
        //}
        //tmp = tmp.Select(v => v.y = controller.skeleton.RKnee.Position().y).ToList();
        tmp.Add(controller.skeleton.Pelvis.Position());
            
        Debug.Log("Modified to knee position (" + controller.skeleton.RKnee.Position().y + "):" + JumpUtil.ArrayToString(tmp.ToArray()));

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
                    
                    CollectDisplacements();
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
