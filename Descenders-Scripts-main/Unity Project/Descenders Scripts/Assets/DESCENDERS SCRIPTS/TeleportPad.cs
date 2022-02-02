using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

public class TeleportPad : ModBehaviour {
	[Tooltip("When active, the player will completely freeze after teleport - this can cause the rider to die if entered too quickly.")]
	public bool ShouldFreezePlayer;
	[Tooltip("The point you want to teleport to (is default)")]
	public GameObject TeleportPoint;
	[System.NonSerialized]
	public GameObject PlayerHuman;
	void Update () {
		if (PlayerHuman == null){
			PlayerHuman = GameObject.Find("Player_Human");
		}
	}
	public void OnTriggerEnter(Collider other){
		if (other.transform.name == "Bike" && other.transform.root.name == "Player_Human")
		{
			if (TeleportPoint == null){
				Debug.LogError("TeleportPad.cs - No TeleportPoint attached to TeleportPad!");
			}
			else{
				TeleportPlayer(TeleportPoint);
			}
		}
	}

	public void TeleportPlayer(GameObject to){
		PlayerHuman.transform.position = to.transform.position;
		if (ShouldFreezePlayer){
			foreach (Rigidbody rb in PlayerHuman.GetComponentsInChildren<Rigidbody>()){
				rb.velocity = new Vector3(0, 0, 0);
			}
		}
	}
}
