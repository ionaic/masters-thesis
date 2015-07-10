using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PositionSampler : MonoBehaviour {
    public float step;
    public JumpController controller;
    public CustomInputManager controls;
    private InverseKinematics ikmanager;
        
    private Vector3[] aabb;
    
    public JumpLogger logger;

    void Start() {
        logger.StartAll();
        ikmanager = GetComponent<InverseKinematics>();
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
    }
    
    void WriteSampleToLogs() {
        Debug.Log("Writing to sample logs");
        string balance_err = controller.BalanceError().ToString();

        List<string> data_angle = new List<string>();
        List<string> data_force = new List<string>();
        List<string> data_torque = new List<string>();
        List<string> data_position = new List<string>();

        data_position.Add(balance_err);
        data_position.Add(controller.skeleton.LHip.Position().ToString());
        data_position.Add(controller.skeleton.RHip.Position().ToString());
        data_position.Add(controller.skeleton.LKnee.Position().ToString());
        data_position.Add(controller.skeleton.RKnee.Position().ToString());
        data_position.Add(controller.skeleton.LFoot.Position().ToString());
        data_position.Add(controller.skeleton.RFoot.Position().ToString());

        data_angle.Add(balance_err);
        data_angle.Add(controller.skeleton.LHip.Angle().ToString());
        data_angle.Add(controller.skeleton.RHip.Angle().ToString());
        data_angle.Add(controller.skeleton.LKnee.Angle().ToString());
        data_angle.Add(controller.skeleton.RKnee.Angle().ToString());
        data_angle.Add(controller.skeleton.LFoot.Angle().ToString());
        data_angle.Add(controller.skeleton.RFoot.Angle().ToString());

        logger.files[0].AddRow(data_angle);
        logger.files[1].AddRow(data_force);
        logger.files[2].AddRow(data_torque);
        logger.files[3].AddRow(data_position);
    }
    
    void SampleHipPositions() {
        controller.skeleton.UpdateCOM();
        controller.skeleton.UpdateSupportingPoly();
        
        List<Vector3> tmp = new List<Vector3>(controller.skeleton.supportingPoly);
        tmp.Add(controller.skeleton.Pelvis.Position());
        aabb = JumpUtil.minAABB3d(tmp.ToArray());

        Vector3 base_pos = controller.skeleton.Pelvis.Position();

        Debug.Log("AABB: " + JumpUtil.ArrayToString(aabb));

        // the interval length will always be the bounding box's last element -
        // first element
        Vector3 aabb_size = aabb[7] - aabb[0];
        int sample_width = (int)(aabb_size.x / step) + 1,
            sample_height = (int)(aabb_size.y / step) + 1,
            sample_depth = (int)(aabb_size.z / step) + 1;

        for (int swidth = 0; swidth < sample_width; swidth++) {
            for (int sdepth = 0; sdepth < sample_depth; sdepth++) {
                for (int sheight = 0; sheight < sample_height; sheight++) {
                    Vector3 displacement = new Vector3(swidth * step, sheight * step, sdepth * step);
                    controller.skeleton.Pelvis.Position(base_pos + displacement);
                    ikmanager.Iterate();
                    WriteSampleToLogs();
                }
            }
        }
        
        controller.skeleton.Pelvis.Position(base_pos);
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
            }
        }
	}
}
