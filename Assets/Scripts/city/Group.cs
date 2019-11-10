using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group : MonoBehaviour
{
    public Vector3 center;
    public Vector3 size;
    public bool containMegaStructure;
    public Block[,] blocks;
    public bool visible = true;

    private void OnDrawGizmos2()
    {
        if(containMegaStructure)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + center + new Vector3(0, size.y/2, 0), new Vector3(2* size.x, size.y, 2* size.z));
        }
        else
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(transform.position + center + new Vector3(0, size.y/2, 0), size);
        }
    }
}
