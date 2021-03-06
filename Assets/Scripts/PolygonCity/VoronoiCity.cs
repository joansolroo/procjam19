﻿using HullDelaunayVoronoi.Voronoi;
using HullDelaunayVoronoi.Delaunay;
using HullDelaunayVoronoi.Primitives;
using UnityEngine;
using System.Collections.Generic;

using UnityEditor;

public class VoronoiCity : MonoBehaviour
{

    public int NumberOfVertices = 1000;

    public float size = 10;
    public Bounds bounds;
    public int seed = 0;
    [Range(1, 50)] public int subdivision = 10;

    private VoronoiMesh2 voronoi;

    List<Vector3> points;
    GraphLinked city = new GraphLinked();
    public List<GraphLinked.Cell> regions;
    public List<GraphLinked> blocks;
    public List<GraphLinked.Cell> blockContours;

    public List<VoronoiCity> neighbours;

    public bool autoGenerate = false;
    private void Start()
    {
        if (autoGenerate)
        {
            Generate();
        }
    }
    void GeneratePoints()
    {
        if (points == null)
        {
            points = new List<Vector3>();
        }
        else
        {
            points.Clear();
        }
        Random.InitState(seed);
        bounds = new Bounds(this.transform.position, new Vector3(size * 2, 100, size * 2));
        for (int i = 0; i < NumberOfVertices; i++)
        {
            float x = size * Random.Range(-0.9f, 0.9f) + this.transform.position.x;
            float z = size * Random.Range(-0.9f, 0.9f) + this.transform.position.z;
            Vector3 point = new Vector3(x, 0, z);
            bool valid = true;
            foreach (var neighbour in neighbours)
            {
                if (neighbour != null && neighbour.bounds.Contains(point))
                {
                    valid = false;
                    break;
                }
            }
            if (valid)
            {
                points.Add(point);
            }
        }

    }
    private void OnValidate()
    {
        GeneratePoints();
    }
    public void Generate()
    {
        if (points == null)
        {
            GeneratePoints();
        }
        GenerateVoronoi();
        float start = Time.realtimeSinceStartup;
        TranslateToUnity(voronoi);
        float end = Time.realtimeSinceStartup;
        Debug.Log("> Voronoi to Cells time:" + (end - start) + "ms");
        start = Time.realtimeSinceStartup;
        Subdivide(subdivision);
        end = Time.realtimeSinceStartup;
        Debug.Log("> Subdivide time:" + (end - start) + "ms");
    }
    void GenerateVoronoi()
    {
        float start = Time.realtimeSinceStartup;
        voronoi = new VoronoiMesh2();
        List<Vertex2> vertices = new List<Vertex2>();
        foreach (var neighbour in neighbours)
        {
            if (neighbour != null && neighbour.frontier != null)
            {
                foreach (var point in neighbour.points)
                {
                    if (bounds.Contains(point))
                    {
                        points.Add(point);
                    }
                }
            }
        }
        foreach (var point in points)
        {
            vertices.Add(new Vertex2(point.x, point.z));
        }

        voronoi.Generate(vertices);
        float end = Time.realtimeSinceStartup;
        Debug.Log("> Voronoi time:" + (end - start) + "ms");
    }

    List<Vector3> frontier;

