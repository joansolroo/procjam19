using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonFactory : ParticleFactory
{
    [Header("Generation templates")]
    public int modelCount = 5;
    public GameObject[] personTemplates;

    [Header("Particle attributes")]
    public ParticlePool personPool;

    [Header("Generated artefacts")]
    public GameObject[] generatedPerson;

    public void Generate()
    {
        Transform PersonContainer = new GameObject().transform;
        PersonContainer.parent = transform;
        PersonContainer.gameObject.name = "PersonContainer";
        PersonContainer.localPosition = Vector3.zero;
        PersonContainer.localScale = Vector3.one;
        PersonContainer.localRotation = Quaternion.identity;

        generatedPerson = new GameObject[modelCount];
        for (int i = 0; i < modelCount; i++)
        {
            generatedPerson[i] = Instantiate(personTemplates[Random.Range(0, personTemplates.Length)]);
            generatedPerson[i].name = "person_" + i.ToString();
            generatedPerson[i].transform.parent = PersonContainer;
            generatedPerson[i].transform.localScale = Vector3.one;
            generatedPerson[i].transform.localRotation = Quaternion.identity;
            generatedPerson[i].SetActive(false);
            generatedPerson[i].GetComponent<Particle>().pool = personPool;
        }
    }

    public override Particle GetParticle()
    {
        return Instantiate(generatedPerson[Random.Range(0, generatedPerson.Length)]).GetComponent<Particle>();
    }
}
