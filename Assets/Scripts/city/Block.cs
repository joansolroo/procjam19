using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : TerrainElement
{
    public Region region;
    public Building building;
    public bool hasPersons = false;
    public float richness;
    public bool isAvenue = false;

    public Block superBlock;
}
