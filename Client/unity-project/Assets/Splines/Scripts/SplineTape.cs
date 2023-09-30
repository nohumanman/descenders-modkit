using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class SplineTape : ModTool.Interface.ModBehaviour
{
#if UNITY_EDITOR

	[Header("Spline info")]
	public List<Transform> splineNodes = new List<Transform>();

	[Header("Input settings")]
	public GameObject polePrefab;
	public GameObject tapePrefab;
	public float tapeHeight = 0.5f;

	[HideInInspector]
	public bool clickToRegen = false;
	bool onValidate;

	//spline info, serialize these but dont show them in inspector
	[HideInInspector]
	public Vector3[] nodePositions;
	[HideInInspector]
	public List<GameObject> polePrefabs = new List<GameObject>();
	[HideInInspector]
	public List<GameObject> tapePrefabs = new List<GameObject>();


	void OnValidate()
	{
		if (Application.isPlaying)
			return;

		onValidate = true;
	}

	void Update()
	{
		//only excecute in edit mode
		if (Application.isPlaying)
			return;

		//null-checks
		for (int i = 0; i < splineNodes.Count; i++)
		{
			if (splineNodes[i] == null)
			{
				Debug.LogWarning("One of the spline nodes is null, make sure every node is filled in");
				return;
			}
		}

		if (clickToRegen)
			RemovePrefabs();

		//check if we need to update because the spline nodes changed
		bool doUpdate = false;
		if (nodePositions == null || splineNodes.Count != nodePositions.Length || clickToRegen || onValidate)
			doUpdate = true;
		else
		{
			for (int i = 0; i < splineNodes.Count; i++)
			{
				if (splineNodes[i].localPosition != nodePositions[i])
					doUpdate = true;
			}
		}

		if (!doUpdate)
			return;
		onValidate = false;

		UpdateSpline();

		clickToRegen = false;
	}

	public void RemovePrefabs()
	{
		foreach(GameObject prefab in polePrefabs)
			if(prefab != null)
				DestroyImmediate(prefab);

		polePrefabs.Clear();

		foreach (GameObject prefab in tapePrefabs)
			if (prefab != null)
				DestroyImmediate(prefab);

		tapePrefabs.Clear();
	}

	public float GetTapeLength()
	{
		if (tapePrefab == null)
			return 1f;
		return tapePrefab.GetComponentInChildren<MeshRenderer>().bounds.max.z;
	}

	public void UpdateSpline()
	{
		if (splineNodes.Count == 0)
			return;

		//get nodes
		Vector3[] nodes = new Vector3[splineNodes.Count];
		for (int i = 0; i < nodes.Length; i++)
		{
			nodes[i] = splineNodes[i].localPosition;
		}
		nodePositions = nodes;

		//add prefabs that are needed
		while (polePrefabs.Count < nodePositions.Length)
		{
			if (polePrefab == null)
			{
				Debug.LogWarning("Missing pole prefab in splinetape");
				break;
			}
			GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(polePrefab);
			prefab.transform.SetParent(splineNodes[polePrefabs.Count], false);
			polePrefabs.Add(prefab);
		}
		while (tapePrefabs.Count < nodePositions.Length-1)
		{
			if (tapePrefab == null)
			{
				Debug.LogWarning("Missing tape prefab in splinetape");
				break;
			}
			GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(tapePrefab);
			prefab.transform.SetParent(splineNodes[tapePrefabs.Count], false);
			tapePrefabs.Add(prefab);
		}

		float tapeLength = GetTapeLength();
		//update tape rotation/position/scale
		for (int i = 0; i < tapePrefabs.Count; i++)
		{
			if (tapePrefabs[i] == null)
			{
				clickToRegen = true;
				return;
			}

			Vector3 dir = nodePositions[i+1] - nodePositions[i];
			tapePrefabs[i].transform.localRotation = Quaternion.LookRotation(dir);
			tapePrefabs[i].transform.localPosition = Vector3.up * tapeHeight;
			tapePrefabs[i].transform.localScale = new Vector3(1, 1, dir.magnitude / tapeLength);
		}
	}

	public void AddNode(GameObject newNode)
	{
		//spawn a new node at the end
		newNode.name = "Node" + (splineNodes.Count + 1);
		newNode.transform.parent = transform;
		newNode.transform.localRotation = Quaternion.identity;
		newNode.transform.localScale = Vector3.one;

		//make a predicted position
		if (splineNodes.Count == 0)
			newNode.transform.localPosition = Vector3.zero;
		else if (splineNodes.Count == 1)
			newNode.transform.localPosition = Vector3.forward * GetTapeLength();
		else
			newNode.transform.localPosition = splineNodes[splineNodes.Count - 1].localPosition + (splineNodes[splineNodes.Count - 1].localPosition - splineNodes[splineNodes.Count - 2].localPosition).normalized * GetTapeLength();

		splineNodes.Add(newNode.transform);
		clickToRegen = true;
		Debug.Log("Added node");
	}

	public void RemoveNode()
	{
		//remove last node
		if (splineNodes.Count == 0)
			return;

		if(splineNodes[splineNodes.Count - 1] != null)
			DestroyImmediate(splineNodes[splineNodes.Count - 1].gameObject);
		splineNodes.RemoveAt(splineNodes.Count - 1);
		clickToRegen = true;

		Debug.Log("Removed node");
	}
#endif
}

