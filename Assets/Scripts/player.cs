using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{
    public Transform model;

    private CharacterController controller;
    public float speed = 10;
    public float rotationSpeed = 6;

    public Transform floorTest;
    public float gravity = 1f;
    public float targetHeight;
    private float currentHeight;

    public LayerMask floorMask;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    Vector3 currentDirection;
    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(floorTest.position, floorTest.TransformDirection(Vector3.down), out hit, Mathf.Infinity, floorMask))
        {
            Debug.DrawRay(floorTest.position, floorTest.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
            Debug.Log("Did Hit");
            currentHeight = hit.distance;
        }

        float dy = 0.1f*(targetHeight - currentHeight);
        if (Mathf.Abs(dy) > 1) dy = Mathf.Sign(dy);

        Vector3 verticalDirection = new Vector3(0, dy, 0) * gravity;
        Vector3 horizontalDirection = transform.forward * Input.GetAxis("Vertical");
        Vector3 direction = verticalDirection + horizontalDirection;
        controller.Move(speed  * direction * Time.deltaTime);
        this.transform.Rotate(Vector3.up, rotationSpeed * Input.GetAxis("Horizontal"));

        if (horizontalDirection.sqrMagnitude == 0)
        {
            direction= verticalDirection+this.transform.forward;
        }

        currentDirection = Vector3.MoveTowards(currentDirection, direction, Time.deltaTime*5);
        model.LookAt(model.position + currentDirection);
    }
}
