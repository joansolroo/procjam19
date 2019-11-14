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
    private Dictionary<GameObject,Particle> active = new Dictionary<GameObject, Particle>();
    List<Particle> toRelease = new List<Particle>();
    public int lastInstanceIndex;

    [Header("Stats")]
    [SerializeField] float updateTime = 0;
    [SerializeField] int activeParticles = 0;
    [SerializeField] int badlyActive = 0;
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

    private void Update()
    {
       float start = Time.realtimeSinceStartup;
       int inactive = 0;
       toRelease.Clear();
       foreach(GameObject k in active.Keys)
        {

            Particle p = active[k];
            if (p.gameObject.activeSelf && p.Alive)
            {
                p.UpdateParticle();
            }
            else
            {
                toRelease.Add(p);
                ++inactive;
            }
        }
       foreach(Particle p in toRelease)
        {
            Release(p);
        }
        updateTime = Time.realtimeSinceStartup - start;
        activeParticles = active.Count;
        badlyActive = inactive;
    }
    public GameObject Take()
    {
        Particle particle;
        if (available.Count > 0)
        {
            particle = available[0];
            available.RemoveAt(0);
            particle.transform.parent = this.transform;
        }
        else if(pool.Count < maxInstance)
        {
            particle = factory.GetParticle();// Instantiate(factory.generatedCars[Random.Range(0, factory.generatedCars.Length)]).GetComponent<Particle>();
            particle.pool = this;
            pool.Add(particle);
            lastInstanceIndex = pool.Count - 1;

            particle.transform.parent = this.transform;
        }
        else
        {
            particle = pool[lastInstanceIndex];
            lastInstanceIndex = (lastInstanceIndex + 1) % maxInstance;
        }
        particle.ResetParticle();
        active[particle.gameObject] = particle;
        return particle.gameObject;
    }
    public T Take<T>()
    {
        return Take().GetComponent<T>();
    }
    public void Release(Particle p)
    {
        p.gameObject.SetActive(false);
        available.Add(p);
        active.Remove(p.gameObject);
    }
}
