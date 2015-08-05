using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PhysicalJoint))]
class PhysicalJointEditor : Editor {
    void OnSceneGui() {
        GameObject obj = (GameObject) target;
        //if (obj != null) {
            PhysicalJoint joint = obj.GetComponent<PhysicalJoint>();
         //   if (joint != null) {
                // public static void DrawSolidArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius);
                // public static float ScaleValueHandle(float value, Vector3 position, Quaternion rotation, float size, Handles.DrawCapFunction capFunc, float snap); 
                
                Handles.color = new Color(1.0f, 1.0f, 1.0f, 0.2f);
                Handles.DrawSolidArc(obj.transform.position
                    , obj.transform.up
                    , -obj.transform.right
                    , 180
                    , joint.shieldArea
                    );
                Handles.color = Color.white;
                
                joint.shieldArea = Handles.ScaleValueHandle(joint.shieldArea
                    , obj.transform.position + obj.transform.forward * joint.shieldArea
                    , obj.transform.rotation
                    , 1.0f
                    , Handles.ConeCap
                    , 1.0f);
        //    }
        //}
    }
}
