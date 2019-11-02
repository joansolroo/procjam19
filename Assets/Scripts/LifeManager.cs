using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeManager : MonoBehaviour
{
    [SerializeField] ParticlePool carPool;
    [SerializeField] List<CarAI> activeCars;
    [SerializeField] ParticlePool peoplePool;

    private void Start()
    {
        for(int c = 0; c < 30; ++c)
        {
            CarAI car = carPool.Take<CarAI>();
            car.transform.position = player.main.transform.position+ Vector3.right*c*5;
            activeCars.Add(car);
        }
    }

    private void Update()
    {
        
    }
}
