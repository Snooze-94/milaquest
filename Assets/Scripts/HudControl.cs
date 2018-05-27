using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudControl : MonoBehaviour {
	static Image crosshairImage;
	
	void Start () {
		crosshairImage = transform.GetChild(0).GetComponent<Image>();	
	}

	public static void SetCrosshair(Sprite spr){
		crosshairImage.sprite = spr;
	}
}
