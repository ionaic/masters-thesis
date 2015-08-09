using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PhysicalJoint))]
class PhysicalJointEditor : Editor {
    void OnSceneGUI() {
        PhysicalJoint joint = (PhysicalJoint) target;
        joint.Init();
            
        
        if (joint.CanPitch()) {
            Handles.color = new Color(1.0f, 0.0f, 0.0f, 0.2f);
            // euler angles are (pitch, yaw, roll)
            Vector3 start = Quaternion.Euler(joint.minPitchAngle, 0.0f, 0.0f) * joint.DirToNext();
            Vector3 end = Quaternion.Euler(joint.maxPitchAngle, 0.0f, 0.0f) * joint.DirToNext();

            // public static void DrawSolidArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius);
            Handles.DrawSolidArc(joint.transform.position // center
                , joint.transform.right // normal
                , start // from
                , joint.maxPitchAngle - joint.minPitchAngle // angle
                , joint.DirToNext().magnitude // radius
                );

            Handles.color = Color.red;
            // public static float ScaleValueHandle(float value, Vector3 position, Quaternion rotation, float size, Handles.DrawCapFunction capFunc, float snap); 
            joint.minPitchAngle = Handles.ScaleValueHandle(joint.minPitchAngle
                , joint.transform.position + start
                , joint.transform.root.rotation
                , 0.25f
                , Handles.SphereCap
                , 1.0f
                );
            joint.maxPitchAngle = Handles.ScaleValueHandle(joint.maxPitchAngle
                , joint.transform.position + end
                , joint.transform.root.rotation
                , 0.25f
                , Handles.SphereCap
                , 1.0f
                );
        }
        if (joint.CanYaw()) {
            Handles.color = new Color(0.0f, 1.0f, 0.0f, 0.2f);
            // euler angles are (pitch, yaw, roll)
            Vector3 start = Quaternion.Euler(0.0f, joint.minYawAngle, 0.0f) * joint.transform.forward;
            Vector3 end = Quaternion.Euler(0.0f, joint.maxYawAngle, 0.0f) * joint.transform.forward;

            // public static void DrawSolidArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius);
            Handles.DrawSolidArc(joint.transform.position // center
                , joint.transform.up // normal
                , start // from
                , joint.maxYawAngle - joint.minYawAngle // angle
                , joint.DirToNext().magnitude // radius
                );

            Handles.color = Color.green;
            // public static float ScaleValueHandle(float value, Vector3 position, Quaternion rotation, float size, Handles.DrawCapFunction capFunc, float snap); 
            joint.minYawAngle = Handles.ScaleValueHandle(joint.minYawAngle
                , joint.transform.position + start
                , joint.transform.root.rotation
                , 0.25f
                , Handles.SphereCap
                , 1.0f
                );
            joint.maxYawAngle = Handles.ScaleValueHandle(joint.maxYawAngle
                , joint.transform.position + end
                , joint.transform.root.rotation
                , 0.25f
                , Handles.SphereCap
                , 1.0f
                );
        }
        if (joint.CanRoll()) {
            Handles.color = new Color(0.0f, 0.0f, 1.0f, 0.2f);
            // euler angles are (pitch, yaw, roll)
            Vector3 start = Quaternion.Euler(0.0f, 0.0f, joint.minRollAngle) * joint.DirToNext();
            Vector3 end = Quaternion.Euler(0.0f, 0.0f, joint.maxRollAngle) * joint.DirToNext();

            // public static void DrawSolidArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius);
            Handles.DrawSolidArc(joint.transform.position // center
                , joint.transform.forward // normal
                , start // from
                , joint.maxRollAngle - joint.minRollAngle // angle
                , joint.DirToNext().magnitude // radius
                );

            Handles.color = Color.blue;
            // public static float ScaleValueHandle(float value, Vector3 position, Quaternion rotation, float size, Handles.DrawCapFunction capFunc, float snap); 
            joint.minRollAngle = Handles.ScaleValueHandle(joint.minRollAngle
                , joint.transform.position + start
                , joint.transform.root.rotation
                , 0.25f
                , Handles.SphereCap
                , 1.0f
                );
            joint.maxRollAngle = Handles.ScaleValueHandle(joint.maxRollAngle
                , joint.transform.position + end
                , joint.transform.root.rotation
                , 0.25f
                , Handles.SphereCap
                , 1.0f
                );
        }
    }
}
