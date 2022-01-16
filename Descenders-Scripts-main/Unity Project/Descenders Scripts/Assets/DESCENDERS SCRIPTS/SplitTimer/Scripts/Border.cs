using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

namespace SplitTimer{
	public class Border : ModBehaviour {
		public TrailTimer trailTimer;
		bool borderShown = false;

		void OnTriggerExit(Collider other)
		{
            if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human")
            {
				Debug.LogWarning("SplitTimer.Border - Exited Border! Invalidating Time.");
				trailTimer.InvalidateTime("INVALID TIME; Out of bounds.");
			}
		}
		public void Update(){
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Alpha0)){
				borderShown = !borderShown;
				ShowBorder(borderShown);
			}
		}
		public void ShowBorder(bool shouldShow = true){
			MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
			if (meshRenderer != null){
				meshRenderer.enabled = shouldShow;
			}
		}
	}
}
