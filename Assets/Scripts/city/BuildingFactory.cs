using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingFactory : MonoBehaviour
{
    [Header("Generatin attributes")]
    public float step = 0.1f;
    public float startHeight = 0.3f;
    public float stopHeight = 1.3f;
    public Building buildingTemplate;
    public int itemsPerStep = 2;

    public int generatedBuildingCount = 0;
    public Dictionary<float, List<GameObject>> buildingList = new Dictionary<float, List<GameObject>>();
    
    void Start()
    {
        generatedBuildingCount = 0;
        for (int h = (int)(startHeight / step); h<(int)(stopHeight / step); h ++)
        {
            for(int i=0; i<itemsPerStep; i++)
            {
                Building building = Instantiate<Building>(buildingTemplate);
                building.transform.parent = transform;
                building.gameObject.name = h.ToString() + '_' + i.ToString();
                building.LocalPosition = Vector3.zero;
                float d = Random.Range(0.7f, 0.8f);
                building.Resize(new Vector3(d, h* step, d));
                building.Init((int)(2 * building.size.y) + 1);

                generatedBuildingCount++;
            }
        }
    }
    
    GameObject GetBuilding(float height)
    {
        float index = (int)(Mathf.Clamp(height, startHeight, stopHeight) / step);
        return Instantiate(buildingList[index][Random.Range(0, buildingList[index].Count)]);
    }
}
