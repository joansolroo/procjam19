using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphLinked : Graph
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
        public int Id { get { return index; } }
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
    public Dictionary<GraphLinked.Node, List<GraphLinked.Link>> neighbours;

    public void ComputeNeighbours()
    {
        neighbours = new Dictionary<GraphLinked.Node, List<GraphLinked.Link>>();
        //compute closed cells inside the graph
        foreach (GraphLinked.Link link in this.links)
        {
            if (!neighbours.ContainsKey(link.from))
                neighbours[link.from] = new List<GraphLinked.Link>();
            neighbours[link.from].Add(link);
            if (!neighbours.ContainsKey(link.to))
                neighbours[link.to] = new List<GraphLinked.Link>();
            neighbours[link.to].Add(link);
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
                /*Link newSegment = new Link(segment.from, node, segment.shell);
                segment.from = node;
                segments[linkId].Insert(segIdx, newSegment);
                links.Add(newSegment);
                return newSegment;*/
                break;
            }
            else if (nodePosition < endPosition)
            {
                Link newSegment = new Link(node, segment.to, segment.shell);
                segment.to = node;
                segments[linkId].Insert(segIdx + 1, newSegment);
                links.Add(newSegment);
                return newSegment;
            }

        }

        return null;
    }

    internal void SegmentShell(Node from, Vector3 targetPosition)
    {
        int segment;
        Vector3 intersectionPosition = this.Contour.Intersects(from.position, targetPosition, out segment);
        var intersectionNode = from;
        if (Vector3.Distance(intersectionPosition, from.position) > 0.1f)
        {
            intersectionNode = new GraphLinked.Node(intersectionPosition, true);
            this.nodes.Add(intersectionNode);
            GraphLinked.Link link = new GraphLinked.Link(from, intersectionNode);
            this.links.Add(link);
        }
        GraphLinked.Link shellSegment = this.Segment(segment, intersectionNode);
    }
}
public class Cell
{
    private Bounds bounds;
    private Vector3 center;
    private List<Vector3> contour;

    public float Area
    {
        get
        {
            float area = 0;
            for (int i = 0; i < contour.Count; ++i)
            {
                Vector3 from = contour[i];
                Vector3 to = contour[(i + 1) % contour.Count];

                area += Vector3.Distance(center, (from + to) / 2) * Vector3.Distance(from, to);
            }
            return area / 2;
        }
    }

    //Sum over the edges, (x2 − x1)(y2 + y1). 
    //If the result is positive the curve is clockwise, if it's negative the curve is counter-clockwise. (The result is twice the enclosed area, with a +/- convention.)
    public int Handness()
    {
        //Sum(x2 − x1)(y2 + y1) < 0?
        float sum = 0;
        for (int i = 0; i < contour.Count; ++i)
        {
            Vector3 from = contour[i];
            Vector3 to = contour[(i + 1) % contour.Count];

            sum += (to.x - from.x) * (to.z + from.z);
        }
        return sum > 0 ? 1 : -1;
    }
    public void Flip()
    {
        for (int i = 0; i < contour.Count / 2; ++i)
        {
            Vector3 left = contour[i];
            Vector3 right = contour[contour.Count - i - 1];
            contour[i] = right;
            contour[contour.Count - i - 1] = left;
        }
    }
    public Cell(List<Vector3> contour)
    {
        Contour = contour;
        int lastIdx = contour.Count - 1;
        if (contour[0] == contour[contour.Count - 1])
        {
            contour.RemoveAt(contour.Count - 1);
        }
        if (Handness() > 0)
        {
            Flip();
        }
    }
    public GraphLinked AsGraph()
    {
        GraphLinked graph = new GraphLinked();
        graph.Contour = this;
        graph.nodes = new List<GraphLinked.Node>();
        graph.links = new List<GraphLinked.Link>();
        graph.segments = new List<List<GraphLinked.Link>>();
        for (int i = 0; i < contour.Count; ++i)
        {
            GraphLinked.Node node = new GraphLinked.Node(contour[i], true);
            graph.nodes.Add(node);
        }

        for (int i = 0; i < contour.Count; ++i)
        {
            GraphLinked.Link link = new GraphLinked.Link(graph.nodes[i], graph.nodes[(i + 1) % contour.Count], true);
            graph.links.Add(link);
            graph.segments.Add(new List<GraphLinked.Link>());
            graph.segments[i].Add(link);
        }
        return graph;
    }
    public Cell Deflated(float margin)
    {
        List<Vector3> deflatedContour = new List<Vector3>();
        for (int c = 0; c < Contour.Count; ++c)
        {
            Vector3 p = (Contour[c] - Center) * (1 - margin) + Center;
            deflatedContour.Add(p);
        }
        return new Cell(deflatedContour);
    }
    public void Deflate(float margin)
    {
        Vector3 newCenter = Vector3.zero;
        for (int c = 0; c < Contour.Count; ++c)
        {
            Contour[c] = (Contour[c] - Center) * (1 - margin) + Center;
        }
        RecalculateBounds();
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