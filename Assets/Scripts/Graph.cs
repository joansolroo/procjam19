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

        public override string ToString()
        {
            return "(" + Id + ")" + position;
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
    public class Cell
    {
        private Cell parent;
        private Bounds bounds;
        private Vector3 center;
        private List<Node> contour;
        private List<Link> links;
        public List<Cell> children = new List<Cell>();
        public float Area
        {
            get
            {
                float sum = 0;
                for (int i = 0; i < contour.Count; ++i)
                {
                    Vector3 from = contour[i].position;
                    Vector3 to = contour[(i + 1) % contour.Count].position;

                    sum += (to.x - from.x) * (to.z + from.z);
                }
                return Mathf.Abs(sum);
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
                Vector3 from = contour[i].position;
                Vector3 to = contour[(i + 1) % contour.Count].position;

                sum += (to.x - from.x) * (to.z + from.z);
            }
            return sum > 0 ? 1 : -1;
        }
        public void Flip()
        {
            for (int i = 0; i < contour.Count / 2; ++i)
            {
                Node left = contour[i];
                Node right = contour[contour.Count - i - 1];
                contour[i] = right;
                contour[contour.Count - i - 1] = left;
            }
        }
        public Cell(List<Node> contour)
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
            graph.links = new List<GraphLinked.Link>();
            graph.segments = new List<List<GraphLinked.Link>>();
            /*for (int i = 0; i < contour.Count; ++i)
            {
                  graph.AddNode(contour[i], true);
            }*/

            for (int i = 0; i < contour.Count; ++i)
            {
                GraphLinked.Link link = new GraphLinked.Link(contour[i],contour[(i + 1) % contour.Count], true);
                graph.links.Add(link);
                graph.segments.Add(new List<GraphLinked.Link>());
                graph.segments[i].Add(link);
            }
            return graph;
        }
        /*public Cell Deflated(float margin)
        {
            List<Vector3> deflatedContour = new List<Vector3>();
            for (int c = 0; c < Contour.Count; ++c)
            {
                Vector3 p = (Contour[c].position - Center) * (1 - margin) + Center;
                deflatedContour.Add(p);
            }
            Cell deflated = new Cell(deflatedContour);
            deflated.Parent = this;
            return deflated;
        }*/
        public void Deflate(float margin)
        {
            Vector3 newCenter = Vector3.zero;
            for (int c = 0; c < Contour.Count; ++c)
            {
                Contour[c].position = (Contour[c].position - Center) * (1 - margin) + Center;
            }
            RecalculateBounds();
        }
        void RecalculateBounds()
        {
            Vector3 min = new Vector3(float.MaxValue, 0, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, 0, float.MinValue);
            Vector3 center = Vector3.zero;
            foreach (Node n in Contour)
            {
                Vector3 p = n.position;
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
                foreach (Node node in Contour)
                {
                    result.Add(node.position - Center);
                }
                return result;
            }
        }

        public Bounds Bounds { get => bounds; private set => bounds = value; }
        public List<Node> Contour
        {
            get => contour;
            set
            {
                contour = value;
                RecalculateBounds();
            }
        }
        public Vector3 Center { get => center; private set => center = value; }
        public Cell Parent
        {
            get => parent;
            set
            {
                if (parent != value)
                {
                    if (parent != null)
                    {
                        parent.children.Remove(this);
                    }
                    parent = value;
                    if (parent != null)
                    {
                        parent.children.Add(this);
                    }
                }
            }
        }

        public bool ContainsPoint(Vector3 point)
        {
            return ContainsPoint(Contour, point);
        }
        public static bool ContainsPoint(List<Node> polyPoints, Vector3 p)
        {
            var j = polyPoints.Count - 1;
            var inside = false;
            for (int i = 0; i < polyPoints.Count; j = i++)
            {
                var pi = polyPoints[i].position;
                var pj = polyPoints[j].position;
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
                    Vector3 from = this.Contour[i].position;
                    Vector3 to = this.Contour[(i + 1) % this.Contour.Count].position;
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
    public Cell Contour;
    //public List<Node> nodes;
    List<Link> links = new List<Link>();
    public List<List<Link>> segments;
    private Dictionary<Hash128, GraphLinked.Node> nodes = new Dictionary<Hash128, GraphLinked.Node>();

    public bool NodeExists(Vector3 position)
    {
        return nodes.ContainsKey(Hash(position));
    }
    public Node GetNode(Vector3 position)
    {
        if (NodeExists(position))
        {
            return nodes[Hash(position)];
        }
        else
        {
            return null;
        }
    }
    Hash128 Hash(Vector3 position)
    {
        Hash128 h = new Hash128();
        HashUtilities.QuantisedVectorHash(ref position, ref h);
        return h;
    }
    public Node AddNode(Vector3 position, bool shell = false)
    {
        if (NodeExists(position))
        {
           
            return nodes[Hash(position)];
        }
        else
        {
            return nodes[Hash(position)] = new Node(position, shell);
        }
    }
    public Link AddLink(Vector3 from, Vector3 to)
    {
        if (NodeExists(from) && NodeExists(to))
        {
            Node fromNode = GetNode(from);
            Node toNode = GetNode(to);
            return AddLink(fromNode, toNode);
        }
        else
        {
            return null;
        }
    }
    public List<Link> GetLinks()
    {
        return links;
    }
   
    public Link AddLink(Node fromNode, Node toNode)
    {
        if (Vector3.Distance(fromNode.position, toNode.position) > float.Epsilon)
        {
            Link link = new Link(fromNode, toNode, fromNode.shell && toNode.shell);
            links.Add(link);
            return link;
        }
        else
        {
            return null;
        }
    }
    /*public Link GetLink(Vector3 from, Vector3 to)
    {

        if (NodeExists(from) && NodeExists(to))
        {
            Node fromNode = GetNode(from);
            Node toNode = GetNode(to);
            return AddLink(fromNode, toNode);
        }
        else
        {
            return null;
        }
    }*/
    public Dictionary<GraphLinked.Node, List<GraphLinked.Link>> ComputeNeighbours()
    {
        Dictionary<GraphLinked.Node, List<GraphLinked.Link>> neighbours = new Dictionary<GraphLinked.Node, List<GraphLinked.Link>>();
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
        return neighbours;
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
            intersectionNode = AddNode(intersectionPosition,true);
            AddLink(from, intersectionNode);
            
        }
        GraphLinked.Link shellSegment = this.Segment(segment, intersectionNode);
    }
}