/**
 *
 * Author: Malcolm Ryan
 * Version: 1.0
 * For Unity Version: 2022.3
 */

using UnityEngine;
using WordsOnPlay.Utils;

namespace WordsOnPlay.Geometry
{
public class GeometryGizmos
{
    public const float defaultVertexRadius = 0.1f;

    public const float edgeArrowPercent = 0.6f;
    public const float edgeArrowAngle = 45; // degrees
    public const float edgeArrowLength = 0.05f;
    
    private static Mesh triangleMesh = CreateTriangleMesh();

    private static Mesh CreateTriangleMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[3];
        mesh.triangles = new int[]
        {
            0, 1, 2
        };
        mesh.normals = new Vector3[]
        {
            Vector3.forward,
            Vector3.forward,
            Vector3.forward
        };

        return mesh;
    }

    public static void DrawGizmo(
        Vertex v, float radius = defaultVertexRadius, bool solid = false, Transform transform = null) 
    {
        Vector3 p = v.position;
        
        if (transform != null)
        {
            p = transform.TransformPoint(p);
        }

        if (solid)
        {
            Gizmos.DrawSphere(p, radius);            
        }
        else 
        {
            Gizmos.DrawWireSphere(p, radius);
        }
    }

    public static void DrawGizmo(HalfEdge e, Transform transform = null) 
    {
        Vector2 a = e.fromVertex.position;
        Vector2 b = e.next.fromVertex.position;

        // arrow
        Vector2 p = Vector3.Lerp(a,b,edgeArrowPercent);
        Vector2 v = (b-a).normalized * edgeArrowLength;
        v = v.Rotate(180 - edgeArrowAngle);
        Vector2 q = p + v;

        Vector3 a3 = a;
        Vector3 b3 = b;
        Vector3 p3 = p;
        Vector3 q3 = q;

        if (transform != null)
        {
            a3 = transform.TransformPoint(a3);
            b3 = transform.TransformPoint(b3);
            p3 = transform.TransformPoint(p3);
            q3 = transform.TransformPoint(q3);
        }

        Gizmos.DrawLine(a3, b3);
        Gizmos.DrawLine(p3, q3);
    }

    public static void DrawGizmo(Face f, bool solid = false, Transform transform = null) 
    {
        HalfEdge e = f.edge;

        if (solid)
        {
            // Note: this assumes that the face is convex
            do 
            {
                DrawTriangleGizmo(e, true, transform);
                e = e.next;
            } while (e != f.edge);
 
        }
        else 
        {
            do 
            {
                DrawGizmo(e);
                e = e.next;
            } while (e != f.edge);
        }
    }


    public static void DrawTriangleGizmo(HalfEdge e, bool solid = false, Transform transform = null)
    {
        Vector3 a = e.fromVertex.position;
        Vector3 b = e.next.fromVertex.position;
        Vector3 c = e.next.next.fromVertex.position;

        if (transform != null) 
        {
            a = transform.TransformPoint(a);
            b = transform.TransformPoint(b);
            c = transform.TransformPoint(c);
        }

        if (solid)
        {
            triangleMesh.vertices[0] = a;
            triangleMesh.vertices[1] = b;
            triangleMesh.vertices[2] = c;

            Graphics.DrawMeshNow(triangleMesh, Vector3.zero, Quaternion.identity);
        }
        else 
        {
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, a);
        }
    }
}
}

