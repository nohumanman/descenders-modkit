using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModTool.Interface;

namespace Quests{
	public class QuestUiManager : ModBehaviour {
		public QuestSystem questSystem;
		public Object UiQuestElement;
		public GameObject ContentMenu;
		private List<GameObject> questUiElements = new List<GameObject>();
		void Start () {
			Refresh();
		}
		void OnEnable(){
			Refresh();
		}
		public void Refresh(){
			foreach (GameObject uiElement in questUiElements.ToArray()){
				questUiElements.Remove(uiElement);
				Destroy(uiElement);
			}
			Quest[] quests = questSystem.gameObject.GetComponentsInChildren<Quest>();
			foreach (Quest quest in quests){
				CreateQuestUiElement(quest);
			}
		}

		void CreateQuestUiElement(Quest quest){
			GameObject questElement = (GameObject)Instantiate(UiQuestElement);
			questElement.transform.SetParent(ContentMenu.transform);
			UiQuest uiQuest = questElement.GetComponent<UiQuest>();
			uiQuest.questNameAndDescription.text = quest.nameOfQuest + " - " + quest.descriptionOfQuest + " for " + quest.reward.ToString() + " units.";
			if (quest.isComplete){
				uiQuest.Track.GetComponentInChildren<Text>().text = "<color=lime>COMPLETE</color>";
			}
			else if (quest == questSystem.currentlyTracking){
				uiQuest.Track.GetComponentInChildren<Text>().text = "<color=white>TRACKED</color>";
				uiQuest.Track.GetComponent<Button>().onClick.AddListener(delegate(){ questSystem.Track(null);uiQuest.Track.GetComponentInChildren<Text>().text = "<color=grey>UNTRACKED</color>"; });
			}
			else{
				uiQuest.Track.GetComponentInChildren<Text>().text = "<color=grey>UNTRACKED</color>";
				uiQuest.Track.GetComponent<Button>().onClick.AddListener(delegate(){ questSystem.Track(quest);});
			}
			questUiElements.Add(questElement);
		}
		public void DisableTrackerIfNotTracking(){
			if (questSystem.currentlyTracking == null){
				questSystem.tracker.SetActive(false);
			}
		}
	}
}