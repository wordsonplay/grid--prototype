/**
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 6.3
 */

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using WordsOnPlay.Utils;

namespace WordsOnPlay.Geometry
{

public class BlueNoise : IEnumerable<Vector2>
{

#region Constants
    const float Sqrt2 = 1.4142135623730951f;
#endregion

#region State
    private int nFails = 100;
    private Rect bounds;
    private float minDistance;
    private List<Vector2> points;
    private KDTree kdTree;
    private System.Random rng;
#endregion
    
#region IEnumerable
    public IEnumerator<Vector2> GetEnumerator()
    {
        return points.GetEnumerator();        
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
#endregion

#region Constructor
    public BlueNoise(Rect bounds, float minDistance, System.Random rng = null)
    {
        this.bounds = bounds;
        this.minDistance = minDistance;
        this.points = new List<Vector2>();
        this.kdTree = new KDTree();
        this.rng = (rng == null ? new System.Random() : rng);
    }
#endregion

#region Public methods
    public Vector2 AddPoint(Vector2 pos)
    {
        points.Add(pos);
        kdTree.AddPoint(pos);
        return pos;
    }

    public Vector2 AddPoint()
    {
        Vector3 pos;
        float distance;

        int tries = 0;

        // find a point not too close to the others
        do 
        {
            tries++; 
            if (tries > nFails)
            {
                tries = 0;
                minDistance *= Sqrt2 / 2f; // halve the distance every 2 fails
            }

            pos = bounds.RandomPoint(rng);
            kdTree.Nearest(pos, out distance);

        } while (distance < minDistance);

        AddPoint(pos);
        return pos;
    }

    public void AddPoints(int nPoints)
    {
        for (int k = 0; k < nPoints; k++)
        {
            AddPoint();
        }
    }
#endregion
}
}
