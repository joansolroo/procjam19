using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="SoundClip", menuName ="Audio/soundclip", order = 2)]
public class SoundClip : ScriptableObject
{
    [SerializeField] public AudioClip clip;
    [SerializeField] [Range(0f,2f)] public float pitchMin;
    [SerializeField] [Range(0f, 2f)] public float pitchMax;

    [SerializeField] [Range(0f, 1f)] public float volume;
    
}
