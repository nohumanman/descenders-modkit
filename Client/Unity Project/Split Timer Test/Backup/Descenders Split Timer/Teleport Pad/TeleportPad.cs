using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

public class TeleportPad : ModBehaviour {
    [Header("Teleport Configuration")]
	[Tooltip("When active, the player will conserve their previous velocity.")]
	public bool ShouldConserveVelocity;
	[Tooltip("The point you want to teleport to")]
	public GameObject TeleportPoint;
	[System.NonSerialized]
	public GameObject PlayerHuman;
	void Update () {
		// find PlayerHuman if not found.
		if (PlayerHuman == null){
			GameObject TemporaryHuman = GameObject.Find("Player_Human");
			if (TemporaryHuman != null){
				PlayerHuman = TemporaryHuman;
			}
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

	public virtual void TeleportPlayer(GameObject to){
		PlayerHuman.transform.position = to.transform.position;
		PlayerHuman.transform.rotation = to.transform.rotation;
		if (!ShouldConserveVelocity){
			PlayerHuman.SendMessage("SetVelocity", Vector3.zero);
		}
	}
}
