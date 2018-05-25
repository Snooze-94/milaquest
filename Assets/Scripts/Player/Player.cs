using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	new Camera camera;
	float xRotation;
	float yRotation;
	float cameraHeight;
	float cameraHeightReference;
	Vector2 smoothRotation;
	Vector2 rotationSpeedReference;
	public float mouseSensitivity = 5f;

	new Rigidbody rigidbody;
	public float walkSpeed = 5f;
	public float runSpeed = 8f;
	public float jumpHeight = 5f;
	bool running = false;

	new CapsuleCollider collider;
	public float crouchSpeed;
	public float crouchHeight;
	public float normalHeight;
	float heightReference;
	bool crouching = false;

	public GameObject currentItem = null;
	bool itemGrabbed = false;

	[HideInInspector]
	public static Vector3 movementInput = Vector3.zero;

	void Start()
	{

		camera = Camera.main;

		xRotation = camera.transform.rotation.x;
		yRotation = transform.rotation.y;
		smoothRotation = new Vector2(xRotation, yRotation);

		rigidbody = GetComponent<Rigidbody>();
		collider = GetComponent<CapsuleCollider>();

		Cursor.lockState = CursorLockMode.Locked;

	}

	void Update()
	{
		if (Input.GetKey(KeyCode.Escape)) AppHelper.Quit();

		if (Input.GetButton("Jump") && GroundCollider.grounded) {
			rigidbody.velocity = Vector3.zero;
			rigidbody.velocity += Vector3.up * jumpHeight;
		}

		running = Input.GetButton("Run");

		bool canStand = !Physics.CheckSphere(transform.position + (Vector3.up * (normalHeight - .3f)), .25f, ~(1 << 10));
		crouching = Input.GetButton("Crouch") || !canStand;
		
	}

	void FixedUpdate()
	{
		MovementControl();
		CameraControl();
		if (!itemGrabbed) GrabControl();
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

		if(Physics.Raycast(camera.transform.position, camera.transform.TransformDirection(Vector3.forward), out hit, 2f, layerMask)){
			if (!currentItem)
			{
				currentItem = hit.collider.gameObject;
			}
		} else {
			if (currentItem) currentItem = null;
		}

		if(currentItem && !itemGrabbed && Input.GetButtonDown("Fire1")){
			itemGrabbed = true;
		}

		if(itemGrabbed){
			GameObject.Destroy(currentItem);
			currentItem = null;
			itemGrabbed = false;
		}
	}

	void MovementControl(){
		movementInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		movementInput *= running ? runSpeed : walkSpeed;

		rigidbody.MovePosition(rigidbody.position + transform.TransformDirection(movementInput) * Time.fixedDeltaTime);
		
	}

	void CameraControl(){

		Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
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

		#if UNITY_WEBPLAYER
			public static string webplayerQuitURL = "http//google.com";
		#endif

		public static void Quit(){
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
			#elif UNITY_WEBPLAYER
				Application.OpenURL(webplayerQuitURL);
			#else
				Application.Quit();
			#endif
		}

	}

}
