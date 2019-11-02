using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Equipment
{
    public abstract class Particle : GameElement
    {
        [Header("Particle properties")]
        [SerializeField] public ParticlePool pool;
        [SerializeField] public float lifeTime;
        [SerializeField] protected float time;

        #region Events
        public delegate void ParticleEvent();
        public ParticleEvent OnCreate;
        public ParticleEvent OnTick;
        public ParticleEvent OnDestroy;
        #endregion

        public float NormalizedTime
        {
            get { return (time / lifeTime); }
        }

        public void ResetParticle()
        {
            DoCreate();
            this.time = lifeTime;
            OnCreate?.Invoke();
        }
        protected void Update()
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                DoDestroy();
                OnDestroy?.Invoke();
                pool.Release(this);
            }
            else
            {
                DoTick();
                OnTick?.Invoke();
            }
        }

        protected abstract void DoCreate();
        protected abstract void DoTick();
        protected abstract void DoDestroy();
    }
}