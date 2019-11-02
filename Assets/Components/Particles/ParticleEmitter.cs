using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEmitter : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] int count=0;
    GameObject[] particleGOs;
    Material[] particleMats;
    Vector3[] particlePos;
    float[] particleTTL;

    int currentCount = 0;
    int current = -1;
    float t;
    Vector3 prevPosition;
    Vector3 initScale;
    [SerializeField] Gradient color;
    [SerializeField] AnimationCurve size;
    [SerializeField] bool local = false;
    [SerializeField] float duration;
    [SerializeField] float rateTime = 1;
    [SerializeField] float rateDistance = 1;
    [SerializeField] bool usePhysics = false;
    [SerializeField] Vector3 velocity;
    [SerializeField] Vector3 velocityNoise;
    [SerializeField] [Range(0.0f, 0.9f)] public float durationDeviation;

    [SerializeField] AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        prevPosition = this.transform.position;
        t = Random.Range(0, 1.0f);

        if (local)
        {
            initScale = prefab.transform.localScale;
        }
        else
        {
            Vector3 scale = prefab.transform.lossyScale;
            initScale = new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z);
        }
        particleGOs = new GameObject[count];
        particleTTL = new float[count];
        if (prefab.GetComponent<Renderer>() != null)
            particleMats = new Material[count];
        particlePos = new Vector3[count];
        for (int p = 0; p < count; ++p)
        {
            GameObject particle = Instantiate(prefab);
            particleGOs[p] = particle;
            particle.SetActive(false);
            if (local)
            {
                particle.transform.parent = this.transform;
            }
            if (particleMats != null)
            {
                particleMats[p] = particle.GetComponent<Renderer>().material;
            }
            particleTTL[p] = duration;
        }
    }

    float movement = 0;
    // Update is called once per frame
    void LateUpdate()
    {
        float dt = Time.deltaTime;

        int previousIdx = (int)t;
        t += dt * rateTime;
        int currentIdx = (int)t;

        if (previousIdx < currentIdx)
        {
            Emit();
        }

        movement += Vector3.Distance(prevPosition, this.transform.position) * rateDistance;
        for (int mp = (int)(movement - 1); mp >= 0; --mp)
        {
            Emit();
            --movement;
        }
        prevPosition = this.transform.position;



        for (int p = 0; p < currentCount; ++p)
        {
            GameObject particle = particleGOs[p];
            if (particle.activeSelf)
            {
                float normalizedTTL = particleTTL[p] / duration;
                if (!usePhysics)
                {
                    Vector3 noise = new Vector3(Random.Range(-velocityNoise.x, velocityNoise.x), Random.Range(-velocityNoise.y, velocityNoise.y), Random.Range(-velocityNoise.z, velocityNoise.z));

                    if (local)
                    {
                        particle.transform.localPosition += (velocity + noise) * Time.deltaTime;
                    }
                    else
                    {
                        particlePos[p] += (velocity + noise) * Time.deltaTime;
                        particle.transform.position = particlePos[p];
                        particle.transform.rotation = Quaternion.identity;

                    }
                    particle.transform.localScale = initScale * size.Evaluate(1 - normalizedTTL);
                }
                particleTTL[p] -= dt;

                if (particleMats != null)
                    particleMats[p].color = color.Evaluate(1 - normalizedTTL);
                if (particleTTL[p] <= 0)
                {
                    particle.SetActive(false);
                }


            }

        }
    }

    public void Emit()
    {
        current = (current + 1) % count;
        GameObject particle = particleGOs[current];

        particlePos[current] = local ? Vector3.zero : this.transform.position;
        particle.SetActive(true);
        particleTTL[current] = duration * Random.Range(1 - durationDeviation, 1 + durationDeviation);

        particle.transform.position = this.transform.position;


        if (usePhysics)
        {
            Rigidbody rb = particle.GetComponent<Rigidbody>();
            rb.velocity = this.transform.TransformDirection(velocity);
        }
        if (audioSource)
        {
            audioSource.pitch = Random.Range(0.4f, 0.5f);
            audioSource.PlayOneShot(audioSource.clip);
        }

        if (currentCount < count)
        {
            ++currentCount;
        }
    }
}
