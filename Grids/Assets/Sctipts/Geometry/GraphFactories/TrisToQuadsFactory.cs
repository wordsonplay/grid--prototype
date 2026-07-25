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

public class TrisToQuadsFactory
{

#region State
    private Graph baseGraph;
    private Graph graph;
    private System.Random rng;
#endregion
    
#region Constructor
    public TrisToQuadsFactory(Graph graph, System.Random rng = null)
    {
        this.baseGraph = graph;        
        this.rng = rng ?? new System.Random();
    }
#endregion

#region Making the Graph
    public Graph MakeGraph()
    {
        graph = GraphOperations.Clone(baseGraph);

        List<Face> faces = new List<Face>(graph.Faces);
        Shuffle<Face>(faces);
        Queue<Face> queue = new Queue<Face>(faces);

        HashSet<Face> isolatedTriangles = new HashSet<Face>();
        HashSet<Face> merged = new HashSet<Face>();
        HashSet<Face> quads = new HashSet<Face>();
        List<HalfEdge> validEdges = new List<HalfEdge>(3);

        while (queue.Count > 0)
        {
            Face face = queue.Dequeue();
            if (!merged.Contains(face))
            {
                validEdges.Clear();
                HalfEdge e = face.edge;
                do
                {
                    if (e.Flip.face != graph.Exterior)
                    {
                        validEdges.Add(e);                    
                    }

                    e = e.Next;
                } while (e != face.edge);

                if (validEdges.Count == 0)
                {
                    isolatedTriangles.Add(face);                
                }
                else
                {
                    int r = rng.Next(validEdges.Count);
                    e = validEdges[r];
                    merged.Add(face);
                    merged.Add(e.Flip.face);
                    Face quad = GraphOperations.DeleteEdge(graph, e);
                    quads.Add(quad);
                }                
            }
        }

        return graph;
    }

    private void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

#endregion
}
}
