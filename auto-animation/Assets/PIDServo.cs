using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PIDServo {
    // PID servo function object
    // function object instead of function so it can store the k values and
    // past errors
    public float k_p;
    public float k_i;
    public float k_d;
    private List<float> past_errors;
    private List<Vector3> past_errors_v;
    
    public PIDServo() {
        past_errors = new List<float>();
        past_errors_v = new List<Vector3>();
    }
    
    public float modify(float error) {
        float I = 0.0f, D = 0.0f;
        if (past_errors.Count() > 0) {
            I = past_errors.Sum();
            D = error - past_errors.Last();
        }

        past_errors.Add(error);

        return k_p * error + k_i * I + k_d * D;
    }
    public Vector3 modify(Vector3 error) {
        Vector3 I = Vector3.zero, D = Vector3.zero;
        if (past_errors_v.Count() > 0) {
            I = past_errors_v.Aggregate((acc, cur) => acc + cur);
            D = error - past_errors_v.Last();
        }

        past_errors_v.Add(error);

        return k_p * error + k_i * I + k_d * D;
    }

    public void Reset() {
        past_errors.Clear();
        past_errors_v.Clear();
    }
}
