using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Particle : GameElement
{
    [Header("Particle properties")]
    public ParticlePool pool;
    public bool useLifetime = true;
    public float lifeTime;
    protected float time;
    bool destroyed = false;
    public bool Alive
    {
        get { return !destroyed; }
    }
    #region Events
    public delegate void ParticleEvent();
    public ParticleEvent OnCreate;
    public ParticleEvent OnTick;
    public ParticleEvent OnDestroy;
    #endregion

    public float NormalizedTime
    {
        get {
            if (lifeTime > 0)
                return (time / lifeTime);
            else
                return 1; }
    }

    public bool Destroyed
    {
        get => destroyed;
    }

    private void Start()
    {
        ResetParticle();
    }
    public void ResetParticle()
    {
        DoCreate();
        this.time = lifeTime;
        OnCreate?.Invoke();
        destroyed = false;
    }
    public void UpdateParticle()
    {
        if(useLifetime && lifeTime > 0)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                Destroy();
            }
        }
        if(!destroyed)
        {
            DoTick();
            OnTick?.Invoke();
        }
    }
    public void Destroy()
    {
        destroyed = true;
        DoDestroy();
        OnDestroy?.Invoke();
        pool.Release(this);
    }
    protected abstract void DoCreate();
    protected abstract void DoTick();
    protected abstract void DoDestroy();
}