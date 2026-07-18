/**
 * 
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 6000.0.53f1
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WordsOnPlay.Utils;

namespace WordsOnPlay.Geometry
{

public class Grid
{
#region Constants
    const float Sqrt2 = 1.4142135623730951f;
#endregion

#region State
    private Rect bounds;
    private int nPoints;
    private List<Vector2> points;
    private System.Random rng;

    private Triangulation triangulation;
    private Graph graph;
#endregion

#region Properties
    public Graph Graph => graph;
#endregion

#region Constructor
    public Grid(IEnumerable<Vector2> points, System.Random rng = null)
    {
        this.rng = (rng == null ? new System.Random() : rng);
        this.points = new List<Vector2>(points);

        Triangulate();
        graph = triangulation.MakeGraph();
        RemoveEdges();
    }
#endregion 

#region Grid generation
    private void Triangulate()
    {
        triangulation = new Triangulation(bounds);

        for (int i = 0; i < points.Count; i++)
        {
            triangulation.AddVertex(points[i], $"V{i}");
        }

        triangulation.Run();
    }

    private void RemoveEdges()
    {
        // this should really be a set, but getting a random element is tricky
        List<Face> triangles = new List<Face>();
        HashSet<Face> squares = new HashSet<Face>();
        HashSet<Face> isolated = new HashSet<Face>();

        foreach (Face f in graph.Faces)
        {
            triangles.Add(f);
        }

        List<HalfEdge> edges = new List<HalfEdge>();
        while (triangles.Count > 0)
        {
            // pick a random triangle
            int i = rng.Next(triangles.Count);
            Face face = triangles[i];

            // look for available edges to remove
            edges.Clear();
            CandidateEdges(face, triangles, edges);

            if (edges.Count == 0)
            {
                triangles.RemoveAt(i);
                isolated.Add(face);
            }
            else
            {
                int j = rng.Next(edges.Count);
                HalfEdge edge = edges[j];
                triangles.Remove(face);
                triangles.Remove(edge.flip.face);

                Face merged = GraphOperations.DeleteEdge(graph, edge);
                squares.Add(merged);
            }

        }

    }

    private List<HalfEdge> CandidateEdges(Face face, List<Face> triangles, List<HalfEdge> edges)
    {
        edges.Clear();
        HalfEdge e = face.edge;
        do
        {
            Face n = e.flip.face;
            if (triangles.Contains(n))
            {
                if (!BadEdge(e) && !BadEdge(e.flip))
                {
                    edges.Add(e);                                        
                }
            }
            e = e.next;
        } while (e != face.edge);

        return edges;
    }

    private bool BadEdge(HalfEdge e)
    {
        // don't remove an edge if it would leave a vertex with only 2 edges
        // I might want to relax this for external edges
                    
        if (GraphOperations.EdgeCount(e.fromVertex) <= 3)
        {
            return true;
        }

        // don't remove an edge if the resulting quad would be convex

        Vector2 pC = e.next.fromVertex.position;
        Vector2 pL = e.next.next.fromVertex.position;
        Vector2 pR = e.flip.next.next.fromVertex.position;

        if (pC.IsOnLeft(pL, pR))
        {
            // return true;
        }

        return false;
    }
#endregion


#region Relaxation
    private IEnumerator RelaxCR()
    {
        var vertices = graph.Vertices;
        List<Vertex> available = new List<Vertex>(vertices);

        while (true)
        {
            int i = rng.Next(available.Count);
            Vertex v = available[i];
            RelaxVertex(v);
            yield return null;
            available.RemoveAt(i);
            if (available.Count == 0)
            {
                available.AddRange(vertices);
            }
        }
    }

    private void RelaxVertex(Vertex vertex)
    {
        HalfEdge eNext = vertex.edge;

        do
        {
            HalfEdge ePrev = eNext.flip;
            eNext = ePrev.next;

            if (ePrev.face != graph.Exterior)
            {
                Vertex vLeft = ePrev.fromVertex;
                Vertex vRight = eNext.next.fromVertex;
                RelaxTriangle(vertex, vLeft, vRight);                
            }
        }
        while (eNext != vertex.edge);
    }

    private void RelaxTriangle(Vertex vertex, Vertex vLeft, Vertex vRight)
    {
        Vector2 p = vertex.position;
        Vector2 pLeft = vLeft.position;
        Vector2 pRight = vRight.position;

        //         Q
        //     P   |
        //         |
        //  L -----M----- R
        //
        // move P towards Q

        Vector2 v = (pRight - pLeft).normalized;
        Vector2 n = v.Perp();
        Vector2 mid = (pRight + pLeft) / 2;
        Vector2 q = mid - n * Sqrt2 / 2f;

        // move the vertex towards the target
        vertex.position = Vector2.Lerp(p, q, 0.1f);
    }

#endregion


#region Verification
    public void Verify()
    {
        var vertexErrors = new Dictionary<Vertex,string>();
        var edgeErrors = new Dictionary<HalfEdge,string>(); 
        var faceErrors = new Dictionary<Face,string>();         

        GraphOperations.VerifyVertices(graph, vertexErrors);
        foreach (Vertex v in vertexErrors.Keys)
        {
            if (vertexErrors[v] != null)
            {
                Debug.LogWarning($"[GridFactory.Verify] {v}: {vertexErrors[v]}");
            }            
        }

        GraphOperations.VerifyEdges(graph, edgeErrors);
        foreach (HalfEdge e in edgeErrors.Keys)
        {
            if (edgeErrors[e] != null)
            {
                Debug.LogWarning($"[GridFactory.Verify] {e}: {edgeErrors[e]}");
            }            
        }

        GraphOperations.VerifyFaces(graph, faceErrors);
        foreach (Face f in faceErrors.Keys)
        {
            if (faceErrors[f] != null)
            {
                Debug.LogWarning($"[GridFactory.Verify] {f}: {faceErrors[f]}");
            }            
        }

    }

#endregion 

}
}