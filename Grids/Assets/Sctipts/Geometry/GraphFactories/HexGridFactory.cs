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

public class HexGridFactory : IGraphFactory
{

#region State
    private int radius;
    private System.Random rng;
    private float scale = 2;

    private Graph graph;
    private Vertex[] vertices;
#endregion
    
#region Constructor
    public HexGridFactory(int radius, float scale = 1f, System.Random rng = null)
    {
        this.radius = radius;
        this.rng = rng ?? new System.Random();
        this.scale = scale;
    }
#endregion

#region Making the Graph
    public Graph MakeGraph()
    {
        graph = new Graph();
        MakeVertices();

        return graph;
    }

    private void MakeVertices()
    {
        int n = radius * (radius + 1) / 2;
        vertices = new Vertex[6 * n + 1];
        float h = Mathf.Sqrt(3f) / 2f;

        for (int y = -radius; y <= radius; y++)
        {
            float yf = (float) y * h;

            int yAbs = y < 0 ? -y : y;
            int min = -radius + yAbs / 2;       // round down
            int max = radius - (yAbs + 1) / 2;  // round up

            int nv = 0;
            for (int x = min; x <= max; x++)
            {                
                float xf = (float)x + (yAbs % 2 == 0 ? 0 : 0.5f);

                Vector2 p = new Vector2(xf, yf) * scale;
                vertices[nv++] = graph.AddVertex(p, $"v({x},{y})");
            }
        }
    }

#endregion
}
}
