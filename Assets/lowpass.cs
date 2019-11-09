using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lowpass : MonoBehaviour
{
    [SerializeField] Transform car;
    AudioLowPassFilter filter;

    public float freq = 20000;
    // Start is called before the first frame update
    void Start()
    {
        filter = GameObject.Find("Test_song").GetComponent<AudioLowPassFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        freq = car.localPosition.y;
        filter.cutoffFrequency = freq;
    }
}
