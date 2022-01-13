using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

namespace Jumbotron{
	public class JumbotronCamera : ModBehaviour {
		public Jumbotron jumbotron;
		[System.NonSerialized]
		public Camera jumbotronCamera;
		[System.NonSerialized]
		public GameObject player_human;
		void Start(){
			jumbotronCamera = GetComponent<Camera>();
		}
		void Update () {
			if (player_human == null){
				player_human = GameObject.Find("Player_Human");
			}
		}

		void FixedUpdate(){
			if (player_human != null){
				transform.LookAt(player_human.transform);
			}
		}
	}
}