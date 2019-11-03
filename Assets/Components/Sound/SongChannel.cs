using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "song channel", menuName = "Audio/Song Channel", order = 2)]
public class SongChannel : ScriptableObject
{
    [SerializeField] AudioClip[] clips;
    [SerializeField] float bpm = 96;
    [SerializeField] string mixerChannel = "New Group";
}
