/* Inverse Kinematics implementation
 * Author: Ian Ooi
 * Created: October 2014
 * Last updated: October 2014
 */

using UnityEngine;
using System.Collections;

[AddComponentMenu("Inverse Kinematics/Inverse Kinematics")]
public class InverseKinematics : MonoBehaviour {
    public SingleChainIKHandle[] scHandle;

	// Update is called once per frame
	void Update () {
        foreach (SingleChainIKHandle handle in scHandle) {
            handle.rotateToTarget();
        }
	}
}
