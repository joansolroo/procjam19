using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeManager : MonoBehaviour
{
    [SerializeField] City city;
    [SerializeField] ParticlePool carPool;
    [SerializeField] List<CarAI> activeCars;
    [SerializeField] ParticlePool peoplePool;

    [SerializeField] int cars = 100;
    private void Start()
    {
        
    }

    bool created = false;
    private void Update()
    {
        if (!created && city.carRoads != null)
        {
            for (int c = 0; c < cars; ++c)
            {
                CarAI car = carPool.Take<CarAI>();
                //car.transform.position = player.main.transform.position+ Vector3.right*c*5;
                activeCars.Add(car);
                car.gameObject.SetActive(true);
            }
            created = true;
        }
    }
}
