using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Car car;
   
    public bool invertVertical = false;
    
    [Header("Camera")]
    public new Camera camera;
    public Vector3 cameraOffset = new Vector3(0, 0.4f, -2);
    public Vector2 pursuitScale = new Vector2(0.5f,0.4f);
    public Vector2 cameraDistance = new Vector2(0, 3);
    void Update()
    {
        float gas = Input.GetAxis("Pedals"); // Acceleration
        float steer = Input.GetAxis("Horizontal"); // Turn
        float vert = Input.GetAxis("Vertical"); // Up and down
        if (invertVertical) vert *= -1;

        car.Move(gas * Mathf.Clamp(gas + transform.position.y / 30,0,5), steer, vert);

        ClampPosition();
        float speed = Mathf.Clamp01(car.direction.magnitude);
        Vector3 targetCameraPosition = cameraOffset + new Vector3(steer * pursuitScale.x * (0.5f + speed * 0.5f), vert * pursuitScale.y * (0.5f + speed * 0.5f), Mathf.Lerp(-cameraDistance.x, -cameraDistance.y, speed / 5));
        camera.transform.localPosition = Vector3.MoveTowards(camera.transform.localPosition, targetCameraPosition,Time.deltaTime*2) ;
    }

    
    
    void ClampPosition()
    {
        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        pos.x = Mathf.Clamp01(pos.x);
        pos.y = Mathf.Clamp01(pos.y);
        transform.position = Camera.main.ViewportToWorldPoint(pos);
    }

}
