using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplitTimer{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(AudioSource))]
	public class RidersGate : MonoBehaviour {
		public string GateName;
		void Start () {
			if (NetClient.Instance == null)
				this.gameObject.AddComponent<NetClient>();
			NetClient.Instance.ridersGates.Add(this);
		}
		public void NetStart(){
			NetClient.Instance.SendData("GATE_NAME|" + GateName);
		}
		public void TriggerGate(){
			GetComponent<Animator>().Play("Open");
			GetComponent<AudioSource>().Play();
		}
	}
}