using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParticlePool : GameElement
{
    public static Dictionary<string, ParticlePool> pools = new Dictionary<string, ParticlePool>();
    public ParticleFactory factory;
    public int maxInstance = 5;

    private List<Particle> pool = new List<Particle>();
    private List<Particle> available = new List<Particle>();
    public int lastInstanceIndex;

    private void OnEnable()
    {
        pools[this.name] = this;
    }
    private void OnDisable()
    {
        pools.Remove(this.name);
    }

    void Start()
    {
        lastInstanceIndex = 0;
    }
    
    public GameObject Take()
    {
        if (available.Count > 0)
        {
            Particle particle = available[0];
            available.RemoveAt(0);
            particle.ResetParticle();
            particle.transform.parent = this.transform;
            return particle.gameObject;
        }
        else if(pool.Count < maxInstance)
        {
            Particle particle = factory.GetParticle();// Instantiate(factory.generatedCars[Random.Range(0, factory.generatedCars.Length)]).GetComponent<Particle>();
            particle.pool = this;
            pool.Add(particle);
            lastInstanceIndex = pool.Count - 1;

            particle.transform.parent = this.transform;
            particle.ResetParticle();
            
            return particle.gameObject;
        }
        else
        {
            Particle particle = pool[lastInstanceIndex];
            lastInstanceIndex = (lastInstanceIndex + 1) % maxInstance;
            particle.ResetParticle();
            return particle.gameObject;
        }
    }
    public T Take<T>()
    {
        return Take().GetComponent<T>();
    }
    public void Release(Particle p)
    {
        available.Add(p);
    }
}
