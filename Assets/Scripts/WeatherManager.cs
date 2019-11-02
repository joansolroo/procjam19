using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] Camera mainCamera;

    [Header("Fog")]
    [SerializeField] bool useFog;
    [SerializeField] Color fogColor;
    [SerializeField] [Range(0,0.1f)]float fogDensity;
    [SerializeField] Renderer floorFog;
    [Header("Rain")]
    [SerializeField] float rainAmount;
    [SerializeField] RainCameraController rainOnCamera;
    [SerializeField] DigitalRuby.RainMaker.RainScript rain;
    [SerializeField] bool rainExposed = true;

    Vector3 cameraPrevPosition;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        fogDensity = RenderSettings.fogDensity;
        cameraPrevPosition = mainCamera.transform.position;
    }
    // Update is called once per frame
    void Update()
    {
        RenderSettings.fog = useFog;
        RenderSettings.fogColor = fogColor;
        fogDensity = Mathf.Max(0.05f, (50 - mainCamera.transform.position.y) / 50f)*0.05f;
        RenderSettings.fogDensity = fogDensity;
        mainCamera.backgroundColor = fogColor;
        floorFog.material.SetColor("_TintColor", fogColor);
        //this is not the best to get the angle
        Vector3 camPosition = mainCamera.transform.position;
        Vector3 cameraVelocity = cameraPrevPosition - camPosition;
        cameraPrevPosition = mainCamera.transform.position;
        if (rainExposed)
        {
            if(!rainOnCamera.IsPlaying)
            { rainOnCamera.Play(); }

            rainOnCamera.GlobalWind = cameraVelocity;
            rain.RainIntensity = rainAmount;
        }
        else
        {
            if (rainOnCamera.IsPlaying)
            { rainOnCamera.Stop(); }

            rain.RainIntensity = rainAmount * 0.9f;
        }
        rainOnCamera.Alpha = rainAmount;
    }
}
