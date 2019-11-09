using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : TerrainElement
{
    public List<Region> regions;
    public GameObject floor;
    public GameObject streetTexture;
    public Block[,] blocks;
    public List<Group> groups;
    public List<Street> streets;

    public float blockVisibilityRadius = 10;
    public float blockVisibilityOffset = 30;
    public int pedestrianDensity = 30;
    public float personVisibilityRadius = 100;
    public float lateralVisibilityRadius = 300;
    public float windowVisibilityRadius = 500;
    public float roofVisibilityRadius = 700;

    public GraphSparse<Vector3> carRoads;
    public GraphSparse<Vector3>.Node[,,] carNodes;
    void Update()
    {
        bool person, lateral, roof;
        foreach (Group group in groups)
        {
            Vector3 p = Camera.main.transform.InverseTransformPoint(group.transform.position);
            float offset = Mathf.Sqrt(group.size.x * group.size.x + group.size.z * group.size.z);
            bool visible = p.z > -offset && p.sqrMagnitude < blockVisibilityRadius * blockVisibilityRadius;

            if(visible)
            {
                group.gameObject.SetActive(true);

                foreach(Block b in group.blocks)
                {
                    float distance = p.magnitude;
                    person = distance < personVisibilityRadius;

                    // pepole
                    if (person && !b.hasPersons)
                    {
                        b.hasPersons = true;
                        b.building.GeneratePersons((b.building.sharedBuilding ? 2 * pedestrianDensity : pedestrianDensity));
                    }
                    else if (!person && b.hasPersons)
                    {
                        b.hasPersons = false;
                        foreach (GameObject pepole in b.building.persons)
                            pepole.SetActive(false);
                        b.building.persons.Clear();
                    }

                    
                    if(b.building)
                    {
                        // windows
                        if(b.building.lodwindow)
                        {
                            if(distance < windowVisibilityRadius)
                            {
                                b.building.lodwindow.enabled = true;
                            }
                            else
                            {
                                b.building.lodwindow.enabled = false;
                            }
                        }


                        // laterals
                        if(distance < lateralVisibilityRadius)
                        {
                            foreach(LODProxy proxy in b.building.lodlateral)
                                proxy.SetState(true);
                        }
                        else
                        {

                            foreach (LODProxy proxy in b.building.lodlateral)
                                proxy.SetState(false);
                        }

                        // roof
                        if (distance < roofVisibilityRadius)
                        {
                            foreach (LODProxy proxy in b.building.lodroof)
                                proxy.SetState(true);
                        }
                        else
                        {

                            foreach (LODProxy proxy in b.building.lodroof)
                                proxy.SetState(false);
                        }
                    }

                }
            }
            else
            {
                group.gameObject.SetActive(false);
            }
        }
    }
    void Update2()
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
                        b.hasPersons = true;
                        b.building.GeneratePersons((b.building.sharedBuilding ? 2* pedestrianDensity : pedestrianDensity));
                    }
                    else if (!person && b.hasPersons)
                    {
                        b.hasPersons = false;
                        foreach (GameObject pepole in b.building.persons)
                            pepole.SetActive(false);
                        b.building.persons.Clear();
                    }
                }
            }
    }

    private void OnDrawGizmos2()
    {
        if (carRoads != null)
        {
            
            GraphSparse<Vector3> roads = carRoads;
            foreach (GraphSparse<Vector3>.Node node1 in roads.nodes)
            {
                int size = 3;
                Gizmos.color = Color.white;
                Gizmos.DrawCube(node1.data, Vector3.one*3*2);
                foreach (GraphSparse<Vector3>.Link link in node1.links)
                {
                    GraphSparse<Vector3>.Node node2 = roads.nodes[link.to];
                    Gizmos.color = Color.gray;
                    Gizmos.DrawLine(node1.data, node2.data);

                    Vector3 direction = node2.data - node1.data;
                    Vector3 offset = Vector3.zero;
                    Color color = Color.black;
                    if (direction.x > 0)
                    {
                        offset = new Vector3(0,1,1);
                        color = Color.green;
                    }
                    else if (direction.x < 0)
                    {
                        offset = new Vector3(0, 1, -1);
                        color = Color.green;
                    }
                    else if (direction.z > 0)
                    {
                        offset = new Vector3(1, -1, 0);
                        color = Color.red;
                    }
                    else if (direction.z < 0)
                    {
                        offset = new Vector3(-1, -1, 0);
                        color = Color.red;
                    }
                    else if (direction.y > 0)
                    {
                        offset = new Vector3(1,0,1)*2;
                        color = Color.blue;
                    }
                    else if (direction.y < 0)
                    {
                        offset = new Vector3(-1, 0, -1)*2;
                        color = Color.blue;
                    }
                    else
                    {
                        Debug.LogWarning("bad link:"+node1.data+",d: "+direction);
                    }
                    Gizmos.color = color;
                    Gizmos.DrawLine(node1.data+offset* size, node2.data+offset* size);
                }
            }
        }
    }
}
