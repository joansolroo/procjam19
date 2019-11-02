using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField] ParticleSystem explosionSystem;
    [SerializeField] ParticleSystem sparkSystem;
    [SerializeField] ParticleSystem hitSystem;
    [SerializeField] ParticleSystem electricitySystem;
    [SerializeField] ParticleSystem smokeSystem;
    [SerializeField] ParticleSystem nukeSystem;
    public static EffectManager main;

    private void Awake()
    {
        main = this;
    }
    public void Hit(Vector3 position)
    {
        StartCoroutine(Emit(hitSystem, position, 3));
    }
    IEnumerator Emit(ParticleSystem system, Vector3 position, int amount)
    {
        float t = 0;
        ParticleSystem.EmitParams par = new ParticleSystem.EmitParams();
        par.position = position;
        int emitted = 0;
        while (emitted < amount)
        {
            system.Emit(par, 1);
            ++emitted;
            yield return new WaitForEndOfFrame();
            t += Time.deltaTime;

        }
    }
    public void Explode(Vector3 position)
    {
        StartCoroutine(Emit(nukeSystem, position, 1));
        StartCoroutine(Emit(explosionSystem, position, 3));
        StartCoroutine(Emit(smokeSystem, position, 10));
    }
    public void Nuke(Vector3 position)
    {
        StartCoroutine(Emit(nukeSystem, position+Vector3.up, 3));
        StartCoroutine(Emit(smokeSystem, position, 10));
    }
    public void Decal(Vector3 pos, Vector3 normal)
    {

    }
}
