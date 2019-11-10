using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lowpass : MonoBehaviour
{
    [SerializeField] Transform car;
    AudioLowPassFilter filter;
    

    public float freq = 20000;
    public float min_freq_cut = 1000; // minimum low cut freq
    public float freq_mul = 170;
    // Start is called before the first frame update
    void Start()
    {
        filter = gameObject.GetComponent<AudioLowPassFilter>();
    }

    // Update is called once per frame
    void Update()
    {
        //freq = car.localPosition.y * freq;
        filter.cutoffFrequency = min_freq_cut + car.localPosition.y * freq_mul;
    }
}
