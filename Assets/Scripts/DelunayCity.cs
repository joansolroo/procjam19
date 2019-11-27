using HullDelaunayVoronoi.Voronoi;
using HullDelaunayVoronoi.Delaunay;
using HullDelaunayVoronoi.Primitives;
using UnityEngine;
using System.Collections.Generic;

using UnityEditor;

public class DelunayCity : MonoBehaviour
{

    public int NumberOfVertices = 1000;

    public float size = 10;
    public Bounds bounds;
    public int seed = 0;
    [Range(1, 50)] public int subdivision = 10;

    private VoronoiMesh2 voronoi;

    private Material lineMaterial;

    public ProceduralBuilding buildingTemplate;

    public proceduralRegion regionTemplate;

    List<Vertex2> vertices;
    List<Cell> cells;
    List<Cell> deflatedCells;
    List<GraphLinked> blocks;
    List<Cell> buildingContours;

    void GeneratePoints()
    {
        if (vertices == null)
        {
            vertices = new List<Vertex2>();
        }
        else
        {
            vertices.Clear();
        }
        Random.InitState(seed);
        for (int i = 0; i < NumberOfVertices; i++)
        {
            float x = size * Random.Range(-1.0f, 1.0f) + this.transform.position.x;
            float y = size * Random.Range(-1.0f, 1.0f) + this.transform.position.z;

            vertices.Add(new Vertex2(x, y));
        }
        bounds = new Bounds(this.transform.position, new Vector3(size * 2, 100, size * 2));
    }
    private void OnValidate()
    {
        GeneratePoints();
    }
    private void Start()
    {
        lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

        if (vertices == null)
        {
            GeneratePoints();
        }
        float start = Time.realtimeSinceStartup;
        voronoi = new VoronoiMesh2();
        voronoi.Generate(vertices);
        float end = Time.realtimeSinceStartup;
        Debug.Log("> Voronoi time:" + (end - start) + "ms");
        start = Time.realtimeSinceStartup;
        TranslateToUnity(voronoi);
        end = Time.realtimeSinceStartup;
        Debug.Log("> Voronoi to Cells time:" + (end - start) + "ms");
        start = Time.realtimeSinceStartup;
        Subdivide(subdivision);
        end = Time.realtimeSinceStartup;
        Debug.Log("> Subdivide time:" + (end - start) + "ms");
        start = Time.realtimeSinceStartup;
        GenerateRegions(cells, 1);
        GenerateRegions(deflatedCells, 2);
        GenerateRegions(buildingContours, 1200, true);
        // GenerateBuildings(buildingContours);
        end = Time.realtimeSinceStartup;
        Debug.Log("> geometry time:" + (end - start) + "ms");
    }

    void GenerateRegions(List<Cell> cells, int height, bool perlin = false)
    {
        foreach (Cell cell in cells)
        {
            proceduralRegion region = Instantiate(regionTemplate);
            float h = height;
            if (perlin)
            {
                h *= Mathf.Pow(Mathf.PerlinNoise(cell.Center.x * 1, cell.Center.z * 1) * 0.9f + Mathf.PerlinNoise(cell.Center.x * 10, cell.Center.z * 10) * 0.1f, 2);
            }
            region.transform.parent = this.transform;
            region.transform.position = cell.Center;
            region.Generate(cell, h, perlin ? Random.Range(0, 0.45f) : 0);
            //region.renderer.material.color = Random.ColorHSV(0, 1, 1, 1, 0.5f, 0.5f);
        }
    }
    void GenerateBuildings(List<Cell> cells)
    {
        foreach (Cell cell in cells)
        {
            float area = cell.Area;
            //if (area < 4000  && area >500)
            {
                ProceduralBuilding building = Instantiate(buildingTemplate);
                building.transform.parent = this.transform;
                building.transform.position = cell.Center;
                int height = (int)(area / 1000 * 5);

                building.Generate(cell.localContour.ToArray(), height, new Vector2(Random.Range(1, 2), Random.Range(1, 2)));
            }
        }
    }

