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
    
    public void Iterate() {
        foreach (SingleChainIKHandle handle in scHandle) {
            handle.rotateToTarget();
        }
    }
    public void Iterate(int iterations) {
        foreach (SingleChainIKHandle handle in scHandle) {
            handle.rotateToTarget(iterations);
        }
    }
}
