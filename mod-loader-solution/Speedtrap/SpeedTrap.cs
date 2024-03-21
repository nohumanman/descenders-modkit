using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIdentification;
using UnityEngine.UI;


namespace ModLoaderSolution
{
	public class SpeedTrap : MonoBehaviour
	{
		public float speed;
		public TextMesh text;
		SpeedTrapInfo x;
		Coroutine coro;
		GameObject label_spd;
		bool updateText;
		public void Start()
        {
			x = gameObject.AddComponent<SpeedTrapInfo>();
			StartCoroutine(UpdateText());
		}
		public void Update()
        {
			if (label_spd == null)
				foreach (GameObject mesh in FindObjectsOfType<GameObject>())
					if (mesh.name == "label_speed")
						label_spd = mesh;
		}
		public void OnTriggerStay()
        {
			if (coro != null)
				StopCoroutine(coro);
			if (text == null)
				text = Utilities.GameObjectFind("SpeedTrap").GetComponent<TextMesh>();
			updateText = true;
		}
		IEnumerator UpdateText()
        {
			while (true)
			{
				if (updateText)
				{
					string json = JsonUtility.ToJson(label_spd.GetComponent("TextMeshProUGUI"));
					if (json != "" && json != null)
						JsonUtility.FromJsonOverwrite(json, x);
					text.text = x.m_text;
				}
				yield return new WaitForSeconds(0.1f);
			}
		}
		IEnumerator flashText()
        {
            while (true)
            {
				text.text = "";
				yield return new WaitForSeconds(0.3f);
				text.text = x.m_text;
				yield return new WaitForSeconds(0.3f);
			}
        }
		public void OnTriggerExit()
        {
			if (coro != null)
				StopCoroutine(coro);
			coro = StartCoroutine(flashText());
			updateText = false;
		}
	}
}
