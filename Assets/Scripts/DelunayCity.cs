using HullDelaunayVoronoi.Voronoi;
using HullDelaunayVoronoi.Delaunay;
using HullDelaunayVoronoi.Primitives;
using UnityEngine;
using System.Collections.Generic;

public class DelunayCity : MonoBehaviour
{

    public int NumberOfVertices = 1000;

    public float size = 10;

    public int seed = 0;

    private VoronoiMesh2 voronoi;

    private Material lineMaterial;

    public ProceduralBuilding buildingTemplate;

    public class Graph
    {
        public class Node
        {
            public int index;
            public Vector3 position;
        }
        public class Link
        {
            public Node from;
            public Node to;
        }
        public Cell Contour;
        public List<Node> nodes;
        public List<Link> links;
    }
    public class Cell
    {
        private Bounds bounds;
        private Vector3 center;
        private List<Vector3> contour;

        public Cell(List<Vector3> contour)
        {
            Contour = contour;
        }
        public Graph AsGraph()
        {
            Graph graph = new Graph();
            graph.Contour = this;
            graph.nodes = new List<Graph.Node>();
            graph.links = new List<Graph.Link>();
            for (int i = 0; i < contour.Count; ++i)
            {
                Graph.Node node = new Graph.Node();
                node.index = i;
                node.position = contour[i];
                graph.nodes.Add(node);
            }
            for (int i = 0; i < contour.Count; ++i)
            {
                Graph.Link link = new Graph.Link();
                link.from = graph.nodes[i];
                link.to = graph.nodes[(i + 1) % contour.Count];
                graph.links.Add(link);
            }
            return graph;
        }
        public Cell Deflate(float margin)
        {
            List<Vector3> deflatedContour = new List<Vector3>();
            Vector3 newCenter = Vector3.zero;
            for (int c = 0; c < Contour.Count; ++c)
            {
                Vector3 p = (Contour[c] - Center) * (1 - margin) + Center;
                deflatedContour.Add(p);
                newCenter += p;
            }
            return new Cell(deflatedContour);
        }
        void RecalculateBounds()
        {
            Vector3 min = new Vector3(float.MaxValue, 0, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, 0, float.MinValue);
            Vector3 center = Vector3.zero;
            foreach (Vector3 p in Contour)
            {
                min.x = Mathf.Min(p.x, min.x);
                min.z = Mathf.Min(p.z, min.z);
                max.x = Mathf.Max(p.x, max.x);
                max.z = Mathf.Max(p.z, max.z);
                center += p;
            }
            Bounds = new Bounds((min + max) / 2, max - min);
            this.center = center / contour.Count;
        }
        public List<Vector3> localContour
        {
            get
            {
                List<Vector3> result = new List<Vector3>();
                foreach (Vector3 point in Contour)
                {
                    result.Add(point - Center);
                }
                return result;
            }
        }

        public Bounds Bounds { get => bounds; private set => bounds = value; }
        public List<Vector3> Contour
        {
            get => contour;
            set
            {
                contour = value;
                RecalculateBounds();
            }
        }
        public Vector3 Center { get => center; private set => center = value; }

