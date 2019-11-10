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
    [SerializeField] Renderer[] floorFog;
    public float groundDensity = 0.02f;

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
        float h = Mathf.Max(mainCamera.transform.position.y, 0.1f);
        fogDensity = Mathf.Min(0.05f, Mathf.Pow(h, -1.3f));
        RenderSettings.fogDensity = fogDensity;
        mainCamera.backgroundColor = fogColor;
        
        Color ff = fogColor;
        ff.a = Mathf.Max(0.05f, groundDensity * Mathf.Pow(h, 0.6f));
        foreach (Renderer fog in floorFog)
            fog.material.SetColor("_TintColor", ff);

        
        for (int i = 0; i < floorFog.Length; i++)
        {
            floorFog[i].transform.position = new Vector3(0, Mathf.Pow(h * (0.5f*i+0.1f), 0.6f)-0.1f, 0);
        }

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
