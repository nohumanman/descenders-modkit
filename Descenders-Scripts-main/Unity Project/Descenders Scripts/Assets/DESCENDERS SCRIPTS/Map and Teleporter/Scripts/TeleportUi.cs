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
	public void SpawnTeleportUiElements(){
		foreach (Transform tr in ContentObj.transform){
			Destroy(tr.gameObject);
		}
		teleportPoints = GameObject.FindObjectsOfType<TeleportPoint>();
		foreach(TeleportPoint teleportPoint in teleportPoints){
			GameObject el = (GameObject)Instantiate(uiElement);
			el.transform.SetParent(ContentObj.transform, false);
			el.GetComponent<Button>().onClick.AddListener(delegate(){teleporter.SpawnAtPoint(teleportPoint.gameObject); uI.DisableUI();});
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
