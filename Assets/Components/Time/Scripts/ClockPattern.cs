using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockPattern : MonoBehaviour
{
    [Header("Clock")]
    [SerializeField] Clock clock;
    [SerializeField] bool[] pattern;

    [Header("Status")]
    [SerializeField] int current = -1;

    [Header("Sound")]
    [SerializeField] AudioClip audioClip;
    [SerializeField] AudioSource audioSource;
    private void OnEnable()
    {
        clock.OnMinorTick += DoMinor;
        clock.OnMayorTick += DoMayor;
    }

    void DoMinor()
    {
        Tick();
    }
    void DoMayor()
    {
        Tick();
    }

    void Tick()
    {
        current = (current + 1) % pattern.Length;
        if (pattern[current])
        {
            audioSource.PlayOneShot(audioClip);
        }
    }
}
