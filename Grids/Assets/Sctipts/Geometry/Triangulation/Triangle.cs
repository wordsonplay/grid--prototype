/**
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;
using System.Collections.Generic;

namespace WordsOnPlay.Geometry
{

public class Triangle 
{
    public List<Triangle> children;
    public HalfEdge[] edges;
    public Face face;
    public string Name => $"T[{edges[0].fromVertex.name},{edges[1].fromVertex.name},{edges[2].fromVertex.name}]";

    public Vector2 Circumcentre
    {
        get
        {
            // https://en.wikipedia.org/wiki/Circumcircle#Cartesian_coordinates_2

            Vector2 a = edges[0].fromVertex.position;         
            Vector2 b = edges[1].fromVertex.position;         
            Vector2 c = edges[2].fromVertex.position;
            b = b - a;
            c = c - a;

            Vector2 u = new Vector2();
            u.x = c.y * b.sqrMagnitude - b.y * c.sqrMagnitude;
            u.y = b.x * c.sqrMagnitude - c.x * b.sqrMagnitude;
            float d = 2 * (b.x * c.y - b.y * c.x);
            u = u / d;
            u = u + a;

            return u;
        }        
    }

    public float Radius
    {
        get
        {
            Vector2 a = edges[0].fromVertex.position;
            Vector2 v = Circumcentre - a;
            return v.magnitude;                   
        }        
    }

    public Triangle(HalfEdge e0, HalfEdge e1, HalfEdge e2)
    {
        this.edges = new HalfEdge[3];
        edges[0] = e0;
        edges[1] = e1;
        edges[2] = e2;
        
        this.face = e0.face;
        this.children = null;
    }

    public bool Contains(Vector2 p) 
    {
        for (int i = 0; i < 3; i++) 
        {
            if (edges[i].Side(p) < 0)
            {
                return false;
            }
        }
        return true;
    }


}

}
