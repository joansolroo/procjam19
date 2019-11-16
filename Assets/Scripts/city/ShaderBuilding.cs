using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderBuilding : TerrainElement
{
    private MeshRenderer mr;
    private MeshFilter mf;

    class MeshConstruct{
        public List<Vector3> verticies;
        public List<Vector3> normals;
        public List<Vector2> textures;
        public List<int> faces;
    }
    private MeshConstruct temp;

    void Start()
    {
        mr = GetComponent<MeshRenderer>();
        mf = GetComponent<MeshFilter>();
        Generate();
    }
    
    public void Generate()
    {
        //  init
        temp = new MeshConstruct();
        temp.verticies = new List<Vector3>();
        temp.normals = new List<Vector3>();
        temp.textures = new List<Vector2>();
        temp.faces = new List<int>();
        BoxCollider box = GetComponent<BoxCollider>();

        //  push a face
        GenerateBloc(new Vector3(0,0,0), size, box);

        //  assign windows value to mesh
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 1;
        mesh.SetVertices(temp.verticies);
        mesh.SetNormals(temp.normals);
        mesh.SetUVs(0, temp.textures);
        mesh.triangles = temp.faces.ToArray();
        //mesh.RecalculateNormals();
        mf.mesh = mesh;
        temp = null;
    }
    private void OnValidate()
    {
        mr = GetComponent<MeshRenderer>();
        mf = GetComponent<MeshFilter>();
        Generate();
    }

    // helpers
    private void GenerateBloc(Vector3 blocCenter, Vector3 blocSize, BoxCollider box)
    {
        // place collider
        if (box == null)
        {
            box = gameObject.AddComponent<BoxCollider>();
        }
        box.size = blocSize;
        box.center = blocCenter + new Vector3(0, blocSize.y / 2, 0);

        //  push a face
        Vector3 fc = blocCenter + new Vector3(0, blocSize.y / 2, blocSize.z / 2);

        temp.verticies.Add(fc + new Vector3( blocSize.x / 2, blocSize.y / 2, 0));
        temp.verticies.Add(fc + new Vector3(-blocSize.x / 2, blocSize.y / 2, 0));
        temp.verticies.Add(fc + new Vector3(-blocSize.x / 2, -blocSize.y / 2, 0));
        temp.verticies.Add(fc + new Vector3( blocSize.x / 2, -blocSize.y / 2, 0));

        temp.textures.Add(new Vector2(0, 0));
        temp.textures.Add(new Vector2(20 * blocSize.x, 0));
        temp.textures.Add(new Vector2(20 * blocSize.x, 20 * blocSize.y));
        temp.textures.Add(new Vector2(0, 20 * blocSize.y));

        temp.normals.Add(new Vector3(0, 0, 1));
        temp.normals.Add(new Vector3(0, 0, 1));
        temp.normals.Add(new Vector3(0, 0, 1));
        temp.normals.Add(new Vector3(0, 0, 1));

        temp.faces.Add(0); temp.faces.Add(1); temp.faces.Add(2);
        temp.faces.Add(0); temp.faces.Add(2); temp.faces.Add(3);

        //  push a face
        fc = blocCenter + new Vector3(0, blocSize.y / 2, -blocSize.z / 2);

        temp.verticies.Add(fc + new Vector3(-blocSize.x / 2, blocSize.y / 2, 0));
        temp.verticies.Add(fc + new Vector3(blocSize.x / 2, blocSize.y / 2, 0));
        temp.verticies.Add(fc + new Vector3(blocSize.x / 2, -blocSize.y / 2, 0));
        temp.verticies.Add(fc + new Vector3(-blocSize.x / 2, -blocSize.y / 2, 0));

        temp.textures.Add(new Vector2(0, 0));
        temp.textures.Add(new Vector2(20 * blocSize.x, 0));
        temp.textures.Add(new Vector2(20 * blocSize.x, 20 * blocSize.y));
        temp.textures.Add(new Vector2(0, 20 * blocSize.y));

        temp.normals.Add(new Vector3(0, 0, -1));
        temp.normals.Add(new Vector3(0, 0, -1));
        temp.normals.Add(new Vector3(0, 0, -1));
        temp.normals.Add(new Vector3(0, 0, -1));

        temp.faces.Add(4); temp.faces.Add(5); temp.faces.Add(6);
        temp.faces.Add(4); temp.faces.Add(6); temp.faces.Add(7);

        //  push a face
        fc = blocCenter + new Vector3(-blocSize.x / 2, blocSize.y / 2, 0);

        temp.verticies.Add(fc + new Vector3(0,  blocSize.y / 2,  blocSize.z / 2));
        temp.verticies.Add(fc + new Vector3(0,  blocSize.y / 2, -blocSize.z / 2));
        temp.verticies.Add(fc + new Vector3(0, -blocSize.y / 2, -blocSize.z / 2));
        temp.verticies.Add(fc + new Vector3(0, -blocSize.y / 2,  blocSize.z / 2));

        temp.textures.Add(new Vector2(0, 0));
        temp.textures.Add(new Vector2(20 * blocSize.x, 0));
        temp.textures.Add(new Vector2(20 * blocSize.x, 20 * blocSize.y));
        temp.textures.Add(new Vector2(0, 20 * blocSize.y));

        temp.normals.Add(new Vector3(-1, 0, 0));
        temp.normals.Add(new Vector3(-1, 0, 0));
        temp.normals.Add(new Vector3(-1, 0, 0));
        temp.normals.Add(new Vector3(-1, 0, 0));

        temp.faces.Add(8); temp.faces.Add(9);  temp.faces.Add(10);
        temp.faces.Add(8); temp.faces.Add(10); temp.faces.Add(11);

        //  push a face
        fc = blocCenter + new Vector3(blocSize.x / 2, blocSize.y / 2, 0);

        temp.verticies.Add(fc + new Vector3(0,  blocSize.y / 2, -blocSize.z / 2));
        temp.verticies.Add(fc + new Vector3(0,  blocSize.y / 2,  blocSize.z / 2));
        temp.verticies.Add(fc + new Vector3(0, -blocSize.y / 2,  blocSize.z / 2));
        temp.verticies.Add(fc + new Vector3(0, -blocSize.y / 2, -blocSize.z / 2));

        temp.textures.Add(new Vector2(0, 0));
        temp.textures.Add(new Vector2(20 * blocSize.x, 0));
        temp.textures.Add(new Vector2(20 * blocSize.x, 20 * blocSize.y));
        temp.textures.Add(new Vector2(0, 20 * blocSize.y));

        temp.normals.Add(new Vector3(1, 0, 0));
        temp.normals.Add(new Vector3(1, 0, 0));
        temp.normals.Add(new Vector3(1, 0, 0));
        temp.normals.Add(new Vector3(1, 0, 0));

        temp.faces.Add(12); temp.faces.Add(13); temp.faces.Add(14);
        temp.faces.Add(12); temp.faces.Add(14); temp.faces.Add(15);

        //  push a face
        fc = blocCenter + new Vector3(0, blocSize.y, 0);

        temp.verticies.Add(fc + new Vector3( blocSize.x / 2, 0,  blocSize.z / 2));
        temp.verticies.Add(fc + new Vector3( blocSize.x / 2, 0, -blocSize.z / 2));
        temp.verticies.Add(fc + new Vector3(-blocSize.x / 2, 0, -blocSize.z / 2));
        temp.verticies.Add(fc + new Vector3(-blocSize.x / 2, 0,  blocSize.z / 2));

        temp.textures.Add(new Vector2(0, 0));
        temp.textures.Add(new Vector2(0, 0));
        temp.textures.Add(new Vector2(0, 0));
        temp.textures.Add(new Vector2(0, 0));

        temp.normals.Add(new Vector3(0, 1, 0));
        temp.normals.Add(new Vector3(0, 1, 0));
        temp.normals.Add(new Vector3(0, 1, 0));
        temp.normals.Add(new Vector3(0, 1, 0));

        temp.faces.Add(16); temp.faces.Add(17); temp.faces.Add(18);
        temp.faces.Add(16); temp.faces.Add(18); temp.faces.Add(19);
    }
}
