using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class TilePrefab : Tile
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/TilePrefab")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<TilePrefab>();
    }

#endif


    public override bool StartUp(Vector3Int position, ITilemap tilemap, GameObject go)
    {
        bool result = base.StartUp(position, tilemap, go);
        if (go)
        {
            Tile3D tile3D = go.GetComponent<Tile3D>();
            if (tile3D)
            {
                tile3D.spriteRenderer.sprite = this.sprite;
            }
            go.transform.localEulerAngles = new Vector3(0, 0, 0);
            go.transform.localScale = Vector3.one;
        }
        return true;
    }
}
