using UnityEngine;
using System.Collections.Generic;
using WordsOnPlay.Geometry;

public class GridTest : MonoBehaviour
{
    private Graph graph;

    [SerializeField] private int width = 1;
    [SerializeField] private int height = 1;
    [SerializeField] private float zigProb = 0;

    public void Awake()
    {
        SquareGridFactory gridFactory = new SquareGridFactory(width, height, zigProb, scale : 4);    
        graph = gridFactory.MakeGraph();
        GraphOperations.Verify(graph);

        TrisToQuadsFactory quadFactory = new TrisToQuadsFactory(graph);
        graph = quadFactory.MakeGraph();
        GraphOperations.Verify(graph);

        DivideGridFactory splitFactory = new DivideGridFactory(graph);
        graph = splitFactory.MakeGraph();
        GraphOperations.Verify(graph);

        splitFactory = new DivideGridFactory(graph);
        graph = splitFactory.MakeGraph();
        GraphOperations.Verify(graph);

        RelaxGrid relax = new RelaxGrid(graph);
        StartCoroutine(relax.RunCR());
    }

    public void OnDrawGizmos()
    {
        if (graph != null)
        {
            GeometryGizmos.DrawGizmo(graph, transform);        
        }
    }
}
