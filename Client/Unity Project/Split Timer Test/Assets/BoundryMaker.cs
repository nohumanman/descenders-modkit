using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundryMaker : MonoBehaviour {

	public Object toSpawn;
	public Transform parent;
	public float minScale;
	public float maxScale;
	Vector3 backPos;
	bool gotBackPos;
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 100))
			{
				Debug.Log(hit.transform.name);
				if (gotBackPos)
					SpawnCubeBetween(backPos, hit.point);
				backPos = hit.point;
				gotBackPos = true;
			}
		}
	}
	void SpawnCubeBetween(Vector3 back, Vector3 forward)
    {
		Vector3 centerPos = new Vector3(back.x + forward.x, back.y + forward.y, back.z + forward.z) / 2f;
		float scaleX = Vector3.Distance(new Vector3(back.x, 0, 0), new Vector3(forward.x, 0, 0));
		float scaleY = Vector3.Distance(new Vector3(0, back.y, 0), new Vector3(0, forward.y, 0));
		scaleY = 20;
		float scaleZ = Vector3.Distance(new Vector3(0, 0, back.z), new Vector3(0, 0, forward.z));
		GameObject x = (GameObject)Instantiate(toSpawn);
		x.transform.position = centerPos;
		x.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
		x.transform.LookAt(forward);
	}
}

