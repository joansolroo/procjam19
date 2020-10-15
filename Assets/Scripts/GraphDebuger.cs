using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GraphDebuger : MonoBehaviour
{
    public new Camera camera;
    public Graph graph = null;
    public List<Vector3> borders = new List<Vector3>();
    public List<Vector3> seeds = new List<Vector3>();

    public bool generateGraph = false;
    public bool generateSeeds = false;
    [Range(0.5f, 10f)] public float debugSize;

    public Vector2 cellSize;
    public Vector2 offset;
    public float angle;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        for(int i=0; i<borders.Count; i++)
        {
            Gizmos.DrawLine(borders[i], borders[(i + 1) % borders.Count]);
        }

        Gizmos.color = Color.green;
        foreach (Vector3 p in seeds)
            Gizmos.DrawWireSphere(p, debugSize);
        
        if (graph != null)
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawLine(graph.boundaries.min, new Vector3(graph.boundaries.min.x, 0, graph.boundaries.max.z));
            Gizmos.DrawLine(new Vector3(graph.boundaries.min.x, 0, graph.boundaries.max.z), graph.boundaries.max);
            Gizmos.DrawLine(graph.boundaries.max, new Vector3(graph.boundaries.max.x, 0, graph.boundaries.min.z));
            Gizmos.DrawLine(new Vector3(graph.boundaries.max.x, 0, graph.boundaries.min.z), graph.boundaries.min);

            Gizmos.color = Color.red;
            foreach (KeyValuePair<Vector2, Graph.Node> entry in graph.nodesDictionary)
            {
                Vector2 p = entry.Key;
                Gizmos.DrawWireCube(new Vector3(p.x, 0, p.y), debugSize * Vector3.one);
            }

            Gizmos.color = Color.yellow;
            foreach (KeyValuePair<int, Graph.Link> entry in graph.links)
            {
                Vector2 p1 = entry.Value.start.position;
                Vector2 p2 = entry.Value.end.position;
                Gizmos.DrawLine(new Vector3(p1.x, 0, p1.y), new Vector3(p2.x, 0, p2.y));
            }

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (ray.direction.y != 0f)
            {
                Vector3 mousep = ray.origin - (ray.origin.y / ray.direction.y) * ray.direction;
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(mousep, debugSize);

                Graph.Cell c = graph.GetCellAt(new Vector2(mousep.x, mousep.z));
                if(c != null)
                {
                    Gizmos.DrawWireSphere(new Vector3(c.barycenter.x, 0, c.barycenter.y), debugSize);
                }
            }

            foreach(KeyValuePair<int, Graph.Cell> entry in graph.cells)
            {
                if(entry.Value.classified == 1)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(new Vector3(entry.Value.barycenter.x, 0, entry.Value.barycenter.y), debugSize);
                }
                else if (entry.Value.classified == 2)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawWireSphere(new Vector3(entry.Value.barycenter.x, 0, entry.Value.barycenter.y), debugSize);
                }
            }
        }
    }

    private void OnValidate()
    {
        Bounds bound = new Bounds();
        foreach (Vector3 p in borders)
            bound.Encapsulate(p);

        if(generateSeeds)
        {
            seeds.Clear();
            for (int i = 0; i < 10; i++)
                seeds.Add(new Vector3(Random.Range(bound.min.x, bound.max.x), 0, Random.Range(bound.min.z, bound.max.z)));
            generateSeeds = false;
        }

        if (generateGraph)
        {
            offset = new Vector2(Random.Range(-50f, 50f), Random.Range(-50f, 50f));
            angle = Random.Range(-45f, 45f);

            graph = new RegularGraph(cellSize, bound, offset, angle);

            List<Graph.Node> cellCorners = new List<Graph.Node>();
            for (int i = 0; i < borders.Count; i++)
            {
                Vector2 p1 = new Vector2(borders[i].x, borders[i].z);
                cellCorners.Add(new Graph.Node(p1, i));
            }

            graph.cell = new Graph.Cell(cellCorners, 0);
            graph.CellAutoLinkage();
            graph.Generate();
            graph.Simplify();
            generateGraph = false;
        }
    }
}
