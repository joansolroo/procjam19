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
    public bool generateMegastructure = false;

    public int generatedBuildingCount = 0;
    public Dictionary<int, List<Building>> buildingList = new Dictionary<int, List<Building>>();
    public Dictionary<int, List<Building>> megaBuildingList = new Dictionary<int, List<Building>>();

    public void Generate()
    {
        Transform BuildingContainer = new GameObject().transform;
        BuildingContainer.gameObject.name = "BuildingContainer";
        BuildingContainer.parent = transform;
        BuildingContainer.localPosition = Vector3.zero;
        BuildingContainer.localScale = Vector3.one;
        BuildingContainer.localRotation = Quaternion.identity;

        generatedBuildingCount = 0;
        for (int h = (int)(startHeight / step); h<(int)(stopHeight / step)+2; h++)
        {
            buildingList.Add(h, new List<Building>());
            for (int i=0; i<itemsPerStep; i++)
            {
                Building building = Instantiate<Building>(buildingTemplate);
                building.transform.parent = BuildingContainer;
                building.gameObject.name = h.ToString() + '_' + i.ToString();
                building.LocalPosition = Vector3.zero;
                float d = Random.Range(0.7f, 0.8f);
                building.Resize(new Vector3(d, h* step, d));
                building.sharedBuilding = false;
                building.Init((int)(2 * building.localSize.y) + 1);

                generatedBuildingCount++;
                buildingList[h].Add(building);
            }
        }

        if(generateMegastructure)
        {
            Transform MegaBuildingContainer = new GameObject().transform;
            MegaBuildingContainer.gameObject.name = "MegaBuildingContainer";
            MegaBuildingContainer.parent = transform;
            MegaBuildingContainer.localPosition = Vector3.zero;
            MegaBuildingContainer.localScale = Vector3.one;
            MegaBuildingContainer.localRotation = Quaternion.identity;

            for (int h = (int)(startHeight / step); h < (int)(stopHeight / step)+2; h++)
            {
                megaBuildingList[h] = new List<Building>();
                for (int i = 0; i < itemsPerStep; i++)
                {
                    Building building = Instantiate<Building>(buildingTemplate);
                    building.transform.parent = MegaBuildingContainer;
                    building.gameObject.name = "mega_" + h.ToString() + '_' + i.ToString();
                    building.LocalPosition = Vector3.zero;
                    float d = Random.Range(1.7f, 1.8f);
                    building.Resize(new Vector3(d, h * step, d));
                    building.sharedBuilding = true;
                    building.Init((int)(4 * building.localSize.y) + 1);

                    generatedBuildingCount++;
                    megaBuildingList[h].Add(building);
                }
            }
        }
    }
    
    public Building GetBuilding(float height)
    {
        int index = (int)(Mathf.Clamp(height, startHeight, stopHeight) / step);
        return Instantiate(buildingList[index][Random.Range(0, buildingList[index].Count)]);
    }
    public Building GetMegaBuilding(float height)
    {
        int index = (int)(Mathf.Clamp(height, startHeight, stopHeight) / step);
        return Instantiate(megaBuildingList[index][Random.Range(0, megaBuildingList[index].Count)]);
    }
}
