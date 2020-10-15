using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegularGraph : Graph
{
    public Vector2 offset;
    public float rotation;
    public Vector2 cellSize;

    public RegularGraph(Vector2 cellSize, Bounds bound, Vector2 displacement, float angle)
    {
        this.cellSize = cellSize;
        boundaries = bound;
        offset = displacement;
        rotation = angle;
    }

    public override void Generate()
    {
        int cellCountx = (int)(boundaries.size.x * Mathf.Sqrt(2) / cellSize.x) + 3;
        int cellCounty = (int)(boundaries.size.z * Mathf.Sqrt(2) / cellSize.y) + 3;

        // nodes
        for (int i=0; i<cellCountx; i++)
            for (int j = 0; j < cellCounty; j++)
            {
                Vector2 p = GetNodePosition(i, j);
                AddNode(p);
            }

        // links and cells
        for (int i = 0; i < cellCountx - 1; i++)
            for (int j = 0; j < cellCounty - 1; j++)
            {
                Node p1 = AddNode(GetNodePosition(i, j));
                Node p2 = AddNode(GetNodePosition(i + 1, j));
                Node p3 = AddNode(GetNodePosition(i, j + 1));
                Node p4 = AddNode(GetNodePosition(i + 1, j + 1));

                Link l1 = AddLink(p1, p2);
                Link l2 = AddLink(p1, p3);
                Link l3 = AddLink(p2, p4);
                Link l4 = AddLink(p3, p4);

                List<Node> nodes = new List<Node>();
                nodes.Add(p1); nodes.Add(p2); nodes.Add(p4); nodes.Add(p3);
                Cell c = AddCell(nodes);

                p1.links.Add(l1); p1.links.Add(l2); p2.links.Add(l1); p3.links.Add(l2);
                p1.cells.Add(c);  p2.cells.Add(c);  p3.cells.Add(c);  p4.cells.Add(c);

                l1.cells.Add(c);  l2.cells.Add(c);  l3.cells.Add(c);  l4.cells.Add(c);

                c.links.Add(l1);  c.links.Add(l2); c.links.Add(l3); c.links.Add(l4);
            }
    }

    public override void Simplify()
    {
        HashSet<Cell> toCheck = new HashSet<Cell>();
        foreach (KeyValuePair<int, Node> entry in nodes)
        {
            if (!cell.ContainPoint(entry.Value.position))
            {
                foreach (Cell c in entry.Value.cells)
                    toCheck.Add(c);
            }
        }
        
        List<Cell> outsiders = new List<Cell>();
        List<Cell> onBorder = new List<Cell>();
        foreach(Cell c in toCheck)
        {
            bool outside = true;
            foreach(Node n in c.corners)
            {
                if(cell.ContainPoint(n.position))
                {
                    outside = false;
                }
            }

            if (outside)
                outsiders.Add(c);
            else
                onBorder.Add(c);
        }

        foreach (Cell c in outsiders)
            c.classified = 2;
        foreach (Cell c in onBorder)
            c.classified = 1;
    }

    private Vector2 GetNodePosition(int i, int j)
    {
        Vector2 p = new Vector2(i * cellSize.x + boundaries.min.x * Mathf.Sqrt(2) - cellSize.x, j * cellSize.y + boundaries.min.z * Mathf.Sqrt(2) - cellSize.y);
        Vector2 center = new Vector2(boundaries.center.x, boundaries.center.z);
        p = Rotate(p - center, rotation);
        return p + center + offset;
    }

    public static Vector2 Rotate(Vector2 v, float degrees)
    {
        return Quaternion.Euler(0, 0, degrees) * v;
    }
}
