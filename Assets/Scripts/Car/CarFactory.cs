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

            GameObject front = Instantiate(frontTemplates[Random.Range(0, frontTemplates.Length)]);
            front.name = "front";
            front.transform.parent = generatedCars[i].transform;
            front.transform.localPosition = Vector3.zero;
            front.transform.localScale = Vector3.one;
            front.transform.localRotation = Quaternion.identity;

            GameObject middle = Instantiate(middleTemplates[Random.Range(0, middleTemplates.Length)]);
            middle.name = "middle";
            middle.transform.parent = generatedCars[i].transform;
            middle.transform.localPosition = Vector3.zero;
            middle.transform.localScale = Vector3.one;
            middle.transform.localRotation = Quaternion.identity;

            GameObject back = Instantiate(backTemplates[Random.Range(0, backTemplates.Length)]);
            back.name = "back";
            back.transform.parent = generatedCars[i].transform;
            back.transform.localPosition = Vector3.zero;
            back.transform.localScale = Vector3.one;
            back.transform.localRotation = Quaternion.identity;
        }
    }

    public override Particle GetParticle()
    {
        return Instantiate(generatedCars[Random.Range(0, generatedCars.Length)]).GetComponent<Particle>();
    }
}
