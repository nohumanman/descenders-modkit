using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

public class SplineExtraInfo : ModTool.Interface.ModBehaviour
{
	public float scale = 1f;
	public float banking = 0f;

	void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = Color.red;
		Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.3f);
	}
}

#endif