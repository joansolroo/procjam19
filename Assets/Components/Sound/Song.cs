using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Song", menuName = "Audio/Song", order = 2)]
public class Song : ScriptableObject
{
    public SongPart[] parts;
    public GraphDense transitions;
    public float BPM = 96;
    void OnValidate()
    {
        if (transitions == null)
            transitions = new GraphDense(parts.Length);
        transitions.nodeCount = parts.Length;
        if (transitions.nodeData.Length != parts.Length)
        {
            transitions.nodeData = new SongPart[parts.Length];
        }
        parts.CopyTo(transitions.nodeData,0);
    }
}