    bool Sort(IList<VoronoiEdge<Vertex2>> edges)
    {
        int count = edges.Count;
        bool sorted = false;
        int cicles = 0;
        while (!sorted && cicles < 10)
        {
            sorted = true;
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
            cicles++;
            for (int i = 0; i < count - 1; ++i)
            {
                int prev = (i - 1 + count) % count;
                int next = (i + 1 + count) % count;

                for (int j = count - 1; j > i; --j)
                {

                    {
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
            }
            if (sorted)
            {
                for (int i = 0; i < count - 1; ++i)
                {
                    int prev = (i - 1 + count) % count;
                    int next = (i + 1 + count) % count;
                    sorted &= edges[prev].To == edges[i].From && edges[i].To == edges[next].From;
                }
            }
        }
        return sorted;
    }
    void TranslateToUnity(VoronoiMesh2 voronoi)
    {
        regions = new List<GraphLinked.Cell>();
        //deflatedCells = new List<Cell>();
        frontier = new List<Vector3>();
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
            if (!valid)
            {
                foreach (DelaunayCell<Vertex2> ce in region.Cells)
                {
                    //invalid.Add(new Vector3(ce.CircumCenter.X, 0, ce.CircumCenter.Y));
                    foreach (var si in ce.Simplex.Vertices)
                    {
                        frontier.Add(new Vector3(si.X, 0, si.Y));
                    }

                }
                continue;
            }
            Vector3 centroid = Vector3.zero;
            IList<VoronoiEdge<Vertex2>> edges = region.Edges;

            valid = Sort(edges);
            if (!valid)
            {
                continue;
            }
           // List<Vector3> contour = new List<Vector3>();
            List<GraphLinked.Node> contour = new List<GraphLinked.Node>();
            foreach (VoronoiEdge<Vertex2> e in edges)
            {
                Vertex2 v0 = e.From.CircumCenter;
                Vector3 p0 = new Vector3(v0.X, 0, v0.Y);
                Vertex2 v1 = e.To.CircumCenter;
                Vector3 p1 = new Vector3(v1.X, 0, v1.Y);
                centroid += p0;

                var nodeFrom = city.AddNode(p0, false);
                var nodeNext = city.AddNode(p1, false);
                contour.Add(nodeFrom);
                var link = city.AddLink(nodeFrom,nodeNext);
            }
            GraphLinked.Cell cell = new GraphLinked.Cell(contour);
            regions.Add(cell);
        }
    }
    void Subdivide(int subdivision = 5)
    {
        blockContours = new List<GraphLinked.Cell>();
        if (subdivision == 1)
        {
            foreach (GraphLinked.Cell cell in regions)
            {
                blockContours.Add(cell);
            }
        }
        else
        {
            blocks = new List<GraphLinked>();
            foreach (GraphLinked.Cell region in regions)
            {

                GraphLinked regionGraph = region.AsGraph();
                blocks.Add(regionGraph);
                Bounds bounds = region.Bounds;
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
                        if (region.ContainsPoint(point))
                        {
                            valid[x, z] = true;
                            nodes[x, z] = regionGraph.AddNode(point);

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
                            // RIGHT
                            int x2 = x + 1;
                            if (x2 > subdivision || !valid[x2, z])
                            {
                                float u2 = x2 / (float)subdivision * bounds.size.x + bounds.min.x;
                                Vector3 targetPosition = new Vector3(u2, 0, v);
                                regionGraph.SegmentShell(from, targetPosition);
                                
                            }
                            else
                            {
                                regionGraph.AddLink(nodes[x, z], nodes[x2, z]);
                                city.AddLink(city.AddNode(nodes[x, z].position), city.AddNode(nodes[x2, z].position));
                            }
                            // LEFT
                            x2 = x - 1;
                            if (x2 < 0 || !valid[x2, z])
                            {
                                float u2 = x2 / (float)subdivision * bounds.size.x + bounds.min.x;
                                Vector3 targetPosition = new Vector3(u2, 0, v);
                                regionGraph.SegmentShell(from, targetPosition);
                            }
                            // no else: intersections with the edge are done only done right-wards

                            // UP
                            int z2 = z + 1;
                            if (z2 > subdivision || !valid[x, z2])
                            {
                                float v2 = z2 / (float)subdivision * bounds.size.z + bounds.min.z;
                                Vector3 targetPosition = new Vector3(u, 0, v2);
                                regionGraph.SegmentShell(from, targetPosition);
                            }
                            else
                            {
                                regionGraph.AddLink(nodes[x, z], nodes[x, z2]);
                                city.AddLink(city.AddNode(nodes[x, z].position), city.AddNode(nodes[x, z2].position));
                            }

                            //DOWN
                            z2 = z - 1;
                            if (z2 < 0 || !valid[x, z2])
                            {
                                float v2 = z2 / (float)subdivision * bounds.size.z + bounds.min.z;
                                Vector3 targetPosition = new Vector3(u, 0, v2);
                                regionGraph.SegmentShell(from, targetPosition);
                            }
                            // no else: intersections with the edge are done only done up-wards
                        }
                    }
                }
                for (int x = 0; x <= subdivision - 1; ++x)
                {
                    for (int z = 0; z <= subdivision - 1; ++z)
                    {
                        if (valid[x, z] && valid[x + 1, z] && valid[x, z + 1] && valid[x + 1, z + 1])
                        {
                            List<GraphLinked.Node> contour = new List<GraphLinked.Node>();
                            contour.Add(nodes[x, z]);
                            contour.Add(nodes[x + 1, z]);
                            contour.Add(nodes[x + 1, z + 1]);
                            contour.Add(nodes[x, z + 1]);
                            GraphLinked.Cell subcell = new GraphLinked.Cell(contour);
                            subcell.Parent = region;
                            blockContours.Add(subcell);
                        }
                    }
                }

                List<GraphLinked.Link> unused = new List<GraphLinked.Link>();
                foreach (var link in regionGraph.segments)
                {
                    unused.AddRange(link);
                }
                // get the cells at the edges
                regionGraph.ComputeNeighbours();
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

                    var neighbours = regionGraph.ComputeNeighbours();
                    while (queueNodes.Count > 0 && count < 100 && solutionPath == null)
                    {
                        List<GraphLinked.Node> currentSolution = queueNodes.Dequeue();
                        List<GraphLinked.Link> currentSolutionLinks = queueLinks.Dequeue();
                        GraphLinked.Node current = currentSolution[currentSolution.Count - 1];

                        used.Add(current);
                        foreach (var nextLink in neighbours[current])
                        {
                            //flips the edge if needed
                            GraphLinked.Node next = current == nextLink.from ? nextLink.to : nextLink.from;

                            // is it a solution?
                            if (currentSolution.Count > 2 && next == link.from)
                            {
                                solutionPath = currentSolution;
                                solutionLinks = currentSolutionLinks;
                            }
                            //no solution, but it can be navigated
                            else if (!used.Contains(next))
                            {
                                // single option, no need to create new solution
                                if (neighbours[current].Count == 1)
                                {
                                    currentSolution.Add(next);
                                    queueNodes.Enqueue(currentSolution);

                                    currentSolutionLinks.Add(nextLink);
                                    queueLinks.Enqueue(currentSolutionLinks);
                                }
                                // more than one edge, create a new solution for each
                                else if (neighbours[current].Count > 1)
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

                            List<GraphLinked.Node> contourCell = new List<GraphLinked.Node>();
                            foreach (var entry in solutionPath)
                            {
                                if (contourCell.Count == 0 || Vector3.SqrMagnitude(entry.position - contourCell[contourCell.Count - 1].position) > float.Epsilon)
                                {
                                    contourCell.Add(entry);
                                }
                            }
                            if (contourCell.Count > 2)
                            {
                                GraphLinked.Cell building = new GraphLinked.Cell(contourCell);
                                if (building.Area > 0)
                                {
                                    foreach (var usedLink in solutionLinks)
                                    {
                                        if (unused.Contains(usedLink))
                                        {
                                            unused.Remove(usedLink);
                                        }
                                    }
                                    building.Parent = region;
                                    blockContours.Add(building);
                                }
                            }
                            else
                            {
                                string p = "";
                                foreach (var element in contourCell)
                                {
                                    p += element.ToString() + " ";
                                }
                                Debug.LogWarning("path:" + p);
                            }
                        }
                        else
                        {
                            Debug.Log("STATUS:" + (solutionPath != null ? solutionPath.Count.ToString() : "null") + ", in " + count + " steps");
                        }
                    }
                }
            }
        }
        /*foreach (Cell building in buildingContours)
        {
            building.Deflate(0.1f);
        }*/

    }

    void GizmosCell(GraphLinked.Cell cell, Vector3 position, bool center = false)
    {
        Vector3 centroid = cell.Center + position;
        var contour = cell.localContour;
        for (int c = 0; c < contour.Count; ++c)
        {
            Vector3 p0 = contour[c] + centroid;
            Vector3 p1 = contour[(c + 1) % contour.Count] + centroid;
            Gizmos.DrawLine(p0, p1);
           
        }
        if (center)
        {
            Gizmos.DrawCube(centroid, Vector3.one*0.005f*size);
            for (int c = 0; c < contour.Count; ++c)
            {
                Vector3 p0 = contour[c] + centroid;
                Gizmos.DrawLine(p0, centroid);
            }
        }
    }
    private void OnDrawGizmos()
    {

        Gizmos.DrawWireCube(bounds.center, bounds.size);
        if (points != null)
        {
            foreach (var vertex in points)
            {
                Gizmos.DrawSphere(vertex, size / 50);
            }
        }
        if (regions == null)
        {
            return;
        }
        Gizmos.color = Color.red;
        foreach (var vertex in frontier)
        {
            Gizmos.DrawWireSphere(vertex, 150);
        }
       
        Gizmos.color = Color.white;
        foreach (var link in city.GetLinks())
        {
            GizmoExtensions.DrawArrow(link.from.position + Vector3.up * 30, link.to.position + Vector3.up * 30);
        }
        /*foreach (var block in blocks) {
            foreach (var link in block.links)
            {
                Gizmos.DrawLine(link.from.position + Vector3.up * 30, link.to.position + Vector3.up * 30);
            }
        }*/
        /*
        int c = 0;
        foreach (GraphLinked.Cell cell in regions)
        {
            Gizmos.color = new Color32((byte)((c*64)%255), (byte)((c/64)%255),255,255);
            GizmosCell(cell, Vector3.zero);
            //foreach (Cell block in cell.children)
            {
                //GizmosCell(block, Vector3.up * 10);
                foreach (GraphLinked.Cell building in cell.children)
                {
                    for (int h = 1; h <= 2; ++h)
                    {
                        GizmosCell(building, Vector3.up * 20*h,true);
                    }
                }
            }
            ++c;
        }*/
    }
}








