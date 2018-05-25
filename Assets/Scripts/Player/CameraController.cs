using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	Camera cam;

	[SerializeField]
	float normalFov;
	[SerializeField]
	float zoomFov;
	[SerializeField]
	float zoomVelocity;

	float zoomReference;

	bool zoomed;

	void Start () {
		cam = GetComponent<Camera>();

	}
	
	void Update () {

		zoomed = Input.GetButton("Fire2");

	}

	private void FixedUpdate()
	{

		cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, zoomed ? zoomFov : normalFov, ref zoomReference, zoomVelocity
			* Time.fixedDeltaTime);
		

	}
}
