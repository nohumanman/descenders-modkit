using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
using UnityEngine.Networking;
using System;
using PlayerIdentification;
using SplitTimer;


namespace RidersGate{
	public class RidersGate : ModBehaviour {
		public AudioSource audioSource;
		public AudioClip beforeOpeningAudio;
		public AudioClip openAudio;
		public float previous_random_delay = -1f;
		public Animator[] animators;
		bool hasChecked = false;
		float old_time;
		public void StartGate(float random_time){
			StartCoroutine(CoroStartGate(random_time));
		}
		IEnumerator CoroStartGate(float random_time){
			audioSource.PlayOneShot(beforeOpeningAudio);
			yield return new WaitForSeconds(beforeOpeningAudio.length);
			Debug.Log("Beginning random tme!");
			yield return new WaitForSeconds(random_time);
			Debug.Log("Random time hit!");
			audioSource.PlayOneShot(openAudio);
			foreach(Animator animator in animators){
				animator.Play("Recend");
			}
			yield return new WaitForSeconds(5f);
			foreach(Animator animator in animators){
				animator.Play("Ascend");
			}
		}
		void Start () {
			//StartGate(UnityEngine.Random.Range(1f, 3.5f));
		}

		void Update () {
			float current_time = DateTime.Now.Second;
			if (old_time == current_time && !hasChecked){
				StartCoroutine(DetectIfShouldStartGate());
				hasChecked = true;
				old_time = current_time;
			}
			else if (!(current_time == old_time)) {
				hasChecked = false;
				old_time = current_time;
			}
		}
		public struct GateRequest{
			public float random_delay;
		}
		IEnumerator DetectIfShouldStartGate(){
			using (
				UnityWebRequest webRequest =
				UnityWebRequest.Get(
					SplitTimer.SplitTimer.Instance.splitTimerApi.server
					+ "/API/DESCENDERS/GET-RIDERS-GATE"
					+ "?steam_name=" + new SteamIntegration().getName()
					+ "&steam_id=" + new SteamIntegration().getSteamId()
					+ "&world_name=" + SplitTimer.SplitTimer.Instance.world_name
				)
			)
			{
				yield return webRequest.SendWebRequest();
				GateRequest gateRequest = JsonUtility.FromJson<GateRequest>(webRequest.downloadHandler.text);
				if (gateRequest.random_delay != previous_random_delay && previous_random_delay != -1f){
					StartGate(gateRequest.random_delay);
					previous_random_delay = gateRequest.random_delay;
				}
				else{
					previous_random_delay = gateRequest.random_delay;
				}
			}
		}
	}
}
