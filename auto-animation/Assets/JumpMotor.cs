using UnityEngine;
using System.Collections;

public class JumpMotor : MonoBehaviour {
    // similar to charactermotor in Unity3D default assets
    // handle the forces of jumping, handle forces on the character and actual
    //   moving of the character
    // provide an interface so that the controller can move the character at a
    //   high level instead of worrying about minutia
    // can be a pared down version of Unity's motor

    // keep track of state of jumping or not
    // if jumping, apply forces until we're grounded and the upward velocity <= 0
    // if not jumping and grounded, we're standing or walking etc.
    // if not jumping and not grounded, falling
    public Vector3 inputMoveDirection;
    private bool isJumping;
    private Vector3 velocity;
    
    void Start () {
        // initialize to not be jumping
        isJumping = false;
        velocity = Vector3.zero;
    }
    
    void Update() {
        Vector3 tmpVelocity = velocity;
        tmpVelocity = ApplyJump(tmpVelocity);
        tmpVelocity = ApplyGravity(tmpVelocity);
    }

    void ApplyInputVelocity(Vector3 v) {

    }

    // function to apply a jump to a character in a particular direction
    Vector3 ApplyJump(Vector3 v) {
        return v;
    }

    Vector3 ApplyGravity(Vector3 v) {
        return v;
    }

    // function for moving character to a point
    public void MoveToPoint(Vector3 destination) {

    }
   
    // functions to apply external force/velocity etc. to handle jumping from a
    // moving location
    public void ApplyExternalVelocity(Vector3 velocity) {

    }
    
    public void ApplyExternalAcceleration(Vector3 acceleration) {

    }

    public void ApplyExternalForce(Vector3 force) {

    }
    
    public void Jump(Vector3 jumpAccel) {

    }
    
    public bool IsGrounded() {
        return !isJumping;
    }
}
