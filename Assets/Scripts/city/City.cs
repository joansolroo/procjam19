using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : TerrainElement
{
    public List<Region> regions;
    public GameObject floor;
    public GameObject streetTexture;
    public Block[,] blocks;
    public List<Street> streets;

    public float blockVisibilityRadius = 10;
    public float blockVisibilityOffset = 30;
    public int pedestrianDensity = 30;
    public float personVisibilityRadius = 100;
    
    void Update()
    {
        for (int i = 0; i < blocks.GetLength(0); i++)
            for (int j = 0; j < blocks.GetLength(1); j++)
            {
                Block b = blocks[i, j];

                // visibility streaming sphere test
                Vector3 p = Camera.main.transform.InverseTransformPoint(b.transform.position);
                float offset = (b.building != null && b.building.sharedBuilding ? 2 * blockVisibilityOffset : blockVisibilityOffset);
                bool visible = p.z > -offset && p.sqrMagnitude < blockVisibilityRadius * blockVisibilityRadius;

                if (!visible && b.visible)
                {
                    b.transform.gameObject.SetActive(false);
                    if (b.building != null)
                    {
                        foreach (GameObject pepole in b.building.persons)
                            pepole.SetActive(false);
                        b.building.persons.Clear();
                    }
                    b.visible = false;
                    b.hasPersons = false;
                }
                else if (visible && !b.visible)
                {
                    b.transform.gameObject.SetActive(true);
                    b.visible = true;
                }

                // person instancing streaming sphere
                if(visible && b.building)
                {
                    float radius = (b.building.sharedBuilding ? 1.5f * personVisibilityRadius : personVisibilityRadius);
                    bool person = p.sqrMagnitude < radius * radius;
                    if(person && !b.hasPersons)
                    {
                        b.building.GeneratePersons((b.building.sharedBuilding ? 2* pedestrianDensity : pedestrianDensity));
                        b.hasPersons = true;
                    }
                    else if (!person && b.hasPersons)
                    {
                        foreach (GameObject pepole in b.building.persons)
                            pepole.SetActive(false);
                        b.building.persons.Clear();
                        b.hasPersons = false;
                    }
                }
            }
    }
}
