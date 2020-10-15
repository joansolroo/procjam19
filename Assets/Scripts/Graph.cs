using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph
{
    public class Node
    {
        public int Id;
        public Vector2 position;
        public List<Link> links = new List<Link>();
        public List<Cell> cells = new List<Cell>();

        public override string ToString()
        {
            return "(" + Id + ")" + position;
        }

        public Node(Vector2 _p, int _id = -1)
        {
            position = _p;
            Id = _id;
        }
    }
    public class Link
    {
        public int Id;
        public Node start;
        public Node end;
        public List<Cell> cells = new List<Cell>();

        public override string ToString()
        {
            return "(" + Id + ") [" + start.Id + " , " + end.Id + " ]";
        }

        public Link(Node _from, Node _to, int _id = -1)
        {
            start = _from;
            end = _to;
            Id = _id;
        }
    }
    public class Cell
    {
        static float epsilon = 0.001f;

        // Atributes
        public int Id;
        public Cell parent;
        public Vector2 barycenter;
        public float boundingRadius;

        //debug
        public int classified = 0;

        public List<Node> corners = new List<Node>();
        public List<Link> links = new List<Link>();

        // Interface
        public Cell(List<Node> _corners, int id)
        {
            Id = id;
            parent = null;
            corners = _corners;

            if (corners[0] == corners[corners.Count - 1])
                corners.RemoveAt(corners.Count - 1);

            ComputeBarycenter();
        }
        public bool ContainPoint(Vector2 p)
        {
            if (corners.Count < 3)
                return false;

            int referenceSign = Sign(corners[1].position - corners[0].position, barycenter - corners[0].position);
            for (int i = 0; i < corners.Count; ++i)
            {
                Vector2 A = corners[i].position;
                Vector2 B = corners[(i + 1) % corners.Count].position;
                
                if (Sign(B - A, p - A) != referenceSign)
                    return false;
            }
            return true;
        }
        
        // Helpers
        private void ComputeBarycenter()
        {
            barycenter = Vector3.zero;
            foreach (Node n in corners)
                barycenter += n.position;
            if (corners.Count > 0)
                barycenter /= corners.Count;

            boundingRadius = 0f;
            foreach (Node n in corners)
            {
                float r = (n.position - barycenter).magnitude;
                if(boundingRadius < r)
                    boundingRadius = r;
            }
        }
        private float ComputeArea()
        {
            float totalArea = 0f;
            for (int i = 0; i < corners.Count; ++i)
            {
                Vector2 A = corners[i].position;
                Vector2 B = corners[(i + 1) % corners.Count].position;
                totalArea += TriangleArea(B - barycenter, A - barycenter);
            }
            return totalArea;
        }
        private float ComputePerimeter()
        {
            float totalPerimeter = 0f;
            for (int i = 0; i < corners.Count; ++i)
            {
                Vector2 A = corners[i].position;
                Vector2 B = corners[(i + 1) % corners.Count].position;

                totalPerimeter += Mathf.Abs((B - A).magnitude);
            }
            return totalPerimeter;
        }
        
        private int Sign(Vector2 a, Vector2 b)
        {
            return a.x* b.y - a.y * b.x > 0f ? 1 : -1;
        }
        private float TriangleArea(Vector2 a, Vector2 b)
        {
            return 0.5f * Mathf.Abs(a.x * b.y - a.y * b.x);
        }
    }

    public Cell cell;
    public Dictionary<Vector2, Link> cellLink = new Dictionary<Vector2, Link>();

    public Bounds boundaries = new Bounds();
    private int nodeIndex = 0;
    private int linkIndex = 0;
    private int cellIndex = 0;

    public Dictionary<int, Link> links = new Dictionary<int, Link>();
    public Dictionary<int, Node> nodes = new Dictionary<int, Node>();
    public Dictionary<int, Cell> cells = new Dictionary<int, Cell>();

    public Dictionary<Vector2, Node> nodesDictionary = new Dictionary<Vector2, Node>();
    public Dictionary<KeyValuePair<Vector2, Vector2>, Link> linksDictionary = new Dictionary<KeyValuePair<Vector2, Vector2>, Link>();

    public virtual Cell GetCellAt(Vector2 position)
    {
        foreach(KeyValuePair<int, Cell> entry in cells)
        {
            if (entry.Value.ContainPoint(position))
                return entry.Value;
        }
        return null;
    }

    public virtual Node AddNode(Vector2 position)
    {
        if (nodesDictionary.ContainsKey(position))
            return nodesDictionary[position];
        else
        {
            Node n = new Node(position, nodeIndex++);
            nodesDictionary.Add(position, n);
            nodes.Add(n.Id, n);
            return n;
        }
    }
    public virtual Link AddLink(Vector2 p1, Vector2 p2)
    {
        if (nodesDictionary.ContainsKey(p1) && nodesDictionary.ContainsKey(p1))
        {
            Node n1 = nodesDictionary[p1];
            Node n2 = nodesDictionary[p2];
            return AddLink(n1, n2);
        }
        else return null;
    }
    public virtual Link AddLink(Node n1, Node n2)
    {
        if (Vector3.Distance(n1.position, n2.position) > float.Epsilon)
        {
            KeyValuePair<Vector2, Vector2> entry1 = new KeyValuePair<Vector2, Vector2>(n1.position, n2.position);
            KeyValuePair<Vector2, Vector2> entry2 = new KeyValuePair<Vector2, Vector2>(n2.position, n1.position);

            if (linksDictionary.ContainsKey(entry1))
                return linksDictionary[entry1];
            if (linksDictionary.ContainsKey(entry1))
                return linksDictionary[entry2];

            Link link = new Link(n1, n2, linkIndex++);
            linksDictionary.Add(entry1, link);
            links.Add(link.Id, link);
            return link;
        }
        else return null;
    }
    public virtual Cell AddCell(List<Node> corners)
    {
        Cell c = new Cell(corners, cellIndex++);
        cells.Add(c.Id, c);
        return c;
    }

    public void Clear()
    {
        links.Clear();
        nodes.Clear();
        cells.Clear();
        nodesDictionary.Clear();
    }

    private Link AddCellLink(Node n1, Node n2)
    {
        if (cellLink.ContainsKey(n1.position))
            return cellLink[n1.position];

        Link link = new Link(n1, n2, linkIndex++);
        cellLink.Add(n1.position, link);
        return link;
    }

    public void CellAutoLinkage()
    {
        for (int i = 0; i < cell.corners.Count; i++)
        {
            Node n1 = cell.corners[i];
            Node n2 = cell.corners[(i + 1) % cell.corners.Count];
            Node n3 = cell.corners[(i - 1 + cell.corners.Count) % cell.corners.Count];

            Link l1 = AddCellLink(n1, n2);
            Link l2 = AddCellLink(n2, n3);

            n1.links.Add(l1);   n1.links.Add(l2);
            n1.cells.Add(cell);
            l1.cells.Add(cell);
            cell.links.Add(l1);
        }
    }

    public virtual void Generate() { }
    public virtual void Simplify() { }
}