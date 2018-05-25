using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCollider : MonoBehaviour {

	public static bool grounded = false;
	
	void OnTriggerStay(Collider other)	
	{
		grounded = true;
	}

	void OnTriggerExit(Collider other)
	{
		grounded = false;
	}

}
