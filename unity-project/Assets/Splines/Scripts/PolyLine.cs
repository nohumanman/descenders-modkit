using UnityEngine;

public enum CurvatureMode
{
	ThreeDimensional,
	Flat,
	Vertical,
}

public class PolyLine : PointSet
{
    // A poly-line is an unclosed polygon.
    public PolyLine(Vector3[] sortedVertices) : base(sortedVertices) { }
	public PolyLine() { }

    public float ArcLength()
    {
        float dist = 0f;
        for (int i = 0; i < points.Length - 1; ++i)
        {
            dist += (points[i + 1] - points[i]).magnitude;
        }
        return dist;
    }

    public int edgeCount
    {
        get{ return points.Length-1; }
    }

    public int pointCount
    {
        get { return points.Length; }
    }

	public Vector3 Point(int pointId)
	{
		pointId = Mathf.Clamp(pointId, 0, points.Length - 1);
		return points[pointId];
	}

	public Vector3 Point(float pointIdFloat)
	{
		int nextPoint =  Mathf.Clamp(Mathf.CeilToInt(pointIdFloat), 1, points.Length - 1);
		float frac = pointIdFloat - Mathf.Floor(pointIdFloat);
		Vector3 diff = points[nextPoint] - points[nextPoint - 1];
		return points[nextPoint - 1] + diff * frac;
	}

    public Vector3 Edge(int edgeId)
    {
        edgeId = Mathf.Clamp(edgeId, 0, points.Length-2);
        return points[edgeId + 1] - points[edgeId];
    }

	//Returns smoothed direction
	public Vector3 Edge(int edgeId, int before, int after)
	{
		Vector3 totalDir = Vector3.zero;
		int total = 0;

		for (int i = edgeId - before; i <= edgeId + after; i++)
		{
			totalDir += Edge(i);
			total++;
		}

		return totalDir / (float)total;
	}

	public Vector3 Normal(int edgeId)
	{
		Vector3 edge = Edge(edgeId);
		return new Vector3(edge.y, -edge.x).normalized;
	}

	public Vector3 Side(int edgeId)
	{
		Vector3 edge = Edge(edgeId);
		return -Vector3.Cross(edge,Vector3.up).normalized;
	}

	public Vector3 Up(int edgeId)
	{
		Vector3 edge = Edge(edgeId);
		Vector3 side = Side(edgeId);
		return Vector3.Cross(edge, side).normalized;
	}
    
    public PolyLine Reversed()
    {
        PolyLine p = new PolyLine(points);
        p.Reverse();
        return p;
    }

    public void Reverse()
    {
        int n = points.Length;
        int c = n / 2;
        int o = n % 2;
        Vector3[] cache = new Vector3[c];
        for (int i = 0; i < n; ++i)
        {
            if (i < c)
            {
                cache[i] = points[i];
                points[i] = points[n - 1 - i];
            }
            else if (i >= c + o)
                points[i] = cache[c-1-(i-c-o)];
        }
    }

	public void SmoothValues(int distance, int iterations = 1)
	{
		//iterations
		for (int iter = 0; iter < iterations; iter++)
		{
			Vector3[] newPoints = new Vector3[points.Length];

			//loop over every point
			for (int i = 0; i < points.Length; i++)
			{
				float totalStrength = 0;
				Vector3 totalPoint = Vector3.zero;

				//get points in the front and back
				for (int j = -distance; j <= distance; j++)
				{
					int point = i+j;
					if (point < 0 || point >= points.Length)
						continue;

					//add to total based on distance
					float dist = (float)Mathf.Abs(j) + 1f;
					float strength = 1f / dist;
					totalStrength += strength;
					totalPoint += strength * points[point];
				}

				newPoints[i] = totalPoint / totalStrength;
				//Debug.Log(totalStrength);
				//Debug.Log(newPoints[i] + "  "+ totalStrength);
			}

			points = newPoints;
		}
	}

