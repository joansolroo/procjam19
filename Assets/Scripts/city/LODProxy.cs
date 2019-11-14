using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODProxy : MonoBehaviour
{
    private MeshRenderer[] meshrenderers;
    private LODProxy[] proxies;

    void Awake()
    {
        meshrenderers = GetComponentsInChildren<MeshRenderer>();
        //proxies = GetComponentsInChildren<LODProxy>();
    }

    // Update is called once per frame
    public void SetState(bool enable)
    {
        foreach (MeshRenderer mr in meshrenderers)
        {
            mr.enabled = enable && OcclusionCulling.IsVisibleAABB(mr.bounds);
        }
        //foreach (LODProxy p in proxies)
        //    p.SetState(enable);
    }

    private void OnDrawGizmos()
    {
        foreach(MeshRenderer renderer in meshrenderers)
        {
            Gizmos.color = renderer.enabled ? Color.green : Color.red;
            Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
        }
    }

}
