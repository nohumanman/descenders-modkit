using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ModTool.Interface;

namespace SplitTimer{
	public class SplitTimerAPI : ModBehaviour {
		public string server = "descenders-api.nohumanman.com";
		public string port = "8080";
		string contact = "";
		public void LoadIntoMap(string world_name, string steam_name, string steam_id){
			contact = server.ToString() + ":" + port.ToString();
			Debug.Log("SplitTimerAPI - Loaded into " + world_name + " with steam name " + steam_name + " and steam id " + steam_id);
			Debug.Log(contact);
			StartCoroutine(CoroLoadIntoMap(world_name, steam_name, steam_id));
		}
		public void EnterCheckpoint(string trail_name, string steam_name, string steam_id, string checkpoint_num, string total_checkpoints){
			Debug.Log("SplitTimerAPI - Checkpoint entered on trail " + trail_name + ". Steam name is " + steam_name + " steam id is " + steam_id + ". Checkpoint " + checkpoint_num + " out of " + total_checkpoints);
			StartCoroutine(
				CoroEnterCheckpoint(
					trail_name,
					steam_name,
					steam_id,
					checkpoint_num,
					total_checkpoints
				));
		}
		IEnumerator CoroLoadIntoMap(string world_name, string steam_name, string steam_id){
			using (
				UnityWebRequest webRequest =
				UnityWebRequest.Get(
					contact
					+ "/API/TIMER/LOADED?world_name="
					+ world_name
					+ "&steam_name="
					+ steam_name
					+ "&steam_id="
					+ steam_id
					)
			)
			{
				yield return webRequest.SendWebRequest();
			}
		}
		IEnumerator CoroEnterCheckpoint(string trail_name, string steam_name, string steam_id, string checkpoint_num, string total_checkpoints){
			using (
				UnityWebRequest webRequest =
				UnityWebRequest.Get(
					contact
					+ "/API/TIMER/ENTER-CHECKPOINT/"
					+ checkpoint_num.ToString()
					+ "?trail_name="
					+ trail_name
					+ "&steam_name="
					+ steam_name
					+ "&steam_id="
					+ steam_id
					+ "&total_checkpoints="
					+ total_checkpoints.ToString()
					)
			)
			{
				yield return webRequest.SendWebRequest();
			}
		}
	}
}

