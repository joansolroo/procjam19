using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : TerrainElement
{
    // attributes
    [Header("Pipeline configuration")]
    public bool generateOnstart = false;
    public bool generateWindows = true;
    public bool generateRoof = true;
    public bool generateLateral = true;
    public bool generateLights = true;

    [Header("Generation attributes")]
    public GameObject blocTemplate;
    public GameObject windowTemplate;
    public GameObject roofTemplate;
    public GameObject cityLightTemplate;
    public string personParticleManager = "PersonParticleManager";
    private int textureIndex;
    public bool sharedBuilding = false;
    [Range(0.0f, 1.0f)] public float lateralDensity = 0.5f;

    public Material[] windowMaterials;
    public Texture[] textureList;
    public Vector3[] textureWindowSizeList;
    public GameObject[] roofEquipementTemplate;
    public GameObject[] lateralEquipementTemplate;
    public GameObject[] megaStructureNameTemplate;
    public GameObject[] megaStructureEquipementTemplate;

    public List<List<Vector3>> paths = new List<List<Vector3>>();
    public List<GameObject> persons = new List<GameObject>();

    private static float roundy = 10f;
    private static float epsilon = 0.0001f;
    private List<GameObject> blocs = new List<GameObject>();

    [Header("LOD related")]
    public MeshRenderer lodwindow;
    public MeshRenderer[] lodbuilding;
    public List<LODProxy> lodlateral = new List<LODProxy>();
    public List<LODProxy> lodroof = new List<LODProxy>();
    public List<LODProxy> lodlight = new List<LODProxy>();
    public bool visibleBuilding = true;
    public bool visibleLateral = true;
    public bool visibleroof = true;
    public bool visiblelights = true;

    void Start()
    {
        if(generateOnstart)
        {
            Resize(new Vector3(transform.localScale.x, 1, transform.localScale.z));
            Init();
            GeneratePersons();
        }
    }

    public void Resize(Vector3 newsize)
    {
        this.transform.localScale = Vector3.one;
        size = newsize;
    }

    // generation functions
    public void Init(int b = 3)
    {
        textureIndex = Random.Range(0, textureList.Length);

        // process
        lodbuilding = new MeshRenderer[2 * b];
        for (int i = 0; i < b; i++)
            PlaceBlocs(i);
        if (generateWindows) GenerateWindows();
        if (generateRoof) PlaceRoofEquipement();
        if (generateLateral) PlaceLateralEquipement();
        if (generateLights) PlaceLights();
    }
    public void GeneratePersons(int personCount = 10)
    {
        // get particle pool
        ParticlePool personPool = ParticlePool.pools[personParticleManager];
        if (personPool == null)
        {
            Debug.LogError("ParticlePool of name : " + personParticleManager + ", not found");
            return;
        }

        // ask for new guys and generate them
        for (int i=0; i< personCount; i++)
        {
            GameObject go = personPool.Take();
            Person person = go.GetComponent<Person>();
            person.path = new List<Vector3>(paths[0]);
            person.ResetPerson(Random.Range(0.0017f, 0.0023f));
            go.SetActive(true);
            persons.Add(go);
        }
    }

    // Random generation process
    private void PlaceBlocs(int i)
    {
        //  place blocs
        GameObject go = Instantiate(blocTemplate);
        go.transform.parent = transform;
        go.SetActive(true);

        float sidesize = (sharedBuilding ? Random.Range(0.4f, 0.6f) : Random.Range(0.6f, 0.9f)) * size.x;
        Vector3 s = new Vector3(sidesize, (i == 0 ? 1 : Random.Range(0.5f, 0.9f)) * size.y, sidesize);
        go.transform.localScale = new Vector3((int)(s.x * roundy) / roundy, (int)(s.y * roundy + 1) / roundy, (int)(s.z * roundy) / roundy);

        Vector3 maxDisplacement = size - go.transform.localScale;
        Vector3 p = new Vector3(Random.Range(-maxDisplacement.x / 2, maxDisplacement.x / 2), go.transform.localScale.y / 2, Random.Range(-maxDisplacement.z / 2, maxDisplacement.z / 2));
        go.transform.localPosition = new Vector3((int)(p.x * roundy) / roundy, p.y, (int)(p.z * roundy) / roundy);
        go.name = this.transform.position.ToString() + "." + i;

        MeshRenderer gomr = go.GetComponent<MeshRenderer>();
        gomr.material.SetTexture("_MainTex", textureList[textureIndex]);
        gomr.material.mainTextureScale = new Vector2(2 * go.transform.localScale.x * roundy, 2 * go.transform.localScale.y * roundy);
        blocs.Add(go);

        //  place roof
        GameObject roofgo = Instantiate(roofTemplate);
        roofgo.transform.parent = transform;
        roofgo.SetActive(true);
        Vector3 s2 = new Vector3(sidesize, sidesize, sidesize);
        roofgo.transform.localScale = new Vector3((int)(s2.x * roundy) / roundy, (int)(s2.z * roundy) / roundy, 1);// (int)(s2.z * roundy) / roundy);
        roofgo.transform.localPosition = new Vector3((int)(p.x * roundy) / roundy, go.transform.localScale.y + epsilon, (int)(p.z * roundy) / roundy);
        roofgo.name = "roof";

        // maj of mesh rederer pointers
        lodbuilding[2 * i] = gomr;
        lodbuilding[2 * i + 1] = roofgo.GetComponent<MeshRenderer>();
    }
    private void GenerateWindows()
    {
        //  generate windows attributes
        List<Vector3> verticies = new List<Vector3>();
        List<Vector2> textures = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        List<int> faces = new List<int>();
        foreach (GameObject go in blocs)
        {
            //Face +z
            for (float k = 0; k < go.transform.localScale.x; k += 0.5f / roundy)
                for (float l = 0.25f / roundy; l < go.transform.localScale.y; l += 0.5f / roundy)
                {
                    Vector3 p = new Vector3(go.transform.localPosition.x - go.transform.localScale.x / 2 + k + 0.25f / roundy, l, go.transform.localPosition.z + go.transform.localScale.z / 2 + epsilon);

                    if (IsEmptySpace(blocs, p) && !IsEmptySpace(blocs, p - new Vector3(0, 0, 2 * epsilon)) && Random.Range(0f, 1f) > 0.8f)
                    {
                        if (textureIndex != 5)
                            PlaceWindow(verticies, normals, textures, faces, p, textureWindowSizeList[textureIndex], new Vector3(0, 0, 1));
                        else
                        {
                            Vector3 left = Vector3.Cross(new Vector3(0, 0, 1), Vector3.up).normalized * (textureWindowSizeList[textureIndex].x / 2 + 0.0005f);
                            PlaceWindow(verticies, normals, textures, faces, p + left, textureWindowSizeList[textureIndex], new Vector3(0, 0, 1));
                            PlaceWindow(verticies, normals, textures, faces, p - left, textureWindowSizeList[textureIndex], new Vector3(0, 0, 1));
                        }
                    }
                }

            //Face -z
            for (float k = 0; k < go.transform.localScale.x; k += 0.5f / roundy)
                for (float l = 0.25f / roundy; l < go.transform.localScale.y; l += 0.5f / roundy)
                {
                    Vector3 p = new Vector3(go.transform.localPosition.x - go.transform.localScale.x / 2 + k + 0.25f / roundy, l, go.transform.localPosition.z - go.transform.localScale.z / 2 - epsilon);

                    if (IsEmptySpace(blocs, p) && !IsEmptySpace(blocs, p + new Vector3(0, 0, 2 * epsilon)) && Random.Range(0f, 1f) > 0.8f)
                    {
                        if (textureIndex != 5)
                            PlaceWindow(verticies, normals, textures, faces, p, textureWindowSizeList[textureIndex], new Vector3(0, 0, -1));
                        else
                        {
                            Vector3 left = Vector3.Cross(new Vector3(0, 0, -1), Vector3.up).normalized * (textureWindowSizeList[textureIndex].x / 2 + 0.0005f);
                            PlaceWindow(verticies, normals, textures, faces, p + left, textureWindowSizeList[textureIndex], new Vector3(0, 0, -1));
                            PlaceWindow(verticies, normals, textures, faces, p - left, textureWindowSizeList[textureIndex], new Vector3(0, 0, -1));
                        }
                    }
                }

            //Face +x
            for (float k = 0; k < go.transform.localScale.z; k += 0.5f / roundy)
                for (float l = 0.25f / roundy; l < go.transform.localScale.y; l += 0.5f / roundy)
                {
                    Vector3 p = new Vector3(go.transform.localPosition.x + go.transform.localScale.x / 2 + epsilon, l, go.transform.localPosition.z - go.transform.localScale.z / 2 + k + 0.25f / roundy);

                    if (IsEmptySpace(blocs, p) && !IsEmptySpace(blocs, p - new Vector3(2 * epsilon, 0, 0)) && Random.Range(0f, 1f) > 0.8f)
                    {
                        if (textureIndex != 5)
                            PlaceWindow(verticies, normals, textures, faces, p, textureWindowSizeList[textureIndex], new Vector3(1, 0, 0));
                        else
                        {
                            Vector3 left = Vector3.Cross(new Vector3(1, 0, 0), Vector3.up).normalized * (textureWindowSizeList[textureIndex].x / 2 + 0.0005f);
                            PlaceWindow(verticies, normals, textures, faces, p + left, textureWindowSizeList[textureIndex], new Vector3(1, 0, 0));
                            PlaceWindow(verticies, normals, textures, faces, p - left, textureWindowSizeList[textureIndex], new Vector3(1, 0, 0));
                        }
                    }
                }

            //Face -x
            for (float k = 0; k < go.transform.localScale.z; k += 0.5f / roundy)
                for (float l = 0.25f / roundy; l < go.transform.localScale.y; l += 0.5f / roundy)
                {
                    Vector3 p = new Vector3(go.transform.localPosition.x - go.transform.localScale.x / 2 - epsilon, l, go.transform.localPosition.z - go.transform.localScale.z / 2 + k + 0.25f / roundy);

                    if (IsEmptySpace(blocs, p) && !IsEmptySpace(blocs, p + new Vector3(2 * epsilon, 0, 0)) && Random.Range(0f, 1f) > 0.8f)
                    {
                        if (textureIndex != 5)
                            PlaceWindow(verticies, normals, textures, faces, p, textureWindowSizeList[textureIndex], new Vector3(-1, 0, 0));
                        else
                        {
                            Vector3 left = Vector3.Cross(new Vector3(-1, 0, 0), Vector3.up).normalized * (textureWindowSizeList[textureIndex].x / 2 + 0.0005f);
                            PlaceWindow(verticies, normals, textures, faces, p + left, textureWindowSizeList[textureIndex], new Vector3(-1, 0, 0));
                            PlaceWindow(verticies, normals, textures, faces, p - left, textureWindowSizeList[textureIndex], new Vector3(-1, 0, 0));
                        }
                    }
                }
        }

        //  assign windows value to mesh
        GameObject windows = new GameObject();
        windows.name = "windows";
        windows.transform.parent = transform;
        windows.transform.localPosition = Vector3.zero;
        windows.transform.localScale = Vector3.one;
        lodwindow = windows.AddComponent<MeshRenderer>();
        lodwindow.materials = windowMaterials;
        MeshFilter mf = windows.AddComponent<MeshFilter>();
        mf.mesh = new Mesh();
        Mesh mesh = mf.mesh;
        mesh.vertices = verticies.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = textures.ToArray();

        // split all windows in multiple group for different lightning
        mesh.subMeshCount = windowMaterials.Length;
        List<int>[] submeshes = new List<int>[windowMaterials.Length];
        for (int i = 0; i < submeshes.Length; i++)
            submeshes[i] = new List<int>();
        for (int i = 0; i < faces.Count; i += 6)
        {
            int subIndex = Random.Range(0, submeshes.Length);
            submeshes[subIndex].Add(faces[i]);
            submeshes[subIndex].Add(faces[i + 1]);
            submeshes[subIndex].Add(faces[i + 2]);
            submeshes[subIndex].Add(faces[i + 3]);
            submeshes[subIndex].Add(faces[i + 4]);
            submeshes[subIndex].Add(faces[i + 5]);
        }
        for (int i = 0; i < submeshes.Length; i++)
            mesh.SetTriangles(submeshes[i], i);
    }
    private void PlaceRoofEquipement()
    {
        // place randoom roof equipement
        if (sharedBuilding && Random.Range(0f, 1f) > 0.5f)
        {
            int nameIndex = Random.Range(0, megaStructureNameTemplate.Length);
            Vector2[] pos = { new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1) };
            int[] ori = { 0, -90, 180, 90 };
            for (int i = 0; i < 4; i++)
            {
                GameObject name = Instantiate(megaStructureNameTemplate[nameIndex]);
                name.transform.parent = transform;
                name.name = "LightningName " + i.ToString();
                name.transform.localPosition = blocs[0].transform.localPosition + new Vector3(pos[i].x * blocs[0].transform.localScale.x / 2, blocs[0].transform.localScale.y / 2, pos[i].y * blocs[0].transform.localScale.z / 2);
                name.transform.localScale = 0.15f * Vector3.one;
                name.transform.localEulerAngles = new Vector3(0, ori[i], 0);

                lodroof.Add(name.GetComponent<LODProxy>());
            }

            if (nameIndex < megaStructureEquipementTemplate.Length)
            {
                GameObject equipement = Instantiate(megaStructureEquipementTemplate[nameIndex]);
                equipement.transform.parent = transform;
                equipement.name = "roofEquipement";
                equipement.transform.localPosition = blocs[0].transform.localPosition + new Vector3(0, blocs[0].transform.localScale.y / 2, 0);
                equipement.transform.localEulerAngles = new Vector3(0, Random.Range(0, 2) == 1 ? 90 : 0, 0);
                equipement.transform.localScale = Vector3.one;

                lodroof.Add(equipement.GetComponent<LODProxy>());
            }
        }
        else if (Random.Range(0f, 1f) > 0.2f)
        {
            int equipementIndex = Random.Range(0, roofEquipementTemplate.Length);
            GameObject equipement = Instantiate(roofEquipementTemplate[equipementIndex]);
            equipement.transform.parent = transform;
            equipement.name = "roofEquipement";
            equipement.transform.localPosition = blocs[0].transform.localPosition + new Vector3(0, blocs[0].transform.localScale.y / 2, 0);
            equipement.transform.localEulerAngles = new Vector3(0, Random.Range(0, 2) == 1 ? 90 : 0, 0);
            equipement.transform.localScale = Vector3.one;
            
            lodroof.Add(equipement.GetComponent<LODProxy>());
        }
    }
    private void PlaceLateralEquipement()
    {
        float dp = 0.02f;
        float offset = 0.15f;

        if (sharedBuilding && size.y < 5 * offset)
            return;

        List<Vector3> availablePositions = new List<Vector3>();
        List<Vector3> ori = new List<Vector3>();
        for (int i = 0; i < blocs.Count; i++) 
        {
            GameObject b = blocs[i];
            Vector3 p = b.transform.localPosition + new Vector3(b.transform.localScale.x / 2, -b.transform.localScale.y / 2 + epsilon + offset * (i + 1), b.transform.localScale.z / 2);
            if (Random.Range(0f, 1f) < lateralDensity && IsEmptySpace(blocs, p + new Vector3(epsilon, 0, 0)) && IsEmptySpace(blocs, p + new Vector3(0, 0, epsilon)))
            {
                bool r = Random.Range(0, 2) == 1;
                availablePositions.Add(p + (r ? new Vector3(dp, 0, 0) : new Vector3(0, 0, dp)));
                ori.Add((r ? new Vector3(0, 0, 0) : new Vector3(0, 90, 0)));
            }

            p = b.transform.localPosition + new Vector3(b.transform.localScale.x / 2, -b.transform.localScale.y / 2 + epsilon + offset * (i + 1), -b.transform.localScale.z / 2);
            if (Random.Range(0f, 1f) < lateralDensity && IsEmptySpace(blocs, p + new Vector3(epsilon, 0, 0)) && IsEmptySpace(blocs, p + new Vector3(0, 0, -epsilon)))
            {
                bool r = Random.Range(0, 2) == 1;
                availablePositions.Add(p + (r ? new Vector3(dp, 0, 0) : new Vector3(0, 0, -dp)));
                ori.Add((r ? new Vector3(0, 0, 0) : new Vector3(0, 90, 0)));
            }

            p = b.transform.localPosition + new Vector3(-b.transform.localScale.x / 2, -b.transform.localScale.y / 2 + epsilon + offset * (i + 1), b.transform.localScale.z / 2);
            if (Random.Range(0f, 1f) < lateralDensity && IsEmptySpace(blocs, p + new Vector3(-epsilon, 0, 0)) && IsEmptySpace(blocs, p + new Vector3(0, 0, epsilon)))
            {
                bool r = Random.Range(0, 2) == 1;
                availablePositions.Add(p + (r ? new Vector3(-dp, 0, 0) : new Vector3(0, 0, dp)));
                ori.Add((r ? new Vector3(0, 0, 0) : new Vector3(0, 90, 0)));
            }

            p = b.transform.localPosition + new Vector3(-b.transform.localScale.x / 2, -b.transform.localScale.y / 2 + epsilon + offset * (i + 1), -b.transform.localScale.z / 2);
            if (Random.Range(0f, 1f) < lateralDensity && IsEmptySpace(blocs, p + new Vector3(-epsilon, 0, 0)) && IsEmptySpace(blocs, p + new Vector3(0, 0, -epsilon))) 
            {
                bool r = Random.Range(0, 2) == 1;
                availablePositions.Add(p + (r ? new Vector3(-dp, 0, 0) : new Vector3(0, 0, -dp)));
                ori.Add((r ? new Vector3(0, 0, 0) : new Vector3(0, 90, 0)));
            }
        }
        for (int i = 0; i < availablePositions.Count; i++)
        {
            Vector3 p = availablePositions[i];
            int equipementIndex = Random.Range(0, lateralEquipementTemplate.Length);
            GameObject equipement = Instantiate(lateralEquipementTemplate[equipementIndex]);
            equipement.transform.parent = transform;
            equipement.name = "lateralEquipement";
            equipement.transform.localPosition = p;
            equipement.transform.localEulerAngles = ori[i];
            equipement.transform.localScale = Vector3.one;

            lodlateral.Add(equipement.GetComponent<LODProxy>());
        }
    }
    private void PlaceLights()
    {
        float d = 0.5f * ((int)(size.x) + 1) - 0.1f;
        Vector3[] pos = { new Vector3(-d, 0, -d / 2), new Vector3(-d, 0, d / 2), new Vector3(d, 0, -d / 2), new Vector3(d, 0, d / 2), new Vector3(-d / 2, 0, -d), new Vector3(d / 2, 0, -d), new Vector3(-d / 2, 0, d), new Vector3(d / 2, 0, d) };
        int[] ori = { 180, 180, 0, 0, 90, 90, -90, -90 };

        for(int i=0; i< pos.Length; i++)
        {
            GameObject go = Instantiate(cityLightTemplate);
            go.transform.parent = transform;
            go.name = "light";
            go.transform.localPosition = pos[i];
            go.transform.localScale = 0.8f * Vector3.one;
            go.transform.localEulerAngles = new Vector3(0,ori[i], 0);

            lodlight.Add(go.GetComponent<LODProxy>());
        }
    }
    public void GeneratePaths()
    {
        List<Vector3> footPath = new List<Vector3>();
        float d = 0.5f * ((int)(size.x) + 1) - 0.12f;// 0.38f;

        footPath.Add(transform.TransformPoint(new Vector3(-d, 0, -d)));
        footPath.Add(transform.TransformPoint(new Vector3(-d, 0,  d)));
        footPath.Add(transform.TransformPoint(new Vector3( d, 0,  d)));
        footPath.Add(transform.TransformPoint(new Vector3( d, 0, -d)));
        paths.Add(footPath);
    }

    // helpers
    private void OnDrawGizmos2()
    {
        Gizmos.color = Color.green;
        foreach (List<Vector3> path in paths)
        {
            for (int i = 0; i < path.Count; i++)
            {
                Gizmos.DrawLine(path[i], path[(i + 1)% path.Count]);
            }
        }
    }
    private bool IsEmptySpace(List<GameObject> blocs, Vector3 p)
    {
        foreach (GameObject g2 in blocs)
        {
            if (p.x < g2.transform.localPosition.x - g2.transform.localScale.x / 2) continue;
            else if (p.x > g2.transform.localPosition.x + g2.transform.localScale.x / 2) continue;
            else if (p.y < g2.transform.localPosition.y - g2.transform.localScale.y / 2) continue;
            else if (p.y > g2.transform.localPosition.y + g2.transform.localScale.y / 2) continue;
            else if (p.z < g2.transform.localPosition.z - g2.transform.localScale.z / 2) continue;
            else if (p.z > g2.transform.localPosition.z + g2.transform.localScale.z / 2) continue;

            return false;
        }
        return true;
    }
    private void PlaceWindow(List<Vector3> verticies, List<Vector3> normals, List<Vector2> textures, List<int> faces, Vector3 p, Vector3 s, Vector3 n)
    {
        Vector3 left = Vector3.Cross(n, Vector3.up).normalized;
        verticies.Add(p - 0.5f * s.x * left - 0.5f * s.y * Vector3.up);
        verticies.Add(p + 0.5f * s.x * left - 0.5f * s.y * Vector3.up);
        verticies.Add(p - 0.5f * s.x * left + 0.5f * s.y * Vector3.up);
        verticies.Add(p + 0.5f * s.x * left + 0.5f * s.y * Vector3.up);
        normals.Add(n);
        normals.Add(n);
        normals.Add(n);
        normals.Add(n);
        textures.Add(new Vector2(0, 0));
        textures.Add(new Vector2(1, 0));
        textures.Add(new Vector2(0, 1));
        textures.Add(new Vector2(1, 1));
        faces.Add(verticies.Count - 4);
        faces.Add(verticies.Count - 2);
        faces.Add(verticies.Count - 3);
        faces.Add(verticies.Count - 2);
        faces.Add(verticies.Count - 1);
        faces.Add(verticies.Count - 3);
    }
    
}
