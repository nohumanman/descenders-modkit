using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SplitTimer{
	public class Metric
    {
		public string id;
		public string value;
    }
	public class MapInfo : MonoBehaviour {
		public string MapId;
		public string MapName;
		public Text debugText;
		public bool debugEnabled = false;
		public List<Metric> metrics;
		public static MapInfo Instance { get; private set; }
		void Awake(){
			if (Instance != null && Instance != this) 
				Destroy(this); 
			else
				Instance = this;
			if (debugText != null)
				debugText.gameObject.SetActive(false);
			metrics = new List<Metric>();
		}
		public void AddMetric(string id, string value)
        {
			bool already = false;
			foreach(Metric currentMetric in metrics)
            {
				if (currentMetric.id == id)
				{
					currentMetric.value = value;
					already = true;
				}
			}
			if (!already)
            {
				Metric x = new Metric();
				x.id = id;
				x.value = value;
				metrics.Add(x);
			}
		}
		void Update()
        {
			if (debugText != null)
			{
				if (Input.GetKeyDown(KeyCode.B) && Input.GetKey(KeyCode.D))
					debugEnabled = !debugEnabled;
				debugText.gameObject.SetActive(debugEnabled);
				debugText.text = "";
				int i = 0;
				foreach(Metric currentMetric in metrics)
                {
					debugText.text += currentMetric.id + ": " + currentMetric.value + "\n";
					i++;
				}
				for (int x; i < 20; i++)
                {
					debugText.text += "\n";
				}
			}
		}
	}
}
