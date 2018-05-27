using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour {

	Rigidbody rb;
	LineRenderer lr;
	public Transform target;

	public float h = 25;
	public float gravity = Physics.gravity.y;

	public bool isGrabbed = false;

	Vector3 grabReference;

	Camera playerCamera;

	public bool throwing = false;

	void Start(){
		playerCamera = Camera.main;
		rb = GetComponent<Rigidbody>();
		lr = GetComponent<LineRenderer>();
		lr.material = new Material (Shader.Find("Sprites/Default"));
	}
	void Update () {
		ClampValues();
		if (throwing) DrawPath();
		if(Input.GetButtonDown("Throw") && isGrabbed && !throwing) {
			target = new GameObject("ThrowTarget").transform;
			target.position = Player.position + Player.tr.TransformDirection(Vector3.forward);
			Player.canMoveCamera = false;
			throwing = true;
			lr.enabled = true;
			h = 1.5f;
		}
		if(Input.GetButtonUp("Throw") && isGrabbed && throwing || (!isGrabbed && throwing)) {
			GameObject.Destroy(target.gameObject);
			target = null;
			Player.canMoveCamera = true;
			throwing = false;
			lr.enabled = false;
		}
		if(throwing && isGrabbed) h += Input.GetAxis("ScrollWheel");
	}

	void ClampValues(){
		h = Mathf.Clamp(h, 1.1f, 10f);
	}

	void FixedUpdate()
	{
		if(isGrabbed) {
			Vector3 hands = playerCamera.transform.position + playerCamera.transform.forward;
			float distance = Vector3.Distance(transform.position, hands);
			if(distance > 1.5f) GetDropped();
			else MoveToHands(hands);

			if(throwing) {
				Vector3 movement = new Vector3(Player.mouseInput.x, 0, Player.mouseInput.y) * Player.mouseSensitivity;
				Vector3 transformed = Player.tr.TransformDirection(movement) / 20;
				target.position += transformed;

			}
		}
		
	}

	public void Launch(){
		rb.freezeRotation = false;
		rb.useGravity = true;
		rb.isKinematic = false;
		isGrabbed = false;
		gameObject.layer = 9;
		Player.itemGrabbed = false;
		Player.currentItem = null;
		Player.SetCrosshair(0);

		Physics.gravity = Vector3.up * gravity;
		rb.velocity = CalculateLaunchData().initialVelocity;
	}

	void DrawPath(){
		LaunchData launchData = CalculateLaunchData();
		Vector3 previousDrawPoint = rb.position;

		int resolution = lr.positionCount - 1;
		for(int i = 0; i <= resolution; i++){
			float simulationTime = i / (float) resolution * launchData.timeToTarget;
			Vector3 displacement = launchData.initialVelocity * simulationTime + Vector3.up * gravity * simulationTime * simulationTime / 2f;
			Vector3 drawPoint = rb.position + displacement;
			lr.SetPosition(i, previousDrawPoint);
			previousDrawPoint = drawPoint;
		}
	}

	LaunchData CalculateLaunchData(){
		float displacementY = target.position.y - rb.position.y;
		Vector3 displacementXZ = new Vector3(target.position.x - rb.position.x, 0, target.position.z - rb.position.z);

		float time = Mathf.Sqrt(-2*h/gravity) + Mathf.Sqrt(2*(displacementY - h)/gravity);

		Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
		Vector3 velocityXZ = displacementXZ / time;

		return new LaunchData(velocityXZ + velocityY * -Mathf.Sign(gravity), time);
	}

	void MoveToHands(Vector3 hands){
		transform.position = Vector3.SmoothDamp(transform.position, hands, ref grabReference, 3f * Time.fixedDeltaTime, float.MaxValue);
	}

	public void GetDropped(){
		Vector3 grabPosition = playerCamera.transform.position + playerCamera.transform.forward;
		Vector3 direction = (grabPosition - transform.position).normalized;
		float distance = Vector3.Distance(transform.position, grabPosition);
		distance *= 15f;
		distance = distance < 1.5f ? 0 : distance;
		distance = Mathf.Clamp(distance, 0f, 8f);
		print(distance);
		rb.velocity = direction * distance;

		rb.freezeRotation = false;
		rb.useGravity = true;
		isGrabbed = false;
		gameObject.layer = 9;
		Player.itemGrabbed = false;
		rb.isKinematic = false;
		Player.currentItem = null;
		Player.SetCrosshair(0);
	}

	public void GetPickedUp(){
		Player.SetCrosshair(2);
		Player.itemGrabbed = true;
		gameObject.layer = 12;
		rb.isKinematic = true;
		rb.useGravity = false;
		rb.freezeRotation = true;
		isGrabbed = true;
	}

	struct LaunchData {
		public readonly Vector3 initialVelocity;
		public readonly float timeToTarget;

		public LaunchData (Vector3 initialVelocity, float timeToTarget){
			this.initialVelocity = initialVelocity;
			this.timeToTarget = timeToTarget;
		}
	}
}
