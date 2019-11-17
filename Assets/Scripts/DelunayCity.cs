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
            static int MAX_ID;
            int index = ++MAX_ID;
            public Vector3 position;
            public bool shell;
            public override bool Equals(object obj)
            {
                Node other = obj as Node;
                return obj != null && other.index == this.index;
            }

            public override int GetHashCode()
            {
                return index;
            }
            public Node(Vector3 position, bool shell = false)
            {
                this.position = position;
                this.shell = shell;
            }
        }
        public class Link
        {
            static int MAX_ID;
            int index = ++MAX_ID;
            public Node from;
            public Node to;
            public bool shell = false;

            public override bool Equals(object obj)
            {
                Link other = obj as Link;
                return obj != null && other.index == this.index;
            }

            public override int GetHashCode()
            {
                return index;
            }

            public Link(Node from, Node to, bool shell = false)
            {
                this.from = from;
                this.to = to;
                this.shell = shell;
            }
        }
        public Cell Contour;
        public List<Node> nodes;
        public List<Link> links;
        public List<List<Link>> segments;
        public Dictionary<Graph.Node, List<Graph.Node>> neighbours;
        
        public void ComputeNeighbours()
        {
            neighbours = new Dictionary<Graph.Node, List<Graph.Node>>();
            //compute closed cells inside the graph
            foreach (Graph.Link link in this.links)
            {
                if (!neighbours.ContainsKey(link.from))
                    neighbours[link.from] = new List<Graph.Node>();
                neighbours[link.from].Add(link.to);
                if (!neighbours.ContainsKey(link.to))
                    neighbours[link.to] = new List<Graph.Node>();
                neighbours[link.to].Add(link.from);
            }
        }

        public Link Segment(int linkId, Node node)
        {
            Link linkToSplit = links[linkId];
            float nodePosition = (node.position - linkToSplit.from.position).sqrMagnitude;
            for (int segIdx = 0; segIdx < segments[linkId].Count; ++segIdx)
            {

                Link segment = segments[linkId][segIdx];
                float startPosition = (segment.from.position - linkToSplit.from.position).sqrMagnitude;
                float endPosition = (segment.to.position - linkToSplit.from.position).sqrMagnitude;

                if (nodePosition < startPosition)
                {
                    break;
                }
                else if (nodePosition < endPosition)
                {
                    Link newSegment = new Link(node, segment.to, segment.shell);
                    segment.to = node;
                    segments[linkId].Insert(segIdx, newSegment);
                    links.Add(newSegment);
                    return newSegment;
                }

            }

            return null;
        }
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
            graph.segments = new List<List<Graph.Link>>();
            for (int i = 0; i < contour.Count; ++i)
            {
                Graph.Node node = new Graph.Node(contour[i], true);
                graph.nodes.Add(node);
            }

            for (int i = 0; i < contour.Count; ++i)
            {
                Graph.Link link = new Graph.Link(graph.nodes[i], graph.nodes[(i + 1) % contour.Count], true);
                graph.links.Add(link);
                graph.segments.Add(new List<Graph.Link>());
                graph.segments[i].Add(link);
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
        public Vector3 Intersects(Vector3 origin, Vector3 target, out int id)
        {
            Vector3 intersection = origin;// origin + Vector3.up ;
            if (this.ContainsPoint(target))
            {
                intersection = target;
                id = -1;
            }
            else
            {
                List<Vector3> list = new List<Vector3>();
                List<int> listIdx = new List<int>();
                Vector3 result = target;
                for (int i = 0; i < this.Contour.Count; ++i)
                {
                    Vector3 from = this.Contour[i];
                    Vector3 to = this.Contour[(i + 1) % this.Contour.Count];
                    Vector3 result2;
                    Math3d.LineLineIntersection(out result, origin, target - origin, from, to - from);
                    {
                        if (this.Bounds.Contains(result))
                        {
                            list.Add(result);
                            listIdx.Add(i);
                        }
                    }
                }
                intersection = list[0];
                id = listIdx[0];
                for (int t = 1; t < list.Count; ++t)
                {
                    if (Vector3.Distance(origin, intersection) > Vector3.Distance(origin, list[t]))
                    {
                        intersection = list[t];
                        id = listIdx[t];
                    }
                }
            }
            return intersection;
            //return intersection;
        }
    }
    List<Cell> cells;
    List<Cell> deflatedCells;
    List<Graph> blocks;
    List<Cell> buildingContours;
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
        Subdivide();
        //GenerateBuildings(buildingContours);
    }
    void GenerateBuildings(List<Cell> cells)
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
    }
    List<Vector3> cellDivision;
    List<KeyValuePair<Vector3, Vector3>> divisions = new List<KeyValuePair<Vector3, Vector3>>();
    void Subdivide()
    {
        int subdivision = 5;
        cellDivision = new List<Vector3>();
        blocks = new List<Graph>();
        buildingContours = new List<Cell>();
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
                        var node = new Graph.Node(point);
                        graph.nodes.Add(node);
                        nodes[x, z] = node;
                    }
                }
            }
            List<Graph.Link> unused = new List<Graph.Link>();
            // create internal nodes and links, divide shell edges when intersecting them 
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

                            int segment;
                            Vector3 position = cell.Intersects(point, border, out segment);
                            var node = new Graph.Node(position, true);
                            graph.nodes.Add(node);

                            Graph.Link link = new Graph.Link(nodes[x, z], node);
                            graph.links.Add(link);
                            Graph.Link shellSegment = graph.Segment(segment, node);
                            unused.Add(shellSegment);
                        }
                        else
                        {
                            Graph.Link link = new Graph.Link(nodes[x, z], nodes[x2, z]);
                            graph.links.Add(link);
                        }
                        x2 = x - 1;
                        if (x2 >= 0 && !valid[x2, z])
                        {
                            float u2 = x2 / (float)subdivision * bounds.size.x + bounds.min.x;
                            Vector3 border = new Vector3(u2, 0, v);
                            int segment;
                            Vector3 position = cell.Intersects(point, border, out segment);
                            var node = new Graph.Node(position, true);
                            graph.nodes.Add(node);

                            Graph.Link link = new Graph.Link(nodes[x, z], node);
                            graph.links.Add(link);
                            Graph.Link shellSegment = graph.Segment(segment, node);
                            unused.Add(shellSegment);
                        }
                        else
                        {
                            Graph.Link link = new Graph.Link(nodes[x, z], nodes[x2, z]);
                            graph.links.Add(link);
                        }
                        int z2 = z + 1;
                        if (z2 <= subdivision && !valid[x, z2])
                        {
                            float v2 = z2 / (float)subdivision * bounds.size.z + bounds.min.z;
                            Vector3 border = new Vector3(u, 0, v2);
                            int segment;
                            Vector3 position = cell.Intersects(point, border, out segment);
                            var node = new Graph.Node(position, true);
                            graph.nodes.Add(node);

                            Graph.Link link = new Graph.Link(nodes[x, z], node);
                            graph.links.Add(link);
                            Graph.Link shellSegment = graph.Segment(segment, node);
                            unused.Add(shellSegment);
                        }
                        else
                        {
                            Graph.Link link = new Graph.Link(nodes[x, z], nodes[x, z2]);
                            graph.links.Add(link);
                        }

                        z2 = z - 1;
                        if (z2 >= 0 && !valid[x, z2])
                        {
                            float v2 = z2 / (float)subdivision * bounds.size.z + bounds.min.z;
                            Vector3 border = new Vector3(u, 0, v2);
                            int segment;
                            Vector3 position = cell.Intersects(point, border, out segment);
                            var node = new Graph.Node(position, true);
                            graph.nodes.Add(node);

                            Graph.Link link = new Graph.Link(nodes[x, z], node);
                            graph.links.Add(link);
                            Graph.Link shellSegment = graph.Segment(segment, node);
                            unused.Add(shellSegment);
                        }
                        else
                        {
                            Graph.Link link = new Graph.Link(nodes[x, z], nodes[x, z2]);
                            graph.links.Add(link);
                        }
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
            /*
            graph.ComputeNeighbours();
            while (unused.Count > 0)
            {
                Debug.Log("sorting");
                List<Graph.Link> contour = new List<Graph.Link>();
                Graph.Link link = unused[0];
                
                unused.RemoveAt(0);
                if (link == null)
                {
                    continue;
                }
                contour.Add(link);

                Queue<List<Graph.Node>> q = new Queue<List<Graph.Node>>();
                HashSet<Graph.Node> used = new HashSet<Graph.Node>();
                var start = new List<Graph.Node>();
                start.Add(link.from);
                q.Enqueue(start);

                int count = 0;
                List<Graph.Node> solution = null;
                while (q.Count > 0 && count < 100 && solution==null)
                {
                    List<Graph.Node> currentSolution = q.Dequeue();
                    Graph.Node current = currentSolution[currentSolution.Count - 1];

                    if (current == null || used.Contains(current))
                        continue;
                    used.Add(current);
                    foreach (var next in graph.neighbours[current])
                    {
                        if ((next.shell && graph.neighbours[current].Count == 1)
                            || (!next.shell && graph.neighbours[current].Count > 1))
                        {
                            var newSolution = new List<Graph.Node>(currentSolution);
                            newSolution.Add(link.from);
                            q.Enqueue(newSolution);
                        }
                    }
                    if (count>2 && current == link.from)
                    {
                        Debug.Log("found in " + count + " steps");
                        solution = currentSolution;
                    }
                    ++count;
                }
                {
                    Debug.Log("STATUS:"+ solution + ", in " + count + " steps");
                }
            }*/
            /*
            Dictionary<Graph.Node, List<Graph.Link>> neighbours = new Dictionary<Graph.Node, List<Graph.Link>>();
            //compute closed cells inside the graph
            foreach (Graph.Link link in graph.links)
            {
                if (!neighbours.ContainsKey(link.from))
                    neighbours[link.from] = new List<Graph.Link>();
                neighbours[link.from].Add(link);
                if (!neighbours.ContainsKey(link.to))
                    neighbours[link.to] = new List<Graph.Link>();
                neighbours[link.to].Add(link);
            }
            HashSet<Graph.Link> contourSegments = new HashSet<Graph.Link>();
            Dictionary<Graph.Link,int> used = new Dictionary<Graph.Link, int>();
            foreach (var link in graph.segments)
            {
                foreach (Graph.Link segment in link)
                {
                    contourSegments.Add(segment);
                    used[segment] = 0;
                }
            }
            foreach (Graph.Link segment in contourSegments)
            {
                if (used[segment]==0)
                {
                    Graph.Node start = segment.from;
                    List<Graph.Node> cellContour = new List<Graph.Node>();
                    ++used[segment];
                }
            }
            */
        }
    }
    
    private void OnDrawGizmos()
    {
        if (cells == null)
        {
            return;
        }

        foreach (Graph graph in blocks)
        {
            Gizmos.color = Color.white;
            foreach (var node in graph.nodes)
            {
                Gizmos.color = node.shell ? Color.green : Color.gray;
                Gizmos.DrawSphere(node.position, 5f);
            }
            foreach (var link in graph.links)
            {
                Gizmos.color = link.shell ? Color.green : Color.gray;
                Gizmos.DrawLine(link.from.position, link.to.position);
            }
            /*Gizmos.color = Color.green;
            foreach (var link in graph.segments)
            {
                foreach (var segment in link)
                {
                    Gizmos.DrawLine(segment.from.position, (segment.to.position- segment.from.position)*0.75f+ segment.from.position);
                }
            }*/
        }
        foreach (Cell cell in buildingContours)
        {
            // Gizmos.DrawWireCube(cell.Bounds.center, cell.Bounds.size);
            Gizmos.color = Color.yellow;
            Vector3 centroid = cell.Center;
            for (int c = 0; c < cell.Contour.Count; ++c)
            {
                Gizmos.DrawSphere(cell.Center, 2f);
                Vector3 p0 = cell.Contour[c];
                Vector3 p1 = cell.Contour[(c + 1) % cell.Contour.Count];
                Gizmos.DrawLine(p0, p1);
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


    private bool InBound(Vertex2 v)
    {
        if (v.X < -size || v.X > size) return false;
        if (v.Y < -size || v.Y > size) return false;

        return true;
    }

}








