using UnityEngine;
using System.Collections;

[AddComponentMenu("Physical Motion Controller/Joint")]
[System.Serializable]
public class PhysicalJoint : MonoBehaviour {
    public float shieldArea = 5.0f;
    public Transform jointTransform;
    private Vector3 internalAngle;
    
    public float jointMass = 0.0f;

    public bool canRoll;
    [Range(-180.0f, 180.0f)]
    public float minRollAngle = -180.0f;
    [Range(-180.0f, 180.0f)]
    public float maxRollAngle = 180.0f;

    public bool canPitch;
    [Range(-180.0f, 180.0f)]
    public float minPitchAngle = -180.0f;
    [Range(-180.0f, 180.0f)]
    public float maxPitchAngle = 180.0f;

    public bool canYaw;
    [Range(-180.0f, 180.0f)]
    public float minYawAngle = -180.0f;
    [Range(-180.0f, 180.0f)]
    public float maxYawAngle = 180.0f;

    private Vector3 restAngle;
    
    // TODO this works for now since i'm allowing negative angles and I can
    // work around it but the proble arises that negative angles are equivalent
    // to a positive angle
    public bool CanRoll() {
        return canRoll && 
            (!Mathf.Approximately(minRollAngle, internalAngle.z) || 
            !Mathf.Approximately(maxRollAngle, internalAngle.z));
    }
    public bool CanPitch() {
        return canPitch && 
            (!Mathf.Approximately(minPitchAngle, internalAngle.x) || 
            !Mathf.Approximately(maxPitchAngle, internalAngle.x));
    }
    public bool CanYaw() {
        return canYaw && 
            (!Mathf.Approximately(minYawAngle, internalAngle.y) || 
            !Mathf.Approximately(maxYawAngle, internalAngle.y));
    }
    
    public Vector3 AxisConstraints() {
        return ConstrainAxis(Vector3.one);
    }

    public Vector3 DirToNext() {
        // TODO fails on pelvis because of weird 3 way connection
        int number = 0;
        Vector3 totalDir = Vector3.zero;

        // average the directions to each child that is a physicaljoint
        foreach(Transform child in transform) {
            PhysicalJoint joint = child.gameObject.GetComponent<PhysicalJoint>();
            if (joint) {
                totalDir += joint.Position();
                number++;
            }
        }
        if (number > 0) {
            return (totalDir / number) - Position();
        }
        else {
            return Vector3.forward;
        }
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

        // update internal representation
        internalAngle.x += clampedX;
        internalAngle.y += clampedY;
        internalAngle.z += clampedZ;

        jointTransform.Rotate(clampedX, clampedY, clampedZ);
    }

    public void Rotate(Vector3 point, Vector3 axis, float angle) {
        axis.Normalize();
        jointTransform.RotateAround(point, ConstrainAxis(axis), angle);
    }

    public void ReClamp() {
        Vector3 angles = internalAngle;
        angles.x = Mathf.Clamp(angles.x, minPitchAngle, maxPitchAngle);
        angles.y = Mathf.Clamp(angles.y, minYawAngle, maxYawAngle);
        angles.z = Mathf.Clamp(angles.z, minRollAngle, maxRollAngle);
        internalAngle = angles;
        jointTransform.eulerAngles = restAngle;
        jointTransform.Rotate(internalAngle);
    }

    public void ReturnToRest() {
        Angle(restAngle);
    }
    
    public void RotateToMin() {
        Angle(new Vector3(minPitchAngle, minYawAngle, minRollAngle));
    }

    public void RotateToMax() {
        Angle(new Vector3(maxPitchAngle, maxYawAngle, maxRollAngle));
    }

    public bool AtMin() {
        bool roll_min = !canRoll && Mathf.Approximately(minRollAngle, internalAngle.z);
        bool pitch_min = !canPitch && Mathf.Approximately(minPitchAngle, internalAngle.x);
        bool yaw_min = !canYaw && Mathf.Approximately(minYawAngle, internalAngle.y);
        return roll_min || pitch_min || yaw_min;
    }

    public bool AtMax() {
        bool roll_max = !canRoll && Mathf.Approximately(maxRollAngle, internalAngle.z);
        bool pitch_max = !canPitch && Mathf.Approximately(maxPitchAngle, internalAngle.x);
        bool yaw_max = !canYaw && Mathf.Approximately(maxYawAngle, internalAngle.y);
        return roll_max || pitch_max || yaw_max;

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

    public Vector3 Position() {
        if (jointTransform) {
            return jointTransform.position;
        }
        else {
            return this.transform.position;
        }
    }

    public void Position(Vector3 newpos) {
        jointTransform.position = newpos;
    }
    
    public Vector3 Angle() {
        return internalAngle;
    }
    
    public void Angle(Vector3 newAngles) {
        internalAngle = newAngles;
        jointTransform.eulerAngles = restAngle;
        jointTransform.Rotate(internalAngle);
    }
    
    public float EnergyForBend() {
        return 0.0f;
    }

    public float Mass() {
        return jointMass != 0 ? jointMass : 1.0f;
    }
    
    public void Init() {
        if (!jointTransform) {
            jointTransform = this.transform;
        }
        restAngle = jointTransform.eulerAngles;
    }

    void Awake() {
        Init();
    }
    void Start() {
        Init();
    }
}
