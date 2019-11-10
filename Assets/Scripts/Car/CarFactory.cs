using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarFactory : ParticleFactory
{
    [Header("Generation templates")]
    public int modelCount = 20;
    public GameObject carBase;
    public GameObject[] frontTemplates;
    public GameObject[] middleTemplates;
    public GameObject[] backTemplates;
    public Material[] materialTemplates;
    public Material glass;
    
    [Header("Particle attributes")]
    public ParticlePool carPool;
    public City carCity;
    public TrafficController carTrafic;

    [Header("Generated artefacts")]
    public GameObject[] generatedCars;

    public void Generate()
    {
        Transform CarsContainer = new GameObject().transform;
        CarsContainer.parent = transform;
        CarsContainer.gameObject.name = "CarsContainer";
        CarsContainer.localPosition = Vector3.zero;
        CarsContainer.localScale = Vector3.one;
        CarsContainer.localRotation = Quaternion.identity;

        generatedCars = new GameObject[modelCount];
        for (int i=0; i< modelCount; i++)
        {
            Material[] material = new Material[4];
            material[0] = materialTemplates[Random.Range(0, materialTemplates.Length)];
            material[1] = materialTemplates[Random.Range(0, materialTemplates.Length)];
            material[2] = materialTemplates[Random.Range(0, materialTemplates.Length)];
            material[3] = glass;

            generatedCars[i] = Instantiate(carBase);
            generatedCars[i].name = "car_" + i.ToString();
            generatedCars[i].transform.parent = CarsContainer;
            generatedCars[i].transform.localScale = Vector3.one;
            generatedCars[i].transform.localRotation = Quaternion.identity;
            generatedCars[i].SetActive(false);

            CarAI p = generatedCars[i].GetComponent<CarAI>();
            p.pool = carPool;
            p.city = carCity;
            p.traffic = carTrafic;

            Transform model = generatedCars[i].GetComponent<Car>().model;
            
            GameObject front = Instantiate(frontTemplates[Random.Range(0, frontTemplates.Length)]);
            front.name = "front";
            front.transform.parent = model;
            front.transform.localPosition = Vector3.zero;
            front.transform.localScale = Vector3.one;
            Material[] m = { material[0], material[1], material[2] };
            front.GetComponent<MeshRenderer>().materials = m;

            GameObject middle = Instantiate(middleTemplates[Random.Range(0, middleTemplates.Length)]);
            middle.name = "middle";
            middle.transform.parent = model;
            middle.transform.localPosition = Vector3.zero;
            middle.transform.localScale = Vector3.one;
            middle.GetComponent<MeshRenderer>().materials = material;

            GameObject back = Instantiate(backTemplates[Random.Range(0, backTemplates.Length)]);
            back.name = "back";
            back.transform.parent = model;
            back.transform.localPosition = Vector3.zero;
            back.transform.localScale = Vector3.one;
            back.GetComponent<MeshRenderer>().materials = material;
        }
    }

    public override Particle GetParticle()
    {
        return Instantiate(generatedCars[Random.Range(0, generatedCars.Length)]).GetComponent<Particle>();
    }
}
