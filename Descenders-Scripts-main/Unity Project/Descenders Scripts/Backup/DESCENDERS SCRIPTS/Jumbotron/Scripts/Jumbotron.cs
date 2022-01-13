using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

namespace Jumbotron{
	public class Jumbotron : ModBehaviour {
		public JumbotronCamera jumbotronCamera;
		
		// used to prevent texture rendering when not in sight.
		void ActivateJumbotron(bool is_active = true){
			jumbotronCamera.enabled = false;
			jumbotronCamera.jumbotronCamera.enabled = false;
		}
		void OnBecameInvisible(){
			ActivateJumbotron(true);
		}
		void OnBecameVisible(){
			ActivateJumbotron(false);
		}
	}
}

