using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud : MonoBehaviour
{
    [SerializeField] public new SpriteRenderer renderer;
    [SerializeField] public float maxOpacity = 1;
    public float targetOpacity;
    public float currentOpacity;
    public Color color;
}
