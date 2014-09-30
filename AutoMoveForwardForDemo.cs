using UnityEngine;
using System.Collections;

public class AutoMoveForwardForDemo : MonoBehaviour {
    public Vector3 vel;
	
	// Update is called once per frame
	void Update () {
	    this.transform.position += vel;
	}
}
