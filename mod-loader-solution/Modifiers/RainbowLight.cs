using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerIdentification;
using ModLoaderSolution;

namespace ModLoaderSolution
{
	public class RainbowLight : MonoBehaviour
	{
		public Light Spotlight;
		Light[] lights;
		IEnumerator RainbowFlash()
        {
			Color[] colors = new Color[5]{Color.red, Color.cyan, Color.yellow, Color.green, Color.blue};
			while (true)
			{
				foreach (Color color in colors)
				{
					if (Spotlight != null)
						if (Spotlight.enabled)
							Spotlight.color = color;
					yield return new WaitForSeconds(0.1f);
				}
				yield return null;
			}
		}
		void Start()
        {
			StartCoroutine(RainbowFlash());
			lights = FindObjectsOfType<Light>();
		}
		void Update()
		{
			if (Input.GetKey(KeyCode.R) && Input.GetKeyDown(KeyCode.L))
			{
				UserInterface.Instance.SpecialNotif("Rainbow light toggled: " + (!Spotlight.enabled).ToString());
				Spotlight = Utilities.GameObjectFind("Spotlight").GetComponent<Light>();
				Spotlight.enabled = !Spotlight.enabled;
				foreach(Light light in lights)
					if (light != Spotlight)
						light.enabled = !Spotlight.enabled;
			}
		}
	}
}
