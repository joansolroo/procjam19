using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "song channel", menuName = "Audio/Song Channel", order = 2)]
public class SongChannel : ScriptableObject
{
    public string name = "name";
    [SerializeField] public AudioClip[] clips;
    [SerializeField] float bpm = 96;
    [SerializeField] string mixerChannel = "New Group";

    public Graph transitions;

    void OnValidate()
    {
        if (transitions == null)
            transitions = new Graph(clips.Length);
        transitions.nodeCount = clips.Length;
        if (transitions.nodeData.Length != clips.Length)
        {
            transitions.nodeData = new SoundClip[clips.Length];
        }
        //clips.CopyTo(transitions.nodeData, 0);
    }
}