        public bool ContainsPoint(Vector3 point)
        {
            return ContainsPoint(Contour, point);
        }
        public static bool ContainsPoint(List<Vector3> polyPoints, Vector3 p)
        {
            var j = polyPoints.Count - 1;
            var inside = false;
            for (int i = 0; i < polyPoints.Count; j = i++)
            {
                var pi = polyPoints[i];
                var pj = polyPoints[j];
                if (((pi.z <= p.z && p.z <= pj.z) || (pj.z <= p.z && p.z <= pi.z)) &&
                    (p.x <= (pj.x - pi.x) * (p.z - pi.z) / (pj.z - pi.z) + pi.x))
                    inside = !inside;
            }
            return inside;
        }
        /*public bool Intersect(out Vector3 intersection, Vector3 origin, Vector3 direction, out int segmentIndex)
        {

            for (int i = 0; i < Contour.Count; ++i)
            {
                Vector3 from = Contour[i];
                Vector3 to = Contour[(i + 1) % Contour.Count];
                if (Math3d.LineLineIntersection(out intersection, origin, direction - origin, from, to - from))
                {
                    segmentIndex = i;
                    return true;
                }
            }
            segmentIndex = -1;
            intersection = Vector3.zero;
            return false;
        }*/
    }
    List<Cell> cells;
    List<Cell> deflatedCells;
    List<Graph> blocks;
    private void Start()
    {

        lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

        List<Vertex2> vertices = new List<Vertex2>();

        Random.InitState(seed);
        for (int i = 0; i < NumberOfVertices; i++)
        {
            float x = size * Random.Range(-1.0f, 1.0f);
            float y = size * Random.Range(-1.0f, 1.0f);

            vertices.Add(new Vertex2(x, y));
        }

        voronoi = new VoronoiMesh2();
        voronoi.Generate(vertices);
        TranslateToUnity(voronoi);
        //GenerateBuildings();
    }
    void GenerateBuildings()
    {
        foreach (Cell cell in cells)
        {
            ProceduralBuilding building = Instantiate(buildingTemplate);
            building.transform.parent = this.transform;
            building.transform.position = cell.Center;
            int height = Random.Range(2, 20);

            building.Generate(cell.localContour.ToArray(), height, new Vector2(Random.Range(1, 5), Random.Range(1, 5)));
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
                if (!InBound(ce.CircumCenter))
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
            deflatedCells.Add(cell.Deflate(0.1f));
        }
        Subdivide();
    }
    List<Vector3> cellDivision;
    List<KeyValuePair<Vector3, Vector3>> divisions = new List<KeyValuePair<Vector3, Vector3>>();
    void Subdivide()
    {
        int subdivision = 5;
        cellDivision = new List<Vector3>();
        blocks = new List<Graph>();
        foreach (Cell cell in deflatedCells)
        {

            Graph graph = cell.AsGraph();
            blocks.Add(graph);
            Bounds bounds = cell.Bounds;
            Vector3 delta = bounds.size / subdivision;
            bool[,] valid = new bool[subdivision + 1, subdivision + 1];
            Graph.Node[,] nodes = new Graph.Node[subdivision + 1, subdivision + 1];
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
                        var node = new Graph.Node();
                        node.position = point;
                        graph.nodes.Add(node);
                        nodes[x, z] = node;
                    }
                }
            }
            for (int x = 0; x <= subdivision; ++x)
            {
                float u = x / (float)subdivision * bounds.size.x + bounds.min.x;
                for (int z = 0; z <= subdivision; ++z)
                {
                    float v = z / (float)subdivision * bounds.size.z + bounds.min.z;
                    if (valid[x, z])
                    {
                        Vector3 point = new Vector3(u, 0, v);
                        int x2 = x + 1;
                        if (x2 <= subdivision && !valid[x2, z])
                        {
                            float u2 = x2 / (float)subdivision * bounds.size.x + bounds.min.x;
                            Vector3 border = new Vector3(u2, 0, v);
                            var node = new Graph.Node();
                            int segment;
                            node.position = Intersects(cell, point, border, out segment);
                            graph.nodes.Add(node);

                            Graph.Link link = new Graph.Link();
                            link.from = nodes[x, z];
                            link.to = node;
                            graph.links.Add(link);
                        }
                        else
                        {
                            Graph.Link link = new Graph.Link();
                            link.from = nodes[x, z];
                            link.to = nodes[x2, z];
                            graph.links.Add(link);
                        }
                        x2 = x - 1;
                        if (x2 >= 0 && !valid[x2, z])
                        {
                            float u2 = x2 / (float)subdivision * bounds.size.x + bounds.min.x;
                            Vector3 border = new Vector3(u2, 0, v);
                            var node = new Graph.Node();
                            int segment;
                            node.position = Intersects(cell, point, border, out segment);
                            graph.nodes.Add(node);

                            Graph.Link link = new Graph.Link();
                            link.from = nodes[x, z];
                            link.to = node;
                            graph.links.Add(link);
                        }
                        else
                        {
                            Graph.Link link = new Graph.Link();
                            link.from = nodes[x, z];
                            link.to = nodes[x2, z];
                            graph.links.Add(link);
                        }
                        int z2 = z + 1;
                        if (z2 <= subdivision && !valid[x, z2])
                        {
                            float v2 = z2 / (float)subdivision * bounds.size.z + bounds.min.z;
                            Vector3 border = new Vector3(u, 0, v2);
                            var node = new Graph.Node();
                            int segment;
                            node.position = Intersects(cell, point, border, out segment);
                            graph.nodes.Add(node);

                            Graph.Link link = new Graph.Link();
                            link.from = nodes[x, z];
                            link.to = node;
                            graph.links.Add(link);
                        }
                        else
                        {
                            Graph.Link link = new Graph.Link();
                            link.from = nodes[x, z];
                            link.to = nodes[x, z2];
                            graph.links.Add(link);
                        }

                        z2 = z - 1;
                        if (z2 >= 0 && !valid[x, z2])
                        {
                            float v2 = z2 / (float)subdivision * bounds.size.z + bounds.min.z;
                            Vector3 border = new Vector3(u, 0, v2);
                            var node = new Graph.Node();
                            int segment;
                            node.position = Intersects(cell, point, border, out segment);
                            graph.nodes.Add(node);

                            Graph.Link link = new Graph.Link();
                            link.from = nodes[x, z];
                            link.to = node;
                            graph.links.Add(link);
                        }
                        else
                        {
                            Graph.Link link = new Graph.Link();
                            link.from = nodes[x, z];
                            link.to = nodes[x, z2];
                            graph.links.Add(link);
                        }
                    }
                    /*
                        Vector3 p01 = new Vector3(point.x - delta.x, 0, point.z);
                        if (!cell.ContainsPoint(p01))
                        {
                            Vector3 intersection;
                            if (Intersects(idx, point, p01, out intersection))
                            {
                                cellDivision.Add(intersection);
                            }
                        }
                        Vector3 p21 = new Vector3(point.x + delta.x, 0, point.z);
                        if (!cell.ContainsPoint(p21))
                        {
                            Vector3 intersection;
                            if (Intersects(idx, point, p21, out intersection))
                            {
                                cellDivision.Add(intersection);
                            }
                        }
                        Vector3 p10 = new Vector3(point.x, 0, point.z - delta.z);
                        if (!cell.ContainsPoint(p10))
                        {
                            Vector3 intersection;
                            if (Intersects(idx, point, p10, out intersection))
                            {
                                cellDivision.Add(intersection);
                            }
                        }
                        Vector3 p12 = new Vector3(point.x, 0, point.z + delta.z);
                        if (!cell.ContainsPoint(p12))
                        {
                            Vector3 intersection;
                            if (Intersects(idx, point, p12, out intersection))
                            {
                                cellDivision.Add(intersection);
                            }
                        }
                    }*/
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (cells == null)
        {
            return;
        }
        /*int count = 0;
        foreach (Cell cell in cells)
        {
            // Gizmos.DrawWireCube(cell.Bounds.center, cell.Bounds.size);
            Gizmos.color = Color.white;
            Vector3 centroid = cell.Center;
            for (int c = 0; c < cell.Contour.Length; ++c)
            {
                Gizmos.DrawSphere(cell.Center, 0.2f);
                Vector3 p0 = cell.Contour[c];
                Vector3 p1 = cell.Contour[(c + 1) % cell.Contour.Length];
                Gizmos.DrawLine(p0, p1);
            }
        }*/

        foreach (Graph graph in blocks)
        {
            foreach (var node in graph.nodes)
            {
                Gizmos.DrawSphere(node.position, 0.25f);
            }
            foreach (var link in graph.links)
            {
                Gizmos.DrawLine(link.from.position, link.to.position);
            }
        }
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

    Vector3 Intersects(Cell cell, Vector3 origin, Vector3 target, out int id)
    {
        List<Vector3> list = new List<Vector3>();
        Vector3 intersection = origin;// origin + Vector3.up ;
        if (cell.ContainsPoint(target))
        {
            intersection = target;
            id = -1;
        }
        else
        {
            Vector3 result = target;
            for (int i = 0; i < cell.Contour.Count; ++i)
            {
                Vector3 from = cell.Contour[i];
                Vector3 to = cell.Contour[(i + 1) % cell.Contour.Count];
                Vector3 result2;
                Math3d.LineLineIntersection(out result, origin, target - origin, from, to - from);
                {
                    if (cell.Bounds.Contains(result))
                    {
                        list.Add(result);

                    }
                }
            }
            id = 0;
            intersection = list[0];
            for (int t = 1; t < list.Count; ++t)
            {
                if (Vector3.Distance(origin, intersection) > Vector3.Distance(origin, list[t]))
                {
                    intersection = list[t];
                    id = t;
                }
            }
        }
        return intersection;
        //return intersection;
    }


    private bool InBound(Vertex2 v)
    {
        if (v.X < -size || v.X > size) return false;
        if (v.Y < -size || v.Y > size) return false;

        return true;
    }

}








