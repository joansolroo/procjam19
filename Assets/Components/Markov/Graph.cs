using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Graph2
{
   //// public abstract bool HasNode(int node);
   //// public abstract bool HasLink(int from, int to);
   //// public abstract float GetLink(int from, int to);
}
[System.Serializable]
public class GraphSparse<T> : Graph2
{
    [System.Serializable]
    public class Node
    {
        public int id;
        public List<Link> links;
        public T data;
    }
    [System.Serializable]
    public class Link
    {
        public int from;
        public int to;
        public float probability;
    }

    public List<Node> nodes;
}

[System.Serializable]
public class GraphDense : Graph2
{
    public int entry = 0;
    public int exit = -1;
    public int nodeCount = 5;

    public ScriptableObject[] nodeData;

    [System.Serializable]
    public struct Links
    {
        public float[] probabilities;
    }

    public Links[] transitions;

    public GraphDense(int size)
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