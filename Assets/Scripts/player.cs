using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{
    public Car car;

    public float targetHeight;

    public float dy;
    public float speed;
    public float steer;
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            targetHeight += Time.deltaTime * 10;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            targetHeight -= Time.deltaTime * 10;
        }
        if (targetHeight < 0) targetHeight = 0;
        dy = (targetHeight - car.CurrentHeight);
        speed = Input.GetAxis("Vertical");
        steer = Input.GetAxis("Horizontal");

        car.Move(speed, steer, dy);
    }
}