    void TranslateToUnity(VoronoiMesh2 voronoi)
    {
        cells = new List<Cell>();
        deflatedCells = new List<Cell>();
        foreach (VoronoiRegion<Vertex2> region in voronoi.Regions)
        {
            float margin = Random.Range(0.1f, 0.5f);
            bool valid = true;
            foreach (DelaunayCell<Vertex2> ce in region.Cells)
            {
                if (!bounds.Contains(new Vector3(ce.CircumCenter.X, 0, ce.CircumCenter.Y)))
                {
                    valid = false;
                    break;
                }
            }
            if (!valid) continue;

            Vector3 centroid = Vector3.zero;
            Gizmos.color = Color.white;
            var edges = region.Edges;
            int count = edges.Count;

            for (int repetitions = 0; repetitions < 5; ++repetitions)
            {
                for (int i = 0; i < count - 1; ++i)
                {
                    for (int j = count - 1; j > i; --j)
                    {
                        // flip if needed
                        if (edges[i].From == edges[j].From || edges[i].To == edges[j].To)
                        {
                            if (edges[i].From == edges[j].From && edges[i].To == edges[j].To)
                            {
                                edges.RemoveAt(j);
                                count = edges.Count;
                            }
                            else
                            {
                                edges[j] = new VoronoiEdge<Vertex2>(edges[j].To, edges[j].From);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < count - 1; ++i)
            {
                int prev = (i - 1 + count) % count;
                int next = (i + 1 + count) % count;
                for (int j = count - 1; j > i; --j)
                {
                    // flip if needed
                    if (edges[i].From == edges[j].From || edges[i].To == edges[j].To)
                    {
                        if (edges[i].From == edges[j].From && edges[i].To == edges[j].To)
                        {
                            Debug.Log("repeated");
                            edges.RemoveAt(j);
                            count = edges.Count;
                        }
                        else
                        {
                            Debug.Log("flipped");
                            edges[j] = new VoronoiEdge<Vertex2>(edges[j].To, edges[j].From);
                        }

                    }

                    if (edges[i].To.CircumCenter == edges[j].From.CircumCenter)
                    {
                        if (next != j)
                        {
                            var tmp = edges[next];
                            edges[next] = edges[j];
                            edges[j] = tmp;
                        }
                        //break;
                    }
                    if (edges[i].From.CircumCenter == edges[j].To.CircumCenter)
                    {
                        if (prev != j)
                        {
                            var tmp = edges[prev];
                            edges[prev] = edges[j];
                            edges[j] = tmp;
                        }
                        //break;
                    }
                }
            }
            Vector3 min = new Vector3(float.MaxValue, 0, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, 0, float.MinValue);
            List<Vector3> contour = new List<Vector3>();
            int c = 0;
            foreach (VoronoiEdge<Vertex2> e in edges)
            {
                Vertex2 v0 = e.From.CircumCenter;
                Vector3 p0 = new Vector3(v0.X, 0, v0.Y);
                Vertex2 v1 = e.To.CircumCenter;
                Vector3 p1 = new Vector3(v1.X, 0, v1.Y);
                centroid += p0;

                contour.Add(p0);
                /* if (c == region.Edges.Count)
                 {
                     cell.contour.Add(p1);
                 }*/
                min.x = Mathf.Min(p0.x, min.x);
                min.z = Mathf.Min(p0.z, min.z);
                max.x = Mathf.Max(p0.x, max.x);
                max.z = Mathf.Max(p0.z, max.z);

                ++c;
            }
            Cell cell = new Cell(contour);
            cells.Add(cell);
            deflatedCells.Add(cell.Deflated(0.1f));
        }
    }
    List<Vector3> cellDivision;
    List<KeyValuePair<Vector3, Vector3>> divisions = new List<KeyValuePair<Vector3, Vector3>>();
    void Subdivide(int subdivision = 5)
    {


        buildingContours = new List<Cell>();
        if (subdivision == 1)
        {
            foreach (Cell cell in deflatedCells)
            {
                buildingContours.Add(cell);
            }
        }
        else
        {
            cellDivision = new List<Vector3>();
            blocks = new List<GraphLinked>();
            foreach (Cell cell in deflatedCells)
            {

                GraphLinked graph = cell.AsGraph();
                blocks.Add(graph);
                Bounds bounds = cell.Bounds;
                Vector3 delta = bounds.size / subdivision;
                bool[,] valid = new bool[subdivision + 1, subdivision + 1];
                GraphLinked.Node[,] nodes = new GraphLinked.Node[subdivision + 1, subdivision + 1];
                for (int x = 0; x <= subdivision; ++x)
                {
                    float u = x / (float)subdivision * bounds.size.x + bounds.min.x;
                    for (int z = 0; z <= subdivision; ++z)
                    {
                        float v = z / (float)subdivision * bounds.size.z + bounds.min.z;
                        Vector3 point = new Vector3(u, 0, v);
                        if (cell.ContainsPoint(point))
                        {
                            valid[x, z] = true;
                            var node = new GraphLinked.Node(point);
                            graph.nodes.Add(node);
                            nodes[x, z] = node;
                        }
                    }
                }

                // create internal nodes and links, divide shell edges when intersecting them 
                for (int x = 0; x <= subdivision; ++x)
                {
                    float u = x / (float)subdivision * bounds.size.x + bounds.min.x;
                    for (int z = 0; z <= subdivision; ++z)
                    {
                        float v = z / (float)subdivision * bounds.size.z + bounds.min.z;
                        if (valid[x, z])
                        {
                            Vector3 fromPosition = new Vector3(u, 0, v);
                            GraphLinked.Node from = nodes[x, z];
                            int x2 = x + 1;
                            if (x2 <= subdivision && !valid[x2, z])
                            {
                                float u2 = x2 / (float)subdivision * bounds.size.x + bounds.min.x;
                                Vector3 targetPosition = new Vector3(u2, 0, v);
                                graph.SegmentShell(from, targetPosition);
                            }
                            else
                            {
                                GraphLinked.Link link = new GraphLinked.Link(nodes[x, z], nodes[x2, z]);
                                graph.links.Add(link);
                            }
                            x2 = x - 1;
                            if (x2 >= 0 && !valid[x2, z])
                            {
                                float u2 = x2 / (float)subdivision * bounds.size.x + bounds.min.x;
                                Vector3 targetPosition = new Vector3(u2, 0, v);
                                graph.SegmentShell(from, targetPosition);
                            }
                            /*else
                            {
                                Graph.Link link = new Graph.Link(nodes[x, z], nodes[x2, z]);
                                graph.links.Add(link);
                            }*/
                            int z2 = z + 1;
                            if (z2 <= subdivision && !valid[x, z2])
                            {
                                float v2 = z2 / (float)subdivision * bounds.size.z + bounds.min.z;
                                Vector3 targetPosition = new Vector3(u, 0, v2);
                                graph.SegmentShell(from, targetPosition);
                            }
                            else
                            {
                                GraphLinked.Link link = new GraphLinked.Link(nodes[x, z], nodes[x, z2]);
                                graph.links.Add(link);
                            }

                            z2 = z - 1;
                            if (z2 >= 0 && !valid[x, z2])
                            {
                                float v2 = z2 / (float)subdivision * bounds.size.z + bounds.min.z;
                                Vector3 targetPosition = new Vector3(u, 0, v2);
                                graph.SegmentShell(from, targetPosition);
                            }
                            /*else
                            {
                                Graph.Link link = new Graph.Link(nodes[x, z], nodes[x, z2]);
                                graph.links.Add(link);
                            }*/
                        }
                    }
                }
                for (int x = 0; x <= subdivision - 1; ++x)
                {
                    for (int z = 0; z <= subdivision - 1; ++z)
                    {
                        if (valid[x, z] && valid[x + 1, z] && valid[x, z + 1] && valid[x + 1, z + 1])
                        {
                            List<Vector3> contour = new List<Vector3>();
                            contour.Add(nodes[x, z].position);
                            contour.Add(nodes[x + 1, z].position);
                            contour.Add(nodes[x + 1, z + 1].position);
                            contour.Add(nodes[x, z + 1].position);
                            buildingContours.Add(new Cell(contour));
                        }
                    }
                }

                List<GraphLinked.Link> unused = new List<GraphLinked.Link>();
                foreach (var link in graph.segments)
                {
                    unused.AddRange(link);
                }
                graph.ComputeNeighbours();
                while (unused.Count > 0)
                {
                    List<GraphLinked.Link> contour = new List<GraphLinked.Link>();
                    GraphLinked.Link link = unused[0];

                    unused.RemoveAt(0);
                    if (link == null)
                    {
                        continue;
                    }
                    contour.Add(link);

                    Queue<List<GraphLinked.Node>> queueNodes = new Queue<List<GraphLinked.Node>>();
                    Queue<List<GraphLinked.Link>> queueLinks = new Queue<List<GraphLinked.Link>>();
                    HashSet<GraphLinked.Node> used = new HashSet<GraphLinked.Node>();
                    var startNodes = new List<GraphLinked.Node>();
                    used.Add(link.from);
                    startNodes.Add(link.from);
                    startNodes.Add(link.to);
                    queueNodes.Enqueue(startNodes);
                    List<GraphLinked.Link> startLink = new List<GraphLinked.Link>();
                    startLink.Add(link);
                    queueLinks.Enqueue(startLink);
                    int count = 0;
                    List<GraphLinked.Node> solutionPath = null;
                    List<GraphLinked.Link> solutionLinks = null;


                    while (queueNodes.Count > 0 && count < 100 && solutionPath == null)
                    {
                        List<GraphLinked.Node> currentSolution = queueNodes.Dequeue();
                        List<GraphLinked.Link> currentSolutionLinks = queueLinks.Dequeue();
                        GraphLinked.Node current = currentSolution[currentSolution.Count - 1];

                        used.Add(current);
                        foreach (var nextLink in graph.neighbours[current])
                        {
                            GraphLinked.Node next = current == nextLink.from ? nextLink.to : nextLink.from;
                            if (currentSolution.Count > 2 && next == link.from)
                            {
                                solutionPath = currentSolution;
                                solutionLinks = currentSolutionLinks;
                            }
                            else if (!used.Contains(next))
                            {
                                if (graph.neighbours[current].Count == 1)
                                {
                                    currentSolution.Add(next);
                                    queueNodes.Enqueue(currentSolution);

                                    currentSolutionLinks.Add(nextLink);
                                    queueLinks.Enqueue(currentSolutionLinks);
                                }
                                else if (graph.neighbours[current].Count > 1)
                                {
                                    var newSolution = new List<GraphLinked.Node>(currentSolution);
                                    newSolution.Add(next);
                                    queueNodes.Enqueue(newSolution);

                                    var newSolutionLinks = new List<GraphLinked.Link>(currentSolutionLinks);
                                    newSolutionLinks.Add(nextLink);
                                    queueLinks.Enqueue(newSolutionLinks);

                                }
                            }
                            ++count;
                        }
                    }
                    {
                        if (solutionPath != null)
                        {
                            foreach (var usedLink in solutionLinks)
                            {
                                if (unused.Contains(usedLink))
                                {
                                    unused.Remove(usedLink);
                                }
                            }
                            List<Vector3> contourCell = new List<Vector3>();
                            foreach (var entry in solutionPath)
                            {
                                contourCell.Add(entry.position);
                            }
                            Cell building = new Cell(contourCell);
                            buildingContours.Add(building);
                        }
                        else
                        {
                            Debug.Log("STATUS:" + (solutionPath != null ? solutionPath.Count.ToString() : "null") + ", in " + count + " steps");
                        }
                    }
                }
            }
        }
            foreach (Cell building in buildingContours)
            {
                building.Deflate(0.1f);
            }

        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            if (vertices != null)
            {
                foreach (var vertex in vertices)
                {
                    Gizmos.DrawSphere(new Vector3(vertex.X, 0, vertex.Y), 100);
                }
            }
            if (cells == null)
            {
                return;
            }

            foreach (Cell cell in cells)
            {
                // Gizmos.DrawWireCube(cell.Bounds.center, cell.Bounds.size);
                Gizmos.color = Color.white;
                Vector3 centroid = cell.Center;
                for (int c = 0; c < cell.Contour.Count; ++c)
                {
                    Vector3 p0 = cell.Contour[c];
                    Vector3 p1 = cell.Contour[(c + 1) % cell.Contour.Count];
                    Gizmos.DrawLine(p0, p1);
                }
            }
            return;
            if (blocks != null)
            {
                foreach (GraphLinked graph in blocks)
                {
                    foreach (var node in graph.nodes)
                    {
                        Gizmos.color = Color.white;
                        //Gizmos.color = node.shell ? Color.green : Color.gray;

                        Gizmos.color = Color.black;
                        Handles.color = Color.black;
                        Vector3 labelPosition = node.position + Vector3.up;
                        Handles.Label(labelPosition + Vector3.up, node.Id.ToString());
                        Gizmos.DrawLine(node.position, labelPosition);
                    }
                }
                /*foreach (GraphLinked graph in blocks)
                {
                    Gizmos.color = Color.white;
                    foreach (var node in graph.nodes)
                    {
                        Gizmos.color = Color.white;
                        //Gizmos.color = node.shell ? Color.green : Color.gray;
                        Gizmos.DrawSphere(node.position, 0.1f);
                    }
                    foreach (var link in graph.links)
                    {
                        Gizmos.color = Color.white;
                        //Gizmos.color = link.shell ? Color.green : Color.gray;
                        if (Vector3.Distance(link.from.position, link.to.position) < 0.01f)
                        {
                            Debug.LogWarning("SAME:" + link.from.Id + "," + link.to.Id);
                        }
                        GizmoExtensions.DrawArrow(link.from.position, link.to.position);

                    }
                }*/
            }



            /*  if (buildingContours!=null)
              {
                  Gizmos.color = Color.white;
                  foreach (Cell cell in buildingContours)
                  {
                      // Gizmos.DrawWireCube(cell.Bounds.center, cell.Bounds.size);
                      Gizmos.color = Color.yellow;
                      Vector3 centroid = cell.Center;
                      for (int c = 0; c < cell.Contour.Count; ++c)
                      {
                          Gizmos.DrawSphere(cell.Center, 0.1f);
                          Vector3 p0 = cell.Contour[c];
                          Vector3 p1 = cell.Contour[(c + 1) % cell.Contour.Count];
                          Gizmos.DrawLine(p0, p1);
                      }
                  }
              }
              */

            /*
            for (int idx = 0; idx < deflatedCells.Count; ++idx)
            {

                Cell cell = deflatedCells[idx];
                Gizmos.color = new Color((count / 5f) % 1, count % 5 / 5 % 5, 0);
                Gizmos.DrawSphere(cell.Center, 0.5f);
                //Gizmos.DrawWireCube(cell.Bounds.center, cell.Bounds.size);
                for (int c = 0; c < cell.Contour.Length; ++c)
                {

                    Gizmos.DrawSphere(cell.Center, 0.2f);
                    Vector3 p0 = cell.Contour[c];
                    Vector3 p1 = cell.Contour[(c + 1) % cell.Contour.Length];
                    Gizmos.DrawLine(p0, p1);

                }

                ++count;
            }

            foreach (Vector3 p in cellDivision)
            {
                Gizmos.DrawSphere(p, 0.25f);
            }
            int ce = 0;
            foreach (var pair in divisions)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(pair.Key, pair.Value);
            }*/
        }
    }








