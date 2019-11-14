using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainElement : MonoBehaviour
{
    private TerrainElement parent;
    public Vector3 size;
    public Bounds Bounds
    {
        get
        {
            Vector3 position = this.transform.position;
            position.y += size.y/2;
            return new Bounds(position, size);
        }
    }
    public List<Collider> colliders;
    public Renderer[] renderers;
    public Vector3Int CellPosition
     {
         get {
            if (parent != null)
                return parent.LocalToCell(this.transform.localPosition);
            else
            {
                Debug.LogWarning("not possible");
                return Vector3Int.zero;
            }
        }
        set
        {
            if (parent != null)
                this.transform.localPosition = parent.CellToLocal(value);
            else
            {
                Debug.LogWarning("not possible");
            }
        }
     }
    public Vector3 LocalPosition
    {
        get { return this.transform.localPosition; }
        set { this.transform.localPosition = value; }
    }
    public Vector3 WorldPosition
    {
        get { return this.transform.position;}
    }

    public TerrainElement Parent
    {
        get => parent;
        set
        {
            parent = value;
            if (parent != null)
            {
                this.transform.parent = parent.transform;
            }
            else
            {
                this.transform.parent = null;
            }
        }
    }

    public bool ValidCell(Vector3 cell)
    {
        return cell.x >= 0 && cell.x < size.x && cell.z >= 0 && cell.z < size.z;
    }

    public Vector3Int LocalToCell(Vector3 local)
    {
        Vector3 centered = local + size / 2;
        return new Vector3Int((int)centered.x, (int)centered.y, (int)centered.z);
    }

    public Vector3 CellToLocal(Vector3Int cell)
    {
        return cell - size / 2;
    }

    public Vector3 LocalToWorld(Vector3 local)
    {
        return transform.TransformPoint(local);
    }
    public Vector3 WorldToLocal(Vector3 world)
    {
        return transform.InverseTransformPoint(world);
    }
    public Vector3 CellToWorld(Vector3Int cell)
    {
        return LocalToWorld(CellToLocal(cell));
    }
    public Vector3Int WorldToCell(Vector3 world)
    {
        return LocalToCell(WorldToLocal(world));
    }
}
