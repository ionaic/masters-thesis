using UnityEngine;
using System.Collections;

[AddComponentMenu("Physical Motion Controller/Joint")]
[System.Serializable]
public class PhysicalJoint : MonoBehaviour {
    public Transform jointTransform;

    public float jointMass = 0.0f;

    public bool canRoll;
    [Range(-360.0f, 360.0f)]
    public float minRollAngle = 0.0f;
    [Range(-360.0f, 360.0f)]
    public float maxRollAngle = 360.0f;

    public bool canPitch;
    [Range(-360.0f, 360.0f)]
    public float minPitchAngle = 0.0f;
    [Range(-360.0f, 360.0f)]
    public float maxPitchAngle = 360.0f;

    public bool canYaw;
    [Range(-360.0f, 360.0f)]
    public float minYawAngle = 0.0f;
    [Range(-360.0f, 360.0f)]
    public float maxYawAngle = 360.0f;
    
    public bool CanRoll() {
        return canRoll && 
            (!Mathf.Approximately(minRollAngle, jointTransform.eulerAngles.z) || 
            !Mathf.Approximately(maxRollAngle, jointTransform.eulerAngles.z));
    }
    public bool CanPitch() {
        return canPitch && 
            (!Mathf.Approximately(minPitchAngle, jointTransform.eulerAngles.x) || 
            !Mathf.Approximately(maxPitchAngle, jointTransform.eulerAngles.x));
    }
    public bool CanYaw() {
        return canYaw && 
            (!Mathf.Approximately(minYawAngle, jointTransform.eulerAngles.y) || 
            !Mathf.Approximately(maxYawAngle, jointTransform.eulerAngles.y));
    }

    public void Rotate(Vector3 eulerAngles) {
        Rotate(eulerAngles.x, eulerAngles.y, eulerAngles.z);
    }
    public void Rotate(float pitch, float yaw, float roll) {
        float clampedX = CanPitch() ? 
            Mathf.Clamp(pitch, minPitchAngle, maxPitchAngle) 
            : 0.0f;
        float clampedY = CanYaw() ?
            Mathf.Clamp(yaw, minYawAngle, maxYawAngle)
            : 0.0f;
        float clampedZ = CanRoll() ? 
            Mathf.Clamp(roll, minRollAngle, maxRollAngle)
            : 0.0f;
        jointTransform.Rotate(clampedX, clampedY, clampedZ);
    }
    public void Rotate(Vector3 point, Vector3 axis, float angle) {
        axis.Normalize();
        jointTransform.RotateAround(point, ConstrainAxis(axis), angle);
    }
    public void ReClamp() {
        Vector3 angles = jointTransform.eulerAngles;
        angles.x = Mathf.Clamp(angles.x, minPitchAngle, maxPitchAngle);
        angles.y = Mathf.Clamp(angles.y, minYawAngle, maxYawAngle);
        angles.z = Mathf.Clamp(angles.z, minRollAngle, maxRollAngle);
        jointTransform.eulerAngles = angles;
    }

    public Vector3 ConstrainAxis(Vector3 axis) {
        Vector3 tmp = axis;
        tmp.x = CanPitch() ? tmp.x : 0.0f;
        tmp.y = CanYaw() ? tmp.y : 0.0f;
        tmp.z = CanRoll() ? tmp.z : 0.0f;
        return tmp;
    }

    public Vector3 ConstrainedEuler(Vector3 eulerAngles) {
        return eulerAngles;
    }

    void Start() {
        jointTransform = this.transform;
    }
}
