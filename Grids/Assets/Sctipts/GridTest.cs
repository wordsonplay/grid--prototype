using UnityEngine;
using System.Collections.Generic;
using WordsOnPlay.Geometry;

public class GridTest : MonoBehaviour
{
    private Graph graph;

    [SerializeField] private int radius = 1;
    [SerializeField] private float scale = 1;

    public void Awake()
    {
        HexGridFactory gridFactory = new HexGridFactory(radius, scale);    
        graph = gridFactory.MakeGraph();
        GraphOperations.Verify(graph);

        // TrisToQuadsFactory quadFactory = new TrisToQuadsFactory(graph);
        // graph = quadFactory.MakeGraph();
        // GraphOperations.Verify(graph);

        // DivideGridFactory splitFactory = new DivideGridFactory(graph);
        // graph = splitFactory.MakeGraph();
        // GraphOperations.Verify(graph);

        // splitFactory = new DivideGridFactory(graph);
        // graph = splitFactory.MakeGraph();
        // GraphOperations.Verify(graph);

        // RelaxGrid relax = new RelaxGrid(graph);
        // StartCoroutine(relax.RunCR());
    }

    public void OnDrawGizmos()
    {
        if (graph != null)
        {
            GeometryGizmos.DrawGizmo(graph, transform);        
        }
    }
}
