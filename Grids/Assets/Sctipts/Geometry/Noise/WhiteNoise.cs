/**
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using WordsOnPlay.Utils;

namespace WordsOnPlay.Geometry
{

public class WhiteNoise : IEnumerable<Vector2>
{

#region State
    private Rect bounds;
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
    public WhiteNoise(Rect bounds, System.Random rng = null)
    {
        this.bounds = bounds;
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
        Vector2 pos = bounds.RandomPoint(rng);
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
