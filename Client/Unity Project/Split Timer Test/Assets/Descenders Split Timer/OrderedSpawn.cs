using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

public class OrderedSpawn : ModBehaviour {

	public GameObject[] SpawnPoints;
	int CurrentSpawnNum = 0;
	int PreviousSpawnNum = 0;
	void Start(){
		SpawnPoints[CurrentSpawnNum].SetActive(true);
		foreach(GameObject SpawnPoint in SpawnPoints)
			if (SpawnPoint != SpawnPoints[CurrentSpawnNum])
				SpawnPoint.SetActive(false);
	}
	void Update(){
		if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.JoystickButton2))
			NextSpawn();
	}
	void NextSpawn(){
		PreviousSpawnNum = CurrentSpawnNum;
		SpawnPoints[PreviousSpawnNum].SetActive(false);
		CurrentSpawnNum++;
		if (CurrentSpawnNum >= SpawnPoints.Length)
			CurrentSpawnNum = 0;
		SpawnPoints[CurrentSpawnNum].SetActive(true);
	}
}
