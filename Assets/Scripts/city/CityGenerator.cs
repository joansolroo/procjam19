using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour
{
    public City city;

    public Building buildingTemplate;
    // Start is called before the first frame update
    void Start()
    {
        Generate();
    }

    void Generate()
    {
        city.blocks = new Block[(int)city.size.x, (int)city.size.z];


        // create the basic tiles: blocks
        for (int x = 0; x < city.size.x; ++x)
        {
            for (int z = 0; z < city.size.x; ++z)
            {
                // SPACE SORTING: BLOCK
                GameObject b = new GameObject();
                Block block = b.AddComponent<Block>();
                city.blocks[x, z] = block;
                block.transform.parent = city.transform;
                block.transform.localScale = Vector3.one;
                block.LocalPosition = new Vector3(x, 0, z) - city.size / 2;
                block.region = city.regions[0];
            }
        }

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

       
        for (int x = 0; x < city.size.x; ++x)
        {
            for (int z = 0; z < city.size.x; ++z)
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
                Building building = Instantiate<Building>(buildingTemplate);
                building.transform.parent = block.transform;
                building.LocalPosition = Vector3.zero;
                float d = Random.Range(0.7f, 1f);
                building.Resize(new Vector3(d, Random.Range(0.9f, 1.5f) * block.richness, d));

                block.building = building;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(city.transform.position, city.size);
        if (city.regions != null)
        {
            foreach (Region region in city.regions)
            {
                Gizmos.color = region.color;
                Gizmos.DrawWireCube(region.transform.position, region.size);
                Gizmos.DrawSphere(region.transform.position, 10);
            }
        }
        if (city.blocks != null)
        {
            foreach (Block block in city.blocks)
            {
                if (block.region != null)
                {
                    Gizmos.color = block.region.color;
                    Gizmos.DrawCube(block.transform.position, new Vector3(0.8f, 0.1f, 0.8f));
                }
                /*else
                {
                    Gizmos.color =Color.white;
                    Gizmos.DrawCube(block.transform.position, new Vector3(0.8f, 0.1f, 0.8f));
                }*/

            }
        }
    }
}
