﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour
{
    public int seed = -1;
    public City city;
    public AutoMergeChildMeshes streetTexture;
    public BuildingFactory buildingactory;
    public CarFactory carFactory;
    public PersonFactory personFactory;
    public int groupSize = 2;
    public bool enableBlockMerge = false; 
    public float blockMergeRatio = 0.05f;
    public bool enableAvenues = false;
    // Start is called before the first frame update
    void Start()
    {
        if (seed < 0)
            seed = Random.Range(0, 4000000);
        Random.InitState(seed);
        buildingactory.generateMegastructure = enableBlockMerge;
        buildingactory.Generate();
        carFactory.Generate();
        personFactory.Generate();
        Generate();
    }

    void Generate()
    {
        GenerateBlocks();
        if(enableAvenues) GenerateAvenue();
        GenerateBuildings();
        GenerateRoads();
        Regroup();
    }

    //  generation pipeline
    private void GenerateBlocks()
    {
        city.blocks = new Block[(int)city.size.x, (int)city.size.z];

        Transform blockContainer = new GameObject().transform;
        blockContainer.gameObject.isStatic = true;
        blockContainer.parent = city.transform;
        blockContainer.localPosition = Vector3.zero;
        blockContainer.localScale = Vector3.one;
        blockContainer.name = "blocks";

        // create the basic tiles: blocks
        for (int x = 0; x < city.size.x; ++x)
        {
            for (int z = 0; z < city.size.z; ++z)
            {
                // SPACE SORTING: BLOCK
                GameObject b = new GameObject();
                b.gameObject.isStatic = true;
                Block block = b.AddComponent<Block>();
                city.blocks[x, z] = block;
                block.transform.parent = blockContainer.transform;
                block.transform.localScale = Vector3.one;
                block.LocalPosition = new Vector3(x, 0, z) - city.size / 2;
                block.region = city.regions[0];
                block.building = null;

                GameObject s = Instantiate(city.streetTexture);
                s.transform.parent = streetTexture.transform;
                s.transform.localPosition = new Vector3(x, 0.001f, z) - city.size / 2;
                s.transform.localScale = Vector3.one;
                s.name = "streetPatch";
            }
        }
        streetTexture.Merge();


        // divide in regions
        foreach (Region region in city.regions)
        {
            // SPACE SORTING: REGION
            region.transform.parent = city.transform;
            region.transform.localScale = Vector3.one;
            region.LocalPosition = new Vector3(Random.Range(0, city.size.x), 0, Random.Range(0, city.size.z)) - city.size / 2;

            for (int dx = 0; dx < region.size.x; ++dx)
            {
                for (int dz = 0; dz < region.size.z; ++dz)
                {
                    Vector3 cell = region.LocalPosition + new Vector3(dx - region.size.x / 2, 0, dz - region.size.z / 2) + city.size / 2;
                    if (city.ValidCell(cell))
                    {
                        city.blocks[(int)(cell.x), (int)(cell.z)].region = region;
                    }
                }
            }
        }
    }
    private void GenerateAvenue()
    {
        foreach (Region region in city.regions)
        {
            if (region.richness > 0.5f)
            {
                Vector3 nexus = new Vector3((int)region.LocalPosition.x, (int)region.LocalPosition.y, (int)region.LocalPosition.z);
                for (int i = 0; i < city.size.x; i++)
                {

                    MarkAsAvenue(nexus + new Vector3(i, 0, 0) + city.size / 2);
                    MarkAsAvenue(nexus - new Vector3(i, 0, 0) + city.size / 2);
                    MarkAsAvenue(nexus + new Vector3(0, 0, i) + city.size / 2);
                    MarkAsAvenue(nexus - new Vector3(0, 0, i) + city.size / 2);
                }
            }
        }
    }
    private void GenerateBuildings()
    {
        for (int x = 0; x < city.size.x; ++x)
        {
            for (int z = 0; z < city.size.z; ++z)
            {
                // smooth the information
                Block block = city.blocks[x, z];
                int radius = 5;
                block.richness = 0;
                for (int dx = 0; dx < radius; ++dx)
                {
                    for (int dz = 0; dz < radius; ++dz)
                    {
                        Vector3 cell = new Vector3(x, 0, z) + new Vector3(dx - radius / 2, 0, dz - radius / 2);
                        if (city.ValidCell(cell) && city.blocks[(int)(cell.x), (int)(cell.z)].region != null)
                        {
                            block.richness += city.blocks[(int)(cell.x), (int)(cell.z)].region.richness;
                        }
                    }
                }
                block.richness /= radius * radius;

                // BUILDING FACTORY
                if (enableBlockMerge && Random.value < blockMergeRatio && AvailableForMergeBlock(x, z))
                {
                    Building building = buildingactory.GetMegaBuilding(Random.Range(0.9f, 1.5f) * block.richness * 1.1f);
                    building.transform.parent = block.transform;
                    building.LocalPosition = new Vector3(0.5f, 0, 0.5f);
                    building.transform.localScale = Vector3.one;
                    building.transform.localEulerAngles = new Vector3(0, -90 * Random.Range(-1, 2), 0);
                    building.gameObject.SetActive(true);
                    building.GeneratePaths();

                    block.superBlock = block;
                    city.blocks[x + 1, z].superBlock = block;
                    city.blocks[x, z + 1].superBlock = block;
                    city.blocks[x + 1, z + 1].superBlock = block;

                    block.building = building;
                    city.blocks[x + 1, z].building = building;
                    city.blocks[x, z + 1].building = building;
                    city.blocks[x + 1, z + 1].building = building;
                    block.building.gameObject.isStatic = true;
                }
                else if (block.building == null && !block.isAvenue)
                {
                    SimpleBuildingGenerate(block);
                }

                block.size = new Vector3(1, block.building ? block.building.localSize.y : 0, 1);
            }
        }
    }
    private void GenerateRoads()
    {
        // ROADS
        int height = 5;
        city.carRoads = new GraphSparse<Vector3>();
        city.carRoads.nodes = new List<GraphSparse<Vector3>.Node>();
        city.carNodes = new GraphSparse<Vector3>.Node[(int)city.size.x + 1, height, (int)city.size.z + 1];

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x <= city.size.x; ++x)
            {
                for (int z = 0; z <= city.size.z; ++z)
                {
                    Block block = city.blocks[(int)Mathf.Clamp(x, 0, city.size.x - 1), (int)Mathf.Clamp(z, 0, city.size.z - 1)];


                    bool ValidRoadNode = true;
                    if (block.superBlock != null)
                    {
                        int x2 = x - 1;
                        int z2 = z - 1;
                        if (x2 >= 0 && x2 < city.size.x
                            && z2 >= 0 && z2 < city.size.z)
                        {
                            Block block2 = city.blocks[x2, z2];

                            if (block.superBlock == block2.superBlock)
                            {
                                ValidRoadNode = false;
                            }
                        }
                    }
                    if (ValidRoadNode)
                    {

                        // create nodes, in a grid
                        int id = (int)(x * (city.size.z + 1) + z + y * (city.size.x + 1) * (city.size.z + 1));
                        GraphSparse<Vector3>.Node node = new GraphSparse<Vector3>.Node();
                        node.id = id;
                        Vector3Int cell = new Vector3Int(x, 0, z);
                        node.data = city.CellToWorld(cell) + new Vector3(-50 / 2, y * (50 / 2) * (block.richness + 0.5f) / 2 + 5, -50 / 2);
                        node.links = new List<GraphSparse<Vector3>.Link>();

                        city.carRoads.nodes.Add(node);
                        city.carNodes[cell.x, y, cell.z] = node;
                        // Create links, predictively (assuming regular grid)

                    }
                    else
                    {
                        city.carRoads.nodes.Add(null);
                        city.carNodes[x, y, z] = null;
                    }
                }
            }
        }
        // TODO optimize: very bad loop
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x <= city.size.x; ++x)
            {
                for (int z = 0; z <= city.size.z; ++z)
                {
                    for (int dx = -1; dx <= 1; ++dx)
                    {
                        int x2 = x + dx;
                        if (x2 >= 0 && x2 <= city.size.x)
                        {
                            for (int dz = -1; dz <= 1; ++dz)
                            {
                                int z2 = z + dz;
                                if (z2 >= 0 && z2 <= city.size.z)
                                {
                                    for (int dy = -1; dy <= 1; ++dy)
                                    {
                                        GraphSparse<Vector3>.Node node = city.carNodes[x, y, z];
                                        if (node != null)
                                        {
                                            Block block = city.blocks[(int)Mathf.Clamp(x, 0, city.size.x - 1), (int)Mathf.Clamp(z, 0, city.size.z - 1)];

                                            int y2 = y + dy;
                                            if (y2 >= 0 && y2 < height)
                                            {
                                                // TODO optimize: this test can be improved by simply not doing a loop
                                                if ((dx != 0 || dz != 0 || dy != 0) && ((Mathf.Abs(dy) + Mathf.Abs(dx) + Mathf.Abs(dz)) < 2))
                                                {
                                                    GraphSparse<Vector3>.Node node2 = city.carNodes[x2, y2, z2];
                                                    if (node2 != null)
                                                    {
                                                        int id2 = (int)(x2 * (city.size.z + 1) + z2 + y2 * (city.size.x + 1) * (city.size.z + 1));

                                                        GraphSparse<Vector3>.Link link = new GraphSparse<Vector3>.Link
                                                        {
                                                            from = node.id,
                                                            to = id2,
                                                            probability = 1
                                                        };
                                                        node.links.Add(link);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    private void Regroup()
    {
        city.groups = new List<Group>();

        for (int x = 0; x < city.size.x; x += groupSize)
        {
            for (int z = 0; z < city.size.z; z += groupSize)
            {
                GameObject group = new GameObject();
                group.gameObject.isStatic = true;
                group.name = "group " + x.ToString() + " " + z.ToString();
                group.transform.parent = transform;
                group.transform.localScale = Vector3.one;
                group.transform.localPosition = new Vector3(x, 0, z) - city.size / 2 + 0.5f * new Vector3(groupSize, 0, groupSize) - 0.5f * new Vector3(1, 0, 1);
                Group g = group.AddComponent<Group>();
                g.blocks = new Block[groupSize, groupSize];

                Vector3 c = Vector3.zero;
                Vector3 s = new Vector3(groupSize, 0, groupSize);
                bool containMegaStructure = false;
                for (int k = 0; k < groupSize; k++)
                {
                    for (int l = 0; l < groupSize; l++)
                    {
                        city.blocks[x + k, z + l].gameObject.transform.parent = group.transform;
                        c += city.blocks[x + k, z + l].gameObject.transform.localPosition;
                        s.y = Mathf.Max(s.y, city.blocks[x + k, z + l].size.y);
                        containMegaStructure |= (city.blocks[x + k, z + l].building != null && city.blocks[x + k, z + l].building.sharedBuilding);
                        g.blocks[k, l] = city.blocks[x + k, z + l];
                    }
                }
                g.center = Vector3.zero;
                g.size = s * city.transform.localScale.x;
                g.containMegaStructure = containMegaStructure;
                city.groups.Add(g);
            }
        }
    }

    // helpers
    private void OnDrawGizmos2()
    {
        Gizmos.DrawWireCube(city.transform.position, city.size);
        /*if (city.regions != null)
        {
            foreach (Region region in city.regions)
            {
                Gizmos.color = region.color;
                Gizmos.DrawWireCube(city.transform.TransformPoint(region.LocalPosition), region.size * this.transform.lossyScale.x);
                Gizmos.DrawSphere(city.transform.TransformPoint(region.LocalPosition), 10);
            }
        }*/
        if (city.blocks != null)
        {
            foreach (Block block in city.blocks)
            {
                if (block.region != null)
                {
                    Gizmos.color = block.region.color;
                    Vector3 s = city.transform.lossyScale;
                    s.y = 1;
                    Gizmos.DrawCube(block.transform.position, s);
                }

            }
        }
        foreach (Street street in city.streets)
        {
            Gizmos.color = Color.black;
            if (street.checkpoints != null)
            {
                int prev = 0;
                for (int current = 1; current < street.checkpoints.Count; ++current)
                {
                    Gizmos.DrawLine(city.transform.TransformPoint(street.checkpoints[prev]) + Vector3.up * 50, city.transform.TransformPoint(street.checkpoints[current]));
                    prev = current;
                }
            }
        }

    }
    private bool AvailableForMergeBlock(int x, int y)
    {
        if (x == 0 || y == 0 || x == city.size.x - 1 || y == city.size.z - 1) return false;
        else if (city.blocks[x, y].building != null || city.blocks[x + 1, y].building != null || city.blocks[x, y + 1].building != null || city.blocks[x + 1, y + 1].building != null) return false;
        else if (city.blocks[x, y].isAvenue || city.blocks[x + 1, y].isAvenue || city.blocks[x, y + 1].isAvenue || city.blocks[x + 1, y + 1].isAvenue) return false;
        else return true;
    }
    private void SimpleBuildingGenerate(Block block)
    {
        Building building = buildingactory.GetBuilding(Random.Range(0.9f, 1.5f) * block.richness);
        building.transform.parent = block.transform;
        building.LocalPosition = Vector3.zero;
        building.transform.localScale = Vector3.one;
        building.transform.localEulerAngles = new Vector3(0, -90 * Random.Range(-1, 2), 0);
        building.gameObject.SetActive(true);
        building.GeneratePaths();
        block.building = building;
    }
    private void TryDestroyCell(Vector3 c)
    {
        if (city.ValidCell(c))
        {
            Block block = city.blocks[(int)c.x, (int)c.z];
            Destroy(block.building.transform.gameObject);
            block.isAvenue = true;
        }
    }
    private void MarkAsAvenue(Vector3 c)
    {
        if (city.ValidCell(c))
            city.blocks[(int)c.x, (int)c.z].isAvenue = true;
    }
}
