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
using WordsOnPlay.Geometry;

namespace WordsOnPlay.Geometry
{

public class SquareGridFactory : IGraphFactory
{

#region State
    private Vector2Int dimension;
    private System.Random rng;
    private float scale = 2;
    private float jitter = 0;
    private float zigProb = 0.5f;

    private Graph graph;
    private Vertex[,] vertices;
    private HalfEdge[,] hEdges;
    private HalfEdge[,] vEdges;
#endregion
    
#region Constructor
    public SquareGridFactory(int width, int height, float zigProb = 0.5f, float jitter = 0, float scale = 2.5f, System.Random rng = null) :
        this(new Vector2Int(width, height), zigProb, jitter, scale, rng)
    {
    }

    public SquareGridFactory(Vector2Int dimension, float zigProb = 0.5f, float jitter = 0, float scale = 2.5f, System.Random rng = null)
    {
        this.dimension = dimension;
        this.rng = rng ?? new System.Random();
        this.zigProb = zigProb;
        this.jitter = jitter;
        this.scale = scale;
    }
#endregion

#region Making the Graph
    public Graph MakeGraph()
    {
        graph = new Graph();
        MakeVertices(jitter);
        MakeGridEdges();
        MakeSquares();
        MakeTriangles(zigProb);        

        return graph;
    }

    private void MakeVertices(float jitter)
    {
        int dx = dimension.x;
        int dy = dimension.y;
        vertices = new Vertex[dx + 1, dy + 1];

        for (int x = 0; x <= dx; x++)
        {
            for (int y = 0; y <= dy; y++)
            {
                Vector2 p = new Vector2(x,y) * scale;

                float angle = 360 * (float)rng.NextDouble();
                float radius = jitter * (float)rng.NextDouble();
                Vector2 offset = jitter * Vector2.right.Rotate(angle);
                p += offset;

                vertices[x, y] = graph.AddVertex(p, $"v({x},{y})");
            }
        }
    }

    private void MakeGridEdges()
    {
        int dx = dimension.x;
        int dy = dimension.y;

        hEdges = new HalfEdge[dx, dy + 1];
        for (int x = 0; x < dx; x++)
        {
            for (int y = 0; y <= dy; y++)
            {
                hEdges[x, y] = graph.AddEdge(vertices[x, y], vertices[x + 1, y]);
            }
        }

        vEdges = new HalfEdge[dx + 1, dy];
        for (int x = 0; x <= dx; x++)
        {
            for (int y = 0; y < dy; y++)
            {
                vEdges[x, y] = graph.AddEdge(vertices[x, y], vertices[x, y + 1]);
            }
        }
    }

    private void MakeSquares()
    {
        int dx = dimension.x;
        int dy = dimension.y;

        // create squares
        for (int x = 0; x < dx; x++)
        {
            for (int y = 0; y < dy; y++)
            {
                //  D--C
                //  |  |
                //  A--B

                HalfEdge eab = hEdges[x, y];
                HalfEdge ebc = vEdges[x + 1, y];
                HalfEdge ecd = hEdges[x, y + 1].Flip;
                HalfEdge eda = vEdges[x, y].Flip;

                eab.face = ebc.face = ecd.face = eda.face = graph.AddFace(eab);

                eab.Next = ebc;
                ebc.Next = ecd;
                ecd.Next = eda;
                eda.Next = eab;
            }
        }

        graph.Exterior.edge = hEdges[0,0].Flip;

        // Set external faces
        for (int x = 0; x < dx; x++)
        {
            hEdges[x, 0].Flip.face = graph.Exterior;
            hEdges[x, 0].Flip.Next = (x == 0 ? vEdges[0, 0] : hEdges[x - 1, 0].Flip);

            hEdges[x, dy].face = graph.Exterior;
            hEdges[x, dy].Next = (x == dx - 1 ? vEdges[dx, dy - 1].Flip : hEdges[x + 1, dy]);
        }

        for (int y = 0; y < dy; y++)
        {
            vEdges[0, y].face = graph.Exterior;
            vEdges[0, y].Next = (y == dy - 1 ? hEdges[0, dy] : vEdges[0, y + 1]);

            vEdges[dx, y].Flip.face = graph.Exterior;
            vEdges[dx, y].Flip.Next = (y == 0 ? hEdges[dx - 1, 0].Flip : vEdges[dx, y - 1].Flip);
        }
    }

    private void MakeTriangles(float zigProb)
    {
        int dx = dimension.x;
        int dy = dimension.y;

        // Create triangles
        for (int x = 0; x < dx; x++)
        {
            for (int y = 0; y < dy; y++)
            {
                Vertex va = vertices[x, y];
                Vertex vb = vertices[x + 1, y];
                Vertex vc = vertices[x + 1, y + 1];
                Vertex vd = vertices[x, y + 1];

                HalfEdge eab = hEdges[x, y];
                HalfEdge ebc = vEdges[x + 1, y];
                HalfEdge ecd = hEdges[x, y + 1].Flip;
                HalfEdge eda = vEdges[x, y].Flip;

                float r = (float)rng.NextDouble();

                if (r <= zigProb)
                {
                    //  D---C
                    //  |f /|
                    //  | / |
                    //  |/  |
                    //  A---B

                    HalfEdge eac = graph.AddEdge(va, vc);
                    HalfEdge eca = eac.Flip;

                    // triangle ABC
                    eca.face = eab.face;
                    eca.Next = eab;
                    ebc.Next = eca;

                    // triangle ACD
                    Face f = graph.AddFace(eac);
                    eac.face = f;
                    ecd.face = f;
                    eda.face = f;

                    eac.Next = ecd;
                    eda.Next = eac;
                }
                else
                {
                    //  D---C
                    //  |\f |
                    //  | \ |
                    //  |  \|
                    //  A---B

                    HalfEdge ebd = graph.AddEdge(vb, vd);
                    HalfEdge edb = ebd.Flip;

                    // triangle ABD
                    ebd.face = eab.face;
                    ebd.Next = eda;
                    eab.Next = ebd;

                    // triangle BCD
                    Face f = graph.AddFace(edb);
                    edb.face = f;
                    ebc.face = f;
                    ecd.face = f;

                    edb.Next = ebc;
                    ecd.Next = edb;
                }
            }
        }
    }
#endregion
}
}
