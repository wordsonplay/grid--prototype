/**
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;
using System;

namespace WordsOnPlay.Geometry
{

public class HalfEdge 
{
    public Vertex fromVertex;
    public HalfEdge next;
    public HalfEdge flip;
    public Face face;

    public Vector2 Direction => next.fromVertex - fromVertex;
    public override string ToString() => $"E[{fromVertex},{flip.fromVertex}]";
    
    internal HalfEdge(Vertex fromVertex)
    {
        this.fromVertex = fromVertex;
    }

    /// <summary>
    /// Test which side of the edge the point lies on
    /// </summary>
    /// <param name="p">The test point</param>
    /// <returns>-1 if p is on the right, 0 if p is on the edge, 1 if p is on th left</returns>

    public int Side(Vector2 p)
    {
        Vector2 a = Direction;
        Vector2 b = p - fromVertex;

        return Math.Sign(a.x * b.y - a.y * b.x);
    }

    public Vector2 Lerp(float t)
    {
        return fromVertex + t * Direction;
    }

    public Vector2 Nearest(Vector2 p)
    {
        Vector2 u = p - fromVertex;
        Vector2 v = Direction;

        float t = Vector2.Dot(u, v) / Vector2.Dot(v, v);

        if (t <= 0)
        {
            return fromVertex;
        }
        else if (t >= 1)
        {
            return next.fromVertex;
        }

        return Lerp(t);
    }

    public float Distance(Vector2 p)
    {
        Vector2 q = Nearest(p);
        return Vector2.Distance(p, q);
    }

}
}
