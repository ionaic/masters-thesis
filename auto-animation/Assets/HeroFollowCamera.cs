using UnityEngine;
using System.Collections;

[AddComponentMenu("Third Person Camera/Spring Follow Camera")]

public class HeroFollowCamera : MonoBehaviour {
	
	public Transform target;
	public float distance = 4.0f;
	public float height = 1.0f;
	public float smoothLag = 0.2f;
	public float maxSpeed = 10.0f;
	public float snapLag = 0.3f;
	public float clampHeadPositionScreenSpace = 0.75f;
	public LayerMask lineOfSightMask = 0;
	
	private bool isSnapping = false;
	private Vector3 headOffset = Vector3.zero;
	private Vector3 centerOffset = Vector3.zero;
	//private HeroController controller;
	public ThirdPersonController controller;
	//private ThirdPersonController controller;
	private Vector3 velocity = Vector3.zero;
	private float targetHeight = 100000.0f;
	
	private float turnSpeedx = 0.0f;
	private float turnSpeedy = 0.0f;
	
	private float x = 0.0f;
	private float y = 0.0f;
	
	private Quaternion something;
	// Use this for initialization
	void Start () 
	{
		CharacterController characterController = target.GetComponent<CharacterController>();
		if(characterController)
		{
			centerOffset = characterController.bounds.center - target.position;
			headOffset = centerOffset;
			headOffset.y = characterController.bounds.max.y - target.position.y;
		}
		if(target)
		{
			//controller = target.GetComponent<HeroController>();	
			controller = target.GetComponent<ThirdPersonController>();	
		}
		if(!controller)
		{
			Debug.Log("Please assign a target to the camera that has a Hero Controller script component.");
		}
		var angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 targetCenter = target.position + centerOffset;
		Vector3 targetHead = target.position + headOffset;

		// when jumping, don't move camera upwards only down!
		if(controller.IsJumping())
		{
			//we'd be moving the camera upwards, do that only if it's really high
			float newTargetHeight = targetCenter.y + height;
			if(newTargetHeight < targetHeight || newTargetHeight - targetHeight > 5)
			{
				targetHeight = targetCenter.y + height;	
			}
		}
		// when walking always update the target height
		else
		{
			targetHeight = targetCenter.y + height;	
		}
		
		/*
		// we start snapping when the user pressed "Fire2"
		if(Input.GetButton("Fire2") && !isSnapping)
		{
			velocity = Vector3.zero;
			isSnapping = true;
		}
		*/
		SetUpRotation(targetCenter, targetHead);
	}
	
	void ApplySnapping(Vector3 targetCenter)
	{
		//Vector3 position = transform.position;
		//Vector3 offset = position - targetCenter;
		//offset.y = 0.0f;
		//float currentDistance = offset.magnitude;
		//
		//float targetAngle = target.eulerAngles.y;
		//float currentAngle = transform.eulerAngles.y;
		//
		//currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref velocity.x, snapLag);
		//currentDistance = Mathf.SmoothDamp(currentDistance, distance, ref velocity.z, snapLag);
		//
		//Vector3 newPosition = targetCenter;
		//newPosition += Quaternion.Euler(0.0f, currentAngle, 0.0f) * Vector3.back * currentDistance;
		//
		//newPosition.y = Mathf.SmoothDamp(position.y, targetCenter.y + height, ref velocity.y, smoothLag, maxSpeed);
		//newPosition = AdjustLineOfSight(newPosition, targetCenter);
		//transform.position = newPosition;
		//
		//// we are close to the target, so we can stop snapping now
		//if(AngleDistance(currentAngle, targetAngle) < 3.0f)
		//{
		//	isSnapping = false;
		//	velocity = Vector3.zero;
		//}
		//
		
	}
	
	Vector3 AdjustLineOfSight(Vector3 newPosition, Vector3 target)
	{
		//RaycastHit hit;
		//if(Physics.Linecast(target, newPosition, out hit, lineOfSightMask.value))
		//{
		//	velocity = Vector3.zero;
		//	return hit.point;
		//}
		return newPosition;
	}
	
