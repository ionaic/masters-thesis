using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MuscleConstants {
    public float LeftHip;
    public float LeftKnee;
    public float LeftAnkle;
    public float RightHip;
    public float RightKnee;
    public float RightAnkle;
    
    public MuscleConstants() {
        LeftHip     = 0.0f;
        LeftKnee    = 0.0f;
        LeftAnkle   = 0.0f;
        RightHip    = 0.0f;
        RightKnee   = 0.0f;
        RightAnkle  = 0.0f;
    }
    public MuscleConstants(float lh, float lk, float la, float rh, float rk, float ra) {
        LeftHip     = lh;
        LeftKnee    = lk;
        LeftAnkle   = la;
        RightHip    = rh;
        RightKnee   = rk;
        RightAnkle  = ra;
    }
    
    public static MuscleConstants operator+(MuscleConstants m1, MuscleConstants m2) {
        return new MuscleConstants( m1.LeftHip      + m2.LeftHip    ,
                                    m1.LeftKnee     + m2.LeftKnee   ,
                                    m1.LeftAnkle    + m2.LeftAnkle  ,
                                    m1.RightHip     + m2.RightHip   ,
                                    m1.RightKnee    + m2.RightKnee  ,
                                    m1.RightAnkle   + m2.RightAnkle);
    }
    public static MuscleConstants operator-(MuscleConstants m1, MuscleConstants m2) {
        return new MuscleConstants( m1.LeftHip      - m2.LeftHip    ,
                                    m1.LeftKnee     - m2.LeftKnee   ,
                                    m1.LeftAnkle    - m2.LeftAnkle  ,
                                    m1.RightHip     - m2.RightHip   ,
                                    m1.RightKnee    - m2.RightKnee  ,
                                    m1.RightAnkle   - m2.RightAnkle);
    }
    public static MuscleConstants operator*(MuscleConstants m1, MuscleConstants m2) {
        return new MuscleConstants( m1.LeftHip      * m2.LeftHip    ,
                                    m1.LeftKnee     * m2.LeftKnee   ,
                                    m1.LeftAnkle    * m2.LeftAnkle  ,
                                    m1.RightHip     * m2.RightHip   ,
                                    m1.RightKnee    * m2.RightKnee  ,
                                    m1.RightAnkle   * m2.RightAnkle);
    }
    public static MuscleConstants operator/(MuscleConstants m1, MuscleConstants m2) {
        return new MuscleConstants( m1.LeftHip      / m2.LeftHip    ,
                                    m1.LeftKnee     / m2.LeftKnee   ,
                                    m1.LeftAnkle    / m2.LeftAnkle  ,
                                    m1.RightHip     / m2.RightHip   ,
                                    m1.RightKnee    / m2.RightKnee  ,
                                    m1.RightAnkle   / m2.RightAnkle);
    }
    public static MuscleConstants operator+(MuscleConstants m1, float f) {
        return new MuscleConstants( m1.LeftHip      + f ,
                                    m1.LeftKnee     + f ,
                                    m1.LeftAnkle    + f ,
                                    m1.RightHip     + f ,
                                    m1.RightKnee    + f ,
                                    m1.RightAnkle   + f);
    }
    public static MuscleConstants operator-(MuscleConstants m1, float f) {
        return new MuscleConstants( m1.LeftHip      - f ,
                                    m1.LeftKnee     - f ,
                                    m1.LeftAnkle    - f ,
                                    m1.RightHip     - f ,
                                    m1.RightKnee    - f ,
                                    m1.RightAnkle   - f);
    }
    public static MuscleConstants operator*(MuscleConstants m1, float f) {
        return new MuscleConstants( m1.LeftHip      * f ,
                                    m1.LeftKnee     * f ,
                                    m1.LeftAnkle    * f ,
                                    m1.RightHip     * f ,
                                    m1.RightKnee    * f ,
                                    m1.RightAnkle   * f);
    }
    public static MuscleConstants operator/(MuscleConstants m1, float f) {
        return new MuscleConstants( m1.LeftHip      / f ,
                                    m1.LeftKnee     / f ,
                                    m1.LeftAnkle    / f ,
                                    m1.RightHip     / f ,
                                    m1.RightKnee    / f ,
                                    m1.RightAnkle   / f);
    }
}

[RequireComponent (typeof(JumpController))]
[RequireComponent (typeof(CameraView))]
[System.Serializable]
public class DataCollection : MonoBehaviour {
    public Vector3[] targetPositions;
    public MuscleConstants[] muscleConstants;
    public float[] airTimes;
    public float[] windupTimes;
    public MuscleConstants muscleConstantRangeStart;
    public float muscleConstantsStep;
    public int muscleConstantsNumSteps;
    public JumpLogger logger;
    private JumpController controller;
    private List<MuscleConstants> constants;

    void Start() {
        controller = GetComponent<JumpController>();

        // make sure we collect all of the various specifications of muscle constants
        GatherMuscleConstants();
    }

    void GatherMuscleConstants() {
        // make a list so we can take care of a range specification in addition to any hand specified constants
        constants = new List<MuscleConstants>(muscleConstants);
        // add each constant in the range to the list
        for (int idx = 0; idx < muscleConstantsNumSteps; ++idx) {
            constants.Add(muscleConstantRangeStart + ((float)idx) * muscleConstantsStep);
        }
    }

    // MAGIC ENUMERATOR STUFF
    //public IEnumerable System.Collections.IEnumerable.GetEnumerator() {
    //    return GetEnumerator();
    //}

    //public IEnumerator<JumpVariables> GetEnumerator() {
    //    
    //}

    public IEnumerable<JumpVariables> VaryingTimes() {
        foreach (float ta in airTimes) {
            foreach (float tw in windupTimes) {
                JumpVariables j = new JumpVariables(controller.jumping);
                j.windup_time = tw;
                j.air_time = ta;
                yield return j;
            }
        }
    }

    public void CollectData() {
        
    }
}
