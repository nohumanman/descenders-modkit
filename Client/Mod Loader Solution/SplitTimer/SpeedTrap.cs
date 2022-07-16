using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIdentification;
using UnityEngine.UI;


namespace SplitTimer
{
	public class SpeedTrap : MonoBehaviour
	{
		public float speed;
		public TextMesh text;
		SpeedTrapInfo x;
		Coroutine coro;
		GameObject label_spd;
		public void Start()
        {
			x = gameObject.AddComponent<SpeedTrapInfo>();
		}
		public void Update()
        {
			if (label_spd == null)
				foreach (GameObject mesh in FindObjectsOfType<GameObject>())
				{
					if (mesh.name == "label_speed")
					{
						label_spd = mesh;
					}
				}
		}
		public void OnTriggerStay()
        {
			if (coro != null)
				StopCoroutine(coro);
			if (text == null)
				text = GameObject.Find("SpeedTrap").GetComponent<TextMesh>();
			string json = JsonUtility.ToJson(label_spd.GetComponent("TextMeshProUGUI"));
			if (json != "" && json != null)
				JsonUtility.FromJsonOverwrite(json, x);
			text.text = x.m_text;
		}
		IEnumerator flashText()
        {
            while (true)
            {
				yield return new WaitForSeconds(0.3f);
				text.text = "";
				yield return new WaitForSeconds(0.3f);
				text.text = x.m_text;
			}
        }
		public void OnTriggerExit()
        {
			if (coro != null)
				StopCoroutine(coro);
			coro = StartCoroutine(flashText());
        }
	}
}
