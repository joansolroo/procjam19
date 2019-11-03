using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : TerrainElement
{
    public List<Region> regions;
    public Block[,] blocks;
    public List<Street> streets;

    public float blockVisibilityRadius = 10;
    public float blockVisibilityOffset = 30;
    public float personVisibilityRadius = 100;

    void Update()
    {
        for (int i = 0; i < blocks.GetLength(0); i++)
            for (int j = 0; j < blocks.GetLength(1); j++)
            {
                Block b = blocks[i, j];

                // visibility streaming sphere test
                Vector3 p = Camera.main.transform.InverseTransformPoint(b.transform.position);
                bool visible = p.z > -blockVisibilityOffset && p.sqrMagnitude < blockVisibilityRadius * blockVisibilityRadius;// || Camera.main.transform.InverseTransformPoint(this.transform.position).z < -20;
                
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
                    bool person = p.sqrMagnitude < personVisibilityRadius * personVisibilityRadius;
                    if(person && !b.hasPersons)
                    {
                        b.building.GeneratePersons(20);
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
