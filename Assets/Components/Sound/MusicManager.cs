using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{

    [SerializeField] AudioClip[] audioClips;
    [SerializeField] AudioSource[] audioSources;
    [SerializeField] int sources = 2;
    public float volume = 0.25f;
    // Start is called before the first frame update
    void Start()
    {
        audioSources = new AudioSource[sources];
        for(int s = 0; s < sources; ++s)
        {
            audioSources[s] = this.gameObject.AddComponent<AudioSource>();
        }
        StartCoroutine(PlaySound());
    }

    IEnumerator PlaySound()
    {
        int currentSource = 0;
        int currentClip = 0;
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        while (true)
        {
            
            AudioSource current = audioSources[currentSource];
            current.clip = audioClips[currentClip];
            current.volume = volume;
            current.Play();
            while (current.time < current.clip.length-0.25f)
            {
                yield return wait;
            }
            currentSource = (currentSource + 1) % audioSources.Length;
            currentClip = (currentClip + 1) % audioClips.Length;
        }
    }
}
