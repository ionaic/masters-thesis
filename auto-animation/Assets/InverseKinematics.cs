/* Inverse Kinematics implementation
 * Author: Ian Ooi
 * Created: October 2014
 * Last updated: October 2014
 */

using UnityEngine;
using System.Collections;

[AddComponentMenu("Inverse Kinematics/Inverse Kinematics")]
public class InverseKinematics : MonoBehaviour {
    public SingleChainIKHandle scHandle;
	
    void Start() {
        //if (scHandle == null) {
        //    scHandle.target = Instantiate(scHandle.jointChain[scHandle.jointChain.Length]) as Transform;
        //    Debug.Log(scHandle.target);
        //}
    }

	// Update is called once per frame
	void Update () {
	    scHandle.rotateToTarget();
	}
}
