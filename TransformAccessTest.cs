using UnityEngine;
using System.Collections;

public class TransformAccessTest : MonoBehaviour {
    public Transform BoneToTouch;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        BoneToTouch.Translate(10.0f, 10.0f, 10.0f);
        Debug.Log("Bone: " + BoneToTouch);
	}
}
