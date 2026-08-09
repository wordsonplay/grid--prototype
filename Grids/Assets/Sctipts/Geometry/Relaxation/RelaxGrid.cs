using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WordsOnPlay.Geometry 
{
public class RelaxGrid
{
    private Graph graph;
    private List<Face> faces;
    private bool isRunning = false;
    private System.Random rng;
    private float rate = 1f;
    private float rateDecay = 0.999f;
    private int facesPerFrame = 100;
    private Dictionary<Face,float> faceRate = new Dictionary<Face,float>();

    public RelaxGrid(Graph graph, System.Random rng = null)
    {
        this.graph = graph;
        faces = new List<Face>(graph.Faces);
        this.rng = rng ?? new System.Random();
    }

    public IEnumerator RunCR()
    {
        isRunning = true;
        while (isRunning)
        {
            for (int i = 0; i < facesPerFrame; i++)
            {
                int r = rng.Next(faces.Count);
                RelaxFace(faces[r]);                
            }
            yield return null;            
        }
    }

    private Vertex[] v = new Vertex[4];
    private Vector2[] p = new Vector2[4];
    private Vector2[] s = new Vector2[4];

    public void RelaxFace(Face face)
    {
        float t;

        t = faceRate.ContainsKey(face) ? faceRate[face] : rate;

        HalfEdge e = face.edge;
        for (int i = 0; i < 4; i++)
        {
            v[i] = e.fromVertex;
            p[i] = v[i];
            e = e.Next;
        }

        ClosestUnitSquare();

        for (int i = 0; i < 4; i++)
        {
            v[i].position = Vector2.Lerp(p[i], s[i], t);                    
        }

        t *= rateDecay;
        faceRate[face] = t;
    }

    private void ClosestUnitSquare()
    {
        Vector2 centre = (p[0] + p[1] + p[2] + p[3]) / 4f;
        Vector2 a = (-p[0] + p[1] + p[2] - p[3]) / 2f;
        Vector2 b = (-p[0] - p[1] + p[2] + p[3]) / 2f;
        Vector2 bPerp = new Vector2(-b.y, b.x);
        Vector2 c = a - bPerp;

        Vector2 u = (c.sqrMagnitude > Mathf.Epsilon ? c.normalized : Vector2.right) / 2f;
        // Vector2 u = Vector2.right / 2f;
        Vector2 v = new Vector2(-u.y, u.x);

        s[0] = centre - u - v;
        s[1] = centre + u - v;
        s[2] = centre + u + v;
        s[3] = centre - u + v;        
    }


}
}