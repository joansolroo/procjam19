using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Car car;
    public bool invertVertical = false;

    void Update()
    {
        float gas = Input.GetAxis("Pedals"); // Acceleration
        float steer = Input.GetAxis("Horizontal"); // Turn
        float vert = Input.GetAxis("Vertical"); // Up and down
        if (invertVertical) vert *= -1;

        car.Move(gas, steer, vert);

        ClampPosition();
    }

    
    
    void ClampPosition()
    {
        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        pos.x = Mathf.Clamp01(pos.x);
        pos.y = Mathf.Clamp01(pos.y);
        transform.position = Camera.main.ViewportToWorldPoint(pos);
    }

}
