using UnityEngine;
using System.Collections.Generic;

public class Spline : PolyLine
{
    // Catmull rom interpolation as extension on the poly line.
    public Spline(Vector3[] sortedVertices) : base(sortedVertices)
    {

    }

	public int maxParameter
	{
		get
		{
			return (points.Length - 1);
		}
	}

    public Vector3 PointAtParameter(float inParameter)
    {
		//get offset and t
		float t = Mathf.Clamp(inParameter, 0f, maxParameter);
        int offset = (int)t;
        t -= offset;

		//get points
		Vector3 p0 = points[Mathf.Clamp(offset - 1, 0, maxParameter)];
		Vector3 p1 = points[offset];
		Vector3 p2 = points[Mathf.Clamp(offset + 1, 0, maxParameter)];
		Vector3 p3 = points[Mathf.Clamp(offset + 2, 0, maxParameter)];

		//check early out
		if (t == 0f)
			return p1;
		if (t == 1f)
			return p2;

		//catmull rom calculation
		return IQCatmullRom(t, p0, p1, p2, p3);
    }

	Vector3 IQCatmullRom(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		Vector3 a = 0.5f * (2f * p1);
		Vector3 b = 0.5f * (p2 - p0);
		Vector3 c = 0.5f * (2f * p0 - 5f * p1 + 4f * p2 - p3);
		Vector3 d = 0.5f * (-p0 + 3f * p1 - 3f * p2 + p3);

		Vector3 pos = a + (b * t) + (c * t * t) + (d * t * t * t);

		return pos;
	}
    
    public PolyLine AsPolyLine(int sampleSteps)
    {
        sampleSteps = Mathf.Max(1, sampleSteps);

        Vector3[] outPoints = new Vector3[sampleSteps * maxParameter + 1];

        for (int i = 0; i < maxParameter; ++i)
        {
            for (int j = 0; j < sampleSteps; ++j)
            {
                float p = j / (float)sampleSteps;
                outPoints[i * sampleSteps + j] = PointAtParameter(i + p);
            }
        }

        outPoints[outPoints.Length-1] = PointAtParameter(maxParameter);

        return new PolyLine(outPoints);
    }
    
	public PolyLine Resampled(float sampleDistance, int initialSampleSteps)
	{
		sampleDistance = Mathf.Max(0.01f, sampleDistance);

		PolyLine simple = AsPolyLine(initialSampleSteps);

		float distanceTraveled = 0;
		List<Vector3> outPoints = new List<Vector3>();
		outPoints.Add(simple.Point(0));

		for (int i = 0; i < simple.pointCount-1; i++)
		{
			//get current points
			Vector3 current = simple.Point(i);
			Vector3 next = simple.Point(i + 1);
			float distance = (next - current).magnitude;

			//check if we will go over the next point this check
			float distanceToTriggerNextPoint = outPoints.Count * sampleDistance;
			while (distanceTraveled + distance >= distanceToTriggerNextPoint)
			{
				//Add interpolated point
				float lerpValue = Mathf.InverseLerp(distanceTraveled, distanceTraveled + distance, distanceToTriggerNextPoint);
				outPoints.Add(Vector3.Lerp(current, next, lerpValue));
				distanceToTriggerNextPoint = outPoints.Count * sampleDistance;
			}

			distanceTraveled += distance;
		}

		return new PolyLine(outPoints.ToArray());
	}

	public PolyLine ResampledWithExtraData(float sampleDistance, int initialSampleSteps, Vector3[] extraData, out PolyLine extraDataLine)
	{
		if (extraData.Length != points.Length)
		{
			Debug.LogError("Extra Data array must match spline array size");
			extraDataLine = null;
			return null;
		}

		Spline ExtraDataSpline = new Spline(extraData);

		sampleDistance = Mathf.Max(0.01f, sampleDistance);

		PolyLine simple = AsPolyLine(initialSampleSteps);
		PolyLine simpleExtra = ExtraDataSpline.AsPolyLine(initialSampleSteps);

		float distanceTraveled = 0;
		List<Vector3> outPoints = new List<Vector3>();
		outPoints.Add(simple.Point(0));

		List<Vector3> outPointsExtra = new List<Vector3>();
		outPointsExtra.Add(simpleExtra.Point(0));

		for (int i = 0; i < simple.pointCount - 1; i++)
		{
			//get current points
			Vector3 current = simple.Point(i);
			Vector3 next = simple.Point(i + 1);
			float distance = (next - current).magnitude;

			Vector3 currentExtra = simpleExtra.Point(i);
			Vector3 nextExtra = simpleExtra.Point(i + 1);

			//check if we will go over the next point this check
			float distanceToTriggerNextPoint = outPoints.Count * sampleDistance;
			while (distanceTraveled + distance >= distanceToTriggerNextPoint)
			{
				//Add interpolated point
				float lerpValue = Mathf.InverseLerp(distanceTraveled, distanceTraveled + distance, distanceToTriggerNextPoint);
				outPoints.Add(Vector3.Lerp(current, next, lerpValue));
				outPointsExtra.Add(Vector3.Lerp(currentExtra, nextExtra, lerpValue));

				distanceToTriggerNextPoint = outPoints.Count * sampleDistance;
			}

			distanceTraveled += distance;
		}

		extraDataLine = new PolyLine(outPointsExtra.ToArray());
		return new PolyLine(outPoints.ToArray());
	}

	override public void DrawAsGizmo()
	{
#if UNITY_EDITOR
		for (int i = 0; i < points.Length; ++i)
        {
			Gizmos.DrawWireSphere(points[i], 0.5f);
        }
#endif
	}
}