	public float GetCurvature(int edge, int before = 5, int after = 5, bool signed = false, CurvatureMode mode = CurvatureMode.ThreeDimensional)
	{
		float curvature = 0;

		if (signed)
		{
			//get start and end
			Vector3 dirStart = Edge(edge - before);
			Vector3 dirEnd = Edge(edge + after);
			Vector3 signedDirectionCheck = Up(edge);

			if (mode == CurvatureMode.Flat)
			{
				dirStart.y = 0;
				dirEnd.y = 0;
				signedDirectionCheck = Vector3.up;
			}
			else if (mode == CurvatureMode.Vertical)
			{
				dirStart.z = new Vector2(dirStart.x,dirStart.z).magnitude;
				dirEnd.z = new Vector2(dirEnd.x, dirEnd.z).magnitude;
				dirStart.x = 0;
				dirEnd.x = 0;
				signedDirectionCheck = Vector3.left;
			}

			curvature = Vector3.Angle(dirStart, dirEnd);

			//use cross product to determine if we're going left or right (or up/down)
			if (Vector3.Dot(Vector3.Cross(dirStart, dirEnd), signedDirectionCheck) < 0)
				curvature = -curvature;
		}
		else
		{
			//add every frame's curvature to the total
			for (int i = edge - before; i < edge + after; i++)
			{
				Vector3 dirStart = Edge(i);
				Vector3 dirEnd = Edge(i + 1);

				if (mode == CurvatureMode.Flat)
				{
					dirStart.y = 0;
					dirEnd.y = 0;
				}
				else if (mode == CurvatureMode.Vertical)
				{
					dirStart.z = new Vector2(dirStart.x, dirStart.z).magnitude;
					dirEnd.z = new Vector2(dirEnd.x, dirEnd.z).magnitude;
					dirStart.x = 0;
					dirEnd.x = 0;
				}

				curvature += Vector3.Angle(dirStart, dirEnd);
			}
		}

		return curvature;
	}

	public int GetClosestPoint(Vector3 pos, bool ignoreY = false)
	{
		//we do 3 steps in smaller intervals to not have to go over every point
		int interval = 100;
		int interval2 = 10;

		float minDist = Mathf.Infinity;
		int minPoint = 0;
		
		//initial interval
		for (int i = 0; i < points.Length - 1; i += interval)
		{
			Vector3 diff = Point(i) - pos;

			if (ignoreY) 
				diff.y = 0;

			float sqrMag = diff.sqrMagnitude;
			if (sqrMag < minDist)
			{
				minDist = sqrMag;
				minPoint = i;
			}
		}

		//second interval
		int minPointInitial = minPoint;
		for (int i = minPointInitial - interval; i < minPointInitial + interval; i += interval2)
		{
			Vector3 diff = Point(i) - pos;

			if (ignoreY)
				diff.y = 0;

			float sqrMag = diff.sqrMagnitude;
			if (sqrMag < minDist)
			{
				minDist = sqrMag;
				minPoint = i;
			}
		}

		//final interval
		int minPointSecond = minPoint;
		for (int i = minPointSecond - interval2; i < minPointSecond + interval2; i += 1)
		{
			Vector3 diff = Point(i) - pos;

			if (ignoreY)
				diff.y = 0;

			float sqrMag = diff.sqrMagnitude;
			if (sqrMag < minDist)
			{
				minDist = sqrMag;
				minPoint = i;
			}
		}

		return minPoint;
	}

    override public void DrawAsGizmo()
	{
#if UNITY_EDITOR
		if (points == null)
			return;

		for (int i = 0; i < points.Length - 1; ++i)
        {
			Vector3 p0 = points[i];
			Vector3 p1 = points[i + 1];
			Vector3 side = Side(i);
			Vector3 up = Up(i);
            //Vector3 n0 = matrix.MultiplyVector(Normal(i)).normalized;
			//Vector3 n1 = matrix.MultiplyVector(Normal(i + 1)).normalized;

			Gizmos.DrawLine(p0, p1);
			Gizmos.DrawRay(p0, side);
			Gizmos.DrawRay(p0, up);
			//Gizmos.DrawLine(p0 + n0 * 2, p1 + n1 * 2);
           // Gizmos.DrawLine(p0, p0+n0*2);
		}
#endif
	}
}