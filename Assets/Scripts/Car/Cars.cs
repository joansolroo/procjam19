using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cars : Particle
{

    public GameObject[] carFrontTemplate;
    public GameObject[] carMidTemplate;
    public GameObject[] carBackTemplate;
    // Start is called before the first frame update
    void Start()
    {
        GenerateCars();
    }

    public void GenerateCars()
    {
        // Pick a random front
        int frontIndex = Random.Range(0, carFrontTemplate.Length);
        GameObject front = Instantiate(carFrontTemplate[frontIndex]);
        // Pick a random mid
        int midIndex = Random.Range(0, carMidTemplate.Length);
        GameObject mid = Instantiate(carMidTemplate[midIndex]);
        // Pick a random front
        int backIndex = Random.Range(0, carBackTemplate.Length);
        GameObject back = Instantiate(carBackTemplate[backIndex]);
    }

    protected override void DoCreate() { }
    protected override void DoTick() { }
    protected override void DoDestroy() { }
}
