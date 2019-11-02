using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : TerrainElement
{
    [SerializeField] new GameObject renderer;

    public void Resize(Vector3 size)
    {
        this.transform.localScale = Vector3.one;
        renderer.transform.localScale = size;
    }
}
