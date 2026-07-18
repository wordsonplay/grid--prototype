/**
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;
using System;
using System.Text;

namespace WordsOnPlay.Geometry
{

[Serializable]
public class Face 
{
    public HalfEdge edge;

    internal Face()
    {
        this.edge = null;
    }

    internal Face(HalfEdge edge)
    {
        this.edge = edge;
    }

    /// <summary>
    /// Test if a given point is inside the face.
    /// Note: this assumes the face is convex, and that winding order is anti-clockwise
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>

    public bool Contains(Vector2 point, bool strict = true)
    {
        HalfEdge e = edge;

        do {
            int side = e.Side(point);

            // inside points should be on the left, i.e. side == 1

            if (side == -1 || (side == 0 && strict))
            {
                return false;
            }
            e = e.next;
        } while (e != edge);

        return true;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("F[");
        HalfEdge e = edge;
        if (edge != null)
        {
            do
            {
                sb.Append(e.fromVertex);
                e = e.next;
            } while (e != edge);            
        }
        sb.Append("]");

        return sb.ToString();
    }
}

}