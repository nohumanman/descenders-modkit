using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModTool.Interface;
using CustomTeleporter;
using CustomUi;
using UnityEngine.EventSystems;

public class TeleportUi : ModBehaviour {
	public Object uiElement;
	private TeleportPoint[] teleportPoints;
	public GameObject ContentObj;
	public Texture2D lockedTexture;
	public UI uI;
	public Teleporter teleporter;
	public void SpawnTeleportUiElements(){
		foreach (Transform tr in ContentObj.transform){
			Destroy(tr.gameObject);
		}
		teleportPoints = GameObject.FindObjectsOfType<TeleportPoint>();
		foreach(TeleportPoint teleportPoint in teleportPoints){
			GameObject el = (GameObject)Instantiate(uiElement);
			el.transform.SetParent(ContentObj.transform, false);
			el.GetComponent<Button>().onClick.AddListener(delegate(){teleporter.SpawnAtPoint(teleportPoint.gameObject); uI.DisableUI();});
			EventTrigger trigger = el.GetComponent<EventTrigger>();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener((eventData) => {
				Debug.Log("Hovered over ui element!");
				teleporter.preemptiveSpawnPoint.gameObject.SetActive(true);
				teleporter.SetPositionFromGameobjectOverTerrain(
					teleportPoint.gameObject,
					teleporter.rectTransformOfCanvas,
					teleporter.preemptiveSpawnPoint.GetComponent<RectTransform>()
				);
				teleporter.preemptiveSpawnPoint.GetComponentInChildren<Text>().text = teleportPoint.teleporterName;
			});
			trigger.triggers.Add(entry);

			EventTrigger.Entry entry2 = new EventTrigger.Entry();
			entry2.eventID = EventTriggerType.PointerExit;
			entry2.callback.AddListener((eventData) => {
				Debug.Log("Exited UI element!");
				teleporter.preemptiveSpawnPoint.gameObject.SetActive(false);
			});
			trigger.triggers.Add(entry2);
			el.GetComponentInChildren<Text>().text = teleportPoint.teleporterName;
		}
	}
	
	void Start () {
		SpawnTeleportUiElements();	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
