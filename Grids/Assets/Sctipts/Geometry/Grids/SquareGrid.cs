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

public class SquareGrid : Graph
{

#region State
    private Vector2Int dimension;
    private Rect bounds;
    private System.Random rng;

    private Vertex[,] vertices;
    private HalfEdge[,] hEdges;
    private HalfEdge[,] vEdges;
#endregion
    
#region Constructor
    public SquareGrid(int width, int height, float zigProb = 0.5f, float jitter = 0, System.Random rng = null) :
        this(new Vector2Int(width, height), zigProb, jitter, rng)
    {       
    }

    public SquareGrid(Vector2Int dimension, float zigProb = 0.5f, float jitter = 0, System.Random rng = null)
    {
        this.dimension = dimension;
        this.bounds = new Rect(0,0,dimension.x, dimension.y);
        this.rng = (rng == null ? new System.Random() : rng);

        MakeVertices(jitter);
        MakeGridEdges();
        MakeSquares();
        MakeTriangles(zigProb);
    }
#endregion

#region Making the Graph

    private void MakeVertices(float jitter)
    {
        int dx = dimension.x;
        int dy = dimension.y;
        vertices = new Vertex[dx + 1, dy + 1];

        for (int x = 0; x <= dx; x++)
        {
            for (int y = 0; y <= dy; y++)
            {
                Vector2 p = new Vector2(x,y);

                float angle = 360 * (float)rng.NextDouble();
                float radius = jitter * (float)rng.NextDouble();
                Vector2 offset = jitter * Vector2.right.Rotate(angle);
                p += offset;

                vertices[x, y] = AddVertex(p, $"v({x},{y})");
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
                hEdges[x, y] = AddEdge(vertices[x, y], vertices[x + 1, y]);
            }
        }

        vEdges = new HalfEdge[dx + 1, dy];
        for (int x = 0; x <= dx; x++)
        {
            for (int y = 0; y < dy; y++)
            {
                vEdges[x, y] = AddEdge(vertices[x, y], vertices[x, y + 1]);
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
                HalfEdge ecd = hEdges[x, y + 1].flip;
                HalfEdge eda = vEdges[x, y].flip;

                eab.face = ebc.face = ecd.face = eda.face = AddFace(eab);

                eab.next = ebc;
                ebc.next = ecd;
                ecd.next = eda;
                eda.next = eab;
            }
        }

        // Set external faces
        for (int x = 0; x < dx; x++)
        {
            hEdges[x, 0].flip.face = Exterior;
            hEdges[x, 0].flip.next = (x == 0 ? vEdges[0, 0] : hEdges[x - 1, 0].flip);

            hEdges[x, dy].face = Exterior;
            hEdges[x, dy].next = (x == dx - 1 ? vEdges[dx, dy - 1].flip : hEdges[x + 1, dy]);
        }

        for (int y = 0; y < dy; y++)
        {
            vEdges[0, y].face = Exterior;
            vEdges[0, y].next = (y == dy - 1 ? hEdges[0, dy] : vEdges[0, y + 1]);

            vEdges[dx, y].flip.face = Exterior;
            vEdges[dx, y].flip.next = (y == 0 ? hEdges[dx - 1, 0] : vEdges[dx, y - 1].flip);
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
                HalfEdge ecd = hEdges[x + 1, y].flip;
                HalfEdge eda = vEdges[x, y].flip;

                float r = (float)rng.NextDouble();
                bool zig = r <= zigProb;
                // avoid creating 'ears' on the corners
                zig = zig || ((x == 0) && (y == 0));
                zig = zig || ((x == dx-1) && (y == dy-1));
                zig = zig && !((x == dx-1) && (y == 0));
                zig = zig && !((x == 0) && (y == dy-1));

                if (zig)
                {
                    //  D---C
                    //  |f /|
                    //  | / |
                    //  |/  |
                    //  A---B

                    HalfEdge eac = AddEdge(va, vc);
                    HalfEdge eca = eac.flip;

                    // triangle ABC
                    eca.face = eab.face;
                    eca.next = eab;
                    ebc.next = eca;

                    // triangle ACD
                    Face f = AddFace(eac);
                    eac.face = f;
                    eac.next = ecd;
                    eda.next = eac;
                }
                else
                {
                    //  D---C
                    //  |\f |
                    //  | \ |
                    //  |  \|
                    //  A---B

                    HalfEdge ebd = AddEdge(vb, vd);
                    HalfEdge edb = ebd.flip;

                    // triangle ABD
                    ebd.face = eab.face;
                    ebd.next = eda;
                    eab.next = ebd;

                    // triangle BCD
                    Face f = AddFace(edb);
                    edb.face = f;
                    edb.next = ebc;
                    ecd.next = edb;
                }
            }
        }
    }
#endregion
}
}
