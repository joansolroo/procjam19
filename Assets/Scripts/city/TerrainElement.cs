using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainElement : MonoBehaviour
{
    public Vector3 size;
   /* public Vector3 CellPosition
    {
        get { return this.transform.localPosition + size / 2; }
        set { this.transform.localPosition = value -size / 2; }
    }*/
    public Vector3 LocalPosition
    {
        get { return this.transform.localPosition; }
        set { this.transform.localPosition = value; }
    }

    public bool ValidCell(Vector3 cell)
    {
        return cell.x >= 0 && cell.x < size.x && cell.z >= 0 && cell.z < size.z;   }
}
