using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
// Sadly, templated types cannot have custom drawers, so, a hack is needed
#if UNITY_EDITOR
using UnityEditor;
// IngredientDrawer
[CustomPropertyDrawer(typeof(Graph<SongChannel>))]
public class CustomGraphSongChannelDrawer : CustomGraphDrawer { }
#endif*/

[CreateAssetMenu(fileName = "Song", menuName = "Audio/Song Part", order = 2)]
public class SongPart : ScriptableObject
{
    public SongChannel[] channels;
    public int duration = 4;
    /*public Graph transitions;

    void OnValidate()
    {
        transitions.nodeCount = channels.Length;
        if (transitions.nodeData.Length != channels.Length)
        {
            transitions.nodeData = new SongChannel[channels.Length];
        }
        channels.CopyTo(transitions.nodeData, 0);
    }*/
}