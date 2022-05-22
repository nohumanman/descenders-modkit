using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplitTimer{
	public class Instantiate : MonoBehaviour {
		void Start(){
            GameObject[] trails = GameObject.FindGameObjectsWithTag("Trail");
            foreach(GameObject trail in trails){
                string json = JsonUtility.ToJson(trail);
                Debug.Log(json);
            }
        }
	}
}