	void ApplyPositionDamping(Vector3 targetCenter)
	{
		//we try to maintain a constant distance on the xz plane with a spring
		// y position is handled with a spereate spring
		Vector3 position = transform.position;
		Vector3 offset = position - targetCenter;
		offset.y = 0.0f;
		Vector3 newTargetPos = offset.normalized * distance + targetCenter;
		
		Vector3 newPosition;
		newPosition.x = Mathf.SmoothDamp(position.x, newTargetPos.x, ref velocity.x, smoothLag, maxSpeed);
		newPosition.z = Mathf.SmoothDamp(position.z, newTargetPos.z, ref velocity.z, smoothLag, maxSpeed);
		newPosition.y = Mathf.SmoothDamp(position.y, newTargetPos.y, ref velocity.y, smoothLag, maxSpeed);
		
		newPosition = AdjustLineOfSight(newPosition, targetCenter);
		
		transform.position = newPosition;
	}
	
	void SetUpRotation(Vector3 centerPos, Vector3 headPos)
	{
		// Now it's getting hairy. The devil is in the details here, the big issue is jumping of course.
		// * When jumping up and down don't center the guy in screen space. This is important to give a feel for how high you jump.
		//   When keeping him centered, it is hard to see the jump.
		// * At the same time we dont want him to ever go out of screen and we want all rotations to be totally smooth
		//
		// So here is what we will do:
		//
		// 1. We first find the rotation around the y axis. Thus he is always centered on the y-axis
		// 2. When grounded we make him be cented
		// 3. When jumping we keep the camera rotation but rotate the camera to get him back into view if his head is above some threshold
		// 4. When landing we must smoothly interpolate towards centering him on screen
		Vector3 cameraPos = transform.position;
		Vector3 offsetToCenter = centerPos - cameraPos;

		Vector3 relativeOffset = Vector3.forward * distance + Vector3.down * height;
        if (Input.GetMouseButton(1)) {
            x += Input.GetAxisRaw("CameraHorizontal");
            y -= Input.GetAxisRaw("CameraVertical");
        }
		//x += Input.GetAxisRaw("Horizontal");
		//y -= Input.GetAxisRaw("Vertical");
		
		x = RotationClamp(x);
		y = ClampAngle(y, -20, 80);
		turnSpeedy = ClampAngle(turnSpeedy, -20, 80);
        	
		var rotation = Quaternion.Euler(y, x, 0.0f);
        var position = rotation * new Vector3(0.0f, height, -distance) + target.position;

        transform.rotation = rotation;// * Quaternion.LookRotation(relativeOffset);
        transform.position = position;
		
		
		//Debug.Log(turnSpeedx);
		
		// calculate the projected center position and top position in world space
		/*
		Ray centerRay = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1.0f));
		Ray topRay = camera.ViewportPointToRay(new Vector3(0.5f, clampHeadPositionScreenSpace, 1.0f));
		
		Vector3 centerRayPos = centerRay.GetPoint(distance);
		Vector3 topRayPos = topRay.GetPoint(distance);
		
		float centerToTopAngle = Vector3.Angle(centerRay.direction, topRay.direction);
		
		float heightToAngle = centerToTopAngle / (centerRayPos.y - topRayPos.y);
		
		float extraLookAngle = heightToAngle * (centerRayPos.y - centerPos.y);
		if(extraLookAngle < centerToTopAngle)
		{
			extraLookAngle = 0.0f;	
		}
		else
		{
			extraLookAngle = extraLookAngle - centerToTopAngle;
			transform.rotation *= Quaternion.Euler(-extraLookAngle, 0.0f, 0.0f);
		}
		*/
		
	}
		
	float AngleDistance (float a, float b)
	{
		a = Mathf.Repeat(a, 360);
		b = Mathf.Repeat(b, 360);
		
		return Mathf.Abs(b - a);
	}
	
	bool IsMovingCamera()
	{
		//if(Input.GetAxisRaw("CameraVertical") != 0.0f || Input.GetAxisRaw("CameraHorizontal") != 0.0f)
		if(Input.GetMouseButton(1))
		//if(Input.GetAxisRaw("Vertical") != 0.0f || Input.GetAxisRaw("Horizontal") != 0.0f)
		{
			return true;	
		}
		return false;
	}
	
	static float ClampAngle (float angle, float min, float max) 
	{
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}
	
	static float RotationClamp(float rotation)
	{
		if (rotation < -360)
			rotation += 360;
		if (rotation > 360)
			rotation -= 360;
		
		return rotation;
	}
}
