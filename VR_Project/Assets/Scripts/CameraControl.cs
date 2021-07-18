/*
 * Kyle Dakin
 * 1659221
 * NHE2422 Advanced Game Development
 * Innovative UI Development of a VR/AR Game
 */

using UnityEngine;

//Adapted from https://catlikecoding.com/unity/tutorials/movement/orbit-camera/
//Changed the control scheme to use head pos instead of mouse
//Added the ability to change distance of camera by moving head forwards and back in frame

public class CameraControl : MonoBehaviour {

	[SerializeField] Transform focus;
	[SerializeField] LayerMask obstructionMask = -1;
	
	[SerializeField, Min(0f)] float focusRadius = 5f;
	[SerializeField, Min(0f)] float alignDelay = 5f;
	
	[SerializeField, Range(1f, 20f)] float distance = 5f;
	[SerializeField, Range(0f, 1f)] float focusCentering = 0.5f;
	[SerializeField, Range(-89f, 89f)] float minVerticalAngle = -45f, maxVerticalAngle = 45f;
	[SerializeField, Range(0f, 90f)] float alignSmoothRange = 45f;
	[SerializeField, Range(1f, 360f)] float rotationSpeed = 90f;
	[SerializeField, Range(0f, 5f)] private float zoomSpeed = 0.5f; //added	

	Camera _regularCamera;

	Vector3 _focusPoint, _previousFocusPoint;

	Vector2 _orbitAngles = new Vector2(45f, 0f);

	float _lastManualRotationTime;
	float _headPosX, _headPosY; //added

	Vector3 CameraHalfExtends {
		get {
			Vector3 halfExtends;
			halfExtends.y =
				_regularCamera.nearClipPlane *
				Mathf.Tan(0.5f * Mathf.Deg2Rad * _regularCamera.fieldOfView);
			halfExtends.x = halfExtends.y * _regularCamera.aspect;
			halfExtends.z = 0f;
			return halfExtends;
		}
	}

	void OnValidate () {
		if (maxVerticalAngle < minVerticalAngle) {
			maxVerticalAngle = minVerticalAngle;
		}
	}

	void Awake () {
		_regularCamera = GetComponent<Camera>();
		_focusPoint = focus.position;
		transform.localRotation = Quaternion.Euler(_orbitAngles);
	}

	void LateUpdate () {
		UpdateFocusPoint();
		Quaternion lookRotation;
		if (ManualRotation() || AutomaticRotation()) {
			ConstrainAngles();
			lookRotation = Quaternion.Euler(_orbitAngles);
		}
		else {
			lookRotation = transform.localRotation;
		}

		Vector3 lookDirection = lookRotation * Vector3.forward;
		Vector3 lookPosition = _focusPoint - lookDirection * distance;

		Vector3 rectOffset = lookDirection * _regularCamera.nearClipPlane;
		Vector3 rectPosition = lookPosition + rectOffset;
		Vector3 castFrom = focus.position;
		Vector3 castLine = rectPosition - castFrom;
		float castDistance = castLine.magnitude;
		Vector3 castDirection = castLine / castDistance;

		if (Physics.BoxCast(
			castFrom, CameraHalfExtends, castDirection, out RaycastHit hit,
			lookRotation, castDistance, obstructionMask
		)) {
			rectPosition = castFrom + castDirection * hit.distance;
			lookPosition = rectPosition - rectOffset;
		}
		
		transform.SetPositionAndRotation(lookPosition, lookRotation);
	}

	void UpdateFocusPoint () {
		_previousFocusPoint = _focusPoint;
		Vector3 targetPoint = focus.position;
		if (focusRadius > 0f) {
			float distance = Vector3.Distance(targetPoint, _focusPoint);
			float t = 1f;
			if (distance > 0.01f && focusCentering > 0f) {
				t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
			}
			if (distance > focusRadius) {
				t = Mathf.Min(t, focusRadius / distance);
			}
			_focusPoint = Vector3.Lerp(targetPoint, _focusPoint, t);
		}
		else {
			_focusPoint = targetPoint;
		}
	}

	bool ManualRotation () {
		Vector2 input = new Vector2(
			_headPosY,
			_headPosX
		);
		const float e = 0.001f;
		if (input.x < -e || input.x > e || input.y < -e || input.y > e) {
			_orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
			_lastManualRotationTime = Time.unscaledTime;
			return true;
		}
		return false;
	}

	bool AutomaticRotation () {
		if (Time.unscaledTime - _lastManualRotationTime < alignDelay) {
			return false;
		}

		Vector2 movement = new Vector2(
			_focusPoint.x - _previousFocusPoint.x,
			_focusPoint.z - _previousFocusPoint.z
		);
		float movementDeltaSqr = movement.sqrMagnitude;
		if (movementDeltaSqr < 0.0001f) {
			return false;
		}

		float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
		float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(_orbitAngles.y, headingAngle));
		float rotationChange =
			rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
		if (deltaAbs < alignSmoothRange) {
			rotationChange *= deltaAbs / alignSmoothRange;
		}
		else if (180f - deltaAbs < alignSmoothRange) {
			rotationChange *= (180f - deltaAbs) / alignSmoothRange;
		}
		_orbitAngles.y =
			Mathf.MoveTowardsAngle(_orbitAngles.y, headingAngle, rotationChange);
		return true;
	}

	void ConstrainAngles () {
		_orbitAngles.x =
			Mathf.Clamp(_orbitAngles.x, minVerticalAngle, maxVerticalAngle);

		if (_orbitAngles.y < 0f) {
			_orbitAngles.y += 360f;
		}
		else if (_orbitAngles.y >= 360f) {
			_orbitAngles.y -= 360f;
		}
	}

	static float GetAngle (Vector2 direction) {
		float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
		return direction.x < 0f ? 360f - angle : angle;
	}
	
	// Input from Python project is a string. Get each x, y, z data and handle the input accordingly
	public void ParseInput(string posX, string posY, string posZ)
	{
		//_headPosX, _headPosY replace MouseX and MouseY as the method of input to control the rotation of the camera
		// posZ changes the distance between the player and the camera, zooming in and out from the scene
		
		switch (posX)
		{
			case "Right":
				_headPosX = 1;
				break;
			case "Left":
				_headPosX = -1;
				break;
			case "_":
				_headPosX = 0;
				break;
		}
		switch (posY)
		{
			case "Up":
				_headPosY = 1;
				break;
			case "Down":
				_headPosY = -1;
				break;
			case "_":
				_headPosY = 0;
				break;
		}

		switch (posZ)
		{
			case "Forward":
				distance += -1 * zoomSpeed;
				break;
			case "Backwards":
				distance += 1 * zoomSpeed;
				break;
			case "_":
				distance += 0;
				break;
		}

		//Clamp the distance of the camera within bounds
		distance = Mathf.Clamp(distance, 1f, 20f);
	}
}