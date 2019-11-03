using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : TerrainElement
{
    public List<Region> regions;
    public Block[,] blocks;
    public List<Street> streets;

    public float blockVisibilityRadius = 1000;
    public float personVisibilityRadius = 100;

    void Update()
    {
        foreach (Block b in blocks)
        {
            // visibility streaming sphere test
            bool invisible = (player.main.transform.position - b.transform.position).sqrMagnitude > blockVisibilityRadius * blockVisibilityRadius || Camera.main.transform.InverseTransformPoint(this.transform.position).z < -20;
            if (invisible && b.visible)
            {
                b.transform.gameObject.SetActive(false);
                foreach (GameObject p in b.building.persons)
                    p.SetActive(false);
                b.building.persons.Clear();
                b.visible = false;
            }
            else if(!invisible && !b.visible)
            {
                b.transform.gameObject.SetActive(true);
                b.visible = true;
            }

            // person instancing streaming sphere
            if(!invisible)
            {
                bool person = (player.main.transform.position - b.transform.position).sqrMagnitude < personVisibilityRadius * personVisibilityRadius;
                if(person && !b.hasPersons)
                {
                    b.building.GeneratePersons(20);
                    b.hasPersons = true;
                }
                else if (!person && b.hasPersons)
                {
                    foreach (GameObject p in b.building.persons)
                        p.SetActive(false);
                    b.building.persons.Clear();
                    b.hasPersons = false;
                }
            }
        }
    }
}
