using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

namespace Jumbotron{
	public class Jumbotron : ModBehaviour {
		public JumbotronCamera jumbotronCamera;
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

