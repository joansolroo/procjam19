using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Song", menuName = "Audio/song", order = 2)]
public class Song : ScriptableObject
{
    public SongChannel[] channels;
    
}
