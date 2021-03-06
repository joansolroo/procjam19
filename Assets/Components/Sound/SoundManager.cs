﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Audio;
public class SoundManager : MonoBehaviour
{
    [SerializeField] Clock metronome;
    [SerializeField] AudioMixer mixer;
    [SerializeField] AudioListener listener;

    //[SerializeField] Song song;
    [SerializeField] AudioSource[] channels;

    [SerializeField] AudioClip clip;
    // Start is called before the first frame update
    void Start()
    {
        metronome.OnTick += Minor;
        //StartCoroutine(Loop());
    }
    /*
    public void Mayor()
    {
       // channels[0].PlayOneShot(clip);
    }*/
    public void Minor()
    {
        if (metronome.globalBeat == 0 || metronome.beat % 16 == 0)
        {
            foreach (AudioSource source in channels)
            {
                if(Random.value > 0.5f)
                {
                    source.Play();
                }
            }    
        }
        foreach (AudioSource source in channels)
        {
            if (Random.value < 0.1f)
            {
                source.volume = Mathf.Clamp01(source.volume+=Random.Range(-0.2f,0.2f));
            }
        }
    }

    public float time;
    private void Update()
    {
        time = channels[0].time;
    }

    IEnumerator Loop()
    {
        while (true)
        {
            channels[0].PlayOneShot(clip);
            yield return new WaitForSecondsRealtime(10);
        }
    }
}
