using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModTool.Interface;

public class ButtonHack : ModBehaviour {
	public float alphaThreshold = 0.1f;

	// Use this for initialization
	void Start () {
		this.GetComponent<Image>().alphaHitTestMinimumThreshold = alphaThreshold;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
