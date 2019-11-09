using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LODProxy : MonoBehaviour
{
    private MeshRenderer[] meshrenderers;
    private LODProxy[] proxies;
    void Start()
    {
        meshrenderers = GetComponentsInChildren<MeshRenderer>();
        proxies = GetComponentsInChildren<LODProxy>();
    }

    // Update is called once per frame
    public void SetState(bool enable)
    {
        foreach (MeshRenderer mr in meshrenderers)
            mr.enabled = enable;
        foreach (LODProxy p in proxies)
            p.SetState(enable);
    }
}
