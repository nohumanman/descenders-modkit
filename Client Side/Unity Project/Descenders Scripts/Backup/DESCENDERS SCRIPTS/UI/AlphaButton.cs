using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Required when Using UI elements.
using ModTool.Interface;

public class AlphaButton : ModBehaviour
{
	public float AlphaThreshold = 0.1f;

	void Start()
	{
		this.GetComponent<Image>().alphaHitTestMinimumThreshold = AlphaThreshold;
	}
}
