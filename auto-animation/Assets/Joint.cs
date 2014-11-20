using UnityEngine;
using System.Collections;

[AddComponentMenu("Physical Motion Controller/Joint")]
[System.Serializable]
public class Joint : MonoBehaviour {
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
            (Mathf.Approximately(minRollAngle, jointTransform.eulerAngles.z) || 
            Mathf.Approximately(maxRollAngle, jointTransform.eulerAngles.z));
    }
    public bool CanPitch() {
        return canPitch && 
            (Mathf.Approximately(minPitchAngle, jointTransform.eulerAngles.x) || 
            Mathf.Approximately(maxPitchAngle, jointTransform.eulerAngles.x));
    }
    public bool CanYaw() {
        return canYaw && 
            (Mathf.Approximately(minYawAngle, jointTransform.eulerAngles.y) || 
            Mathf.Approximately(maxYawAngle, jointTransform.eulerAngles.y));
    }

    public void Rotate(Vector3 eulerAngles) {
        Rotate(eulerAngles.x, eulerAngles.y, eulerAngles.z);
    }
    public void Rotate(float pitch, float yaw, float roll) {
        float clampedX = Mathf.Clamp(pitch, minPitchAngle, maxPitchAngle);
        float clampedY = Mathf.Clamp(yaw, minPitchAngle, maxPitchAngle);
        float clampedZ = Mathf.Clamp(roll, minPitchAngle, maxPitchAngle);
        jointTransform.Rotate(clampedX, clampedY, clampedZ);
    }

    void Start() {
        if (!jointTransform) {
            jointTransform = this.transform;
        }
    }
}
