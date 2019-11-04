using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Graph
{
    public int nodeCount = 5;
    public int entry = 0;
    public int exit = -1;

    public ScriptableObject[] nodeData;

    [System.Serializable]
    public struct Links
    {
        public float[] probabilities;
    }

    public Links[] transitions;

    public Graph(int size)
    {
        nodeCount = size;
        transitions = new Links[size];
        for(int l = 0; l < size;++l)
        {
            transitions[l].probabilities = new float[size];
        }
        nodeData = new ScriptableObject[size];
    }
}