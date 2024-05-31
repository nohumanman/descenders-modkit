using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ModLoaderSolution
{
	public class SplitTimerText : MonoBehaviour
	{
		Color32 startingColor;
		public static SplitTimerText Instance { get; private set; }
		public Text text;
		public float timeStart;
		public string checkpointTime = "";
		public bool count = false;
		bool uiEnabled = true;
        public bool hidden = true;
		void Awake()
		{
			DontDestroyOnLoad(gameObject.transform.root);
			if (Instance != null && Instance != this)
				Destroy(this);
			else
				Instance = this;
		}
        Coroutine disableCP = null;
		public void CheckpointTime(string message)
        {
            if (disableCP != null)
			    StopCoroutine(disableCP);
			checkpointTime = message;
            disableCP = StartCoroutine(DisableCheckpoint());
		}
		IEnumerator DisableCheckpoint()
        {
			StopCoroutine("DisableCheckpoint");
			yield return new WaitForSeconds(5f);
			checkpointTime = "";
        }
		public IEnumerator DisableTimerText(float tim)
        {
			StopCoroutine("DisableTimerText");
			yield return new WaitForSeconds(tim);
			SetText("");
		}
		bool wasConnected = false;
		public double finalTime = 0;
		public void Update()
        {
			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.U))
			{
				UserInterface.Instance.SpecialNotif("UI Toggled: " + (!uiEnabled).ToString());
				uiEnabled = !uiEnabled;
				text.text = "";
			}
			if (!NetClient.Instance.IsConnected())
			{
				text.color = new Color32(247, 56, 42, 255);
				wasConnected = false;
			}
			else if (!wasConnected)
			{
				TextColToDefault();
				wasConnected = true;
			}
			if (count)
			{
				SetText(FormatTime(Time.time - timeStart).ToString() + "\n" + checkpointTime);
				finalTime = Time.time;
			}
		}
		public void TextColToDefault()
        {
			text.color = startingColor;
        }
		void Start()
        {
			text = GetComponent<Text>();
			text.supportRichText = true;
			startingColor = text.color;
			SetText("");
			Utilities.GameObjectFind("TextShadow").GetComponent<Text>().supportRichText = true;
		}
		public void SetText(string textToSet)
        {
			textToSet = textToSet.Replace("\\n", "\n");
			if (uiEnabled && !hidden)
				text.text = textToSet + "\n"; // we give it this extra \n because it's too large otherwise
			else
				text.text = "";
		}
        public Trail currentTrail;
		public void RestartTimer(Trail trailFocus)
		{
			hidden = false;
            currentTrail = trailFocus;
            timeStart =	Time.time;
			checkpointTime = "";
			count = true;
			text.color = startingColor;
		}
		public void StopTimer()
		{
			count = false;
			StartCoroutine(DisableTimerText(15));
        }
		public string FormatTime(double time)
		{
			int intTime = (int)time;
			int minutes = intTime / 60;
			int seconds = intTime % 60;
			double fraction = (time - intTime) * 1000;
			fraction = Mathf.Round((float)fraction);
			string timeText = System.String.Format("{0:00}:{1:00}.{2:000}", minutes, seconds, fraction);
			return timeText;
		}
	}
}