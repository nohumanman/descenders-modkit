using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SplitTimer
{
	public class TimerText : MonoBehaviour
	{
		public static TimerText Instance { get; private set; }
		public Text text;
		public float time;
		public bool count = false;
		void Awake()
		{
			if (Instance != null && Instance != this)
				Destroy(this);
			else
				Instance = this;
		}
		void Start()
        {
			text = GetComponent<Text>();
			StartCoroutine(UpdateTime());
		}
		public void RestartTimer()
		{
			time = 0;
			count = true;
			TimerText.Instance.text.color = Color.black;
		}
		public void StopTimer()
		{
			count = false;
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
					text.text = FormatTime(time).ToString();
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
			string timeText = System.String.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, fraction);
			return timeText;
		}
	}
}