using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
using UnityEngine.Networking;
using System;


namespace RidersGate{
	public class RidersGate : ModBehaviour {
		public AudioSource audioSource;
		public AudioClip beforeOpeningAudio;
		public AudioClip openAudio;
		public Animator animator;
		bool hasChecked = false;
		public string contact = "http://descenders-api.nohumanman.com:8080";
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
		}
		void Start () {
			//StartGate(UnityEngine.Random.Range(1f, 3.5f));
		}

		void Update () {
			float current_time = DateTime.Now.Second;
			if (current_time % 2 == 0 && !hasChecked){
				StartCoroutine(DetectIfShouldStartGate());
				hasChecked = true;
			}
			else if (!(current_time % 2 == 0)) {
				hasChecked = false;
			}
		}
		public struct GateRequest{
			public string should_start;
			public float random_delay;
		}
		IEnumerator DetectIfShouldStartGate(){
			Debug.Log("Detecting if gate should start!");
			using (
				UnityWebRequest webRequest =
				UnityWebRequest.Get(
					contact + "/API/GET-RIDERS-GATE"
				)
			)
			{
				yield return webRequest.SendWebRequest();
				GateRequest gateRequest = JsonUtility.FromJson<GateRequest>(webRequest.downloadHandler.text);
				if (gateRequest.should_start == "True"){
					StartGate(gateRequest.random_delay);
				}
			}
		}
	}
}
