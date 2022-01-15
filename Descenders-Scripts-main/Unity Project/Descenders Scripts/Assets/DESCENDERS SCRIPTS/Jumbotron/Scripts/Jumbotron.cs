using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

namespace Jumbotron{
	public class Jumbotron : ModBehaviour {
		public JumbotronCamera jumbotronCamera;
		
		// used to prevent texture rendering when not in sight.
		void ActivateJumbotron(bool is_active = true){
			jumbotronCamera.enabled = is_active;
			jumbotronCamera.jumbotronCamera.enabled = is_active;
		}
		void OnBecameInvisible(){
			Debug.Log("Invisible!");
			ActivateJumbotron(false);
		}
		void OnBecameVisible(){
			Debug.Log("visible!");
			ActivateJumbotron(true);
		}
	}
}

