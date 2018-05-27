using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public LayerMask crouchLayerMask;

	public static Vector3 position = Vector3.zero;
	public static Transform tr;
	new Camera camera;
	float xRotation;
	float yRotation;
	float cameraHeight;
	float cameraHeightReference;
	Vector2 smoothRotation;
	Vector2 rotationSpeedReference;
	public static float mouseSensitivity = 3f;

	public float walkSpeed = 5f;
	public float runSpeed = 8f;
	public float jumpHeight = 5f;
	bool running = false;
	Vector3 moveReference;

	new Rigidbody rigidbody;
	new CapsuleCollider collider;
	public float crouchSpeed;
	public float crouchHeight;
	public float normalHeight;
	float heightReference;
	bool crouching = false;

	public static GameObject currentItem = null;
	public static bool itemGrabbed = false;
	Vector3 itemGrabbedReference;

	[HideInInspector]
	

	public static Sprite[] crosshairs;

	public static bool canMoveCamera = true;

	public static Vector2 mouseInput = Vector2.zero;

	public static Vector3 movementInput = Vector3.zero;

	void Start()
	{

		camera = Camera.main;

		xRotation = camera.transform.rotation.x;
		yRotation = transform.rotation.y;
		smoothRotation = new Vector2(xRotation, yRotation);

		collider = GetComponent<CapsuleCollider>();
		rigidbody = GetComponent<Rigidbody>();
		tr = transform;

		Cursor.lockState = CursorLockMode.Locked;
		LoadCrosshairs();
	}

	void Update()
	{
		if (Input.GetKey(KeyCode.Escape)) AppHelper.Quit();

		if (Input.GetButton("Jump") && GroundCollider.grounded) {
			rigidbody.velocity = Vector3.zero;
			rigidbody.velocity += Vector3.up * jumpHeight;
		}

		running = Input.GetButton("Run");

		bool canStand = !Physics.CheckSphere(transform.position + (Vector3.up * (normalHeight - .4f)), .25f, crouchLayerMask);
		crouching = Input.GetButton("Crouch") || !canStand;

		position = transform.position;
		
		
		if (!itemGrabbed) GrabControl();
		else if (itemGrabbed && Input.GetButtonDown("Fire1") && currentItem.GetComponent<Throwable>().throwing) currentItem.GetComponent<Throwable>().Launch();
		else if (itemGrabbed && Input.GetButtonDown("Fire1") && !currentItem.GetComponent<Throwable>().throwing) currentItem.GetComponent<Throwable>().GetDropped();

		GetInput();
	}

	void FixedUpdate()
	{
		MovementControl();
		if (canMoveCamera) CameraControl();
		CrouchControl();
	}

	void CrouchControl(){

		float targetHeight = crouching ? crouchHeight : normalHeight;
		

		collider.height = Mathf.SmoothDamp(collider.height, targetHeight, ref heightReference, Time.fixedDeltaTime * crouchSpeed);
		collider.center = Vector3.up * (collider.height / 2);

	}

	void GrabControl(){
		int layerMask = (1 << 9);
		RaycastHit hit;


		if(Physics.Raycast(camera.transform.position, camera.transform.TransformDirection(Vector3.forward), out hit, 2f, layerMask) && !itemGrabbed){
			if (!currentItem)
			{
				currentItem = hit.collider.gameObject;
				SetCrosshair(1);
			}
		} else {
			if (currentItem) {
				currentItem = null;
				SetCrosshair(0);
			}
		}

		if(currentItem && !itemGrabbed && Input.GetButtonDown("Fire1")){
			print(currentItem);
			currentItem.GetComponent<Throwable>().GetPickedUp();
		}
	}

	void LoadCrosshairs(){
		crosshairs = new Sprite[3];
		crosshairs[0] = Resources.Load<Sprite>("textures/crosshair/normal");
		crosshairs[1] = Resources.Load<Sprite>("textures/crosshair/grab");
		crosshairs[2] = Resources.Load<Sprite>("textures/crosshair/nothing");
	}

	void GetInput(){
		mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
		movementInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
	}

	public static void SetCrosshair(int crs){
		HudControl.SetCrosshair(crosshairs[crs]);
	}

	void MovementControl(){
		//if (crouching) movementInput *= walkSpeed / 3;	
		movementInput *= running ? runSpeed : walkSpeed;

		collider.material.dynamicFriction = GroundCollider.grounded ? 0.6f : 0f;
		collider.material.staticFriction = GroundCollider.grounded ? 0.6f : 0f;

		rigidbody.MovePosition(rigidbody.position + transform.TransformDirection(movementInput) * Time.fixedDeltaTime);
	}

	void CameraControl(){

		mouseInput *= mouseSensitivity * (Time.fixedDeltaTime * 60);
		xRotation += -mouseInput.y;
		yRotation += mouseInput.x;

		xRotation = Mathf.Clamp(xRotation, -80, 80);

		smoothRotation = Vector2.SmoothDamp(smoothRotation, new Vector2(xRotation, yRotation), ref rotationSpeedReference, .01f, float.MaxValue, Time.fixedDeltaTime);

		camera.transform.eulerAngles = new Vector3(smoothRotation.x, camera.transform.eulerAngles.y, camera.transform.eulerAngles.z);
		transform.eulerAngles = new Vector3(0, smoothRotation.y, 0);

		float cameraHeightTarget = collider.height - .2f;
		cameraHeight = Mathf.SmoothDamp(cameraHeight, cameraHeightTarget, ref cameraHeightReference, Time.fixedDeltaTime);
		camera.transform.localPosition = Vector3.up * cameraHeight;

		
	}

	public static class AppHelper{
		public static void Quit(){
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
			#else
				Application.Quit();
			#endif
		}

	}

}
