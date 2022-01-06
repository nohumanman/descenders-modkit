using UnityEngine;
using ModTool.Interface;
using System;

namespace Quests{
	public class QuestSystem : ModBehaviour {
		public QuestUiManager questUiManager;
		public Quest currentlyTracking;
		public GameObject tracker;
		public Quest[] allQuests;
		public RectTransform canvasRectTransform;
		public void Track(Quest toTrack){
			if (toTrack == null){
				tracker.SetActive(false);
				currentlyTracking = toTrack;
				return;
			}
			else{
				//tracker.SetActive(true);
				currentlyTracking = toTrack;
				questUiManager.Refresh();
			}
		}

		public void Update(){
			if (currentlyTracking != null){
				try{
					SetPositionFromGameobjectOverTerrain(currentlyTracking.locationOfQuest.gameObject, canvasRectTransform, tracker.GetComponent<RectTransform>());
				}
				catch (NullReferenceException){}
			}
		}
		void SetPositionFromGameobjectOverTerrain(GameObject player, RectTransform canvasRectTransform, RectTransform thisRectTransform){
			Camera camera = Camera.main.GetComponent<Camera>();
			Vector2 ViewportPosition = 
				camera.WorldToViewportPoint(
					player.transform.position
				);
			Vector2 WorldObject_ScreenPosition=new Vector2(
				(
					(ViewportPosition.x * 
					canvasRectTransform.sizeDelta.x
					)
					-
					(canvasRectTransform.sizeDelta.x * 
					0.5f)
				),
				(
					(ViewportPosition.y * 
					canvasRectTransform.sizeDelta.y
					)
					-
					(canvasRectTransform.sizeDelta.y *
					0.5f)
				));
			thisRectTransform.anchoredPosition = 
				WorldObject_ScreenPosition;
		}
	}
}
