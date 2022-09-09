﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnIt : MonoBehaviour {
	public Object toSpawn;
	public Transform parent;
	public float minScale;
	public float maxScale;
	public bool lockY;
	public bool lockX;
	public bool lockZ;
	void Update () {
		if (Input.GetKeyDown(KeyCode.Mouse0))
        {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 100))
			{
				Debug.Log(hit.transform.name);
				GameObject spawned = (GameObject)Instantiate(toSpawn, parent);
				spawned.transform.position = hit.point;
				Vector3 originalRot = spawned.transform.rotation.eulerAngles;
				spawned.transform.rotation = Random.rotation;
				if (lockX)
				{
					spawned.transform.rotation = Quaternion.Euler(
						new Vector3(
							originalRot.x,
							spawned.transform.rotation.eulerAngles.y,
							spawned.transform.rotation.eulerAngles.z
						)
					);
				}
				if (lockY)
				{
					spawned.transform.rotation = Quaternion.Euler(
						new Vector3(
							spawned.transform.rotation.eulerAngles.x,
							originalRot.y,
							spawned.transform.rotation.eulerAngles.z
						)
					);
				}
				if (lockZ)
				{
					spawned.transform.rotation = Quaternion.Euler(
						new Vector3(
							spawned.transform.rotation.eulerAngles.x,
							spawned.transform.rotation.eulerAngles.y,
							originalRot.z
						)
					);
				}
				spawned.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
				Debug.Log("hit");
			}
		}
	}
}

