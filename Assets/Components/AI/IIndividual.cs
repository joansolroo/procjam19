using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IIndividual
{
    GameObject GetGameObject();

    bool Alive();
    void Sense();
    void Act();
}
