using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SplineFence : ModTool.Interface.ModBehaviour
{
#if UNITY_EDITOR
	[Header("Spline info")]
	public Transform[] splineNodes;

	[Header("Input meshes")]
	public Mesh inputMesh;
	#pragma warning disable
	Vector3[] baseVertices;
	#pragma warning disable
	public Mesh inputCollisionMesh;
	#pragma warning disable
	Vector3[] baseCollisionVertices;

	public float rotateInputMesh = 0f;
	public Vector3 offsetInputMesh = Vector3.zero;
	Vector4 lastOffsetInfo;

	[Header("Generation settings")]
	public bool linearSpline = false;
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
			}
		}

		if (!doUpdate)
			return;
		onValidate = false;

		UpdateSpline();

		UpdateMesh();
	}

	public int GetMeshMultiplier()
	{
		float length = line.ArcLength() / GetMeshLength();
		return Mathf.FloorToInt(length);
	}

	public Vector4 GetMeshInputOffsets()
	{
		return new Vector4(rotateInputMesh, offsetInputMesh.x, offsetInputMesh.y, offsetInputMesh.z);
	}

	public void UpdateSpline()
	{
		if (splineNodes.Length < 2)
			return;

		//get nodes
		Vector3[] nodes = new Vector3[splineNodes.Length];
		for (int i = 0; i < nodes.Length; i++)
		{
			nodes[i] = splineNodes[i].localPosition;
		}
		nodePositions = nodes;

		//make new spline
		Spline spline = new Spline(nodes);
		line = ResampleSplineWithRealDistance(spline, GetMeshLength(), 100);
	}

	public void UpdateMesh()
	{
		//check if the length has changed and we need to do a mesh because of that
		//bool newMultiplier = multiplier != GetMeshMultiplier(); // unused
		//bool newInputOffset = lastOffsetInfo != GetMeshInputOffsets(); // unused

		//check for mesh regen
		//if (baseVertices == null || baseVertices.Length == 0 || clickToRegen || newMultiplier || newInputOffset)
		//{
			//get multiplier from spline length
			multiplier = GetMeshMultiplier();
			lastOffsetInfo = GetMeshInputOffsets();

			//regenerate rendering mesh
			if (inputMesh != null && GetComponent<MeshFilter>())
			{
				if (GetComponent<MeshFilter>().sharedMesh)
					GetComponent<MeshFilter>().sharedMesh = null;

				deformedMesh = DuplicateMesh(inputMesh, line);
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

				deformedCollisionMesh = DuplicateMesh(inputCollisionMesh, line);
				deformedCollisionMesh.name = "dform" + Random.Range(0, 100000000); //give the mesh a random name
				GetComponent<MeshCollider>().sharedMesh = deformedCollisionMesh;

				baseCollisionVertices = deformedCollisionMesh.vertices;
				deformedCollisionVertices = deformedCollisionMesh.vertices;
			}

			clickToRegen = false;
		//}
	}

	float GetMeshLength()
	{
		if (meshLengthAfterOffset != 0)
			return meshLengthAfterOffset + offsetBetweenMeshes;
		return inputMesh.bounds.max.z + 0.001f + offsetBetweenMeshes;
	}

	public Mesh DuplicateMesh(Mesh original, PolyLine followLine)
	{
		//check if we need to apply an offset to the input mesh
		if (rotateInputMesh != 0 || offsetInputMesh != Vector3.zero)
		{
			original = OffsetMesh(original, rotateInputMesh, offsetInputMesh);
			meshLengthAfterOffset = original.bounds.max.z + 0.001f;
		}

		Mesh newmesh = new Mesh();
		int multiply = followLine.pointCount;
		//float meshLength = GetMeshLength(); // unused

		//duplicate mesh several times with offset along spline
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

		//for each mesh
		for (int i = 0; i < multiply; i++)
		{
			//loop over vertices and triangles
			for (int v = 0; v < origVertices.Length; v++)
			{
				//deform vertices to look at the next node
				Vector3 dir = followLine.Edge(i);
				if (i == multiply - 1) //flip last one
					dir = -dir;
				Quaternion deform = Quaternion.LookRotation(dir);

				newVertices[i * origVertices.Length + v] = deform * origVertices[v] + followLine.Point(i);
				newUV[i * origVertices.Length + v] = origUV[v];
				newNormals[i * origVertices.Length + v] = deform * origNormals[v];
				newTangents[i * origVertices.Length + v] = deform * origTangents[v];
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

	public PolyLine ResampleSplineWithRealDistance(Spline spline, float sampleDistance, int initialSampleSteps)
	{
		sampleDistance = Mathf.Max(0.01f, sampleDistance);

		PolyLine simple = spline.AsPolyLine(initialSampleSteps);
		if (linearSpline)
			simple = AsPolyLineLinear(spline, initialSampleSteps); //a rather hacky way to get a linear spline

		List<Vector3> outPoints = new List<Vector3>();
		outPoints.Add(simple.Point(0));
		Vector3 lastPoint = simple.Point(0);

		for (int i = 0; i < simple.pointCount - 1; i++)
		{
			//get current points
			Vector3 current = simple.Point(i);
			Vector3 next = simple.Point(i + 1);
			float distance = (current - lastPoint).magnitude;
			float nextDistance = (next - lastPoint).magnitude;

			//check if we will go over the next point this check
			if (nextDistance >= sampleDistance)
			{
				float lerpValue = Mathf.InverseLerp(distance, nextDistance, sampleDistance);
				Vector3 newPoint = Vector3.Lerp(current, next, lerpValue);
				outPoints.Add(newPoint);
				//print(Vector3.Magnitude(newPoint - lastPoint));

				lastPoint = newPoint;
			}
		}

		return new PolyLine(outPoints.ToArray());
	}

	public PolyLine AsPolyLineLinear(Spline spline, int sampleSteps)
	{
		sampleSteps = Mathf.Max(1, sampleSteps);

		Vector3[] outPoints = new Vector3[sampleSteps * spline.maxParameter + 1];

		for (int i = 0; i < spline.maxParameter; ++i)
		{
			for (int j = 0; j < sampleSteps; ++j)
			{
				float p = j / (float)sampleSteps;
				outPoints[i * sampleSteps + j] = PointAtParameter(spline, i + p);
			}
		}

		outPoints[outPoints.Length - 1] = PointAtParameter(spline, spline.maxParameter);

		return new PolyLine(outPoints);
	}

	public Vector3 PointAtParameter(Spline spline, float inParameter)
	{
		//get offset and t
		float t = Mathf.Clamp(inParameter, 0f, spline.maxParameter);
		int offset = (int)t;
		t -= offset;

		//get points
		Vector3 p1 = spline.Point(offset);
		Vector3 p2 = spline.Point(Mathf.Clamp(offset + 1, 0, spline.maxParameter));

		//lerp
		return Vector3.Lerp(p1, p2, t);
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