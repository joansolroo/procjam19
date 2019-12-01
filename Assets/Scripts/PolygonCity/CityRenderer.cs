using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityRenderer : MonoBehaviour
{
    [SerializeField] VoronoiCity city;

    [SerializeField] ProceduralBuilding buildingTemplate;
    [SerializeField] ProceduralRegion regionTemplate;

    // Start is called before the first frame update
    void Start()
    {
        city.Generate();
        float start = Time.realtimeSinceStartup;
        GenerateRegions(city.regions, 1);
        /*List<GraphLinked.Cell> deflatedCells = new List<GraphLinked.Cell>();
        foreach(var cell in city.regions)
        {
            deflatedCells.Add(cell.Deflated(0.1f));
        }
        GenerateRegions(deflatedCells, 2);*/
        GenerateRegions(city.blockContours, 1200, true);
        // GenerateBuildings(buildingContours);
        float end = Time.realtimeSinceStartup;
        Debug.Log("> geometry time:" + (end - start) + "ms");
    }
    void GenerateRegions(List<GraphLinked.Cell> cells, int height, bool perlin = false)
    {
        foreach (GraphLinked.Cell cell in cells)
        {
            ProceduralRegion region = Instantiate(regionTemplate);
            float h = height;
            if (perlin)
            {
                h *= Mathf.Pow(Mathf.PerlinNoise(cell.Center.x * 1, cell.Center.z * 1) * 0.9f + Mathf.PerlinNoise(cell.Center.x * 10, cell.Center.z * 10) * 0.1f, 2);
            }
            region.transform.parent = this.transform;
            region.transform.position = cell.Center;
            region.Generate(cell, h, perlin ? Random.Range(0, 0.45f) : 0);
            //region.renderer.material.color = Random.ColorHSV(0, 1, 1, 1, 0.5f, 0.5f);
        }
    }
    void GenerateBuildings(List<GraphLinked.Cell> cells)
    {
        foreach (GraphLinked.Cell cell in cells)
        {
            float area = cell.Area;
            //if (area < 4000  && area >500)
            {
                ProceduralBuilding building = Instantiate(buildingTemplate);
                building.transform.parent = this.transform;
                building.transform.position = cell.Center;
                int height = (int)(area / 1000 * 5);

                building.Generate(cell.localContour.ToArray(), height, new Vector2(Random.Range(1, 2), Random.Range(1, 2)));
            }
        }
    }
}
