/**
 * Half-edge Graph data structure
 * 
 * Note: This is just a container for keeping track of nodes, edges and faces. 
 * It does not maintain any guarantees about the structure of the graph
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 6.0
 */

using System;
using UnityEngine;
using System.Collections.Generic;

namespace WordsOnPlay.Geometry 
{
    public class Graph
    {
        private HashSet<Vertex> vertices;
        public IEnumerable<Vertex> Vertices => vertices;

        private HashSet<HalfEdge> edges;
        public IEnumerable<HalfEdge> Edges => edges;

        private HashSet<Face> faces;    // internal faces only
        public IEnumerable<Face> Faces => faces;
        
        private Face exterior;          // external face
        public Face Exterior => exterior;

#region Constructors
        public Graph()
        {
            vertices = new HashSet<Vertex>();
            edges = new HashSet<HalfEdge>();
            faces = new HashSet<Face>();
            exterior = new Face();  // exterior is not in faces
        }
#endregion

#region Vertices
        public Vertex AddVertex(Vector2 position, string name)
        {
            Vertex v = new Vertex(position, name);
            vertices.Add(v);
            return v;            
        }

        public bool RemoveVertex(Vertex vertex)
        {
            return vertices.Remove(vertex);
        }
#endregion

#region Face
        public Face AddFace(HalfEdge edge = null)
        {
            Face f = new Face(edge);
            faces.Add(f);
            return f;          
        }

        public bool RemoveFace(Face face)
        {
            return faces.Remove(face);
        }

#endregion

#region Edges
        public HalfEdge AddEdge(Vertex va) 
        {
            HalfEdge e = new HalfEdge(va);
            edges.Add(e);
            return e;
        }

        public HalfEdge AddEdge(Vertex va, Vertex vb) 
        {
            HalfEdge eAB = new HalfEdge(va);
            HalfEdge eBA = new HalfEdge(vb);
            eAB.flip = eBA;
            eBA.flip = eAB;

            edges.Add(eAB);
            edges.Add(eBA);

            // return the forward edge
            return eAB;
        }

        public bool RemoveEdge(HalfEdge e)
        {
            return edges.Remove(e);
        }

        public bool RemoveEdgePair(HalfEdge e)
        {
            return RemoveEdge(e) && RemoveEdge(e.flip);
        }
#endregion


    }
}