using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Equipment;

public class ParticlePool : GameElement
{
    public static Dictionary<string, ParticlePool> pools = new Dictionary<string, ParticlePool>();
    public Particle prefab;
    public int maxInstance = 5;

    private List<Particle> pool = new List<Particle>();
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
        if(pool.Count < maxInstance)
        {
            Particle particle = Instantiate(prefab);
            particle.pool = this;
            pool.Add(particle);
            lastInstanceIndex = pool.Count - 1;

            particle.ResetParticle();
            particle.transform.parent = this.transform;
            return particle.gameObject;
        }
        else
        {
            GameObject go = pool[lastInstanceIndex].gameObject;
            lastInstanceIndex = (lastInstanceIndex + 1) % maxInstance;
            return go;
        }
    }
    public void Release(Particle p)
    {
        Debug.LogWarning("Particle release: not implemented.");
    }
}
