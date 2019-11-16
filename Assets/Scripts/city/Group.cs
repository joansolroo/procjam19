using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group : TerrainElement
{
    public Vector3 center;
    public bool containMegaStructure;
    public Block[,] blocks;

    private void OnDrawGizmos()
    {
        Gizmos.color = OcclusionCulling.ContainsCamera(Bounds)? Color.yellow: visible ? Color.green : Color.red;
        /*if (containMegaStructure)
        {
            Gizmos.DrawWireCube(transform.position + center + new Vector3(0, size.y/2, 0), new Vector3(2* size.x, size.y, 2* size.z)/2);
        }
        else*/
        {
            Gizmos.DrawWireCube(transform.position + center + new Vector3(0, size.y/2, 0), size/2);
        }
    }
}
