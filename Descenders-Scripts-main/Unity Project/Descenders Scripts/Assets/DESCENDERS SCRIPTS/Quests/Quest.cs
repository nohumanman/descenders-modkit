using UnityEngine;
using ModTool.Interface;
using CustomCurrencySystem;

namespace Quests{
	public class Quest : ModBehaviour {
		public string nameOfQuest;
		public string descriptionOfQuest;
		public QuestType typeOfQuest;
		public Transform locationOfQuest;
		public bool isComplete;
		public float reward;
		public QuestUiManager questUiManager;
		public CurrencySystem currencySystem;
		
		public void Complete(){
			currencySystem.AddToBal(reward);
			questUiManager.Refresh();
			isComplete = true;
		}
	}
	
	public enum QuestType{
		CheckpointRace, TrailRace, TrickQuest
	}
}
