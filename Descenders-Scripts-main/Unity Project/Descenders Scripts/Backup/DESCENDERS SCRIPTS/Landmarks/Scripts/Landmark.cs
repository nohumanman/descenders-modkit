using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;
using CustomCurrencySystem;
using UnityEngine.UI;
using CustomUi;
using Quests;
namespace Landmarks {
	public class Landmark : ModBehaviour {
		public string landmarkTitle;
		public string landmarkDesc;
		public Texture2D picture;
		public Camera cam;
		public AudioSource audioSource;
		public Animator animator;
		public CurrencySystem currencySystem;
		public LandmarkSystem landmarkSystem;
		public float priceOfLandmark;
		public TeleportUi teleportUi;
		public bool landmarkLocked = true;
		public QuestUiManager questUiManager;
		public UI uI;
		
		public void UnlockCheckmark(){
			if (currencySystem.TakeFromBal(priceOfLandmark) == true){
				uI.DisableUI();
				uI.tracker.SetActive(false);
				landmarkLocked = false;
				landmarkSystem.FormDescription();
				cam.enabled = true;
				animator.Play("GetLandmark");
				teleportUi.SpawnTeleportUiElements();
				audioSource.Play();
				// wait 7 seconds
				StartCoroutine(temp());
			}
		}
		
		IEnumerator temp(){
			yield return new WaitForSeconds(7f);
			cam.enabled = false;
			questUiManager.DisableTrackerIfNotTracking();
		}
	}
}
