using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplitTimer{
	public class MapInfo : MonoBehaviour {
		public string MapId;
		public string MapName;
		public static MapInfo Instance { get; private set; }
		void Awake(){
			if (Instance != null && Instance != this) 
				Destroy(this); 
			else
				Instance = this; 
		}
	}
}
