using UnityEngine;

[System.Serializable]
public class PIDServo {
    // PID servo function object
    // function object instead of function so it can store the k values
    public float k_p;
    public float k_i;
    public float k_d;
    
    public float modify(float error) {
        return k_p * error + k_i * error + k_d * error;
    }
    public Vector3 modify(Vector3 error) {
        return k_p * error + k_i * error + k_d * error;
    }
}

