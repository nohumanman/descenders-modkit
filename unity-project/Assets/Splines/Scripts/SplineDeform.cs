using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SplineDeform : ModTool.Interface.ModBehaviour
{
#if UNITY_EDITOR
	[Header("Spline info")]
	public Transform[] splineNodes;

	[Header("Input meshes")]
	public Mesh inputMesh;
	Vector3[] baseVertices;
	public Mesh inputCollisionMesh;
	Vector3[] baseCollisionVertices;

	public float rotateInputMesh = 0f;
	public Vector3 offsetInputMesh = Vector3.zero;
	Vector4 lastOffsetInfo;

	[Header("Generation settings")]
	public bool linearSpline = false;
	public bool dontScaleLength = false;
	public bool dontBendMeshes = false;
	[Range(0.0f, 1.0f)]
	public float keepWorldUp = 0f;
	public float offsetBetweenMeshes = 0f;

	[Header("Click to manually regen mesh")]
	public bool clickToRegen = false;
	bool onValidate = false;

	[HideInInspector] //serialize this value
	public float meshLengthAfterOffset = 0f;
	int multiplier;

	Mesh deformedMesh;
	Vector3[] deformedVertices;
	Mesh deformedCollisionMesh;
	Vector3[] deformedCollisionVertices;

	//spline info
	PolyLine line;
	PolyLine lineExtraInfo;
	float segmentDistance = 0.1f;
	Vector3[] nodePositions;
	Vector3[] nodeExtraInfo;

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
		for (int i = 0; i < splineNodes.Length; i++)
		{
			if (splineNodes[i] == null)
			{
				Debug.LogWarning("One of the spline deform nodes is null, make sure every node is filled in");
				return;
			}
		}

		//check if we need to update because the spline nodes changed
		bool doUpdate = false;
		if (nodePositions == null || splineNodes.Length != nodePositions.Length || clickToRegen || onValidate)
			doUpdate = true;
		else
		{
			for (int i = 0; i < splineNodes.Length; i++)
			{
				if (splineNodes[i].localPosition != nodePositions[i])
					doUpdate = true;

				if (splineNodes[i].GetComponent<SplineExtraInfo>())
					if (splineNodes[i].GetComponent<SplineExtraInfo>().scale != nodeExtraInfo[i].x ||
						splineNodes[i].GetComponent<SplineExtraInfo>().banking != nodeExtraInfo[i].y)
						doUpdate = true;
			}
		}

		if (!doUpdate)
			return;
		onValidate = false;

		UpdateSpline();

		UpdateMesh();
		
		DeformMesh();
	}

	public int GetMeshMultiplier()
	{
		float length = line.ArcLength() / GetMeshLength();
		if(dontScaleLength)
			return Mathf.FloorToInt(length);
		return Mathf.RoundToInt(length);
	}

	public Vector4 GetMeshInputOffsets()
	{
		return new Vector4(rotateInputMesh, offsetInputMesh.x, offsetInputMesh.y, offsetInputMesh.z);
	}

	public void UpdateSpline()
	{
		if (splineNodes.Length < 2)
			return;

		//get nodes and their info
		Vector3[] nodes = new Vector3[splineNodes.Length];
		Vector3[] extraInfo = new Vector3[splineNodes.Length];
		for (int i = 0; i < nodes.Length; i++)
		{
			nodes[i] = splineNodes[i].localPosition;
			if (splineNodes[i].GetComponent<SplineExtraInfo>())
			{
				extraInfo[i].x = splineNodes[i].GetComponent<SplineExtraInfo>().scale;
				extraInfo[i].y = splineNodes[i].GetComponent<SplineExtraInfo>().banking;
			}
			else
			{
				extraInfo[i].x = 1;
				extraInfo[i].y = 0;
			}
		}
		nodePositions = nodes;
		nodeExtraInfo = extraInfo;

		//make new spline
		Spline spline = new Spline(nodes);
		line = spline.ResampledWithExtraData(segmentDistance, linearSpline ? 0 : 100, nodeExtraInfo, out lineExtraInfo);
	}

	public void UpdateMesh()
	{
		//check if the length has changed and we need to do a mesh because of that
		bool newMultiplier = multiplier != GetMeshMultiplier();
		bool newInputOffset = lastOffsetInfo != GetMeshInputOffsets();

		//check for mesh regen
		if (baseVertices == null || baseVertices.Length == 0 || clickToRegen || newMultiplier || newInputOffset)
		{
			//get multiplier from spline length
			multiplier = GetMeshMultiplier();
			lastOffsetInfo = GetMeshInputOffsets();

			//regenerate rendering mesh
			if (inputMesh != null && GetComponent<MeshFilter>())
			{
				if (GetComponent<MeshFilter>().sharedMesh)
					GetComponent<MeshFilter>().sharedMesh = null;

				deformedMesh = DuplicateMesh(inputMesh, multiplier);
				deformedMesh.name = "dform" + Random.Range(0, 100000000); //give the mesh a random name
				GetComponent<MeshFilter>().sharedMesh = deformedMesh;

				baseVertices = deformedMesh.vertices;
				deformedVertices = deformedMesh.vertices;
			}

			//regenerate collision mesh
			if (inputCollisionMesh != null && GetComponent<MeshCollider>())
			{
				if (GetComponent<MeshCollider>().sharedMesh)
					GetComponent<MeshCollider>().sharedMesh = null;

				deformedCollisionMesh = DuplicateMesh(inputCollisionMesh, multiplier);
				deformedCollisionMesh.name = "dform" + Random.Range(0, 100000000); //give the mesh a random name
				GetComponent<MeshCollider>().sharedMesh = deformedCollisionMesh;

				baseCollisionVertices = deformedCollisionMesh.vertices;
				deformedCollisionVertices = deformedCollisionMesh.vertices;
			}

			clickToRegen = false;
		}
	}

	public void DeformMesh()
	{
		//adjust z scale to match total spline length;
		float zScale = line.ArcLength() / (GetMeshLength() * multiplier - offsetBetweenMeshes);
		if (dontScaleLength)
			zScale = 1f;

		//update mesh
		if (deformedVertices != null && deformedMesh != null)
		{
			for (int i = 0; i < deformedVertices.Length; i++)
			{
				deformedVertices[i] = DeformVertex(baseVertices[i], zScale);
			}

			deformedMesh.vertices = deformedVertices;
			deformedMesh.RecalculateNormals();
			deformedMesh.RecalculateTangents();
			deformedMesh.RecalculateBounds();
		}

		//update collision mesh
		if (deformedCollisionVertices != null && deformedCollisionMesh != null)
		{
			for (int i = 0; i < deformedCollisionVertices.Length; i++)
			{
				deformedCollisionVertices[i] = DeformVertex(baseCollisionVertices[i], zScale);
			}

			deformedCollisionMesh.vertices = deformedCollisionVertices;
			deformedCollisionMesh.RecalculateNormals();
			deformedCollisionMesh.RecalculateTangents();
			deformedCollisionMesh.RecalculateBounds();

			//needed to actually update mesh
			GetComponent<MeshCollider>().sharedMesh = null;
			GetComponent<MeshCollider>().sharedMesh = deformedCollisionMesh;
		}
	}

	float GetMeshLength()
	{
		if (meshLengthAfterOffset != 0)
			return meshLengthAfterOffset + offsetBetweenMeshes;
		return inputMesh.bounds.max.z + 0.001f + offsetBetweenMeshes;
	}

	public Mesh DuplicateMesh(Mesh original, int multiply = 1)
	{
		//check if we need to apply an offset to the input mesh
		if (rotateInputMesh != 0 || offsetInputMesh != Vector3.zero)
		{
			original = OffsetMesh(original, rotateInputMesh, offsetInputMesh);
			meshLengthAfterOffset = original.bounds.max.z + 0.001f;
		}

		Mesh newmesh = new Mesh();
		
		//check if we just need to copy the mesh or paste several behind each other
		if (multiply == 1)
		{
			newmesh.vertices = original.vertices;
			newmesh.triangles = original.triangles;
			newmesh.uv = original.uv;
			newmesh.normals = original.normals;
			newmesh.colors = original.colors;
			newmesh.tangents = original.tangents;
		}
		else if (multiply > 1)
		{
			float meshLength = GetMeshLength();

			//duplicate mesh several times with a z offset
			Vector3[] origVertices = original.vertices;
			int[] origTriangles = original.triangles;
			Vector2[] origUV = original.uv;
			Vector3[] origNormals = original.normals;
			Vector4[] origTangents = original.tangents;

			Vector3[] newVertices = new Vector3[origVertices.Length * multiply];
			int[] newTriangles = new int[origTriangles.Length * multiply];
			Vector2[] newUV = new Vector2[origVertices.Length * multiply];
			Vector3[] newNormals = new Vector3[origVertices.Length * multiply];
			Vector4[] newTangents = new Vector4[origVertices.Length * multiply];

			for (int i = 0; i < multiply; i++)
			{
				for (int v = 0; v < origVertices.Length; v++)
				{
					newVertices[i * origVertices.Length + v] = origVertices[v] + (meshLength * Vector3.forward * i);
					newUV[i * origVertices.Length + v] = origUV[v];
					newNormals[i * origVertices.Length + v] = origNormals[v];
					newTangents[i * origVertices.Length + v] = origTangents[v];
				}

				for (int t = 0; t < origTriangles.Length; t++)
				{
					newTriangles[i * origTriangles.Length + t] = origTriangles[t] + i * origVertices.Length;
				}
			}

			newmesh.vertices = newVertices;
			newmesh.triangles = newTriangles;
			newmesh.uv = newUV;
			newmesh.normals = newNormals;
			newmesh.tangents = newTangents;
		}
		return newmesh;
	}
	public Mesh OffsetMesh(Mesh original, float rotation, Vector3 translation)
	{
		Mesh newmesh = new Mesh();

		Quaternion rot = Quaternion.AngleAxis(rotation, Vector3.up);
		Vector3[] origVertices = original.vertices;
		Vector3[] newVertices = original.vertices;
		for (int v = 0; v < origVertices.Length; v++)
		{
			newVertices[v] = rot * origVertices[v] + translation;
		}

		newmesh.vertices = newVertices;
		newmesh.triangles = original.triangles;
		newmesh.uv = original.uv;
		newmesh.normals = original.normals;
		newmesh.colors = original.colors;
		newmesh.tangents = original.tangents;

		newmesh.RecalculateBounds();
		newmesh.RecalculateNormals();
		newmesh.RecalculateTangents();

		return newmesh;
	}

	public Vector3 DeformVertex(Vector3 baseVertex, float scaleZ)
	{
		if (!dontBendMeshes) //normal deform mode
		{
			float movement = baseVertex.z / segmentDistance * scaleZ;
			int pointInt = Mathf.FloorToInt(movement);

			Vector3 extraInfo = lineExtraInfo.Point(movement);
			Quaternion rotation = Quaternion.AngleAxis(extraInfo.y, line.Edge(pointInt));

			Vector3 point = line.Point(movement);
			Vector3 side = rotation * line.Side(pointInt);
			Vector3 up = rotation * line.Up(pointInt);
			up = Vector3.Lerp(up, Vector3.up, keepWorldUp).normalized;

			return point + baseVertex.x * side * extraInfo.x + baseVertex.y * up;
		}
		else
		{
			//get start & end & T
			float movement = baseVertex.z;
			float meshNumber = movement / GetMeshLength();
			float t = meshNumber - Mathf.Floor(meshNumber);
			float start = Mathf.Floor(meshNumber) * GetMeshLength();
			float end = start + GetMeshLength();

			//get a linear interpolation between start & end
			Vector3 startPos = line.Point(start * scaleZ / segmentDistance);
			Vector3 endPos = line.Point(end * scaleZ / segmentDistance);
			Vector3 diff = endPos - startPos;
			Vector3 dir = diff.normalized;

			//determine vectors based on interpolated line
			Vector3 side = new Vector3(dir.z, 0, -dir.x);
			Vector3 up = Vector3.Cross(dir,side);
			up = Vector3.Lerp(up, Vector3.up, keepWorldUp).normalized;

			return startPos + diff * t + baseVertex.x * side + baseVertex.y * up;
		}
	}
	public Vector3 DeformDirection(Vector3 direction, float curveOffset = 0f)
	{
		int pointPos = Mathf.FloorToInt(curveOffset / segmentDistance);

		Vector3 dir3D = line.Edge(pointPos);

		Quaternion rotation = Quaternion.FromToRotation(transform.right, dir3D);

		return rotation * direction;
	}

	void OnDrawGizmos()
	{
		//if(line != null)
		//	line.DrawAsGizmo();
		/*foreach (Transform trans in splineNodes)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawCube(trans.transform.position, Vector3.one * 1.0f);
		}*/
	}
#endif
}