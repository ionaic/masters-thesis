using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PhysicalJoint))]
class PhysicalJointEditor : Editor {
    void OnSceneGUI() {
        PhysicalJoint joint = (PhysicalJoint) target;
        joint.Init();
            
        Handles.color = new Color(1.0f, 1.0f, 1.0f, 0.2f);
        
        // euler angles are (pitch, yaw, roll)
        Vector3 start = Quaternion.Euler(joint.minPitchAngle, 0.0f, 0.0f) * joint.DirToNext();

        // public static void DrawSolidArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius);
        Handles.DrawSolidArc(joint.transform.position // center
            , joint.transform.right // normal
            , start // from
            , joint.maxPitchAngle - joint.minPitchAngle // angle
            , joint.DirToNext().magnitude // radius
            );
        Handles.color = Color.white;
        
        // public static float ScaleValueHandle(float value, Vector3 position, Quaternion rotation, float size, Handles.DrawCapFunction capFunc, float snap); 
        joint.minPitchAngle = Handles.ScaleValueHandle(joint.minPitchAngle
            , joint.transform.position + start
            , joint.transform.root.rotation
            , 0.5f
            , Handles.SphereCap
            , 1.0f
            );
        joint.maxPitchAngle = Handles.ScaleValueHandle(joint.maxPitchAngle
            , joint.transform.position + Quaternion.Euler(joint.maxPitchAngle, 0.0f, 0.0f) * joint.DirToNext()
            , joint.transform.root.rotation
            , 0.5f
            , Handles.SphereCap
            , 1.0f
            );
    }
}
