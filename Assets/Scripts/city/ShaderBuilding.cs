using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderBuilding : MonoBehaviour
{
    private MeshRenderer mr;
    private MeshFilter mf;

    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        mf = GetComponent<MeshFilter>();
        Generate();
    }
    
    public void Generate()
    {
        //  init
        List<Vector3> verticies = new List<Vector3>();
        List<Vector2> textures = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        List<int> faces = new List<int>();

        //  push a face
        verticies.Add(new Vector3(-1, 1, 0));
        verticies.Add(new Vector3( 1, 1, 0));
        verticies.Add(new Vector3( 1, 0, 0));
        verticies.Add(new Vector3(-1, 0, 0));

        textures.Add(new Vector2(0, 0));
        textures.Add(new Vector2(20, 0));
        textures.Add(new Vector2(20, 10));
        textures.Add(new Vector2(0, 10));

        normals.Add(new Vector3(0, 0, -1));
        normals.Add(new Vector3(0, 0, -1));
        normals.Add(new Vector3(0, 0, -1));
        normals.Add(new Vector3(0, 0, -1));

        faces.Add(0); faces.Add(1); faces.Add(2);
        faces.Add(0); faces.Add(2); faces.Add(3);

        //  assign windows value to mesh
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 1;
        mesh.vertices = verticies.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = textures.ToArray();
        mesh.triangles = faces.ToArray();
        mf.mesh = mesh;
    }
    private void OnValidate()
    {
        mr = GetComponent<MeshRenderer>();
        mf = GetComponent<MeshFilter>();
        Generate();
    }
}
