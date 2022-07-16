using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SplitTimer
{
	public class SplitTimerText : MonoBehaviour
	{
		public static SplitTimerText Instance { get; private set; }
		public Text text;
		public float time;
		public string checkpointTime = "";
		public bool count = false;
		bool uiEnabled = true;
		void Awake()
		{
			if (Instance != null && Instance != this)
				Destroy(this);
			else
				Instance = this;
		}
		public void CheckpointTime(string message)
        {
			checkpointTime = message;
			StartCoroutine(DisableCheckpoint());
		}
		IEnumerator DisableCheckpoint()
        {
			yield return new WaitForSeconds(5f);
			checkpointTime = "";
        }
		public IEnumerator DisableTimerText(float tim)
        {
			yield return new WaitForSeconds(tim);
			SetText("");
		}
		public void Update()
        {
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.U))
			{
				uiEnabled = !uiEnabled;
				text.text = "";
			}
		}
		void Start()
        {
			text = GetComponent<Text>();
			SetText("");
			StartCoroutine(UpdateTime());
		}
		public void SetText(string textToSet)
        {
			if (uiEnabled)
				text.text = textToSet;
			else
				text.text = "";
		}
		public void RestartTimer()
		{
			time = 0;
			count = true;
			SplitTimerText.Instance.text.color = Color.black;
		}
		public void StopTimer()
		{
			count = false;
			StartCoroutine(DisableTimerText(15));
		}
		public void FixedUpdate()
		{
			if (count)
				time += Time.deltaTime;				
		}
		IEnumerator UpdateTime()
        {
			while (true)
            {
				if (count)
					SetText(FormatTime(time).ToString() + "\n" + checkpointTime);
				yield return new WaitForSeconds(0.01f);
			}
		}
		private string FormatTime(float time)
		{
			int intTime = (int)time;
			int minutes = intTime / 60;
			int seconds = intTime % 60;
			float fraction = time * 1000;
			fraction = (fraction % 1000);
			string timeText = System.String.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, fraction);
			return timeText;
		}
	}
}