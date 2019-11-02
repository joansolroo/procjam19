using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Particle : GameElement
{
    [Header("Particle properties")]
    [SerializeField] public ParticlePool pool;
    [SerializeField] public float lifeTime;
    [SerializeField] protected float time;
    bool destroyed = false;
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

    public bool Destroyed { get => destroyed; }

    private void Start()
    {
        ResetParticle();
    }
    public void ResetParticle()
    {
        destroyed = false;
        DoCreate();
        this.time = lifeTime;
        OnCreate?.Invoke();
    }
    protected void Update()
    {
        if(lifeTime > 0)
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