using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModTool.Interface;

namespace DescendersCompetitive{
	public class SpawnPoint : ModBehaviour {
		[Header("Teleport Configuration")]
		[Tooltip("The points you want to teleport to")]
		// note: these MUST all be marked as spawnpoint
		public GameObject[] SpawnPoints;
		[Tooltip("ordered for in-order, random for random")]
		public SpawnStyle spawnStyle;

		int CurrentSpawnNum = 0;
		int PreviousSpawnNum = 0;
		public enum SpawnStyle{
			Ordered, Random
		}
		void Start(){
			// if random, start with random spawn
			if (spawnStyle == SpawnStyle.Random)
				CurrentSpawnNum = Random.Range(0, SpawnPoints.Length-1);
			// activate current spawn and deactivate the rest
			SpawnPoints[CurrentSpawnNum].SetActive(true);
			foreach(GameObject SpawnPoint in SpawnPoints)
				if (SpawnPoint != SpawnPoints[CurrentSpawnNum])
					SpawnPoint.SetActive(false);
		}
		void Update(){
			// if we press R or B or B on PS4 remote
			if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.JoystickButton2))
				NextSpawn(); // move the spawnpoint
		}
		void NextSpawn(){
			PreviousSpawnNum = CurrentSpawnNum;
			// deactivate previous spawn point
			SpawnPoints[PreviousSpawnNum].SetActive(false);
			// if ordered, increment
			if (spawnStyle == SpawnStyle.Ordered)
				CurrentSpawnNum++;
			else // otherwise, randomise
				CurrentSpawnNum = Random.Range(0, SpawnPoints.Length-1);
			// if we've gone to an invalid spawnpoint, go to start of list
			if (CurrentSpawnNum >= SpawnPoints.Length)
				CurrentSpawnNum = 0;
			// activate the current spawn point
			SpawnPoints[CurrentSpawnNum].SetActive(true);
		}
	}
}