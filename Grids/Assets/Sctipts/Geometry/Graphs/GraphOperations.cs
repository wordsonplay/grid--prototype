/**
 * Operations on Graph strucutre
 * 
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 6.0
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WordsOnPlay.Geometry
{
    public static class GraphOperations
    {

#region Clone
        public static Graph Clone(Graph graph)
        {
            Graph copy = new Graph();

            Dictionary<Vertex, Vertex> vMap = CloneVertices(copy, graph);
            Dictionary<HalfEdge, HalfEdge> eMap = CloneEdges(copy, graph, vMap);
            Dictionary<Face, Face> fMap = CloneFaces(copy, graph, eMap);

            // connect them together
            foreach (Vertex vOld in graph.Vertices)
            {
                Vertex vNew = vMap[vOld];
                HalfEdge eOld = vOld.edge;

                if (vOld.edge != null)
                {
                    HalfEdge eNew = eMap[eOld];
                    vNew.edge = eNew;

                    do
                    {
                        eNew.Next = eMap[eOld.Next];
                        eNew.Flip = eMap[eOld.Flip];
                        eNew.face = fMap[eOld.face];

                        eOld = eOld.Flip.Next;
                        eNew = eMap[eOld];
                    } while (eOld != vOld.edge);
                }
            }
            return copy;
        }

        private static Dictionary<Vertex, Vertex> CloneVertices(Graph copy, Graph graph)
        {
            Dictionary<Vertex, Vertex> vMap = new Dictionary<Vertex, Vertex>();

            foreach (Vertex vOld in graph.Vertices)
            {
                Vertex vNew = copy.AddVertex(vOld.position, vOld.name);
                vMap[vOld] = vNew;
            }

            return vMap;
        }

        private static Dictionary<HalfEdge, HalfEdge> CloneEdges(Graph copy, Graph graph, Dictionary<Vertex, Vertex> vMap)
        {
            Dictionary<HalfEdge, HalfEdge> eMap = new Dictionary<HalfEdge, HalfEdge>();

            // copy outgoing edges from each vertex 
            foreach (Vertex vOld in graph.Vertices)
            {
                Vertex vNew = vMap[vOld];

                if (vOld.edge != null)
                {
                    HalfEdge eOld = vOld.edge;

                    do
                    {
                        HalfEdge eNew = copy.AddEdge(vNew);
                        eMap[eOld] = eNew;

                        eOld = eOld.Flip.Next;
                    } while (eOld != vOld.edge);
                }
            }

            return eMap;
        }

        private static Dictionary<Face, Face> CloneFaces(Graph copy, Graph graph, Dictionary<HalfEdge, HalfEdge> eMap)
        {
            Dictionary<Face, Face> fMap = new Dictionary<Face, Face>();
            fMap[graph.Exterior] = copy.Exterior;
            copy.Exterior.edge = eMap[graph.Exterior.edge];

            foreach (Face fOld in graph.Faces)
            {
                Face fNew = copy.AddFace(eMap[fOld.edge]);
                fMap[fOld] = fNew;
            }

            return fMap;
        }
#endregion

#region Vertex lookup
        /// <summary>
        /// Lookup a vertex in the graph by name.
        /// Note: this is slow, because I don't want to keep a dictionary
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="name"></param>
        /// <returns></returns>

        public static Vertex FindVertex(Graph graph, string name)
        {
            foreach (Vertex v in graph.Vertices)
            {
                if (v.name.Equals(name))
                {
                    return v;
                }
            }

            return null;
        }
#endregion

#region Changing graph structure

        /// <summary>
        /// Delete a vertex and all connected edges
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="vertex"></param>

        public static void DeleteVertex(Graph graph, Vertex vertex)
        {
            while (vertex.edge != null)
            {
                DeleteEdge(graph, vertex.edge);
            }

            graph.RemoveVertex(vertex);            
        }

        /// <summary>
        /// Delete an edge from the graph. Merge the faces on either side.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="edge"></param>

        public static Face DeleteEdge(Graph graph, HalfEdge edge)
        {
            DeleteHalfEdge(graph, edge);
            DeleteHalfEdge(graph, edge.Flip);
            return MergeFaces(graph, edge);
        }

        private static void DeleteHalfEdge(Graph graph, HalfEdge edge)
        {
            HalfEdge ePrev = PreviousEdge(graph, edge);
            ePrev.Next = edge.Flip.Next;

            if (edge.fromVertex.edge == edge)
            {
                // set to null if this is the last outgoing edge
                edge.fromVertex.edge = edge.Flip.Next == edge ? null : edge.Flip.Next;
            }

            if (edge.face.edge == edge)
            {
                // find a valid edge
                if (edge.Next == edge.Flip)
                {
                    if (edge.Flip.Next == edge)
                    {
                        edge.face.edge = null;    
                    }
                    else
                    {
                        edge.face.edge = edge.Flip.Next;
                    }
                }
                else
                {
                    edge.face.edge = edge.Next; 
                }
            }

            graph.RemoveEdge(edge);
        }

        private static Face MergeFaces(Graph graph, HalfEdge edge)
        {
            // Don't delete the exterior face

            Face keptFace = edge.face;
            Face deletedFace = edge.Flip.face;

            if (deletedFace == graph.Exterior)
            {
                keptFace = graph.Exterior;
                deletedFace = edge.face;
            }

            if (deletedFace != keptFace)
            {
                HalfEdge e = deletedFace.edge;                
                do
                {
                    e.face = keptFace;
                    e = e.Next;
                }
                while (e != deletedFace.edge);

                graph.RemoveFace(deletedFace);
            }

            return keptFace;
        }

        private static HalfEdge PreviousEdge(Graph graph, HalfEdge edge)
        {
            HalfEdge ePrev = edge;

            while (ePrev.Next != edge)
            {
                ePrev = ePrev.Next;
            }

            return ePrev;
        }

#endregion

#region Verify structure

        public static void Verify(Graph graph)
        {
            GraphOperations.VerifyVertices(graph);
            GraphOperations.VerifyEdges(graph);
            GraphOperations.VerifyFaces(graph);

        }

        public static void VerifyVertices(Graph graph)
        {
            foreach (Vertex v in graph.Vertices)
            {       
                VerifyVertex(graph, v);
            }
        }

        public static void VerifyVertex(Graph graph, Vertex vertex)
        {
            if (vertex.edge == null)
            {
                Debug.LogError($"{vertex}.edge == null");
            }
            else 
            {
                HalfEdge e = vertex.edge;

                do
                {
                    if (!graph.Edges.Contains(e))
                    {
                        Debug.LogError($"{e} is not in graph");
                    }
                    if (e.fromVertex == null)
                    {
                        Debug.LogError($"e.fromVertex == null != {vertex}");
                    }
                    else if (e.fromVertex != vertex)
                    {
                        Debug.LogError($"{e}.fromVertex == {e.fromVertex} != {vertex}");
                    }
                    e = e.Flip.Next;
                }   
                while (e != vertex.edge);                 
            }
        }

        public static void VerifyEdges(Graph graph)
        {
            foreach (HalfEdge e in graph.Edges)
            {
                VerifyEdge(graph, e);
            }
        }

        public static void VerifyEdge(Graph graph, HalfEdge edge)
        {
            if (edge.Flip == null)
            {
                Debug.LogError($"{edge}.Flip == null");
            }
            else if (edge.Flip.Flip != edge)
            {
                Debug.LogError($"{edge}.Flip.Flip == {edge.Flip.Flip} != {edge}");
            }

            if (edge.Next == null)
            {
                Debug.LogError($"{edge}.Next == null");                
            }
            else if (edge.Next.Prev != edge)
            {
                Debug.LogError($"{edge}.Next.Prev == {edge.Next.Prev} != {edge}");
            }

            if (edge.Next != null && edge.Flip != null)
            {
                if (edge.ToVertex != edge.Flip.fromVertex)
                {
                    Debug.LogError($"{edge}.ToVertex == {edge.ToVertex} != {edge.Flip.fromVertex}");                    
                }
            }

            if (edge.Prev == null)
            {
                Debug.LogError($"{edge}.Prev == null");
            }
            else if (edge.Prev.Next != edge)
            {
                Debug.LogError($"{edge}.Prev.Next == {edge.Prev.Next} != {edge}");                
            }

            if (edge.fromVertex == null)
            {
                Debug.LogError($"{edge}.fromVertex == null");
            }

            if (edge.face == null)
            {
                Debug.LogError($"{edge}.face == null");
            }

        }

        public static void VerifyFaces(Graph graph)
        {
            VerifyFace(graph, graph.Exterior);

            foreach (Face f in graph.Faces)
            {
                VerifyFace(graph, f);
            }

        }

        public static void VerifyFace(Graph graph, Face face)
        {
            if (face.edge == null)
            {
                Debug.LogError($"{face}.edge == null");
            }
            else
            {   
                HalfEdge e = face.edge;
                do
                {
                    if (!graph.Edges.Contains(e))
                    {
                        Debug.LogError($"{e} is not in graph");
                    }
                    if (e.face != face)
                    {
                        Debug.LogError($"{e}.face == {e.face} != {face}");
                    }
                    e = e.Next;
                } while (e != face.edge);
            }
        }

#endregion

#region Measures
        static public int EdgeCount(Vertex v)
        {
            int count = 0;
            HalfEdge e = v.edge;

            if (e != null)
            {
                do
                {
                    count++;
                    e = e.Flip.Next;
                } while (e != v.edge);
            }
            return count;
        }

        static public int EdgeCount(Face f)
        {
            int count = 0;
            HalfEdge e = f.edge;

            if (e != null)
            {
                do
                {
                    count++;
                    e = e.Next;
                } while (e != f.edge);
            }
            return count;
        }
#endregion
 
#region Neighbours
        /// <summary>
        /// Find the set of faces that share an edge with the given one
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="face"></param>
        /// <param name="set"></param>
        /// <returns>The set of neighbours</returns>

        static public HashSet<Face> EdgeNeighbours(Graph graph, Face face, HashSet<Face> set)
        {
            set.Clear();
            HalfEdge e = face.edge;
            do
            {
                set.Add(e.Flip.face);
                e = e.Next;
            } while (e != face.edge);

            return set;
        }

        /// <summary>
        /// Find the set of faces that share a vertex with the given one
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="face"></param>
        /// <param name="set"></param>
        /// <returns>The set of neighbours</returns>

        static public HashSet<Face> VertexNeighbours(Graph graph, Face face, HashSet<Face> set)
        {
            set.Clear();
            HalfEdge e = face.edge;
            do
            {
                HalfEdge e1 = e.Flip.Prev;
                e = e.Next;

                while (e1 != e.Flip)
                {                
                    set.Add(e1.face);
                    e1 = e1.Flip.Prev;
                }

            } while (e != face.edge);

            return set;
        }

        private delegate HashSet<Face> Neighbourhood(Graph graph, Face face, HashSet<Face> set);

        static private HashSet<Face> Neighbours(Neighbourhood neighbourhood, Graph graph, Face face, int depth, HashSet<Face> set)
        {
            // iterative deepening search using given neighbourhood function

            Queue<Face> horizon = new Queue<Face>();
            Queue<Face> nextHorizon = new Queue<Face>();
            horizon.Enqueue(face);
            set.Add(face);
            HashSet<Face> neighbours = new HashSet<Face>();

            for (int d = 0; d < depth; d++)
            {
                while (horizon.Count > 0)
                {
                    Face f = horizon.Dequeue();

                    neighbourhood(graph, f, neighbours);
                    foreach (Face f1 in neighbours)
                    {
                        if (!set.Contains(f1))
                        {
                            nextHorizon.Enqueue(f1);
                            set.Add(f1);
                        }
                    }                                            
                }

                (horizon, nextHorizon) = (nextHorizon, horizon);
            }

            return set;
        }

        static private HashSet<Face> EdgeNeighbours(Graph graph, Face face, int depth, HashSet<Face> set)
        {
            return Neighbours(EdgeNeighbours, graph, face, depth, set);
        }

        static private HashSet<Face> VertexNeighbours(Graph graph, Face face, int depth, HashSet<Face> set)
        {
            return Neighbours(VertexNeighbours, graph, face, depth, set);
        }

#endregion
    }

}