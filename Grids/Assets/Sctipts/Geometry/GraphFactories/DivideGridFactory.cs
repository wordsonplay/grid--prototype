using UnityEngine;
using System.Collections.Generic;

namespace WordsOnPlay.Geometry
{
public class DivideGridFactory : IGraphFactory
{

    private Graph baseGraph;

    public DivideGridFactory(Graph graph)
    {
        this.baseGraph = graph;        
    }

    public Graph MakeGraph()
    {
        Graph graph = GraphOperations.Clone(baseGraph);
        SplitEdges(graph);
        SplitFaces(graph);

        return graph;        
    }


    private Graph SplitEdges(Graph graph)
    {
        List<HalfEdge> edges = new List<HalfEdge>(graph.Edges);

        int nv = 0;
        while (edges.Count > 0)
        {
            HalfEdge eab = edges[0];
            HalfEdge eba = eab.Flip;
            edges.Remove(eab);
            edges.Remove(eba);
            
            Vector2 a = eab.fromVertex;
            Vector2 b = eba.fromVertex;
            Vector2 c = (a+b) / 2;

            Vertex vC = graph.AddVertex(c, $"m_{nv}");

            //  A --\C---\ 
            //   \---C\-- B

            HalfEdge ecb = graph.AddEdge(vC);
            ecb.face = eab.face;
            ecb.Next = eab.Next;
            eab.Next = ecb;
            
            HalfEdge eca = graph.AddEdge(vC);
            eca.face = eba.face;
            eca.Next = eba.Next;
            eba.Next = eca;

            eab.Flip = eca;
            eba.Flip = ecb;

            nv++;
        }

        return graph;        
    }


    private Graph SplitFaces(Graph graph)
    {
        List<Face> faces = new List<Face>(graph.Faces);
        foreach (Face f in faces)
        {
            Vertex vc = AddVertex(graph, f);
            SplitFace(graph, f, vc);
        }

        return graph;        
    }

    private Vertex AddVertex(Graph graph, Face face)
    {
        // average the vertex positions
        Vector2 p = Vector2.zero;
        HalfEdge e = face.edge;
        int n = 0;
        do
        {
            p += e.fromVertex;
            e = e.Next;
            n++;
        } while (e != face.edge);
        p /= n;

        return graph.AddVertex(p, $"c({face})");        
    }

    private void SplitFace(Graph graph, Face face, Vertex vc)
    {
        // this assumes f.edge is one of the 'old' edges, heading into a midpoint
        HalfEdge e = face.edge;
        HalfEdge ecmPrev = null;
        HalfEdge embPrev = null;
        HalfEdge eFirst = null;
        do
        {
            HalfEdge eam = e;
            e = e.Next;
            HalfEdge emb = e;
            e = e.Next;

            // B\---M\---A
            //    /| |
            //     | |/
            //      C ---\
            //        prev

            Vertex vm = emb.fromVertex;
            HalfEdge emc = graph.AddEdge(vm, vc);
            HalfEdge ecm = emc.Flip;

            eam.Next = emc;
            emc.Next = ecmPrev;
            ecm.Next = emb;

            if (eFirst == null)
            {
                eFirst = emc;
                emc.face = face;
            }
            else
            {
                Face f = graph.AddFace(emc);
                f.edge = eam;
                eam.face = f;
                emc.face = f;
                ecmPrev.face = f;
                embPrev.face = f;
            }

            ecmPrev = ecm;
            embPrev = emb;
        } while (e != face.edge);

        // connect the first edge to the last
        eFirst.Next = ecmPrev;
        ecmPrev.face = face;
        embPrev.face = face;        
    }
}
}
