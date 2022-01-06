using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ModTool.Interface;
using CustomTeleporter;
using CustomUi;

public class TeleportUi : ModBehaviour {
	public Object uiElement;
	private TeleportPoint[] teleportPoints;
	public GameObject ContentObj;
	public Texture2D lockedTexture;
	public UI uI;
	public Teleporter teleporter;
	// Use this for initialization
	public void SpawnTeleportUiElements(){
		foreach (Transform tr in ContentObj.transform){
			Destroy(tr.gameObject);
		}
		teleportPoints = GameObject.FindObjectsOfType<TeleportPoint>();
		foreach(TeleportPoint teleportPoint in teleportPoints){
			if (teleportPoint.dependantLandmark == null || !teleportPoint.dependantLandmark.landmarkLocked){
				// spawn unlocked teleporter
				GameObject el = (GameObject)Instantiate(uiElement);
				el.transform.SetParent(ContentObj.transform);
				el.GetComponent<Button>().onClick.AddListener(delegate(){teleporter.SpawnAtPoint(teleportPoint.gameObject); uI.DisableUI();});
				el.GetComponentInChildren<Text>().text = teleportPoint.teleporterName;
			}
			else if (teleportPoint.dependantLandmark.landmarkLocked){
				// spawn locked teleporter
				GameObject el = (GameObject)Instantiate(uiElement);
				el.transform.SetParent(ContentObj.transform);
				el.GetComponentInChildren<Text>().text = "";
				el.GetComponentInChildren<RawImage>().texture = lockedTexture;
			}
		}
	}
	void Start () {
		SpawnTeleportUiElements();	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
