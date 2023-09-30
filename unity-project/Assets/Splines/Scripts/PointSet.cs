using UnityEngine;
using System;
using System.Collections.Generic;


public class PointSet : ICloneable
{
    protected Vector3[] points;
    
    public PointSet(Vector3[] sortedVertices)
    {
        points = sortedVertices;
    }

	public PointSet() { }

	public Vector3[] GetPoints()
	{
		return points;
	}

	public void SetPoint(int index, Vector3 value)
	{
		if (index < points.Length && index > 0)
			points[index] = value;
	}

	public virtual object Clone() 
	{
		PointSet result = new PointSet((Vector3[])points.Clone());
		return result;
	} 

    private int MinX()
    {
        float best = points[0].x;
        int id = 0;
        for( int i = 1 ; i < points.Length ; ++i )
        {
            if( points[i].x < best )
            {
                best = points[i].x;
                id = i;
            }
        }
        return id;
    }
    private int MaxX()
    {
        float best = points[0].x;
        int id = 0;
        for( int i = 1 ; i < points.Length ; ++i )
        {
            if( points[i].x > best )
            {
                best = points[i].x;
                id = i;
            }
        }
        return id;
    }
    private int MinY()
    {
        float best = points[0].y;
        int id = 0;
        for( int i = 1 ; i < points.Length ; ++i )
        {
            if( points[i].y < best )
            {
                best = points[i].y;
                id = i;
            }
        }
        return id;
    }
    private int MaxY()
    {
        float best = points[0].y;
        int id = 0;
        for( int i = 1 ; i < points.Length ; ++i )
        {
            if( points[i].y > best )
            {
                best = points[i].y;
                id = i;
            }
        }
        return id;
    }

    virtual public void DrawAsGizmo()
	{
#if UNITY_EDITOR
		for (int i = 0; i < points.Length; ++i)
        {
            Gizmos.DrawWireSphere(points[i], 1f);
		}
#endif
	}
}