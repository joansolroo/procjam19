using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skybox : MonoBehaviour
{
    [SerializeField] Cloud cloudTemplate;
    [SerializeField] List<Cloud> clouds = new List<Cloud>();
    [SerializeField] List<float> cloudOpacity = new List<float>();
    [SerializeField] [Range(1, 10)] int density = 1;
    [SerializeField] [Range(1, 10)] int multiplier = 1;
    [SerializeField] Vector3 volume = new Vector3(1000, 1, 1000);
     public float cloudSize = 100;
    public float maxAlpha = 0.25f;
    public int width = 10;
    public int height = 10;
    // Start is called before the first frame update
    void Start()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int z = 0; z < height; ++z)
            {
                for (int c = 0; c < multiplier; ++c)
                {
                    Cloud cloud = Instantiate<Cloud>(cloudTemplate);
                    cloud.transform.parent = this.transform;
                    float h = Random.Range(0, 1f);
                    cloud.transform.localPosition = new Vector3(x*volume.x, 0, z*volume.z) - volume/2
                        + new Vector3(Random.Range(-1f, 1f), Mathf.Lerp(-2,2,h), Random.Range(-1f, 1f)) * 0.25f;

                    cloud.transform.localScale = new Vector3(Random.Range(0.5f, 2f), Random.Range(0.1f, 1f), 0.5f)* cloudSize;
                    cloud.renderer.color = Color.Lerp(Color.black,Color.white,h);// Random.ColorHSV(0.5f, 0.6f, 0, 0.5f, 0.4f, 0.8f, 0.25f, 0.5f);
                    cloud.color = cloud.renderer.color;
                    cloudOpacity.Add(cloud.renderer.color.a);
                    clouds.Add(cloud);

                    cloud.targetOpacity = cloud.maxOpacity;

                }
            }
        }
       // camera = Camera.main;
    }
    [SerializeField] [Range(0, 1f)] float cloudiness;
    float time = 0;
    [SerializeField] float cloudSpeed = 0.1f;
    public float dayLight = 1;
    new public Camera camera;
    private void Update()
    {
        if (camera.transform.position.y > this.transform.position.y-100)
        {
            camera.clearFlags = CameraClearFlags.Skybox;
        }
        else
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
        }
        time += Time.deltaTime * cloudSpeed;
       // dayLight = Mathf.Clamp(dayLight + Time.deltaTime * (WorldTime.IsDay() ? 1 : -1), 0, 1);

        for (int c = 0; c < clouds.Count; ++c)
        {

            Cloud cloud = clouds[c];
            Vector2 seed = new Vector2(cloud.transform.position.x, time + cloud.transform.position.z) / 100;
            seed.x += (time + cloud.transform.position.y) * 0.1f;
            seed.y += (time + cloud.transform.position.y) * 0.1f;
            float h = Mathf.PerlinNoise(seed.x, seed.y);
            h = 1;// Mathf.Lerp(-5, 10, h);
            cloud.transform.position += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 0.25f * Time.deltaTime;
            float alpha;

            if (cloud.currentOpacity == cloud.targetOpacity)
            {
                if (cloud.currentOpacity < 0) cloud.targetOpacity = cloud.maxOpacity;
            }
            else 
            {
                cloud.currentOpacity = Mathf.MoveTowards(cloud.currentOpacity, cloud.targetOpacity, Time.deltaTime* (cloud.targetOpacity<0?10:0.1f));
            }

            
            alpha = (cloudOpacity[c] * cloudiness * h * cloud.currentOpacity);
            float distance = ((Vector3.Distance(cloud.transform.position, Camera.main.transform.position)-10) / 1000);
            alpha = Mathf.Lerp(0, alpha, distance);
            alpha =  Mathf.Clamp(alpha, 0, maxAlpha);

            if (alpha > 0)
            {
                if (!cloud.gameObject.activeSelf)
                {
                    cloud.gameObject.SetActive(true);
                }

                Color color = cloud.color * dayLight;

                color.a = alpha;

                //cloud.color = color;
                cloud.renderer.color = color;
            }
            else
            {
                if (cloud.gameObject.activeSelf)
                {
                    cloud.gameObject.SetActive(false);
                }
            }
           // cloud.targetOpacity = Mathf.MoveTowards(cloud.targetOpacity, cloud., Time.deltaTime / 10);
        }
    }
}
