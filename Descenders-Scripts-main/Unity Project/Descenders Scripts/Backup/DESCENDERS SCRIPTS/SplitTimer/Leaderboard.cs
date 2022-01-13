using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModTool.Interface;

namespace SplitTimer{
	public class Leaderboard : ModBehaviour {
		public TextMesh textMesh;
		public void UpdateLeaderboard(){
			// make get request to refresh leaderboard.
			StartCoroutine(CoroUpdateLeaderboard());
		}
		IEnumerator CoroUpdateLeaderboard(){
			// make get request to refresh leaderboard.
			yield return null;
		}
	}
}