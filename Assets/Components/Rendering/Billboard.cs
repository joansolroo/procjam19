using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] new Camera camera;

    private void Start()
    {
        camera = Camera.main;
    }
    // Update is called once per frame
    void LateUpdate()
    {
        //this.transform.LookAt(Camera.main.transform);
        this.transform.rotation = camera.transform.rotation;
    }
}
