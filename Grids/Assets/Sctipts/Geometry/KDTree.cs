/**
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using WordsOnPlay.Utils;

namespace WordsOnPlay.Geometry
{

[Serializable]
public class KDTree
{
#region Private classes
    private class KDNode 
    {
        public KDNode[] children;
        public Vector2 point;
        public int splitAxis;

        public float SplitValue {
            get {
                return point[splitAxis];
            }
        }

        public KDNode(Vector2 point, int splitAxis)
        {
            this.point = point;
            this.splitAxis = splitAxis;
            this.children = new KDNode[2];
        }

        public int Child(Vector2 v) 
        {
            return (v[splitAxis] <= SplitValue ? 0 : 1);
        }

    }

    private class Comparer : IComparer<Vector2> 
    {
        private int axis;

        public Comparer(int axis) 
        {
            this.axis = axis;
        }

        public int Compare(Vector2 a, Vector2 b)
        {
            return Math.Sign(a[axis] - b[axis]);
        }
    }
#endregion

#region Fields
    private Comparer[] comparers;

    private KDNode root = null;
    private int dimension;
    private Rect bounds;
    private int count = 0;
#endregion

#region Properties
    public int Count { get { return count; } }
#endregion

#region Constructors
    public KDTree() 
    {
        dimension = 2;
        comparers = MakeComparers(dimension);
        bounds = MakeBounds();
        count = 0;
        root = null;
    }

    public KDTree(Vector2[] points) 
    {
        dimension = 2;
        comparers = MakeComparers(dimension);
        bounds = MakeBounds(points);
        count = points.Length;
        root = MakeTree(points, 0, points.Length, 0);
    }

    private Comparer[] MakeComparers(int dimension) 
    {
        Comparer[] comparers = new Comparer[dimension];
        for (int i = 0; i < dimension; i++) {
            comparers[i] = new Comparer(i);
        }
        return comparers;
    }

    private Rect MakeBounds()
    {
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        return new Rect(min, max - min);
    }

    private Rect MakeBounds(Vector2[] vertices)
    {
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        for (int i = 0; i < vertices.Length; i++) 
        {
            min.x = Mathf.Min(min.x, vertices[i].x);
            min.y = Mathf.Min(min.y, vertices[i].y);
            max.x = Mathf.Max(max.x, vertices[i].x);
            max.y = Mathf.Max(max.y, vertices[i].y);
        }

        return new Rect(min, max - min);
    }

    private KDNode MakeTree(Vector2[] vertices, int start, int len, int axis, KDNode parent = null) 
    {
        if (vertices.Length == 0)
        {
            return null;
        }

        Array.Sort(vertices, start, len, comparers[axis]);

        int mid = start + len / 2;

        KDNode node = new KDNode(vertices[mid], axis);

        axis = (axis + 1) % dimension;

        if (mid > start)
        {
            node.children[0] = MakeTree(vertices, start, len / 2, axis, node);
        }

        if (len > 2) 
        {
            node.children[1] = MakeTree(vertices, mid + 1, len - len / 2 - 1, axis, node);
        }

        return node;        
    }
#endregion

#region Update
    public void Rebuild() 
    {
        List<Vector2> vertices = new List<Vector2>();
        CollectVertices(root, vertices);
        root = MakeTree(vertices.ToArray(), 0, vertices.Count, 0);
    }

    private void CollectVertices(KDNode node, List<Vector2> vertices)
    {
        if (node == null)
        {
            return;
        }

        CollectVertices(node.children[0], vertices);
        vertices.Add(node.point);
        CollectVertices(node.children[1], vertices);
    }

    public void AddPoint(Vector2 v)
    {
        count++;
      
        if (root == null) 
        {
            root = new KDNode(v, 0);
            bounds = new Rect(v.x, v.y, 0, 0);
        }
        else 
        {
            KDNode parent = FindNode(root, v);
            KDNode node = new KDNode(v, (parent.splitAxis + 1) % dimension);
            int next = parent.Child(v);
            parent.children[next] = node;

            UpdateBounds(v);
        }
    }

    private KDNode FindNode(KDNode node, Vector2 v)
    {
        int next = node.Child(v);

        while (node.children[next] != null)
        {
            node = node.children[next];
            next = node.Child(v);            
        }
        return node;
    }

    private void UpdateBounds(Vector2 v)
    {
        Vector2 min = bounds.min;
        Vector2 max = bounds.max;

        min.x = Mathf.Min(min.x, v.x);
        min.y = Mathf.Min(min.y, v.y);
        max.x = Mathf.Max(max.x, v.x);
        max.y = Mathf.Max(max.y, v.y);

        bounds.min = min;
        bounds.max = max;
    }
#endregion

#region Get Nearest

    public Vector2? Nearest(Vector2 v, out float distance)
    {
        KDNode nearest = null;
        distance = float.PositiveInfinity;
        Nearest(root, v, ref nearest, ref distance);

        if (nearest == null)
        {
            return null;
        }
        else 
        {
            return nearest.point;
        }
    }

    private void Nearest(KDNode node, Vector2 v, ref KDNode nearest, ref float dNearest)
    {
        if (node == null)
        {
            return;
        }

        float d = Vector2.Distance(v, node.point);
        if (d < dNearest) 
        {
            nearest = node;
            dNearest = d;
        }

        int next = node.Child(v);
        Nearest(node.children[next], v, ref nearest, ref dNearest);

        float dSplit = Mathf.Abs(v[node.splitAxis] - node.SplitValue);
        if (dSplit < dNearest)
        {
            // search the other side
            Nearest(node.children[1-next], v, ref nearest, ref dNearest);
        }

    }
#endregion

#region Gizmos
    public void DrawGizmo(Transform transform = null) 
    {
        bounds.DrawGizmo(transform);
        DrawGizmo(root, bounds, transform);
    }

    private void DrawGizmo(KDNode node, Rect rect, Transform transform) 
    {
        if (node == null) 
        {
            return;

        }
        Vector2 min = rect.min;
        Vector2 max = rect.max;

        min[node.splitAxis] = node.point[node.splitAxis];
        max[node.splitAxis] = node.point[node.splitAxis];

        if (transform != null)
        {
            Gizmos.DrawLine(transform.TransformPoint(min), transform.TransformPoint(max));
        }
        else 
        {
            Gizmos.DrawLine(min, max);
        }

        Rect r = rect;
        r.max = max;
        DrawGizmo(node.children[0], r, transform);

        r = rect;
        r.min = min;
        DrawGizmo(node.children[1], r, transform);

    }
#endregion

}

}